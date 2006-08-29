using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;
using TvLibrary.Epg;

namespace TvLibrary.Implementations.DVB
{
  public class DvbSs2EpgGrabber : ITVEPG
  {

    #region events
    public event EpgReceivedHandler OnEpgReceived;
    #endregion

    #region variables
    TvCardDvbSS2 _card;
    #endregion

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="card">The card.</param>
    public DvbSs2EpgGrabber(TvCardDvbSS2 card)
    {
      _card = (TvCardDvbSS2)card;
    }

    /// <summary>
    /// Starts the EPG grabber.
    /// When the epg has been received the OnEpgReceived event will be fired
    /// </summary>
    public void GrabEpg()
    {
      _card.OnEpgReceived += new TvCardDvbSS2.EpgProcessedHandler(_card_OnEpgReceived);
      _card.GrabEpg();
    }

    /// <summary>
    /// _card_s the on epg received.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="epg">The epg.</param>
    void _card_OnEpgReceived(object sender, List<EpgChannel> epg)
    {
      _card.OnEpgReceived -= new TvCardDvbSS2.EpgProcessedHandler(_card_OnEpgReceived);
      if (OnEpgReceived != null)
      {
        OnEpgReceived(this, epg);
      }
    }
  }
}
