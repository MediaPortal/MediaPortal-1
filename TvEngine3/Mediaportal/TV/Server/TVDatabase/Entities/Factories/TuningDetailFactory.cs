using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class TuningDetailFactory
  {

    public static TuningDetail CreateTuningDetail(int idChannel, string name, string provider, int channelType, int channelNumber, int frequency, int countryId, MediaTypeEnum mediaType, int networkId, int transportId, int serviceId, int pmtPid, bool freeToAir, int modulation, int polarisation, int symbolrate, int diseqc, int bandwidth, int majorChannel, int minorChannel, int videoSource, int audioSource, bool isVCRSignal, int tuningSource, int band, int satIndex, int innerFecRate, int pilot, int rollOff, string url, int bitrate)
    {
      var tuningDetail = new TuningDetail
      {
        IdChannel = idChannel,
        Name= name,
        Provider = provider,
        ChannelType = channelType,
        ChannelNumber = channelNumber,
        Frequency= frequency,
        CountryId=countryId,
        MediaType = (int) mediaType,
        NetworkId= networkId, 
        TransportId= transportId, 
        ServiceId= serviceId,
        PmtPid=pmtPid , 
        FreeToAir= freeToAir, 
        Modulation= modulation, 
        Polarisation=polarisation , 
        Symbolrate= symbolrate, 
        DiSEqC= diseqc,         
        Bandwidth= bandwidth, 
        MajorChannel= majorChannel, 
        MinorChannel=minorChannel ,
        VideoSource= videoSource, 
        AudioSource=audioSource , 
        IsVCRSignal= isVCRSignal, 
        TuningSource= tuningSource,
        Band= band,
        SatIndex=satIndex , 
        InnerFecRate=innerFecRate , 
        Pilot= pilot, 
        RollOff=rollOff , 
        Url= url, 
        Bitrate= bitrate

      };
      return tuningDetail;
    }
    /**/
   


   
  }
}
