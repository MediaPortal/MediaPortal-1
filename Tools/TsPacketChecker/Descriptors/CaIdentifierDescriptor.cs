using System;
using System.Collections.Generic;

namespace TsPacketChecker
{
  internal class CaIdentifierDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private ushort[] _caSystemIds;
    #endregion
    #region Constructor
    public CaIdentifierDescriptor()
    {
    }
    #endregion
    #region Properties
    public ushort[] CaSystemIds { get { return _caSystemIds; } set { _caSystemIds = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Ca Identifier Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }   

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        var al = new List<ushort>();
        for (var offset2 = _lastIndex; offset2 < _lastIndex + DescriptorLength - 1; offset2 += 2)
          al.Add((ushort)((buffer[offset2] << 8) | buffer[offset2 + 1]));
        _caSystemIds = al.ToArray();

      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Ca Identifier Descriptor message is short"));
      }
    }
    #endregion
  }
}