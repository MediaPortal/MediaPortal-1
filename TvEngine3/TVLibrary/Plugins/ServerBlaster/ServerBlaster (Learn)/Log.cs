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
using System.IO;
using System.Reflection;

namespace MediaPortal.GUI.Library
{
  public class Log
  {
    private Log() {}

    static Log()
    {
      try
      {
        Uri uri = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);

        _logFilename = uri.LocalPath.Replace(".exe", ".log");
      }
      catch
      {
        _logFilename = "myblaster.log";
      }
    }

    public static void Delete()
    {
      lock (typeof (Log)) if (File.Exists(_logFilename)) File.Delete(_logFilename);
    }

    public static void Write(string strFormat, params object[] arg)
    {
      if (_logEnabled == false) return;

      lock (typeof (Log))
      {
        try
        {
          using (StreamWriter writer = new StreamWriter(_logFilename, true))
          {
            writer.BaseStream.Seek(0, SeekOrigin.End); // set the file pointer to the end of 
            writer.Write(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ");
            writer.WriteLine(strFormat, arg);
            writer.Close();
          }

          Console.WriteLine(string.Format(strFormat, arg));
        }
        catch (Exception) {}
      }
    }

    public static string Filename
    {
      get { return _logFilename; }
    }

    public static bool Enabled
    {
      get { return _logEnabled; }
      set { _logEnabled = value; }
    }

    protected static string _logFilename;
    protected static bool _logEnabled = true;
  }
}