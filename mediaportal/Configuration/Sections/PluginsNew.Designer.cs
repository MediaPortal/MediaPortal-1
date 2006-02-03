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
      System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Process Plugins", System.Windows.Forms.HorizontalAlignment.Left);
      System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Window Plugins", System.Windows.Forms.HorizontalAlignment.Left);
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PluginsNew));
      this.listViewPlugins = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeaderName = new System.Windows.Forms.ColumnHeader();
      this.imageListLargePlugins = new System.Windows.Forms.ImageList(this.components);
      this.imageListSmallPlugins = new System.Windows.Forms.ImageList(this.components);
      this.mpLabelPluginName = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelDescription = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelAuthor = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpBeveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.mpTabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpGroupBoxPluginInfo = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpTabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.mpGroupBoxPluginInfo.SuspendLayout();
      this.SuspendLayout();
      // 
      // listViewPlugins
      // 
      this.listViewPlugins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName});
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
      this.listViewPlugins.Size = new System.Drawing.Size(432, 244);
      this.listViewPlugins.SmallImageList = this.imageListSmallPlugins;
      this.listViewPlugins.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listViewPlugins.StateImageList = this.imageListLargePlugins;
      this.listViewPlugins.TabIndex = 0;
      this.listViewPlugins.UseCompatibleStateImageBehavior = false;
      this.listViewPlugins.DoubleClick += new System.EventHandler(this.listViewPlugins_DoubleClick);
      this.listViewPlugins.SelectedIndexChanged += new System.EventHandler(this.listViewPlugins_SelectedIndexChanged);
      // 
      // columnHeaderName
      // 
      this.columnHeaderName.Text = "Name";
      this.columnHeaderName.Width = 424;
      // 
      // imageListLargePlugins
      // 
      this.imageListLargePlugins.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListLargePlugins.ImageStream")));
      this.imageListLargePlugins.TransparentColor = System.Drawing.Color.Transparent;
      this.imageListLargePlugins.Images.SetKeyName(0, "plugin_process.png");
      this.imageListLargePlugins.Images.SetKeyName(1, "plugin_window.png");
      // 
      // imageListSmallPlugins
      // 
      this.imageListSmallPlugins.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSmallPlugins.ImageStream")));
      this.imageListSmallPlugins.TransparentColor = System.Drawing.Color.Transparent;
      this.imageListSmallPlugins.Images.SetKeyName(0, "plugin_process.png");
      this.imageListSmallPlugins.Images.SetKeyName(1, "plugin_window.png");
      // 
      // mpLabelPluginName
      // 
      this.mpLabelPluginName.AutoSize = true;
      this.mpLabelPluginName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.mpLabelPluginName.Location = new System.Drawing.Point(16, 20);
      this.mpLabelPluginName.Name = "mpLabelPluginName";
      this.mpLabelPluginName.Size = new System.Drawing.Size(96, 16);
      this.mpLabelPluginName.TabIndex = 4;
      this.mpLabelPluginName.Text = "Plugin Name";
      // 
      // mpLabelDescription
      // 
      this.mpLabelDescription.AutoSize = true;
      this.mpLabelDescription.Location = new System.Drawing.Point(16, 64);
      this.mpLabelDescription.Name = "mpLabelDescription";
      this.mpLabelDescription.Size = new System.Drawing.Size(60, 13);
      this.mpLabelDescription.TabIndex = 5;
      this.mpLabelDescription.Text = "Description";
      // 
      // mpLabelAuthor
      // 
      this.mpLabelAuthor.AutoSize = true;
      this.mpLabelAuthor.Location = new System.Drawing.Point(16, 40);
      this.mpLabelAuthor.Name = "mpLabelAuthor";
      this.mpLabelAuthor.Size = new System.Drawing.Size(38, 13);
      this.mpLabelAuthor.TabIndex = 6;
      this.mpLabelAuthor.Text = "Author";
      // 
      // mpBeveledLine1
      // 
      this.mpBeveledLine1.Location = new System.Drawing.Point(14, 58);
      this.mpBeveledLine1.Name = "mpBeveledLine1";
      this.mpBeveledLine1.Size = new System.Drawing.Size(412, 2);
      this.mpBeveledLine1.TabIndex = 7;
      // 
      // mpTabControl1
      // 
      this.mpTabControl1.Controls.Add(this.tabPage1);
      this.mpTabControl1.Location = new System.Drawing.Point(0, 8);
      this.mpTabControl1.Name = "mpTabControl1";
      this.mpTabControl1.SelectedIndex = 0;
      this.mpTabControl1.Size = new System.Drawing.Size(472, 400);
      this.mpTabControl1.TabIndex = 8;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpGroupBoxPluginInfo);
      this.tabPage1.Controls.Add(this.listViewPlugins);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(464, 374);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Plugins";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpGroupBoxPluginInfo
      // 
      this.mpGroupBoxPluginInfo.Controls.Add(this.mpLabelPluginName);
      this.mpGroupBoxPluginInfo.Controls.Add(this.mpLabelAuthor);
      this.mpGroupBoxPluginInfo.Controls.Add(this.mpLabelDescription);
      this.mpGroupBoxPluginInfo.Controls.Add(this.mpBeveledLine1);
      this.mpGroupBoxPluginInfo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxPluginInfo.Location = new System.Drawing.Point(12, 272);
      this.mpGroupBoxPluginInfo.Name = "mpGroupBoxPluginInfo";
      this.mpGroupBoxPluginInfo.Size = new System.Drawing.Size(440, 92);
      this.mpGroupBoxPluginInfo.TabIndex = 8;
      this.mpGroupBoxPluginInfo.TabStop = false;
      this.mpGroupBoxPluginInfo.Visible = false;
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
      this.mpGroupBoxPluginInfo.ResumeLayout(false);
      this.mpGroupBoxPluginInfo.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView listViewPlugins;
    private System.Windows.Forms.ImageList imageListLargePlugins;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelPluginName;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelDescription;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelAuthor;
    private MediaPortal.UserInterface.Controls.MPBeveledLine mpBeveledLine1;
    private MediaPortal.UserInterface.Controls.MPTabControl mpTabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.ColumnHeader columnHeaderName;
    private System.Windows.Forms.ImageList imageListSmallPlugins;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBoxPluginInfo;


  }
}
