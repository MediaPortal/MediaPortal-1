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
using System.Windows.Forms;
using System.Xml;
using System.Net;
using System.IO;

using TvLibrary.Log;

namespace SetupTv
{
  public static class HelpSystem
  {
    static string helpReferencesFile = String.Format(@"{0}\HelpReferences.xml", Log.GetPathName());
    static string helpReferencesTemp = String.Format(@"{0}_temp", helpReferencesFile);
    static string helpReferencesURL = @"http://install.team-mediaportal.com/HelpReferences_TVServer.xml";

    public static void ShowHelp(string sectionName)
    {
      if (!System.IO.File.Exists(helpReferencesFile))
      {
        MessageBox.Show("No help reference found.\r\nPlease update your help references by pressing 'Update Help' on Project Section.");
        return;
      }

      XmlDocument doc = new XmlDocument();
      doc.Load(helpReferencesFile);

      XmlNode generalNode = doc.SelectSingleNode("/helpsystem/general");
      XmlNodeList sectionNodes = doc.SelectNodes("/helpsystem/sections/section");

      for (int i = 0; i < sectionNodes.Count; i++)
      {
        XmlNode sectionNode = sectionNodes[i];
        if (sectionNode.Attributes["name"].Value == sectionName)
        {
          System.Diagnostics.Process.Start(
            String.Format(@"{0}{1}",
            generalNode.Attributes["baseurl"].Value,
            sectionNode.Attributes["suburl"].Value));
          return;
        }
      }

      MessageBox.Show(String.Format("No help reference found for section: {0}\r\n\r\nPlease update your help references by pressing 'Update Help' on Project Section.", sectionName));
    }

    public static void UpdateHelpReferences()
    {
      Application.DoEvents();
      try
      {
        if (File.Exists(helpReferencesTemp))
          File.Delete(helpReferencesTemp);

        Application.DoEvents();
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(helpReferencesURL);
        Application.DoEvents();

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
          Application.DoEvents();
          using (Stream resStream = response.GetResponseStream())
          {
            using (System.IO.TextReader tin = new StreamReader(resStream))
            {
              using (System.IO.TextWriter tout = System.IO.File.CreateText(helpReferencesTemp))
              {
                while (true)
                {
                  string line = tin.ReadLine();
                  if (line == null) break;
                  tout.WriteLine(line);
                }
              }
            }
          }
        }

        System.IO.File.Delete(helpReferencesFile);
        System.IO.File.Move(helpReferencesTemp, helpReferencesFile);

        MessageBox.Show("HelpReferences update succeeded.");
      }
      catch (Exception)
      {
        MessageBox.Show("HelpReferences update failed.");
      }
      Application.DoEvents();
    }
  }
}
