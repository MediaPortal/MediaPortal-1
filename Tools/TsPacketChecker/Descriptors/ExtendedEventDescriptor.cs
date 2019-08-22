using System;
using System.Collections.ObjectModel;

namespace TsPacketChecker
{
  internal class ExtendedEventDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private int _descriptorNumber;
    private int _lastDescriptorNumber;
    private string _languageCode;
    private Collection<string> _itemDescriptions;
    private Collection<string> _items;
    private byte[] _text;
    private byte[] _textCodePage;
    #endregion
    #region Constructor
    public ExtendedEventDescriptor()
    {
    }
    #endregion
    #region Properties
    /// <summary>
    /// Get the item descriptions.
    /// </summary>
    public Collection<string> ItemDescriptions { get { return (_itemDescriptions); } }

    /// <summary>
    /// Get the items.
    /// </summary>
    public Collection<string> Items { get { return (_items); } }

    /// <summary>
    /// Get the descriptor number.
    /// </summary>
    public int DescriptorNumber { get { return (_descriptorNumber); } }

    /// <summary>
    /// Get the last descriptor number.
    /// </summary>
    public int LastDescriptorNumber { get { return (_lastDescriptorNumber); } }

    /// <summary>
    /// Get the non itemised text.
    /// </summary>
    public byte[] Text { get { return (_text); } }

    /// <summary>
    /// Get the language code.
    /// </summary>
    public string LanguageCode { get { return (_languageCode); } }
    public byte[] TextCodePage { get { return _textCodePage; } set { _textCodePage = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Extended Event Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }

    

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        _descriptorNumber = (int)buffer[_lastIndex] >> 4;
        _lastDescriptorNumber = (int)buffer[_lastIndex] & 0x0f;
        _lastIndex++;

        _languageCode = Utils.GetString(buffer, _lastIndex, 3);
        _lastIndex += 3;

        int totalItemLength = (int)buffer[_lastIndex];
        _lastIndex++;

        if (totalItemLength != 0)
        {
          _itemDescriptions = new Collection<string>();
          _items = new Collection<string>();

          while (totalItemLength != 0)
          {
            int itemDescriptionLength = (int)buffer[_lastIndex];
            _lastIndex++;

            if (itemDescriptionLength != 0)
            {
              string itemDescription = Utils.GetString(buffer, _lastIndex, itemDescriptionLength);
              _itemDescriptions.Add(itemDescription.ToUpper());
              _lastIndex += itemDescriptionLength;
            }

            int itemLength = (int)buffer[_lastIndex];
            _lastIndex++;

            if (itemLength != 0)
            {
              string item = Utils.GetString(buffer, _lastIndex, itemLength);
              _items.Add(item);
              _lastIndex += itemLength;
            }
            else
              _items.Add("");

            totalItemLength -= (itemDescriptionLength + itemLength + 2);
          }
        }

        int textLength = (int)buffer[_lastIndex];
        _lastIndex++;

        if (textLength != 0)
        {
          _text = Utils.GetBytes(buffer, _lastIndex, textLength);

          int byteLength = textLength > 2 ? 3 : 1;
          _textCodePage = Utils.GetBytes(buffer, _lastIndex, byteLength);

          _lastIndex += textLength;
        }       
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Extended Event Descriptor message is short"));
      }
    }
    #endregion
  }
}