/*
  *	Copyright (C) 2005 Team MediaPortal
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
using System.IO;
using System.Net;
using System.Web;
using System.Text;
//using System.Collections.Generic;
using System.Windows.Forms;
using MediaPortal.EPG;
using MediaPortal.Utils.Services;
using MediaPortal.Webepg.TV.Database;

namespace MediaPortal.EPG.TestWebEPG
{
  public class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]

    static void Main()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      ILog log = new Log("WebEPG", Log.Level.Debug);
      services.Add<ILog>(log);

      log.Info("WebEPG: Starting");
      System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;

#if DEBUG
        
      WebEPG epg = new WebEPG();
      epg.Import();
        
#else
      try
      {
        WebEPG epg = new WebEPG();
        epg.Import();
      }
      catch (Exception ex)
      {
        log.Error("WebEPG: Fatal Error");
        log.Error("WebEPG: {0}", ex.Message);
      }
#endif

      log.Info("WebEPG: Finished");
    }
  }
}