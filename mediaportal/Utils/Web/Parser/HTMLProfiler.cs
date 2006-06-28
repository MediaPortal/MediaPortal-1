/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Text.RegularExpressions;

namespace MediaPortal.Utils.Web
{
  public class HTMLProfiler : Profiler
  {
    Parser Template;
    int[,] _subProfile;
    int[,] _arrayTagPos;
    string _strTags;
    string _strSubProfile;
    string _strPageStart;
    string _strPageEnd;
    string _strEncoding = "";

    public HTMLProfiler(string strSource, string tags) //bool ahrefs)
    {
      _strSource = strSource.Replace("\r", "");
      _strSource = _strSource.Replace("\n", "");
      _strSource = _strSource.Replace("\t", "");
      _strTags = tags;
      BuildProfiler();
    }

    public HTMLProfiler(string strSource, string tags, string PageStart, string PageEnd, string encoding)
      : this(strSource, tags)
    {
      _strPageStart = PageStart;
      _strPageEnd = PageEnd;
      _strEncoding = encoding;
    }

    public HTMLProfiler(string strSource, string tags, string strSubProfile)
      : this(strSource, tags)
    {
      _strSubProfile = strSubProfile;
    }

    public string GetSource(int index)
    {
      int startTag = _subProfile[index, 0];
      int endTag = startTag + _subProfile[index, 1];
      int sourceStart = this._arrayTagPos[startTag, 0];
      int sourceLength = this._arrayTagPos[endTag, 1] - sourceStart + 1;
      return this._strSource.Substring(sourceStart, sourceLength);
    }

    override public Profiler GetPageProfiler(HTTPRequest page)
    {
      HTMLPage webPage = new HTMLPage(page, _strEncoding);
      string source = webPage.GetPage();
      HTMLProfiler retProfiler = null;

      if (source != null)
      {
        int startIndex = source.IndexOf(_strPageStart, 0);
        if (startIndex == -1)
        {
          // report Error
          startIndex = 0;
        }

        int endIndex = source.IndexOf(_strPageEnd, startIndex);

        if (endIndex == -1)
        {
          //report Error
          endIndex = source.Length;
        }

        source = source.Substring(startIndex, endIndex - startIndex);

        //			webPage.SetStart(_strPageStart);
        //			webPage.SetEnd(_strPageEnd);
        ////			if(!webPage.SetStart(_strPageStart))
        ////				//Log.WriteFile(Log.LogType.Log, true, "WebEPG: Start String not found");
        ////			if(!webPage.SetEnd(_strPageEnd))
        ////				//Log.WriteFile(Log.LogType.Log, true, "WebEPG: End String not found");
        retProfiler = new HTMLProfiler(source, _strTags, ProfileString());
        retProfiler.Template = GetProfileParser(0);
      }
      return retProfiler;
    }

    override public void GetParserData(int index, ref ParserData data)
    {
      Parser Source = this.GetProfileParser(index);
      Template.GetData(Source, ref data);
    }

    override public int subProfileCount()
    {
      if (_strProfile == null)
        return 0;

      int[] arraySubProfiles = new int[_strProfile.Length];
      int count = 0;
      int index = 0;
      int nextSubProfile = 0;

      while ((nextSubProfile = _strProfile.IndexOf(_strSubProfile, index)) != -1)
      {
        arraySubProfiles[count] = nextSubProfile;
        count++;
        index = nextSubProfile + _strSubProfile.Length - 1;
      }

      _subProfile = new int[count, 2];
      for (index = 0; index < count; index++)
      {
        _subProfile[index, 0] = arraySubProfiles[index];
        _subProfile[index, 1] = _strSubProfile.Length;
      }

      return count;
    }

    private int TagEnd(string strSource, int StartPos)
    {
      int index = 0;
      int nesting = 0;

      if (strSource[StartPos] == '<')
        index++;

      while (StartPos + index < strSource.Length)
      {
        if (strSource[StartPos + index] == '<')
          nesting++;
        if (strSource[StartPos + index] == '>')
        {
          if (nesting > 0)
            nesting--;
          else
            break;
        }
        index++;
      }

      return index;

    }

