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
using TvLibrary.Interfaces;

namespace TvLibrary.Log
{
  public class Log
  {
    #region Implementation of ILogger

    public static LogLevel Level
    {
      get { return GlobalServiceProvider.Instance.Get<ILogger>().Level; }
      set { GlobalServiceProvider.Instance.Get<ILogger>().Level = value; }
    }

    public static void Epg(string format, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Epg(format, args);
    }


    public static void Debug(string format, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Debug(format, args);
    }

    public static void Debug(string format, Exception ex, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Debug(format, ex, args);
    }

    public static void Info(string format, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Info(format, args);
    }

    public static void Info(string format, Exception ex, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Info(format, ex, args);
    }

    public static void Warn(string format, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Warn(format, args);
    }

    public static void Warn(string format, Exception ex, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Warn(format, ex, args);
    }

    public static void Error(string format, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Error(format, args);
    }

    public static void Error(string format, Exception ex, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Error(format, ex, args);
    }

    public static void Error(Exception ex)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Error(ex);
    }

    public static void Write(Exception ex)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Write(ex);
    }

    public static void Write(string format, params object[] arg)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().Write(format, arg);
    }

    public static void WriteThreadId(string format, params object[] arg)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().WriteThreadId(format, arg);
    }

    public static void WriteFile(string format, params object[] arg)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().WriteFile(format, arg);
    }

    public static void WriteFile(string format, Exception ex)
    {
      GlobalServiceProvider.Instance.Get<ILogger>().WriteFile(format, ex);
    }

    #endregion
  }
}
