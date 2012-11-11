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
using System.Linq;
using System.Windows.Forms;
using MediaPortal.Common.Utils;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.TvPlugin.Helper;
using WindowPlugins;
using Action = MediaPortal.GUI.Library.Action;

namespace Mediaportal.TV.TvPlugin.Radio
{
  [PluginIcons("Resources\\TvPlugin.Radio.gif", "Resources\\TvPlugin.Radio_disabled.gif")]
  public class Radio : WindowPluginBase, IComparer<GUIListItem>, ISetupForm, IShowPlugin
  {

    #region constants    

    #endregion

    #region enums

    private enum SortMethod
    {
      Name = 0,
      Type = 1,
      Genre = 2,
      Bitrate = 3,
      Number = 4
    }

    #endregion

    #region Base variables

    private static Channel _currentChannel = null;
    private static bool _autoTurnOnRadio = false;
    private SortMethod currentSortMethod = SortMethod.Number;    
    private readonly DirectoryHistory directoryHistory = new DirectoryHistory();
    private string currentFolder = null;
    private string lastFolder = "..";
    private int selectedItemIndex = -1;
    private static bool hideAllChannelsGroup = false;
    private string rootGroup = "(none)";
    private static ChannelGroup selectedGroup;
    public static List<ChannelGroup> AllRadioGroups = new List<ChannelGroup>();

    #endregion
    
    #region properties
    
    public static Channel CurrentChannel {
      get { return _currentChannel; }
      set { _currentChannel = value; }
    }
    
    public static ChannelGroup SelectedGroup 
    {
      get { 
        if(selectedGroup == null)
        { // if user is at the root level then no group is selected
          // this then causes issues in guide as it does not know what
          // group to show so return the first available one
          return AllRadioGroups[0];
        }
        else
        {
          return selectedGroup;
        }
      }
      set { selectedGroup = value; }
    }
    
    #endregion

    #region SkinControls

    [SkinControl(6)] protected GUIButtonControl btnPrevious = null;
    [SkinControl(7)] protected GUIButtonControl btnNext = null;

    #endregion

    public Radio()
    {
      TVUtil.SetGentleConfigFile();
      GetID = (int)Window.WINDOW_RADIO;
    }

    public override bool Init()
    {      
      return Load(GUIGraphicsContext.Skin + @"\MyRadio.xml");
    }

    #region Serialisation

    protected override void LoadSettings()
    {
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

        if (xmlreader.GetValueAsBool("myradio", "rememberlastgroup", true))
        {
          currentFolder = xmlreader.GetValueAsString("myradio", "lastgroup", null);
        }
        hideAllChannelsGroup = xmlreader.GetValueAsBool("myradio", "hideAllChannelsGroup", false);
        rootGroup = xmlreader.GetValueAsString("myradio", "rootgroup", "(none)");

        _autoTurnOnRadio = xmlreader.GetValueAsBool("myradio", "autoturnonradio", false);
      }
    }

    protected override void SaveSettings()
    {
      base.SaveSettings();
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue(SerializeName, "layout", (int)currentLayout);
        xmlwriter.SetValueAsBool(SerializeName, "sortasc", m_bSortAscending);
        
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

        xmlwriter.SetValue("myradio", "lastgroup", lastFolder);
        if (_currentChannel != null)
        {
            xmlwriter.SetValue("myradio", "channel", _currentChannel.DisplayName);
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
      this.LogInfo("RadioHome:OnAdded");

      LoadSettings();
      LoadChannelGroups();
    }    

    protected override void OnPageLoad()
    {
      this.LogInfo("RadioHome:OnPageLoad");
      base.OnPageLoad();
      GUIMessage msgStopRecorder = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msgStopRecorder);      
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

      SetLastChannel();

      if ((_autoTurnOnRadio) && !(g_Player.Playing && g_Player.IsRadio))
      {
        Play(facadeLayout.SelectedListItem);
      }

      btnSortBy.SortChanged += SortChanged;       
    }    

    private static void LoadChannelGroups()
    {
      Settings xmlreader = new MPSettings();
      string currentchannelName = xmlreader.GetValueAsString("myradio", "channel", String.Empty);      
      IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.GetChannelsByName(currentchannelName); 
      if (channels != null && channels.Count > 0)
      {
        _currentChannel = channels[0];
      }

      if (AllRadioGroups.Count == 0)
      {
        ChannelGroupIncludeRelationEnum include = ChannelGroupIncludeRelationEnum.GroupMaps;
        include |= ChannelGroupIncludeRelationEnum.GroupMapsChannel;
        AllRadioGroups = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllCustomChannelGroups(include, MediaTypeEnum.Radio).ToList();        
      }
    }


