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
using System.Windows.Forms;
using TvControl;
using TvDatabase;
using TvLibrary.Streaming;

namespace SetupTv.Sections
{
  public partial class StreamingServer : SectionSettings
  {
    public StreamingServer()
      : this("Streaming Server")
    {
    }

    public StreamingServer(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      timer1.Enabled = true;
    }

    public override void OnSectionDeActivated()
    {
      timer1.Enabled = false;
      base.OnSectionDeActivated();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      List<RtspClient> clients = RemoteControl.Instance.StreamingClients;
      for (int i = 0; i < clients.Count; ++i)
      {
        RtspClient client = clients[i];
        if (i >= listView1.Items.Count)
        {
          ListViewItem item = new ListViewItem(client.StreamName);
          item.Tag = client;
          item.SubItems.Add(client.IpAdress);
          if (client.IsActive)
            item.SubItems.Add("yes");
          else
            item.SubItems.Add("no");
          item.SubItems.Add(client.DateTimeStarted.ToString("yyyy-MM-dd HH:mm:ss"));
          item.SubItems.Add(client.Description);
          item.ImageIndex = 0;
          listView1.Items.Add(item);
        }
        else
        {
          ListViewItem item = listView1.Items[i];
          item.Text = client.StreamName;
          item.SubItems[1].Text = client.IpAdress;
          item.SubItems[2].Text = client.IsActive ? "yes" : "no";
          item.SubItems[3].Text = client.DateTimeStarted.ToString("yyyy-MM-dd HH:mm:ss");
          item.SubItems[4].Text = client.Description;
          item.ImageIndex = 0;
        }
      }
      while (listView1.Items.Count > clients.Count)
        listView1.Items.RemoveAt(listView1.Items.Count - 1);

      listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void mpButtonKick_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in listView1.SelectedItems)
      {
        RtspClient client = (RtspClient)item.Tag;

        User user = new User();
        user.Name = System.Net.Dns.GetHostEntry(client.IpAdress).HostName;

        IList dbsCards = Card.ListAll();

        foreach (Card card in dbsCards)
        {
          if (!card.Enabled)
            continue;
          if (!RemoteControl.Instance.CardPresent(card.IdCard))
            continue;

          User[] users = RemoteControl.Instance.GetUsersForCard(card.IdCard);
          foreach (User u in users)
          {
            if (u.Name == user.Name || u.Name == "setuptv")
            {
              Channel ch = Channel.Retrieve(u.IdChannel);

              if (ch.Name == client.Description)
              {
                user.CardId = card.IdCard;
                break;
              }
            }
          }
          if (user.CardId > -1)
            break;
        }

        bool res = RemoteControl.Instance.StopTimeShifting(ref user, TvStoppedReason.KickedByAdmin);

        if (res)
        {
          listView1.Items.Remove(item);
        }
      }
      listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
    {
      mpButtonKick_Click(sender, e);
    }
  }
}
