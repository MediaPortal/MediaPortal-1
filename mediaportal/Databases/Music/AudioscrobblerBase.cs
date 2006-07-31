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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Timers;
using System.Text;
using System.Xml;
using MediaPortal.Util;
using MediaPortal.Music.Database;
using MediaPortal.GUI.Library;

namespace MediaPortal.Music.Database
{
  #region Callback argument types
  public enum lastFMFeed
  {
    recenttracks,
    weeklyartistchart,
    weeklytrackchart,
    topartists,
    toptracks,
    friends,
    neighbours
  }

  public enum NetworkErrorType
  {
    ConnectFailed,
    SubmitFailed,
    NoResponse,
    EmptyResponse,
    InvalidResponse
  }

  /// <summary>
  /// A class of this type is passed with every ErrorEvent.
  /// </summary>
  public abstract class ErrorEventArgs
  {
    public object Error;
    public string Details;

    public ErrorEventArgs(object error_)
    {
      Error = error_;
    }
  }

  /// <summary>
  /// Specialization for network related error events.
  /// </summary>
  public class NetworkErrorEventArgs : ErrorEventArgs
  {
    public NetworkErrorEventArgs(object error_)
      : base(error_)
    {
    }
  }

  /// <summary>
  /// A class of this type is passed with every AuthErrorEvent
  /// </summary>
  public class AuthErrorEventArgs
  {
    public AuthErrorEventArgs()
    {
    }
  }

  /// <summary>
  /// A class of this type is passed with every ConnectEvent
  /// </summary>
  public class ConnectEventArgs
  {
    public ConnectEventArgs()
    {
    }
  }

  /// <summary>
  /// A class of this type is passed with every DisconnectEvent
  /// </summary>
  public class DisconnectEventArgs
  {
    public DisconnectEventArgs()
    {
    }
  }

  /// <summary>
  /// A class of this type is passed with every SubmitEvent
  /// </summary>
  public class SubmitEventArgs
  {
    public Song song;

    public SubmitEventArgs(Song song_)
    {
      song = song_;
    }
  }

  //public class UpdateAvailableEventArgs
  //{
  //  public string version;

  //  public UpdateAvailableEventArgs(string version_)
  //  {
  //    version = version_;
  //  }
  //}
  #endregion

  public class AudioscrobblerBase
  {
    #region Constants
    const int MAX_QUEUE_SIZE = 1000;
    const int HANDSHAKE_INTERVAL = 30;     //< In minutes.
    const int CONNECT_WAIT_TIME = 5;      //< Min secs between connects.
    const int SUBMIT_INTERVAL = 180;    //< Seconds.
    const string CLIENT_NAME = "ark";
    const string CLIENT_VERSION = "1.4";
    const string SCROBBLER_URL = "http://post.audioscrobbler.com";
    const string PROTOCOL_VERSION = "1.1";
    const string CACHEFILE_NAME = "audioscrobbler-cache.txt";
    #endregion

    #region Variables
    // Client-specific config variables.
    private string username;
    private string password;

    private string cacheFile;

    // Other internal properties.
    List<Song> songList = null;
    private ArrayList queue;
    private Object queueLock;
    //    private EventQueue          eventQueue;
    private Object submitLock;
    private DateTime lastHandshake;        //< last successful attempt.
    private TimeSpan handshakeInterval;
    private DateTime lastConnectAttempt;
    private TimeSpan minConnectWaitTime;
    private System.Timers.Timer submitTimer;
    private bool connected;

    // Data received by the Audioscrobbler service.
    private string md5challenge;
    private string submitUrl;
    #endregion

    /// <summary>
    /// ctor
    /// </summary>
    public AudioscrobblerBase()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {        
        username = xmlreader.GetValueAsString("audioscrobbler", "user", "");
        string tmpPass;
        tmpPass = xmlreader.GetValueAsString("audioscrobbler", "pass", "");
        if (tmpPass != String.Empty)
        {
          try
          {
            EncryptDecrypt Crypter = new EncryptDecrypt();
            password = Crypter.Decrypt(tmpPass);
          }
          catch (Exception ex)
          {
            Log.Write("Audioscrobbler: Password decryption failed {0}", ex.Message);
          }
        }
      }
      

