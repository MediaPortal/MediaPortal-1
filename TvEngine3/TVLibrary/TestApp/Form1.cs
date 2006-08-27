using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.DVB;

using TvLibrary.Epg;
namespace TestApp
{
  public partial class Form1 : Form
  {
    #region imports
    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    protected static extern void StreamSetup();

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    protected static extern void StreamRun();

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    protected static extern void StreamAddTs(string streamName, string fileName);

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    protected static extern void StreamRemove(string streamName);
    #endregion

    TvCardCollection _tvcards = new TvCardCollection();
    ITVCard _currentCard;
    bool _stopStreaming = false;
    bool _streamingRunning = false;
    Player _player;

    public Form1()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      foreach (ITVCard card in _tvcards.Cards)
      {
        comboBoxCards.Items.Add(card);
      }
      if (_tvcards.Cards.Count > 0)
      {
        comboBoxCards.SelectedIndex = 0;
        _currentCard = _tvcards.Cards[0];
      }
      timer1.Tick += new EventHandler(timer1_Tick);
      timer1.Enabled = true;
    }

    void timer1_Tick(object sender, EventArgs e)
    {
      lock (this)
      {
        if (_currentCard == null) return;
        if (_currentCard.IsTunerLocked)
          labelTunerLock.Text = "yes";
        else
          labelTunerLock.Text = "no";

        int level = _currentCard.SignalLevel;
        if (level < 0) level = 0;
        if (level > 100) level = 100;
        progressBarLevel.Value = level;


        int quality = _currentCard.SignalQuality;
        if (quality < 0) quality = 0;
        if (quality > 100) quality = 100;
        if (_currentCard.Channel != null)
          labelChannel.Text = _currentCard.Channel.ToString();
        else
          labelChannel.Text = "";
        progressBarQuality.Value = quality;
        buttonScan.Enabled = (_currentCard.IsTunerLocked);
        buttonTimeShift.Enabled = (_currentCard.IsTunerLocked && (_currentCard.IsRecording == false));
        buttonRecord.Enabled = (_currentCard.IsTimeShifting || _currentCard.IsRecording);
        button3.Enabled = (_currentCard.IsTunerLocked && (_currentCard.IsRecording == false));
        button2.Enabled = (_currentCard.IsTimeShifting || _currentCard.IsRecording);
        btnEPG.Enabled = _currentCard.IsTunerLocked;
        if (_currentCard.IsRecording)
          buttonRecord.Text = "Stop recording";
        else
          buttonRecord.Text = "Record";

        if (_currentCard.IsTimeShifting)
          buttonTimeShift.Text = "Stop timeshifting";
        else
          buttonTimeShift.Text = "Timeshift";

        if (_currentCard.IsReceivingAudioVideo)
          labelScrambled.Text = "no";
        else
          labelScrambled.Text = "yes";
      }
    }

