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
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;

namespace MpeCore.Classes.ZipProvider
{
  public class ZipProviderClass : IDisposable
  {
    private List<string> _tempFileList = new List<string>();
    private ZipFile _zipPackageFile;

    /// <summary>
    /// Loads the specified zipfile.
    /// 
    /// </summary>
    /// <param name="zipfile">The zipfile.</param>
    /// <returns>if something wrong, return null else the loaded package</returns>
    public PackageClass Load(string zipfile)
    {
      try
      {
        PackageClass pak = new PackageClass();
        _zipPackageFile = ZipFile.Read(zipfile);
        string tempPackageFile = Path.GetTempFileName();
        var fs = new FileStream(tempPackageFile, FileMode.Create);
        _zipPackageFile["MediaPortalExtension.xml"].Extract(fs);
        fs.Close();
        pak.Load(tempPackageFile);
        _tempFileList.Add(tempPackageFile);
        foreach (FileItem fileItem in pak.UniqueFileList.Items)
        {
          if (fileItem.SystemFile)
          {
            string tempfil = Path.GetTempFileName();
            tempfil = Path.GetDirectoryName(tempfil) + Path.GetFileNameWithoutExtension(tempfil) +
                      Path.GetExtension(fileItem.LocalFileName);
            Extract(fileItem, tempfil);
            fileItem.TempFileLocation = tempfil;
            //fileItem.LocalFileName = tempfil;
            _tempFileList.Add(tempfil);
          }
        }
        pak.ZipProvider = this;
        pak.GetFilePaths();
        pak.GeneralInfo.Location = zipfile;
        return pak;
      }
      catch (Exception)
      {
        if (_zipPackageFile != null)
          _zipPackageFile.Dispose();
        return null;
      }
    }

    public DateTime FileDate(FileItem item)
    {
      return _zipPackageFile[item.ZipFileName].LastModified;
    }

    public bool Extract(FileItem item, string extractLocation)
    {
      try
      {
        //if (File.Exists(item.TempFileLocation))
        //    File.Copy(item.TempFileLocation, extractLocation, true);
        var fs = new FileStream(extractLocation, FileMode.Create);
        _zipPackageFile[item.ZipFileName].Extract(fs);
        fs.Close();
        File.SetCreationTime(extractLocation, FileDate(item));
        File.SetLastAccessTime(extractLocation, FileDate(item));
        File.SetLastWriteTime(extractLocation, FileDate(item));
        item.TempFileLocation = extractLocation;
      }
      catch (Exception)
      {
        return false;
      }
      return true;
    }

    public bool Save(PackageClass pak, string filename)
    {
      pak.GeneralInfo.OnlineLocation = pak.ReplaceInfo(pak.GeneralInfo.OnlineLocation);

      string temfile = Path.GetTempFileName();
      pak.Save(temfile);
      if (!Directory.Exists(Path.GetDirectoryName(filename)))
        Directory.CreateDirectory(Path.GetDirectoryName(filename));
      using (ZipFile zip = new ZipFile())
      {
        zip.AddFile(temfile).FileName = "MediaPortalExtension.xml";

        foreach (FileItem fileItem in pak.UniqueFileList.Items)
        {
          zip.AddFile(fileItem.LocalFileName).FileName = fileItem.ZipFileName;
        }

        zip.Save(filename);
      }
      File.Delete(temfile);
      return true;
    }

    private void Clear()
    {
      if (_zipPackageFile != null)
        _zipPackageFile.Dispose();
      foreach (string s in _tempFileList)
      {
        try
        {
          File.Delete(s);
        }
        catch (Exception) {}
      }
    }

    #region IDisposable Members

    public void Dispose()
    {
      Clear();
    }

    #endregion
  }
}