#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using MediaPortal.Services;
using MediaPortal.TV.Database;
using MediaPortal.Utils.Time;
using MediaPortal.Utils.Web;
using MediaPortal.WebEPG.Config.Grabber;

namespace MediaPortal.WebEPG.Parser
{
  /// <summary>
  /// Program data used by IParses to stored the data for each program
  /// </summary>
  public class ProgramData : IParserData
  {
    #region Variables

    private string _channelId = string.Empty;
    private string _title = string.Empty;
    private string _subTitle = string.Empty;
    private string _description = string.Empty;
    private string _genre = string.Empty;
    private List<string> _actors;
    private DataPreference _preference;
    private HTTPRequest _sublink;
    private Dictionary<string, int> _months;
    private WorldDateTime _startTime;
    private WorldDateTime _endTime;
    private int _episode = 0;
    private int _season = 0;
    private bool _repeat = false;
    private bool _subtitles = false;

    #endregion

    #region Constructors/Destructors

    public ProgramData()
    {
    }

    public ProgramData(Dictionary<string, int> months) //string[] months)
    {
      _months = months;
      //if (months != null)
      //{
      //  _months = new Dictionary<string, int>();
      //  for (int i = 0; i < months.Length; i++)
      //  {
      //    _months.Add(months[i], i + 1); ;
      //  }
      //}
    }

    #endregion

    #region Properties

    // Public Properties
    public DataPreference Preference
    {
      get { return _preference; }
      set { _preference = value; }
    }

    public HTTPRequest SublinkRequest
    {
      get { return _sublink; }
      set { _sublink = value; }
    }

    public string ChannelId
    {
      get { return _channelId; }
      set { _channelId = value; }
    }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <remarks>
    /// If the case of title being set is all UPPER case then it is converted to proper case.
    /// </remarks>
    /// <value>The title.</value>
    public string Title
    {
      get { return _title; }
      set { _title = ProperCase(value); }
    }

    public string SubTitle
    {
      get { return _subTitle; }
      set { _subTitle = ProperCase(value); }
    }

    public string Description
    {
      get { return _description; }
      set { _description = value; }
    }

    public string Genre
    {
      get { return _genre; }
      set { _genre = ProperCase(value); }
    }

    public WorldDateTime StartTime
    {
      get { return _startTime; }
      set { _startTime = value; }
    }

    public WorldDateTime EndTime
    {
      get { return _endTime; }
      set { _endTime = value; }
    }

    #endregion

    #region Public Methods

    public bool IsRemoved(List<ModifyInfo> actions)
    {
      for (int i = 0; i < actions.Count; i++)
      {
        if (actions[i].action == ModifyInfo.Action.Remove)
        {
          if ((actions[i].channel == "*" || actions[i].channel == _channelId))
          {
            string fieldText = GetElement(actions[i].field);

            if (fieldText != null && fieldText.Contains(actions[i].search))
            {
              return true;
            }
          }
        }
      }

      return false;
    }

    public void Replace(List<ModifyInfo> actions)
    {
      for (int i = 0; i < actions.Count; i++)
      {
        if (actions[i].action == ModifyInfo.Action.Replace)
        {
          if ((actions[i].channel == "*" || actions[i].channel == _channelId))
          {
            string fieldText = GetElement(actions[i].field);

            if (fieldText != null)
            {
              Regex replace = new Regex(actions[i].search);
              string newValue = replace.Replace(fieldText, actions[i].text);
              SetElement(actions[i].field, newValue);
            }
          }
        }
      }
    }

