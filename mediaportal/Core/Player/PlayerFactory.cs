/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System;
using System.Collections;

using System.Reflection;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

namespace MediaPortal.Player
{
  /// <summary>
  /// 
  /// </summary>
  public class PlayerFactory
  {
    static ArrayList m_externalPlayers = new ArrayList();
    static bool m_loadedExternalPlayers = false;

    static private void LoadExternalPlayers()
    {

      Log.Write("Loading external players plugins");

      string[] strFiles = System.IO.Directory.GetFiles(@"plugins\ExternalPlayers", "*.dll");

      foreach (string strFile in strFiles)
      {

        try
        {

          Assembly assem = Assembly.LoadFrom(strFile);

          if (assem != null)
          {

            Type[] types = assem.GetExportedTypes();



            foreach (Type t in types)
            {

              try
              {

                if (t.IsClass)
                {

                  if (t.IsSubclassOf(typeof(IExternalPlayer)))
                  {

                    object newObj = (object)Activator.CreateInstance(t);

                    Log.Write("  found plugin:{0} in {1}", t.ToString(), strFile);

                    IExternalPlayer player = (IExternalPlayer)newObj;

                    Log.Write("  player:{0}.  author: {1}", player.PlayerName, player.AuthorName);

                    m_externalPlayers.Add(player);

                  }

                }

              }

              catch (Exception e)
              {

                Log.Write("Error loading external player: {0}", t.ToString());

                Log.Write("Error: {0}", e.StackTrace);

              }

            }

          }

        }

        catch (Exception e)
        {

          Log.Write("Error loading external player: {0}", e);

        }

      }

      m_loadedExternalPlayers = true;

    }

    static public IExternalPlayer GetExternalPlayer(string strFile)
    {

      if (!m_loadedExternalPlayers)
      {
        LoadExternalPlayers();
      }

      foreach (IExternalPlayer player in m_externalPlayers)
      {

        using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
        {

          bool enabled = xmlreader.GetValueAsBool("plugins", player.PlayerName, false);

          player.Enabled = enabled;

        }

        if (player.Enabled && player.SupportsFile(strFile))
        {

          return player;

        }

      }

      return null;

    }

    static public IPlayer Create(string strFileName)
    {
      IPlayer newPlayer = null;

      string strExt = System.IO.Path.GetExtension(strFileName).ToLower();
      if (strExt != ".tv" && strExt != ".sbe" && strExt != ".dvr-ms"
              && strFileName.ToLower().IndexOf("live.ts") < 0
              && strFileName.ToLower().IndexOf("radio.ts") < 0)
      {
        newPlayer = GetExternalPlayer(strFileName);
        if (newPlayer != null)
        {
          return newPlayer;
        }
      }



      if (Utils.IsVideo(strFileName))
      {
        if (strExt == ".tv" || strExt == ".sbe" || strExt == ".dvr-ms")
        {
          if (strExt == ".sbe" || strExt == ".dvr-ms")
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT, 0, 0, 0, 0, 0, null);
            GUIWindowManager.SendMessage(msg);
          }
         
          newPlayer = new Player.StreamBufferPlayer9();
          return newPlayer;
        }
      }
      if (strExt == ".ts")
      {
        if (strFileName.ToLower().IndexOf("radio.ts") >= 0)
          return new Player.BaseTStreamBufferPlayer();

        newPlayer = new Player.TStreamBufferPlayer9();
        return newPlayer;
      }
      if (Utils.IsVideo(strFileName))
      {
        newPlayer = new Player.VideoPlayerVMR9();
        return newPlayer;
      }

      if (strExt == ".radio")
      {
        newPlayer = new Player.RadioTuner();
        return newPlayer;
      }

      if (Utils.IsCDDA(strFileName))
      {
        newPlayer = new Player.AudioPlayerWMP9();

        return newPlayer;
      }

      if (Utils.IsAudio(strFileName))
      {
        using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
        {
          string strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "player", "Windows Media Player 9");
          if (String.Compare(strAudioPlayer, "Windows Media Player 9", true) == 0)
          {
            newPlayer = new Player.AudioPlayerWMP9();
            return newPlayer;
          }
          newPlayer = new Player.AudioPlayerVMR7();
          return newPlayer;
        }
      }


      newPlayer = new Player.AudioPlayerWMP9();
      return newPlayer;

    }
  }
}
