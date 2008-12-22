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
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace ThumbDBLib
{
  public class ThumbNail
  {
    string _name;
    int _width;
    int _height;
    long _offset;
    string _folder;

    public ThumbNail(string name, Size size, long offset, string folder)
    {
      _name = name;
      _width = size.Width;
      _height = size.Height;
      _offset = offset;
      _folder = folder;

    }

    public string Name
    {
      get
      {
        return _name;
      }
    }

    public int Width
    {
      get
      {
        return _width;
      }
    }

    public int Height
    {
      get
      {
        return _height;
      }
    }

    public Image Image
    {
      get
      {
        using (FileStream stream = new FileStream(_folder + @"\mpthumbs.db", FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          stream.Seek(_offset, SeekOrigin.Begin);
          BinaryReader reader = new BinaryReader(stream);
          string file = reader.ReadString();
          int width = reader.ReadInt32();
          int height = reader.ReadInt32();
          int length = reader.ReadInt32();
          byte[] image = new byte[length];
          stream.Read(image, 0, image.Length);
          MemoryStream memoryStream = new MemoryStream(image);
          return Image.FromStream(memoryStream);
        }
      }
    }
  }
}
