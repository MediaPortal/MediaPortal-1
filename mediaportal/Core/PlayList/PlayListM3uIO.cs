using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

namespace MediaPortal.Playlists
{
  public class PlayListM3uIO : IPlayListIO
  {
    const string M3U_START_MARKER = "#EXTM3U";
    const string M3U_INFO_MARKER = "#EXTINF";
    private PlayList playlist;
    private StreamReader file;
    private string basePath;

    public PlayListM3uIO()
    {
    }

    public bool Load(PlayList incomingPlaylist, string playlistFileName)
    {
      if (playlistFileName == null)
        return false;
      playlist = incomingPlaylist;
      playlist.Clear();

      try
      {
        playlist.Name = Path.GetFileName(playlistFileName);
        basePath = Path.GetDirectoryName(Path.GetFullPath(playlistFileName));

        using (file = new StreamReader(playlistFileName))
        {
          if (file == null)
            return false;

          string line = file.ReadLine();
          if (line == null || line.Length == 0)
            return false;

          string trimmedLine = line.Trim();

          if (trimmedLine != M3U_START_MARKER)
          {
            string fileName = trimmedLine;
            if (!AddItem("", 0, fileName))
              return false;
          }

          line = file.ReadLine();
          while (line != null)
          {
            trimmedLine = line.Trim();

            if (trimmedLine.StartsWith(M3U_INFO_MARKER))
            {
              string songName = null;
              int lDuration = 0;

              if (ExtractM3uInfo(trimmedLine, ref songName, ref lDuration))
              {
                line = file.ReadLine();
                if (!AddItem(songName, lDuration, line))
                  break;
              }
            }
            else
            {
              if (!AddItem("", 0, trimmedLine))
                break;
            }
            line = file.ReadLine();
          }
        }
      }
      catch (Exception ex)
      {
        Log.Info("exception loading playlist {0} err:{1} stack:{2}", playlistFileName, ex.Message, ex.StackTrace);
        return false;
      }
      return true;
    }

    private static bool ExtractM3uInfo(string trimmedLine, ref string songName, ref int lDuration)
    {
      //bool successfull;
      int iColon = (int)trimmedLine.IndexOf(":");
      int iComma = (int)trimmedLine.IndexOf(",");
      if (iColon >= 0 && iComma >= 0 && iComma > iColon)
      {
        iColon++;
        string duration = trimmedLine.Substring(iColon, iComma - iColon);
        iComma++;
        songName = trimmedLine.Substring(iComma);
        lDuration = System.Int32.Parse(duration);
        return true;
      }
      return false;
    }


    private bool AddItem(string songName, int duration, string fileName)
    {
      if (fileName == null || fileName.Length == 0)
        return false;

      MediaPortal.Util.Utils.GetQualifiedFilename(basePath, ref fileName);
      PlayListItem newItem = new PlayListItem(songName, fileName, duration);
      newItem.Type = PlayListItem.PlayListItemType.Audio;
      if (songName.Length == 0)
      {
        newItem.Description = Path.GetFileName(fileName);
      }
      playlist.Add(newItem);
      return true;
    }

    public void Save(PlayList playlist, string fileName)
    {
      try
      {
        using (StreamWriter writer = new StreamWriter(fileName, false))
        {
          writer.WriteLine(M3U_START_MARKER);

          foreach (PlayListItem item in playlist)
          {
            writer.WriteLine("{0}:{1},{2}", M3U_INFO_MARKER, item.Duration, item.Description);
            writer.WriteLine("{0}", item.FileName);
          }
        }
      }
      catch (Exception e)
      {
        Log.Info("failed to save a playlist {0}. err: {1} stack: {2}", fileName, e.Message, e.StackTrace);
      }
    }
  }
}
