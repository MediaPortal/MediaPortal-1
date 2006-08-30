/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TvLibrary.Teletext
{
  public class TeletextPage : IDisposable
  {
    #region constants
    const int MAX_SUB_PAGES = 0x80;
    const int MAX_ROWS = 50;
    #endregion

    #region variables
    int _pageNumber = -1;
    int _numberOfSubPages = -1;
    IntPtr[] _pageCache = new IntPtr[MAX_SUB_PAGES];

    DateTime _lastTimeRoulated = DateTime.MinValue;
    DateTime _lastTimeReceived = DateTime.MinValue;
    int _previousSubPageNumber = -1;
    TimeSpan _rotationTime = new TimeSpan(0, 0, 15);
    #endregion

    #region ctor
    public TeletextPage(int pageNumber)
    {
      _pageNumber = pageNumber;
    }
    #endregion

    #region properties
    public int SubPageCount
    {
      get
      {
        return _numberOfSubPages;
      }
    }
    public TimeSpan RotationTime
    {
      get
      {
        return _rotationTime;
      }
    }
    public DateTime LastTimeReceived
    {
      get
      {
        return _lastTimeReceived;
      }
    }

    public DateTime LastTimeRoulated
    {
      get
      {
        return _lastTimeRoulated;
      }
    }
    #endregion
    #region public methods
    public byte[] GetSubPage(int subPageNumber)
    {
      if (subPageNumber < 0 || subPageNumber > _numberOfSubPages)
      {
        return null;
      }
      byte[] pageChars = new byte[MAX_ROWS * 42];
      Marshal.Copy(_pageCache[subPageNumber], pageChars, 0, MAX_ROWS * 42);
      return pageChars;
    }

    public void Clear(int subPageNumber)
    {
      if (subPageNumber < 0 || subPageNumber > _numberOfSubPages)
      {
        return;
      }
      if (_pageCache[subPageNumber] == IntPtr.Zero) return;
      byte[] data = new byte[MAX_ROWS * 42];

      for (int i = 0; i < data.Length;++i )
      {
        data[i] = 32;
      }
      Marshal.Copy(data, 0, _pageCache[subPageNumber], MAX_ROWS * 42);
    }

    public bool Delete(int pageNumber,int subPageNumber)
    {
      if (_numberOfSubPages < subPageNumber) return false;
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

    public void SubPageReceived(int pageNumber,int subPageNumber, byte[] pageData, out bool isUpdate, out bool isNew, out bool isDeleted)
    {
      //if (pageNumber == 0x100)
        //{
        //Log.Log.Write("received {0:X}/{1} total:{2} prev:{3}", 
        //  pageNumber, subPageNumber,_numberOfSubPages,_previousSubPageNumber);
      //}
      isDeleted = false;
      isUpdate = false;
      isNew = false;
      if (subPageNumber < 0 || subPageNumber >= 0x80)
      {
        //invalid subpage 
        return;
      }
      if (_numberOfSubPages > 0 && subPageNumber != _numberOfSubPages)
      {
        _rotationTime = DateTime.Now - _lastTimeRoulated;
        if (RotationTime.TotalSeconds < 1)
          _rotationTime = new TimeSpan(0, 0, 1);
        if (RotationTime.TotalSeconds >15)
          _rotationTime = new TimeSpan(0, 0, 15);
      }
      if (subPageNumber == _numberOfSubPages + 1)
      {
        //received a new subpage        
        //if (pageNumber == 0x100)
          //  Log.Log.WriteFile(" subpage added total:{0} prev:{1}", _numberOfSubPages, _previousSubPageNumber);

        _lastTimeRoulated = DateTime.Now;
        _lastTimeReceived = DateTime.Now;
        _numberOfSubPages = subPageNumber;
        _previousSubPageNumber = subPageNumber;
        AllocPage(subPageNumber);
        UpdatePage(subPageNumber, pageData);
        _lastTimeReceived = DateTime.Now;
        isNew = true;
        return;
      }

      if (subPageNumber > _numberOfSubPages)
      {
        _numberOfSubPages++;
        _lastTimeRoulated = DateTime.Now;
        _previousSubPageNumber = _numberOfSubPages;
        AllocPage(_numberOfSubPages);
        UpdatePage(_numberOfSubPages, pageData);
        _lastTimeReceived = DateTime.Now;
        isNew = true;
        //if (pageNumber == 0x100)
          //  Log.Log.WriteFile(" subpage added2 total:{0} prev:{1}", _numberOfSubPages, _previousSubPageNumber);
        return;
      }

      if (subPageNumber == _previousSubPageNumber)
      {
        //same subpage received
        //if (pageNumber == 0x100) Log.Log.WriteFile(" same subpage");
        _lastTimeReceived = DateTime.Now;
        AllocPage(subPageNumber);

        isUpdate = UpdatePage(subPageNumber, pageData);
        return;
      }

      if (subPageNumber < _previousSubPageNumber)
      {
        _lastTimeReceived = DateTime.Now;
        if (_previousSubPageNumber == _numberOfSubPages)
        {
          //normal roulation
          //if (pageNumber == 0x100) Log.Log.WriteFile(" from {0}->0", _previousSubPageNumber, subPageNumber);
          _lastTimeRoulated = DateTime.Now;
          _previousSubPageNumber = subPageNumber;
          AllocPage(subPageNumber);
          isUpdate = UpdatePage(subPageNumber, pageData);
        }
        else
        {
          //if (pageNumber == 0x100) Log.Log.WriteFile(" from {0}->{1} remove subs", _previousSubPageNumber, subPageNumber);
          //subpage removed
          for (int i = _previousSubPageNumber + 1; i <= 0x80; ++i)
          {
            FreePage(i);
          }
          _numberOfSubPages = _previousSubPageNumber;
          _previousSubPageNumber = subPageNumber;
          isDeleted = true;

          AllocPage(subPageNumber);
          UpdatePage(subPageNumber, pageData);
        }
      }
      _previousSubPageNumber = subPageNumber;
      _lastTimeReceived = DateTime.Now;
    }
    #endregion

    #region private members
    void SavePage(byte[] page, string fileName)
    {
      using (FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate))
      {
        stream.Write(page, 0, 1008);
      }
    }
    bool UpdatePage(int subPageNumber, byte[] pageData)
    {
      bool updated = false;
      byte[] data = GetSubPage(subPageNumber);
      for (int row = 0; row < 31; row++)
      {
        int off = row * 42;
        if (pageData[off] == 32) continue;
        for (int col = 0; col < 42; col++)
        {
          if (updated == false)
          {
            if (row != 0)
            {
              if (data[off + col] != pageData[off + col])
              {
                updated = true;
              }
            }
          }
          data[off + col] = pageData[off + col];
        }
      }
      Marshal.Copy(data, 0, _pageCache[subPageNumber], MAX_ROWS * 42);
      return updated;
    }

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

    void AllocPage(int subPageNumber)
    {
      if (subPageNumber < 0 || subPageNumber >= 0x80)
      {
        //invalid subpage 
        return;
      }

      if (_pageCache[subPageNumber] != IntPtr.Zero) return;
      int size = MAX_ROWS * 42;
      byte[] emptyPage = new byte[size];
      for (int i = 0; i < size; ++i)
        emptyPage[i] = 32;

      _pageCache[subPageNumber] = Marshal.AllocHGlobal(size);
      Marshal.Copy(emptyPage, 0, _pageCache[subPageNumber], size);
    }
    #endregion

    #region IDisposable Members

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
