using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Windows.Shapes;
using System.Diagnostics;
using ProjectInfinity;
using ProjectInfinity.Logging;
namespace ProjectInfinity.Menu.View
{
  /// <summary>
  /// Interaction logic for MenuPage.xaml
  /// </summary>
  public partial class MenuPage
  {
    public MenuPage()
    {
      this.ShowsNavigationUI = false;
      InitializeComponent();
      this.Loaded += new RoutedEventHandler(OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      MenuViewModel viewModel = new MenuViewModel();
      DataContext = viewModel;
      try
      {
        using (FileStream steam = new FileStream(@"skin\default\Home\home.xaml", FileMode.Open, FileAccess.Read))
        {
          UIElement documentRoot = (UIElement)XamlReader.Load(steam);
          Content = documentRoot;
        }
      }
      catch (Exception ex)
      {
        ServiceScope.Get<ILogger>().Error("error loading home.xaml");
        ServiceScope.Get<ILogger>().Error(ex);
      }
      this.InputBindings.Add(new KeyBinding(viewModel.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));

    }
  }
}