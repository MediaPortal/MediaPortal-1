/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using TvDatabase;

using TvControl;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using DirectShowLib.BDA;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Xml;

namespace SetupTv.Sections
{
  public partial class CardDvbC : SectionSettings
  {
    readonly int _cardNumber;

    private List<DVBCTuning> _dvbcChannels = new List<DVBCTuning>();
    bool _isScanning;
    bool _stopScanning;

    FileFilters fileFilters;

    CI_Menu_Dialog ciMenuDialog; // ci menu dialog object

    public CardDvbC()
      : this("DVBC")
    {
    }
    public CardDvbC(string name)
      : base(name)
    {
    }

    public CardDvbC(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      //insert complete ci menu dialog to tab
      Card dbCard = Card.Retrieve(_cardNumber);
      if (dbCard.CAM == true)
      {
        ciMenuDialog = new CI_Menu_Dialog(_cardNumber);
        this.tabPageCIMenu.Controls.Add(ciMenuDialog);
      }
      else
      {
        this.tabPageCIMenu.Dispose();
      }
      base.Text = name;
      Init();
    }
    /// <summary>
    /// Saves a new list with found transponders
    /// </summary>
    /// <param name="fileName">Path for output filename</param>
    void SaveList(string fileName)
    {
      try
      {
        System.IO.TextWriter parFileXML = System.IO.File.CreateText(fileName);
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<DVBCTuning>));
        xmlSerializer.Serialize(parFileXML, _dvbcChannels);
        parFileXML.Close();
      }
      catch (Exception ex)
      {
        Log.Error("Error saving tuningdetails: {0}", ex.ToString());
        MessageBox.Show("Transponder list could not be saved, check error.log for details.");
      }
    }

    /// <summary>
    /// Load existing list from xml file 
    /// </summary>
    /// <param name="fileName">Path for input filen</param>
    void LoadList(string fileName)
    {
      try
      {
        XmlReader parFileXML = XmlReader.Create(fileName);
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<DVBCTuning>));
        _dvbcChannels = (List<DVBCTuning>)xmlSerializer.Deserialize(parFileXML);
        parFileXML.Close();
      }
      catch (Exception ex)
      {
        Log.Error("Error loading tuningdetails: {0}", ex.ToString());
        MessageBox.Show("Transponder list could not be loaded, check error.log for details.");
      }
    }

    void Init()
    {
      mpComboBoxCountry.Items.Clear();
      try
      {
        fileFilters = new FileFilters("DVBC", ref mpComboBoxCountry, ref mpComboBoxRegion);
      }
      catch (Exception)
      {
        MessageBox.Show(@"Unable to open TuningParameters\dvbc\*.xml");
        return;
      }
      SetButtonState();
    }

    void UpdateStatus()
    {
      progressBarLevel.Value = Math.Min(100, RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));

    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      mpComboBoxMod.SelectedIndex = 3;
      UpdateStatus();
      TvBusinessLayer layer = new TvBusinessLayer();
      int index = Int32.Parse(layer.GetSetting("dvbc" + _cardNumber + "Country", "0").Value);
      if (index < mpComboBoxCountry.Items.Count)
        mpComboBoxCountry.SelectedIndex = index;


      checkBoxCreateGroups.Checked = (layer.GetSetting("dvbc" + _cardNumber + "creategroups", "false").Value == "true");

      if (ciMenuDialog != null)
      {
        ciMenuDialog.OnSectionActivated();
      }
    }
    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbc" + _cardNumber + "Country", "0");
      setting.Value = mpComboBoxCountry.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbc" + _cardNumber + "creategroups", "false");
      setting.Value = checkBoxCreateGroups.Checked ? "true" : "false";
      setting.Persist();

      if (ciMenuDialog != null)
      {
        ciMenuDialog.OnSectionDeActivated();
      }
    }



    private void mpButtonScanTv_Click_1(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Card is disabled, please enable the card before scanning");
          return;
        }
        if (!RemoteControl.Instance.CardPresent(card.IdCard))
        {
          MessageBox.Show(this, "Card is not found, please make sure card is present before scanning");
          return;
        }
        // Check if the card is locked for scanning.
        User user;
        if (RemoteControl.Instance.IsCardInUse(_cardNumber, out user))
        {
          MessageBox.Show(this, "Card is locked. Scanning not possible at the moment ! Perhaps you are scanning an other part of a hybrid card.");
          return;
        }
        SetButtonState();
        CustomFileName tuningFile = (CustomFileName)mpComboBoxRegion.SelectedItem;
        _dvbcChannels = (List<DVBCTuning>)fileFilters.LoadList(tuningFile.FileName, typeof(List<DVBCTuning>));
        if (_dvbcChannels == null)
        {
          return;
        }
        StartScanThread();
        listViewStatus.Items.Clear();
      }
      else
      {
        _stopScanning = true;
      }
    }

    private void StartScanThread()
    {
      Thread scanThread = new Thread(DoScan);
      scanThread.Name = "DVB-C scan thread";
      scanThread.Start();
    }
    
    class suminfo
    {
      public int newChannel = 0;
      public int updChannel = 0;
      public int newChannelSum = 0;
      public int updChannelSum = 0;
    }

    /// <summary>
    /// Scan Thread
    /// </summary>
    void DoScan()
    {
      suminfo tv = new suminfo();
      suminfo radio = new suminfo();

      string buttonText = mpButtonScanTv.Text;
      User user = new User();
      user.CardId = _cardNumber;
      try
      {
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        RemoteControl.Instance.EpgGrabberEnabled = false;
        if (_dvbcChannels.Count == 0)
          return;

        SetButtonState();
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

        for (int index = 0; index < _dvbcChannels.Count; ++index)
        {
          if (_stopScanning)
            return;
          float percent = ((float)(index)) / _dvbcChannels.Count;
          percent *= 100f;
          if (percent > 100f)
            percent = 100f;
          progressBar1.Value = (int)percent;


          DVBCChannel tuneChannel = new DVBCChannel(_dvbcChannels[index]); // new DVBCChannel();
          string line = String.Format("{0}tp- {1}", 1 + index, tuneChannel.TuningInfo.ToString());
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();

          if (index == 0)
          {
            RemoteControl.Instance.Tune(ref user, tuneChannel, -1);
          }

          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
          UpdateStatus();

          if (channels == null || channels.Length == 0)
          {
            if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
            {
              line = String.Format("{0}tp- {1} {2} {3}:No signal", 1 + index, tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
            line = String.Format("{0}tp- {1} {2} {3}:Nothing found", 1 + index, tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate);
            item.Text = line;
            item.ForeColor = Color.Red;
            continue;
          }

          radio.newChannel = 0;
          radio.updChannel = 0;
          tv.newChannel = 0;
          tv.updChannel = 0;
          for (int i = 0; i < channels.Length; ++i)
          {
            Channel dbChannel;
            DVBCChannel channel = (DVBCChannel)channels[i];
            //TuningDetail currentDetail = layer.GetChannel(channel);
            TuningDetail currentDetail = layer.GetChannel(channel.Provider, channel.Name, channel.ServiceId);
            bool exists;
            if (currentDetail == null)
            {
              //add new channel
              exists = false;
              dbChannel = layer.AddChannel(channel.Provider, channel.Name);
              dbChannel.SortOrder = 10000;
              if (channel.LogicalChannelNumber >= 1)
              {
                dbChannel.SortOrder = channel.LogicalChannelNumber;
              }
            }
            else
            {
              exists = true;
              dbChannel = currentDetail.ReferencedChannel();
            }

            dbChannel.IsTv = channel.IsTv;
            dbChannel.IsRadio = channel.IsRadio;
            dbChannel.FreeToAir = channel.FreeToAir;
            dbChannel.Persist();

            if (dbChannel.IsTv)
            {
              layer.AddChannelToGroup(dbChannel, TvConstants.TvGroupNames.AllChannels);
              if (checkBoxCreateGroups.Checked)
              {
                layer.AddChannelToGroup(dbChannel, channel.Provider);
              }
            }
            if (dbChannel.IsRadio)
            {
              layer.AddChannelToRadioGroup(dbChannel, TvConstants.RadioGroupNames.AllChannels);
              if (checkBoxCreateGroups.Checked)
              {
                layer.AddChannelToRadioGroup(dbChannel, channel.Provider);
              }
            }

            if (currentDetail == null)
            {
              layer.AddTuningDetails(dbChannel, channel);
            }
            else
            {
              //update tuning details...
              TuningDetail td = layer.UpdateTuningDetails(dbChannel, channel, currentDetail);
              td.Persist();
            }

            if (channel.IsTv)
            {
              if (exists)
              {
                tv.updChannel++;
              }
              else
              {
                tv.newChannel++;
              }
            }
            if (channel.IsRadio)
            {
              if (exists)
              {
                radio.updChannel++;
              }
              else
              {
                radio.newChannel++;
              }
            }
            layer.MapChannelToCard(card, dbChannel, false);
            line = String.Format("{0}tp- {1} {2} {3}:New TV/Radio:{4}/{5} Updated TV/Radio:{6}/{7}", 1 + index, tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate, tv.newChannel, radio.newChannel, tv.updChannel, radio.updChannel);
            item.Text = line;
          }
          tv.updChannelSum += tv.updChannel;
          tv.newChannelSum += tv.newChannel;
          radio.updChannelSum += radio.updChannel;
          radio.newChannelSum += radio.newChannel;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        RemoteControl.Instance.StopCard(user);
        RemoteControl.Instance.EpgGrabberEnabled = true;
        progressBar1.Value = 100;
        mpButtonScanTv.Text = buttonText;
        _isScanning = false;
        SetButtonState();
      }
      listViewStatus.Items.Add(new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", radio.newChannelSum, radio.updChannelSum)));
      listViewStatus.Items.Add(new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", tv.newChannelSum, tv.updChannelSum)));
      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
      lastItem.EnsureVisible();
    }

    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      if (card.Enabled == false)
      {
        MessageBox.Show(this, "Card is disabled, please enable the card before scanning");
        return;
      }
      if (!RemoteControl.Instance.CardPresent(card.IdCard))
      {
        MessageBox.Show(this, "Card is not found, please make sure card is present before scanning");
        return;
      }
      RemoteControl.Instance.EpgGrabberEnabled = false;
      SetButtonState();
      _dvbcChannels.Clear();
      DVBCChannel tuneChannel = GetManualTuning();

      listViewStatus.Items.Clear();
      string line = String.Format("Scan freq:{0} {1} symbolrate:{2} ...", tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate);
      ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
      item.EnsureVisible();
      Application.DoEvents();
      IChannel[] channels = RemoteControl.Instance.ScanNIT(_cardNumber, tuneChannel);
      if (channels != null)
      {
        for (int i = 0; i < channels.Length; ++i)
        {
          DVBCChannel ch = (DVBCChannel)channels[i];
          _dvbcChannels.Add(ch.TuningInfo);
          item = listViewStatus.Items.Add(new ListViewItem(ch.TuningInfo.ToString()));
          item.EnsureVisible();
        }
      }

      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Scan done, found {0} transponders...", _dvbcChannels.Count)));
      lastItem.EnsureVisible();
      mpButtonScanNIT.Enabled = true;

      RemoteControl.Instance.EpgGrabberEnabled = true;
      if (_dvbcChannels.Count != 0)
      {
        if (DialogResult.Yes == MessageBox.Show(String.Format("Found {0} transponders. Would you like to scan those?", _dvbcChannels.Count), "Manual scan results", MessageBoxButtons.YesNo))
        {
          StartScanThread();
        }
      }
      SetButtonState();
    }

    /// <summary>
    /// Sets correct button state 
    /// </summary>
    private void SetButtonState()
    {
      mpButtonScanSingleTP.Enabled = !_isScanning;
      mpComboBoxCountry.Enabled = !_isScanning;
      mpComboBoxRegion.Enabled = !_isScanning;
      mpButtonScanNIT.Enabled = !_isScanning;
      mpButtonSaveList.Enabled = (_dvbcChannels.Count != 0) && !_isScanning;
    }

    /// <summary>
    /// Get Tunining details from manual scan section
    /// </summary>
    /// <returns></returns>
    private DVBCChannel GetManualTuning()
    {
      DVBCChannel tuneChannel = new DVBCChannel();
      tuneChannel.Frequency = Int32.Parse(textBoxFreq.Text);
      tuneChannel.ModulationType = (ModulationType)mpComboBoxMod.SelectedIndex;
      tuneChannel.SymbolRate = Int32.Parse(textBoxSymbolRate.Text);
      return tuneChannel;
    }

    private void CardDvbC_Load(object sender, EventArgs e)
    {

    }
    /// <summary>
    /// Saves current transponder list
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void mpButtonSaveList_Click(object sender, EventArgs e)
    {
      if (_dvbcChannels.Count != 0)
      {
        String filePath = String.Format(@"{0}\TuningParameters\dvbc\Manual_Scans.{1}.xml", Log.GetPathName(), DateTime.Now.ToString("yyyy-MM-dd"));
        SaveList(filePath);
        Init(); // refresh list
      }
    }
    /// <summary>
    /// Scans a manual entered transponder for channels
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void mpButtonScanSingleTP_Click(object sender, EventArgs e)
    {
      DVBCChannel tuneChannel = GetManualTuning();
      _dvbcChannels.Clear();
      _dvbcChannels.Add(tuneChannel.TuningInfo);
      StartScanThread();
    }

  }
}
