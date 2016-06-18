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
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

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

    public Satellites(ServerConfigurationChangedEventHandler handler)
      : base("Satellites", handler)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("satellites: activating");

      _changedTuners.Clear();

      IList<Tuner> tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerRelation.None);
      _allSatelliteTunerIds = new List<int>(tuners.Count);
      foreach (Tuner tuner in tuners)
      {
        if (((int)BroadcastStandard.MaskSatellite & tuner.SupportedBroadcastStandards) != 0)
        {
          _allSatelliteTunerIds.Add(tuner.IdTuner);
        }
      }

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

      DebugSettings();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("satellites: deactivating");

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
    }

    private void DebugTunerSatelliteSettings(TunerSatellite tunerSatellite)
    {
      this.LogDebug("satellites: tuner satellite...");
      this.LogDebug("  ID                = {0}", tunerSatellite.IdTunerSatellite);
      this.LogDebug("  satellite         = {0} [{1}]", tunerSatellite.Satellite, tunerSatellite.IdSatellite);
      this.LogDebug("  tuner ID          = {0}", !tunerSatellite.IdTuner.HasValue ? "[null]" : tunerSatellite.IdTuner.ToString());
      this.LogDebug("  SAT>IP source     = {0}", tunerSatellite.SatIpSource);
      this.LogDebug("  LNB type          = {0} [{1}]", tunerSatellite.LnbType.Name, tunerSatellite.IdLnbType);
      this.LogDebug("  DiSEqC switch     = {0}", (DiseqcPort)tunerSatellite.DiseqcPort);
      this.LogDebug("  DiSEqC motor pos. = {0}", GetDiseqcMotorPositionDescription(tunerSatellite.DiseqcMotorPosition));
      this.LogDebug("  tone burst        = {0}", (ToneBurst)tunerSatellite.ToneBurst);
      this.LogDebug("  22 kHz tone state = {0}", (Tone22kState)tunerSatellite.Tone22kState);
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
      row.Cells["dataGridViewColumnTuner"].Value = tunerSatellite.Tuner.ToString();
      row.Cells["dataGridViewColumnSatIpSource"].Value = tunerSatellite.SatIpSource.ToString();
      if (tunerSatellite.SatIpSource == 0)
      {
        row.Cells["dataGridViewColumnLnbType"].Value = tunerSatellite.LnbType.ToString();
        row.Cells["dataGridViewColumnDiseqcMotor"].Value = GetDiseqcMotorPositionDescription(tunerSatellite.DiseqcMotorPosition);
        row.Cells["dataGridViewColumnDiseqcSwitch"].Value = ((DiseqcPort)tunerSatellite.DiseqcPort).GetDescription();
        row.Cells["dataGridViewColumnToneBurst"].Value = ((ToneBurst)tunerSatellite.ToneBurst).GetDescription();
        row.Cells["dataGridViewColumnTone22kState"].Value = ((Tone22kState)tunerSatellite.Tone22kState).GetDescription();
      }

      string isToroidalDish = "No";
      if (tunerSatellite.IsToroidalDish)
      {
        isToroidalDish = "Yes";
      }
      row.Cells["dataGridViewColumnIsToroidalDish"].Value = isToroidalDish;
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
  }
}