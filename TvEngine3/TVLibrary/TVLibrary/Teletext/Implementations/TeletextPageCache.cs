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
using System;

namespace TvLibrary.Teletext
{
  ///<summary>
  /// Teletext page cache
  ///</summary>
  public class TeletextPageCache : IDisposable
  {
    #region constants
    const int MIN_PAGE = 0x100;
    const int MAX_PAGE = 0x900;
    #endregion

    #region delegates
    /// <summary>
    /// On Page updated event
    /// </summary>
    public event PageEventHandler OnPageUpdated;
    /// <summary>
    /// On page added event
    /// </summary>
    public event PageEventHandler OnPageAdded;
    /// <summary>
    /// On Page deleted event
    /// </summary>
    public event PageEventHandler OnPageDeleted;
    #endregion

    #region variables

    readonly TeletextPage[] _pageCache = new TeletextPage[MAX_PAGE];
    string _channelName = "";
    DateTime _checkTimer = DateTime.MinValue;
    #endregion

    /// <summary>
    /// Channel name
    /// </summary>
    public string ChannelName
    {
      get
      {
        return _channelName;
      }
      set
      {
        _channelName = value;
      }
    }

    /// <summary>
    /// Clear the cache
    /// </summary>
    public void Clear()
    {
      // free alloctated memory
      for (int pageNr = MIN_PAGE; pageNr < MAX_PAGE; pageNr++)
      {
        if (_pageCache[pageNr] != null)
        {
          _pageCache[pageNr].Dispose();
          _pageCache[pageNr] = null;
        }
      }
      _channelName = "";
    }

    /// <summary>
    /// Gets the number of subpages for the given page number
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <returns>Number of sub pages</returns>
    public int NumberOfSubpages(int pageNumber)
    {
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException(String.Format("page is invalid:0x{0:X}", pageNumber));

      if (_pageCache[pageNumber] == null)
        return -1;
      return _pageCache[pageNumber].SubPageCount;
    }

    /// <summary>
    /// Checks, if the given page number is in the cache
    /// </summary>
    /// <param name="pageNumber">Page nubmer</param>
    /// <returns>true, if the page is in the cache</returns>
    public bool PageExists(int pageNumber)
    {
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException("pageNumber");

      if (_pageCache[pageNumber] == null)
        return false;
      return true;
    }

    /// <summary>
    /// Gets the page, subpage 
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="subPageNumber">Subpage number</param>
    /// <returns>Bytes of the page, subpage</returns>
    public byte[] GetPage(int pageNumber, int subPageNumber)
    {
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException("pageNumber");

      if (_pageCache[pageNumber] == null)
        return null;
      return subPageNumber > _pageCache[pageNumber].SubPageCount ? null : _pageCache[pageNumber].GetSubPage(subPageNumber);
    }


    /// <summary>
    /// Checks, if the sub page exists
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="subPageNumber">Subpage number</param>
    /// <returns>returns true if the page and the subpage exists</returns>
    public bool SubPageExists(int pageNumber, int subPageNumber)
    {
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException("pageNumber");

      if (_pageCache[pageNumber] == null)
        return false;
      return subPageNumber <= _pageCache[pageNumber].SubPageCount;
    }

    /// <summary>
    /// Gets the rotation time of teletext page
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <returns>Timespan of the rotation</returns>
    public TimeSpan RotationTime(int pageNumber)
    {
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException("pageNumber");

      return _pageCache[pageNumber] == null ? new TimeSpan(0, 0, 15) : _pageCache[pageNumber].RotationTime;
    }

    /// <summary>
    /// Deletes the page, subpage
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="subPageNumber">Subpage number</param>
    public void DeletePage(int pageNumber, int subPageNumber)
    {
      //if (pageNumber == 0x600) Trace.WriteLine(String.Format("DeletePage {0:X}/{1:X}", pageNumber, subPageNumber));
      if (subPageNumber > 0)
      {
        subPageNumber--;
      }
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException("pageNumber");

      if (_pageCache[pageNumber] == null)
        return;
      if (_pageCache[pageNumber].Delete(pageNumber, subPageNumber))
      {
        if (OnPageDeleted != null)
        {
          OnPageDeleted(pageNumber, subPageNumber);
        }
      }
      if (_pageCache[pageNumber].SubPageCount < 0)
      {
        _pageCache[pageNumber].Dispose();
        _pageCache[pageNumber] = null;
      }
    }

    /// <summary>
    /// Called when a page is received to store in the cache
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="subPageNumber">Subpage number</param>
    /// <param name="pageData">PageData </param>
    /// <param name="vbiLines">VBI lines</param>
    public void PageReceived(int pageNumber, int subPageNumber, byte[] pageData, string vbiLines)
    {
      //if (pageNumber == 0x600) Trace.WriteLine(String.Format("PageReceived {0:X}/{1:X}", pageNumber, subPageNumber));
      if (pageNumber < MIN_PAGE || pageNumber >= MAX_PAGE)
        throw new ArgumentOutOfRangeException("pageNumber");


      if (_pageCache[pageNumber] == null)
      {
        _pageCache[pageNumber] = new TeletextPage(pageNumber);
      }
      if (subPageNumber == 0)
      {
        if (_pageCache[pageNumber].SubPageCount > 0)
        {
          for (int i = 1; i <= _pageCache[pageNumber].SubPageCount; ++i)
          {

            _pageCache[pageNumber].Delete(pageNumber, i);
            if (OnPageDeleted != null)
            {
              OnPageDeleted(pageNumber, i);
            }
          }
        }
      }

      if (subPageNumber > 0)
      {
        subPageNumber--;
      }
      bool isUpdate, isNew, isDeleted;
      _pageCache[pageNumber].SubPageReceived(pageNumber, ref subPageNumber, ref pageData, out isUpdate, out isNew, out isDeleted, vbiLines);
      if (isNew)
      {
        if (OnPageAdded != null)
        {
          OnPageAdded(pageNumber, subPageNumber);
        }
      }
      if (isDeleted)
      {
        if (OnPageDeleted != null)
        {
          OnPageDeleted(pageNumber, subPageNumber);
        }
      }
      if (isUpdate)
      {
        if (OnPageUpdated != null)
        {
          OnPageUpdated(pageNumber, subPageNumber);
        }
      }
      TimeSpan ts = DateTime.Now - _checkTimer;
      if (ts.TotalSeconds < 10)
        return;
      for (pageNumber = MIN_PAGE; pageNumber < MAX_PAGE; pageNumber++)
      {
        if (_pageCache[pageNumber] == null)
          continue;
        if (_pageCache[pageNumber].SubPageCount < 0)
          continue;
        ts = DateTime.Now - _pageCache[pageNumber].LastTimeReceived;
        if (ts.TotalSeconds >= 120)
        {
          //if (pageNumber == 0x600)
          //  Trace.WriteLine("timeout on 600");
          _pageCache[pageNumber].Dispose();
          _pageCache[pageNumber] = null;
          if (OnPageDeleted != null)
          {
            OnPageDeleted(pageNumber, 0);
          }
        }
      }
      _checkTimer = DateTime.Now;
    }

    #region IDisposable Members
    /// <summary>
    /// Destructor
    /// </summary>
    public void Dispose()
    {
      Clear();
    }

    #endregion
  }
}
