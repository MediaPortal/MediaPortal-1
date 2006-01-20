using System;
using System.Collections.Generic;
using System.Text;

using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Epg
{
  class EPGLanguage
  {
    private string _title;
    private string _description;
    private string _language;
    public EPGLanguage(string language, string title, string description)
    {
      _title = title;
      _description = description;
      _language = language;
    }
    public string Language
    {
      get { return _language; }
    }
    public string Title
    {
      get { return _title; }
    }
    public string Description
    {
      get { return _description; }
    }
  }
}
