using System;
using System.Text;

namespace TsPacketChecker
{
  internal class ISO639LanguageDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private string _language;
    private int _audioType;
    #endregion
    #region Constructor
    public ISO639LanguageDescriptor()
    {
    }
    #endregion
    #region Properties
    public string Language { get { return _language; } set { _language = value; } }
    public int AudioType { get { return _audioType; } set { _audioType = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Iso639Language Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }
    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        _language = Encoding.UTF8.GetString(buffer, _lastIndex, 3);
        _lastIndex += 3;
        _audioType = buffer[_lastIndex];
        _lastIndex++;
      }
      catch(IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Iso639Language Descriptor message is short"));
      }
    }
    #endregion
  }
}