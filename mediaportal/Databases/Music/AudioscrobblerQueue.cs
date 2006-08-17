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
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using System.Web;
using System.Xml;

namespace MediaPortal.Music.Database
{
  class AudioscrobblerQueue
  {
    class QueuedTrack
    {
      #region public getters
      public QueuedTrack(Song track)
      {
        this.artist = track.Artist;
        this.album = track.Album;
        this.title = track.Title;
        this.duration = (int)track.Duration;
        this.start_time = track.getQueueTime();
      }

      public QueuedTrack(string artist, string album,
                string title, int duration, string start_time)
      {
        this.artist = artist;
        this.album = album;
        this.title = title;
        this.duration = duration;
        this.start_time = start_time;
      }

      public string StartTime
      {
        get { return start_time; }
      }
      public string Artist
      {
        get { return artist; }
      }
      public string Album
      {
        get { return album; }
      }
      public string Title
      {
        get { return title; }
      }
      public int Duration
      {
        get { return duration; }
      }
      #endregion

      string artist;
      string album;
      string title;
      int duration;
      string start_time;
    }
    
    ArrayList queue;
    string xml_path;
    bool dirty;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="queueUserFilePath_">the user specific cachefile</param>
    public AudioscrobblerQueue(string queueUserFilePath_)
    {
      xml_path = queueUserFilePath_;
      queue = new ArrayList();

      LoadQueue();
    }

    public void Save()
    {
      if (!dirty)
        return;

      XmlTextWriter writer = new XmlTextWriter(xml_path, Encoding.Default);

      writer.Formatting = Formatting.Indented;
      writer.Indentation = 4;
      writer.IndentChar = ' ';

      writer.WriteStartDocument(true);

      writer.WriteStartElement("AudioscrobblerQueue");
      foreach (QueuedTrack track in queue)
      {
        writer.WriteStartElement("CachedSong");
        writer.WriteElementString("Artist", track.Artist);
        writer.WriteElementString("Album", track.Album);
        writer.WriteElementString("Title", track.Title);
        writer.WriteElementString("Duration", track.Duration.ToString());
        writer.WriteElementString("Playtime", track.StartTime);
        writer.WriteEndElement(); // Track
      }
      writer.WriteEndElement(); // AudioscrobblerQueue
      writer.WriteEndDocument();
      writer.Close();
    }

    public void LoadQueue()
    {
      queue.Clear();

      try
      {
        string query = "//AudioscrobblerQueue/CachedSong";
        XmlDocument doc = new XmlDocument();

        doc.Load(xml_path);
        XmlNodeList nodes = doc.SelectNodes(query);

        foreach (XmlNode node in nodes)
        {
          string artist = "";
          string album = "";
          string title = "";
          int duration = 0;
          string start_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") ;

          foreach (XmlNode child in node.ChildNodes)
          {
            if (child.Name == "Artist" && child.ChildNodes.Count != 0)
            {
              artist = child.ChildNodes[0].Value;
            }
            else if (child.Name == "Album" && child.ChildNodes.Count != 0)
            {
              album = child.ChildNodes[0].Value;
            }
            else if (child.Name == "Title" && child.ChildNodes.Count != 0)
            {
              title = child.ChildNodes[0].Value;
            }
            else if (child.Name == "Duration" && child.ChildNodes.Count != 0)
            {
              duration = Convert.ToInt32(child.ChildNodes[0].Value);
            }
            else if (child.Name == "Playtime" && child.ChildNodes.Count != 0)
            {
              start_time = (child.ChildNodes[0].Value);              
            }
          }

          queue.Add(new QueuedTrack(artist, album, title, duration, start_time));
        }
      }
      catch
      {
      }
    }

    public string GetTransmitInfo(out int num_tracks)
    {
      StringBuilder sb = new StringBuilder();

      int i;
      for (i = 0; i < queue.Count; i++)
      {
        /* we queue a maximum of 10 tracks per request */
        if (i == 9)
          break;

        QueuedTrack track = (QueuedTrack)queue[i];

        sb.AppendFormat(
             "&a[{6}]={0}&t[{6}]={1}&b[{6}]={2}&m[{6}]={3}&l[{6}]={4}&i[{6}]={5}",
             HttpUtility.UrlEncode(track.Artist),
             HttpUtility.UrlEncode(track.Title),
             HttpUtility.UrlEncode(track.Album),
             "" /* musicbrainz id */,
             track.Duration.ToString(),
             HttpUtility.UrlEncode(track.StartTime),
             i);
      }

      num_tracks = i;
      return sb.ToString();
    }

    public void Add(Song track)
    {
      queue.Add(new QueuedTrack(track));
      dirty = true;
    }

    public void RemoveRange(int first, int count)
    {
      queue.RemoveRange(first, count);
      dirty = true;
    }

    public int Count
    {
      get { return queue.Count; }
    }
  }

}
