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

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvSetup.xaml
  /// </summary>

  public partial class TvSetup : System.Windows.Controls.Page
  {
    public TvSetup()
    {
      InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      // Sets keyboard focus on the first Button in the sample.
      Keyboard.Focus(textboxServer);
    }
    void OnMouseEnter(object sender, MouseEventArgs e)
    {
      IInputElement b = sender as IInputElement;
      if (b != null)
      {
        Keyboard.Focus(b);
      }
    }
    void OnTest(object sender, EventArgs args)
    {
      RemoteControl.Clear();
      RemoteControl.HostName = textboxServer.Text;
      bool tvServerOk = false;
      bool databaseOk = false;

      //check connection with tvserver
      try
      {
        int cards = RemoteControl.Instance.Cards;
        Gentle.Framework.ProviderFactory.ResetGentle(true);
        string connectionString, provider;
        RemoteControl.Instance.GetDatabaseConnectionString(out connectionString, out provider);
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connectionString);

        tvServerOk = true;

      }
      catch (Exception)
      {
        RemoteControl.Clear();
      }

      //check connection with database
      try
      {
        IList cards = Card.ListAll();
        databaseOk = true;
      }
      catch (Exception)
      {
      }
      if (tvServerOk && databaseOk)
      {
        try
        {
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
          ChannelNavigator.Instance.Initialize();
          UserSettings.SetString("tv", "serverHostName", RemoteControl.HostName);

        }
        catch (Exception)
        {
          result1.Content = "Unable to modify gentle.config";
          Keyboard.Focus(textboxServer);
          return;
        }
        this.NavigationService.GoBack();
      }
      else if (tvServerOk)
      {
        result1.Content = "Connected to tvserver, but unable to connect to database";
        Keyboard.Focus(textboxServer);
      }
      else
      {
        result1.Content = "Failed to connect to tvserver";
        Keyboard.Focus(textboxServer);
      }

    }
  }
}