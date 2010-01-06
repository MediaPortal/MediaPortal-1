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

namespace MediaPortal.Drawing
{
  public sealed class DoubleCollection : CollectionBase
  {
    #region Methods

    public void Add(double value)
    {
      List.Add(value);
    }

    public bool Contains(double value)
    {
      return List.Contains(value);
    }

    public void CopyTo(double[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException("array");
      }

      List.CopyTo(array, arrayIndex);
    }

    public int IndexOf(double value)
    {
      return List.IndexOf(value);
    }

    public void Insert(int index, Double value)
    {
      List.Insert(index, value);
    }

    public bool Remove(double value)
    {
      if (List.Contains(value) == false)
      {
        return false;
      }

      List.Remove(value);

      return true;
    }

    #endregion Methods

    #region Properties

    public Double this[int index]
    {
      get { return (double)List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties
  }
}