using System;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using InkCloudLauncher.ViewModel;

namespace InkCloudLauncher;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;

        HomeLayer.Transitions = new Transitions
        {
            new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(240), Easing = new CubicEaseOut() },
            new ThicknessTransition { Property = MarginProperty, Duration = TimeSpan.FromMilliseconds(520), Easing = new ExponentialEaseOut() }
        };

        PlazaLayer.Transitions = new Transitions
        {
            new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(280), Easing = new CubicEaseOut() },
            new ThicknessTransition { Property = MarginProperty, Duration = TimeSpan.FromMilliseconds(600), Easing = new ExponentialEaseOut() }
        };

        PlazaPageHost.Transitions = new Transitions
        {
            new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(220), Easing = new CubicEaseOut() },
            new ThicknessTransition { Property = MarginProperty, Duration = TimeSpan.FromMilliseconds(300), Easing = new ExponentialEaseOut() }
        };

        SettingPageHost.Transitions = new Transitions
        {
            new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(220), Easing = new CubicEaseOut() },
            new ThicknessTransition { Property = MarginProperty, Duration = TimeSpan.FromMilliseconds(300), Easing = new ExponentialEaseOut() }
        };

        DownloadPageHost.Transitions = new Transitions
        {
            new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(220), Easing = new CubicEaseOut() },
            new ThicknessTransition { Property = MarginProperty, Duration = TimeSpan.FromMilliseconds(300), Easing = new ExponentialEaseOut() }
        };

        DragHandle.Transitions = new Transitions
        {
            new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(180), Easing = new CubicEaseOut() },
            new ThicknessTransition { Property = MarginProperty, Duration = TimeSpan.FromMilliseconds(240), Easing = new ExponentialEaseOut() }
        };

        MicaLayer.Transitions = new Transitions
        {
            new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(420), Easing = new CubicEaseOut() }
        };
    }

    private async void OnWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (_viewModel.IsSettingPageVisible || _viewModel.IsDownloadPageVisible)
        {
            return;
        }

        if (e.Delta.Y < 0)
        {
            await _viewModel.OpenNavigationAsync();
        }
        else if (e.Delta.Y > 0)
        {
            await _viewModel.CloseNavigationAsync();
        }
    }

    private void OnHandleEntered(object? sender, PointerEventArgs e)
    {
        _viewModel.UpdateHint(e.GetPosition(this), Bounds.Size);
    }

    private void OnHandleExited(object? sender, PointerEventArgs e)
    {
        _viewModel.HideHintIfIdle();
    }

    private void OnHandlePressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (_viewModel.BeginHandleDrag(e.GetPosition(this), Bounds.Size))
        {
            e.Pointer.Capture(DragHandle);
        }
    }

    private void OnHandleMoved(object? sender, PointerEventArgs e)
    {
        _viewModel.MoveHandle(e.GetPosition(this), Bounds.Size);
    }

    private async void OnHandleReleased(object? sender, PointerReleasedEventArgs e)
    {
        e.Pointer.Capture(null);
        await _viewModel.EndHandleDragAsync(e.GetPosition(this));
    }

    private void DragWindow(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void MinimizeWindow(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseWindow(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
