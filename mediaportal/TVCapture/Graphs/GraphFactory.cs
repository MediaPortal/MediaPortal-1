#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#endregion

using MediaPortal.TV.Scanning;
using TVCapture;

namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Singleton class implementing a factory which can be used for creating new
  /// instances of IGraph for a particular TVCapture card
  /// <seealso cref="MediaPortal.TV.Recording.IGraph"/>
  /// </summary>
  public class GraphFactory
  {
    private GraphFactory()
    {
    }

    public static ITuning CreateTuning(TVCaptureDevice card)
    {
      if (!card.CreateGraph())
      {
        return null;
      }
      if (card.Network == NetworkType.Analog)
      {
        return new AnalogTVTuning();
      }
      if (card.Network == NetworkType.DVBT)
      {
        return new DVBTTuning();
      }
      if (card.Network == NetworkType.DVBS)
      {
        return new DVBSTuning();
      }
      if (card.Network == NetworkType.DVBC)
      {
        return new DVBCTuning();
      }
      if (card.Network == NetworkType.ATSC)
      {
        return new ATSCTuning();
      }
      return null;
    }

    /// <summary>
    /// Creates a new object which supports the specified TVCapture card and implements
    /// the timeshifting/viewing/recording logic for this card
    /// </summary>
    /// <param name="card">Tvcapture card which must be supported by the newly created graphbuilder</param>
    /// <returns>Object which can create a DirectShow graph for this card or null if TVCapture card is not supported</returns>
    /// <seealso>MediaPortal.TV.Recording.TVCaptureDevice</seealso>
    public static IGraph CreateGraph(TVCaptureDevice card)
    {
      if (card.CardType == CardTypes.Digital_BDA)
      {
        return new DVBGraphBDA(card);
      }

      if (card.CardType == CardTypes.Digital_SS2)
      {
        return new DVBGraphSkyStar2(card);
        //return new DVBGraphSS2(card.ID);
      }
      /*
      if (card.CardType == TVCapture.CardTypes.Digital_TTPremium)
      {
        return new DVBGraphTTPremium(card);
      }
      */
      if (card.CardType == CardTypes.Analog)
      {
        return new SinkGraphEx(card);
      }
      if (card.CardType == CardTypes.Analog_MCE)
      {
        return new SinkGraphEx(card);
      }

      return new DummyGraph(card);
    }
  }
}