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

namespace MediaPortal.ServiceImplementations
{
  public class Log
  {
    public static void Info(string format, params object[] arg)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Info(format, arg);
    }
    
    public static void Error(string format, params object[] arg)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Error(format, arg);
    }

    public static void Error(Exception ex)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Error(ex);
    }

    public static void Warn(string format, params object[] arg)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Warn(format, arg);
    }

    public static void Debug(string format, params object[] arg)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Debug(format, arg);
    }

    public static void SetLogLevel(LogLevel logLevel)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Level = logLevel;
    }
  }
}