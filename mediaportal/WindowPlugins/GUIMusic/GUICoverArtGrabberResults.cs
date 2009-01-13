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
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Amazon;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Music
{
  public class GUICoverArtGrabberResults : GUIWindow, IRenderLayer
  {
    public delegate void FindCoverArtProgressHandler(AmazonWebservice aws, int progressPercent);

    public event FindCoverArtProgressHandler FindCoverArtProgress;

    public delegate void FindCoverArtDoneHandler(AmazonWebservice aws, EventArgs e);

    public event FindCoverArtDoneHandler FindCoverArtDone;

    public delegate void AlbumNotFoundRetryingFilteredHandler(
      AmazonWebservice aws, string origAlbumName, string filteredAlbumName);

    public event AlbumNotFoundRetryingFilteredHandler AlbumNotFoundRetryingFiltered;

    public enum SearchDepthMode
    {
      Album = 0,
      Share = 2,
    } ;

    private enum ControlIDs
    {
      IMG_COVERART = 10,
      LBL_COVER = 11,
      LBL_ARTIST_NAME = 12,
      LBL_ALBUM_NAME = 13,
      LBL_RELEASE_YEAR = 14,
      LBL_NO_MATCHES_FOUND = 15,
      BTN_SKIP = 20,
      BTN_CANCEL = 21,
      LIST_ALBUM = 25,
      IMG_AMAZON = 30,
    }

    [SkinControl((int) ControlIDs.LIST_ALBUM)] protected GUIListControl listView = null;

    [SkinControl((int) ControlIDs.IMG_COVERART)] protected GUIImage imgCoverArt = null;

    [SkinControl((int) ControlIDs.LBL_COVER)] protected GUILabelControl lblCoverLabel = null;

    [SkinControl((int) ControlIDs.LBL_ARTIST_NAME)] protected GUIFadeLabel lblArtistName = null;

    [SkinControl((int) ControlIDs.LBL_ALBUM_NAME)] protected GUIFadeLabel lblAlbumName = null;

    [SkinControl((int) ControlIDs.LBL_RELEASE_YEAR)] protected GUILabelControl lblReleaseYear = null;

    [SkinControl((int) ControlIDs.LBL_NO_MATCHES_FOUND)] protected GUILabelControl lblNoMatches = null;

    [SkinControl((int) ControlIDs.BTN_SKIP)] protected GUIButtonControl btnSkip = null;

    [SkinControl((int) ControlIDs.BTN_CANCEL)] protected GUIButtonControl btnCancel = null;

    [SkinControl((int) ControlIDs.IMG_AMAZON)] protected GUIImage imgAmazon = null;

    #region Base Dialog Variables

    private bool m_bRunning = false;
    private bool m_bRefresh = false;
    private int m_dwParentWindowID = 0;
    private GUIWindow m_pParentWindow = null;

    #endregion

    #region Variables

    private const int MAX_UNFILTERED_SEARCH_ITEMS = 40;
    private const int MAX_SEARCH_ITEMS = 12;

    private string[] DiskSetSubstrings = new string[]
                                           {
                                             "-disk",
                                             "- disk",
                                             "(disk",
                                             "( disk",
                                             "-disc",
                                             "- disc",
                                             "(disc",
                                             "( disc",
                                             "-vol",
                                             "- vol",
                                           };


    private string _ThumbPath = string.Empty;
    private Texture coverArtTexture = null;
    private bool _prevOverlay = false;

    private string _Artist = string.Empty;
    private string _Album = string.Empty;
    private string _AlbumPath = string.Empty;
    private AmazonWebservice amazonWS = null;
    private AlbumInfo _SelectedAlbum = null;
    private SearchDepthMode _SearchMode = SearchDepthMode.Album;
    private static bool _CancelledByUser = false;
    private bool IsCompilationAlbum = false;

    #endregion

    #region Properties

    public bool NeedsRefresh
    {
      get { return m_bRefresh; }
    }

    public SearchDepthMode SearchMode
    {
      get { return _SearchMode; }
      set { _SearchMode = value; }
    }

    public AlbumInfo SelectedAlbum
    {
      get { return _SelectedAlbum; }
    }

    public static bool CancelledByUser
    {
      get { return _CancelledByUser; }
      set { _CancelledByUser = value; }
    }

    public AmazonWebservice AmazonWebService
    {
      get { return amazonWS; }
    }

    #endregion

    public GUICoverArtGrabberResults()
    {
      GetID = (int) Window.WINDOW_MUSIC_COVERART_GRABBER_RESULTS;
    }

    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.Skin + @"\MyMusicCoverArtGrabberResults.xml");
      return result;
    }

    public override void OnAction(Action action)
    {
      Console.WriteLine(action.wID);

      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        CancelledByUser = true;
        Close();
        return;
      }

      base.OnAction(action);
    }

    #region Base Dialog Members

    private void Close()
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
      OnMessage(msg);

      GUIWindowManager.UnRoute();
      m_pParentWindow = null;
      m_bRunning = false;
    }

    public void DoModal(int dwParentId)
    {
      _CancelledByUser = false;
      m_bRefresh = false;
      m_dwParentWindowID = dwParentId;
      m_pParentWindow = GUIWindowManager.GetWindow(m_dwParentWindowID);

      if (null == m_pParentWindow)
      {
        m_dwParentWindowID = 0;
        return;
      }

      GUIWindowManager.RouteToWindow(GetID);

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, 0, 0, null);
      OnMessage(msg);

      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
      m_bRunning = true;
      UpdateAlbumCoverList();
      SetButtonVisibility();

      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
      GUILayerManager.UnRegisterLayer(this);
    }

    #endregion

    protected override void OnPageDestroy(int newWindowId)
    {
      if (m_bRunning)
      {
        // Probably user pressed H (SWITCH_HOME)
        GUIWindowManager.UnRoute();
        m_pParentWindow = null;
        m_bRunning = false;
      }
      base.OnPageDestroy(newWindowId);

      if (coverArtTexture != null)
      {
        coverArtTexture.Dispose();
        coverArtTexture = null;
      }
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      coverArtTexture = null;
      Reset();

      listView.NavigateRight = (int) ControlIDs.LIST_ALBUM;

      if (_SearchMode == SearchDepthMode.Album)
      {
        btnCancel.NavigateUp = (int) ControlIDs.BTN_CANCEL;
        listView.NavigateLeft = (int) ControlIDs.BTN_CANCEL;
      }

      else
      {
        btnCancel.NavigateUp = (int) ControlIDs.BTN_SKIP;
        listView.NavigateLeft = (int) ControlIDs.BTN_SKIP;
      }
      GUIPropertyManager.SetProperty("#currentmodule",
                                     String.Format("{0}/{1}", GUILocalizeStrings.Get(100005),
                                                   GUILocalizeStrings.Get(4515)));
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      switch (controlId)
      {
        case (int) ControlIDs.LIST_ALBUM:
          {
            int selectedItem = listView.SelectedListItemIndex;

            if (selectedItem >= amazonWS.AlbumCount)
            {
              Log.Info("Cover art grabber:user selected item #{0}", listView.SelectedListItemIndex);

              _CancelledByUser = false;
              _SelectedAlbum = null;
              Close();
              return;
            }

            _SelectedAlbum = (AlbumInfo) amazonWS.AlbumInfoList[selectedItem];
            this.Close();
            break;
          }

        case (int) ControlIDs.BTN_SKIP:
          {
            Log.Info("Cover art grabber:[{0}-{1}] skipped by user", _Artist, _Album);

            _CancelledByUser = false;
            _SelectedAlbum = null;
            Close();
            break;
          }

        case (int) ControlIDs.BTN_CANCEL:
          {
            Log.Info("Cover art grabber:user cancelled out of grab results");

            _CancelledByUser = true;
            Close();
            break;
          }
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          _prevOverlay = GUIGraphicsContext.Overlay;
          base.OnMessage(message);
          GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(4515));
          return true;
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          base.OnMessage(message);
          GUIGraphicsContext.Overlay = _prevOverlay;
          return true;

        default:
          return base.OnMessage(message);
      }

      //return base.OnMessage(message);
    }

    private void SetButtonVisibility()
    {
      switch (_SearchMode)
      {
        case SearchDepthMode.Album:
          {
            this.btnCancel.Visible = true;
            this.btnSkip.Visible = false;
            break;
          }

        case SearchDepthMode.Share:
          {
            this.btnCancel.Visible = true;
            this.btnSkip.Visible = true;
            break;
          }
      }
    }

    public void GetAlbumCovers(string artist, string album, string strPath, int parentWindowID,
                               bool checkForCompilationAlbum)
    {
      _SelectedAlbum = null;
      IsCompilationAlbum = false;

      if (checkForCompilationAlbum)
      {
        IsCompilationAlbum = GetIsCompilationAlbum(strPath, -1);
      }

      _Artist = artist;
      _Album = album;
      _AlbumPath = strPath;
      string origAlbumName = _Album;
      string filteredAlbumFormatString = GUILocalizeStrings.Get(4518);

      if (filteredAlbumFormatString.Length == 0)
      {
        filteredAlbumFormatString = "Album title not found\r\nTrying: {0}";
      }

      _ThumbPath = GetCoverArtThumbPath(artist, album, strPath);
      amazonWS = new AmazonWebservice();
      amazonWS.MaxSearchResultItems = MAX_SEARCH_ITEMS;

      amazonWS.FindCoverArtProgress += new AmazonWebservice.FindCoverArtProgressHandler(amazonWS_GetAlbumInfoProgress);
      amazonWS.FindCoverArtDone += new AmazonWebservice.FindCoverArtDoneHandler(amazonWS_FindCoverArtDone);

      Log.Info("Cover art grabber:getting cover art for [{0}-{1}]...", _Artist, _Album);

      if (IsCompilationAlbum)
      {
        Log.Info("Cover art grabber:compilation album found", _Artist, _Album);

        amazonWS.MaxSearchResultItems = MAX_UNFILTERED_SEARCH_ITEMS;
        _Artist = "";
        string filterString = string.Format("{0} = \"{1}\"", GUILocalizeStrings.Get(484), " ");
        string filter = string.Format(filteredAlbumFormatString, filterString);

        Log.Info("Cover art grabber:trying again with blank artist name...");
        InternalGetAlbumCovers(_Artist, _Album, filter);
      }

      else
      {
        InternalGetAlbumCovers(_Artist, _Album, string.Empty);
      }

      // Did we fail to find any albums?
      if (!amazonWS.HasAlbums && !amazonWS.AbortGrab)
      {
        // Check if the album title includes a disk number description that might 
        // be altering the proper album title such as: White album (Disk 2)

        string cleanAlbumName = string.Empty;

        if (StripDiskNumberFromAlbumName(_Album, ref cleanAlbumName))
        {
          amazonWS.MaxSearchResultItems = MAX_UNFILTERED_SEARCH_ITEMS;

          if (AlbumNotFoundRetryingFiltered != null)
          {
            AlbumNotFoundRetryingFiltered(amazonWS, origAlbumName, cleanAlbumName);
          }

          Log.Info("Cover art grabber:[{0}-{1}] not found. Trying [{0}-{2}]...", _Artist, _Album, cleanAlbumName);

          string filter = string.Format(filteredAlbumFormatString, cleanAlbumName);
          origAlbumName = _Album;
          InternalGetAlbumCovers(_Artist, cleanAlbumName, filter);
        }

        else if (GetProperAlbumName(_Album, ref cleanAlbumName))
        {
          amazonWS.MaxSearchResultItems = MAX_UNFILTERED_SEARCH_ITEMS;

          if (AlbumNotFoundRetryingFiltered != null)
          {
            AlbumNotFoundRetryingFiltered(amazonWS, origAlbumName, cleanAlbumName);
          }

          Log.Info("Cover art grabber:[{0}-{1}] not found. Trying album name without sub-title [{0}-{2}]...", _Artist,
                   _Album, cleanAlbumName);

          string filter = string.Format(filteredAlbumFormatString, cleanAlbumName);
          origAlbumName = _Album;
          InternalGetAlbumCovers(_Artist, cleanAlbumName, filter);
        }
      }

      // Still no albums?
      if (!IsCompilationAlbum && !amazonWS.HasAlbums && !amazonWS.AbortGrab)
      {
        amazonWS.MaxSearchResultItems = MAX_UNFILTERED_SEARCH_ITEMS;

        if (AlbumNotFoundRetryingFiltered != null)
        {
          AlbumNotFoundRetryingFiltered(amazonWS, origAlbumName, GUILocalizeStrings.Get(4506));
        }

        string filterString = string.Format("{0} = \"{1}\"", GUILocalizeStrings.Get(483), " ");
        string filter = string.Format(filteredAlbumFormatString, filterString);

        // Try searching by artist only to get all albums for this artist...
        Log.Info("Cover art grabber:[{0}-{1}] not found. Trying again with blank album name...", _Artist, _Album);
        InternalGetAlbumCovers(_Artist, "", filter);
      }

      // if we're searching for a single album the progress dialog will 
      // be displayed so we need to close it...
      if (SearchMode == SearchDepthMode.Album)
      {
        GUIDialogProgress dlgProgress =
          (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
        if (dlgProgress != null)
        {
          dlgProgress.SetPercentage(100);
          dlgProgress.Progress();
          dlgProgress.Close();
        }
      }

      amazonWS.FindCoverArtProgress -= new AmazonWebservice.FindCoverArtProgressHandler(amazonWS_GetAlbumInfoProgress);
      amazonWS.FindCoverArtDone -= new AmazonWebservice.FindCoverArtDoneHandler(amazonWS_FindCoverArtDone);
    }

    private string GetCoverArtThumbPath(string artist, string album, string albumPath)
    {
      string thumbPath;

      // Look for the album cover thumbnail...
      thumbPath = Util.Utils.GetAlbumThumbName(artist, album);

      // If it doesn't exist look for it in the album folder
      if (!File.Exists(thumbPath))
      {
        thumbPath = String.Format(@"{0}\folder.jpg", Util.Utils.RemoveTrailingSlash(albumPath));
      }

      // If it still doesn't exist use the missing_coverart image
      if (!File.Exists(thumbPath))
      {
        thumbPath = GUIGraphicsContext.Skin + @"\media\missing_coverart.png";
      }

      return thumbPath;
    }

    private bool GetIsCompilationAlbum(string path, int numFilesToCheck)
    {
      MusicDatabase dbMusic = null;

      DirectoryInfo di = new DirectoryInfo(path);
      FileInfo[] files = null;

      if (di == null)
      {
        return false;
      }

      files = di.GetFiles();

      if (files == null || files.Length == 0)
      {
        return false;
      }

      dbMusic = MusicDatabase.Instance;

      string artist = string.Empty;
      bool IsCompilationAlbum = false;
      int difArtistCount = 0;

      int checkCount = 0;

      if (numFilesToCheck == -1)
      {
        checkCount = files.Length;
      }

      else
      {
        Math.Min(files.Length, numFilesToCheck);
      }

      MusicTag tag = null;
      Song song = null;

      for (int i = 0; i < checkCount; i++)
      {
        string curTrackPath = files[i].FullName;

        if (!Util.Utils.IsAudio(curTrackPath))
        {
          continue;
        }

        song = new Song();

        if (dbMusic.GetSongByFileName(curTrackPath, ref song))
        {
          if (artist == string.Empty)
          {
            artist = song.Artist;
            continue;
          }

          if (artist.ToLower().CompareTo(song.Artist.ToLower()) != 0)
          {
            difArtistCount++;
          }
        }

        else
        {
          tag = TagReader.TagReader.ReadTag(curTrackPath);

          if (tag != null)
          {
            if (artist == string.Empty)
            {
              artist = tag.Artist;
              continue;
            }

            if (artist.ToLower().CompareTo(tag.Artist.ToLower()) != 0)
            {
              difArtistCount++;
            }
          }
        }
      }

      if (difArtistCount > 0)
      {
        IsCompilationAlbum = true;
      }

      return IsCompilationAlbum;
    }

    private bool InternalGetAlbumCovers(string artist, string album, string filteredAlbumText)
    {
      if (amazonWS.AbortGrab)
      {
        return false;
      }

      amazonWS.ArtistName = artist;
      amazonWS.AlbumName = album;
      bool result = false;

      if (SearchMode == SearchDepthMode.Album)
      {
        GUIDialogProgress dlgProgress =
          (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
        if (dlgProgress != null)
        {
          dlgProgress.SetHeading(185);
          dlgProgress.SetLine(1, album);
          dlgProgress.SetLine(2, artist);
          dlgProgress.SetLine(3, filteredAlbumText);
          dlgProgress.SetPercentage(0);
          dlgProgress.Progress();
          dlgProgress.ShowProgressBar(false);

          GUIWindowManager.Process();
        }

        result = amazonWS.GetAlbumInfo();
      }

      else
      {
        result = amazonWS.GetAlbumInfo();
      }

      return result;
    }

    private void amazonWS_GetAlbumInfoProgress(AmazonWebservice aws, int progressPercent)
    {
      if (SearchMode == SearchDepthMode.Album)
      {
        GUIDialogProgress dlgProgress =
          (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);

        if (dlgProgress != null)
        {
          dlgProgress.ShowProgressBar(true);
          dlgProgress.SetPercentage(progressPercent);
          dlgProgress.Progress();
        }
      }

      else
      {
        // The GUICoverArtGrabberProgress window will manage showing cover art grabber progress
        if (FindCoverArtProgress != null)
        {
          FindCoverArtProgress(amazonWS, progressPercent);
        }
      }
    }

    private void amazonWS_FindCoverArtDone(AmazonWebservice aws, EventArgs e)
    {
      if (FindCoverArtDone != null)
      {
        FindCoverArtDone(amazonWS, e);
      }
    }

    private void UpdateAlbumCoverList()
    {
      if (listView == null)
      {
        return;
      }

      listView.Clear();

      Console.WriteLine("Current cover art image: " + imgCoverArt.FileName);
      imgCoverArt.SetFileName("");
      imgCoverArt.SetFileName(_ThumbPath);
      Console.WriteLine("New cover art image: " + imgCoverArt.FileName);

      if (amazonWS.HasAlbums)
      {
        for (int i = 0; i < amazonWS.AlbumCount; i++)
        {
          AlbumInfo albuminfo = (AlbumInfo) amazonWS.AlbumInfoList[i];
          GUIListItem item = new GUIListItem(albuminfo.Album);
          item.Label2 = albuminfo.Artist;
          item.IconImageBig = albuminfo.Image;
          item.IconImage = albuminfo.Image;
          listView.Add(item);
        }

        //listView.Focus = true;
        //btnCancel.Focus = false;

        listView.Focus = true;
        btnCancel.Focus = false;
        btnSkip.Focus = false;
      }

      else
      {
        listView.Focus = false;
        //btnCancel.Focus = true;
        //btnSkip.Focus = false;
        btnCancel.Focus = false;
        btnSkip.Focus = true;
      }
    }

    private bool GetProperAlbumName(string origAlbumName, ref string cleanAlbumName)
    {
      string temp = origAlbumName;
      int pos = temp.IndexOf("(");

      if (pos > 0)
      {
        cleanAlbumName = temp.Substring(0, pos).Trim();
      }

      return pos > 0;
    }

    private bool StripDiskNumberFromAlbumName(string origAlbumName, ref string cleanAlbumName)
    {
      string temp = origAlbumName;

      bool replaced = false;
      do
      {
        origAlbumName = origAlbumName.Replace("  ", " ");
        replaced = origAlbumName.CompareTo(temp) != 0;
        temp = origAlbumName;
      } while (replaced);

      string testSting = origAlbumName.ToLower();
      bool modified = false;
      int nPos = -1;

      for (int i = 0; i < DiskSetSubstrings.Length; i++)
      {
        nPos = -1;
        string s = DiskSetSubstrings[i];
        nPos = testSting.IndexOf(s);

        if (nPos > 0)
        {
          break;
        }
      }

      if (nPos != -1)
      {
        cleanAlbumName = origAlbumName.Substring(0, nPos).Trim();
        modified = true;
      }

      return modified;
    }

    private void Reset()
    {
      if (amazonWS == null)
      {
        return;
      }

      listView.Clear();
      listView.KeepAspectRatio = false;
      listView.Visible = true;
      lblNoMatches.Visible = false;

      string noMatches = GUILocalizeStrings.Get(4516);

      if (noMatches.Length == 0)
      {
        noMatches = "No cover art found";
      }

      lblNoMatches.Label = noMatches;
      lblAlbumName.Label = _Album;

      string coverArtText = GUILocalizeStrings.Get(4519);

      if (coverArtText.Length == 0)
      {
        coverArtText = "Current cover art";
      }

      lblCoverLabel.Label = coverArtText;

      if (IsCompilationAlbum && _Artist.Length == 0)
      {
        lblArtistName.Label = GUILocalizeStrings.Get(340);
      }

      else
      {
        lblArtistName.Label = _Artist;
      }

      lblReleaseYear.Label = "";

      if (!amazonWS.HasAlbums && !amazonWS.AbortGrab)
      {
        ShowNoMatchesText();
      }

      lblAlbumName.AllowScrolling = false;
      lblArtistName.AllowScrolling = false;
    }

    private void ShowNoMatchesText()
    {
      listView.Visible = false;
      lblNoMatches.Visible = true;
    }

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }

    #endregion
  }
}