#region Usings

using System;
using System.Collections;
using System.Threading;
using System.Management;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Ripper;
using Core.Util;
#endregion

namespace MediaPortal.GUI.GUIBurner
{
  /// <summary>
  /// Summary description for GUIBurner.
  /// </summary>
	public class GUIBurner : GUIWindow 
	{

		#region Private Enumerations
		enum Controls 
		{
			CONTROL_COPY_CD_DVD			= 2,	 
			CONTROL_MAKE_AUDIO			= 3,	 
			CONTROL_MAKE_VIDEO_CD		= 4,	 
			CONTROL_MAKE_VIDEO_DVD	= 5,
			CONTROL_CONVERT_DVR			= 6,
			CONTROL_MAKE_DATA_CD		= 7,
			CONTROL_MAKE_DATA_DVD		= 8,
			CONTROL_CD_INFO					= 9,
			CONTROL_FORMAT_RD				= 10,
			CONTROL_EJECT_CD				= 11,
			CONTROL_MARK_ALL				= 12,
			CONTROL_LIST_DIR				= 20,
			CONTROL_LIST_COPY				= 30,
			CONTROL_CD_DETAILS			= 50
		};

		enum States
		{
			STATE_MAIN = 0,
			STATE_COPY_CDDVD = 1,
			STATE_MAKE_AUDIO = 2,
			STATE_MAKE_DATA_CD = 3,
			STATE_MAKE_DATA_DVD = 4,
			STATE_MAKE_VIDEO_CD = 5,
			STATE_MAKE_VIDEO_DVD = 6,
			STATE_CONVERT_DVR = 7,
			STATE_DISK_INFO = 8
		};

		private States currentState = States.STATE_MAIN;

		#endregion

		#region Private Variables

		private ArrayList cFiles  = new ArrayList();
		private struct file 
		{
			public string name;
			public long size;
			public string path;
		}
		
		private	XPBurn.XPBurnCD burnClass = new XPBurn.XPBurnCD(); 
		
		string[] video = new string[20];
		string[] vname = new string[20];
		string[] sound = new string[20];
		string[] sname = new string[20];
		string[] pictures = new string[20];
		string[] pname = new string[20];

		private int recorder;
		private ArrayList files = new ArrayList();
		private string tmpFolder;
		private string tmpStr;
		private ArrayList currentExt=null;
		private string currentFolder=null;
		private string[] drives=new string[35];
		private int driveCount=0;
		private long actSize=0;
		private long cdSize=681574400;
		private long dvdSize=5046586572;
		private int perc=0;
		private long max=681574400;
		private bool fastFormat;
		private	bool convertDVR;
		private	bool deleteDVRSrc;
		private string soundFolder="";
		private string videoFolder="";
		private	BurnerThread bt = new BurnerThread();
		static ArrayList dvr_extensions	= new ArrayList();

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
			get { return (int)GUIWindow.Window.WINDOW_MY_BURNER; }
			set { base.GetID = value; }
		}

		public override bool Init() 
		{
			return Load (GUIGraphicsContext.Skin+@"\myburner.xml");
		}

