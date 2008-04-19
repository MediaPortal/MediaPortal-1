#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.IO;
using System.Diagnostics;

namespace MediaPortal.Support
{
  public class TvServerLogger : ILogCreator
  {
    public TvServerLogger()
    {
    }

    public void CreateLogs(string destinationFolder)
    {
      string logPath=Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)+"\\Team MediaPortal\\MediaPortal TV Server\\log";
      if (!Directory.Exists(logPath))
        return;
      DirectoryInfo dir=new DirectoryInfo(logPath);
      FileInfo[] fis=dir.GetFiles("*.log");
      foreach (FileInfo fi in fis)
        fi.CopyTo(destinationFolder+"\\tvserver_"+fi.Name,true);
    }

    public string ActionMessage
    {
      get { return "Gathering TvServer log information if any..."; }    }
  }
}
