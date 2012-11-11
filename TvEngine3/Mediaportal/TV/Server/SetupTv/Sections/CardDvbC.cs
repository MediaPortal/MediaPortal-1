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
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections.CIMenu;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using MediaPortal.Common.Utils;
using DirectShowLib.BDA;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Xml;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class CardDvbC : SectionSettings
  {


    #region Member variables

    private readonly int _cardNumber;

    private List<DVBCTuning> _dvbcChannels = new List<DVBCTuning>();
    private String buttonText;

    private FileFilters fileFilters;

    private CI_Menu_Dialog ciMenuDialog; // ci menu dialog object

    private ScanState scanState; // scan state

    private bool _isScanning
    {
      get { return scanState == ScanState.Scanning || scanState == ScanState.Cancel; }
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

    private void EnableSections() {}

    public CardDvbC()
      : this("DVBC") {}

    public CardDvbC(string name)
      : base(name) {}

    public CardDvbC(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      //insert complete ci menu dialog to tab
      Card dbCard = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardNumber, CardIncludeRelationEnum.None);
      if (dbCard.UseConditionalAccess == true)
      {
        ciMenuDialog = new CI_Menu_Dialog(_cardNumber);
        tabPageCIMenu.Controls.Add(ciMenuDialog);
      }
      else
      {
        tabPageCIMenu.Dispose();
      }
      base.Text = name;
      Init();
    }

    #endregion

    #region Init and Section (de-)Activate

    private void Init()
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

      checkBoxCreateSignalGroup.Text = "Create \"" + TvConstants.TvGroupNames.DVBC + "\" group";

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
        String filePath = String.Format(@"{0}\TuningParameters\dvbc\Manual_Scans.{1}.xml", PathManager.GetDataPath,
                                        DateTime.Now.ToString("yyyy-MM-dd"));
        SaveList(filePath);
        PersistState();
        Init(); // refresh list
      }
    }

    /// <summary>
    /// Saves a new list with found transponders
    /// </summary>
    /// <param name="fileName">Path for output filename</param>
    private void SaveList(string fileName)
    {
      try
      {
        System.IO.TextWriter parFileXML = System.IO.File.CreateText(fileName);
        XmlSerializer xmlSerializer = new XmlSerializer(typeof (List<DVBCTuning>));
        xmlSerializer.Serialize(parFileXML, _dvbcChannels);
        parFileXML.Close();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Error saving tuningdetails");
        MessageBox.Show("Transponder list could not be saved, check error.log for details.");
      }
    }

    /// <summary>
    /// Load existing list from xml file 
    /// </summary>
    /// <param name="fileName">Path for input filen</param>
    private void LoadList(string fileName)
    {
      try
      {
        XmlReader parFileXML = XmlReader.Create(fileName);
        XmlSerializer xmlSerializer = new XmlSerializer(typeof (List<DVBCTuning>));
        _dvbcChannels = (List<DVBCTuning>)xmlSerializer.Deserialize(parFileXML);
        parFileXML.Close();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Error loading tuningdetails");
        MessageBox.Show("Transponder list could not be loaded, check error.log for details.");
      }
    }

    /// <summary>
    /// Reads previous settings and assign them to controls
    /// </summary>
    private void SetDefaults()
    {
      
      int index = Math.Max(Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbc" + _cardNumber + "Country", "0").Value), 0);
      // limit to >= 0
      if (index < mpComboBoxCountry.Items.Count)
      {
        mpComboBoxCountry.SelectedIndex = index;
      }

      index = Math.Max(Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbc" + _cardNumber + "Region", "0").Value), 0); // limit to >= 0
      if (index < mpComboBoxRegion.Items.Count)
      {
        mpComboBoxRegion.SelectedIndex = index;
      }

      textBoxFreq.Text = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbc" + _cardNumber + "Freq", "306000").Value;
      textBoxSymbolRate.Text = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbc" + _cardNumber + "Symbolrate", "6900").Value;

      index = Math.Max(Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbc" + _cardNumber + "Modulation", "3").Value), 0);
      // limit to >= 0
      if (index < mpComboBoxMod.Items.Count)
      {
        mpComboBoxMod.SelectedIndex = index;
      }

      checkBoxCreateGroups.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbc" + _cardNumber + "creategroups", "false").Value == "true");
      checkBoxCreateSignalGroup.Checked =
        (ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbc" + _cardNumber + "createsignalgroup", "false").Value == "true");
    }

    /// <summary>
    /// Saves control status
    /// </summary>
    private void PersistState()
    {
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbc" + _cardNumber + "Country", mpComboBoxCountry.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbc" + _cardNumber + "Region", mpComboBoxRegion.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbc" + _cardNumber + "Freq", textBoxFreq.Text);
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbc" + _cardNumber + "Symbolrate", textBoxSymbolRate.Text);
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbc" + _cardNumber + "Modulation", mpComboBoxMod.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbc" + _cardNumber + "creategroups", checkBoxCreateGroups.Checked ? "true" : "false");
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbc" + _cardNumber + "createsignalgroup", checkBoxCreateSignalGroup.Checked ? "true" : "false");      
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
          
          Card card = ServiceAgents.Instance.CardServiceAgent.GetCardByDevicePath(ServiceAgents.Instance.ControllerServiceAgent.CardDevice(_cardNumber));
          if (card.Enabled == false)
          {
            MessageBox.Show(this, "Tuner is disabled. Please enable the tuner before scanning.");
            return;
          }
          if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(card.IdCard))
          {
            MessageBox.Show(this, "Tuner is not found. Please make sure the tuner is present before scanning.");
            return;
          }
          // Check if the card is locked for scanning.
          IUser user;
          if (ServiceAgents.Instance.ControllerServiceAgent.IsCardInUse(_cardNumber, out user))
          {
            MessageBox.Show(this,
                            "Tuner is locked. Scanning is not possible at the moment. Perhaps you are using another part of a hybrid card?");
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
              _dvbcChannels = (List<DVBCTuning>)fileFilters.LoadList(tuningFile.FileName, typeof (List<DVBCTuning>));
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
              string line = String.Format("Scan freq:{0} {1} symbolrate:{2} ...", tuneChannel.Frequency,
                                          tuneChannel.ModulationType, tuneChannel.SymbolRate);
              ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
              item.EnsureVisible();

              IChannel[] channels = ServiceAgents.Instance.ControllerServiceAgent.ScanNIT(_cardNumber, tuneChannel);
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

              ListViewItem lastItem =
                listViewStatus.Items.Add(
                  new ListViewItem(String.Format("Scan done, found {0} transponders...", _dvbcChannels.Count)));
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
          else
          {
            scanState = ScanState.Done;
            SetButtonState();
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
    private void UpdateStatus()
    {
      progressBarLevel.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalQuality(_cardNumber));
    }

    #region Scan Thread

    /// <summary>
    /// Scan Thread
    /// </summary>
    private void DoScan()
    {
      suminfo tv = new suminfo();
      suminfo radio = new suminfo();
      IUser user = new User();
      user.CardId = _cardNumber;
      try
      {
        scanState = ScanState.Scanning;
        if (_dvbcChannels.Count == 0)
          return;

        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = false;

        SetButtonState();
        
        Card card = ServiceAgents.Instance.CardServiceAgent.GetCardByDevicePath(ServiceAgents.Instance.ControllerServiceAgent.CardDevice(_cardNumber));

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

          UpdateStatus();
          if (index == 0)
          {
            ServiceAgents.Instance.ControllerServiceAgent.Scan(user.Name, user.CardId, out user, tuneChannel, -1);
            UpdateStatus();
          }

          IChannel[] channels = ServiceAgents.Instance.ControllerServiceAgent.Scan(_cardNumber, tuneChannel);
          UpdateStatus();

          if (channels == null || channels.Length == 0)
          {
            if (ServiceAgents.Instance.ControllerServiceAgent.TunerLocked(_cardNumber) == false)
            {
              line = String.Format("{0}tp- {1} {2} {3}:No signal", 1 + index, tuneChannel.Frequency,
                                   tuneChannel.ModulationType, tuneChannel.SymbolRate);
              item.Text = line;
              item.ForeColor = Color.Red;
              continue;
            }
            line = String.Format("{0}tp- {1} {2} {3}:Nothing found", 1 + index, tuneChannel.Frequency,
                                 tuneChannel.ModulationType, tuneChannel.SymbolRate);
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
            bool exists;
            TuningDetail currentDetail;
            //Check if we already have this tuningdetail. The user has the option to enable channel move detection...
            if (checkBoxEnableChannelMoveDetection.Checked)
            {
              //According to the DVB specs ONID + SID is unique, therefore we do not need to use the TSID to identify a service.
              //The DVB spec recommends that the SID should not change if a service moves. This theoretically allows us to
              //track channel movements.              
              TuningDetailSearchEnum tuningDetailSearchEnum = TuningDetailSearchEnum.NetworkId;
              tuningDetailSearchEnum |= TuningDetailSearchEnum.ServiceId;
              currentDetail = ServiceAgents.Instance.ChannelServiceAgent.GetTuningDetailCustom(channel, tuningDetailSearchEnum);
            }
            else
            {
              //There are certain providers that do not maintain unique ONID + SID combinations.
              //In those cases, ONID + TSID + SID is generally unique. The consequence of using the TSID to identify
              //a service is that channel movement tracking won't work (each transponder/mux should have its own TSID).
              currentDetail = ServiceAgents.Instance.ChannelServiceAgent.GetTuningDetail(channel);                                          
            }

            if (currentDetail == null)
            {
              //add new channel
              exists = false;
              dbChannel = ChannelFactory.CreateChannel(channel.Name);
              dbChannel.SortOrder = 10000;
              if (channel.LogicalChannelNumber >= 1)
              {
                dbChannel.SortOrder = channel.LogicalChannelNumber;
              }
              dbChannel.MediaType = (int) channel.MediaType;
              dbChannel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(dbChannel);
              dbChannel.AcceptChanges();
            }
            else
            {
              exists = true;
              dbChannel = currentDetail.Channel;
            }

            if (dbChannel.MediaType == (int)MediaTypeEnum.TV)
            {
              ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.TvGroupNames.AllChannels, MediaTypeEnum.TV);
              MappingHelper.AddChannelToGroup(ref dbChannel, @group);                                          
              if (checkBoxCreateSignalGroup.Checked)
              {
                group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.TvGroupNames.DVBC, MediaTypeEnum.TV);
                MappingHelper.AddChannelToGroup(ref dbChannel, @group);                                                          
              }
              if (checkBoxCreateGroups.Checked)
              {
                group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(channel.Provider, MediaTypeEnum.TV);
                MappingHelper.AddChannelToGroup(ref dbChannel, @group);                                                                          
              }
            }
            else if (dbChannel.MediaType == (int)MediaTypeEnum.Radio)
            {
              ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.RadioGroupNames.AllChannels, MediaTypeEnum.Radio);
              MappingHelper.AddChannelToGroup(ref dbChannel, @group);
              if (checkBoxCreateSignalGroup.Checked)
              {
                group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.RadioGroupNames.DVBC, MediaTypeEnum.Radio);
                MappingHelper.AddChannelToGroup(ref dbChannel, @group);
              }
              if (checkBoxCreateGroups.Checked)
              {
                group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(channel.Provider, MediaTypeEnum.Radio);
                MappingHelper.AddChannelToGroup(ref dbChannel, @group);
              }
            }

            if (currentDetail == null)
            {
              ServiceAgents.Instance.ChannelServiceAgent.AddTuningDetail(dbChannel.IdChannel, channel);
            }
            else
            {
              //update tuning details...
              ServiceAgents.Instance.ChannelServiceAgent.UpdateTuningDetail(dbChannel.IdChannel, currentDetail.IdTuning, channel);
            }

            if (channel.MediaType == MediaTypeEnum.TV)
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
            if (channel.MediaType == MediaTypeEnum.Radio)
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
            MappingHelper.AddChannelToCard(dbChannel, card, false);
            line = String.Format("{0}tp- {1} {2} {3}:New TV/Radio:{4}/{5} Updated TV/Radio:{6}/{7}", 1 + index,
                                 tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate,
                                 tv.newChannel, radio.newChannel, tv.updChannel, radio.updChannel);
            item.Text = line;
          }
          tv.updChannelSum += tv.updChannel;
          radio.updChannelSum += radio.updChannel;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
      finally
      {
        ServiceAgents.Instance.ControllerServiceAgent.StopCard(user.CardId);
        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = true;
        progressBar1.Value = 100;

        scanState = ScanState.Done;
        SetButtonState();
      }
      listViewStatus.Items.Add(
        new ListViewItem(String.Format("Total radio channels updated:{0}, new:{1}", radio.updChannelSum,
                                       radio.newChannelSum)));
      foreach (IChannel newChannel in radio.newChannels)
      {
        listViewStatus.Items.Add(new ListViewItem(String.Format("  -> new channel: {0}", newChannel.Name)));
      }

      listViewStatus.Items.Add(
        new ListViewItem(String.Format("Total tv channels updated:{0}, new:{1}", tv.updChannelSum, tv.newChannelSum)));
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
        checkBoxCreateGroups.Enabled = false;
        checkBoxCreateSignalGroup.Enabled = false;
        checkBoxEnableChannelMoveDetection.Enabled = false;
        checkBoxAdvancedTuning.Enabled = false;
      }
      else
      {
        mpGrpAdvancedTuning.Visible = checkBoxAdvancedTuning.Checked && !_isScanning;
        mpGrpScanProgress.Visible = _isScanning;
        checkBoxCreateGroups.Enabled = !_isScanning;
        checkBoxCreateSignalGroup.Enabled = !_isScanning;
        checkBoxEnableChannelMoveDetection.Enabled = !_isScanning;
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