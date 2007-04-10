using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Playlist
{
  public interface IPlaylistItem
  {
    /// <summary>
    /// Display text of each item
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Path to the file to play (absolute path)
    /// </summary>
    string Path { get; }

    /// <summary>
    /// File size as a string
    /// </summary>
    string Size { get; }

    /// <summary>
    /// File size as double (for sorting etc)
    /// </summary>
    double RealSize { get; }

    /// <summary>
    /// If you want a custom icon beside the Title, set this to point
    /// to the file (absolute/relative)
    /// </summary>
    string Logo { get; }
  }
}
