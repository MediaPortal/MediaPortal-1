using System;
using System.Collections;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
namespace MediaPortal.GUI.TV
{
	/// <summary>
	/// 
	/// </summary>
	public class GUITVScheduler :GUIWindow, IComparer
	{
    enum Controls
    {
      CONTROL_BTNSORTBY=2,
      CONTROL_BTNSORTASC=4,
      CONTROL_NEW=6,
      CONTROL_CLEANUP=7,
      CONTROL_LIST=10,
    };
    enum SortMethod
    {
      SORT_CHANNEL=0,
      SORT_DATE=1,
      SORT_NAME=2,
      SORT_TYPE=3,
      SORT_STATUS=4,
    }

    SortMethod        currentSortMethod=SortMethod.SORT_DATE;
    bool              m_bSortAscending=true;

    public  GUITVScheduler()
    {
      GetID=(int)GUIWindow.Window.WINDOW_SCHEDULER;
    }
    ~GUITVScheduler()
    {
    }
    
    public override bool Init()
    {
      bool bResult=Load (GUIGraphicsContext.Skin+@"\mytvscheduler.xml");
      LoadSettings();
      return bResult;
    }


    #region Serialisation
    void LoadSettings()
    {
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        string strTmp="";
        strTmp=(string)xmlreader.GetValue("tvscheduler","sort");
        if (strTmp!=null)
        {
          if (strTmp=="channel") currentSortMethod=SortMethod.SORT_CHANNEL;
          else if (strTmp=="date") currentSortMethod=SortMethod.SORT_DATE;
          else if (strTmp=="name") currentSortMethod=SortMethod.SORT_NAME;
          else if (strTmp=="type") currentSortMethod=SortMethod.SORT_TYPE;
          else if (strTmp=="status") currentSortMethod=SortMethod.SORT_STATUS;
        }
        m_bSortAscending=xmlreader.GetValueAsBool("tvscheduler","sortascending",true);
      }
    }

