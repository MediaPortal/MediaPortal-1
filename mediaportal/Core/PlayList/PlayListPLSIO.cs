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
using System.IO;
using System.Net;
using System.Text;

namespace MediaPortal.Playlists
{
  public class PlayListPLSIO : IPlayListIO
  {
    private const string START_PLAYLIST_MARKER = "[playlist]";

    public bool Load(PlayList playlist, string fileName)
    {
      return Load(playlist, fileName, null);
    }

    public bool Load(PlayList playlist, string fileName, string label)
    {
      string basePath = String.Empty;
      Stream stream;

      if (fileName.ToLowerInvariant().StartsWith("http"))
      {
        // We've got a URL pointing to a pls
        WebClient client = new WebClient();
        client.Proxy.Credentials = CredentialCache.DefaultCredentials;
        byte[] buffer = client.DownloadData(fileName);
        stream = new MemoryStream(buffer);
      }
      else
      {
        // We've got a plain pls file
        basePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
        stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
      }

      playlist.Clear();
      playlist.Name = Path.GetFileName(fileName);
      Encoding fileEncoding = Encoding.Default;
      StreamReader file = new StreamReader(stream, fileEncoding, true);
      try
      {
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
            leftPart = leftPart.ToLowerInvariant();
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
              if (infoLine == "")
              {
                // For a URL we need to set the label in for the Playlist name, in order to be played.
                if (label != null && fileName.ToLowerInvariant().StartsWith("http"))
                {
                  infoLine = label;
                }
                else
                {
                  infoLine = Path.GetFileName(fileName);
                }
              }
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
              int duration = Int32.Parse(durationLine);

              // Remove trailing slashes. Might cause playback issues
              if (fileName.EndsWith("/"))
              {
                fileName = fileName.Substring(0, fileName.Length - 1);
              }

              PlayListItem newItem = new PlayListItem(infoLine, fileName, duration);
              if (fileName.ToLowerInvariant().StartsWith("http:") || fileName.ToLowerInvariant().StartsWith("https:") ||
                  fileName.ToLowerInvariant().StartsWith("mms:") || fileName.ToLowerInvariant().StartsWith("rtp:"))
              {
                newItem.Type = PlayListItem.PlayListItemType.AudioStream;
              }
              else
              {
                Util.Utils.GetQualifiedFilename(basePath, ref fileName);
                newItem.FileName = fileName;
                newItem.Type = PlayListItem.PlayListItemType.Audio;
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
      }
      finally
      {
        if (file != null)
        {
          file.Close();
        }
      }

      return true;
    }

    public void Save(PlayList playlist, string fileName)
    {
      using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
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