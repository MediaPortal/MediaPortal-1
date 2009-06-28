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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Playlists;
using MediaPortal.Ripper;
using MediaPortal.TagReader;
using MediaPortal.Util;
using XPBurn;

namespace MediaPortal.GUI.GUIBurner
{
  /// <summary>
  /// Summary description for GUIBurner.
  /// </summary>
  [PluginIcons("WindowPlugins.GUIBurner.Burner.gif", "WindowPlugins.GUIBurner.BurnerDisabled.gif")]
  public class GUIBurner : GUIWindow, ISetupForm, IShowPlugin
  {
    #region Private Enumerations

    private enum Controls : int
    {
      CONTROL_BUTTON1 = 2,
      CONTROL_BUTTON2 = 3,
      CONTROL_BUTTON3 = 4,
      CONTROL_BUTTON4 = 5,
      CONTROL_BUTTON5 = 6,
      CONTROL_BUTTON6 = 7,

      CONTROL_LIST_DIR = 20,
      CONTROL_LIST_COPY = 30,
      CONTROL_CD_DETAILS = 50
    } ;

    private enum States
    {
      STATE_MAIN = 0,
      STATE_VIDEO,
      STATE_AUDIO,
      STATE_DATA,
      STATE_MAKE_AUDIO_CD,
      STATE_MAKE_DATA_CD,
      STATE_MAKE_VIDEO_DVD,
      STATE_MAKE_DATA_DVD,
      STATE_DISK_INFO,
    } ;

    private enum BurnTypes : int
    {
      VIDEO_CD = 1,
      VIDEO_DVD = 2,
      AUDIO_CD = 3,
      DATA_CD = 4,
      DATA_DVD = 5
    } ;

    private BurnTypes burnType = BurnTypes.AUDIO_CD;
    private States currentState = States.STATE_MAIN;

    #endregion

    #region Private Variables

    private BurnVideoDVD VideoDvdBurner;
    private BurnDataDVD DataDvdBurner;

    private struct file
    {
      public string name;
      public long size;
      public string path;
    }

    private XPBurnCD CDBurner = null; // Microsoft code from http://msdn.microsoft.com/vcsharp/downloads/samples/xpburn/
    // http://download.microsoft.com/download/6/9/c/69c5d1b7-e3ac-4986-99f1-0c55dc374d66/xpburn.msi

    //string[] video = new string[50];
    //string[] vname = new string[50];
    //string[] sound = new string[50];
    //string[] sname = new string[50];
    //string[] pictures = new string[50];
    //string[] pname = new string[50];

    //private string recordpath1 = "";  // for TV card 1
    //private string recordpath2 = "";	// for TV card 2

    private int recorder; // Recorder name
    private string recorderdrive = ""; // Drive letter
    private ArrayList files = new ArrayList();

    private string tmpFolder;
    private string tmpStr;
    private string dvdBurnFolder = null;
    private ArrayList currentExt = null;
    private string currentFolder = null;
    private string[] drives = new string[50];
    private int driveCount = 0;
    private long totalSize = 0;
    private int totalTime = 0;
    private long cdMaxSize = 681574400;
    private long dvdMaxSize = 5046586572;
    private int cdMaxTime = 4440; // Seconds = 74 min
    private int dvdMaxTime = 7920; // Seconds = 2 hours 12 min. 

    private int perc = 0;
    private long max = 0;
    private bool fastFormat;

    private bool PalTvFormat = true; // Which format for the DVD
    private bool AspectRatio4x3 = true; // Which aspect ratio for the DVD
    private bool LeaveFilesForDebugging = true; // Leave temporary files to aid debugging
    private bool DummyBurn = false; // Tell the BurnDVD not to actually burn the DVD
    private bool DoNotEject = true; // Do not eject the disc after burning has finished

    private static ArrayList mp3_extensions = new ArrayList();
    private static ArrayList video_extensions = new ArrayList();
    private static ArrayList data_extensions = new ArrayList();

    public static int soundFileSize = 0;
    private static long lStartTime = 0;

