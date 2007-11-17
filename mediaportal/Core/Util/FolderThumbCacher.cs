#region Copyright (C) 2007 Team MediaPortal

/* 
 *	Copyright (C) 2007 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using System.Threading;

using MediaPortal.Services;
using MediaPortal.ServiceImplementations;
using MediaPortal.TagReader;
using MediaPortal.Threading;


namespace MediaPortal.Util
{
  /// <summary>
  /// searches for folder.jpg in the mp3 directory and creates cached thumbs in MP's thumbs\folder dir
  /// </summary>
  public class FolderThumbCacher
  {
    string _filename = string.Empty;
    bool _overWrite = false;
    Work work;

    // aFilePath must only be the path of the directory
    public FolderThumbCacher(string aFilePath, bool aOverWriteExisting)
    {
      lock (_filename)
      {
        _filename = aFilePath;
        _overWrite = aOverWriteExisting;
        work = new Work(new DoWorkHandler(this.PerformRequest));
        work.ThreadPriority = ThreadPriority.Lowest;
        GlobalServiceProvider.Get<IThreadPool>().Add(work, QueuePriority.Low);
      }
    }

    void PerformRequest()
    {
      bool replace = _overWrite;
      string filename = _filename;
      string strFolderThumb = string.Empty;
      strFolderThumb = MediaPortal.Util.Utils.GetLocalFolderThumbForDir(filename);

      string strRemoteFolderThumb = string.Empty;
      strRemoteFolderThumb = String.Format(@"{0}\folder.jpg", MediaPortal.Util.Utils.RemoveTrailingSlash(filename));

      if (System.IO.File.Exists(strRemoteFolderThumb))
      {
        // if there was no cached thumb although there was a folder.jpg then the user didn't scan his collection:
        // -- punish him with slowness and create the thumbs for the next time...
        try
        {
          Log.Info("GUIMusicFiles: On-Demand-Creating missing folder thumb cache for {0}", strRemoteFolderThumb);
          string localFolderLThumb = Util.Utils.ConvertToLargeCoverArt(strFolderThumb);

          if (!System.IO.File.Exists(strFolderThumb) || replace)
            MediaPortal.Util.Picture.CreateThumbnail(strRemoteFolderThumb, strFolderThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0, true);
          if (!System.IO.File.Exists(localFolderLThumb) || replace)
          {
            // just copy the folder.jpg if it is reasonable in size - otherwise re-create it
            System.IO.FileInfo fiRemoteFolderArt = new System.IO.FileInfo(strRemoteFolderThumb);
            if (fiRemoteFolderArt.Length < 32000)
              System.IO.File.Copy(strRemoteFolderThumb, localFolderLThumb, true);
            else
              MediaPortal.Util.Picture.CreateThumbnail(strRemoteFolderThumb, localFolderLThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0, false);
          }

          return;
        }
        catch (Exception)
        {
          return;
        }
      }
    }
  }
}