namespace SetupTv.Sections
{
  partial class ChannelsInGroupControl
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChannelsInGroupControl));
      this.listView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.addToFavoritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.deleteThisChannelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.removeEntireGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.editChannelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.buttonDown = new System.Windows.Forms.Button();
      this.buttonUtp = new System.Windows.Forms.Button();
      this.mpButtonDel = new MediaPortal.UserInterface.Controls.MPButton();
      this.contextMenuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // listView1
      // 
      this.listView1.AllowDrop = true;
      this.listView1.AllowRowReorder = true;
      this.listView1.CheckBoxes = true;
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
      this.listView1.ContextMenuStrip = this.contextMenuStrip1;
      this.listView1.FullRowSelect = true;
      this.listView1.HideSelection = false;
      this.listView1.Location = new System.Drawing.Point(0, 0);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(454, 313);
      this.listView1.StateImageList = this.imageList1;
      this.listView1.TabIndex = 0;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
      this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
      this.listView1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listView1_ItemDrag);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "#";
      this.columnHeader1.Width = 90;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Name";
      this.columnHeader2.Width = 200;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Types";
      this.columnHeader3.Width = 120;
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToFavoritesToolStripMenuItem,
            this.deleteThisChannelToolStripMenuItem,
            this.removeEntireGroupToolStripMenuItem,
            this.editChannelToolStripMenuItem});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(187, 92);
      // 
      // addToFavoritesToolStripMenuItem
      // 
      this.addToFavoritesToolStripMenuItem.Name = "addToFavoritesToolStripMenuItem";
      this.addToFavoritesToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
      this.addToFavoritesToolStripMenuItem.Text = "Remove from Group";
      this.addToFavoritesToolStripMenuItem.Click += new System.EventHandler(this.addToFavoritesToolStripMenuItem_Click);
      // 
      // deleteThisChannelToolStripMenuItem
      // 
      this.deleteThisChannelToolStripMenuItem.Name = "deleteThisChannelToolStripMenuItem";
      this.deleteThisChannelToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
      this.deleteThisChannelToolStripMenuItem.Text = "Delete this channel";
      this.deleteThisChannelToolStripMenuItem.Click += new System.EventHandler(this.deleteThisChannelToolStripMenuItem_Click);
      // 
      // removeEntireGroupToolStripMenuItem
      // 
      this.removeEntireGroupToolStripMenuItem.Name = "removeEntireGroupToolStripMenuItem";
      this.removeEntireGroupToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
      this.removeEntireGroupToolStripMenuItem.Text = "Remove entire group";
      this.removeEntireGroupToolStripMenuItem.Click += new System.EventHandler(this.removeEntireGroupToolStripMenuItem_Click);
      // 
      // editChannelToolStripMenuItem
      // 
      this.editChannelToolStripMenuItem.Name = "editChannelToolStripMenuItem";
      this.editChannelToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
      this.editChannelToolStripMenuItem.Text = "Edit channel";
      this.editChannelToolStripMenuItem.Click += new System.EventHandler(this.editChannelToolStripMenuItem_Click);
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
      // buttonDown
      // 
      this.buttonDown.Location = new System.Drawing.Point(12, 343);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(44, 23);
      this.buttonDown.TabIndex = 6;
      this.buttonDown.Text = "Down";
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // buttonUtp
      // 
      this.buttonUtp.Location = new System.Drawing.Point(12, 319);
      this.buttonUtp.Name = "buttonUtp";
      this.buttonUtp.Size = new System.Drawing.Size(44, 23);
      this.buttonUtp.TabIndex = 5;
      this.buttonUtp.Text = "Up";
      this.buttonUtp.UseVisualStyleBackColor = true;
      this.buttonUtp.Click += new System.EventHandler(this.buttonUtp_Click);
      // 
      // mpButtonDel
      // 
      this.mpButtonDel.Location = new System.Drawing.Point(62, 319);
      this.mpButtonDel.Name = "mpButtonDel";
      this.mpButtonDel.Size = new System.Drawing.Size(116, 23);
      this.mpButtonDel.TabIndex = 7;
      this.mpButtonDel.Text = "Remove from group";
      this.mpButtonDel.UseVisualStyleBackColor = true;
      this.mpButtonDel.Click += new System.EventHandler(this.mpButtonDel_Click);
      // 
      // ChannelsInGroupControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpButtonDel);
      this.Controls.Add(this.buttonDown);
      this.Controls.Add(this.buttonUtp);
      this.Controls.Add(this.listView1);
      this.Name = "ChannelsInGroupControl";
      this.Size = new System.Drawing.Size(457, 374);
      this.Load += new System.EventHandler(this.ChannelsInGroupControl_Load);
      this.contextMenuStrip1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.Button buttonDown;
    private System.Windows.Forms.Button buttonUtp;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonDel;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private System.Windows.Forms.ToolStripMenuItem addToFavoritesToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem deleteThisChannelToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem removeEntireGroupToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem editChannelToolStripMenuItem;
    private System.Windows.Forms.ImageList imageList1;
  }
}
