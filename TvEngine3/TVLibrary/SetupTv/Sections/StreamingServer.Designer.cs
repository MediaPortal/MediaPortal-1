namespace SetupTv.Sections
{
  partial class StreamingServer
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StreamingServer));
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.SettingsGroupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.ApplyButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.PortNoNumericTextBox = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.IpAddressComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelChannelCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.ClientsGroupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.listView1 = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.mpButtonKick = new MediaPortal.UserInterface.Controls.MPButton();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SettingsGroupBox.SuspendLayout();
      this.ClientsGroupBox.SuspendLayout();
      this.contextMenuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // timer1
      // 
      this.timer1.Interval = 1000;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "user.ico");
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
      this.tabControl1.Size = new System.Drawing.Size(476, 414);
      this.tabControl1.TabIndex = 10;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.SettingsGroupBox);
      this.tabPage1.Controls.Add(this.mpLabelChannelCount);
      this.tabPage1.Controls.Add(this.ClientsGroupBox);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(468, 388);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Streaming server";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // SettingsGroupBox
      // 
      this.SettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.SettingsGroupBox.Controls.Add(this.ApplyButton);
      this.SettingsGroupBox.Controls.Add(this.PortNoNumericTextBox);
      this.SettingsGroupBox.Controls.Add(this.mpLabel2);
      this.SettingsGroupBox.Controls.Add(this.IpAddressComboBox);
      this.SettingsGroupBox.Controls.Add(this.mpLabel1);
      this.SettingsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.SettingsGroupBox.Location = new System.Drawing.Point(6, 6);
      this.SettingsGroupBox.Name = "SettingsGroupBox";
      this.SettingsGroupBox.Size = new System.Drawing.Size(456, 108);
      this.SettingsGroupBox.TabIndex = 5;
      this.SettingsGroupBox.TabStop = false;
      this.SettingsGroupBox.Text = "Settings";
      // 
      // ApplyButton
      // 
      this.ApplyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.ApplyButton.Location = new System.Drawing.Point(369, 74);
      this.ApplyButton.Name = "ApplyButton";
      this.ApplyButton.Size = new System.Drawing.Size(81, 22);
      this.ApplyButton.TabIndex = 10;
      this.ApplyButton.Text = "Apply";
      this.ApplyButton.UseVisualStyleBackColor = true;
      this.ApplyButton.Click += new System.EventHandler(this.ApplyButton_Click);
      // 
      // PortNoNumericTextBox
      // 
      this.PortNoNumericTextBox.Location = new System.Drawing.Point(6, 76);
      this.PortNoNumericTextBox.Name = "PortNoNumericTextBox";
      this.PortNoNumericTextBox.Size = new System.Drawing.Size(100, 20);
      this.PortNoNumericTextBox.TabIndex = 9;
      this.PortNoNumericTextBox.Text = "554";
      this.PortNoNumericTextBox.Value = 554;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(3, 60);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(186, 13);
      this.mpLabel2.TabIndex = 8;
      this.mpLabel2.Text = "Port for RTSP streaming (default 554):";
      // 
      // IpAddressComboBox
      // 
      this.IpAddressComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.IpAddressComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.IpAddressComboBox.FormattingEnabled = true;
      this.IpAddressComboBox.Location = new System.Drawing.Point(6, 32);
      this.IpAddressComboBox.Name = "IpAddressComboBox";
      this.IpAddressComboBox.Size = new System.Drawing.Size(330, 21);
      this.IpAddressComboBox.TabIndex = 6;
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(3, 16);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(245, 13);
      this.mpLabel1.TabIndex = 7;
      this.mpLabel1.Text = "IP address used by the server for RTSP streaming:";
      // 
      // mpLabelChannelCount
      // 
      this.mpLabelChannelCount.AutoSize = true;
      this.mpLabelChannelCount.Location = new System.Drawing.Point(13, 14);
      this.mpLabelChannelCount.Name = "mpLabelChannelCount";
      this.mpLabelChannelCount.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannelCount.TabIndex = 2;
      // 
      // ClientsGroupBox
      // 
      this.ClientsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.ClientsGroupBox.Controls.Add(this.listView1);
      this.ClientsGroupBox.Controls.Add(this.mpButtonKick);
      this.ClientsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ClientsGroupBox.Location = new System.Drawing.Point(6, 120);
      this.ClientsGroupBox.Name = "ClientsGroupBox";
      this.ClientsGroupBox.Size = new System.Drawing.Size(456, 265);
      this.ClientsGroupBox.TabIndex = 6;
      this.ClientsGroupBox.TabStop = false;
      this.ClientsGroupBox.Text = "Clients";
      // 
      // listView1
      // 
      this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
      this.listView1.ContextMenuStrip = this.contextMenuStrip1;
      this.listView1.FullRowSelect = true;
      this.listView1.Location = new System.Drawing.Point(6, 19);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(444, 214);
      this.listView1.SmallImageList = this.imageList1;
      this.listView1.TabIndex = 3;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Stream";
      this.columnHeader1.Width = 50;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "IP Adress";
      this.columnHeader2.Width = 100;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Active";
      this.columnHeader3.Width = 50;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Connected since";
      this.columnHeader4.Width = 120;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Description";
      this.columnHeader5.Width = 120;
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.toolStripMenuItem1});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(104, 32);
      // 
      // deleteToolStripMenuItem
      // 
      this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
      this.deleteToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
      this.deleteToolStripMenuItem.Text = "Kick";
      this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(100, 6);
      // 
      // mpButtonKick
      // 
      this.mpButtonKick.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonKick.Location = new System.Drawing.Point(6, 239);
      this.mpButtonKick.Name = "mpButtonKick";
      this.mpButtonKick.Size = new System.Drawing.Size(55, 23);
      this.mpButtonKick.TabIndex = 4;
      this.mpButtonKick.Text = "Kick";
      this.mpButtonKick.UseVisualStyleBackColor = true;
      this.mpButtonKick.Click += new System.EventHandler(this.mpButtonKick_Click);
      // 
      // StreamingServer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "StreamingServer";
      this.Size = new System.Drawing.Size(482, 419);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.SettingsGroupBox.ResumeLayout(false);
      this.SettingsGroupBox.PerformLayout();
      this.ClientsGroupBox.ResumeLayout(false);
      this.contextMenuStrip1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

		private System.Windows.Forms.Timer timer1;
    private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannelCount;
		private MediaPortal.UserInterface.Controls.MPButton mpButtonKick;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
    private MediaPortal.UserInterface.Controls.MPGroupBox SettingsGroupBox;
    private MediaPortal.UserInterface.Controls.MPGroupBox ClientsGroupBox;
    private MediaPortal.UserInterface.Controls.MPButton ApplyButton;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox PortNoNumericTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPComboBox IpAddressComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
  }
}