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
    Exception _exception;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvMediaPlayer"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public TvMediaPlayer(VirtualCard card)
    {
      _card = card;
      _exception = null;
      MediaFailed += new EventHandler<ExceptionEventArgs>(TvMediaPlayer_MediaFailed);
    }

    void TvMediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
    {
      _exception = e.ErrorException;
    }
    #endregion
    public string ErrorMessage
    {
      get
      {
        if (_exception == null) return "";
        return _exception.Message;
      }
    }
    public bool HasError
    {
      get
      {
        return (_exception != null);
      }
    }

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
