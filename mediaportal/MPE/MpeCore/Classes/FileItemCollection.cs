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

namespace MpeCore.Classes
{
  public class FileItemCollection
  {
    public FileItemCollection()
    {
      Items = new List<FileItem>();
    }

    /// <summary>
    /// Gets or sets list of included FileItems.
    /// </summary>
    /// <value>The items.</value>
    public List<FileItem> Items { get; set; }


    /// <summary>
    /// Adds the specified file item.
    /// </summary>
    /// <param name="item">The file item.</param>
    public void Add(FileItem item)
    {
      Items.Add(item);
    }

    /// <summary>
    /// Determine if exists the name of the local file.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns></returns>
    public bool ExistLocalFileName(FileItem item)
    {
      return ExistLocalFileName(item.LocalFileName);
    }

    /// <summary>
    /// Determine if exists the name of the local file.
    /// </summary>
    /// <param name="fileName">File name with path.</param>
    /// <returns></returns>
    public bool ExistLocalFileName(string fileName)
    {
      foreach (FileItem item in Items)
      {
        if (item.LocalFileName.CompareTo(fileName) == 0)
          return true;
      }
      return false;
    }


    /// <summary>
    /// Gets the file item identified by the local file name
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public FileItem GetByLocalFileName(string fileName)
    {
      foreach (FileItem item in Items)
      {
        if (item.LocalFileName.CompareTo(fileName) == 0)
          return item;
      }
      return null;
    }

    public FileItem GetByLocalFileName(FileItem item)
    {
      return GetByLocalFileName(item.LocalFileName);
    }

    /// <summary>
    /// Gets the specified item by the local filename and destination teplate.
    /// </summary>
    /// <param name="fileName">Name of the local file.</param>
    /// <param name="dest">Destination template</param>
    /// <returns>If not found return Null</returns>
    public FileItem Get(string fileName, string dest)
    {
      foreach (FileItem item in Items)
      {
        if (item.LocalFileName.CompareTo(fileName) == 0 && item.DestinationFilename.CompareTo(dest) == 0)
          return item;
      }
      return null;
    }
  }
}