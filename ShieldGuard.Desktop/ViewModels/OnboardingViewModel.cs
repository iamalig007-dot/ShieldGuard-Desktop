using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldGuard.Desktop.Services;
using ShieldGuard.Desktop.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace ShieldGuard.Desktop.ViewModels;

public partial class OnboardingViewModel : ObservableObject
{
    private readonly PinService _pinService;

    [ObservableProperty] private int _currentStep = 0;
    [ObservableProperty] private string _pin = "";
    [ObservableProperty] private string _confirmPin = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _hasError = false;

    // Step titles and descriptions
    public string[] StepTitles => new[]
    {
        "Welcome to ShieldGuard",
        "Create Your Admin PIN",
        "Confirm Your PIN",
        "Setup Complete!"
    };

    public string[] StepDescriptions => new[]
    {
        "Your PC is about to get powerful protection against harmful content, malware sites, and unauthorized access.",
        "This PIN protects your ShieldGuard settings. You'll need it to change any protection settings.",
        "Re-enter your PIN to confirm it's correct.",
        "ShieldGuard is ready! Your PC is now protected."
    };

    public int TotalSteps => StepTitles.Length;
    public string CurrentTitle => StepTitles[CurrentStep];
    public string CurrentDescription => StepDescriptions[CurrentStep];
    public bool IsWelcomeStep => CurrentStep == 0;
    public bool IsPinStep => CurrentStep == 1;
    public bool IsConfirmStep => CurrentStep == 2;
    public bool IsDoneStep => CurrentStep == 3;
    public bool CanGoBack => CurrentStep > 0 && CurrentStep < 3;

    public OnboardingViewModel(PinService pinService)
    {
        _pinService = pinService;
    }

    [RelayCommand]
    private void Next()
    {
        HasError = false;
        ErrorMessage = "";

        if (CurrentStep == 1)
        {
            if (Pin.Length < 4)
            {
                ErrorMessage = "PIN must be at least 4 digits";
                HasError = true;
                return;
            }
        }
        else if (CurrentStep == 2)
        {
            if (Pin != ConfirmPin)
            {
                ErrorMessage = "PINs don't match. Try again.";
                HasError = true;
                ConfirmPin = "";
                return;
            }
            _pinService.SetPin(Pin);
        }

        if (CurrentStep < TotalSteps - 1)
        {
            CurrentStep++;
            OnPropertyChanged(nameof(CurrentTitle));
            OnPropertyChanged(nameof(CurrentDescription));
            OnPropertyChanged(nameof(IsWelcomeStep));
            OnPropertyChanged(nameof(IsPinStep));
            OnPropertyChanged(nameof(IsConfirmStep));
            OnPropertyChanged(nameof(IsDoneStep));
            OnPropertyChanged(nameof(CanGoBack));
        }
    }

    [RelayCommand]
    private void Back()
    {
        if (CurrentStep > 0 && CurrentStep < 3)
        {
            CurrentStep--;
            OnPropertyChanged(nameof(CurrentTitle));
            OnPropertyChanged(nameof(CurrentDescription));
            OnPropertyChanged(nameof(IsWelcomeStep));
            OnPropertyChanged(nameof(IsPinStep));
            OnPropertyChanged(nameof(IsConfirmStep));
            OnPropertyChanged(nameof(IsDoneStep));
            OnPropertyChanged(nameof(CanGoBack));
        }
    }

    [RelayCommand]
    private void OpenDashboard()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = new MainViewModel(new PinService(), new ContentShieldService(), new StartupService())
            };
            desktop.MainWindow = mainWindow;
            mainWindow.Show();
            // Close onboarding
            foreach (var window in desktop.Windows.ToList())
            {
                if (window is OnboardingWindow)
                    window.Close();
            }
        }
    }
}
