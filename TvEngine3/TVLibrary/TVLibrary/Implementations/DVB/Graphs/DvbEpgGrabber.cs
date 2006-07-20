using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;
using TvLibrary.Epg;

namespace TvLibrary.Implementations.DVB
{
  public class DvbEpgGrabber : ITVEPG
  {

    #region events
    public event EpgReceivedHandler OnEpgReceived;
    #endregion

    #region variables
    TvCardDvbBase _card;
    TvCardDvbBase.EpgProcessedHandler _handler;
    #endregion

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="card"></param>
    public DvbEpgGrabber(TvCardDvbBase card)
    {
      _card = (TvCardDvbBase)card;
      _handler = new TvCardDvbBase.EpgProcessedHandler(_card_OnEpgReceived);
    }

    /// <summary>
    /// Starts the EPG grabber.
    /// When the epg has been received the OnEpgReceived event will be fired
    /// </summary>
    public void GrabEpg()
    {
      Log.Log.Write("dvbepggrab:GrabEpg");
      _card.OnEpgReceived += _handler;
      _card.GrabEpg();
    }

    void _card_OnEpgReceived(object sender, List<EpgChannel> epg)
    {
      Log.Log.Write("dvbepggrab:OnEpgReceived");
      _card.OnEpgReceived -= _handler;
      if (OnEpgReceived != null)
      {
        OnEpgReceived(this, epg);
      }
    }
  }
}
