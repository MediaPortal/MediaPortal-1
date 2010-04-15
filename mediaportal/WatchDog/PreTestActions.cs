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
using System.Diagnostics;
using System.IO;
using MediaPortal.Configuration;

namespace WatchDog
{
  /// <summary>
  /// Performs actions necessary before doing MediaPortal tests.
  /// </summary>
  public class PreTestActions : ProgressDialog
  {
    private const int totalActions = 2;
    private const int actionAmount = 100 / totalActions;

    private static string[] logNames = {"Application", "System"};

    public PreTestActions() {}

    private void updateProgress(int subActions)
    {
      int subActionAmount = actionAmount / subActions;
      base.setProgress(base.getProgress() + subActionAmount);
    }

    public bool PerformActions()
    {
      ClearEventLog();
      ClearMPLogDir();
      base.Done();
      return true;
    }

    private void ClearEventLog()
    {
      base.setAction("Clearing EventLogs...");
      Update();
      int subActions = logNames.Length;
      foreach (string strLogName in logNames)
      {
        EventLog e = new EventLog(strLogName);
        try
        {
          e.Clear();
        }
        catch (Exception) {}
        updateProgress(subActions);
      }
      if (subActions == 0)
      {
        updateProgress(1);
      }
    }

    private void ClearDir(string strDir)
    {
      string[] files = Directory.GetFiles(strDir);
      string[] dirs = Directory.GetDirectories(strDir);

      int subActions = files.Length + dirs.Length;

      foreach (string file in files)
      {
        if (File.Exists(file))
        {
          try
          {
            File.Delete(file);
            updateProgress(subActions);
          }
          catch (Exception) {}
        }
      }

      foreach (string dir in dirs)
      {
        if (Directory.Exists(dir))
        {
          try
          {
            Directory.Delete(dir, true);
            updateProgress(subActions);
          }
          catch (Exception){}
        }
      }

      if (subActions == 0)
      {
        updateProgress(1);
      }
    }

    private void ClearMPLogDir()
    {
      base.setAction("Clearing MediaPortal log subdirectory...");
      Update();
      ClearDir(Config.GetFolder(Config.Dir.Log));
    }
  }
}