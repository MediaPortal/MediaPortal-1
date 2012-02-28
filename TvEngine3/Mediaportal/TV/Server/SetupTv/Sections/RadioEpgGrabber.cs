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
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.ServiceAgents;


namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class RadioEpgGrabber : SectionSettings
  {
    private bool _loaded;
    private readonly MPListViewStringColumnSorter lvwColumnSorter;

    public RadioEpgGrabber()
      : this("Radio Epg grabber") { }

    public RadioEpgGrabber(string name)
      : base(name)
    {
      InitializeComponent();
      lvwColumnSorter = new MPListViewStringColumnSorter();
      lvwColumnSorter.Order = SortOrder.None;
      mpListView1.ListViewItemSorter = lvwColumnSorter;
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

        
        Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSetting("radioLanguages");

        string values = "";
        for (int j = 0; j < list.Count; j++)
        {
          ListViewItem item = new ListViewItem(new string[] { list[j], codes[j] });
          mpListView2.Items.Add(item);
          item.Tag = codes[j];
          if (setting.value == "")
          {
            values += item.Tag;
            values += ",";
          }
          else
          {
            if (setting.value.IndexOf((string)item.Tag) >= 0)
            {
              item.Checked = true;
            }
          }
        }
        mpListView2.Sort();

        if (setting.value == "")
        {
          setting.value = values;
          ServiceAgents.Instance.SettingServiceAgent.SaveSetting(setting.tag, setting.value);
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
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("epgRadioStoreOnlySelected", mpCheckBoxStoreOnlySelected.Checked ? "yes" : "no");      
      base.OnSectionDeActivated();
      SaveSettings();
    }

    private bool _ignoreItemCheckedEvent = true;
    public override void OnSectionActivated()
    {
      mpListView1.BeginUpdate();
      try
      {
        _ignoreItemCheckedEvent = true;
        LoadLanguages();
        Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSetting("epgRadioStoreOnlySelected");
        mpCheckBoxStoreOnlySelected.Checked = (setting.value == "yes");
        Dictionary<string, CardType> cards = new Dictionary<string, CardType>();
        IList<Card> dbsCards = ServiceAgents.Instance.CardServiceAgent.ListAllCards(CardIncludeRelationEnum.None);
        foreach (Card card in dbsCards)
        {
          cards[card.devicePath] = ServiceAgents.Instance.ControllerServiceAgent.Type(card.idCard);
        }
        base.OnSectionActivated();
        mpListView1.Items.Clear();

        ChannelIncludeRelationEnum includeRelations = ChannelIncludeRelationEnum.TuningDetails;
        includeRelations |= ChannelIncludeRelationEnum.ChannelMaps;
        IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannelsByMediaType(MediaTypeEnum.Radio);

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
          if (ch.mediaType != (int)MediaTypeEnum.Radio)
            continue;
          if (ch.IsWebstream())
            continue;

          IList<TuningDetail> tuningDetails = ch.TuningDetails;
          foreach (TuningDetail detail in tuningDetails)
          {
            if (detail.freeToAir)
            {
              hasFta = true;
            }
            if (!detail.freeToAir)
            {
              hasScrambled = true;
            }
          }

          int imageIndex;
          if (hasFta && hasScrambled)
          {
            imageIndex = 2;
          }
          else if (hasScrambled)
          {
            imageIndex = 1;
          }
          else
          {
            imageIndex = 0;
          }

          ListViewItem item = mpListView1.Items.Add(ch.displayName, imageIndex);
          foreach (ChannelMap map in ch.ChannelMaps)
          {
            if (cards.ContainsKey(map.Card.devicePath))
            {
              CardType type = cards[map.Card.devicePath];
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
          item.Checked = ch.grabEpg;
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
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("radioLanguages", value);
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
        channel.grabEpg = e.Item.Checked;
        ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(channel); 
      }      
    }

    private void linkLabelRadioAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        for (int i = 0; i < mpListView1.Items.Count; ++i)
        {
          mpListView1.Items[i].Checked = true;
        }
      }
      finally
      {
        mpListView1.EndUpdate();
      }
    }

    private void linkLabelRadioAllGrouped_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        for (int i = 0; i < mpListView1.Items.Count; ++i)
        {
          Channel ch = (Channel)mpListView1.Items[i].Tag;
          mpListView1.Items[i].Checked = (ch.GroupMaps.Count > 1);
          // if count > 1 we assume that the channel has one or more custom group(s) associated with it.
        }
      }
      finally
      {
        mpListView1.EndUpdate();
      }
    }

    private void linkLabelRadioGroupedVisible_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        for (int i = 0; i < mpListView1.Items.Count; ++i)
        {
          Channel ch = (Channel)mpListView1.Items[i].Tag;
          mpListView1.Items[i].Checked = (ch.GroupMaps.Count > 1 && ch.visibleInGuide);
          // if count > 1 we assume that the channel has one or more custom group(s) associated with it.
        }
      }
      finally
      {
        mpListView1.EndUpdate();
      }
    }

    private void linkLabelRadioNone_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        for (int i = 0; i < mpListView1.Items.Count; ++i)
        {
          mpListView1.Items[i].Checked = false;
        }
      }
      finally
      {
        mpListView1.EndUpdate();
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
        ServiceAgents.Instance.SettingServiceAgent.SaveSetting("radioLanguages", value);
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
        ServiceAgents.Instance.SettingServiceAgent.SaveSetting("radioLanguages", ",");                
      }
      finally
      {
        mpListView2.EndUpdate();
      }
    }
  }
}