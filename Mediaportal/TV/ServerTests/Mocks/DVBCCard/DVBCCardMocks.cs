using System;
using System.Collections.Generic;
using TvControl;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvService;
using TypeMock.ArrangeActAssert;

namespace TVServiceTests.Mocks.DVBCCard
{
 public class DVBSCardMocks : DVBCardMocks
  {
    public DVBSCardMocks() : base ()
    {      
    }

    public DVBSCardMocks(List<IChannel> tuningDetails, IUser user)
      : base(tuningDetails, user)
    {    
    }

    protected override bool IsSameDVBCardType(IChannel channel)
    {
      return (channel is DVBSChannel);      
    }

   protected override CardType GetCardType()
   {
     return CardType.DvbS;
   }

   /*
    #region public methods - FTA cards

    public static ITvCardHandler AddIdleFTADVBCCard(IChannel tuningDetail1, User user)
    {
      bool isDVBC = (tuningDetail1 is DVBCChannel);

      ITvCardHandler cardHandler1 = AddIdleCard(tuningDetail1, user);
      
      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(0);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBC);

      return cardHandler1;
    }

    public static ITvCardHandler AddAbsentFTADVBCCard(IChannel tuningDetail1, User user)
    {
      bool isDVBC = (tuningDetail1 is DVBCChannel);

      ITvCardHandler cardHandler1 = AddAbsentCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(0);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBC);

      return cardHandler1;
    }

    public static ITvCardHandler AddDisabledFTADVBCCard(IChannel tuningDetail1, User user)
    {
      bool isDVBC = (tuningDetail1 is DVBCChannel);

      ITvCardHandler cardHandler1 = AddDisabledCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(0);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBC);

      return cardHandler1;
    }



    public static ITvCardHandler AddLockedFTADVBCCard(IChannel tuningDetail1, User user)
    {
      bool isDVBC = (tuningDetail1 is DVBCChannel);

      ITvCardHandler cardHandler1 = AddLockedCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(0);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBC);

      return cardHandler1;
    }

    #endregion

    #region public methods - CAM enabled cards

    public static ITvCardHandler AddIdleCAMDVBCCard(IChannel tuningDetail1, User user, int camLimit, int numberOfChannelsDecrypting)
    {
      bool isDVBC = (tuningDetail1 is DVBCChannel);

      ITvCardHandler cardHandler1 = AddIdleCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(numberOfChannelsDecrypting);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBC);

      return cardHandler1;
    }

    public static ITvCardHandler AddAbsentCAMDVBCCard(IChannel tuningDetail1, User user, int camLimit, int numberOfChannelsDecrypting)
    {
      bool isDVBC = (tuningDetail1 is DVBCChannel);

      ITvCardHandler cardHandler1 = AddAbsentCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(numberOfChannelsDecrypting);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBC);

      return cardHandler1;
    }

    public static ITvCardHandler AddDisabledCAMDVBCCard(IChannel tuningDetail1, User user, int camLimit, int numberOfChannelsDecrypting)
    {
      bool isDVBC = (tuningDetail1 is DVBCChannel);

      ITvCardHandler cardHandler1 = AddDisabledCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(numberOfChannelsDecrypting);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBC);

      return cardHandler1;
    }



    public static ITvCardHandler AddLockedCAMDVBCCard(IChannel tuningDetail1, User user, int camLimit, int numberOfChannelsDecrypting)
    {
      bool isDVBC = (tuningDetail1 is DVBCChannel);

      ITvCardHandler cardHandler1 = AddLockedCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(numberOfChannelsDecrypting);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBC);

      return cardHandler1;
    }

    #endregion
    */
  }
}
