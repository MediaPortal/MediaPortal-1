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
using MediaPortal.Services;

namespace MediaPortal.ServiceImplementations
{
  public class Log
  {
    public static void SetConfigurationMode()
    {
      GlobalServiceProvider.Get<ILog>().SetConfigurationMode();
    }

    public static void BackupLogFiles()
    {
      GlobalServiceProvider.Get<ILog>().BackupLogFiles();
    }

    public static void BackupLogFile(LogType logType)
    {
      GlobalServiceProvider.Get<ILog>().BackupLogFile(logType);
    }

    public static void Info(string format, params object[] arg)
    {
      GlobalServiceProvider.Get<ILog>().Info(format, arg);
    }

    public static void Info(LogType type, string format, params object[] arg)
    {
      GlobalServiceProvider.Get<ILog>().Info(type, format, arg);
    }

    public static void Error(string format, params object[] arg)
    {
      GlobalServiceProvider.Get<ILog>().Error(format, arg);
    }

    public static void Error(Exception ex)
    {
      GlobalServiceProvider.Get<ILog>().Error(ex);
    }

    public static void Error(LogType type, string format, params object[] arg)
    {
      GlobalServiceProvider.Get<ILog>().Error(type, format, arg);
    }

    public static void Warn(string format, params object[] arg)
    {
      GlobalServiceProvider.Get<ILog>().Warn(format, arg);
    }

    public static void Warn(LogType type, string format, params object[] arg)
    {
      GlobalServiceProvider.Get<ILog>().Warn(type, format, arg);
    }

    public static void Debug(string format, params object[] arg)
    {
      GlobalServiceProvider.Get<ILog>().Debug(format, arg);
    }

    public static void Debug(LogType type, string format, params object[] arg)
    {
      GlobalServiceProvider.Get<ILog>().Debug(type, format, arg);
    }

    public static void WriteFile(LogType type, bool isError, string format, params object[] arg)
    {
      GlobalServiceProvider.Get<ILog>().WriteFile(type, isError, format, arg);
    }

    public static void WriteFile(LogType type, string format, params object[] arg)
    {
      GlobalServiceProvider.Get<ILog>().WriteFile(type, format, arg);
    }

    public static void SetLogLevel(Level logLevel)
    {
      GlobalServiceProvider.Get<ILog>().SetLogLevel(logLevel);
    }
  }
}