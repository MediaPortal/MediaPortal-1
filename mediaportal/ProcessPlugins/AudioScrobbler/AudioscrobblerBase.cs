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
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Timers;
using System.Text;
using MediaPortal.Music.Database;


namespace ProcessPlugins.Audioscrobbler 
{
  //
  // Callback argument types.
  //
  public enum NetworkErrorType {
    ConnectFailed,
    SubmitFailed,
    NoResponse,
    EmptyResponse,
    InvalidResponse
  }
    
  /**
   * A class of this type is passed with every ErrorEvent.
   */
  public abstract class ErrorEventArgs
  {
    public object Error;
    public string Details;

    public ErrorEventArgs (object error_)
    {
      Error = error_;
    }
  }

  /**
   * Specialization for network related error events.
   */
  public class NetworkErrorEventArgs : ErrorEventArgs
  {
    public NetworkErrorEventArgs (object error_)
    : base(error_)
    {
    }
  }

  /**
   * A class of this type is passed with every AuthErrorEvent.
   */
  public class AuthErrorEventArgs
  {
    public AuthErrorEventArgs ()
    {
    }
  }

  /**
   * A class of this type is passed with every ConnectEvent.
   */
  public class ConnectEventArgs
  {
    public ConnectEventArgs ()
    {
    }
  }

  /**
   * A class of this type is passed with every DisconnectEvent.
   */
  public class DisconnectEventArgs
  {
    public DisconnectEventArgs ()
    {
    }
  }

  /**
   * A class of this type is passed with every SubmitEvent.
   */
  public class SubmitEventArgs
  {
    public Song song;

    public SubmitEventArgs (Song song_)
    {
      song = song_;
    }
  }

  /**
   * A class of this type is passed with every UpdateAvailableEvent.
   */
  public class UpdateAvailableEventArgs
  {
    public string version;
    
    public UpdateAvailableEventArgs (string version_)
    {
      version = version_;
    }
  }

  public class AudioscrobblerBase
  {
  //public delegate TRet Functor<TRet>();
  //public delegate TRet Functor<TRet, TArg1>(TArg1 arg1);

    /// Called whenever a network error occurs.
    //public event Functor<NetworkErrorEventArgs> NetworkErrorEvent;
    //public event Functor<NetworkErrorEventArgs> NetworkErrorEventLazy;


    /// Called whenever an authentication failure happened.
    //public event Functor<AuthErrorEventArgs> AuthErrorEvent;
    //public event Functor<AuthErrorEventArgs> AuthErrorEventLazy;

    /// Called whenever the handshake (login) was successful.
    //public event Functor<ConnectEventArgs> ConnectEvent;
    //public event Functor<ConnectEventArgs> ConnectEventLazy;

    /// Called whenever the connection was successfully terminated.
    //public event Functor<DisconnectEventArgs> DisconnectEvent;
    //public event Functor<DisconnectEventArgs> DisconnectEventLazy;
    
    /// Called whenever a song was successfully submitted.
    //public event Functor<SubmitEventArgs> SubmitEvent;
    //public event Functor<SubmitEventArgs> SubmitEventLazy;
    
    /// Called when availibility of a client update was detected.
    //public event Functor<UpdateAvailableEventArgs> UpdateAvailableEvent;
    //public event Functor<UpdateAvailableEventArgs> UpdateAvailableEventLazy;
    
    // Constants.
    const int    MAX_QUEUE_SIZE      = 1000;
    const int    HANDSHAKE_INTERVAL  = 30;     //< In minutes.
    const int    CONNECT_WAIT_TIME   = 2;      //< Min secs between connects.
    const int    SUBMIT_INTERVAL     = 120;    //< Seconds.
    const string CLIENT_NAME         = "MediaPortal";
    const string CLIENT_VERSION      = "0.1.6";
    const string SCROBBLER_URL       = "http://post.audioscrobbler.com";
    const string PROTOCOL_VERSION    = "1.1";
    const string CACHEFILE_NAME      = "audioscrobbler-cache.txt";
    
    // Client-specific config variables.
    private string              username;
    private string              password;
    private string              pluginDir;
    private string              cacheFile;
    
    // Other internal properties.
    private ArrayList           queue;
    private Object              queueLock;
//    private EventQueue          eventQueue;
    private Object              submitLock;
    private DateTime            lastHandshake;        //< last successful attempt.
    private TimeSpan            handshakeInterval;
    private DateTime            lastConnectAttempt;
    private TimeSpan            minConnectWaitTime;
    private System.Timers.Timer submitTimer;
    private bool                connected;
    