    void SaveSettings()
    {
      using(AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        switch (currentSortMethod)
        {
          case SortMethod.SORT_CHANNEL:
            xmlwriter.SetValue("tvscheduler","sort","channel");
            break;
          case SortMethod.SORT_DATE:
            xmlwriter.SetValue("tvscheduler","sort","date");
            break;
          case SortMethod.SORT_NAME:
            xmlwriter.SetValue("tvscheduler","sort","name");
            break;
          case SortMethod.SORT_TYPE:
            xmlwriter.SetValue("tvscheduler","sort","type");
            break;
          case SortMethod.SORT_STATUS:
            xmlwriter.SetValue("tvscheduler","sort","status");
            break;
        }
        xmlwriter.SetValueAsBool("tvscheduler","sortascending",m_bSortAscending);
      }
    }
    #endregion


    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
        {
          GUIWindowManager.PreviousWindow();
          return;
        }
        case Action.ActionType.ACTION_SHOW_GUI:
          if (Recorder.View)
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
          }
          break;
      }
      base.OnAction(action);
    }
    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          
          SaveSettings();
        }
          break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
          base.OnMessage(message);
					
					LoadSettings();
          LoadDirectory();
          return true;
        }
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl=message.SenderControlId;
          if (iControl==(int)Controls.CONTROL_BTNSORTASC)
          {
            m_bSortAscending=!m_bSortAscending;
            OnSort();
          }

          if (iControl==(int)Controls.CONTROL_BTNSORTBY) // sort by
          {
            switch (currentSortMethod)
            {
              case SortMethod.SORT_CHANNEL:
                currentSortMethod=SortMethod.SORT_DATE;
                break;
              case SortMethod.SORT_DATE:
                currentSortMethod=SortMethod.SORT_NAME;
                break;
              case SortMethod.SORT_NAME:
                currentSortMethod=SortMethod.SORT_TYPE;
                break;
              case SortMethod.SORT_TYPE:
                currentSortMethod=SortMethod.SORT_STATUS;
                break;
              case SortMethod.SORT_STATUS:
                currentSortMethod=SortMethod.SORT_CHANNEL;
                break;
            }
            OnSort();
          }
          if (iControl==(int)Controls.CONTROL_LIST)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
            OnMessage(msg);         
            int iItem=(int)msg.Param1;
            int iAction=(int)message.Param1;
            if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
            {
              OnClick(iItem);
            }
          }
          if (iControl==(int)Controls.CONTROL_CLEANUP)
          {
            OnCleanup();
          }
          if (iControl==(int)Controls.CONTROL_NEW)
          {
            OnNewShedule();
          }
        break;

      }
      return base.OnMessage(message);
    }

    void LoadDirectory()
    {
      GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST);

      ArrayList itemlist = new ArrayList();
      TVDatabase.GetRecordings(ref itemlist);
      foreach (TVRecording rec in itemlist)
      {
        GUIListItem item=new GUIListItem();
        item.Label=rec.Title;
        item.TVTag=rec;
        string strLogo=Utils.GetCoverArt(GUITVHome.TVChannelCovertArt,rec.Channel);
        if (!System.IO.File.Exists(strLogo))
        {
          strLogo="defaultVideoBig.png";
        }
        int card;
        if (Recorder.IsRecordingSchedule(rec,out card))
        {
          item.PinImage="tvguide_record_button.png";
        }
        item.ThumbnailImage=strLogo;
        item.IconImageBig=strLogo;
        item.IconImage=strLogo;
        GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
      }
      
      string strObjects=String.Format("{0} {1}", itemlist.Count, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount",strObjects);

      OnSort();
      UpdateButtons();
    }

    void UpdateButtons()
    {
      string strLine="";
      switch (currentSortMethod)
      {
        case SortMethod.SORT_CHANNEL:
          strLine=GUILocalizeStrings.Get(620);// Sort by: Channel
          break;
        case SortMethod.SORT_DATE:
          strLine=GUILocalizeStrings.Get(621);// Sort by: Date
          break;
        case SortMethod.SORT_NAME:
          strLine=GUILocalizeStrings.Get(268);// Sort by: Title
          break;
        case SortMethod.SORT_TYPE:
          strLine=GUILocalizeStrings.Get(623);// Sort by: Type
          break;
        case SortMethod.SORT_STATUS:
          strLine=GUILocalizeStrings.Get(685);// Sort by: Status
          break;
      }
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTNSORTBY,strLine);

      if (m_bSortAscending)
        GUIControl.DeSelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
      else
        GUIControl.SelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
    }
    GUIListItem GetSelectedItem()
    {
      int iControl;
      iControl=(int)Controls.CONTROL_LIST;
      GUIListItem item = GUIControl.GetSelectedListItem(GetID,iControl);
      return item;
    }

    GUIListItem GetItem(int iItem)
    {
      int iControl;
      iControl=(int)Controls.CONTROL_LIST;
      GUIListItem item = GUIControl.GetListItem(GetID,iControl,iItem);
      return item;
    }

    int GetSelectedItemNo()
    {
      int iControl;
      iControl=(int)Controls.CONTROL_LIST;

      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
      OnMessage(msg);         
      int iItem=(int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      int iControl;
      iControl=(int)Controls.CONTROL_LIST;

      return GUIControl.GetItemCount(GetID,iControl);
    }


    #region Sort Members
    void OnSort()
    {
      SetLabels();
      GUIListControl list=(GUIListControl)GetControl((int)Controls.CONTROL_LIST);
      list.Sort(this);
      UpdateButtons();
    }

    public int Compare(object x, object y)
    {
      if (x==y) return 0;
      GUIListItem item1=(GUIListItem)x;
      GUIListItem item2=(GUIListItem)y;
      if (item1==null) return -1;
      if (item2==null) return -1;

      int iComp=0;
      TimeSpan ts;
      TVRecording rec1=(TVRecording)item1.TVTag;
      TVRecording rec2=(TVRecording)item2.TVTag;
      switch (currentSortMethod)
      {
        case SortMethod.SORT_NAME:
          if (m_bSortAscending)
          {
            iComp=String.Compare(rec1.Title,rec2.Title,true);
            if (iComp==0) goto case SortMethod.SORT_CHANNEL;
            else return iComp;
          }
          else
          {
            iComp=String.Compare(rec2.Title ,rec1.Title,true);
            if (iComp==0) goto case SortMethod.SORT_CHANNEL;
            else return iComp;
          }
        
        case SortMethod.SORT_STATUS:
          if (m_bSortAscending)
          {
            iComp=String.Compare(item1.Label3,item2.Label3,true);
          }
          else
          {
            iComp=String.Compare(item2.Label3,item1.Label3,true);
          }
          if (iComp==0) goto case SortMethod.SORT_CHANNEL;
          else return iComp;

        case SortMethod.SORT_CHANNEL:
          if (m_bSortAscending)
          {
            iComp=String.Compare(rec1.Channel,rec2.Channel,true);
            if (iComp==0) goto case SortMethod.SORT_DATE;
            else return iComp;
          }
          else
          {
            iComp=String.Compare(rec2.Channel,rec1.Channel,true);
            if (iComp==0) goto case SortMethod.SORT_DATE;
            else return iComp;
          }

        case SortMethod.SORT_DATE:
          if (m_bSortAscending)
          {
            if (rec1.StartTime==rec2.StartTime) return 0;
            if (rec1.StartTime>rec2.StartTime) return 1;
            return -1;
          }
          else
          {
            if (rec2.StartTime==rec1.StartTime) return 0;
            if (rec2.StartTime>rec1.StartTime) return 1;
            return -1;
          }

        case SortMethod.SORT_TYPE:
          item1.Label2=GetRecType(rec1.RecType);
          item2.Label2=GetRecType(rec2.RecType);
          if (rec1.RecType!=rec2.RecType) 
          {
            if (m_bSortAscending)
              return (int)rec1.RecType-(int)rec2.RecType;
            else
              return (int)rec2.RecType-(int)rec1.RecType;
          }
          if (rec1.StartTime!=rec2.StartTime)
          {
            if (m_bSortAscending)
            {
              ts=rec1.StartTime - rec2.StartTime;
              return (int)(ts.Minutes);
            }
            else
            {
              ts=rec2.StartTime - rec1.StartTime;
              return (int)(ts.Minutes);
            }
          }
          if (rec1.Channel!=rec2.Channel)
            if (m_bSortAscending)
              return String.Compare(rec1.Channel,rec2.Channel);
            else
              return String.Compare(rec2.Channel,rec1.Channel);
          if (rec1.Title!=rec2.Title)
            if (m_bSortAscending)
              return String.Compare(rec1.Title,rec2.Title);
            else
              return String.Compare(rec2.Title,rec1.Title);
          return 0;
      } 
      return 0;
    }
    #endregion


    void SetLabels()
    {
      SortMethod method=currentSortMethod;
      bool bAscending=m_bSortAscending;

      for (int i=0; i < GetItemCount();++i)
      {
        GUIListItem item=GetItem(i);
        TVRecording rec=(TVRecording)item.TVTag;
        
        switch (rec.Status)
        {
          case TVRecording.RecordingStatus.Waiting:
            item.Label3=GUILocalizeStrings.Get(681);//waiting
          break;
          case TVRecording.RecordingStatus.Finished:
            item.Label3=GUILocalizeStrings.Get(683);//Finished
            break;
          case TVRecording.RecordingStatus.Canceled:
            item.Label3=GUILocalizeStrings.Get(684);//Canceled
            break;
        }

        // check with recorder.
        int card;
        if (Recorder.IsRecordingSchedule(rec, out card))
        {
          item.Label3=GUILocalizeStrings.Get(682);//Recording
        }
        
        switch (currentSortMethod)
        {
          case SortMethod.SORT_CHANNEL:
            goto case SortMethod.SORT_NAME;
          case SortMethod.SORT_DATE:
            goto case SortMethod.SORT_NAME;
          case SortMethod.SORT_NAME:
            goto case SortMethod.SORT_TYPE;
          case SortMethod.SORT_TYPE:
            string strType="";
            item.Label=rec.Title;
            string strTime=String.Format("{0} {1} - {2}", 
                                rec.StartTime.ToShortDateString() , 
                                rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
                                rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
            switch (rec.RecType)
            {
              case TVRecording.RecordingType.Once:
								item.Label2=String.Format("{0} {1} - {2}", 
												Utils.GetShortDayString(rec.StartTime) , 
												rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
												rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));;
                break;
              case TVRecording.RecordingType.Daily:
                strTime=String.Format("{0}-{1}", 
                                        rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
                                        rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
                strType=GUILocalizeStrings.Get(648) ;
                item.Label2=String.Format("{0} {1}",strType,strTime);
                break;

              case TVRecording.RecordingType.WeekDays:
                strTime=String.Format("{0}-{1} {2}-{3}",
                  GUILocalizeStrings.Get(657),//657=Mon
                  GUILocalizeStrings.Get(661),//661=Fri
                  rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
                  rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
                strType=GUILocalizeStrings.Get(648) ;
                item.Label2=String.Format("{0} {1}",strType,strTime);
                break;

              case TVRecording.RecordingType.Weekly:
                string day;
                switch (rec.StartTime.DayOfWeek)
                {
                  case DayOfWeek.Monday :	day = GUILocalizeStrings.Get(11);	break;
                  case DayOfWeek.Tuesday :	day = GUILocalizeStrings.Get(12);	break;
                  case DayOfWeek.Wednesday :	day = GUILocalizeStrings.Get(13);	break;
                  case DayOfWeek.Thursday :	day = GUILocalizeStrings.Get(14);	break;
                  case DayOfWeek.Friday :	day = GUILocalizeStrings.Get(15);	break;
                  case DayOfWeek.Saturday :	day = GUILocalizeStrings.Get(16);	break;
                  default:	day = GUILocalizeStrings.Get(17);	break;
                }

                strTime=String.Format("{0}-{1}", 
                                      rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
                                      rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
                strType=GUILocalizeStrings.Get(649) ;
                item.Label2=String.Format("{0} {1} {2}",strType,day,strTime);
                break;
              case TVRecording.RecordingType.EveryTimeOnThisChannel:
                item.Label=rec.Title;
                item.Label2=GUILocalizeStrings.Get(650);
                break;
              case TVRecording.RecordingType.EveryTimeOnEveryChannel:
                item.Label=rec.Title;
                item.Label2=GUILocalizeStrings.Get(651);
                break;
            }
            break;
        }
      }
    }

    void OnClick(int iItem)
    {
      GUIListItem item = GetItem(iItem);
      if (item==null) return;
      TVRecording rec=(TVRecording)item.TVTag;

      GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg==null) return;
      
      dlg.Reset();
      dlg.SetHeading(rec.Title);
			
      for (int i=618; i <= 619; ++i)
      {
        dlg.Add( GUILocalizeStrings.Get(i));
      }
      dlg.Add( GUILocalizeStrings.Get(626));
      dlg.DoModal( GetID);
      if (dlg.SelectedLabel==-1) return;
      switch (dlg.SelectedLabel)
      {
        case 0: // delete
        {
          int card;
          if (Recorder.IsRecordingSchedule(rec, out card))
          {
            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null != dlgYesNo)
            {
              dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));
              dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730));
              dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731));
              dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732));
              dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

              Recorder.StopRecording(card);
            }
          }
          TVDatabase.RemoveRecording(rec);
          LoadDirectory();
        }
        break;
        
        case 1: // edit date/time
          OnEdit(rec);
        break;

        case 2: // edit Type
          ChangeType(rec);
        break;
      }
    }
    
    void ChangeType(TVRecording rec)
    {
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(616));//616=Select Recording type
        //611=Record once
        //612=Record everytime on this channel
        //613=Record everytime on every channel
        //614=Record every week at this time
        //615=Record every day at this time
				for (int i=611; i <= 615; ++i)
				{
					dlg.Add( GUILocalizeStrings.Get(i));
        }
        dlg.Add( GUILocalizeStrings.Get(672));// 672=Record Mon-Fri
        switch (rec.RecType)
        {
          case TVRecording.RecordingType.Once:
            dlg.SelectedLabel=0;
          break;
          case TVRecording.RecordingType.EveryTimeOnThisChannel:
            dlg.SelectedLabel=1;
            break;
          case TVRecording.RecordingType.EveryTimeOnEveryChannel:
            dlg.SelectedLabel=2;
            break;
          case TVRecording.RecordingType.Weekly:
            dlg.SelectedLabel=3;
            break;
          case TVRecording.RecordingType.Daily:
            dlg.SelectedLabel=4;
            break;
          case TVRecording.RecordingType.WeekDays:
            dlg.SelectedLabel=5;
            break;
        }
				dlg.DoModal( GetID);
				if (dlg.SelectedLabel==-1) return;
				switch (dlg.SelectedLabel)
				{
					case 0://once
						rec.RecType=TVRecording.RecordingType.Once;
            rec.Canceled=0;
            break;
					case 1://everytime, this channel
						rec.RecType=TVRecording.RecordingType.EveryTimeOnThisChannel;
            rec.Canceled=0;
            break;
					case 2://everytime, all channels
						rec.RecType=TVRecording.RecordingType.EveryTimeOnEveryChannel;
            rec.Canceled=0;
            break;
					case 3://weekly
						rec.RecType=TVRecording.RecordingType.Weekly;
            rec.Canceled=0;
            break;
					case 4://daily
						rec.RecType=TVRecording.RecordingType.Daily;
            rec.Canceled=0;
            break;
          case 5://Mo-Fi
            rec.RecType=TVRecording.RecordingType.WeekDays;
            rec.Canceled=0;
            break;
				}
				TVDatabase.ChangeRecording(ref rec);
        LoadDirectory();

			}
		}

    void OnNewShedule()
    {
      GUIDialogDateTime dlg = (GUIDialogDateTime)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_DATETIME);
      if (dlg!=null)
      {
        ArrayList channels = new ArrayList();
        TVDatabase.GetChannels(ref channels);
        dlg.SetHeading(638);
        dlg.Items.Clear();
        foreach (TVChannel chan in channels)
        {
          dlg.Items.Add(chan.Name);
        }
        dlg.StartDateTime=DateTime.Now;
        dlg.EndDateTime=DateTime.Now.AddHours(2);
        dlg.DoModal(GetID);
        if (dlg.IsConfirmed)
        {
          TVRecording rec = new TVRecording();
          rec.Channel=dlg.Channel;
          rec.Start=Utils.datetolong(dlg.StartDateTime);
          rec.End=Utils.datetolong(dlg.EndDateTime);
          rec.RecType = TVRecording.RecordingType.Once;
          rec.Title=GUILocalizeStrings.Get(413);
          TVDatabase.AddRecording(ref rec);
          LoadDirectory();
        }
      }
    }

    void OnEdit(TVRecording rec)
    {
      GUIDialogDateTime dlg = (GUIDialogDateTime)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_DATETIME);
      if (dlg!=null)
      {
        int card;
        ArrayList channels = new ArrayList();
        TVDatabase.GetChannels(ref channels);
        dlg.SetHeading(637);
        dlg.Items.Clear();
        dlg.EnableChannel=true;
        dlg.EnableStartTime=true;
        if (Recorder.IsRecordingSchedule(rec, out card))
        {
          dlg.EnableChannel=false;
          dlg.EnableStartTime=false;
        }
        foreach (TVChannel chan in channels)
        {
          dlg.Items.Add(chan.Name);
        }
        dlg.Channel=rec.Channel;
        dlg.StartDateTime=rec.StartTime;
        dlg.EndDateTime=rec.EndTime;
        dlg.DoModal(GetID);
        if (dlg.IsConfirmed)
        {
          rec.Channel=dlg.Channel;
          rec.Start=Utils.datetolong(dlg.StartDateTime);
          rec.End=Utils.datetolong(dlg.EndDateTime);
          rec.Canceled=0;
          TVDatabase.ChangeRecording(ref rec);
          LoadDirectory();
        }
      }
    }

    string GetRecType(TVRecording.RecordingType recType)
    {
      string strType="";
      switch(recType)
      {
        case TVRecording.RecordingType.Daily:
          strType=GUILocalizeStrings.Get(648);//daily
          break;
        case TVRecording.RecordingType.EveryTimeOnEveryChannel:
          strType=GUILocalizeStrings.Get(651);//Everytime on any channel
          break;
        case TVRecording.RecordingType.EveryTimeOnThisChannel:
          strType=GUILocalizeStrings.Get(650);//Everytime on this channel
          break;
        case TVRecording.RecordingType.Once:
          strType=GUILocalizeStrings.Get(647);//Once
          break;
        case TVRecording.RecordingType.WeekDays:
          strType=GUILocalizeStrings.Get(680);//Mon-Fri
          break;
        case TVRecording.RecordingType.Weekly:
          strType=GUILocalizeStrings.Get(679);//Weekly
          break;
      }
      return strType;
    }

    void OnCleanup()
    {
      int iCleaned=0;
      ArrayList itemlist = new ArrayList();
      TVDatabase.GetRecordings(ref itemlist);
      foreach (TVRecording rec in itemlist)
      {
        if (rec.IsDone() || rec.Canceled!=0)
        {
          iCleaned++;
          TVDatabase.RemoveRecording(rec);
        }
      }
      GUIDialogOK pDlgOK			 = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      LoadDirectory();
      if (pDlgOK!=null)
      {
        pDlgOK.SetHeading(624);
        pDlgOK.SetLine(1,String.Format("{0}{1}", GUILocalizeStrings.Get(625),iCleaned));
        pDlgOK.SetLine(2,"");
        pDlgOK.DoModal(GetID);
      }
    }
  }
}
