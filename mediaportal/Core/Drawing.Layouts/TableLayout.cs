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
using System.Windows;
using System.Windows.Controls;
using MediaPortal.GUI.Library;

namespace MediaPortal.Drawing.Layouts
{
  public class TableLayout : ILayout
  {
    #region Fields

    private double _width; // Overall table width.
    private int _cols; // The number of table columns.
    private ArrayList _columns = new ArrayList(); // Info about each column not accounting for spacing.
    private ArrayList _rowHeight = new ArrayList(); // Height of each row not accounting for spacing.
    private Size _size = Size.Empty;
    private Size _spacing = Size.Empty;

    private class Column
    {
      public double width = 0;
      public bool isCalculated = true;

      public Column(double w)
      {
        width = w;
      }
    }

    #endregion Fields

    #region Cell Class

    public class Cell : ILayoutDetail
    {
      private int _colSpan = 1;
      private int _rowSpan = 1;
      private int _targetCol = 0;

    #region constructors

      public Cell()
      {
      }

      public Cell(int colSpan)
      {
        ColSpan = colSpan;
      }

      public Cell(int colSpan, int rowSpan)
      {
        ColSpan = colSpan;
        RowSpan = rowSpan;
      }

      public Cell(int colSpan, int rowSpan, int targetCol)
      {
        ColSpan = colSpan;
        RowSpan = rowSpan;
        TargetCol = targetCol;
      }

      #endregion constructors

      public int ColSpan
      {
        get { return _colSpan; }
        set
        {
          if (value > 0)
          {
            _colSpan = value;
          }
          else
          {
            Log.Warn("TableLayout: column span value must be > 0; ignoring specified value");
          }
        }
      }

      public int RowSpan
      {
        get { return _rowSpan; }
        set
        {
          if (value > 0)
          {
            _rowSpan = value;
          }
          else
          {
            Log.Warn("TableLayout: row span value must be > 0; ignoring specified value");
          }
        }
      }

      public int TargetCol
      {
        get { return _targetCol; }
        set
        {
          if (value >= 0)
          {
            _targetCol = value;
          }
        }
      }
    }

    #endregion Cell Class

    #region Constructors

    public TableLayout()
      : this(400)
    {
    }

    public TableLayout(int width)
      : this(width, 1)
    {
    }

    public TableLayout(int width, int columns)
      : this(width, columns, 0)
    {
    }

    public TableLayout(int width, int columns, double spacing)
      : this(width, columns, spacing, spacing)
    {
    }

    public TableLayout(int width, int columns, double horizontalSpacing, double verticalSpacing)
    {
      if (width < 1)
      {
        throw new ArgumentException("width cannot be < 1");
      }

      if (columns < 1)
      {
        throw new ArgumentException("columns cannot be < 1");
      }

      if (horizontalSpacing < 0)
      {
        throw new ArgumentException("horizontal spacing cannot be < 0");
      }

      if (verticalSpacing < 0)
      {
        throw new ArgumentException("vertical spacing cannot be < 0");
      }

      _width = GUIGraphicsContext.ScaleHorizontal(width);
      _cols = columns;
      _spacing.Width = Math.Max(0, horizontalSpacing);
      _spacing.Height = Math.Max(0, verticalSpacing);

      // Default columns are all the same width.
      for (int i = 0; i < _cols; i++)
      {
        _columns.Add( new Column((_width - (_cols-1)*_spacing.Width) / _cols));
      }
    }

    #endregion Constructors

    #region Methods

    private void ApplyAlignment(GUIControl element, Thickness t, double x, double y, double w, double h)
    {
      Rect rect = new Rect(x, y, element.Width, element.Height);

      switch (element.HorizontalAlignment)
      {
        case HorizontalAlignment.Center:
          rect.X = x + ((w - element.Width) / 2);
          break;
        case HorizontalAlignment.Right:
          rect.X = x + w - element.Width;
          break;
        case HorizontalAlignment.Stretch:
          rect.Width = w;
          break;
      }

      switch (element.VerticalAlignment)
      {
        case VerticalAlignment.Center:
          rect.Y = y + ((h - element.Height) / 2);
          break;
        case VerticalAlignment.Bottom:
          rect.Y = y + h - element.Height;
          break;
        case VerticalAlignment.Stretch:
          rect.Height = h;
          break;
      }

      element.Arrange(rect);
    }

