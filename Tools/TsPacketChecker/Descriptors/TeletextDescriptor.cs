using System;
using System.Collections.Generic;
using System.Text;

namespace TsPacketChecker
{
  internal class TeletextDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private List<Language> _languages;

    #endregion
    #region Constructor
    public TeletextDescriptor()
    {
    }
    #endregion
    #region Properties
    public List<Language> Languages { get { return _languages; }set { _languages = value; } }
    #endregion

    public static Dictionary<byte, string> TeletextTypes = new Dictionary<byte, string>
        {
            {0x00, "reserved for future use"},
            {0x01, "initial Teletext page"},
            {0x02, "Teletext subtitle page"},
            {0x03, "additional information page"},
            {0x04, "programme schedule page"},
            {0x05, "Teletext subtitle page for hearing impaired people"},
            {0x07, "reserved for future use"},
            {0x08, "reserved for future use"},
            {0x09, "reserved for future use"},
            {0x0A, "reserved for future use"},
            {0x0B, "reserved for future use"},
            {0x0C, "reserved for future use"},
            {0x0D, "reserved for future use"},
            {0x0E, "reserved for future use"},
            {0x0F, "reserved for future use"},
            {0x10, "reserved for future use"},
            {0x11, "reserved for future use"},
            {0x12, "reserved for future use"},
            {0x13, "reserved for future use"},
            {0x14, "reserved for future use"},
            {0x15, "reserved for future use"},
            {0x16, "reserved for future use"},
            {0x17, "reserved for future use"},
            {0x18, "reserved for future use"},
            {0x19, "reserved for future use"},
            {0x1A, "reserved for future use"},
            {0x1B, "reserved for future use"},
            {0x1C, "reserved for future use"},
            {0x1D, "reserved for future use"},
            {0x1E, "reserved for future use"},
            {0x1F, "reserved for future use"}
        };
    

    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Teletext Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }    

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        var languages = new List<Language>();
        
        do
        {
          var lang = new Language
          {
            Iso639LanguageCode = Encoding.UTF8.GetString(buffer, _lastIndex, 3),
            TeletextType = (byte)((buffer[_lastIndex + 3] >> 3) & 0x01f),
            TeletextMagazineNumber = (byte)(buffer[_lastIndex + 3] & 0x7),
            TeletextPageNumber = buffer[_lastIndex + 4]
          };

          languages.Add(lang);

          _lastIndex += 5;
        } while (_lastIndex < index + 2 + DescriptorLength);

        _languages = languages;
        
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("Teletext Descriptor message is short"));
      }
    }
    #endregion
    public class Language
    {
      public Language()
      {
      }

      public Language(Language lang)
      {
        Iso639LanguageCode = lang.Iso639LanguageCode;
        TeletextType = lang.TeletextType;
        TeletextMagazineNumber = lang.TeletextMagazineNumber;
        TeletextPageNumber = lang.TeletextPageNumber;
      }

      public string Iso639LanguageCode { get; internal set; }
      public byte TeletextType { get; internal set; }
      public byte TeletextMagazineNumber { get; internal set; }
      public byte TeletextPageNumber { get; internal set; }
    }
  }
}