    // Convert to short pathnames (madlldlib)
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.U4)]
    public static extern int
      GetShortPathName(
      [MarshalAs(UnmanagedType.LPTStr)] string inputFilePath,
      [MarshalAs(UnmanagedType.LPTStr)] StringBuilder outputFilePath,
      [MarshalAs(UnmanagedType.U4)] int bufferSize);

    #endregion

    #region Constructor

    public GUIBurner()
    {
      GetID = (int) Window.WINDOW_MY_BURNER;
    }

    #endregion

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        fastFormat = xmlreader.GetValueAsBool("burner", "fastformat", true);
        tmpFolder = xmlreader.GetValueAsString("burner", "temp_folder", Path.GetTempPath());
        dvdBurnFolder = xmlreader.GetValueAsString("burner", "dvdburnexe_folder", "c:\\");

        recorder = xmlreader.GetValueAsInt("burner", "recorder", 0);
        recorderdrive = xmlreader.GetValueAsString("burner", "recorderdrive", "");

        PalTvFormat = xmlreader.GetValueAsBool("burner", "PalTvFormat", true);
        AspectRatio4x3 = xmlreader.GetValueAsBool("burner", "AspectRatio4x3", true);
        LeaveFilesForDebugging = xmlreader.GetValueAsBool("burner", "leavedebugfiles", true);
        DummyBurn = xmlreader.GetValueAsBool("burner", "dummyburn", false);
        DoNotEject = xmlreader.GetValueAsBool("burner", "DoNotEject", true);
      }
      driveCount = 0;
      GetDrives();

      tmpFolder = Util.Utils.RemoveTrailingSlash(tmpFolder);

      try
      {
        CDBurner = new XPBurnCD();
        CDBurner.BurnerDrive = CDBurner.RecorderDrives[recorder].ToString();
      }
      catch (Exception ex)
      {
        Log.Error("Problem creating XPBurn");
        Log.Error(ex);
      }
    }

    #endregion

    #region Overrides

    public override bool Init()
    {
      mp3_extensions.Clear();
      mp3_extensions.Add(".mp3");

      data_extensions.Clear();
      data_extensions.Add("*");

      video_extensions.Clear();
      video_extensions.Add(".mpg");
      video_extensions.Add(".divx");
      video_extensions.Add(".avi");

      return Load(GUIGraphicsContext.Skin + @"\myburner.xml");
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
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          base.OnMessage(message);

          LoadSettings();

          GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(2100)); // My Burner
          GUIPropertyManager.SetProperty("#burner_title", GUILocalizeStrings.Get(2100)); // My Burner
          GUIPropertyManager.SetProperty("#burner_perc", "-5");
          GUIPropertyManager.SetProperty("#burner_size", " ");
          GUIPropertyManager.SetProperty("#burner_info", " ");
          GUIPropertyManager.SetProperty("#convert_info", " ");
          totalSize = 0;
          totalTime = 0;
          currentState = States.STATE_MAIN;
          UpdateButtons();

          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:

          #region GUI_MSG_CLICKED

          base.OnMessage(message);

          int iControl = message.SenderControlId;

          if (iControl == (int) Controls.CONTROL_BUTTON1)
          {
            #region Button 1

            switch (currentState)
            {
              case States.STATE_MAIN: // If Main change Folder to Video
                currentState = States.STATE_VIDEO;
                UpdateButtons();
                break;

              default:
                currentState = States.STATE_MAIN; // default goto main
                UpdateButtons();
                break;
            }
            return true;

            #endregion
          }
          if (iControl == (int) Controls.CONTROL_BUTTON2) // Button 2
          {
            #region Button 2

            switch (currentState)
            {
                // If on Main screen
              case States.STATE_MAIN:
                currentState = States.STATE_AUDIO;
                UpdateButtons();
                break;

                // If on Audio screen
              case States.STATE_AUDIO:
                currentState = States.STATE_MAKE_AUDIO_CD;
                ShowList();
                break;

                // If on Video screen
              case States.STATE_VIDEO:
                currentState = States.STATE_MAKE_VIDEO_DVD;
                ShowList();
                break;

                // If on Data screen
              case States.STATE_DATA:
                currentState = States.STATE_MAKE_DATA_CD;
                ShowList();
                break;

                // If on Audio CD Menu
              case States.STATE_MAKE_AUDIO_CD:
                burnType = BurnTypes.AUDIO_CD;
                BurnCD(burnType);
                break;

                // If on Data CD Menu
              case States.STATE_MAKE_DATA_CD:
                burnType = BurnTypes.DATA_CD;
                BurnCD(burnType);
                break;

                // If on Video DVD Menu
              case States.STATE_MAKE_VIDEO_DVD:
                burnType = BurnTypes.VIDEO_DVD;
                BurnDVD(burnType);
                break;

                // If on Data DVD Menu
              case States.STATE_MAKE_DATA_DVD:
                burnType = BurnTypes.DATA_DVD;
                BurnDVD(burnType);
                break;
            }
            return true;

            #endregion
          }

          if (iControl == (int) Controls.CONTROL_BUTTON3)
          {
            #region Button 3

            switch (currentState)
            {
              case States.STATE_MAIN:
                currentState = States.STATE_DATA;
                UpdateButtons();
                break;

              case States.STATE_DATA:
                currentState = States.STATE_MAKE_DATA_DVD;
                ShowList();
                break;

              case States.STATE_MAKE_VIDEO_DVD:
                ImportVideoPlaylist();
                break;

              case States.STATE_MAKE_AUDIO_CD:
                ImportAudioPlaylist();
                break;
            }
            return true;

            #endregion
          }

          if (iControl == (int) Controls.CONTROL_BUTTON4)
          {
            #region Button 4

            switch (currentState)
            {
              case States.STATE_MAIN:
                currentState = States.STATE_DISK_INFO; // Disk Info button
                ShowList();
                CdInfo();
                break;
            }
            return true;

            #endregion
          }

          if (iControl == (int) Controls.CONTROL_BUTTON5)
          {
            #region Button 5

            switch (currentState)
            {
              case States.STATE_MAIN:
                CdRwFormat();
                break;
            }
            return true;

            #endregion
          }

          if (iControl == (int) Controls.CONTROL_BUTTON6)
          {
            #region Button6

            switch (currentState)
            {
              case States.STATE_MAIN:
                CDBurner.Eject();
                break;
            }
            return true;

            #endregion
          }

          if (iControl == (int) Controls.CONTROL_LIST_COPY)
            // User click on one of the files in the BurnList. Which will remove that file
          {
            #region CONTROL_LIST_COPY

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
            OnMessage(msg);
            int iItem = (int) msg.Param1;
            int iAction = (int) message.Param1;
            files.Clear();

            if (iAction == (int) Action.ActionType.ACTION_SELECT_ITEM)
            {
              bool sel = true;
              GUIListItem item = GUIControl.GetSelectedListItem(GetID, (int) Controls.CONTROL_LIST_COPY);
              int count = GUIControl.GetItemCount(GetID, (int) Controls.CONTROL_LIST_COPY);
              for (int i = 0; i < count; i++)
              {
                GUIListItem cItem = GUIControl.GetListItem(GetID, (int) Controls.CONTROL_LIST_COPY, i);
                if (cItem.Label == item.Label)
                {
                  if (cItem.Path == item.Path)
                  {
                    sel = false;
                  }
                }
                if (sel)
                {
                  file fl = new file();
                  fl.name = cItem.Label;
                  fl.path = cItem.Path;
                  fl.size = cItem.FileInfo.Length;
                  files.Add(fl);
                }
                sel = true;
              }
              totalSize = 0;
              totalTime = 0;

              GUIControl.ClearControl(GetID, (int) Controls.CONTROL_LIST_COPY);
              foreach (file f in files)
              {
                GUIListItem pItem = new GUIListItem(f.name);
                FileInformation fi = new FileInformation();
                fi.Length = f.size;
                fi.Name = f.name;
                pItem.Path = f.path;
                pItem.FileInfo = (FileInformation) fi;
                GUIControl.AddListItemControl(GetID, (int) Controls.CONTROL_LIST_COPY, pItem);

                totalSize = totalSize + f.size;

                MusicTag tag = TagReader.TagReader.ReadTag(pItem.Path);
                pItem.MusicTag = tag;
                totalTime = totalTime + tag.Duration;
              }

              UpdatePercentageFullDisplay();
            }

            #endregion
          }
          if (iControl == (int) Controls.CONTROL_LIST_DIR) // User clicked on the Dir Browser window to locate a file
          {
            #region CONTROL_LIST_DIR

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
            OnMessage(msg);

            int iItem = (int) msg.Param1;
            int iAction = (int) message.Param1;
            if (iAction == (int) Action.ActionType.ACTION_SELECT_ITEM)
            {
              GUIListItem item = GUIControl.GetSelectedListItem(GetID, (int) Controls.CONTROL_LIST_DIR);
              if (item.Label.StartsWith("..")) // go back folder
              {
                #region Parent

                if (item.Path == "")
                {
                  LoadDriveListControl();
                }
                else
                {
                  LoadListControl(item.Path, currentExt);
                }

                #endregion
              }
                //else if (item.Label.StartsWith("["))		// is a share
                //{
                //  #region Share
                //  String shareName = item.Label.Substring(1);
                //  shareName = shareName.Substring(0, shareName.Length - 1);
                //  if (shareName == GUILocalizeStrings.Get(2133))
                //  {
                //    currentFolder = recordpath1;
                //    LoadListControl(currentFolder, currentExt);
                //  }
                //  if (shareName == GUILocalizeStrings.Get(2144)) // if two tv cards installed
                //  {
                //    currentFolder = recordpath1;
                //    LoadListControl(currentFolder, currentExt);
                //  }
                //  if (shareName == GUILocalizeStrings.Get(2145))
                //  {
                //    currentFolder = recordpath2;
                //    LoadListControl(currentFolder, currentExt);
                //  }
                //  else
                //  {
                //    for (int i = 0; i < 50; i++)
                //    {
                //      if (pname[i] == shareName)
                //      {
                //        currentFolder = pictures[i];
                //        LoadListControl(currentFolder, currentExt);
                //        break;
                //      }
                //      if (sname[i] == shareName)
                //      {
                //        currentFolder = sound[i];
                //        LoadListControl(currentFolder, currentExt);
                //        break;
                //      }
                //      if (vname[i] == shareName)
                //      {
                //        currentFolder = video[i];
                //        LoadListControl(currentFolder, currentExt);
                //        break;
                //      }
                //    }
                //  }
                //  LoadListControl(currentFolder, currentExt);
                //  #endregion
                //}
              else if (item.IsFolder) // is a folder
              {
                #region Folder

                LoadListControl(item.Path, currentExt);

                #endregion
              }
              else if (item.Label.Substring(1, 1) == ":") // is a drive
              {
                #region Drive

                currentFolder = item.Label;
                if (currentFolder != string.Empty)
                {
                  LoadListControl(currentFolder, currentExt);
                }
                else
                {
                  LoadDriveListControl();
                }

                #endregion
              }
              else
              {
                #region File

                int indx = currentFolder.IndexOf("\\\\");
                if (indx > 0)
                {
                  currentFolder = currentFolder.Remove(indx, 1);
                }

                GUIListItem pItem = new GUIListItem(item);


                // Work out how big the CD/DVD is so far...both in terms of file size (used for Data) and play length (user for Audio/Video)
                totalSize = totalSize + pItem.FileInfo.Length;

                MusicTag tag = TagReader.TagReader.ReadTag(pItem.Path);
                if (tag != null)
                {
                  pItem.MusicTag = tag;
                  totalTime = totalTime + tag.Duration;
                }

                if (SpaceOnMedia() == true) // Check if there is enough room on the CD/DVD (depending on currentState)
                {
                  GUIControl.AddListItemControl(GetID, (int) Controls.CONTROL_LIST_COPY, pItem);
                }
                else
                {
                  totalSize = totalSize - pItem.FileInfo.Length;
                  totalTime = totalTime - tag.Duration;
                }

                UpdatePercentageFullDisplay();

                #endregion
              }
            }
            return true;

            #endregion
          }
          return true;

          #endregion
      }
      return base.OnMessage(message);
    }

    #endregion

    #region Audio Functions

    private static bool ReportStatusMad(uint frameCount, uint byteCount, ref MadlldlibWrapper.mad_header mh, bool kill)
    {
      int perc = (int) (((float) byteCount/(float) soundFileSize)*100);
      long lDiff = (DateTime.Now.Ticks - lStartTime)/10000;
      if (lDiff > 500)
      {
        lStartTime = DateTime.Now.Ticks;
        GUIPropertyManager.SetProperty("#burner_perc", perc.ToString());
        GUIWindowManager.Process();
      }
      return true;
    }


    private bool ConvertMP3toWAV(string inputFile, string outputFile)
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
      {
        inputFile = inputFilePath.ToString();
      }

      if (outputFilePath.Length > 0)
      {
        outputFile = outputFilePath.ToString();
      }

      // Determine file size
      FileInfo wavFileInfo = null;
      FileInfo srcFileInfo = new FileInfo(inputFile);
      soundFileSize = (int) srcFileInfo.Length;

      // status/error message reporting. 
      // String length must be set 
      // explicitly

      StringBuilder status = new StringBuilder();
      status.Capacity = 256;

      // call the decoding function
      try
      {
        int st = MadlldlibWrapper.DecodeMP3(inputFile, outputFile, MadlldlibWrapper.DEC_WAV, status, defaultCallback);
        GC.KeepAlive(defaultCallback); // this prevents garbage collection
        wavFileInfo = new FileInfo(outputFile);
      }
      catch (Exception ex)
      {
        Log.Warn("Error converting MP3: {0}", ex.Message);
      }

      bool result = (File.Exists(outputFile) && wavFileInfo != null && (wavFileInfo.Length > srcFileInfo.Length));

      return result;
    }

    #endregion

    #region Private Methods

    private void UpdatePercentageFullDisplay()
    {
      switch (currentState)
      {
        case States.STATE_MAKE_AUDIO_CD:
        case States.STATE_MAKE_VIDEO_DVD:
          {
            if (totalTime > 0)
            {
              perc = Convert.ToInt16(totalTime/(max/100d));
            }
            else
            {
              perc = 0;
            }

            tmpStr = Util.Utils.SecondsToHMSString(totalTime) + "  of " + Util.Utils.SecondsToHMSString((int) max);
          }
          break;

        case States.STATE_MAKE_DATA_CD:
        case States.STATE_MAKE_DATA_DVD:
          {
            if (totalSize > 0)
            {
              perc = Convert.ToInt16(totalSize/(max/100d));
            }
            else
            {
              perc = 0;
            }
            tmpStr = Util.Utils.GetSize(totalSize) + " of " + Util.Utils.GetSize(max);
          }
          break;
      }

      GUIPropertyManager.SetProperty("#burner_size", tmpStr);
      GUIPropertyManager.SetProperty("#burner_perc", perc.ToString());
    }

    /// <summary>
    /// Check that there is enough room available on the CD/DVD
    /// </summary>
    /// <returns>True is room available - else false</returns>
    private bool SpaceOnMedia()
    {
      switch (currentState)
      {
        case States.STATE_MAKE_AUDIO_CD:
          if (totalTime > max)
          {
            GUIDialogNotify dlgNotify = (GUIDialogNotify) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_NOTIFY);
            if (null != dlgNotify)
            {
              dlgNotify.SetHeading(GUILocalizeStrings.Get(2100)); // Burner
              dlgNotify.SetText(GUILocalizeStrings.Get(2146)); // Not enough room on CD
              dlgNotify.DoModal(GetID);
            }
            return false;
          }
          break;

        case States.STATE_MAKE_VIDEO_DVD:
          if (totalTime > max)
          {
            GUIDialogNotify dlgNotify = (GUIDialogNotify) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_NOTIFY);
            if (null != dlgNotify)
            {
              dlgNotify.SetHeading(GUILocalizeStrings.Get(2100)); // Burner
              dlgNotify.SetText(GUILocalizeStrings.Get(2147)); // Not enough room on DVD
              dlgNotify.DoModal(GetID);
            }
            return false;
          }
          break;

        case States.STATE_MAKE_DATA_CD:
          if (totalTime > max)
          {
            GUIDialogNotify dlgNotify = (GUIDialogNotify) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_NOTIFY);
            if (null != dlgNotify)
            {
              dlgNotify.SetHeading(GUILocalizeStrings.Get(2100)); // Burner
              dlgNotify.SetText(GUILocalizeStrings.Get(2146)); // Not enough room on CD
              dlgNotify.DoModal(GetID);
            }
            return false;
          }
          break;

        case States.STATE_MAKE_DATA_DVD:
          if (totalSize > max)
          {
            GUIDialogNotify dlgNotify = (GUIDialogNotify) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_NOTIFY);
            if (null != dlgNotify)
            {
              dlgNotify.SetHeading(GUILocalizeStrings.Get(2100)); // Burner
              dlgNotify.SetText(GUILocalizeStrings.Get(2146)); // Not enough room on DVD
              dlgNotify.DoModal(GetID);
            }
            return false;
          }
          break;
      }
      return true;
    }

    private void LoadListControl(string folder, ArrayList Exts)
    {
      //clear the list
      folder = Util.Utils.RemoveTrailingSlash(folder);
      file f = new file();
      GUIControl.ClearControl(GetID, (int) Controls.CONTROL_LIST_DIR);
      VirtualDirectory Directory;
      List<GUIListItem> itemlist;
      Directory = new VirtualDirectory();
      Directory.SetExtensions(Exts);
      itemlist = Directory.GetDirectoryExt(folder);

      foreach (GUIListItem item in itemlist)
      {
        if (!item.IsFolder) // if item a folder
        {
          GUIListItem pItem = new GUIListItem(item.FileInfo.Name);
          pItem.FileInfo = item.FileInfo;
          pItem.IsFolder = false;
          pItem.Path = String.Format(@"{0}\{1}", folder, item.FileInfo.Name);
          GUIControl.AddListItemControl(GetID, (int) Controls.CONTROL_LIST_DIR, pItem);
          f.name = item.FileInfo.Name;
          f.size = item.FileInfo.Length;
          files.Add(f);
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
            if (pos >= 0)
            {
              prevFolder = folder.Substring(0, pos);
            }
            pItem.Path = prevFolder;
          }
          Util.Utils.SetDefaultIcons(pItem);
          GUIControl.AddListItemControl(GetID, (int) Controls.CONTROL_LIST_DIR, pItem);
        }
      }

      //set object count label
      int iTotalItems = GUIControl.GetItemCount(GetID, (int) Controls.CONTROL_LIST_DIR);
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(iTotalItems));

      currentFolder = folder;
    }

    private void LoadDriveListControl()
    {
      currentFolder = "";
      //clear the list
      GUIControl.ClearControl(GetID, (int) Controls.CONTROL_LIST_DIR);
      for (int i = 0; i < driveCount; i++)
      {
        GUIListItem pItem = new GUIListItem(drives[i]);
        pItem.Path = drives[i];
        pItem.IsFolder = true;
        Util.Utils.SetDefaultIcons(pItem);
        GUIControl.AddListItemControl(GetID, (int) Controls.CONTROL_LIST_DIR, pItem);
      }

      //set object count label
      int iTotalItems = GUIControl.GetItemCount(GetID, (int) Controls.CONTROL_LIST_DIR);
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(iTotalItems));
    }

    private void DisableButtons()
    {
      GUIControl.DisableControl(GetID, (int) Controls.CONTROL_BUTTON1);
      GUIControl.DisableControl(GetID, (int) Controls.CONTROL_BUTTON2);
      GUIControl.DisableControl(GetID, (int) Controls.CONTROL_BUTTON3);
      GUIControl.DisableControl(GetID, (int) Controls.CONTROL_BUTTON4);
      GUIControl.DisableControl(GetID, (int) Controls.CONTROL_BUTTON5);
      GUIControl.DisableControl(GetID, (int) Controls.CONTROL_BUTTON6);
    }

    private void ShowList()
    {
      GUIControl.ClearControl(GetID, (int) Controls.CONTROL_LIST_COPY);

      switch (currentState)
      {
          //        case States.STATE_DISK_INFO:
          //          UpdateButtons();
          //          GUIPropertyManager.SetProperty("#burner_title", GUILocalizeStrings.Get(2123));  //Disk info
          //          break;

        case States.STATE_MAKE_AUDIO_CD:
          UpdateButtons();
          GUIPropertyManager.SetProperty("#burner_title", GUILocalizeStrings.Get(2102)); //Create Audio-CD
          currentExt = mp3_extensions;
          LoadDriveListControl();
          currentFolder = "";
          max = cdMaxTime;
          totalTime = 0;
          break;

        case States.STATE_MAKE_VIDEO_DVD:
          UpdateButtons();
          GUIPropertyManager.SetProperty("#burner_title", GUILocalizeStrings.Get(2104)); //Create Video-DVD
          currentExt = video_extensions;
          LoadDriveListControl();
          currentFolder = "";
          max = dvdMaxTime;
          totalTime = 0;
          break;


        case States.STATE_MAKE_DATA_CD:
          UpdateButtons();
          GUIPropertyManager.SetProperty("#burner_title", GUILocalizeStrings.Get(2105)); //Create Data-CD
          currentExt = data_extensions;
          LoadDriveListControl();
          currentFolder = "";
          max = cdMaxSize;
          totalSize = 0;
          break;

        case States.STATE_MAKE_DATA_DVD:
          UpdateButtons();
          GUIPropertyManager.SetProperty("#burner_title", GUILocalizeStrings.Get(2106)); //Create Data-DVD
          currentExt = data_extensions;
          LoadDriveListControl();
          currentFolder = "";
          max = dvdMaxSize;
          totalSize = 0;
          break;
      }
    }

    private void UpdateButtons()
    {
      switch (currentState)
      {
        case States.STATE_MAIN: // Main Menu

          GUIPropertyManager.SetProperty("#burner_title", GUILocalizeStrings.Get(2143));

          GUIControl.ClearControl(GetID, (int) Controls.CONTROL_LIST_DIR);
          GUIControl.ClearControl(GetID, (int) Controls.CONTROL_LIST_COPY);

          AllButtonsDisabledAndHidden();

          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON1, GUILocalizeStrings.Get(2134)); //Video
          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON1);
          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON1);

          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON2, GUILocalizeStrings.Get(2135)); //Audio
          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON2);
          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON2);

          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON3, GUILocalizeStrings.Get(2136)); //Data
          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON3);
          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON3);

          //GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BUTTON4, GUILocalizeStrings.Get(2123)); //Disk info
          //GUIControl.ShowControl(GetID, (int)Controls.CONTROL_BUTTON4);
          //GUIControl.EnableControl(GetID, (int)Controls.CONTROL_BUTTON4);

          //GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BUTTON5, GUILocalizeStrings.Get(2114)); //Erase disc
          //GUIControl.ShowControl(GetID, (int)Controls.CONTROL_BUTTON5);
          //GUIControl.EnableControl(GetID, (int)Controls.CONTROL_BUTTON5);

          //GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BUTTON6, GUILocalizeStrings.Get(2126)); //Eject CD/DVD
          //GUIControl.ShowControl(GetID, (int)Controls.CONTROL_BUTTON6);
          //GUIControl.EnableControl(GetID, (int)Controls.CONTROL_BUTTON6);
          break;

        case States.STATE_VIDEO: // Video Menu
          AllButtonsDisabledAndHidden();

          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON1);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON1, GUILocalizeStrings.Get(712)); // Back
          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON1);

          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON2);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON2, GUILocalizeStrings.Get(2104));
            // Create Video-DVD
          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON2);

          break;

        case States.STATE_AUDIO: // Audio Menu
          AllButtonsDisabledAndHidden();

          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON1);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON1, GUILocalizeStrings.Get(712)); //Back
          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON1);

          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON2);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON2, GUILocalizeStrings.Get(2102));
            //Create Audio-CD
          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON2);

          break;


        case States.STATE_DATA: // Data Menu
          AllButtonsDisabledAndHidden();

          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON1);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON1, GUILocalizeStrings.Get(712)); //Back
          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON1);

          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON2);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON2, GUILocalizeStrings.Get(2105));
            //Create Data-CD
          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON2);

          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON3);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON3, GUILocalizeStrings.Get(2106));
            //Create Data-DVD
          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON3);

          break;


        case States.STATE_DISK_INFO:
          AllButtonsDisabledAndHidden();

          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON1);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON1, GUILocalizeStrings.Get(712)); //Back
          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON1);

          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_CD_DETAILS);
          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_CD_DETAILS);
          break;


        case States.STATE_MAKE_AUDIO_CD:
          AllButtonsDisabledAndHidden();

          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON1);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON1, GUILocalizeStrings.Get(712)); // Back
          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON1);

          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON2);
          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON2);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON2, GUILocalizeStrings.Get(2144));
            // Start Burning

          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON3);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON3, GUILocalizeStrings.Get(2145));
            // Import Current Playlist
          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON3);

          break;

        case States.STATE_MAKE_VIDEO_DVD:
          AllButtonsDisabledAndHidden();

          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON1);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON1, GUILocalizeStrings.Get(712)); // Back
          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON1);

          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON2);
          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON2);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON2, GUILocalizeStrings.Get(2144));
            // Start Burning

          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON3);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON3, GUILocalizeStrings.Get(2145));
            // Import Current Playlist
          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON3);

          break;


        case States.STATE_MAKE_DATA_CD:
          AllButtonsDisabledAndHidden();

          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON1);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON1, GUILocalizeStrings.Get(712)); // Back
          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON1);

          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON2);
          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON2);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON2, GUILocalizeStrings.Get(2144));
            // Start Burning

          break;


        case States.STATE_MAKE_DATA_DVD:
          AllButtonsDisabledAndHidden();

          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON1);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON1, GUILocalizeStrings.Get(712)); // Back
          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON1);

          GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BUTTON2);
          GUIControl.EnableControl(GetID, (int) Controls.CONTROL_BUTTON2);
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BUTTON2, GUILocalizeStrings.Get(2144));
            // Start Burning

          break;
      }
    }

    private void AllButtonsDisabledAndHidden()
    {
      GUIControl.HideControl(GetID, (int) Controls.CONTROL_BUTTON1);
      GUIControl.HideControl(GetID, (int) Controls.CONTROL_BUTTON2);
      GUIControl.HideControl(GetID, (int) Controls.CONTROL_BUTTON3);
      GUIControl.HideControl(GetID, (int) Controls.CONTROL_BUTTON4);
      GUIControl.HideControl(GetID, (int) Controls.CONTROL_BUTTON5);
      GUIControl.HideControl(GetID, (int) Controls.CONTROL_BUTTON6);
      GUIControl.DisableControl(GetID, (int) Controls.CONTROL_BUTTON1);
      GUIControl.DisableControl(GetID, (int) Controls.CONTROL_BUTTON2);
      GUIControl.DisableControl(GetID, (int) Controls.CONTROL_BUTTON3);
      GUIControl.DisableControl(GetID, (int) Controls.CONTROL_BUTTON4);
      GUIControl.DisableControl(GetID, (int) Controls.CONTROL_BUTTON5);
      GUIControl.DisableControl(GetID, (int) Controls.CONTROL_BUTTON6);
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
    /// fills the drive array. 3=HD 5=CD
    /// </summary>
    private void GetDrives()
    {
      foreach (string drive in Environment.GetLogicalDrives())
      {
        switch ((DriveType) Util.Utils.getDriveType(drive))
        {
          case DriveType.Removable:
          case DriveType.Fixed:
          case DriveType.RemoteDisk:
          case DriveType.RamDisk:
            drives[driveCount] = drive;
            driveCount++;
            break;
        }
      }
    }

    private void okDialog(string header, string text2)
    {
      GUIDialogOK dlgOk = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      dlgOk.SetHeading(header);
      dlgOk.SetLine(2, text2);
      dlgOk.DoModal(GetID);
    }

    #endregion

    #region DVD Burning

    #region Listen for BurnDVD Events

    private void DVDBurner_FileFinished(object sender, FileFinishedEventArgs e)
    {
      //string text = "Completed File: " + e.SourceFile;
      //GUIPropertyManager.SetProperty("#convert_info", text);
      //GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BACK, text);
    }

    private void DVDBurner_OutputReceived(object sender, DataReceivedEventArgs e)
    {
      string text = e.Data.ToString();
      GUIPropertyManager.SetProperty("#convert_info", text);
    }

    private void DVDBurner_AllFinished(object sender, EventArgs e)
    {
      //string text = "Completed";
      //GUIPropertyManager.SetProperty("#convert_info", text);
    }

    private void DVDBurner_BurnDVDStatusUpdate(object sender, BurnDVDStatusUpdateEventArgs e)
    {
      string text = e.Status;
      GUIPropertyManager.SetProperty("#convert_info", text);
    }

    #endregion

    /// <summary>
    /// Import the current Video playlist into MyBurner
    /// </summary>
    private void ImportVideoPlaylist()
    {
      PlayList playlist = PlayListPlayer.SingletonPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);

      int NumberOfFiles = playlist.Count;

      for (int i = 0; i < NumberOfFiles; i++)
      {
        string FileName = playlist[i].FileName;

        GUIListItem Item = new GUIListItem();
        Item.Path = Path.GetDirectoryName(FileName);
        Item.Label = Path.GetFileName(FileName);

        GUIControl.AddListItemControl(GetID, (int) Controls.CONTROL_LIST_COPY, Item);

        Log.Info("MyBurner Added Video File From Video Playlist: {0}", FileName);
      }
    }

    /// <summary>
    /// Import the current Audio playlist into MyBurner
    /// </summary>
    private void ImportAudioPlaylist()
    {
      PlayList playlist = PlayListPlayer.SingletonPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      int NumberOfFiles = playlist.Count;

      for (int i = 0; i < NumberOfFiles; i++)
      {
        string FileName = playlist[i].FileName;

        GUIListItem Item = new GUIListItem();
        Item.Path = Path.GetDirectoryName(FileName);
        Item.Label = Path.GetFileName(FileName);

        GUIControl.AddListItemControl(GetID, (int) Controls.CONTROL_LIST_COPY, Item);

        Log.Info("MyBurner Added Audio File From Audio Playlist: {0}", FileName);
      }
    }


    ///<summary>Functions to write to DVDs.</summary>
    ///<param name="bTyp">What kind of DVD to create. Of type BurnTypes</param>
    private void BurnDVD(BurnTypes bTyp)
    {
      ArrayList FilePathsToBurn = new ArrayList();

      AutoPlay.StopListening();

      GUIPropertyManager.SetProperty("#burner_size", "");
      GUIPropertyManager.SetProperty("#convert_info", "");

      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      if (null != dlgYesNo)
      {
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(2100)); // Burner
        //if (CDBurner.MediaInfo.isUsable)
        //  dlgYesNo.SetLine(1, string.Empty);
        //else
        dlgYesNo.SetLine(1, GUILocalizeStrings.Get(2108)); // Insert blank media
        dlgYesNo.SetLine(2, GUILocalizeStrings.Get(2109)); // then press OK
        dlgYesNo.DoModal(GetID);
        if (dlgYesNo.IsConfirmed)
        {
          int NumberOfFiles = GUIControl.GetItemCount(GetID, (int) Controls.CONTROL_LIST_COPY);
          for (int i = 0; i < NumberOfFiles; i++)
          {
            GUIListItem cItem = GUIControl.GetListItem(GetID, (int) Controls.CONTROL_LIST_COPY, i);
            try
            {
              FilePathsToBurn.Add(cItem.Path);
              Log.Info("BurnDVD Add File: {0}", cItem.Path + "\\" + cItem.Label);
            }
            catch (Exception ex)
            {
              Log.Info("MyBurner:BurnDVD(): ", ex.Message);
            }
          }


          Log.Info("BurnDVD BurnType: {0}", bTyp.ToString());

          string strTempFolder = tmpFolder + @"\DVD";

          if (bTyp == BurnTypes.VIDEO_DVD)
          {
            string strTvFormat;
            string strAspectRatio;

            if (PalTvFormat == true)
            {
              strTvFormat = "pal";
            }
            else
            {
              strTvFormat = "ntsc";
            }

            if (AspectRatio4x3 == true)
            {
              strAspectRatio = @"4/3";
            }
            else
            {
              strAspectRatio = @"16/9";
            }

            VideoDvdBurner = new BurnVideoDVD(FilePathsToBurn, strTempFolder, strTvFormat, strAspectRatio, dvdBurnFolder,
                                              LeaveFilesForDebugging, recorderdrive, DummyBurn);

            //Listen for some events
            VideoDvdBurner.FileFinished += new BurnVideoDVD.FileFinishedEventHandler(DVDBurner_FileFinished);
            //VideoDvdBurner.OutputReceived += new DataReceivedEventHandler(DVDBurner_OutputReceived);
            VideoDvdBurner.AllFinished += new EventHandler(DVDBurner_AllFinished);
            VideoDvdBurner.BurnDVDStatusUpdate +=
              new BurnVideoDVD.BurnDVDStatusUpdateEventHandler(DVDBurner_BurnDVDStatusUpdate);
            VideoDvdBurner.Start();
          }
          if (bTyp == BurnTypes.DATA_DVD)
          {
            DataDvdBurner = new BurnDataDVD(FilePathsToBurn, strTempFolder, dvdBurnFolder, LeaveFilesForDebugging,
                                            recorderdrive, DummyBurn);

            //Listen for some events
            DataDvdBurner.FileFinished += new BurnDataDVD.FileFinishedEventHandler(DVDBurner_FileFinished);
            //DataDvdBurner.OutputReceived += new DataReceivedEventHandler(DVDBurner_OutputReceived);
            DataDvdBurner.AllFinished += new EventHandler(DVDBurner_AllFinished);
            DataDvdBurner.BurnDVDStatusUpdate +=
              new BurnDataDVD.BurnDVDStatusUpdateEventHandler(DVDBurner_BurnDVDStatusUpdate);
            DataDvdBurner.Start();
          }
        }
      }
      currentState = States.STATE_MAIN;
      UpdateButtons();
      AutoPlay.StartListening();
    }

    #endregion

    #region CD Burning

    private void BurnCD(BurnTypes bTyp)
    {
      AutoPlay.StopListening();

      GUIPropertyManager.SetProperty("#burner_size", "");
      GUIPropertyManager.SetProperty("#convert_info", "");

      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      if (dlgYesNo != null)
      {
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(2100));

        if (CDBurner.MediaInfo.isWritable && CDBurner.MediaInfo.isUsable)
        {
          dlgYesNo.SetLine(1, string.Empty);
        }
        else
        {
          // Please insert blank CD/DVD
          dlgYesNo.SetLine(1, GUILocalizeStrings.Get(2108));
        }
        dlgYesNo.SetLine(2, GUILocalizeStrings.Get(2109));
        dlgYesNo.DoModal(GetID);
        if (dlgYesNo.IsConfirmed) // Burn the CD
        {
          int count = GUIControl.GetItemCount(GetID, (int) Controls.CONTROL_LIST_COPY);

          for (int i = 0; i < count; i++)
          {
            GUIListItem cItem = GUIControl.GetListItem(GetID, (int) Controls.CONTROL_LIST_COPY, i);

            bool FileCanBeUsed = File.Exists(cItem.Path);
            if (bTyp == BurnTypes.AUDIO_CD)
            {
              string strTempFolder = Util.Utils.RemoveTrailingSlash(tmpFolder);
              string outName = Path.ChangeExtension(cItem.FileInfo.Name, ".wav");
              GUIPropertyManager.SetProperty("#burner_size", "MP3->WAV  " + outName);
              //ConvertMP3toWAV(cItem.Path + "\\" + cItem.FileInfo.Name, tmpFolder + "\\" + outName);
              if (ConvertMP3toWAV(cItem.Path, strTempFolder + "\\" + outName))
              {
                cItem.Label = outName;
                cItem.FileInfo.Name = outName;

                FileInfo fi = new FileInfo(strTempFolder + "\\" + outName);
                cItem.FileInfo.Length = (int) fi.Length;

                cItem.Path = fi.FullName;
              }
              else
              {
                FileCanBeUsed = false;
              }
            }

            // Otherwise we are a data CD and dont need to do any conversions

            try
            {
              //CDBurner.AddFile(cItem.Path + "\\" + cItem.Label, cItem.Path + "\\" + cItem.Label);
              if (FileCanBeUsed)
              {
                CDBurner.AddFile(cItem.Path, cItem.Label);
                Log.Info("MyBurner: Added File: {0}", cItem.Path);
              }
            }
            catch (Exception ex)
            {
              Log.Error("MyBurner: {0}", ex.Message);
            }
          }

          if (bTyp == BurnTypes.AUDIO_CD)
          {
            CDBurner.ActiveFormat = RecordType.afMusic;
            Log.Info("MyBurner: Burn type - Audio");
          }
          else
          {
            CDBurner.ActiveFormat = RecordType.afData;
            Log.Info("MyBurner: Burn type - Data");
          }

          if (CDBurner.MediaInfo.isWritable == false || CDBurner.MediaInfo.isUsable == false)
            // MultiSession??? || CDBurner.MediaInfo.isBlank == false)
          {
            okDialog(GUILocalizeStrings.Get(2100), GUILocalizeStrings.Get(2127)); // The CD is not writable

            GUIPropertyManager.SetProperty("#burner_size", "");
            DeleteTemporaryWavFiles();
          }
          else
          {
            CDBurner.PreparingBurn += new NotifyEstimatedTime(CDBurner_PreparingBurn);
            CDBurner.AddProgress += new NotifyCDProgress(CDBurner_AddProgress);
            CDBurner.BlockProgress += new NotifyCDProgress(CDBurner_BlockProgress);
            CDBurner.ClosingDisc += new NotifyEstimatedTime(CDBurner_ClosingDisc);
            CDBurner.BurnComplete += new NotifyCompletionStatus(CDBurner_BurnComplete);

            try
            {
              CDBurner.RecordDisc(DummyBurn, !DoNotEject);
            }
            catch (Exception ex)
            {
              Log.Error("MyBurner: ", ex.Message);
            }
          }

          GUIControl.ClearControl(GetID, (int) Controls.CONTROL_LIST_DIR);
          GUIControl.ClearControl(GetID, (int) Controls.CONTROL_LIST_COPY);
        }
      }
      currentState = States.STATE_MAIN;
      UpdateButtons();
      AutoPlay.StartListening();
    }

    private void DeleteTemporaryWavFiles()
    {
      int count = GUIControl.GetItemCount(GetID, (int) Controls.CONTROL_LIST_COPY);

      for (int i = 0; i < count; i++)
      {
        GUIListItem cItem = GUIControl.GetListItem(GetID, (int) Controls.CONTROL_LIST_COPY, i);

        try
        {
          string WavFilename = tmpFolder + "\\" + cItem.FileInfo.Name;
          File.Delete(WavFilename);
          Log.Info("Delete WAV: {0}", WavFilename);
        }
        catch (Exception ex)
        {
          Log.Info("MyBurner: ", ex.Message);
        }
      }
    }

    private void CdRwFormat()
    {
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      if (dlgYesNo != null)
      {
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(2100)); // Burner
        dlgYesNo.SetLine(1, GUILocalizeStrings.Get(2115)); // Insert CD RW
        dlgYesNo.SetLine(2, GUILocalizeStrings.Get(2109)); // then press OK
        dlgYesNo.DoModal(GetID);
        if (dlgYesNo.IsConfirmed) // format CD
        {
          if (CDBurner.MediaInfo.isUsable == false)
          {
            okDialog(GUILocalizeStrings.Get(2100), GUILocalizeStrings.Get(2124)); //Cannot Erase: Media is not writable
          }
          else
          {
            DisableButtons();
            EraseKind eraseType = new EraseKind();
            if (fastFormat == true)
            {
              eraseType = EraseKind.ekQuick;
            }
            else
            {
              eraseType = EraseKind.ekFull;
            }

            GUIPropertyManager.SetProperty("#convert_info", GUILocalizeStrings.Get(2125)); //Erasing Disk....
            try
            {
              CDBurner.Erase(eraseType);
            }
            catch (Exception ex)
            {
              Log.Info("MyBurner:Unable format CD/RW", ex.Message);
            }

            CDBurner.EraseComplete += new NotifyCompletionStatus(EraseFinished);
          }
        }
      }
    }

    private void CdInfo()
    {
      string info = GUILocalizeStrings.Get(2123);
      currentState = States.STATE_DISK_INFO;
      UpdateButtons();
      try
      {
        info = "\nDisc Space : " + CDBurner.DiscSpace.ToString() + "\n";
        info = info + "Free Disc Space : " + CDBurner.FreeDiscSpace.ToString() + "\n";
        if (CDBurner.IsBurning == false && CDBurner.IsErasing == false)
        {
          info = info + "Media Is Usable : " + CDBurner.MediaInfo.isUsable.ToString() + "\n";
          info = info + "Media Is Blank : " + CDBurner.MediaInfo.isBlank.ToString() + "\n";
          info = info + "Media Is ReadWrite : " + CDBurner.MediaInfo.isReadWrite.ToString() + "\n";
          info = info + "Media Is Writable : " + CDBurner.MediaInfo.isWritable.ToString() + "\n";
        }
        info = info + "Product ID : " + CDBurner.ProductID.ToString() + "\n";
        if (CDBurner.RecorderType == RecorderType.rtCDR)
        {
          info = info + "Recorder Type : CDR\n";
        }
        if (CDBurner.RecorderType == RecorderType.rtCDRW)
        {
          info = info + "Recorder Type : CDRW\n";
        }
        info = info + "Max Write Speed : " + CDBurner.MaxWriteSpeed.ToString() + "\n";
        info = info + "Revision : " + CDBurner.Revision + "\n";
        info = info + "Vendor : " + CDBurner.Vendor + "\n";
        info = info + "Volume Name : " + CDBurner.VolumeName + "\n";
        info = info + "Write Speed : " + CDBurner.WriteSpeed.ToString() + "\n";
      }
      catch (Exception ex)
      {
        Log.Info("MyBurner:Error CD Info", ex.Message);
      }
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_CD_DETAILS, info);
    }

    private void EraseFinished(UInt32 status)
    {
      GUIPropertyManager.SetProperty("#convert_info", GUILocalizeStrings.Get(2111));
      UpdateButtons();
    }

    private void CDBurner_PreparingBurn(int nEstimatedSeconds)
    {
      GUIPropertyManager.SetProperty("#convert_info", GUILocalizeStrings.Get(2128) + " " + nEstimatedSeconds.ToString());
    }

    private void CDBurner_AddProgress(int nCompletedSteps, int nTotalSteps)
    {
      GUIPropertyManager.SetProperty("#convert_info", GUILocalizeStrings.Get(2129));
      if (nCompletedSteps > 0)
      {
        perc = Convert.ToInt16(nCompletedSteps/(nTotalSteps/100d));
      }
      else
      {
        perc = 0;
      }
      GUIPropertyManager.SetProperty("#burner_perc", perc.ToString());
    }

    private void CDBurner_BlockProgress(int nCompletedSteps, int nTotalSteps)
    {
      GUIPropertyManager.SetProperty("#convert_info",
                                     GUILocalizeStrings.Get(2130) + " " + nCompletedSteps.ToString() + " " +
                                     GUILocalizeStrings.Get(2131) + " " + nTotalSteps.ToString());
      if (nCompletedSteps > 0)
      {
        perc = Convert.ToInt16(nCompletedSteps/(nTotalSteps/100d));
      }
      else
      {
        perc = 0;
      }
      GUIPropertyManager.SetProperty("#burner_perc", perc.ToString());
    }

    private void CDBurner_ClosingDisc(int nEstimatedSeconds)
    {
      GUIPropertyManager.SetProperty("#convert_info",
                                     string.Format(GUILocalizeStrings.Get(2132), nEstimatedSeconds.ToString()));
    }

    private void CDBurner_BurnComplete(uint status)
    {
      GUIPropertyManager.SetProperty("#convert_info", GUILocalizeStrings.Get(2111)); //Finished !
      //XPBurn.XPBurnCD CDBurner = new XPBurn.XPBurnCD();

      DeleteTemporaryWavFiles();
    }

    #endregion

    #region ISetupForm Members

    public int GetWindowId()
    {
      return GetID;
    }

    public string PluginName()
    {
      return "Burner";
    }

    public string Description()
    {
      return @"Burn CD and DVD with MediaPortal";
    }

    public string Author()
    {
      return "Gucky62, EgonSpenglerUk";
    }

    public void ShowPlugin()
    {
      Form BurnerSetup = new BurnerSetupForm();
      BurnerSetup.ShowDialog();
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool HasSetup()
    {
      return true;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(2100);
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = "hover_my burner.png";
      return true;
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion
  }
}