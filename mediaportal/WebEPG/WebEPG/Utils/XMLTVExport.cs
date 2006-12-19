#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Net;
using MediaPortal.TV.Database;

namespace MediaPortal.WebEPG
{
  /// <summary>
  ///
  /// </summary>
  public class XMLTVExport
  {
    XmlTextWriter _writer;
    string _xmltvTempFile;
    string _xmltvFile;

    public XMLTVExport(string dir)
    {
      _xmltvTempFile = Path.Combine(dir, "TVguide-writing.xml");
      _xmltvFile = Path.Combine(dir, "TVguide.xml");
    }

    public void Open()
    {
      _writer = new XmlTextWriter(_xmltvTempFile, System.Text.Encoding.UTF8);
      _writer.Formatting = Formatting.Indented;
      //Write the <xml version="1.0"> element
      _writer.WriteStartDocument();
      _writer.WriteDocType("tv", null, "xmltv.dtd", null);
      _writer.WriteStartElement("tv");
      _writer.WriteAttributeString("generator-info-name", "WebEPG");
    }

    public void Close()
    {
      _writer.WriteEndElement();
      _writer.Flush();
      _writer.Close();
      System.IO.File.Delete(_xmltvFile);
      System.IO.File.Move(_xmltvTempFile, _xmltvFile);
    }

    public void WriteChannel(string id, string name)
    {
      _writer.WriteStartElement("channel");
      _writer.WriteAttributeString("id", id);
      _writer.WriteElementString("display-name", name);
      _writer.WriteEndElement();
      _writer.Flush();
    }

    public void WriteProgram(TVProgram program, int copy)
    {
      string channelid = program.Channel;
      if(copy > 0)
        channelid += copy.ToString();

      if (program.Start != 0 &&
          program.Channel != String.Empty &&
          program.Title != String.Empty)
      {
        _writer.WriteStartElement("programme");
        _writer.WriteAttributeString("start", program.Start.ToString());
        if(program.End != 0)
          _writer.WriteAttributeString("stop", program.End.ToString());
        _writer.WriteAttributeString("channel", channelid);

        _writer.WriteStartElement("title");
        //_writer.WriteAttributeString("lang", "de");
        _writer.WriteString(program.Title);
        _writer.WriteEndElement();

        if (program.Episode != String.Empty && program.Episode != "unknown")
          _writer.WriteElementString("sub-title", program.Episode);

        if (program.Description != String.Empty && program.Description != "unknown")
          _writer.WriteElementString("desc", program.Description);

        if (program.Genre != String.Empty && program.Genre != "-")
          _writer.WriteElementString("category", program.Genre);

        string episodeNum = String.Empty;
        if (program.EpisodeNum != String.Empty)
          episodeNum += program.EpisodeNum;

        if (program.SeriesNum != String.Empty)
          episodeNum += "." + program.SeriesNum;

        if(episodeNum != String.Empty)
          _writer.WriteElementString("episode-num", episodeNum);

        if (program.Repeat != String.Empty)
          _writer.WriteElementString("previously-shown", null);

        /*
          _writer.WriteStartElement("credits");
          _writer.WriteElementString("director", "xxx");
          _writer.WriteElementString("actor", "xxx");
          _writer.WriteElementString("actor", "xxx");
          _writer.WriteElementString("actor", "xxx");
          _writer.WriteEndElement();

          _writer.WriteElementString("date", "xxx");
          _writer.WriteElementString("country", "xxx");
          _writer.WriteElementString("episode-num", "xxx");

          _writer.WriteStartElement("video");
          _writer.WriteElementString("aspect", "xxx");
          _writer.WriteEndElement();

          _writer.WriteStartElement("star-rating");
          _writer.WriteElementString("value", "3/3");
          _writer.WriteEndElement();
          */

        _writer.WriteEndElement();
        _writer.Flush();
      }
    }
  }
}
