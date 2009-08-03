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
using System.Xml;
using MediaPortal.Utils.Web;
using MediaPortal.WebEPG.Config.Grabber;

namespace MediaPortal.WebEPG.Parser
{
  /// <summary>
  /// Parser for EPG in xml format
  /// </summary>
  public class XmlParser : IParser
  {
    #region Variables

    private XmlParserTemplate _data;
    //XmlDocument _xmlDoc;
    private XmlNodeList _nodeList;
    private HTTPRequest _page;
    private string _source;
    private string _channelName;
    private Type _dataType;

    #endregion

    #region Constructors/Destructors

    public XmlParser(XmlParserTemplate data)
    {
      _page = null;
      _data = data;
      _dataType = typeof (ProgramData);
    }

    #endregion

    #region Public Methods

    public void SetChannel(string name)
    {
      _channelName = name;
    }

    #endregion

    #region IParser Implementations

    public int ParseUrl(HTTPRequest page)
    {
      int count = 0;

      if (_page != page)
      {
        HTMLPage webPage = new HTMLPage(page);
        _source = webPage.GetPage();
        _page = new HTTPRequest(page);
      }

      XmlDocument _xmlDoc = new XmlDocument();
      try
      {
        _xmlDoc.LoadXml(_source);
        if (_data.Channel != string.Empty)
        {
          _nodeList =
            _xmlDoc.DocumentElement.SelectNodes(_data.XPath + "[@" + _data.Channel + "=\"" + _channelName + "\"]");
        }
        else
        {
          _nodeList = _xmlDoc.DocumentElement.SelectNodes(_data.XPath);
        }
      }
      catch (XmlException) // ex)
      {
        //Log.Error("WebEPG: XML failed");
        return count;
      }

      if (_nodeList != null)
      {
        count = _nodeList.Count;
      }

      return count;
    }

    public IParserData GetData(int index)
    {
      IParserData xmlData = (IParserData) Activator.CreateInstance(_dataType);

      XmlNode progNode = _nodeList.Item(index);
      if (progNode != null)
      {
        for (int i = 0; i < _data.Fields.Count; i++)
        {
          XmlField field = _data.Fields[i];

          XmlNode node;
          if ((node = progNode.SelectSingleNode(field.XmlName)) != null)
          {
            xmlData.SetElement(field.FieldName, node.InnerText);
          }
          if ((node = progNode.Attributes.GetNamedItem(field.XmlName)) != null)
          {
            xmlData.SetElement(field.FieldName, node.InnerText);
          }
        }
      }

      return xmlData;
    }

    //  override public void GetParserData(int index, ref ParserData data)
    //  {
    //    ProgramData program = (ProgramData) data;

    //    XmlNode progNode = _nodeList.Item(index);
    //    if(progNode != null)
    //    {
    //      XmlNode node;
    //      if(_Data.TitleEntry != "" && (node = progNode.SelectSingleNode(_Data.TitleEntry)) != null)
    //        program.Title = node.InnerText;

    //      if(_Data.SubtitleEntry != "" && (node = progNode.SelectSingleNode(_Data.SubtitleEntry)) != null)
    //        program.SubTitle = node.InnerText;

    //      if(_Data.DescEntry != "" && (node = progNode.SelectSingleNode(_Data.DescEntry)) != null)
    //        program.Description = node.InnerText;

    //      if(_Data.GenreEntry != "" && (node = progNode.SelectSingleNode(_Data.GenreEntry)) != null)
    //        program.Genre = node.InnerText;

    //      if(_Data.StartEntry != "")
    //      {
    //        if((node = progNode.Attributes.GetNamedItem(_Data.StartEntry)) != null)
    //          program.StartTime = GetDateTime(node.InnerText);

    //        if((node = progNode.SelectSingleNode(_Data.StartEntry)) != null)
    //          program.StartTime = GetDateTime(node.InnerText);
    //      }

    //      if(_Data.EndEntry != "")
    //      {
    //        if((node = progNode.Attributes.GetNamedItem(_Data.EndEntry)) != null)
    //          program.EndTime = GetDateTime(node.InnerText);

    //        if((node = progNode.SelectSingleNode(_Data.EndEntry)) != null)
    //          program.EndTime = GetDateTime(node.InnerText);
    //      }
    //    }
    //  }

    #endregion
  }
}