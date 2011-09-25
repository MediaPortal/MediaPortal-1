using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TvControl;
using TvDatabase;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB;
using TvLibrary.Interfaces;
using TvService;
using TypeMock.ArrangeActAssert;

namespace TVServiceTests.Mocks
{
  /*
  public class CardMocks
  {

    public static ITvCardHandler IsTunedToSameTransponder(ITvCardHandler cardHandler1, IChannel tuningDetail1)
    {
      Isolate.WhenCalled(() => cardHandler1.Tuner.IsTunedToTransponder(tuningDetail1)).WillReturn(true);
      return cardHandler1;
    }

    public static ITvCardHandler IsNotTunedToSameTransponder(ITvCardHandler cardHandler1, IChannel tuningDetail1)
    {
      Isolate.WhenCalled(() => cardHandler1.Tuner.IsTunedToTransponder(tuningDetail1)).WillReturn(false);
      return cardHandler1;
    }

    #region private methods

    protected static ITvCardHandler AddIdleCard(IChannel tuningDetail1, User user)
    {
      ITvCardHandler cardHandler1 = Isolate.Fake.Instance<ITvCardHandler>();
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.IdCard).WillReturn(1);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.Enabled).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.ReferencedServer().HostName).WillReturn("testHostname");

      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.DevicePath).WillReturn("devicePath");
      Isolate.WhenCalled(() => cardHandler1.Tuner.IsTunedToTransponder(tuningDetail1)).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Users.IsOwner(user)).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.Users.GetUsers()).WillReturn(null);
      Isolate.WhenCalled(() => RemoteControl.Instance.CardPresent(1)).WillReturn(true);
      return cardHandler1;
    }

    protected static ITvCardHandler AddAbsentCard(IChannel tuningDetail1, User user)
    {
      ITvCardHandler cardHandler1 = Isolate.Fake.Instance<ITvCardHandler>();

      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.IdCard).WillReturn(1);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.Enabled).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.ReferencedServer().HostName).WillReturn("testHostname");
      //Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.DevicePath).WillReturn("devicePath");
      Isolate.WhenCalled(() => cardHandler1.Tuner.IsTunedToTransponder(tuningDetail1)).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Users.IsOwner(user)).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.Users.GetUsers()).WillReturn(null);
      Isolate.WhenCalled(() => RemoteControl.Instance.CardPresent(1)).WillReturn(false);
      return cardHandler1;
    }
    protected static ITvCardHandler AddDisabledCard(IChannel tuningDetail1, User user)
    {
      ITvCardHandler cardHandler1 = Isolate.Fake.Instance<ITvCardHandler>();
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.IdCard).WillReturn(1);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.Enabled).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.ReferencedServer().HostName).WillReturn("testHostname");
      //Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.DevicePath).WillReturn("devicePath");
      Isolate.WhenCalled(() => cardHandler1.Tuner.IsTunedToTransponder(tuningDetail1)).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Users.IsOwner(user)).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.Users.GetUsers()).WillReturn(null);
      Isolate.WhenCalled(() => RemoteControl.Instance.CardPresent(1)).WillReturn(true);
      return cardHandler1;
    }

    protected static ITvCardHandler AddLockedCard(IChannel tuningDetail1, User user)
    {
      ITvCardHandler cardHandler1 = Isolate.Fake.Instance<ITvCardHandler>();
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.IdCard).WillReturn(1);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.Enabled).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.ReferencedServer().HostName).WillReturn("testHostname");
      //Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(tuningDetail1)).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.DevicePath).WillReturn("devicePath");
      Isolate.WhenCalled(() => cardHandler1.Tuner.IsTunedToTransponder(tuningDetail1)).WillReturn(false);
      Isolate.WhenCalled(() => cardHandler1.Users.IsOwner(user)).WillReturn(false);
      Isolate.WhenCalled(() => RemoteControl.Instance.CardPresent(1)).WillReturn(true);

      User[] users = new User[1];
      users[0] = new User("test user", false, 1);

      Isolate.WhenCalled(() => cardHandler1.Users.GetUsers()).WillReturn(users);
      return cardHandler1;
    }
    #endregion
    

   
  }*/
}
