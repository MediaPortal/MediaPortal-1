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
using MediaPortal.UserInterface.Controls;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace SetupTv.Sections
{
  public partial class RadioChannelMapping : SectionSettings
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
        return _card.IdCard + " - " + _card.Name;
      }
    }

    private readonly MPListViewStringColumnSorter lvwColumnSorter1;
    private readonly MPListViewStringColumnSorter lvwColumnSorter2;

    public RadioChannelMapping()
      : this("Radio Mapping") {}

    public RadioChannelMapping(string name)
      : base(name)
    {
      InitializeComponent();
      lvwColumnSorter1 = new MPListViewStringColumnSorter();
      lvwColumnSorter1.Order = SortOrder.Ascending;
      mpListViewChannels.ListViewItemSorter = lvwColumnSorter1;
      lvwColumnSorter2 = new MPListViewStringColumnSorter();
      lvwColumnSorter2.Order = SortOrder.Ascending;
      mpListViewMapped.ListViewItemSorter = lvwColumnSorter2;
      //mpListViewChannels.ListViewItemSorter = new MPListViewSortOnColumn(0);
      //mpListViewMapped.ListViewItemSorter = new MPListViewSortOnColumn(0);
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


    private void mpButtonMap_Click_1(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Mapping selected channels to TV-Card...",
                                      "This can take some time\n\nPlease be patient...");
      dlg.Show(this);
      dlg.WaitForDisplay();
      Card card = ((CardInfo)mpComboBoxCard.SelectedItem).Card;
      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      try
      {
        ListView.SelectedListViewItemCollection selectedItems = mpListViewChannels.SelectedItems;
        TvBusinessLayer layer = new TvBusinessLayer();
        foreach (ListViewItem item in selectedItems)
        {
          Channel channel = (Channel)item.Tag;
          ChannelMap map = layer.MapChannelToCard(card, channel, false);
          mpListViewChannels.Items.Remove(item);
          ListViewItem newItem = mpListViewMapped.Items.Add(channel.DisplayName, item.ImageIndex);
          newItem.Tag = map;
        }
        dlg.Close();
      }
      finally
      {
        mpListViewChannels.EndUpdate();
        mpListViewMapped.EndUpdate();
      }
    }


    private void mpButtonUnmap_Click_1(object sender, EventArgs e)
    {
      NotifyForm dlg = new NotifyForm("Unmapping selected channels from TV-Card...",
                                      "This can take some time\n\nPlease be patient...");
      dlg.Show(this);
      dlg.WaitForDisplay();
      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      try
      {
        ListView.SelectedListViewItemCollection selectedItems = mpListViewMapped.SelectedItems;

        foreach (ListViewItem item in selectedItems)
        {
          ChannelMap map = (ChannelMap)item.Tag;
          mpListViewMapped.Items.Remove(item);
          Channel referencedChannel = map.ReferencedChannel();
          ListViewItem newItem = mpListViewChannels.Items.Add(referencedChannel.DisplayName, item.ImageIndex);
          newItem.Tag = referencedChannel;
          map.Remove();
        }
        mpListViewChannels.Sort();
        mpListViewMapped.Sort();
        dlg.Close();
      }
      finally
      {
        mpListViewChannels.EndUpdate();
        mpListViewMapped.EndUpdate();
      }
    }


    private void mpComboBoxCard_SelectedIndexChanged_1(object sender, EventArgs e)
    {
      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      try
      {
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
          if (channel.IsRadio == false)
            continue;


          List<TuningDetail> tuningDetails = GetTuningDetailsByCardType(channel, cardType, enableDVBS2);
          bool foundValidTuningDetail = tuningDetails.Count > 0;
          if (foundValidTuningDetail)
          {
            int imageIndex = GetImageIndex(tuningDetails);
            string displayName = channel.DisplayName;
            if (map.EpgOnly)
              displayName = channel.DisplayName + " (EPG Only)";
            ListViewItem item = new ListViewItem(displayName, imageIndex);
            item.Tag = map;
            items.Add(item);

            foreach (Channel ch in channels)
            {
              if (ch.IdChannel == channel.IdChannel)
              {
                //No risk of concurrent modification so remove it directly.
                channels.Remove(ch);
                break;
              }
            }
          }
          else
          {
            map.Delete();
          }
        }
        mpListViewMapped.Items.AddRange(items.ToArray());

        items = new List<ListViewItem>();
        foreach (Channel channel in channels)
        {
          if (!channel.IsRadio)
            continue;
          List<TuningDetail> tuningDetails = GetTuningDetailsByCardType(channel, cardType, enableDVBS2);
          // only add channel that is tuneable on the device selected.
          bool foundValidTuningDetail = tuningDetails.Count > 0;
          if (foundValidTuningDetail)
          {
            int imageIndex = GetImageIndex(tuningDetails);
            ListViewItem item = new ListViewItem(channel.DisplayName, imageIndex);
            item.Tag = channel;
            items.Add(item);
          }
        }
        mpListViewChannels.Items.AddRange(items.ToArray());
        mpListViewChannels.Sort();
      }
      finally
      {
        mpListViewChannels.EndUpdate();
        mpListViewMapped.EndUpdate();
      }
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

    private static List<TuningDetail> GetTuningDetailsByCardType(Channel channel, CardType cardType, bool enableDVBS2)
    {
      List<TuningDetail> result = new List<TuningDetail>();
      foreach (TuningDetail tDetail in channel.ReferringTuningDetail())
      {
        switch (cardType)
        {
          case CardType.Analog:
            if (tDetail.ChannelType == 0)
              result.Add(tDetail);
            break;
          case CardType.Atsc:
            if (tDetail.ChannelType == 1)
              result.Add(tDetail);
            break;
          case CardType.DvbC:
            if (tDetail.ChannelType == 2)
              result.Add(tDetail);
            break;
          case CardType.DvbS:
            if (tDetail.ChannelType == 3)
            {
              if (!enableDVBS2 && (tDetail.Pilot > -1 || tDetail.RollOff > -1))
              {
                Log.Debug(String.Format(
                  "Imported channel {0} detected as DVB-S2. Skipped! \n Enable \"DVB-S2 tuning\" option in your TV-Card properties to be able to map these channels.",
                  tDetail.Name));
              }
              else
              {
                result.Add(tDetail);
              }
            }
            break;
          case CardType.DvbT:
            if (tDetail.ChannelType == 4)
              result.Add(tDetail);
            break;
          case CardType.DvbIP:
            if (tDetail.ChannelType == 7)
              result.Add(tDetail);
            break;
          case CardType.RadioWebStream:
            if (tDetail.ChannelType == 5)
              result.Add(tDetail);
            break;
          default:
            break;
        }
      }
      return result;
    }

    private static int GetImageIndex(IList<TuningDetail> tuningDetails)
    {
      bool hasFta = false;
      bool hasScrambled = false;
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
        imageIndex = 2;
      }
      else if (hasScrambled)
      {
        imageIndex = 1;
      }
      else
      {
        imageIndex = 0;
      }
      return imageIndex;
    }
  }
}