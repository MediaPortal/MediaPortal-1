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
  /// Summary description for Solver.
  /// </summary>
  public class Solver
  {
    private static Random random = new Random(DateTime.Now.Millisecond);
    public static int nodes = 0, rateAll = 0;
    public static bool unsolvableCell = false;

    public static bool ValidNumberPlace(Grid grid)
    {
      // Check only one solution exists
      IList solutions = Solve((Grid)grid.Clone(), 2);

      if (solutions.Count == 1)
      {
        return true;
      }

      // Either unsolvable or multiple solutions
      return false;
    }

    public static Grid Solve(Grid grid)
    {
      IList solutions = Solve((Grid)grid.Clone(), 1);
      if (solutions.Count == 0)
      {
        return null;
      }
      else
      {
        return (Grid)solutions[0];
      }
    }

    public static bool CheckMinimality(Grid grid)
    {
      Grid temp = (Grid)grid.Clone();

      for (int row = 0; row < temp.CellsInRow; row++)
      {
        for (int column = 0; column < temp.CellsInRow; column++)
        {
          if (temp.cells[row, column] != 0)
          {
            int value = temp.cells[row, column];
            temp.cells[row, column] = 0;
            if (ValidNumberPlace(temp))
            {
              return false;
            }
            temp.cells[row, column] = value;
          }
        }
      }

      return true;
    }

    public static Grid Minimize(Grid grid)
    {
      IList removeCandidates = new ArrayList();
      for (int cellNumber = 0; cellNumber < 81; cellNumber++)
      {
        int row = cellNumber / 9;
        int column = cellNumber % 9;
        if (grid.cells[row, column] != 0)
        {
          removeCandidates.Add(cellNumber);
        }
      }

      while (removeCandidates.Count > 0)
      {
        int removeIndex = random.Next(removeCandidates.Count);
        int removeCellNumber = (int)removeCandidates[removeIndex];
        int row = removeCellNumber / 9;
        int column = removeCellNumber % 9;

        int value = grid.cells[row, column];
        grid.cells[row, column] = 0;
        if (!ValidNumberPlace(grid))
        {
          grid.cells[row, column] = value;
        }
        else
        {
          //return Minimize(grid);
        }
        removeCandidates.RemoveAt(removeIndex);
      }

      return grid;
    }

    public static Grid Generate(int blocksAcross)
    {
      // First generate a random completed grid that is valid
      Grid empty = new Grid(blocksAcross);
      Grid solution = Solve(empty);
      Grid puzzle = (Grid)solution.Clone();
      return Minimize(puzzle);
      /*
            Random random = new Random(DateTime.Now.Millisecond);

            IList removeCandidates = new ArrayList();
            for (int cellNumber = 1; cellNumber <= 81; cellNumber++)
            {
              removeCandidates.Add(cellNumber);
            }

            while (removeCandidates.Count > 0)
            {
              int removeIndex = random.Next(removeCandidates.Count);
              int row = removeIndex / puzzle.CellsInRow;
              int column = removeIndex % puzzle.CellsInRow;

              if (puzzle.cells[row, column] != 0)
              {
                int value = puzzle.cells[row, column];
                puzzle.cells[row, column] = 0;
                IList solutions = Solve((Grid)puzzle.Clone(), 2);
                if (solutions.Count == 2 || solutions.Count == 0)
                {
                  puzzle.cells[row, column] = value;
                }
                else
                {
                  cellsRemoved;
                }
              }
            }

            IList solns = Solve((Grid)puzzle.Clone(), 10);
            return puzzle;
       */
    }

    /// <summary>
    /// Apply singles strategy to supplied grid as much as possible
    /// </summary>
    /// <param name="grid"></param>
    public static void Singles(Grid grid)
    {
      int row = 0;
      int column = 0;

      while (row < grid.CellsInRow)
      {
        while (column < grid.CellsInRow)
        {
          if (grid.cells[row, column] == 0)
          {
            IList candidates = grid.Possibilities(row, column);

            if (candidates.Count == 1)
            {
              grid.cells[row, column] = (int)candidates[0];

              // Restart search from top left
              row = 0;
              column = 0;
            }
            else
            {
              column++;
            }
          }
          else
          {
            column++;
          }
        }

        row++;
      }
    }

    public void HiddenSingles(Grid grid)
    {
      // First try rows
      for (int row = 0; row < grid.CellsInRow; row++)
      {
        int[] hiddenSingles = new int[grid.CellsInRow];

        for (int value = 0; value < grid.CellsInRow; value++)
        {
          hiddenSingles[value] = -1;
        }

        // Now get possibilities for all cells
        for (int column = 0; column < grid.CellsInRow; column++)
        {
          IList candidates = grid.Possibilities(row, column);
          foreach (int candidate in candidates)
          {
            if (hiddenSingles[candidate - 1] == -1)
            {
              hiddenSingles[candidate - 1] = column;
            }
            else
            {
              hiddenSingles[candidate - 1] = grid.CellsInRow;
            }
          }
        }

        for (int value = 0; value < grid.CellsInRow; value++)
        {
          if (hiddenSingles[value] >= 0 && hiddenSingles[value] < grid.CellsInRow)
          {
            // Got a hidden single
            grid.cells[row, hiddenSingles[value]] = value;
          }
        }
      }
    }

    /// <summary>
    /// Solve given sudoku grid and solutions upto maximum number specified
    /// </summary>
    /// <param name="g"></param>
    /// <returns></returns>
    public static IList Solve(Grid grid, int maxSolutions)
    {
      ArrayList solutions = new ArrayList();
      // If grid is solved then return as solution
      if (grid.CountFilledCells() == (grid.CellsInRow * grid.CellsInRow))
      {
        solutions.Add(grid.Clone());
        return solutions;
      }

      // Solve singles
      //Singles(grid);

      // Choose unsolved cell
      int leastCandidatesRow = -1;
      int leastCandidatesColumn = -1;
      IList leastCandidates = null;

      for (int row = 0; row < grid.CellsInRow; row++)
      {
        for (int column = 0; column < grid.CellsInRow; column++)
        {
          if (grid.cells[row, column] == 0)
          {
            IList candidates = grid.Possibilities(row, column);

            // If cell has no possible value then grid is not solvable so quit now
            if (candidates.Count == 0)
            {
              return solutions;
            }
            else if (leastCandidates == null || leastCandidates.Count > candidates.Count)
            {
              leastCandidatesRow = row;
              leastCandidatesColumn = column;
              leastCandidates = candidates;
            }
          }
        }
      }

      // For all candidates of unsolved cell
      if (leastCandidates != null)
      {
        while (leastCandidates.Count > 0)
        {
          // Set candidate
          int candidateIndex = random.Next(leastCandidates.Count);
          grid.cells[leastCandidatesRow, leastCandidatesColumn] = (int)leastCandidates[candidateIndex];
          leastCandidates.RemoveAt(candidateIndex);

          Grid nextLevelGrid = (Grid)grid.Clone();

          IList nextLevelSolutions = Solve(nextLevelGrid, maxSolutions);

          solutions.AddRange(nextLevelSolutions);

          // Trim number of solutions so we don't exceed maximum required
          if (solutions.Count > maxSolutions)
          {
            solutions.RemoveRange(0, solutions.Count - maxSolutions);
          }

          if (solutions.Count == maxSolutions)
          {
            return solutions;
          }
        }
      }
      return solutions;
    }

    /// <summary>
    /// rate a grid
    /// returns it as int
    /// the lower the harder
    /// </summary>
    /// <param name="grid">Grid</param>
    /// <returns></returns>
    public static int Rate(Grid gridOrg)
    {
      Grid grid = (Grid)gridOrg.Clone();
      int rate = 0;
      rateAll = 0;
      //			for(int trys = 0; trys<10;trys++)
      //			{
      //			grid = (Grid)gridOrg.Clone();
      //      		rate = 0;
      while (grid.CountFilledCells() < (grid.CellsInRow * grid.CellsInRow))
      {
        int cand = FindUniqueCandidates(grid);

        rate += cand;
        if (cand > 0)
        {
          int candidateIndex = random.Next(cand);
          int m = -1, i = 0, j = 0, candidateRow = 0, candidateCol = 0;

          for (i = 0; i < 9 && m < candidateIndex; i++)
          {
            for (j = 0; j < 9 && m < candidateIndex; j++)
            {
              if (grid.uniqueCandidates[i, j] != 0)
              {
                m++;
                if (m == candidateIndex)
                {
                  candidateRow = i;
                  candidateCol = j;
                }
              }
            }
          }
          grid.cells[candidateRow, candidateCol] = grid.uniqueCandidates[candidateRow, candidateCol];
        }
        else
        {
          //rate = rate - ((grid.BlocksAcross * grid.BlocksAcross) - grid.CountFilledCells());
          //no more unique cells -> difficult grid
          rate = grid.CountFilledCells();
          break;
        }

      }
      //				rateAll += rate;
      //				MediaPortal.GUI.Library.Log.WriteFile(MediaPortal.GUI.Library.LogType.Log, "Soduko Rate end "+trys+" notes: {0}", rate);
      //			}
      return rate;
    }

    /// <summary>
    /// Find all Unique Candidates in grid
    /// and fill out the uniqueCandidates Array in the grid
    /// </summary>
    /// <param name="grid">Grid</param>
    /// <returns>the number of Unique Candidates</returns>
    public static int FindUniqueCandidates(Grid grid)
    {
      //			Grid grid = (Grid)gridOrg.Clone();
      grid.resetUniqueCandidates();
      nodes = 0;
      int uniquesCand = 0;
      unsolvableCell = false;

      //find Unique Candidates in rows
      for (int searchRow = 0; searchRow < grid.CellsInRow; searchRow++)
      {
        for (int searchCol = 0; searchCol < grid.CellsInRow; searchCol++)
        {
          if (!grid.IsCellEmpty(searchRow, searchCol))
          {
            continue;
          }
          IList candidates = grid.Possibilities(searchRow, searchCol);
          IList candidates2 = new ArrayList(candidates);
          if (candidates.Count > 1)
          {
            for (int cand = 0; cand < candidates.Count; cand++)
            {
              if (FindCandidateInRow(grid, searchRow, searchCol, (int)candidates[cand]))
              {
                candidates2.Remove(candidates[cand]);
              }
            }
          }

          if (candidates2.Count == 1)
          {
            grid.uniqueCandidates[searchRow, searchCol] = (int)candidates2[0];
          }
        }
      }

      //find Unique Candidates in colums
      for (int searchCol = 0; searchCol < grid.CellsInRow; searchCol++)
      {
        for (int searchRow = 0; searchRow < grid.CellsInRow; searchRow++)
        {
          if (!grid.IsCellEmpty(searchRow, searchCol))
          {
            continue;
          }
          IList candidates = grid.Possibilities(searchRow, searchCol);
          IList candidates2 = new ArrayList(candidates);
          if (candidates.Count > 1)
          {
            for (int cand = 0; cand < candidates.Count; cand++)
            {
              if (FindCandidateInCol(grid, searchRow, searchCol, (int)candidates[cand]))
              {
                candidates2.Remove(candidates[cand]);
              }
            }
          }
          if (candidates2.Count == 1)
          {
            grid.uniqueCandidates[searchRow, searchCol] = (int)candidates2[0];
          }
        }

      }

      //find Unique Candidates in boxes
      for (int searchRow = 0; searchRow < grid.CellsInRow; searchRow++)
      {
        for (int searchCol = 0; searchCol < grid.CellsInRow; searchCol++)
        {
          if (!grid.IsCellEmpty(searchRow, searchCol))
          {
            continue;
          }
          IList candidates = grid.Possibilities(searchRow, searchCol);
          IList candidates2 = new ArrayList(candidates);
          if (candidates.Count > 1)
          {
            for (int cand = 0; cand < candidates.Count; cand++)
            {
              if (FindCandidateInBox(grid, searchRow, searchCol, (int)candidates[cand]))
              {
                candidates2.Remove(candidates[cand]);
              }
            }
          }
          else
          {
            if (candidates.Count == 1)
            {
              uniquesCand++;
            }
            else
            {
              unsolvableCell = true;
            }
          }
          if (candidates2.Count == 1)
          {
            grid.uniqueCandidates[searchRow, searchCol] = (int)candidates2[0];
          }
        }
      }

      for (int i = 0; i < 9; i++)
      {
        for (int j = 0; j < 9; j++)
        {
          if (grid.uniqueCandidates[i, j] != 0)
          {
            nodes++;
          }
        }
      }
      nodes = nodes + (uniquesCand / 2);
      return nodes;
    }

    public static bool FindCandidateInRow(Grid grid, int row, int col, int Num)
    {

      for (int i = 0; i < grid.CellsInRow; i++)
      {
        if (grid.IsCellEmpty(row, i) && i != col && grid.Possibilities(row, i).Contains(Num))
        {
          return true;
        }
      }
      return false;
    }

    public static bool FindCandidateInCol(Grid grid, int row, int col, int Num)
    {

      for (int i = 0; i < grid.CellsInRow; i++)
      {
        if (grid.IsCellEmpty(i, col) && i != row && grid.Possibilities(i, col).Contains(Num))
        {
          return true;
        }
      }
      return false;
    }

    public static bool FindCandidateInBox(Grid grid, int row, int col, int Num)
    {
      int firstRow = (row / 3) * 3;
      int firstCol = (col / 3) * 3;
      for (int i = 0; i < 3; i++)
      {
        for (int j = 0; j < 3; j++)
        {
          if (grid.IsCellEmpty(firstRow + i, firstCol + j) && grid.Possibilities(firstRow + i, firstCol + j).Contains(Num) && !(firstRow + i != row || firstCol + j != col))
          {
            return true;
          }
        }
      }
      return false;
    }

    public static Grid FillOutCells(Grid grid, Grid solution, int number)
    {
      for (; number > 0; number--)
      {
        int candidateIndex = random.Next(81 - grid.CountFilledCells());
        int m = -1, i = 0, j = 0;
        for (i = 0; i < 9 && m < candidateIndex; i++)
        {
          for (j = 0; j < 9 && m < candidateIndex; j++)
          {
            if (grid.cells[i, j] == 0)
            {
              m++;
              if (m == candidateIndex)
              {
                grid.cells[i, j] = solution.cells[i, j];
              }
            }
          }
        }
      }
      return grid;
    }

  }

}
