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
using TvDatabase;

namespace SetupTv.Sections
{
  public partial class Servers : SectionSettings
  {
    public Servers()
      : this("TV Servers") {}

    public Servers(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      IList<Server> servers = Server.ListAll();
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
        item.SubItems.Add(server.RtspPort.ToString());
        item.Tag = server;
      }
      mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void Servers_Load(object sender, EventArgs e) {}

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

    private void buttonChooseIp_Click(object sender, EventArgs e)
    {
      if (mpListView1.SelectedIndices.Count < 1)
        return;
      int index = mpListView1.SelectedIndices[0];
      ListViewItem item = mpListView1.Items[index];
      Server server = (Server)item.Tag;

      FormEditIpAdress dlg = new FormEditIpAdress();
      dlg.HostName = server.HostName;
      dlg.PortNo = server.RtspPort;
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        if (dlg.HostName.Equals(server.HostName) == false || dlg.PortNo != server.RtspPort)
        {
          item.Text = dlg.HostName;
          item.SubItems[2].Text = dlg.PortNo.ToString();
          server.HostName = dlg.HostName;
          server.RtspPort = dlg.PortNo;
          server.Persist();
          ServiceNeedsToRestart();
        }
      }
    }

    private void ServiceNeedsToRestart()
    {
      if (
        MessageBox.Show(this, "Changes made require TvService to restart. Restart it now?", "TvService",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
      {
        NotifyForm dlgNotify = new NotifyForm("Restart TvService...", "This can take some time\n\nPlease be patient...");
        dlgNotify.Show();
        dlgNotify.WaitForDisplay();

        RemoteControl.Instance.Restart();

        dlgNotify.Close();
      }
    }

    private void mpListView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (mpListView1.SelectedItems.Count == 0)
      {
        buttonChooseIp.Enabled = false;
      }
      else
      {
        buttonChooseIp.Enabled = true;
      }
    }

    private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (mpListView1.SelectedItems.Count == 0)
      {
        chooseIPForStreamingToolStripMenuItem.Enabled = false;
      }
      else
      {
        chooseIPForStreamingToolStripMenuItem.Enabled = true;
      }
    }

    private void chooseIPForStreamingToolStripMenuItem_Click(object sender, EventArgs e)
    {
      buttonChooseIp_Click(null, null);
    }
  }
}