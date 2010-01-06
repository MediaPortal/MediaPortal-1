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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace MediaPortal.UserInterface.Controls
{
  /// <summary>
  /// Summary description for MPTabControl.
  /// </summary>
  public class MPTabControl : System.Windows.Forms.TabControl
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    private int dragMoveCounter = 0;
    private bool allowReorderTabs = false;
    private int lastHoverTabIndex = -1;

    public MPTabControl()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();
      AllowDrop = true;

      // TODO: Add any initialization after the InitForm call
    }

    public bool AllowReorderTabs
    {
      get { return allowReorderTabs; }
      set
      {
        allowReorderTabs = value;
        AllowDrop = true;
      }
    }

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {}

    #endregion

    protected override void OnDragOver(System.Windows.Forms.DragEventArgs e)
    {
      base.OnDragOver(e);

      Point pt = new Point(e.X, e.Y);
      //We need client coordinates.
      pt = PointToClient(pt);

      //Get the tab we are hovering over.
      TabPage hoverTab = GetTabPageByTab(pt);
      if (hoverTab == null)
      {
        if (lastHoverTabIndex != -1)
        {
          lastHoverTabIndex = -1;
          e.Effect = DragDropEffects.None;
          this.Refresh();
        }
        return;
      }

      int hoverTabIndex = this.TabPages.IndexOf(hoverTab);
      if (hoverTabIndex == this.SelectedIndex || hoverTabIndex == 0)
      {
        if (hoverTabIndex != lastHoverTabIndex)
        {
          lastHoverTabIndex = hoverTabIndex;
          e.Effect = DragDropEffects.None;
          this.Refresh();
        }

        return;
      }

      //Make sure we are on a tab.
      if (hoverTab != null)
      {
        //Make sure there is a TabPage being dragged.
        if (e.Data.GetDataPresent(typeof (TabPage)))
        {
          e.Effect = DragDropEffects.Move;

          if (hoverTabIndex != lastHoverTabIndex)
          {
            this.Refresh();

            lastHoverTabIndex = hoverTabIndex;

            int targetIndex = hoverTabIndex;
            if (targetIndex < this.SelectedIndex)
              targetIndex--;

            Color lineColor = SystemColors.Highlight;

            using (Graphics g = this.CreateGraphics())
            {
              Rectangle tabRect = GetTabRect(targetIndex);

              int x = tabRect.X + tabRect.Width - 1;
              if (hoverTabIndex == this.TabCount - 1)
              {
                //for whatever reason the x + width is not accurate on last tab
                x = x - 1;
              }

              g.DrawLine(new Pen(lineColor, 2),
                         new Point(x + 1, tabRect.Y),
                         new Point(x + 1, tabRect.Y + tabRect.Height));

              g.FillPolygon(new SolidBrush(lineColor),
                            new Point[]
                              {
                                new Point(x - 6,
                                          tabRect.Y),
                                new Point(x + 7,
                                          tabRect.Y),
                                new Point(x,
                                          tabRect.Y + 7)
                              });
            }
          }
        }
      }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);

      if (!this.AllowReorderTabs)
      {
        return;
      }

      if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
      {
        dragMoveCounter = 0;
        return;
      }

      Point pt = new Point(e.X, e.Y);
      TabPage tp = GetTabPageByTab(pt);

      if (tp == null || tp != this.SelectedTab || this.TabPages[0] == tp)
      {
        dragMoveCounter = 0;
        return;
      }

      dragMoveCounter++;

      if (dragMoveCounter >= 3)
      {
        dragMoveCounter = 0;
        DoDragDrop(tp, DragDropEffects.All);
      }
    }

    protected override void OnDragLeave(EventArgs e)
    {
      base.OnDragLeave(e);

      lastHoverTabIndex = -1;

      this.Refresh();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
      base.OnMouseUp(e);
    }

    /// <summary>
    /// Finds the TabPage whose tab is contains the given point.
    /// </summary>
    /// <param name="pt">The point (given in client coordinates) to look for a TabPage.</param>
    /// <returns>The TabPage whose tab is at the given point (null if there isn't one).</returns>
    private TabPage GetTabPageByTab(Point pt)
    {
      TabPage tp = null;

      for (int i = 0; i < TabPages.Count; i++)
      {
        if (GetTabRect(i).Contains(pt))
        {
          tp = TabPages[i];
          break;
        }
      }

      return tp;
    }

    /// <summary>
    /// Loops over all the TabPages to find the index of the given TabPage.
    /// </summary>
    /// <param name="page">The TabPage we want the index for.</param>
    /// <returns>The index of the given TabPage(-1 if it isn't found.)</returns>
    private int FindIndex(TabPage page)
    {
      for (int i = 0; i < TabPages.Count; i++)
      {
        if (TabPages[i] == page)
          return i;
      }

      return -1;
    }
  }
}