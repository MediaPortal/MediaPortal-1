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

namespace MediaPortal.Util
{
  /// <summary>
  /// Summary description for FileInformation.
  /// </summary>
  public class FileInformation
  {
    private long length = 0;
    private DateTime creationTime = DateTime.MinValue;
    private DateTime modificationTime = DateTime.MinValue;
    private string name = string.Empty;

    public FileInformation() {}

    public FileInformation(string file, bool isFolder)
    {
      if (isFolder)
      {
        System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(file);
        Length = 0;
        Name = info.Name;
        try
        {
          CreationTime = info.CreationTime;
          ModificationTime = info.LastWriteTime;
        }
        catch (Exception) {}
      }
      else
      {
        System.IO.FileInfo info = new System.IO.FileInfo(file);
        Length = info.Length;
        Name = info.Name;
        try
        {
          CreationTime = info.CreationTime;
          ModificationTime = info.LastWriteTime;
        }
        catch (Exception) {}
      }
    }

    public long Length
    {
      get { return length; }
      set { length = value; }
    }

    public string Name
    {
      get { return name; }
      set { name = value; }
    }

    public DateTime CreationTime
    {
      get { return creationTime; }
      set { creationTime = value; }
    }

    public DateTime ModificationTime
    {
      get { return modificationTime; }
      set { modificationTime = value; }
    }
  }
}