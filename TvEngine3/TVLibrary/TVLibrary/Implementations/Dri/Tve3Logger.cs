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
using UPnP.Infrastructure;

namespace TvLibrary.Implementations.Dri
{
  public class Tve3Logger : ILogger
  {
    protected const string LOG_FORMAT_STR = "{0}: {1}";

    protected void WriteException(Exception ex)
    {
      Log.Log.WriteFile("Exception: " + ex);
      Log.Log.WriteFile("  Message: " + ex.Message);
      Log.Log.WriteFile("  Site   : " + ex.TargetSite);
      Log.Log.WriteFile("  Source : " + ex.Source);
      Log.Log.WriteFile("Stack Trace:");
      Log.Log.WriteFile(ex.StackTrace);
      if (ex.InnerException != null)
      {
        Log.Log.WriteFile("Inner Exception(s):");
        WriteException(ex.InnerException);
      }
    }

    public void Debug(string format, params object[] args)
    {
      Log.Log.Debug(LOG_FORMAT_STR, "Debug", string.Format(format, args));
    }

    public void Debug(string format, Exception ex, params object[] args)
    {
      Log.Log.Debug(LOG_FORMAT_STR, "Debug", string.Format(format, args));
      WriteException(ex);
    }

    public void Info(string format, params object[] args)
    {
      Log.Log.Info(LOG_FORMAT_STR, "Info.", string.Format(format, args));
    }

    public void Info(string format, Exception ex, params object[] args)
    {
      Log.Log.Info(LOG_FORMAT_STR, "Info.", string.Format(format, args));
      WriteException(ex);
    }

    public void Warn(string format, params object[] args)
    {
      Log.Log.Error(LOG_FORMAT_STR, "Warn.", string.Format(format, args));
    }

    public void Warn(string format, Exception ex, params object[] args)
    {
      Log.Log.Error(LOG_FORMAT_STR, "Warn.", string.Format(format, args));
      WriteException(ex);
    }

    public void Error(string format, params object[] args)
    {
      Log.Log.Error(LOG_FORMAT_STR, "Error", string.Format(format, args));
    }

    public void Error(string format, Exception ex, params object[] args)
    {
      Log.Log.Error(LOG_FORMAT_STR, "Error", string.Format(format, args));
      WriteException(ex);
    }

    public void Error(Exception ex)
    {
      Log.Log.Error(LOG_FORMAT_STR, "Crit.", "Error");
      WriteException(ex);
    }

    public void Critical(string format, params object[] args)
    {
      Log.Log.Error(LOG_FORMAT_STR, "Crit.", string.Format(format, args));
    }

    public void Critical(string format, Exception ex, params object[] args)
    {
      Log.Log.Error(LOG_FORMAT_STR, "Crit.", string.Format(format, args));
      WriteException(ex);
    }

    public void Critical(Exception ex)
    {
      Log.Log.Error(LOG_FORMAT_STR, "Crit.", "Critical error");
      WriteException(ex);
    }
  }
}
