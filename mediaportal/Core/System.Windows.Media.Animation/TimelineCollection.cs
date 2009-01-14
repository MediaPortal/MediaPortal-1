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

namespace System.Windows.Media.Animation
{
  public sealed class TimelineCollection : CollectionBase
  {
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
      get { return (Timeline) List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties
  }
}