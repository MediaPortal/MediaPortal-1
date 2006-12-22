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

namespace SetupTv.Sections
{
  public partial class RadioChannels : SectionSettings
  {

    public RadioChannels()
      : this("Radio Stations")
    {
    }

    public RadioChannels(string name)
      : base(name)
    {
      InitializeComponent();
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
        ListViewItem item = mpListView1.Items.Add(ch.Name,3);
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
        item.Tag = ch;
        item.SubItems.Add(line);
        item.SubItems.Add(details[0]);
        item.SubItems.Add(details[1]);
        item.SubItems.Add(details[2]);
        item.SubItems.Add(details[3]);
      }
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
      RemoteControl.Instance.OnNewSchedule();
    }

    private void mpListView1_ItemDrag(object sender, ItemDragEventArgs e)
    {

    }
  }
}