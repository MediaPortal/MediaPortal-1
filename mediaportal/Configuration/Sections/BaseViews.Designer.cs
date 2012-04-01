namespace MediaPortal.Configuration.Sections
{
  partial class BaseViews
  {
    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      this.groupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.btnSetDefaults = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.dataGridViews = new System.Windows.Forms.DataGridView();
      this.dgViewName = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dgLocalisedName = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGrid = new System.Windows.Forms.DataGridView();
      this.dgSelection = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgOperator = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgRestriction = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dgLimit = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dgViewAs = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgSortBy = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgAsc = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.dgSkip = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.lblActionCodes = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnDelete = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViews)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox
      // 
      this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox.Controls.Add(this.btnSetDefaults);
      this.groupBox.Controls.Add(this.btnAdd);
      this.groupBox.Controls.Add(this.dataGridViews);
      this.groupBox.Controls.Add(this.dataGrid);
      this.groupBox.Controls.Add(this.lblActionCodes);
      this.groupBox.Controls.Add(this.btnDelete);
      this.groupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox.Location = new System.Drawing.Point(6, 0);
      this.groupBox.Name = "groupBox";
      this.groupBox.Size = new System.Drawing.Size(462, 408);
      this.groupBox.TabIndex = 0;
      this.groupBox.TabStop = false;
      // 
      // btnSetDefaults
      // 
      this.btnSetDefaults.Location = new System.Drawing.Point(28, 172);
      this.btnSetDefaults.Name = "btnSetDefaults";
      this.btnSetDefaults.Size = new System.Drawing.Size(83, 22);
      this.btnSetDefaults.TabIndex = 9;
      this.btnSetDefaults.Text = "Set defaults";
      this.btnSetDefaults.UseVisualStyleBackColor = true;
      this.btnSetDefaults.Click += new System.EventHandler(this.btnSetDefaults_Click);
      // 
      // btnAdd
      // 
      this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAdd.Location = new System.Drawing.Point(274, 172);
      this.btnAdd.Name = "btnAdd";
      this.btnAdd.Size = new System.Drawing.Size(83, 22);
      this.btnAdd.TabIndex = 5;
      this.btnAdd.Text = "Add View";
      this.btnAdd.UseVisualStyleBackColor = true;
      this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
      // 
      // dataGridViews
      // 
      this.dataGridViews.AllowDrop = true;
      this.dataGridViews.AllowUserToAddRows = false;
      this.dataGridViews.AllowUserToDeleteRows = false;
      this.dataGridViews.AllowUserToResizeColumns = false;
      this.dataGridViews.AllowUserToResizeRows = false;
      this.dataGridViews.BackgroundColor = System.Drawing.SystemColors.Control;
      this.dataGridViews.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.dataGridViews.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgViewName,
            this.dgLocalisedName});
      this.dataGridViews.Location = new System.Drawing.Point(16, 19);
      this.dataGridViews.MultiSelect = false;
      this.dataGridViews.Name = "dataGridViews";
      this.dataGridViews.RowHeadersVisible = false;
      this.dataGridViews.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridViews.Size = new System.Drawing.Size(440, 147);
      this.dataGridViews.TabIndex = 4;
      this.dataGridViews.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViews_CellEndEdit);
      this.dataGridViews.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.OnCellPainting);
      this.dataGridViews.SelectionChanged += new System.EventHandler(this.dataGridViews_SelectionChanged);
      this.dataGridViews.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
      this.dataGridViews.DragOver += new System.Windows.Forms.DragEventHandler(this.OnDragOver);
      this.dataGridViews.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
      this.dataGridViews.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
      // 
      // dgViewName
      // 
      this.dgViewName.HeaderText = "Displayed View Name";
      this.dgViewName.Name = "dgViewName";
      this.dgViewName.ReadOnly = true;
      this.dgViewName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
      this.dgViewName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.dgViewName.Width = 200;
      // 
      // dgLocalisedName
      // 
      this.dgLocalisedName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.dgLocalisedName.HeaderText = "Enter the View Name or Localised Code";
      this.dgLocalisedName.Name = "dgLocalisedName";
      this.dgLocalisedName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
      this.dgLocalisedName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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
            this.dgOperator,
            this.dgRestriction,
            this.dgLimit,
            this.dgViewAs,
            this.dgSortBy,
            this.dgAsc,
            this.dgSkip});
      this.dataGrid.Location = new System.Drawing.Point(16, 200);
      this.dataGrid.MultiSelect = false;
      this.dataGrid.Name = "dataGrid";
      this.dataGrid.RowHeadersVisible = false;
      this.dataGrid.Size = new System.Drawing.Size(440, 161);
      this.dataGrid.TabIndex = 8;
      this.dataGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.OnCellPainting);
      this.dataGrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGrid_CurrentCellDirtyStateChanged);
      this.dataGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGrid_DataError);
      this.dataGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
      this.dataGrid.DragOver += new System.Windows.Forms.DragEventHandler(this.OnDragOver);
      this.dataGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGrid_KeyDown);
      this.dataGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
      this.dataGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
      // 
      // dgSelection
      // 
      this.dgSelection.HeaderText = "Selection";
      this.dgSelection.Name = "dgSelection";
      this.dgSelection.ToolTipText = "Select the field that should be retrieved from the database";
      this.dgSelection.Width = 80;
      // 
      // dgOperator
      // 
      this.dgOperator.HeaderText = "Operator";
      this.dgOperator.Name = "dgOperator";
      this.dgOperator.ToolTipText = "Choose an Operator for the Restriction";
      this.dgOperator.Width = 55;
      // 
      // dgRestriction
      // 
      this.dgRestriction.HeaderText = "Restriction";
      this.dgRestriction.Name = "dgRestriction";
      this.dgRestriction.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.dgRestriction.ToolTipText = "Restrict the selected rows by the value specified";
      this.dgRestriction.Width = 75;
      // 
      // dgLimit
      // 
      this.dgLimit.HeaderText = "Limit";
      this.dgLimit.Name = "dgLimit";
      this.dgLimit.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      this.dgLimit.ToolTipText = "Enter a Limit for Rows to be returned";
      this.dgLimit.Width = 48;
      // 
      // dgViewAs
      // 
      this.dgViewAs.HeaderText = "Layout";
      this.dgViewAs.Name = "dgViewAs";
      this.dgViewAs.ToolTipText = "Select how the returned data should be shown";
      this.dgViewAs.Width = 60;
      // 
      // dgSortBy
      // 
      this.dgSortBy.HeaderText = "SortBy";
      this.dgSortBy.Name = "dgSortBy";
      this.dgSortBy.ToolTipText = "Choose the sort field";
      this.dgSortBy.Width = 60;
      // 
      // dgAsc
      // 
      this.dgAsc.FalseValue = "false";
      this.dgAsc.HeaderText = "Asc";
      this.dgAsc.Name = "dgAsc";
      this.dgAsc.ToolTipText = "Chose sort direction";
      this.dgAsc.TrueValue = "true";
      this.dgAsc.Width = 28;
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
      // lblActionCodes
      // 
      this.lblActionCodes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblActionCodes.Location = new System.Drawing.Point(16, 364);
      this.lblActionCodes.Name = "lblActionCodes";
      this.lblActionCodes.Size = new System.Drawing.Size(430, 29);
      this.lblActionCodes.TabIndex = 7;
      this.lblActionCodes.Text = "Use the \"Ins\" and \"Del\" key to insert and delete lines. Drag the rows to change o" +
    "rder";
      this.lblActionCodes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnDelete
      // 
      this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDelete.Location = new System.Drawing.Point(363, 172);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(83, 22);
      this.btnDelete.TabIndex = 6;
      this.btnDelete.Text = "Delete View";
      this.btnDelete.UseVisualStyleBackColor = true;
      this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
      // 
      // BaseViews
      // 
      this.Controls.Add(this.groupBox);
      this.Name = "BaseViews";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViews)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox;
    private MediaPortal.UserInterface.Controls.MPLabel lblActionCodes;
    private MediaPortal.UserInterface.Controls.MPButton btnDelete;
    private System.Windows.Forms.DataGridView dataGrid;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgSelection;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgOperator;
    private System.Windows.Forms.DataGridViewTextBoxColumn dgRestriction;
    private System.Windows.Forms.DataGridViewTextBoxColumn dgLimit;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgViewAs;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgSortBy;
    private System.Windows.Forms.DataGridViewCheckBoxColumn dgAsc;
    private System.Windows.Forms.DataGridViewCheckBoxColumn dgSkip;
    private System.Windows.Forms.DataGridView dataGridViews;
    private MediaPortal.UserInterface.Controls.MPButton btnAdd;
    private System.Windows.Forms.DataGridViewTextBoxColumn dgViewName;
    private System.Windows.Forms.DataGridViewTextBoxColumn dgLocalisedName;
    private UserInterface.Controls.MPButton btnSetDefaults;
  }
}
