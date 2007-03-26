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
    #region variables
    VirtualCard _card;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvMediaPlayer"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public TvMediaPlayer(VirtualCard card)
    {
      _card = card;
    }
    #endregion

    #region IDisposable
    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
      base.Stop();
      base.Close();
      if (_card != null)
      {
        if (_card.IsTimeShifting)
        {
          _card.StopTimeShifting();
        }
      }
      TvPlayerCollection.Instance.Release(this);
    }
    #endregion
  }
}
