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
  public class DataProfiler : Profiler
  {
    Parser Template;
    int[,] _subProfile;
    int[,] _arrayTagPos;
    char _cTag;
    char _cDelim;

    public DataProfiler(string strSource, char Tag, char Delim)
    {
      _strSource = strSource;
      _strProfile = strSource;
      _cTag = Tag;
      _cDelim = Delim;
      ProfilerStart();
    }

    public string GetSource(int index)
    {
      int startTag = _subProfile[index,0];
      int endTag = startTag + _subProfile[index,1];
      int sourceStart = this._arrayTagPos[startTag,0];
      int sourceLength = this._arrayTagPos[endTag,1] - sourceStart + 1;
      return this._strSource.Substring(sourceStart, sourceLength);
    }

    override public Profiler GetPageProfiler(HTTPRequest page)
    {
      HTMLPage webPage = new HTMLPage(page);
      DataProfiler retProfiler = new DataProfiler(webPage.GetPage(), _cTag, _cDelim);
      retProfiler.Template = GetProfileParser(0);
      return retProfiler;
    }

    override public void GetParserData(int index, ref ParserData data)
    {
      MediaPortal.Utils.Web.Parser Listing = this.GetProfileParser(index);
      Template.GetData(Listing, ref data);
    }

    override public MediaPortal.Utils.Web.Parser GetProfileParser(int index)
    {
      MediaPortal.Utils.Web.Parser profileParser = new MediaPortal.Utils.Web.Parser(_subProfile[index,1]*2);

      int startTag = _subProfile[index,0];

      int sourceStart = 0;
      int sourceLength = this._arrayTagPos[startTag,0];
      if(index > 0)
      {
        sourceStart = this._arrayTagPos[startTag-1,1] + 1;
        sourceLength = this._arrayTagPos[startTag,0] - sourceStart;
      }

      profileParser.Add(this._strSource.Substring(sourceStart, sourceLength));

      sourceStart = this._arrayTagPos[startTag,0];
      sourceLength = this._arrayTagPos[startTag,1] - sourceStart + 1;
      profileParser.Add(this._strSource.Substring(sourceStart, sourceLength));

      int i;
      for(i = 0; i < (_subProfile[index,1] - 1); i++)
      {
        sourceStart = this._arrayTagPos[startTag+i, 1] + 1;
        sourceLength = this._arrayTagPos[startTag+i+1, 0] - sourceStart;
        profileParser.Add(this._strSource.Substring(sourceStart, sourceLength));

        sourceStart = this._arrayTagPos[startTag+i+1,0];
        sourceLength = this._arrayTagPos[startTag+i+1,1] - sourceStart + 1;
        profileParser.Add(this._strSource.Substring(sourceStart, sourceLength));
      }

      return profileParser;
    }

    private void ProfilerStart()
    {
      if (_strSource.Length == 0)
        return;

      int index = 0;
      int profileIndex = 0;
      int tagCount = 0;
      int subProfileStart = 0;
      int subProfileIndex = 0;

      int [,] arrayProfilePos = new int[_strSource.Length, 2];
      int [,] subProfilePos = new int[_strSource.Length, 2];

      for(index = 0; index < _strSource.Length; index++)
      {
        if(_strSource[index] == _cTag)
        {
          arrayProfilePos[profileIndex,0] = index;
          arrayProfilePos[profileIndex,1] = index;
          profileIndex++;
          tagCount++;
        }

        if(_strSource[index] == _cDelim)
        {
          arrayProfilePos[profileIndex,0] = index;
          arrayProfilePos[profileIndex,1] = index;
          profileIndex++;
          tagCount++;
          subProfilePos[subProfileIndex,0] = subProfileStart;
          subProfilePos[subProfileIndex,1] = tagCount;
          subProfileStart=profileIndex;
          tagCount=0;
          subProfileIndex++;
        }
      }

      _arrayTagPos = new int[profileIndex,2];
      for (index = 0; index < profileIndex; index++)
      {
        _arrayTagPos[index,0] = arrayProfilePos[index, 0];
        _arrayTagPos[index,1] = arrayProfilePos[index, 1];
      }

      _profileCount = subProfileIndex;
      _subProfile = new int[subProfileIndex,2];
      for (index = 0; index < subProfileIndex; index++)
      {
        _subProfile[index,0] = subProfilePos[index, 0];
        _subProfile[index,1] = subProfilePos[index, 1];
      }
    }
  }
}
