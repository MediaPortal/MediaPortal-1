using System;
using System.Collections.Generic;
using System.Text;
using TvControl;

namespace MyTv
{
  public interface ITvPlayerCollection
  {
    /// <summary>
    /// creates and returns a new media player
    /// </summary>
    /// <param name="card">The card.</param>
    /// <param name="uri">The URI.</param>
    /// <returns></returns
    TvMediaPlayer Get(VirtualCard card, string fileName);

    /// <summary>
    /// Releases the specified player.
    /// </summary>
    /// <param name="player">The player.</param>
    void Release(TvMediaPlayer player);

    /// <summary>
    /// Gets the <see cref="MyTv.TvMediaPlayer"/> at the specified index.
    /// </summary>
    /// <value></value>
    TvMediaPlayer this[int index] { get;}

    /// <summary>
    /// Gets the number of players active.
    /// </summary>
    /// <value>The number of active players.</value>
    int Count { get;}

    /// <summary>
    /// Disposes all players
    /// </summary>
    void DisposeAll();
  }
}
