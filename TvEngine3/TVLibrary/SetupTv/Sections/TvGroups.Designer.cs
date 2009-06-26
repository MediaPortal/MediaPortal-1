namespace SetupTv.Sections
{
  partial class TvGroups
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TvGroups));
      this.mpTabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpButtonRenameGroup = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonDown = new System.Windows.Forms.Button();
      this.buttonUtp = new System.Windows.Forms.Button();
      this.mpButtonAddGroup = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonDeleteGroup = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpListViewGroups = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.cbFtaChannels = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbVisibleChannels = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.btnDown = new System.Windows.Forms.Button();
      this.btnUp = new System.Windows.Forms.Button();
      this.mpButtonUnmap = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonMap = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListViewMapped = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.mpListViewChannels = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.mpComboBoxGroup = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpTabControl1
      // 
      this.mpTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpTabControl1.Controls.Add(this.tabPage1);
      this.mpTabControl1.Controls.Add(this.tabPage2);
      this.mpTabControl1.Location = new System.Drawing.Point(3, 3);
      this.mpTabControl1.Name = "mpTabControl1";
      this.mpTabControl1.SelectedIndex = 0;
      this.mpTabControl1.Size = new System.Drawing.Size(461, 385);
      this.mpTabControl1.TabIndex = 0;
      this.mpTabControl1.TabIndexChanged += new System.EventHandler(this.mpTabControl1_TabIndexChanged);
      this.mpTabControl1.SelectedIndexChanged += new System.EventHandler(this.mpTabControl1_SelectedIndexChanged);
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpButtonRenameGroup);
      this.tabPage1.Controls.Add(this.buttonDown);
      this.tabPage1.Controls.Add(this.buttonUtp);
      this.tabPage1.Controls.Add(this.mpButtonAddGroup);
      this.tabPage1.Controls.Add(this.mpButtonDeleteGroup);
      this.tabPage1.Controls.Add(this.mpListViewGroups);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(453, 359);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Groups";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpButtonRenameGroup
      // 
      this.mpButtonRenameGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonRenameGroup.Location = new System.Drawing.Point(272, 314);
      this.mpButtonRenameGroup.Name = "mpButtonRenameGroup";
      this.mpButtonRenameGroup.Size = new System.Drawing.Size(75, 23);
      this.mpButtonRenameGroup.TabIndex = 7;
      this.mpButtonRenameGroup.Text = "Rename";
      this.mpButtonRenameGroup.UseVisualStyleBackColor = true;
      this.mpButtonRenameGroup.Click += new System.EventHandler(this.mpButtonRenameGroup_Click);
      // 
      // buttonDown
      // 
      this.buttonDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonDown.Location = new System.Drawing.Point(17, 328);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(55, 23);
      this.buttonDown.TabIndex = 6;
      this.buttonDown.Text = "Down";
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // buttonUtp
      // 
      this.buttonUtp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonUtp.Location = new System.Drawing.Point(17, 304);
      this.buttonUtp.Name = "buttonUtp";
      this.buttonUtp.Size = new System.Drawing.Size(55, 23);
      this.buttonUtp.TabIndex = 5;
      this.buttonUtp.Text = "Up";
      this.buttonUtp.UseVisualStyleBackColor = true;
      this.buttonUtp.Click += new System.EventHandler(this.buttonUtp_Click);
      // 
      // mpButtonAddGroup
      // 
      this.mpButtonAddGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonAddGroup.Location = new System.Drawing.Point(181, 314);
      this.mpButtonAddGroup.Name = "mpButtonAddGroup";
      this.mpButtonAddGroup.Size = new System.Drawing.Size(75, 23);
      this.mpButtonAddGroup.TabIndex = 2;
      this.mpButtonAddGroup.Text = "Add";
      this.mpButtonAddGroup.UseVisualStyleBackColor = true;
      this.mpButtonAddGroup.Click += new System.EventHandler(this.mpButtonAddGroup_Click);
      // 
      // mpButtonDeleteGroup
      // 
      this.mpButtonDeleteGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonDeleteGroup.Location = new System.Drawing.Point(359, 314);
      this.mpButtonDeleteGroup.Name = "mpButtonDeleteGroup";
      this.mpButtonDeleteGroup.Size = new System.Drawing.Size(75, 23);
      this.mpButtonDeleteGroup.TabIndex = 1;
      this.mpButtonDeleteGroup.Text = "Delete";
      this.mpButtonDeleteGroup.UseVisualStyleBackColor = true;
      this.mpButtonDeleteGroup.Click += new System.EventHandler(this.mpButtonDeleteGroup_Click);
      // 
      // mpListViewGroups
      // 
      this.mpListViewGroups.AllowDrop = true;
      this.mpListViewGroups.AllowRowReorder = false;
      this.mpListViewGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpListViewGroups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
      this.mpListViewGroups.FullRowSelect = true;
      this.mpListViewGroups.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.mpListViewGroups.HideSelection = false;
      this.mpListViewGroups.Location = new System.Drawing.Point(17, 6);
      this.mpListViewGroups.Name = "mpListViewGroups";
      this.mpListViewGroups.Size = new System.Drawing.Size(417, 292);
      this.mpListViewGroups.TabIndex = 0;
      this.mpListViewGroups.UseCompatibleStateImageBehavior = false;
      this.mpListViewGroups.View = System.Windows.Forms.View.Details;
      this.mpListViewGroups.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mpListViewGroups_MouseDoubleClick);
      this.mpListViewGroups.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mpListViewGroups_ColumnClick);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Name";
      this.columnHeader1.Width = 400;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.cbFtaChannels);
      this.tabPage2.Controls.Add(this.cbVisibleChannels);
      this.tabPage2.Controls.Add(this.btnDown);
      this.tabPage2.Controls.Add(this.btnUp);
      this.tabPage2.Controls.Add(this.mpButtonUnmap);
      this.tabPage2.Controls.Add(this.mpButtonMap);
      this.tabPage2.Controls.Add(this.mpLabel3);
      this.tabPage2.Controls.Add(this.mpListViewMapped);
      this.tabPage2.Controls.Add(this.mpListViewChannels);
      this.tabPage2.Controls.Add(this.mpComboBoxGroup);
      this.tabPage2.Controls.Add(this.mpLabel1);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(453, 359);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Mapping";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // cbFtaChannels
      // 
      this.cbFtaChannels.AutoSize = true;
      this.cbFtaChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbFtaChannels.Location = new System.Drawing.Point(10, 58);
      this.cbFtaChannels.Name = "cbFtaChannels";
      this.cbFtaChannels.Size = new System.Drawing.Size(174, 17);
      this.cbFtaChannels.TabIndex = 19;
      this.cbFtaChannels.Text = "Show only Free-To-Air channels";
      this.cbFtaChannels.UseVisualStyleBackColor = true;
      this.cbFtaChannels.CheckedChanged += new System.EventHandler(this.mpComboBoxGroup_SelectedIndexChanged);
      // 
      // cbVisibleChannels
      // 
      this.cbVisibleChannels.AutoSize = true;
      this.cbVisibleChannels.Checked = true;
      this.cbVisibleChannels.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbVisibleChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbVisibleChannels.Location = new System.Drawing.Point(10, 37);
      this.cbVisibleChannels.Name = "cbVisibleChannels";
      this.cbVisibleChannels.Size = new System.Drawing.Size(148, 17);
      this.cbVisibleChannels.TabIndex = 18;
      this.cbVisibleChannels.Text = "Show only guide channels";
      this.cbVisibleChannels.UseVisualStyleBackColor = true;
      this.cbVisibleChannels.CheckedChanged += new System.EventHandler(this.mpComboBoxGroup_SelectedIndexChanged);
      // 
      // btnDown
      // 
      this.btnDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnDown.Location = new System.Drawing.Point(200, 330);
      this.btnDown.Name = "btnDown";
      this.btnDown.Size = new System.Drawing.Size(43, 23);
      this.btnDown.TabIndex = 17;
      this.btnDown.Text = "Down";
      this.btnDown.UseVisualStyleBackColor = true;
      this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
      // 
      // btnUp
      // 
      this.btnUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnUp.Location = new System.Drawing.Point(200, 301);
      this.btnUp.Name = "btnUp";
      this.btnUp.Size = new System.Drawing.Size(43, 23);
      this.btnUp.TabIndex = 16;
      this.btnUp.Text = "Up";
      this.btnUp.UseVisualStyleBackColor = true;
      this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
      // 
      // mpButtonUnmap
      // 
      this.mpButtonUnmap.Location = new System.Drawing.Point(200, 113);
      this.mpButtonUnmap.Name = "mpButtonUnmap";
      this.mpButtonUnmap.Size = new System.Drawing.Size(43, 23);
      this.mpButtonUnmap.TabIndex = 15;
      this.mpButtonUnmap.Text = "<<";
      this.mpButtonUnmap.UseVisualStyleBackColor = true;
      this.mpButtonUnmap.Click += new System.EventHandler(this.mpButtonUnmap_Click);
      // 
      // mpButtonMap
      // 
      this.mpButtonMap.Location = new System.Drawing.Point(200, 84);
      this.mpButtonMap.Name = "mpButtonMap";
      this.mpButtonMap.Size = new System.Drawing.Size(43, 23);
      this.mpButtonMap.TabIndex = 14;
      this.mpButtonMap.Text = ">>";
      this.mpButtonMap.UseVisualStyleBackColor = true;
      this.mpButtonMap.Click += new System.EventHandler(this.mpButtonMap_Click);
      // 
      // mpLabel3
      // 
      this.mpLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(249, 39);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(92, 13);
      this.mpLabel3.TabIndex = 13;
      this.mpLabel3.Text = "Channels in group";
      // 
      // mpListViewMapped
      // 
      this.mpListViewMapped.AllowDrop = true;
      this.mpListViewMapped.AllowRowReorder = true;
      this.mpListViewMapped.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpListViewMapped.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader3});
      this.mpListViewMapped.LargeImageList = this.imageList1;
      this.mpListViewMapped.Location = new System.Drawing.Point(249, 58);
      this.mpListViewMapped.Name = "mpListViewMapped";
      this.mpListViewMapped.Size = new System.Drawing.Size(194, 295);
      this.mpListViewMapped.SmallImageList = this.imageList1;
      this.mpListViewMapped.TabIndex = 12;
      this.mpListViewMapped.UseCompatibleStateImageBehavior = false;
      this.mpListViewMapped.View = System.Windows.Forms.View.Details;
      this.mpListViewMapped.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mpListViewMapped_MouseDoubleClick);
      this.mpListViewMapped.DragDrop += new System.Windows.Forms.DragEventHandler(this.mpListViewMapped_DragDrop);
      this.mpListViewMapped.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mpListViewMapped_ColumnClick);
      this.mpListViewMapped.DragEnter += new System.Windows.Forms.DragEventHandler(this.mpListViewMapped_DragEnter);
      this.mpListViewMapped.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.mpListViewMapped_ItemDrag);
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Name";
      this.columnHeader2.Width = 115;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Channel number";
      this.columnHeader3.Width = 90;
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "radio_scrambled.png");
      this.imageList1.Images.SetKeyName(1, "tv_fta_.png");
      this.imageList1.Images.SetKeyName(2, "tv_scrambled.png");
      this.imageList1.Images.SetKeyName(3, "radio_fta_.png");
      // 
      // mpListViewChannels
      // 
      this.mpListViewChannels.AllowDrop = true;
      this.mpListViewChannels.AllowRowReorder = false;
      this.mpListViewChannels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.mpListViewChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
      this.mpListViewChannels.LargeImageList = this.imageList1;
      this.mpListViewChannels.Location = new System.Drawing.Point(10, 81);
      this.mpListViewChannels.Name = "mpListViewChannels";
      this.mpListViewChannels.Size = new System.Drawing.Size(184, 272);
      this.mpListViewChannels.SmallImageList = this.imageList1;
      this.mpListViewChannels.TabIndex = 10;
      this.mpListViewChannels.UseCompatibleStateImageBehavior = false;
      this.mpListViewChannels.View = System.Windows.Forms.View.Details;
      this.mpListViewChannels.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mpListViewChannels_MouseDoubleClick);
      this.mpListViewChannels.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mpListViewChannels_ColumnClick);
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Name";
      this.columnHeader4.Width = 180;
      // 
      // mpComboBoxGroup
      // 
      this.mpComboBoxGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpComboBoxGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxGroup.FormattingEnabled = true;
      this.mpComboBoxGroup.Location = new System.Drawing.Point(101, 4);
      this.mpComboBoxGroup.Name = "mpComboBoxGroup";
      this.mpComboBoxGroup.Size = new System.Drawing.Size(344, 21);
      this.mpComboBoxGroup.TabIndex = 8;
      this.mpComboBoxGroup.SelectedIndexChanged += new System.EventHandler(this.mpComboBoxGroup_SelectedIndexChanged);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(7, 7);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(68, 13);
      this.mpLabel1.TabIndex = 9;
      this.mpLabel1.Text = "Group name:";
      // 
      // TvGroups
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpTabControl1);
      this.Name = "TvGroups";
      this.Size = new System.Drawing.Size(467, 388);
      this.mpTabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tabPage2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPTabControl mpTabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private MediaPortal.UserInterface.Controls.MPListView mpListViewGroups;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonDeleteGroup;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonAddGroup;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonUnmap;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonMap;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPListView mpListViewMapped;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPListView mpListViewChannels;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxGroup;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ImageList imageList1;
      private System.Windows.Forms.Button buttonDown;
      private System.Windows.Forms.Button buttonUtp;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonRenameGroup;
    private System.Windows.Forms.Button btnDown;
    private System.Windows.Forms.Button btnUp;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbVisibleChannels;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbFtaChannels;
  }
}