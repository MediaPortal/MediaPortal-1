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

using System.Collections;

namespace System.Windows
{
  public sealed class VisualCollection : CollectionBase
  {
    #region Methods

    public void Add(Visual visual)
    {
      if (visual == null)
      {
        throw new ArgumentNullException("visual");
      }

      List.Add(visual);
    }

    public bool Contains(Visual visual)
    {
      if (visual == null)
      {
        throw new ArgumentNullException("visual");
      }

      return List.Contains(visual);
    }

    public void CopyTo(Visual[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException("array");
      }

      List.CopyTo(array, arrayIndex);
    }

    public int IndexOf(Visual visual)
    {
      if (visual == null)
      {
        throw new ArgumentNullException("visual");
      }

      return List.IndexOf(visual);
    }

    public void Insert(int index, Visual visual)
    {
      if (visual == null)
      {
        throw new ArgumentNullException("visual");
      }

      List.Insert(index, visual);
    }

    public bool Remove(Visual visual)
    {
      if (visual == null)
      {
        throw new ArgumentNullException("visual");
      }

      if (List.Contains(visual) == false)
      {
        return false;
      }

      List.Remove(visual);

      return true;
    }

    #endregion Methods

    #region Properties

    public Visual this[int index]
    {
      get { return (Visual) List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties
  }
}