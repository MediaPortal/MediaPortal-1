#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using TvLibrary.Interfaces;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  public partial class TvChannelMapping : SectionSettings
  {
    public class CardInfo
    {
      protected Card _card;

      public Card Card
      {
        get { return _card; }
      }

      public CardInfo(Card card)
      {
        _card = card;
      }

      public override string ToString()
      {
        return _card.Name;
      }
    }

    public TvChannelMapping()
      : this("TV Mapping") {}

    private readonly MPListViewStringColumnSorter lvwColumnSorter1;
    private readonly MPListViewStringColumnSorter lvwColumnSorter2;

    public TvChannelMapping(string name)
      : base(name)
    {
      InitializeComponent();
      //mpListViewChannels.ListViewItemSorter = new MPListViewSortOnColumn(1);
      lvwColumnSorter1 = new MPListViewStringColumnSorter();
      lvwColumnSorter1.Order = SortOrder.None;
      mpListViewChannels.ListViewItemSorter = lvwColumnSorter1;
      lvwColumnSorter2 = new MPListViewStringColumnSorter();
      lvwColumnSorter2.Order = SortOrder.None;
      mpListViewMapped.ListViewItemSorter = lvwColumnSorter2;
    }

    public override void OnSectionActivated()
    {
      mpComboBoxCard.Items.Clear();
      IList<Card> cards = Card.ListAll();
      foreach (Card card in cards)
      {
        if (card.Enabled == false)
          continue;
        if (!RemoteControl.Instance.CardPresent(card.IdCard))
          continue;
        mpComboBoxCard.Items.Add(new CardInfo(card));
      }
      if (mpComboBoxCard.Items.Count > 0)
        mpComboBoxCard.SelectedIndex = 0;
    }

    private void mpButtonMap_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Mapping selected channels to TV-Card...",
                                      "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      Card card = ((CardInfo)mpComboBoxCard.SelectedItem).Card;
      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      ListView.SelectedListViewItemCollection selectedItems = mpListViewChannels.SelectedItems;
      TvBusinessLayer layer = new TvBusinessLayer();
      foreach (ListViewItem item in selectedItems)
      {
        Channel channel = (Channel)item.Tag;
        ChannelMap map = layer.MapChannelToCard(card, channel, mpCheckBoxMapForEpgOnly.Checked);
        mpListViewChannels.Items.Remove(item);

        int imageIndex = 1;
        if (channel.FreeToAir == false)
          imageIndex = 2;
        string displayName = channel.DisplayName;
        if (mpCheckBoxMapForEpgOnly.Checked)
          displayName = channel.DisplayName + " (EPG Only)";
        ListViewItem newItem = mpListViewMapped.Items.Add(displayName, imageIndex);
        newItem.Tag = map;
      }
      dlg.Close();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
      //DatabaseManager.Instance.SaveChanges();
    }

    private void mpButtonUnmap_Click(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Unmapping selected channels from TV-Card...",
                                      "This can take some time\n\nPlease be patient...");
      dlg.Show();
      dlg.WaitForDisplay();
      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();

      ListView.SelectedListViewItemCollection selectedItems = mpListViewMapped.SelectedItems;

      foreach (ListViewItem item in selectedItems)
      {
        ChannelMap map = (ChannelMap)item.Tag;
        mpListViewMapped.Items.Remove(item);


        int imageIndex = 1;
        if (map.ReferencedChannel().FreeToAir == false)
          imageIndex = 2;
        ListViewItem newItem = mpListViewChannels.Items.Add(map.ReferencedChannel().DisplayName, imageIndex);
        newItem.Tag = map.ReferencedChannel();


        map.Remove();
      }
      mpListViewChannels.Sort();
      dlg.Close();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
      // DatabaseManager.Instance.SaveChanges();
    }

    private void mpComboBoxCard_SelectedIndexChanged(object sender, EventArgs e)
    {
      //DatabaseManager.Instance.SaveChanges();

      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      mpListViewMapped.Items.Clear();
      mpListViewChannels.Items.Clear();

      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Channel));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList<Channel> channels = ObjectFactory.GetCollection<Channel>(stmt.Execute());

      Card card = ((CardInfo)mpComboBoxCard.SelectedItem).Card;
      IList<ChannelMap> maps = card.ReferringChannelMap();

      // get cardtype, dvb, analogue etc.		
      CardType cardType = RemoteControl.Instance.Type(card.IdCard);
      //Card card = Card.Retrieve(card.IdCard);
      TvBusinessLayer layer = new TvBusinessLayer();
      bool enableDVBS2 = (layer.GetSetting("dvbs" + card.IdCard + "enabledvbs2", "false").Value == "true");


      List<ListViewItem> items = new List<ListViewItem>();
      foreach (ChannelMap map in maps)
      {
        Channel channel = null;
        try
        {
          channel = map.ReferencedChannel();
        }
        catch (Exception) {}
        if (channel == null)
          continue;
        if (channel.IsTv == false)
          continue;
        int imageIndex = 1;
        if (channel.FreeToAir == false)
          imageIndex = 2;
        string displayName = channel.DisplayName;
        if (map.EpgOnly)
          displayName = channel.DisplayName + " (EPG Only)";
        ListViewItem item = new ListViewItem(displayName, imageIndex);
        item.Tag = map;
        items.Add(item);
        bool remove = false;
        foreach (Channel ch in channels)
        {
          if (ch.IdChannel == channel.IdChannel)
          {
            remove = true;
            break;
          }
        }
        if (remove)
        {
          channels.Remove(channel);
        }
      }
      mpListViewMapped.Items.AddRange(items.ToArray());
      items = new List<ListViewItem>();
      foreach (Channel channel in channels)
      {
        if (channel.IsTv == false)
          continue;
        if (channel.IsWebstream())
          continue;

        // only add channels that is tuneable on the device selected.
        bool foundValidTuningDetail = false;
        foreach (TuningDetail tDetail in channel.ReferringTuningDetail())
        {
          switch (cardType)
          {
            case CardType.Analog:
              foundValidTuningDetail = (tDetail.ChannelType == 0);
              break;

            case CardType.Atsc:
              foundValidTuningDetail = (tDetail.ChannelType == 1);
              break;

            case CardType.DvbC:
              foundValidTuningDetail = (tDetail.ChannelType == 2);
              break;

            case CardType.DvbS:

              if (!enableDVBS2 && (tDetail.Pilot > -1 || tDetail.RollOff > -1))
              {
                continue;
              }

              foundValidTuningDetail = (tDetail.ChannelType == 3);
              break;

            case CardType.DvbT:
              foundValidTuningDetail = (tDetail.ChannelType == 4);
              break;

            case CardType.DvbIP:
              foundValidTuningDetail = (tDetail.ChannelType == 7);
              break;

            case CardType.RadioWebStream:
              foundValidTuningDetail = (tDetail.ChannelType == 5);
              break;

            default:
              foundValidTuningDetail = true;
              break;
          }

          if (foundValidTuningDetail)
          {
            break;
          }
        }
        if (!foundValidTuningDetail)
        {
          continue;
        }

        int imageIndex = 1;
        if (channel.FreeToAir == false)
          imageIndex = 2;
        ListViewItem item = new ListViewItem(channel.DisplayName, imageIndex);
        item.Tag = channel;
        items.Add(item);
      }
      mpListViewChannels.Items.AddRange(items.ToArray());
      mpListViewChannels.Sort();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
    }

    private void mpListViewChannels_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter1.SortColumn)
      {
        // Reverse the current sort direction for this column.
        lvwColumnSorter1.Order = lvwColumnSorter1.Order == SortOrder.Ascending
                                   ? SortOrder.Descending
                                   : SortOrder.Ascending;
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter1.SortColumn = e.Column;
        lvwColumnSorter1.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      mpListViewChannels.Sort();
    }

    private void mpListViewMapped_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter2.SortColumn)
      {
        // Reverse the current sort direction for this column.
        lvwColumnSorter2.Order = lvwColumnSorter2.Order == SortOrder.Ascending
                                   ? SortOrder.Descending
                                   : SortOrder.Ascending;
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter2.SortColumn = e.Column;
        lvwColumnSorter2.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      mpListViewMapped.Sort();
    }

    private void mpListViewMapped_DoubleClick(object sender, EventArgs e)
    {
      if (mpListViewMapped.SelectedItems.Count == 0)
        return;
      ListViewItem item = mpListViewMapped.SelectedItems[0];
      ChannelMap map = (ChannelMap)item.Tag;
      Channel channel = map.ReferencedChannel();
      if (map.EpgOnly)
      {
        item.Text = channel.DisplayName;
        map.EpgOnly = false;
      }
      else
      {
        item.Text = channel.DisplayName + " (EPG Only)";
        map.EpgOnly = true;
      }
      map.Persist();
    }
  }
}