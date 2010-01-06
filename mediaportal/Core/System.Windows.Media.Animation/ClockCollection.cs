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

namespace System.Windows.Media.Animation
{
  public sealed class ClockCollection : CollectionBase
  {
    #region Constructors

    public ClockCollection() {}

    public ClockCollection(Clock parent)
    {
      _parent = parent;
    }

    #endregion Constructors

    #region Methods

    public void Add(Clock clock)
    {
      if (clock == null)
      {
        throw new ArgumentNullException("clock");
      }

      List.Add(clock);
    }

    public bool Contains(Clock clock)
    {
      if (clock == null)
      {
        throw new ArgumentNullException("clock");
      }

      return List.Contains(clock);
    }

    public void CopyTo(Clock[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException("array");
      }

      List.CopyTo(array, arrayIndex);
    }

    public override bool Equals(object other)
    {
      throw new NotImplementedException();
    }

    public static bool Equals(ClockCollection l, ClockCollection r)
    {
      throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
      throw new NotImplementedException();
    }

    public int IndexOf(Clock clock)
    {
      if (clock == null)
      {
        throw new ArgumentNullException("clock");
      }

      return List.IndexOf(clock);
    }

    public void Insert(int index, Clock clock)
    {
      if (clock == null)
      {
        throw new ArgumentNullException("clock");
      }

      List.Insert(index, clock);
    }

    public bool Remove(Clock clock)
    {
      if (clock == null)
      {
        throw new ArgumentNullException("clock");
      }

      if (List.Contains(clock) == false)
      {
        return false;
      }

      List.Remove(clock);

      return true;
    }

    #endregion Methods

    #region Operators

//		public static bool operator == (ClockCollection l, ClockCollection r)
//		{
//			throw new NotImplementedException();
//		}

//		public static bool operator != (ClockCollection l, ClockCollection r)
//		{
//			throw new NotImplementedException();
//		}

    #endregion Operators

    #region Properties

    public Clock this[int index]
    {
      get { return (Clock)List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties

    #region Fields

    private Clock _parent;

    #endregion Fields
  }
}