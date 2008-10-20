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

#region usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;
using System.Text;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.Util;
using MediaPortal.Music.Database;
using MediaPortal.GUI.Library;

#endregion

namespace MediaPortal.Music.Database
{
  public static class AudioscrobblerBase
  {
    #region Events
    public delegate void HandshakeCompleted(HandshakeType ReasonForHandshake, DateTime lastSuccessfulHandshake);
    public static event HandshakeCompleted workerSuccess;

    public delegate void HandshakeFailed(HandshakeType ReasonForHandshake, DateTime lastSuccessfulHandshake, Exception errorReason);
    public static event HandshakeFailed workerFailed;

    public delegate void RadioHandshakeCompleted();
    public static event RadioHandshakeCompleted RadioHandshakeSuccess;

    public delegate void RadioHandshakeFailed();
    public static event RadioHandshakeFailed RadioHandshakeError;
    #endregion

    #region Enums
    public enum HandshakeType : int
    {
      Init = 0,
      Submit = 1,
      ChangeUser = 2,
      PreRadio = 3,
      Recover = 4,
      Announce = 5,
    }
    #endregion

    #region Constants
    const int MAX_QUEUE_SIZE = 50;
    const int HANDSHAKE_INTERVAL = 60;     //< In minutes.
    const int CONNECT_WAIT_TIME = 3;       //< Min secs between connects.
    const string CLIENT_NAME = "mpm";      //assigned by Russ Garrett from Last.fm Ltd.
    const string CLIENT_VERSION = "0.1";
    const string SCROBBLER_URL = "http://post.audioscrobbler.com";
    const string RADIO_SCROBBLER_URL = "http://ws.audioscrobbler.com/radio/";
    const string PROTOCOL_VERSION = "1.2";
    #endregion

    #region Variables
    // Client-specific config variables.
    private static string username;
    private static string password;
    private static string olduser;
    private static string oldpass;

    // Utils
    private static bool _artistsStripped = false;
    private static string _artistPrefixes = string.Empty;

    // Other internal properties.    
    private static Thread submitThread;
    static AudioscrobblerQueue queue;
    private static Object queueLock;
    private static Object submitLock;
    private static DateTime lastHandshake;        //< last successful attempt.
    private static DateTime lastRadioHandshake;
    private static TimeSpan handshakeInterval;
    private static TimeSpan handshakeRadioInterval;
    private static DateTime lastConnectAttempt;

    private static TimeSpan minConnectWaitTime;

    private static bool _useDebugLog;
    private static bool _signedIn;

    // Data received by the Audioscrobbler service.
    private static string MD5Response;
    private static string sessionID;
    private static string submitUrl;
    private static string nowPlayingUrl;
    private static CookieContainer _cookies;

    // radio related
    private static string _radioStreamLocation;
    private static string _radioSession;
    private static bool _subscriber;
    /// <summary>
    /// Determines whether the radio tracks appear in the scrobbled tracks list
    /// </summary>
    private static bool _recordToProfile = true;

    private static Song _currentSong;
    #endregion

    #region Constructor

    /// <summary>
    /// ctor
    /// </summary>
    static AudioscrobblerBase()
    {
      LoadSettings();
      workerSuccess += new HandshakeCompleted(OnHandshakeSuccessful);
      workerFailed += new HandshakeFailed(OnHandshakeFailed);

      if (_useDebugLog)
        Log.Info("AudioscrobblerBase: new scrobbler for {0} with {1} cached songs - debuglog = {2}", Username, Convert.ToString(queue.Count), Convert.ToString(_useDebugLog));
    }

    #endregion

    #region Settings

    static void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        username = xmlreader.GetValueAsString("audioscrobbler", "user", "");
        _recordToProfile = xmlreader.GetValueAsBool("audioscrobbler", "submitradiotracks", true);
        _artistPrefixes = xmlreader.GetValueAsString("musicfiles", "artistprefixes", "The, Les, Die");
        _artistsStripped = xmlreader.GetValueAsBool("musicfiles", "stripartistprefixes", false);

        string tmpPass;

        tmpPass = MusicDatabase.Instance.AddScrobbleUserPassword(Convert.ToString(MusicDatabase.Instance.AddScrobbleUser(username)), "");
        _useDebugLog = (MusicDatabase.Instance.AddScrobbleUserSettings(Convert.ToString(MusicDatabase.Instance.AddScrobbleUser(username)), "iDebugLog", -1) == 1) ? true : false;

        if (tmpPass != string.Empty)
        {
          try
          {
            EncryptDecrypt Crypter = new EncryptDecrypt();
            password = Crypter.Decrypt(tmpPass);
          }
          catch (Exception ex)
          {
            Log.Error("Audioscrobbler: Password decryption failed {0}", ex.Message);
          }
        }
      }

      queue = new AudioscrobblerQueue(Config.GetFile(Config.Dir.Database, "LastFmCache-" + Username + ".xml"));

      queueLock = new Object();
      submitLock = new Object();

