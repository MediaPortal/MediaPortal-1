#region Copyright (C) 2005-2006 Team MediaPortal

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
using System.Reflection;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Utils.Services;

namespace MediaPortal.Player
{
  /// <summary>
  /// 
  /// </summary>
  public class PlayerFactory : IPlayerFactory
  {
    static ArrayList _externalPlayerList = new ArrayList();
    static bool _externalPlayersLoaded = false;
    static ILog _log;

    public PlayerFactory()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    private void LoadExternalPlayers()
    {
      _log.Info("Loading external players plugins");
      string[] fileList = System.IO.Directory.GetFiles(@"plugins\ExternalPlayers", "*.dll");
      foreach (string fileName in fileList)
      {
        try
        {
          Assembly assem = Assembly.LoadFrom(fileName);
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
                    _log.Info("  found plugin:{0} in {1}", t.ToString(), fileName);

                    IExternalPlayer player = (IExternalPlayer)newObj;
                    _log.Info("  player:{0}.  author: {1}", player.PlayerName, player.AuthorName);
                    _externalPlayerList.Add(player);
                  }
                }
              }
              catch (Exception e)
              {
                _log.Info("Error loading external player: {0}", t.ToString());
                _log.Info("Error: {0}", e.StackTrace);
              }
            }
          }
        }
        catch (Exception e)
        {
          _log.Info("Error loading external player: {0}", e);
        }
      }
      _externalPlayersLoaded = true;
    }

    public IExternalPlayer GetExternalPlayer(string fileName)
    {
      if (!_externalPlayersLoaded)
      {
        LoadExternalPlayers();
      }

      foreach (IExternalPlayer player in _externalPlayerList)
      {
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          bool enabled = xmlreader.GetValueAsBool("plugins", player.PlayerName, false);
          player.Enabled = enabled;
        }

        if (player.Enabled && player.SupportsFile(fileName))
        {
          return player;
        }
      }
      return null;
    }

    public IPlayer Create(string fileName)
    {
      IPlayer newPlayer = null;
      if (fileName.ToLower().IndexOf("rtsp:") >= 0)
      {
        return new RTSPPlayer();
      }
      string extension = System.IO.Path.GetExtension(fileName).ToLower();
      if (extension != ".tv" && extension != ".sbe" && extension != ".dvr-ms"
              && fileName.ToLower().IndexOf(".tsbuffer") < 0
              && fileName.ToLower().IndexOf("radio.tsbuffer") < 0)
      {
        newPlayer = GetExternalPlayer(fileName);
        if (newPlayer != null)
        {
          if (!GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow))
          {
            _log.Info("PlayerFactory: Disabling DX9 exclusive mode");
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
            GUIWindowManager.SendMessage(msg);
          }
          return newPlayer;
        }
      }

      if (MediaPortal.Util.Utils.IsVideo(fileName))
      {
        if (extension == ".tv" || extension == ".sbe" || extension == ".dvr-ms")
        {
          if (extension == ".sbe" || extension == ".dvr-ms")
          {
            //GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT, 0, 0, 0, 0, 0, null);
            //GUIWindowManager.SendMessage(msg);
          }

          newPlayer = new Player.StreamBufferPlayer9();
          return newPlayer;
        }
      }
      if (extension == ".tsbuffer" || extension == ".ts")
      {
        if (fileName.ToLower().IndexOf("radio.tsbuffer") >= 0)
          return new Player.BaseTStreamBufferPlayer();

        newPlayer = new Player.TStreamBufferPlayer9();
        return newPlayer;
      }
      if (!MediaPortal.Util.Utils.IsAVStream(fileName) && MediaPortal.Util.Utils.IsVideo(fileName))
      {
        newPlayer = new Player.VideoPlayerVMR9();
        return newPlayer;
      }

      if (extension == ".radio")
      {
        newPlayer = new Player.RadioTuner();
        return newPlayer;
      }

      if (MediaPortal.Util.Utils.IsCDDA(fileName))
      {
        newPlayer = new Player.AudioPlayerWMP9();

        return newPlayer;
      }

      if (MediaPortal.Util.Utils.IsAudio(fileName))
      {
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
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