    public void Merge(ProgramData data)
    {
      if (data != null)
      {
        // Preference not yet set?
        if (this._preference == null)
        {
          this._preference = new DataPreference();
        }

        if (data._preference == null)
        {
          data._preference = new DataPreference();
        }

        // Merge values with Preference
        if (data._title != string.Empty &&
            (this._title == string.Empty || data._preference.Title > this._preference.Title))
        {
          this._title = data._title;
          this._preference.Title = data._preference.Title;
        }

        if (data._subTitle != string.Empty &&
            (this._subTitle == string.Empty || data._preference.Subtitle > this._preference.Subtitle))
        {
          this._subTitle = data._subTitle;
          this._preference.Subtitle = data._preference.Subtitle;
        }

        if (data._description != string.Empty &&
            (this._description == string.Empty || data._preference.Description > this._preference.Description))
        {
          this._description = data._description;
          this._preference.Description = data._preference.Description;
        }

        if (data._genre != string.Empty &&
            (this._genre == string.Empty || data._preference.Genre > this._preference.Genre))
        {
          this._genre = data._genre;
          this._preference.Genre = data._preference.Genre;
        }

        if (data._repeat)
        {
          this._repeat = data._repeat;
        }

        if (data._subtitles)
        {
          this._subtitles = data._subtitles;
        }

        if (data._episode > 0)
        {
          this._episode = data._episode;
        }

        if (data._season > 0)
        {
          this._season = data._season;
        }

        // Merge values without pPreference
        if (data._channelId != string.Empty && this._channelId == string.Empty)
        {
          this._channelId = data._channelId;
        }

        // Do not merge Date/Time ... causes
        //if (data._startTime != null && this._startTime == null)
        //  this._startTime = data._startTime;

        //if (data._endTime != null && this._endTime == null)
        //  this._endTime = data._endTime;
      }
    }

    public TVProgram ToTvProgram()
    {
      TVProgram program = new TVProgram();

      program.Channel = _channelId;
      program.Title = _title;
      program.Episode = _subTitle;
      program.Genre = _genre;
      program.Description = _description;
      program.Start = _startTime.ToLocalLongDateTime();
      if (_episode > 0)
      {
        program.EpisodeNum = _episode.ToString();
      }
      if (_season > 0)
      {
        program.SeriesNum = _season.ToString();
      }
      if (_repeat)
      {
        program.Repeat = "Repeat";
      }
      if (_endTime != null)
      {
        program.End = _endTime.ToLocalLongDateTime();
      }

      return program;
    }

    public bool HasSublink()
    {
      if (_sublink != null)
      {
        return true;
      }

      return false;
    }

    #endregion

    #region Private Methods

    private string GetElement(string tag)
    {
      switch (tag)
      {
        case "#DESCRIPTION":
          return _description;
        case "#TITLE":
          return _title;
        case "#SUBTITLE":
          return _subTitle;
        case "#GENRE":
          return _genre;
        default:
          break;
      }
      return null;
    }

    //private string[,] GetSearchParams(string SearchList)
    //{
    //  int pos = 0;
    //  int num = 0;
    //  int offset;

    //  while ((offset = SearchList.IndexOf(';', pos)) != -1)
    //  {
    //    pos = offset + 1;
    //    num++;
    //  }

    //  string[,] SearchParams = new string[num, 2];

    //  int startPos = 0;
    //  int endPos = 0;
    //  for (int i = 0; i < num; i++)
    //  {
    //    if ((startPos = SearchList.IndexOf('[', startPos)) != -1)
    //    {
    //      if ((endPos = SearchList.IndexOf(']', startPos + 1)) != -1)
    //      {
    //        SearchParams[i, 0] = SearchList.Substring(startPos + 1, endPos - startPos - 1);
    //      }
    //    }

    //    if ((startPos = SearchList.IndexOf('"', endPos)) != -1)
    //    {
    //      if ((endPos = SearchList.IndexOf('"', startPos + 1)) != -1)
    //      {
    //        SearchParams[i, 1] = SearchList.Substring(startPos + 1, endPos - startPos - 1);
    //      }
    //    }

    //    startPos = SearchList.IndexOf(';', startPos);
    //  }

    //  return SearchParams;
    //}

    private BasicTime GetTime(string strTime)
    {
      BasicTime time;

      try
      {
        time = new BasicTime(strTime);
      }
      catch (ArgumentOutOfRangeException)
      {
        return null;
      }
      return time;
    }

    private int GetMonth(string strMonth)
    {
      if (_months == null)
      {
        return int.Parse(strMonth);
      }
      else
      {
        return _months[strMonth];
      }
    }

    private List<string> GetActors(string strActors)
    {
      List<string> actorList = new List<string>();

      int index = 0;
      int start;
      char[] delimitors = new char[2] {',', '\n'};
      while ((start = strActors.IndexOfAny(delimitors, index)) != -1)
      {
        string actor = strActors.Substring(index, start - index);
        actorList.Add(actor.Trim(' ', '\n', '\t'));
        index = start + 1;
      }

      return actorList;
    }

