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
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using System.Net;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class StreamingServer : SectionSettings
  {
    private int _rtspPort = Convert.ToInt32(ServiceAgents.Instance.SettingServiceAgent.GetSetting("rtspport").Value);
    private string _hostname = ServiceAgents.Instance.SettingServiceAgent.GetSetting("hostname").Value;

    private class IpAddressOption
    {
   
      public string DisplayString;
      public string HostName;

      public IpAddressOption(string displayString, string hostName)
      {
        DisplayString = displayString;
        HostName = hostName;
      }

      public override string ToString()
      {
        return DisplayString;
      }
    }
    

    public StreamingServer()
      : this("Streaming Server") {}

    public StreamingServer(string name)
      : base(name)
    {
      InitializeComponent();

      
    }

    public override void OnSectionActivated()
    {
      timer1.Enabled = true;      

      try
      {
        _hostname = Dns.GetHostEntry(_hostname).HostName;
      }
      catch (Exception ex)
      {
        Log.Error("Failed to get our server host name");
        Log.Write(ex);
      }
      IEnumerable<string> ipAdresses = ServiceAgents.Instance.ControllerServiceAgent.ServerIpAdresses;
      IpAddressComboBox.Items.Clear();
      IpAddressComboBox.Items.Add(new IpAddressOption("(auto)", _hostname));
      int selected = 0;
      int counter = 1;
      foreach (string ipAdress in ipAdresses)
      {
        IpAddressComboBox.Items.Add(new IpAddressOption(ipAdress, ipAdress));
        if (String.Compare(ipAdress, _hostname, true) == 0)
        {
          selected = counter;
        }
        counter++;
      }
      IpAddressComboBox.SelectedIndex = selected;
      
      if (_rtspPort >= PortNoNumericUpDown.Minimum && _rtspPort <= PortNoNumericUpDown.Maximum)
      {
        PortNoNumericUpDown.Value = _rtspPort;
        PortNoNumericUpDown.Text = _rtspPort.ToString(); // in case value is the same but text is empty
      }
    }

    public override void OnSectionDeActivated()
    {
      timer1.Enabled = false;
      base.OnSectionDeActivated();

      ApplyStreamingSettings();
    }

    private void ApplyStreamingSettings()
    {
      string newHostName = ((IpAddressOption)IpAddressComboBox.SelectedItem).HostName;
      int newRtspPort = (int)PortNoNumericUpDown.Value;
      bool needRestart = false;
      //int.TryParse(PortNoNumericUpDown.Text, out newRtspPort);
      if (_hostname != newHostName ||
          _rtspPort != newRtspPort)
      {
        _hostname = newHostName;
        _rtspPort = newRtspPort;
        needRestart = true;
      }
     
      if (needRestart)
      {

        ServiceAgents.Instance.SettingServiceAgent.SaveSetting("rtspport", _rtspPort.ToString());
        ServiceAgents.Instance.SettingServiceAgent.SaveSetting("hostname", _hostname);

        ServiceNeedsToRestart();
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

    private void timer1_Tick(object sender, EventArgs e)
    {
      List<RtspClient> clients = ServiceAgents.Instance.ControllerServiceAgent.StreamingClients;
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

        IUser user = new User();
        user.Name = System.Net.Dns.GetHostEntry(client.IpAdress).HostName;

        IList<Card> dbsCards = ServiceAgents.Instance.CardServiceAgent.ListAllCards(CardIncludeRelationEnum.None);

        foreach (Card card in dbsCards)
        {
          if (!card.Enabled)
            continue;
          if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(card.IdCard))
            continue;

          IDictionary<string, IUser> users = ServiceAgents.Instance.ControllerServiceAgent.GetUsersForCard(card.IdCard);
          foreach (KeyValuePair<string, IUser> u in users)
          {
            if (u.Value.Name == user.Name || u.Value.Name == "setuptv")
            {
              foreach(var subchannel in u.Value.SubChannels.Values)
              {
                Channel ch = ServiceAgents.Instance.ChannelServiceAgent.GetChannel(subchannel.IdChannel);
                if (ch.DisplayName == client.Description)
                {
                  user.CardId = card.IdCard;
                  break;
                } 
              }              
            }
          }
          if (user.CardId > -1)
            break;
        }

        bool res = ServiceAgents.Instance.ControllerServiceAgent.StopTimeShifting(user.Name, out user, TvStoppedReason.KickedByAdmin);

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

    private void ApplyButton_Click(object sender, EventArgs e)
    {
      ApplyStreamingSettings();
    }
  }
}