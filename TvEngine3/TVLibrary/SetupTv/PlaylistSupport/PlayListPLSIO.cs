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

using System.Text;
using System.IO;
using SetupTv;

namespace MediaPortal.Playlists
{
  public class PlayListPLSIO : IPlayListIO
  {
    const string START_PLAYLIST_MARKER = "[playlist]";

    public bool Load(PlayList playlist, string fileName)
    {
      string extension = Path.GetExtension(fileName);
      extension.ToLower();

      playlist.Clear();
      playlist.Name = Path.GetFileName(fileName);
      string basePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
      Encoding fileEncoding = Encoding.Default;
      FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
      StreamReader file = new StreamReader(stream, fileEncoding, true);

      string line = file.ReadLine();
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
        fileEncoding = Encoding.Default; // No unicode??? rtv
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
              infoLine = Path.GetFileName(fileName);
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
              Utils.GetQualifiedFilename(basePath, ref fileName);
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
        new PlayListItem(infoLine, fileName, 0);
      }


      return true;
    }

    public void Save(PlayList playListParam, string fileName)
    {
      using (StreamWriter writer = new StreamWriter(fileName, false))
      {
        writer.WriteLine(START_PLAYLIST_MARKER);
        for (int i = 0; i < playListParam.Count; i++)
        {
          PlayListItem item = playListParam[i];
          writer.WriteLine("File{0}={1}", i + 1, item.FileName);
          writer.WriteLine("Title{0}={1}", i + 1, item.Description);
          writer.WriteLine("Length{0}={1}", i + 1, item.Duration);
        }
        writer.WriteLine("NumberOfEntries={0}", playListParam.Count);
        writer.WriteLine("Version=2");
      }
    }
  }
}
