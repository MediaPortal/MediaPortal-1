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
