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
      this.hdrDetail2 = new System.Windows.Forms.ColumnHeader();
      this.hdrDetail3 = new System.Windows.Forms.ColumnHeader();
      this.hdrDetail1 = new System.Windows.Forms.ColumnHeader();
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.hdrHekje = new System.Windows.Forms.ColumnHeader();
      this.hdrProvider = new System.Windows.Forms.ColumnHeader();
      this.hdrTypes = new System.Windows.Forms.ColumnHeader();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.mpButtonAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonDel = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonEdit = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonDown = new System.Windows.Forms.Button();
      this.buttonUtp = new System.Windows.Forms.Button();
      this.mpButtonDeleteEncrypted = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonClear = new MediaPortal.UserInterface.Controls.MPButton();
      this.renameMarkedChannelsBySIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.addSIDInFrontOfNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.btnAddFromPLS = new MediaPortal.UserInterface.Controls.MPButton();
      this.contextMenuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // hdrDetail2
      // 
      this.hdrDetail2.Text = "Details";
      // 
      // hdrDetail3
      // 
      this.hdrDetail3.Text = "Details";
      // 
      // hdrDetail1
      // 
      this.hdrDetail1.Text = "Details";
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.hdrHekje,
            this.hdrProvider,
            this.hdrTypes,
            this.hdrDetail1,
            this.hdrDetail2,
            this.hdrDetail3});
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.LargeImageList = this.imageList1;
      this.mpListView1.Location = new System.Drawing.Point(14, 30);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(438, 302);
      this.mpListView1.SmallImageList = this.imageList1;
      this.mpListView1.TabIndex = 1;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      this.mpListView1.DoubleClick += new System.EventHandler(this.mpButtonEdit_Click);
      this.mpListView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mpListView1_ColumnClick);
      this.mpListView1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.mpListView1_ItemDrag);
      // 
      // hdrHekje
      // 
      this.hdrHekje.Text = "Name";
      this.hdrHekje.Width = 120;
      // 
      // hdrProvider
      // 
      this.hdrProvider.Text = "Provider";
      // 
      // hdrTypes
      // 
      this.hdrTypes.Text = "Types";
      this.hdrTypes.Width = 90;
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
      // mpButtonAdd
      // 
      this.mpButtonAdd.Location = new System.Drawing.Point(127, 338);
      this.mpButtonAdd.Name = "mpButtonAdd";
      this.mpButtonAdd.Size = new System.Drawing.Size(54, 23);
      this.mpButtonAdd.TabIndex = 14;
      this.mpButtonAdd.Text = "Add";
      this.mpButtonAdd.UseVisualStyleBackColor = true;
      this.mpButtonAdd.Click += new System.EventHandler(this.mpButtonAdd_Click);
      // 
      // mpButtonDel
      // 
      this.mpButtonDel.Location = new System.Drawing.Point(67, 362);
      this.mpButtonDel.Name = "mpButtonDel";
      this.mpButtonDel.Size = new System.Drawing.Size(54, 23);
      this.mpButtonDel.TabIndex = 10;
      this.mpButtonDel.Text = "Delete";
      this.mpButtonDel.UseVisualStyleBackColor = true;
      this.mpButtonDel.Click += new System.EventHandler(this.mpButtonDel_Click);
      // 
      // mpButtonEdit
      // 
      this.mpButtonEdit.Location = new System.Drawing.Point(67, 338);
      this.mpButtonEdit.Name = "mpButtonEdit";
      this.mpButtonEdit.Size = new System.Drawing.Size(54, 23);
      this.mpButtonEdit.TabIndex = 13;
      this.mpButtonEdit.Text = "Edit";
      this.mpButtonEdit.UseVisualStyleBackColor = true;
      this.mpButtonEdit.Click += new System.EventHandler(this.mpButtonEdit_Click);
      // 
      // buttonDown
      // 
      this.buttonDown.Location = new System.Drawing.Point(17, 362);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(44, 23);
      this.buttonDown.TabIndex = 12;
      this.buttonDown.Text = "Down";
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // buttonUtp
      // 
      this.buttonUtp.Location = new System.Drawing.Point(17, 338);
      this.buttonUtp.Name = "buttonUtp";
      this.buttonUtp.Size = new System.Drawing.Size(44, 23);
      this.buttonUtp.TabIndex = 11;
      this.buttonUtp.Text = "Up";
      this.buttonUtp.UseVisualStyleBackColor = true;
      this.buttonUtp.Click += new System.EventHandler(this.buttonUtp_Click);
      // 
      // mpButtonDeleteEncrypted
      // 
      this.mpButtonDeleteEncrypted.Location = new System.Drawing.Point(210, 362);
      this.mpButtonDeleteEncrypted.Name = "mpButtonDeleteEncrypted";
      this.mpButtonDeleteEncrypted.Size = new System.Drawing.Size(103, 23);
      this.mpButtonDeleteEncrypted.TabIndex = 15;
      this.mpButtonDeleteEncrypted.Text = "Delete Scrambled";
      this.mpButtonDeleteEncrypted.UseVisualStyleBackColor = true;
      this.mpButtonDeleteEncrypted.Click += new System.EventHandler(this.mpButtonDeleteEncrypted_Click);
      // 
      // mpButtonClear
      // 
      this.mpButtonClear.Location = new System.Drawing.Point(210, 338);
      this.mpButtonClear.Name = "mpButtonClear";
      this.mpButtonClear.Size = new System.Drawing.Size(55, 23);
      this.mpButtonClear.TabIndex = 16;
      this.mpButtonClear.Text = "Clear";
      this.mpButtonClear.UseVisualStyleBackColor = true;
      this.mpButtonClear.Click += new System.EventHandler(this.mpButtonClear_Click);
      // 
      // renameMarkedChannelsBySIDToolStripMenuItem
      // 
      this.renameMarkedChannelsBySIDToolStripMenuItem.Name = "renameMarkedChannelsBySIDToolStripMenuItem";
      this.renameMarkedChannelsBySIDToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
      this.renameMarkedChannelsBySIDToolStripMenuItem.Text = "Rename selected channel(s) by SID";
      this.renameMarkedChannelsBySIDToolStripMenuItem.Click += new System.EventHandler(this.renameMarkedChannelsBySIDToolStripMenuItem_Click);
      // 
      // addSIDInFrontOfNameToolStripMenuItem
      // 
      this.addSIDInFrontOfNameToolStripMenuItem.Name = "addSIDInFrontOfNameToolStripMenuItem";
      this.addSIDInFrontOfNameToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
      this.addSIDInFrontOfNameToolStripMenuItem.Text = "Add SID in front of name";
      this.addSIDInFrontOfNameToolStripMenuItem.Click += new System.EventHandler(this.addSIDInFrontOfNameToolStripMenuItem_Click);
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameMarkedChannelsBySIDToolStripMenuItem,
            this.addSIDInFrontOfNameToolStripMenuItem});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(256, 48);
      // 
      // btnAddFromPLS
      // 
      this.btnAddFromPLS.Location = new System.Drawing.Point(127, 362);
      this.btnAddFromPLS.Name = "btnAddFromPLS";
      this.btnAddFromPLS.Size = new System.Drawing.Size(77, 23);
      this.btnAddFromPLS.TabIndex = 17;
      this.btnAddFromPLS.Text = "Add from .pls";
      this.btnAddFromPLS.UseVisualStyleBackColor = true;
      this.btnAddFromPLS.Click += new System.EventHandler(this.btnAddFromPLS_Click);
      // 
      // RadioChannels
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.btnAddFromPLS);
      this.Controls.Add(this.mpButtonDeleteEncrypted);
      this.Controls.Add(this.mpButtonDel);
      this.Controls.Add(this.mpButtonClear);
      this.Controls.Add(this.mpButtonEdit);
      this.Controls.Add(this.mpButtonAdd);
      this.Controls.Add(this.buttonDown);
      this.Controls.Add(this.buttonUtp);
      this.Controls.Add(this.mpListView1);
      this.Name = "RadioChannels";
      this.Size = new System.Drawing.Size(467, 388);
      this.contextMenuStrip1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ColumnHeader hdrDetail2;
    private System.Windows.Forms.ColumnHeader hdrDetail3;
    private System.Windows.Forms.ColumnHeader hdrDetail1;
    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader hdrTypes;
    private System.Windows.Forms.ImageList imageList1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonAdd;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonDel;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonEdit;
    private System.Windows.Forms.Button buttonDown;
    private System.Windows.Forms.Button buttonUtp;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonDeleteEncrypted;
    private System.Windows.Forms.ColumnHeader hdrHekje;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonClear;
    private System.Windows.Forms.ColumnHeader hdrProvider;
    private System.Windows.Forms.ToolStripMenuItem renameMarkedChannelsBySIDToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem addSIDInFrontOfNameToolStripMenuItem;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private MediaPortal.UserInterface.Controls.MPButton btnAddFromPLS;
  }
}