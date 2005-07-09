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
using Toub.MediaCenter.Dvrms.Metadata;
namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIRecordedTV :GUIWindow, IComparer
  {
		#region variables
    enum Controls
    { 
			LABEL_PROGRAMTITLE=13,
			LABEL_PROGRAMTIME=14,
			LABEL_PROGRAMDESCRIPTION=15,
			LABEL_PROGRAMGENRE=17,
    };

    enum SortMethod
    {
      Channel=0,
      Date=1,
      Name=2,
      Genre=3,
      Played=4,
			Duration=5
    }
		enum ViewAs
		{
			List,
			Album
		}

		ViewAs						currentViewMethod=ViewAs.Album;
    SortMethod        currentSortMethod=SortMethod.Date;
    bool              m_bSortAscending=true;
		bool							m_bDeleteWatchedShow=false;
		int								m_iSelectedItem=0;
		string            currentShow=String.Empty;
		
		[SkinControlAttribute(2)]			  protected GUIButtonControl btnViewAs=null;
		[SkinControlAttribute(3)]				protected GUIButtonControl btnSortBy=null;
		[SkinControlAttribute(4)]				protected GUIToggleButtonControl btnSortAsc=null;
		[SkinControlAttribute(5)]				protected GUIButtonControl btnView=null;
		[SkinControlAttribute(6)]				protected GUIButtonControl btnCleanup=null;

		[SkinControlAttribute(10)]			protected GUIListControl listAlbums=null;
		[SkinControlAttribute(11)]			protected GUIListControl listViews=null;

		#endregion
    public  GUIRecordedTV()
    {
      GetID=(int)GUIWindow.Window.WINDOW_RECORDEDTV;
    }

    #region Serialisation
    void LoadSettings()
    {
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				string strTmp=String.Empty;
				strTmp=(string)xmlreader.GetValue("tvrecorded","sort");
				if (strTmp!=null)
				{
					if (strTmp=="channel") currentSortMethod=SortMethod.Channel;
					else if (strTmp=="date") currentSortMethod=SortMethod.Date;
					else if (strTmp=="name") currentSortMethod=SortMethod.Name;
					else if (strTmp=="type") currentSortMethod=SortMethod.Genre;
					else if (strTmp=="played") currentSortMethod=SortMethod.Played;
					else if (strTmp=="duration") currentSortMethod=SortMethod.Duration;
				}
				strTmp=(string)xmlreader.GetValue("tvrecorded","view");
				if (strTmp!=null)
				{
					if (strTmp=="album") currentViewMethod=ViewAs.Album;
					else if (strTmp=="list") currentViewMethod=ViewAs.List;
				}
				
				m_bSortAscending=xmlreader.GetValueAsBool("tvrecorded","sortascending",true);
				m_bDeleteWatchedShow= xmlreader.GetValueAsBool("capture", "deletewatchedshows", false);
      }
    }

    void SaveSettings()
    {
      using(MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            xmlwriter.SetValue("tvrecorded","sort","channel");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue("tvrecorded","sort","date");
            break;
          case SortMethod.Name:
            xmlwriter.SetValue("tvrecorded","sort","name");
            break;
          case SortMethod.Genre:
            xmlwriter.SetValue("tvrecorded","sort","type");
            break;
          case SortMethod.Played:
            xmlwriter.SetValue("tvrecorded","sort","played");
						break;
					case SortMethod.Duration:
						xmlwriter.SetValue("tvrecorded","sort","duration");
						break;
        }
				switch (currentViewMethod)
				{
					case ViewAs.Album:
						xmlwriter.SetValue("tvrecorded","view","album");
						break;
					case ViewAs.List:
						xmlwriter.SetValue("tvrecorded","view","list");
						break;
				}
        xmlwriter.SetValueAsBool("tvrecorded","sortascending",m_bSortAscending);
      }
    }
    #endregion

		#region overrides
		public override bool Init()
		{
			g_Player.PlayBackStopped +=new MediaPortal.Player.g_Player.StoppedHandler(OnPlayRecordingBackStopped);
			g_Player.PlayBackEnded +=new MediaPortal.Player.g_Player.EndedHandler(OnPlayRecordingBackEnded);
			g_Player.PlayBackStarted +=new MediaPortal.Player.g_Player.StartedHandler(OnPlayRecordingBackStarted);

			bool bResult=Load (GUIGraphicsContext.Skin+@"\mytvrecordedtv.xml");
			LoadSettings();
			return bResult;
		}

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
							currentShow=String.Empty;
							LoadDirectory();
							return;
						}
					}
				}
			}
      switch (action.wID)
      {
				case Action.ActionType.ACTION_SHOW_GUI:
					if ( !g_Player.Playing && Recorder.IsViewing())
					{
						//if we're watching tv
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					else if (g_Player.Playing && g_Player.IsTV && !g_Player.IsTVRecording)
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
            OnDeleteRecording( item);
					UpdateProperties();
				}
        break;
      }
      base.OnAction(action);
    }

		protected override void OnPageDestroy(int newWindowId)
		{
			m_iSelectedItem=GetSelectedItemNo();
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
			base.OnPageDestroy (newWindowId);
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();

					
			DiskManagement.ImportDvrMsFiles();
			LoadSettings();
			LoadDirectory();

			GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int)GUIWindow.Window.WINDOW_TV,GetID,0,0,0,null);
			msg.SendToTargetWindow=true;
			GUIWindowManager.SendThreadMessage(msg);
					
			while (m_iSelectedItem>=GetItemCount() && m_iSelectedItem>0) m_iSelectedItem--;
			GUIControl.SelectItemControl(GetID,listViews.GetID,m_iSelectedItem);
			GUIControl.SelectItemControl(GetID,listAlbums.GetID,m_iSelectedItem);

		}


		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnSortAsc)
			{
				m_bSortAscending=!m_bSortAscending;
				OnSort();
			}
			if (control==btnView)
			{
				ShowViews();
				return;
			}


			if (control==btnViewAs)
			{
				switch (currentViewMethod)
				{
					case ViewAs.Album:
						currentViewMethod=ViewAs.List;
						break;
					case ViewAs.List:
						currentViewMethod=ViewAs.Album;
						break;
				}
				LoadDirectory();
			}

			if (control == btnSortBy) // sort by
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
						currentSortMethod=SortMethod.Duration;
						break;
					case SortMethod.Duration:
						currentSortMethod=SortMethod.Channel;
						break;
				}
				OnSort();
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
				if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
				{
					OnPlayRecording(iItem);
				}
				if (actionType == Action.ActionType.ACTION_SHOW_INFO) 
				{
					OnShowContextMenu();
				}
			}
		}

    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
					case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
						UpdateProperties();
					break;
      }
      return base.OnMessage(message);
    }

		protected override void OnShowContextMenu()
		{
			int iItem=GetSelectedItemNo();
			GUIListItem pItem=GetItem(iItem);
			if (pItem==null) return;
			if (pItem.IsFolder) return;
			TVRecorded rec=(TVRecorded)pItem.TVTag;

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
					OnDeleteRecording(iItem);
				}
					break;

				case 0: // play
				{
					if ( OnPlayRecording(iItem))
						return;
				}
					break;
			}
		}
		#endregion

		#region recording methods
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

    void LoadDirectory()
    {
			GUIControl.ClearControl(GetID,listAlbums.GetID);
			GUIControl.ClearControl(GetID,listViews.GetID);

			ArrayList recordings = new ArrayList();
      ArrayList itemlist = new ArrayList();
			TVDatabase.GetRecordedTV(ref recordings);
			if (currentShow==String.Empty)
			{
				foreach (TVRecorded rec in recordings)
				{
					bool add=true;
					foreach (GUIListItem item in itemlist)
					{
						TVRecorded rec2 = item.TVTag as TVRecorded;
						if (rec.Title.Equals(rec2.Title)) 
						{
							item.IsFolder=true;
							Utils.SetDefaultIcons(item);
							string strLogo=Utils.GetCoverArt(Thumbs.TVShows,rec.Title);
							if (System.IO.File.Exists(strLogo))
							{
								item.ThumbnailImage=strLogo;
								item.IconImageBig=strLogo;
								item.IconImage=strLogo;
							}
							add=false;
							break;
						}
					}
					if (add)
					{
						GUIListItem item=new GUIListItem();
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
						itemlist.Add(item);
					}
				}
			}
			else
			{
				GUIListItem item=new GUIListItem();
				item.Label="..";
				item.IsFolder=true;
				Utils.SetDefaultIcons(item);
				itemlist.Add(item);
				foreach (TVRecorded rec in recordings)
				{
					if (rec.Title.Equals(currentShow))
					{
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
						itemlist.Add(item);
					}
				}
			}
			foreach (GUIListItem item in itemlist)
      {
				listAlbums.Add(item);
				listViews.Add(item);
      }
      
      string strObjects=String.Format("{0} {1}", itemlist.Count, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount",strObjects);
			GUIControl cntlLabel = GetControl(12);
			
			if (currentViewMethod==ViewAs.Album)
				cntlLabel.YPosition = listAlbums.SpinY;
			else
				cntlLabel.YPosition = listViews.SpinY;

      OnSort();
      UpdateButtonStates();
			UpdateProperties();
	
    }

    void UpdateButtonStates()
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
				case SortMethod.Duration:
					strLine=GUILocalizeStrings.Get(1017);//Sort by: Duration
					break;
			}
			GUIControl.SetControlLabel(GetID,btnSortBy.GetID,strLine);
			switch (currentViewMethod)
			{
				case ViewAs.Album:
					strLine=GUILocalizeStrings.Get(100);
					break;
				case ViewAs.List:
					strLine=GUILocalizeStrings.Get(101);
					break;
			}
			GUIControl.SetControlLabel(GetID,btnViewAs.GetID,strLine);


			if (m_bSortAscending)
				btnSortAsc.Selected=false;
			else
				btnSortAsc.Selected=true;

			if (currentViewMethod==ViewAs.List)
			{
				GUIControl.HideControl(GetID,(int)Controls.LABEL_PROGRAMTITLE);
				GUIControl.HideControl(GetID,(int)Controls.LABEL_PROGRAMDESCRIPTION);
				GUIControl.HideControl(GetID,(int)Controls.LABEL_PROGRAMGENRE);
				GUIControl.HideControl(GetID,(int)Controls.LABEL_PROGRAMTIME);
				listAlbums.IsVisible=false;
				listViews.IsVisible=true;
			}
			else
			{
				GUIControl.ShowControl(GetID,(int)Controls.LABEL_PROGRAMTITLE);
				GUIControl.ShowControl(GetID,(int)Controls.LABEL_PROGRAMDESCRIPTION);
				GUIControl.ShowControl(GetID,(int)Controls.LABEL_PROGRAMGENRE);
				GUIControl.ShowControl(GetID,(int)Controls.LABEL_PROGRAMTIME);
				listAlbums.IsVisible=true;
				listViews.IsVisible=false;
			}
    }

		void SetLabels()
		{
			SortMethod method=currentSortMethod;
			bool bAscending=m_bSortAscending;

			for (int i=0; i < listAlbums.Count;++i)
			{
				GUIListItem item1=listAlbums[i];
				GUIListItem item2=listViews[i];
				if (item1.Label=="..") continue;
				TVRecorded rec=(TVRecorded)item1.TVTag;
				item1.Label=item2.Label=rec.Title;
				TimeSpan ts = rec.EndTime-rec.StartTime;
				string strTime=String.Format("{0} {1} ({2})", 
					Utils.GetShortDayString(rec.StartTime) , 
					rec.StartTime.ToShortTimeString() , 
					Utils.SecondsToHMString( (int)ts.TotalSeconds));
				item1.Label2=item2.Label2=strTime;
				if (currentViewMethod==ViewAs.Album)
				{
					item1.Label3=item2.Label3=rec.Genre;
				}
				else 
				{
					if (currentSortMethod==SortMethod.Channel)
						item1.Label2=item2.Label2=rec.Channel;
				}
			}
		}

		bool OnPlayRecording(int iItem)
		{
			GUIListItem pItem=GetItem(iItem);
			if (pItem==null) return false;
			if (pItem.IsFolder) 
			{
				if (pItem.Label.Equals("..")) 
					currentShow=String.Empty;
				else 
					currentShow=pItem.Label;
				LoadDirectory();
				return false;
			}

			TVRecorded rec=(TVRecorded)pItem.TVTag;
			if (System.IO.File.Exists(rec.FileName))
			{
				Log.Write("TVRecording:play:{0}",rec.FileName);
				g_Player.Stop();
				Recorder.StopViewing();
        
				rec.Played++;
				TVDatabase.PlayedRecordedTV(rec);
				IMDBMovie movieDetails = new IMDBMovie();
				VideoDatabase.GetMovieInfo(rec.FileName, ref movieDetails);
				int movieid=VideoDatabase.GetMovieId(rec.FileName);
				int stoptime=0;
				if (movieid >=0)
				{
					Log.Write("play got movie id:{0} for {1}", movieid,rec.FileName);
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

		void OnDeleteRecording(int iItem)
		{
			m_iSelectedItem=GetSelectedItemNo();
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
			while (m_iSelectedItem>=GetItemCount() && m_iSelectedItem>0) m_iSelectedItem--;
			GUIControl.SelectItemControl(GetID,listViews.GetID,m_iSelectedItem);
			GUIControl.SelectItemControl(GetID,listAlbums.GetID,m_iSelectedItem);
		}

		void OnDeleteWatchedRecordings()
		{
			m_iSelectedItem=GetSelectedItemNo();
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
			while (m_iSelectedItem>=GetItemCount() && m_iSelectedItem>0) m_iSelectedItem--;
			GUIControl.SelectItemControl(GetID,listViews.GetID,m_iSelectedItem);
			GUIControl.SelectItemControl(GetID,listAlbums.GetID,m_iSelectedItem);
		}

		void UpdateProperties()
		{
			TVRecorded rec;
			GUIListItem pItem=GetItem( GetSelectedItemNo() );
			if (pItem==null)
			{
				rec = new TVRecorded();
				rec.SetProperties();
				return;
			}
			rec=pItem.TVTag as TVRecorded;
			if (rec==null)
			{
				rec = new TVRecorded();
				rec.SetProperties();
				return;
			}
			rec.SetProperties();
		}

		#endregion

		#region album/list view management
    GUIListItem GetSelectedItem()
    {
      int iControl;
      iControl=listAlbums.GetID;
			if (currentViewMethod==ViewAs.List)
				iControl=listViews.GetID;
      GUIListItem item = GUIControl.GetSelectedListItem(GetID,iControl);
      return item;
    }

		GUIListItem GetItem(int iItem)
		{
			if (currentViewMethod==ViewAs.List) 
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
			if (currentViewMethod==ViewAs.List)
				iControl=listViews.GetID;

      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
      OnMessage(msg);         
      int iItem=(int)msg.Param1;
      return iItem;
		}
		int GetItemCount()
		{
			if (currentViewMethod==ViewAs.List)
				return listViews.Count;
			else 
				return listAlbums.Count;
		}
#endregion

    #region Sort Members
    void OnSort()
    {
      SetLabels();
			listAlbums.Sort(this);
			listViews.Sort(this);
      UpdateButtonStates();
    }

    public int Compare(object x, object y)
    {
      if (x==y) return 0;
      GUIListItem item1=(GUIListItem)x;
      GUIListItem item2=(GUIListItem)y;
      if (item1==null) return -1;
      if (item2==null) return -1;
			if (item1.IsFolder && item1.Label=="..") return -1;
			if (item2.IsFolder && item2.Label=="..") return -1;
			if (item1.IsFolder && !item2.IsFolder) return -1;
			else if (!item1.IsFolder && item2.IsFolder) return 1; 

      int iComp=0;
      TimeSpan ts;
      TVRecorded rec1=(TVRecorded)item1.TVTag;
      TVRecorded rec2=(TVRecorded)item2.TVTag;
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

				case SortMethod.Duration:
				{
					long duration1=rec1.End-rec1.Start;
					long duration2=rec2.End-rec2.Start;
					if (m_bSortAscending)
					{
						if (duration1==duration2) goto case SortMethod.Date;
						if (duration1>duration2) return 1;
						return -1;
					}
					else
					{
						if (duration1==duration2) goto case SortMethod.Date;
						if (duration1<duration2) return 1;
						return -1;
					}
				}
				
        case SortMethod.Date:
          if (m_bSortAscending)
          {
            if (rec1.StartTime==rec2.StartTime) return 0;
            if (rec1.StartTime<rec2.StartTime) return 1;
            return -1;
          }
          else
          {
            if (rec1.StartTime==rec2.StartTime) return 0;
            if (rec1.StartTime>rec2.StartTime) return 1;
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


		#region playback events
    private void OnPlayRecordingBackStopped(MediaPortal.Player.g_Player.MediaType type, int stoptime, string filename)
    {
      if (type!=g_Player.MediaType.Recording) return;
      int movieid=VideoDatabase.GetMovieId(filename);
      if (movieid<0) return;
      if (stoptime>0)
        VideoDatabase.SetMovieStopTime(movieid,stoptime);
      else
        VideoDatabase.DeleteMovieStopTime(movieid);
    }

    private void OnPlayRecordingBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
			if (type!=g_Player.MediaType.Recording) return;
			int movieid=VideoDatabase.GetMovieId(filename);
      if (movieid<0) return;
      
			VideoDatabase.DeleteMovieStopTime(movieid);

			if (m_bDeleteWatchedShow)
			{
				g_Player.Stop();

				ArrayList itemlist = new ArrayList();
				TVDatabase.GetRecordedTV(ref itemlist);
				foreach (TVRecorded rec in itemlist)
				{
					if (rec.FileName.ToLower().Equals(filename.ToLower()) )
					{
						TVDatabase.RemoveRecordedTV(rec);
						break;
					}
				}
				DiskManagement.DeleteRecording(filename);
				VideoDatabase.DeleteMovieInfo(filename);
				VideoDatabase.DeleteMovie(filename);
			}
			else
			{
				IMDBMovie details = new IMDBMovie();
				VideoDatabase.GetMovieInfoById(movieid, ref details);
				details.Watched++;
				VideoDatabase.SetWatched(details);
			}
    }

    private void OnPlayRecordingBackStarted(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
      if (type!=g_Player.MediaType.Recording) return;
      VideoDatabase.AddMovieFile(filename);
    }

		#endregion
  }
}
