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
    public DataGridRow()
    {
    }
    public DataGridRow(int rowSpan)
    {
      _rowSpan = rowSpan;
    }
    public DataGridRow(DataGridCell cell)
    {
      _cells.Add(cell);
    }
    public DataGridRow(DataGridCell cell, int rowSpan)
    {
      _cells.Add(cell);
      _rowSpan = rowSpan;
    }
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
