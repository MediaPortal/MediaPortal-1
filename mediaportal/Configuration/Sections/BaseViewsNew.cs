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
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.GUI.DatabaseViews;
using MediaPortal.GUI.Library;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class BaseViewsNew : SectionSettings
  {
    #region Variables

    private DataTable _datasetLevels;
    private DatabaseViewDefinition _currentView;
    private List<DatabaseViewDefinition> _views;
    private bool _updating = false;
    private bool _settingsChanged = false;

    private List<string> _selections = new List<string>();
    private List<string> _viewsAs = new List<string>();
    private List<string> _sortBy = new List<string>();

    // Drag & Drop
    private int _dragDropCurrentIndex = -1;
    private Rectangle _dragDropRectangle;
    private int _dragDropSourceIndex;
    private int _dragDropTargetIndex;

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
        _section = value;
      }
    }

    #endregion

    #region ctor

    public BaseViewsNew()
      : base("<Unknown>")
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public BaseViewsNew(string name)
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
      arrColumnNames = new string[3];
      arrColumnNames[0] = "Selection";
      arrColumnNames[1] = "ViewAs";
      arrColumnNames[2] = "SortBy";
      

      //Create the Data Table object which will then be used to hold
      //columns and rows
      _datasetLevels = new DataTable();

      //Add the string array of columns to the DataColumn object       
      for (int i = 0; i < arrColumnNames.Length; i++)
      {
        string str = arrColumnNames[i];
        dtCol = new DataColumn(str);
        dtCol.DataType = Type.GetType("System.String");
        dtCol.DefaultValue = "";
        _datasetLevels.Columns.Add(dtCol);
      }

      // Add 2 columns with checkbox at the end of the Datarow     
      DataColumn dtcCheck = new DataColumn("SortAsc"); //create the data column object
      dtcCheck.DataType = Type.GetType("System.Boolean"); //Set its data Type
      dtcCheck.DefaultValue = true; //Set the default value
      dtcCheck.AllowDBNull = false;
      _datasetLevels.Columns.Add(dtcCheck); //Add the above column to the Data Table

      DataColumn skipCheck = new DataColumn("Skip"); //create the data column object
      skipCheck.DataType = Type.GetType("System.Boolean"); //Set its data Type
      skipCheck.DefaultValue = false; //Set the default value
      skipCheck.AllowDBNull = false;
      _datasetLevels.Columns.Add(skipCheck); //Add the above column to the Data Table

      // Set the Data Properties for the field to map to the data table
      dgSelection.DataPropertyName = "Selection";
      dgViewAs.DataPropertyName = "ViewAs";
      dgSortBy.DataPropertyName = "SortBy";
      dgSortAsc.DataPropertyName = "SortAsc";
      dgSkip.DataPropertyName = "Skip";

      //Set the Data Grid Source as the Data Table created above
      dataGrid.AutoGenerateColumns = false;
      dataGrid.DataSource = _datasetLevels;
    }

    private void LoadTreeView()
    {
      _updating = true;

      foreach (DatabaseViewDefinition view in _views)
      {
        TreeNode node = new TreeNode();
        node.Text = view.LocalizedName;
        node.Tag = view;
        SetNodeColor(node);
        AddSubNodes(ref node, view);
        treeViewMenu.Nodes.Add(node);
      }

      treeViewMenu.ExpandAll();

      dataGrid.Hide();
      lblActionCodes.Hide();
      _updating = false;

    }

    /// <summary>
    /// Add the subnodes recusrively
    /// </summary>
    /// <param name="parentNode"></param>
    /// <param name="view"></param>
    private void AddSubNodes(ref TreeNode parentNode, DatabaseViewDefinition view)
    {
      foreach (DatabaseViewDefinition subView in view.SubViews)
      {
        TreeNode subNode = new TreeNode(subView.LocalizedName);
        subView.Parent = view.Id.ToString();
        subNode.Tag = subView;
        SetNodeColor(subNode);
        AddSubNodes(ref subNode, subView);
        parentNode.Nodes.Add(subNode);
      }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Fill the Grid with the selected data
    /// </summary>
    private void FillViewGrid()
    {
      _datasetLevels.Clear();
      if (_currentView == null)
      {
        return;
      }

      //fill in all rows...
      foreach (DatabaseFilterLevel level in _currentView.Levels)
      {
        _datasetLevels.Rows.Add(
          new object[]
            {
              level.Selection,
              level.DefaultView,
              level.SortBy,
              level.SortAscending,
              level.SkipLevel,
            }
          );
      }
      dataGrid.DataSource = _datasetLevels;
    }

    /// <summary>
    /// Store changes in the view
    /// </summary>
    private void StoreGridInView()
    {
      if (_updating)
      {
        return;
      }
      if (dataGrid.DataSource == null)
      {
        return;
      }
      if (_currentView == null)
      {
        return;
      }

      _currentView.Levels.Clear();
      int i = 0;
      foreach (DataRow row in _datasetLevels.Rows)
      {
        var level = new DatabaseFilterLevel();
        level.Selection = (string)row[0];
        level.DefaultView = (string)row[1];
        level.SortBy = (string)row[2];
        level.SortAscending = (bool)row[3];
        level.SkipLevel = (bool)row[4];
        _currentView.Levels.Add(level);
        i++;
      }
    }

    /// <summary>
    /// Set the Treenode color, to indicate, if a node has filters
    /// </summary>
    /// <param name="node"></param>
    private void SetNodeColor(TreeNode node)
    {
      var view = (DatabaseViewDefinition)node.Tag;
      if (view == null)
      {
        return;
      }

      if (view.Filters.Count > 0 )
      {
        node.BackColor = Color.Yellow;
      }
      else
      {
        node.BackColor = Color.White;
      }
    }

    #endregion

    #region Event Handler
    
    /// <summary>
    /// Add a new View entry 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnAdd_Click(object sender, EventArgs e)
    {
      _settingsChanged = true;

      StoreGridInView(); // Save possible pending changes in current Node

      var view = new DatabaseViewDefinition {Id = Guid.NewGuid()};
      var treeNode = new TreeNode("New View") {Tag = view};
      treeViewMenu.Nodes.Add(treeNode);
      treeViewMenu.SelectedNode = treeNode;

      _datasetLevels.Rows.Clear();
      var row = _datasetLevels.NewRow();
      row[0] = "";
      row[1] = ViewsAs[0]; // Set default Value
      row[2] = SortBy[0];  // Set default Value
      row[3] = true;
      row[4] = false;
      _datasetLevels.Rows.Add(row);

      btnDeleteView.Enabled = false;
      btnEditFilter.Enabled = false;
      btnCopyView.Enabled = false;
      btnUpView.Enabled = false;
      btnDownView.Enabled = false;
    }

    /// <summary>
    /// Delete the selected View
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnDelete_Click(object sender, EventArgs e)
    {
      _updating = true;

      TreeNode selectedNode = treeViewMenu.SelectedNode;
      if (selectedNode == null)
      {
        return;
      }

      bool removeNode = true;
      if (selectedNode.Nodes.Count > 0)
      {
        if (MessageBox.Show("The selected View has SubViews.\r\nDo you really want to delete?", "Delete View", MessageBoxButtons.YesNo,MessageBoxIcon.Warning) == DialogResult.No)
        {
          removeNode = false;
        }
      }

      if (removeNode)
      {
        _settingsChanged = true;
        selectedNode.Remove();
        treeViewMenu.Invalidate();

        btnDeleteView.Enabled = false;
        btnEditFilter.Enabled = false;
        btnCopyView.Enabled = false;
        btnUpView.Enabled = false;
        btnDownView.Enabled = false;
      }

      _updating = false;
    }

    /// <summary>
    /// Edit a new Filter
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btEditFilter_Click(object sender, EventArgs e)
    {
      if (_currentView == null)
      {
        return;
      }

      BaseViewsFilter filterForm = new BaseViewsFilter(this);
      filterForm.Filter = _currentView.Filters;
      if (filterForm.ShowDialog() == DialogResult.OK)
      {
        _currentView.Filters = filterForm.Filter;
        treeViewMenu.SelectedNode.Tag = _currentView;
        SetNodeColor(treeViewMenu.SelectedNode);
        filterForm.Dispose();
        _settingsChanged = true;
      }
    }


    private void btnUpView_Click(object sender, EventArgs e)
    {
      TreeNode node = treeViewMenu.SelectedNode;
      TreeNode parent = node.Parent;
      treeViewMenu.BeginUpdate();
      if (parent != null)
      {
        int index = parent.Nodes.IndexOf(node);
        if (index > 0)
        {
          parent.Nodes.RemoveAt(index);
          parent.Nodes.Insert(index - 1, node);
        }
      }
      else if (treeViewMenu.Nodes.Contains(node)) //root node
      {
        int index = treeViewMenu.Nodes.IndexOf(node);
        if (index > 0)
        {
          treeViewMenu.Nodes.RemoveAt(index);
          treeViewMenu.Nodes.Insert(index - 1, node);
        }
      }
      _settingsChanged = true;
      treeViewMenu.EndUpdate();
      treeViewMenu.SelectedNode = node;
    }

    private void btnDownView_Click(object sender, EventArgs e)
    {
      TreeNode node = treeViewMenu.SelectedNode;
      TreeNode parent = node.Parent;
      treeViewMenu.BeginUpdate();
      if (parent != null)
      {
        int index = parent.Nodes.IndexOf(node);
        if (index < parent.Nodes.Count - 1)
        {
          parent.Nodes.RemoveAt(index);
          parent.Nodes.Insert(index + 1, node);
        }
      }
      else if (treeViewMenu.Nodes.Contains(node)) //root node
      {
        int index = treeViewMenu.Nodes.IndexOf(node);
        if (index < treeViewMenu.Nodes.Count - 1)
        {
          treeViewMenu.Nodes.RemoveAt(index);
          treeViewMenu.Nodes.Insert(index + 1, node);
        }
      }
      _settingsChanged = true;
      treeViewMenu.EndUpdate();
      treeViewMenu.SelectedNode = node;
    }

    private void btnCopyView_Click(object sender, EventArgs e)
    {
      TreeNode clonedNode = (TreeNode)treeViewMenu.SelectedNode.Clone();
      var view = (DatabaseViewDefinition)clonedNode.Tag;
      clonedNode.Tag = view.Clone();
      treeViewMenu.Nodes.Add(clonedNode);
      treeViewMenu.SelectedNode = clonedNode;
      _settingsChanged = true;
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

      if (MessageBox.Show("This will clear your existing View settings.\r\nDo you really want to revert to default views?", "Revert to Default Views", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
      {
        return;
      }

      if (File.Exists(defaultViews))
      {
        File.Copy(defaultViews, customViews, true);

        treeViewMenu.Nodes.Clear();
        _views.Clear();

        try
        {
          using (FileStream fileStream = new FileInfo(customViews).OpenRead())
          {
            XmlSerializer serializer = new XmlSerializer(typeof(List<DatabaseViewDefinition>));
            _views = (List<DatabaseViewDefinition>)serializer.Deserialize(fileStream);
            fileStream.Close();
          }
        }
        catch (Exception) { }
        _settingsChanged = true;
        LoadTreeView();
      }
    }

    #region TreeView

    /// <summary>
    /// Selection has changed. Save any changes to the node
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void treeViewMenu_BeforeSelect(object sender, TreeViewCancelEventArgs e)
    {
      if (treeViewMenu.SelectedNode == null)
      {
        return;
      }

      _currentView = (DatabaseViewDefinition)treeViewMenu.SelectedNode.Tag;
      StoreGridInView();
      treeViewMenu.SelectedNode.Tag = _currentView;
    }

    /// <summary>
    /// A new node has been selected
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void treeViewMenu_AfterSelect(object sender, TreeViewEventArgs e)
    {
      _currentView = (DatabaseViewDefinition)treeViewMenu.SelectedNode.Tag;
      if (treeViewMenu.SelectedNode.Nodes.Count > 0)
      {
        _datasetLevels.Clear();
        dataGrid.Hide();
        lblActionCodes.Hide();
      }
      else
      {
        FillViewGrid();
        dataGrid.Show();
        lblActionCodes.Show();
      }

      btnDeleteView.Enabled = true;
      btnEditFilter.Enabled = true;
      btnCopyView.Enabled = true;
      btnUpView.Enabled = true;
      btnDownView.Enabled = true;
    }

    /// <summary>
    /// Set the Localised Name into the Label Text
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void treeViewMenu_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
    {
      // Don't allow empty labels
      if (e.Label == null)
      {
        e.CancelEdit = false;
        return;
      }

      var view = (DatabaseViewDefinition)e.Node.Tag;
      view.Name = e.Label;
      e.Node.Tag = view;
      e.Node.Text = view.LocalizedName;
      e.CancelEdit = true; // We want to have our localised version of the text
      _settingsChanged = true;
    }

    #endregion

    #region Filter Grid

    /// <summary>
    /// Handle the click on the Add / Delete Button columns
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void dataGrid_CellClick(object sender, DataGridViewCellEventArgs e)
    {
      // Ignore clicks that are not on button cells.
      if (e.ColumnIndex < 5)
      {
        return;
      }

      switch (e.ColumnIndex)
      {
        case 5: // Add
          DataRow row = _datasetLevels.NewRow();
          row[0] = "";
          row[1] = ViewsAs[0]; // Set default Value
          row[2] = SortBy[0];  // Set default Value
          row[3] = true;
          row[4] = false;
          _datasetLevels.Rows.InsertAt(row, e.RowIndex + 1);
          break;

        case 6: // Delete
          _datasetLevels.Rows.RemoveAt(e.RowIndex);
          break;
      }

      _settingsChanged = true;
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
          (typeof(FormatException).IsAssignableFrom(e.Exception.GetType()) ||
           typeof(ArgumentException).IsAssignableFrom(e.Exception.GetType())))
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
      if (!dataGrid.CurrentCell.OwningColumn.GetType().Equals(typeof(DataGridViewTextBoxColumn)))
      {
        dataGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
      }
      _settingsChanged = true;
    }

    #endregion

    #region Drag & Drop

    #region Treeview


    private void treeViewMenu_ItemDrag(object sender, ItemDragEventArgs e)
    {
      StoreGridInView(); // Save possible pending changes in current Node
      DoDragDrop(e.Item, DragDropEffects.Move | DragDropEffects.Copy);
    }

    private void treeViewMenu_DragEnter(object sender, DragEventArgs e)
    {
      // Ctrl-Key pressed? 
      if ((e.KeyState & 8) == 8)
      {
        e.Effect = DragDropEffects.Copy;
      }
      else
      {
        e.Effect = DragDropEffects.Move;
      }
    }

    private void treeViewMenu_DragDrop(object sender, DragEventArgs e)
    {
      TreeNode sourceNode = e.Data.GetData(typeof(TreeNode)) as TreeNode;

      Point p = treeViewMenu.PointToClient(new Point(e.X, e.Y));
      TreeNode targetNode = treeViewMenu.GetNodeAt(p);

      // Ignore, if Target and source is the same
      if (sourceNode == targetNode)
      {
        return;
      }

      TreeNode newNode = (TreeNode)sourceNode.Clone();

      if (targetNode != null)
      {
        var parentView = (DatabaseViewDefinition)targetNode.Tag;
        var view = (DatabaseViewDefinition)newNode.Tag;
        view.Parent = parentView.Id.ToString();
        newNode.Tag = view;
        targetNode.Nodes.Add(newNode);
      }
      else
      {
        // Add new Node to Root of Treeview
        var view = (DatabaseViewDefinition)newNode.Tag;
        view.Parent = Guid.Empty.ToString();
        newNode.Tag = view;
        treeViewMenu.Nodes.Add(newNode);
      }

      if (e.Effect == DragDropEffects.Move)
      {
        sourceNode.Remove();
      }
      _settingsChanged = true;
      treeViewMenu.ExpandAll();
      treeViewMenu.Invalidate();
    }

    #endregion

    #region Datagrid

    private void dataGrid_OnMouseDown(object sender, MouseEventArgs e)
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
        }
      }
      else
      {
        _dragDropRectangle = Rectangle.Empty;
      }

      base.OnMouseDown(e);
    }

    private void dataGrid_OnMouseMove(object sender, MouseEventArgs e)
    {
      DataGridView dgV = (DataGridView)sender;
      if (dgV.AllowDrop)
      {
        if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
        {
          if (_dragDropRectangle != Rectangle.Empty && !_dragDropRectangle.Contains(e.X, e.Y))
          {
            DragDropEffects DropEffect = dgV.DoDragDrop(dgV.Rows[_dragDropSourceIndex],
                                                        DragDropEffects.Move);
          }
        }
      }
      base.OnMouseMove(e);
    }

    private void dataGrid_OnDragOver(object sender, DragEventArgs e)
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

    private void dataGrid_OnDragDrop(object sender, DragEventArgs drgevent)
    {
      _updating = true;
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
              DataRow row = _datasetLevels.NewRow();
              // Copy the existing row elements, before removing it from table
              for (int i = 0; i < _datasetLevels.Columns.Count; i++)
              {
                row[i] = _datasetLevels.Rows[_dragDropSourceIndex][i];
              }
              _datasetLevels.Rows.RemoveAt(_dragDropSourceIndex);

              if (_dragDropTargetIndex > _dragDropSourceIndex)
                _dragDropTargetIndex--;

              _datasetLevels.Rows.InsertAt(row, _dragDropTargetIndex);
            }

            dgV.ClearSelection();
            dgV.Rows[_dragDropTargetIndex].Selected = true;
          }
        }
      }
      base.OnDragDrop(drgevent);
      _updating = false;
    }

    private void dataGrid_OnCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
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

    #endregion

    #region Overridden Methods

    /// <summary>
    /// Load the Views
    /// </summary>
    /// <param name="mediaType"></param>
    /// <param name="selections"></param>
    /// <param name="viewsAs"></param>
    /// <param name="sortBy"></param>
    protected void LoadSettings(
      string mediaType,
      string[] selections,
      string[] viewsAs,
      string[] sortBy
      )
    {
      string defaultViews = Path.Combine(ViewHandler.DefaultsDirectory, mediaType + "Views.xml");
      string customViews = Config.GetFile(Config.Dir.Config, mediaType + "Views.xml");
      Selections = selections;
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
                MediaPortal.GUI.Library.Log.Info("Views: Found old view format: {0} Copying default views.",
                                                 customViews);
                File.Copy(defaultViews, customViews, true);
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

      _views = new List<DatabaseViewDefinition>();

      try
      {
        using (FileStream fileStream = new FileInfo(customViews).OpenRead())
        {
          XmlSerializer serializer = new XmlSerializer(typeof(List<DatabaseViewDefinition>));
          _views = (List<DatabaseViewDefinition>)serializer.Deserialize(fileStream);
          fileStream.Close();
        }
      }
      catch (Exception) { }

      SetupGrid();
      LoadTreeView();
    }


    /// <summary>
    /// Save the Views
    /// </summary>
    /// <param name="mediaType"></param>
    protected void SaveSettings(string mediaType)
    {
      StoreGridInView(); // Save pending changes
      if (_settingsChanged)
      {
        _views.Clear();
        foreach (TreeNode node in treeViewMenu.Nodes)
        {
          var view = (DatabaseViewDefinition)node.Tag;
          view.SubViews.Clear();
          view.SubViews = GetSubNodes(node);
          _views.Add(view);
        }

        // Now Serialize the View to the XML
        XmlSerializer serializer = new XmlSerializer(typeof(List<DatabaseViewDefinition>));

        string customViews = Config.GetFile(Config.Dir.Config, mediaType + "Views.xml");
        Stream fs = new FileStream(customViews, FileMode.Create);
        XmlWriterSettings writerSettings = new XmlWriterSettings();
        writerSettings.Indent = true;
        writerSettings.Encoding = Encoding.Unicode;
        XmlWriter writer = XmlWriter.Create(fs, writerSettings);

        // Serialize using the XmlTextWriter.
        serializer.Serialize(writer, _views);
        writer.Close();
      }
    }

    /// <summary>
    /// Loop recursive through the Treeview to get all the subnodes
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private List<DatabaseViewDefinition> GetSubNodes(TreeNode node)
    {
      var subViews = new List<DatabaseViewDefinition>();
      if (node.Nodes.Count > 0)
      {
        foreach (TreeNode subNode in node.Nodes)
        {
          var view = (DatabaseViewDefinition) subNode.Tag;
          view.SubViews = GetSubNodes(subNode);
          subViews.Add(view);
        }
      }

      return subViews;
    } 

    #endregion
  }
}