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
      this.mpCheckBoxStoreOnlySelected = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.mpLabelChannelCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.mpListView2 = new MediaPortal.UserInterface.Controls.MPListView();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.linkLabelLanguageNone = new System.Windows.Forms.LinkLabel();
      this.linkLabelLanguageAll = new System.Windows.Forms.LinkLabel();
      this.linkLabelRadioNone = new System.Windows.Forms.LinkLabel();
      this.linkLabelRadioGroupedVisible = new System.Windows.Forms.LinkLabel();
      this.linkLabelRadioAllGrouped = new System.Windows.Forms.LinkLabel();
      this.linkLabelRadioAll = new System.Windows.Forms.LinkLabel();
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpCheckBoxStoreOnlySelected
      // 
      this.mpCheckBoxStoreOnlySelected.AutoSize = true;
      this.mpCheckBoxStoreOnlySelected.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxStoreOnlySelected.Location = new System.Drawing.Point(10, 6);
      this.mpCheckBoxStoreOnlySelected.Name = "mpCheckBoxStoreOnlySelected";
      this.mpCheckBoxStoreOnlySelected.Size = new System.Drawing.Size(199, 17);
      this.mpCheckBoxStoreOnlySelected.TabIndex = 20;
      this.mpCheckBoxStoreOnlySelected.Text = "Store data only for selected channels";
      this.mpCheckBoxStoreOnlySelected.UseVisualStyleBackColor = true;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "ID";
      this.columnHeader4.Width = 36;
      // 
      // mpLabelChannelCount
      // 
      this.mpLabelChannelCount.AutoSize = true;
      this.mpLabelChannelCount.Location = new System.Drawing.Point(13, 14);
      this.mpLabelChannelCount.Name = "mpLabelChannelCount";
      this.mpLabelChannelCount.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannelCount.TabIndex = 2;
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(7, 33);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(148, 13);
      this.mpLabel1.TabIndex = 2;
      this.mpLabel1.Text = "Grab EPG for these channels:";
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Language";
      this.columnHeader2.Width = 152;
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
      this.mpListView2.IsChannelListView = false;
      this.mpListView2.Location = new System.Drawing.Point(254, 51);
      this.mpListView2.Name = "mpListView2";
      this.mpListView2.Size = new System.Drawing.Size(217, 312);
      this.mpListView2.TabIndex = 3;
      this.mpListView2.UseCompatibleStateImageBehavior = false;
      this.mpListView2.View = System.Windows.Forms.View.Details;
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.ItemSize = new System.Drawing.Size(87, 18);
      this.tabControl1.Location = new System.Drawing.Point(3, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(485, 431);
      this.tabControl1.TabIndex = 11;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.linkLabelLanguageNone);
      this.tabPage1.Controls.Add(this.linkLabelLanguageAll);
      this.tabPage1.Controls.Add(this.linkLabelRadioNone);
      this.tabPage1.Controls.Add(this.linkLabelRadioGroupedVisible);
      this.tabPage1.Controls.Add(this.linkLabelRadioAllGrouped);
      this.tabPage1.Controls.Add(this.linkLabelRadioAll);
      this.tabPage1.Controls.Add(this.mpCheckBoxStoreOnlySelected);
      this.tabPage1.Controls.Add(this.mpLabelChannelCount);
      this.tabPage1.Controls.Add(this.mpLabel1);
      this.tabPage1.Controls.Add(this.mpListView1);
      this.tabPage1.Controls.Add(this.mpListView2);
      this.tabPage1.Controls.Add(this.mpLabel2);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(477, 405);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Radio EPG Grabber";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // linkLabelLanguageNone
      // 
      this.linkLabelLanguageNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabelLanguageNone.AutoSize = true;
      this.linkLabelLanguageNone.Location = new System.Drawing.Point(280, 376);
      this.linkLabelLanguageNone.Name = "linkLabelLanguageNone";
      this.linkLabelLanguageNone.Size = new System.Drawing.Size(33, 13);
      this.linkLabelLanguageNone.TabIndex = 30;
      this.linkLabelLanguageNone.TabStop = true;
      this.linkLabelLanguageNone.Text = "None";
      this.linkLabelLanguageNone.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelLanguageNone_LinkClicked);
      // 
      // linkLabelLanguageAll
      // 
      this.linkLabelLanguageAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabelLanguageAll.AutoSize = true;
      this.linkLabelLanguageAll.Location = new System.Drawing.Point(256, 376);
      this.linkLabelLanguageAll.Name = "linkLabelLanguageAll";
      this.linkLabelLanguageAll.Size = new System.Drawing.Size(18, 13);
      this.linkLabelLanguageAll.TabIndex = 29;
      this.linkLabelLanguageAll.TabStop = true;
      this.linkLabelLanguageAll.Text = "All";
      this.linkLabelLanguageAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelLanguageAll_LinkClicked);
      // 
      // linkLabelRadioNone
      // 
      this.linkLabelRadioNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabelRadioNone.AutoSize = true;
      this.linkLabelRadioNone.Location = new System.Drawing.Point(192, 376);
      this.linkLabelRadioNone.Name = "linkLabelRadioNone";
      this.linkLabelRadioNone.Size = new System.Drawing.Size(33, 13);
      this.linkLabelRadioNone.TabIndex = 28;
      this.linkLabelRadioNone.TabStop = true;
      this.linkLabelRadioNone.Text = "None";
      this.linkLabelRadioNone.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelRadioNone_LinkClicked);
      // 
      // linkLabelRadioGroupedVisible
      // 
      this.linkLabelRadioGroupedVisible.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabelRadioGroupedVisible.AutoSize = true;
      this.linkLabelRadioGroupedVisible.Location = new System.Drawing.Point(96, 376);
      this.linkLabelRadioGroupedVisible.Name = "linkLabelRadioGroupedVisible";
      this.linkLabelRadioGroupedVisible.Size = new System.Drawing.Size(90, 13);
      this.linkLabelRadioGroupedVisible.TabIndex = 27;
      this.linkLabelRadioGroupedVisible.TabStop = true;
      this.linkLabelRadioGroupedVisible.Text = "Grouped && Visible";
      this.linkLabelRadioGroupedVisible.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelRadioGroupedVisible_LinkClicked);
      // 
      // linkLabelRadioAllGrouped
      // 
      this.linkLabelRadioAllGrouped.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabelRadioAllGrouped.AutoSize = true;
      this.linkLabelRadioAllGrouped.Location = new System.Drawing.Point(32, 376);
      this.linkLabelRadioAllGrouped.Name = "linkLabelRadioAllGrouped";
      this.linkLabelRadioAllGrouped.Size = new System.Drawing.Size(62, 13);
      this.linkLabelRadioAllGrouped.TabIndex = 26;
      this.linkLabelRadioAllGrouped.TabStop = true;
      this.linkLabelRadioAllGrouped.Text = "All Grouped";
      this.linkLabelRadioAllGrouped.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelRadioAllGrouped_LinkClicked);
      // 
      // linkLabelRadioAll
      // 
      this.linkLabelRadioAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabelRadioAll.AutoSize = true;
      this.linkLabelRadioAll.Location = new System.Drawing.Point(8, 376);
      this.linkLabelRadioAll.Name = "linkLabelRadioAll";
      this.linkLabelRadioAll.Size = new System.Drawing.Size(18, 13);
      this.linkLabelRadioAll.TabIndex = 25;
      this.linkLabelRadioAll.TabStop = true;
      this.linkLabelRadioAll.Text = "All";
      this.linkLabelRadioAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelRadioAll_LinkClicked);
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
      this.mpListView1.IsChannelListView = false;
      this.mpListView1.LargeImageList = this.imageList1;
      this.mpListView1.Location = new System.Drawing.Point(10, 51);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(214, 312);
      this.mpListView1.SmallImageList = this.imageList1;
      this.mpListView1.TabIndex = 1;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      this.mpListView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mpListView1_ColumnClick);
      this.mpListView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.mpListView1_ItemChecked);
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
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(251, 33);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(187, 13);
      this.mpLabel2.TabIndex = 4;
      this.mpLabel2.Text = "Grab EPG for the following languages:";
      // 
      // RadioEpgGrabber
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "RadioEpgGrabber";
      this.Size = new System.Drawing.Size(491, 437);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBoxStoreOnlySelected;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannelCount;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPListView mpListView2;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ImageList imageList1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private System.Windows.Forms.LinkLabel linkLabelLanguageNone;
    private System.Windows.Forms.LinkLabel linkLabelLanguageAll;
    private System.Windows.Forms.LinkLabel linkLabelRadioNone;
    private System.Windows.Forms.LinkLabel linkLabelRadioGroupedVisible;
    private System.Windows.Forms.LinkLabel linkLabelRadioAllGrouped;
    private System.Windows.Forms.LinkLabel linkLabelRadioAll;
  }
}