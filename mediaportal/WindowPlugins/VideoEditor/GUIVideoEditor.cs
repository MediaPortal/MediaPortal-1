#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Core.Transcoding;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using Mpeg2SplitterPackage;

namespace WindowPlugins.VideoEditor
{
  public class GUIVideoEditor : GUIWindow
  {
    #region GUIControls

    [SkinControl(23)] protected GUILabelControl titelLbl = null;
    [SkinControl(24)] protected GUIButtonControl backBtn = null;
    //[SkinControlAttribute(25)]		//protected GUIButtonControl cancelBtn = null;
    [SkinControl(32)] protected GUILabelControl startTime = null;
    [SkinControl(34)] protected GUILabelControl oldDurationLbl = null;
    [SkinControl(101)] protected GUIListControl videoListLct = null;
    [SkinControl(102)] protected GUISpinControl joinCutSpinCtrl = null;
    [SkinControl(99)] protected GUIVideoControl videoWindow = null;
    [SkinControl(103)] protected GUIListControl joinListCtrl = null;
    [SkinControl(104)] protected GUIButtonControl startJoinBtn = null;
    [SkinControl(105)] protected GUIProgressControl progressBar = null;
    [SkinControl(106)] protected GUILabelControl progressPercent = null;

    #endregion

    #region Own Variables

    private const int maxDrives = 50;
    private int cntDrives = 0;
    private string[] drives = new string[maxDrives];
    private string currentFolder = "";
    private VirtualDirectory directory = VirtualDirectories.Instance.Movies;
    private ArrayList extensions;
    private VideoEditorPreview cutScr;
    private List<FileInfo> joiningList;
    private string filetoConvert;
    private DvrMsModifier dvrmsMod;
    private Thread joinThread;
    private bool working = false;

    #endregion

    public GUIVideoEditor()
    {
      GetID = (int) Window.WINDOW_VIDEO_EDITOR;
    }

