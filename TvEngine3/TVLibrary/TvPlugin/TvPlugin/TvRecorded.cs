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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
//using System.Windows;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;

using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Services;
using MediaPortal.Threading;
using MediaPortal.Util;
using MediaPortal.Configuration;
///@using MediaPortal.Video.Database;
using Toub.MediaCenter.Dvrms.Metadata;
///@using MediaPortal.TV.DiskSpace;

using TvDatabase;
using TvControl;

using Gentle.Common;
using Gentle.Framework;

using TvLibrary.Interfaces;
using TvLibrary.Implementations.DVB;

namespace TvPlugin
{
  #region ThumbCacher
  public class RecordingThumbCacher
  {
    Work work;

    public RecordingThumbCacher()
    {
      work = new Work(new DoWorkHandler(this.PerformRequest));
      work.ThreadPriority = System.Threading.ThreadPriority.Lowest;
      MediaPortal.Services.GlobalServiceProvider.Get<IThreadPool>().Add(work, QueuePriority.Low);
    }

    void PerformRequest()
    {
      //if (_creatingThumbNails)
      //  return;
      //try
      //{
      //  _creatingThumbNails = true;

      //  IList recordings = Recording.ListAll();
      //  foreach (Recording rec in recordings)
      //  {
      //    string thumbNail = Utils.GetCoverArtName(Thumbs.TVRecorded, Utils.SplitFilename(Path.ChangeExtension(rec.FileName, @".png")));
      //    if (!File.Exists(thumbNail))
      //    {
      //      Log.Info("RecordedTV: No thumbnail found at {0} for recording {1} - grabbing from file now", thumbNail, rec.FileName);
      //      if (!DvrMsImageGrabber.GrabFrame(rec.FileName, thumbNail))
      //        Log.Info("GUIRecordedTV: No thumbnail created for {0}", Utils.SplitFilename(rec.FileName));
      //      try
      //      {
      //        MediaPlayer player = new MediaPlayer();
      //        player.Open(new Uri(rec.FileName, UriKind.Absolute));
      //        player.ScrubbingEnabled = true;
      //        player.Play();
      //        player.Pause();
      //        // Grab the frame 10 minutes after start to respect pre-recording times.
      //        player.Position = new TimeSpan(0, 10, 0);
      //        System.Threading.Thread.Sleep(5000);
      //        RenderTargetBitmap rtb = new RenderTargetBitmap(720, 576, 1 / 200, 1 / 200, PixelFormats.Pbgra32);
      //        DrawingVisual dv = new DrawingVisual();
      //        DrawingContext dc = dv.RenderOpen();
      //        dc.DrawVideo(player, new Rect(0, 0, 720, 576));
      //        dc.Close();
      //        rtb.Render(dv);
      //        PngBitmapEncoder encoder = new PngBitmapEncoder();
      //        encoder.Frames.Add(BitmapFrame.Create(rtb));
      //        using (FileStream stream = new FileStream(thumbNail, FileMode.OpenOrCreate))
      //        {
      //          encoder.Save(stream);
      //        }
      //        player.Stop();
      //        player.Close();
      //      }
      //      catch (Exception ex)
      //      {
      //        Log.Info("RecordedTV: No thumbnail created for {0}", Utils.SplitFilename(rec.FileName));
      //      }
      //    }
      //  }
      //}
      //finally
      //{
      //  _creatingThumbNails = false;
      //}
    }
  }
  #endregion

  public class TvRecorded : GUIWindow, IComparer<GUIListItem>
  {
    #region variables

    enum Controls
    {
      LABEL_PROGRAMTITLE = 13,
      LABEL_PROGRAMTIME = 14,
      LABEL_PROGRAMDESCRIPTION = 15,
      LABEL_PROGRAMGENRE = 17,
    };

    enum SortMethod
    {
      Channel = 0,
      Date = 1,
      Name = 2,
      Genre = 3,
      Played = 4,
      Duration = 5
    }
    enum ViewAs
    {
      List,
      Album,
      BigIcon,
    }

    ViewAs currentViewMethod = ViewAs.Album;
    SortMethod currentSortMethod = SortMethod.Date;
    private static Recording m_oActiveRecording = null;
    private static bool m_bIsLiveRecording = false;
    bool m_bSortAscending = true;
    bool _deleteWatchedShows = false;
    bool _createRecordedThumbs = true;
    int m_iSelectedItem = 0;
    string currentShow = string.Empty;
    //bool _creatingThumbNails = false;
    RecordingThumbCacher thumbworker = null;

    [SkinControlAttribute(2)]
    protected GUIButtonControl btnViewAs = null;
    [SkinControlAttribute(3)]
    protected GUISortButtonControl btnSortBy = null;
    [SkinControlAttribute(5)]
    protected GUIButtonControl btnView = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl btnCleanup = null;
    [SkinControlAttribute(10)]
    protected GUIListControl listAlbums = null;
    [SkinControlAttribute(11)]
    protected GUIListControl listViews = null;

    #endregion

    public TvRecorded()
    {
      GetID = (int)GUIWindow.Window.WINDOW_RECORDEDTV;
    }

    public override void OnAdded()
    {
      // replace g_player's ShowFullScreenWindowTV
      g_Player.ShowFullScreenWindowTV = ShowFullScreenWindowTVHandler;
      g_Player.ShowFullScreenWindowVideo = ShowFullScreenWindowVideoHandler; // singleseaters uses this

      Log.Info("TvRecorded:OnAdded");
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_RECORDEDTV, this);
      Restore();
      PreInit();
      ResetAllControls();
    }

    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    public static Recording ActiveRecording()
    {
      return m_oActiveRecording;
    }

    public static void SetActiveRecording(Recording rec)
    {
      m_oActiveRecording = rec;
      m_bIsLiveRecording = IsRecordingActual(rec);
    }    

