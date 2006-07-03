using System;
using System.Collections.Generic;
using System.Text;
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