    #region Overrides

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
        using (Settings xmlreader = new MPSettings())
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
        if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2078)) //Join
        {
          joinListCtrl.IsVisible = true;
          startJoinBtn.IsEnabled = true;
          titelLbl.Label = GUILocalizeStrings.Get(2074); //Please, choose the files you would like to join:
        }
        joiningList = new List<FileInfo>();
        progressBar.Visible = false;

        if (currentFolder == "")
        {
          LoadShares();
          LoadDrives();
        }
        else
        {
          LoadListControl(currentFolder, extensions);
        }
        CheckHasMencoder();
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      g_Player.Release();
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("VideoEditor", "lastUsedFolder", currentFolder);
      }
      base.OnPageDestroy(new_windowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (working)
      {
        base.OnClicked(controlId, control, actionType);
        return;
      }
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
          if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2077)) //Cut
          {
            ToCutScreen(item.Path);
          }
          if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2078)) //join
          {
            joiningList.Add(new FileInfo(item.Path));
            extensions.Clear();
            extensions.Add(Path.GetExtension(item.Path));
            LoadListControl(currentFolder, extensions);
            // joinListCtrl.Add(new GUIListItem(item.Path));
            LoadJoinList();
          }
          if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2071)) //Dvr-ms to mpeg
          {
            filetoConvert = item.Path;
            progressBar.Percentage = 0;
            progressBar.Visible = true;
            int duration;
            g_Player.Play(item.Path);
            duration = (int) g_Player.Duration;
            g_Player.Stop();
            dvrmsMod = new DvrMsModifier();
            dvrmsMod.OnProgress += new DvrMsModifier.Progress(OnProgress);
            dvrmsMod.OnFinished += new DvrMsModifier.Finished(dvrmsMod_OnFinished);
            dvrmsMod.TranscodeToMpeg(new FileInfo(item.Path), duration);
          }
          if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2066)) // Mpeg to divx
          {
            CompressionSettings comprSettings = new CompressionSettings();
            CompressSettings settingWindow =
              (CompressSettings) GUIWindowManager.GetWindow((int) Window.WINDOW_VIDEO_EDITOR_COMPRESSSETTINGS);
            if (settingWindow == null)
            {
              return;
            }
            //            settingWindow.Settings = comprSettings;   BAV: do you need this one here or only line 216
            settingWindow.DoModal(this.GetID);
            if (settingWindow.Result)
            {
              settingWindow.Reset();
              progressBar.Percentage = 0;
              progressPercent.Label = "0";
              progressBar.Visible = true;
              progressPercent.Visible = true;
              videoListLct.Focusable = false;
              comprSettings = settingWindow.Settings;
              EditSettings settings = new EditSettings(comprSettings);
              settings.FileName = item.Path;
              settings.DeleteAfter = false;
              Converter divxConverter = new Converter(settings);
              divxConverter.OnProgress += new Converter.Progress(OnProgress);
              divxConverter.OnFinished += new Converter.Finished(divxConverter_OnFinished);
              divxConverter.CheckHasMencoder();
              working = true;
              divxConverter.Convert(VideoFormat.Divx);
            }
          }

          if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2068)) // Dvrms to divx
          {
            filetoConvert = item.Path;
            progressBar.Percentage = 0;
            progressBar.Visible = true;
            int duration;
            g_Player.Play(item.Path);
            duration = (int) g_Player.Duration;
            g_Player.Stop();
            g_Player.Release();
            Thread.Sleep(1000);
            dvrmsMod = new DvrMsModifier();
            dvrmsMod.OnProgress += new DvrMsModifier.Progress(OnProgress);
            dvrmsMod.OnFinished += new DvrMsModifier.Finished(dvrmsMod_OnFinished);
            dvrmsMod.ConvertToDivx(new FileInfo(item.Path), duration);
          }
        }

        else if (item.Label.Substring(1, 1) == ":") // is a drive
        {
          currentFolder = item.Label;
          if (currentFolder != string.Empty)
          {
            LoadListControl(currentFolder, extensions);
          }
          else
          {
            LoadShares();
          }
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
        if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2078)) //join
        {
          joinListCtrl.IsVisible = true;
          startJoinBtn.IsEnabled = true;
          startJoinBtn.Label = GUILocalizeStrings.Get(2079); //Start joining
          titelLbl.Label = GUILocalizeStrings.Get(2074); //Please, choose the files you would like to join:
          extensions.Clear();
          extensions.Add(".dvr-ms");
          extensions.Add(".mpeg");
          extensions.Add(".mpg");
          extensions.Add(".ts");
          LoadListControl(currentFolder, extensions);
        }
        else if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2077)) //cut
        {
          joinListCtrl.IsVisible = false;
          startJoinBtn.IsEnabled = false;
          titelLbl.Label = GUILocalizeStrings.Get(2092); //Please, choose a file you would like to edit:
          extensions.Clear();
          extensions.Add(".dvr-ms");
          extensions.Add(".mpeg");
          extensions.Add(".mpg");
          extensions.Add(".ts");
          LoadListControl(currentFolder, extensions);
        }
        else if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2071)) //dvr-ms to mpeg
        {
          joinListCtrl.IsVisible = false;
          startJoinBtn.IsEnabled = false;
          //startJoinBtn.Label = GUILocalizeStrings.Get(2072);    //Start converting
          titelLbl.Label = GUILocalizeStrings.Get(2073); //"Please, choose a file you would like to convert:";
          extensions.Clear();
          extensions.Add(".dvr-ms");
          LoadListControl(currentFolder, extensions);
        }
        else if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2071)) //dvr-ms to mpeg
        {
          joinListCtrl.IsVisible = false;
          startJoinBtn.IsEnabled = false;
          //startJoinBtn.Label = GUILocalizeStrings.Get(2072);    //Start converting
          titelLbl.Label = GUILocalizeStrings.Get(2073); //"Please, choose a file you would like to convert:";
        }
        else if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2066)) //mpeg to divx
        {
          joinListCtrl.IsVisible = false;
          startJoinBtn.IsEnabled = false;
          //startJoinBtn.Label = GUILocalizeStrings.Get(2072);    //Start converting
          titelLbl.Label = GUILocalizeStrings.Get(2065);
            //"Please, choose the mpeg file you would like to convert to divx:";
          extensions.Clear();
          extensions.Add(".mpeg");
          extensions.Add(".mpg");
          LoadListControl(currentFolder, extensions);
        }
        else if (joinCutSpinCtrl.GetLabel() == GUILocalizeStrings.Get(2068)) //dvr-ms to divx
        {
          joinListCtrl.IsVisible = false;
          startJoinBtn.IsEnabled = false;
          //startJoinBtn.Label = GUILocalizeStrings.Get(2072);    //Start converting
          titelLbl.Label = GUILocalizeStrings.Get(2067);
            //"Please, choose the dvr-ms file you would like to convert to divx:";
        }
      }

      if (control == startJoinBtn)
      {
        if (joiningList[0] != null && joiningList[1] != null)
        {
          if (joiningList[0].Extension.ToLower() == ".dvr-ms")
          {
            DvrMsModifier joinmod = new DvrMsModifier();
            {
              progressBar.Visible = true;
              joinmod.JoinDvr(joiningList);
              joinmod.OnFinished += new DvrMsModifier.Finished(joinmod_OnFinished);
              joinmod.OnProgress += new DvrMsModifier.Progress(OnProgress);
            }
          }
          else if ((joiningList[0].Extension.ToLower() == ".mpeg") || (joiningList[0].Extension.ToLower() == ".mpg"))
          {
            joinThread = new Thread(new ThreadStart(joinMpeg));
            joinThread.Priority = ThreadPriority.BelowNormal;
            joinThread.Name = "VideoJoiner";
            joinThread.Start();
          }
        }
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void divxConverter_OnFinished()
    {
      progressBar.Percentage = 100;
      progressPercent.Label = "100";
      GUIDialogYesNo yesnoDialog = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      yesnoDialog.SetHeading(2111); // Finished !
      yesnoDialog.SetLine(1, 2070); // //Finished to convert the video file
      yesnoDialog.SetLine(2, 2083); // Would you like to delete the original file?
      yesnoDialog.DoModal(GetID);
      if (yesnoDialog.IsConfirmed)
      {
        try
        {
          File.Delete(filetoConvert);
        }
        catch
        {
        }
      }
      working = false;
      progressBar.Visible = false;
      progressPercent.Visible = false;
      videoListLct.Focusable = true;
      extensions.Clear();
      extensions.Add(".mpeg");
      extensions.Add(".mpg");
      extensions.Add(".ts");
      LoadListControl(currentFolder, extensions);
    }

    private void joinMpeg()
    {
      FileInfo outFilename;
      string outPath;
      if (joiningList[0].Extension.ToLower() == ".mpeg")
      {
        outPath = joiningList[0].FullName.Replace(".mpeg", "_joined.mpeg");
      }
      else //if (joiningList[0].Extension.ToLower() == ".mpg")
      {
        outPath = joiningList[0].FullName.Replace(".mpg", "_joined.mpg");
      }
      outFilename = new FileInfo(outPath);

      if (outFilename.Exists)
      {
        outFilename.Delete();
      }
      Mpeg2Splitter cMpeg2Splitter = new Mpeg2Splitter();
      cMpeg2Splitter.OnProgress += new Mpeg2Splitter.Progress(OnProgress);
      cMpeg2Splitter.OnFinished += new Mpeg2Splitter.Finished(joinmod_OnFinished);
      cMpeg2Splitter.Join(joiningList, outFilename.FullName);
      //progressLbl.Label = "100";
      progressBar.Percentage = 100;
    }

    private void joinmod_OnFinished()
    {
      progressBar.Percentage = 100;
      progressPercent.Label = "100";
      GUIDialogYesNo yesnoDialog = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      yesnoDialog.SetHeading(2111); // Finished !
      yesnoDialog.SetLine(1, 2069); // //Finished to convert the video file
      yesnoDialog.SetLine(2, 2083); // Would you like to delete the original file?
      yesnoDialog.DoModal(GetID);
      if (yesnoDialog.IsConfirmed)
      {
        File.Delete(filetoConvert);
        /*	recInfo = new TVRecorded();
				
            if (TVDatabase.GetRecordedTVByFilename(filetoConvert, ref recInfo))
            {
                TVDatabase.RemoveRecordedTV(recInfo);
                recInfo.FileName = System.IO.Path.ChangeExtension(filetoConvert, ".mpeg");
                TVDatabase.AddRecordedTV(recInfo);
            }*/
      }

      progressBar.Visible = false;
      progressPercent.Visible = false;
      working = false;
      LoadListControl(currentFolder, extensions);
    }

    private void dvrmsMod_OnFinished()
    {
      progressBar.Percentage = 100;
      progressPercent.Label = "100";
      GUIDialogYesNo yesnoDialog = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      yesnoDialog.SetHeading(2111); // Finished !
      yesnoDialog.SetLine(1, 2070); // //Finished to convert the video file
      yesnoDialog.SetLine(2, 2083); // Would you like to delete the original file?
      yesnoDialog.DoModal(GetID);
      if (yesnoDialog.IsConfirmed)
      {
        File.Delete(filetoConvert);
      }

      progressBar.Visible = false;
      progressPercent.Visible = false;
      extensions.Clear();
      extensions.Add(".dvr-ms");
      extensions.Add(".mpeg");
      extensions.Add(".mpg");
      extensions.Add(".ts");
      LoadListControl(currentFolder, extensions);
    }

    private void OnProgress(int percentage)
    {
      progressBar.Percentage = percentage;
      progressPercent.Label = percentage.ToString();
    }

    private void LoadJoinList()
    {
      joinListCtrl.Clear();
      foreach (FileInfo file in joiningList)
      {
        joinListCtrl.Add(new GUIListItem(file.FullName));
      }
    }

    #endregion

    public void CheckHasMencoder()
    {
      string mencoderPath = "";
      using (Settings xmlreader = new MPSettings())
      {
        mencoderPath = xmlreader.GetValueAsString("VideoEditor", "mencoder", "");
      }
      bool hasMencoder = File.Exists(mencoderPath);
      if (!hasMencoder)
      {
        hasMencoder = File.Exists(Application.StartupPath + @"\mencoder.exe");
        if (hasMencoder)
        {
          mencoderPath = Application.StartupPath + @"\mencoder.exe";
        }
      }
      if (!hasMencoder)
      {
        //isnt working that way:	joinCutSpinCtrl.RemoveSubItem(joinCutSpinCtrl.SubItemCount - 1);
      }
    }

    private enum DriveType
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
        switch ((DriveType) Utils.getDriveType(drive))
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
          Utils.SetDefaultIcons(item);
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
        {
          folder = Utils.RemoveTrailingSlash(folder);
        }

        //directory = new VirtualDirectory();
        directory.SetExtensions(exts);
        List<GUIListItem> itemlist = directory.GetDirectoryExt(folder);
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
            pItem.Path = item.Path;
            if (item.Label == "..")
            {
              string prevFolder = "";
              int pos = folder.LastIndexOf(@"\");
              if (pos >= 0)
              {
                prevFolder = folder.Substring(0, pos);
              }
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
        Log.Error("VideoEditor: (LoadListControl) " + ex.Message);
      }
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem selected = joinListCtrl.SelectedListItem;
      if (selected == null)
      {
        return;
      }
      else
      {
        /* GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
         if (dlg == null) return;
         dlg.Reset();
         dlg.SetHeading(498); // menu
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
      using (Settings xmlreader = new MPSettings())
      {
        //ShowTrailerButton = xmlreader.GetValueAsBool("plugins", "My Trailers", true);
        // fileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);
        //fileMenuPinCode = Utils.DecryptPin(xmlreader.GetValueAsString("filemenu", "pincode", string.Empty));
        directory.Clear();
        videoListLct.Clear();
        string strDefault = xmlreader.GetValueAsString("movies", "default", string.Empty);
        for (int i = 0; i < VirtualDirectory.MaxSharesCount; i++)
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
          share.Name = xmlreader.GetValueAsString("movies", strShareName, string.Empty);
          share.Path = xmlreader.GetValueAsString("movies", strSharePath, string.Empty);
          string pinCode = Utils.DecryptPin(xmlreader.GetValueAsString("movies", strPincode, string.Empty));
          if (pinCode != string.Empty)
          {
            share.Pincode = Convert.ToInt32(pinCode);
          }
          else
          {
            share.Pincode = -1;
          }

          share.IsFtpShare = xmlreader.GetValueAsBool("movies", shareType, false);
          share.FtpServer = xmlreader.GetValueAsString("movies", shareServer, string.Empty);
          share.FtpLoginName = xmlreader.GetValueAsString("movies", shareLogin, string.Empty);
          share.FtpPassword = xmlreader.GetValueAsString("movies", sharePwd, string.Empty);
          share.FtpPort = xmlreader.GetValueAsInt("movies", sharePort, 21);
          share.FtpFolder = xmlreader.GetValueAsString("movies", remoteFolder, "/");
          share.DefaultView = (Share.Views) xmlreader.GetValueAsInt("movies", shareViewPath, (int) Share.Views.List);

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
          else
          {
            break;
          }
        }
        //m_askBeforePlayingDVDImage = xmlreader.GetValueAsBool("daemon", "askbeforeplaying", false);
      }

      List<GUIListItem> itemlist = directory.GetRootExt();
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
        {
          MessageBox.Show("No path");
        }
        if (cutScr == null)
        {
          cutScr = new VideoEditorPreview(filepath);
          cutScr.Init();
          if (GUIWindowManager.GetWindow(cutScr.GetID) == null)
          {
            GUIWindow win = (GUIWindow) cutScr;
            GUIWindowManager.Add(ref win);
          }
        }
        else
        {
          cutScr.CutFileName = filepath;
        }

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