using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using Programs.Utils;
using ProgramsDatabase;

namespace GUIPrograms
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

		enum View
		{
			VIEW_AS_LIST    =       0,
			VIEW_AS_ICONS    =      1,
			VIEW_AS_LARGEICONS  =   2,
		}
		View currentView = View.VIEW_AS_LARGEICONS;

		static Applist apps = ProgramsDatabase.ProgamDatabase.AppList;
		AppItem lastApp = null;

		/// <summary>
		/// Constructor used to specify to the MediaPortal Core the window that we 
		/// are creating.
		/// </summary>
		public GUIPrograms()
		{
			GetID = (int)GUIWindow.Window.WINDOW_FILES;
			apps = ProgramsDatabase.ProgamDatabase.AppList;			
			LoadSettings();
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
		


		public override void OnAction(Action action)
		{
			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				// <ESC> keypress in some myProgram Menu => jump to main menu
				GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_HOME);
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
					DisplayApplications();
					UpdateButtons();
					ShowThumbPanel();
					return true;

				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
					lastApp = null;
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
						GUIGraphicsContext.SendMessage(msg);         
						int iItem=(int)msg.Param1;
						int iAction=(int)message.Param1;
	
						if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
						{
							GUIListItem item = GetSelectedItem();
							if( item.IsFolder )
							{
								if( item.Label.Equals( ProgramUtils.cBackLabel ) )
								{
									// "back" - item was clicked
									if (lastApp != null)
									{
										// in filescreen....
										bool bTopLevel = lastApp.BackItemClick(item);
										if (bTopLevel)
										{
											lastApp = null;
											DisplayApplications();
										}
										UpdateButtons();
									}
									else
									{
										// in appscreen.... (disabled because back button is not visible anymore...)
										GUIWindowManager.PreviousWindow(); 
									}
								}
								else
								{
									if (lastApp == null)
									{
										// application-item clicked
										if (item.MusicTag != null)
										{
											lastApp = (AppItem)item.MusicTag;
											lastApp.DisplayFiles(null); 
										}
									}
									else 
									{
										// subfolder clicked
										lastApp.DisplayFiles(item); 
									}
									UpdateButtons();
								}
							}
							else
							{
								// file item was clicked => launch it!
								// string strFile = item.Label;
								if (lastApp != null) 
								{
									lastApp.LaunchFile(item);
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
							lastApp.Refresh();
							lastApp.DisplayFiles(null);
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
			{return lastApp.RefreshButtonVisible();}
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

		void DisplayApplications()
		{
			GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST ); 
			GUIControl.ClearControl(GetID, (int)Controls.CONTROL_THUMBS );
			foreach(AppItem app in apps )
			{
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
			string strObjects=String.Format("{0} {1}", apps.Count, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
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