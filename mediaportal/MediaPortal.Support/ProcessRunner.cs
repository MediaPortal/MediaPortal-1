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

namespace MediaPortal.Support
{
  public class ProcessRunner
  {
    protected string arguments = string.Empty;
    protected string executable = null;
    protected int lastExitCode = -1;


    public string Arguments
    {
      get { return arguments; }
      set { arguments = value; }
    }

    public string Executable
    {
      get { return executable; }
      set { executable = value; }
    }

    public int LastExitCode
    {
      get { return lastExitCode; }
    }

    public ProcessRunner() {}

    public virtual void Run()
    {
      if (executable == null)
      {
        throw new ArgumentNullException("executable");
      }

      Process pr = new Process();
      pr.StartInfo.FileName = executable;
      pr.StartInfo.Arguments = arguments;
      pr.StartInfo.CreateNoWindow = true;
      pr.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
      pr.Start();
      pr.WaitForExit();
      lastExitCode = pr.ExitCode;
    }
  }
}