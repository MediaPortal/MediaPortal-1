#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#endregion

using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Util;
using MediaPortal.TagReader;
using MediaPortal.Music.Database;
using MediaPortal.GUI.Library;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class Wizard_SelectPlugins : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPLabel labelHD;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel labelFolder;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPLabel labelMusicCount;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPLabel labelPhotoCount;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPLabel labelMovieCount;
    private MediaPortal.UserInterface.Controls.MPButton buttonStopStart;
    private System.ComponentModel.IContainer components = null;

    const int MaximumShares = 20;
    long totalAudio;
    long totalVideo;
    long totalPhotos;
    ArrayList sharesVideos = new ArrayList();
    ArrayList sharesMusic = new ArrayList();
    ArrayList sharesPhotos = new ArrayList();
    bool stopScanning = false;
    private System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.Timer timer1;
    private MediaPortal.UserInterface.Controls.MPLabel fileLabel;
    bool isScanning = false;

    public Wizard_SelectPlugins()
      : this("Media Search")
    {
    }

    public Wizard_SelectPlugins(string name)
      : base(name)
    {

      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
      SetDefaultShares();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.fileLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.buttonStopStart = new MediaPortal.UserInterface.Controls.MPButton();
      this.labelMovieCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelPhotoCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelMusicCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelFolder = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelHD = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.fileLabel);
      this.groupBox1.Controls.Add(this.progressBar1);
      this.groupBox1.Controls.Add(this.buttonStopStart);
      this.groupBox1.Controls.Add(this.labelMovieCount);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.labelPhotoCount);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.labelMusicCount);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.labelFolder);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.labelHD);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Find local media";
      // 
      // fileLabel
      // 
      this.fileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.fileLabel.Location = new System.Drawing.Point(16, 288);
      this.fileLabel.Name = "fileLabel";
      this.fileLabel.Size = new System.Drawing.Size(440, 23);
      this.fileLabel.TabIndex = 13;
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar1.Location = new System.Drawing.Point(16, 264);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(440, 16);
      this.progressBar1.TabIndex = 12;
      this.progressBar1.Visible = false;
      // 
      // buttonStopStart
      // 
      this.buttonStopStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonStopStart.Location = new System.Drawing.Point(384, 228);
      this.buttonStopStart.Name = "buttonStopStart";
      this.buttonStopStart.Size = new System.Drawing.Size(72, 22);
      this.buttonStopStart.TabIndex = 11;
      this.buttonStopStart.Text = "Scan";
      this.buttonStopStart.Click += new System.EventHandler(this.buttonStop_Click);
      // 
      // labelMovieCount
      // 
      this.labelMovieCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.labelMovieCount.Location = new System.Drawing.Point(176, 232);
      this.labelMovieCount.Name = "labelMovieCount";
      this.labelMovieCount.Size = new System.Drawing.Size(200, 16);
      this.labelMovieCount.TabIndex = 10;
      this.labelMovieCount.Text = "-";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 232);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(128, 16);
      this.label6.TabIndex = 9;
      this.label6.Text = "Total movies found:";
      // 
      // labelPhotoCount
      // 
      this.labelPhotoCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.labelPhotoCount.Location = new System.Drawing.Point(176, 208);
      this.labelPhotoCount.Name = "labelPhotoCount";
      this.labelPhotoCount.Size = new System.Drawing.Size(280, 16);
      this.labelPhotoCount.TabIndex = 8;
      this.labelPhotoCount.Text = "-";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 208);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(128, 16);
      this.label5.TabIndex = 7;
      this.label5.Text = "Total photo\'s found:";
      // 
      // labelMusicCount
      // 
      this.labelMusicCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.labelMusicCount.Location = new System.Drawing.Point(176, 184);
      this.labelMusicCount.Name = "labelMusicCount";
      this.labelMusicCount.Size = new System.Drawing.Size(280, 16);
      this.labelMusicCount.TabIndex = 6;
      this.labelMusicCount.Text = "-";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 184);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(128, 16);
      this.label4.TabIndex = 5;
      this.label4.Text = "Total music files found:";
      // 
      // labelFolder
      // 
      this.labelFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.labelFolder.Location = new System.Drawing.Point(128, 96);
      this.labelFolder.Name = "labelFolder";
      this.labelFolder.Size = new System.Drawing.Size(328, 64);
      this.labelFolder.TabIndex = 4;
      this.labelFolder.Text = "-";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 104);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(40, 16);
      this.label3.TabIndex = 3;
      this.label3.Text = "Folder:";
      // 
      // labelHD
      // 
      this.labelHD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.labelHD.Location = new System.Drawing.Point(128, 72);
      this.labelHD.Name = "labelHD";
      this.labelHD.Size = new System.Drawing.Size(328, 16);
      this.labelHD.TabIndex = 2;
      this.labelHD.Text = "-";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 72);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(56, 16);
      this.label2.TabIndex = 1;
      this.label2.Text = "Harddisk:";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(408, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "MediaPortal will now search your harddisk(s) for any music, photo\'s and movies.";
      // 
      // timer1
      // 
      this.timer1.Interval = 1000;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // Wizard_SelectPlugins
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "Wizard_SelectPlugins";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

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

    private void buttonStop_Click(object sender, System.EventArgs e)
    {
      if (!isScanning)
      {
        buttonStopStart.Text = "Stop";
        DoScan();
        buttonStopStart.Text = "Scan...";
      }
      else
      {
        stopScanning = true;
      }
    }

    void SetDefaultShares()
    {
      //string[] drives = Environment.GetLogicalDrives();
      //foreach (string drive in drives)
      //{
      //  int driveType = Util.Utils.getDriveType(drive);
      //  if (driveType == (int)DriveType.DVD)
      //  {
      //    string driveName = String.Format("({0}:) CD/DVD", drive.Substring(0, 1).ToUpper());
      //    Shares.ShareData share = new Shares.ShareData(driveName, drive, "");
      //    sharesMusic.Add(share);
      //    sharesPhotos.Add(share);
      //    sharesVideos.Add(share);
      //  }
      //}

      //using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      //{
      //  xmlwriter.SetValue("music", "default", AddAudioShare(Util.Win32API.GetFolderPath(Util.Win32API.CSIDL_MYMUSIC)));
      //  xmlwriter.SetValue("pictures", "default", AddPhotoShare(Util.Win32API.GetFolderPath(Util.Win32API.CSIDL_MYPICTURES)));
      //  xmlwriter.SetValue("movies", "default", AddVideoShare(Util.Win32API.GetFolderPath(Util.Win32API.CSIDL_MYVIDEO)));
      //}
      //SaveShare(sharesMusic, "music");
      //SaveShare(sharesPhotos, "pictures");
      //SaveShare(sharesVideos, "movies");
      MediaPortal.Util.VirtualDirectory.SetInitialDefaultShares(true, true, true, true);
    }

    void DoScan()
    {
      isScanning = true;
      stopScanning = false;
      totalAudio = 0;
      totalPhotos = 0;
      totalVideo = 0;
      fileLabel.Text = "";
      progressBar1.Visible = true;
      progressBar1.Value = 0;
      timer1.Enabled = true;
      string[] drives = Environment.GetLogicalDrives();
      foreach (string drive in drives)
      {
        int driveType = Util.Utils.getDriveType(drive);
        if (driveType != (int)DriveType.Fixed) continue;
        labelHD.Text = String.Format("{0}", drive);
        ScanFolder(labelHD.Text, true, true, true);
        if (stopScanning) break;
      }

      ScanMusic();
      progressBar1.Visible = false;
      timer1.Enabled = false;
      labelMovieCount.Text = totalVideo.ToString();
      labelPhotoCount.Text = totalPhotos.ToString();
      labelMusicCount.Text = totalAudio.ToString();
      labelHD.Text = "";
      labelFolder.Text = "";
      isScanning = false;
    }

    void ScanMusic()
    {
      timer1.Enabled = false;
      progressBar1.Value = 0;
      MediaPortal.Music.Database.MusicDatabase m_dbs = MediaPortal.Music.Database.MusicDatabase.Instance;
      MediaPortal.Music.Database.MusicDatabase.DatabaseReorgChanged += new MusicDBReorgEventHandler(SetPercentDonebyEvent);
      int appel = m_dbs.MusicDatabaseReorg(null);
    }

    void SetPercentDonebyEvent(object sender, DatabaseReorgEventArgs e)
    {
      progressBar1.Value = e.progress;
      SetStatus(e.phase);
    }

    private void SetStatus(string status)
    {
      fileLabel.Text = status;
      System.Windows.Forms.Application.DoEvents();
    }

    void ScanFolder(string folder, bool scanForAudio, bool scanForVideo, bool scanForPhotos)
    {
      //dont go into dvd folders
      if (folder.ToLower().IndexOf(@"\video_ts") >= 0) return;
      if (folder.ToLower().IndexOf(@":\recycler") >= 0) return;
      if (folder.ToLower().IndexOf(@":\$win") >= 0) return;
      string[] files;
      string[] folders;
      try
      {
        string systemFolder = Environment.SystemDirectory;
        int pos = systemFolder.LastIndexOf(@"\");
        string windowsFolder = systemFolder.Substring(0, pos);
        if (folder.IndexOf(windowsFolder) >= 0) return;
        if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) >= 0) return;
        if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) >= 0) return;
        if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)) >= 0) return;
        if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles)) >= 0) return;
        if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.Cookies)) >= 0) return;
        if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)) >= 0) return;
        if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)) >= 0) return;
        if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.Favorites)) >= 0) return;
        if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.History)) >= 0) return;
        if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache)) >= 0) return;
        if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) >= 0) return;
        if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.Recent)) >= 0) return;
        files = Directory.GetFiles(folder);
        folders = Directory.GetDirectories(folder);
      }
      catch (Exception)
      {
        return;
      }
      bool isAudioFolder = false;
      bool isVideoFolder = false;
      bool isPhotoFolder = false;
      labelFolder.Text = folder;
      labelMovieCount.Text = totalVideo.ToString();
      labelPhotoCount.Text = totalPhotos.ToString();
      labelMusicCount.Text = totalAudio.ToString();
      System.Windows.Forms.Application.DoEvents();
      long videoCount = totalVideo;
      long audioCount = totalAudio;
      long photoCount = totalPhotos;
      bool isDVD = false;

      foreach (string file in files)
      {
        string ext = System.IO.Path.GetExtension(file).ToLower();
        if (ext == ".exe" || ext == ".dll" || ext == ".ocx")
        {
          isAudioFolder = false;
          isVideoFolder = false;
          isPhotoFolder = false;
          break;
        }

        if (stopScanning) return;
        if (MediaPortal.Util.Utils.IsAudio(file))
        {
          totalAudio++;
          if (scanForAudio)
          {
            isAudioFolder = true;
            scanForAudio = false;//no need to scan subfolders
          }
        }
        if (MediaPortal.Util.Utils.IsVideo(file))
        {
          totalVideo++;
          if (scanForVideo)
          {
            isVideoFolder = true;
            scanForVideo = false;//no need to scan subfolders
          }
        }
        if (MediaPortal.Util.Utils.IsPicture(file))
        {
          if (file.Length < 260)
          {
            if (file.ToLower() != "folder.jpg")
            {
              FileInfo info = new FileInfo(file);
              if (info.Length >= 500 * 1024) // > 500KByte
              {
                totalPhotos++;
                if (scanForPhotos)
                {
                  isPhotoFolder = true;
                  scanForPhotos = false;//no need to scan subfolders
                }
              }
            }
          }
          else
            Log.Info("ScanFolder: Path > 260: {0}", file);
        }
      }
      foreach (string subfolder in folders)
      {
        if (stopScanning) return;
        if (subfolder != "." && subfolder != "..")
        {
          if (scanForVideo)
          {
            foreach (string tmpFolder in folders)
            {
              try
              {
                string[] subfolders = Directory.GetDirectories(tmpFolder);
                if (subfolder.ToLower().IndexOf(@"\video_ts") >= 0)
                {
                  isDVD = true;
                }
              }
              catch (Exception) { }
            }
          }
          if (isDVD && !isVideoFolder) AddVideoShare(folder);
          ScanFolder(subfolder, scanForAudio, scanForVideo, scanForPhotos);
        }
      }
      if (isAudioFolder)
      {
        audioCount = (totalAudio - audioCount);
        if (audioCount >= 5)
          AddAudioShare(folder);
      }
      if (isVideoFolder)
      {
        videoCount = (totalVideo - videoCount);
        AddVideoShare(folder);
      }
      if (isPhotoFolder)
      {
        photoCount = (totalPhotos - photoCount);
        if (photoCount >= 5)
          AddPhotoShare(folder);
      }
    }

    string AddAudioShare(string folder)
    {
      string name = folder;
      int pos = folder.LastIndexOf(@"\");
      if (pos > 0)
      {
        name = name.Substring(pos + 1);
      }
      BaseShares.ShareData share = new BaseShares.ShareData(name, folder, "");
      sharesMusic.Add(share);
      return name;
    }

    string AddVideoShare(string folder)
    {
      string name = folder;
      int pos = folder.LastIndexOf(@"\");
      if (pos > 0)
      {
        name = name.Substring(pos + 1);
      }
      BaseShares.ShareData share = new BaseShares.ShareData(name, folder, "");
      sharesVideos.Add(share);
      return name;
    }

    string AddPhotoShare(string folder)
    {
      string name = folder;
      int pos = folder.LastIndexOf(@"\");
      if (pos > 0)
      {
        name = name.Substring(pos + 1);
      }
      BaseShares.ShareData share = new BaseShares.ShareData(name, folder, "");
      sharesPhotos.Add(share);
      return name;
    }

    void SaveShare(ArrayList sharesList, string mediaType)
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        for (int index = 0; index < MaximumShares; index++)
        {
          string shareName = String.Format("sharename{0}", index);
          string sharePath = String.Format("sharepath{0}", index);
          string sharePin = String.Format("pincode{0}", index);

          string shareType = String.Format("sharetype{0}", index);
          string shareServer = String.Format("shareserver{0}", index);
          string shareLogin = String.Format("sharelogin{0}", index);
          string sharePwd = String.Format("sharepassword{0}", index);
          string sharePort = String.Format("shareport{0}", index);
          string shareRemotePath = String.Format("shareremotepath{0}", index);

          string shareNameData = string.Empty;
          string sharePathData = string.Empty;
          string sharePinData = string.Empty;

          bool shareTypeData = false;
          string shareServerData = string.Empty;
          string shareLoginData = string.Empty;
          string sharePwdData = string.Empty;
          int sharePortData = 21;
          string shareRemotePathData = string.Empty;

          if (sharesList != null && sharesList.Count > index)
          {
            BaseShares.ShareData shareData = sharesList[index] as BaseShares.ShareData;

            if (shareData != null)
            {
              shareNameData = shareData.Name;
              sharePathData = shareData.Folder;
              sharePinData = shareData.PinCode;

              shareTypeData = shareData.IsRemote;
              shareServerData = shareData.Server;
              shareLoginData = shareData.LoginName;
              sharePwdData = shareData.PassWord;
              sharePortData = shareData.Port;
              shareRemotePathData = shareData.RemoteFolder;

            }
          }

          xmlwriter.SetValue(mediaType, shareName, shareNameData);
          xmlwriter.SetValue(mediaType, sharePath, sharePathData);
          xmlwriter.SetValue(mediaType, sharePin, sharePinData);

          xmlwriter.SetValueAsBool(mediaType, shareType, shareTypeData);
          xmlwriter.SetValue(mediaType, shareServer, shareServerData);
          xmlwriter.SetValue(mediaType, shareLogin, shareLoginData);
          xmlwriter.SetValue(mediaType, sharePwd, sharePwdData);
          xmlwriter.SetValue(mediaType, sharePort, sharePortData.ToString());
          xmlwriter.SetValue(mediaType, shareRemotePath, shareRemotePathData);
        }

      }
    }

    private void timer1_Tick(object sender, System.EventArgs e)
    {
      if (progressBar1.Value + 1 < 100) progressBar1.Value++;
      else progressBar1.Value = 0;
    }
  }
}

