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
      this.mpTabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpButtonAddGroup = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonDeleteGroup = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpListViewGroups = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.mpButtonUnmap = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonMap = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListViewMapped = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
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
      this.mpTabControl1.Controls.Add(this.tabPage1);
      this.mpTabControl1.Controls.Add(this.tabPage2);
      this.mpTabControl1.Location = new System.Drawing.Point(3, 3);
      this.mpTabControl1.Name = "mpTabControl1";
      this.mpTabControl1.SelectedIndex = 0;
      this.mpTabControl1.Size = new System.Drawing.Size(461, 382);
      this.mpTabControl1.TabIndex = 0;
      this.mpTabControl1.TabIndexChanged += new System.EventHandler(this.mpTabControl1_TabIndexChanged);
      this.mpTabControl1.SelectedIndexChanged += new System.EventHandler(this.mpTabControl1_SelectedIndexChanged);
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpButtonAddGroup);
      this.tabPage1.Controls.Add(this.mpButtonDeleteGroup);
      this.tabPage1.Controls.Add(this.mpListViewGroups);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(453, 356);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Groups";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpButtonAddGroup
      // 
      this.mpButtonAddGroup.Location = new System.Drawing.Point(265, 314);
      this.mpButtonAddGroup.Name = "mpButtonAddGroup";
      this.mpButtonAddGroup.Size = new System.Drawing.Size(75, 23);
      this.mpButtonAddGroup.TabIndex = 2;
      this.mpButtonAddGroup.Text = "Add";
      this.mpButtonAddGroup.UseVisualStyleBackColor = true;
      this.mpButtonAddGroup.Click += new System.EventHandler(this.mpButtonAddGroup_Click);
      // 
      // mpButtonDeleteGroup
      // 
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
      this.mpListViewGroups.AllowRowReorder = true;
      this.mpListViewGroups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
      this.mpListViewGroups.FullRowSelect = true;
      this.mpListViewGroups.Location = new System.Drawing.Point(17, 6);
      this.mpListViewGroups.Name = "mpListViewGroups";
      this.mpListViewGroups.Size = new System.Drawing.Size(417, 292);
      this.mpListViewGroups.TabIndex = 0;
      this.mpListViewGroups.UseCompatibleStateImageBehavior = false;
      this.mpListViewGroups.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Name";
      this.columnHeader1.Width = 250;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.mpButtonUnmap);
      this.tabPage2.Controls.Add(this.mpButtonMap);
      this.tabPage2.Controls.Add(this.mpLabel3);
      this.tabPage2.Controls.Add(this.mpListViewMapped);
      this.tabPage2.Controls.Add(this.mpLabel2);
      this.tabPage2.Controls.Add(this.mpListViewChannels);
      this.tabPage2.Controls.Add(this.mpComboBoxGroup);
      this.tabPage2.Controls.Add(this.mpLabel1);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(453, 356);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Mapping";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // mpButtonUnmap
      // 
      this.mpButtonUnmap.Location = new System.Drawing.Point(219, 127);
      this.mpButtonUnmap.Name = "mpButtonUnmap";
      this.mpButtonUnmap.Size = new System.Drawing.Size(27, 23);
      this.mpButtonUnmap.TabIndex = 15;
      this.mpButtonUnmap.Text = "<<";
      this.mpButtonUnmap.UseVisualStyleBackColor = true;
      this.mpButtonUnmap.Click += new System.EventHandler(this.mpButtonUnmap_Click);
      // 
      // mpButtonMap
      // 
      this.mpButtonMap.Location = new System.Drawing.Point(219, 98);
      this.mpButtonMap.Name = "mpButtonMap";
      this.mpButtonMap.Size = new System.Drawing.Size(27, 23);
      this.mpButtonMap.TabIndex = 14;
      this.mpButtonMap.Text = ">>";
      this.mpButtonMap.UseVisualStyleBackColor = true;
      this.mpButtonMap.Click += new System.EventHandler(this.mpButtonMap_Click);
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(249, 39);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(94, 13);
      this.mpLabel3.TabIndex = 13;
      this.mpLabel3.Text = "Channels in Group";
      // 
      // mpListViewMapped
      // 
      this.mpListViewMapped.AllowDrop = true;
      this.mpListViewMapped.AllowRowReorder = true;
      this.mpListViewMapped.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
      this.mpListViewMapped.Location = new System.Drawing.Point(252, 58);
      this.mpListViewMapped.Name = "mpListViewMapped";
      this.mpListViewMapped.Size = new System.Drawing.Size(193, 282);
      this.mpListViewMapped.TabIndex = 12;
      this.mpListViewMapped.UseCompatibleStateImageBehavior = false;
      this.mpListViewMapped.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Name";
      this.columnHeader2.Width = 180;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(7, 39);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(54, 13);
      this.mpLabel2.TabIndex = 11;
      this.mpLabel2.Text = "Channels:";
      // 
      // mpListViewChannels
      // 
      this.mpListViewChannels.AllowDrop = true;
      this.mpListViewChannels.AllowRowReorder = true;
      this.mpListViewChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
      this.mpListViewChannels.Location = new System.Drawing.Point(10, 58);
      this.mpListViewChannels.Name = "mpListViewChannels";
      this.mpListViewChannels.Size = new System.Drawing.Size(193, 282);
      this.mpListViewChannels.TabIndex = 10;
      this.mpListViewChannels.UseCompatibleStateImageBehavior = false;
      this.mpListViewChannels.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Name";
      this.columnHeader4.Width = 180;
      // 
      // mpComboBoxGroup
      // 
      this.mpComboBoxGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxGroup.FormattingEnabled = true;
      this.mpComboBoxGroup.Location = new System.Drawing.Point(101, 4);
      this.mpComboBoxGroup.Name = "mpComboBoxGroup";
      this.mpComboBoxGroup.Size = new System.Drawing.Size(249, 21);
      this.mpComboBoxGroup.TabIndex = 8;
      this.mpComboBoxGroup.SelectedIndexChanged += new System.EventHandler(this.mpComboBoxGroup_SelectedIndexChanged);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(29, 7);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(39, 13);
      this.mpLabel1.TabIndex = 9;
      this.mpLabel1.Text = "Group:";
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
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPListView mpListViewChannels;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxGroup;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
  }
}