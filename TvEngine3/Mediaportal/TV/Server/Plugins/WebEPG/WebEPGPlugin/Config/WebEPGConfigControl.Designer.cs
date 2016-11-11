using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.Plugins.WebEPGImport.Config
{
  partial class WebEPGSetup
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.bsMergedChannel = new System.Windows.Forms.BindingSource(this.components);
      this.gbMapping = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.label3 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.cbCountry = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.bAutoMap = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.gbGlobal = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.checkBoxDeleteBeforeImport = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.buttonBrowse = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.textBoxFolder = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.DestinationComboBox = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.mpLabel1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.nMaxGrab = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.lGrabDay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.bSave = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.tabMain = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabControl();
      this.tabGeneral = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.groupBox1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelStatus = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonManualImport = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelPrograms = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelChannels = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelLastImport = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.label5 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.label10 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.label11 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.label12 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.tabTVMappings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.TvMappings = new Mediaportal.TV.Server.Plugins.WebEPGImport.Config.WebEPGMappingControl();
      this.tabRadioMappings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.RadioMappings = new Mediaportal.TV.Server.Plugins.WebEPGImport.Config.WebEPGMappingControl();
      this.tabSchedule = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.groupBox3 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.mpLabel2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.ScheduleGrabCheckBox = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.panel1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPPanel();
      this.SundayCheckBox = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.SaturdayCheckBox = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.FridayCheckBox = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.ThursdayCheckBox = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.WednesdayCheckBox = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.TuesdayCheckBox = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.MondayCheckBox = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.grabTimeTextBox = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPMaskedTextBox();
      this.StatusTimer = new System.Windows.Forms.Timer(this.components);
      this.folderBrowserDialogTVGuide = new System.Windows.Forms.FolderBrowserDialog();
      ((System.ComponentModel.ISupportInitialize)(this.bsMergedChannel)).BeginInit();
      this.gbMapping.SuspendLayout();
      this.gbGlobal.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nMaxGrab)).BeginInit();
      this.tabMain.SuspendLayout();
      this.tabGeneral.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tabTVMappings.SuspendLayout();
      this.tabRadioMappings.SuspendLayout();
      this.tabSchedule.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // bsMergedChannel
      // 
      this.bsMergedChannel.DataSource = typeof(WebEPG.config.WebEPG.MergedChannel);
      // 
      // gbMapping
      // 
      this.gbMapping.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbMapping.Controls.Add(this.label3);
      this.gbMapping.Controls.Add(this.cbCountry);
      this.gbMapping.Controls.Add(this.bAutoMap);
      this.gbMapping.Location = new System.Drawing.Point(3, 133);
      this.gbMapping.Name = "gbMapping";
      this.gbMapping.Size = new System.Drawing.Size(466, 58);
      this.gbMapping.TabIndex = 25;
      this.gbMapping.TabStop = false;
      this.gbMapping.Text = "Auto Mapping";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(6, 26);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(43, 13);
      this.label3.TabIndex = 22;
      this.label3.Text = "Country";
      // 
      // cbCountry
      // 
      this.cbCountry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbCountry.FormattingEnabled = true;
      this.cbCountry.Location = new System.Drawing.Point(84, 23);
      this.cbCountry.Name = "cbCountry";
      this.cbCountry.Size = new System.Drawing.Size(298, 21);
      this.cbCountry.Sorted = true;
      this.cbCountry.TabIndex = 21;
      // 
      // bAutoMap
      // 
      this.bAutoMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bAutoMap.Location = new System.Drawing.Point(389, 21);
      this.bAutoMap.Name = "bAutoMap";
      this.bAutoMap.Size = new System.Drawing.Size(72, 23);
      this.bAutoMap.TabIndex = 19;
      this.bAutoMap.Text = "Auto Map";
      this.bAutoMap.UseVisualStyleBackColor = true;
      this.bAutoMap.Click += new System.EventHandler(this.bAutoMap_Click);
      // 
      // gbGlobal
      // 
      this.gbGlobal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbGlobal.Controls.Add(this.checkBoxDeleteBeforeImport);
      this.gbGlobal.Controls.Add(this.buttonBrowse);
      this.gbGlobal.Controls.Add(this.textBoxFolder);
      this.gbGlobal.Controls.Add(this.DestinationComboBox);
      this.gbGlobal.Controls.Add(this.mpLabel1);
      this.gbGlobal.Controls.Add(this.nMaxGrab);
      this.gbGlobal.Controls.Add(this.lGrabDay);
      this.gbGlobal.Location = new System.Drawing.Point(3, 0);
      this.gbGlobal.Name = "gbGlobal";
      this.gbGlobal.Size = new System.Drawing.Size(466, 127);
      this.gbGlobal.TabIndex = 22;
      this.gbGlobal.TabStop = false;
      this.gbGlobal.Text = "Global Settings";
      // 
      // checkBoxDeleteBeforeImport
      // 
      this.checkBoxDeleteBeforeImport.AutoSize = true;
      this.checkBoxDeleteBeforeImport.Location = new System.Drawing.Point(9, 102);
      this.checkBoxDeleteBeforeImport.Name = "checkBoxDeleteBeforeImport";
      this.checkBoxDeleteBeforeImport.Size = new System.Drawing.Size(268, 17);
      this.checkBoxDeleteBeforeImport.TabIndex = 35;
      this.checkBoxDeleteBeforeImport.Text = "Delete old programs before import (Recommended!)";
      this.checkBoxDeleteBeforeImport.UseVisualStyleBackColor = true;
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBrowse.Location = new System.Drawing.Point(437, 74);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(23, 23);
      this.buttonBrowse.TabIndex = 23;
      this.buttonBrowse.Text = "...";
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
      // 
      // textBoxFolder
      // 
      this.textBoxFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxFolder.Location = new System.Drawing.Point(106, 76);
      this.textBoxFolder.Name = "textBoxFolder";
      this.textBoxFolder.Size = new System.Drawing.Size(325, 20);
      this.textBoxFolder.TabIndex = 22;
      // 
      // DestinationComboBox
      // 
      this.DestinationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.DestinationComboBox.FormattingEnabled = true;
      this.DestinationComboBox.Items.AddRange(new object[] {
            "TV Server database",
            "tvguide.xml file configured in XmlTv Importer plugin",
            "tvguide.xml in the following folder:"});
      this.DestinationComboBox.Location = new System.Drawing.Point(106, 49);
      this.DestinationComboBox.Name = "DestinationComboBox";
      this.DestinationComboBox.Size = new System.Drawing.Size(354, 21);
      this.DestinationComboBox.TabIndex = 15;
      this.DestinationComboBox.SelectedIndexChanged += new System.EventHandler(this.DestinationComboBox_SelectedIndexChanged);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(6, 52);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(90, 13);
      this.mpLabel1.TabIndex = 14;
      this.mpLabel1.Text = "Write programs to";
      // 
      // nMaxGrab
      // 
      this.nMaxGrab.Location = new System.Drawing.Point(106, 22);
      this.nMaxGrab.Maximum = new decimal(new int[] {
            14,
            0,
            0,
            0});
      this.nMaxGrab.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.nMaxGrab.Name = "nMaxGrab";
      this.nMaxGrab.Size = new System.Drawing.Size(67, 20);
      this.nMaxGrab.TabIndex = 13;
      this.nMaxGrab.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
      // 
      // lGrabDay
      // 
      this.lGrabDay.Location = new System.Drawing.Point(6, 24);
      this.lGrabDay.Name = "lGrabDay";
      this.lGrabDay.Size = new System.Drawing.Size(72, 16);
      this.lGrabDay.TabIndex = 9;
      this.lGrabDay.Text = "Grab Days";
      // 
      // bSave
      // 
      this.bSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bSave.Location = new System.Drawing.Point(394, 364);
      this.bSave.Name = "bSave";
      this.bSave.Size = new System.Drawing.Size(72, 24);
      this.bSave.TabIndex = 23;
      this.bSave.Text = "Save";
      this.bSave.UseVisualStyleBackColor = true;
      this.bSave.Click += new System.EventHandler(this.bSave_Click);
      // 
      // tabMain
      // 
      this.tabMain.Controls.Add(this.tabGeneral);
      this.tabMain.Controls.Add(this.tabTVMappings);
      this.tabMain.Controls.Add(this.tabRadioMappings);
      this.tabMain.Controls.Add(this.tabSchedule);
      this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabMain.Location = new System.Drawing.Point(0, 0);
      this.tabMain.Name = "tabMain";
      this.tabMain.SelectedIndex = 0;
      this.tabMain.Size = new System.Drawing.Size(480, 420);
      this.tabMain.TabIndex = 26;
      // 
      // tabGeneral
      // 
      this.tabGeneral.Controls.Add(this.groupBox1);
      this.tabGeneral.Controls.Add(this.bSave);
      this.tabGeneral.Controls.Add(this.gbMapping);
      this.tabGeneral.Controls.Add(this.gbGlobal);
      this.tabGeneral.Location = new System.Drawing.Point(4, 22);
      this.tabGeneral.Name = "tabGeneral";
      this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
      this.tabGeneral.Size = new System.Drawing.Size(472, 394);
      this.tabGeneral.TabIndex = 1;
      this.tabGeneral.Text = "General";
      this.tabGeneral.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.labelStatus);
      this.groupBox1.Controls.Add(this.buttonManualImport);
      this.groupBox1.Controls.Add(this.labelPrograms);
      this.groupBox1.Controls.Add(this.labelChannels);
      this.groupBox1.Controls.Add(this.labelLastImport);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.label10);
      this.groupBox1.Controls.Add(this.label11);
      this.groupBox1.Controls.Add(this.label12);
      this.groupBox1.Location = new System.Drawing.Point(3, 197);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(466, 119);
      this.groupBox1.TabIndex = 31;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Grabber status report:";
      // 
      // labelStatus
      // 
      this.labelStatus.AutoSize = true;
      this.labelStatus.Location = new System.Drawing.Point(163, 97);
      this.labelStatus.Name = "labelStatus";
      this.labelStatus.Size = new System.Drawing.Size(0, 13);
      this.labelStatus.TabIndex = 24;
      // 
      // buttonManualImport
      // 
      this.buttonManualImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonManualImport.Location = new System.Drawing.Point(388, 23);
      this.buttonManualImport.Name = "buttonManualImport";
      this.buttonManualImport.Size = new System.Drawing.Size(72, 23);
      this.buttonManualImport.TabIndex = 26;
      this.buttonManualImport.Text = "Grab now!";
      this.buttonManualImport.UseVisualStyleBackColor = true;
      this.buttonManualImport.Click += new System.EventHandler(this.buttonManualImport_Click);
      // 
      // labelPrograms
      // 
      this.labelPrograms.AutoSize = true;
      this.labelPrograms.Location = new System.Drawing.Point(163, 75);
      this.labelPrograms.Name = "labelPrograms";
      this.labelPrograms.Size = new System.Drawing.Size(0, 13);
      this.labelPrograms.TabIndex = 23;
      // 
      // labelChannels
      // 
      this.labelChannels.AutoSize = true;
      this.labelChannels.Location = new System.Drawing.Point(163, 52);
      this.labelChannels.Name = "labelChannels";
      this.labelChannels.Size = new System.Drawing.Size(0, 13);
      this.labelChannels.TabIndex = 22;
      // 
      // labelLastImport
      // 
      this.labelLastImport.AutoSize = true;
      this.labelLastImport.Location = new System.Drawing.Point(163, 28);
      this.labelLastImport.Name = "labelLastImport";
      this.labelLastImport.Size = new System.Drawing.Size(0, 13);
      this.labelLastImport.TabIndex = 21;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(17, 97);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(64, 13);
      this.label5.TabIndex = 20;
      this.label5.Text = "Grab status:";
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(17, 75);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(123, 13);
      this.label10.TabIndex = 19;
      this.label10.Text = "Total programs imported:";
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(17, 52);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(123, 13);
      this.label11.TabIndex = 18;
      this.label11.Text = "Total channels imported:";
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(17, 28);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(84, 13);
      this.label12.TabIndex = 17;
      this.label12.Text = "Last grab run at:";
      // 
      // tabTVMappings
      // 
      this.tabTVMappings.Controls.Add(this.TvMappings);
      this.tabTVMappings.Location = new System.Drawing.Point(4, 22);
      this.tabTVMappings.Name = "tabTVMappings";
      this.tabTVMappings.Size = new System.Drawing.Size(449, 429);
      this.tabTVMappings.TabIndex = 2;
      this.tabTVMappings.Text = "TV Mappings";
      this.tabTVMappings.UseVisualStyleBackColor = true;
      // 
      // TvMappings
      // 
      this.TvMappings.BackColor = System.Drawing.Color.Transparent;
      this.TvMappings.ChannelMapping = null;
      this.TvMappings.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TvMappings.HChannelConfigInfo = null;
      this.TvMappings.IsTvMapping = true;
      this.TvMappings.Location = new System.Drawing.Point(0, 0);
      this.TvMappings.Name = "TvMappings";
      this.TvMappings.Size = new System.Drawing.Size(449, 429);
      this.TvMappings.TabIndex = 0;
      this.TvMappings.SelectGrabberClick += new System.EventHandler(this.Mappings_SelectGrabberClick);
      this.TvMappings.AutoMapChannels += new System.EventHandler(this.Mappings_AutoMapChannels);
      // 
      // tabRadioMappings
      // 
      this.tabRadioMappings.Controls.Add(this.RadioMappings);
      this.tabRadioMappings.Location = new System.Drawing.Point(4, 22);
      this.tabRadioMappings.Name = "tabRadioMappings";
      this.tabRadioMappings.Size = new System.Drawing.Size(472, 394);
      this.tabRadioMappings.TabIndex = 3;
      this.tabRadioMappings.Text = "Radio Mappings";
      this.tabRadioMappings.UseVisualStyleBackColor = true;
      // 
      // RadioMappings
      // 
      this.RadioMappings.BackColor = System.Drawing.Color.Transparent;
      this.RadioMappings.ChannelMapping = null;
      this.RadioMappings.Dock = System.Windows.Forms.DockStyle.Fill;
      this.RadioMappings.HChannelConfigInfo = null;
      this.RadioMappings.IsTvMapping = false;
      this.RadioMappings.Location = new System.Drawing.Point(0, 0);
      this.RadioMappings.Name = "RadioMappings";
      this.RadioMappings.Size = new System.Drawing.Size(472, 394);
      this.RadioMappings.TabIndex = 0;
      this.RadioMappings.SelectGrabberClick += new System.EventHandler(this.Mappings_SelectGrabberClick);
      this.RadioMappings.AutoMapChannels += new System.EventHandler(this.Mappings_AutoMapChannels);
      // 
      // tabSchedule
      // 
      this.tabSchedule.Controls.Add(this.groupBox3);
      this.tabSchedule.Location = new System.Drawing.Point(4, 22);
      this.tabSchedule.Name = "tabSchedule";
      this.tabSchedule.Size = new System.Drawing.Size(449, 429);
      this.tabSchedule.TabIndex = 4;
      this.tabSchedule.Text = "Schedule";
      this.tabSchedule.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.mpLabel2);
      this.groupBox3.Controls.Add(this.ScheduleGrabCheckBox);
      this.groupBox3.Controls.Add(this.panel1);
      this.groupBox3.Controls.Add(this.grabTimeTextBox);
      this.groupBox3.Location = new System.Drawing.Point(3, 3);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(443, 240);
      this.groupBox3.TabIndex = 1;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Grabber scheduling";
      // 
      // mpLabel2
      // 
      this.mpLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpLabel2.Location = new System.Drawing.Point(6, 192);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(431, 45);
      this.mpLabel2.TabIndex = 6;
      this.mpLabel2.Text = "Note: If you have configured the PowerScheduler plugin to wake up for EPG grabbin" +
          "g, the above schedule will be followed in addition to that of PowerScheduler";
      this.mpLabel2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // ScheduleGrabCheckBox
      // 
      this.ScheduleGrabCheckBox.AutoSize = true;
      this.ScheduleGrabCheckBox.Location = new System.Drawing.Point(88, 61);
      this.ScheduleGrabCheckBox.Name = "ScheduleGrabCheckBox";
      this.ScheduleGrabCheckBox.Size = new System.Drawing.Size(232, 17);
      this.ScheduleGrabCheckBox.TabIndex = 5;
      this.ScheduleGrabCheckBox.Text = "Grab EPG on the following days at this time:";
      this.ScheduleGrabCheckBox.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.SundayCheckBox);
      this.panel1.Controls.Add(this.SaturdayCheckBox);
      this.panel1.Controls.Add(this.FridayCheckBox);
      this.panel1.Controls.Add(this.ThursdayCheckBox);
      this.panel1.Controls.Add(this.WednesdayCheckBox);
      this.panel1.Controls.Add(this.TuesdayCheckBox);
      this.panel1.Controls.Add(this.MondayCheckBox);
      this.panel1.Location = new System.Drawing.Point(78, 84);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(256, 83);
      this.panel1.TabIndex = 3;
      // 
      // SundayCheckBox
      // 
      this.SundayCheckBox.AutoSize = true;
      this.SundayCheckBox.Location = new System.Drawing.Point(10, 59);
      this.SundayCheckBox.Name = "SundayCheckBox";
      this.SundayCheckBox.Size = new System.Drawing.Size(62, 17);
      this.SundayCheckBox.TabIndex = 6;
      this.SundayCheckBox.Text = "Sunday";
      this.SundayCheckBox.UseVisualStyleBackColor = true;
      // 
      // SaturdayCheckBox
      // 
      this.SaturdayCheckBox.AutoSize = true;
      this.SaturdayCheckBox.Location = new System.Drawing.Point(170, 37);
      this.SaturdayCheckBox.Name = "SaturdayCheckBox";
      this.SaturdayCheckBox.Size = new System.Drawing.Size(68, 17);
      this.SaturdayCheckBox.TabIndex = 5;
      this.SaturdayCheckBox.Text = "Saturday";
      this.SaturdayCheckBox.UseVisualStyleBackColor = true;
      // 
      // FridayCheckBox
      // 
      this.FridayCheckBox.AutoSize = true;
      this.FridayCheckBox.Location = new System.Drawing.Point(96, 37);
      this.FridayCheckBox.Name = "FridayCheckBox";
      this.FridayCheckBox.Size = new System.Drawing.Size(54, 17);
      this.FridayCheckBox.TabIndex = 4;
      this.FridayCheckBox.Text = "Friday";
      this.FridayCheckBox.UseVisualStyleBackColor = true;
      // 
      // ThursdayCheckBox
      // 
      this.ThursdayCheckBox.AutoSize = true;
      this.ThursdayCheckBox.Location = new System.Drawing.Point(10, 37);
      this.ThursdayCheckBox.Name = "ThursdayCheckBox";
      this.ThursdayCheckBox.Size = new System.Drawing.Size(70, 17);
      this.ThursdayCheckBox.TabIndex = 3;
      this.ThursdayCheckBox.Text = "Thursday";
      this.ThursdayCheckBox.UseVisualStyleBackColor = true;
      // 
      // WednesdayCheckBox
      // 
      this.WednesdayCheckBox.AutoSize = true;
      this.WednesdayCheckBox.Location = new System.Drawing.Point(170, 14);
      this.WednesdayCheckBox.Name = "WednesdayCheckBox";
      this.WednesdayCheckBox.Size = new System.Drawing.Size(83, 17);
      this.WednesdayCheckBox.TabIndex = 2;
      this.WednesdayCheckBox.Text = "Wednesday";
      this.WednesdayCheckBox.UseVisualStyleBackColor = true;
      // 
      // TuesdayCheckBox
      // 
      this.TuesdayCheckBox.AutoSize = true;
      this.TuesdayCheckBox.Location = new System.Drawing.Point(96, 14);
      this.TuesdayCheckBox.Name = "TuesdayCheckBox";
      this.TuesdayCheckBox.Size = new System.Drawing.Size(67, 17);
      this.TuesdayCheckBox.TabIndex = 1;
      this.TuesdayCheckBox.Text = "Tuesday";
      this.TuesdayCheckBox.UseVisualStyleBackColor = true;
      // 
      // MondayCheckBox
      // 
      this.MondayCheckBox.AutoSize = true;
      this.MondayCheckBox.Location = new System.Drawing.Point(10, 14);
      this.MondayCheckBox.Name = "MondayCheckBox";
      this.MondayCheckBox.Size = new System.Drawing.Size(64, 17);
      this.MondayCheckBox.TabIndex = 0;
      this.MondayCheckBox.Text = "Monday";
      this.MondayCheckBox.UseVisualStyleBackColor = true;
      // 
      // grabTimeTextBox
      // 
      this.grabTimeTextBox.Location = new System.Drawing.Point(324, 60);
      this.grabTimeTextBox.Mask = "00:00";
      this.grabTimeTextBox.Name = "grabTimeTextBox";
      this.grabTimeTextBox.Size = new System.Drawing.Size(36, 20);
      this.grabTimeTextBox.TabIndex = 2;
      this.grabTimeTextBox.Text = "0400";
      this.grabTimeTextBox.ValidatingType = typeof(System.DateTime);
      // 
      // StatusTimer
      // 
      this.StatusTimer.Enabled = true;
      this.StatusTimer.Interval = 10000;
      this.StatusTimer.Tick += new System.EventHandler(this.StatusTimer_Tick);
      // 
      // WebEPGSetup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabMain);
      this.Name = "WebEPGSetup";
      this.Size = new System.Drawing.Size(480, 420);
      ((System.ComponentModel.ISupportInitialize)(this.bsMergedChannel)).EndInit();
      this.gbMapping.ResumeLayout(false);
      this.gbMapping.PerformLayout();
      this.gbGlobal.ResumeLayout(false);
      this.gbGlobal.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nMaxGrab)).EndInit();
      this.tabMain.ResumeLayout(false);
      this.tabGeneral.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tabTVMappings.ResumeLayout(false);
      this.tabRadioMappings.ResumeLayout(false);
      this.tabSchedule.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MPGroupBox gbMapping;
    private MPLabel label3;
    private MPComboBox cbCountry;
    private MPButton bAutoMap;
    private MPGroupBox gbGlobal;
    private MPNumericUpDown nMaxGrab;
    private MPLabel lGrabDay;
    private MPButton bSave;
    private MPTabControl tabMain;
    private MPTabPage tabGeneral;
    private MPButton buttonManualImport;
    private MPGroupBox groupBox1;
    private MPLabel labelStatus;
    private MPLabel labelPrograms;
    private MPLabel labelChannels;
    private MPLabel labelLastImport;
    private MPLabel label5;
    private MPLabel label10;
    private MPLabel label11;
    private MPLabel label12;
    private System.Windows.Forms.Timer StatusTimer;
    private MPComboBox DestinationComboBox;
    private MPLabel mpLabel1;
    private MPButton buttonBrowse;
    private MPTextBox textBoxFolder;
    private MPCheckBox checkBoxDeleteBeforeImport;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogTVGuide;
    private System.Windows.Forms.BindingSource bsMergedChannel;
    private MPTabPage tabTVMappings;
    private MPTabPage tabRadioMappings;
    private MPTabPage tabSchedule;
    private WebEPGMappingControl RadioMappings;
    private WebEPGMappingControl TvMappings;
    private MPGroupBox groupBox3;
    private MPPanel panel1;
    private MPCheckBox SundayCheckBox;
    private MPCheckBox SaturdayCheckBox;
    private MPCheckBox FridayCheckBox;
    private MPCheckBox ThursdayCheckBox;
    private MPCheckBox WednesdayCheckBox;
    private MPCheckBox TuesdayCheckBox;
    private MPCheckBox MondayCheckBox;
    private MPMaskedTextBox grabTimeTextBox;
    private MPCheckBox ScheduleGrabCheckBox;
    private MPLabel mpLabel2;
  }
}
