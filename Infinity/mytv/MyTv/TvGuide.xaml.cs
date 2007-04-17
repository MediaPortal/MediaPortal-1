using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dialogs;
using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
using ProjectInfinity.Controls;
using ProjectInfinity.Plugins;
namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvGuide.xaml
  /// </summary>

  public partial class TvGuide : System.Windows.Controls.Page, IMenuCommand, IDisposable
  {
    #region variables
    private delegate void StartTimeShiftingDelegate(Channel channel);
    private delegate void EndTimeShiftingDelegate(TvResult result, VirtualCard card);
    private delegate void MediaPlayerErrorDelegate();
    private delegate void ConnectToServerDelegate();
    #endregion


    #region variables
    TvGuideViewModel _model;
    #endregion

    #region IMenuCommand Members

    public void Run()
    {
      ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvGuide.xaml", UriKind.Relative));
    }

    #region IDisposable Members
    public void Dispose()
    {
    }
    #endregion

    #endregion
    #region ctor
    public TvGuide()
    {
      InitializeComponent();
    }
    #endregion

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      gridMain.Children.Clear();
      using (FileStream steam = new FileStream(@"skin\default\mytv\TvGuide.xaml", FileMode.Open, FileAccess.Read))
      {
        UIElement documentRoot = (UIElement)XamlReader.Load(steam);
        gridMain.Children.Add(documentRoot);
      }
      if (!ServiceScope.IsRegistered<TvGuideViewModel>())
      {
        _model = new TvGuideViewModel(this);
        ServiceScope.Add<TvGuideViewModel>(_model);
      }
      _model = ServiceScope.Get<TvGuideViewModel>();
      _model.Page = this;
      _model.Reload();
      gridMain.DataContext = _model;
      this.InputBindings.Add(new KeyBinding(_model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));

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