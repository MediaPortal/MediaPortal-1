#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Player;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;

namespace MediaPortal.Freedb
{
  /// <summary>
  /// Summary description for FreeDBHttpImpl.
  /// </summary>
  public class FreeDBHttpImpl //: IFreeDB
  {
    private const string APPNAME = "MediaPortal";
    private const string APPVERSION = "1.0";
    private FreeDBSite m_server = null;
    private string m_serverURL = null;
    private string m_idStr = null;
    private string m_message = null;
    private int m_code = 0;

    public FreeDBHttpImpl()
    {
      // if BASS is not the default audio engine, we need to load the CD Plugin first
      if (!BassMusicPlayer.IsDefaultMusicPlayer)
      {
        // Load the CD Plugin
        string appPath = Application.StartupPath;
        string decoderFolderPath = Path.Combine(appPath, @"musicplayer\plugins\audio decoders");

        BassRegistration.BassRegistration.Register();
        int pluginHandle = Bass.BASS_PluginLoad(decoderFolderPath + "\\basscd.dll");
      }

      StringBuilder buff = new StringBuilder(512);
      buff.Append("&hello=");
      buff.Append(Environment.UserName.Replace(" ", "_"));
      buff.Append('+');
      buff.Append(Environment.MachineName);
      buff.Append('+');
      buff.Append(APPNAME);
      buff.Append('+');
      buff.Append(APPVERSION);
      buff.Append('+');
      buff.Append("&proto=5");
      m_idStr = buff.ToString();
    }

    public bool Connect()
    {
      m_server = new FreeDBSite("freedb.freedb.org", FreeDBSite.FreeDBProtocol.HTTP, 80, "/~cddb/cddb.cgi",
                                "N000.00", "W000.00", "Random freedb server");

      m_serverURL = "http://" + m_server.Host + ":" + m_server.Port + m_server.URI;

      return true;
    }

    public bool Connect(FreeDBSite site)
    {
      m_server = site;
      m_serverURL = "http://" + m_server.Host + ":" + m_server.Port + m_server.URI;
      return true;
    }

    public bool Disconnect()
    {
      return true;
    }

    public FreeDBSite[] GetFeedbSites()
    {
      FreeDBSite[] retval = null;
      using (StreamReader urlRdr = GetStreamFromSite("sites"))
      {
        m_message = urlRdr.ReadLine();

        int code = GetCode(m_message);
        m_message = m_message.Substring(4); // remove the code...
        char[] sep = {' '};


        switch (code)
        {
          case 210: // OK, Site Information Follows.
            // Read in all sites.
            string[] sites = ParseMultiLine(urlRdr);
            retval = new FreeDBSite[sites.Length];
            int index = 0;
            // Loop through server list and extract different parts.
            foreach (string site in sites)
            {
              string loc = "";
              string[] siteInfo = site.Split(sep);
              retval[index] = new FreeDBSite();
              retval[index].Host = siteInfo[0];
              retval[index].Protocol =
                (FreeDBSite.FreeDBProtocol)Enum.Parse(typeof (FreeDBSite.FreeDBProtocol), siteInfo[1], true);
              retval[index].Port = Convert.ToInt32(siteInfo[2]);
              retval[index].URI = siteInfo[3];
              retval[index].Latitude = siteInfo[4];
              retval[index].Longitude = siteInfo[5];

              for (int i = 6; i < siteInfo.Length; i++)
              {
                loc += retval[i] + " ";
              }
              retval[index].Location = loc;
              index++;
            }
            break;
          case 401: // No Site Information Available.
            break;
            ;
          default:
            break;
        }
      }
      return retval;
    }

    public string GetServerMessage()
    {
      return m_message;
    }

    public string[] GetListOfGenres()
    {
      return GetInfo("cddb+lscat");
    }

    public string[] GetHelp(string topic)
    {
      return GetInfo("help " + topic);
    }

    public string[] GetLog()
    {
      return GetInfo("log");
    }

    public string[] GetMessageOfTheDay()
    {
      return GetInfo("motd");
    }

    public string[] GetStatus()
    {
      return GetInfo("stat");
    }

    public string[] GetUsers()
    {
      return GetInfo("whom");
    }

