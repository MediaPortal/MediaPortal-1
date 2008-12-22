#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.TV.Database;
using DShowNET;
using DirectShowLib;

using System.Globalization;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for EditTVChannelForm.
  /// </summary>
  public class EditTVChannelForm : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    private MediaPortal.UserInterface.Controls.MPButton cancelButton;
    private MediaPortal.UserInterface.Controls.MPButton okButton;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxGeneralName;
    private MediaPortal.UserInterface.Controls.MPLabel labelGeneralName;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAnalogFrequency;
    private MediaPortal.UserInterface.Controls.MPLabel labelAnalogFrequency;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxExternalChannelNumber;
    private MediaPortal.UserInterface.Controls.MPLabel labelExternalChannelNumber;
    private MediaPortal.UserInterface.Controls.MPLabel labelExternalType;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxExternalType;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxExternalInput;
    private MediaPortal.UserInterface.Controls.MPLabel labelExternalInput;
    private MediaPortal.UserInterface.Controls.MPLabel labelAnalogTvStandard;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxAnalogTvStandard;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControl1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageGeneral;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageAnalog;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageDvbt;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageDvbc;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageDvbs;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageExternal;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxAnalogCountry;
    private MediaPortal.UserInterface.Controls.MPLabel labelAnalogCountry;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtNetworkId;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtServiceId;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtTransportId;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtCarrierFrequency;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtNetworkId;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtServiceId;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtTransportId;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtCarrierFrequency;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcCarrierFrequency;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcCarrierFrequency;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcTransportId;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcTransportId;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcServiceId;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcServiceId;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcNetworkId;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcNetworkId;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcSymbolrate;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcSymbolrate;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcInnerFec;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcModulation;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxDvbcInnerFec;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxDvbcModulation;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsPolarisation;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsInnerFec;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsSymbolrate;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsCarrierFrequency;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsTransportId;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsServiceId;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsNetworkId;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxDvbsPolarisation;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxDvbsInnerFec;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsSymbolrate;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsCarrierFrequency;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsTransportId;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsServiceId;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsNetworkId;
    private int sortPlace = 0;
    private int channelId = -1;
    private MediaPortal.UserInterface.Controls.MPLabel labelGeneralChannel;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtProvider;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtProvider;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcProvider;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcProvider;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsProvider;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsProvider;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtAudioPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtVideoPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtTeletextPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtAudioPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtVideoPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtTeletextPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcTeletextPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcVideoPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcAudioPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcTeletextPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcVideoPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcAudioPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsTeletextPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsVideoPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsAudioPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsTeletextPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsVideoPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsAudioPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsEcmPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsEcmPid;
    private MediaPortal.UserInterface.Controls.MPLabel label40;
    private MediaPortal.UserInterface.Controls.MPLabel label41;
    private MediaPortal.UserInterface.Controls.MPLabel label42;
    private MediaPortal.UserInterface.Controls.MPLabel label43;
    private MediaPortal.UserInterface.Controls.MPLabel label44;
    private MediaPortal.UserInterface.Controls.MPLabel label45;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcPmtPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcPmtPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtPmtPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtPmtPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsPmtPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsPmtPid;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxGeneralEncryptedChannel;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtBandWidth;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtBandWidth;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcAudio1Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcAudio2Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcAudio3Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcAc3Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcAudioLanguage2;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcAudioLanguage1;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcAudioLanguage;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcAudioLanguage3;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcAudio1Pid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcAudioLanguage3;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcAudioLanguage2;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcAudioLanguage1;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcAudioLanguage;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcAc3Pid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcAudio3Pid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcAudio2Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtAudioLanguage3;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtAudioLanguage3;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtAudioLanguage2;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtAudioLanguage1;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtAudioLanguage;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtAudioLanguage2;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtAudioLanguage1;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtAudioLanguage;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtAc3Pid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtAudio3Pid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtAudio2Pid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtAudio1Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtAc3Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtAudio3Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtAudio2Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtAudio1Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsAudioLanguage3;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsAudioLanguage3;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsAudioLanguage2;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsAudioLanguage1;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsAudioLanguage;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsAudioLanguage2;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsAudioLanguage1;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsAudioLanguage;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsAc3Pid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsAudio3Pid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsAudio2Pid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsAudio1Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsAc3Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsAudio3Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsAudio2Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsAudio1Pid;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxGeneralChannel;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;
    private MediaPortal.UserInterface.Controls.MPLabel labelSpecial;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageAtsc;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscAudioLanguage3;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscAudioLanguage2;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscAudioLanguage1;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscAudioLanguage;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscAc3Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscAudio3Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscAudio2Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscAudio1Pid;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscPmtPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscTeletextPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscVideoPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscAudioPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscProvider;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscModulation;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscInnerFec;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscSymbolRate;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscCarrierFrequency;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscTransportId;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscMajorChannel;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscPhysicalChannel;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscMinorChannel;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscPhysicalChannel;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxAtscInnerFec;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscSymbolRate;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscCarrierFrequency;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscTransportId;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscMajorChannel;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscMinorChannel;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscAudioLanguage3;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscAudioLanguage2;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscAudioLanguage1;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscAudioLanguage;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscAc3Pid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscAudio3Pid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscAudio2Pid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscAudio1Pid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscPmtPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscTeletextPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscVideoPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscAudioPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscProvider;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxAtscModulation;
    private MediaPortal.UserInterface.Controls.MPLabel label95;
    private MediaPortal.UserInterface.Controls.MPLabel label96;
    private MediaPortal.UserInterface.Controls.MPLabel label97;
    private MediaPortal.UserInterface.Controls.MPLabel label98;
    private MediaPortal.UserInterface.Controls.MPLabel label99;
    private MediaPortal.UserInterface.Controls.MPLabel label100;
    private MediaPortal.UserInterface.Controls.MPLabel label101;
    private MediaPortal.UserInterface.Controls.MPLabel label102;
    private MediaPortal.UserInterface.Controls.MPLabel label103;
    int orgChannelNumber = -1;
    bool DVBTHasEITPresentFollow, DVBTHasEITSchedule;
    bool DVBCHasEITPresentFollow, DVBCHasEITSchedule;
    bool DVBSHasEITPresentFollow, DVBSHasEITSchedule;
    bool ATSCHasEITPresentFollow, ATSCHasEITSchedule;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbcPcrPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcPcrPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbtPcrPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtPcrPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsPcrPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDvbsPcrPid;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxAtscPcrPid;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbcRedNote;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbtRedNote;
    private MediaPortal.UserInterface.Controls.MPLabel labelDvbsRedNote;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscRedNote;
    private MediaPortal.UserInterface.Controls.MPLabel labelGeneralRedNote;
    private MediaPortal.UserInterface.Controls.MPLabel labelAtscPcrPid;

    public EditTVChannelForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // Set size of window
      //
      comboBoxExternalType.SelectedIndex = 0;
      comboBoxAnalogTvStandard.Text = "Default";
      TunerCountry country = new TunerCountry(-1, "Default", "");
      comboBoxAnalogCountry.Items.Add(country);
      comboBoxAnalogCountry.Items.AddRange(TunerCountries.Countries);
      comboBoxAnalogCountry.Text = "Default";
      comboBoxGeneralChannel.Items.Clear();
      for (int i = 1; i < 255; ++i)
        comboBoxGeneralChannel.Items.Add(i.ToString());
      for (int i = 0; i < TVChannel.SpecialChannels.Length; ++i)
        comboBoxGeneralChannel.Items.Add(TVChannel.SpecialChannels[i].Name);

      comboBoxGeneralChannel.Items.Add("SVHS");
      comboBoxGeneralChannel.Items.Add("RGB");
      comboBoxGeneralChannel.Items.Add("CVBS#1");
      comboBoxGeneralChannel.Items.Add("CVBS#2");

    }

    public int SortingPlace
    {
      get { return sortPlace; }
      set { sortPlace = value; }
    }
    int ParseInt(string label)
    {
      try
      {
        int number = Int32.Parse(label);
        return number;
      }
      catch (Exception)
      {
      }
      return -1;
    }
    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditTVChannelForm));
      this.comboBoxAnalogTvStandard = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelAnalogTvStandard = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxAnalogFrequency = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelAnalogFrequency = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxGeneralName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelGeneralName = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.comboBoxExternalInput = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelExternalInput = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxExternalType = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelExternalType = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxExternalChannelNumber = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelExternalChannelNumber = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageGeneral = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.labelGeneralRedNote = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label97 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label96 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label95 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelSpecial = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxGeneralChannel = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkBoxGeneralEncryptedChannel = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label45 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label44 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelGeneralChannel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPageAnalog = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.label98 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label43 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxAnalogCountry = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelAnalogCountry = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPageDvbt = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.labelDvbtRedNote = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbtPcrPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbtPcrPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label100 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbtAudioLanguage3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbtAudioLanguage3 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbtAudioLanguage2 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbtAudioLanguage1 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbtAudioLanguage = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbtAudioLanguage2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbtAudioLanguage1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbtAudioLanguage = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbtAc3Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbtAudio3Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbtAudio2Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbtAudio1Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbtAc3Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbtAudio3Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbtAudio2Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbtAudio1Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbtBandWidth = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbtBandWidth = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbtPmtPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbtPmtPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbtTeletextPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbtVideoPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbtAudioPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbtTeletextPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbtVideoPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbtAudioPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbtProvider = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbtProvider = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbtCarrierFrequency = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbtCarrierFrequency = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbtTransportId = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbtTransportId = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbtServiceId = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbtServiceId = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbtNetworkId = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbtNetworkId = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label42 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPageDvbc = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.labelDvbcRedNote = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbcPcrPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbcPcrPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label99 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbcAudioLanguage3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbcAudioLanguage3 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbcAudioLanguage2 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbcAudioLanguage1 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbcAudioLanguage = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbcAudioLanguage2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbcAudioLanguage1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbcAudioLanguage = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbcAc3Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbcAudio3Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbcAudio2Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbcAudio1Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbcAc3Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbcAudio3Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbcAudio2Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbcAudio1Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbcPmtPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbcPmtPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label41 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbcTeletextPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbcVideoPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbcAudioPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbcTeletextPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbcVideoPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbcAudioPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbcProvider = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbcProvider = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxDvbcModulation = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBoxDvbcInnerFec = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelDvbcModulation = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbcInnerFec = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbcSymbolrate = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbcSymbolrate = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbcCarrierFrequency = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbcCarrierFrequency = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbcTransportId = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbcTransportId = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbcServiceId = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbcServiceId = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbcNetworkId = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbcNetworkId = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPageDvbs = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.labelDvbsRedNote = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbsPcrPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbsPcrPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label101 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbsAudioLanguage3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbsAudioLanguage3 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbsAudioLanguage2 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbsAudioLanguage1 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbsAudioLanguage = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbsAudioLanguage2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbsAudioLanguage1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbsAudioLanguage = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbsAc3Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbsAudio3Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbsAudio2Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbsAudio1Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbsAc3Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbsAudio3Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbsAudio2Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbsAudio1Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbsPmtPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbsPmtPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label40 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbsEcmPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbsEcmPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbsTeletextPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbsVideoPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxDvbsAudioPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbsTeletextPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbsVideoPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbsAudioPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbsProvider = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbsProvider = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxDvbsPolarisation = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBoxDvbsInnerFec = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelDvbsPolarisation = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDvbsInnerFec = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbsSymbolrate = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbsSymbolrate = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbsCarrierFrequency = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbsCarrierFrequency = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbsTransportId = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbsTransportId = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbsServiceId = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbsServiceId = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxDvbsNetworkId = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelDvbsNetworkId = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPageAtsc = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.labelAtscRedNote = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxAtscPcrPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelAtscPcrPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label102 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxAtscMinorChannel = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelAtscMinorChannel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAtscAudioLanguage3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxAtscAudioLanguage3 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxAtscAudioLanguage2 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxAtscAudioLanguage1 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxAtscAudioLanguage = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelAtscAudioLanguage2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAtscAudioLanguage1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAtscAudioLanguage = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxAtscAc3Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxAtscAudio3Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxAtscAudio2Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxAtscAudio1Pid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelAtscAc3Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAtscAudio3Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAtscAudio2Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAtscAudio1Pid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxAtscPmtPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelAtscPmtPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxAtscTeletextPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxAtscVideoPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxAtscAudioPid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelAtscTeletextPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAtscVideoPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAtscAudioPid = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxAtscProvider = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelAtscProvider = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxAtscModulation = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBoxAtscInnerFec = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelAtscModulation = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAtscInnerFec = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxAtscSymbolRate = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelAtscSymbolRate = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxAtscCarrierFrequency = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelAtscCarrierFrequency = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxAtscTransportId = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelAtscTransportId = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxAtscMajorChannel = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelAtscMajorChannel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxAtscPhysicalChannel = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelAtscPhysicalChannel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPageExternal = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.label103 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControl1.SuspendLayout();
      this.tabPageGeneral.SuspendLayout();
      this.tabPageAnalog.SuspendLayout();
      this.tabPageDvbt.SuspendLayout();
      this.tabPageDvbc.SuspendLayout();
      this.tabPageDvbs.SuspendLayout();
      this.tabPageAtsc.SuspendLayout();
      this.tabPageExternal.SuspendLayout();
      this.SuspendLayout();
      // 
      // comboBoxAnalogTvStandard
      // 
      this.comboBoxAnalogTvStandard.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxAnalogTvStandard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxAnalogTvStandard.Items.AddRange(new object[] {
            "Default",
            "NTSC M",
            "NTSC M J",
            "NTSC 433",
            "PAL B",
            "PAL D",
            "PAL G",
            "PAL H",
            "PAL I",
            "PAL M",
            "PAL N",
            "PAL 60",
            "SECAM B",
            "SECAM D",
            "SECAM G",
            "SECAM H",
            "SECAM K",
            "SECAM K1",
            "SECAM L",
            "SECAM L1",
            "PAL N COMBO"});
      this.comboBoxAnalogTvStandard.Location = new System.Drawing.Point(136, 80);
      this.comboBoxAnalogTvStandard.Name = "comboBoxAnalogTvStandard";
      this.comboBoxAnalogTvStandard.Size = new System.Drawing.Size(224, 21);
      this.comboBoxAnalogTvStandard.TabIndex = 3;
      this.comboBoxAnalogTvStandard.SelectedIndexChanged += new System.EventHandler(this.comboTvStandard_SelectedIndexChanged);
      // 
      // labelAnalogTvStandard
      // 
      this.labelAnalogTvStandard.AutoSize = true;
      this.labelAnalogTvStandard.Location = new System.Drawing.Point(32, 80);
      this.labelAnalogTvStandard.Name = "labelAnalogTvStandard";
      this.labelAnalogTvStandard.Size = new System.Drawing.Size(70, 13);
      this.labelAnalogTvStandard.TabIndex = 11;
      this.labelAnalogTvStandard.Text = "TV Standard:";
      // 
      // textBoxAnalogFrequency
      // 
      this.textBoxAnalogFrequency.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxAnalogFrequency.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAnalogFrequency.Location = new System.Drawing.Point(136, 48);
      this.textBoxAnalogFrequency.MaxLength = 10;
      this.textBoxAnalogFrequency.Name = "textBoxAnalogFrequency";
      this.textBoxAnalogFrequency.Size = new System.Drawing.Size(168, 20);
      this.textBoxAnalogFrequency.TabIndex = 2;
      this.textBoxAnalogFrequency.Text = "0";
      this.textBoxAnalogFrequency.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frequencyTextBox_KeyPress);
      // 
      // labelAnalogFrequency
      // 
      this.labelAnalogFrequency.Location = new System.Drawing.Point(32, 48);
      this.labelAnalogFrequency.Name = "labelAnalogFrequency";
      this.labelAnalogFrequency.Size = new System.Drawing.Size(104, 32);
      this.labelAnalogFrequency.TabIndex = 10;
      this.labelAnalogFrequency.Text = "Frequency (leave 0 for default)";
      // 
      // textBoxGeneralName
      // 
      this.textBoxGeneralName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxGeneralName.BorderColor = System.Drawing.Color.Red;
      this.textBoxGeneralName.Location = new System.Drawing.Point(136, 24);
      this.textBoxGeneralName.Name = "textBoxGeneralName";
      this.textBoxGeneralName.Size = new System.Drawing.Size(264, 20);
      this.textBoxGeneralName.TabIndex = 0;
      // 
      // labelGeneralName
      // 
      this.labelGeneralName.AutoSize = true;
      this.labelGeneralName.Location = new System.Drawing.Point(32, 24);
      this.labelGeneralName.Name = "labelGeneralName";
      this.labelGeneralName.Size = new System.Drawing.Size(35, 13);
      this.labelGeneralName.TabIndex = 6;
      this.labelGeneralName.Text = "Name";
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(421, 431);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(341, 431);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 0;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // comboBoxExternalInput
      // 
      this.comboBoxExternalInput.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxExternalInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxExternalInput.Enabled = false;
      this.comboBoxExternalInput.ImeMode = System.Windows.Forms.ImeMode.NoControl;
      this.comboBoxExternalInput.Items.AddRange(new object[] {
            "CVBS#1",
            "CVBS#2",
            "SVHS",
            "RGB"});
      this.comboBoxExternalInput.Location = new System.Drawing.Point(128, 56);
      this.comboBoxExternalInput.Name = "comboBoxExternalInput";
      this.comboBoxExternalInput.Size = new System.Drawing.Size(224, 21);
      this.comboBoxExternalInput.TabIndex = 1;
      // 
      // labelExternalInput
      // 
      this.labelExternalInput.AutoSize = true;
      this.labelExternalInput.Location = new System.Drawing.Point(24, 56);
      this.labelExternalInput.Name = "labelExternalInput";
      this.labelExternalInput.Size = new System.Drawing.Size(48, 13);
      this.labelExternalInput.TabIndex = 12;
      this.labelExternalInput.Text = "Input via";
      // 
      // comboBoxExternalType
      // 
      this.comboBoxExternalType.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxExternalType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxExternalType.Items.AddRange(new object[] {
            "Received by tv card",
            "Received by external settop box"});
      this.comboBoxExternalType.Location = new System.Drawing.Point(128, 24);
      this.comboBoxExternalType.Name = "comboBoxExternalType";
      this.comboBoxExternalType.Size = new System.Drawing.Size(224, 21);
      this.comboBoxExternalType.TabIndex = 0;
      this.comboBoxExternalType.SelectedIndexChanged += new System.EventHandler(this.typeComboBox_SelectedIndexChanged);
      // 
      // labelExternalType
      // 
      this.labelExternalType.AutoSize = true;
      this.labelExternalType.Location = new System.Drawing.Point(24, 32);
      this.labelExternalType.Name = "labelExternalType";
      this.labelExternalType.Size = new System.Drawing.Size(31, 13);
      this.labelExternalType.TabIndex = 10;
      this.labelExternalType.Text = "Type";
      // 
      // textBoxExternalChannelNumber
      // 
      this.textBoxExternalChannelNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxExternalChannelNumber.BorderColor = System.Drawing.Color.Empty;
      this.textBoxExternalChannelNumber.Enabled = false;
      this.textBoxExternalChannelNumber.Location = new System.Drawing.Point(128, 88);
      this.textBoxExternalChannelNumber.Name = "textBoxExternalChannelNumber";
      this.textBoxExternalChannelNumber.Size = new System.Drawing.Size(272, 20);
      this.textBoxExternalChannelNumber.TabIndex = 2;
      // 
      // labelExternalChannelNumber
      // 
      this.labelExternalChannelNumber.Location = new System.Drawing.Point(24, 88);
      this.labelExternalChannelNumber.Name = "labelExternalChannelNumber";
      this.labelExternalChannelNumber.Size = new System.Drawing.Size(88, 32);
      this.labelExternalChannelNumber.TabIndex = 8;
      this.labelExternalChannelNumber.Text = "channel number on settopbox";
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPageGeneral);
      this.tabControl1.Controls.Add(this.tabPageAnalog);
      this.tabControl1.Controls.Add(this.tabPageDvbt);
      this.tabControl1.Controls.Add(this.tabPageDvbc);
      this.tabControl1.Controls.Add(this.tabPageDvbs);
      this.tabControl1.Controls.Add(this.tabPageAtsc);
      this.tabControl1.Controls.Add(this.tabPageExternal);
      this.tabControl1.Location = new System.Drawing.Point(8, 8);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(488, 408);
      this.tabControl1.TabIndex = 4;
      // 
      // tabPageGeneral
      // 
      this.tabPageGeneral.Controls.Add(this.labelGeneralRedNote);
      this.tabPageGeneral.Controls.Add(this.label97);
      this.tabPageGeneral.Controls.Add(this.label96);
      this.tabPageGeneral.Controls.Add(this.label95);
      this.tabPageGeneral.Controls.Add(this.labelSpecial);
      this.tabPageGeneral.Controls.Add(this.comboBoxGeneralChannel);
      this.tabPageGeneral.Controls.Add(this.checkBoxGeneralEncryptedChannel);
      this.tabPageGeneral.Controls.Add(this.label45);
      this.tabPageGeneral.Controls.Add(this.label44);
      this.tabPageGeneral.Controls.Add(this.labelGeneralChannel);
      this.tabPageGeneral.Controls.Add(this.labelGeneralName);
      this.tabPageGeneral.Controls.Add(this.textBoxGeneralName);
      this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
      this.tabPageGeneral.Name = "tabPageGeneral";
      this.tabPageGeneral.Size = new System.Drawing.Size(480, 382);
      this.tabPageGeneral.TabIndex = 0;
      this.tabPageGeneral.Text = "General";
      this.tabPageGeneral.UseVisualStyleBackColor = true;
      // 
      // labelGeneralRedNote
      // 
      this.labelGeneralRedNote.AutoSize = true;
      this.labelGeneralRedNote.ForeColor = System.Drawing.Color.Red;
      this.labelGeneralRedNote.Location = new System.Drawing.Point(296, 352);
      this.labelGeneralRedNote.Name = "labelGeneralRedNote";
      this.labelGeneralRedNote.Size = new System.Drawing.Size(171, 13);
      this.labelGeneralRedNote.TabIndex = 19;
      this.labelGeneralRedNote.Text = "Red fields denote  required values.";
      // 
      // label97
      // 
      this.label97.AutoSize = true;
      this.label97.Location = new System.Drawing.Point(40, 272);
      this.label97.Name = "label97";
      this.label97.Size = new System.Drawing.Size(229, 13);
      this.label97.TabIndex = 18;
      this.label97.Text = "For digital TV, the channel number is not used. ";
      // 
      // label96
      // 
      this.label96.Location = new System.Drawing.Point(40, 184);
      this.label96.Name = "label96";
      this.label96.Size = new System.Drawing.Size(328, 64);
      this.label96.TabIndex = 17;
      this.label96.Text = resources.GetString("label96.Text");
      // 
      // label95
      // 
      this.label95.AutoSize = true;
      this.label95.Location = new System.Drawing.Point(24, 256);
      this.label95.Name = "label95";
      this.label95.Size = new System.Drawing.Size(56, 13);
      this.label95.TabIndex = 16;
      this.label95.Text = "Digital TV:";
      // 
      // labelSpecial
      // 
      this.labelSpecial.Location = new System.Drawing.Point(272, 64);
      this.labelSpecial.Name = "labelSpecial";
      this.labelSpecial.Size = new System.Drawing.Size(64, 16);
      this.labelSpecial.TabIndex = 15;
      // 
      // comboBoxGeneralChannel
      // 
      this.comboBoxGeneralChannel.BorderColor = System.Drawing.Color.Red;
      this.comboBoxGeneralChannel.Location = new System.Drawing.Point(136, 56);
      this.comboBoxGeneralChannel.Name = "comboBoxGeneralChannel";
      this.comboBoxGeneralChannel.Size = new System.Drawing.Size(121, 21);
      this.comboBoxGeneralChannel.TabIndex = 1;
      this.comboBoxGeneralChannel.SelectedIndexChanged += new System.EventHandler(this.comboBoxChannels_SelectedIndexChanged);
      // 
      // checkBoxGeneralEncryptedChannel
      // 
      this.checkBoxGeneralEncryptedChannel.AutoSize = true;
      this.checkBoxGeneralEncryptedChannel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxGeneralEncryptedChannel.Location = new System.Drawing.Point(32, 88);
      this.checkBoxGeneralEncryptedChannel.Name = "checkBoxGeneralEncryptedChannel";
      this.checkBoxGeneralEncryptedChannel.Size = new System.Drawing.Size(214, 17);
      this.checkBoxGeneralEncryptedChannel.TabIndex = 2;
      this.checkBoxGeneralEncryptedChannel.Text = "TV channel is encrypted (digital TV only)";
      this.checkBoxGeneralEncryptedChannel.UseVisualStyleBackColor = true;
      // 
      // label45
      // 
      this.label45.AutoSize = true;
      this.label45.Location = new System.Drawing.Point(24, 168);
      this.label45.Name = "label45";
      this.label45.Size = new System.Drawing.Size(60, 13);
      this.label45.TabIndex = 12;
      this.label45.Text = "Analog TV:";
      // 
      // label44
      // 
      this.label44.Location = new System.Drawing.Point(24, 128);
      this.label44.Name = "label44";
      this.label44.Size = new System.Drawing.Size(344, 32);
      this.label44.TabIndex = 11;
      this.label44.Text = "Enter the name of this TV Channel. If you\'re using the TV guide then make sure it" +
          " matches the channel name from the TV guide.";
      // 
      // labelGeneralChannel
      // 
      this.labelGeneralChannel.AutoSize = true;
      this.labelGeneralChannel.Location = new System.Drawing.Point(32, 56);
      this.labelGeneralChannel.Name = "labelGeneralChannel";
      this.labelGeneralChannel.Size = new System.Drawing.Size(46, 13);
      this.labelGeneralChannel.TabIndex = 10;
      this.labelGeneralChannel.Text = "Channel";
      // 
      // tabPageAnalog
      // 
      this.tabPageAnalog.Controls.Add(this.label98);
      this.tabPageAnalog.Controls.Add(this.label43);
      this.tabPageAnalog.Controls.Add(this.comboBoxAnalogCountry);
      this.tabPageAnalog.Controls.Add(this.labelAnalogCountry);
      this.tabPageAnalog.Controls.Add(this.labelAnalogFrequency);
      this.tabPageAnalog.Controls.Add(this.textBoxAnalogFrequency);
      this.tabPageAnalog.Controls.Add(this.labelAnalogTvStandard);
      this.tabPageAnalog.Controls.Add(this.comboBoxAnalogTvStandard);
      this.tabPageAnalog.Location = new System.Drawing.Point(4, 22);
      this.tabPageAnalog.Name = "tabPageAnalog";
      this.tabPageAnalog.Size = new System.Drawing.Size(480, 382);
      this.tabPageAnalog.TabIndex = 1;
      this.tabPageAnalog.Text = "Analog";
      this.tabPageAnalog.UseVisualStyleBackColor = true;
      // 
      // label98
      // 
      this.label98.AutoSize = true;
      this.label98.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label98.Location = new System.Drawing.Point(32, 16);
      this.label98.Name = "label98";
      this.label98.Size = new System.Drawing.Size(114, 13);
      this.label98.TabIndex = 18;
      this.label98.Text = "Analog TV settings";
      // 
      // label43
      // 
      this.label43.Location = new System.Drawing.Point(32, 152);
      this.label43.Name = "label43";
      this.label43.Size = new System.Drawing.Size(344, 72);
      this.label43.TabIndex = 17;
      this.label43.Text = resources.GetString("label43.Text");
      // 
      // comboBoxAnalogCountry
      // 
      this.comboBoxAnalogCountry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxAnalogCountry.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxAnalogCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxAnalogCountry.Location = new System.Drawing.Point(136, 112);
      this.comboBoxAnalogCountry.MaxDropDownItems = 16;
      this.comboBoxAnalogCountry.Name = "comboBoxAnalogCountry";
      this.comboBoxAnalogCountry.Size = new System.Drawing.Size(312, 21);
      this.comboBoxAnalogCountry.Sorted = true;
      this.comboBoxAnalogCountry.TabIndex = 15;
      // 
      // labelAnalogCountry
      // 
      this.labelAnalogCountry.AutoSize = true;
      this.labelAnalogCountry.Location = new System.Drawing.Point(32, 112);
      this.labelAnalogCountry.Name = "labelAnalogCountry";
      this.labelAnalogCountry.Size = new System.Drawing.Size(43, 13);
      this.labelAnalogCountry.TabIndex = 16;
      this.labelAnalogCountry.Text = "Country";
      // 
      // tabPageDvbt
      // 
      this.tabPageDvbt.Controls.Add(this.labelDvbtRedNote);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtPcrPid);
      this.tabPageDvbt.Controls.Add(this.labelDvbtPcrPid);
      this.tabPageDvbt.Controls.Add(this.label100);
      this.tabPageDvbt.Controls.Add(this.labelDvbtAudioLanguage3);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtAudioLanguage3);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtAudioLanguage2);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtAudioLanguage1);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtAudioLanguage);
      this.tabPageDvbt.Controls.Add(this.labelDvbtAudioLanguage2);
      this.tabPageDvbt.Controls.Add(this.labelDvbtAudioLanguage1);
      this.tabPageDvbt.Controls.Add(this.labelDvbtAudioLanguage);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtAc3Pid);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtAudio3Pid);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtAudio2Pid);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtAudio1Pid);
      this.tabPageDvbt.Controls.Add(this.labelDvbtAc3Pid);
      this.tabPageDvbt.Controls.Add(this.labelDvbtAudio3Pid);
      this.tabPageDvbt.Controls.Add(this.labelDvbtAudio2Pid);
      this.tabPageDvbt.Controls.Add(this.labelDvbtAudio1Pid);
      this.tabPageDvbt.Controls.Add(this.labelDvbtBandWidth);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtBandWidth);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtPmtPid);
      this.tabPageDvbt.Controls.Add(this.labelDvbtPmtPid);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtTeletextPid);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtVideoPid);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtAudioPid);
      this.tabPageDvbt.Controls.Add(this.labelDvbtTeletextPid);
      this.tabPageDvbt.Controls.Add(this.labelDvbtVideoPid);
      this.tabPageDvbt.Controls.Add(this.labelDvbtAudioPid);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtProvider);
      this.tabPageDvbt.Controls.Add(this.labelDvbtProvider);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtCarrierFrequency);
      this.tabPageDvbt.Controls.Add(this.labelDvbtCarrierFrequency);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtTransportId);
      this.tabPageDvbt.Controls.Add(this.labelDvbtTransportId);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtServiceId);
      this.tabPageDvbt.Controls.Add(this.labelDvbtServiceId);
      this.tabPageDvbt.Controls.Add(this.textBoxDvbtNetworkId);
      this.tabPageDvbt.Controls.Add(this.labelDvbtNetworkId);
      this.tabPageDvbt.Controls.Add(this.label42);
      this.tabPageDvbt.Location = new System.Drawing.Point(4, 22);
      this.tabPageDvbt.Name = "tabPageDvbt";
      this.tabPageDvbt.Size = new System.Drawing.Size(480, 382);
      this.tabPageDvbt.TabIndex = 2;
      this.tabPageDvbt.Text = "DVB-T";
      this.tabPageDvbt.UseVisualStyleBackColor = true;
      // 
      // labelDvbtRedNote
      // 
      this.labelDvbtRedNote.AutoSize = true;
      this.labelDvbtRedNote.ForeColor = System.Drawing.Color.Red;
      this.labelDvbtRedNote.Location = new System.Drawing.Point(296, 352);
      this.labelDvbtRedNote.Name = "labelDvbtRedNote";
      this.labelDvbtRedNote.Size = new System.Drawing.Size(171, 13);
      this.labelDvbtRedNote.TabIndex = 68;
      this.labelDvbtRedNote.Text = "Red fields denote  required values.";
      // 
      // textBoxDvbtPcrPid
      // 
      this.textBoxDvbtPcrPid.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbtPcrPid.Location = new System.Drawing.Point(360, 104);
      this.textBoxDvbtPcrPid.Name = "textBoxDvbtPcrPid";
      this.textBoxDvbtPcrPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtPcrPid.TabIndex = 67;
      // 
      // labelDvbtPcrPid
      // 
      this.labelDvbtPcrPid.AutoSize = true;
      this.labelDvbtPcrPid.Location = new System.Drawing.Point(288, 104);
      this.labelDvbtPcrPid.Name = "labelDvbtPcrPid";
      this.labelDvbtPcrPid.Size = new System.Drawing.Size(50, 13);
      this.labelDvbtPcrPid.TabIndex = 66;
      this.labelDvbtPcrPid.Text = "PCR PID";
      // 
      // label100
      // 
      this.label100.AutoSize = true;
      this.label100.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label100.Location = new System.Drawing.Point(32, 16);
      this.label100.Name = "label100";
      this.label100.Size = new System.Drawing.Size(168, 13);
      this.label100.TabIndex = 65;
      this.label100.Text = "Digital terrestrial TV settings";
      // 
      // labelDvbtAudioLanguage3
      // 
      this.labelDvbtAudioLanguage3.AutoSize = true;
      this.labelDvbtAudioLanguage3.Location = new System.Drawing.Point(288, 312);
      this.labelDvbtAudioLanguage3.Name = "labelDvbtAudioLanguage3";
      this.labelDvbtAudioLanguage3.Size = new System.Drawing.Size(43, 13);
      this.labelDvbtAudioLanguage3.TabIndex = 64;
      this.labelDvbtAudioLanguage3.Text = "Audio 3";
      // 
      // textBoxDvbtAudioLanguage3
      // 
      this.textBoxDvbtAudioLanguage3.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbtAudioLanguage3.Location = new System.Drawing.Point(360, 312);
      this.textBoxDvbtAudioLanguage3.Name = "textBoxDvbtAudioLanguage3";
      this.textBoxDvbtAudioLanguage3.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtAudioLanguage3.TabIndex = 63;
      // 
      // textBoxDvbtAudioLanguage2
      // 
      this.textBoxDvbtAudioLanguage2.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbtAudioLanguage2.Location = new System.Drawing.Point(360, 288);
      this.textBoxDvbtAudioLanguage2.Name = "textBoxDvbtAudioLanguage2";
      this.textBoxDvbtAudioLanguage2.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtAudioLanguage2.TabIndex = 62;
      // 
      // textBoxDvbtAudioLanguage1
      // 
      this.textBoxDvbtAudioLanguage1.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbtAudioLanguage1.Location = new System.Drawing.Point(360, 264);
      this.textBoxDvbtAudioLanguage1.Name = "textBoxDvbtAudioLanguage1";
      this.textBoxDvbtAudioLanguage1.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtAudioLanguage1.TabIndex = 61;
      // 
      // textBoxDvbtAudioLanguage
      // 
      this.textBoxDvbtAudioLanguage.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbtAudioLanguage.Location = new System.Drawing.Point(360, 240);
      this.textBoxDvbtAudioLanguage.Name = "textBoxDvbtAudioLanguage";
      this.textBoxDvbtAudioLanguage.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtAudioLanguage.TabIndex = 60;
      // 
      // labelDvbtAudioLanguage2
      // 
      this.labelDvbtAudioLanguage2.AutoSize = true;
      this.labelDvbtAudioLanguage2.Location = new System.Drawing.Point(288, 288);
      this.labelDvbtAudioLanguage2.Name = "labelDvbtAudioLanguage2";
      this.labelDvbtAudioLanguage2.Size = new System.Drawing.Size(43, 13);
      this.labelDvbtAudioLanguage2.TabIndex = 59;
      this.labelDvbtAudioLanguage2.Text = "Audio 2";
      // 
      // labelDvbtAudioLanguage1
      // 
      this.labelDvbtAudioLanguage1.AutoSize = true;
      this.labelDvbtAudioLanguage1.Location = new System.Drawing.Point(288, 264);
      this.labelDvbtAudioLanguage1.Name = "labelDvbtAudioLanguage1";
      this.labelDvbtAudioLanguage1.Size = new System.Drawing.Size(43, 13);
      this.labelDvbtAudioLanguage1.TabIndex = 58;
      this.labelDvbtAudioLanguage1.Text = "Audio 1";
      // 
      // labelDvbtAudioLanguage
      // 
      this.labelDvbtAudioLanguage.AutoSize = true;
      this.labelDvbtAudioLanguage.Location = new System.Drawing.Point(288, 240);
      this.labelDvbtAudioLanguage.Name = "labelDvbtAudioLanguage";
      this.labelDvbtAudioLanguage.Size = new System.Drawing.Size(34, 13);
      this.labelDvbtAudioLanguage.TabIndex = 57;
      this.labelDvbtAudioLanguage.Text = "Audio";
      // 
      // textBoxDvbtAc3Pid
      // 
      this.textBoxDvbtAc3Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbtAc3Pid.Location = new System.Drawing.Point(360, 208);
      this.textBoxDvbtAc3Pid.Name = "textBoxDvbtAc3Pid";
      this.textBoxDvbtAc3Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtAc3Pid.TabIndex = 56;
      // 
      // textBoxDvbtAudio3Pid
      // 
      this.textBoxDvbtAudio3Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbtAudio3Pid.Location = new System.Drawing.Point(360, 184);
      this.textBoxDvbtAudio3Pid.Name = "textBoxDvbtAudio3Pid";
      this.textBoxDvbtAudio3Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtAudio3Pid.TabIndex = 55;
      // 
      // textBoxDvbtAudio2Pid
      // 
      this.textBoxDvbtAudio2Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbtAudio2Pid.Location = new System.Drawing.Point(360, 160);
      this.textBoxDvbtAudio2Pid.Name = "textBoxDvbtAudio2Pid";
      this.textBoxDvbtAudio2Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtAudio2Pid.TabIndex = 54;
      // 
      // textBoxDvbtAudio1Pid
      // 
      this.textBoxDvbtAudio1Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbtAudio1Pid.Location = new System.Drawing.Point(360, 136);
      this.textBoxDvbtAudio1Pid.Name = "textBoxDvbtAudio1Pid";
      this.textBoxDvbtAudio1Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtAudio1Pid.TabIndex = 53;
      // 
      // labelDvbtAc3Pid
      // 
      this.labelDvbtAc3Pid.AutoSize = true;
      this.labelDvbtAc3Pid.Location = new System.Drawing.Point(288, 208);
      this.labelDvbtAc3Pid.Name = "labelDvbtAc3Pid";
      this.labelDvbtAc3Pid.Size = new System.Drawing.Size(48, 13);
      this.labelDvbtAc3Pid.TabIndex = 52;
      this.labelDvbtAc3Pid.Text = "AC3 PID";
      // 
      // labelDvbtAudio3Pid
      // 
      this.labelDvbtAudio3Pid.AutoSize = true;
      this.labelDvbtAudio3Pid.Location = new System.Drawing.Point(288, 184);
      this.labelDvbtAudio3Pid.Name = "labelDvbtAudio3Pid";
      this.labelDvbtAudio3Pid.Size = new System.Drawing.Size(64, 13);
      this.labelDvbtAudio3Pid.TabIndex = 51;
      this.labelDvbtAudio3Pid.Text = "Audio 3 PID";
      // 
      // labelDvbtAudio2Pid
      // 
      this.labelDvbtAudio2Pid.AutoSize = true;
      this.labelDvbtAudio2Pid.Location = new System.Drawing.Point(288, 160);
      this.labelDvbtAudio2Pid.Name = "labelDvbtAudio2Pid";
      this.labelDvbtAudio2Pid.Size = new System.Drawing.Size(64, 13);
      this.labelDvbtAudio2Pid.TabIndex = 50;
      this.labelDvbtAudio2Pid.Text = "Audio 2 PID";
      // 
      // labelDvbtAudio1Pid
      // 
      this.labelDvbtAudio1Pid.AutoSize = true;
      this.labelDvbtAudio1Pid.Location = new System.Drawing.Point(288, 136);
      this.labelDvbtAudio1Pid.Name = "labelDvbtAudio1Pid";
      this.labelDvbtAudio1Pid.Size = new System.Drawing.Size(64, 13);
      this.labelDvbtAudio1Pid.TabIndex = 49;
      this.labelDvbtAudio1Pid.Text = "Audio 1 PID";
      // 
      // labelDvbtBandWidth
      // 
      this.labelDvbtBandWidth.AutoSize = true;
      this.labelDvbtBandWidth.Location = new System.Drawing.Point(32, 256);
      this.labelDvbtBandWidth.Name = "labelDvbtBandWidth";
      this.labelDvbtBandWidth.Size = new System.Drawing.Size(57, 13);
      this.labelDvbtBandWidth.TabIndex = 36;
      this.labelDvbtBandWidth.Text = "Bandwidth";
      // 
      // textBoxDvbtBandWidth
      // 
      this.textBoxDvbtBandWidth.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbtBandWidth.Location = new System.Drawing.Point(168, 256);
      this.textBoxDvbtBandWidth.Name = "textBoxDvbtBandWidth";
      this.textBoxDvbtBandWidth.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtBandWidth.TabIndex = 35;
      // 
      // textBoxDvbtPmtPid
      // 
      this.textBoxDvbtPmtPid.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbtPmtPid.Location = new System.Drawing.Point(168, 232);
      this.textBoxDvbtPmtPid.Name = "textBoxDvbtPmtPid";
      this.textBoxDvbtPmtPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtPmtPid.TabIndex = 34;
      // 
      // labelDvbtPmtPid
      // 
      this.labelDvbtPmtPid.AutoSize = true;
      this.labelDvbtPmtPid.Location = new System.Drawing.Point(32, 232);
      this.labelDvbtPmtPid.Name = "labelDvbtPmtPid";
      this.labelDvbtPmtPid.Size = new System.Drawing.Size(51, 13);
      this.labelDvbtPmtPid.TabIndex = 33;
      this.labelDvbtPmtPid.Text = "PMT PID";
      // 
      // textBoxDvbtTeletextPid
      // 
      this.textBoxDvbtTeletextPid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbtTeletextPid.Location = new System.Drawing.Point(168, 208);
      this.textBoxDvbtTeletextPid.Name = "textBoxDvbtTeletextPid";
      this.textBoxDvbtTeletextPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtTeletextPid.TabIndex = 15;
      // 
      // textBoxDvbtVideoPid
      // 
      this.textBoxDvbtVideoPid.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbtVideoPid.Location = new System.Drawing.Point(168, 184);
      this.textBoxDvbtVideoPid.Name = "textBoxDvbtVideoPid";
      this.textBoxDvbtVideoPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtVideoPid.TabIndex = 14;
      // 
      // textBoxDvbtAudioPid
      // 
      this.textBoxDvbtAudioPid.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbtAudioPid.Location = new System.Drawing.Point(168, 160);
      this.textBoxDvbtAudioPid.Name = "textBoxDvbtAudioPid";
      this.textBoxDvbtAudioPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtAudioPid.TabIndex = 13;
      // 
      // labelDvbtTeletextPid
      // 
      this.labelDvbtTeletextPid.AutoSize = true;
      this.labelDvbtTeletextPid.Location = new System.Drawing.Point(32, 208);
      this.labelDvbtTeletextPid.Name = "labelDvbtTeletextPid";
      this.labelDvbtTeletextPid.Size = new System.Drawing.Size(66, 13);
      this.labelDvbtTeletextPid.TabIndex = 12;
      this.labelDvbtTeletextPid.Text = "Teletext PID";
      // 
      // labelDvbtVideoPid
      // 
      this.labelDvbtVideoPid.AutoSize = true;
      this.labelDvbtVideoPid.Location = new System.Drawing.Point(32, 184);
      this.labelDvbtVideoPid.Name = "labelDvbtVideoPid";
      this.labelDvbtVideoPid.Size = new System.Drawing.Size(55, 13);
      this.labelDvbtVideoPid.TabIndex = 11;
      this.labelDvbtVideoPid.Text = "Video PID";
      // 
      // labelDvbtAudioPid
      // 
      this.labelDvbtAudioPid.AutoSize = true;
      this.labelDvbtAudioPid.Location = new System.Drawing.Point(32, 160);
      this.labelDvbtAudioPid.Name = "labelDvbtAudioPid";
      this.labelDvbtAudioPid.Size = new System.Drawing.Size(55, 13);
      this.labelDvbtAudioPid.TabIndex = 10;
      this.labelDvbtAudioPid.Text = "Audio PID";
      // 
      // textBoxDvbtProvider
      // 
      this.textBoxDvbtProvider.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbtProvider.Location = new System.Drawing.Point(168, 136);
      this.textBoxDvbtProvider.Name = "textBoxDvbtProvider";
      this.textBoxDvbtProvider.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtProvider.TabIndex = 9;
      // 
      // labelDvbtProvider
      // 
      this.labelDvbtProvider.AutoSize = true;
      this.labelDvbtProvider.Location = new System.Drawing.Point(32, 136);
      this.labelDvbtProvider.Name = "labelDvbtProvider";
      this.labelDvbtProvider.Size = new System.Drawing.Size(46, 13);
      this.labelDvbtProvider.TabIndex = 8;
      this.labelDvbtProvider.Text = "Provider";
      // 
      // textBoxDvbtCarrierFrequency
      // 
      this.textBoxDvbtCarrierFrequency.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbtCarrierFrequency.Location = new System.Drawing.Point(168, 112);
      this.textBoxDvbtCarrierFrequency.Name = "textBoxDvbtCarrierFrequency";
      this.textBoxDvbtCarrierFrequency.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtCarrierFrequency.TabIndex = 7;
      // 
      // labelDvbtCarrierFrequency
      // 
      this.labelDvbtCarrierFrequency.AutoSize = true;
      this.labelDvbtCarrierFrequency.Location = new System.Drawing.Point(32, 112);
      this.labelDvbtCarrierFrequency.Name = "labelDvbtCarrierFrequency";
      this.labelDvbtCarrierFrequency.Size = new System.Drawing.Size(118, 13);
      this.labelDvbtCarrierFrequency.TabIndex = 6;
      this.labelDvbtCarrierFrequency.Text = "Carrier Frequency (kHz)";
      // 
      // textBoxDvbtTransportId
      // 
      this.textBoxDvbtTransportId.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbtTransportId.Location = new System.Drawing.Point(168, 88);
      this.textBoxDvbtTransportId.Name = "textBoxDvbtTransportId";
      this.textBoxDvbtTransportId.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtTransportId.TabIndex = 5;
      // 
      // labelDvbtTransportId
      // 
      this.labelDvbtTransportId.AutoSize = true;
      this.labelDvbtTransportId.Location = new System.Drawing.Point(32, 88);
      this.labelDvbtTransportId.Name = "labelDvbtTransportId";
      this.labelDvbtTransportId.Size = new System.Drawing.Size(66, 13);
      this.labelDvbtTransportId.TabIndex = 4;
      this.labelDvbtTransportId.Text = "Transport ID";
      // 
      // textBoxDvbtServiceId
      // 
      this.textBoxDvbtServiceId.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbtServiceId.Location = new System.Drawing.Point(168, 64);
      this.textBoxDvbtServiceId.Name = "textBoxDvbtServiceId";
      this.textBoxDvbtServiceId.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtServiceId.TabIndex = 3;
      // 
      // labelDvbtServiceId
      // 
      this.labelDvbtServiceId.AutoSize = true;
      this.labelDvbtServiceId.Location = new System.Drawing.Point(32, 64);
      this.labelDvbtServiceId.Name = "labelDvbtServiceId";
      this.labelDvbtServiceId.Size = new System.Drawing.Size(57, 13);
      this.labelDvbtServiceId.TabIndex = 2;
      this.labelDvbtServiceId.Text = "Service ID";
      // 
      // textBoxDvbtNetworkId
      // 
      this.textBoxDvbtNetworkId.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbtNetworkId.Location = new System.Drawing.Point(168, 40);
      this.textBoxDvbtNetworkId.Name = "textBoxDvbtNetworkId";
      this.textBoxDvbtNetworkId.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbtNetworkId.TabIndex = 1;
      // 
      // labelDvbtNetworkId
      // 
      this.labelDvbtNetworkId.AutoSize = true;
      this.labelDvbtNetworkId.Location = new System.Drawing.Point(32, 40);
      this.labelDvbtNetworkId.Name = "labelDvbtNetworkId";
      this.labelDvbtNetworkId.Size = new System.Drawing.Size(61, 13);
      this.labelDvbtNetworkId.TabIndex = 0;
      this.labelDvbtNetworkId.Text = "Network ID";
      // 
      // label42
      // 
      this.label42.Location = new System.Drawing.Point(288, 16);
      this.label42.Name = "label42";
      this.label42.Size = new System.Drawing.Size(152, 80);
      this.label42.TabIndex = 16;
      this.label42.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // tabPageDvbc
      // 
      this.tabPageDvbc.Controls.Add(this.labelDvbcRedNote);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcPcrPid);
      this.tabPageDvbc.Controls.Add(this.labelDvbcPcrPid);
      this.tabPageDvbc.Controls.Add(this.label99);
      this.tabPageDvbc.Controls.Add(this.labelDvbcAudioLanguage3);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcAudioLanguage3);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcAudioLanguage2);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcAudioLanguage1);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcAudioLanguage);
      this.tabPageDvbc.Controls.Add(this.labelDvbcAudioLanguage2);
      this.tabPageDvbc.Controls.Add(this.labelDvbcAudioLanguage1);
      this.tabPageDvbc.Controls.Add(this.labelDvbcAudioLanguage);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcAc3Pid);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcAudio3Pid);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcAudio2Pid);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcAudio1Pid);
      this.tabPageDvbc.Controls.Add(this.labelDvbcAc3Pid);
      this.tabPageDvbc.Controls.Add(this.labelDvbcAudio3Pid);
      this.tabPageDvbc.Controls.Add(this.labelDvbcAudio2Pid);
      this.tabPageDvbc.Controls.Add(this.labelDvbcAudio1Pid);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcPmtPid);
      this.tabPageDvbc.Controls.Add(this.labelDvbcPmtPid);
      this.tabPageDvbc.Controls.Add(this.label41);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcTeletextPid);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcVideoPid);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcAudioPid);
      this.tabPageDvbc.Controls.Add(this.labelDvbcTeletextPid);
      this.tabPageDvbc.Controls.Add(this.labelDvbcVideoPid);
      this.tabPageDvbc.Controls.Add(this.labelDvbcAudioPid);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcProvider);
      this.tabPageDvbc.Controls.Add(this.labelDvbcProvider);
      this.tabPageDvbc.Controls.Add(this.comboBoxDvbcModulation);
      this.tabPageDvbc.Controls.Add(this.comboBoxDvbcInnerFec);
      this.tabPageDvbc.Controls.Add(this.labelDvbcModulation);
      this.tabPageDvbc.Controls.Add(this.labelDvbcInnerFec);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcSymbolrate);
      this.tabPageDvbc.Controls.Add(this.labelDvbcSymbolrate);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcCarrierFrequency);
      this.tabPageDvbc.Controls.Add(this.labelDvbcCarrierFrequency);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcTransportId);
      this.tabPageDvbc.Controls.Add(this.labelDvbcTransportId);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcServiceId);
      this.tabPageDvbc.Controls.Add(this.labelDvbcServiceId);
      this.tabPageDvbc.Controls.Add(this.textBoxDvbcNetworkId);
      this.tabPageDvbc.Controls.Add(this.labelDvbcNetworkId);
      this.tabPageDvbc.Location = new System.Drawing.Point(4, 22);
      this.tabPageDvbc.Name = "tabPageDvbc";
      this.tabPageDvbc.Size = new System.Drawing.Size(480, 382);
      this.tabPageDvbc.TabIndex = 3;
      this.tabPageDvbc.Text = "DVB-C";
      this.tabPageDvbc.UseVisualStyleBackColor = true;
      // 
      // labelDvbcRedNote
      // 
      this.labelDvbcRedNote.AutoSize = true;
      this.labelDvbcRedNote.ForeColor = System.Drawing.Color.Red;
      this.labelDvbcRedNote.Location = new System.Drawing.Point(296, 352);
      this.labelDvbcRedNote.Name = "labelDvbcRedNote";
      this.labelDvbcRedNote.Size = new System.Drawing.Size(171, 13);
      this.labelDvbcRedNote.TabIndex = 52;
      this.labelDvbcRedNote.Text = "Red fields denote  required values.";
      // 
      // textBoxDvbcPcrPid
      // 
      this.textBoxDvbcPcrPid.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbcPcrPid.Location = new System.Drawing.Point(360, 104);
      this.textBoxDvbcPcrPid.Name = "textBoxDvbcPcrPid";
      this.textBoxDvbcPcrPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcPcrPid.TabIndex = 51;
      // 
      // labelDvbcPcrPid
      // 
      this.labelDvbcPcrPid.AutoSize = true;
      this.labelDvbcPcrPid.Location = new System.Drawing.Point(288, 104);
      this.labelDvbcPcrPid.Name = "labelDvbcPcrPid";
      this.labelDvbcPcrPid.Size = new System.Drawing.Size(50, 13);
      this.labelDvbcPcrPid.TabIndex = 50;
      this.labelDvbcPcrPid.Text = "PCR PID";
      // 
      // label99
      // 
      this.label99.AutoSize = true;
      this.label99.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label99.Location = new System.Drawing.Point(32, 16);
      this.label99.Name = "label99";
      this.label99.Size = new System.Drawing.Size(146, 13);
      this.label99.TabIndex = 49;
      this.label99.Text = "Digital cable TV settings";
      // 
      // labelDvbcAudioLanguage3
      // 
      this.labelDvbcAudioLanguage3.AutoSize = true;
      this.labelDvbcAudioLanguage3.Location = new System.Drawing.Point(288, 312);
      this.labelDvbcAudioLanguage3.Name = "labelDvbcAudioLanguage3";
      this.labelDvbcAudioLanguage3.Size = new System.Drawing.Size(43, 13);
      this.labelDvbcAudioLanguage3.TabIndex = 48;
      this.labelDvbcAudioLanguage3.Text = "Audio 3";
      // 
      // textBoxDvbcAudioLanguage3
      // 
      this.textBoxDvbcAudioLanguage3.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbcAudioLanguage3.Location = new System.Drawing.Point(360, 312);
      this.textBoxDvbcAudioLanguage3.Name = "textBoxDvbcAudioLanguage3";
      this.textBoxDvbcAudioLanguage3.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcAudioLanguage3.TabIndex = 47;
      // 
      // textBoxDvbcAudioLanguage2
      // 
      this.textBoxDvbcAudioLanguage2.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbcAudioLanguage2.Location = new System.Drawing.Point(360, 288);
      this.textBoxDvbcAudioLanguage2.Name = "textBoxDvbcAudioLanguage2";
      this.textBoxDvbcAudioLanguage2.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcAudioLanguage2.TabIndex = 46;
      // 
      // textBoxDvbcAudioLanguage1
      // 
      this.textBoxDvbcAudioLanguage1.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbcAudioLanguage1.Location = new System.Drawing.Point(360, 264);
      this.textBoxDvbcAudioLanguage1.Name = "textBoxDvbcAudioLanguage1";
      this.textBoxDvbcAudioLanguage1.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcAudioLanguage1.TabIndex = 45;
      // 
      // textBoxDvbcAudioLanguage
      // 
      this.textBoxDvbcAudioLanguage.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbcAudioLanguage.Location = new System.Drawing.Point(360, 240);
      this.textBoxDvbcAudioLanguage.Name = "textBoxDvbcAudioLanguage";
      this.textBoxDvbcAudioLanguage.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcAudioLanguage.TabIndex = 44;
      // 
      // labelDvbcAudioLanguage2
      // 
      this.labelDvbcAudioLanguage2.AutoSize = true;
      this.labelDvbcAudioLanguage2.Location = new System.Drawing.Point(288, 288);
      this.labelDvbcAudioLanguage2.Name = "labelDvbcAudioLanguage2";
      this.labelDvbcAudioLanguage2.Size = new System.Drawing.Size(43, 13);
      this.labelDvbcAudioLanguage2.TabIndex = 43;
      this.labelDvbcAudioLanguage2.Text = "Audio 2";
      // 
      // labelDvbcAudioLanguage1
      // 
      this.labelDvbcAudioLanguage1.AutoSize = true;
      this.labelDvbcAudioLanguage1.Location = new System.Drawing.Point(288, 264);
      this.labelDvbcAudioLanguage1.Name = "labelDvbcAudioLanguage1";
      this.labelDvbcAudioLanguage1.Size = new System.Drawing.Size(43, 13);
      this.labelDvbcAudioLanguage1.TabIndex = 42;
      this.labelDvbcAudioLanguage1.Text = "Audio 1";
      // 
      // labelDvbcAudioLanguage
      // 
      this.labelDvbcAudioLanguage.AutoSize = true;
      this.labelDvbcAudioLanguage.Location = new System.Drawing.Point(288, 240);
      this.labelDvbcAudioLanguage.Name = "labelDvbcAudioLanguage";
      this.labelDvbcAudioLanguage.Size = new System.Drawing.Size(34, 13);
      this.labelDvbcAudioLanguage.TabIndex = 41;
      this.labelDvbcAudioLanguage.Text = "Audio";
      // 
      // textBoxDvbcAc3Pid
      // 
      this.textBoxDvbcAc3Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbcAc3Pid.Location = new System.Drawing.Point(360, 208);
      this.textBoxDvbcAc3Pid.Name = "textBoxDvbcAc3Pid";
      this.textBoxDvbcAc3Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcAc3Pid.TabIndex = 40;
      // 
      // textBoxDvbcAudio3Pid
      // 
      this.textBoxDvbcAudio3Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbcAudio3Pid.Location = new System.Drawing.Point(360, 184);
      this.textBoxDvbcAudio3Pid.Name = "textBoxDvbcAudio3Pid";
      this.textBoxDvbcAudio3Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcAudio3Pid.TabIndex = 39;
      // 
      // textBoxDvbcAudio2Pid
      // 
      this.textBoxDvbcAudio2Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbcAudio2Pid.Location = new System.Drawing.Point(360, 160);
      this.textBoxDvbcAudio2Pid.Name = "textBoxDvbcAudio2Pid";
      this.textBoxDvbcAudio2Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcAudio2Pid.TabIndex = 38;
      // 
      // textBoxDvbcAudio1Pid
      // 
      this.textBoxDvbcAudio1Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbcAudio1Pid.Location = new System.Drawing.Point(360, 136);
      this.textBoxDvbcAudio1Pid.Name = "textBoxDvbcAudio1Pid";
      this.textBoxDvbcAudio1Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcAudio1Pid.TabIndex = 37;
      // 
      // labelDvbcAc3Pid
      // 
      this.labelDvbcAc3Pid.AutoSize = true;
      this.labelDvbcAc3Pid.Location = new System.Drawing.Point(288, 208);
      this.labelDvbcAc3Pid.Name = "labelDvbcAc3Pid";
      this.labelDvbcAc3Pid.Size = new System.Drawing.Size(48, 13);
      this.labelDvbcAc3Pid.TabIndex = 36;
      this.labelDvbcAc3Pid.Text = "AC3 PID";
      // 
      // labelDvbcAudio3Pid
      // 
      this.labelDvbcAudio3Pid.AutoSize = true;
      this.labelDvbcAudio3Pid.Location = new System.Drawing.Point(288, 184);
      this.labelDvbcAudio3Pid.Name = "labelDvbcAudio3Pid";
      this.labelDvbcAudio3Pid.Size = new System.Drawing.Size(64, 13);
      this.labelDvbcAudio3Pid.TabIndex = 35;
      this.labelDvbcAudio3Pid.Text = "Audio 3 PID";
      // 
      // labelDvbcAudio2Pid
      // 
      this.labelDvbcAudio2Pid.AutoSize = true;
      this.labelDvbcAudio2Pid.Location = new System.Drawing.Point(288, 160);
      this.labelDvbcAudio2Pid.Name = "labelDvbcAudio2Pid";
      this.labelDvbcAudio2Pid.Size = new System.Drawing.Size(64, 13);
      this.labelDvbcAudio2Pid.TabIndex = 34;
      this.labelDvbcAudio2Pid.Text = "Audio 2 PID";
      // 
      // labelDvbcAudio1Pid
      // 
      this.labelDvbcAudio1Pid.AutoSize = true;
      this.labelDvbcAudio1Pid.Location = new System.Drawing.Point(288, 136);
      this.labelDvbcAudio1Pid.Name = "labelDvbcAudio1Pid";
      this.labelDvbcAudio1Pid.Size = new System.Drawing.Size(64, 13);
      this.labelDvbcAudio1Pid.TabIndex = 33;
      this.labelDvbcAudio1Pid.Text = "Audio 1 PID";
      // 
      // textBoxDvbcPmtPid
      // 
      this.textBoxDvbcPmtPid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbcPmtPid.Location = new System.Drawing.Point(168, 304);
      this.textBoxDvbcPmtPid.Name = "textBoxDvbcPmtPid";
      this.textBoxDvbcPmtPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcPmtPid.TabIndex = 32;
      // 
      // labelDvbcPmtPid
      // 
      this.labelDvbcPmtPid.AutoSize = true;
      this.labelDvbcPmtPid.Location = new System.Drawing.Point(32, 304);
      this.labelDvbcPmtPid.Name = "labelDvbcPmtPid";
      this.labelDvbcPmtPid.Size = new System.Drawing.Size(51, 13);
      this.labelDvbcPmtPid.TabIndex = 31;
      this.labelDvbcPmtPid.Text = "PMT PID";
      // 
      // label41
      // 
      this.label41.Location = new System.Drawing.Point(288, 16);
      this.label41.Name = "label41";
      this.label41.Size = new System.Drawing.Size(152, 80);
      this.label41.TabIndex = 30;
      this.label41.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // textBoxDvbcTeletextPid
      // 
      this.textBoxDvbcTeletextPid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbcTeletextPid.Location = new System.Drawing.Point(168, 280);
      this.textBoxDvbcTeletextPid.Name = "textBoxDvbcTeletextPid";
      this.textBoxDvbcTeletextPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcTeletextPid.TabIndex = 29;
      // 
      // textBoxDvbcVideoPid
      // 
      this.textBoxDvbcVideoPid.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbcVideoPid.Location = new System.Drawing.Point(168, 256);
      this.textBoxDvbcVideoPid.Name = "textBoxDvbcVideoPid";
      this.textBoxDvbcVideoPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcVideoPid.TabIndex = 28;
      // 
      // textBoxDvbcAudioPid
      // 
      this.textBoxDvbcAudioPid.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbcAudioPid.Location = new System.Drawing.Point(168, 232);
      this.textBoxDvbcAudioPid.Name = "textBoxDvbcAudioPid";
      this.textBoxDvbcAudioPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcAudioPid.TabIndex = 27;
      // 
      // labelDvbcTeletextPid
      // 
      this.labelDvbcTeletextPid.AutoSize = true;
      this.labelDvbcTeletextPid.Location = new System.Drawing.Point(32, 280);
      this.labelDvbcTeletextPid.Name = "labelDvbcTeletextPid";
      this.labelDvbcTeletextPid.Size = new System.Drawing.Size(66, 13);
      this.labelDvbcTeletextPid.TabIndex = 26;
      this.labelDvbcTeletextPid.Text = "Teletext PID";
      // 
      // labelDvbcVideoPid
      // 
      this.labelDvbcVideoPid.AutoSize = true;
      this.labelDvbcVideoPid.Location = new System.Drawing.Point(32, 256);
      this.labelDvbcVideoPid.Name = "labelDvbcVideoPid";
      this.labelDvbcVideoPid.Size = new System.Drawing.Size(55, 13);
      this.labelDvbcVideoPid.TabIndex = 25;
      this.labelDvbcVideoPid.Text = "Video PID";
      // 
      // labelDvbcAudioPid
      // 
      this.labelDvbcAudioPid.AutoSize = true;
      this.labelDvbcAudioPid.Location = new System.Drawing.Point(32, 232);
      this.labelDvbcAudioPid.Name = "labelDvbcAudioPid";
      this.labelDvbcAudioPid.Size = new System.Drawing.Size(55, 13);
      this.labelDvbcAudioPid.TabIndex = 24;
      this.labelDvbcAudioPid.Text = "Audio PID";
      // 
      // textBoxDvbcProvider
      // 
      this.textBoxDvbcProvider.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbcProvider.Location = new System.Drawing.Point(168, 208);
      this.textBoxDvbcProvider.Name = "textBoxDvbcProvider";
      this.textBoxDvbcProvider.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcProvider.TabIndex = 23;
      // 
      // labelDvbcProvider
      // 
      this.labelDvbcProvider.AutoSize = true;
      this.labelDvbcProvider.Location = new System.Drawing.Point(32, 208);
      this.labelDvbcProvider.Name = "labelDvbcProvider";
      this.labelDvbcProvider.Size = new System.Drawing.Size(46, 13);
      this.labelDvbcProvider.TabIndex = 22;
      this.labelDvbcProvider.Text = "Provider";
      // 
      // comboBoxDvbcModulation
      // 
      this.comboBoxDvbcModulation.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxDvbcModulation.Items.AddRange(new object[] {
            "Not Set",
            "1024QAM",
            "112QAM",
            "128QAM",
            "160QAM",
            "16QAM",
            "16VSB",
            "192QAM",
            "224QAM",
            "256QAM",
            "320QAM",
            "384QAM",
            "448QAM",
            "512QAM",
            "640QAM",
            "64QAM",
            "768QAM",
            "80QAM",
            "896QAM",
            "8VSB",
            "96QAM",
            "ANALOG_AMPLITUDE",
            "ANALOG_FREQUENCY",
            "BPSK",
            "OQPSK",
            "QPSK"});
      this.comboBoxDvbcModulation.Location = new System.Drawing.Point(168, 184);
      this.comboBoxDvbcModulation.Name = "comboBoxDvbcModulation";
      this.comboBoxDvbcModulation.Size = new System.Drawing.Size(80, 21);
      this.comboBoxDvbcModulation.TabIndex = 21;
      // 
      // comboBoxDvbcInnerFec
      // 
      this.comboBoxDvbcInnerFec.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxDvbcInnerFec.Items.AddRange(new object[] {
            "Max",
            "Not Defined",
            "Not set",
            "RS 204/188",
            "ViterBi"});
      this.comboBoxDvbcInnerFec.Location = new System.Drawing.Point(168, 160);
      this.comboBoxDvbcInnerFec.Name = "comboBoxDvbcInnerFec";
      this.comboBoxDvbcInnerFec.Size = new System.Drawing.Size(80, 21);
      this.comboBoxDvbcInnerFec.TabIndex = 20;
      // 
      // labelDvbcModulation
      // 
      this.labelDvbcModulation.AutoSize = true;
      this.labelDvbcModulation.Location = new System.Drawing.Point(32, 184);
      this.labelDvbcModulation.Name = "labelDvbcModulation";
      this.labelDvbcModulation.Size = new System.Drawing.Size(59, 13);
      this.labelDvbcModulation.TabIndex = 19;
      this.labelDvbcModulation.Text = "Modulation";
      // 
      // labelDvbcInnerFec
      // 
      this.labelDvbcInnerFec.AutoSize = true;
      this.labelDvbcInnerFec.Location = new System.Drawing.Point(32, 160);
      this.labelDvbcInnerFec.Name = "labelDvbcInnerFec";
      this.labelDvbcInnerFec.Size = new System.Drawing.Size(51, 13);
      this.labelDvbcInnerFec.TabIndex = 18;
      this.labelDvbcInnerFec.Text = "InnerFEC";
      // 
      // textBoxDvbcSymbolrate
      // 
      this.textBoxDvbcSymbolrate.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbcSymbolrate.Location = new System.Drawing.Point(168, 136);
      this.textBoxDvbcSymbolrate.Name = "textBoxDvbcSymbolrate";
      this.textBoxDvbcSymbolrate.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcSymbolrate.TabIndex = 17;
      // 
      // labelDvbcSymbolrate
      // 
      this.labelDvbcSymbolrate.AutoSize = true;
      this.labelDvbcSymbolrate.Location = new System.Drawing.Point(32, 136);
      this.labelDvbcSymbolrate.Name = "labelDvbcSymbolrate";
      this.labelDvbcSymbolrate.Size = new System.Drawing.Size(59, 13);
      this.labelDvbcSymbolrate.TabIndex = 16;
      this.labelDvbcSymbolrate.Text = "Symbolrate";
      // 
      // textBoxDvbcCarrierFrequency
      // 
      this.textBoxDvbcCarrierFrequency.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbcCarrierFrequency.Location = new System.Drawing.Point(168, 112);
      this.textBoxDvbcCarrierFrequency.Name = "textBoxDvbcCarrierFrequency";
      this.textBoxDvbcCarrierFrequency.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcCarrierFrequency.TabIndex = 15;
      // 
      // labelDvbcCarrierFrequency
      // 
      this.labelDvbcCarrierFrequency.AutoSize = true;
      this.labelDvbcCarrierFrequency.Location = new System.Drawing.Point(32, 112);
      this.labelDvbcCarrierFrequency.Name = "labelDvbcCarrierFrequency";
      this.labelDvbcCarrierFrequency.Size = new System.Drawing.Size(118, 13);
      this.labelDvbcCarrierFrequency.TabIndex = 14;
      this.labelDvbcCarrierFrequency.Text = "Carrier Frequency (kHz)";
      // 
      // textBoxDvbcTransportId
      // 
      this.textBoxDvbcTransportId.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbcTransportId.Location = new System.Drawing.Point(168, 88);
      this.textBoxDvbcTransportId.Name = "textBoxDvbcTransportId";
      this.textBoxDvbcTransportId.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcTransportId.TabIndex = 13;
      // 
      // labelDvbcTransportId
      // 
      this.labelDvbcTransportId.AutoSize = true;
      this.labelDvbcTransportId.Location = new System.Drawing.Point(32, 88);
      this.labelDvbcTransportId.Name = "labelDvbcTransportId";
      this.labelDvbcTransportId.Size = new System.Drawing.Size(66, 13);
      this.labelDvbcTransportId.TabIndex = 12;
      this.labelDvbcTransportId.Text = "Transport ID";
      // 
      // textBoxDvbcServiceId
      // 
      this.textBoxDvbcServiceId.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbcServiceId.Location = new System.Drawing.Point(168, 64);
      this.textBoxDvbcServiceId.Name = "textBoxDvbcServiceId";
      this.textBoxDvbcServiceId.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcServiceId.TabIndex = 11;
      // 
      // labelDvbcServiceId
      // 
      this.labelDvbcServiceId.AutoSize = true;
      this.labelDvbcServiceId.Location = new System.Drawing.Point(32, 64);
      this.labelDvbcServiceId.Name = "labelDvbcServiceId";
      this.labelDvbcServiceId.Size = new System.Drawing.Size(57, 13);
      this.labelDvbcServiceId.TabIndex = 10;
      this.labelDvbcServiceId.Text = "Service ID";
      // 
      // textBoxDvbcNetworkId
      // 
      this.textBoxDvbcNetworkId.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbcNetworkId.Location = new System.Drawing.Point(168, 40);
      this.textBoxDvbcNetworkId.Name = "textBoxDvbcNetworkId";
      this.textBoxDvbcNetworkId.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbcNetworkId.TabIndex = 9;
      // 
      // labelDvbcNetworkId
      // 
      this.labelDvbcNetworkId.AutoSize = true;
      this.labelDvbcNetworkId.Location = new System.Drawing.Point(32, 40);
      this.labelDvbcNetworkId.Name = "labelDvbcNetworkId";
      this.labelDvbcNetworkId.Size = new System.Drawing.Size(61, 13);
      this.labelDvbcNetworkId.TabIndex = 8;
      this.labelDvbcNetworkId.Text = "Network ID";
      // 
      // tabPageDvbs
      // 
      this.tabPageDvbs.Controls.Add(this.labelDvbsRedNote);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsPcrPid);
      this.tabPageDvbs.Controls.Add(this.labelDvbsPcrPid);
      this.tabPageDvbs.Controls.Add(this.label101);
      this.tabPageDvbs.Controls.Add(this.labelDvbsAudioLanguage3);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsAudioLanguage3);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsAudioLanguage2);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsAudioLanguage1);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsAudioLanguage);
      this.tabPageDvbs.Controls.Add(this.labelDvbsAudioLanguage2);
      this.tabPageDvbs.Controls.Add(this.labelDvbsAudioLanguage1);
      this.tabPageDvbs.Controls.Add(this.labelDvbsAudioLanguage);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsAc3Pid);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsAudio3Pid);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsAudio2Pid);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsAudio1Pid);
      this.tabPageDvbs.Controls.Add(this.labelDvbsAc3Pid);
      this.tabPageDvbs.Controls.Add(this.labelDvbsAudio3Pid);
      this.tabPageDvbs.Controls.Add(this.labelDvbsAudio2Pid);
      this.tabPageDvbs.Controls.Add(this.labelDvbsAudio1Pid);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsPmtPid);
      this.tabPageDvbs.Controls.Add(this.labelDvbsPmtPid);
      this.tabPageDvbs.Controls.Add(this.label40);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsEcmPid);
      this.tabPageDvbs.Controls.Add(this.labelDvbsEcmPid);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsTeletextPid);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsVideoPid);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsAudioPid);
      this.tabPageDvbs.Controls.Add(this.labelDvbsTeletextPid);
      this.tabPageDvbs.Controls.Add(this.labelDvbsVideoPid);
      this.tabPageDvbs.Controls.Add(this.labelDvbsAudioPid);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsProvider);
      this.tabPageDvbs.Controls.Add(this.labelDvbsProvider);
      this.tabPageDvbs.Controls.Add(this.comboBoxDvbsPolarisation);
      this.tabPageDvbs.Controls.Add(this.comboBoxDvbsInnerFec);
      this.tabPageDvbs.Controls.Add(this.labelDvbsPolarisation);
      this.tabPageDvbs.Controls.Add(this.labelDvbsInnerFec);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsSymbolrate);
      this.tabPageDvbs.Controls.Add(this.labelDvbsSymbolrate);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsCarrierFrequency);
      this.tabPageDvbs.Controls.Add(this.labelDvbsCarrierFrequency);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsTransportId);
      this.tabPageDvbs.Controls.Add(this.labelDvbsTransportId);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsServiceId);
      this.tabPageDvbs.Controls.Add(this.labelDvbsServiceId);
      this.tabPageDvbs.Controls.Add(this.textBoxDvbsNetworkId);
      this.tabPageDvbs.Controls.Add(this.labelDvbsNetworkId);
      this.tabPageDvbs.Location = new System.Drawing.Point(4, 22);
      this.tabPageDvbs.Name = "tabPageDvbs";
      this.tabPageDvbs.Size = new System.Drawing.Size(480, 382);
      this.tabPageDvbs.TabIndex = 4;
      this.tabPageDvbs.Text = "DVB-S";
      this.tabPageDvbs.UseVisualStyleBackColor = true;
      // 
      // labelDvbsRedNote
      // 
      this.labelDvbsRedNote.AutoSize = true;
      this.labelDvbsRedNote.ForeColor = System.Drawing.Color.Red;
      this.labelDvbsRedNote.Location = new System.Drawing.Point(296, 352);
      this.labelDvbsRedNote.Name = "labelDvbsRedNote";
      this.labelDvbsRedNote.Size = new System.Drawing.Size(171, 13);
      this.labelDvbsRedNote.TabIndex = 84;
      this.labelDvbsRedNote.Text = "Red fields denote  required values.";
      // 
      // textBoxDvbsPcrPid
      // 
      this.textBoxDvbsPcrPid.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbsPcrPid.Location = new System.Drawing.Point(360, 104);
      this.textBoxDvbsPcrPid.Name = "textBoxDvbsPcrPid";
      this.textBoxDvbsPcrPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsPcrPid.TabIndex = 83;
      // 
      // labelDvbsPcrPid
      // 
      this.labelDvbsPcrPid.AutoSize = true;
      this.labelDvbsPcrPid.Location = new System.Drawing.Point(288, 104);
      this.labelDvbsPcrPid.Name = "labelDvbsPcrPid";
      this.labelDvbsPcrPid.Size = new System.Drawing.Size(50, 13);
      this.labelDvbsPcrPid.TabIndex = 82;
      this.labelDvbsPcrPid.Text = "PCR PID";
      // 
      // label101
      // 
      this.label101.AutoSize = true;
      this.label101.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label101.Location = new System.Drawing.Point(32, 16);
      this.label101.Name = "label101";
      this.label101.Size = new System.Drawing.Size(159, 13);
      this.label101.TabIndex = 81;
      this.label101.Text = "Digital satellite TV settings";
      // 
      // labelDvbsAudioLanguage3
      // 
      this.labelDvbsAudioLanguage3.AutoSize = true;
      this.labelDvbsAudioLanguage3.Location = new System.Drawing.Point(288, 312);
      this.labelDvbsAudioLanguage3.Name = "labelDvbsAudioLanguage3";
      this.labelDvbsAudioLanguage3.Size = new System.Drawing.Size(43, 13);
      this.labelDvbsAudioLanguage3.TabIndex = 80;
      this.labelDvbsAudioLanguage3.Text = "Audio 3";
      // 
      // textBoxDvbsAudioLanguage3
      // 
      this.textBoxDvbsAudioLanguage3.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbsAudioLanguage3.Location = new System.Drawing.Point(360, 312);
      this.textBoxDvbsAudioLanguage3.Name = "textBoxDvbsAudioLanguage3";
      this.textBoxDvbsAudioLanguage3.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsAudioLanguage3.TabIndex = 79;
      // 
      // textBoxDvbsAudioLanguage2
      // 
      this.textBoxDvbsAudioLanguage2.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbsAudioLanguage2.Location = new System.Drawing.Point(360, 288);
      this.textBoxDvbsAudioLanguage2.Name = "textBoxDvbsAudioLanguage2";
      this.textBoxDvbsAudioLanguage2.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsAudioLanguage2.TabIndex = 78;
      // 
      // textBoxDvbsAudioLanguage1
      // 
      this.textBoxDvbsAudioLanguage1.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbsAudioLanguage1.Location = new System.Drawing.Point(360, 264);
      this.textBoxDvbsAudioLanguage1.Name = "textBoxDvbsAudioLanguage1";
      this.textBoxDvbsAudioLanguage1.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsAudioLanguage1.TabIndex = 77;
      // 
      // textBoxDvbsAudioLanguage
      // 
      this.textBoxDvbsAudioLanguage.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbsAudioLanguage.Location = new System.Drawing.Point(360, 240);
      this.textBoxDvbsAudioLanguage.Name = "textBoxDvbsAudioLanguage";
      this.textBoxDvbsAudioLanguage.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsAudioLanguage.TabIndex = 76;
      // 
      // labelDvbsAudioLanguage2
      // 
      this.labelDvbsAudioLanguage2.AutoSize = true;
      this.labelDvbsAudioLanguage2.Location = new System.Drawing.Point(288, 288);
      this.labelDvbsAudioLanguage2.Name = "labelDvbsAudioLanguage2";
      this.labelDvbsAudioLanguage2.Size = new System.Drawing.Size(43, 13);
      this.labelDvbsAudioLanguage2.TabIndex = 75;
      this.labelDvbsAudioLanguage2.Text = "Audio 2";
      // 
      // labelDvbsAudioLanguage1
      // 
      this.labelDvbsAudioLanguage1.AutoSize = true;
      this.labelDvbsAudioLanguage1.Location = new System.Drawing.Point(288, 264);
      this.labelDvbsAudioLanguage1.Name = "labelDvbsAudioLanguage1";
      this.labelDvbsAudioLanguage1.Size = new System.Drawing.Size(43, 13);
      this.labelDvbsAudioLanguage1.TabIndex = 74;
      this.labelDvbsAudioLanguage1.Text = "Audio 1";
      // 
      // labelDvbsAudioLanguage
      // 
      this.labelDvbsAudioLanguage.AutoSize = true;
      this.labelDvbsAudioLanguage.Location = new System.Drawing.Point(288, 240);
      this.labelDvbsAudioLanguage.Name = "labelDvbsAudioLanguage";
      this.labelDvbsAudioLanguage.Size = new System.Drawing.Size(34, 13);
      this.labelDvbsAudioLanguage.TabIndex = 73;
      this.labelDvbsAudioLanguage.Text = "Audio";
      // 
      // textBoxDvbsAc3Pid
      // 
      this.textBoxDvbsAc3Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbsAc3Pid.Location = new System.Drawing.Point(360, 208);
      this.textBoxDvbsAc3Pid.Name = "textBoxDvbsAc3Pid";
      this.textBoxDvbsAc3Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsAc3Pid.TabIndex = 72;
      // 
      // textBoxDvbsAudio3Pid
      // 
      this.textBoxDvbsAudio3Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbsAudio3Pid.Location = new System.Drawing.Point(360, 184);
      this.textBoxDvbsAudio3Pid.Name = "textBoxDvbsAudio3Pid";
      this.textBoxDvbsAudio3Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsAudio3Pid.TabIndex = 71;
      // 
      // textBoxDvbsAudio2Pid
      // 
      this.textBoxDvbsAudio2Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbsAudio2Pid.Location = new System.Drawing.Point(360, 160);
      this.textBoxDvbsAudio2Pid.Name = "textBoxDvbsAudio2Pid";
      this.textBoxDvbsAudio2Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsAudio2Pid.TabIndex = 70;
      // 
      // textBoxDvbsAudio1Pid
      // 
      this.textBoxDvbsAudio1Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbsAudio1Pid.Location = new System.Drawing.Point(360, 136);
      this.textBoxDvbsAudio1Pid.Name = "textBoxDvbsAudio1Pid";
      this.textBoxDvbsAudio1Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsAudio1Pid.TabIndex = 69;
      // 
      // labelDvbsAc3Pid
      // 
      this.labelDvbsAc3Pid.AutoSize = true;
      this.labelDvbsAc3Pid.Location = new System.Drawing.Point(288, 208);
      this.labelDvbsAc3Pid.Name = "labelDvbsAc3Pid";
      this.labelDvbsAc3Pid.Size = new System.Drawing.Size(48, 13);
      this.labelDvbsAc3Pid.TabIndex = 68;
      this.labelDvbsAc3Pid.Text = "AC3 PID";
      // 
      // labelDvbsAudio3Pid
      // 
      this.labelDvbsAudio3Pid.AutoSize = true;
      this.labelDvbsAudio3Pid.Location = new System.Drawing.Point(288, 184);
      this.labelDvbsAudio3Pid.Name = "labelDvbsAudio3Pid";
      this.labelDvbsAudio3Pid.Size = new System.Drawing.Size(64, 13);
      this.labelDvbsAudio3Pid.TabIndex = 67;
      this.labelDvbsAudio3Pid.Text = "Audio 3 PID";
      // 
      // labelDvbsAudio2Pid
      // 
      this.labelDvbsAudio2Pid.AutoSize = true;
      this.labelDvbsAudio2Pid.Location = new System.Drawing.Point(288, 160);
      this.labelDvbsAudio2Pid.Name = "labelDvbsAudio2Pid";
      this.labelDvbsAudio2Pid.Size = new System.Drawing.Size(64, 13);
      this.labelDvbsAudio2Pid.TabIndex = 66;
      this.labelDvbsAudio2Pid.Text = "Audio 2 PID";
      // 
      // labelDvbsAudio1Pid
      // 
      this.labelDvbsAudio1Pid.AutoSize = true;
      this.labelDvbsAudio1Pid.Location = new System.Drawing.Point(288, 136);
      this.labelDvbsAudio1Pid.Name = "labelDvbsAudio1Pid";
      this.labelDvbsAudio1Pid.Size = new System.Drawing.Size(64, 13);
      this.labelDvbsAudio1Pid.TabIndex = 65;
      this.labelDvbsAudio1Pid.Text = "Audio 1 PID";
      // 
      // textBoxDvbsPmtPid
      // 
      this.textBoxDvbsPmtPid.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbsPmtPid.Location = new System.Drawing.Point(168, 328);
      this.textBoxDvbsPmtPid.Name = "textBoxDvbsPmtPid";
      this.textBoxDvbsPmtPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsPmtPid.TabIndex = 48;
      // 
      // labelDvbsPmtPid
      // 
      this.labelDvbsPmtPid.AutoSize = true;
      this.labelDvbsPmtPid.Location = new System.Drawing.Point(32, 328);
      this.labelDvbsPmtPid.Name = "labelDvbsPmtPid";
      this.labelDvbsPmtPid.Size = new System.Drawing.Size(51, 13);
      this.labelDvbsPmtPid.TabIndex = 47;
      this.labelDvbsPmtPid.Text = "PMT PID";
      // 
      // label40
      // 
      this.label40.Location = new System.Drawing.Point(288, 16);
      this.label40.Name = "label40";
      this.label40.Size = new System.Drawing.Size(152, 80);
      this.label40.TabIndex = 46;
      this.label40.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // textBoxDvbsEcmPid
      // 
      this.textBoxDvbsEcmPid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbsEcmPid.Location = new System.Drawing.Point(168, 304);
      this.textBoxDvbsEcmPid.Name = "textBoxDvbsEcmPid";
      this.textBoxDvbsEcmPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsEcmPid.TabIndex = 45;
      // 
      // labelDvbsEcmPid
      // 
      this.labelDvbsEcmPid.AutoSize = true;
      this.labelDvbsEcmPid.Location = new System.Drawing.Point(32, 304);
      this.labelDvbsEcmPid.Name = "labelDvbsEcmPid";
      this.labelDvbsEcmPid.Size = new System.Drawing.Size(51, 13);
      this.labelDvbsEcmPid.TabIndex = 44;
      this.labelDvbsEcmPid.Text = "ECM PID";
      // 
      // textBoxDvbsTeletextPid
      // 
      this.textBoxDvbsTeletextPid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbsTeletextPid.Location = new System.Drawing.Point(168, 280);
      this.textBoxDvbsTeletextPid.Name = "textBoxDvbsTeletextPid";
      this.textBoxDvbsTeletextPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsTeletextPid.TabIndex = 43;
      // 
      // textBoxDvbsVideoPid
      // 
      this.textBoxDvbsVideoPid.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbsVideoPid.Location = new System.Drawing.Point(168, 256);
      this.textBoxDvbsVideoPid.Name = "textBoxDvbsVideoPid";
      this.textBoxDvbsVideoPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsVideoPid.TabIndex = 42;
      // 
      // textBoxDvbsAudioPid
      // 
      this.textBoxDvbsAudioPid.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbsAudioPid.Location = new System.Drawing.Point(168, 232);
      this.textBoxDvbsAudioPid.Name = "textBoxDvbsAudioPid";
      this.textBoxDvbsAudioPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsAudioPid.TabIndex = 41;
      // 
      // labelDvbsTeletextPid
      // 
      this.labelDvbsTeletextPid.AutoSize = true;
      this.labelDvbsTeletextPid.Location = new System.Drawing.Point(32, 280);
      this.labelDvbsTeletextPid.Name = "labelDvbsTeletextPid";
      this.labelDvbsTeletextPid.Size = new System.Drawing.Size(66, 13);
      this.labelDvbsTeletextPid.TabIndex = 40;
      this.labelDvbsTeletextPid.Text = "Teletext PID";
      // 
      // labelDvbsVideoPid
      // 
      this.labelDvbsVideoPid.AutoSize = true;
      this.labelDvbsVideoPid.Location = new System.Drawing.Point(32, 256);
      this.labelDvbsVideoPid.Name = "labelDvbsVideoPid";
      this.labelDvbsVideoPid.Size = new System.Drawing.Size(55, 13);
      this.labelDvbsVideoPid.TabIndex = 39;
      this.labelDvbsVideoPid.Text = "Video PID";
      // 
      // labelDvbsAudioPid
      // 
      this.labelDvbsAudioPid.AutoSize = true;
      this.labelDvbsAudioPid.Location = new System.Drawing.Point(32, 232);
      this.labelDvbsAudioPid.Name = "labelDvbsAudioPid";
      this.labelDvbsAudioPid.Size = new System.Drawing.Size(55, 13);
      this.labelDvbsAudioPid.TabIndex = 38;
      this.labelDvbsAudioPid.Text = "Audio PID";
      // 
      // textBoxDvbsProvider
      // 
      this.textBoxDvbsProvider.BorderColor = System.Drawing.Color.Empty;
      this.textBoxDvbsProvider.Location = new System.Drawing.Point(168, 208);
      this.textBoxDvbsProvider.Name = "textBoxDvbsProvider";
      this.textBoxDvbsProvider.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsProvider.TabIndex = 37;
      // 
      // labelDvbsProvider
      // 
      this.labelDvbsProvider.AutoSize = true;
      this.labelDvbsProvider.Location = new System.Drawing.Point(32, 208);
      this.labelDvbsProvider.Name = "labelDvbsProvider";
      this.labelDvbsProvider.Size = new System.Drawing.Size(46, 13);
      this.labelDvbsProvider.TabIndex = 36;
      this.labelDvbsProvider.Text = "Provider";
      // 
      // comboBoxDvbsPolarisation
      // 
      this.comboBoxDvbsPolarisation.BorderColor = System.Drawing.Color.Red;
      this.comboBoxDvbsPolarisation.Items.AddRange(new object[] {
            "Horizontal",
            "Vertical"});
      this.comboBoxDvbsPolarisation.Location = new System.Drawing.Point(168, 184);
      this.comboBoxDvbsPolarisation.Name = "comboBoxDvbsPolarisation";
      this.comboBoxDvbsPolarisation.Size = new System.Drawing.Size(80, 21);
      this.comboBoxDvbsPolarisation.TabIndex = 35;
      // 
      // comboBoxDvbsInnerFec
      // 
      this.comboBoxDvbsInnerFec.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxDvbsInnerFec.Items.AddRange(new object[] {
            "Max",
            "Not Defined",
            "Not Set",
            "RS 204/188",
            "ViterBi"});
      this.comboBoxDvbsInnerFec.Location = new System.Drawing.Point(168, 160);
      this.comboBoxDvbsInnerFec.Name = "comboBoxDvbsInnerFec";
      this.comboBoxDvbsInnerFec.Size = new System.Drawing.Size(80, 21);
      this.comboBoxDvbsInnerFec.TabIndex = 34;
      // 
      // labelDvbsPolarisation
      // 
      this.labelDvbsPolarisation.AutoSize = true;
      this.labelDvbsPolarisation.Location = new System.Drawing.Point(32, 184);
      this.labelDvbsPolarisation.Name = "labelDvbsPolarisation";
      this.labelDvbsPolarisation.Size = new System.Drawing.Size(61, 13);
      this.labelDvbsPolarisation.TabIndex = 33;
      this.labelDvbsPolarisation.Text = "Polarisation";
      // 
      // labelDvbsInnerFec
      // 
      this.labelDvbsInnerFec.AutoSize = true;
      this.labelDvbsInnerFec.Location = new System.Drawing.Point(32, 160);
      this.labelDvbsInnerFec.Name = "labelDvbsInnerFec";
      this.labelDvbsInnerFec.Size = new System.Drawing.Size(51, 13);
      this.labelDvbsInnerFec.TabIndex = 32;
      this.labelDvbsInnerFec.Text = "InnerFEC";
      // 
      // textBoxDvbsSymbolrate
      // 
      this.textBoxDvbsSymbolrate.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbsSymbolrate.Location = new System.Drawing.Point(168, 136);
      this.textBoxDvbsSymbolrate.Name = "textBoxDvbsSymbolrate";
      this.textBoxDvbsSymbolrate.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsSymbolrate.TabIndex = 31;
      // 
      // labelDvbsSymbolrate
      // 
      this.labelDvbsSymbolrate.AutoSize = true;
      this.labelDvbsSymbolrate.Location = new System.Drawing.Point(32, 136);
      this.labelDvbsSymbolrate.Name = "labelDvbsSymbolrate";
      this.labelDvbsSymbolrate.Size = new System.Drawing.Size(59, 13);
      this.labelDvbsSymbolrate.TabIndex = 30;
      this.labelDvbsSymbolrate.Text = "Symbolrate";
      // 
      // textBoxDvbsCarrierFrequency
      // 
      this.textBoxDvbsCarrierFrequency.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbsCarrierFrequency.Location = new System.Drawing.Point(168, 112);
      this.textBoxDvbsCarrierFrequency.Name = "textBoxDvbsCarrierFrequency";
      this.textBoxDvbsCarrierFrequency.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsCarrierFrequency.TabIndex = 29;
      // 
      // labelDvbsCarrierFrequency
      // 
      this.labelDvbsCarrierFrequency.AutoSize = true;
      this.labelDvbsCarrierFrequency.Location = new System.Drawing.Point(32, 112);
      this.labelDvbsCarrierFrequency.Name = "labelDvbsCarrierFrequency";
      this.labelDvbsCarrierFrequency.Size = new System.Drawing.Size(118, 13);
      this.labelDvbsCarrierFrequency.TabIndex = 28;
      this.labelDvbsCarrierFrequency.Text = "Carrier Frequency (kHz)";
      // 
      // textBoxDvbsTransportId
      // 
      this.textBoxDvbsTransportId.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbsTransportId.Location = new System.Drawing.Point(168, 88);
      this.textBoxDvbsTransportId.Name = "textBoxDvbsTransportId";
      this.textBoxDvbsTransportId.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsTransportId.TabIndex = 27;
      // 
      // labelDvbsTransportId
      // 
      this.labelDvbsTransportId.AutoSize = true;
      this.labelDvbsTransportId.Location = new System.Drawing.Point(32, 88);
      this.labelDvbsTransportId.Name = "labelDvbsTransportId";
      this.labelDvbsTransportId.Size = new System.Drawing.Size(66, 13);
      this.labelDvbsTransportId.TabIndex = 26;
      this.labelDvbsTransportId.Text = "Transport ID";
      // 
      // textBoxDvbsServiceId
      // 
      this.textBoxDvbsServiceId.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbsServiceId.Location = new System.Drawing.Point(168, 64);
      this.textBoxDvbsServiceId.Name = "textBoxDvbsServiceId";
      this.textBoxDvbsServiceId.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsServiceId.TabIndex = 25;
      // 
      // labelDvbsServiceId
      // 
      this.labelDvbsServiceId.AutoSize = true;
      this.labelDvbsServiceId.Location = new System.Drawing.Point(32, 64);
      this.labelDvbsServiceId.Name = "labelDvbsServiceId";
      this.labelDvbsServiceId.Size = new System.Drawing.Size(57, 13);
      this.labelDvbsServiceId.TabIndex = 24;
      this.labelDvbsServiceId.Text = "Service ID";
      // 
      // textBoxDvbsNetworkId
      // 
      this.textBoxDvbsNetworkId.BorderColor = System.Drawing.Color.Red;
      this.textBoxDvbsNetworkId.Location = new System.Drawing.Point(168, 40);
      this.textBoxDvbsNetworkId.Name = "textBoxDvbsNetworkId";
      this.textBoxDvbsNetworkId.Size = new System.Drawing.Size(80, 20);
      this.textBoxDvbsNetworkId.TabIndex = 23;
      // 
      // labelDvbsNetworkId
      // 
      this.labelDvbsNetworkId.AutoSize = true;
      this.labelDvbsNetworkId.Location = new System.Drawing.Point(32, 40);
      this.labelDvbsNetworkId.Name = "labelDvbsNetworkId";
      this.labelDvbsNetworkId.Size = new System.Drawing.Size(61, 13);
      this.labelDvbsNetworkId.TabIndex = 22;
      this.labelDvbsNetworkId.Text = "Network ID";
      // 
      // tabPageAtsc
      // 
      this.tabPageAtsc.Controls.Add(this.labelAtscRedNote);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscPcrPid);
      this.tabPageAtsc.Controls.Add(this.labelAtscPcrPid);
      this.tabPageAtsc.Controls.Add(this.label102);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscMinorChannel);
      this.tabPageAtsc.Controls.Add(this.labelAtscMinorChannel);
      this.tabPageAtsc.Controls.Add(this.labelAtscAudioLanguage3);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscAudioLanguage3);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscAudioLanguage2);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscAudioLanguage1);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscAudioLanguage);
      this.tabPageAtsc.Controls.Add(this.labelAtscAudioLanguage2);
      this.tabPageAtsc.Controls.Add(this.labelAtscAudioLanguage1);
      this.tabPageAtsc.Controls.Add(this.labelAtscAudioLanguage);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscAc3Pid);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscAudio3Pid);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscAudio2Pid);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscAudio1Pid);
      this.tabPageAtsc.Controls.Add(this.labelAtscAc3Pid);
      this.tabPageAtsc.Controls.Add(this.labelAtscAudio3Pid);
      this.tabPageAtsc.Controls.Add(this.labelAtscAudio2Pid);
      this.tabPageAtsc.Controls.Add(this.labelAtscAudio1Pid);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscPmtPid);
      this.tabPageAtsc.Controls.Add(this.labelAtscPmtPid);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscTeletextPid);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscVideoPid);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscAudioPid);
      this.tabPageAtsc.Controls.Add(this.labelAtscTeletextPid);
      this.tabPageAtsc.Controls.Add(this.labelAtscVideoPid);
      this.tabPageAtsc.Controls.Add(this.labelAtscAudioPid);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscProvider);
      this.tabPageAtsc.Controls.Add(this.labelAtscProvider);
      this.tabPageAtsc.Controls.Add(this.comboBoxAtscModulation);
      this.tabPageAtsc.Controls.Add(this.comboBoxAtscInnerFec);
      this.tabPageAtsc.Controls.Add(this.labelAtscModulation);
      this.tabPageAtsc.Controls.Add(this.labelAtscInnerFec);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscSymbolRate);
      this.tabPageAtsc.Controls.Add(this.labelAtscSymbolRate);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscCarrierFrequency);
      this.tabPageAtsc.Controls.Add(this.labelAtscCarrierFrequency);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscTransportId);
      this.tabPageAtsc.Controls.Add(this.labelAtscTransportId);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscMajorChannel);
      this.tabPageAtsc.Controls.Add(this.labelAtscMajorChannel);
      this.tabPageAtsc.Controls.Add(this.textBoxAtscPhysicalChannel);
      this.tabPageAtsc.Controls.Add(this.labelAtscPhysicalChannel);
      this.tabPageAtsc.Location = new System.Drawing.Point(4, 22);
      this.tabPageAtsc.Name = "tabPageAtsc";
      this.tabPageAtsc.Size = new System.Drawing.Size(480, 382);
      this.tabPageAtsc.TabIndex = 6;
      this.tabPageAtsc.Text = "ATSC";
      this.tabPageAtsc.UseVisualStyleBackColor = true;
      // 
      // labelAtscRedNote
      // 
      this.labelAtscRedNote.AutoSize = true;
      this.labelAtscRedNote.ForeColor = System.Drawing.Color.Red;
      this.labelAtscRedNote.Location = new System.Drawing.Point(296, 352);
      this.labelAtscRedNote.Name = "labelAtscRedNote";
      this.labelAtscRedNote.Size = new System.Drawing.Size(171, 13);
      this.labelAtscRedNote.TabIndex = 95;
      this.labelAtscRedNote.Text = "Red fields denote  required values.";
      // 
      // textBoxAtscPcrPid
      // 
      this.textBoxAtscPcrPid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscPcrPid.Location = new System.Drawing.Point(360, 104);
      this.textBoxAtscPcrPid.Name = "textBoxAtscPcrPid";
      this.textBoxAtscPcrPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscPcrPid.TabIndex = 94;
      // 
      // labelAtscPcrPid
      // 
      this.labelAtscPcrPid.AutoSize = true;
      this.labelAtscPcrPid.Location = new System.Drawing.Point(288, 104);
      this.labelAtscPcrPid.Name = "labelAtscPcrPid";
      this.labelAtscPcrPid.Size = new System.Drawing.Size(50, 13);
      this.labelAtscPcrPid.TabIndex = 93;
      this.labelAtscPcrPid.Text = "PCR PID";
      // 
      // label102
      // 
      this.label102.AutoSize = true;
      this.label102.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label102.Location = new System.Drawing.Point(32, 16);
      this.label102.Name = "label102";
      this.label102.Size = new System.Drawing.Size(147, 13);
      this.label102.TabIndex = 92;
      this.label102.Text = "Digital ATSC TV settings";
      // 
      // textBoxAtscMinorChannel
      // 
      this.textBoxAtscMinorChannel.BorderColor = System.Drawing.Color.Red;
      this.textBoxAtscMinorChannel.Location = new System.Drawing.Point(168, 88);
      this.textBoxAtscMinorChannel.Name = "textBoxAtscMinorChannel";
      this.textBoxAtscMinorChannel.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscMinorChannel.TabIndex = 91;
      // 
      // labelAtscMinorChannel
      // 
      this.labelAtscMinorChannel.AutoSize = true;
      this.labelAtscMinorChannel.Location = new System.Drawing.Point(32, 88);
      this.labelAtscMinorChannel.Name = "labelAtscMinorChannel";
      this.labelAtscMinorChannel.Size = new System.Drawing.Size(74, 13);
      this.labelAtscMinorChannel.TabIndex = 90;
      this.labelAtscMinorChannel.Text = "Minor channel";
      // 
      // labelAtscAudioLanguage3
      // 
      this.labelAtscAudioLanguage3.AutoSize = true;
      this.labelAtscAudioLanguage3.Location = new System.Drawing.Point(288, 312);
      this.labelAtscAudioLanguage3.Name = "labelAtscAudioLanguage3";
      this.labelAtscAudioLanguage3.Size = new System.Drawing.Size(43, 13);
      this.labelAtscAudioLanguage3.TabIndex = 89;
      this.labelAtscAudioLanguage3.Text = "Audio 3";
      // 
      // textBoxAtscAudioLanguage3
      // 
      this.textBoxAtscAudioLanguage3.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscAudioLanguage3.Location = new System.Drawing.Point(360, 312);
      this.textBoxAtscAudioLanguage3.Name = "textBoxAtscAudioLanguage3";
      this.textBoxAtscAudioLanguage3.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscAudioLanguage3.TabIndex = 88;
      // 
      // textBoxAtscAudioLanguage2
      // 
      this.textBoxAtscAudioLanguage2.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscAudioLanguage2.Location = new System.Drawing.Point(360, 288);
      this.textBoxAtscAudioLanguage2.Name = "textBoxAtscAudioLanguage2";
      this.textBoxAtscAudioLanguage2.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscAudioLanguage2.TabIndex = 87;
      // 
      // textBoxAtscAudioLanguage1
      // 
      this.textBoxAtscAudioLanguage1.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscAudioLanguage1.Location = new System.Drawing.Point(360, 264);
      this.textBoxAtscAudioLanguage1.Name = "textBoxAtscAudioLanguage1";
      this.textBoxAtscAudioLanguage1.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscAudioLanguage1.TabIndex = 86;
      // 
      // textBoxAtscAudioLanguage
      // 
      this.textBoxAtscAudioLanguage.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscAudioLanguage.Location = new System.Drawing.Point(360, 240);
      this.textBoxAtscAudioLanguage.Name = "textBoxAtscAudioLanguage";
      this.textBoxAtscAudioLanguage.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscAudioLanguage.TabIndex = 85;
      // 
      // labelAtscAudioLanguage2
      // 
      this.labelAtscAudioLanguage2.AutoSize = true;
      this.labelAtscAudioLanguage2.Location = new System.Drawing.Point(288, 288);
      this.labelAtscAudioLanguage2.Name = "labelAtscAudioLanguage2";
      this.labelAtscAudioLanguage2.Size = new System.Drawing.Size(43, 13);
      this.labelAtscAudioLanguage2.TabIndex = 84;
      this.labelAtscAudioLanguage2.Text = "Audio 2";
      // 
      // labelAtscAudioLanguage1
      // 
      this.labelAtscAudioLanguage1.AutoSize = true;
      this.labelAtscAudioLanguage1.Location = new System.Drawing.Point(288, 264);
      this.labelAtscAudioLanguage1.Name = "labelAtscAudioLanguage1";
      this.labelAtscAudioLanguage1.Size = new System.Drawing.Size(43, 13);
      this.labelAtscAudioLanguage1.TabIndex = 83;
      this.labelAtscAudioLanguage1.Text = "Audio 1";
      // 
      // labelAtscAudioLanguage
      // 
      this.labelAtscAudioLanguage.AutoSize = true;
      this.labelAtscAudioLanguage.Location = new System.Drawing.Point(288, 240);
      this.labelAtscAudioLanguage.Name = "labelAtscAudioLanguage";
      this.labelAtscAudioLanguage.Size = new System.Drawing.Size(34, 13);
      this.labelAtscAudioLanguage.TabIndex = 82;
      this.labelAtscAudioLanguage.Text = "Audio";
      // 
      // textBoxAtscAc3Pid
      // 
      this.textBoxAtscAc3Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscAc3Pid.Location = new System.Drawing.Point(360, 208);
      this.textBoxAtscAc3Pid.Name = "textBoxAtscAc3Pid";
      this.textBoxAtscAc3Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscAc3Pid.TabIndex = 81;
      // 
      // textBoxAtscAudio3Pid
      // 
      this.textBoxAtscAudio3Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscAudio3Pid.Location = new System.Drawing.Point(360, 184);
      this.textBoxAtscAudio3Pid.Name = "textBoxAtscAudio3Pid";
      this.textBoxAtscAudio3Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscAudio3Pid.TabIndex = 80;
      // 
      // textBoxAtscAudio2Pid
      // 
      this.textBoxAtscAudio2Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscAudio2Pid.Location = new System.Drawing.Point(360, 160);
      this.textBoxAtscAudio2Pid.Name = "textBoxAtscAudio2Pid";
      this.textBoxAtscAudio2Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscAudio2Pid.TabIndex = 79;
      // 
      // textBoxAtscAudio1Pid
      // 
      this.textBoxAtscAudio1Pid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscAudio1Pid.Location = new System.Drawing.Point(360, 136);
      this.textBoxAtscAudio1Pid.Name = "textBoxAtscAudio1Pid";
      this.textBoxAtscAudio1Pid.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscAudio1Pid.TabIndex = 78;
      // 
      // labelAtscAc3Pid
      // 
      this.labelAtscAc3Pid.AutoSize = true;
      this.labelAtscAc3Pid.Location = new System.Drawing.Point(288, 208);
      this.labelAtscAc3Pid.Name = "labelAtscAc3Pid";
      this.labelAtscAc3Pid.Size = new System.Drawing.Size(48, 13);
      this.labelAtscAc3Pid.TabIndex = 77;
      this.labelAtscAc3Pid.Text = "AC3 PID";
      // 
      // labelAtscAudio3Pid
      // 
      this.labelAtscAudio3Pid.AutoSize = true;
      this.labelAtscAudio3Pid.Location = new System.Drawing.Point(288, 184);
      this.labelAtscAudio3Pid.Name = "labelAtscAudio3Pid";
      this.labelAtscAudio3Pid.Size = new System.Drawing.Size(64, 13);
      this.labelAtscAudio3Pid.TabIndex = 76;
      this.labelAtscAudio3Pid.Text = "Audio 3 PID";
      // 
      // labelAtscAudio2Pid
      // 
      this.labelAtscAudio2Pid.AutoSize = true;
      this.labelAtscAudio2Pid.Location = new System.Drawing.Point(288, 160);
      this.labelAtscAudio2Pid.Name = "labelAtscAudio2Pid";
      this.labelAtscAudio2Pid.Size = new System.Drawing.Size(64, 13);
      this.labelAtscAudio2Pid.TabIndex = 75;
      this.labelAtscAudio2Pid.Text = "Audio 2 PID";
      // 
      // labelAtscAudio1Pid
      // 
      this.labelAtscAudio1Pid.AutoSize = true;
      this.labelAtscAudio1Pid.Location = new System.Drawing.Point(288, 136);
      this.labelAtscAudio1Pid.Name = "labelAtscAudio1Pid";
      this.labelAtscAudio1Pid.Size = new System.Drawing.Size(64, 13);
      this.labelAtscAudio1Pid.TabIndex = 74;
      this.labelAtscAudio1Pid.Text = "Audio 1 PID";
      // 
      // textBoxAtscPmtPid
      // 
      this.textBoxAtscPmtPid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscPmtPid.Location = new System.Drawing.Point(168, 328);
      this.textBoxAtscPmtPid.Name = "textBoxAtscPmtPid";
      this.textBoxAtscPmtPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscPmtPid.TabIndex = 73;
      // 
      // labelAtscPmtPid
      // 
      this.labelAtscPmtPid.AutoSize = true;
      this.labelAtscPmtPid.Location = new System.Drawing.Point(32, 328);
      this.labelAtscPmtPid.Name = "labelAtscPmtPid";
      this.labelAtscPmtPid.Size = new System.Drawing.Size(51, 13);
      this.labelAtscPmtPid.TabIndex = 72;
      this.labelAtscPmtPid.Text = "PMT PID";
      // 
      // textBoxAtscTeletextPid
      // 
      this.textBoxAtscTeletextPid.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscTeletextPid.Location = new System.Drawing.Point(168, 304);
      this.textBoxAtscTeletextPid.Name = "textBoxAtscTeletextPid";
      this.textBoxAtscTeletextPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscTeletextPid.TabIndex = 70;
      // 
      // textBoxAtscVideoPid
      // 
      this.textBoxAtscVideoPid.BorderColor = System.Drawing.Color.Red;
      this.textBoxAtscVideoPid.Location = new System.Drawing.Point(168, 280);
      this.textBoxAtscVideoPid.Name = "textBoxAtscVideoPid";
      this.textBoxAtscVideoPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscVideoPid.TabIndex = 69;
      // 
      // textBoxAtscAudioPid
      // 
      this.textBoxAtscAudioPid.BorderColor = System.Drawing.Color.Red;
      this.textBoxAtscAudioPid.Location = new System.Drawing.Point(168, 256);
      this.textBoxAtscAudioPid.Name = "textBoxAtscAudioPid";
      this.textBoxAtscAudioPid.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscAudioPid.TabIndex = 68;
      // 
      // labelAtscTeletextPid
      // 
      this.labelAtscTeletextPid.AutoSize = true;
      this.labelAtscTeletextPid.Location = new System.Drawing.Point(32, 304);
      this.labelAtscTeletextPid.Name = "labelAtscTeletextPid";
      this.labelAtscTeletextPid.Size = new System.Drawing.Size(66, 13);
      this.labelAtscTeletextPid.TabIndex = 67;
      this.labelAtscTeletextPid.Text = "Teletext PID";
      // 
      // labelAtscVideoPid
      // 
      this.labelAtscVideoPid.AutoSize = true;
      this.labelAtscVideoPid.Location = new System.Drawing.Point(32, 280);
      this.labelAtscVideoPid.Name = "labelAtscVideoPid";
      this.labelAtscVideoPid.Size = new System.Drawing.Size(55, 13);
      this.labelAtscVideoPid.TabIndex = 66;
      this.labelAtscVideoPid.Text = "Video PID";
      // 
      // labelAtscAudioPid
      // 
      this.labelAtscAudioPid.AutoSize = true;
      this.labelAtscAudioPid.Location = new System.Drawing.Point(32, 256);
      this.labelAtscAudioPid.Name = "labelAtscAudioPid";
      this.labelAtscAudioPid.Size = new System.Drawing.Size(55, 13);
      this.labelAtscAudioPid.TabIndex = 65;
      this.labelAtscAudioPid.Text = "Audio PID";
      // 
      // textBoxAtscProvider
      // 
      this.textBoxAtscProvider.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscProvider.Location = new System.Drawing.Point(168, 232);
      this.textBoxAtscProvider.Name = "textBoxAtscProvider";
      this.textBoxAtscProvider.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscProvider.TabIndex = 64;
      // 
      // labelAtscProvider
      // 
      this.labelAtscProvider.AutoSize = true;
      this.labelAtscProvider.Location = new System.Drawing.Point(32, 232);
      this.labelAtscProvider.Name = "labelAtscProvider";
      this.labelAtscProvider.Size = new System.Drawing.Size(46, 13);
      this.labelAtscProvider.TabIndex = 63;
      this.labelAtscProvider.Text = "Provider";
      // 
      // comboBoxAtscModulation
      // 
      this.comboBoxAtscModulation.BorderColor = System.Drawing.Color.Red;
      this.comboBoxAtscModulation.Items.AddRange(new object[] {
            "Not Set",
            "1024QAM",
            "112QAM",
            "128QAM",
            "160QAM",
            "16QAM",
            "16VSB",
            "192QAM",
            "224QAM",
            "256QAM",
            "320QAM",
            "384QAM",
            "448QAM",
            "512QAM",
            "640QAM",
            "64QAM",
            "768QAM",
            "80QAM",
            "896QAM",
            "8VSB",
            "96QAM",
            "ANALOG_AMPLITUDE",
            "ANALOG_FREQUENCY",
            "BPSK",
            "OQPSK",
            "QPSK"});
      this.comboBoxAtscModulation.Location = new System.Drawing.Point(168, 208);
      this.comboBoxAtscModulation.Name = "comboBoxAtscModulation";
      this.comboBoxAtscModulation.Size = new System.Drawing.Size(80, 21);
      this.comboBoxAtscModulation.TabIndex = 62;
      // 
      // comboBoxAtscInnerFec
      // 
      this.comboBoxAtscInnerFec.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxAtscInnerFec.Items.AddRange(new object[] {
            "Max",
            "Not Defined",
            "Not set",
            "RS 204/188",
            "ViterBi"});
      this.comboBoxAtscInnerFec.Location = new System.Drawing.Point(168, 184);
      this.comboBoxAtscInnerFec.Name = "comboBoxAtscInnerFec";
      this.comboBoxAtscInnerFec.Size = new System.Drawing.Size(80, 21);
      this.comboBoxAtscInnerFec.TabIndex = 61;
      // 
      // labelAtscModulation
      // 
      this.labelAtscModulation.AutoSize = true;
      this.labelAtscModulation.Location = new System.Drawing.Point(32, 208);
      this.labelAtscModulation.Name = "labelAtscModulation";
      this.labelAtscModulation.Size = new System.Drawing.Size(59, 13);
      this.labelAtscModulation.TabIndex = 60;
      this.labelAtscModulation.Text = "Modulation";
      // 
      // labelAtscInnerFec
      // 
      this.labelAtscInnerFec.AutoSize = true;
      this.labelAtscInnerFec.Location = new System.Drawing.Point(32, 184);
      this.labelAtscInnerFec.Name = "labelAtscInnerFec";
      this.labelAtscInnerFec.Size = new System.Drawing.Size(51, 13);
      this.labelAtscInnerFec.TabIndex = 59;
      this.labelAtscInnerFec.Text = "InnerFEC";
      // 
      // textBoxAtscSymbolRate
      // 
      this.textBoxAtscSymbolRate.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscSymbolRate.Location = new System.Drawing.Point(168, 160);
      this.textBoxAtscSymbolRate.Name = "textBoxAtscSymbolRate";
      this.textBoxAtscSymbolRate.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscSymbolRate.TabIndex = 58;
      // 
      // labelAtscSymbolRate
      // 
      this.labelAtscSymbolRate.AutoSize = true;
      this.labelAtscSymbolRate.Location = new System.Drawing.Point(32, 160);
      this.labelAtscSymbolRate.Name = "labelAtscSymbolRate";
      this.labelAtscSymbolRate.Size = new System.Drawing.Size(59, 13);
      this.labelAtscSymbolRate.TabIndex = 57;
      this.labelAtscSymbolRate.Text = "Symbolrate";
      // 
      // textBoxAtscCarrierFrequency
      // 
      this.textBoxAtscCarrierFrequency.BorderColor = System.Drawing.Color.Empty;
      this.textBoxAtscCarrierFrequency.Location = new System.Drawing.Point(168, 136);
      this.textBoxAtscCarrierFrequency.Name = "textBoxAtscCarrierFrequency";
      this.textBoxAtscCarrierFrequency.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscCarrierFrequency.TabIndex = 56;
      // 
      // labelAtscCarrierFrequency
      // 
      this.labelAtscCarrierFrequency.AutoSize = true;
      this.labelAtscCarrierFrequency.Location = new System.Drawing.Point(32, 136);
      this.labelAtscCarrierFrequency.Name = "labelAtscCarrierFrequency";
      this.labelAtscCarrierFrequency.Size = new System.Drawing.Size(118, 13);
      this.labelAtscCarrierFrequency.TabIndex = 55;
      this.labelAtscCarrierFrequency.Text = "Carrier Frequency (kHz)";
      // 
      // textBoxAtscTransportId
      // 
      this.textBoxAtscTransportId.BorderColor = System.Drawing.Color.Red;
      this.textBoxAtscTransportId.Location = new System.Drawing.Point(168, 112);
      this.textBoxAtscTransportId.Name = "textBoxAtscTransportId";
      this.textBoxAtscTransportId.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscTransportId.TabIndex = 54;
      // 
      // labelAtscTransportId
      // 
      this.labelAtscTransportId.AutoSize = true;
      this.labelAtscTransportId.Location = new System.Drawing.Point(32, 112);
      this.labelAtscTransportId.Name = "labelAtscTransportId";
      this.labelAtscTransportId.Size = new System.Drawing.Size(66, 13);
      this.labelAtscTransportId.TabIndex = 53;
      this.labelAtscTransportId.Text = "Transport ID";
      // 
      // textBoxAtscMajorChannel
      // 
      this.textBoxAtscMajorChannel.BorderColor = System.Drawing.Color.Red;
      this.textBoxAtscMajorChannel.Location = new System.Drawing.Point(168, 64);
      this.textBoxAtscMajorChannel.Name = "textBoxAtscMajorChannel";
      this.textBoxAtscMajorChannel.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscMajorChannel.TabIndex = 52;
      // 
      // labelAtscMajorChannel
      // 
      this.labelAtscMajorChannel.AutoSize = true;
      this.labelAtscMajorChannel.Location = new System.Drawing.Point(32, 64);
      this.labelAtscMajorChannel.Name = "labelAtscMajorChannel";
      this.labelAtscMajorChannel.Size = new System.Drawing.Size(74, 13);
      this.labelAtscMajorChannel.TabIndex = 51;
      this.labelAtscMajorChannel.Text = "Major channel";
      // 
      // textBoxAtscPhysicalChannel
      // 
      this.textBoxAtscPhysicalChannel.BorderColor = System.Drawing.Color.Red;
      this.textBoxAtscPhysicalChannel.Location = new System.Drawing.Point(168, 40);
      this.textBoxAtscPhysicalChannel.Name = "textBoxAtscPhysicalChannel";
      this.textBoxAtscPhysicalChannel.Size = new System.Drawing.Size(80, 20);
      this.textBoxAtscPhysicalChannel.TabIndex = 50;
      // 
      // labelAtscPhysicalChannel
      // 
      this.labelAtscPhysicalChannel.AutoSize = true;
      this.labelAtscPhysicalChannel.Location = new System.Drawing.Point(32, 40);
      this.labelAtscPhysicalChannel.Name = "labelAtscPhysicalChannel";
      this.labelAtscPhysicalChannel.Size = new System.Drawing.Size(125, 13);
      this.labelAtscPhysicalChannel.TabIndex = 49;
      this.labelAtscPhysicalChannel.Text = "Physical channel number";
      // 
      // tabPageExternal
      // 
      this.tabPageExternal.Controls.Add(this.label103);
      this.tabPageExternal.Controls.Add(this.comboBoxExternalType);
      this.tabPageExternal.Controls.Add(this.labelExternalChannelNumber);
      this.tabPageExternal.Controls.Add(this.textBoxExternalChannelNumber);
      this.tabPageExternal.Controls.Add(this.labelExternalType);
      this.tabPageExternal.Controls.Add(this.labelExternalInput);
      this.tabPageExternal.Controls.Add(this.comboBoxExternalInput);
      this.tabPageExternal.Location = new System.Drawing.Point(4, 22);
      this.tabPageExternal.Name = "tabPageExternal";
      this.tabPageExternal.Size = new System.Drawing.Size(480, 382);
      this.tabPageExternal.TabIndex = 5;
      this.tabPageExternal.Text = "External";
      this.tabPageExternal.UseVisualStyleBackColor = true;
      // 
      // label103
      // 
      this.label103.Location = new System.Drawing.Point(32, 128);
      this.label103.Name = "label103";
      this.label103.Size = new System.Drawing.Size(416, 80);
      this.label103.TabIndex = 13;
      this.label103.Text = resources.GetString("label103.Text");
      // 
      // EditTVChannelForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(504, 462);
      this.Controls.Add(this.tabControl1);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MinimumSize = new System.Drawing.Size(384, 208);
      this.Name = "EditTVChannelForm";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Edit properties of TV channel";
      this.Load += new System.EventHandler(this.EditTVChannelForm_Load);
      this.tabControl1.ResumeLayout(false);
      this.tabPageGeneral.ResumeLayout(false);
      this.tabPageGeneral.PerformLayout();
      this.tabPageAnalog.ResumeLayout(false);
      this.tabPageAnalog.PerformLayout();
      this.tabPageDvbt.ResumeLayout(false);
      this.tabPageDvbt.PerformLayout();
      this.tabPageDvbc.ResumeLayout(false);
      this.tabPageDvbc.PerformLayout();
      this.tabPageDvbs.ResumeLayout(false);
      this.tabPageDvbs.PerformLayout();
      this.tabPageAtsc.ResumeLayout(false);
      this.tabPageAtsc.PerformLayout();
      this.tabPageExternal.ResumeLayout(false);
      this.tabPageExternal.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    private void channelTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void frequencyTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Make sure we only type one comma or dot
      //
      if (e.KeyChar == '.' || e.KeyChar == ',')
      {
        if (textBoxAnalogFrequency.Text.IndexOfAny(new char[] { ',', '.' }) >= 0)
        {
          e.Handled = true;
          return;
        }
      }

      if (char.IsNumber(e.KeyChar) == false && (e.KeyChar != 8 && e.KeyChar != '.' && e.KeyChar != ','))
      {
        e.Handled = true;
      }
    }

    private void okButton_Click(object sender, System.EventArgs e)
    {
      /*
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);
			TelevisionChannel newChannel=Channel;
			if (!newChannel.External)
			{
				bool channelAlreadyExists=false;
				foreach (TVChannel chan in channels)
				{
					if (chan.ID == newChannel.ID) continue;
					if (chan.Number == newChannel.Channel)
					{
						channelAlreadyExists=true;
						break;
					}
				}
				if (channelAlreadyExists)
				{
					MessageBox.Show("A channel already exists with this channel number", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
			}
      */
      SaveChannel();
      this.DialogResult = DialogResult.OK;
      this.Hide();
    }

    private void cancelButton_Click(object sender, System.EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Hide();
    }


    private void typeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      textBoxExternalChannelNumber.Enabled = comboBoxExternalInput.Enabled = (comboBoxExternalType.SelectedIndex > 0);
      comboBoxGeneralChannel.Enabled = textBoxAnalogFrequency.Enabled = !textBoxExternalChannelNumber.Enabled;

    }


    private void comboTvStandard_SelectedIndexChanged(object sender, System.EventArgs e)
    {
    }

    private void EditTVChannelForm_Load(object sender, System.EventArgs e)
    {

    }


    private void label3_Click(object sender, System.EventArgs e)
    {

    }

    public TelevisionChannel Channel
    {
      get
      {
        TelevisionChannel channel = new TelevisionChannel();

        channel.ID = channelId;
        channel.Name = textBoxGeneralName.Text;
        channel.External = (comboBoxExternalType.SelectedIndex > 0);
        if (!channel.External)
        {
          //					if (orgChannelNumber>255)
          //						channel.Channel=orgChannelNumber;
          //					else
          {
            if (comboBoxGeneralChannel.Text.Length == 0 && comboBoxGeneralChannel.SelectedIndex < 0 && orgChannelNumber == -1)
            {
              channel.Channel = TVDatabase.FindFreeTvChannelNumber(1);
            }
            else
            {
              string chanNr = (string)comboBoxGeneralChannel.SelectedItem;
              if (chanNr == null)
                chanNr = comboBoxGeneralChannel.Text.ToUpper().Trim();
              channel.Channel = -1;
              for (int i = 0; i < TVChannel.SpecialChannels.Length; ++i)
              {
                if (chanNr.Equals(TVChannel.SpecialChannels[i].Name))
                {
                  //get free nr
                  if (orgChannelNumber == -1)
                  {
                    channel.Channel = TVDatabase.FindFreeTvChannelNumber(TVChannel.SpecialChannels[i].Number);
                  }
                  else
                  {
                    int nr = TVDatabase.FindFreeTvChannelNumber(TVChannel.SpecialChannels[i].Number);
                    if (nr == TVChannel.SpecialChannels[i].Number)
                      orgChannelNumber = nr;
                    channel.Channel = orgChannelNumber;
                  }

                  Frequency freq = new Frequency(TVChannel.SpecialChannels[i].Frequency);
                  textBoxAnalogFrequency.Text = freq.ToString();

                  break;
                }
              }
              if (chanNr.Equals("SVHS")) channel.Channel = (int)ExternalInputs.svhs;
              if (chanNr.Equals("CVBS#1")) channel.Channel = (int)ExternalInputs.cvbs1;
              if (chanNr.Equals("CVBS#2")) channel.Channel = (int)ExternalInputs.cvbs2;
              if (chanNr.Equals("RGB")) channel.Channel = (int)ExternalInputs.rgb;
              if (channel.Channel == -1)
              {
                channel.Channel = Convert.ToInt32(chanNr);
              }
            }
          }
        }

        channel.Scrambled = checkBoxGeneralEncryptedChannel.Checked;
        try
        {

          if (textBoxAnalogFrequency.Text.IndexOfAny(new char[] { ',', '.' }) >= 0)
          {
            char[] separators = new char[] { '.', ',' };

            for (int index = 0; index < separators.Length; index++)
            {
              try
              {
                textBoxAnalogFrequency.Text = textBoxAnalogFrequency.Text.Replace(',', separators[index]);
                textBoxAnalogFrequency.Text = textBoxAnalogFrequency.Text.Replace('.', separators[index]);

                //
                // MegaHertz
                //
                channel.Frequency = Convert.ToDouble(textBoxAnalogFrequency.Text.Length > 0 ? textBoxAnalogFrequency.Text : "0", CultureInfo.InvariantCulture);

                break;
              }
              catch
              {
                //
                // Failed to convert, try next separator
                //
              }
            }
          }
          else
          {
            //
            // Hertz
            //
            if (textBoxAnalogFrequency.Text.Length > 3)
            {
              channel.Frequency = Convert.ToInt32(textBoxAnalogFrequency.Text.Length > 0 ? textBoxAnalogFrequency.Text : "0");
            }
            else
            {
              channel.Frequency = Convert.ToDouble(textBoxAnalogFrequency.Text.Length > 0 ? textBoxAnalogFrequency.Text : "0", CultureInfo.InvariantCulture);
            }
          }
        }
        catch
        {
          channel.Frequency = 0;
        }

        //
        // Fetch advanced settings
        //
        channel.ExternalTunerChannel = textBoxExternalChannelNumber.Text;

        if (channel.External)
        {
          channel.Frequency = 0;
          if (comboBoxExternalInput.SelectedIndex >= 0)
          {
            string externalName = (string)comboBoxExternalInput.SelectedItem;
            if (externalName.Equals("SVHS")) channel.Channel = (int)ExternalInputs.svhs;
            if (externalName.Equals("CVBS#1")) channel.Channel = (int)ExternalInputs.cvbs1;
            if (externalName.Equals("CVBS#2")) channel.Channel = (int)ExternalInputs.cvbs2;
            if (externalName.Equals("RGB")) channel.Channel = (int)ExternalInputs.rgb;
          }
        }

        string standard = comboBoxAnalogTvStandard.Text;
        if (standard == "Default") channel.standard = AnalogVideoStandard.None;
        if (standard == "NTSC M") channel.standard = AnalogVideoStandard.NTSC_M;
        if (standard == "NTSC M J") channel.standard = AnalogVideoStandard.NTSC_M_J;
        if (standard == "NTSC 433") channel.standard = AnalogVideoStandard.NTSC_433;
        if (standard == "PAL B") channel.standard = AnalogVideoStandard.PAL_B;
        if (standard == "PAL D") channel.standard = AnalogVideoStandard.PAL_D;
        if (standard == "PAL G") channel.standard = AnalogVideoStandard.PAL_G;
        if (standard == "PAL H") channel.standard = AnalogVideoStandard.PAL_H;
        if (standard == "PAL I") channel.standard = AnalogVideoStandard.PAL_I;
        if (standard == "PAL M") channel.standard = AnalogVideoStandard.PAL_M;
        if (standard == "PAL N") channel.standard = AnalogVideoStandard.PAL_N;
        if (standard == "PAL 60") channel.standard = AnalogVideoStandard.PAL_60;
        if (standard == "SECAM B") channel.standard = AnalogVideoStandard.SECAM_B;
        if (standard == "SECAM D") channel.standard = AnalogVideoStandard.SECAM_D;
        if (standard == "SECAM G") channel.standard = AnalogVideoStandard.SECAM_G;
        if (standard == "SECAM H") channel.standard = AnalogVideoStandard.SECAM_H;
        if (standard == "SECAM K") channel.standard = AnalogVideoStandard.SECAM_K;
        if (standard == "SECAM K1") channel.standard = AnalogVideoStandard.SECAM_K1;
        if (standard == "SECAM L") channel.standard = AnalogVideoStandard.SECAM_L;
        if (standard == "SECAM L1") channel.standard = AnalogVideoStandard.SECAM_L1;
        if (standard == "PAL N COMBO") channel.standard = AnalogVideoStandard.PAL_N_COMBO;

        TunerCountry tunerCountry = comboBoxAnalogCountry.SelectedItem as TunerCountry;
        if (tunerCountry != null)
          channel.Country = tunerCountry.Id;
        else
          channel.Country = -1;
        return channel;
      }

      set
      {
        TelevisionChannel channel = value as TelevisionChannel;

        if (channel != null)
        {
          orgChannelNumber = channel.Channel;
          channelId = channel.ID;
          for (int i = 0; i < comboBoxAnalogCountry.Items.Count; ++i)
          {
            TunerCountry tunerCountry = comboBoxAnalogCountry.Items[i] as TunerCountry;
            if (tunerCountry.Id == channel.Country)
            {
              comboBoxAnalogCountry.SelectedIndex = i;
              break;
            }
          }
          checkBoxGeneralEncryptedChannel.Checked = channel.Scrambled;
          textBoxGeneralName.Text = channel.Name;
          comboBoxGeneralChannel.SelectedItem = channel.Channel.ToString();
          comboBoxGeneralChannel.Text = channel.Channel.ToString();
          labelSpecial.Text = string.Empty;
          string chanNr = (string)comboBoxGeneralChannel.SelectedItem;
          if (chanNr == null)
            chanNr = comboBoxGeneralChannel.Text.ToUpper().Trim();
          for (int i = 0; i < TVChannel.SpecialChannels.Length; ++i)
          {
            if (channel.Frequency.Hertz == TVChannel.SpecialChannels[i].Frequency)
            {
              labelSpecial.Text = TVChannel.SpecialChannels[i].Name;
            }
          }
          textBoxAnalogFrequency.Text = channel.Frequency.ToString();

          comboBoxExternalType.SelectedIndex = channel.External ? 1 : 0;
          textBoxExternalChannelNumber.Text = channel.ExternalTunerChannel;

          if (channel.External == true)
          {
            switch (channel.Channel)
            {
              case (int)ExternalInputs.svhs:
                comboBoxExternalInput.Text = "SVHS";
                break;

              case (int)ExternalInputs.cvbs1:
                comboBoxExternalInput.Text = "CVBS#1";
                break;

              case (int)ExternalInputs.cvbs2:
                comboBoxExternalInput.Text = "CVBS#2";
                break;
            }
          }

          //
          // Disable boxes for static channels
          //
          if (channel.Name.Equals("CVBS#1") || channel.Name.Equals("CVBS#2") || channel.Name.Equals("SVHS") || channel.Name.Equals("RGB"))
          {
            comboBoxGeneralChannel.SelectedItem = channel.Name;
            comboBoxAnalogTvStandard.Enabled = true;
            textBoxGeneralName.Enabled = comboBoxGeneralChannel.Enabled = textBoxAnalogFrequency.Enabled = false;
          }
          comboBoxAnalogTvStandard.SelectedIndex = 0;
          if (channel.standard == AnalogVideoStandard.None) comboBoxAnalogTvStandard.SelectedIndex = 0;
          if (channel.standard == AnalogVideoStandard.NTSC_M) comboBoxAnalogTvStandard.SelectedIndex = 1;
          if (channel.standard == AnalogVideoStandard.NTSC_M_J) comboBoxAnalogTvStandard.SelectedIndex = 2;
          if (channel.standard == AnalogVideoStandard.NTSC_433) comboBoxAnalogTvStandard.SelectedIndex = 3;
          if (channel.standard == AnalogVideoStandard.PAL_B) comboBoxAnalogTvStandard.SelectedIndex = 4;
          if (channel.standard == AnalogVideoStandard.PAL_D) comboBoxAnalogTvStandard.SelectedIndex = 5;
          if (channel.standard == AnalogVideoStandard.PAL_G) comboBoxAnalogTvStandard.SelectedIndex = 6;
          if (channel.standard == AnalogVideoStandard.PAL_H) comboBoxAnalogTvStandard.SelectedIndex = 7;
          if (channel.standard == AnalogVideoStandard.PAL_I) comboBoxAnalogTvStandard.SelectedIndex = 8;
          if (channel.standard == AnalogVideoStandard.PAL_M) comboBoxAnalogTvStandard.SelectedIndex = 9;
          if (channel.standard == AnalogVideoStandard.PAL_N) comboBoxAnalogTvStandard.SelectedIndex = 10;
          if (channel.standard == AnalogVideoStandard.PAL_60) comboBoxAnalogTvStandard.SelectedIndex = 11;
          if (channel.standard == AnalogVideoStandard.SECAM_B) comboBoxAnalogTvStandard.SelectedIndex = 12;
          if (channel.standard == AnalogVideoStandard.SECAM_D) comboBoxAnalogTvStandard.SelectedIndex = 13;
          if (channel.standard == AnalogVideoStandard.SECAM_G) comboBoxAnalogTvStandard.SelectedIndex = 14;
          if (channel.standard == AnalogVideoStandard.SECAM_H) comboBoxAnalogTvStandard.SelectedIndex = 15;
          if (channel.standard == AnalogVideoStandard.SECAM_K) comboBoxAnalogTvStandard.SelectedIndex = 16;
          if (channel.standard == AnalogVideoStandard.SECAM_K1) comboBoxAnalogTvStandard.SelectedIndex = 17;
          if (channel.standard == AnalogVideoStandard.SECAM_L) comboBoxAnalogTvStandard.SelectedIndex = 18;
          if (channel.standard == AnalogVideoStandard.SECAM_L1) comboBoxAnalogTvStandard.SelectedIndex = 19;
          if (channel.standard == AnalogVideoStandard.PAL_N_COMBO) comboBoxAnalogTvStandard.SelectedIndex = 20;

          if (channel.Channel >= 0)
          {
            int freq, ONID, TSID, SID, symbolrate, innerFec, modulation, audioPid, videoPid, teletextPid, pmtPid, bandwidth;
            string provider;
            MediaPortal.TV.Recording.DVBSections dvbSections = new MediaPortal.TV.Recording.DVBSections();
            int audio1, audio2, audio3, ac3Pid, pcrPid;
            string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;

            //DVB-T
            TVDatabase.GetDVBTTuneRequest(channelId, out provider, out freq, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out bandwidth, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3, out DVBTHasEITPresentFollow, out DVBTHasEITSchedule, out pcrPid);
            label42.Text = dvbSections.GetNetworkProvider(ONID);
            textBoxDvbtCarrierFrequency.Text = freq.ToString(); ;
            textBoxDvbtNetworkId.Text = ONID.ToString(); ;
            textBoxDvbtTransportId.Text = TSID.ToString(); ;
            textBoxDvbtServiceId.Text = SID.ToString();
            textBoxDvbtProvider.Text = provider;
            textBoxDvbtAudioPid.Text = audioPid.ToString();
            textBoxDvbtVideoPid.Text = videoPid.ToString();
            textBoxDvbtTeletextPid.Text = teletextPid.ToString();
            textBoxDvbtPmtPid.Text = pmtPid.ToString();
            textBoxDvbtBandWidth.Text = bandwidth.ToString();
            textBoxDvbtAudio1Pid.Text = audio1.ToString();
            textBoxDvbtAudio2Pid.Text = audio2.ToString();
            textBoxDvbtAudio3Pid.Text = audio3.ToString();
            textBoxDvbtAc3Pid.Text = ac3Pid.ToString();
            textBoxDvbtAudioLanguage.Text = audioLanguage;
            textBoxDvbtAudioLanguage1.Text = audioLanguage1;
            textBoxDvbtAudioLanguage2.Text = audioLanguage2;
            textBoxDvbtAudioLanguage3.Text = audioLanguage3;
            textBoxDvbtPcrPid.Text = pcrPid.ToString();

            //DVB-C
            TVDatabase.GetDVBCTuneRequest(channelId, out provider, out freq, out symbolrate, out innerFec, out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3, out DVBCHasEITPresentFollow, out DVBCHasEITSchedule, out pcrPid);
            label41.Text = dvbSections.GetNetworkProvider(ONID);
            textBoxDvbcCarrierFrequency.Text = freq.ToString(); ;
            textBoxDvbcNetworkId.Text = ONID.ToString(); ;
            textBoxDvbcTransportId.Text = TSID.ToString(); ;
            textBoxDvbcServiceId.Text = SID.ToString();
            textBoxDvbcSymbolrate.Text = symbolrate.ToString();
            comboBoxDvbcInnerFec.SelectedIndex = FecToIndex(innerFec);
            comboBoxDvbcModulation.SelectedIndex = ModulationToIndex(modulation);
            textBoxDvbcProvider.Text = provider;
            textBoxDvbcAudioPid.Text = audioPid.ToString();
            textBoxDvbcVideoPid.Text = videoPid.ToString();
            textBoxDvbcTeletextPid.Text = teletextPid.ToString();
            textBoxDvbcPmtPid.Text = pmtPid.ToString();
            textBoxDvbcAudio1Pid.Text = audio1.ToString();
            textBoxDvbcAudio2Pid.Text = audio2.ToString();
            textBoxDvbcAudio3Pid.Text = audio3.ToString();
            textBoxDvbcAc3Pid.Text = ac3Pid.ToString();
            textBoxDvbcAudioLanguage.Text = audioLanguage;
            textBoxDvbcAudioLanguage1.Text = audioLanguage1;
            textBoxDvbcAudioLanguage2.Text = audioLanguage2;
            textBoxDvbcAudioLanguage3.Text = audioLanguage3;
            textBoxDvbcPcrPid.Text = pcrPid.ToString();

            //DVB-S
            DVBChannel ch = new DVBChannel();
            TVDatabase.GetSatChannel(channelId, 1, ref ch);
            label40.Text = dvbSections.GetNetworkProvider(ch.NetworkID);
            textBoxDvbsCarrierFrequency.Text = ch.Frequency.ToString(); ;
            textBoxDvbsNetworkId.Text = ch.NetworkID.ToString(); ;

            textBoxDvbsTransportId.Text = ch.TransportStreamID.ToString(); ;
            textBoxDvbsServiceId.Text = ch.ProgramNumber.ToString();
            textBoxDvbsSymbolrate.Text = ch.Symbolrate.ToString();
            comboBoxDvbsInnerFec.SelectedIndex = FecToIndex(ch.FEC);
            comboBoxDvbsPolarisation.SelectedIndex = PolarisationToIndex(ch.Polarity);
            textBoxDvbsProvider.Text = ch.ServiceProvider;
            textBoxDvbsAudioPid.Text = ch.AudioPid.ToString();
            textBoxDvbsVideoPid.Text = ch.VideoPid.ToString();
            textBoxDvbsTeletextPid.Text = ch.TeletextPid.ToString();
            textBoxDvbsEcmPid.Text = ch.ECMPid.ToString();
            textBoxDvbsPmtPid.Text = ch.PMTPid.ToString();
            textBoxDvbsAudio1Pid.Text = ch.Audio1.ToString();
            textBoxDvbsAudio2Pid.Text = ch.Audio2.ToString();
            textBoxDvbsAudio3Pid.Text = ch.Audio3.ToString();
            textBoxDvbsAc3Pid.Text = ch.AC3Pid.ToString();
            textBoxDvbsAudioLanguage.Text = ch.AudioLanguage;
            textBoxDvbsAudioLanguage1.Text = ch.AudioLanguage1;
            textBoxDvbsAudioLanguage2.Text = ch.AudioLanguage2;
            textBoxDvbsAudioLanguage3.Text = ch.AudioLanguage3;

            DVBSHasEITPresentFollow = ch.HasEITPresentFollow;
            DVBSHasEITSchedule = ch.HasEITSchedule;
            textBoxDvbsPcrPid.Text = ch.PCRPid.ToString();

            //ATSC
            int physical, minor, major;
            TVDatabase.GetATSCTuneRequest(channelId, out  physical, out provider, out freq, out symbolrate, out innerFec, out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3, out minor, out major, out ATSCHasEITPresentFollow, out ATSCHasEITSchedule, out pcrPid);
            textBoxAtscPhysicalChannel.Text = physical.ToString();
            textBoxAtscMinorChannel.Text = minor.ToString();
            textBoxAtscMajorChannel.Text = major.ToString();
            textBoxAtscCarrierFrequency.Text = freq.ToString(); ;
            textBoxAtscTransportId.Text = TSID.ToString(); ;
            textBoxAtscSymbolRate.Text = symbolrate.ToString();
            comboBoxAtscInnerFec.SelectedIndex = FecToIndex(innerFec);
            comboBoxAtscModulation.SelectedIndex = ModulationToIndex(modulation);
            textBoxAtscProvider.Text = provider;
            textBoxAtscAudioPid.Text = audioPid.ToString();
            textBoxAtscVideoPid.Text = videoPid.ToString();
            textBoxAtscTeletextPid.Text = teletextPid.ToString();
            textBoxAtscPmtPid.Text = pmtPid.ToString();
            textBoxAtscAudio1Pid.Text = audio1.ToString();
            textBoxAtscAudio2Pid.Text = audio2.ToString();
            textBoxAtscAudio3Pid.Text = audio3.ToString();
            textBoxAtscAc3Pid.Text = ac3Pid.ToString();
            textBoxAtscAudioLanguage.Text = audioLanguage;
            textBoxAtscAudioLanguage1.Text = audioLanguage1;
            textBoxAtscAudioLanguage2.Text = audioLanguage2;
            textBoxAtscAudioLanguage3.Text = audioLanguage3;
            textBoxAtscPcrPid.Text = pcrPid.ToString();

          }
        }//if(channel != null)
      }//set
    }//public TelevisionChannel Channel

    int FecToIndex(int fec)
    {
      switch ((TunerLib.FECMethod)fec)
      {
        case TunerLib.FECMethod.BDA_FEC_MAX: return 0;
        case TunerLib.FECMethod.BDA_FEC_METHOD_NOT_DEFINED: return 1;
        case TunerLib.FECMethod.BDA_FEC_METHOD_NOT_SET: return 2;
        case TunerLib.FECMethod.BDA_FEC_RS_204_188: return 3;
        case TunerLib.FECMethod.BDA_FEC_VITERBI: return 4;
      }
      return 2;
    }
    int IndexToFec(int index)
    {
      switch (index)
      {
        case 0: return (int)TunerLib.FECMethod.BDA_FEC_MAX;
        case 1: return (int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_DEFINED;
        case 2: return (int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_SET;
        case 3: return (int)TunerLib.FECMethod.BDA_FEC_RS_204_188;
        case 4: return (int)TunerLib.FECMethod.BDA_FEC_VITERBI;
      }
      return 2;
    }
    int ModulationToIndex(int modulation)
    {
      switch ((TunerLib.ModulationType)modulation)
      {
        case TunerLib.ModulationType.BDA_MOD_NOT_SET: return 0;
        case TunerLib.ModulationType.BDA_MOD_1024QAM: return 1;
        case TunerLib.ModulationType.BDA_MOD_112QAM: return 2;
        case TunerLib.ModulationType.BDA_MOD_128QAM: return 3;
        case TunerLib.ModulationType.BDA_MOD_160QAM: return 4;
        case TunerLib.ModulationType.BDA_MOD_16QAM: return 5;
        case TunerLib.ModulationType.BDA_MOD_16VSB: return 6;
        case TunerLib.ModulationType.BDA_MOD_192QAM: return 7;
        case TunerLib.ModulationType.BDA_MOD_224QAM: return 8;
        case TunerLib.ModulationType.BDA_MOD_256QAM: return 9;
        case TunerLib.ModulationType.BDA_MOD_320QAM: return 10;
        case TunerLib.ModulationType.BDA_MOD_384QAM: return 11;
        case TunerLib.ModulationType.BDA_MOD_448QAM: return 12;
        case TunerLib.ModulationType.BDA_MOD_512QAM: return 13;
        case TunerLib.ModulationType.BDA_MOD_640QAM: return 14;
        case TunerLib.ModulationType.BDA_MOD_64QAM: return 15;
        case TunerLib.ModulationType.BDA_MOD_768QAM: return 16;
        case TunerLib.ModulationType.BDA_MOD_80QAM: return 17;
        case TunerLib.ModulationType.BDA_MOD_896QAM: return 18;
        case TunerLib.ModulationType.BDA_MOD_8VSB: return 19;
        case TunerLib.ModulationType.BDA_MOD_96QAM: return 20;
        case TunerLib.ModulationType.BDA_MOD_ANALOG_AMPLITUDE: return 21;
        case TunerLib.ModulationType.BDA_MOD_ANALOG_FREQUENCY: return 22;
        case TunerLib.ModulationType.BDA_MOD_BPSK: return 23;
        case TunerLib.ModulationType.BDA_MOD_OQPSK: return 24;
        case TunerLib.ModulationType.BDA_MOD_QPSK: return 25;
      }
      return 0;
    }

    int IndexToModulation(int index)
    {
      switch (index)
      {
        case 0: return (int)TunerLib.ModulationType.BDA_MOD_NOT_SET;
        case 1: return (int)TunerLib.ModulationType.BDA_MOD_1024QAM;
        case 2: return (int)TunerLib.ModulationType.BDA_MOD_112QAM;
        case 3: return (int)TunerLib.ModulationType.BDA_MOD_128QAM;
        case 4: return (int)TunerLib.ModulationType.BDA_MOD_160QAM;
        case 5: return (int)TunerLib.ModulationType.BDA_MOD_16QAM;
        case 6: return (int)TunerLib.ModulationType.BDA_MOD_16VSB;
        case 7: return (int)TunerLib.ModulationType.BDA_MOD_192QAM;
        case 8: return (int)TunerLib.ModulationType.BDA_MOD_224QAM;
        case 9: return (int)TunerLib.ModulationType.BDA_MOD_256QAM;
        case 10: return (int)TunerLib.ModulationType.BDA_MOD_320QAM;
        case 11: return (int)TunerLib.ModulationType.BDA_MOD_384QAM;
        case 12: return (int)TunerLib.ModulationType.BDA_MOD_448QAM;
        case 13: return (int)TunerLib.ModulationType.BDA_MOD_512QAM;
        case 14: return (int)TunerLib.ModulationType.BDA_MOD_640QAM;
        case 15: return (int)TunerLib.ModulationType.BDA_MOD_64QAM;
        case 16: return (int)TunerLib.ModulationType.BDA_MOD_768QAM;
        case 17: return (int)TunerLib.ModulationType.BDA_MOD_80QAM;
        case 18: return (int)TunerLib.ModulationType.BDA_MOD_896QAM;
        case 19: return (int)TunerLib.ModulationType.BDA_MOD_8VSB;
        case 20: return (int)TunerLib.ModulationType.BDA_MOD_96QAM;
        case 21: return (int)TunerLib.ModulationType.BDA_MOD_ANALOG_AMPLITUDE;
        case 22: return (int)TunerLib.ModulationType.BDA_MOD_ANALOG_FREQUENCY;
        case 23: return (int)TunerLib.ModulationType.BDA_MOD_BPSK;
        case 24: return (int)TunerLib.ModulationType.BDA_MOD_OQPSK;
        case 25: return (int)TunerLib.ModulationType.BDA_MOD_QPSK;
      }
      return (int)TunerLib.ModulationType.BDA_MOD_NOT_SET;
    }
    int PolarisationToIndex(int polarisation)
    {
      if (polarisation < 0 || polarisation > 1) return 0;
      return polarisation;
    }
    int IndexToPolarisation(int index)
    {
      if (index < 0 || index > 1) return 0;
      return index;
    }

    void SaveChannel()
    {
      TelevisionChannel chan = Channel;
      TVChannel tvchannel = new TVChannel();
      tvchannel.ID = chan.ID;
      tvchannel.Name = chan.Name;
      tvchannel.Number = chan.Channel;
      tvchannel.Country = chan.Country;
      tvchannel.External = chan.External;
      tvchannel.ExternalTunerChannel = chan.ExternalTunerChannel;
      tvchannel.TVStandard = chan.standard;
      tvchannel.VisibleInGuide = chan.VisibleInGuide;

      if (tvchannel.Number == 0)
      {
        //get a unique number
        ArrayList chans = new ArrayList();
        TVDatabase.GetChannels(ref chans);
        tvchannel.Number = chans.Count;
        while (true)
        {
          bool ok = true;
          foreach (TVChannel ch in chans)
          {
            if (ch.Number == tvchannel.Number)
            {
              ok = false;
              tvchannel.Number++;
              break;
            }
          }
          if (ok) break;
        }
        comboBoxGeneralChannel.SelectedItem = tvchannel.Number.ToString();
      }

      tvchannel.Frequency = chan.Frequency.Hertz;
      if (chan.Frequency.Hertz < 1000)
        tvchannel.Frequency = chan.Frequency.Hertz * 1000000L;

      if (tvchannel.ID < 0)
      {
        channelId = TVDatabase.AddChannel(tvchannel);
      }
      else
      {
        TVDatabase.UpdateChannel(tvchannel, SortingPlace);
      }
      orgChannelNumber = tvchannel.Number;

      int freq, ONID, TSID, SID, symbolrate, innerFec, modulation, polarisation;
      int bandWidth, pmtPid, audioPid, videoPid, teletextPid;
      string provider;
      //dvb-T
      int audio1, audio2, audio3, ac3Pid, pcrPid;
      string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;

      try
      {
        freq = ParseInt(textBoxDvbtCarrierFrequency.Text);
        ONID = ParseInt(textBoxDvbtNetworkId.Text);
        TSID = ParseInt(textBoxDvbtTransportId.Text);
        SID = ParseInt(textBoxDvbtServiceId.Text);
        audioPid = ParseInt(textBoxDvbtAudioPid.Text);
        videoPid = ParseInt(textBoxDvbtVideoPid.Text);
        teletextPid = ParseInt(textBoxDvbtTeletextPid.Text);
        pmtPid = ParseInt(textBoxDvbtPmtPid.Text);
        provider = textBoxDvbtProvider.Text;
        bandWidth = ParseInt(textBoxDvbtBandWidth.Text);
        audio1 = ParseInt(textBoxDvbtAudio1Pid.Text);
        audio2 = ParseInt(textBoxDvbtAudio2Pid.Text);
        audio3 = ParseInt(textBoxDvbtAudio3Pid.Text);
        ac3Pid = ParseInt(textBoxDvbtAc3Pid.Text);
        audioLanguage = textBoxDvbtAudioLanguage.Text;
        audioLanguage1 = textBoxDvbtAudioLanguage1.Text;
        audioLanguage2 = textBoxDvbtAudioLanguage2.Text;
        audioLanguage3 = textBoxDvbtAudioLanguage3.Text;
        pcrPid = ParseInt(textBoxDvbtPcrPid.Text);
        if (ONID > 0 && TSID > 0 && SID > 0 && freq > 0)
        {
          TVDatabase.MapDVBTChannel(tvchannel.Name, provider, tvchannel.ID, freq, ONID, TSID, SID, audioPid, videoPid, teletextPid, pmtPid, bandWidth, audio1, audio2, audio3, ac3Pid, pcrPid, audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3, DVBTHasEITPresentFollow, DVBTHasEITSchedule);
        }
      }
      catch (Exception) { }


      //dvb-C
      try
      {
        freq = ParseInt(textBoxDvbcCarrierFrequency.Text);
        ONID = ParseInt(textBoxDvbcNetworkId.Text);
        TSID = ParseInt(textBoxDvbcTransportId.Text);
        SID = ParseInt(textBoxDvbcServiceId.Text);
        symbolrate = ParseInt(textBoxDvbcSymbolrate.Text);
        innerFec = IndexToFec(comboBoxDvbcInnerFec.SelectedIndex);
        modulation = IndexToModulation(comboBoxDvbcModulation.SelectedIndex);
        provider = textBoxDvbcProvider.Text;
        audioPid = ParseInt(textBoxDvbcAudioPid.Text);
        videoPid = ParseInt(textBoxDvbcVideoPid.Text);
        teletextPid = ParseInt(textBoxDvbcTeletextPid.Text);
        pmtPid = ParseInt(textBoxDvbcPmtPid.Text);
        audio1 = ParseInt(textBoxDvbcAudio1Pid.Text);
        audio2 = ParseInt(textBoxDvbcAudio2Pid.Text);
        audio3 = ParseInt(textBoxDvbcAudio3Pid.Text);
        ac3Pid = ParseInt(textBoxDvbcAc3Pid.Text);
        audioLanguage = textBoxDvbcAudioLanguage.Text;
        audioLanguage1 = textBoxDvbcAudioLanguage1.Text;
        audioLanguage2 = textBoxDvbcAudioLanguage2.Text;
        audioLanguage3 = textBoxDvbcAudioLanguage3.Text;
        pcrPid = ParseInt(textBoxDvbcPcrPid.Text);
        if (ONID > 0 && TSID > 0 && SID > 0 && freq > 0)
        {
          TVDatabase.MapDVBCChannel(tvchannel.Name, provider, tvchannel.ID, freq, symbolrate, innerFec, modulation, ONID, TSID, SID, audioPid, videoPid, teletextPid, pmtPid, audio1, audio2, audio3, ac3Pid, pcrPid, audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3, DVBCHasEITPresentFollow, DVBCHasEITSchedule);
        }
      }
      catch (Exception) { }

      //ATSC
      try
      {
        int physical, minor, major;
        physical = ParseInt(textBoxAtscPhysicalChannel.Text);
        minor = ParseInt(textBoxAtscMinorChannel.Text);
        major = ParseInt(textBoxAtscMajorChannel.Text);
        freq = ParseInt(textBoxAtscCarrierFrequency.Text);
        TSID = ParseInt(textBoxAtscTransportId.Text);
        symbolrate = ParseInt(textBoxAtscSymbolRate.Text);
        innerFec = IndexToFec(comboBoxAtscInnerFec.SelectedIndex);
        modulation = IndexToModulation(comboBoxAtscModulation.SelectedIndex);
        provider = textBoxAtscProvider.Text;
        audioPid = ParseInt(textBoxAtscAudioPid.Text);
        videoPid = ParseInt(textBoxAtscVideoPid.Text);
        teletextPid = ParseInt(textBoxAtscTeletextPid.Text);
        pmtPid = ParseInt(textBoxAtscPmtPid.Text);
        audio1 = ParseInt(textBoxAtscAudio1Pid.Text);
        audio2 = ParseInt(textBoxAtscAudio2Pid.Text);
        audio3 = ParseInt(textBoxAtscAudio3Pid.Text);
        ac3Pid = ParseInt(textBoxAtscAc3Pid.Text);
        audioLanguage = textBoxAtscAudioLanguage.Text;
        audioLanguage1 = textBoxAtscAudioLanguage1.Text;
        audioLanguage2 = textBoxAtscAudioLanguage2.Text;
        audioLanguage3 = textBoxAtscAudioLanguage3.Text;
        pcrPid = ParseInt(textBoxAtscPcrPid.Text);
        if (major > 0 && TSID > 0 && minor > 0 && physical > 0)
        {
          TVDatabase.MapATSCChannel(tvchannel.Name, physical, minor, major, provider, tvchannel.ID, freq, symbolrate, innerFec, modulation, -1, TSID, -1, audioPid, videoPid, teletextPid, pmtPid, audio1, audio2, audio3, ac3Pid, pcrPid, audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3, ATSCHasEITPresentFollow, ATSCHasEITSchedule);
        }
      }
      catch (Exception) { }

      //dvb-S
      try
      {
        DVBChannel ch = new DVBChannel();
        TVDatabase.GetSatChannel(tvchannel.ID, 1, ref ch);

        freq = ParseInt(textBoxDvbsCarrierFrequency.Text);
        ONID = ParseInt(textBoxDvbsNetworkId.Text);
        TSID = ParseInt(textBoxDvbsTransportId.Text);
        SID = ParseInt(textBoxDvbsServiceId.Text);
        symbolrate = ParseInt(textBoxDvbsSymbolrate.Text);
        innerFec = IndexToFec(comboBoxDvbsInnerFec.SelectedIndex);
        polarisation = IndexToPolarisation(comboBoxDvbsPolarisation.SelectedIndex);
        provider = textBoxDvbsProvider.Text;
        audioPid = ParseInt(textBoxDvbsAudioPid.Text);
        videoPid = ParseInt(textBoxDvbsVideoPid.Text);
        teletextPid = ParseInt(textBoxDvbsTeletextPid.Text);
        pmtPid = ParseInt(textBoxDvbsPmtPid.Text);
        audio1 = ParseInt(textBoxDvbsAudio1Pid.Text);
        audio2 = ParseInt(textBoxDvbsAudio2Pid.Text);
        audio3 = ParseInt(textBoxDvbsAudio3Pid.Text);
        ac3Pid = ParseInt(textBoxDvbsAc3Pid.Text);
        audioLanguage = textBoxDvbsAudioLanguage.Text;
        audioLanguage1 = textBoxDvbsAudioLanguage1.Text;
        audioLanguage2 = textBoxDvbsAudioLanguage2.Text;
        audioLanguage3 = textBoxDvbsAudioLanguage3.Text;
        pcrPid = ParseInt(textBoxDvbsPcrPid.Text);
        if (ONID > 0 && TSID > 0 && SID > 0 && freq > 0)
        {
          ch.ServiceType = 1;
          ch.Frequency = freq;
          ch.NetworkID = ONID;
          ch.TransportStreamID = TSID;
          ch.ProgramNumber = SID;
          ch.Symbolrate = symbolrate;
          ch.FEC = innerFec;
          ch.Polarity = polarisation;
          ch.ServiceProvider = provider;
          ch.ServiceName = tvchannel.Name;
          ch.ID = tvchannel.ID;
          ch.AudioPid = audioPid;
          ch.VideoPid = videoPid;
          ch.TeletextPid = teletextPid;
          ch.ECMPid = ParseInt(textBoxDvbsEcmPid.Text);
          ch.PMTPid = pmtPid;
          ch.PCRPid = pcrPid;
          ch.Audio1 = audio1;
          ch.Audio2 = audio2;
          ch.Audio3 = audio3;
          ch.AC3Pid = ac3Pid;
          ch.AudioLanguage = audioLanguage;
          ch.AudioLanguage1 = audioLanguage1;
          ch.AudioLanguage2 = audioLanguage2;
          ch.AudioLanguage3 = audioLanguage3;
          ch.HasEITPresentFollow = DVBSHasEITPresentFollow;
          ch.HasEITSchedule = DVBSHasEITSchedule;
          TVDatabase.RemoveSatChannel(ch);
          TVDatabase.AddSatChannel(ch);
        }
      }
      catch (Exception) { }

    }

    private void comboBoxChannels_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      labelSpecial.Text = string.Empty;
      string chanNr = (string)comboBoxGeneralChannel.SelectedItem;
      if (chanNr == null)
        chanNr = comboBoxGeneralChannel.Text.ToUpper().Trim();
      for (int i = 0; i < TVChannel.SpecialChannels.Length; ++i)
      {
        if (chanNr.Equals(TVChannel.SpecialChannels[i].Name))
        {
          Frequency freq = new Frequency(TVChannel.SpecialChannels[i].Frequency);
          textBoxAnalogFrequency.Text = freq.ToString();
          labelSpecial.Text = TVChannel.SpecialChannels[i].Name;
          break;
        }
      }
    }
  }

  public class TelevisionChannel
  {
    public int ID;
    public string Name = string.Empty;
    public int Channel = 0;
    public Frequency Frequency = new Frequency(0);
    public bool External = false;
    public string ExternalTunerChannel = string.Empty;
    public bool VisibleInGuide = true;
    public AnalogVideoStandard standard = AnalogVideoStandard.None;
    public int Country;
    public bool Scrambled = false;
  }

  public class Frequency
  {
    public enum Format
    {
      Hertz,
      MegaHertz
    }

    public Frequency(long hertz)
    {
      this.hertz = hertz;
    }

    public Frequency(double megahertz)
    {
      this.hertz = (long)(megahertz * (1000000d));
    }

    private long hertz = 0;

    public long Hertz
    {
      get { return hertz; }
      set
      {
        hertz = value;
        if (hertz <= 1000)
          hertz *= (int)1000000d;
      }
    }

    public double MegaHertz
    {
      get { return (double)hertz / 1000000d; }
      set
      {
        hertz = (long)(value * 1000000d);
      }
    }

    public static implicit operator Frequency(int hertz)
    {
      return new Frequency(hertz);
    }

    public static implicit operator Frequency(long hertz)
    {
      return new Frequency(hertz);
    }

    public static implicit operator Frequency(double megaHertz)
    {
      return new Frequency((long)(megaHertz * (1000000d)));
    }

    public string ToString(Format format)
    {
      string result = string.Empty;

      try
      {
        switch (format)
        {
          case Format.Hertz:
            result = String.Format("{0}", Hertz);
            break;

          case Format.MegaHertz:
            result = String.Format("{0:#,###0.000}", MegaHertz);
            break;
        }
      }
      catch
      {
        //
        // Failed to convert
        //
      }

      return result;
    }

    public override string ToString()
    {
      return ToString(Format.MegaHertz);
    }
  }

}
