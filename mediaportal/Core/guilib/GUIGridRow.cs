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

namespace MediaPortal.GUI.Library
{
  public class GUIGridRow
  {
    #region variables

    private List<GUIGridCell> _cells = new List<GUIGridCell>();
    private int _absoluteHeight = 0;
    private int _relativeHeight = 0;
    private int _calculatedHeight = 0;

    private int _positionY;
    private GUIGridControl _gridControl;

    #endregion

    #region ctor

    public GUIGridRow(GUIGridControl gridControl)
    {
      _gridControl = gridControl;
    }

    #endregion

    #region properties

    public int CalculatedHeight
    {
      get { return _calculatedHeight; }
      set { _calculatedHeight = value; }
    }

    public GUIGridControl GridControl
    {
      get { return _gridControl; }
      set { _gridControl = value; }
    }

    public int AbsoluteHeight
    {
      get { return _absoluteHeight; }
      set
      {
        _absoluteHeight = value;
        _relativeHeight = 0;
      }
    }

    public int RelativeHeight
    {
      get { return _relativeHeight; }
      set
      {
        if (_relativeHeight < 0 || _relativeHeight > 100)
        {
          throw new ArgumentOutOfRangeException("_relativeHeight");
        }
        _relativeHeight = value;
        _absoluteHeight = 0;
      }
    }


    public int RenderHeight
    {
      get
      {
        if (AbsoluteHeight > 0)
        {
          return AbsoluteHeight;
        }
        if (RelativeHeight > 0)
        {
          float height = ((float)GridControl.Height) / ((float)(GridControl.Count));
          height *= (((float)RelativeHeight) / 100.0f);
          return (int)height;
        }
        return _calculatedHeight;
      }
    }

    public List<GUIGridCell> Columns
    {
      get { return _cells; }
      set { _cells = value; }
    }

    public int Count
    {
      get { return _cells.Count; }
    }

    #endregion

    #region public members

    public void FreeResources()
    {
      for (int column = 0; column < _cells.Count; ++column)
      {
        _cells[column].FreeResources();
      }
    }

    public void Render(int offsetX, ref int offsetY, float timePassed)
    {
      _positionY = offsetY;
      for (int column = 0; column < _cells.Count; ++column)
      {
        _cells[column].Render(ref offsetX, offsetY, timePassed);
      }
      offsetY += RenderHeight;
    }

    public GUIGridRow GetRowAt(int y)
    {
      for (int row = 0; row < GridControl.Rows.Count; row++)
      {
        if (y >= GridControl.Rows[row]._positionY &&
            y < GridControl.Rows[row]._positionY + GridControl.Rows[row].RenderHeight)
        {
          return GridControl.Rows[row];
        }
      }
      return null;
    }

    #endregion

    #region private members

    #endregion
  }
}