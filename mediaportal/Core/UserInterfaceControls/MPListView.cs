#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Runtime.InteropServices;

namespace MediaPortal.UserInterface.Controls
{
  /// <summary>
  /// Summary description for ListView.
  /// </summary>
  public class MPListView : System.Windows.Forms.ListView
  {
    [DllImport("user32")]
    private static extern int GetDoubleClickTime();

    private const string REORDER = "Reorder";
    private bool allowRowReorder = false;
    private DateTime lastClick = DateTime.MinValue;

    public bool AllowRowReorder
    {
      get { return this.allowRowReorder; }
      set
      {
        this.allowRowReorder = value;
        base.AllowDrop = value;
      }
    }

    public new SortOrder Sorting
    {
      get { return SortOrder.None; }
      set { base.Sorting = SortOrder.None; }
    }

    public MPListView()
      : base()
    {
      this.AllowRowReorder = true;
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
      if (!this.AllowRowReorder)
      {
        return;
      }
      base.DoDragDrop(REORDER, DragDropEffects.Move);
      base.OnItemDrag(e);
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
      if (!this.AllowRowReorder)
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
      if (text.CompareTo(REORDER) == 0)
      {
        e.Effect = DragDropEffects.Move;
      }
      else
      {
        e.Effect = DragDropEffects.None;
      }
      base.OnDragEnter(e);
    }

    protected override void OnDragOver(DragEventArgs e)
    {
      if (!this.AllowRowReorder)
      {
        e.Effect = DragDropEffects.None;
        return;
      }
      if (!e.Data.GetDataPresent(DataFormats.Text))
      {
        e.Effect = DragDropEffects.None;
        return;
      }
      Point cp = base.PointToClient(new Point(e.X, e.Y));
      ListViewItem hoverItem = base.GetItemAt(cp.X, cp.Y);
      if (hoverItem == null)
      {
        e.Effect = DragDropEffects.None;
        return;
      }
      foreach (ListViewItem moveItem in base.SelectedItems)
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
      if (!this.AllowRowReorder)
      {
        return;
      }
      if (base.SelectedItems.Count == 0)
      {
        return;
      }
      Point cp = base.PointToClient(new Point(e.X, e.Y));
      ListViewItem dragToItem = base.GetItemAt(cp.X, cp.Y);
      if (dragToItem == null)
      {
        return;
      }
      BeginUpdate();
      int dropIndex = dragToItem.Index;
      if (dropIndex > base.SelectedItems[0].Index)
      {
        dropIndex++;
      }
      ArrayList insertItems =
        new ArrayList(base.SelectedItems.Count);
      foreach (ListViewItem item in base.SelectedItems)
      {
        insertItems.Add(item.Clone());
      }
      for (int i = insertItems.Count - 1; i >= 0; i--)
      {
        ListViewItem insertItem =
          (ListViewItem)insertItems[i];
        base.Items.Insert(dropIndex, insertItem);
      }
      foreach (ListViewItem removeItem in base.SelectedItems)
      {
        base.Items.Remove(removeItem);
      }
      base.OnDragDrop(e);
      EndUpdate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
      if (SelectedItems.Count == 0)
      {
        if (((TimeSpan)(DateTime.Now - lastClick)).TotalMilliseconds < GetDoubleClickTime())
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
  }
}