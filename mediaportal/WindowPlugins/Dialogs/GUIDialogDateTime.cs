using System;
using System.Collections;
using MediaPortal.GUI.Library;


namespace MediaPortal.Dialogs
{
  /// <summary>
  /// Shows a dialog box with an OK button  
  /// </summary>
  public class GUIDialogDateTime: GUIWindow
  {
    int[] months = new int[] {0,31,28,31,30,31,30,31,31,30,31,30,31};
    enum Controls : int
    {
      Channel=1,
      StartTimeHours=2,
      StartTimeMinutes=3,
      StartDateDay=4,
      StartDateMonth=5,
      StartDateYear=6,
      EndTimeHours=7,
      EndTimeMinutes=8,
      EndDateDay=9,
      EndDateMonth=10,
      EndDateYear=11,
      OKButton=12,
      Heading=13
    }
    #region Base Dialog Variables
    bool m_bRunning=false;
    int m_dwParentWindowID=0;
    GUIWindow m_pParentWindow=null;
    #endregion
    
    bool m_bConfirmed = false;
    bool m_bPrevOverlay=true;
    string m_strChannel="";
    bool   m_bEditStartTime=true;
    bool   m_bEditChannel=true;
    DateTime m_dtStartDateTime=DateTime.Now;
    DateTime m_dtEndDateTime=DateTime.Now;
    ArrayList m_items = new ArrayList();

    public GUIDialogDateTime()
    {
      GetID=(int)GUIWindow.Window.WINDOW_DIALOG_DATETIME;
    }

    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\dialogDateTime.xml");
    }

