/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using DirectShowLib.SBE;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;

namespace WindowPlugins.VideoEditor
{
	public class GUIVideoEditor : GUIWindow
	{
		readonly int windowID = 170601;

		#region GUIControls
		[SkinControlAttribute(23)]
		protected GUILabelControl titelLbl = null;
		[SkinControlAttribute(24)]
		protected GUIButtonControl backBtn = null;
		//[SkinControlAttribute(25)]
		//protected GUIButtonControl cancelBtn = null;
		[SkinControlAttribute(32)]
		protected GUILabelControl startTime = null;
		[SkinControlAttribute(34)]
		protected GUILabelControl oldDurationLbl = null;
		[SkinControlAttribute(101)]
		protected GUIListControl videoListLct = null;
    [SkinControlAttribute(102)]
    protected GUISpinControl joinCutSpinCtrl = null;
		[SkinControlAttribute(99)]
		protected GUIVideoControl videoWindow = null;
    [SkinControlAttribute(103)]
    protected GUIListControl joinListCtrl = null;
    [SkinControlAttribute(104)]
    protected GUIButtonControl startJoinBtn = null;
		[SkinControlAttribute(105)]
		protected GUIProgressControl progressBar = null;
		#endregion

		#region Own Variables
		const int maxDrives = 50;
		int cntDrives = 0;
		string[] drives = new string[maxDrives];
		string currentFolder = "";
    //string lastUsedFolder = "";
    VirtualDirectory directory = new VirtualDirectory();
		ArrayList extensions;
		VideoEditorPreview cutScr;
		List<System.IO.FileInfo> joiningList;
		//MediaPortal.Core.Transcoding.Dvrms2Mpeg tompeg;
		string filetoConvert;
		//bool inDatabase;
		TVRecorded recInfo;
		DvrMsModifier dvrmsMod;
		#endregion

		public GUIVideoEditor()
		{ }

		#region Overrides
		public override int GetID
		{
			get
			{
				return windowID;
			}
		}
		public override bool Init()
		{
			try
			{

				bool init = Load(GUIGraphicsContext.Skin + @"\VideoEditorStartScreen.xml");
				if (init)
				{
					GetDrives();
				}
				return init;
			}
			catch
			{
				//MessageBox.Show("Fehler","Fehler",MessageBoxButtons.OKCancel);
				return false;
			}
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad();
			try 
			{
				using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
				{
					currentFolder = xmlreader.GetValueAsString("VideoEditor", "lastUsedFolder", "");
				}
				extensions = new ArrayList();
				extensions.Add(".dvr-ms");
				extensions.Add(".mpeg");
				extensions.Add(".mpg");
				extensions.Add(".ts");
				videoListLct.Clear();
				videoListLct.UpdateLayout();
        startJoinBtn.IsEnabled = false;
        joinListCtrl.IsVisible = false;
				if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2078))  //Join
				{
					joinListCtrl.IsVisible = true;
					startJoinBtn.IsEnabled = true;
					titelLbl.Label = GUILocalizeStrings.Get(2074); //Please, choose the files you would like to join:
				}
				joiningList = new List<System.IO.FileInfo>();
				progressBar.Visible = false;