    private void buttonTune_Click(object sender, EventArgs e)
    {
      timer1.Enabled = false;/*
      DVBCChannel ch = new DVBCChannel();
      ch.Frequency = 340000;
      ch.NetworkId = 500;
      ch.TransportId = 2;
      ch.ServiceId = 2010;
      ch.PmtPid = 0x7d0;
      ch.SymbolRate = 6875;
      ch.ModulationType = DirectShowLib.BDA.ModulationType.Mod64Qam;
      ch.PcrPid = 0x7da;
      _currentCard.TuneScan(ch);
      timer1.Enabled = true;
      return;*/
      try
      {
        if ((_currentCard as TvCardAnalog) != null)
        {
          FormAnalogChannel dlg = new FormAnalogChannel();
          if (_currentCard.Channel != null)
            dlg.Channel = _currentCard.Channel;

          dlg.ShowDialog();
          _currentCard.TuneScan(dlg.Channel);
          return;
        }

        if ((_currentCard as TvCardDVBT) != null)
        {
          FormDVBTChannel dlg = new FormDVBTChannel();
          if (_currentCard.Channel != null)
            dlg.Channel = _currentCard.Channel;

          dlg.ShowDialog();
          _currentCard.TuneScan(dlg.Channel);
          return;
        }

        if ((_currentCard as TvCardDVBC) != null)
        {
          FormDVBCChannel dlg = new FormDVBCChannel();
          if (_currentCard.Channel != null)
            dlg.Channel = _currentCard.Channel;

          dlg.ShowDialog();
          _currentCard.TuneScan(dlg.Channel);
          return;
        }
        if ((_currentCard as TvCardDVBS) != null)
        {
          FormDVBSChannel dlg = new FormDVBSChannel();
          if (_currentCard.Channel != null)
            dlg.Channel = _currentCard.Channel;

          dlg.ShowDialog();
          _currentCard.TuneScan(dlg.Channel);
          return;
        }
        if ((_currentCard as TvCardDvbSS2) != null)
        {
          FormDVBSChannel dlg = new FormDVBSChannel();
          if (_currentCard.Channel != null)
            dlg.Channel = _currentCard.Channel;

          dlg.ShowDialog();
          _currentCard.TuneScan(dlg.Channel);
          return;
        }
        if ((_currentCard as TvCardATSC) != null)
        {
          FormATSCChannel dlg = new FormATSCChannel();
          if (_currentCard.Channel != null)
            dlg.Channel = _currentCard.Channel;

          dlg.ShowDialog();
          _currentCard.TuneScan(dlg.Channel);
          return;
        }
      }
      finally
      {
        timer1.Enabled = true;
      }
    }


    private void buttonScan_Click(object sender, EventArgs e)
    {
      if ((_currentCard as TvCardAnalog) != null)
      {
        MessageBox.Show("Scanning is not possible for analog tv cards");
        return;
      }
      timer1.Enabled = false;
      button3.Enabled = false;
      button2.Enabled = false;
      buttonTimeShift.Enabled = false;
      buttonRecord.Enabled = false;
      buttonTune.Enabled = false;
      buttonScan.Enabled = false;
      btnEPG.Enabled = false;
      ITVScanning scanner = _currentCard.ScanningInterface;
      scanner.Reset();
      List<IChannel> channels = scanner.Scan(_currentCard.Channel);
      scanner.Dispose();
      listViewChannels.Items.Clear();
      foreach (IChannel channel in channels)
      {
        ListViewItem item = new ListViewItem(channel.ToString());
        item.Tag = channel;
        listViewChannels.Items.Add(item);
      }
      MessageBox.Show(String.Format("Found {0} channels", channels.Count));
      buttonScan.Enabled = true;
      buttonTune.Enabled = true;
      btnEPG.Enabled = true;
      timer1.Enabled = true;

    }
    private void btnEPG_Click(object sender, EventArgs e)
    {
      if ((_currentCard as TvCardAnalog) != null)
      {
        MessageBox.Show("EPG grabbing is not possible for analog tv cards");
        return;
      }
      ITVEPG epgGrabber = _currentCard.EpgInterface;
      epgGrabber.OnEpgReceived += new EpgReceivedHandler(epgGrabber_OnEpgReceived);
      epgGrabber.GrabEpg();
      MessageBox.Show("Grabbing epg...");
    }

    void epgGrabber_OnEpgReceived(object sender, List<EpgChannel> epg)
    {
      if (epg != null)
      {
        using (FileStream stream = new FileStream("epg.xml", FileMode.OpenOrCreate, FileAccess.Write))
        {
          using (StreamWriter writer = new StreamWriter(stream))
          {
            foreach (EpgChannel epgChan in epg)
            {
              writer.WriteLine("Channel:{0} programs:{1}", epgChan.ToString(), epgChan.Programs.Count);
              foreach (EpgProgram program in epgChan.Programs)
              {
                writer.WriteLine(" {0}-{1} title:{2} genre:{3} description:{4}",
                    program.StartTime, program.EndTime, program.Text[0].Title, program.Text[0].Genre, program.Text[0].Description);

              }
            }
          }
        }
      }
      MessageBox.Show("Epg grabbed and save to epg.xml...");
    }

