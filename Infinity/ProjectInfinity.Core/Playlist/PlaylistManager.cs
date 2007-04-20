using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using ProjectInfinity.Logging;

namespace ProjectInfinity.Playlist
{
  public class PlaylistManager : IPlaylistManager
  {
    #region variables
    List<IPlaylistItem> _items = new List<IPlaylistItem>();
    #endregion

    #region IPlaylistManager members
    public void Add(IPlaylistItem item)
    {
      _items.Add(item);
    }

    public void Remove(IPlaylistItem item)
    {
      _items.Remove(item);
    }

    public void RemoveAt(int index)
    {
      _items.RemoveAt(index);
    }

    public void Clear()
    {
      _items.Clear();
    }

    /// <summary>
    /// Moves a specified item up in the playlist order
    /// </summary>
    /// <param name="item">item to move</param>
    public void MovePlaylistItemUp(object item)
    {
      int currPos = _items.IndexOf((IPlaylistItem)item);
      int newPos = currPos - 1;

      if (newPos < 0) newPos = 0;

      _items.Insert(newPos, (IPlaylistItem)item);
      _items.RemoveAt(currPos + 1);
    }

    /// <summary>
    /// Moves a specified item down in the playlist order
    /// </summary>
    /// <param name="item">item to move</param>
    public void MovePlaylistItemDown(object item)
    {
      int currPos = _items.IndexOf((IPlaylistItem)item);
      int newPos = currPos + 2;

      if (newPos > _items.Count) newPos = _items.Count;

      _items.Insert(newPos, (IPlaylistItem)item);
      _items.RemoveAt(currPos);
    }

    public IPlaylistItem this[int index]
    {
      get { return _items[index]; }
    }

    public IList PlaylistItems
    {
      get { return _items; }
    }

    public int Count
    {
      get { return _items.Count; }
    }
    #endregion
  }
}
