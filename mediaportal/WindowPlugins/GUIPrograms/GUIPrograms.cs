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
		CONTROL_LIST            =    7,
		CONTROL_THUMBS			=    8,
		CONTROL_LBLMYPROGRAMS   =    9,
		CONTROL_LBLCURAPP        =   10,
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
		}
		View currentView = View.VIEW_AS_LARGEICONS;

		static Applist apps = ProgramsDatabase.ProgramDatabase.AppList;
		AppItem lastApp = null;
		string lastFilepath = "";
		MapSettings       _MapSettings = new MapSettings();

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
			using(AMS.Profile.Xml xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				switch (currentView)
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
				}
			}
		}

		void LoadSettings()
		{
			using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				string strTmp="";
				strTmp=(string)xmlreader.GetValue("myprograms","viewby");
				if (strTmp!=null)
				{
					if (strTmp=="list") currentView=View.VIEW_AS_LIST;
					else if (strTmp=="icons") currentView=View.VIEW_AS_ICONS;
					else if (strTmp=="largeicons") currentView=View.VIEW_AS_LARGEICONS;
				}
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


		public override void OnAction(Action action)
		{
			//			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG ||action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				// <ESC> keypress in some myProgram Menu => jump to main menu
				SaveFolderSettings("");
				GUIWindowManager.PreviousWindow();
				return;
			}
		
			if (action.wID == Action.ActionType.ACTION_SHOW_INFO) 
			{
				// <F3> keypress
				if (null != lastApp) 
				{
					GUIListItem item = GetSelectedItem();
					if (!item.Label.Equals( ProgramUtils.cBackLabel ))
					{
						// show file info but only if the selected item is not the back button
						lastApp.OnInfo(item);
					}
				}
				return;
			}

			base.OnAction(action);
		}

		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{
				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
					// display application list
					base.OnMessage(message);
					LoadFolderSettings("");
					lastApp = apps.GetAppByID(_MapSettings.LastAppID);
					if (lastApp != null)
					{
						lastFilepath = lastApp.DefaultFilepath();
					}
					else
					{
						lastFilepath = "";
					}
					UpdateListControl();
					UpdateButtons();
					ShowThumbPanel();
					return true;

				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
					SaveSettings();
					break;

			
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					int iControl=message.SenderControlId;
					if (iControl==(int)Controls.CONTROL_BTNVIEWASICONS)
					{
						// switch to next view
						switch (currentView)
						{
							case View.VIEW_AS_LIST:
								currentView=View.VIEW_AS_ICONS;
								break;
							case View.VIEW_AS_ICONS:
								currentView=View.VIEW_AS_LARGEICONS;
								break;
							case View.VIEW_AS_LARGEICONS:
								currentView=View.VIEW_AS_LIST;
								break;
						}
						UpdateButtons();
						ShowThumbPanel();
						GUIControl.FocusControl(GetID,iControl);
					}
					else if (iControl==(int)Controls.CONTROL_THUMBS||iControl==(int)Controls.CONTROL_LIST)
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
								// non-folder item clicked => always a fileitem!
								FileItemClicked(item);
							}
							else
							{
								// folder-item clicked.... 
								if( item.Label.Equals( ProgramUtils.cBackLabel ) )
								{
									BackItemClicked(item);
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
							GUIListControl list=(GUIListControl)GetControl((int)Controls.CONTROL_LIST);
							GUIThumbnailPanel panel=(GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMBS);
							lastApp.OnSort(list, panel);
							UpdateButtons();
						}
						GUIControl.FocusControl(GetID,iControl);
					}
					else if (iControl==(int)Controls.CONTROL_BTNSORTASC)
					{
						// toggle asc / desc for current sort method...
						if (lastApp != null)
						{
							GUIListControl list=(GUIListControl)GetControl((int)Controls.CONTROL_LIST);
							GUIThumbnailPanel panel=(GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMBS);
							lastApp.OnSortToggle(list, panel);
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

		void BackItemClicked(GUIListItem item)
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
					}
					else
					{
						// back to home screen.....
						_MapSettings.LastAppID = -1;
						lastFilepath = "";
					}
				}

				UpdateListControl();
			}
			else
			{
				// from root.... go back to main menu
				GUIWindowManager.PreviousWindow(); 
			}


		}


		void UpdateButtons()
		{

			GUIControl.HideControl(GetID,(int)Controls.CONTROL_LIST);
			GUIControl.HideControl(GetID,(int)Controls.CONTROL_THUMBS);
      
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
      

			int iControl=(int)Controls.CONTROL_LIST;
			if (currentView != View.VIEW_AS_LIST )
			{
				iControl=(int)Controls.CONTROL_THUMBS;
			}
			GUIControl.ShowControl(GetID,iControl);
			GUIControl.FocusControl(GetID,iControl);
      

			string strLine="";
			View view=currentView;
			switch (view)
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
			}
			GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTNVIEWASICONS,strLine);

			if (lastApp != null)
			{
				GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTNSORTBY, lastApp.CurrentSortTitle());
				if (lastApp.CurrentSortIsAscending())
					GUIControl.DeSelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
				else
					GUIControl.SelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
			}
		}

		void ShowThumbPanel()
		{
			GUIThumbnailPanel pControl=(GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMBS);
			pControl.ShowBigIcons( currentView == View.VIEW_AS_LARGEICONS );
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
			GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST ); 
			GUIControl.ClearControl(GetID, (int)Controls.CONTROL_THUMBS );
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

			string strObjects=String.Format("{0} {1}", TotalItems, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);

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
					GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST, gli );
					GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMBS,gli);
				}
			}
			return(Total);
		}


		private GUIListItem GetSelectedItem()
		{
			int iControl;
			if (currentView != View.VIEW_AS_LIST )
			{
				iControl=(int)Controls.CONTROL_THUMBS;
			}
			else
				iControl=(int)Controls.CONTROL_LIST;
			GUIListItem item = GUIControl.GetSelectedListItem(GetID,iControl);
			return item;
		}

	}
}