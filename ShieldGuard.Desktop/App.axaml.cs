using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ShieldGuard.Desktop.Services;
using ShieldGuard.Desktop.ViewModels;
using ShieldGuard.Desktop.Views;

namespace ShieldGuard.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var pinService = new PinService();
            
            if (!pinService.HasPin())
            {
                // First launch — show onboarding/setup
                desktop.MainWindow = new OnboardingWindow
                {
                    DataContext = new OnboardingViewModel(pinService)
                };
            }
            else
            {
                // Already set up — show PIN unlock
                desktop.MainWindow = new PinUnlockWindow
                {
                    DataContext = new PinUnlockViewModel(pinService)
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
