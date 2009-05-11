#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows.Forms;
using MediaPortal.GUI.View;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class BaseViews : SectionSettings
  {
    public class SyncedCheckBox : CheckBox
    {
      private DataGrid grid;
      private int cell;

      public int Cell
      {
        get { return cell; }
        set { cell = value; }
      }

      public DataGrid Grid
      {
        get { return grid; }
        set { grid = value; }
      }

      protected override void OnLayout(LayoutEventArgs levent)
      {
        DataGridCell currentCell = Grid.CurrentCell;

        if (currentCell.ColumnNumber == Cell)
        {
          DataTable ds = Grid.DataSource as DataTable;
          if (ds != null)
          {
            if (currentCell.RowNumber < ds.Rows.Count)
            {
              DataRow row = ds.Rows[currentCell.RowNumber];
              this.Checked = (bool) row.ItemArray[Cell];
            }
          }
        }
        base.OnLayout(levent);
      }
    }

    public class SyncedComboBox : ComboBox
    {
      private DataGrid grid;
      private int cell;

      public SyncedComboBox(string name)
      {
        this.Cursor = Cursors.Arrow;
        this.DropDownStyle = ComboBoxStyle.DropDownList;
        this.Dock = DockStyle.Fill;
        this.DisplayMember = name;
        this.MaxDropDownItems = 10;
      }

      public int Cell
      {
        get { return cell; }
        set { cell = value; }
      }

      public DataGrid Grid
      {
        get { return grid; }
        set { grid = value; }
      }

      protected override void OnLayout(LayoutEventArgs levent)
      {
        try
        {
          DataGridCell currentCell = Grid.CurrentCell;

          if (currentCell.ColumnNumber == Cell)
          {
            DataTable ds = Grid.DataSource as DataTable;
            if (ds != null)
            {
              foreach (string item in Items)
              {
                if (currentCell.RowNumber < ds.Rows.Count)
                {
                  DataRow row = ds.Rows[currentCell.RowNumber];
                  string currentValue = (string) row.ItemArray[Cell];
                  if (currentValue == item)
                  {
                    SelectedItem = item;
                    break;
                  }
                }
              }
            }
          }
          base.OnLayout(levent);
        }
        catch (Exception)
        {
        }
      }
    }

    private DataGrid dataGrid;
    private DataTable datasetFilters;
    private ViewDefinition currentView;
    public ArrayList views;
    private bool updating = false;
    public bool settingsChanged = false;

    private List<string> _selections = new List<string>();
    private List<string> _sqloperators = new List<string>();
    private List<string> _viewsAs = new List<string>();
    private List<string> _sortBy = new List<string>();

    private MPGroupBox groupBox;
    private MPComboBox cbViews;
    private MPLabel lblViewName;
    private MPTextBox tbViewName;
    private MPLabel lblActionCodes;
    private MPButton btnSave;
    private MPButton btnDelete;
    private MPLabel lblViews;
    private IContainer components = null;

    public string[] Selections
    {
      get { return _selections.ToArray(); }
      set
      {
        _selections.Clear();
        _selections.AddRange(value);
      }
    }

    public string[] Sqloperators
    {
      get { return _sqloperators.ToArray(); }
      set
      {
        _sqloperators.Clear();
        _sqloperators.AddRange(value);
      }
    }

    public string[] ViewsAs
    {
      get { return _viewsAs.ToArray(); }
      set
      {
        _viewsAs.Clear();
        _viewsAs.AddRange(value);
      }
    }

    public string[] SortBy
    {
      get { return _sortBy.ToArray(); }
      set
      {
        _sortBy.Clear();
        _sortBy.AddRange(value);
      }
    }

    public BaseViews()
      : base("<Unknown>")
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    public BaseViews(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lblViews = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbViews = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lblViewName = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbViewName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.dataGrid = new System.Windows.Forms.DataGrid();
      this.lblActionCodes = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnDelete = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.dataGrid)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox
      // 
      this.groupBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox.Controls.Add(this.lblViews);
      this.groupBox.Controls.Add(this.cbViews);
      this.groupBox.Controls.Add(this.lblViewName);
      this.groupBox.Controls.Add(this.tbViewName);
      this.groupBox.Controls.Add(this.dataGrid);
      this.groupBox.Controls.Add(this.lblActionCodes);
      this.groupBox.Controls.Add(this.btnSave);
      this.groupBox.Controls.Add(this.btnDelete);
      this.groupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox.Location = new System.Drawing.Point(0, 0);
      this.groupBox.Name = "groupBox";
      this.groupBox.Size = new System.Drawing.Size(472, 408);
      this.groupBox.TabIndex = 0;
      this.groupBox.TabStop = false;
      // 
      // lblViews
      // 
      this.lblViews.AutoSize = true;
      this.lblViews.Location = new System.Drawing.Point(13, 27);
      this.lblViews.Name = "lblViews";
      this.lblViews.Size = new System.Drawing.Size(33, 13);
      this.lblViews.TabIndex = 0;
      this.lblViews.Text = "View:";
      // 
      // cbViews
      // 
      this.cbViews.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.cbViews.BorderColor = System.Drawing.Color.Empty;
      this.cbViews.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbViews.Location = new System.Drawing.Point(145, 24);
      this.cbViews.Name = "cbViews";
      this.cbViews.Size = new System.Drawing.Size(311, 21);
      this.cbViews.TabIndex = 1;
      this.cbViews.SelectedIndexChanged += new System.EventHandler(this.cbViews_SelectedIndexChanged);
      // 
      // lblViewName
      // 
      this.lblViewName.AutoSize = true;
      this.lblViewName.Location = new System.Drawing.Point(13, 54);
      this.lblViewName.Name = "lblViewName";
      this.lblViewName.Size = new System.Drawing.Size(126, 13);
      this.lblViewName.TabIndex = 2;
      this.lblViewName.Text = "Name or Localized Code:";
      // 
      // tbViewName
      // 
      this.tbViewName.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.tbViewName.BorderColor = System.Drawing.Color.Empty;
      this.tbViewName.Location = new System.Drawing.Point(145, 51);
      this.tbViewName.Name = "tbViewName";
      this.tbViewName.Size = new System.Drawing.Size(311, 20);
      this.tbViewName.TabIndex = 3;
      // 
      // dataGrid
      // 
      this.dataGrid.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGrid.DataMember = "";
      this.dataGrid.FlatMode = true;
      this.dataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.dataGrid.Location = new System.Drawing.Point(16, 78);
      this.dataGrid.Name = "dataGrid";
      this.dataGrid.Size = new System.Drawing.Size(440, 258);
      this.dataGrid.TabIndex = 4;
      // 
      // lblActionCodes
      // 
      this.lblActionCodes.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.lblActionCodes.Location = new System.Drawing.Point(16, 339);
      this.lblActionCodes.Name = "lblActionCodes";
      this.lblActionCodes.Size = new System.Drawing.Size(440, 29);
      this.lblActionCodes.TabIndex = 7;
      this.lblActionCodes.Text = "Actions Codes in last column: a = Insert line after, b = Insert line before, d = " +
                                 "delete line";
      this.lblActionCodes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // btnSave
      // 
      this.btnSave.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSave.Location = new System.Drawing.Point(304, 376);
      this.btnSave.Name = "btnSave";
      this.btnSave.Size = new System.Drawing.Size(72, 22);
      this.btnSave.TabIndex = 5;
      this.btnSave.Text = "Save";
      this.btnSave.UseVisualStyleBackColor = true;
      this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
      // 
      // btnDelete
      // 
      this.btnDelete.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDelete.Location = new System.Drawing.Point(384, 376);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(72, 22);
      this.btnDelete.TabIndex = 6;
      this.btnDelete.Text = "Delete";
      this.btnDelete.UseVisualStyleBackColor = true;
      this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
      // 
      // BaseViews
      // 
      this.Controls.Add(this.groupBox);
      this.Name = "BaseViews";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox.ResumeLayout(false);
      this.groupBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) (this.dataGrid)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion

    public void LoadViews()
    {
      updating = true;
      cbViews.Items.Clear();
      foreach (ViewDefinition view in views)
      {
        if (view.Name != string.Empty)
        {
          cbViews.Items.Add(view);
        }
      }
      ViewDefinition newDef = new ViewDefinition();
      newDef.Name = "new...";
      cbViews.Items.Add(newDef);
      if (cbViews.Items.Count > 0)
      {
        cbViews.SelectedIndex = 0;
      }

      UpdateView();
      updating = false;
    }

    private void UpdateView()
    {
      updating = true;
      currentView = (ViewDefinition) cbViews.SelectedItem;
      if (currentView == null)
      {
        return;
      }
      tbViewName.Text = currentView.Name;

      // Declare and initialize local variables used
      DataColumn dtCol = null; //Data Column variable
      string[] arrColumnNames = null; //string array variable


      // Create the combo box object and set its properties
      SyncedComboBox cbSelection = new SyncedComboBox("Selection");
      foreach (string strText in Selections)
      {
        cbSelection.Items.Add(strText);
      }
      cbSelection.Grid = dataGrid;
      cbSelection.Cell = 0;
      //Event that will be fired when selected index in the combo box is changed
      cbSelection.SelectionChangeCommitted += new EventHandler(ComboBox_SelectionChangeCommitted);

      SyncedComboBox cbOperators = new SyncedComboBox("Operator");
      foreach (string strText in Sqloperators)
      {
        cbOperators.Items.Add(strText);
      }
      cbOperators.Grid = dataGrid;
      cbOperators.Cell = 1;
      cbOperators.SelectionChangeCommitted += new EventHandler(ComboBox_SelectionChangeCommitted);

      SyncedComboBox cbView = new SyncedComboBox("ViewAs");
      foreach (string strText in ViewsAs)
      {
        cbView.Items.Add(strText);
      }
      cbView.Grid = dataGrid;
      cbView.Cell = 4;
      cbView.SelectionChangeCommitted += new EventHandler(ComboBox_SelectionChangeCommitted);

      SyncedComboBox cbSort = new SyncedComboBox("SortBy");
      foreach (string strText in SortBy)
      {
        cbSort.Items.Add(strText);
      }
      cbSort.Grid = dataGrid;
      cbSort.Cell = 5;
      cbSort.SelectionChangeCommitted += new EventHandler(ComboBox_SelectionChangeCommitted);


      //Create the String array object, initialize the array with the column
      //names to be displayed
      arrColumnNames = new string[6];
      arrColumnNames[0] = "Selection";
      arrColumnNames[1] = "Operator";
      arrColumnNames[2] = "Restriction";
      arrColumnNames[3] = "Limit";
      arrColumnNames[4] = "ViewAs";
      arrColumnNames[5] = "SortBy";

      //Create the Data Table object which will then be used to hold
      //columns and rows
      datasetFilters = new DataTable("Selection");

      //Add the string array of columns to the DataColumn object       
      for (int i = 0; i < arrColumnNames.Length; i++)
      {
        string str = arrColumnNames[i];
        dtCol = new DataColumn(str);
        dtCol.DataType = Type.GetType("System.String");
        dtCol.DefaultValue = "";
        datasetFilters.Columns.Add(dtCol);
      }

      // Add a Column with checkbox at last in the Grid     
      DataColumn dtcCheck = new DataColumn("Sort Ascending"); //create the data          //column object with the name 
      dtcCheck.DataType = Type.GetType("System.Boolean"); //Set its //data Type
      dtcCheck.DefaultValue = false; //Set the default value
      dtcCheck.AllowDBNull = false;
      dtcCheck.ColumnName = "Asc";
      datasetFilters.Columns.Add(dtcCheck); //Add the above column to the //Data Table

      // Add the Action column
      dtCol = new DataColumn("Act");
      dtCol.DataType = Type.GetType("System.String");
      dtCol.DefaultValue = "";
      datasetFilters.Columns.Add(dtCol);

      //fill in all rows...
      for (int i = 0; i < currentView.Filters.Count; ++i)
      {
        FilterDefinition def = (FilterDefinition) currentView.Filters[i];
        string limit = def.Limit.ToString();
        if (def.Limit < 0)
        {
          limit = "";
        }
        datasetFilters.Rows.Add(
          new object[]
            {
              def.Where,
              def.SqlOperator,
              def.Restriction,
              limit,
              def.DefaultView,
              def.DefaultSort,
              def.SortAscending,
              ""
            }
          );
      }

      //Set the Data Grid Source as the Data Table created above
      dataGrid.CaptionText = string.Empty;
      dataGrid.DataSource = datasetFilters;

      //set style property when first time the grid loads, next time onwards it //will maintain its property
      if (!dataGrid.TableStyles.Contains("Selection"))
      {
        //Create a DataGridTableStyle object     
        DataGridTableStyle dgdtblStyle = new DataGridTableStyle();
        //Set its properties
        dgdtblStyle.MappingName = datasetFilters.TableName; //its table name of dataset
        dataGrid.TableStyles.Add(dgdtblStyle);
        dgdtblStyle.RowHeadersVisible = false;
        dgdtblStyle.HeaderBackColor = Color.LightSteelBlue;
        dgdtblStyle.AllowSorting = false;
        dgdtblStyle.HeaderBackColor = Color.FromArgb(8, 36, 107);
        dgdtblStyle.RowHeadersVisible = false;
        dgdtblStyle.HeaderForeColor = Color.White;
        dgdtblStyle.HeaderFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, ((Byte) (0)));
        dgdtblStyle.GridLineColor = Color.DarkGray;
        dgdtblStyle.PreferredRowHeight = 22;
        dataGrid.BackgroundColor = Color.White;

        //Take the columns in a GridColumnStylesCollection object and set //the size of the
        //individual columns   
        GridColumnStylesCollection colStyle;
        colStyle = dataGrid.TableStyles[0].GridColumnStyles;
        colStyle[0].Width = 80;
        colStyle[1].Width = 60;
        colStyle[2].Width = 78;
        colStyle[3].Width = 48;
        colStyle[4].Width = 55;
        colStyle[5].Width = 55;
        colStyle[6].Width = 30;
        colStyle[7].Width = 30;

        // Set an eventhandler to be fired, when entering something in the action column
        DataGridTextBoxColumn tbAction = (DataGridTextBoxColumn) dgdtblStyle.GridColumnStyles[7];
        tbAction.TextBox.KeyPress += new KeyPressEventHandler(tbAction_KeyPress);

        /*
				DataGridColumnStyle boolCol = new FormattableBooleanColumn();
				boolCol.MappingName = "Sort Ascending";
				boolCol.HeaderText = "Sort Ascending";
				boolCol.Width = 60;
				dgdtblStyle.GridColumnStyles.Add(boolCol);
				*/
      }
      DataGridTextBoxColumn dgtb = (DataGridTextBoxColumn) dataGrid.TableStyles[0].GridColumnStyles[0];
      //Add the combo box to the text box taken in the above step 
      dgtb.TextBox.Controls.Add(cbSelection);

      dgtb = (DataGridTextBoxColumn) dataGrid.TableStyles[0].GridColumnStyles[1];
      dgtb.TextBox.Controls.Add(cbOperators);

      dgtb = (DataGridTextBoxColumn) dataGrid.TableStyles[0].GridColumnStyles[4];
      dgtb.TextBox.Controls.Add(cbView);

      dgtb = (DataGridTextBoxColumn) dataGrid.TableStyles[0].GridColumnStyles[5];
      dgtb.TextBox.Controls.Add(cbSort);

      DataGridBoolColumn boolColumn = (DataGridBoolColumn) dataGrid.TableStyles[0].GridColumnStyles[6];
      boolColumn.AllowNull = false;

      updating = false;
    }

    private void cbViews_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (updating)
      {
        return;
      }
      StoreGridInView();
      dataGrid.DataSource = null;
      UpdateView();
    }

    private void ComboBox_SelectionChangeCommitted(object sender, EventArgs e)
    {
      if (updating)
      {
        return;
      }
      SyncedComboBox box = sender as SyncedComboBox;
      if (box == null)
      {
        return;
      }
      DataGridCell currentCell = dataGrid.CurrentCell;
      DataTable table = dataGrid.DataSource as DataTable;

      if (currentCell.RowNumber == table.Rows.Count)
      {
        table.Rows.Add(new object[] {"", "", "", ""});
      }
      table.Rows[currentCell.RowNumber][currentCell.ColumnNumber] = (string) box.SelectedItem;
    }

    private void tbAction_KeyPress(object sender, KeyPressEventArgs e)
    {
      DataRow row = datasetFilters.NewRow();
      row[0] = row[1] = row[2] = row[3] = row[4] = row[5] = row[7] = "";
      row[6] = false;

      e.Handled = true;

      int rowSelected = dataGrid.CurrentRowIndex;
      if (rowSelected == -1)
      {
        return;
      }
      if (rowSelected == datasetFilters.Rows.Count)
      {
        return;
      }

      switch (e.KeyChar)
      {
        case 'a':
          datasetFilters.Rows.InsertAt(row, rowSelected + 1);
          break;
        case 'b':
          datasetFilters.Rows.InsertAt(row, rowSelected);
          break;
        case 'd':
          datasetFilters.Rows.RemoveAt(rowSelected);
          break;
      }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
      StoreGridInView();
    }

    private void StoreGridInView()
    {
      if (updating)
      {
        return;
      }
      if (dataGrid.DataSource == null)
      {
        return;
      }
      if (currentView == null)
      {
        return;
      }
      settingsChanged = true;
      ViewDefinition view = currentView;
      DataTable dt = dataGrid.DataSource as DataTable;
      if (view.Name == "new...")
      {
        if (dt.Rows.Count == 0)
        {
          return;
        }
        view = new ViewDefinition();
        view.Name = tbViewName.Text;
        views.Add(view);
        currentView = view;
        cbViews.Items.Insert(cbViews.Items.Count - 1, view);
        updating = true;
        cbViews.SelectedItem = view;
        updating = false;
      }
      else
      {
        updating = true;
        view.Name = tbViewName.Text;
        int index = cbViews.Items.IndexOf(view);
        if (index >= 0)
        {
          cbViews.Items[index] = view;
        }
        cbViews.Update();
        updating = false;
      }
      view.Name = tbViewName.Text;
      view.Filters.Clear();

      foreach (DataRow row in dt.Rows)
      {
        FilterDefinition def = new FilterDefinition();
        def.Where = row[0] as string;
        if (def.Where == string.Empty)
        {
          continue;
        }
        def.SqlOperator = row[1].ToString();
        def.Restriction = row[2].ToString();
        try
        {
          def.Limit = Int32.Parse(row[3].ToString());
        }
        catch (Exception)
        {
          def.Limit = -1;
        }
        def.DefaultView = row[4].ToString();
        def.DefaultSort = row[5].ToString();
        def.SortAscending = (bool) row[6];
        view.Filters.Add(def);
      }
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
      ViewDefinition viewSelected = cbViews.SelectedItem as ViewDefinition;
      if (viewSelected == null)
      {
        return;
      }
      for (int i = 0; i < views.Count; ++i)
      {
        ViewDefinition view = views[i] as ViewDefinition;
        if (view == viewSelected)
        {
          views.RemoveAt(i);
          break;
        }
      }
      LoadViews();
    }


    protected void LoadSettings(
      string mediaType,
      string[] selections,
      string[] sqloperators,
      string[] viewsAs,
      string[] sortBy
      )
    {
      string customViews = Config.GetFile(Config.Dir.Config, mediaType + "Views.xml");
      string defaultViews = Config.GetFile(Config.Dir.Base, "default" + mediaType + "Views.xml");
      Selections = selections;
      Sqloperators = sqloperators;
      ViewsAs = viewsAs;
      SortBy = sortBy;

      if (!File.Exists(customViews))
      {
        File.Copy(defaultViews, customViews);
      }

      views = new ArrayList();

      try
      {
        using (FileStream fileStream = new FileInfo(customViews).OpenRead())
        {
          SoapFormatter formatter = new SoapFormatter();
          views = (ArrayList) formatter.Deserialize(fileStream);
          fileStream.Close();
        }
      }
      catch (Exception)
      {
      }

      LoadViews();
    }

    protected void SaveSettings(string mediaType)
    {
      string customViews = Config.GetFile(Config.Dir.Config, mediaType + "Views.xml");
      if (settingsChanged)
      {
        try
        {
          using (FileStream fileStream = new FileInfo(customViews).OpenWrite())
          {
            SoapFormatter formatter = new SoapFormatter();
            formatter.Serialize(fileStream, views);
            fileStream.Close();
          }
        }
        catch (Exception)
        {
        }
      }
    }
  }
}