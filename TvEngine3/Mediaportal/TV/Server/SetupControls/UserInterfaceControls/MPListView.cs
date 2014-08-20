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
using System.Runtime.InteropServices;
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
    private bool _isChannelListView = false;
    private ListViewItem _lastItem = null;
    private bool _doubleClickEventPending = false;

    public bool AllowRowReorder
    {
      get
      {
        return _allowRowReorder;
      }
      set
      {
        _allowRowReorder = value;
        base.AllowDrop = value;
      }
    }

    public bool IsChannelListView
    {
      get
      {
        return _isChannelListView;
      }
      set
      {
        _isChannelListView = value;
      }
    }

    public MPListView()
    {
      // Activate double buffering.
      SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

      // Enable catching the WM_ERASEBKGND message.
      SetStyle(ControlStyles.EnableNotifyMessage, true);
    }

    #region drag/drop row reordering support

    // Probably based on http://www.codeproject.com/Articles/4576/Drag-and-Drop-ListView-row-reordering

    private DragDropEffects GetDragDropEffect(DragEventArgs e, out MPListView listView)
    {
      listView = null;
      if (!_isChannelListView)
      {
        if (string.Equals((e.Data.GetData(REORDER.GetType()) as string), REORDER))
        {
          return DragDropEffects.Move;
        }
      }
      else
      {
        listView = e.Data.GetData(typeof(MPListView)) as MPListView;
        if (listView != null)
        {
          if (listView == this)
          {
            return DragDropEffects.Move;
          }
          return DragDropEffects.Copy;
        }
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

      if (!_isChannelListView)
      {
        // Support simple reordering of rows.
        DoDragDrop(REORDER, DragDropEffects.Move);
      }
      else
      {
        // Support copying items from one list view to another as well as reordering.
        DoDragDrop(this, DragDropEffects.Move | DragDropEffects.Copy);
      }
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
      base.OnDragEnter(e);
      if (!AllowRowReorder)
      {
        e.Effect = DragDropEffects.None;
        return;
      }

      MPListView lv;
      e.Effect = GetDragDropEffect(e, out lv);
    }

    protected override void OnDragLeave(EventArgs e)
    {
      Invalidate();
      base.OnDragLeave(e);
    }

    protected override void OnDragOver(DragEventArgs e)
    {
      if (!AllowRowReorder)
      {
        e.Effect = DragDropEffects.None;
        return;
      }

      MPListView sourceListView = null;
      e.Effect = GetDragDropEffect(e, out sourceListView);
      if (e.Effect == DragDropEffects.None)
      {
        return;
      }
      bool isGroupMapping = (sourceListView != null && sourceListView != this);

      ListViewItem hoverItem = null;
      if (Items.Count == 0)
      {
        Invalidate();
      }
      else
      {
        Point cp = PointToClient(new Point(e.X, e.Y));
        hoverItem = GetItemAt(cp.X, cp.Y);
        if (hoverItem == null)
        {
          // Dragging should be done over list view items. The only exception
          // is when dragging from one list view to another and the target
          // list view is empty.
          Invalidate();
          if (!isGroupMapping)
          {
            e.Effect = DragDropEffects.None;
            return;
          }
        }
        else
        {
          // Can't drop onto one of the items that is selected for dragging.
          foreach (ListViewItem moveItem in SelectedItems)
          {
            if (moveItem.Index == hoverItem.Index)
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
          if (View == View.Details || View == View.List)
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
      }

      base.OnDragOver(e);

      e.Effect = GetDragDropEffect(e, out sourceListView);

      if (hoverItem != null)
      {
        hoverItem.EnsureVisible();
      }
    }

    protected override void OnDragDrop(DragEventArgs e)
    {
      if (!AllowRowReorder)
      {
        return;
      }

      MPListView sourceListView = e.Data.GetData(typeof(MPListView)) as MPListView;
      bool isGroupMapping = (sourceListView != null && sourceListView != this);
      if (SelectedItems.Count == 0 && !_isChannelListView && !isGroupMapping)
      {
        return;
      }

      // Determine where we're going to insert the dragged item(s).
      int dropIndex = Items.Count;
      Point cp = PointToClient(new Point(e.X, e.Y));
      ListViewItem dragToItem = GetItemAt(cp.X, cp.Y);
      if (dragToItem != null)
      {
        // Always drop below as there is no space to draw the insert line above the first item.
        dropIndex = dragToItem.Index;
        dropIndex++;
      }

      BeginUpdate();
      try
      {
        MPListView targetListView = null;
        if (!isGroupMapping)
        {
          // Reorder.
          targetListView = this;
        }
        else
        {
          // Move/copy.
          targetListView = sourceListView;
        }

        // Insert copies of the selected items in the drop position.
        ArrayList insertItems = new ArrayList(targetListView.SelectedItems.Count);
        foreach (ListViewItem item in targetListView.SelectedItems)
        {
          insertItems.Add(item.Clone());
        }
        for (int i = insertItems.Count - 1; i >= 0; i--)
        {
          ListViewItem insertItem = (ListViewItem)insertItems[i];
          Items.Insert(dropIndex, insertItem);
        }

        // Remove the original items if reordering.
        if (!isGroupMapping)
        {
          foreach (ListViewItem removeItem in SelectedItems)
          {
            Items.Remove(removeItem);
          }
        }

        base.OnDragDrop(e);

        if (!isGroupMapping)
        {
          base.OnItemDrag(new ItemDragEventArgs(MouseButtons.Left, insertItems[0]));
        }
        else
        {
          base.OnItemDrag(new ItemDragEventArgs(MouseButtons.Left, sourceListView));
        }
      }
      finally
      {
        EndUpdate();
      }
    }

    #endregion

    #region double-click doesn't check the item

    protected override void OnItemCheck(ItemCheckEventArgs e)
    {
      if (_doubleClickEventPending)
      {
        e.NewValue = e.CurrentValue;
        _doubleClickEventPending = false;
      }
      else
      {
        base.OnItemCheck(e);
      }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
      // Is this a double-click?
      if ((e.Button == MouseButtons.Left) && (e.Clicks > 1))
      {
        _doubleClickEventPending = true;
      }
      base.OnMouseDown(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      _doubleClickEventPending = false;
      base.OnKeyDown(e);
    }

    #endregion

    protected override void OnPaintBackground(PaintEventArgs pevent) {}

    protected override void OnNotifyMessage(Message m)
    {
      // filter WM_ERASEBKGND
      if (m.Msg != 0x14)
      {
        base.OnNotifyMessage(m);
      }
    }
  }
}