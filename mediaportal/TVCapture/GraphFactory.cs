using System;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// 
	/// </summary>
	public class GraphFactory
	{
		private GraphFactory()
		{
		}

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
        return new SinkGraph(iTunerCountry,bCable, card.VideoDevice);
      }
      else
      {
        //return new DScalerGraph(...)
      }
      return null;
    }
	}
}
