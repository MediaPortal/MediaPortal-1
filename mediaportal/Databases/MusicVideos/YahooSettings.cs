#region Copyright (C) 2006 Team MediaPortal

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

#endregion

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using MediaPortal.GUI.Library;
using System.Xml;

namespace MediaPortal.MusicVideos.Database
{
  public class YahooSettings
  {
    private static YahooSettings instance = new YahooSettings();
    public Dictionary<string, YahooSite> moYahooSiteTable;
    //public Hashtable moYahooSiteTable;
    public List<string> moBitRateList;
    public string msDefaultBitRate;
    public string msDefaultCountryName;

    private YahooSettings()
    {
      loadSettings();
    }

    public static YahooSettings getInstance()
    {
      if (instance == null)
      {
        instance = new YahooSettings();
      }
      return instance;
    }
    public void loadSettings()
    {
      XmlTextReader loXmlreader = null;
      try
      {
        if (moYahooSiteTable == null || moYahooSiteTable.Count < 1)
        {
          Log.Write("YahooSettings: loading settings");
          //moYahooSiteTable = new Dictionary<string, YahooSite>();
          moYahooSiteTable = new Dictionary<string, YahooSite>();
          moBitRateList = new List<string>();
          Log.Write("Yahoo Settings: {0}", moBitRateList == null);
          YahooSite loSite;
          string lsValue;
          using (loXmlreader = new XmlTextReader("MusicVideoSettings.xml"))
          //using (MediaPortal.Profile.Settings loXmlreader = new MediaPortal.Profile.Settings("MusicVideoSettings.xml"))
          {
            while (loXmlreader.Read())
            {
              if (loXmlreader.NodeType == XmlNodeType.Element && loXmlreader.Name == "country")
              {
                loSite = new YahooSite();
                //Log.Write("found country {0}", loXmlreader.GetAttribute("name"));
                //Log.Write("Yahoo Settings: found country {0}", loXmlreader.GetValueAsString("name"));
                loSite.countryName = loXmlreader.GetAttribute("name");
                loSite.countryId = loXmlreader.GetAttribute("id");
                loSite.NewURL = loXmlreader.GetAttribute("newURL");
                loSite.TopURL = loXmlreader.GetAttribute("topURL");
                loSite.SearchURL = loXmlreader.GetAttribute("searchURL");
                loSite.GenreListURL = loXmlreader.GetAttribute("GenreListURL");
                loSite.GenreURL = loXmlreader.GetAttribute("GenreURL");
                moYahooSiteTable.Add(loSite.countryName, loSite);
                if (loXmlreader.GetAttribute("default").Equals("Y"))
                {
                  msDefaultCountryName = loSite.countryName;
                }

                Log.Write("Yahoo Settings: Site created with name:{0},id={1},top={2},search={3}", loSite.countryName, loSite.countryId, loSite.TopURL, loSite.SearchURL);
              }
              else if (loXmlreader.Name == "bitrate")
              {
                //Log.Write("node type {0}", loXmlreader.NodeType);
                if (loXmlreader.NodeType == XmlNodeType.Element)
                {
                  if (loXmlreader.GetAttribute("default").Equals("Y"))
                  {
                    lsValue = loXmlreader.ReadString();
                    moBitRateList.Add(lsValue);
                    msDefaultBitRate = lsValue;
                  }
                  else
                  {
                    lsValue = loXmlreader.ReadString();
                    moBitRateList.Add(lsValue);
                  }
                }

              }

            }
          }
        }
      }
      catch (Exception e)
      {
        Log.Write(e);
      }
      finally
      {
        try
        {
          loXmlreader.Close();

          Log.Write("Yahoo Settings: load settings closed");
          loXmlreader = null;
        }
        catch (Exception ex)
        {
          Log.Write("Yahoo Settings: Exception - {0}", ex);
        }
      }
    }
    public void saveSettings()
    {
      XmlTextReader loReader = null;
      Log.Write("Yahoo Settings: saving settings.");
      try
      {
        string filename = "MusicVideoSettings.xml";

        loReader = new XmlTextReader(filename);

        XmlDocument xmlDoc = new XmlDocument();

        try
        {
          xmlDoc.Load(loReader);
          loReader.Close();
        }
        catch (System.IO.FileNotFoundException)
        {
          Log.Write("Yahoo Settings: MusicVideoSettings.xml not found.");
          //if file is not found, create a new xml file
          //XmlTextWriter xmlWriter = new XmlTextWriter(filename, System.Text.Encoding.UTF8);
          //xmlWriter.Formatting = Formatting.Indented;
          //xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
          //xmlWriter.WriteStartElement("FAVOURITES");

          //xmlWriter.Close();
          //xmlDoc.Load(filename);
          return;
        }
        XmlNode root = xmlDoc.DocumentElement;

        XmlNodeList xmlNodeList = xmlDoc.GetElementsByTagName("country");

        XmlAttribute defaultAttribute;
        XmlAttribute nameAttribute;
        foreach (XmlNode xmlNode in xmlNodeList)
        {
          nameAttribute = xmlNode.Attributes["name"];

          defaultAttribute = xmlNode.Attributes["default"];

          //Log.Write("name attribute={0}-", nameAttribute.Value);
          //Log.Write("current_country={0}-", msDefaultCountryName);
          if (nameAttribute.Value == msDefaultCountryName)
            defaultAttribute.Value = "Y";
          else
            defaultAttribute.Value = "N";
        }

        xmlNodeList = xmlDoc.GetElementsByTagName("bitrate");

        string lsCurrentBitRate;
        foreach (XmlNode xmlNode in xmlNodeList)
        {
          defaultAttribute = xmlNode.Attributes["default"];
          lsCurrentBitRate = xmlNode.InnerXml;

          if (lsCurrentBitRate == msDefaultBitRate)
            defaultAttribute.Value = "Y";
          else
            defaultAttribute.Value = "N";
        }

        xmlDoc.Save(filename);

        //moFavoriteList.Remove(video);

      }
      catch (Exception e)
      {
        Log.Write(e);
        Log.Write("Yahoo Settings: save settings failed.");
      }
      finally
      {
        try
        {
          loReader.Close();
          Log.Write("Yahoo Settings: save settings - reader closed");
          loReader = null;
        }
        catch (Exception ex)
        {
          Log.Write("Yahoo Settings: Exception - {0}", ex);
        }
      }
    }
  }

}