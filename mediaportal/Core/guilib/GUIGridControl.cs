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

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Serialization;

using System.Windows.Controls;
using MediaPortal.Drawing;
using MediaPortal.Drawing.Layouts;

using Microsoft.DirectX.Direct3D;
namespace MediaPortal.GUI.Library
{
  public class GUIGridControl : GUIControl
  {
    #region skin elements
    [XMLSkinElement("HoriziontalScroll")]
    protected bool _horizontalScroll = true;

    [XMLSkinElement("VerticalScroll")]
    protected bool _verticalScroll = true;
    #endregion

    #region variables
    List<GUIGridRow> _rows = new List<GUIGridRow>();
    int _scrollPositionX = 0;
    int _scrollPositionY = 0;
    int _totalWidth = 0;
    int _cursorPositionX = 0;
    GUIGridCell _currentSelectedItem;
    float _scrollTimeElapsed = 0;
    #endregion
    #region events
    public delegate void GridControlEventHandler(object sender, GUIGridCell selectedItem, GUIGridCell previousSelectedItem);
    public event GridControlEventHandler OnSelectionChanged;
    #endregion

    #region ctor
    public GUIGridControl(int parentId)
      : base(parentId)
    {
    }
    #endregion

    #region properties
    public int TotalWidth
    {
      get
      {
        return _totalWidth;
      }
      set
      {
        _totalWidth = value;
      }
    }
    public bool HoriziontalScroll
    {
      get
      {
        return _horizontalScroll;
      }
      set
      {
        _horizontalScroll = value;
      }
    }

    public bool VerticalScroll
    {
      get
      {
        return _verticalScroll;
      }
      set
      {
        _verticalScroll = value;
      }
    }

    public List<GUIGridRow> Rows
    {
      get
      {
        return _rows;
      }
      set
      {
        _rows = value;
      }
    }

    public int Count
    {
      get
      {
        return _rows.Count;
      }
    }
    public GUIGridCell SelectedGridItem
    {
      get
      {
        return _currentSelectedItem;
      }
      set
      {
        if (_currentSelectedItem != value)
        {
          GUIGridCell previousSelectedItem = _currentSelectedItem;
          _currentSelectedItem = value;
          if (OnSelectionChanged != null)
          {
            OnSelectionChanged(this, _currentSelectedItem, previousSelectedItem);
          }
        }
      }
    }
    #endregion

    #region public methods
    public override void FinalizeConstruction()
    {
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
    }

    public override void AllocResources()
    {
      _totalWidth = Width;
      SelectedGridItem = null;
      _scrollPositionX = 0;
      _scrollPositionY = 0;
      LayoutRows();
      SetInitialSelectedItem();
    }

    public override void FreeResources()
    {
      for (int row = 0; row < Count; row++)
      {
        _rows[row].FreeResources();
      }
      _rows.Clear();
    }

    public override void Render(float timePassed)
    {
      if (!IsVisible)
      {
        base.Render(timePassed);
        return;
      }
      ScrollToSelectedItem(timePassed);
      int offsetX = _positionX + _scrollPositionX;
      int offsetY = _positionY + _scrollPositionY;
      Viewport newviewport, oldviewport;
      newviewport = new Viewport();
      oldviewport = GUIGraphicsContext.DX9Device.Viewport;
      newviewport.X = _positionX;
      newviewport.Y = _positionY;
      newviewport.Width = _width;
      newviewport.Height = _height;
      newviewport.MinZ = 0.0f;
      newviewport.MaxZ = 1.0f;
      GUIGraphicsContext.DX9Device.Viewport = newviewport;
      for (int row = 0; row < _rows.Count; ++row)
      {
        _rows[row].Render(offsetX, ref offsetY, timePassed);
      }

      GUIGraphicsContext.DX9Device.Viewport = oldviewport;
      base.Render(timePassed);
    }


    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PAGE_UP:
          break;

        case Action.ActionType.ACTION_PAGE_DOWN:
          break;

        case Action.ActionType.ACTION_HOME:
          OnHome();
          break;

