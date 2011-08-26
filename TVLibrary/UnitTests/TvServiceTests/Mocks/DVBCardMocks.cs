using System.Collections.Generic;
using TvControl;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvService;
using TypeMock.ArrangeActAssert;

namespace TVServiceTests.Mocks
{
  public abstract class DVBCardMocks : IDVBCardMocks
  {
    protected bool _idle = true;
    protected bool _enabled = true;
    protected bool _present = true;
    protected bool _hasCAM = false;
    protected IUser _user;
    protected int _numberOfChannelsDecrypting = 0;
    protected int _cardId = 0;
    protected int _decryptLimit = 0;
    protected int _priority = 0;
    protected bool _isTunedToTransponder = false;
    protected bool _isOwner = true;
    protected int _numberOfUsersOnCard = 0;
    protected List<IChannel> _tuningDetails = new List<IChannel>();

    public DVBCardMocks()
    {
      _cardId = CardManager.GetNextAvailCardId();
    }

    public DVBCardMocks(List<IChannel> tuningDetails, IUser user)
    {
      _cardId = CardManager.GetNextAvailCardId();
      _tuningDetails = tuningDetails;
      _user = user;

    }

    public bool Idle
    {
      get { return _idle; }
      set 
      {
        if (value)
        {
          _numberOfUsersOnCard = 0;
        }
        else
        {
          if (_numberOfUsersOnCard == 0)
          {
            _numberOfUsersOnCard = 1;
          }
        }        
        _idle = value; 
      }
    }

    public bool HasCAM
    {
      get { return _hasCAM; }
      set { _hasCAM = value; }
    }

    public bool Enabled
    {
      get { return _enabled; }
      set { _enabled = value; }
    }

    public bool Present
    {
      get { return _present; }
      set { _present = value; }
    }

    public int NumberOfChannelsDecrypting
    {
      get { return _numberOfChannelsDecrypting; }
      set { _numberOfChannelsDecrypting = value; }
    }

    public bool IsTunedToTransponder
    {
      get { return _isTunedToTransponder; }
      set { _isTunedToTransponder = value; }
    }

    public bool IsOwner
    {
      get { return _isOwner; }
      set { _isOwner = value; }
    }

    public int NumberOfUsersOnCard
    {
      get { return _numberOfUsersOnCard; }
      set { _numberOfUsersOnCard = value; }
    }

    public int Priority
    {
      get { return _priority; }
      set { _priority = value; }
    }

    public int CardId
    {
      get { return _cardId; }
      set { _cardId = value; }
    }

    public int DecryptLimit
    {
      get { return _decryptLimit; }
      set { _decryptLimit = value; }
    }

    public ITvCardHandler GetMockedCardHandler ()
    {
      ITvCardHandler cardHandler1 = Isolate.Fake.Instance<ITvCardHandler>();


      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.IdCard).WillReturn(_cardId);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.Enabled).WillReturn(_enabled);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.ReferencedServer().HostName).WillReturn("testHostname");
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.DevicePath).WillReturn("devicePath");
      Isolate.WhenCalled(() => cardHandler1.SupportsSubChannels).WillReturn(true);
      Isolate.WhenCalled(() => cardHandler1.Users.IsOwner(_user)).WillReturn(_isOwner);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.DecryptLimit).WillReturn(_decryptLimit);      

      if (_numberOfUsersOnCard == 0)
      {
        Isolate.WhenCalled(() => cardHandler1.Users.GetUsers()).WillReturn(null);
      }
      else
      {
        List<User> users = new List<User>();
        for (int i = 0; i < _numberOfUsersOnCard; i++)
        {
          User otherUser = new User("other_user" + i, false);
          users.Add(otherUser);
        }
        Isolate.WhenCalled(() => cardHandler1.Users.GetUsers()).WillReturn(users.ToArray());
      }

      Isolate.WhenCalled(() => RemoteControl.Instance.CardPresent(_cardId)).WillReturn(_present);
      

      Isolate.WhenCalled(() => cardHandler1.NumberOfChannelsDecrypting).WillReturn(_numberOfChannelsDecrypting);
      Isolate.WhenCalled(() => cardHandler1.DataBaseCard.CAM).WillReturn(_hasCAM);

      foreach (IChannel channel in _tuningDetails)
      {
        bool isSameDVBCardType = IsSameDVBCardType(channel);
        Isolate.WhenCalled(() => cardHandler1.Tuner.IsTunedToTransponder(channel)).WillReturn(_isTunedToTransponder);
        Isolate.WhenCalled(() => cardHandler1.Tuner.CanTune(channel)).WillReturn(isSameDVBCardType);        
      }

      Isolate.WhenCalled(() => cardHandler1.Type).WillReturn(GetCardType());

      return cardHandler1;
    }

    protected abstract bool IsSameDVBCardType(IChannel channel);

    protected abstract CardType GetCardType();
  }
}
