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
      CONTROL_TRANSITION=3,
      CONTROL_XFADE=4,
      CONTROL_KENBURNS=5,
      CONTROL_RANDOM=6
    };

    int m_iSpeed=3;
    int m_iTransistion=20;
    bool m_bXFade=false;
    bool m_bKenBurns=false;
    bool m_bRandom=false;

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

          if (m_bXFade)
          {
            GUIControl.SelectControl(GetID, (int)Controls.CONTROL_XFADE);
          }
          if (m_bKenBurns)
					{
						GUIControl.SelectControl(GetID, (int)Controls.CONTROL_KENBURNS);
					}
          if (m_bRandom)
          {
            GUIControl.SelectControl(GetID, (int)Controls.CONTROL_RANDOM);
          }

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
          if (iControl==(int)Controls.CONTROL_XFADE)
          {
            m_bXFade=true;
            m_bKenBurns=false;
            m_bRandom=false;
            UpdateButtons();
            return true;
          }
          if (iControl==(int)Controls.CONTROL_KENBURNS)
          {
            m_bXFade=false;
            m_bKenBurns=true;
            m_bRandom=false;
            UpdateButtons();
            return true;
          }
          if (iControl==(int)Controls.CONTROL_RANDOM)
          {
            m_bXFade=false;
            m_bKenBurns=false;
            m_bRandom=true;
            UpdateButtons();
            return true;
          }
        }
        break;

      }
      return base.OnMessage(message);
    }
    
    void UpdateButtons()
    {
      if (m_bRandom)
        GUIControl.SelectControl(GetID, (int)Controls.CONTROL_RANDOM);
      else
        GUIControl.DeSelectControl(GetID, (int)Controls.CONTROL_RANDOM);

      if (m_bXFade)
        GUIControl.SelectControl(GetID, (int)Controls.CONTROL_XFADE);
      else
        GUIControl.DeSelectControl(GetID, (int)Controls.CONTROL_XFADE);

      if (m_bKenBurns)
        GUIControl.SelectControl(GetID, (int)Controls.CONTROL_KENBURNS);
      else
        GUIControl.DeSelectControl(GetID, (int)Controls.CONTROL_KENBURNS);

    }
    
    #region Serialisation
    void LoadSettings()
    {
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        m_iSpeed=xmlreader.GetValueAsInt("pictures","speed",3);
        m_iTransistion=xmlreader.GetValueAsInt("pictures","transition",20);
        m_bKenBurns=xmlreader.GetValueAsBool("pictures","kenburns", false);
        m_bRandom=xmlreader.GetValueAsBool("pictures","random", false);
        m_bXFade = (!m_bRandom & !m_bKenBurns);
      }      
    }

    void SaveSettings()
    {
      using(AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValue("pictures","speed",m_iSpeed.ToString());
        xmlwriter.SetValue("pictures","transition",m_iTransistion.ToString());
        xmlwriter.SetValueAsBool("pictures","kenburns",m_bKenBurns);
        xmlwriter.SetValueAsBool("pictures","random",m_bRandom);
      }
    }
    #endregion

  }
}