    private void buttonTimeShift_Click(object sender, EventArgs e)
    {
      if (_currentCard.IsTimeShifting)
      {
        _currentCard.StopTimeShifting();
        MessageBox.Show("Timeshifting stopped");
      }
      else
      {
        string fileName = String.Format("live{0}.dvr-ms", comboBoxCards.SelectedIndex);
        if (System.IO.File.Exists(fileName))
        {
          System.IO.File.Delete(fileName);
        }
        _currentCard.StartTimeShifting(fileName);
        MessageBox.Show("Timeshifting to:" + fileName);
      }
    }

    private void buttonRecord_Click(object sender, EventArgs e)
    {
      if (_currentCard.IsRecording)
      {
        _currentCard.StopRecording();
        MessageBox.Show("Recording stopped");
      }
      else
      {
        string fileName = String.Format("recording{0}.dvr-ms", comboBoxCards.SelectedIndex);
        if (System.IO.File.Exists(fileName))
        {
          System.IO.File.Delete(fileName);
        }
        _currentCard.StartRecording(DirectShowLib.SBE.RecordingType.Content, fileName, 0);
        MessageBox.Show("Recording to:" + fileName);
      }
    }

    private void listViewChannels_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listViewChannels.SelectedItems.Count <= 0) return;
      ListViewItem item = listViewChannels.SelectedItems[0];
      IChannel channel = (IChannel)item.Tag;
      _currentCard.TuneScan(channel);
    }

    private void comboBoxCards_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (comboBoxCards.SelectedIndex < 0) return;
      _currentCard = _tvcards.Cards[comboBoxCards.SelectedIndex];
      listViewChannels.Items.Clear();
    }
    protected override void OnClosed(EventArgs e)
    {
      _stopStreaming = true;
      foreach (ITVCard card in _tvcards.Cards)
      {
        card.Dispose();
      }
      base.OnClosed(e);
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (_currentCard == null) return;
      _currentCard.GrabTeletext = true;
      if (_currentCard.TeletextDecoder == null || _currentCard.GrabTeletext == false)
      {
        MessageBox.Show("This card does not support teletext");
        return;
      }
      _currentCard.TeletextDecoder.SetPageSize(400, 300);
      if (_currentCard.TeletextDecoder.NumberOfSubpages(0x100) < 0)
      {
        MessageBox.Show("Page 100/0 not found (yet)");
        return;
      }
      pictureBox1.Image = _currentCard.TeletextDecoder.GetPage(0x100, 0);
    }

    private void button3_Click(object sender, EventArgs e)
    {
      if (_currentCard.IsTimeShifting)
      {
        //_player.Stop();
        _player = null;
        _currentCard.StopTimeShifting();
        _stopStreaming = true;
        button3.Text = "Timeshift .ts";
        MessageBox.Show("Stopped .ts timeshifting");
        return;
      }
      _currentCard.StartTimeShifting("live.ts");
      button3.Text = "Stop .ts timeshift";

      //_player = new Player();
      //_player.Play(_currentCard.TimeShiftFileName,this);
      if (_streamingRunning==false)
      {
        _stopStreaming = false;
        _streamingRunning = true;
        Thread thread = new Thread(new ThreadStart(workerThread));
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Name = "Streaming thread";
        thread.Start();
      }
      //MessageBox.Show("Timeshifting to live.ts");
    }

    private void button2_Click(object sender, EventArgs e)
    {
      if (_currentCard.IsRecording)
      {
        _currentCard.StopRecording();
        button2.Text = "Record .mpg";
        MessageBox.Show("Stopped recording");
        return;
      }
      _currentCard.StartRecording(DirectShowLib.SBE.RecordingType.Content, "recording.mpg", 0);
      button2.Text = "Stop .mpg record";
      MessageBox.Show("Recording to recording.mpg");
    }

    protected void workerThread()
    {
      try
      {
        string fileAndPath = System.IO.Path.GetFullPath("live.ts.tsbuffer");
        StreamSetup();
        StreamAddTs("stream0", fileAndPath);
        while (_stopStreaming == false)
        {
          StreamRun();
        }
      }
      catch (Exception)
      {

      }
      _streamingRunning = false;
    }
  }
}