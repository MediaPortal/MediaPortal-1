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
using System.Timers;
using System.Security.Cryptography;
using System.Collections;
using System.Runtime.InteropServices;
using System.Web;

using MediaPortal.Util;
using MediaPortal.GUI.Library;

namespace MediaPortal.Music.Database
{
  public class AudioscrobblerEngine
  {
    enum State
    {
      IDLE,
      NEED_HANDSHAKE,
      NEED_TRANSMIT,
      WAITING_FOR_REQ_STREAM,
      WAITING_FOR_HANDSHAKE_RESP,
      WAITING_FOR_RESP
    };

    [Flags]
    enum ConnectionState : int
    {
      INTERNET_CONNECTION_MODEM = 0x1,
      INTERNET_CONNECTION_LAN = 0x2,
      INTERNET_CONNECTION_PROXY = 0x4,
      INTERNET_RAS_INSTALLED = 0x10,
      INTERNET_CONNECTION_OFFLINE = 0x20,
      INTERNET_CONNECTION_CONFIGURED = 0x40
    }

    const int TICK_INTERVAL = 2000; /* 2 seconds */
    const int FAILURE_LOG_MINUTES = 5; /* 5 minute delay on logging failure to upload information */
    const int RETRY_SECONDS = 60; /* 60 second delay for transmission retries */
    const string CLIENT_ID = "mpm";
    const string CLIENT_VERSION = "0.1";
    const string SCROBBLER_URL = "http://post.audioscrobbler.com/";
    const string SCROBBLER_VERSION = "1.1";

    string username;
    string md5_pass;
    string post_url;
    string security_token;

    uint timeout_id;
    uint timeoutTicks;
    DateTime next_interval;
    DateTime last_upload_failed_logged;

    private System.Timers.Timer engineTimer;

    AudioscrobblerQueue queue;

    bool connected = false;
    //bool queued; /* if current_track has been queued */

    WebRequest current_web_req;
    IAsyncResult current_async_result;
    State state;

    // check connectivity http://www.developerfusion.co.uk/show/5346/
    [DllImport("wininet.dll", CharSet = CharSet.Auto)]
    static extern bool InternetGetConnectedState(ref ConnectionState lpdwFlags, int dwReserved); 


    public AudioscrobblerEngine()
    {
      timeout_id = 0;
      timeoutTicks = 0;
      state = State.IDLE;
      queue = new AudioscrobblerQueue();
    }

    bool CheckInternetConnection(bool force)
    {
      if (!connected || force)
      {
        ConnectionState Description = 0;
        string connState = InternetGetConnectedState(ref Description, 0).ToString();
        connected = Convert.ToBoolean(connState);
        Log.Write("AudioscrobblerEngine: check connection - {0}", Description.ToString());
      }
      return connected;
    }

    private void InitEngineTimer()
    {
      engineTimer = new System.Timers.Timer();
      engineTimer.Interval = TICK_INTERVAL;
      engineTimer.Elapsed += new ElapsedEventHandler(StateTransitionHandler);
      engineTimer.Start();
    }

    public void Start()
    {
      //song_started = false;
      //PlayerEngineCore.EventChanged += OnPlayerEngineEventChanged;
      //if (timeout_id == 0)
      //{
      //  timeout_id = Timeout.Add(TICK_INTERVAL, StateTransitionHandler);
      //}
      //queue.Load();
      InitEngineTimer();
    }

    public void Stop()
    {
      //PlayerEngineCore.EventChanged -= OnPlayerEngineEventChanged;

      if (timeout_id != 0)
      {
        //  GLib.Source.Remove (timeout_id);
        timeout_id = 0;
      }

      if (current_web_req != null)
      {
        current_web_req.Abort();
      }

      queue.Save();
      Log.Write("AudioscrobblerEngine: shutdown");
    }

    public void SetUserPassword(string username, string pass)
    {
      if (username == "" || pass == "")
        return;

      this.username = username;
      this.md5_pass = MD5Encode(pass);

      if (security_token != null)
      {
        security_token = null;
        state = State.NEED_HANDSHAKE;
      }
    }

    string MD5Encode(string pass)
    {
      if (pass == null || pass == String.Empty)
        return String.Empty;

      MD5 md5 = MD5.Create();

      byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(pass));

      return CryptoConvert.ToHex(hash).ToLower();
    }

    public void AddSongToScrobblerQueue(Song currentTrack_)
    {
      if (currentTrack_ == null)
      {
        //queued = false;
      }
      else
      {
        queue.Add(currentTrack_, currentTrack_.DateTimePlayed);
      }
    }

