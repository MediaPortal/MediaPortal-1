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
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Playlists;
using MediaPortal.Radio.Database;
using MediaPortal.Util;
//using MediaPortal.Music.Database;

namespace MediaPortal.GUI.Radio
{
  [PluginIcons("WindowPlugins.GUIRadio.Radio.gif", "WindowPlugins.GUIRadio.Radio_disabled.gif")]
  public class GUIRadio : GUIWindow, IComparer<GUIListItem>, ISetupForm, IShowPlugin
  {
    #region enums

    private enum SortMethod
    {
      Name = 0,
      Type = 1,
      Genre = 2,
      Bitrate = 3,
      Number = 4
    }

    private enum View : int
    {
      List = 0,
      Icons = 1,
      LargeIcons = 2,
    }

    #endregion

    #region Base variabeles

    private View currentView = View.List;
    private SortMethod currentSortMethod = SortMethod.Number;
    private bool sortAscending = true;
    private VirtualDirectory virtualDirectory = new VirtualDirectory();
    private DirectoryHistory directoryHistory = new DirectoryHistory();
    private string currentFolder = string.Empty;
    private string startFolder = string.Empty;
    private string currentRadioFolder = string.Empty;
    private int selectedItemIndex = -1;
    private PlayList currentPlayList = null;
    private PlayListPlayer playlistPlayer;
    //GUIRadioLastFM LastFMStation;

    //bool _useLastFM = false;

    #endregion

    #region SkinControls

    [SkinControl(50)] protected GUIFacadeControl facadeView = null;
    [SkinControl(2)] protected GUIButtonControl btnViewAs = null;
    [SkinControl(3)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(6)] protected GUIButtonControl btnPrevious = null;
    [SkinControl(7)] protected GUIButtonControl btnNext = null;

    #endregion

    public GUIRadio()
    {
      GetID = (int) Window.WINDOW_RADIO;

      playlistPlayer = PlayListPlayer.SingletonPlayer;
      LoadSettings();
    }


    public override bool Init()
    {
      currentFolder = string.Empty;
      bool bResult = Load(GUIGraphicsContext.Skin + @"\MyRadio.xml");

      return bResult;
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        currentRadioFolder = xmlreader.GetValueAsString("radio", "folder", string.Empty);

        string tmpLine = string.Empty;
        tmpLine = (string) xmlreader.GetValue("myradio", "viewby");
        if (tmpLine != null)
        {
          if (tmpLine == "list")
          {
            currentView = View.List;
          }
          else if (tmpLine == "icons")
          {
            currentView = View.Icons;
          }
          else if (tmpLine == "largeicons")
          {
            currentView = View.LargeIcons;
          }
        }

        tmpLine = (string) xmlreader.GetValue("myradio", "sort");
        if (tmpLine != null)
        {
          if (tmpLine == "name")
          {
            currentSortMethod = SortMethod.Name;
          }
          else if (tmpLine == "type")
          {
            currentSortMethod = SortMethod.Type;
          }
          else if (tmpLine == "genre")
          {
            currentSortMethod = SortMethod.Genre;
          }
          else if (tmpLine == "bitrate")
          {
            currentSortMethod = SortMethod.Bitrate;
          }
          else if (tmpLine == "number")
          {
            currentSortMethod = SortMethod.Number;
          }
        }

        sortAscending = xmlreader.GetValueAsBool("myradio", "sortascending", true);

        //_useLastFM = xmlreader.GetValueAsBool("plugins", "Audioscrobbler", false);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        switch (currentView)
        {
          case View.List:
            xmlwriter.SetValue("myradio", "viewby", "list");
            break;
          case View.Icons:
            xmlwriter.SetValue("myradio", "viewby", "icons");
            break;
          case View.LargeIcons:
            xmlwriter.SetValue("myradio", "viewby", "largeicons");
            break;
        }

        switch (currentSortMethod)
        {
          case SortMethod.Name:
            xmlwriter.SetValue("myradio", "sort", "name");
            break;
          case SortMethod.Type:
            xmlwriter.SetValue("myradio", "sort", "type");
            break;
          case SortMethod.Genre:
            xmlwriter.SetValue("myradio", "sort", "genre");
            break;
          case SortMethod.Bitrate:
            xmlwriter.SetValue("myradio", "sort", "bitrate");
            break;
          case SortMethod.Number:
            xmlwriter.SetValue("myradio", "sort", "number");
            break;
        }

        xmlwriter.SetValueAsBool("myradio", "sortascending", sortAscending);
      }
    }

