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

#region Usings
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Ripper;
using MediaPortal.TV.Recording;
using Core.Util;
#endregion

namespace MediaPortal.GUI.GUIBurner
{
  /// <summary>
  /// Summary description for GUIBurner.
  /// </summary>
	public class GUIBurner : GUIWindow 
	{
		public static int WINDOW_STATUS = 760;

		#region Private Enumerations
		enum Controls 
		{
			CONTROL_BACK					  = 2,
		
			CONTROL_VIDEO						= 2,
			CONTROL_AUDIO						= 3,
			CONTROL_DATA						= 4,
			CONTROL_CD_INFO					= 5,
			CONTROL_FORMAT_RD				= 6,
			CONTROL_EJECT_CD				= 7,

			CONTROL_CONVERT					= 3,
			CONTROL_CUT							= 4,
			CONTROL_MAKE_VIDEO_CD		= 5,	 
			CONTROL_MAKE_VIDEO_DVD	= 6,

			CONTROL_CONVERT_DVR			= 3,
			CONTROL_CONVERT_DIVX		= 4,
			CONTROL_CONVERT_MPEG4		= 5,	 
			
			CONTROL_MAKE_AUDIO			= 3,	 
			CONTROL_MAKE_MP3_CD			= 4,	 
			CONTROL_MAKE_MP3_DVD		= 5,	 

			CONTROL_COPY_CD_DVD			= 3,	 
			CONTROL_MAKE_DATA_CD		= 4,
			CONTROL_MAKE_DATA_DVD		= 5,
			CONTROL_MAKE_MP_BACKUP  = 6,

			CONTROL_MARK_ALL				= 12,

			CONTROL_LIST_DIR				= 20,
			CONTROL_LIST_COPY				= 30,
			CONTROL_CD_DETAILS			= 50
		};

		enum States
		{
			STATE_MAIN						= 0,
			STATE_COPY_CDDVD			= 1,
			STATE_MAKE_AUDIO			= 2,
			STATE_MAKE_DATA_CD		= 3,
			STATE_MAKE_DATA_DVD		= 4,
			STATE_MAKE_VIDEO_CD		= 5,
			STATE_MAKE_VIDEO_DVD	= 6,
			STATE_CONVERT_DVR			= 7,
			STATE_DISK_INFO				= 8,
			STATE_VIDEO						= 9,
			STATE_AUDIO						= 10,
			STATE_DATA						= 11,
			STATE_CONVERT					= 12,
			STATE_MP3_CD					= 13,
			STATE_MP3_DVD					= 14,
			STATE_MAKE_BACKUP			= 15
	};

		enum BurnTypes
		{
			DATA_CD			= 0,
			DATA_DVD		= 1,
			MP3_CD			= 2,
			MP3_DVD			= 3,
			DIVX_CD			= 4,
			DIVX_DVD		= 5,
			MP_BACKUP		= 6,
			AUDIO_CD		= 7
	};
		private BurnTypes burnType = BurnTypes.DATA_CD;
		private States currentState = States.STATE_MAIN;

		#endregion

		#region Private Variables

		private struct file 
		{
			public string name;
			public long size;
			public string path;
		}
		
		private struct convFile 
		{
			public string name;
			public long size;
			public long oldSize;
			public string path;
		}

		convFile[] cFiles = new convFile[100];
		private	XPBurn.XPBurnCD burnClass; 
		
		string[] video = new string[50];
		string[] vname = new string[50];
		string[] sound = new string[50];
		string[] sname = new string[50];
		string[] pictures = new string[50];
		string[] pname = new string[50];

		private string recordpath1="";  // for TV card 1
		private string recordpath2="";	// for TV card 2
		private int recordCards=0;
		private int recorder;
		private ArrayList files = new ArrayList();
		private ArrayList backupFiles = new ArrayList();
		private string tmpFolder;
		private string tmpStr;
		private ArrayList currentExt=null;
		private string currentFolder=null;
		private string[] drives=new string[50];
		private int driveCount=0;
		private long actSize=0;
		private long cdSize=681574400;
		private long dvdSize=5046586572;
		private int perc=0;
		private long max=681574400;
		private bool fastFormat;
		private bool isBurner=true;
		private	bool convertDVR;
		private	bool deleteDVRSrc;
		private bool changeTVDatabase;
		private bool dateTimeFolder;
		private bool copyMpegPath=false;
		private bool deleteDatabase=false;
		private string mpegPath="";
		private int maxAutoFiles=0;
		private	BurnerThread bt = new BurnerThread();
		static ArrayList dvr_extensions	= new ArrayList();
		static ArrayList mp3_extensions	= new ArrayList();
		public static int soundFileSize = 0;
		private bool convertAuto;
		private System.Windows.Forms.Timer convertTimer = new System.Windows.Forms.Timer();
		private static long lStartTime=0;


		// Convert to short pathnames
		// (madlldlib)

