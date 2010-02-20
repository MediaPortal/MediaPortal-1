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
using System.Collections.Generic;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.Support;

namespace WatchDog
{
  /// <summary>
  /// Performs actions necessary after doing MediaPortal tests.
  /// </summary>
  public class PostTestActions : ProgressDialog
  {
    private int totalActions;

    private string _tmpDir;
    private string _zipFile;

    // Constructor
    public PostTestActions(string tmpDir, string zipFile)
    {
      _tmpDir = tmpDir;
      _zipFile = zipFile;
    }

    private void updateProgress(int subActions)
    {
      int actionAmount = 100 / totalActions;
      int subActionAmount = actionAmount / subActions;
      base.setProgress(base.getProgress() + subActionAmount);
    }

    private delegate void LoggerDelegate(string destinationfolder);

    private void CreateHTMLIndexFile()
    {
      StreamWriter sw = new StreamWriter(_tmpDir + "\\index.html");
      sw.WriteLine("<html><head><title>MediaPortal Test Tool -- LogReport</title></head><body>");
      sw.WriteLine("<h1>MediaPortal Test Tool<br>Logs created at " + DateTime.Now.ToString() + "</h1>");
      sw.WriteLine("<table border=0><tr>");
      // System infos
      sw.WriteLine("<td bgcolor=lemonchiffon valign=top><strong>System infos</strong>");
      sw.WriteLine("<ul>");
      sw.WriteLine("<li><a href=PlatformInfo.html>Platform infos</a></li>");
      sw.WriteLine("<li><a href=DxDiag.txt>DirectX infos</a></li>");
      sw.WriteLine("<li><a href=Hotfixes.xml>Hotfixes</a></li>");
      sw.WriteLine("</ul></td><td width=10></td>");
      // Eventlog
      sw.WriteLine("<td bgcolor=lemonchiffon valign=top><strong>Eventlog</strong>");
      sw.WriteLine("<ul>");
      sw.WriteLine("<li><a href=system_eventlog.csv>System eventlog</a></li>");
      sw.WriteLine("<li><a href=application_eventlog.csv>Application eventlog</a></li>");
      sw.WriteLine("</ul></td>");
      sw.WriteLine("</tr><tr height=10><td></td><td></td><td></td></tr><tr>");
      // MediaPortal logs
      sw.WriteLine("<td bgcolor=lemonchiffon valign=top><strong>MediaPortal logs</strong>");
      sw.WriteLine("<ul>");
      sw.WriteLine("<li><a href=MediaPortal.log>MediaPortal.log</a></li>");
      sw.WriteLine("<li><a href=Configuration.log>Configuration.log</a></li>");
      sw.WriteLine("<li><a href=Error.log>Error.log</a></li>");
      sw.WriteLine("<li><a href=Recorder.log>Recorder.log</a></li>");
      if (File.Exists(_tmpDir + "\\TsReader.log"))
      {
        sw.WriteLine("<li><a href=tsreader.log>TsReader.log</a></li>");
      }
      sw.WriteLine("<li><a href=vmr9.log>vmr9.log</a></li>");
      sw.WriteLine("</ul></td></ul></td><td width=10></td>");
      // TvServer logs
      if (File.Exists(_tmpDir + "\\tvserver_tv.log"))
      {
        sw.WriteLine("<td bgcolor=lemonchiffon valign=top><strong>TvServer logs</strong>");
        sw.WriteLine("<ul>");
        sw.WriteLine("<li><a href=tvserver_tv.log>TV.log</a></li>");
        sw.WriteLine("<li><a href=tvserver_epg.log>EPG.log</a></li>");
        sw.WriteLine("<li><a href=tvserver_error.log>Error.log</a></li>");
        sw.WriteLine("<li><a href=tvserver_Streaming Server.log>Streaming Server.log</a></li>");
        sw.WriteLine("<li><a href=tvserver_TsWriter.log>TsWriter.log</a></li>");
        sw.WriteLine("<li><a href=tvserver_Player.log>Player.log</a></li>");
        sw.WriteLine("</ul></td>");
      }
      else
      {
        sw.WriteLine("<td bgcolor=lemonchiffon valign=top><strong>TvServer logs <i>(not available)</i></strong></td>");
      }
      sw.WriteLine("</tr></table></body></html>");
      sw.Close();
    }

    public bool PerformActions()
    {
      List<ILogCreator> logs = CreateLoggers();

      totalActions = logs.Count;


      if (!Directory.Exists(_tmpDir))
      {
        Directory.CreateDirectory(_tmpDir);
      }

      foreach (ILogCreator logCreator in logs)
      {
        setAction(logCreator.ActionMessage);
        Update();
        logCreator.CreateLogs(_tmpDir);
        updateProgress(1);
      }

      CreateHTMLIndexFile();

      base.setAction("Creating ZIP Archive with gathered information...");
      try
      {
        // Get config file also to help debugging
        File.Copy(Config.GetFolder(Config.Dir.Config) + @"\\MediaPortal.xml", _tmpDir + @"\\MediaPortal.xml", true);

        if (File.Exists(_zipFile))
        {
          File.Delete(_zipFile);
        }
        using (Archiver archiver = new Archiver())
        {
          archiver.AddDirectory(_tmpDir, _zipFile, false);
        }
        Directory.Delete(_tmpDir, true);
      }
      catch (Exception ex)
      {
        Utils.ErrorDlg(ex.ToString());
      }
      updateProgress(1);
      base.Done();
      return true;
    }

    private List<ILogCreator> CreateLoggers()
    {
      List<ILogCreator> logs = new List<ILogCreator>();
      logs.Add(new MediaPortalLogs(Config.GetFolder(Config.Dir.Log)));
      logs.Add(new TvServerLogger());
      logs.Add(new DxDiagLog(new ProcessRunner()));
      logs.Add(new HotFixInformationLogger());
      logs.Add(new EventLogCsvLogger("Application"));
      logs.Add(new EventLogCsvLogger("System"));
      logs.Add(new PlatformLogger());
      return logs;
    }
  }
}