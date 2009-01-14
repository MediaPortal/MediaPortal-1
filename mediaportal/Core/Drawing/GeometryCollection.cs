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

namespace MediaPortal.Drawing
{
  public sealed class GeometryCollection : CollectionBase
  {
    #region Methods

    public void Add(Geometry geometry)
    {
      if (geometry == null)
      {
        throw new ArgumentNullException("geometry");
      }

      List.Add(geometry);
    }

    public bool Contains(Geometry geometry)
    {
      if (geometry == null)
      {
        throw new ArgumentNullException("geometry");
      }

      return List.Contains(geometry);
    }

    public void CopyTo(Geometry[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException("array");
      }

      List.CopyTo(array, arrayIndex);
    }

    public int IndexOf(Geometry geometry)
    {
      if (geometry == null)
      {
        throw new ArgumentNullException("geometry");
      }

      return List.IndexOf(geometry);
    }

    public void Insert(int index, Geometry geometry)
    {
      if (geometry == null)
      {
        throw new ArgumentNullException("geometry");
      }

      List.Insert(index, geometry);
    }

    public bool Remove(Geometry geometry)
    {
      if (geometry == null)
      {
        throw new ArgumentNullException("geometry");
      }

      if (List.Contains(geometry) == false)
      {
        return false;
      }

      List.Remove(geometry);

      return true;
    }

    #endregion Methods

    #region Properties

    public Geometry this[int index]
    {
      get { return (Geometry) List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties
  }
}