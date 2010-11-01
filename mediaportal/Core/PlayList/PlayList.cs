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
using System.Collections.Generic;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.TagReader;
using MediaPortal.Util;

namespace MediaPortal.Playlists
{
  //public class PlayList : IEnumerable<PlayListItem>
  [Serializable()]
  public class PlayList : IEnumerable<PlayListItem> //, IComparer
  {
    protected string _playListName = "";
    protected List<PlayListItem> _listPlayListItems = new List<PlayListItem>();

    public PlayList() {}

    public bool AllPlayed()
    {
      foreach (PlayListItem item in _listPlayListItems)
      {
        if (!item.Played)
        {
          return false;
        }
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
      if (item == null)
      {
        return;
      }
      //Log.Debug("Playlist: add {0}", item.FileName);
      _listPlayListItems.Add(item);
    }

    public bool Insert(PlayListItem item, int currentSong)
    {
      bool success = false;
      if (item == null)
      {
        return success;
      }

      Log.Info("Playlist: Insert {0} at {1}", item.FileName, Convert.ToString(currentSong));
      if (currentSong < _listPlayListItems.Count)
      {
        _listPlayListItems.Insert(currentSong + 1, item);
        success = true;
      }
      else
      {
        _listPlayListItems.Add(item);
        success = true;
      }
      return success;
    }

    public bool Insert(PlayListItem item, PlayListItem afterThisItem)
    {
      bool success = false;
      if (item == null)
      {
        return success;
      }

      for (int i = 0; i < _listPlayListItems.Count; ++i)
      {
        if (afterThisItem.FileName == _listPlayListItems[i].FileName)
        {
          Log.Info("Playlist: Insert {0} after {1}", item.FileName, afterThisItem.FileName);
          _listPlayListItems.Insert(i + 1, item);
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
        if (value == null)
        {
          return;
        }
        _playListName = value;
      }
    }

    public int Remove(string fileName)
    {
      if (fileName == null)
      {
        return -1;
      }

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
      set { _listPlayListItems[iItem] = value; }
    }


    public int RemoveDVDItems()
    {
      //TODO
      return 0;
    }

    public virtual void Shuffle()
    {
      PseudoRandomNumberGenerator r = new PseudoRandomNumberGenerator();

      // iterate through each catalogue item performing arbitrary swaps
      if (Count > 1)
      {
        for (int item = 0; item < Count; item++)
        {
          int nArbitrary = r.Next(0, Count - 1);

          PlayListItem anItem = _listPlayListItems[nArbitrary];
          _listPlayListItems[nArbitrary] = _listPlayListItems[item];
          _listPlayListItems[item] = anItem;
        }
      }
    }

    public IEnumerator<PlayListItem> GetEnumerator()
    {
      return _listPlayListItems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      IEnumerable enumerable = (IEnumerable)_listPlayListItems;
      return enumerable.GetEnumerator();
    }

    public int MovePlayListItemUp(int iItem)
    {
      int selectedItemIndex = -1;

      if (iItem < 0 || iItem >= _listPlayListItems.Count)
      {
        return -1;
      }

      int iPreviousItem = iItem - 1;

      if (iPreviousItem < 0)
      {
        iPreviousItem = _listPlayListItems.Count - 1;
      }

      PlayListItem playListItem1 = _listPlayListItems[iItem];
      PlayListItem playListItem2 = _listPlayListItems[iPreviousItem];

      if (playListItem1 == null || playListItem2 == null)
      {
        return -1;
      }

      try
      {
        Log.Info("Moving playlist item {0} up. Old index:{1}, new index{2}", playListItem1.Description, iItem,
                 iPreviousItem);
        Monitor.Enter(this);
        _listPlayListItems[iItem] = playListItem2;
        _listPlayListItems[iPreviousItem] = playListItem1;
        selectedItemIndex = iPreviousItem;
      }

      catch (Exception ex)
      {
        Log.Info("PlayList.MovePlayListItemUp caused an exception: {0}", ex.Message);
        selectedItemIndex = -1;
      }

      finally
      {
        Monitor.Exit(this);
      }

      return selectedItemIndex;
    }

    public int MovePlayListItemDown(int iItem)
    {
      int selectedItemIndex = -1;

      if (iItem < 0 || iItem >= _listPlayListItems.Count)
      {
        return -1;
      }

      int iNextItem = iItem + 1;

      if (iNextItem >= _listPlayListItems.Count)
      {
        iNextItem = 0;
      }

      PlayListItem playListItem1 = _listPlayListItems[iItem];
      PlayListItem playListItem2 = _listPlayListItems[iNextItem];

      if (playListItem1 == null || playListItem2 == null)
      {
        return -1;
      }

      try
      {
        Log.Info("Moving playlist item {0} down. Old index:{1}, new index{2}", playListItem1.Description, iItem,
                 iNextItem);
        Monitor.Enter(this);
        _listPlayListItems[iItem] = playListItem2;
        _listPlayListItems[iNextItem] = playListItem1;
        selectedItemIndex = iNextItem;
      }

      catch (Exception ex)
      {
        Log.Info("PlayList.MovePlayListItemDown caused an exception: {0}", ex.Message);
        selectedItemIndex = -1;
      }

      finally
      {
        Monitor.Exit(this);
      }

      return selectedItemIndex;
    }

    public void Sort()
    {
      _listPlayListItems.Sort(new PlayListItemComparer());
    }
  }

  internal class PlayListItemComparer : IComparer<PlayListItem>
  {
    public enum SortMethod
    {
      Alpha,
      ArtistByTrack,
      ArtistByAlbum
    } ;

    private SortMethod _SortBy = SortMethod.ArtistByAlbum;

    public SortMethod SortBy
    {
      set { _SortBy = value; }
    }

    public PlayListItemComparer() {}

    public PlayListItemComparer(SortMethod sortBy)
    {
      _SortBy = sortBy;
    }

    public int Compare(PlayListItem item1, PlayListItem item2)
    {
      if (item1.MusicTag == null || item2.MusicTag == null)
      {
        return 0;
      }

      MusicTag tag1 = (MusicTag)item1.MusicTag;
      MusicTag tag2 = (MusicTag)item2.MusicTag;

      int stringCompareResults = tag1.Album.CompareTo(tag2.Album);

      if (stringCompareResults == 0)
      {
        return tag1.Track.CompareTo(tag2.Track);
      }

      else
      {
        return stringCompareResults;
      }
    }
  }
}