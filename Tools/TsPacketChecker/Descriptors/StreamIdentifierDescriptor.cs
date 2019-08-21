using System;

namespace TsPacketChecker
{
  internal class StreamIdentifierDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private int _componentTag;
    #endregion
    #region Constructor
    /// <summary>
    /// Initialize a new instance of the StreamIdentifierDescriptor class.
    /// </summary>
    internal StreamIdentifierDescriptor() { }
    #endregion
    #region Properties
    public int ComponentTag { get => _componentTag; set => _componentTag = value; } 
    #endregion
    #region Overrides

    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("StreamIdentifier: Index requested before block processed"));
        return (_lastIndex);
      }
    }

    

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;

      try
      {
        _componentTag = (int)buffer[_lastIndex];
        _lastIndex++;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Stream Identifier Descriptor message is short"));
      }
    } 
    #endregion
  }
}