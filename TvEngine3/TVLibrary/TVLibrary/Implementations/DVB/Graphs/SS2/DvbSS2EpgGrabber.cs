/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
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
