using System;
using System.IO;
using System.Collections;
using System.Management;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;

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
	  CONTROL_MAKE_DIR				= 5,	 
	  CONTROL_RESET_SELECT		= 6,
		CONTROL_MARK_ALL				= 7,
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

	private struct file 
	{
		public string name;
		public long size;
		public string fullpath;
		public string path;
	}

	private ArrayList files = new ArrayList(); 
	private ArrayList selected = new ArrayList();
	private string tmpStr;
	private ArrayList currentExt=null;
	private string currentFolder=null;
	private string[] drives=new string[27];
	private string[] drivesCd=new string[27];
	private int driveCount=0;
	private int driveCdCount=0;
	private long actSize=0;
	private int fileCount=0;

	public GUIExplorer()
	{
	  //
	  // TODO: Add constructor logic here
	  //
	}
	public override int GetID 
	{
	  get { return WINDOW_STATUS; }
	  set { base.GetID = value; }
	}

	public override bool Init() 
	{
	  Log.Write("Start My Explorer");
		GetDrives(true);
		GetDrives(false);
	  return Load (GUIGraphicsContext.Skin+@"\myexplorer.xml");
	}

	public override void OnAction(Action action) 
	{
	  if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU) 
	  {
		GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_HOME);
		return;
	  }
	  base.OnAction(action);
	}

		public override bool OnMessage(GUIMessage message) 
		{
			switch ( message.Message ) 
			{  
				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
					base.OnMessage(message);
					GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(2200));
					GUIPropertyManager.SetProperty("#explorer_title",GUILocalizeStrings.Get(2200));
					GUIPropertyManager.SetProperty("#explorer_size"," ");
					currentState=States.STATE_MAIN;
					currentExt=Util.Utils.AudioExtensions;
					currentExt.AddRange(Util.Utils.PictureExtensions);
					currentExt.AddRange(Util.Utils.VideoExtensions);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_SELECT_DEST);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_COPY);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_DIR);
					return true;
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					//get sender control
					base.OnMessage(message);
					int iControl=message.SenderControlId;
					if (iControl==(int)Controls.CONTROL_SELECT_SOURCE) 
					{
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_SELECT_DEST);
						//GUIControl.DisableControl(GetID,(int)Controls.CONTROL_SELECT_SOURCE);
						currentState=States.STATE_SELECT_SOURCE;
						GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
						GUIPropertyManager.SetProperty("#explorer_title",GUILocalizeStrings.Get(2201));
						LoadDriveListControl(true);
						currentFolder="";
						actSize=0;
						return true;
					}
					if (iControl==(int)Controls.CONTROL_SELECT_DEST)
					{
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_COPY);
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_MAKE_DIR);
						//GUIControl.DisableControl(GetID,(int)Controls.CONTROL_SELECT_DEST);
						currentState=States.STATE_SELECT_DEST;
						GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
						GUIPropertyManager.SetProperty("#explorer_title",GUILocalizeStrings.Get(2202));
						LoadDriveListControl(false);
						currentFolder="";
						actSize=0;
						return true;
					}
					if (iControl==(int)Controls.CONTROL_COPY) // copy data
					{
						if (currentState==States.STATE_SELECT_DEST)
						{ 
							GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
							if (null==dlgYesNo) break;
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(2200)); 
							dlgYesNo.SetLine(2,GUILocalizeStrings.Get(2209));
							dlgYesNo.DoModal(GetID);
							if (!dlgYesNo.IsConfirmed) break; 
							int indx=currentFolder.IndexOf("\\\\");
							if (indx>0) 
							{
								currentFolder=currentFolder.Remove(indx,1);
							}
							foreach(file f in selected) 
							{
								Move(f.fullpath, f.name, currentFolder+"\\");
								Log.Write(" From {0} to {1}",f.fullpath,currentFolder+"\\"+f.name);
							}
							GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
							dlgOk.SetHeading(GUILocalizeStrings.Get(2200)); 
							dlgOk.SetLine(2,fileCount.ToString()+" "+GUILocalizeStrings.Get(2210));
							dlgOk.DoModal(GetID);
							fileCount=0;
							selected.Clear();
							currentState=States.STATE_MAIN;
							GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
							GUIPropertyManager.SetProperty("#explorer_size"," ");
							GUIControl.EnableControl(GetID,(int)Controls.CONTROL_SELECT_SOURCE);
							GUIControl.DisableControl(GetID,(int)Controls.CONTROL_SELECT_DEST);
							GUIControl.DisableControl(GetID,(int)Controls.CONTROL_COPY);
							GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_DIR);
						}
					}
					if (iControl==(int)Controls.CONTROL_MAKE_DIR) // copy data
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
							System.GC.Collect(); // collect some garbage
							string verStr = keyBoard.Text;
							GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
							if (null==dlgYesNo) break;
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(2200)); 
							dlgYesNo.SetLine(1,GUILocalizeStrings.Get(2207));
							dlgYesNo.SetLine(2,verStr+" "+GUILocalizeStrings.Get(2208));
							dlgYesNo.DoModal(GetID);
							if (!dlgYesNo.IsConfirmed) break; 
							string path = currentFolder+"\\"+verStr;
							try 
							{
								// Determine whether the directory exists.
								if (Directory.Exists(path)) 
								{
									Log.Write("That path exists already.");
									break;
								}
								DirectoryInfo di = Directory.CreateDirectory(path);
								Log.Write("The directory was created successfully at {0}.", Directory.GetCreationTime(path));
							}
							catch (Exception )
							{
								Log.Write("Error Make Dir");
							}
							LoadListControl(currentFolder,currentExt);
						}
					}
					if (iControl==(int)Controls.CONTROL_RESET_SELECT) // select Make Data page
					{
						GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
						if (null==dlgYesNo) break;
						dlgYesNo.SetHeading(GUILocalizeStrings.Get(2200)); 
						dlgYesNo.SetLine(2,GUILocalizeStrings.Get(2205));
						dlgYesNo.DoModal(GetID);
						if (!dlgYesNo.IsConfirmed) break; 
						fileCount=0;
						selected.Clear();
						currentState=States.STATE_MAIN;
						GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
						GUIPropertyManager.SetProperty("#explorer_size"," ");
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_SELECT_DEST);
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_COPY);
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_DIR);
						return true;
					}

					if (iControl==(int)Controls.CONTROL_MARK_ALL) // select Make Data page
					{
						int count = GUIControl.GetItemCount(GetID, (int)Controls.CONTROL_LIST_DIR);
						for (int i=0; i<count; i++) 
						{
							GUIListItem item = GUIControl.GetListItem(GetID, (int)Controls.CONTROL_LIST_DIR,i);
							if (!item.Label.StartsWith("\\..") && !item.Label.StartsWith("\\") && item.Label.Substring(1,1)!=":") 
							{
								if (currentState==States.STATE_SELECT_SOURCE) 
								{
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
									selected.Add(fl);

									foreach(file f in files) 
									{
										if (f.name==item.Label) 
										{
											actSize=actSize+f.size;
										}
									}
									fileCount++;
									tmpStr=fileCount.ToString()+ " Files "+CalcExt(actSize)+" ";
									GUIPropertyManager.SetProperty("#explorer_size",tmpStr);
								}
							}
						}
					}
					if (iControl==(int)Controls.CONTROL_LIST_DIR) // select List Dir
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);         
						int iItem=(int)msg.Param1;
						int iAction=(int)message.Param1;
						if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM) 
						{
							GUIListItem item = GUIControl.GetSelectedListItem(GetID, (int)Controls.CONTROL_LIST_DIR );
							if (item.Label.StartsWith(".."))  // go back folder
							{ 
								if (item.Path=="") 
									LoadDriveListControl(true);
								else
									LoadListControl(item.Path,currentExt);
							} 
							else if (item.IsFolder)  // is a folder
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
								if (currentState==States.STATE_SELECT_SOURCE) 
								{
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
									selected.Add(fl);

									foreach(file f in files) 
									{
										if (f.name==item.Label) 
										{
											actSize=actSize+f.size;
										}
									}
									fileCount++;
									tmpStr=fileCount.ToString()+ " Files "+CalcExt(actSize)+" ";
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
				if(!item.IsFolder) 
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

		private void LoadDriveListControl(bool cd) 
		{	
			currentFolder="";
			//clear the list
			GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
			if (cd==true) 
			{
				for (int i=0; i<driveCdCount; i++) 
				{
					GUIListItem pItem = new GUIListItem(drivesCd[i]);
					pItem.Path=drives[i];
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
		

		/// <summary>
		/// calculate KB,MB and GB View
		/// </summary>
		private string CalcExt(long m)
		{
			string lw="";
			if (m >= 1073741824) 
			{
				m = (m / (1024 * 1024 * 1024));
				lw=m.ToString()+" GB";
			} 
			else if (m >= 1048576 ) 
			{
				m = (m / (1024 * 1024));
				lw=m.ToString()+" MB";
			} 
			else if (m >= 1024 ) 
			{
				m = (m / 1024);
				lw=m.ToString()+" KB";
			}
			return lw;
		}
 
		void keyboard_TextChanged(int kindOfSearch,string data)
		{
			//
		}

		/// <summary>
		/// fills the drive array. 3=HD 5=CD
		/// </summary>
		private void GetDrives(bool cd) 
		{
			ManagementObjectSearcher query;
			ManagementObjectCollection queryCollection;
			System.Management.ObjectQuery oq;
			string stringMachineName = "localhost";
			string lw;
			int m;
			char d='C';
			for (int i=0; i<10; i++) 
			{
				m=0;
				lw=d+":";
				//Connect to the remote computer
				ConnectionOptions co = new ConnectionOptions();

				//Point to machine
				System.Management.ManagementScope ms = new System.Management.ManagementScope("\\\\" + stringMachineName + "\\root\\cimv2", co);

				oq = new System.Management.ObjectQuery("SELECT * FROM Win32_LogicalDisk WHERE DeviceID = '"+lw+"'");
				query = new ManagementObjectSearcher(ms,oq);
				queryCollection = query.Get();
				foreach ( ManagementObject mo in queryCollection) 
				{
					m=Convert.ToInt32(mo["DriveType"]);
				}
				if (cd==true) 
				{
					if (m==5) m=3;
					if (m==3 || m==4 || m==2) 
					{
						drivesCd[driveCdCount]=d+":\\";
						driveCdCount++;
					}
				} 
				else 
				{
					if (m==3 || m==4 || m==2) 
					{
						drives[driveCount]=d+":\\";
						driveCount++;
					}
				}
				d++;
			}
		}

		void Move(string source, string name, string destination) 
		{
			try 
			{
				FileInfo fi = new FileInfo(source);
				fi.MoveTo(destination+name);
			}
			catch (ArgumentNullException) 
			{
				Log.Write("MyExplorer Error: Path is a null reference.");
			}
			catch (System.Security.SecurityException) 
			{
				Log.Write("MyExplorer Error: The caller does not have the required permission.");
			}
			catch (ArgumentException) 
			{
				Log.Write("MyExplorer Error: Path is an empty string, contains only white spaces, or contains invalid characters.");    
			}
			catch (System.IO.IOException) 
			{
				Log.Write("MyExplorer Error: An attempt was made to move a directory to a different volume, or destDirName already exists."); 
			}
		}

  }
}
