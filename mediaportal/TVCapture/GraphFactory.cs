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
#if (!UseCaptureCardDefinitions)
			return new AnalogTVTuning();
#else
			if (!card.CreateGraph()) return null;
			if (card.Network == NetworkType.ATSC) return new AnalogTVTuning();
			if (card.Network == NetworkType.DVBT) return new DVBTTuning();
#endif
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
#if (!UseCaptureCardDefinitions)
      int iTunerCountry=31;
      string strTunerType="Antenna";
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        strTunerType=xmlreader.GetValueAsString("capture","tuner","Antenna");
        iTunerCountry=xmlreader.GetValueAsInt("capture","country",31);
      }
      bool bCable=false;
      if (!strTunerType.Equals("Antenna")) bCable=true;


      if (card.SupportsMPEG2)
      {
        if (card.IsMCECard)
          return new MCESinkGraph(card.ID,iTunerCountry,bCable, card.VideoDevice, card.FrameSize,card.FrameRate);
        return new SinkGraph(card.ID,iTunerCountry,bCable, card.VideoDevice, card.FrameSize,card.FrameRate);
      }
	    if (card.ToString() == "ATI Rage Theater Video Capture")
	    {
		  return new AIWGraph(iTunerCountry,bCable,card.VideoDevice,card.AudioDevice,card.VideoCompressor,card.AudioCompressor,card.FrameSize,card.FrameRate,card.AudioInputPin,card.RecordingLevel);
	    }
		  if (card.ToString() == "B2C2 MPEG-2 Source")
		  {
			  return new DVBGraphSS2(iTunerCountry,bCable,card.VideoDevice,card.AudioDevice,card.VideoCompressor,card.AudioCompressor,card.FrameSize,card.FrameRate,card.AudioInputPin,card.RecordingLevel);
		  }
      return new SWEncodingGraph(card.ID,iTunerCountry,bCable, card.VideoDevice,card.AudioDevice,card.VideoCompressor,card.AudioCompressor, card.FrameSize,card.FrameRate, card.AudioInputPin, card.RecordingLevel);
    }
#else
      int    countryCode = 31;
      string tunerInput  = "Antenna";
      using (AMS.Profile.Xml xmlReader = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        tunerInput  = xmlReader.GetValueAsString("capture", "tuner", "Antenna");
        countryCode = xmlReader.GetValueAsInt("capture", "country", 31);
      }

      bool isCableInput = false;
      if (!tunerInput.Equals("Antenna")) isCableInput = true;

      // #MW#
      // Added properties to card...
      // Could be read using serialization...
      card.IsCableInput = isCableInput;
      card.CountryCode  = countryCode;

      // There are a few types of cards:
      //	-	Software based cards, ie no hardware MPEG2 encoder
      //	- Hardware MPEG2 encoders
      //	- Hardware MPEG2 "MCE" compatible encoders which for instance always include Radio...
			if(card.IsBDACard)
				return new DVBGraphBDA(card);
			
			if (card.DeviceType.ToLower()=="hw") return new SinkGraph(card);
			if (card.DeviceType.ToLower()=="mce") return new MCESinkGraph(card.ID,card.CountryCode,card.IsCableInput,card.VideoDevice,card.FrameSize,card.FrameRate);
			if (card.DeviceType.ToLower()=="s/w") return new SWEncodingGraph(card.ID,card.CountryCode,card.IsCableInput,card.VideoDevice,card.AudioDevice,card.VideoCompressor,card.AudioCompressor,card.FrameSize,card.FrameRate,card.AudioInputPin,card.RecordingLevel);
      if (card.SupportsMPEG2)
      {
        // #MW#
        // Use a single call for all MPEG2 cards, also the MCE versions...
        // NOT tested of course, since I have only MCE versions!
        // The extra code (3 lines??) found in the MPEG2 SinkGraph is now included in here.
        return new SinkGraphEx(card);
      }

      // Special graph building for the ATI AIW cards
      if (card.ToString() == "ATI Rage Theater Video Capture")
      {
        return new AIWGraph(countryCode,isCableInput,card.VideoDevice,card.AudioDevice,card.VideoCompressor,card.AudioCompressor,card.FrameSize,card.FrameRate,card.AudioInputPin,card.RecordingLevel);
      }

      // Standard call for all other software based cards.
      return new SWEncodingGraph(card.ID,countryCode,isCableInput, card.VideoDevice,card.AudioDevice,card.VideoCompressor,card.AudioCompressor, card.FrameSize,card.FrameRate, card.AudioInputPin, card.RecordingLevel);
    }
#endif
	}
}

