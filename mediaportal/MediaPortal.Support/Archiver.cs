#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using Ionic.Zip;
using Ionic.Zlib;

namespace MediaPortal.Support
{
  public class Archiver : IDisposable
  {
    public ZipFile zip = new ZipFile();

    public void AddFile(string file, string zipfile)
    {
      AddFile(file, zipfile, true);
    }

    public void AddFile(string file, string zipfile, bool keepDirectoryStructure)
    {
      zip.CompressionLevel = CompressionLevel.BEST_COMPRESSION;
      if (keepDirectoryStructure)
        zip.AddFile(file);
      else
        zip.AddFile(file, "");
      zip.Save(zipfile);
    }

    public void AddDirectory(string directory, string zipfile)
    {
      AddDirectory(directory, zipfile, true);
    }

    public void AddDirectory(string directory, string zipfile, bool keepDirectoryStructure)
    {
      zip.CompressionLevel = CompressionLevel.BEST_COMPRESSION;
      if (keepDirectoryStructure)
        zip.AddDirectory(directory);
      else
        zip.AddDirectory(directory, "");
      zip.Save(zipfile);
    }

    public void Dispose()
    {
      zip.Dispose();
    }
  }
}