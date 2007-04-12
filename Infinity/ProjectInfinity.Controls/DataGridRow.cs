using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ProjectInfinity.Controls
{
  public class DataGridRow
  {
    int _rowSpan=-1;
    List<DataGridCell> _cells = new List<DataGridCell>();
    DataGrid _grid;
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridRow"/> class.
    /// </summary>
    public DataGridRow()
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridRow"/> class.
    /// </summary>
    /// <param name="rowSpan">The row span.</param>
    public DataGridRow(int rowSpan)
    {
      _rowSpan = rowSpan;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridRow"/> class.
    /// </summary>
    /// <param name="cell">The cell.</param>
    public DataGridRow(DataGridCell cell)
    {
      _cells.Add(cell);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridRow"/> class.
    /// </summary>
    /// <param name="cell">The cell.</param>
    /// <param name="rowSpan">The row span.</param>
    public DataGridRow(DataGridCell cell, int rowSpan)
    {
      _cells.Add(cell);
      _rowSpan = rowSpan;
    }
    /// <summary>
    /// Gets or sets the data grid.
    /// </summary>
    /// <value>The data grid.</value>
    public DataGrid DataGrid
    {
      get
      {
        return _grid;
      }
      set
      {
        _grid = value;
      }
    }

    /// <summary>
    /// Gets or sets the row span.
    /// </summary>
    /// <value>The row span.</value>
    public int RowSpan
    {
      get
      {
        return _rowSpan;
      }
      set
      {
        _rowSpan = value;
      }
    }

    /// <summary>
    /// Gets or sets the cells.
    /// </summary>
    /// <value>The cells.</value>
    public List<DataGridCell> Cells
    {
      get
      {
        return _cells;
      }
      set
      {
        _cells = value;
      }
    }

  }
}
