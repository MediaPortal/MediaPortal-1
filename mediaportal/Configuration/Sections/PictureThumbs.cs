#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using CSScriptLibrary;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.Threading;
using MediaPortal.Util;
using MediaPortal.Picture.Database;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class PictureThumbs : SectionSettings
  {
    private bool settingsLoaded = false;
    private static bool noLargeThumbnails = true;
    private Thread _scanThread = null;
    private static int countfiles = 1;
    private static int totalFiles = 0;

    private string[] Extensions
    {
      get { return extensions; }
      set { extensions = value; }
    }

    private string[] extensions = new string[] {".jpg"};

    #region ctor

    public PictureThumbs()
      : this("Picture Database")
    {
    }

    public PictureThumbs(string name)
      : base("Picture Database")
    {
      InitializeComponent();
    }

    #endregion

    #region UI

    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    public override void OnSectionActivated()
    {
      //
      // Clear any existing entries
      //
      sharesListBox.Items.Clear();

      //
      // Load picture shares
      //
      SectionSettings section = GetSection("Picture Folders");

      if (section != null)
      {
        ArrayList shares = (ArrayList) section.GetSetting("shares");

        foreach (string share in shares)
        {
          //
          // Add to share to list box and default to selected
          //
          sharesListBox.Items.Add(share, CheckState.Checked);
        }
      }

      UpdateControlStatus();
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateControlStatus()
    {
      startButton.Enabled = sharesListBox.CheckedItems.Count > 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void startButton_Click(object sender, EventArgs e)
    {
      groupBox1.Enabled = false;
      groupBox2.Enabled = true;
      fileLabel.Enabled = true;
      RebuildDatabase();
      groupBox1.Enabled = true;
      groupBox2.Enabled = false;
      fileLabel.Enabled = false;
    }

    private void clearButton_Click(object sender, EventArgs e)
    {
      DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the entire picture database?",
                                                  "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

      if (dialogResult == DialogResult.Yes)
      {
        if (ClearDatabase())
        {
          MessageBox.Show("Picture database has been cleared", "Picture Database", MessageBoxButtons.OK,
                          MessageBoxIcon.Exclamation);
        }
        else
        {
          MessageBox.Show("Picture database could not be cleared", "Picture Database", MessageBoxButtons.OK,
                          MessageBoxIcon.Exclamation);
        }
      }
    }

    #endregion

    #region Functions

    private void OnScanDatabase()
    {
      //reset Value
      countfiles = 0;

      ThreadStart ts = new ThreadStart(OnScanDatabaseThread);
      _scanThread = new Thread(ts);
      _scanThread.Name = "PicturesScan";
      _scanThread.Priority = ThreadPriority.BelowNormal;
      _scanThread.Start();
    }

    private void OnScanDatabaseThread()
    {
      try
      {
        ArrayList paths = new ArrayList();

        SetStatus("Starting picture thumbnail generation and database additions");

        ArrayList scanShares = new ArrayList();

        for (int index = 0; index < sharesListBox.CheckedIndices.Count; index++)
        {
          string fullPath = sharesListBox.Items[(int) sharesListBox.CheckedIndices[index]].ToString();

          if (Directory.Exists(fullPath))
          {
            paths.Add(fullPath);
          }
        }

        Stopwatch benchclockfile = new Stopwatch();
        benchclockfile.Start();

        // get all pictures from the path
        ArrayList availableFiles = new ArrayList();
        foreach (string path in paths)
        {
          CountFiles(path, ref availableFiles);
        }

        if (!noLargeThumbnails)
        {
          // Display double count (for generate small and large thumbnail)
          totalFiles = availableFiles.Count*2;
        }
        else
        {
          totalFiles = availableFiles.Count;
        }

        Log.Info("PictureDatabase: Beginning picture database reorganization and thumbnail generation...");

        // treat each picture file one by one
        foreach (string file in availableFiles)
        {
          Log.Info("Scanning file: {0}", file);
          // create thumb if not created and add file to db if not already there
          CreateThumbsAndAddPictureToDB(file);
        }
        benchclockfile.Stop();
        if (noLargeThumbnails)
        {
          Log.Debug("Pictures Configuration : Creation of selected thumb for '{0}' files, took {1} seconds",
                    availableFiles.Count,
                    benchclockfile.Elapsed.TotalSeconds);
        }
        else
        {
          Log.Debug("Pictures Configuration : Creation of selected thumb for '{0}' files, took {1} seconds",
                    availableFiles.Count * 2,
                    benchclockfile.Elapsed.TotalSeconds);
        }

        Log.Info("PictureDatabase: Database reorganization and thumbnail generation finished");

        SetStatus(String.Format("Finished. {0} files processsed", totalFiles));

      }
      catch (Exception)
      {
      }
    }

    private void CreateThumbsAndAddPictureToDB(string file)
    {
      int iRotate = PictureDatabase.GetRotation(file);
      if (iRotate == -1)
      {
        Log.Debug("PictureDatabase: Database is not available. File {0} has not been added", file);
      }

      string thumbnailImage = Util.Utils.GetPicturesThumbPathname(file);
      string thumbnailImageL = Util.Utils.GetPicturesLargeThumbPathname(file);

      if (!noLargeThumbnails && !File.Exists(thumbnailImageL))
      {
        if (Util.Picture.CreateThumbnail(file, thumbnailImageL, (int)Thumbs.ThumbLargeResolution,
                                         (int)Thumbs.ThumbLargeResolution, iRotate, Thumbs.SpeedThumbsLarge,
                                         true, false))
        {
          Log.Debug("PictureDatabase: Creation of missing large thumb successful for {0}", file);
          countfiles++;
          SetStatus(String.Format("{0}/{1} thumbnails generated", countfiles, totalFiles));
        }
      }
      if (!File.Exists(thumbnailImage))
      {
        if (Util.Picture.CreateThumbnail(file, thumbnailImage, (int)Thumbs.ThumbResolution,
                                         (int)Thumbs.ThumbResolution, iRotate, Thumbs.SpeedThumbsSmall,
                                         false, false))
        {
          Log.Debug("PictureDatabase: Creation of missing thumb successful for {0}", file);
          countfiles++;
          SetStatus(String.Format("{0}/{1} thumbnails generated", countfiles, totalFiles));
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private bool ClearDatabase()
    {
      string database = Config.GetFile(Config.Dir.Database, "PictureDatabase.db3");
      if (File.Exists(database))
      {
        PictureDatabase.Dispose();
        try
        {
          File.Delete(database);
        }
        catch (Exception)
        {
          return false;
        }
        finally
        {
          PictureDatabase.ReOpen();
        }
      }
      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="status"></param>
    private void SetStatus(string status)
    {
      fileLabel.Text = status;
      Application.DoEvents();
    }

    public class CreateThumbsAndAddPictureToDBFolderThread
    {
      private VirtualDirectory vDir = new VirtualDirectory();

      private string _filepath = string.Empty;
      private Work work;

      public CreateThumbsAndAddPictureToDBFolderThread(string Filepath)
      {
        _filepath = Filepath;

        work = new Work(new DoWorkHandler(this.PerformRequest));
        work.ThreadPriority = ThreadPriority.BelowNormal;
        GlobalServiceProvider.Get<IThreadPool>().Add(work, QueuePriority.Low);
      }

      /// <summary>
      /// creates cached thumbs in MP's thumbs dir
      /// </summary>
      private void PerformRequest()
      {
        Stopwatch benchclock = new Stopwatch();
        benchclock.Start();
        string path = _filepath;
        List<GUIListItem> itemlist = null;

        vDir.SetExtensions(Util.Utils.PictureExtensions);

        if (!vDir.IsRemote(path))
        {
          itemlist = vDir.GetDirectoryUnProtectedExt(path, true);

          foreach (GUIListItem item in itemlist)
          {
            if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
            {
              return;
            }
            if (String.IsNullOrEmpty(item.Path))
            {
              continue;
            }
            if (path.Length >= item.Path.Length)
            {
              Log.Warn("GUIPictures: Omitting outside path {0} from check share {1}", item.Path, path);
              continue;
            }
            if (!item.IsFolder)
            {
              int iRotate = PictureDatabase.GetRotation(item.Path);
              if (iRotate == -1)
              {
                Log.Debug("PictureDatabase: Database is not available. File {0} has not been added", item.Path);
              }

              string thumbnailImage = Util.Utils.GetPicturesThumbPathname(item.Path);
              string thumbnailImageL = Util.Utils.GetPicturesLargeThumbPathname(item.Path);

              if (!noLargeThumbnails && !File.Exists(thumbnailImageL))
              {
                if (Util.Picture.CreateThumbnail(item.Path, thumbnailImageL, (int)Thumbs.ThumbLargeResolution,
                                                 (int)Thumbs.ThumbLargeResolution, iRotate, Thumbs.SpeedThumbsLarge,
                                                 true, false))
                {
                  Log.Debug("PictureDatabase: Creation of missing large thumb successful for {0}", item.Path);
                  countfiles++;
                }
              }
              if (!File.Exists(thumbnailImage))
              {
                if (Util.Picture.CreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbResolution,
                                                 (int)Thumbs.ThumbResolution, iRotate, Thumbs.SpeedThumbsSmall,
                                                 false, false))
                {
                  Log.Debug("PictureDatabase: Creation of missing thumb successful for {0}", item.Path);
                  countfiles++;
                }
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private void RebuildDatabase()
    {
      OnScanDatabase();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="totalFiles"></param>
    private static void CountFiles(string path, ref ArrayList availableFiles)
    {
      //
      // Count the files in the current directory
      //
      try
      {
        VirtualDirectory dir = new VirtualDirectory();
        dir.SetExtensions(Util.Utils.PictureExtensions);
        List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(path, true);
        foreach (GUIListItem item in items)
        {
          if (item.IsFolder)
          {
            if (item.Label != "..")
            {
              CountFiles(item.Path, ref availableFiles);
              CreateThumbsAndAddPictureToDBFolderThread ManualThumbBuilderFolder =
                new CreateThumbsAndAddPictureToDBFolderThread(item.Path);
            }
          }
          else
          {
            availableFiles.Add(item.Path);
          }
        }
      }
      catch (Exception e)
      {
        Log.Info("Exception counting files:{0}", e);
        // Ignore
      }
    }

    #endregion

    #region Persistance

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        // skipCheckBox.Checked = xmlreader.GetValueAsBool("picturedatabase", "scanskipexisting", false);
        noLargeThumbnails = xmlreader.GetValueAsBool("thumbnails", "picturenolargethumbondemand", true);

        // int iNumber = xmlreader.GetValueAsInt("picturedatabase", "number", 0);
        // if (iNumber > 0)
        // {
        //   string strLimit = "";
        //   string strDatabase = "";
        //   string strLanguage = "";
        //   string strTitle = "";
        //   for (int i = 0; i < iNumber; i++)
        //   {
        //     strLimit = xmlreader.GetValueAsString("moviedatabase", "limit" + i.ToString(), "false");
        //     strDatabase = xmlreader.GetValueAsString("moviedatabase", "database" + i.ToString(), "false");
        //     strLanguage = xmlreader.GetValueAsString("moviedatabase", "language" + i.ToString(), "false");
        //     strTitle = xmlreader.GetValueAsString("moviedatabase", "title" + i.ToString(), "false");

        //     if ((strLimit != "false") && (strDatabase != "false") && (strLanguage != "false") && (strTitle != "false"))
        //     {
        //       //ListViewItem item = this.lvDatabase.Items.Add(strDatabase);
        //       //item.SubItems.Add(strTitle);
        //       //item.SubItems.Add(strLanguage);
        //       //item.SubItems.Add(strLimit);
        //     }
        //   }
        // }

        //// ReloadGrabberScripts();
      }

      settingsLoaded = true;
    }

    public override void SaveSettings()
    {
      if (!settingsLoaded)
      {
        return;
      }

      using (Settings xmlwriter = new MPSettings())
      {
        // Database
        //xmlwriter.SetValueAsBool("picturedatabase", "scanskipexisting", skipCheckBox.Checked);
      }
    }

    #endregion
  }
}