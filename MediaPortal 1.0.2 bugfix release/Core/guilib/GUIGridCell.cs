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

namespace MediaPortal.GUI.Library
{
  public class GUIGridCell
  {
    #region variables

    private GUIControl _control;
    private int _absoluteWidth = 0;
    private int _relativeWidth = 0;
    private int _calculatedWidth = 0;
    private int _positionX = 0;
    private GUIGridRow _row;
    private bool _hasFocus = false;
    private bool _resourcesAllocated = false;

    #endregion

    #region ctor

    public GUIGridCell(GUIGridRow row)
    {
      _row = row;
    }

    #endregion

    #region properties

    public GUIGridRow Row
    {
      get { return _row; }
      set { _row = value; }
    }

    public int CalculatedWidth
    {
      get { return _calculatedWidth; }
      set { _calculatedWidth = value; }
    }

    public int AbsoluteWidth
    {
      get { return _absoluteWidth; }
      set
      {
        _relativeWidth = 0;
        _absoluteWidth = value;
      }
    }

    public int RelativeWidth
    {
      get { return _relativeWidth; }
      set
      {
        if (_relativeWidth < 0 || _relativeWidth > 100)
        {
          throw new ArgumentOutOfRangeException("_relativeWidth");
        }
        _relativeWidth = value;
        _absoluteWidth = 0;
      }
    }

    public GUIControl Control
    {
      get { return _control; }
      set { _control = value; }
    }

    public int RenderWidth
    {
      get
      {
        if (AbsoluteWidth > 0)
        {
          return AbsoluteWidth;
        }
        if (RelativeWidth > 0)
        {
          float width = (float) Row.GridControl.TotalWidth;
          width *= (((float) RelativeWidth)/100.0f);
          return (int) width;
        }
        return _calculatedWidth;
      }
    }

    public bool Focus
    {
      get { return _hasFocus; }
      set
      {
        _hasFocus = value;
        if (_control != null)
        {
          _control.Focus = value;
        }
      }
    }

    #endregion

    #region public methods

    public void FreeResources()
    {
      if (_resourcesAllocated)
      {
        _control.FreeResources();
        _resourcesAllocated = false;
      }
    }

    public void Render(ref int offsetX, int offsetY, float timePassed)
    {
      _positionX = offsetX;
      if (_control == null)
      {
        return;
      }
      _control.Width = RenderWidth;
      _control.Height = Row.RenderHeight;

      _control.SetPosition(offsetX, offsetY);

      if (offsetY + Row.RenderHeight > 0 && offsetY < GUIGraphicsContext.Height)
      {
        if (offsetX + RenderWidth >= 0 && offsetX < GUIGraphicsContext.Width)
        {
          if (_resourcesAllocated == false)
          {
            _resourcesAllocated = true;
            _control.AllocResources();
          }
          _control.Render(timePassed);
        }
        else if (_resourcesAllocated)
        {
          _control.FreeResources();
          _resourcesAllocated = false;
        }
      }
      else if (_resourcesAllocated)
      {
        _control.FreeResources();
        _resourcesAllocated = false;
      }
      offsetX += RenderWidth;
    }


    public GUIGridCell GetColumnAt(GUIGridRow row, int x)
    {
      for (int column = 0; column < row.Count; column++)
      {
        GUIGridCell cell = row.Columns[column];
        if (x >= cell._positionX && x < cell._positionX + cell.RenderWidth)
        {
          return cell;
        }
      }
      return null;
    }

    public GUIGridCell OnUp(int x)
    {
      GUIGridRow row = Row.GetRowAt(_control.YPosition - 1);
      if (row == null)
      {
        return null;
      }
      return GetColumnAt(row, x);
    }

    public GUIGridCell OnDown(int x)
    {
      GUIGridRow row = Row.GetRowAt(_control.YPosition + Row.RenderHeight + 1);
      if (row == null)
      {
        return null;
      }
      return GetColumnAt(row, x);
    }

    public GUIGridCell OnLeft()
    {
      int index = ColumnIndex;
      index--;
      if (index < 0)
      {
        return null;
      }
      return Row.Columns[index];
    }

    public GUIGridCell OnRight()
    {
      int index = ColumnIndex;
      index++;
      if (index >= Row.Count)
      {
        return null;
      }
      return Row.Columns[index];
    }

    #endregion

    #region private members

    private int ColumnIndex
    {
      get
      {
        for (int column = 0; column < Row.Count; column++)
        {
          if (this == Row.Columns[column])
          {
            return column;
          }
        }
        return -1;
      }
    }

    #endregion
  }
}