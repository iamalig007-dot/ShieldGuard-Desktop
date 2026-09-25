using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldGuard.Desktop.Services;
using System;
using System.Threading.Tasks;

namespace ShieldGuard.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly PinService _pinService;
    private readonly ContentShieldService _contentShield;
    private readonly StartupService _startup;

    [ObservableProperty] private bool _contentShieldEnabled;
    [ObservableProperty] private bool _startupEnabled;
    [ObservableProperty] private string _statusTitle = "LOADING...";
    [ObservableProperty] private string _statusSubtitle = "Checking protection status";
    [ObservableProperty] private bool _isFullyProtected = false;
    [ObservableProperty] private int _blockedDomainCount = 0;
    [ObservableProperty] private string _shieldProgress = "0 / 2";
    [ObservableProperty] private bool _isWorking = false;
    [ObservableProperty] private string _workingMessage = "";

    // PIN change
    [ObservableProperty] private bool _showPinChange = false;
    [ObservableProperty] private string _currentPin = "";
    [ObservableProperty] private string _newPin = "";
    [ObservableProperty] private string _pinMessage = "";
    [ObservableProperty] private bool _pinMessageIsError = false;

    public MainViewModel(PinService pinService, ContentShieldService contentShield, StartupService startup)
    {
        _pinService = pinService;
        _contentShield = contentShield;
        _startup = startup;
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        ContentShieldEnabled = _contentShield.IsEnabled();
        StartupEnabled = _startup.IsStartupEnabled();
        BlockedDomainCount = _contentShield.GetBlockedDomainCount();

        int shieldsActive = (ContentShieldEnabled ? 1 : 0) + (StartupEnabled ? 1 : 0);
        ShieldProgress = $"{shieldsActive} / 2";
        IsFullyProtected = shieldsActive == 2;

        if (IsFullyProtected)
        {
            StatusTitle = "🟢 FULLY PROTECTED";
            StatusSubtitle = $"All shields active • {BlockedDomainCount:N0} threats blocked";
        }
        else
        {
            int missing = 2 - shieldsActive;
            StatusTitle = $"🟡 NEEDS ATTENTION";
            StatusSubtitle = $"{missing} shield{(missing > 1 ? "s" : "")} inactive — tap below to fix";
        }
    }

    [RelayCommand]
    private async Task ToggleContentShield()
    {
        try
        {
            IsWorking = true;
            if (ContentShieldEnabled)
            {
                WorkingMessage = "Disabling Content Shield...";
                await Task.Run(() => _contentShield.Disable());
            }
            else
            {
                var progress = new Progress<string>(msg => WorkingMessage = msg);
                await Task.Run(() => _contentShield.EnableAsync(progress).GetAwaiter().GetResult());
            }
            RefreshStatus();
        }
        catch (Exception ex)
        {
            WorkingMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsWorking = false;
            WorkingMessage = "";
        }
    }

    [RelayCommand]
    private void ToggleStartup()
    {
        try
        {
            if (StartupEnabled)
                _startup.DisableStartup();
            else
                _startup.EnableStartup();
            RefreshStatus();
        }
        catch (Exception ex)
        {
            WorkingMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ShowChangePinPanel()
    {
        ShowPinChange = !ShowPinChange;
        CurrentPin = "";
        NewPin = "";
        PinMessage = "";
    }

    [RelayCommand]
    private void SaveNewPin()
    {
        if (!_pinService.VerifyPin(CurrentPin))
        {
            PinMessage = "Current PIN is incorrect";
            PinMessageIsError = true;
            return;
        }
        if (NewPin.Length < 4)
        {
            PinMessage = "New PIN must be at least 4 digits";
            PinMessageIsError = true;
            return;
        }
        _pinService.SetPin(NewPin);
        PinMessage = "✅ PIN changed successfully!";
        PinMessageIsError = false;
        CurrentPin = "";
        NewPin = "";
    }
}
