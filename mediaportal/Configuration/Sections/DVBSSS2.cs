using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SQLite.NET;
using MediaPortal.TV.Database;
using MediaPortal.Radio.Database;
using System.Xml;


namespace MediaPortal.Configuration.Sections
{
	/// <summary>
	/// Zusammenfassung für DVBSSS2.
	/// </summary>
	public class DVBSSS2  : MediaPortal.Configuration.SectionSettings
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.ComboBox comboBox3;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.TextBox sat4;
		private System.Windows.Forms.TextBox sat3;
		private System.Windows.Forms.TextBox sat2;
		private System.Windows.Forms.TextBox sat1;
		private System.Windows.Forms.OpenFileDialog ofd;
		private System.Windows.Forms.ComboBox diseqca;
		private System.Windows.Forms.ComboBox diseqcb;
		private System.Windows.Forms.ComboBox diseqcc;
		private System.Windows.Forms.ComboBox diseqcd;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox feedback;
		// globals
		private DVBSections m_dvbSec=null;
		private DVBSections.Transponder[] transpList=null;
		private System.Windows.Forms.ComboBox lnbconfig1;
		private System.Windows.Forms.ComboBox lnbconfig2;
		private System.Windows.Forms.ComboBox lnbconfig3;
		private System.Windows.Forms.ComboBox lnbconfig4;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.PropertyGrid propertyGrid1;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.TreeView treeView2;
		private System.Windows.Forms.CheckBox checkBox2;
		/// <summary> 
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		
		public DVBSSS2(): this("SkyStar2 Settings")
		{
			LoadConfig();
			TVChannel	tv=new TVChannel();
			
		}
		enum KuBand
		{
			Lof1=9750,
			Switch = 11700,
			Lof2 = 10600
		}
		enum Circular
		{
			Lof =10750
		}
		enum CBand
		{
			Lof=5150
		}
		public DVBSSS2(string name): base(name)
		{
			// Dieser Aufruf ist für den Windows Form-Designer erforderlich.
			InitializeComponent();
			LoadConfig();
			m_dvbSec=new DVBSections();

			// TODO: Initialisierungen nach dem Aufruf von InitializeComponent hinzufügen

		}