      connected = false;
      queue = new ArrayList();
      queueLock = new Object();
      //eventQueue           = new EventQueue();
      submitLock = new Object();
      lastHandshake = DateTime.MinValue;
      handshakeInterval = new TimeSpan(0, HANDSHAKE_INTERVAL, 0);
      lastConnectAttempt = DateTime.MinValue;
      minConnectWaitTime = new TimeSpan(0, 0, CONNECT_WAIT_TIME);
      cacheFile = CACHEFILE_NAME;

      // Loading the queue should be fast - no thread required
      LoadQueue();
    }
    

    #region Public getters and setters
    /// <summary>
    /// The last.fm account name
    /// </summary>
    public string Username
    {
      get
      {
        return username;
      }
      set
      {
        // don't attempt to reconnect if nothing has changed
        if (value != this.username)
        {
          this.username = value;
          // allow a new handshake to occur
          lastHandshake = DateTime.MinValue;
        }
      }
    }

    /// <summary>
    /// Password for account on last.fm
    /// </summary>
    public string Password
    {
      get
      {
        return password;
      }
      set
      {
        if (value != this.password)
        {
          this.password = value;
          // allow a new handshake to occur
          lastHandshake = DateTime.MinValue;
          //          Log.Write("AudioscrobblerBase.Password", "Password changed");
        }
      }
    }

    /// <summary>
    /// Check connected status - returns true if currently connected, false otherwise.
    /// </summary>
    public bool Connected
    {
      get
      {
        return connected;
      }
    }

    /// <summary>
    /// Returns the number of songs in the queue
    /// </summary>
    public int QueueLength
    {
      get
      {
        return queue.Count;
      }
    }
    #endregion

    #region Public methods.
    /// <summary>
    /// Connect to the Audioscrobbler service. While connected any queued songs are submitted to Audioscrobbler.
    /// </summary>
    public void Connect()
    {
      //Log.Write("AudioscrobblerBase.Connect: {0}", "Start");
      // Try to submit all queued songs immediately.
      StartSubmitQueueThread();
      // From now on, try to submit queued songs periodically.
      InitSubmitTimer();
      //Log.Write("AudioscrobblerBase.Connect: {0}", "End");
    }

    /// <summary>
    /// Disconnect from the Audioscrobbler service, however, already running transactions are still completed.
    /// </summary>
    public void Disconnect()
    {
      //Log.Write("AudioscrobblerBase.Disconnect: {0}", "Start");
      if (submitTimer != null)
        submitTimer.Close();
      connected = false;
      TriggerDisconnectEvent(new DisconnectEventArgs());
      //Log.Write("AudioscrobblerBase.Disconnect: {0}", "End");
    }

    /// <summary>
    /// Push the given song on the queue.
    /// </summary>
    /// <param name="song_">The song to be enqueued.</param>
    public void pushQueue(Song song_)
    {
      string logmessage = "Adding to queue: " + song_.ToShortString();
      Log.Write("AudioscrobblerBase.pushQueue: {0}", logmessage);

      // Enqueue the song.
      song_.AudioScrobblerStatus = SongStatus.Cached;
      lock (queueLock)
      {
        while (queue.Count > MAX_QUEUE_SIZE)
          queue.RemoveAt(0);
        queue.Add(song_);
      }

      // Try to submit immediately.
      StartSubmitQueueThread();

      // Reset the submit timer.
      submitTimer.Close();
      InitSubmitTimer();
      Log.Write("AudioscrobblerBase.pushQueue: {0}", "Finish");
    }



    /// <summary>
    /// Clears the queue. Also clears the cached queue from the disk.
    /// </summary>
    public void ClearQueue()
    {
      lock (queueLock)
      {
        queue.Clear();
        SaveQueue();
      }
    }
    #endregion

    #region Public event triggers
    public void TriggerAuthErrorEvent(AuthErrorEventArgs args_)
    {
      //Disconnect();
      //Log.Write("AudioscrobblerBase.TriggerAuthErrorEvent: {0}", "Start");
      //if (AuthErrorEvent != null)
      //  AuthErrorEvent(args_);
      //if (AuthErrorEventLazy == null)
      //  return;
      //Log.Write("AudioscrobblerBase.TriggerAuthErrorEvent: {0}", "End");
    }

