#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

using System.IO;
using System.Text;
using System.Xml;
using System;
using TvDatabase;
using MediaPortal.WebEPG.Parser;
using Gentle.Framework;
using MediaPortal.Utils.Time;

namespace MediaPortal.WebEPG
{
  /// <summary>
  ///
  /// </summary>
  public class XMLTVExport
    : IEpgDataSink
  {
    #region Variables

    private XmlTextWriter _writer;
    private string _xmltvTempFile;
    private string _xmltvFile;
    private string _currentChannelName;

    #endregion

    #region ctor

    public XMLTVExport(string dir)
    {
      _xmltvTempFile = Path.Combine(dir, "TVguide-writing.xml");
      _xmltvFile = Path.Combine(dir, "TVguide.xml");
    }

    #endregion

    #region private methods

    private int GetDBChannelId(string externalId)
    {
      Channel dbChannel = Broker.TryRetrieveInstance<Channel>(new Key(true, "ExternalId", externalId));
      if (dbChannel != null)
      {
        return dbChannel.IdChannel;
      }
      return 0;
    }

    #endregion

    #region IEPGDataSink implementation

    void IEpgDataSink.Open()
    {
      _writer = new XmlTextWriter(_xmltvTempFile, Encoding.UTF8);
      _writer.Formatting = Formatting.Indented;
      //Write the <xml version="1.0"> element
      _writer.WriteStartDocument();
      _writer.WriteDocType("tv", null, "xmltv.dtd", null);
      _writer.WriteStartElement("tv");
      _writer.WriteAttributeString("generator-info-name", "WebEPG");
      _currentChannelName = string.Empty;
    }

    void IEpgDataSink.Close()
    {
      _writer.WriteEndElement();
      _writer.Flush();
      _writer.Close();
      File.Delete(_xmltvFile);
      File.Move(_xmltvTempFile, _xmltvFile);
    }

    void IEpgDataSink.WriteChannel(string id, string name)
    {
      _writer.WriteStartElement("channel");
      _writer.WriteAttributeString("id", name + "-" + id);
      _writer.WriteElementString("display-name", name);
      _writer.WriteEndElement();
      _writer.Flush();
    }

    bool IEpgDataSink.StartChannelPrograms(string id, string name)
    {
      _currentChannelName = name;
      return true;
    }

    void IEpgDataSink.SetTimeWindow(TimeRange window) {}

    void IEpgDataSink.WriteProgram(ProgramData programData, bool merged)
    {
      string channelId = programData.ChannelId;
      if (merged)
      {
        channelId = "[Merged]";
      }

      if (programData.StartTime != null &&
          programData.ChannelId != string.Empty &&
          programData.Title != string.Empty)
      {
        _writer.WriteStartElement("programme");
        TimeSpan t = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        string tzOff = " " + t.Hours.ToString("+00;-00") + t.Minutes.ToString("00");
        _writer.WriteAttributeString("start", programData.StartTime.ToLocalTime().ToString("yyyyMMddHHmmss") + tzOff);
        if (programData.EndTime != null)
        {
          _writer.WriteAttributeString("stop", programData.EndTime.ToLocalTime().ToString("yyyyMMddHHmmss") + tzOff);
        }
        _writer.WriteAttributeString("channel", _currentChannelName + "-" + channelId);

        _writer.WriteStartElement("title");
        //_writer.WriteAttributeString("lang", "de");
        _writer.WriteString(programData.Title);
        _writer.WriteEndElement();

        if (programData.SubTitle != string.Empty && programData.SubTitle != "unknown")
        {
          _writer.WriteElementString("sub-title", programData.SubTitle);
        }

        if (programData.Description != string.Empty && programData.Description != "unknown")
        {
          _writer.WriteElementString("desc", programData.Description);
        }

        if (programData.Genre != string.Empty && programData.Genre != "-")
        {
          _writer.WriteElementString("category", programData.Genre);
        }

        string episodeNum = string.Empty;
        if (programData.Episode > 0)
        {
          episodeNum += programData.Episode;
        }

        if (programData.Season > 0)
        {
          episodeNum += "." + programData.Season;
        }

        if (episodeNum != string.Empty)
        {
          _writer.WriteElementString("episode-num", episodeNum);
        }

        //if (program.Repeat != string.Empty)
        //{
        //  _writer.WriteElementString("previously-shown", null);
        //}

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

    void IEpgDataSink.EndChannelPrograms(string id, string name) {}

    #endregion
  }
}