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

using System.Collections;

namespace System.Windows
{
  public sealed class UIElementCollection : CollectionBase
  {
    #region Methods

    public void Add(UIElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      List.Add(element);
    }

    public bool Contains(UIElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return List.Contains(element);
    }

    public void CopyTo(UIElement[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException("array");
      }

      List.CopyTo(array, arrayIndex);
    }

    public int IndexOf(UIElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return List.IndexOf(element);
    }

    public void Insert(int index, UIElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      List.Insert(index, element);
    }

    public bool Remove(UIElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      if (List.Contains(element) == false)
      {
        return false;
      }

      List.Remove(element);

      return true;
    }

    #endregion Methods

    #region Properties

    public UIElement this[int index]
    {
      get { return (UIElement) List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties
  }
}