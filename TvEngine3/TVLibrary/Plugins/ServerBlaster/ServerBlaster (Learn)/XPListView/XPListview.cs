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
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


//*************************************************************
//
//	Thanks to Michael Dobler for the idea on using standard 
//	Windows API calls to provide listview grouping support
//
//*************************************************************

namespace XPListview
{
  public class XPListView : System.Windows.Forms.ListView
  {
    private ColumnHeader _autoGroupCol = null;
    private ArrayList _autoGroupList = new ArrayList();
    private XPListViewItemCollection _items;
    private XPListViewGroupCollection _groups;
    private bool _showInGroups = false;
    private bool _autoGroup = false;
    private string _emptyAutoGroupText = "";
    private IntPtr _apiRetVal;

    public XPListView()
    {
      _items = new XPListViewItemCollection(this);
      _groups = new XPListViewGroupCollection(this);
    }

    #region Designer Properties

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
     Description("the items collection of this view"),
     Editor(typeof (XPListViewItemCollectionEditor), typeof (System.Drawing.Design.UITypeEditor)),
     Category("Behavior")]
    public new XPListViewItemCollection Items
    {
      get { return _items; }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
     Description("collection of available groups (manually added)"),
     Editor(typeof (System.ComponentModel.Design.CollectionEditor), typeof (System.Drawing.Design.UITypeEditor)),
     Category("Grouping")]
    public XPListViewGroupCollection Groups
    {
      get { return _groups; }
    }

