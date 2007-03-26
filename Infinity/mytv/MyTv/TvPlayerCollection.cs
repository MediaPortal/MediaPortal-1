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
      player.MediaFailed += new EventHandler<System.Windows.Media.ExceptionEventArgs>(player_MediaFailed);
      player.Open(uri);
      _players.Add(player);
      return player;
    }

    /// <summary>
    /// Handles the MediaFailed event of the player control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Media.ExceptionEventArgs"/> instance containing the event data.</param>
    void player_MediaFailed(object sender, System.Windows.Media.ExceptionEventArgs e)
    {
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
