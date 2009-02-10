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
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TvLibrary.Teletext
{
  /// <summary>
  /// Teletext page container
  /// </summary>
  public class TeletextPage : IDisposable
  {
    #region constants
    const int MAX_SUB_PAGES = 0x80;
    const int MAX_ROWS = 50;
    #endregion

    #region variables

    readonly int _pageNumber = -1;
    int _numberOfSubPages = -1;
    readonly IntPtr[] _pageCache = new IntPtr[MAX_SUB_PAGES];
    DateTime _lastTimeRoulated = DateTime.MinValue;
    DateTime _lastTimeReceived = DateTime.MinValue;
    int _previousSubPageNumber = -1;
    TimeSpan _rotationTime = new TimeSpan(0, 0, 15);
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TeletextPage"/> class.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    public TeletextPage(int pageNumber)
    {
      _pageNumber = pageNumber;
    }
    #endregion

    #region properties

    ///<summary>
    /// Gets the page number
    ///</summary>
    public int PageNumber
    {
      get { return _pageNumber; }
    }

    /// <summary>
    /// Gets the sub page count.
    /// </summary>
    /// <value>The sub page count.</value>
    public int SubPageCount
    {
      get
      {
        return _numberOfSubPages;
      }
    }
    /// <summary>
    /// Gets the rotation time.
    /// </summary>
    /// <value>The rotation time.</value>
    public TimeSpan RotationTime
    {
      get
      {
        return _rotationTime;
      }
    }
    /// <summary>
    /// Gets the last time received.
    /// </summary>
    /// <value>The last time received.</value>
    public DateTime LastTimeReceived
    {
      get
      {
        return _lastTimeReceived;
      }
    }

    /// <summary>
    /// Gets the last time roulated.
    /// </summary>
    /// <value>The last time roulated.</value>
    public DateTime LastTimeRoulated
    {
      get
      {
        return _lastTimeRoulated;
      }
    }
    #endregion

    #region public methods
    /// <summary>
    /// Gets the sub page.
    /// </summary>
    /// <param name="subPageNumber">The sub page number.</param>
    /// <returns></returns>
    public byte[] GetSubPage(int subPageNumber)
    {
      if (subPageNumber < 0 || subPageNumber > _numberOfSubPages)
      {
        return null;
      }
      if (_pageCache[subPageNumber] == IntPtr.Zero)
        return null;
      byte[] pageChars = new byte[MAX_ROWS * 42];
      Marshal.Copy(_pageCache[subPageNumber], pageChars, 0, MAX_ROWS * 42);
      return pageChars;
    }

    /// <summary>
    /// Deletes the specified page number.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="subPageNumber">The sub page number.</param>
    /// <returns></returns>
    public bool Delete(int pageNumber, int subPageNumber)
    {
      if (_numberOfSubPages < subPageNumber)
        return false;
      //if (pageNumber == 0x100)
      //{
      //  Log.Log.WriteFile("del {0:X} {1}-{2}", pageNumber, subPageNumber, _numberOfSubPages);
      //}
      //subpage removed
      for (int i = subPageNumber; i <= _numberOfSubPages; ++i)
      {
        FreePage(i);
      }
      _numberOfSubPages = subPageNumber - 1;
      return true;
    }

    /// <summary>
    /// Subs the page received.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="subPageNumber">The sub page number.</param>
    /// <param name="pageData">The page data.</param>
    /// <param name="isUpdate">if set to <c>true</c> [is update].</param>
    /// <param name="isNew">if set to <c>true</c> [is new].</param>
    /// <param name="isDeleted">if set to <c>true</c> [is deleted].</param>
    /// <param name="vbiLines">VBI lines</param>
    public void SubPageReceived(int pageNumber, ref int subPageNumber, ref byte[] pageData, out bool isUpdate, out bool isNew, out bool isDeleted, string vbiLines)
    {
      isDeleted = false;
      isUpdate = false;
      isNew = false;

      if (subPageNumber < 0 || subPageNumber >= 0x80)
      {
        //invalid subpage 
        return;
      }
      if (SubPageExists(subPageNumber))
      {
        if (PageDiffers(pageData, subPageNumber))
        {
          Trace.WriteLine(String.Format("page:{0:X}/{1:X} updated {2}", pageNumber, subPageNumber, vbiLines));
          isUpdate = true;
          UpdatePage(subPageNumber, pageData);
        }
      }



      if (_numberOfSubPages > 0 && subPageNumber != _numberOfSubPages)
      {
        _rotationTime = DateTime.Now - _lastTimeRoulated;
        if (RotationTime.TotalSeconds < 1)
          _rotationTime = new TimeSpan(0, 0, 1);
        if (RotationTime.TotalSeconds > 15)
          _rotationTime = new TimeSpan(0, 0, 15);
      }
      if (subPageNumber == _numberOfSubPages + 1)
      {
        //received a new subpage        
        //if (pageNumber == 0x600)
        // Trace.WriteLine(String.Format(" subpage added total:{0} prev:{1}", _numberOfSubPages, _previousSubPageNumber));

        _lastTimeRoulated = DateTime.Now;
        _lastTimeReceived = DateTime.Now;
        _numberOfSubPages = subPageNumber;
        _previousSubPageNumber = subPageNumber;
        if (!SubPageExists(subPageNumber))
        {
          AllocPage(subPageNumber, pageData);
        }
        _lastTimeReceived = DateTime.Now;
        isNew = true;
        return;
      }

      if (subPageNumber > _numberOfSubPages)
      {
        _lastTimeRoulated = DateTime.Now;
        _lastTimeReceived = DateTime.Now;
        _numberOfSubPages++;
        subPageNumber = _numberOfSubPages;
        _previousSubPageNumber = subPageNumber;
        if (!SubPageExists(subPageNumber))
        {
          AllocPage(subPageNumber, pageData);
        }
        _lastTimeReceived = DateTime.Now;
        isNew = true;

        return;
      }

      if (subPageNumber == _previousSubPageNumber)
      {
        //same subpage received
        //if (pageNumber == 0x600) Trace.WriteLine("Same subpage");
        _lastTimeReceived = DateTime.Now;
        if (!SubPageExists(subPageNumber))
        {
          AllocPage(subPageNumber, pageData);
        }

        return;
      }

      if (subPageNumber < _previousSubPageNumber)
      {
        _lastTimeReceived = DateTime.Now;
        if (_previousSubPageNumber == _numberOfSubPages)
        {
          //normal roulation
          //if (pageNumber == 0x600)
          //Trace.WriteLine(String.Format(" from {0}->0", _previousSubPageNumber, subPageNumber));
          _lastTimeRoulated = DateTime.Now;
          _previousSubPageNumber = subPageNumber;
          if (!SubPageExists(subPageNumber))
          {
            AllocPage(subPageNumber, pageData);
          }
        }
        else
        {
          //          Trace.WriteLine(String.Format(" from {0}->{1} remove subs", _previousSubPageNumber, subPageNumber));
          //subpage removed
          for (int i = _previousSubPageNumber + 1; i <= 0x80; ++i)
          {
            FreePage(i);
          }
          _numberOfSubPages = _previousSubPageNumber;
          _previousSubPageNumber = subPageNumber;
          isDeleted = true;

          if (!SubPageExists(subPageNumber))
          {
            AllocPage(subPageNumber, pageData);
          }
        }
      }
      else
      {
        //if (pageNumber == 0x600)
        //Trace.WriteLine(String.Format(" from {0}->{1}", _previousSubPageNumber, subPageNumber));
        _lastTimeReceived = DateTime.Now;
        if (!SubPageExists(subPageNumber))
        {
          AllocPage(subPageNumber, pageData);
        }
      }
      _previousSubPageNumber = subPageNumber;
      _lastTimeReceived = DateTime.Now;
    }
    #endregion

    #region private members

    bool PageDiffers(byte[] pageData, int subPageNumber)
    {
      if (subPageNumber < 0 || subPageNumber > _numberOfSubPages)
      {
        return false;
      }
      IntPtr pagePtr = _pageCache[subPageNumber];
      if (pagePtr == IntPtr.Zero)
        return false;
      unsafe
      {

        byte* ptr = (byte*)pagePtr.ToPointer();
        bool isSet = Hamming.IsEraseBitSet(0, ref pageData);
        for (int row = 0; row < 31; row++)
        {
          int off = row * 42;
          if (row != 0)
          {
            if (pageData[off] == 32 && isSet)
            {
              for (int col = 0; col < 42; col++)
              {
                if (ptr[off + col] != 32 && pageData[off + col] == 32)
                {
                  return true;
                }
              }
              continue;
            }
          }


          off = row * 42;
          int rowNr = Hamming.GetPacketNumber(off, ref pageData);

          if (rowNr < 0)
            continue;
          for (int col = 0; col < 42; col++)
          {
            byte newData = pageData[off + col];
            if (rowNr != 0)
            {
              if (ptr[off + col] != newData)
              {
                if (rowNr >= 1 && rowNr <= 24)
                {
                  if (col >= 2)
                  {
                    if (OddParity.IsCorrect(newData))
                    {
                      // Trace.WriteLine(String.Format("2) {0:X}/{1} r:{2} c:{3} {4} {5:X}!={6:X}", _pageNumber, subPageNumber, row, col, _clearSubPage[subPageNumber], ptr[off + col], pageData[off + col]));
                      return true;
                    }
                  }
                  else if (Hamming.Decode[newData] != 0xff)
                  {
                    //bytes 0-1 = row/column, hamming 8/4 coded

                    //Trace.WriteLine(String.Format("3) {0:X}/{1} r:{2} c:{3} {4} {5:X}!={6:X}", _pageNumber, subPageNumber, row, col, _clearSubPage[subPageNumber], ptr[off + col], pageData[off + col]));
                    return true;
                  }
                }
                else
                {
                  //rows 25,26,27

                  //Trace.WriteLine(String.Format("4) {0:X}/{1} r:{2} c:{3} {4} {5:X}!={6:X}", _pageNumber, subPageNumber, row, col, _clearSubPage[subPageNumber], ptr[off + col], pageData[off + col]));
                  return true;
                }
              }
            }
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Updates the page.
    /// </summary>
    /// <param name="subPageNumber">The sub page number.</param>
    /// <param name="pageData">The page data.</param>
    /// <returns></returns>
    void UpdatePage(int subPageNumber, byte[] pageData)
    {
      if (subPageNumber < 0 || subPageNumber > _numberOfSubPages)
      {
        return;
      }
      IntPtr pagePtr = _pageCache[subPageNumber];
      unsafe
      {
        byte* ptr = (byte*)pagePtr.ToPointer();
        bool isSet = Hamming.IsEraseBitSet(0, ref pageData);
        for (int row = 0; row < 31; row++)
        {
          int off = row * 42;
          if (row != 0)
          {
            if (pageData[off] == 32 && isSet)
            {
              for (int col = 0; col < 42; col++)
              {
                if (ptr[off + col] != 32 && pageData[off + col] == 32)
                {
                  ptr[off + col] = 32;
                }
              }
              continue;
            }
          }


          off = row * 42;
          int rowNr = Hamming.GetPacketNumber(off, ref pageData);
          if (rowNr < 0)
            continue;
          for (int col = 0; col < 42; col++)
          {
            byte newData = pageData[off + col];
            if (rowNr != 0)
            {
              if (ptr[off + col] != newData)
              {
                if (rowNr >= 1 && rowNr <= 24)
                {
                  if (col >= 2)
                  {
                    if (OddParity.IsCorrect(newData))
                    {
                      ptr[off + col] = newData;
                    }
                  }
                  else if (Hamming.Decode[newData] != 0xff)
                  {
                    //bytes 0-1 = row/column, hamming 8/4 coded
                    ptr[off + col] = newData;
                  }
                }
                else
                {
                  //rows 25,26,27
                  ptr[off + col] = newData;
                }
              }
            }
            else
            {
              //row 0
              ptr[off + col] = newData;
            }
          }
        }
      }

      return;
    }

    /// <summary>
    /// Frees the page.
    /// </summary>
    /// <param name="subPageNumber">The sub page number.</param>
    void FreePage(int subPageNumber)
    {
      if (subPageNumber < 0 || subPageNumber >= 0x80)
      {
        //invalid subpage 
        return;
      }
      if (_pageCache[subPageNumber] != IntPtr.Zero)
      {
        Marshal.FreeHGlobal(_pageCache[subPageNumber]);
        _pageCache[subPageNumber] = IntPtr.Zero;
      }
    }

    /// <summary>
    /// Allocs the page.
    /// </summary>
    /// <param name="subPageNumber">The sub page number.</param>
    /// <param name="page">Page data</param>
    void AllocPage(int subPageNumber, byte[] page)
    {
      if (subPageNumber < 0 || subPageNumber >= 0x80)
      {
        //invalid subpage 
        return;
      }

      if (_pageCache[subPageNumber] != IntPtr.Zero)
        return;
      const int size = MAX_ROWS * 42;

      _pageCache[subPageNumber] = Marshal.AllocHGlobal(size);
      Marshal.Copy(page, 0, _pageCache[subPageNumber], size);
    }
    /// <summary>
    /// Subs the page exists.
    /// </summary>
    /// <param name="subPageNumber">The sub page number.</param>
    /// <returns></returns>
    bool SubPageExists(int subPageNumber)
    {
      if (subPageNumber < 0 || subPageNumber >= 0x80)
      {
        return false;
      }

      if (_pageCache[subPageNumber] == IntPtr.Zero)
        return false;
      return true;
    }
    #endregion

    #region IDisposable Members

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      for (int i = 0; i < MAX_SUB_PAGES; ++i)
      {
        FreePage(i);
      }
    }

    #endregion
  }
}
