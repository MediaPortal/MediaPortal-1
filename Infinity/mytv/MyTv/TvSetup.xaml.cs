using System;
using System.Xml;
using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TvControl;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvSetup.xaml
  /// </summary>
  /// 
  public partial class TvSetup : System.Windows.Controls.Page
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TvSetup"/> class.
    /// </summary>
    public TvSetup()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Called when page gets loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      labelText.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 102);//Please enter the hostname of the tvserver
      buttonSave.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 103);//Save
      // Sets keyboard focus on the first Button in the sample.
      Keyboard.Focus(textboxServer);
    }
    /// <summary>
    /// Called when mouse enters a button
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
    void OnMouseEnter(object sender, MouseEventArgs e)
    {
      IInputElement b = sender as IInputElement;
      if (b != null)
      {
        Keyboard.Focus(b);
      }
    }
    /// <summary>
    /// Called when Save button is pressed
    /// tries to connect to the tvserver/database 
    /// if succeeded 
    ///   - stores the settings
    ///   - goes back to previous screen
    /// if not succeeded
    ///   - a dialog box is shown 
    ///   - user can retry
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnTest(object sender, EventArgs args)
    {
      string connectionString = "", provider = "";
      RemoteControl.Clear();
      RemoteControl.HostName = textboxServer.Text;
      bool tvServerOk = false;
      bool databaseOk = false;

      //check connection with tvserver
      try
      {
        ServiceScope.Get<ILogger>().Info("Connect to tvserver {0}", textboxServer.Text);
        int cards = RemoteControl.Instance.Cards;
        Gentle.Framework.ProviderFactory.ResetGentle(true);
        RemoteControl.Instance.GetDatabaseConnectionString(out connectionString, out provider);
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connectionString);

        tvServerOk = true;

      }
      catch (Exception ex)
      {
        ServiceScope.Get<ILogger>().Info("Unable to connect to  tvserver at {0}", RemoteControl.HostName);
        ServiceScope.Get<ILogger>().Error(ex);
        RemoteControl.Clear();
      }

      //check connection with database
      try
      {
        ServiceScope.Get<ILogger>().Info("Connect to database {0}", connectionString);
        IList cards = Card.ListAll();
        databaseOk = true;
      }
      catch (Exception ex)
      {
        ServiceScope.Get<ILogger>().Info("Unable to connect open database at {0}", connectionString);
        ServiceScope.Get<ILogger>().Error(ex);
      }
      if (tvServerOk && databaseOk)
      {
        try
        {
          RemoteControl.Instance.GetDatabaseConnectionString(out connectionString, out provider);

          XmlDocument doc = new XmlDocument();
          doc.Load("gentle.config");
          XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
          XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString");
          XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
          node.InnerText = connectionString;
          nodeProvider.InnerText = provider;
          doc.Save("gentle.config");
          ChannelNavigator.Instance.Initialize();
          UserSettings.SetString("tv", "serverHostName", RemoteControl.HostName);

        }
        catch (Exception)
        {
          result1.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 104);//"Unable to modify gentle.config";
          Keyboard.Focus(textboxServer);
          return;
        }
        this.NavigationService.GoBack();
      }
      else if (tvServerOk)
      {
        result1.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 105);//"Connected to tvserver, but unable to connect to database";
        Keyboard.Focus(textboxServer);
      }
      else
      {
        result1.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 106);//"Failed to connect to tvserver";
        Keyboard.Focus(textboxServer);
      }
    }
  }
}