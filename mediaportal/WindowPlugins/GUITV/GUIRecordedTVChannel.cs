using System;
using System.Collections;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Player;
using MediaPortal.Video.Database;
namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIRecordedTVChannel :GUIWindow, IComparer
  {
    enum Controls
    {
      CONTROL_BTNSORTBY=2,
      CONTROL_BTNSORTASC=4,
      CONTROL_BTNVIEW=5,
      CONTROL_BTNCLEANUP=6,
      CONTROL_LIST_ALBUM=10,
      CONTROL_LIST=11,
    };

    enum SortMethod
    {
      SORT_CHANNEL=0,
      SORT_DATE=1,
      SORT_NAME=2,
      SORT_GENRE=3,
      SORT_PLAYED=4,
    }

    SortMethod        currentSortMethod=SortMethod.SORT_DATE;
    bool              m_bSortAscending=true;
    bool              showRoot=true;
    string            currentChannel=String.Empty;

    public  GUIRecordedTVChannel()
    {
      GetID=(int)GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL;
    }
    ~GUIRecordedTVChannel()
    {
    }
    
    public override bool Init()
    {

      bool bResult=Load (GUIGraphicsContext.Skin+@"\mytvrecordedtvchannel.xml");
      LoadSettings();
      return bResult;
    }



    #region Serialisation
    void LoadSettings()
    {
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        string strTmp="";
        strTmp=(string)xmlreader.GetValue("tvrecordedchannel","sort");
        if (strTmp!=null)
        {
          if (strTmp=="channel") currentSortMethod=SortMethod.SORT_CHANNEL;
          else if (strTmp=="date") currentSortMethod=SortMethod.SORT_DATE;
          else if (strTmp=="name") currentSortMethod=SortMethod.SORT_NAME;
          else if (strTmp=="type") currentSortMethod=SortMethod.SORT_GENRE;
          else if (strTmp=="played") currentSortMethod=SortMethod.SORT_PLAYED;
        }
        m_bSortAscending=xmlreader.GetValueAsBool("tvrecordedchannel","sortascending",true);
      }
    }

    void SaveSettings()
    {
      using(AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        switch (currentSortMethod)
        {
          case SortMethod.SORT_CHANNEL:
            xmlwriter.SetValue("tvrecordedchannel","sort","channel");
            break;
          case SortMethod.SORT_DATE:
            xmlwriter.SetValue("tvrecordedchannel","sort","date");
            break;
          case SortMethod.SORT_NAME:
            xmlwriter.SetValue("tvrecordedchannel","sort","name");
            break;
          case SortMethod.SORT_GENRE:
            xmlwriter.SetValue("tvrecordedchannel","sort","type");
            break;
          case SortMethod.SORT_PLAYED:
            xmlwriter.SetValue("tvrecordedchannel","sort","played");
            break;
        }
        xmlwriter.SetValueAsBool("tvrecordedchannel","sortascending",m_bSortAscending);
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
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
          break;

        case Action.ActionType.ACTION_DELETE_ITEM:  
        {
          int item=GetSelectedItemNo();
          if (item>=0)
            OnDeleteItem( item);
        }
          break;
      }
      base.OnAction(action);
      Update();
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          
          SaveSettings();
					if ( !GUITVHome.IsTVWindow(message.Param1) )
					{
						if (! g_Player.Playing)
						{
							if (GUIGraphicsContext.ShowBackground)
							{
								// stop timeshifting & viewing... 
	              
								Recorder.StopViewing();
							}
						}
					}

        }
          break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
          base.OnMessage(message);
					
          LoadSettings();
          LoadDirectory();
          GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int)GUIWindow.Window.WINDOW_TV,GetID,0,0,0,null);
          msg.SendToTargetWindow=true;
          GUIWindowManager.SendThreadMessage(msg);
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
                currentSortMethod=SortMethod.SORT_GENRE;
                break;
              case SortMethod.SORT_GENRE:
                currentSortMethod=SortMethod.SORT_PLAYED;
                break;
              case SortMethod.SORT_PLAYED:
                currentSortMethod=SortMethod.SORT_CHANNEL;
                break;
            }
            OnSort();
          }
          
          if (iControl==(int)Controls.CONTROL_BTNVIEW)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
            OnMessage(msg);
            int nSelected = (int)msg.Param1;
            int nNewWindow = (int)GUIWindow.Window.WINDOW_RECORDEDTV;
            switch (nSelected)
            {
              case 0 : //	all
                nNewWindow = (int)GUIWindow.Window.WINDOW_RECORDEDTV;
                break;
              case 1 : //	genre
                nNewWindow = (int)GUIWindow.Window.WINDOW_RECORDEDTVGENRE;
                break;
              case 2 : //	channel
                nNewWindow = (int)GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL;
                break;
            }

            if (nNewWindow != GetID)
            {
              GUIWindowManager.ActivateWindow(nNewWindow);
              return true;
            }
          }
          if (iControl==(int)Controls.CONTROL_BTNCLEANUP)
          {
            OnDeleteWatchedRecordings();
          }
          if (iControl==(int)Controls.CONTROL_LIST_ALBUM || iControl==(int)Controls.CONTROL_LIST)
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
          break;

      }
      return base.OnMessage(message);
    }

    void LoadDirectory()
    {
      int iControl=(int)Controls.CONTROL_LIST_ALBUM;
      if (showRoot)
        iControl=(int)Controls.CONTROL_LIST;
      GUIControl.ShowControl(GetID,iControl);
      GUIControl.FocusControl(GetID,iControl);
      
      GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_ALBUM);
      GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST);
      
      int objects=0;
      ArrayList itemlist = new ArrayList();
      TVDatabase.GetRecordedTV(ref itemlist);
      if (showRoot)
      {
        GUIControl.HideControl(GetID,(int)Controls.CONTROL_LIST_ALBUM);
        ArrayList channels = new ArrayList();
        foreach (TVRecorded rec in itemlist)
        {
          bool add=true;
          string channel=rec.Channel;
          if (rec.Channel.Length<2) rec.Channel=GUILocalizeStrings.Get(2014);//unknown
          for (int i=0; i < channels.Count;++i)
          {
            string tmpchannel=(string)channels[i];
            if (tmpchannel==channel) 
            {
              add=false;
              break;
            }
          }
          if (add)
          {
            channels.Add(rec.Channel);
            objects++;
            GUIListItem item=new GUIListItem();
            item.Label=rec.Channel;
            string strLogo=Utils.GetCoverArt(GUITVHome.TVChannelCovertArt,rec.Channel);
            if (!System.IO.File.Exists(strLogo))
            {
              strLogo="defaultVideoBig.png";
            }
            item.ThumbnailImage=strLogo;
            item.IconImageBig=strLogo;
            item.IconImage=strLogo;
            GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST_ALBUM,item);
            GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
          }
        }
      }
      else
      {
        GUIControl.HideControl(GetID,(int)Controls.CONTROL_LIST);
        GUIListItem item=new GUIListItem();
        item.IsFolder=true;
        item.Label="..";
        Utils.SetDefaultIcons(item);
        item.IconImage=item.IconImageBig;
        GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST_ALBUM,item);
        GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
        objects++;
        foreach (TVRecorded rec in itemlist)
        {
          if (rec.Channel.Length<2) rec.Channel=GUILocalizeStrings.Get(2014);//unknown
          if (rec.Channel == currentChannel)
          {
            objects++;
            item=new GUIListItem();
            item.Label=rec.Title;
            item.TVTag=rec;
            string strLogo=Utils.GetCoverArt(GUITVHome.TVChannelCovertArt,rec.Channel);
            if (!System.IO.File.Exists(strLogo))
            {
              strLogo="defaultVideoBig.png";
            }
            AddRecordingToDatabase(rec);
            item.ThumbnailImage=strLogo;
            item.IconImageBig=strLogo;
            item.IconImage=strLogo;
            GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST_ALBUM,item);
            GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
          }
        }
      }
      
      string strObjects=String.Format("{0} {1}", objects, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount",objects.ToString());

      OnSort();
      UpdateButtons();
      Update();
    }

    void AddRecordingToDatabase(TVRecorded rec)
    {
      VideoDatabase.AddMovieFile(rec.FileName);
      IMDBMovie movieDetails = new IMDBMovie();
      int movieid=VideoDatabase.GetMovieInfo(rec.FileName, ref movieDetails);
      movieDetails.Title=rec.Title;
      movieDetails.Genre=rec.Genre;
      movieDetails.Plot=rec.Description;
      movieDetails.Year=rec.StartTime.Year;
      VideoDatabase.SetMovieInfoById(movieid, ref movieDetails);
    }

    void UpdateButtons()
    {
      string strLine="";
      switch (currentSortMethod)
      {
        case SortMethod.SORT_CHANNEL:
          strLine=GUILocalizeStrings.Get(620);//Sort by: Channel
          break;
        case SortMethod.SORT_DATE:
          strLine=GUILocalizeStrings.Get(621);//Sort by: Date
          break;
        case SortMethod.SORT_NAME:
          strLine=GUILocalizeStrings.Get(268);//Sort by: Title
          break;
        case SortMethod.SORT_GENRE:
          strLine=GUILocalizeStrings.Get(678);//Sort by: Genre
          break;
        case SortMethod.SORT_PLAYED:
          strLine=GUILocalizeStrings.Get(671);//Sort by: Watched
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
      iControl=(int)Controls.CONTROL_LIST_ALBUM;
      if (showRoot)
        iControl=(int)Controls.CONTROL_LIST;
      GUIListItem item = GUIControl.GetSelectedListItem(GetID,iControl);
      return item;
    }

    GUIListItem GetItem(int iItem)
    {
      int iControl;
      iControl=(int)Controls.CONTROL_LIST_ALBUM;
      if (showRoot)
        iControl=(int)Controls.CONTROL_LIST;
      GUIListItem item = GUIControl.GetListItem(GetID,iControl,iItem);
      return item;
    }

    int GetSelectedItemNo()
    {
      int iControl;
      iControl=(int)Controls.CONTROL_LIST_ALBUM;
      if (showRoot)
        iControl=(int)Controls.CONTROL_LIST;

      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
      OnMessage(msg);         
      int iItem=(int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      int iControl;
      iControl=(int)Controls.CONTROL_LIST_ALBUM;
      if (showRoot)
        iControl=(int)Controls.CONTROL_LIST;

      return GUIControl.GetItemCount(GetID,iControl);
    }


    #region Sort Members
    void OnSort()
    {
      SetLabels();
      GUIListControl list=(GUIListControl)GetControl((int)Controls.CONTROL_LIST_ALBUM);
      list.Sort(this);
      list=(GUIListControl)GetControl((int)Controls.CONTROL_LIST);
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

      if (item1.Label=="..") return -1;
      if (item2.Label=="..") return 1;
      if (showRoot)
      {
        if (m_bSortAscending)
        {
          return String.Compare(item1.Label,item2.Label,true);
        }
        else
        {
          return String.Compare(item2.Label,item1.Label,true);
        }
      }

      int iComp=0;
      TimeSpan ts;
      TVRecorded rec1=item1.TVTag as TVRecorded;
      TVRecorded rec2=item2.TVTag as TVRecorded;
      switch (currentSortMethod)
      {
        case SortMethod.SORT_PLAYED:
          item1.Label2=String.Format("{0} {1}",rec1.Played, GUILocalizeStrings.Get(677));//times
          item2.Label2=String.Format("{0} {1}",rec2.Played, GUILocalizeStrings.Get(677));//times
          if (rec1.Played==rec2.Played) goto case SortMethod.SORT_NAME;
          else
          {
            if (m_bSortAscending) return rec1.Played-rec2.Played;
            else return rec2.Played-rec1.Played;
          }

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

        case SortMethod.SORT_GENRE:
          item1.Label2=rec1.Genre;
          item2.Label2=rec2.Genre;
          if (rec1.Genre!=rec2.Genre) 
          {
            if (m_bSortAscending)
              return String.Compare(rec1.Genre,rec2.Genre,true);
            else
              return String.Compare(rec2.Genre,rec1.Genre,true);
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
      if (showRoot) return;
      SortMethod method=currentSortMethod;
      bool bAscending=m_bSortAscending;

      for (int i=0; i < GetItemCount();++i)
      {
        GUIListItem item=GetItem(i);
        TVRecorded rec=item.TVTag as TVRecorded;
        if (rec!=null)
        {
          item.Label=rec.Title;
          TimeSpan ts = rec.EndTime-rec.StartTime;
          string strTime=String.Format("{0} {1} ", 
            Utils.GetShortDayString(rec.StartTime) , 
            Utils.SecondsToHMString( (int)ts.TotalSeconds));
          item.Label2=strTime;
          item.Label3=rec.Genre;
        }
      }
    }

    void OnClick(int iItem)
    {
      GUIListItem pItem=GetItem(iItem);
      if (pItem==null) return;
      TVRecorded rec=pItem.TVTag as TVRecorded;

      if (pItem.IsFolder && !showRoot && pItem.Label=="..")
      {
        showRoot=true;
        LoadDirectory();
        return;
      }
      if (pItem.IsFolder) return;

      if (showRoot)
      {
        currentChannel=pItem.Label;
        showRoot=false;
        LoadDirectory();
        return;
      }

      GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg==null) return;
      dlg.Reset();
      dlg.SetHeading(rec.Title);
			
      for (int i=655; i <= 656; ++i)
      {
        dlg.Add( GUILocalizeStrings.Get(i));
			}
      dlg.DoModal( GetID);
      if (dlg.SelectedLabel==-1) return;
      switch (dlg.SelectedLabel)
      {
        case 1: // delete
        {
          OnDeleteItem(iItem);
          LoadDirectory();
        }
          break;

        case 0: // play
        {
          if ( OnPlay(iItem))
            return;
        }
          break;
      }
    }

    bool OnPlay(int iItem)
    {
      GUIListItem pItem=GetItem(iItem);
      if (pItem==null) return false;
      if (pItem.IsFolder) return false;

      TVRecorded rec=(TVRecorded)pItem.TVTag;
      if (System.IO.File.Exists(rec.FileName))
      {
        g_Player.Stop();
        
        rec.Played++;
        TVDatabase.PlayedRecordedTV(rec);
        IMDBMovie movieDetails = new IMDBMovie();
        int movieid=VideoDatabase.GetMovieInfo(rec.FileName, ref movieDetails);
        int stoptime=0;
        if (movieid >=0)
        {
          stoptime=VideoDatabase.GetMovieStopTime(movieid);
          if (stoptime>0)
          {
            string title=System.IO.Path.GetFileName(rec.FileName);
            VideoDatabase.GetMovieInfoById( movieid, ref movieDetails);
            if (movieDetails.Title!=String.Empty) title=movieDetails.Title;
          
            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo) return false;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
						dlgYesNo.SetLine(1, title);
						dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936)+Utils.SecondsToHMSString(stoptime) );
            dlgYesNo.SetDefaultToYes(true);
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
            
            if (!dlgYesNo.IsConfirmed) stoptime=0;
          }
        }
        if ( g_Player.Play(rec.FileName))
        {
          if (Utils.IsVideo(rec.FileName))
          {
            GUIGraphicsContext.IsFullScreenVideo=true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
          }
          if (stoptime>0)
          {
            g_Player.SeekAbsolute(stoptime);
          }
          return true;
        }
      }
      return false;
    }

    void OnDeleteItem(int iItem)
    {
      GUIListItem pItem=GetItem(iItem);
      if (pItem==null) return;
      if (pItem.IsFolder) return;
      TVRecorded rec=(TVRecorded)pItem.TVTag;
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (null==dlgYesNo) return;
      dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));
			dlgYesNo.SetLine(1, rec.Channel);
			dlgYesNo.SetLine(2, rec.Title);

      dlgYesNo.SetLine(3, "");
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed) return;
      TVDatabase.RemoveRecordedTV(rec);
      DeleteRecording(rec.FileName);
      LoadDirectory();
    }

    void OnDeleteWatchedRecordings()
    {
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (null==dlgYesNo) return;
      dlgYesNo.SetHeading(GUILocalizeStrings.Get(676));//delete watched recordings?
      dlgYesNo.SetLine(1, "");
      dlgYesNo.SetLine(2, "");
      dlgYesNo.SetLine(3, "");
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed) return;
      ArrayList itemlist = new ArrayList();
      TVDatabase.GetRecordedTV(ref itemlist);
      foreach (TVRecorded rec in itemlist)
      {
        if (rec.Played>0)
        {
          DeleteRecording(rec.FileName);
          TVDatabase.RemoveRecordedTV(rec);
        }
      }

      LoadDirectory();
    }

    void Update()
    {
      GUIListItem pItem=GetItem( GetSelectedItemNo() );
      if (pItem==null)
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Title","");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre","");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Time","");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Description","");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb","");
        return;
      }
      TVRecorded rec=pItem.TVTag as TVRecorded;
      if (rec!=null)
      {
        string strTime=String.Format("{0} {1} - {2}", 
          Utils.GetShortDayString(rec.StartTime) , 
          rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
          rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

        GUIPropertyManager.SetProperty("#TV.RecordedTV.Title",rec.Title);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre",rec.Genre);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Time",strTime);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Description",rec.Description);
        
        string strLogo=Utils.GetCoverArt(GUITVHome.TVChannelCovertArt,rec.Channel);
        if (System.IO.File.Exists(strLogo))
        {
          GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb",strLogo);
        }
        else
        {
          GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb","defaultVideoBig.png");
        }
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Title","");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre","");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Time","");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Description","");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb","defaultVideoBig.png");
      }

    
    }
    void DeleteRecording(string strFilename)
		{
			try
			{
				string strFName=System.IO.Path.GetFileNameWithoutExtension(strFilename);
				string strDir=System.IO.Path.GetDirectoryName(strFilename);
				string[] strFiles=System.IO.Directory.GetFiles(strDir,strFName+"*.*");
				if (strFiles==null) return;
				for (int i=0; i < strFiles.Length; ++i)
				{
					Utils.FileDelete(strFiles[i]);
				}
			}
			catch(Exception)
			{}
    }
  }
}
