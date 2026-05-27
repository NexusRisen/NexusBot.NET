using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SysBot.Pokemon;

public static class EncryptionUtil
{
    // Primary key for AES-256 data encryption (Trade Codes, Medals)
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("DudeBot_Secure_Key_2026_!@#$%^&*"); 
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("DB_IV_1234567890"); 

    // Secondary obfuscation key for Connection Strings (Internal use)
    private const byte ObfKey = 0xAA;

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using MemoryStream ms = new();
        using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
        {
            using (StreamWriter sw = new(cs))
            {
                sw.Write(plainText);
            }
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;
        try
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream ms = new(Convert.FromBase64String(cipherText));
            using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new(cs);
            return sr.ReadToEnd();
        }
        catch
        {
            return cipherText;
        }
    }

    /// <summary>
    /// Multi-pass obfuscation for sensitive system strings (IPs, Credentials).
    /// Prevents simple "strings" inspection of the binary.
    /// </summary>
    public static string SysObfuscate(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        // Pass 1: XOR with static key
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = (byte)(bytes[i] ^ ObfKey);
        
        // Pass 2: Reverse
        Array.Reverse(bytes);
        
        // Pass 3: XOR with index-based salt
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = (byte)(bytes[i] ^ (i % 255));

        return Convert.ToBase64String(bytes);
    }

    public static string SysDeobfuscate(string obfuscated)
    {
        try
        {
            var bytes = Convert.FromBase64String(obfuscated);
            
            // Reverse Pass 3
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)(bytes[i] ^ (i % 255));
            
            // Reverse Pass 2
            Array.Reverse(bytes);
            
            // Reverse Pass 1
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)(bytes[i] ^ ObfKey);
                
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }
}
