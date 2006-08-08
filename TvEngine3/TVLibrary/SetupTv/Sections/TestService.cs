using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

namespace SetupTv.Sections
{
  public partial class TestService : SectionSettings
  {
    Player _player;
    public TestService()
      : this("Manual Control")
    {
    }

    public TestService(string name)
      : base(name)
    {
      InitializeComponent();
    }


    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      mpGroupBox1.Visible = false;
      RemoteControl.Instance.EpgGrabberEnabled = true;
      mpComboBoxChannels.Items.Clear();
      EntityQuery query = new EntityQuery(typeof(Channel));
      query.AddOrderBy(Channel.SortOrderEntityColumn);
      EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>(query);
      foreach (Channel ch in channels)
      {
        if (ch.IsTv == false) continue;
        mpComboBoxChannels.Items.Add(ch.Name);

      }
      if (mpComboBoxChannels.Items.Count > 0)
        mpComboBoxChannels.SelectedIndex = 0;
      timer1.Enabled = true;

      mpListView1.Items.Clear();
      try
      {
        EntityList<Card> cards = DatabaseManager.Instance.GetEntities<Card>();
        TvServer server = new TvServer();
        foreach (Card dbsCard in cards)
        {
          VirtualCard vcard = server.Card(dbsCard.IdCard);
          CardType type = vcard.Type;
          string name = vcard.Name;
          ListViewItem item = mpListView1.Items.Add((mpListView1.Items.Count + 1).ToString());
          item.SubItems.Add(type.ToString());
          string tmp = "idle";
          if (vcard.IsTimeShifting) tmp = "Timeshifting";
          if (vcard.IsRecording) tmp = "Recording";
          item.SubItems.Add(tmp);
          IChannel channel = vcard.Channel;
          if (channel == null)
            item.SubItems.Add("");
          else
            item.SubItems.Add(channel.Name);
          item.SubItems.Add("");
          item.Tag = vcard;
        }
      }
      catch (Exception)
      {
        MessageBox.Show("Unable to access service. Is the TvService running??");
      }
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      timer1.Enabled = false;
      RemoteControl.Instance.EpgGrabberEnabled = false;
    }

    private void TestService_Load(object sender, EventArgs e)
    {

    }

    private void mpButtonTimeShift_Click(object sender, EventArgs e)
    {

      if (ServiceHelper.IsStopped) return;
      if (mpComboBoxChannels.SelectedItem == null) return;
      string channel = mpComboBoxChannels.SelectedItem.ToString();

      TvServer server = new TvServer();
      VirtualCard card = GetCardTimeShiftingChannel(channel);
      if (card != null)
      {
        if (_player != null)
        {
          _player.Stop();
          _player = null;
        }
        card.StopTimeShifting();
      }
      else
      {
        server.StartTimeShifting(channel, out card);
        if (!card.IsScrambled)
        {
          if (System.IO.File.Exists(card.TimeShiftFileName))
          {
            try
            {
              //_player = new Player();
              //_player.Play(card.TimeShiftFileName, this.Handle);
            }
            catch (Exception)
            {
              _player.Stop();
              _player = null;
            }
          }
        }

      }
    }

    private void mpButtonRec_Click(object sender, EventArgs e)
    {
      if (ServiceHelper.IsStopped) return;
      if (mpComboBoxChannels.SelectedItem == null) return;
      string channel = mpComboBoxChannels.SelectedItem.ToString();
      TvServer server = new TvServer();
      VirtualCard card = GetCardRecordingChannel(channel);
      if (card != null)
      {
        card.StopRecording();
      }
      else
      {
        card = GetCardTimeShiftingChannel(channel);
        if (card != null)
        {
          string fileName = String.Format(@"{0}\{1}.mpg", card.RecordingFolder, Utils.MakeFileName(channel));
          card.StartRecording(fileName, true, 0);
        }
      }
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      if (!ServiceHelper.IsRunning)
      {
        buttonRestart.Text = "Start Service";
        return;
      }
      buttonRestart.Text = "Stop Service";

      UpdateCardStatus();

      if (mpComboBoxChannels.SelectedItem != null)
      {
        TvServer server = new TvServer();
        string channel = mpComboBoxChannels.SelectedItem.ToString();
        VirtualCard card = GetCardTimeShiftingChannel(channel);
        if (card != null)
        {
          mpGroupBox1.Visible = true;
          mpGroupBox1.Text = String.Format("Status of card {0}", card.Name);
          if (card.IsTunerLocked)
            mpLabelTunerLocked.Text = "yes";
          else
            mpLabelTunerLocked.Text = "no";
          progressBarLevel.Value = card.SignalLevel;
          progressBarQuality.Value = card.SignalQuality;

          mpCheckBoxRec.Checked = card.IsRecording;
          mpLabelRecording.Text = card.RecordingFileName;

          mpCheckBoxTimeShift.Checked = card.IsTimeShifting;
          mpLabelTimeShift.Text = card.TimeShiftFileName;
          if (mpCheckBoxRec.Checked)
          {
            mpButtonRec.Text = "Stop Record";
          }
          else
          {
            mpButtonRec.Text = "Record";
          }
          mpButtonTimeShift.Text = "Stop TimeShift";
          return;
        }
        else
        {
          mpGroupBox1.Visible = false;
        }
      }
      mpLabelTunerLocked.Text = "no";
      progressBarLevel.Value = 0;
      progressBarQuality.Value = 0;
      mpCheckBoxRec.Checked = false;
      mpLabelRecording.Text = "";
      mpCheckBoxTimeShift.Checked = false;
      mpLabelTimeShift.Text = "";
      mpButtonRec.Text = "Record";
      mpButtonTimeShift.Text = "Start TimeShift";
    }

