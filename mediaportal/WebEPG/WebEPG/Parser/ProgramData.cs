/*
  *	Copyright (C) 2005 Team MediaPortal
  *	http://www.team-mediaportal.com
  *
  *  This Program is free software; you can redistribute it and/or modify
  *  it under the terms of the GNU General Public License as published by
  *  the Free Software Foundation; either version 2, or (at your option)
  *  any later version.
  *
  *  This Program is distributed in the hope that it will be useful,
  *  but WITHOUT ANY WARRANTY; without even the implied warranty of
  *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
  *  GNU General Public License for more details.
  *
  *  You should have received a copy of the GNU General Public License
  *  along with GNU Make; see the file COPYING.  If not, write to
  *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
  *  http://www.gnu.org/copyleft/gpl.html
  *
  */

using System;
using System.Text;
using MediaPortal.Utils.Web;

namespace MediaPortal.WebEPG
{
  public class ProgramDateTime
  {
    public int Hour = 0;
    public int Minute = 0;
    public int Day = 0;
    public int Month = 0;
    public int Year = 0;
  }

  public class ProgramData : ParserData
  {
    private string _channelID = String.Empty;
    private string _title = String.Empty;
    private string _subTitle = String.Empty;
    private string _description = String.Empty;
    private string _month = String.Empty;
    private string _genre = String.Empty;
    private int _day = 0;
    private ProgramDateTime _startTime;
    private ProgramDateTime _endTime;

    public string ChannelID
    {
      get { return _channelID; }
      set { _channelID = value; }
    }

    public string Title
    {
      get { return _title; }
      set { _title = value; }
    }

    public string SubTitle
    {
      get { return _subTitle; }
      set { _subTitle = value; }
    }

    public string Description
    {
      get { return _description; }
      set { _description = value; }
    }

    public string Month
    {
      get { return _month; }
      set { _month = value; }
    }

    public string Genre
    {
      get { return _genre; }
      set { _genre = value; }
    }

    public int Day
    {
      get { return _day; }
      set { _day = value; }
    }

    public ProgramDateTime StartTime
    {
      get { return _startTime; }
      set { _startTime = value; }
    }

    public ProgramDateTime EndTime
    {
      get { return _endTime; }
      set { _endTime = value; }
    }


    override public void SetElement(string tag, string element)
    {
      switch (tag)
      {
      case "<#START>":
        _startTime = getTime(element);
        break;
      case "<#END>":
        _endTime = getTime(element);
        break;
      case "<#DAY>":
        _day = int.Parse(element);
        break;
      case "<#DESCRIPTION>":
        if(_description == string.Empty)
          _description = element.Trim(' ', '\n', '\t');
        else
          _description = _description + "\n" + element.Trim(' ', '\n', '\t');
        break;
      case "<#MONTH>":
        _month = element.Trim(' ', '\n', '\t');
        break;
      case "<#TITLE>":
        _title = element.Trim(' ', '\n', '\t');
        break;
      case "<#SUBTITLE>":
        _subTitle = element.Trim(' ', '\n', '\t');
        break;
      case "<#GENRE>":
        _genre = element.Trim(' ', '\n', '\t');
        break;
      default:
        break;
      }
    }

    public bool IsProgram(string SearchList)
    {
      string[,] Params = GetSearchParams(SearchList);

      for (int i = 0; i < Params.Length / 2; i++)
      {
        switch (Params[i, 0])
        {
        case "TITLE":
          if (Title.Contains(Params[i, 1]))
            return true;
          break;
        case "DESC":
          if (Description.Contains(Params[i, 1]))
            return true;
          break;
        default:
          break;
        }
      }

      return false;
    }

    private string[,] GetSearchParams(string SearchList)
    {
      int pos = 0;
      int num = 0;
      int offset;

      while ((offset = SearchList.IndexOf(';', pos)) != -1)
      {
        pos = offset + 1;
        num++;
      }

      string[,] SearchParams = new string[num, 2];

      int startPos = 0;
      int endPos = 0;
      for (int i = 0; i < num; i++)
      {
        if ((startPos = SearchList.IndexOf('[', startPos)) != -1)
        {
          if ((endPos = SearchList.IndexOf(']', startPos + 1)) != -1)
          {
            SearchParams[i, 0] = SearchList.Substring(startPos + 1, endPos - startPos - 1);
          }
        }

        if ((startPos = SearchList.IndexOf('"', endPos)) != -1)
        {
          if ((endPos = SearchList.IndexOf('"', startPos + 1)) != -1)
          {
            SearchParams[i, 1] = SearchList.Substring(startPos + 1, endPos - startPos - 1);
          }
        }

        startPos = SearchList.IndexOf(';', startPos);
      }

      return SearchParams;
    }

    private ProgramDateTime getTime(string strTime)
    {
      if(strTime == "")
        return null;

      int sepPos;
      //bool found = false;
      ProgramDateTime dt = new ProgramDateTime();
      char[] timeSeperators = { ':', '.', 'h' };

      if ((sepPos = strTime.IndexOfAny(timeSeperators)) != -1) // IndexOf(":")) != -1)
      {
        try
        {
          dt.Hour = int.Parse(strTime.Substring(0, sepPos));
          dt.Minute = int.Parse(strTime.Substring(sepPos + 1, 2));
        }
        catch(Exception)
        {
          // log exception - time parsing error (Warning) - template got some other text
          return null;
        }
      }
      else
      {
        return null;
      }

      /*
        if ((sepPos = strTime.IndexOf(".")) != -1)
        {
        iTime[0] = int.Parse(strTime.Substring(0, sepPos));
        iTime[1] = int.Parse(strTime.Substring(sepPos+1, 2));
        found = true;
        }

        if ((sepPos = strTime.IndexOf("h")) != -1)
        {
        iTime[0] = int.Parse(strTime.Substring(0, sepPos));
        iTime[1] = int.Parse(strTime.Substring(sepPos+1, 2));
        found = true;
        }

        if (!found)
        */

      if (strTime.ToLower().IndexOf("pm") != -1 && dt.Hour != 0)
      {
        if(dt.Hour != 12)
          dt.Hour += 12;
      }

      if (strTime.ToLower().IndexOf("am") != -1 && dt.Hour == 12)
        dt.Hour = 0;

      if (dt.Hour == 24)
        dt.Hour = 0;

      return dt;
    }
  }
}