    private int GetNumber(string element)
    {
      string number = string.Empty;
      int numberValue;
      bool found = false;

      for (int i = 0; i < element.Length; i++)
      {
        if (!found)
        {
          if (Char.IsDigit(element[i]))
          {
            number += element[i];
            found = true;
          }
        }
        else
        {
          if (Char.IsDigit(element[i]))
          {
            number += element[i];
          }
          else
          {
            break;
          }
        }
      }

      try
      {
        numberValue = Int32.Parse(number);
      }
      catch (Exception)
      {
        numberValue = 0;
      }

      return numberValue;
    }

    private void GetDate(string element)
    {
      if (_startTime == null)
      {
        _startTime = new WorldDateTime();
      }

      int pos = 0;
      if ((pos = element.IndexOf("/")) != -1)
      {
        _startTime.Day = Int32.Parse(element.Substring(0, pos));
      }

      int start = pos + 1;
      if ((pos = element.IndexOf("/", start)) != -1)
      {
        _startTime.Month = Int32.Parse(element.Substring(start, pos - start));
      }
    }

    /// <summary>
    /// Converts string which are all in UPPER case to proper case.
    /// </summary>
    /// <param name="value">The string.</param>
    /// <returns>string</returns>
    private string ProperCase(string value)
    {
      // test if value is all in UPPER case
      if (value == value.ToUpper())
      {
        // convert to Title case - dependant on culture
        TextInfo text = CultureInfo.CurrentCulture.TextInfo;
        return text.ToTitleCase(value.ToLower());
      }

      return value;
    }

    #endregion

    #region IParserData Implementations

    public void SetElement(string tag, string element)
    {
      try
      {
        switch (tag)
        {
          case "#STARTXMLTV":
            _startTime = new WorldDateTime(element, false);
            break;
          case "#ENDXMLTV":
            _endTime = new WorldDateTime(element, false);
            break;
          case "#START":
            BasicTime startTime = GetTime(element);
            if (_startTime == null)
            {
              _startTime = new WorldDateTime();
            }
            _startTime.Hour = startTime.Hour;
            _startTime.Minute = startTime.Minute;
            break;
          case "#END":
            BasicTime endTime = GetTime(element);
            if (_endTime == null)
            {
              _endTime = new WorldDateTime();
            }
            _endTime.Hour = endTime.Hour;
            _endTime.Minute = endTime.Minute;
            break;
          case "#DATE":
            GetDate(element);
            break;
          case "#DAY":
            if (_startTime == null)
            {
              _startTime = new WorldDateTime();
            }
            _startTime.Day = int.Parse(element);
            break;
          case "#DESCRIPTION":
            if (_description == string.Empty)
            {
              _description = element.Trim(' ', '\n', '\t');
            }
            else
            {
              _description = _description + "\n" + element.Trim(' ', '\n', '\t');
            }
            break;
          case "#MONTH":
            if (_startTime == null)
            {
              _startTime = new WorldDateTime();
            }
            _startTime.Month = GetMonth(element.Trim(' ', '\n', '\t'));
            break;
          case "#TITLE":
            Title = element.Trim(' ', '\n', '\t');
            break;
          case "#SUBTITLE":
            SubTitle = element.Trim(' ', '\n', '\t');
            break;
          case "#GENRE":
            Genre = element.Trim(' ', '\n', '\t');
            break;
          case "#ACTORS":
            _actors = GetActors(element);
            break;
          case "#EPISODE":
            _episode = GetNumber(element);
            break;
          case "#SEASON":
            _season = GetNumber(element);
            break;
          case "#REPEAT":
            if (element.ToLower().IndexOf("false") == -1)
            {
              _repeat = true;
            }
            break;
          case "#SUBTITLES":
            if (element.ToLower().IndexOf("false") == -1)
            {
              _subtitles = true;
            }
            break;
          default:
            break;
        }
      }
      catch (Exception)
      {
        GlobalServiceProvider.Instance.Get<ILog>().Error(LogType.WebEPG, "Parsing error {0} : {1}", tag, element);
      }
    }

    #endregion
  }
}