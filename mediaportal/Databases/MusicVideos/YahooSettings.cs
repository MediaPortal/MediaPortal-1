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
    public Dictionary<string, YahooSite> _yahooSiteTable;
    //public Hashtable _yahooSiteTable;
    public List<string> _bitRateList;
    public string _defaultBitRate;
    public string _defaultCountryName;

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
        if (_yahooSiteTable == null || _yahooSiteTable.Count < 1)
        {
          Log.Write("YahooSettings: loading settings");
          //_yahooSiteTable = new Dictionary<string, YahooSite>();
          _yahooSiteTable = new Dictionary<string, YahooSite>();
          _bitRateList = new List<string>();
          Log.Write("Yahoo Settings: {0}", _bitRateList == null);
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
                loSite._yahooSiteCountryName = loXmlreader.GetAttribute("name");
                loSite._yahooSiteCountryId = loXmlreader.GetAttribute("id");
                loSite._yahooSiteNewURL = loXmlreader.GetAttribute("newURL");
                loSite._yahooSiteTopURL = loXmlreader.GetAttribute("topURL");
                loSite._yahooSiteSearchURL = loXmlreader.GetAttribute("searchURL");
                loSite._yahooSiteGenreListURL = loXmlreader.GetAttribute("_yahooSiteGenreListURL");
                loSite._yahooSiteGenreURL = loXmlreader.GetAttribute("_yahooSiteGenreURL");
                _yahooSiteTable.Add(loSite._yahooSiteCountryName, loSite);
                if (loXmlreader.GetAttribute("default").Equals("Y"))
                {
                  _defaultCountryName = loSite._yahooSiteCountryName;
                }

                Log.Write("Yahoo Settings: Site created with name:{0},id={1},top={2},search={3}", loSite._yahooSiteCountryName, loSite._yahooSiteCountryId, loSite._yahooSiteTopURL, loSite._yahooSiteSearchURL);
              }
              else if (loXmlreader.Name == "bitrate")
              {
                //Log.Write("node type {0}", loXmlreader.NodeType);
                if (loXmlreader.NodeType == XmlNodeType.Element)
                {
                  if (loXmlreader.GetAttribute("default").Equals("Y"))
                  {
                    lsValue = loXmlreader.ReadString();
                    _bitRateList.Add(lsValue);
                    _defaultBitRate = lsValue;
                  }
                  else
                  {
                    lsValue = loXmlreader.ReadString();
                    _bitRateList.Add(lsValue);
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

        using (loReader = new XmlTextReader(filename))          
        {

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
            //Log.Write("current_country={0}-", _defaultCountryName);
            if (nameAttribute.Value == _defaultCountryName)
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

            if (lsCurrentBitRate == _defaultBitRate)
              defaultAttribute.Value = "Y";
            else
              defaultAttribute.Value = "N";
          }

          xmlDoc.Save(filename);

          //_yahooFavoriteList.Remove(video);

        }
      }
      catch (Exception e)
      {
        Log.Write(e);
        Log.Write("Yahoo Settings: save settings failed.");
      }
    }
  }

}