#region Copyright (C) 2005-2012 Team MediaPortal

// Copyright (C) 2005-2012 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;


// ReSharper disable CheckNamespace
namespace MediaPortal.Subtitle
// ReSharper restore CheckNamespace
{

  /// <summary>
  /// 
  /// </summary>
  // ReSharper disable InconsistentNaming
  public class SRTSubReader : ISubtitleReader
  // ReSharper restore InconsistentNaming
  {
    private readonly SubTitles _subtitles = new SubTitles();

    #pragma warning disable 1570
    /// <summary>
    ///  Regular expression built for C# on: Do, Dez 20, 2012, 01:03:00 
    ///  Using Expresso Version: 3.0.4334, http://www.ultrapico.com
    ///  
    ///  A description of the regular expression:
    ///  
    ///  Match expression but don't capture it. [\d+]
    ///      Any digit, one or more repetitions
    ///  \s*\r\n
    ///      Whitespace, any number of repetitions
    ///      Carriage return
    ///      New line
    ///  Match expression but don't capture it. [(?<startHH>\d+)\:(?<startMM>\d+)\:(?<startSS>\d+)(?:[,.](?<startMS>\d+))?]
    ///      (?<startHH>\d+)\:(?<startMM>\d+)\:(?<startSS>\d+)(?:[,.](?<startMS>\d+))?
    ///          [startHH]: A named capture group. [\d+]
    ///              Any digit, one or more repetitions
    ///          Literal :
    ///          [startMM]: A named capture group. [\d+]
    ///              Any digit, one or more repetitions
    ///          Literal :
    ///          [startSS]: A named capture group. [\d+]
    ///              Any digit, one or more repetitions
    ///          Match expression but don't capture it. [[,.](?<startMS>\d+)], zero or one repetitions
    ///              [,.](?<startMS>\d+)
    ///                  Any character in this class: [,.]
    ///                  [startMS]: A named capture group. [\d+]
    ///                      Any digit, one or more repetitions
    ///  \s*--\>\s*
    ///      Whitespace, any number of repetitions
    ///      --
    ///      Literal >
    ///      Whitespace, any number of repetitions
    ///  Match expression but don't capture it. [(?<endHH>\d+)\:(?<endMM>\d+)\:(?<endSS>\d+)(?:[,.](?<endMS>\d+))?]
    ///      (?<endHH>\d+)\:(?<endMM>\d+)\:(?<endSS>\d+)(?:[,.](?<endMS>\d+))?
    ///          [endHH]: A named capture group. [\d+]
    ///              Any digit, one or more repetitions
    ///          Literal :
    ///          [endMM]: A named capture group. [\d+]
    ///              Any digit, one or more repetitions
    ///          Literal :
    ///          [endSS]: A named capture group. [\d+]
    ///              Any digit, one or more repetitions
    ///          Match expression but don't capture it. [[,.](?<endMS>\d+)], zero or one repetitions
    ///              [,.](?<endMS>\d+)
    ///                  Any character in this class: [,.]
    ///                  [endMS]: A named capture group. [\d+]
    ///                      Any digit, one or more repetitions
    ///  \s*\r\n
    ///      Whitespace, any number of repetitions
    ///      Carriage return
    ///      New line
    ///  [text]: A named capture group. [[\s\S]*?]
    ///      Any character in this class: [\s\S], any number of repetitions, as few as possible
    ///  Match expression but don't capture it. [\s*\r\n|\z], one or more repetitions
    ///      Select from 2 alternatives
    ///          \s*\r\n
    ///              Whitespace, any number of repetitions
    ///              Carriage return
    ///              New line
    ///          End of string
    ///  Match expression but don't capture it. [\s*\r\n|\z]
    ///      Select from 2 alternatives
    ///          \s*\r\n
    ///              Whitespace, any number of repetitions
    ///              Carriage return
    ///              New line
    ///          End of string
    ///  
    ///
    /// </summary>
    #pragma warning restore 1570
    public static Regex SRTRegEx = new Regex(
          "(?:\\d+)\\s*\\r\\n(?:(?<startHH>\\d+)\\:(?<startMM>\\d+)\\:(" +
          "?<startSS>\\d+)(?:[,.](?<startMS>\\d+))?)\\s*--\\>\\s*(?:(?<" +
          "endHH>\\d+)\\:(?<endMM>\\d+)\\:(?<endSS>\\d+)(?:[,.](?<endMS" +
          ">\\d+))?)\\s*\\r\\n(?<text>[\\s\\S]*?)(?:\\s*\\r\\n|\\z)+(?:" +
          "\\s*\\r\\n|\\z)",
        RegexOptions.Multiline
        | RegexOptions.ECMAScript
        | RegexOptions.Compiled
        );
    
   

    /// <summary>
    /// 
    /// </summary>
    /// <param name="strFileName"></param>
    /// <returns></returns>
    public override bool SupportsFile(string strFileName)
    {
      string extension = Path.GetExtension(strFileName);
      return extension != null && extension.ToLower() == ".srt";
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strFileName"></param>
    /// <returns></returns>
    public override bool ReadSubtitles(string strFileName)
    {
      _subtitles.Clear(); // isn't this redundant as it is done by the Subtitle constructor already?

      using (var input = new StreamReader(strFileName, Encoding.GetEncoding(1252)))
      {
        // match file against SRT format
        MatchCollection matches = SRTRegEx.Matches(input.ReadToEnd());

        // process each match
        foreach (Match match in matches)
        {
          int startHH = Convert.ToInt32(match.Groups["startHH"].Value);
          int startMM = Convert.ToInt32(match.Groups["startMM"].Value);
          int startSS = Convert.ToInt32(match.Groups["startSS"].Value);
          int startMS = Convert.ToInt32(match.Groups["startMS"].Value);
          int endHH   = Convert.ToInt32(match.Groups["endHH"  ].Value);
          int endMM   = Convert.ToInt32(match.Groups["endMM"  ].Value);
          int endSS   = Convert.ToInt32(match.Groups["endSS"  ].Value);
          int endMS   = Convert.ToInt32(match.Groups["endMS"  ].Value);
          String text = match.Groups["text"   ].Value;

          // convert time stamps to milliseconds
          int startTime = startHH * 3600000 + startMM * 60000 + startSS * 1000 + startMS;
          int endTime   = endHH   * 3600000 + endMM   * 60000 + endSS   * 1000 + endMS;

          var newline = new SubTitles.Line {StartTime = startTime, EndTime = endTime, Text = text};
          _subtitles.Add(newline);
        }
      }
      return _subtitles.Count > 0;
    }


    /// <summary>
    /// 
    /// </summary>
    public override SubTitles Subs
    {
      get { return _subtitles; }
    }
  }
}