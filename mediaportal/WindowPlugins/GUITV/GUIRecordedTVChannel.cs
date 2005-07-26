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
			CONTROL_BTNSORTBY=3,
      CONTROL_BTNVIEW=5,
      CONTROL_BTNCLEANUP=6,
			CONTROL_ALBUM=10,
      CONTROL_LIST=11,
    };

    enum SortMethod
    {
      Channel=0,
      Date=1,
      Name=2,
      Genre=3,
      Played=4,
    }

    SortMethod        currentSortMethod=SortMethod.Date;
    bool              m_bSortAscending=true;
    bool              showRoot=true;
    string            currentChannel=String.Empty;
		[SkinControlAttribute(3)]				protected GUISortButtonControl btnSortBy=null;
		[SkinControlAttribute(5)]				protected GUIButtonControl btnView=null;
		[SkinControlAttribute(6)]				protected GUIButtonControl btnCleanup=null;

		[SkinControlAttribute(10)]			protected GUIListControl listAlbums=null;
		[SkinControlAttribute(11)]			protected GUIListControl listViews=null;
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
      using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        string strTmp=String.Empty;
        strTmp=(string)xmlreader.GetValue("tvrecordedchannel","sort");
        if (strTmp!=null)
        {
          if (strTmp=="channel") currentSortMethod=SortMethod.Channel;
          else if (strTmp=="date") currentSortMethod=SortMethod.Date;
          else if (strTmp=="name") currentSortMethod=SortMethod.Name;
          else if (strTmp=="type") currentSortMethod=SortMethod.Genre;
          else if (strTmp=="played") currentSortMethod=SortMethod.Played;
        }
        m_bSortAscending=xmlreader.GetValueAsBool("tvrecordedchannel","sortascending",true);
      }
    }

    void SaveSettings()
    {
      using(MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            xmlwriter.SetValue("tvrecordedchannel","sort","channel");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue("tvrecordedchannel","sort","date");
            break;
          case SortMethod.Name:
            xmlwriter.SetValue("tvrecordedchannel","sort","name");
            break;
          case SortMethod.Genre:
            xmlwriter.SetValue("tvrecordedchannel","sort","type");
            break;
          case SortMethod.Played:
            xmlwriter.SetValue("tvrecordedchannel","sort","played");
            break;
        }
        xmlwriter.SetValueAsBool("tvrecordedchannel","sortascending",m_bSortAscending);
      }
    }
    #endregion


    public override void OnAction(Action action)
    {
			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				if (listAlbums.Focus || listViews.Focus)
				{
					GUIListItem item = GetItem(0);
					if (item != null)
					{
						if (item.IsFolder && item.Label == "..")
						{
							showRoot=true;
							LoadDirectory();
							return;
						}
					}
				}
			}
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
        {
          GUIWindowManager.ShowPreviousWindow();
          return;
        }
				case Action.ActionType.ACTION_SHOW_GUI:
					if ( !g_Player.Playing && Recorder.IsViewing())
					{
						//if we're watching tv
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					else if (g_Player.Playing  && g_Player.IsTV && !g_Player.IsTVRecording)
					{
						//if we're watching a tv recording
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					else if (g_Player.Playing&&g_Player.HasVideo)
					{
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
					}
          break;

        case Action.ActionType.ACTION_DELETE_ITEM:  
        {
          int item=GetSelectedItemNo();
          if (item>=0)
            OnDeleteItem( item);
        }
          break;
      }
			if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
			{
				ShowContextMenu();
			}
      base.OnAction(action);
      Update();
    }

		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
          
			SaveSettings();
			if ( !GUITVHome.IsTVWindow(newWindowId) )
			{
				if (Recorder.IsViewing() && ! (Recorder.IsTimeShifting()||Recorder.IsRecording()) )
				{
					if (GUIGraphicsContext.ShowBackground)
					{
						// stop timeshifting & viewing... 
	              
						Recorder.StopViewing();
					}
				}
			}
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadSettings();
			LoadDirectory();
			GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int)GUIWindow.Window.WINDOW_TV,GetID,0,0,0,null);
			msg.SendToTargetWindow=true;
			GUIWindowManager.SendThreadMessage(msg);

			btnSortBy.SortChanged += new SortEventHandler(SortChanged);			

			return ;
		}

		void ShowViews()
		{
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(652); // my recorded tv
			dlg.AddLocalizedString( 914);
			dlg.AddLocalizedString( 135);
			dlg.AddLocalizedString( 915);
			dlg.DoModal( GetID);
			if (dlg.SelectedLabel==-1) return;
			int nNewWindow=GetID;
			switch (dlg.SelectedId)
			{
				case 914 : //	all
					nNewWindow = (int)GUIWindow.Window.WINDOW_RECORDEDTV;
					break;
				case 135 : //	genres
					nNewWindow = (int)GUIWindow.Window.WINDOW_RECORDEDTVGENRE;
					break;
				case 915 : //	channel
					nNewWindow = (int)GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL;
					break;
			}
			if (nNewWindow != GetID)
			{
				GUIWindowManager.ReplaceWindow(nNewWindow);
			}
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnSortBy)
			{
				switch (currentSortMethod)
				{
					case SortMethod.Channel:
						currentSortMethod=SortMethod.Date;
						break;
					case SortMethod.Date:
						currentSortMethod=SortMethod.Name;
						break;
					case SortMethod.Name:
						currentSortMethod=SortMethod.Genre;
						break;
					case SortMethod.Genre:
						currentSortMethod=SortMethod.Played;
						break;
					case SortMethod.Played:
						currentSortMethod=SortMethod.Channel;
						break;
				}
				OnSort();
			}

			if (control==btnView)
			{
				ShowViews();
				return;
			}
			if (control==btnCleanup)
			{
				OnDeleteWatchedRecordings();
			}
			if (control==listAlbums || control==listViews)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,control.GetID,0,0,null);
				OnMessage(msg);         
				int iItem=(int)msg.Param1;
				GUIListItem item=GetSelectedItem();
				if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
				{
					if (showRoot)
					{
						currentChannel=item.Label;
						showRoot=false;
						LoadDirectory();
					}
					else
					{
						if (item.Label=="..")
						{
							showRoot=true;
							LoadDirectory();
						}
						else
						{
							OnPlay(iItem);
						}
					}
				}
				
				if (actionType == Action.ActionType.ACTION_SHOW_INFO) 
				{
					OnShowContextMenu();
				}
			}
		}

    void LoadDirectory()
    {
      int iControl=listAlbums.GetID;
      if (showRoot)
        iControl=listViews.GetID;
      GUIControl.ShowControl(GetID,iControl);
      GUIControl.FocusControl(GetID,iControl);
      
      GUIControl.ClearControl(GetID,listAlbums.GetID);
      GUIControl.ClearControl(GetID,listViews.GetID);
      
      int objects=0;
      ArrayList itemlist = new ArrayList();
      TVDatabase.GetRecordedTV(ref itemlist);
      if (showRoot)
      {
        listAlbums.IsVisible=false;
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
            string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,rec.Channel);
            if (!System.IO.File.Exists(strLogo))
            {
              strLogo="defaultVideoBig.png";
            }
            item.ThumbnailImage=strLogo;
            item.IconImageBig=strLogo;
            item.IconImage=strLogo;
            listAlbums.Add(item);
            listViews.Add(item);
          }
        }
      }
      else
      {
        listViews.IsVisible=false;
        GUIListItem item=new GUIListItem();
        item.IsFolder=true;
        item.Label="..";
        Utils.SetDefaultIcons(item);
        item.IconImage=item.IconImageBig;
        listAlbums.Add(item);
        listViews.Add(item);
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
            string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,rec.Channel);
            if (!System.IO.File.Exists(strLogo))
            {
              strLogo="defaultVideoBig.png";
            }
            item.ThumbnailImage=strLogo;
            item.IconImageBig=strLogo;
            item.IconImage=strLogo;
            listAlbums.Add(item);
            listViews.Add(item);
          }
        }
      }
      
      string strObjects=String.Format("{0} {1}", objects, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount",strObjects.ToString());
			GUIControl cntlLabel = GetControl(12);

			if (listAlbums.IsVisible)
				cntlLabel.YPosition = listAlbums.SpinY;
			else
				cntlLabel.YPosition = listViews.SpinY;

      OnSort();
      UpdateButtons();
      Update();
    }


    void UpdateButtons()
    {
      string strLine=String.Empty;
      switch (currentSortMethod)
      {
        case SortMethod.Channel:
          strLine=GUILocalizeStrings.Get(620);//Sort by: Channel
          break;
        case SortMethod.Date:
          strLine=GUILocalizeStrings.Get(621);//Sort by: Date
          break;
        case SortMethod.Name:
          strLine=GUILocalizeStrings.Get(268);//Sort by: Title
          break;
        case SortMethod.Genre:
          strLine=GUILocalizeStrings.Get(678);//Sort by: Genre
          break;
        case SortMethod.Played:
          strLine=GUILocalizeStrings.Get(671);//Sort by: Watched
          break;
      }

      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTNSORTBY,strLine);

		btnSortBy.IsAscending = m_bSortAscending;
    }
    GUIListItem GetSelectedItem()
    {
      int iControl;
      iControl=listAlbums.GetID;
      if (showRoot)
        iControl=listViews.GetID;
      GUIListItem item = GUIControl.GetSelectedListItem(GetID,iControl);
      return item;
    }

    GUIListItem GetItem(int iItem)
		{
				if (showRoot) 
		 {
			 if (iItem<0 || iItem>=listViews.Count) return null;
			 return listViews[iItem];
		 }
		 else 
		 {
			 if (iItem<0 || iItem>=listAlbums.Count) return null;
			 return listAlbums[iItem];
		 }
    }

    int GetSelectedItemNo()
    {
      int iControl;
      iControl=listAlbums.GetID;
      if (showRoot)
        iControl=listViews.GetID;

      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
      OnMessage(msg);         
      int iItem=(int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      if (showRoot)
        return listViews.Count;
			else 
				return listAlbums.Count;
    }


    #region Sort Members
    void OnSort()
    {
      SetLabels();
      listAlbums.Sort(this);
			listViews.Sort(this);
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
        case SortMethod.Played:
          item1.Label2=String.Format("{0} {1}",rec1.Played, GUILocalizeStrings.Get(677));//times
          item2.Label2=String.Format("{0} {1}",rec2.Played, GUILocalizeStrings.Get(677));//times
          if (rec1.Played==rec2.Played) goto case SortMethod.Name;
          else
          {
            if (m_bSortAscending) return rec1.Played-rec2.Played;
            else return rec2.Played-rec1.Played;
          }

        case SortMethod.Name:
          if (m_bSortAscending)
          {
            iComp=String.Compare(rec1.Title,rec2.Title,true);
            if (iComp==0) goto case SortMethod.Channel;
            else return iComp;
          }
          else
          {
            iComp=String.Compare(rec2.Title ,rec1.Title,true);
            if (iComp==0) goto case SortMethod.Channel;
            else return iComp;
          }
        

        case SortMethod.Channel:
          if (m_bSortAscending)
          {
            iComp=String.Compare(rec1.Channel,rec2.Channel,true);
            if (iComp==0) goto case SortMethod.Date;
            else return iComp;
          }
          else
          {
            iComp=String.Compare(rec2.Channel,rec1.Channel,true);
            if (iComp==0) goto case SortMethod.Date;
            else return iComp;
          }

        case SortMethod.Date:
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

        case SortMethod.Genre:
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

    void ShowContextMenu()
		{
			int iItem=GetSelectedItemNo();
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
				Log.Write("TVRecording:play:{0}",rec.FileName);
				g_Player.Stop();
				Recorder.StopViewing();
        
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
            if (movieDetails.Title!=String.Empty) title=movieDetails.Title;
          
            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo) return false;
						dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
						dlgYesNo.SetLine(1, rec.Channel);
						dlgYesNo.SetLine(2, title);
						dlgYesNo.SetLine(3, GUILocalizeStrings.Get(936)+Utils.SecondsToHMSString(stoptime) );
            dlgYesNo.SetDefaultToYes(true);
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
            
            if (!dlgYesNo.IsConfirmed) stoptime=0;
          }
        }
				Log.Write("GUIRecordedTV Play:{0}",rec.FileName);
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
			dlgYesNo.SetLine(3, String.Empty);
			dlgYesNo.SetDefaultToYes(true);
      dlgYesNo.DoModal(GetID);

			if (!dlgYesNo.IsConfirmed) return;
			TVDatabase.RemoveRecordedTV(rec);
			VideoDatabase.DeleteMovieInfo(rec.FileName);
			VideoDatabase.DeleteMovie(rec.FileName);
      DiskManagement.DeleteRecording(rec.FileName);
      LoadDirectory();
    }

    void OnDeleteWatchedRecordings()
    {
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (null==dlgYesNo) return;
      dlgYesNo.SetHeading(GUILocalizeStrings.Get(676));//delete watched recordings?
      dlgYesNo.SetLine(1, String.Empty);
      dlgYesNo.SetLine(2, String.Empty);
			dlgYesNo.SetLine(3, String.Empty);
			dlgYesNo.SetDefaultToYes(true);
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed) return;
      ArrayList itemlist = new ArrayList();
      TVDatabase.GetRecordedTV(ref itemlist);
      foreach (TVRecorded rec in itemlist)
      {
        if (rec.Played>0)
        {
					DiskManagement.DeleteRecording(rec.FileName);
					TVDatabase.RemoveRecordedTV(rec);
					VideoDatabase.DeleteMovieInfo(rec.FileName);
					VideoDatabase.DeleteMovie(rec.FileName);
        }
				else if (!System.IO.File.Exists(rec.FileName))
				{
					TVDatabase.RemoveRecordedTV(rec);
					VideoDatabase.DeleteMovieInfo(rec.FileName);
					VideoDatabase.DeleteMovie(rec.FileName);
				}

      }

      LoadDirectory();
    }

    void Update()
    {
      GUIListItem pItem=GetItem( GetSelectedItemNo() );
      if (pItem==null)
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Title",String.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre",String.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Time",String.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Description",String.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb",String.Empty);
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
        
        string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,rec.Channel);
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
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Title",String.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre",String.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Time",String.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Description",String.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb","defaultVideoBig.png");
      }

    
    }

	  void SortChanged(object sender, SortEventArgs e)
	  {
		  m_bSortAscending = e.Order != System.Windows.Forms.SortOrder.Descending;
		  OnSort();
	  }
  }
}
