#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.ServiceProcess;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Action = MediaPortal.GUI.Library.Action;

namespace Mediaportal.TV.TvPlugin
{
  public class TvSetup : GUIInternalWindow
  {

    #region Variables

    private string _hostName;
    [SkinControl(24)] protected GUIButtonControl btnChange = null;
    [SkinControl(25)] protected GUIButtonControl btnBack = null;
    [SkinControl(30)] protected GUILabelControl lblHostName = null;

    #endregion

    public TvSetup()
    {
      GetID = (int)Window.WINDOW_SETTINGS_TVENGINE;
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        _hostName = xmlreader.GetValueAsString("tvservice", "hostname", "");
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.SetValue("tvservice", "hostname", _hostName);
      }
    }

    #endregion

    #region private static methods

    private static void SwitchToHomeView()
    {
      bool basicHome;
      using (Settings xmlreader = new MPSettings())
      {
        basicHome = xmlreader.GetValueAsBool("gui", "startbasichome", false);
      }

      int homeWindow = basicHome ? (int)Window.WINDOW_SECOND_HOME : (int)Window.WINDOW_HOME;
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

      if (!CheckTcpPort(ServiceHelper.PortHttpService)) 
      {
        portErrors.Add(ServiceHelper.PortHttpService + "(HTTP) Service");
        succeeded = false;
      }

      if (!CheckTcpPort(ServiceHelper.PortTcpService)) 
      {
        portErrors.Add(ServiceHelper.PortTcpService + "(TCP) Service");
        succeeded = false;
      }

      if (succeeded)
      {
        try
        {
          int cards = ServiceAgents.Instance.ControllerServiceAgent.Cards;
          // Need to setup the database connection string here
          // before checking for database connectivity,
          // otherwhise we will check for the wrong database provider
          TVHome.Navigator.SetupDatabaseConnection();
        }
        catch (Exception)
        {          
          succeeded = false;
        }
      }
      return succeeded;
    }

    private bool CheckDatabaseConnection(List<string> portErrors)
    {
      bool succeeded = true;
      try
      {
        IEnumerable<Card> cards = ServiceAgents.Instance.CardServiceAgent.ListAllCards(CardIncludeRelationEnum.None);
      }
      catch (Exception)
      {
        succeeded = false;
      }
      
      return succeeded;
    }

    private bool CheckStreamingConnection(bool tvServerAvailable, bool databaseAvailable, List<string> portErrors)
    {
      bool succeeded = true;
      int RtspPort = 0;

      if (tvServerAvailable)
      {
        RtspPort = ServiceAgents.Instance.ControllerServiceAgent.StreamingPort;
      }
      if (RtspPort == 0 && databaseAvailable)
      {
        //todo gibman RTSP lookup removed for now, hardcoded to 554
        try
        {
          /*IList<Server.TVDatabase.Entities.Server> servers = Server.TVDatabase.Gentle.Server.ListAll();
          foreach (Server.TVDatabase.Entities.Server server in servers)
          {
            if (ServiceAgents.Instance.ControllerService.isMaster)
            {
              RtspPort = ServiceAgents.Instance.ControllerService.rtspPort;
              break;
            }
          }*/
          RtspPort = 554;
        }
        catch (Exception)
        {
          // do nothing
        }
      }

      if (RtspPort > 0)
      {
        if (!CheckTcpPort(RtspPort)) // RTSP streaming
        {
          portErrors.Add(RtspPort.ToString() + " (TCP) RTSP streaming");
          succeeded = false;
        }
      }
      else
      {
        succeeded = false;
      }

      return succeeded;
    }

