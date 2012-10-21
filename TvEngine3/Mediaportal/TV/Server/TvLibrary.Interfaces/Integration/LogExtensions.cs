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
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Integration
{
  public static class LogExtensions
  {
    static string BuildPrefix(object caller, string message)
    {
      return string.Format("{0}: {1}", caller.GetType().Name, message);
    }

    public static void LogDebug(this object caller, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Debug(BuildPrefix(caller, message), args);
    }
    public static void LogInfo(this object caller, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Info(BuildPrefix(caller, message), args);
    }
    public static void LogError(this object caller, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Error(BuildPrefix(caller, message), args);
    }
    public static void LogError(this object caller, Exception exception, string message, params object[] args)
    {
      GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger.Error(BuildPrefix(caller, message), exception, args);
    }
  }
}
