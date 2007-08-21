/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using DirectShowLib.BDA;


using Gentle.Common;
using Gentle.Framework;
using TvDatabase;
using TvLibrary;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;

namespace SetupTv.Sections
{
  public partial class RadioChannels : SectionSettings
  {
    public class ListViewColumnSorter : IComparer
    {
      public enum OrderTypes
      {
        AsString,
        AsValue
      };
      public int SortColumn = 0;
      public SortOrder Order = SortOrder.Ascending;
      public OrderTypes OrderType = OrderTypes.AsString;

      public int Compare(object x, object y)
      {
        int compareResult = 0;
        ListViewItem listviewX, listviewY;
        // Cast the objects to be compared to ListViewItem objects
        listviewX = (ListViewItem)x;
        listviewY = (ListViewItem)y;
        switch (OrderType)
        {
          case OrderTypes.AsString:
            if (SortColumn == 0)
            {
              compareResult = String.Compare(listviewX.Text, listviewY.Text);
            }
            else
            {
              // Compare the two items
              compareResult = String.Compare(listviewX.SubItems[SortColumn].Text, listviewY.SubItems[SortColumn].Text);
            }
            break;
          case OrderTypes.AsValue:
            string line1, line2;
            if (SortColumn == 0) line1 = listviewX.Text;
            else line1 = listviewX.SubItems[SortColumn].Text;

            if (SortColumn == 0) line2 = listviewY.Text;
            else line2 = listviewY.SubItems[SortColumn].Text;
            int pos1 = line1.IndexOf("%"); line1 = line1.Substring(0, pos1);
            int pos2 = line2.IndexOf("%"); line2 = line2.Substring(0, pos2);
            float value1 = float.Parse(line1);
            float value2 = float.Parse(line2);
            if (value1 < value2)
              compareResult = -1;
            else if (value1 > value2)
              compareResult = 1;
            else
              compareResult = 0;
            break;

        }
        // Calculate correct return value based on object comparison
        if (Order == SortOrder.Ascending)
        {
          // Ascending sort is selected,
          // return normal result of compare operation
          return compareResult;
        }
        else if (Order == SortOrder.Descending)
        {
          // Descending sort is selected,
          // return negative result of compare operation
          return (-compareResult);
        }
        else
        {
          // Return '0' to indicate they are equal
          return 0;
        }
      }
    }

    private ListViewColumnSorter lvwColumnSorter;
    public RadioChannels()
      : this("Radio Stations")
    {
    }

    public RadioChannels(string name)
      : base(name)
    {
      InitializeComponent();
      lvwColumnSorter = new ListViewColumnSorter();
      lvwColumnSorter.Order = SortOrder.None;
      this.mpListView1.ListViewItemSorter = lvwColumnSorter;
    }

