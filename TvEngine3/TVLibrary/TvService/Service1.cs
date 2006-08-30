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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using TvLibrary.Log;
using TvControl;

namespace TvService
{
  public partial class Service1 : ServiceBase
  {
    #region variables
    TVController _controller;
    #endregion

    public Service1()
    {
      string applicationPath = System.Windows.Forms.Application.ExecutablePath;
      applicationPath = System.IO.Path.GetFullPath(applicationPath);
      applicationPath = System.IO.Path.GetDirectoryName(applicationPath);
      System.IO.Directory.SetCurrentDirectory(applicationPath);
      InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
      Log.WriteFile("TV service started");
      Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      Process currentProcess = Process.GetCurrentProcess();
      //currentProcess.PriorityClass = ProcessPriorityClass.High;
      _controller = new TVController();
      StartRemoting();

    }

    protected override void OnStop()
    {
      Log.WriteFile("TV service stopping");
      
      StopRemoting();
      RemoteControl.Clear();
      if (_controller != null)
      {
        _controller.Dispose();
        _controller = null;
      }
      GC.Collect();
      GC.Collect();
      GC.Collect();
      GC.Collect();
      Log.WriteFile("TV service stopped");
    }

    void StartRemoting()
    {
      try
      {
        // setup the remoting channels
        try
        {
          string remotingFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
          // process the remoting configuration file
          RemotingConfiguration.Configure(remotingFile, false);
          

        }
        catch (Exception ex)
        {
          Log.Write(ex);
        }

        // create the object reference and make the singleton instance available
        ObjRef objref = RemotingServices.Marshal(_controller, "TvControl", typeof(TvControl.IController));
        RemoteControl.Clear();

      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    void StopRemoting()
    {
      Log.WriteFile("TV service StopRemoting");
      try
      {
        if (_controller != null)
        {
          RemotingServices.Disconnect(_controller);
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      Log.WriteFile("Remoting stopped");
    }
    public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      Log.WriteFile("Tvservice stopped due to a thread exception");
      Log.Write(e.Exception);
    }

    void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      Log.WriteFile("Tvservice stopped due to a app domain exception {0}",e.ExceptionObject);
    }

  }
}
