/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Diagnostics;
using System.IO;

namespace MediaPortal.Utils.Services
{
	public class LogFileWriter : StreamWriter
	{
		private StreamWriter _errorStream;
		private string _errorName;

		public LogFileWriter(string directory, string name)	
			: base(directory + "\\" + name + ".log")
		{
			base.AutoFlush = true;

			_errorName = directory + "\\" + name + "_error.log";
			if (System.IO.File.Exists(_errorName))
				System.IO.File.Delete(_errorName);
		}

		public override void WriteLine(string value)
		{
			base.WriteLine(value);
			if (value.IndexOf("[ERROR]") != -1)
			{
				if (_errorStream == null)
				{
					if (System.IO.File.Exists(_errorName))
						System.IO.File.Delete(_errorName);

					_errorStream = new StreamWriter(_errorName);
					_errorStream.AutoFlush = true;
				}
				_errorStream.WriteLine(value);
			}
		}
	}
}
