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
using MediaPortal.CoreServices;
using MediaPortal.Services;

namespace MediaPortal.Tests.MockObjects
{
  /// <summary>
  /// Dummy <see cref="ILog"/> service implementation that does absolutely nothing.
  /// </summary>
  public class NoLog : ILogger
  {
    #region Implementation of ILogger

    private LogLevel _level;

    public LogLevel Level
    {
      get { return _level; }
      set { _level = value; }
    }

    public void Debug(string format, params object[] args) {}
    
    public void Info(string format, params object[] args) {}

    public void Warn(string format, params object[] args) {}

    public void Error(string format, params object[] args) {}

    public void Error(Exception ex) {}

    public void Error(string message, Exception ex) {}

    public void Critical(string format, params object[] args) {}

    public void Critical(Exception ex) {}

    public void Critical(string message, Exception ex) {}

    public void Epg(string format, params object[] args) {}
    

    #endregion
  }
}