    protected string PreProcess(string strSource)
    {
      int index = 0;
      int tagLength;
      bool endTag;
      string strStripped = "";

      while (index < strSource.Length)
      {
        if (strSource[index] == '<')
        {
          endTag = false;
          if (strSource[index + 1] == '/')
          {
            index++;
            endTag = true;
          }
          char TagS = char.ToUpper(strSource[index + 1]);

          if (TagS == 'B' && char.ToUpper(strSource[index + 2]) == 'R')
          {
            strStripped += "<br>";

            while (index < strSource.Length &&
              strSource[index] != '>')
              index++;
            index++;
          }
          else
          {
            if (_strTags.IndexOf(TagS) != -1 || TagS == '#')
            {
              tagLength = TagEnd(strSource, index);
              if (endTag)
                strStripped += '<';

              int copyLength = tagLength;
              if (TagS != 'A')
              {
                int strip;
                if ((strip = strSource.IndexOf(' ', index, copyLength)) != -1)
                  copyLength = strip;
              }
              strStripped += strSource.Substring(index, copyLength);
              strStripped += '>';

              index += tagLength + 1;
            }
            else
            {
              tagLength = TagEnd(strSource, index);
              index += tagLength + 1;
            }
          }
        }
        else
        {
          if (strSource[index] != '\x06')
            strStripped += strSource[index];

          index++;
        }
      }

      return strStripped;
    }

    override public Parser GetProfileParser(int index)
    {
      Parser profileParser = new Parser(_subProfile[index, 1] * 2 - 1);

      int startTag = _subProfile[index, 0];
      int sourceStart = this._arrayTagPos[startTag, 0];
      int sourceLength = this._arrayTagPos[startTag, 1] - sourceStart + 1;
      string element = PreProcess(this._strSource.Substring(sourceStart, sourceLength));
      profileParser.Add(element);

      for (int i = 0; i < (_subProfile[index, 1] - 1); i++)
      {
        sourceStart = this._arrayTagPos[startTag + i, 1] + 1;
        sourceLength = this._arrayTagPos[startTag + i + 1, 0] - sourceStart;
        element = PreProcess(this._strSource.Substring(sourceStart, sourceLength));
        profileParser.Add(element);

        sourceStart = this._arrayTagPos[startTag + i + 1, 0];
        sourceLength = this._arrayTagPos[startTag + i + 1, 1] - sourceStart + 1;
        element = PreProcess(this._strSource.Substring(sourceStart, sourceLength));
        profileParser.Add(element);
      }

      return profileParser;
    }

    public string SearchRegex(int index, string regex, bool remove)
    {
      int startTag = _subProfile[index, 0];
      int endTag = _subProfile[index, 1];
      int sourceStart = this._arrayTagPos[startTag, 0];
      int sourceLength = this._arrayTagPos[startTag + endTag, 1] - sourceStart + 1;

      Match result = null;
      try
      {
        Regex searchRegex = new Regex(regex);
        result = searchRegex.Match(_strSource.ToLower(), sourceStart, sourceLength);
      }
      catch (System.ArgumentException)// ex)
      {
        //Log.WriteFile(Log.LogType.Log, true, "WebEPG: Regex error: {0} {1}", regex, ex.ToString());
        return "";
      }

      if (result.Success)
      {
        if (remove)
        {
          char[] sourceArray = _strSource.ToCharArray();
          for (int i = result.Index; i < result.Index + result.Length; i++)
            sourceArray[i] = '\x06';
          _strSource = new string(sourceArray);
        }
        return _strSource.Substring(result.Index, result.Length);
      }

      return "";
    }

