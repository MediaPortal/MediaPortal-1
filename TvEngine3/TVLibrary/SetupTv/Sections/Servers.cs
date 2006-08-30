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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

using TvDatabase;
namespace SetupTv.Sections
{
  public partial class Servers : SectionSettings
  {
    public Servers()
      : this("TV Servers")
    {
    }

    public Servers(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      EntityList<Server> servers=DatabaseManager.Instance.GetEntities<Server>();
      mpListView1.Items.Clear();
      foreach (Server server in servers)
      {
        ListViewItem item = mpListView1.Items.Add(server.HostName);
        if (server.IsMaster)
        {
          item.SubItems.Add("Master");
        }
        else
        {
          item.SubItems.Add("Slave");
        }
        item.Tag = server;
      }
    }

    private void Servers_Load(object sender, EventArgs e)
    {

    }

    private void buttonDelete_Click(object sender, EventArgs e)
    {

      if (mpListView1.SelectedIndices.Count < 1) return;
      int index = mpListView1.SelectedIndices[0];
      ListViewItem item = mpListView1.Items[index];
      Server server = (Server)item.Tag;
      server.DeleteAll();
      OnSectionActivated();
    }

    private void buttonAdd_Click(object sender, EventArgs e)
    {

    }

    private void buttonMaster_Click(object sender, EventArgs e)
    {
      if (mpListView1.SelectedIndices.Count < 1) return;
      int index = mpListView1.SelectedIndices[0];
      for (int i = 0; i < mpListView1.Items.Count; ++i)
      {
        ListViewItem item = mpListView1.Items[i];
        Server server = (Server)item.Tag;
        if (i != index)
        {
          item.SubItems[0].Text = "Slave";
          server.IsMaster = false;
        }
        else
        {
          item.SubItems[0].Text = "Master";
          server.IsMaster = true;
          RemoteControl.HostName = server.HostName;
        }
        server.IsMaster = false;
      }
    }
  }
}