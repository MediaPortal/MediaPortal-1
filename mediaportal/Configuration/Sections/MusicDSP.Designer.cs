namespace MediaPortal.Configuration.Sections
{
  partial class MusicDSP
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.DSPTabPg = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.label11 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label17 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonSetGain = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBoxGainDBValue = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label23 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label14 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.trackBarGain = new System.Windows.Forms.TrackBar();
      this.groupBoxGain = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBoxDynamicAmplification = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkBoxDAmp = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxCompressor = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label22 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelCompThreshold = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label13 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxCompressor = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.trackBarCompressor = new System.Windows.Forms.TrackBar();
      this.MusicDSPTabCtl = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.VSTTagPg = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonVSTSearch = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonSelectVSTDir = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBoxVSTPluginDir = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonVSTRemove = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonVSTAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.listBoxSelectedVSTPlugins = new System.Windows.Forms.ListBox();
      this.listBoxFoundVSTPlugins = new System.Windows.Forms.ListBox();
      this.WinampTabPg = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.label7 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.buttonWARemove = new System.Windows.Forms.Button();
      this.buttonWAAdd = new System.Windows.Forms.Button();
      this.listBoxSelectedWAPlugins = new System.Windows.Forms.ListBox();
      this.listBoxFoundWAPlugins = new System.Windows.Forms.ListBox();
      this.buttonWASearch = new System.Windows.Forms.Button();
      this.buttonSelectWADir = new System.Windows.Forms.Button();
      this.textBoxWAPluginDir = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.btFileselect = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBoxMusicFile = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btStop = new MediaPortal.UserInterface.Controls.MPButton();
      this.btPlay = new MediaPortal.UserInterface.Controls.MPButton();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.DSPTabPg.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarGain)).BeginInit();
      this.groupBoxGain.SuspendLayout();
      this.groupBoxCompressor.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarCompressor)).BeginInit();
      this.MusicDSPTabCtl.SuspendLayout();
      this.VSTTagPg.SuspendLayout();
      this.WinampTabPg.SuspendLayout();
      this.SuspendLayout();
      // 
      // DSPTabPg
      // 
      this.DSPTabPg.BackColor = System.Drawing.SystemColors.Control;
      this.DSPTabPg.Controls.Add(this.label11);
      this.DSPTabPg.Controls.Add(this.label17);
      this.DSPTabPg.Controls.Add(this.buttonSetGain);
      this.DSPTabPg.Controls.Add(this.textBoxGainDBValue);
      this.DSPTabPg.Controls.Add(this.label23);
      this.DSPTabPg.Controls.Add(this.label14);
      this.DSPTabPg.Controls.Add(this.trackBarGain);
      this.DSPTabPg.Controls.Add(this.groupBoxGain);
      this.DSPTabPg.Controls.Add(this.groupBoxCompressor);
      this.DSPTabPg.Location = new System.Drawing.Point(4, 22);
      this.DSPTabPg.Name = "DSPTabPg";
      this.DSPTabPg.Padding = new System.Windows.Forms.Padding(3);
      this.DSPTabPg.Size = new System.Drawing.Size(458, 305);
      this.DSPTabPg.TabIndex = 1;
      this.DSPTabPg.Text = "BASS DSP / FX";
      // 
      // label11
      // 
      this.label11.Location = new System.Drawing.Point(337, 207);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(38, 23);
      this.label11.TabIndex = 74;
      this.label11.Text = "-25dB";
      this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label17
      // 
      this.label17.Location = new System.Drawing.Point(92, 80);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(38, 23);
      this.label17.TabIndex = 69;
      this.label17.Text = "+16dB";
      this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonSetGain
      // 
      this.buttonSetGain.Location = new System.Drawing.Point(100, 242);
      this.buttonSetGain.Name = "buttonSetGain";
      this.buttonSetGain.Size = new System.Drawing.Size(54, 23);
      this.buttonSetGain.TabIndex = 67;
      this.buttonSetGain.Text = "Set";
      this.buttonSetGain.UseVisualStyleBackColor = true;
      this.buttonSetGain.Click += new System.EventHandler(this.buttonSetGain_Click);
      // 
      // textBoxGainDBValue
      // 
      this.textBoxGainDBValue.BorderColor = System.Drawing.Color.Empty;
      this.textBoxGainDBValue.Location = new System.Drawing.Point(59, 244);
      this.textBoxGainDBValue.Name = "textBoxGainDBValue";
      this.textBoxGainDBValue.Size = new System.Drawing.Size(40, 20);
      this.textBoxGainDBValue.TabIndex = 66;
      this.textBoxGainDBValue.Text = "0";
      this.textBoxGainDBValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // label23
      // 
      this.label23.Location = new System.Drawing.Point(101, 144);
      this.label23.Name = "label23";
      this.label23.Size = new System.Drawing.Size(38, 23);
      this.label23.TabIndex = 70;
      this.label23.Text = "0dB";
      this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label14
      // 
      this.label14.Location = new System.Drawing.Point(96, 205);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(38, 23);
      this.label14.TabIndex = 68;
      this.label14.Text = "-16dB";
      this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // trackBarGain
      // 
      this.trackBarGain.LargeChange = 100;
      this.trackBarGain.Location = new System.Drawing.Point(66, 84);
      this.trackBarGain.Maximum = 16000;
      this.trackBarGain.Minimum = -16000;
      this.trackBarGain.Name = "trackBarGain";
      this.trackBarGain.Orientation = System.Windows.Forms.Orientation.Vertical;
      this.trackBarGain.Size = new System.Drawing.Size(45, 143);
      this.trackBarGain.SmallChange = 10;
      this.trackBarGain.TabIndex = 65;
      this.trackBarGain.TickFrequency = 2000;
      this.trackBarGain.ValueChanged += new System.EventHandler(this.trackBarGain_ValueChanged);
      // 
      // groupBoxGain
      // 
      this.groupBoxGain.Controls.Add(this.comboBoxDynamicAmplification);
      this.groupBoxGain.Controls.Add(this.checkBoxDAmp);
      this.groupBoxGain.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGain.Location = new System.Drawing.Point(44, 6);
      this.groupBoxGain.Name = "groupBoxGain";
      this.groupBoxGain.Size = new System.Drawing.Size(151, 293);
      this.groupBoxGain.TabIndex = 71;
      this.groupBoxGain.TabStop = false;
      this.groupBoxGain.Text = "Gain";
      // 
      // comboBoxDynamicAmplification
      // 
      this.comboBoxDynamicAmplification.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxDynamicAmplification.Enabled = false;
      this.comboBoxDynamicAmplification.FormattingEnabled = true;
      this.comboBoxDynamicAmplification.Items.AddRange(new object[] {
            "Soft",
            "Medium",
            "Hard"});
      this.comboBoxDynamicAmplification.Location = new System.Drawing.Point(7, 39);
      this.comboBoxDynamicAmplification.Name = "comboBoxDynamicAmplification";
      this.comboBoxDynamicAmplification.Size = new System.Drawing.Size(121, 21);
      this.comboBoxDynamicAmplification.TabIndex = 72;
      // 
      // checkBoxDAmp
      // 
      this.checkBoxDAmp.AutoSize = true;
      this.checkBoxDAmp.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDAmp.Location = new System.Drawing.Point(7, 18);
      this.checkBoxDAmp.Name = "checkBoxDAmp";
      this.checkBoxDAmp.Size = new System.Drawing.Size(127, 17);
      this.checkBoxDAmp.TabIndex = 72;
      this.checkBoxDAmp.Text = "Dynamic Amplification";
      this.checkBoxDAmp.UseVisualStyleBackColor = true;
      this.checkBoxDAmp.CheckedChanged += new System.EventHandler(this.checkBoxDAmp_CheckedChanged);
      // 
      // groupBoxCompressor
      // 
      this.groupBoxCompressor.Controls.Add(this.label22);
      this.groupBoxCompressor.Controls.Add(this.labelCompThreshold);
      this.groupBoxCompressor.Controls.Add(this.label13);
      this.groupBoxCompressor.Controls.Add(this.checkBoxCompressor);
      this.groupBoxCompressor.Controls.Add(this.trackBarCompressor);
      this.groupBoxCompressor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxCompressor.Location = new System.Drawing.Point(273, 6);
      this.groupBoxCompressor.Name = "groupBoxCompressor";
      this.groupBoxCompressor.Size = new System.Drawing.Size(151, 293);
      this.groupBoxCompressor.TabIndex = 73;
      this.groupBoxCompressor.TabStop = false;
      this.groupBoxCompressor.Text = "Compressor";
      // 
      // label22
      // 
      this.label22.Location = new System.Drawing.Point(74, 113);
      this.label22.Name = "label22";
      this.label22.Size = new System.Drawing.Size(38, 23);
      this.label22.TabIndex = 77;
      this.label22.Text = "-6dB";
      this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelCompThreshold
      // 
      this.labelCompThreshold.Location = new System.Drawing.Point(12, 53);
      this.labelCompThreshold.Name = "labelCompThreshold";
      this.labelCompThreshold.Size = new System.Drawing.Size(110, 23);
      this.labelCompThreshold.TabIndex = 75;
      this.labelCompThreshold.Text = "Threshold: -6.0 dB";
      this.labelCompThreshold.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label13
      // 
      this.label13.Location = new System.Drawing.Point(74, 84);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(38, 23);
      this.label13.TabIndex = 76;
      this.label13.Text = "-0dB";
      this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // checkBoxCompressor
      // 
      this.checkBoxCompressor.AutoSize = true;
      this.checkBoxCompressor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxCompressor.Location = new System.Drawing.Point(15, 14);
      this.checkBoxCompressor.Name = "checkBoxCompressor";
      this.checkBoxCompressor.Size = new System.Drawing.Size(79, 17);
      this.checkBoxCompressor.TabIndex = 72;
      this.checkBoxCompressor.Text = "Compressor";
      this.checkBoxCompressor.UseVisualStyleBackColor = true;
      this.checkBoxCompressor.CheckedChanged += new System.EventHandler(this.checkBoxCompressor_CheckedChanged);
      // 
      // trackBarCompressor
      // 
      this.trackBarCompressor.Location = new System.Drawing.Point(23, 84);
      this.trackBarCompressor.Maximum = 0;
      this.trackBarCompressor.Minimum = -250;
      this.trackBarCompressor.Name = "trackBarCompressor";
      this.trackBarCompressor.Orientation = System.Windows.Forms.Orientation.Vertical;
      this.trackBarCompressor.Size = new System.Drawing.Size(45, 143);
      this.trackBarCompressor.TabIndex = 73;
      this.trackBarCompressor.TickFrequency = 25;
      this.trackBarCompressor.Value = -60;
      this.trackBarCompressor.ValueChanged += new System.EventHandler(this.trackBarCompressor_ValueChanged);
      // 
      // MusicDSPTabCtl
      // 
      this.MusicDSPTabCtl.Controls.Add(this.DSPTabPg);
      this.MusicDSPTabCtl.Controls.Add(this.VSTTagPg);
      this.MusicDSPTabCtl.Controls.Add(this.WinampTabPg);
      this.MusicDSPTabCtl.Location = new System.Drawing.Point(3, 3);
      this.MusicDSPTabCtl.Name = "MusicDSPTabCtl";
      this.MusicDSPTabCtl.SelectedIndex = 0;
      this.MusicDSPTabCtl.Size = new System.Drawing.Size(466, 331);
      this.MusicDSPTabCtl.TabIndex = 0;
      // 
      // VSTTagPg
      // 
      this.VSTTagPg.BackColor = System.Drawing.Color.Transparent;
      this.VSTTagPg.Controls.Add(this.label5);
      this.VSTTagPg.Controls.Add(this.label4);
      this.VSTTagPg.Controls.Add(this.label3);
      this.VSTTagPg.Controls.Add(this.buttonVSTSearch);
      this.VSTTagPg.Controls.Add(this.buttonSelectVSTDir);
      this.VSTTagPg.Controls.Add(this.textBoxVSTPluginDir);
      this.VSTTagPg.Controls.Add(this.label2);
      this.VSTTagPg.Controls.Add(this.buttonVSTRemove);
      this.VSTTagPg.Controls.Add(this.buttonVSTAdd);
      this.VSTTagPg.Controls.Add(this.listBoxSelectedVSTPlugins);
      this.VSTTagPg.Controls.Add(this.listBoxFoundVSTPlugins);
      this.VSTTagPg.Location = new System.Drawing.Point(4, 22);
      this.VSTTagPg.Name = "VSTTagPg";
      this.VSTTagPg.Padding = new System.Windows.Forms.Padding(3);
      this.VSTTagPg.Size = new System.Drawing.Size(458, 305);
      this.VSTTagPg.TabIndex = 2;
      this.VSTTagPg.Text = "VST";
      this.VSTTagPg.UseVisualStyleBackColor = true;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(251, 281);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(176, 13);
      this.label5.TabIndex = 20;
      this.label5.Text = "Double click for plugin configuration";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(251, 53);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(184, 13);
      this.label4.TabIndex = 19;
      this.label4.Text = "Selected Plugins in Ascending Priority";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(11, 53);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(87, 13);
      this.label3.TabIndex = 18;
      this.label3.Text = "Available Plugins";
      // 
      // buttonVSTSearch
      // 
      this.buttonVSTSearch.Location = new System.Drawing.Point(395, 15);
      this.buttonVSTSearch.Name = "buttonVSTSearch";
      this.buttonVSTSearch.Size = new System.Drawing.Size(57, 23);
      this.buttonVSTSearch.TabIndex = 17;
      this.buttonVSTSearch.Text = "Search";
      this.buttonVSTSearch.UseVisualStyleBackColor = true;
      this.buttonVSTSearch.Click += new System.EventHandler(this.buttonVSTSearch_Click);
      // 
      // buttonSelectVSTDir
      // 
      this.buttonSelectVSTDir.Location = new System.Drawing.Point(363, 15);
      this.buttonSelectVSTDir.Name = "buttonSelectVSTDir";
      this.buttonSelectVSTDir.Size = new System.Drawing.Size(25, 23);
      this.buttonSelectVSTDir.TabIndex = 16;
      this.buttonSelectVSTDir.Text = "...";
      this.buttonSelectVSTDir.UseVisualStyleBackColor = true;
      this.buttonSelectVSTDir.Click += new System.EventHandler(this.buttonSelectVSTDir_Click);
      // 
      // textBoxVSTPluginDir
      // 
      this.textBoxVSTPluginDir.BorderColor = System.Drawing.Color.Empty;
      this.textBoxVSTPluginDir.Location = new System.Drawing.Point(74, 17);
      this.textBoxVSTPluginDir.Name = "textBoxVSTPluginDir";
      this.textBoxVSTPluginDir.Size = new System.Drawing.Size(289, 20);
      this.textBoxVSTPluginDir.TabIndex = 15;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 20);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(55, 13);
      this.label2.TabIndex = 14;
      this.label2.Text = "Plugin Dir:";
      // 
      // buttonVSTRemove
      // 
      this.buttonVSTRemove.Location = new System.Drawing.Point(218, 178);
      this.buttonVSTRemove.Name = "buttonVSTRemove";
      this.buttonVSTRemove.Size = new System.Drawing.Size(28, 23);
      this.buttonVSTRemove.TabIndex = 13;
      this.buttonVSTRemove.Text = "<";
      this.buttonVSTRemove.UseVisualStyleBackColor = true;
      this.buttonVSTRemove.Click += new System.EventHandler(this.buttonVSTRemove_Click);
      // 
      // buttonVSTAdd
      // 
      this.buttonVSTAdd.Location = new System.Drawing.Point(218, 127);
      this.buttonVSTAdd.Name = "buttonVSTAdd";
      this.buttonVSTAdd.Size = new System.Drawing.Size(28, 23);
      this.buttonVSTAdd.TabIndex = 12;
      this.buttonVSTAdd.Text = ">";
      this.buttonVSTAdd.UseVisualStyleBackColor = true;
      this.buttonVSTAdd.Click += new System.EventHandler(this.buttonVSTAdd_Click);
      // 
      // listBoxSelectedVSTPlugins
      // 
      this.listBoxSelectedVSTPlugins.FormattingEnabled = true;
      this.listBoxSelectedVSTPlugins.HorizontalScrollbar = true;
      this.listBoxSelectedVSTPlugins.Location = new System.Drawing.Point(253, 72);
      this.listBoxSelectedVSTPlugins.Name = "listBoxSelectedVSTPlugins";
      this.listBoxSelectedVSTPlugins.Size = new System.Drawing.Size(195, 199);
      this.listBoxSelectedVSTPlugins.TabIndex = 11;
      this.listBoxSelectedVSTPlugins.DoubleClick += new System.EventHandler(this.listBoxSelectedVSTPlugins_DoubleClick);
      // 
      // listBoxFoundVSTPlugins
      // 
      this.listBoxFoundVSTPlugins.FormattingEnabled = true;
      this.listBoxFoundVSTPlugins.HorizontalScrollbar = true;
      this.listBoxFoundVSTPlugins.Location = new System.Drawing.Point(11, 72);
      this.listBoxFoundVSTPlugins.Name = "listBoxFoundVSTPlugins";
      this.listBoxFoundVSTPlugins.Size = new System.Drawing.Size(195, 199);
      this.listBoxFoundVSTPlugins.Sorted = true;
      this.listBoxFoundVSTPlugins.TabIndex = 10;
      // 
      // WinampTabPg
      // 
      this.WinampTabPg.BackColor = System.Drawing.Color.Transparent;
      this.WinampTabPg.Controls.Add(this.label7);
      this.WinampTabPg.Controls.Add(this.label8);
      this.WinampTabPg.Controls.Add(this.label9);
      this.WinampTabPg.Controls.Add(this.buttonWARemove);
      this.WinampTabPg.Controls.Add(this.buttonWAAdd);
      this.WinampTabPg.Controls.Add(this.listBoxSelectedWAPlugins);
      this.WinampTabPg.Controls.Add(this.listBoxFoundWAPlugins);
      this.WinampTabPg.Controls.Add(this.buttonWASearch);
      this.WinampTabPg.Controls.Add(this.buttonSelectWADir);
      this.WinampTabPg.Controls.Add(this.textBoxWAPluginDir);
      this.WinampTabPg.Controls.Add(this.label6);
      this.WinampTabPg.Location = new System.Drawing.Point(4, 22);
      this.WinampTabPg.Name = "WinampTabPg";
      this.WinampTabPg.Padding = new System.Windows.Forms.Padding(3);
      this.WinampTabPg.Size = new System.Drawing.Size(458, 305);
      this.WinampTabPg.TabIndex = 3;
      this.WinampTabPg.Text = "Winamp";
      this.WinampTabPg.UseVisualStyleBackColor = true;
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(248, 277);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(203, 13);
      this.label7.TabIndex = 29;
      this.label7.Text = "Double click to change plugin parameters";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(251, 53);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(184, 13);
      this.label8.TabIndex = 28;
      this.label8.Text = "Selected Plugins in Ascending Priority";
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(11, 53);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(87, 13);
      this.label9.TabIndex = 27;
      this.label9.Text = "Available Plugins";
      // 
      // buttonWARemove
      // 
      this.buttonWARemove.Location = new System.Drawing.Point(218, 169);
      this.buttonWARemove.Name = "buttonWARemove";
      this.buttonWARemove.Size = new System.Drawing.Size(28, 23);
      this.buttonWARemove.TabIndex = 26;
      this.buttonWARemove.Text = "<";
      this.buttonWARemove.UseVisualStyleBackColor = true;
      this.buttonWARemove.Click += new System.EventHandler(this.buttonWARemove_Click);
      // 
      // buttonWAAdd
      // 
      this.buttonWAAdd.Location = new System.Drawing.Point(218, 118);
      this.buttonWAAdd.Name = "buttonWAAdd";
      this.buttonWAAdd.Size = new System.Drawing.Size(28, 23);
      this.buttonWAAdd.TabIndex = 25;
      this.buttonWAAdd.Text = ">";
      this.buttonWAAdd.UseVisualStyleBackColor = true;
      this.buttonWAAdd.Click += new System.EventHandler(this.buttonWAAdd_Click);
      // 
      // listBoxSelectedWAPlugins
      // 
      this.listBoxSelectedWAPlugins.FormattingEnabled = true;
      this.listBoxSelectedWAPlugins.HorizontalScrollbar = true;
      this.listBoxSelectedWAPlugins.Location = new System.Drawing.Point(253, 72);
      this.listBoxSelectedWAPlugins.Name = "listBoxSelectedWAPlugins";
      this.listBoxSelectedWAPlugins.Size = new System.Drawing.Size(195, 199);
      this.listBoxSelectedWAPlugins.TabIndex = 24;
      this.listBoxSelectedWAPlugins.DoubleClick += new System.EventHandler(this.listBoxSelectedWAPlugins_DoubleClick);
      // 
      // listBoxFoundWAPlugins
      // 
      this.listBoxFoundWAPlugins.FormattingEnabled = true;
      this.listBoxFoundWAPlugins.HorizontalScrollbar = true;
      this.listBoxFoundWAPlugins.Location = new System.Drawing.Point(11, 72);
      this.listBoxFoundWAPlugins.Name = "listBoxFoundWAPlugins";
      this.listBoxFoundWAPlugins.Size = new System.Drawing.Size(195, 199);
      this.listBoxFoundWAPlugins.Sorted = true;
      this.listBoxFoundWAPlugins.TabIndex = 23;
      // 
      // buttonWASearch
      // 
      this.buttonWASearch.Location = new System.Drawing.Point(395, 15);
      this.buttonWASearch.Name = "buttonWASearch";
      this.buttonWASearch.Size = new System.Drawing.Size(57, 23);
      this.buttonWASearch.TabIndex = 22;
      this.buttonWASearch.Text = "Search";
      this.buttonWASearch.UseVisualStyleBackColor = true;
      this.buttonWASearch.Click += new System.EventHandler(this.buttonWASearch_Click);
      // 
      // buttonSelectWADir
      // 
      this.buttonSelectWADir.Location = new System.Drawing.Point(363, 15);
      this.buttonSelectWADir.Name = "buttonSelectWADir";
      this.buttonSelectWADir.Size = new System.Drawing.Size(25, 23);
      this.buttonSelectWADir.TabIndex = 21;
      this.buttonSelectWADir.Text = "...";
      this.buttonSelectWADir.UseVisualStyleBackColor = true;
      this.buttonSelectWADir.Click += new System.EventHandler(this.buttonSelectWADir_Click);
      // 
      // textBoxWAPluginDir
      // 
      this.textBoxWAPluginDir.Location = new System.Drawing.Point(74, 17);
      this.textBoxWAPluginDir.Name = "textBoxWAPluginDir";
      this.textBoxWAPluginDir.Size = new System.Drawing.Size(289, 20);
      this.textBoxWAPluginDir.TabIndex = 20;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(15, 19);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(55, 13);
      this.label6.TabIndex = 19;
      this.label6.Text = "Plugin Dir:";
      // 
      // btFileselect
      // 
      this.btFileselect.Location = new System.Drawing.Point(437, 339);
      this.btFileselect.Name = "btFileselect";
      this.btFileselect.Size = new System.Drawing.Size(29, 23);
      this.btFileselect.TabIndex = 10;
      this.btFileselect.Text = "....";
      this.btFileselect.UseVisualStyleBackColor = true;
      this.btFileselect.Click += new System.EventHandler(this.btFileselect_Click);
      // 
      // textBoxMusicFile
      // 
      this.textBoxMusicFile.BorderColor = System.Drawing.Color.Empty;
      this.textBoxMusicFile.Location = new System.Drawing.Point(68, 340);
      this.textBoxMusicFile.Name = "textBoxMusicFile";
      this.textBoxMusicFile.Size = new System.Drawing.Size(363, 20);
      this.textBoxMusicFile.TabIndex = 9;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(5, 344);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(57, 13);
      this.label1.TabIndex = 8;
      this.label1.Text = "Music File:";
      // 
      // btStop
      // 
      this.btStop.Enabled = false;
      this.btStop.Location = new System.Drawing.Point(168, 366);
      this.btStop.Name = "btStop";
      this.btStop.Size = new System.Drawing.Size(75, 23);
      this.btStop.TabIndex = 7;
      this.btStop.Text = "Stop";
      this.btStop.UseVisualStyleBackColor = true;
      this.btStop.Click += new System.EventHandler(this.btStop_Click);
      // 
      // btPlay
      // 
      this.btPlay.Location = new System.Drawing.Point(68, 366);
      this.btPlay.Name = "btPlay";
      this.btPlay.Size = new System.Drawing.Size(75, 23);
      this.btPlay.TabIndex = 6;
      this.btPlay.Text = "Play";
      this.btPlay.UseVisualStyleBackColor = true;
      this.btPlay.Click += new System.EventHandler(this.btPlay_Click);
      // 
      // MusicDSP
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.btFileselect);
      this.Controls.Add(this.MusicDSPTabCtl);
      this.Controls.Add(this.textBoxMusicFile);
      this.Controls.Add(this.btPlay);
      this.Controls.Add(this.btStop);
      this.Controls.Add(this.label1);
      this.Name = "MusicDSP";
      this.Size = new System.Drawing.Size(472, 400);
      this.Load += new System.EventHandler(this.MusicDSP_Load);
      this.DSPTabPg.ResumeLayout(false);
      this.DSPTabPg.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarGain)).EndInit();
      this.groupBoxGain.ResumeLayout(false);
      this.groupBoxGain.PerformLayout();
      this.groupBoxCompressor.ResumeLayout(false);
      this.groupBoxCompressor.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarCompressor)).EndInit();
      this.MusicDSPTabCtl.ResumeLayout(false);
      this.VSTTagPg.ResumeLayout(false);
      this.VSTTagPg.PerformLayout();
      this.WinampTabPg.ResumeLayout(false);
      this.WinampTabPg.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPTabPage DSPTabPg;
    private MediaPortal.UserInterface.Controls.MPTabControl MusicDSPTabCtl;
    private MediaPortal.UserInterface.Controls.MPTabPage VSTTagPg;
    private MediaPortal.UserInterface.Controls.MPTabPage WinampTabPg;
    private MediaPortal.UserInterface.Controls.MPButton btFileselect;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxMusicFile;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPButton btStop;
    private MediaPortal.UserInterface.Controls.MPButton btPlay;
    private MediaPortal.UserInterface.Controls.MPLabel label17;
    private MediaPortal.UserInterface.Controls.MPButton buttonSetGain;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxGainDBValue;
    private MediaPortal.UserInterface.Controls.MPLabel label23;
    private MediaPortal.UserInterface.Controls.MPLabel label14;
    private System.Windows.Forms.TrackBar trackBarGain;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxGain;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxDynamicAmplification;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxDAmp;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPButton buttonVSTSearch;
    private MediaPortal.UserInterface.Controls.MPButton buttonSelectVSTDir;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxVSTPluginDir;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPButton buttonVSTRemove;
    private MediaPortal.UserInterface.Controls.MPButton buttonVSTAdd;
    private System.Windows.Forms.ListBox listBoxSelectedVSTPlugins;
    private System.Windows.Forms.ListBox listBoxFoundVSTPlugins;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPLabel label22;
    private MediaPortal.UserInterface.Controls.MPLabel label13;
    private MediaPortal.UserInterface.Controls.MPLabel label11;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxCompressor;
    private MediaPortal.UserInterface.Controls.MPLabel labelCompThreshold;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxCompressor;
    private System.Windows.Forms.TrackBar trackBarCompressor;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Button buttonWARemove;
    private System.Windows.Forms.Button buttonWAAdd;
    private System.Windows.Forms.ListBox listBoxSelectedWAPlugins;
    private System.Windows.Forms.ListBox listBoxFoundWAPlugins;
    private System.Windows.Forms.Button buttonWASearch;
    private System.Windows.Forms.Button buttonSelectWADir;
    private System.Windows.Forms.TextBox textBoxWAPluginDir;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.ToolTip toolTip;

  }
}
