using System;
using System.Collections.Generic;
using System.Text;

using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Epg
{
  #region EPGEvent class
  class EPGEvent
  {
    private string _genre;
    private DateTime _startTime;
    private DateTime _endTime;
    List<EPGLanguage> _listLanguages = new List<EPGLanguage>();
    public EPGEvent(string genre, DateTime startTime, DateTime endTime)
    {
      _genre = genre;
      _startTime = startTime;
      _endTime = endTime;
    }
    public string Genre
    {
      get { return _genre; }
    }
    public DateTime StartTime
    {
      get { return _startTime; }
    }
    public DateTime EndTime
    {
      get { return _endTime; }
    }
    public List<EPGLanguage> Languages
    {
      get { return _listLanguages; }
    }
  }

  #endregion

}
