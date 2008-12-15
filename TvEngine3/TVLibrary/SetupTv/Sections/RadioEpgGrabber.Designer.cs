namespace SetupTv.Sections
{
  partial class RadioEpgGrabber
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RadioEpgGrabber));
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListView2 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonAll = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonNone = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonClearChannels = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonAllChannels = new MediaPortal.UserInterface.Controls.MPButton();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpButtonAllGrouped = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabelChannelCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.mpListView1.CheckBoxes = true;
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3});
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.LargeImageList = this.imageList1;
      this.mpListView1.Location = new System.Drawing.Point(6, 29);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(208, 297);
      this.mpListView1.SmallImageList = this.imageList1;
      this.mpListView1.TabIndex = 1;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      this.mpListView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.mpListView1_ItemChecked);
      this.mpListView1.SelectedIndexChanged += new System.EventHandler(this.mpListView1_SelectedIndexChanged);
      this.mpListView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mpListView1_ColumnClick);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Name";
      this.columnHeader1.Width = 125;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Types";
      this.columnHeader3.Width = 61;
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
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(6, 13);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(148, 13);
      this.mpLabel1.TabIndex = 2;
      this.mpLabel1.Text = "Grab EPG for these channels:";
      // 
      // mpListView2
      // 
      this.mpListView2.AllowDrop = true;
      this.mpListView2.AllowRowReorder = true;
      this.mpListView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpListView2.CheckBoxes = true;
      this.mpListView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader4});
      this.mpListView2.Location = new System.Drawing.Point(236, 29);
      this.mpListView2.Name = "mpListView2";
      this.mpListView2.Size = new System.Drawing.Size(205, 297);
      this.mpListView2.TabIndex = 3;
      this.mpListView2.UseCompatibleStateImageBehavior = false;
      this.mpListView2.View = System.Windows.Forms.View.Details;
      this.mpListView2.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.mpListView2_ItemChecked);
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Language";
      this.columnHeader2.Width = 165;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "ID";
      this.columnHeader4.Width = 39;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(233, 13);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(187, 13);
      this.mpLabel2.TabIndex = 4;
      this.mpLabel2.Text = "Grab EPG for the following languages:";
      this.mpLabel2.Click += new System.EventHandler(this.mpLabel2_Click);
      // 
      // mpButtonAll
      // 
      this.mpButtonAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonAll.Location = new System.Drawing.Point(285, 341);
      this.mpButtonAll.Name = "mpButtonAll";
      this.mpButtonAll.Size = new System.Drawing.Size(75, 23);
      this.mpButtonAll.TabIndex = 5;
      this.mpButtonAll.Text = "All";
      this.mpButtonAll.UseVisualStyleBackColor = true;
      this.mpButtonAll.Click += new System.EventHandler(this.mpButtonAll_Click);
      // 
      // mpButtonNone
      // 
      this.mpButtonNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonNone.Location = new System.Drawing.Point(366, 341);
      this.mpButtonNone.Name = "mpButtonNone";
      this.mpButtonNone.Size = new System.Drawing.Size(75, 23);
      this.mpButtonNone.TabIndex = 6;
      this.mpButtonNone.Text = "None";
      this.mpButtonNone.UseVisualStyleBackColor = true;
      this.mpButtonNone.Click += new System.EventHandler(this.mpButtonNone_Click);
      // 
      // mpButtonClearChannels
      // 
      this.mpButtonClearChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonClearChannels.Location = new System.Drawing.Point(161, 341);
      this.mpButtonClearChannels.Name = "mpButtonClearChannels";
      this.mpButtonClearChannels.Size = new System.Drawing.Size(53, 23);
      this.mpButtonClearChannels.TabIndex = 8;
      this.mpButtonClearChannels.Text = "None";
      this.mpButtonClearChannels.UseVisualStyleBackColor = true;
      this.mpButtonClearChannels.Click += new System.EventHandler(this.mpButtonClearChannels_Click);
      // 
      // mpButtonAllChannels
      // 
      this.mpButtonAllChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonAllChannels.Location = new System.Drawing.Point(6, 341);
      this.mpButtonAllChannels.Name = "mpButtonAllChannels";
      this.mpButtonAllChannels.Size = new System.Drawing.Size(56, 23);
      this.mpButtonAllChannels.TabIndex = 7;
      this.mpButtonAllChannels.Text = "All";
      this.mpButtonAllChannels.UseVisualStyleBackColor = true;
      this.mpButtonAllChannels.Click += new System.EventHandler(this.mpButtonAllChannels_Click);
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Location = new System.Drawing.Point(1, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(460, 398);
      this.tabControl1.TabIndex = 10;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpButtonAllGrouped);
      this.tabPage1.Controls.Add(this.mpLabelChannelCount);
      this.tabPage1.Controls.Add(this.mpButtonClearChannels);
      this.tabPage1.Controls.Add(this.mpLabel1);
      this.tabPage1.Controls.Add(this.mpButtonAllChannels);
      this.tabPage1.Controls.Add(this.mpListView1);
      this.tabPage1.Controls.Add(this.mpButtonNone);
      this.tabPage1.Controls.Add(this.mpListView2);
      this.tabPage1.Controls.Add(this.mpButtonAll);
      this.tabPage1.Controls.Add(this.mpLabel2);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(452, 372);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Radio EPG grabber";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpButtonAllGrouped
      // 
      this.mpButtonAllGrouped.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonAllGrouped.Location = new System.Drawing.Point(75, 341);
      this.mpButtonAllGrouped.Name = "mpButtonAllGrouped";
      this.mpButtonAllGrouped.Size = new System.Drawing.Size(75, 23);
      this.mpButtonAllGrouped.TabIndex = 19;
      this.mpButtonAllGrouped.Text = "All grouped";
      this.mpButtonAllGrouped.UseVisualStyleBackColor = true;
      this.mpButtonAllGrouped.Click += new System.EventHandler(this.mpButtonAllGrouped_Click);
      // 
      // mpLabelChannelCount
      // 
      this.mpLabelChannelCount.AutoSize = true;
      this.mpLabelChannelCount.Location = new System.Drawing.Point(13, 14);
      this.mpLabelChannelCount.Name = "mpLabelChannelCount";
      this.mpLabelChannelCount.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannelCount.TabIndex = 2;
      // 
      // RadioEpgGrabber
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "RadioEpgGrabber";
      this.Size = new System.Drawing.Size(463, 404);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPListView mpListView2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonAll;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonNone;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonClearChannels;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonAllChannels;
    private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannelCount;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonAllGrouped;
    private System.Windows.Forms.ColumnHeader columnHeader4;

  }
}