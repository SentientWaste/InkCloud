using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Threading;
using InkCloudLauncher.Service;
using InkCloudLauncher.ViewModel;

namespace InkCloudLauncher.View;

public partial class DownloadView : UserControl
{
    private MainWindowViewModel? _vm;

    public DownloadView()
    {
        InitializeComponent();

        DownloadContentHost.Transitions = new Transitions
        {
            new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(240), Easing = new CubicEaseOut() },
            new ThicknessTransition { Property = MarginProperty, Duration = TimeSpan.FromMilliseconds(340), Easing = new ExponentialEaseOut() }
        };

        DataContextChanged += (_, _) =>
        {
            if (_vm != null)
            {
                _vm.PropertyChanged -= OnVmChanged;
            }

            _vm = DataContext as MainWindowViewModel;

            if (_vm != null)
            {
                _vm.PropertyChanged += OnVmChanged;
            }
        };
    }

    private void OnVmChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedDownloadSection))
        {
            Dispatcher.UIThread.Post(() => DownloadScrollViewer.Offset = new Vector(0, 0), DispatcherPriority.Background);
        }
    }

    private void OnGameVersionSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (GameVersionList.SelectedItem is not MinecraftVersionItem version || DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (vm.OpenGameVersionConfigCommand.CanExecute(version.Id))
        {
            vm.OpenGameVersionConfigCommand.Execute(version.Id);
        }

        GameVersionList.SelectedItem = null;
    }

}
