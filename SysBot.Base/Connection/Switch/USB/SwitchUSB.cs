using LibUsbDotNet;
using LibUsbDotNet.Main;
using LibUsbDotNet.LibUsb;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SysBot.Base;

/// <summary>
/// Abstract class representing the communication over USB.
/// </summary>
public abstract class SwitchUSB : IConsoleConnection, IDisposable
{
    private static readonly UsbContext _context = new();

    private readonly object _sync = new();

    private readonly int Port;

    private UsbEndpointReader? reader;

    private IUsbDevice? SwDevice;

    private UsbEndpointWriter? writer;

    protected SwitchUSB(int port)
    {
        Port = port;
        Name = Label = $"USB-{port}";
    }

    public void Dispose()
    {
        Disconnect();
        GC.SuppressFinalize(this);
    }

    public int BaseDelay { get; set; } = 1;

    public bool Connected { get; protected set; }

    public int DelayFactor { get; set; } = 1000;

    public string Label { get; set; }

    public int MaximumTransferSize { get; set; } = 0x1C0;

    public string Name { get; }

    public void Connect()
    {
        SwDevice = TryFindUSB();
        if (SwDevice == null)
            throw new Exception("USB device not found.");

        lock (_sync)
        {
            if (SwDevice.IsOpen)
                SwDevice.Close();
            SwDevice.Open();

            SwDevice.SetConfiguration(1);
            bool resagain = SwDevice.ClaimInterface(0);
            if (!resagain)
            {
                SwDevice.ReleaseInterface(0);
                SwDevice.ClaimInterface(0);
            }

            reader = SwDevice.OpenEndpointReader(ReadEndpointID.Ep01);
            writer = SwDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
        }
    }

    public void Disconnect()
    {
        lock (_sync)
        {
            if (SwDevice is { IsOpen: true } x)
            {
                x.ReleaseInterface(0);
                x.Close();
            }

            reader = null;
            writer = null;
        }
    }

    public void Log(string message) => LogInfo(message);

    public void LogError(string message) => LogUtil.LogError(Label, message);

    public void LogInfo(string message) => LogUtil.LogInfo(Label, message);

    public int Read(byte[] buffer)
    {
        lock (_sync)
            return ReadInternal(buffer);
    }

    public void Reset()
    {
        Disconnect();
        Connect();
    }

    public int Send(byte[] buffer)
    {
        lock (_sync)
            return SendInternal(buffer);
    }

    protected byte[] PixelPeekUSB()
    {
        Thread.Sleep(1);
        lock (_sync)
        {
            if (reader == null)
                throw new Exception("USB device not found or not connected.");

            // Use local buffer for size header
            Span<byte> sizeHeader = stackalloc byte[4];
            var ec = reader.Read(sizeHeader, 5000, out _);
            if (ec != Error.Success)
                throw new Exception(ec.ToString());

            int size = BitConverter.ToInt32(sizeHeader);
            byte[] buffer = new byte[size];
            int transfSize = 0;
            while (transfSize < size)
            {
                Thread.Sleep(1);
                ec = reader.Read(buffer.AsSpan(transfSize, Math.Min(4096, size - transfSize)), 5000, out int lenVal);
                if (ec != Error.Success)
                {
                    LogError($"Error while getting screenshot: {ec}");
                    Disconnect();
                    break;
                }
                transfSize += lenVal;
            }
            return buffer;
        }
    }

    protected byte[] Read(ICommandBuilder b, ulong offset, int length)
    {
        var cmd = b.Peek(offset, length, false);
        SendInternal(cmd);
        return ReadBulkUSB();
    }

