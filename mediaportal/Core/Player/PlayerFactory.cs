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
using System.Reflection;
using System.Runtime.CompilerServices;
using MediaPortal.GUI.Library;
using MediaPortal.MusicPlayer.BASS;
using MediaPortal.Profile;
using MediaPortal.Common.Utils;
using Config = MediaPortal.Configuration.Config;

namespace MediaPortal.Player
{
  /// <summary>
  /// 
  /// </summary>
  public class PlayerFactory : IPlayerFactory
  {
    private static ArrayList _externalPlayerList = new ArrayList();
    private static bool _externalPlayersLoaded = false;

    public PlayerFactory() {}

    public enum StreamingPlayers : int
    {
      BASS = 0,
      WMP9 = 1,
      VMR7 = 2,
      RTSP = 3,
    }

    public static ArrayList ExternalPlayerList
    {
      get { return _externalPlayerList; }
    }

    private void LoadExternalPlayers()
    {
      Log.Info("Loading external players plugins");
      string[] fileList = Util.Utils.GetFiles(Config.GetSubFolder(Config.Dir.Plugins, "ExternalPlayers"), "dll");
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
                  if (typeof (IExternalPlayer).IsAssignableFrom(t))
                  {
                    if (!CompatibilityManager.IsPluginCompatible(t))
                    {
                      Log.Error(
                        "  external player: {0} is tagged as incompatible with the current MediaPortal version and won't be loaded!",
                        t.FullName);
                      continue;
                    }

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

    public IPlayer GetExternalPlayer(string fileName)
    {
      if (!_externalPlayersLoaded)
      {
        LoadExternalPlayers();
      }

      foreach (IExternalPlayer player in _externalPlayerList)
      {
        using (Settings xmlreader = new MPSettings())
        {
          bool enabled = xmlreader.GetValueAsBool("plugins", player.PlayerName, false);
          player.Enabled = enabled;
        }

        if (player.Enabled && player.SupportsFile(fileName))
        {
          return player as IPlayer;
        }
      }
      return null;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IPlayer Create(string fileName)
    {
      IPlayer newPlayer = Create(fileName, null);
      return newPlayer;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IPlayer Create(string fileName, g_Player.MediaType type)
    {
      IPlayer newPlayer = null;
      try
      {
        g_Player.MediaType? paramType = type as g_Player.MediaType?;
        if (paramType.HasValue)
        {
          newPlayer = Create(fileName, paramType);
        }
        else
        {
          newPlayer = Create(fileName, null);
        }
      }
      catch (Exception ex)
      {
        Log.Error("PlayerFactory: Error creating player instance - {0}", ex.Message);
        newPlayer = Create(fileName, null);
      }
      return newPlayer;
    }

    /// <summary>
    /// We do not want to change neither the enum nor the previous Create overloaded calls to maintain backward compatibility
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private IPlayer Create(string aFileName, g_Player.MediaType? aMediaType)
    {
      try
      {
        // Set to anything here as it will only be passed if aMediaType is not null
        g_Player.MediaType localType = g_Player.MediaType.Video;
        if (aMediaType != null && aMediaType != g_Player.MediaType.Unknown)
        {
          localType = (g_Player.MediaType)aMediaType;
        }

        // Get settings only once
        using (Settings xmlreader = new MPSettings())
        {
          string strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "playerId", "0"); // BASS Player
          bool Vmr9Enabled = xmlreader.GetValueAsBool("musicvideo", "useVMR9", true);
          bool InternalBDPlayer = xmlreader.GetValueAsBool("bdplayer", "useInternalBDPlayer", true);
          bool Usemoviecodects = xmlreader.GetValueAsBool("movieplayer", "usemoviecodects", false);

          if (aFileName.ToLowerInvariant().IndexOf("rtsp:") >= 0)
          {
            if (aMediaType != null)
            {
              return new TSReaderPlayer(localType);
            }
            else
            {
              return new TSReaderPlayer();
            }
          }

          if (aFileName.StartsWith("mms:") && aFileName.EndsWith(".ymvp"))
          {
            if (Vmr9Enabled)
            {
              return new VideoPlayerVMR9();
            }
            else
            {
              return new AudioPlayerWMP9();
            }
          }

          string extension = Path.GetExtension(aFileName).ToLowerInvariant();
          if (extension == ".bdmv")
          {
            if (InternalBDPlayer)
            {
            return new BDPlayer();
          }
            else
            {
              return new VideoPlayerVMR9();
            }
          }

          if (extension != ".tv" && extension != ".sbe" && extension != ".dvr-ms" &&
              aFileName.ToLowerInvariant().IndexOf(".tsbuffer") < 0 && aFileName.ToLowerInvariant().IndexOf("radio.tsbuffer") < 0)
          {
            IPlayer newPlayer = GetExternalPlayer(aFileName);
            if (newPlayer != null)
            {
              Log.Info("PlayerFactory: Disabling DX9 exclusive mode");
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
              GUIWindowManager.SendMessage(msg);
              return newPlayer;
            }
          }

          if (Util.Utils.IsVideo(aFileName))
          {
            if (extension == ".tv" || extension == ".sbe" || extension == ".dvr-ms")
            {
              //if (extension == ".sbe" || extension == ".dvr-ms")
              //{
              //  //GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT, 0, 0, 0, 0, 0, null);
              //  //GUIWindowManager.SendMessage(msg);
              //}
              return new StreamBufferPlayer9();
            }
            if (extension == ".ifo" || Util.VirtualDirectory.IsImageFile(extension))
            {
              return new DVDPlayer9();
            }
          }

          // Use TsReader for timeshift buffer file for TvEngine3 & .ts recordings etc.
          if (extension == ".tsbuffer" || extension == ".ts" || extension == ".rec")
            //new support for Topfield recordings
          {
            if (aFileName.ToLowerInvariant().IndexOf("radio.tsbuffer") >= 0)
            {
              if (aMediaType != null)
              {
                return new BaseTSReaderPlayer(localType);
              }
              else
              {
                return new BaseTSReaderPlayer();
              }
            }
            if (aMediaType != null)
            {
              if (
                (GUIWindow.Window)
                (Enum.Parse(typeof(GUIWindow.Window), GUIWindowManager.ActiveWindow.ToString())) ==
                GUIWindow.Window.WINDOW_VIDEOS && Usemoviecodects || (g_Player.IsExtTS && Usemoviecodects))
              {
                return new VideoPlayerVMR9(localType);
              }
              else
              {
                return new TSReaderPlayer(localType);
              }
            }
            else
            {
              return new TSReaderPlayer();
            }
          }

          if (!Util.Utils.IsAVStream(aFileName) && Util.Utils.IsVideo(aFileName) && localType != g_Player.MediaType.Music)
          {
            if (aMediaType != null)
            {
              return new VideoPlayerVMR9(localType);
            }
            else
            {
              return new VideoPlayerVMR9();
            }
          }

          if (Util.Utils.IsCDDA(aFileName))
          {
            // Check if, we should use BASS for CD Playback
            if ((AudioPlayer)Enum.Parse(typeof(AudioPlayer), strAudioPlayer) != AudioPlayer.DShow)
            {
              if (BassMusicPlayer.BassFreed)
              {
                BassMusicPlayer.Player.InitBass();
              }

              return BassMusicPlayer.Player;
            }
            else
            {
              return new AudioPlayerWMP9();
            }
          }

          if (Util.Utils.IsAudio(aFileName) || localType == g_Player.MediaType.Music)
          {
            if ((AudioPlayer)Enum.Parse(typeof(AudioPlayer), strAudioPlayer) != AudioPlayer.DShow)
            {
              if (BassMusicPlayer.BassFreed)
              {
                BassMusicPlayer.Player.InitBass();
              }

              return BassMusicPlayer.Player;
            }
            else if (String.Compare(strAudioPlayer, "Windows Media Player 9", true) == 0)
            {
              return new AudioPlayerWMP9();
            }
            else
            {
              return new AudioPlayerVMR7();
            }
          }

          // Use WMP Player as Default
          return new AudioPlayerWMP9();
        }
      }
      finally
      {
        Log.Debug("PlayerFactory: Successfully created player instance for file - {0}", aFileName);
      }
    }
  }
}