		/// <summary> 
		/// Die verwendeten Ressourcen bereinigen.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
					m_dvbSec.CleanUp();
				}
			}
			base.Dispose( disposing );
		}

		#region Vom Komponenten-Designer generierter Code
		/// <summary> 
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.lnbconfig4 = new System.Windows.Forms.ComboBox();
			this.lnbconfig3 = new System.Windows.Forms.ComboBox();
			this.lnbconfig2 = new System.Windows.Forms.ComboBox();
			this.lnbconfig1 = new System.Windows.Forms.ComboBox();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.diseqcd = new System.Windows.Forms.ComboBox();
			this.diseqcc = new System.Windows.Forms.ComboBox();
			this.diseqcb = new System.Windows.Forms.ComboBox();
			this.sat4 = new System.Windows.Forms.TextBox();
			this.sat3 = new System.Windows.Forms.TextBox();
			this.sat2 = new System.Windows.Forms.TextBox();
			this.button4 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.sat1 = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.diseqca = new System.Windows.Forms.ComboBox();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.label6 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.comboBox3 = new System.Windows.Forms.ComboBox();
			this.button5 = new System.Windows.Forms.Button();
			this.label9 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this.feedback = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.treeView2 = new System.Windows.Forms.TreeView();
			this.button6 = new System.Windows.Forms.Button();
			this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
			this.label2 = new System.Windows.Forms.Label();
			this.ofd = new System.Windows.Forms.OpenFileDialog();
			this.groupBox2.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 200);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(424, 184);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "PlugIns:";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.lnbconfig4);
			this.groupBox2.Controls.Add(this.lnbconfig3);
			this.groupBox2.Controls.Add(this.lnbconfig2);
			this.groupBox2.Controls.Add(this.lnbconfig1);
			this.groupBox2.Controls.Add(this.checkBox1);
			this.groupBox2.Controls.Add(this.diseqcd);
			this.groupBox2.Controls.Add(this.diseqcc);
			this.groupBox2.Controls.Add(this.diseqcb);
			this.groupBox2.Controls.Add(this.sat4);
			this.groupBox2.Controls.Add(this.sat3);
			this.groupBox2.Controls.Add(this.sat2);
			this.groupBox2.Controls.Add(this.button4);
			this.groupBox2.Controls.Add(this.button3);
			this.groupBox2.Controls.Add(this.button2);
			this.groupBox2.Controls.Add(this.sat1);
			this.groupBox2.Controls.Add(this.button1);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.diseqca);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(8, 8);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(424, 176);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "DiSeqC / Satellites ";
			// 
			// lnbconfig4
			// 
			this.lnbconfig4.Items.AddRange(new object[] {
															"0 KHz",
															"22 KHz",
															"33 Khz",
															"44 KHz"});
			this.lnbconfig4.Location = new System.Drawing.Point(336, 136);
			this.lnbconfig4.Name = "lnbconfig4";
			this.lnbconfig4.Size = new System.Drawing.Size(80, 21);
			this.lnbconfig4.TabIndex = 19;
			// 
			// lnbconfig3
			// 
			this.lnbconfig3.Items.AddRange(new object[] {
															"0 KHz",
															"22 KHz",
															"33 Khz",
															"44 KHz"});
			this.lnbconfig3.Location = new System.Drawing.Point(336, 112);
			this.lnbconfig3.Name = "lnbconfig3";
			this.lnbconfig3.Size = new System.Drawing.Size(80, 21);
			this.lnbconfig3.TabIndex = 18;
			this.lnbconfig3.SelectedIndexChanged += new System.EventHandler(this.comboBox4_SelectedIndexChanged);
			// 
			// lnbconfig2
			// 
			this.lnbconfig2.Items.AddRange(new object[] {
															"0 KHz",
															"22 KHz",
															"33 Khz",
															"44 KHz"});
			this.lnbconfig2.Location = new System.Drawing.Point(336, 88);
			this.lnbconfig2.Name = "lnbconfig2";
			this.lnbconfig2.Size = new System.Drawing.Size(80, 21);
			this.lnbconfig2.TabIndex = 17;
			// 
			// lnbconfig1
			// 
			this.lnbconfig1.Items.AddRange(new object[] {
															"0 KHz",
															"22 KHz",
															"33 Khz",
															"44 KHz"});
			this.lnbconfig1.Location = new System.Drawing.Point(336, 64);
			this.lnbconfig1.Name = "lnbconfig1";
			this.lnbconfig1.Size = new System.Drawing.Size(80, 21);
			this.lnbconfig1.TabIndex = 16;
			// 
			// checkBox1
			// 
			this.checkBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBox1.Location = new System.Drawing.Point(16, 24);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(128, 16);
			this.checkBox1.TabIndex = 15;
			this.checkBox1.Text = "Using DiSEqC";
			this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
			// 
			// diseqcd
			// 
			this.diseqcd.Items.AddRange(new object[] {
														 "None",
														 "Simple A",
														 "Simple B",
														 "Level 1 A/A",
														 "Level 1 B/A",
														 "Level 1 A/B",
														 "Level 1 B/B"});
			this.diseqcd.Location = new System.Drawing.Point(16, 138);
			this.diseqcd.Name = "diseqcd";
			this.diseqcd.Size = new System.Drawing.Size(104, 21);
			this.diseqcd.TabIndex = 14;
			this.diseqcd.Text = "None";
			// 
			// diseqcc
			// 
			this.diseqcc.Items.AddRange(new object[] {
														 "None",
														 "Simple A",
														 "Simple B",
														 "Level 1 A/A",
														 "Level 1 B/A",
														 "Level 1 A/B",
														 "Level 1 B/B"});
			this.diseqcc.Location = new System.Drawing.Point(16, 114);
			this.diseqcc.Name = "diseqcc";
			this.diseqcc.Size = new System.Drawing.Size(104, 21);
			this.diseqcc.TabIndex = 13;
			this.diseqcc.Text = "None";
			// 
			// diseqcb
			// 
			this.diseqcb.Items.AddRange(new object[] {
														 "None",
														 "Simple A",
														 "Simple B",
														 "Level 1 A/A",
														 "Level 1 B/A",
														 "Level 1 A/B",
														 "Level 1 B/B"});
			this.diseqcb.Location = new System.Drawing.Point(16, 90);
			this.diseqcb.Name = "diseqcb";
			this.diseqcb.Size = new System.Drawing.Size(104, 21);
			this.diseqcb.TabIndex = 12;
			this.diseqcb.Text = "None";
			// 
			// sat4
			// 
			this.sat4.Location = new System.Drawing.Point(128, 138);
			this.sat4.Name = "sat4";
			this.sat4.Size = new System.Drawing.Size(168, 20);
			this.sat4.TabIndex = 10;
			this.sat4.Text = "";
			// 
			// sat3
			// 
			this.sat3.Location = new System.Drawing.Point(128, 114);
			this.sat3.Name = "sat3";
			this.sat3.Size = new System.Drawing.Size(168, 20);
			this.sat3.TabIndex = 9;
			this.sat3.Text = "";
			// 
			// sat2
			// 
			this.sat2.Location = new System.Drawing.Point(128, 90);
			this.sat2.Name = "sat2";
			this.sat2.Size = new System.Drawing.Size(168, 20);
			this.sat2.TabIndex = 8;
			this.sat2.Text = "";
			// 
			// button4
			// 
			this.button4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button4.Location = new System.Drawing.Point(303, 139);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(24, 16);
			this.button4.TabIndex = 7;
			this.button4.Text = "...";
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// button3
			// 
			this.button3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button3.Location = new System.Drawing.Point(303, 115);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(24, 16);
			this.button3.TabIndex = 6;
			this.button3.Text = "...";
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button2
			// 
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button2.Location = new System.Drawing.Point(303, 91);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(24, 16);
			this.button2.TabIndex = 5;
			this.button2.Text = "...";
			// 
			// sat1
			// 
			this.sat1.Location = new System.Drawing.Point(128, 66);
			this.sat1.Name = "sat1";
			this.sat1.Size = new System.Drawing.Size(168, 20);
			this.sat1.TabIndex = 4;
			this.sat1.Text = "";
			// 
			// button1
			// 
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button1.Location = new System.Drawing.Point(303, 67);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(24, 16);
			this.button1.TabIndex = 3;
			this.button1.Text = "...";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 48);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(56, 16);
			this.label5.TabIndex = 2;
			this.label5.Text = "Satellites:";
			// 
			// diseqca
			// 
			this.diseqca.Items.AddRange(new object[] {
														 "None",
														 "Simple A",
														 "Simple B",
														 "Level 1 A/A",
														 "Level 1 B/A",
														 "Level 1 A/B",
														 "Level 1 B/B"});
			this.diseqca.Location = new System.Drawing.Point(16, 66);
			this.diseqca.Name = "diseqca";
			this.diseqca.Size = new System.Drawing.Size(104, 21);
			this.diseqca.TabIndex = 1;
			this.diseqca.Text = "None";
			this.diseqca.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(72, 366);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(352, 16);
			this.progressBar1.TabIndex = 4;
			// 
			// treeView1
			// 
			this.treeView1.ImageIndex = -1;
			this.treeView1.Location = new System.Drawing.Point(16, 32);
			this.treeView1.Name = "treeView1";
			this.treeView1.SelectedImageIndex = -1;
			this.treeView1.Size = new System.Drawing.Size(408, 232);
			this.treeView1.TabIndex = 5;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 8);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(56, 16);
			this.label6.TabIndex = 6;
			this.label6.Text = "Channels:";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(8, 307);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(40, 16);
			this.label8.TabIndex = 7;
			this.label8.Text = "Scan:";
			// 
			// comboBox3
			// 
			this.comboBox3.Location = new System.Drawing.Point(72, 304);
			this.comboBox3.Name = "comboBox3";
			this.comboBox3.Size = new System.Drawing.Size(264, 21);
			this.comboBox3.TabIndex = 8;
			this.comboBox3.SelectedIndexChanged += new System.EventHandler(this.comboBox3_SelectedIndexChanged);
			// 
			// button5
			// 
			this.button5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button5.Location = new System.Drawing.Point(344, 304);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(80, 21);
			this.button5.TabIndex = 9;
			this.button5.Text = "Start";
			this.button5.Click += new System.EventHandler(this.button5_Click);
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(8, 366);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(56, 16);
			this.label9.TabIndex = 10;
			this.label9.Text = "Progress:";
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(448, 423);
			this.tabControl1.TabIndex = 11;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.Channelmanagment_SelectedIndexChanged);
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.groupBox1);
			this.tabPage1.Controls.Add(this.groupBox2);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(440, 397);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Card config";
			this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.checkBox2);
			this.tabPage2.Controls.Add(this.feedback);
			this.tabPage2.Controls.Add(this.label1);
			this.tabPage2.Controls.Add(this.radioButton2);
			this.tabPage2.Controls.Add(this.radioButton1);
			this.tabPage2.Controls.Add(this.button5);
			this.tabPage2.Controls.Add(this.comboBox3);
			this.tabPage2.Controls.Add(this.label8);
			this.tabPage2.Controls.Add(this.treeView1);
			this.tabPage2.Controls.Add(this.label6);
			this.tabPage2.Controls.Add(this.progressBar1);
			this.tabPage2.Controls.Add(this.label9);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(440, 397);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Channels";
			// 
			// checkBox2
			// 
			this.checkBox2.Checked = true;
			this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBox2.Location = new System.Drawing.Point(16, 272);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(184, 16);
			this.checkBox2.TabIndex = 16;
			this.checkBox2.Text = "Skip scrambled Services";
			// 
			// feedback
			// 
			this.feedback.Location = new System.Drawing.Point(72, 336);
			this.feedback.Name = "feedback";
			this.feedback.Size = new System.Drawing.Size(352, 20);
			this.feedback.TabIndex = 15;
			this.feedback.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 339);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 16);
			this.label1.TabIndex = 14;
			this.label1.Text = "Scanning:";
			// 
			// radioButton2
			// 
			this.radioButton2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioButton2.Location = new System.Drawing.Point(184, 8);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(56, 16);
			this.radioButton2.TabIndex = 12;
			this.radioButton2.Text = "Radio";
			this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
			// 
			// radioButton1
			// 
			this.radioButton1.Checked = true;
			this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioButton1.Location = new System.Drawing.Point(88, 8);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(80, 16);
			this.radioButton1.TabIndex = 11;
			this.radioButton1.TabStop = true;
			this.radioButton1.Text = "Television";
			this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.treeView2);
			this.tabPage3.Controls.Add(this.button6);
			this.tabPage3.Controls.Add(this.propertyGrid1);
			this.tabPage3.Controls.Add(this.label2);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(440, 397);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Channel-Edit";
			// 
			// treeView2
			// 
			this.treeView2.HideSelection = false;
			this.treeView2.ImageIndex = -1;
			this.treeView2.Location = new System.Drawing.Point(24, 32);
			this.treeView2.Name = "treeView2";
			this.treeView2.SelectedImageIndex = -1;
			this.treeView2.Size = new System.Drawing.Size(392, 104);
			this.treeView2.TabIndex = 6;
			this.treeView2.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView2_AfterSelect);
			// 
			// button6
			// 
			this.button6.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button6.Location = new System.Drawing.Point(336, 368);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(80, 24);
			this.button6.TabIndex = 3;
			this.button6.Text = "Apply";
			this.button6.Click += new System.EventHandler(this.button6_Click_2);
			// 
			// propertyGrid1
			// 
			this.propertyGrid1.CommandsVisibleIfAvailable = true;
			this.propertyGrid1.HelpVisible = false;
			this.propertyGrid1.LargeButtons = false;
			this.propertyGrid1.LineColor = System.Drawing.SystemColors.ScrollBar;
			this.propertyGrid1.Location = new System.Drawing.Point(24, 144);
			this.propertyGrid1.Name = "propertyGrid1";
			this.propertyGrid1.Size = new System.Drawing.Size(392, 216);
			this.propertyGrid1.TabIndex = 2;
			this.propertyGrid1.Text = "propertyGrid1";
			this.propertyGrid1.ToolbarVisible = false;
			this.propertyGrid1.ViewBackColor = System.Drawing.SystemColors.Window;
			this.propertyGrid1.ViewForeColor = System.Drawing.SystemColors.WindowText;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "Channels:";
			// 
			// ofd
			// 
			this.ofd.Filter = "Transponder-Listings (*.tpl)|*.tpl";
			this.ofd.Title = "Choose Transponder-Listing Files";
			// 
			// DVBSSS2
			// 
			this.Controls.Add(this.tabControl1);
			this.Name = "DVBSSS2";
			this.Size = new System.Drawing.Size(448, 456);
			this.groupBox2.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		}

		private void Channelmanagment_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			
			comboBox3.Items.Clear();
			if(sat1.Text.Length>0)
				comboBox3.Items.Add(sat1.Text);
			if(sat2.Text.Length>0)
				comboBox3.Items.Add(sat2.Text);
			if(sat3.Text.Length>0)
				comboBox3.Items.Add(sat3.Text);
			if(sat4.Text.Length>0)
				comboBox3.Items.Add(sat4.Text);
			// disable ... 
			comboBox3.SelectedIndex=-1;
			button5.Enabled=false;
			if(tabControl1.SelectedIndex==2)
			{
				GetChannels();
				//BuildUpTreeView(tvList);
			}

		}

		private void comboBox2_SelectedIndexChanged(object sender, System.EventArgs e)
		{

		}
		private void SaveConfig()
		{
			string a=sat1.Text;
			string b=sat2.Text;
			string c=sat3.Text;
			string d=sat4.Text;
			string path=@Application.StartupPath+@"\";
			try
			{

				using(AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml(path+"MediaPortal.xml"))
				{
					xmlwriter.SetValueAsBool("DVBSS2","use_diseqc",checkBox1.Checked);
					xmlwriter.SetValue("DVBSS2","diseqca",diseqca.SelectedIndex);
					xmlwriter.SetValue("DVBSS2","diseqcb",diseqcb.SelectedIndex);
					xmlwriter.SetValue("DVBSS2","diseqcc",diseqcc.SelectedIndex);
					xmlwriter.SetValue("DVBSS2","diseqcd",diseqcd.SelectedIndex);
					xmlwriter.SetValue("DVBSS2","lnbconfig1",lnbconfig1.SelectedIndex);
					xmlwriter.SetValue("DVBSS2","lnbconfig2",lnbconfig2.SelectedIndex);
					xmlwriter.SetValue("DVBSS2","lnbconfig3",lnbconfig3.SelectedIndex);
					xmlwriter.SetValue("DVBSS2","lnbconfig4",lnbconfig4.SelectedIndex);					xmlwriter.SetValue("DVBSS2","sata",a);
					xmlwriter.SetValue("DVBSS2","satb",b);
					xmlwriter.SetValue("DVBSS2","satc",c);
					xmlwriter.SetValue("DVBSS2","satd",d);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}			
		}
		void LoadConfig()
		{
			string path=@Application.StartupPath+@"\";

			try
			{
				using(AMS.Profile.Xml xmlreader = new AMS.Profile.Xml(path+"MediaPortal.xml"))
				{
					checkBox1.Checked=xmlreader.GetValueAsBool("DVBSS2","use_diseqc",checkBox1.Checked);
					diseqca.SelectedIndex=xmlreader.GetValueAsInt("DVBSS2","diseqca",0);
					lnbconfig1.SelectedIndex=xmlreader.GetValueAsInt("DVBSS2","lnbconfig1",0);
					sat1.Text=xmlreader.GetValueAsString("DVBSS2","sata","");
					if(checkBox1.Checked==false)
						DisableSatConfig();
					else
					{
						EnableSatConfig();
						diseqcb.SelectedIndex=xmlreader.GetValueAsInt("DVBSS2","diseqcb",0);
						diseqcc.SelectedIndex=xmlreader.GetValueAsInt("DVBSS2","diseqcc",0);	
						diseqcd.SelectedIndex=xmlreader.GetValueAsInt("DVBSS2","diseqcd",0);	
						lnbconfig2.SelectedIndex=xmlreader.GetValueAsInt("DVBSS2","lnbconfig2",0);
						lnbconfig3.SelectedIndex=xmlreader.GetValueAsInt("DVBSS2","lnbconfig3",0);
						lnbconfig4.SelectedIndex=xmlreader.GetValueAsInt("DVBSS2","lnbconfig4",0);
						sat2.Text=xmlreader.GetValueAsString("DVBSS2","satb","");
						sat3.Text=xmlreader.GetValueAsString("DVBSS2","satc","");
						sat4.Text=xmlreader.GetValueAsString("DVBSS2","satd","");
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
		private void button6_Click(object sender, System.EventArgs e)
		{
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			DialogResult res=DialogResult.No;

			res=ofd.ShowDialog();
			if(res==DialogResult.OK)
			{
				sat1.Text=ofd.FileName;
			}
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			DialogResult res=DialogResult.No;

			res=ofd.ShowDialog();
			if(res==DialogResult.OK)
			{
				sat2.Text=ofd.FileName;
			}

		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			DialogResult res=DialogResult.No;

			res=ofd.ShowDialog();
			if(res==DialogResult.OK)
			{
				sat3.Text=ofd.FileName;
			}

		}

		private void button4_Click(object sender, System.EventArgs e)
		{
			DialogResult res=DialogResult.No;

			res=ofd.ShowDialog();
			if(res==DialogResult.OK)
			{
				sat4.Text=ofd.FileName;
			}

		}



		public override void SaveSettings()
		{
			SaveConfig();
			SaveChannelList();
		}


		private void checkBox1_CheckedChanged(object sender, System.EventArgs e)
		{
			if(checkBox1.Checked==false)
			{
				DisableSatConfig();
			}
			else
			{
				EnableSatConfig();
			}
		}
		private void DisableSatConfig()
		{
				diseqcb.Enabled=
				diseqcc.Enabled=
				diseqcd.Enabled=	
				sat2.Enabled=
				sat3.Enabled=
					lnbconfig2.Enabled=
					lnbconfig3.Enabled=
					lnbconfig4.Enabled=
				button2.Enabled=
				button3.Enabled=
				button4.Enabled=
				sat4.Enabled=false;

		}
		private void EnableSatConfig()
		{
				diseqcb.Enabled=
				diseqcc.Enabled=
				diseqcd.Enabled=	
				sat2.Enabled=
				sat3.Enabled=
				lnbconfig2.Enabled=
				lnbconfig3.Enabled=
				lnbconfig4.Enabled=
				button2.Enabled=
				button3.Enabled=
				button4.Enabled=
				sat4.Enabled=true;

		}


		private void comboBox3_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(comboBox3.SelectedItem.ToString()!="")
				button5.Enabled=true;
			else
				button5.Enabled=false;
		}

		private void button5_Click(object sender, System.EventArgs e)
		{
			DVBSections.Transponder[] list=null;
			string					fileName=comboBox3.SelectedItem.ToString();
			int						dIndex=comboBox3.SelectedIndex;
			int						lnbkhz=0;
			int						diseqc=0;
			int						lnb_0=0;
			int						lnb_1=0;
			int						lnb_switch=0;

			button5.Enabled=false;
			try
			{

				switch (dIndex)
				{
					case 0:
						diseqc=diseqca.SelectedIndex;
						lnbkhz=lnbconfig1.SelectedIndex;
						break;
					case 1:
						diseqc=diseqcb.SelectedIndex;
						lnbkhz=lnbconfig2.SelectedIndex;
						break;
					case 2:
						diseqc=diseqcc.SelectedIndex;
						lnbkhz=lnbconfig3.SelectedIndex;
						break;
					case 3:
						diseqc=diseqcd.SelectedIndex;
						lnbkhz=lnbconfig4.SelectedIndex;
						break;

				}

				lnb_0=9750;
				lnb_1=10600;
				lnb_switch=11700;

			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
				return;
			}

			//int lnbkhz=lnbselect.SelectedIndex;


			if(comboBox3.SelectedItem.ToString()!="")
			{
				if(System.IO.File.Exists(fileName)==true)
				{
					if(m_dvbSec.Run()==true)
					{
						m_dvbSec.OpenTPLFile(comboBox3.SelectedItem.ToString(),ref list,diseqc,lnbkhz,lnb_0,lnb_1,lnb_switch,progressBar1,feedback);
						
						// setting up list
						transpList=(DVBSections.Transponder[])list.Clone();
						m_dvbSec.CleanUp();
						BuildUpTreeView(treeView1);

					}// dvbsec.run

				}// file exists
			}
			button5.Enabled=true;

		}

		private void BuildUpTreeView(TreeView tvObj)
		{
			ArrayList					provider=new ArrayList();
			ArrayList					channels=new ArrayList();
			int							reqService=0;
			string						path=@Application.StartupPath+@"\";
			DVBSections.Transponder[]	list=transpList;
			//using(AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml(path+"dvbss_channels.xml"))
			// clear list
			if(list==null)
				return;

			if(list.Length==0)
				return;

			tvObj.Nodes.Clear();
			
			// radio television
			
			if(radioButton1.Checked==true)
				reqService=1;
			else
				reqService=2;

			if(tvObj.Name=="tvList")
				reqService=1;

			if(tvObj.Name=="radioList")
				reqService=2;
			// ckeck providers
			foreach(DVBSections.Transponder transponder in list)
			{
				foreach(DVBSections.ChannelInfo ch in transponder.channels)
				{
					string provName=ch.service_provider_name;
					int service=ch.serviceType;
					if(service==reqService)
					{
						if(provName.Length>0)
						{
							bool flag=false;
							foreach(TreeNode tn in tvObj.Nodes)
							{
								string tagText=(string)tn.Tag;

								if(tagText==provName)
								{
									flag=true;
									break;
								}
							}
							if(flag==false)
							{
								TreeNode node=new TreeNode();
								node.Tag=provName;
								node.Text=provName;
								tvObj.Nodes.Add(node);
								provider.Add(node);
							}
						}
					}

				}
			}

			// add the cannels
			foreach(DVBSections.Transponder transponder in list)
			{
				foreach(DVBSections.ChannelInfo ch in transponder.channels)
				{
					string provName=ch.service_provider_name;
					string servName=ch.service_name;
					int		service=ch.serviceType;
					if(ch.scrambled==true && checkBox2.Checked==true)
						continue;

					if(service==reqService)
					{				
						if(provName.Length>0 && servName.Length>0)
						{
							TreeNode tn=GetNodeByTag(provName,provider);
							if(tn!=null)
							{// add channel nodes
								TreeNode node=new TreeNode(servName);
								node.Tag="channel";
								foreach(DVBSections.PMTData pids in ch.pid_list)
								{
									string descrStream="unknown stream (type "+pids.stream_type.ToString()+")";
									if(pids.isVideo==true)
										descrStream="Video";
									if(pids.isAudio==true)
										descrStream="Audio";
									if(pids.isAC3Audio==true)
										descrStream="AC3";
									if(pids.isTeletext==true)
										descrStream="Teletext ("+pids.teletextLANG+")";
									node.Nodes.Add(new TreeNode("Pid :"+pids.elementary_PID.ToString()+" (Stream-Type: "+descrStream+"// Data: "+pids.data+" )"));
								}
								
															
								//node.Nodes.Add(new TreeNode("Service-ID:"+ch.program_number.ToString()));
								//							node.Nodes.Add(new TreeNode("PCR-Pid:"+ch.pcr_pid.ToString()));
								//							node.Nodes.Add(new TreeNode("Video-Pid:"+GetVideoPid(ch.pid_list)));
								tn.Nodes.Add(node);
								//treeView1.
							}
						}
					}
				}
				
			}
	
				
		
		}// function
		private TreeNode GetNodeByTag(string tag,ArrayList treeNodes)
		{
			foreach(TreeNode tn in treeNodes)
			{
				string tagText=(string)tn.Tag;
				if(tagText==tag)
					return tn;
			}
			return null;
		}



		private void radioButton1_CheckedChanged(object sender, System.EventArgs e)
		{
			if(transpList!=null)
				BuildUpTreeView(treeView1);
		}

		private void radioButton2_CheckedChanged(object sender, System.EventArgs e)
		{
			if(transpList!=null)
				BuildUpTreeView(treeView1);

		}

		private void tabPage1_Click(object sender, System.EventArgs e)
		{
		
		}

		private void comboBox4_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}


		private void tvList_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
		
		}
		private void SaveChannelList()
		{
			DVBSections.Transponder[]	list=transpList;
			int televisionCounter=1;
			int radioCounter=1;

			// clear sat database
			TVDatabase.RemoveAllSatChannels();

			if(list==null)
			{
				//System.Windows.Forms.MessageBox.Show("Do a Channelsearch first!");
				return;
			}
			
			foreach(DVBSections.Transponder transponder in list)
			{
				foreach(DVBSections.ChannelInfo ch in transponder.channels)
				{
					if(ch.scrambled==true && checkBox2.Checked==true)
						continue;
					if(ch.serviceType==1) // television
					{
						int ret=-1;
						DShowNET.AnalogVideoStandard standard=new DShowNET.AnalogVideoStandard();
						standard=DShowNET.AnalogVideoStandard.None;
						string channelText=ch.service_name;
						if(channelText==null)
							channelText="unnamed service "+televisionCounter.ToString();
						TVChannel tv=new TVChannel();
						tv.VisibleInGuide=true;
						tv.Name=channelText;//+" ("+n.ToString()+")";
						tv.Frequency=0;
						tv.Number=televisionCounter;
						tv.XMLId="";
						int dbID=TVDatabase.AddChannel(tv);
						if(dbID==-1)
							continue;
						tv.ID=dbID;
						int audioPid=GetAudioPid(ch.pid_list);
						int videoPid=GetVideoPid(ch.pid_list);
						int teleTextPid=GetTeletextPid(ch.pid_list);
						int ac3Pid=GetAC3Pid(ch.pid_list);
						int[] otherAudio=GetOtherAudioPids(ch.pid_list,audioPid);
						if(otherAudio==null)
							otherAudio=new int[]{0,0,0};

						ret=TVDatabase.AddSatChannel(dbID,ch.freq,ch.symb,6,ch.lnbkhz,ch.diseqc,ch.program_number,
							ch.serviceType,ch.service_provider_name,tv.Name,(ch.eitSchedule?1:0),(ch.eitPreFollow?1:0),
							audioPid,videoPid,ac3Pid,otherAudio[0],otherAudio[1],otherAudio[2],teleTextPid,(ch.scrambled?1:0),ch.pol,ch.lnb01,ch.networkID,ch.transportStreamID,ch.pcr_pid);
						televisionCounter++;
					}
					if(ch.serviceType==2) // for radio
					{
						MediaPortal.Radio.Database.RadioStation rc=new MediaPortal.Radio.Database.RadioStation();
						string channelText=ch.service_name;
						if(channelText==null)
							channelText="unnamed service "+radioCounter.ToString();
						rc.Name=channelText;
						
						rc.URL="";
						rc.Genre="DVB-Radio Service";
						rc.Frequency=(radioCounter<< 16);
						
						int dbID=RadioDatabase.AddStation(ref rc);
						if(dbID==-1)
							continue;
						int audioPid=GetAudioPid(ch.pid_list);
						int ret=TVDatabase.AddSatChannel(radioCounter,ch.freq,ch.symb,6,ch.lnbkhz,ch.diseqc,ch.program_number,
							ch.serviceType,ch.service_provider_name,rc.Name,(ch.eitSchedule?1:0),(ch.eitPreFollow?1:0),
							audioPid,0,0,0,0,0,0,(ch.scrambled?1:0),ch.pol,ch.lnb01,ch.networkID,ch.transportStreamID,ch.pcr_pid);
						radioCounter++;

					}
						

				}

			}
		}
		private void button7_Click(object sender, System.EventArgs e)
		{
			SaveChannelList();
		}
		//
		private int GetAudioPid(ArrayList ch)
		{
			if(ch==null)
				return -1;
			foreach(DVBSections.PMTData pids in ch)
				if(pids.isAudio)
					return pids.elementary_PID;
			return 0;

		}
		//
		private int GetVideoPid(ArrayList ch)
		{
			if(ch==null)
				return -1;
			foreach(DVBSections.PMTData pids in ch)
				if(pids.isVideo)
					return pids.elementary_PID;
			return 0;
		}
		private int GetTeletextPid(ArrayList ch)
		{
			if(ch==null)
				return -1;
			foreach(DVBSections.PMTData pids in ch)
				if(pids.isTeletext)
					return pids.elementary_PID;
			return 0;
		}
		private int GetAC3Pid(ArrayList ch)
		{
			if(ch==null)
				return -1;
			foreach(DVBSections.PMTData pids in ch)
				if(pids.isAC3Audio)
					return pids.elementary_PID;
			return 0;
		}
		private int[] GetOtherAudioPids(ArrayList ch,int audio)
		{
			int[] pidArray=new int[3]{0,0,0};

			int n=0;
			if(ch==null)
				return null;
			foreach(DVBSections.PMTData pids in ch)
			{
				if(pids.isAudio && pids.elementary_PID!=audio)
				{pidArray[n]=pids.elementary_PID;n++;}
			}
			return null;
		}

		private void button9_Click(object sender, System.EventArgs e)
		{
//			treeView2.Nodes.Clear();
//			System.Collections.Hashtable	ht=new Hashtable();
//			System.Collections.Hashtable	se=new Hashtable();
//			DVBSections						sec=new DVBSections();
//			DVBSections.EIT_Program_Info	epi=new MediaPortal.Configuration.Sections.DVBSections.EIT_Program_Info();
//			int								maxTables=0x51;
//			sec.Run();
//			int count=0;
//			epi.eitList=new ArrayList();
//
//				
//			for(int tabs=0x50;tabs<maxTables;tabs++)
//			{
//				GC.Collect();
//				progressBar2.Value=0;
//				label7.Text=tabs.ToString();
//				treeView2.Nodes.Clear();
//				for(int i=0;i<25000;i++) // run 500 times
//				{
//					epi.eitList.Clear();
//					epi.eitList=sec.GetEITSchedule(tabs);
//					foreach(DVBSections.EITDescr eitInfo in epi.eitList)
//					{
//					
//						maxTables=eitInfo.lastTable;
//						textBox1.Text=count.ToString();
//						sec.SetEITToDatabase(eitInfo);
//						if(ht.ContainsKey(eitInfo.event_id)==false)
//						{
//							treeView2.Nodes.Add(eitInfo.event_name+" "+eitInfo.program_number.ToString());
//							ht.Add(eitInfo.event_id,eitInfo.event_id);
//						}
//					}
//					progressBar2.Value=i;
//
//				}
//			}
//			maxTables=0x61;
//			for(int tabs=0x60;tabs<maxTables;tabs++)
//			{
//				GC.Collect();
//				progressBar2.Value=0;
//				label7.Text=tabs.ToString();
//				treeView2.Nodes.Clear();
//				for(int i=0;i<25000;i++) // run 500 times
//				{
//					epi.eitList.Clear();
//					epi.eitList=sec.GetEITSchedule(tabs);
//					foreach(DVBSections.EITDescr eitInfo in epi.eitList)
//					{
//					
//						maxTables=eitInfo.lastTable;
//						textBox1.Text=count.ToString();
//						sec.SetEITToDatabase(eitInfo);
//						if(ht.ContainsKey(eitInfo.event_id)==false)
//						{
//							treeView2.Nodes.Add(eitInfo.event_name+" "+eitInfo.program_number.ToString());
//							ht.Add(eitInfo.event_id,eitInfo.event_id);
//						}
//					}
//					progressBar2.Value=i;
//
//				}
//			}
//			
//			
//			System.Windows.Forms.MessageBox.Show("Ready.");
//			sec.CleanUp();

		}


		private void GetChannels()
		{
			ArrayList chList=new ArrayList();
			TVDatabase.GetSatChannels(ref chList);
			if(chList==null)
				return;
			treeView2.Nodes.Clear();
			foreach(DVBChannel channel in chList)
			{
				if(channel.ToString()!="")
				{
					TreeNode node=new TreeNode(channel.ServiceName);
					node.Tag=channel;
					treeView2.Nodes.Add(node);
				}
			}
		}

		private void button6_Click_2(object sender, System.EventArgs e)
		{
			if(propertyGrid1.SelectedObject !=null)
			{
				TVDatabase.UpdateSatChannel((DVBChannel)propertyGrid1.SelectedObject);
				GetChannels();
			}
		}

		private void treeView2_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			propertyGrid1.SelectedObject=e.Node.Tag;
		}



	}// class
}// namespace
