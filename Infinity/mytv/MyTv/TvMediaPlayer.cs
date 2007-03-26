using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TvControl;
namespace MyTv
{
  public class TvMediaPlayer : MediaPlayer
  {
    VirtualCard _card;
    public TvMediaPlayer(VirtualCard card)
    {
      _card = card;
    }

    public void Dispose()
    {
      base.Stop();
      base.Close();
      if (_card!=null)
      {
        if (_card.IsTimeShifting)
        {
          _card.StopTimeShifting();
        }
      }
      TvPlayerCollection.Instance.Release(this);
    }
  }
}
