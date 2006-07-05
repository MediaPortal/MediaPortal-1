using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MediaPortal.Util;

namespace MediaPortal.Playlists
{
  public class PlayListPLSIO : IPlayListIO
  {
    const string START_PLAYLIST_MARKER = "[playlist]";
    const string PLAYLIST_NAME = "PlaylistName";

    public bool Load(PlayList playlist, string fileName)
    {
      string basePath;
      string extension = Path.GetExtension(fileName);
      extension.ToLower();

      playlist.Clear();
      playlist.Name = Path.GetFileName(fileName);
      basePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
      Encoding fileEncoding = Encoding.Default;
      FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
      StreamReader file = new StreamReader(stream, fileEncoding, true);
      if (file == null)
      {
        return false;
      }

      string line;
      line = file.ReadLine();
      if (line == null)
      {
        file.Close();
        return false;
      }

      string strLine = line.Trim();
      //CUtil::RemoveCRLF(strLine);
      if (strLine != START_PLAYLIST_MARKER)
      {
        if (strLine.StartsWith("http") || strLine.StartsWith("HTTP") ||
              strLine.StartsWith("mms") || strLine.StartsWith("MMS") ||
              strLine.StartsWith("rtp") || strLine.StartsWith("RTP"))
        {
          PlayListItem newItem = new PlayListItem(strLine, strLine, 0);
          newItem.Type = PlayListItem.PlayListItemType.AudioStream;
          playlist.Add(newItem);
          file.Close();
          return true;
        }
        fileEncoding = Encoding.Default;
        stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        file = new StreamReader(stream, fileEncoding, true);

        //file.Close();
        //return false;
      }
      string infoLine = "";
      string durationLine = "";
      fileName = "";
      line = file.ReadLine();
      while (line != null)
      {
        strLine = line.Trim();
        //CUtil::RemoveCRLF(strLine);
        int equalPos = strLine.IndexOf("=");
        if (equalPos > 0)
        {
          string leftPart = strLine.Substring(0, equalPos);
          equalPos++;
          string valuePart = strLine.Substring(equalPos);
          leftPart = leftPart.ToLower();
          if (leftPart.StartsWith("file"))
          {
            if (valuePart.Length > 0 && valuePart[0] == '#')
            {
              line = file.ReadLine();
              continue;
            }

            if (fileName.Length != 0)
            {
              PlayListItem newItem = new PlayListItem(infoLine, fileName, 0);
              playlist.Add(newItem);
              fileName = "";
              infoLine = "";
              durationLine = "";
            }
            fileName = valuePart;
          }
          if (leftPart.StartsWith("title"))
          {
            infoLine = valuePart;
          }
          else
          {
            if (infoLine == "") infoLine = System.IO.Path.GetFileName(fileName);
          }
          if (leftPart.StartsWith("length"))
          {
            durationLine = valuePart;
          }
          if (leftPart == "playlistname")
          {
            playlist.Name = valuePart;
          }

          if (durationLine.Length > 0 && infoLine.Length > 0 && fileName.Length > 0)
          {
            int duration = System.Int32.Parse(durationLine);
            duration *= 1000;

            string tmp = fileName.ToLower();
            PlayListItem newItem = new PlayListItem(infoLine, fileName, duration);
            if (tmp.IndexOf("http:") < 0 && tmp.IndexOf("mms:") < 0 && tmp.IndexOf("rtp:") < 0)
            {
              MediaPortal.Util.Utils.GetQualifiedFilename(basePath, ref fileName);
              newItem.Type = PlayListItem.PlayListItemType.AudioStream;
            }
            playlist.Add(newItem);
            fileName = "";
            infoLine = "";
            durationLine = "";
          }
        }
        line = file.ReadLine();
      }
      file.Close();

      if (fileName.Length > 0)
      {
        PlayListItem newItem = new PlayListItem(infoLine, fileName, 0);
      }


      return true;
    }

    public void Save(PlayList playlist, string fileName)
    {
      using (StreamWriter writer = new StreamWriter(fileName, false))
      {
        writer.WriteLine(START_PLAYLIST_MARKER);
        for (int i = 0; i < playlist.Count; i++)
        {
          PlayListItem item = playlist[i];
          writer.WriteLine("File{0}={1}", i + 1, item.FileName);
          writer.WriteLine("Title{0}={1}", i + 1, item.Description);
          writer.WriteLine("Length{0}={1}", i + 1, item.Duration);
        }
        writer.WriteLine("NumberOfEntries={0}", playlist.Count);
        writer.WriteLine("Version=2");
      }
    }
  }
}