    void StateTransitionHandler(object trash_, ElapsedEventArgs args_)
    {
      /* if we're not connected, don't bother doing anything
       * involving the network. */
      if (!CheckInternetConnection(false))
        return;

      /* and address changes in our engine state */
      switch (state)
      {
        case State.IDLE:
          if (queue.Count > 0)
          {
            if (username != null && md5_pass != null && security_token == null)
              state = State.NEED_HANDSHAKE;
            else
              state = State.NEED_TRANSMIT;
          }
          break;
        case State.NEED_HANDSHAKE:
          if (DateTime.Now > next_interval)
          {
            Handshake();
          }
          break;
        case State.NEED_TRANSMIT:
          if (DateTime.Now > next_interval)
          {
            TransmitQueue();
          }
          break;
        case State.WAITING_FOR_RESP:
        case State.WAITING_FOR_REQ_STREAM:
          timeoutTicks++;
          if (timeoutTicks * TICK_INTERVAL > 20000)
          {
            Log.Write("AudioscrobblerEngine: current request timed out - {0}", "aborting..");
            current_web_req.Abort();
            state = State.IDLE;
          }
          break;
        case State.WAITING_FOR_HANDSHAKE_RESP:
          /* nothing here */
          break;
      }

      return;
    }

    //
    // Async code for transmitting the current queue of tracks
    //
    class TransmitState
    {
      public StringBuilder StringBuilder;
      public int Count;
    }

    void TransmitQueue()
    {
      int num_tracks_transmitted;

      /* save here in case we're interrupted before we complete
       * the request.  we save it again when we get an OK back
       * from the server */
      queue.Save();

      next_interval = DateTime.MinValue;

      if (post_url == null)
      {
        return;
      }

      StringBuilder sb = new StringBuilder();

      sb.AppendFormat("u={0}&s={1}", HttpUtility.UrlEncode(username), security_token);

      sb.Append(queue.GetTransmitInfo(out num_tracks_transmitted));

      current_web_req = WebRequest.Create(post_url);
      current_web_req.Method = "POST";
      current_web_req.ContentType = "application/x-www-form-urlencoded";
      current_web_req.ContentLength = sb.Length;

      TransmitState ts = new TransmitState();
      ts.Count = num_tracks_transmitted;
      ts.StringBuilder = sb;

      state = State.WAITING_FOR_REQ_STREAM;
      Log.Write("AudioscrobblerEngine: submitting - {0}", ts);
      current_async_result = current_web_req.BeginGetRequestStream(TransmitGetRequestStream, ts);
      ts = null;
      if (current_async_result == null)
      {
        next_interval = DateTime.Now + new TimeSpan(0, 0, RETRY_SECONDS);
        state = State.IDLE;
      }

    }

    void TransmitGetRequestStream(IAsyncResult ar)
    {
      Stream stream;

      try
      {
        stream = current_web_req.EndGetRequestStream(ar);
      }
      catch (Exception e)
      {
        Log.Write("AudioscrobblerEngine: Failed to get the request stream: {0}", e.Message);

        state = State.IDLE;
        next_interval = DateTime.Now + new TimeSpan(0, 0, RETRY_SECONDS);
        return;
      }

      TransmitState ts = (TransmitState)ar.AsyncState;
      StringBuilder sb = ts.StringBuilder;

      StreamWriter writer = new StreamWriter(stream);
      writer.Write(sb.ToString());
      writer.Close();

      state = State.WAITING_FOR_RESP;
      current_async_result = current_web_req.BeginGetResponse(TransmitGetResponse, ts);
      if (current_async_result == null)
      {
        next_interval = DateTime.Now + new TimeSpan(0, 0, RETRY_SECONDS);
        state = State.IDLE;
      }
    }

