#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Drawing;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for ProgramViews.
  /// </summary>
  public class ProgramViews : System.Windows.Forms.UserControl
  {
    // two classes ripped from frodos MusicViews.cs
    public class SyncedCheckBox : CheckBox
    {
      DataGrid grid;
      int cell;
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
      DataGrid grid;
      int cell;
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
          //if (true||SelectedIndex<0)
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
          }
          base.OnLayout(levent);
        }
        catch (Exception)
        {
        }
      }
    }

    private MediaPortal.UserInterface.Controls.MPTextBox tbViewName;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPButton btnDelete;
    private MediaPortal.UserInterface.Controls.MPButton btnSave;
    private MediaPortal.UserInterface.Controls.MPComboBox cbViews;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private System.Windows.Forms.DataGrid dataGrid1;
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;


    ViewDefinition currentView;
    ArrayList views;
    DataSet ds = new DataSet();
    bool updating = false;

    string[] selections = new string[]
    {
      "title",
      "filename",
      "country",
      "genre",
      "year",
      "manufacturer",
      "rating",
      "launchcount",
      "lastTimeLaunched",
      "genre2",
      "genre3",
      "genre4",
      "genre5"
    };
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    string[] sqloperators = new string[]
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

    public ProgramViews()
      : this("Program Views")
    {
    }

    public ProgramViews(string name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      views = new ArrayList();
      if (System.IO.File.Exists(Config.GetFile(Config.Dir.Config, "programViews2.xml")))
      {
        using (FileStream fileStream = new FileStream(Config.GetFile(Config.Dir.Config, "programViews2.xml"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
          try
          {
            SoapFormatter formatter = new SoapFormatter();
            views = (ArrayList)formatter.Deserialize(fileStream);
            fileStream.Close();
          }
          catch
          {
          }
        }
      }
      else
      {
        Log.Info("Warning: no programViews2.xml found!");
      }
      LoadViews();
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

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.tbViewName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnDelete = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.cbViews = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.dataGrid1 = new System.Windows.Forms.DataGrid();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tbViewName
      // 
      this.tbViewName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbViewName.BorderColor = System.Drawing.Color.Empty;
      this.tbViewName.Location = new System.Drawing.Point(62, 47);
      this.tbViewName.Name = "tbViewName";
      this.tbViewName.Size = new System.Drawing.Size(254, 21);
      this.tbViewName.TabIndex = 20;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(6, 50);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(38, 13);
      this.label2.TabIndex = 19;
      this.label2.Text = "Name:";
      // 
      // btnDelete
      // 
      this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDelete.Location = new System.Drawing.Point(244, 374);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(72, 22);
      this.btnDelete.TabIndex = 18;
      this.btnDelete.Text = "Delete";
      this.btnDelete.UseVisualStyleBackColor = true;
      this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
      // 
      // btnSave
      // 
      this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSave.Location = new System.Drawing.Point(166, 374);
      this.btnSave.Name = "btnSave";
      this.btnSave.Size = new System.Drawing.Size(72, 22);
      this.btnSave.TabIndex = 17;
      this.btnSave.Text = "Save";
      this.btnSave.UseVisualStyleBackColor = true;
      this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
      // 
      // cbViews
      // 
      this.cbViews.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbViews.BorderColor = System.Drawing.Color.Empty;
      this.cbViews.Location = new System.Drawing.Point(62, 20);
      this.cbViews.Name = "cbViews";
      this.cbViews.Size = new System.Drawing.Size(254, 21);
      this.cbViews.TabIndex = 16;
      this.cbViews.SelectedIndexChanged += new System.EventHandler(this.cbViews_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(6, 23);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(33, 13);
      this.label1.TabIndex = 15;
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
      this.dataGrid1.Location = new System.Drawing.Point(6, 74);
      this.dataGrid1.Name = "dataGrid1";
      this.dataGrid1.Size = new System.Drawing.Size(310, 294);
      this.dataGrid1.TabIndex = 14;
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.btnDelete);
      this.mpGroupBox1.Controls.Add(this.tbViewName);
      this.mpGroupBox1.Controls.Add(this.btnSave);
      this.mpGroupBox1.Controls.Add(this.label1);
      this.mpGroupBox1.Controls.Add(this.dataGrid1);
      this.mpGroupBox1.Controls.Add(this.label2);
      this.mpGroupBox1.Controls.Add(this.cbViews);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(3, 3);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(322, 402);
      this.mpGroupBox1.TabIndex = 83;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Program Views";
      // 
      // ProgramViews
      // 
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "ProgramViews";
      this.Size = new System.Drawing.Size(328, 408);
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion



    void LoadViews()
    {
      updating = true;
      cbViews.Items.Clear();
      foreach (ViewDefinition view in views)
      {
        if (view.Name != String.Empty)
        {
          cbViews.Items.Add(view.Name);
        }
      }
      cbViews.Items.Add("new...");
      if (cbViews.Items.Count > 0)
        cbViews.SelectedIndex = 0;

      UpdateView();
      updating = false;
    }

    void UpdateView()
    {
      updating = true;
      currentView = null;
      int index = cbViews.SelectedIndex;
      if (index < 0) return;
      if (index < views.Count)
        currentView = views[index] as ViewDefinition;
      if (currentView == null)
      {
        currentView = new ViewDefinition();
        currentView.Name = "new...";
      }
      tbViewName.Text = currentView.Name;

      //Declare and initialize local variables used
      DataColumn dtCol = null;//Data Column variable
      string[] arrColumnNames = null;//string array variable
      SyncedComboBox cbSelection, cbOperators;  //combo box var              
      DataTable datasetFilters;//Data Table var

      //Create the combo box object and set its properties
      cbSelection = new SyncedComboBox();
      cbSelection.Cursor = System.Windows.Forms.Cursors.Arrow;
      cbSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      cbSelection.Dock = DockStyle.Fill;
      cbSelection.DisplayMember = "Selection";
      foreach (string strText in selections)
        cbSelection.Items.Add(strText);
      cbSelection.Grid = dataGrid1;
      cbSelection.Cell = 0;
      //Event that will be fired when selected index in the combo box is changed
      cbSelection.SelectionChangeCommitted += new EventHandler(cbSelection_SelectionChangeCommitted);

      //Create the combo box object and set its properties
      cbOperators = new SyncedComboBox();
      cbOperators.Cursor = System.Windows.Forms.Cursors.Arrow;
      cbOperators.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      cbOperators.Dock = DockStyle.Fill;
      cbOperators.DisplayMember = "Operator";
      foreach (string strText in sqloperators)
        cbOperators.Items.Add(strText);
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
        dtCol.DataType = System.Type.GetType("System.String");
        dtCol.DefaultValue = "";
        datasetFilters.Columns.Add(dtCol);
      }

      //Add a Column with checkbox at last in the Grid     
      DataColumn dtcCheck = new DataColumn("Sort Ascending");//create the data          //column object with the name 
      dtcCheck.DataType = System.Type.GetType("System.Boolean");//Set its //data Type
      dtcCheck.DefaultValue = false;//Set the default value
      dtcCheck.AllowDBNull = false;
      dtcCheck.ColumnName = "Sort Ascending";
      datasetFilters.Columns.Add(dtcCheck);//Add the above column to the //Data Table

      //fill in all rows...
      for (int i = 0; i < currentView.Filters.Count; ++i)
      {
        FilterDefinition def = (FilterDefinition)currentView.Filters[i];
        string limit = def.Limit.ToString();
        if (def.Limit < 0) limit = "";
        datasetFilters.Rows.Add(new object[] { def.Where, def.SqlOperator, def.Restriction, limit, def.SortAscending });
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
        dgdtblStyle.MappingName = datasetFilters.TableName;//its table name of dataset
        dataGrid1.TableStyles.Add(dgdtblStyle);
        dgdtblStyle.RowHeadersVisible = false;
        dgdtblStyle.HeaderBackColor = Color.LightSteelBlue;
        dgdtblStyle.AllowSorting = false;
        dgdtblStyle.HeaderBackColor = Color.FromArgb(8, 36, 107);
        dgdtblStyle.RowHeadersVisible = false;
        dgdtblStyle.HeaderForeColor = Color.White;
        dgdtblStyle.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
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


      updating = false;
    }



    private void cbViews_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (updating) return;
      StoreGridInView();
      UpdateView();
    }

    private void cbSelection_SelectionChangeCommitted(object sender, EventArgs e)
    {
      if (updating) return;
      SyncedComboBox box = sender as SyncedComboBox;
      if (box == null) return;
      DataGridCell currentCell = dataGrid1.CurrentCell;
      DataTable table = dataGrid1.DataSource as DataTable;

      if (currentCell.RowNumber == table.Rows.Count)
        table.Rows.Add(new object[] { "", "", "", "" });
      table.Rows[currentCell.RowNumber][currentCell.ColumnNumber] = (string)box.SelectedItem;
    }


    private void cbOperators_SelectionChangeCommitted(object sender, EventArgs e)
    {
      if (updating) return;
      SyncedComboBox box = sender as SyncedComboBox;
      if (box == null) return;
      DataGridCell currentCell = dataGrid1.CurrentCell;
      DataTable table = dataGrid1.DataSource as DataTable;

      if (currentCell.RowNumber == table.Rows.Count)
        table.Rows.Add(new object[] { "", "", "", "" });
      table.Rows[currentCell.RowNumber][currentCell.ColumnNumber] = (string)box.SelectedItem;

    }


    private void btnSave_Click(object sender, System.EventArgs e)
    {
      StoreGridInView();
      try
      {
        using (FileStream fileStream = new FileStream(Config.GetFile(Config.Dir.Config, "programViews2.xml"), FileMode.Create, FileAccess.Write, FileShare.Read))
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

    void StoreGridInView()
    {
      if (updating) return;
      if (dataGrid1.DataSource == null) return;
      if (currentView == null) return;
      ViewDefinition view = null;
      for (int i = 0; i < views.Count; ++i)
      {
        ViewDefinition tmp = views[i] as ViewDefinition;
        if (tmp.Name == currentView.Name)
        {
          view = tmp;
          break;
        }
      }
      DataTable dt = dataGrid1.DataSource as DataTable;
      if (view == null)
      {
        if (dt.Rows.Count == 0) return;
        view = new ViewDefinition();
        view.Name = tbViewName.Text;
        views.Add(view);
        currentView = view;
        cbViews.Items.Insert(cbViews.Items.Count - 1, view.Name);
        updating = true;
        cbViews.SelectedItem = view.Name;
        updating = false;
      }
      else
      {
        updating = true;
        for (int i = 0; i < cbViews.Items.Count; ++i)
        {
          string label = (string)cbViews.Items[i];
          if (label == currentView.Name)
          {
            cbViews.Items[i] = tbViewName.Text;
            break;
          }
        }
        updating = false;
      }
      view.Name = tbViewName.Text;
      view.Filters.Clear();

      foreach (DataRow row in dt.Rows)
      {
        FilterDefinition def = new FilterDefinition();
        def.Where = row[0] as string;
        if (def.Where == String.Empty) continue;
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
        view.Filters.Add(def);
      }
    }

    private void btnDelete_Click(object sender, System.EventArgs e)
    {
      string viewName = cbViews.SelectedItem as string;
      if (viewName == null) return;
      for (int i = 0; i < views.Count; ++i)
      {
        ViewDefinition view = views[i] as ViewDefinition;
        if (view.Name == viewName)
        {
          views.RemoveAt(i);
          break;
        }
      }
      LoadViews();
    }



  }
}
