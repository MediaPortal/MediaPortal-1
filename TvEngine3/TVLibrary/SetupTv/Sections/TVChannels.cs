/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;
using DirectShowLib;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

using TvDatabase;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
namespace SetupTv.Sections
{
  public partial class TvChannels : SectionSettings
  {
    public TvChannels()
      : this("TV Channels")
    {
    }

    public TvChannels(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionDeActivated()
    {
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        Channel ch = (Channel)mpListView1.Items[i].Tag;
        if (ch.SortOrder != i + 1)
        {
          ch.SortOrder = i + 1;
        }
        ch.VisibleInGuide = mpListView1.Items[i].Checked;
      }
      DatabaseManager.Instance.SaveChanges();
      base.OnSectionDeActivated();
    }
    public override void OnSectionActivated()
    {

      CountryCollection countries = new CountryCollection();
      Dictionary<string, CardType> cards = new Dictionary<string, CardType>();
      EntityList<Card> dbsCards = DatabaseManager.Instance.GetEntities<Card>();
      foreach (Card card in dbsCards)
      {
        cards[card.DevicePath] = RemoteControl.Instance.Type(card.IdCard);
      }
      base.OnSectionActivated();
      mpListView1.BeginUpdate();
      mpListView1.Items.Clear();

      int channelCount = 0;
      EntityQuery query = new EntityQuery(typeof(Channel));
      query.AddOrderBy(Channel.SortOrderEntityColumn);
      EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>(query);
      foreach (Channel ch in channels)
      {
        bool analog = false;
        bool dvbc = false;
        bool dvbt = false;
        bool dvbs = false;
        bool atsc = false;
        if (ch.IsTv == false) continue;
        channelCount++;
        ListViewItem item = mpListView1.Items.Add((mpListView1.Items.Count + 1).ToString());
        item.SubItems.Add(ch.Name);
        foreach (ChannelMap map in ch.ChannelMaps)
        {
          if (cards.ContainsKey(map.Card.DevicePath))
          {
            CardType type = cards[map.Card.DevicePath];
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
        details[0] = "";
        details[1] = "";
        details[2] = "";
        details[3] = "";
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
        item.Checked = ch.VisibleInGuide;
        item.Tag = ch;
        item.SubItems.Add(line);
        item.SubItems.Add(details[0]);
        item.SubItems.Add(details[1]);
        item.SubItems.Add(details[2]);
        item.SubItems.Add(details[3]);
      }
      mpListView1.EndUpdate();
      mpLabelChannelCount.Text = String.Format("Total channels:{0}", channelCount);
    }

    private void mpButtonClear_Click(object sender, EventArgs e)
    {

      mpListView1.BeginUpdate();
      EntityList<TuningDetail> details = DatabaseManager.Instance.GetEntities<TuningDetail>();
      while (details.Count > 0) details[0].Delete();
      EntityList<GroupMap> groupmaps = DatabaseManager.Instance.GetEntities<GroupMap>();
      while (groupmaps.Count > 0) groupmaps[0].Delete();
      EntityList<ChannelMap> channelMaps = DatabaseManager.Instance.GetEntities<ChannelMap>();
      while (channelMaps.Count > 0) channelMaps[0].Delete();
      EntityList<Recording> recordings = DatabaseManager.Instance.GetEntities<Recording>();
      while (recordings.Count > 0) recordings[0].Delete();
      EntityList<CanceledSchedule> canceled = DatabaseManager.Instance.GetEntities<CanceledSchedule>();
      while (canceled.Count > 0) canceled[0].Delete();
      EntityList<Schedule> schedules = DatabaseManager.Instance.GetEntities<Schedule>();
      while (schedules.Count > 0) schedules[0].Delete();
      EntityList<Program> progs = DatabaseManager.Instance.GetEntities<Program>();
      while (progs.Count > 0) progs[0].Delete();
      EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>();
      while (channels.Count > 0) channels[0].Delete();

      mpListView1.EndUpdate();
      DatabaseManager.Instance.SaveChanges();
      DatabaseManager.Instance.ClearQueryCache();
      OnSectionActivated();
    }

    private void TvChannels_Load(object sender, EventArgs e)
    {

    }

    private void mpButtonClearEncrypted_Click(object sender, EventArgs e)
    {
      //@ TODO : does not work
      EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>();
      channels.ShouldRemoveDeletedEntities = false;

      mpListView1.BeginUpdate();
      for (int i = 0; i < channels.Count; ++i)
      {
        if (channels[i].IsTv)
        {
          for (int x = 0; x < channels[i].TuningDetails.Count; x++)
          {
            if (channels[i].TuningDetails[x].FreeToAir == false)
            {
              channels[i].DeleteAll();
              break;
            }
          }
        }
      }
      mpListView1.EndUpdate();
      DatabaseManager.Instance.SaveChanges();
      OnSectionActivated();
    }

    private void mpButtonDel_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;
        channel.DeleteAll();
        mpListView1.Items.Remove(item);
      }
      mpListView1.EndUpdate();
      DatabaseManager.Instance.SaveChanges();
    }

    private void buttonUtp_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      for (int i = 0; i < indexes.Count; ++i)
      {
        int index = indexes[i];
        if (index > 0)
        {
          ListViewItem item = mpListView1.Items[index];
          mpListView1.Items.RemoveAt(index);
          mpListView1.Items.Insert(index - 1, item);
        }
      }
      ReOrder();
      mpListView1.EndUpdate();
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {

      mpListView1.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        if (index > 0)
        {
          ListViewItem item = mpListView1.Items[index];
          mpListView1.Items.RemoveAt(index);
          mpListView1.Items.Insert(index + 1, item);
        }
      }
      ReOrder();
      mpListView1.EndUpdate();
    }

    void ReOrder()
    {
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        mpListView1.Items[i].Text = (i + 1).ToString();

        Channel channel = (Channel)mpListView1.Items[i].Tag;
        channel.SortOrder = i;
      }
    }

    private void mpListView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
      try
      {
        int oldIndex = e.Item;
        ListViewItem item = mpListView1.Items[oldIndex];
        int newIndex = (Int32.Parse(e.Label) - 1);
        if (newIndex == oldIndex) return;

        mpListView1.Items.RemoveAt(oldIndex);
        mpListView1.Items.Insert(newIndex, item);
        ReOrder();
        e.CancelEdit = true;
      }
      catch (Exception)
      {
      }
    }

    private void mpButtonEdit_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      Channel channel = (Channel)mpListView1.Items[indexes[0]].Tag;
      FormEditChannel dlg = new FormEditChannel();
      dlg.Channel = channel;
      dlg.ShowDialog();
      DatabaseManager.SaveChanges();
      OnSectionActivated();
    }
  }
}