    // Data received by the Audioscrobbler service.
    private string              md5challenge;
    private string              submitUrl;

    /**
     * Constructor.
     * \param username The Audioscrobbler account name.
     * \param password The password for the given account.
     * \param dir A local directory in which the queue can be cached.
     */
    public AudioscrobblerBase(string username_, string password_, string plugindir_)
    {
      username             = username_;
      password             = password_;
      pluginDir            = plugindir_;
      connected            = false;
      queue                = new ArrayList();
      queueLock            = new Object();
      //eventQueue           = new EventQueue();
      submitLock           = new Object();
      lastHandshake        = DateTime.MinValue;
      handshakeInterval    = new TimeSpan(0, HANDSHAKE_INTERVAL, 0);
      lastConnectAttempt   = DateTime.MinValue;
      minConnectWaitTime   = new TimeSpan(0, 0, CONNECT_WAIT_TIME);
      cacheFile            = pluginDir + "/" + CACHEFILE_NAME;
      
      // Loading the queue should be fast - no thread required
      LoadQueue();
    }
    

    /***************************************************
     * Public getters and setters.
     ***************************************************/
    /**
     * The Audioscrobbler account name.
     */
    public string Username
    {
      get {
        return username;
      }
      set {
        // don't attempt to reconnect if nothing has changed
        if (value != this.username) {
          this.username = value;
          // allow a new handshake to occur
          lastHandshake = DateTime.MinValue;
//          Global.Log(1, "GAudioscrobbler.Username", "New username: " + username);
        }
      }
    }
  
    /**
     * The Audioscrobbler password.
     */
    public string Password
    {
      get {
        return password;
      }
      set {
        if (value != this.password) {
          this.password = value;
          // allow a new handshake to occur
          lastHandshake = DateTime.MinValue;
//          Global.Log(1, "GAudioscrobbler.Password", "Password changed");
        }
      }
    }

    /**
     * Check connected status.
     * \return True if currently connected, false otherwise.
     */
    public bool Connected
    {
      get {
        return connected;
      }
    }

    /**
     * Returns the number of songs in the queue.
     */
    public int QueueLength
    {
      get {
        return queue.Count;
      }
    }


    /***************************************************
     * Public methods.
     ***************************************************/
    /**
     * Connect to the Audioscrobbler service.
     * The service stays connected until Disconnect() is called, the object
     * is destroyed, or an error occurs. As long as connected, any enqueued
     * songs are submitted to Audioscrobbler.
     */
    public void Connect ()
    {
      // Global.Log(0, "GAudioscrobbler.Connect", "Start");
      // Try to submit all queued songs immediately.
      StartSubmitQueueThread();
      // From now on, try to submit queued songs periodically.
      InitSubmitTimer();
      // Global.Log(0, "GAudioscrobbler.Connect", "End");
    }

    /**
     * Disconnect from the Audioscrobbler service, however, already running
     * transactions are still completed.
     */
    public void Disconnect ()
    {
      // Global.Log(0, "GAudioscrobbler.Disconnect", "Start");
      if (submitTimer != null)
        submitTimer.Close();
      connected = false;
      TriggerDisconnectEvent(new DisconnectEventArgs());
      // Global.Log(0, "GAudioscrobbler.Disconnect", "End");
    }
    
    /**
     * Push the given song on the queue.
     * \param song The song to be enqueued.
     */
    public void pushQueue(Song song_)
    {
      string logmessage = "Adding to queue: " + song_.ToShortString();
      // Global.Log(1, "GAudioscrobbler.pushQueue", logmessage);

      // Enqueue the song.
      song_.AudioScrobblerStatus = SongStatus.Cached;
      lock (queueLock) {
        while (queue.Count > MAX_QUEUE_SIZE)
          queue.RemoveAt(0);
        queue.Add(song_);
      }

      // Try to submit immediately.
      StartSubmitQueueThread();

      // Reset the submit timer.
      submitTimer.Close();
      InitSubmitTimer();
      // Global.Log(0, "GAudioscrobbler.pushQueue", "Finish");
    }

    /**
     * Clears the queue. Also clears the cached queue from the disk.
     */
    public void ClearQueue ()
    {
      lock (queueLock) {
        queue.Clear();
        SaveQueue();
      }
    }


