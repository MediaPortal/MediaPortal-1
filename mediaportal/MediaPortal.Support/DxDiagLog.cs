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

namespace MediaPortal.Support
{
  public class DxDiagLog : ILogCreator
  {
    private ProcessRunner runner;

    public DxDiagLog(ProcessRunner runner)
    {
      this.runner = runner;
    }

    public void CreateLogs(string destinationFolder)
    {
      string dstFile = destinationFolder + "\\dxdiag.txt";
      CreateDxDiagFile(dstFile);
    }

    private void CreateDxDiagFile(string tmpFile)
    {
      string executable = Environment.GetEnvironmentVariable("windir") + @"\system32\dxdiag.exe";
      string arguments = "/whql:off /t " + tmpFile;
      runner.Arguments = arguments;
      runner.Executable = executable;
      runner.Run();
    }

    public string ActionMessage
    {
      get { return "Gathering DirectX diagnostics text file..."; }
    }
  }
}