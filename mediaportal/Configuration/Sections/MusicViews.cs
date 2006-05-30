#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class MusicViews : SectionSettings
  {
      private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
  
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
              this.Checked = (bool)row.ItemArray[Cell];
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
                  string currentValue = (string)row.ItemArray[Cell];
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
        { }
      }
  }
      private DataGrid dataGrid1;
      private MediaPortal.UserInterface.Controls.MPLabel label1;
      private MediaPortal.UserInterface.Controls.MPComboBox cbViews;
      private MediaPortal.UserInterface.Controls.MPButton btnSave;
      private MediaPortal.UserInterface.Controls.MPButton btnDelete;
    private IContainer components = null;

    private ViewDefinition currentView;
      private ArrayList views;
      private MediaPortal.UserInterface.Controls.MPLabel label2;
      private MediaPortal.UserInterface.Controls.MPTextBox tbViewName;
    private bool updating = false;

    private string[] selections = new string[]
      {
        "album",
        "artist",
        "title",
        "genre",
        "year",
        "track",
        "timesplayed",
        "rating",
        "favorites"
      };

    private string[] sqloperators = new string[]
      {
        "",
        "=",
        ">",
        "<",
        ">=",
        "<=",
        "<>",
        "like",

      };

    private string[] viewsAs = new string[]
			{
				"List",
				"Icons",
				"Big Icons",
				"Filmstrip",
		};

    public MusicViews()
      : this("Music Views")
    { }

    public MusicViews(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      views = new ArrayList();
      FileInfo fi = new FileInfo("MusicViews.xml");
      if (fi.Exists)
      {
        try
        {
          using (FileStream fileStream = fi.OpenRead())
          {
            try
            {
              SoapFormatter formatter = new SoapFormatter();
              views = (ArrayList)formatter.Deserialize(fileStream);
            }
            finally
            {
              fileStream.Close();
            }
          }
        }
        catch
        { }
      }
      else
      {
        Log.Write("MusicViews.xml not found.  No Music Views will be available...");
      }
      LoadViews();
    }

    private void LoadViews()
    {
      updating = true;
      cbViews.Items.Clear();
      foreach (ViewDefinition view in views)
      {
        if (view.Name != String.Empty)
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
      currentView = (ViewDefinition)cbViews.SelectedItem;
      if (currentView == null)
      {
          return;
      }
      tbViewName.Text = currentView.Name;

      //Declare and initialize local variables used
      DataColumn dtCol = null; //Data Column variable
      string[] arrColumnNames = null; //string array variable
      SyncedComboBox cbSelection, cbOperators; //combo box var              
      DataTable datasetFilters; //Data Table var

      //Create the combo box object and set its properties
      cbSelection = new SyncedComboBox();
      cbSelection.Cursor = Cursors.Arrow;
      cbSelection.DropDownStyle = ComboBoxStyle.DropDownList;
      cbSelection.Dock = DockStyle.Fill;
      cbSelection.DisplayMember = "Selection";
      foreach (string strText in selections)
      {
        cbSelection.Items.Add(strText);
      }
      cbSelection.Grid = dataGrid1;
      cbSelection.Cell = 0;
      //Event that will be fired when selected index in the combo box is changed
      cbSelection.SelectionChangeCommitted += new EventHandler(cbSelection_SelectionChangeCommitted);

      //Create the combo box object and set its properties
      cbOperators = new SyncedComboBox();
      cbOperators.Cursor = Cursors.Arrow;
      cbOperators.DropDownStyle = ComboBoxStyle.DropDownList;
      cbOperators.Dock = DockStyle.Fill;
      cbOperators.DisplayMember = "Operator";
      foreach (string strText in sqloperators)
      {
        cbOperators.Items.Add(strText);
      }
      cbOperators.Grid = dataGrid1;
      cbOperators.Cell = 1;
      cbOperators.SelectionChangeCommitted += new EventHandler(cbOperators_SelectionChangeCommitted);

      //Create the String array object, initialize the array with the column
      //names to be displayed
      arrColumnNames = new string[4];
      arrColumnNames[0] = "Selection";
      arrColumnNames[1] = "Operator";
      arrColumnNames[2] = "Restriction";
      arrColumnNames[3] = "Limit";

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

      //Add a Column with checkbox at last in the Grid     
      DataColumn dtcCheck = new DataColumn("Sort Ascending"); //create the data          //column object with the name 
      dtcCheck.DataType = Type.GetType("System.Boolean"); //Set its //data Type
      dtcCheck.DefaultValue = false; //Set the default value
      dtcCheck.AllowDBNull = false;
      dtcCheck.ColumnName = "Sort Ascending";
      datasetFilters.Columns.Add(dtcCheck); //Add the above column to the //Data Table


      dtCol = new DataColumn("ViewAs");
      dtCol.DataType = Type.GetType("System.String");
      dtCol.DefaultValue = "";
      datasetFilters.Columns.Add(dtCol);

      SyncedComboBox cbView = new SyncedComboBox();
      cbView.Cursor = Cursors.Arrow;
      cbView.DropDownStyle = ComboBoxStyle.DropDownList;
      cbView.Dock = DockStyle.Fill;
      cbView.DisplayMember = "ViewAs";
      foreach (string strText in viewsAs)
      {
        cbView.Items.Add(strText);
      }
      cbView.Grid = dataGrid1;
      cbView.Cell = 1;
      cbView.SelectionChangeCommitted += new EventHandler(cbView_SelectionChangeCommitted);



      //fill in all rows...
      for (int i = 0; i < currentView.Filters.Count; ++i)
      {
        FilterDefinition def = (FilterDefinition)currentView.Filters[i];
        string limit = def.Limit.ToString();
        if (def.Limit < 0)
        {
          limit = "";
        }
        datasetFilters.Rows.Add(
            new object[] {
													 def.Where, def.SqlOperator, def.Restriction, limit, def.SortAscending,
													 def.DefaultView
												 }
                               );
      }

      //Set the Data Grid Source as the Data Table created above
      dataGrid1.CaptionText = String.Empty;
      dataGrid1.DataSource = datasetFilters;

      //set style property when first time the grid loads, next time onwards it //will maintain its property
      if (!dataGrid1.TableStyles.Contains("Selection"))
      {
        //Create a DataGridTableStyle object     
        DataGridTableStyle dgdtblStyle = new DataGridTableStyle();
        //Set its properties
        dgdtblStyle.MappingName = datasetFilters.TableName; //its table name of dataset
        dataGrid1.TableStyles.Add(dgdtblStyle);
        dgdtblStyle.RowHeadersVisible = false;
        dgdtblStyle.HeaderBackColor = Color.LightSteelBlue;
        dgdtblStyle.AllowSorting = false;
        dgdtblStyle.HeaderBackColor = Color.FromArgb(8, 36, 107);
        dgdtblStyle.RowHeadersVisible = false;
        dgdtblStyle.HeaderForeColor = Color.White;
        dgdtblStyle.HeaderFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, ((Byte)(0)));
        dgdtblStyle.GridLineColor = Color.DarkGray;
        dgdtblStyle.PreferredRowHeight = 22;
        dataGrid1.BackgroundColor = Color.White;

        //Take the columns in a GridColumnStylesCollection object and set //the size of the
        //individual columns   
        GridColumnStylesCollection colStyle;
        colStyle = dataGrid1.TableStyles[0].GridColumnStyles;
        colStyle[0].Width = 100;
        colStyle[1].Width = 50;
        colStyle[2].Width = 50;
        colStyle[3].Width = 80;

        /*
				DataGridColumnStyle boolCol = new FormattableBooleanColumn();
				boolCol.MappingName = "Sort Ascending";
				boolCol.HeaderText = "Sort Ascending";
				boolCol.Width = 60;
				dgdtblStyle.GridColumnStyles.Add(boolCol);
				*/
      }
      DataGridTextBoxColumn dgtb = (DataGridTextBoxColumn)dataGrid1.TableStyles[0].GridColumnStyles[0];
      //Add the combo box to the text box taken in the above step 
      dgtb.TextBox.Controls.Add(cbSelection);

      dgtb = (DataGridTextBoxColumn)dataGrid1.TableStyles[0].GridColumnStyles[1];
      dgtb.TextBox.Controls.Add(cbOperators);

      DataGridBoolColumn boolColumn = (DataGridBoolColumn)dataGrid1.TableStyles[0].GridColumnStyles[4];
      boolColumn.AllowNull = false;

      dgtb = (DataGridTextBoxColumn)dataGrid1.TableStyles[0].GridColumnStyles[5];
      dgtb.TextBox.Controls.Add(cbView);

      updating = false;
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
        this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
        this.tbViewName = new MediaPortal.UserInterface.Controls.MPTextBox();
        this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
        this.btnDelete = new MediaPortal.UserInterface.Controls.MPButton();
        this.btnSave = new MediaPortal.UserInterface.Controls.MPButton();
        this.cbViews = new MediaPortal.UserInterface.Controls.MPComboBox();
        this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
        this.dataGrid1 = new System.Windows.Forms.DataGrid();
        this.groupBox1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
        this.SuspendLayout();
        // 
        // groupBox1
        // 
        this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.groupBox1.Controls.Add(this.tbViewName);
        this.groupBox1.Controls.Add(this.label2);
        this.groupBox1.Controls.Add(this.btnDelete);
        this.groupBox1.Controls.Add(this.btnSave);
        this.groupBox1.Controls.Add(this.cbViews);
        this.groupBox1.Controls.Add(this.label1);
        this.groupBox1.Controls.Add(this.dataGrid1);
        this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.groupBox1.Location = new System.Drawing.Point(0, 0);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(471, 352);
        this.groupBox1.TabIndex = 0;
        this.groupBox1.TabStop = false;
        // 
        // tbViewName
        // 
        this.tbViewName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.tbViewName.Location = new System.Drawing.Point(168, 44);
        this.tbViewName.Name = "tbViewName";
        this.tbViewName.Size = new System.Drawing.Size(288, 20);
        this.tbViewName.TabIndex = 3;
        // 
        // label2
        // 
        this.label2.Location = new System.Drawing.Point(16, 48);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(132, 16);
        this.label2.TabIndex = 2;
        this.label2.Text = "Name or Localized Code:";
        // 
        // btnDelete
        // 
        this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.btnDelete.Location = new System.Drawing.Point(384, 320);
        this.btnDelete.Name = "btnDelete";
        this.btnDelete.Size = new System.Drawing.Size(72, 22);
        this.btnDelete.TabIndex = 6;
        this.btnDelete.Text = "Delete";
        this.btnDelete.UseVisualStyleBackColor = true;
        this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
        // 
        // btnSave
        // 
        this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.btnSave.Location = new System.Drawing.Point(304, 320);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(72, 22);
        this.btnSave.TabIndex = 5;
        this.btnSave.Text = "Save";
        this.btnSave.UseVisualStyleBackColor = true;
        this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
        // 
        // cbViews
        // 
        this.cbViews.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.cbViews.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cbViews.Location = new System.Drawing.Point(168, 20);
        this.cbViews.Name = "cbViews";
        this.cbViews.Size = new System.Drawing.Size(288, 21);
        this.cbViews.TabIndex = 1;
        this.cbViews.SelectedIndexChanged += new System.EventHandler(this.cbViews_SelectedIndexChanged);
        // 
        // label1
        // 
        this.label1.Location = new System.Drawing.Point(16, 24);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(32, 16);
        this.label1.TabIndex = 0;
        this.label1.Text = "View:";
        // 
        // dataGrid1
        // 
        this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.dataGrid1.DataMember = "";
        this.dataGrid1.FlatMode = true;
        this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.dataGrid1.Location = new System.Drawing.Point(16, 72);
        this.dataGrid1.Name = "dataGrid1";
        this.dataGrid1.Size = new System.Drawing.Size(440, 237);
        this.dataGrid1.TabIndex = 4;
        // 
        // MusicViews
        // 
        this.Controls.Add(this.groupBox1);
        this.Name = "MusicViews";
        this.Size = new System.Drawing.Size(472, 408);
        this.groupBox1.ResumeLayout(false);
        this.groupBox1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion

    private void cbViews_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (updating)
      {
        return;
      }
      StoreGridInView();
      dataGrid1.DataSource = null;
      UpdateView();
    }

    private void cbSelection_SelectionChangeCommitted(object sender, EventArgs e)
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
      DataGridCell currentCell = dataGrid1.CurrentCell;
      DataTable table = dataGrid1.DataSource as DataTable;

      if (currentCell.RowNumber == table.Rows.Count)
      {
        table.Rows.Add(new object[] { "", "", "", "" });
      }
      table.Rows[currentCell.RowNumber][currentCell.ColumnNumber] = (string)box.SelectedItem;
    }


    private void cbView_SelectionChangeCommitted(object sender, EventArgs e)
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
      DataGridCell currentCell = dataGrid1.CurrentCell;
      DataTable table = dataGrid1.DataSource as DataTable;

      if (currentCell.RowNumber == table.Rows.Count)
      {
        table.Rows.Add(new object[] { "", "", "", "" });
      }
      table.Rows[currentCell.RowNumber][currentCell.ColumnNumber] = (string)box.SelectedItem;

    }
    private void cbOperators_SelectionChangeCommitted(object sender, EventArgs e)
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
      DataGridCell currentCell = dataGrid1.CurrentCell;
      DataTable table = dataGrid1.DataSource as DataTable;

      if (currentCell.RowNumber == table.Rows.Count)
      {
        table.Rows.Add(new object[] { "", "", "", "" });
      }
      table.Rows[currentCell.RowNumber][currentCell.ColumnNumber] = (string)box.SelectedItem;

    }

    private void btnSave_Click(object sender, EventArgs e)
    {
      StoreGridInView();
    }
      public override void SaveSettings()
      {
          try
          {
              using (FileStream fileStream = new FileStream("musicViews.xml", FileMode.Create, FileAccess.Write, FileShare.Read))
              {
                  SoapFormatter formatter = new SoapFormatter();
                  formatter.Serialize(fileStream, views);
                  fileStream.Close();
              }
          }
          catch (Exception)
          { }
      }

    private void StoreGridInView()
    {
      if (updating)
      {
        return;
      }
      if (dataGrid1.DataSource == null)
      {
        return;
      }
      if (currentView == null)
      {
        return;
      }
      ViewDefinition view = currentView;
      DataTable dt = dataGrid1.DataSource as DataTable;
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
        if (def.Where == String.Empty)
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
        def.SortAscending = (bool)row[4];
        def.DefaultView = row[5].ToString();
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
  }
}