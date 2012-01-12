#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

namespace MediaPortal.Services
{
  public interface ILog
  {
    void BackupLogFiles();
    void BackupLogFile(LogType logType);
    
    void Info(string format, params object[] arg);
    void Info(LogType type, string format, params object[] arg);
    
    void Warn(string format, params object[] arg);
    void Warn(LogType type, string format, params object[] arg);
    
    void Debug(string format, params object[] arg);
    void Debug(LogType type, string format, params object[] arg);
    
    void Error(string format, params object[] arg);
    void Error(LogType type, string format, params object[] arg);
    void Error(Exception ex);

    void SetConfigurationMode();
    
    /// <summary>
    /// Sets the loglevel on the fly.
    /// </summary>
    /// <param name="logLevel">The loglevel.</param>
    void SetLogLevel(Level logLevel);
  }
}