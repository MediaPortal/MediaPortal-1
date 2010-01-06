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
using System.ComponentModel;

namespace MediaPortal.Drawing
{
  [TypeConverter(typeof (PointCollectionConverter))]
  public sealed class PointCollection : CollectionBase
  {
    #region Methods

    public void Add(Point point)
    {
      List.Add(point);
    }

    public bool Contains(Point point)
    {
      return List.Contains(point);
    }

    public void CopyTo(Point[] array, int arrayIndex)
    {
      List.CopyTo(array, arrayIndex);
    }

    public int IndexOf(Point point)
    {
      return List.IndexOf(point);
    }

    public void Insert(int index, Point point)
    {
      List.Insert(index, point);
    }

    public bool Remove(Point point)
    {
      if (List.Contains(point) == false)
      {
        return false;
      }

      List.Remove(point);

      return true;
    }

    #endregion Methods

    #region Properties

    public Point this[int index]
    {
      get { return (Point)List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties
  }
}