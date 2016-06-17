#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections.Generic;
using System.Windows.Forms;
using Common.GUIPlugins;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Integration;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using Action = MediaPortal.GUI.Library.Action;

namespace Mediaportal.TV.TvPlugin.Radio
{
  [PluginIcons("Resources\\TvPlugin.Radio.gif", "Resources\\TvPlugin.Radio_disabled.gif")]
  public class Radio : WindowPluginBase, IComparer<GUIListItem>, ISetupForm, IShowPlugin
  {
    #region enums

    private enum SortMethod
    {
      Name = 0,
      Number = 4
    }

    #endregion

    #region Base variables

    private static Channel _currentChannel = null;
    private static bool _autoTurnOnRadio = false;
    private SortMethod _currentSortMethod = SortMethod.Number;    
    private readonly DirectoryHistory _directoryHistory = new DirectoryHistory();
    private string _currentFolder = null;
    private string _lastFolder = "..";
    private int _selectedItemIndex = -1;
    private string _rootGroup = "(none)";
    private static ChannelGroup _selectedGroup;
    public static IList<ChannelGroup> AllRadioGroups = new List<ChannelGroup>();
    private static bool _settingsRadioLoaded = false;

    #endregion
    
    #region properties
    
    public static Channel CurrentChannel
    {
      get
      {
        return _currentChannel;
      }
      set
      {
        _currentChannel = value;
      }
    }
    
    public static ChannelGroup SelectedGroup 
    {
      get
      { 
        if (_selectedGroup == null)
        { // if user is at the root level then no group is selected
          // this then causes issues in guide as it does not know what
          // group to show so return the first available one
          return AllRadioGroups[0];
        }
        else
        {
          return _selectedGroup;
        }
      }
      set
      {
        _selectedGroup = value;
      }
    }
    
    #endregion

    #region SkinControls

    [SkinControl(6)] protected GUIButtonControl btnPrevious = null;
    [SkinControl(7)] protected GUIButtonControl btnNext = null;

    #endregion

    public Radio()
    {
      IntegrationProviderHelper.Register();
      GetID = (int)Window.WINDOW_RADIO;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\MyRadio.xml"));
    }

    #region Serialisation

    protected override void LoadSettings()
    {
      if (_settingsRadioLoaded)
      {
        return;
      }

      base.LoadSettings();
      using (Settings xmlreader = new MPSettings())
      {
        currentLayout = (GUIFacadeControl.Layout)xmlreader.GetValueAsInt(SerializeName, "layout", (int)GUIFacadeControl.Layout.List);
        m_bSortAscending = xmlreader.GetValueAsBool(SerializeName, "sortasc", true);

        string tmpLine = xmlreader.GetValue("myradio", "sort");
        if (tmpLine != null)
        {
          if (tmpLine == "name")
          {
            _currentSortMethod = SortMethod.Name;
          }
          else if (tmpLine == "number")
          {
            _currentSortMethod = SortMethod.Number;
          }
        }

        if (xmlreader.GetValueAsBool("myradio", "rememberlastgroup", true))
        {
          _currentFolder = xmlreader.GetValueAsString("myradio", "lastgroup", null);
        }
        _rootGroup = xmlreader.GetValueAsString("myradio", "rootgroup", "(none)");

        _autoTurnOnRadio = xmlreader.GetValueAsBool("myradio", "autoturnonradio", false);
      }
      _settingsRadioLoaded = true;
    }

    protected override void SaveSettings()
    {
      base.SaveSettings();
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue(SerializeName, "layout", (int)currentLayout);
        xmlwriter.SetValueAsBool(SerializeName, "sortasc", m_bSortAscending);
        
        switch (_currentSortMethod)
        {
          case SortMethod.Name:
            xmlwriter.SetValue("myradio", "sort", "name");
            break;
          case SortMethod.Number:
            xmlwriter.SetValue("myradio", "sort", "number");
            break;
        }

        xmlwriter.SetValue("myradio", "lastgroup", _lastFolder);
        if (_currentChannel != null)
        {
          xmlwriter.SetValue("myradio", "channel", _currentChannel.Name);
        }
      }
    }

