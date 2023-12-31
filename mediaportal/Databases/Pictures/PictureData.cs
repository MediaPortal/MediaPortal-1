#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
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

using static MediaPortal.GUI.Pictures.ExifMetadata;

namespace MediaPortal.Picture.Database
{
  [Serializable()]
  public class PictureData
  {

    public PictureData()
    {
      FileName = string.Empty;
      DateTaken = DateTime.MinValue; 
      Exif = new Metadata();
    }

    public PictureData Clone()
    {
      return new PictureData { FileName = FileName, DateTaken = DateTaken, Exif = Exif };
    }

    public void Clear()
    {
      FileName = string.Empty;
      DateTaken = DateTime.MinValue; 
      Exif = new Metadata();
    }

    #region Getters & Setters

    public string FileName { get; set; }

    public DateTime DateTaken { get; set; }

    public Metadata Exif { get; set; }

    #endregion

    #region Methods

    public string ToShortString()
    {
      return Path.GetFileNameWithoutExtension(FileName);
    }

    public override string ToString()
    {
      return Path.GetFileNameWithoutExtension(FileName) + "\t" + DateTaken.ToString("s");
    }

    #endregion
  }
}