    /***************************************************
     * Public event triggers.
     ***************************************************/
    public void TriggerAuthErrorEvent (AuthErrorEventArgs args_)
    {
      // Global.Log(0, "GAudioscrobbler.EmitAuthErrorEvent", "Start");
      //if (AuthErrorEvent != null)
      //  AuthErrorEvent(args_);
      //if (AuthErrorEventLazy == null)
      //  return;
      //Functor func =
      //  EventQueue.BindOne<AuthErrorEventArgs>(AuthErrorEventLazy, args_);
      //eventQueue.Queue(func);
      // Global.Log(0, "GAudioscrobbler.EmitAuthErrorEvent", "End");
    }

    public void TriggerNetworkErrorEvent (NetworkErrorEventArgs args_)
    {
      // Global.Log(0, "GAudioscrobbler.EmitNetworkErrorEvent", "Start");
      //if (NetworkErrorEvent != null)
      //  NetworkErrorEvent(args_);
      //if (NetworkErrorEventLazy == null)
      //  return;
      //Functor func =
      //  EventQueue.BindOne<NetworkErrorEventArgs>(NetworkErrorEventLazy,
      //                                                  args_);
      //eventQueue.Queue(func);
      // Global.Log(0, "GAudioscrobbler.EmitNetworkErrorEvent", "End");
    }

    public void TriggerSubmitEvent (SubmitEventArgs args_)
    {
      // Global.Log(0, "GAudioscrobbler.EmitSubmitEvent", "Start");
      //if (SubmitEvent != null)
      //  SubmitEvent(args_);
      //if (SubmitEventLazy == null)
      //  return;
      //Functor func =
      //  EventQueue.BindOne<SubmitEventArgs>(SubmitEventLazy, args_);
      //eventQueue.Queue(func);
      // Global.Log(0, "GAudioscrobbler.EmitSubmitEvent", "End");
    }

    public void TriggerUpdateAvailableEvent (UpdateAvailableEventArgs args_)
    {
      // Global.Log(0, "GAudioscrobbler.EmitUpdateAvailableEvent", "Start");
      //if (UpdateAvailableEvent != null)
      //  UpdateAvailableEvent(args_);
      //if (UpdateAvailableEventLazy == null)
      //  return;
      //Functor func =
      //  EventQueue.BindOne<UpdateAvailableEventArgs>(UpdateAvailableEventLazy, args_);
      //eventQueue.Queue(func);
      // Global.Log(0, "GAudioscrobbler.EmitUpdateAvailableEvent", "End");
    }

    public void TriggerConnectEvent (ConnectEventArgs args_)
    {
      // Global.Log(0, "GAudioscrobbler.EmitConnectEvent", "Start");
      //if (ConnectEvent != null)
      //  ConnectEvent(args_);
      //if (ConnectEventLazy == null)
      //  return;
      //Functor func =
      //  EventQueue.BindOne<ConnectEventArgs>(ConnectEventLazy, args_);
      //eventQueue.Queue(func);
      // Global.Log(0, "GAudioscrobbler.EmitConnectEvent", "End");
    }

    public void TriggerDisconnectEvent (DisconnectEventArgs args_)
    {
      // Global.Log(0, "GAudioscrobbler.EmitDisconnectEvent", "Start");
      //if (DisconnectEvent != null)
      //  DisconnectEvent(args_);
      //if (DisconnectEventLazy == null)
      //  return;
      //Functor func =
      //  EventQueue.BindOne<DisconnectEventArgs>(DisconnectEventLazy, args_);
      //eventQueue.Queue(func);
      // Global.Log(0, "GAudioscrobbler.EmitDisconnectEvent", "End");
    }


