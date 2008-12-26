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
using System.Windows.Forms;
using TvControl;
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
      IList servers = Server.ListAll();
      mpListView1.Items.Clear();
      foreach (Server server in servers)
      {
        ListViewItem item = mpListView1.Items.Add(server.HostName, 0);
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
      mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void Servers_Load(object sender, EventArgs e)
    {

    }

    private void buttonDelete_Click(object sender, EventArgs e)
    {

      if (mpListView1.SelectedIndices.Count < 1)
        return;
      int index = mpListView1.SelectedIndices[0];
      ListViewItem item = mpListView1.Items[index];
      Server server = (Server)item.Tag;
      server.Delete();
      MessageBox.Show(this, "Changes made require TvService to restart. Please restart the tvservice");
      RemoteControl.Instance.Restart();
      OnSectionActivated();
    }

    private void buttonMaster_Click(object sender, EventArgs e)
    {
      if (mpListView1.SelectedIndices.Count < 1)
        return;
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
        server.Persist();
      }
      RemoteControl.Instance.Restart();
      MessageBox.Show(this, "Changes made require TvService to restart. Please restart the tvservice");
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (mpListView1.SelectedIndices.Count < 1)
        return;
      int index = mpListView1.SelectedIndices[0];
      ListViewItem item = mpListView1.Items[index];
      Server server = (Server)item.Tag;

      FormEditIpAdress dlg = new FormEditIpAdress();
      dlg.HostName = server.HostName;
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        item.Text = dlg.HostName;
        server.HostName = dlg.HostName;
        server.Persist();
        RemoteControl.Instance.Restart();
        MessageBox.Show(this, "Changes made require TvService to restart. Please restart the tvservice");
      }

    }


  }
}
