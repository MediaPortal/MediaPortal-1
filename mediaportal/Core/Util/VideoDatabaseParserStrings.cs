#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.ServiceImplementations;

namespace MediaPortal.Util
{
  public static class VideoDatabaseParserStrings
  {
    /// <summary>
    /// Sections:
    /// GUIVideoArtistInfo, FanArt, IMDBposter, IMDBActors,
    /// IMPAwardsposter, TMDBPosters, TMDBActorImages
    /// IMDBActorInfoMain, IMDBActorInfoDetails, IMDBActorInfoMovies
    /// </summary>
    /// <param name="section"></param>
    /// <returns></returns>
    public static string[] GetParserStrings(string section)
    {
      ArrayList result = new ArrayList();
      
      try
      {
        string parserIndexFile = Config.GetFile(Config.Dir.Config, "scripts\\VDBParserStrings.xml");
        string parserIndexUrl = @"http://install.team-mediaportal.com/MP1/VDBParserStrings.xml";
        XmlDocument doc = new XmlDocument();
        
        if (!File.Exists(parserIndexFile))
        {
          if (DownloadFile(parserIndexFile, parserIndexUrl) == false)
          {
            string parserIndexFileBase = Config.GetFile(Config.Dir.Base, "VDBParserStrings.xml");

            if (File.Exists(parserIndexFileBase))
            {
              File.Copy(parserIndexFileBase, parserIndexFile, true);
            }
            else
            {
              return result.ToArray(typeof(string)) as string[];
            }
          }
        }

        doc.Load(parserIndexFile);

        if (doc.DocumentElement != null)
        {
          string sec = "/section/" + section;
          XmlNode dbSections = doc.DocumentElement.SelectSingleNode(sec);
          
          if (dbSections == null)
          {
            return result.ToArray(typeof(string)) as string[];
          }

          XmlNodeList parserStrings = dbSections.SelectNodes("string");

          if (parserStrings != null)
          {
            foreach (XmlNode parserString in parserStrings)
            {
              result.Add(parserString.InnerText);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex.Message);
      }
      return result.ToArray(typeof(string)) as string[];
    }

    private static bool DownloadFile(string filepath, string url)
    {
      string parserTempFile = Path.GetTempFileName();

      try
      {
        if (File.Exists(parserTempFile))
        {
          File.Delete(parserTempFile);
        }

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        try
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          // request.Proxy = WebProxy.GetDefaultProxy();
          request.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        catch (Exception) { }
        
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
          using (Stream resStream = response.GetResponseStream())
          {
            using (TextReader tin = new StreamReader(resStream, Encoding.Default))
            {
              using (TextWriter tout = File.CreateText(parserTempFile))
              {
                while (true)
                {
                  string line = tin.ReadLine();
                  if (line == null)
                  {
                    break;
                  }
                  tout.WriteLine(line);
                }
              }
            }
          }
        }

        File.Delete(filepath);
        File.Move(parserTempFile, filepath);
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("EXCEPTION in DownloadFile | {0}\r\n{1}", ex.Message, ex.Source);
        return false;
      }
    }
  }
}
