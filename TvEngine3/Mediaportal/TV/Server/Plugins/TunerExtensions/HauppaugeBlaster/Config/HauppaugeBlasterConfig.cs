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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBlaster.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBlaster.Config
{
  internal partial class HauppaugeBlasterConfig : SectionSettings
  {
    private string _blastCfgLocation = null;
    private string _tunerExternalIdPort1 = null;
    private string _tunerExternalIdPort2 = null;

    public HauppaugeBlasterConfig()
      : base("Hauppauge Blaster")
    {
      ServiceAgents.Instance.AddGenericService<IHauppaugeBlasterConfigService>();
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("Hauppauge blaster config: activating");

      ServiceAgents.Instance.PluginService<IHauppaugeBlasterConfigService>().GetBlasterTunerExternalIds(out _tunerExternalIdPort1, out _tunerExternalIdPort2);
      this.LogDebug("  port 1 tuner      = {0}", _tunerExternalIdPort1 ?? "[null]");
      this.LogDebug("  port 2 tuner      = {0}", _tunerExternalIdPort2 ?? "[null]");

      string irBlastVersion;
      bool isHcwIrBlastDllPresent;
      string blasterVersion;
      int blasterPortCount;
      ServiceAgents.Instance.PluginService<IHauppaugeBlasterConfigService>().GetBlasterInstallDetails(out irBlastVersion, out _blastCfgLocation, out isHcwIrBlastDllPresent, out blasterVersion, out blasterPortCount);
      this.LogDebug("  IRBlast version    = {0}", irBlastVersion ?? "[null]");
      this.LogDebug("  BlastCfg location  = {0}", _blastCfgLocation ?? "[null]");
      this.LogDebug("  DLL present?       = {0}", isHcwIrBlastDllPresent);
      this.LogDebug("  blaster version    = {0}", blasterVersion);
      this.LogDebug("  blaster port count = {0}", blasterPortCount);
      if (isHcwIrBlastDllPresent)
      {
        string blasterInstallState = "Blaster port count is 0.";
        if (blasterPortCount > 0)
        {
          blasterInstallState = string.Format("Blaster port count is {0} ({1}).", blasterPortCount, blasterVersion);
        }
        labelInstallState.Text = string.Format("Hauppauge IRBlast {0} installed. {1}", irBlastVersion ?? "[unknown]", blasterInstallState);
        labelInstallState.ForeColor = Color.ForestGreen;
      }
      else
      {
        labelInstallState.Text = "Hauppauge IRBlast not detected.";
        labelInstallState.ForeColor = Color.Red;
      }

      buttonBlastCfg.Enabled = blasterPortCount > 0 && _blastCfgLocation != null && File.Exists(_blastCfgLocation);

      comboBoxTunerSelectionPort1.Items.Clear();
      comboBoxTunerSelectionPort2.Items.Clear();
      if (blasterPortCount == 0)
      {
        comboBoxTunerSelectionPort1.Enabled = false;
        comboBoxTunerSelectionPort2.Enabled = false;
      }
      else
      {
        IList<Tuner> tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerRelation.None);
        this.LogDebug("Hauppauge blaster config: total tuner count = {0}", tuners.Count);

        comboBoxTunerSelectionPort1.Enabled = true;
        comboBoxTunerSelectionPort1.Items.Add(new Tuner() { Name = string.Empty, ExternalId = string.Empty });
        comboBoxTunerSelectionPort1.SelectedIndex = 0;
        foreach (Tuner tuner in tuners)
        {
          BroadcastStandard tunerSupportedBroadcastStandards = (BroadcastStandard)tuner.SupportedBroadcastStandards;
          if (!tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.AnalogTelevision) && !tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.ExternalInput))
          {
            continue;
          }
          comboBoxTunerSelectionPort1.Items.Add(tuner);
          if (string.Equals(_tunerExternalIdPort1, tuner.ExternalId))
          {
            comboBoxTunerSelectionPort1.SelectedIndex = comboBoxTunerSelectionPort1.Items.Count - 1;
          }
        }

        if (blasterPortCount > 1)
        {
          comboBoxTunerSelectionPort2.Enabled = true;
          bool isFirst = true;
          foreach (Tuner tuner in comboBoxTunerSelectionPort1.Items)
          {
            comboBoxTunerSelectionPort2.Items.Add(tuner);
            if (isFirst || string.Equals(_tunerExternalIdPort2, tuner.ExternalId))
            {
              comboBoxTunerSelectionPort2.SelectedIndex = comboBoxTunerSelectionPort2.Items.Count - 1;
            }
            isFirst = false;
          }
        }

        this.LogDebug("Hauppauge blaster config: analog tuner count = {0}", comboBoxTunerSelectionPort1.Items.Count - 1);
      }

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("Hauppauge blaster config: deactivating");

      string selectedTunerExternalIdPort1 = string.Empty;
      string selectedTunerExternalIdPort2 = string.Empty;
      bool doSave = false;
      Tuner selectedTuner;

      if (comboBoxTunerSelectionPort1.Enabled)
      {
        selectedTuner = comboBoxTunerSelectionPort1.SelectedItem as Tuner;
        if (selectedTuner != null)
        {
          selectedTunerExternalIdPort1 = selectedTuner.ExternalId;
        }
        if (!string.Equals(_tunerExternalIdPort1, selectedTunerExternalIdPort1))
        {
          this.LogInfo("Hauppauge blaster config: port 1 tuner changed from {0} to {1}", _tunerExternalIdPort1 ?? "[null]", selectedTunerExternalIdPort1);
          doSave = true;
          _tunerExternalIdPort1 = selectedTunerExternalIdPort1;
        }
      }

      if (comboBoxTunerSelectionPort2.Enabled)
      {
        selectedTuner = comboBoxTunerSelectionPort2.SelectedItem as Tuner;
        if (selectedTuner != null)
        {
          selectedTunerExternalIdPort2 = selectedTuner.ExternalId;
        }
        if (!string.Equals(_tunerExternalIdPort2, selectedTunerExternalIdPort2))
        {
          this.LogInfo("Hauppauge blaster config: port 2 tuner changed from {0} to {1}", _tunerExternalIdPort2 ?? "[null]", selectedTunerExternalIdPort2);
          doSave = true;
          _tunerExternalIdPort2 = selectedTunerExternalIdPort2;
        }
      }

      if (doSave)
      {
        ServiceAgents.Instance.PluginService<IHauppaugeBlasterConfigService>().SaveBlasterTunerExternalIds(_tunerExternalIdPort1, _tunerExternalIdPort2);
      }

      base.OnSectionDeActivated();
    }

    private void buttonBlastCfg_Click(object sender, System.EventArgs e)
    {
      this.LogDebug("Hauppauge blaster config: start BlastCfg, location = {0}", _blastCfgLocation ?? "[null]");
      if (string.IsNullOrWhiteSpace(_blastCfgLocation))
      {
        return;
      }

      try
      {
        Process.Start(_blastCfgLocation);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Hauppauge blaster config: failed to start BlastCfg, location = {0}", _blastCfgLocation);
      }
    }
  }
}