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

namespace MediaPortal.Services
{
  public interface ILog
  {
    void BackupLogFiles();
    void BackupLogFile(LogType logType);

    [Obsolete("This method will disappear.  Use one of the Info, Warn, Debug or Error variants instead.", true)]
    void Write(Exception ex);

    /// <summary>
    /// Write a string to the logfile.
    /// </summary>
    /// <param name="format">The format of the string.</param>
    /// <param name="arg">An array containing the actual data of the string.</param>
    [Obsolete("This method will disappear.  Use one of the Info, Warn, Debug or Error variants instead.", true)]
    void Write(string format, params object[] arg);

    void Info(string format, params object[] arg);
    void Info(LogType type, string format, params object[] arg);
    void Warn(string format, params object[] arg);
    void Warn(LogType type, string format, params object[] arg);
    void Debug(string format, params object[] arg);
    void Debug(LogType type, string format, params object[] arg);
    void Error(string format, params object[] arg);
    void Error(LogType type, string format, params object[] arg);
    void Error(Exception ex);

    ///// <summary>
    ///// Write a string to the logfile.
    ///// </summary>
    ///// <param name="format">The format of the string.</param>
    ///// <param name="arg">An array containing the actual data of the string.</param>
    //[Obsolete("This method will disappear because the thread information is always logged now.",false)]
    //void WriteThreadId(string format, params object[] arg);
    //[Obsolete("This method will disappear because the thread information is always logged now.", false)]
    //void WriteThreadId(LogType type, string format, params object[] arg);
    //[Obsolete("This method will disappear because the thread information is always logged now.", false)]
    //void WriteFileThreadId(LogType type, bool isError, string format, params object[] arg);
    //[Obsolete("This method will disappear because the thread information is always logged now.", false)]
    //void InfoThread(string format, params object[] arg);
    //[Obsolete("This method will disappear because the thread information is always logged now.", false)]
    //void WarnThread(string format, params object[] arg);
    //[Obsolete("This method will disappear because the thread information is always logged now.", false)]
    //void ErrorThread(string format, params object[] arg);
    void SetConfigurationMode();
    void WriteFile(LogType type, bool isError, string format, params object[] arg);
    void WriteFile(LogType type, string format, params object[] arg);
    void WriteFile(LogType type, Level logLevel, string format, params object[] arg);

    /// <summary>
    /// Sets the loglevel on the fly.
    /// </summary>
    /// <param name="logLevel">The loglevel.</param>
    void SetLogLevel(Level logLevel);
  }
}