using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

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