    public override void OnSectionActivated()
    {
      IList dbsCards = Card.ListAll();
      CountryCollection countries = new CountryCollection();
      Dictionary<int, CardType> cards = new Dictionary<int, CardType>();

      foreach (Card card in dbsCards)
      {
        cards[card.IdCard] = RemoteControl.Instance.Type(card.IdCard);
      }
      mpListView1.BeginUpdate();
      mpListView1.Items.Clear();
      IList chs = Channel.ListAll();
      int channelCount = 0;
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
      IList allmaps = ChannelMap.ListAll();


      List<ListViewItem> items = new List<ListViewItem>();
      foreach (Channel ch in channels)
      {
        bool analog = false;
        bool dvbc = false;
        bool dvbt = false;
        bool dvbs = false;
        bool atsc = false;
        bool notmapped = true;
        if (ch.IsRadio==false) continue;
        channelCount++;
        IList maps = ch.ReferringChannelMap();
        foreach (ChannelMap map in maps)
        {
          if (cards.ContainsKey(map.IdCard))
          {
            CardType type = cards[map.IdCard];
            switch (type)
            {
              case CardType.Analog: analog = true; notmapped = false; break;
              case CardType.DvbC: dvbc = true; notmapped = false; break;
              case CardType.DvbT: dvbt = true; notmapped = false; break;
              case CardType.DvbS: dvbs = true; notmapped = false; break;
              case CardType.Atsc: atsc = true; notmapped = false; break;
            }
          }
        }
        StringBuilder builder = new StringBuilder();

        if (analog)
        {
          builder.Append("Analog");
        }
        if (notmapped)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("Channel not mapped to a card");
        }
        if (dvbc)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("DVB-C");
        }
        if (dvbt)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("DVB-T");
        }
        if (dvbs)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("DVB-S");
        }
        if (atsc)
        {
          if (builder.Length > 0) builder.Append(",");
          builder.Append("ATSC");
        }
        int imageIndex = 0;
        if (ch.FreeToAir == false)
          imageIndex = 3;
        ListViewItem item = new ListViewItem(ch.DisplayName, imageIndex);
        item.SubItems.Add("-");
        item.Checked = ch.VisibleInGuide;
        item.Tag = ch;
        item.SubItems.Add(builder.ToString());
        string provider = "";
        foreach (TuningDetail detail in ch.ReferringTuningDetail())
        {
          provider += String.Format("{0},", detail.Provider);
          float frequency;
          switch (detail.ChannelType)
          {
            case 0://analog
              if (detail.VideoSource == (int)AnalogChannel.VideoInputType.Tuner)
              {
                frequency = detail.Frequency;
                frequency /= 1000000.0f;
                item.SubItems.Add(String.Format("#{0} {1} MHz", detail.ChannelNumber, frequency.ToString("f2")));
              }
              else
              {
                item.SubItems.Add(detail.VideoSource.ToString());
              }
              break;

            case 1://ATSC
              item.SubItems.Add(String.Format("{0} {1}:{2}", detail.ChannelNumber, detail.MajorChannel, detail.MinorChannel));
              break;

            case 2:// DVBC
              frequency = detail.Frequency;
              frequency /= 1000.0f;
              item.SubItems.Add(String.Format("{0} MHz SR:{1}", frequency.ToString("f2"), detail.Symbolrate));
              break;

            case 3:// DVBS
              frequency = detail.Frequency;
              frequency /= 1000.0f;
              item.SubItems.Add(String.Format("{0} MHz {1}", frequency.ToString("f2"), (((Polarisation)detail.Polarisation))));
              break;

            case 4:// DVBT
              frequency = detail.Frequency;
              frequency /= 1000.0f;
              item.SubItems.Add(String.Format("{0} MHz BW:{1}", frequency.ToString("f2"), detail.Bandwidth));
              break;
          }
        }
        if (provider.Length > 1) provider = provider.Substring(0, provider.Length - 1);
        item.SubItems[1].Text = (provider);
        items.Add(item);
      }
      mpListView1.Items.AddRange(items.ToArray());
      mpListView1.EndUpdate();
    }

    private void buttonDelete_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;
        channel.Delete();
        mpListView1.Items.Remove(item);
      }
      //DatabaseManager.Instance.SaveChanges();
      ReOrder();
      RemoteControl.Instance.OnNewSchedule();
    }

    private void mpListView1_ItemDrag(object sender, ItemDragEventArgs e)
    {
      ReOrder();
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

    void ReOrder()
    {
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        //mpListView1.Items[i].Text = (i + 1).ToString();

        Channel channel = (Channel)mpListView1.Items[i].Tag;
        if (channel.SortOrder != i)
        {
          channel.SortOrder = i;
          channel.Persist();
        }
      }
    }

    private void mpButtonEdit_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      Channel channel = (Channel)mpListView1.Items[indexes[0]].Tag;
      FormEditChannel dlg = new FormEditChannel();
      dlg.Channel = channel;
      dlg.ShowDialog(this);
      channel.Persist();
      OnSectionActivated();
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      ListView.SelectedIndexCollection indexes = mpListView1.SelectedIndices;
      if (indexes.Count == 0) return;
      if (mpListView1.Items.Count < 2) return;
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        ListViewItem item = mpListView1.Items[index];
        mpListView1.Items.RemoveAt(index);
        if (index + 1 < mpListView1.Items.Count)
          mpListView1.Items.Insert(index + 1, item);
        else
          mpListView1.Items.Add(item);
      }
      ReOrder();
      mpListView1.EndUpdate();
    }

    private void mpButtonDel_Click(object sender, EventArgs e)
    {
      mpListView1.BeginUpdate();
      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;
        channel.Delete();
        mpListView1.Items.Remove(item);
      }
      mpListView1.EndUpdate();
      ReOrder();
    }

    private void mpButtonAdd_Click(object sender, EventArgs e)
    {
      FormEditChannel dlg = new FormEditChannel();
      dlg.Channel = null;
      dlg.IsTv = false;
      dlg.ShowDialog(this);
      OnSectionActivated();
    }

    private void mpButtonDeleteEncrypted_Click(object sender, EventArgs e)
    {
      List<ListViewItem> itemsToRemove = new List<ListViewItem>();
      foreach (ListViewItem item in mpListView1.Items)
      {
        Channel channel = (Channel)item.Tag;
        if (channel.FreeToAir == false)
        {
          channel.Delete();
          itemsToRemove.Add(item);
        }
      }
      foreach (ListViewItem item in itemsToRemove)
        mpListView1.Items.Remove(item);
      ReOrder();
      RemoteControl.Instance.OnNewSchedule();
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
      ReOrder();
    }

    private void mpButtonClear_Click(object sender, EventArgs e)
    {
      IList channels = Channel.ListAll();
      foreach (Channel channel in channels)
      {
        if (channel.IsRadio)
        {
          Gentle.Framework.Broker.Execute("delete from TvMovieMapping WHERE idChannel=" + channel.IdChannel.ToString());
          channel.Delete();
        }
      }
      OnSectionActivated();
    }

    private void renameMarkedChannelsBySIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;
        IList details = channel.ReferringTuningDetail();
        if (details.Count > 0)
        {
          channel.DisplayName = ((TuningDetail)details[0]).ServiceId.ToString();
          channel.Persist();
          item.Tag = channel;
        }
      }
      OnSectionActivated();
    }

    private void addSIDInFrontOfNameToolStripMenuItem_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in mpListView1.SelectedItems)
      {
        Channel channel = (Channel)item.Tag;
        IList details = channel.ReferringTuningDetail();
        if (details.Count > 0)
        {
          channel.DisplayName = ((TuningDetail)details[0]).ServiceId.ToString() + " " + channel.DisplayName;
          channel.Persist();
          item.Tag = channel;
        }
      }
      OnSectionActivated();
    }
  }
}