    public string GetVersion()
    {
      GetInfo("ver", false);
      return GetServerMessage();
    }

    public CDInfoDetail GetDiscDetails(string category, string discid)
    {
      string[] content = GetInfo("cddb+read+" + category + "+" + discid);
      XMCDParser parser = new XMCDParser();
      CDInfoDetail cdInfo = parser.Parse2(content);
      return cdInfo;
    }

    public CDInfo[] GetDiscInfo(char driveLetter)
    {
      CDInfo[] retval = null;
      string command = "cddb+query+" + GetCDDBDiscIDInfo(driveLetter, '+');
      using (StreamReader urlRdr = GetStreamFromSite(command))
      {
        m_message = urlRdr.ReadLine();

        int code = GetCode(m_message);
        m_message = m_message.Substring(4); // remove the code...

        char[] sep = {' '};
        string title = "";
        int index = 0;
        string[] match;
        string[] matches;

        switch (code)
        {
          case 200: // Exact Match...
            match = m_message.Split(sep);
            retval = new CDInfo[1];

            retval[0] = new CDInfo();
            retval[0].Category = match[0];
            retval[0].DiscId = match[1];
            for (int i = 2; i < match.Length; i++)
            {
              title += match[i] + " ";
            }
            retval[0].Title = title.Trim();
            break;
          case 202: // no match found
            break;
          case 211: // Found Inexact Matches. List Follows.
          case 210: // Found Exact Matches. List Follows.
            matches = ParseMultiLine(urlRdr);
            retval = new CDInfo[matches.Length];
            foreach (string line in matches)
            {
              match = line.Split(sep);

              retval[index] = new CDInfo();
              retval[index].Category = match[0];
              retval[index].DiscId = match[1];
              for (int i = 2; i < match.Length; i++)
              {
                title += match[i] + " ";
              }
              retval[index].Title = title.Trim();
              index++;
            }
            break;
          case 403: // Database Entry is Corrupt.
            retval = null;
            break;
          case 409: // No handshake... Should not happen!
            retval = null;
            break;
          default:
            retval = null;
            break;
        }
      }
      return retval;
    }

    public CDInfo[] GetDiscInfoByID(string ID)
    {
      CDInfo[] retval = null;
      string command = "cddb+query+" + ID.Replace(" ", "+");
      using (StreamReader urlRdr = GetStreamFromSite(command))
      {
        m_message = urlRdr.ReadLine();

        int code = GetCode(m_message);
        m_message = m_message.Substring(4); // remove the code...

        char[] sep = {' '};
        string title = "";
        int index = 0;
        string[] match;
        string[] matches;

        switch (code)
        {
          case 200: // Exact Match...
            match = m_message.Split(sep);
            retval = new CDInfo[1];

            retval[0] = new CDInfo();
            retval[0].Category = match[0];
            retval[0].DiscId = match[1];
            for (int i = 2; i < match.Length; i++)
            {
              title += match[i] + " ";
            }
            retval[0].Title = title.Trim();
            break;
          case 202: // no match found
            break;
          case 211: // Found Inexact Matches. List Follows.
          case 210: // Found Exact Matches. List Follows.
            matches = ParseMultiLine(urlRdr);
            retval = new CDInfo[matches.Length];
            foreach (string line in matches)
            {
              match = line.Split(sep);

              retval[index] = new CDInfo();
              retval[index].Category = match[0];
              retval[index].DiscId = match[1];
              for (int i = 2; i < match.Length; i++)
              {
                title += match[i] + " ";
              }
              retval[index].Title = title.Trim();
              index++;
            }
            break;
          case 403: // Database Entry is Corrupt.
            retval = null;
            break;
          case 409: // No handshake... Should not happen!
            retval = null;
            break;
          default:
            retval = null;
            break;
        }
      }
      return retval;
    }

    private string[] GetInfo(string command)
    {
      return GetInfo(command, true);
    }

