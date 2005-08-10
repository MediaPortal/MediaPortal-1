/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.TV.Database;
using DShowNET;

using System.Globalization;

namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for EditTVChannelForm.
	/// </summary>
	public class EditTVChannelForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.TextBox nameTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox frequencyTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox externalChannelTextBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox typeComboBox;
		private System.Windows.Forms.ComboBox inputComboBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox comboTvStandard;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.TabPage tabPage6;
		private System.Windows.Forms.ComboBox countryComboBox;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.TextBox tbDVBTONID;
		private System.Windows.Forms.TextBox tbDVBTSID;
		private System.Windows.Forms.TextBox tbDVBTTSID;
		private System.Windows.Forms.TextBox tbDVBTFreq;
		private System.Windows.Forms.TextBox tbDVBCFreq;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.TextBox tbDVBCTSID;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.TextBox tbDVBCSID;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.TextBox tbDVBCONID;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.TextBox tbDVBCSR;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.ComboBox cbDVBCInnerFeq;
		private System.Windows.Forms.ComboBox cbDVBCModulation;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.Label label25;
		private System.Windows.Forms.Label label26;
		private System.Windows.Forms.ComboBox cbDVBSPolarisation;
		private System.Windows.Forms.ComboBox cbDvbSInnerFec;
		private System.Windows.Forms.TextBox tbDVBSSymbolrate;
		private System.Windows.Forms.TextBox tbDVBSFreq;
		private System.Windows.Forms.TextBox tbDVBSTSID;
		private System.Windows.Forms.TextBox tbDVBSSID;
		private System.Windows.Forms.TextBox tbDVBSONID;
		private int sortPlace=0;
		private int channelId=-1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label27;
		private System.Windows.Forms.TextBox tbDVBTProvider;
		private System.Windows.Forms.TextBox tbDVBCProvider;
		private System.Windows.Forms.Label label28;
		private System.Windows.Forms.TextBox tbDVBSProvider;
		private System.Windows.Forms.Label label29;
		private System.Windows.Forms.Label label30;
		private System.Windows.Forms.Label label31;
		private System.Windows.Forms.Label label32;
		private System.Windows.Forms.TextBox tbDVBTAudioPid;
		private System.Windows.Forms.TextBox tbDVBTVideoPid;
		private System.Windows.Forms.TextBox tbDVBTTeletextPid;
		private System.Windows.Forms.TextBox tbDVBCTeletextPid;
		private System.Windows.Forms.TextBox tbDVBCVideoPid;
		private System.Windows.Forms.TextBox tbDVBCAudioPid;
		private System.Windows.Forms.Label label33;
		private System.Windows.Forms.Label label34;
		private System.Windows.Forms.Label label35;
		private System.Windows.Forms.TextBox tbDVBSTeletextPid;
		private System.Windows.Forms.TextBox tbDVBSVideoPid;
		private System.Windows.Forms.TextBox tbDVBSAudioPid;
		private System.Windows.Forms.Label label36;
		private System.Windows.Forms.Label label37;
		private System.Windows.Forms.Label label38;
		private System.Windows.Forms.Label label39;
		private System.Windows.Forms.TextBox tbDVBSECMpid;
		private System.Windows.Forms.Label label40;
		private System.Windows.Forms.Label label41;
		private System.Windows.Forms.Label label42;
		private System.Windows.Forms.Label label43;
		private System.Windows.Forms.Label label44;
		private System.Windows.Forms.Label label45;
		private System.Windows.Forms.Label label46;
		private System.Windows.Forms.TextBox tbDVBCPmtPid;
		private System.Windows.Forms.Label label47;
		private System.Windows.Forms.TextBox tbDVBTPmtPid;
		private System.Windows.Forms.Label label48;
		private System.Windows.Forms.TextBox tbDVBSPmtPid;
		private System.Windows.Forms.CheckBox checkBoxScrambled;
		private System.Windows.Forms.TextBox tbBandWidth;
		private System.Windows.Forms.Label label49;
		private System.Windows.Forms.Label label50;
		private System.Windows.Forms.Label label51;
		private System.Windows.Forms.Label label52;
		private System.Windows.Forms.Label label53;
		private System.Windows.Forms.Label label54;
		private System.Windows.Forms.Label label55;
		private System.Windows.Forms.Label label56;
		private System.Windows.Forms.Label label57;
		private System.Windows.Forms.TextBox tbDVBCAudio1;
		private System.Windows.Forms.TextBox tbDVBCAudioLanguage3;
		private System.Windows.Forms.TextBox tbDVBCAudioLanguage2;
		private System.Windows.Forms.TextBox tbDVBCAudioLanguage1;
		private System.Windows.Forms.TextBox tbDVBCAudioLanguage;
		private System.Windows.Forms.TextBox tbDVBCAC3;
		private System.Windows.Forms.TextBox tbDVBCAudio3;
		private System.Windows.Forms.TextBox tbDVBCAudio2;
		private System.Windows.Forms.Label label58;
		private System.Windows.Forms.TextBox tbDVBTAudioLanguage3;
		private System.Windows.Forms.TextBox tbDVBTAudioLanguage2;
		private System.Windows.Forms.TextBox tbDVBTAudioLanguage1;
		private System.Windows.Forms.TextBox tbDVBTAudioLanguage;
		private System.Windows.Forms.Label label59;
		private System.Windows.Forms.Label label60;
		private System.Windows.Forms.Label label61;
		private System.Windows.Forms.TextBox tbDVBTAC3;
		private System.Windows.Forms.TextBox tbDVBTAudio3;
		private System.Windows.Forms.TextBox tbDVBTAudio2;
		private System.Windows.Forms.TextBox tbDVBTAudio1;
		private System.Windows.Forms.Label label62;
		private System.Windows.Forms.Label label63;
		private System.Windows.Forms.Label label64;
		private System.Windows.Forms.Label label65;
		private System.Windows.Forms.Label label66;
		private System.Windows.Forms.TextBox tbDVBSAudioLanguage3;
		private System.Windows.Forms.TextBox tbDVBSAudioLanguage2;
		private System.Windows.Forms.TextBox tbDVBSAudioLanguage1;
		private System.Windows.Forms.TextBox tbDVBSAudioLanguage;
		private System.Windows.Forms.Label label67;
		private System.Windows.Forms.Label label68;
		private System.Windows.Forms.Label label69;
		private System.Windows.Forms.TextBox tbDVBSAC3;
		private System.Windows.Forms.TextBox tbDVBSAudio3;
		private System.Windows.Forms.TextBox tbDVBSAudio2;
		private System.Windows.Forms.TextBox tbDVBSAudio1;
		private System.Windows.Forms.Label label70;
		private System.Windows.Forms.Label label71;
		private System.Windows.Forms.Label label72;
		private System.Windows.Forms.Label label73;
		private System.Windows.Forms.ComboBox comboBoxChannels;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Label labelSpecial;
		private System.Windows.Forms.TabPage tabPage7;
		private System.Windows.Forms.Label label74;
		private System.Windows.Forms.Label label75;
		private System.Windows.Forms.Label label76;
		private System.Windows.Forms.Label label77;
		private System.Windows.Forms.Label label78;
		private System.Windows.Forms.Label label79;
		private System.Windows.Forms.Label label80;
		private System.Windows.Forms.Label label81;
		private System.Windows.Forms.Label label82;
		private System.Windows.Forms.Label label84;
		private System.Windows.Forms.Label label85;
		private System.Windows.Forms.Label label86;
		private System.Windows.Forms.Label label87;
		private System.Windows.Forms.Label label88;
		private System.Windows.Forms.Label label89;
		private System.Windows.Forms.Label label90;
		private System.Windows.Forms.Label label91;
		private System.Windows.Forms.Label label92;
		private System.Windows.Forms.Label label93;
		private System.Windows.Forms.Label label94;
		private System.Windows.Forms.Label label83;
		private System.Windows.Forms.TextBox tbATSCPhysicalChannel;
		private System.Windows.Forms.ComboBox cbATSCInnerFec;
		private System.Windows.Forms.TextBox tbATSCSymbolRate;
		private System.Windows.Forms.TextBox tbATSCFrequency;
		private System.Windows.Forms.TextBox tbATSCTSID;
		private System.Windows.Forms.TextBox tbATSCMajor;
		private System.Windows.Forms.TextBox tbATSCMinor;
		private System.Windows.Forms.TextBox tbATSCAudioLanguage3;
		private System.Windows.Forms.TextBox tbATSCAudioLanguage2;
		private System.Windows.Forms.TextBox tbATSCAudioLanguage1;
		private System.Windows.Forms.TextBox tbATSCAudioLanguage;
		private System.Windows.Forms.TextBox tbATSCAC3Pid;
		private System.Windows.Forms.TextBox tbATSCAudio3Pid;
		private System.Windows.Forms.TextBox tbATSCAudio2Pid;
		private System.Windows.Forms.TextBox tbATSCAudio1Pid;
		private System.Windows.Forms.TextBox tbATSCPMTPid;
		private System.Windows.Forms.TextBox tbATSCTeletextPid;
		private System.Windows.Forms.TextBox tbATSCVideoPid;
		private System.Windows.Forms.TextBox tbATSCAudioPid;
		private System.Windows.Forms.TextBox tbATSCProvider;
		private System.Windows.Forms.ComboBox cbATSCModulation;
		private System.Windows.Forms.Label label95;
		private System.Windows.Forms.Label label96;
		private System.Windows.Forms.Label label97;
		private System.Windows.Forms.Label label98;
		private System.Windows.Forms.Label label99;
		private System.Windows.Forms.Label label100;
		private System.Windows.Forms.Label label101;
		private System.Windows.Forms.Label label102;
		private System.Windows.Forms.Label label103;
		int orgChannelNumber=-1;
		bool DVBTHasEITPresentFollow,DVBTHasEITSchedule;
		bool DVBCHasEITPresentFollow,DVBCHasEITSchedule;
		bool DVBSHasEITPresentFollow,DVBSHasEITSchedule;
		bool ATSCHasEITPresentFollow,ATSCHasEITSchedule;
		private System.Windows.Forms.TextBox tbDVBCPCR;
		private System.Windows.Forms.Label label104;
		private System.Windows.Forms.TextBox tbDVBTPCR;
		private System.Windows.Forms.Label label105;
		private System.Windows.Forms.Label label106;
		private System.Windows.Forms.TextBox tbDVBSPCR;
		private System.Windows.Forms.TextBox tbATSCPCR;
		private System.Windows.Forms.Label label108;
		private System.Windows.Forms.Label label109;
		private System.Windows.Forms.Label label110;
		private System.Windows.Forms.Label label111;
		private System.Windows.Forms.Label label112;
		private System.Windows.Forms.Label label107;

		public EditTVChannelForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// Set size of window
			//
			typeComboBox.SelectedIndex=0;
			comboTvStandard.Text = "Default";
			TunerCountry country = new TunerCountry(-1,"Default");
			countryComboBox.Items.Add(country);
			countryComboBox.Items.AddRange(TunerCountries.Countries);
			comboBoxChannels.Items.Clear();
			for (int i=1; i < 255; ++i)
				comboBoxChannels.Items.Add( i.ToString());
			for (int i=0; i < TVChannel.SpecialChannels.Length; ++i)
				comboBoxChannels.Items.Add( TVChannel.SpecialChannels[i].Name);

			comboBoxChannels.Items.Add("SVHS");
			comboBoxChannels.Items.Add("RGB");
			comboBoxChannels.Items.Add("CVBS#1");
			comboBoxChannels.Items.Add("CVBS#2");
			
		}

		public int SortingPlace
		{
			get { return sortPlace;}
			set { sortPlace=value;}
		}
		int ParseInt(string label)
		{
			try
			{
				int number=Int32.Parse(label);
				return number;
			}
			catch(Exception)
			{
			}
			return -1;
		}
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.comboTvStandard = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.frequencyTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.nameTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.inputComboBox = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.typeComboBox = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.externalChannelTextBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.label97 = new System.Windows.Forms.Label();
			this.label96 = new System.Windows.Forms.Label();
			this.label95 = new System.Windows.Forms.Label();
			this.labelSpecial = new System.Windows.Forms.Label();
			this.comboBoxChannels = new System.Windows.Forms.ComboBox();
			this.checkBoxScrambled = new System.Windows.Forms.CheckBox();
			this.label45 = new System.Windows.Forms.Label();
			this.label44 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.label98 = new System.Windows.Forms.Label();
			this.label43 = new System.Windows.Forms.Label();
			this.countryComboBox = new System.Windows.Forms.ComboBox();
			this.label8 = new System.Windows.Forms.Label();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.label108 = new System.Windows.Forms.Label();
			this.tbDVBCPCR = new System.Windows.Forms.TextBox();
			this.label104 = new System.Windows.Forms.Label();
			this.label99 = new System.Windows.Forms.Label();
			this.label57 = new System.Windows.Forms.Label();
			this.tbDVBCAudioLanguage3 = new System.Windows.Forms.TextBox();
			this.tbDVBCAudioLanguage2 = new System.Windows.Forms.TextBox();
			this.tbDVBCAudioLanguage1 = new System.Windows.Forms.TextBox();
			this.tbDVBCAudioLanguage = new System.Windows.Forms.TextBox();
			this.label54 = new System.Windows.Forms.Label();
			this.label55 = new System.Windows.Forms.Label();
			this.label56 = new System.Windows.Forms.Label();
			this.tbDVBCAC3 = new System.Windows.Forms.TextBox();
			this.tbDVBCAudio3 = new System.Windows.Forms.TextBox();
			this.tbDVBCAudio2 = new System.Windows.Forms.TextBox();
			this.tbDVBCAudio1 = new System.Windows.Forms.TextBox();
			this.label53 = new System.Windows.Forms.Label();
			this.label52 = new System.Windows.Forms.Label();
			this.label51 = new System.Windows.Forms.Label();
			this.label50 = new System.Windows.Forms.Label();
			this.tbDVBCPmtPid = new System.Windows.Forms.TextBox();
			this.label46 = new System.Windows.Forms.Label();
			this.label41 = new System.Windows.Forms.Label();
			this.tbDVBCTeletextPid = new System.Windows.Forms.TextBox();
			this.tbDVBCVideoPid = new System.Windows.Forms.TextBox();
			this.tbDVBCAudioPid = new System.Windows.Forms.TextBox();
			this.label33 = new System.Windows.Forms.Label();
			this.label34 = new System.Windows.Forms.Label();
			this.label35 = new System.Windows.Forms.Label();
			this.tbDVBCProvider = new System.Windows.Forms.TextBox();
			this.label28 = new System.Windows.Forms.Label();
			this.cbDVBCModulation = new System.Windows.Forms.ComboBox();
			this.cbDVBCInnerFeq = new System.Windows.Forms.ComboBox();
			this.label19 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.tbDVBCSR = new System.Windows.Forms.TextBox();
			this.label17 = new System.Windows.Forms.Label();
			this.tbDVBCFreq = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.tbDVBCTSID = new System.Windows.Forms.TextBox();
			this.label14 = new System.Windows.Forms.Label();
			this.tbDVBCSID = new System.Windows.Forms.TextBox();
			this.label15 = new System.Windows.Forms.Label();
			this.tbDVBCONID = new System.Windows.Forms.TextBox();
			this.label16 = new System.Windows.Forms.Label();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.label109 = new System.Windows.Forms.Label();
			this.tbDVBTPCR = new System.Windows.Forms.TextBox();
			this.label105 = new System.Windows.Forms.Label();
			this.label100 = new System.Windows.Forms.Label();
			this.label58 = new System.Windows.Forms.Label();
			this.tbDVBTAudioLanguage3 = new System.Windows.Forms.TextBox();
			this.tbDVBTAudioLanguage2 = new System.Windows.Forms.TextBox();
			this.tbDVBTAudioLanguage1 = new System.Windows.Forms.TextBox();
			this.tbDVBTAudioLanguage = new System.Windows.Forms.TextBox();
			this.label59 = new System.Windows.Forms.Label();
			this.label60 = new System.Windows.Forms.Label();
			this.label61 = new System.Windows.Forms.Label();
			this.tbDVBTAC3 = new System.Windows.Forms.TextBox();
			this.tbDVBTAudio3 = new System.Windows.Forms.TextBox();
			this.tbDVBTAudio2 = new System.Windows.Forms.TextBox();
			this.tbDVBTAudio1 = new System.Windows.Forms.TextBox();
			this.label62 = new System.Windows.Forms.Label();
			this.label63 = new System.Windows.Forms.Label();
			this.label64 = new System.Windows.Forms.Label();
			this.label65 = new System.Windows.Forms.Label();
			this.label49 = new System.Windows.Forms.Label();
			this.tbBandWidth = new System.Windows.Forms.TextBox();
			this.tbDVBTPmtPid = new System.Windows.Forms.TextBox();
			this.label47 = new System.Windows.Forms.Label();
			this.tbDVBTTeletextPid = new System.Windows.Forms.TextBox();
			this.tbDVBTVideoPid = new System.Windows.Forms.TextBox();
			this.tbDVBTAudioPid = new System.Windows.Forms.TextBox();
			this.label32 = new System.Windows.Forms.Label();
			this.label31 = new System.Windows.Forms.Label();
			this.label30 = new System.Windows.Forms.Label();
			this.tbDVBTProvider = new System.Windows.Forms.TextBox();
			this.label27 = new System.Windows.Forms.Label();
			this.tbDVBTFreq = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.tbDVBTTSID = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.tbDVBTSID = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.tbDVBTONID = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label42 = new System.Windows.Forms.Label();
			this.tabPage5 = new System.Windows.Forms.TabPage();
			this.label110 = new System.Windows.Forms.Label();
			this.tbDVBSPCR = new System.Windows.Forms.TextBox();
			this.label106 = new System.Windows.Forms.Label();
			this.label101 = new System.Windows.Forms.Label();
			this.label66 = new System.Windows.Forms.Label();
			this.tbDVBSAudioLanguage3 = new System.Windows.Forms.TextBox();
			this.tbDVBSAudioLanguage2 = new System.Windows.Forms.TextBox();
			this.tbDVBSAudioLanguage1 = new System.Windows.Forms.TextBox();
			this.tbDVBSAudioLanguage = new System.Windows.Forms.TextBox();
			this.label67 = new System.Windows.Forms.Label();
			this.label68 = new System.Windows.Forms.Label();
			this.label69 = new System.Windows.Forms.Label();
			this.tbDVBSAC3 = new System.Windows.Forms.TextBox();
			this.tbDVBSAudio3 = new System.Windows.Forms.TextBox();
			this.tbDVBSAudio2 = new System.Windows.Forms.TextBox();
			this.tbDVBSAudio1 = new System.Windows.Forms.TextBox();
			this.label70 = new System.Windows.Forms.Label();
			this.label71 = new System.Windows.Forms.Label();
			this.label72 = new System.Windows.Forms.Label();
			this.label73 = new System.Windows.Forms.Label();
			this.tbDVBSPmtPid = new System.Windows.Forms.TextBox();
			this.label48 = new System.Windows.Forms.Label();
			this.label40 = new System.Windows.Forms.Label();
			this.tbDVBSECMpid = new System.Windows.Forms.TextBox();
			this.label39 = new System.Windows.Forms.Label();
			this.tbDVBSTeletextPid = new System.Windows.Forms.TextBox();
			this.tbDVBSVideoPid = new System.Windows.Forms.TextBox();
			this.tbDVBSAudioPid = new System.Windows.Forms.TextBox();
			this.label36 = new System.Windows.Forms.Label();
			this.label37 = new System.Windows.Forms.Label();
			this.label38 = new System.Windows.Forms.Label();
			this.tbDVBSProvider = new System.Windows.Forms.TextBox();
			this.label29 = new System.Windows.Forms.Label();
			this.cbDVBSPolarisation = new System.Windows.Forms.ComboBox();
			this.cbDvbSInnerFec = new System.Windows.Forms.ComboBox();
			this.label20 = new System.Windows.Forms.Label();
			this.label21 = new System.Windows.Forms.Label();
			this.tbDVBSSymbolrate = new System.Windows.Forms.TextBox();
			this.label22 = new System.Windows.Forms.Label();
			this.tbDVBSFreq = new System.Windows.Forms.TextBox();
			this.label23 = new System.Windows.Forms.Label();
			this.tbDVBSTSID = new System.Windows.Forms.TextBox();
			this.label24 = new System.Windows.Forms.Label();
			this.tbDVBSSID = new System.Windows.Forms.TextBox();
			this.label25 = new System.Windows.Forms.Label();
			this.tbDVBSONID = new System.Windows.Forms.TextBox();
			this.label26 = new System.Windows.Forms.Label();
			this.tabPage7 = new System.Windows.Forms.TabPage();
			this.label111 = new System.Windows.Forms.Label();
			this.tbATSCPCR = new System.Windows.Forms.TextBox();
			this.label107 = new System.Windows.Forms.Label();
			this.label102 = new System.Windows.Forms.Label();
			this.tbATSCMinor = new System.Windows.Forms.TextBox();
			this.label83 = new System.Windows.Forms.Label();
			this.label74 = new System.Windows.Forms.Label();
			this.tbATSCAudioLanguage3 = new System.Windows.Forms.TextBox();
			this.tbATSCAudioLanguage2 = new System.Windows.Forms.TextBox();
			this.tbATSCAudioLanguage1 = new System.Windows.Forms.TextBox();
			this.tbATSCAudioLanguage = new System.Windows.Forms.TextBox();
			this.label75 = new System.Windows.Forms.Label();
			this.label76 = new System.Windows.Forms.Label();
			this.label77 = new System.Windows.Forms.Label();
			this.tbATSCAC3Pid = new System.Windows.Forms.TextBox();
			this.tbATSCAudio3Pid = new System.Windows.Forms.TextBox();
			this.tbATSCAudio2Pid = new System.Windows.Forms.TextBox();
			this.tbATSCAudio1Pid = new System.Windows.Forms.TextBox();
			this.label78 = new System.Windows.Forms.Label();
			this.label79 = new System.Windows.Forms.Label();
			this.label80 = new System.Windows.Forms.Label();
			this.label81 = new System.Windows.Forms.Label();
			this.tbATSCPMTPid = new System.Windows.Forms.TextBox();
			this.label82 = new System.Windows.Forms.Label();
			this.tbATSCTeletextPid = new System.Windows.Forms.TextBox();
			this.tbATSCVideoPid = new System.Windows.Forms.TextBox();
			this.tbATSCAudioPid = new System.Windows.Forms.TextBox();
			this.label84 = new System.Windows.Forms.Label();
			this.label85 = new System.Windows.Forms.Label();
			this.label86 = new System.Windows.Forms.Label();
			this.tbATSCProvider = new System.Windows.Forms.TextBox();
			this.label87 = new System.Windows.Forms.Label();
			this.cbATSCModulation = new System.Windows.Forms.ComboBox();
			this.cbATSCInnerFec = new System.Windows.Forms.ComboBox();
			this.label88 = new System.Windows.Forms.Label();
			this.label89 = new System.Windows.Forms.Label();
			this.tbATSCSymbolRate = new System.Windows.Forms.TextBox();
			this.label90 = new System.Windows.Forms.Label();
			this.tbATSCFrequency = new System.Windows.Forms.TextBox();
			this.label91 = new System.Windows.Forms.Label();
			this.tbATSCTSID = new System.Windows.Forms.TextBox();
			this.label92 = new System.Windows.Forms.Label();
			this.tbATSCMajor = new System.Windows.Forms.TextBox();
			this.label93 = new System.Windows.Forms.Label();
			this.tbATSCPhysicalChannel = new System.Windows.Forms.TextBox();
			this.label94 = new System.Windows.Forms.Label();
			this.tabPage6 = new System.Windows.Forms.TabPage();
			this.label103 = new System.Windows.Forms.Label();
			this.label112 = new System.Windows.Forms.Label();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage4.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.tabPage5.SuspendLayout();
			this.tabPage7.SuspendLayout();
			this.tabPage6.SuspendLayout();
			this.SuspendLayout();
			// 
			// comboTvStandard
			// 
			this.comboTvStandard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboTvStandard.Items.AddRange(new object[] {
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
			this.comboTvStandard.Location = new System.Drawing.Point(128, 80);
			this.comboTvStandard.Name = "comboTvStandard";
			this.comboTvStandard.Size = new System.Drawing.Size(224, 21);
			this.comboTvStandard.TabIndex = 3;
			this.comboTvStandard.SelectedIndexChanged += new System.EventHandler(this.comboTvStandard_SelectedIndexChanged);
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(24, 80);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(88, 16);
			this.label7.TabIndex = 11;
			this.label7.Text = "TV Standard:";
			this.label7.Click += new System.EventHandler(this.label7_Click);
			// 
			// frequencyTextBox
			// 
			this.frequencyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.frequencyTextBox.Location = new System.Drawing.Point(128, 48);
			this.frequencyTextBox.MaxLength = 10;
			this.frequencyTextBox.Name = "frequencyTextBox";
			this.frequencyTextBox.Size = new System.Drawing.Size(168, 20);
			this.frequencyTextBox.TabIndex = 2;
			this.frequencyTextBox.Text = "0";
			this.frequencyTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frequencyTextBox_KeyPress);
			this.frequencyTextBox.TextChanged += new System.EventHandler(this.frequencyTextBox_TextChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 48);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 32);
			this.label1.TabIndex = 10;
			this.label1.Text = "Frequency (leave 0 for default)";
			this.label1.Click += new System.EventHandler(this.label1_Click);
			// 
			// nameTextBox
			// 
			this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.nameTextBox.Location = new System.Drawing.Point(128, 16);
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.Size = new System.Drawing.Size(264, 20);
			this.nameTextBox.TabIndex = 0;
			this.nameTextBox.Text = "";
			this.nameTextBox.TextChanged += new System.EventHandler(this.nameTextBox_TextChanged);
			// 
			// label2
			// 
			this.label2.ForeColor = System.Drawing.Color.Red;
			this.label2.Location = new System.Drawing.Point(24, 16);
			this.label2.Name = "label2";
			this.label2.TabIndex = 6;
			this.label2.Text = "* Name";
			this.label2.Click += new System.EventHandler(this.label2_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = new System.Drawing.Point(421, 431);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.okButton.Location = new System.Drawing.Point(341, 431);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// inputComboBox
			// 
			this.inputComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.inputComboBox.Enabled = false;
			this.inputComboBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.inputComboBox.Items.AddRange(new object[] {
																											 "CVBS#1",
																											 "CVBS#2",
																											 "SVHS",
																											 "RGB"});
			this.inputComboBox.Location = new System.Drawing.Point(128, 56);
			this.inputComboBox.Name = "inputComboBox";
			this.inputComboBox.Size = new System.Drawing.Size(224, 21);
			this.inputComboBox.TabIndex = 1;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(24, 56);
			this.label6.Name = "label6";
			this.label6.TabIndex = 12;
			this.label6.Text = "Input via";
			this.label6.Click += new System.EventHandler(this.label6_Click);
			// 
			// typeComboBox
			// 
			this.typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.typeComboBox.Items.AddRange(new object[] {
																											"Received by tv card",
																											"Received by external settop box"});
			this.typeComboBox.Location = new System.Drawing.Point(128, 24);
			this.typeComboBox.Name = "typeComboBox";
			this.typeComboBox.Size = new System.Drawing.Size(224, 21);
			this.typeComboBox.TabIndex = 0;
			this.typeComboBox.SelectedIndexChanged += new System.EventHandler(this.typeComboBox_SelectedIndexChanged);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(24, 32);
			this.label5.Name = "label5";
			this.label5.TabIndex = 10;
			this.label5.Text = "Type";
			this.label5.Click += new System.EventHandler(this.label5_Click);
			// 
			// externalChannelTextBox
			// 
			this.externalChannelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.externalChannelTextBox.Enabled = false;
			this.externalChannelTextBox.Location = new System.Drawing.Point(128, 88);
			this.externalChannelTextBox.Name = "externalChannelTextBox";
			this.externalChannelTextBox.Size = new System.Drawing.Size(272, 20);
			this.externalChannelTextBox.TabIndex = 2;
			this.externalChannelTextBox.Text = "";
			this.externalChannelTextBox.TextChanged += new System.EventHandler(this.externalChannelTextBox_TextChanged);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(24, 88);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 32);
			this.label4.TabIndex = 8;
			this.label4.Text = "channel number on settopbox";
			this.label4.Click += new System.EventHandler(this.label4_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage5);
			this.tabControl1.Controls.Add(this.tabPage7);
			this.tabControl1.Controls.Add(this.tabPage6);
			this.tabControl1.Location = new System.Drawing.Point(8, 8);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(488, 408);
			this.tabControl1.TabIndex = 4;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.label112);
			this.tabPage1.Controls.Add(this.label97);
			this.tabPage1.Controls.Add(this.label96);
			this.tabPage1.Controls.Add(this.label95);
			this.tabPage1.Controls.Add(this.labelSpecial);
			this.tabPage1.Controls.Add(this.comboBoxChannels);
			this.tabPage1.Controls.Add(this.checkBoxScrambled);
			this.tabPage1.Controls.Add(this.label45);
			this.tabPage1.Controls.Add(this.label44);
			this.tabPage1.Controls.Add(this.label3);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.nameTextBox);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(480, 382);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "General";
			this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
			// 
			// label97
			// 
			this.label97.Location = new System.Drawing.Point(32, 264);
			this.label97.Name = "label97";
			this.label97.Size = new System.Drawing.Size(248, 23);
			this.label97.TabIndex = 18;
			this.label97.Text = "For digital TV, the channel number is not used. ";
			// 
			// label96
			// 
			this.label96.Location = new System.Drawing.Point(32, 176);
			this.label96.Name = "label96";
			this.label96.Size = new System.Drawing.Size(328, 64);
			this.label96.TabIndex = 17;
			this.label96.Text = @"For analog tv the channel number is the number on which the tv channel can be received. Note that for most countries this is not the logical number you use when zapping. Channel numbers and frequencies can be found at the website of your analog tv (cable) provider";
			// 
			// label95
			// 
			this.label95.Location = new System.Drawing.Point(16, 248);
			this.label95.Name = "label95";
			this.label95.Size = new System.Drawing.Size(344, 16);
			this.label95.TabIndex = 16;
			this.label95.Text = "Digital TV:";
			// 
			// labelSpecial
			// 
			this.labelSpecial.Location = new System.Drawing.Point(264, 56);
			this.labelSpecial.Name = "labelSpecial";
			this.labelSpecial.Size = new System.Drawing.Size(64, 16);
			this.labelSpecial.TabIndex = 15;
			// 
			// comboBoxChannels
			// 
			this.comboBoxChannels.Location = new System.Drawing.Point(128, 48);
			this.comboBoxChannels.Name = "comboBoxChannels";
			this.comboBoxChannels.Size = new System.Drawing.Size(121, 21);
			this.comboBoxChannels.TabIndex = 1;
			this.comboBoxChannels.SelectedIndexChanged += new System.EventHandler(this.comboBoxChannels_SelectedIndexChanged);
			// 
			// checkBoxScrambled
			// 
			this.checkBoxScrambled.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxScrambled.Location = new System.Drawing.Point(24, 80);
			this.checkBoxScrambled.Name = "checkBoxScrambled";
			this.checkBoxScrambled.Size = new System.Drawing.Size(264, 24);
			this.checkBoxScrambled.TabIndex = 2;
			this.checkBoxScrambled.Text = "Tv channel is encrypted (only for digital TV)";
			// 
			// label45
			// 
			this.label45.Location = new System.Drawing.Point(16, 160);
			this.label45.Name = "label45";
			this.label45.Size = new System.Drawing.Size(344, 16);
			this.label45.TabIndex = 12;
			this.label45.Text = "Analog TV:";
			// 
			// label44
			// 
			this.label44.Location = new System.Drawing.Point(16, 120);
			this.label44.Name = "label44";
			this.label44.Size = new System.Drawing.Size(344, 32);
			this.label44.TabIndex = 11;
			this.label44.Text = "Enter the name of this TV Channel. If you\'re using the TVGuide then make sure it " +
				"matches the channel name from the TVGuide";
			// 
			// label3
			// 
			this.label3.ForeColor = System.Drawing.Color.Red;
			this.label3.Location = new System.Drawing.Point(24, 48);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(88, 16);
			this.label3.TabIndex = 10;
			this.label3.Text = "* Channel";
			this.label3.Click += new System.EventHandler(this.label3_Click_1);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.label98);
			this.tabPage2.Controls.Add(this.label43);
			this.tabPage2.Controls.Add(this.countryComboBox);
			this.tabPage2.Controls.Add(this.label8);
			this.tabPage2.Controls.Add(this.label1);
			this.tabPage2.Controls.Add(this.frequencyTextBox);
			this.tabPage2.Controls.Add(this.label7);
			this.tabPage2.Controls.Add(this.comboTvStandard);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(480, 382);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Analog";
			this.tabPage2.Click += new System.EventHandler(this.tabPage2_Click);
			// 
			// label98
			// 
			this.label98.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label98.Location = new System.Drawing.Point(24, 8);
			this.label98.Name = "label98";
			this.label98.Size = new System.Drawing.Size(224, 23);
			this.label98.TabIndex = 18;
			this.label98.Text = "Analog TV settings for this TV channel";
			// 
			// label43
			// 
			this.label43.Location = new System.Drawing.Point(24, 152);
			this.label43.Name = "label43";
			this.label43.Size = new System.Drawing.Size(344, 72);
			this.label43.TabIndex = 17;
			this.label43.Text = @"You can leave the frequency to 0. In this case Mediaportal will use the default frequency for this channel. However if you have channels like 24-- or 63+ then you might need to fill in the correct frequency which you can normally find on the website of you analog cable tv provider";
			// 
			// countryComboBox
			// 
			this.countryComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.countryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.countryComboBox.Location = new System.Drawing.Point(128, 112);
			this.countryComboBox.MaxDropDownItems = 16;
			this.countryComboBox.Name = "countryComboBox";
			this.countryComboBox.Size = new System.Drawing.Size(312, 20);
			this.countryComboBox.Sorted = true;
			this.countryComboBox.TabIndex = 15;
			this.countryComboBox.SelectedIndexChanged += new System.EventHandler(this.countryComboBox_SelectedIndexChanged);
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(24, 112);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(56, 16);
			this.label8.TabIndex = 16;
			this.label8.Text = "Country";
			this.label8.Click += new System.EventHandler(this.label8_Click);
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.label108);
			this.tabPage4.Controls.Add(this.tbDVBCPCR);
			this.tabPage4.Controls.Add(this.label104);
			this.tabPage4.Controls.Add(this.label99);
			this.tabPage4.Controls.Add(this.label57);
			this.tabPage4.Controls.Add(this.tbDVBCAudioLanguage3);
			this.tabPage4.Controls.Add(this.tbDVBCAudioLanguage2);
			this.tabPage4.Controls.Add(this.tbDVBCAudioLanguage1);
			this.tabPage4.Controls.Add(this.tbDVBCAudioLanguage);
			this.tabPage4.Controls.Add(this.label54);
			this.tabPage4.Controls.Add(this.label55);
			this.tabPage4.Controls.Add(this.label56);
			this.tabPage4.Controls.Add(this.tbDVBCAC3);
			this.tabPage4.Controls.Add(this.tbDVBCAudio3);
			this.tabPage4.Controls.Add(this.tbDVBCAudio2);
			this.tabPage4.Controls.Add(this.tbDVBCAudio1);
			this.tabPage4.Controls.Add(this.label53);
			this.tabPage4.Controls.Add(this.label52);
			this.tabPage4.Controls.Add(this.label51);
			this.tabPage4.Controls.Add(this.label50);
			this.tabPage4.Controls.Add(this.tbDVBCPmtPid);
			this.tabPage4.Controls.Add(this.label46);
			this.tabPage4.Controls.Add(this.label41);
			this.tabPage4.Controls.Add(this.tbDVBCTeletextPid);
			this.tabPage4.Controls.Add(this.tbDVBCVideoPid);
			this.tabPage4.Controls.Add(this.tbDVBCAudioPid);
			this.tabPage4.Controls.Add(this.label33);
			this.tabPage4.Controls.Add(this.label34);
			this.tabPage4.Controls.Add(this.label35);
			this.tabPage4.Controls.Add(this.tbDVBCProvider);
			this.tabPage4.Controls.Add(this.label28);
			this.tabPage4.Controls.Add(this.cbDVBCModulation);
			this.tabPage4.Controls.Add(this.cbDVBCInnerFeq);
			this.tabPage4.Controls.Add(this.label19);
			this.tabPage4.Controls.Add(this.label18);
			this.tabPage4.Controls.Add(this.tbDVBCSR);
			this.tabPage4.Controls.Add(this.label17);
			this.tabPage4.Controls.Add(this.tbDVBCFreq);
			this.tabPage4.Controls.Add(this.label13);
			this.tabPage4.Controls.Add(this.tbDVBCTSID);
			this.tabPage4.Controls.Add(this.label14);
			this.tabPage4.Controls.Add(this.tbDVBCSID);
			this.tabPage4.Controls.Add(this.label15);
			this.tabPage4.Controls.Add(this.tbDVBCONID);
			this.tabPage4.Controls.Add(this.label16);
			this.tabPage4.Location = new System.Drawing.Point(4, 22);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(480, 382);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "DVB-C";
			this.tabPage4.Click += new System.EventHandler(this.tabPage4_Click);
			// 
			// label108
			// 
			this.label108.ForeColor = System.Drawing.Color.Red;
			this.label108.Location = new System.Drawing.Point(24, 352);
			this.label108.Name = "label108";
			this.label108.Size = new System.Drawing.Size(248, 16);
			this.label108.TabIndex = 52;
			this.label108.Text = "Fields with * and in red are required";
			// 
			// tbDVBCPCR
			// 
			this.tbDVBCPCR.Location = new System.Drawing.Point(360, 104);
			this.tbDVBCPCR.Name = "tbDVBCPCR";
			this.tbDVBCPCR.TabIndex = 51;
			this.tbDVBCPCR.Text = "";
			// 
			// label104
			// 
			this.label104.ForeColor = System.Drawing.Color.Red;
			this.label104.Location = new System.Drawing.Point(288, 104);
			this.label104.Name = "label104";
			this.label104.Size = new System.Drawing.Size(72, 16);
			this.label104.TabIndex = 50;
			this.label104.Text = "* PCR PID";
			// 
			// label99
			// 
			this.label99.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label99.Location = new System.Drawing.Point(24, 8);
			this.label99.Name = "label99";
			this.label99.Size = new System.Drawing.Size(272, 23);
			this.label99.TabIndex = 49;
			this.label99.Text = "Digital Cable TV settings for this TV channel";
			// 
			// label57
			// 
			this.label57.Location = new System.Drawing.Point(288, 312);
			this.label57.Name = "label57";
			this.label57.Size = new System.Drawing.Size(72, 16);
			this.label57.TabIndex = 48;
			this.label57.Text = "Audio 3:";
			// 
			// tbDVBCAudioLanguage3
			// 
			this.tbDVBCAudioLanguage3.Location = new System.Drawing.Point(360, 312);
			this.tbDVBCAudioLanguage3.Name = "tbDVBCAudioLanguage3";
			this.tbDVBCAudioLanguage3.TabIndex = 47;
			this.tbDVBCAudioLanguage3.Text = "";
			// 
			// tbDVBCAudioLanguage2
			// 
			this.tbDVBCAudioLanguage2.Location = new System.Drawing.Point(360, 288);
			this.tbDVBCAudioLanguage2.Name = "tbDVBCAudioLanguage2";
			this.tbDVBCAudioLanguage2.TabIndex = 46;
			this.tbDVBCAudioLanguage2.Text = "";
			// 
			// tbDVBCAudioLanguage1
			// 
			this.tbDVBCAudioLanguage1.Location = new System.Drawing.Point(360, 264);
			this.tbDVBCAudioLanguage1.Name = "tbDVBCAudioLanguage1";
			this.tbDVBCAudioLanguage1.TabIndex = 45;
			this.tbDVBCAudioLanguage1.Text = "";
			// 
			// tbDVBCAudioLanguage
			// 
			this.tbDVBCAudioLanguage.Location = new System.Drawing.Point(360, 240);
			this.tbDVBCAudioLanguage.Name = "tbDVBCAudioLanguage";
			this.tbDVBCAudioLanguage.TabIndex = 44;
			this.tbDVBCAudioLanguage.Text = "";
			// 
			// label54
			// 
			this.label54.Location = new System.Drawing.Point(288, 288);
			this.label54.Name = "label54";
			this.label54.Size = new System.Drawing.Size(72, 16);
			this.label54.TabIndex = 43;
			this.label54.Text = "Audio 2:";
			// 
			// label55
			// 
			this.label55.Location = new System.Drawing.Point(288, 264);
			this.label55.Name = "label55";
			this.label55.Size = new System.Drawing.Size(72, 16);
			this.label55.TabIndex = 42;
			this.label55.Text = "Audio 1:";
			// 
			// label56
			// 
			this.label56.Location = new System.Drawing.Point(288, 240);
			this.label56.Name = "label56";
			this.label56.Size = new System.Drawing.Size(72, 16);
			this.label56.TabIndex = 41;
			this.label56.Text = "Audio :";
			// 
			// tbDVBCAC3
			// 
			this.tbDVBCAC3.Location = new System.Drawing.Point(360, 208);
			this.tbDVBCAC3.Name = "tbDVBCAC3";
			this.tbDVBCAC3.TabIndex = 40;
			this.tbDVBCAC3.Text = "";
			// 
			// tbDVBCAudio3
			// 
			this.tbDVBCAudio3.Location = new System.Drawing.Point(360, 184);
			this.tbDVBCAudio3.Name = "tbDVBCAudio3";
			this.tbDVBCAudio3.TabIndex = 39;
			this.tbDVBCAudio3.Text = "";
			// 
			// tbDVBCAudio2
			// 
			this.tbDVBCAudio2.Location = new System.Drawing.Point(360, 160);
			this.tbDVBCAudio2.Name = "tbDVBCAudio2";
			this.tbDVBCAudio2.TabIndex = 38;
			this.tbDVBCAudio2.Text = "";
			// 
			// tbDVBCAudio1
			// 
			this.tbDVBCAudio1.Location = new System.Drawing.Point(360, 136);
			this.tbDVBCAudio1.Name = "tbDVBCAudio1";
			this.tbDVBCAudio1.TabIndex = 37;
			this.tbDVBCAudio1.Text = "";
			// 
			// label53
			// 
			this.label53.Location = new System.Drawing.Point(288, 208);
			this.label53.Name = "label53";
			this.label53.Size = new System.Drawing.Size(72, 16);
			this.label53.TabIndex = 36;
			this.label53.Text = "AC3 Pid:";
			// 
			// label52
			// 
			this.label52.Location = new System.Drawing.Point(288, 184);
			this.label52.Name = "label52";
			this.label52.Size = new System.Drawing.Size(72, 16);
			this.label52.TabIndex = 35;
			this.label52.Text = "Audio Pid3:";
			// 
			// label51
			// 
			this.label51.Location = new System.Drawing.Point(288, 160);
			this.label51.Name = "label51";
			this.label51.Size = new System.Drawing.Size(72, 16);
			this.label51.TabIndex = 34;
			this.label51.Text = "Audio Pid2:";
			// 
			// label50
			// 
			this.label50.Location = new System.Drawing.Point(288, 136);
			this.label50.Name = "label50";
			this.label50.Size = new System.Drawing.Size(72, 16);
			this.label50.TabIndex = 33;
			this.label50.Text = "Audio Pid1:";
			// 
			// tbDVBCPmtPid
			// 
			this.tbDVBCPmtPid.Location = new System.Drawing.Point(160, 312);
			this.tbDVBCPmtPid.Name = "tbDVBCPmtPid";
			this.tbDVBCPmtPid.TabIndex = 32;
			this.tbDVBCPmtPid.Text = "";
			// 
			// label46
			// 
			this.label46.Location = new System.Drawing.Point(24, 312);
			this.label46.Name = "label46";
			this.label46.Size = new System.Drawing.Size(72, 16);
			this.label46.TabIndex = 31;
			this.label46.Text = "* PMT pid:";
			// 
			// label41
			// 
			this.label41.Location = new System.Drawing.Point(224, 48);
			this.label41.Name = "label41";
			this.label41.Size = new System.Drawing.Size(160, 16);
			this.label41.TabIndex = 30;
			// 
			// tbDVBCTeletextPid
			// 
			this.tbDVBCTeletextPid.Location = new System.Drawing.Point(160, 288);
			this.tbDVBCTeletextPid.Name = "tbDVBCTeletextPid";
			this.tbDVBCTeletextPid.TabIndex = 29;
			this.tbDVBCTeletextPid.Text = "";
			// 
			// tbDVBCVideoPid
			// 
			this.tbDVBCVideoPid.Location = new System.Drawing.Point(160, 264);
			this.tbDVBCVideoPid.Name = "tbDVBCVideoPid";
			this.tbDVBCVideoPid.TabIndex = 28;
			this.tbDVBCVideoPid.Text = "";
			// 
			// tbDVBCAudioPid
			// 
			this.tbDVBCAudioPid.Location = new System.Drawing.Point(160, 240);
			this.tbDVBCAudioPid.Name = "tbDVBCAudioPid";
			this.tbDVBCAudioPid.TabIndex = 27;
			this.tbDVBCAudioPid.Text = "";
			// 
			// label33
			// 
			this.label33.Location = new System.Drawing.Point(24, 288);
			this.label33.Name = "label33";
			this.label33.Size = new System.Drawing.Size(72, 16);
			this.label33.TabIndex = 26;
			this.label33.Text = "Teletext pid:";
			// 
			// label34
			// 
			this.label34.ForeColor = System.Drawing.Color.Red;
			this.label34.Location = new System.Drawing.Point(24, 264);
			this.label34.Name = "label34";
			this.label34.Size = new System.Drawing.Size(72, 16);
			this.label34.TabIndex = 25;
			this.label34.Text = "* Video pid:";
			// 
			// label35
			// 
			this.label35.ForeColor = System.Drawing.Color.Red;
			this.label35.Location = new System.Drawing.Point(24, 240);
			this.label35.Name = "label35";
			this.label35.Size = new System.Drawing.Size(72, 16);
			this.label35.TabIndex = 24;
			this.label35.Text = "* Audio pid:";
			// 
			// tbDVBCProvider
			// 
			this.tbDVBCProvider.Location = new System.Drawing.Point(160, 216);
			this.tbDVBCProvider.Name = "tbDVBCProvider";
			this.tbDVBCProvider.TabIndex = 23;
			this.tbDVBCProvider.Text = "";
			// 
			// label28
			// 
			this.label28.Location = new System.Drawing.Point(24, 216);
			this.label28.Name = "label28";
			this.label28.Size = new System.Drawing.Size(100, 16);
			this.label28.TabIndex = 22;
			this.label28.Text = "Provider:";
			// 
			// cbDVBCModulation
			// 
			this.cbDVBCModulation.Items.AddRange(new object[] {
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
			this.cbDVBCModulation.Location = new System.Drawing.Point(160, 192);
			this.cbDVBCModulation.Name = "cbDVBCModulation";
			this.cbDVBCModulation.Size = new System.Drawing.Size(121, 21);
			this.cbDVBCModulation.TabIndex = 21;
			this.cbDVBCModulation.SelectedIndexChanged += new System.EventHandler(this.cbDVBCModulation_SelectedIndexChanged);
			// 
			// cbDVBCInnerFeq
			// 
			this.cbDVBCInnerFeq.Items.AddRange(new object[] {
																												"Max",
																												"Not Defined",
																												"Not set",
																												"RS 204/188",
																												"ViterBi"});
			this.cbDVBCInnerFeq.Location = new System.Drawing.Point(160, 168);
			this.cbDVBCInnerFeq.Name = "cbDVBCInnerFeq";
			this.cbDVBCInnerFeq.Size = new System.Drawing.Size(121, 21);
			this.cbDVBCInnerFeq.TabIndex = 20;
			this.cbDVBCInnerFeq.SelectedIndexChanged += new System.EventHandler(this.cbDVBCInnerFeq_SelectedIndexChanged);
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(24, 192);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(100, 16);
			this.label19.TabIndex = 19;
			this.label19.Text = "Modulation";
			this.label19.Click += new System.EventHandler(this.label19_Click);
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(24, 168);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(100, 16);
			this.label18.TabIndex = 18;
			this.label18.Text = "InnerFEC";
			this.label18.Click += new System.EventHandler(this.label18_Click);
			// 
			// tbDVBCSR
			// 
			this.tbDVBCSR.Location = new System.Drawing.Point(160, 144);
			this.tbDVBCSR.Name = "tbDVBCSR";
			this.tbDVBCSR.TabIndex = 17;
			this.tbDVBCSR.Text = "";
			this.tbDVBCSR.TextChanged += new System.EventHandler(this.tbDVBCSR_TextChanged);
			// 
			// label17
			// 
			this.label17.ForeColor = System.Drawing.Color.Red;
			this.label17.Location = new System.Drawing.Point(24, 144);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(100, 16);
			this.label17.TabIndex = 16;
			this.label17.Text = "* Symbolrate";
			this.label17.Click += new System.EventHandler(this.label17_Click);
			// 
			// tbDVBCFreq
			// 
			this.tbDVBCFreq.Location = new System.Drawing.Point(160, 120);
			this.tbDVBCFreq.Name = "tbDVBCFreq";
			this.tbDVBCFreq.TabIndex = 15;
			this.tbDVBCFreq.Text = "";
			this.tbDVBCFreq.TextChanged += new System.EventHandler(this.tbDVBCFreq_TextChanged);
			// 
			// label13
			// 
			this.label13.ForeColor = System.Drawing.Color.Red;
			this.label13.Location = new System.Drawing.Point(24, 120);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(128, 16);
			this.label13.TabIndex = 14;
			this.label13.Text = "* Carrier Frequency (KHz)";
			this.label13.Click += new System.EventHandler(this.label13_Click);
			// 
			// tbDVBCTSID
			// 
			this.tbDVBCTSID.Location = new System.Drawing.Point(160, 96);
			this.tbDVBCTSID.Name = "tbDVBCTSID";
			this.tbDVBCTSID.TabIndex = 13;
			this.tbDVBCTSID.Text = "";
			this.tbDVBCTSID.TextChanged += new System.EventHandler(this.tbDVBCTSID_TextChanged);
			// 
			// label14
			// 
			this.label14.ForeColor = System.Drawing.Color.Red;
			this.label14.Location = new System.Drawing.Point(24, 96);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(104, 16);
			this.label14.TabIndex = 12;
			this.label14.Text = "* Transport ID:";
			this.label14.Click += new System.EventHandler(this.label14_Click);
			// 
			// tbDVBCSID
			// 
			this.tbDVBCSID.Location = new System.Drawing.Point(160, 72);
			this.tbDVBCSID.Name = "tbDVBCSID";
			this.tbDVBCSID.TabIndex = 11;
			this.tbDVBCSID.Text = "";
			this.tbDVBCSID.TextChanged += new System.EventHandler(this.tbDVBCSID_TextChanged);
			// 
			// label15
			// 
			this.label15.ForeColor = System.Drawing.Color.Red;
			this.label15.Location = new System.Drawing.Point(24, 72);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(96, 16);
			this.label15.TabIndex = 10;
			this.label15.Text = "* Service ID:";
			this.label15.Click += new System.EventHandler(this.label15_Click);
			// 
			// tbDVBCONID
			// 
			this.tbDVBCONID.Location = new System.Drawing.Point(160, 48);
			this.tbDVBCONID.Name = "tbDVBCONID";
			this.tbDVBCONID.Size = new System.Drawing.Size(56, 20);
			this.tbDVBCONID.TabIndex = 9;
			this.tbDVBCONID.Text = "";
			this.tbDVBCONID.TextChanged += new System.EventHandler(this.tbDVBCONID_TextChanged);
			// 
			// label16
			// 
			this.label16.ForeColor = System.Drawing.Color.Red;
			this.label16.Location = new System.Drawing.Point(24, 48);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(64, 16);
			this.label16.TabIndex = 8;
			this.label16.Text = "* Network ID:";
			this.label16.Click += new System.EventHandler(this.label16_Click);
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.label109);
			this.tabPage3.Controls.Add(this.tbDVBTPCR);
			this.tabPage3.Controls.Add(this.label105);
			this.tabPage3.Controls.Add(this.label100);
			this.tabPage3.Controls.Add(this.label58);
			this.tabPage3.Controls.Add(this.tbDVBTAudioLanguage3);
			this.tabPage3.Controls.Add(this.tbDVBTAudioLanguage2);
			this.tabPage3.Controls.Add(this.tbDVBTAudioLanguage1);
			this.tabPage3.Controls.Add(this.tbDVBTAudioLanguage);
			this.tabPage3.Controls.Add(this.label59);
			this.tabPage3.Controls.Add(this.label60);
			this.tabPage3.Controls.Add(this.label61);
			this.tabPage3.Controls.Add(this.tbDVBTAC3);
			this.tabPage3.Controls.Add(this.tbDVBTAudio3);
			this.tabPage3.Controls.Add(this.tbDVBTAudio2);
			this.tabPage3.Controls.Add(this.tbDVBTAudio1);
			this.tabPage3.Controls.Add(this.label62);
			this.tabPage3.Controls.Add(this.label63);
			this.tabPage3.Controls.Add(this.label64);
			this.tabPage3.Controls.Add(this.label65);
			this.tabPage3.Controls.Add(this.label49);
			this.tabPage3.Controls.Add(this.tbBandWidth);
			this.tabPage3.Controls.Add(this.tbDVBTPmtPid);
			this.tabPage3.Controls.Add(this.label47);
			this.tabPage3.Controls.Add(this.tbDVBTTeletextPid);
			this.tabPage3.Controls.Add(this.tbDVBTVideoPid);
			this.tabPage3.Controls.Add(this.tbDVBTAudioPid);
			this.tabPage3.Controls.Add(this.label32);
			this.tabPage3.Controls.Add(this.label31);
			this.tabPage3.Controls.Add(this.label30);
			this.tabPage3.Controls.Add(this.tbDVBTProvider);
			this.tabPage3.Controls.Add(this.label27);
			this.tabPage3.Controls.Add(this.tbDVBTFreq);
			this.tabPage3.Controls.Add(this.label12);
			this.tabPage3.Controls.Add(this.tbDVBTTSID);
			this.tabPage3.Controls.Add(this.label11);
			this.tabPage3.Controls.Add(this.tbDVBTSID);
			this.tabPage3.Controls.Add(this.label10);
			this.tabPage3.Controls.Add(this.tbDVBTONID);
			this.tabPage3.Controls.Add(this.label9);
			this.tabPage3.Controls.Add(this.label42);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(480, 382);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "DVB-T";
			this.tabPage3.Click += new System.EventHandler(this.tabPage3_Click);
			// 
			// label109
			// 
			this.label109.ForeColor = System.Drawing.Color.Red;
			this.label109.Location = new System.Drawing.Point(16, 352);
			this.label109.Name = "label109";
			this.label109.Size = new System.Drawing.Size(248, 16);
			this.label109.TabIndex = 68;
			this.label109.Text = "Fields with * and in red are required";
			// 
			// tbDVBTPCR
			// 
			this.tbDVBTPCR.Location = new System.Drawing.Point(360, 80);
			this.tbDVBTPCR.Name = "tbDVBTPCR";
			this.tbDVBTPCR.TabIndex = 67;
			this.tbDVBTPCR.Text = "";
			// 
			// label105
			// 
			this.label105.ForeColor = System.Drawing.Color.Red;
			this.label105.Location = new System.Drawing.Point(288, 80);
			this.label105.Name = "label105";
			this.label105.Size = new System.Drawing.Size(72, 16);
			this.label105.TabIndex = 66;
			this.label105.Text = "* PCR PID";
			// 
			// label100
			// 
			this.label100.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label100.Location = new System.Drawing.Point(16, 8);
			this.label100.Name = "label100";
			this.label100.Size = new System.Drawing.Size(272, 23);
			this.label100.TabIndex = 65;
			this.label100.Text = "Digital TV Terrestrial settings for this TV channel";
			// 
			// label58
			// 
			this.label58.Location = new System.Drawing.Point(288, 288);
			this.label58.Name = "label58";
			this.label58.Size = new System.Drawing.Size(72, 16);
			this.label58.TabIndex = 64;
			this.label58.Text = "Audio 3:";
			// 
			// tbDVBTAudioLanguage3
			// 
			this.tbDVBTAudioLanguage3.Location = new System.Drawing.Point(360, 288);
			this.tbDVBTAudioLanguage3.Name = "tbDVBTAudioLanguage3";
			this.tbDVBTAudioLanguage3.TabIndex = 63;
			this.tbDVBTAudioLanguage3.Text = "";
			// 
			// tbDVBTAudioLanguage2
			// 
			this.tbDVBTAudioLanguage2.Location = new System.Drawing.Point(360, 264);
			this.tbDVBTAudioLanguage2.Name = "tbDVBTAudioLanguage2";
			this.tbDVBTAudioLanguage2.TabIndex = 62;
			this.tbDVBTAudioLanguage2.Text = "";
			// 
			// tbDVBTAudioLanguage1
			// 
			this.tbDVBTAudioLanguage1.Location = new System.Drawing.Point(360, 240);
			this.tbDVBTAudioLanguage1.Name = "tbDVBTAudioLanguage1";
			this.tbDVBTAudioLanguage1.TabIndex = 61;
			this.tbDVBTAudioLanguage1.Text = "";
			// 
			// tbDVBTAudioLanguage
			// 
			this.tbDVBTAudioLanguage.Location = new System.Drawing.Point(360, 216);
			this.tbDVBTAudioLanguage.Name = "tbDVBTAudioLanguage";
			this.tbDVBTAudioLanguage.TabIndex = 60;
			this.tbDVBTAudioLanguage.Text = "";
			// 
			// label59
			// 
			this.label59.Location = new System.Drawing.Point(288, 264);
			this.label59.Name = "label59";
			this.label59.Size = new System.Drawing.Size(72, 16);
			this.label59.TabIndex = 59;
			this.label59.Text = "Audio 2:";
			// 
			// label60
			// 
			this.label60.Location = new System.Drawing.Point(288, 240);
			this.label60.Name = "label60";
			this.label60.Size = new System.Drawing.Size(72, 16);
			this.label60.TabIndex = 58;
			this.label60.Text = "Audio 1:";
			// 
			// label61
			// 
			this.label61.Location = new System.Drawing.Point(288, 216);
			this.label61.Name = "label61";
			this.label61.Size = new System.Drawing.Size(72, 16);
			this.label61.TabIndex = 57;
			this.label61.Text = "Audio :";
			// 
			// tbDVBTAC3
			// 
			this.tbDVBTAC3.Location = new System.Drawing.Point(360, 184);
			this.tbDVBTAC3.Name = "tbDVBTAC3";
			this.tbDVBTAC3.TabIndex = 56;
			this.tbDVBTAC3.Text = "";
			// 
			// tbDVBTAudio3
			// 
			this.tbDVBTAudio3.Location = new System.Drawing.Point(360, 160);
			this.tbDVBTAudio3.Name = "tbDVBTAudio3";
			this.tbDVBTAudio3.TabIndex = 55;
			this.tbDVBTAudio3.Text = "";
			// 
			// tbDVBTAudio2
			// 
			this.tbDVBTAudio2.Location = new System.Drawing.Point(360, 136);
			this.tbDVBTAudio2.Name = "tbDVBTAudio2";
			this.tbDVBTAudio2.TabIndex = 54;
			this.tbDVBTAudio2.Text = "";
			// 
			// tbDVBTAudio1
			// 
			this.tbDVBTAudio1.Location = new System.Drawing.Point(360, 112);
			this.tbDVBTAudio1.Name = "tbDVBTAudio1";
			this.tbDVBTAudio1.TabIndex = 53;
			this.tbDVBTAudio1.Text = "";
			// 
			// label62
			// 
			this.label62.Location = new System.Drawing.Point(288, 184);
			this.label62.Name = "label62";
			this.label62.Size = new System.Drawing.Size(72, 16);
			this.label62.TabIndex = 52;
			this.label62.Text = "AC3 Pid:";
			// 
			// label63
			// 
			this.label63.Location = new System.Drawing.Point(288, 160);
			this.label63.Name = "label63";
			this.label63.Size = new System.Drawing.Size(72, 16);
			this.label63.TabIndex = 51;
			this.label63.Text = "Audio Pid3:";
			// 
			// label64
			// 
			this.label64.Location = new System.Drawing.Point(288, 136);
			this.label64.Name = "label64";
			this.label64.Size = new System.Drawing.Size(72, 16);
			this.label64.TabIndex = 50;
			this.label64.Text = "Audio Pid2:";
			// 
			// label65
			// 
			this.label65.Location = new System.Drawing.Point(288, 112);
			this.label65.Name = "label65";
			this.label65.Size = new System.Drawing.Size(72, 16);
			this.label65.TabIndex = 49;
			this.label65.Text = "Audio Pid1:";
			// 
			// label49
			// 
			this.label49.ForeColor = System.Drawing.Color.Red;
			this.label49.Location = new System.Drawing.Point(16, 296);
			this.label49.Name = "label49";
			this.label49.TabIndex = 36;
			this.label49.Text = "* Bandwidth:";
			// 
			// tbBandWidth
			// 
			this.tbBandWidth.Location = new System.Drawing.Point(152, 296);
			this.tbBandWidth.Name = "tbBandWidth";
			this.tbBandWidth.TabIndex = 35;
			this.tbBandWidth.Text = "";
			// 
			// tbDVBTPmtPid
			// 
			this.tbDVBTPmtPid.Location = new System.Drawing.Point(152, 272);
			this.tbDVBTPmtPid.Name = "tbDVBTPmtPid";
			this.tbDVBTPmtPid.TabIndex = 34;
			this.tbDVBTPmtPid.Text = "";
			// 
			// label47
			// 
			this.label47.ForeColor = System.Drawing.Color.Red;
			this.label47.Location = new System.Drawing.Point(16, 272);
			this.label47.Name = "label47";
			this.label47.Size = new System.Drawing.Size(72, 16);
			this.label47.TabIndex = 33;
			this.label47.Text = "* PMT pid:";
			// 
			// tbDVBTTeletextPid
			// 
			this.tbDVBTTeletextPid.Location = new System.Drawing.Point(152, 248);
			this.tbDVBTTeletextPid.Name = "tbDVBTTeletextPid";
			this.tbDVBTTeletextPid.TabIndex = 15;
			this.tbDVBTTeletextPid.Text = "";
			// 
			// tbDVBTVideoPid
			// 
			this.tbDVBTVideoPid.Location = new System.Drawing.Point(152, 224);
			this.tbDVBTVideoPid.Name = "tbDVBTVideoPid";
			this.tbDVBTVideoPid.TabIndex = 14;
			this.tbDVBTVideoPid.Text = "";
			// 
			// tbDVBTAudioPid
			// 
			this.tbDVBTAudioPid.Location = new System.Drawing.Point(152, 200);
			this.tbDVBTAudioPid.Name = "tbDVBTAudioPid";
			this.tbDVBTAudioPid.TabIndex = 13;
			this.tbDVBTAudioPid.Text = "";
			// 
			// label32
			// 
			this.label32.Location = new System.Drawing.Point(16, 248);
			this.label32.Name = "label32";
			this.label32.Size = new System.Drawing.Size(72, 16);
			this.label32.TabIndex = 12;
			this.label32.Text = "Teletext pid:";
			// 
			// label31
			// 
			this.label31.ForeColor = System.Drawing.Color.Red;
			this.label31.Location = new System.Drawing.Point(16, 224);
			this.label31.Name = "label31";
			this.label31.Size = new System.Drawing.Size(72, 16);
			this.label31.TabIndex = 11;
			this.label31.Text = "* Video pid:";
			// 
			// label30
			// 
			this.label30.ForeColor = System.Drawing.Color.Red;
			this.label30.Location = new System.Drawing.Point(16, 200);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(72, 16);
			this.label30.TabIndex = 10;
			this.label30.Text = "* Audio pid:";
			// 
			// tbDVBTProvider
			// 
			this.tbDVBTProvider.Location = new System.Drawing.Point(152, 176);
			this.tbDVBTProvider.Name = "tbDVBTProvider";
			this.tbDVBTProvider.TabIndex = 9;
			this.tbDVBTProvider.Text = "";
			// 
			// label27
			// 
			this.label27.Location = new System.Drawing.Point(16, 176);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(100, 16);
			this.label27.TabIndex = 8;
			this.label27.Text = "Provider:";
			// 
			// tbDVBTFreq
			// 
			this.tbDVBTFreq.Location = new System.Drawing.Point(152, 152);
			this.tbDVBTFreq.Name = "tbDVBTFreq";
			this.tbDVBTFreq.TabIndex = 7;
			this.tbDVBTFreq.Text = "";
			this.tbDVBTFreq.TextChanged += new System.EventHandler(this.tbDVBTFreq_TextChanged);
			// 
			// label12
			// 
			this.label12.ForeColor = System.Drawing.Color.Red;
			this.label12.Location = new System.Drawing.Point(16, 152);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(136, 16);
			this.label12.TabIndex = 6;
			this.label12.Text = "* Carrier Frequency (KHz)";
			this.label12.Click += new System.EventHandler(this.label12_Click);
			// 
			// tbDVBTTSID
			// 
			this.tbDVBTTSID.Location = new System.Drawing.Point(152, 128);
			this.tbDVBTTSID.Name = "tbDVBTTSID";
			this.tbDVBTTSID.TabIndex = 5;
			this.tbDVBTTSID.Text = "";
			this.tbDVBTTSID.TextChanged += new System.EventHandler(this.tbDVBTTSID_TextChanged);
			// 
			// label11
			// 
			this.label11.ForeColor = System.Drawing.Color.Red;
			this.label11.Location = new System.Drawing.Point(16, 128);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(88, 16);
			this.label11.TabIndex = 4;
			this.label11.Text = "* Transport ID:";
			this.label11.Click += new System.EventHandler(this.label11_Click);
			// 
			// tbDVBTSID
			// 
			this.tbDVBTSID.Location = new System.Drawing.Point(152, 104);
			this.tbDVBTSID.Name = "tbDVBTSID";
			this.tbDVBTSID.TabIndex = 3;
			this.tbDVBTSID.Text = "";
			this.tbDVBTSID.TextChanged += new System.EventHandler(this.tbDVBTSID_TextChanged);
			// 
			// label10
			// 
			this.label10.ForeColor = System.Drawing.Color.Red;
			this.label10.Location = new System.Drawing.Point(16, 104);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(88, 16);
			this.label10.TabIndex = 2;
			this.label10.Text = "* Service ID:";
			this.label10.Click += new System.EventHandler(this.label10_Click);
			// 
			// tbDVBTONID
			// 
			this.tbDVBTONID.Location = new System.Drawing.Point(152, 80);
			this.tbDVBTONID.Name = "tbDVBTONID";
			this.tbDVBTONID.Size = new System.Drawing.Size(56, 20);
			this.tbDVBTONID.TabIndex = 1;
			this.tbDVBTONID.Text = "";
			this.tbDVBTONID.TextChanged += new System.EventHandler(this.tbDVBTONID_TextChanged);
			// 
			// label9
			// 
			this.label9.ForeColor = System.Drawing.Color.Red;
			this.label9.Location = new System.Drawing.Point(16, 80);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(88, 16);
			this.label9.TabIndex = 0;
			this.label9.Text = "* Network ID:";
			this.label9.Click += new System.EventHandler(this.label9_Click);
			// 
			// label42
			// 
			this.label42.Location = new System.Drawing.Point(208, 32);
			this.label42.Name = "label42";
			this.label42.Size = new System.Drawing.Size(256, 32);
			this.label42.TabIndex = 16;
			// 
			// tabPage5
			// 
			this.tabPage5.Controls.Add(this.label110);
			this.tabPage5.Controls.Add(this.tbDVBSPCR);
			this.tabPage5.Controls.Add(this.label106);
			this.tabPage5.Controls.Add(this.label101);
			this.tabPage5.Controls.Add(this.label66);
			this.tabPage5.Controls.Add(this.tbDVBSAudioLanguage3);
			this.tabPage5.Controls.Add(this.tbDVBSAudioLanguage2);
			this.tabPage5.Controls.Add(this.tbDVBSAudioLanguage1);
			this.tabPage5.Controls.Add(this.tbDVBSAudioLanguage);
			this.tabPage5.Controls.Add(this.label67);
			this.tabPage5.Controls.Add(this.label68);
			this.tabPage5.Controls.Add(this.label69);
			this.tabPage5.Controls.Add(this.tbDVBSAC3);
			this.tabPage5.Controls.Add(this.tbDVBSAudio3);
			this.tabPage5.Controls.Add(this.tbDVBSAudio2);
			this.tabPage5.Controls.Add(this.tbDVBSAudio1);
			this.tabPage5.Controls.Add(this.label70);
			this.tabPage5.Controls.Add(this.label71);
			this.tabPage5.Controls.Add(this.label72);
			this.tabPage5.Controls.Add(this.label73);
			this.tabPage5.Controls.Add(this.tbDVBSPmtPid);
			this.tabPage5.Controls.Add(this.label48);
			this.tabPage5.Controls.Add(this.label40);
			this.tabPage5.Controls.Add(this.tbDVBSECMpid);
			this.tabPage5.Controls.Add(this.label39);
			this.tabPage5.Controls.Add(this.tbDVBSTeletextPid);
			this.tabPage5.Controls.Add(this.tbDVBSVideoPid);
			this.tabPage5.Controls.Add(this.tbDVBSAudioPid);
			this.tabPage5.Controls.Add(this.label36);
			this.tabPage5.Controls.Add(this.label37);
			this.tabPage5.Controls.Add(this.label38);
			this.tabPage5.Controls.Add(this.tbDVBSProvider);
			this.tabPage5.Controls.Add(this.label29);
			this.tabPage5.Controls.Add(this.cbDVBSPolarisation);
			this.tabPage5.Controls.Add(this.cbDvbSInnerFec);
			this.tabPage5.Controls.Add(this.label20);
			this.tabPage5.Controls.Add(this.label21);
			this.tabPage5.Controls.Add(this.tbDVBSSymbolrate);
			this.tabPage5.Controls.Add(this.label22);
			this.tabPage5.Controls.Add(this.tbDVBSFreq);
			this.tabPage5.Controls.Add(this.label23);
			this.tabPage5.Controls.Add(this.tbDVBSTSID);
			this.tabPage5.Controls.Add(this.label24);
			this.tabPage5.Controls.Add(this.tbDVBSSID);
			this.tabPage5.Controls.Add(this.label25);
			this.tabPage5.Controls.Add(this.tbDVBSONID);
			this.tabPage5.Controls.Add(this.label26);
			this.tabPage5.Location = new System.Drawing.Point(4, 22);
			this.tabPage5.Name = "tabPage5";
			this.tabPage5.Size = new System.Drawing.Size(480, 382);
			this.tabPage5.TabIndex = 4;
			this.tabPage5.Text = "DVB-S";
			this.tabPage5.Click += new System.EventHandler(this.tabPage5_Click);
			// 
			// label110
			// 
			this.label110.ForeColor = System.Drawing.Color.Red;
			this.label110.Location = new System.Drawing.Point(24, 352);
			this.label110.Name = "label110";
			this.label110.Size = new System.Drawing.Size(248, 16);
			this.label110.TabIndex = 84;
			this.label110.Text = "Fields with * and in red are required";
			// 
			// tbDVBSPCR
			// 
			this.tbDVBSPCR.Location = new System.Drawing.Point(368, 104);
			this.tbDVBSPCR.Name = "tbDVBSPCR";
			this.tbDVBSPCR.TabIndex = 83;
			this.tbDVBSPCR.Text = "";
			// 
			// label106
			// 
			this.label106.ForeColor = System.Drawing.Color.Red;
			this.label106.Location = new System.Drawing.Point(296, 104);
			this.label106.Name = "label106";
			this.label106.Size = new System.Drawing.Size(72, 16);
			this.label106.TabIndex = 82;
			this.label106.Text = "* PCR Pid:";
			// 
			// label101
			// 
			this.label101.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label101.Location = new System.Drawing.Point(24, 8);
			this.label101.Name = "label101";
			this.label101.Size = new System.Drawing.Size(272, 23);
			this.label101.TabIndex = 81;
			this.label101.Text = "Digital TV Satelite settings for this TV channel";
			// 
			// label66
			// 
			this.label66.Location = new System.Drawing.Point(296, 312);
			this.label66.Name = "label66";
			this.label66.Size = new System.Drawing.Size(72, 16);
			this.label66.TabIndex = 80;
			this.label66.Text = "Audio 3:";
			// 
			// tbDVBSAudioLanguage3
			// 
			this.tbDVBSAudioLanguage3.Location = new System.Drawing.Point(368, 312);
			this.tbDVBSAudioLanguage3.Name = "tbDVBSAudioLanguage3";
			this.tbDVBSAudioLanguage3.TabIndex = 79;
			this.tbDVBSAudioLanguage3.Text = "";
			// 
			// tbDVBSAudioLanguage2
			// 
			this.tbDVBSAudioLanguage2.Location = new System.Drawing.Point(368, 288);
			this.tbDVBSAudioLanguage2.Name = "tbDVBSAudioLanguage2";
			this.tbDVBSAudioLanguage2.TabIndex = 78;
			this.tbDVBSAudioLanguage2.Text = "";
			// 
			// tbDVBSAudioLanguage1
			// 
			this.tbDVBSAudioLanguage1.Location = new System.Drawing.Point(368, 264);
			this.tbDVBSAudioLanguage1.Name = "tbDVBSAudioLanguage1";
			this.tbDVBSAudioLanguage1.TabIndex = 77;
			this.tbDVBSAudioLanguage1.Text = "";
			// 
			// tbDVBSAudioLanguage
			// 
			this.tbDVBSAudioLanguage.Location = new System.Drawing.Point(368, 240);
			this.tbDVBSAudioLanguage.Name = "tbDVBSAudioLanguage";
			this.tbDVBSAudioLanguage.TabIndex = 76;
			this.tbDVBSAudioLanguage.Text = "";
			// 
			// label67
			// 
			this.label67.Location = new System.Drawing.Point(296, 288);
			this.label67.Name = "label67";
			this.label67.Size = new System.Drawing.Size(72, 16);
			this.label67.TabIndex = 75;
			this.label67.Text = "Audio 2:";
			// 
			// label68
			// 
			this.label68.Location = new System.Drawing.Point(296, 264);
			this.label68.Name = "label68";
			this.label68.Size = new System.Drawing.Size(72, 16);
			this.label68.TabIndex = 74;
			this.label68.Text = "Audio 1:";
			// 
			// label69
			// 
			this.label69.Location = new System.Drawing.Point(296, 240);
			this.label69.Name = "label69";
			this.label69.Size = new System.Drawing.Size(72, 16);
			this.label69.TabIndex = 73;
			this.label69.Text = "Audio :";
			// 
			// tbDVBSAC3
			// 
			this.tbDVBSAC3.Location = new System.Drawing.Point(368, 208);
			this.tbDVBSAC3.Name = "tbDVBSAC3";
			this.tbDVBSAC3.TabIndex = 72;
			this.tbDVBSAC3.Text = "";
			// 
			// tbDVBSAudio3
			// 
			this.tbDVBSAudio3.Location = new System.Drawing.Point(368, 184);
			this.tbDVBSAudio3.Name = "tbDVBSAudio3";
			this.tbDVBSAudio3.TabIndex = 71;
			this.tbDVBSAudio3.Text = "";
			// 
			// tbDVBSAudio2
			// 
			this.tbDVBSAudio2.Location = new System.Drawing.Point(368, 160);
			this.tbDVBSAudio2.Name = "tbDVBSAudio2";
			this.tbDVBSAudio2.TabIndex = 70;
			this.tbDVBSAudio2.Text = "";
			// 
			// tbDVBSAudio1
			// 
			this.tbDVBSAudio1.Location = new System.Drawing.Point(368, 136);
			this.tbDVBSAudio1.Name = "tbDVBSAudio1";
			this.tbDVBSAudio1.TabIndex = 69;
			this.tbDVBSAudio1.Text = "";
			// 
			// label70
			// 
			this.label70.Location = new System.Drawing.Point(296, 208);
			this.label70.Name = "label70";
			this.label70.Size = new System.Drawing.Size(72, 16);
			this.label70.TabIndex = 68;
			this.label70.Text = "AC3 Pid:";
			// 
			// label71
			// 
			this.label71.Location = new System.Drawing.Point(296, 184);
			this.label71.Name = "label71";
			this.label71.Size = new System.Drawing.Size(72, 16);
			this.label71.TabIndex = 67;
			this.label71.Text = "Audio Pid3:";
			// 
			// label72
			// 
			this.label72.Location = new System.Drawing.Point(296, 160);
			this.label72.Name = "label72";
			this.label72.Size = new System.Drawing.Size(72, 16);
			this.label72.TabIndex = 66;
			this.label72.Text = "Audio Pid2:";
			// 
			// label73
			// 
			this.label73.Location = new System.Drawing.Point(296, 136);
			this.label73.Name = "label73";
			this.label73.Size = new System.Drawing.Size(72, 16);
			this.label73.TabIndex = 65;
			this.label73.Text = "Audio Pid1:";
			// 
			// tbDVBSPmtPid
			// 
			this.tbDVBSPmtPid.Location = new System.Drawing.Point(160, 320);
			this.tbDVBSPmtPid.Name = "tbDVBSPmtPid";
			this.tbDVBSPmtPid.TabIndex = 48;
			this.tbDVBSPmtPid.Text = "";
			// 
			// label48
			// 
			this.label48.ForeColor = System.Drawing.Color.Red;
			this.label48.Location = new System.Drawing.Point(24, 320);
			this.label48.Name = "label48";
			this.label48.Size = new System.Drawing.Size(72, 16);
			this.label48.TabIndex = 47;
			this.label48.Text = "* PMT pid:";
			// 
			// label40
			// 
			this.label40.Location = new System.Drawing.Point(232, 32);
			this.label40.Name = "label40";
			this.label40.Size = new System.Drawing.Size(152, 16);
			this.label40.TabIndex = 46;
			// 
			// tbDVBSECMpid
			// 
			this.tbDVBSECMpid.Location = new System.Drawing.Point(160, 296);
			this.tbDVBSECMpid.Name = "tbDVBSECMpid";
			this.tbDVBSECMpid.TabIndex = 45;
			this.tbDVBSECMpid.Text = "";
			// 
			// label39
			// 
			this.label39.Location = new System.Drawing.Point(24, 296);
			this.label39.Name = "label39";
			this.label39.Size = new System.Drawing.Size(100, 16);
			this.label39.TabIndex = 44;
			this.label39.Text = "ECM pid:";
			// 
			// tbDVBSTeletextPid
			// 
			this.tbDVBSTeletextPid.Location = new System.Drawing.Point(160, 272);
			this.tbDVBSTeletextPid.Name = "tbDVBSTeletextPid";
			this.tbDVBSTeletextPid.TabIndex = 43;
			this.tbDVBSTeletextPid.Text = "";
			// 
			// tbDVBSVideoPid
			// 
			this.tbDVBSVideoPid.Location = new System.Drawing.Point(160, 248);
			this.tbDVBSVideoPid.Name = "tbDVBSVideoPid";
			this.tbDVBSVideoPid.TabIndex = 42;
			this.tbDVBSVideoPid.Text = "";
			// 
			// tbDVBSAudioPid
			// 
			this.tbDVBSAudioPid.Location = new System.Drawing.Point(160, 224);
			this.tbDVBSAudioPid.Name = "tbDVBSAudioPid";
			this.tbDVBSAudioPid.TabIndex = 41;
			this.tbDVBSAudioPid.Text = "";
			// 
			// label36
			// 
			this.label36.Location = new System.Drawing.Point(24, 272);
			this.label36.Name = "label36";
			this.label36.Size = new System.Drawing.Size(72, 16);
			this.label36.TabIndex = 40;
			this.label36.Text = "Teletext pid:";
			// 
			// label37
			// 
			this.label37.ForeColor = System.Drawing.Color.Red;
			this.label37.Location = new System.Drawing.Point(24, 248);
			this.label37.Name = "label37";
			this.label37.Size = new System.Drawing.Size(72, 16);
			this.label37.TabIndex = 39;
			this.label37.Text = "* Video pid:";
			// 
			// label38
			// 
			this.label38.ForeColor = System.Drawing.Color.Red;
			this.label38.Location = new System.Drawing.Point(24, 224);
			this.label38.Name = "label38";
			this.label38.Size = new System.Drawing.Size(72, 16);
			this.label38.TabIndex = 38;
			this.label38.Text = "* Audio pid:";
			// 
			// tbDVBSProvider
			// 
			this.tbDVBSProvider.Location = new System.Drawing.Point(160, 200);
			this.tbDVBSProvider.Name = "tbDVBSProvider";
			this.tbDVBSProvider.TabIndex = 37;
			this.tbDVBSProvider.Text = "";
			// 
			// label29
			// 
			this.label29.Location = new System.Drawing.Point(24, 200);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(100, 16);
			this.label29.TabIndex = 36;
			this.label29.Text = "Provider:";
			// 
			// cbDVBSPolarisation
			// 
			this.cbDVBSPolarisation.Items.AddRange(new object[] {
																														"Horizontal",
																														"Vertical"});
			this.cbDVBSPolarisation.Location = new System.Drawing.Point(160, 176);
			this.cbDVBSPolarisation.Name = "cbDVBSPolarisation";
			this.cbDVBSPolarisation.Size = new System.Drawing.Size(121, 21);
			this.cbDVBSPolarisation.TabIndex = 35;
			this.cbDVBSPolarisation.SelectedIndexChanged += new System.EventHandler(this.cbDVBSPolarisation_SelectedIndexChanged);
			// 
			// cbDvbSInnerFec
			// 
			this.cbDvbSInnerFec.Items.AddRange(new object[] {
																												"Max",
																												"Not Defined",
																												"Not Set",
																												"RS 204/188",
																												"ViterBi"});
			this.cbDvbSInnerFec.Location = new System.Drawing.Point(160, 152);
			this.cbDvbSInnerFec.Name = "cbDvbSInnerFec";
			this.cbDvbSInnerFec.Size = new System.Drawing.Size(121, 21);
			this.cbDvbSInnerFec.TabIndex = 34;
			this.cbDvbSInnerFec.SelectedIndexChanged += new System.EventHandler(this.cbDvbSInnerFec_SelectedIndexChanged);
			// 
			// label20
			// 
			this.label20.ForeColor = System.Drawing.Color.Red;
			this.label20.Location = new System.Drawing.Point(24, 176);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(100, 16);
			this.label20.TabIndex = 33;
			this.label20.Text = "* Polarisation";
			this.label20.Click += new System.EventHandler(this.label20_Click);
			// 
			// label21
			// 
			this.label21.Location = new System.Drawing.Point(24, 152);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(100, 16);
			this.label21.TabIndex = 32;
			this.label21.Text = "InnerFEC";
			this.label21.Click += new System.EventHandler(this.label21_Click);
			// 
			// tbDVBSSymbolrate
			// 
			this.tbDVBSSymbolrate.Location = new System.Drawing.Point(160, 128);
			this.tbDVBSSymbolrate.Name = "tbDVBSSymbolrate";
			this.tbDVBSSymbolrate.TabIndex = 31;
			this.tbDVBSSymbolrate.Text = "";
			this.tbDVBSSymbolrate.TextChanged += new System.EventHandler(this.tbDVBSSymbolrate_TextChanged);
			// 
			// label22
			// 
			this.label22.ForeColor = System.Drawing.Color.Red;
			this.label22.Location = new System.Drawing.Point(24, 128);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(100, 16);
			this.label22.TabIndex = 30;
			this.label22.Text = "* Symbolrate";
			this.label22.Click += new System.EventHandler(this.label22_Click);
			// 
			// tbDVBSFreq
			// 
			this.tbDVBSFreq.Location = new System.Drawing.Point(160, 104);
			this.tbDVBSFreq.Name = "tbDVBSFreq";
			this.tbDVBSFreq.TabIndex = 29;
			this.tbDVBSFreq.Text = "";
			this.tbDVBSFreq.TextChanged += new System.EventHandler(this.tbDVBSFreq_TextChanged);
			// 
			// label23
			// 
			this.label23.ForeColor = System.Drawing.Color.Red;
			this.label23.Location = new System.Drawing.Point(24, 104);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(136, 16);
			this.label23.TabIndex = 28;
			this.label23.Text = "* Carrier Frequency (KHz)";
			this.label23.Click += new System.EventHandler(this.label23_Click);
			// 
			// tbDVBSTSID
			// 
			this.tbDVBSTSID.Location = new System.Drawing.Point(160, 80);
			this.tbDVBSTSID.Name = "tbDVBSTSID";
			this.tbDVBSTSID.TabIndex = 27;
			this.tbDVBSTSID.Text = "";
			this.tbDVBSTSID.TextChanged += new System.EventHandler(this.tbDVBSTSID_TextChanged);
			// 
			// label24
			// 
			this.label24.ForeColor = System.Drawing.Color.Red;
			this.label24.Location = new System.Drawing.Point(24, 80);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(88, 16);
			this.label24.TabIndex = 26;
			this.label24.Text = "* Transport ID:";
			this.label24.Click += new System.EventHandler(this.label24_Click);
			// 
			// tbDVBSSID
			// 
			this.tbDVBSSID.Location = new System.Drawing.Point(160, 56);
			this.tbDVBSSID.Name = "tbDVBSSID";
			this.tbDVBSSID.TabIndex = 25;
			this.tbDVBSSID.Text = "";
			this.tbDVBSSID.TextChanged += new System.EventHandler(this.tbDVBSSID_TextChanged);
			// 
			// label25
			// 
			this.label25.ForeColor = System.Drawing.Color.Red;
			this.label25.Location = new System.Drawing.Point(24, 56);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(80, 16);
			this.label25.TabIndex = 24;
			this.label25.Text = "* Service ID:";
			this.label25.Click += new System.EventHandler(this.label25_Click);
			// 
			// tbDVBSONID
			// 
			this.tbDVBSONID.Location = new System.Drawing.Point(160, 32);
			this.tbDVBSONID.Name = "tbDVBSONID";
			this.tbDVBSONID.Size = new System.Drawing.Size(64, 20);
			this.tbDVBSONID.TabIndex = 23;
			this.tbDVBSONID.Text = "";
			this.tbDVBSONID.TextChanged += new System.EventHandler(this.tbDVBSONID_TextChanged);
			// 
			// label26
			// 
			this.label26.ForeColor = System.Drawing.Color.Red;
			this.label26.Location = new System.Drawing.Point(24, 32);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(80, 16);
			this.label26.TabIndex = 22;
			this.label26.Text = "* Network ID:";
			this.label26.Click += new System.EventHandler(this.label26_Click);
			// 
			// tabPage7
			// 
			this.tabPage7.Controls.Add(this.label111);
			this.tabPage7.Controls.Add(this.tbATSCPCR);
			this.tabPage7.Controls.Add(this.label107);
			this.tabPage7.Controls.Add(this.label102);
			this.tabPage7.Controls.Add(this.tbATSCMinor);
			this.tabPage7.Controls.Add(this.label83);
			this.tabPage7.Controls.Add(this.label74);
			this.tabPage7.Controls.Add(this.tbATSCAudioLanguage3);
			this.tabPage7.Controls.Add(this.tbATSCAudioLanguage2);
			this.tabPage7.Controls.Add(this.tbATSCAudioLanguage1);
			this.tabPage7.Controls.Add(this.tbATSCAudioLanguage);
			this.tabPage7.Controls.Add(this.label75);
			this.tabPage7.Controls.Add(this.label76);
			this.tabPage7.Controls.Add(this.label77);
			this.tabPage7.Controls.Add(this.tbATSCAC3Pid);
			this.tabPage7.Controls.Add(this.tbATSCAudio3Pid);
			this.tabPage7.Controls.Add(this.tbATSCAudio2Pid);
			this.tabPage7.Controls.Add(this.tbATSCAudio1Pid);
			this.tabPage7.Controls.Add(this.label78);
			this.tabPage7.Controls.Add(this.label79);
			this.tabPage7.Controls.Add(this.label80);
			this.tabPage7.Controls.Add(this.label81);
			this.tabPage7.Controls.Add(this.tbATSCPMTPid);
			this.tabPage7.Controls.Add(this.label82);
			this.tabPage7.Controls.Add(this.tbATSCTeletextPid);
			this.tabPage7.Controls.Add(this.tbATSCVideoPid);
			this.tabPage7.Controls.Add(this.tbATSCAudioPid);
			this.tabPage7.Controls.Add(this.label84);
			this.tabPage7.Controls.Add(this.label85);
			this.tabPage7.Controls.Add(this.label86);
			this.tabPage7.Controls.Add(this.tbATSCProvider);
			this.tabPage7.Controls.Add(this.label87);
			this.tabPage7.Controls.Add(this.cbATSCModulation);
			this.tabPage7.Controls.Add(this.cbATSCInnerFec);
			this.tabPage7.Controls.Add(this.label88);
			this.tabPage7.Controls.Add(this.label89);
			this.tabPage7.Controls.Add(this.tbATSCSymbolRate);
			this.tabPage7.Controls.Add(this.label90);
			this.tabPage7.Controls.Add(this.tbATSCFrequency);
			this.tabPage7.Controls.Add(this.label91);
			this.tabPage7.Controls.Add(this.tbATSCTSID);
			this.tabPage7.Controls.Add(this.label92);
			this.tabPage7.Controls.Add(this.tbATSCMajor);
			this.tabPage7.Controls.Add(this.label93);
			this.tabPage7.Controls.Add(this.tbATSCPhysicalChannel);
			this.tabPage7.Controls.Add(this.label94);
			this.tabPage7.Location = new System.Drawing.Point(4, 22);
			this.tabPage7.Name = "tabPage7";
			this.tabPage7.Size = new System.Drawing.Size(480, 382);
			this.tabPage7.TabIndex = 6;
			this.tabPage7.Text = "ATSC";
			// 
			// label111
			// 
			this.label111.ForeColor = System.Drawing.Color.Red;
			this.label111.Location = new System.Drawing.Point(24, 360);
			this.label111.Name = "label111";
			this.label111.Size = new System.Drawing.Size(248, 16);
			this.label111.TabIndex = 95;
			this.label111.Text = "Fields with * and in red are required";
			// 
			// tbATSCPCR
			// 
			this.tbATSCPCR.Location = new System.Drawing.Point(360, 96);
			this.tbATSCPCR.Name = "tbATSCPCR";
			this.tbATSCPCR.TabIndex = 94;
			this.tbATSCPCR.Text = "";
			// 
			// label107
			// 
			this.label107.Location = new System.Drawing.Point(288, 96);
			this.label107.Name = "label107";
			this.label107.Size = new System.Drawing.Size(72, 16);
			this.label107.TabIndex = 93;
			this.label107.Text = "PCR Pid:";
			// 
			// label102
			// 
			this.label102.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label102.Location = new System.Drawing.Point(24, 8);
			this.label102.Name = "label102";
			this.label102.Size = new System.Drawing.Size(272, 23);
			this.label102.TabIndex = 92;
			this.label102.Text = "Digital TV (ATSC) settings for this TV channel";
			// 
			// tbATSCMinor
			// 
			this.tbATSCMinor.Location = new System.Drawing.Point(160, 88);
			this.tbATSCMinor.Name = "tbATSCMinor";
			this.tbATSCMinor.TabIndex = 91;
			this.tbATSCMinor.Text = "";
			// 
			// label83
			// 
			this.label83.ForeColor = System.Drawing.Color.Red;
			this.label83.Location = new System.Drawing.Point(16, 88);
			this.label83.Name = "label83";
			this.label83.Size = new System.Drawing.Size(88, 16);
			this.label83.TabIndex = 90;
			this.label83.Text = "* Minor channel";
			// 
			// label74
			// 
			this.label74.Location = new System.Drawing.Point(286, 304);
			this.label74.Name = "label74";
			this.label74.Size = new System.Drawing.Size(72, 16);
			this.label74.TabIndex = 89;
			this.label74.Text = "Audio 3:";
			// 
			// tbATSCAudioLanguage3
			// 
			this.tbATSCAudioLanguage3.Location = new System.Drawing.Point(358, 304);
			this.tbATSCAudioLanguage3.Name = "tbATSCAudioLanguage3";
			this.tbATSCAudioLanguage3.TabIndex = 88;
			this.tbATSCAudioLanguage3.Text = "";
			// 
			// tbATSCAudioLanguage2
			// 
			this.tbATSCAudioLanguage2.Location = new System.Drawing.Point(358, 280);
			this.tbATSCAudioLanguage2.Name = "tbATSCAudioLanguage2";
			this.tbATSCAudioLanguage2.TabIndex = 87;
			this.tbATSCAudioLanguage2.Text = "";
			// 
			// tbATSCAudioLanguage1
			// 
			this.tbATSCAudioLanguage1.Location = new System.Drawing.Point(358, 256);
			this.tbATSCAudioLanguage1.Name = "tbATSCAudioLanguage1";
			this.tbATSCAudioLanguage1.TabIndex = 86;
			this.tbATSCAudioLanguage1.Text = "";
			// 
			// tbATSCAudioLanguage
			// 
			this.tbATSCAudioLanguage.Location = new System.Drawing.Point(358, 232);
			this.tbATSCAudioLanguage.Name = "tbATSCAudioLanguage";
			this.tbATSCAudioLanguage.TabIndex = 85;
			this.tbATSCAudioLanguage.Text = "";
			// 
			// label75
			// 
			this.label75.Location = new System.Drawing.Point(286, 280);
			this.label75.Name = "label75";
			this.label75.Size = new System.Drawing.Size(72, 16);
			this.label75.TabIndex = 84;
			this.label75.Text = "Audio 2:";
			// 
			// label76
			// 
			this.label76.Location = new System.Drawing.Point(286, 256);
			this.label76.Name = "label76";
			this.label76.Size = new System.Drawing.Size(72, 16);
			this.label76.TabIndex = 83;
			this.label76.Text = "Audio 1:";
			// 
			// label77
			// 
			this.label77.Location = new System.Drawing.Point(286, 232);
			this.label77.Name = "label77";
			this.label77.Size = new System.Drawing.Size(72, 16);
			this.label77.TabIndex = 82;
			this.label77.Text = "Audio :";
			// 
			// tbATSCAC3Pid
			// 
			this.tbATSCAC3Pid.Location = new System.Drawing.Point(358, 200);
			this.tbATSCAC3Pid.Name = "tbATSCAC3Pid";
			this.tbATSCAC3Pid.TabIndex = 81;
			this.tbATSCAC3Pid.Text = "";
			// 
			// tbATSCAudio3Pid
			// 
			this.tbATSCAudio3Pid.Location = new System.Drawing.Point(358, 176);
			this.tbATSCAudio3Pid.Name = "tbATSCAudio3Pid";
			this.tbATSCAudio3Pid.TabIndex = 80;
			this.tbATSCAudio3Pid.Text = "";
			// 
			// tbATSCAudio2Pid
			// 
			this.tbATSCAudio2Pid.Location = new System.Drawing.Point(358, 152);
			this.tbATSCAudio2Pid.Name = "tbATSCAudio2Pid";
			this.tbATSCAudio2Pid.TabIndex = 79;
			this.tbATSCAudio2Pid.Text = "";
			// 
			// tbATSCAudio1Pid
			// 
			this.tbATSCAudio1Pid.Location = new System.Drawing.Point(358, 128);
			this.tbATSCAudio1Pid.Name = "tbATSCAudio1Pid";
			this.tbATSCAudio1Pid.TabIndex = 78;
			this.tbATSCAudio1Pid.Text = "";
			// 
			// label78
			// 
			this.label78.Location = new System.Drawing.Point(286, 200);
			this.label78.Name = "label78";
			this.label78.Size = new System.Drawing.Size(72, 16);
			this.label78.TabIndex = 77;
			this.label78.Text = "AC3 Pid:";
			// 
			// label79
			// 
			this.label79.Location = new System.Drawing.Point(286, 176);
			this.label79.Name = "label79";
			this.label79.Size = new System.Drawing.Size(72, 16);
			this.label79.TabIndex = 76;
			this.label79.Text = "Audio Pid3:";
			// 
			// label80
			// 
			this.label80.Location = new System.Drawing.Point(286, 152);
			this.label80.Name = "label80";
			this.label80.Size = new System.Drawing.Size(72, 16);
			this.label80.TabIndex = 75;
			this.label80.Text = "Audio Pid2:";
			// 
			// label81
			// 
			this.label81.Location = new System.Drawing.Point(286, 128);
			this.label81.Name = "label81";
			this.label81.Size = new System.Drawing.Size(72, 16);
			this.label81.TabIndex = 74;
			this.label81.Text = "Audio Pid1:";
			// 
			// tbATSCPMTPid
			// 
			this.tbATSCPMTPid.Location = new System.Drawing.Point(160, 328);
			this.tbATSCPMTPid.Name = "tbATSCPMTPid";
			this.tbATSCPMTPid.TabIndex = 73;
			this.tbATSCPMTPid.Text = "";
			// 
			// label82
			// 
			this.label82.Location = new System.Drawing.Point(16, 328);
			this.label82.Name = "label82";
			this.label82.Size = new System.Drawing.Size(64, 16);
			this.label82.TabIndex = 72;
			this.label82.Text = "PMT pid:";
			// 
			// tbATSCTeletextPid
			// 
			this.tbATSCTeletextPid.Location = new System.Drawing.Point(160, 304);
			this.tbATSCTeletextPid.Name = "tbATSCTeletextPid";
			this.tbATSCTeletextPid.TabIndex = 70;
			this.tbATSCTeletextPid.Text = "";
			// 
			// tbATSCVideoPid
			// 
			this.tbATSCVideoPid.Location = new System.Drawing.Point(160, 280);
			this.tbATSCVideoPid.Name = "tbATSCVideoPid";
			this.tbATSCVideoPid.TabIndex = 69;
			this.tbATSCVideoPid.Text = "";
			// 
			// tbATSCAudioPid
			// 
			this.tbATSCAudioPid.Location = new System.Drawing.Point(160, 256);
			this.tbATSCAudioPid.Name = "tbATSCAudioPid";
			this.tbATSCAudioPid.TabIndex = 68;
			this.tbATSCAudioPid.Text = "";
			// 
			// label84
			// 
			this.label84.Location = new System.Drawing.Point(16, 304);
			this.label84.Name = "label84";
			this.label84.Size = new System.Drawing.Size(72, 16);
			this.label84.TabIndex = 67;
			this.label84.Text = "Teletext pid:";
			// 
			// label85
			// 
			this.label85.ForeColor = System.Drawing.Color.Red;
			this.label85.Location = new System.Drawing.Point(16, 280);
			this.label85.Name = "label85";
			this.label85.Size = new System.Drawing.Size(72, 16);
			this.label85.TabIndex = 66;
			this.label85.Text = "* Video pid:";
			// 
			// label86
			// 
			this.label86.ForeColor = System.Drawing.Color.Red;
			this.label86.Location = new System.Drawing.Point(16, 256);
			this.label86.Name = "label86";
			this.label86.Size = new System.Drawing.Size(72, 16);
			this.label86.TabIndex = 65;
			this.label86.Text = "* Audio pid:";
			// 
			// tbATSCProvider
			// 
			this.tbATSCProvider.Location = new System.Drawing.Point(158, 232);
			this.tbATSCProvider.Name = "tbATSCProvider";
			this.tbATSCProvider.TabIndex = 64;
			this.tbATSCProvider.Text = "";
			// 
			// label87
			// 
			this.label87.Location = new System.Drawing.Point(16, 232);
			this.label87.Name = "label87";
			this.label87.Size = new System.Drawing.Size(100, 16);
			this.label87.TabIndex = 63;
			this.label87.Text = "Provider:";
			// 
			// cbATSCModulation
			// 
			this.cbATSCModulation.Items.AddRange(new object[] {
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
			this.cbATSCModulation.Location = new System.Drawing.Point(158, 208);
			this.cbATSCModulation.Name = "cbATSCModulation";
			this.cbATSCModulation.Size = new System.Drawing.Size(121, 21);
			this.cbATSCModulation.TabIndex = 62;
			// 
			// cbATSCInnerFec
			// 
			this.cbATSCInnerFec.Items.AddRange(new object[] {
																												"Max",
																												"Not Defined",
																												"Not set",
																												"RS 204/188",
																												"ViterBi"});
			this.cbATSCInnerFec.Location = new System.Drawing.Point(158, 184);
			this.cbATSCInnerFec.Name = "cbATSCInnerFec";
			this.cbATSCInnerFec.Size = new System.Drawing.Size(121, 21);
			this.cbATSCInnerFec.TabIndex = 61;
			// 
			// label88
			// 
			this.label88.ForeColor = System.Drawing.Color.Red;
			this.label88.Location = new System.Drawing.Point(16, 208);
			this.label88.Name = "label88";
			this.label88.Size = new System.Drawing.Size(100, 16);
			this.label88.TabIndex = 60;
			this.label88.Text = "* Modulation";
			// 
			// label89
			// 
			this.label89.Location = new System.Drawing.Point(16, 184);
			this.label89.Name = "label89";
			this.label89.Size = new System.Drawing.Size(100, 16);
			this.label89.TabIndex = 59;
			this.label89.Text = "InnerFEC";
			// 
			// tbATSCSymbolRate
			// 
			this.tbATSCSymbolRate.Location = new System.Drawing.Point(158, 160);
			this.tbATSCSymbolRate.Name = "tbATSCSymbolRate";
			this.tbATSCSymbolRate.TabIndex = 58;
			this.tbATSCSymbolRate.Text = "";
			// 
			// label90
			// 
			this.label90.Location = new System.Drawing.Point(16, 160);
			this.label90.Name = "label90";
			this.label90.Size = new System.Drawing.Size(100, 16);
			this.label90.TabIndex = 57;
			this.label90.Text = "Symbolrate";
			// 
			// tbATSCFrequency
			// 
			this.tbATSCFrequency.Location = new System.Drawing.Point(158, 136);
			this.tbATSCFrequency.Name = "tbATSCFrequency";
			this.tbATSCFrequency.TabIndex = 56;
			this.tbATSCFrequency.Text = "";
			// 
			// label91
			// 
			this.label91.Location = new System.Drawing.Point(16, 136);
			this.label91.Name = "label91";
			this.label91.Size = new System.Drawing.Size(128, 16);
			this.label91.TabIndex = 55;
			this.label91.Text = "Carrier Frequency (KHz)";
			// 
			// tbATSCTSID
			// 
			this.tbATSCTSID.Location = new System.Drawing.Point(158, 112);
			this.tbATSCTSID.Name = "tbATSCTSID";
			this.tbATSCTSID.TabIndex = 54;
			this.tbATSCTSID.Text = "";
			// 
			// label92
			// 
			this.label92.ForeColor = System.Drawing.Color.Red;
			this.label92.Location = new System.Drawing.Point(16, 112);
			this.label92.Name = "label92";
			this.label92.Size = new System.Drawing.Size(72, 16);
			this.label92.TabIndex = 53;
			this.label92.Text = "* Transport ID:";
			// 
			// tbATSCMajor
			// 
			this.tbATSCMajor.Location = new System.Drawing.Point(160, 64);
			this.tbATSCMajor.Name = "tbATSCMajor";
			this.tbATSCMajor.TabIndex = 52;
			this.tbATSCMajor.Text = "";
			// 
			// label93
			// 
			this.label93.ForeColor = System.Drawing.Color.Red;
			this.label93.Location = new System.Drawing.Point(16, 64);
			this.label93.Name = "label93";
			this.label93.Size = new System.Drawing.Size(88, 16);
			this.label93.TabIndex = 51;
			this.label93.Text = "* Major channel";
			// 
			// tbATSCPhysicalChannel
			// 
			this.tbATSCPhysicalChannel.Location = new System.Drawing.Point(160, 40);
			this.tbATSCPhysicalChannel.Name = "tbATSCPhysicalChannel";
			this.tbATSCPhysicalChannel.Size = new System.Drawing.Size(56, 20);
			this.tbATSCPhysicalChannel.TabIndex = 50;
			this.tbATSCPhysicalChannel.Text = "";
			// 
			// label94
			// 
			this.label94.ForeColor = System.Drawing.Color.Red;
			this.label94.Location = new System.Drawing.Point(16, 40);
			this.label94.Name = "label94";
			this.label94.Size = new System.Drawing.Size(138, 16);
			this.label94.TabIndex = 49;
			this.label94.Text = "* Physical channel number";
			// 
			// tabPage6
			// 
			this.tabPage6.Controls.Add(this.label103);
			this.tabPage6.Controls.Add(this.typeComboBox);
			this.tabPage6.Controls.Add(this.label4);
			this.tabPage6.Controls.Add(this.externalChannelTextBox);
			this.tabPage6.Controls.Add(this.label5);
			this.tabPage6.Controls.Add(this.label6);
			this.tabPage6.Controls.Add(this.inputComboBox);
			this.tabPage6.Location = new System.Drawing.Point(4, 22);
			this.tabPage6.Name = "tabPage6";
			this.tabPage6.Size = new System.Drawing.Size(480, 382);
			this.tabPage6.TabIndex = 5;
			this.tabPage6.Text = "External";
			this.tabPage6.Click += new System.EventHandler(this.tabPage6_Click);
			// 
			// label103
			// 
			this.label103.Location = new System.Drawing.Point(32, 128);
			this.label103.Name = "label103";
			this.label103.Size = new System.Drawing.Size(416, 80);
			this.label103.TabIndex = 13;
			this.label103.Text = @"If you would like mediaportal to remote control your settopbox (sattelite receiver for example) then specify that the channel is received by the external settopbox. Next specify which video input of your tvcard receives the video output of your settop box and on which channel number the settopbox receives this tvchannel. To get mediaportal to remote control the settop box you will need a USBUIRT, MCE remote blaster or other remote control device.";
			// 
			// label112
			// 
			this.label112.ForeColor = System.Drawing.Color.Red;
			this.label112.Location = new System.Drawing.Point(24, 336);
			this.label112.Name = "label112";
			this.label112.Size = new System.Drawing.Size(336, 23);
			this.label112.TabIndex = 19;
			this.label112.Text = "Fields with * and in red are required fields";
			// 
			// EditTVChannelForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(504, 462);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = new System.Drawing.Size(384, 208);
			this.Name = "EditTVChannelForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit properties of TV channel";
			this.Load += new System.EventHandler(this.EditTVChannelForm_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage4.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.tabPage5.ResumeLayout(false);
			this.tabPage7.ResumeLayout(false);
			this.tabPage6.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void channelTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
			{
				e.Handled = true;
			}
		}

		private void frequencyTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			//
			// Make sure we only type one comma or dot
			//
			if(e.KeyChar == '.' || e.KeyChar == ',')
			{
				if(frequencyTextBox.Text.IndexOfAny(new char[] {',','.'}) >= 0)
				{
					e.Handled = true;
					return;
				}
			}
			
			if(char.IsNumber(e.KeyChar) == false && (e.KeyChar != 8 && e.KeyChar != '.' && e.KeyChar != ','))
			{
				e.Handled = true;
			}		
		}

		private void okButton_Click(object sender, System.EventArgs e)
		{
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
			externalChannelTextBox.Enabled = inputComboBox.Enabled = (typeComboBox.SelectedIndex>0);
			comboBoxChannels.Enabled = frequencyTextBox.Enabled = !externalChannelTextBox.Enabled;
			
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

				channel.ID=channelId;
				channel.Name = nameTextBox.Text;
				channel.External = (typeComboBox.SelectedIndex>0);
				if (!channel.External)
				{
					if (orgChannelNumber>255)
						channel.Channel=orgChannelNumber;
					else
					{
						if (comboBoxChannels.Text.Length==0 &&comboBoxChannels.SelectedIndex<0 && orgChannelNumber==-1)
						{
							channel.Channel=TVDatabase.FindFreeTvChannelNumber(1);
						}
						else
						{
							string chanNr=(string)comboBoxChannels.SelectedItem;
							if (chanNr==null)
								chanNr=comboBoxChannels.Text.ToUpper().Trim();
							channel.Channel = -1;
							for (int i=0; i < TVChannel.SpecialChannels.Length;++i)
							{
								if (chanNr.Equals(TVChannel.SpecialChannels[i].Name))
								{
									//get free nr
									if (orgChannelNumber==-1)
									{
										channel.Channel=TVDatabase.FindFreeTvChannelNumber(TVChannel.SpecialChannels[i].Number);
									}
									else
									{
										int nr=TVDatabase.FindFreeTvChannelNumber(TVChannel.SpecialChannels[i].Number);
										if (nr==TVChannel.SpecialChannels[i].Number)
											orgChannelNumber=nr;
										channel.Channel=orgChannelNumber;
									}

									Frequency freq = new Frequency(TVChannel.SpecialChannels[i].Frequency);
									frequencyTextBox.Text=freq.ToString();

									break;
								}
							}
							if (chanNr.Equals("SVHS")) channel.Channel=(int)ExternalInputs.svhs;
							if (chanNr.Equals("CVBS#1")) channel.Channel=(int)ExternalInputs.cvbs1;
							if (chanNr.Equals("CVBS#2")) channel.Channel=(int)ExternalInputs.cvbs2;
							if (chanNr.Equals("RGB")) channel.Channel=(int)ExternalInputs.rgb;
							if (channel.Channel ==-1)
							{
								channel.Channel = Convert.ToInt32(chanNr);
							}
						}
					}
				}
				
				channel.Scrambled=checkBoxScrambled.Checked;
				try
				{

					if(frequencyTextBox.Text.IndexOfAny(new char[] { ',','.' }) >= 0)
					{
						char[] separators = new char[] {'.', ','};

						for(int index = 0; index < separators.Length; index++)
						{
							try
							{
								frequencyTextBox.Text = frequencyTextBox.Text.Replace(',', separators[index]);
								frequencyTextBox.Text = frequencyTextBox.Text.Replace('.', separators[index]);

								//
								// MegaHerz
								//
								channel.Frequency = Convert.ToDouble(frequencyTextBox.Text.Length > 0 ? frequencyTextBox.Text : "0", CultureInfo.InvariantCulture);

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
						// Herz
						//
						if(frequencyTextBox.Text.Length > 3)
						{
							channel.Frequency = Convert.ToInt32(frequencyTextBox.Text.Length > 0 ? frequencyTextBox.Text : "0");
						}
						else
						{
							channel.Frequency = Convert.ToDouble(frequencyTextBox.Text.Length > 0 ? frequencyTextBox.Text : "0", CultureInfo.InvariantCulture);
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
				channel.ExternalTunerChannel = externalChannelTextBox.Text;

				if(channel.External)
				{
					channel.Frequency = 0;
					if (inputComboBox.SelectedIndex>=0)
					{
						string externalName  = (string)inputComboBox.SelectedItem;
						if (externalName.Equals("SVHS")) channel.Channel=(int)ExternalInputs.svhs;
						if (externalName.Equals("CVBS#1")) channel.Channel=(int)ExternalInputs.cvbs1;
						if (externalName.Equals("CVBS#2")) channel.Channel=(int)ExternalInputs.cvbs2;
						if (externalName.Equals("RGB")) channel.Channel=(int)ExternalInputs.rgb;
					}
				}

				string standard=comboTvStandard.Text;
				if (standard=="Default") channel.standard = AnalogVideoStandard.None;
				if (standard=="NTSC M") channel.standard = AnalogVideoStandard.NTSC_M;
				if (standard=="NTSC M J") channel.standard = AnalogVideoStandard.NTSC_M_J;
				if (standard=="NTSC 433") channel.standard = AnalogVideoStandard.NTSC_433;
				if (standard=="PAL B") channel.standard = AnalogVideoStandard.PAL_B;
				if (standard=="PAL D") channel.standard = AnalogVideoStandard.PAL_D;
				if (standard=="PAL G") channel.standard = AnalogVideoStandard.PAL_G;
				if (standard=="PAL H") channel.standard = AnalogVideoStandard.PAL_H;
				if (standard=="PAL I") channel.standard = AnalogVideoStandard.PAL_I;
				if (standard=="PAL M") channel.standard = AnalogVideoStandard.PAL_M;
				if (standard=="PAL N") channel.standard = AnalogVideoStandard.PAL_N;
				if (standard=="PAL 60") channel.standard = AnalogVideoStandard.PAL_60;
				if (standard=="SECAM B") channel.standard = AnalogVideoStandard.SECAM_B;
				if (standard=="SECAM D") channel.standard = AnalogVideoStandard.SECAM_D;
				if (standard=="SECAM G") channel.standard = AnalogVideoStandard.SECAM_G;
				if (standard=="SECAM H") channel.standard = AnalogVideoStandard.SECAM_H;
				if (standard=="SECAM K") channel.standard = AnalogVideoStandard.SECAM_K;
				if (standard=="SECAM K1") channel.standard = AnalogVideoStandard.SECAM_K1;
				if (standard=="SECAM L") channel.standard = AnalogVideoStandard.SECAM_L;
				if (standard=="SECAM L1") channel.standard = AnalogVideoStandard.SECAM_L1;
				if (standard=="PAL N COMBO") channel.standard = AnalogVideoStandard.PAL_N_COMBO;

				TunerCountry tunerCountry = countryComboBox.SelectedItem as TunerCountry;
				if (tunerCountry!=null)
					channel.Country=tunerCountry.Id;
				else
					channel.Country=-1;
				return channel;
			}

			set
			{
				TelevisionChannel channel = value as TelevisionChannel;

				if(channel != null)
				{
					orgChannelNumber=channel.Channel;
					channelId=channel.ID;
					for (int i=0; i < countryComboBox.Items.Count;++i)
					{
						TunerCountry tunerCountry = countryComboBox.Items[i] as TunerCountry;
						if (tunerCountry.Id==channel.Country)
						{
							countryComboBox.SelectedIndex=i;
							break;
						}
					}
					checkBoxScrambled.Checked=channel.Scrambled;
					nameTextBox.Text = channel.Name;
					comboBoxChannels.SelectedItem= channel.Channel.ToString();
					comboBoxChannels.Text= channel.Channel.ToString();
					if (channel.Channel>255)
						comboBoxChannels.Enabled=false;
					labelSpecial.Text=String.Empty;
					string chanNr=(string)comboBoxChannels.SelectedItem;
					if (chanNr==null)
						chanNr=comboBoxChannels.Text.ToUpper().Trim();
					for (int i=0; i < TVChannel.SpecialChannels.Length;++i)
					{
						if (channel.Frequency.Herz==TVChannel.SpecialChannels[i].Frequency)
						{
							labelSpecial.Text=TVChannel.SpecialChannels[i].Name;
						}
					}
					frequencyTextBox.Text = channel.Frequency.ToString();

					typeComboBox.SelectedIndex = channel.External ? 1 : 0;
					externalChannelTextBox.Text = channel.ExternalTunerChannel;

					if(channel.External == true)
					{
						switch(channel.Channel)
						{
							case (int)ExternalInputs.svhs:
								inputComboBox.Text = "SVHS";
								break;

							case (int)ExternalInputs.cvbs1:
								inputComboBox.Text = "CVBS#1";
								break;

							case (int)ExternalInputs.cvbs2:
								inputComboBox.Text = "CVBS#2";
								break;
						}
					}

					//
					// Disable boxes for static channels
					//
					if(channel.Name.Equals("CVBS#1") || channel.Name.Equals("CVBS#2") || channel.Name.Equals("SVHS") || channel.Name.Equals("RGB"))
					{
						comboBoxChannels.SelectedItem=channel.Name;
						comboTvStandard.Enabled = true;
						nameTextBox.Enabled = comboBoxChannels.Enabled = frequencyTextBox.Enabled =false;
					}
					comboTvStandard.SelectedIndex=0;
					if ( channel.standard == AnalogVideoStandard.None) comboTvStandard.SelectedIndex=0;
					if ( channel.standard == AnalogVideoStandard.NTSC_M) comboTvStandard.SelectedIndex=1;
					if ( channel.standard == AnalogVideoStandard.NTSC_M_J) comboTvStandard.SelectedIndex=2;
					if ( channel.standard == AnalogVideoStandard.NTSC_433) comboTvStandard.SelectedIndex=3;
					if ( channel.standard == AnalogVideoStandard.PAL_B) comboTvStandard.SelectedIndex=4;
					if ( channel.standard == AnalogVideoStandard.PAL_D) comboTvStandard.SelectedIndex=5;
					if ( channel.standard == AnalogVideoStandard.PAL_G) comboTvStandard.SelectedIndex=6;
					if ( channel.standard == AnalogVideoStandard.PAL_H) comboTvStandard.SelectedIndex=7;
					if ( channel.standard == AnalogVideoStandard.PAL_I) comboTvStandard.SelectedIndex=8;
					if ( channel.standard == AnalogVideoStandard.PAL_M) comboTvStandard.SelectedIndex=9;
					if ( channel.standard == AnalogVideoStandard.PAL_N) comboTvStandard.SelectedIndex=10;
					if ( channel.standard == AnalogVideoStandard.PAL_60) comboTvStandard.SelectedIndex=11;
					if ( channel.standard == AnalogVideoStandard.SECAM_B) comboTvStandard.SelectedIndex=12;
					if ( channel.standard == AnalogVideoStandard.SECAM_D) comboTvStandard.SelectedIndex=13;
					if ( channel.standard == AnalogVideoStandard.SECAM_G) comboTvStandard.SelectedIndex=14;
					if ( channel.standard == AnalogVideoStandard.SECAM_H) comboTvStandard.SelectedIndex=15;
					if ( channel.standard == AnalogVideoStandard.SECAM_K) comboTvStandard.SelectedIndex=16;
					if ( channel.standard == AnalogVideoStandard.SECAM_K1) comboTvStandard.SelectedIndex=17;
					if ( channel.standard == AnalogVideoStandard.SECAM_L) comboTvStandard.SelectedIndex=18;
					if ( channel.standard == AnalogVideoStandard.SECAM_L1) comboTvStandard.SelectedIndex=19;
					if ( channel.standard == AnalogVideoStandard.PAL_N_COMBO) comboTvStandard.SelectedIndex=20;

					if (channel.Channel>=0)
					{
						int freq,ONID,TSID,SID,symbolrate,innerFec,modulation, audioPid,videoPid,teletextPid,pmtPid,bandwidth;
						string provider;
						MediaPortal.TV.Recording.DVBSections dvbSections=new MediaPortal.TV.Recording.DVBSections();
						int audio1, audio2, audio3, ac3Pid,pcrPid;
						string audioLanguage,  audioLanguage1, audioLanguage2, audioLanguage3;
						
						//DVB-T
						TVDatabase.GetDVBTTuneRequest(channelId,out provider,out freq,out ONID, out TSID,out SID, out audioPid,out videoPid,out teletextPid, out pmtPid, out bandwidth, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3, out DVBTHasEITPresentFollow, out DVBTHasEITSchedule, out pcrPid);
						label42.Text=dvbSections.GetNetworkProvider(ONID);
						tbDVBTFreq.Text=freq.ToString();;
						tbDVBTONID.Text=ONID.ToString();;
						tbDVBTTSID.Text=TSID.ToString();;
						tbDVBTSID.Text=SID.ToString();
						tbDVBTProvider.Text=provider;
						tbDVBTAudioPid.Text=audioPid.ToString();
						tbDVBTVideoPid.Text=videoPid.ToString();
						tbDVBTTeletextPid.Text=teletextPid.ToString();
						tbDVBTPmtPid.Text=pmtPid.ToString();
						tbBandWidth.Text=bandwidth.ToString();
						tbDVBTAudio1.Text=audio1.ToString();
						tbDVBTAudio2.Text=audio2.ToString();
						tbDVBTAudio3.Text=audio3.ToString();
						tbDVBTAC3.Text=ac3Pid.ToString();
						tbDVBTAudioLanguage.Text=audioLanguage;
						tbDVBTAudioLanguage1.Text=audioLanguage1;
						tbDVBTAudioLanguage2.Text=audioLanguage2;
						tbDVBTAudioLanguage3.Text=audioLanguage3;
						tbDVBTPCR.Text=pcrPid.ToString();

						//DVB-C
						TVDatabase.GetDVBCTuneRequest(channelId,out provider,out freq, out symbolrate,out innerFec,out modulation,out ONID, out TSID, out SID, out audioPid,out videoPid,out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3, out DVBCHasEITPresentFollow, out DVBCHasEITSchedule,out pcrPid);
						label41.Text=dvbSections.GetNetworkProvider(ONID);
						tbDVBCFreq.Text=freq.ToString();;
						tbDVBCONID.Text=ONID.ToString();;
						tbDVBCTSID.Text=TSID.ToString();;
						tbDVBCSID.Text=SID.ToString();
						tbDVBCSR.Text=symbolrate.ToString();
						cbDVBCInnerFeq.SelectedIndex=FecToIndex(innerFec);
						cbDVBCModulation.SelectedIndex=ModulationToIndex(modulation);
						tbDVBCProvider.Text=provider;
						tbDVBCAudioPid.Text=audioPid.ToString();
						tbDVBCVideoPid.Text=videoPid.ToString();
						tbDVBCTeletextPid.Text=teletextPid.ToString();
						tbDVBCPmtPid.Text=pmtPid.ToString();
						tbDVBCAudio1.Text=audio1.ToString();
						tbDVBCAudio2.Text=audio2.ToString();
						tbDVBCAudio3.Text=audio3.ToString();
						tbDVBCAC3.Text=ac3Pid.ToString();
						tbDVBCAudioLanguage.Text=audioLanguage;
						tbDVBCAudioLanguage1.Text=audioLanguage1;
						tbDVBCAudioLanguage2.Text=audioLanguage2;
						tbDVBCAudioLanguage3.Text=audioLanguage3;
						tbDVBCPCR.Text=pcrPid.ToString();

						//DVB-S
						DVBChannel ch = new DVBChannel();
						TVDatabase.GetSatChannel(channelId,1,ref ch);
						label40.Text=dvbSections.GetNetworkProvider(ch.NetworkID);
						tbDVBSFreq.Text=ch.Frequency.ToString();;
						tbDVBSONID.Text=ch.NetworkID.ToString();;
					    
						tbDVBSTSID.Text=ch.TransportStreamID.ToString();;
						tbDVBSSID.Text=ch.ProgramNumber.ToString();
						tbDVBSSymbolrate.Text=ch.Symbolrate.ToString();
						cbDvbSInnerFec.SelectedIndex=FecToIndex(ch.FEC);
						cbDVBSPolarisation.SelectedIndex=PolarisationToIndex(ch.Polarity);
						tbDVBSProvider.Text=ch.ServiceProvider;
						tbDVBSAudioPid.Text=ch.AudioPid.ToString();
						tbDVBSVideoPid.Text=ch.VideoPid.ToString();
						tbDVBSTeletextPid.Text=ch.TeletextPid.ToString();
						tbDVBSECMpid.Text = ch.ECMPid.ToString();
						tbDVBSPmtPid.Text=ch.PMTPid.ToString();
						tbDVBSAudio1.Text=ch.Audio1.ToString();
						tbDVBSAudio2.Text=ch.Audio2.ToString();
						tbDVBSAudio3.Text=ch.Audio3.ToString();
						tbDVBSAC3.Text=ch.AC3Pid.ToString();
						tbDVBSAudioLanguage.Text=ch.AudioLanguage;
						tbDVBSAudioLanguage1.Text=ch.AudioLanguage1;
						tbDVBSAudioLanguage2.Text=ch.AudioLanguage2;
						tbDVBSAudioLanguage3.Text=ch.AudioLanguage3;

						DVBSHasEITPresentFollow=ch.HasEITPresentFollow;
						DVBSHasEITSchedule=ch.HasEITSchedule;
						tbDVBSPCR.Text=ch.PCRPid.ToString();

						//ATSC
						int physical,minor,major;
						TVDatabase.GetATSCTuneRequest(channelId,out  physical, out provider,out freq, out symbolrate,out innerFec,out modulation,out ONID, out TSID, out SID, out audioPid,out videoPid,out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3, out minor, out major, out ATSCHasEITPresentFollow, out ATSCHasEITSchedule, out pcrPid);
						tbATSCPhysicalChannel.Text=physical.ToString();
						tbATSCMinor.Text=minor.ToString();
						tbATSCMajor.Text=major.ToString();
						tbATSCFrequency.Text=freq.ToString();;
						tbATSCTSID.Text=TSID.ToString();;
						tbATSCSymbolRate.Text=symbolrate.ToString();
						cbATSCInnerFec.SelectedIndex=FecToIndex(innerFec);
						cbATSCModulation.SelectedIndex=ModulationToIndex(modulation);
						tbATSCProvider.Text=provider;
						tbATSCAudioPid.Text=audioPid.ToString();
						tbATSCVideoPid.Text=videoPid.ToString();
						tbATSCTeletextPid.Text=teletextPid.ToString();
						tbATSCPMTPid.Text=pmtPid.ToString();
						tbATSCAudio1Pid.Text=audio1.ToString();
						tbATSCAudio2Pid.Text=audio2.ToString();
						tbATSCAudio3Pid.Text=audio3.ToString();
						tbATSCAC3Pid.Text=ac3Pid.ToString();
						tbATSCAudioLanguage.Text=audioLanguage;
						tbATSCAudioLanguage1.Text=audioLanguage1;
						tbATSCAudioLanguage2.Text=audioLanguage2;
						tbATSCAudioLanguage3.Text=audioLanguage3;
						tbATSCPCR.Text=pcrPid.ToString();

					}
				}//if(channel != null)
			}//set
		}//public TelevisionChannel Channel
		
		int FecToIndex(int fec)
		{
			switch ( (TunerLib.FECMethod)fec)
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
			switch ( index )
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
			switch ( (TunerLib.ModulationType)modulation )
			{
				case TunerLib.ModulationType.BDA_MOD_NOT_SET : return 0; 
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
			switch ( index )
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
			if (polarisation< 0 || polarisation> 1) return 0;
			return polarisation;
		}
		int IndexToPolarisation(int index)
		{
			if (index< 0 || index> 1) return 0;
			return index;
		}
		
		void SaveChannel()
		{
			TelevisionChannel chan = Channel;
			TVChannel tvchannel = new TVChannel();
			tvchannel.ID=chan.ID;
			tvchannel.Name=chan.Name;
			tvchannel.Number=chan.Channel;
			tvchannel.Country=chan.Country;
			tvchannel.External=chan.External;
			tvchannel.ExternalTunerChannel=chan.ExternalTunerChannel;
			tvchannel.TVStandard=chan.standard;
			tvchannel.VisibleInGuide=chan.VisibleInGuide;

			if (tvchannel.Number==0)
			{
				//get a unique number
				ArrayList chans=new ArrayList();
				TVDatabase.GetChannels(ref chans);
				tvchannel.Number=chans.Count;
				while (true)
				{
					bool ok=true;
					foreach (TVChannel ch in chans)
					{
						if (ch.Number==tvchannel.Number)
						{
							ok=false;
							tvchannel.Number++;
							break;
						}
					}
					if (ok) break;
				}
				comboBoxChannels.SelectedItem=tvchannel.Number.ToString();
			}

			tvchannel.Frequency=chan.Frequency.Herz;
			if(chan.Frequency.Herz < 1000)
				tvchannel.Frequency = chan.Frequency.Herz* 1000000L;

			if (tvchannel.ID<0)
			{
				tvchannel.ID=TVDatabase.AddChannel(tvchannel);
			}
			else
			{
				TVDatabase.UpdateChannel(tvchannel,SortingPlace);
			}
			orgChannelNumber=tvchannel.Number;

			int freq,ONID,TSID,SID,symbolrate,innerFec,modulation,polarisation;
			int bandWidth,pmtPid,audioPid,videoPid,teletextPid;
			string provider;
			//dvb-T
			int audio1, audio2, audio3, ac3Pid,pcrPid;
			string audioLanguage,  audioLanguage1, audioLanguage2, audioLanguage3;
						
			try
			{
				freq=ParseInt(tbDVBTFreq.Text);
				ONID=ParseInt(tbDVBTONID.Text);
				TSID=ParseInt(tbDVBTTSID.Text);
				SID=ParseInt(tbDVBTSID.Text);
				audioPid=ParseInt(tbDVBTAudioPid.Text);
				videoPid=ParseInt(tbDVBTVideoPid.Text);
				teletextPid=ParseInt(tbDVBTTeletextPid.Text);
				pmtPid=ParseInt(tbDVBTPmtPid.Text);
				provider=tbDVBTProvider.Text;
				bandWidth=ParseInt(tbBandWidth.Text);
				audio1=ParseInt(tbDVBTAudio1.Text);
				audio2=ParseInt(tbDVBTAudio2.Text);
				audio3=ParseInt(tbDVBTAudio3.Text);
				ac3Pid=ParseInt(tbDVBTAC3.Text);
				audioLanguage=tbDVBTAudioLanguage.Text;
				audioLanguage1=tbDVBTAudioLanguage1.Text;
				audioLanguage2=tbDVBTAudioLanguage2.Text;
				audioLanguage3=tbDVBTAudioLanguage3.Text;
				pcrPid=ParseInt(tbDVBTPCR.Text);
				if (ONID>0 && TSID>0 && SID > 0 && freq>0)
				{
					TVDatabase.MapDVBTChannel(tvchannel.Name,provider,tvchannel.ID,freq,ONID,TSID,SID, audioPid,videoPid,teletextPid, pmtPid,bandWidth,audio1,audio2,audio3,ac3Pid,pcrPid,audioLanguage,audioLanguage1,audioLanguage2,audioLanguage3, DVBTHasEITPresentFollow, DVBTHasEITSchedule);
				}
			}
			catch(Exception){}


			//dvb-C
			try
			{
				freq=ParseInt(tbDVBCFreq.Text);
				ONID=ParseInt(tbDVBCONID.Text);
				TSID=ParseInt(tbDVBCTSID.Text);
				SID=ParseInt(tbDVBCSID.Text);
				symbolrate=ParseInt(tbDVBCSR.Text);
				innerFec=IndexToFec(cbDVBCInnerFeq.SelectedIndex);
				modulation=IndexToModulation(cbDVBCModulation.SelectedIndex);
				provider=tbDVBCProvider.Text;
				audioPid=ParseInt(tbDVBCAudioPid.Text);
				videoPid=ParseInt(tbDVBCVideoPid.Text);
				teletextPid=ParseInt(tbDVBCTeletextPid.Text);
				pmtPid=ParseInt(tbDVBCPmtPid.Text);
				audio1=ParseInt(tbDVBCAudio1.Text);
				audio2=ParseInt(tbDVBCAudio2.Text);
				audio3=ParseInt(tbDVBCAudio3.Text);
				ac3Pid=ParseInt(tbDVBCAC3.Text);
				audioLanguage=tbDVBCAudioLanguage.Text;
				audioLanguage1=tbDVBCAudioLanguage1.Text;
				audioLanguage2=tbDVBCAudioLanguage2.Text;
				audioLanguage3=tbDVBCAudioLanguage3.Text;
				pcrPid=ParseInt(tbDVBCPCR.Text);
				if (ONID>0 && TSID>0 && SID > 0 && freq>0)
				{
					TVDatabase.MapDVBCChannel(tvchannel.Name,provider,tvchannel.ID,freq,symbolrate,innerFec,modulation,ONID,TSID,SID, audioPid,videoPid,teletextPid,pmtPid,audio1,audio2,audio3,ac3Pid,pcrPid,audioLanguage,audioLanguage1,audioLanguage2,audioLanguage3, DVBCHasEITPresentFollow, DVBCHasEITSchedule);
				}
			}
			catch(Exception){}

			//ATSC
			try
			{
				int physical,minor,major;
				physical=ParseInt(tbATSCPhysicalChannel.Text);
				minor=ParseInt(tbATSCMinor.Text);
				major=ParseInt(tbATSCMajor.Text);
				freq=ParseInt(tbATSCFrequency.Text);
				TSID=ParseInt(tbATSCTSID.Text);
				symbolrate=ParseInt(tbATSCSymbolRate.Text);
				innerFec=IndexToFec(cbATSCInnerFec.SelectedIndex);
				modulation=IndexToModulation(cbATSCModulation.SelectedIndex);
				provider=tbATSCProvider.Text;
				audioPid=ParseInt(tbATSCAudioPid.Text);
				videoPid=ParseInt(tbATSCVideoPid.Text);
				teletextPid=ParseInt(tbATSCTeletextPid.Text);
				pmtPid=ParseInt(tbATSCPMTPid.Text);
				audio1=ParseInt(tbATSCAudio1Pid.Text);
				audio2=ParseInt(tbATSCAudio2Pid.Text);
				audio3=ParseInt(tbATSCAudio3Pid.Text);
				ac3Pid=ParseInt(tbATSCAC3Pid.Text);
				audioLanguage=tbATSCAudioLanguage.Text;
				audioLanguage1=tbATSCAudioLanguage1.Text;
				audioLanguage2=tbATSCAudioLanguage2.Text;
				audioLanguage3=tbATSCAudioLanguage3.Text;
				pcrPid=ParseInt(tbATSCPCR.Text);
				if (major>0 && TSID>0 && minor>0 && physical>0 )
				{
					TVDatabase.MapATSCChannel(tvchannel.Name,physical,minor,major,provider,tvchannel.ID,freq,symbolrate,innerFec,modulation,-1,TSID,-1, audioPid,videoPid,teletextPid,pmtPid,audio1,audio2,audio3,ac3Pid,pcrPid,audioLanguage,audioLanguage1,audioLanguage2,audioLanguage3, ATSCHasEITPresentFollow, ATSCHasEITSchedule);
				}
			}
			catch(Exception){}

			//dvb-S
			try
			{
				DVBChannel ch = new DVBChannel();
				TVDatabase.GetSatChannel(tvchannel.ID,1,ref ch);

				freq=ParseInt(tbDVBSFreq.Text);
				ONID=ParseInt(tbDVBSONID.Text);
				TSID=ParseInt(tbDVBSTSID.Text);
				SID=ParseInt(tbDVBSSID.Text);
				symbolrate=ParseInt(tbDVBSSymbolrate.Text);
				innerFec=IndexToFec(cbDvbSInnerFec.SelectedIndex);
				polarisation=IndexToPolarisation(cbDVBSPolarisation.SelectedIndex);
				provider=tbDVBSProvider.Text;
				audioPid=ParseInt(tbDVBSAudioPid.Text);
				videoPid=ParseInt(tbDVBSVideoPid.Text);
				teletextPid=ParseInt(tbDVBSTeletextPid.Text);
				pmtPid=ParseInt(tbDVBSPmtPid.Text);
				audio1=ParseInt(tbDVBSAudio1.Text);
				audio2=ParseInt(tbDVBSAudio2.Text);
				audio3=ParseInt(tbDVBSAudio3.Text);
				ac3Pid=ParseInt(tbDVBSAC3.Text);
				audioLanguage=tbDVBSAudioLanguage.Text;
				audioLanguage1=tbDVBSAudioLanguage1.Text;
				audioLanguage2=tbDVBSAudioLanguage2.Text;
				audioLanguage3=tbDVBSAudioLanguage3.Text;
				pcrPid=ParseInt(tbDVBSPCR.Text);
				if (ONID>0 && TSID>0 && SID > 0 && freq>0)
				{
					ch.ServiceType=1;
					ch.Frequency=freq;
					ch.NetworkID=ONID;
					ch.TransportStreamID=TSID;
					ch.ProgramNumber=SID;
					ch.Symbolrate=symbolrate;
					ch.FEC=innerFec;
					ch.Polarity=polarisation;
					ch.ServiceProvider=provider;
					ch.ServiceName=tvchannel.Name;
					ch.ID=tvchannel.ID;
					ch.AudioPid=audioPid;
					ch.VideoPid=videoPid;
					ch.TeletextPid=teletextPid;
					ch.ECMPid = ParseInt(tbDVBSECMpid.Text);
					ch.PMTPid=pmtPid;
					ch.PCRPid=pcrPid;
					ch.Audio1=audio1;
					ch.Audio2=audio2;
					ch.Audio3=audio3;
					ch.AC3Pid=ac3Pid;
					ch.AudioLanguage=audioLanguage;
					ch.AudioLanguage1=audioLanguage1;
					ch.AudioLanguage2=audioLanguage2;
					ch.AudioLanguage3=audioLanguage3;
					ch.HasEITPresentFollow= DVBSHasEITPresentFollow;
					ch.HasEITSchedule= DVBSHasEITSchedule;
					TVDatabase.UpdateSatChannel(ch);
				}
			}
			catch(Exception){}

		}

		private void label23_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBSONID_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label22_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBSFreq_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label24_Click(object sender, System.EventArgs e)
		{
		
		}

		private void channelTextBox_TextChanged_1(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBSSID_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label26_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBSTSID_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label7_Click(object sender, System.EventArgs e)
		{
		
		}

		private void frequencyTextBox_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label1_Click(object sender, System.EventArgs e)
		{
		
		}

		private void nameTextBox_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label2_Click(object sender, System.EventArgs e)
		{
		
		}

		private void label6_Click(object sender, System.EventArgs e)
		{
		
		}

		private void label5_Click(object sender, System.EventArgs e)
		{
		
		}

		private void externalChannelTextBox_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label4_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

		private void tabPage1_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tabPage2_Click(object sender, System.EventArgs e)
		{
		
		}

		private void countryComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label8_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tabPage3_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBTFreq_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void tabPage6_Click(object sender, System.EventArgs e)
		{
		
		}

		private void label12_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBTTSID_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label25_Click(object sender, System.EventArgs e)
		{
		
		}

		private void label11_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBTSID_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBSSymbolrate_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label10_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBTONID_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label9_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tabPage4_Click(object sender, System.EventArgs e)
		{
		
		}

		private void cbDVBCModulation_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

		private void cbDVBCInnerFeq_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label19_Click(object sender, System.EventArgs e)
		{
		
		}

		private void label18_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBCSR_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label3_Click_1(object sender, System.EventArgs e)
		{
		
		}

		private void label17_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBCFreq_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label13_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBCTSID_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label14_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBCSID_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label15_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tbDVBCONID_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label16_Click(object sender, System.EventArgs e)
		{
		
		}

		private void tabPage5_Click(object sender, System.EventArgs e)
		{
		
		}

		private void cbDVBSPolarisation_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

		private void cbDvbSInnerFec_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

		private void label20_Click(object sender, System.EventArgs e)
		{
		
		}

		private void label21_Click(object sender, System.EventArgs e)
		{
		
		}

		private void comboBoxChannels_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			labelSpecial.Text=String.Empty;
			string chanNr=(string)comboBoxChannels.SelectedItem;
			if (chanNr==null)
				chanNr=comboBoxChannels.Text.ToUpper().Trim();
			for (int i=0; i < TVChannel.SpecialChannels.Length;++i)
			{
				if (chanNr.Equals(TVChannel.SpecialChannels[i].Name))
				{
					Frequency freq = new Frequency(TVChannel.SpecialChannels[i].Frequency);
					frequencyTextBox.Text=freq.ToString();
					labelSpecial.Text=TVChannel.SpecialChannels[i].Name;
					break;
				}
			}
		}
	}

	public class TelevisionChannel
	{
		public int ID;
		public string Name = String.Empty;
		public int Channel = 0;
		public Frequency Frequency = new Frequency(0);
		public bool External = false;
		public string ExternalTunerChannel = String.Empty;
		public bool VisibleInGuide = true;
		public AnalogVideoStandard standard=AnalogVideoStandard.None;
		public int Country;
		public bool Scrambled=false;
	}

	public class Frequency
	{
		public enum Format
		{
			Herz,
			MegaHerz
		}

		public Frequency(long herz)
		{
			this.herz = herz;
		}

		public Frequency(double megaherz)
		{
			this.herz = (long) (megaherz * (1000000d));
		}

		private long herz = 0;

		public long Herz
		{
			get { return herz; }
			set 
			{
				herz = value; 
				if(herz <= 1000)
					herz *= (int)1000000d;
			}
		}

		public double MegaHerz
		{
			get { return (double)herz / 1000000d; }
			set 
			{
				herz = (long) (value * 1000000d);
			}
		}

		public static implicit operator Frequency(int herz)
		{
			return new Frequency(herz);
		}

		public static implicit operator Frequency(long herz)
		{
			return new Frequency(herz);
		}

		public static implicit operator Frequency(double megaHerz)
		{
			return new Frequency((long)(megaHerz * (1000000d)));
		}

		public string ToString(Format format)
		{
			string result = String.Empty;

			try
			{
				switch(format)
				{
					case Format.Herz:
						result = String.Format("{0}", Herz);
						break;

					case Format.MegaHerz:
						result = String.Format("{0:#,###0.000}", MegaHerz);
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
			return ToString(Format.MegaHerz);
		}
	}

}
