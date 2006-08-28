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
  public static class AudioscrobblerBase
  {
    #region Constants
    const int MAX_QUEUE_SIZE = 10;
    const int HANDSHAKE_INTERVAL = 30;     //< In minutes.
    const int CONNECT_WAIT_TIME = 5;      //< Min secs between connects.
    static int SUBMIT_INTERVAL = 30;    //< Seconds.
    const string CLIENT_NAME = "mpm"; //assigned by Russ Garrett from Last.fm Ltd.
    const string CLIENT_VERSION = "0.1";
    const string SCROBBLER_URL = "http://post.audioscrobbler.com";
    const string PROTOCOL_VERSION = "1.1";
    #endregion

    #region Variables
    // Client-specific config variables.
    private static string username;
    private static string password;

    //private string cacheFile;

    // Other internal properties.    
    private static Thread submitThread;
    static AudioscrobblerQueue queue;
    private static Object queueLock;
    private static Object submitLock;
    private static int _antiHammerCount = 0;
    private static DateTime lastHandshake;        //< last successful attempt.
    private static TimeSpan handshakeInterval;
    private static DateTime lastConnectAttempt;

    private static TimeSpan minConnectWaitTime;

    private static bool _disableTimerThread;
    private static bool _useDebugLog;
    private static System.Timers.Timer submitTimer;
    private static bool _signedIn;

    // Data received by the Audioscrobbler service.
    private static string md5challenge;
    private static string submitUrl;
    #endregion

    /// <summary>
    /// ctor
    /// </summary>
    static AudioscrobblerBase()
    {
      LoadSettings();

      if (_useDebugLog)
        Log.Info("AudioscrobblerBase: new scrobbler for {0} with {1} cached songs - debuglog={2}", Username, Convert.ToString(queue.Count), Convert.ToString(_useDebugLog));
    //    else
    //Log.Info("AudioscrobblerBase: new scrobbler for {0} - debuglog={1} directonly={2}", Username, Convert.ToString(_useDebugLog), Convert.ToString(_disableTimerThread));
    }

    static void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        //_useDebugLog = xmlreader.GetValueAsBool("audioscrobbler", "usedebuglog", false);
        //_dismissOnError = xmlreader.GetValueAsBool("audioscrobbler", "dismisscacheonerror", false);
        _disableTimerThread = xmlreader.GetValueAsBool("audioscrobbler", "disabletimerthread", true);

        username = xmlreader.GetValueAsString("audioscrobbler", "user", "");
        string tmpPass;
        //tmpPass = xmlreader.GetValueAsString("audioscrobbler", "pass", "");
        MusicDatabase mdb = new MusicDatabase();

        tmpPass = mdb.AddScrobbleUserPassword(Convert.ToString(mdb.AddScrobbleUser(username)), "");
        _useDebugLog = (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(username)), "iDebugLog", -1) == 1) ? true : false;

        if (tmpPass != String.Empty)
        {
          try
          {
            EncryptDecrypt Crypter = new EncryptDecrypt();
            password = Crypter.Decrypt(tmpPass);
          }
          catch (Exception ex)
          {
            Log.Info("Audioscrobbler: Password decryption failed {0}", ex.Message);
          }
        }
      }  

      queue = new AudioscrobblerQueue("Trackcache-" + Username + ".xml");

      queueLock = new Object();
      submitLock = new Object();

      _signedIn = false;
      lastHandshake = DateTime.MinValue;
      handshakeInterval = new TimeSpan(0, HANDSHAKE_INTERVAL, 0);
      lastConnectAttempt = DateTime.MinValue;
      minConnectWaitTime = new TimeSpan(0, 0, CONNECT_WAIT_TIME);
    }

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
      //Log.Info("AudioscrobblerBase.Connect: {0}", "Start");
      // Try to submit all queued songs immediately.
      if (!_disableTimerThread)
        StartSubmitQueueThread();
      // From now on, try to submit queued songs periodically.
      InitSubmitTimer();
      //Log.Info("AudioscrobblerBase.Connect: {0}", "End");
    }

    /// <summary>
    /// Disconnect from the Audioscrobbler service, however, already running transactions are still completed.
    /// </summary>
    public static void Disconnect()
    {
      if (submitTimer != null)
        submitTimer.Close();
      queue.Save();
      _signedIn = false;
    }

    public static void ChangeUser(string scrobbleUser_, string scrobblePassword_)
    {
      string olduser = username;
      string oldpass = password;
      if (username != scrobbleUser_)
      {
        queue.Save();
        queue = null;        
        md5challenge = "";
        string tmpPass = "";
        try
        {
          EncryptDecrypt Crypter = new EncryptDecrypt();
          tmpPass = Crypter.Decrypt(scrobblePassword_);
        }
        catch (Exception ex)
        {
          Log.Info("Audioscrobbler: warning on password decryption {0}", ex.Message);
        }
        username = scrobbleUser_;
        password = tmpPass;
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
        {
          xmlwriter.SetValue("audioscrobbler", "user", username);
          //xmlwriter.SetValue("audioscrobbler", "pass", password);
        }

        if (!DoHandshake(true))
        {
          Log.Info("AudioscrobblerBase: {0}", "ChangeUser failed - using previous account");
          username = olduser;
          password = oldpass;
        }
        else
        {
          LoadSettings();
          Log.Info("AudioscrobblerBase: Changed user to {0} - loaded {1} queue items", scrobbleUser_, queue.Count);
        }
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
        Log.Info("AudioscrobblerBase: {0}", logmessage);

      // Enqueue the song.
      song_.AudioScrobblerStatus = SongStatus.Cached;
      lock (queueLock)
      {
        queue.Add(song_);
      }

      if (_antiHammerCount == 0)
      {
        if (_disableTimerThread)
          if (submitThread != null)
            if (submitThread.IsAlive)
            {
              try
              {
                Log.Info("AudioscrobblerBase: {0}", "trying to kill submit thread (no longer needed)");
                StopSubmitQueueThread();
              }
              catch (Exception ex)
              {
                Log.Info("AudioscrobblerBase: result of thread.Abort - {0}", ex.Message);
              }
            }
        
        // Try to submit immediately.
        StartSubmitQueueThread();

        // Reset the submit timer.
        submitTimer.Close();
        InitSubmitTimer();
      }
      else
        if (_useDebugLog)
          Log.Info("AudioscrobblerBase: {0}", "direct submit cancelled because of previous errors");
    }
       

    #region Public event triggers

    public static void TriggerSafeModeEvent()
    {
      if (_antiHammerCount < 5)
      {
        _antiHammerCount = _antiHammerCount + 1;
        DoHandshake(true);
        SUBMIT_INTERVAL = SUBMIT_INTERVAL * _antiHammerCount;
        // prevent null argument exception
        if (SUBMIT_INTERVAL == 0)
          SUBMIT_INTERVAL = 120;
        // reset the timer
        if (submitTimer != null)
        {
          submitTimer.Close();
          InitSubmitTimer();
        }
        Log.Info("AudioscrobblerBase: falling back to safe mode: new interval: {0} sec", Convert.ToString(SUBMIT_INTERVAL));
      }
    }

    #endregion

    #region Networking related functions
    /// <summary>
    /// Handshake with the Audioscrobbler service
    /// </summary>
    /// <returns>True if the connection was successful, false otherwise</returns>
    private static bool DoHandshake(bool forceNow_)
    {
      // Handle uninitialized username/password.
      if (username == "" || password == "")
      {
        Log.Info("AudioscrobblerBase: {0}", "user or password not defined");
        return false;
      }

      if (!forceNow_)
      {
        // Check whether we had a *successful* handshake recently.
        if (DateTime.Now < lastHandshake.Add(handshakeInterval))
        {
          string nexthandshake = lastHandshake.Add(handshakeInterval).ToString();
          string logmessage = "Next handshake due at " + nexthandshake;
          if (_useDebugLog)
            Log.Info("AudioscrobblerBase: {0}", logmessage);
          return true;
        }
      }

      //Log.Info("AudioscrobblerBase.DoHandshake: {0}", "Attempting handshake");
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
        Log.Info("AudioscrobblerBase: {0}", "Handshake failed");
        return false;
      }

      // Send the event.
      if (!_signedIn)
        _signedIn = true;

      lastHandshake = DateTime.Now;
      // reset to leave "safe mode"
      _antiHammerCount = 0;

      if (_useDebugLog)
        Log.Info("AudioscrobblerBase: {0}", "Handshake successful");
      return true;
    }
    

    /// <summary>
    /// Executes the given HTTP request and parses the response of the server.
    /// </summary>
    /// <param name="url_">The url to open</param>
    /// <param name="postdata_">Data to be sent via HTTP POST, an empty string for GET</param>
    /// <returns>True if the request was successfully completed, false otherwise</returns>
    private static bool GetResponse(string url_, string postdata_)
    {
      // Enforce a minimum wait time between connects.
      DateTime nextconnect = lastConnectAttempt.Add(minConnectWaitTime);
      if (DateTime.Now < nextconnect)
      {
        TimeSpan waittime = nextconnect - DateTime.Now;        
        string logmessage = "Avoiding too fast connects. Sleeping until "
                             + nextconnect.ToString();
        if (_useDebugLog)
          Log.Info("AudioscrobblerBase: {0}", logmessage);
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
        string logmessage = "WebRequest.Create failed: " + e.Message;
        Log.Info("AudioscrobblerBase.GetResponse: {0}", logmessage);
        return false;
      }

      // Attach POST data to the request, if any.
      if (postdata_ != "")
      {
        //Log.Info("AudioscrobblerBase.GetResponse: POST to {0}", url_);
        Log.Info("AudioscrobblerBase: Submitting data: {0}", postdata_);
        string logmessage = "Connecting to '" + url_ + "\nData: " + postdata_;
                
        try
        {
          byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postdata_);
          request.Method = "POST";
          request.ContentLength = postHeaderBytes.Length;
          request.ContentType = "application/x-www-form-urlencoded";

          // Create stream writer - this can also fail if we aren't connected
          Stream requestStream = request.GetRequestStream();
          requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
          requestStream.Close();
        }
        catch (Exception e)
        {
          logmessage = "HttpWebRequest.GetRequestStream: " + e.Message;
          Log.Info("AudioscrobblerBase.GetResponse: {0}", logmessage);
          return false;
        }
      }

      // Create the response object.
      if (_useDebugLog)
        Log.Info("AudioscrobblerBase: {0}", "Waiting for response");
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
        string logmessage = "HttpWebRequest.GetResponse: " + e.Message;
        Log.Info("AudioscrobblerBase.GetResponse: {0}", logmessage);
        return false;
      }

      // now we are connected
      if (_useDebugLog)
        Log.Info("AudioscrobblerBase: {0}", "Response received");

      bool success = false;
      bool parse_success = false;
      try
      {
        string respType = reader.ReadLine();
        if (respType == null)
        {
          Log.Info("AudioscrobblerBase.GetResponse: {0}", "Empty response from Audioscrobbler server.");
          return false;
        }

        // Parse the response.
        if (respType.StartsWith("UPTODATE"))
          success = parse_success = parseUpToDateMessage(respType, reader);
        else if (respType.StartsWith("UPDATE"))
          Log.Info("AudioscrobblerBase: {0}", "UPDATE needed!");
        else if (respType.StartsWith("OK"))
          success = parse_success = parseOkMessage(respType, reader);
        else if (respType.StartsWith("FAILED"))
          parse_success = parseFailedMessage(respType, reader);
        else if (respType.StartsWith("BADUSER") || respType.StartsWith("BADAUTH"))
          parse_success = parseBadUserMessage(respType, reader);
        else
        {
          string logmessage = "** CRITICAL ** Unknown response " + respType;
          Log.Info("AudioscrobblerBase: {0}", logmessage);
        }

        // read next line to look for an interval
        while ((respType = reader.ReadLine()) != null)
          if (respType.StartsWith("INTERVAL"))
            parse_success = parseIntervalMessage(respType, reader);
      }
      catch (Exception ex)
      {
        Log.Info("AudioscrobblerBase: Exception on reading response lines - {0}", ex.Message);
      }

      if (!parse_success)
      {
        return false;
      }

      //Log.Info("AudioscrobblerBase.GetResponse: {0}", "End");
      return success;
    }
    #endregion

    static void OnSubmitTimerTick(object trash_, ElapsedEventArgs args_)
    {
      if (!_disableTimerThread || _antiHammerCount > 0)
        StartSubmitQueueThread();
    }

    /// <summary>
    /// Creates a thread to submit all queued songs.
    /// </summary>
    private static void StartSubmitQueueThread()
    {
      submitThread = new Thread(new ThreadStart(SubmitQueue));
      submitThread.IsBackground = true;
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
      int _submittedSongs = 0;

      // Make sure that a connection is possible.
      if (!DoHandshake(false))
      {
        Log.Info("AudioscrobblerBase: {0}", "Handshake failed.");
        return;
      }
      // If the queue is empty, nothing else to do today.
      if (queue.Count <= 0)
      {
        if (_useDebugLog)
          Log.Info("AudioscrobblerBase: {0}", "Queue is empty");
        return;
      }

      // Only one thread should attempt to run through the queue at a time.
      lock (submitLock)
      {
        // Save the queue now since connecting to AS may time out, which
        // takes time, and the user could quit, losing one valuable song...
        queue.Save();

        // Build POST data from the username and the password.
        string webUsername = System.Web.HttpUtility.UrlEncode(username);
        string md5resp = HashPassword();
        string postData = "u=" + webUsername + "&s=" + md5resp;

        StringBuilder sb = new StringBuilder();

        sb.Append(postData);
        sb.Append(queue.GetTransmitInfo(out _submittedSongs));

        postData = sb.ToString();

        if (!postData.Contains("&a[0]"))
        {
          if (_useDebugLog)
            Log.Info("AudioscrobblerBase: postData did not contain info for {0}", "latest song");
          //if (_dismissOnError)
          //  ClearQueue();
          //else
          //  ResetQueue();
          return;
        }

        // Submit or die.
        if (!GetResponse(submitUrl, postData))
        {
          Log.Info("AudioscrobblerBase: {0}", "Submit failed.");
          return;
        }

        // Remove the submitted songs from the queue.
        lock (queueLock)
        {
          try
          {
            queue.RemoveRange(0, _submittedSongs);
            queue.Save();
          }
          catch (Exception ex)
          {
            Log.Info("AudioscrobblerBase: submit thread clearing cache - {0}", ex.Message);
          }
        }
      }
    }

    #endregion

    #region Audioscrobbler response parsers.
    private static bool parseUpToDateMessage(string type_, StreamReader reader_)
    {
      try
      {
        md5challenge = reader_.ReadLine().Trim();
        submitUrl = reader_.ReadLine().Trim();
      }
      catch (Exception e)
      {
        string logmessage = "Failed to parse UPTODATE response: " + e.Message;
        Log.Info("AudioscrobblerBase.parseUpToDateMessage: {0}", logmessage);
        md5challenge = "";
        return false;
      }
      if (_useDebugLog)
        Log.Info("AudioscrobblerBase: {0}", "Your client is up to date.");
      return true;
    }

    private static bool parseOkMessage(string type_, StreamReader reader_)
    {
      Log.Info("AudioscrobblerBase: {0}", "Action successfully completed.");
      return true;
    }

    private static bool parseFailedMessage(string type_, StreamReader reader_)
    {
      try
      {
        //Log.Info("AudioscrobblerBase.parseFailedMessage: {0}", "Called.");
        string logmessage = "";
        if (type_.Length > 7)
          logmessage = "FAILED: " + type_.Substring(7);
        else
          logmessage = "FAILED";
        if (_useDebugLog)
          Log.Info("AudioscrobblerBase: {0}", logmessage);
        if (logmessage == "FAILED: Plugin bug: Not all request variables are set")
          Log.Info("AudioscrobblerBase: A server error may have occured / if you receive this often a proxy may truncate your request - {0}", "read: http://www.last.fm/forum/24/_/74505/1#f808273");
        TriggerSafeModeEvent();
        return true;
      }
      catch (Exception e)
      {
        string logmessage = "Failed to parse FAILED response: " + e.Message;
        Log.Info("AudioscrobblerBase.parseFailedMessage: {0}", logmessage);
        return false;
      }
    }

    private static bool parseBadUserMessage(string type_, StreamReader reader_)
    {
      Log.Info("AudioscrobblerBase: {0}", "PLEASE CHECK YOUR ACCOUNT CONFIG! - re-trying handshake now");
      TriggerSafeModeEvent();
      return true;
    }

    private static bool parseIntervalMessage(string type_, StreamReader reader_)
    {
      try
      {
        string logmessage = "";
        if (type_.Length > 9)
        {
          int newInterval = Convert.ToInt32(type_.Substring(9));
          logmessage = "last.fm's servers currently allow an interval of: " + Convert.ToString(newInterval) + " sec";
          if (newInterval > 30)
            SUBMIT_INTERVAL = newInterval;
        }
        else
          logmessage = "INTERVAL";
        if (_useDebugLog)
          Log.Info("AudioscrobblerBase: {0}", logmessage);
      }
      catch (Exception ex)
      {
        string logmessage = "Failed to parse INTERVAL response: " + ex.Message;
        Log.Info("AudioscrobblerBase.parseIntervalMessage: {0}", logmessage);
      }
      return true;
    }
    #endregion

    #region Utilities
    private static void InitSubmitTimer()
    {
      submitTimer = new System.Timers.Timer();
      submitTimer.Interval = SUBMIT_INTERVAL * 1000;
      submitTimer.Elapsed += new ElapsedEventHandler(OnSubmitTimerTick);
      submitTimer.Start();
    }

    private static string HashPassword()
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
