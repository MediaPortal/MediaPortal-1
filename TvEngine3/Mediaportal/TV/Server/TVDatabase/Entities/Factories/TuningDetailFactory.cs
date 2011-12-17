using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class TuningDetailFactory
  {

    public static TuningDetail CreateTuningDetail(int idChannel, string name, string provider, int channelType, int channelNumber, int frequency,
                      int countryId, MediaTypeEnum mediaType, int networkId, int transportId, int serviceId,
                      int pmtPid, bool freeToAir, int modulation, int polarisation, int symbolrate, int diseqc,
                      int switchingFrequency, int bandwidth, int majorChannel, int minorChannel,
                      int videoSource, int audioSource, bool isVCRSignal, int tuningSource,
                      int band,
                      int satIndex, int innerFecRate, int pilot, int rollOff, string url, int bitrate)
    {
      var tuningDetail = new TuningDetail
      {
        idChannel = idChannel,
        name= name,
        provider = provider,
        channelType = channelType,
        channelNumber = channelNumber,
        frequency= frequency,
        countryId=countryId,
        mediaType = (int) mediaType,
        networkId= networkId, 
        transportId= transportId, 
        serviceId= serviceId,
        pmtPid=pmtPid , 
        freeToAir= freeToAir, 
        modulation= modulation, 
        polarisation=polarisation , 
        symbolrate= symbolrate, 
        diseqc= diseqc,
        switchingFrequency = switchingFrequency, 
        bandwidth= bandwidth, 
        majorChannel= majorChannel, 
        minorChannel=minorChannel ,
        videoSource= videoSource, 
        audioSource=audioSource , 
        isVCRSignal= isVCRSignal, 
        tuningSource= tuningSource,
        band= band,
        satIndex=satIndex , 
        innerFecRate=innerFecRate , 
        pilot= pilot, 
        rollOff=rollOff , 
        url= url, 
        bitrate= bitrate

      };
      return tuningDetail;
    }
    /**/
   


   
  }
}
