using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class Channels
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Channels));
      this.listViewChannels = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderChannelsName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.columnHeaderChannelsNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.columnHeaderChannelsGroups = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.columnHeaderChannelsProvider = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.columnHeaderChannelsTuningDetails = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.tabControl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabControl();
      this.tabPageChannels = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.textBoxChannelNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.buttonChannelAddToGroup = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonChannelAdd = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonChannelDelete = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonChannelTest = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonChannelSplit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonChannelEdit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelFilter = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonChannelMerge = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.textBoxFilter = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.buttonChannelPreview = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.tabPageChannelGroups = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.groupBoxGroup = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.buttonGroupOrder = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonGroupAdd = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonGroupDelete = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.comboBoxChannelGroup = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.buttonGroupRename = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.groupBoxGroupOrder = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelGroupOrderByName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelGroupOrderByNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelGroupOrderManual = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonGroupOrderByName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonGroupOrderByNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonGroupOrderDown = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonGroupOrderUp = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.groupBoxGroupChannels = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.buttonGroupChannelsAdd = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonGroupChannelsRemove = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.listViewChannelsInGroup = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderChannelsInGroupName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.columnHeaderChannelsInGroupNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.toolTip = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPToolTip();
      this.tabControl.SuspendLayout();
      this.tabPageChannels.SuspendLayout();
      this.tabPageChannelGroups.SuspendLayout();
      this.groupBoxGroup.SuspendLayout();
      this.groupBoxGroupOrder.SuspendLayout();
      this.groupBoxGroupChannels.SuspendLayout();
      this.SuspendLayout();
      // 
      // listViewChannels
      // 
      this.listViewChannels.AllowColumnReorder = true;
      this.listViewChannels.AllowRowReorder = false;
      this.listViewChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewChannels.CheckBoxes = true;
      this.listViewChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderChannelsName,
            this.columnHeaderChannelsNumber,
            this.columnHeaderChannelsGroups,
            this.columnHeaderChannelsProvider,
            this.columnHeaderChannelsTuningDetails});
      this.listViewChannels.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
      this.listViewChannels.FullRowSelect = true;
      this.listViewChannels.LabelEdit = true;
      this.listViewChannels.LargeImageList = this.imageList1;
      this.listViewChannels.Location = new System.Drawing.Point(9, 32);
      this.listViewChannels.Name = "listViewChannels";
      this.listViewChannels.Size = new System.Drawing.Size(457, 327);
      this.listViewChannels.SmallImageList = this.imageList1;
      this.listViewChannels.TabIndex = 2;
      this.listViewChannels.UseCompatibleStateImageBehavior = false;
      this.listViewChannels.View = System.Windows.Forms.View.Details;
      this.listViewChannels.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listViewChannels_AfterLabelEdit);
      this.listViewChannels.BeforeLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listViewChannels_BeforeLabelEdit);
      this.listViewChannels.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewChannels_ColumnClick);
      this.listViewChannels.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listViewChannels_ItemCheck);
      this.listViewChannels.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewChannels_ItemChecked);
      this.listViewChannels.SelectedIndexChanged += new System.EventHandler(this.listViewChannels_SelectedIndexChanged);
      this.listViewChannels.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewChannels_KeyDown);
      this.listViewChannels.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewChannels_MouseClick);
      this.listViewChannels.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewChannels_MouseDoubleClick);
      this.listViewChannels.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.listViewChannels_PreviewKeyDown);
      // 
      // columnHeaderChannelsName
      // 
      this.columnHeaderChannelsName.Text = "Name";
      this.columnHeaderChannelsName.Width = 120;
      // 
      // columnHeaderChannelsNumber
      // 
      this.columnHeaderChannelsNumber.Text = "#";
      this.columnHeaderChannelsNumber.Width = 50;
      // 
      // columnHeaderChannelsGroups
      // 
      this.columnHeaderChannelsGroups.Text = "Group(s)";
      this.columnHeaderChannelsGroups.Width = 80;
      // 
      // columnHeaderChannelsProvider
      // 
      this.columnHeaderChannelsProvider.Text = "Provider(s)";
      this.columnHeaderChannelsProvider.Width = 80;
      // 
      // columnHeaderChannelsTuningDetails
      // 
      this.columnHeaderChannelsTuningDetails.Text = "Tuning Detail(s)";
      this.columnHeaderChannelsTuningDetails.Width = 100;
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "radio_fta_.png");
      this.imageList1.Images.SetKeyName(1, "radio_scrambled.png");
      this.imageList1.Images.SetKeyName(2, "icon.radio_scrambled_and_fta.png");
      this.imageList1.Images.SetKeyName(3, "tv_fta_.png");
      this.imageList1.Images.SetKeyName(4, "tv_scrambled.png");
      this.imageList1.Images.SetKeyName(5, "icon.tv_scrambled_and_fta.png");
      // 
      // tabControl
      // 
      this.tabControl.AllowDrop = true;
      this.tabControl.AllowReorderTabs = false;
      this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl.Controls.Add(this.tabPageChannels);
      this.tabControl.Controls.Add(this.tabPageChannelGroups);
      this.tabControl.Location = new System.Drawing.Point(0, 0);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(480, 420);
      this.tabControl.TabIndex = 0;
      this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
      // 
      // tabPageChannels
      // 
      this.tabPageChannels.Controls.Add(this.textBoxChannelNumber);
      this.tabPageChannels.Controls.Add(this.buttonChannelAddToGroup);
      this.tabPageChannels.Controls.Add(this.buttonChannelAdd);
      this.tabPageChannels.Controls.Add(this.buttonChannelDelete);
      this.tabPageChannels.Controls.Add(this.buttonChannelTest);
      this.tabPageChannels.Controls.Add(this.buttonChannelSplit);
      this.tabPageChannels.Controls.Add(this.buttonChannelEdit);
      this.tabPageChannels.Controls.Add(this.labelFilter);
      this.tabPageChannels.Controls.Add(this.buttonChannelMerge);
      this.tabPageChannels.Controls.Add(this.textBoxFilter);
      this.tabPageChannels.Controls.Add(this.buttonChannelPreview);
      this.tabPageChannels.Controls.Add(this.listViewChannels);
      this.tabPageChannels.Location = new System.Drawing.Point(4, 22);
      this.tabPageChannels.Name = "tabPageChannels";
      this.tabPageChannels.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageChannels.Size = new System.Drawing.Size(472, 394);
      this.tabPageChannels.TabIndex = 0;
      this.tabPageChannels.Text = "Channels";
      this.tabPageChannels.UseVisualStyleBackColor = true;
      // 
      // textBoxChannelNumber
      // 
      this.textBoxChannelNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
      this.textBoxChannelNumber.Location = new System.Drawing.Point(160, 203);
      this.textBoxChannelNumber.Margin = new System.Windows.Forms.Padding(0);
      this.textBoxChannelNumber.MaxLength = 10;
      this.textBoxChannelNumber.Name = "textBoxChannelNumber";
      this.textBoxChannelNumber.Size = new System.Drawing.Size(100, 20);
      this.textBoxChannelNumber.TabIndex = 11;
      this.textBoxChannelNumber.TabStop = false;
      this.textBoxChannelNumber.Text = "0";
      this.textBoxChannelNumber.Visible = false;
      this.textBoxChannelNumber.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxChannelNumber_KeyDown);
      this.textBoxChannelNumber.Leave += new System.EventHandler(this.textBoxChannelNumber_Leave);
      this.textBoxChannelNumber.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.textBoxChannelNumber_PreviewKeyDown);
      // 
      // buttonChannelAddToGroup
      // 
      this.buttonChannelAddToGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonChannelAddToGroup.Location = new System.Drawing.Point(160, 365);
      this.buttonChannelAddToGroup.Name = "buttonChannelAddToGroup";
      this.buttonChannelAddToGroup.Size = new System.Drawing.Size(82, 23);
      this.buttonChannelAddToGroup.TabIndex = 6;
      this.buttonChannelAddToGroup.Text = "Add To &Group";
      this.buttonChannelAddToGroup.UseVisualStyleBackColor = true;
      this.buttonChannelAddToGroup.Click += new System.EventHandler(this.buttonChannelAddToGroup_Click);
      // 
      // buttonChannelAdd
      // 
      this.buttonChannelAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonChannelAdd.Location = new System.Drawing.Point(9, 365);
      this.buttonChannelAdd.Name = "buttonChannelAdd";
      this.buttonChannelAdd.Size = new System.Drawing.Size(40, 23);
      this.buttonChannelAdd.TabIndex = 3;
      this.buttonChannelAdd.Text = "&Add";
      this.buttonChannelAdd.UseVisualStyleBackColor = true;
      this.buttonChannelAdd.Click += new System.EventHandler(this.buttonChannelAdd_Click);
      // 
      // buttonChannelDelete
      // 
      this.buttonChannelDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonChannelDelete.Location = new System.Drawing.Point(95, 365);
      this.buttonChannelDelete.Name = "buttonChannelDelete";
      this.buttonChannelDelete.Size = new System.Drawing.Size(48, 23);
      this.buttonChannelDelete.TabIndex = 5;
      this.buttonChannelDelete.Text = "&Delete";
      this.buttonChannelDelete.UseVisualStyleBackColor = true;
      this.buttonChannelDelete.Click += new System.EventHandler(this.buttonChannelDelete_Click);
      // 
      // buttonChannelTest
      // 
      this.buttonChannelTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonChannelTest.Location = new System.Drawing.Point(426, 365);
      this.buttonChannelTest.Name = "buttonChannelTest";
      this.buttonChannelTest.Size = new System.Drawing.Size(40, 23);
      this.buttonChannelTest.TabIndex = 10;
      this.buttonChannelTest.Text = "&Test";
      this.buttonChannelTest.UseVisualStyleBackColor = true;
      this.buttonChannelTest.Click += new System.EventHandler(this.buttonChannelTest_Click);
      // 
      // buttonChannelSplit
      // 
      this.buttonChannelSplit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonChannelSplit.Location = new System.Drawing.Point(307, 365);
      this.buttonChannelSplit.Name = "buttonChannelSplit";
      this.buttonChannelSplit.Size = new System.Drawing.Size(45, 23);
      this.buttonChannelSplit.TabIndex = 8;
      this.buttonChannelSplit.Text = "&Split";
      this.buttonChannelSplit.UseVisualStyleBackColor = true;
      this.buttonChannelSplit.Click += new System.EventHandler(this.buttonChannelSplit_Click);
      // 
      // buttonChannelEdit
      // 
      this.buttonChannelEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonChannelEdit.Location = new System.Drawing.Point(52, 365);
      this.buttonChannelEdit.Name = "buttonChannelEdit";
      this.buttonChannelEdit.Size = new System.Drawing.Size(40, 23);
      this.buttonChannelEdit.TabIndex = 4;
      this.buttonChannelEdit.Text = "&Edit";
      this.buttonChannelEdit.UseVisualStyleBackColor = true;
      this.buttonChannelEdit.Click += new System.EventHandler(this.buttonChannelEdit_Click);
      // 
      // labelFilter
      // 
      this.labelFilter.AutoSize = true;
      this.labelFilter.Location = new System.Drawing.Point(6, 9);
      this.labelFilter.Name = "labelFilter";
      this.labelFilter.Size = new System.Drawing.Size(32, 13);
      this.labelFilter.TabIndex = 0;
      this.labelFilter.Text = "Filter:";
      // 
      // buttonChannelMerge
      // 
      this.buttonChannelMerge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonChannelMerge.Location = new System.Drawing.Point(259, 365);
      this.buttonChannelMerge.Name = "buttonChannelMerge";
      this.buttonChannelMerge.Size = new System.Drawing.Size(45, 23);
      this.buttonChannelMerge.TabIndex = 7;
      this.buttonChannelMerge.Text = "&Merge";
      this.buttonChannelMerge.UseVisualStyleBackColor = true;
      this.buttonChannelMerge.Click += new System.EventHandler(this.buttonChannelMerge_Click);
      // 
      // textBoxFilter
      // 
      this.textBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxFilter.Location = new System.Drawing.Point(44, 6);
      this.textBoxFilter.Name = "textBoxFilter";
      this.textBoxFilter.Size = new System.Drawing.Size(422, 20);
      this.textBoxFilter.TabIndex = 1;
      this.textBoxFilter.TextChanged += new System.EventHandler(this.textBoxFilter_TextChanged);
      // 
      // buttonChannelPreview
      // 
      this.buttonChannelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonChannelPreview.Location = new System.Drawing.Point(370, 365);
      this.buttonChannelPreview.Name = "buttonChannelPreview";
      this.buttonChannelPreview.Size = new System.Drawing.Size(53, 23);
      this.buttonChannelPreview.TabIndex = 9;
      this.buttonChannelPreview.Text = "&Preview";
      this.buttonChannelPreview.UseVisualStyleBackColor = true;
      this.buttonChannelPreview.Click += new System.EventHandler(this.buttonChannelPreview_Click);
      // 
      // tabPageChannelGroups
      // 
      this.tabPageChannelGroups.Controls.Add(this.groupBoxGroup);
      this.tabPageChannelGroups.Controls.Add(this.groupBoxGroupOrder);
      this.tabPageChannelGroups.Controls.Add(this.groupBoxGroupChannels);
      this.tabPageChannelGroups.Controls.Add(this.listViewChannelsInGroup);
      this.tabPageChannelGroups.Location = new System.Drawing.Point(4, 22);
      this.tabPageChannelGroups.Name = "tabPageChannelGroups";
      this.tabPageChannelGroups.Size = new System.Drawing.Size(472, 394);
      this.tabPageChannelGroups.TabIndex = 1;
      this.tabPageChannelGroups.Text = "Channel Groups";
      this.tabPageChannelGroups.UseVisualStyleBackColor = true;
      // 
      // groupBoxGroup
      // 
      this.groupBoxGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGroup.Controls.Add(this.buttonGroupOrder);
      this.groupBoxGroup.Controls.Add(this.buttonGroupAdd);
      this.groupBoxGroup.Controls.Add(this.buttonGroupDelete);
      this.groupBoxGroup.Controls.Add(this.comboBoxChannelGroup);
      this.groupBoxGroup.Controls.Add(this.buttonGroupRename);
      this.groupBoxGroup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGroup.Location = new System.Drawing.Point(3, 3);
      this.groupBoxGroup.Name = "groupBoxGroup";
      this.groupBoxGroup.Size = new System.Drawing.Size(466, 49);
      this.groupBoxGroup.TabIndex = 0;
      this.groupBoxGroup.TabStop = false;
      this.groupBoxGroup.Text = "Group";
      // 
      // buttonGroupOrder
      // 
      this.buttonGroupOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonGroupOrder.Location = new System.Drawing.Point(405, 17);
      this.buttonGroupOrder.Name = "buttonGroupOrder";
      this.buttonGroupOrder.Size = new System.Drawing.Size(55, 23);
      this.buttonGroupOrder.TabIndex = 4;
      this.buttonGroupOrder.Text = "&Order";
      this.buttonGroupOrder.UseVisualStyleBackColor = true;
      this.buttonGroupOrder.Click += new System.EventHandler(this.buttonGroupOrder_Click);
      // 
      // buttonGroupAdd
      // 
      this.buttonGroupAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonGroupAdd.Location = new System.Drawing.Point(231, 17);
      this.buttonGroupAdd.Name = "buttonGroupAdd";
      this.buttonGroupAdd.Size = new System.Drawing.Size(55, 23);
      this.buttonGroupAdd.TabIndex = 1;
      this.buttonGroupAdd.Text = "&Add";
      this.buttonGroupAdd.UseVisualStyleBackColor = true;
      this.buttonGroupAdd.Click += new System.EventHandler(this.buttonGroupAdd_Click);
      // 
      // buttonGroupDelete
      // 
      this.buttonGroupDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonGroupDelete.Location = new System.Drawing.Point(347, 17);
      this.buttonGroupDelete.Name = "buttonGroupDelete";
      this.buttonGroupDelete.Size = new System.Drawing.Size(55, 23);
      this.buttonGroupDelete.TabIndex = 3;
      this.buttonGroupDelete.Text = "&Delete";
      this.buttonGroupDelete.UseVisualStyleBackColor = true;
      this.buttonGroupDelete.Click += new System.EventHandler(this.buttonGroupDelete_Click);
      // 
      // comboBoxChannelGroup
      // 
      this.comboBoxChannelGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxChannelGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxChannelGroup.FormattingEnabled = true;
      this.comboBoxChannelGroup.Location = new System.Drawing.Point(6, 19);
      this.comboBoxChannelGroup.Name = "comboBoxChannelGroup";
      this.comboBoxChannelGroup.Size = new System.Drawing.Size(210, 21);
      this.comboBoxChannelGroup.TabIndex = 0;
      this.comboBoxChannelGroup.SelectedIndexChanged += new System.EventHandler(this.comboBoxChannelGroup_SelectedIndexChanged);
      // 
      // buttonGroupRename
      // 
      this.buttonGroupRename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonGroupRename.Location = new System.Drawing.Point(289, 17);
      this.buttonGroupRename.Name = "buttonGroupRename";
      this.buttonGroupRename.Size = new System.Drawing.Size(55, 23);
      this.buttonGroupRename.TabIndex = 2;
      this.buttonGroupRename.Text = "&Rename";
      this.buttonGroupRename.UseVisualStyleBackColor = true;
      this.buttonGroupRename.Click += new System.EventHandler(this.buttonGroupRename_Click);
      // 
      // groupBoxGroupOrder
      // 
      this.groupBoxGroupOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGroupOrder.Controls.Add(this.labelGroupOrderByName);
      this.groupBoxGroupOrder.Controls.Add(this.labelGroupOrderByNumber);
      this.groupBoxGroupOrder.Controls.Add(this.labelGroupOrderManual);
      this.groupBoxGroupOrder.Controls.Add(this.buttonGroupOrderByName);
      this.groupBoxGroupOrder.Controls.Add(this.buttonGroupOrderByNumber);
      this.groupBoxGroupOrder.Controls.Add(this.buttonGroupOrderDown);
      this.groupBoxGroupOrder.Controls.Add(this.buttonGroupOrderUp);
      this.groupBoxGroupOrder.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGroupOrder.Location = new System.Drawing.Point(354, 163);
      this.groupBoxGroupOrder.Name = "groupBoxGroupOrder";
      this.groupBoxGroupOrder.Size = new System.Drawing.Size(109, 134);
      this.groupBoxGroupOrder.TabIndex = 3;
      this.groupBoxGroupOrder.TabStop = false;
      this.groupBoxGroupOrder.Text = "Channel Order";
      // 
      // labelGroupOrderByName
      // 
      this.labelGroupOrderByName.AutoSize = true;
      this.labelGroupOrderByName.Location = new System.Drawing.Point(6, 24);
      this.labelGroupOrderByName.Name = "labelGroupOrderByName";
      this.labelGroupOrderByName.Size = new System.Drawing.Size(53, 13);
      this.labelGroupOrderByName.TabIndex = 0;
      this.labelGroupOrderByName.Text = "By Name:";
      // 
      // labelGroupOrderByNumber
      // 
      this.labelGroupOrderByNumber.AutoSize = true;
      this.labelGroupOrderByNumber.Location = new System.Drawing.Point(6, 53);
      this.labelGroupOrderByNumber.Name = "labelGroupOrderByNumber";
      this.labelGroupOrderByNumber.Size = new System.Drawing.Size(62, 13);
      this.labelGroupOrderByNumber.TabIndex = 2;
      this.labelGroupOrderByNumber.Text = "By Number:";
      // 
      // labelGroupOrderManual
      // 
      this.labelGroupOrderManual.AutoSize = true;
      this.labelGroupOrderManual.Location = new System.Drawing.Point(6, 82);
      this.labelGroupOrderManual.Name = "labelGroupOrderManual";
      this.labelGroupOrderManual.Size = new System.Drawing.Size(45, 13);
      this.labelGroupOrderManual.TabIndex = 4;
      this.labelGroupOrderManual.Text = "Manual:";
      // 
      // buttonGroupOrderByName
      // 
      this.buttonGroupOrderByName.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_none;
      this.buttonGroupOrderByName.Location = new System.Drawing.Point(73, 19);
      this.buttonGroupOrderByName.Name = "buttonGroupOrderByName";
      this.buttonGroupOrderByName.Size = new System.Drawing.Size(30, 23);
      this.buttonGroupOrderByName.TabIndex = 1;
      this.buttonGroupOrderByName.UseVisualStyleBackColor = true;
      this.buttonGroupOrderByName.Click += new System.EventHandler(this.buttonGroupOrderByName_Click);
      // 
      // buttonGroupOrderByNumber
      // 
      this.buttonGroupOrderByNumber.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_none;
      this.buttonGroupOrderByNumber.Location = new System.Drawing.Point(73, 48);
      this.buttonGroupOrderByNumber.Name = "buttonGroupOrderByNumber";
      this.buttonGroupOrderByNumber.Size = new System.Drawing.Size(30, 23);
      this.buttonGroupOrderByNumber.TabIndex = 3;
      this.buttonGroupOrderByNumber.UseVisualStyleBackColor = true;
      this.buttonGroupOrderByNumber.Click += new System.EventHandler(this.buttonGroupOrderByNumber_Click);
      // 
      // buttonGroupOrderDown
      // 
      this.buttonGroupOrderDown.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_down;
      this.buttonGroupOrderDown.Location = new System.Drawing.Point(73, 102);
      this.buttonGroupOrderDown.Name = "buttonGroupOrderDown";
      this.buttonGroupOrderDown.Size = new System.Drawing.Size(30, 23);
      this.buttonGroupOrderDown.TabIndex = 6;
      this.buttonGroupOrderDown.UseVisualStyleBackColor = true;
      this.buttonGroupOrderDown.Click += new System.EventHandler(this.buttonGroupOrderDown_Click);
      // 
      // buttonGroupOrderUp
      // 
      this.buttonGroupOrderUp.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_up;
      this.buttonGroupOrderUp.Location = new System.Drawing.Point(73, 77);
      this.buttonGroupOrderUp.Name = "buttonGroupOrderUp";
      this.buttonGroupOrderUp.Size = new System.Drawing.Size(30, 23);
      this.buttonGroupOrderUp.TabIndex = 5;
      this.buttonGroupOrderUp.UseVisualStyleBackColor = true;
      this.buttonGroupOrderUp.Click += new System.EventHandler(this.buttonGroupOrderUp_Click);
      // 
      // groupBoxGroupChannels
      // 
      this.groupBoxGroupChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGroupChannels.Controls.Add(this.buttonGroupChannelsAdd);
      this.groupBoxGroupChannels.Controls.Add(this.buttonGroupChannelsRemove);
      this.groupBoxGroupChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGroupChannels.Location = new System.Drawing.Point(354, 80);
      this.groupBoxGroupChannels.Name = "groupBoxGroupChannels";
      this.groupBoxGroupChannels.Size = new System.Drawing.Size(109, 77);
      this.groupBoxGroupChannels.TabIndex = 2;
      this.groupBoxGroupChannels.TabStop = false;
      this.groupBoxGroupChannels.Text = "Channels";
      // 
      // buttonGroupChannelsAdd
      // 
      this.buttonGroupChannelsAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonGroupChannelsAdd.Location = new System.Drawing.Point(24, 19);
      this.buttonGroupChannelsAdd.Name = "buttonGroupChannelsAdd";
      this.buttonGroupChannelsAdd.Size = new System.Drawing.Size(60, 23);
      this.buttonGroupChannelsAdd.TabIndex = 0;
      this.buttonGroupChannelsAdd.Text = "Add";
      this.buttonGroupChannelsAdd.UseVisualStyleBackColor = true;
      this.buttonGroupChannelsAdd.Click += new System.EventHandler(this.buttonGroupChannelsAdd_Click);
      // 
      // buttonGroupChannelsRemove
      // 
      this.buttonGroupChannelsRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonGroupChannelsRemove.Location = new System.Drawing.Point(24, 48);
      this.buttonGroupChannelsRemove.Name = "buttonGroupChannelsRemove";
      this.buttonGroupChannelsRemove.Size = new System.Drawing.Size(60, 23);
      this.buttonGroupChannelsRemove.TabIndex = 1;
      this.buttonGroupChannelsRemove.Text = "Remove";
      this.buttonGroupChannelsRemove.UseVisualStyleBackColor = true;
      this.buttonGroupChannelsRemove.Click += new System.EventHandler(this.buttonGroupChannelsRemove_Click);
      // 
      // listViewChannelsInGroup
      // 
      this.listViewChannelsInGroup.AllowDrop = true;
      this.listViewChannelsInGroup.AllowRowReorder = true;
      this.listViewChannelsInGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewChannelsInGroup.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderChannelsInGroupName,
            this.columnHeaderChannelsInGroupNumber});
      this.listViewChannelsInGroup.FullRowSelect = true;
      this.listViewChannelsInGroup.HideSelection = false;
      this.listViewChannelsInGroup.Location = new System.Drawing.Point(9, 58);
      this.listViewChannelsInGroup.Name = "listViewChannelsInGroup";
      this.listViewChannelsInGroup.Size = new System.Drawing.Size(338, 333);
      this.listViewChannelsInGroup.TabIndex = 1;
      this.listViewChannelsInGroup.UseCompatibleStateImageBehavior = false;
      this.listViewChannelsInGroup.View = System.Windows.Forms.View.Details;
      this.listViewChannelsInGroup.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewChannelsInGroup_ColumnClick);
      this.listViewChannelsInGroup.SelectedIndexChanged += new System.EventHandler(this.listViewChannelsInGroup_SelectedIndexChanged);
      this.listViewChannelsInGroup.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewChannelsInGroup_KeyDown);
      // 
      // columnHeaderChannelsInGroupName
      // 
      this.columnHeaderChannelsInGroupName.Text = "Name";
      this.columnHeaderChannelsInGroupName.Width = 246;
      // 
      // columnHeaderChannelsInGroupNumber
      // 
      this.columnHeaderChannelsInGroupNumber.Text = "#";
      this.columnHeaderChannelsInGroupNumber.Width = 43;
      // 
      // Channels
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl);
      this.Name = "Channels";
      this.Size = new System.Drawing.Size(480, 420);
      this.tabControl.ResumeLayout(false);
      this.tabPageChannels.ResumeLayout(false);
      this.tabPageChannels.PerformLayout();
      this.tabPageChannelGroups.ResumeLayout(false);
      this.groupBoxGroup.ResumeLayout(false);
      this.groupBoxGroupOrder.ResumeLayout(false);
      this.groupBoxGroupOrder.PerformLayout();
      this.groupBoxGroupChannels.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MPListView listViewChannels;
    private MPColumnHeader columnHeaderChannelsTuningDetails;
    private MPColumnHeader columnHeaderChannelsName;
    private MPTabControl tabControl;
    private MPTabPage tabPageChannels;
    private System.Windows.Forms.ImageList imageList1;
    private MPColumnHeader columnHeaderChannelsProvider;
    private MPButton buttonChannelAdd;
    private MPButton buttonChannelEdit;
    private MPButton buttonChannelDelete;
    private MPButton buttonChannelPreview;
    private MPButton buttonChannelTest;
    private MPButton buttonChannelAddToGroup;
    private MPColumnHeader columnHeaderChannelsGroups;
    private MPTextBox textBoxFilter;
    private MPLabel labelFilter;
    private MPButton buttonChannelSplit;
    private MPButton buttonChannelMerge;
    private MPColumnHeader columnHeaderChannelsNumber;
    private MPTextBox textBoxChannelNumber;
    private MPTabPage tabPageChannelGroups;
    private MPGroupBox groupBoxGroup;
    private MPButton buttonGroupDelete;
    private MPButton buttonGroupRename;
    private MPGroupBox groupBoxGroupOrder;
    private MPLabel labelGroupOrderByName;
    private MPLabel labelGroupOrderByNumber;
    private MPLabel labelGroupOrderManual;
    private MPButton buttonGroupOrderByName;
    private MPButton buttonGroupOrderByNumber;
    private MPButton buttonGroupOrderDown;
    private MPButton buttonGroupOrderUp;
    private MPGroupBox groupBoxGroupChannels;
    private MPButton buttonGroupChannelsAdd;
    private MPButton buttonGroupChannelsRemove;
    private MPListView listViewChannelsInGroup;
    private MPColumnHeader columnHeaderChannelsInGroupName;
    private MPColumnHeader columnHeaderChannelsInGroupNumber;
    private MPButton buttonGroupOrder;
    private MPButton buttonGroupAdd;
    private MPComboBox comboBoxChannelGroup;
    private MPToolTip toolTip;
  }
}