    #endregion

    #region BaseWindow Members

    protected override string SerializeName
    {
      get
      {
        return "myradio";
      }
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (facadeLayout.Focus)
        {
          GUIListItem item = facadeLayout[0];
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
        GUIListItem item = facadeLayout[0];
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

    public override void OnAdded()
    {
      IntegrationProviderHelper.Register();
      this.LogInfo("RadioHome:OnAdded");

      LoadSettings();
      LoadChannelGroups();
    }

    protected override void OnPageLoad()
    {
      this.LogInfo("RadioHome:OnPageLoad");

      TVHome.ShowTvEngineSettingsUIIfConnectionDown();

      // Reload ChannelGroups
      Radio radioLoad = (Radio)GUIWindowManager.GetWindow((int)Window.WINDOW_RADIO);
      radioLoad.OnAdded();

      base.OnPageLoad();
      GUIMessage msgStopRecorder = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msgStopRecorder);
      switch (_currentSortMethod)
      {
        case SortMethod.Name:
          btnSortBy.SelectedItem = 0;
          break;
        case SortMethod.Number:
          btnSortBy.SelectedItem = 4;
          break;
      }      

      SelectCurrentItem();
      LoadDirectory(_currentFolder);

      SetLastChannel();

      if ((_autoTurnOnRadio) && !(g_Player.Playing && g_Player.IsRadio))
      {
        GUIListItem item = facadeLayout.SelectedListItem;
        if (item != null && item.Label != ".." && !item.IsFolder)
        {
          Play(facadeLayout.SelectedListItem);
        }
      }

      btnSortBy.SortChanged += SortChanged;
    }    

    private static void LoadChannelGroups()
    {
      if (!TVHome.Connected)
      {
        return;
      }
      Settings xmlreader = new MPSettings();
      string currentchannelName = xmlreader.GetValueAsString("myradio", "channel", String.Empty);      
      IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.GetChannelsByName(currentchannelName, ChannelRelation.None);
      if (channels != null && channels.Count > 0)
      {
        _currentChannel = channels[0];
      }

      if (AllRadioGroups.Count == 0)
      {
        AllRadioGroups = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroupsByMediaType(MediaType.Radio, ChannelGroupRelation.GroupMapsChannel);
      }
    }

    private void SetLastChannel()
    {
      if (_currentChannel != null)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _selectedItemIndex);      
        UpdateButtonStates();
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      _selectedItemIndex = facadeLayout.SelectedListItemIndex;
      SaveSettings();
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClick(int iItem)
    {
      this.LogInfo("OnClick");
      GUIListItem item = facadeLayout[iItem];
      if (item.MusicTag == null)
      {
        _selectedItemIndex = -1;
        LoadDirectory(null);
      }
      if (item.IsFolder)
      {
        _selectedItemIndex = -1;
        LoadDirectory(item.Label);
      }
      else
      {
        Play(item);
      }
    }

    protected override void UpdateButtonStates()
    {
      base.UpdateButtonStates();
      
      string strLine = string.Empty;
      switch (_currentSortMethod)
      {
        case SortMethod.Name:
          strLine = GUILocalizeStrings.Get(103);
          break;
        case SortMethod.Number:
          strLine = GUILocalizeStrings.Get(620);
          break;
      }
      if (btnSortBy != null)
      {
        btnSortBy.Label = strLine;
      }

      if (null != facadeLayout)
        facadeLayout.EnableScrollLabel = _currentSortMethod == SortMethod.Name;
    }

    protected override bool AllowLayout(GUIFacadeControl.Layout layout)
    {
      switch (layout)
      {
        case GUIFacadeControl.Layout.List:
        case GUIFacadeControl.Layout.SmallIcons:
        case GUIFacadeControl.Layout.LargeIcons:
          return true;
      }
      return false;
    }  

    protected override void LoadDirectory(string strNewDirectory)
    {
      if (!TVHome.Connected)
      {
        return;
      }
      GUIListItem SelectedItem = facadeLayout.SelectedListItem;
      if (SelectedItem != null)
      {
        if (SelectedItem.IsFolder && SelectedItem.Label != "..")
        {
          _directoryHistory.Set(SelectedItem.Label, _currentFolder);
        }
      }
      _currentFolder = strNewDirectory;
      GUIControl.ClearControl(GetID, facadeLayout.GetID);

      int totalItems = 0;
      if (_currentFolder == null || _currentFolder == "..")
      {
        ChannelGroup root = null;
        foreach (ChannelGroup group in AllRadioGroups)
        {
          if (group.GroupName == _rootGroup)
          {
            root = group;
            continue;
          }
          GUIListItem item = new GUIListItem();
          item.Label = group.GroupName;
          item.IsFolder = true;
          item.MusicTag = group;
          item.ThumbnailImage = String.Empty;
          Utils.SetDefaultIcons(item);
          string thumbnail = Utils.GetCoverArt(Thumbs.Radio, "folder_" + group.GroupName);
          if (!string.IsNullOrEmpty(thumbnail))
          {
            item.IconImageBig = thumbnail;
            item.IconImage = thumbnail;
            item.ThumbnailImage = thumbnail;
          }
          facadeLayout.Add(item);
          totalItems++;
        }
        if (root != null)
        {
          IList<GroupMap> maps = root.GroupMaps;
          foreach (GroupMap map in maps)
          {
            Channel channel = map.Channel;
            GUIListItem item = new GUIListItem();

            if (_currentChannel != null)
            {
              if (channel.IdChannel == _currentChannel.IdChannel)
              {
                _selectedItemIndex = totalItems - 1;
              }
            }

            item.Label = channel.Name;
            item.IsFolder = false;
            item.MusicTag = channel;
            item.IconImageBig = "DefaultMyradioBig.png";
            item.IconImage = "DefaultMyradio.png";
            string thumbnail = Utils.GetCoverArt(Thumbs.Radio, channel.Name);
            if (!string.IsNullOrEmpty(thumbnail))
            {
              item.IconImageBig = thumbnail;
              item.IconImage = thumbnail;
              item.ThumbnailImage = thumbnail;
            }
            facadeLayout.Add(item);
            totalItems++;
          }
        }
        _selectedGroup = null;
      }
      else
      {
        if (SelectedItem == null)
        {
          return;
        }
        ChannelGroup group = SelectedItem.MusicTag as ChannelGroup;
        if (group == null)
        {
          return;
        }
        _selectedGroup = group;
        _lastFolder = _currentFolder;
        GUIListItem item = new GUIListItem();
        item.Label = "..";
        item.IsFolder = true;
        item.MusicTag = null;
        item.ThumbnailImage = String.Empty;
        Utils.SetDefaultIcons(item);
        facadeLayout.Add(item);
        IList<GroupMap> maps = group.GroupMaps;
        foreach (GroupMap map in maps)
        {
          Channel channel = map.Channel;

          if (channel != null)
          {
            item = new GUIListItem();
            item.Label = channel.Name;
            item.IsFolder = false;
            item.MusicTag = channel;
            item.AlbumInfoTag = map;
            item.IconImageBig = "DefaultMyradioBig.png";
            item.IconImage = "DefaultMyradio.png";
            string thumbnail = Utils.GetCoverArt(Thumbs.Radio, channel.Name);
            if (!string.IsNullOrEmpty(thumbnail))
            {
              item.IconImageBig = thumbnail;
              item.IconImage = thumbnail;
              item.ThumbnailImage = thumbnail;
            }
            facadeLayout.Add(item);
            totalItems++;
          }
        }
      }

      SwitchLayout();
      OnSort();

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Utils.GetObjectCountLabel(totalItems));

      SelectCurrentItem();
      SetLabels();
      
      for (int i = 0; i < facadeLayout.Count; ++i)
      {       
        GUIListItem item = facadeLayout[i];
        if (item != null)
        {
          Channel channel = item.MusicTag as Channel;   

          if ((channel != null) && (_currentChannel != null))
          {
            if (channel.IdChannel == _currentChannel.IdChannel)
            {
              _selectedItemIndex = i++;
              break;
            }
          }
        }

        //set selected item
        if (_selectedItemIndex >= 0)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _selectedItemIndex);
        }
      }
    }

    #endregion

    private static void SetLabels()
    {
      return;
    }

    #region Sort Members

    private void OnSort()
    {
      SetLabels();
      facadeLayout.Sort(this);
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
        return 1;
      }
      if (item1.IsFolder && !item2.IsFolder)
      {
        return -1;
      }
      if (!item1.IsFolder && item2.IsFolder)
      {
        return 1;
      }


      SortMethod method = _currentSortMethod;
      bool bAscending = m_bSortAscending;
      Channel channel1 = item1.MusicTag as Channel;
      Channel channel2 = item2.MusicTag as Channel;
      switch (method)
      {
        case SortMethod.Name:
          if (bAscending)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          return String.Compare(item2.Label, item1.Label, true);
        case SortMethod.Number:
          if (channel1 != null && channel2 != null)
          {
            GroupMap channel1GroupMap = (GroupMap)item1.AlbumInfoTag;
            GroupMap channel2GroupMap = (GroupMap)item2.AlbumInfoTag;
            int channel1GroupSort = channel1GroupMap.SortOrder;
            int channel2GroupSort = channel2GroupMap.SortOrder;
            if (bAscending)
            {
              if (channel1GroupSort > channel2GroupSort)
              {
                return 1;
              }
              return -1;
            }
            if (channel2GroupSort > channel1GroupSort)
            {
              return 1;
            }
            return -1;
          }

          if (channel1 != null)
          {
            return -1;
          }
          return channel2 != null ? 1 : 0;
          //break;
      }
      return 0;
    }

    #endregion

    protected override void OnShowSort()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
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

      dlg.SelectedLabel = (int)_currentSortMethod;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 103:
          _currentSortMethod = SortMethod.Name;
          break;
        case 620:
          _currentSortMethod = SortMethod.Number;
          break;
        default:
          _currentSortMethod = SortMethod.Name;
          break;
      }

      OnSort();
      if (btnSortBy != null)
      {
        GUIControl.FocusControl(GetID, btnSortBy.GetID);
      }
    }

    private static string GetPlayPath(Channel channel)
    {
      return string.Format("{0}.radio", channel.TuningDetails[0].Frequency);
    }

    private static void Play(GUIListItem item)
    {
      // We have the Station Name in there to retrieve the correct Coverart for the station in the Vis Window
      GUIPropertyManager.RemovePlayerProperties();
      GUIPropertyManager.SetProperty("#Play.Current.ArtistThumb", item.Label);
      GUIPropertyManager.SetProperty("#Play.Current.Album", item.Label);
      GUIPropertyManager.SetProperty("#Play.Current.Thumb", item.ThumbnailImage);
      if (item.MusicTag == null)
      {
        return;
      }
      _currentChannel = (Channel)item.MusicTag;
      
      Play();
    }
      
    public static void Play ()
    {
      // We have the Station Name in there to retrieve the correct Coverart for the station in the Vis Window
      GUIPropertyManager.RemovePlayerProperties();
      GUIPropertyManager.SetProperty("#Play.Current.ArtistThumb", _currentChannel.Name);
      GUIPropertyManager.SetProperty("#Play.Current.Album", _currentChannel.Name);
      GUIPropertyManager.SetProperty("#Play.Current.Title", _currentChannel.Name);
      
      string strLogo = Utils.GetCoverArt(Thumbs.Radio, _currentChannel.Name);
      if (string.IsNullOrEmpty(strLogo))
      {
        strLogo = "defaultMyRadioBig.png";
      }
      
      GUIPropertyManager.SetProperty("#Play.Current.Thumb", strLogo);

      TVHome.ViewChannelAndCheck(_currentChannel);
    }

    private void SortChanged(object sender, SortEventArgs e)
    {
      m_bSortAscending = e.Order != SortOrder.Descending;

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
      return "Connect to TV service to listen to radio";
    }

    public void ShowPlugin()
    {
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