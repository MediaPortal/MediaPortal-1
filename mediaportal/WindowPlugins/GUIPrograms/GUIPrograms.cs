using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml.Serialization;

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using Programs.Utils;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{

	enum Controls
	{
		CONTROL_BTNVIEWASICONS  =    2,
		CONTROL_BTNSORTBY		=    4,
		CONTROL_BTNSORTASC    =    5,
		CONTROL_BTNREFRESH      =    3,
		CONTROL_LBLMYPROGRAMS   =    9,
		CONTROL_LBLCURAPP        =   10,
		CONTROL_VIEW				= 7,
	};


	/// <summary>
	/// The GUIPrograms plugin is used to list a collection of arbitrary files
	/// and use them as arguments when launching external applications.
	/// </summary>
	/// 
	public class GUIPrograms : GUIWindow
	{

		[Serializable]
			public class MapSettings
		{
			protected int   _SortBy;
			protected int   _ViewAs;
			protected bool _SortAscending;
			protected int _LastAppID;
			protected int _LastFileID;

			public MapSettings()
			{
				_SortBy=0;//name
				_ViewAs=0;//list
				_SortAscending=true;
				_LastAppID=-1;
				_LastFileID=-1;
			}


			[XmlElement("SortBy")]
			public int SortBy
			{
				get { return _SortBy;}
				set { _SortBy=value;}
			}
      
			[XmlElement("ViewAs")]
			public int ViewAs
			{
				get { return _ViewAs;}
				set { _ViewAs=value;}
			}
      
			[XmlElement("SortAscending")]
			public bool SortAscending
			{
				get { return _SortAscending;}
				set { _SortAscending=value;}
			}

			[XmlElement("LastAppID")]
			public int LastAppID
			{
				get { return _LastAppID;}
				set { _LastAppID=value;}
			}

			[XmlElement("LastFileID")]
			public int LastFileID
			{
				get { return _LastFileID;}
				set { _LastFileID=value;}
			}

		}

		enum View
		{
			VIEW_AS_LIST    =       0,
			VIEW_AS_ICONS    =      1,
			VIEW_AS_LARGEICONS  =   2,
			VIEW_AS_FILMSTRIP  =    3,
		}

		static Applist apps = ProgramsDatabase.ProgramDatabase.AppList;
		AppItem lastApp = null;
		string lastFilepath = "";
		MapSettings       _MapSettings = new MapSettings();
		int m_iItemSelected=-1;   
		string m_iItemSelectedLabel= "";
		
		// filmstrip slideshow timer stuff
		int m_iSpeed=3; // speed in seconds between two slides
		int m_lSlideTime=0;



		/// <summary>
		/// Constructor used to specify to the MediaPortal Core the window that we 
		/// are creating.
		/// </summary>
		public GUIPrograms()
		{
			GetID = (int)GUIWindow.Window.WINDOW_FILES;
			apps = ProgramsDatabase.ProgramDatabase.AppList;			
			LoadSettings();
		}

		~GUIPrograms()
		{
			SaveSettings();
			FolderSettings.DeleteFolderSetting("root","Programs");
		}


		/// <summary>
		/// Overridden in order to load the skin we are going to use.
		/// </summary>
		/// <returns>Whether or not the skin could be loaded</returns>
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\myprograms.xml");
		}


		void SaveSettings()
		{
			using(MediaPortal.Profile.Xml xmlwriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				switch ((View)_MapSettings.ViewAs)
				{
					case View.VIEW_AS_LIST: 
						xmlwriter.SetValue("myprograms","viewby","list");
						break;
					case View.VIEW_AS_ICONS: 
						xmlwriter.SetValue("myprograms","viewby","icons");
						break;
					case View.VIEW_AS_LARGEICONS: 
						xmlwriter.SetValue("myprograms","viewby","largeicons");
						break;
					case View.VIEW_AS_FILMSTRIP: 
						xmlwriter.SetValue("myprograms","viewby","filmstrip");
						break;
				}
				xmlwriter.SetValue("myprograms","lastAppID", _MapSettings.LastAppID.ToString());
				xmlwriter.SetValue("myprograms","sortby", _MapSettings.SortBy);
				// avoid bool conversion...... don't wanna know why it doesn't work! :-(
				if (_MapSettings.SortAscending)
				{
					xmlwriter.SetValue("myprograms","sortasc", "yes");
				}
				else
				{
					xmlwriter.SetValue("myprograms","sortasc", "no");
				}
				//Log.Write("dw myPrograms: saving xmlsettings lastappid {0}", _MapSettings.LastAppID);
			}
		}

		void LoadSettings()
		{
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				string strTmp="";
				strTmp=(string)xmlreader.GetValue("myprograms","viewby");
				if (strTmp!=null)
				{
					if (strTmp=="list") _MapSettings.ViewAs = (int)View.VIEW_AS_LIST;
					else if (strTmp=="icons") _MapSettings.ViewAs = (int)View.VIEW_AS_ICONS;
					else if (strTmp=="largeicons") _MapSettings.ViewAs = (int)View.VIEW_AS_LARGEICONS;
					else if (strTmp=="filmstrip") _MapSettings.ViewAs = (int)View.VIEW_AS_FILMSTRIP;
				}
				
				_MapSettings.LastAppID = xmlreader.GetValueAsInt("myprograms", "lastAppID", -1);
				_MapSettings.SortBy = xmlreader.GetValueAsInt("myprograms", "sortby", 0);
// NEIN!				_MapSettings.SortAscending = xmlreader.GetValueAsBool("myprograms", "sortasc", true);
				strTmp=(string)xmlreader.GetValue("myprograms","sortasc");
				if (strTmp!=null)
				{
					_MapSettings.SortAscending = (strTmp.ToLower() == "yes");
				}
				else
				{
					_MapSettings.SortAscending = true;
				}

			}
		}

		void LoadLastAppIDFromSettings()
		{
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				_MapSettings.LastAppID = xmlreader.GetValueAsInt("myprograms", "lastAppID", -1);
				_MapSettings.SortBy = xmlreader.GetValueAsInt("myprograms", "sortby", 0);
				_MapSettings.SortAscending = xmlreader.GetValueAsBool("myprograms", "sortasc", true);
			}
		}
		
		void LoadFolderSettings(string strDirectory)
		{
			if (strDirectory=="") strDirectory="root";
			object o;
			FolderSettings.GetFolderSetting(strDirectory,"Programs",typeof(GUIPrograms.MapSettings), out o);
			if (o!=null) _MapSettings = o as MapSettings;
			if (_MapSettings==null) _MapSettings  = new MapSettings();
		}

		void SaveFolderSettings(string strDirectory)
		{
			if (strDirectory=="") strDirectory="root";
			FolderSettings.AddFolderSetting(strDirectory,"Programs",typeof(GUIPrograms.MapSettings), _MapSettings);
		}


		public override void Render(float timePassed)
		{
			RenderFilmStrip();
			base.Render(timePassed);
		}

		private void RenderFilmStrip()
		{
			// in filmstrip mode, start a slideshow if more than one
			// pic is available for the selected item
			GUIFacadeControl pControl=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
			if (pControl == null) return;
			if (pControl.FilmstripView == null) return;
			if (pControl.FilmstripView.InfoImageFileName == "") return;
			if (_MapSettings == null) return;
			if (_MapSettings.ViewAs == (int)View.VIEW_AS_FILMSTRIP)
			{
				// does the thumb needs replacing??
				int dwTimeElapsed = ((int)(DateTime.Now.Ticks/10000)) - m_lSlideTime;
				if (dwTimeElapsed >= (m_iSpeed*1000))
				{
					RefreshFilmstripThumb(pControl.FilmstripView); // only refresh the picture, don't refresh the other data otherwise scrolling of labels is interrupted!
				}
			}
		}

		private void RefreshFilmstripThumb(GUIFilmstripControl pControl)
		{
			GUIListItem item = GetSelectedItem();
			// some preconditions...
			if (lastApp == null) return;
			if (item.MusicTag == null) return;
			if (!(item.MusicTag is FileItem)) return;
			FileItem curFile = item.MusicTag as FileItem;
			// ok... let's get a filename
			string strThumb = lastApp.GetCurThumb(curFile); 
			if (System.IO.File.Exists(strThumb) )
			{
				pControl.InfoImageFileName = strThumb;
			}
			lastApp.NextThumb(); // try to find a next thumbnail
			m_lSlideTime=(int)(DateTime.Now.Ticks/10000); // reset timer!
		}


		public override void OnAction(Action action)
		{
			if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
			{
				// <U> keypress
				BackItemClicked();
				UpdateButtons();
				return;
			}

			if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG ||action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				// <ESC> keypress in some myProgram Menu => jump to main menu
				SaveFolderSettings("");
				GUIWindowManager.ShowPreviousWindow();
				return;
			}
		
			if (action.wID == Action.ActionType.ACTION_SHOW_INFO) 
			{
				// <F3> keypress
				if (null != lastApp) 
				{
					m_iItemSelected=GetSelectedItemNo();
					GUIListItem item = GetSelectedItem();
					m_iItemSelectedLabel = item.Label;
					if (!item.Label.Equals( ProgramUtils.cBackLabel ) && (!item.IsFolder))
					{
						// show file info but only if the selected item is not the back button
						lastApp.OnInfo(item);
						UpdateListControl();
					}
				}
				return;
			}

			base.OnAction(action);
		}

		public override bool OnMessage(GUIMessage message)
		{
			GUIFacadeControl view=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
			switch ( message.Message )
			{
				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
					// display application list
					//					Log.Write("GUIPrograms: gui_msg_windows_init");
					base.OnMessage(message);
					LoadFolderSettings("");
					LoadLastAppIDFromSettings(); // hacky load back the last app id, otherwise this can get lost from dx resets....
					lastApp = apps.GetAppByID(_MapSettings.LastAppID);
					if (lastApp != null)
					{
						lastFilepath = lastApp.DefaultFilepath();
						lastApp.CurrentSortIndex = _MapSettings.SortBy;
						lastApp.CurrentSortIsAscending = _MapSettings.SortAscending;
						//Log.Write("dw myPrograms: lastApp initialized {0} {1}", lastApp.AppID, lastApp.Title);
						//Log.Write("dw myPrograms: lastFilepath initialized {0}", lastFilepath);
					}
					else
					{
						lastFilepath = "";
					}
					UpdateListControl();
					ShowThumbPanel();
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000); // reset timer!
					return true;

				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
					//					Log.Write("GUIPrograms: gui_msg_windows_deinit");
					SaveSettings();
					// make sure the selected index wasn't reseted already
					// and save the index only if it's non-zero
					// otherwise: DXDevice.Reset clears selection 
					int iItemIndex = GetSelectedItemNo();
					if (iItemIndex > 0)
					{
						GUIListItem item = GetSelectedItem();
						m_iItemSelected=GetSelectedItemNo();
						m_iItemSelectedLabel = item.Label;

					}
					break;
			
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					int iControl=message.SenderControlId;
					if (iControl==(int)Controls.CONTROL_BTNVIEWASICONS)
					{
						// switch to next view
						switch ((View)_MapSettings.ViewAs)
						{
							case View.VIEW_AS_LIST:
								_MapSettings.ViewAs = (int)View.VIEW_AS_ICONS;
								break;
							case View.VIEW_AS_ICONS:
								_MapSettings.ViewAs = (int)View.VIEW_AS_LARGEICONS;
								break;
							case View.VIEW_AS_LARGEICONS:
								_MapSettings.ViewAs = (int)View.VIEW_AS_FILMSTRIP;
								break;
							case View.VIEW_AS_FILMSTRIP:
								_MapSettings.ViewAs = (int)View.VIEW_AS_LIST;
								break;
						}
						ShowThumbPanel();
						GUIControl.FocusControl(GetID,iControl);
					}
					else if (iControl==(int)Controls.CONTROL_VIEW)
					{
						// application or file-item was clicked....
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);         
						int iItem=(int)msg.Param1;
						int iAction=(int)message.Param1;
	
						if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
						{
							GUIListItem item = GetSelectedItem();
							if( !item.IsFolder )
							{
								m_iItemSelected=GetSelectedItemNo();
								m_iItemSelectedLabel = item.Label;
								//								Log.Write("GUIPrograms: ACTION_SELECT_ITEM writes itemsel: {0}", m_iItemSelected);
								// non-folder item clicked => always a fileitem!
								FileItemClicked(item);
							}
							else
							{
								// folder-item clicked.... 
								m_iItemSelected=-1;
								m_iItemSelectedLabel = "";
								if( item.Label.Equals( ProgramUtils.cBackLabel ) )
								{
									BackItemClicked();
									UpdateButtons();
								}
								else
								{
									// application-item or subfolder
									FolderItemClicked(item);
									UpdateButtons();
								}
							}
						}
					}
					else if (iControl==(int)Controls.CONTROL_BTNSORTBY)
					{
						// get next sort method...
						if (lastApp != null)
						{
							lastApp.OnSort(view, true);
							_MapSettings.SortBy = lastApp.CurrentSortIndex;
							UpdateButtons();
						}
						GUIControl.FocusControl(GetID,iControl);
					}
					else if (iControl==(int)Controls.CONTROL_BTNSORTASC)
					{
						// toggle asc / desc for current sort method...
						if (lastApp != null)
						{
							lastApp.OnSortToggle(view);
							_MapSettings.SortAscending = lastApp.CurrentSortIsAscending;
							UpdateButtons();
						}
						GUIControl.FocusControl(GetID,iControl);
					}
					else if (iControl==(int)Controls.CONTROL_BTNREFRESH)
					{
						if (lastApp != null)
						{
							lastApp.Refresh(true);
							lastFilepath = lastApp.DefaultFilepath();
							UpdateListControl();
						}
					}
					break;
			}
			return base.OnMessage( message );
		}

		bool RefreshButtonVisible()
		{
			if (lastApp == null) 
			{return false;}
			else
			{
				return (lastApp.RefreshButtonVisible() && lastApp.GUIRefreshPossible && lastApp.EnableGUIRefresh);
			}
		}

		void FileItemClicked(GUIListItem item)
		{
			// file item was clicked => launch it!
			// string strFile = item.Label;
			if (lastApp != null) 
			{
				_MapSettings.LastAppID = lastApp.AppID;
				lastFilepath = lastApp.DefaultFilepath(); 
				//				Log.Write("dw myPrograms: FileItemClicked: lastAppID changes to {0} {1}", _MapSettings.LastAppID, lastApp.Title);

				lastApp.LaunchFile(item);
			}
		}

		void FolderItemClicked(GUIListItem item)
		{
			if (item.MusicTag != null)
			{
				if (item.MusicTag is AppItem)
				{
					bool bPinOk = true;
					AppItem candidate = (AppItem)item.MusicTag;
					if (candidate.Pincode > 0)
					{
						bPinOk = candidate.CheckPincode();
					}
					if (bPinOk)
					{
						lastApp = candidate;
						_MapSettings.LastAppID = lastApp.AppID;
						lastFilepath = lastApp.DefaultFilepath();
						//						Log.Write("dw myPrograms: FolderItemClicked: lastAppID changes to {0} {1}", _MapSettings.LastAppID, lastApp.Title);

					}
				}
				else if (item.MusicTag is FileItem)
				{
					// example: subfolder in directory-cache mode
					// => set filepath which will be a search criteria for sql / browse
					if (lastFilepath == "")
					{
						// first subfolder
						lastFilepath = lastApp.FileDirectory + "\\" + item.Label;
					}
					else
					{
						// subsequent subfolder
						lastFilepath = lastFilepath + "\\" + item.Label;
					}
				}
				UpdateListControl();
			}
			else
			{
				// tag is null
				// example: subfolder in directory-browse mode
				lastFilepath = item.Path;
				UpdateListControl();
			}
		}

		void BackItemClicked()
		{
			if (lastApp != null)
			{
				// debug: Log.Write("lastFilepath {0} / lastApp.FileDirectory {1} / lastApp.Title {2}", this.lastFilepath, lastApp.FileDirectory, lastApp.Title);
				if ((lastFilepath != null) && (lastFilepath != "") && (lastFilepath != lastApp.FileDirectory))
				{
					// back item in flielist clicked
					string strNewPath = System.IO.Path.GetDirectoryName(lastFilepath);
					lastFilepath = strNewPath;
				}
				else
				{
					// back item in application list clicked
					// go to father item
					lastApp = apps.GetAppByID(lastApp.FatherID);
					if (lastApp != null)
					{
						_MapSettings.LastAppID = lastApp.AppID;
						lastFilepath = lastApp.DefaultFilepath(); 
						//						Log.Write("dw myPrograms: BackItemClicked 1: lastAppID changes to {0} {1}", _MapSettings.LastAppID, lastApp.Title);
					}
					else
					{
						// back to home screen.....
						_MapSettings.LastAppID = -1;
						lastFilepath = "";
						//						Log.Write("dw myPrograms: BackItemClicked 2: lastAppID changes to {0}", _MapSettings.LastAppID);
					}
				}
				UpdateListControl();
			}
			else
			{
				// from root.... go back to main menu
				GUIWindowManager.ShowPreviousWindow(); 
			}


		}


		void UpdateButtons()
		{

			if (RefreshButtonVisible())
			{
				GUIControl.ShowControl(GetID, (int)Controls.CONTROL_BTNREFRESH);
			}
			else 
			{
				GUIControl.HideControl(GetID, (int)Controls.CONTROL_BTNREFRESH);
			}

			// display apptitle if available.....
			if (lastApp != null)
			{
				GUIControl.HideControl(GetID, (int)Controls.CONTROL_LBLMYPROGRAMS);
				GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LBLCURAPP, lastApp.Title);
				GUIControl.ShowControl(GetID, (int)Controls.CONTROL_LBLCURAPP);
				GUIControl.ShowControl(GetID, (int)Controls.CONTROL_BTNSORTBY);
				GUIControl.ShowControl(GetID, (int)Controls.CONTROL_BTNSORTASC);
			}
			else 
			{
				GUIControl.HideControl(GetID, (int)Controls.CONTROL_LBLCURAPP);
				GUIControl.ShowControl(GetID, (int)Controls.CONTROL_LBLMYPROGRAMS);
				GUIControl.HideControl(GetID, (int)Controls.CONTROL_BTNSORTBY);
				GUIControl.HideControl(GetID, (int)Controls.CONTROL_BTNSORTASC);
			}
      
			string strLine="";
			switch ((View)_MapSettings.ViewAs)
			{
				case View.VIEW_AS_LIST:
					strLine=GUILocalizeStrings.Get(101);
					break;
				case View.VIEW_AS_ICONS:
					strLine=GUILocalizeStrings.Get(100);
					break;
				case View.VIEW_AS_LARGEICONS:
					strLine=GUILocalizeStrings.Get(417);
					break;
				case View.VIEW_AS_FILMSTRIP:
					strLine=GUILocalizeStrings.Get(733);
					break;
			}
			GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTNVIEWASICONS,strLine);

			if (lastApp != null)
			{
				GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTNSORTBY, lastApp.CurrentSortTitle());
				if (lastApp.CurrentSortIsAscending)
					GUIControl.DeSelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
				else
					GUIControl.SelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
			}
		}

		void ShowThumbPanel()
		{
			int iItem=GetSelectedItemNo(); 
			GUIFacadeControl pControl=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
			if ( _MapSettings.ViewAs== (int)View.VIEW_AS_LARGEICONS )
			{
				pControl.View=GUIFacadeControl.ViewMode.LargeIcons;
			}
			else if (_MapSettings.ViewAs== (int)View.VIEW_AS_ICONS)
			{
				pControl.View=GUIFacadeControl.ViewMode.SmallIcons;
			}
			else if (_MapSettings.ViewAs== (int)View.VIEW_AS_LIST)
			{
				pControl.View=GUIFacadeControl.ViewMode.List;
			}
			else if (_MapSettings.ViewAs== (int)View.VIEW_AS_FILMSTRIP)
			{
				pControl.View=GUIFacadeControl.ViewMode.Filmstrip;
			}
			if (iItem>-1)
			{
				GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIEW,iItem);
			}
			UpdateButtons();
		}

		int GetCurrentFatherID()
		{
			if (lastApp != null)
			{
				return lastApp.AppID;
			}
			else
			{
				return -1; // root
			}
		}


		bool thereAreAppsToDisplay()
		{
			if (lastApp == null)
			{
				return true; // root has apps
			}
			else
			{
				return lastApp.SubItemsAllowed(); // grouper items for example
			}
		}

		bool thereAreFilesOrLinksToDisplay()
		{
			return(lastApp != null); // all apps can have files except the root
		}

		bool isBackButtonNecessary()
		{
			return(lastApp != null); // always show back button except for root
		}

		void UpdateListControl()
		{
			int TotalItems = 0;
			GUIControl.ClearControl(GetID,(int)Controls.CONTROL_VIEW);
			if (isBackButtonNecessary())
			{
				ProgramUtils.AddBackButton();
			}

			if (thereAreAppsToDisplay())
			{
				TotalItems = TotalItems + DisplayApps();
			}
			
			if (thereAreFilesOrLinksToDisplay())
			{
				TotalItems = TotalItems + DisplayFiles();
			}

			if (lastApp != null)
			{
				GUIFacadeControl pControl=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
				lastApp.OnSort(pControl, false);
			}
			

			string strObjects=String.Format("{0} {1}", TotalItems, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);

			if (m_iItemSelected>=0)
			{
				//				int nIndex = IndexOfLabelText(this.m_iItemSelectedLabel);
				//				if ((nIndex >= 0) && (nIndex <= pControl.ListView.Count - 1))
				//				{
				//				}
				GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_VIEW,m_iItemSelected);
			}
		}

		int IndexOfLabelText(string strValue)
		{
			// TODO: if launch changes position of item..... index is wrong!
			int res = -1;

			return res;
		}


		int DisplayFiles()
		{
			int Total = 0;
			if (lastApp == null) {return Total;}
			Total = lastApp.DisplayFiles(this.lastFilepath);
			return(Total);
		}

		int DisplayApps()
		{
			int Total = 0;
			foreach(AppItem app in apps.appsOfFatherID(GetCurrentFatherID()))
			{
				if (app.Enabled)
				{
					Total = Total + 1;
					GUIListItem gli = new GUIListItem( app.Title );
					if (app.Imagefile != "")
					{
						gli.ThumbnailImage = app.Imagefile;
						gli.IconImageBig = app.Imagefile;
						gli.IconImage = app.Imagefile;
					}
					else 
					{
						gli.ThumbnailImage = GUIGraphicsContext.Skin+@"\media\DefaultFolderBig.png";
						gli.IconImageBig = GUIGraphicsContext.Skin+@"\media\DefaultFolderBig.png";
						gli.IconImage = GUIGraphicsContext.Skin+@"\media\DefaultFolderNF.png";
					}
					gli.MusicTag = app;
					gli.IsFolder = true; // pseudo-folder....
					gli.OnItemSelected +=new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(OnItemSelected);
					GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_VIEW,gli);
				}
			}
			return(Total);
		}


		private void OnItemSelected(GUIListItem item, GUIControl parent)
		{
			GUIFilmstripControl filmstrip=parent as GUIFilmstripControl ;
			if (filmstrip==null) return;
			string thumbName = "";
			if ((item.ThumbnailImage != GUIGraphicsContext.Skin+@"\media\DefaultFolderBig.png")
				&& (item.ThumbnailImage != ""))
			{
				// only show big thumb if there is really one....
				thumbName = item.ThumbnailImage;
			}
			filmstrip.InfoImageFileName= thumbName;
		}

		private GUIListItem GetSelectedItem()
		{
			GUIListItem item = GUIControl.GetSelectedListItem(GetID,(int)Controls.CONTROL_VIEW);
			return item;
		}


		int GetSelectedItemNo()
		{
			GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,(int)Controls.CONTROL_VIEW,0,0,null);
			OnMessage(msg);         
			int iItem=(int)msg.Param1;
			return iItem;
		}


	}
}