    public bool GetHyperLink(int profileIndex, string match, ref HTTPRequest linkURL)
    {

      string regex = "<a href=[^>]*" + match.ToLower() + "[^>]*>";

      string result = SearchRegex(profileIndex, regex, false);

      bool linkFound = false;
      string strLinkURL = string.Empty;

      if (result != "")
      {
        int start = -1;
        char delim = '>';

        if ((start = result.IndexOf("=")) != -1)
        {
          for (int i = 0; i < result.Length - start; i++)
          {
            if (result[start + i] == '\"' || result[start + i] == '\'')
            {
              delim = result[start + i];
              break;
            }
          }
        }

        int end = -1;
        if (delim != '>')
        {
          start = -1;
          start = result.IndexOf(delim);
        }
        if (start != -1)
          end = result.IndexOf(delim, ++start);
        if (end != -1)
        {
          strLinkURL = result.Substring(start, end - start);
          linkFound = true;
        }
      }

      //if(strLinkURL.ToLower().IndexOf("http") == -1)
      //{
      if (strLinkURL.ToLower().IndexOf("javascript") != -1)
      {
        string[] param = GetJavaSubLinkParams(strLinkURL);

        for (int i = 0; i < param.Length; i++)
          linkURL.ReplaceTag("[" + (i + 1).ToString() + "]", param[i]);
      }
      else
      {
        linkURL = linkURL.Add(strLinkURL.Trim());
      }
      //}

      return linkFound;
    }

    private string[] GetJavaSubLinkParams(string link)
    {

      int args = -1;
      int[,] param = null;
      int start = -1;

      if ((start = link.IndexOf("(")) != -1)
      {
        args = 0;
        param = new int[link.Length - start, 2];
        param[0, 0] = start + 1;
        for (int i = 0; i < link.Length - start; i++)
        {
          if (link[start + i] == ',')
          {
            param[args, 1] = start + i;
            args++;
            param[args, 0] = start + i + 1;
          }
          if (link[start + i] == ')')
          {
            param[args, 1] = start + i;
            break;
          }
        }
      }

      string[] array = null;
      if (args != -1 && param != null)
      {
        args++;
        array = new string[args];
        for (int i = 0; i < args; i++)
          array[i] = link.Substring(param[i, 0], param[i, 1] - param[i, 0]).Trim('\"', '\'');
      }

      return array;
    }

    private void BuildProfiler()
    {
      if (_strSource.Length == 0)
        return;

      int index = 0;
      int nextTag = 0;
      int profileIndex = 0;
      int tagLength;
      char tag;
      char tagS;
      bool endTag;

      int[,] arrayProfilePos = new int[_strSource.Length, 2];
      char[] arrayProfile = new char[_strSource.Length];


      while (index < _strSource.Length && (nextTag = _strSource.IndexOf('<', index)) != -1)
      {
        arrayProfilePos[profileIndex, 0] = nextTag;

        nextTag++;

        endTag = false;
        if (_strSource[nextTag] == '/')
        {
          nextTag++;
          endTag = true;
        }


        tag = char.ToUpper(_strSource[nextTag]);
        tagS = tag;

        if (tag == 'T')
        {
          nextTag++;
          if (char.ToUpper(_strSource[nextTag]) != 'A')
            tag = char.ToUpper(_strSource[nextTag]);
        }

        tagLength = TagEnd(_strSource, nextTag);
        nextTag += tagLength;

        arrayProfilePos[profileIndex, 1] = nextTag;

        if (endTag)
          arrayProfile[profileIndex] = tag;
        else
          arrayProfile[profileIndex] = char.ToLower(tag);

        if (_strTags.IndexOf(tagS) != -1)
          profileIndex++;

        index = nextTag + 1;
      }


      _strProfile = "";
      _arrayTagPos = new int[profileIndex, 2];
      for (index = 0; index < profileIndex; index++)
      {
        _strProfile += arrayProfile[index];
        _arrayTagPos[index, 0] = arrayProfilePos[index, 0];
        _arrayTagPos[index, 1] = arrayProfilePos[index, 1];
      }

      if (_subProfile == null)
      {
        _subProfile = new int[1, 2];
        _subProfile[0, 0] = 0;
        _subProfile[0, 1] = _strProfile.Length;
      }
    }
  }
}
