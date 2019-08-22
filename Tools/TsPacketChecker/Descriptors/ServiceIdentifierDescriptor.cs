using System;

namespace TsPacketChecker
{
  internal class ServiceIdentifierDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private string _identifier;


    #endregion
    #region Constructor
    public ServiceIdentifierDescriptor()
    {
    }
    #endregion
    #region Properties
    public string Identifier { get => _identifier; set => _identifier = value; }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Service Identifier Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }    

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        int identifierLength = index + DescriptorLength - _lastIndex;

        if (identifierLength != 0)
        {
          _identifier = Utils.GetString(buffer, _lastIndex, identifierLength);
          _lastIndex += identifierLength;
        }
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Service Identifier Descriptor message is short"));
      }
    }
    #endregion
  }
}