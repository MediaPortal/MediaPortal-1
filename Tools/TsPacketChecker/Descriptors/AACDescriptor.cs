using System;

namespace WindowsApplication13
{
  internal class AACDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private byte _profileAndLevel;
    private int _aACTypeFlag;
    private bool _sAOCDETypeFlag;
    private byte _aACType;
    private byte[] _additionalInfoBytes;
    #endregion
    #region Constructor
    public AACDescriptor()
    {
    }
    #endregion
    #region Properties
    public byte ProfileAndLevel { get { return _profileAndLevel; } set { _profileAndLevel = value; } }
    public int AACTypeFlag { get { return _aACTypeFlag; } set { _aACTypeFlag = value; } }
    public bool SAOCDETypeFlag { get { return _sAOCDETypeFlag; } set { _sAOCDETypeFlag = value; } }
    public byte AACType { get {return _aACType; } set { _aACType = value; } }
    public byte[] AdditionalInfoBytes { get { return _additionalInfoBytes; } set { _additionalInfoBytes = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("AAC Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }    

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        var headerLength = _lastIndex;
        _profileAndLevel = buffer[_lastIndex];
        _lastIndex++;
        _aACTypeFlag = (buffer[_lastIndex] >> 7) & 0x01;

        var i = 4;
        if (_aACTypeFlag == 0x01)
        {
          headerLength++;
          _aACType = buffer[i++];
        }

        _additionalInfoBytes = new byte[DescriptorLength - headerLength];
        Buffer.BlockCopy(buffer, buffer[i], _additionalInfoBytes, 0, DescriptorLength - headerLength);
        _lastIndex = index + DescriptorLength;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The AAC Descriptor message is short"));
      }
    }
    #endregion
  }
}