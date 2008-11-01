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

using System;
using System.IO;
using System.Collections;
using System.Reflection;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

namespace MediaPortal.Player
{
  /// <summary>
  /// 
  /// </summary>
  public class PlayerFactory : IPlayerFactory
  {
    static ArrayList _externalPlayerList = new ArrayList();
    static bool _externalPlayersLoaded = false;

    public PlayerFactory()
    {
    }

    public enum StreamingPlayers : int
    {
      BASS = 0,
      WMP9 = 1,
      VMR7 = 2,
      RTSP = 3,
    }
    private bool CheckMpgFile(string fileName)
    {
      try
      {
        if (!System.IO.File.Exists(fileName)) return false;
        using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
          using (BinaryReader reader = new BinaryReader(stream))
          {
            stream.Seek(0, SeekOrigin.Begin);
            byte[] header = reader.ReadBytes(5);
            if (header[0] != 0 || header[1] != 0 || header[2] != 1 || header[3] != 0xba) return false;
            if ((header[4] & 0x40) == 0) return false;
            stream.Seek(0x800, SeekOrigin.Begin); header = reader.ReadBytes(5);
            if (header[0] != 0 || header[1] != 0 || header[2] != 1 || header[3] != 0xba) return false;
            if ((header[4] & 0x40) == 0) return false;
            stream.Seek(0x8000, SeekOrigin.Begin); header = reader.ReadBytes(5);
            if (header[0] != 0 || header[1] != 0 || header[2] != 1 || header[3] != 0xba) return false;
            if ((header[4] & 0x40) == 0) return false;
            return true;
          }
        }
      }
      catch (Exception e)
      {
        // If an IOException is raised, the file may be in use/being recorded so we assume that it is a correct mpeg file
        // This fixes replaying mpeg files while being recorded
        if (e.GetType().ToString() == "System.IO.IOException")
          return true;
        Log.Info("Exception in CheckMpgFile with message: {0}", e.Message);
      }
      return false;
    }

