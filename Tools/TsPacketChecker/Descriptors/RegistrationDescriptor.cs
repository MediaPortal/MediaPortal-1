using System;
using System.Text;

namespace TsPacketChecker
{
  internal class RegistrationDescriptor : Descriptor
  {
    #region Private Fields
    private string _organization;
    private byte[] _additionalIdentificationInfo;
    private int _lastIndex = -1;
    #endregion
    #region Constructor
    public RegistrationDescriptor()
    {
    }
    #endregion
    #region Properties
    public string Organization
    {
      get { return _organization; }
      set { _organization = value; }
    }
    public byte[] AdditionalIdentificationInfo
    {
      get { return _additionalIdentificationInfo; }
      set { _additionalIdentificationInfo = value; }
    }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Registration Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }
    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        if (buffer.Length - _lastIndex <= DescriptorLength) return;
        _organization = Encoding.ASCII.GetString(buffer, _lastIndex, 4);
        _lastIndex += 4;

        if (DescriptorLength <= 4) return;
        _additionalIdentificationInfo = new byte[DescriptorLength - 4];
        Buffer.BlockCopy(buffer, _lastIndex, _additionalIdentificationInfo, 0, _additionalIdentificationInfo.Length);
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Registration Descriptor message is short"));
      }
    }
    #endregion
  }
}