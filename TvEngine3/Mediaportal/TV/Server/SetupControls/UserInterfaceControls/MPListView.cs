#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace Mediaportal.TV.Server.SetupControls.UserInterfaceControls
{
  /// <summary>
  /// Summary description for ListView.
  /// </summary>
  public class MPListView : ListView
  {
    private const string REORDER = "Reorder";
    private bool _allowRowReorder = false;
    private ListViewItem _lastItem = null;
    private bool _supressItemCheckChange = false;

    public bool AllowRowReorder
    {
      get
      {
        return _allowRowReorder;
      }
      set
      {
        _allowRowReorder = value;
        if (_allowRowReorder)
        {
          AllowDrop = value;
        }
      }
    }

    public MPListView()
    {
      // Avoid flickering:
      // 1. Activate double buffering.
      SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
      // 2. Enable catching the WM_ERASEBKGND message.
      SetStyle(ControlStyles.EnableNotifyMessage, true);
    }

    #region drag/drop row reordering support

    // Probably based on http://www.codeproject.com/Articles/4576/Drag-and-Drop-ListView-row-reordering

    private DragDropEffects GetDragDropEffect(DragEventArgs e)
    {
      if (string.Equals((e.Data.GetData(REORDER.GetType()) as string), REORDER))
      {
        return DragDropEffects.Move;
      }
      return DragDropEffects.None;
    }

    protected override void OnItemDrag(ItemDragEventArgs e)
    {
      base.OnItemDrag(e);
      if (!AllowRowReorder)
      {
        return;
      }

      // Support simple reordering of rows.
      DoDragDrop(REORDER, DragDropEffects.Move);
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
      base.OnDragEnter(e);
      if (!AllowRowReorder)
      {
        return;
      }

      e.Effect = GetDragDropEffect(e);
    }

    protected override void OnDragLeave(EventArgs e)
    {
      Invalidate();
      base.OnDragLeave(e);
    }

    protected override void OnDragOver(DragEventArgs e)
    {
      base.OnDragOver(e);
      if (!AllowRowReorder)
      {
        return;
      }

      e.Effect = GetDragDropEffect(e);
      if (e.Effect == DragDropEffects.None)
      {
        return;
      }

      DefaultDragOverHandler(e);
    }

    public void DefaultDragOverHandler(DragEventArgs e)
    {
      ListViewItem hoverItem = null;
      if (Items.Count != 0)
      {
        Point cp = PointToClient(new Point(e.X, e.Y));
        hoverItem = GetItemAt(cp.X, cp.Y);
      }
      if (AllowRowReorder && hoverItem == null)
      {
        Invalidate();
        e.Effect = DragDropEffects.None;
        return;
      }

      // Can't drop onto one of the items that is selected for dragging.
      foreach (ListViewItem moveItem in SelectedItems)
      {
        if (AllowRowReorder && moveItem.Index == hoverItem.Index)
        {
          e.Effect = DragDropEffects.None;
          hoverItem.EnsureVisible();
          Invalidate();
          return;
        }
      }

      // We draw a line across the list view to show the insert position.
      // Remove the old line.
      if (hoverItem != _lastItem)
      {
        Invalidate();
      }
      _lastItem = hoverItem;

      // Draw the new line.
      Color lineColor = SystemColors.Highlight;
      if (hoverItem != null && (View == View.Details || View == View.List))
      {
        using (Graphics g = CreateGraphics())
        {
          int totalColWidth = 0;

          for (int i = 0; i < Columns.Count; i++)
          {
            totalColWidth += Columns[i].Width;
          }

          g.DrawLine(new Pen(lineColor, 2),
                      new Point(hoverItem.Bounds.X,
                                hoverItem.Bounds.Y + hoverItem.Bounds.Height),
                      new Point(hoverItem.Bounds.X + totalColWidth,
                                hoverItem.Bounds.Y + hoverItem.Bounds.Height));

          g.FillPolygon(new SolidBrush(lineColor),
                        new Point[]
                          {
                            new Point(hoverItem.Bounds.X,
                                      hoverItem.Bounds.Y + hoverItem.Bounds.Height - 7),
                            new Point(hoverItem.Bounds.X + 6,
                                      hoverItem.Bounds.Y + hoverItem.Bounds.Height - 1),
                            new Point(hoverItem.Bounds.X,
                                      hoverItem.Bounds.Y + hoverItem.Bounds.Height + 6)
                          });

          g.FillPolygon(new SolidBrush(lineColor),
                        new Point[]
                          {
                            new Point(totalColWidth,
                                      hoverItem.Bounds.Y + hoverItem.Bounds.Height - 7),
                            new Point(totalColWidth - 7,
                                      hoverItem.Bounds.Y + hoverItem.Bounds.Height - 1),
                            new Point(totalColWidth,
                                      hoverItem.Bounds.Y + hoverItem.Bounds.Height + 7)
                          });
        }
      }
    }

    protected override void OnDragDrop(DragEventArgs e)
    {
      base.OnDragDrop(e);
      if (!AllowRowReorder)
      {
        return;
      }

      DefaultDragDropHandler(e);
    }

    public void DefaultDragDropHandler(DragEventArgs e)
    {
      if (SelectedItems.Count == 0)
      {
        return;
      }

      // Determine where we're going to insert the dragged item(s).
      Point cp = PointToClient(new Point(e.X, e.Y));
      ListViewItem dragToItem = GetItemAt(cp.X, cp.Y);
      if (dragToItem == null)
      {
        return;
      }

      // Always drop below as there is no space to draw the insert line above the first item.
      int dropIndex = dragToItem.Index;
      dropIndex++;

      BeginUpdate();
      try
      {
        // Move the items.
        foreach (ListViewItem item in SelectedItems)
        {
          int index = item.Index;
          Items.RemoveAt(index);
          if (index <= dropIndex)
          {
            Items.Insert(dropIndex - 1, item);
          }
          else
          {
            Items.Insert(dropIndex++, item);
          }
        }
      }
      finally
      {
        EndUpdate();
      }
    }

    #endregion

    #region double-click and CTRL/SHIFT + click don't check/uncheck the item

    protected override void OnItemCheck(ItemCheckEventArgs e)
    {
      if (_supressItemCheckChange || Control.ModifierKeys.HasFlag(Keys.Control) || Control.ModifierKeys.HasFlag(Keys.Shift))
      {
        e.NewValue = e.CurrentValue;
        if (_supressItemCheckChange)
        {
          _supressItemCheckChange = false;
        }
      }
      else
      {
        base.OnItemCheck(e);
      }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
      // Is this a double-click?
      if (e.Button == MouseButtons.Left && e.Clicks > 1)
      {
        _supressItemCheckChange = true;
      }
      base.OnMouseDown(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      // CTRL + A = select all
      if (e.KeyCode == Keys.A && e.Control && MultiSelect)
      {
        foreach (ListViewItem i in Items)
        {
          i.Selected = true;
        }
      }

      _supressItemCheckChange = false;
      base.OnKeyDown(e);
    }

    #endregion

    #region avoid flickering

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void OnNotifyMessage(Message m)
    {
      // filter WM_ERASEBKGND
      if (m.Msg != 0x14)
      {
        base.OnNotifyMessage(m);
      }
    }

    #endregion
  }
}