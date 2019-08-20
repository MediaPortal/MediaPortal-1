using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsApplication13
{
  internal class SubtitlingDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private List<Language> _languages;
    #endregion
    #region Constructor
    public SubtitlingDescriptor()
    {
    }
    #endregion
    #region Properties
    internal List<Language> Languages { get { return _languages; } set { _languages = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Subtitling Descriptor: Index requested before block processed"));
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
            SubtitlingType = buffer[_lastIndex + 3],
            CompositionPageId = (ushort)((buffer[_lastIndex + 4] << 8) + buffer[_lastIndex + 5]),
            AncillaryPageId = (ushort)((buffer[_lastIndex + 6] << 8) + buffer[_lastIndex + 7])
          };

          languages.Add(lang);

          _lastIndex += 8;
        } while (_lastIndex < index + 2 + DescriptorLength);

        _languages = languages;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Subtitling Descriptor message is short"));
      }
    }
    #endregion
  }

  internal class Language
  {
    public object Iso639LanguageCode { get; set; }
    public object SubtitlingType { get; set; }
    public ushort CompositionPageId { get; set; }
    public ushort AncillaryPageId { get; set; }
  }
}