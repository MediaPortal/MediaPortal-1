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
using System.Windows.Serialization;

namespace System.Windows
{
  public sealed class SetterBaseCollection : CollectionBase, IAddChild
  {
    #region Constructors

    public SetterBaseCollection()
    {
    }

    #endregion Constructors

    #region Methods

    public void Add(SetterBase setter)
    {
      if (setter == null)
      {
        throw new ArgumentNullException("setter");
      }

      List.Add(setter);
    }

    public bool Contains(SetterBase setter)
    {
      if (setter == null)
      {
        throw new ArgumentNullException("setter");
      }

      return List.Contains(setter);
    }

    public void CopyTo(Setter[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException("array");
      }

      List.CopyTo(array, arrayIndex);
    }

    public SetterBaseCollection GetCurrentValue()
    {
      throw new NotImplementedException();
    }

    void IAddChild.AddChild(object child)
    {
      if (child == null)
      {
        throw new ArgumentNullException("child");
      }

      if (child is SetterBase == false)
      {
        throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", child.GetType(), typeof (SetterBase)));
      }

      List.Add((SetterBase) child);
    }

    void IAddChild.AddText(string text)
    {
    }

    public int IndexOf(SetterBase setter)
    {
      if (setter == null)
      {
        throw new ArgumentNullException("setter");
      }

      return List.IndexOf(setter);
    }

    public void Insert(int index, SetterBase setter)
    {
      if (setter == null)
      {
        throw new ArgumentNullException("setter");
      }

      List.Insert(index, setter);
    }

    public bool Remove(SetterBase setter)
    {
      if (setter == null)
      {
        throw new ArgumentNullException("setter");
      }

      if (List.Contains(setter) == false)
      {
        return false;
      }

      List.Remove(setter);

      return true;
    }

    #endregion Methods

    #region Properties

    public SetterBase this[int index]
    {
      get { return (Setter) List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties
  }
}