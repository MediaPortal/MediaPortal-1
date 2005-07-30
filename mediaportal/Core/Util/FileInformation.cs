/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System;
using System.IO;

namespace Core.Util
{
	/// <summary>
	/// Summary description for FileInformation.
	/// </summary>
  public class FileInformation
  {
    long      length=0;
    DateTime  creationTime=DateTime.MinValue;
		DateTime  modificationTime=DateTime.MinValue;
    string    name=String.Empty;

    public FileInformation()
    {
    }

    public FileInformation(string file)
    {
      System.IO.FileInfo info = new System.IO.FileInfo(file);
      Length=info.Length;
      Name=info.Name;
      try
      {
        CreationTime=info.CreationTime;
				ModificationTime=info.LastWriteTime;
      }
      catch(Exception)
      {
				creationTime=DateTime.MinValue;
				ModificationTime=DateTime.MinValue;
      }
    }
    public long Length
    {
      get { return length;}
      set { length=value;}
    }
    
    public string Name
    {
      get { return name;}
      set { name=value;}
    }
    public DateTime CreationTime
    {
      get { return creationTime;}
      set { creationTime=value;}
		}
		public DateTime ModificationTime
		{
			get { return modificationTime;}
			set { modificationTime=value;}
		}
 	}
}
