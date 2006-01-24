/* 
 *	Copyright (C) 2005 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#region usings
using System;
using System.IO;
using System.Collections;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
#endregion

namespace MediaPortal.GUI.GUIExplorer
{
	/// <summary>
	/// Summary description for GUIExplorer
	/// </summary>
	public class GUIExplorer : GUIWindow 
	{
		public static int WINDOW_STATUS = 770;

		#region Private Enumerations

		enum Controls 
		{
			CONTROL_SELECT_SOURCE		= 2,	 
			CONTROL_SELECT_DEST			= 3,	 
			CONTROL_COPY						= 4,	 
			CONTROL_MOVE						= 5,	
			CONTROL_DELETE					= 6,
			CONTROL_MAKE_DIR				= 7,	 
			CONTROL_RESET_SELECT		= 8,
			CONTROL_MARK_ALL				= 9,
			CONTROL_TRASHCAN				= 10,
			CONTROL_LIST_DIR				= 20
		};

		enum States
		{
			STATE_MAIN					= 0,
			STATE_SELECT_SOURCE = 1,
			STATE_SELECT_DEST		= 2,
			STATE_COPY					= 3,
			STATE_MAKE_DIR			= 4,
			STATE_RESET_SELECT	= 5
		};

		private States currentState = States.STATE_MAIN;

		#endregion

		#region Private Variables
		private struct file 
		{
			public string name;
			public long size;
			public string fullpath;
			public string path;
		}

		string[] video = new string[20];		// video shares folder
		string[] vname = new string[20];		// video share names
		string[] sound = new string[20];		// sound shares folder
		string[] sname = new string[20];		// sound shares names
		string[] pictures = new string[20]; // pictures shares folder
		string[] pname = new string[20];		// pictures shares names

		private string tempFolder="";				// trashcan folder
		private bool showOnlyShares=false;	// shows only shares in destination folder
		private bool enableDelete=false;		// shows delete button
		private bool deleteImmed=false;			// enable immediate delete funtion
		private bool deleteTemp=false;			// enable trashcan

		private ArrayList files = new ArrayList(); 
		private ArrayList selected = new ArrayList();
		private string tmpStr;
		private ArrayList currentExt= new ArrayList();
		private string currentFolder=null;
		private string[] drives=new string[27];
		private string[] drivesCd=new string[27];
		private int driveCount=0;
		private int driveCdCount=0;
		private long actSize=0;
		private int fileCount=0;
		#endregion

		#region Constructor
		public GUIExplorer()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		#endregion
	
		#region Overrides		
		public override int GetID 
		{
			get { return WINDOW_STATUS; }
			set { base.GetID = value; }
		}

		public override bool Init() 
		{
			return Load (GUIGraphicsContext.Skin+@"\myexplorer.xml");
		}

		public override void OnAction(Action action) 
		{
			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU) 
			{
				GUIWindowManager.ShowPreviousWindow();  
				return;
			}
			base.OnAction(action);
		}

		public override bool OnMessage(GUIMessage message) 
		{
			switch ( message.Message ) 
			{  
				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:	// MyExplorer starts
					base.OnMessage(message);
					GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(2200));
					GUIPropertyManager.SetProperty("#explorer_title",GUILocalizeStrings.Get(2200));
					GUIPropertyManager.SetProperty("#explorer_size"," ");
					LoadShareSettings();										// loads showShares settings from XML
					driveCount=0;
					driveCdCount=0;
					GetDrives();														// loads all drives
					LoadSettings();													// loads all settings from XML
					ResetValues();																
					currentExt.Add("*");
					return true;
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					//get sender control
					base.OnMessage(message);
					int iControl=message.SenderControlId;
					if (iControl==(int)Controls.CONTROL_SELECT_SOURCE)		// select source
					{
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_SELECT_DEST);
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_COPY);
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MOVE);
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_DELETE);
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_DIR);
						currentState=States.STATE_SELECT_SOURCE;
						GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
						GUIPropertyManager.SetProperty("#explorer_title",GUILocalizeStrings.Get(2201));
						LoadDriveListControl(true);
						currentFolder="";
						actSize=0;
						return true;
					}
					if (iControl==(int)Controls.CONTROL_SELECT_DEST)		// select destination
					{
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_COPY);
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_MOVE);
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_MAKE_DIR);
						currentState=States.STATE_SELECT_DEST;
						GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
						GUIPropertyManager.SetProperty("#explorer_title",GUILocalizeStrings.Get(2202));
						LoadDriveListControl(false);
						currentFolder="";
						actSize=0;
						return true;
					}
					if (iControl==(int)Controls.CONTROL_TRASHCAN)			// select trashcan
					{
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_COPY);
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_MOVE);
						currentState=States.STATE_SELECT_SOURCE;
						GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
						GUIPropertyManager.SetProperty("#explorer_title",GUILocalizeStrings.Get(2202));
						LoadListControl(tempFolder,currentExt);
						currentFolder=tempFolder;
						actSize=0;
						return true;
					}
					if (iControl==(int)Controls.CONTROL_COPY || iControl==(int)Controls.CONTROL_MOVE) // select copy data
					{
						if (currentState==States.STATE_SELECT_DEST)
						{ 
							GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
							if (null==dlgYesNo) break;
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(2200)); 
							if (iControl==(int)Controls.CONTROL_COPY) 
							{
								dlgYesNo.SetLine(2,GUILocalizeStrings.Get(2209)); 
								GUIPropertyManager.SetProperty("#explorer_size",GUILocalizeStrings.Get(2211));
							} 
							else 
							{
								dlgYesNo.SetLine(2,GUILocalizeStrings.Get(2214));
								GUIPropertyManager.SetProperty("#explorer_size",GUILocalizeStrings.Get(2215));
							}
							dlgYesNo.DoModal(GetID);
							if (dlgYesNo.IsConfirmed) 
							{ 
								foreach(file f in selected) 
								{
									if (iControl==(int)Controls.CONTROL_COPY) 
									{
										Move(false,f.fullpath, f.name, currentFolder+"\\");
									} 
									else 
									{
										Move(true,f.fullpath, f.name, currentFolder+"\\");
									}
								}
								GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
								dlgOk.SetHeading(GUILocalizeStrings.Get(2200)); 
								if (iControl==(int)Controls.CONTROL_COPY) 
								{
									dlgOk.SetLine(2,fileCount.ToString()+" "+GUILocalizeStrings.Get(2210));
								} 
								else 
								{
									dlgOk.SetLine(2,fileCount.ToString()+" "+GUILocalizeStrings.Get(2216));
								}
								dlgOk.DoModal(GetID);
							}
							ResetValues();
							LoadListControl(currentFolder,currentExt);
						}
					}
					if (iControl==(int)Controls.CONTROL_MAKE_DIR) // select make directory
					{
						if (currentState==States.STATE_SELECT_DEST) 
						{
							int activeWindow=(int)GUIWindowManager.ActiveWindow;
							GUIPropertyManager.SetProperty("#explorer_title",GUILocalizeStrings.Get(2204));
							VirtualSearchKeyboard keyBoard=(VirtualSearchKeyboard)GUIWindowManager.GetWindow(1001);
							keyBoard.Text = "";
							keyBoard.Reset();
							keyBoard.TextChanged+=new MediaPortal.Dialogs.VirtualSearchKeyboard.TextChangedEventHandler(keyboard_TextChanged); // add the event handler
							keyBoard.DoModal(activeWindow); // show it...
							keyBoard.TextChanged-=new MediaPortal.Dialogs.VirtualSearchKeyboard.TextChangedEventHandler(keyboard_TextChanged);	// remove the handler			
							string verStr = keyBoard.Text;
							GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
							if (null==dlgYesNo) break;
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(2200)); 
							dlgYesNo.SetLine(1,GUILocalizeStrings.Get(2207));
							dlgYesNo.SetLine(2,verStr+" "+GUILocalizeStrings.Get(2208));
							dlgYesNo.DoModal(GetID);
							if (dlgYesNo.IsConfirmed) 
							{
								string path = currentFolder+"\\"+verStr;
								try 
								{
									// Determine whether the directory exists.
									if (Directory.Exists(path)) 
									{
										GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
										dlgOk.SetHeading(GUILocalizeStrings.Get(2200)); 
										dlgOk.SetLine(2,GUILocalizeStrings.Get(2224));
										dlgOk.DoModal(GetID);
									} 
									else 
									{
										DirectoryInfo di = Directory.CreateDirectory(path);
										GUIDialogOK dlgOk2 = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
										dlgOk2.SetHeading(GUILocalizeStrings.Get(2200)); 
										dlgOk2.SetLine(2,GUILocalizeStrings.Get(2224));
										dlgOk2.DoModal(GetID);
									}
								}
								catch (Exception )
								{
									Log.Write("Error Make Dir");
								}
								LoadListControl(currentFolder,currentExt);
							}
						}
					}
					if (iControl==(int)Controls.CONTROL_RESET_SELECT) // select reset all selections
					{
						GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
						if (null==dlgYesNo) break;
						dlgYesNo.SetHeading(GUILocalizeStrings.Get(2200)); 
						dlgYesNo.SetLine(2,GUILocalizeStrings.Get(2205));
						dlgYesNo.DoModal(GetID);
						if (dlgYesNo.IsConfirmed)
						{ 
							ResetValues();
						}
						return true;
					}

					if (iControl==(int)Controls.CONTROL_MARK_ALL) // select mark all
					{
						if (currentState==States.STATE_SELECT_SOURCE) 
						{
							GUIControl.EnableControl(GetID,(int)Controls.CONTROL_SELECT_DEST); // you can select destination only when a file is selected
							if(enableDelete==true)	
							{
								GUIControl.EnableControl(GetID,(int)Controls.CONTROL_DELETE);	// you can delete files only when a file is selected
							}
							int count = GUIControl.GetItemCount(GetID, (int)Controls.CONTROL_LIST_DIR);
							for (int i=0; i<count; i++) 
							{
								GUIListItem item = GUIControl.GetListItem(GetID, (int)Controls.CONTROL_LIST_DIR,i);
								if (item.IconImage!="check-box.png" && !item.Label.StartsWith("\\..") && !item.Label.StartsWith("\\") && item.Label.Substring(1,1)!=":") 
								{
									item.IconImage = "check-box.png";
									file fl = new file();
									fl.name=item.Label;
									fl.fullpath=currentFolder+"\\"+item.Label;
									fl.path=currentFolder;								

									foreach(file f in files) 
									{
										if (f.name==item.Label) 
										{
											actSize=actSize+f.size;
											fl.size=f.size;
											break;
										}
									}
									selected.Add(fl);
									fileCount++;
									tmpStr=fileCount.ToString()+ " Files "+Utils.GetSize(actSize)+" ";
									GUIPropertyManager.SetProperty("#explorer_size",tmpStr);
								}
							}
						}
					}
					if (iControl==(int)Controls.CONTROL_DELETE) // delete selected files
					{
						GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
						if (null==dlgYesNo) break;
						dlgYesNo.SetHeading(GUILocalizeStrings.Get(2200)); 
						dlgYesNo.SetLine(2,GUILocalizeStrings.Get(2221)); 
						GUIPropertyManager.SetProperty("#explorer_size",GUILocalizeStrings.Get(2211));
						dlgYesNo.DoModal(GetID);
						if (dlgYesNo.IsConfirmed) 
						{ 
							foreach(file f in selected) 
							{
								if(deleteImmed==true)
								{
									Utils.FileDelete(f.fullpath);
								} 
								else 
								{
									if(deleteTemp==true) 
									{ 
										try 
										{
											if(currentFolder==tempFolder) 
											{
												Utils.FileDelete(f.fullpath);
											}
											else
											{
												FileInfo fi = new FileInfo(f.fullpath);
												fi.MoveTo(tempFolder+"\\"+f.name);
											}
										}
										catch (Exception)
										{
											Log.Write("MyExplorer Delete Error: {0} | {1}",f.fullpath,tempFolder);
										}
									}
								}
							}
							GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
							dlgOk.SetHeading(GUILocalizeStrings.Get(2200)); 
							dlgOk.SetLine(2,fileCount.ToString()+" "+GUILocalizeStrings.Get(2222));
							dlgOk.DoModal(GetID);
						}
						ResetValues();
						LoadListControl(currentFolder,currentExt);
					}
					if (iControl==(int)Controls.CONTROL_LIST_DIR) // select list dir
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);         
						int iItem=(int)msg.Param1;
						int iAction=(int)message.Param1;
						if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM) 
						{
							GUIListItem item = GUIControl.GetSelectedListItem(GetID, (int)Controls.CONTROL_LIST_DIR );
							if (item.Label.StartsWith(".."))				// go back folder
							{ 
								if (item.Path=="") 
									LoadDriveListControl(true);
								else
									LoadListControl(item.Path,currentExt);
							}
							else if(item.Label.StartsWith("("))
							{
								LoadListControl(item.Label[1] + @":\", currentExt);
							}
							else if (item.Label.StartsWith("["))		// is a share
							{ 
								String shareName=item.Label.Substring(1);
								shareName=shareName.Substring(0,shareName.Length-1);
								for (int i=0; i<20; i++) 
								{
									if (pname[i]==shareName)
									{
										currentFolder=pictures[i];
										LoadListControl(currentFolder,currentExt);
										break;
									}
									if (sname[i]==shareName)
									{
										currentFolder=sound[i];
										LoadListControl(currentFolder,currentExt);
										break;
									}
									if (vname[i]==shareName)
									{
										currentFolder=video[i];
										LoadListControl(currentFolder,currentExt);
										break;
									}
								}
								LoadListControl(currentFolder,currentExt);
							} 
							else if (item.IsFolder)								// is a folder
							{		
								LoadListControl(item.Path,currentExt);
							} 
							else if (item.Label.Substring(1,1)==":")  // is a drive
							{ 
								currentFolder=item.Label;
								if (currentFolder!=String.Empty)
									LoadListControl(currentFolder,currentExt);
								else
									LoadDriveListControl(true);
							} 							
							else 
							{
								if (currentState==States.STATE_SELECT_SOURCE)	// mark files only in source select mode
								{
									GUIControl.EnableControl(GetID,(int)Controls.CONTROL_SELECT_DEST); // you can select destination only when a file is selected
									if(enableDelete==true)	
									{
										GUIControl.EnableControl(GetID,(int)Controls.CONTROL_DELETE);	// you can delete files only when a file is selected
									}
									if (item.IconImage=="check-box.png")  // if file selected then unselect now
									{
										item.FreeIcons();
										int indx=0;
										int indxm=-1;
										long s=0;
										foreach(file fil in selected) 
										{
											if (item.Label==fil.name) 
											{
												indxm=indx;
												s=fil.size;
												break;
											}
											indx++;
										}
										if (indxm>=0)
										{
											selected.RemoveAt(indxm);
											actSize=actSize-s;
											fileCount--;
										}
									}
									else 
									{	// select file
										item.IconImage = "check-box.png";	
										int indx=currentFolder.IndexOf("\\\\");
										if (indx>0) 
										{
											currentFolder=currentFolder.Remove(indx,1);
										}
										file fl = new file();
										fl.name=item.Label;
										fl.fullpath=currentFolder+"\\"+item.Label;
										fl.path=currentFolder;

										foreach(file f in files) 
										{
											if (f.name==item.Label) 
											{
												actSize=actSize+f.size;
												fl.size=f.size;
												break;
											}
										}
										selected.Add(fl);
										fileCount++;
									}
									tmpStr=fileCount.ToString()+ " Files "+Utils.GetSize(actSize)+" ";
									GUIPropertyManager.SetProperty("#explorer_size",tmpStr);
								}
							}
						}
						return true;
					}
					return true;
			}
			return base.OnMessage (message);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Reset all Values to start settings
		/// </summary>
		private void ResetValues()
		{
			fileCount=0;
			selected.Clear();
			currentState=States.STATE_MAIN;
			GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
			GUIPropertyManager.SetProperty("#explorer_size"," ");
			if(deleteTemp==false) 
			{
				GUIControl.HideControl(GetID,(int)Controls.CONTROL_TRASHCAN);
			}
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_SELECT_DEST);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_COPY);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MOVE);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_DELETE);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_DIR);
			GUIControl.EnableControl(GetID,(int)Controls.CONTROL_SELECT_SOURCE);
		}

		/// <summary>
		/// Loads files from folder in a list control
		/// </summary>
		private void LoadListControl(string folder,ArrayList Exts) 
		{	
			//clear the list
			folder=Utils.RemoveTrailingSlash(folder);
			file f = new file();
			GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
			VirtualDirectory Directory;
			ArrayList itemlist;
			Directory = new VirtualDirectory();
			Directory.SetExtensions(Exts);
			itemlist = Directory.GetDirectory(folder);
				
			foreach (GUIListItem item in itemlist) 
			{
				if(!item.IsFolder) // if item a folder
				{
					GUIListItem pItem = new GUIListItem(item.FileInfo.Name);
					pItem.IsFolder=false;
					pItem.Path=String.Format(@"{0}\{1}", folder,item.FileInfo.Name);
					GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST_DIR,pItem);
					f.name=item.FileInfo.Name;
					f.size=item.FileInfo.Length;
					files.Add(f);
				} 
				else 
				{
					GUIListItem pItem = new GUIListItem(item.Label);
					pItem.IsFolder=true;
					pItem.Path=String.Format(@"{0}\{1}", folder,item.Label);
					if (item.Label=="..")
					{
						string prevFolder="";
						int pos=folder.LastIndexOf(@"\");
						if (pos>=0) prevFolder=folder.Substring(0,pos);
						pItem.Path=prevFolder;
					}
					Utils.SetDefaultIcons(pItem);
					GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST_DIR,pItem);
				}
			}
			string strObjects =String.Format("{0} {1}",GUIControl.GetItemCount(GetID,(int)Controls.CONTROL_LIST_DIR).ToString(), GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);

			currentFolder=folder;
		}

		/// <summary>
		/// Loads drivelist and shares in a list control
		/// </summary>
		private void LoadDriveListControl(bool cd) 
		{	
			currentFolder="";
			//clear the list
			GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);

			cd = false;

			if (cd==true) 
			{
				for (int i=0; i<driveCdCount; i++) 
				{
					GUIListItem pItem = new GUIListItem(drivesCd[i]);
					pItem.Path=drivesCd[i];
					pItem.IsFolder=true;
					Utils.SetDefaultIcons(pItem);
					GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST_DIR,pItem);
				}
			}
			else
			{
				for (int i=0; i<driveCount; i++) 
				{
					GUIListItem pItem = new GUIListItem(drives[i]);
					pItem.Path=drives[i];
					pItem.IsFolder=true;
					Utils.SetDefaultIcons(pItem);
					GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST_DIR,pItem);
				}
			}

			string strObjects =String.Format("{0} {1}",GUIControl.GetItemCount(GetID,(int)Controls.CONTROL_LIST_DIR).ToString(), GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
		}
		
 
		void keyboard_TextChanged(int kindOfSearch,string data)
		{
			//
		}

		enum DriveType
		{
			Removable = 2,
			Fixed = 3,
			RemoteDisk = 4,
			CD = 5,
			DVD = 5,
			RamDisk = 6
		}

		/// <summary>
		/// fills the drive array. 
		/// when showOnlyShares==false then the array drives contains all drives witout CD. 
		/// array drivesCd contains all drives 
		/// </summary>
		void GetDrives()
		{
			foreach(string drive in Environment.GetLogicalDrives())
			{
				DriveType driveType = (DriveType)Util.Utils.getDriveType(drive);
				string name="";

				switch(driveType)
				{
					case DriveType.Removable:
						name=String.Format("({0}:) Removable",drive.Substring(0, 1).ToUpper());								
						break;
					case DriveType.Fixed:
						name=String.Format("({0}:) Fixed",drive.Substring(0, 1).ToUpper());
						break;
					case DriveType.RemoteDisk:
						name=String.Format("({0}:) Remote",drive.Substring(0, 1).ToUpper());
						break;
					case DriveType.DVD:
						name=String.Format("({0}:) CD/DVD",drive.Substring(0, 1).ToUpper());
						break;
					case DriveType.RamDisk:
						name=String.Format("({0}:) Ram",drive.Substring(0, 1).ToUpper());
						break;
				}

				if(showOnlyShares == false)
				{
					drives[driveCount]=name;
					driveCount++;
				}

				if(driveType == DriveType.DVD)
				{
					drivesCd[driveCdCount]=name;
					driveCdCount++;
				}
			}
		}

		/// <summary>
		/// Moves or Copy a file
		/// if mc==false copy a file otherwise move a file
		/// </summary>
		
		void Move(bool mc, string source, string name, string destination) 
		{
			bool doNot=false;
			try 
			{
				if (System.IO.File.Exists(destination+name))
				{
					GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
					dlgYesNo.SetHeading(GUILocalizeStrings.Get(2200)); 
					dlgYesNo.SetLine(1,name);
					dlgYesNo.SetLine(2,GUILocalizeStrings.Get(2217));
					dlgYesNo.SetLine(3,GUILocalizeStrings.Get(2218));
					dlgYesNo.DoModal(GetID);
					if (dlgYesNo.IsConfirmed) 
					{
						doNot=false;
						Utils.FileDelete(destination+name);
					}
					else 
					{
						doNot=true;
					}
				}
				if (doNot==false) 
				{
					FileInfo fi = new FileInfo(source);
					if (mc)
					{
						fi.MoveTo(destination+name);
					} 
					else 
					{
						fi.CopyTo(destination+name,false);
					}
				}
			}
			catch (Exception) 
			{
				Log.Write("MyExplorer Error: from {0} to {1} MC:{2}",source,destination+name,mc);
			}
		}

		/// <summary>
		/// Loads only the ShowOnlyShare status
		/// </summary>
		private void LoadShareSettings() 
		{
			using(MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml")) 
			{
				showOnlyShares=xmlreader.GetValueAsBool("myexplorer","show_only_shares",false);
			}
		}

		/// <summary>
		/// Loads all Settings from MediaPortal.xml
		/// </summary>
		private void LoadSettings() 
		{
			using(MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml")) 
			{
				tempFolder=xmlreader.GetValueAsString("myexplorer","temp_folder","");
				enableDelete=xmlreader.GetValueAsBool("myexplorer","enable_delete",false);
				deleteImmed=xmlreader.GetValueAsBool("myexplorer","delete_immediately",false);
				deleteTemp=xmlreader.GetValueAsBool("myexplorer","delete_temp",false);
				for (int i=0; i<20; i++) 
				{
					sound[i]=xmlreader.GetValueAsString("music","sharepath"+i.ToString()," ").Trim();		
					sname[i]=xmlreader.GetValueAsString("music","sharename"+i.ToString()," ").Trim();		
					vname[i]=xmlreader.GetValueAsString("movies","sharename"+i.ToString()," ").Trim();
					video[i]=xmlreader.GetValueAsString("movies","sharepath"+i.ToString()," ").Trim();
					pname[i]=xmlreader.GetValueAsString("pictures","sharename"+i.ToString()," ").Trim();
					pictures[i]=xmlreader.GetValueAsString("pictures","sharepath"+i.ToString()," ").Trim();

					if (pname[i].StartsWith("CD/")==false && pictures[i]!="") 
					{
						drives[driveCount]="["+pname[i]+"]";
						driveCount++;
						drivesCd[driveCdCount]="["+pname[i]+"]";
						driveCdCount++;
					}
					if (vname[i].StartsWith("CD/")==false && video[i]!="") 
					{
						drives[driveCount]="["+vname[i]+"]";
						driveCount++;
						drivesCd[driveCdCount]="["+vname[i]+"]";
						driveCdCount++;
					}
					if (sname[i].StartsWith("CD/")==false && sound[i]!="") 
					{
						drives[driveCount]="["+sname[i]+"]";
						driveCount++;
						drivesCd[driveCdCount]="["+sname[i]+"]";
						driveCdCount++;
					}
				}
			}
		}
		#endregion
	}
}
