using System;

namespace WindowsApplication13
{
  internal class DTSDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private int _samplerateCode;
    private int _bitrate;
    private int _numberOfBlocks;
    private int _frameSize;
    private int _suroundMode;
    private int _lFEFlag;
    private int _extendedSurroundFlag;
    private byte[] _additionalInfoBytes;

    #endregion
    #region Constructor
    public DTSDescriptor()
    {
    }
    #endregion
    #region Properties
    public int SamplerateCode { get { return _samplerateCode; } set { _samplerateCode = value; } }
    public int Bitrate { get { return _bitrate; } set { _bitrate = value; } }
    public int NumberOfBlocks { get { return _numberOfBlocks; } set { _numberOfBlocks = value; } }
    public int FrameSize { get { return _frameSize; } set { _frameSize = value; } }
    public int SuroundMode { get { return _suroundMode; } set { _suroundMode = value; } }
    public int LFEFlag { get { return _lFEFlag; } set { _lFEFlag = value; } }
    public int ExtendedSurroundFlag { get { return _extendedSurroundFlag; } set { _extendedSurroundFlag = value; } }
    public byte[] AdditionalInfoBytes { get { return _additionalInfoBytes; } set { _additionalInfoBytes = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("DTS Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }    

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        _samplerateCode = (buffer[_lastIndex] >> 4) & 0x0f;
        _bitrate = ((buffer[_lastIndex] & 0x0f) << 2) | ((buffer[_lastIndex++] >> 6) & 0x02);
        
        _numberOfBlocks = ((buffer[3] & 0x3f) << 2) | ((buffer[_lastIndex++] >> 7) & 0x01);
        _frameSize = ((buffer[_lastIndex] & 0x7f) << 7) | (buffer[_lastIndex++] >> 1);
        _suroundMode = ((buffer[_lastIndex] & 0x01) << 6) | ((buffer[_lastIndex++] >> 3) & 0x1f);
        _lFEFlag = (buffer[_lastIndex] >> 2) & 0x01;
        _extendedSurroundFlag = buffer[_lastIndex] & 0x03;
        _additionalInfoBytes = new byte[DescriptorLength - 5];
        Buffer.BlockCopy(buffer, buffer[_lastIndex++], _additionalInfoBytes, 0, DescriptorLength - 5);
        _lastIndex = index + DescriptorLength;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The DTS Descriptor message is short"));
      }
    }
    #endregion
  }
}