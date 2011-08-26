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
using TvLibrary.Interfaces;
using similaritymetrics;
using MediaPortal.UserInterface.Controls;
using TvLibrary.Log;

namespace SetupTv.Sections
{
  public partial class TvCombinations : SectionSettings
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

    private readonly MPListViewStringColumnSorter lvwColumnSorter2;
    private readonly MPListViewStringColumnSorter lvwColumnSorter3;

    private Dictionary<int, CardType> cards;

    public TvCombinations(string name)
      : base(name)
    {
      InitializeComponent();

      lvwColumnSorter2 = new MPListViewStringColumnSorter();
      lvwColumnSorter3 = new MPListViewStringColumnSorter();
      lvwColumnSorter3.Order = SortOrder.Ascending;
      lvwColumnSorter2.Order = SortOrder.Descending;
      lvwColumnSorter2.OrderType = MPListViewStringColumnSorter.OrderTypes.AsValue;
      mpListViewMapped.ListViewItemSorter = lvwColumnSorter2;
      mpListViewChannels.ListViewItemSorter = lvwColumnSorter3;
    }

    public override void OnSectionDeActivated()
    {
      //DatabaseManager.Instance.SaveChanges();
      RemoteControl.Instance.OnNewSchedule();
      base.OnSectionDeActivated();
    }

    public override void OnSectionActivated()
    {
      cards = new Dictionary<int, CardType>();
      IList<Card> dbsCards = Card.ListAll();
      mpComboBoxCard.Items.Clear();
      foreach (Card card in dbsCards)
      {
        if (card.Enabled == false)
          continue;
        if (!RemoteControl.Instance.CardPresent(card.IdCard))
          continue;
        cards[card.IdCard] = RemoteControl.Instance.Type(card.IdCard);
        mpComboBoxCard.Items.Add(new CardInfo(card));
      }
      mpComboBoxCard.SelectedIndex = 0;
      base.OnSectionActivated();
    }


    private void TvCombinations_Load(object sender, EventArgs e) {}

    private void mpComboBoxCard_SelectedIndexChanged(object sender, EventArgs e)
    {
      mpListViewChannels.BeginUpdate();
      try
      {
        mpListViewChannels.Items.Clear();
        mpListViewMapped.Items.Clear();

        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Channel));
        sb.AddOrderByField(true, "sortOrder");
        CardInfo cardInfo = (CardInfo)mpComboBoxCard.SelectedItem;

