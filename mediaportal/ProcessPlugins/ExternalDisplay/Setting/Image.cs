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
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using MediaPortal.Services;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <author>JoeDalton</author>
  [Serializable]
  public class Image
  {
    private Bitmap bitmap;
    private string fileName;
    private Value value;

    [XmlAttribute] public int X;

    [XmlAttribute] public int Y;

    [XmlElement("Text", typeof (Text))]
    [XmlElement("Property", typeof (Property))]
    [XmlElement("Parse", typeof (Parse))]
    [DefaultValue(null)]
    public Value Value
    {
      get { return value; }
      set { this.value = value; }
    }


    [XmlAttribute]
    [DefaultValue(null)]
    public string File
    {
      get { return fileName; }
      set { fileName = value; }
    }

    public Image()
    {
    }

    public Image(int x, int y, string file)
    {
      X = x;
      Y = y;
      File = file;
    }

    [XmlIgnore]
    public Bitmap Bitmap
    {
      get
      {
        try
        {
          if (value != null)
          {
            string file = value.Evaluate();
            if (file==fileName)
              return bitmap;
            fileName = file;
            if (System.IO.File.Exists(file))
            {
              bitmap = (Bitmap) Bitmap.FromFile(file);
            }
            return bitmap;
          }
          if (bitmap == null)
          {
            if (System.IO.File.Exists(fileName))
            {
              bitmap = (Bitmap) Bitmap.FromFile(fileName);
            }
            else
            {
              GlobalServiceProvider.Get<ILog>().Error(
                "Error while loading image file {0}.  File not found!  Defaulting to a single pixel.", File);
              bitmap = new Bitmap(1, 1);
            }
          }
        }
        catch (OutOfMemoryException)
        {
          GlobalServiceProvider.Get<ILog>().Error(
            "Out of memory while loading image file {0}!  Probably bad image format.  Defaulting to a single pixel.",
            File);
          //We provide a default image, to avoid reloading it and getting the same error again
          bitmap = new Bitmap(1, 1);
        }
        return bitmap;
      }
    }
  }
}