    public void TriggerNetworkErrorEvent(NetworkErrorEventArgs args_)
    {
      //Log.Write("AudioscrobblerBase.TriggerNetworkErrorEvent: {0}", "Start");
      //if (NetworkErrorEvent != null)
      //  NetworkErrorEvent(args_);
      //if (NetworkErrorEventLazy == null)
      //  return;
      //Log.Write("AudioscrobblerBase.TriggerNetworkErrorEvent: {0}", "End");
    }

    public void TriggerSubmitEvent(SubmitEventArgs args_)
    {
      //Log.Write("AudioscrobblerBase.TriggerSubmitEvent: {0}", "Start");
      //if (SubmitEvent != null)
      //  SubmitEvent(args_);
      //if (SubmitEventLazy == null)
      //  return;
      //Log.Write("AudioscrobblerBase.TriggerSubmitEvent: {0}", "End");
    }

    public void TriggerConnectEvent(ConnectEventArgs args_)
    {
      //Log.Write("AudioscrobblerBase.TriggerConnectEvent: {0}", "Start");
      //if (ConnectEvent != null)
      //  ConnectEvent(args_);
      //if (ConnectEventLazy == null)
      //  return;
      //Log.Write("AudioscrobblerBase.TriggerConnectEvent: {0}", "End");
    }

    public void TriggerDisconnectEvent(DisconnectEventArgs args_)
    {
      //Log.Write("AudioscrobblerBase.TriggerDisconnectEvent: {0}", "Start");
      //if (DisconnectEvent != null)
      //  DisconnectEvent(args_);
      //if (DisconnectEventLazy == null)
      //  return;
      //Log.Write("AudioscrobblerBase.TriggerDisconnectEvent: {0}", "End");
    }
    #endregion

    #region Networking related functions
    /// <summary>
    /// Handshake with the Audioscrobbler service
    /// </summary>
    /// <returns>True if the connection was successful, false otherwise</returns>
    private bool DoHandshake()
    {
      //Log.Write("AudioscrobblerBase.DoHandshake: {0}", "Start");

      // Handle uninitialized username/password.
      if (username == "" || password == "")
      {
        AuthErrorEventArgs args = new AuthErrorEventArgs();
        TriggerAuthErrorEvent(args);
        return false;
      }

      // Check whether we had a *successful* handshake recently.
      if (DateTime.Now < lastHandshake.Add(handshakeInterval))
      {
       // string nexthandshake = lastHandshake.Add(handshakeInterval).ToString();
        //string logmessage = "Next handshake due at " + nexthandshake;
        //Log.Write("AudioscrobblerBase.DoHandshake: {0}", logmessage);
        return true;
      }

      //Log.Write("AudioscrobblerBase.DoHandshake: {0}", "Attempting handshake");
      string url = SCROBBLER_URL
                 + "?hs=true"
                 + "&p=" + PROTOCOL_VERSION
                 + "&c=" + CLIENT_NAME
                 + "&v=" + CLIENT_VERSION
                 + "&u=" + System.Web.HttpUtility.UrlEncode(username);

      // Parse handshake response
      bool success = GetResponse(url, "");

      if (!success)
      {
        Log.Write("AudioscrobblerBase.DoHandshake: {0}", "Handshake failed");
        TriggerAuthErrorEvent(new AuthErrorEventArgs());
        return false;
      }

      // Send the event.
      if (!connected)
      {
        connected = true;
        TriggerConnectEvent(new ConnectEventArgs());
      }

      lastHandshake = DateTime.Now;
      Log.Write("AudioscrobblerBase.DoHandshake: {0}", "Handshake successful");
      return true;
    }
    
