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
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
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

    public TvChannelMapping()
      : this("TV Mapping")
    {
    }

    public TvChannelMapping(string name)
      : base(name)
    {
      InitializeComponent();
      mpListViewChannels.ListViewItemSorter = new MPListViewSortOnColumn(1);
    }

    public override void OnSectionActivated()
    {
      mpComboBoxCard.Items.Clear();
      EntityList<Card> cards = DatabaseManager.Instance.GetEntities<Card>();
      foreach (Card card in cards)
      {
        mpComboBoxCard.Items.Add(new CardInfo(card));
      }
      mpComboBoxCard.SelectedIndex = 0;
    }

    private void mpButtonMap_Click(object sender, EventArgs e)
    {
      Card card = ((CardInfo)mpComboBoxCard.SelectedItem).Card;

      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      ListView.SelectedListViewItemCollection selectedItems = mpListViewChannels.SelectedItems;
      TvBusinessLayer layer = new TvBusinessLayer();
      foreach (ListViewItem item in selectedItems)
      {
        Channel channel = (Channel)item.Tag;
        ChannelMap map=layer.MapChannelToCard(card, channel);
        mpListViewChannels.Items.Remove(item);

        ListViewItem newItem = mpListViewMapped.Items.Add(channel.Name);
        newItem.Tag = map;
      }
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
      DatabaseManager.Instance.SaveChanges();
    }

    private void mpButtonUnmap_Click(object sender, EventArgs e)
    {
      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      ListView.SelectedListViewItemCollection selectedItems = mpListViewMapped.SelectedItems;

      foreach (ListViewItem item in selectedItems)
      {
        ChannelMap map = (ChannelMap)item.Tag;
        mpListViewMapped.Items.Remove(item);


        ListViewItem newItem = mpListViewChannels.Items.Add(map.Channel.Name);
        newItem.Tag = map.Channel;


        map.Delete();
      }
      mpListViewChannels.Sort();

      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
      DatabaseManager.Instance.SaveChanges();
    }

    private void mpComboBoxCard_SelectedIndexChanged(object sender, EventArgs e)
    {
      DatabaseManager.Instance.SaveChanges();

      mpListViewChannels.BeginUpdate();
      mpListViewMapped.BeginUpdate();
      mpListViewMapped.Items.Clear();
      mpListViewChannels.Items.Clear();

      EntityQuery query = new EntityQuery(typeof(Channel));
      query.AddOrderBy(Channel.SortOrderEntityColumn);
      EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>(query);

      Card card = ((CardInfo)mpComboBoxCard.SelectedItem).Card;
      ReadOnlyEntityList<ChannelMap> maps = card.ChannelMaps;
      maps.ApplySort(new ChannelMap.Comparer(), false);
      foreach (ChannelMap map in maps)
      {
        Channel channel = map.Channel;
        if (channel.IsTv == false) continue;
        ListViewItem item = mpListViewMapped.Items.Add(channel.Name);
        item.Tag = map;
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


      foreach (Channel channel in channels)
      {
        if (channel.IsTv == false) continue;
        ListViewItem item = mpListViewChannels.Items.Add(channel.Name);
        item.Tag = channel;
      }
      mpListViewChannels.Sort();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
    }

    private void mpListViewChannels_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
  }
}