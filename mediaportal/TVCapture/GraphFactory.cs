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

    /// <summary>
    /// Creates a new object which supports the specified TVCapture card and implements
    /// the timeshifting/viewing/recording logic for this card
    /// </summary>
    /// <param name="card">Tvcapture card which must be supported by the newly created graphbuilder</param>
    /// <returns>Object which can create a DirectShow graph for this card or null if TVCapture card is not supported</returns>
    /// <seealso>MediaPortal.TV.Recording.TVCaptureDevice</seealso>
    static public IGraph CreateGraph(TVCaptureDevice card)
    {
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
      return new SWEncodingGraph(card.ID,iTunerCountry,bCable, card.VideoDevice,card.AudioDevice,card.VideoCompressor,card.AudioCompressor, card.FrameSize,card.FrameRate, card.AudioInputPin, card.RecordingLevel);
      
    }
	}
}
