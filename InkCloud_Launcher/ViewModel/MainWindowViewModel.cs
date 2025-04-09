using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkCloud_Launcher.ViewModel
{
	public partial class MainWindowViewModel : ObservableObject
	{
		[RelayCommand]
		private void ChangeTheme()
		{
			if (Application.Current.RequestedThemeVariant == ThemeVariant.Dark)
			{
				Application.Current.RequestedThemeVariant = ThemeVariant.Light;
			}
			else
			{
				Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
			}
		}
	}
}
