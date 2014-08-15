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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Timers;
using System.Xml;
using Castle.Core;
using Ionic.Zip;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.Interfaces;
using Mediaportal.TV.Server.Plugins.XmlTvImport.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.XmlTvImport
{
  [Interceptor("PluginExceptionInterceptor")]
  [ComponentProxyBehavior(AdditionalInterfaces = new [] { typeof(ITvServerPluginStartedAll), typeof(ITvServerPluginCommunciation) })]
  public class XmlTvImporter : ITvServerPlugin, ITvServerPluginStartedAll, ITvServerPluginCommunciation
  {
    #region private classes

    private class FileDownloader : WebClient
    {
      public int TimeOut = 60000;

      public FileDownloader(int timeOutMilliseconds = 60000)
      {
        TimeOut = timeOutMilliseconds;
      }

      protected override WebRequest GetWebRequest(Uri address)
      {
        var result = base.GetWebRequest(address);
        if (result != null)
        {
          result.Timeout = TimeOut;
        }
        return result;
      }
    }

    private class XmlTvFileSettings
    {
      public string FileLocation;
      public bool UseTimeCorrection = false;
      public int TimeCorrectionHours = 0;
      public int TimeCorrectionMinutes = 0;

      public void Debug()
      {
        if (UseTimeCorrection)
        {
          this.LogDebug("XMLTV: file, location = {0}, time correction = {1:d2}:{2:d2}", FileLocation, TimeCorrectionHours, TimeCorrectionMinutes);
          return;
        }
        this.LogDebug("XMLTV: file, location = {0}, time correction = 00:00", FileLocation);
      }
    }

    #endregion

    #region constants

    private const int TIMEOUT_SCHEDULED_ACTION_DOWNLOAD = 360000;   // 6 minutes
    private const int TIMEOUT_SCHEDULED_ACTION_PROGRAM = 600000;    // 10 minutes
    private const int RETRY_DELAY_SCHEDULED_ACTIONS = 30000;        // 30 seconds

    #endregion

    #region variables

    private static object _lockScheduledActionsAndImport = new object();
    private static object _lockScheduledActionsStatus = new object();
    private static object _lockImportStatus = new object();

    private bool _registeredForPowerEvents = false;
    private bool _registeredForPowerSchedulerTrigger = false;
    private System.Timers.Timer _timer = null;

    #endregion

    #region ITvServerPlugin members

    /// <summary>
    /// The name of this TV Server plugin.
    /// </summary>
    public string Name
    {
      get
      {
        return "XMLTV EPG";
      }
    }

    /// <summary>
    /// The version of this TV Server plugin.
    /// </summary>
    public string Version
    {
      get
      {
        return "1.1.0.0";
      }
    }

    /// <summary>
    /// The author of this TV Server plugin.
    /// </summary>
    public string Author
    {
      get
      {
        return "Frodo, mm1352000";
      }
    }

    /// <summary>
    /// Determine whether this TV Server plugin should only run on the master server, or if it can also
    /// run on slave servers.
    /// </summary>
    /// <remarks>
    /// This property is obsolete. Master-slave configurations are not supported.
    /// </remarks>
    public bool MasterOnly
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Get an instance of the configuration section for use in TV Server configuration (SetupTv).
    /// </summary>
    public SectionSettings Setup
    {
      get
      {
        return new XmlTvSetup();
      }
    }

    /// <summary>
    /// Start this TV Server plugin.
    /// </summary>
    public void Start(IInternalControllerService controllerService)
    {
      this.LogDebug("XMLTV: started");

      RegisterPowerEventHandler();
      PerformScheduledStartOrResumeActionsInThread();

      _timer = new System.Timers.Timer();
      _timer.Interval = 60000;
      _timer.Enabled = true;
      _timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);
    }

    /// <summary>
    /// Stop this TV Server plugin.
    /// </summary>
    public void Stop()
    {
      this.LogDebug("XMLTV: stopped");

      UnRegisterPowerEventHandler();
      UnRegisterPowerSchedulerEpgHandler();

      if (_timer != null)
      {
        _timer.Enabled = false;
        _timer.Stop();
        _timer.Dispose();
        _timer = null;
      }
    }

    #endregion

    #region power event handling

    private void RegisterPowerEventHandler()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerEventHandler>())
      {
        GlobalServiceProvider.Instance.Get<IPowerEventHandler>().AddPowerEventHandler(new PowerEventHandler(OnPowerEvent));
        this.LogDebug("XMLTV: registered for power events");
        _registeredForPowerEvents = true;
      }
      else
      {
        this.LogWarn("XMLTV: failed to register for power events");
      }
    }

    private void UnRegisterPowerEventHandler()
    {
      if (!_registeredForPowerEvents)
      {
        return;
      }
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerEventHandler>())
      {
        GlobalServiceProvider.Instance.Get<IPowerEventHandler>().RemovePowerEventHandler(new PowerEventHandler(OnPowerEvent));
        this.LogDebug("XMLTV: unregistered for power events");
      }
      else
      {
        this.LogError("XMLTV: failed to unregister for power events");
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private bool OnPowerEvent(PowerEventType powerStatus)
    {
      switch (powerStatus)
      {
        case PowerEventType.ResumeAutomatic:
        case PowerEventType.ResumeCritical:
        case PowerEventType.ResumeSuspend:
          // ResumeSuspend may not be broadcast unless the user has triggered
          // resume, so we handle ResumeAutomatic and ResumeCritical as well.
          this.LogDebug("XMLTV: resumed, status = {0}", powerStatus);
          PerformScheduledStartOrResumeActionsInThread();
          break;
      }
      return true;
    }

    #endregion

    #region PowerScheduler integration

    private void RegisterPowerSchedulerEpgHandler()
    {
      // PowerScheduler can be configured to wake the system and trigger EPG
      // grabbing. Register for that notification.
      if (GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        IEpgHandler handler = GlobalServiceProvider.Instance.Get<IEpgHandler>();
        handler.EPGScheduleDue += new EPGScheduleHandler(PowerSchedulerEpgGrab);
        this.LogDebug("XMLTV: registered as PowerScheduler EPG handler");
        _registeredForPowerSchedulerTrigger = true;
      }
    }

    private void UnRegisterPowerSchedulerEpgHandler()
    {
      if (!_registeredForPowerSchedulerTrigger)
      {
        return;
      }

      if (GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        IEpgHandler handler = GlobalServiceProvider.Instance.Get<IEpgHandler>();
        handler.EPGScheduleDue -= new EPGScheduleHandler(PowerSchedulerEpgGrab);
        this.LogDebug("XMLTV: unregistered as PowerScheduler EPG handler");
      }
    }

    private void SetStandbyAllowed(bool allowed)
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        this.LogDebug("XMLTV: update standby permission, allowed = {0}, timeout = 1 hour", allowed);
        GlobalServiceProvider.Instance.Get<IEpgHandler>().SetStandbyAllowed(this, allowed, 3600);
      }
    }

    #endregion

    #region action/import triggers

    private void PowerSchedulerEpgGrab()
    {
      this.LogInfo("XMLTV: PowerScheduler triggered EPG grab/update");
      OnTimerElapsed(null, null);
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
      // Don't do anything if the scheduled actions or import are currently running.
      if (!Monitor.TryEnter(_lockScheduledActionsAndImport, 5000))
      {
        return;
      }

      try
      {
        TimeSpan timeSincePreviousFinishSuccess = DateTime.Now - SettingsManagement.GetValue("xmlTvScheduledActionsPreviousFinish", DateTime.MinValue);
        if (SettingsManagement.GetValue("xmlTvScheduledActionsOnStartup", false))
        {
          // If configured to run on startup/resume, don't run the scheduled
          // actions unless it has been at least a day since they last ran.
          if (timeSincePreviousFinishSuccess.TotalMinutes >= 1440)  // 1440 minutes = 1 day
          {
            this.LogInfo("XMLTV: time to perform scheduled actions (start/resume), {0} minutes since last run", timeSincePreviousFinishSuccess.TotalMinutes);
            PerformScheduledActions();
          }
        }
        else
        {
          // If configured to run at a fixed time each day, don't run the
          // scheduled actions unless it is time to run them (10 minute window)
          // and it has been at least half a day since they last ran.
          DateTime scheduledTime = SettingsManagement.GetValue("xmlTvScheduledActionsTime", DateTime.Now);
          DateTime scheduledTimeToday = DateTime.Today;
          scheduledTimeToday.AddHours(scheduledTime.Hour);
          scheduledTimeToday.AddMinutes(scheduledTime.Minute);
          TimeSpan timeUntilRunToday = DateTime.Now - scheduledTimeToday;
          if (timeSincePreviousFinishSuccess.TotalMinutes >= 720 && Math.Abs(timeUntilRunToday.TotalMinutes) <= 5)
          {
            this.LogInfo("XMLTV: time to perform scheduled actions (fixed time {0}), {1} minutes since last run", scheduledTime.TimeOfDay, timeSincePreviousFinishSuccess.TotalMinutes);
            PerformScheduledActions();
          }
        }

        PerformImport(true);
      }
      catch (Exception ex)
      {
        // The functions that perform the scheduled actions and update the
        // guide should catch their own exceptions. This is not expected.
        this.LogError(ex, "XMLTV: failed to perform timing checks");
      }
      finally
      {
        Monitor.Exit(_lockScheduledActionsAndImport);
      }
    }

    #endregion

    #region channel mapping

    internal IDictionary<string, IDictionary<string, IList<string>>> ReadChannelsFromAllDataFiles()
    {
      this.LogDebug("XMLTV: read channels from all data files");
      IDictionary<string, IDictionary<string, IList<string>>> fileChannels = new Dictionary<string, IDictionary<string, IList<string>>>(5);

      // Don't attempt to open files while the scheduled actions are running.
      if (!Monitor.TryEnter(_lockScheduledActionsAndImport, 5000))
      {
        this.LogWarn("XMLTV: can't read channel names while scheduled actions are running");
        return fileChannels;
      }

      try
      {
        ICollection<XmlTvFileSettings> files = ReadImportSettings();
        foreach (XmlTvFileSettings file in files)
        {
          file.Debug();
          fileChannels.Add(Path.GetFileNameWithoutExtension(file.FileLocation), ReadChannelsFromFile(file.FileLocation));
        }
        this.LogDebug("XMLTV: file count = {0}, channel count = {1}", files.Count, fileChannels.Count);
        return fileChannels;
      }
      finally
      {
        Monitor.Exit(_lockScheduledActionsAndImport);
      }
    }

    private IDictionary<string, IList<string>> ReadChannelsFromFile(string fileName)
    {
      IDictionary<string, IList<string>> channels = new Dictionary<string, IList<string>>(100);
      try
      {
        using (XmlTextReader xmlReader = new XmlTextReader(fileName))
        {
          xmlReader.DtdProcessing = DtdProcessing.Ignore;
          if (xmlReader.ReadToDescendant("tv") && xmlReader.ReadToDescendant("channel"))
          {
            do
            {
              string id = xmlReader.GetAttribute("id");
              if (string.IsNullOrEmpty(id))
              {
                this.LogWarn("XMLTV: found channel without ID in file \"{0}\"", fileName);
              }
              else
              {
                XmlReader xmlChannel = xmlReader.ReadSubtree();
                xmlChannel.ReadStartElement();
                List<string> displayNames = new List<string>(5);
                while (!xmlChannel.EOF)
                {
                  if (xmlChannel.NodeType == XmlNodeType.Element)
                  {
                    if (xmlChannel.Name.ToLowerInvariant().Equals("display-name"))
                    {
                      displayNames.Add(xmlChannel.ReadString());
                    }
                    else
                    {
                      xmlChannel.Skip();
                    }
                  }
                  else
                  {
                    xmlChannel.Read();
                  }
                }
                channels.Add(id, displayNames);
              }
            }
            while (xmlReader.ReadToNextSibling("channel"));
          }

          xmlReader.Close();
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "XMLTV: failed to read channels from file \"{0}\"", fileName);
      }
      return channels;
    }

    #endregion

    #region scheduled actions

    private void PerformScheduledStartOrResumeActionsInThread()
    {
      ThreadPool.QueueUserWorkItem(
        delegate
        {
          this.LogDebug("XMLTV: perform scheduled actions at start/resume");
          try
          {
            lock (_lockScheduledActionsAndImport)
            {
              if (!SettingsManagement.GetValue("xmlTvScheduledActionsOnStartup", false))
              {
                this.LogDebug("XMLTV: actions configured for fixed time");
                return;
              }

              TimeSpan ts = DateTime.Now - SettingsManagement.GetValue("xmlTvScheduledActionsPreviousFinish", DateTime.MinValue);
              if (ts.TotalMinutes < 1440) // 1440 minutes = 1 day
              {
                this.LogDebug("XMLTV: not time to perform scheduled actions, {0} minutes since last run", ts.TotalMinutes);
                return;
              }
              PerformScheduledActions();
            }
          }
          catch (Exception ex)
          {
            this.LogError(ex, "XMLTV: failed to perform scheduled actions at start/resume");
          }
        }
      );
    }

    internal void PerformScheduledActions()
    {
      this.LogInfo("XMLTV: perform scheduled actions");

      try
      {
        SetStandbyAllowed(false);
        lock (_lockScheduledActionsAndImport)
        {
          if (SettingsManagement.GetValue("xmlTvScheduledActionsDownload", false) && !PerformDownloadFile())
          {
            return;
          }
          if (SettingsManagement.GetValue("xmlTvScheduledActionsProgram", false) && !PerformExecuteProgram())
          {
            return;
          }
          SettingsManagement.SaveValue("xmlTvScheduledActionsPreviousFinish", DateTime.Now);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "XMLTV: failed to perform scheduled actions");
      }
      finally
      {
        SetStandbyAllowed(true);
      }
    }

    private void UpdateScheduledActionStatus(string status)
    {
      lock (_lockScheduledActionsStatus)
      {
        SettingsManagement.SaveValue("xmlTvScheduledActionsStatusDateTime", DateTime.Now);
        SettingsManagement.SaveValue("xmlTvScheduledActionsStatus", status);
      }
    }

    internal void ReadScheduledActionStatus(out DateTime dateTime, out string status)
    {
      lock (_lockScheduledActionsStatus)
      {
        dateTime = SettingsManagement.GetValue("xmlTvScheduledActionsStatusDateTime", DateTime.Now);
        status = SettingsManagement.GetValue("xmlTvScheduledActionsStatus", string.Empty);
      }
    }

    #region file download and extraction

    private bool PerformDownloadFile()
    {
      this.LogInfo("XMLTV: perform download file");

      UpdateScheduledActionStatus("download pre-processing");
      string url = SettingsManagement.GetValue("xmlTvScheduledActionsDownloadUrl", "http://www.mysite.com/tvguide.xml");
      if (string.IsNullOrWhiteSpace(url))
      {
        this.LogError("XMLTV: download enabled but URL not specified");
        UpdateScheduledActionStatus("URL not specified");
        return false;
      }
      string outputFolder = SettingsManagement.GetValue("xmlTvFolder", string.Empty);
      if (string.IsNullOrWhiteSpace(outputFolder) || !Directory.Exists(outputFolder))
      {
        this.LogError("XMLTV: output folder for download \"{0}\" is not valid or does not exist", outputFolder ?? "[null]");
        UpdateScheduledActionStatus("invalid output folder");
        return false;
      }
      this.LogDebug("XMLTV: URL = {0}, output folder = {1}", url, outputFolder);

      string userName = null;
      string password = null;
      if (url.ToLowerInvariant().StartsWith("ftp://"))
      {
        // Extract credentials from the URL (optional).
        // ftp://user:pass@www.somesite.com/TVguide.xml
        int indexPasswordEnd = url.IndexOf("@");
        if (indexPasswordEnd > -1)
        {
          int indexUserNameStart = 6;   // skip "ftp://"
          int indexUserNameEnd = url.IndexOf(":", indexUserNameStart);

          userName = url.Substring(indexUserNameStart, (indexUserNameEnd - indexUserNameStart));
          password = url.Substring(indexUserNameEnd + 1, (indexPasswordEnd - indexUserNameEnd - 1));
          url = "ftp://" + url.Substring(indexPasswordEnd + 1);
          this.LogDebug("XMLTV: FTP with authentication, user name = {0}, password = {1}, URL = {2}", userName, password, url);
        }
      }

      Uri uri = new Uri(url);
      FileInfo fileInfo = new FileInfo(uri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped).ToLowerInvariant().Trim());
      string outputFileName = fileInfo.Name;
      bool isZipFile = string.Equals(fileInfo.Extension, ".zip");
      if (isZipFile)
      {
        outputFileName = Path.Combine(outputFolder, outputFileName);
      }
      else
      {
        outputFileName = Path.Combine(outputFolder, "tvguide.xml");
      }

      if (!DownloadFile(uri, userName, password, outputFileName))
      {
        return false;
      }
      if (isZipFile && !ExtractZip(outputFileName, outputFolder))
      {
        return false;
      }
      UpdateScheduledActionStatus("download successful");
      return true;
    }

    private bool DownloadFile(Uri uri, string userName, string password, string outputFileName)
    {
      this.LogDebug("XMLTV: starting download, output file name = {0}", outputFileName);
      UpdateScheduledActionStatus("starting download");
      using (var downloader = new FileDownloader(TIMEOUT_SCHEDULED_ACTION_DOWNLOAD))
      {
        if (userName != null)
        {
          downloader.Credentials = new NetworkCredential(userName, password);
          downloader.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }

        int retryNumber = 0;
        while (retryNumber < 10)
        {
          if (retryNumber > 0)
          {
            Thread.Sleep(RETRY_DELAY_SCHEDULED_ACTIONS);
            UpdateScheduledActionStatus(string.Format("download retry {0}", retryNumber));
          }
          try
          {
            downloader.DownloadFile(uri, outputFileName);
            this.LogInfo("XMLTV: download completed successfully");
            UpdateScheduledActionStatus("download completed");
            return true;
          }
          catch (Exception ex)
          {
            this.LogError(ex, "XMLTV: failed to download and save file, retry = {0}, URI = {1}", retryNumber, uri);
          }
          retryNumber++;
        }
      }

      UpdateScheduledActionStatus("download failed, check error log for details");
      return false;
    }

    private bool ExtractZip(string fileName, string outputFolder)
    {
      this.LogDebug("XMLTV: starting extraction");
      UpdateScheduledActionStatus("starting extraction");
      int retryNumber = 0;
      while (retryNumber < 10)
      {
        if (retryNumber > 0)
        {
          Thread.Sleep(RETRY_DELAY_SCHEDULED_ACTIONS);
          UpdateScheduledActionStatus(string.Format("extract retry {0}", retryNumber));
        }
        try
        {
          ZipFile zipFile = new ZipFile(fileName);
          try
          {
            zipFile.ExtractAll(outputFolder, ExtractExistingFileAction.OverwriteSilently);
            this.LogInfo("XMLTV: extraction completed successfully");
            UpdateScheduledActionStatus("completed extraction");
            return true;
          }
          finally
          {
            zipFile.Dispose();
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex, "XMLTV: failed to extract downloaded zip file, retry = {0}, file = {1}, output folder = {2}", retryNumber, fileName, outputFolder);
        }
        retryNumber++;
      }

      UpdateScheduledActionStatus("extraction failed, check error log for details");
      return false;
    }

    #endregion

    private bool PerformExecuteProgram()
    {
      this.LogInfo("XMLTV: perform execute program");

      string programFile = SettingsManagement.GetValue("xmlTvScheduledActionsProgramLocation", @"c:\Program Files\My Program\MyProgram.exe");
      if (string.IsNullOrWhiteSpace(programFile) || !File.Exists(programFile))
      {
        this.LogError("XMLTV: program to execute \"{0}\" is not valid or does not exist", programFile ?? "[null]");
        UpdateScheduledActionStatus("invalid program file");
        return false;
      }

      this.LogDebug("XMLTV: starting execution, program = {0}", programFile);
      UpdateScheduledActionStatus("starting program");
      int retryNumber = 0;
      while (retryNumber < 10)
      {
        if (retryNumber > 0)
        {
          Thread.Sleep(RETRY_DELAY_SCHEDULED_ACTIONS);
          UpdateScheduledActionStatus(string.Format("program retry {0}", retryNumber));
        }
        try
        {
          Process p = Process.Start(programFile);
          if (p.WaitForExit(TIMEOUT_SCHEDULED_ACTION_PROGRAM))
          {
            this.LogInfo("XMLTV: program completed, exit code = {0}", p.ExitCode);
            if (p.ExitCode == 0)
            {
              UpdateScheduledActionStatus(string.Format("program completed, success", p.ExitCode));
              return true;
            }
            UpdateScheduledActionStatus(string.Format("program completed, failed (exit code {0})", p.ExitCode));
          }
          else
          {
            UpdateScheduledActionStatus(string.Format("program timeout, not completed after {0} seconds", TIMEOUT_SCHEDULED_ACTION_PROGRAM / 1000));
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex, "XMLTV: failed to execute program, retry = {0}, program = {1}", retryNumber, programFile);
        }
        retryNumber++;
      }

      UpdateScheduledActionStatus("program failed, check error log for details");
      return false;
    }

    #endregion

    #region import

    internal void PerformImport(bool checkForNewData = true)
    {
      // Don't do anything if the scheduled actions or import are currently
      // running. This is here to handle forced imports only.
      if (!Monitor.TryEnter(_lockScheduledActionsAndImport, 5000))
      {
        return;
      }

      try
      {
        ICollection<XmlTvFileSettings> settings = ReadImportSettings();
        if (settings == null || (checkForNewData && !HaveNewData(settings)))
        {
          return;
        }
        if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>() &&
            GlobalServiceProvider.Instance.Get<IPowerScheduler>().IsSuspendInProgress())
        {
          this.LogWarn("XMLTV: import prevented by suspend in progress");
          return;
        }

        if (checkForNewData)
        {
          this.LogInfo("XMLTV: detected new data, starting import");
        }
        else
        {
          this.LogInfo("XMLTV: starting forced import");
        }

        XMLTVImport.ImportStats stats = new XMLTVImport.ImportStats();
        UpdateImportStatus("starting import", stats);
        DateTime importDate = DateTime.Now;
        SetStandbyAllowed(false);

        try
        {
          bool deleteBeforeImport = SettingsManagement.GetValue("xmlTvDeleteBeforeImport", true);

          foreach (XmlTvFileSettings file in settings)
          {
            try
            {
              file.Debug();
              XMLTVImport importer = new XMLTVImport(10);

              importer.Import(file.FileLocation, deleteBeforeImport, UpdateImportStatus,
                              file.UseTimeCorrection, file.TimeCorrectionHours, file.TimeCorrectionMinutes,
                              ref stats);

              this.LogDebug("XMLTV: finished importing \"{0}\", [file/unmapped/DB] channel count = {1}/{2}/{3}, program count = {4}/{5}/{6}", file.FileLocation,
                            stats.FileChannelCount, stats.FileChannelCountUnmapped, stats.FileChannelCountDb,
                            stats.FileProgramCount, stats.FileProgramCountUnmapped, stats.FileProgramCountDb);
              stats.ResetFileStats();
            }
            catch (Exception ex)
            {
              this.LogError(ex, "XMLTV: failed to import from file \"{0}\"", file.FileLocation);
            }
          }

          UpdateImportStatus("waiting for database import to complete", stats);
          SettingsManagement.SaveValue("xmlTvImportPreviousDateTime", importDate);
          ProgramManagement.InitiateInsertPrograms();

          this.LogInfo("XMLTV: import completed, [file/unmapped/DB] channel count = {0}/{1}/{2}, program count = {3}/{4}/{5}",
                        stats.TotalChannelCountFiles, stats.TotalChannelCountFilesUnmapped, stats.TotalChannelCountDb,
                        stats.TotalProgramCountFiles, stats.TotalProgramCountFilesUnmapped, stats.TotalProgramCountDb);
          if (stats.TotalProgramCountDb > 0)
          {
            UpdateImportStatus("import completed, success", stats);
          }
          else if (stats.TotalChannelCountFiles == 0)
          {
            UpdateImportStatus("import completed, no channels in data file(s)", stats);
          }
          else if (stats.TotalProgramCountFiles == 0)
          {
            UpdateImportStatus("import completed, no programs in data file(s)", stats);
          }
          else if (stats.TotalChannelCountFilesUnmapped == stats.TotalChannelCountFiles)
          {
            UpdateImportStatus("import completed, channels not mapped", stats);
          }
          else
          {
            UpdateImportStatus("import completed, old data", stats);
          }
        }
        finally
        {
          SetStandbyAllowed(true);
        }
      }
      finally
      {
        Monitor.Exit(_lockScheduledActionsAndImport);
      }
    }

    private void UpdateImportStatus(string status, XMLTVImport.ImportStats stats = null)
    {
      lock (_lockImportStatus)
      {
        SettingsManagement.SaveValue("xmlTvImportStatusDateTime", DateTime.Now);
        SettingsManagement.SaveValue("xmlTvImportStatus", status);
        if (stats != null)
        {
          SettingsManagement.SaveValue("xmlTvImportStatusChannelCounts", stats.GetTotalChannelDescription());
          SettingsManagement.SaveValue("xmlTvImportStatusProgramCounts", stats.GetTotalProgramDescription());
        }
      }
    }

    internal void ReadImportStatus(out DateTime dateTime, out string status, out string channelCounts, out string programCounts)
    {
      lock (_lockImportStatus)
      {
        dateTime = SettingsManagement.GetValue("xmlTvImportStatusDateTime", DateTime.Now);
        status = SettingsManagement.GetValue("xmlTvImportStatus", string.Empty);
        channelCounts = SettingsManagement.GetValue("xmlTvImportStatusChannelCounts", string.Empty);
        programCounts = SettingsManagement.GetValue("xmlTvImportStatusProgramCounts", string.Empty);
      }
    }

    private ICollection<XmlTvFileSettings> ReadImportSettings()
    {
      string folder = SettingsManagement.GetValue("xmlTvFolder", string.Empty);
      bool configured = !string.IsNullOrWhiteSpace(folder);
      if (!configured || !Directory.Exists(folder))
      {
        if (configured)     // avoid verbose logging
        {
          this.LogError("XMLTV: import folder \"{0}\" is not valid or does not exist", folder ?? "[null]");
        }
        UpdateImportStatus("invalid import folder");
        return null;
      }

      string fileName = Path.Combine(folder, "tvguide.lst");
      if (!File.Exists(fileName))
      {
        fileName = Path.Combine(folder, "tvguide.xml");
        if (!File.Exists(fileName))
        {
          if (!string.Equals(folder, string.Empty))  // avoid verbose logging
          {
            this.LogError("XMLTV: failed to find file to import in folder \"{0}\"", folder);
          }
          UpdateImportStatus("no files to import");
          return null;
        }

        XmlTvFileSettings settings = new XmlTvFileSettings();
        settings.FileLocation = fileName;
        settings.UseTimeCorrection = SettingsManagement.GetValue("xmlTvUseTimeCorrection", false);
        settings.TimeCorrectionHours = SettingsManagement.GetValue("xmlTvTimeCorrectionHours", 0);
        settings.TimeCorrectionMinutes = SettingsManagement.GetValue("xmlTvTimeCorrectionMins", 0);
        return new List<XmlTvFileSettings> { settings };
      }

      try
      {
        string[] lines = File.ReadAllLines(fileName);
        List<XmlTvFileSettings> settings = new List<XmlTvFileSettings>(lines.Length);
        foreach (string line in lines)
        {
          if (string.IsNullOrWhiteSpace(line))
          {
            continue;
          }

          XmlTvFileSettings s = new XmlTvFileSettings();
          string[] parts = line.Split('|');
          if (parts.Length >= 3)
          {
            s.UseTimeCorrection = int.TryParse(parts[1].Trim(), out s.TimeCorrectionHours) && int.TryParse(parts[2].Trim(), out s.TimeCorrectionMinutes);
            if (!s.UseTimeCorrection)
            {
              this.LogWarn("XMLTV: failed to parse time corrections from list file line \"{0}\"", line);
              s.TimeCorrectionHours = 0;
              s.TimeCorrectionMinutes = 0;
            }
          }

          s.FileLocation = parts[0].Trim();
          if (!Path.IsPathRooted(s.FileLocation))
          {
            s.FileLocation = Path.Combine(folder, s.FileLocation);
          }
          if (!File.Exists(s.FileLocation))
          {
            this.LogWarn("XMLTV: file \"{0}\" extracted from list file line \"{1}\" does not exist", s.FileLocation, line);
            continue;
          }
          settings.Add(s);
        }
        return settings;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "XMLTV: failed to parse list file \"{0}\"", fileName);
        return null;
      }
    }

    private bool HaveNewData(ICollection<XmlTvFileSettings> files)
    {
      DateTime previousImportStarted = SettingsManagement.GetValue("xmlTvImportPreviousDateTime", DateTime.MinValue);
      bool newData = false;

      foreach (XmlTvFileSettings file in files)
      {
        try
        {
          if (!CanReadFile(file.FileLocation))
          {
            this.LogWarn("XMLTV: not able to read file \"{0}\", assuming update running now", file.FileLocation);
            return false;
          }
          if (File.GetLastWriteTime(file.FileLocation) > previousImportStarted)
          {
            newData = true;
          }
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "XMLTV: failed to check freshness for data file \"{0}\"", file.FileLocation);
        }
      }
      if (newData)
      {
        return true;
      }

      // Don't forget to check the list file itself. Files may have been added
      // or removed from the list.
      string listFile = string.Empty;
      try
      {
        listFile = Path.Combine(SettingsManagement.GetValue("xmlTvFolder", string.Empty), "tvguide.lst");
        if (File.Exists(listFile) && File.GetLastWriteTime(listFile) > previousImportStarted)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "XMLTV: failed to check freshness for list file \"{0}\"", listFile);
      }

      return false;
    }

    private static bool CanReadFile(string fileName)
    {
      try
      {
        using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          using (var streamReader = new StreamReader(fileStream, Encoding.Default, true))
          {
            return true;
          }
        }
      }
      catch
      {
        return false;
      }
    }

    #endregion

    #region ITvServerPluginStartedAll member

    public void StartedAll()
    {
      RegisterPowerSchedulerEpgHandler();
    }

    #endregion

    #region ITvServerPluginCommunication members

    public object GetServiceInstance
    {
      get
      {
        return new XmlTvImportService();
      }
    }

    public Type GetServiceInterfaceForContractType
    {
      get
      {
        return typeof(IXmlTvImportService);
      }
    }

    #endregion
  }
}