    /***************************************************
     * Networking related functions.
     ***************************************************/
    /**
     * Handshake with the Audioscrobbler service. This does not require
     * a username and a password, it only checks connectivity.
     * Still, we ensure that a username and password were given,
     * because otherwise making a connection would not make sense.
     * \return True if the connection was successful, false otherwise.
     */
    private bool DoHandshake () 
    {
      // Global.Log(0, "GAudioscrobbler.DoHandshake", "Start");
      
      // Handle uninitialized username/password.
      if (username == "" || password == "") {
        AuthErrorEventArgs args = new AuthErrorEventArgs();
        TriggerAuthErrorEvent(args);
        return false;
      }
      
      // Check whether we had a *successful* handshake recently.
      if (DateTime.Now < lastHandshake.Add(handshakeInterval)) {
        string nexthandshake = lastHandshake.Add(handshakeInterval).ToString();
        string logmessage    = "Next handshake due at " + nexthandshake;
        // Global.Log(0, "GAudioscrobbler.DoHandshake", logmessage);
        return true;
      }

      // Global.Log(1, "GAudioscrobbler.DoHandshake", "Attempting handshake");
      string url = SCROBBLER_URL
                 + "?hs=true"
                 + "&p=" + PROTOCOL_VERSION
                 + "&c=" + CLIENT_NAME
                 + "&v=" + CLIENT_VERSION
                 + "&u=" + System.Web.HttpUtility.UrlEncode(username);

      // Parse handshake response
      bool success = GetResponse(url, "");
      
      if (!success) {
        // Global.Log(1, "GAudioscrobbler.DoHandshake", "Handshake failed");
        return false;
      }

      // Send the event.
      if (!connected) {
        connected = true;
        TriggerConnectEvent(new ConnectEventArgs());
      }

      lastHandshake = DateTime.Now;
      // Global.Log(0, "GAudioscrobbler.DoHandshake", "Handshake successful");
      return true;
    }

    /**
     * Executes the given HTTP request and parses the response of the server.
     * \param url The url to open.
     * \param postdata Data to be sent via HTTP POST, an empty string for GET.
     * \return True if the request was successfully completed, false otherwise.
     */
    private bool GetResponse(string url_, string postdata_)
    {
      // Global.Log(0, "GAudioscrobbler.GetResponse", "Start");

      // Enforce a minimum wait time between connects.
      DateTime nextconnect = lastConnectAttempt.Add(minConnectWaitTime);
      if (DateTime.Now < nextconnect) {
        TimeSpan waittime    = nextconnect - DateTime.Now;
        string   logmessage  = "Connects too fast. Sleeping until "
                             + nextconnect.ToString();
        // Global.Log(1, "GAudioscrobbler.GetResponse", logmessage);
        Thread.Sleep(waittime);
      }
      lastConnectAttempt = DateTime.Now;

      // Connect.
      HttpWebRequest request = null;
      try {
        request = (HttpWebRequest)WebRequest.Create(url_);
        if (request == null)
          throw(new Exception());
      }
      catch (Exception e) {
        NetworkErrorType      type = NetworkErrorType.ConnectFailed;
        NetworkErrorEventArgs args = new NetworkErrorEventArgs(type);
        args.Details = "Connection to the Audioscrobbler server failed.";
        TriggerNetworkErrorEvent(args);
        string logmessage = "WebRequest.Create failed: " + e.Message;
        // Global.Log(1, "GAudioscrobbler.GetResponse", logmessage);
        return false;
      }

      // Attach POST data to the request, if any.
      if (postdata_ != "") {
        // Global.Log(1, "GAudioscrobbler.GetResponse", "POST to " + url_ );
        // Global.Log(1, "GAudioscrobbler.GetResponse", "Data: "   + postdata_ );
        string logmessage = "Connecting to '" + url_ + "\nData: " + postdata_;
        
        // TODO: what is the illegal characters warning all about?
        byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postdata_);
        request.Method        = "POST";
        request.ContentLength = postHeaderBytes.Length;
        request.ContentType   = "application/x-www-form-urlencoded";
        
        // Create stream writer - this can also fail if we aren't connected
        try {
          Stream requestStream = request.GetRequestStream();
          requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
          requestStream.Close();
        } catch (Exception e) {
          NetworkErrorType      type = NetworkErrorType.SubmitFailed;
          NetworkErrorEventArgs args = new NetworkErrorEventArgs(type);
          args.Details = "Error while trying to submit to Audioscrobbler.";
          TriggerNetworkErrorEvent(args);
          logmessage = "HttpWebRequest.GetRequestStream: " + e.Message;
          // Global.Log(1, "GAudioscrobbler.GetResponse", logmessage);
          return false;
        }
      }

