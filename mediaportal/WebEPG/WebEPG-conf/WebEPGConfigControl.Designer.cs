namespace WebEPG_conf
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
      this.gbImport = new System.Windows.Forms.GroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbSource = new System.Windows.Forms.ComboBox();
      this.bImport = new MediaPortal.UserInterface.Controls.MPButton();
      this.gbGlobal = new System.Windows.Forms.GroupBox();
      this.nMaxGrab = new System.Windows.Forms.NumericUpDown();
      this.lGrabDay = new MediaPortal.UserInterface.Controls.MPLabel();
      this.gbChannels = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.mtbNewChannel = new System.Windows.Forms.MaskedTextBox();
      this.lvMapping = new System.Windows.Forms.ListView();
      this.bClearMapping = new System.Windows.Forms.Button();
      this.lCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbCount = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.bRemove = new MediaPortal.UserInterface.Controls.MPButton();
      this.bAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.bSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.gbChannelDetails.SuspendLayout();
      this.tcMappingDetails.SuspendLayout();
      this.tpSingle.SuspendLayout();
      this.gbGrabber.SuspendLayout();
      this.tpMultiple.SuspendLayout();
      this.gbMapping.SuspendLayout();
      this.gbImport.SuspendLayout();
      this.gbGlobal.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nMaxGrab)).BeginInit();
      this.gbChannels.SuspendLayout();
      this.SuspendLayout();
      // 
      // gbChannelDetails
      // 
      this.gbChannelDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.gbChannelDetails.Controls.Add(this.tcMappingDetails);
      this.gbChannelDetails.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbChannelDetails.Location = new System.Drawing.Point(396, 3);
      this.gbChannelDetails.Name = "gbChannelDetails";
      this.gbChannelDetails.Size = new System.Drawing.Size(324, 197);
      this.gbChannelDetails.TabIndex = 21;
      this.gbChannelDetails.TabStop = false;
      this.gbChannelDetails.Text = "Mapping Details";
      // 
      // tcMappingDetails
      // 
      this.tcMappingDetails.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
      this.tcMappingDetails.Controls.Add(this.tpSingle);
      this.tcMappingDetails.Controls.Add(this.tpMultiple);
      this.tcMappingDetails.Location = new System.Drawing.Point(6, 17);
      this.tcMappingDetails.Name = "tcMappingDetails";
      this.tcMappingDetails.SelectedIndex = 0;
      this.tcMappingDetails.Size = new System.Drawing.Size(312, 174);
      this.tcMappingDetails.TabIndex = 16;
      this.tcMappingDetails.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tcMappingDetails_Selecting);
      // 
      // tpSingle
      // 
      this.tpSingle.Controls.Add(this.gbGrabber);
      this.tpSingle.Controls.Add(this.bChannelID);
      this.tpSingle.Controls.Add(this.l_cID);
      this.tpSingle.Controls.Add(this.tbChannelName);
      this.tpSingle.Location = new System.Drawing.Point(4, 25);
      this.tpSingle.Name = "tpSingle";
      this.tpSingle.Padding = new System.Windows.Forms.Padding(3);
      this.tpSingle.Size = new System.Drawing.Size(304, 145);
      this.tpSingle.TabIndex = 0;
      this.tpSingle.Text = "Single";
      this.tpSingle.UseVisualStyleBackColor = true;
      // 
      // gbGrabber
      // 
      this.gbGrabber.Controls.Add(this.tbGrabDays);
      this.gbGrabber.Controls.Add(this.lGuideDays);
      this.gbGrabber.Controls.Add(this.bGrabber);
      this.gbGrabber.Controls.Add(this.Grabber);
      this.gbGrabber.Controls.Add(this.tbGrabSite);
      this.gbGrabber.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbGrabber.Location = new System.Drawing.Point(-1, 43);
      this.gbGrabber.Name = "gbGrabber";
      this.gbGrabber.Size = new System.Drawing.Size(305, 65);
      this.gbGrabber.TabIndex = 15;
      this.gbGrabber.TabStop = false;
      this.gbGrabber.Text = "Grabber Details";
      // 
      // tbGrabDays
      // 
      this.tbGrabDays.BorderColor = System.Drawing.Color.Empty;
      this.tbGrabDays.Location = new System.Drawing.Point(68, 39);
      this.tbGrabDays.Name = "tbGrabDays";
      this.tbGrabDays.ReadOnly = true;
      this.tbGrabDays.Size = new System.Drawing.Size(116, 20);
      this.tbGrabDays.TabIndex = 7;
      // 
      // lGuideDays
      // 
      this.lGuideDays.Location = new System.Drawing.Point(3, 42);
      this.lGuideDays.Name = "lGuideDays";
      this.lGuideDays.Size = new System.Drawing.Size(71, 17);
      this.lGuideDays.TabIndex = 8;
      this.lGuideDays.Text = "Guide Days";
      // 
      // bGrabber
      // 
      this.bGrabber.Location = new System.Drawing.Point(269, 12);
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
      this.Grabber.Size = new System.Drawing.Size(56, 23);
      this.Grabber.TabIndex = 1;
      this.Grabber.Text = "Site";
      // 
      // tbGrabSite
      // 
      this.tbGrabSite.BorderColor = System.Drawing.Color.Empty;
      this.tbGrabSite.Location = new System.Drawing.Point(68, 13);
      this.tbGrabSite.Name = "tbGrabSite";
      this.tbGrabSite.ReadOnly = true;
      this.tbGrabSite.Size = new System.Drawing.Size(195, 20);
      this.tbGrabSite.TabIndex = 0;
      // 
      // bChannelID
      // 
      this.bChannelID.Location = new System.Drawing.Point(268, 17);
      this.bChannelID.Name = "bChannelID";
      this.bChannelID.Size = new System.Drawing.Size(30, 20);
      this.bChannelID.TabIndex = 9;
      this.bChannelID.Text = "...";
      this.bChannelID.UseVisualStyleBackColor = true;
      this.bChannelID.Click += new System.EventHandler(this.bChannelID_Click);
      // 
      // l_cID
      // 
      this.l_cID.Location = new System.Drawing.Point(2, 20);
      this.l_cID.Name = "l_cID";
      this.l_cID.Size = new System.Drawing.Size(56, 20);
      this.l_cID.TabIndex = 8;
      this.l_cID.Text = "Channel";
      // 
      // tbChannelName
      // 
      this.tbChannelName.BorderColor = System.Drawing.Color.Empty;
      this.tbChannelName.Location = new System.Drawing.Point(67, 17);
      this.tbChannelName.Name = "tbChannelName";
      this.tbChannelName.ReadOnly = true;
      this.tbChannelName.Size = new System.Drawing.Size(195, 20);
      this.tbChannelName.TabIndex = 7;
      // 
      // tpMultiple
      // 
      this.tpMultiple.Controls.Add(this.bMergedEdit);
      this.tpMultiple.Controls.Add(this.bMergedRemove);
      this.tpMultiple.Controls.Add(this.bMergedAdd);
      this.tpMultiple.Controls.Add(this.lvMerged);
      this.tpMultiple.Location = new System.Drawing.Point(4, 25);
      this.tpMultiple.Name = "tpMultiple";
      this.tpMultiple.Padding = new System.Windows.Forms.Padding(3);
      this.tpMultiple.Size = new System.Drawing.Size(304, 145);
      this.tpMultiple.TabIndex = 1;
      this.tpMultiple.Text = "Multiple (Merged)";
      this.tpMultiple.UseVisualStyleBackColor = true;
      // 
      // bMergedEdit
      // 
      this.bMergedEdit.Location = new System.Drawing.Point(115, 116);
      this.bMergedEdit.Name = "bMergedEdit";
      this.bMergedEdit.Size = new System.Drawing.Size(75, 23);
      this.bMergedEdit.TabIndex = 3;
      this.bMergedEdit.Text = "Edit";
      this.bMergedEdit.UseVisualStyleBackColor = true;
      this.bMergedEdit.Click += new System.EventHandler(this.bMergedEdit_Click);
      // 
      // bMergedRemove
      // 
      this.bMergedRemove.Location = new System.Drawing.Point(227, 117);
      this.bMergedRemove.Name = "bMergedRemove";
      this.bMergedRemove.Size = new System.Drawing.Size(75, 23);
      this.bMergedRemove.TabIndex = 2;
      this.bMergedRemove.Text = "Remove";
      this.bMergedRemove.UseVisualStyleBackColor = true;
      this.bMergedRemove.Click += new System.EventHandler(this.bMergedRemove_Click);
      // 
      // bMergedAdd
      // 
      this.bMergedAdd.Location = new System.Drawing.Point(6, 116);
      this.bMergedAdd.Name = "bMergedAdd";
      this.bMergedAdd.Size = new System.Drawing.Size(75, 23);
      this.bMergedAdd.TabIndex = 1;
      this.bMergedAdd.Text = "Add";
      this.bMergedAdd.UseVisualStyleBackColor = true;
      this.bMergedAdd.Click += new System.EventHandler(this.bMergedAdd_Click);
      // 
      // lvMerged
      // 
      this.lvMerged.FullRowSelect = true;
      this.lvMerged.HideSelection = false;
      this.lvMerged.Location = new System.Drawing.Point(-1, 0);
      this.lvMerged.MultiSelect = false;
      this.lvMerged.Name = "lvMerged";
      this.lvMerged.Size = new System.Drawing.Size(310, 111);
      this.lvMerged.TabIndex = 0;
      this.lvMerged.UseCompatibleStateImageBehavior = false;
      this.lvMerged.View = System.Windows.Forms.View.Details;
      // 
      // gbMapping
      // 
      this.gbMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.gbMapping.Controls.Add(this.label3);
      this.gbMapping.Controls.Add(this.cbCountry);
      this.gbMapping.Controls.Add(this.bAutoMap);
      this.gbMapping.Location = new System.Drawing.Point(396, 283);
      this.gbMapping.Name = "gbMapping";
      this.gbMapping.Size = new System.Drawing.Size(324, 58);
      this.gbMapping.TabIndex = 25;
      this.gbMapping.TabStop = false;
      this.gbMapping.Text = "Auto Mapping";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(9, 26);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(43, 13);
      this.label3.TabIndex = 22;
      this.label3.Text = "Country";
      // 
      // cbCountry
      // 
      this.cbCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCountry.FormattingEnabled = true;
      this.cbCountry.Location = new System.Drawing.Point(86, 23);
      this.cbCountry.Name = "cbCountry";
      this.cbCountry.Size = new System.Drawing.Size(151, 21);
      this.cbCountry.Sorted = true;
      this.cbCountry.TabIndex = 21;
      // 
      // bAutoMap
      // 
      this.bAutoMap.Location = new System.Drawing.Point(244, 21);
      this.bAutoMap.Name = "bAutoMap";
      this.bAutoMap.Size = new System.Drawing.Size(72, 23);
      this.bAutoMap.TabIndex = 19;
      this.bAutoMap.Text = "Auto Map";
      this.bAutoMap.UseVisualStyleBackColor = true;
      this.bAutoMap.Click += new System.EventHandler(this.bAutoMap_Click);
      // 
      // gbImport
      // 
      this.gbImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.gbImport.Controls.Add(this.label1);
      this.gbImport.Controls.Add(this.cbSource);
      this.gbImport.Controls.Add(this.bImport);
      this.gbImport.Location = new System.Drawing.Point(396, 347);
      this.gbImport.Name = "gbImport";
      this.gbImport.Size = new System.Drawing.Size(324, 54);
      this.gbImport.TabIndex = 24;
      this.gbImport.TabStop = false;
      this.gbImport.Text = "Import Channel Data";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(6, 25);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(44, 13);
      this.label1.TabIndex = 13;
      this.label1.Text = "Source:";
      // 
      // cbSource
      // 
      this.cbSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbSource.FormattingEnabled = true;
      this.cbSource.Items.AddRange(new object[] {
            "MediaPortal",
            "TV Server"});
      this.cbSource.Location = new System.Drawing.Point(86, 19);
      this.cbSource.Name = "cbSource";
      this.cbSource.Size = new System.Drawing.Size(151, 21);
      this.cbSource.TabIndex = 12;
      this.cbSource.SelectedIndexChanged += new System.EventHandler(this.cbSource_SelectedIndexChanged);
      // 
      // bImport
      // 
      this.bImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.bImport.Location = new System.Drawing.Point(244, 19);
      this.bImport.Name = "bImport";
      this.bImport.Size = new System.Drawing.Size(72, 24);
      this.bImport.TabIndex = 11;
      this.bImport.Text = "Import";
      this.bImport.UseVisualStyleBackColor = true;
      this.bImport.Click += new System.EventHandler(this.bImport_Click);
      // 
      // gbGlobal
      // 
      this.gbGlobal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.gbGlobal.Controls.Add(this.nMaxGrab);
      this.gbGlobal.Controls.Add(this.lGrabDay);
      this.gbGlobal.Location = new System.Drawing.Point(396, 218);
      this.gbGlobal.Name = "gbGlobal";
      this.gbGlobal.Size = new System.Drawing.Size(324, 59);
      this.gbGlobal.TabIndex = 22;
      this.gbGlobal.TabStop = false;
      this.gbGlobal.Text = "Global Settings";
      // 
      // nMaxGrab
      // 
      this.nMaxGrab.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.nMaxGrab.Location = new System.Drawing.Point(86, 22);
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
      this.nMaxGrab.Size = new System.Drawing.Size(56, 20);
      this.nMaxGrab.TabIndex = 13;
      this.nMaxGrab.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
      // 
      // lGrabDay
      // 
      this.lGrabDay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.lGrabDay.Location = new System.Drawing.Point(3, 26);
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
      this.gbChannels.Controls.Add(this.label2);
      this.gbChannels.Controls.Add(this.mtbNewChannel);
      this.gbChannels.Controls.Add(this.lvMapping);
      this.gbChannels.Controls.Add(this.bClearMapping);
      this.gbChannels.Controls.Add(this.lCount);
      this.gbChannels.Controls.Add(this.tbCount);
      this.gbChannels.Controls.Add(this.bRemove);
      this.gbChannels.Controls.Add(this.bAdd);
      this.gbChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbChannels.Location = new System.Drawing.Point(0, 3);
      this.gbChannels.Name = "gbChannels";
      this.gbChannels.Size = new System.Drawing.Size(390, 494);
      this.gbChannels.TabIndex = 20;
      this.gbChannels.TabStop = false;
      this.gbChannels.Text = "Channel Mapping";
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
      this.mtbNewChannel.Size = new System.Drawing.Size(206, 20);
      this.mtbNewChannel.TabIndex = 18;
      // 
      // lvMapping
      // 
      this.lvMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lvMapping.FullRowSelect = true;
      this.lvMapping.HideSelection = false;
      this.lvMapping.Location = new System.Drawing.Point(6, 47);
      this.lvMapping.Name = "lvMapping";
      this.lvMapping.Size = new System.Drawing.Size(376, 407);
      this.lvMapping.TabIndex = 17;
      this.lvMapping.UseCompatibleStateImageBehavior = false;
      this.lvMapping.View = System.Windows.Forms.View.Details;
      this.lvMapping.SelectedIndexChanged += new System.EventHandler(this.lvMapping_SelectedIndexChanged);
      // 
      // bClearMapping
      // 
      this.bClearMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bClearMapping.Location = new System.Drawing.Point(205, 461);
      this.bClearMapping.Name = "bClearMapping";
      this.bClearMapping.Size = new System.Drawing.Size(90, 23);
      this.bClearMapping.TabIndex = 20;
      this.bClearMapping.Text = "Clear Mapping";
      this.bClearMapping.UseVisualStyleBackColor = true;
      this.bClearMapping.Click += new System.EventHandler(this.bClearMapping_Click);
      // 
      // lCount
      // 
      this.lCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lCount.Location = new System.Drawing.Point(67, 466);
      this.lCount.Name = "lCount";
      this.lCount.Size = new System.Drawing.Size(80, 16);
      this.lCount.TabIndex = 1;
      this.lCount.Text = "Channel Count";
      // 
      // tbCount
      // 
      this.tbCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.tbCount.BorderColor = System.Drawing.Color.Empty;
      this.tbCount.Location = new System.Drawing.Point(6, 463);
      this.tbCount.Name = "tbCount";
      this.tbCount.ReadOnly = true;
      this.tbCount.Size = new System.Drawing.Size(55, 20);
      this.tbCount.TabIndex = 0;
      // 
      // bRemove
      // 
      this.bRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bRemove.Location = new System.Drawing.Point(301, 460);
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
      this.bAdd.Location = new System.Drawing.Point(297, 17);
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
      this.bSave.Location = new System.Drawing.Point(640, 476);
      this.bSave.Name = "bSave";
      this.bSave.Size = new System.Drawing.Size(72, 24);
      this.bSave.TabIndex = 23;
      this.bSave.Text = "Save";
      this.bSave.UseVisualStyleBackColor = true;
      this.bSave.Click += new System.EventHandler(this.bSave_Click);
      // 
      // WebEPGConfigControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.gbChannelDetails);
      this.Controls.Add(this.gbMapping);
      this.Controls.Add(this.gbImport);
      this.Controls.Add(this.gbGlobal);
      this.Controls.Add(this.gbChannels);
      this.Controls.Add(this.bSave);
      this.Name = "WebEPGConfigControl";
      this.Size = new System.Drawing.Size(720, 500);
      this.gbChannelDetails.ResumeLayout(false);
      this.tcMappingDetails.ResumeLayout(false);
      this.tpSingle.ResumeLayout(false);
      this.tpSingle.PerformLayout();
      this.gbGrabber.ResumeLayout(false);
      this.gbGrabber.PerformLayout();
      this.tpMultiple.ResumeLayout(false);
      this.gbMapping.ResumeLayout(false);
      this.gbMapping.PerformLayout();
      this.gbImport.ResumeLayout(false);
      this.gbImport.PerformLayout();
      this.gbGlobal.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.nMaxGrab)).EndInit();
      this.gbChannels.ResumeLayout(false);
      this.gbChannels.PerformLayout();
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
    private System.Windows.Forms.GroupBox gbImport;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox cbSource;
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
  }
}
