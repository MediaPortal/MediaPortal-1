using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
namespace ProjectInfinity.Controls
{
  public class DataGridCell 
  {
    int _columnSpan = -1;
    int _column = -1;
    DataGrid _grid;
    FrameworkElement _content;

    public DataGridCell()
    {
    }
    public DataGridCell(int column)
    {
      _column = column;
    }
    public DataGridCell(int column, int columnSpan)
    {
      _column = column;
      _columnSpan = columnSpan;
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

    public FrameworkElement Content
    {
      get
      {
        return _content;
      }
      set
      {
        _content = value;
        _content.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(_content_GotKeyboardFocus);
      }
    }

    void _content_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      _grid.OnItemSelected(this);
    }

    public int ColumnSpan
    {
      get
      {
        return _columnSpan;
      }
      set
      {
        _columnSpan = value;
      }
    }

    public int Column
    {
      get
      {
        return _column;
      }
      set
      {
        _column = value;
      }
    }


  }
}
