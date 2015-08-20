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
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class StreamingServer : SectionSettings
  {
    private string _interface = null;
    private int _port = 0;
    private Timer _clientListUpdateTimer = null;

    private class ComboBoxInterface
    {
      public string DisplayName;
      public string Interface;

      public ComboBoxInterface(string displayString, string hostName)
      {
        DisplayName = displayString;
        Interface = hostName;
      }

      public override string ToString()
      {
        return DisplayName;
      }
    }
    
    public StreamingServer(ServerConfigurationChangedEventHandler handler)
      : base("Streaming Server", handler)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("streamer: activating");

      _interface = ServiceAgents.Instance.SettingServiceAgent.GetValue("rtspServerInterface", string.Empty);
      _port = ServiceAgents.Instance.SettingServiceAgent.GetValue("rtspServerPort", 554);
      this.LogDebug("streamer: configured interface = {0}, port = {1}", _interface, _port);

      string actualInterface;
      ushort actualPort;
      ServiceAgents.Instance.ControllerServiceAgent.GetStreamingServerInformation(out actualInterface, out actualPort);
      this.LogDebug("streamer: actual interface = {0}, port = {1}", actualInterface, actualPort);

      IEnumerable<string> ipAdresses = ServiceAgents.Instance.ControllerServiceAgent.ServerIpAddresses;
      int selectedIndex = 0;
      int index = 1;
      comboBoxInterface.BeginUpdate();
      try
      {
        comboBoxInterface.Items.Clear();
        comboBoxInterface.Items.Add(new ComboBoxInterface("(auto)", string.Empty));
        foreach (string ipAddress in ipAdresses)
        {
          comboBoxInterface.Items.Add(new ComboBoxInterface(ipAddress, ipAddress));
          if (string.Equals(ipAddress, _interface))
          {
            selectedIndex = index;
          }
          index++;
        }
        comboBoxInterface.SelectedIndex = selectedIndex;
      }
      finally
      {
        comboBoxInterface.EndUpdate();
      }

      numericUpDownPort.Value = _port;

      if (string.IsNullOrEmpty(actualInterface) || actualPort <= 0)
      {
        labelStatusValue.Text = "Server is not running. Check configured interface and port are available.";
        labelStatusValue.ForeColor = System.Drawing.Color.Red;
      }
      else
      {
        labelStatusValue.Text = string.Format("Server is running on interface {0} port {1}.", actualInterface, actualPort);
        labelStatusValue.ForeColor = System.Drawing.Color.Green;
      }

      _clientListUpdateTimer = new Timer();
      _clientListUpdateTimer.Interval = 3000;
      _clientListUpdateTimer.Tick += new EventHandler(OnClientListUpdateTimerTick);
      _clientListUpdateTimer.Enabled = true;
      _clientListUpdateTimer.Start();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("streamer: deactivating");

      _clientListUpdateTimer.Enabled = false;
      _clientListUpdateTimer.Stop();
      _clientListUpdateTimer.Dispose();
      _clientListUpdateTimer = null;

      string newInterface = ((ComboBoxInterface)comboBoxInterface.SelectedItem).Interface;
      if (string.Equals(_interface, newInterface) && _port == numericUpDownPort.Value)
      {
        base.OnSectionDeActivated();
        return;
      }

      if (listViewClients.Items.Count > 0)
      {
        DialogResult result = MessageBox.Show("Clients will be disconnected. Are you sure you want to continue?", MESSAGE_CAPTION, MessageBoxButtons.YesNo);
        if (result == DialogResult.No)
        {
          base.OnSectionDeActivated();
          return;
        }
      }

      if (!string.Equals(_interface, newInterface))
      {
        this.LogInfo("streamer: interface changed from {0} to {1}", _interface, newInterface);
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("rtspServerInterface", newInterface);
      }
      if (_port != numericUpDownPort.Value)
      {
        this.LogInfo("streamer: port changed from {0} to {1}", _port, numericUpDownPort.Value);
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("rtspServerPort", (int)numericUpDownPort.Value);
      }
      OnServerConfigurationChanged(this, false, true, null);

      base.OnSectionDeActivated();
    }

    private void OnClientListUpdateTimerTick(object sender, EventArgs e)
    {
      // Update the client list. Take care to do it on the UI thread.
      this.Invoke((MethodInvoker)delegate
      {
        UpdateClientList();
      });
    }

    private void UpdateClientList()
    {
      bool log = false;
      ICollection<RtspClient> clients = ServiceAgents.Instance.ControllerServiceAgent.StreamingClients;
      if (clients.Count != listViewClients.Items.Count)
      {
        this.LogDebug("streamer: client list update...");
        log = true;
      }
      List<ListViewItem> items = new List<ListViewItem>(clients.Count);
      foreach (RtspClient client in clients)
      {
        ListViewItem item = new ListViewItem(client.StreamId);
        item.Tag = client;
        item.SubItems.Add(client.ClientSessionId.ToString());
        item.SubItems.Add(client.ClientIpAddress);
        item.SubItems.Add(client.StreamDescription);
        item.SubItems.Add(client.ClientConnectionDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        item.SubItems.Add(client.IsClientActive ? "yes" : "no");
        item.SubItems.Add(client.StreamUrl);
        listViewClients.Items.Add(item);

        if (log)
        {
          this.LogDebug("  client...");
          this.LogDebug("    session ID      = {0}", client.ClientSessionId);
          this.LogDebug("    IP address      = {0}", client.ClientIpAddress);
          this.LogDebug("    active?         = {0}", client.IsClientActive);
          this.LogDebug("    connection time = {0}", client.ClientConnectionDateTime);
          this.LogDebug("    stream ID       = {0}", client.StreamId);
          this.LogDebug("    description     = {0}", client.StreamDescription);
          this.LogDebug("    stream URL      = {0}", client.StreamUrl);
        }
      }

      listViewClients.BeginUpdate();
      try
      {
        listViewClients.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
        listViewClients.Items.Clear();
        listViewClients.Items.AddRange(items.ToArray());
        listViewClients.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
      }
      finally
      {
        listViewClients.EndUpdate();
      }
    }

    private void buttonKick_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in listViewClients.SelectedItems)
      {
        RtspClient client = (RtspClient)item.Tag;
        this.LogInfo("streamer: kick client, session ID = {0}", client.ClientSessionId);
        ServiceAgents.Instance.ControllerServiceAgent.DisconnectStreamingClient(client.ClientSessionId);
      }
    }
  }
}