      _signedIn = false;
      lastHandshake = DateTime.MinValue;
      handshakeInterval = new TimeSpan(0, HANDSHAKE_INTERVAL, 0);
      handshakeRadioInterval = new TimeSpan(0, 5 * HANDSHAKE_INTERVAL, 0);  // Radio is session based - no need to re-handshake soon
      lastConnectAttempt = DateTime.MinValue;
      minConnectWaitTime = new TimeSpan(0, 0, CONNECT_WAIT_TIME);
      _cookies = new CookieContainer();
      _radioStreamLocation = string.Empty;
      _radioSession = string.Empty;
      _subscriber = false;
      _currentSong = new Song();
    }

    #endregion

    #region Public getters and setters
    /// <summary>
    /// The last.fm account name
    /// </summary>
    public static string Username
    {
      get
      {
        return username;
      }
      set
      {
        // don't attempt to reconnect if nothing has changed
        if (value != username)
        {
          username = value;
          // allow a new handshake to occur
          lastHandshake = DateTime.MinValue;
        }
      }
    }

    /// <summary>
    /// Password for account on last.fm
    /// </summary>
    public static string Password
    {
      get
      {
        return password;
      }
      set
      {
        if (value != password)
        {
          password = value;
          // allow a new handshake to occur
          lastHandshake = DateTime.MinValue;
          //          Log.Info("AudioscrobblerBase.Password", "Password changed");
        }
      }
    }

    /// <summary>
    /// Current Song set by Audioscrobbler plugin for Now Playing Announcement
    /// </summary>
    public static Song CurrentSong
    {
      get
      {
        return _currentSong;
      }
      set
      {
        if (value != _currentSong)
        {
          _currentSong = value;
        }
      }
    }

    public static string RadioSession
    {
      get
      {
        return _radioSession;
      }
    }

    public static string RadioStreamLocation
    {
      get
      {
        return _radioStreamLocation;
      }
    }

    /// <summary>
    /// Get/Set if you like your radio songs to appear in your last.fm profile
    /// </summary>
    public static bool SubmitRadioSongs
    {
      get { return _recordToProfile; }

      set
      {
        if (value != _recordToProfile)
        {
          _recordToProfile = value;
        }
      }
    }

    /// <summary>
    /// Get the subscription status. Must be preceded by "RadioSession" before.
    /// </summary>
    public static bool Subscriber
    {
      get
      {
        return _subscriber;
      }
    }

    /// <summary>
    /// Check connected status - returns true if currently connected, false otherwise.
    /// </summary>
    public static bool Connected
    {
      get
      {
        return _signedIn;
      }
    }

    /// <summary>
    /// Returns the number of songs in the queue
    /// </summary>
    public static int QueueLength
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
    public static void Connect()
    {
      // start thread on start
      if (!_signedIn)
        DoHandshake(true, HandshakeType.Init);
    }

    /// <summary>
    /// Disconnect from the Audioscrobbler service, however, already running transactions are still completed.
    /// </summary>
    public static void Disconnect()
    {
      if (queue != null)
        queue.Save();
      _signedIn = false;
    }

    public static void ChangeUser(string scrobbleUser_, string scrobblePassword_)
    {
      olduser = username;
      oldpass = password;
      if (username != scrobbleUser_)
      {
        queue.Save();
        queue = null;
        MD5Response = "";
        string tmpPass = "";
        try
        {
          EncryptDecrypt Crypter = new EncryptDecrypt();
          tmpPass = Crypter.Decrypt(scrobblePassword_);
        }
        catch (Exception ex)
        {
          Log.Warn("Audioscrobbler: warning on password decryption {0}", ex.Message);
        }
        username = scrobbleUser_;
        password = tmpPass;
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlwriter.SetValue("audioscrobbler", "user", username);
          //xmlwriter.SetValue("audioscrobbler", "pass", password);
        }

        DoHandshake(true, HandshakeType.ChangeUser);
      }
    }

    /// <summary>
    /// Push the given song on the queue.
    /// </summary>
    /// <param name="song_">The song to be enqueued.</param>
    public static void pushQueue(Song song_)
    {
      string logmessage = "Adding to queue: " + song_.ToShortString();
      if (_useDebugLog)
        Log.Debug("AudioscrobblerBase: {0}", logmessage);

      // Enqueue the song.
      //song_.AudioScrobblerStatus = SongStatus.Cached;
      lock (queueLock)
      {
        queue.Add(song_);
      }


      if (submitThread != null)
        if (submitThread.IsAlive)
        {
          try
          {
            Log.Debug("AudioscrobblerBase: trying to kill submit thread (no longer needed)");
            StopSubmitQueueThread();
          }
          catch (Exception ex)
          {
            Log.Debug("AudioscrobblerBase: result of thread.Abort - {0}", ex.Message);
          }
        }

      // Try to submit immediately.
      StartSubmitQueueThread();

      // Reset the submit timer.
      // submitTimer.Close();
      // InitSubmitTimer();
    }

    #region Public event triggers

    public static void TriggerSafeModeEvent()
    {
      DoHandshake(true, HandshakeType.Recover);

      Log.Warn("AudioscrobblerBase: falling back to safe mode");
    }


    #endregion

    #region Networking related functions
    /// <summary>
    /// Handshake with the Audioscrobbler service
    /// </summary>
    /// <returns>True if the connection was successful, false otherwise</returns>
    private static void DoHandshake(bool forceNow_, HandshakeType ReasonForHandshake)
    {
      if (_useDebugLog)
        Log.Debug("AudioscrobblerBase: Attempting {0} handshake", ReasonForHandshake.ToString());

      // Handle uninitialized username/password.
      if (username.Length < 1 || password.Length < 1)
      {
        Log.Error("AudioscrobblerBase: {0}", "user or password not defined");
        workerFailed(ReasonForHandshake, DateTime.MinValue, new Exception("Account details insufficent"));
        return;
      }

      if (!forceNow_ || ReasonForHandshake != HandshakeType.Recover)
      {
        // Check whether we had a *successful* handshake recently.
        if (DateTime.Now < lastHandshake.Add(handshakeInterval))
        {
          string nexthandshake = lastHandshake.Add(handshakeInterval).ToString();
          string logmessage = "Next handshake due at " + nexthandshake;
          if (_useDebugLog)
            Log.Debug("AudioscrobblerBase: {0}", logmessage);
          workerSuccess(ReasonForHandshake, lastHandshake);
          return;
        }
      }

      if (ReasonForHandshake != HandshakeType.Init && !_signedIn)
      {
        if (ReasonForHandshake == HandshakeType.PreRadio)
        {
          Log.Warn("AudioscrobblerBase: Disconnected - nevertheless trying radio handshake to listen without submits");
          AttemptRadioHandshake();
          return;
        }
        else
        {
          Log.Warn("AudioscrobblerBase: Disconnected - not attempting {0} handshake", ReasonForHandshake.ToString());
          workerFailed(ReasonForHandshake, DateTime.MinValue, new Exception("Disconnected!"));
          return;
        }
      }

      BackgroundWorker worker = new BackgroundWorker();
      worker.DoWork += new DoWorkEventHandler(Worker_TryHandshake);
      worker.RunWorkerAsync(ReasonForHandshake);
    }

    private static void Worker_TryHandshake(object sender, DoWorkEventArgs e)
    {
      Thread.CurrentThread.Name = "Scrobbler handshake";
      HandshakeType ReasonForHandshake = (HandshakeType)e.Argument;
      Exception errorReason = null;
      bool success = false;
      string authTime = Convert.ToString(Util.Utils.GetUnixTime(DateTime.UtcNow));
      string tmpPass = HashSingleString(password);
      string tmpAuth = HashSingleString(tmpPass + authTime);

      string url = SCROBBLER_URL
                 + "?hs=true"
                 + "&p=" + PROTOCOL_VERSION
                 + "&c=" + CLIENT_NAME
                 + "&v=" + CLIENT_VERSION
                 + "&u=" + System.Web.HttpUtility.UrlEncode(username)
                 + "&t=" + authTime
                 + "&a=" + tmpAuth;

      try
      {
        // Parse handshake response
        success = GetResponse(url, "", false);

        if (success)
        {
          if (!_signedIn)
            _signedIn = true;

          lastHandshake = DateTime.Now;

          if (_useDebugLog)
            Log.Debug("AudioscrobblerBase: {0} handshake successful", ReasonForHandshake.ToString());

          workerSuccess(ReasonForHandshake, lastHandshake);
        }
        else
        {
          //Log.Warn("AudioscrobblerBase: {0}", "Handshake failed");
          workerFailed(ReasonForHandshake, lastHandshake, errorReason);
        }
      }
      catch (Exception ex)
      {
        errorReason = ex;
        workerFailed(ReasonForHandshake, lastHandshake, errorReason);
      }
    }

    private static void OnHandshakeSuccessful(AudioscrobblerBase.HandshakeType ReasonForHandshake, DateTime lastSuccessfulHandshake)
    {
      switch (ReasonForHandshake)
      {
        case HandshakeType.ChangeUser:
          LoadSettings();
          Log.Info("AudioscrobblerBase: Changed user to {0} - loaded {1} queue items", username, queue.Count);
          break;
        case HandshakeType.PreRadio:
          AttemptRadioHandshake();
          break;
        case HandshakeType.Init:
          AttemptSubmitNow();
          break;
        case HandshakeType.Submit:
          AttemptSubmitNow();
          break;
        case HandshakeType.Announce:
          AttemptAnnounceNow();
          break;
      }
    }

    private static void OnHandshakeFailed(AudioscrobblerBase.HandshakeType ReasonForHandshake, DateTime lastSuccessfulHandshake, Exception errorReason)
    {
      switch (ReasonForHandshake)
      {
        case HandshakeType.ChangeUser:
          Log.Warn("AudioscrobblerBase: {0}", "ChangeUser failed - using previous account");
          username = olduser;
          password = oldpass;
          break;
        case HandshakeType.PreRadio:
          Log.Warn("AudioscrobblerBase: {0}", "Handshake failed - not attempting radio login");
          RadioHandshakeError();
          break;
        case HandshakeType.Submit:
          Log.Warn("AudioscrobblerBase: {0}", "Handshake failed - no submits");
          break;
        case HandshakeType.Init:
          Log.Warn("AudioscrobblerBase: {0}", "Handshake failed - could not log in");
          break;
        case HandshakeType.Recover:
          Log.Warn("AudioscrobblerBase: {0}", "Handshake failed - could not recover. Disconnecting...");
          Disconnect();
          break;
        case HandshakeType.Announce:
          Log.Warn("AudioscrobblerBase: {0}", "Handshake failed - not announcing current song");
          break;
      }
    }

    public static void DoRadioHandshake(bool forceNow_)
    {
      // Handle uninitialized username/password.
      if (username.Length < 1 || password.Length < 1)
      {
        Log.Error("AudioscrobblerBase: {0}", "user or password not defined for Last.FM Radio");
        RadioHandshakeError();
        return;
      }

      if (!forceNow_)
      {
        // Check whether we had a *successful* handshake recently.
        if (DateTime.Now < lastRadioHandshake.Add(handshakeRadioInterval))
        {
          string nextRadioHandshake = lastRadioHandshake.Add(handshakeRadioInterval).ToString();
          string logmessage = "Next radio handshake due at " + nextRadioHandshake;
          if (_useDebugLog)
            Log.Debug("AudioscrobblerBase: {0}", logmessage);

          RadioHandshakeSuccess();
          return;
        }
      }

      DoHandshake(false, HandshakeType.PreRadio);
    }

    private static bool AttemptRadioHandshake()
    {
      // http://ws.audioscrobbler.com/radio/handshake.php?version=1.3.1.1&platform=win32&username=f1n4rf1n&passwordmd5=3847af7ab43a1c31503e8bef7736c41f&language=de&player=wmplayer HTTP/1.1

      string tmpUser = System.Web.HttpUtility.UrlEncode(username).ToLower();
      string tmpPass = HashSingleString(password);
      string url = RADIO_SCROBBLER_URL
                 + "handshake.php?"
                 + "version=" + "1.3.2.9"
                 + "&platform=" + "win32"
                 + "&username=" + tmpUser
                 + "&passwordmd5=" + tmpPass
                 + "&language=" + "en"
                 + "&player=unknown";

      // Parse handshake response
      bool success = GetResponse(url, "", true);

      if (!success)
      {
        Log.Warn("AudioscrobblerBase: {0}", "Radio handshake failed");
        RadioHandshakeError();
        return false;
      }

      lastRadioHandshake = DateTime.Now;

      if (_useDebugLog)
        Log.Debug("AudioscrobblerBase: {0}", "Radio handshake successful");

      //url = "http://ws.audioscrobbler.com/ass/"
      //     + "upgrade.php?"
      //     + "platform=" + "win"
      //     + "&version=" + "1.0.7"
      //     + "&lang=" + "en"
      //     + "&user=" + tmpUser;

      //GetResponse(url, "", true);
      //if (_useDebugLog)
      //  Log.Debug("AudioscrobblerBase: {0}", "Upgrade request send");

      RadioHandshakeSuccess();
      return true;
    }


    /// <summary>
    /// Executes the given HTTP request and parses the response of the server.
    /// </summary>
    /// <param name="url_">The url to open</param>
    /// <param name="postdata_">Data to be sent via HTTP POST, an empty string for GET</param>
    /// <returns>True if the request was successfully completed, false otherwise</returns>
    private static bool GetResponse(string url_, string postdata_, bool useGet_)
    {
      // Enforce a minimum wait time between connects.
      DateTime nextconnect = lastConnectAttempt.Add(minConnectWaitTime);
      if (DateTime.Now < nextconnect)
      {
        TimeSpan waittime = nextconnect - DateTime.Now;
        string logmessage = "Avoiding too fast connects. Sleeping until "
                             + nextconnect.ToString();
        if (_useDebugLog)
          Log.Debug("AudioscrobblerBase: {0}", logmessage);
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
        else
        {
          request.CookieContainer = _cookies;
          //request.UserAgent = "Last.fm Client 1.3.1.1 (Windows)";

          try
          {
            // Use the current user in case an NTLM Proxy or similar is used.
            request.Proxy = WebProxy.GetDefaultProxy();
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;
          }
          catch (Exception) { }

          //request.Timeout = 20000;
          request.Pipelined = false;

          request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
        }
      }
      catch (Exception e)
      {
        string logmessage = "WebRequest.Create failed: " + e.Message;
        Log.Error("AudioscrobblerBase.GetResponse: {0}", logmessage);
        return false;
      }

      // Attach POST data to the request, if any.
      if (postdata_ != "")
      {
        //Log.Info("AudioscrobblerBase.GetResponse: POST to {0}", url_);
        if (url_.Contains(nowPlayingUrl))
          Log.Info("AudioscrobblerBase: Announcing current track: {0}", postdata_);
        else
          Log.Info("AudioscrobblerBase: Submitting data: {0}", postdata_);

        try
        {
          byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postdata_);
          if (useGet_)
          {
            request.Method = "GET";
          }
          else
          {
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
          }
          request.ContentLength = postHeaderBytes.Length;

          // Create stream writer - this can also fail if we aren't connected
          Stream requestStream = request.GetRequestStream();
          requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
          requestStream.Close();
        }
        catch (Exception e)
        {
          Log.Warn("AudioscrobblerBase.GetResponse: {0}", e.Message);
          return false;
        }
      }

      // Create the response object.
      if (_useDebugLog)
        Log.Debug("AudioscrobblerBase: {0}", "Waiting for response");
      StreamReader reader = null;
      string statusCode = string.Empty;

      try
      {
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        if (response == null)
          throw (new Exception());
        else
        {
          // Print the properties of each cookie.
          int i = 0;
          foreach (Cookie cook in response.Cookies)
          {
            _cookies.Add(cook);
            i++;
            if (_useDebugLog)
            {
              Log.Debug("AudioscrobblerBase: Cookie: {0}", Convert.ToString(i));
              Log.Debug("AudioscrobblerBase: {0} = {1}", cook.Name, cook.Value);
              Log.Debug("AudioscrobblerBase: Domain: {0}", cook.Domain);
              Log.Debug("AudioscrobblerBase: Path: {0}", cook.Path);
              Log.Debug("AudioscrobblerBase: Port: {0}", cook.Port);
              Log.Debug("AudioscrobblerBase: Secure: {0}", cook.Secure);
              Log.Debug("AudioscrobblerBase: When issued: {0}", cook.TimeStamp);
              Log.Debug("AudioscrobblerBase: Expires: {0} (expired? {1})", cook.Expires, cook.Expired);
              Log.Debug("AudioscrobblerBase: Don't save: {0}", cook.Discard);
              Log.Debug("AudioscrobblerBase: Comment: {0}", cook.Comment);
              Log.Debug("AudioscrobblerBase: Uri for comments: {0}", cook.CommentUri);
              Log.Debug("AudioscrobblerBase: Version: RFC {0}", cook.Version == 1 ? "2109" : "2965");

              // Show the string representation of the cookie.
              Log.Debug("AudioscrobblerBase: String: {0}", cook.ToString());
            }
          }
        }
        reader = new StreamReader(response.GetResponseStream());
        statusCode = response.StatusDescription;
      }

      catch (Exception e)
      {
        string logmessage = "HttpWebRequest.GetResponse: " + e.Message;
        Log.Error("AudioscrobblerBase.GetResponse: {0}", logmessage);
        return false;
      }

      // now we are connected
      if (_useDebugLog)
        Log.Debug("AudioscrobblerBase: Response received - status: {0}", statusCode);

      bool success = false;
      bool parse_success = false;
      try
      {
        string respType = reader.ReadLine();
        if (respType == null)
        {
          Log.Error("AudioscrobblerBase.GetResponse: {0}", "Empty response from Audioscrobbler server.");
          return false;
        }

        // Parse the response.
        if (respType.StartsWith("UPTODATE"))
          success = parse_success = parseUpToDateMessage(respType, reader);
        else if (respType.StartsWith("UPDATE"))
          Log.Error("AudioscrobblerBase: {0}", "UPDATE needed!");
        else if (respType.StartsWith("OK"))
          success = parse_success = parseOkMessage(respType, reader);
        else if (respType.StartsWith("FAILED"))
          parse_success = parseFailedMessage(respType, reader);
        else if (respType.StartsWith("BADUSER") || respType.StartsWith("BADAUTH"))
          parse_success = parseBadUserMessage(respType, reader);
        else if (respType.StartsWith("BADTIME"))
          parse_success = parseBadTimeMessage(respType, reader);
        else if (respType.StartsWith("session="))
          success = parse_success = parseRadioStreamMessage(respType, reader);
        else if (respType.StartsWith(@"[App]")) // upgrade message
          success = parse_success = true;

        else
        {
          string logmessage = "** CRITICAL ** Unknown response";
          while ((respType = reader.ReadLine()) != null)
            logmessage += "\n " + respType;
          Log.Error("AudioscrobblerBase: {0}", logmessage);
        }

        // read next line to look for an interval
        //while ((respType = reader.ReadLine()) != null)
        //  if (respType.StartsWith("INTERVAL"))
        //    parse_success = parseIntervalMessage(respType, reader);

      }
      catch (Exception ex)
      {
        Log.Error("AudioscrobblerBase: Exception on reading response lines - {0}", ex.Message);
      }

      if (!parse_success)
      {
        return false;
      }

      //Log.Info("AudioscrobblerBase.GetResponse: {0}", "End");
      return success;
    }
    #endregion

    /// <summary>
    /// Creates a thread to submit all queued songs.
    /// </summary>
    private static void StartSubmitQueueThread()
    {
      submitThread = new Thread(new ThreadStart(SubmitQueue));
      submitThread.IsBackground = true;
      submitThread.Name = "Scrobbler";
      submitThread.Priority = ThreadPriority.BelowNormal;
      submitThread.Start();
    }

    private static void StopSubmitQueueThread()
    {
      if (submitThread != null)
        submitThread.Abort();
    }

    /// <summary>
    /// Submit all queued songs to the Audioscrobbler service
    /// </summary>
    private static void SubmitQueue()
    {
      // Make sure that a connection is possible.
      DoHandshake(false, HandshakeType.Submit);
    }

    public static void AnnounceNowPlaying()
    {
      DoHandshake(false, HandshakeType.Announce);
    }

    private static void AttemptAnnounceNow()
    {
      if (CurrentSong.Artist != string.Empty && CurrentSong.Title != string.Empty)
      {
        BackgroundWorker worker = new BackgroundWorker();
        worker.DoWork += new DoWorkEventHandler(Worker_TryAnnounceTracks);
        worker.RunWorkerAsync();
      }
      else
        Log.Debug("AudioscrobblerBase: AttemptAnnounceNow aborted because of incomplete data");
    }

    private static void AttemptSubmitNow()
    {
      // If the queue is empty, nothing else to do today.
      if (queue.Count <= 0)
      {
        if (_useDebugLog)
          Log.Debug("AudioscrobblerBase: {0}", "Queue is empty");
        return;
      }

      BackgroundWorker worker = new BackgroundWorker();
      worker.DoWork += new DoWorkEventHandler(Worker_TrySubmitTracks);
      worker.RunWorkerAsync();
    }

    private static void Worker_TryAnnounceTracks(object sender, DoWorkEventArgs e)
    {
      Thread.CurrentThread.Name = "Scrobbler nowplaying";
      // s=<sessionID>       The Session ID string as returned by the handshake. Required.
      // a=<artist>          The artist name. Required.
      // t=<track>           The track name. Required.
      // b=<album>           The album title, or empty if not known.
      // l=<secs>            The length of the track in seconds, or empty if not known.
      // n=<tracknumber>     The position of the track on the album, or empty if not known.
      // m=<mb-trackid>      The MusicBrainz Track ID, or empty if not known.

      string announceData = string.Empty;
      StringBuilder sb = new StringBuilder();

      sb.Append("s=");
      sb.Append(sessionID);
      sb.Append("&a=");
      sb.Append(getValidURLLastFMString(UndoArtistPrefix(CurrentSong.Artist)));
      sb.Append("&t=");
      sb.Append(System.Web.HttpUtility.UrlEncode(CurrentSong.Title));
      sb.Append("&b=");
      sb.Append(getValidURLLastFMString(CurrentSong.Album));
      sb.Append("&l=");
      sb.Append(CurrentSong.Duration);
      sb.Append("&n=");
      sb.Append(CurrentSong.Track > 0 ? Convert.ToString(CurrentSong.Track) : "");
      sb.Append("&m=");
      sb.Append("");

      announceData = sb.ToString();

      // Submit or die.
      if (!GetResponse(nowPlayingUrl, announceData, false))
      {
        Log.Warn("AudioscrobblerBase: Now playing announcement failed.");
        return;
      }
    }

    private static void Worker_TrySubmitTracks(object sender, DoWorkEventArgs e)
    {
      // Only one thread should attempt to run through the queue at a time.
      lock (submitLock)
      {
        Thread.CurrentThread.Name = "Scrobbler submit";
        int _submittedSongs = 0;

        // Save the queue now since connecting to AS may time out, which
        // takes time, and the user could quit, losing one valuable song...
        queue.Save();

        /*
           s=<sessionID>            The Session ID string as returned by the handshake. Required.
           a[0]=<artist>            The artist name. Required.
           t[0]=<track>             The track title. Required.
           i[0]=<time>              The time the track started playing, in UNIX timestamp format (integer number of seconds since 00:00:00, January 1st 1970 UTC). This must be in the UTC time zone, and is required.
           o[0]=<source>            The source of the track. Required, must be one of the following codes:

               P = Chosen by the user, no shuffle
               S = Chosen by the user, shuffle enabled
               T = Chosen by the user, unknown shuffle status (e.g. iPod)
               R = Non-personalised broadcast (e.g. Shoutcast, BBC Radio 1)
               E = Personalised recommendation except Last.fm (e.g. Pandora, Launchcast)
               L = Last.fm (any mode)
               U = Source unknown

           r[0]=<rating>

               L = Love
               B = Ban
               S = Skip (only if source=L)

           b[0]=<album>             The album title, or empty if not known.
           n[0]=<tracknumber>       The position of the track on the album, or empty if not known.
           m[0]=<mb-trackid>        The MusicBrainz Track ID, or empty if not known.
           l[0]=<secs>              The length of the track in seconds, or empty if not known. 
        */

        string postData = "s=" + sessionID;

        StringBuilder sb = new StringBuilder();

        sb.Append(postData);
        sb.Append(queue.GetTransmitInfo(out _submittedSongs));

        postData = sb.ToString();

        if (!postData.Contains("&a[0]"))
        {
          if (_useDebugLog)
            Log.Debug("AudioscrobblerBase: postData did not contain all track parameter");
          return;
        }

        // Submit or die.
        if (!GetResponse(submitUrl, postData, false))
        {
          Log.Error("AudioscrobblerBase: {0}", "Submit failed.");
          return;
        }

        // Remove the submitted songs from the queue.
        if (_useDebugLog)
          Log.Debug("AudioscrobblerBase: remove submitted songs from queue - set lock");
        lock (queueLock)
        {
          try
          {
            queue.RemoveRange(0, _submittedSongs);
            queue.Save();
          }
          catch (Exception ex)
          {
            Log.Error("AudioscrobblerBase: submit thread clearing cache - {0}", ex.Message);
          }
        }
        Log.Debug("AudioscrobblerBase: submitted songs successfully removed from queue. Idle...");
      }
    }
    #endregion

    #region Audioscrobbler response parsers.
    private static bool parseUpToDateMessage(string type_, StreamReader reader_)
    {
      if (_useDebugLog)
        Log.Debug("AudioscrobblerBase: {0}", "Your client is up to date.");
      return true;
    }

    private static bool parseOkMessage(string type_, StreamReader reader_)
    {
      try
      {
        sessionID = reader_.ReadLine().Trim();
        nowPlayingUrl = reader_.ReadLine().Trim();
        submitUrl = reader_.ReadLine().Trim();
      }
      catch (Exception)
      {
      }
      Log.Info("AudioscrobblerBase: {0}", "Action successfully completed.");
      return true;
    }

    private static bool parseFailedMessage(string type_, StreamReader reader_)
    {
      try
      {
        string logmessage = "";
        if (type_.Length > 7)
          logmessage = "FAILED: " + type_.Substring(7);
        else
          logmessage = "FAILED";
        if (_useDebugLog)
          Log.Debug("AudioscrobblerBase: {0}", logmessage);
        if (logmessage == "FAILED: Plugin bug: Not all request variables are set")
          Log.Info("AudioscrobblerBase: A server error may have occured / if you receive this often a proxy may truncate your request - {0}", "read: http://www.last.fm/forum/24/_/74505/1#f808273");
        TriggerSafeModeEvent();
        return true;
      }
      catch (Exception e)
      {
        string logmessage = "Failed to parse FAILED response: " + e.Message;
        Log.Error("AudioscrobblerBase.parseFailedMessage: {0}", logmessage);
        return false;
      }
    }

    private static bool parseBadUserMessage(string type_, StreamReader reader_)
    {
      Log.Warn("AudioscrobblerBase: {0}", "PLEASE CHECK YOUR ACCOUNT CONFIG! - re-trying handshake now");
      //TriggerSafeModeEvent();
      return true;
    }

    private static bool parseBadTimeMessage(string type_, StreamReader reader_)
    {
      Log.Warn("AudioscrobblerBase: {0}", "BADTIME response received!");
      //TriggerSafeModeEvent();
      return true;
    }

    private static bool parseRadioStreamMessage(string type_, StreamReader reader_)
    {
      if (type_.Contains("FAILED") || type_.Contains("failed"))
      {
        string logmessage = "AudioscrobblerBase: Radio session failed";
        while ((type_ = reader_.ReadLine()) != null)
        {
          logmessage += type_ + ", ";
        }

        Log.Warn(logmessage);
        return false;
      }


      if (type_.Length > 8)
      {
        _radioSession = type_.Substring(8);
        Log.Info("AudioscrobblerBase: Initialising radio session {0}", _radioSession);

        int i = 0;
        while ((type_ = reader_.ReadLine()) != null)
        {
          i++;
          if (i == 1)
            _radioStreamLocation = type_.Substring(11);
          if (i == 2)
          {
            if (type_.Substring(11) == "1")
              _subscriber = true;
            else
              _subscriber = false;

          }
        }
        Log.Info("AudioscrobblerBase: Successfully initialised radio stream {0} - subscriber: {1}", _radioStreamLocation, _subscriber);

        return true;
      }

      return false;
    }
    #endregion

    #region Utilities

    private static string HashSubmitToken()
    {
      return HashMD5LoginStrings(false, password);
    }

    private static string HashSingleString(string singleString)
    {
      return HashMD5LoginStrings(true, singleString);
    }

    private static string HashMD5LoginStrings(bool passwordOnly_, string inputString_)
    {
      // generate MD5 response from user's password

      // The MD5 response is md5(md5(password) + challenge), where MD5
      // is the ascii-encoded MD5 representation, and + represents
      // concatenation.

      ////MD5 hash = MD5.Create();
      ////UTF8Encoding encoding = new UTF8Encoding();
      ////byte[] barr = hash.ComputeHash(encoding.GetBytes(password));

      ////string tmp = CryptoConvert.ToHex(barr).ToLower();

      ////barr = hash.ComputeHash(encoding.GetBytes(tmp + md5challenge));
      ////string md5response = CryptoConvert.ToHex(barr).ToLower();

      ////return md5response;


      MD5 hash = MD5CryptoServiceProvider.Create();
      UTF8Encoding encoding = new UTF8Encoding();
      byte[] barr = hash.ComputeHash(encoding.GetBytes(inputString_));

      string tmp = string.Empty;
      for (int i = 0; i < barr.Length; i++)
      {
        tmp += barr[i].ToString("x2");
      }
      if (passwordOnly_)
        return tmp;

      barr = hash.ComputeHash(encoding.GetBytes(tmp + MD5Response));

      string md5response = string.Empty;
      for (int i = 0; i < barr.Length; i++)
      {
        md5response += barr[i].ToString("x2");
      }

      return md5response;
    }

    private static string removeInvalidChars(string inputString_)
    {
      string cleanString = inputString_;
      int dotIndex = 0;

      // remove CD1, CD2, CDn from Tracks
      if (Util.Utils.ShouldStack(cleanString, cleanString))
        Util.Utils.RemoveStackEndings(ref cleanString);
      // remove [DJ Spacko MIX (2000)]
      dotIndex = cleanString.IndexOf("[");
      if (dotIndex > 0)
        cleanString = cleanString.Remove(dotIndex);
      dotIndex = cleanString.IndexOf("(");
      if (dotIndex > 0)
        cleanString = cleanString.Remove(dotIndex);
      dotIndex = cleanString.IndexOf("feat.");
      if (dotIndex > 0)
        cleanString = cleanString.Remove(dotIndex);

      // TODO: build REGEX here
      // replace our artist concatenation
      // cleanString = cleanString.Replace("|", "&");
      if (cleanString.Contains("|"))
        cleanString = cleanString.Remove(cleanString.IndexOf("|"));
      // substitute "&" with "and" <-- as long as needed
      //      cleanString = cleanString.Replace("&", " and ");
      // make sure there's only one space
      //      cleanString = cleanString.Replace("  ", " ");
      // substitute "/" with "+"
      //      cleanString = cleanString.Replace(@"/", "+");
      // clean soundtracks
      cleanString = cleanString.Replace("OST ", " ");
      cleanString = cleanString.Replace("Soundtrack - ", " ");

      if (cleanString.EndsWith("Soundtrack"))
        cleanString = cleanString.Remove(cleanString.IndexOf("Soundtrack"));
      if (cleanString.EndsWith("OST"))
        cleanString = cleanString.Remove(cleanString.IndexOf("OST"));
      if (cleanString.EndsWith(" EP"))
        cleanString = cleanString.Remove(cleanString.IndexOf(" EP"));
      if (cleanString.EndsWith(" (EP)"))
        cleanString = cleanString.Remove(cleanString.IndexOf(" (EP)"));

      return cleanString.Trim();
    }

    private static string removeEndingChars(string inputString_)
    {
      int dotIndex = 0;
      // build a clean end
      inputString_ = inputString_.Trim();
      dotIndex = inputString_.LastIndexOf('-');
      if (dotIndex >= inputString_.Length - 2)
        inputString_ = inputString_.Remove(dotIndex);
      dotIndex = inputString_.LastIndexOf('+');
      if (dotIndex >= inputString_.Length - 2)
        inputString_ = inputString_.Remove(dotIndex);

      return inputString_;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static string getValidURLLastFMString(string lastFMString)
    {
      string outString = string.Empty;
      string urlString = System.Web.HttpUtility.UrlEncode(lastFMString);

      try
      {
        outString = removeInvalidChars(lastFMString);

        if (!string.IsNullOrEmpty(outString))
          urlString = System.Web.HttpUtility.UrlEncode(removeEndingChars(outString));

        outString = urlString;

        // add chars here which need to be followed by "+" to be recognized correctly by last.fm
        // consider some special cases like R.E.M. / D.D.E. / P.O.D etc
        List<Char> invalidSingleChars = new List<Char>();
        // invalidSingleChars.Add('.');
        // invalidSingleChars.Add(',');

        foreach (Char singleChar in invalidSingleChars)
        {
          // do not loop unless needed
          if (urlString.IndexOf(singleChar) > 0)
          {
            // check each letter of the string
            for (int s = 0; s < urlString.Length; s++)
            {
              // the evil char has been detected
              if (urlString[s] == singleChar)
              {
                outString = urlString.Insert(s + 1, "+");
                urlString = outString;
                // skip checking the just inserted position
                s++;
              }
            }
          }
        }
        outString = outString.Replace("++", "+");
        // build a clean end
        outString = removeEndingChars(outString);
      }
      catch (Exception ex)
      {
        Log.Error("AudioscrobblerBase: Error while building valid url string - {0}", ex.Message);
        return urlString;
      }
      return outString;
    }

    public static string UndoArtistPrefix(string aStrippedArtist)
    {
      //"The, Les, Die"
      if (_artistsStripped)
      {
        try
        {
          string[] allPrefixes = null;
          allPrefixes = _artistPrefixes.Split(',');
          if (allPrefixes != null && allPrefixes.Length > 0)
          {
            for (int i = 0; i < allPrefixes.Length; i++)
            {
              string cpyPrefix = allPrefixes[i];
              if (aStrippedArtist.ToLowerInvariant().EndsWith(cpyPrefix.ToLowerInvariant()))
              {
                // strip the separating "," as well
                int prefixPos = aStrippedArtist.IndexOf(',');
                if (prefixPos > 0)
                {
                  aStrippedArtist = aStrippedArtist.Remove(prefixPos);
                  cpyPrefix = cpyPrefix.Trim(new char[] { ' ', ',' });
                  aStrippedArtist = cpyPrefix + " " + aStrippedArtist;
                  // abort here since artists should only have one prefix stripped
                  return aStrippedArtist;
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("AudioscrobblerBase: An error occured undoing prefix strip for artist: {0} - {1}", aStrippedArtist, ex.Message);
        }
      }

      return aStrippedArtist;
    }

    public static string StripArtistPrefix(string aArtistToStrip)
    {
      //"The, Les, Die"
      if (_artistsStripped)
        Util.Utils.StripArtistNamePrefix(ref aArtistToStrip, true);

      return aArtistToStrip;
    }
    #endregion

  } // class AudioscrobblerBase
} // namespace AudioscrobblerBase