    public static bool IsLiveRecording()
    {
      return m_bIsLiveRecording;
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string strTmp = xmlreader.GetValueAsString("tvrecorded", "sort", "channel");

        if (strTmp == "channel") currentSortMethod = SortMethod.Channel;
        else if (strTmp == "date") currentSortMethod = SortMethod.Date;
        else if (strTmp == "name") currentSortMethod = SortMethod.Name;
        else if (strTmp == "type") currentSortMethod = SortMethod.Genre;
        else if (strTmp == "played") currentSortMethod = SortMethod.Played;
        else if (strTmp == "duration") currentSortMethod = SortMethod.Duration;

        strTmp = xmlreader.GetValueAsString("tvrecorded", "view", "list");

        if (strTmp == "album")
        {
          currentViewMethod = ViewAs.Album;
          if (listViews.Focus) listAlbums.Focus = true;
        }
        else
        {
          currentViewMethod = ViewAs.List;
          if (listAlbums.Focus) listViews.Focus = true;
        }
        //          else if (strTmp == "bigicon")
        //            currentViewMethod = ViewAs.BigIcon;

        m_bSortAscending = xmlreader.GetValueAsBool("tvrecorded", "sortascending", true);
        _deleteWatchedShows = xmlreader.GetValueAsBool("capture", "deletewatchedshows", false);
        _createRecordedThumbs = xmlreader.GetValueAsBool("thumbnails", "tvrecordedondemand", true);
      }
      thumbworker = null;
    }

    private void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            xmlwriter.SetValue("tvrecorded", "sort", "channel");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue("tvrecorded", "sort", "date");
            break;
          case SortMethod.Name:
            xmlwriter.SetValue("tvrecorded", "sort", "name");
            break;
          case SortMethod.Genre:
            xmlwriter.SetValue("tvrecorded", "sort", "type");
            break;
          case SortMethod.Played:
            xmlwriter.SetValue("tvrecorded", "sort", "played");
            break;
          case SortMethod.Duration:
            xmlwriter.SetValue("tvrecorded", "sort", "duration");
            break;
        }
        switch (currentViewMethod)
        {
          case ViewAs.Album:
            xmlwriter.SetValue("tvrecorded", "view", "album");
            break;
          case ViewAs.List:
            xmlwriter.SetValue("tvrecorded", "view", "list");
            break;
          //          case ViewAs.BigIcon:
          //            xmlwriter.SetValue("tvrecorded", "view", "bigicon");
          //            break;
        }
        xmlwriter.SetValueAsBool("tvrecorded", "sortascending", m_bSortAscending);
      }
    }

    #endregion

    /// <summary>
    /// This function replaces g_player.ShowFullScreenWindowTV
    /// </summary>
    ///<returns></returns>        
    private static bool ShowFullScreenWindowTVHandler()
    {
      if (g_Player.IsTVRecording)
      {
        // watching TV
        if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          return true;
        Log.Info("TVRecorded: ShowFullScreenWindow switching to fullscreen tv");
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
        GUIGraphicsContext.IsFullScreenVideo = true;
        return true;
      }
      return g_Player.ShowFullScreenWindowTVDefault();
    }

    /// <summary>
    /// This function replaces g_player.ShowFullScreenWindowVideo
    /// </summary>
    ///<returns></returns>        
    private static bool ShowFullScreenWindowVideoHandler()
    {
      if (g_Player.IsTVRecording)
      {
        // watching TV
        if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          return true;
        Log.Info("TVRecorded: ShowFullScreenWindow switching to fullscreen video");
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
        GUIGraphicsContext.IsFullScreenVideo = true;
        return true;
      }
      return g_Player.ShowFullScreenWindowVideoDefault();
    }

    #region overrides

    public override bool Init()
    {
      g_Player.PlayBackStopped += new MediaPortal.Player.g_Player.StoppedHandler(OnPlayRecordingBackStopped);
      g_Player.PlayBackEnded += new MediaPortal.Player.g_Player.EndedHandler(OnPlayRecordingBackEnded);
      g_Player.PlayBackStarted += new MediaPortal.Player.g_Player.StartedHandler(OnPlayRecordingBackStarted);

      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvrecordedtv.xml");
      //LoadSettings();
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_RECORDEDTV, this);
      Restore();
      PreInit();
      ResetAllControls();
      return bResult;
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (listAlbums.Focus || listViews.Focus)
        {
          GUIListItem item = GetItem(0);
          if (item != null)
          {
            if (item.IsFolder && item.Label == "..")
            {
              currentShow = string.Empty;
              LoadDirectory();
              return;
            }
          }
        }
      }
      switch (action.wID)
      {
        case Action.ActionType.ACTION_DELETE_ITEM:
          {
            int item = GetSelectedItemNo();
            if (item >= 0)
              OnDeleteRecording(item);
            UpdateProperties();
          }
          break;
      }
      base.OnAction(action);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      m_iSelectedItem = GetSelectedItemNo();
      SaveSettings();
      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        if (TVHome.Card.IsTimeShifting && !(TVHome.Card.IsTimeShifting || TVHome.Card.IsRecording))
        {
          if (GUIGraphicsContext.ShowBackground)
          {
            // stop timeshifting & viewing... 

            //@Recorder.StopViewing();
          }
        }
      }
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      //DiskManagement.ImportDvrMsFiles();
      LoadSettings();
      LoadDirectory();

      /*
      if (!g_Player.IsTVRecording && !g_Player.Playing)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int)GUIWindow.Window.WINDOW_TV, GetID, 0, 0, 0, null);
        msg.SendToTargetWindow = true;
        GUIWindowManager.SendThreadMessage(msg);
      }
      */
      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0) m_iSelectedItem--;
      GUIControl.SelectItemControl(GetID, listViews.GetID, m_iSelectedItem);
      GUIControl.SelectItemControl(GetID, listAlbums.GetID, m_iSelectedItem);

      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
      //CreateThumbnails();
      //if (GlobalServiceProvider.Get<IThreadPool>().BusyThreadCount == 0)
      //{
      if (thumbworker == null)
      {
        if (_createRecordedThumbs)
          thumbworker = new RecordingThumbCacher();
      }
      else
        Log.Info("GUIRecordedTV: thumbworker already running - didn't start another one");
      //}
      //else
      //  Log.Info("GUIRecordedTV: threadpool busy - didn't start thumb creation");
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnView)
      {
        ShowViews();
        return;
      }

      if (control == btnViewAs)
      {
        switch (currentViewMethod)
        {
          case ViewAs.Album:
            currentViewMethod = ViewAs.List;
            break;
          case ViewAs.List:
            currentViewMethod = ViewAs.Album;
            break;
        }
        LoadDirectory();
      }

      if (control == btnSortBy) // sort by
      {
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            currentSortMethod = SortMethod.Date;
            break;
          case SortMethod.Date:
            currentSortMethod = SortMethod.Name;
            break;
          case SortMethod.Name:
            currentSortMethod = SortMethod.Genre;
            break;
          case SortMethod.Genre:
            currentSortMethod = SortMethod.Played;
            break;
          case SortMethod.Played:
            currentSortMethod = SortMethod.Duration;
            break;
          case SortMethod.Duration:
            currentSortMethod = SortMethod.Channel;
            break;
        }
        OnSort();
      }

      if (control == btnCleanup)
      {
        OnCleanup();
      }

      if (control == listAlbums || control == listViews)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0, null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnPlayRecording(iItem);
        }
        if (actionType == Action.ActionType.ACTION_SHOW_INFO)
        {
          OnShowContextMenu();
        }
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
          UpdateProperties();
          break;
      }
      return base.OnMessage(message);
    }

    protected override void OnShowContextMenu()
    {
      int iItem = GetSelectedItemNo();
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null) return;
      if (pItem.IsFolder) return;
      Recording rec = (Recording)pItem.TVTag;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(rec.Title);

      dlg.AddLocalizedString(655);     //Play recorded tv
      dlg.AddLocalizedString(656);     //Delete recorded tv
      //if (pItem.IsPlayed)
      //  dlg.AddLocalizedstring(830); //Reset watched status
      dlg.AddLocalizedString(1048);//Settings

      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;
      switch (dlg.SelectedId)
      {
        case 656: // delete
          OnDeleteRecording(iItem);
          break;

        case 655: // play
          if (OnPlayRecording(iItem))
            return;
          break;

        case 1048: // Settings
          TvRecordedInfo.CurrentProgram = rec;
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_RECORDED_INFO);
          break;

        //case 830: // Reset watched status
        //  {
        //    ResetWatchedStatus(rec.FileName);
        //    LoadDirectory();
        //  }
        //  break;
      }
    }

    public override void Process()
    {
      TVHome.UpdateProgressPercentageBar();
    }

    #endregion

    #region private methods

    private void ShowViews()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(652); // my recorded tv
      dlg.AddLocalizedString(914);
      dlg.AddLocalizedString(135);
      dlg.AddLocalizedString(915);
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;
      int nNewWindow = GetID;
      switch (dlg.SelectedId)
      {
        case 914: //	all
          nNewWindow = (int)GUIWindow.Window.WINDOW_RECORDEDTV;
          break;
        case 135: //	genres
          nNewWindow = (int)GUIWindow.Window.WINDOW_RECORDEDTVGENRE;
          break;
        case 915: //	channel
          nNewWindow = (int)GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL;
          break;
      }
      if (nNewWindow != GetID)
      {
        GUIWindowManager.ReplaceWindow(nNewWindow);
        Restore();
        PreInit();
        ResetAllControls();
      }
    }

    private void LoadDirectory()
    {
      GUIControl.ClearControl(GetID, listAlbums.GetID);
      GUIControl.ClearControl(GetID, listViews.GetID);

      List<GUIListItem> itemlist = new List<GUIListItem>();
      try
      {
        IList recordings = Recording.ListAll();
        if (currentShow == string.Empty)
        {
          foreach (Recording rec in recordings)
          {
            // catch exceptions here so MP will go on list further recs
            try
            {
              bool add = true;

              // combine recordings with the same name to a folder located on top
              foreach (GUIListItem item in itemlist)
              {
                if (item.TVTag != null)
                {
                  Recording rec2 = item.TVTag as Recording;
                  if (rec2 != null)
                  {
                    if (rec.Title.Equals(rec2.Title))
                    {
                      item.IsFolder = true;
                      Utils.SetDefaultIcons(item);
                      string strLogo = Utils.GetCoverArt(Thumbs.TVShows, rec.Title);
                      if (File.Exists(strLogo))
                      {
                        item.ThumbnailImage = strLogo;
                        item.IconImageBig = strLogo;
                        item.IconImage = strLogo;
                      }
                      add = false;
                      break;
                    }
                  }
                }
              }
              GUIListItem it = BuildItemFromRecording(rec);

              if (it != null)
              {
                if (add)
                {
                  // Add new list item for this recording
                  //GUIListItem item = BuildItemFromRecording(rec);                
                  itemlist.Add(it);
                }
                else
                {
                  if (IsRecordingActual(rec))
                  {
                    int i = 0;
                    foreach (GUIListItem obj in itemlist)
                    {
                      if (obj.Label.Equals(rec.Title) && obj.IsFolder)
                      {
                        it.IsFolder = true;
                        Utils.SetDefaultIcons(it);

                        itemlist.RemoveAt(i);
                        itemlist.Insert(i, it);
                        break;
                      }
                      i++;
                    }
                  }
                }
              }
            }
            catch (Exception recex)
            {
              Log.Error("TVRecorded: error processing recordings - {0}", recex.Message);
            }
          }
        }
        else
        {
          GUIListItem item = new GUIListItem("..");
          item.IsFolder = true;
          Utils.SetDefaultIcons(item);
          itemlist.Add(item);

          foreach (Recording rec in recordings)
          {
            if (!string.IsNullOrEmpty(rec.Title))
            {
              if (rec.Title.Equals(currentShow))
              {
                // Add new list item for this recording
                item = BuildItemFromRecording(rec);
                if (item != null)
                  itemlist.Add(item);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvRecorded: Error fetching recordings from database {0}", ex.Message);
      }

      foreach (GUIListItem item in itemlist)
      {
        listAlbums.Add(item);
        listViews.Add(item);
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", MediaPortal.Util.Utils.GetObjectCountLabel(itemlist.Count));

      OnSort();
      UpdateProperties();
    }

    private static bool IsRecordingActual(Recording aRecording)
    {

      TimeSpan tsRecording = (aRecording.EndTime - aRecording.StartTime);
      DateTime now = DateTime.Now;

      bool recStartEndSame = (tsRecording.TotalSeconds == 0);           

      if (recStartEndSame)
      {
        TvServer server = new TvServer();
        VirtualCard card;
        bool isRec = server.IsRecording(aRecording.ReferencedChannel().Name, out card);

        if (isRec)
        {
          if (aRecording.IsManual)
          {
            return true;
          }

          IList prgList = (IList)Program.RetrieveByTitle(aRecording.Title);

          if (prgList.Count > 0)
          {

            Schedule sched = Schedule.Retrieve(card.RecordingScheduleId);

            foreach (Program prg in prgList)
            {              
              if (sched.IsManual)
              {
                TimeSpan ts = now - aRecording.EndTime;
                if (aRecording.StartTime <= prg.EndTime && ts.TotalHours < 24) // if endtime is over 24 hrs old, then we do not consider it as a currently rec. program
                {                
                  return true;                
                }
              }
              else if (sched.IsRecordingProgram(prg, false))
              {
                return true;
              }                                            
            }
          }
        }
      }
      return false;
    }

    private static GUIListItem BuildItemFromRecording(Recording aRecording)
    {
      string strDefaultUnseenIcon = GUIGraphicsContext.Skin + @"\Media\defaultVideoBig.png";
      string strDefaultSeenIcon = GUIGraphicsContext.Skin + @"\Media\defaultVideoSeenBig.png";
      GUIListItem item = null;
      try
      {
        if (!string.IsNullOrEmpty(aRecording.Title))
        {
          item = new GUIListItem(aRecording.Title);
          item.TVTag = aRecording;
          string strLogo = Thumbs.TVRecorded + @"\" + Path.ChangeExtension(Utils.SplitFilename(aRecording.FileName), ".png");   

          if (!File.Exists(strLogo))
          {
            strLogo = Utils.GetCoverArt(Thumbs.TVChannel, aRecording.ReferencedChannel().DisplayName);
            if (!File.Exists(strLogo))
              strLogo = aRecording.TimesWatched > 0 ? strDefaultSeenIcon : strDefaultUnseenIcon;
          }
          else
          {
            string strLogoL = Utils.ConvertToLargeCoverArt(strLogo);
            if (File.Exists(strLogoL))
              item.IconImageBig = strLogoL;
            else
              item.IconImageBig = strLogo;
          }
          item.ThumbnailImage = strLogo;
          item.IconImage = strLogo;

          //Mark the recording with a "rec. symbol" if it is an active recording.

          if (IsRecordingActual(aRecording))
          {
            item.PinImage = Thumbs.TvRecordingIcon;
          }
        }
        else
          Log.Warn("TVRecorded: invalid recording title for {0}", aRecording.FileName);
      }
      catch (NullReferenceException singleex)
      {
        Log.Warn("TVRecorded: error building item from recording {0} - {1}", aRecording.FileName, singleex.Message);
      }

      return item;
    }

    private void UpdateButtonStates()
    {
      string strLine = string.Empty;
      switch (currentSortMethod)
      {
        case SortMethod.Channel:
          strLine = GUILocalizeStrings.Get(620);//Sort by: Channel
          break;
        case SortMethod.Date:
          strLine = GUILocalizeStrings.Get(621);//Sort by: Date
          break;
        case SortMethod.Name:
          strLine = GUILocalizeStrings.Get(268);//Sort by: Title
          break;
        case SortMethod.Genre:
          strLine = GUILocalizeStrings.Get(678);//Sort by: Genre
          break;
        case SortMethod.Played:
          strLine = GUILocalizeStrings.Get(671);//Sort by: Watched
          break;
        case SortMethod.Duration:
          strLine = GUILocalizeStrings.Get(1017);//Sort by: Duration
          break;
      }
      GUIControl.SetControlLabel(GetID, btnSortBy.GetID, strLine);
      switch (currentViewMethod)
      {
        case ViewAs.Album:
          strLine = GUILocalizeStrings.Get(100);
          break;
        case ViewAs.List:
          strLine = GUILocalizeStrings.Get(101);
          break;
        //        case ViewAs.BigIcon:
        //          strLine = GUILocalizeStrings.Get(417);
        //          break;
      }
      GUIControl.SetControlLabel(GetID, btnViewAs.GetID, strLine);

      btnSortBy.IsAscending = m_bSortAscending;

      GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMTITLE);
      GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMDESCRIPTION);
      GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMGENRE);
      GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMTIME);

      if (currentViewMethod == ViewAs.List)
      {
        listAlbums.IsVisible = false;
        listViews.IsVisible = true;
      }
      else
      {
        listAlbums.IsVisible = true;
        listViews.IsVisible = false;
      }
    }

    private void SetLabels()
    {
      SortMethod method = currentSortMethod;
      bool bAscending = m_bSortAscending;

      for (int i = 0; i < listAlbums.Count; ++i)
      {
        try
        {
          GUIListItem item1 = listAlbums[i];
          GUIListItem item2 = listViews[i];
          if (item1.Label == "..") continue;
          Recording rec = (Recording)item1.TVTag;
          item1.Label = item2.Label = rec.Title;
          TimeSpan ts = rec.EndTime - rec.StartTime;
          string strTime = string.Format("{0} {1} ({2})",
            Utils.GetShortDayString(rec.StartTime),
            rec.StartTime.ToShortTimeString(),
            Utils.SecondsToHMString((int)ts.TotalSeconds));
          item1.Label2 = item2.Label2 = strTime;
          if (currentViewMethod == ViewAs.Album)
          {
            if (rec.Genre != "unknown")
              item1.Label3 = item2.Label3 = rec.Genre;
            else
              item1.Label3 = item2.Label3 = string.Empty;
          }
          else
          {
            if (currentSortMethod == SortMethod.Channel)
              item1.Label2 = item2.Label2 = rec.ReferencedChannel().DisplayName;
          }
          if (rec.TimesWatched > 0)
          {
            if (!item1.IsFolder)
              item1.IsPlayed = true;
            if (!item2.IsFolder)
              item2.IsPlayed = true;
          }
        }
        catch (Exception ex)
        {
          Log.Warn("TVRecorded: error in SetLabels - {0}", ex.Message);
        }
      }
    }

    private bool OnPlayRecording(int iItem)
    {
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null) return false;
      if (pItem.IsFolder)
      {
        if (pItem.Label.Equals(".."))
          currentShow = string.Empty;
        else
          currentShow = pItem.Label;
        LoadDirectory();
        return false;
      }

      Recording rec = (Recording)pItem.TVTag;
      IList itemlist = Recording.ListAll();

      m_oActiveRecording = rec;
      m_bIsLiveRecording = false;
      TvServer server = new TvServer();      
      foreach (Recording recItem in itemlist)
      {
        if (rec.IdRecording == recItem.IdRecording && IsRecordingActual(recItem))
        {
          m_bIsLiveRecording = true;
          break;
        }
      }

      // if we are currently playing a TV recording and want to play a new tv recoding, then we will avoid the .stop() call, since it will 
      // simly show the last TV channel (time consuming).
      // instead we just playback the newly selected TV recording
      if (!g_Player.IsTVRecording)
      {
        Log.Info("OnPlayRecording - calling g_Player.Stop(true); ");
        g_Player.Stop(true);
      }

      if (TVHome.Card != null)
        TVHome.Card.StopTimeShifting();

      int stoptime = rec.StopTime;
      if (stoptime > 0)
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
        if (null == dlgYesNo) return false;
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
        dlgYesNo.SetLine(1, rec.ReferencedChannel().DisplayName);
        dlgYesNo.SetLine(2, rec.Title);
        dlgYesNo.SetLine(3, GUILocalizeStrings.Get(936) + Utils.SecondsToHMSString(rec.StopTime));
        dlgYesNo.SetDefaultToYes(true);
        dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
        if (!dlgYesNo.IsConfirmed) stoptime = 0;
      }

      else if (m_bIsLiveRecording)
      {

        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        if (dlg != null)
        {
          dlg.Reset();
          dlg.SetHeading(rec.Title);
          dlg.AddLocalizedString(979); //Play recording from beginning
          dlg.AddLocalizedString(980); //Play recording from live point
          dlg.DoModal(GetID);
          if (dlg.SelectedId == 979)
          {
          }
          else if (dlg.SelectedId == 980)
          {
            TVHome.ViewChannelAndCheck(rec.ReferencedChannel());
            if (g_Player.Playing)
              g_Player.ShowFullScreenWindow();
            return true;
          }
        }
      }


      /*
        IMDBMovie movieDetails = new IMDBMovie();
        VideoDatabase.GetMovieInfo(rec.FileName, ref movieDetails);
        int idMovie = VideoDatabase.GetMovieId(rec.FileName);
        int idFile = VideoDatabase.GetFileId(rec.FileName);
        if (idMovie >= 0 && idFile >= 0 )
        {
          Log.Info("play got movie id:{0} for {1}", idMovie, rec.FileName);
          stoptime = VideoDatabase.GetMovieStopTime(idMovie);
          if (stoptime > 0)
          {
            string title = System.IO.Path.GetFileName(rec.FileName);
            if (movieDetails.Title != string.Empty) title = movieDetails.Title;

            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo) return false;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
            dlgYesNo.SetLine(1, rec.Channel);
            dlgYesNo.SetLine(2, title);
            dlgYesNo.SetLine(3, GUILocalizeStrings.Get(936) + Utils.SecondsToHMSstring(stoptime));
            dlgYesNo.SetDefaultToYes(true);
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

            if (!dlgYesNo.IsConfirmed) stoptime = 0;
          }
        }
      */
      string fileName = rec.FileName;

      bool recFileExists = System.IO.File.Exists(fileName);

      if (!recFileExists && !TVHome.UseRTSP())
      {
        if (TVHome.RecordingPath().Length > 0)
        {

          string path = Path.GetDirectoryName(fileName);
          int index = path.LastIndexOf("\\");


          if (index == -1)
          {
            fileName = TVHome.RecordingPath() + "\\" + Path.GetFileName(fileName);
          }
          else
          {
            fileName = TVHome.RecordingPath() + path.Substring(index) + "\\" + Path.GetFileName(fileName);
          }
        }
        else
        {
          fileName = fileName.Replace(":", "");
          fileName = "\\\\" + RemoteControl.HostName + "\\" + fileName;
        }
        recFileExists = System.IO.File.Exists(fileName);
      }

      //populates recording metadata to g_player;
      g_Player.currentFileName = fileName;

      if (!System.IO.File.Exists(fileName))
      {
        fileName = TVHome.TvServer.GetStreamUrlForFileName(rec.IdRecording);
      }

			//should these two line also be in tvguidebase and tvscheduler?
      g_Player.currentTitle = rec.Title;
      g_Player.currentDescription = rec.Description;

      Log.Info("TvRecorded Play:{0} - using rtsp mode:{1}", fileName, TVHome.UseRTSP());
      if (g_Player.Play(fileName, g_Player.MediaType.Recording))
      {
        if (Utils.IsVideo(fileName))
        {
          g_Player.ShowFullScreenWindow();
          g_Player.SeekAbsolute(0);
        }
        if (stoptime > 0)
        {
          g_Player.SeekAbsolute(stoptime);
        }
				//if playback starts successfully then update timeswatched
				rec.TimesWatched++;
				rec.Persist();
        return true;
      }

      return false;
    }

    private void OnDeleteRecording(int iItem)
    {
      m_iSelectedItem = GetSelectedItemNo();
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null) return;
      if (pItem.IsFolder) return;
      Recording rec = (Recording)pItem.TVTag;      
      
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo) return;

      bool isRecPlaying = false;
      if (g_Player.currentFileName.Length > 0 && g_Player.IsTVRecording && g_Player.Playing)
      {
        FileInfo fInfo = new FileInfo(g_Player.currentFileName);
        isRecPlaying = (rec.FileName.IndexOf(fInfo.Name) > -1);
      }

      dlgYesNo.SetDefaultToYes(false);
      bool isRec = IsRecordingActual(rec);
      bool activeRecDeleted = false;
      TvServer server = new TvServer(); ;
      if (isRec)
      { 
        
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));//Delete this recording?
        dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730));//This schedule is recording. If you delete
        dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731));//the schedule then the recording is stopped.
        dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732));//are you sure
        dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
        if (!dlgYesNo.IsConfirmed)
        {
          return;
        }        
        
        IList schedulesList = Schedule.ListAll();
        if (schedulesList != null)
        {
          if (rec != null)
          {
            foreach (Schedule s in schedulesList)
            {
              if (s.ReferencedChannel().IdChannel == rec.ReferencedChannel().IdChannel)
              {
                TimeSpan ts = s.StartTime - rec.StartTime;

                ScheduleRecordingType scheduleType = (ScheduleRecordingType)s.ScheduleType;

                bool isManual = (scheduleType == ScheduleRecordingType.Once);

                if ((ts.Minutes == 0 && isManual) || (s.ProgramName.Equals(rec.Title) && isManual) || (isManual))
                {
                  VirtualCard card;

                  if (!server.IsRecording(rec.ReferencedChannel().Name, out card)) return;
                  if (isRecPlaying)
                  {
                    g_Player.Stop();
                  }
                  TVHome.PromptAndDeleteRecordingSchedule(s.IdSchedule, null, false, true);                                    
                  activeRecDeleted = true;                  
                }
              }
            }
          }
        }

      }
      else
      {
        if (rec.TimesWatched > 0) dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));
        else dlgYesNo.SetHeading(GUILocalizeStrings.Get(820));
        dlgYesNo.SetLine(1, rec.ReferencedChannel().DisplayName);
        dlgYesNo.SetLine(2, rec.Title);
        dlgYesNo.SetLine(3, string.Empty);
        dlgYesNo.DoModal(GetID);
        if (!dlgYesNo.IsConfirmed)
        {
          return;
        }
        if (isRecPlaying)
        {
          g_Player.Stop();
        }
      }

      // we have to make sure that the recording process on the server has indeed stopped, otherwise we are not able to delete the 
      // recording file.
      // we will max. wait 5 sec.
      if (activeRecDeleted)
      {
        DateTime now = DateTime.Now;
        bool timeOut = false;
        VirtualCard card;
        while (server.IsRecording(rec.ReferencedChannel().Name, out card) && !timeOut)
        {
          TimeSpan ts = (DateTime.Now - now);
          timeOut = ts.TotalSeconds > 5; //5 sec, then timeout
          Thread.Sleep(1000);
        }
      }

      //somehow we were unable to delete the file, notify the user.
      if (!server.DeleteRecording(rec.IdRecording))
      {
        GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);

        if (dlgOk != null)
        {
          dlgOk.SetHeading(257);
          dlgOk.SetLine(1, GUILocalizeStrings.Get(200054));
          dlgOk.SetLine(2, rec.Title);
          dlgOk.DoModal(GetID);
        }
      }
      
      Gentle.Common.CacheManager.Clear();

      LoadDirectory();
      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0) m_iSelectedItem--;
      GUIControl.SelectItemControl(GetID, listViews.GetID, m_iSelectedItem);
      GUIControl.SelectItemControl(GetID, listAlbums.GetID, m_iSelectedItem);
    }

    private void OnCleanup()
    {
      m_iSelectedItem = GetSelectedItemNo();
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo) return;
      dlgYesNo.SetHeading(GUILocalizeStrings.Get(200043));//Cleanup recordings?
      dlgYesNo.SetLine(1, GUILocalizeStrings.Get(200050));//This will delete your recordings from harddisc
      dlgYesNo.SetLine(2, GUILocalizeStrings.Get(506)); // Are you sure?
      dlgYesNo.SetLine(3, string.Empty);
      dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
      if (!dlgYesNo.IsConfirmed)
        return;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(200043));//Cleanup recordings?

      dlg.Add(new GUIListItem(GUILocalizeStrings.Get(676))); // Only watched recordings?
      dlg.Add(new GUIListItem(GUILocalizeStrings.Get(200044))); // Only invalid recordings?
      dlg.Add(new GUIListItem(GUILocalizeStrings.Get(200045))); // Both?
      if (currentShow != "") dlg.Add(new GUIListItem(GUILocalizeStrings.Get(200049))); // Only watched recordings from this folder. 
      dlg.Add(new GUIListItem(GUILocalizeStrings.Get(222))); // Cancel?
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0) return;
      if (dlg.SelectedLabel > 4) return;

      if ((dlg.SelectedLabel == 0) || (dlg.SelectedLabel == 2))
        DeleteWatchedRecordings(null);
      if ((dlg.SelectedLabel == 1) || (dlg.SelectedLabel == 2))
        DeleteInvalidRecordings();
      if (dlg.SelectedLabel == 3 && currentShow != "")
        DeleteWatchedRecordings(currentShow);
      Gentle.Common.CacheManager.Clear();
      dlg.Reset();
      LoadDirectory();
      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0) m_iSelectedItem--;
      GUIControl.SelectItemControl(GetID, listViews.GetID, m_iSelectedItem);
      GUIControl.SelectItemControl(GetID, listAlbums.GetID, m_iSelectedItem);
    }

    private void DeleteWatchedRecordings(string _currentTitle)
    {
      IList itemlist = Recording.ListAll();
      TvServer server = new TvServer();
      foreach (Recording rec in itemlist)
      {
        if (rec.TimesWatched > 0)
          if (_currentTitle == null || _currentTitle == rec.Title)
            server.DeleteRecording(rec.IdRecording);
      }
    }

    private void DeleteInvalidRecordings()
    {
      IList itemlist = Recording.ListAll();
      TvServer server = new TvServer();
      foreach (Recording rec in itemlist)
      {
        if (!server.IsRecordingValid(rec.IdRecording))
          server.DeleteRecording(rec.IdRecording);
      }
    }

    private void UpdateProperties()
    {
      Recording rec;
      GUIListItem pItem = GetItem(GetSelectedItemNo());
      if (pItem == null)
      {
        SetProperties(null);
        return;
      }
      rec = pItem.TVTag as Recording;
      if (rec == null)
      {
        SetProperties(null);
        return;
      }
      SetProperties(rec);
    }

    private void SetProperties(Recording rec)
    {
      if (rec == null)
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", "");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", "");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", "");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", "");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", "");
        return;
      }
      string strTime = string.Format("{0} {1} - {2}",
        Utils.GetShortDayString(rec.StartTime),
        rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
        rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", rec.Title);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", rec.Genre);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", strTime);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", rec.Description);

      string strLogo = "";
      if (rec.ReferencedChannel() != null)
      {
        strLogo = Utils.GetCoverArt(Thumbs.TVChannel, rec.ReferencedChannel().DisplayName);
      }
      if (System.IO.File.Exists(strLogo))
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", strLogo);
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", "defaultVideoBig.png");
      }
    }
    #endregion

    #region album/list view management
    GUIListItem GetSelectedItem()
    {
      int iControl;
      iControl = listAlbums.GetID;
      if (currentViewMethod == ViewAs.List)
        iControl = listViews.GetID;
      GUIListItem item = GUIControl.GetSelectedListItem(GetID, iControl);
      return item;
    }

    GUIListItem GetItem(int iItem)
    {
      if (currentViewMethod == ViewAs.List)
      {
        if (iItem < 0 || iItem >= listViews.Count) return null;
        return listViews[iItem];
      }
      else
      {
        if (iItem < 0 || iItem >= listAlbums.Count) return null;
        return listAlbums[iItem];
      }
    }

    int GetSelectedItemNo()
    {
      int iControl;
      iControl = listAlbums.GetID;
      if (currentViewMethod == ViewAs.List)
        iControl = listViews.GetID;

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
      OnMessage(msg);
      int iItem = (int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      if (currentViewMethod == ViewAs.List)
        return listViews.Count;
      else
        return listAlbums.Count;
    }
    #endregion

    #region Sort Members
    void OnSort()
    {
      SetLabels();
      listAlbums.Sort(this);
      listViews.Sort(this);
      UpdateButtonStates();
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2) return 0;
      if (item1 == null) return -1;
      if (item2 == null) return -1;
      if (item1.IsFolder && item1.Label == "..") return -1;
      if (item2.IsFolder && item2.Label == "..") return -1;
      if (item1.IsFolder && !item2.IsFolder) return -1;
      else if (!item1.IsFolder && item2.IsFolder) return 1;

      int iComp = 0;
      TimeSpan ts;
      Recording rec1 = (Recording)item1.TVTag;
      Recording rec2 = (Recording)item2.TVTag;
      switch (currentSortMethod)
      {
        case SortMethod.Played:
          item1.Label2 = string.Format("{0} {1}", rec1.TimesWatched, GUILocalizeStrings.Get(677));//times
          item2.Label2 = string.Format("{0} {1}", rec2.TimesWatched, GUILocalizeStrings.Get(677));//times
          if (rec1.TimesWatched == rec2.TimesWatched) goto case SortMethod.Name;
          else
          {
            if (m_bSortAscending) return rec1.TimesWatched - rec2.TimesWatched;
            else return rec2.TimesWatched - rec1.TimesWatched;
          }

        case SortMethod.Name:
          if (m_bSortAscending)
          {
            iComp = string.Compare(rec1.Title, rec2.Title, true);
            if (iComp == 0) goto case SortMethod.Channel;
            else return iComp;
          }
          else
          {
            iComp = string.Compare(rec2.Title, rec1.Title, true);
            if (iComp == 0) goto case SortMethod.Channel;
            else return iComp;
          }


        case SortMethod.Channel:
          if (m_bSortAscending)
          {
            iComp = string.Compare(rec1.ReferencedChannel().DisplayName, rec2.ReferencedChannel().DisplayName, true);
            if (iComp == 0) goto case SortMethod.Date;
            else return iComp;
          }
          else
          {
            iComp = string.Compare(rec2.ReferencedChannel().DisplayName, rec1.ReferencedChannel().DisplayName, true);
            if (iComp == 0) goto case SortMethod.Date;
            else return iComp;
          }

        case SortMethod.Duration:
          {
            TimeSpan duration1 = (rec1.EndTime - rec1.StartTime);
            TimeSpan duration2 = rec2.EndTime - rec2.StartTime;
            if (m_bSortAscending)
            {
              if (duration1 == duration2) goto case SortMethod.Date;
              if (duration1 > duration2) return 1;
              return -1;
            }
            else
            {
              if (duration1 == duration2) goto case SortMethod.Date;
              if (duration1 < duration2) return 1;
              return -1;
            }
          }

        case SortMethod.Date:
          if (m_bSortAscending)
          {
            if (rec1.StartTime == rec2.StartTime) return 0;
            if (rec1.StartTime < rec2.StartTime) return 1;
            return -1;
          }
          else
          {
            if (rec1.StartTime == rec2.StartTime) return 0;
            if (rec1.StartTime > rec2.StartTime) return 1;
            return -1;
          }

        case SortMethod.Genre:
          item1.Label2 = rec1.Genre;
          item2.Label2 = rec2.Genre;
          if (rec1.Genre != rec2.Genre)
          {
            if (m_bSortAscending)
              return string.Compare(rec1.Genre, rec2.Genre, true);
            else
              return string.Compare(rec2.Genre, rec1.Genre, true);
          }
          if (rec1.StartTime != rec2.StartTime)
          {
            if (m_bSortAscending)
            {
              ts = rec1.StartTime - rec2.StartTime;
              return (int)(ts.Minutes);
            }
            else
            {
              ts = rec2.StartTime - rec1.StartTime;
              return (int)(ts.Minutes);
            }
          }
          if (rec1.IdChannel != rec2.IdChannel)
            if (m_bSortAscending)
              return string.Compare(rec1.ReferencedChannel().DisplayName, rec2.ReferencedChannel().DisplayName);
            else
              return string.Compare(rec2.ReferencedChannel().DisplayName, rec1.ReferencedChannel().DisplayName);
          if (rec1.Title != rec2.Title)
            if (m_bSortAscending)
              return string.Compare(rec1.Title, rec2.Title);
            else
              return string.Compare(rec2.Title, rec1.Title);
          return 0;
      }
      return 0;
    }

    void SortChanged(object sender, SortEventArgs e)
    {
      m_bSortAscending = e.Order != System.Windows.Forms.SortOrder.Descending;
      OnSort();
    }
    #endregion

    #region playback events
    private void OnPlayRecordingBackStopped(MediaPortal.Player.g_Player.MediaType type, int stoptime, string filename)
    {
      Log.Info("TvRecorded:OnStopped {0} {1}", type, filename);
      if (type != g_Player.MediaType.Recording) return;

      // the m_oActiveRecording object should always reflect the currently played back recording
      // we can not rely on filename from the method parameter, as this can be a RTSP://123455 kind of URL.
      if (m_oActiveRecording != null) filename = m_oActiveRecording.FileName;

      
      if (filename.Length > 0)
      {
        FileInfo f = new FileInfo(filename);
        filename = f.Name;
      }

      TvBusinessLayer layer = new TvBusinessLayer();
      Recording rec = layer.GetRecordingByFileName(filename);
      if (rec != null)
      {       
        if (stoptime >= g_Player.Duration) { stoptime = 0; }; //temporary workaround before end of stream get's properly implemented        
        rec.StopTime = stoptime;
        rec.Persist();
      }
      else
      {
        Log.Info("TvRecorded:OnStopped no recording found with filename {0}", filename);
      }

      /*
      if (GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow))
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int)GUIWindow.Window.WINDOW_TV, GetID, 0, 0, 0, null);
        msg.SendToTargetWindow = true;
        GUIWindowManager.SendThreadMessage(msg);
      }
      */
    }

    private void OnPlayRecordingBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Recording) return;

      if (filename.Substring(0, 4) == "rtsp") { filename = g_Player.currentFileName; };

      g_Player.Stop();

      TvBusinessLayer layer = new TvBusinessLayer();
      Recording rec = layer.GetRecordingByFileName(filename);
      if (rec != null)
      {
        if (_deleteWatchedShows || rec.KeepUntil == (int)KeepMethodType.UntilWatched)
        {
          TvServer server = new TvServer();
          server.DeleteRecording(rec.IdRecording);
        }
        else
        {
          rec.StopTime = 0;
          rec.Persist();
        }
      }

      //@int movieid = VideoDatabase.GetMovieId(filename);
      //@if (movieid < 0) return;

      //@VideoDatabase.DeleteMovieStopTime(movieid);

      //@IMDBMovie details = new IMDBMovie();
      //@VideoDatabase.GetMovieInfoById(movieid, ref details);
      //@details.Watched++;
      //@VideoDatabase.SetWatched(details);
    }

    private void OnPlayRecordingBackStarted(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Recording) return;

      // set audio track based on user prefs. 
      int prefLangIdx = TVHome.GetPreferedAudioStreamIndex();

      MediaPortal.GUI.Library.Log.Debug("TVRecorded.OnPlayRecordingBackStarted(): setting audioIndex on tsreader {0}", prefLangIdx);
      g_Player.CurrentAudioStream = prefLangIdx;

      //@VideoDatabase.AddMovieFile(filename);
    }


    /// TODO : Update for TVE3 DB
    //private void ResetWatchedStatus(string filename)
    //{
    //  if (VideoDatabase.HasMovieInfo(filename))
    //  {
    //    IMDBMovie movieDetails = new IMDBMovie();
    //    int idMovie = VideoDatabase.GetMovieInfo(filename, ref movieDetails);
    //    movieDetails.Watched = 0;
    //    VideoDatabase.SetWatched(movieDetails);
    //  }
    //  int fileId = VideoDatabase.GetFileId(filename);
    //  VideoDatabase.DeleteMovieStopTime(fileId);

    //  TVRecorded rec = new TVRecorded();
    //  TVDatabase.GetRecordedTVByFilename(filename, ref rec);
    //  rec.TimesWatched = 0;
    //  TVDatabase.PlayedRecordedTV(rec);
    //}
    #endregion


  }
}