    private string[] GetInfo(string command, bool multipleLine)
    {
      string[] retval = null;
      using (StreamReader urlRdr = GetStreamFromSite(command))
      {
        m_message = urlRdr.ReadLine();

        int code = GetCode(m_message);
        m_message = m_message.Substring(4); // remove the code...

        switch (code / 100)
        {
          case 2: // no problem
            retval = ParseMultiLine(urlRdr);
            break;
          case 4: // no permission
            retval = null;
            break;
          case 5: // problem
            retval = null;
            break;
          default:
            retval = null;
            break;
        }
      }
      return retval;
    }

    private StreamReader GetStreamFromSite(string command)
    {
      Uri url = new Uri(m_serverURL + "?cmd=" + command + m_idStr);

      WebRequest req = WebRequest.Create(url);
      try
      {
        // Use the current user in case an NTLM Proxy or similar is used.
        // wr.Proxy = WebProxy.GetDefaultProxy();
        req.Proxy.Credentials = CredentialCache.DefaultCredentials;
      }
      catch (Exception) {}
      StreamReader urlRdr = new StreamReader(new StreamReader(req.GetResponse().GetResponseStream()).BaseStream,
                                             Encoding.GetEncoding(0));

      return urlRdr;
    }

    private int GetCode(string content)
    {
      m_code = Convert.ToInt32(content.Substring(0, 3));
      return m_code;
    }

    private string[] ParseMultiLine(StreamReader streamReader)
    {
      ArrayList strarray = new ArrayList();
      string curLine;

      while ((curLine = streamReader.ReadLine()) != null)
      {
        curLine = curLine.Trim();
        if (curLine.Trim().Length > 0 && !curLine.Trim().Equals("."))
        {
          strarray.Add(curLine);
        }
      }
      return (string[])strarray.ToArray(typeof (string));
    }

    private int Drive2BassID(char driveLetter)
    {
      for (int i = 0; i < 25; i++)
      {
        if (BassCd.BASS_CD_GetInfo(i).DriveLetter == driveLetter)
        {
          return i;
        }
      }
      return -1;
    }

    public string GetCDDBDiscIDInfo(char driveLetter, char separator)
    {
      string retval = null;
      int drive = Drive2BassID(driveLetter);
      if (drive > -1)
      {
        string id = BassCd.BASS_CD_GetID(drive, BASSCDId.BASS_CDID_CDDB);
        retval = id.Replace(' ', separator);
        BassCd.BASS_CD_Release(drive);
      }
      return retval;
    }

    public string GetCDDBDiscID(char driveLetter)
    {
      string retval = null;
      int drive = Drive2BassID(driveLetter);
      if (drive > -1)
      {
        string id = BassCd.BASS_CD_GetID(drive, BASSCDId.BASS_CDID_CDDB);
        retval = id.Substring(0, 8);
        BassCd.BASS_CD_Release(drive);
      }

      return retval;
    }

    // hwahrmann 20/06/2007
    // The following Code got replaced by using BASS CD, cause it was interfering with BASS Audio CD Playback
    /*
    public string GetCDDBDiscIDInfo (char driveLetter, char separator)
    {   
      string retval = null;
      CDDrive cddrive = new CDDrive();
      if ( cddrive.Open(driveLetter) )
      {
        if ( cddrive.IsCDReady() )
        {
          if ( cddrive.Refresh() )
          {
            string[] offsets = cddrive.GetFreeDBTrackOffsets();
            StringBuilder buff = new StringBuilder(512);

            buff.Append(cddrive.GetFreeDBDiscID());
            buff.Append(separator);
            buff.Append(cddrive.GetNumTracks());
            buff.Append(separator);
            for(int i = 0; i < offsets.Length; i++)
            {
              buff.Append(offsets[i]);
              buff.Append(separator);
            }
            buff.Append(cddrive.GetFreeDBTime());
            retval = buff.ToString();
          }
        }
        cddrive.Close();
      }
      return retval;
    }

		public string GetCDDBDiscID (char driveLetter)
		{   
			string retval = null;
			CDDrive cddrive = new CDDrive();
			if ( cddrive.Open(driveLetter) )
			{
				if ( cddrive.IsCDReady() )
				{
					if ( cddrive.Refresh() )
					{
						retval = cddrive.GetFreeDBDiscID();
					}
				}
        cddrive.Close();
			}
			return retval;
		}
    */
  }
}