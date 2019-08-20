using System;

namespace WindowsApplication13
{
  internal class EnhancedAC3Descriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private bool _componentTypeFlag;
    private bool _bsIdFlag;
    private bool _mainIdFlag;
    private bool _asvcFlag;
    private bool _mixInfoExists;
    private bool _substream1Flag;
    private bool _substream2Flag;
    private bool _substream3Flag;
    private byte _componentType;
    private byte _bsId;
    private byte _mainId;
    private byte _asvc;
    private byte _substream1;
    private byte _substream2;
    private byte _substream3;

    #endregion
    #region Constructor
    public EnhancedAC3Descriptor()
    {
    }
    #endregion
    #region Properties
    public bool ComponentTypeFlag { get { return _componentTypeFlag; }set { _componentTypeFlag = value; } }
    public bool BsIdFlag { get { return _bsIdFlag; } set { _bsIdFlag = value; } }
    public bool MainIdFlag { get { return _mainIdFlag; } set { _mainIdFlag = value; } }
    public bool AsvcFlag { get { return _asvcFlag; } set { _asvcFlag = value; } }
    public bool MixInfoExists { get { return _mixInfoExists; } set { _mixInfoExists = value; } }
    public bool Substream1Flag { get { return _substream1Flag; } set { _substream1Flag = value; } }
    public bool Substream2Flag { get { return _substream2Flag; } set { _substream2Flag = value; } }
    public bool Substream3Flag { get { return _substream3Flag; } set { _substream3Flag = value; } }
    public byte ComponentType { get { return _componentType; } set { _componentType = value; } }
    public byte BsId { get { return _bsId; } set { _bsId = value; } }
    public byte MainId { get { return _mainId; } set { _mainId = value; } }
    public byte Asvc { get { return _asvc; } set { _asvc = value; } }
    public byte Substream1 { get { return _substream1; } set { _substream1 = value; } }
    public byte Substream2 { get { return _substream2; } set { _substream2 = value; } }
    public byte Substream3 { get { return _substream3; } set {_substream3 = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Enhanced AC3 Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        _componentTypeFlag = (buffer[_lastIndex] & 0x80) == 0x80;
        _bsIdFlag = (buffer[_lastIndex] & 0x40) == 0x40;
        _mainIdFlag = (buffer[_lastIndex] & 0x20) == 0x20;
        _asvcFlag = (buffer[_lastIndex] & 0x10) == 0x10;
        _mixInfoExists = (buffer[_lastIndex] & 0x08) == 0x08;
        _substream1Flag = (buffer[_lastIndex] & 0x04) == 0x04;
        _substream2Flag = (buffer[_lastIndex] & 0x02) == 0x02;
        _substream3Flag = (buffer[_lastIndex++] & 0x01) == 0x01;

        if (_componentTypeFlag) _componentType = buffer[_lastIndex++];
        if (_bsIdFlag) _bsId = buffer[_lastIndex++];
        if (_mainIdFlag) _mainId = buffer[_lastIndex++];
        if (_asvcFlag) _asvc = buffer[_lastIndex++];

        if (_substream1Flag) _substream1 = buffer[_lastIndex++];
        if (_substream2Flag) _substream2 = buffer[_lastIndex++];
        if (_substream3Flag) _substream3 = buffer[_lastIndex];
        _lastIndex = index + DescriptorLength;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Enhanced AC3 Descriptor message is short"));
      }
    }
    #endregion
  }
}