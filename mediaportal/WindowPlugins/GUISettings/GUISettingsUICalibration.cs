using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Settings
{
	/// <summary>
	/// 
	/// </summary>
  public class GUISettingsUICalibration : GUIWindow
  {
    const int CONTROL_LABEL =2;
    int			m_iCountU;
    int			m_iCountD;
    int			m_iCountL;
    int			m_iCountR;
    int			m_iSpeed;
    long    m_dwLastTime;

    public GUISettingsUICalibration()
    {
      GetID=(int)GUIWindow.Window.WINDOW_UI_CALIBRATION;
    }

    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\settingsUICalibration.xml");
    }

		public override int GetFocusControlId()
		{
			return 1;
		}

    public override void OnAction(Action action)
    {
      if ((DateTime.Now.Ticks/10000)-m_dwLastTime > 500)
      {
        m_iSpeed=1;
        m_iCountU=0;
        m_iCountD=0;
        m_iCountL=0;
        m_iCountR=0;
      }
      m_dwLastTime=(DateTime.Now.Ticks/10000);

      int iXOff = GUIGraphicsContext.OffsetX;
      int iYOff = GUIGraphicsContext.OffsetY;

      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        GUIWindowManager.ShowPreviousWindow();
        return;
      }
      if (m_iSpeed>10) m_iSpeed=10; // Speed limit for accellerated cursors

      switch (action.wID)
      {
        case Action.ActionType.ACTION_MOVE_LEFT:
        {
          if (m_iCountL==0) m_iSpeed=1;
          if (iXOff>-128)
          {
            iXOff-=m_iSpeed;
            m_iCountL++;
            if (m_iCountL > 5) 
            {
              m_iSpeed+=1;
              m_iCountL=1;
            }
          }
          m_iCountU=0;
          m_iCountD=0;
          m_iCountR=0;
        }
        break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
        {
          if (m_iCountR==0) m_iSpeed=1;
          if (iXOff<128)
          {
            iXOff+=m_iSpeed;
            m_iCountR++;
            if (m_iCountR > 5) 
            {
              m_iSpeed+=1;
              m_iCountR=1;
            }
          }
          m_iCountU=0;
          m_iCountD=0;
          m_iCountL=0;
        }
        break;

        case Action.ActionType.ACTION_MOVE_UP:
        {
          if (m_iCountU==0) m_iSpeed=1;
          if (iYOff>-128)
          {
            iYOff-=m_iSpeed;
            m_iCountU++;
            if (m_iCountU > 5) 
            {
              m_iSpeed+=1;
              m_iCountU=1;
            }
          }
          m_iCountD=0;
          m_iCountL=0;
          m_iCountR=0;
        }
        break;

        case Action.ActionType.ACTION_MOVE_DOWN:
        {
          if (m_iCountD==0) m_iSpeed=1;
          if ( iYOff< 128 )
          {
            iYOff+=m_iSpeed;
            m_iCountD++;
            if (m_iCountD > 5) 
            {
              m_iSpeed+=1;
              m_iCountD=1;
            }
          }
          m_iCountU=0;
          m_iCountL=0;
          m_iCountR=0;
        }
        break;

        case Action.ActionType.ACTION_CALIBRATE_RESET:
          iXOff=0;
          iYOff=0;
          m_iSpeed=1;
          m_iCountU=0;
          m_iCountD=0;
          m_iCountL=0;
          m_iCountR=0;
        break;
        case Action.ActionType.ACTION_ANALOG_MOVE:
          float fX=2*action.fAmount1;
          float fY=2*action.fAmount2;
          if ( fX !=0.0 || fY!=0.0 )
          {
            iXOff += (int)fX;
            if ( iXOff < -128	 ) iXOff=-128;
            if ( iXOff > 128 ) iXOff=128;

            iYOff -= (int)fY;
            if ( iYOff < -128	 ) iYOff=-128;
            if ( iYOff > 128 ) iYOff=128;
          }
        break;
      }
      // do the movement
      if (GUIGraphicsContext.OffsetX != iXOff || GUIGraphicsContext.OffsetY != iYOff)
      {
        GUIGraphicsContext.OffsetX=iXOff ;
        GUIGraphicsContext.OffsetY=iYOff ;

        string strOffset;
        strOffset=String.Format("{0},{1}", iXOff, iYOff);
        GUIControl.SetControlLabel(GetID, CONTROL_LABEL,	strOffset);

        GUIGraphicsContext.OffsetX=GUIGraphicsContext.OffsetX;
        GUIGraphicsContext.OffsetY=GUIGraphicsContext.OffsetY;
        ResetAllControls();
        GUIWindowManager.ResetAllControls();
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          GUIWindowManager.Restore();
          GUIWindowManager.PreInit();
            GUIGraphicsContext.Save();
        }
        break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
          base.OnMessage(message);
          m_iSpeed=1;
          m_iCountU=0;
          m_iCountD=0;
          m_iCountL=0;
          m_iCountR=0;

          string strOffset;
          strOffset=String.Format("{0},{1}", GUIGraphicsContext.OffsetX, GUIGraphicsContext.OffsetY);
          GUIControl.SetControlLabel(GetID, CONTROL_LABEL,	strOffset);
          return true;
        }
      }
      return base.OnMessage(message);
    }
	}
}
