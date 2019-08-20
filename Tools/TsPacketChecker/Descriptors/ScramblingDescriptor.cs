using System;

namespace WindowsApplication13
{
  internal class ScramblingDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private int _scramblingMode;
    #endregion
    #region Constructor
    public ScramblingDescriptor()
    {
    }
    #endregion
    #region Properties
    public int ScramblingMode { get { return _scramblingMode; } set { _scramblingMode = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Scrambling Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }   

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        if (DescriptorLength != 0)
        {
          _scramblingMode = (buffer[_lastIndex] << 8) + buffer[_lastIndex + 1];
          _lastIndex += 4;
        }
        _lastIndex =  DescriptorLength;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Scrambling Descriptor message is short"));
      }
    }
    #endregion
  }
}