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
using System.Xml;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.MusicVideos.Database;
using MediaPortal.Utils.Services;


namespace MediaPortal.MusicVideos.Database
{
  public class YahooFavorites
  {
    public List<YahooVideo> moFavoriteList = new List<YahooVideo>();
    //protected Dictionary<string, List<YahooVideo>> moFavoriteTable;
    protected bool mbNameEnabled = false;
    protected string msSelectedFavoriteName = "Default";
    static IConfig _config;
    static ILog _log;

    public YahooFavorites()
    {
      ServiceProvider loServices = GlobalServiceProvider.Instance;
      _log = loServices.Get<ILog>();
      _config = loServices.Get<IConfig>();
    }

    public ArrayList getFavoriteNames()
    {
      MusicVideoDatabase loDb = MusicVideoDatabase.getInstance();
      return loDb.getFavorites();
    }
    public void setSelectedFavorite(string fsFavoriteName)
    {
      msSelectedFavoriteName = fsFavoriteName;
    }
    public string getSelectedFavorite()
    {
      return msSelectedFavoriteName;
    }
    public List<YahooVideo> getFavoriteVideos()
    {
      MusicVideoDatabase loDb = MusicVideoDatabase.getInstance();
      return loDb.getFavoriteVideos(msSelectedFavoriteName);
    }

    public void loadFavorites()
    {
      //if (moFavoriteList == null || moFavoriteList.Count < 1)
      //{
      //    moFavoriteList = new List<YahooVideo>();
      //    moFavoriteTable = new Dictionary<string,List<YahooVideo>>();
      //    string lsCurrentName = msDefaultFavoriteName;
      //    try
      //    {
      //        XmlTextReader loXmlreader = new XmlTextReader("plugins/windows/MusicVideoFavorites.xml");
      //        YahooVideo loVideo;
      //        //loXmlreader.
      //        while (loXmlreader.Read())
      //        {
      //            if(loXmlreader.NodeType == XmlNodeType.Element && loXmlreader.Name == "Favorite"){
      //                mbNameEnabled = true;
      //                if (moFavoriteList.Count > 0)
      //                {
      //                    moFavoriteTable.Add(lsCurrentName, moFavoriteList);
      //                }
      //                lsCurrentName = loXmlreader.GetAttribute("name");
      //                Log.Write("Xml name = {0}",lsCurrentName);


      //            }
      //            if (loXmlreader.NodeType == XmlNodeType.Element && loXmlreader.Name == "SONG")
      //            {
      //                loVideo = new YahooVideo();
      //                loVideo.artistId = loXmlreader.GetAttribute("ArtistId");
      //                loVideo.artistName = loXmlreader.GetAttribute("Artist");
      //                loVideo.songId = loXmlreader.GetAttribute("SongId");
      //                loVideo.songName = loXmlreader.GetAttribute("SongTitle");
      //                loVideo.countryId = loXmlreader.GetAttribute("CtryId");
      //                Log.Write("found favorite:{0}", loVideo.ToString());
      //                moFavoriteList.Add(loVideo);
      //            }
      //        }
      //        loXmlreader.Close();
      //    }
      //    catch (Exception e) { Log.Write(e); }
      //}
      //if (!mbNameEnabled)
      //{
      //    reformatFavoriteFile();
      //}
    }
    private void reformatFavoriteFile()
    {

      try
      {
        string filename = _config.Get(Config.Options.ConfigPath) + "MusicVideoFavorites.xml";
        string newfilename = _config.Get(Config.Options.ConfigPath) + "MusicVideoFavorites.xml.arc";
        //rename the file
        File.Move(filename, newfilename);

        XmlDocument xmlDoc1 = new XmlDocument();
        XmlDocument xmlDoc2 = new XmlDocument();
        try
        {
          xmlDoc1.Load(newfilename);

          //create the new format favorite file
          XmlTextWriter xmlWriter = new XmlTextWriter(filename, System.Text.Encoding.UTF8);
          xmlWriter.Formatting = Formatting.Indented;
          xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
          xmlWriter.WriteStartElement("FAVOURITES");

          xmlWriter.Close();
          xmlDoc2.Load(filename);
        }
        catch (System.IO.FileNotFoundException)
        {
          _log.Debug("in FileNotFound.");
          //if file is not found, create a new xml file
          XmlTextWriter xmlWriter = new XmlTextWriter(filename, System.Text.Encoding.UTF8);
          xmlWriter.Formatting = Formatting.Indented;
          xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
          xmlWriter.WriteStartElement("FAVOURITES");

          xmlWriter.Close();
          xmlDoc1.Load(filename);
        }

        XmlNode root1 = xmlDoc1.DocumentElement;
        XmlNode root2 = xmlDoc2.DocumentElement;
        XmlNodeList loSongNodes = xmlDoc1.GetElementsByTagName("SONG");
        XmlElement loNewFavoriteNode = xmlDoc2.CreateElement("Favorite");
        //XmlAttribute loFavAttributeName = xmlDoc2.CreateAttribute("name");
        //loFavAttributeName.Value = "default";
        //loNewFavoriteNode.Attributes.Append(loFavAttributeName);
        loNewFavoriteNode.SetAttribute("name", "default");
        XmlNode loNewNode;
        foreach (XmlNode loSongNode in loSongNodes)
        {
          loNewNode = xmlDoc2.ImportNode(loSongNode, true);
          loNewFavoriteNode.AppendChild(loNewNode);
          //loSongNode.ParentNode = loNewFavoriteNode;
          //root.RemoveChild(loSongNode);
          //loNewFavoriteNode.AppendChild(loSongNode);
        }
        //root.RemoveAll();

        root2.AppendChild(loNewFavoriteNode);
        xmlDoc2.Save(filename);


      }
      catch (Exception e)
      {
        _log.Error(e);
        _log.Error("failed.");
      }

    }
    public void addFavorite(YahooVideo video)
    {

      _log.Debug("in addFav.");
      try
      {
        MusicVideoDatabase loDb = MusicVideoDatabase.getInstance();
        bool lbAdded = loDb.addFavoriteVideo(msSelectedFavoriteName, video);
        if (lbAdded)
        {
          moFavoriteList.Add(video);
        }
        else
        {
          _log.Error("{0} could not be added to favorite {1}.", video.songName, msSelectedFavoriteName);
        }

      }
      catch (Exception e)
      {
        _log.Error(e);
        _log.Error("failed.");
      }
    }
    public void removeFavorite(YahooVideo video)
    {

      _log.Debug("in removeFav.");
      try
      {
        MusicVideoDatabase loDb = MusicVideoDatabase.getInstance();
        bool lbRemoved = loDb.removeFavoriteVideo(video, msSelectedFavoriteName);
        if (lbRemoved)
        {
          moFavoriteList.Remove(video);
        }
        else
        {
          _log.Debug("{0} could not be removed from favorite {1}", video.songName, msSelectedFavoriteName);
        }
        moFavoriteList.Remove(video);

      }
      catch (Exception e)
      {
        _log.Error(e);
        _log.Error("failed.");
      }
    }
  }
}