    #endregion

    #region BaseWindow Members

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (facadeView.Focus)
        {
          GUIListItem item = facadeView[0];
          if (item != null)
          {
            if (item.IsFolder && item.Label == "..")
            {
              LoadDirectory(item.Path);
              return;
            }
          }
        }
      }

      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = facadeView[0];
        if (item != null)
        {
          if (item.IsFolder && item.Label == "..")
          {
            LoadDirectory(item.Path);
          }
        }
        return;
      }
      if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_MUSIC_PLAYLIST);
        return;
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      LoadSettings();
      switch (currentSortMethod)
      {
        case SortMethod.Name:
          btnSortBy.SelectedItem = 0;
          break;
        case SortMethod.Type:
          btnSortBy.SelectedItem = 1;
          break;
        case SortMethod.Genre:
          btnSortBy.SelectedItem = 2;
          break;
        case SortMethod.Bitrate:
          btnSortBy.SelectedItem = 3;
          break;
        case SortMethod.Number:
          btnSortBy.SelectedItem = 4;
          break;
      }


      currentPlayList = null;
      virtualDirectory = new VirtualDirectory();
      Share share = new Share("default", currentRadioFolder);
      share.Default = true;
      virtualDirectory.Add(share);
      virtualDirectory.AddExtension(".pls");
      virtualDirectory.AddExtension(".asx");

      SelectCurrentItem();
      UpdateButtonStates();
      LoadDirectory(currentFolder);
      btnSortBy.SortChanged += new SortEventHandler(SortChanged);


      base.OnPageLoad();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      selectedItemIndex = facadeView.SelectedListItemIndex;
      SaveSettings();
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      if (control == btnViewAs)
      {
        bool shouldContinue = false;
        do
        {
          shouldContinue = false;
          switch (currentView)
          {
            case View.List:
              currentView = View.Icons;
              if (facadeView.ThumbnailView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
              }
              break;

            case View.Icons:
              currentView = View.LargeIcons;
              if (facadeView.ThumbnailView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
              }
              break;

            case View.LargeIcons:
              currentView = View.List;
              if (facadeView.ListView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.List;
              }
              break;
          }
        } while (shouldContinue);

        SelectCurrentItem();
        GUIControl.FocusControl(GetID, controlId);
        return;
      } //if (control == btnViewAs)

      if (control == btnSortBy)
      {
        OnShowSort();
      }

      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        int itemIndex = (int) msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(itemIndex);
        }
      }
    }


    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
          //case GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED:
          //  if (_useLastFM)
          //  {
          //    if ((int)LastFMStation.CurrentStreamState > 1)
          //      LastFMStation.CurrentStreamState = StreamPlaybackState.initialized;
          //  }
          //  break;

          //case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
          //  goto case GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED;

        case GUIMessage.MessageType.GUI_MSG_PLAY_RADIO_STATION:
          if (message.Label.Length == 0)
          {
            return true;
          }
          playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
          playlistPlayer.Reset();

          ArrayList stations = new ArrayList();
          RadioDatabase.GetStations(ref stations);
          foreach (RadioStation station in stations)
          {
            PlayListItem playlistItem = new PlayListItem();
            if (station.URL == string.Empty)
            {
              playlistItem.Type = PlayListItem.PlayListItemType.Radio;
            }
            else
            {
              playlistItem.Type = PlayListItem.PlayListItemType.AudioStream;
            }
            playlistItem.FileName = GetPlayPath(station);
            playlistItem.Description = station.Name;
            playlistItem.Duration = 0;
            playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
          }
          playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_TEMP;
          foreach (RadioStation station in stations)
          {
            if (station.Name.Equals(message.Label))
            {
              playlistPlayer.Play(GetPlayPath(station));
              return true;
            }
          }
          break;
      }
      return base.OnMessage(message);
    }


    //bool ViewByIcon
    //{
    //  get
    //  {
    //    if (currentView != View.List) return true;
    //    return false;
    //  }
    //}

    //bool ViewByLargeIcon
    //{
    //  get
    //  {
    //    if (currentView == View.LargeIcons) return true;
    //    return false;
    //  }
    //}

    //GUIListItem GetSelectedItem()
    //{
    //  if (ViewByIcon)
    //    return thumbnailView.SelectedListItem;
    //  else
    //    return listView.SelectedListItem;
    //}

    //GUIListItem GetItem(int itemIndex)
    //{
    //  if (ViewByIcon)
    //  {
    //    if (itemIndex >= thumbnailView.Count) return null;
    //    return thumbnailView[itemIndex];
    //  }
    //  else
    //  {
    //    if (itemIndex >= listView.Count) return null;
    //    return listView[itemIndex];
    //  }
    //}

    //int GetSelectedItemNo()
    //{
    //  if (ViewByIcon)
    //    return thumbnailView.SelectedListItemIndex;
    //  else
    //    return listView.SelectedListItemIndex;
    //}

    //int GetItemCount()
    //{
    //  if (ViewByIcon)
    //    return thumbnailView.Count;
    //  else
    //    return listView.Count;
    //}

    private void UpdateButtonStates()
    {
      facadeView.IsVisible = false;
      facadeView.IsVisible = true;
      GUIControl.FocusControl(GetID, facadeView.GetID);

      string strLine = string.Empty;
      View view = currentView;
      switch (view)
      {
        case View.List:
          strLine = GUILocalizeStrings.Get(101);
          break;
        case View.Icons:
          strLine = GUILocalizeStrings.Get(100);
          break;
        case View.LargeIcons:
          strLine = GUILocalizeStrings.Get(417);
          break;
      }
      if (btnViewAs != null)
      {
        btnViewAs.Label = strLine;
      }

      switch (currentSortMethod)
      {
        case SortMethod.Name:
          strLine = GUILocalizeStrings.Get(103);
          break;
        case SortMethod.Type:
          strLine = GUILocalizeStrings.Get(668);
          break;
        case SortMethod.Genre:
          strLine = GUILocalizeStrings.Get(669);
          break;
        case SortMethod.Bitrate:
          strLine = GUILocalizeStrings.Get(670);
          break;
        case SortMethod.Number:
          strLine = GUILocalizeStrings.Get(620);
          break;
      }
      if (btnSortBy != null)
      {
        btnSortBy.Label = strLine;
        btnSortBy.IsAscending = sortAscending;
      }
    }

    private void SelectCurrentItem()
    {
      int iItem = facadeView.SelectedListItemIndex;
      if (iItem > -1)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
      }
      UpdateButtonStates();
    }

    private void SwitchView()
    {
      switch (currentView)
      {
        case View.List:
          facadeView.View = GUIFacadeControl.ViewMode.List;
          break;
        case View.Icons:
          facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
          break;
        case View.LargeIcons:
          facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
          break;
      }

      UpdateButtonStates(); // Ensure "View: xxxx" button label is updated to suit
    }

    //void ShowThumbPanel()
    //{
    //  int itemIndex = facadeView.SelectedListItemIndex;
    //  thumbnailView.ShowBigIcons(ViewByLargeIcon);
    //  if (itemIndex > -1)
    //  {
    //    GUIControl.SelectItemControl(GetID, listView.GetID, itemIndex);
    //    GUIControl.SelectItemControl(GetID, thumbnailView.GetID, itemIndex);
    //  }
    //  UpdateButtons();
    //}

    private void LoadDirectory(string strNewDirectory)
    {
      GUIWaitCursor.Show();
      GUIListItem SelectedItem = facadeView.SelectedListItem;
      if (SelectedItem != null)
      {
        if (SelectedItem.IsFolder && SelectedItem.Label != "..")
        {
          directoryHistory.Set(SelectedItem.Label, currentFolder);
        }
      }
      currentFolder = strNewDirectory;
      GUIControl.ClearControl(GetID, facadeView.GetID);

      string objectCount = String.Empty;
      int totalItems = 0;

      if (currentPlayList != null)
      {
        GUIListItem item = new GUIListItem();
        item.Label = "..";
        item.Path = currentFolder;
        item.IsFolder = true;
        item.MusicTag = null;
        item.ThumbnailImage = string.Empty;
        Util.Utils.SetDefaultIcons(item);
        facadeView.Add(item);

        for (int i = 0; i < currentPlayList.Count; ++i)
        {
          item = new GUIListItem();
          item.Label = currentPlayList[i].Description;
          item.Path = currentPlayList[i].FileName;
          item.IsFolder = false;
          item.MusicTag = null;
          item.ThumbnailImage = string.Empty;
          item.IconImageBig = "DefaultMyradioStreamBig.png";
          item.IconImage = "DefaultMyradioStream.png";
          string thumbnail = Util.Utils.GetCoverArt(Thumbs.Radio, item.Label);
          if (File.Exists(thumbnail))
          {
            item.IconImageBig = thumbnail;
            item.IconImage = thumbnail;
            item.ThumbnailImage = thumbnail;
          }
          facadeView.Add(item);
          totalItems++;
        }
      }
      else
      {
        if (currentFolder.Length == 0 || currentFolder.Equals(currentRadioFolder))
        {
          ArrayList stations = new ArrayList();
          RadioDatabase.GetStations(ref stations);
          foreach (RadioStation station in stations)
          {
            GUIListItem item = new GUIListItem();
            item.Label = station.Name;
            item.IsFolder = false;
            item.MusicTag = station;
            if (station.URL.Length > 5)
            {
              item.IconImageBig = "DefaultMyradioStreamBig.png";
              item.IconImage = "DefaultMyradioStream.png";
              string thumbnail = Util.Utils.GetCoverArt(Thumbs.Radio, station.Name);
              if (File.Exists(thumbnail))
              {
                item.IconImageBig = thumbnail;
                item.IconImage = thumbnail;
                item.ThumbnailImage = thumbnail;
              }
            }
            else
            {
              if (station.Scrambled)
              {
                item.IconImageBig = "DefaultMyradioBigLocked.png";
                item.IconImage = "DefaultMyradioLocked.png";
              }
              else
              {
                item.IconImageBig = "DefaultMyradioBig.png";
                item.IconImage = "DefaultMyradio.png";
              }
              string thumbnail = Util.Utils.GetCoverArt(Thumbs.Radio, station.Name);
              if (File.Exists(thumbnail))
              {
                item.IconImageBig = thumbnail;
                item.IconImage = thumbnail;
                item.ThumbnailImage = thumbnail;
              }
              //if (item.ThumbnailImage==string.Empty)
              //  item.ThumbnailImage="DefaultMyradioBig.png";
            }
            facadeView.Add(item);
            totalItems++;
          }
        }

        if (currentRadioFolder.Length != 0)
        {
          string folerName = currentFolder;
          if (folerName.Length == 0)
          {
            folerName = currentRadioFolder;
          }
          List<GUIListItem> items = virtualDirectory.GetDirectoryExt(folerName);
          foreach (GUIListItem item in items)
          {
            if (!item.IsFolder)
            {
              item.MusicTag = null;
              //item.ThumbnailImage="DefaultMyradioStream.png";
              item.IconImageBig = "DefaultMyradioStreamBig.png";
              item.IconImage = "DefaultMyradioStream.png";
            }
            else
            {
              if (item.Label.Equals(".."))
              {
                if (currentFolder.Length == 0 || currentFolder.Equals(currentRadioFolder))
                {
                  continue;
                }
              }
            }
            facadeView.Add(item);
            totalItems++;
          }
        }
      }

      SwitchView();
      OnSort();

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(totalItems));

      SelectCurrentItem();

      //set selected item
      if (selectedItemIndex >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, selectedItemIndex);
      }

      GUIWaitCursor.Hide();
    }

    #endregion

    private void SetLabels()
    {
      SortMethod method = currentSortMethod;

      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item = facadeView[i];
        if (item.MusicTag != null)
        {
          RadioStation station = (RadioStation) item.MusicTag;
          if (method == SortMethod.Bitrate)
          {
            if (station.BitRate > 0)
            {
              item.Label2 = station.BitRate.ToString();
            }
            else
            {
              double frequency = station.Frequency;
              frequency /= 1000000d;
              item.Label2 = String.Format("{0:###.##} MHz.", frequency);
            }
          }
          else
          {
            if (station.Genre == Strings.Unknown && station.Frequency > 0)
            {
              double frequency = station.Frequency;
              frequency /= 1000000d;
              if (frequency > 80 && frequency < 120)
              {
                item.Label2 = String.Format("{0:###.##} MHz.", frequency);
              }
              else
              {
                item.Label2 = station.Genre;
              }
            }
            else
            {
              item.Label2 = station.Genre;
            }
          }
        }
      }
    }

    #region Sort Members

    private void OnSort()
    {
      SetLabels();
      facadeView.Sort(this);
      UpdateButtonStates();
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2)
      {
        return 0;
      }
      if (item1 == null)
      {
        return -1;
      }
      if (item2 == null)
      {
        return -1;
      }
      if (item1.IsFolder && item1.Label == "..")
      {
        return -1;
      }
      if (item2.IsFolder && item2.Label == "..")
      {
        return -1;
      }
      if (item1.IsFolder && !item2.IsFolder)
      {
        return -1;
      }
      else if (!item1.IsFolder && item2.IsFolder)
      {
        return 1;
      }


      SortMethod method = currentSortMethod;
      bool bAscending = sortAscending;
      RadioStation station1 = item1.MusicTag as RadioStation;
      RadioStation station2 = item2.MusicTag as RadioStation;
      switch (method)
      {
        case SortMethod.Name:
          if (bAscending)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }

        case SortMethod.Type:
          string strURL1 = string.Empty;
          string strURL2 = string.Empty;
          if (station1 != null)
          {
            strURL1 = station1.URL;
          }
          else
          {
            if (item1.IconImage.ToLower().Equals("defaultmyradiostream.png"))
            {
              strURL1 = "1";
            }
          }

          if (station2 != null)
          {
            strURL2 = station2.URL;
          }
          else
          {
            if (item2.IconImage.ToLower().Equals("defaultmyradiostream.png"))
            {
              strURL2 = "1";
            }
          }

          if (strURL1.Equals(strURL2))
          {
            if (bAscending)
            {
              return String.Compare(item1.Label, item2.Label, true);
            }
            else
            {
              return String.Compare(item2.Label, item1.Label, true);
            }
          }
          if (bAscending)
          {
            if (strURL1.Length > 0)
            {
              return 1;
            }
            else
            {
              return -1;
            }
          }
          else
          {
            if (strURL1.Length > 0)
            {
              return -1;
            }
            else
            {
              return 1;
            }
          }
          //break;

        case SortMethod.Genre:
          if (station1 != null && station2 != null)
          {
            if (station1.Genre.Equals(station2.Genre))
            {
              goto case SortMethod.Bitrate;
            }
            if (bAscending)
            {
              return String.Compare(station1.Genre, station2.Genre, true);
            }
            else
            {
              return String.Compare(station2.Genre, station1.Genre, true);
            }
          }
          else
          {
            return 0;
          }
          //break;

        case SortMethod.Number:
          if (station1 != null && station2 != null)
          {
            if (bAscending)
            {
              if (station1.Sort > station2.Sort)
              {
                return 1;
              }
              else
              {
                return -1;
              }
            }
            else
            {
              if (station2.Sort > station1.Sort)
              {
                return 1;
              }
              else
              {
                return -1;
              }
            }
          }

          if (station1 != null)
          {
            return -1;
          }
          if (station2 != null)
          {
            return 1;
          }
          return 0;
          //break;
        case SortMethod.Bitrate:
          if (station1 != null && station2 != null)
          {
            if (bAscending)
            {
              if (station1.BitRate > station2.BitRate)
              {
                return 1;
              }
              else
              {
                return -1;
              }
            }
            else
            {
              if (station2.BitRate > station1.BitRate)
              {
                return 1;
              }
              else
              {
                return -1;
              }
            }
          }
          return 0;
      }
      return 0;
    }

    #endregion

    private void OnClick(int itemIndex)
    {
      GUIListItem item = facadeView.SelectedListItem;
      if (item == null)
      {
        return;
      }
      if (item.IsFolder)
      {
        if (currentPlayList != null)
        {
          currentPlayList = null;
        }
        selectedItemIndex = -1;
        LoadDirectory(item.Path);
      }
      else
      {
        Play(item);
        GUIPropertyManager.SetProperty("#selecteditem", item.Label);
      }
    }

    private void OnShowSort()
    {
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(495); // Sort options

      dlg.AddLocalizedString(103); // name
      dlg.AddLocalizedString(668); // Type
      dlg.AddLocalizedString(669); // genre
      dlg.AddLocalizedString(670); // bitrate
      dlg.AddLocalizedString(620); // number

      dlg.SelectedLabel = (int) currentSortMethod;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 103:
          currentSortMethod = SortMethod.Name;
          break;
        case 668:
          currentSortMethod = SortMethod.Type;
          break;
        case 669:
          currentSortMethod = SortMethod.Genre;
          break;
        case 670:
          currentSortMethod = SortMethod.Bitrate;
          break;
        case 620:
          currentSortMethod = SortMethod.Number;
          break;
        default:
          currentSortMethod = SortMethod.Name;
          break;
      }

      OnSort();
      if (btnSortBy != null)
      {
        GUIControl.FocusControl(GetID, btnSortBy.GetID);
      }
    }

    private bool IsUrl(string fileName)
    {
      if (fileName.ToLower().StartsWith("http:") || fileName.ToLower().StartsWith("https:") ||
          fileName.ToLower().StartsWith("mms:") || fileName.ToLower().StartsWith("rtsp:"))
      {
        return true;
      }
      return false;
    }

    private void FillPlayList()
    {
      playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
      playlistPlayer.Reset();

      // are we looking @ a playlist
      if (currentPlayList != null)
      {
        //yes, then add current playlist to playlist player
        for (int i = 0; i < currentPlayList.Count; ++i)
        {
          PlayListItem playlistItem = new PlayListItem();
          // If we got a Url, we should set the type to AudioStream
          if (IsUrl(currentPlayList[i].FileName))
          {
            playlistItem.Type = PlayListItem.PlayListItemType.AudioStream;
          }
          else
          {
            playlistItem.Type = currentPlayList[i].Type;
          }

          playlistItem.FileName = currentPlayList[i].FileName;
          playlistItem.Description = currentPlayList[i].Description;
          playlistItem.Duration = currentPlayList[i].Duration;
          playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
        }
      }
      else
      {
        //add current directory to playlist player
        for (int i = 0; i < facadeView.Count; ++i)
        {
          GUIListItem item = facadeView[i];
          if (item.IsFolder)
          {
            continue;
          }

          // We could get a Playlist as part of a URL
          string strPath = item.Path;
          if (strPath == String.Empty)
          {
            RadioStation station = item.MusicTag as RadioStation;
            strPath = station.URL;
          }

          // if item is a playlist
          if (Util.Utils.IsPlayList(strPath))
          {
            // then load the playlist
            PlayList playlist = new PlayList();
            IPlayListIO loader = PlayListFactory.CreateIO(strPath);
            loader.Load(playlist, strPath, item.Label);

            // and if it contains any items
            if (playlist.Count > 0)
            {
              // then add the 1st item to the playlist player
              PlayListItem playlistItem = new PlayListItem();
              playlistItem.FileName = playlist[0].FileName;
              playlistItem.Description = playlist[0].Description;
              playlistItem.Duration = playlist[0].Duration;
              // If we got a Url, we should set the type to AudioStream
              if (IsUrl(playlist[0].FileName))
              {
                playlistItem.Type = PlayListItem.PlayListItemType.AudioStream;
              }
              else
              {
                if (currentPlayList != null)
                {
                  playlistItem.Type = currentPlayList[0].Type;
                }
                else
                {
                  playlistItem.Type = PlayListItem.PlayListItemType.Radio;
                }
              }
              playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
            }
          }
          else
          {
            // item is just a normal file like .asx, .pls
            // or a radio station from the setup.
            RadioStation station = item.MusicTag as RadioStation;
            PlayListItem playlistItem = new PlayListItem();
            if (station != null)
            {
              playlistItem.FileName = GetPlayPath(station);
              if (station.URL == string.Empty)
              {
                playlistItem.Type = PlayListItem.PlayListItemType.Radio;
              }
              else
              {
                playlistItem.Type = PlayListItem.PlayListItemType.AudioStream;
              }
            }
            else
            {
              playlistItem.Type = PlayListItem.PlayListItemType.AudioStream;
              playlistItem.FileName = item.Path;
            }
            playlistItem.Description = item.Label;
            playlistItem.Duration = 0;
            playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
          }
        }
      }
      playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_TEMP;
    }

    private void Play(GUIListItem item)
    {
      if (Util.Utils.IsPlayList(item.Path))
      {
        currentPlayList = new PlayList();
        IPlayListIO loader = PlayListFactory.CreateIO(item.Path);
        loader.Load(currentPlayList, item.Path);
        if (currentPlayList.Count == 1)
        {
          // add current directory 2 playlist and play this item
          string strURL = currentPlayList[0].FileName;
          currentPlayList = null;
          FillPlayList();
          playlistPlayer.Play(strURL);
          return;
        }
        if (currentPlayList.Count == 0)
        {
          currentPlayList = null;
        }
        LoadDirectory(currentFolder);
      }
      else
      {
        // We have the Station Name in there to retrieve the correct Coverart for the station in the Vis Window
        GUIPropertyManager.RemovePlayerProperties();
        GUIPropertyManager.SetProperty("#Play.Current.ArtistThumb", item.Label);
        GUIPropertyManager.SetProperty("#Play.Current.Album", item.Label);
        if (currentPlayList != null)
        {
          // add current playlist->playlist and play selected item
          string strURL = item.Path;
          FillPlayList();
          playlistPlayer.Play(strURL);
          return;
        }

        // add current directory 2 playlist and play this item
        RadioStation station = item.MusicTag as RadioStation;
        FillPlayList();

        PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP);
        for (int i = 0; i < playlist.Count; ++i)
        {
          PlayListItem playItem = playlist[i];
          if (playItem.Description.Equals(item.Label))
          {
            playlistPlayer.Play(i);
            break;
          }
        }
      }
    }

    private string GetPlayPath(RadioStation station)
    {
      if (station.URL.Length > 5)
      {
        return station.URL;
      }
      else
      {
        string fileName = String.Format("{0}.radio", station.Frequency);
        return fileName;
      }
    }

    private void SortChanged(object sender, SortEventArgs e)
    {
      sortAscending = e.Order != SortOrder.Descending;

      OnSort();
      UpdateButtonStates();

      GUIControl.FocusControl(GetID, ((GUIControl) sender).GetID);
    }

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string PluginName()
    {
      return "Radio";
    }

    public bool HasSetup()
    {
      return false;
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(665);
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = @"hover_my radio.png";
      return true;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string Description()
    {
      return "Listen to analog, DVB and internet radio";
    }

    public void ShowPlugin()
    {
      // TODO:  Add GUIRadio.ShowPlugin implementation
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;
    }

    #endregion
  }
}