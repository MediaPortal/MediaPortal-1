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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaPortal.Util
{

  #region ThumbnailExtractor

  /// Summary description for ThumbnailExtractor.
  public class ThumbnailExtractor : IDisposable
  {
    #region ShellFolder Enumerations

    [Flags]
    private enum ESTRRET : int
    {
      STRRET_WSTR = 0x0000, // Use STRRET.pOleStr
      STRRET_OFFSET = 0x0001, // Use STRRET.uOffset to Ansi
      STRRET_CSTR = 0x0002 // Use STRRET.cStr
    }

    [Flags]
    private enum ESHCONTF : int
    {
      SHCONTF_FOLDERS = 32,
      SHCONTF_NONFOLDERS = 64,
      SHCONTF_INCLUDEHIDDEN = 128
    }

    [Flags]
    private enum ESHGDN : int
    {
      SHGDN_NORMAL = 0,
      SHGDN_INFOLDER = 1,
      SHGDN_FORADDRESSBAR = 16384,
      SHGDN_FORPARSING = 32768
    }

    [Flags]
    private enum ESFGAO : int
    {
      SFGAO_CANCOPY = 1,
      SFGAO_CANMOVE = 2,
      SFGAO_CANLINK = 4,
      SFGAO_CANRENAME = 16,
      SFGAO_CANDELETE = 32,
      SFGAO_HASPROPSHEET = 64,
      SFGAO_DROPTARGET = 256,
      SFGAO_CAPABILITYMASK = 375,
      SFGAO_LINK = 65536,
      SFGAO_SHARE = 131072,
      SFGAO_READONLY = 262144,
      SFGAO_GHOSTED = 524288,
      SFGAO_DISPLAYATTRMASK = 983040,
      SFGAO_FILESYSANCESTOR = 268435456,
      SFGAO_FOLDER = 536870912,
      SFGAO_FILESYSTEM = 1073741824,
      SFGAO_HASSUBFOLDER = -2147483648,
      SFGAO_CONTENTSMASK = -2147483648,
      SFGAO_VALIDATE = 16777216,
      SFGAO_REMOVABLE = 33554432,
      SFGAO_COMPRESSED = 67108864
    }

    #endregion

    #region IExtractImage Enumerations

    private enum EIEIFLAG
    {
      IEIFLAG_ASYNC = 0x0001, // ask the extractor if it supports ASYNC extract (free threaded)
      IEIFLAG_CACHE = 0x0002, // returned from the extractor if it does NOT cache the thumbnail
      IEIFLAG_ASPECT = 0x0004, // passed to the extractor to beg it to render to the aspect ratio of the supplied rect
      IEIFLAG_OFFLINE = 0x0008, // if the extractor shouldn't hit the net to get any content neede for the rendering
      IEIFLAG_GLEAM = 0x0010, // does the image have a gleam ? this will be returned if it does
      IEIFLAG_SCREEN = 0x0020, // render as if for the screen (this is exlusive with IEIFLAG_ASPECT )
      IEIFLAG_ORIGSIZE = 0x0040, // render to the approx size passed, but crop if neccessary
      IEIFLAG_NOSTAMP = 0x0080, // returned from the extractor if it does NOT want an icon stamp on the thumbnail
      IEIFLAG_NOBORDER = 0x0100, // returned from the extractor if it does NOT want an a border around the thumbnail
      IEIFLAG_QUALITY = 0x0200
      // passed to the Extract method to indicate that a slower, higher quality image is desired, re-compute the thumbnail
    }

    #endregion

    #region ShellFolder Structures

    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 4, Size = 0, CharSet = CharSet.Auto)]
    private struct STRRET_CSTR
    {
      public ESTRRET uType;
      [MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 520)] public byte[] cStr;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Auto)]
    private struct STRRET_ANY
    {
      [FieldOffset(0)] public ESTRRET uType;
      [FieldOffset(4)] public IntPtr pOLEString;
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    private struct SIZE
    {
      public int cx;
      public int cy;
    }

    #endregion

    #region Com Interop for IUnknown

    [ComImport, Guid("00000000-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IUnknown
    {
      [PreserveSig]
      IntPtr QueryInterface(ref Guid riid, out IntPtr pVoid);

      [PreserveSig]
      IntPtr AddRef();

      [PreserveSig]
      IntPtr Release();
    }

    #endregion

    #region COM Interop for IMalloc

    [ComImportAttribute()]
    [GuidAttribute("00000002-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    //helpstring("IMalloc interface")
    private interface IMalloc
    {
      [PreserveSig]
      IntPtr Alloc(int cb);

      [PreserveSig]
      IntPtr Realloc(
        IntPtr pv,
        int cb);

      [PreserveSig]
      void Free(IntPtr pv);

      [PreserveSig]
      int GetSize(IntPtr pv);

      [PreserveSig]
      int DidAlloc(IntPtr pv);

      [PreserveSig]
      void HeapMinimize();
    } ;

    #endregion

    #region COM Interop for IEnumIDList

    [ComImportAttribute()]
    [GuidAttribute("000214F2-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    //helpstring("IEnumIDList interface")
    private interface IEnumIDList
    {
      [PreserveSig]
      int Next(
        int celt,
        ref IntPtr rgelt,
        out int pceltFetched);

      void Skip(
        int celt);

      void Reset();

      void Clone(
        ref IEnumIDList ppenum);
    } ;

    #endregion

    #region COM Interop for IShellFolder

    [ComImportAttribute()]
    [GuidAttribute("000214E6-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    //helpstring("IShellFolder interface")
    private interface IShellFolder
    {
      void ParseDisplayName(
        IntPtr hwndOwner,
        IntPtr pbcReserved,
        [MarshalAs(UnmanagedType.LPWStr)] string lpszDisplayName,
        out int pchEaten,
        out IntPtr ppidl,
        out int pdwAttributes
        );

      void EnumObjects(
        IntPtr hwndOwner,
        [MarshalAs(UnmanagedType.U4)] ESHCONTF grfFlags,
        ref IEnumIDList ppenumIDList
        );

      void BindToObject(
        IntPtr pidl,
        IntPtr pbcReserved,
        ref Guid riid,
        ref IShellFolder ppvOut
        );

      void BindToStorage(
        IntPtr pidl,
        IntPtr pbcReserved,
        ref Guid riid,
        IntPtr ppvObj
        );

      [PreserveSig]
      int CompareIDs(
        IntPtr lParam,
        IntPtr pidl1,
        IntPtr pidl2
        );

      void CreateViewObject(
        IntPtr hwndOwner,
        ref Guid riid,
        IntPtr ppvOut
        );

      void GetAttributesOf(
        int cidl,
        IntPtr apidl,
        [MarshalAs(UnmanagedType.U4)] ref ESFGAO rgfInOut
        );

      void GetUIObjectOf(
        IntPtr hwndOwner,
        int cidl,
        ref IntPtr apidl,
        ref Guid riid,
        out int prgfInOut,
        ref IUnknown ppvOut
        );

      void GetDisplayNameOf(
        IntPtr pidl,
        [MarshalAs(UnmanagedType.U4)] ESHGDN uFlags,
        ref STRRET_CSTR lpName
        );

      void SetNameOf(
        IntPtr hwndOwner,
        IntPtr pidl,
        [MarshalAs(UnmanagedType.LPWStr)] string lpszName,
        [MarshalAs(UnmanagedType.U4)] ESHCONTF uFlags,
        ref IntPtr ppidlOut
        );
    } ;

    #endregion

    #region COM Interop for IExtractImage

    [ComImportAttribute()]
    [GuidAttribute("BB2E617C-0920-11d1-9A0B-00C04FC2D6C1")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    //helpstring("IExtractImage"),
    private interface IExtractImage
    {
      void GetLocation(
        [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszPathBuffer,
        int cch,
        ref int pdwPriority,
        ref SIZE prgSize,
        int dwRecClrDepth,
        ref int pdwFlags
        );

      void Extract(
        out IntPtr phBmpThumbnail
        );
    }

    #endregion

    #region UnManagedMethods for IShellFolder

    private class UnManagedMethods
    {
      [DllImport("shell32", CharSet = CharSet.Auto)]
      internal static extern int SHGetMalloc(out IMalloc ppMalloc);

      [DllImport("shell32", CharSet = CharSet.Auto)]
      internal static extern int SHGetDesktopFolder(out IShellFolder ppshf);

      [DllImport("shell32", CharSet = CharSet.Auto)]
      internal static extern int SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);

      [DllImport("gdi32", CharSet = CharSet.Auto)]
      internal static extern int DeleteObject(IntPtr hObject);
    }

    #endregion

    #region Member Variables

    private IMalloc alloc = null;
    private bool disposed = false;
    private System.Drawing.Size desiredSize = new System.Drawing.Size(100, 100);
    private System.Drawing.Bitmap thumbNail = null;

    #endregion

    #region Implementation

    public System.Drawing.Bitmap ThumbNail
    {
      get { return thumbNail; }
    }

    public System.Drawing.Size DesiredSize
    {
      get { return desiredSize; }
      set { desiredSize = value; }
    }

    private IMalloc Allocator
    {
      get
      {
        if (!disposed)
        {
          if (alloc == null)
          {
            //Kaushik - fails here at times
            UnManagedMethods.SHGetMalloc(out alloc);
          }
        }
        else
        {
          System.Diagnostics.Debug.Assert(false, "Object has been disposed.");
        }
        return alloc;
      }
    }

    public System.Drawing.Bitmap GetThumbnail(string file)
    {
      if ((!File.Exists(file)) && (!Directory.Exists(file)))
      {
        throw new FileNotFoundException(
          String.Format("The file '{0}' does not exist", file),
          file);
      }

      if (thumbNail != null)
      {
        thumbNail.Dispose();
        thumbNail = null;
      }

      IShellFolder folder = null;
      try
      {
        folder = getDesktopFolder;
      }
      catch (Exception ex)
      {
        throw ex;
      }

      if (folder != null)
      {
        IntPtr pidlMain = IntPtr.Zero;
        try
        {
          int cParsed = 0;
          int pdwAttrib = 0;
          string filePath = Path.GetDirectoryName(file);
          pidlMain = IntPtr.Zero;
          folder.ParseDisplayName(
            IntPtr.Zero,
            IntPtr.Zero,
            filePath,
            out cParsed,
            out pidlMain,
            out pdwAttrib);
        }
        catch (Exception ex)
        {
          Marshal.ReleaseComObject(folder);
          throw ex;
        }

        if (pidlMain != IntPtr.Zero)
        {
          // IShellFolder:
          Guid iidShellFolder = new
            Guid("000214E6-0000-0000-C000-000000000046");
          IShellFolder item = null;

          try
          {
            folder.BindToObject(pidlMain, IntPtr.Zero, ref iidShellFolder, ref item);
          }
          catch (Exception ex)
          {
            Marshal.ReleaseComObject(folder);
            Allocator.Free(pidlMain);
            throw ex;
          }

          //if (item != null)
          //{
          //  //
          //  IEnumIDList idEnum = null;
          //  try
          //  {
          //    item.EnumObjects(
          //    IntPtr.Zero,
          //    (ESHCONTF.SHCONTF_FOLDERS |
          //    ESHCONTF.SHCONTF_NONFOLDERS),
          //    ref idEnum);
          //  }
          //  catch (Exception ex)
          //  {
          //    Marshal.ReleaseComObject(folder);
          //    Allocator.Free(pidlMain);
          //    throw ex;
          //  }

          //  if (idEnum != null)
          //  {
          //    // start reading the enum:
          //    int hRes = 0;
          //    IntPtr pidl = IntPtr.Zero;
          //    int fetched = 0;
          //    bool complete = false;
          //    while (!complete)
          //    {
          //      hRes = idEnum.Next(1, ref pidl, out fetched);
          //      if (hRes != 0)
          //      {
          //        pidl = IntPtr.Zero;
          //        complete = true;
          //      }
          //      else
          //      {
          //        if (getThumbnail(file, pidl, item))
          //        {
          //          complete = true;
          //        }
          //      }
          //      if (pidl != IntPtr.Zero)
          //      {
          //        Allocator.Free(pidl);
          //      }
          //    }

          //    Marshal.ReleaseComObject(idEnum);
          //  }


          //  Marshal.ReleaseComObject(item);
          //}

          if (item != null)
          {
            string sFileName = Path.GetFileName(file);
            IntPtr pidlFile = IntPtr.Zero;
            try
            {
              int cParsed = 0;
              int pdwAttrib = 0;
              item.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, sFileName, out cParsed, out pidlFile, out pdwAttrib);
              if (pidlFile != IntPtr.Zero)
              {
                getThumbnail(file, pidlFile, item);
                Allocator.Free(pidlFile);
              }
            }
            catch (Exception ex)
            {
              Marshal.ReleaseComObject(folder);
              Allocator.Free(pidlMain);
              throw ex;
            }
            Marshal.ReleaseComObject(item);
          }

          Allocator.Free(pidlMain);
        }

        Marshal.ReleaseComObject(folder);
      }
      return thumbNail;
    }

    private bool getThumbnail(string file, IntPtr pidl, IShellFolder item)
    {
      IntPtr hBmp = IntPtr.Zero;
      IExtractImage extractImage = null;

      try
      {
        string pidlPath = PathFromPidl(pidl);
        if (Path.GetFileName(pidlPath).ToUpper().Equals(Path.GetFileName(file).ToUpper()))
        {
          // we have the item:
          IUnknown iunk = null;
          int prgf = 0;
          Guid iidExtractImage = new Guid("BB2E617C-0920-11d1-9A0B-00C04FC2D6C1");
          item.GetUIObjectOf(IntPtr.Zero, 1, ref pidl, ref iidExtractImage, out prgf, ref iunk);
          extractImage = (IExtractImage)iunk;

          if (extractImage != null)
          {
            //Got an IExtractImage object!
            SIZE sz = new SIZE();
            sz.cx = desiredSize.Width;
            sz.cy = desiredSize.Height;
            StringBuilder location = new StringBuilder(260, 260);
            int priority = 0;
            int requestedColourDepth = 32;
            //EIEIFLAG flags = EIEIFLAG.IEIFLAG_ASPECT | EIEIFLAG.IEIFLAG_SCREEN;
            EIEIFLAG flags = EIEIFLAG.IEIFLAG_ORIGSIZE | EIEIFLAG.IEIFLAG_ASPECT | EIEIFLAG.IEIFLAG_QUALITY;
            int uFlags = (int)flags;

            // E.g. for PDFs on Vista...
            try
            {
              extractImage.GetLocation(location, location.Capacity, ref priority, ref sz, requestedColourDepth,
                                       ref uFlags);
            }
            catch (Exception) {}

            extractImage.Extract(out hBmp);
            if (hBmp != IntPtr.Zero)
            {
              // create the image object:
              thumbNail = System.Drawing.Bitmap.FromHbitmap(hBmp);
              // is thumbNail owned by the Bitmap?
            }

            Marshal.ReleaseComObject(extractImage);
            extractImage = null;
          }
          return true;
        }
        else
        {
          return false;
        }
      }
      catch (Exception ex)
      {
        if (hBmp != IntPtr.Zero)
        {
          UnManagedMethods.DeleteObject(hBmp);
        }
        if (extractImage != null)
        {
          Marshal.ReleaseComObject(extractImage);
        }
        throw ex;
      }
    }

    private string PathFromPidl(IntPtr pidl)
    {
      StringBuilder path = new StringBuilder(260, 260);
      int result = UnManagedMethods.SHGetPathFromIDList(pidl, path);
      if (result == 0)
      {
        return string.Empty;
      }
      else
      {
        return path.ToString();
      }
    }

    private IShellFolder getDesktopFolder
    {
      get
      {
        IShellFolder ppshf;
        int r = UnManagedMethods.SHGetDesktopFolder(out ppshf);
        return ppshf;
      }
    }

    #endregion

    #region Constructor, Destructor, Dispose

    public ThumbnailExtractor() {}

    public void Dispose()
    {
      if (!disposed)
      {
        if (alloc != null)
        {
          Marshal.ReleaseComObject(alloc);
        }
        alloc = null;

        if (thumbNail != null)
        {
          thumbNail.Dispose();
        }

        disposed = true;
      }
    }

    ~ThumbnailExtractor()
    {
      Dispose();
    }

    #endregion
  }

  #endregion
}