using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class TvCards
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TvCards));
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.tabPageTunerGroups = new System.Windows.Forms.TabPage();
      this.groupBoxTunerInGroup = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.buttonTunerInGroupAdd = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonTunerInGroupRemove = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.groupBoxGroup = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.buttonGroupRename = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonGroupAdd = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonGroupDelete = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelTunerGroupExplanation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.treeViewTunerGroups = new System.Windows.Forms.TreeView();
      this.tabPageTuners = new System.Windows.Forms.TabPage();
      this.groupBoxTuner = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.buttonTunerEdit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonTunerDelete = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.groupBoxStreamTuners = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelStreamTunerExplanation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownStreamTunerCount = new System.Windows.Forms.NumericUpDown();
      this.labelStreamTunerCount = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.listViewTuners = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeaderId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeaderUseConditionalAccess = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeaderCaLimit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeaderGrabEpg = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeaderExternalId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.groupBoxPriority = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.buttonTunerPriorityUp = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonTunerPriorityDown = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPageTunerGroups.SuspendLayout();
      this.groupBoxTunerInGroup.SuspendLayout();
      this.groupBoxGroup.SuspendLayout();
      this.tabPageTuners.SuspendLayout();
      this.groupBoxTuner.SuspendLayout();
      this.groupBoxStreamTuners.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStreamTunerCount)).BeginInit();
      this.groupBoxPriority.SuspendLayout();
      this.tabControl1.SuspendLayout();
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
      this.tabPageTunerGroups.Controls.Add(this.labelTunerGroupExplanation);
      this.tabPageTunerGroups.Controls.Add(this.treeViewTunerGroups);
      this.tabPageTunerGroups.Location = new System.Drawing.Point(4, 22);
      this.tabPageTunerGroups.Name = "tabPageTunerGroups";
      this.tabPageTunerGroups.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageTunerGroups.Size = new System.Drawing.Size(472, 394);
      this.tabPageTunerGroups.TabIndex = 1;
      this.tabPageTunerGroups.Text = "Tuner Groups";
      this.tabPageTunerGroups.UseVisualStyleBackColor = true;
      // 
      // groupBoxTunerInGroup
      // 
      this.groupBoxTunerInGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxTunerInGroup.Controls.Add(this.buttonTunerInGroupAdd);
      this.groupBoxTunerInGroup.Controls.Add(this.buttonTunerInGroupRemove);
      this.groupBoxTunerInGroup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxTunerInGroup.Location = new System.Drawing.Point(328, 338);
      this.groupBoxTunerInGroup.Name = "groupBoxTunerInGroup";
      this.groupBoxTunerInGroup.Size = new System.Drawing.Size(138, 50);
      this.groupBoxTunerInGroup.TabIndex = 3;
      this.groupBoxTunerInGroup.TabStop = false;
      this.groupBoxTunerInGroup.Text = "Tuner";
      // 
      // buttonTunerInGroupAdd
      // 
      this.buttonTunerInGroupAdd.Location = new System.Drawing.Point(6, 19);
      this.buttonTunerInGroupAdd.Name = "buttonTunerInGroupAdd";
      this.buttonTunerInGroupAdd.Size = new System.Drawing.Size(60, 23);
      this.buttonTunerInGroupAdd.TabIndex = 0;
      this.buttonTunerInGroupAdd.Text = "Add";
      this.buttonTunerInGroupAdd.UseVisualStyleBackColor = true;
      this.buttonTunerInGroupAdd.Click += new System.EventHandler(this.buttonTunerInGroupAdd_Click);
      // 
      // buttonTunerInGroupRemove
      // 
      this.buttonTunerInGroupRemove.Enabled = false;
      this.buttonTunerInGroupRemove.Location = new System.Drawing.Point(72, 19);
      this.buttonTunerInGroupRemove.Name = "buttonTunerInGroupRemove";
      this.buttonTunerInGroupRemove.Size = new System.Drawing.Size(60, 23);
      this.buttonTunerInGroupRemove.TabIndex = 1;
      this.buttonTunerInGroupRemove.Text = "Remove";
      this.buttonTunerInGroupRemove.UseVisualStyleBackColor = true;
      this.buttonTunerInGroupRemove.Click += new System.EventHandler(this.buttonTunerInGroupRemove_Click);
      // 
      // groupBoxGroup
      // 
      this.groupBoxGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.groupBoxGroup.Controls.Add(this.buttonGroupRename);
      this.groupBoxGroup.Controls.Add(this.buttonGroupAdd);
      this.groupBoxGroup.Controls.Add(this.buttonGroupDelete);
      this.groupBoxGroup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGroup.Location = new System.Drawing.Point(6, 338);
      this.groupBoxGroup.Name = "groupBoxGroup";
      this.groupBoxGroup.Size = new System.Drawing.Size(204, 50);
      this.groupBoxGroup.TabIndex = 2;
      this.groupBoxGroup.TabStop = false;
      this.groupBoxGroup.Text = "Group";
      // 
      // buttonGroupRename
      // 
      this.buttonGroupRename.Location = new System.Drawing.Point(72, 19);
      this.buttonGroupRename.Name = "buttonGroupRename";
      this.buttonGroupRename.Size = new System.Drawing.Size(60, 23);
      this.buttonGroupRename.TabIndex = 1;
      this.buttonGroupRename.Text = "Rename";
      this.buttonGroupRename.UseVisualStyleBackColor = true;
      this.buttonGroupRename.Click += new System.EventHandler(this.buttonGroupRename_Click);
      // 
      // buttonGroupAdd
      // 
      this.buttonGroupAdd.Location = new System.Drawing.Point(6, 19);
      this.buttonGroupAdd.Name = "buttonGroupAdd";
      this.buttonGroupAdd.Size = new System.Drawing.Size(60, 23);
      this.buttonGroupAdd.TabIndex = 0;
      this.buttonGroupAdd.Text = "Add";
      this.buttonGroupAdd.UseVisualStyleBackColor = true;
      this.buttonGroupAdd.Click += new System.EventHandler(this.buttonGroupAdd_Click);
      // 
      // buttonGroupDelete
      // 
      this.buttonGroupDelete.Location = new System.Drawing.Point(138, 19);
      this.buttonGroupDelete.Name = "buttonGroupDelete";
      this.buttonGroupDelete.Size = new System.Drawing.Size(60, 23);
      this.buttonGroupDelete.TabIndex = 2;
      this.buttonGroupDelete.Text = "Delete";
      this.buttonGroupDelete.UseVisualStyleBackColor = true;
      this.buttonGroupDelete.Click += new System.EventHandler(this.buttonGroupDelete_Click);
      // 
      // labelTunerGroupExplanation
      // 
      this.labelTunerGroupExplanation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelTunerGroupExplanation.Location = new System.Drawing.Point(6, 5);
      this.labelTunerGroupExplanation.Name = "labelTunerGroupExplanation";
      this.labelTunerGroupExplanation.Size = new System.Drawing.Size(460, 121);
      this.labelTunerGroupExplanation.TabIndex = 0;
      this.labelTunerGroupExplanation.Text = resources.GetString("labelTunerGroupExplanation.Text");
      // 
      // treeViewTunerGroups
      // 
      this.treeViewTunerGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.treeViewTunerGroups.ImageIndex = 0;
      this.treeViewTunerGroups.ImageList = this.imageList1;
      this.treeViewTunerGroups.Location = new System.Drawing.Point(6, 136);
      this.treeViewTunerGroups.Name = "treeViewTunerGroups";
      this.treeViewTunerGroups.SelectedImageIndex = 0;
      this.treeViewTunerGroups.Size = new System.Drawing.Size(460, 196);
      this.treeViewTunerGroups.TabIndex = 1;
      this.treeViewTunerGroups.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewTunerGroups_AfterSelect);
      // 
      // tabPageTuners
      // 
      this.tabPageTuners.Controls.Add(this.groupBoxTuner);
      this.tabPageTuners.Controls.Add(this.groupBoxStreamTuners);
      this.tabPageTuners.Controls.Add(this.listViewTuners);
      this.tabPageTuners.Controls.Add(this.groupBoxPriority);
      this.tabPageTuners.Location = new System.Drawing.Point(4, 22);
      this.tabPageTuners.Name = "tabPageTuners";
      this.tabPageTuners.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageTuners.Size = new System.Drawing.Size(472, 394);
      this.tabPageTuners.TabIndex = 0;
      this.tabPageTuners.Text = "Tuners";
      this.tabPageTuners.UseVisualStyleBackColor = true;
      // 
      // groupBoxTuner
      // 
      this.groupBoxTuner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxTuner.Controls.Add(this.buttonTunerEdit);
      this.groupBoxTuner.Controls.Add(this.buttonTunerDelete);
      this.groupBoxTuner.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxTuner.Location = new System.Drawing.Point(328, 338);
      this.groupBoxTuner.Name = "groupBoxTuner";
      this.groupBoxTuner.Size = new System.Drawing.Size(138, 50);
      this.groupBoxTuner.TabIndex = 3;
      this.groupBoxTuner.TabStop = false;
      this.groupBoxTuner.Text = "Tuner";
      // 
      // buttonTunerEdit
      // 
      this.buttonTunerEdit.Enabled = false;
      this.buttonTunerEdit.Location = new System.Drawing.Point(6, 19);
      this.buttonTunerEdit.Name = "buttonTunerEdit";
      this.buttonTunerEdit.Size = new System.Drawing.Size(60, 23);
      this.buttonTunerEdit.TabIndex = 0;
      this.buttonTunerEdit.Text = "Edit";
      this.buttonTunerEdit.UseVisualStyleBackColor = true;
      this.buttonTunerEdit.Click += new System.EventHandler(this.buttonTunerEdit_Click);
      // 
      // buttonTunerDelete
      // 
      this.buttonTunerDelete.Enabled = false;
      this.buttonTunerDelete.Location = new System.Drawing.Point(72, 19);
      this.buttonTunerDelete.Name = "buttonTunerDelete";
      this.buttonTunerDelete.Size = new System.Drawing.Size(60, 23);
      this.buttonTunerDelete.TabIndex = 1;
      this.buttonTunerDelete.Text = "Delete";
      this.buttonTunerDelete.UseVisualStyleBackColor = true;
      this.buttonTunerDelete.Click += new System.EventHandler(this.buttonTunerDelete_Click);
      // 
      // groupBoxStreamTuners
      // 
      this.groupBoxStreamTuners.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxStreamTuners.Controls.Add(this.labelStreamTunerExplanation);
      this.groupBoxStreamTuners.Controls.Add(this.numericUpDownStreamTunerCount);
      this.groupBoxStreamTuners.Controls.Add(this.labelStreamTunerCount);
      this.groupBoxStreamTuners.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxStreamTuners.Location = new System.Drawing.Point(6, 6);
      this.groupBoxStreamTuners.Name = "groupBoxStreamTuners";
      this.groupBoxStreamTuners.Size = new System.Drawing.Size(460, 73);
      this.groupBoxStreamTuners.TabIndex = 0;
      this.groupBoxStreamTuners.TabStop = false;
      this.groupBoxStreamTuners.Text = "Internet/Network Stream Tuners";
      // 
      // labelStreamTunerExplanation
      // 
      this.labelStreamTunerExplanation.Location = new System.Drawing.Point(6, 16);
      this.labelStreamTunerExplanation.Name = "labelStreamTunerExplanation";
      this.labelStreamTunerExplanation.Size = new System.Drawing.Size(446, 28);
      this.labelStreamTunerExplanation.TabIndex = 0;
      this.labelStreamTunerExplanation.Text = "If you have enough internet and/or network bandwidth you can enable multiple stre" +
          "am tuners so that you can view and record multiple channels simultaneously.";
      // 
      // numericUpDownStreamTunerCount
      // 
      this.numericUpDownStreamTunerCount.Location = new System.Drawing.Point(218, 47);
      this.numericUpDownStreamTunerCount.Name = "numericUpDownStreamTunerCount";
      this.numericUpDownStreamTunerCount.Size = new System.Drawing.Size(47, 20);
      this.numericUpDownStreamTunerCount.TabIndex = 2;
      this.numericUpDownStreamTunerCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // labelStreamTunerCount
      // 
      this.labelStreamTunerCount.AutoSize = true;
      this.labelStreamTunerCount.Location = new System.Drawing.Point(6, 49);
      this.labelStreamTunerCount.Name = "labelStreamTunerCount";
      this.labelStreamTunerCount.Size = new System.Drawing.Size(206, 13);
      this.labelStreamTunerCount.TabIndex = 1;
      this.labelStreamTunerCount.Text = "Number of internet/network stream tuners:";
      // 
      // listViewTuners
      // 
      this.listViewTuners.AllowDrop = true;
      this.listViewTuners.AllowRowReorder = false;
      this.listViewTuners.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewTuners.CheckBoxes = true;
      this.listViewTuners.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderEnabled,
            this.columnHeaderId,
            this.columnHeaderType,
            this.columnHeaderName,
            this.columnHeaderUseConditionalAccess,
            this.columnHeaderCaLimit,
            this.columnHeaderGrabEpg,
            this.columnHeaderExternalId});
      this.listViewTuners.FullRowSelect = true;
      this.listViewTuners.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.listViewTuners.HideSelection = false;
      this.listViewTuners.IsChannelListView = false;
      this.listViewTuners.Location = new System.Drawing.Point(6, 85);
      this.listViewTuners.Name = "listViewTuners";
      this.listViewTuners.Size = new System.Drawing.Size(460, 247);
      this.listViewTuners.TabIndex = 1;
      this.listViewTuners.UseCompatibleStateImageBehavior = false;
      this.listViewTuners.View = System.Windows.Forms.View.Details;
      this.listViewTuners.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewTuners_ItemChecked);
      this.listViewTuners.SelectedIndexChanged += new System.EventHandler(this.listViewTuners_SelectedIndexChanged);
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
      // columnHeaderUseConditionalAccess
      // 
      this.columnHeaderUseConditionalAccess.Text = "CA";
      // 
      // columnHeaderCaLimit
      // 
      this.columnHeaderCaLimit.Text = "CA Limit";
      this.columnHeaderCaLimit.Width = 65;
      // 
      // columnHeaderGrabEpg
      // 
      this.columnHeaderGrabEpg.Text = "Grab EPG";
      // 
      // columnHeaderExternalId
      // 
      this.columnHeaderExternalId.Text = "External ID";
      // 
      // groupBoxPriority
      // 
      this.groupBoxPriority.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.groupBoxPriority.Controls.Add(this.buttonTunerPriorityUp);
      this.groupBoxPriority.Controls.Add(this.buttonTunerPriorityDown);
      this.groupBoxPriority.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxPriority.Location = new System.Drawing.Point(6, 338);
      this.groupBoxPriority.Name = "groupBoxPriority";
      this.groupBoxPriority.Size = new System.Drawing.Size(138, 50);
      this.groupBoxPriority.TabIndex = 2;
      this.groupBoxPriority.TabStop = false;
      this.groupBoxPriority.Text = "Priority";
      // 
      // buttonTunerPriorityUp
      // 
      this.buttonTunerPriorityUp.Location = new System.Drawing.Point(6, 19);
      this.buttonTunerPriorityUp.Name = "buttonTunerPriorityUp";
      this.buttonTunerPriorityUp.Size = new System.Drawing.Size(60, 23);
      this.buttonTunerPriorityUp.TabIndex = 0;
      this.buttonTunerPriorityUp.Text = "Up";
      this.buttonTunerPriorityUp.UseVisualStyleBackColor = true;
      this.buttonTunerPriorityUp.Click += new System.EventHandler(this.buttonTunerPriorityUp_Click);
      // 
      // buttonTunerPriorityDown
      // 
      this.buttonTunerPriorityDown.Location = new System.Drawing.Point(72, 19);
      this.buttonTunerPriorityDown.Name = "buttonTunerPriorityDown";
      this.buttonTunerPriorityDown.Size = new System.Drawing.Size(60, 23);
      this.buttonTunerPriorityDown.TabIndex = 1;
      this.buttonTunerPriorityDown.Text = "Down";
      this.buttonTunerPriorityDown.UseVisualStyleBackColor = true;
      this.buttonTunerPriorityDown.Click += new System.EventHandler(this.buttonTunerPriorityDown_Click);
      // 
      // tabControl1
      // 
      this.tabControl1.AccessibleName = "";
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPageTuners);
      this.tabControl1.Controls.Add(this.tabPageTunerGroups);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(480, 420);
      this.tabControl1.TabIndex = 0;
      // 
      // TvCards
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "TvCards";
      this.Size = new System.Drawing.Size(480, 420);
      this.tabPageTunerGroups.ResumeLayout(false);
      this.groupBoxTunerInGroup.ResumeLayout(false);
      this.groupBoxGroup.ResumeLayout(false);
      this.tabPageTuners.ResumeLayout(false);
      this.groupBoxTuner.ResumeLayout(false);
      this.groupBoxStreamTuners.ResumeLayout(false);
      this.groupBoxStreamTuners.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStreamTunerCount)).EndInit();
      this.groupBoxPriority.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.TabPage tabPageTunerGroups;
    private System.Windows.Forms.TreeView treeViewTunerGroups;
    private System.Windows.Forms.TabPage tabPageTuners;
    private MPGroupBox groupBoxStreamTuners;
    private MPLabel labelStreamTunerExplanation;
    private System.Windows.Forms.NumericUpDown numericUpDownStreamTunerCount;
    private MPLabel labelStreamTunerCount;
    private MPButton buttonTunerDelete;
    private MPListView listViewTuners;
    private System.Windows.Forms.ColumnHeader columnHeaderEnabled;
    private System.Windows.Forms.ColumnHeader columnHeaderType;
    private System.Windows.Forms.ColumnHeader columnHeaderUseConditionalAccess;
    private System.Windows.Forms.ColumnHeader columnHeaderCaLimit;
    private System.Windows.Forms.ColumnHeader columnHeaderId;
    private System.Windows.Forms.ColumnHeader columnHeaderName;
    private System.Windows.Forms.ColumnHeader columnHeaderGrabEpg;
    private System.Windows.Forms.ColumnHeader columnHeaderExternalId;
    private MPButton buttonTunerEdit;
    private MPButton buttonTunerPriorityUp;
    private MPButton buttonTunerPriorityDown;
    private System.Windows.Forms.TabControl tabControl1;
    private MPLabel labelTunerGroupExplanation;
    private MPGroupBox groupBoxPriority;
    private MPGroupBox groupBoxTuner;
    private MPGroupBox groupBoxTunerInGroup;
    private MPButton buttonTunerInGroupAdd;
    private MPButton buttonTunerInGroupRemove;
    private MPGroupBox groupBoxGroup;
    private MPButton buttonGroupAdd;
    private MPButton buttonGroupDelete;
    private MPButton buttonGroupRename;
  }
}
