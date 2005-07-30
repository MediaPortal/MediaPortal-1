/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
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
    readonly int[] months = new int[] {0,31,28,31,30,31,30,31,31,30,31,30,31};
    
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
    string channel=String.Empty;
    bool   enableEditStartTime=true;
    bool   enableEditChannel=true;
    DateTime startDateTime=DateTime.Now;
    DateTime endDateTime=DateTime.Now;
    ArrayList itemList = new ArrayList();

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

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
      int iYear,iMonth,iDay;
      int iHour,iMin;
      if ( control==spinStartMonth)
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
      if ( control==spinEndMonth)
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
      if ( control==btnOK)
      {
        iHour=spinStartHour.Value;
        iMin=spinStartMinute.Value;
        iDay=spinStartDay.Value;
        iMonth=spinStartMonth.Value;
        iYear=spinStartYear.Value;
        channel=spinChannel.GetLabel();

        startDateTime = new DateTime(iYear,iMonth,iDay,iHour,iMin,0,0);
        iHour=spinEndHour.Value;
        iMin=spinEndMinute.Value;
        iDay=spinEndDay.Value;
        iMonth=spinEndMonth.Value;
        iYear =spinEndYear.Value;
        endDateTime = new DateTime(iYear,iMonth,iDay,iHour,iMin,0,0);

        m_bConfirmed=true;
        Close();
        return ;
      }
		}

    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
				{
					m_pParentWindow=null;
					m_bRunning=false;
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
          spinStartHour.Value=startDateTime.Hour;

          spinStartMinute.SetRange(0,59);
          spinStartMinute.Value=startDateTime.Minute;

          if (DateTime.IsLeapYear(startDateTime.Year) && startDateTime.Month==2)
            spinStartDay.SetRange(1,29);
          else 
            spinStartDay.SetRange(1,months[startDateTime.Month]);
          spinStartDay.Value=startDateTime.Day;
          
          spinStartMonth.SetRange(1,12);
          spinStartMonth.Value=startDateTime.Month;
          
          spinStartYear.Value=startDateTime.Year;
          spinStartYear.SetRange(2004,2010);
          

					spinChannel.Reset();
          int i=0,iSel=0;
          foreach (string strLabel in itemList)
          {
            spinChannel.AddLabel(strLabel,0);
            if (channel==strLabel)
            {
              iSel=i;
            }
            i++;
          }
          if (iSel>=0)
            SelectItemControl(GetID, (int)spinChannel.GetID,iSel);

          spinEndHour.SetRange(0,23);
          spinEndHour.Value=endDateTime.Hour;

          spinEndMinute.SetRange(0,59);
          spinEndMinute.Value=endDateTime.Minute;

          if (DateTime.IsLeapYear(endDateTime.Year) && endDateTime.Month==2)
            spinEndDay.SetRange(1,29);
          else
            spinEndDay.SetRange(1,months[endDateTime.Month]);
          spinEndDay.Value=endDateTime.Day;
          

          spinEndMonth.SetRange(1,12);
          spinEndMonth.Value=endDateTime.Month;
          

          spinEndYear.Value=endDateTime.Year;
          spinEndYear.SetRange(2004,2010);
          
					spinStartHour.Disabled=!enableEditStartTime;
					spinStartMinute.Disabled=!enableEditStartTime;
					spinStartDay.Disabled=!enableEditStartTime;
					spinStartMonth.Disabled=!enableEditStartTime;
					spinStartYear.Disabled=!enableEditStartTime;
          

					spinChannel.Disabled=!enableEditChannel;

        }
        return true;

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
      get { return itemList;}
    }

    public DateTime StartDateTime
    {
      get { return startDateTime;}
      set { startDateTime=value;}
    }
    public DateTime EndDateTime
    {
      get { return endDateTime;}
      set { endDateTime=value;}
    }
    public void SetHeading(int iString)
    {
      SetHeading (GUILocalizeStrings.Get(iString) );
    }
    public void  SetHeading( string line)
    {
			LoadSkin();
			AllocResources();
			InitControls();

			lblHeading.Label=line;
    }
    public string Channel
    {
      get { return channel;}
      set { channel=value;}
    }
    public bool EnableStartTime
    {
      get { return enableEditStartTime;}
      set { enableEditStartTime=value;}
    }
    public bool EnableChannel
    {
      get { return enableEditChannel;}
      set { enableEditChannel=value;}
    }

  }
}