    /// <summary>
    /// Executes the given HTTP request and parses the response of the server.
    /// </summary>
    /// <param name="url_">The url to open</param>
    /// <param name="postdata_">Data to be sent via HTTP POST, an empty string for GET</param>
    /// <returns>True if the request was successfully completed, false otherwise</returns>
    private bool GetResponse(string url_, string postdata_)
    {
      //Log.Write("AudioscrobblerBase.GetResponse: {0}", "Start");

      // Enforce a minimum wait time between connects.
      DateTime nextconnect = lastConnectAttempt.Add(minConnectWaitTime);
      if (DateTime.Now < nextconnect)
      {
        TimeSpan waittime = nextconnect - DateTime.Now;
        string logmessage = "Avoiding too fast connects. Sleeping until "
                             + nextconnect.ToString();
        Log.Write("AudioscrobblerBase.GetResponse: {0}", logmessage);
        Thread.Sleep(waittime);
      }
      lastConnectAttempt = DateTime.Now;

      // Connect.
      HttpWebRequest request = null;
      try
      {
        request = (HttpWebRequest)WebRequest.Create(url_);
        if (request == null)
          throw (new Exception());
      }
      catch (Exception e)
      {
        NetworkErrorType type = NetworkErrorType.ConnectFailed;
        NetworkErrorEventArgs args = new NetworkErrorEventArgs(type);
        args.Details = "Connection to the Audioscrobbler server failed.";
        TriggerNetworkErrorEvent(args);
        string logmessage = "WebRequest.Create failed: " + e.Message;
        Log.Write("AudioscrobblerBase.GetResponse: {0}", logmessage);
        return false;
      }

      // Attach POST data to the request, if any.
      if (postdata_ != "")
      {
        //Log.Write("AudioscrobblerBase.GetResponse: POST to {0}", url_);
        Log.Write("AudioscrobblerBase.GetResponse: Submitting data: {0}", postdata_);
        string logmessage = "Connecting to '" + url_ + "\nData: " + postdata_;

        // TODO: what is the illegal characters warning all about?
        byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postdata_);
        request.Method = "POST";
        request.ContentLength = postHeaderBytes.Length;
        request.ContentType = "application/x-www-form-urlencoded";

        // Create stream writer - this can also fail if we aren't connected
        try
        {
          Stream requestStream = request.GetRequestStream();
          requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
          requestStream.Close();
        }
        catch (Exception e)
        {
          NetworkErrorType type = NetworkErrorType.SubmitFailed;
          NetworkErrorEventArgs args = new NetworkErrorEventArgs(type);
          args.Details = "Error while trying to submit to Audioscrobbler.";
          TriggerNetworkErrorEvent(args);
          logmessage = "HttpWebRequest.GetRequestStream: " + e.Message;
          Log.Write("AudioscrobblerBase.GetResponse: {0}", logmessage);
          return false;
        }
      }

      // Create the response object.
      //Log.Write("AudioscrobblerBase.GetResponse: {0}", "Waiting for response");
      StreamReader reader = null;
      try
      {
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        if (response == null)
          throw (new Exception());
        reader = new StreamReader(response.GetResponseStream());
      }
      catch (Exception e)
      {
        NetworkErrorType type = NetworkErrorType.NoResponse;
        NetworkErrorEventArgs args = new NetworkErrorEventArgs(type);
        args.Details = "Error while waiting for Audioscrobbler response.";
        TriggerNetworkErrorEvent(args);
        string logmessage = "HttpWebRequest.GetResponse: " + e.Message;
        Log.Write("AudioscrobblerBase.GetResponse: {0}", logmessage);
        return false;
      }

      /* At this point we are connected.  Now we get the first line of the
       * response. That should be one of the following:
       * UPTODATE
       * UPDATE
       * OK
       * FAILED
       * BADUSER / BADAUTH
       */
      Log.Write("AudioscrobblerBase.GetResponse: {0}", "Response received");
      string respType = reader.ReadLine();
      if (respType == null)
      {
        NetworkErrorType type = NetworkErrorType.EmptyResponse;
        NetworkErrorEventArgs args = new NetworkErrorEventArgs(type);
        args.Details = "Empty response from Audioscrobbler server.";
        TriggerNetworkErrorEvent(args);
        Log.Write("AudioscrobblerBase.GetResponse: {0}", args.Details);
        return false;
      }

