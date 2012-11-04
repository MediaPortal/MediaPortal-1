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
using TvControl;
using Gentle.Framework;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  public partial class TvEpgGrabber : SectionSettings
  {
    private bool _loaded;
    private readonly MPListViewStringColumnSorter lvwColumnSorter;

    public TvEpgGrabber()
      : this("TV EPG Grabber") { }

    public TvEpgGrabber(string name)
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
        TvLibrary.Epg.Languages languages = new TvLibrary.Epg.Languages();
        List<String> codes = languages.GetLanguageCodes();
        List<String> list = languages.GetLanguages();

        TvBusinessLayer layer = new TvBusinessLayer();
        Setting setting = layer.GetSetting("epgLanguages");

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
          setting.Persist();
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
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("epgStoreOnlySelected");
      setting.Value = mpCheckBoxStoreOnlySelected.Checked ? "yes" : "no";
      setting.Persist();
      base.OnSectionDeActivated();
      SaveSettings();
    }

    public override void OnSectionActivated()
    {
      mpListView1.BeginUpdate();
      try
      {
        LoadLanguages();
        TvBusinessLayer layer = new TvBusinessLayer();
        Setting setting = layer.GetSetting("epgStoreOnlySelected");
        mpCheckBoxStoreOnlySelected.Checked = (setting.Value == "yes");
        Dictionary<string, CardType> cards = new Dictionary<string, CardType>();
        IList<Card> dbsCards = Card.ListAll();
        foreach (Card card in dbsCards)
        {
          cards[card.DevicePath] = RemoteControl.Instance.Type(card.IdCard);
        }
        base.OnSectionActivated();
        mpListView1.Items.Clear();

        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
        sb.AddOrderByField(true, "sortOrder");
        SqlStatement stmt = sb.GetStatement(true);
        IList<Channel> channels = ObjectFactory.GetCollection<Channel>(stmt.Execute());

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
          if (ch.IsTv == false)
            continue;
          if (ch.IsWebstream())
            continue;

          IList<TuningDetail> tuningDetails = ch.ReferringTuningDetail();
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
          foreach (ChannelMap map in ch.ReferringChannelMap())
          {
            if (cards.ContainsKey(map.ReferencedCard().DevicePath))
            {
              CardType type = cards[map.ReferencedCard().DevicePath];
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
      }
    }

    public override void SaveSettings()
    {
      if (false == _loaded)
        return;
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("epgLanguages");
      setting.Value = ",";
      for (int i = 0; i < mpListView2.Items.Count; ++i)
      {
        if (mpListView2.Items[i].Checked)
        {
          string code = (string)mpListView2.Items[i].Tag;
          setting.Value += code;
          setting.Value += ",";
        }
      }
      setting.Persist();
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
      Channel channel = e.Item.Tag as Channel;
      if (channel == null)
        return;
      channel.GrabEpg = e.Item.Checked;
      channel.Persist();
    }

    private void linkLabelTVAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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

    private void linkLabelTVAllGrouped_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        for (int i = 0; i < mpListView1.Items.Count; ++i)
        {
          Channel ch = (Channel)mpListView1.Items[i].Tag;
          mpListView1.Items[i].Checked = (ch.ReferringGroupMap().Count > 1);
          // if count > 1 we assume that the channel has one or more custom group(s) associated with it.
        }
      }
      finally
      {
        mpListView1.EndUpdate();
      }
    }

    private void linkLabelTVGroupedVisible_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        for (int i = 0; i < mpListView1.Items.Count; ++i)
        {
          Channel ch = (Channel)mpListView1.Items[i].Tag;
          mpListView1.Items[i].Checked = (ch.ReferringGroupMap().Count > 1 && ch.VisibleInGuide);
          // if count > 1 we assume that the channel has one or more custom group(s) associated with it.
        }
      }
      finally
      {
        mpListView1.EndUpdate();
      }
    }

    private void linkLabelTVNone_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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
        TvLibrary.Epg.Languages languages = new TvLibrary.Epg.Languages();
        List<String> codes = languages.GetLanguageCodes();
        TvBusinessLayer layer = new TvBusinessLayer();
        Setting setting = layer.GetSetting("epgLanguages");
        setting.Value = "";
        foreach (string code in codes)
        {
          setting.Value += code;
          setting.Value += ",";
        }
        //Log.WriteFile("tvsetup:epggrabber:all: epglang={0}", setting.Value);
        setting.Persist();
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
        TvBusinessLayer layer = new TvBusinessLayer();
        Setting setting = layer.GetSetting("epgLanguages");
        setting.Value = ",";
        Log.WriteFile("tvsetup:epggrabber:none: epglang={0}", setting.Value);
        setting.Persist();
      }
      finally
      {
        mpListView2.EndUpdate();
      }
    }
  }
}