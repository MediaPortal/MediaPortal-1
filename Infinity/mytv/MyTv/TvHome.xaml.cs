//
// Left todo:
//  fullscreen tv : -zap osd and other osds
//  tvguide       : -zapping
//                : -conflicts
//
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
using Dialogs;
using TvControl;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using DirectShowLib;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
using ProjectInfinity.Settings;
using ProjectInfinity.TaskBar;
namespace MyTv
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>

  public partial class TvHome : System.Windows.Controls.Page
  {
    #region variables
    TvBaseViewModel _model;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvHome"/> class.
    /// </summary>
    public TvHome()
    {
      this.ShowsNavigationUI = false;
      ServiceScope.Get<IWindowsTaskBar>().Show();

      InitializeComponent();
    }
    #endregion

    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      gridMain.Children.Clear();
      using (FileStream steam = new FileStream(@"skin\default\mytv\tvhome.xaml", FileMode.Open, FileAccess.Read))
      {
        UIElement documentRoot = (UIElement)XamlReader.Load(steam);
        gridMain.Children.Add(documentRoot);
      }
      _model = new TvBaseViewModel();
      gridMain.DataContext = _model;
      this.InputBindings.Add(new KeyBinding(_model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));

      ServiceScope.Get<ILogger>().Info("mytv:OnLoaded");
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
    }
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.X)
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          e.Handled = true;
          ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvFullScreen.xaml", UriKind.Relative));
          return;
        }
      }
    }

  }

}