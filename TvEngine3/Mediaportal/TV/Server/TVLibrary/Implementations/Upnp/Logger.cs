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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using UPnP.Infrastructure;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Upnp
{
  public class Logger : ILogger
  {
    public void Debug(string format, params object[] args)
    {
      this.LogDebug(format, args);
    }

    public void Debug(string format, Exception ex, params object[] args)
    {
      this.LogDebug(ex, format, args);
    }

    public void Info(string format, params object[] args)
    {
      this.LogInfo(format, args);
    }

    public void Info(string format, Exception ex, params object[] args)
    {
      this.LogInfo(ex, format, args);
    }

    public void Warn(string format, params object[] args)
    {
      this.LogWarn(format, args);
    }

    public void Warn(string format, Exception ex, params object[] args)
    {
      this.LogWarn(ex, format, args);
    }

    public void Error(string format, params object[] args)
    {
      this.LogError(format, args);
    }

    public void Error(string format, Exception ex, params object[] args)
    {
      this.LogError(ex, format, args);
    }

    public void Error(Exception ex)
    {
      this.LogError(ex);
    }

    public void Critical(string format, params object[] args)
    {
      this.LogCritical(format, args);
    }

    public void Critical(string format, Exception ex, params object[] args)
    {
      this.LogCritical(ex, format, args);
    }

    public void Critical(Exception ex)
    {
      this.LogCritical(ex, "UPnP: critical exception");
    }
  }
}