/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.IO;
//using System.Drawing;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Net;
//using MediaPortal.Dialogs;
//using MediaPortal.GUI.Library;
//using MediaPortal.Util;

namespace MediaPortal.Webepg.TV.Database
{
	/// <summary>
	/// 
	/// </summary>
	public class XMLTVExport
	{
        XmlTextWriter m_writer;

        public XMLTVExport()
        {
        }

        public void Open()
        {
            m_writer = new XmlTextWriter("TVguide-writing.xml", System.Text.Encoding.UTF8);
            m_writer.Formatting = Formatting.Indented;
            //Write the <xml version="1.0"> element
            m_writer.WriteStartDocument();
            m_writer.WriteDocType("tv", null, "xmltv.dtd", null);
            m_writer.WriteStartElement("tv");
            m_writer.WriteAttributeString("generator-info-name", "WebEPG");
        }

        public void Close()
        {
            m_writer.WriteEndElement();
            m_writer.Flush();
            m_writer.Close();
			System.IO.File.Delete("TVguide.xml");
			System.IO.File.Move("TVguide-writing.xml","TVguide.xml");
        }

        public void WriteChannel(string id, string name)
        {
            m_writer.WriteStartElement("channel");
            m_writer.WriteAttributeString("id", id);
            m_writer.WriteElementString("display-name", name);
            m_writer.WriteEndElement();
        }

        public void WriteProgram(TVProgram program, int copy)
        {
			string channelid = program.Channel;
			if(copy > 0)
				channelid += copy.ToString();

            //Stream file = new Stream(
            if (program.Start != 0 &&
                program.Channel != String.Empty &&
                program.Title != String.Empty)
            {
                m_writer.WriteStartElement("programme");
                m_writer.WriteAttributeString("start", program.Start.ToString());
                if(program.End != 0)
                   m_writer.WriteAttributeString("stop", program.End.ToString());
                m_writer.WriteAttributeString("channel", channelid);

                m_writer.WriteStartElement("title");
                //m_writer.WriteAttributeString("lang", "de");
                m_writer.WriteString(program.Title);
                m_writer.WriteEndElement();

				string desc = "";
				if (program.Episode != String.Empty)
					desc += program.Episode + " ";

                if (program.Description != String.Empty)
					desc += program.Description;

				if(desc != "")
                   m_writer.WriteElementString("desc", desc);

				if (program.Genre != String.Empty)
					m_writer.WriteElementString("category", program.Genre);

                /*
                m_writer.WriteStartElement("credits");
                m_writer.WriteElementString("director", "xxx");
                m_writer.WriteElementString("actor", "xxx");
                m_writer.WriteElementString("actor", "xxx");
                m_writer.WriteElementString("actor", "xxx");
                m_writer.WriteEndElement();

                m_writer.WriteElementString("date", "xxx");
                m_writer.WriteElementString("country", "xxx");
                m_writer.WriteElementString("episode-num", "xxx");

                m_writer.WriteStartElement("video");
                m_writer.WriteElementString("aspect", "xxx");
                m_writer.WriteEndElement();

                m_writer.WriteStartElement("star-rating");
                m_writer.WriteElementString("value", "3/3");
                m_writer.WriteEndElement();
                */

                m_writer.WriteEndElement();
            }
        }
	}
}
