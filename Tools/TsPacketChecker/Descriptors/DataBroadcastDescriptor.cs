using System;
using System.Text;

namespace WindowsApplication13
{
  internal class DataBroadcastDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private string _textDescription;
    private string _languageCode;
    private byte[] _selectorBytes;
    private byte _componentTag;
    private int _dataBroadcastId;
    #endregion
    #region Constructor
    public DataBroadcastDescriptor()
    {
    }
    #endregion
    #region Properties
    public int DataBroadcastId { get { return _dataBroadcastId; } set { _dataBroadcastId = value; } }
    public byte ComponentTag { get { return _componentTag; } set { _componentTag = value; } }
    public byte[] SelectorBytes { get { return _selectorBytes; } set { _selectorBytes = value; } }
    public string LanguageCode { get { return _languageCode; } set { _languageCode = value; }}
    public string TextDescription { get { return _textDescription; } set { _textDescription = value; }}
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Data Broadcast Descriptor: Index requested before block processed"));
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
          _dataBroadcastId = (buffer[_lastIndex] << 8) + buffer[_lastIndex + 1];
          _lastIndex += 2;
          _componentTag = buffer[_lastIndex];
          _lastIndex++;
          var selectorLength = (int)buffer[_lastIndex];
          _lastIndex++;
          if (selectorLength != 0)
          {
            Buffer.BlockCopy(buffer, _lastIndex, _selectorBytes, 0, selectorLength);
            _lastIndex += selectorLength;
          }
          _languageCode = Encoding.UTF8.GetString(buffer, _lastIndex, 3);
          _lastIndex += 3;
        }
        var textLength = (int)buffer[_lastIndex];
        _lastIndex++;
        if (textLength != 0)
        {
          _textDescription = Encoding.UTF8.GetString(buffer, _lastIndex, textLength);
          _lastIndex += textLength;
        }
        _lastIndex = DescriptorLength;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Data Broadcast Descriptor message is short"));
      }
    }
    #endregion
  }
}