				if (currentFolder == "")
				{
					LoadShares();
					LoadDrives();
				}
				else
					LoadListControl(currentFolder, extensions);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}

		}
		protected override void OnPageDestroy(int new_windowId)
		{
			g_Player.Release();
			using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
			{
				xmlwriter.SetValue("VideoEditor", "lastUsedFolder", currentFolder);
			}
			base.OnPageDestroy(new_windowId);
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control == backBtn)
			{
				GUIWindowManager.ShowPreviousWindow();
			}
			if (control == videoListLct)
			{
				GUIListItem item = videoListLct.SelectedListItem;
				//System.Windows.Forms.MessageBox.Show(item.Path);
				if (!item.IsFolder)
				{
					if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2077))	//Cut
						ToCutScreen(item.Path);
					if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2078))	//join
					{
						joiningList.Add(new System.IO.FileInfo(item.Path));
						extensions.Clear();
						extensions.Add(System.IO.Path.GetExtension(item.Path));
						LoadListControl(currentFolder,extensions);
						// joinListCtrl.Add(new GUIListItem(item.Path));
						LoadJoinList();
					}
					if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2071)) //Dvr-ms to mpeg
					{
						recInfo = new TVRecorded();
						/*MediaPortal.Core.Transcoding.TranscodeInfo mpegInfo = new MediaPortal.Core.Transcoding.TranscodeInfo();
						mpegInfo.Author = "MediaPortal";
						
						if (inDatabase)
						{
							mpegInfo.Channel = recInfo.Channel;
							mpegInfo.Description = recInfo.Description;
							mpegInfo.Duration = (int)(recInfo.EndTime.Subtract(recInfo.StartTime)).Seconds;
							mpegInfo.End = recInfo.EndTime;
							mpegInfo.file = recInfo.FileName;
							mpegInfo.Start = recInfo.StartTime;
							mpegInfo.Title = recInfo.Title;
						}
						else
						{
							mpegInfo.Channel = "none";
							mpegInfo.Description = "none";
							g_Player.Play(item.Path);
							mpegInfo.Duration = (int)g_Player.Duration;
							g_Player.Stop();
							mpegInfo.End = new DateTime();
							mpegInfo.file = item.Path;
							mpegInfo.Start = new DateTime();
							mpegInfo.Title = System.IO.Path.GetFileNameWithoutExtension(item.Path);
						}*/
						filetoConvert = item.Path;
						/*
						tompeg = new MediaPortal.Core.Transcoding.Dvrms2Mpeg();
						if (!tompeg.Transcode(mpegInfo, MediaPortal.Core.Transcoding.VideoFormat.Mpeg2, MediaPortal.Core.Transcoding.Quality.High))
						{
						//	titelLbl.Label = "finished";
							
						}
						//else
							//titelLbl.Label = "nicht gut gelaufen";
						System.Threading.Thread isTranscoding = new System.Threading.Thread(new System.Threading.ThreadStart(IsConverting));
						isTranscoding.Start();
						//if (tompeg.IsFinished())*/
						progressBar.Percentage = 0;
						progressBar.Visible = true;
						int duration;
						g_Player.Play(item.Path);
						duration = (int)g_Player.Duration;
						g_Player.Stop();
						dvrmsMod = new DvrMsModifier();
						dvrmsMod.OnProgress += new DvrMsModifier.Progress(OnProgress);
						dvrmsMod.OnFinished += new DvrMsModifier.Finished(dvrmsMod_OnFinished);
						dvrmsMod.TranscodeToMpeg(new System.IO.FileInfo(item.Path), duration);

					}
					if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2066)) // Mpeg to divx
					{
						Mpeg2Divx convert = new Mpeg2Divx();
						progressBar.Percentage = 50;
						progressBar.Visible = true;
						MediaPortal.Core.Transcoding.TranscodeInfo info = new MediaPortal.Core.Transcoding.TranscodeInfo();
						info.Author = "MediaPortal";
						//info.Channel = tinfo.recorded.Channel;
						//info.Description = tinfo.recorded.Description;
						info.Title = "test";//tinfo.recorded.Title;
						//info.Start = tinfo.recorded.StartTime;
						//info.End = tinfo.recorded.EndTime;
						//TimeSpan ts = (tinfo.recorded.EndTime - tinfo.recorded.StartTime);
						//info.Duration = (int)ts.TotalSeconds;
                        info.file = item.Path; //tinfo.recorded.FileName;
						convert.CreateProfile(new System.Drawing.Size(360, 288), 2000, 25);
						convert.Transcode(info, MediaPortal.Core.Transcoding.VideoFormat.Divx, MediaPortal.Core.Transcoding.Quality.High);
						while (!convert.IsFinished())
						{
							System.Threading.Thread.Sleep(1000);
						}
					}

                    if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2068)) // Dvrms to divx
                    {
                        Dvrms2Divx convert = new Dvrms2Divx();
                        progressBar.Percentage = 50;
                        progressBar.Visible = true;
                        MediaPortal.Core.Transcoding.TranscodeInfo info = new MediaPortal.Core.Transcoding.TranscodeInfo();
                        info.Author = "MediaPortal";
                        //info.Channel = tinfo.recorded.Channel;
                        //info.Description = tinfo.recorded.Description;
                        info.Title = "test";//tinfo.recorded.Title;
                        //info.Start = tinfo.recorded.StartTime;
                        //info.End = tinfo.recorded.EndTime;
                        //TimeSpan ts = (tinfo.recorded.EndTime - tinfo.recorded.StartTime);
                        //info.Duration = (int)ts.TotalSeconds;
                        info.file = item.Path; //tinfo.recorded.FileName;
                        convert.CreateProfile(new System.Drawing.Size(360, 288), 2000, 25);
                        convert.Transcode(info, MediaPortal.Core.Transcoding.VideoFormat.Divx, MediaPortal.Core.Transcoding.Quality.High);
                        while (!convert.IsFinished())
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
				}

				else if (item.Label.Substring(1, 1) == ":")  // is a drive
				{
					currentFolder = item.Label;
					if (currentFolder != String.Empty)
						LoadListControl(currentFolder, extensions);
					else
						LoadShares();
					LoadDrives();
				}
				else
				{
					currentFolder = item.Path;
					LoadListControl(currentFolder, extensions);
				}
				if (item.Path == "")
				{
					LoadShares();
					LoadDrives();
				}
			}

			if (control == joinCutSpinCtrl)
			{
				if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2078))		//join
				{
					joinListCtrl.IsVisible = true;
					startJoinBtn.IsEnabled = true;
					startJoinBtn.Label = GUILocalizeStrings.Get(2079); //Start joining
					titelLbl.Label = GUILocalizeStrings.Get(2074);	//Please, choose the files you would like to join:
				}
				else if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2077))		//cut
				{
					joinListCtrl.IsVisible = false;
					startJoinBtn.IsEnabled = false;
					titelLbl.Label = GUILocalizeStrings.Get(2092);	//Please, choose a file you would like to edit:
				}
				else if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2071))		//dvr-ms to mpeg
				{
					joinListCtrl.IsVisible = false;
					startJoinBtn.IsEnabled = false;
					//startJoinBtn.Label = GUILocalizeStrings.Get(2072);    //Start converting
					titelLbl.Label = GUILocalizeStrings.Get(2073);		//"Please, choose a file you would like to convert:";
				}
                else if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2071))		//dvr-ms to mpeg
                {
                    joinListCtrl.IsVisible = false;
                    startJoinBtn.IsEnabled = false;
                    //startJoinBtn.Label = GUILocalizeStrings.Get(2072);    //Start converting
                    titelLbl.Label = GUILocalizeStrings.Get(2073);		//"Please, choose a file you would like to convert:";
                }
                else if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2066))		//mpeg to divx
                {
                    joinListCtrl.IsVisible = false;
                    startJoinBtn.IsEnabled = false;
                    //startJoinBtn.Label = GUILocalizeStrings.Get(2072);    //Start converting
                    titelLbl.Label = GUILocalizeStrings.Get(2065);		//"Please, choose the mpeg file you would like to convert to divx:";
                }
                else if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2068))		//dvr-ms to divx
                {
                    joinListCtrl.IsVisible = false;
                    startJoinBtn.IsEnabled = false;
                    //startJoinBtn.Label = GUILocalizeStrings.Get(2072);    //Start converting
                    titelLbl.Label = GUILocalizeStrings.Get(2067);		//"Please, choose the dvr-ms file you would like to convert to divx:";
                }
			}

			if (control == startJoinBtn)
			{
				if (joiningList[0].Extension.ToLower() == ".dvr-ms")
				{
					DvrMsModifier joinmod = new DvrMsModifier();
					if (joiningList[0] != null && joiningList[1] != null)
					{
						progressBar.Visible = true;
						joinmod.JoinDvr(joiningList);
						joinmod.OnFinished += new DvrMsModifier.Finished(joinmod_OnFinished);
						joinmod.OnProgress += new DvrMsModifier.Progress(OnProgress);
					}
					//else
					//System.Windows.Forms.MessageBox.Show("keineDatei");
				}
			}


			//System.Windows.Forms.MessageBox.Show(controlId.ToString() + "::" + control.Name + "::" + actionType.ToString());
			base.OnClicked(controlId, control, actionType);
		}

		void joinmod_OnFinished()
		{
			GUIDialogYesNo yesnoDialog = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
			yesnoDialog.SetHeading(2111); // Finished !
			yesnoDialog.SetLine(1, 2069); // //Finished to convert the video file
			yesnoDialog.SetLine(2, 2083); // Would you like to delete the original file?
			yesnoDialog.DoModal(GetID);
			if (yesnoDialog.IsConfirmed)
			{
				System.IO.File.Delete(filetoConvert);
			/*	recInfo = new TVRecorded();
				
				if (TVDatabase.GetRecordedTVByFilename(filetoConvert, ref recInfo))
				{
					TVDatabase.RemoveRecordedTV(recInfo);
					recInfo.FileName = System.IO.Path.ChangeExtension(filetoConvert, ".mpeg");
					TVDatabase.AddRecordedTV(recInfo);
				}*/
			}
			progressBar.Percentage = 0;
			progressBar.Visible = false;
			LoadListControl(currentFolder, extensions);
		}

		void dvrmsMod_OnFinished()
		{
			GUIDialogYesNo yesnoDialog = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
			yesnoDialog.SetHeading(2111); // Finished !
			yesnoDialog.SetLine(1, 2070); // //Finished to convert the video file
			yesnoDialog.SetLine(2, 2083); // Would you like to delete the original file?
			yesnoDialog.DoModal(GetID);
			if (yesnoDialog.IsConfirmed)
			{
				recInfo = new TVRecorded();
				System.IO.File.Delete(filetoConvert);
				if (TVDatabase.GetRecordedTVByFilename(filetoConvert, ref recInfo))
				{
					TVDatabase.RemoveRecordedTV(recInfo);
					recInfo.FileName = System.IO.Path.ChangeExtension(filetoConvert, ".mpeg");
					TVDatabase.AddRecordedTV(recInfo);
				}
			}
			progressBar.Percentage = 0;
			progressBar.Visible = false;
			extensions.Clear();
			extensions.Add(".dvr-ms");
			extensions.Add(".mpeg");
			extensions.Add(".mpg");
			extensions.Add(".ts");
			LoadListControl(currentFolder, extensions);
		}

		void OnProgress(int percentage)
		{
			progressBar.Percentage = percentage;
		}

		private void LoadJoinList()
		{
			joinListCtrl.Clear();
			foreach (System.IO.FileInfo file in joiningList)
			{
				joinListCtrl.Add(new GUIListItem(file.FullName));
			}
		}

		#endregion

	
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
		/// get the number of drives
		/// </summary>
		private void GetDrives()
		{
			cntDrives = 0;
			foreach (string drive in Environment.GetLogicalDrives())
			{
				switch ((DriveType)MediaPortal.Util.Utils.getDriveType(drive))
				{
					case DriveType.Removable:
					case DriveType.CD:
					//case DriveType.DVD:
					case DriveType.Fixed:
					case DriveType.RemoteDisk:
					case DriveType.RamDisk:
						drives[cntDrives] = drive;
						cntDrives++;
						break;
				}
			}
		}

		/// <summary>
		/// Add the drives to the listcontrol with the matching icons
		/// </summary>
		private void LoadDrives()
		{
			try
			{
				currentFolder = "";
				for (int i = 0; i < cntDrives; i++)
				{
					GUIListItem item = new GUIListItem(drives[i]);
					item.IsFolder = true;
					item.Path = drives[i];
					MediaPortal.Util.Utils.SetDefaultIcons(item);
					videoListLct.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("VideoEditor: (LoadDrives) " + ex.StackTrace);
			}
		}

		#region Eventhandler

		/// <summary>
		/// Load the list control with the items of the specified directory
		/// </summary>
		/// <param name="folder">Path of the director to load</param>
		/// <param name="exts">the extensions to show</param>
		private void LoadListControl(string folder, ArrayList exts)
		{
			try
			{
				if (folder != null && folder != "")
					folder = MediaPortal.Util.Utils.RemoveTrailingSlash(folder);
				
				//directory;
				ArrayList itemlist;
				//directory = new VirtualDirectory();
				directory.SetExtensions(exts);
				itemlist = directory.GetDirectory(folder);
				videoListLct.Clear();
				foreach (GUIListItem item in itemlist)
				{
					if (!item.IsFolder) // if item a folder
					{
						GUIListItem pItem = new GUIListItem(item.FileInfo.Name);
						pItem.FileInfo = item.FileInfo;
						pItem.IsFolder = false;
						pItem.Path = String.Format(@"{0}\{1}", folder, item.FileInfo.Name);
						videoListLct.Add(pItem);
					}
					else
					{
						GUIListItem pItem = new GUIListItem(item.Label);
						pItem.IsFolder = true;
						pItem.Path = String.Format(@"{0}\{1}", folder, item.Label);
						if (item.Label == "..")
						{
							string prevFolder = "";
							int pos = folder.LastIndexOf(@"\");
							if (pos >= 0) prevFolder = folder.Substring(0, pos);
							pItem.Path = prevFolder;
						}
						Utils.SetDefaultIcons(pItem);
						videoListLct.Add(pItem);
					}
				}
				currentFolder = folder;
			}
			catch (Exception ex)
			{
				Log.Error("VideoEditor: (LoadListControl) "+ ex.Message);
			}
		}

    protected override void OnShowContextMenu()
    {
      GUIListItem selected =  joinListCtrl.SelectedListItem;
      if (selected == null) return;
      else
      {
       /* GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        if (dlg == null) return;
        dlg.Reset();
        dlg.SetHeading(924); // menu
        dlg.Add("Löschen");
        dlg.DoModal(GetID);
        if (dlg.SelectedId == -1) return;*/
				joiningList.RemoveAt(joinListCtrl.SelectedListItemIndex);
				LoadJoinList();
				if (joiningList.Count <= 0)
				{
					extensions.Add(".dvr-ms");
					extensions.Add(".mpeg");
					extensions.Add(".mpg");
					extensions.Add(".ts");
					LoadListControl(currentFolder, extensions);
				}
        //joinListCtrl.RemoveSubItem(joinListCtrl.SelectedListItemIndex);//joinListCtrl.SelectedListItem.);//SelectedLabelText);
				//System.Windows.Forms.MessageBox.Show(selected.Label + "::" + joinListCtrl.SelectedItem.ToString() + "::" + joinListCtrl.SelectedListItemIndex.ToString());
      }
    }

    private void LoadShares()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        //ShowTrailerButton = xmlreader.GetValueAsBool("plugins", "My Trailers", true);
       // fileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);
        //fileMenuPinCode = Utils.DecryptPin(xmlreader.GetValueAsString("filemenu", "pincode", String.Empty));
        directory.Clear();
        videoListLct.Clear();
        string strDefault = xmlreader.GetValueAsString("movies", "default", String.Empty);
        for (int i = 0; i < 20; i++)
        {
          string strShareName = String.Format("sharename{0}", i);
          string strSharePath = String.Format("sharepath{0}", i);
          string strPincode = String.Format("pincode{0}", i);

          string shareType = String.Format("sharetype{0}", i);
          string shareServer = String.Format("shareserver{0}", i);
          string shareLogin = String.Format("sharelogin{0}", i);
          string sharePwd = String.Format("sharepassword{0}", i);
          string sharePort = String.Format("shareport{0}", i);
          string remoteFolder = String.Format("shareremotepath{0}", i);
          string shareViewPath = String.Format("shareview{0}", i);

          Share share = new Share();
          share.Name = xmlreader.GetValueAsString("movies", strShareName, String.Empty);
          share.Path = xmlreader.GetValueAsString("movies", strSharePath, String.Empty);
          string pinCode = Utils.DecryptPin(xmlreader.GetValueAsString("movies", strPincode, string.Empty));
          if (pinCode != string.Empty)
            share.Pincode = Convert.ToInt32(pinCode);
          else
            share.Pincode = -1;

          share.IsFtpShare = xmlreader.GetValueAsBool("movies", shareType, false);
          share.FtpServer = xmlreader.GetValueAsString("movies", shareServer, String.Empty);
          share.FtpLoginName = xmlreader.GetValueAsString("movies", shareLogin, String.Empty);
          share.FtpPassword = xmlreader.GetValueAsString("movies", sharePwd, String.Empty);
          share.FtpPort = xmlreader.GetValueAsInt("movies", sharePort, 21);
          share.FtpFolder = xmlreader.GetValueAsString("movies", remoteFolder, "/");
          share.DefaultView = (Share.Views)xmlreader.GetValueAsInt("movies", shareViewPath, (int)Share.Views.List);

          if (share.Name.Length > 0)
          {
            if (strDefault == share.Name)
            {
              share.Default = true;
              if (currentFolder.Length == 0)
              {
                currentFolder = share.Path;
              //  m_strDirectoryStart = share.Path;
              }
            }
            directory.Add(share);
          }
          else break;
        }
        //m_askBeforePlayingDVDImage = xmlreader.GetValueAsBool("daemon", "askbeforeplaying", false);
      }
      
      ArrayList itemlist = new ArrayList();
      itemlist = directory.GetRoot();
      foreach (GUIListItem item in itemlist)
      {
       // GUIListItem pItem = new GUIListItem(item.FileInfo.Name);
       // pItem.FileInfo = item.FileInfo;
      //  pItem.IsFolder = false;
       // pItem.Path = String.Format(@"{0}\{1}", folder, item.FileInfo.Name);
        videoListLct.Add(item);
      }
    }

		protected void ToCutScreen(string filepath)
		{
			try
			{
				if (filepath == null)
					System.Windows.Forms.MessageBox.Show("No path");
				if (cutScr == null)
				{
					cutScr = new VideoEditorPreview(filepath);
					cutScr.Init();
					if (GUIWindowManager.GetWindow(cutScr.GetID) == null)
					{
						GUIWindow win = (GUIWindow)cutScr;
						GUIWindowManager.Add(ref win);
					}
				}
				else
					cutScr.CutFileName = filepath;

				GUIWindowManager.ActivateWindow(cutScr.GetID);
			}
			catch (Exception ex)
			{
				Log.Error("VideoEditor: (ToCutScreen) " + ex.Message);
			}
		}
		#endregion
	}
}
