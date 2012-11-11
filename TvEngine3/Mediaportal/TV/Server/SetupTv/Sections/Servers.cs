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
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Dialogs;
using Mediaportal.TV.Server.TVControl.ServiceAgents;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class Servers : SectionSettings
  {
    private class Server
    {
      public string Hostname { get; set; }
      public int RtspPort { get; set; }
    }

    public Servers()
      : this("TV Servers") {}

    public Servers(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      var hostname = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("hostname", System.Net.Dns.GetHostName()).Value;
      var rtspPort = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("rtspport", "554").Value;
      mpListView1.Items.Clear();
      var server = new Server {Hostname = hostname, RtspPort = Convert.ToInt32(rtspPort)};

      ListViewItem item = mpListView1.Items.Add(hostname, 0);      
      item.SubItems.Add(rtspPort);
      
      item.Tag = server;

      mpListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void Servers_Load(object sender, EventArgs e) {}



    private void buttonChooseIp_Click(object sender, EventArgs e)
    {
      if (mpListView1.SelectedIndices.Count < 1)
        return;
      int index = mpListView1.SelectedIndices[0];
      ListViewItem item = mpListView1.Items[index];
      var server = (Server)item.Tag;

      var dlg = new FormEditIpAdress {HostName = server.Hostname, PortNo = server.RtspPort};
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        if (dlg.HostName.Equals(server.Hostname) == false || dlg.PortNo != server.RtspPort)
        {
          item.Text = dlg.HostName;
          item.SubItems[2].Text = dlg.PortNo.ToString();
          ServiceAgents.Instance.SettingServiceAgent.SaveSetting("hostname", dlg.HostName);
          ServiceAgents.Instance.SettingServiceAgent.SaveSetting("rtspport", dlg.PortNo.ToString());                    
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

        ServiceAgents.Instance.ControllerServiceAgent.Restart();

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