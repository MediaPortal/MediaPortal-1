using System;
using System.Collections.Generic;
using System.Text;
using TvControl;

namespace MyTv
{
  class TvPlayerCollection
  {
    #region variables
    static TvPlayerCollection _instance = null;
    List<TvMediaPlayer> _players = new List<TvMediaPlayer>();
    #endregion

    #region static ctor
    /// <summary>
    /// Gets the TvPlayerCollection instance.
    /// </summary>
    /// <value>The TvPlayerCollection instance.</value>
    public static TvPlayerCollection Instance
    {
      get
      {
        if (_instance == null)
          _instance = new TvPlayerCollection();
        return _instance;
      }
    }
    #endregion

    #region public methods
    /// <summary>
    /// creates and returns a new media player
    /// </summary>
    /// <param name="card">The card.</param>
    /// <param name="uri">The URI.</param>
    /// <returns></returns>
    public TvMediaPlayer Get(VirtualCard card, Uri uri)
    {
      TvMediaPlayer player = new TvMediaPlayer(card);
      player.Open(uri);
      _players.Add(player);
      return player;
    }


    /// <summary>
    /// Releases the specified player.
    /// </summary>
    /// <param name="player">The player.</param>
    public void Release(TvMediaPlayer player)
    {
      for (int i = 0; i < _players.Count; ++i)
      {
        if (player == _players[i])
        {
          _players.RemoveAt(i);
          return;
        }
      }
    }

    /// <summary>
    /// Gets the <see cref="MyTv.TvMediaPlayer"/> at the specified index.
    /// </summary>
    /// <value></value>
    public TvMediaPlayer this[int index]
    {
      get
      {
        return _players[index];
      }
    }

    /// <summary>
    /// Gets the number of players active.
    /// </summary>
    /// <value>The number of active players.</value>
    public int Count
    {
      get
      {
        return _players.Count;
      }
    }
    #endregion
  }
}
