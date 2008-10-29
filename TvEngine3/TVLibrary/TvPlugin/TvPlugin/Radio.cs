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
using System.Net;
using System.Globalization;

using MediaPortal;
using MediaPortal.Configuration;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Playlists;
using MediaPortal.Radio.Database;

using TvDatabase;
using TvControl;
using System.Threading;
using System.Windows.Forms;

using Gentle.Common;
using Gentle.Framework;

namespace TvPlugin
{
  [PluginIcons("TvPlugin.Radio.gif", "TvPlugin.Radio_disabled.gif")]
  public class Radio : GUIWindow, IComparer<GUIListItem>, ISetupForm, IShowPlugin
  {
    #region enums

    enum SortMethod
    {
      Name = 0,
      Type = 1,
      Genre = 2,
      Bitrate = 3,
      Number = 4
    }

    enum View : int
    {
      List = 0,
      Icons = 1,
      LargeIcons = 2,
    }

    #endregion

    #region Base variabeles

    View currentView = View.List;
    SortMethod currentSortMethod = SortMethod.Number;
    bool sortAscending = true;
    DirectoryHistory directoryHistory = new DirectoryHistory();
    string currentFolder = null;
    string lastFolder = "..";
    int selectedItemIndex = -1;
    bool showAllChannelsGroup = true;
    string rootGroup = "(none)";
    public static RadioChannelGroup selectedGroup = null;

    #endregion

    #region SkinControls
    
    [SkinControlAttribute(50)]   protected GUIFacadeControl facadeView = null;
    [SkinControlAttribute(2)]    protected GUIButtonControl btnViewAs = null;
    [SkinControlAttribute(3)]    protected GUISortButtonControl btnSortBy = null;
    [SkinControlAttribute(6)]    protected GUIButtonControl btnPrevious = null;
    [SkinControlAttribute(7)]    protected GUIButtonControl btnNext = null;

    #endregion

    //heartbeat related stuff
    private const int HEARTBEAT_INTERVAL = 5; //seconds
    private Thread heartBeatTransmitterThread = null;    

    public Radio()
    {
      GetID = (int)GUIWindow.Window.WINDOW_RADIO;
      LoadSettings();
      startHeartBeatThread();

      Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(g_Player_PlayBackStopped);
    }

    void Application_ApplicationExit(object sender, EventArgs e)
    {
      try
      {
        if (TVHome.Card.IsTimeShifting)
        {
          if (!TVHome.Card.IsRecording)
          {
            TVHome.Card.User.Name = new User().Name;
            TVHome.Card.StopTimeShifting();
          }
        }
        stopHeartBeatThread();
      }
      catch (Exception)
      {
      }
    }

    private void HeartBeatTransmitter()
    {

      // when debugging we want to disable heartbeats

#if DEBUG
      return;
#endif
      
      while (true)
      {
        if (TVHome.Connected && TVHome.Card.IsTimeShifting)
        {
          // send heartbeat to tv server each 5 sec.
          // this way we signal to the server that we are alive thus avoid being kicked.
          // Log.Debug("Radio: sending HeartBeat signal to server.");
          try
          {
            RemoteControl.Instance.HeartBeat(TVHome.Card.User);
          }
          catch (Exception e)
          {
            Log.Error("Radio: failed sending HeartBeat signal to server. ({0})", e.Message);
          }
        }
        Thread.Sleep(HEARTBEAT_INTERVAL * 1000); //sleep for 5 secs. before sending heartbeat again
      }
    }

    private void startHeartBeatThread()
    {
      // setup heartbeat transmitter thread.						
      // thread already running, then leave it.
      if (heartBeatTransmitterThread != null)
      {
        if (heartBeatTransmitterThread.IsAlive)
        {
          return;
        }
      }
      Log.Debug("Radio: HeartBeat Transmitter started.");
      heartBeatTransmitterThread = new Thread(HeartBeatTransmitter);
      //GEMX 01.04.08: Better debuggin plus IsBackground=true fixes MP not closing correctly if TvService is not running
      heartBeatTransmitterThread.IsBackground = true;
      heartBeatTransmitterThread.Name="TvClient-Radio: HeartBeat transmitter thread";
      heartBeatTransmitterThread.Start();
    }

    private void stopHeartBeatThread()
    {
      if (heartBeatTransmitterThread != null)
      {
        if (heartBeatTransmitterThread.IsAlive)
        {
          Log.Debug("Radio: HeartBeat Transmitter stopped.");
          heartBeatTransmitterThread.Abort();
        }
      }
    }

