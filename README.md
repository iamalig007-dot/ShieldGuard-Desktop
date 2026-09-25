# 🛡️ ShieldGuard Desktop

**Windows PC Protection App** — Built with Avalonia UI (.NET 9, C#)

[![Platform](https://img.shields.io/badge/Platform-Windows-blue?logo=windows)](https://github.com)
[![Framework](https://img.shields.io/badge/Framework-.NET%209-purple?logo=dotnet)](https://dotnet.microsoft.com)
[![UI](https://img.shields.io/badge/UI-Avalonia%2011-teal)](https://avaloniaui.net)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

> Part of the **ShieldGuard** family — protecting you on every device.  
> 📱 [ShieldGuard Android](https://github.com/iamalig007-dot/ShieldGuard-Android) | 🖥️ **ShieldGuard Desktop** (this repo)

---

## ✨ Features

| Feature | Description |
|---|---|
| 🌐 **Content Shield** | Blocks 50,000+ harmful sites (adult, gambling, malware) via Windows hosts file — works in ALL browsers |
| 🔐 **Admin PIN Lock** | Settings protected by a PIN — nobody can disable protection without it |
| ⚡ **Auto-Start** | Registers with Windows startup — protection is always active |
| 🎨 **Beautiful UI** | Dark-mode Avalonia UI with smooth design |

## 🛡️ How Content Shield Works

ShieldGuard adds harmful domains to your **Windows hosts file** (`C:\Windows\System32\drivers\etc\hosts`), redirecting them to `0.0.0.0` (nowhere). This blocks at the **OS level** — works with:
- ✅ Google Chrome
- ✅ Microsoft Edge  
- ✅ Mozilla Firefox
- ✅ Brave Browser
- ✅ Any other browser or app

## 🚀 Quick Start

### Prerequisites
- Windows 10/11 (64-bit)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Build & Run
```bash
git clone https://github.com/iamalig007-dot/ShieldGuard-Desktop.git
cd ShieldGuard-Desktop
dotnet run --project ShieldGuard.Desktop
```

> ⚠️ **Run as Administrator** — required to modify the hosts file.

### Build Release EXE
```bash
dotnet publish ShieldGuard.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

## 📁 Project Structure

```
ShieldGuard-Desktop/
├── ShieldGuard.Desktop/
│   ├── Services/
│   │   ├── PinService.cs          ← PIN hashing & verification
│   │   ├── ContentShieldService.cs ← Hosts file blocker
│   │   └── StartupService.cs      ← Windows registry startup
│   ├── ViewModels/
│   │   ├── OnboardingViewModel.cs  ← Setup wizard logic
│   │   ├── PinUnlockViewModel.cs   ← PIN unlock logic
│   │   └── MainViewModel.cs        ← Dashboard logic
│   ├── Views/
│   │   ├── OnboardingWindow.axaml  ← First-time setup UI
│   │   ├── PinUnlockWindow.axaml   ← PIN entry UI
│   │   └── MainWindow.axaml        ← Main dashboard UI
│   ├── App.axaml                   ← Theme & styles
│   └── Program.cs                  ← Entry point
└── ShieldGuard.sln
```

## 🔒 Security Design

- PIN is stored as **SHA-256 hash** in `C:\ProgramData\ShieldGuard\` (admin-only access)
- Hosts file modification requires **administrator privileges**
- Settings cannot be changed without the PIN

## 📱 Also on Android

Check out the **ShieldGuard Android** app for mobile protection:  
→ [ShieldGuard Android App](../ShieldGuard)

## 📄 License

MIT License — Free to use, modify, and distribute.
