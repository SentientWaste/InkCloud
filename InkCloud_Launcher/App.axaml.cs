using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using InkCloud_Launcher.Views;
using Microsoft.Win32;

namespace InkCloud_Launcher
{
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
                desktop.MainWindow = new MainWindow();
            }

			using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
			{
				var isDark = key?.GetValue("AppsUseLightTheme") as int? == 0;
                if (isDark) {Application.Current.RequestedThemeVariant = ThemeVariant.Dark;}
                else {Application.Current.RequestedThemeVariant = ThemeVariant.Light;}
			}

			base.OnFrameworkInitializationCompleted();
        }
    }
}