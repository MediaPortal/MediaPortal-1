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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Logging
{
  public static class LogExtensions
  {
    public static void LogDebug(this object caller, string message, params object[] args)
    {
      Log.Debug(caller.GetType(), message, args);      
    }

    public static void LogDebug(this object caller, Exception exception, string message, params object[] args)
    {
      Log.Debug(caller.GetType(), exception, message, args);
    }    

    public static void LogInfo(this object caller, string message, params object[] args)
    {
      Log.Info(caller.GetType(), message, args);      
    }

    public static void LogInfo(this object caller, Exception exception, string message, params object[] args)
    {
      Log.Info(caller.GetType(), exception, message, args);
    }    

    public static void LogWarn(this object caller, string message, params object[] args)
    {
      Log.Warn(caller.GetType(), message, args);
    }

    public static void LogWarn(this object caller, Exception exception, string message, params object[] args)
    {
      Log.Warn(caller.GetType(), exception, message, args);
    }    

    public static void LogCritical(this object caller, string message, params object[] args)
    {
      Log.Critical(caller.GetType(), message, args);
    }

    public static void LogCritical(this object caller, Exception exception, string message, params object[] args)
    {
      Log.Critical(caller.GetType(), exception, message, args);
    }    

    public static void LogError(this object caller, string message, params object[] args)
    {
      Log.Error(caller.GetType(), message, args);
    }
    public static void LogError(this object caller, Exception exception, string message, params object[] args)
    {
      Log.Error(caller.GetType(), message, exception, args);      
    }

    public static void LogError(this object caller, Exception exception)
    {
      Log.Error(caller.GetType(), "", exception);
    }
  }
}
