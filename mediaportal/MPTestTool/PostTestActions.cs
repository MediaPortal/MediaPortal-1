#region Copyright (C) 2005 Team MediaPortal

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

#endregion

using System;
using System.IO;
using System.Xml;
using System.Data;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

using MediaPortal.Support;
using System.Collections.Generic;

namespace MPTestTool
{
  /// <summary>
  /// Performs actions necessary after doing MediaPortal tests.
  /// </summary>
  public class PostTestActions : ProgressDialog, IActionsCanceler
  {
    private bool actionsContinue = true;

    private int totalActions;

    private string mypath;
    private string mpdir;
    private string mplogdir;
    private string resultFile;
    private string nick;
    private bool havePsLogList;

    struct MPVersion
    {
      public string Version;
      public string MPexeCreationTime;
      public string MPexeHash;
      public string LookupText;

      public MPVersion(string Version,
                       string MPexeCreationTime,
                       string MPexeHash,
                       string LookupText
                      )
      {
        this.Version = Version;
        this.MPexeCreationTime = MPexeCreationTime;
        this.MPexeHash = MPexeHash;
        this.LookupText = LookupText;
      }
    }

    // Constructor
    public PostTestActions(string mypath, string mpdir, string logdir, string nick, bool havePsLogList)
    {
      this.mypath = mypath;
      this.mpdir = mpdir;
      this.mplogdir = mpdir + "log";
      this.resultFile = logdir;
      this.nick = nick;
      this.havePsLogList = havePsLogList;
      base.setCaller(this);
    }
    public void ActionCanceled()
    {
      actionsContinue = false;
    }
    private void Error(string text)
    {
      MessageBox.Show(
                      "An Error occurred:\n\n" + text,
                      "Error",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Error
                     );
    }
    private void updateProgress(int subActions)
    {
      int actionAmount = 100 / totalActions;
      int subActionAmount = actionAmount / subActions;
      base.setProgress(base.getProgress() + subActionAmount);
    }

    private delegate void LoggerDelegate(string destinationfolder);

    public bool PerformActions()
    {
      List<ILogCreator> logs = CreateLoggers();

      totalActions = logs.Count;

      List<WaitHandle> waitHandles = new List<WaitHandle>();

      string logdir = Environment.GetEnvironmentVariable("TEMP") + "\\MediaPortal";
      EmptyDirectory(logdir);

      foreach (ILogCreator logCreator in logs)
      {
        LoggerDelegate del = new LoggerDelegate(logCreator.CreateLogs);
        if (actionsContinue)
        {
          base.setAction(logCreator.ActionMessage);
          IAsyncResult iar = del.BeginInvoke(logdir, delegate(IAsyncResult apa)
          {
            del.EndInvoke(apa);
          }, null);
          waitHandles.Add(iar.AsyncWaitHandle);
        }
      }

      WaitForAllToFinish(waitHandles);

      if (actionsContinue)
      {
        base.setAction("Creating ZIP Archive with gathered information...");
        try
        {
          using (Archiver archiver = new Archiver(resultFile))
            archiver.AddDirectory(logdir);
          Directory.Delete(logdir, true);
        }
        catch (Exception ex)
        {
          Error(ex.ToString());
        }
        updateProgress(1);
      }
      if (actionsContinue)
        base.Done();
      return actionsContinue;
    }

    private static void EmptyDirectory(string logdir)
    {
      if (Directory.Exists(logdir))
        Directory.Delete(logdir, true);

      Directory.CreateDirectory(logdir);
    }

    private void WaitForAllToFinish(List<WaitHandle> waitHandles)
    {
      WaitHandle[] handleArray = waitHandles.ToArray();

      while (!WaitHandle.WaitAll(handleArray, 0, false))
      {
        WaitHandle.WaitAny(handleArray);
        updateProgress(1);
      }
    }

    private List<ILogCreator> CreateLoggers()
    {
      List<ILogCreator> logs = new List<ILogCreator>();
      logs.Add(new MediaPortalLogs(mplogdir));
      logs.Add(new DxDiagLog(new ProcessRunner()));
      logs.Add(new HotFixInformationLogger());

      if (havePsLogList)
      {
        logs.Add(new PsLogEventsLogger(new ProcessRunner()));
      }
      else
      {
        logs.Add(new EventLogLogger("Application"));
        logs.Add(new EventLogLogger("System"));
      }
      return logs;
    }

    private void MoveLogsToZipFile(string zipFile)
    {

    }

    private MPVersion GetMPVersion()
    {
      String version = String.Empty;
      String xmlVersionURL = @"http://scoop.cybox.nl/mediaportal/mediaportalversions.xml";
      String lookupText = "Online lookup successful";
      String localVersion = String.Empty;
      String localHash = String.Empty;
      String localCreationTime = String.Empty;
      String MPexe = mpdir + "MediaPortal.exe";

      // Create the DataSet
      DataSet ds = new DataSet("mediaportalversions");
      DataTable mpversions = new DataTable("mpversion");
      DataColumn v = new DataColumn("version");
      DataColumn h = new DataColumn("hash");
      mpversions.Columns.Add(v);
      mpversions.Columns.Add(h);
      ds.Tables.Add(mpversions);

      // Gather local info
      localHash = FileUtils.getHashValue(MPexe);
      localCreationTime = FileUtils.getCreationTime(MPexe);
      ConfigReader cr = new ConfigReader();
      cr.configFile = mpdir + "MediaPortal.exe.config";
      localVersion = cr.GetValue("//appSettings//add[@key='version']");

      // Try remote version lookup
      try
      {
        ds.ReadXml(new XmlTextReader(xmlVersionURL));
        foreach (DataRow row in mpversions.Rows)
        {
          if (row["hash"].Equals(localHash))
            version = (String)row["version"];
        }
      }
      catch (Exception ex)
      {
        lookupText = "Online lookup failed (" + ex.Message + ")";
        version = localVersion;
      }
      if (version.Equals(String.Empty))
      {
        version = localVersion;
        lookupText = "No matching version found in online database";
      }

      // Return gathered info
      return new MPVersion(version, localCreationTime, localHash, lookupText);
    }
    //private void CreateSysInfoFile()
    //{
    //  int subActions = 1;
    //  base.setAction("Gathering system information...");
    //  MPVersion v = GetMPVersion();
    //  using (StreamWriter sw = new StreamWriter(logdir + nick + "_" + "sysinfo.html"))
    //  {
    //    sw.Write(
    //                 HTMLUtils.getHeader(
    //                                     "System information from " + nick,
    //                                     "Verdana",
    //                                     "10pt",
    //                                     "#ffffff"
    //                                    )
    //                );
    //    sw.Write(HTMLUtils.getHeading("System information from " + nick));
    //    sw.Write(HTMLUtils.getMPVersionInfo(v.Version, v.MPexeCreationTime, v.MPexeHash, v.LookupText));
    //    HotfixInformation hi = new HotfixInformation();
    //    foreach (string cat in hi.GetCategories())
    //    {
    //      Hashtable h = hi.GetHotfixes(cat);
    //      sw.Write(HTMLUtils.getHotfixTable(cat, h));
    //    }
    //    sw.Write(HTMLUtils.getFooter());
    //  }
    //  updateProgress(subActions);
    //}


  }

}
