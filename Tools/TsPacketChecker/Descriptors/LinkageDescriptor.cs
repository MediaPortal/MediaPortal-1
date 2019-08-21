using System;

namespace TsPacketChecker
{
  internal class LinkageDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private ushort _transportStreamId;
    private ushort _originalNetworkId;
    private ushort _serviceId;
    private byte _linkageType;

    #endregion
    #region Constructor
    public LinkageDescriptor()
    {
    }
    #endregion
    #region Properties
    public ushort TransportStreamId { get { return _transportStreamId; } set { _transportStreamId = value; } }
    public ushort OriginalNetworkId { get { return _originalNetworkId; } set { _originalNetworkId = value; } }
    public ushort ServiceId { get { return _serviceId; } set { _serviceId = value; }}
    public byte LinkageType { get { return _linkageType; } set { _linkageType = value; }}
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Linkage Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }    

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        _transportStreamId = (ushort)((buffer[_lastIndex] << 8) + buffer[_lastIndex++]);
        _originalNetworkId = (ushort)((buffer[_lastIndex] << 8) + buffer[_lastIndex++]);
        _serviceId = (ushort)((buffer[_lastIndex] << 8) + buffer[_lastIndex++]);
        _linkageType = buffer[_lastIndex];
        _lastIndex = DescriptorLength;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Linkage Descriptor message is short"));
      }
    }

    
    #endregion
  }
}