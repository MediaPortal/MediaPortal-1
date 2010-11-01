namespace SetupTv.Sections.WebEPGConfig
{
  partial class WebEPGMappingControl
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
      this.gbChannels = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.bAutoMap = new System.Windows.Forms.Button();
      this.bImport = new MediaPortal.UserInterface.Controls.MPButton();
      this.lvMapping = new System.Windows.Forms.ListView();
      this.bClearMapping = new System.Windows.Forms.Button();
      this.lCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbCount = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.bRemove = new MediaPortal.UserInterface.Controls.MPButton();
      this.label1 = new System.Windows.Forms.Label();
      this.GroupComboBox = new System.Windows.Forms.ComboBox();
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
      this.dgvMerged = new System.Windows.Forms.DataGridView();
      this.idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.grabberColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ChooseGrabberColumn = new System.Windows.Forms.DataGridViewButtonColumn();
      this.startColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.endColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.bsMergedChannel = new System.Windows.Forms.BindingSource(this.components);
      this.gbChannels.SuspendLayout();
      this.gbChannelDetails.SuspendLayout();
      this.tcMappingDetails.SuspendLayout();
      this.tpSingle.SuspendLayout();
      this.gbGrabber.SuspendLayout();
      this.tpMultiple.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgvMerged)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.bsMergedChannel)).BeginInit();
      this.SuspendLayout();
      // 
      // gbChannels
      // 
      this.gbChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbChannels.Controls.Add(this.bAutoMap);
      this.gbChannels.Controls.Add(this.bImport);
      this.gbChannels.Controls.Add(this.lvMapping);
      this.gbChannels.Controls.Add(this.bClearMapping);
      this.gbChannels.Controls.Add(this.lCount);
      this.gbChannels.Controls.Add(this.tbCount);
      this.gbChannels.Controls.Add(this.bRemove);
      this.gbChannels.Controls.Add(this.label1);
      this.gbChannels.Controls.Add(this.GroupComboBox);
      this.gbChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbChannels.Location = new System.Drawing.Point(3, 3);
      this.gbChannels.Name = "gbChannels";
      this.gbChannels.Size = new System.Drawing.Size(443, 241);
      this.gbChannels.TabIndex = 22;
      this.gbChannels.TabStop = false;
      this.gbChannels.Text = "Channel Mapping";
      // 
      // bAutoMap
      // 
      this.bAutoMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bAutoMap.Location = new System.Drawing.Point(365, 17);
      this.bAutoMap.Name = "bAutoMap";
      this.bAutoMap.Size = new System.Drawing.Size(72, 23);
      this.bAutoMap.TabIndex = 23;
      this.bAutoMap.Text = "Auto Map";
      this.bAutoMap.UseVisualStyleBackColor = true;
      this.bAutoMap.Click += new System.EventHandler(this.bAutoMap_Click);
      // 
      // bImport
      // 
      this.bImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bImport.Location = new System.Drawing.Point(287, 17);
      this.bImport.Name = "bImport";
      this.bImport.Size = new System.Drawing.Size(72, 23);
      this.bImport.TabIndex = 11;
      this.bImport.Text = "Import";
      this.bImport.UseVisualStyleBackColor = true;
      this.bImport.Click += new System.EventHandler(this.bImport_Click);
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
      this.lvMapping.Size = new System.Drawing.Size(431, 160);
      this.lvMapping.TabIndex = 17;
      this.lvMapping.UseCompatibleStateImageBehavior = false;
      this.lvMapping.View = System.Windows.Forms.View.Details;
      this.lvMapping.SelectedIndexChanged += new System.EventHandler(this.lvMapping_SelectedIndexChanged);
      this.lvMapping.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvMapping_ColumnClick);
      // 
      // bClearMapping
      // 
      this.bClearMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bClearMapping.Location = new System.Drawing.Point(260, 211);
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
      this.bRemove.Location = new System.Drawing.Point(356, 211);
      this.bRemove.Name = "bRemove";
      this.bRemove.Size = new System.Drawing.Size(81, 24);
      this.bRemove.TabIndex = 17;
      this.bRemove.Text = "Remove";
      this.bRemove.UseVisualStyleBackColor = true;
      this.bRemove.Click += new System.EventHandler(this.bRemove_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(6, 22);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(39, 13);
      this.label1.TabIndex = 22;
      this.label1.Text = "Group:";
      // 
      // GroupComboBox
      // 
      this.GroupComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.GroupComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.GroupComboBox.FormattingEnabled = true;
      this.GroupComboBox.Location = new System.Drawing.Point(48, 19);
      this.GroupComboBox.Name = "GroupComboBox";
      this.GroupComboBox.Size = new System.Drawing.Size(233, 21);
      this.GroupComboBox.TabIndex = 21;
      // 
      // gbChannelDetails
      // 
      this.gbChannelDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbChannelDetails.Controls.Add(this.tcMappingDetails);
      this.gbChannelDetails.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbChannelDetails.Location = new System.Drawing.Point(3, 250);
      this.gbChannelDetails.Name = "gbChannelDetails";
      this.gbChannelDetails.Size = new System.Drawing.Size(443, 176);
      this.gbChannelDetails.TabIndex = 23;
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
      this.tcMappingDetails.Size = new System.Drawing.Size(431, 151);
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
      this.tpSingle.Size = new System.Drawing.Size(423, 125);
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
      this.gbGrabber.Size = new System.Drawing.Size(417, 81);
      this.gbGrabber.TabIndex = 15;
      this.gbGrabber.TabStop = false;
      this.gbGrabber.Text = "Grabber Details";
      // 
      // tbGrabDays
      // 
      this.tbGrabDays.Location = new System.Drawing.Point(79, 44);
      this.tbGrabDays.Name = "tbGrabDays";
      this.tbGrabDays.ReadOnly = true;
      this.tbGrabDays.Size = new System.Drawing.Size(116, 20);
      this.tbGrabDays.TabIndex = 7;
      // 
      // lGuideDays
      // 
      this.lGuideDays.Location = new System.Drawing.Point(3, 47);
      this.lGuideDays.Name = "lGuideDays";
      this.lGuideDays.Size = new System.Drawing.Size(70, 17);
      this.lGuideDays.TabIndex = 8;
      this.lGuideDays.Text = "Guide Days";
      // 
      // bGrabber
      // 
      this.bGrabber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bGrabber.Location = new System.Drawing.Point(381, 17);
      this.bGrabber.Name = "bGrabber";
      this.bGrabber.Size = new System.Drawing.Size(30, 20);
      this.bGrabber.TabIndex = 6;
      this.bGrabber.Text = "...";
      this.bGrabber.UseVisualStyleBackColor = true;
      this.bGrabber.Visible = false;
      // 
      // Grabber
      // 
      this.Grabber.Location = new System.Drawing.Point(3, 21);
      this.Grabber.Name = "Grabber";
      this.Grabber.Size = new System.Drawing.Size(56, 17);
      this.Grabber.TabIndex = 1;
      this.Grabber.Text = "Site";
      // 
      // tbGrabSite
      // 
      this.tbGrabSite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbGrabSite.Location = new System.Drawing.Point(79, 18);
      this.tbGrabSite.Name = "tbGrabSite";
      this.tbGrabSite.ReadOnly = true;
      this.tbGrabSite.Size = new System.Drawing.Size(296, 20);
      this.tbGrabSite.TabIndex = 0;
      // 
      // bChannelID
      // 
      this.bChannelID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bChannelID.Location = new System.Drawing.Point(384, 12);
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
      this.tbChannelName.Size = new System.Drawing.Size(296, 20);
      this.tbChannelName.TabIndex = 7;
      // 
      // tpMultiple
      // 
      this.tpMultiple.Controls.Add(this.bMergedEdit);
      this.tpMultiple.Controls.Add(this.bMergedRemove);
      this.tpMultiple.Controls.Add(this.bMergedAdd);
      this.tpMultiple.Controls.Add(this.dgvMerged);
      this.tpMultiple.Location = new System.Drawing.Point(4, 22);
      this.tpMultiple.Name = "tpMultiple";
      this.tpMultiple.Padding = new System.Windows.Forms.Padding(3);
      this.tpMultiple.Size = new System.Drawing.Size(423, 125);
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
      this.bMergedEdit.Visible = false;
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
      this.bMergedRemove.Visible = false;
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
      this.bMergedAdd.Visible = false;
      // 
      // dgvMerged
      // 
      this.dgvMerged.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.dgvMerged.AutoGenerateColumns = false;
      this.dgvMerged.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
      this.dgvMerged.BackgroundColor = System.Drawing.SystemColors.Window;
      this.dgvMerged.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dgvMerged.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idColumn,
            this.grabberColumn,
            this.ChooseGrabberColumn,
            this.startColumn,
            this.endColumn});
      this.dgvMerged.DataSource = this.bsMergedChannel;
      this.dgvMerged.Location = new System.Drawing.Point(3, 6);
      this.dgvMerged.Name = "dgvMerged";
      this.dgvMerged.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.dgvMerged.Size = new System.Drawing.Size(416, 113);
      this.dgvMerged.TabIndex = 5;
      this.dgvMerged.VirtualMode = true;
      this.dgvMerged.RowValidating += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dgvMerged_RowValidating);
      this.dgvMerged.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMerged_CellEndEdit);
      this.dgvMerged.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMerged_CellContentClick);
      // 
      // idColumn
      // 
      this.idColumn.DataPropertyName = "id";
      this.idColumn.HeaderText = "Channel";
      this.idColumn.Name = "idColumn";
      this.idColumn.ReadOnly = true;
      this.idColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      // 
      // grabberColumn
      // 
      this.grabberColumn.DataPropertyName = "grabber";
      this.grabberColumn.FillWeight = 105F;
      this.grabberColumn.HeaderText = "Grabber";
      this.grabberColumn.Name = "grabberColumn";
      this.grabberColumn.ReadOnly = true;
      // 
      // ChooseGrabberColumn
      // 
      this.ChooseGrabberColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
      this.ChooseGrabberColumn.HeaderText = "";
      this.ChooseGrabberColumn.MinimumWidth = 30;
      this.ChooseGrabberColumn.Name = "ChooseGrabberColumn";
      this.ChooseGrabberColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
      this.ChooseGrabberColumn.Text = "...";
      this.ChooseGrabberColumn.ToolTipText = "Press to select a channel/grabber";
      this.ChooseGrabberColumn.UseColumnTextForButtonValue = true;
      this.ChooseGrabberColumn.Width = 30;
      // 
      // startColumn
      // 
      this.startColumn.DataPropertyName = "start";
      this.startColumn.FillWeight = 50F;
      this.startColumn.HeaderText = "Start";
      this.startColumn.Name = "startColumn";
      // 
      // endColumn
      // 
      this.endColumn.DataPropertyName = "end";
      this.endColumn.FillWeight = 50F;
      this.endColumn.HeaderText = "End";
      this.endColumn.Name = "endColumn";
      // 
      // bsMergedChannel
      // 
      this.bsMergedChannel.DataSource = typeof(MediaPortal.WebEPG.Config.MergedChannel);
      // 
      // WebEPGMappingControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.gbChannels);
      this.Controls.Add(this.gbChannelDetails);
      this.Name = "WebEPGMappingControl";
      this.Size = new System.Drawing.Size(449, 429);
      this.Load += new System.EventHandler(this.WebEPGMappingControl_Load);
      this.gbChannels.ResumeLayout(false);
      this.gbChannels.PerformLayout();
      this.gbChannelDetails.ResumeLayout(false);
      this.tcMappingDetails.ResumeLayout(false);
      this.tpSingle.ResumeLayout(false);
      this.tpSingle.PerformLayout();
      this.gbGrabber.ResumeLayout(false);
      this.gbGrabber.PerformLayout();
      this.tpMultiple.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dgvMerged)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.bsMergedChannel)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox gbChannels;
    private MediaPortal.UserInterface.Controls.MPButton bImport;
    private System.Windows.Forms.ListView lvMapping;
    private System.Windows.Forms.Button bClearMapping;
    private MediaPortal.UserInterface.Controls.MPLabel lCount;
    private MediaPortal.UserInterface.Controls.MPTextBox tbCount;
    private MediaPortal.UserInterface.Controls.MPButton bRemove;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox GroupComboBox;
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
    private System.Windows.Forms.BindingSource bsMergedChannel;
    private System.Windows.Forms.DataGridView dgvMerged;
    private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn grabberColumn;
    private System.Windows.Forms.DataGridViewButtonColumn ChooseGrabberColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn startColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn endColumn;
    private System.Windows.Forms.Button bAutoMap;
  }
}
