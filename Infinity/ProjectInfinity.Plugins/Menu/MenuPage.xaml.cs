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
    }
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      gridMain.Children.Clear();
      using (FileStream steam = new FileStream(@"skin\default\Home\home.xaml", FileMode.Open, FileAccess.Read))
      {
        gridMain.DataContext = new MenuViewModel();
        UIElement documentRoot = (UIElement)XamlReader.Load(steam);
        gridMain.Children.Add(documentRoot);
      }
    }
  }
}