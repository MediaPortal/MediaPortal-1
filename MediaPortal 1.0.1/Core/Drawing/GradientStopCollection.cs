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
  public sealed class GradientStopCollection : CollectionBase
  {
    #region Methods

    public void Add(GradientStop stop)
    {
      if (stop == null)
      {
        throw new ArgumentNullException("stop");
      }

      List.Add(stop);
    }

    public bool Contains(GradientStop stop)
    {
      if (stop == null)
      {
        throw new ArgumentNullException("stop");
      }

      return List.Contains(stop);
    }

    public void CopyTo(GradientStop[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException("array");
      }

      List.CopyTo(array, arrayIndex);
    }

    public int IndexOf(GradientStop stop)
    {
      if (stop == null)
      {
        throw new ArgumentNullException("stop");
      }

      return List.IndexOf(stop);
    }

    public void Insert(int index, GradientStop stop)
    {
      if (stop == null)
      {
        throw new ArgumentNullException("stop");
      }

      List.Insert(index, stop);
    }

    public bool Remove(GradientStop stop)
    {
      if (stop == null)
      {
        throw new ArgumentNullException("stop");
      }

      if (List.Contains(stop) == false)
      {
        return false;
      }

      List.Remove(stop);

      return true;
    }

    #endregion Methods

    #region Properties

    public GradientStop this[int index]
    {
      get { return (GradientStop) List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties
  }
}