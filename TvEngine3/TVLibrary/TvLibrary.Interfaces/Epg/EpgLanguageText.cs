using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Epg
{
  [Serializable]
  public class EpgLanguageText
  {
    #region variables
    string _language;
    string _title;
    string _description;
    string _genre;
    #endregion

    #region ctor
    public EpgLanguageText(string language, string title, string description, string genre)
    {
      Language = language;
      Title = title;
      Description = description;
      Genre = genre;
    }
    #endregion

    #region properties
    public string Language
    {
      get
      {
        return _language;
      }
      set
      {
        _language = value;
        if (_language == null) _language = "";
      }
    }
    public string Title
    {
      get
      {
        return _title;
      }
      set
      {
        _title = value;
        if (_title == null) _title = "";
      }
    }
    public string Description
    {
      get
      {
        return _description;
      }
      set
      {
        _description = value;
        if (_description == null) _description = "";
      }
    }
    public string Genre
    {
      get
      {
        return _genre;
      }
      set
      {
        _genre = value;
        if (_genre == null) _genre = "";
      }
    }
    #endregion
  }
}
