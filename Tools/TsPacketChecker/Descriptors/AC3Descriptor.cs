using System;

namespace WindowsApplication13
{
  internal class AC3Descriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private bool _componentTypeFlag;
    private bool _bsIdFlag;
    private bool _mainIdFlag;
    private bool _asvcFlag;
    private byte _componentType;
    private byte _bsId;
    private byte _mainId;
    private byte _asvc;

    #endregion
    #region Constructor
    public AC3Descriptor()
    {
    }
    #endregion
    #region Properties
    public bool ComponentTypeFlag { get { return _componentTypeFlag; } set { _componentTypeFlag = value; } }
    public bool BsIdFlag { get { return _bsIdFlag; } set { _bsIdFlag = value; } }
    public bool MainIdFlag { get { return _mainIdFlag; } set { _mainIdFlag = value; } }
    public bool AsvcFlag { get { return _asvcFlag; } set { _asvcFlag = value; } }
    public byte ComponentType { get { return _componentType; } set { _componentType = value; } }
    public byte BsId { get { return _bsId; } set { _bsId = value; } }
    public byte MainId { get { return _mainId; } set { _mainId = value; } }
    public byte Asvc { get { return _asvc; } set { _asvc = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("AC3 Descriptor: Index requested before block processed"));
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
        _asvcFlag = (buffer[_lastIndex++] & 0x10) == 0x10;

        if (_componentTypeFlag) _componentType = buffer[_lastIndex++];
        if (_bsIdFlag) _bsId = buffer[_lastIndex++];
        if (_mainIdFlag) _mainId = buffer[_lastIndex++];
        if (_asvcFlag) _asvc = buffer[_lastIndex];
        _lastIndex = index + DescriptorLength;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The AC3 Descriptor message is short"));
      }
    }
    #endregion
  }
}