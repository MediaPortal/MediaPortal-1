namespace SetupTv.Sections
{
  partial class WebEPGConfigControl
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
      this.gbChannelDetails = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.tcMappingDetails = new System.Windows.Forms.TabControl();
      this.tpSingle = new System.Windows.Forms.TabPage();
      this.gbGrabber = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.tbGrabDays = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lGuideDays = new MediaPortal.UserInterface.Controls.MPLabel();
      this.bGrabber = new MediaPortal.UserInterface.Controls.MPButton();
      this.Grabber = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbGrabSite = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.bChannelID = new MediaPortal.UserInterface.Controls.MPButton();
      this.l_cID = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbChannelName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tpMultiple = new System.Windows.Forms.TabPage();
      this.bMergedEdit = new System.Windows.Forms.Button();
      this.bMergedRemove = new System.Windows.Forms.Button();
      this.bMergedAdd = new System.Windows.Forms.Button();
      this.lvMerged = new System.Windows.Forms.ListView();
      this.gbMapping = new System.Windows.Forms.GroupBox();
      this.label3 = new System.Windows.Forms.Label();
      this.cbCountry = new System.Windows.Forms.ComboBox();
      this.bAutoMap = new System.Windows.Forms.Button();
      this.bImport = new MediaPortal.UserInterface.Controls.MPButton();
      this.gbGlobal = new System.Windows.Forms.GroupBox();
      this.checkBoxDeleteOnlyOverlapping = new System.Windows.Forms.CheckBox();
      this.checkBoxDeleteBeforeImport = new System.Windows.Forms.CheckBox();
      this.buttonBrowse = new System.Windows.Forms.Button();
      this.textBoxFolder = new System.Windows.Forms.TextBox();
      this.DestinationComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.nMaxGrab = new System.Windows.Forms.NumericUpDown();
      this.lGrabDay = new MediaPortal.UserInterface.Controls.MPLabel();
      this.gbChannels = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.LoadRadioCheckBox = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.GroupComboBox = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.mtbNewChannel = new System.Windows.Forms.MaskedTextBox();
      this.lvMapping = new System.Windows.Forms.ListView();
      this.bClearMapping = new System.Windows.Forms.Button();
      this.lCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbCount = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.bRemove = new MediaPortal.UserInterface.Controls.MPButton();
      this.bAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.bSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.tabMain = new System.Windows.Forms.TabControl();
      this.tabSettings = new System.Windows.Forms.TabPage();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.labelStatus = new System.Windows.Forms.Label();
      this.buttonManualImport = new System.Windows.Forms.Button();
      this.labelPrograms = new System.Windows.Forms.Label();
      this.labelChannels = new System.Windows.Forms.Label();
      this.labelLastImport = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label10 = new System.Windows.Forms.Label();
      this.label11 = new System.Windows.Forms.Label();
      this.label12 = new System.Windows.Forms.Label();
      this.tabMappings = new System.Windows.Forms.TabPage();
      this.StatusTimer = new System.Windows.Forms.Timer(this.components);
      this.folderBrowserDialogTVGuide = new System.Windows.Forms.FolderBrowserDialog();
      this.gbChannelDetails.SuspendLayout();
      this.tcMappingDetails.SuspendLayout();
      this.tpSingle.SuspendLayout();
      this.gbGrabber.SuspendLayout();
      this.tpMultiple.SuspendLayout();
      this.gbMapping.SuspendLayout();
      this.gbGlobal.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nMaxGrab)).BeginInit();
      this.gbChannels.SuspendLayout();
      this.tabMain.SuspendLayout();
      this.tabSettings.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tabMappings.SuspendLayout();
      this.SuspendLayout();
      // 
      // gbChannelDetails
      // 
      this.gbChannelDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbChannelDetails.Controls.Add(this.tcMappingDetails);
      this.gbChannelDetails.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbChannelDetails.Location = new System.Drawing.Point(3, 250);
      this.gbChannelDetails.Name = "gbChannelDetails";
      this.gbChannelDetails.Size = new System.Drawing.Size(436, 167);
      this.gbChannelDetails.TabIndex = 21;
      this.gbChannelDetails.TabStop = false;
      this.gbChannelDetails.Text = "Mapping Details";
      // 
      // tcMappingDetails
      // 
      this.tcMappingDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tcMappingDetails.Controls.Add(this.tpSingle);
      this.tcMappingDetails.Controls.Add(this.tpMultiple);
      this.tcMappingDetails.Location = new System.Drawing.Point(6, 19);
      this.tcMappingDetails.Name = "tcMappingDetails";
      this.tcMappingDetails.SelectedIndex = 0;
      this.tcMappingDetails.Size = new System.Drawing.Size(424, 142);
      this.tcMappingDetails.TabIndex = 16;
      this.tcMappingDetails.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tcMappingDetails_Selecting);
      // 
      // tpSingle
      // 
      this.tpSingle.Controls.Add(this.gbGrabber);
      this.tpSingle.Controls.Add(this.bChannelID);
      this.tpSingle.Controls.Add(this.l_cID);
      this.tpSingle.Controls.Add(this.tbChannelName);
      this.tpSingle.Location = new System.Drawing.Point(4, 22);
      this.tpSingle.Name = "tpSingle";
      this.tpSingle.Padding = new System.Windows.Forms.Padding(3);
      this.tpSingle.Size = new System.Drawing.Size(416, 116);
      this.tpSingle.TabIndex = 0;
      this.tpSingle.Text = "Single";
      this.tpSingle.UseVisualStyleBackColor = true;
      // 
      // gbGrabber
      // 
      this.gbGrabber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbGrabber.Controls.Add(this.tbGrabDays);
      this.gbGrabber.Controls.Add(this.lGuideDays);
      this.gbGrabber.Controls.Add(this.bGrabber);
      this.gbGrabber.Controls.Add(this.Grabber);
      this.gbGrabber.Controls.Add(this.tbGrabSite);
      this.gbGrabber.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbGrabber.Location = new System.Drawing.Point(3, 38);
      this.gbGrabber.Name = "gbGrabber";
      this.gbGrabber.Size = new System.Drawing.Size(410, 65);
      this.gbGrabber.TabIndex = 15;
      this.gbGrabber.TabStop = false;
      this.gbGrabber.Text = "Grabber Details";
      // 
      // tbGrabDays
      // 
      this.tbGrabDays.Location = new System.Drawing.Point(79, 39);
      this.tbGrabDays.Name = "tbGrabDays";
      this.tbGrabDays.ReadOnly = true;
      this.tbGrabDays.Size = new System.Drawing.Size(116, 20);
      this.tbGrabDays.TabIndex = 7;
      // 
      // lGuideDays
      // 
      this.lGuideDays.Location = new System.Drawing.Point(3, 42);
      this.lGuideDays.Name = "lGuideDays";
      this.lGuideDays.Size = new System.Drawing.Size(70, 17);
      this.lGuideDays.TabIndex = 8;
      this.lGuideDays.Text = "Guide Days";
      // 
      // bGrabber
      // 
      this.bGrabber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bGrabber.Location = new System.Drawing.Point(374, 12);
      this.bGrabber.Name = "bGrabber";
      this.bGrabber.Size = new System.Drawing.Size(30, 20);
      this.bGrabber.TabIndex = 6;
      this.bGrabber.Text = "...";
      this.bGrabber.UseVisualStyleBackColor = true;
      this.bGrabber.Visible = false;
      // 
      // Grabber
      // 
      this.Grabber.Location = new System.Drawing.Point(3, 16);
      this.Grabber.Name = "Grabber";
      this.Grabber.Size = new System.Drawing.Size(56, 17);
      this.Grabber.TabIndex = 1;
      this.Grabber.Text = "Site";
      // 
      // tbGrabSite
      // 
      this.tbGrabSite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbGrabSite.Location = new System.Drawing.Point(79, 13);
      this.tbGrabSite.Name = "tbGrabSite";
      this.tbGrabSite.ReadOnly = true;
      this.tbGrabSite.Size = new System.Drawing.Size(289, 20);
      this.tbGrabSite.TabIndex = 0;
      // 
      // bChannelID
      // 
      this.bChannelID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bChannelID.Location = new System.Drawing.Point(377, 12);
      this.bChannelID.Name = "bChannelID";
      this.bChannelID.Size = new System.Drawing.Size(30, 20);
      this.bChannelID.TabIndex = 9;
      this.bChannelID.Text = "...";
      this.bChannelID.UseVisualStyleBackColor = true;
      this.bChannelID.Click += new System.EventHandler(this.bChannelID_Click);
      // 
      // l_cID
      // 
      this.l_cID.Location = new System.Drawing.Point(6, 15);
      this.l_cID.Name = "l_cID";
      this.l_cID.Size = new System.Drawing.Size(56, 17);
      this.l_cID.TabIndex = 8;
      this.l_cID.Text = "Channel";
      // 
      // tbChannelName
      // 
      this.tbChannelName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbChannelName.Location = new System.Drawing.Point(82, 12);
      this.tbChannelName.Name = "tbChannelName";
      this.tbChannelName.ReadOnly = true;
      this.tbChannelName.Size = new System.Drawing.Size(289, 20);
      this.tbChannelName.TabIndex = 7;
      // 
      // tpMultiple
      // 
      this.tpMultiple.Controls.Add(this.bMergedEdit);
      this.tpMultiple.Controls.Add(this.bMergedRemove);
      this.tpMultiple.Controls.Add(this.bMergedAdd);
      this.tpMultiple.Controls.Add(this.lvMerged);
      this.tpMultiple.Location = new System.Drawing.Point(4, 22);
      this.tpMultiple.Name = "tpMultiple";
      this.tpMultiple.Padding = new System.Windows.Forms.Padding(3);
      this.tpMultiple.Size = new System.Drawing.Size(416, 116);
      this.tpMultiple.TabIndex = 1;
      this.tpMultiple.Text = "Multiple (Merged)";
      this.tpMultiple.UseVisualStyleBackColor = true;
      // 
      // bMergedEdit
      // 
      this.bMergedEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bMergedEdit.Location = new System.Drawing.Point(254, 87);
      this.bMergedEdit.Name = "bMergedEdit";
      this.bMergedEdit.Size = new System.Drawing.Size(75, 23);
      this.bMergedEdit.TabIndex = 3;
      this.bMergedEdit.Text = "Edit";
      this.bMergedEdit.UseVisualStyleBackColor = true;
      this.bMergedEdit.Click += new System.EventHandler(this.bMergedEdit_Click);
      // 
      // bMergedRemove
      // 
      this.bMergedRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bMergedRemove.Location = new System.Drawing.Point(335, 87);
      this.bMergedRemove.Name = "bMergedRemove";
      this.bMergedRemove.Size = new System.Drawing.Size(75, 23);
      this.bMergedRemove.TabIndex = 2;
      this.bMergedRemove.Text = "Remove";
      this.bMergedRemove.UseVisualStyleBackColor = true;
      this.bMergedRemove.Click += new System.EventHandler(this.bMergedRemove_Click);
      // 
      // bMergedAdd
      // 
      this.bMergedAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bMergedAdd.Location = new System.Drawing.Point(173, 87);
      this.bMergedAdd.Name = "bMergedAdd";
      this.bMergedAdd.Size = new System.Drawing.Size(75, 23);
      this.bMergedAdd.TabIndex = 1;
      this.bMergedAdd.Text = "Add";
      this.bMergedAdd.UseVisualStyleBackColor = true;
      this.bMergedAdd.Click += new System.EventHandler(this.bMergedAdd_Click);
      // 
      // lvMerged
      // 
      this.lvMerged.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lvMerged.FullRowSelect = true;
      this.lvMerged.HideSelection = false;
      this.lvMerged.Location = new System.Drawing.Point(0, 0);
      this.lvMerged.MultiSelect = false;
      this.lvMerged.Name = "lvMerged";
      this.lvMerged.Size = new System.Drawing.Size(416, 81);
      this.lvMerged.TabIndex = 0;
      this.lvMerged.UseCompatibleStateImageBehavior = false;
      this.lvMerged.View = System.Windows.Forms.View.Details;
      // 
      // gbMapping
      // 
      this.gbMapping.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbMapping.Controls.Add(this.label3);
      this.gbMapping.Controls.Add(this.cbCountry);
      this.gbMapping.Controls.Add(this.bAutoMap);
      this.gbMapping.Location = new System.Drawing.Point(3, 163);
      this.gbMapping.Name = "gbMapping";
      this.gbMapping.Size = new System.Drawing.Size(443, 58);
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
      this.cbCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCountry.FormattingEnabled = true;
      this.cbCountry.Location = new System.Drawing.Point(84, 23);
      this.cbCountry.Name = "cbCountry";
      this.cbCountry.Size = new System.Drawing.Size(275, 21);
      this.cbCountry.Sorted = true;
      this.cbCountry.TabIndex = 21;
      // 
      // bAutoMap
      // 
      this.bAutoMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bAutoMap.Location = new System.Drawing.Point(366, 21);
      this.bAutoMap.Name = "bAutoMap";
      this.bAutoMap.Size = new System.Drawing.Size(72, 23);
      this.bAutoMap.TabIndex = 19;
      this.bAutoMap.Text = "Auto Map";
      this.bAutoMap.UseVisualStyleBackColor = true;
      this.bAutoMap.Click += new System.EventHandler(this.bAutoMap_Click);
      // 
      // bImport
      // 
      this.bImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bImport.Location = new System.Drawing.Point(358, 17);
      this.bImport.Name = "bImport";
      this.bImport.Size = new System.Drawing.Size(72, 24);
      this.bImport.TabIndex = 11;
      this.bImport.Text = "Import";
      this.bImport.UseVisualStyleBackColor = true;
      this.bImport.Click += new System.EventHandler(this.bImport_Click);
      // 
      // gbGlobal
      // 
      this.gbGlobal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbGlobal.Controls.Add(this.checkBoxDeleteOnlyOverlapping);
      this.gbGlobal.Controls.Add(this.checkBoxDeleteBeforeImport);
      this.gbGlobal.Controls.Add(this.buttonBrowse);
      this.gbGlobal.Controls.Add(this.textBoxFolder);
      this.gbGlobal.Controls.Add(this.DestinationComboBox);
      this.gbGlobal.Controls.Add(this.mpLabel1);
      this.gbGlobal.Controls.Add(this.nMaxGrab);
      this.gbGlobal.Controls.Add(this.lGrabDay);
      this.gbGlobal.Location = new System.Drawing.Point(3, 0);
      this.gbGlobal.Name = "gbGlobal";
      this.gbGlobal.Size = new System.Drawing.Size(443, 157);
      this.gbGlobal.TabIndex = 22;
      this.gbGlobal.TabStop = false;
      this.gbGlobal.Text = "Global Settings";
      // 
      // checkBoxDeleteOnlyOverlapping
      // 
      this.checkBoxDeleteOnlyOverlapping.AutoSize = true;
      this.checkBoxDeleteOnlyOverlapping.Location = new System.Drawing.Point(20, 125);
      this.checkBoxDeleteOnlyOverlapping.Name = "checkBoxDeleteOnlyOverlapping";
      this.checkBoxDeleteOnlyOverlapping.Size = new System.Drawing.Size(262, 17);
      this.checkBoxDeleteOnlyOverlapping.TabIndex = 36;
      this.checkBoxDeleteOnlyOverlapping.Text = "but only if new programs overlap (Recommended!)";
      this.checkBoxDeleteOnlyOverlapping.UseVisualStyleBackColor = true;
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
      this.checkBoxDeleteBeforeImport.CheckedChanged += new System.EventHandler(this.checkBoxDeleteBeforeImport_CheckedChanged);
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBrowse.Location = new System.Drawing.Point(414, 74);
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
      this.textBoxFolder.Size = new System.Drawing.Size(302, 20);
      this.textBoxFolder.TabIndex = 22;
      // 
      // DestinationComboBox
      // 
      this.DestinationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.DestinationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.DestinationComboBox.FormattingEnabled = true;
      this.DestinationComboBox.Items.AddRange(new object[] {
            "TV Server database",
            "tvguide.xml file configured in XmlTv Importer plugin",
            "tvguide.xml in the following folder:"});
      this.DestinationComboBox.Location = new System.Drawing.Point(106, 49);
      this.DestinationComboBox.Name = "DestinationComboBox";
      this.DestinationComboBox.Size = new System.Drawing.Size(331, 21);
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
      // gbChannels
      // 
      this.gbChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbChannels.Controls.Add(this.bImport);
      this.gbChannels.Controls.Add(this.label2);
      this.gbChannels.Controls.Add(this.mtbNewChannel);
      this.gbChannels.Controls.Add(this.lvMapping);
      this.gbChannels.Controls.Add(this.bClearMapping);
      this.gbChannels.Controls.Add(this.lCount);
      this.gbChannels.Controls.Add(this.tbCount);
      this.gbChannels.Controls.Add(this.bRemove);
      this.gbChannels.Controls.Add(this.bAdd);
      this.gbChannels.Controls.Add(this.LoadRadioCheckBox);
      this.gbChannels.Controls.Add(this.label1);
      this.gbChannels.Controls.Add(this.GroupComboBox);
      this.gbChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbChannels.Location = new System.Drawing.Point(3, 3);
      this.gbChannels.Name = "gbChannels";
      this.gbChannels.Size = new System.Drawing.Size(436, 241);
      this.gbChannels.TabIndex = 20;
      this.gbChannels.TabStop = false;
      this.gbChannels.Text = "Channel Mapping";
      // 
      // LoadRadioCheckBox
      // 
      this.LoadRadioCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.LoadRadioCheckBox.AutoSize = true;
      this.LoadRadioCheckBox.Location = new System.Drawing.Point(230, 22);
      this.LoadRadioCheckBox.Name = "LoadRadioCheckBox";
      this.LoadRadioCheckBox.Size = new System.Drawing.Size(122, 17);
      this.LoadRadioCheckBox.TabIndex = 23;
      this.LoadRadioCheckBox.Text = "Load radio channels";
      this.LoadRadioCheckBox.UseVisualStyleBackColor = true;
      this.LoadRadioCheckBox.Visible = false;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(3, 23);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(39, 13);
      this.label1.TabIndex = 22;
      this.label1.Text = "Group:";
      this.label1.Visible = false;
      // 
      // GroupComboBox
      // 
      this.GroupComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.GroupComboBox.FormattingEnabled = true;
      this.GroupComboBox.Location = new System.Drawing.Point(48, 20);
      this.GroupComboBox.Name = "GroupComboBox";
      this.GroupComboBox.Size = new System.Drawing.Size(176, 21);
      this.GroupComboBox.TabIndex = 21;
      this.GroupComboBox.Visible = false;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(6, 22);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(71, 13);
      this.label2.TabIndex = 19;
      this.label2.Text = "New Channel";
      // 
      // mtbNewChannel
      // 
      this.mtbNewChannel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mtbNewChannel.Location = new System.Drawing.Point(85, 19);
      this.mtbNewChannel.Name = "mtbNewChannel";
      this.mtbNewChannel.Size = new System.Drawing.Size(180, 20);
      this.mtbNewChannel.TabIndex = 18;
      // 
      // lvMapping
      // 
      this.lvMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lvMapping.FullRowSelect = true;
      this.lvMapping.HideSelection = false;
      this.lvMapping.Location = new System.Drawing.Point(6, 45);
      this.lvMapping.Name = "lvMapping";
      this.lvMapping.Size = new System.Drawing.Size(424, 160);
      this.lvMapping.TabIndex = 17;
      this.lvMapping.UseCompatibleStateImageBehavior = false;
      this.lvMapping.View = System.Windows.Forms.View.Details;
      this.lvMapping.SelectedIndexChanged += new System.EventHandler(this.lvMapping_SelectedIndexChanged);
      this.lvMapping.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvMapping_ColumnClick);
      // 
      // bClearMapping
      // 
      this.bClearMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bClearMapping.Location = new System.Drawing.Point(253, 211);
      this.bClearMapping.Name = "bClearMapping";
      this.bClearMapping.Size = new System.Drawing.Size(90, 24);
      this.bClearMapping.TabIndex = 20;
      this.bClearMapping.Text = "Clear Mapping";
      this.bClearMapping.UseVisualStyleBackColor = true;
      this.bClearMapping.Click += new System.EventHandler(this.bClearMapping_Click);
      // 
      // lCount
      // 
      this.lCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lCount.Location = new System.Drawing.Point(67, 217);
      this.lCount.Name = "lCount";
      this.lCount.Size = new System.Drawing.Size(80, 16);
      this.lCount.TabIndex = 1;
      this.lCount.Text = "Channel Count";
      // 
      // tbCount
      // 
      this.tbCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.tbCount.Location = new System.Drawing.Point(6, 214);
      this.tbCount.Name = "tbCount";
      this.tbCount.ReadOnly = true;
      this.tbCount.Size = new System.Drawing.Size(55, 20);
      this.tbCount.TabIndex = 0;
      // 
      // bRemove
      // 
      this.bRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bRemove.Location = new System.Drawing.Point(349, 211);
      this.bRemove.Name = "bRemove";
      this.bRemove.Size = new System.Drawing.Size(81, 24);
      this.bRemove.TabIndex = 17;
      this.bRemove.Text = "Remove";
      this.bRemove.UseVisualStyleBackColor = true;
      this.bRemove.Click += new System.EventHandler(this.bRemove_Click);
      // 
      // bAdd
      // 
      this.bAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bAdd.Location = new System.Drawing.Point(271, 17);
      this.bAdd.Name = "bAdd";
      this.bAdd.Size = new System.Drawing.Size(81, 24);
      this.bAdd.TabIndex = 12;
      this.bAdd.Text = "Add";
      this.bAdd.UseVisualStyleBackColor = true;
      this.bAdd.Click += new System.EventHandler(this.bAdd_Click);
      // 
      // bSave
      // 
      this.bSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bSave.Location = new System.Drawing.Point(371, 399);
      this.bSave.Name = "bSave";
      this.bSave.Size = new System.Drawing.Size(72, 24);
      this.bSave.TabIndex = 23;
      this.bSave.Text = "Save";
      this.bSave.UseVisualStyleBackColor = true;
      this.bSave.Click += new System.EventHandler(this.bSave_Click);
      // 
      // tabMain
      // 
      this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabMain.Controls.Add(this.tabSettings);
      this.tabMain.Controls.Add(this.tabMappings);
      this.tabMain.Location = new System.Drawing.Point(0, 0);
      this.tabMain.Name = "tabMain";
      this.tabMain.SelectedIndex = 0;
      this.tabMain.Size = new System.Drawing.Size(457, 455);
      this.tabMain.TabIndex = 26;
      // 
      // tabSettings
      // 
      this.tabSettings.Controls.Add(this.groupBox1);
      this.tabSettings.Controls.Add(this.bSave);
      this.tabSettings.Controls.Add(this.gbMapping);
      this.tabSettings.Controls.Add(this.gbGlobal);
      this.tabSettings.Location = new System.Drawing.Point(4, 22);
      this.tabSettings.Name = "tabSettings";
      this.tabSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabSettings.Size = new System.Drawing.Size(449, 429);
      this.tabSettings.TabIndex = 1;
      this.tabSettings.Text = "Settings";
      this.tabSettings.UseVisualStyleBackColor = true;
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
      this.groupBox1.Location = new System.Drawing.Point(3, 227);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(443, 119);
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
      this.buttonManualImport.Location = new System.Drawing.Point(365, 23);
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
      // tabMappings
      // 
      this.tabMappings.Controls.Add(this.gbChannels);
      this.tabMappings.Controls.Add(this.gbChannelDetails);
      this.tabMappings.Location = new System.Drawing.Point(4, 22);
      this.tabMappings.Name = "tabMappings";
      this.tabMappings.Padding = new System.Windows.Forms.Padding(3);
      this.tabMappings.Size = new System.Drawing.Size(449, 429);
      this.tabMappings.TabIndex = 0;
      this.tabMappings.Text = "Mappings";
      this.tabMappings.UseVisualStyleBackColor = true;
      // 
      // StatusTimer
      // 
      this.StatusTimer.Enabled = true;
      this.StatusTimer.Interval = 10000;
      this.StatusTimer.Tick += new System.EventHandler(this.StatusTimer_Tick);
      // 
      // WebEPGConfigControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabMain);
      this.Name = "WebEPGConfigControl";
      this.Size = new System.Drawing.Size(457, 455);
      this.gbChannelDetails.ResumeLayout(false);
      this.tcMappingDetails.ResumeLayout(false);
      this.tpSingle.ResumeLayout(false);
      this.tpSingle.PerformLayout();
      this.gbGrabber.ResumeLayout(false);
      this.gbGrabber.PerformLayout();
      this.tpMultiple.ResumeLayout(false);
      this.gbMapping.ResumeLayout(false);
      this.gbMapping.PerformLayout();
      this.gbGlobal.ResumeLayout(false);
      this.gbGlobal.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nMaxGrab)).EndInit();
      this.gbChannels.ResumeLayout(false);
      this.gbChannels.PerformLayout();
      this.tabMain.ResumeLayout(false);
      this.tabSettings.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tabMappings.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox gbChannelDetails;
    private System.Windows.Forms.TabControl tcMappingDetails;
    private System.Windows.Forms.TabPage tpSingle;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbGrabber;
    private MediaPortal.UserInterface.Controls.MPTextBox tbGrabDays;
    private MediaPortal.UserInterface.Controls.MPLabel lGuideDays;
    private MediaPortal.UserInterface.Controls.MPButton bGrabber;
    private MediaPortal.UserInterface.Controls.MPLabel Grabber;
    private MediaPortal.UserInterface.Controls.MPTextBox tbGrabSite;
    private MediaPortal.UserInterface.Controls.MPButton bChannelID;
    private MediaPortal.UserInterface.Controls.MPLabel l_cID;
    private MediaPortal.UserInterface.Controls.MPTextBox tbChannelName;
    private System.Windows.Forms.TabPage tpMultiple;
    private System.Windows.Forms.Button bMergedEdit;
    private System.Windows.Forms.Button bMergedRemove;
    private System.Windows.Forms.Button bMergedAdd;
    private System.Windows.Forms.ListView lvMerged;
    private System.Windows.Forms.GroupBox gbMapping;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox cbCountry;
    private System.Windows.Forms.Button bAutoMap;
    private MediaPortal.UserInterface.Controls.MPButton bImport;
    private System.Windows.Forms.GroupBox gbGlobal;
    private System.Windows.Forms.NumericUpDown nMaxGrab;
    private MediaPortal.UserInterface.Controls.MPLabel lGrabDay;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbChannels;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.MaskedTextBox mtbNewChannel;
    private System.Windows.Forms.ListView lvMapping;
    private System.Windows.Forms.Button bClearMapping;
    private MediaPortal.UserInterface.Controls.MPLabel lCount;
    private MediaPortal.UserInterface.Controls.MPTextBox tbCount;
    private MediaPortal.UserInterface.Controls.MPButton bRemove;
    private MediaPortal.UserInterface.Controls.MPButton bAdd;
    private MediaPortal.UserInterface.Controls.MPButton bSave;
    private System.Windows.Forms.TabControl tabMain;
    private System.Windows.Forms.TabPage tabMappings;
    private System.Windows.Forms.TabPage tabSettings;
    private System.Windows.Forms.Button buttonManualImport;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label labelStatus;
    private System.Windows.Forms.Label labelPrograms;
    private System.Windows.Forms.Label labelChannels;
    private System.Windows.Forms.Label labelLastImport;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Timer StatusTimer;
    private MediaPortal.UserInterface.Controls.MPComboBox DestinationComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private System.Windows.Forms.Button buttonBrowse;
    private System.Windows.Forms.TextBox textBoxFolder;
    private System.Windows.Forms.CheckBox checkBoxDeleteBeforeImport;
    private System.Windows.Forms.CheckBox checkBoxDeleteOnlyOverlapping;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogTVGuide;
    private System.Windows.Forms.CheckBox LoadRadioCheckBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox GroupComboBox;
  }
}
