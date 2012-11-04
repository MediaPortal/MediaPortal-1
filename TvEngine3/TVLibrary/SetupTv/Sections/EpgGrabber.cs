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
using System.Text;
using System.Windows.Forms;
using TvControl;
using Gentle.Framework;
using TvDatabase;
using TvLibrary.Interfaces;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  public partial class EpgGrabber : SectionSettings
  {
    private bool _loaded;
    private readonly MPListViewStringColumnSorter lvwColumnSorter;
    private bool ignoreCheckChanges;
    private readonly string languagesSettingsKey;
    private readonly string storeOnlySelectedSettingsKey;
    private readonly bool isTV;

    private EpgGrabber() { }

    public EpgGrabber(string name, string languagesSettingsKey, string storeOnlySelectedSettingsKey, bool isTV)
      : base(name)
    {
      InitializeComponent();
      lvwColumnSorter = new MPListViewStringColumnSorter();
      lvwColumnSorter.Order = SortOrder.None;
      mpListView1.ListViewItemSorter = lvwColumnSorter;
      mpListView2.ListViewItemSorter = lvwColumnSorter;
      ignoreCheckChanges = true;
      this.languagesSettingsKey = languagesSettingsKey;
      this.storeOnlySelectedSettingsKey = storeOnlySelectedSettingsKey;
      this.isTV = isTV;
      this.tabPage1.Text = name;
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
        Setting setting = layer.GetSetting(languagesSettingsKey);

        for (int j = 0; j < list.Count; j++)
        {
          ListViewItem item = new ListViewItem(new string[] { list[j], codes[j] });
          mpListView2.Items.Add(item);
          item.Tag = codes[j];
          item.Checked = setting.Value.IndexOf((string)item.Tag) >= 0;
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
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting(storeOnlySelectedSettingsKey);
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
        Setting setting = layer.GetSetting(storeOnlySelectedSettingsKey);
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
          if (isTV)
          {
            if (ch.IsTv == false)
              continue;
          }
          else
          {
            if (ch.IsRadio == false)
              continue;
          }
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

          string imageName = isTV ? "tv_" : "radio_";
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
      Setting setting = layer.GetSetting(languagesSettingsKey);
      HashSet<string> selectedLanguages = new HashSet<string>();
      for (int i = 0; i < mpListView2.Items.Count; ++i)
      {
        if (mpListView2.Items[i].Checked)
        {
          string code = (string)mpListView2.Items[i].Tag;
          if (!selectedLanguages.Contains(code))
            selectedLanguages.Add(code);
        }
      }
      StringBuilder sb = new StringBuilder();
      sb.Append(',');
      foreach (string lang in selectedLanguages)
      {
        sb.Append(lang);
        sb.Append(',');
      }
      setting.Value = sb.ToString();
      setting.Persist();
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
      Channel channel = e.Item.Tag as Channel;
      if (channel == null)
        return;
      channel.GrabEpg = e.Item.Checked;
      channel.Persist();
    }

    private void linkLabelAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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

    private void linkLabelAllGrouped_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        for (int i = 0; i < mpListView1.Items.Count; ++i)
        {
          Channel ch = (Channel)mpListView1.Items[i].Tag;
          if (isTV)
            mpListView1.Items[i].Checked = (ch.ReferringGroupMap().Count > 1);
          else
            mpListView1.Items[i].Checked = (ch.ReferringRadioGroupMap().Count > 1);
          // if count > 1 we assume that the channel has one or more custom group(s) associated with it.
        }
      }
      finally
      {
        mpListView1.EndUpdate();
      }
    }

    private void linkLabelGroupedVisible_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      mpListView1.BeginUpdate();
      try
      {
        for (int i = 0; i < mpListView1.Items.Count; ++i)
        {
          Channel ch = (Channel)mpListView1.Items[i].Tag;
          if (isTV)
            mpListView1.Items[i].Checked = (ch.ReferringGroupMap().Count > 1 && ch.VisibleInGuide);
          else
            mpListView1.Items[i].Checked = (ch.ReferringRadioGroupMap().Count > 1 && ch.VisibleInGuide);
          // if count > 1 we assume that the channel has one or more custom group(s) associated with it.
        }
      }
      finally
      {
        mpListView1.EndUpdate();
      }
    }

    private void linkLabelNone_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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

    private void CheckAll(bool aChecked)
    {
      mpListView2.BeginUpdate();
      ignoreCheckChanges = true;
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
        ignoreCheckChanges = false;
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

    private void mpListView2_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      if (!ignoreCheckChanges)
      {
        ignoreCheckChanges = true;
        foreach (ListViewItem item in mpListView2.Items)
        {
          if (item != e.Item)
          {
            if (item.SubItems[1].Text == e.Item.SubItems[1].Text)
              item.Checked = e.Item.Checked;
          }
        }
        ignoreCheckChanges = false;
      }
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void EpgGrabber_Load(object sender, EventArgs e)
    {
      ignoreCheckChanges = false;
    }
  }
}