      // Create the response object.
      // Global.Log(1, "GAudioscrobbler.GetResponse", "Waiting for response");
      StreamReader reader = null;
      try {
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        if (response == null)
          throw(new Exception());
        reader = new StreamReader(response.GetResponseStream());
      }
      catch (Exception e) {
        NetworkErrorType      type = NetworkErrorType.NoResponse;
        NetworkErrorEventArgs args = new NetworkErrorEventArgs(type);
        args.Details = "Error while waiting for Audioscrobbler response.";
        TriggerNetworkErrorEvent(args);
        string logmessage = "HttpWebRequest.GetResponse: " + e.Message;
        // Global.Log(1, "GAudioscrobbler.GetResponse", logmessage);
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
      // Global.Log(1, "GAudioscrobbler.GetResponse", "Response received");
      string respType = reader.ReadLine();
      if (respType == null) {
        NetworkErrorType      type = NetworkErrorType.EmptyResponse;
        NetworkErrorEventArgs args = new NetworkErrorEventArgs(type);
        args.Details = "Empty response from Audioscrobbler server.";
        TriggerNetworkErrorEvent(args);
        // Global.Log(1, "GAudioscrobbler.GetResponse", args.Details);
        return false;
      }

      // Parse the response.
      bool success       = false;
      bool parse_success = false;
      if (respType.StartsWith("UPTODATE"))
        success = parse_success = parseUpToDateMessage(respType, reader);
      else if (respType.StartsWith("UPDATE"))
        success = parse_success = parseUpdateMessage(respType, reader);
      else if (respType.StartsWith("OK"))
        success = parse_success = parseOkMessage(respType, reader);
      else if (respType.StartsWith("FAILED"))
        parse_success = parseFailedMessage(respType, reader);
      else if (respType.StartsWith("BADUSER") ||
               respType.StartsWith("BADAUTH"))
        parse_success = parseBadUserMessage(respType, reader);
      else {
        string logmessage = "** CRITICAL ** Unknown response " + respType;
        // Global.Log(2, "GAudioscrobbler.GetResponse", logmessage);
      }

      if (!parse_success) {
        NetworkErrorType      type = NetworkErrorType.InvalidResponse;
        NetworkErrorEventArgs args = new NetworkErrorEventArgs(type);
        args.Details = "Unknown response from Audioscrobbler server.";
        TriggerNetworkErrorEvent(args);
        return false;
      }

      // Global.Log(0, "GAudioscrobbler.GetResponse", "End");
      return success;
    }

    void OnSubmitTimerTick (object trash_, ElapsedEventArgs args_)
    {
      StartSubmitQueueThread ();
    }
    
    /**
     * Creates a thread to submit all queued songs.
     */
    private void StartSubmitQueueThread ()
    {
      Thread thread       = new Thread (new ThreadStart(SubmitQueue));
      thread.IsBackground = true;
      thread.Priority     = ThreadPriority.BelowNormal;
      thread.Start();
    }
    
    /**
     * Submit all queued songs to the Audioscrobbler service.
     */
    private void SubmitQueue ()
    {
      // Global.Log(0, "GAudioscrobbler.SubmitQueue", "Start");

      // Make sure that a connection is possible.
      if (!DoHandshake()) {
        // Global.Log(1, "GAudioscrobbler.SubmitQueue", "Handshake failed.");
        return;
      }

      // If the queue is empty, nothing else to do today.
      if (queue.Count <= 0) {
        // Global.Log(0, "GAudioscrobbler.SubmitQueue", "Queue is empty");
        return;
      }

      // Only one thread should attempt to run through the queue at a time.
      lock (submitLock) {
        // Save the queue now since connecting to AS may time out, which
        // takes time, and the user could quit, losing one valuable song...
        SaveQueue();

        // Create a copy of queue since it might change.
        Song[] songs   = null;
        lock (queueLock) {
          songs   = (Song[])queue.ToArray(typeof(Song));
        }

        // Build POST data from the username and the password.
        string webUsername = System.Web.HttpUtility.UrlEncode(username);
        string md5resp     = HashPassword();
        string postData    = "u=" + webUsername + "&s=" + md5resp;

        // Append the songs to be submitted.
        int n_songs = 0;
        foreach (Song song in songs) {
          postData += "&" + song.GetPostData(n_songs);
          n_songs++;
        }

        // Submit or die.
        if (!GetResponse(submitUrl, postData)) {
          // Global.Log(1, "GAudioscrobbler.SubmitQueue", "Submit failed.");
          return;
        }

        // Remove the submitted songs from the queue.
        lock (queueLock) {
          for (int i = 0; i < n_songs; i++)
            queue.RemoveAt(0);
        }

        // Send an event for each of the submitted songs.
        // Global.Log(0, "GAudioscrobbler.SubmitQueue", "Sending SubmitEvents");
        foreach (Song song in songs) {
          song.AudioScrobblerStatus = SongStatus.Submitted;
          SubmitEventArgs args = new SubmitEventArgs(song);
          TriggerSubmitEvent(args);
        }

        // Save again.
        SaveQueue();
      }

      // Global.Log(0, "GAudioscrobbler.SubmitQueue", "End");
    }


