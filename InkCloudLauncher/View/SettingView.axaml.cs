using System;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using InkCloudLauncher.ViewModel;

namespace InkCloudLauncher.View;

public partial class SettingView : UserControl
{
    private MainWindowViewModel? _vm;
    private readonly DispatcherTimer _scrollTimer;
    private DateTime _scrollStartedAt;
    private double _scrollStart;
    private double _scrollTarget;

    public SettingView()
    {
        InitializeComponent();

        SettingContentHost.Transitions = new Transitions
        {
            new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(240), Easing = new CubicEaseOut() },
            new ThicknessTransition { Property = MarginProperty, Duration = TimeSpan.FromMilliseconds(340), Easing = new ExponentialEaseOut() }
        };

        _scrollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _scrollTimer.Tick += OnScrollTick;

        DataContextChanged += (_, _) =>
        {
            if (_vm != null)
            {
                _vm.PropertyChanged -= OnViewModelChanged;
            }

            _vm = null;

            if (DataContext is MainWindowViewModel vm)
            {
                _vm = vm;
                _vm.PropertyChanged += OnViewModelChanged;
            }
        };
    }

    private void OnViewModelChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedSettingSection))
        {
            AnimateScrollTo(0);
        }
    }

    private void OnSettingScrollWheel(object? sender, PointerWheelEventArgs e)
    {
        var maxOffset = Math.Max(0, SettingScrollViewer.Extent.Height - SettingScrollViewer.Viewport.Height);
        if (maxOffset <= 0)
        {
            return;
        }

        _scrollStart = SettingScrollViewer.Offset.Y;
        AnimateScrollTo(Math.Clamp(_scrollTarget - e.Delta.Y * 92, 0, maxOffset));

        e.Handled = true;
    }

    private void OnSettingNavPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string section && DataContext is MainWindowViewModel vm)
        {
            vm.PressSettingSection(section);
        }
    }

    private void OnSettingNavCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            Dispatcher.UIThread.Post(vm.ReleaseSettingSection, DispatcherPriority.Background);
        }
    }

    private void AnimateScrollTo(double target)
    {
        _scrollStart = SettingScrollViewer.Offset.Y;
        _scrollTarget = target;
        _scrollStartedAt = DateTime.UtcNow;

        if (!_scrollTimer.IsEnabled)
        {
            _scrollTimer.Start();
        }
    }

    private void OnScrollTick(object? sender, EventArgs e)
    {
        var elapsed = (DateTime.UtcNow - _scrollStartedAt).TotalMilliseconds;
        var progress = Math.Clamp(elapsed / 280, 0, 1);
        var eased = 1 - Math.Pow(1 - progress, 4);
        var y = _scrollStart + (_scrollTarget - _scrollStart) * eased;

        SettingScrollViewer.Offset = new Vector(SettingScrollViewer.Offset.X, y);

        if (progress >= 1)
        {
            _scrollTimer.Stop();
        }
    }
}
