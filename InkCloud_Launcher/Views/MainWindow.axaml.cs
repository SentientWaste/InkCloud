using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using InkCloud_Launcher.ViewModel;

namespace InkCloud_Launcher.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
			DataContext = new MainWindowViewModel();
		}

		private void TitleBar_PointerPressed(object sender, PointerPressedEventArgs e)
		{
			if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
			{
				this.BeginMoveDrag(e);
			}
		}
	}
}