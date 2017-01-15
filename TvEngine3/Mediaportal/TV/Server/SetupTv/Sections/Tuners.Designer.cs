using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class Tuners
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Tuners));
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.tabPageTunerGroups = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.groupBoxTunerInGroup = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.buttonTunerInGroupAdd = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonTunerInGroupRemove = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.groupBoxGroup = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.buttonGroupRename = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonGroupAdd = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonGroupDelete = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.treeViewTunerGroups = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTreeView();
      this.tabPageTuners = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.labelTunerPriority = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonTunerPriorityDown = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonTunerPriorityUp = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.numericUpDownStreamTunerCount = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.buttonTunerEdit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonTunerDelete = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelStreamTunerCount = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.listViewTuners = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderEnabled = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderId = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderType = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderName = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderPriority = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderConditionalAccess = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderGrabEpg = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderExternalId = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.tabControl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabControl();
      this.tabPageTunerGroups.SuspendLayout();
      this.groupBoxTunerInGroup.SuspendLayout();
      this.groupBoxGroup.SuspendLayout();
      this.tabPageTuners.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStreamTunerCount)).BeginInit();
      this.tabControl.SuspendLayout();
      this.SuspendLayout();
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "card.gif");
      // 
      // tabPageTunerGroups
      // 
      this.tabPageTunerGroups.Controls.Add(this.groupBoxTunerInGroup);
      this.tabPageTunerGroups.Controls.Add(this.groupBoxGroup);
      this.tabPageTunerGroups.Controls.Add(this.treeViewTunerGroups);
      this.tabPageTunerGroups.Location = new System.Drawing.Point(4, 22);
      this.tabPageTunerGroups.Name = "tabPageTunerGroups";
      this.tabPageTunerGroups.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageTunerGroups.Size = new System.Drawing.Size(472, 394);
      this.tabPageTunerGroups.TabIndex = 1;
      this.tabPageTunerGroups.Text = "Tuner Groups";
      // 
      // groupBoxTunerInGroup
      // 
      this.groupBoxTunerInGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxTunerInGroup.Controls.Add(this.buttonTunerInGroupAdd);
      this.groupBoxTunerInGroup.Controls.Add(this.buttonTunerInGroupRemove);
      this.groupBoxTunerInGroup.Location = new System.Drawing.Point(338, 338);
      this.groupBoxTunerInGroup.Name = "groupBoxTunerInGroup";
      this.groupBoxTunerInGroup.Size = new System.Drawing.Size(128, 50);
      this.groupBoxTunerInGroup.TabIndex = 2;
      this.groupBoxTunerInGroup.TabStop = false;
      this.groupBoxTunerInGroup.Text = "Tuner";
      // 
      // buttonTunerInGroupAdd
      // 
      this.buttonTunerInGroupAdd.Location = new System.Drawing.Point(6, 19);
      this.buttonTunerInGroupAdd.Name = "buttonTunerInGroupAdd";
      this.buttonTunerInGroupAdd.Size = new System.Drawing.Size(55, 23);
      this.buttonTunerInGroupAdd.TabIndex = 0;
      this.buttonTunerInGroupAdd.Text = "Add";
      this.buttonTunerInGroupAdd.Click += new System.EventHandler(this.buttonTunerInGroupAdd_Click);
      // 
      // buttonTunerInGroupRemove
      // 
      this.buttonTunerInGroupRemove.Enabled = false;
      this.buttonTunerInGroupRemove.Location = new System.Drawing.Point(67, 19);
      this.buttonTunerInGroupRemove.Name = "buttonTunerInGroupRemove";
      this.buttonTunerInGroupRemove.Size = new System.Drawing.Size(55, 23);
      this.buttonTunerInGroupRemove.TabIndex = 1;
      this.buttonTunerInGroupRemove.Text = "Remove";
      this.buttonTunerInGroupRemove.Click += new System.EventHandler(this.buttonTunerInGroupRemove_Click);
      // 
      // groupBoxGroup
      // 
      this.groupBoxGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.groupBoxGroup.Controls.Add(this.buttonGroupRename);
      this.groupBoxGroup.Controls.Add(this.buttonGroupAdd);
      this.groupBoxGroup.Controls.Add(this.buttonGroupDelete);
      this.groupBoxGroup.Location = new System.Drawing.Point(6, 338);
      this.groupBoxGroup.Name = "groupBoxGroup";
      this.groupBoxGroup.Size = new System.Drawing.Size(189, 50);
      this.groupBoxGroup.TabIndex = 1;
      this.groupBoxGroup.TabStop = false;
      this.groupBoxGroup.Text = "Group";
      // 
      // buttonGroupRename
      // 
      this.buttonGroupRename.Location = new System.Drawing.Point(67, 19);
      this.buttonGroupRename.Name = "buttonGroupRename";
      this.buttonGroupRename.Size = new System.Drawing.Size(55, 23);
      this.buttonGroupRename.TabIndex = 1;
      this.buttonGroupRename.Text = "&Rename";
      this.buttonGroupRename.Click += new System.EventHandler(this.buttonGroupRename_Click);
      // 
      // buttonGroupAdd
      // 
      this.buttonGroupAdd.Location = new System.Drawing.Point(6, 19);
      this.buttonGroupAdd.Name = "buttonGroupAdd";
      this.buttonGroupAdd.Size = new System.Drawing.Size(55, 23);
      this.buttonGroupAdd.TabIndex = 0;
      this.buttonGroupAdd.Text = "&Add";
      this.buttonGroupAdd.Click += new System.EventHandler(this.buttonGroupAdd_Click);
      // 
      // buttonGroupDelete
      // 
      this.buttonGroupDelete.Location = new System.Drawing.Point(128, 19);
      this.buttonGroupDelete.Name = "buttonGroupDelete";
      this.buttonGroupDelete.Size = new System.Drawing.Size(55, 23);
      this.buttonGroupDelete.TabIndex = 2;
      this.buttonGroupDelete.Text = "&Delete";
      this.buttonGroupDelete.Click += new System.EventHandler(this.buttonGroupDelete_Click);
      // 
      // treeViewTunerGroups
      // 
      this.treeViewTunerGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.treeViewTunerGroups.ImageIndex = 0;
      this.treeViewTunerGroups.ImageList = this.imageList1;
      this.treeViewTunerGroups.Location = new System.Drawing.Point(6, 6);
      this.treeViewTunerGroups.Name = "treeViewTunerGroups";
      this.treeViewTunerGroups.SelectedImageIndex = 0;
      this.treeViewTunerGroups.Size = new System.Drawing.Size(460, 326);
      this.treeViewTunerGroups.TabIndex = 0;
      this.treeViewTunerGroups.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewTunerGroups_AfterSelect);
      this.treeViewTunerGroups.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeViewTunerGroups_KeyDown);
      // 
      // tabPageTuners
      // 
      this.tabPageTuners.Controls.Add(this.labelTunerPriority);
      this.tabPageTuners.Controls.Add(this.buttonTunerPriorityDown);
      this.tabPageTuners.Controls.Add(this.buttonTunerPriorityUp);
      this.tabPageTuners.Controls.Add(this.numericUpDownStreamTunerCount);
      this.tabPageTuners.Controls.Add(this.buttonTunerEdit);
      this.tabPageTuners.Controls.Add(this.buttonTunerDelete);
      this.tabPageTuners.Controls.Add(this.labelStreamTunerCount);
      this.tabPageTuners.Controls.Add(this.listViewTuners);
      this.tabPageTuners.Location = new System.Drawing.Point(4, 22);
      this.tabPageTuners.Name = "tabPageTuners";
      this.tabPageTuners.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageTuners.Size = new System.Drawing.Size(472, 394);
      this.tabPageTuners.TabIndex = 0;
      this.tabPageTuners.Text = "Tuners";
      // 
      // labelTunerPriority
      // 
      this.labelTunerPriority.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.labelTunerPriority.AutoSize = true;
      this.labelTunerPriority.Location = new System.Drawing.Point(211, 366);
      this.labelTunerPriority.Name = "labelTunerPriority";
      this.labelTunerPriority.Size = new System.Drawing.Size(41, 13);
      this.labelTunerPriority.TabIndex = 3;
      this.labelTunerPriority.Text = "Priority:";
      // 
      // buttonTunerPriorityDown
      // 
      this.buttonTunerPriorityDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonTunerPriorityDown.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_down;
      this.buttonTunerPriorityDown.Location = new System.Drawing.Point(294, 361);
      this.buttonTunerPriorityDown.Name = "buttonTunerPriorityDown";
      this.buttonTunerPriorityDown.Size = new System.Drawing.Size(30, 23);
      this.buttonTunerPriorityDown.TabIndex = 5;
      this.buttonTunerPriorityDown.Click += new System.EventHandler(this.buttonTunerPriorityDown_Click);
      // 
      // buttonTunerPriorityUp
      // 
      this.buttonTunerPriorityUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonTunerPriorityUp.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_up;
      this.buttonTunerPriorityUp.Location = new System.Drawing.Point(258, 361);
      this.buttonTunerPriorityUp.Name = "buttonTunerPriorityUp";
      this.buttonTunerPriorityUp.Size = new System.Drawing.Size(30, 23);
      this.buttonTunerPriorityUp.TabIndex = 4;
      this.buttonTunerPriorityUp.Click += new System.EventHandler(this.buttonTunerPriorityUp_Click);
      // 
      // numericUpDownStreamTunerCount
      // 
      this.numericUpDownStreamTunerCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.numericUpDownStreamTunerCount.Location = new System.Drawing.Point(140, 364);
      this.numericUpDownStreamTunerCount.Name = "numericUpDownStreamTunerCount";
      this.numericUpDownStreamTunerCount.Size = new System.Drawing.Size(47, 20);
      this.numericUpDownStreamTunerCount.TabIndex = 2;
      // 
      // buttonTunerEdit
      // 
      this.buttonTunerEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonTunerEdit.Enabled = false;
      this.buttonTunerEdit.Location = new System.Drawing.Point(350, 361);
      this.buttonTunerEdit.Name = "buttonTunerEdit";
      this.buttonTunerEdit.Size = new System.Drawing.Size(55, 23);
      this.buttonTunerEdit.TabIndex = 6;
      this.buttonTunerEdit.Text = "&Edit";
      this.buttonTunerEdit.Click += new System.EventHandler(this.buttonTunerEdit_Click);
      // 
      // buttonTunerDelete
      // 
      this.buttonTunerDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonTunerDelete.Enabled = false;
      this.buttonTunerDelete.Location = new System.Drawing.Point(411, 361);
      this.buttonTunerDelete.Name = "buttonTunerDelete";
      this.buttonTunerDelete.Size = new System.Drawing.Size(55, 23);
      this.buttonTunerDelete.TabIndex = 7;
      this.buttonTunerDelete.Text = "&Delete";
      this.buttonTunerDelete.Click += new System.EventHandler(this.buttonTunerDelete_Click);
      // 
      // labelStreamTunerCount
      // 
      this.labelStreamTunerCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.labelStreamTunerCount.AutoSize = true;
      this.labelStreamTunerCount.Location = new System.Drawing.Point(3, 358);
      this.labelStreamTunerCount.Name = "labelStreamTunerCount";
      this.labelStreamTunerCount.Size = new System.Drawing.Size(131, 26);
      this.labelStreamTunerCount.TabIndex = 1;
      this.labelStreamTunerCount.Text = "Number of virtual Internet/\r\nnetwork stream tuners:";
      // 
      // listViewTuners
      // 
      this.listViewTuners.AllowColumnReorder = true;
      this.listViewTuners.AllowDrop = true;
      this.listViewTuners.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewTuners.CheckBoxes = true;
      this.listViewTuners.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderEnabled,
            this.columnHeaderId,
            this.columnHeaderType,
            this.columnHeaderName,
            this.columnHeaderPriority,
            this.columnHeaderConditionalAccess,
            this.columnHeaderGrabEpg,
            this.columnHeaderExternalId});
      this.listViewTuners.FullRowSelect = true;
      this.listViewTuners.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.listViewTuners.HideSelection = false;
      this.listViewTuners.Location = new System.Drawing.Point(6, 6);
      this.listViewTuners.Name = "listViewTuners";
      this.listViewTuners.Size = new System.Drawing.Size(460, 344);
      this.listViewTuners.TabIndex = 0;
      this.listViewTuners.UseCompatibleStateImageBehavior = false;
      this.listViewTuners.View = System.Windows.Forms.View.Details;
      this.listViewTuners.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewTuners_ItemChecked);
      this.listViewTuners.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listViewTuners_ItemDrag);
      this.listViewTuners.SelectedIndexChanged += new System.EventHandler(this.listViewTuners_SelectedIndexChanged);
      this.listViewTuners.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewTuners_DragDrop);
      this.listViewTuners.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewTuners_DragEnter);
      this.listViewTuners.DragOver += new System.Windows.Forms.DragEventHandler(this.listViewTuners_DragOver);
      this.listViewTuners.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewTuners_KeyDown);
      this.listViewTuners.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewTuners_MouseDoubleClick);
      // 
      // columnHeaderEnabled
      // 
      this.columnHeaderEnabled.Text = "Enabled";
      this.columnHeaderEnabled.Width = 65;
      // 
      // columnHeaderId
      // 
      this.columnHeaderId.Text = "ID";
      this.columnHeaderId.Width = 35;
      // 
      // columnHeaderType
      // 
      this.columnHeaderType.Text = "Type";
      this.columnHeaderType.Width = 50;
      // 
      // columnHeaderName
      // 
      this.columnHeaderName.Text = "Name";
      this.columnHeaderName.Width = 130;
      // 
      // columnHeaderPriority
      // 
      this.columnHeaderPriority.Text = "Priority";
      this.columnHeaderPriority.Width = 50;
      // 
      // columnHeaderConditionalAccess
      // 
      this.columnHeaderConditionalAccess.Text = "CA";
      this.columnHeaderConditionalAccess.Width = 65;
      // 
      // columnHeaderGrabEpg
      // 
      this.columnHeaderGrabEpg.Text = "Grab EPG";
      this.columnHeaderGrabEpg.Width = 65;
      // 
      // columnHeaderExternalId
      // 
      this.columnHeaderExternalId.Text = "External ID";
      // 
      // tabControl
      // 
      this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl.Controls.Add(this.tabPageTuners);
      this.tabControl.Controls.Add(this.tabPageTunerGroups);
      this.tabControl.Location = new System.Drawing.Point(0, 0);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(480, 420);
      this.tabControl.TabIndex = 0;
      this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // Tuners
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.tabControl);
      this.Name = "Tuners";
      this.Size = new System.Drawing.Size(480, 420);
      this.tabPageTunerGroups.ResumeLayout(false);
      this.groupBoxTunerInGroup.ResumeLayout(false);
      this.groupBoxGroup.ResumeLayout(false);
      this.tabPageTuners.ResumeLayout(false);
      this.tabPageTuners.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStreamTunerCount)).EndInit();
      this.tabControl.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ImageList imageList1;
    private MPTabPage tabPageTunerGroups;
    private MPTreeView treeViewTunerGroups;
    private MPTabPage tabPageTuners;
    private MPNumericUpDown numericUpDownStreamTunerCount;
    private MPLabel labelStreamTunerCount;
    private MPButton buttonTunerDelete;
    private MPListView listViewTuners;
    private MPColumnHeader columnHeaderEnabled;
    private MPColumnHeader columnHeaderType;
    private MPColumnHeader columnHeaderConditionalAccess;
    private MPColumnHeader columnHeaderId;
    private MPColumnHeader columnHeaderName;
    private MPColumnHeader columnHeaderGrabEpg;
    private MPColumnHeader columnHeaderExternalId;
    private MPButton buttonTunerEdit;
    private MPButton buttonTunerPriorityUp;
    private MPButton buttonTunerPriorityDown;
    private MPTabControl tabControl;
    private MPGroupBox groupBoxTunerInGroup;
    private MPButton buttonTunerInGroupAdd;
    private MPButton buttonTunerInGroupRemove;
    private MPGroupBox groupBoxGroup;
    private MPButton buttonGroupAdd;
    private MPButton buttonGroupDelete;
    private MPButton buttonGroupRename;
    private MPLabel labelTunerPriority;
    private MPColumnHeader columnHeaderPriority;
  }
}
