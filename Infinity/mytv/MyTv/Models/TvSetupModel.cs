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
using ProjectInfinity.Navigation;

namespace MyTv
{
  public class TvSetupModel : TvBaseViewModel
  {
    #region variables
    string _labelError;
    ICommand _saveCommand;
    string _hostName;
    #endregion

    #region ctor
    public TvSetupModel(Page page)
      :base(page)
    {
      _labelError="";
      _hostName = "";
    }
    #endregion
    #region properties
    public string LabelHeader
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 128);//setup tv;
      }
    }
    public string LabelDate
    {
      get
      {
        return DateTime.Now.ToString("dd-MM HH:mm");
      }
    }
    public string LabelSave
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 103);//Save
      }
    }
    public string LabelText
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 102);//Please enter the hostname of the tvserver
      }
    }
    public string LabelError
    {
      get
      {
        return _labelError;
      }
      set
      {
        _labelError = value;
        ChangeProperty("LabelError");

      }
    }
    #endregion
    #region commands
    public ICommand Save
    {
      get
      {
        if (_saveCommand == null)
        {
          _saveCommand = new SaveCommand(this);
        }
        return _saveCommand;
      }
    }
    public string HostName
    {
      get
      {
        return _hostName;
      }
      set
      {
        _hostName = value;
      }
    }
    #endregion
    #region command classes
    public class SaveCommand : ICommand
    {
      TvSetupModel _viewModel;
      public SaveCommand(TvSetupModel model)
      {
        _viewModel = model;
      }

      #region ICommand Members

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public event EventHandler CanExecuteChanged;

      public void Execute(object parameter)
      {
        string connectionString = "", provider = "";
        RemoteControl.Clear();
        RemoteControl.HostName = _viewModel.HostName;
        bool tvServerOk = false;
        bool databaseOk = false;

        //check connection with tvserver
        try
        {
          ServiceScope.Get<ILogger>().Info("Connect to tvserver {0}", _viewModel.HostName);
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
            TvChannelNavigator.Instance.Initialize();
            UserSettings.SetString("tv", "serverHostName", RemoteControl.HostName);
            ServiceScope.Get<ILogger>().Info("mytv:setuptv->connected successfully");
            ServiceScope.Get<INavigationService>().GoBack();

          }
          catch (Exception)
          {
            ServiceScope.Get<ILogger>().Info("mytv:setuptv->Unable to modify gentle.config");
            _viewModel.LabelError = ServiceScope.Get<ILocalisation>().ToString("mytv", 104);//"Unable to modify gentle.config";
            return;
          }
        }
        else if (tvServerOk)
        {
          ServiceScope.Get<ILogger>().Info("mytv:setuptv->unable to connect to database");
          _viewModel.LabelError = ServiceScope.Get<ILocalisation>().ToString("mytv", 105);//"Connected to tvserver, but unable to connect to database";
        }
        else
        {
          ServiceScope.Get<ILogger>().Info("mytv:setuptv->unable to connect to tvserver");
          _viewModel.LabelError = ServiceScope.Get<ILocalisation>().ToString("mytv", 106);//"Failed to connect to tvserver";
        }
      }

      #endregion
    }
    #endregion
  }
}
