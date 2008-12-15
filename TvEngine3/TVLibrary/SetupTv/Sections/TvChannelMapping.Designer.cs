namespace SetupTv.Sections
{
  partial class TvChannelMapping
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TvChannelMapping));
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpCheckBoxMapForEpgOnly = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.mpButtonUnmap = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonMap = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListViewMapped = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListViewChannels = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpComboBoxCard = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabelChannelCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
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
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Location = new System.Drawing.Point(3, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(454, 392);
      this.tabControl1.TabIndex = 18;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpCheckBoxMapForEpgOnly);
      this.tabPage1.Controls.Add(this.pictureBox1);
      this.tabPage1.Controls.Add(this.mpButtonUnmap);
      this.tabPage1.Controls.Add(this.mpButtonMap);
      this.tabPage1.Controls.Add(this.mpLabel3);
      this.tabPage1.Controls.Add(this.mpListViewMapped);
      this.tabPage1.Controls.Add(this.mpLabel2);
      this.tabPage1.Controls.Add(this.mpListViewChannels);
      this.tabPage1.Controls.Add(this.mpLabel1);
      this.tabPage1.Controls.Add(this.mpComboBoxCard);
      this.tabPage1.Controls.Add(this.mpLabelChannelCount);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(446, 366);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "TV Mapping";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpCheckBoxMapForEpgOnly
      // 
      this.mpCheckBoxMapForEpgOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpCheckBoxMapForEpgOnly.AutoSize = true;
      this.mpCheckBoxMapForEpgOnly.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxMapForEpgOnly.Location = new System.Drawing.Point(6, 343);
      this.mpCheckBoxMapForEpgOnly.Name = "mpCheckBoxMapForEpgOnly";
      this.mpCheckBoxMapForEpgOnly.Size = new System.Drawing.Size(355, 17);
      this.mpCheckBoxMapForEpgOnly.TabIndex = 27;
      this.mpCheckBoxMapForEpgOnly.Text = "Perform mapping for EPG grabbing only (you can also use doubleclick)";
      this.mpCheckBoxMapForEpgOnly.UseVisualStyleBackColor = true;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(3, 2);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(33, 23);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pictureBox1.TabIndex = 26;
      this.pictureBox1.TabStop = false;
      // 
      // mpButtonUnmap
      // 
      this.mpButtonUnmap.Location = new System.Drawing.Point(211, 103);
      this.mpButtonUnmap.Name = "mpButtonUnmap";
      this.mpButtonUnmap.Size = new System.Drawing.Size(27, 23);
      this.mpButtonUnmap.TabIndex = 25;
      this.mpButtonUnmap.Text = "<<";
      this.mpButtonUnmap.UseVisualStyleBackColor = true;
      this.mpButtonUnmap.Click += new System.EventHandler(this.mpButtonUnmap_Click);
      // 
      // mpButtonMap
      // 
      this.mpButtonMap.Location = new System.Drawing.Point(211, 74);
      this.mpButtonMap.Name = "mpButtonMap";
      this.mpButtonMap.Size = new System.Drawing.Size(27, 23);
      this.mpButtonMap.TabIndex = 24;
      this.mpButtonMap.Text = ">>";
      this.mpButtonMap.UseVisualStyleBackColor = true;
      this.mpButtonMap.Click += new System.EventHandler(this.mpButtonMap_Click);
      // 
      // mpLabel3
      // 
      this.mpLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(250, 32);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(131, 13);
      this.mpLabel3.TabIndex = 23;
      this.mpLabel3.Text = "Channels mapped to card:";
      // 
      // mpListViewMapped
      // 
      this.mpListViewMapped.AllowDrop = true;
      this.mpListViewMapped.AllowRowReorder = false;
      this.mpListViewMapped.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpListViewMapped.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
      this.mpListViewMapped.LargeImageList = this.imageList1;
      this.mpListViewMapped.Location = new System.Drawing.Point(246, 50);
      this.mpListViewMapped.Name = "mpListViewMapped";
      this.mpListViewMapped.Size = new System.Drawing.Size(193, 290);
      this.mpListViewMapped.SmallImageList = this.imageList1;
      this.mpListViewMapped.TabIndex = 22;
      this.mpListViewMapped.UseCompatibleStateImageBehavior = false;
      this.mpListViewMapped.View = System.Windows.Forms.View.Details;
      this.mpListViewMapped.DoubleClick += new System.EventHandler(this.mpListViewMapped_DoubleClick);
      this.mpListViewMapped.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mpListViewMapped_ColumnClick);
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Name";
      this.columnHeader2.Width = 180;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(0, 32);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(99, 13);
      this.mpLabel2.TabIndex = 21;
      this.mpLabel2.Text = "Available channels:";
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
      this.mpListViewChannels.Location = new System.Drawing.Point(3, 50);
      this.mpListViewChannels.Name = "mpListViewChannels";
      this.mpListViewChannels.Size = new System.Drawing.Size(193, 290);
      this.mpListViewChannels.SmallImageList = this.imageList1;
      this.mpListViewChannels.TabIndex = 20;
      this.mpListViewChannels.UseCompatibleStateImageBehavior = false;
      this.mpListViewChannels.View = System.Windows.Forms.View.Details;
      this.mpListViewChannels.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mpListViewChannels_ColumnClick);
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Name";
      this.columnHeader4.Width = 180;
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(42, 9);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(32, 13);
      this.mpLabel1.TabIndex = 19;
      this.mpLabel1.Text = "Card:";
      // 
      // mpComboBoxCard
      // 
      this.mpComboBoxCard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpComboBoxCard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxCard.FormattingEnabled = true;
      this.mpComboBoxCard.Location = new System.Drawing.Point(80, 6);
      this.mpComboBoxCard.Name = "mpComboBoxCard";
      this.mpComboBoxCard.Size = new System.Drawing.Size(360, 21);
      this.mpComboBoxCard.TabIndex = 18;
      this.mpComboBoxCard.SelectedIndexChanged += new System.EventHandler(this.mpComboBoxCard_SelectedIndexChanged);
      // 
      // mpLabelChannelCount
      // 
      this.mpLabelChannelCount.AutoSize = true;
      this.mpLabelChannelCount.Location = new System.Drawing.Point(13, 14);
      this.mpLabelChannelCount.Name = "mpLabelChannelCount";
      this.mpLabelChannelCount.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannelCount.TabIndex = 2;
      // 
      // TvChannelMapping
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "TvChannelMapping";
      this.Size = new System.Drawing.Size(457, 395);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannelCount;
		private System.Windows.Forms.PictureBox pictureBox1;
		private MediaPortal.UserInterface.Controls.MPButton mpButtonUnmap;
		private MediaPortal.UserInterface.Controls.MPButton mpButtonMap;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
		private MediaPortal.UserInterface.Controls.MPListView mpListViewMapped;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
		private MediaPortal.UserInterface.Controls.MPListView mpListViewChannels;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
      private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxCard;
      private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxMapForEpgOnly;
  }
}