    private void LoadExternalPlayers()
    {
      Log.Info("Loading external players plugins");
      string[] fileList = System.IO.Directory.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "ExternalPlayers"), "*.dll");
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
                    Log.Info("  found plugin:{0} in {1}", t.ToString(), fileName);

                    IExternalPlayer player = (IExternalPlayer)newObj;
                    Log.Info("  player:{0}.  author: {1}", player.PlayerName, player.AuthorName);
                    _externalPlayerList.Add(player);
                  }
                }
              }
              catch (Exception e)
              {
                Log.Info("Error loading external player: {0}", t.ToString());
                Log.Info("Error: {0}", e.StackTrace);
              }
            }
          }
        }
        catch (Exception e)
        {
          Log.Info("Error loading external player: {0}", e);
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
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
      return Create(fileName, null);
    }

    public IPlayer Create(string fileName, g_Player.MediaType type)
    {
      try
      {
        g_Player.MediaType? paramType = type as g_Player.MediaType?;
        if (paramType.HasValue)
          return Create(fileName, paramType.Value);
      }
      catch (Exception ex)
      {
        Log.Error("PlayerFactory: Error creating player instance - {0}", ex.Message);
      }
      return (Create(fileName, null));
    }

    /// <summary>
    /// We do not want to change neither the enum nor the previous Create overloaded calls to maintain backward compatibility
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private IPlayer Create(string fileName, g_Player.MediaType? mtype)
    {
      try
      {
        // Get settings only once
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          string strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "player", "Internal dshow player");
          int streamPlayer = xmlreader.GetValueAsInt("audioscrobbler", "streamplayertype", 0);
          bool Vmr9Enabled = xmlreader.GetValueAsBool("musicvideo", "useVMR9", true);

          // Free BASS to avoid problems with Digital Audio, when watching movies
          if (BassMusicPlayer.IsDefaultMusicPlayer)
          {
            if (!Util.Utils.IsAudio(fileName))
              BassMusicPlayer.Player.FreeBass();
          }

          if (fileName.ToLower().IndexOf("rtsp:") >= 0)
          {
            if (mtype.HasValue)
              return new TSReaderPlayer(mtype.Value);
            else
              return new TSReaderPlayer();
          }

          if (fileName.StartsWith("mms:") && fileName.EndsWith(".ymvp"))
          {
            if (Vmr9Enabled)
              return new VideoPlayerVMR9();
            else
              return new AudioPlayerWMP9();
          }

          string extension = Path.GetExtension(fileName).ToLower();
          if (extension != ".tv" && extension != ".sbe" && extension != ".dvr-ms" && fileName.ToLower().IndexOf(".tsbuffer") < 0 && fileName.ToLower().IndexOf("radio.tsbuffer") < 0)
          {
            IPlayer newPlayer = GetExternalPlayer(fileName);
            if (newPlayer != null)
            {
              Log.Info("PlayerFactory: Disabling DX9 exclusive mode");
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
              GUIWindowManager.SendMessage(msg);
              return newPlayer;
            }
          }

          if (Util.Utils.IsVideo(fileName))
          {
            if (extension == ".tv" || extension == ".sbe" || extension == ".dvr-ms")
            {
              //if (extension == ".sbe" || extension == ".dvr-ms")
              //{
              //  //GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT, 0, 0, 0, 0, 0, null);
              //  //GUIWindowManager.SendMessage(msg);
              //}
              return new Player.StreamBufferPlayer9();
            }
          }

          // Use TsReader for timeshift buffer file for TvEngine3 & .ts recordings etc.
          if (extension == ".tsbuffer" || extension == ".ts")
          {
            if (fileName.ToLower().IndexOf("radio.tsbuffer") >= 0)
            {
              if (mtype.HasValue)
                return new BaseTSReaderPlayer(mtype.Value);
              else
                return new Player.BaseTSReaderPlayer();
            }
            if (mtype.HasValue)
              return new Player.TSReaderPlayer(mtype.Value);
            else
              return new Player.TSReaderPlayer();
          }

          if (!Util.Utils.IsAVStream(fileName) && Util.Utils.IsVideo(fileName))
          {
            if (mtype.HasValue)
            {
              return new Player.VideoPlayerVMR9(mtype.Value);
            }
            else
              return new Player.VideoPlayerVMR9();
          }

          if (extension == ".radio")
          {
            return new Player.RadioTuner();
          }

          if (Util.Utils.IsCDDA(fileName))
          {
            // Check if, we should use BASS for CD Playback
            if (String.Compare(strAudioPlayer, "BASS engine", true) == 0)
            {
              if (BassMusicPlayer.BassFreed)
                BassMusicPlayer.Player.InitBass();

              return BassMusicPlayer.Player;
            }
            else
              return new Player.AudioPlayerWMP9();
          }

          if (Util.Utils.IsAudio(fileName))
          {
            //// choose player for Internet radio streams 
            //if (Util.Utils.IsLastFMStream(fileName))
            //{
            //  switch (streamPlayer)
            //  {
            //    case 0:
            //      if (BassMusicPlayer.BassFreed)
            //        BassMusicPlayer.Player.InitBass();
            //      return BassMusicPlayer.Player;
            //    case 1:
            //      return new Player.AudioPlayerWMP9();
            //    case 2:
            //      return new Player.AudioPlayerVMR7();
            //    default:
            //      if (BassMusicPlayer.BassFreed)
            //        BassMusicPlayer.Player.InitBass();
            //      return BassMusicPlayer.Player;
            //  }
            //}

            if (String.Compare(strAudioPlayer, "BASS engine", true) == 0)
            {
              if (BassMusicPlayer.BassFreed)
                BassMusicPlayer.Player.InitBass();

              return BassMusicPlayer.Player;
            }
            else if (String.Compare(strAudioPlayer, "Windows Media Player 9", true) == 0)
            {
              return new Player.AudioPlayerWMP9();
            }
            else
              return new Player.AudioPlayerVMR7();
          }

          // Use WMP Player as Default
          return new Player.AudioPlayerWMP9();
        }
      }
      finally
      {
        Log.Debug("PlayerFactory: Successfully created player instance for file - {0}", fileName);
      }
    }

  }
}