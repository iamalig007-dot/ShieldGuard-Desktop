using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldGuard.Desktop.Services;
using ShieldGuard.Desktop.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System.Collections.Generic;
using System.Linq;

namespace ShieldGuard.Desktop.ViewModels;

public partial class PinUnlockViewModel : ObservableObject
{
    private readonly PinService _pinService;

    [ObservableProperty] private string _pin = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _hasError = false;
    [ObservableProperty] private int _attempts = 0;

    public PinUnlockViewModel(PinService pinService)
    {
        _pinService = pinService;
    }

    [RelayCommand]
    private void Unlock()
    {
        HasError = false;
        ErrorMessage = "";

        if (_pinService.VerifyPin(Pin))
        {
            // Open main dashboard
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(
                        new PinService(),
                        new ContentShieldService(),
                        new StartupService())
                };
                desktop.MainWindow = mainWindow;
                mainWindow.Show();
                foreach (var window in desktop.Windows.ToList())
                {
                    if (window is PinUnlockWindow)
                        window.Close();
                }
            }
        }
        else
        {
            Attempts++;
            Pin = "";
            ErrorMessage = $"Incorrect PIN. Attempt {Attempts}";
            HasError = true;
        }
    }

    [RelayCommand]
    private void AppendDigit(string digit)
    {
        if (Pin.Length < 8)
            Pin += digit;
    }

    [RelayCommand]
    private void DeleteDigit()
    {
        if (Pin.Length > 0)
            Pin = Pin[..^1];
    }
}
