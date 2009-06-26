#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

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
    private int totalActions = 2;

    private static string[] logNames = {"Application", "System"};

    public PreTestActions()
    {
    }

    private void updateProgress(int subActions)
    {
      int actionAmount = 100/totalActions;
      int subActionAmount = actionAmount/subActions;
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
        catch (Exception)
        {
        }
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
          File.Delete(file);
          updateProgress(subActions);
        }
      }

      foreach (string dir in dirs)
      {
        if (Directory.Exists(dir))
        {
          Directory.Delete(dir, true);
          updateProgress(subActions);
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