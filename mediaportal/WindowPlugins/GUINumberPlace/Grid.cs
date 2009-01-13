#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

namespace MediaPortal.GUI.NumberPlace
{
  /// <summary>
  /// Summary description for Grid.
  /// </summary>
  public class Grid : ICloneable
  {
    private int blocksAcross;

    public int BlocksAcross
    {
      get { return blocksAcross; }
    }

    private int cellsInRow;

    public int CellsInRow
    {
      get { return cellsInRow; }
    }

    public int[,] cells;

    private const int EMPTY = 0;

    public int[,] uniqueCandidates;

    public Grid()
    {
    }

    public Grid(int blocksAcross)
    {
      Resize(blocksAcross);
    }

    public object Clone()
    {
      Grid clone = new Grid(this.BlocksAcross);
      for (int row = 0; row < cellsInRow; row++)
      {
        for (int column = 0; column < cellsInRow; column++)
        {
          clone.cells[row, column] = cells[row, column];
        }
      }

      return clone;
    }

    public void Resize(int blocksAcross)
    {
      this.blocksAcross = blocksAcross;
      cellsInRow = blocksAcross*blocksAcross;
      cells = new int[cellsInRow,cellsInRow];
    }

    public bool IsCellEmpty(int row, int column)
    {
      return IsCellEmpty(cells[row, column]);
    }

    public bool IsCellEmpty(int cell)
    {
      return cell == EMPTY;
    }

    public int CountFilledCells()
    {
      int cellsFilled = 0;
      foreach (int cell in cells)
      {
        if (!IsCellEmpty(cell))
        {
          cellsFilled++;
        }
      }

      return cellsFilled;
    }

    public void Reset()
    {
      for (int row = 0; row < cellsInRow; row++)
      {
        for (int column = 0; column < cellsInRow; column++)
        {
          cells[row, column] = EMPTY;
        }
      }
    }

    public override bool Equals(object obj)
    {
      if (!(obj is Grid))
      {
        return false;
      }

      Grid grid = (Grid) obj;
      if (blocksAcross != grid.blocksAcross)
      {
        return false;
      }

      for (int row = 0; row < cellsInRow; row++)
      {
        for (int column = 0; column < cellsInRow; column++)
        {
          if (cells[row, column] != grid.cells[row, column])
          {
            return false;
          }
        }
      }

      return true;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public IList Possibilities(int row, int column)
    {
      // Calculate possibilities for a cell
      IList possibilities = new ArrayList();
      if (cells[row, column] == 0)
      {
        // Work out whats used already
        bool[] values = new bool[cellsInRow];
        for (int rowIndex = 0; rowIndex < cellsInRow; rowIndex++)
        {
          int value = cells[rowIndex, column];
          if (value > 0)
          {
            values[value - 1] = true;
          }
        }
        for (int columnIndex = 0; columnIndex < cellsInRow; columnIndex++)
        {
          int value = cells[row, columnIndex];
          if (value > 0)
          {
            values[value - 1] = true;
          }
        }

        int startRow = blocksAcross*(row/blocksAcross);
        for (int rowIndex = startRow; rowIndex < (startRow + blocksAcross); rowIndex++)
        {
          int startColumn = blocksAcross*(column/blocksAcross);
          for (int columnIndex = startColumn; columnIndex < (startColumn + blocksAcross); columnIndex++)
          {
            int value = cells[rowIndex, columnIndex];
            if (value > 0)
            {
              values[value - 1] = true;
            }
          }
        }

        for (int possibility = 0; possibility < cellsInRow; possibility++)
        {
          if (!values[possibility])
          {
            possibilities.Add(possibility + 1);
          }
        }
      }

      return possibilities;
    }

    public bool IsBlockValid(int blockRow, int blockColumn)
    {
      bool[] values = new bool[cellsInRow];

      int startRow = blocksAcross*blockRow;
      for (int row = startRow; row < (startRow + blocksAcross); row++)
      {
        int startColumn = blocksAcross*blockColumn;
        for (int column = startColumn; column < (startColumn + blocksAcross); column++)
        {
          int value = cells[row, column];
          if (value > 0)
          {
            if (values[value - 1])
            {
              // Got a duplicate
              return false;
            }
            else
            {
              values[value - 1] = true;
            }
          }
        }
      }

      return true;
    }

    public bool IsColumnValid(int column)
    {
      bool[] values = new bool[cellsInRow];

      for (int row = 0; row < cellsInRow; row++)
      {
        int value = cells[row, column];
        if (value > 0)
        {
          if (values[value - 1])
          {
            // Got a duplicate
            return false;
          }
          else
          {
            values[value - 1] = true;
          }
        }
      }

      return true;
    }

    public bool IsRowValid(int row)
    {
      bool[] values = new bool[cellsInRow];

      for (int column = 0; column < cellsInRow; column++)
      {
        int value = cells[row, column];
        if (value > 0)
        {
          if (values[value - 1])
          {
            // Got a duplicate
            return false;
          }
          else
          {
            values[value - 1] = true;
          }
        }
      }

      return true;
    }

    public bool IsValid()
    {
      for (int index = 0; index < cellsInRow; index++)
      {
        if (!IsColumnValid(index))
        {
          return false;
        }
        else if (!IsRowValid(index))
        {
          return false;
        }
        else if (!IsBlockValid(index%blocksAcross, index/blocksAcross))
        {
          return false;
        }
      }

      return true;
    }

    public override string ToString()
    {
      string formattedGrid = "";

      for (int row = 0; row < cellsInRow; row++)
      {
        for (int column = 0; column < cellsInRow; column++)
        {
          formattedGrid += cells[row, column];
          if (column < (cellsInRow - 1))
          {
            formattedGrid += ", ";
          }
        }
        formattedGrid += "\n";
      }

      return formattedGrid;
    }

    public void resetUniqueCandidates()
    {
      uniqueCandidates = new int[cellsInRow,cellsInRow];
    }
  }
}