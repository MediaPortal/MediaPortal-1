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
using System.Windows.Serialization;

namespace System.Windows.Media.Animation
{
  public sealed class StoryboardCollection : CollectionBase, IAddChild
  {
    #region Constructors

    public StoryboardCollection() {}

    #endregion Constructors

    #region Methods

    public void Add(Timeline timeline)
    {
      if (timeline == null)
      {
        throw new ArgumentNullException("timeline");
      }

      List.Add(timeline);
    }

    public bool Contains(Timeline timeline)
    {
      if (timeline == null)
      {
        throw new ArgumentNullException("timeline");
      }

      return List.Contains(timeline);
    }

    public void CopyTo(Timeline[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException("array");
      }

      List.CopyTo(array, arrayIndex);
    }

    public StoryboardCollection GetCurrentValue()
    {
      throw new NotImplementedException();
    }

    void IAddChild.AddChild(object child)
    {
      if (child == null)
      {
        throw new ArgumentNullException("child");
      }

      if (child is Timeline == false)
      {
        throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", child.GetType(), typeof (Timeline)));
      }

      List.Add((Timeline)child);
    }

    void IAddChild.AddText(string text) {}

    public int IndexOf(Timeline timeline)
    {
      if (timeline == null)
      {
        throw new ArgumentNullException("timeline");
      }

      return List.IndexOf(timeline);
    }

    public void Insert(int index, Timeline timeline)
    {
      if (timeline == null)
      {
        throw new ArgumentNullException("timeline");
      }

      List.Insert(index, timeline);
    }

    public bool Remove(Timeline timeline)
    {
      if (timeline == null)
      {
        throw new ArgumentNullException("timeline");
      }

      if (List.Contains(timeline) == false)
      {
        return false;
      }

      List.Remove(timeline);

      return true;
    }

    #endregion Methods

    #region Properties

    public Timeline this[int index]
    {
      get { return (Timeline)List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties
  }
}