using System;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Base;

public class BotSource<T>(RoutineExecutor<T> Bot) : IDisposable
    where T : class, IConsoleBotConfig
{
    public readonly RoutineExecutor<T> Bot = Bot;

    private readonly object _lock = new();
    private CancellationTokenSource Source = new();

    public bool IsPaused { get; private set; }

    public bool IsRunning { get; private set; }

    public bool IsStopping { get; private set; }

    public bool IsRestarting { get; private set; }

    private bool _disposed;

    private Task? _stopTask;

    public virtual void Pause()
    {
        lock (_lock)
        {
            if (!IsRunning || IsStopping || IsPaused || IsRestarting)
                return;

            IsPaused = true;
            Task.Run(Bot.SoftStop)
                .ContinueWith(ReportFailure, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
                .ContinueWith(_ =>
                {
                    lock (_lock)
                    {
                        IsPaused = false;
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }

    public virtual void RebootAndStop()
    {
        lock (_lock)
        {
            if (IsRestarting) return;
            Stop();

            Task.Run(() => Bot.RebootAndStopAsync(Source.Token)
                .ContinueWith(ReportFailure, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
                .ContinueWith(_ =>
                {
                    lock (_lock)
                    {
                        IsRunning = false;
                    }
                }));

            IsRunning = true;
        }
    }

    public virtual void Restart()
    {
        lock (_lock)
        {
            if (IsRestarting) return;
            IsRestarting = true;
        }

        bool ok = true;
        Task.Run(Bot.Connection.Reset).ContinueWith(task =>
        {
            ok = false;
            ReportFailure(task);
        }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
        .ContinueWith(_ =>
        {
            lock (_lock)
            {
                IsRestarting = false;
            }
            if (ok)
                Start();
        }, TaskContinuationOptions.RunContinuationsAsynchronously | TaskContinuationOptions.NotOnFaulted);
    }

    public virtual void Resume()
    {
        Start();
    }

    public virtual void Start()
    {
        lock (_lock)
        {
            if (IsPaused)
                Stop(); // can't soft-resume; just re-launch

            if (IsStopping)
            {
                _stopTask?.ContinueWith(_ => Start());
                return;
            }

            if (IsRunning)
                return;

            IsRunning = true;
            var token = Source.Token;
            Task.Run(async () => await Bot.RunAsync(token)
                .ContinueWith(ReportFailure, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
                .ContinueWith(_ =>
                {
                    lock (_lock)
                    {
                        IsRunning = false;
                    }
                }));
        }
    }

    public virtual void Stop()
    {
        lock (_lock)
        {
            if (!IsRunning || IsStopping)
                return;

            IsStopping = true;
            Source.Cancel();
            // Don't dispose yet to avoid ObjectDisposedException in running tasks
            var oldSource = Source;
            Source = new CancellationTokenSource();

            _stopTask = Task.Run(async () => await Bot.HardStop()
                .ContinueWith(ReportFailure, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)
                .ContinueWith(_ =>
                {
                    lock (_lock)
                    {
                        IsPaused = IsRunning = IsStopping = false;
                        _stopTask = null;
                        oldSource.Dispose();
                    }
                }));
        }
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Source.Cancel();
            Source.Dispose();
        }

        _disposed = true;
    }

    private void ReportFailure(Task finishedTask)
    {
        var ident = Bot.Connection.Name;
        var ae = finishedTask.Exception;
        if (ae == null)
        {
            LogUtil.LogError("Bot has stopped without error.", ident);
            return;
        }

        LogUtil.LogError("Bot has crashed!", ident);

        if (!string.IsNullOrEmpty(ae.Message))
            LogUtil.LogError("Aggregate message: " + ae.Message, ident);

        var st = ae.StackTrace;
        if (!string.IsNullOrEmpty(st))
            LogUtil.LogError("Aggregate stacktrace: " + st, ident);

        foreach (var e in ae.InnerExceptions)
        {
            if (!string.IsNullOrEmpty(e.Message))
                LogUtil.LogError("Inner message: " + e.Message, ident);
            LogUtil.LogError("Inner stacktrace: " + e.StackTrace, ident);
        }
    }
}
