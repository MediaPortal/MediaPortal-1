using System;

namespace WindowsApplication13
{
  internal class CaDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private int _systemIdentifier;
    private int _caPid;
    #endregion
    #region Constructor
    public CaDescriptor()
    {
    }
    #endregion
    #region Properties
    public int SystemIdentifier { get { return _systemIdentifier; } set { _systemIdentifier = value; } }
    public int CaPid { get {return _caPid; } set { _caPid = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("CA Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }    

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        _systemIdentifier = Utils.Convert2BytesToInt(buffer, _lastIndex);
        _lastIndex += 2;
        _caPid = ((buffer[_lastIndex] & 0x1f)*256) + (int)buffer[_lastIndex];
        _lastIndex = index + DescriptorLength;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The CA Descriptor message is short"));
      }
    }
    #endregion
  }
}