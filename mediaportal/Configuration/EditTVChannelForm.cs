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
		private System.Windows.Forms.TextBox channelTextBox;
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
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EditTVChannelForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// Set size of window
			//
			typeComboBox.Text = "Internal";
			comboTvStandard.Text = "Default";
			TunerCountry country = new TunerCountry(-1,"Default");
			countryComboBox.Items.Add(country);
			countryComboBox.Items.AddRange(TunerCountries.Countries);
		}

		public int SortingPlace
		{
			get { return sortPlace;}
			set { sortPlace=value;}
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
			this.label3 = new System.Windows.Forms.Label();
			this.channelTextBox = new System.Windows.Forms.TextBox();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.countryComboBox = new System.Windows.Forms.ComboBox();
			this.label8 = new System.Windows.Forms.Label();
			this.tabPage3 = new System.Windows.Forms.TabPage();
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
			this.tabPage4 = new System.Windows.Forms.TabPage();
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
			this.tabPage5 = new System.Windows.Forms.TabPage();
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
			this.tabPage6 = new System.Windows.Forms.TabPage();
			this.label30 = new System.Windows.Forms.Label();
			this.label31 = new System.Windows.Forms.Label();
			this.label32 = new System.Windows.Forms.Label();
			this.tbDVBTAudioPid = new System.Windows.Forms.TextBox();
			this.tbDVBTVideoPid = new System.Windows.Forms.TextBox();
			this.tbDVBTTeletextPid = new System.Windows.Forms.TextBox();
			this.tbDVBCTeletextPid = new System.Windows.Forms.TextBox();
			this.tbDVBCVideoPid = new System.Windows.Forms.TextBox();
			this.tbDVBCAudioPid = new System.Windows.Forms.TextBox();
			this.label33 = new System.Windows.Forms.Label();
			this.label34 = new System.Windows.Forms.Label();
			this.label35 = new System.Windows.Forms.Label();
			this.tbDVBSTeletextPid = new System.Windows.Forms.TextBox();
			this.tbDVBSVideoPid = new System.Windows.Forms.TextBox();
			this.tbDVBSAudioPid = new System.Windows.Forms.TextBox();
			this.label36 = new System.Windows.Forms.Label();
			this.label37 = new System.Windows.Forms.Label();
			this.label38 = new System.Windows.Forms.Label();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.tabPage4.SuspendLayout();
			this.tabPage5.SuspendLayout();
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
			this.comboTvStandard.Location = new System.Drawing.Point(128, 40);
			this.comboTvStandard.Name = "comboTvStandard";
			this.comboTvStandard.Size = new System.Drawing.Size(224, 21);
			this.comboTvStandard.TabIndex = 3;
			this.comboTvStandard.SelectedIndexChanged += new System.EventHandler(this.comboTvStandard_SelectedIndexChanged);
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(24, 48);
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
			this.frequencyTextBox.Location = new System.Drawing.Point(128, 16);
			this.frequencyTextBox.MaxLength = 10;
			this.frequencyTextBox.Name = "frequencyTextBox";
			this.frequencyTextBox.Size = new System.Drawing.Size(80, 20);
			this.frequencyTextBox.TabIndex = 2;
			this.frequencyTextBox.Text = "0";
			this.frequencyTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frequencyTextBox_KeyPress);
			this.frequencyTextBox.TextChanged += new System.EventHandler(this.frequencyTextBox_TextChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 8);
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
			this.nameTextBox.Size = new System.Drawing.Size(176, 20);
			this.nameTextBox.TabIndex = 0;
			this.nameTextBox.Text = "";
			this.nameTextBox.TextChanged += new System.EventHandler(this.nameTextBox_TextChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 16);
			this.label2.Name = "label2";
			this.label2.TabIndex = 6;
			this.label2.Text = "Name";
			this.label2.Click += new System.EventHandler(this.label2_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = new System.Drawing.Point(333, 391);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.okButton.Location = new System.Drawing.Point(253, 391);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 2;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// inputComboBox
			// 
			this.inputComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.inputComboBox.Enabled = false;
			this.inputComboBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.inputComboBox.Items.AddRange(new object[] {
																											 "Composite #1",
																											 "Composite #2",
																											 "SVHS"});
			this.inputComboBox.Location = new System.Drawing.Point(128, 56);
			this.inputComboBox.Name = "inputComboBox";
			this.inputComboBox.Size = new System.Drawing.Size(224, 21);
			this.inputComboBox.TabIndex = 1;
			this.inputComboBox.SelectedIndexChanged += new System.EventHandler(this.inputComboBox_SelectedIndexChanged);
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(24, 56);
			this.label6.Name = "label6";
			this.label6.TabIndex = 12;
			this.label6.Text = "Input";
			this.label6.Click += new System.EventHandler(this.label6_Click);
			// 
			// typeComboBox
			// 
			this.typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.typeComboBox.Items.AddRange(new object[] {
																											"Internal",
																											"External"});
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
			this.externalChannelTextBox.Size = new System.Drawing.Size(184, 20);
			this.externalChannelTextBox.TabIndex = 2;
			this.externalChannelTextBox.Text = "";
			this.externalChannelTextBox.TextChanged += new System.EventHandler(this.externalChannelTextBox_TextChanged);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(24, 88);
			this.label4.Name = "label4";
			this.label4.TabIndex = 8;
			this.label4.Text = "External channel";
			this.label4.Click += new System.EventHandler(this.label4_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Controls.Add(this.tabPage5);
			this.tabControl1.Controls.Add(this.tabPage6);
			this.tabControl1.Location = new System.Drawing.Point(8, 8);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(400, 368);
			this.tabControl1.TabIndex = 4;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.label3);
			this.tabPage1.Controls.Add(this.channelTextBox);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.nameTextBox);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(392, 286);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "General";
			this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 48);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(88, 16);
			this.label3.TabIndex = 10;
			this.label3.Text = "Channel";
			this.label3.Click += new System.EventHandler(this.label3_Click_1);
			// 
			// channelTextBox
			// 
			this.channelTextBox.Location = new System.Drawing.Point(128, 48);
			this.channelTextBox.MaxLength = 4;
			this.channelTextBox.Name = "channelTextBox";
			this.channelTextBox.Size = new System.Drawing.Size(40, 20);
			this.channelTextBox.TabIndex = 9;
			this.channelTextBox.Text = "0";
			this.channelTextBox.TextChanged += new System.EventHandler(this.channelTextBox_TextChanged_1);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.countryComboBox);
			this.tabPage2.Controls.Add(this.label8);
			this.tabPage2.Controls.Add(this.label1);
			this.tabPage2.Controls.Add(this.frequencyTextBox);
			this.tabPage2.Controls.Add(this.label7);
			this.tabPage2.Controls.Add(this.comboTvStandard);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(392, 286);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Analog";
			this.tabPage2.Click += new System.EventHandler(this.tabPage2_Click);
			// 
			// countryComboBox
			// 
			this.countryComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.countryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.countryComboBox.Location = new System.Drawing.Point(128, 72);
			this.countryComboBox.MaxDropDownItems = 16;
			this.countryComboBox.Name = "countryComboBox";
			this.countryComboBox.Size = new System.Drawing.Size(224, 20);
			this.countryComboBox.Sorted = true;
			this.countryComboBox.TabIndex = 15;
			this.countryComboBox.SelectedIndexChanged += new System.EventHandler(this.countryComboBox_SelectedIndexChanged);
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(24, 72);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(56, 16);
			this.label8.TabIndex = 16;
			this.label8.Text = "Country";
			this.label8.Click += new System.EventHandler(this.label8_Click);
			// 
			// tabPage3
			// 
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
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(392, 286);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "DVB-T";
			this.tabPage3.Click += new System.EventHandler(this.tabPage3_Click);
			// 
			// tbDVBTProvider
			// 
			this.tbDVBTProvider.Location = new System.Drawing.Point(152, 152);
			this.tbDVBTProvider.Name = "tbDVBTProvider";
			this.tbDVBTProvider.TabIndex = 9;
			this.tbDVBTProvider.Text = "";
			// 
			// label27
			// 
			this.label27.Location = new System.Drawing.Point(16, 152);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(100, 16);
			this.label27.TabIndex = 8;
			this.label27.Text = "Provider:";
			// 
			// tbDVBTFreq
			// 
			this.tbDVBTFreq.Location = new System.Drawing.Point(152, 120);
			this.tbDVBTFreq.Name = "tbDVBTFreq";
			this.tbDVBTFreq.TabIndex = 7;
			this.tbDVBTFreq.Text = "";
			this.tbDVBTFreq.TextChanged += new System.EventHandler(this.tbDVBTFreq_TextChanged);
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(16, 120);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(128, 16);
			this.label12.TabIndex = 6;
			this.label12.Text = "Carrier Frequency (KHz)";
			this.label12.Click += new System.EventHandler(this.label12_Click);
			// 
			// tbDVBTTSID
			// 
			this.tbDVBTTSID.Location = new System.Drawing.Point(152, 88);
			this.tbDVBTTSID.Name = "tbDVBTTSID";
			this.tbDVBTTSID.TabIndex = 5;
			this.tbDVBTTSID.Text = "";
			this.tbDVBTTSID.TextChanged += new System.EventHandler(this.tbDVBTTSID_TextChanged);
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(16, 88);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(72, 16);
			this.label11.TabIndex = 4;
			this.label11.Text = "Transport ID:";
			this.label11.Click += new System.EventHandler(this.label11_Click);
			// 
			// tbDVBTSID
			// 
			this.tbDVBTSID.Location = new System.Drawing.Point(152, 56);
			this.tbDVBTSID.Name = "tbDVBTSID";
			this.tbDVBTSID.TabIndex = 3;
			this.tbDVBTSID.Text = "";
			this.tbDVBTSID.TextChanged += new System.EventHandler(this.tbDVBTSID_TextChanged);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(16, 56);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(64, 16);
			this.label10.TabIndex = 2;
			this.label10.Text = "Service ID:";
			this.label10.Click += new System.EventHandler(this.label10_Click);
			// 
			// tbDVBTONID
			// 
			this.tbDVBTONID.Location = new System.Drawing.Point(152, 24);
			this.tbDVBTONID.Name = "tbDVBTONID";
			this.tbDVBTONID.TabIndex = 1;
			this.tbDVBTONID.Text = "";
			this.tbDVBTONID.TextChanged += new System.EventHandler(this.tbDVBTONID_TextChanged);
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(16, 24);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(64, 16);
			this.label9.TabIndex = 0;
			this.label9.Text = "Network ID:";
			this.label9.Click += new System.EventHandler(this.label9_Click);
			// 
			// tabPage4
			// 
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
			this.tabPage4.Size = new System.Drawing.Size(392, 342);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "DVB-C";
			this.tabPage4.Click += new System.EventHandler(this.tabPage4_Click);
			// 
			// tbDVBCProvider
			// 
			this.tbDVBCProvider.Location = new System.Drawing.Point(160, 200);
			this.tbDVBCProvider.Name = "tbDVBCProvider";
			this.tbDVBCProvider.TabIndex = 23;
			this.tbDVBCProvider.Text = "";
			// 
			// label28
			// 
			this.label28.Location = new System.Drawing.Point(24, 200);
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
			this.cbDVBCModulation.Location = new System.Drawing.Point(160, 168);
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
			this.cbDVBCInnerFeq.Location = new System.Drawing.Point(160, 144);
			this.cbDVBCInnerFeq.Name = "cbDVBCInnerFeq";
			this.cbDVBCInnerFeq.Size = new System.Drawing.Size(121, 21);
			this.cbDVBCInnerFeq.TabIndex = 20;
			this.cbDVBCInnerFeq.SelectedIndexChanged += new System.EventHandler(this.cbDVBCInnerFeq_SelectedIndexChanged);
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(24, 168);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(100, 16);
			this.label19.TabIndex = 19;
			this.label19.Text = "Modulation";
			this.label19.Click += new System.EventHandler(this.label19_Click);
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(24, 144);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(100, 16);
			this.label18.TabIndex = 18;
			this.label18.Text = "InnerFEC";
			this.label18.Click += new System.EventHandler(this.label18_Click);
			// 
			// tbDVBCSR
			// 
			this.tbDVBCSR.Location = new System.Drawing.Point(160, 120);
			this.tbDVBCSR.Name = "tbDVBCSR";
			this.tbDVBCSR.TabIndex = 17;
			this.tbDVBCSR.Text = "";
			this.tbDVBCSR.TextChanged += new System.EventHandler(this.tbDVBCSR_TextChanged);
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(24, 120);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(100, 16);
			this.label17.TabIndex = 16;
			this.label17.Text = "Symbolrate";
			this.label17.Click += new System.EventHandler(this.label17_Click);
			// 
			// tbDVBCFreq
			// 
			this.tbDVBCFreq.Location = new System.Drawing.Point(160, 96);
			this.tbDVBCFreq.Name = "tbDVBCFreq";
			this.tbDVBCFreq.TabIndex = 15;
			this.tbDVBCFreq.Text = "";
			this.tbDVBCFreq.TextChanged += new System.EventHandler(this.tbDVBCFreq_TextChanged);
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(24, 96);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(128, 16);
			this.label13.TabIndex = 14;
			this.label13.Text = "Carrier Frequency (KHz)";
			this.label13.Click += new System.EventHandler(this.label13_Click);
			// 
			// tbDVBCTSID
			// 
			this.tbDVBCTSID.Location = new System.Drawing.Point(160, 72);
			this.tbDVBCTSID.Name = "tbDVBCTSID";
			this.tbDVBCTSID.TabIndex = 13;
			this.tbDVBCTSID.Text = "";
			this.tbDVBCTSID.TextChanged += new System.EventHandler(this.tbDVBCTSID_TextChanged);
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(24, 72);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(72, 16);
			this.label14.TabIndex = 12;
			this.label14.Text = "Transport ID:";
			this.label14.Click += new System.EventHandler(this.label14_Click);
			// 
			// tbDVBCSID
			// 
			this.tbDVBCSID.Location = new System.Drawing.Point(160, 48);
			this.tbDVBCSID.Name = "tbDVBCSID";
			this.tbDVBCSID.TabIndex = 11;
			this.tbDVBCSID.Text = "";
			this.tbDVBCSID.TextChanged += new System.EventHandler(this.tbDVBCSID_TextChanged);
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(24, 48);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(64, 16);
			this.label15.TabIndex = 10;
			this.label15.Text = "Service ID:";
			this.label15.Click += new System.EventHandler(this.label15_Click);
			// 
			// tbDVBCONID
			// 
			this.tbDVBCONID.Location = new System.Drawing.Point(160, 24);
			this.tbDVBCONID.Name = "tbDVBCONID";
			this.tbDVBCONID.TabIndex = 9;
			this.tbDVBCONID.Text = "";
			this.tbDVBCONID.TextChanged += new System.EventHandler(this.tbDVBCONID_TextChanged);
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(24, 24);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(64, 16);
			this.label16.TabIndex = 8;
			this.label16.Text = "Network ID:";
			this.label16.Click += new System.EventHandler(this.label16_Click);
			// 
			// tabPage5
			// 
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
			this.tabPage5.Size = new System.Drawing.Size(392, 342);
			this.tabPage5.TabIndex = 4;
			this.tabPage5.Text = "DVB-S";
			this.tabPage5.Click += new System.EventHandler(this.tabPage5_Click);
			// 
			// tbDVBSProvider
			// 
			this.tbDVBSProvider.Location = new System.Drawing.Point(160, 192);
			this.tbDVBSProvider.Name = "tbDVBSProvider";
			this.tbDVBSProvider.TabIndex = 37;
			this.tbDVBSProvider.Text = "";
			// 
			// label29
			// 
			this.label29.Location = new System.Drawing.Point(24, 192);
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
			this.cbDVBSPolarisation.Location = new System.Drawing.Point(160, 160);
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
			this.cbDvbSInnerFec.Location = new System.Drawing.Point(160, 136);
			this.cbDvbSInnerFec.Name = "cbDvbSInnerFec";
			this.cbDvbSInnerFec.Size = new System.Drawing.Size(121, 21);
			this.cbDvbSInnerFec.TabIndex = 34;
			this.cbDvbSInnerFec.SelectedIndexChanged += new System.EventHandler(this.cbDvbSInnerFec_SelectedIndexChanged);
			// 
			// label20
			// 
			this.label20.Location = new System.Drawing.Point(24, 160);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(100, 16);
			this.label20.TabIndex = 33;
			this.label20.Text = "Polarisation";
			this.label20.Click += new System.EventHandler(this.label20_Click);
			// 
			// label21
			// 
			this.label21.Location = new System.Drawing.Point(24, 136);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(100, 16);
			this.label21.TabIndex = 32;
			this.label21.Text = "InnerFEC";
			this.label21.Click += new System.EventHandler(this.label21_Click);
			// 
			// tbDVBSSymbolrate
			// 
			this.tbDVBSSymbolrate.Location = new System.Drawing.Point(160, 112);
			this.tbDVBSSymbolrate.Name = "tbDVBSSymbolrate";
			this.tbDVBSSymbolrate.TabIndex = 31;
			this.tbDVBSSymbolrate.Text = "";
			this.tbDVBSSymbolrate.TextChanged += new System.EventHandler(this.tbDVBSSymbolrate_TextChanged);
			// 
			// label22
			// 
			this.label22.Location = new System.Drawing.Point(24, 112);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(100, 16);
			this.label22.TabIndex = 30;
			this.label22.Text = "Symbolrate";
			this.label22.Click += new System.EventHandler(this.label22_Click);
			// 
			// tbDVBSFreq
			// 
			this.tbDVBSFreq.Location = new System.Drawing.Point(160, 88);
			this.tbDVBSFreq.Name = "tbDVBSFreq";
			this.tbDVBSFreq.TabIndex = 29;
			this.tbDVBSFreq.Text = "";
			this.tbDVBSFreq.TextChanged += new System.EventHandler(this.tbDVBSFreq_TextChanged);
			// 
			// label23
			// 
			this.label23.Location = new System.Drawing.Point(24, 88);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(128, 16);
			this.label23.TabIndex = 28;
			this.label23.Text = "Carrier Frequency (KHz)";
			this.label23.Click += new System.EventHandler(this.label23_Click);
			// 
			// tbDVBSTSID
			// 
			this.tbDVBSTSID.Location = new System.Drawing.Point(160, 64);
			this.tbDVBSTSID.Name = "tbDVBSTSID";
			this.tbDVBSTSID.TabIndex = 27;
			this.tbDVBSTSID.Text = "";
			this.tbDVBSTSID.TextChanged += new System.EventHandler(this.tbDVBSTSID_TextChanged);
			// 
			// label24
			// 
			this.label24.Location = new System.Drawing.Point(24, 64);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(72, 16);
			this.label24.TabIndex = 26;
			this.label24.Text = "Transport ID:";
			this.label24.Click += new System.EventHandler(this.label24_Click);
			// 
			// tbDVBSSID
			// 
			this.tbDVBSSID.Location = new System.Drawing.Point(160, 40);
			this.tbDVBSSID.Name = "tbDVBSSID";
			this.tbDVBSSID.TabIndex = 25;
			this.tbDVBSSID.Text = "";
			this.tbDVBSSID.TextChanged += new System.EventHandler(this.tbDVBSSID_TextChanged);
			// 
			// label25
			// 
			this.label25.Location = new System.Drawing.Point(24, 40);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(64, 16);
			this.label25.TabIndex = 24;
			this.label25.Text = "Service ID:";
			this.label25.Click += new System.EventHandler(this.label25_Click);
			// 
			// tbDVBSONID
			// 
			this.tbDVBSONID.Location = new System.Drawing.Point(160, 16);
			this.tbDVBSONID.Name = "tbDVBSONID";
			this.tbDVBSONID.TabIndex = 23;
			this.tbDVBSONID.Text = "";
			this.tbDVBSONID.TextChanged += new System.EventHandler(this.tbDVBSONID_TextChanged);
			// 
			// label26
			// 
			this.label26.Location = new System.Drawing.Point(24, 16);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(64, 16);
			this.label26.TabIndex = 22;
			this.label26.Text = "Network ID:";
			this.label26.Click += new System.EventHandler(this.label26_Click);
			// 
			// tabPage6
			// 
			this.tabPage6.Controls.Add(this.typeComboBox);
			this.tabPage6.Controls.Add(this.label4);
			this.tabPage6.Controls.Add(this.externalChannelTextBox);
			this.tabPage6.Controls.Add(this.label5);
			this.tabPage6.Controls.Add(this.label6);
			this.tabPage6.Controls.Add(this.inputComboBox);
			this.tabPage6.Location = new System.Drawing.Point(4, 22);
			this.tabPage6.Name = "tabPage6";
			this.tabPage6.Size = new System.Drawing.Size(392, 286);
			this.tabPage6.TabIndex = 5;
			this.tabPage6.Text = "External";
			this.tabPage6.Click += new System.EventHandler(this.tabPage6_Click);
			// 
			// label30
			// 
			this.label30.Location = new System.Drawing.Point(16, 184);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(72, 16);
			this.label30.TabIndex = 10;
			this.label30.Text = "Audio pid:";
			// 
			// label31
			// 
			this.label31.Location = new System.Drawing.Point(16, 208);
			this.label31.Name = "label31";
			this.label31.Size = new System.Drawing.Size(72, 16);
			this.label31.TabIndex = 11;
			this.label31.Text = "Video pid:";
			// 
			// label32
			// 
			this.label32.Location = new System.Drawing.Point(16, 232);
			this.label32.Name = "label32";
			this.label32.Size = new System.Drawing.Size(72, 16);
			this.label32.TabIndex = 12;
			this.label32.Text = "Teletext pid:";
			// 
			// tbDVBTAudioPid
			// 
			this.tbDVBTAudioPid.Location = new System.Drawing.Point(152, 184);
			this.tbDVBTAudioPid.Name = "tbDVBTAudioPid";
			this.tbDVBTAudioPid.TabIndex = 13;
			this.tbDVBTAudioPid.Text = "";
			// 
			// tbDVBTVideoPid
			// 
			this.tbDVBTVideoPid.Location = new System.Drawing.Point(152, 208);
			this.tbDVBTVideoPid.Name = "tbDVBTVideoPid";
			this.tbDVBTVideoPid.TabIndex = 14;
			this.tbDVBTVideoPid.Text = "";
			// 
			// tbDVBTTeletextPid
			// 
			this.tbDVBTTeletextPid.Location = new System.Drawing.Point(152, 232);
			this.tbDVBTTeletextPid.Name = "tbDVBTTeletextPid";
			this.tbDVBTTeletextPid.TabIndex = 15;
			this.tbDVBTTeletextPid.Text = "";
			// 
			// tbDVBCTeletextPid
			// 
			this.tbDVBCTeletextPid.Location = new System.Drawing.Point(160, 280);
			this.tbDVBCTeletextPid.Name = "tbDVBCTeletextPid";
			this.tbDVBCTeletextPid.TabIndex = 29;
			this.tbDVBCTeletextPid.Text = "";
			// 
			// tbDVBCVideoPid
			// 
			this.tbDVBCVideoPid.Location = new System.Drawing.Point(160, 256);
			this.tbDVBCVideoPid.Name = "tbDVBCVideoPid";
			this.tbDVBCVideoPid.TabIndex = 28;
			this.tbDVBCVideoPid.Text = "";
			// 
			// tbDVBCAudioPid
			// 
			this.tbDVBCAudioPid.Location = new System.Drawing.Point(160, 232);
			this.tbDVBCAudioPid.Name = "tbDVBCAudioPid";
			this.tbDVBCAudioPid.TabIndex = 27;
			this.tbDVBCAudioPid.Text = "";
			// 
			// label33
			// 
			this.label33.Location = new System.Drawing.Point(24, 280);
			this.label33.Name = "label33";
			this.label33.Size = new System.Drawing.Size(72, 16);
			this.label33.TabIndex = 26;
			this.label33.Text = "Teletext pid:";
			// 
			// label34
			// 
			this.label34.Location = new System.Drawing.Point(24, 256);
			this.label34.Name = "label34";
			this.label34.Size = new System.Drawing.Size(72, 16);
			this.label34.TabIndex = 25;
			this.label34.Text = "Video pid:";
			// 
			// label35
			// 
			this.label35.Location = new System.Drawing.Point(24, 232);
			this.label35.Name = "label35";
			this.label35.Size = new System.Drawing.Size(72, 16);
			this.label35.TabIndex = 24;
			this.label35.Text = "Audio pid:";
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
			this.label37.Location = new System.Drawing.Point(24, 248);
			this.label37.Name = "label37";
			this.label37.Size = new System.Drawing.Size(72, 16);
			this.label37.TabIndex = 39;
			this.label37.Text = "Video pid:";
			// 
			// label38
			// 
			this.label38.Location = new System.Drawing.Point(24, 224);
			this.label38.Name = "label38";
			this.label38.Size = new System.Drawing.Size(72, 16);
			this.label38.TabIndex = 38;
			this.label38.Text = "Audio pid:";
			// 
			// EditTVChannelForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(416, 422);
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
			this.tabPage3.ResumeLayout(false);
			this.tabPage4.ResumeLayout(false);
			this.tabPage5.ResumeLayout(false);
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
			SaveChannel();
			this.DialogResult = DialogResult.OK;
			this.Hide();
		}

		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Hide();
		}

		private void channelTextBox_TextChanged(object sender, System.EventArgs e)
		{
			if(channelTextBox.Text.Length > 0)
			{
				int channel = Int32.Parse(channelTextBox.Text);
				frequencyTextBox.Enabled = (channel > 0 && channel < (int)ExternalInputs.svhs);
			}
		}    

		private void typeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			externalChannelTextBox.Enabled = inputComboBox.Enabled = typeComboBox.Text.Equals("External");
			channelTextBox.Enabled = frequencyTextBox.Enabled = !externalChannelTextBox.Enabled;
		}

		private void inputComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			switch(inputComboBox.Text)
			{
				case "SVHS":
					channelTextBox.Text = ((int)ExternalInputs.svhs).ToString();
					break;

				case "Composite #1":
					channelTextBox.Text = ((int)ExternalInputs.cvbs1).ToString();
					break;

				case "Composite #2":
					channelTextBox.Text = ((int)ExternalInputs.cvbs2).ToString();
					break;
			}
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
				channel.Channel = Convert.ToInt32(channelTextBox.Text.Length > 0 ? channelTextBox.Text : "0");

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
				channel.External = typeComboBox.Text.Equals("External");
				channel.ExternalTunerChannel = externalChannelTextBox.Text;

				if(channel.External)
				{
					channel.Frequency = 0;
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
					nameTextBox.Text = channel.Name;
					channelTextBox.Text = channel.Channel.ToString();
					frequencyTextBox.Text = channel.Frequency.ToString();

					typeComboBox.Text = channel.External ? "External" : "Internal";
					externalChannelTextBox.Text = channel.ExternalTunerChannel;

					if(channel.External == true)
					{
						switch(channel.Channel)
						{
							case (int)ExternalInputs.svhs:
								inputComboBox.Text = "SVHS";
								break;

							case (int)ExternalInputs.cvbs1:
								inputComboBox.Text = "Composite #1";
								break;

							case (int)ExternalInputs.cvbs2:
								inputComboBox.Text = "Composite #2";
								break;
						}
					}

					//
					// Disable boxes for static channels
					//
					if(channel.Name.Equals("Composite #1") || channel.Name.Equals("Composite #2") || channel.Name.Equals("SVHS"))
					{
						comboTvStandard.Enabled = true;
						nameTextBox.Enabled = channelTextBox.Enabled = frequencyTextBox.Enabled =false;
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
						int freq,ONID,TSID,SID,symbolrate,innerFec,modulation, audioPid,videoPid,teletextPid;
						string provider;
						//DVB-T
						TVDatabase.GetDVBTTuneRequest(channelId,out provider,out freq,out ONID, out TSID,out SID, out audioPid,out videoPid,out teletextPid);
						tbDVBTFreq.Text=freq.ToString();;
						tbDVBTONID.Text=ONID.ToString();;
						tbDVBTTSID.Text=TSID.ToString();;
						tbDVBTSID.Text=SID.ToString();
						tbDVBTProvider.Text=provider;
						tbDVBTAudioPid.Text=audioPid.ToString();
						tbDVBTVideoPid.Text=videoPid.ToString();
						tbDVBTTeletextPid.Text=teletextPid.ToString();

						//DVB-C
						TVDatabase.GetDVBCTuneRequest(channelId,out provider,out freq, out symbolrate,out innerFec,out modulation,out ONID, out TSID, out SID, out audioPid,out videoPid,out teletextPid);
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


						//DVB-S
						DVBChannel ch = new DVBChannel();
						TVDatabase.GetSatChannel(channelId,1,ref ch);
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
			return 0;
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
				channelTextBox.Text=tvchannel.Number.ToString();
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

			int freq,ONID,TSID,SID,symbolrate,innerFec,modulation,polarisation;
			int audioPid,videoPid,teletextPid;
			string provider;
			//dvb-T
			try
			{
				freq=Int32.Parse(tbDVBTFreq.Text);
				ONID=Int32.Parse(tbDVBTONID.Text);
				TSID=Int32.Parse(tbDVBTTSID.Text);
				SID=Int32.Parse(tbDVBTSID.Text);
				audioPid=Int32.Parse(tbDVBTAudioPid.Text);
				videoPid=Int32.Parse(tbDVBTVideoPid.Text);
				teletextPid=Int32.Parse(tbDVBTTeletextPid.Text);
				provider=tbDVBTProvider.Text;
				if (ONID>0 && TSID>0 && SID > 0 && freq>0)
				{
					TVDatabase.MapDVBTChannel(tvchannel.Name,provider,tvchannel.ID,freq,ONID,TSID,SID, audioPid,videoPid,teletextPid);
				}
			}
			catch(Exception){}


			//dvb-C
			try
			{
				freq=Int32.Parse(tbDVBCFreq.Text);
				ONID=Int32.Parse(tbDVBCONID.Text);
				TSID=Int32.Parse(tbDVBCTSID.Text);
				SID=Int32.Parse(tbDVBCSID.Text);
				symbolrate=Int32.Parse(tbDVBCSR.Text);
				innerFec=IndexToFec(cbDVBCInnerFeq.SelectedIndex);
				modulation=IndexToModulation(cbDVBCModulation.SelectedIndex);
				provider=tbDVBCProvider.Text;
				audioPid=Int32.Parse(tbDVBCAudioPid.Text);
				videoPid=Int32.Parse(tbDVBCVideoPid.Text);
				teletextPid=Int32.Parse(tbDVBCTeletextPid.Text);
				if (ONID>0 && TSID>0 && SID > 0 && freq>0)
				{
					TVDatabase.MapDVBCChannel(tvchannel.Name,provider,tvchannel.ID,freq,symbolrate,innerFec,modulation,ONID,TSID,SID, audioPid,videoPid,teletextPid);
				}
			}
			catch(Exception){}

			//dvb-S
			try
			{
				DVBChannel ch = new DVBChannel();
				TVDatabase.GetSatChannel(tvchannel.ID,1,ref ch);

				freq=Int32.Parse(tbDVBSFreq.Text);
				ONID=Int32.Parse(tbDVBSONID.Text);
				TSID=Int32.Parse(tbDVBSTSID.Text);
				SID=Int32.Parse(tbDVBSSID.Text);
				symbolrate=Int32.Parse(tbDVBSSymbolrate.Text);
				innerFec=IndexToFec(cbDvbSInnerFec.SelectedIndex);
				polarisation=IndexToPolarisation(cbDVBSPolarisation.SelectedIndex);
				provider=tbDVBSProvider.Text;
				audioPid=Int32.Parse(tbDVBSAudioPid.Text);
				videoPid=Int32.Parse(tbDVBSVideoPid.Text);
				teletextPid=Int32.Parse(tbDVBSTeletextPid.Text);
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