      // Parse the response.
      bool success = false;
      bool parse_success = false;
      if (respType.StartsWith("UPTODATE"))
        success = parse_success = parseUpToDateMessage(respType, reader);
      else if (respType.StartsWith("UPDATE"))
        Log.Write("AudioscrobblerBase.GetResponse: {0}", "UPDATE needed!");
      else if (respType.StartsWith("OK"))
        success = parse_success = parseOkMessage(respType, reader);
      else if (respType.StartsWith("FAILED"))
        parse_success = parseFailedMessage(respType, reader);
      else if (respType.StartsWith("BADUSER") ||
               respType.StartsWith("BADAUTH"))
        parse_success = parseBadUserMessage(respType, reader);
      else
      {
        string logmessage = "** CRITICAL ** Unknown response " + respType;
        Log.Write("AudioscrobblerBase.GetResponse: {0}", logmessage);
      }

      if (!parse_success)
      {
        NetworkErrorType type = NetworkErrorType.InvalidResponse;
        NetworkErrorEventArgs args = new NetworkErrorEventArgs(type);
        args.Details = "Unknown response from Audioscrobbler server.";
        TriggerNetworkErrorEvent(args);
        return false;
      }

      //Log.Write("AudioscrobblerBase.GetResponse: {0}", "End");
      return success;
    }

    void OnSubmitTimerTick(object trash_, ElapsedEventArgs args_)
    {
      StartSubmitQueueThread();
    }

    /// <summary>
    /// Creates a thread to submit all queued songs.
    /// </summary>
    private void StartSubmitQueueThread()
    {
      Thread thread = new Thread(new ThreadStart(SubmitQueue));
      thread.IsBackground = true;
      thread.Priority = ThreadPriority.BelowNormal;
      thread.Start();
    }

    /// <summary>
    /// Submit all queued songs to the Audioscrobbler service
    /// </summary>
    private void SubmitQueue()
    {
      //Log.Write("AudioscrobblerBase.SubmitQueue: {0}", "Start");

      // Make sure that a connection is possible.
      if (!DoHandshake())
      {
        Log.Write("AudioscrobblerBase.SubmitQueue: {0}", "Handshake failed.");
        return;
      }

      // If the queue is empty, nothing else to do today.
      if (queue.Count <= 0)
      {
        //Log.Write("AudioscrobblerBase.SubmitQueue: {0}", "Queue is empty");
        return;
      }

      // Only one thread should attempt to run through the queue at a time.
      lock (submitLock)
      {
        // Save the queue now since connecting to AS may time out, which
        // takes time, and the user could quit, losing one valuable song...
        SaveQueue();

        // Create a copy of queue since it might change.
        Song[] songs = null;
        lock (queueLock)
        {
          songs = (Song[])queue.ToArray(typeof(Song));
        }

        // Build POST data from the username and the password.
        string webUsername = System.Web.HttpUtility.UrlEncode(username);
        string md5resp = HashPassword();
        string postData = "u=" + webUsername + "&s=" + md5resp;

        // Append the songs to be submitted.
        int n_songs = 0;
        foreach (Song song in songs)
        {
          postData += "&" + song.GetPostData(n_songs);
          n_songs++;
        }

        // Submit or die.
        if (!GetResponse(submitUrl, postData))
        {
          Log.Write("AudioscrobblerBase.SubmitQueue: {0}", "Submit failed.");
          return;
        }

        // Remove the submitted songs from the queue.
        lock (queueLock)
        {
          for (int i = 0; i < n_songs; i++)
            queue.RemoveAt(0);
        }

        // Send an event for each of the submitted songs.
        //Log.Write("AudioscrobblerBase.SubmitQueue: {0}", "Sending SubmitEvents");
        foreach (Song song in songs)
        {
          song.AudioScrobblerStatus = SongStatus.Submitted;
          SubmitEventArgs args = new SubmitEventArgs(song);
          TriggerSubmitEvent(args);
        }

        // Save again.
        SaveQueue();
      }

      //Log.Write("AudioscrobblerBase.SubmitQueue: {0}", "End");
    }

    
    public List<Song> ParseXMLDoc(string xmlFileInput, string queryNodePath, lastFMFeed xmlfeed)
    {
      songList = new List<Song>();
      try
      {
        XmlDocument doc = new XmlDocument();

        doc.Load(xmlFileInput);
        XmlNodeList nodes = doc.SelectNodes(queryNodePath);

        foreach (XmlNode node in nodes)
        {
          Song nodeSong = new Song();
          foreach (XmlNode child in node.ChildNodes)
          {
            switch (xmlfeed)
            {
              case (lastFMFeed.recenttracks):
                {
                  if (child.Name == "artist" && child.ChildNodes.Count != 0)                                    
                    nodeSong.Artist = child.ChildNodes[0].Value;                  
                  else if (child.Name == "name" && child.ChildNodes.Count != 0)
                    nodeSong.Title = child.ChildNodes[0].Value;
                  else if (child.Name == "mbid" && child.ChildNodes.Count != 0)
                    nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
                  else if (child.Name == "url" && child.ChildNodes.Count != 0)
                    nodeSong.URL = child.ChildNodes[0].Value;
                  else if (child.Name == "date" && child.ChildNodes.Count != 0)
                    nodeSong.DateTimePlayed = Convert.ToDateTime(child.ChildNodes[0].Value);
                }
                break;
              case (lastFMFeed.topartists):
                {
                  if (child.Name == "name" && child.ChildNodes.Count != 0)
                    nodeSong.Artist = child.ChildNodes[0].Value;
                  //else if (child.Name == "name" && child.ChildNodes.Count != 0)
                  //  nodeSong.Title = child.ChildNodes[0].Value;
                  else if (child.Name == "mbid" && child.ChildNodes.Count != 0)
                    nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
                  else if (child.Name == "playcount" && child.ChildNodes.Count != 0)
                    nodeSong.TimesPlayed = Convert.ToInt32(child.ChildNodes[0].Value);
                  else if (child.Name == "url" && child.ChildNodes.Count != 0)
                    nodeSong.URL = child.ChildNodes[0].Value;
                }
                break;
              case (lastFMFeed.weeklyartistchart):
                goto case lastFMFeed.topartists;

              case (lastFMFeed.toptracks):
                {
                  if (child.Name == "artist" && child.ChildNodes.Count != 0)
                    nodeSong.Artist = child.ChildNodes[0].Value;
                  else if (child.Name == "name" && child.ChildNodes.Count != 0)
                    nodeSong.Title = child.ChildNodes[0].Value;
                  else if (child.Name == "mbid" && child.ChildNodes.Count != 0)
                    nodeSong.MusicBrainzID = child.ChildNodes[0].Value;
                  else if (child.Name == "playcount" && child.ChildNodes.Count != 0)
                    nodeSong.TimesPlayed = Convert.ToInt32(child.ChildNodes[0].Value);
                  else if (child.Name == "url" && child.ChildNodes.Count != 0)
                    nodeSong.URL = child.ChildNodes[0].Value;
                }
                break;
              case (lastFMFeed.weeklytrackchart):
                goto case lastFMFeed.toptracks;

            } //switch
          }
          songList.Add(nodeSong);
        }
      }
      catch
      {
        // input nice exception here...
      }
      return songList;  
    }
    #endregion

    #region Audioscrobbler response parsers.
    private bool parseUpToDateMessage(string type_, StreamReader reader_)
    {
      Log.Write("AudioscrobblerBase.parseUpToDateMessage: {0}", "Called.");
      try
      {
        md5challenge = reader_.ReadLine().Trim();
        submitUrl = reader_.ReadLine().Trim();
      }
      catch (Exception e)
      {
        string logmessage = "Failed to parse UPTODATE response: " + e.Message;
        Log.Write("AudioscrobblerBase.parseUpToDateMessage: {0}", logmessage);
        md5challenge = "";
        return false;
      }
      return true;
    }

    private bool parseOkMessage(string type_, StreamReader reader_)
    {
      Log.Write("AudioscrobblerBase.parseOkMessage: {0}", "Action successfully completed.");
      return true;
    }

    private bool parseFailedMessage(string type_, StreamReader reader_)
    {
      //Log.Write("AudioscrobblerBase.parseFailedMessage: {0}", "Called.");
      string logmessage = "";
      if (type_.Length > 7)
        logmessage = "FAILED: " + type_.Substring(7);
      else
        logmessage = "FAILED";      
      Log.Write("AudioscrobblerBase.parseFailedMessage: {0}", logmessage);
      if (logmessage == "FAILED: Plugin bug: Not all request variables are set")
        Log.Write("AudioscrobblerBase: A server error may have occured - {0}", "read: http://www.last.fm/forum/21713/_/116514");
      return false;
    }

    private bool parseBadUserMessage(string type_, StreamReader reader_)
    {
      Log.Write("AudioscrobblerBase.parseBadUserMessage: {0}", "PLEASE CHECK YOUR ACCOUNT CONFIG!");
      AuthErrorEventArgs args = new AuthErrorEventArgs();
      TriggerAuthErrorEvent(args);
      return true;
    }
    #endregion

    #region Filesystem access
    private bool LoadQueue()
    {
      FileStream fs;
      try
      {
        fs = new FileStream(cacheFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
      }
      catch (Exception e)
      {
        Log.Write("AudioscrobblerBase.LoadQueue: Unable to open cache file: {0}", e.Message);
        return false;
      }

      StreamReader file = new StreamReader(fs);
      String line;
      // this lock isn't strictly necessary - this is only called once in
      // the constructor
      lock (queueLock)
      {
        while ((line = file.ReadLine()) != null)
        {
          try
          {
            Song s = Song.ParseFromLine(line);
            s.AudioScrobblerStatus = SongStatus.Loaded;
            queue.Add(s);


            /*FIXME: Maybe, but the user shouldn't have to care actually.
            LoadedEventArgs args = new LoadedEventArgs(song);
            if (LoadedEvent != null)
              LoadedEvent(args);
            queueEvent(LoadedEventLazy, args);
            */
          }
          catch (Exception e)
          {
            Log.Write("AudioscrobblerBase.LoadQueue: Unable to parse the cached song: : {0}", line);
            Log.Write("AudioscrobblerBase.LoadQueue: Unable to parse the cached song: : {0}", e.Message);
          }
        }
      }
      file.Close();
      Log.Write("AudioscrobblerBase.LoadQueue: Songs loaded from cache: {0}", queue.Count);

      return true;
    }


    private void SaveQueue()
    {
      FileStream fs;
      try
      {
        fs = new FileStream(cacheFile, FileMode.Create, FileAccess.Write);
      }
      catch (Exception e)
      {
        Log.Write("AudioscrobblerBase.SaveQueue: Unable to open queue file: {0}", e.Message);
        return;
      }

      StreamWriter file = new StreamWriter(fs);
      // lock this - it should be quick with buffered output anyway
      lock (queueLock)
      {
        foreach (Song s in queue)
        {
          try
          {
            file.WriteLine(s.ToString());
          }
          catch (IOException e)
          {
            Log.Write("AudioscrobblerBase.SaveQueue: Failed to write queue to file: {0}", e.Message);
          }
        }
      }
      file.Close();
    }
    #endregion

    #region Utilities
    private void InitSubmitTimer()
    {
      submitTimer = new System.Timers.Timer();
      submitTimer.Interval = SUBMIT_INTERVAL * 1000;
      submitTimer.Elapsed += new ElapsedEventHandler(OnSubmitTimerTick);
      submitTimer.Start();
    }

    private string HashPassword()
    {
      // generate MD5 response from user's password

      // The MD5 response is md5(md5(password) + challenge), where MD5
      // is the ascii-encoded MD5 representation, and + represents
      // concatenation.

      MD5 hash = MD5.Create();
      UTF8Encoding encoding = new UTF8Encoding();
      byte[] barr = hash.ComputeHash(encoding.GetBytes(password));

      string tmp = CryptoConvert.ToHex(barr).ToLower();

      barr = hash.ComputeHash(encoding.GetBytes(tmp + md5challenge));
      string md5response = CryptoConvert.ToHex(barr).ToLower();

      return md5response;
    }
    #endregion
  } // class AudioscrobblerBase
} // namespace AudioscrobblerBase
