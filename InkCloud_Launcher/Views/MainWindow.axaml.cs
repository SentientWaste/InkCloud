using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
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
        new DefaultPageTransitions(TimeSpan.FromSeconds(1));

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
        Dispatcher.UIThread.Post(async () => {
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
        }, DispatcherPriority.Render);
    }

    private void Button_Click(object? sender, RoutedEventArgs e) {
        Close();
    }

    private void Button_Click_1(object? sender, RoutedEventArgs e) {
        WindowState = WindowState.Minimized;
    }
}

enum ControlType {
    Control1,
    Control2
}