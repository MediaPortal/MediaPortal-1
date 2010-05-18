namespace SetupTv.Sections
{
  partial class ChannelsInRadioGroupControl
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChannelsInRadioGroupControl));
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.addToFavoritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.removeChannelFromGroup = new System.Windows.Forms.ToolStripMenuItem();
      this.deleteThisChannelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.editChannelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpButtonOrderByNumber = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonOrderByName = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonPreview = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonDel = new MediaPortal.UserInterface.Controls.MPButton();
      this.listView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.mpButtonDown = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonUp = new MediaPortal.UserInterface.Controls.MPButton();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.contextMenuStrip1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToFavoritesToolStripMenuItem,
            this.removeChannelFromGroup,
            this.deleteThisChannelToolStripMenuItem,
            this.editChannelToolStripMenuItem});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(182, 92);
      // 
      // addToFavoritesToolStripMenuItem
      // 
      this.addToFavoritesToolStripMenuItem.Name = "addToFavoritesToolStripMenuItem";
      this.addToFavoritesToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
      this.addToFavoritesToolStripMenuItem.Text = "Copy to group";
      this.addToFavoritesToolStripMenuItem.Click += new System.EventHandler(this.addToFavoritesToolStripMenuItem_Click);
      // 
      // removeChannelFromGroup
      // 
      this.removeChannelFromGroup.Name = "removeChannelFromGroup";
      this.removeChannelFromGroup.Size = new System.Drawing.Size(181, 22);
      this.removeChannelFromGroup.Text = "Remove from Group";
      this.removeChannelFromGroup.Click += new System.EventHandler(this.removeChannelFromGroup_Click);
      // 
      // deleteThisChannelToolStripMenuItem
      // 
      this.deleteThisChannelToolStripMenuItem.Name = "deleteThisChannelToolStripMenuItem";
      this.deleteThisChannelToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
      this.deleteThisChannelToolStripMenuItem.Text = "Delete this channel";
      this.deleteThisChannelToolStripMenuItem.Click += new System.EventHandler(this.deleteThisChannelToolStripMenuItem_Click);
      // 
      // editChannelToolStripMenuItem
      // 
      this.editChannelToolStripMenuItem.Name = "editChannelToolStripMenuItem";
      this.editChannelToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
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
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpGroupBox1.Controls.Add(this.mpButtonOrderByNumber);
      this.mpGroupBox1.Controls.Add(this.mpButtonOrderByName);
      this.mpGroupBox1.Controls.Add(this.mpButtonDown);
      this.mpGroupBox1.Controls.Add(this.mpButtonUp);
      this.mpGroupBox1.Controls.Add(this.mpButtonPreview);
      this.mpGroupBox1.Controls.Add(this.mpButtonDel);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 279);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(278, 77);
      this.mpGroupBox1.TabIndex = 5;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Channels";
      // 
      // mpButtonOrderByNumber
      // 
      this.mpButtonOrderByNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonOrderByNumber.Image = global::SetupTv.Properties.Resources.icon_sort_none;
      this.mpButtonOrderByNumber.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.mpButtonOrderByNumber.Location = new System.Drawing.Point(125, 45);
      this.mpButtonOrderByNumber.Name = "mpButtonOrderByNumber";
      this.mpButtonOrderByNumber.Size = new System.Drawing.Size(114, 23);
      this.mpButtonOrderByNumber.TabIndex = 31;
      this.mpButtonOrderByNumber.Text = "Order by: Number";
      this.mpButtonOrderByNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.mpButtonOrderByNumber.UseVisualStyleBackColor = true;
      this.mpButtonOrderByNumber.Click += new System.EventHandler(this.mpButtonOrderByNumber_Click);
      // 
      // mpButtonOrderByName
      // 
      this.mpButtonOrderByName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonOrderByName.Image = global::SetupTv.Properties.Resources.icon_sort_none;
      this.mpButtonOrderByName.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.mpButtonOrderByName.Location = new System.Drawing.Point(125, 19);
      this.mpButtonOrderByName.Name = "mpButtonOrderByName";
      this.mpButtonOrderByName.Size = new System.Drawing.Size(114, 23);
      this.mpButtonOrderByName.TabIndex = 30;
      this.mpButtonOrderByName.Text = "Order by: Name";
      this.mpButtonOrderByName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.mpButtonOrderByName.UseVisualStyleBackColor = true;
      this.mpButtonOrderByName.Click += new System.EventHandler(this.mpButtonOrderByName_Click);
      // 
      // mpButtonPreview
      // 
      this.mpButtonPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonPreview.Location = new System.Drawing.Point(6, 19);
      this.mpButtonPreview.Name = "mpButtonPreview";
      this.mpButtonPreview.Size = new System.Drawing.Size(116, 23);
      this.mpButtonPreview.TabIndex = 6;
      this.mpButtonPreview.Text = "Preview";
      this.mpButtonPreview.UseVisualStyleBackColor = true;
      this.mpButtonPreview.Click += new System.EventHandler(this.mpButtonPreview_Click);
      // 
      // mpButtonDel
      // 
      this.mpButtonDel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonDel.Location = new System.Drawing.Point(6, 45);
      this.mpButtonDel.Name = "mpButtonDel";
      this.mpButtonDel.Size = new System.Drawing.Size(116, 23);
      this.mpButtonDel.TabIndex = 5;
      this.mpButtonDel.Text = "Remove from group";
      this.mpButtonDel.UseVisualStyleBackColor = true;
      this.mpButtonDel.Click += new System.EventHandler(this.mpButtonDel_Click);
      // 
      // listView1
      // 
      this.listView1.AllowDrop = true;
      this.listView1.AllowRowReorder = true;
      this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listView1.CheckBoxes = true;
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
      this.listView1.ContextMenuStrip = this.contextMenuStrip1;
      this.listView1.FullRowSelect = true;
      this.listView1.HideSelection = false;
      this.listView1.IsChannelListView = false;
      this.listView1.LabelEdit = true;
      this.listView1.LargeImageList = this.imageList1;
      this.listView1.Location = new System.Drawing.Point(0, 0);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(439, 273);
      this.listView1.SmallImageList = this.imageList1;
      this.listView1.TabIndex = 0;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
      this.listView1.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listView1_AfterLabelEdit);
      this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
      this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
      this.listView1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listView1_ItemDrag);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Name";
      this.columnHeader1.Width = 300;
      // 
      // mpButtonDown
      // 
      this.mpButtonDown.Image = global::SetupTv.Properties.Resources.icon_down;
      this.mpButtonDown.Location = new System.Drawing.Point(242, 45);
      this.mpButtonDown.Name = "mpButtonDown";
      this.mpButtonDown.Size = new System.Drawing.Size(30, 23);
      this.mpButtonDown.TabIndex = 29;
      this.mpButtonDown.UseVisualStyleBackColor = true;
      this.mpButtonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // mpButtonUp
      // 
      this.mpButtonUp.Image = global::SetupTv.Properties.Resources.icon_up;
      this.mpButtonUp.Location = new System.Drawing.Point(242, 19);
      this.mpButtonUp.Name = "mpButtonUp";
      this.mpButtonUp.Size = new System.Drawing.Size(30, 23);
      this.mpButtonUp.TabIndex = 28;
      this.mpButtonUp.UseVisualStyleBackColor = true;
      this.mpButtonUp.Click += new System.EventHandler(this.buttonUtp_Click);
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Channel number";
      this.columnHeader2.Width = 120;
      // 
      // ChannelsInRadioGroupControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.listView1);
      this.Name = "ChannelsInRadioGroupControl";
      this.Size = new System.Drawing.Size(439, 356);
      this.Load += new System.EventHandler(this.ChannelsInGroupControl_Load);
      this.contextMenuStrip1.ResumeLayout(false);
      this.mpGroupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private System.Windows.Forms.ToolStripMenuItem deleteThisChannelToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem editChannelToolStripMenuItem;
    private System.Windows.Forms.ImageList imageList1;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonPreview;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonDel;
    private System.Windows.Forms.ToolStripMenuItem addToFavoritesToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem removeChannelFromGroup;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonDown;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonUp;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonOrderByNumber;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonOrderByName;
    private System.Windows.Forms.ColumnHeader columnHeader2;
  }
}
