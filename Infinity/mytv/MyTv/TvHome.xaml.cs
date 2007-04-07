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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using Dialogs;
using TvControl;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using DirectShowLib;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
namespace MyTv
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>

  public partial class TvHome : System.Windows.Controls.Page
  {
    #region variables
    TvHomeViewModel _model;
    private delegate void ConnectToServerDelegate();
    static bool _firstTime = true;
    TvPlayerCollection _players = TvPlayerCollection.Instance;
    TvChannelNavigator _navigator =  TvChannelNavigator.Instance;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvHome"/> class.
    /// </summary>
    public TvHome()
    {
      this.ShowsNavigationUI = false;
      WindowTaskbar.Show();

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
      _model = new TvHomeViewModel(this);
      gridMain.DataContext = _model;
      this.InputBindings.Add(new KeyBinding(_model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));

      ServiceScope.Get<ILogger>().Info("mytv:OnLoaded");
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      // Sets keyboard focus on the first Button in the sample.
      Keyboard.Focus(buttonTvGuide);
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(handleMouse));
      ConnectToServer();
    }
    void handleMouse(object sender, MouseEventArgs e)
    {
      FrameworkElement element = Mouse.DirectlyOver as FrameworkElement;
      while (element.TemplatedParent != null)
      {
        element = (FrameworkElement)element.TemplatedParent;
        if (element as Button != null)
        {
          Keyboard.Focus((Button)element);
          return;
        }
        if (element as CheckBox != null)
        {
          Keyboard.Focus((CheckBox)element);
          return;
        }
        if (element as RadioButton != null)
        {
          Keyboard.Focus((RadioButton)element);
          return;
        }
      }
    }
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.X)
      {
        if (ServiceScope.Get<ITvPlayerCollection>().Count > 0)
        {
          e.Handled = true;
          this.NavigationService.Navigate(new Uri("/MyTv;component/TvFullScreen.xaml", UriKind.Relative));
          return;
        }
      }
      if (e.Key == System.Windows.Input.Key.Enter)
      {
        if (Keyboard.FocusedElement as CheckBox != null)
        {
          CheckBox box = Keyboard.FocusedElement as CheckBox;
          box.IsChecked = !box.IsChecked;
        }
      }
    }

    /// <summary>
    /// background worker. Connects to server.
    /// on success call OnSucceededToConnectToServer() via dispatcher
    /// if failed call OnFailedToConnectToServer() via dispatcher
    /// </summary>
    void ConnectToServer()
    {
      try
      {
        if (!_firstTime)
        {
          return;
        }
        ServiceScope.Get<ILogger>().Info("mytv:ConnectToServer");
        RemoteControl.HostName = UserSettings.GetString("tv", "serverHostName");

        string connectionString, provider;
        RemoteControl.Instance.GetDatabaseConnectionString(out connectionString, out provider);

        XmlDocument doc = new XmlDocument();
        doc.Load("gentle.config");
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString");
        XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
        node.InnerText = connectionString;
        nodeProvider.InnerText = provider;
        doc.Save("gentle.config");
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connectionString);

        ServiceScope.Get<ILogger>().Info("mytv:initialize channel navigator");
        TvChannelNavigator.Instance.Initialize();

        int cards = RemoteControl.Instance.Cards;
        IList channels = Channel.ListAll();
      }
      catch (Exception ex)
      {
        ServiceScope.Get<ILogger>().Info("mytv:initialize connect failed");
        ServiceScope.Get<ILogger>().Error(ex);
        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new ConnectToServerDelegate(OnFailedToConnectToServer));
        return;
      }
      ServiceScope.Get<ILogger>().Info("mytv:initialize connect succeeded");
      this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new ConnectToServerDelegate(OnSucceededToConnectToServer));
    }

    /// <summary>
    /// Called when we failed to connect to server.
    /// navigate to tv-setup window
    /// </summary>
    void OnFailedToConnectToServer()
    {
      this.NavigationService.Navigate(new Uri("/MyTv;component/TvSetup.xaml", UriKind.Relative));
    }

    /// <summary>
    /// Called when we succeeded in connecting to the tvserver
    /// update infobox and show video
    /// </summary>
    void OnSucceededToConnectToServer()
    {
      _firstTime = false;
      ServiceScope.Get<ILogger>().Info("mytv:check wmp version");
      WindowMediaPlayerCheck check = new WindowMediaPlayerCheck();
      if (!check.IsInstalled)
      {
        MpDialogOk dlg = new MpDialogOk();
        Window w = Window.GetWindow(this);
        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlg.Owner = w;
        dlg.Title = "";
        dlg.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);//Error
        dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 11);//Infinity needs Windows Media Player 10 or higher to playback video!";
        dlg.ShowDialog();
        return;
      }
      ServiceScope.Get<ILogger>().Info("mytv:check tsreader.ax installed");
      TsReaderCheck checkReader = new TsReaderCheck();
      {
        if (!checkReader.IsInstalled)
        {
          MpDialogOk dlg = new MpDialogOk();
          Window w = Window.GetWindow(this);
          dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
          dlg.Owner = w;
          dlg.Title = "";
          dlg.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);//Error
          dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 12);//Infinity needs TsReader.ax to be registered!
          dlg.ShowDialog();
          return;
        }
      }
      

      ServiceScope.Get<ILogger>().Info("mytv:OnSucceededToConnectToServer done");
    }
  }

}