		[DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
		[return:MarshalAs(UnmanagedType.U4)]
		public static extern int 
			GetShortPathName(
			[MarshalAs(UnmanagedType.LPTStr)] 
			string inputFilePath,
			[MarshalAs(UnmanagedType.LPTStr)] 
			StringBuilder outputFilePath,
			[MarshalAs(UnmanagedType.U4)] 
			int bufferSize);
		#endregion
		
		#region Constructor
		public GUIBurner()
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
			InitializeConvertTimer();
			return Load (GUIGraphicsContext.Skin+@"\myburner.xml");
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
				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
					base.OnMessage(message);
					driveCount=0;
					GetDrives();
					LoadSettings();
					GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(2100));
					GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2100));
					GUIPropertyManager.SetProperty("#burner_perc","-5");
					GUIPropertyManager.SetProperty("#burner_size"," ");
					GUIPropertyManager.SetProperty("#burner_info"," ");
					GUIPropertyManager.SetProperty("#convert_info"," ");
					actSize=0;
					currentState=States.STATE_MAIN;
					UpdateButtons();
					return true;
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					//get sender control
					base.OnMessage(message);
					int iControl=message.SenderControlId;
					if (iControl==(int)Controls.CONTROL_VIDEO) 
					{
						switch (currentState) 
						{
							case States.STATE_MAIN :							// If Main change Folder to Video
								currentState=States.STATE_VIDEO;
								UpdateButtons();
								break;
							case States.STATE_VIDEO :
								currentState=States.STATE_MAIN;			// If Video change Folder to Main
								UpdateButtons();
								break;
							case States.STATE_AUDIO :
								currentState=States.STATE_MAIN;			// If Audio change Folder to Main
								UpdateButtons();
								break;
							case States.STATE_DATA :
								currentState=States.STATE_MAIN;			// If Data change Folder to Main
								UpdateButtons();
								break;
//							case States.STATE_CONVERT :
//								currentState=States.STATE_VIDEO;		// If Convert change Folder to Video
//								UpdateButtons();
//								break;
							case States.STATE_DISK_INFO :					// If Disk Info change Folder to Main
								currentState=States.STATE_MAIN;		
								UpdateButtons();
								break;
							case States.STATE_MAKE_BACKUP :				// If MP Backup change Folder to Main
								currentState=States.STATE_MAIN;		
								UpdateButtons();
								break;
							
							default:
								currentState=States.STATE_MAIN;			// default goto main
								UpdateButtons();
								break;
						}
						return true;
					}
					if (iControl==(int)Controls.CONTROL_AUDIO) 
					{
						switch (currentState) 
						{
							case States.STATE_MAIN :
								currentState=States.STATE_AUDIO;
								UpdateButtons();
								break;
							case States.STATE_AUDIO :
								currentState=States.STATE_MAKE_AUDIO;		
								ShowList();
								break;
							case States.STATE_VIDEO :
								currentState=States.STATE_CONVERT;
								ShowList();
								break;
							case States.STATE_CONVERT :
								ConvertDvrMs();
								break;
							case States.STATE_MAKE_DATA_CD :
								burnType = BurnTypes.DATA_CD;
								BurnCD(burnType);
								break;
							case States.STATE_MAKE_AUDIO :
								burnType = BurnTypes.AUDIO_CD;
								BurnCD(burnType);
								break;
							case States.STATE_MAKE_DATA_DVD :
								burnType = BurnTypes.DATA_DVD;
								BurnCD(burnType);
								break;
							case States.STATE_MAKE_BACKUP :
								burnType = BurnTypes.MP_BACKUP;
								BurnCD(burnType);
								break;
							case States.STATE_MP3_CD :
								burnType = BurnTypes.MP3_CD;
								BurnCD(burnType);
								break;
							case States.STATE_MP3_DVD :
								burnType = BurnTypes.MP3_DVD;
								BurnCD(burnType);
								break;
						}
						return true;
					}
					if (iControl==(int)Controls.CONTROL_DATA) 
					{
						switch (currentState) 
						{
							case States.STATE_MAIN :
								currentState=States.STATE_DATA;
								UpdateButtons();
								break;
							case States.STATE_DATA :
								currentState=States.STATE_MAKE_DATA_CD;
								ShowList();
								break;
							case States.STATE_AUDIO :
								currentState=States.STATE_MP3_CD;
								ShowList();
								break;
						}
						return true;
					}
					if (iControl==(int)Controls.CONTROL_CD_INFO) 
					{
						switch (currentState) 
						{
							case States.STATE_MAIN :
								currentState=States.STATE_DISK_INFO;
								ShowList();
								CdInfo();
								break;
							case States.STATE_DATA :
								currentState=States.STATE_MAKE_DATA_DVD;
								ShowList();
								break;
							case States.STATE_AUDIO :
								currentState=States.STATE_MP3_DVD;
								ShowList();
								break;
						}
						return true;
					}
					if (iControl==(int)Controls.CONTROL_FORMAT_RD) 
					{
						switch (currentState) 
						{
							case States.STATE_MAIN :
								CdRwFormat();
								break;
							case States.STATE_DATA :
								currentState=States.STATE_MAKE_BACKUP;
								ShowList();
								break;
						}
						return true;
					}
					if (iControl==(int)Controls.CONTROL_EJECT_CD) 
					{
						switch (currentState) 
						{
							case States.STATE_MAIN :
								if (isBurner==true) 
								{
									burnClass.Eject();
								}
								break;
						}
						return true;
					}

					if (iControl==(int)Controls.CONTROL_LIST_COPY) // select Copy Dir
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);         
						int iItem=(int)msg.Param1;
						int iAction=(int)message.Param1;
						files.Clear();

						if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM) 
						{
							bool sel=true;
							GUIListItem item = GUIControl.GetSelectedListItem(GetID, (int)Controls.CONTROL_LIST_COPY );
							int count = GUIControl.GetItemCount(GetID, (int)Controls.CONTROL_LIST_COPY);
							for (int i=0; i<count; i++) 
							{
								GUIListItem cItem = GUIControl.GetListItem(GetID, (int)Controls.CONTROL_LIST_COPY,i);
								if (cItem.Label==item.Label) 
								{
									if (cItem.Path==item.Path) 
									{
										sel=false;
									}
								}
								if (sel) 
								{
									file fl = new file();
									fl.name=cItem.Label;
									fl.path=cItem.Path;
									fl.size=cItem.FileInfo.Length;
									files.Add(fl);
								}
								sel=true;
							}
							actSize=0;
							GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY );
							foreach(file f in files) 
							{
								GUIListItem pItem = new GUIListItem(f.name);
								FileInformation fi = new FileInformation();
								fi.Length=f.size;
								actSize=actSize+f.size;
								fi.Name=f.name;
								pItem.Path=f.path;
								pItem.FileInfo=(FileInformation)fi;
								GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST_COPY,pItem);
							}
							if (actSize>0) 
								perc=Convert.ToInt16(actSize/(max/100d)); 
							else 
								perc=0;
							tmpStr=Utils.GetSize(actSize)+" ";
							GUIPropertyManager.SetProperty("#burner_size",tmpStr);
							GUIPropertyManager.SetProperty("#burner_perc",perc.ToString());
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
							if (currentState!=States.STATE_MAKE_BACKUP) 
							{
								GUIListItem item = GUIControl.GetSelectedListItem(GetID, (int)Controls.CONTROL_LIST_DIR );
								if (item.Label.StartsWith(".."))				// go back folder
								{ 
									if (item.Path=="") 
										LoadDriveListControl();
									else
										LoadListControl(item.Path,currentExt);
								} 
								else if (item.Label.StartsWith("["))		// is a share
								{ 
									String shareName=item.Label.Substring(1);
									shareName=shareName.Substring(0,shareName.Length-1);
									if (shareName==GUILocalizeStrings.Get(2133)) 
									{
										currentFolder=recordpath1;
										LoadListControl(currentFolder,currentExt);
									} 
									if (shareName==GUILocalizeStrings.Get(2144)) // if two tv cards installed
									{
										currentFolder=recordpath1;
										LoadListControl(currentFolder,currentExt);
									} 
									if (shareName==GUILocalizeStrings.Get(2145)) 
									{
										currentFolder=recordpath2;
										LoadListControl(currentFolder,currentExt);
									} 
									else 
									{
										for (int i=0; i< 50; i++) 
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
										LoadDriveListControl();
								} 							
								else 
								{
									int indx=currentFolder.IndexOf("\\\\");
									if (indx>0) 
									{
										currentFolder=currentFolder.Remove(indx,1);
									}
									GUIListItem pItem = new GUIListItem(item);
									pItem.Path=currentFolder;	
									bool isdoub=false;
									int count = GUIControl.GetItemCount(GetID, (int)Controls.CONTROL_LIST_COPY);
									for (int i=0; i<count; i++) 
									{
										GUIListItem cItem = GUIControl.GetListItem(GetID, (int)Controls.CONTROL_LIST_COPY,i);
										if (cItem.Label==pItem.Label) 
										{
											if (cItem.Path==pItem.Path) 
											{
												isdoub=true;
											}
										}
									}
									if (isdoub==false) 
									{
										if (currentState==States.STATE_MAKE_AUDIO) 
										{
											string outName=System.IO.Path.ChangeExtension(pItem.FileInfo.Name,".wav");
											GUIPropertyManager.SetProperty("#burner_size","MP3->WAV  "+outName);
											
											makeWav(pItem.Path+"\\"+pItem.FileInfo.Name,tmpFolder+"\\"+outName);
											pItem.Label=outName;
											pItem.FileInfo.Name=outName;
											pItem.Path=tmpFolder;
											FileInfo fi = new FileInfo(tmpFolder+"\\"+outName);		
											pItem.FileInfo.Length = (int)fi.Length;
										} 
										GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST_COPY,pItem);
										actSize=actSize+pItem.FileInfo.Length;
										if (actSize>0) 
											perc=Convert.ToInt16(actSize/(max/100d)); 
										else 
											perc=0;
										tmpStr=Utils.GetSize(actSize)+" ";
										GUIPropertyManager.SetProperty("#burner_size",tmpStr);
										GUIPropertyManager.SetProperty("#burner_perc",perc.ToString());
									}
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

		#region Audio Functions
		// Decoding callback (madlldlib)
		
		private static bool	ReportStatusMad(uint frameCount, uint byteCount, ref MadlldlibWrapper.mad_header mh, bool kill) 
		{
			int perc = (int)(((float)byteCount/(float)soundFileSize)*100);
			long lDiff = (DateTime.Now.Ticks - lStartTime)/10000;
			if (lDiff>500) 
			{
				lStartTime = DateTime.Now.Ticks;
				GUIPropertyManager.SetProperty("#burner_perc",perc.ToString());
				GUIWindowManager.Process();
			}
			return true;
		}


		private void makeWav(string inputFile,string outputFile)
		{
			lStartTime = DateTime.Now.Ticks;
			StringBuilder inputFilePath = new StringBuilder();
			StringBuilder outputFilePath = new StringBuilder();
			const int MAX_STRLEN = 260;	

			MadlldlibWrapper.Callback defaultCallback = new MadlldlibWrapper.Callback(ReportStatusMad);
			// Convert to short pathnames

			inputFilePath.Capacity = MAX_STRLEN;
			outputFilePath.Capacity = MAX_STRLEN;
			GetShortPathName(inputFile, inputFilePath, MAX_STRLEN);
			GetShortPathName(outputFile, outputFilePath, MAX_STRLEN);			

			// Assign if returned path is not zero:

			if (inputFilePath.Length > 0) 
				inputFile = inputFilePath.ToString();

			if (outputFilePath.Length > 0)
				outputFile = outputFilePath.ToString();			
								
			// Determine file size
			FileInfo fi = new FileInfo(inputFile);		
			soundFileSize = (int)fi.Length;
		
			// status/error message reporting. 
			// String length must be set 
			// explicitly

			StringBuilder status = new StringBuilder(); 
			status.Capacity=256;				
			
			// call the decoding function
			try 
			{
				int st=MadlldlibWrapper.DecodeMP3(inputFile, outputFile,MadlldlibWrapper.DEC_WAV, status, defaultCallback);
			} 
			catch(Exception) 
			{
				Log.Write("Error converting MP3");
			}
			// this prevents garbage collection
			// from occurring on callback

			GC.KeepAlive(defaultCallback);				
		}
		#endregion

		#region Private Methods

		private void ConvertDvrMs()
		{
			if (convertDVR==true && bt.isConverting==false) //Convert Video Files
			{
				int fCount=0;
				bt.ClearFiles();
				bt.deleteDvrMsSrc=deleteDVRSrc;
				bt.changeDatabase=changeTVDatabase;
				bt.copyMpeg=copyMpegPath;
				bt.deleteDatabase=deleteDatabase;
		    bt.mpegPath=mpegPath;
				int count = GUIControl.GetItemCount(GetID, (int)Controls.CONTROL_LIST_COPY);
				for (int i=0; i<count; i++) 
				{
					GUIListItem cItem = GUIControl.GetListItem(GetID, (int)Controls.CONTROL_LIST_COPY,i);
					string ext=System.IO.Path.GetExtension(cItem.Label);
					if (ext.ToLower() !=".dvr-ms") continue;
					bt.AddFiles(cItem.Label,cItem.Path);
					fCount++;
				}								
				if (fCount>0) // Start Thread to converting Files
				{
					ThreadStart ts = new ThreadStart(bt.TranscodeThread);
					Thread t = new Thread(ts);
					t.IsBackground=true;
					t.Priority=ThreadPriority.BelowNormal;
					GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
					dlgOk.SetHeading(2100); 
					dlgOk.SetLine(1,2120);
					dlgOk.SetLine(2,2122);
					dlgOk.DoModal(GetID);
					t.Start();
				}
				else 
				{
					GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
					dlgOk.SetHeading(2100); 
					dlgOk.SetLine(1,2120);
					dlgOk.SetLine(2,2121);
					dlgOk.DoModal(GetID);
				}
				actSize=0;
				currentState=States.STATE_MAIN;
				UpdateButtons();
				GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);
				GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
			}
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
				if(!item.IsFolder) // if item a folder
				{
					GUIListItem pItem = new GUIListItem(item.FileInfo.Name);
					pItem.FileInfo=item.FileInfo;
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

		private void LoadDriveListControl() 
		{	
			currentFolder="";
			//clear the list
			GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
			for (int i=0; i<driveCount; i++) 
			{
				GUIListItem pItem = new GUIListItem(drives[i]);
				pItem.Path=drives[i];
				pItem.IsFolder=true;
				Utils.SetDefaultIcons(pItem);
				GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST_DIR,pItem);
			}
			string strObjects =String.Format("{0} {1}",GUIControl.GetItemCount(GetID,(int)Controls.CONTROL_LIST_DIR).ToString(), GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
		}

		private void LoadBackupListControl() 
		{	
			//clear the list
			GUIPropertyManager.SetProperty("#convert_info",GUILocalizeStrings.Get(2107));
			int len=System.IO.Directory.GetCurrentDirectory().Length;
			GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
			GetBackup();
			foreach (string file in backupFiles) 
			{
				GUIListItem pItem = new GUIListItem(file.Substring(len));
				pItem.IsFolder=false;
				Utils.SetDefaultIcons(pItem);
				GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST_DIR,pItem);
			}
			string strObjects =String.Format("{0} {1}",GUIControl.GetItemCount(GetID,(int)Controls.CONTROL_LIST_DIR).ToString(), GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
			GUIPropertyManager.SetProperty("#convert_info","");
		}

		private void DisableButtons()
		{
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_VIDEO);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_AUDIO);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_DATA);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CD_INFO);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_EJECT_CD);
		}

		private void ShowList()
		{
			GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);					
			switch (currentState)
			{
				case States.STATE_DISK_INFO :
					UpdateButtons();
					GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2123));
					break;
				case States.STATE_CONVERT :
					UpdateButtons();
					GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2137));
					currentExt=dvr_extensions;
					LoadDriveListControl();
					currentFolder="";
					max=dvdSize*5;
					actSize=0;
					break;
				case States.STATE_MAKE_DATA_CD :
					UpdateButtons();
					GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2105));
					currentExt=Util.Utils.AudioExtensions;
					currentExt.AddRange(Util.Utils.PictureExtensions);
					currentExt.AddRange(Util.Utils.VideoExtensions);
					LoadDriveListControl();
					currentFolder="";
					max=cdSize;
					actSize=0;
					break;
				case States.STATE_MAKE_DATA_DVD :
					UpdateButtons();
					GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2106));
					currentExt=Util.Utils.AudioExtensions;
					currentExt.AddRange(Util.Utils.PictureExtensions);
					currentExt.AddRange(Util.Utils.VideoExtensions);
					LoadDriveListControl();
					currentFolder="";
					max=dvdSize;
					actSize=0;
					break;
				case States.STATE_MAKE_BACKUP :
					UpdateButtons();
					GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2144));
					currentFolder="";
					LoadBackupListControl();
					max=cdSize;
					actSize=0;
					break;
				case States.STATE_MAKE_AUDIO :
					UpdateButtons();
					GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2102));
					currentExt=mp3_extensions;
					LoadDriveListControl();
					currentFolder="";
					max=cdSize;
					actSize=0;
					break;
				case States.STATE_MP3_CD :
					UpdateButtons();
					GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2139));
					currentExt=Util.Utils.AudioExtensions;
					LoadDriveListControl();
					currentFolder="";
					max=cdSize;
					actSize=0;
					break;
				case States.STATE_MP3_DVD :
					UpdateButtons();
					GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2140));
					currentExt=Util.Utils.AudioExtensions;
					LoadDriveListControl();
					currentFolder="";
					max=cdSize;
					actSize=0;
					break;
			}
		}

		private void UpdateButtons()
		{
			switch (currentState)
			{
				case States.STATE_MAIN :  // Main Menu
					GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2143));
					GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
					GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);
					GUIControl.HideControl(GetID,(int)Controls.CONTROL_CD_DETAILS);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CD_DETAILS);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_VIDEO,GUILocalizeStrings.Get(2134));
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_VIDEO);
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_VIDEO);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_AUDIO,GUILocalizeStrings.Get(2135));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_DATA,GUILocalizeStrings.Get(2136));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_CD_INFO,GUILocalizeStrings.Get(2123));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_FORMAT_RD,GUILocalizeStrings.Get(2114));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_EJECT_CD,GUILocalizeStrings.Get(2126));
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_DATA);
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_CD_INFO);
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_EJECT_CD);
					if (isBurner==true) 
					{
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_AUDIO);
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_DATA);
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_CD_INFO);
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_EJECT_CD);
					} 
					else 
					{
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_AUDIO);
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_DATA);
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CD_INFO);
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_EJECT_CD);
					}
					break;
				case States.STATE_VIDEO : // Video Menu
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_DATA);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BACK,GUILocalizeStrings.Get(712));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_CONVERT,GUILocalizeStrings.Get(2137));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_CUT,GUILocalizeStrings.Get(2138));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_MAKE_VIDEO_CD,GUILocalizeStrings.Get(2103));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_MAKE_VIDEO_DVD,GUILocalizeStrings.Get(2104));
					GUIControl.HideControl(GetID,(int)Controls.CONTROL_EJECT_CD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_EJECT_CD);
					break;
				case States.STATE_CONVERT : // Video Convert Menu
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BACK,GUILocalizeStrings.Get(712));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_CONVERT,GUILocalizeStrings.Get(2118));
					if(bt.isConverting==true) 
					{
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CONVERT);
					}
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_CUT,GUILocalizeStrings.Get(2141));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_MAKE_VIDEO_CD,GUILocalizeStrings.Get(2142));
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CUT);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_VIDEO_CD);
					GUIControl.HideControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
					GUIControl.HideControl(GetID,(int)Controls.CONTROL_EJECT_CD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_EJECT_CD);
					break;
				case States.STATE_AUDIO : // Audio Menu
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BACK,GUILocalizeStrings.Get(712));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_MAKE_AUDIO,GUILocalizeStrings.Get(2102));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_MAKE_MP3_CD,GUILocalizeStrings.Get(2139));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_MAKE_MP3_DVD,GUILocalizeStrings.Get(2140));
					GUIControl.HideControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
					GUIControl.HideControl(GetID,(int)Controls.CONTROL_EJECT_CD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_EJECT_CD);
					break;
				case States.STATE_DATA : // Data Menu
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BACK,GUILocalizeStrings.Get(712));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_COPY_CD_DVD,GUILocalizeStrings.Get(2101));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_MAKE_DATA_CD,GUILocalizeStrings.Get(2105));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_MAKE_DATA_DVD,GUILocalizeStrings.Get(2106));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_MAKE_MP_BACKUP,GUILocalizeStrings.Get(2144));
					GUIControl.HideControl(GetID,(int)Controls.CONTROL_EJECT_CD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_EJECT_CD);
					break;
				case States.STATE_DISK_INFO : // CD Disk Info
					AllButtonsOff();
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BACK,GUILocalizeStrings.Get(712));
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_CD_DETAILS);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_CD_DETAILS);
					break;
				case States.STATE_MAKE_DATA_CD : // Burn Data CD Menu
					AllButtonsOff();
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BACK,GUILocalizeStrings.Get(712));
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_AUDIO,GUILocalizeStrings.Get(2100));
					break;
				case States.STATE_MAKE_DATA_DVD : // Burn Data DVD Menu
					AllButtonsOff();
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BACK,GUILocalizeStrings.Get(712));
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_AUDIO,GUILocalizeStrings.Get(2100));
					break;
				case States.STATE_MAKE_BACKUP : // Burn Data DVD Menu
					AllButtonsOff();
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BACK,GUILocalizeStrings.Get(712));
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_AUDIO,GUILocalizeStrings.Get(2100));
					break;
				case States.STATE_MP3_CD : // Burn MP3 CD Menu
					AllButtonsOff();
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BACK,GUILocalizeStrings.Get(712));
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_AUDIO,GUILocalizeStrings.Get(2100));
					break;
				case States.STATE_MAKE_AUDIO : // Burn MP3 CD Menu
					AllButtonsOff();
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BACK,GUILocalizeStrings.Get(712));
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_AUDIO,GUILocalizeStrings.Get(2100));
					break;
				case States.STATE_MP3_DVD : // Burn MP3 DVD Menu
					AllButtonsOff();
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_BACK);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BACK,GUILocalizeStrings.Get(712));
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_AUDIO);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_AUDIO,GUILocalizeStrings.Get(2100));
					break;
			}
		}

		private void AllButtonsOff()
		{
			GUIControl.HideControl(GetID,(int)Controls.CONTROL_VIDEO);
			GUIControl.HideControl(GetID,(int)Controls.CONTROL_AUDIO);
			GUIControl.HideControl(GetID,(int)Controls.CONTROL_DATA);
			GUIControl.HideControl(GetID,(int)Controls.CONTROL_CD_INFO);
			GUIControl.HideControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
			GUIControl.HideControl(GetID,(int)Controls.CONTROL_EJECT_CD);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_VIDEO);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_AUDIO);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_DATA);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CD_INFO);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
			GUIControl.DisableControl(GetID,(int)Controls.CONTROL_EJECT_CD);
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
		/// fills the drive array. 3=HD 5=CD
		/// </summary>
		private void GetDrives() 
		{
			foreach(string drive in Environment.GetLogicalDrives())
			{
				switch((DriveType)Util.Utils.getDriveType(drive))
				{
					case DriveType.Removable:
					case DriveType.Fixed:
					case DriveType.RemoteDisk:
					case DriveType.RamDisk:
						drives[driveCount]=drive;
						driveCount++;
						break;
				}
			}
		}

		private void LoadSettings() 
		{
			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml")) 
			{
				Log.Write("GUIBurner.LoadSettings: 1");

				isBurner=xmlreader.GetValueAsBool("burner","burn",true);
				fastFormat=xmlreader.GetValueAsBool("burner","fastformat",true);
				tmpFolder=xmlreader.GetValueAsString("burner","temp_folder","c:\\");
				recorder=xmlreader.GetValueAsInt("burner","recorder",0);
				convertDVR=xmlreader.GetValueAsBool("burner","convertdvr",true);
				deleteDVRSrc=xmlreader.GetValueAsBool("burner","deletedvrsource",false);
				changeTVDatabase=xmlreader.GetValueAsBool("burner","changetvdatabase",false);
				dateTimeFolder=xmlreader.GetValueAsBool("burner","dateTimeFolder",false);

				copyMpegPath=xmlreader.GetValueAsBool("burner","mpegpath",false);
				deleteDatabase=xmlreader.GetValueAsBool("burner","deletetvdatabase",false);
				mpegPath=xmlreader.GetValueAsString("burner","mpeg_folder","");
				
				Log.Write("GUIBurner.LoadSettings: 2");

				if (isBurner==true) 
				{
					burnClass= new XPBurn.XPBurnCD();
					burnClass.BurnerDrive = burnClass.RecorderDrives[recorder].ToString();
				}

				Log.Write("GUIBurner.LoadSettings: 3");

				if(recordCards==1) 
				{
					drives[driveCount++]="["+GUILocalizeStrings.Get(2133)+"]";
				} 
				else if (recordCards==2) 
				{
					drives[driveCount++]="["+GUILocalizeStrings.Get(2144)+"]";
					drives[driveCount++]="["+GUILocalizeStrings.Get(2145)+"]";
				}

				Log.Write("GUIBurner.LoadSettings: 4");

				for (int i=0; i< 50; i++) 
				{
					Log.Write("GUIBurner.LoadSettings: 5.{0}", i + 1);

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
					}

					if (vname[i].StartsWith("CD/")==false && video[i]!="") 
					{
						drives[driveCount]="["+vname[i]+"]";
						driveCount++;
					}
					if (sname[i].StartsWith("CD/")==false && sound[i]!="") 
					{
						drives[driveCount]="["+sname[i]+"]";
						driveCount++;
					}
				}	
			}
		}

		private void okDialog(string header, string text2) 
		{
			GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
			dlgOk.SetHeading(header); 
			dlgOk.SetLine(2,text2);
			dlgOk.DoModal(GetID);
		}
		#endregion

		#region Burner Functions

		private void BurnCD(BurnTypes bTyp)
		{
			AutoPlay.StopListening();
			GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
			if (null!=dlgYesNo) 
			{
				dlgYesNo.SetHeading(GUILocalizeStrings.Get(2100)); 
				dlgYesNo.SetLine(1,GUILocalizeStrings.Get(2108));
				dlgYesNo.SetLine(2,GUILocalizeStrings.Get(2109));
				dlgYesNo.DoModal(GetID);
				if (dlgYesNo.IsConfirmed)  // burn CD
				{
					if (bTyp==BurnTypes.MP_BACKUP) 
					{
						try 
						{
							foreach (string file in backupFiles) 
							{
								burnClass.AddFile(file,file);
							}
						}
						catch(Exception ex)
						{
							Log.Write("MyBurner: ", ex.Message);
						}
					} 
					else 
					{
						int count = GUIControl.GetItemCount(GetID, (int)Controls.CONTROL_LIST_COPY);
						for (int i=0; i<count; i++) 
						{
							GUIListItem cItem = GUIControl.GetListItem(GetID, (int)Controls.CONTROL_LIST_COPY,i);
							try 
							{
								burnClass.AddFile(cItem.Path+"\\"+cItem.Label,cItem.Path+"\\"+cItem.Label);
								Log.Write("Add File: {0}",cItem.Path+"\\"+cItem.Label);
							}
							catch(Exception ex)
							{
								Log.Write("MyBurner: ", ex.Message);
							}
						}
					}
					if (bTyp == BurnTypes.AUDIO_CD) 
					{
						burnClass.ActiveFormat = XPBurn.RecordType.afMusic;
						Log.Write("Burn Audio");
					} 
					else 
					{
						burnClass.ActiveFormat = XPBurn.RecordType.afData;
					}
					GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
					GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);
					if(burnClass.MediaInfo.isWritable == false) 
					{
						okDialog(GUILocalizeStrings.Get(2100), GUILocalizeStrings.Get(2127));
					} 
					else 
					{
						burnClass.PreparingBurn +=new XPBurn.NotifyEstimatedTime(burnClass_PreparingBurn);
						burnClass.AddProgress +=new XPBurn.NotifyCDProgress(burnClass_AddProgress);
						burnClass.BlockProgress+=new XPBurn.NotifyCDProgress(burnClass_BlockProgress);
						burnClass.ClosingDisc+=new XPBurn.NotifyEstimatedTime(burnClass_ClosingDisc);
						burnClass.BurnComplete+=new XPBurn.NotifyCompletionStatus(burnClass_BurnComplete);
						try 
						{
							burnClass.RecordDisc(false,false);
						}
						catch(Exception ex)
						{
							Log.Write("MyBurner: ", ex.Message);
						}
					}
				}
			}
			currentState=States.STATE_MAIN;
			UpdateButtons();
			AutoPlay.StartListening();
		}

		private void CdRwFormat()
		{
			if (isBurner==true) 
			{
				GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
				if (dlgYesNo!=null) 
				{
					dlgYesNo.SetHeading(GUILocalizeStrings.Get(2100)); 
					dlgYesNo.SetLine(1,GUILocalizeStrings.Get(2115));
					dlgYesNo.SetLine(2,GUILocalizeStrings.Get(2109));
					dlgYesNo.DoModal(GetID);
					if (dlgYesNo.IsConfirmed) // format CD
					{
						if(burnClass.MediaInfo.isUsable==false) 
						{
							okDialog(GUILocalizeStrings.Get(2100), GUILocalizeStrings.Get(2124));
						} 
						else 
						{
							DisableButtons();
							XPBurn.EraseKind eraseType = new XPBurn.EraseKind();
							if(fastFormat==true) 
							{
								eraseType=XPBurn.EraseKind.ekQuick;
							} 
							else 
							{
								eraseType=XPBurn.EraseKind.ekFull;
							}
							GUIPropertyManager.SetProperty("#convert_info",GUILocalizeStrings.Get(2125));
							try 
							{
								burnClass.Erase(eraseType);
							}			
							catch(Exception ex)
							{
								Log.Write("MyBurner:Unable format CD/RW", ex.Message);
							}
							burnClass.EraseComplete +=new XPBurn.NotifyCompletionStatus(EraseFinished);	
						}
					}
				}
			}
		}

		private void CdInfo()
		{
			if (isBurner==true) 
			{
				string info=GUILocalizeStrings.Get(2123);
				currentState=States.STATE_DISK_INFO;
				UpdateButtons();
				try 
				{
					info="\nDisc Space : " + burnClass.DiscSpace.ToString()+"\n";
					info=info+"Free Disc Space : " + burnClass.FreeDiscSpace.ToString()+"\n";
					if (burnClass.IsBurning==false && burnClass.IsErasing==false) 
					{
						info=info+"Media Is Usable : " + burnClass.MediaInfo.isUsable.ToString()+"\n";
						info=info+"Media Is Blank : " + burnClass.MediaInfo.isBlank.ToString()+"\n";
						info=info+"Media Is ReadWrite : " + burnClass.MediaInfo.isReadWrite.ToString()+"\n";
						info=info+"Media Is Writable : " + burnClass.MediaInfo.isWritable.ToString()+"\n";
					}
					info=info+"Product ID : " + burnClass.ProductID.ToString()+"\n";
					if (burnClass.RecorderType==XPBurn.RecorderType.rtCDR)  { info=info+"Recorder Type : CDR\n"; }
					if (burnClass.RecorderType==XPBurn.RecorderType.rtCDRW) { info=info+"Recorder Type : CDRW\n"; }
					info=info+"Max Write Speed : " + burnClass.MaxWriteSpeed.ToString()+"\n";
					info=info+"Revision : " + burnClass.Revision+"\n";
					info=info+"Vendor : " + burnClass.Vendor+"\n";
					info=info+"Volume Name : " + burnClass.VolumeName+"\n";
					info=info+"Write Speed : " + burnClass.WriteSpeed.ToString()+"\n";
				}
				catch(Exception ex)
				{
					Log.Write("MyBurner:Error CD Info", ex.Message);
				}
				GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_CD_DETAILS, info );
			}
		}
		private void EraseFinished(System.UInt32 status)
		{
			GUIPropertyManager.SetProperty("#convert_info",GUILocalizeStrings.Get(2111));
			UpdateButtons();
		}

		private void burnClass_PreparingBurn(int nEstimatedSeconds)
		{
				GUIPropertyManager.SetProperty("#convert_info",GUILocalizeStrings.Get(2128)+" "+nEstimatedSeconds.ToString());
		}

		private void burnClass_AddProgress(int nCompletedSteps, int nTotalSteps)
		{
			GUIPropertyManager.SetProperty("#convert_info",GUILocalizeStrings.Get(2129));
			if (nCompletedSteps>0) 
				perc=Convert.ToInt16(nCompletedSteps/(nTotalSteps/100d)); 
			else 
				perc=0;
			GUIPropertyManager.SetProperty("#burner_perc",perc.ToString());
		}

		private void burnClass_BlockProgress(int nCompletedSteps, int nTotalSteps)
		{
			GUIPropertyManager.SetProperty("#convert_info",GUILocalizeStrings.Get(2130)+" "+nCompletedSteps.ToString()+" "+GUILocalizeStrings.Get(2131)+" "+nTotalSteps.ToString());
			if (nCompletedSteps>0) 
				perc=Convert.ToInt16(nCompletedSteps/(nTotalSteps/100d)); 
			else 
				perc=0;
			GUIPropertyManager.SetProperty("#burner_perc",perc.ToString());
		}

		private void burnClass_ClosingDisc(int nEstimatedSeconds)
		{
			GUIPropertyManager.SetProperty("#convert_info",GUILocalizeStrings.Get(2132)+" "+nEstimatedSeconds.ToString());
		}

		private void burnClass_BurnComplete(uint status)
		{
			GUIPropertyManager.SetProperty("#convert_info",GUILocalizeStrings.Get(2111));
			XPBurn.XPBurnCD burnClass = new XPBurn.XPBurnCD(); 
		}
		#endregion

		#region Backup Functions
		private void GetBackup() 
		{
			string dest=System.IO.Directory.GetCurrentDirectory()+"\\backup";
			string file="";
			string folder="";
			backupFiles.Clear();		
			if (dateTimeFolder==true) 
			{
				DateTime cur=DateTime.Now;
				dest=dest+"\\backup"+cur.Day+"-"+cur.Month+"-"+cur.Year+" "+cur.Hour+cur.Minute;
			}
			using (MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				int count=xmlreader.GetValueAsInt("burner","backuplines",0);
				for(int i=0; i<=count; i++)
				{
					file=xmlreader.GetValueAsString("burner","backupline#"+i.ToString(),"");
					int indx=file.IndexOf("*.*",0);
					if (indx>=0) 
					{
						if (indx>1) 
						{
							folder="\\"+file.Substring(0,indx-1);
						} 
						else 
						{
							folder="";
						}
						file=System.IO.Directory.GetCurrentDirectory()+"\\"+file.Substring(0,indx);
						FileInfo scFile = new FileInfo(file);
						DirectoryInfo scDir = scFile.Directory;
						ParseFolder(scDir,dest+folder,"");
					} 
					else 
					{
						indx=file.IndexOf("*.",0);
						if (indx>=0) 
						{
							if (indx>1) 
							{
								folder="\\"+file.Substring(0,indx-1);
							} 
							else 
							{
								folder="";
							}
							string ex=file.Substring(indx+1);
							file=System.IO.Directory.GetCurrentDirectory()+"\\"+file.Substring(0,indx);
							FileInfo scFile = new FileInfo(file);
							DirectoryInfo scDir = scFile.Directory;
							ParseFolder(scDir,dest+folder,ex);
						} 
						else 
						{
							if (file!="") 
							{
								FileInfo fi = new FileInfo(System.IO.Directory.GetCurrentDirectory()+"\\"+file);
								fi.CopyTo(dest+"\\"+file, true);
								backupFiles.Add(dest+"\\"+file);
								GUIPropertyManager.SetProperty("#convert_info",dest+"\\"+file);
								GUIWindowManager.Process();
							}
						}
					}
				}
			}
		}
		
		private void ParseFolder(DirectoryInfo source, string dest, string ext) 
		{
			bool result = true;
			if(!Directory.Exists(dest))
				Directory.CreateDirectory(dest);

			foreach(FileInfo fi in source.GetFiles()) 
			{
				if (ext!="") 
				{
					if (fi.Extension==ext) 
					{
						string destFile = dest + "\\" + fi.Name;
						result = result && (fi.CopyTo(destFile, true) != null);
						backupFiles.Add( destFile);
						GUIPropertyManager.SetProperty("#convert_info",destFile);
						GUIWindowManager.Process();
					}
				}
				else 
				{
					string destFile = dest + "\\" + fi.Name;
					result = result && (fi.CopyTo(destFile, true) != null);
					backupFiles.Add( destFile);					
					GUIPropertyManager.SetProperty("#convert_info",destFile);
					GUIWindowManager.Process();
				}
			}
			if (ext=="") 
			{
				foreach(DirectoryInfo subDir in source.GetDirectories())
				{
					string destTmp = subDir.FullName.Replace(source.FullName, dest);
					ParseFolder(subDir, destTmp,ext);
				}
			}
		}

		#endregion

		#region Timer Functions		
		/// <summary>
		/// init 60 sec timer for automatic convert. 
		/// </summary>
		private void InitializeConvertTimer() 
		{
			dvr_extensions.Clear();
			dvr_extensions.Add(".dvr-ms");
			mp3_extensions.Clear();
			mp3_extensions.Add(".mp3");
			
			ArrayList     m_tvcards    = new ArrayList();
			recordCards=0;
		
			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml")) 
			{
				convertAuto=xmlreader.GetValueAsBool("burner","convertautomatic",false);
			}
			if (convertAuto==true) 
			{
				try
				{
					using (Stream r = File.Open(@"capturecards.xml", FileMode.Open, FileAccess.Read))
					{
						SoapFormatter c = new SoapFormatter();
						m_tvcards = (ArrayList)c.Deserialize(r);
						r.Close();
					} 
				}
				catch(Exception)
				{
					Log.WriteFile(Log.LogType.Recorder,"Recorder: invalid capturecards.xml found! please delete it");
				}

				if (m_tvcards.Count==0) 
				{
					Log.WriteFile(Log.LogType.Recorder,"Recorder: no capture cards found. automatic convert canceled");
				} 
				else 
				{
					for (int i=0; i < m_tvcards.Count;i++)
					{
						TVCaptureDevice card=(TVCaptureDevice)m_tvcards[i];
						if(card.UseForRecording==true) 
						{
							if (i==0) 
							{
								recordpath1=card.RecordingPath;
								recordCards=1;
							}
							if (i==1) 
							{
								recordpath2=card.RecordingPath;
								if (recordpath1!=recordpath2)
								{
									recordCards=2;
								}
							}
						}
						card.ID=(i+1);
					}

					bt.ClearFiles();
					VirtualDirectory Directory;
					ArrayList itemlist;
					Directory = new VirtualDirectory();
					Directory.SetExtensions(dvr_extensions);
					itemlist = Directory.GetDirectory(recordpath1);
					maxAutoFiles=0;
					foreach (GUIListItem item in itemlist) 
					{
						if (item.IsFolder==false)
						{ 
							cFiles[maxAutoFiles].name=item.Label;
							cFiles[maxAutoFiles].path=recordpath1;
							if (item.FileInfo!=null) 
							{
								cFiles[maxAutoFiles].size=item.FileInfo.Length;
							}
							cFiles[maxAutoFiles++].oldSize=0;
						}
					}
					convertTimer.Tick += new EventHandler(OnTimer);
					convertTimer.Interval = 60000;	  //60 sec Intervall
					convertTimer.Enabled=true;
					convertTimer.Start();
				}
			}
		}

		// if a new file in TV-Record folder start converting
		private void OnTimer(Object sender, EventArgs e) 
		{
			if(bt.isConverting==false) 
			{
				VirtualDirectory Directory;
				ArrayList itemlist;
				Directory = new VirtualDirectory();
				Directory.SetExtensions(dvr_extensions);
				itemlist = Directory.GetDirectory(recordpath1);

				foreach (GUIListItem item in itemlist) 
				{
					if (item.IsFolder==false)
					{ 
						if (item.FileInfo!=null) 
						{
							bool hit=false;
							for(int i=0;i<maxAutoFiles;i++) 
							{	
								if (item.Label==cFiles[i].name) 
								{
									hit=true;
									if (item.FileInfo!=null) 
									{
										if (item.FileInfo.Length>cFiles[i].oldSize)
										{
											cFiles[i].oldSize=item.FileInfo.Length;
										} 
										else 
										{
											if (cFiles[i].oldSize==item.FileInfo.Length) // ready to convert
											{
												bt.ClearFiles();
												bt.deleteDvrMsSrc=true;
												bt.changeDatabase=true;
												bt.AddFiles(item.Label+".dvr-ms",recordpath1);
												ThreadStart ts = new ThreadStart(bt.TranscodeThread);
												Thread t = new Thread(ts);
												t.IsBackground=true;
												t.Priority=ThreadPriority.Lowest;
												t.Start();
												if (maxAutoFiles==1) 
												{
													maxAutoFiles--;
												} 
												else 
												{
													for(int x=i;x<maxAutoFiles;x++) 
													{
														cFiles[x]=cFiles[x+1];	
													}
													maxAutoFiles--;
												}
											}
										}
									}
								}
							}
							if (hit==false) 
							{
								if (item.IsFolder==false)
								{ 
									cFiles[maxAutoFiles].name=item.Label;
									cFiles[maxAutoFiles].path=item.Path;
									if (item.FileInfo!=null) 
									{
										cFiles[maxAutoFiles].size=item.FileInfo.Length;
									}
									cFiles[maxAutoFiles++].oldSize=0;
								}
							}
						}
					}
				}
			}
		}
		#endregion
	}
}
