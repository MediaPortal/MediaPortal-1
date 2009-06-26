namespace SetupTv.Sections
{
  partial class RadioChannels
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RadioChannels));
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.hdrhekje = new System.Windows.Forms.ColumnHeader();
      this.hdrProvider = new System.Windows.Forms.ColumnHeader();
      this.hdrTypes = new System.Windows.Forms.ColumnHeader();
      this.hdrDetail1 = new System.Windows.Forms.ColumnHeader();
      this.hdrDetail2 = new System.Windows.Forms.ColumnHeader();
      this.hdrDetail3 = new System.Windows.Forms.ColumnHeader();
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.addToFavoritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.deleteThisChannelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.editChannelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.renameSelectedChannelsBySIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.addSIDInFrontOfNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.renumberChannelsBySIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.mpButtonClear = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabelChannelCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonDel = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonUtp = new System.Windows.Forms.Button();
      this.buttonDown = new System.Windows.Forms.Button();
      this.mpButtonEdit = new MediaPortal.UserInterface.Controls.MPButton();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.testScrambled = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnPreview = new System.Windows.Forms.Button();
      this.mpButtonUncheckEncrypted = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnPlaylist = new System.Windows.Forms.Button();
      this.mpButtonDeleteEncrypted = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.contextMenuStrip1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpListView1.CheckBoxes = true;
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.hdrhekje,
            this.hdrProvider,
            this.hdrTypes,
            this.hdrDetail1,
            this.hdrDetail2,
            this.hdrDetail3});
      this.mpListView1.ContextMenuStrip = this.contextMenuStrip1;
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.LabelEdit = true;
      this.mpListView1.LargeImageList = this.imageList1;
      this.mpListView1.Location = new System.Drawing.Point(9, 11);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(438, 280);
      this.mpListView1.SmallImageList = this.imageList1;
      this.mpListView1.TabIndex = 0;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      this.mpListView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mpListView1_MouseDoubleClick);
      this.mpListView1.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.mpListView1_AfterLabelEdit);
      this.mpListView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mpListView1_ColumnClick);
      this.mpListView1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.mpListView1_ItemDrag);
      // 
      // hdrhekje
      // 
      this.hdrhekje.Text = "Name";
      this.hdrhekje.Width = 120;
      // 
      // hdrProvider
      // 
      this.hdrProvider.Text = "Provider";
      // 
      // hdrTypes
      // 
      this.hdrTypes.Text = "Types";
      this.hdrTypes.Width = 50;
      // 
      // hdrDetail1
      // 
      this.hdrDetail1.Text = "Details";
      this.hdrDetail1.Width = 66;
      // 
      // hdrDetail2
      // 
      this.hdrDetail2.Text = "Details";
      this.hdrDetail2.Width = 50;
      // 
      // hdrDetail3
      // 
      this.hdrDetail3.Text = "Details";
      this.hdrDetail3.Width = 50;
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToFavoritesToolStripMenuItem,
            this.deleteThisChannelToolStripMenuItem,
            this.editChannelToolStripMenuItem,
            this.toolStripMenuItem1,
            this.renameSelectedChannelsBySIDToolStripMenuItem,
            this.addSIDInFrontOfNameToolStripMenuItem,
            this.renumberChannelsBySIDToolStripMenuItem});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(258, 142);
      // 
      // addToFavoritesToolStripMenuItem
      // 
      this.addToFavoritesToolStripMenuItem.Name = "addToFavoritesToolStripMenuItem";
      this.addToFavoritesToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
      this.addToFavoritesToolStripMenuItem.Text = "Add to favorites";
      // 
      // deleteThisChannelToolStripMenuItem
      // 
      this.deleteThisChannelToolStripMenuItem.Name = "deleteThisChannelToolStripMenuItem";
      this.deleteThisChannelToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
      this.deleteThisChannelToolStripMenuItem.Text = "Delete this channel";
      this.deleteThisChannelToolStripMenuItem.Click += new System.EventHandler(this.deleteThisChannelToolStripMenuItem_Click);
      // 
      // editChannelToolStripMenuItem
      // 
      this.editChannelToolStripMenuItem.Name = "editChannelToolStripMenuItem";
      this.editChannelToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
      this.editChannelToolStripMenuItem.Text = "Edit channel";
      this.editChannelToolStripMenuItem.Click += new System.EventHandler(this.editChannelToolStripMenuItem_Click);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(254, 6);
      // 
      // renameSelectedChannelsBySIDToolStripMenuItem
      // 
      this.renameSelectedChannelsBySIDToolStripMenuItem.Name = "renameSelectedChannelsBySIDToolStripMenuItem";
      this.renameSelectedChannelsBySIDToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
      this.renameSelectedChannelsBySIDToolStripMenuItem.Text = "Rename selected channel(s) by SID";
      this.renameSelectedChannelsBySIDToolStripMenuItem.Click += new System.EventHandler(this.renameSelectedChannelsBySIDToolStripMenuItem_Click);
      // 
      // addSIDInFrontOfNameToolStripMenuItem
      // 
      this.addSIDInFrontOfNameToolStripMenuItem.Name = "addSIDInFrontOfNameToolStripMenuItem";
      this.addSIDInFrontOfNameToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
      this.addSIDInFrontOfNameToolStripMenuItem.Text = "Add SID in front of name";
      this.addSIDInFrontOfNameToolStripMenuItem.Click += new System.EventHandler(this.addSIDInFrontOfNameToolStripMenuItem_Click);
      // 
      // renumberChannelsBySIDToolStripMenuItem
      // 
      this.renumberChannelsBySIDToolStripMenuItem.Name = "renumberChannelsBySIDToolStripMenuItem";
      this.renumberChannelsBySIDToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
      this.renumberChannelsBySIDToolStripMenuItem.Text = "Renumber channels by SID";
      this.renumberChannelsBySIDToolStripMenuItem.Click += new System.EventHandler(this.renumberChannelsBySIDToolStripMenuItem_Click);
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
      // mpButtonClear
      // 
      this.mpButtonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonClear.Location = new System.Drawing.Point(257, 322);
      this.mpButtonClear.Name = "mpButtonClear";
      this.mpButtonClear.Size = new System.Drawing.Size(50, 23);
      this.mpButtonClear.TabIndex = 7;
      this.mpButtonClear.Text = "Clear";
      this.mpButtonClear.UseVisualStyleBackColor = true;
      this.mpButtonClear.Click += new System.EventHandler(this.mpButtonClear_Click);
      // 
      // mpLabelChannelCount
      // 
      this.mpLabelChannelCount.AutoSize = true;
      this.mpLabelChannelCount.Location = new System.Drawing.Point(13, 14);
      this.mpLabelChannelCount.Name = "mpLabelChannelCount";
      this.mpLabelChannelCount.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannelCount.TabIndex = 2;
      // 
      // mpButtonDel
      // 
      this.mpButtonDel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonDel.Location = new System.Drawing.Point(201, 322);
      this.mpButtonDel.Name = "mpButtonDel";
      this.mpButtonDel.Size = new System.Drawing.Size(50, 23);
      this.mpButtonDel.TabIndex = 5;
      this.mpButtonDel.Text = "&Delete";
      this.mpButtonDel.UseVisualStyleBackColor = true;
      this.mpButtonDel.Click += new System.EventHandler(this.mpButtonDel_Click);
      // 
      // buttonUtp
      // 
      this.buttonUtp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonUtp.Location = new System.Drawing.Point(9, 297);
      this.buttonUtp.Name = "buttonUtp";
      this.buttonUtp.Size = new System.Drawing.Size(45, 23);
      this.buttonUtp.TabIndex = 1;
      this.buttonUtp.Text = "Up";
      this.buttonUtp.UseVisualStyleBackColor = true;
      this.buttonUtp.Click += new System.EventHandler(this.buttonUtp_Click);
      // 
      // buttonDown
      // 
      this.buttonDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonDown.Location = new System.Drawing.Point(9, 322);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(45, 23);
      this.buttonDown.TabIndex = 2;
      this.buttonDown.Text = "Down";
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // mpButtonEdit
      // 
      this.mpButtonEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonEdit.Location = new System.Drawing.Point(125, 297);
      this.mpButtonEdit.Name = "mpButtonEdit";
      this.mpButtonEdit.Size = new System.Drawing.Size(50, 23);
      this.mpButtonEdit.TabIndex = 4;
      this.mpButtonEdit.Text = "&Edit";
      this.mpButtonEdit.UseVisualStyleBackColor = true;
      this.mpButtonEdit.Click += new System.EventHandler(this.mpButtonEdit_Click);
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.FileName = "openFileDialog1";
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
      this.tabControl1.Size = new System.Drawing.Size(465, 400);
      this.tabControl1.TabIndex = 8;
      this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.testScrambled);
      this.tabPage1.Controls.Add(this.btnPreview);
      this.tabPage1.Controls.Add(this.mpButtonUncheckEncrypted);
      this.tabPage1.Controls.Add(this.btnPlaylist);
      this.tabPage1.Controls.Add(this.mpButtonDeleteEncrypted);
      this.tabPage1.Controls.Add(this.mpButtonAdd);
      this.tabPage1.Controls.Add(this.mpListView1);
      this.tabPage1.Controls.Add(this.mpButtonClear);
      this.tabPage1.Controls.Add(this.mpButtonDel);
      this.tabPage1.Controls.Add(this.mpButtonEdit);
      this.tabPage1.Controls.Add(this.mpLabelChannelCount);
      this.tabPage1.Controls.Add(this.buttonDown);
      this.tabPage1.Controls.Add(this.buttonUtp);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(457, 374);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Channels";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // testScrambled
      // 
      this.testScrambled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.testScrambled.Location = new System.Drawing.Point(332, 348);
      this.testScrambled.Name = "testScrambled";
      this.testScrambled.Size = new System.Drawing.Size(115, 23);
      this.testScrambled.TabIndex = 11;
      this.testScrambled.Text = "Test scrambled";
      this.testScrambled.UseVisualStyleBackColor = true;
      this.testScrambled.Click += new System.EventHandler(this.testScrambled_Click);
      // 
      // btnPreview
      // 
      this.btnPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnPreview.Location = new System.Drawing.Point(69, 322);
      this.btnPreview.Name = "btnPreview";
      this.btnPreview.Size = new System.Drawing.Size(106, 23);
      this.btnPreview.TabIndex = 10;
      this.btnPreview.Text = "&Preview";
      this.btnPreview.UseVisualStyleBackColor = true;
      this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
      // 
      // mpButtonUncheckEncrypted
      // 
      this.mpButtonUncheckEncrypted.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonUncheckEncrypted.Location = new System.Drawing.Point(332, 297);
      this.mpButtonUncheckEncrypted.Name = "mpButtonUncheckEncrypted";
      this.mpButtonUncheckEncrypted.Size = new System.Drawing.Size(115, 23);
      this.mpButtonUncheckEncrypted.TabIndex = 8;
      this.mpButtonUncheckEncrypted.Text = "Uncheck encrypted";
      this.mpButtonUncheckEncrypted.UseVisualStyleBackColor = true;
      this.mpButtonUncheckEncrypted.Click += new System.EventHandler(this.mpButtonUncheckEncrypted_Click);
      // 
      // btnPlaylist
      // 
      this.btnPlaylist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnPlaylist.Location = new System.Drawing.Point(201, 297);
      this.btnPlaylist.Name = "btnPlaylist";
      this.btnPlaylist.Size = new System.Drawing.Size(106, 23);
      this.btnPlaylist.TabIndex = 6;
      this.btnPlaylist.Text = "&Import playlist";
      this.btnPlaylist.UseVisualStyleBackColor = true;
      this.btnPlaylist.Click += new System.EventHandler(this.btnPlaylist_Click);
      // 
      // mpButtonDeleteEncrypted
      // 
      this.mpButtonDeleteEncrypted.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonDeleteEncrypted.Location = new System.Drawing.Point(332, 322);
      this.mpButtonDeleteEncrypted.Name = "mpButtonDeleteEncrypted";
      this.mpButtonDeleteEncrypted.Size = new System.Drawing.Size(115, 23);
      this.mpButtonDeleteEncrypted.TabIndex = 9;
      this.mpButtonDeleteEncrypted.Text = "Delete encrypted";
      this.mpButtonDeleteEncrypted.UseVisualStyleBackColor = true;
      this.mpButtonDeleteEncrypted.Click += new System.EventHandler(this.mpButtonDeleteEncrypted_Click);
      // 
      // mpButtonAdd
      // 
      this.mpButtonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonAdd.Location = new System.Drawing.Point(69, 297);
      this.mpButtonAdd.Name = "mpButtonAdd";
      this.mpButtonAdd.Size = new System.Drawing.Size(50, 23);
      this.mpButtonAdd.TabIndex = 3;
      this.mpButtonAdd.Text = "&Add";
      this.mpButtonAdd.UseVisualStyleBackColor = true;
      this.mpButtonAdd.Click += new System.EventHandler(this.mpButtonAdd_Click);
      // 
      // RadioChannels
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "RadioChannels";
      this.Size = new System.Drawing.Size(474, 412);
      this.contextMenuStrip1.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader hdrTypes;
    private System.Windows.Forms.ColumnHeader hdrDetail1;
    private System.Windows.Forms.ColumnHeader hdrDetail2;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonClear;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannelCount;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonDel;
    private System.Windows.Forms.Button buttonUtp;
    private System.Windows.Forms.Button buttonDown;
    private System.Windows.Forms.ColumnHeader hdrhekje;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonEdit;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private System.Windows.Forms.ToolStripMenuItem addToFavoritesToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem deleteThisChannelToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem editChannelToolStripMenuItem;
    private System.Windows.Forms.ImageList imageList1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonAdd;
    private System.Windows.Forms.ColumnHeader hdrDetail3;
    private System.Windows.Forms.ColumnHeader hdrProvider;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonDeleteEncrypted;
    private System.Windows.Forms.Button btnPlaylist;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonUncheckEncrypted;
    private System.Windows.Forms.Button btnPreview;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem renameSelectedChannelsBySIDToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem addSIDInFrontOfNameToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem renumberChannelsBySIDToolStripMenuItem;
    private MediaPortal.UserInterface.Controls.MPButton testScrambled;
  }
}