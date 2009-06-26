namespace SetupTv.Sections
{
  partial class RadioChannelMapping
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RadioChannelMapping));
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.mpButtonUnmap = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonMap = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListViewMapped = new MediaPortal.UserInterface.Controls.MPListView();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListViewChannels = new MediaPortal.UserInterface.Controls.MPListView();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxCard = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpLabelChannelCount = new MediaPortal.UserInterface.Controls.MPLabel();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SuspendLayout();
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Name";
      this.columnHeader2.Width = 180;
      // 
      // mpButtonUnmap
      // 
      this.mpButtonUnmap.Location = new System.Drawing.Point(215, 130);
      this.mpButtonUnmap.Name = "mpButtonUnmap";
      this.mpButtonUnmap.Size = new System.Drawing.Size(27, 23);
      this.mpButtonUnmap.TabIndex = 4;
      this.mpButtonUnmap.Text = "<<";
      this.mpButtonUnmap.UseVisualStyleBackColor = true;
      this.mpButtonUnmap.Click += new System.EventHandler(this.mpButtonUnmap_Click_1);
      // 
      // mpButtonMap
      // 
      this.mpButtonMap.Location = new System.Drawing.Point(215, 101);
      this.mpButtonMap.Name = "mpButtonMap";
      this.mpButtonMap.Size = new System.Drawing.Size(27, 23);
      this.mpButtonMap.TabIndex = 3;
      this.mpButtonMap.Text = ">>";
      this.mpButtonMap.UseVisualStyleBackColor = true;
      this.mpButtonMap.Click += new System.EventHandler(this.mpButtonMap_Click_1);
      // 
      // mpLabel3
      // 
      this.mpLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(245, 42);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(154, 13);
      this.mpLabel3.TabIndex = 13;
      this.mpLabel3.Text = "Radio stations mapped to card:";
      // 
      // mpListViewMapped
      // 
      this.mpListViewMapped.AllowDrop = true;
      this.mpListViewMapped.AllowRowReorder = true;
      this.mpListViewMapped.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpListViewMapped.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
      this.mpListViewMapped.LargeImageList = this.imageList1;
      this.mpListViewMapped.Location = new System.Drawing.Point(248, 61);
      this.mpListViewMapped.Name = "mpListViewMapped";
      this.mpListViewMapped.Size = new System.Drawing.Size(196, 292);
      this.mpListViewMapped.SmallImageList = this.imageList1;
      this.mpListViewMapped.TabIndex = 2;
      this.mpListViewMapped.UseCompatibleStateImageBehavior = false;
      this.mpListViewMapped.View = System.Windows.Forms.View.Details;
      this.mpListViewMapped.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mpListViewMapped_ColumnClick);
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
      // columnHeader4
      // 
      this.columnHeader4.Text = "Name";
      this.columnHeader4.Width = 180;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(3, 42);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(118, 13);
      this.mpLabel2.TabIndex = 11;
      this.mpLabel2.Text = "Available radio stations:";
      // 
      // mpListViewChannels
      // 
      this.mpListViewChannels.AllowDrop = true;
      this.mpListViewChannels.AllowRowReorder = true;
      this.mpListViewChannels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.mpListViewChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
      this.mpListViewChannels.LargeImageList = this.imageList1;
      this.mpListViewChannels.Location = new System.Drawing.Point(6, 61);
      this.mpListViewChannels.Name = "mpListViewChannels";
      this.mpListViewChannels.Size = new System.Drawing.Size(193, 292);
      this.mpListViewChannels.SmallImageList = this.imageList1;
      this.mpListViewChannels.TabIndex = 1;
      this.mpListViewChannels.UseCompatibleStateImageBehavior = false;
      this.mpListViewChannels.View = System.Windows.Forms.View.Details;
      this.mpListViewChannels.SelectedIndexChanged += new System.EventHandler(this.mpListViewChannels_SelectedIndexChanged);
      this.mpListViewChannels.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mpListViewChannels_ColumnClick);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(44, 7);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(32, 13);
      this.mpLabel1.TabIndex = 9;
      this.mpLabel1.Text = "Card:";
      // 
      // mpComboBoxCard
      // 
      this.mpComboBoxCard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpComboBoxCard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxCard.FormattingEnabled = true;
      this.mpComboBoxCard.Location = new System.Drawing.Point(79, 4);
      this.mpComboBoxCard.Name = "mpComboBoxCard";
      this.mpComboBoxCard.Size = new System.Drawing.Size(365, 21);
      this.mpComboBoxCard.TabIndex = 0;
      this.mpComboBoxCard.SelectedIndexChanged += new System.EventHandler(this.mpComboBoxCard_SelectedIndexChanged_1);
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(6, 4);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(33, 23);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pictureBox1.TabIndex = 16;
      this.pictureBox1.TabStop = false;
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
      this.tabControl1.Size = new System.Drawing.Size(464, 385);
      this.tabControl1.TabIndex = 17;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpLabelChannelCount);
      this.tabPage1.Controls.Add(this.pictureBox1);
      this.tabPage1.Controls.Add(this.mpButtonUnmap);
      this.tabPage1.Controls.Add(this.mpComboBoxCard);
      this.tabPage1.Controls.Add(this.mpButtonMap);
      this.tabPage1.Controls.Add(this.mpLabel1);
      this.tabPage1.Controls.Add(this.mpLabel3);
      this.tabPage1.Controls.Add(this.mpListViewChannels);
      this.tabPage1.Controls.Add(this.mpListViewMapped);
      this.tabPage1.Controls.Add(this.mpLabel2);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(456, 359);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Radio Mapping";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpLabelChannelCount
      // 
      this.mpLabelChannelCount.AutoSize = true;
      this.mpLabelChannelCount.Location = new System.Drawing.Point(13, 14);
      this.mpLabelChannelCount.Name = "mpLabelChannelCount";
      this.mpLabelChannelCount.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannelCount.TabIndex = 2;
      // 
      // RadioChannelMapping
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "RadioChannelMapping";
      this.Size = new System.Drawing.Size(467, 388);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonUnmap;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonMap;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPListView mpListViewMapped;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPListView mpListViewChannels;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxCard;
    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannelCount;
  }
}