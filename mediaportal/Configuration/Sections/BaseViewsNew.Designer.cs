namespace MediaPortal.Configuration.Sections
{
  partial class BaseViewsNew
  {
    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseViewsNew));
      this.groupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.btnCopyView = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnDownView = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnUpView = new MediaPortal.UserInterface.Controls.MPButton();
      this.dataGrid = new System.Windows.Forms.DataGridView();
      this.lblActionCodes = new MediaPortal.UserInterface.Controls.MPLabel();
      this.treeViewMenu = new System.Windows.Forms.TreeView();
      this.btnEditFilter = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnSetDefaults = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnAddView = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnDeleteView = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.toolTipViewButtons = new System.Windows.Forms.ToolTip(this.components);
      this.dgSelection = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgViewAs = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgSortBy = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgSortAsc = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.dgSkip = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.dgColAdd = new System.Windows.Forms.DataGridViewImageColumn();
      this.dgColDelete = new System.Windows.Forms.DataGridViewImageColumn();
      this.groupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox
      // 
      this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox.Controls.Add(this.btnCopyView);
      this.groupBox.Controls.Add(this.btnDownView);
      this.groupBox.Controls.Add(this.btnUpView);
      this.groupBox.Controls.Add(this.dataGrid);
      this.groupBox.Controls.Add(this.lblActionCodes);
      this.groupBox.Controls.Add(this.treeViewMenu);
      this.groupBox.Controls.Add(this.btnEditFilter);
      this.groupBox.Controls.Add(this.btnSetDefaults);
      this.groupBox.Controls.Add(this.btnAddView);
      this.groupBox.Controls.Add(this.btnDeleteView);
      this.groupBox.Controls.Add(this.mpLabel1);
      this.groupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox.Location = new System.Drawing.Point(6, 0);
      this.groupBox.Name = "groupBox";
      this.groupBox.Size = new System.Drawing.Size(462, 408);
      this.groupBox.TabIndex = 0;
      this.groupBox.TabStop = false;
      // 
      // btnCopyView
      // 
      this.btnCopyView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCopyView.Enabled = false;
      this.btnCopyView.Image = global::MediaPortal.Configuration.Properties.Resources.icon_copy_view;
      this.btnCopyView.Location = new System.Drawing.Point(419, 175);
      this.btnCopyView.Name = "btnCopyView";
      this.btnCopyView.Size = new System.Drawing.Size(25, 25);
      this.btnCopyView.TabIndex = 16;
      this.toolTipViewButtons.SetToolTip(this.btnCopyView, "Copy View");
      this.btnCopyView.UseVisualStyleBackColor = false;
      this.btnCopyView.Click += new System.EventHandler(this.btnCopyView_Click);
      // 
      // btnDownView
      // 
      this.btnDownView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDownView.Enabled = false;
      this.btnDownView.Image = global::MediaPortal.Configuration.Properties.Resources.icon_down_view;
      this.btnDownView.Location = new System.Drawing.Point(419, 144);
      this.btnDownView.Name = "btnDownView";
      this.btnDownView.Size = new System.Drawing.Size(25, 25);
      this.btnDownView.TabIndex = 15;
      this.toolTipViewButtons.SetToolTip(this.btnDownView, "Move Down");
      this.btnDownView.UseVisualStyleBackColor = false;
      this.btnDownView.Click += new System.EventHandler(this.btnDownView_Click);
      // 
      // btnUpView
      // 
      this.btnUpView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnUpView.Enabled = false;
      this.btnUpView.Image = global::MediaPortal.Configuration.Properties.Resources.icon_up_view;
      this.btnUpView.Location = new System.Drawing.Point(419, 113);
      this.btnUpView.Name = "btnUpView";
      this.btnUpView.Size = new System.Drawing.Size(25, 25);
      this.btnUpView.TabIndex = 14;
      this.toolTipViewButtons.SetToolTip(this.btnUpView, "Move Up");
      this.btnUpView.UseVisualStyleBackColor = false;
      this.btnUpView.Click += new System.EventHandler(this.btnUpView_Click);
      // 
      // dataGrid
      // 
      this.dataGrid.AllowDrop = true;
      this.dataGrid.AllowUserToAddRows = false;
      this.dataGrid.AllowUserToDeleteRows = false;
      this.dataGrid.AllowUserToResizeColumns = false;
      this.dataGrid.AllowUserToResizeRows = false;
      this.dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGrid.BackgroundColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.LightSteelBlue;
      dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.dataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgSelection,
            this.dgViewAs,
            this.dgSortBy,
            this.dgSortAsc,
            this.dgSkip,
            this.dgColAdd,
            this.dgColDelete});
      this.dataGrid.Location = new System.Drawing.Point(16, 254);
      this.dataGrid.MultiSelect = false;
      this.dataGrid.Name = "dataGrid";
      this.dataGrid.RowHeadersVisible = false;
      this.dataGrid.Size = new System.Drawing.Size(433, 120);
      this.dataGrid.TabIndex = 13;
      this.dataGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGrid_CellClick);
      this.dataGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGrid_OnCellPainting);
      this.dataGrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGrid_CurrentCellDirtyStateChanged);
      this.dataGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGrid_DataError);
      this.dataGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.dataGrid_OnDragDrop);
      this.dataGrid.DragOver += new System.Windows.Forms.DragEventHandler(this.dataGrid_OnDragOver);
      this.dataGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_OnMouseDown);
      this.dataGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_OnMouseMove);
      // 
      // lblActionCodes
      // 
      this.lblActionCodes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblActionCodes.Location = new System.Drawing.Point(15, 373);
      this.lblActionCodes.Name = "lblActionCodes";
      this.lblActionCodes.Size = new System.Drawing.Size(430, 29);
      this.lblActionCodes.TabIndex = 12;
      this.lblActionCodes.Text = "Drag & Drop the rows to change order";
      this.lblActionCodes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // treeViewMenu
      // 
      this.treeViewMenu.AllowDrop = true;
      this.treeViewMenu.LabelEdit = true;
      this.treeViewMenu.Location = new System.Drawing.Point(16, 20);
      this.treeViewMenu.Name = "treeViewMenu";
      this.treeViewMenu.Size = new System.Drawing.Size(397, 211);
      this.treeViewMenu.TabIndex = 11;
      this.treeViewMenu.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewMenu_AfterLabelEdit);
      this.treeViewMenu.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewMenu_ItemDrag);
      this.treeViewMenu.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewMenu_BeforeSelect);
      this.treeViewMenu.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewMenu_AfterSelect);
      this.treeViewMenu.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewMenu_DragDrop);
      this.treeViewMenu.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeViewMenu_DragEnter);
      // 
      // btnEditFilter
      // 
      this.btnEditFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnEditFilter.Enabled = false;
      this.btnEditFilter.Image = global::MediaPortal.Configuration.Properties.Resources.icon_edit_view;
      this.btnEditFilter.Location = new System.Drawing.Point(419, 82);
      this.btnEditFilter.Name = "btnEditFilter";
      this.btnEditFilter.Size = new System.Drawing.Size(25, 25);
      this.btnEditFilter.TabIndex = 10;
      this.toolTipViewButtons.SetToolTip(this.btnEditFilter, "Edit Filter");
      this.btnEditFilter.UseVisualStyleBackColor = false;
      this.btnEditFilter.Click += new System.EventHandler(this.btEditFilter_Click);
      // 
      // btnSetDefaults
      // 
      this.btnSetDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSetDefaults.Image = global::MediaPortal.Configuration.Properties.Resources.icon_default_view;
      this.btnSetDefaults.Location = new System.Drawing.Point(419, 206);
      this.btnSetDefaults.Name = "btnSetDefaults";
      this.btnSetDefaults.Size = new System.Drawing.Size(25, 22);
      this.btnSetDefaults.TabIndex = 9;
      this.toolTipViewButtons.SetToolTip(this.btnSetDefaults, "Set default View");
      this.btnSetDefaults.UseVisualStyleBackColor = true;
      this.btnSetDefaults.Click += new System.EventHandler(this.btnSetDefaults_Click);
      // 
      // btnAddView
      // 
      this.btnAddView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAddView.Image = ((System.Drawing.Image)(resources.GetObject("btnAddView.Image")));
      this.btnAddView.Location = new System.Drawing.Point(419, 20);
      this.btnAddView.Name = "btnAddView";
      this.btnAddView.Size = new System.Drawing.Size(25, 25);
      this.btnAddView.TabIndex = 5;
      this.toolTipViewButtons.SetToolTip(this.btnAddView, "Add View");
      this.btnAddView.UseVisualStyleBackColor = true;
      this.btnAddView.Click += new System.EventHandler(this.btnAdd_Click);
      // 
      // btnDeleteView
      // 
      this.btnDeleteView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDeleteView.Enabled = false;
      this.btnDeleteView.Image = global::MediaPortal.Configuration.Properties.Resources.icon_delete_view;
      this.btnDeleteView.Location = new System.Drawing.Point(419, 51);
      this.btnDeleteView.Name = "btnDeleteView";
      this.btnDeleteView.Size = new System.Drawing.Size(25, 25);
      this.btnDeleteView.TabIndex = 6;
      this.toolTipViewButtons.SetToolTip(this.btnDeleteView, "Delete View");
      this.btnDeleteView.UseVisualStyleBackColor = true;
      this.btnDeleteView.Click += new System.EventHandler(this.btnDelete_Click);
      // 
      // mpLabel1
      // 
      this.mpLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mpLabel1.Location = new System.Drawing.Point(16, 227);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(430, 29);
      this.mpLabel1.TabIndex = 17;
      this.mpLabel1.Text = "Drag the rows to change order. Drag on a Root node to create a subview";
      this.mpLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // dgSelection
      // 
      this.dgSelection.HeaderText = "Selection";
      this.dgSelection.Name = "dgSelection";
      this.dgSelection.ToolTipText = "Select the field that should be retrieved from the database";
      this.dgSelection.Width = 140;
      // 
      // dgViewAs
      // 
      this.dgViewAs.HeaderText = "Layout";
      this.dgViewAs.Name = "dgViewAs";
      this.dgViewAs.ToolTipText = "Select how the returned data should be shown";
      this.dgViewAs.Width = 90;
      // 
      // dgSortBy
      // 
      this.dgSortBy.HeaderText = "SortBy";
      this.dgSortBy.Name = "dgSortBy";
      this.dgSortBy.ToolTipText = "Choose the sort field";
      this.dgSortBy.Width = 90;
      // 
      // dgSortAsc
      // 
      this.dgSortAsc.FalseValue = "false";
      this.dgSortAsc.HeaderText = "Asc";
      this.dgSortAsc.Name = "dgSortAsc";
      this.dgSortAsc.ToolTipText = "Chose sort direction";
      this.dgSortAsc.TrueValue = "true";
      this.dgSortAsc.Width = 30;
      // 
      // dgSkip
      // 
      this.dgSkip.FalseValue = "false";
      this.dgSkip.HeaderText = "Skip";
      this.dgSkip.Name = "dgSkip";
      this.dgSkip.ToolTipText = "Don\'t display this level, if only 1 row is returned";
      this.dgSkip.TrueValue = "true";
      this.dgSkip.Width = 30;
      // 
      // dgColAdd
      // 
      this.dgColAdd.HeaderText = "";
      this.dgColAdd.Image = global::MediaPortal.Configuration.Properties.Resources.icon_add_view;
      this.dgColAdd.Name = "dgColAdd";
      this.dgColAdd.Resizable = System.Windows.Forms.DataGridViewTriState.False;
      this.dgColAdd.ToolTipText = "Add line after selected";
      this.dgColAdd.Width = 25;
      // 
      // dgColDelete
      // 
      this.dgColDelete.HeaderText = "";
      this.dgColDelete.Image = global::MediaPortal.Configuration.Properties.Resources.icon_delete_view;
      this.dgColDelete.Name = "dgColDelete";
      this.dgColDelete.ToolTipText = "Delete line";
      this.dgColDelete.Width = 25;
      // 
      // BaseViewsNew
      // 
      this.Controls.Add(this.groupBox);
      this.Name = "BaseViewsNew";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox;
    private MediaPortal.UserInterface.Controls.MPButton btnDeleteView;
    private MediaPortal.UserInterface.Controls.MPButton btnAddView;
    private UserInterface.Controls.MPButton btnSetDefaults;
    private UserInterface.Controls.MPButton btnEditFilter;
    private System.Windows.Forms.TreeView treeViewMenu;
    private UserInterface.Controls.MPLabel lblActionCodes;
    private System.Windows.Forms.DataGridView dataGrid;
    private System.Windows.Forms.ToolTip toolTipViewButtons;
    private System.ComponentModel.IContainer components;
    private UserInterface.Controls.MPButton btnUpView;
    private UserInterface.Controls.MPButton btnCopyView;
    private UserInterface.Controls.MPButton btnDownView;
    private UserInterface.Controls.MPLabel mpLabel1;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgSelection;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgViewAs;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgSortBy;
    private System.Windows.Forms.DataGridViewCheckBoxColumn dgSortAsc;
    private System.Windows.Forms.DataGridViewCheckBoxColumn dgSkip;
    private System.Windows.Forms.DataGridViewImageColumn dgColAdd;
    private System.Windows.Forms.DataGridViewImageColumn dgColDelete;
  }
}
