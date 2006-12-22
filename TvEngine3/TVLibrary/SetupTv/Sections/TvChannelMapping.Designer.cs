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
      this.mpComboBoxCard = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListViewChannels = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListViewMapped = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.mpButtonMap = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonUnmap = new MediaPortal.UserInterface.Controls.MPButton();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // mpComboBoxCard
      // 
      this.mpComboBoxCard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxCard.FormattingEnabled = true;
      this.mpComboBoxCard.Location = new System.Drawing.Point(96, 16);
      this.mpComboBoxCard.Name = "mpComboBoxCard";
      this.mpComboBoxCard.Size = new System.Drawing.Size(249, 21);
      this.mpComboBoxCard.TabIndex = 0;
      this.mpComboBoxCard.SelectedIndexChanged += new System.EventHandler(this.mpComboBoxCard_SelectedIndexChanged);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(58, 19);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(32, 13);
      this.mpLabel1.TabIndex = 1;
      this.mpLabel1.Text = "Card:";
      // 
      // mpListViewChannels
      // 
      this.mpListViewChannels.AllowDrop = true;
      this.mpListViewChannels.AllowRowReorder = false;
      this.mpListViewChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
      this.mpListViewChannels.LargeImageList = this.imageList1;
      this.mpListViewChannels.Location = new System.Drawing.Point(19, 74);
      this.mpListViewChannels.Name = "mpListViewChannels";
      this.mpListViewChannels.Size = new System.Drawing.Size(193, 297);
      this.mpListViewChannels.SmallImageList = this.imageList1;
      this.mpListViewChannels.TabIndex = 2;
      this.mpListViewChannels.UseCompatibleStateImageBehavior = false;
      this.mpListViewChannels.View = System.Windows.Forms.View.Details;
      this.mpListViewChannels.SelectedIndexChanged += new System.EventHandler(this.mpListViewChannels_SelectedIndexChanged);
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Name";
      this.columnHeader4.Width = 180;
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
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(16, 55);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(54, 13);
      this.mpLabel2.TabIndex = 3;
      this.mpLabel2.Text = "Channels:";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(258, 55);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(131, 13);
      this.mpLabel3.TabIndex = 5;
      this.mpLabel3.Text = "Channels mapped to card:";
      // 
      // mpListViewMapped
      // 
      this.mpListViewMapped.AllowDrop = true;
      this.mpListViewMapped.AllowRowReorder = false;
      this.mpListViewMapped.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
      this.mpListViewMapped.LargeImageList = this.imageList1;
      this.mpListViewMapped.Location = new System.Drawing.Point(261, 74);
      this.mpListViewMapped.Name = "mpListViewMapped";
      this.mpListViewMapped.Size = new System.Drawing.Size(193, 297);
      this.mpListViewMapped.SmallImageList = this.imageList1;
      this.mpListViewMapped.TabIndex = 4;
      this.mpListViewMapped.UseCompatibleStateImageBehavior = false;
      this.mpListViewMapped.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Name";
      this.columnHeader2.Width = 180;
      // 
      // mpButtonMap
      // 
      this.mpButtonMap.Location = new System.Drawing.Point(228, 114);
      this.mpButtonMap.Name = "mpButtonMap";
      this.mpButtonMap.Size = new System.Drawing.Size(27, 23);
      this.mpButtonMap.TabIndex = 6;
      this.mpButtonMap.Text = ">>";
      this.mpButtonMap.UseVisualStyleBackColor = true;
      this.mpButtonMap.Click += new System.EventHandler(this.mpButtonMap_Click);
      // 
      // mpButtonUnmap
      // 
      this.mpButtonUnmap.Location = new System.Drawing.Point(228, 143);
      this.mpButtonUnmap.Name = "mpButtonUnmap";
      this.mpButtonUnmap.Size = new System.Drawing.Size(27, 23);
      this.mpButtonUnmap.TabIndex = 7;
      this.mpButtonUnmap.Text = "<<";
      this.mpButtonUnmap.UseVisualStyleBackColor = true;
      this.mpButtonUnmap.Click += new System.EventHandler(this.mpButtonUnmap_Click);
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(19, 16);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(33, 23);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pictureBox1.TabIndex = 17;
      this.pictureBox1.TabStop = false;
      // 
      // TvChannelMapping
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.mpButtonUnmap);
      this.Controls.Add(this.mpButtonMap);
      this.Controls.Add(this.mpLabel3);
      this.Controls.Add(this.mpListViewMapped);
      this.Controls.Add(this.mpLabel2);
      this.Controls.Add(this.mpListViewChannels);
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.mpComboBoxCard);
      this.Name = "TvChannelMapping";
      this.Size = new System.Drawing.Size(467, 388);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBoxCard;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPListView mpListViewChannels;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPListView mpListViewMapped;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonMap;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonUnmap;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.PictureBox pictureBox1;
  }
}