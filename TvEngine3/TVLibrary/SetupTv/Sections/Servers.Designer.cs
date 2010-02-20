namespace SetupTv.Sections
{
  partial class Servers
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Servers));
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.chooseIPForStreamingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.buttonDelete = new System.Windows.Forms.Button();
      this.buttonMaster = new System.Windows.Forms.Button();
      this.buttonChooseIp = new System.Windows.Forms.Button();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
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
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
      this.mpListView1.ContextMenuStrip = this.contextMenuStrip1;
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.HideSelection = false;
      this.mpListView1.IsChannelListView = false;
      this.mpListView1.LargeImageList = this.imageList1;
      this.mpListView1.Location = new System.Drawing.Point(0, 0);
      this.mpListView1.MultiSelect = false;
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(452, 203);
      this.mpListView1.SmallImageList = this.imageList1;
      this.mpListView1.TabIndex = 0;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      this.mpListView1.SelectedIndexChanged += new System.EventHandler(this.mpListView1_SelectedIndexChanged);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Server";
      this.columnHeader1.Width = 200;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Type";
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "RTSP Port";
      this.columnHeader3.Width = 75;
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chooseIPForStreamingToolStripMenuItem});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(214, 26);
      this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
      // 
      // chooseIPForStreamingToolStripMenuItem
      // 
      this.chooseIPForStreamingToolStripMenuItem.Name = "chooseIPForStreamingToolStripMenuItem";
      this.chooseIPForStreamingToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
      this.chooseIPForStreamingToolStripMenuItem.Text = "Change streaming settings";
      this.chooseIPForStreamingToolStripMenuItem.Click += new System.EventHandler(this.chooseIPForStreamingToolStripMenuItem_Click);
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "computer.gif");
      // 
      // buttonDelete
      // 
      this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDelete.Location = new System.Drawing.Point(377, 178);
      this.buttonDelete.Name = "buttonDelete";
      this.buttonDelete.Size = new System.Drawing.Size(75, 23);
      this.buttonDelete.TabIndex = 3;
      this.buttonDelete.Text = "Delete";
      this.buttonDelete.UseVisualStyleBackColor = true;
      this.buttonDelete.Visible = false;
      this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
      // 
      // buttonMaster
      // 
      this.buttonMaster.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonMaster.Location = new System.Drawing.Point(167, 178);
      this.buttonMaster.Name = "buttonMaster";
      this.buttonMaster.Size = new System.Drawing.Size(135, 23);
      this.buttonMaster.TabIndex = 2;
      this.buttonMaster.Text = "Set as master server";
      this.buttonMaster.UseVisualStyleBackColor = true;
      this.buttonMaster.Visible = false;
      this.buttonMaster.Click += new System.EventHandler(this.buttonMaster_Click);
      // 
      // buttonChooseIp
      // 
      this.buttonChooseIp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonChooseIp.Enabled = false;
      this.buttonChooseIp.Location = new System.Drawing.Point(6, 178);
      this.buttonChooseIp.Name = "buttonChooseIp";
      this.buttonChooseIp.Size = new System.Drawing.Size(155, 23);
      this.buttonChooseIp.TabIndex = 1;
      this.buttonChooseIp.Text = "Change  streaming settings";
      this.buttonChooseIp.UseVisualStyleBackColor = true;
      this.buttonChooseIp.Visible = false;
      this.buttonChooseIp.Click += new System.EventHandler(this.buttonChooseIp_Click);
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
      this.tabControl1.Size = new System.Drawing.Size(466, 232);
      this.tabControl1.TabIndex = 10;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpListView1);
      this.tabPage1.Controls.Add(this.buttonMaster);
      this.tabPage1.Controls.Add(this.buttonDelete);
      this.tabPage1.Controls.Add(this.buttonChooseIp);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(458, 206);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Servers";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // Servers
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "Servers";
      this.Size = new System.Drawing.Size(469, 240);
      this.Load += new System.EventHandler(this.Servers_Load);
      this.contextMenuStrip1.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.Button buttonDelete;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.Button buttonMaster;
    private System.Windows.Forms.Button buttonChooseIp;
    private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private System.Windows.Forms.ToolStripMenuItem chooseIPForStreamingToolStripMenuItem;
    private System.Windows.Forms.ColumnHeader columnHeader3;
  }
}