    private void CheckTvServiceStatus()
    {
      // check if we are in a single seat environment, if so check if TvService is started - if not start it
      if (Network.IsSingleSeat())
      {
        this.LogInfo("TvSetup: Seems we are in a single seat environment - Checking if tvservice is running");
        ServiceController ctrl = null;
        try
        {
          ctrl = new ServiceController("TvService");
        }
        catch (Exception ex)
        {
          this.LogError(ex, "TvSetup: We can't get an instance of the tvservice service with error");
        }
        if (ctrl != null)
        {
          if (ctrl.Status == ServiceControllerStatus.Stopped)
          {
            this.LogInfo("TvSetup: TvService is stopped - trying to start...");
            ctrl.Start();
            // Wait until service started but no longer than 30 seconds
            try
            {
              ctrl.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));
            }
            catch (Exception) {}
            if (ctrl.Status == ServiceControllerStatus.Running)
            {
              this.LogInfo("TvSetup: TvService started.");
            }
            else
            {
              this.LogInfo("TvSetup: Failed to start TvService");
            }
          }
          else if (ctrl.Status == ServiceControllerStatus.Running)
          {
            this.LogInfo("TvSetup: TvService already started.");
          }
          else
          {
            this.LogInfo("TvSetup: TvService seems to be in an unusual state. Please check. Current state={0}",
                     ctrl.Status.ToString());
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
          ServiceAgents.Instance.Hostname = _hostName;
          if (lblHostName != null)
          {
            lblHostName.Label = _hostName;
          }
          GUIPropertyManager.SetProperty("#TV.setup.hostname", _hostName);

          CheckTvServiceStatus();

          List<string> portErrors = new List<string>();
          bool tvServerOk = CheckTvServerConnection(portErrors);
          bool databaseOk = CheckDatabaseConnection(portErrors);
          bool streamingOk = CheckStreamingConnection(tvServerOk, databaseOk, portErrors);

          //Show the check results dialog to the user
          GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          if (tvServerOk && databaseOk && streamingOk)
          {
            //TVHome.OnPageLoadDone = false;
            RemoteControl.UseIncreasedTimeoutForInitialConnection = true;

            TVHome.Navigator.ReLoad();
            Settings xmlreader = new MPSettings();
            TVHome.Navigator.LoadSettings(xmlreader);
            if (pDlgOK != null)
            {
              pDlgOK.SetHeading(GUILocalizeStrings.Get(605));
              pDlgOK.SetLine(1, GUILocalizeStrings.Get(200064)); // Connected to TvServer
              pDlgOK.SetLine(2, "");
              pDlgOK.SetLine(3, "");
              pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
            }
            //goto TV home, even directly fullscreen if configured this way.
            GUIWindowManager.ActivateWindow((int)Window.WINDOW_TV, true);

            return;
          }
          pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          if (pDlgOK != null)
          {
            if (portErrors.Count > 0)
            {
              pDlgOK.SetHeading(GUILocalizeStrings.Get(200065)); // Some ports seem to be blocked
              for (int i = 0; i < 4; i++)
              {
                if (i < portErrors.Count)
                {
                  pDlgOK.SetLine(i + 1, portErrors[i]);
                }
                else
                {
                  pDlgOK.SetLine(i + 1, "");
                }
              }
              pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
            }
            pDlgOK.SetHeading(GUILocalizeStrings.Get(605));
            if (tvServerOk)
            {
              pDlgOK.SetLine(1, GUILocalizeStrings.Get(200064)); // Connected to TvServer
            }
            else
            {
              pDlgOK.SetLine(1, GUILocalizeStrings.Get(200066)); // Unable to connect to TvServer
            }

            if (databaseOk)
            {
              pDlgOK.SetLine(2, GUILocalizeStrings.Get(200067)); // Connected to database
            }
            else
            {
              pDlgOK.SetLine(2, GUILocalizeStrings.Get(200068)); // Unable to connect to database
            }
            if (portErrors.Count == 0)
            {
              pDlgOK.SetLine(3, GUILocalizeStrings.Get(200069)); // All ip ports seem to be fine
            }
            else
            {
              pDlgOK.SetLine(3, GUILocalizeStrings.Get(200070)); // Please check firewall
            }
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
      if (lblHostName != null)
      {
        lblHostName.Label = _hostName;
      }
      GUIPropertyManager.SetProperty("#TV.setup.hostname", _hostName);
      base.OnPageLoad();
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      SaveSettings();      
      ServiceAgents.Instance.Hostname = _hostName;
      base.OnPageDestroy(new_windowId);
    }

    #endregion

    protected bool GetKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (keyboard == null)
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