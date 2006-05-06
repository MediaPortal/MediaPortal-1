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
//using System.Collections.Generic;
using System.Text;
using System.Xml;
using MediaPortal.Utils.Web;
using MediaPortal.Webepg.GUI.Library;
using MediaPortal.Webepg.TV.Database;

namespace MediaPortal.WebEPG
{
  public class XMLProfiler : Profiler
  {
    XMLProfilerData _Data;
    //XmlDocument _xmlDoc;
    XmlNodeList _nodeList;
    HTTPRequest _page;

    public XMLProfiler(string strSource, XMLProfilerData data)
    {
      _strSource = strSource;
      _Data = data;
      if(_strSource != "")
        NodeProfiler();
    }

    public void SetChannelID(string id)
    {
      _Data.ChannelID = id;
    }

    override public Profiler GetPageProfiler(HTTPRequest page)
    {
      if(_page == null && page != _page)
      {
        HTMLPage webPage = new HTMLPage(page);
        _strSource = webPage.GetPage();
        _page = page;
      }
      return new XMLProfiler(_strSource, _Data);
    }

    override public MediaPortal.Utils.Web.Parser GetProfileParser(int index)
    {
      return null;
    }

    override public void GetParserData(int index, ref ParserData data)
    {
      ProgramData program = (ProgramData) data;

      XmlNode progNode = _nodeList.Item(index);
      if(progNode != null)
      {
        XmlNode node;
        if(_Data.TitleEntry != "" && (node = progNode.SelectSingleNode(_Data.TitleEntry)) != null)
          program.Title = node.InnerText;

        if(_Data.SubtitleEntry != "" && (node = progNode.SelectSingleNode(_Data.SubtitleEntry)) != null)
          program.SubTitle = node.InnerText;

        if(_Data.DescEntry != "" && (node = progNode.SelectSingleNode(_Data.DescEntry)) != null)
          program.Description = node.InnerText;

        if(_Data.GenreEntry != "" && (node = progNode.SelectSingleNode(_Data.GenreEntry)) != null)
          program.Genre = node.InnerText;

        if(_Data.StartEntry != "")
        {
          if((node = progNode.Attributes.GetNamedItem(_Data.StartEntry)) != null)
            program.StartTime = GetDateTime(node.InnerText);

          if((node = progNode.SelectSingleNode(_Data.StartEntry)) != null)
            program.StartTime = GetDateTime(node.InnerText);
        }

        if(_Data.EndEntry != "")
        {
          if((node = progNode.Attributes.GetNamedItem(_Data.EndEntry)) != null)
            program.EndTime = GetDateTime(node.InnerText);

          if((node = progNode.SelectSingleNode(_Data.EndEntry)) != null)
            program.EndTime = GetDateTime(node.InnerText);
        }
      }
    }

    private ProgramDateTime GetDateTime(string strDateTime)
    {
      ProgramDateTime time = new ProgramDateTime();
      if(strDateTime.Length > 4)
      {
        long ldate = long.Parse(strDateTime.Substring(0,14));
        ldate /=100L;
        time.Minute =(int)(ldate%100L);
        ldate /=100L;
        time.Hour =(int)(ldate%100L);
        ldate /= 100L;
        time.Day = (int)(ldate % 100L); 
        ldate /= 100L;
        time.Month = (int)(ldate % 100L); 
        ldate /= 100L;
        time.Year = (int)ldate;
      }
      else
      {
        int idate = int.Parse(strDateTime);
        time.Minute = idate%100;
        time.Hour = idate / 100;
      }
      return time;
    }

    private void NodeProfiler()
    {
      XmlDocument _xmlDoc = new XmlDocument();
      try
      {
        _xmlDoc.LoadXml(_strSource);
        if(_Data.ChannelEntry != "")
          _nodeList =  _xmlDoc.DocumentElement.SelectNodes(_Data.XPath + "[@" + _Data.ChannelEntry + "=\"" + _Data.ChannelID + "\"]");
        else
          _nodeList =  _xmlDoc.DocumentElement.SelectNodes(_Data.XPath);
      }
      catch(System.Xml.XmlException) // ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "WebEPG: XML failed");
        return;
      }

      if(_nodeList == null)
        Log.WriteFile(Log.LogType.Log, false, "WebEPG: No programs found");
      else
        _profileCount = _nodeList.Count;
    }
  }

}
