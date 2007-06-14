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
        int imageIndex = 3;
        if (ch.FreeToAir == false)
          imageIndex = 0;
        ListViewItem item = mpListView1.Items.Add((mpListView1.Items.Count + 1).ToString(), imageIndex);
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
        details[0] = ch.Name;
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
        item.Tag = ch;
        item.SubItems.Add(details[0]);
        item.SubItems.Add(line);
        item.SubItems.Add(details[1]);
        item.SubItems.Add(details[2]);
        item.SubItems.Add(details[3]);
      }
      mpListView1.EndUpdate();
      ReOrder();
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
        mpListView1.Items[i].Text = (i + 1).ToString();

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
  }
}