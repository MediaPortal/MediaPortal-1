using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace ProjectInfinity.Playlist
{
  public interface IPlaylistManager
  {
    /// <summary>
    /// Adds a new item to the playlist.
    /// </summary>
    /// <param name="item">the IPlaylistItem</param>
    void Add(IPlaylistItem item);

    /// <summary>
    /// Removes an existing item from the playlist.
    /// </summary>
    /// <param name="item">the IPlaylistItem</param>
    void Remove(IPlaylistItem item);

    /// <summary>
    /// Removes an existing item from the playlist (by index).
    /// </summary>
    /// <param name="index">index of the IPlaylistItem</param>
    void RemoveAt(int index);

    /// <summary>
    /// Clears the whole playlist.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets the <see cref="IPlaylistItem" /> at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    IPlaylistItem this[int index] { get; }

    IList PlaylistItems { get; }

    /// <summary>
    /// Gets the count of items in the playlist.
    /// </summary>
    int Count { get; }
  }
}
