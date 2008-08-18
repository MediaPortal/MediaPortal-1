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
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;
using DirectShowLib;

using Gentle.Common;
using Gentle.Framework;
using TvDatabase;
using TvLibrary;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  public partial class RadioEpgGrabber : SectionSettings
  {
    bool _loaded = false;
    private MPListViewStringColumnSorter lvwColumnSorter;
    public RadioEpgGrabber()
      : this("Radio Epg grabber")
    {
    }

    public RadioEpgGrabber(string name)
      : base(name)
    {
      InitializeComponent();
      lvwColumnSorter = new MPListViewStringColumnSorter();
      lvwColumnSorter.Order = SortOrder.None;
      this.mpListView1.ListViewItemSorter = lvwColumnSorter;
    }


    void LoadLanguages()
    {
      _loaded = true;
      mpListView2.BeginUpdate();
      mpListView2.Items.Clear();
      TvLibrary.Epg.Languages languages = new TvLibrary.Epg.Languages();
      List<String> codes = languages.GetLanguageCodes();
      List<String> list = languages.GetLanguages();

      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("radioLanguages");

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

      mpListView2.EndUpdate();
      if (setting.Value == "")
      {
        setting.Value = values;
        setting.Persist();
        //DatabaseManager.Instance.SaveChanges();
      }
    }
    public override void OnSectionDeActivated()
    {
      SaveSettings();
      base.OnSectionDeActivated();
    }

    public override void OnSectionActivated()
    {
      LoadLanguages();

      mpListView1.BeginUpdate();
      CountryCollection countries = new CountryCollection();
      Dictionary<string, CardType> cards = new Dictionary<string, CardType>();
      IList dbsCards = Card.ListAll();
      foreach (Card card in dbsCards)
      {
        cards[card.DevicePath] = RemoteControl.Instance.Type(card.IdCard);
      }
      base.OnSectionActivated();
      mpListView1.Items.Clear();

      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());

      foreach (Channel ch in channels)
      {
        bool analog = false;
        bool dvbc = false;
        bool dvbt = false;
        bool dvbs = false;
        bool atsc = false;
        if (ch.IsRadio == false) continue;
        if (ch.IsWebstream()) continue;
        if (ch.IsFMRadio()) continue;
        int imageIndex = 3;
        if (ch.FreeToAir == false)
          imageIndex = 0;
        ListViewItem item = mpListView1.Items.Add(ch.DisplayName, imageIndex);
        foreach (ChannelMap map in ch.ReferringChannelMap())
        {
          if (cards.ContainsKey(map.ReferencedCard().DevicePath))
          {
            CardType type = cards[map.ReferencedCard().DevicePath];
            switch (type)
            {
              case CardType.Analog: analog = true; break;
              case CardType.DvbC: dvbc = true; break;
              case CardType.DvbT: dvbt = true; break;
              case CardType.DvbS: dvbs = true; break;
              case CardType.Atsc: atsc = true; break;
            }
          }
        }
        string line = "";
        string[] details = new string[4];
        if (analog)
        {
          line += "Analog";
        }
        if (dvbc)
        {
          if (line != "") line += ",";
          line += "DVB-C";
        }
        if (dvbt)
        {
          if (line != "") line += ",";
          line += "DVB-T";
        }
        if (dvbs)
        {
          if (line != "") line += ",";
          line += "DVB-S";
        }
        if (atsc)
        {
          if (line != "") line += ",";
          line += "ATSC";
        }
        item.SubItems.Add(line);
        item.Checked = ch.GrabEpg;
        item.Tag = ch;

      }
      mpListView1.EndUpdate();
    }

    private void mpListView1_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      Channel channel = e.Item.Tag as Channel;
      if (channel == null) return;
      channel.GrabEpg = e.Item.Checked;
      channel.Persist();
    }

    private void mpButtonAll_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      for (int i = 0; i < mpListView2.Items.Count; ++i)
      {
        mpListView2.Items[i].Checked = true;
      }
      TvLibrary.Epg.Languages languages = new TvLibrary.Epg.Languages();
      List<String> codes = languages.GetLanguageCodes();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("radioLanguages");
      setting.Value = "";
      foreach (string code in codes)
      {
        setting.Value += code;
        setting.Value += ",";
      }
      mpListView1.EndUpdate();
    }

    private void mpButtonAllGrouped_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        Channel ch = (Channel)mpListView1.Items[i].Tag;
        mpListView1.Items[i].Checked = (ch.ReferringRadioGroupMap().Count > 1); // if count > 1 we assume that the channel has one or more custom group(s) associated with it.
      }
      mpListView1.EndUpdate();
    }

    private void mpButtonNone_Click(object sender, EventArgs e)
    {

      mpListView1.BeginUpdate();
      for (int i = 0; i < mpListView2.Items.Count; ++i)
      {
        mpListView2.Items[i].Checked = false;
      }
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("radioLanguages");
      setting.Value = ",";
      setting.Persist();
      mpListView1.EndUpdate();
    }


    public override void SaveSettings()
    {
      if (false == _loaded) return;
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("radioLanguages");
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

    public override void LoadSettings()
    {
      base.LoadSettings();
    }

    private void mpButtonAllChannels_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        mpListView1.Items[i].Checked = true;
      }
      mpListView1.EndUpdate();
    }

    private void mpButtonClearChannels_Click(object sender, EventArgs e)
    {

      mpListView1.BeginUpdate();
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        mpListView1.Items[i].Checked = false;
      }
      mpListView1.EndUpdate();
    }

    private void mpLabel2_Click(object sender, EventArgs e)
    {

    }

    private void mpListView1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpListView2_ItemChecked(object sender, ItemCheckedEventArgs e)
    {

    }

    private void mpListView1_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        if (lvwColumnSorter.Order == SortOrder.Ascending)
        {
          lvwColumnSorter.Order = SortOrder.Descending;
        }
        else
        {
          lvwColumnSorter.Order = SortOrder.Ascending;
        }
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter.SortColumn = e.Column;
        lvwColumnSorter.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      this.mpListView1.Sort();
    }
  }
}
