#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;
using TvDatabase;

namespace MPTvClient
{

  public partial class frmMain : Form
  {
    ServerInterface serverIntf;
    ExternalPlayer extPlayer;

    public frmMain()
    {
      InitializeComponent();
      serverIntf = new ServerInterface();
      extPlayer = new ExternalPlayer();
      ClientSettings.Load();
      miAlwaysPerformConnectionChecks.Checked = ClientSettings.alwaysPerformConnectionChecks;
    }
    private void SetDisconected()
    {
      tmrRefresh.Enabled = false;
      extPlayer.Stop();
      serverIntf.ResetConnection();
      StBarLabel.Text = "Not connected.";
      StBar.Update();
      MessageBox.Show(serverIntf.lastException.ToString(), "Exception raised", MessageBoxButtons.OK, MessageBoxIcon.Error);
      btnConnect.Visible = true;
    }
    private void UpdateTVChannels()
    {
      if (cbGroups.SelectedIndex == -1)
        return;
      StBarLabel.Text = "Loading referenced channels...";
      StBar.Update();
      List<ChannelInfo> refChannelInfos = serverIntf.GetChannelInfosForGroup(cbGroups.SelectedItem.ToString());
      if (refChannelInfos == null)
        SetDisconected();
      gridTVChannels.Rows.Clear();
      foreach (ChannelInfo chanInfo in refChannelInfos)
        gridTVChannels.Rows.Add(chanInfo.channelID, chanInfo.name, chanInfo.epgNow.timeInfo + "\n" + chanInfo.epgNext.timeInfo, chanInfo.epgNow.description + "\n" + chanInfo.epgNext.description);
      gridTVChannels.AutoResizeColumns();
      StBarLabel.Text = "";
    }
    private void UpdateRadioChannels()
    {
      if (cbRadioGroups.SelectedIndex == -1)
        return;
      StBarLabel.Text = "Loading referenced radio channels...";
      StBar.Update();
      List<ChannelInfo> refChannelInfos = serverIntf.GetChannelInfosForRadioGroup(cbRadioGroups.SelectedItem.ToString());
      if (refChannelInfos == null)
        SetDisconected();
      gridRadioChannels.Rows.Clear();
      foreach (ChannelInfo chanInfo in refChannelInfos)
      {
        string type = "DVB";
        if (chanInfo.isWebStream)
          type = "WebStream";
        gridRadioChannels.Rows.Add(chanInfo.channelID, chanInfo.name, type, chanInfo.epgNow.timeInfo + "\n" + chanInfo.epgNext.timeInfo, chanInfo.epgNow.description + "\n" + chanInfo.epgNext.description);
      }
      gridRadioChannels.AutoResizeColumns();
      StBarLabel.Text = "";
    }
    private void UpdateRecordings()
    {
      StBarLabel.Text = "Loading recordings...";
      StBar.Update();
      List<RecordingInfo> recInfos = serverIntf.GetRecordings();
      if (recInfos == null)
        SetDisconected();
      gridRecordings.Rows.Clear();
      foreach (RecordingInfo rec in recInfos)
        gridRecordings.Rows.Add(rec.recordingID, rec.timeInfo, rec.genre, rec.title, rec.description);
      gridRecordings.AutoResizeColumns();
      StBarLabel.Text = "";
    }
    private void UpdateSchedules()
    {
      StBarLabel.Text = "Loading schedules...";
      StBar.Update();
      List<ScheduleInfo> schedInfos = serverIntf.GetSchedules();
      if (schedInfos == null)
        SetDisconected();
      gridSchedules.Rows.Clear();
      foreach (ScheduleInfo sched in schedInfos)
        gridSchedules.Rows.Add(sched.scheduleID, sched.startTime.ToString(), sched.endTime.ToString(), sched.description, sched.channelName, sched.type);
      gridSchedules.AutoResizeColumns();
      StBarLabel.Text = "";
    }
    private void btnConnect_Click(object sender, EventArgs e)
    {
      if (!ClientSettings.IsValid())
      {
        MessageBox.Show("The settings are invalid.\nPlease check the configuration.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      string connStr; string provider;
      if (!serverIntf.GetDatabaseConnectionString(out connStr, out provider))
      {
        SetupDatabaseForm frm = new SetupDatabaseForm();
        frm.ShowDialog();
        return;
      }
      if (ClientSettings.alwaysPerformConnectionChecks)
      {
        StBarLabel.Text = "Running connection test...";
        StBar.Update();
        frmConnectionTest connTest = new frmConnectionTest();
        connTest.RunChecks(ClientSettings.serverHostname, provider);
        if (connTest.GetFailedCount() > 0)
        {
          if (MessageBox.Show("Some ports on the TvServer machine are not reachable.\n\nDo you want to try to connect nevertheless?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
          {
            StBarLabel.Text = "Not connected";
            return;
          }
        }
      }
      StBarLabel.Text = "Connecting to TvServer and loading groups/channels...";
      StBar.Update();
      serverIntf.ResetConnection();
      if (!serverIntf.Connect(ClientSettings.serverHostname))
      {
        SetDisconected();
        return;
      }
      cbGroups.Items.Clear();
      List<string> groups = serverIntf.GetGroupNames();
      if (groups == null)
      {
        SetDisconected();
        return;
      }
      foreach (string group in groups)
      {
        if (!cbAllChannels.Checked && group == "All Channels")
          continue;
        cbGroups.Items.Add(group);
      }
      if (cbGroups.Items.Count > 0)
        cbGroups.SelectedIndex = 0;

      cbRadioGroups.Items.Clear();
      List<string> radioGroups = serverIntf.GetRadioGroupNames();
      if (radioGroups == null)
      {
        SetDisconected();
        return;
      }
      foreach (string group in radioGroups)
      {
        if (!cbAllChannels.Checked && group=="All Channels")
          continue;
        cbRadioGroups.Items.Add(group);
      }
      if (cbRadioGroups.Items.Count > 0)
        cbRadioGroups.SelectedIndex = 0;      

      StBarLabel.Text = "";
      tmrRefresh.Enabled = true;
      btnConnect.Visible = false;
    }
    private void cbGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
      UpdateTVChannels();
    }
    private void timer1_Tick(object sender, EventArgs e)
    {
      if (!extPlayer.IsRunning())
        serverIntf.StopTimeShifting();
      ReceptionDetails recDetails = serverIntf.GetReceptionDetails();
      if (recDetails == null)
      {
        SetDisconected();
        return;
      }
      prLevel.Value = recDetails.signalLevel;
      prQuality.Value = recDetails.signalQuality;
      List<StreamingStatus> statusList = serverIntf.GetStreamingStatus();
      if (statusList == null)
      {
        SetDisconected();
        return;
      }
      lvStatus.Items.Clear();
      foreach (StreamingStatus sstate in statusList)
      {
        ListViewItem item = lvStatus.Items.Add(sstate.cardId.ToString());
        item.SubItems.Add(sstate.cardName);
        item.SubItems.Add(sstate.cardType);
        item.SubItems.Add(sstate.status);
        item.SubItems.Add(sstate.channelName);
        item.SubItems.Add(sstate.userName);
      }
      lvStatus.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void gridChannels_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      StBarLabel.Text = "Trying to start timeshifting...";
      StBar.Update();
      serverIntf.StopTimeShifting();
      extPlayer.Stop();
      string rtspURL = "";
      TvResult result = serverIntf.StartTimeShifting(int.Parse(gridTVChannels.SelectedRows[0].Cells[0].Value.ToString()), ref rtspURL);
      StBarLabel.Text = "";
      StBar.Update();
      if (result != TvResult.Succeeded)
        MessageBox.Show("Could not start timeshifting\nReason: " + result.ToString());
      else
      {
        string args = string.Format(ClientSettings.playerArgs, rtspURL);
        if (!extPlayer.Start(ClientSettings.playerPath, args))
          MessageBox.Show("Failed to start external player.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (extPlayer.IsRunning())
        extPlayer.Stop();
      serverIntf.StopTimeShifting();
      serverIntf.ResetConnection();
      ClientSettings.frmLeft = this.Left;
      ClientSettings.frmTop = this.Top;
      ClientSettings.frmWidth = this.Width;
      ClientSettings.frmHeight = this.Height;
      ClientSettings.Save();
    }

    private void externalPlayerToolStripMenuItem_Click(object sender, EventArgs e)
    {
      frmExternalPlayerConfig frm = new frmExternalPlayerConfig();
      frm.InitForm(ClientSettings.playerPath, ClientSettings.playerArgs,ClientSettings.useOverride,ClientSettings.overrideURL);
      if (frm.ShowDialog() == DialogResult.OK)
      {
        frm.GetConfig(ref ClientSettings.playerPath, ref ClientSettings.playerArgs,ref ClientSettings.useOverride,ref ClientSettings.overrideURL);
        ClientSettings.Save();
      }
    }
    private void serverConnectionToolStripMenuItem_Click(object sender, EventArgs e)
    {
      frmServerConnectionConfig frm = new frmServerConnectionConfig();
      frm.InitForm(ClientSettings.serverHostname);
      if (frm.ShowDialog() == DialogResult.OK)
      {
        frm.GetConfig(ref ClientSettings.serverHostname);
        ClientSettings.Save();
      }
    }

    private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {

    }

    private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
    {
      switch (tabCtrl.SelectedIndex)
      {
        case 0:
          UpdateTVChannels();
          break;
        case 1:
          UpdateRadioChannels();
          break;
        case 2:
          UpdateRecordings();
          break;
        case 3:
          UpdateSchedules();
          break;
      }
    }

    private void gridTVChannels_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      StBarLabel.Text = "Trying to start timeshifting...";
      StBar.Update();
      serverIntf.StopTimeShifting();
      extPlayer.Stop();
      string rtspURL = "";
      TvResult result = serverIntf.StartTimeShifting(int.Parse(gridTVChannels.SelectedRows[0].Cells[0].Value.ToString()), ref rtspURL);
      StBarLabel.Text = "";
      StBar.Update();
      if (result != TvResult.Succeeded)
        MessageBox.Show("Could not start timeshifting\nReason: " + result.ToString());
      else
      {
        if (ClientSettings.useOverride)
          rtspURL = ClientSettings.overrideURL;
        string args = string.Format(ClientSettings.playerArgs, rtspURL);
        if (!extPlayer.Start(ClientSettings.playerPath, args))
          MessageBox.Show("Failed to start external player.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void gridRadioChannels_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      serverIntf.StopTimeShifting();
      extPlayer.Stop();
      string rtspURL = "";
      if (gridRadioChannels.SelectedRows[0].Cells[2].Value.ToString() == "DVB")
      {
        StBarLabel.Text = "Trying to start timeshifting...";
        StBar.Update();
        TvResult result = serverIntf.StartTimeShifting(int.Parse(gridRadioChannels.SelectedRows[0].Cells[0].Value.ToString()), ref rtspURL);
        StBarLabel.Text = "";
        StBar.Update();
        if (result != TvResult.Succeeded)
          MessageBox.Show("Could not start timeshifting\nReason: " + result.ToString());
      }
      else
        rtspURL=serverIntf.GetWebStreamURL(int.Parse(gridRadioChannels.SelectedRows[0].Cells[0].Value.ToString()));
      if (ClientSettings.useOverride)
        rtspURL = ClientSettings.overrideURL;
      string args = string.Format(ClientSettings.playerArgs, rtspURL);
      if (!extPlayer.Start(ClientSettings.playerPath, args))
          MessageBox.Show("Failed to start external player.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void gridRecordings_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      StBarLabel.Text = "Trying to replay recording...";
      StBar.Update();
      serverIntf.StopTimeShifting();
      extPlayer.Stop();
      string rtspURL = serverIntf.GetRecordingURL(int.Parse(gridRecordings.SelectedRows[0].Cells[0].Value.ToString()));
      StBarLabel.Text = "";
      StBar.Update();
      if (rtspURL == "")
        MessageBox.Show("Could not start recording");
      else
      {
        string args = string.Format(ClientSettings.playerArgs, rtspURL);
        if (!extPlayer.Start(ClientSettings.playerPath, args))
          MessageBox.Show("Failed to start external player.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
    {
      miTimeShift.Visible = false;
      miReplay.Visible = false;
      miDelete.Visible = false;
      switch (tabCtrl.SelectedIndex)
      {
        case 0:
          miTimeShift.Visible = true;
          break;
        case 1:
          miTimeShift.Visible = true;
          break;
        case 2:
          miReplay.Visible = true;
          miDelete.Visible = true;
          break;
        case 3:
          miDelete.Visible = true;
          break;
      }
    }

    private void miRefresh_Click(object sender, EventArgs e)
    {
      refreshToolStripMenuItem_Click(sender, e);
    }

    private void miTimeShift_Click(object sender, EventArgs e)
    {
      if (tabCtrl.SelectedIndex == 0)
        gridTVChannels_CellDoubleClick(sender, null);
      if (tabCtrl.SelectedIndex == 1)
        gridRadioChannels_CellDoubleClick(sender, null);
    }

    private void miReplay_Click(object sender, EventArgs e)
    {
      gridRecordings_CellDoubleClick(sender, null);
    }

    private void miDelete_Click(object sender, EventArgs e)
    {
      if (tabCtrl.SelectedIndex == 2)
      {
        string idRecording = gridRecordings.SelectedRows[0].Cells[0].Value.ToString();
        if (idRecording == "")
          return;
        serverIntf.DeleteRecording(int.Parse(idRecording));
        UpdateRecordings();
      }
      if (tabCtrl.SelectedIndex == 3)
      {
        string idSchedule = gridSchedules.SelectedRows[0].Cells[0].Value.ToString();
        if (idSchedule == "")
          return;
        serverIntf.DeleteSchedule(int.Parse(idSchedule));
        UpdateSchedules();
      }
    }

    private void btnShowEPG_Click(object sender, EventArgs e)
    {
      List<ChannelInfo> infos=new List<ChannelInfo>();
      DataGridView grid = gridTVChannels;
      Button btn = (Button)sender;
      if (btn != btnShowEPG)
        grid = gridRadioChannels;
      foreach (DataGridViewRow row in grid.Rows)
      {
        ChannelInfo info=new ChannelInfo();
        info.channelID=row.Cells[0].Value.ToString();
        info.name=row.Cells[1].Value.ToString();
        infos.Add(info);
      }
      frmEPG frm = new frmEPG(serverIntf, infos);
      frm.Show();
    }

    private void frmMain_Shown(object sender, EventArgs e)
    {
      if (ClientSettings.frmLeft != 0 && ClientSettings.frmTop != 0)
      {
        this.Left = ClientSettings.frmLeft;
        this.Top = ClientSettings.frmTop;
        this.Width = ClientSettings.frmWidth;
        this.Height = ClientSettings.frmHeight;
      }
    }

    private void databaseSettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      SetupDatabaseForm frm = new SetupDatabaseForm();
      frm.ShowDialog();
    }

    private void miAlwaysPerformConnectionChecks_CheckedChanged(object sender, EventArgs e)
    {
      ClientSettings.alwaysPerformConnectionChecks = miAlwaysPerformConnectionChecks.Checked;
      ClientSettings.Save();
    }

    private void cbRadioGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
      UpdateRadioChannels();
    }
  }
}