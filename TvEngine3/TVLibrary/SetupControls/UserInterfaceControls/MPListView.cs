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
    private static extern int GetDoubleClickTime();

    private const string REORDER = "Reorder";
    private bool allowRowReorder;
    private bool isChannelListView;
    private DateTime lastClick = DateTime.MinValue;
    private ListViewItem lastItem = null;

    public bool AllowRowReorder
    {
      get { return allowRowReorder; }
      set
      {
        allowRowReorder = value;
        base.AllowDrop = value;
      }
    }

    public bool IsChannelListView
    {
      get { return isChannelListView; }
      set { isChannelListView = value; }
    }

    public new SortOrder Sorting
    {
      get { return SortOrder.None; }
      set { base.Sorting = SortOrder.None; }
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

      //ItemDragEventArgs args = null;

      if (!isChannelListView)
      {
        DoDragDrop(REORDER, DragDropEffects.Move);
        //args = new ItemDragEventArgs(e.Button, e.Item);
      }
      else
      {
        //we pass over the whole listview to be able to easily go through all selected items
        //when the user drops the channel to another group
        DoDragDrop(this, DragDropEffects.Move | DragDropEffects.Copy);
        //args = new ItemDragEventArgs(e.Button, this);
      }

      base.OnItemDrag(e);
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
      if (!AllowRowReorder)
      {
        e.Effect = DragDropEffects.None;
        return;
      }

      //if (!e.Data.GetDataPresent(DataFormats.Text))
      //{
      //  e.Effect = DragDropEffects.None;
      //  return;
      //}

      //String text = (String)e.Data.GetData(REORDER.GetType());
      //e.Effect = text.CompareTo(REORDER) == 0 ? DragDropEffects.Move : DragDropEffects.None;

      if (!isChannelListView)
      {
        e.Effect = (e.Data.GetData(REORDER.GetType()) as string) == REORDER
                     ? DragDropEffects.Move
                     : DragDropEffects.None;
      }
      else
      {
        MPListView lv = e.Data.GetData(typeof (MPListView)) as MPListView;
        if (lv != null)
        {
          if (lv == this)
          {
            e.Effect = DragDropEffects.Move;
          }
          else
          {
            e.Effect = DragDropEffects.Copy;
          }
        }
        else
        {
          e.Effect = DragDropEffects.None;
        }
      }

      base.OnDragEnter(e);
    }

    protected override void OnDragLeave(EventArgs e)
    {
      this.Invalidate();

      base.OnDragLeave(e);
    }

    protected override void OnDragOver(DragEventArgs e)
    {
      if (!AllowRowReorder)
      {
        e.Effect = DragDropEffects.None;
        return;
      }

      bool isGroupMapping = false;
      MPListView sourceListView = null;

      if (!isChannelListView)
      {
        if ((e.Data.GetData(REORDER.GetType()) as string) != REORDER)
        {
          e.Effect = DragDropEffects.None;
          return;
        }
      }
      else
      {
        //channel group assignment is going on
        sourceListView = e.Data.GetData(typeof (MPListView)) as MPListView;
        if (sourceListView == null)
        {
          e.Effect = DragDropEffects.None;
          return;
        }

        isGroupMapping = (sourceListView != this);
      }

      ListViewItem hoverItem = null;

      if (this.Items.Count > 0)
      {
        Point cp = PointToClient(new Point(e.X, e.Y));

        hoverItem = GetItemAt(cp.X, cp.Y);
        if (hoverItem == null)
        {
          if (!isGroupMapping)
          {
            e.Effect = DragDropEffects.None;
            this.Invalidate();
            return;
          }
        }

        if (hoverItem != null)
        {
          foreach (ListViewItem moveItem in SelectedItems)
          {
            if (moveItem.Index == hoverItem.Index)
            {
              e.Effect = DragDropEffects.None;
              hoverItem.EnsureVisible();
              this.Invalidate();
              return;
            }
          }

          if (hoverItem != lastItem)
          {
            this.Invalidate();
          }

          lastItem = hoverItem;

          Color lineColor = SystemColors.Highlight;

          if (this.View == View.Details || this.View == View.List)
          {
            using (Graphics g = this.CreateGraphics())
            {
              int totalColWidth = 0;

              for (int i = 0; i < this.Columns.Count; i++)
              {
                totalColWidth += this.Columns[i].Width;
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
        } //if (hoverItem != null)
        else
        {
          this.Invalidate();
        }
      }
      else
      {
        this.Invalidate();
      }

      base.OnDragOver(e);

      if (!isChannelListView)
      {
        e.Effect = (e.Data.GetData(REORDER.GetType()) as string) == REORDER
                     ? DragDropEffects.Move
                     : DragDropEffects.None;
      }
      else
      {
        if (sourceListView != null)
        {
          if (sourceListView == this)
          {
            e.Effect = DragDropEffects.Move;
          }
          else
          {
            e.Effect = DragDropEffects.Copy;
          }
        }
        else
        {
          e.Effect = DragDropEffects.None;
        }
      }

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

      MPListView sourceListView = e.Data.GetData(typeof (MPListView)) as MPListView;
      bool isGroupMapping = (sourceListView != this);

      if (SelectedItems.Count == 0 && !isChannelListView && !isGroupMapping)
      {
        return;
      }

      int dropIndex = this.Items.Count;

      Point cp = PointToClient(new Point(e.X, e.Y));
      ListViewItem dragToItem = GetItemAt(cp.X, cp.Y);

      //when dropping a channel to an empty channel group there can't be an item anywhere
      if (dragToItem == null && !isGroupMapping)
      {
        return;
      }
      else if (dragToItem != null)
      {
        dropIndex = dragToItem.Index;

        //there is no space above the first item for drawing the the new line, which shows the user, where the item will be dropped
        dropIndex++;
      }

      BeginUpdate();

      MPListView targetListView = null;

      if (!isGroupMapping)
      {
        //must be reorder then
        targetListView = this;
      }
      else
      {
        //is group mapping so try to get the selected items from source listview
        targetListView = sourceListView;
      }

      ArrayList insertItems = new ArrayList(targetListView.SelectedItems.Count);

      foreach (ListViewItem item in targetListView.SelectedItems)
      {
        insertItems.Add(item.Clone());
      }

      for (int i = insertItems.Count - 1; i >= 0; i--)
      {
        ListViewItem insertItem = (ListViewItem)insertItems[i];

        //delete old items first
        for (int j = Items.Count - 1; j >= 0; j--)
        {
          int idChannelThis = 0;
          int idChannelTarget = 0;

          try
          {
            //we can have Channel and ChannelMap here, thats why we use late-binding here to be able to compare them without need for referencing the classes
            idChannelThis =
              (int)
              Items[j].Tag.GetType().InvokeMember("IdChannel", System.Reflection.BindingFlags.GetProperty, null,
                                                  Items[j].Tag, null);
            idChannelTarget =
              (int)
              insertItem.Tag.GetType().InvokeMember("IdChannel", System.Reflection.BindingFlags.GetProperty, null,
                                                    insertItem.Tag, null);
          }
          catch
          {
            continue;
          }

          if (idChannelThis == idChannelTarget)
          {
            Items.RemoveAt(j);

            if (j < dropIndex)
            {
              dropIndex--;
            }
          }
        }

        Items.Insert(dropIndex, insertItem);
      }

      if (!isGroupMapping)
      {
        //removing the source items is only needed when doing reordering...
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