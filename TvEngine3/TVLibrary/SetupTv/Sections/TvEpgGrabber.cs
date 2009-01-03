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

    bool _loaded;

    private readonly MPListViewStringColumnSorter lvwColumnSorter;
    public TvEpgGrabber()
      : this("TV Epg grabber")
    {
    }

    public TvEpgGrabber(string name)
      : base(name)
    {
      InitializeComponent();
      lvwColumnSorter = new MPListViewStringColumnSorter();
      lvwColumnSorter.Order = SortOrder.None;
      mpListView1.ListViewItemSorter = lvwColumnSorter;
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
        if (ch.IsTv == false)
          continue;
        if (ch.IsWebstream())
          continue;
        int imageIndex = 1;
        if (ch.FreeToAir == false)
          imageIndex = 2;
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
        item.SubItems.Add(line);
        item.Checked = ch.GrabEpg;
        item.Tag = ch;

      }
      mpListView1.EndUpdate();
    }

    private void mpListView1_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      Channel channel = e.Item.Tag as Channel;
      if (channel == null)
        return;
      channel.GrabEpg = e.Item.Checked;
      channel.Persist();
    }

    private void mpButtonAll_Click(object sender, EventArgs e)
    {
      mpListView2.BeginUpdate();
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
      mpListView2.EndUpdate();
      setting.Persist();
    }

    private void mpButtonNone_Click(object sender, EventArgs e)
    {
      mpListView2.BeginUpdate();
      for (int i = 0; i < mpListView2.Items.Count; ++i)
      {
        mpListView2.Items[i].Checked = false;
      }
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("epgLanguages");
      setting.Value = ",";
      Log.WriteFile("tvsetup:epggrabber:none: epglang={0}", setting.Value);
      setting.Persist();
      mpListView2.EndUpdate();
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

    private void mpButtonAllChannels_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        mpListView1.Items[i].Checked = true;
      }
      mpListView1.EndUpdate();
    }

    private void mpButtonNoneChannels_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        mpListView1.Items[i].Checked = false;
      }
      mpListView1.EndUpdate();
    }

    private void mpButtonAllGrouped_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        Channel ch = (Channel)mpListView1.Items[i].Tag;
        mpListView1.Items[i].Checked = (ch.ReferringGroupMap().Count > 1); // if count > 1 we assume that the channel has one or more custom group(s) associated with it.
      }
      mpListView1.EndUpdate();
    }

    private void mpListView1_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        lvwColumnSorter.Order = lvwColumnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
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




  }
}
