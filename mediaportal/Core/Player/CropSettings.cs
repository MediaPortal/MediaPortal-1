#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

#region usings

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace MediaPortal.Player
{
  /// <summary>
  /// Class which holds crop settings for the PlaneScene
  /// </summary>
  public class CropSettings
  {

    #region vars

    private int _top;
    private int _bottom;
    private int _left;
    private int _right;

    #endregion

    #region Ctor

    public CropSettings() : this(0, 0, 0, 0) { }
    public CropSettings(int top, int bottom, int left, int right)
    {
      _top = top;
      _bottom = bottom;
      _left = left;
      _right = right;
    }

    #endregion

    #region properties

    /// <summary>
    /// Number of scanlines to remove at the top of the picture
    /// </summary>
    public int Top
    {
      get { return _top; }
      set { _top = value; }
    }

    /// <summary>
    /// Number of scanlines to remove at the bottom of the picture
    /// </summary>
    public int Bottom
    {
      get { return _bottom; }
      set { _bottom = value; }
    }

    /// <summary>
    /// Number of columns to remove from the left side of the picture
    /// </summary>
    public int Left
    {
      get { return _left; }
      set { _left = value; }
    }

    /// <summary>
    /// Number of columns to remove from the right side of the picture
    /// </summary>
    public int Right
    {
      get { return _right; }
      set { _right = value; }
    }

    #endregion
  }
}
