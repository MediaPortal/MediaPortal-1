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
using System.Globalization;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using TvControl;
using DirectShowLib;

using Gentle.Common;
using Gentle.Framework;
using DirectShowLib.BDA;
using TvDatabase;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvLibrary.Channels;
using similaritymetrics;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  public partial class TvCombinations : SectionSettings
  {
    public class CardInfo
    {
      protected Card _card;

      public Card Card
      {
        get
        {
          return _card;
        }
      }

      public CardInfo(Card card)
      {
        _card = card;
      }

      public override string ToString()
      {
        return _card.Name.ToString();
      }
    }
    private MPListViewStringColumnSorter lvwColumnSorter;
    private MPListViewStringColumnSorter lvwColumnSorter2;
    private MPListViewStringColumnSorter lvwColumnSorter3;

    bool _redrawTab1 = false;
    public TvCombinations()
      : this("Combinations")
    {
    }

    public TvCombinations(string name)
      : base(name)
    {
      InitializeComponent();

      lvwColumnSorter2 = new MPListViewStringColumnSorter();
      lvwColumnSorter3 = new MPListViewStringColumnSorter();
      lvwColumnSorter2.Order = SortOrder.Descending;
      lvwColumnSorter2.OrderType = MPListViewStringColumnSorter.OrderTypes.AsValue;
      this.mpListViewMapped.ListViewItemSorter = lvwColumnSorter2;
      this.mpListViewChannels.ListViewItemSorter = lvwColumnSorter3;

    }

    public override void OnSectionDeActivated()
    {

      //DatabaseManager.Instance.SaveChanges();
      RemoteControl.Instance.OnNewSchedule();
      base.OnSectionDeActivated();
    }

    public override void OnSectionActivated()
    {
      _redrawTab1 = false;
      mpComboBoxCard.Items.Clear();
      IList dbsCards = Card.ListAll();
      foreach (Card card in dbsCards)
      {
        mpComboBoxCard.Items.Add(new CardInfo(card));
      }
      mpComboBoxCard.SelectedIndex = 0;

      CountryCollection countries = new CountryCollection();
      Dictionary<int, CardType> cards = new Dictionary<int, CardType>();

      foreach (Card card in dbsCards)
      {
        cards[card.IdCard] = RemoteControl.Instance.Type(card.IdCard);
      }
      base.OnSectionActivated();

      
    }

    
    private void TvCombinations_Load(object sender, EventArgs e)
    {

    }

    private void mpButtonUnmap_Click(object sender, EventArgs e)
    {

    }

    private void mpComboBoxCard_SelectedIndexChanged(object sender, EventArgs e)
    {
      mpListViewChannels.BeginUpdate();
      mpListViewChannels.Items.Clear();
      mpListViewMapped.Items.Clear();

      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());

      Card card = ((CardInfo)mpComboBoxCard.SelectedItem).Card;
      IList maps = card.ReferringChannelMap();


      List<ListViewItem> items = new List<ListViewItem>();
      foreach (ChannelMap map in maps)
      {
        Channel channel = null;
        try
        {
          channel = map.ReferencedChannel();
        }
        catch (Exception)
        {
        }
        if (channel == null)
          continue;
        if (channel.IsTv == false) continue;
        int imageIndex = 1;
        if (channel.FreeToAir == false)
          imageIndex = 2;
        ListViewItem item = new ListViewItem(channel.DisplayName, imageIndex);
        item.Tag = channel;
        items.Add(item);
      }
      mpListViewChannels.Items.AddRange(items.ToArray());
      mpListViewChannels.EndUpdate();
      mpListViewChannels.Sort();
    }

    private void mpListViewChannels_SelectedIndexChanged(object sender, EventArgs e)
    {
      mpListViewMapped.Items.Clear();
      if (mpListViewChannels.SelectedIndices == null) return;
      if (mpListViewChannels.SelectedIndices.Count != 1) return;
      Card card = ((CardInfo)mpComboBoxCard.SelectedItem).Card;
      ListViewItem selectedItem = mpListViewChannels.Items[mpListViewChannels.SelectedIndices[0]];
      Channel selectedChannel = (Channel)selectedItem.Tag;
      IList allChannels = Channel.ListAll();
      List<ListViewItem> items = new List<ListViewItem>();
      NotifyForm dlg = new NotifyForm("Searching for Similar Channels...", "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      foreach (Channel channel in allChannels)
      {
        if (channel.IsTv == false) continue;
        IList details = channel.ReferringTuningDetail();
        if (details != null)
        {
          if (details.Count > 0)
          {
            TuningDetail detail = (TuningDetail)details[0];
            if (detail.ChannelType == 5)
              continue;
          }
        }
        bool isMapped = false;
        IList list = channel.ReferringChannelMap();
        foreach (ChannelMap map in list)
        {
          if (map.IdCard == card.IdCard)
          {
            isMapped = true;
            break;
          }
        }
        if (isMapped) continue;
        Levenstein comparer = new Levenstein();
        float result = comparer.getSimilarity(selectedChannel.DisplayName, channel.DisplayName);


        int imageIndex = 1;
        if (channel.FreeToAir == false)
          imageIndex = 2;
        ListViewItem item = new ListViewItem((result * 100f).ToString("f2") + "%", imageIndex);
        item.Tag = channel;
        item.SubItems.Add(channel.DisplayName);
        items.Add(item);
      }
      mpListViewMapped.Items.AddRange(items.ToArray());
      mpListViewMapped.Sort();
      dlg.Close();
    }

    private void btnCombine_Click(object sender, EventArgs e)
    {
      if (mpListViewChannels.SelectedIndices == null) return;
      if (mpListViewChannels.SelectedIndices.Count != 1) return;
      if (mpListViewMapped.SelectedIndices == null) return;
      if (mpListViewMapped.SelectedIndices.Count != 1) return;


      Card card = ((CardInfo)mpComboBoxCard.SelectedItem).Card;

      ListViewItem selectedItem = mpListViewChannels.Items[mpListViewChannels.SelectedIndices[0]];
      Channel selectedChannel = (Channel)selectedItem.Tag;

      ListViewItem selectedItem2 = mpListViewMapped.Items[mpListViewMapped.SelectedIndices[0]];
      Channel selectedChannel2 = (Channel)selectedItem2.Tag;

      TvBusinessLayer layer = new TvBusinessLayer();
      NotifyForm dlg = new NotifyForm("Combining Channels...", "Updating TuningDetail Table\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      foreach (TuningDetail detail in selectedChannel2.ReferringTuningDetail())
      {
        detail.IdChannel = selectedChannel.IdChannel;
        detail.Persist();
      }
      dlg.Close();
      dlg = new NotifyForm("Combining Channels...", "Updating ChannelMap Table\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      foreach (ChannelMap map in selectedChannel2.ReferringChannelMap())
      {
        map.IdChannel = selectedChannel.IdChannel;
        map.Persist();
      }
      dlg.Close();
      dlg = new NotifyForm("Combining Channels...", "Updating GroupMap Table\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      foreach (GroupMap groupMap in selectedChannel2.ReferringGroupMap())
      {
        groupMap.IdChannel = selectedChannel.IdChannel;
        groupMap.Persist();
      }
      dlg.Close();
      dlg = new NotifyForm("Combining Channels...", "Updating Program Table\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      foreach (Program program in selectedChannel2.ReferringProgram())
      {
        program.IdChannel = selectedChannel.IdChannel;
        program.Persist();
      }
      dlg.Close();
      dlg = new NotifyForm("Combining Channels...", "Updating Recording Table\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      foreach (Recording recording in selectedChannel2.ReferringRecording())
      {
        recording.IdChannel = selectedChannel.IdChannel;
        recording.Persist();
      }
      dlg.Close();
      dlg = new NotifyForm("Combining Channels...", "Updating Schedule Table\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      foreach (Schedule schedule in selectedChannel2.ReferringSchedule())
      {
        schedule.IdChannel = selectedChannel.IdChannel;
        schedule.Persist();
      }
      dlg.Close();
      selectedChannel2.Remove();

      mpListViewChannels_SelectedIndexChanged(null, null);
      _redrawTab1 = true;
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (tabControl1.SelectedIndex == 0)
      {
        if (_redrawTab1)
        {
          OnSectionActivated();
        }
      }
    }

    private void mpListViewMapped_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpListViewChannels_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter3.SortColumn)
      {
        // Reverse the current sort direction for this column.
        if (lvwColumnSorter3.Order == SortOrder.Ascending)
        {
          lvwColumnSorter3.Order = SortOrder.Descending;
        }
        else
        {
          lvwColumnSorter3.Order = SortOrder.Ascending;
        }
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter3.SortColumn = e.Column;
        lvwColumnSorter3.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      this.mpListViewChannels.Sort();
    }

    private void mpListViewMapped_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter2.SortColumn)
      {
        // Reverse the current sort direction for this column.
        if (lvwColumnSorter2.Order == SortOrder.Ascending)
        {
          lvwColumnSorter2.Order = SortOrder.Descending;
        }
        else
        {
          lvwColumnSorter2.Order = SortOrder.Ascending;
        }
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter2.SortColumn = e.Column;
        lvwColumnSorter2.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      this.mpListViewMapped.Sort();
    }
  }
}

 	  	 
