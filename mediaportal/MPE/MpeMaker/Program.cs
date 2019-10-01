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
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using MpeMaker.Classes;

namespace MpeMaker
{
  internal static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      // .NET 4.0: Use TLS v1.2. Many download sources no longer support the older and now insecure TLS v1.0/1.1 and SSL v3.
      ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xc00;
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      if (args.Length > 0)
      {
        MainForm dlg = new MainForm(new ProgramArguments(args));
        if (!dlg.IsDisposed)
          Application.Run(dlg);
      }
      else
        Application.Run(new MainForm());
    }
  }
}