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
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class TestService : SectionSettings
  {

    private class ServerMonitor
    {
      #region events & delegates

      public delegate void ServerDisconnectedDelegate();
      public delegate void ServerConnectedDelegate();

      public event ServerDisconnectedDelegate OnServerDisconnected;
      public event ServerConnectedDelegate OnServerConnected;

      #endregion

      private Thread _serverMonitorThread;
      private readonly static ManualResetEvent _evtHeartbeatCtrl = new ManualResetEvent(false);
      private const int SERVER_ALIVE_INTERVAL_SEC = 5;
      private bool _isConnected;

      public void Start()
      {
        //System.Diagnostics.Debugger.Launch();
        StartServerMonitorThread();
      }

      public void Stop()
      {
        StopServerMonitorThread();
      }

      private void StartServerMonitorThread()
      {
        if (_serverMonitorThread == null || !_serverMonitorThread.IsAlive)
        {
          _evtHeartbeatCtrl.Reset();
          this.LogDebug("ServerMonitor: ServerMonitor thread started.");
          _serverMonitorThread = new Thread(ServerMonitorThread) { IsBackground = true, Name = "ServerMonitor thread" };
          _serverMonitorThread.Start();
        }
      }

      private void StopServerMonitorThread()
      {
        if (_serverMonitorThread != null && _serverMonitorThread.IsAlive)
        {
          try
          {
            _evtHeartbeatCtrl.Set();
            _serverMonitorThread.Join();
            this.LogDebug("ServerMonitor: ServerMonitor thread stopped.");
          }
          catch (Exception) { }
        }
      }


      private void ServerMonitorThread()
      {
        while (!_evtHeartbeatCtrl.WaitOne(SERVER_ALIVE_INTERVAL_SEC * 1000))
        {

          bool isconnected = false;
          try
          {
            ServiceAgents.Instance.DiscoverServiceAgent.Ping();
            isconnected = true;
          }
          catch
          {
          }
          finally
          {
            if (!_isConnected && isconnected)
            {
              if (OnServerConnected != null)
              {
                OnServerConnected();
              }
            }
            else if (_isConnected && !isconnected)
            {
              if (OnServerDisconnected != null)
              {
                OnServerDisconnected();
              }
            }
            _isConnected = isconnected;
          }
        }
      }
    }

    private IList<Tuner> _tuners;
    private Dictionary<int, string> _channelNames;

    private ServerMonitor _serverMonitor = new ServerMonitor();

    public TestService()
      : base("Manual Control")
    {
      InitializeComponent();
      mpComboBoxChannels.ImageList = imageList1;
      DoubleBuffered = true;

      _serverMonitor.OnServerConnected += new ServerMonitor.ServerConnectedDelegate(_serverMonitor_OnServerConnected);
      _serverMonitor.OnServerDisconnected += new ServerMonitor.ServerDisconnectedDelegate(_serverMonitor_OnServerDisconnected);
      _serverMonitor.Start();
    }

    void _serverMonitor_OnServerDisconnected()
    {
      
    }

    void _serverMonitor_OnServerConnected()
    {
      
    }

    public override void OnSectionActivated()
    {
      _tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerIncludeRelationEnum.None);
      base.OnSectionActivated();
      mpGroupBox1.Visible = false;

      comboBoxGroups.Items.Clear();
      IList<ChannelGroup> groups = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroups(ChannelGroupIncludeRelationEnum.None);
      foreach (ChannelGroup group in groups)
        comboBoxGroups.Items.Add(new ComboBoxExItem(group.GroupName, -1, group.IdGroup));
      if (comboBoxGroups.Items.Count == 0)
        comboBoxGroups.Items.Add(new ComboBoxExItem("(no groups defined)", -1, -1));
      comboBoxGroups.SelectedIndex = 0;

      timer1.Enabled = true;


      int prio = ServiceAgents.Instance.SettingServiceAgent.GetValue("PriorityUser", 2);
      txtPrio.Text = prio.ToString();

      UpdateAdvMode();

      mpListView1.Items.Clear();

      mpButtonRec.Enabled = false;
      //mpButtonPark.Enabled = false;

      _channelNames = new Dictionary<int, string>();
      IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannels(ChannelIncludeRelationEnum.None);
      foreach (Channel ch in channels)
      {
        _channelNames.Add(ch.IdChannel, ch.Name);
      }
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      timer1.Enabled = false;
    }

    private void mpButtonTimeShift_Click(object sender, EventArgs e)
    {
      if (!ServiceHelper.IsAvailable) return;
      if (mpComboBoxChannels.SelectedItem == null) return;
      int id = ((ComboBoxExItem)mpComboBoxChannels.SelectedItem).Id;
      
      IVirtualCard card = GetCardTimeShiftingChannel(id);

      if (card != null && IsSameUser(card))
      {
        var user = card.User.Clone() as IUser;

        if (txtUsername.Text.Length > 0)
        {
          user.Name = txtUsername.Text;
          user.SubChannels.Clear();
        }

        ServiceAgents.Instance.ControllerServiceAgent.StopTimeShifting(user.Name, out user);

        //card.StopTimeShifting();
        mpButtonRec.Enabled = false;
        //mpButtonPark.Enabled = false;
      }
      else
      {
        int cardId = -1;
        foreach (ListViewItem listViewItem in mpListView1.SelectedItems)
        {
          if (listViewItem.SubItems[2].Text != "disabled")
          {
            cardId = Convert.ToInt32(listViewItem.SubItems[0].Tag);
            break; // Keep the first card enabled selected only
          }
        }                   
        IHeartbeatEventCallbackClient handler = new HeartbeatEventCallback();
        ServiceAgents.Instance.EventServiceAgent.RegisterHeartbeatCallbacks(handler);
        ICiMenuEventCallback menuHandler = new CiMenuEventCallback();
        ServiceAgents.Instance.EventServiceAgent.UnRegisterCiMenuCallbacks(menuHandler, false);
        IUser user;
        TvResult result = ServiceAgents.Instance.ControllerServiceAgent.StartTimeShifting(GetUserName(cardId, id), id, out card, out user);
        if (result != TvResult.Succeeded)
        {
          HandleTvResult(result);
        }
        else
        {
          mpButtonRec.Enabled = true;
          //mpButtonPark.Enabled = true;
        }
      }
    }

    private void HandleTvResult(TvResult result)
    {
      switch (result)
      {
        case TvResult.NoPmtFound:
          MessageBox.Show(this, "No PMT found");
          break;
        case TvResult.NoSignalDetected:
          MessageBox.Show(this, "No signal");
          break;
        case TvResult.CardIsDisabled:
          MessageBox.Show(this, "Card is not enabled");
          break;
        case TvResult.AllCardsBusy:
          MessageBox.Show(this, "All cards are busy");
          break;
        case TvResult.ChannelIsScrambled:
          MessageBox.Show(this, "Channel is scrambled");
          break;
        case TvResult.NoVideoAudioDetected:
          MessageBox.Show(this, "No Video/Audio detected");
          break;
        case TvResult.UnableToStartGraph:
          MessageBox.Show(this, "Unable to create/start graph");
          break;
        case TvResult.ChannelNotMappedToAnyCard:
          MessageBox.Show(this, "Channel is not mapped to any card");
          break;
        case TvResult.NoTuningDetails:
          MessageBox.Show(this, "No tuning information available for this channel");
          break;
        case TvResult.UnknownChannel:
          MessageBox.Show(this, "Unknown channel");
          break;
        case TvResult.UnknownError:
          MessageBox.Show(this, "Unknown error occured");
          break;
        case TvResult.GraphBuildingFailed:
          MessageBox.Show(this, "Unable to create graph");
          break;
        case TvResult.SWEncoderMissing:
          MessageBox.Show(this, "No suppported software encoder installed");
          break;
        case TvResult.NoFreeDiskSpace:
          MessageBox.Show(this, "No free disk space");
          break;
        case TvResult.TuneCancelled:
          MessageBox.Show(this, "Tune cancelled");
          break;
        case TvResult.ChannelNotActive:
          MessageBox.Show(this, "Channel not active");
          break;
      }
    }

    private bool IsSameUser(IVirtualCard card)
    {
      return txtUsername.Text.Length == 0 || card.User.Name == txtUsername.Text;
    }

    private string GetUserName(int cardId, int id)
    {
      string userName;
      if (txtUsername.Text.Length > 0 && mpCheckBoxAdvMode.Checked)
      {
        userName = txtUsername.Text;
      }
      else
      {
        userName = "setuptv-" + id + "-" + cardId;
      }
      return userName;
    }

    private void OnHeartbeatRequestReceived()
    {
      //ignore
    }

    private void mpButtonRec_Click(object sender, EventArgs e)
    {
      if (!ServiceHelper.IsAvailable) return;
      if (mpComboBoxChannels.SelectedItem == null) return;
      string channel = mpComboBoxChannels.SelectedItem.ToString();
      int id = ((ComboBoxExItem)mpComboBoxChannels.SelectedItem).Id;
      VirtualCard card = GetCardRecordingChannel(id);
      if (card != null)
      {
        card.StopRecording();
        mpButtonRec.Enabled = false;
        mpButtonTimeShift.Enabled = true;
      }
      else
      {
        card = GetCardTimeShiftingChannel(id);
        if (card != null)
        {
          foreach (var c in System.IO.Path.GetInvalidFileNameChars())
          {
            channel = channel.Replace(c, '_');
          }
          string fileName = string.Format("Test - {0}", channel);
          card.StartRecording(ref fileName);
          mpButtonTimeShift.Enabled = false;
        }
      }
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      if (!ServiceHelper.IsRestrictedMode)
      {
        bool isServiceRunning = ServiceHelper.IsRunning;
        if (isServiceRunning)
        {
          buttonRestart.Text = "Stop Service";
        }
        else
        {
          buttonRestart.Text = "Start Service";
        }
        if (!isServiceRunning || !ServiceHelper.IsInitialized)
        {
          mpButtonTimeShift.Text = "Start TimeShift";
          mpButtonTimeShift.Enabled = false;
          mpButtonRec.Text = "Record";
          mpButtonRec.Enabled = false;
          mpGroupBox1.Visible = false;
          comboBoxGroups.Enabled = false;
          mpComboBoxChannels.Enabled = false;
          mpListView1.Items.Clear();
          return;
        }
      }
      else
      {
        buttonRestart.Text = "*Restricted Mode*";
      }
      
      comboBoxGroups.Enabled = true;
      mpComboBoxChannels.Enabled = true;
      UpdateCardStatus();

      if (mpComboBoxChannels.SelectedItem != null)
      {
        int id = ((ComboBoxExItem)mpComboBoxChannels.SelectedItem).Id;
        VirtualCard card = GetCardTimeShiftingChannel(id);
        if (card != null)
        {
          mpGroupBox1.Visible = true;
          mpGroupBox1.Text = String.Format("Status of card {0}", card.Name);
          bool isLocked;
          bool isPresent;
          int strength;
          int quality;
          card.GetSignalStatus(false, out isLocked, out isPresent, out strength, out quality);
          mpLabelTunerLocked.Text = isLocked ? "Yes" : "No";
          progressBarLevel.Value = strength;
          mpLabelSignalLevel.Text = strength.ToString();
          progressBarQuality.Value = quality;
          mpLabelSignalQuality.Text = quality.ToString();

          if (!string.IsNullOrWhiteSpace(card.ChannelName))
          {
            mpLabelChannel.Text = card.ChannelName;
          }
          mpLabelRecording.Text = card.RecordingFileName;
          mpLabelTimeShift.Text = card.TimeShiftFileName;

          int bytes = 0;
          int disc = 0;
          ServiceAgents.Instance.ControllerServiceAgent.GetStreamQualityCounters(card.User.Name, out bytes, out disc);
          txtBytes.Value = bytes;
          txtDisc.Value = disc;

          //bool sameUser = IsSameUser(card);

          mpButtonTimeShift.Text = card.IsTimeShifting /*&& sameUser*/ ? "Stop TimeShift" : "Start TimeShift";          
          mpButtonRec.Text = card.IsRecording ? "Stop Rec/TimeShift" : "Record";
          mpButtonRec.Enabled = card.IsTimeShifting;
          //mpButtonPark.Enabled = card.IsTimeShifting;
          //bool isChannelParked = IsChannelParked(card, id);
          //mpButtonPark.Text = isChannelParked ? "Unpark" : "Park";
          mpButtonTimeShift.Enabled = card.IsTimeShifting; //!isChannelParked;


          return;
        }
        mpGroupBox1.Visible = false;
      }
      mpLabelTunerLocked.Text = "no";
      progressBarLevel.Value = 0;
      mpLabelSignalLevel.Text = "";
      progressBarQuality.Value = 0;
      mpLabelSignalQuality.Text = "";
      mpLabelRecording.Text = "";
      mpLabelTimeShift.Text = "";
      mpButtonRec.Text = "Record";
      //mpButtonPark.Text = "Park";
      mpButtonTimeShift.Text = "Start TimeShift";
      mpButtonTimeShift.Enabled = true;
    }

    private bool IsChannelParked(VirtualCard card, int id)
    {
      bool isChannelParked = false;
      foreach (ISubChannel subch in card.User.SubChannels.Values)
      {
        if (subch.IdChannel == id)
        {
          if (subch.Usage == TvUsage.Parked)
          {
            isChannelParked = true;
            break;
          }
        }
      }
      return isChannelParked;
    }

    private void UpdateCardStatus()
    {
      if (_tuners == null)
      {
        return;
      }
      if (_tuners.Count == 0)
      {
        return;
      }

      Utils.UpdateCardStatus(mpListView1);      
    }

    private void buttonRestart_Click(object sender, EventArgs e)
    {
      if (ServiceHelper.IsRestrictedMode)
      {
        MessageBox.Show("It is not possible to stop or restart the TV service." + Environment.NewLine + Environment.NewLine +
          "Controlling a TV service running on a remote server in the same network workgroup requires:" + Environment.NewLine +
          "1. You to be logged into an adminstrator account." + Environment.NewLine +
          "2. The existence of an administrator account with the same user name and password on the server."
          , MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }
      try
      {
        buttonRestart.Enabled = false;
        timer1.Enabled = false;

        if (ServiceHelper.IsStopped)
        {
          if (ServiceHelper.Start())
          {
            ServiceHelper.WaitInitialized();
          }
        }
        else if (ServiceHelper.IsRunning)
        {
          try
          {
            ServiceHelper.Stop();
          }
          finally
          {
            ServiceHelper.IgnoreDisconnections = true;
          }
        }
       
        timer1.Enabled = true;
      }
      finally
      {
        buttonRestart.Enabled = true;
      }
    }

    /// <summary>
    /// returns the virtualcard which is timeshifting the channel specified
    /// </summary>
    /// <param name="channelId">Id of the channel</param>
    /// <returns>virtual card</returns>
    public VirtualCard GetCardTimeShiftingChannel(int channelId)
    {
      IList<Tuner> tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerIncludeRelationEnum.None);
      foreach (Tuner tuner in tuners)
      {
        if (tuner.IsEnabled == false) continue;
        if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(tuner.IdTuner))
          continue;
        IDictionary<string, IUser> usersForCard = ServiceAgents.Instance.ControllerServiceAgent.GetUsersForCard(tuner.IdTuner);

        foreach (IUser user1 in usersForCard.Values)
        {          
          foreach (var subchannel in user1.SubChannels.Values)
          {
            if (subchannel.IdChannel == channelId)
            {
              var vcard = new VirtualCard(user1);
              if (vcard.IsTimeShifting)
              {
                return vcard;
              }
            } 
          }          
        }
      }
      return null;
    }

    /// <summary>
    /// returns the virtualcard which is recording the channel specified
    /// </summary>
    /// <param name="channelId">Id of the channel</param>
    /// <returns>virtual card</returns>
    public VirtualCard GetCardRecordingChannel(int channelId)
    {
      IList<Tuner> tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerIncludeRelationEnum.None);
      foreach (Tuner tuner in tuners)
      {
        if (tuner.IsEnabled == false) continue;
        if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(tuner.IdTuner))
          continue;
        IDictionary<string, IUser> usersForCard = ServiceAgents.Instance.ControllerServiceAgent.GetUsersForCard(tuner.IdTuner);

        foreach (IUser user in usersForCard.Values)
        {
          foreach (var subchannel in user.SubChannels.Values)
          {
            if (subchannel.IdChannel == channelId)
            {
              var vcard = new VirtualCard(user);
              if (vcard.IsRecording)
              {
                return vcard;
              }
            } 
          }          
        }
      }
      return null;
    }

    private void comboBoxGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
      ComboBoxExItem idItem = (ComboBoxExItem)comboBoxGroups.Items[comboBoxGroups.SelectedIndex];
      mpComboBoxChannels.Items.Clear();
      if (idItem.Id == -1)
      {        
        IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannels(ChannelIncludeRelationEnum.TuningDetails);
        foreach (Channel ch in channels)
        {
          if (ch.MediaType != (int)MediaType.Television) continue;
          bool hasFta = false;
          bool hasScrambled = false;
          IList<TuningDetail> tuningDetails = ch.TuningDetails;
          foreach (TuningDetail detail in tuningDetails)
          {
            if (detail.IsEncrypted)
            {
              hasScrambled = true;
            }
            else
            {
              hasFta = true;
            }
          }

          int imageIndex;
          if (hasFta && hasScrambled)
          {
            imageIndex = 5;
          }
          else if (hasScrambled)
          {
            imageIndex = 4;
          }
          else
          {
            imageIndex = 3;
          }
          ComboBoxExItem item = new ComboBoxExItem(ch.Name, imageIndex, ch.IdChannel);

          mpComboBoxChannels.Items.Add(item);
        }
      }
      else
      {
        ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetChannelGroup(idItem.Id);
        IList<GroupMap> maps = group.GroupMaps;
        bool hasFta = false;
        foreach (GroupMap map in maps)
        {
          Channel ch = map.Channel;
          if (ch.MediaType != (int)MediaType.Television)
          hasFta = false;
          bool hasScrambled = false;
          IList<TuningDetail> tuningDetails = ch.TuningDetails;
          foreach (TuningDetail detail in tuningDetails)
          {
            if (detail.IsEncrypted)
            {
              hasScrambled = true;
            }
            else
            {
              hasFta = true;
            }
          }

          int imageIndex;
          if (hasFta && hasScrambled)
          {
            imageIndex = 5;
          }
          else if (hasScrambled)
          {
            imageIndex = 4;
          }
          else
          {
            imageIndex = 3;
          }
          ComboBoxExItem item = new ComboBoxExItem(ch.Name, imageIndex, ch.IdChannel);
          mpComboBoxChannels.Items.Add(item);
        }
      }
      if (mpComboBoxChannels.Items.Count > 0)
        mpComboBoxChannels.SelectedIndex = 0;
    }

    private void mpButtonPark_Click(object sender, EventArgs e)
    {
      if (!ServiceHelper.IsAvailable) return;
      if (mpComboBoxChannels.SelectedItem == null) return;
      string channel = mpComboBoxChannels.SelectedItem.ToString();
      int id = ((ComboBoxExItem)mpComboBoxChannels.SelectedItem).Id;

      //VirtualCard card = GetCardTimeShiftingChannel(id);
      //if (card != null)
      {
        //var user = card.User.Clone() as IUser;
        //bool isChannelParked = IsChannelParked(card, id);

        /*if (txtUsername.Text.Length > 0)
        {
          user.Name = txtUsername.Text;
          user.SubChannelsCountOk.Clear();
        }

        if (isChannelParked)
        {          
          TvResult result = ServiceAgents.Instance.ControllerServiceAgent.UnParkTimeShifting(ref user, id);
          mpButtonTimeShift.Enabled = true;
        }
        else
        {
          TvResult result = ServiceAgents.Instance.ControllerServiceAgent.ParkTimeShifting(ref user, 0, id);
          mpButtonTimeShift.Enabled = false;
        } */
        double duration = 0;
        double.TryParse(txtDuration.Text, out duration);
        IUser user = new User(txtUsername.Text, UserType.Normal);
        bool result = ServiceAgents.Instance.ControllerServiceAgent.ParkTimeShifting(user.Name, duration, id, out user);
      }      
    }

    private void mpButtonUnPark_Click(object sender, EventArgs e)
    {
      if (!ServiceHelper.IsAvailable) return;
      if (mpComboBoxChannels.SelectedItem == null) return;
      string channel = mpComboBoxChannels.SelectedItem.ToString();
      int id = ((ComboBoxExItem)mpComboBoxChannels.SelectedItem).Id;

      IUser user = new User(txtUsername.Text, UserType.Normal);


      double duration = 0;
      double.TryParse(txtDuration.Text, out duration);
      IVirtualCard card;
      bool result = ServiceAgents.Instance.ControllerServiceAgent.UnParkTimeShifting(user.Name, duration, id, out user, out card);
    }

    private void mpButtonAdvStartTimeshift_Click(object sender, EventArgs e)
    {
      if (!ServiceHelper.IsAvailable) return;
      if (mpComboBoxChannels.SelectedItem == null) return;
      string channel = mpComboBoxChannels.SelectedItem.ToString();
      int id = ((ComboBoxExItem)mpComboBoxChannels.SelectedItem).Id;


      
      IVirtualCard card;

      int prio;
      bool parsed = int.TryParse(txtPrio.Text, out prio);
      TvResult result;
      IUser user;
      if (parsed)
      {
        result = ServiceAgents.Instance.ControllerServiceAgent.StartTimeShifting(txtUsername.Text, id, out card, out user, prio);
      }
      else
      {
        result = ServiceAgents.Instance.ControllerServiceAgent.StartTimeShifting(txtUsername.Text, id, out card, out user);
      }
      
      HandleTvResult(result);
    }

    private void mpButtonAdvStopTimeshift_Click(object sender, EventArgs e)
    {
      if (!ServiceHelper.IsAvailable) return;
      if (mpComboBoxChannels.SelectedItem == null) return;
      string channel = mpComboBoxChannels.SelectedItem.ToString();
      IUser user;
      bool result = ServiceAgents.Instance.ControllerServiceAgent.StopTimeShifting(txtUsername.Text, out user);
    }

    private void mpCheckBoxAdvMode_CheckedChanged(object sender, EventArgs e)
    {
      UpdateAdvMode();
    }

    private void UpdateAdvMode()
    {
      mpButtonTimeShift.Visible = !mpCheckBoxAdvMode.Checked;
      mpButtonRec.Visible = !mpCheckBoxAdvMode.Checked;
      mpButtonAdvStartTimeshift.Visible = mpCheckBoxAdvMode.Checked;
      mpButtonAdvStopTimeshift.Visible = mpCheckBoxAdvMode.Checked;
      mpButtonPark.Visible = mpCheckBoxAdvMode.Checked;
      mpButtonUnPark.Visible = mpCheckBoxAdvMode.Checked;

      label27.Visible = mpCheckBoxAdvMode.Checked;
      label5.Visible= mpCheckBoxAdvMode.Checked;
      txtUsername.Visible = mpCheckBoxAdvMode.Checked;
      txtPrio.Visible = mpCheckBoxAdvMode.Checked;
      label6.Visible = mpCheckBoxAdvMode.Checked;
      txtDuration.Visible = mpCheckBoxAdvMode.Checked;

      if (!mpCheckBoxAdvMode.Checked)
      {
        mpButtonTimeShift.Location = new Point(221, 254);
        mpButtonRec.Location = new Point(343, 254);
      }
    }
  }

  internal class CiMenuEventCallback : ICiMenuEventCallback
  {
    #region Implementation of ICiMenuEventCallback

    public void CiMenuCallback(CiMenu menu)
    {
      
    }

    #endregion
  }

  internal class HeartbeatEventCallback : IHeartbeatEventCallbackClient
  {
    #region Implementation of IHeartbeatEventCallback

    public void HeartbeatRequestReceived()
    {
    }

    #endregion
  }
}