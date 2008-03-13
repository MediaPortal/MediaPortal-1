using System;
using System.Collections.Generic;
using System.Text;

namespace TvDatabase
{
  public class NowAndNext
  {
    int     _idChannel;
    DateTime _nowStart;
    DateTime _nowEnd;
    string _titleNow;
    string _titleNext;
    int _idProgramNow;
    int _idProgramNext;

    public NowAndNext(int idChannel, DateTime nowStart, DateTime nowEnd, string titleNow, string titleNext, int idProgramNow, int idProgramNext)
    {
       _idChannel=idChannel;
      _nowStart=nowStart;
      _nowEnd=nowEnd;
      _titleNow=titleNow;
      _titleNext=titleNext;
      _idProgramNow=idProgramNow;
      _idProgramNext=idProgramNext;
    }

    public int IdChannel
    {
      get
      {
        return _idChannel ;
      }
      set
      {
        _idChannel = value;
      }
    }

    public DateTime NowStartTime
    {
      get
      {
        return _nowStart;
      }
      set
      {
        _nowStart = value;
      }
    }

    public DateTime NowEndTime
    {
      get
      {
        return _nowEnd;
      }
      set
      {
        _nowEnd = value;
      }
    }

    public string TitleNow
    {
      get
      {
        return _titleNow;
      }
      set
      {
        _titleNow = value;
      }
    }

    public string TitleNext
    {
      get
      {
        return _titleNext;
      }
      set
      {
        _titleNext = value;
      }
    }

    public int IdProgramNow
    {
      get
      {
        return _idProgramNow;
      }
      set
      {
        _idProgramNow = value;
      }
    }

    public int IdProgramNext
    {
      get
      {
        return _idProgramNext;
      }
      set
      {
        _idProgramNext = value;
      }
    }
  }
}
