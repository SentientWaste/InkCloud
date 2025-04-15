using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace InkCloud_Launcher.ViewModel;

public sealed partial class MainWindowViewModel : ObservableObject {
    [RelayCommand]
    private void ChangeTheme() =>
        Application.Current!.RequestedThemeVariant = Application.Current.ActualThemeVariant == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
}