    protected byte[] ReadBulkUSB()
    {
        // Give it time to push back.
        Thread.Sleep(1);

        lock (_sync)
        {
            try
            {
                if (reader == null)
                    throw new Exception("USB device not found or not connected.");

                // Let usb-botbase tell us the response size.
                Span<byte> sizeHeader = stackalloc byte[4];
                var ec = reader.Read(sizeHeader, 5000, out int ret);
                if (ec != Error.Success && ret == 0)
                    throw new Exception(ec.ToString());

                int size = BitConverter.ToInt32(sizeHeader);
                byte[] buffer = new byte[size];

                // Loop until we have read everything.
                int transfSize = 0;
                while (transfSize < size)
                {
                    Thread.Sleep(1);
                    ec = reader.Read(buffer.AsSpan(transfSize, Math.Min(4096, size - transfSize)), 5000, out int lenVal);
                    if (ec != Error.Success)
                        throw new Exception(ec.ToString());

                    transfSize += lenVal;
                }
                return buffer;
            }
            catch (Exception ex)
            {
                // LibUsbDotNet 3.0 uses Error.Success instead of ErrorCode.None
                Log($"{nameof(ReadBulkUSB)} failed: {ex.Message}");
                return [0];
            }
        }
    }

    protected byte[] ReadMulti(ICommandBuilder b, IReadOnlyDictionary<ulong, int> offsetSizes)
    {
        var cmd = b.PeekMulti(offsetSizes, false);
        SendInternal(cmd);
        return ReadBulkUSB();
    }

    protected void Write(ICommandBuilder b, ReadOnlySpan<byte> data, ulong offset)
    {
        if (data.Length > MaximumTransferSize)
            WriteLarge(b, data, offset);
        else
            WriteSmall(b, data, offset);
    }

    public void WriteSmall(ICommandBuilder b, ReadOnlySpan<byte> data, ulong offset)
    {
        lock (_sync)
        {
            var cmd = b.Poke(offset, data, false);
            SendInternal(cmd);
            Thread.Sleep(1);
        }
    }

    private int ReadInternal(byte[] buffer)
    {
        try
        {
            Span<byte> sizeOfReturn = stackalloc byte[4];
            if (reader == null)
                throw new Exception("USB device not found or not connected.");

            var ec = reader.Read(sizeOfReturn, 5000, out int ret);
            if (ec != Error.Success && ret == 0)
                throw new Exception(ec.ToString());

            ec = reader.Read(buffer.AsSpan(), 5000, out var lenVal);
            if (ec != Error.Success)
                throw new Exception(ec.ToString());

            return lenVal;
        }
        catch (Exception ex)
        {
            Log($"{nameof(ReadInternal)} failed: {ex.Message}");
            return 0;
        }
    }

    private int SendInternal(byte[] buffer)
    {
        try
        {
            if (writer == null)
                throw new Exception("USB device not found or not connected.");

            uint pack = (uint)buffer.Length + 2;
            var ec = writer.Write(BitConverter.GetBytes(pack).AsSpan(), 2000, out int ret);
            if (ec != Error.Success && ret == 0)
                throw new Exception(ec.ToString());

            ec = writer.Write(buffer.AsSpan(), 2000, out var l);
            if (ec != Error.Success)
                throw new Exception(ec.ToString());

            return l;
        }
        catch (Exception ex)
        {
            Log($"{nameof(SendInternal)} failed: {ex.Message}");
            return 0;
        }
    }

    private IUsbDevice? TryFindUSB()
    {
        lock (_context)
        {
            return _context.Find(device =>
                device.VendorId == 0x057E &&
                device.ProductId == 0x3000 &&
                (device as UsbDevice)?.Address.ToString() == Port.ToString());
        }
    }

    private void WriteLarge(ICommandBuilder b, ReadOnlySpan<byte> data, ulong offset)
    {
        while (data.Length != 0)
        {
            var length = Math.Min(data.Length, MaximumTransferSize);
            var slice = data[..length];
            WriteSmall(b, slice, offset);

            data = data[length..];
            offset += (uint)length;
            Thread.Sleep((MaximumTransferSize / DelayFactor) + BaseDelay);
        }
    }
}
