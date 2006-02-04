namespace MediaPortal.Configuration.Sections
{
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PluginsNew));
      System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Process Plugins", System.Windows.Forms.HorizontalAlignment.Left);
      System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Window Plugins", System.Windows.Forms.HorizontalAlignment.Left);
      this.imageListLargePlugins = new System.Windows.Forms.ImageList(this.components);
      this.imageListSmallPlugins = new System.Windows.Forms.ImageList(this.components);
      this.mpTabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.groupBoxPluginInfo = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelPluginName = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAuthor = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelDescription = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpBeveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.listViewPlugins = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeaderName = new System.Windows.Forms.ColumnHeader();
      this.contextMenuStrip = new MediaPortal.UserInterface.Controls.MPContextMenuStrip();
      this.toolTip = new MediaPortal.UserInterface.Controls.MPToolTip();
      this.imageListContextMenu = new System.Windows.Forms.ImageList(this.components);
      this.mpTabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.groupBoxPluginInfo.SuspendLayout();
      this.SuspendLayout();
      // 
      // imageListLargePlugins
      // 
      this.imageListLargePlugins.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListLargePlugins.ImageStream")));
      this.imageListLargePlugins.TransparentColor = System.Drawing.Color.Transparent;
      this.imageListLargePlugins.Images.SetKeyName(0, "plugin_process.png");
      this.imageListLargePlugins.Images.SetKeyName(1, "plugin_window.png");
      this.imageListLargePlugins.Images.SetKeyName(2, "plugin_process_off.png");
      this.imageListLargePlugins.Images.SetKeyName(3, "plugin_window_off.png");
      // 
      // imageListSmallPlugins
      // 
      this.imageListSmallPlugins.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSmallPlugins.ImageStream")));
      this.imageListSmallPlugins.TransparentColor = System.Drawing.Color.Transparent;
      this.imageListSmallPlugins.Images.SetKeyName(0, "plugin_process.png");
      this.imageListSmallPlugins.Images.SetKeyName(1, "plugin_window.png");
      this.imageListSmallPlugins.Images.SetKeyName(2, "plugin_process_off.png");
      this.imageListSmallPlugins.Images.SetKeyName(3, "plugin_window_off.png");
      // 
      // mpTabControl1
      // 
      this.mpTabControl1.Controls.Add(this.tabPage1);
      this.mpTabControl1.Location = new System.Drawing.Point(0, 8);
      this.mpTabControl1.Name = "mpTabControl1";
      this.mpTabControl1.SelectedIndex = 0;
      this.mpTabControl1.Size = new System.Drawing.Size(472, 400);
      this.mpTabControl1.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.groupBoxPluginInfo);
      this.tabPage1.Controls.Add(this.listViewPlugins);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(464, 374);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Plugins";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // groupBoxPluginInfo
      // 
      this.groupBoxPluginInfo.Controls.Add(this.labelPluginName);
      this.groupBoxPluginInfo.Controls.Add(this.labelAuthor);
      this.groupBoxPluginInfo.Controls.Add(this.labelDescription);
      this.groupBoxPluginInfo.Controls.Add(this.mpBeveledLine1);
      this.groupBoxPluginInfo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxPluginInfo.Location = new System.Drawing.Point(12, 272);
      this.groupBoxPluginInfo.Name = "groupBoxPluginInfo";
      this.groupBoxPluginInfo.Size = new System.Drawing.Size(440, 92);
      this.groupBoxPluginInfo.TabIndex = 1;
      this.groupBoxPluginInfo.TabStop = false;
      this.groupBoxPluginInfo.Visible = false;
      // 
      // labelPluginName
      // 
      this.labelPluginName.AutoSize = true;
      this.labelPluginName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelPluginName.Location = new System.Drawing.Point(16, 20);
      this.labelPluginName.Name = "labelPluginName";
      this.labelPluginName.Size = new System.Drawing.Size(96, 16);
      this.labelPluginName.TabIndex = 0;
      this.labelPluginName.Text = "Plugin Name";
      // 
      // labelAuthor
      // 
      this.labelAuthor.AutoSize = true;
      this.labelAuthor.Location = new System.Drawing.Point(16, 40);
      this.labelAuthor.Name = "labelAuthor";
      this.labelAuthor.Size = new System.Drawing.Size(38, 13);
      this.labelAuthor.TabIndex = 1;
      this.labelAuthor.Text = "Author";
      // 
      // labelDescription
      // 
      this.labelDescription.AutoSize = true;
      this.labelDescription.Location = new System.Drawing.Point(16, 64);
      this.labelDescription.Name = "labelDescription";
      this.labelDescription.Size = new System.Drawing.Size(60, 13);
      this.labelDescription.TabIndex = 3;
      this.labelDescription.Text = "Description";
      // 
      // mpBeveledLine1
      // 
      this.mpBeveledLine1.Location = new System.Drawing.Point(14, 58);
      this.mpBeveledLine1.Name = "mpBeveledLine1";
      this.mpBeveledLine1.Size = new System.Drawing.Size(412, 2);
      this.mpBeveledLine1.TabIndex = 2;
      // 
      // listViewPlugins
      // 
      this.listViewPlugins.Activation = System.Windows.Forms.ItemActivation.OneClick;
      this.listViewPlugins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName});
      this.listViewPlugins.ContextMenuStrip = this.contextMenuStrip;
      listViewGroup1.Header = "Process Plugins";
      listViewGroup1.Name = "listViewGroupProcess";
      listViewGroup2.Header = "Window Plugins";
      listViewGroup2.Name = "listViewGroupWindow";
      this.listViewPlugins.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
      this.listViewPlugins.HideSelection = false;
      this.listViewPlugins.LabelWrap = false;
      this.listViewPlugins.LargeImageList = this.imageListLargePlugins;
      this.listViewPlugins.Location = new System.Drawing.Point(16, 20);
      this.listViewPlugins.MultiSelect = false;
      this.listViewPlugins.Name = "listViewPlugins";
      this.listViewPlugins.ShowItemToolTips = true;
      this.listViewPlugins.Size = new System.Drawing.Size(432, 332);
      this.listViewPlugins.SmallImageList = this.imageListSmallPlugins;
      this.listViewPlugins.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listViewPlugins.StateImageList = this.imageListLargePlugins;
      this.listViewPlugins.TabIndex = 0;
      this.listViewPlugins.UseCompatibleStateImageBehavior = false;
      this.listViewPlugins.DoubleClick += new System.EventHandler(this.listViewPlugins_DoubleClick);
      this.listViewPlugins.SelectedIndexChanged += new System.EventHandler(this.listViewPlugins_SelectedIndexChanged);
      this.listViewPlugins.Click += new System.EventHandler(this.listViewPlugins_Click);
      // 
      // columnHeaderName
      // 
      this.columnHeaderName.Text = "Name";
      this.columnHeaderName.Width = 424;
      // 
      // contextMenuStrip
      // 
      this.contextMenuStrip.Name = "contextMenuStrip";
      this.contextMenuStrip.Size = new System.Drawing.Size(61, 4);
      // 
      // imageListContextMenu
      // 
      this.imageListContextMenu.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListContextMenu.ImageStream")));
      this.imageListContextMenu.TransparentColor = System.Drawing.Color.Transparent;
      this.imageListContextMenu.Images.SetKeyName(0, "Icon 178.ico");
      this.imageListContextMenu.Images.SetKeyName(1, "Icon 365.ico");
      this.imageListContextMenu.Images.SetKeyName(2, "Icon 134.ico");
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
      this.groupBoxPluginInfo.ResumeLayout(false);
      this.groupBoxPluginInfo.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView listViewPlugins;
    private System.Windows.Forms.ImageList imageListLargePlugins;
    private MediaPortal.UserInterface.Controls.MPLabel labelPluginName;
    private MediaPortal.UserInterface.Controls.MPLabel labelDescription;
    private MediaPortal.UserInterface.Controls.MPLabel labelAuthor;
    private MediaPortal.UserInterface.Controls.MPBeveledLine mpBeveledLine1;
    private MediaPortal.UserInterface.Controls.MPTabControl mpTabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.ColumnHeader columnHeaderName;
    private System.Windows.Forms.ImageList imageListSmallPlugins;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxPluginInfo;
    private MediaPortal.UserInterface.Controls.MPToolTip toolTip;
    private MediaPortal.UserInterface.Controls.MPContextMenuStrip contextMenuStrip;
    private System.Windows.Forms.ImageList imageListContextMenu;


  }
}
