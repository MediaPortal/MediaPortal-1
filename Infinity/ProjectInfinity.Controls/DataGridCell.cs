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

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridCell"/> class.
    /// </summary>
    public DataGridCell()
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridCell"/> class.
    /// </summary>
    /// <param name="column">The column.</param>
    public DataGridCell(int column)
    {
      _column = column;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridCell"/> class.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="columnSpan">The column span.</param>
    public DataGridCell(int column, int columnSpan)
    {
      _column = column;
      _columnSpan = columnSpan;
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
    /// Gets or sets the content.
    /// </summary>
    /// <value>The content.</value>
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

    /// <summary>
    /// Handles the GotKeyboardFocus event of the _content control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Input.KeyboardFocusChangedEventArgs"/> instance containing the event data.</param>
    void _content_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      _grid.OnItemSelected(this);
    }

    /// <summary>
    /// Gets or sets the column span.
    /// </summary>
    /// <value>The column span.</value>
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

    /// <summary>
    /// Gets or sets the column.
    /// </summary>
    /// <value>The column.</value>
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