    public override bool SupportsDelayedLoad
    {
      get { return true;}
    }    
    public override void PreInit()
    {
    }


    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG ||action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        Close();
        return;
      }
      base.OnAction(action);
    }

    #region Base Dialog Members
    public void RenderDlg(float timePassed)
    {
      // render the parent window
      if (null!=m_pParentWindow) 
        m_pParentWindow.Render(timePassed);

      GUIFontManager.Present();
      // render this dialog box
      base.Render(timePassed);
    }

    void Close()
    {
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,GetID,0,0,0,0,null);
      OnMessage(msg);

      GUIWindowManager.UnRoute();
      m_pParentWindow=null;
      m_bRunning=false;
    }

    public void DoModal(int dwParentId)
    {
      m_dwParentWindowID=dwParentId;
      m_pParentWindow=GUIWindowManager.GetWindow( m_dwParentWindowID);
      if (null==m_pParentWindow)
      {
        m_dwParentWindowID=0;
        return;
      }

      GUIWindowManager.RouteToWindow( GetID );

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,GetID,0,0,0,0,null);
      OnMessage(msg);

      m_bRunning=true;
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
        System.Threading.Thread.Sleep(100);
      }
    }
    #endregion
	
    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          GUIGraphicsContext.Overlay=m_bPrevOverlay;		
          FreeResources();
          DeInitControls();

          return true;
        }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
          m_bPrevOverlay=GUIGraphicsContext.Overlay;
          base.OnMessage(message);
          m_bConfirmed = false;
          GUIGraphicsContext.Overlay=false;
          GUISpinControl cntl=(GUISpinControl)GetControl((int)Controls.StartTimeHours);
          cntl.SetRange(0,23);
          
          cntl.Value=m_dtStartDateTime.Hour;

          cntl=(GUISpinControl)GetControl((int)Controls.StartTimeMinutes);
          cntl.SetRange(0,59);
          
          cntl.Value=m_dtStartDateTime.Minute;

          cntl=(GUISpinControl)GetControl((int)Controls.StartDateDay);
          if (DateTime.IsLeapYear(m_dtStartDateTime.Year) && m_dtStartDateTime.Month==2)
            cntl.SetRange(1,29);
          else 
            cntl.SetRange(1,months[m_dtStartDateTime.Month]);
          cntl.Value=m_dtStartDateTime.Day;
          
          

          cntl=(GUISpinControl)GetControl((int)Controls.StartDateMonth);
          cntl.SetRange(1,12);
          cntl.Value=m_dtStartDateTime.Month;
          

          cntl=(GUISpinControl)GetControl((int)Controls.StartDateYear);
          cntl.Value=m_dtStartDateTime.Year;
          cntl.SetRange(2004,2010);
          

          ClearControl(GetID, (int)Controls.Channel);
          int i=0,iSel=0;
          foreach (string strLabel in m_items)
          {
            AddItemControl(GetID,(int)Controls.Channel,strLabel);
            if (m_strChannel==strLabel)
            {
              iSel=i;
            }
            i++;
          }
          if (iSel>=0)
            SelectItemControl(GetID, (int)Controls.Channel,iSel);

          cntl=(GUISpinControl)GetControl((int)Controls.Channel);
          

          
          cntl=(GUISpinControl)GetControl((int)Controls.EndTimeHours);
          cntl.SetRange(0,23);
          
          cntl.Value=m_dtEndDateTime.Hour;

          cntl=(GUISpinControl)GetControl((int)Controls.EndTimeMinutes);
          cntl.SetRange(0,59);
          
          cntl.Value=m_dtEndDateTime.Minute;

          cntl=(GUISpinControl)GetControl((int)Controls.EndDateDay);
          if (DateTime.IsLeapYear(m_dtEndDateTime.Year) && m_dtEndDateTime.Month==2)
            cntl.SetRange(1,29);
          else
            cntl.SetRange(1,months[m_dtEndDateTime.Month]);
          cntl.Value=m_dtEndDateTime.Day;
          

          cntl=(GUISpinControl)GetControl((int)Controls.EndDateMonth);
          cntl.SetRange(1,12);
          cntl.Value=m_dtEndDateTime.Month;
          

          cntl=(GUISpinControl)GetControl((int)Controls.EndDateYear);
          cntl.Value=m_dtEndDateTime.Year;
          cntl.SetRange(2004,2010);
          
          if (!m_bEditStartTime)
          {
            DisableControl((int)Controls.StartTimeHours);
            DisableControl((int)Controls.StartTimeMinutes);
            DisableControl((int)Controls.StartDateDay);
            DisableControl((int)Controls.StartDateMonth);
            DisableControl((int)Controls.StartDateYear);
          }
          else
          {
            EnableControl((int)Controls.StartTimeHours);
            EnableControl((int)Controls.StartTimeMinutes);
            EnableControl((int)Controls.StartDateDay);
            EnableControl((int)Controls.StartDateMonth);
            EnableControl((int)Controls.StartDateYear);
          }

          if (!m_bEditChannel)
          {
            DisableControl((int)Controls.Channel);
          }
          else
          {
            EnableControl((int)Controls.Channel);
          }

        }
        return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
        {
          int iYear,iMonth,iDay;
          int iHour,iMin;
          int iControl=message.SenderControlId;
          GUISpinControl cntl;
          if ( iControl==(int)Controls.StartDateMonth)
          {
            cntl=(GUISpinControl)GetControl((int)Controls.StartDateYear);
            iYear=cntl.Value;

            cntl=(GUISpinControl)GetControl((int)Controls.StartDateMonth);
            iMonth=cntl.Value;
            cntl=(GUISpinControl)GetControl((int)Controls.StartDateDay);
            if (iMonth==2 && DateTime.IsLeapYear(iYear) )
            {
              cntl.SetRange(1, 29);
            }
            else
            {
              cntl.SetRange(1, months[iMonth]);
            }
          }
          if ( iControl==(int)Controls.EndDateMonth)
          {
            cntl=(GUISpinControl)GetControl((int)Controls.EndDateYear);
            iYear=cntl.Value;

            cntl=(GUISpinControl)GetControl((int)Controls.EndDateMonth);
            iMonth=cntl.Value;
            cntl=(GUISpinControl)GetControl((int)Controls.EndDateDay);
            if (iMonth==2 && DateTime.IsLeapYear(iYear) )
            {
              cntl.SetRange(1, 29);
            }
            else
            {
              cntl.SetRange(1, months[iMonth]);
            }
          }
          if ( iControl==(int)Controls.OKButton )
          {
            cntl=(GUISpinControl)GetControl((int)Controls.StartTimeHours);
            iHour=cntl.Value;

            cntl=(GUISpinControl)GetControl((int)Controls.StartTimeMinutes);
            iMin=cntl.Value;

            cntl=(GUISpinControl)GetControl((int)Controls.StartDateDay);
            iDay=cntl.Value;

            cntl=(GUISpinControl)GetControl((int)Controls.StartDateMonth);
            iMonth=cntl.Value;

            cntl=(GUISpinControl)GetControl((int)Controls.StartDateYear);
            iYear=cntl.Value;

            cntl=(GUISpinControl)GetControl((int)Controls.Channel);
            m_strChannel=cntl.GetLabel();

            m_dtStartDateTime = new DateTime(iYear,iMonth,iDay,iHour,iMin,0,0);
        
            cntl=(GUISpinControl)GetControl((int)Controls.EndTimeHours);
            iHour=cntl.Value;

            cntl=(GUISpinControl)GetControl((int)Controls.EndTimeMinutes);
            iMin=cntl.Value;

            cntl=(GUISpinControl)GetControl((int)Controls.EndDateDay);
            iDay=cntl.Value;

            cntl=(GUISpinControl)GetControl((int)Controls.EndDateMonth);
            iMonth=cntl.Value;

            cntl=(GUISpinControl)GetControl((int)Controls.EndDateYear);
            iYear =cntl.Value;
            m_dtEndDateTime = new DateTime(iYear,iMonth,iDay,iHour,iMin,0,0);

            m_bConfirmed=true;
            Close();
            return true;
          }
        }
        break;
      }

      return base.OnMessage(message);
    }


    public bool IsConfirmed
    {
      get { return m_bConfirmed;}
    }


    public override void Render(float timePassed)
    {
      RenderDlg(timePassed);
    }
    void ClearControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, iWindowId,0, iControlId,0,0,null);
      OnMessage(msg);
    }

    void AddItemControl(int iWindowId, int iControlId,string strLabel)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, iWindowId,0, iControlId,0,0,null);
      msg.Label=strLabel;
      OnMessage(msg);
    }    
    void SelectItemControl(int iWindowId, int iControlId,int iItem)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, iWindowId,0, iControlId,iItem,0,null);
      OnMessage(msg);
    }
    
    public ArrayList Items
    {
      get { return m_items;}
    }

    public DateTime StartDateTime
    {
      get { return m_dtStartDateTime;}
      set { m_dtStartDateTime=value;}
    }
    public DateTime EndDateTime
    {
      get { return m_dtEndDateTime;}
      set { m_dtEndDateTime=value;}
    }
    public void SetHeading(int iString)
    {
      SetHeading (GUILocalizeStrings.Get(iString) );
    }
    public void  SetHeading( string strLine)
    {
			LoadSkin();
			AllocResources();
			InitControls();

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID,0, (int)Controls.Heading,0,0,null);
      msg.Label=strLine; 
      OnMessage(msg);
    }
    public string Channel
    {
      get { return m_strChannel;}
      set { m_strChannel=value;}
    }
    public bool EnableStartTime
    {
      get { return m_bEditStartTime;}
      set { m_bEditStartTime=value;}
    }
    public bool EnableChannel
    {
      get { return m_bEditChannel;}
      set { m_bEditChannel=value;}
    }

    void DisableControl(int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DISABLED, GetID, 0, iControlId, 0, 0, null);
      OnMessage(msg);
    }
		
    void EnableControl( int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ENABLED, GetID, 0, iControlId, 0, 0, null);
      OnMessage(msg);
    }
  }
}
