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
    #region Member variables
    readonly int _cardNumber;

    private List<DVBCTuning> _dvbcChannels = new List<DVBCTuning>();
    private String buttonText;

    FileFilters fileFilters;

    CI_Menu_Dialog ciMenuDialog; // ci menu dialog object

    ScanState scanState; // scan state

    bool _isScanning
    {
      get 
      {
        return scanState == ScanState.Scanning || scanState == ScanState.Cancel; 
      }
    }

    #endregion 

    #region Properties
    /// <summary>
    /// Returns active scan type
    /// </summary>
    private ScanTypes ActiveScanType
    {
      get
      {
        if (checkBoxAdvancedTuning.Checked == false)
        {
          return ScanTypes.Predefined;
        }
        if (scanPredefProvider.Checked == true)
        {
          return ScanTypes.Predefined;
        }
        if (scanSingleTransponder.Checked == true)
        {
          return ScanTypes.SingleTransponder;
        }
        if (scanNIT.Checked == true)
        {
          return ScanTypes.NIT;
        }
        return ScanTypes.Predefined;
      }
    }
    #endregion

    #region Constructors
    private void EnableSections()
    {
 
    }
    
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
    #endregion

    #region Init and Section (de-)Activate
    void Init()
    {
      // set to same positions as progress
      mpGrpAdvancedTuning.Top = mpGrpScanProgress.Top;
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
      SetDefaults();

      buttonText = mpButtonScanTv.Text;

      scanState = ScanState.Initialized;
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      mpComboBoxMod.SelectedIndex = 3;
      UpdateStatus();
      SetDefaults();

      if (ciMenuDialog != null)
      {
        ciMenuDialog.OnSectionActivated();
      }
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      PersistState();

      if (ciMenuDialog != null)
      {
        ciMenuDialog.OnSectionDeActivated();
      }
    }
    #endregion

    #region Loading and Saving functions

    /// <summary>
    /// Saves current transponder list
    /// </summary>
    private void SaveTransponderList()
    {
      if (_dvbcChannels.Count != 0)
      {
        String filePath = String.Format(@"{0}\TuningParameters\dvbc\Manual_Scans.{1}.xml", Log.GetPathName(), DateTime.Now.ToString("yyyy-MM-dd"));
        SaveList(filePath);
        PersistState();
        Init(); // refresh list
      }
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

    /// <summary>
    /// Reads previous settings and assign them to controls
    /// </summary>
    private void SetDefaults()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      int index = Math.Max(Int32.Parse(layer.GetSetting("dvbc" + _cardNumber + "Country", "0").Value), 0); // limit to >= 0
      if (index < mpComboBoxCountry.Items.Count)
      {
        mpComboBoxCountry.SelectedIndex = index;
      }

      index = Math.Max(Int32.Parse(layer.GetSetting("dvbc" + _cardNumber + "Region", "0").Value), 0); // limit to >= 0
      if (index < mpComboBoxRegion.Items.Count)
      {
        mpComboBoxRegion.SelectedIndex = index;
      }

      textBoxFreq.Text = layer.GetSetting("dvbc" + _cardNumber + "Freq", "306000").Value;
      textBoxSymbolRate.Text = layer.GetSetting("dvbc" + _cardNumber + "Symbolrate", "6900").Value;

      index = Math.Max(Int32.Parse(layer.GetSetting("dvbc" + _cardNumber + "Modulation", "3").Value), 0); // limit to >= 0
      if (index < mpComboBoxMod.Items.Count)
      {
        mpComboBoxMod.SelectedIndex = index;
      }

      checkBoxCreateGroups.Checked = (layer.GetSetting("dvbc" + _cardNumber + "creategroups", "false").Value == "true");
    }

    /// <summary>
    /// Saves control status
    /// </summary>
    private void PersistState()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbc" + _cardNumber + "Country", "0");
      setting.Value = mpComboBoxCountry.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbc" + _cardNumber + "Region", "0");
      setting.Value = mpComboBoxRegion.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbc" + _cardNumber + "Freq", "306000");
      setting.Value = textBoxFreq.Text;
      setting.Persist();

      setting = layer.GetSetting("dvbc" + _cardNumber + "Symbolrate", "6900");
      setting.Value = textBoxSymbolRate.Text;
      setting.Persist();

      setting = layer.GetSetting("dvbc" + _cardNumber + "Modulation", "3");
      setting.Value = mpComboBoxMod.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbc" + _cardNumber + "creategroups", "false");
      setting.Value = checkBoxCreateGroups.Checked ? "true" : "false";
      setting.Persist();
    }

    /// <summary>
    /// Get Tuning details from manual scan section
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

    #endregion

    #region Scan handling
    private void InitScanProcess()
    {
      // once completed reset to new beginning
      switch (scanState)
      {
        case ScanState.Done:
          scanState = ScanState.Initialized;
          listViewStatus.Items.Clear();
          SetButtonState();
          return;

        case ScanState.Initialized:
          // common checks
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
          ShowActiveGroup(1); // force progess visible
          // End common checks

          listViewStatus.Items.Clear();

          // Scan type dependent handling
          _dvbcChannels.Clear();
          switch (ActiveScanType)
          {
            // use tuning details from file
            case ScanTypes.Predefined:
              CustomFileName tuningFile = (CustomFileName)mpComboBoxRegion.SelectedItem;
              _dvbcChannels = (List<DVBCTuning>)fileFilters.LoadList(tuningFile.FileName, typeof(List<DVBCTuning>));
              if (_dvbcChannels == null)
              {
                _dvbcChannels = new List<DVBCTuning>();
              }
              break;

            // scan Network Information Table for transponder info
            case ScanTypes.NIT:
              _dvbcChannels.Clear();
              DVBCChannel tuneChannel = GetManualTuning();

              listViewStatus.Items.Clear();
              string line = String.Format("Scan freq:{0} {1} symbolrate:{2} ...", tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate);
              ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
              item.EnsureVisible();

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

              // automatically save list for re-use
              SaveTransponderList();
              break;

            // scan only single inputted transponder
            case ScanTypes.SingleTransponder:
              DVBCChannel singleTuneChannel = GetManualTuning();
              _dvbcChannels.Add(singleTuneChannel.TuningInfo);
              break;
          }
          if (_dvbcChannels.Count != 0)
          {
            StartScanThread();
          }
          break;

        case ScanState.Scanning:
          scanState = ScanState.Cancel;
          SetButtonState();
          break;

        case ScanState.Cancel:
          return;
      }
    }

    private void StartScanThread()
    {
      Thread scanThread = new Thread(DoScan);
      scanThread.Name = "DVB-C scan thread";
      scanThread.Start();
    }

    /// <summary>
    /// Updates signal level info
    /// </summary>
    void UpdateStatus()
    {
      progressBarLevel.Value = Math.Min(100, RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));
    }

    #region Scan Thread
    /// <summary>
    /// Scan Thread
    /// </summary>
    void DoScan()
    {
      suminfo tv = new suminfo();
      suminfo radio = new suminfo();
      User user = new User();
      user.CardId = _cardNumber;
      try
      {
        scanState = ScanState.Scanning;
        if (_dvbcChannels.Count == 0)
          return;

        RemoteControl.Instance.EpgGrabberEnabled = false;

        SetButtonState();
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

        for (int index = 0; index < _dvbcChannels.Count; ++index)
        {
          if (scanState == ScanState.Cancel)
            return;

          float percent = ((float)(index)) / _dvbcChannels.Count;
          percent *= 100f;
          if (percent > 100f)
            percent = 100f;
          progressBar1.Value = (int)percent;

          Application.DoEvents();

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
              layer.AddChannelToGroup(dbChannel, TvConstants.TvGroupNames.DVBC);
              if (checkBoxCreateGroups.Checked)
              {
                layer.AddChannelToGroup(dbChannel, channel.Provider);
              }
            }
            if (dbChannel.IsRadio)
            {
              layer.AddChannelToRadioGroup(dbChannel, TvConstants.RadioGroupNames.AllChannels);
              layer.AddChannelToRadioGroup(dbChannel, TvConstants.RadioGroupNames.DVBC);
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
                tv.newChannels.Add(channel);
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
                radio.newChannels.Add(channel);
              }
            }
            layer.MapChannelToCard(card, dbChannel, false);
            line = String.Format("{0}tp- {1} {2} {3}:New TV/Radio:{4}/{5} Updated TV/Radio:{6}/{7}", 1 + index, tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate, tv.newChannel, radio.newChannel, tv.updChannel, radio.updChannel);
            item.Text = line;
          }
          tv.updChannelSum += tv.updChannel;
          radio.updChannelSum += radio.updChannel;
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

        scanState = ScanState.Done;
        SetButtonState();
      }
      listViewStatus.Items.Add(new ListViewItem(String.Format("Total radio channels updated:{0}, new:{1}", radio.updChannelSum, radio.newChannelSum)));
      foreach (IChannel newChannel in radio.newChannels)
      {
        listViewStatus.Items.Add(new ListViewItem(String.Format("  -> new channel: {0}",newChannel.Name)));
      }

      listViewStatus.Items.Add(new ListViewItem(String.Format("Total tv channels updated:{0}, new:{1}", tv.updChannelSum, tv.newChannelSum)));
      foreach (IChannel newChannel in tv.newChannels)
      {
        listViewStatus.Items.Add(new ListViewItem(String.Format("  -> new channel: {0}", newChannel.Name)));
      }
      ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
      lastItem.EnsureVisible();
    }
    #endregion

    #endregion

    #region GUI handling
    /// <summary>
    /// Sets correct button state 
    /// </summary>
    private void SetButtonState()
    {
      mpComboBoxCountry.Enabled = !_isScanning && ActiveScanType == ScanTypes.Predefined;
      mpComboBoxRegion.Enabled = !_isScanning && ActiveScanType == ScanTypes.Predefined;
   
      textBoxFreq.Enabled = ActiveScanType != ScanTypes.Predefined;
      textBoxSymbolRate.Enabled = ActiveScanType != ScanTypes.Predefined;
      mpComboBoxMod.Enabled = ActiveScanType != ScanTypes.Predefined;

      int forceProgress = 0;
      switch (scanState)
      {
        default:
        case ScanState.Initialized:
          mpButtonScanTv.Text = "Scan for channels";
          break;

        case ScanState.Scanning:
          mpButtonScanTv.Text = "Cancel...";
          break;

        case ScanState.Cancel:
          mpButtonScanTv.Text = "Cancelling...";
          break;

        case ScanState.Done:
          mpButtonScanTv.Text = "New scan";
          forceProgress = 1; // leave window open
          break;
      }

      ShowActiveGroup(forceProgress);
    }

    /// <summary>
    /// Show either scan option or progress
    /// </summary>
    /// <param name="ForceShowProgress">1 to force progress visible</param>
    private void ShowActiveGroup(int ForceShowProgress)
    {
      if (ForceShowProgress == 1)
      {
        mpGrpAdvancedTuning.Visible = false;
        mpGrpScanProgress.Visible = true;
        checkBoxAdvancedTuning.Enabled = false;
      }
      else
      {
        mpGrpAdvancedTuning.Visible = checkBoxAdvancedTuning.Checked && !_isScanning;
        mpGrpScanProgress.Visible = _isScanning;
        checkBoxAdvancedTuning.Enabled = !_isScanning;
      }

      if (mpGrpAdvancedTuning.Visible)
      {
        mpGrpAdvancedTuning.BringToFront();
      }
      if (mpGrpScanProgress.Visible)
      {
        mpGrpScanProgress.BringToFront();
      }
      UpdateZOrder();
      Application.DoEvents();
      Thread.Sleep(100);
    }
    #endregion

    #region GUI event handlers
    private void mpButtonScanTv_Click_1(object sender, EventArgs e)
    {
      InitScanProcess();
    }

    private void UpdateGUIControls(object sender, EventArgs e)
    {
      SetButtonState();
    }
    #endregion
  }
}
