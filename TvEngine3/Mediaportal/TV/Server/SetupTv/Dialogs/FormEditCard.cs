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
using DirectShowLib;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditCard : Form
  {
    private Card _tuner;
    private CardType _tunerType;
    private bool _tunerIsInGroup;

    public FormEditCard(Card tuner, CardType tunerType, bool tunerIsInGroup)
    {
      _tuner = tuner;
      _tunerType = tunerType;
      _tunerIsInGroup = tunerIsInGroup;
      InitializeComponent();
    }

    public Card Tuner
    {
      get
      {
        return _tuner;
      }
    }

    private void FormEditCard_Load(object sender, EventArgs e)
    {
      textBoxTunerName.Text = _tuner.Name;
      checkBoxEpgGrabEnabled.Checked = _tuner.GrabEPG;

      // Tuners can't be preloaded if they're part of a tuner group.
      if (_tunerIsInGroup)
      {
        checkBoxPreloadTuner.Enabled = false;
        _tuner.PreloadCard = false;
      }
      checkBoxPreloadTuner.Checked = _tuner.PreloadCard;

      checkBoxAlwaysSendDiseqcCommands.Checked = _tuner.AlwaysSendDiseqcCommands;
      numericUpDownDiseqcCommandRepeatCount.Value = _tuner.DiseqcCommandRepeatCount;
      groupBoxDiseqcSettings.Enabled = _tunerType == CardType.DvbS;

      checkBoxConditionalAccessEnabled.Checked = _tuner.UseConditionalAccess;
      numericUpDownDecryptLimit.Value = _tuner.DecryptLimit;
      comboBoxMultiChannelDecryptMode.Items.AddRange(typeof(MultiChannelDecryptMode).GetDescriptions());
      comboBoxMultiChannelDecryptMode.SelectedItem = ((MultiChannelDecryptMode)_tuner.MultiChannelDecryptMode).GetDescription();
      comboBoxCamType.Items.AddRange(typeof(CamType).GetDescriptions());
      comboBoxCamType.SelectedItem = ((CamType)_tuner.CamType).GetDescription();
      SetConditionalAccessFieldAvailability();

      comboBoxIdleMode.Items.AddRange(typeof(IdleMode).GetDescriptions());
      comboBoxIdleMode.SelectedItem = ((IdleMode)_tuner.IdleMode).GetDescription();

      if (_tunerType == CardType.Analog || !_tuner.DevicePath.StartsWith("@device"))
      {
        comboBoxNetworkProvider.Enabled = false;
      }
      else
      {
        // Network provider availability depends on the TV service environment.
        // - The Microsoft specific network provider should be available on all
        //   operating systems from XP to present day.
        // - The Microsoft generic network provider is available only on XP MCE
        //   2005 + Update Rollup 2 and newer.
        // - The MediaPortal network provider has not been released at time of
        //   writing.
        // We have no way to determine which network providers are actually
        // available, so we show them all.
        comboBoxNetworkProvider.Enabled = true;
        comboBoxNetworkProvider.Items.AddRange(typeof(DbNetworkProvider).GetDescriptions());
        comboBoxNetworkProvider.SelectedItem = ((DbNetworkProvider)_tuner.NetProvider).GetDescription();
      }

      if (_tunerType == CardType.Analog)
      {
        comboBoxPidFilterMode.Enabled = false;
      }
      else
      {
        comboBoxPidFilterMode.Items.AddRange(typeof(PidFilterMode).GetDescriptions());
        comboBoxPidFilterMode.SelectedItem = ((PidFilterMode)_tuner.PidFilterMode).GetDescription();
      }

      checkBoxUseCustomTuning.Checked = _tuner.UseCustomTuning;
    }

    private void buttonSave_Click(object sender, EventArgs e)
    {
      textBoxTunerName.Text = textBoxTunerName.Text.Trim();
      if (textBoxTunerName.Text.Length == 0)
      {
        MessageBox.Show("Please enter a name for the tuner.");
        return;
      }

      _tuner.Name = textBoxTunerName.Text;
      _tuner.GrabEPG = checkBoxEpgGrabEnabled.Checked;
      _tuner.PreloadCard = checkBoxPreloadTuner.Checked;

      _tuner.UseConditionalAccess = checkBoxConditionalAccessEnabled.Checked;
      _tuner.DecryptLimit = (int)numericUpDownDecryptLimit.Value;
      _tuner.MultiChannelDecryptMode = Convert.ToInt32(typeof(MultiChannelDecryptMode).GetEnumFromDescription((string)comboBoxMultiChannelDecryptMode.SelectedItem));
      _tuner.CamType = Convert.ToInt32(typeof(CamType).GetEnumFromDescription((string)comboBoxCamType.SelectedItem));

      _tuner.AlwaysSendDiseqcCommands = checkBoxAlwaysSendDiseqcCommands.Checked;
      _tuner.DiseqcCommandRepeatCount = (int)numericUpDownDiseqcCommandRepeatCount.Value;

      _tuner.IdleMode = Convert.ToInt32(typeof(IdleMode).GetEnumFromDescription((string)comboBoxIdleMode.SelectedItem));
      if (comboBoxNetworkProvider.Enabled)
      {
        _tuner.NetProvider = Convert.ToInt32(typeof(DbNetworkProvider).GetEnumFromDescription((string)comboBoxNetworkProvider.SelectedItem));
      }
      if (comboBoxPidFilterMode.Enabled)
      {
        _tuner.PidFilterMode = Convert.ToInt32(typeof(PidFilterMode).GetEnumFromDescription((string)comboBoxPidFilterMode.SelectedItem));
      }
      _tuner.UseCustomTuning = checkBoxUseCustomTuning.Checked;

      _tuner = ServiceAgents.Instance.CardServiceAgent.SaveCard(_tuner);
      Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void SetConditionalAccessFieldAvailability()
    {
      labelDecryptLimit1.Enabled = checkBoxConditionalAccessEnabled.Checked;
      numericUpDownDecryptLimit.Enabled = checkBoxConditionalAccessEnabled.Checked;
      labelDecryptLimit2.Enabled = checkBoxConditionalAccessEnabled.Checked;
      labelMultiChannelDecryptMode.Enabled = checkBoxConditionalAccessEnabled.Checked;
      comboBoxMultiChannelDecryptMode.Enabled = checkBoxConditionalAccessEnabled.Checked;
      labelCamType.Enabled = checkBoxConditionalAccessEnabled.Checked;
      comboBoxCamType.Enabled = checkBoxConditionalAccessEnabled.Checked;
    }

    private void checkBoxConditionalAccessEnabled_CheckedChanged(object sender, EventArgs e)
    {
      SetConditionalAccessFieldAvailability();
    }
  }
}