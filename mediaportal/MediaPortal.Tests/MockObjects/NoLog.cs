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
using MediaPortal.Services;

namespace MediaPortal.Tests.MockObjects
{
  /// <summary>
  /// Dummy <see cref="ILog"/> service implementation that does absolutely nothing.
  /// </summary>
  public class NoLog : ILog
  {
    public void BackupLogFiles() {}

    public void BackupLogFile(LogType logType) {}

    public void Write(Exception ex) {}

    /// <summary>
    /// Write a string to the logfile.
    /// </summary>
    /// <param name="format">The format of the string.</param>
    /// <param name="arg">An array containing the actual data of the string.</param>
    public void Write(string format, params object[] arg) {}

    public void Info(string format, params object[] arg) {}

    public void Info(LogType type, string format, params object[] arg) {}

    public void Warn(string format, params object[] arg) {}

    public void Warn(LogType type, string format, params object[] arg) {}

    public void Debug(string format, params object[] arg) {}

    public void Debug(LogType type, string format, params object[] arg) {}

    public void Error(string format, params object[] arg) {}

    public void Error(LogType type, string format, params object[] arg) {}

    public void Error(Exception ex) {}


    public void SetConfigurationMode() {}

    public void WriteFile(LogType type, bool isError, string format, params object[] arg) {}

    public void WriteFile(LogType type, string format, params object[] arg) {}

    public void WriteFile(LogType type, Level logLevel, string format, params object[] arg) {}

    public void SetLogLevel(Level logLevel) {}
  }
}