using System.Collections.Generic;
using TvControl;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB;
using TvLibrary.Interfaces;
using TvService;
using TypeMock.ArrangeActAssert;

namespace TVServiceTests.Mocks.DVBSCard
{
  public class DVBCCardMocks : DVBCardMocks
  {
    public DVBCCardMocks() : base ()
    {      
    }

    public DVBCCardMocks(List<IChannel> tuningDetails, IUser user)
      : base(tuningDetails, user)
    {    
    }

    protected override bool IsSameDVBCardType(IChannel channel)
    {
      return (channel is DVBCChannel);      
    }

    protected override CardType GetCardType()
    {
      return CardType.DvbC;
    }

    /*
    #region public methods

    public static ITvCardHandler AddIdleFTADVBSCard(IChannel tuningDetail1, User user)
    {
      bool isDVBS = (tuningDetail1 is DVBSChannel);

      ITvCardHandler cardHandler1 = AddIdleCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(0);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBS);

      return cardHandler1;
    }

    public static ITvCardHandler AddAbsentFTADVBSCard(IChannel tuningDetail1, User user)
    {
      bool isDVBS = (tuningDetail1 is DVBSChannel);

      ITvCardHandler cardHandler1 = AddAbsentCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(0);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBS);

      return cardHandler1;
    }

    public static ITvCardHandler AddDisabledFTADVBSCard(IChannel tuningDetail1, User user)
    {
      bool isDVBS = (tuningDetail1 is DVBSChannel);

      ITvCardHandler cardHandler1 = AddDisabledCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(0);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBS);

      return cardHandler1;
    }



    public static ITvCardHandler AddLockedFTADVBSCard(IChannel tuningDetail1, User user)
    {
      bool isDVBS = (tuningDetail1 is DVBSChannel);

      ITvCardHandler cardHandler1 = AddLockedCard(tuningDetail1, user);

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(0);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(isDVBS);

      return cardHandler1;
    }



    #endregion
     */
  }
}