    [Category("Grouping"),
     Description("flag if the grouping view is active"),
     DefaultValue(false)]
    public bool ShowInGroups
    {
      get { return _showInGroups; }
      set
      {
        if (_showInGroups != value)
        {
          _showInGroups = value;
          if (_autoGroup && value == false)
          {
            _autoGroup = false;
            _autoGroupCol = null;
            _autoGroupList.Clear();
          }

          int param = 0;
          int wParam = System.Convert.ToInt32(value);
          ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_ENABLEGROUPVIEW, wParam, ref param);
        }
      }
    }

    [Category("Grouping"),
     Description("flag if the autogroup mode is active"),
     DefaultValue(false)]
    public bool AutoGroupMode
    {
      get { return _autoGroup; }
      set
      {
        _autoGroup = value;
        if (_autoGroupCol != null)
        {
          AutoGroupByColumn(_autoGroupCol.Index);
        }
      }
    }

    [Category("Grouping"),
     Description("column by with values the listiew is automatically grouped"),
     DefaultValue(typeof (ColumnHeader), ""),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public ColumnHeader AutoGroupColumn
    {
      get { return _autoGroupCol; }
      set
      {
        _autoGroupCol = value;

        if (_autoGroupCol != null)
        {
          AutoGroupByColumn(_autoGroupCol.Index);
        }
      }
    }

    [Category("Grouping"),
     Description("the text that is displayed instead of an empty auto group text"),
     DefaultValue("")]
    public string EmptyAutoGroupText
    {
      get { return _emptyAutoGroupText; }
      set
      {
        _emptyAutoGroupText = value;

        if (_autoGroupCol != null)
        {
          AutoGroupByColumn(_autoGroupCol.Index);
        }
      }
    }

    [Browsable(false),
     Description("readonly array of all automatically created groups"),
     Category("Grouping")]
    public Array Autogroups
    {
      get { return _autoGroupList.ToArray(typeof (String)); }
    }

    #endregion

    public void ShowTiles(int[] columns)
    {
      ListViewAPI.LVTILEVIEWINFO apiTileView;
      ListViewAPI.LVTILEINFO apiTile;

      IntPtr lpcol = Marshal.AllocHGlobal(columns.Length * 4);
      Marshal.Copy(columns, 0, lpcol, columns.Length);

      int param = 0;
      _apiRetVal = ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_SETVIEW, ListViewAPI.LV_VIEW_TILE, ref param);

      apiTileView = new ListViewAPI.LVTILEVIEWINFO();
      apiTileView.cbSize = Marshal.SizeOf(typeof (ListViewAPI.LVTILEVIEWINFO));
      apiTileView.dwMask = ListViewAPI.LVTVIM_COLUMNS | ListViewAPI.LVTVIM_TILESIZE;
      apiTileView.dwFlags = ListViewAPI.LVTVIF_AUTOSIZE;
      apiTileView.cLines = columns.Length;


      _apiRetVal = ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_SETTILEVIEWINFO, 0, ref apiTileView);

      foreach (XPListViewItem itm in this.Items)
      {
        apiTile = new ListViewAPI.LVTILEINFO();
        apiTile.cbSize = Marshal.SizeOf(typeof (ListViewAPI.LVTILEINFO));
        apiTile.iItem = itm.Index;
        apiTile.cColumns = columns.Length;
        apiTile.puColumns = (int)lpcol;

        _apiRetVal = ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_SETTILEINFO, 0, ref apiTile);
      }

      //columns = null;
      Marshal.FreeHGlobal(lpcol);
    }


    public void SetTileSize(Size size)
    {
      ListViewAPI.LVTILEVIEWINFO apiTileView;
      ListViewAPI.INTEROP_SIZE apiSize;

      this.SuspendLayout();

      int param = 0;
      _apiRetVal = ListViewAPI.SendMessage((System.IntPtr)this.Handle, ListViewAPI.LVM_GETVIEW, ListViewAPI.LV_VIEW_TILE,
                                           ref param);
      if ((int)_apiRetVal != ListViewAPI.LV_VIEW_TILE)
      {
        return;
      }

      apiSize = new ListViewAPI.INTEROP_SIZE();
      apiSize.cx = size.Width;
      apiSize.cy = size.Height;

      apiTileView = new ListViewAPI.LVTILEVIEWINFO();
      apiTileView.cbSize = Marshal.SizeOf(typeof (ListViewAPI.LVTILEVIEWINFO));
      apiTileView.dwMask = ListViewAPI.LVTVIM_TILESIZE | ListViewAPI.LVTVIM_LABELMARGIN;
      _apiRetVal = ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_GETTILEVIEWINFO, 0, ref apiTileView);
      apiTileView.dwFlags = ListViewAPI.LVTVIF_FIXEDSIZE;
      apiTileView.sizeTile = apiSize;
      _apiRetVal = ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_SETTILEVIEWINFO, 0, ref apiTileView);

      this.ResumeLayout();
    }


    public void SetTileWidth(int width)
    {
      ListViewAPI.LVTILEVIEWINFO apiTileView;

      this.SuspendLayout();

      int param = 0;
      _apiRetVal = ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_GETVIEW, ListViewAPI.LV_VIEW_TILE, ref param);
      if ((int)_apiRetVal != ListViewAPI.LV_VIEW_TILE)
      {
        return;
      }

      apiTileView = new ListViewAPI.LVTILEVIEWINFO();
      apiTileView.cbSize = Marshal.SizeOf(typeof (ListViewAPI.LVTILEVIEWINFO));
      apiTileView.dwMask = ListViewAPI.LVTVIM_TILESIZE | ListViewAPI.LVTVIM_LABELMARGIN;
      _apiRetVal = ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_GETTILEVIEWINFO, 0, ref apiTileView);
      apiTileView.dwFlags = ListViewAPI.LVTVIF_FIXEDWIDTH;
      apiTileView.sizeTile.cx = width;
      _apiRetVal = ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_SETTILEVIEWINFO, 0, ref apiTileView);

      this.ResumeLayout();
    }


    public void SetTileHeight(int height)
    {
      ListViewAPI.LVTILEVIEWINFO apiTileView;

      this.SuspendLayout();

      int param = 0;
      _apiRetVal = ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_GETVIEW, ListViewAPI.LV_VIEW_TILE, ref param);
      if ((int)_apiRetVal != ListViewAPI.LV_VIEW_TILE)
      {
        return;
      }


      apiTileView = new ListViewAPI.LVTILEVIEWINFO();

      apiTileView.cbSize = Marshal.SizeOf(typeof (ListViewAPI.LVTILEVIEWINFO));
      apiTileView.dwMask = ListViewAPI.LVTVIM_TILESIZE | ListViewAPI.LVTVIM_LABELMARGIN;
      _apiRetVal = ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_GETTILEVIEWINFO, 0, ref apiTileView);
      apiTileView.dwFlags = ListViewAPI.LVTVIF_FIXEDHEIGHT;
      apiTileView.sizeTile.cy = height;
      _apiRetVal = ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_SETTILEVIEWINFO, 0, ref apiTileView);


      this.ResumeLayout();
    }


    public bool AutoGroupByColumn(int columnID)
    {
      if (columnID >= this.Columns.Count || columnID < 0)
      {
        return false;
      }

      try
      {
        _autoGroupList.Clear();
        foreach (XPListViewItem itm in this.Items)
        {
          if (
            !_autoGroupList.Contains(itm.SubItems[columnID].Text == ""
                                       ? _emptyAutoGroupText
                                       : itm.SubItems[columnID].Text))
          {
            _autoGroupList.Add(itm.SubItems[columnID].Text == "" ? EmptyAutoGroupText : itm.SubItems[columnID].Text);
          }
        }

        _autoGroupList.Sort();

        ListViewAPI.ClearListViewGroup(this);
        foreach (string text in _autoGroupList)
        {
          ListViewAPI.AddListViewGroup(this, text, _autoGroupList.IndexOf(text));
        }

        foreach (XPListViewItem itm in this.Items)
        {
          ListViewAPI.AddItemToGroup(this, itm.Index,
                                     _autoGroupList.IndexOf(itm.SubItems[columnID].Text == ""
                                                              ? _emptyAutoGroupText
                                                              : itm.SubItems[columnID].Text));
        }

        int param = 0;
        ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_ENABLEGROUPVIEW, 1, ref param);
        _showInGroups = true;
        _autoGroup = true;
        _autoGroupCol = this.Columns[columnID];

        this.Refresh();

        return true;
      }
      catch (Exception ex)
      {
        throw new SystemException("Error in XPListView.AutoGroupByColumn: " + ex.Message);
      }
    }


    public bool Regroup()
    {
      try
      {
        ListViewAPI.ClearListViewGroup(this);
        foreach (XPListViewGroup grp in this.Groups)
        {
          ListViewAPI.AddListViewGroup(this, grp.GroupText, grp.GroupIndex);
        }

        foreach (XPListViewItem itm in this.Items)
        {
          ListViewAPI.AddItemToGroup(this, itm.Index, itm.GroupIndex);
        }

        int param = 0;
        ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_ENABLEGROUPVIEW, 1, ref param);
        _showInGroups = true;
        _autoGroup = false;
        _autoGroupCol = null;
        _autoGroupList.Clear();

        return true;
      }
      catch (Exception ex)
      {
        throw new SystemException("Error in XPListView.Regroup: " + ex.Message);
      }
    }


    public void RedrawItems()
    {
      ListViewAPI.RedrawItems(this, true);
      this.ArrangeIcons();
    }


    public void UpdateItems()
    {
      ListViewAPI.UpdateItems(this);
    }


    public void SetColumnStyle(int column, Font font, Color foreColor, Color backColor)
    {
      this.SuspendLayout();

      foreach (XPListViewItem itm in this.Items)
      {
        if (itm.SubItems.Count > column)
        {
          itm.SubItems[column].Font = font;
          itm.SubItems[column].BackColor = backColor;
          itm.SubItems[column].ForeColor = foreColor;
        }
      }

      this.ResumeLayout();
    }


    public void SetColumnStyle(int column, Font font, Color foreColor)
    {
      SetColumnStyle(column, font, foreColor, this.BackColor);
    }


    public void SetColumnStyle(int column, Font font)
    {
      SetColumnStyle(column, font, this.ForeColor, this.BackColor);
    }


    public void ResetColumnStyle(int column)
    {
      this.SuspendLayout();

      foreach (XPListViewItem itm in this.Items)
      {
        if (itm.SubItems.Count > column)
        {
          itm.SubItems[column].ResetStyle();
        }
      }

      this.ResumeLayout();
    }


    public void SetBackgroundImage(string ImagePath, ImagePosition Position)
    {
      ListViewAPI.SetListViewImage(this, ImagePath, Position);
    }


    private void _items_ItemAdded(object sender, ListViewItemEventArgs e)
    {
      string text;
      if (_autoGroup)
      {
        text = e.Item.SubItems[_autoGroupCol.Index].Text;
        if (!_autoGroupList.Contains(text))
        {
          _autoGroupList.Add(text);
          ListViewAPI.AddListViewGroup(this, text, _autoGroupList.IndexOf(text));
        }
        ListViewAPI.AddItemToGroup(this, e.Item.Index, _autoGroupList.IndexOf(text));
      }
    }


    protected override void OnColumnClick(System.Windows.Forms.ColumnClickEventArgs e)
    {
      base.OnColumnClick(e);
      this.SuspendLayout();
      if (_showInGroups)
      {
        int param = 0;
        ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_ENABLEGROUPVIEW, 0, ref param);
      }
      this.ListViewItemSorter = new XPListViewItemComparer(e.Column);
      if (this.Sorting == SortOrder.Descending)
      {
        this.Sorting = SortOrder.Ascending;
      }
      else
      {
        this.Sorting = SortOrder.Descending;
      }
      this.Sort();
      if (_showInGroups)
      {
        int param = 0;
        ListViewAPI.SendMessage(this.Handle, ListViewAPI.LVM_ENABLEGROUPVIEW, 1, ref param);
        if (_autoGroup == true)
        {
          AutoGroupByColumn(_autoGroupCol.Index);
        }
        else
        {
          Regroup();
        }
      }
      this.ResumeLayout();
    }

    protected override void WndProc(ref Message m)
    {
      base.WndProc(ref m);

      switch (m.Msg)
      {
        case ListViewAPI.OCM_NOTIFY:
          ListViewAPI.NMHDR lmsg = (ListViewAPI.NMHDR)m.GetLParam(typeof (ListViewAPI.NMHDR));

          switch (lmsg.code)
          {
            case (int)ListViewAPI.NM_CUSTOMDRAW:
              NotifyListCustomDraw(ref m);
              break;

            case (int)ListViewAPI.LVN_GETDISPINFOW:
              break;

            case (int)ListViewAPI.LVN_ITEMCHANGING:
              break;

            default:
              break;
          }
          break;
      }
    }

    private bool NotifyListCustomDraw(ref Message m)
    {
      m.Result = (IntPtr)ListViewAPI.CDRF_DODEFAULT;
      ListViewAPI.NMCUSTOMDRAW nmcd = (ListViewAPI.NMCUSTOMDRAW)m.GetLParam(typeof (ListViewAPI.NMCUSTOMDRAW));
      IntPtr thisHandle = Handle;

      if (nmcd.hdr.hwndFrom != Handle)
      {
        return false;
      }

      switch (nmcd.dwDrawStage)
      {
        case (int)ListViewAPI.CDDS_PREPAINT:
          m.Result = (IntPtr)ListViewAPI.CDRF_NOTIFYITEMDRAW;
          break;
        case (int)ListViewAPI.CDDS_ITEMPREPAINT:
          m.Result = (IntPtr)ListViewAPI.CDRF_NOTIFYSUBITEMDRAW;
          break;
        case (int)(ListViewAPI.CDDS_ITEMPREPAINT | ListViewAPI.CDDS_SUBITEM):
          break;
        default:
          break;
      }
      return false;
    }
  }


  /// <summary>
  /// Only basic support for sorting in this sample - would need to be updated for asc/desc support
  /// </summary>
  public class XPListViewItemComparer : IComparer
  {
    private int col;

    public XPListViewItemComparer()
    {
      col = 0;
    }

    public XPListViewItemComparer(int column)
    {
      col = column;
    }

    public int Compare(object x, object y)
    {
      return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
    }
  }
}