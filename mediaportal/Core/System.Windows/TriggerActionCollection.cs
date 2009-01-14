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
  public sealed class TriggerActionCollection : CollectionBase, IAddChild
  {
    #region Constructors

    public TriggerActionCollection()
    {
    }

    #endregion Constructors

    #region Methods

    public void Add(TriggerAction action)
    {
      if (action == null)
      {
        throw new ArgumentNullException("action");
      }

      List.Add(action);
    }

    public bool Contains(TriggerAction action)
    {
      if (action == null)
      {
        throw new ArgumentNullException("action");
      }

      return List.Contains(action);
    }

    public void CopyTo(TriggerAction[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException("array");
      }

      List.CopyTo(array, arrayIndex);
    }

    public TriggerActionCollection GetCurrentValue()
    {
      throw new NotImplementedException();
    }

    void IAddChild.AddChild(object child)
    {
      if (child == null)
      {
        throw new ArgumentNullException("child");
      }

      if (child is TriggerAction == false)
      {
        throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", child.GetType(), typeof (TriggerAction)));
      }

      List.Add((TriggerAction) child);
    }

    void IAddChild.AddText(string text)
    {
    }

    public int IndexOf(TriggerAction action)
    {
      if (action == null)
      {
        throw new ArgumentNullException("action");
      }

      return List.IndexOf(action);
    }

    public void Insert(int index, TriggerAction action)
    {
      if (action == null)
      {
        throw new ArgumentNullException("action");
      }

      List.Insert(index, action);
    }

    public bool Remove(TriggerAction action)
    {
      if (action == null)
      {
        throw new ArgumentNullException("action");
      }

      if (List.Contains(action) == false)
      {
        return false;
      }

      List.Remove(action);

      return true;
    }

    #endregion Methods

    #region Properties

    public TriggerAction this[int index]
    {
      get { return (TriggerAction) List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties
  }
}