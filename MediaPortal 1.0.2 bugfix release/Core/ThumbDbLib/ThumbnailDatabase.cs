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
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace ThumbDBLib
{

  public class ThumbnailDatabase
  {
    string _folder;
    List<ThumbNail> _cache;

    public ThumbnailDatabase(string folder)
    {
      _folder = folder;
      _cache = LoadDatabase();
    }
    public string Folder
    {
      get { return _folder; }
    }
    public bool Exists(string fileName)
    {
      lock (_cache)
      {
        string file = System.IO.Path.GetFileName(fileName);
        for (int i = 0; i < _cache.Count; ++i)
        {
          if (String.Compare(_cache[i].Name, file, true) == 0) return true;
        }
      }
      return false;
    }

    public void Add(string fileName)
    {
      if (Exists(fileName)) return;

      try
      {
        using (ThumbnailCreatorNet creator = new ThumbnailCreatorNet())
        //using (ThumbnailCreator creator = new ThumbnailCreator())
        {
          //creator.DesiredSize = new Size(128, 128);
          creator.SetParams(128, 128, false, true);
          using (Image image = creator.GetThumbNail(fileName))
          {
            AddThumbnail(fileName, image);
          }
        }
      }
      catch (Exception)
      {
      }
    }

    public ThumbNail Get(string fileName)
    {
      lock (_cache)
      {
        string file = System.IO.Path.GetFileName(fileName);
        for (int i = 0; i < _cache.Count; ++i)
        {
          if (String.Compare(_cache[i].Name, file, true) == 0) return _cache[i];
        }
      }
      return null;
    }

    void AddThumbnail(string fileName, Image image)
    {
      lock (_cache)
      {
        string file = System.IO.Path.GetFileName(fileName);
        if (String.Compare(file, "mpthumbs.db", true) == 0) return;
        using (FileStream stream = new FileStream(_folder + @"\mpthumbs.db", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
        {
          stream.Seek(0, SeekOrigin.End);
          long offset = stream.Position;
          BinaryWriter writer = new BinaryWriter(stream);
          writer.Write(file);
          writer.Write((int)image.Width);
          writer.Write((int)image.Height);
          MemoryStream imageStream = new MemoryStream();
          image.Save(imageStream, System.Drawing.Imaging.ImageFormat.Jpeg);
          writer.Write((int)imageStream.Length);
          writer.Write(imageStream.GetBuffer(), 0, (int)imageStream.Length);
          writer.Flush();
          stream.Flush();
          stream.Close();

          ThumbNail thumb = new ThumbNail(file, new Size(image.Width, image.Height), offset, _folder);
          _cache.Add(thumb);
        }
        System.IO.File.SetAttributes(_folder + @"\mpthumbs.db", FileAttributes.Hidden);
      }
    }

    public List<ThumbNail> Thumbnails
    {
      get
      {
        return _cache;
      }
    }

    List<ThumbNail> LoadDatabase()
    {
      List<ThumbNail> thumbs = new List<ThumbNail>();
      if (!System.IO.File.Exists(_folder + @"\mpthumbs.db")) return thumbs;

      using (FileStream stream = new FileStream(_folder + @"\mpthumbs.db", FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        BinaryReader reader = new BinaryReader(stream);
        while (true)
        {
          long offset = stream.Position;
          if (stream.Position >= stream.Length) break;
          string file = reader.ReadString();
          if (file == null) break;
          if (file.Length == 0) break;
          int width = reader.ReadInt32();
          int height = reader.ReadInt32();
          int length = reader.ReadInt32();
          stream.Seek(length, SeekOrigin.Current);

          ThumbNail thumb = new ThumbNail(file, new Size(width, height), offset, _folder);
          thumbs.Add(thumb);
        }
        stream.Close();
      }
      return thumbs;
    }
  }
}
