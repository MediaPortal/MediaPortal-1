using System;

namespace TsPacketChecker
{
  internal class PrivateDataSpecifierDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private int _dataSpecifier;
    #endregion
    #region Constructor
    public PrivateDataSpecifierDescriptor()
    {
    }
    #endregion
    #region Properties
    public int DataSpecifier { get { return _dataSpecifier; } set { _dataSpecifier = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Private Data Specifier Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }    

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        if(DescriptorLength != 0)
                {
          _dataSpecifier = (buffer[_lastIndex] << 24) + (buffer[_lastIndex + 1] << 16) +
                          (buffer[_lastIndex + 2] << 8) + buffer[_lastIndex + 3];
          _lastIndex += 4;
        }
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Private Data Specifier Descriptor message is short"));
      }
    }
    #endregion
  }
}