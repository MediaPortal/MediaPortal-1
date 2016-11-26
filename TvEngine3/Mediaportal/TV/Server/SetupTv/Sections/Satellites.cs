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
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Dialogs;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers.Enum;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum;
using MediaPortal.Common.Utils.ExtensionMethods;
using TuningDetail = Mediaportal.TV.Server.SetupTV.Sections.Helpers.TuningDetail;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class Satellites : SectionSettings
  {
    private const TunerSatelliteRelation REQUIRED_TUNER_SATELLITE_RELATIONS = TunerSatelliteRelation.LnbType | TunerSatelliteRelation.Satellite | TunerSatelliteRelation.Tuner;

    private HashSet<int> _changedTuners = new HashSet<int>();
    private List<int> _allSatelliteTunerIds = new List<int>();

    private decimal _originalUsalsLatitude;
    private decimal _originalUsalsLongitude;
    private decimal _originalUsalsAltitude;
    private decimal _originalDiseqcMotorSpeedSlow;
    private decimal _originalDiseqcMotorSpeedFast;

    private readonly System.Timers.Timer _diseqcMotorStatusUpdateTimer = new System.Timers.Timer();

    public Satellites(ServerConfigurationChangedEventHandler handler)
      : base("Satellites", handler)
    {
      InitializeComponent();

      _diseqcMotorStatusUpdateTimer.Interval = 1000;
      _diseqcMotorStatusUpdateTimer.Elapsed += UpdateDiseqcMotorStatus;
    }

    ~Satellites()
    {
      if (_diseqcMotorStatusUpdateTimer != null)
      {
        _diseqcMotorStatusUpdateTimer.Dispose();
      }
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("satellites: activating");

      _changedTuners.Clear();

      // satellites tab
      IList<TunerSatellite> tunerSatellites = ServiceAgents.Instance.TunerServiceAgent.ListAllTunerSatellites(REQUIRED_TUNER_SATELLITE_RELATIONS);
      dataGridViewTunerSatellites.Rows.Clear();
      dataGridViewTunerSatellites.Rows.Add(tunerSatellites.Count);
      int rowIndex = 0;
      foreach (TunerSatellite tunerSatellite in tunerSatellites)
      {
        DataGridViewRow row = dataGridViewTunerSatellites.Rows[rowIndex++];
        PopulateRowForTunerSatellite(row, tunerSatellite);
      }
      dataGridViewTunerSatellites_SelectionChanged(null, null);

      _originalUsalsLatitude = (decimal)ServiceAgents.Instance.SettingServiceAgent.GetValue("usalsLatitude", 0.0);
      numericUpDownUsalsLatitude.Value = _originalUsalsLatitude;
      _originalUsalsLongitude = (decimal)ServiceAgents.Instance.SettingServiceAgent.GetValue("usalsLongitude", 0.0);
      numericUpDownUsalsLongitude.Value = _originalUsalsLongitude;
      _originalUsalsAltitude = (decimal)ServiceAgents.Instance.SettingServiceAgent.GetValue("usalsAltitude", 0);
      numericUpDownUsalsAltitude.Value = _originalUsalsAltitude;
      _originalDiseqcMotorSpeedSlow = (decimal)ServiceAgents.Instance.SettingServiceAgent.GetValue("diseqcMotorSpeedSlow", TunerSatellite.DISEQC_MOTOR_DEFAULT_SPEED_SLOW);
      numericUpDownDiseqcMotorSpeedSlow.Value = _originalDiseqcMotorSpeedSlow;
      _originalDiseqcMotorSpeedFast = (decimal)ServiceAgents.Instance.SettingServiceAgent.GetValue("diseqcMotorSpeedFast", TunerSatellite.DISEQC_MOTOR_DEFAULT_SPEED_FAST);
      numericUpDownDiseqcMotorSpeedFast.Value = _originalDiseqcMotorSpeedFast;

      // DiSEqC motor setup tab
      IList<Tuner> tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerRelation.None);
      _allSatelliteTunerIds = new List<int>(tuners.Count);
      comboBoxDiseqcMotorSetupTuner.BeginUpdate();
      try
      {
        comboBoxDiseqcMotorSetupTuner.Items.Clear();
        foreach (Tuner tuner in tuners)
        {
          if (((int)BroadcastStandard.MaskSatellite & tuner.SupportedBroadcastStandards) != 0)
          {
            _allSatelliteTunerIds.Add(tuner.IdTuner);
            if (tuner.IsEnabled)
            {
              comboBoxDiseqcMotorSetupTuner.Items.Add(tuner);
            }
          }
        }
        if (comboBoxDiseqcMotorSetupTuner.Items.Count > 0)
        {
          comboBoxDiseqcMotorSetupTuner.SelectedIndex = 0;
        }
      }
      finally
      {
        comboBoxDiseqcMotorSetupTuner.EndUpdate();
      }

      numericUpDownDiseqcMotorSetupPositionStored.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("diseqcMotorSetupPositionStored", 1);
      numericUpDownDiseqcMotorSetupPositionUsals.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("diseqcMotorSetupPositionUsals", Satellite.DefaultSatelliteLongitude.GetValueOrDefault(0)) / 10;
      numericUpDownDiseqcMotorSetupManualMoveStepCount.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("diseqcMotorSetupManualMoveStepCount", 10);

      int longitude = ServiceAgents.Instance.SettingServiceAgent.GetValue("diseqcMotorSetupSatellite", Satellite.DefaultSatelliteLongitude.GetValueOrDefault(100000));
      IList<Satellite> satellites = ServiceAgents.Instance.TunerServiceAgent.ListAllSatellites();
      comboBoxDiseqcMotorSetupCheckSatellite.BeginUpdate();
      try
      {
        comboBoxDiseqcMotorSetupCheckSatellite.Items.Clear();
        foreach (Satellite satellite in satellites)
        {
          comboBoxDiseqcMotorSetupCheckSatellite.Items.Add(satellite);
          if (longitude == satellite.Longitude)
          {
            comboBoxDiseqcMotorSetupCheckSatellite.SelectedIndex = comboBoxDiseqcMotorSetupCheckSatellite.Items.Count - 1;
          }
        }
        if (comboBoxDiseqcMotorSetupCheckSatellite.Items.Count > 0)
        {
          comboBoxDiseqcMotorSetupCheckSatellite.SelectedIndex = 0;
        }
      }
      finally
      {
        comboBoxDiseqcMotorSetupCheckSatellite.EndUpdate();
      }

      string transmitter = ServiceAgents.Instance.SettingServiceAgent.GetValue("diseqcMotorSetupTransmitter", string.Empty);
      if (!string.IsNullOrEmpty(transmitter))
      {
        foreach (object item in comboBoxDiseqcMotorSetupCheckTransmitter.Items)
        {
          if (string.Equals(item.ToString(), transmitter))
          {
            comboBoxDiseqcMotorSetupCheckTransmitter.SelectedItem = item;
            break;
          }
        }
      }
      if (comboBoxDiseqcMotorSetupCheckTransmitter.SelectedItem == null)
      {
        comboBoxDiseqcMotorSetupCheckTransmitter.SelectedIndex = 0;
      }

      bool isTunerAvailable = comboBoxDiseqcMotorSetupTuner.Items.Count > 0;
      groupBoxDiseqcMotorSetupPosition.Enabled = isTunerAvailable;
      groupBoxDiseqcMotorSetupManualMove.Enabled = isTunerAvailable;
      groupBoxDiseqcMotorSetupMoveLimits.Enabled = isTunerAvailable;
      groupBoxDiseqcMotorSetupCheck.Enabled = isTunerAvailable;

      DebugSettings();

      _diseqcMotorStatusUpdateTimer.Enabled = tabControl.SelectedIndex == 1;

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("satellites: deactivating");

      _diseqcMotorStatusUpdateTimer.Enabled = false;

      if (_originalUsalsLatitude != numericUpDownUsalsLatitude.Value)
      {
        this.LogInfo("satellites: USALS latitude changed from {0}° to {1}°", _originalUsalsLatitude, numericUpDownUsalsLatitude.Value);
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("usalsLatitude", (double)numericUpDownUsalsLatitude.Value);
        _changedTuners.UnionWith(_allSatelliteTunerIds);
      }
      if (_originalUsalsLongitude != numericUpDownUsalsLongitude.Value)
      {
        this.LogInfo("satellites: USALS longitude changed from {0}° to {1}°", _originalUsalsLongitude, numericUpDownUsalsLongitude.Value);
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("usalsLongitude", (double)numericUpDownUsalsLongitude.Value);
        _changedTuners.UnionWith(_allSatelliteTunerIds);
      }
      if (_originalUsalsAltitude != numericUpDownUsalsAltitude.Value)
      {
        this.LogInfo("satellites: USALS altitude changed from {0}° to {1}°", _originalUsalsAltitude, numericUpDownUsalsAltitude.Value);
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("usalsAltitude", (int)numericUpDownUsalsAltitude.Value);
        _changedTuners.UnionWith(_allSatelliteTunerIds);
      }
      if (_originalDiseqcMotorSpeedSlow != numericUpDownDiseqcMotorSpeedSlow.Value)
      {
        this.LogInfo("satellites: DiSEqC motor slow speed changed from {0} °/s to {1} °/s", _originalDiseqcMotorSpeedSlow, numericUpDownDiseqcMotorSpeedSlow.Value);
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("diseqcMotorSpeedSlow", (double)numericUpDownDiseqcMotorSpeedSlow.Value);
        _changedTuners.UnionWith(_allSatelliteTunerIds);
      }
      if (_originalDiseqcMotorSpeedFast != numericUpDownDiseqcMotorSpeedFast.Value)
      {
        this.LogInfo("satellites: DiSEqC motor fast speed changed from {0} °/s to {1} °/s", _originalDiseqcMotorSpeedFast, numericUpDownDiseqcMotorSpeedFast.Value);
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("diseqcMotorSpeedFast", (double)numericUpDownDiseqcMotorSpeedFast.Value);
        _changedTuners.UnionWith(_allSatelliteTunerIds);
      }

      if (_changedTuners.Count > 0)
      {
        OnServerConfigurationChanged(this, false, _changedTuners);
      }

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("diseqcMotorSetupPositionStored", (int)numericUpDownDiseqcMotorSetupPositionStored.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("diseqcMotorSetupPositionUsals", (int)(numericUpDownDiseqcMotorSetupPositionUsals.Value * 10));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("diseqcMotorSetupManualMoveStepCount", (int)numericUpDownDiseqcMotorSetupManualMoveStepCount.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("diseqcMotorSetupSatellite", ((Satellite)comboBoxDiseqcMotorSetupCheckSatellite.SelectedItem).Longitude);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("diseqcMotorSetupTransmitter", comboBoxDiseqcMotorSetupCheckTransmitter.SelectedItem.ToString());

      base.OnSectionDeActivated();
    }

    private void DebugSettings()
    {
      this.LogDebug("  USALS location...");
      this.LogDebug("    latitude        = {0}°", numericUpDownUsalsLatitude.Value);
      this.LogDebug("    longitude       = {0}°", numericUpDownUsalsLongitude.Value);
      this.LogDebug("    altitude        = {0} m", numericUpDownUsalsAltitude.Value);
      this.LogDebug("  DiSEqC motor speed...");
      this.LogDebug("    slow            = {0} °/s", numericUpDownDiseqcMotorSpeedSlow.Value);
      this.LogDebug("    fast            = {0} °/s", numericUpDownDiseqcMotorSpeedFast.Value);
      this.LogDebug("  DiSEqC motor setup...");
      this.LogDebug("    stored position = {0}", numericUpDownDiseqcMotorSetupPositionStored.Value);
      this.LogDebug("    USALS position  = {0}", numericUpDownDiseqcMotorSetupPositionUsals.Value);
      this.LogDebug("    move step count = {0}", numericUpDownDiseqcMotorSetupManualMoveStepCount.Value);
      this.LogDebug("    satellite       = {0}", comboBoxDiseqcMotorSetupCheckSatellite.SelectedItem);
      this.LogDebug("    transmitter     = {0}", comboBoxDiseqcMotorSetupCheckTransmitter.SelectedItem);
    }

    private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
      _diseqcMotorStatusUpdateTimer.Enabled = tabControl.SelectedIndex == 1;
    }

    #region satellites tab

    private void DebugTunerSatelliteSettings(TunerSatellite tunerSatellite)
    {
      this.LogDebug("satellites: tuner satellite...");
      this.LogDebug("  ID                = {0}", tunerSatellite.IdTunerSatellite);
      this.LogDebug("  satellite         = {0} [{1}]", tunerSatellite.Satellite, tunerSatellite.IdSatellite);
      this.LogDebug("  tuner ID          = {0}", !tunerSatellite.IdTuner.HasValue ? "[null]" : tunerSatellite.IdTuner.ToString());
      this.LogDebug("  LNB type          = {0} [{1}]", tunerSatellite.LnbType.Name, tunerSatellite.IdLnbType);
      this.LogDebug("  SAT>IP source     = {0}", tunerSatellite.SatIpSource);
      this.LogDebug("  DiSEqC switch     = {0}", (DiseqcPort)tunerSatellite.DiseqcPort);
      this.LogDebug("  DiSEqC motor pos. = {0}", GetDiseqcMotorPositionDescription(tunerSatellite.DiseqcMotorPosition));
      this.LogDebug("  tone burst        = {0}", (ToneBurst)tunerSatellite.ToneBurst);
      this.LogDebug("  22 kHz tone state = {0}", (Tone22kState)tunerSatellite.Tone22kState);
      this.LogDebug("  polarisations     = [{0}]", (Polarisation)tunerSatellite.Polarisations);
      this.LogDebug("  is toroidal dish? = {0}", tunerSatellite.IsToroidalDish);
    }

    private static string GetDiseqcMotorPositionDescription(int position)
    {
      if (position == TunerSatellite.DISEQC_MOTOR_POSITION_NONE)
      {
        return "None";
      }
      else if (position == TunerSatellite.DISEQC_MOTOR_POSITION_USALS)
      {
        return "USALS";
      }
      return string.Format("Stored {0}", position);
    }

    private void PopulateRowForTunerSatellite(DataGridViewRow row, TunerSatellite tunerSatellite)
    {
      DebugTunerSatelliteSettings(tunerSatellite);
      row.Tag = tunerSatellite;
      row.Cells["dataGridViewColumnSatellite"].Value = tunerSatellite.Satellite.ToString();
      row.Cells["dataGridViewColumnTuner"].Value = tunerSatellite.IdTuner.HasValue ? tunerSatellite.Tuner.ToString() : "All";
      row.Cells["dataGridViewColumnLnbType"].Value = tunerSatellite.LnbType.ToString();
      row.Cells["dataGridViewColumnSatIpSource"].Value = tunerSatellite.SatIpSource.ToString();
      if (tunerSatellite.SatIpSource == 0)
      {
        row.Cells["dataGridViewColumnDiseqcMotor"].Value = GetDiseqcMotorPositionDescription(tunerSatellite.DiseqcMotorPosition);
        row.Cells["dataGridViewColumnDiseqcSwitch"].Value = ((DiseqcPort)tunerSatellite.DiseqcPort).GetDescription();
        row.Cells["dataGridViewColumnToneBurst"].Value = ((ToneBurst)tunerSatellite.ToneBurst).GetDescription();
        row.Cells["dataGridViewColumnTone22kState"].Value = ((Tone22kState)tunerSatellite.Tone22kState).GetDescription();
      }

      Polarisation polarisations = (Polarisation)tunerSatellite.Polarisations;
      string polarisationsString = string.Empty;
      if (polarisations.HasFlag(Polarisation.CircularLeft | Polarisation.CircularRight | Polarisation.LinearHorizontal | Polarisation.LinearVertical))
      {
        polarisationsString = "All";
        polarisations = Polarisation.Automatic;
      }
      else if (polarisations.HasFlag(Polarisation.CircularLeft | Polarisation.CircularRight))
      {
        polarisationsString = "Circular";
        polarisations &= ~(Polarisation.CircularLeft | Polarisation.CircularRight);
      }
      else if (polarisations.HasFlag(Polarisation.LinearHorizontal | Polarisation.LinearVertical))
      {
        polarisationsString = "Linear";
        polarisations &= ~(Polarisation.LinearHorizontal | Polarisation.LinearVertical);
      }
      if (polarisations != Polarisation.Automatic)
      {
        if (string.IsNullOrEmpty(polarisationsString))
        {
          polarisationsString = string.Join(", ", typeof(Polarisation).GetDescriptions((int)polarisations, false));
        }
        else
        {
          polarisationsString = string.Format("{0}, {1}", polarisationsString, string.Join(", ", typeof(Polarisation).GetDescriptions((int)polarisations, false)));
        }
      }
      row.Cells["dataGridViewColumnPolarisations"].Value = polarisationsString;

      row.Cells["dataGridViewColumnIsToroidalDish"].Value = tunerSatellite.IsToroidalDish ? "Yes" : "No";
    }

    private void buttonTunerSatelliteAdd_Click(object sender, EventArgs e)
    {
      using (FormEditTunerSatellite dlg = new FormEditTunerSatellite())
      {
        if (dlg.ShowDialog() != DialogResult.OK)
        {
          return;
        }

        TunerSatellite tunerSatellite = ServiceAgents.Instance.TunerServiceAgent.GetTunerSatellite(dlg.TunerSatellite.IdTunerSatellite, REQUIRED_TUNER_SATELLITE_RELATIONS);
        PopulateRowForTunerSatellite(dataGridViewTunerSatellites.Rows[dataGridViewTunerSatellites.Rows.Add()], tunerSatellite);
        if (tunerSatellite.IdTuner.HasValue)
        {
          _changedTuners.Add(tunerSatellite.IdTuner.Value);
        }
        else
        {
          _changedTuners.UnionWith(_allSatelliteTunerIds);
        }
      }
    }

    private void buttonTunerSatelliteEdit_Click(object sender, EventArgs e)
    {
      if (dataGridViewTunerSatellites.SelectedRows == null)
      {
        return;
      }

      foreach (DataGridViewRow row in dataGridViewTunerSatellites.SelectedRows)
      {
        TunerSatellite tunerSatellite = row.Tag as TunerSatellite;
        if (tunerSatellite == null)
        {
          continue;
        }

        int? originalTunerId = tunerSatellite.IdTuner;
        using (FormEditTunerSatellite dlg = new FormEditTunerSatellite(tunerSatellite.IdTunerSatellite))
        {
          if (dlg.ShowDialog() != DialogResult.OK)
          {
            continue;
          }
        }

        tunerSatellite = ServiceAgents.Instance.TunerServiceAgent.GetTunerSatellite(tunerSatellite.IdTunerSatellite, REQUIRED_TUNER_SATELLITE_RELATIONS);
        PopulateRowForTunerSatellite(row, tunerSatellite);
        if (originalTunerId.HasValue)
        {
          _changedTuners.Add(originalTunerId.Value);
        }
        else
        {
          _changedTuners.UnionWith(_allSatelliteTunerIds);
        }
        if (originalTunerId != tunerSatellite.IdTuner)
        {
          if (tunerSatellite.IdTuner.HasValue)
          {
            _changedTuners.Add(tunerSatellite.IdTuner.Value);
          }
          else
          {
            _changedTuners.UnionWith(_allSatelliteTunerIds);
          }
        }
      }
    }

    private void buttonTunerSatelliteDelete_Click(object sender, EventArgs e)
    {
      if (dataGridViewTunerSatellites.SelectedRows == null)
      {
        return;
      }

      NotifyForm dlg = null;
      try
      {
        if (dataGridViewTunerSatellites.SelectedRows.Count > 10)
        {
          dlg = new NotifyForm("Deleting selected tuner satellites...", "This can take some time." + Environment.NewLine + Environment.NewLine + "Please be patient...");
          dlg.Show(this);
          dlg.WaitForDisplay();
        }

        foreach (DataGridViewRow row in dataGridViewTunerSatellites.SelectedRows)
        {
          TunerSatellite tunerSatellite = row.Tag as TunerSatellite;
          if (tunerSatellite != null)
          {
            this.LogInfo("satellites: tuner satellite {0} deleted", tunerSatellite.IdTunerSatellite);
            if (tunerSatellite.IdTuner.HasValue)
            {
              _changedTuners.Add(tunerSatellite.IdTuner.Value);
            }
            else
            {
              _changedTuners.UnionWith(_allSatelliteTunerIds);
            }
            ServiceAgents.Instance.TunerServiceAgent.DeleteTunerSatellite(tunerSatellite.IdTunerSatellite);
          }
          dataGridViewTunerSatellites.Rows.RemoveAt(row.Index);
        }
      }
      finally
      {
        if (dlg != null)
        {
          dlg.Close();
          dlg.Dispose();
        }
      }
    }

    private void dataGridViewTunerSatellites_SelectionChanged(object sender, EventArgs e)
    {
      // The edit and delete buttons are only enabled when at least one tuner
      // satellite row is selected.
      bool enableButtons = dataGridViewTunerSatellites.SelectedRows != null && dataGridViewTunerSatellites.SelectedRows.Count > 0;
      buttonTunerSatelliteEdit.Enabled = enableButtons;
      buttonTunerSatelliteDelete.Enabled = enableButtons;
    }

    private void dataGridViewTunerSatellites_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Delete)
      {
        buttonTunerSatelliteDelete_Click(null, null);
        e.Handled = true;
      }
    }

    #endregion

    #region DiSEqC motor setup tab

    private void UpdateDiseqcMotorStatus(object sender, System.Timers.ElapsedEventArgs e)
    {
      tabPageDiseqcMotorSetup.Invoke((MethodInvoker)delegate
      {
        int satellitePosition = -1;
        double satelliteLongitude = 0;
        int stepCountAzimuth = 0;
        int stepCountElevation = 0;

        bool isSignalLocked = false;
        bool isSignalPresent = false;
        int signalStrength = 0;
        int signalQuality = 0;

        int tunerId;
        if (GetSelectedTunerId(out tunerId, false))
        {
          ServiceAgents.Instance.ControllerServiceAgent.DiSEqCGetPosition(tunerId, out satellitePosition, out satelliteLongitude, out stepCountAzimuth, out stepCountElevation);
          ServiceAgents.Instance.ControllerServiceAgent.GetSignalStatus(tunerId, false, out isSignalLocked, out isSignalPresent, out signalStrength, out signalQuality);
        }

        if ((satellitePosition < 0 || satellitePosition > 255) && (satelliteLongitude < 180 || satelliteLongitude > 180))
        {
          labelDiseqcMotorSetupPositionCurrentValue.Text = "Unknown";
        }
        else
        {
          string positionName;
          if (satellitePosition == 0)
          {
            positionName = "Reference";
          }
          else if (satellitePosition < 0 && satellitePosition > 255)
          {
            positionName = string.Format("Stored {0}", satellitePosition);
          }
          else
          {
            positionName = string.Format("USALS {0}°", satelliteLongitude);
          }
          if (stepCountAzimuth != 0 || stepCountElevation != 0)
          {
            positionName = string.Format("{0}, {1} azimuth, {2} elevation", stepCountAzimuth, stepCountElevation);
          }
          labelDiseqcMotorSetupPositionCurrentValue.Text = positionName;
        }

        labelDiseqcMotorSetupCheckIsSignalLockedValue.Text = isSignalLocked ? "Yes" : "No";
        labelDiseqcMotorSetupCheckIsSignalPresentValue.Text = isSignalPresent ? "Yes" : "No";
        progressBarDiseqcMotorSetupCheckSignalStrength.Value = signalStrength;
        progressBarDiseqcMotorSetupCheckSignalQuality.Value = signalQuality;
      });
    }

    private bool GetSelectedTunerId(out int tunerId, bool showMessage = true)
    {
      tunerId = -1;
      Tuner tuner = comboBoxDiseqcMotorSetupTuner.SelectedItem as Tuner;
      if (tuner == null)
      {
        return false;
      }
      tunerId = tuner.IdTuner;
      if (ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(tunerId))
      {
        return true;
      }
      MessageBox.Show("Tuner not found. Please ensure the tuner is connected, enabled, available and accessible.", SectionSettings.MESSAGE_CAPTION);
      return false;
    }

    private void buttonDiseqcMotorSetupPositionStoredGoTo_Click(object sender, EventArgs e)
    {
      int tunerId;
      if (GetSelectedTunerId(out tunerId))
      {
        byte position = (byte)numericUpDownDiseqcMotorSetupPositionStored.Value;
        this.LogDebug("satellites: DiSEqC motor go to stored position, tuner ID = {0}, position = {1}", tunerId, position);
        ServiceAgents.Instance.ControllerServiceAgent.DiSEqCGotoStoredPosition(tunerId, position);
      }
    }

    private void buttonDiseqcMotorSetupPositionStoredStore_Click(object sender, EventArgs e)
    {
      int tunerId;
      if (GetSelectedTunerId(out tunerId))
      {
        byte position = (byte)numericUpDownDiseqcMotorSetupPositionStored.Value;
        this.LogDebug("satellites: DiSEqC motor store position, tuner ID = {0}, position = {1}", tunerId, position);
        ServiceAgents.Instance.ControllerServiceAgent.DiSEqCStorePosition(tunerId, position);
      }
    }

    private void buttonDiseqcMotorSetupPositionUsalsGoTo_Click(object sender, EventArgs e)
    {
      int tunerId;
      if (GetSelectedTunerId(out tunerId))
      {
        double longitude = (double)numericUpDownDiseqcMotorSetupPositionUsals.Value;
        this.LogDebug("satellites: DiSEqC motor go to USALS (angular) position, tuner ID = {0}, longitude = {1}", tunerId, longitude);
        ServiceAgents.Instance.ControllerServiceAgent.DiSEqCGotoAngularPosition(tunerId, longitude);
      }
    }

    private void buttonDiseqcMotorSetupPositionReset_Click(object sender, EventArgs e)
    {
      int tunerId;
      if (GetSelectedTunerId(out tunerId))
      {
        double longitude = (double)numericUpDownDiseqcMotorSetupPositionUsals.Value;
        this.LogDebug("satellites: DiSEqC reset, tuner ID = {0}", tunerId);
        ServiceAgents.Instance.ControllerServiceAgent.DiSEqCReset(tunerId);
      }
    }

    private void buttonDiseqcMotorSetupPositionReferenceGoTo_Click(object sender, EventArgs e)
    {
      int tunerId;
      if (GetSelectedTunerId(out tunerId))
      {
        this.LogDebug("satellites: DiSEqC motor go to reference position, tuner ID = {0}", tunerId);
        ServiceAgents.Instance.ControllerServiceAgent.DiSEqCGotoReferencePosition(tunerId);
      }
    }

    private void Drive(DiseqcDirection direction)
    {
      int tunerId;
      if (GetSelectedTunerId(out tunerId))
      {
        byte stepCount = (byte)numericUpDownDiseqcMotorSetupManualMoveStepCount.Value;
        this.LogDebug("satellites: DiSEqC motor drive, tuner ID = {0}, direction = {1}, step count = {2}", tunerId, direction, stepCount);
        ServiceAgents.Instance.ControllerServiceAgent.DiSEqCDriveMotor(tunerId, direction, stepCount);
      }
    }

    private void buttonDiseqcMotorSetupManualMoveUp_Click(object sender, EventArgs e)
    {
      Drive(DiseqcDirection.Up);
    }

    private void buttonDiseqcMotorSetupManualMoveWest_Click(object sender, EventArgs e)
    {
      Drive(DiseqcDirection.West);
    }

    private void buttonDiseqcMotorSetupManualMoveHalt_Click(object sender, EventArgs e)
    {
      int tunerId;
      if (GetSelectedTunerId(out tunerId))
      {
        this.LogDebug("satellites: DiSEqC motor halt, tuner ID = {0}", tunerId);
        ServiceAgents.Instance.ControllerServiceAgent.DiSEqCStopMotor(tunerId);
      }
    }

    private void buttonDiseqcMotorSetupManualMoveEast_Click(object sender, EventArgs e)
    {
      Drive(DiseqcDirection.East);
    }

    private void buttonDiseqcMotorSetupManualMoveDown_Click(object sender, EventArgs e)
    {
      Drive(DiseqcDirection.Down);
    }

    private void buttonDiseqcMotorSetupMoveLimitsDisable_Click(object sender, EventArgs e)
    {
      int tunerId;
      if (GetSelectedTunerId(out tunerId))
      {
        this.LogDebug("satellites: DiSEqC motor disable movement limits, tuner ID = {0}", tunerId);
        ServiceAgents.Instance.ControllerServiceAgent.DiSEqCForceLimit(tunerId, false);
      }
    }

    private void buttonDiseqcMotorSetupMoveLimitsSetWest_Click(object sender, EventArgs e)
    {
      int tunerId;
      if (GetSelectedTunerId(out tunerId))
      {
        this.LogDebug("satellites: DiSEqC motor set Westward movement limit, tuner ID = {0}", tunerId);
        ServiceAgents.Instance.ControllerServiceAgent.DiSEqCSetWestLimit(tunerId);
      }
    }

    private void buttonDiseqcMotorSetupMoveLimitsSetEast_Click(object sender, EventArgs e)
    {
      int tunerId;
      if (GetSelectedTunerId(out tunerId))
      {
        this.LogDebug("satellites: DiSEqC motor set Eastward movement limit, tuner ID = {0}", tunerId);
        ServiceAgents.Instance.ControllerServiceAgent.DiSEqCSetEastLimit(tunerId);
      }
    }

    private void buttonDiseqcMotorSetupMoveLimitsEnable_Click(object sender, EventArgs e)
    {
      int tunerId;
      if (GetSelectedTunerId(out tunerId))
      {
        this.LogDebug("satellites: DiSEqC motor enable movement limits, tuner ID = {0}", tunerId);
        ServiceAgents.Instance.ControllerServiceAgent.DiSEqCForceLimit(tunerId, true);
      }
    }

    private void comboBoxDiseqcMotorSetupCheckSatellite_SelectedIndexChanged(object sender, EventArgs e)
    {
      Satellite satellite = (Satellite)comboBoxDiseqcMotorSetupCheckSatellite.SelectedItem;
      TuningDetailFilter.Load(-1, TuningDetailGroup.Satellite, satellite.ToString() + ".xml", comboBoxDiseqcMotorSetupCheckTransmitter);
    }

    private void buttonDiseqcMotorSetupCheckTune_Click(object sender, EventArgs e)
    {
      int tunerId;
      if (!GetSelectedTunerId(out tunerId))
      {
        return;
      }
      TuningDetail tuningDetail = comboBoxDiseqcMotorSetupCheckTransmitter.SelectedItem as TuningDetail;
      this.LogDebug("satellites: DiSEqC motor tune, tuner ID = {0}, satellite = {1}, tuning detail = {2}", tunerId, Satellite.LongitudeString(tuningDetail.Longitude), tuningDetail);
      tuningDetail.Longitude = TunerSatellite.LONGITUDE_UNSPECIFIED;
      IChannel tuningChannel = tuningDetail.GetTuningChannel();
      // TODO
    }

    #endregion
  }
}