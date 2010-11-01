#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

using System.Collections;

namespace System.Windows.Controls
{
  public sealed class ColumnDefinitionCollection : CollectionBase
  {
    #region Constructors

    public ColumnDefinitionCollection() {}

    #endregion Constructors

    #region Methods

    public void Add(ColumnDefinition column)
    {
      if (column == null)
      {
        throw new ArgumentNullException("column");
      }

      List.Add(column);
    }

    public bool Contains(ColumnDefinition column)
    {
      if (column == null)
      {
        throw new ArgumentNullException("column");
      }

      return List.Contains(column);
    }

    public void CopyTo(ColumnDefinition[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException("array");
      }

      List.CopyTo(array, arrayIndex);
    }

    public int IndexOf(ColumnDefinition column)
    {
      if (column == null)
      {
        throw new ArgumentNullException("column");
      }

      return List.IndexOf(column);
    }

    public void Insert(int index, ColumnDefinition column)
    {
      if (column == null)
      {
        throw new ArgumentNullException("column");
      }

      List.Insert(index, column);
    }

    public bool Remove(ColumnDefinition column)
    {
      if (column == null)
      {
        throw new ArgumentNullException("column");
      }

      if (List.Contains(column) == false)
      {
        return false;
      }

      List.Remove(column);

      return true;
    }

    #endregion Methods

    #region Properties

    public ColumnDefinition this[int index]
    {
      get { return (ColumnDefinition)List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties
  }
}