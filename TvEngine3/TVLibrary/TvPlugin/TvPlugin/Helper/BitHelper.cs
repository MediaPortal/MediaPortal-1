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
using System.Collections.Generic;
using System.Text;

namespace TvPlugin
{
  /// <summary>
  /// Helper class for bitwise operations on flags
  /// </summary>
  /// <typeparam name="T">enum [FLAG] to handle</typeparam>
  public class BitHelper<T>
  {
    private Int32 _value = 0;
    /// <summary>
    /// Returns values
    /// </summary>
    public Int32 Value { get { return _value; } }

    /// <summary>
    /// ctor
    /// </summary>
    public BitHelper()
    {
      Clear();
    }

    /// <summary>
    /// reset to zero
    /// </summary>
    public void Clear()
    {
      _value = 0;
    }

    /// <summary>
    /// Sets one or more flags
    /// </summary>
    /// <param name="val">flag or list of flags( | )</param>
    public void Set(T val)
    {
      _value |= Convert.ToInt32(val);
    }

    /// <summary>
    /// Resets one or more flags
    /// </summary>
    /// <param name="val"></param>
    public void Reset(T val)
    {
      _value &= ~Convert.ToInt32(val);
    }

    /// <summary>
    /// Checks if at least one flag is set
    /// </summary>
    /// <param name="val">flag or flag list to test</param>
    /// <returns>true if one or more are set</returns>
    public bool IsSet(T val)
    {
      return ((_value & Convert.ToInt32(val)) != 0);
    }

    /// <summary>
    /// Checks if at least one flag is set
    /// </summary>
    /// <param name="val">flag or flag list to test</param>
    /// <returns>true if one or more are set</returns>
    public bool IsNotSet(T val)
    {
      return !IsSet(val);
    }
    /// <summary>
    /// Checks if all flags are set
    /// </summary>
    /// <param name="val">flag or flag list to test</param>
    /// <returns>true if all are set</returns>
    public bool AllSet(T val)
    {
      return ((_value & Convert.ToInt32(val)) == Convert.ToInt32(val));
    }

    public override string ToString()
    {
      return _value.ToString();
    }
  }
}