        case Action.ActionType.ACTION_END:
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
          OnDown();
          if (SelectedGridItem == null)
            base.OnAction(action);
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          OnUp();
          if (SelectedGridItem == null)
            base.OnAction(action);
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          OnLeft();
          if (SelectedGridItem == null)
            base.OnAction(action);
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
          OnRight();
          if (SelectedGridItem == null)
            base.OnAction(action);
          break;

        case Action.ActionType.ACTION_KEY_PRESSED:
          break;

        case Action.ActionType.ACTION_MOUSE_MOVE:
          break;
        case Action.ActionType.ACTION_MOUSE_CLICK:
          break;
        default:
          break;
      }
    }
    public override bool OnMessage(GUIMessage message)
    {
      if (message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS)
      {
        if (SelectedGridItem == null)
          SetInitialSelectedItem();
        if (SelectedGridItem != null)
        {
          SelectedGridItem.Focus = true;
        }
      }

      if (message.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS)
      {
        if (SelectedGridItem != null)
        {
          SelectedGridItem.Focus = false;
        }
      }
      return base.OnMessage(message);
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      bool focus = base.HitTest(x, y, out controlID, out focused);
      if (!focus) return focus;
      if (Count == 0) return focus;
      if (SelectedGridItem != null)
        SelectedGridItem.Focus = false;

      SelectedGridItem = GetItemAt(x, y);
      if (SelectedGridItem != null)
      {
        SelectedGridItem.Focus = true;
      }
      return focus;
    }

    #endregion
    #region private members
    void SetInitialSelectedItem()
    {
      if (SelectedGridItem != null) return;
      if (Count > 0)
      {
        GUIGridRow row = _rows[0];
        if (row.Count > 0)
        {
          SelectedGridItem = row.Columns[0];
          _cursorPositionX = SelectedGridItem.Control.XPosition + (SelectedGridItem.RenderWidth / 2);
        }
      }
    }

    void OnHome()
    {
      if (SelectedGridItem != null)
        SelectedGridItem.Focus = false;
      SetInitialSelectedItem();
      SelectedGridItem.Focus = true;
      _scrollPositionX = 0;
      _scrollPositionY = 0;
      _cursorPositionX = SelectedGridItem.Control.XPosition + (SelectedGridItem.RenderWidth / 2);
    }
    void OnUp()
    {
      if (SelectedGridItem == null) return;
      SelectedGridItem.Focus = false;
      SelectedGridItem = SelectedGridItem.OnUp(_cursorPositionX);
      if (SelectedGridItem != null)
        SelectedGridItem.Focus = true;

    }
    void OnDown()
    {
      if (SelectedGridItem == null) return;
      SelectedGridItem.Focus = false;
      SelectedGridItem = SelectedGridItem.OnDown(_cursorPositionX);
      if (SelectedGridItem != null)
        SelectedGridItem.Focus = true;
    }
    void OnLeft()
    {
      if (SelectedGridItem == null) return;
      SelectedGridItem.Focus = false;
      SelectedGridItem = SelectedGridItem.OnLeft();
      if (SelectedGridItem != null)
        SelectedGridItem.Focus = true;
      _cursorPositionX = SelectedGridItem.Control.XPosition + (SelectedGridItem.RenderWidth / 2);
    }
    void OnRight()
    {
      if (SelectedGridItem == null) return;
      SelectedGridItem.Focus = false;
      SelectedGridItem = SelectedGridItem.OnRight();
      if (SelectedGridItem != null)
        SelectedGridItem.Focus = true;
      _cursorPositionX = SelectedGridItem.Control.XPosition + (SelectedGridItem.RenderWidth / 2);
    }

    GUIGridCell GetItemAt(int x, int y)
    {
      if (Count == 0) return null;
      GUIGridRow row = _rows[0].GetRowAt(y);
      if (row == null) return null;
      if (row.Count == 0) return null;
      return row.Columns[0].GetColumnAt(row, x);
    }

    void ScrollToSelectedItem(float timePassed)
    {
      if (SelectedGridItem == null) return;
      int x = SelectedGridItem.Control.XPosition;
      int y = SelectedGridItem.Control.YPosition;
      int width = SelectedGridItem.Control.Width;
      int height = SelectedGridItem.Control.Height;
      int xOffset = 0;
      int yOffset = 0;
      if (x > 0 && y > 0)
      {
        if (HoriziontalScroll)
        {
          if (x < XPosition)
          {
            xOffset = XPosition - x;
          }
          else if (x + width > XPosition + Width)
          {
            xOffset = (XPosition + Width) - (x + width);
          }
        }

        if (VerticalScroll)
        {
          if (y < YPosition)
          {
            yOffset = (YPosition - y);
          }
          else if (y + height > YPosition + Height)
          {
            yOffset = (YPosition + Height) - (y + height);
          }
        }
      }
      if (xOffset == 0 && yOffset == 0)
      {
        _scrollTimeElapsed = 0;
        return;
      }
      // 1 frame=20 msec;
      // scroll duration=400msec=20 frames
      float timeLeft = 0.4f - _scrollTimeElapsed;
      int framesLeft = (int)(timeLeft / 0.02f);
      if (framesLeft<=0)
      {
        _scrollPositionX += xOffset;
        _scrollPositionY += yOffset;
        return;
      }
      _scrollPositionX += (int)(((float)xOffset) / ((float)framesLeft));
      _scrollPositionY += (int)(((float)yOffset) / ((float)framesLeft));
      _scrollTimeElapsed += timePassed;

    }

    public void LayoutRows()
    {
      if (Count == 0) return;
      int height = 0;
      int rowsLeft = 0;
      for (int row = 0; row < Count; row++)
      {
        if (Rows[row].AbsoluteHeight > 0)
        {
          height += Rows[row].AbsoluteHeight;
        }
        else if (Rows[row].RelativeHeight > 0)
        {
          height += Rows[row].RenderHeight;
        }
        else
        {
          rowsLeft++;
        }
      }
      if (rowsLeft > 0)
      {
        int heightLeft = Height - height;
        heightLeft /= rowsLeft;
        if (heightLeft > 0)
        {
          for (int row = 0; row < Count; row++)
          {
            if (Rows[row].AbsoluteHeight <= 0 && Rows[row].RelativeHeight <= 0)
            {
              Rows[row].CalculatedHeight = heightLeft;
            }
          }
        }
      }

      //once to determine total width of each row
      for (int row = 0; row < Count; row++)
      {
        int rowWidth;
        LayoutColumns(Rows[row], out rowWidth);
        if (rowWidth > TotalWidth) TotalWidth = rowWidth;
      }
      //second time to set the width correctly
      for (int row = 0; row < Count; row++)
      {
        int rowWidth;
        LayoutColumns(Rows[row], out rowWidth);
      }
    }

    void LayoutColumns(GUIGridRow Row, out int rowWidth)
    {
      rowWidth = TotalWidth;
      if (Row.Count == 0) return;
      int width = 0;
      int columnsLeft = 0;
      for (int column = 0; column < Row.Count; column++)
      {
        Row.Columns[column].CalculatedWidth = 0;
        if (Row.Columns[column].AbsoluteWidth > 0)
        {
          width += Row.Columns[column].AbsoluteWidth;
        }
        else if (Row.Columns[column].RelativeWidth > 0)
        {
          width += Row.Columns[column].RenderWidth;
        }
        else
        {
          columnsLeft++;
        }
      }
      rowWidth = width;
      if (columnsLeft == 0) return;
      int widthLeft = TotalWidth - width;
      widthLeft /= columnsLeft;
      if (widthLeft > 0)
      {
        rowWidth = TotalWidth;
        for (int column = 0; column < Row.Count; column++)
        {
          if (Row.Columns[column].AbsoluteWidth <= 0 && Row.Columns[column].RelativeWidth <= 0)
          {
            Row.Columns[column].CalculatedWidth = widthLeft;
          }
        }
      }
    }
    #endregion
  }
}
