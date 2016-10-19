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
using System.Globalization;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditTunerSatellite : Form
  {
    private const string DISEQC_MOTOR_POSITION_TYPE_NONE = "None";
    private const string DISEQC_MOTOR_POSITION_TYPE_USALS = "USALS";
    private const string DISEQC_MOTOR_POSITION_TYPE_STORED = "Stored";

    private const int ALL_TUNERS_ID = -1;

    private int _idTunerSatellite = -1;
    private IList<Tuner> _satelliteTuners = null;
    private TunerSatellite _tunerSatellite = null;

    public FormEditTunerSatellite(int idTunerSatellite = -1)
    {
      InitializeComponent();
    }

    public TunerSatellite TunerSatellite
    {
      get
      {
        return _tunerSatellite;
      }
    }

    private void FormEditTunerSatellite_Load(object sender, EventArgs e)
    {
      // It is valid to define tuner satellite records:
      // 1. ...for {satellite + tuner} pairs.
      // 2. ...for satellites, to specify the default parameters for tuners
      //    that do not have specific records (ie. as per point 1).
      // It is valid to define more than one record of both types for a given
      // satellite. This enables people to describe a tuner that can receive
      // Ku, C and/or Ka band signals with different reception (LNB, DiSEqC
      // etc.) settings. If we were to limit to one record of each type, each
      // tuner would be limited to receiving one band per satellite. That is
      // not desirable.

      if (_idTunerSatellite > 0)
      {
        this.LogInfo("tuner satellite: start edit, ID = {0}", _idTunerSatellite);
        Text = "Edit Tuner Satellite";
      }
      else
      {
        this.LogInfo("tuner satellite: create new");
        Text = "Add Tuner Satellite";
      }

      IList<Tuner> tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerRelation.None);
      _satelliteTuners = new List<Tuner>(tuners.Count + 1);
      _satelliteTuners.Add(new Tuner { Name = "All (Default)", IdTuner = ALL_TUNERS_ID });
      List<int> allSatelliteTunerIds = new List<int>(tuners.Count + 1);
      allSatelliteTunerIds.Add(ALL_TUNERS_ID);
      foreach (Tuner tuner in tuners)
      {
        if (((int)BroadcastStandard.MaskSatellite & tuner.SupportedBroadcastStandards) != 0)
        {
          _satelliteTuners.Add(tuner);
          allSatelliteTunerIds.Add(tuner.IdTuner);
        }
      }

      Satellite defaultSatellite = null;
      int defaultSatelliteLongitude = Satellite.DefaultSatelliteLongitude.GetValueOrDefault(100000);
      comboBoxSatellite.BeginUpdate();
      try
      {
        comboBoxSatellite.Items.Clear();
        IList<Satellite> satellites = ServiceAgents.Instance.TunerServiceAgent.ListAllSatellites();
        foreach (Satellite satellite in satellites)
        {
          comboBoxSatellite.Items.Add(satellite);
          if (_tunerSatellite != null && _tunerSatellite.IdSatellite == satellite.IdSatellite)
          {
            comboBoxSatellite.SelectedItem = satellite;
          }
          else if (satellite.Longitude == defaultSatelliteLongitude)
          {
            defaultSatellite = satellite;
          }
        }
        if (comboBoxSatellite.SelectedItem == null)
        {
          comboBoxSatellite.SelectedItem = defaultSatellite;
          if (comboBoxSatellite.SelectedItem == null)
          {
            comboBoxSatellite.SelectedIndex = 0;
          }
        }
      }
      finally
      {
        comboBoxSatellite.EndUpdate();
      }

      comboBoxDiseqcSwitchPort.Items.AddRange(typeof(DiseqcPort).GetDescriptions());
      comboBoxToneBurst.Items.AddRange(typeof(ToneBurst).GetDescriptions());
      comboBoxTone22kState.Items.AddRange(typeof(Tone22kState).GetDescriptions());

      if (_tunerSatellite != null)
      {
        numericUpDownSatIpSource.Value = _tunerSatellite.SatIpSource;
        comboBoxDiseqcSwitchPort.SelectedItem = ((DiseqcPort)_tunerSatellite.DiseqcPort).GetDescription();
        comboBoxToneBurst.SelectedItem = ((ToneBurst)_tunerSatellite.ToneBurst).GetDescription();
        comboBoxTone22kState.SelectedItem = ((Tone22kState)_tunerSatellite.Tone22kState).GetDescription();
        checkBoxIsToroidalDish.Checked = _tunerSatellite.IsToroidalDish;
      }
      else
      {
        numericUpDownSatIpSource.Value = 0;
        comboBoxDiseqcSwitchPort.SelectedItem = DiseqcPort.None.GetDescription();
        comboBoxToneBurst.SelectedItem = ToneBurst.None.GetDescription();
        comboBoxTone22kState.SelectedItem = Tone22kState.Automatic.GetDescription();
        checkBoxIsToroidalDish.Checked = false;
      }

      IList<LnbType> lnbTypes = ServiceAgents.Instance.TunerServiceAgent.ListAllLnbTypes();
      LnbType defaultLnbType = null;
      int defaultLnbTypeLowBandLof = 9750000;  // "universal";
      string countryName = RegionInfo.CurrentRegion.EnglishName;
      if (string.Equals(countryName, "Australia"))
      {
        defaultLnbTypeLowBandLof = 10700000;
      }
      else if (string.Equals(countryName, "New Zealand"))
      {
        defaultLnbTypeLowBandLof = 10750000;
      }
      else if (string.Equals(countryName, "United States"))
      {
        defaultLnbTypeLowBandLof = 11250000;
      }
      comboBoxLnbType.BeginUpdate();
      try
      {
        comboBoxLnbType.Items.Clear();
        foreach (LnbType lnbType in lnbTypes)
        {
          comboBoxLnbType.Items.Add(lnbType);
          if (_tunerSatellite != null && _tunerSatellite.IdLnbType == lnbType.IdLnbType)
          {
            comboBoxLnbType.SelectedItem = lnbType;
          }
          else if (lnbType.LowBandFrequency == defaultLnbTypeLowBandLof)
          {
            defaultLnbType = lnbType;
          }
        }
        if (comboBoxLnbType.SelectedItem == null)
        {
          comboBoxLnbType.SelectedItem = defaultLnbType;
          if (comboBoxLnbType.SelectedItem == null)
          {
            comboBoxLnbType.SelectedIndex = 0;
          }
        }
      }
      finally
      {
        comboBoxLnbType.EndUpdate();
      }

      numericUpDownDiseqcMotorPosition.Value = 1;
      comboBoxDiseqcMotorPositionType.BeginUpdate();
      try
      {
        comboBoxDiseqcMotorPositionType.Items.Clear();
        comboBoxDiseqcMotorPositionType.Items.Add(DISEQC_MOTOR_POSITION_TYPE_NONE);
        comboBoxDiseqcMotorPositionType.Items.Add(DISEQC_MOTOR_POSITION_TYPE_USALS);
        comboBoxDiseqcMotorPositionType.Items.Add(DISEQC_MOTOR_POSITION_TYPE_STORED);
        if (_tunerSatellite == null || _tunerSatellite.DiseqcMotorPosition == TunerSatellite.DISEQC_MOTOR_POSITION_NONE)
        {
          comboBoxDiseqcMotorPositionType.SelectedItem = DISEQC_MOTOR_POSITION_TYPE_NONE;
        }
        else if (_tunerSatellite.DiseqcMotorPosition == TunerSatellite.DISEQC_MOTOR_POSITION_USALS)
        {
          comboBoxDiseqcMotorPositionType.SelectedItem = DISEQC_MOTOR_POSITION_TYPE_USALS;
        }
        else
        {
          comboBoxDiseqcMotorPositionType.SelectedItem = DISEQC_MOTOR_POSITION_TYPE_STORED;
          numericUpDownDiseqcMotorPosition.Value = _tunerSatellite.DiseqcMotorPosition;
        }
      }
      finally
      {
        comboBoxDiseqcMotorPositionType.EndUpdate();
      }
    }

    private void buttonOkay_Click(object sender, EventArgs e)
    {
      if (_idTunerSatellite > 0)
      {
        this.LogInfo("tuner satellite: save new");
        _tunerSatellite = new TunerSatellite();
      }
      else
      {
        this.LogInfo("tuner satellite: save changes, ID = {0}", _idTunerSatellite);
      }

      _tunerSatellite.IdSatellite = ((Satellite)comboBoxSatellite.SelectedItem).IdSatellite;
      _tunerSatellite.IdTuner = ((Tuner)comboBoxTuner.SelectedItem).IdTuner;
      _tunerSatellite.IdLnbType = ((LnbType)comboBoxLnbType.SelectedItem).IdLnbType;
      _tunerSatellite.SatIpSource = (int)numericUpDownSatIpSource.Value;
      _tunerSatellite.DiseqcPort = Convert.ToInt32(typeof(DiseqcPort).GetEnumFromDescription((string)comboBoxDiseqcSwitchPort.SelectedItem));

      string diseqcMotorPositionType = (string)comboBoxDiseqcMotorPositionType.SelectedItem;
      if (string.Equals(diseqcMotorPositionType, DISEQC_MOTOR_POSITION_TYPE_STORED))
      {
        _tunerSatellite.DiseqcMotorPosition = (int)numericUpDownDiseqcMotorPosition.Value;
      }
      else if (string.Equals(diseqcMotorPositionType, DISEQC_MOTOR_POSITION_TYPE_USALS))
      {
        _tunerSatellite.DiseqcMotorPosition = TunerSatellite.DISEQC_MOTOR_POSITION_USALS;
      }
      else
      {
        _tunerSatellite.DiseqcMotorPosition = TunerSatellite.DISEQC_MOTOR_POSITION_NONE;
      }

      _tunerSatellite.ToneBurst = Convert.ToInt32(typeof(ToneBurst).GetEnumFromDescription((string)comboBoxToneBurst.SelectedItem));
      _tunerSatellite.Tone22kState = Convert.ToInt32(typeof(Tone22kState).GetEnumFromDescription((string)comboBoxTone22kState.SelectedItem));
      _tunerSatellite.IsToroidalDish = checkBoxIsToroidalDish.Checked;

      _tunerSatellite = ServiceAgents.Instance.TunerServiceAgent.SaveTunerSatellite(_tunerSatellite);
      DialogResult = DialogResult.OK;
      Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      if (_idTunerSatellite <= 0)
      {
        this.LogInfo("tuner satellite: cancel new");
      }
      else
      {
        this.LogInfo("tuner satellite: cancel changes, ID = {0}", _idTunerSatellite);
      }
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void comboBoxSatellite_SelectedIndexChanged(object sender, EventArgs e)
    {
      // Try to maintain the current selection.
      Tuner selectedTuner = (Tuner)comboBoxTuner.SelectedItem;
      comboBoxTuner.BeginUpdate();
      try
      {
        comboBoxTuner.Items.Clear();
        foreach (Tuner tuner in _satelliteTuners)
        {
          comboBoxTuner.Items.Add(tuner);
          if (selectedTuner != null && selectedTuner.IdTuner == tuner.IdTuner)
          {
            comboBoxTuner.SelectedItem = tuner;
          }
        }
        if (comboBoxTuner.SelectedItem == null)
        {
          comboBoxTuner.SelectedIndex = 0;
        }
      }
      finally
      {
        comboBoxTuner.EndUpdate();
      }
    }

    private void comboBoxLnbType_Enter(object sender, EventArgs e)
    {
      // Show the LNB parameters in a tool tip when the mouse hovers over the
      // LNB type combo box.
      LnbType lnbType = (LnbType)comboBoxLnbType.SelectedItem;
      if (lnbType == null)
      {
        return;
      }

      string toolTipText;
      if (lnbType.LowBandFrequency <= 0)
      {
        return;
      }
      if (lnbType.HighBandFrequency <= 0)
      {
        toolTipText = string.Format("LOF = {0:0.###} GHz", (float)lnbType.LowBandFrequency / 1000000);
      }
      else if (lnbType.SwitchFrequency <= 0)
      {
        toolTipText = string.Format("LV/CR LOF = {0:0.###} GHz, LH/CL LOF = {1:0.###} GHz", (float)lnbType.LowBandFrequency / 1000000, (float)lnbType.HighBandFrequency / 1000000);
      }
      else
      {
        toolTipText = string.Format("low band LOF = {0:0.###} GHz, high band LOF = {1:0.###} GHz, switch = {3:0.###} GHz", (float)lnbType.LowBandFrequency / 1000000, (float)lnbType.HighBandFrequency / 1000000, (float)lnbType.SwitchFrequency / 1000000);
      }

      ToolTip tt = new ToolTip();
      tt.SetToolTip(comboBoxLnbType, toolTipText);
    }

    private void comboBoxLnbType_SelectedIndexChanged(object sender, EventArgs e)
    {
      // If the LNB type's local oscillator frequency is selected using the 22
      // kHz tone state (eg. universal LNB type) then disable the 22 kHz tone
      // state combo box. Otherwise enable it.
      LnbType lnbType = (LnbType)comboBoxLnbType.SelectedItem;
      comboBoxTone22kState.Enabled = lnbType.SwitchFrequency > 0 && numericUpDownSatIpSource.Value == 0;
      if (!comboBoxTone22kState.Enabled)
      {
        comboBoxTone22kState.SelectedItem = Tone22kState.Automatic.GetDescription();
      }
    }

    private void numericUpDownSatIpSource_ValueChanged(object sender, EventArgs e)
    {
      bool isNotSatIp = numericUpDownSatIpSource.Value == 0;
      comboBoxDiseqcSwitchPort.Enabled = isNotSatIp;
      comboBoxDiseqcMotorPositionType.Enabled = isNotSatIp;
      comboBoxToneBurst.Enabled = isNotSatIp;

      if (isNotSatIp)
      {
        comboBoxDiseqcMotorPositionType_SelectedIndexChanged(null, null);
      }
      else
      {
        comboBoxDiseqcSwitchPort.SelectedItem = DiseqcPort.None.GetDescription();
        comboBoxDiseqcMotorPositionType.SelectedItem = DISEQC_MOTOR_POSITION_TYPE_NONE;
        comboBoxToneBurst.SelectedItem = ToneBurst.None.GetDescription();
      }
      comboBoxLnbType_SelectedIndexChanged(null, null);
    }

    private void comboBoxDiseqcMotorPositionType_SelectedIndexChanged(object sender, EventArgs e)
    {
      string positionType = (string)comboBoxDiseqcMotorPositionType.SelectedItem;
      numericUpDownDiseqcMotorPosition.Enabled = string.Equals(positionType, DISEQC_MOTOR_POSITION_TYPE_STORED);
    }
  }
}