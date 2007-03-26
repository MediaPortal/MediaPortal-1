using System;
using System.Collections.Generic;
using System.Text;
using TvControl;

namespace MyTv
{
  class TvPlayerCollection
  {
    static TvPlayerCollection _instance = null;
    List<TvMediaPlayer> _players = new List<TvMediaPlayer>();

    public static TvPlayerCollection Instance
    {
      get
      {
        if (_instance == null)
          _instance = new TvPlayerCollection();
        return _instance;
      }
    }

    public TvMediaPlayer Get(VirtualCard card, Uri uri)
    {
      TvMediaPlayer player = new TvMediaPlayer(card);
      player.MediaFailed += new EventHandler<System.Windows.Media.ExceptionEventArgs>(player_MediaFailed);
      player.Open(uri);
      _players.Add(player);
      return player;
    }

    void player_MediaFailed(object sender, System.Windows.Media.ExceptionEventArgs e)
    {
    }

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

    public TvMediaPlayer this[int index]
    {
      get
      {
        return _players[index];
      }
    }

    public int Count
    {
      get
      {
        return _players.Count;
      }
    }
  }
}
