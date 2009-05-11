#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Text;
using System.IO;
using System.Xml;

namespace MediaPortal.Support
{
  public class HotFixInformationLogger : ILogCreator
  {
    public void CreateLogs(string destinationFolder)
    {
      string filename = Path.GetFullPath(destinationFolder) + "\\hotfixes.xml";
      HotfixInformation hotfixes = new HotfixInformation();

      using (XmlWriter writer = XmlWriter.Create(filename))
      {
        writer.WriteStartElement("hotfixes");
        foreach (HotfixItem item in hotfixes)
        {
          writer.WriteStartElement("hotfix");
          writer.WriteAttributeString("name", item.Name);
          writer.WriteAttributeString("category", item.Category);
          writer.WriteAttributeString("displayName", item.DisplayName);
          writer.WriteAttributeString("installDate", item.InstallDate);
          writer.WriteAttributeString("releaseType", item.ReleaseType);
          writer.WriteAttributeString("uninstallString", item.UninstallString);
          writer.WriteAttributeString("url", item.URL);
          writer.WriteEndElement();
        }
        writer.WriteEndElement();
      }
    }

    public string ActionMessage
    {
      get { return "Gathering hotfix information..."; }
    }
  }
}
