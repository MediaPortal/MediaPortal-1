/* 
 *	Copyright (C) 2005 Team MediaPortal
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
  public class Log : ILog
  {
    private Level _minLevel;
    private TextWriter _logStream;

    public enum Level
    {
        Error = 0,
        Warning = 1,
        Information = 2,
        Debug = 3
    }

    private string GetLevelName(Level logLevel)
    {
        switch (logLevel)
        {
            case Level.Error:
                return "ERROR";

            case Level.Warning:
                return "Warn.";

            case Level.Information:
                return "Info.";

            case Level.Debug:
                return "Debug";
        }

        return "Unknown";
    }

    public Log(TextWriter stream, Level minLevel)
    {
      _minLevel = minLevel;
			_logStream = stream;
    }

	  public Log(string name, Level minLevel)
		{
			_minLevel = minLevel;
			LogFile file = new LogFile(name);
			_logStream = file.GetStream();
		}

    public void Info(string strFormat, params object[] arg)
    {
        Write(Level.Information, strFormat, arg);
    }

    public void Warn(string strFormat, params object[] arg)
    {
        Write(Level.Warning, strFormat, arg);
    }

    public void Error(string strFormat, params object[] arg)
    {
        Write(Level.Error, strFormat, arg);
    }

    public void Debug(string strFormat, params object[] arg)
    {
        Write(Level.Debug, strFormat, arg);
    }

    private void Write(Level logLevel, string strFormat, params object[] arg)
		{
      if (logLevel <= _minLevel)
      {
				StringWriter str = new StringWriter();
				str.Write(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString());
				str.Write(" [" + GetLevelName(logLevel) + "] ");
				str.Write(strFormat, arg);

				_logStream.WriteLine(str.ToString());
      }
		}
  }
}
