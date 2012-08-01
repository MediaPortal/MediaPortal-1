#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.GUI.View;
using MediaPortal.GUI.Library;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class BaseViews : SectionSettings
  {
    #region Variables

    private DataTable datasetFilters;
    private DataTable datasetViews;
    private ViewDefinition currentView;
    public ArrayList views;
    private bool updating = false;
    public bool settingsChanged = false;

    private List<string> _selections = new List<string>();
    private List<string> _sqloperators = new List<string>();
    private List<string> _viewsAs = new List<string>();
    private List<string> _sortBy = new List<string>();

    // Drag & Drop
    private int _dragDropCurrentIndex = -1;
    private Rectangle _dragDropRectangle;
    private int _dragDropSourceIndex;
    private int _dragDropTargetIndex;
    private string _dragDropInitiatingGrid = "";

    private string _section = string.Empty;

    #endregion

    #region Properties

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

    public string Section
    {
      set
      {
        _section = value ;
      }
    }

    #endregion

    #region ctor

    public BaseViews()
      : base("<Unknown>")
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public BaseViews(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    #endregion

    #region Initialisation

    /// <summary>
    /// Set up the Datagrid column and the DataTable to which the grid is bound
    /// </summary>

    private void SetupGrid()
    {
      // Declare and initialize local variables used
      DataColumn dtCol = null; //Data Column variable
      string[] arrColumnNames = null; //string array variable

      // Fill the Combo Values
      foreach (string strText in Selections)
      {
        dgSelection.Items.Add(strText);
      }

      foreach (string strText in Sqloperators)
      {
        dgOperator.Items.Add(strText);
      }

      foreach (string strText in ViewsAs)
      {
        dgViewAs.Items.Add(strText);
      }

      foreach (string strText in SortBy)
      {
        dgSortBy.Items.Add(strText);
      }

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

      // Add 2 columns with checkbox at the end of the Datarow     
      DataColumn dtcCheck = new DataColumn("SortAsc"); //create the data column object
      dtcCheck.DataType = Type.GetType("System.Boolean"); //Set its data Type
      dtcCheck.DefaultValue = true; //Set the default value
      dtcCheck.AllowDBNull = false;
      datasetFilters.Columns.Add(dtcCheck); //Add the above column to the Data Table

      DataColumn skipCheck = new DataColumn("Skip"); //create the data column object
      skipCheck.DataType = Type.GetType("System.Boolean"); //Set its data Type
      skipCheck.DefaultValue = false; //Set the default value
      skipCheck.AllowDBNull = false;
      datasetFilters.Columns.Add(skipCheck); //Add the above column to the Data Table

      // Set the Data Properties for the field to map to the data table
      dgSelection.DataPropertyName = "Selection";
      dgOperator.DataPropertyName = "Operator";
      dgRestriction.DataPropertyName = "Restriction";
      dgLimit.DataPropertyName = "Limit";
      dgViewAs.DataPropertyName = "ViewAs";
      dgSortBy.DataPropertyName = "SortBy";
      dgAsc.DataPropertyName = "SortAsc";
      dgSkip.DataPropertyName = "Skip";

      //Set the Data Grid Source as the Data Table created above
      dataGrid.AutoGenerateColumns = false;
      dataGrid.DataSource = datasetFilters;

      // Setup the Views Grid
      datasetViews = new DataTable("Views");

      dtCol = new DataColumn("ViewName");
      dtCol.DataType = Type.GetType("System.String");
      dtCol.DefaultValue = "";
      datasetViews.Columns.Add(dtCol);

      dtCol = new DataColumn("LocalisedName");
      dtCol.DataType = Type.GetType("System.String");
      dtCol.DefaultValue = "";
      datasetViews.Columns.Add(dtCol);

      dtCol = new DataColumn("View");
      dtCol.DataType = typeof (ViewDefinition);
      dtCol.DefaultValue = null;
      datasetViews.Columns.Add(dtCol);

      dgViewName.DataPropertyName = "ViewName";
      dgLocalisedName.DataPropertyName = "LocalisedName";

      dataGridViews.AutoGenerateColumns = false;
      dataGridViews.DataSource = datasetViews;
    }

    private void LoadViews()
    {
      updating = true;
      datasetViews.Rows.Clear();
      foreach (ViewDefinition view in views)
      {
        if (view.Name != string.Empty)
        {
          datasetViews.Rows.Add(
            new object[]
              {
                view.LocalizedName,
                view.Name,
                view,
              }
            );
        }
      }

      updating = false;

      if (dataGridViews.Rows.Count > 0)
      {
        dataGridViews.Rows[0].Selected = true;
        UpdateView();
      }
    }

    #endregion

    #region Private Methods

    private void UpdateView()
    {
      datasetFilters.Clear();
      int selectedRow = 0;
      if (dataGridViews.SelectedRows.Count > 0)
      {
        selectedRow = dataGridViews.SelectedRows[0].Index;
      }
      currentView = (ViewDefinition)datasetViews.Rows[selectedRow][2];
      if (currentView == null)
      {
        return;
      }

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
          new object[]
            {
              def.Where,
              def.SqlOperator,
              def.Restriction,
              limit,
              def.DefaultView,
              def.DefaultSort,
              def.SortAscending,
              def.SkipLevel,
            }
          );
      }
    }

    #endregion

    #region Event Handler

    /// <summary>
    /// A new View has selected. 
    /// Store the changes of the current one and fill the grid with the data of the selected one
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void dataGridViews_SelectionChanged(object sender, EventArgs e)
    {
      if (updating)
      {
        return;
      }
      StoreGridInView();
      UpdateView();
    }

    /// <summary>
    /// Store the Grid Values in the View
    /// </summary>
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
        def.SortAscending = (bool)row[6];
        def.SkipLevel = (bool)row[7];
        view.Filters.Add(def);
      }
    }

    /// <summary>
    /// Add a new View entry 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnAdd_Click(object sender, EventArgs e)
    {
      updating = true;
      ViewDefinition view = new ViewDefinition();
      view.Name = "..new";

      datasetViews.Rows.Add(
        new object[]
          {
            view.LocalizedName,
            view.Name,
            view,
          }
        );

      dataGridViews.Rows[dataGridViews.Rows.Count - 1].Selected = true;
      dataGridViews.FirstDisplayedScrollingRowIndex = dataGridViews.Rows.Count - 1;
      dataGridViews.CurrentCell = dataGridViews.Rows[dataGridViews.Rows.Count - 1].Cells[1];
      dataGridViews.BeginEdit(false);
      currentView = view;

      datasetFilters.Rows.Clear();
      DataRow row = datasetFilters.NewRow();
      row[0] = row[1] = row[2] = row[3] = row[5] = "";
      row[4] = ViewsAs[0]; // Set default Value
      row[6] = true;
      row[7] = false;
      datasetFilters.Rows.Add(row);
      updating = false;
    }

    /// <summary>
    /// Delete the selected View
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnDelete_Click(object sender, EventArgs e)
    {
      updating = true;
      if (dataGridViews.CurrentRow != null)
      {
        dataGridViews.Rows.Remove(dataGridViews.CurrentRow);
      }
      updating = false;

      if (dataGridViews.CurrentRow != null)
      {
        int newSelection = dataGridViews.CurrentRow.Index - 1;
        if (newSelection > -1)
        {
          dataGridViews.Rows[newSelection].Selected = true;
        }
      }
    }

    /// <summary>
    /// Set defaults views (will copy view files from MPProgram\defaults directory)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnSetDefaults_Click(object sender, EventArgs e)
    {
      string defaultViews = Path.Combine(ViewHandler.DefaultsDirectory, _section + "Views.xml");
      string customViews = Config.GetFile(Config.Dir.Config, _section + "Views.xml");

      if (File.Exists(defaultViews))
      {
        File.Copy(defaultViews, customViews, true);

        views.Clear();

        try
        {
          using (FileStream fileStream = new FileInfo(customViews).OpenRead())
          {
            SoapFormatter formatter = new SoapFormatter();
            views = (ArrayList)formatter.Deserialize(fileStream);
            fileStream.Close();
          }
        }
        catch (Exception) { }
        LoadViews();
      }
    }

    /// <summary>
    /// Only allow valid values to be entered.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void dataGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
      if (e.Exception == null) return;

      // If the user-specified value is invalid, cancel the change 
      if ((e.Context & DataGridViewDataErrorContexts.Commit) != 0 &&
          (typeof (FormatException).IsAssignableFrom(e.Exception.GetType()) ||
           typeof (ArgumentException).IsAssignableFrom(e.Exception.GetType())))
      {
        e.Cancel = true;
      }
      else
      {
        // Rethrow any exceptions that aren't related to the user input.
        e.ThrowException = true;
      }
    }

    /// <summary>
    /// Handles editing of data columns
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void dataGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
    {
      // For combo box and check box cells, commit any value change as soon
      // as it is made rather than waiting for the focus to leave the cell.
      if (!dataGrid.CurrentCell.OwningColumn.GetType().Equals(typeof (DataGridViewTextBoxColumn)))
      {
        dataGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
      }
    }

    /// <summary>
    /// Handles Edit in the Datacolumns of the View Grid
    /// So that the View Name is updated with the choosen Name
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void dataGridViews_CellEndEdit(object sender, DataGridViewCellEventArgs e)
    {
      if (currentView != null)
      {
        currentView.Name = dataGridViews.Rows[e.RowIndex].Cells[1].Value.ToString();
        dataGridViews.Rows[e.RowIndex].Cells[0].Value = currentView.LocalizedName;
      }
    }

    /// <summary>
    /// Handle the Keypress for the Filter Datagrid
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void dataGrid_KeyDown(object sender, KeyEventArgs e)
    {
      int rowSelected = -1;
      if (dataGrid.CurrentRow != null)
      {
        rowSelected = dataGrid.CurrentRow.Index;
      }

      switch (e.KeyCode)
      {
        case System.Windows.Forms.Keys.Insert:
          DataRow row = datasetFilters.NewRow();
          row[0] = row[1] = row[2] = row[3] = row[5] = "";
          row[4] = ViewsAs[0]; // Set default Value
          row[6] = true;
          row[7] = false;
          if (rowSelected == -1)
          {
            rowSelected = 0;
          }
          datasetFilters.Rows.InsertAt(row, rowSelected + 1);
          e.Handled = true;
          break;
        case System.Windows.Forms.Keys.Delete:
          if (rowSelected > -1)
          {
            datasetFilters.Rows.RemoveAt(rowSelected);
          }
          e.Handled = true;
          break;
      }
    }

    #region Drag & Drop

    private void OnMouseDown(object sender, MouseEventArgs e)
    {
      DataGridView dgV = (DataGridView)sender;
      //stores values for drag/drop operations if necessary
      if (dgV.AllowDrop)
      {
        int selectedRow = dgV.HitTest(e.X, e.Y).RowIndex;
        if (selectedRow > -1)
        {
          Size DragSize = SystemInformation.DragSize;
          _dragDropRectangle = new Rectangle(new Point(e.X - (DragSize.Width / 2), e.Y - (DragSize.Height / 2)),
                                             DragSize);
          _dragDropSourceIndex = selectedRow;
          _dragDropInitiatingGrid = dgV.Name;
        }
      }
      else
      {
        _dragDropRectangle = Rectangle.Empty;
        _dragDropInitiatingGrid = "";
      }

      base.OnMouseDown(e);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      DataGridView dgV = (DataGridView)sender;
      if (dgV.AllowDrop)
      {
        if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
        {
          if (dgV.Name == _dragDropInitiatingGrid)
          {
            if (_dragDropRectangle != Rectangle.Empty && !_dragDropRectangle.Contains(e.X, e.Y))
            {
              DragDropEffects DropEffect = dgV.DoDragDrop(dgV.Rows[_dragDropSourceIndex],
                                                          DragDropEffects.Move);
            }
          }
        }
      }
      base.OnMouseMove(e);
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
      DataGridView dgV = (DataGridView)sender;
      //runs while the drag/drop is in progress
      if (dgV.AllowDrop)
      {
        e.Effect = DragDropEffects.Move;
        int CurRow =
          dgV.HitTest(dgV.PointToClient(new Point(e.X, e.Y)).X,
                      dgV.PointToClient(new Point(e.X, e.Y)).Y).RowIndex;
        if (_dragDropCurrentIndex != CurRow)
        {
          _dragDropCurrentIndex = CurRow;
          dgV.Invalidate(); //repaint
        }
      }
      base.OnDragOver(e);
    }

    private void OnDragDrop(object sender, DragEventArgs drgevent)
    {
      updating = true;
      DataGridView dgV = (DataGridView)sender;
      //runs after a drag/drop operation for column/row has completed
      if (dgV.AllowDrop)
      {
        if (drgevent.Effect == DragDropEffects.Move)
        {
          Point ClientPoint = dgV.PointToClient(new Point(drgevent.X, drgevent.Y));

          _dragDropTargetIndex = dgV.HitTest(ClientPoint.X, ClientPoint.Y).RowIndex;
          if (_dragDropTargetIndex > -1 && _dragDropCurrentIndex < dgV.RowCount - 1)
          {
            _dragDropCurrentIndex = -1;

            if (dgV.Name == "dataGrid")
            {
              DataRow row = datasetFilters.NewRow();
              // Copy the existing row elements, before removing it from table
              for (int i = 0; i < datasetFilters.Columns.Count; i++)
              {
                row[i] = datasetFilters.Rows[_dragDropSourceIndex][i];
              }
              datasetFilters.Rows.RemoveAt(_dragDropSourceIndex);

              if (_dragDropTargetIndex > _dragDropSourceIndex)
                _dragDropTargetIndex--;

              datasetFilters.Rows.InsertAt(row, _dragDropTargetIndex);
            }
            else if (dgV.Name == "dataGridViews")
            {
              DataRow row = datasetViews.NewRow();
              // Copy the existing row elements
              for (int i = 0; i < datasetViews.Columns.Count; i++)
              {
                row[i] = datasetViews.Rows[_dragDropSourceIndex][i];
              }
              datasetViews.Rows.RemoveAt(_dragDropSourceIndex);

              if (_dragDropTargetIndex > _dragDropSourceIndex)
                _dragDropTargetIndex--;

              datasetViews.Rows.InsertAt(row, _dragDropTargetIndex);
            }

            dgV.ClearSelection();
            dgV.Rows[_dragDropTargetIndex].Selected = true;
          }
        }
      }
      base.OnDragDrop(drgevent);
      updating = false;
    }

    private void OnCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
      DataGridView dgV = (DataGridView)sender;
      if (_dragDropCurrentIndex > -1)
      {
        if (e.RowIndex == _dragDropCurrentIndex && _dragDropCurrentIndex < dgV.RowCount - 1)
        {
          //if this cell is in the same row as the mouse cursor
          Pen p = new Pen(Color.Red, 3);
          e.Graphics.DrawLine(p, e.CellBounds.Left, e.CellBounds.Top - 1, e.CellBounds.Right, e.CellBounds.Top - 1);
        }
      }
    }

    #endregion

    #endregion

    #region Overridden Methods

    /// <summary>
    /// Load the Views
    /// </summary>
    /// <param name="mediaType"></param>
    /// <param name="selections"></param>
    /// <param name="sqloperators"></param>
    /// <param name="viewsAs"></param>
    /// <param name="sortBy"></param>
    protected void LoadSettings(
      string mediaType,
      string[] selections,
      string[] sqloperators,
      string[] viewsAs,
      string[] sortBy
      )
    {
      string defaultViews = Path.Combine(ViewHandler.DefaultsDirectory, mediaType + "Views.xml");
      string customViews = Config.GetFile(Config.Dir.Config, mediaType + "Views.xml");
      Selections = selections;
      Sqloperators = sqloperators;
      ViewsAs = viewsAs;
      SortBy = sortBy;

      if (!File.Exists(customViews))
      {
        File.Copy(defaultViews, customViews);
      }
      else
      {
        // Let's see, if we got a pre 1.2 file
        try
        {
          // Can't use XPath here, as the XML Namespace is dependend on the version of MP Core
          // And this might change.
          // So we iterate through the file, until we found a filterdef.
          XmlDocument xmlDoc = new XmlDocument();
          xmlDoc.Load(customViews);
          XmlElement rootElement = xmlDoc.DocumentElement;
          if (rootElement != null)
          {
            XmlNode body = rootElement.ChildNodes[0];
            foreach (XmlNode node in body.ChildNodes)
            {
              if (node.Name == "a3:FilterDefinition")
              {
                XmlNode skipLevel = node.SelectSingleNode("skipLevel");
                if (skipLevel == null)
                {
                  MediaPortal.GUI.Library.Log.Info("Views: Found old view format: {0} Copying default views.",
                                                   customViews);
                  File.Copy(defaultViews, customViews, true);
                  break;
                }
                break;
              }
            }
          }
        }
        catch (Exception)
        {
          MediaPortal.GUI.Library.Log.Error("Views: Exception reading view {0}. Copying default views.", customViews);
          File.Copy(defaultViews, customViews, true);
        }
      }

      views = new ArrayList();

      try
      {
        using (FileStream fileStream = new FileInfo(customViews).OpenRead())
        {
          SoapFormatter formatter = new SoapFormatter();
          views = (ArrayList)formatter.Deserialize(fileStream);
          fileStream.Close();
        }
      }
      catch (Exception) {}

      SetupGrid();
      LoadViews();
    }


    /// <summary>
    /// Save the Views
    /// </summary>
    /// <param name="mediaType"></param>
    protected void SaveSettings(string mediaType)
    {
      StoreGridInView(); // Save pending changes
      string customViews = Config.GetFile(Config.Dir.Config, mediaType + "Views.xml");
      if (settingsChanged)
      {
        // Rebuild the Arraylist with the views out of the Datagrid
        views.Clear();
        foreach (DataGridViewRow row in dataGridViews.Rows)
        {
          ViewDefinition view = (ViewDefinition)datasetViews.Rows[row.Index][2];
          if (view.Filters.Count > 0)
          {
            views.Add(view);
          }
        }

        try
        {
          // Don't use FileInfo.OpenWrite
          // From msdn:
          //  If you overwrite a longer string (such as "This is a test of the OpenWrite method") with a shorter string (like "Second run"), the file will contain a mix of the strings ("Second runtest of the OpenWrite method").
          using (FileStream fileStream = new FileStream(customViews, FileMode.Truncate))
          {
            SoapFormatter formatter = new SoapFormatter();
            formatter.Serialize(fileStream, views);
            fileStream.Close();
          }
        }
        catch (Exception) {}
      }
    }
    
    #endregion
    
  }
}