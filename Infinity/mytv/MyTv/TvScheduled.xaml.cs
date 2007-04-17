using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using Dialogs;
using TvControl;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvScheduled.xaml
  /// </summary>

  public partial class TvScheduled : System.Windows.Controls.Page, IMenuCommand, IDisposable
  {
    TvScheduledViewModel _model;
    public TvScheduled()
    {
      InitializeComponent();
    }

    #region IMenuCommand Members

    public void Run()
    {
      ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvScheduled.xaml", UriKind.Relative));
    }

    #region IDisposable Members
    public void Dispose()
    {
    }
    #endregion

    #endregion
    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      gridMain.Children.Clear();
      using (FileStream steam = new FileStream(@"skin\default\mytv\tvscheduled.xaml", FileMode.Open, FileAccess.Read))
      {
        UIElement documentRoot = (UIElement)XamlReader.Load(steam);
        gridMain.Children.Add(documentRoot);
      }
      _model = new TvScheduledViewModel(this);
      gridMain.DataContext = _model;

      //this.InputBindings.Add(new KeyBinding(_model.FullScreenTv, new KeyGesture(System.Windows.Input.Key.X, ModifierKeys.None)));
      this.InputBindings.Add(new KeyBinding(_model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));

      // Sets keyboard focus on the first Button in the sample.
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(OnPreviewKeyDown));
    }

    protected void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.X)
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvFullScreen.xaml", UriKind.Relative));
          return;
        }
      }
    }

  }
}