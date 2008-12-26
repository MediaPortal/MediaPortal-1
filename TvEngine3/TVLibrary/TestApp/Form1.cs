using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.DVB;

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

    readonly TvCardCollection _tvcards = new TvCardCollection();
    ITVCard _currentCard;
    bool _stopStreaming;
    bool _streamingRunning;
    int _currentPageNumber=0x600;
    //Player _player;

    public Form1()
    {
      InitializeComponent();
      Thread.CurrentThread.Name = "TestApp";
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
      timer1.Tick += timer1_Tick;
      timer1.Enabled = true;
    }

    void timer1_Tick(object sender, EventArgs e)
    {
      lock (this)
      {
        if (_currentCard == null) return;
        labelTunerLock.Text = _currentCard.IsTunerLocked ? "yes" : "no";

        int level = _currentCard.SignalLevel;
        if (level < 0) level = 0;
        if (level > 100) level = 100;
        progressBarLevel.Value = level;


        int quality = _currentCard.SignalQuality;
        if (quality < 0) quality = 0;
        if (quality > 100) quality = 100;
        if (_currentCard.SubChannels.Length > 0)
        {
          labelChannel.Text = _currentCard.SubChannels[0].CurrentChannel != null ? _currentCard.SubChannels[0].CurrentChannel.ToString() : "";
          progressBarQuality.Value = quality;
          buttonScan.Enabled = true;// (_currentCard.IsTunerLocked);
          buttonTimeShift.Enabled = (_currentCard.IsTunerLocked && (_currentCard.SubChannels[0].IsRecording == false));
          buttonRecord.Enabled = (_currentCard.SubChannels[0].IsTimeShifting || _currentCard.SubChannels[0].IsRecording);
          buttonTimeShiftTS.Enabled = (_currentCard.IsTunerLocked && (_currentCard.SubChannels[0].IsRecording == false));
          buttonRecordMpg.Enabled = (_currentCard.SubChannels[0].IsTimeShifting || _currentCard.SubChannels[0].IsRecording);
          btnEPG.Enabled = _currentCard.IsTunerLocked;
          buttonRecord.Text = _currentCard.SubChannels[0].IsRecording ? "Stop recording" : "Record";

          buttonTimeShift.Text = _currentCard.SubChannels[0].IsTimeShifting ? "Stop timeshifting" : "Timeshift";

          labelScrambled.Text = _currentCard.SubChannels[0].IsReceivingAudioVideo ? "no" : "yes";
        } else {
          buttonTimeShift.Enabled = _currentCard.IsTunerLocked;
          buttonTimeShiftTS.Enabled = _currentCard.IsTunerLocked;
          buttonRecord.Enabled = false;
          buttonRecordMpg.Enabled = false;
        }
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
          if ((_currentCard.SubChannels.Length > 0) && (_currentCard.SubChannels[0].CurrentChannel != null))
            dlg.Channel = _currentCard.SubChannels[0].CurrentChannel;

          dlg.ShowDialog();
          _currentCard.Tune(0, dlg.Channel);
          //_currentCard.TuneScan(dlg.Channel);
          return;
        }

        if ((_currentCard as TvCardDVBT) != null)
        {
          FormDVBTChannel dlg = new FormDVBTChannel();
          if (_currentCard.SubChannels[0].CurrentChannel != null)
            dlg.Channel = _currentCard.SubChannels[0].CurrentChannel;

          dlg.ShowDialog();
          _currentCard.Tune(0, dlg.Channel);
          //_currentCard.TuneScan(dlg.Channel);
          return;
        }

        if ((_currentCard as TvCardDVBC) != null)
        {
          FormDVBCChannel dlg = new FormDVBCChannel();
          if (_currentCard.SubChannels[0].CurrentChannel != null)
            dlg.Channel = _currentCard.SubChannels[0].CurrentChannel;

          dlg.ShowDialog();
          _currentCard.Tune(0, dlg.Channel);
          //_currentCard.TuneScan(dlg.Channel);
          return;
        }
        if ((_currentCard as TvCardDVBS) != null)
        {
          FormDVBSChannel dlg = new FormDVBSChannel();
          if (_currentCard.SubChannels[0].CurrentChannel != null)
            dlg.Channel = _currentCard.SubChannels[0].CurrentChannel;

          dlg.ShowDialog();
          _currentCard.Tune(0, dlg.Channel);
          //_currentCard.TuneScan(dlg.Channel);
          return;
        }
        if ((_currentCard as TvCardDvbSS2) != null)
        {
          FormDVBSChannel dlg = new FormDVBSChannel();
          if (_currentCard.SubChannels[0].CurrentChannel != null)
            dlg.Channel = _currentCard.SubChannels[0].CurrentChannel;

          dlg.ShowDialog();
          _currentCard.Tune(0, dlg.Channel);
          //_currentCard.TuneScan(dlg.Channel);
          return;
        }
        if ((_currentCard as TvCardATSC) != null)
        {
          FormATSCChannel dlg = new FormATSCChannel();
          if (_currentCard.SubChannels[0].CurrentChannel != null)
            dlg.Channel = _currentCard.SubChannels[0].CurrentChannel;

          dlg.ShowDialog();
          _currentCard.Tune(0, dlg.Channel);
          //_currentCard.TuneScan(dlg.Channel);
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
        if (MessageBox.Show(this, "Scanning is not possible for analog tv cards. (try it anyway ?:) )", "Not posible, Try anyway ?", MessageBoxButtons.YesNo) != DialogResult.Yes)
          return;
      }
      timer1.Enabled = false;
      buttonTimeShiftTS.Enabled = false;
      buttonRecordMpg.Enabled = false;
      buttonTimeShift.Enabled = false;
      buttonRecord.Enabled = false;
      buttonTune.Enabled = false;
      buttonScan.Enabled = false;
      btnEPG.Enabled = false;
      ITVScanning scanner = _currentCard.ScanningInterface;
      scanner.Reset();
      List<IChannel> channels = scanner.Scan(_currentCard.SubChannels[0].CurrentChannel, new TvLibrary.ScanParameters());
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
        MessageBox.Show(this, "EPG grabbing is not possible for analog tv cards");
        return;
      }
      // ITVEPG epgGrabber = _currentCard.EpgInterface;
      // epgGrabber.OnEpgReceived += new EpgReceivedHandler(epgGrabber_OnEpgReceived);
      //epgGrabber.GrabEpg();
      MessageBox.Show(this, "Grabbing epg...");
    }

    private void buttonTimeShift_Click(object sender, EventArgs e)
    {
      if (_currentCard.SubChannels[0].IsTimeShifting)
      {
        _currentCard.SubChannels[0].StopTimeShifting();
        MessageBox.Show(this, "Timeshifting stopped");
      }
      else
      {
        string fileName = String.Format("live{0}.dvr-ms", comboBoxCards.SelectedIndex);
        if (File.Exists(fileName))
        {
          File.Delete(fileName);
        }
        _currentCard.SubChannels[0].StartTimeShifting(fileName);
        MessageBox.Show(this, "Timeshifting to:" + fileName);
      }
    }

    private void buttonRecord_Click(object sender, EventArgs e)
    {
      if (_currentCard.SubChannels[0].IsRecording)
      {
        _currentCard.SubChannels[0].StopRecording();
        MessageBox.Show(this, "Recording stopped");
      }
      else
      {
        string fileName = String.Format("recording{0}.dvr-ms", comboBoxCards.SelectedIndex);
        if (File.Exists(fileName))
        {
          File.Delete(fileName);
        }
        _currentCard.SubChannels[0].StartRecording(false, fileName);
        MessageBox.Show(this, "Recording to:" + fileName);
      }
    }

    private void listViewChannels_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listViewChannels.SelectedItems.Count <= 0) return;
      ListViewItem item = listViewChannels.SelectedItems[0];
      IChannel channel = (IChannel)item.Tag;
      _currentCard.Tune(0, channel);
      //_currentCard.TuneScan(channel);
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
      _currentCard.SubChannels[0].GrabTeletext = true;
      if (_currentCard.SubChannels[0].TeletextDecoder == null || _currentCard.SubChannels[0].GrabTeletext == false)
      {
        MessageBox.Show(this, "This card does not support teletext");
        return;
      }
      _currentCard.SubChannels[0].TeletextDecoder.SetPageSize(400, 400);
      _currentCard.SubChannels[0].TeletextDecoder.OnPageDeleted += TeletextDecoder_OnPageDeleted;
      _currentCard.SubChannels[0].TeletextDecoder.OnPageAdded += TeletextDecoder_OnPageAdded;
      _currentCard.SubChannels[0].TeletextDecoder.OnPageUpdated += TeletextDecoder_OnPageUpdated;
    }

    static void TeletextDecoder_OnPageDeleted(int pageNumber, int subPageNumber)
    {
    }

    void UpdatePage(int pageNumber, int subPageNumber)
    {
      if (pageNumber ==_currentPageNumber) return;
      /*
      if (pageNumber == 0x600)
      {
        byte[] page = _currentCard.TeletextDecoder.GetRawPage(pageNumber, subPageNumber);

        using (FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate))
        {
          stream.Write(page, 0, 1008);
        }
      }*/
      //if (pageNumber == 0x600)
      //  pictureBox1.Image = _currentCard.TeletextDecoder.GetPage(pageNumber, subPageNumber);
      // if (pageNumber == 0x601)
     //   pictureBox2.Image = _currentCard.TeletextDecoder.GetPage(pageNumber, subPageNumber);
      if (pageNumber == 0x602)
        pictureBox3.Image = _currentCard.SubChannels[0].TeletextDecoder.GetPage(pageNumber, subPageNumber);
     // if (pageNumber == 0x603)
     //   pictureBox4.Image = _currentCard.TeletextDecoder.GetPage(pageNumber, subPageNumber);

    }

    void TeletextDecoder_OnPageUpdated(int pageNumber, int subPageNumber)
    {
        UpdatePage(pageNumber, subPageNumber);
    }

    void TeletextDecoder_OnPageAdded(int pageNumber, int subPageNumber)
    {
      UpdatePage(pageNumber, subPageNumber);
    }

    private void buttonTimeShiftTS_Click(object sender, EventArgs e)
    {
      if (_currentCard.SubChannels[0].IsTimeShifting)
      {
        //_player.Stop();
        //_player = null;
        _currentCard.SubChannels[0].StopTimeShifting();
        _stopStreaming = true;
        buttonTimeShiftTS.Text = "Timeshift .ts";
        MessageBox.Show(this, "Stopped .ts timeshifting");
        return;
      }
      _currentCard.SubChannels[0].StartTimeShifting("live.ts");
      buttonTimeShiftTS.Text = "Stop .ts timeshift";

      //_player = new Player();
      //_player.Play(_currentCard.TimeShiftFileName,this);
      if (_streamingRunning == false)
      {
        _stopStreaming = false;
        _streamingRunning = true;
        Thread thread = new Thread(workerThread);
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Name = "Streaming thread";
        thread.Start();
      }
      //MessageBox.Show(this,"Timeshifting to live.ts");
    }

    private void buttonRecordMpg_Click(object sender, EventArgs e)
    {
      if (_currentCard.SubChannels[0].IsRecording)
      {
        _currentCard.SubChannels[0].StopRecording();
        buttonRecordMpg.Text = "Record .mpg";
        MessageBox.Show(this, "Stopped recording");
        return;
      }
      _currentCard.SubChannels[0].StartRecording(false, "recording.mpg");
      buttonRecordMpg.Text = "Stop .mpg record";
      MessageBox.Show(this, "Recording to recording.mpg");
    }

    protected void workerThread()
    {
      try
      {
        string fileAndPath = Path.GetFullPath("live.ts.tsbuffer");
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

    private void pictureBox3_Click(object sender, EventArgs e)
    {

    }

    private void textBoxPageNr_TextChanged(object sender, EventArgs e)
    {
      try
      {
          int pageNumber=Convert.ToInt32(textBoxPageNr.Text,16);
          _currentPageNumber = pageNumber;
          pictureBox3.Image = _currentCard.SubChannels[0].TeletextDecoder.GetPage(_currentPageNumber, 0);
      }catch(Exception){}
      
    }
  }
}