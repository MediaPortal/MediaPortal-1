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
using System.Collections.Generic;
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;


namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class EpgGrabber : SectionSettings
  {
    private bool _loaded;
    private readonly MPListViewStringColumnSorter lvwColumnSorter;

    public EpgGrabber(string name, MediaTypeEnum mediaType)
      : base(name)
    {
      MediaTypeEnum = mediaType;
      InitializeComponent();
      lvwColumnSorter = new MPListViewStringColumnSorter();
      lvwColumnSorter.Order = SortOrder.None;
      mpListView1.ListViewItemSorter = lvwColumnSorter;
    }

    public MediaTypeEnum MediaTypeEnum
    {
      get { return _mediaTypeEnum; }
      set { _mediaTypeEnum = value; }
    }

    private void LoadLanguages()
    {
      _loaded = true;
      mpListView2.BeginUpdate();
      try
      {
        mpListView2.Items.Clear();
        Languages languages = new Languages();
        List<String> codes = languages.GetLanguageCodes();
        List<String> list = languages.GetLanguages();

        Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSetting("epgLanguages");

        string values = "";
        for (int j = 0; j < list.Count; j++)
        {
          ListViewItem item = new ListViewItem(new string[] { list[j], codes[j] });
          mpListView2.Items.Add(item);
          item.Tag = codes[j];
          if (setting.Value == "")
          {
            values += item.Tag;
            values += ",";
          }
          else
          {
            if (setting.Value.IndexOf((string)item.Tag) >= 0)
            {
              item.Checked = true;
            }
          }
        }
        mpListView2.Sort();

        if (setting.Value == "")
        {
          setting.Value = values;
          ServiceAgents.Instance.SettingServiceAgent.SaveSetting(setting.Tag, setting.Value);
          //DatabaseManager.Instance.SaveChanges();
        }
      }
      finally
      {
        mpListView2.EndUpdate();
      }
    }

    public override void OnSectionDeActivated()
    {
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("epgStoreOnlySelected", mpCheckBoxStoreOnlySelected.Checked ? "yes" : "no");
      base.OnSectionDeActivated();
      SaveSettings();
    }

    private bool _ignoreItemCheckedEvent = true;
    private MediaTypeEnum _mediaTypeEnum;

    public override void OnSectionActivated()
    {
      mpListView1.BeginUpdate();
      try
      {
        _ignoreItemCheckedEvent = true;
        LoadLanguages();
        Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSetting("epgStoreOnlySelected");
        mpCheckBoxStoreOnlySelected.Checked = (setting.Value == "yes");
        Dictionary<string, CardType> cards = new Dictionary<string, CardType>();
        IList<Card> dbsCards = ServiceAgents.Instance.CardServiceAgent.ListAllCards(CardIncludeRelationEnum.None);
        foreach (Card card in dbsCards)
        {
          cards[card.DevicePath] = ServiceAgents.Instance.ControllerServiceAgent.Type(card.IdCard);
        }
        base.OnSectionActivated();
        mpListView1.Items.Clear();

        ChannelIncludeRelationEnum includeRelations = ChannelIncludeRelationEnum.TuningDetails;
        includeRelations |= ChannelIncludeRelationEnum.ChannelMaps;
        includeRelations |= ChannelIncludeRelationEnum.ChannelMapsCard;        
        IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannelsByMediaType(_mediaTypeEnum, includeRelations);


        foreach (Channel ch in channels)
        {
          bool analog = false;
          bool dvbc = false;
          bool dvbt = false;
          bool dvbs = false;
          bool atsc = false;
          bool dvbip = false;
          bool hasFta = false;
          bool hasScrambled = false;
          if (ch.MediaType != (decimal)_mediaTypeEnum)
            continue;
          if (ch.IsWebstream())
          {
            continue;
          }

          IList<TuningDetail> tuningDetails = ch.TuningDetails;
          foreach (TuningDetail detail in tuningDetails)
          {
            if (detail.FreeToAir)
            {
              hasFta = true;
            }
            if (!detail.FreeToAir)
            {
              hasScrambled = true;
            }
          }

          int imageIndex;
          if (hasFta && hasScrambled)
          {
            imageIndex = 5;
          }
          else if (hasScrambled)
          {
            imageIndex = 4;
          }
          else
          {
            imageIndex = 3;
          }

          ListViewItem item = mpListView1.Items.Add(ch.DisplayName, imageIndex);
          foreach (ChannelMap map in ch.ChannelMaps)
          {
            if (cards.ContainsKey(map.Card.DevicePath))
            {
              CardType type = cards[map.Card.DevicePath];
              switch (type)
              {
                case CardType.Analog:
                  analog = true;
                  break;
                case CardType.DvbC:
                  dvbc = true;
                  break;
                case CardType.DvbT:
                  dvbt = true;
                  break;
                case CardType.DvbS:
                  dvbs = true;
                  break;
                case CardType.Atsc:
                  atsc = true;
                  break;
                case CardType.DvbIP:
                  dvbip = true;
                  break;
              }
            }
          }
          string line = "";
          if (analog)
          {
            line += "Analog";
          }
          if (dvbc)
          {
            if (line != "")
              line += ",";
            line += "DVB-C";
          }
          if (dvbt)
          {
            if (line != "")
              line += ",";
            line += "DVB-T";
          }
          if (dvbs)
          {
            if (line != "")
              line += ",";
            line += "DVB-S";
          }
          if (atsc)
          {
            if (line != "")
              line += ",";
            line += "ATSC";
          }
          if (dvbip)
          {
            if (line != "")
              line += ",";
            line += "DVB-IP";
          }
          item.SubItems.Add(line);
          item.Checked = ch.GrabEpg;
          item.Tag = ch;
        }
      }
      finally
      {
        mpListView1.EndUpdate();
        _ignoreItemCheckedEvent = false;
      }
    }

    public override void SaveSettings()
    {
      if (false == _loaded)
        return;      
      
      string value = ",";
      for (int i = 0; i < mpListView2.Items.Count; ++i)
      {
        if (mpListView2.Items[i].Checked)
        {
          string code = (string)mpListView2.Items[i].Tag;
          value += code;
          value += ",";
        }
      }
      
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("epgLanguages", value);
      base.SaveSettings();
    }

    private void mpListView1_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        lvwColumnSorter.Order = lvwColumnSorter.Order == SortOrder.Ascending
                                  ? SortOrder.Descending
                                  : SortOrder.Ascending;
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter.SortColumn = e.Column;
        lvwColumnSorter.Order = SortOrder.Ascending;
      }
      // Perform the sort with these new sort options.
      mpListView1.Sort();
    }

    private void mpListView1_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      if (!_ignoreItemCheckedEvent)
      {
        Channel channel = e.Item.Tag as Channel;
        if (channel == null)
          return;
        channel.GrabEpg = e.Item.Checked;
        ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(channel);
      }      
    }

    private void linkLabelTVAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        _ignoreItemCheckedEvent = true;

        ICollection<Channel> channels = new List<Channel>();

        for (int i = 0; i < mpListView1.Items.Count; ++i)
        {          
          mpListView1.Items[i].Checked = true;
          var channel = mpListView1.Items[i].Tag as Channel;
          if (channel != null)
          {
            channel.GrabEpg = true;
            channels.Add(channel);
          }
        }
        if (channels.Count > 0)
        {
          ServiceAgents.Instance.ChannelServiceAgent.SaveChannels(channels);
        }
      }
      finally
      {
        mpListView1.EndUpdate();
        _ignoreItemCheckedEvent = false;
      }
    }

    private void linkLabelTVAllGrouped_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        _ignoreItemCheckedEvent = true;
        ICollection<Channel> channels = new List<Channel>();
        for (int i = 0; i < mpListView1.Items.Count; ++i)
        {
          Channel ch = (Channel)mpListView1.Items[i].Tag;
          mpListView1.Items[i].Checked = (ch.GroupMaps.Count > 1);
          channels.Add(ch);
          // if count > 1 we assume that the channel has one or more custom group(s) associated with it.
        }
        if (channels.Count > 0)
        {
          ServiceAgents.Instance.ChannelServiceAgent.SaveChannels(channels);
        }
      }
      finally
      {
        mpListView1.EndUpdate();
        _ignoreItemCheckedEvent = false;
      }
    }

    private void linkLabelTVGroupedVisible_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        _ignoreItemCheckedEvent = true;
        ICollection<Channel> channels = new List<Channel>();
        for (int i = 0; i < mpListView1.Items.Count; ++i)
        {
          Channel ch = (Channel)mpListView1.Items[i].Tag;
          mpListView1.Items[i].Checked = (ch.GroupMaps.Count > 1 && ch.VisibleInGuide);
          channels.Add(ch);
          // if count > 1 we assume that the channel has one or more custom group(s) associated with it.
        }
        if (channels.Count > 0)
        {
          ServiceAgents.Instance.ChannelServiceAgent.SaveChannels(channels);
        }
      }
      finally
      {
        mpListView1.EndUpdate();
        _ignoreItemCheckedEvent = false;
      }
    }

    private void linkLabelTVNone_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        _ignoreItemCheckedEvent = true;
        ICollection<Channel> channels = new List<Channel>();
        for (int i = 0; i < mpListView1.Items.Count; ++i)
        {
          mpListView1.Items[i].Checked = false;
          channels.Add(mpListView1.Items[i].Tag as Channel);
        }
        if (channels.Count > 0)
        {
          ServiceAgents.Instance.ChannelServiceAgent.SaveChannels(channels);
        }
      }
      finally
      {
        mpListView1.EndUpdate();
        _ignoreItemCheckedEvent = false;
      }
    }

    private void linkLabelLanguageAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView2.BeginUpdate();
      try
      {
        for (int i = 0; i < mpListView2.Items.Count; ++i)
        {
          mpListView2.Items[i].Checked = true;
        }
        Languages languages = new Languages();
        List<String> codes = languages.GetLanguageCodes();
        
        string value = "";
        foreach (string code in codes)
        {
          value += code;
          value += ",";
        }
        //Log.WriteFile("tvsetup:epggrabber:all: epglang={0}", setting.value);
        Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("epgLanguages", value);
      }
      finally
      {
        mpListView2.EndUpdate();
      }
    }

    private void linkLabelLanguageNone_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView2.BeginUpdate();
      try
      {
        for (int i = 0; i < mpListView2.Items.Count; ++i)
        {
          mpListView2.Items[i].Checked = false;
        }
        Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("epgLanguages", ",");
        Log.WriteFile("tvsetup:epggrabber:none: epglang={0}", setting.Value);
      }
      finally
      {
        mpListView2.EndUpdate();
      }
    }
  }
}