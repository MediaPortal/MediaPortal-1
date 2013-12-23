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
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      this.groupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.treeViewMenu = new System.Windows.Forms.TreeView();
      this.btAddFilter = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnSetDefaults = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnAddView = new MediaPortal.UserInterface.Controls.MPButton();
      this.dataGrid = new System.Windows.Forms.DataGridView();
      this.dgSelection = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgSortBy = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgViewAs = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgSkip = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.btnDelete = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox
      // 
      this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox.Controls.Add(this.treeViewMenu);
      this.groupBox.Controls.Add(this.btAddFilter);
      this.groupBox.Controls.Add(this.btnSetDefaults);
      this.groupBox.Controls.Add(this.btnAddView);
      this.groupBox.Controls.Add(this.dataGrid);
      this.groupBox.Controls.Add(this.btnDelete);
      this.groupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox.Location = new System.Drawing.Point(6, 0);
      this.groupBox.Name = "groupBox";
      this.groupBox.Size = new System.Drawing.Size(462, 408);
      this.groupBox.TabIndex = 0;
      this.groupBox.TabStop = false;
      // 
      // treeViewMenu
      // 
      this.treeViewMenu.AllowDrop = true;
      this.treeViewMenu.LabelEdit = true;
      this.treeViewMenu.Location = new System.Drawing.Point(16, 20);
      this.treeViewMenu.Name = "treeViewMenu";
      this.treeViewMenu.Size = new System.Drawing.Size(297, 200);
      this.treeViewMenu.TabIndex = 11;
      this.treeViewMenu.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewMenu_AfterLabelEdit);
      this.treeViewMenu.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewMenu_ItemDrag);
      this.treeViewMenu.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewMenu_DragDrop);
      this.treeViewMenu.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeViewMenu_DragEnter);
      // 
      // btAddFilter
      // 
      this.btAddFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btAddFilter.Location = new System.Drawing.Point(16, 367);
      this.btAddFilter.Name = "btAddFilter";
      this.btAddFilter.Size = new System.Drawing.Size(75, 23);
      this.btAddFilter.TabIndex = 10;
      this.btAddFilter.Text = "Add Filter";
      this.btAddFilter.UseVisualStyleBackColor = true;
      this.btAddFilter.Click += new System.EventHandler(this.btAddFilter_Click);
      // 
      // btnSetDefaults
      // 
      this.btnSetDefaults.Location = new System.Drawing.Point(319, 198);
      this.btnSetDefaults.Name = "btnSetDefaults";
      this.btnSetDefaults.Size = new System.Drawing.Size(83, 22);
      this.btnSetDefaults.TabIndex = 9;
      this.btnSetDefaults.Text = "Set defaults";
      this.btnSetDefaults.UseVisualStyleBackColor = true;
      this.btnSetDefaults.Click += new System.EventHandler(this.btnSetDefaults_Click);
      // 
      // btnAddView
      // 
      this.btnAddView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAddView.Location = new System.Drawing.Point(319, 20);
      this.btnAddView.Name = "btnAddView";
      this.btnAddView.Size = new System.Drawing.Size(83, 22);
      this.btnAddView.TabIndex = 5;
      this.btnAddView.Text = "Add View";
      this.btnAddView.UseVisualStyleBackColor = true;
      this.btnAddView.Click += new System.EventHandler(this.btnAdd_Click);
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
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.Color.LightSteelBlue;
      dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.dataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
      this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgSelection,
            this.dgSortBy,
            this.dgViewAs,
            this.dgSkip});
      this.dataGrid.Location = new System.Drawing.Point(16, 240);
      this.dataGrid.MultiSelect = false;
      this.dataGrid.Name = "dataGrid";
      this.dataGrid.RowHeadersVisible = false;
      this.dataGrid.Size = new System.Drawing.Size(440, 121);
      this.dataGrid.TabIndex = 8;
      this.dataGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGrid_OnCellPainting);
      this.dataGrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGrid_CurrentCellDirtyStateChanged);
      this.dataGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGrid_DataError);
      this.dataGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.dataGrid_OnDragDrop);
      this.dataGrid.DragOver += new System.Windows.Forms.DragEventHandler(this.dataGrid_OnDragOver);
      this.dataGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGrid_KeyDown);
      this.dataGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_OnMouseDown);
      this.dataGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_OnMouseMove);
      // 
      // dgSelection
      // 
      this.dgSelection.HeaderText = "Selection";
      this.dgSelection.Name = "dgSelection";
      this.dgSelection.ReadOnly = true;
      this.dgSelection.Width = 230;
      // 
      // dgSortBy
      // 
      this.dgSortBy.HeaderText = "SortBy";
      this.dgSortBy.Name = "dgSortBy";
      this.dgSortBy.ReadOnly = true;
      // 
      // dgViewAs
      // 
      this.dgViewAs.HeaderText = "Layout";
      this.dgViewAs.Name = "dgViewAs";
      this.dgViewAs.ToolTipText = "Select how the returned data should be shown";
      this.dgViewAs.Width = 70;
      // 
      // dgSkip
      // 
      this.dgSkip.FalseValue = "false";
      this.dgSkip.HeaderText = "Skip";
      this.dgSkip.Name = "dgSkip";
      this.dgSkip.ToolTipText = "Don\'t display this level, if only 1 row is returned";
      this.dgSkip.TrueValue = "true";
      this.dgSkip.Width = 31;
      // 
      // btnDelete
      // 
      this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDelete.Location = new System.Drawing.Point(319, 59);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(83, 22);
      this.btnDelete.TabIndex = 6;
      this.btnDelete.Text = "Delete View";
      this.btnDelete.UseVisualStyleBackColor = true;
      this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
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
    private MediaPortal.UserInterface.Controls.MPButton btnDelete;
    private System.Windows.Forms.DataGridView dataGrid;
    private MediaPortal.UserInterface.Controls.MPButton btnAddView;
    private UserInterface.Controls.MPButton btnSetDefaults;
    private UserInterface.Controls.MPButton btAddFilter;
    private System.Windows.Forms.TreeView treeViewMenu;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgSelection;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgSortBy;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgViewAs;
    private System.Windows.Forms.DataGridViewCheckBoxColumn dgSkip;
  }
}
