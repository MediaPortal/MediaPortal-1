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
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using System.Web;
using System.Xml;
using System.Runtime.CompilerServices;

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
        this.tracknr = track.Track;
        this.duration = (int)track.Duration;
        this.start_time = track.getQueueTime(true);
      }

      public QueuedTrack(string artist, string album, string title, int tracknr, int duration, string start_time)
      {
        this.artist = artist;
        this.album = album;
        this.title = title;
        this.tracknr = tracknr;
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
      public int TrackNr
      {
        get { return tracknr; }
      }
      public int Duration
      {
        get { return duration; }
      }
      #endregion

      string artist;
      string album;
      string title;
      int tracknr;
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

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Save()
    {
      if (!dirty)
        return;

      using (XmlTextWriter writer = new XmlTextWriter(xml_path, Encoding.UTF8))
      {
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
          writer.WriteElementString("TrackNr", track.TrackNr.ToString());
          writer.WriteElementString("Duration", track.Duration.ToString());
          //DateTime startTime = DateTime.Now;
          //string submitStartTime = string.Empty;
          //if (DateTime.TryParse(track.StartTime, out startTime))
          //  submitStartTime = Convert.ToString(Util.Utils.GetUnixTime(startTime.ToUniversalTime()));
          //else
          //  submitStartTime = Convert.ToString(Util.Utils.GetUnixTime(DateTime.UtcNow - new TimeSpan(0, 0, track.Duration)));
          writer.WriteElementString("Playtime", track.StartTime);
          writer.WriteEndElement(); // Track
        }
        writer.WriteEndElement(); // AudioscrobblerQueue
        writer.WriteEndDocument();
        writer.Close();
      }
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
          int tracknr = 0;
          int duration = 0;
          string start_time = Convert.ToString(Util.Utils.GetUnixTime(DateTime.UtcNow));

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
            else if (child.Name == "TrackNr" && child.ChildNodes.Count != 0)
            {
              tracknr = Convert.ToInt32(child.ChildNodes[0].Value);
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

          queue.Add(new QueuedTrack(artist, album, title, tracknr, duration, start_time));
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
        
        //s=<sessionID>
        //a[0]=<artist>
        //t[0]=<track>
        //i[0]=<time>
        //o[0]=<source>
        //r[0]=<rating>
        //l[0]=<secs>
        //b[0]=<album>
        //n[0]=<tracknumber>
        //m[0]=<mb-trackid>

        string trackNr = track.TrackNr > 0 ? Convert.ToString(track.TrackNr) : "";

        sb.AppendFormat(
             "&a[{0}]={1}&t[{0}]={2}&i[{0}]={3}&o[{0}]={4}&r[{0}]={5}&l[{0}]={6}&b[{0}]={7}&n[{0}]={8}&m[{0}]={9}",
             i,
             AudioscrobblerBase.getValidURLLastFMString(AudioscrobblerBase.UndoArtistPrefix(track.Artist)),
             System.Web.HttpUtility.UrlEncode(track.Title),
             track.StartTime,
             "P" /* chosen by user */ ,
             "",
             track.Duration.ToString(),
             AudioscrobblerBase.getValidURLLastFMString(track.Album),
             trackNr /* track.tracknr */,
             "" /* musicbrainz id */
             );
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
