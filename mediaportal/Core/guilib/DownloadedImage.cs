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
using System.Net;
using MediaPortal.Configuration;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Summary description for DownloadedImage.
  /// </summary>
  internal class DownloadedImage
  {
    private string _fileName;
    private string _url;
    private DateTime _dateDownloaded = DateTime.MinValue;
    private int _cacheMinutes = 60*30; //30minutes

    public DownloadedImage(string url)
    {
      URL = url;
      int pos = url.LastIndexOf("/");

      _fileName = GetTempFileName();
    }

    private string GetTempFileName()
    {
      int x = 0;
      while (true)
      {
        string tempFile = Config.GetFile(Config.Dir.Thumbs, String.Format("MPTemp{0}.gif", x));
        string tempFile2 = Config.GetFile(Config.Dir.Thumbs, String.Format("MPTemp{0}.jpg", x));
        string tempFile3 = Config.GetFile(Config.Dir.Thumbs, String.Format("MPTemp{0}.bmp", x));
        if (!File.Exists(tempFile) &&
            !File.Exists(tempFile2) &&
            !File.Exists(tempFile3))
        {
          return tempFile;
        }
        ++x;
      }
    }


    public string FileName
    {
      get { return _fileName; }
      set { _fileName = value; }
    }

    public string URL
    {
      get { return _url; }
      set { _url = value; }
    }

    public int CacheTime
    {
      get { return _cacheMinutes; }
      set { _cacheMinutes = value; }
    }

    public bool ShouldDownLoad
    {
      get
      {
        TimeSpan ts = DateTime.Now - _dateDownloaded;
        if (ts.TotalSeconds > CacheTime)
        {
          return true;
        }
        return false;
      }
    }

    public bool Download()
    {
      using (WebClient client = new WebClient())
      {
        try
        {
          try
          {
            File.Delete(FileName);
          }
          catch (Exception)
          {
            Log.Info("DownloadedImage:Download() Delete failed:{0}", FileName);
          }
          
          client.Proxy.Credentials = CredentialCache.DefaultCredentials;
          client.DownloadFile(URL, FileName);
          try
          {
            string extension = "";
            string contentType = client.ResponseHeaders["Content-type"].ToLower();
            if (contentType.IndexOf("gif") >= 0)
            {
              extension = ".gif";
            }
            if (contentType.IndexOf("jpg") >= 0)
            {
              extension = ".jpg";
            }
            if (contentType.IndexOf("jpeg") >= 0)
            {
              extension = ".jpg";
            }
            if (contentType.IndexOf("bmp") >= 0)
            {
              extension = ".bmp";
            }
            if (extension.Length > 0)
            {
              string newFile = Path.ChangeExtension(FileName, extension);
              if (!newFile.ToLower().Equals(FileName.ToLower()))
              {
                try
                {
                  File.Delete(newFile);
                }
                catch (Exception)
                {
                  Log.Info("DownloadedImage:Download() Delete failed:{0}", newFile);
                }
                File.Move(FileName, newFile);
                FileName = newFile;
              }
            }
          }
          catch (Exception)
          {
            Log.Info("DownloadedImage:Download() DownloadFile failed:{0}->{1}", URL, FileName);
          }
          _dateDownloaded = DateTime.Now;
          return true;
        }
        catch (Exception ex)
        {
          Log.Info("download failed:{0}", ex.Message);
        }
      }
      return false;
    }
  }
}