using System;

namespace WindowsApplication13
{
  internal class DataBroadcastIdDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private ushort _dataBroadcastId;

    #endregion
    #region Constructor
    public DataBroadcastIdDescriptor()
    {
    }
    #endregion
    #region Properties
    public ushort DataBroadcastId { get { return _dataBroadcastId; } set { _dataBroadcastId = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Data Broadcast Id Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }   

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        _dataBroadcastId = (ushort)((buffer[_lastIndex] << 8) + buffer[_lastIndex++]);
        _lastIndex = DescriptorLength;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Data Broadcast Id Descriptor message is short"));
      }
    }
    #endregion
  }
}