namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardHandler
{
  public class OwnerSubChannel
  {
    private readonly int _ownerSubchannelId = -1;
    private string _ownerName = "";

    public OwnerSubChannel(int subChannelId, string name)
    {
      _ownerSubchannelId = subChannelId;
      _ownerName = name;
    }

    public int OwnerSubChannelId
    {
      get { return _ownerSubchannelId; }
    }

    public string OwnerName
    {
      get { return _ownerName; }
      set { _ownerName = value; }
    }
  }
}