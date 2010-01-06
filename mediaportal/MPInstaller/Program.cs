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
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using MediaPortal.MPInstaller;
using MediaPortal.Configuration;


namespace MediaPortal.MPInstaller
{
  internal static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      string fil = string.Empty;
      if (args.Length > 0)
      {
        fil = args[0];
      }
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Thread.CurrentThread.Name = "MPInstaller";

      if (fil == @"/queue")
      {
        QueueEnumerator queue = new QueueEnumerator();
        QueueInstaller dlg = new QueueInstaller();
        dlg.queue = queue.Load(MpiFileList.QUEUE_LISTING);
        if (dlg.queue.Items.Count > 0)
        {
          Application.Run(dlg);
        }
        System.Diagnostics.Process.Start(Config.GetFile(Config.Dir.Base, "MediaPortal.exe"));
        return;
      }

      if (!String.IsNullOrEmpty(fil))
      {
        if (Path.GetExtension(fil).ToUpper() == ".MPI" || Path.GetExtension(fil).ToUpper() == ".MPE1")
        {
          InstallWizard wiz = new InstallWizard();
          wiz.package.LoadFromFile(fil);
          if (wiz.package.isValid)
          {
            wiz.starStep();
          }
          else
          {
            MessageBox.Show("Invalid package !");
          }
        }
      }
      else
      {
        Application.Run(new ControlPanel());
      }
    }
  }
}