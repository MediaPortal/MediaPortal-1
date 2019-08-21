using System;

namespace TsPacketChecker
{
  internal class AdaptationFieldDataDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private byte _adaptationFieldDataIdentifier;

    #endregion
    #region Constructor
    public AdaptationFieldDataDescriptor()
    {
    }
    #endregion
    #region Properties
    public byte AdaptationFieldDataIdentifier { get { return _adaptationFieldDataIdentifier; } set { _adaptationFieldDataIdentifier = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Adaptation Field Data Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }

   

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        _adaptationFieldDataIdentifier = (byte)(buffer[_lastIndex]);
        _lastIndex = index + DescriptorLength;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Adaptation Field Data Descriptor message is short"));
      }
    }
    #endregion
  }  
}