    void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type == g_Player.MediaType.Radio)
      {
        VirtualCard card = TVHome.Card;
        if (card == null)
          return;

        if (card.IsTimeShifting && card.Channel != null)
        {
          if (card.Channel.IsRadio)
          {
            card.StopTimeShifting();
          }
        }
      }
    }

    public override void OnAdded()
    {
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_RADIO, this);
      Restore();
      PreInit();
      ResetAllControls();
    }

    public override bool Init()
    {
      currentFolder = null;
      bool bResult = Load(GUIGraphicsContext.Skin + @"\MyRadio.xml");
      return bResult;
    }


    #region Serialisation
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string tmpLine = String.Empty;
        tmpLine = (string)xmlreader.GetValue("myradio", "viewby");
        if (tmpLine != null)
        {
          if (tmpLine == "list") currentView = View.List;
          else if (tmpLine == "icons") currentView = View.Icons;
          else if (tmpLine == "largeicons") currentView = View.LargeIcons;
        }

        tmpLine = (string)xmlreader.GetValue("myradio", "sort");
        if (tmpLine != null)
        {
          if (tmpLine == "name") currentSortMethod = SortMethod.Name;
          else if (tmpLine == "type") currentSortMethod = SortMethod.Type;
          else if (tmpLine == "genre") currentSortMethod = SortMethod.Genre;
          else if (tmpLine == "bitrate") currentSortMethod = SortMethod.Bitrate;
          else if (tmpLine == "number") currentSortMethod = SortMethod.Number;
        }

        sortAscending = xmlreader.GetValueAsBool("myradio", "sortascending", true);
        if (xmlreader.GetValueAsBool("myradio", "rememberlastgroup", true))
          currentFolder = xmlreader.GetValueAsString("myradio", "lastgroup", null);
        showAllChannelsGroup = xmlreader.GetValueAsBool("myradio", "showallchannelsgroup", true);
        rootGroup = xmlreader.GetValueAsString("myradio", "rootgroup", "(none)");
      }
    }

    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
        xmlwriter.SetValue("myradio", "lastgroup", lastFolder);

        if (TVHome.Navigator.Channel.IsRadio)
        {
          xmlwriter.SetValue("myradio", "channel", TVHome.Navigator.Channel.DisplayName);
        }        
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
              LoadDirectory(null);
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
            LoadDirectory(null);
          }
        }
        return;
      }
      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIMessage msgStopRecorder = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msgStopRecorder);
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

      SelectCurrentItem();
      LoadDirectory(currentFolder);
      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      selectedItemIndex = facadeView.SelectedListItemIndex;
      SaveSettings();
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
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
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
              break;

            case View.Icons:
              currentView = View.LargeIcons;
              if (facadeView.ThumbnailView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
              break;

            case View.LargeIcons:
              currentView = View.List;
              if (facadeView.ListView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.List;
              break;
          }
        } while (shouldContinue);

        SelectCurrentItem();
        GUIControl.FocusControl(GetID, controlId);
        return;
      }//if (control == btnViewAs)

      if (control == btnSortBy)
      {
        OnShowSort();
      }

      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        int itemIndex = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(itemIndex);
        }
      }
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

    void UpdateButtonStates()
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

    void SelectCurrentItem()
    {
      if (facadeView == null) return;
      int iItem = facadeView.SelectedListItemIndex;
      if (iItem > -1)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
      }
      UpdateButtonStates();
    }

    void SwitchView()
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

    void LoadDirectory(string strNewDirectory)
    {
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
      if (currentFolder==null || currentFolder=="..")
      {
        IList groups=RadioChannelGroup.ListAll();
        foreach (RadioChannelGroup group in groups)
        {
          if (!showAllChannelsGroup)
          {
            if (group.GroupName == "All Channels")
              continue;
          }
          if (group.GroupName == rootGroup)
            continue;
          GUIListItem item=new GUIListItem();
          item.Label = group.GroupName;
          item.IsFolder = true;
          item.MusicTag = group;
          item.ThumbnailImage = String.Empty;
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          string thumbnail = MediaPortal.Util.Utils.GetCoverArt(Thumbs.Radio, "folder_" + group.GroupName);
          if (System.IO.File.Exists(thumbnail))
          {
            item.IconImageBig = thumbnail;
            item.IconImage = thumbnail;
            item.ThumbnailImage = thumbnail;
          }
          facadeView.Add(item);
          totalItems++;
        }
        if (rootGroup != "(none)")
        {
          TvBusinessLayer layer = new TvBusinessLayer();
          RadioChannelGroup root = layer.GetRadioChannelGroupByName(rootGroup);
          if (root != null)
          {
            IList maps = root.ReferringRadioGroupMap();
            foreach (RadioGroupMap map in maps)
            {
              Channel channel = map.ReferencedChannel();
              GUIListItem item = new GUIListItem();
              item.Label = channel.DisplayName;
              item.IsFolder = false;
              item.MusicTag = channel;
              if (channel.IsWebstream())
              {
                item.IconImageBig = "DefaultMyradioStreamBig.png";
                item.IconImage = "DefaultMyradioStream.png";
              }
              else
              {
                item.IconImageBig = "DefaultMyradioBig.png";
                item.IconImage = "DefaultMyradio.png";
              }
              string thumbnail = MediaPortal.Util.Utils.GetCoverArt(Thumbs.Radio, channel.DisplayName);
              if (System.IO.File.Exists(thumbnail))
              {
                item.IconImageBig = thumbnail;
                item.IconImage = thumbnail;
                item.ThumbnailImage = thumbnail;
              }
              facadeView.Add(item);
              totalItems++;
            }
          }
        }
        selectedGroup = null;
      }
      else
      {
        TvBusinessLayer layer=new TvBusinessLayer();
        RadioChannelGroup group=layer.GetRadioChannelGroupByName(currentFolder);
        if (group==null)
          return;
        selectedGroup = group;
        lastFolder = currentFolder;
        GUIListItem item=new GUIListItem();
        item.Label = "..";
        item.IsFolder = true;
        item.MusicTag = null;
        item.ThumbnailImage = String.Empty;
        MediaPortal.Util.Utils.SetDefaultIcons(item);
        facadeView.Add(item);
        IList maps=group.ReferringRadioGroupMap();
        foreach (RadioGroupMap map in maps)
        {
          Channel channel=map.ReferencedChannel();
          item=new GUIListItem();
          item.Label = channel.DisplayName;
          item.IsFolder = false;
          item.MusicTag = channel;
          if (channel.IsWebstream())
          {
            item.IconImageBig = "DefaultMyradioStreamBig.png";
            item.IconImage = "DefaultMyradioStream.png";
          }
          else
          {
            item.IconImageBig = "DefaultMyradioBig.png";
            item.IconImage = "DefaultMyradio.png";
          }
          string thumbnail = MediaPortal.Util.Utils.GetCoverArt(Thumbs.Radio, channel.DisplayName);
          if (System.IO.File.Exists(thumbnail))
          {
            item.IconImageBig = thumbnail;
            item.IconImage = thumbnail;
            item.ThumbnailImage = thumbnail;
          }
          facadeView.Add(item);
          totalItems++;
        }
      }

      SwitchView();
      OnSort();

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", MediaPortal.Util.Utils.GetObjectCountLabel(totalItems));

      SelectCurrentItem();
      SetLabels();

      //set selected item
      if (selectedItemIndex >= 0)
        GUIControl.SelectItemControl(GetID, facadeView.GetID, selectedItemIndex);
    }
    #endregion

    void SetLabels()
    {
      // TODO: why this is disabled?      
      return;
      /*SortMethod method = currentSortMethod;

      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (item.MusicTag != null && !item.IsFolder)
        {
          Channel channel = (Channel)item.MusicTag;
          IList details=channel.ReferringTuningDetail();
          TuningDetail detail=(TuningDetail)details[0];
          if (method == SortMethod.Bitrate)
          {
            if (detail.Bitrate > 0)
              item.Label2 = detail.Bitrate.ToString();
            else
            {
              double frequency = detail.Frequency;
              if (detail.ChannelType==6)
                frequency /= 1000000d;
              else
                frequency /= 1000d;
              item.Label2 = System.String.Format("{0:###.##} MHz.", frequency);
            }
          }
        }
      }*/
    }

    #region Sort Members
    void OnSort()
    {
      SetLabels();
      facadeView.Sort(this);
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


      SortMethod method = currentSortMethod;
      bool bAscending = sortAscending;
      Channel channel1 = item1.MusicTag as Channel;
      IList details1 = channel1.ReferringTuningDetail();
      TuningDetail detail1 = (TuningDetail)details1[0];
      Channel channel2 = item2.MusicTag as Channel;
      IList details2 = channel2.ReferringTuningDetail();
      TuningDetail detail2 = (TuningDetail)details2[0];
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
          string strURL1 = "0";
          string strURL2 = "0";
          if (item1.IconImage.ToLower().Equals("defaultmyradiostream.png"))
            strURL1 = "1";
          if (item2.IconImage.ToLower().Equals("defaultmyradiostream.png"))
              strURL2 = "1";
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
            if (strURL1.Length > 0) return 1;
            else return -1;
          }
          else
          {
            if (strURL1.Length > 0) return -1;
            else return 1;
          }
        //break;

        case SortMethod.Number:
          if (channel1 != null && channel2 != null)
          {
            if (bAscending)
            {
              if (channel1.SortOrder > channel2.SortOrder) return 1;
              else return -1;
            }
            else
            {
              if (channel2.SortOrder > channel1.SortOrder) return 1;
              else return -1;
            }
          }

          if (channel1 != null) return -1;
          if (channel2 != null) return 1;
          return 0;
        //break;
        case SortMethod.Bitrate:
          if (detail1 != null && detail2 != null)
          {
            if (bAscending)
            {
              if (detail1.Bitrate > detail2.Bitrate) return 1;
              else return -1;
            }
            else
            {
              if (detail2.Bitrate > detail1.Bitrate) return 1;
              else return -1;
            }
          }
          return 0;
      }
      return 0;
    }
    #endregion

    void OnClick(int itemIndex)
    {
      Log.Info("OnClick");
      GUIListItem item = facadeView.SelectedListItem;
      if (item.MusicTag == null)
      {
        selectedItemIndex = -1;
        LoadDirectory(null);
      }
      if (item.IsFolder)
      {
        selectedItemIndex = -1;
        LoadDirectory(item.Label);
      }
      else
      {
        Play(item);
      }
    }

    void OnShowSort()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(495); // Sort options

      dlg.AddLocalizedString(103); // name
      dlg.AddLocalizedString(668); // Type
      dlg.AddLocalizedString(669); // genre
      dlg.AddLocalizedString(670); // bitrate
      dlg.AddLocalizedString(620); // number

      dlg.SelectedLabel = (int)currentSortMethod;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1) return;

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
        GUIControl.FocusControl(GetID, btnSortBy.GetID);
    }

    bool IsUrl(string fileName)
    {
      if (fileName.ToLower().StartsWith("http:") || fileName.ToLower().StartsWith("https:") ||
      fileName.ToLower().StartsWith("mms:") || fileName.ToLower().StartsWith("rtp:"))
      {
        return true;
      }
      return false;
    }

    string GetPlayPath(Channel channel)
    {
      IList details=channel.ReferringTuningDetail();
      TuningDetail detail=(TuningDetail)details[0];
      if (channel.IsWebstream())
        return detail.Url;
      {
        string fileName = String.Format("{0}.radio", detail.Frequency);
        return fileName;
      }
    }

    void Play(GUIListItem item)
    {
      // We have the Station Name in there to retrieve the correct Coverart for the station in the Vis Window
      GUIPropertyManager.RemovePlayerProperties();
      GUIPropertyManager.SetProperty("#Play.Current.ArtistThumb", item.Label);
      GUIPropertyManager.SetProperty("#Play.Current.Album", item.Label);
      if (item.MusicTag == null) return;
      Channel channel=(Channel)item.MusicTag;
      if (g_Player.Playing)
      {
        if (!g_Player.IsTimeShifting || (g_Player.IsTimeShifting && channel.IsWebstream()))
        {
          g_Player.Stop();
        }
      }
      if (channel.IsFMRadio() || channel.IsWebstream())
        g_Player.PlayAudioStream(GetPlayPath(channel));
      else
        TVHome.ViewChannelAndCheck(channel);
    }

    void SortChanged(object sender, SortEventArgs e)
    {
      sortAscending = e.Order != System.Windows.Forms.SortOrder.Descending;

      OnSort();
      UpdateButtonStates();

      GUIControl.FocusControl(GetID, ((GUIControl)sender).GetID);
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
      return true;
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(665);
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = @"hover_my radio.png";
      return true;
    }

    public string Author()
    {
      return "Frodo, gemx";
    }

    public string Description()
    {
      return "Connect to TV service to listen to analog, DVB and internet radio";
    }

    public void ShowPlugin()
    {
      RadioSetupForm setup = new RadioSetupForm();
      setup.ShowDialog();
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
