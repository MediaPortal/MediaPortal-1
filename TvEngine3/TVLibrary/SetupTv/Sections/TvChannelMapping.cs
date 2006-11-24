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
      IList cards = Card.ListAll();
      foreach (Card card in cards)
      {
        if (card.Enabled == false) continue;
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
        ChannelMap map = layer.MapChannelToCard(card, channel);
        mpListViewChannels.Items.Remove(item);

        ListViewItem newItem = mpListViewMapped.Items.Add(channel.Name);
        newItem.Tag = map;
      }
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
      //DatabaseManager.Instance.SaveChanges();
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


        ListViewItem newItem = mpListViewChannels.Items.Add(map.ReferencedChannel().Name);
        newItem.Tag = map.ReferencedChannel();


        map.Remove();
      }
      mpListViewChannels.Sort();

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

      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
      sb.AddOrderByField(true, "sortOrder");
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());

      Card card = ((CardInfo)mpComboBoxCard.SelectedItem).Card;
      IList maps = card.ReferringChannelMap();


      List<ListViewItem> items = new List<ListViewItem>();
      foreach (ChannelMap map in maps)
      {
        Channel channel = map.ReferencedChannel();
        if (channel.IsTv == false) continue;
        ListViewItem item = new ListViewItem(channel.Name);
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
        if (channel.IsTv == false) continue;
        ListViewItem item = new ListViewItem(channel.Name);
        item.Tag = channel;
        items.Add(item);
      }
      mpListViewChannels.Items.AddRange(items.ToArray());
      mpListViewChannels.Sort();
      mpListViewChannels.EndUpdate();
      mpListViewMapped.EndUpdate();
    }

    private void mpListViewChannels_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
  }
}