        Card card = cardInfo.Card;
        CardType cardType = cards[card.IdCard];
        IList<ChannelMap> maps = card.ReferringChannelMap();


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
          TvBusinessLayer layer = new TvBusinessLayer();
          bool enableDVBS2 = (layer.GetSetting("dvbs" + card.IdCard + "enabledvbs2", "false").Value == "true");
          List<TuningDetail> tuningDetails = GetTuningDetailsByCardType(channel, cardType, enableDVBS2);
          int imageIndex = GetImageIndex(tuningDetails);
          ListViewItem item = new ListViewItem(channel.DisplayName, imageIndex);
          item.Tag = channel;
          items.Add(item);
        }
        mpListViewChannels.Items.AddRange(items.ToArray());
        mpListViewChannels.Sort();
      }
      finally
      {
        mpListViewChannels.EndUpdate();
      }
    }

    private void mpListViewChannels_SelectedIndexChanged(object sender, EventArgs e)
    {
      mpListViewMapped.BeginUpdate();
      try
      {
        mpListViewMapped.Items.Clear();
        if (mpListViewChannels.SelectedIndices == null)
          return;
        if (mpListViewChannels.SelectedIndices.Count != 1)
          return;
        Card card = ((CardInfo)mpComboBoxCard.SelectedItem).Card;
        ListViewItem selectedItem = mpListViewChannels.Items[mpListViewChannels.SelectedIndices[0]];
        Channel selectedChannel = (Channel)selectedItem.Tag;
        IList<Channel> allChannels = Channel.ListAll();
        List<ListViewItem> items = new List<ListViewItem>();
        NotifyForm dlg = new NotifyForm("Searching for Similar Channels...",
                                        "This can take some time\n\nPlease be patient...");
        dlg.Show(this);
        dlg.WaitForDisplay();
        foreach (Channel channel in allChannels)
        {
          if (channel.IsTv == false)
            continue;

          bool isMapped = false;
          IList<ChannelMap> list = channel.ReferringChannelMap();
          foreach (ChannelMap map in list)
          {
            if (map.IdCard == card.IdCard)
            {
              isMapped = true;
              break;
            }
          }
          if (isMapped)
            continue;

          Levenstein comparer = new Levenstein();
          float result = comparer.getSimilarity(selectedChannel.DisplayName, channel.DisplayName);

          IList<TuningDetail> details = channel.ReferringTuningDetail();
          int imageIndex = GetImageIndex(details);
          ListViewItem item = new ListViewItem((result * 100f).ToString("f2") + "%", imageIndex);
          item.Tag = channel;
          item.SubItems.Add(channel.DisplayName);
          items.Add(item);
        }
        mpListViewMapped.Items.AddRange(items.ToArray());
        mpListViewMapped.Sort();
        dlg.Close();
      }
      finally
      {
        mpListViewMapped.EndUpdate();
      }
    }

    private void btnCombine_Click(object sender, EventArgs e)
    {
      if (mpListViewChannels.SelectedIndices == null)
        return;
      if (mpListViewChannels.SelectedIndices.Count != 1)
        return;
      if (mpListViewMapped.SelectedIndices == null)
        return;

      ListViewItem selectedItem = mpListViewChannels.Items[mpListViewChannels.SelectedIndices[0]];
      Channel selectedChannel = (Channel)selectedItem.Tag;

      ListView.SelectedListViewItemCollection selectedItemsToCombine = mpListViewMapped.SelectedItems;

      foreach (ListViewItem listViewItem in selectedItemsToCombine)
      {
        Channel selectedChannel2 = (Channel)listViewItem.Tag;
        NotifyForm dlg = new NotifyForm("Combining Channels...", "Updating TuningDetail Table\n\nPlease be patient...");
        dlg.Show(this);
        dlg.WaitForDisplay();
        foreach (TuningDetail detail in selectedChannel2.ReferringTuningDetail())
        {
          detail.IdChannel = selectedChannel.IdChannel;
          detail.Persist();
        }
        dlg.Close();
        dlg = new NotifyForm("Combining Channels...", "Updating ChannelMap Table\n\nPlease be patient...");
        dlg.Show(this);
        dlg.WaitForDisplay();
        foreach (ChannelMap map in selectedChannel2.ReferringChannelMap())
        {
          map.IdChannel = selectedChannel.IdChannel;
          map.Persist();
        }
        dlg.Close();
        dlg = new NotifyForm("Combining Channels...", "Updating GroupMap Table\n\nPlease be patient...");
        dlg.Show(this);
        dlg.WaitForDisplay();
        foreach (GroupMap groupMap in selectedChannel2.ReferringGroupMap())
        {
          bool alreadyExistsInGroup = false;
          foreach (GroupMap groupMapMaster in selectedChannel.ReferringGroupMap())
          {
            if (groupMapMaster.IdGroup == groupMap.IdGroup)
            {
              groupMap.Remove();
              alreadyExistsInGroup = true;
              continue;
            }
          }
          if (!alreadyExistsInGroup)
          {
            groupMap.IdChannel = selectedChannel.IdChannel;
            groupMap.Persist();
          }
        }
        dlg.Close();
        dlg = new NotifyForm("Combining Channels...", "Updating Program Table\n\nPlease be patient...");
        dlg.Show(this);
        dlg.WaitForDisplay();
        foreach (Program program in selectedChannel2.ReferringProgram())
        {
          program.IdChannel = selectedChannel.IdChannel;
          program.Persist();
        }
        dlg.Close();
        dlg = new NotifyForm("Combining Channels...", "Updating Recording Table\n\nPlease be patient...");
        dlg.Show(this);
        dlg.WaitForDisplay();
        foreach (Recording recording in selectedChannel2.ReferringRecording())
        {
          recording.IdChannel = selectedChannel.IdChannel;
          recording.Persist();
        }
        dlg.Close();
        dlg = new NotifyForm("Combining Channels...", "Updating Schedule Table\n\nPlease be patient...");
        dlg.Show(this);
        dlg.WaitForDisplay();
        foreach (Schedule schedule in selectedChannel2.ReferringSchedule())
        {
          schedule.IdChannel = selectedChannel.IdChannel;
          schedule.Persist();
        }
        dlg.Close();
        selectedChannel2.Remove();
        mpListViewMapped.Items.Remove(listViewItem);
      }
    }

    private void mpListViewChannels_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter3.SortColumn)
      {
        // Reverse the current sort direction for this column.
        lvwColumnSorter3.Order = lvwColumnSorter3.Order == SortOrder.Ascending
                                   ? SortOrder.Descending
                                   : SortOrder.Ascending;
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter3.SortColumn = e.Column;
        lvwColumnSorter3.Order = SortOrder.Ascending;
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
                  "Imported channel {0} detected as DVB-S2. Skipped! \n Enable \"DVB-S2 tuning\" option in your TV-Card properties to be able to combine these channels.",
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
      return imageIndex;
    }
  }
}