    public void Arrange(GUIGroup element)
    {
      Thickness t = element.Margin;

      int rows = _rowHeight.Count; // Calculated number of table rows.
      int cols = _cols; // User specified number of table columns.
      int i = 0; // Index for retrieving child controls.
      GUIControl child;
      Cell TableCell;

      double x = element.Location.X + t.Left;
      double y = element.Location.Y + t.Top;
      double w = 0;
      double h = 0;
      double ch = 0;

      for (int r = 0; r < rows; r++)
      {
        for (int c = 0; c < cols; c++)
        {
          if (i >= element.Children.Count)
          {
            break; // Bailout if we run out of controls while looking at the last row.
          }

          child = element.Children[i];

          if (child.Visibility == Visibility.Collapsed)
          {
            i++; // Advance to the next control.
            continue;
          }

          // Get the table cell details for this child.
          TableCell = GetCell((GUIControl)child);

          // If the target column is not the current column then move the x position to the target column position.
          // A target column value of 0 indicates no target column preference is set.
          if (TableCell.TargetCol > 0 && c != TableCell.TargetCol - 1)
          {
            // Check whether the target column in the current row.
            if (c < TableCell.TargetCol - 1) // Target column value is 1-based.
            {
              for (int a = c; a < TableCell.TargetCol - 1; a++)
              {
                x += ((Column)_columns[a]).width + _spacing.Width;
              }
              c = TableCell.TargetCol - 1;
            }
            else if (TableCell.TargetCol - 1 >= 0)
            {
              // Advance to the next row to set the target column.
              c = cols;
              continue;
            }
          }

          if (TableCell.ColSpan > 1)
          {
            // When spanning columns we must add the spacing.
            w = 0;
            for (int a = c; a <= c + TableCell.ColSpan - 1; a++)
            {
              w += ((Column)_columns[a]).width + _spacing.Width;
              // Log.Info("TableLayout.Arrange: spanning spanWidth={0:0.000}, colWidth={1:0.000}, w={2:0.000}", _spacing.Width, ((Column)_columns[a]).width, w);
            }
            w -= _spacing.Width;
          }
          else
          {
            w = ((Column)_columns[c]).width;
            // Log.Info("TableLayout.Arrange: w={0:0.000}", w);
          }

          h = (double)_rowHeight[r];
          ch = child.Height * TableCell.RowSpan;

          //Log.Info("TableLayout.Arrange: (c={0}, r={1}) x={2:0.000},y={3:0.000},w={4:0.000}", c + 1, r + 1, x, y, w);
          ApplyAlignment(child, t, x, y, w, ch);

          // Move to the last column in the span.
          c += TableCell.ColSpan - 1;

          // Move to the start of the next column.
          x += w + _spacing.Width;

          i++; // Advance to the next control.
        }
        x = element.Location.X + t.Left;
        y += h + _spacing.Height;
      }
    }

    public Size Measure(GUIGroup element, Size availableSize)
    {
      Thickness t = element.Margin;
      double tableContentHeight = 0; // Calculated overall table height less vertical spacing.

      int cols = _cols; // User specified number of table columns.

      int i = 0; // Index for retrieving child controls.
      int c = 1; // Current column number.
      double rowHeight = 0d; // Current row height.
      GUIControl child;
      Cell TableCell;

      while (i < element.Children.Count)
      {
        c = 1;
        while (c <= cols)
        {
          if (i >= element.Children.Count)
          {
            break; // Bailout if we run out of controls while looking at the last row.
          }

          child = element.Children[i];

          if (child.Visibility == Visibility.Collapsed)
          {
            i++; // Advance to the next control.
            continue;
          }

          // Get the table cell details for this child.
          TableCell = GetCell((GUIControl)child);

          if (TableCell.TargetCol > 0 && TableCell.TargetCol < c)
          {
            // We have passed the target column in this row.
            // Advance to the next row.
            c = cols + 1;
            continue;
          }

          if (TableCell.TargetCol > c)
          {
            // Advance to the specified column.
            c = TableCell.TargetCol;
          }

          child.Measure(availableSize);
          rowHeight = Math.Max(rowHeight, (double)child.Height * TableCell.RowSpan);

          // Advance column index according to specified column span.
          c += TableCell.ColSpan - 1;

          // Columns with no span specify the exact desired width of the column.
          if (TableCell.ColSpan == 1)
          {
            ((Column)_columns[c-1]).width = (double)child.Width;
            ((Column)_columns[c-1]).isCalculated = false;
          }

          i++; // Advance to the next control.
          c++; // Advance to the next column.
        }

        // Recalculate column width for each column with unspecified width.
        double span = 0;
        int count = 0;
        foreach (Column col in _columns)
        {
          if (!col.isCalculated)
          {
            span += col.width;
            count++;
          }
        }
        if (count > 0)
        {
          span = (_width - span) / (cols - count);

          foreach (Column col in _columns)
          {
            if (col.isCalculated)
            {
              col.width = span;
            }
          }
        }

        tableContentHeight += rowHeight;
        _rowHeight.Add(rowHeight); // Store the row height.
        rowHeight = 0; // Reset for next row.
      }

      // for (int x = 0; x <= _columns.Count - 1; x++)
      // {
      //   Log.Info("TableLayout.Measure: columns[{0}]={1}, calc={2}", x, ((Column)_columns[x]).width, ((Column)_columns[x]).isCalculated);
      // }

      _size.Width = _width; // Table is fixed width.
      _size.Height = (tableContentHeight + _spacing.Height * (_rowHeight.Count - 1)) + t.Height;

      return _size;
    }

    private Cell GetCell(GUIControl control)
    {
      return (control.LayoutDetail == null) ? new Cell() : (Cell)control.LayoutDetail;
    }

    #endregion Methods

    #region Properties

    public double Width
    {
      get { return _width; }
      set
      {
        if (value != _width)
        {
          _width = value;
        }
      }
    }

    public int Columns
    {
      get { return _cols; }
      set
      {
        if (value != _cols)
        {
          _cols = value;
        }
      }
    }

    public int Rows
    {
      get { return _rowHeight.Count; }
    }

    public Size Size
    {
      get { return _size; }
    }

    public Size Spacing
    {
      get { return _spacing; }
      set
      {
        if (Equals(_spacing, value) == false)
        {
          _spacing = value;
        }
      }
    }

    #endregion Properties
  }
}