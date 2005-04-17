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
    
    #region Base Dialog Variables
    bool m_bRunning=false;
    int m_dwParentWindowID=0;
    GUIWindow m_pParentWindow=null;
    #endregion
    
		
		[SkinControlAttribute(1)]				protected GUISpinControl spinChannel=null;
		[SkinControlAttribute(2)]				protected GUISpinControl spinStartHour=null;
		[SkinControlAttribute(3)]				protected GUISpinControl spinStartMinute=null;
		[SkinControlAttribute(4)]				protected GUISpinControl spinStartDay=null;
		[SkinControlAttribute(5)]				protected GUISpinControl spinStartMonth=null;
		[SkinControlAttribute(6)]				protected GUISpinControl spinStartYear=null;
		[SkinControlAttribute(7)]				protected GUISpinControl spinEndHour=null;
		[SkinControlAttribute(8)]				protected GUISpinControl spinEndMinute=null;
		[SkinControlAttribute(9)]				protected GUISpinControl spinEndDay=null;
		[SkinControlAttribute(10)]			protected GUISpinControl spinEndMonth=null;
		[SkinControlAttribute(11)]			protected GUISpinControl spinEndYear=null;
		[SkinControlAttribute(12)]			protected GUIButtonControl btnOK=null;
		[SkinControlAttribute(13)]			protected GUILabelControl lblHeading=null;

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
			lock (this)
			{
				// render the parent window
				if (null!=m_pParentWindow) 
					m_pParentWindow.Render(timePassed);

				GUIFontManager.Present();
				// render this dialog box
				base.Render(timePassed);
			}
    }

    void Close()
		{
			GUIWindowManager.IsSwitchingToNewWindow=true;
			lock (this)
			{
				GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,GetID,0,0,0,0,null);
				OnMessage(msg);

				GUIWindowManager.UnRoute();
				m_pParentWindow=null;
				m_bRunning=false;
			}
			GUIWindowManager.IsSwitchingToNewWindow=false;
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

			GUIWindowManager.IsSwitchingToNewWindow=true;
      GUIWindowManager.RouteToWindow( GetID );

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,GetID,0,0,0,0,null);
      OnMessage(msg);

			GUIWindowManager.IsSwitchingToNewWindow=false;
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
          spinStartHour.SetRange(0,23);
          spinStartHour.Value=m_dtStartDateTime.Hour;

          spinStartMinute.SetRange(0,59);
          spinStartMinute.Value=m_dtStartDateTime.Minute;

          if (DateTime.IsLeapYear(m_dtStartDateTime.Year) && m_dtStartDateTime.Month==2)
            spinStartDay.SetRange(1,29);
          else 
            spinStartDay.SetRange(1,months[m_dtStartDateTime.Month]);
          spinStartDay.Value=m_dtStartDateTime.Day;
          
          spinStartMonth.SetRange(1,12);
          spinStartMonth.Value=m_dtStartDateTime.Month;
          
          spinStartYear.Value=m_dtStartDateTime.Year;
          spinStartYear.SetRange(2004,2010);
          

					spinChannel.Reset();
          int i=0,iSel=0;
          foreach (string strLabel in m_items)
          {
            spinChannel.AddLabel(strLabel,0);
            if (m_strChannel==strLabel)
            {
              iSel=i;
            }
            i++;
          }
          if (iSel>=0)
            SelectItemControl(GetID, (int)spinChannel.GetID,iSel);

          spinEndHour.SetRange(0,23);
          spinEndHour.Value=m_dtEndDateTime.Hour;

          spinEndMinute.SetRange(0,59);
          spinEndMinute.Value=m_dtEndDateTime.Minute;

          if (DateTime.IsLeapYear(m_dtEndDateTime.Year) && m_dtEndDateTime.Month==2)
            spinEndMonth.SetRange(1,29);
          else
            spinEndMonth.SetRange(1,months[m_dtEndDateTime.Month]);
          spinEndMonth.Value=m_dtEndDateTime.Day;
          

          spinEndMonth.SetRange(1,12);
          spinEndMonth.Value=m_dtEndDateTime.Month;
          

          spinEndYear.Value=m_dtEndDateTime.Year;
          spinEndYear.SetRange(2004,2010);
          
					spinStartHour.Disabled=!m_bEditStartTime;
					spinStartMinute.Disabled=!m_bEditStartTime;
					spinStartDay.Disabled=!m_bEditStartTime;
					spinStartMonth.Disabled=!m_bEditStartTime;
					spinStartYear.Disabled=!m_bEditStartTime;
          

					spinChannel.Disabled=!m_bEditChannel;

        }
        return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
        {
          int iYear,iMonth,iDay;
          int iHour,iMin;
          int iControl=message.SenderControlId;
          if ( iControl==(int)spinStartMonth.GetID)
          {
            iYear=spinStartYear.Value;
            iMonth=spinStartMonth.Value;
            if (iMonth==2 && DateTime.IsLeapYear(iYear) )
            {
              spinStartDay.SetRange(1, 29);
            }
            else
            {
              spinStartDay.SetRange(1, months[iMonth]);
            }
          }
          if ( iControl==(int)spinEndMonth.GetID)
          {
            iYear=spinEndYear.Value;
            iMonth=spinEndMonth.Value;
            if (iMonth==2 && DateTime.IsLeapYear(iYear) )
            {
              spinEndDay.SetRange(1, 29);
            }
            else
            {
              spinEndDay.SetRange(1, months[iMonth]);
            }
          }
          if ( iControl==(int)btnOK.GetID )
          {
            iHour=spinStartHour.Value;
            iMin=spinStartMinute.Value;
            iDay=spinStartDay.Value;
            iMonth=spinStartMonth.Value;
            iYear=spinStartYear.Value;
            m_strChannel=spinChannel.GetLabel();

            m_dtStartDateTime = new DateTime(iYear,iMonth,iDay,iHour,iMin,0,0);
            iHour=spinEndHour.Value;
            iMin=spinEndMinute.Value;
            iDay=spinEndDay.Value;
            iMonth=spinEndMonth.Value;
            iYear =spinEndYear.Value;
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

			lblHeading.Label=strLine;
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

  }
}
