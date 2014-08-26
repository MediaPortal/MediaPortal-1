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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.SetupTV.Sections.CIMenu;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class CardDvbS : SectionSettings
  {  

    #region variables

    private readonly int _cardNumber;
    private List<Transponder> _transponders = new List<Transponder>();
    

    private int _tvChannelsNew;
    private int _radioChannelsNew;
    private int _tvChannelsUpdated;
    private int _radioChannelsUpdated;
    private bool _enableEvents;
    private bool _ignoreCheckBoxCreateGroupsClickEvent;
    private IUser _user;

    private CI_Menu_Dialog ciMenuDialog; // ci menu dialog object

    private ScanState scanState; // scan state

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

    #region ctors

    public CardDvbS()
      : this("DVBS") {}

    public CardDvbS(string name)
      : base(name) {}

    public CardDvbS(string name, int cardNumber)
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

    #region helper methods

    

    

    #endregion

    #region DVB-S scanning tab

    private void Init()
    {
      // set to same positions as progress
      mpGrpAdvancedTuning.Top = mpGrpScanProgress.Top;

      _enableEvents = false;

      int idx = 0;

      mpComboBoxPolarisation.Items.AddRange(new object[]
                                              {
                                                "Not Set",
                                                "Not Defined",
                                                "Horizontal",
                                                "Vertical",
                                                "Circular Left",
                                                "Circular Right"
                                              });
      mpComboBoxPolarisation.SelectedIndex = 2;

      mpComboBoxMod.Items.AddRange(new object[]
                                     {
                                       "Not Set",
                                       "Not Defined",
                                       "16 QAM",
                                       "32 QAM",
                                       "64 QAM",
                                       "80 QAM",
                                       "96 QAM",
                                       "112 QAM",
                                       "128 QAM",
                                       "160 QAM",
                                       "192 QAM",
                                       "224 QAM",
                                       "256 QAM",
                                       "320 QAM",
                                       "384 QAM",
                                       "448 QAM",
                                       "512 QAM",
                                       "640 QAM",
                                       "768 QAM",
                                       "896 QAM",
                                       "1024 QAM",
                                       "QPSK",
                                       "BPSK",
                                       "OQPSK",
                                       "8 VSB",
                                       "16 VSB",
                                       "Analog Amplitude",
                                       "Analog Frequency",
                                       "8 PSK",
                                       "RF",
                                       "16 APSK",
                                       "32 APSK",
                                       "QPSK (DVB-S2)",
                                       "8 PSK (DVB-S2)",
                                       "DirectTV"
                                     });
      mpComboBoxMod.SelectedIndex = 0;

      mpComboBoxInnerFecRate.Items.AddRange(new object[]
                                              {
                                                "Not Set",
                                                "Not Defined",
                                                "1/2",
                                                "2/3",
                                                "3/4",
                                                "3/5",
                                                "4/5",
                                                "5/6",
                                                "5/11",
                                                "7/8",
                                                "1/4",
                                                "1/3",
                                                "2/5",
                                                "6/7",
                                                "8/9",
                                                "9/10"
                                              });
      mpComboBoxInnerFecRate.SelectedIndex = 0;

      mpComboBoxPilot.Items.AddRange(new object[]
                                       {
                                         "Not Set",
                                         "Not Defined",
                                         "Off",
                                         "On"
                                       });
      mpComboBoxPilot.SelectedIndex = 0;

      mpComboBoxRollOff.Items.AddRange(new object[]
                                         {
                                           "Not Set",
                                           "Not Defined",
                                           ".20 Roll Off",
                                           ".25 Roll Off",
                                           ".35 Roll Off"
                                         });
      mpComboBoxRollOff.SelectedIndex = 0;
      

      checkBoxCreateGroups.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _cardNumber + "creategroups", false));
      checkBoxCreateGroupsSat.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _cardNumber + "creategroupssat", false));
      checkBoxCreateSignalGroup.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _cardNumber + "createsignalgroup", false));
      checkBoxEnableDVBS2.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _cardNumber + "enabledvbs2", false));

      _enableEvents = true;      

      checkBoxAdvancedTuning.Checked = false;
      checkBoxAdvancedTuning.Enabled = true;

      checkBoxCreateSignalGroup.Text = "\"" + TvConstants.TvGroupNames.DVBS + "\"";

      scanState = ScanState.Initialized;
      SetControlStates();
    }

    public override void OnSectionDeActivated()
    {
      timer1.Enabled = false;
      SaveSettings();
      base.OnSectionDeActivated();
      if (ciMenuDialog != null)
      {
        ciMenuDialog.OnSectionDeActivated();
      }
    }

    public override void SaveSettings()
    {
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _cardNumber + "creategroups", checkBoxCreateGroups.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _cardNumber + "creategroupssat", checkBoxCreateGroupsSat.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _cardNumber + "createsignalgroup", checkBoxCreateSignalGroup.Checked);

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _cardNumber + "enabledvbs2", checkBoxEnableDVBS2.Checked);
    }

    private void UpdateStatus()
    {
      progressBarLevel.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalQuality(_cardNumber));
      progressBarSatLevel.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalLevel(_cardNumber));
      progressBarSatQuality.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalQuality(_cardNumber));
      labelTunerLock.Text = ServiceAgents.Instance.ControllerServiceAgent.TunerLocked(_cardNumber) ? "Yes" : "No";
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();

      FillTunerSatellites();


      UpdateStatus();
      labelCurrentPosition.Text = "";
      tabControl1_SelectedIndexChanged(null, null);
      _user = new User();

      if (ciMenuDialog != null)
      {
        ciMenuDialog.OnSectionActivated();
      }
    }

    private void FillTunerSatellites()
    {
      mpListViewTunerSatellites.BeginUpdate();
      try
      {
        mpListViewTunerSatellites.Items.Clear();
        
        foreach (TunerSatellite tunerSatellite in Card.TunerSatellites)
        {
          ListViewItem item = mpListViewTunerSatellites.Items.Add(tunerSatellite.Satellite.Name);
          item.SubItems.Add(tunerSatellite.LnbType.Name);
          item.Tag = tunerSatellite;
        }        

        for (int i = 0; i < mpListViewTunerSatellites.Items.Count; ++i)
        {
          mpListViewTunerSatellites.Items[i].Checked = false;
        }
      }
      finally
      {
        mpListViewTunerSatellites.EndUpdate();
      }
    }

    private Card Card
    {
      get { return ServiceAgents.Instance.CardServiceAgent.GetCardByDevicePath(ServiceAgents.Instance.ControllerServiceAgent.CardDevice(_cardNumber)); }
    }

    #region Scan handling

    private void InitScanProcess()
    {
      // once completed reset to new beginning
      switch (scanState)
      {
        case ScanState.Done:
          scanState = ScanState.Initialized;
          listViewStatus.Items.Clear();
          SetControlStates();
          return;

        case ScanState.Initialized:
         SaveSettings();
                    
          if (Card.Enabled == false)
          {
            MessageBox.Show(this, "Tuner is disabled. Please enable the tuner before scanning.");
            return;
          }
          if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(Card.IdCard))
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

          StartScanThread();
          break;

        case ScanState.Scanning:
          scanState = ScanState.Cancel;
          SetControlStates();
          break;

        case ScanState.Cancel:
          return;
      }
    }

    #endregion

    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      InitScanProcess();
    }

    private void DoScan()
    {
      MethodInvoker updateControls = new MethodInvoker(SetControlStates);
      try
      {
        scanState = ScanState.Scanning;
        Invoke(updateControls);
        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = false;

        listViewStatus.Items.Clear();
        _tvChannelsNew = 0;
        _radioChannelsNew = 0;
        _tvChannelsUpdated = 0;
        _radioChannelsUpdated = 0;

        for (int i = 0; i < mpListViewTunerSatellites.Items.Count; ++i)
        {
          if (mpListViewTunerSatellites.Items[i].Checked)
          {
            TunerSatellite tunerSatellite = (TunerSatellite)mpListViewTunerSatellites.Items[i].Tag;
            Scan(tunerSatellite);

            if (scanState == ScanState.Cancel)
              return;
          }
        }                

        listViewStatus.Items.Add(
          new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", _radioChannelsNew,
                                         _radioChannelsUpdated)));
        listViewStatus.Items.Add(
          new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", _tvChannelsNew, _tvChannelsUpdated)));
        ListViewItem item = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
        item.EnsureVisible();
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
      finally
      {
        IUser user = new User();
        user.CardId = _cardNumber;
        ServiceAgents.Instance.ControllerServiceAgent.StopCard(user.CardId);
        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = true;
        progressBar1.Value = 100;
        scanState = ScanState.Done;
        Invoke(updateControls);
      }
    }

    private void Scan(TunerSatellite tunerSatellite) //int lnb, LnbType lnbType, DiseqcPort diseqc, SatelliteContext context
    {
      // all transponders to scan
      List<DVBSChannel> _channels = new List<DVBSChannel>();

      // get default sat position from DB
      int position = -1;
      bool setting = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _cardNumber + "motorEnabled", false);
      if (setting)
      {
        position = tunerSatellite.DiseqcMotorPosition.GetValueOrDefault();        
      }

      // what to scan
      Satellite context = tunerSatellite.Satellite;      

      switch (ActiveScanType)
      {
        case ScanTypes.Predefined:
          LoadTransponders(context);

          foreach (Transponder t in _transponders)
          {
            DVBSChannel curChannel = t.toDVBSChannel;
            _channels.Add(curChannel);
          }
          break;

          // scan Network Information Table for transponder info
        case ScanTypes.NIT:
          _transponders.Clear();
          DVBSChannel tuneChannel = GetManualTuning();
          
          

          tuneChannel.Diseqc = (DiseqcPort)tunerSatellite.DiseqcSwitchSetting;
          tuneChannel.LnbType = tunerSatellite.LnbType;
          tuneChannel.SatelliteIndex = position;

          listViewStatus.Items.Clear();
          string line = String.Format("lnb:{0} {1}tp- {2} {3} {4}", tunerSatellite.LnbType, 1, tuneChannel.Frequency,
                                      tuneChannel.Polarisation, tuneChannel.SymbolRate);
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();

          IChannel[] channels = ServiceAgents.Instance.ControllerServiceAgent.ScanNIT(_cardNumber, tuneChannel);
          if (channels != null)
          {
            for (int i = 0; i < channels.Length; ++i)
            {
              DVBSChannel curChannel = (DVBSChannel)channels[i];
              _channels.Add(curChannel);
              item = listViewStatus.Items.Add(new ListViewItem(curChannel.ToString()));
              item.EnsureVisible();
            }
          }

          ListViewItem lastItem =
            listViewStatus.Items.Add(
              new ListViewItem(String.Format("Scan done, found {0} transponders...", _channels.Count)));
          lastItem.EnsureVisible();
          break;

          // scan only single TP
        case ScanTypes.SingleTransponder:
          _channels.Add(GetManualTuning());
          break;
      }

      // no channels
      if (_channels.Count == 0)
        return;

      IUser user = new User();
      user.CardId = _cardNumber;
      int scanIndex = 0; // count of really scanned TPs (S2 skipped)
      for (int index = 0; index < _channels.Count; ++index)
      {
        if (scanState == ScanState.Cancel)
          return;

        DVBSChannel tuneChannel = _channels[index];
        float percent = ((float)(index)) / _channels.Count;
        percent *= 100f;
        if (percent > 100f)
          percent = 100f;
        progressBar1.Value = (int)percent;

        // If this is a DVB-S2 transponder and DVB-S2
        // scanning is not enabled then skip it. Note that
        // a roll-off of .35 is the default for standard
        // DVB-S.
        if ((tuneChannel.RollOff == RollOff.Twenty ||
             tuneChannel.RollOff == RollOff.TwentyFive ||
             tuneChannel.Pilot == Pilot.On) &&
            !checkBoxEnableDVBS2.Checked)
        {
          continue;
        }

        scanIndex++;

        tuneChannel.Diseqc = (DiseqcPort)tunerSatellite.DiseqcSwitchSetting;
        tuneChannel.LnbType = tunerSatellite.LnbType;
        tuneChannel.SatelliteIndex = position;
        string line = String.Format("lnb:{0} {1}tp- {2} {3} {4}", tunerSatellite.LnbType, 1 + index, tuneChannel.Frequency,
                                    tuneChannel.Polarisation, tuneChannel.SymbolRate);
        ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
        item.EnsureVisible();

        if (scanIndex == 1) // first scanned
        {
          ServiceAgents.Instance.ControllerServiceAgent.Scan(user.Name, user.CardId, out user, tuneChannel, -1);
        }
        UpdateStatus();

        IChannel[] channels = ServiceAgents.Instance.ControllerServiceAgent.Scan(_cardNumber, tuneChannel);

        UpdateStatus();

        if (channels == null || channels.Length == 0)
        {
          if (ServiceAgents.Instance.ControllerServiceAgent.TunerLocked(_cardNumber) == false)
          {
            line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:No signal", tunerSatellite.LnbType, scanIndex, tuneChannel.Frequency,
                                 tuneChannel.Polarisation, tuneChannel.SymbolRate);
            item.Text = line;
            item.ForeColor = Color.Red;
            continue;
          }
          line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:Nothing found", tunerSatellite.LnbType, scanIndex, tuneChannel.Frequency,
                               tuneChannel.Polarisation, tuneChannel.SymbolRate);
          item.Text = line;
          item.ForeColor = Color.Red;
          continue;
        }

        int newChannels = 0;
        int updatedChannels = 0;
        for (int i = 0; i < channels.Length; ++i)
        {
          Channel dbChannel;
          DVBSChannel channel = (DVBSChannel)channels[i];
          bool exists;
          TuningDetailDvbS2 currentDetail;
          ServiceDetail currentServiceDetail;
          //Check if we already have this tuningdetail. The user has the option to enable channel move detection...
          if (checkBoxEnableChannelMoveDetection.Checked)
          {
            //According to the DVB specs ONID + SID is unique, therefore we do not need to use the TSID to identify a service.
            //The DVB spec recommends that the SID should not change if a service moves. This theoretically allows us to
            //track channel movements.
            TuningDetailSearchEnum tuningDetailSearchEnum = TuningDetailSearchEnum.NetworkId;
            tuningDetailSearchEnum |= TuningDetailSearchEnum.ServiceId;
            currentServiceDetail = ServiceAgents.Instance.ChannelServiceAgent.GetServiceDetailCustom(channel, tuningDetailSearchEnum);
            currentDetail = (TuningDetailDvbS2)currentServiceDetail.TuningDetail;                             
          }
          else
          {
            //There are certain providers that do not maintain unique ONID + SID combinations.
            //In those cases, ONID + TSID + SID is generally unique. The consequence of using the TSID to identify
            //a service is that channel movement tracking won't work (each transponder/mux should have its own TSID).            
            currentServiceDetail = ServiceAgents.Instance.ChannelServiceAgent.GetServiceDetail(channel);
            currentDetail = (TuningDetailDvbS2)currentServiceDetail.TuningDetail;
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
            dbChannel.MediaType = (int)channel.MediaType;
            dbChannel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(dbChannel);
            dbChannel.AcceptChanges();
          }
          else
          {
            exists = true;
            dbChannel = currentServiceDetail.Channel;
          }

          if (dbChannel.MediaType == (int)MediaTypeEnum.TV)
          {
            ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.TvGroupNames.AllChannels, MediaTypeEnum.TV);
            MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            if (checkBoxCreateSignalGroup.Checked)
            {
              group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.TvGroupNames.DVBS, MediaTypeEnum.TV);
              MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            }
            if (checkBoxCreateGroupsSat.Checked)
            {
              group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(tunerSatellite.Satellite.Name, MediaTypeEnum.TV);
              MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            }
            if (checkBoxCreateGroups.Checked)
            {
              group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(channel.Provider, MediaTypeEnum.TV);
              MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            }
          }
          if (dbChannel.MediaType == (int)MediaTypeEnum.Radio)
          {
            ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.RadioGroupNames.AllChannels, MediaTypeEnum.Radio);
            MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            if (checkBoxCreateSignalGroup.Checked)
            {
              group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.RadioGroupNames.DVBS, MediaTypeEnum.Radio);
              MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            }
            if (checkBoxCreateGroupsSat.Checked)
            {
              group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(tunerSatellite.Satellite.Name, MediaTypeEnum.Radio);
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
            channel.SatelliteIndex = position; // context.Satellite.IdSatellite;
            ServiceAgents.Instance.ChannelServiceAgent.AddTuningDetail(dbChannel.IdChannel, channel, _cardNumber);
          }
          else
          {
            //update tuning details...

            

            channel.SatelliteIndex = position; // context.Satellite.IdSatellite;
            //tunerSatellite.DiseqcMotorPosition = position;            
            ServiceAgents.Instance.ChannelServiceAgent.UpdateTuningDetail(dbChannel.IdChannel, currentDetail.IdTuningDetail, channel, _cardNumber);
          }
          if (channel.MediaType == MediaTypeEnum.TV)
          {
            if (exists)
            {
              _tvChannelsUpdated++;
              updatedChannels++;
            }
            else
            {
              _tvChannelsNew++;
              newChannels++;
            }
          }
          if (channel.MediaType == MediaTypeEnum.Radio)
          {
            if (exists)
            {
              _radioChannelsUpdated++;
              updatedChannels++;
            }
            else
            {
              _radioChannelsNew++;
              newChannels++;
            }
          }
          MappingHelper.AddChannelToCard(dbChannel, Card, false);
          line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:New:{5} Updated:{6}",
                               tunerSatellite.LnbType, 1 + index, tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate,
                               newChannels, updatedChannels);
          item.Text = line;
        }
      }
    }

    private void CardDvbS_Load(object sender, EventArgs e) {}

    #endregion

    #region DiSEqC Motor tab

    private void SetupMotor()
    {
      _enableEvents = false;

      bool enabled = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _cardNumber + "motorEnabled", false);
      checkBox1.Checked = enabled;
      checkBox1_CheckedChanged(null, null);

      comboBoxStepSize.Items.Clear();
      for (int i = 1; i < 127; ++i)
        comboBoxStepSize.Items.Add(i.ToString());

      int stepsize = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _cardNumber + "motorStepSize", 10);
      comboBoxStepSize.SelectedIndex = stepsize - 1;

      comboBoxSat.Items.Clear();

      int index = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _cardNumber + "selectedMotorSat", 0);

      //todo MM diseq motor

      /*List<Satellite> satellites = LoadSatellites();

      foreach (Satellite sat in satellites)
      {
        comboBoxSat.Items.Add(sat);
      }
      if (index >= 0 && index < satellites.Count)
        comboBoxSat.SelectedIndex = index;
      else
        comboBoxSat.SelectedIndex = 0;
      LoadMotorTransponder();*/
      _enableEvents = true;
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (tabControl1.SelectedIndex == 1)
      {
        SetupMotor();
        timer1.Enabled = true;
      }
      else
      {
        timer1.Enabled = false;
      }
    }

    private void buttonMoveWest_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //move motor west
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCDriveMotor(_cardNumber, DiseqcDirection.West,
                                              (byte)(1 + comboBoxStepSize.SelectedIndex));
      comboBox1_SelectedIndexChanged(null, null); //tune..;
    }

    private void buttonSetWestLimit_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //set motor west limit
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCSetWestLimit(_cardNumber);
    }

    private void tabPage2_Click(object sender, EventArgs e) {}

    private void btnDiseqCGoto_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      //goto selected sat
      if (comboBoxSat.SelectedIndex < 0)
        return;
      if (checkBox1.Checked == false)
        return;
      Satellite sat = (Satellite)comboBoxSat.Items[comboBoxSat.SelectedIndex];

      Card card = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardNumber);

      //todo MM diseq motor
      /*IList<DisEqcMotor> motorSettings = card.DisEqcMotors;
      foreach (DisEqcMotor motor in motorSettings)
      {
        if (motor.IdSatellite == sat.Satellite.IdSatellite)
        {
          ServiceAgents.Instance.ControllerServiceAgent.DiSEqCGotoPosition(_cardNumber, (byte)motor.Position);
          MessageBox.Show("Satellite moving to position:" + motor.Position, "Info", MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
          comboBox1_SelectedIndexChanged(null, null);
          return;
        }
      }
      MessageBox.Show("No position stored for this satellite", "Warning", MessageBoxButtons.OK,
                      MessageBoxIcon.Exclamation);*/
    }

    private void buttonStore_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;

      /*
      //todo MM how does this work

      //store motor position..
      int index = -1;
      Satellite sat = (Satellite)comboBoxSat.SelectedItem;
      Card card = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardNumber);

      foreach (TunerSatellite tunersat in sat.TunerSatellites)
      {
        tunersat.DiseqcMotorPosition = 
      }
      

      IList<DisEqcMotor> motorSettings = card.DisEqcMotors;
      foreach (DisEqcMotor motor in motorSettings)
      {
        if (motor.IdSatellite == sat.Satellite.IdSatellite)
        {
          index = motor.Position;
          break;
        }
      }
      if (index < 0)
      {
        index = motorSettings.Count + 1;
        DisEqcMotor motor = new DisEqcMotor();
        motor.IdCard = card.IdCard;
        motor.IdSatellite = sat.Satellite.IdSatellite;
        motor.Position = index;
        ServiceAgents.Instance.CardServiceAgent.SaveTunerSatellite(motor);

      }
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCStorePosition(_cardNumber, (byte)(index));
      MessageBox.Show("Satellite position stored to:" + index, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);*/
    }

    private void buttonMoveEast_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //move motor east
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCDriveMotor(_cardNumber, DiseqcDirection.East,
                                              (byte)(1 + comboBoxStepSize.SelectedIndex));
      comboBox1_SelectedIndexChanged(null, null); //tune..
    }

    private void buttonSetEastLimit_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //set motor east limit
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCSetEastLimit(_cardNumber);
    }

    private void comboBoxSat_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _cardNumber + "selectedMotorSat", comboBoxSat.SelectedIndex);
      LoadMotorTransponder();
      comboBox1_SelectedIndexChanged(null, null);
    }


    private void checkBoxEnabled_CheckedChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      if (checkBoxEnabled.Checked)
      {
        ServiceAgents.Instance.ControllerServiceAgent.DiSEqCForceLimit(_cardNumber, true);
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _cardNumber + "limitsEnabled", true);
      }
      else
      {
        if (
          MessageBox.Show("Disabling the east/west limits could damage your dish!!! Are you sure?", "Warning",
                          MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
        {
          ServiceAgents.Instance.ControllerServiceAgent.DiSEqCForceLimit(_cardNumber, false);
          ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _cardNumber + "limitsEnabled", false);
        }
        else
        {
          _enableEvents = false;
          checkBoxEnabled.Checked = true;
          ServiceAgents.Instance.ControllerServiceAgent.DiSEqCForceLimit(_cardNumber, true);
          ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _cardNumber + "limitsEnabled", true);
          _enableEvents = true;
        }
      }
    }
    private void LoadMotorTransponder()
    {
      checkBoxEnabled.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("dvbs" + _cardNumber + "limitsEnabled", true);

      comboBox1.Items.Clear();
      Satellite sat = (Satellite)comboBoxSat.SelectedItem;
      LoadTransponders(sat);
      _transponders.Sort();
      foreach (Transponder transponder in _transponders)
      {
        comboBox1.Items.Add(transponder);
      }
      if (comboBox1.Items.Count > 0)
        comboBox1.SelectedIndex = 0;
      bool eventsEnabled = _enableEvents;
      _enableEvents = true;
      comboBox1_SelectedIndexChanged(null, null);
      _enableEvents = eventsEnabled;
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      Transponder transponder = (Transponder)comboBox1.SelectedItem;
      DVBSChannel tuneChannel = new DVBSChannel();
      tuneChannel.Frequency = transponder.CarrierFrequency;
      tuneChannel.Polarisation = transponder.Polarisation;
      tuneChannel.SymbolRate = transponder.SymbolRate;
      tuneChannel.ModulationType = transponder.Modulation;
      tuneChannel.Pilot = transponder.Pilot;
      tuneChannel.RollOff = transponder.Rolloff;
      tuneChannel.InnerFecRate = transponder.InnerFecRate;
      tuneChannel.Diseqc = DiseqcPort.None;
      /*if (mpComboLnbType1.SelectedIndex >= 0)
        tuneChannel.LnbType = (LnbType)mpComboLnbType1.SelectedItem;
      if (mpComboDiseqc1.SelectedIndex >= 0)
        tuneChannel.Diseqc = (DiseqcPort)mpComboDiseqc1.SelectedIndex;*/
      _user.CardId = _cardNumber;
      ServiceAgents.Instance.ControllerServiceAgent.StopCard(_user.CardId);
      _user.CardId = _cardNumber;
      ServiceAgents.Instance.ControllerServiceAgent.Tune(_user.Name, _user.CardId, out _user, tuneChannel, -1);
      progressBarLevel.Value = 1;
      progressBarQuality.Value = 1;
      progressBarSatLevel.Value = 1;
      progressBarSatQuality.Value = 1;
      labelTunerLock.Text = String.Empty;
    }

    private void buttonStop_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCStopMotor(_cardNumber);
      comboBox1_SelectedIndexChanged(null, null);
    }

    private void buttonGotoStart_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCGotoReferencePosition(_cardNumber);
      comboBox1_SelectedIndexChanged(null, null);
    }

    private void buttonUp_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //move motor up
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCDriveMotor(_cardNumber, DiseqcDirection.Up,
                                              (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //move motor up
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCDriveMotor(_cardNumber, DiseqcDirection.Down,
                                              (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void comboBoxStepSize_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _cardNumber + "motorStepSize", (1 + comboBoxStepSize.SelectedIndex));
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      comboBoxSat.Enabled = checkBox1.Checked;
      comboBox1.Enabled = checkBox1.Checked;
      buttonGoto.Enabled = checkBox1.Checked;
      comboBoxStepSize.Enabled = checkBox1.Checked;
      buttonUp.Enabled = checkBox1.Checked;
      buttonDown.Enabled = checkBox1.Checked;
      buttonMoveWest.Enabled = checkBox1.Checked;
      buttonMoveEast.Enabled = checkBox1.Checked;
      buttonStop.Enabled = checkBox1.Checked;
      checkBoxEnabled.Enabled = checkBox1.Checked;
      buttonGotoStart.Enabled = checkBox1.Checked;
      buttonStore.Enabled = checkBox1.Checked;
      buttonSetWestLimit.Enabled = checkBox1.Checked;
      buttonSetEastLimit.Enabled = checkBox1.Checked;
      buttonReset.Enabled = checkBox1.Checked;

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("dvbs" + _cardNumber + "motorEnabled", checkBox1.Checked);
    }

    private bool reentrant;
    private DateTime _signalTimer = DateTime.MinValue;

    private void timer1_Tick(object sender, EventArgs e)
    {
      if (reentrant)
        return;
      try
      {
        reentrant = true;
        TimeSpan ts = DateTime.Now - _signalTimer;
        if (ts.TotalMilliseconds > 500)
        {
          if (checkBox1.Checked == false)
            return;

          ServiceAgents.Instance.ControllerServiceAgent.UpdateSignalSate(_cardNumber);
          _signalTimer = DateTime.Now;
          int satPos, stepsAzimuth, stepsElevation;
          ServiceAgents.Instance.ControllerServiceAgent.DiSEqCGetPosition(_cardNumber, out satPos, out stepsAzimuth, out stepsElevation);
          if (satPos < 0)
            labelCurrentPosition.Text = "unknown";
          else
          {
            string offset = "";
            string satPosition = String.Format("Satellite postion:#{0}", satPos);
            if (stepsAzimuth < 0)
              offset = String.Format("{0} steps west", -stepsAzimuth);
            else if (stepsAzimuth > 0)
              offset = String.Format("{0} steps east", stepsAzimuth);
            if (stepsElevation < 0)
            {
              offset = offset.Length != 0
                         ? String.Format("{0}, {1} steps up", offset, -stepsElevation)
                         : String.Format("{0} steps up", -stepsElevation);
            }
            else if (stepsElevation > 0)
            {
              offset = offset.Length != 0
                         ? String.Format("{0}, {1} steps down", offset, stepsElevation)
                         : String.Format("{0} steps down", stepsElevation);
            }
            labelCurrentPosition.Text = offset.Length > 0
                                          ? String.Format("{0} of {1}", offset, satPosition)
                                          : satPosition;
          }
        }
        UpdateStatus();
      }
      finally
      {
        reentrant = false;
      }
    }

    private void buttonReset_Click(object sender, EventArgs e)
    {
      if (checkBox1.Checked == false)
        return;
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCReset(_cardNumber);
    }

    #endregion
  
    #region LNB selection tab

    

    private void checkEnableDVBS2_CheckedChanged(object sender, EventArgs e)
    {
      mpComboBoxPilot.Enabled = false;
      mpComboBoxRollOff.Enabled = false;
      if (checkBoxEnableDVBS2.Enabled && checkBoxEnableDVBS2.Checked && ActiveScanType != ScanTypes.Predefined)
      {
        mpComboBoxPilot.Enabled = true;
        mpComboBoxRollOff.Enabled = true;
      }
    }

    #endregion

    private void checkBoxCreateGroupsSat_CheckedChanged(object sender, EventArgs e)
    {
      if (_ignoreCheckBoxCreateGroupsClickEvent)
        return;
      _ignoreCheckBoxCreateGroupsClickEvent = true;
      if (checkBoxCreateGroups.Checked)
      {
        checkBoxCreateGroups.Checked = false;
      }
      _ignoreCheckBoxCreateGroupsClickEvent = false;
    }

    private void checkBoxCreateGroups_CheckedChanged(object sender, EventArgs e)
    {
      if (_ignoreCheckBoxCreateGroupsClickEvent)
        return;
      _ignoreCheckBoxCreateGroupsClickEvent = true;
      if (checkBoxCreateGroupsSat.Checked)
      {
        checkBoxCreateGroupsSat.Checked = false;
      }
      _ignoreCheckBoxCreateGroupsClickEvent = false;
    }

    private void mpButtonManualScan_Click(object sender, EventArgs e) {}

    #region GUI handling

    /// <summary>
    /// Sets correct visibility and enabled states for UI controls.
    /// </summary>
    private void SetControlStates()
    {
      int gotoView = 1;
      switch (scanState)
      {
        default:
        case ScanState.Initialized:
          if (mpButtonScanTv.Text != "Scan for channels" || !mpButtonScanTv.Enabled)
          {
            mpButtonScanTv.Enabled = true;
          }
          mpButtonScanTv.Text = "Scan for channels";
          gotoView = 0;
          break;

        case ScanState.Scanning:
          mpButtonScanTv.Text = "Cancel...";
          break;

        case ScanState.Cancel:
          mpButtonScanTv.Text = "Cancelling...";
          break;

        case ScanState.Done:
          mpButtonScanTv.Text = "New scan";
          break;

        case ScanState.Updating:
          mpButtonScanTv.Text = "Scan for channels";
          mpButtonScanTv.Enabled = false;
          break;
      }

      bool enableScanControls = scanState == ScanState.Initialized;
      /*Control[] scanControls = new Control[]
                                 {
                                   mpLNB1, mpLNB2, mpLNB3, mpLNB4,
                                   mpComboDiseqc1, mpComboDiseqc2, mpComboDiseqc3, mpComboDiseqc4,
                                   mpComboLnbType1, mpComboLnbType2, mpComboLnbType3, mpComboLnbType4,
                                   mpTransponder1, mpTransponder2, mpTransponder3, mpTransponder4,
                                   checkBoxCreateGroupsSat, checkBoxCreateGroups, checkBoxCreateSignalGroup,
                                   checkBoxEnableDVBS2, checkBoxEnableChannelMoveDetection, checkBoxAdvancedTuning,
                                   buttonUpdate
                                 };
      for (int ctlIndex = 0; ctlIndex < scanControls.Length; ctlIndex++)
      {
        scanControls[ctlIndex].Enabled = enableScanControls;
      }*/

      bool enableNonPredef = scanState == ScanState.Initialized && ActiveScanType != ScanTypes.Predefined;
      Control[] nonPredefControls = new Control[]
                                      {
                                        textBoxFreq, textBoxSymbolRate,
                                        mpComboBoxPolarisation, mpComboBoxMod,
                                        mpComboBoxInnerFecRate
                                      };
      for (int ctlIndex = 0; ctlIndex < nonPredefControls.Length; ctlIndex++)
      {
        nonPredefControls[ctlIndex].Enabled = enableNonPredef;
      }

      // Only give access to the DVB-S2 fields when DVB-S2 scanning
      // is enabled - helps prevent users who don't know what the
      // fields are for from doing something they shouldn't.
      mpComboBoxPilot.Enabled = false;
      mpComboBoxRollOff.Enabled = false;
      if (checkBoxEnableDVBS2.Enabled && checkBoxEnableDVBS2.Checked && ActiveScanType != ScanTypes.Predefined)
      {
        mpComboBoxPilot.Enabled = true;
        mpComboBoxRollOff.Enabled = true;
      }

      SwitchToView(gotoView);
    }

    /// <summary>
    /// Show either scan parameters or scan progress.
    /// </summary>
    /// <param name="view">0 for parameters, 1 for progress</param>
    private void SwitchToView(int view)
    {
      if (view == 1)
      {
        mpGrpAdvancedTuning.Visible = false;
        mpGrpScanProgress.Visible = true;
      }
      else
      {
        mpGrpAdvancedTuning.Visible = checkBoxAdvancedTuning.Checked;
        mpGrpScanProgress.Visible = false;
      }

      if (mpGrpScanProgress.Visible)
      {
        mpGrpAdvancedTuning.SendToBack();
        mpGrpScanProgress.BringToFront();
      }
      else
      {
        mpGrpScanProgress.SendToBack();
        mpGrpAdvancedTuning.BringToFront();
      }
      UpdateZOrder();
      Application.DoEvents();
      Thread.Sleep(100);
    }

    private void UpdateGUIControls(object sender, EventArgs e)
    {
      SetControlStates();
    }

    #endregion

    private void StartScanThread()
    {
      Thread scanThread = new Thread(DoScan);
      scanThread.Name = "DVB-S scan thread";
      scanThread.Start();
    }

    private static Transponder ToTransonder(IChannel channel)
    {
      DVBSChannel ch = (DVBSChannel)channel;
      Transponder t = new Transponder();
      t.CarrierFrequency = Convert.ToInt32(ch.Frequency);
      t.InnerFecRate = ch.InnerFecRate;
      t.Modulation = ch.ModulationType;
      t.Pilot = ch.Pilot;
      t.Rolloff = ch.RollOff;
      t.SymbolRate = ch.SymbolRate;
      t.Polarisation = ch.Polarisation;
      return t;
    }

    private DVBSChannel GetManualTuning()
    {
      DVBSChannel tuneChannel = new DVBSChannel();
      tuneChannel.Frequency = Int32.Parse(textBoxFreq.Text);
      tuneChannel.SymbolRate = Int32.Parse(textBoxSymbolRate.Text);
      tuneChannel.Polarisation = (Polarisation)mpComboBoxPolarisation.SelectedIndex - 1;
      tuneChannel.ModulationType = (ModulationType)mpComboBoxMod.SelectedIndex - 1;
      tuneChannel.InnerFecRate = (BinaryConvolutionCodeRate)mpComboBoxInnerFecRate.SelectedIndex - 1;
      if (checkBoxEnableDVBS2.Checked)
      {
        tuneChannel.Pilot = (Pilot)mpComboBoxPilot.SelectedIndex - 1;
        tuneChannel.RollOff = (RollOff)mpComboBoxRollOff.SelectedIndex - 1;
      }
      else
      {
        tuneChannel.Pilot = Pilot.NotSet;
        tuneChannel.RollOff = RollOff.NotSet;
      }
      return tuneChannel;
    }

    private void mpButtonSaveList_Click(object sender, EventArgs e)
    {
      SaveManualScanList();
    }

    /// <summary>
    /// Loads new xml transponder list
    /// </summary>
    /// <param name="FileName"></param>
    private void LoadTransponders(Satellite context)
    {
      String fileName = context.LocalTransponderFile;
      if (!File.Exists(fileName))
      {
        //DownloadTransponder(context);
        //moved
      }

      // clear before refilling
      _transponders.Clear();
      try
      {
        XmlReader parFileXML = XmlReader.Create(fileName);
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Transponder>));
        _transponders = (List<Transponder>)xmlSerializer.Deserialize(parFileXML);
        parFileXML.Close();
        _transponders.Sort();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Error loading tuningdetails");
        MessageBox.Show("Transponder list could not be loaded, check error.log for details.");
      }
    }

    private String SaveManualScanList()
    {
      _transponders.Sort();
      String filePath = String.Format(@"{0}\TuningParameters\dvbs\Manual_Scans.{1}.xml", PathManager.GetDataPath,
                                      DateTime.Now.ToString("yyyy-MM-dd"));
      System.IO.TextWriter parFileXML = System.IO.File.CreateText(filePath);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (List<Transponder>));
      xmlSerializer.Serialize(parFileXML, _transponders);
      parFileXML.Close();
      Init();
      return Path.GetFileNameWithoutExtension(filePath);
    }

    private void mpButtonScanSingleTP_Click(object sender, EventArgs e)
    {
      DVBSChannel tuneChannel = GetManualTuning();
      Transponder t = ToTransonder(tuneChannel);
      _transponders.Add(t);
      StartScanThread();
    }

    private void checkBoxAdvancedTuning_CheckedChanged(object sender, EventArgs e)
    {
      mpGrpAdvancedTuning.Visible = checkBoxAdvancedTuning.Checked;
      SetControlStates();
    }

    private void mpCombo_MouseHover(object sender, EventArgs e)
    {
      toolTip1.SetToolTip((Control)sender, ((MPComboBox)sender).SelectedItem.ToString());
    }
  }
}