    private void SetLastChannel()
    {
      if (_currentChannel != null)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, selectedItemIndex);      
        UpdateButtonStates();        
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      selectedItemIndex = facadeLayout.SelectedListItemIndex;
      SaveSettings();
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClick(int iItem)
    {
      this.LogInfo("OnClick");
      GUIListItem item = facadeLayout[iItem];
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

    protected override void UpdateButtonStates()
    {
      base.UpdateButtonStates();
      
      string strLine = string.Empty;
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
      }

      if (null != facadeLayout)
        facadeLayout.EnableScrollLabel = currentSortMethod == SortMethod.Name;
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
      GUIListItem SelectedItem = facadeLayout.SelectedListItem;
      if (SelectedItem != null)
      {
        if (SelectedItem.IsFolder && SelectedItem.Label != "..")
        {
          directoryHistory.Set(SelectedItem.Label, currentFolder);
        }
      }
      currentFolder = strNewDirectory;
      GUIControl.ClearControl(GetID, facadeLayout.GetID);

      int totalItems = 0;
      if (currentFolder == null || currentFolder == "..")
      {
 
        IList<ChannelGroup> groups = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroupsByMediaType(MediaTypeEnum.Radio).ToList();
        foreach (ChannelGroup group in groups)
        {
          if (hideAllChannelsGroup && group.GroupName.Equals(TvConstants.RadioGroupNames.AllChannels) &&
              groups.Count > 1)
          {
            continue;
          }

          if (group.GroupName == rootGroup)
          {
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
        if (rootGroup != "(none)")
        {
          ChannelGroup root = ServiceAgents.Instance.ChannelGroupServiceAgent.GetChannelGroupByNameAndMediaType(rootGroup, MediaTypeEnum.Radio);
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
                  selectedItemIndex = totalItems-1;
                }
              }

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
              string thumbnail = Utils.GetCoverArt(Thumbs.Radio, channel.DisplayName);
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
        selectedGroup = null;
      }
      else
      {
        ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetChannelGroupByNameAndMediaType(currentFolder, MediaTypeEnum.Radio);
        if (group == null)
        {
          return;
        }
        selectedGroup = group;
        lastFolder = currentFolder;
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
              item.Label = channel.DisplayName;
              item.IsFolder = false;
              item.MusicTag = channel;
              item.AlbumInfoTag = map;
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
              string thumbnail = Utils.GetCoverArt(Thumbs.Radio, channel.DisplayName);
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
              selectedItemIndex = i++;
              break;
            }
          }
        }

        //set selected item
        if (selectedItemIndex >= 0)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, selectedItemIndex);
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
        return -1;
      }
      if (item1.IsFolder && !item2.IsFolder)
      {
        return -1;
      }
      if (!item1.IsFolder && item2.IsFolder)
      {
        return 1;
      }


      SortMethod method = currentSortMethod;
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

        case SortMethod.Type:
          string strURL1 = "0";
          string strURL2 = "0";
          if (item1.IconImage.ToLower().Equals("defaultmyradiostream.png"))
          {
            strURL1 = "1";
          }
          if (item2.IconImage.ToLower().Equals("defaultmyradiostream.png"))
          {
            strURL2 = "1";
          }
          if (strURL1.Equals(strURL2))
          {
            if (bAscending)
            {
              return String.Compare(item1.Label, item2.Label, true);
            }
            return String.Compare(item2.Label, item1.Label, true);
          }
          if (bAscending)
          {
            if (strURL1.Length > 0)
            {
              return 1;
            }
            return -1;
          }
          if (strURL1.Length > 0)
          {
            return -1;
          }
          return 1;
          //break;

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
        case SortMethod.Bitrate:
          IList<TuningDetail> details1 = channel1.TuningDetails;
          TuningDetail detail1 = details1[0];
          IList<TuningDetail> details2 = channel2.TuningDetails;
          TuningDetail detail2 = details2[0];
          if (detail1 != null && detail2 != null)
          {
            if (bAscending)
            {
              if (detail1.Bitrate > detail2.Bitrate)
              {
                return 1;
              }
              return -1;
            }
            if (detail2.Bitrate > detail1.Bitrate)
            {
              return 1;
            }
            return -1;
          }
          return 0;
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

      dlg.SelectedLabel = (int)currentSortMethod;

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

    private static string GetPlayPath(Channel channel)
    {
      IList<TuningDetail> details = channel.TuningDetails;
      TuningDetail detail = details[0];
      if (channel.IsWebstream())
      {
        return detail.Url;
      }
      {
        string fileName = String.Format("{0}.radio", detail.Frequency);
        return fileName;
      }
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
      GUIPropertyManager.SetProperty("#Play.Current.ArtistThumb", _currentChannel.DisplayName);
      GUIPropertyManager.SetProperty("#Play.Current.Album", _currentChannel.DisplayName);
      GUIPropertyManager.SetProperty("#Play.Current.Title", _currentChannel.DisplayName);
      
      string strLogo = Utils.GetCoverArt(Thumbs.Radio, _currentChannel.DisplayName);
      if (string.IsNullOrEmpty(strLogo))
      {
          strLogo = "defaultMyRadioBig.png";
      }
      
      GUIPropertyManager.SetProperty("#Play.Current.Thumb", strLogo);

      if (g_Player.Playing)
      {
        if (!g_Player.IsTimeShifting || (g_Player.IsTimeShifting && _currentChannel.IsWebstream()))
        {
          g_Player.Stop();
        }
      }

      if (_currentChannel.IsWebstream())
      {
        g_Player.PlayAudioStream(GetPlayPath(_currentChannel));
        GUIPropertyManager.SetProperty("#Play.Current.Title", _currentChannel.DisplayName);
      }
      else
      {
        if (g_Player.IsRadio && g_Player.Playing)
        {
          Channel currentlyPlaying = TVHome.Navigator.Channel.Entity;
          if (currentlyPlaying != null && currentlyPlaying.IdChannel == _currentChannel.IdChannel)
          {
            return;
          }
        }
        TVHome.ViewChannelAndCheck(_currentChannel, 0);
      }
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