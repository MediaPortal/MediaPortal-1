#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2006 Team MediaPortal
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
using System.IO;
using System.Net;
using System.Text;

using MediaPortal.GUI.Library;

namespace MediaPortal.Music.Database
{
  public class AudioscrobblerRadio
  {
    // constructor
    public AudioscrobblerRadio()
    {
      LoadSettings();
    }

    static private string _currentRadioURL = String.Empty;
    private string _currentSession = String.Empty;
    private bool _isSubscriber = false;

    // TO DO: 
    // Steps to get a stream:
    // 1. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/adjust.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&url=lastfm://globaltags/alternative%20rock,ebm,progressive%20rock
    // 2. http.request.uri = Request URI: http://streamer1.last.fm/last.mp3?Session=e5b0c80f5b5d0937d407fb77a913cb6a
    // 3. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/control.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&command=rtp
    // 4. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/adjust.php?session=e5b0c80f5b5d0937d407fb77a913cb6a&url=lastfm://settings/discovery/off
    // 5. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/np.php?session=e5b0c80f5b5d0937d407fb77a913cb6a


    // TASKS:
    // Stopwatch and Parser for nowplaying
    // SKIP Button

    private void LoadSettings()
    {
      _currentSession = AudioscrobblerBase.RadioSession;
      _isSubscriber = AudioscrobblerBase.Subscriber;
      _currentRadioURL = "http://streamer1.last.fm/last.mp3?Session=" + _currentSession;
      // some testing here..
      if (SendCommandRequest("http://ws.audioscrobbler.com/radio/control.php?session=" + _currentSession + "&command=rtp"))
        Log.Info("AudioscrobblerRadio: Stream adjusted");
      if (SendCommandRequest("http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + "&url=lastfm://globaltags/Metal,Viking+Metal,Melodic+Death+Metal"))
        Log.Info("AudioscrobblerRadio: Streaming: metal, viking metal, melodic death metal");
    }


    static public string CurrentStream
    {
      get
      {
        return _currentRadioURL;
      }
    }




    private bool SendCommandRequest(string url_)
    {
      HttpWebRequest request = null;

      // send the command
      try
      {
        request = (HttpWebRequest)WebRequest.Create(url_);
        if (request == null)
          throw (new Exception());
      }
      catch (Exception e)
      {
        Log.Error("AudioscrobblerRadio: SendCommandRequest failed - {0}", e.Message);
        return false;
      }

      StreamReader reader = null;

      // get the response
      try
      {

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        if (response == null)
          throw (new Exception());

        reader = new StreamReader(response.GetResponseStream());
      }
      catch (Exception e)
      {
        Log.Error("AudioscrobblerRadio: SendCommandRequest: Response failed {0}", e.Message);
        return false;
      }

      // parse the response
      try
      {
        string responseMessage = reader.ReadLine();

        if (responseMessage.StartsWith("response=OK"))
          return true;
        else
        {
          string logmessage = responseMessage;
          while ((responseMessage = reader.ReadLine()) != null)
            logmessage += "\n" + responseMessage;          
        }
      }
      catch (Exception e)
      {
        Log.Error("AudioscrobblerRadio: SendCommandRequest: Parsing response failed {0}", e.Message);
        return false;
      }

      return false;
    }
  }
}
