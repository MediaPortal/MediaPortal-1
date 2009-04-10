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

namespace MediaPortal.GUI.Library
{
  public sealed class GUIControlCollection : CollectionBase
  {
    #region Methods

    public void Add(GUIControl control)
    {
      if (control == null)
      {
        throw new ArgumentNullException("control");
      }

      List.Add(control);
    }

    public bool Contains(GUIControl control)
    {
      if (control == null)
      {
        throw new ArgumentNullException("control");
      }

      return List.Contains(control);
    }

    public void CopyTo(GUIControl[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException("array");
      }

      List.CopyTo(array, arrayIndex);
    }

    public int IndexOf(GUIControl control)
    {
      if (control == null)
      {
        throw new ArgumentNullException("control");
      }

      return List.IndexOf(control);
    }

    public void Insert(int index, GUIControl control)
    {
      if (control == null)
      {
        throw new ArgumentNullException("control");
      }

      List.Insert(index, control);
    }

    public bool Remove(GUIControl control)
    {
      if (control == null)
      {
        throw new ArgumentNullException("control");
      }

      if (List.Contains(control) == false)
      {
        return false;
      }

      List.Remove(control);

      return true;
    }

    #endregion Methods

    #region Properties

    public GUIControl this[int index]
    {
      get { return (GUIControl) List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties
  }
}