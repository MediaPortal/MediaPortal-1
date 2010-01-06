using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace XPListview
{
  public enum ImagePosition
  {
    TopLeft,
    TopCenter,
    TopRight,
    CenterLeft,
    CenterRight,
    AbsoluteMiddle,
    BottomLeft,
    BottomCenter,
    BottomRight,
  }


  public class ListViewAPI
  {
    #region Constants for Listview-Messages

    public const int LVM_FIRST = 4096;
    public const int HDM_SETIMAGELIST = 4616;
    public const int LVM_ARRANGE = (LVM_FIRST + 22);
    public const int LVM_DELETEALLITEMS = (LVM_FIRST + 9);
    public const int LVM_DELETECOLUMN = (LVM_FIRST + 28);
    public const int LVM_DELETEITEM = (LVM_FIRST + 8);
    public const int LVM_ENABLEGROUPVIEW = (LVM_FIRST + 157);
    public const int LVM_GETCOLUMN = (LVM_FIRST + 25);
    public const int LVM_GETCOLUMNW = (LVM_FIRST + 95);
    public const int LVM_GETGROUPINFO = (LVM_FIRST + 149);
    public const int LVM_GETGROUPMETRICS = (LVM_FIRST + 156);
    public const int LVM_GETHEADER = (LVM_FIRST + 31);
    public const int LVM_GETITEM = (LVM_FIRST + 5);
    public const int LVM_GETTILEINFO = (LVM_FIRST + 165);
    public const int LVM_GETTILEVIEWINFO = (LVM_FIRST + 163);
    public const int LVM_GETTOOLTIPS = (LVM_FIRST + 78);
    public const int LVM_GETVIEW = (LVM_FIRST + 143);
    public const int LVM_HASGROUP = (LVM_FIRST + 161);
    public const int LVM_INSERTCOLUMN = (LVM_FIRST + 27);
    public const int LVM_INSERTGROUP = (LVM_FIRST + 145);
    public const int LVM_INSERTGROUPSORTED = (LVM_FIRST + 159);
    public const int LVM_INSERTITEM = (LVM_FIRST + 7);
    public const int LVM_ISGROUPVIEWENABLED = (LVM_FIRST + 175);
    public const int LVM_MOVEGROUP = (LVM_FIRST + 151);
    public const int LVM_MOVEITEMTOGROUP = (LVM_FIRST + 154);
    public const int LVM_REDRAWITEMS = (LVM_FIRST + 21);
    public const int LVM_REMOVEALLGROUPS = (LVM_FIRST + 160);
    public const int LVM_REMOVEGROUP = (LVM_FIRST + 150);
    public const int LVM_SETCOLUMN = (LVM_FIRST + 26);
    public const int LVM_SETEXTENDEDLISTVIEWSTYLE = (LVM_FIRST + 54);
    public const int LVM_SETGROUPINFO = (LVM_FIRST + 147);
    public const int LVM_SETGROUPMETRICS = (LVM_FIRST + 155);
    public const int LVM_SETINFOTIP = (LVM_FIRST + 173);
    public const int LVM_SETITEM = (LVM_FIRST + 6);
    public const int LVM_SETTILEINFO = (LVM_FIRST + 164);
    public const int LVM_SETTILEVIEWINFO = (LVM_FIRST + 162);
    public const int LVM_SETTILEWIDTH = (LVM_FIRST + 141);
    public const int LVM_SETTOOLTIPS = (LVM_FIRST + 74);
    public const int LVM_SETVIEW = (LVM_FIRST + 142);
    public const int LVM_SORTGROUPS = (LVM_FIRST + 158);
    public const int LVM_SORTITEMS = (LVM_FIRST + 48);
    public const int LVM_UPDATE = (LVM_FIRST + 42);
    public const int LVBKIF_STYLE_NORMAL = 0;
    public const int LVBKIF_SOURCE_URL = 2;
    public const int LVBKIF_STYLE_TILE = 16;
    public const int LVBKIF_FLAG_TILEOFFSET = 0x00000100;
    public const int LVM_SETBKIMAGE = (LVM_FIRST + 68);
    public const int LVM_SETTEXTBKCOLOR = (LVM_FIRST + 38);
    public const int CLR_NONE = -1;

    public const int WM_NOTIFY = 0x004E;
    public const int WM_USER = 0x0400;
    public const int OCM__BASE = (WM_USER + 0x1c00);
    public const int OCM_NOTIFY = (OCM__BASE + WM_NOTIFY);
    public const int NM_FIRST = (0 - 0);
    public const int NM_CUSTOMDRAW = (NM_FIRST - 12);

    public const int LVN_FIRST = (0 - 100);
    public const int LVN_ITEMCHANGING = (LVN_FIRST - 0);
    public const int LVN_ITEMCHANGED = (LVN_FIRST - 1);
    public const int LVN_GETDISPINFOW = (LVN_FIRST - 77);
    public const int LVN_SETDISPINFOA = (LVN_FIRST - 51);
    public const int CDRF_DODEFAULT = 0x00000000;
    public const int CDDS_PREPAINT = 0x00000001;
    public const int CDDS_ITEM = 0x00010000;
    public const int CDDS_SUBITEM = 0x00020000;
    public const int DDS_ITEMPREPAINT = (CDDS_ITEM | CDDS_PREPAINT);
    public const int CDRF_NOTIFYITEMDRAW = 0x00000020;
    public const int CDDS_ITEMPREPAINT = (CDDS_ITEM | CDDS_PREPAINT);
    public const int CDRF_NOTIFYSUBITEMDRAW = 0x00000020;

    #endregion

    #region Constants for LVCOLUMN.mask

    public const int LVCF_FMT = 1;
    public const int LVCF_IMAGE = 16;
    public const int LVCF_ORDER = 23;
    public const int LVCF_SUBITEM = 8;
    public const int LVCF_TEXT = 4;
    public const int LVCF_WIDTH = 2;

    #endregion

    #region Constants for LVCOLUMN.fmt

    public const int LVCFMT_BITMAP_ON_RIGHT = 4096;
    public const int LVCFMT_CENTER = 2;
    public const int LVCFMT_COL_HAS_IMAGES = 32768;
    public const int LVCFMT_IMAGE = 2048;
    public const int LVCFMT_JUSTIFYMASK = 3;
    public const int LVCFMT_LEFT = 0;
    public const int LVCFMT_RIGHT = 1;

    #endregion

    #region Constants for LVGROUP.mask

    public const int LVGF_ALIGN = 8;
    public const int LVGF_FOOTER = 2;
    public const int LVGF_GROUPID = 16;
    public const int LVGF_HEADER = 1;
    public const int LVGF_NONE = 0;
    public const int LVGF_STATE = 4;

    #endregion

    #region Constants for LVGROUP.uAlign

    public const int LVGA_FOOTER_CENTER = 16;
    public const int LVGA_FOOTER_LEFT = 8;
    public const int LVGA_FOOTER_RIGHT = 23;
    public const int LVGA_HEADER_CENTER = 2;
    public const int LVGA_HEADER_LEFT = 1;
    public const int LVGA_HEADER_RIGHT = 4;

    #endregion

    #region Constants for LVGROUP.state

    public const int LVGS_COLLAPSED = 1;
    public const int LVGS_HIDDEN = 2;
    public const int LVGS_NORMAL = 0;

    #endregion

    #region Constants for LVTILEVIEWINFO.dwMask

    public const int LVTVIM_COLUMNS = 2;
    public const int LVTVIM_TILESIZE = 1;
    public const int LVTVIM_LABELMARGIN = 4;

    #endregion

    #region Constants for LVTILEVIEWINFO.dwFlags

    public const int LVTVIF_AUTOSIZE = 0;
    public const int LVTVIF_FIXEDHEIGHT = 2;
    public const int LVTVIF_FIXEDSIZE = 3;
    public const int LVTVIF_FIXEDWIDTH = 1;

    #endregion

    #region Constants for LVM_SETVIEW Message

    public const int LV_VIEW_DETAILS = 1;
    public const int LV_VIEW_ICON = 0;
    public const int LV_VIEW_LIST = 3;
    public const int LV_VIEW_MAX = 4;
    public const int LV_VIEW_SMALLICON = 2;
    public const int LV_VIEW_TILE = 4;

    #endregion

    #region Constants for LVITEM.mask

    public const int LVIF_COLUMNS = 512;
    public const int LVIF_DI_SETITEM = 4096;
    public const int LVIF_GROUPID = 256;
    public const int LVIF_IMAGE = 2;
    public const int LVIF_INDENT = 16;
    public const int LVIF_NORECOMPUTE = 2048;
    public const int LVIF_PARAM = 4;
    public const int LVIF_STATE = 8;
    public const int LVIF_TEXT = 1;

    #endregion

    #region Structs for Interop

    public struct INTEROP_SIZE
    {
      public int cx;
      public int cy;
    }

    public struct INTEROP_RECT
    {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;
    }

    #endregion

    #region Structs for ListView API

    [StructLayout(LayoutKind.Sequential)]
    public struct LVITEM
    {
      public int mask;
      public int iItem;
      public int iSubItem;
      public int state;
      public int stateMask;
      [MarshalAs(UnmanagedType.LPTStr)] public string pszText;
      public int cchTextMax;
      public int iImage;
      public int lParam;
      public int iIndent;
      public int iGroupId;
      public int cColumns;
      public int puColumns;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LVCOLUMN
    {
      public int mask;
      public int fmt;
      public int cx;
      [MarshalAs(UnmanagedType.LPTStr)] public string pszText;
      public int cchTextMax;
      public int iSubItem;
      public int iImage;
      public int iOrder;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LVGROUP
    {
      public int cbSize;
      public int mask;
      [MarshalAs(UnmanagedType.LPWStr)] public string pszHeader;
      public int cchHeader;
      [MarshalAs(UnmanagedType.LPWStr)] public string pszFooter;
      public int cchFooter;
      public int iGroupId;
      public int stateMask;
      public int state;
      public int uAlign;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LVTILEVIEWINFO
    {
      public int cbSize;
      public int dwMask;
      public int dwFlags;
      public INTEROP_SIZE sizeTile;
      public int cLines;
      public INTEROP_SIZE rcLabelMargin;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LVTILEINFO
    {
      public int cbSize;
      public int iItem;
      public int cColumns;
      public int puColumns;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LVBKIMAGE
    {
      public int ulFlags;
      public int hbm; // Not used according to MSDN
      public string pszImage;
      public int cchImageMax;
      public int xOffsetPercent;
      public int yOffsetPercent;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NMHDR
    {
      public IntPtr hwndFrom;
      public int idFrom;
      public int code;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NMCUSTOMDRAW
    {
      public NMHDR hdr;
      public int dwDrawStage;
      public IntPtr hdc;
      public RECT rc;
      public int dwItemSpec;
      public int uItemState;
      public IntPtr lItemlParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
      public int left;
      public int top;
      public int right;
      public int bottom;
    }

    #endregion

    #region Overloaded SendMessage Methods

    [DllImport("User32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref int lParam);

    [DllImport("User32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVCOLUMN lParam);

    [DllImport("User32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVTILEINFO lParam);

    [DllImport("User32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVTILEVIEWINFO lParam);

    [DllImport("User32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVGROUP lParam);

    [DllImport("User32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVITEM lParam);

    [DllImport("User32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVBKIMAGE lParam);

    #endregion

    #region Static Methods

    public static int AddItemToGroup(XPListView lst, int index, int groupID)
    {
      LVITEM apiItem;
      int ptrRetVal;

      try
      {
        if (lst == null)
        {
          return 0;
        }

        apiItem = new LVITEM();
        apiItem.mask = LVIF_GROUPID;
        apiItem.iItem = index;
        apiItem.iGroupId = groupID;

        ptrRetVal = (int)SendMessage(lst.Handle, ListViewAPI.LVM_SETITEM, 0, ref apiItem);

        return ptrRetVal;
      }
      catch (Exception ex)
      {
        throw new System.Exception("An exception in ListViewAPI.AddItemToGroup occured: " + ex.Message);
      }
    }


    public static int AddListViewGroup(XPListView lst, string text, int index)
    {
      LVGROUP apiGroup;
      int ptrRetVal;

      try
      {
        if (lst == null)
        {
          return -1;
        }

        apiGroup = new LVGROUP();
        apiGroup.mask = LVGF_GROUPID | LVGF_HEADER | LVGF_STATE;
        apiGroup.pszHeader = text;
        apiGroup.cchHeader = apiGroup.pszHeader.Length;
        apiGroup.iGroupId = index;
        apiGroup.stateMask = LVGS_NORMAL;
        apiGroup.state = LVGS_NORMAL;
        apiGroup.cbSize = Marshal.SizeOf(typeof (LVGROUP));

        ptrRetVal = (int)SendMessage(lst.Handle, ListViewAPI.LVM_INSERTGROUP, -1, ref apiGroup);
        return ptrRetVal;
      }
      catch (Exception ex)
      {
        throw new System.Exception("An exception in ListViewAPI.AddListViewGroup occured: " + ex.Message);
      }
    }


    public static int RemoveListViewGroup(XPListView lst, int index)
    {
      int ptrRetVal;

      try
      {
        if (lst != null)
        {
          return -1;
        }

        int param = 0;
        ptrRetVal = (int)SendMessage(lst.Handle, LVM_REMOVEGROUP, index, ref param);

        return ptrRetVal;
      }
      catch (Exception ex)
      {
        throw new System.Exception("An exception in ListViewAPI.RemoveListViewGroup occured: " + ex.Message);
      }
    }


    public static void ClearListViewGroup(XPListView lst)
    {
      int ptrRetVal;

      try
      {
        if (lst == null)
        {
          return;
        }

        int param = 0;
        ptrRetVal = (int)SendMessage(lst.Handle, LVM_REMOVEALLGROUPS, 0, ref param);
      }
      catch (Exception ex)
      {
        throw new System.Exception("An exception in ListViewAPI.ClearListViewGroup occured: " + ex.Message);
      }
    }


    public static void RedrawItems(XPListView lst, bool update)
    {
      int ptrRetVal;

      try
      {
        if (lst != null)
        {
          return;
        }

        int param = lst.Items.Count - 1;
        ptrRetVal = (int)SendMessage(lst.Handle, LVM_REDRAWITEMS, 0, ref param);

        if (update)
        {
          UpdateItems(lst);
        }

        lst.Refresh();
      }
      catch (Exception ex)
      {
        throw new System.Exception("An exception in ListViewAPI.RedrawItems occured: " + ex.Message);
      }
    }


    public static void UpdateItems(XPListView lst)
    {
      int ptrRetVal;

      try
      {
        if (lst != null)
        {
          return;
        }

        for (int i = 0; i < lst.Items.Count - 1; i++)
        {
          int param = 0;
          ptrRetVal = (int)SendMessage(lst.Handle, LVM_UPDATE, i, ref param);
        }
      }
      catch (Exception ex)
      {
        throw new System.Exception("An exception in ListViewAPI.UpdateItems occured: " + ex.Message);
      }
    }


    public static void SetListViewImage(XPListView lst, string ImagePath, ImagePosition Position)
    {
      int x = 0;
      int y = 0;

      GetImageLocation(Position, ref x, ref y);

      try
      {
        LVBKIMAGE apiItem = new LVBKIMAGE();
        apiItem.pszImage = ImagePath + Convert.ToChar(0);
        apiItem.cchImageMax = ImagePath.Length;
        apiItem.ulFlags = LVBKIF_SOURCE_URL | LVBKIF_STYLE_NORMAL;
        apiItem.xOffsetPercent = x;
        apiItem.yOffsetPercent = y;

        // Set the background colour of the ListView to 0XFFFFFFFF (-1) so it will be transparent
        int clear = CLR_NONE;
        SendMessage(lst.Handle, LVM_SETTEXTBKCOLOR, 0, ref clear);

        SendMessage(lst.Handle, LVM_SETBKIMAGE, 0, ref apiItem);
      }
      catch (Exception ex)
      {
        throw new System.Exception("An exception in ListViewAPI.SetListViewImage occured: " + ex.Message);
      }
    }

    public static void SetListViewImage(XPListView lst, string ImagePath, int XTileOffsetPercent, int YTileOffsetPercent)
    {
      try
      {
        LVBKIMAGE apiItem = new LVBKIMAGE();
        apiItem.pszImage = ImagePath + Convert.ToChar(0);
        apiItem.cchImageMax = ImagePath.Length;
        apiItem.ulFlags = LVBKIF_SOURCE_URL | LVBKIF_STYLE_TILE;
        apiItem.xOffsetPercent = XTileOffsetPercent;
        apiItem.yOffsetPercent = YTileOffsetPercent;

        // Set the background colour of the ListView to 0XFFFFFFFF (-1) so it will be transparent
        int clear = CLR_NONE;
        SendMessage(lst.Handle, LVM_SETTEXTBKCOLOR, 0, ref clear);

        SendMessage(lst.Handle, LVM_SETBKIMAGE, 0, ref apiItem);
      }
      catch (Exception ex)
      {
        throw new System.Exception("An exception in ListViewAPI.SetListViewImage occured: " + ex.Message);
      }
    }


    private static void GetImageLocation(ImagePosition Position, ref int XOffset, ref int YOffset)
    {
      switch (Position)
      {
        case ImagePosition.TopLeft:
          XOffset = YOffset = 0;
          break;
        case ImagePosition.TopCenter:
          XOffset = 50;
          YOffset = 0;
          break;
        case ImagePosition.TopRight:
          XOffset = 100;
          YOffset = 0;
          break;
        case ImagePosition.CenterLeft:
          XOffset = 0;
          YOffset = 50;
          break;
        case ImagePosition.CenterRight:
          XOffset = 100;
          YOffset = 50;
          break;
        case ImagePosition.AbsoluteMiddle:
          XOffset = YOffset = 50;
          break;
        case ImagePosition.BottomLeft:
          XOffset = 0;
          YOffset = 100;
          break;
        case ImagePosition.BottomCenter:
          XOffset = 50;
          YOffset = 100;
          break;
        case ImagePosition.BottomRight:
          XOffset = 100;
          YOffset = 100;
          break;
      }
    }

    #endregion
  }
}