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
using System.ComponentModel;
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
    private int _lastDragOverInsertIndex = -1;
    private bool _supressItemCheckChange = false;

    [DefaultValue(false)]
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

      if (AllowRowReorder)
      {
        // Support simple reordering of rows.
        DoDragDrop(REORDER, DragDropEffects.Move);
      }
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
      base.OnDragEnter(e);

      if (AllowRowReorder)
      {
        e.Effect = GetDragDropEffect(e);
      }
    }

    protected override void OnDragLeave(EventArgs e)
    {
      Invalidate();
      base.OnDragLeave(e);
    }

    protected override void OnDragOver(DragEventArgs e)
    {
      base.OnDragOver(e);

      if (AllowRowReorder)
      {
        e.Effect = GetDragDropEffect(e);
        if (e.Effect != DragDropEffects.None)
        {
          DefaultDragOverHandler(true, e);
        }
      }
    }

    public void DefaultDragOverHandler(bool isReorderingRows, DragEventArgs e)
    {
      // Draw a line across the list view to show the insert position.

      // Determine where the line should be drawn.
      int lineStartX = 0;
      int lineY = 0;
      int insertIndex = -1;
      if (Items.Count == 0)
      {
        // This doesn't work. The line flickers. I guess it isn't valid to draw
        // the line in the item area when there are no items.
        /*BeginUpdate();
        Items.Add(new ListViewItem());
        lineStartX = Items[0].Bounds.Left;
        lineY = Items[0].Bounds.Top;
        Items.RemoveAt(0);
        EndUpdate();*/
      }
      else
      {
        Point cp = PointToClient(new Point(e.X, e.Y));
        ListViewItem dragOverItem = GetItemAt(cp.X, cp.Y);
        if (dragOverItem == null)
        {
          // Find the closest matching item.
          foreach (ListViewItem item in Items)
          {
            if (cp.Y < item.Bounds.Top + (item.Bounds.Height / 2))
            {
              lineStartX = item.Bounds.Left;
              lineY = item.Bounds.Top;
              insertIndex = item.Index;
              break;
            }
          }
          if (lineY == 0)
          {
            lineStartX = Items[Items.Count - 1].Bounds.Left;
            lineY = Items[Items.Count - 1].Bounds.Bottom;
            insertIndex = Items.Count;
          }
        }
        else
        {
          // Should we draw the line above or below the drag-over item?
          insertIndex = dragOverItem.Index;
          if (cp.Y < dragOverItem.Bounds.Top + (dragOverItem.Bounds.Height / 2))
          {
            // (above)
            lineStartX = dragOverItem.Bounds.Left;
            lineY = dragOverItem.Bounds.Top;
          }
          else
          {
            insertIndex++;
            if (insertIndex < Items.Count)
            {
              lineStartX = Items[insertIndex].Bounds.Left;
              lineY = Items[insertIndex].Bounds.Top;
            }
            else
            {
              lineStartX = Items[Items.Count - 1].Bounds.Left;
              lineY = Items[Items.Count - 1].Bounds.Bottom;
            }
          }
        }
      }

      // If moving items within a list view, check if the drag is valid (items
      // actually need to be moved).
      if (isReorderingRows)
      {
        bool isValidDrag = insertIndex != -1 && insertIndex != SelectedItems[0].Index && (insertIndex == Items.Count || !Items[insertIndex].Selected);
        if (isValidDrag && SelectedItems[0].Index < insertIndex)
        {
          isValidDrag = false;
          for (int i = SelectedItems[0].Index + 1; i < insertIndex; i++)
          {
            if (!Items[i].Selected)
            {
              isValidDrag = true;
              break;
            }
          }
        }

        if (!isValidDrag)
        {
          Invalidate();
          e.Effect = DragDropEffects.None;
          if (insertIndex < Items.Count)
          {
            Items[insertIndex].EnsureVisible();
          }
          else
          {
            Items[Items.Count - 1].EnsureVisible();
          }
          return;
        }
      }

      // Remove the old line.
      if (insertIndex != _lastDragOverInsertIndex)
      {
        Invalidate();
      }
      _lastDragOverInsertIndex = insertIndex;

      // Draw the new line.
      Color lineColor = SystemColors.Highlight;
      if (insertIndex != -1 && (View == View.Details || View == View.List))
      {
        using (Graphics g = CreateGraphics())
        {
          int totalColWidth = 0;
          for (int i = 0; i < Columns.Count; i++)
          {
            totalColWidth += Columns[i].Width;
          }

          g.DrawLine(new Pen(lineColor, 2),
                      new Point(lineStartX, lineY),
                      new Point(lineStartX + totalColWidth, lineY));

          g.FillPolygon(new SolidBrush(lineColor),
                        new Point[]
                          {
                            new Point(lineStartX, lineY - 7),
                            new Point(lineStartX + 7, lineY - 1),
                            new Point(lineStartX, lineY + 7)
                          });

          g.FillPolygon(new SolidBrush(lineColor),
                        new Point[]
                          {
                            new Point(totalColWidth, lineY - 7),
                            new Point(totalColWidth - 7, lineY - 1),
                            new Point(totalColWidth, lineY + 7)
                          });
        }
      }
    }

    protected override void OnDragDrop(DragEventArgs e)
    {
      base.OnDragDrop(e);

      if (AllowRowReorder)
      {
        DefaultDragDropHandler(e);
      }
    }

    public void DefaultDragDropHandler(DragEventArgs e)
    {
      if (SelectedItems.Count == 0)
      {
        return;
      }

      // Determine where we're going to insert the dragged item(s).
      Point cp = PointToClient(new Point(e.X, e.Y));
      ListViewItem dropOnItem = GetItemAt(cp.X, cp.Y);
      int insertIndex = Items.Count;
      if (dropOnItem != null)
      {
        insertIndex = dropOnItem.Index;
        if (cp.Y >= dropOnItem.Bounds.Top + (dropOnItem.Bounds.Height / 2))
        {
          insertIndex++;
        }
      }
      else
      {
        foreach (ListViewItem item in Items)
        {
          if (cp.Y < item.Bounds.Top + (item.Bounds.Height / 2))
          {
            insertIndex = item.Index;
            break;
          }
        }
      }

      BeginUpdate();
      try
      {
        // Move the item(s). If moving multiple items, maintain relative
        // positions where possible. For example, if there are two items
        // between the first and second selected items, there will still be two
        // items between the first and second selected items after they've been
        // moved.
        int offset = SelectedItems[0].Index - insertIndex;
        bool isMoveUp = offset >= 0;
        if (!isMoveUp)
        {
          for (int i = SelectedItems[0].Index + 1; i < insertIndex; i++)
          {
            if (Items[i].Selected)
            {
              offset++;
            }
          }
        }
        if (offset == 0 || offset == -1)
        {
          Invalidate();
          return;
        }
        foreach (ListViewItem item in SelectedItems)
        {
          int index = item.Index;
          Items.RemoveAt(index);
          if (isMoveUp)
          {
            index = index - offset;
          }
          else
          {
            int moved = 0;
            while (moved > offset && index < Items.Count)
            {
              if (!Items[index].Selected && --moved == offset)
              {
                break;
              }
              index++;
            }
          }
          Items.Insert(index, item);
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