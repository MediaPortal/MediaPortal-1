#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

namespace MediaPortal.Support
{
  public class Archiver : IDisposable
  {
    private ZipOutputStream zipStream;

    public Archiver(string file)
    {
      zipStream = CreateZipStream(file);
    }

    public void AddFile(string file)
    {
      Crc32 crc = new Crc32();
      FileStream fs = File.OpenRead(file);
      byte[] buf = new byte[fs.Length];
      fs.Read(buf, 0, buf.Length);
      ZipEntry ze = new ZipEntry(file.Substring(file.LastIndexOf("\\") + 1));
      ze.Size = fs.Length;
      fs.Close();
      crc.Reset();
      crc.Update(buf);
      ze.Crc = crc.Value;
      zipStream.PutNextEntry(ze);
      zipStream.Write(buf, 0, buf.Length);
    }

    public void AddDirectory(string directory)
    {
      string[] filesInDir = Directory.GetFiles(directory);
      foreach (string file in filesInDir)
      {
        this.AddFile(file);
      }
    }

    private static ZipOutputStream CreateZipStream(string zipFile)
    {
      ZipOutputStream zos = new ZipOutputStream(File.Create(zipFile));
      zos.SetLevel(9);
      return zos;
    }

    public void Dispose()
    {
      zipStream.Dispose();
    }
  }
}