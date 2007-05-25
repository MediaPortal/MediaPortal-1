using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.Configuration;
using System.Text;
using TvControl;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
namespace TvPlugin
{
  public class TvSetup : GUIWindow
  {
    string _hostName;
    [SkinControlAttribute(24)]    protected GUIButtonControl btnChange = null;
    [SkinControlAttribute(30)]    protected GUILabelControl lblHostName = null;

    public TvSetup()
    {
      GetID = (int)GUIWindow.Window.WINDOW_SETTINGS_TVENGINE;
    }

    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _hostName = xmlreader.GetValueAsString("tvservice", "hostname", "");
      }
    }

    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValue("tvservice", "hostname", _hostName);
      }
    }

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
            GUIWindowManager.ShowPreviousWindow();
            return;
          }
      }
      base.OnAction(action);
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == btnChange)
      {
        if (GetKeyboard(ref _hostName))
        {
          RemoteControl.Clear();
          RemoteControl.HostName = _hostName;
          lblHostName.Label = _hostName;

          bool tvServerOk = false;
          bool databaseOk = false;

          //check connection with tvserver
          try
          {
            int cards = RemoteControl.Instance.Cards;
            tvServerOk = true;
            TVHome.Navigator.ReLoad();
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

          //check connection with tvserver
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
            pDlgOK.SetHeading(GUILocalizeStrings.Get(605));
            if (tvServerOk)
              pDlgOK.SetLine(1, "Connected to TvServer");
            else
              pDlgOK.SetLine(1, "Unable to connect to the tvserver");

            if (databaseOk)
              pDlgOK.SetLine(2, "Connected to database");
            else
              pDlgOK.SetLine(2, "Unable to connect to database");
            pDlgOK.SetLine(3, "");
            pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
            return;
          }
        }
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

    protected bool GetKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
        return false;
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
