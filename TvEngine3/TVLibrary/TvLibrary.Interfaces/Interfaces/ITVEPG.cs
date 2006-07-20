using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Epg;

namespace TvLibrary.Interfaces
{
  #region delegates
  public delegate void EpgReceivedHandler(object sender, List<EpgChannel> epg);
  #endregion

  public interface ITVEPG
  {
    #region events
    event EpgReceivedHandler OnEpgReceived;
    #endregion

    /// <summary>
    /// Starts the EPG grabber.
    /// When the epg has been received the OnEpgReceived event will be fired
    /// </summary>
    void GrabEpg();

  }
}
