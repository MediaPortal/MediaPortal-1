/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using TvDatabase;
using TvLibrary.Log;
namespace TvService
{
  public class PostProcessing
  {
    public void Process(RecordingDetail recording)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      bool comSkipEnabled = (layer.GetSetting("comskipEnabled", "yes").Value == "yes");
      if (!comSkipEnabled) return;

      string batchFile = layer.GetSetting("comskipLocation", "").Value;
      if (!System.IO.File.Exists(batchFile))
      {
        Log.WriteFile("postprocessor: cannot locate batchfile:{0}", batchFile);
        return;
      }
      string strParams = String.Format("{0} {1}",recording.FileName, recording.Channel);
      StartProcess(batchFile, strParams, false, true);

    }
    Process StartProcess(ProcessStartInfo procStartInfo, bool bWaitForExit)
    {
      Process proc = new Process();
      proc.StartInfo = procStartInfo;
      try
      {
        Log.WriteFile("Start process {0} {1}", procStartInfo.FileName, procStartInfo.Arguments);
        proc.Start();
        if (bWaitForExit)
        {
          proc.WaitForExit();
        }
      }
      catch (Exception ex)
      {
        string ErrorString = String.Format("Utils: Error starting process!\n  filename: {0}\n  arguments: {1}\n  WorkingDirectory: {2}\n  stack: {3} {4} {5}",
          proc.StartInfo.FileName,
          proc.StartInfo.Arguments,
          proc.StartInfo.WorkingDirectory,
          ex.Message,
          ex.Source,
          ex.StackTrace);
        Log.WriteFile(ErrorString);
      }
      return proc;
    }

    Process StartProcess(string strProgram, string strParams, bool bWaitForExit, bool bMinimized)
    {
      if (strProgram == null) return null;
      if (strProgram.Length == 0) return null;

      string strWorkingDir = System.IO.Path.GetFullPath(strProgram);
      string strFileName = System.IO.Path.GetFileName(strProgram);
      strWorkingDir = strWorkingDir.Substring(0, strWorkingDir.Length - (strFileName.Length + 1));

      ProcessStartInfo procInfo = new ProcessStartInfo();
      procInfo.FileName = strFileName;
      procInfo.WorkingDirectory = strWorkingDir;
      procInfo.Arguments = strParams;
      if (bMinimized)
      {
        procInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
        procInfo.CreateNoWindow = true;
      }
      return StartProcess(procInfo, bWaitForExit);
    }

  }
}
