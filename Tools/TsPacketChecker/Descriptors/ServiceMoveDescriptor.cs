using System;

namespace TsPacketChecker
{
  internal class ServiceMoveDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private int _newOriginalNetworkId;
    private int _newTransportStreamId;
    private int _newServiceId;
    #endregion
    #region Constructor
    public ServiceMoveDescriptor()
    {
    }
    #endregion
    #region Properties
    public int NewOriginalNetworkId { get { return _newOriginalNetworkId; } set { _newOriginalNetworkId = value; } }
    public int NewTransportStreamId { get { return _newTransportStreamId; } set { _newTransportStreamId = value; } }
    public int NewServiceId { get { return _newServiceId; } set { _newServiceId = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Service Move Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }    

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        _newOriginalNetworkId = (int)((buffer[_lastIndex]) + (buffer[_lastIndex++]));
        _newTransportStreamId = (int)((buffer[_lastIndex]) + (buffer[_lastIndex++]));
        _newServiceId = (int)((buffer[_lastIndex]) + (buffer[_lastIndex++]));
        _lastIndex = index + DescriptorLength;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Service Move Descriptor message is short"));
      }
    }
    #endregion
  }
}