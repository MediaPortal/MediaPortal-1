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
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditCard : Form
  {
    private Card _card;
    private string _cardType;

    public FormEditCard()
    {
      InitializeComponent();
    }

    public Card Card
    {
      get { return _card; }
      set { _card = value; }
    }

    public String CardType
    {
      set { _cardType = value; }
    }

    private void FormEditCard_Load(object sender, EventArgs e)
    {
      mpTextBoxDeviceName.Text = _card.Name;

      // Analog tuners and capture devices don't have many of these settings.
      bool isAnalogDevice = _cardType.Equals("Analog");
      checkBoxAllowEpgGrab.Enabled = !isAnalogDevice;
      checkBoxConditionalAccessEnabled.Enabled = !isAnalogDevice;
      numericUpDownDecryptLimit.Enabled = !isAnalogDevice;
      mpComboBoxMultiChannelDecryptMode.Enabled = !isAnalogDevice;
      mpComboBoxCamType.Enabled = !isAnalogDevice;
      if (_cardType.Equals("DvbS"))
      {
        checkBoxAlwaysSendDiseqcCommands.Enabled = true;
        numericUpDownDiseqcCommandRepeatCount.Enabled = true;
      }
      else
      {
        checkBoxAlwaysSendDiseqcCommands.Enabled = false;
        numericUpDownDiseqcCommandRepeatCount.Enabled = false;
      }
      mpComboBoxPidFilterMode.Enabled = !isAnalogDevice;
      if (isAnalogDevice)
      {
        checkBoxAllowEpgGrab.Checked = false;
        checkBoxConditionalAccessEnabled.Checked = false;
        numericUpDownDecryptLimit.Value = 0;
        checkBoxAlwaysSendDiseqcCommands.Checked = false;
        numericUpDownDiseqcCommandRepeatCount.Value = 0;
      }
      else
      {
        checkBoxAllowEpgGrab.Checked = _card.GrabEPG;
        checkBoxConditionalAccessEnabled.Checked = _card.UseConditionalAccess;
        numericUpDownDecryptLimit.Value = _card.DecryptLimit;
        mpComboBoxMultiChannelDecryptMode.SelectedItem = ((MultiChannelDecryptMode)_card.MultiChannelDecryptMode).ToString();
        mpComboBoxCamType.SelectedItem = ((CamType)_card.CamType).ToString();
        checkBoxAlwaysSendDiseqcCommands.Checked = _card.AlwaysSendDiseqcCommands;
        numericUpDownDiseqcCommandRepeatCount.Value = _card.DiseqcCommandRepeatCount;
        mpComboBoxPidFilterMode.SelectedItem = ((PidFilterMode)_card.PidFilterMode).ToString();
      }
      mpComboBoxIdleMode.SelectedItem = ((DeviceIdleMode)_card.IdleMode).ToString();
      setConditionalAccessFieldVisibility();

      // Devices can't be preloaded if they're part of a hybrid group.
      IList<CardGroupMap> groupList = _card.CardGroupMaps;
      if (groupList.Count != 0)
      {
        checkBoxPreloadCard.Enabled = false;
        _card.PreloadCard = false;
      }
      checkBoxPreloadCard.Checked = _card.PreloadCard;

      checkBoxUseCustomTuning.Checked = _card.UseCustomTuning;

      if (isAnalogDevice || _cardType.Equals("DvbIP"))
      {
        comboBoxNetworkProvider.Enabled = false;
      }
      else
      {
        comboBoxNetworkProvider.Enabled = true;

        // Add available network providers based on device type and operating system.
        comboBoxNetworkProvider.Items.Add(Enum.Parse(typeof(DbNetworkProvider), _cardType));
        // The generic network provider is available only on XP MCE 2005 + Update Rollup 2,
        // Windows Vista [Home Premium and Ultimate], and Windows 7 [Home Premium, Ultimate,
        // Professional, and Enterprise]
        if (FilterGraphTools.IsThisComObjectInstalled(typeof(NetworkProvider).GUID))
        {
          comboBoxNetworkProvider.Items.Add(DbNetworkProvider.Generic);
        }
        comboBoxNetworkProvider.SelectedItem = (DbNetworkProvider)_card.NetProvider;
      }
    }

    private void mpButtonSave_Click(object sender, EventArgs e)
    {
      if (mpTextBoxDeviceName.Text.Trim().Length == 0)
      {
        MessageBox.Show("Please enter a name for the device.");
        return;
      }

      _card.Name = mpTextBoxDeviceName.Text;
      _card.GrabEPG = checkBoxAllowEpgGrab.Checked;
      _card.UseConditionalAccess = checkBoxConditionalAccessEnabled.Checked;
      _card.DecryptLimit = (int)numericUpDownDecryptLimit.Value;
      _card.AlwaysSendDiseqcCommands = checkBoxAlwaysSendDiseqcCommands.Checked;
      _card.DiseqcCommandRepeatCount = (int)numericUpDownDiseqcCommandRepeatCount.Value;
      _card.IdleMode = (int)Enum.Parse(typeof(DeviceIdleMode), (String)mpComboBoxIdleMode.SelectedItem);

      // Careful here! The selected items will be null for certain device types.
      if (!_cardType.Equals("Analog"))
      {
        _card.MultiChannelDecryptMode = (int)Enum.Parse(typeof(MultiChannelDecryptMode), (String)mpComboBoxMultiChannelDecryptMode.SelectedItem);
        _card.CamType = (int)Enum.Parse(typeof(CamType), (String)mpComboBoxCamType.SelectedItem);
        _card.PidFilterMode = (int)Enum.Parse(typeof(PidFilterMode), (String)mpComboBoxPidFilterMode.SelectedItem);
        if (!_cardType.Equals("DvbIP"))
        {
          _card.NetProvider = (int)(DbNetworkProvider)comboBoxNetworkProvider.SelectedItem;
        }
      }
      _card.PreloadCard = checkBoxPreloadCard.Checked;
      _card.UseCustomTuning = checkBoxUseCustomTuning.Checked;

      Close();
    }

    private void mpButtonCancel_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void setConditionalAccessFieldVisibility()
    {
      label1.Visible = checkBoxConditionalAccessEnabled.Checked;
      label3.Visible = checkBoxConditionalAccessEnabled.Checked;
      numericUpDownDecryptLimit.Visible = checkBoxConditionalAccessEnabled.Checked;
      label4.Visible = checkBoxConditionalAccessEnabled.Checked;
      mpLabel3.Visible = checkBoxConditionalAccessEnabled.Checked;
      mpComboBoxMultiChannelDecryptMode.Visible = checkBoxConditionalAccessEnabled.Checked;
      label5.Visible = checkBoxConditionalAccessEnabled.Checked;
      mpComboBoxCamType.Visible = checkBoxConditionalAccessEnabled.Checked;
    }

    private void checkBoxCAMenabled_CheckedChanged(object sender, EventArgs e)
    {
      setConditionalAccessFieldVisibility();
    }
  }
}