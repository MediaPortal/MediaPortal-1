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

    public bool Load(PlayList incomingPlaylist, string playlistFileName)
    {
      if (playlistFileName == null) return false;
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
            string fileName = line;
            //CUtil::RemoveCRLF(fileName);
            if (fileName.Length > 1)
            {
              Utils.GetQualifiedFilename(basePath, ref fileName);
              PlayListItem newItem = new PlayListItem(fileName, fileName, 0);
              newItem.Type = PlayListItem.PlayListItemType.Audio;
              string strDescription;
              strDescription = Path.GetFileName(fileName);
              newItem.Description = strDescription;
              playlist.Add(newItem);
            }
          }

          line = file.ReadLine();
          while (line != null)
          {
            trimmedLine = line.Trim();

            if (trimmedLine.StartsWith(M3U_INFO_MARKER))
            {
              // start of info 
              int iColon = (int)trimmedLine.IndexOf(":");
              int iComma = (int)trimmedLine.IndexOf(",");
              if (iColon >= 0 && iComma >= 0 && iComma > iColon)
              {
                iColon++;
                string duration = trimmedLine.Substring(iColon, iComma - iColon);
                iComma++;
                string songName = trimmedLine.Substring(iComma);
                int lDuration = System.Int32.Parse(duration);

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
      catch (Exception)
      {
        return false;
      }
      return true;
    }


    private bool AddItem(string songName, int duration, string fileName)
    {
      if (fileName == null || fileName.Length == 0)
        return false;

      Utils.GetQualifiedFilename(basePath, ref fileName);
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
        Log.Write("failed to save a playlist {0}. err: {1} stack: {2}", fileName, e.Message, e.StackTrace);
      }
    }
  }
}
