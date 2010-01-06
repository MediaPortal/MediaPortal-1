#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Xml;

namespace MediaPortal.Music.Database
{
  internal class AudioscrobblerQueue
  {
    private class QueuedTrack
    {
      #region public getters

      public QueuedTrack(Song track)
      {
        this.artist = track.Artist;
        this.title = track.Title;
        this.start_time = track.getQueueTime(true);
        this.source = track.getSourceParam();
        this.rating = track.getRateActionParam();
        this.duration = (int)track.Duration;
        this.album = track.Album;
        this.tracknr = track.Track;
        this.auth = track.AuthToken;
      }

      public QueuedTrack(string artist, string title, string start_time, string source, string rateaction, int duration,
                         string album, int tracknr, string auth)
      {
        this.artist = artist;
        this.title = title;
        this.start_time = start_time;
        this.source = source;
        this.rating = rateaction;
        this.duration = duration;
        this.album = album;
        this.tracknr = tracknr;
        this.auth = auth;
      }

      public string Artist
      {
        get { return artist; }
      }

      public string Title
      {
        get { return title; }
      }

      public string StartTime
      {
        get { return start_time; }
      }

      public string Source
      {
        get { return source; }
      }

      public string Rating
      {
        get { return rating; }
      }

      public int Duration
      {
        get { return duration; }
      }

      public string Album
      {
        get { return album; }
      }

      public int TrackNr
      {
        get { return tracknr; }
      }

      public string Auth
      {
        get { return auth; }
      }

      #endregion

      private string artist;
      private string title;
      private string start_time;
      private string source;
      private string rating;
      private int duration;
      private string album;
      private int tracknr;
      private string auth;
    }

    private ArrayList queue;
    private string xml_path;
    private bool dirty;

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
      {
        return;
      }

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
          writer.WriteElementString("Title", track.Title);
          writer.WriteElementString("Playtime", track.StartTime);
          writer.WriteElementString("Source", track.Source);
          writer.WriteElementString("RateAction", track.Rating);
          writer.WriteElementString("Auth", track.Auth);
          writer.WriteElementString("Duration", track.Duration.ToString());
          writer.WriteElementString("Album", track.Album);
          writer.WriteElementString("TrackNr", track.TrackNr.ToString());

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
          string artist = String.Empty;
          string title = String.Empty;
          string start_time = Convert.ToString(Util.Utils.GetUnixTime(DateTime.UtcNow));
          string source = "P";
          string rating = String.Empty;
          string auth = String.Empty;
          int duration = 0;
          string album = String.Empty;
          int tracknr = 0;

          foreach (XmlNode child in node.ChildNodes)
          {
            if (child.Name == "Artist" && child.ChildNodes.Count != 0)
            {
              artist = child.ChildNodes[0].Value;
            }
            else if (child.Name == "Title" && child.ChildNodes.Count != 0)
            {
              title = child.ChildNodes[0].Value;
            }
            else if (child.Name == "Playtime" && child.ChildNodes.Count != 0)
            {
              start_time = (child.ChildNodes[0].Value);
            }
            else if (child.Name == "Source" && child.ChildNodes.Count != 0)
            {
              source = (child.ChildNodes[0].Value);
            }
            else if (child.Name == "RateAction" && child.ChildNodes.Count != 0)
            {
              rating = (child.ChildNodes[0].Value);
            }
            else if (child.Name == "Auth" && child.ChildNodes.Count != 0)
            {
              auth = (child.ChildNodes[0].Value);
            }
            else if (child.Name == "Duration" && child.ChildNodes.Count != 0)
            {
              duration = Convert.ToInt32(child.ChildNodes[0].Value);
            }
            else if (child.Name == "Album" && child.ChildNodes.Count != 0)
            {
              album = child.ChildNodes[0].Value;
            }
            else if (child.Name == "TrackNr" && child.ChildNodes.Count != 0)
            {
              tracknr = Convert.ToInt32(child.ChildNodes[0].Value);
            }
          }

          queue.Add(new QueuedTrack(artist, title, start_time, source, rating, duration, album, tracknr, auth));
        }
      }
      catch {}
    }

    public string GetTransmitInfo(out int num_tracks)
    {
      StringBuilder sb = new StringBuilder();

      int i;
      for (i = 0; i < queue.Count; i++)
      {
        /* we queue a maximum of 10 tracks per request */
        if (i == 9)
        {
          break;
        }

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
        //m[0}=<mb-trackid>    

        string trackNr = track.TrackNr > 0 ? Convert.ToString(track.TrackNr) : String.Empty;

        sb.AppendFormat(
          "&a[{0}]={1}&t[{0}]={2}&i[{0}]={3}&o[{0}]={4}&r[{0}]={5}&l[{0}]={6}&b[{0}]={7}&n[{0}]={8}&m[{0}]={9}",
          i, // number of queued items = 0
          AudioscrobblerBase.getValidURLLastFMString(AudioscrobblerBase.UndoArtistPrefix(track.Artist)), // artist = 1
          HttpUtility.UrlEncode(track.Title), // track = 2
          track.StartTime, // time = 3
          track.Source, // source = 4
          track.Rating, // rating = 5
          track.Duration.ToString(), // secs = 6
          AudioscrobblerBase.getValidURLLastFMString(track.Album), // album = 7
          trackNr, // tracknumber = 8
          String.Empty // The MusicBrainz Track ID, or empty if not known.
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