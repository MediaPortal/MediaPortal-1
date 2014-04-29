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
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using MediaPortal.Common.Utils.Localisation;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class EpgGrabber : SectionSettings
  {
    private bool _loaded;
    private readonly MPListViewStringColumnSorter lvwColumnSorter;
    private readonly string languagesSettingsKey;
    private readonly string storeOnlySelectedSettingsKey;    

    private EpgGrabber() { }

    public EpgGrabber(string name, string languagesSettingsKey, string storeOnlySelectedSettingsKey, MediaTypeEnum mediaTypeEnum)
      : base(name)
    {
      InitializeComponent();
      lvwColumnSorter = new MPListViewStringColumnSorter();
      lvwColumnSorter.Order = SortOrder.None;
      mpListView1.ListViewItemSorter = lvwColumnSorter;
      mpListView2.ListViewItemSorter = lvwColumnSorter;
      this.languagesSettingsKey = languagesSettingsKey;
      this.storeOnlySelectedSettingsKey = storeOnlySelectedSettingsKey;
      _mediaTypeEnum = mediaTypeEnum;
      this.tabPage1.Text = name;
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
        string epgLanguages = ServiceAgents.Instance.SettingServiceAgent.GetValue(languagesSettingsKey, string.Empty);
        foreach (Iso639Language lang in Iso639LanguageCollection.Instance.Languages)
        {
          string code = lang.TerminologicCode;
          if (!lang.TerminologicCode.Equals(lang.BibliographicCode))
          {
            code += "," + lang.BibliographicCode;
          }
          ListViewItem item = new ListViewItem(new string[] { lang.Name, code });
          mpListView2.Items.Add(item);
          item.Tag = code;
          item.Checked = epgLanguages.IndexOf(code) >= 0;
        }
        mpListView2.Sort();
      }
      finally
      {
        mpListView2.EndUpdate();
      }
    }

    public override void OnSectionDeActivated()
    {      
      ServiceAgents.Instance.SettingServiceAgent.SaveValue(storeOnlySelectedSettingsKey, mpCheckBoxStoreOnlySelected.Checked);
      base.OnSectionDeActivated();
      SaveSettings();
    }

    private MediaTypeEnum _mediaTypeEnum;
    private bool _ignoreItemCheckedEvent = true;

    public override void OnSectionActivated()
    {
      mpListView1.BeginUpdate();
      try
      {
        _ignoreItemCheckedEvent = true;
        LoadLanguages();
        mpCheckBoxStoreOnlySelected.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue(storeOnlySelectedSettingsKey, false);

        Dictionary<int, CardType> cards = new Dictionary<int, CardType>();
        IList<Card> dbsCards = ServiceAgents.Instance.CardServiceAgent.ListAllCards(CardIncludeRelationEnum.None);
        foreach (Card card in dbsCards)
        {
          cards[card.IdCard] = ServiceAgents.Instance.ControllerServiceAgent.Type(card.IdCard);
        }
        base.OnSectionActivated();
        mpListView1.Items.Clear();

        ChannelIncludeRelationEnum includeRelations = ChannelIncludeRelationEnum.TuningDetails;
        includeRelations |= ChannelIncludeRelationEnum.ChannelMaps;
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
          if (ch.IsWebstream())
            continue;

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

          string imageName = null;
          if (_mediaTypeEnum == MediaTypeEnum.TV)
          {
            imageName = "tv_";
          }
          else if (_mediaTypeEnum == MediaTypeEnum.Radio)
          {
            imageName = "radio_";
          }
          
          if (hasFta && hasScrambled)
          {
            imageName += "scrambled_and_fta.png";
          }
          else if (hasScrambled)
          {
            imageName += "scrambled.png";
          }
          else
          {
            imageName += "fta_.png";
          }

          ListViewItem item = mpListView1.Items.Add(ch.DisplayName, imageName);
          foreach (ChannelMap map in ch.ChannelMaps)
          {
            if (cards.ContainsKey(map.IdCard))
            {
              CardType type = cards[map.IdCard];
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

      List<string> checkedCodes = new List<string>();
      foreach (ListViewItem i in mpListView2.Items)
      {
        if (i.Checked)
        {
          checkedCodes.Add((string)i.Tag);
        }
      }

      ServiceAgents.Instance.SettingServiceAgent.SaveValue(languagesSettingsKey, string.Join(",", checkedCodes));
      base.SaveSettings();
    }

    private void mpListView_ColumnClick(object sender, ColumnClickEventArgs e)
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
      ((MPListView)sender).Sort();
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

    private void linkLabelAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        foreach (ListViewItem i in mpListView1.Items)
        {
          i.Checked = true;
        }
      }
      finally
      {
        mpListView1.EndUpdate();
      }
    }

    private void linkLabelAllGrouped_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        _ignoreItemCheckedEvent = true;
        ICollection<Channel> channels = new List<Channel>();
        foreach (ListViewItem i in mpListView1.Items)
        {
          Channel ch = (Channel)i.Tag;
          i.Checked = (ch.GroupMaps.Count > 1);
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

    private void linkLabelGroupedVisible_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        _ignoreItemCheckedEvent = true;
        ICollection<Channel> channels = new List<Channel>();
        foreach (ListViewItem i in mpListView1.Items)
        {
          Channel ch = (Channel)i.Tag;
          i.Checked = (ch.GroupMaps.Count > 1 && ch.VisibleInGuide);
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

    private void linkLabelNone_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        _ignoreItemCheckedEvent = true;
        ICollection<Channel> channels = new List<Channel>();
        foreach (ListViewItem i in mpListView1.Items)
        {
          i.Checked = false;
          channels.Add(i.Tag as Channel);
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

    private void CheckAll(bool aChecked)
    {
      mpListView2.BeginUpdate();
      try
      {
        foreach (ListViewItem lv in mpListView2.Items)
        {
          lv.Checked = aChecked;
        }
      }
      finally
      {
        mpListView2.EndUpdate();
      }
    }

    private void linkLabelLanguageAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      CheckAll(true);
    }

    private void linkLabelLanguageNone_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      CheckAll(false);
    }
  }
}
