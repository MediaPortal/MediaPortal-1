using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// 
  /// </summary>
  public class GUISettingsSlideshow : GUIWindow
  {
    enum Controls
    {
      CONTROL_SPEED =2,
      CONTROL_TRANSITION=3
    };

    int m_iSpeed=3;
    int m_iTransistion=20;

    public GUISettingsSlideshow()
    {
      GetID=(int)GUIWindow.Window.WINDOW_SETTINGS_SLIDESHOW;
    }

    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\SettingsSlideShow.xml");
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

          GUIControl.ClearControl(GetID,(int)Controls.CONTROL_TRANSITION);
          for (int i=1; i <=50;++i)
          {
            GUIControl.AddItemLabelControl(GetID,(int)Controls.CONTROL_TRANSITION,i.ToString());
          }
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_SPEED,m_iSpeed-1);
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_TRANSITION,m_iTransistion-1);
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
          }
          if (iControl==(int)Controls.CONTROL_TRANSITION)
          {
            string strLabel=message.Label;
            m_iTransistion=Int32.Parse(strLabel);
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
        m_iSpeed=xmlreader.GetValueAsInt("pictures","speed",3);
        m_iTransistion=xmlreader.GetValueAsInt("pictures","transition",20);
      }
      
    }

    void SaveSettings()
    {
      using(AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValue("pictures","speed",m_iSpeed.ToString());
        xmlwriter.SetValue("pictures","transition",m_iTransistion.ToString());
      }
    }
    #endregion

  }
}
