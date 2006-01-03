using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaPortal.TV.Teletext
{
  public class TeletextPageCache
  {
    #region constants
    const int MIN_PAGE = 0x100;
    const int MAX_PAGE = 0x900;
    const int MAX_SUB_PAGES = 0x80;
    const int MAX_ROWS = 50;
    #endregion


    IntPtr[,] _pageCache = new IntPtr[MAX_PAGE, MAX_SUB_PAGES];

    public void Clear()
    {
      // free alloctated memory
      for (int pageNr = MIN_PAGE; pageNr < MAX_PAGE; pageNr++)
      {
        for (int subPageNr = 0; subPageNr < MAX_SUB_PAGES; subPageNr++)
        {
          if (_pageCache[pageNr, subPageNr] != IntPtr.Zero)
          {
            Marshal.FreeHGlobal(_pageCache[pageNr, subPageNr]);
            _pageCache[pageNr, subPageNr] = IntPtr.Zero;
          }
        }
      }
    }

    public int NumberOfSubpages(int pageNumber)
    {
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException(String.Format("page is invalid:0x{0:X}",pageNumber));
      int totalSubPages = 0;
      for (int subPageNr = 0; subPageNr < MAX_SUB_PAGES; subPageNr++)
      {
        if (_pageCache[pageNumber, subPageNr] != IntPtr.Zero)
        {
          totalSubPages++;
        }
      }
      return totalSubPages;
    }
 
    public bool PageExists(int pageNumber)
    {
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException("page"); 
      for (int subPageNumber = 0; subPageNumber < MAX_SUB_PAGES; subPageNumber++)
      {
        if (_pageCache[pageNumber, subPageNumber] != IntPtr.Zero)
        {
          return true;
        }
      }
      return false;
    }

    public byte[] GetPage(int pageNumber, int subPageNumber)
    {
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException("page");
      if (subPageNumber < 0 || subPageNumber >= MAX_SUB_PAGES)
        throw new ArgumentOutOfRangeException("subPageNumber");

      byte[] pageChars = new byte[MAX_ROWS * 42];
      Marshal.Copy(_pageCache[pageNumber, subPageNumber], pageChars, 0, MAX_ROWS*42);
      return pageChars;
    }

    public void SetPage(int pageNumber, int subPageNumber, byte[] pageData)
    {
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException("page");
      if (subPageNumber < 0 || subPageNumber >= MAX_SUB_PAGES)
        throw new ArgumentOutOfRangeException("subPageNumber");


      AllocPage(pageNumber, subPageNumber);
      int copyLen = pageData.Length;
      if (copyLen > MAX_ROWS * 42) copyLen = MAX_ROWS * 42;
      Marshal.Copy(pageData, 0, _pageCache[pageNumber, subPageNumber], copyLen);
    }
    
    //
    // returns true if the page and the subpage exists
    public bool SubPageExists(int pageNumber, int subPageNumber)
    {
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException("page");
      if (subPageNumber < 0 || subPageNumber >= MAX_SUB_PAGES)
        throw new ArgumentOutOfRangeException("subPageNumber");

      if (_pageCache[pageNumber, subPageNumber] != IntPtr.Zero)
        return true;
      return false;
    }

    public bool AllocPage(int pageNumber, int subPageNumber)
    {
      int size = MAX_ROWS * 42;
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException("page");
      if (subPageNumber < 0 || subPageNumber >= MAX_SUB_PAGES)
        throw new ArgumentOutOfRangeException("subPageNumber");

      if (_pageCache[pageNumber, subPageNumber] != IntPtr.Zero) return true;
      byte[] emptyPage = new byte[size];
      for (int i = 0; i < size; ++i)
        emptyPage[i] = 32;

      _pageCache[pageNumber, subPageNumber] = Marshal.AllocHGlobal(size);
      Marshal.Copy(emptyPage, 0, _pageCache[pageNumber, subPageNumber], size);
      if (_pageCache[pageNumber, subPageNumber] == IntPtr.Zero)
        return false;
      return true;
    }

    public void ClearPage(int pageNumber, int subPageNumber)
    {
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException("page");
      if (subPageNumber < 0 || subPageNumber >= MAX_SUB_PAGES)
        throw new ArgumentOutOfRangeException("subPageNumber");

      if (_pageCache[pageNumber, subPageNumber] == IntPtr.Zero) return;

      Marshal.FreeHGlobal(_pageCache[pageNumber, subPageNumber]);
      _pageCache[pageNumber, subPageNumber] = IntPtr.Zero;

      AllocPage(pageNumber, subPageNumber);
    }

    public IntPtr GetPagePtr(int pageNumber, int subPageNumber)
    {
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException("page");
      if (subPageNumber < 0 || subPageNumber >= MAX_SUB_PAGES)
        throw new ArgumentOutOfRangeException("subPageNumber");

      return _pageCache[pageNumber, subPageNumber]  ;
    }
  }
}