    /***************************************************
     * Audioscrobbler response parsers.
     ***************************************************/
    private bool parseUpToDateMessage(string type_, StreamReader reader_)
    {
      // Global.Log(0, "GAudioscrobbler.parseUpToDateMessage", "Called.");
      try {
        md5challenge = reader_.ReadLine().Trim();
        submitUrl    = reader_.ReadLine().Trim();
      } catch (Exception e) {
        string logmessage = "Failed to parse UPTODATE response: " + e.Message;
        // Global.Log(2, "GAudioscrobbler.parseUpToDateMessage", logmessage);
        md5challenge = "";
        return false;
      }
      return true;
    }

    private bool parseUpdateMessage(string type_, StreamReader reader_)
    {
      string version = type_.Substring(7);
      UpdateAvailableEventArgs args = new UpdateAvailableEventArgs(version);
      TriggerUpdateAvailableEvent(args);
      parseUpToDateMessage(type_, reader_);
      return true;
    }

    private bool parseOkMessage(string type_, StreamReader reader_)
    {
      // Global.Log(0, "GAudioscrobbler.parseOkMessage", "Called.");
      return true;
    }

    private bool parseFailedMessage(string type_, StreamReader reader_)
    {
      // Global.Log(0, "GAudioscrobbler.parseFailedMessage", "Called.");
      string logmessage = "";
      if (type_.Length > 7)
        logmessage = "FAILED: " + type_.Substring(7);
      else
        logmessage = "FAILED";
      // Global.Log(2, "GAudioscrobbler.parseFailedMessage", logmessage);
      return false;
    }

    private bool parseBadUserMessage(string type_, StreamReader reader_)
    {
      // Global.Log(0, "GAudioscrobbler.parseBadUserMessage", "Called.");
      AuthErrorEventArgs args = new AuthErrorEventArgs();
      TriggerAuthErrorEvent(args);
      return true;
    }


    /***************************************************
     * Filesystem access.
     ***************************************************/
    private bool LoadQueue()
    {
      FileStream fs;
      try {
        fs = new FileStream(cacheFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
      } catch (Exception e) {
        // Global.Log(2, "GAudioscrobbler.LoadQueue", "Unable to open cache file:\n" + e.Message);
        return false;
      }

      StreamReader file = new StreamReader(fs);
      String line;
      // this lock isn't strictly necessary - this is only called once in
      // the constructor
      lock (queueLock) {
        while ((line = file.ReadLine()) != null) {
          try {
            Song s = Song.ParseFromLine(line);
            s.AudioScrobblerStatus = SongStatus.Loaded;
            queue.Add(s);


            /*FIXME: Maybe, but the user shouldn't have to care actually.
            LoadedEventArgs args = new LoadedEventArgs(song);
            if (LoadedEvent != null)
              LoadedEvent(args);
            queueEvent(LoadedEventLazy, args);
            */
          } catch (Exception e) {
            // Global.Log(2, "GAudioscrobbler.LoadQueue", "Unable to parse the cached song:\n" + e.Message + "\n" + line);
          }
        }
      }
      file.Close();
      // Global.Log(1, "GAudioscrobbler.LoadQueue", "Loaded " + queue.Count + " songs from the cache");

      return true;
    }


    private void SaveQueue()
    {
      FileStream fs;
      try {
        fs = new FileStream(cacheFile, FileMode.Create, FileAccess.Write);
      } catch (Exception e) {
        // Global.Log(2, "GAudioscrobbler.SaveQueue", "Unable to open queue file:\n" + e.Message);
        return;
      }

      StreamWriter file = new StreamWriter(fs);
      // lock this - it should be quick with buffered output anyway
      lock (queueLock) {
        foreach (Song s in queue) {
          try {
            file.WriteLine(s.ToString());
          } catch (IOException e) {
            // Global.Log(2, "GAudioscrobbler.SaveQueue", "Failed to write queue to file:\n" + 
                //e.Message);
          }
        }
      }
      file.Close();
    }


    /***************************************************
     * Utilities.
     ***************************************************/
    private void InitSubmitTimer()
    {
      submitTimer          = new System.Timers.Timer();
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
  } // class GAudioscrobbler
} // namespace GAudioscrobbler
