using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using InkCloud_Launcher.Services;
using InkCloud_Launcher.Views;
using Microsoft.Win32;

namespace InkCloud_Launcher;

public sealed partial class App : Application {
    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void RegisterServices() {
        base.RegisterServices();
        SettingService.InitService();
    }

    public override void OnFrameworkInitializationCompleted() {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.Exit += OnExit;
            desktop.Startup += OnStartup;

            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e) {
        SettingService.Current?.Save();
    }

    private void OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e) {
        SettingService.Current?.Init();
    }
}