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
using System.Diagnostics;
using System.IO;

namespace MediaPortal.EPG.WebEPGTester
{
	public class LogFile
	{
		private string _directory;
		private string _name;

		public LogFile(string directory, string name)
		{
			_directory = directory;
			_name = name;

			if(!System.IO.Directory.Exists(directory))
				System.IO.Directory.CreateDirectory(directory);

		  string logName = directory + "\\" + name + ".log";
			string bakName = directory + "\\" + name + ".bak";

			if (System.IO.File.Exists(bakName))
				System.IO.File.Delete(bakName);
			if (System.IO.File.Exists(logName))
				System.IO.File.Move(logName, bakName);

      string errLogName = directory + "\\" + name + "_error.log";
      //string errBakName = directory + "\\" + name + "_error.bak";

      //if (System.IO.File.Exists(errBakName))
      //  System.IO.File.Delete(errBakName);
      if (System.IO.File.Exists(errLogName))
        System.IO.File.Delete(errLogName);
		}

		public LogFile(string name) : this("log", name)
		{
		}

		public TextWriter GetStream()
		{
			return (TextWriter)new LogFileWriter(_directory, _name);
		}

    public TextWriter GetSharedStream()
    {
      return (TextWriter)new LogSharedFileWriter(_directory, _name);
    }
	}
}
