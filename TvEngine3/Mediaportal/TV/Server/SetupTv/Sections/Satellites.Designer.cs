using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class Satellites
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Satellites));
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpGroupBox1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.btnUpdateList = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.btnAddSat = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.btnEditSat = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.btnDelSat = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.mpLabel2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpListViewSatellites = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.mpLabelChannelCount = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.listViewStatus = new System.Windows.Forms.ListView();
      this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "radio_fta_.png");
      this.imageList1.Images.SetKeyName(1, "radio_scrambled.png");
      this.imageList1.Images.SetKeyName(2, "icon.radio_scrambled_and_fta.png");
      this.imageList1.Images.SetKeyName(3, "tv_fta_.png");
      this.imageList1.Images.SetKeyName(4, "tv_scrambled.png");
      this.imageList1.Images.SetKeyName(5, "icon.tv_scrambled_and_fta.png");
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Location = new System.Drawing.Point(3, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(454, 458);
      this.tabControl1.TabIndex = 18;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.listViewStatus);
      this.tabPage1.Controls.Add(this.mpGroupBox1);
      this.tabPage1.Controls.Add(this.mpLabel2);
      this.tabPage1.Controls.Add(this.mpListViewSatellites);
      this.tabPage1.Controls.Add(this.mpLabelChannelCount);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(446, 432);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Satellites";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpGroupBox1.Controls.Add(this.btnUpdateList);
      this.mpGroupBox1.Controls.Add(this.btnAddSat);
      this.mpGroupBox1.Controls.Add(this.btnEditSat);
      this.mpGroupBox1.Controls.Add(this.btnDelSat);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(6, 379);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(298, 47);
      this.mpGroupBox1.TabIndex = 26;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Satellite";
      // 
      // btnUpdateList
      // 
      this.btnUpdateList.Location = new System.Drawing.Point(206, 13);
      this.btnUpdateList.Name = "btnUpdateList";
      this.btnUpdateList.Size = new System.Drawing.Size(80, 23);
      this.btnUpdateList.TabIndex = 24;
      this.btnUpdateList.Text = "Update List";
      this.btnUpdateList.UseVisualStyleBackColor = true;
      this.btnUpdateList.Click += new System.EventHandler(this.btnUpdateList_Click);
      // 
      // btnAddSat
      // 
      this.btnAddSat.Location = new System.Drawing.Point(10, 13);
      this.btnAddSat.Name = "btnAddSat";
      this.btnAddSat.Size = new System.Drawing.Size(60, 23);
      this.btnAddSat.TabIndex = 21;
      this.btnAddSat.Text = "&Add";
      this.btnAddSat.UseVisualStyleBackColor = true;
      this.btnAddSat.Click += new System.EventHandler(this.btnAddSat_Click);
      // 
      // btnEditSat
      // 
      this.btnEditSat.Location = new System.Drawing.Point(74, 13);
      this.btnEditSat.Name = "btnEditSat";
      this.btnEditSat.Size = new System.Drawing.Size(60, 23);
      this.btnEditSat.TabIndex = 22;
      this.btnEditSat.Text = "&Edit";
      this.btnEditSat.UseVisualStyleBackColor = true;
      this.btnEditSat.Click += new System.EventHandler(this.btnEditSat_Click);
      // 
      // btnDelSat
      // 
      this.btnDelSat.Location = new System.Drawing.Point(140, 13);
      this.btnDelSat.Name = "btnDelSat";
      this.btnDelSat.Size = new System.Drawing.Size(60, 23);
      this.btnDelSat.TabIndex = 23;
      this.btnDelSat.Text = "&Delete";
      this.btnDelSat.UseVisualStyleBackColor = true;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(3, 3);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(98, 13);
      this.mpLabel2.TabIndex = 21;
      this.mpLabel2.Text = "Available Satellites:";
      // 
      // mpListViewSatellites
      // 
      this.mpListViewSatellites.AllowDrop = true;
      this.mpListViewSatellites.AllowRowReorder = false;
      this.mpListViewSatellites.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.mpListViewSatellites.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
      this.mpListViewSatellites.IsChannelListView = false;
      this.mpListViewSatellites.LargeImageList = this.imageList1;
      this.mpListViewSatellites.Location = new System.Drawing.Point(6, 21);
      this.mpListViewSatellites.Name = "mpListViewSatellites";
      this.mpListViewSatellites.Size = new System.Drawing.Size(437, 253);
      this.mpListViewSatellites.SmallImageList = this.imageList1;
      this.mpListViewSatellites.TabIndex = 20;
      this.mpListViewSatellites.UseCompatibleStateImageBehavior = false;
      this.mpListViewSatellites.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Name";
      this.columnHeader4.Width = 73;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "LocalTransponderFile";
      this.columnHeader1.Width = 138;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Position";
      this.columnHeader2.Width = 90;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "TransponderListUrl";
      this.columnHeader3.Width = 129;
      // 
      // mpLabelChannelCount
      // 
      this.mpLabelChannelCount.AutoSize = true;
      this.mpLabelChannelCount.Location = new System.Drawing.Point(13, 14);
      this.mpLabelChannelCount.Name = "mpLabelChannelCount";
      this.mpLabelChannelCount.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannelCount.TabIndex = 2;
      // 
      // listViewStatus
      // 
      this.listViewStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5});
      this.listViewStatus.Location = new System.Drawing.Point(6, 280);
      this.listViewStatus.Name = "listViewStatus";
      this.listViewStatus.Size = new System.Drawing.Size(434, 93);
      this.listViewStatus.TabIndex = 68;
      this.listViewStatus.UseCompatibleStateImageBehavior = false;
      this.listViewStatus.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Status";
      this.columnHeader5.Width = 350;
      // 
      // Satellites
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "Satellites";
      this.Size = new System.Drawing.Size(457, 461);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
    private MPLabel mpLabelChannelCount;
		private MPLabel mpLabel2;
		private MPListView mpListViewSatellites;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private MPGroupBox mpGroupBox1;
    private MPButton btnAddSat;
    private MPButton btnEditSat;
    private MPButton btnDelSat;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private MPButton btnUpdateList;
    private System.Windows.Forms.ListView listViewStatus;
    private System.Windows.Forms.ColumnHeader columnHeader5;
  }
}
