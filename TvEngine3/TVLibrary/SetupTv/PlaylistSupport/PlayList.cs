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
using System.Collections.Generic;
using System.Collections;

namespace MediaPortal.Playlists
{
  //public class PlayList : IEnumerable<PlayListItem>
  [Serializable]
  public class PlayList : IEnumerable<PlayListItem> //, IComparer
  {
    protected string _playListName = "";
    protected List<PlayListItem> _listPlayListItems = new List<PlayListItem>();

    public bool AllPlayed()
    {
      foreach (PlayListItem item in _listPlayListItems)
      {
        if (!item.Played) return false;
      }
      return true;
    }

    public void ResetStatus()
    {
      foreach (PlayListItem item in _listPlayListItems)
      {
        item.Played = false;
      }
    }

    public void Add(PlayListItem item)
    {
      if (item == null) return;
      //Log.Debug("Playlist: add {0}", item.FileName);
      _listPlayListItems.Add(item);
    }

    public bool Insert(PlayListItem item, int currentSong)
    {
      if (item == null)
        return false;  

      if (currentSong < _listPlayListItems.Count)
      {
        _listPlayListItems.Insert(currentSong + 1, item);
      }
      else
      {
        _listPlayListItems.Add(item);
      }
      return true;
    }

    public bool Insert(PlayListItem item, PlayListItem afterThisItem)
    {
      bool success = false;
      if (item == null)
        return false;      

      for (int i = 0; i < _listPlayListItems.Count; ++i)
      {
        if (afterThisItem.FileName == _listPlayListItems[i].FileName)
        {
          _listPlayListItems.Insert(i+1, item);
          success = true;
        }
      }
      return success;
    }

    public string Name
    {
      get { return _playListName; }
      set
      {
        if (value == null) return;
        _playListName = value;
      }
    }

    public int Remove(string fileName)
    {
      if (fileName == null) return -1;

      for (int i = 0; i < _listPlayListItems.Count; ++i)
      {
        PlayListItem item = _listPlayListItems[i];
        if (item.FileName == fileName)
        {
          _listPlayListItems.RemoveAt(i);
          return i;
        }
      }
      return -1;
    }

    public void Clear()
    {
      _listPlayListItems.Clear();
    }

    public int Count
    {
      get { return _listPlayListItems.Count; }
    }

    public PlayListItem this[int iItem]
    {
      get { return _listPlayListItems[iItem]; }
    }


    public int RemoveDVDItems()
    {
      //TODO
      return 0;
    }

    public virtual void Shuffle()
    {
      Random r = new Random(DateTime.Now.Millisecond);

      // iterate through each catalogue item performing arbitrary swaps
      for (int item = 0; item < Count; item++)
      {
        int nArbitrary = r.Next(Count);

        PlayListItem anItem = _listPlayListItems[nArbitrary];
        _listPlayListItems[nArbitrary] = _listPlayListItems[item];
        _listPlayListItems[item] = anItem;
      }
    }

    public IEnumerator<PlayListItem> GetEnumerator()
    {
      return _listPlayListItems.GetEnumerator();

    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      IEnumerable enumerable = _listPlayListItems;
      return enumerable.GetEnumerator();
    }

    public int MovePlayListItemUp(int iItem)
    {
      int selectedItemIndex;

      if (iItem < 0 || iItem >= _listPlayListItems.Count)
        return -1;

      int iPreviousItem = iItem - 1;

      if (iPreviousItem < 0)
        iPreviousItem = _listPlayListItems.Count - 1;

      PlayListItem playListItem1 = _listPlayListItems[iItem];
      PlayListItem playListItem2 = _listPlayListItems[iPreviousItem];

      if (playListItem1 == null || playListItem2 == null)
        return -1;

      try
      {
        System.Threading.Monitor.Enter(this);
        _listPlayListItems[iItem] = playListItem2;
        _listPlayListItems[iPreviousItem] = playListItem1;
        selectedItemIndex = iPreviousItem;
      }

      catch (Exception)
      {
        selectedItemIndex = -1;
      }

      finally
      {
        System.Threading.Monitor.Exit(this);
      }

      return selectedItemIndex;
    }

    public int MovePlayListItemDown(int iItem)
    {
      int selectedItemIndex;

      if (iItem < 0 || iItem >= _listPlayListItems.Count)
        return -1;

      int iNextItem = iItem + 1;

      if (iNextItem >= _listPlayListItems.Count)
        iNextItem = 0;

      PlayListItem playListItem1 = _listPlayListItems[iItem];
      PlayListItem playListItem2 = _listPlayListItems[iNextItem];

      if (playListItem1 == null || playListItem2 == null)
        return -1;

      try
      {
        System.Threading.Monitor.Enter(this);
        _listPlayListItems[iItem] = playListItem2;
        _listPlayListItems[iNextItem] = playListItem1;
        selectedItemIndex = iNextItem;
      }

      catch (Exception)
      {
        selectedItemIndex = -1;
      }

      finally
      {
        System.Threading.Monitor.Exit(this);
      }

      return selectedItemIndex;
    }
  }
}
