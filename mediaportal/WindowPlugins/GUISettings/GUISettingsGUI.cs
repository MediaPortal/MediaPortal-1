using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// 
  /// </summary>
  public class GUISettingsGUI : GUIWindow
  {
    enum Controls
    {
      CONTROL_SPEED =2,
      CONTROL_EXAMPLE=25,
      CONTROL_EXAMPLE2=26,
    };

    int m_iSpeed=5;

    public GUISettingsGUI()
    {
      GetID=(int)GUIWindow.Window.WINDOW_SETTINGS_GUI;
    }

    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\SettingsGUI.xml");
    }
    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
        {
          GUIWindowManager.PreviousWindow();
          return;
        }
      }
      base.OnAction(action);
    }
    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
          base.OnMessage(message);
          LoadSettings();
          GUIControl.ClearControl(GetID,(int)Controls.CONTROL_SPEED);
          for (int i=1; i <=10;++i)
          {
            GUIControl.AddItemLabelControl(GetID,(int)Controls.CONTROL_SPEED,i.ToString());
          }
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_SPEED,m_iSpeed-1);

          GUIControl.ClearControl(GetID,(int)Controls.CONTROL_EXAMPLE);
          GUIControl.ClearControl(GetID,(int)Controls.CONTROL_EXAMPLE2);

          string strTmp="Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." ;
          GUIControl.AddItemLabelControl( GetID,  (int)Controls.CONTROL_EXAMPLE, strTmp );
          GUIControl.SetControlLabel( GetID,  (int)Controls.CONTROL_EXAMPLE2, strTmp );
          return true;
        }
        
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          SaveSettings();
        }
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
        {
          int iControl=message.SenderControlId;
          if (iControl==(int)Controls.CONTROL_SPEED)
          {
            string strLabel=message.Label;
            m_iSpeed=Int32.Parse(strLabel);
            GUIGraphicsContext.ScrollSpeed=m_iSpeed;
          }

        }
          break;

      }
      return base.OnMessage(message);
    }

    
    #region Serialisation
    void LoadSettings()
    {
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        m_iSpeed=xmlreader.GetValueAsInt("general","scrollspeed",5);
      }
      
    }

    void SaveSettings()
    {
      using(AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValue("general","scrollspeed",m_iSpeed.ToString());
      }
    }
    #endregion

  }
}