		public override void OnAction(Action action) 
		{
			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU) 
			{
				GUIWindowManager.PreviousWindow();
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
					GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(2100));//Burn
					GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2100));//Burn
					GUIPropertyManager.SetProperty("#burner_perc","0");
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
					if (iControl==(int)Controls.CONTROL_COPY_CD_DVD) // select Main Page or copy cd/dvd
					{
						if (currentState!=States.STATE_MAIN) // select return Button
						{
							currentState=States.STATE_MAIN;
							UpdateButtons();
						} 
						else 
						{
							GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2101));//Copy CD/DVD
							currentState=States.STATE_COPY_CDDVD;
							UpdateButtons();
							GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
							if (null==dlgYesNo) break;
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(2100)); //Burn
							dlgYesNo.SetLine(1,GUILocalizeStrings.Get(2110));//Insert original CD/DVD
							dlgYesNo.SetLine(2,GUILocalizeStrings.Get(2109));//then press OK
							dlgYesNo.DoModal(GetID);
						}
						return true;
					}
					if (iControl==(int)Controls.CONTROL_MAKE_AUDIO) // select Make Audio page
					{
						if (currentState==States.STATE_MAIN) 
						{
							currentState=States.STATE_MAKE_AUDIO;
							UpdateButtons();
							GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);
							GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2102));//Create Audio-CD
							currentExt=Util.Utils.AudioExtensions;
							LoadListControl(soundFolder,currentExt);
							currentFolder=soundFolder;
							max=cdSize;
							actSize=0;
						} 
						else // Start Convert/Burning Action 
						{
							if (currentState==States.STATE_CONVERT_DVR) //Convert Video Files
							{
								if (!bt.CheckEnvironment())
								{
									GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
									dlgOk.SetHeading(2100); //Burn
									dlgOk.SetLine(0,2133);//Cannot convert DVR-MS to MPG
									dlgOk.SetLine(1,2134);//Required DirectShow filters are
									dlgOk.SetLine(2,2135);//not installed
									dlgOk.DoModal(GetID);
									return true;
								}
								int fCount=0;
								bt.ClearFiles();
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
									t.Start();
									GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
									dlgOk.SetHeading(2100); //Burn
									dlgOk.SetLine(2,2120);//Converting file(s)
									dlgOk.DoModal(GetID);
								}
								else 
								{
									GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
									dlgOk.SetHeading(2100); //Burn
									dlgOk.SetLine(2,2121);// Couldn't convert file(s)!
									dlgOk.DoModal(GetID);
								}
								actSize=0;
								currentState=States.STATE_MAIN;
								UpdateButtons();
								GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);
								GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
							}

							if (currentState==States.STATE_MAKE_DATA_CD || currentState==States.STATE_MAKE_DATA_DVD) //Burn Data
							{
								AutoPlay.StopListening();
								GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
								if (null!=dlgYesNo) {
									dlgYesNo.SetHeading(GUILocalizeStrings.Get(2100)); //Burn
									dlgYesNo.SetLine(1,GUILocalizeStrings.Get(2108));//Insert empty CD/DVD
									dlgYesNo.SetLine(2,GUILocalizeStrings.Get(2109));//then press OK
									dlgYesNo.DoModal(GetID);
									if (dlgYesNo.IsConfirmed)  // burn CD
									{
										int count = GUIControl.GetItemCount(GetID, (int)Controls.CONTROL_LIST_COPY);
										for (int i=0; i<count; i++) 
										{
											GUIListItem cItem = GUIControl.GetListItem(GetID, (int)Controls.CONTROL_LIST_COPY,i);
											try 
											{
												GUIPropertyManager.SetProperty("#convert_info",cItem.Path+"\\"+cItem.Label);
												burnClass.AddFile(cItem.Path+"\\"+cItem.Label,cItem.Path+"\\"+cItem.Label);
											}
											catch(Exception ex)
											{
												Log.Write("MyBurner: ", ex.Message);
											}
										}
										burnClass.ActiveFormat = XPBurn.RecordType.afData;
										GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
										GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);
										if(burnClass.MediaInfo.isWritable == false) 
										{
											//Burn, The CD is not writable
											okDialog(GUILocalizeStrings.Get(2100), GUILocalizeStrings.Get(2127));
										} 
										else 
										{
											GUIControl.HideControl(GetID,(int)Controls.CONTROL_COPY_CD_DVD);
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
						}
						return true;
					}
					if (iControl==(int)Controls.CONTROL_MAKE_VIDEO_CD) // select Make Audio page
					{
						if (currentState!=States.STATE_MAIN) 
						{
							currentState=States.STATE_MAKE_VIDEO_CD;
							UpdateButtons();
							GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);
							GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2103));//Create Video-CD
							currentExt=Util.Utils.AudioExtensions;
							LoadListControl(soundFolder,currentExt);
							currentFolder=videoFolder;
							max=cdSize;
							actSize=0;
						}
						return true;
					}
					if (iControl==(int)Controls.CONTROL_MAKE_VIDEO_DVD) // select Make Audio page
					{
						if (currentState!=States.STATE_MAIN) 
						{
							currentState=States.STATE_MAKE_VIDEO_DVD;
							UpdateButtons();
							GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);
							GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2104));//Create Video-DVD
							currentExt=Util.Utils.AudioExtensions;
							LoadListControl(soundFolder,currentExt);
							currentFolder=videoFolder;
							max=dvdSize;
							actSize=0;
						}
						return true;
					}
					if (iControl==(int)Controls.CONTROL_CONVERT_DVR) // select Convert DVR->MS page
					{
						if (currentState==States.STATE_MAIN) 
						{
							currentState=States.STATE_CONVERT_DVR;
							UpdateButtons();
							GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);
							GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2118));//Convert DVR-MS
							currentExt=dvr_extensions;
							LoadDriveListControl();
							currentFolder="";
							max=dvdSize*5;
							actSize=0;
						}
						return true;
					}
					if (iControl==(int)Controls.CONTROL_MAKE_DATA_CD) // select Make Data page
					{
						if (currentState==States.STATE_MAIN) 
						{
							currentState=States.STATE_MAKE_DATA_CD;
							UpdateButtons();
							GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);
							GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2105));//Create Data-CD
							currentExt=Util.Utils.AudioExtensions;
							currentExt.AddRange(Util.Utils.PictureExtensions);
							currentExt.AddRange(Util.Utils.VideoExtensions);
							LoadDriveListControl();
							currentFolder="";
							max=cdSize;
							actSize=0;
						}
						return true;
					}
					if (iControl==(int)Controls.CONTROL_MAKE_DATA_DVD) // select Make Data page
					{
						if (currentState!=States.STATE_MAIN) 
						{
							currentState=States.STATE_MAKE_DATA_DVD;
							UpdateButtons();
							GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);
							GUIPropertyManager.SetProperty("#burner_title",GUILocalizeStrings.Get(2106));//Create Data-DVD
							currentExt=Util.Utils.AudioExtensions;
							currentExt.AddRange(Util.Utils.PictureExtensions);
							currentExt.AddRange(Util.Utils.VideoExtensions);
							LoadDriveListControl();
							currentFolder="";
							max=dvdSize;
							actSize=0;
						}
						return true;
					}
					if (iControl==(int)Controls.CONTROL_CD_INFO) 
					{
						string info=GUILocalizeStrings.Get(2123);//Disk info
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
							if (burnClass.RecorderType==XPBurn.RecorderType.RECORDER_CDR)  { info=info+"Recorder Type : CDR\n"; }
							if (burnClass.RecorderType==XPBurn.RecorderType.RECORDER_CDRW) { info=info+"Recorder Type : CDRW\n"; }
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
					if (iControl==(int)Controls.CONTROL_EJECT_CD) 
					{
						burnClass.Eject();
					}
					if (iControl==(int)Controls.CONTROL_FORMAT_RD) 
					{
						GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
						if (null==dlgYesNo) break;
						dlgYesNo.SetHeading(GUILocalizeStrings.Get(2100)); //Burn
						dlgYesNo.SetLine(1,GUILocalizeStrings.Get(2115));//Insert CD/DVD RW
						dlgYesNo.SetLine(2,GUILocalizeStrings.Get(2109));//then press OK
						dlgYesNo.DoModal(GetID);
						if (dlgYesNo.IsConfirmed) // format CD
						{
							if(burnClass.MediaInfo.isUsable==false) 
							{
								//Burn, Cannot Erase: Media is not writable
								okDialog(GUILocalizeStrings.Get(2100), GUILocalizeStrings.Get(2124));
							} 
							else 
							{
								GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_AUDIO);
								GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
								GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);
								GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MARK_ALL);
								XPBurn.EraseKind eraseType = new XPBurn.EraseKind();
								if(fastFormat==true) 
								{
									eraseType=XPBurn.EraseKind.ekQuick;
								} 
								else 
								{
									eraseType=XPBurn.EraseKind.ekFull;
								}
								GUIPropertyManager.SetProperty("#convert_info",GUILocalizeStrings.Get(2125));//Erase Disk....
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
					if (iControl==(int)Controls.CONTROL_MARK_ALL) // select Mark All
					{
						int indx=currentFolder.IndexOf("\\\\");
						if (indx>0) 
						{
							currentFolder=currentFolder.Remove(indx,1);
						}
						int count = GUIControl.GetItemCount(GetID, (int)Controls.CONTROL_LIST_DIR);
						for (int i=0; i<count; i++) 
						{
							GUIListItem item = GUIControl.GetListItem(GetID, (int)Controls.CONTROL_LIST_DIR,i);
							if (!item.Label.StartsWith("\\..") && !item.Label.StartsWith("\\") && item.Label.Substring(1,1)!=":") 
							{
								bool isdoub=false;
								int dcount = GUIControl.GetItemCount(GetID, (int)Controls.CONTROL_LIST_COPY);
								for (int ii=0; i<dcount; ii++) 
								{
									GUIListItem cItem = GUIControl.GetListItem(GetID, (int)Controls.CONTROL_LIST_COPY,ii);
									if (cItem.Label==item.Label) 
									{
										if (cItem.Path==item.Path) 
										{
											isdoub=true;
											//break;	
										}
									}
								}
								if (isdoub==false) 
								{
									GUIListItem pItem = new GUIListItem(item);
									pItem.Path=currentFolder;	
									GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST_COPY,pItem);
									actSize=actSize+pItem.FileInfo.Length;
									if (actSize>0) 
										perc=Convert.ToInt16(actSize/(max/100d)); 
									else 
										perc=0;
									tmpStr=CalcExt(actSize)+" ";
									GUIPropertyManager.SetProperty("#burner_size",tmpStr);
									GUIPropertyManager.SetProperty("#burner_perc",perc.ToString());
								}
							}
						}
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
							tmpStr=CalcExt(actSize)+" ";
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
									GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST_COPY,pItem);
									actSize=actSize+pItem.FileInfo.Length;
									if (actSize>0) 
										perc=Convert.ToInt16(actSize/(max/100d)); 
									else 
										perc=0;
									tmpStr=CalcExt(actSize)+" ";
									GUIPropertyManager.SetProperty("#burner_size",tmpStr);
									GUIPropertyManager.SetProperty("#burner_perc",perc.ToString());
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

		public override void Process()
		{
			
			if(bt.isConverting==true) 
			{
				GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CONVERT_DVR);
			}
		}

		#region Private Methods
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
			string strObjects =String.Format("{0} {1}",GUIControl.GetItemCount(GetID,(int)Controls.CONTROL_LIST_DIR).ToString(), GUILocalizeStrings.Get(632));//Objects
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
		
		private void UpdateButtons()
		{
			switch (currentState)
			{
				case States.STATE_MAIN :
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_COPY_CD_DVD,GUILocalizeStrings.Get(2101));//Copy CD/DVD
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_MAKE_AUDIO,GUILocalizeStrings.Get(2102));//Create Audio-CD
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_VIDEO_CD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_VIDEO_DVD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_COPY_CD_DVD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_AUDIO);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CD_DETAILS);
					GUIControl.HideControl(GetID,(int)Controls.CONTROL_CD_DETAILS);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_MARK_ALL);
					if (convertDVR) 
					{
						GUIControl.EnableControl(GetID,(int)Controls.CONTROL_CONVERT_DVR);
					} 
					else 
					{
						GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CONVERT_DVR);
					}
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_CD_INFO);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_MAKE_DATA_CD);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_MAKE_DATA_DVD);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
					break;
				case States.STATE_DISK_INFO:
					GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_DIR);
					GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST_COPY);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_CD_DETAILS);
					GUIControl.ShowControl(GetID,(int)Controls.CONTROL_CD_DETAILS);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_COPY_CD_DVD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_AUDIO);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_COPY_CD_DVD,GUILocalizeStrings.Get(712));
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_DATA_CD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_DATA_DVD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_VIDEO_CD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CONVERT_DVR);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_VIDEO_DVD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
					break;
				case States.STATE_MAKE_DATA_CD :
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_COPY_CD_DVD);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_MAKE_AUDIO);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_COPY_CD_DVD,GUILocalizeStrings.Get(712));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_MAKE_AUDIO,GUILocalizeStrings.Get(2107));
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_DATA_CD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_DATA_DVD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_VIDEO_CD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CONVERT_DVR);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_VIDEO_DVD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
					break;
				case States.STATE_MAKE_DATA_DVD :
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_COPY_CD_DVD);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_MAKE_AUDIO);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_COPY_CD_DVD,GUILocalizeStrings.Get(712));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_MAKE_AUDIO,GUILocalizeStrings.Get(2107));
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_DATA_CD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_DATA_DVD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CONVERT_DVR);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_VIDEO_CD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_VIDEO_DVD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
					break;
				case States.STATE_CONVERT_DVR:
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_COPY_CD_DVD);
					GUIControl.EnableControl(GetID,(int)Controls.CONTROL_MAKE_AUDIO);
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_COPY_CD_DVD,GUILocalizeStrings.Get(712));
					GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_MAKE_AUDIO,GUILocalizeStrings.Get(2118));
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_DATA_CD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CONVERT_DVR);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_DATA_DVD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_VIDEO_CD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_MAKE_VIDEO_DVD);
					GUIControl.DisableControl(GetID,(int)Controls.CONTROL_FORMAT_RD);
					break;
			}
			if(bt.isConverting==true) 
			{
				GUIControl.DisableControl(GetID,(int)Controls.CONTROL_CONVERT_DVR);
			}
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
 
		/// <summary>
		/// fills the drive array. 3=HD 5=CD
		/// </summary>
		private void GetDrives() 
		{
			ManagementObjectSearcher query;
			ManagementObjectCollection queryCollection;
			System.Management.ObjectQuery oq;
			string stringMachineName = "localhost";
			string lw;
			int m;
			char d='C';
			for (int i=0; i<24; i++) 
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
				if (m==4) m=3; // shows Netdrives
				if (m==2) m=3; // shows Cardreader
				if (m==3) 
				{
					drives[driveCount]=d+":\\";
					driveCount++;
				}
				d++;
			}
		}

		private void LoadSettings() 
		{
			using(AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml")) 
			{
				dvr_extensions.Clear();
				dvr_extensions.Add(".dvr-ms");
				fastFormat=xmlreader.GetValueAsBool("burner","fastformat",true);
				tmpFolder=xmlreader.GetValueAsString("burner","temp_folder","c:\\image.iso");
				recorder=xmlreader.GetValueAsInt("burner","recorder",0);
				convertDVR=xmlreader.GetValueAsBool("burner","convertdvr",true);
				deleteDVRSrc=xmlreader.GetValueAsBool("burner","deletedvrsource",false);
				burnClass.BurnerDrive = burnClass.RecorderDrives[recorder].ToString();

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
		private void EraseFinished(System.UInt32 status)
		{
			GUIPropertyManager.SetProperty("#convert_info",GUILocalizeStrings.Get(2111));
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
	}
}
