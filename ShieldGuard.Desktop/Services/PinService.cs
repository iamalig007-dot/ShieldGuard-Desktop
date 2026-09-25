using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ShieldGuard.Desktop.Services;

public class PinService
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "ShieldGuard");
    private static readonly string PinFile = Path.Combine(SettingsDir, "pin.dat");

    public bool HasPin() => File.Exists(PinFile);

    public void SetPin(string pin)
    {
        Directory.CreateDirectory(SettingsDir);
        var hash = HashPin(pin);
        File.WriteAllText(PinFile, hash);
    }

    public bool VerifyPin(string pin)
    {
        if (!HasPin()) return false;
        var stored = File.ReadAllText(PinFile).Trim();
        return stored == HashPin(pin);
    }

    public void ClearPin()
    {
        if (File.Exists(PinFile))
            File.Delete(PinFile);
    }

    private static string HashPin(string pin)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes("ShieldGuardSalt_" + pin));
        return Convert.ToHexString(bytes);
    }
}
