using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Epg
{
  [Serializable]
  public class EpgProgram :IComparable<EpgProgram>
  {
    #region variables
    List<EpgLanguageText> _languageText;
    DateTime _startTime;
    DateTime _endTime;
    #endregion

    #region ctor

    public EpgProgram(DateTime startTime, DateTime endTime)
    {
      _startTime = startTime;
      _endTime = endTime;
      _languageText = new List<EpgLanguageText>();
    }

    #endregion


    #region properties
    public List<EpgLanguageText> Text 
    {
      get
      {
        return _languageText;
      }
      set
      {
        _languageText = value;
      }
    }

    public DateTime StartTime 
    {
      get
      {
        return _startTime;
      }
      set
      {
        _startTime = value;
      }
    }

    public DateTime EndTime 
    { 
      get
      {
        return _endTime;
      }
      set
      {
        _endTime = value;
      }
    }
    #endregion

    #region IComparable<EpgProgram> Members

    public int CompareTo(EpgProgram other)
    {
      if (other._endTime <= StartTime) return 1;
      if (other.StartTime >= EndTime) return -1;
      return 0;
    }

    #endregion
  }
}
