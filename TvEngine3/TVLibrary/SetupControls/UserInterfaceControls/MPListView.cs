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
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Runtime.InteropServices;

namespace MediaPortal.UserInterface.Controls
{
  /// <summary>
  /// Summary description for ListView.
  /// </summary>
  public class MPListView : ListView
  {
    [DllImport("user32")]
    static extern int GetDoubleClickTime();

    private const string REORDER = "Reorder";
    private bool allowRowReorder;
    private DateTime lastClick = DateTime.MinValue;

    public bool AllowRowReorder
    {
      get
      {
        return allowRowReorder;
      }
      set
      {
        allowRowReorder = value;
        base.AllowDrop = value;
      }
    }

    public new SortOrder Sorting
    {
      get
      {
        return SortOrder.None;
      }
      set
      {
        base.Sorting = SortOrder.None;
      }
    }

    public MPListView()
    {
      //  Activate double buffering
      SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

      // Allows for catching the WM_ERASEBKGND message
      SetStyle(ControlStyles.EnableNotifyMessage, true);
      AllowRowReorder = true;
    }

    //protected override void WndProc(ref System.Windows.Forms.Message m)
    //{
    //  const int WM_PAINT = 0xf ;

    //  switch(m.Msg)
    //  {
    //    case WM_PAINT:
    //      if(this.View == System.Windows.Forms.View.Details && this.Columns.Count > 0)
    //      {
    //        this.Columns[this.Columns.Count - 1].Width = -2 ;
    //      }
    //      break ;
    //  }

    //  base.WndProc (ref m);
    //}

    protected override void OnItemDrag(ItemDragEventArgs e)
    {
      if (!AllowRowReorder)
      {
        return;
      }
      DoDragDrop(REORDER, DragDropEffects.Move);
      base.OnItemDrag(e);
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
      if (!AllowRowReorder)
      {
        e.Effect = DragDropEffects.None;
        return;
      }
      if (!e.Data.GetDataPresent(DataFormats.Text))
      {
        e.Effect = DragDropEffects.None;
        return;
      }
      String text = (String)e.Data.GetData(REORDER.GetType());
      e.Effect = text.CompareTo(REORDER) == 0 ? DragDropEffects.Move : DragDropEffects.None;
      base.OnDragEnter(e);
    }

    protected override void OnDragOver(DragEventArgs e)
    {
      if (!AllowRowReorder)
      {
        e.Effect = DragDropEffects.None;
        return;
      }
      if (!e.Data.GetDataPresent(DataFormats.Text))
      {
        e.Effect = DragDropEffects.None;
        return;
      }
      Point cp = PointToClient(new Point(e.X, e.Y));
      ListViewItem hoverItem = GetItemAt(cp.X, cp.Y);
      if (hoverItem == null)
      {
        e.Effect = DragDropEffects.None;
        return;
      }
      foreach (ListViewItem moveItem in SelectedItems)
      {
        if (moveItem.Index == hoverItem.Index)
        {
          e.Effect = DragDropEffects.None;
          hoverItem.EnsureVisible();
          return;
        }
      }
      base.OnDragOver(e);
      String text = (String)e.Data.GetData(REORDER.GetType());
      if (text.CompareTo(REORDER) == 0)
      {
        e.Effect = DragDropEffects.Move;
        hoverItem.EnsureVisible();
      }
      else
      {
        e.Effect = DragDropEffects.None;
      }
    }

    protected override void OnDragDrop(DragEventArgs e)
    {
      if (!AllowRowReorder)
      {
        return;
      }
      if (SelectedItems.Count == 0)
      {
        return;
      }
      Point cp = PointToClient(new Point(e.X, e.Y));
      ListViewItem dragToItem = GetItemAt(cp.X, cp.Y);
      if (dragToItem == null)
      {
        return;
      }
      BeginUpdate();
      int dropIndex = dragToItem.Index;
      if (dropIndex > SelectedItems[0].Index)
      {
        dropIndex++;
      }
      ArrayList insertItems =
        new ArrayList(SelectedItems.Count);
      foreach (ListViewItem item in SelectedItems)
      {
        insertItems.Add(item.Clone());
      }
      for (int i = insertItems.Count - 1; i >= 0; i--)
      {
        ListViewItem insertItem =
         (ListViewItem)insertItems[i];
        Items.Insert(dropIndex, insertItem);
      }
      foreach (ListViewItem removeItem in SelectedItems)
      {
        Items.Remove(removeItem);
      }
      base.OnDragDrop(e);
      EndUpdate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
      if (SelectedItems.Count == 0)
      {
        if (((DateTime.Now - lastClick)).TotalMilliseconds < GetDoubleClickTime())
        {
          OnDoubleClick(e);
          lastClick = DateTime.MinValue;
        }
        else
          lastClick = DateTime.Now;
      }
      else
        lastClick = DateTime.MinValue;

      base.OnMouseUp(e);
    }

    protected override void OnDoubleClick(EventArgs e)
    {
      lastClick = DateTime.MinValue;
      base.OnDoubleClick(e);
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
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
  }
}
