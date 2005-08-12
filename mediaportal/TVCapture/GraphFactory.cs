/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
		
		static public ITuning CreateTuning(TVCaptureDevice card)
		{
			if (!card.CreateGraph()) return null;
			if (card.Network == NetworkType.Analog) return new AnalogTVTuning();
			if (card.Network == NetworkType.DVBT) return new DVBTTuning();
			if (card.Network == NetworkType.DVBS) return new DVBSTuning();
			if (card.Network == NetworkType.DVBC) return new DVBCTuning();
			if (card.Network == NetworkType.ATSC) return new ATSCTuning();
			return null;
		}

    /// <summary>
    /// Creates a new object which supports the specified TVCapture card and implements
    /// the timeshifting/viewing/recording logic for this card
    /// </summary>
    /// <param name="card">Tvcapture card which must be supported by the newly created graphbuilder</param>
    /// <returns>Object which can create a DirectShow graph for this card or null if TVCapture card is not supported</returns>
    /// <seealso>MediaPortal.TV.Recording.TVCaptureDevice</seealso>
    static public IGraph CreateGraph(TVCaptureDevice card)
    {
      int    countryCode = 31;
      string tunerInput  = "Antenna";
      using (MediaPortal.Profile.Xml xmlReader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        tunerInput  = xmlReader.GetValueAsString("capture", "tuner", "Antenna");
        countryCode = xmlReader.GetValueAsInt("capture", "country", 31);
      }

      bool isCableInput = false;
      if (!tunerInput.Equals("Antenna")) isCableInput = true;

      card.IsCableInput = isCableInput;
      card.DefaultCountryCode  = countryCode;

      if(card.IsBDACard)
				return new DVBGraphBDA(card);

			if (card.ToString() == "B2C2 MPEG-2 Source")
			{
				return new DVBGraphSS2(card.ID,countryCode,isCableInput,card.VideoDevice,card.AudioDevice,card.VideoCompressor,card.AudioCompressor,card.FrameSize,card.FrameRate,card.AudioInputPin,card.RecordingLevel);
			}

			// Special graph building for the ATI AIW cards
//			if (card.ToString() == "ATI Rage Theater Video Capture")
//			{
//				return new AIWGraph(countryCode,isCableInput,card.VideoDevice,card.AudioDevice,card.VideoCompressor,card.AudioCompressor,card.FrameSize,card.FrameRate,card.AudioInputPin,card.RecordingLevel,card.FriendlyName);
//			}
			
			if (card.DeviceType!=null)
			{
				if (card.DeviceType.ToLower()=="hw") return new SinkGraph(card);
				if (card.DeviceType.ToLower()=="mce") return new MCESinkGraph(card.ID,card.DefaultCountryCode,card.IsCableInput,card.VideoDevice,card.FrameSize,card.FrameRate,card.FriendlyName);
				if (card.DeviceType.ToLower()=="s/w") return new SWEncodingGraph(card.ID,card.DefaultCountryCode,card.IsCableInput,card.VideoDevice,card.AudioDevice,card.VideoCompressor,card.AudioCompressor,card.FrameSize,card.FrameRate,card.AudioInputPin,card.RecordingLevel,card.FriendlyName);
			}

			if (card.SupportsMPEG2)
      {
        return new SinkGraphEx(card);
      }

      // Standard call for all other software based cards.
      return new SWEncodingGraph(card.ID,countryCode,isCableInput, card.VideoDevice,card.AudioDevice,card.VideoCompressor,card.AudioCompressor, card.FrameSize,card.FrameRate, card.AudioInputPin, card.RecordingLevel,card.FriendlyName);
    }
	}
}

