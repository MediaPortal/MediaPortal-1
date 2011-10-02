using System.Collections.Generic;
using TvControl;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvService;
using TypeMock.ArrangeActAssert;

namespace TVServiceTests.Mocks.DVBTCard
{
  public class DVBTCardMocks : DVBCardMocks
  {

    public DVBTCardMocks() : base ()
    {      
    }

    public DVBTCardMocks(List<IChannel> tuningDetails, IUser user)
      : base(tuningDetails, user)
    {    
    }

    protected override bool IsSameDVBCardType(IChannel channel)
    {
      return (channel is DVBTChannel);      
    }

    protected override CardType GetCardType()
    {
      return CardType.DvbT;
    }

    #region public methods

    /*
    public static ITvCardHandler AddIdleFTADVBTCard(IChannel tuningDetail1, User user)
    {
      bool isDVBT = (tuningDetail1 is DVBTChannel);

      ITvCardHandler cardHandler1 = AddIdleCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(0);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBT);

      return cardHandler1;
    }

    
    public static ITvCardHandler AddAbsentFTADVBTCard(IChannel tuningDetail1, User user)
    {
      bool isDVBT = (tuningDetail1 is DVBTChannel);

      ITvCardHandler cardHandler1 = AddAbsentCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(0);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBT);

      return cardHandler1;
    }

    public static ITvCardHandler AddDisabledFTADVBTCard(IChannel tuningDetail1, User user)
    {
      bool isDVBT = (tuningDetail1 is DVBTChannel);

      ITvCardHandler cardHandler1 = AddDisabledCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(0);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBT);

      return cardHandler1;
    }



    public static ITvCardHandler AddLockedFTADVBTCard(IChannel tuningDetail1, User user)
    {
      bool isDVBT = (tuningDetail1 is DVBTChannel);

      ITvCardHandler cardHandler1 = AddLockedCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(0);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBT);

      return cardHandler1;
    }
    */

    #endregion
  }
}
