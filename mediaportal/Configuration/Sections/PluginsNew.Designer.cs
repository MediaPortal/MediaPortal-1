namespace MediaPortal.Configuration.Sections
{
#pragma warning disable 108
  partial class PluginsNew
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
			System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Window Plugins", System.Windows.Forms.HorizontalAlignment.Left);
			System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("External Players", System.Windows.Forms.HorizontalAlignment.Left);
			System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Process Plugins", System.Windows.Forms.HorizontalAlignment.Left);
			System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Other Plugins", System.Windows.Forms.HorizontalAlignment.Left);
			System.Windows.Forms.ListViewGroup listViewGroup5 = new System.Windows.Forms.ListViewGroup("Incompatible Plugins", System.Windows.Forms.HorizontalAlignment.Left);
			this.mpTabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.mpButtonConfig = new MediaPortal.UserInterface.Controls.MPButton();
			this.mpButtonPlugin = new MediaPortal.UserInterface.Controls.MPButton();
			this.mpButtonHome = new MediaPortal.UserInterface.Controls.MPButton();
			this.mpButtonEnable = new MediaPortal.UserInterface.Controls.MPButton();
			this.listViewPlugins = new MediaPortal.UserInterface.Controls.MPListView();
			this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.contextMenuStrip = new MediaPortal.UserInterface.Controls.MPContextMenuStrip();
			this.toolTip = new MediaPortal.UserInterface.Controls.MPToolTip();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.mpTabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// mpTabControl1
			// 
			this.mpTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.mpTabControl1.Controls.Add(this.tabPage1);
			this.mpTabControl1.Location = new System.Drawing.Point(0, 8);
			this.mpTabControl1.Name = "mpTabControl1";
			this.mpTabControl1.SelectedIndex = 0;
			this.mpTabControl1.Size = new System.Drawing.Size(472, 400);
			this.mpTabControl1.TabIndex = 0;
			this.mpTabControl1.TabStop = false;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.linkLabel1);
			this.tabPage1.Controls.Add(this.mpButtonConfig);
			this.tabPage1.Controls.Add(this.mpButtonPlugin);
			this.tabPage1.Controls.Add(this.mpButtonHome);
			this.tabPage1.Controls.Add(this.mpButtonEnable);
			this.tabPage1.Controls.Add(this.listViewPlugins);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(464, 374);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Plugins";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// linkLabel1
			// 
			this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Location = new System.Drawing.Point(284, 311);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(160, 13);
			this.linkLabel1.TabIndex = 5;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Browse and install new plugins...";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// mpButtonConfig
			// 
			this.mpButtonConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.mpButtonConfig.Enabled = false;
			this.mpButtonConfig.Location = new System.Drawing.Point(361, 328);
			this.mpButtonConfig.Name = "mpButtonConfig";
			this.mpButtonConfig.Size = new System.Drawing.Size(75, 23);
			this.mpButtonConfig.TabIndex = 4;
			this.mpButtonConfig.Text = "&Config";
			this.mpButtonConfig.UseVisualStyleBackColor = true;
			this.mpButtonConfig.Click += new System.EventHandler(this.itemConfigure_Click);
			// 
			// mpButtonPlugin
			// 
			this.mpButtonPlugin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.mpButtonPlugin.Enabled = false;
			this.mpButtonPlugin.Location = new System.Drawing.Point(253, 328);
			this.mpButtonPlugin.Name = "mpButtonPlugin";
			this.mpButtonPlugin.Size = new System.Drawing.Size(75, 23);
			this.mpButtonPlugin.TabIndex = 3;
			this.mpButtonPlugin.Text = "In &Plugins";
			this.mpButtonPlugin.UseVisualStyleBackColor = true;
			this.mpButtonPlugin.Click += new System.EventHandler(this.itemMyPlugins_Click);
			// 
			// mpButtonHome
			// 
			this.mpButtonHome.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.mpButtonHome.Enabled = false;
			this.mpButtonHome.Location = new System.Drawing.Point(145, 328);
			this.mpButtonHome.Name = "mpButtonHome";
			this.mpButtonHome.Size = new System.Drawing.Size(75, 23);
			this.mpButtonHome.TabIndex = 2;
			this.mpButtonHome.Text = "In &Home";
			this.mpButtonHome.UseVisualStyleBackColor = true;
			this.mpButtonHome.Click += new System.EventHandler(this.itemMyHome_Click);
			// 
			// mpButtonEnable
			// 
			this.mpButtonEnable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.mpButtonEnable.Enabled = false;
			this.mpButtonEnable.Location = new System.Drawing.Point(37, 328);
			this.mpButtonEnable.Name = "mpButtonEnable";
			this.mpButtonEnable.Size = new System.Drawing.Size(75, 23);
			this.mpButtonEnable.TabIndex = 1;
			this.mpButtonEnable.Text = "&Enable";
			this.mpButtonEnable.UseVisualStyleBackColor = true;
			this.mpButtonEnable.Click += new System.EventHandler(this.itemEnabled_Click);
			// 
			// listViewPlugins
			// 
			this.listViewPlugins.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.listViewPlugins.AllowDrop = true;
			this.listViewPlugins.AllowRowReorder = false;
			this.listViewPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listViewPlugins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName});
			this.listViewPlugins.FullRowSelect = true;
			listViewGroup1.Header = "Window Plugins";
			listViewGroup1.Name = "listViewGroupWindow";
			listViewGroup2.Header = "External Players";
			listViewGroup2.Name = "listViewGroupExternalPlayers";
			listViewGroup3.Header = "Process Plugins";
			listViewGroup3.Name = "listViewGroupProcess";
			listViewGroup4.Header = "Other Plugins";
			listViewGroup4.Name = "listViewGroupOther";
			listViewGroup5.Header = "Incompatible Plugins";
			listViewGroup5.Name = "listViewGroupIncompatible";
			this.listViewPlugins.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3,
            listViewGroup4,
            listViewGroup5});
			this.listViewPlugins.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listViewPlugins.HideSelection = false;
			this.listViewPlugins.HotTracking = true;
			this.listViewPlugins.HoverSelection = true;
			this.listViewPlugins.Location = new System.Drawing.Point(20, 20);
			this.listViewPlugins.MultiSelect = false;
			this.listViewPlugins.Name = "listViewPlugins";
			this.listViewPlugins.ShowItemToolTips = true;
			this.listViewPlugins.Size = new System.Drawing.Size(424, 288);
			this.listViewPlugins.TabIndex = 0;
			this.listViewPlugins.UseCompatibleStateImageBehavior = false;
			this.listViewPlugins.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewPlugins_ItemSelectionChanged);
			this.listViewPlugins.Click += new System.EventHandler(this.listViewPlugins_Click);
			this.listViewPlugins.DoubleClick += new System.EventHandler(this.listViewPlugins_DoubleClick);
			this.listViewPlugins.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewPlugins_MouseClick);
			// 
			// columnHeaderName
			// 
			this.columnHeaderName.Text = "Name";
			this.columnHeaderName.Width = 420;
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.BackColor = System.Drawing.SystemColors.Window;
			this.contextMenuStrip.MinimumSize = new System.Drawing.Size(10, 0);
			this.contextMenuStrip.Name = "contextMenuStrip";
			this.contextMenuStrip.Size = new System.Drawing.Size(61, 4);
			this.contextMenuStrip.TabStop = true;
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.Filter = "MPI files|*.mpi|ZIP files|*.zip|All files|*.*";
			// 
			// PluginsNew
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = false;
			this.Controls.Add(this.mpTabControl1);
			this.Name = "PluginsNew";
			this.Size = new System.Drawing.Size(472, 408);
			this.mpTabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.ResumeLayout(false);

    }

    #endregion

	private MediaPortal.UserInterface.Controls.MPListView listViewPlugins;
    private MediaPortal.UserInterface.Controls.MPTabControl mpTabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.ColumnHeader columnHeaderName;
    private MediaPortal.UserInterface.Controls.MPToolTip toolTip;
	private MediaPortal.UserInterface.Controls.MPContextMenuStrip contextMenuStrip;
	private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonConfig;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonPlugin;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonHome;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonEnable;
    private System.Windows.Forms.LinkLabel linkLabel1;


  }
}
