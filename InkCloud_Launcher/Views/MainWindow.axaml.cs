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
        await host.NavigationAsync(new TestPage());
    }

    private async void Home_Click(object? sender, RoutedEventArgs e) {
        await host.NavigationAsync(new HomePage());
    }

    private void TitleBar_PointerPressed(object sender, PointerPressedEventArgs e) {
        BeginMoveDrag(e);
    }

    private async void MainWindowLoaded(object? sender, RoutedEventArgs e) {
        await host.NavigationAsync(new HomePage());
    }

    private async Task RunAsync(object page, CancellationToken cancellationToken = default) {

    }

    private void Button_Click(object? sender, RoutedEventArgs e) {
        Close();
    }

    private void Button_Click_1(object? sender, RoutedEventArgs e) {
        WindowState = WindowState.Minimized;
    }
}