    private void mpCheckBoxTimeShift_CheckedChanged(object sender, EventArgs e)
    {

    }
    void UpdateCardStatus()
    {
      if (ServiceHelper.IsStopped) return;
      try
      {
        TvServer server = new TvServer();

        for (int i = 0; i < mpListView1.Items.Count; ++i)
        {
          VirtualCard card = (VirtualCard)mpListView1.Items[i].Tag;
          ListViewItem item = mpListView1.Items[i];
          string tmp = "idle";
          if (card.IsTimeShifting) tmp = "Timeshifting";
          if (card.IsRecording) tmp = "Recording";
          if (card.IsScanning) tmp = "Scanning";
          if (card.IsGrabbingEpg) tmp = "Grabbing EPG";
          item.SubItems[2].Text = tmp;

          if (tmp == "idle")
          {
            item.SubItems[4].Text = "";
          }
          else
          {
            if (card.IsScrambled) tmp = "yes";
            else tmp = "no";
            item.SubItems[4].Text = tmp;
          }

          IChannel channel = card.Channel;
          if (channel == null)
            item.SubItems[3].Text = "";
          else
            item.SubItems[3].Text = channel.Name;
        }
      }
      catch (Exception)
      {
        MessageBox.Show("Unable to access service. Is the TvService running??");
      }
    }

    private void buttonRestart_Click(object sender, EventArgs e)
    {
      try
      {
        buttonRestart.Enabled = false;
        timer1.Enabled = false;

        RemoteControl.Clear();
        if (ServiceHelper.IsStopped)
        {
          if (ServiceHelper.Start())
          {
          }
        }
        else if (ServiceHelper.IsRunning)
        {
          if (ServiceHelper.Stop())
          {
          }
        }
        RemoteControl.Clear();
        timer1.Enabled = true;
      }
      finally
      {
        buttonRestart.Enabled = true;
      }
    }

    private void mpButtonReGrabEpg_Click(object sender, EventArgs e)
    {
      RemoteControl.Instance.EpgGrabberEnabled = false;
      EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>();
      foreach (Channel ch in channels)
      {
        ch.LastGrabTime = Schedule.MinSchedule;
      }
      DatabaseManager.SaveChanges();
      RemoteControl.Instance.EpgGrabberEnabled = true;
    }


    /// <summary>
    /// returns the virtualcard which is timeshifting the channel specified
    /// </summary>
    /// <param name="channel">channel name</param>
    /// <returns>virtual card</returns>
    public VirtualCard GetCardTimeShiftingChannel(string channelName)
    {
      EntityList<Card> cards = DatabaseManager.Instance.GetEntities<Card>();
      foreach (Card card in cards)
      {
        if (RemoteControl.Instance.IsTimeShifting(card.IdCard))
        {
          if (RemoteControl.Instance.CurrentChannelName(card.IdCard) == channelName)
          {
            VirtualCard vcard=new VirtualCard(card.IdCard, RemoteControl.HostName);
            vcard.RecordingFolder = card.RecordingFolder;
            return vcard;
          }
        }
      }
      return null;
    }

    /// <summary>
    /// returns the virtualcard which is recording the channel specified
    /// </summary>
    /// <param name="channel">channel name</param>
    /// <returns>virtual card</returns>
    public VirtualCard GetCardRecordingChannel(string channel)
    {
      EntityList<Card> cards = DatabaseManager.Instance.GetEntities<Card>();
      foreach (Card card in cards)
      {
        if (RemoteControl.Instance.IsRecording(card.IdCard))
        {
          if (RemoteControl.Instance.CurrentChannelName(card.IdCard) == channel)
          {
            VirtualCard vcard = new VirtualCard(card.IdCard, RemoteControl.HostName);
            vcard.RecordingFolder = card.RecordingFolder;
            return vcard;
          }
        }
      }
      return null;
    }
  }
}