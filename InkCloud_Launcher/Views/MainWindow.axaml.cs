using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using InkCloud_Launcher.Media.Transitions.Page;
using InkCloud_Launcher.ViewModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InkCloud_Launcher.Views;

public partial class MainWindow : Window {
    private ControlType _controlType;
    private CancellationTokenSource _cancellationTokenSource = new();

    private IPageTransition _pageTransition =
        new DefaultPageTransitions(TimeSpan.FromSeconds(0.75), new CircularEaseInOut());

    public MainWindow() {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        Loaded += MainWindowLoaded;

        home.Click += Home_Click;
        test.Click += Test_Click;
    }

    private async void Test_Click(object? sender, RoutedEventArgs e) {
        using (_cancellationTokenSource) {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new();
        }

        await RunAsync(new TestPage(), _cancellationTokenSource.Token);
    }

    private async void Home_Click(object? sender, RoutedEventArgs e) {
        using (_cancellationTokenSource) {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new();
        }

        await RunAsync(new HomePage(), _cancellationTokenSource.Token);
    }

    private void TitleBar_PointerPressed(object sender, PointerPressedEventArgs e) {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
            this.BeginMoveDrag(e);
        }
    }

    private async void MainWindowLoaded(object? sender, RoutedEventArgs e) {
        await RunAsync(new HomePage());
    }

    private async Task RunAsync(object page, CancellationToken cancellationToken = default) {
        await Dispatcher.UIThread.InvokeAsync(async () => {
            if (_controlType is ControlType.Control1) {
                from.Content = page;
                if (_pageTransition != null) {
                    await _pageTransition.Start(to, from, true, cancellationToken);
                } else {
                    to.IsVisible = false;
                    from.IsVisible = true;
                }

                _controlType = ControlType.Control2;
            } else {
                to.Content = page;
                if (_pageTransition != null) {
                    await _pageTransition.Start(from, to, false, cancellationToken);
                } else {
                    from.IsVisible = false;
                    to.IsVisible = true;
                }

                _controlType = ControlType.Control1;
            }
        }, DispatcherPriority.Background);
    }
}

enum ControlType {
    Control1,
    Control2
}