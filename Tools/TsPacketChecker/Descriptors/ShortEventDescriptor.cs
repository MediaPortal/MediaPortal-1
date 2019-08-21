using System;

namespace TsPacketChecker
{
  internal class ShortEventDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private string _languageCode;
    private string _eventName;
    private string _shortDescription;

    private byte[] _eventNameCodePage;
    private byte[] _shortDescriptionCodePage;

    #endregion
    #region Constructor
    public ShortEventDescriptor()
    {
    }
    #endregion
    #region Properties
    public string LanguageCode { get => _languageCode; set => _languageCode = value; }
    public string EventName { get => _eventName; set => _eventName = value; }
    public string ShortDescription { get => _shortDescription; set => _shortDescription = value; }
    public byte[] EventNameCodePage { get => _eventNameCodePage; set => _eventNameCodePage = value; }
    public byte[] ShortDescriptionCodePage { get => _shortDescriptionCodePage; set => _shortDescriptionCodePage = value; }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Short Event Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }
    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        _languageCode = Utils.GetString(buffer, _lastIndex, 3);
        _lastIndex += _languageCode.Length;

        int eventNameLength = (int)buffer[_lastIndex];
        _lastIndex++;

        if (eventNameLength != 0)
        {
          _eventName = Utils.GetString(buffer, _lastIndex, eventNameLength);

          int byteLength = eventNameLength > 2 ? 3 : 1;
          _eventNameCodePage = Utils.GetBytes(buffer, _lastIndex, byteLength);
          _lastIndex += eventNameLength;
        }
        int textLength = (int)buffer[_lastIndex];
        _lastIndex++;

        if (textLength != 0)
        {
          _shortDescription = Utils.GetString(buffer, _lastIndex, textLength);
          int byteLength = textLength > 2 ? 3 : 1;
          _shortDescriptionCodePage = Utils.GetBytes(buffer, _lastIndex, byteLength);
          _lastIndex += textLength;
        }
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Short Event Descriptor message is short"));
      }
    }
    #endregion
  }
}