using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Presentation
{ 
  [DataContract]
  public class CardPresentation
  {
    [DataMember] 
    private string _isOwner;

    [DataMember] 
    private bool _idle;
    
    [DataMember] 
    private string _isScrambled;

    [DataMember]     
    private readonly string _cardType = "unknown";
    
    [DataMember] 
    private bool _subChannelsCountOk = true;

    [DataMember]
    private string _state = "";

    [DataMember]
    private string _channelName = "";

    [DataMember]
    private string _userName = "";    

    [DataMember]
    private int? _cardId;

    [DataMember]
    private string _cardName = "";

    [DataMember]
    private int _subChannels;

    public CardPresentation(string cardType, int cardId, string cardName)
    {
      _cardType = cardType;
      _cardId = cardId;
      _cardName = cardName;
    }

    public int? CardId
    {
      get { return _cardId; }
    }

    public string CardName
    {
      get { return _cardName; }
    }

    public string CardType
    {
      get { return _cardType; }
    }

    public bool SubChannelsCountOk
    {
      get { return _subChannelsCountOk; }
      set { _subChannelsCountOk = value; }
    }

    public string State
    {
      get { return _state; }
      set { _state = value; }
    }

    public string ChannelName
    {
      get { return _channelName; }
      set { _channelName = value; }
    }

    public string UserName
    {
      get { return _userName; }
      set { _userName = value; }
    }

    public bool Idle
    {
      get { return _idle; }
      set { _idle = value; }
    }

    public string IsScrambled
    {
      get { return _isScrambled; }
      set { _isScrambled = value; }
    }

    public string IsOwner
    {
      get { return _isOwner; }
      set { _isOwner = value; }
    }

    public int SubChannels
    {
      get { return _subChannels; }
      set { _subChannels = value; }
    }
  }
}