    void TransmitGetResponse(IAsyncResult ar)
    {
      WebResponse resp;

      try
      {
        resp = current_web_req.EndGetResponse(ar);
      }
      catch (Exception e)
      {
        Log.Write("AudioscrobblerEngine: Failed to get the response: {0}", e.Message);

        state = State.IDLE;
        next_interval = DateTime.Now + new TimeSpan(0, 0, RETRY_SECONDS);
        return;
      }

      TransmitState ts = (TransmitState)ar.AsyncState;

      Stream s = resp.GetResponseStream();

      StreamReader sr = new StreamReader(s, Encoding.UTF8);

      string line;
      line = sr.ReadLine();

      DateTime now = DateTime.Now;
      if (line.StartsWith("FAILED"))
      {
        if (now - last_upload_failed_logged > TimeSpan.FromMinutes(FAILURE_LOG_MINUTES))
        {
          Log.Write("AudioscrobblerEngine: submit failed - {0}", line.Substring("FAILED".Length).Trim());
          last_upload_failed_logged = now;
        }
        /* retransmit the queue on the next interval */
        state = State.NEED_TRANSMIT;
      }
      else if (line.StartsWith("BADUSER") || line.StartsWith("BADAUTH"))
      {
        if (now - last_upload_failed_logged > TimeSpan.FromMinutes(FAILURE_LOG_MINUTES))
        {
          Log.Write("AudioscrobblerEngine: submit failed - {0}", "invalid authentication");
          last_upload_failed_logged = now;
        }
        /* attempt to re-handshake (and retransmit) on the next interval */
        security_token = null;
        next_interval = DateTime.Now + new TimeSpan(0, 0, RETRY_SECONDS);
        state = State.IDLE;
        return;
      }
      else if (line.StartsWith("OK"))
      {
        /* if we've previously logged failures, be nice and log the successful upload. */
        if (last_upload_failed_logged != DateTime.MinValue)
        {
          //LogCore.Instance.PushInformation("Audioscrobbler upload succeeded", "", false);
          Log.Write("AudioscrobblerEngine: recovered from error");
          last_upload_failed_logged = DateTime.MinValue;
        }
        /* we succeeded, pop the elements off our queue */
        queue.RemoveRange(0, ts.Count);
        queue.Save();
        state = State.IDLE;
        Log.Write("AudioscrobblerEngine: submit successful");
      }
      else
      {
        if (now - last_upload_failed_logged > TimeSpan.FromMinutes(FAILURE_LOG_MINUTES))
        {
          //LogCore.Instance.PushDebug("Audioscrobbler upload failed", String.Format("Unrecognized response: {0}", line), false);
          Log.Write("AudioscrobblerEngine: submit failed - {0}", String.Format("Unrecognized response: {0}", line));
          last_upload_failed_logged = now;
        }
        state = State.IDLE;
      }

      /* now get the next interval */
      try
      {
        line = sr.ReadLine();
        if (line.StartsWith("INTERVAL"))
        {
          int interval_seconds = Int32.Parse(line.Substring("INTERVAL".Length));
          next_interval = DateTime.Now + new TimeSpan(0, 0, interval_seconds);
        }
        else
        {
          Log.Write("AudioscrobblerEngine: No INTERVAL received");
        }
      }
      catch (Exception ex)
      {
        Log.Write("AudioscrobblerEngine: Exception reading the INTERVAL - {0}", ex.Message);
      }
    }

    //
    // Async code for handshaking
    //
    void Handshake()
    {
      string uri = String.Format("{0}?hs=true&p={1}&c={2}&v={3}&u={4}",
                    SCROBBLER_URL,
                    SCROBBLER_VERSION,
                    CLIENT_ID, CLIENT_VERSION,
                    HttpUtility.UrlEncode(username));

      current_web_req = WebRequest.Create(uri);

      state = State.WAITING_FOR_HANDSHAKE_RESP;
      current_async_result = current_web_req.BeginGetResponse(HandshakeGetResponse, null);
      if (current_async_result == null)
      {
        next_interval = DateTime.Now + new TimeSpan(0, 0, RETRY_SECONDS);
        state = State.IDLE;
      }
    }

    void HandshakeGetResponse(IAsyncResult ar)
    {
      bool success = false;
      WebResponse resp;

      try
      {
        resp = current_web_req.EndGetResponse(ar);
      }
      catch (Exception e)
      {
        Log.Write("AudioscrobblerEngine: handshake failed {0}", e.Message);
        /* back off for a time before trying again */
        state = State.IDLE;
        next_interval = DateTime.Now + new TimeSpan(0, 0, RETRY_SECONDS);
        return;
      }

      Stream s = resp.GetResponseStream();

      StreamReader sr = new StreamReader(s, Encoding.UTF8);

      string line;

      line = sr.ReadLine();
      if (line.StartsWith("FAILED"))
      {
        Log.Write("AudioscrobblerEngine: sign-on failed - {0}", line.Substring("FAILED".Length).Trim());
      }
      else if (line.StartsWith("BADUSER"))
      {
        Log.Write("AudioscrobblerEngine: sign-on failed - {0}", "unrecognized user/password");
      }
      else if (line.StartsWith("UPDATE"))
      {
        Log.Write("AudioscrobblerEngine: plugin needs an update");
        success = true;
      }
      else if (line.StartsWith("UPTODATE"))
      {
        Log.Write("AudioscrobblerEngine: signed on");
        success = true;
      }

      /* read the challenge string and post url, if
       * this was a successful handshake */
      if (success == true)
      {
        string challenge = sr.ReadLine().Trim();
        post_url = sr.ReadLine().Trim();

        security_token = MD5Encode(md5_pass + challenge);
        //Console.WriteLine ("security token = {0}", security_token);
      }

      /* read the trailing interval */
      line = sr.ReadLine();
      if (line.StartsWith("INTERVAL"))
      {
        int interval_seconds = Int32.Parse(line.Substring("INTERVAL".Length));
        next_interval = DateTime.Now + new TimeSpan(0, 0, interval_seconds);
      }
      else
      {
        Log.Write("AudioscrobblerEngine: No INTERVAL received");
      }

      /* XXX we shouldn't just try to handshake again for BADUSER */
      state = success ? State.IDLE : State.NEED_HANDSHAKE;
    }
  }
}