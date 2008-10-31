using System;
using System.Collections;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using TvControl;
using TvDatabase;
using System.Net.Sockets;
using System.ServiceProcess;

namespace TvPlugin
{
  public class TvSetup : GUIWindow
  {
    #region Variables
    string _hostName;
    [SkinControlAttribute(24)]    protected GUIButtonControl btnChange = null;
    [SkinControlAttribute(25)]    protected GUIButtonControl btnBack = null;
    [SkinControlAttribute(30)]    protected GUILabelControl lblHostName = null;
    #endregion

    public TvSetup()
    {
      GetID = (int)Window.WINDOW_SETTINGS_TVENGINE;
    }

    #region Serialisation
    private void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _hostName = xmlreader.GetValueAsString("tvservice", "hostname", "");
      }
    }
    private void SaveSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValue("tvservice", "hostname", _hostName);
      }
    }
    #endregion

    #region private static methods

    private static void SwitchToHomeView()
    {
      bool basicHome;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        basicHome = xmlreader.GetValueAsBool("general", "startbasichome", false);
      }

      int homeWindow = basicHome ? (int) Window.WINDOW_SECOND_HOME : (int) GUIWindow.Window.WINDOW_HOME;
      GUIWindowManager.ActivateWindow(homeWindow);
    }

    #endregion

    #region Check helpers
    private bool CheckTcpPort(int port)
    {
      TcpClient client = new TcpClient();
      try
      {
        client.Connect(_hostName, port);
      }
      catch (Exception)
      {
        return false;
      }
      client.Close();
      return true;
    }
    private bool CheckUdpPort(int port)
    {
      UdpClient client = new UdpClient();
      try
      {
        client.Connect(_hostName, port);
      }
      catch (Exception)
      {
        return false;
      }
      client.Close();
      return true;
    }

    private bool CheckTvServerConnection(List<string> portErrors)
    {
      bool succeeded = true;
      if (!CheckTcpPort(31456)) // TvService.RemoteControl
      {
        portErrors.Add("31456 (TCP) RemoteControl");
        succeeded = false;
      }
      if (!CheckTcpPort(554)) // RTSP streaming
      {
        portErrors.Add("554 (TCP) RTSP streaming");
        succeeded = false;
      }
      if (succeeded)
      {
        try
        {
          int cards = RemoteControl.Instance.Cards;
          TVHome.Navigator.ReLoad();
        }
        catch (Exception)
        {
          RemoteControl.Clear();
          succeeded = false;
        }
      }
      return succeeded;
    }
    private bool CheckDatabaseConnection(List<string> portErrors)
    {
      bool succeeded = true;
      string provider=Gentle.Framework.ProviderFactory.GetDefaultProvider().Name;
      if (provider.ToLower() == "sqlserver")
      {
        if (!CheckTcpPort(1433)) // MS SQL TCP Port
        {
          portErrors.Add("1433 (TCP) MS SQL Server");
          succeeded = false;
        }
        if (!CheckUdpPort(1434)) // MS SQL UDP Port
        {
          portErrors.Add("1434 (UDP) MS SQL Server");
          succeeded = false;
        }
      }
      else
      {
        if (!CheckTcpPort(3306)) // MS SQL TCP Port
        {
          portErrors.Add("3306 (TCP) MySQL Server");
          succeeded = false;
        }
      }
      if (succeeded)
      {
        try
        {
          IList cards = Card.ListAll();
        }
        catch (Exception)
        {
          succeeded = false;
        }
      }
      return succeeded;
    }
    private void CheckTvServiceStatus()
    {
      // check if we are in a single seat environment, if so check if TvService is started - if not start it
      if (TVHome.IsSingleSeat())
      {
        Log.Info("TvSetup: Seems we are in a single seat environment - Checking if tvservice is running");
        ServiceController ctrl = null;
        try
        {
          ctrl = new ServiceController("TvService");
        }
        catch (Exception ex)
        {
          Log.Info("TvSetup: We can't get an instance of the tvservice service with error: " + ex.Message);
        }
        if (ctrl != null)
        {
          if (ctrl.Status == ServiceControllerStatus.Stopped)
          {
            Log.Info("TvSetup: TvService is stopped - trying to start...");
            ctrl.Start();
            // Wait until service started but no longer than 30 seconds
            try
            {
              ctrl.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));
            }
            catch (Exception)
            {
            }
            if (ctrl.Status == ServiceControllerStatus.Running)
              Log.Info("TvSetup: TvService started.");
            else
              Log.Info("TvSetup: Failed to start TvService");
          }
          else if (ctrl.Status == ServiceControllerStatus.Running)
          {
            Log.Info("TvSetup: TvService already started.");
          }
          else
          {
            Log.Info("TvSetup: TvService seems to be in an unusual state. Please check. Current state={0}", ctrl.Status.ToString());
          }
        }
      }
    }
    #endregion

    #region Overrides

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\TvServerSetup.xml");
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            SwitchToHomeView();
            return;
          }
      }
      base.OnAction(action);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnChange)
      {
        if (GetKeyboard(ref _hostName))
        {
          RemoteControl.Clear();
          RemoteControl.HostName = _hostName;
          lblHostName.Label = _hostName;

          CheckTvServiceStatus();

          List<string> portErrors=new List<string>();
          bool tvServerOk = CheckTvServerConnection(portErrors);
          bool databaseOk = CheckDatabaseConnection(portErrors);
          
          //Show the check results dialog to the user
          GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
          if (tvServerOk && databaseOk)
          {
            TVHome.Navigator.ReLoad();
            if (pDlgOK != null)
            {
              pDlgOK.SetHeading(GUILocalizeStrings.Get(605));
              pDlgOK.SetLine(1, "Connected to TvServer");
              pDlgOK.SetLine(2, "");
              pDlgOK.SetLine(3, "");
              pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
            }
            return;
          }
          RemoteControl.Clear();
          pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
          if (pDlgOK != null)
          {
            if (portErrors.Count > 0)
            {
              pDlgOK.SetHeading("Some ports seem to be blocked");
              for (int i = 0; i < 4; i++)
              {
                if (i < portErrors.Count)
                  pDlgOK.SetLine(i + 1, portErrors[i]);
                else
                  pDlgOK.SetLine(i + 1, "");
              }
              pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
            }
            pDlgOK.SetHeading(GUILocalizeStrings.Get(605));
            if (tvServerOk)
              pDlgOK.SetLine(1, "Connected to TvServer");
            else
              pDlgOK.SetLine(1, "Unable to connect to the tvserver");

            if (databaseOk)
              pDlgOK.SetLine(2, "Connected to database");
            else
              pDlgOK.SetLine(2, "Unable to connect to database");
            if (portErrors.Count==0)
              pDlgOK.SetLine(3, "All ip ports seem to be fine");
            else
              pDlgOK.SetLine(3, "Please check firewall");
            pDlgOK.DoModal(GUIWindowManager.ActiveWindow);

            return;
          }
        }
      }
      else if (control == btnBack)
      {
        SwitchToHomeView();
      }
      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnPageLoad()
    {
      LoadSettings();
      lblHostName.Label = _hostName;
      base.OnPageLoad();
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      SaveSettings();
      RemoteControl.Clear();
      RemoteControl.HostName = _hostName;
      base.OnPageDestroy(new_windowId);
    }

    #endregion

    protected bool GetKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (keyboard == null )
      {
        return false;
      }
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
        return true;
      }
      return false;
    }
  }
}
