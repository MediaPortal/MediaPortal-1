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
  public class LogSharedFileWriter : StreamWriter
  {
    private StreamWriter _errorStream = null;
    private StreamWriter _logStream = null;
    private string _errorName;
    private string _logName;
    private string _buffer;

    public LogSharedFileWriter(string directory, string name)
      : base(directory + "\\" + name + ".log", true)
    {
      base.Close();
      _buffer = string.Empty;
      _logName = directory + "\\" + name + ".log";
      _errorName = directory + "\\" + name + "_error.log";
    }

    public override void WriteLine(string value)
    {
      try
      {
        _logStream = new StreamWriter(_logName, true);
        if (value.IndexOf("[ERROR]") != -1)
          _errorStream = new StreamWriter(_errorName, true);
      }
      catch (IOException)
      {
        _buffer += value;
        return;
      }

      if (_buffer != string.Empty)
      {
        WriteFile(_buffer);
        _buffer = string.Empty;
      }

      WriteFile(value);

      _logStream.Close();
      if (value.IndexOf("[ERROR]") != -1)
        _errorStream.Close();
    }

    private void WriteFile(string value)
    {
      _logStream.WriteLine(value);
      if (value.IndexOf("[ERROR]") != -1)
        _errorStream.WriteLine(value);
    }
  }
}
