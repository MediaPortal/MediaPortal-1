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
using Mediaportal.TV.Server.Plugins.XmlTvImport.Config;
using Mediaportal.TV.Server.Plugins.XmlTvImport.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.XmlTvImport
{
  [Interceptor("PluginExceptionInterceptor")]
  [ComponentProxyBehavior(AdditionalInterfaces = new [] { typeof(ITvServerPluginStartedAll), typeof(ITvServerPluginCommunication) })]
  public class XmlTvImport : ITvServerPlugin, ITvServerPluginStartedAll, ITvServerPluginCommunication
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
          this.LogDebug("XMLTV import: file, location = {0}, time correction = {1:d2}:{2:d2}", FileLocation, TimeCorrectionHours, TimeCorrectionMinutes);
          return;
        }
        this.LogDebug("XMLTV import: file, location = {0}, time correction = 00:00", FileLocation);
      }
    }

    #endregion

    #region constants

    private const int TIME_OUT_SCHEDULED_ACTION_DOWNLOAD = 360000;  // 6 minutes
    private const int TIME_OUT_SCHEDULED_ACTION_PROGRAM = 600000;   // 10 minutes
    private const int RETRY_COUNT_SCHEDULED_ACTIONS = 10;
    private const int RETRY_DELAY_SCHEDULED_ACTIONS = 30000;        // 30 seconds

    private const string SETTING_NAME_IMPORT_PREVIOUS_START = "xmlTvImportPreviousStartDateTime";
    private const string SETTING_NAME_SCHEDULED_ACTIONS_PREVIOUS_FINISH = "xmlTvScheduledActionsPreviousFinishDateTime";

    #endregion

    #region variables

    private static object _lockScheduledActionsAndImport = new object();
    private static object _lockScheduledActionsStatus = new object();
    private static object _lockImportStatus = new object();

    private XmlTvImportService _service = null;
    private Importer _importer = new Importer();
    private bool _isRegisteredForPowerEvents = false;
    private bool _isRegisteredForPowerSchedulerTrigger = false;
    private System.Timers.Timer _timer = null;
    private bool _isScheduledActionsRunning = false;
    private bool _isImportRunning = false;
    private bool _isScheduledActionsAndImportCancelled = false;

    #endregion

    public XmlTvImport()
    {
      _service = new XmlTvImportService();
      XmlTvImportService.Importer = this;
    }

    #region ITvServerPlugin members

    /// <summary>
    /// The name of this TV Server plugin.
    /// </summary>
    public string Name
    {
      get
      {
        return "XMLTV Import";
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
    /// Get an instance of the configuration section for use in TV Server configuration (SetupTv).
    /// </summary>
    public SectionSettings Setup
    {
      get
      {
        return new XmlTvImportConfig();
      }
    }

    /// <summary>
    /// Start this TV Server plugin.
    /// </summary>
    public void Start(IInternalControllerService controllerService)
    {
      this.LogDebug("XMLTV import: start");

      RegisterPowerEventHandler();
      ExecuteScheduledActionsOnStartupOrResumeInThread();

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
      this.LogDebug("XMLTV import: stop");

      CancelScheduledActionsAndImport();
      lock (_lockScheduledActionsAndImport)
      {
        _isScheduledActionsAndImportCancelled = false;

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
    }

    #endregion

    #region power event handling

    private void RegisterPowerEventHandler()
    {
      if (!_isRegisteredForPowerEvents)
      {
        if (GlobalServiceProvider.Instance.IsRegistered<IPowerEventHandler>())
        {
          IPowerEventHandler handler = GlobalServiceProvider.Instance.Get<IPowerEventHandler>();
          if (handler != null)
          {
            handler.AddPowerEventHandler(new PowerEventHandler(OnPowerEvent));
            this.LogDebug("XMLTV import: registered for power events");
            _isRegisteredForPowerEvents = true;
          }
        }
        else if (_isRegisteredForPowerEvents)
        {
          this.LogWarn("XMLTV import: failed to register for power events");
        }
      }
    }

    private void UnRegisterPowerEventHandler()
    {
      if (!_isRegisteredForPowerEvents)
      {
        if (GlobalServiceProvider.Instance.IsRegistered<IPowerEventHandler>())
        {
          IPowerEventHandler handler = GlobalServiceProvider.Instance.Get<IPowerEventHandler>();
          if (handler != null)
          {
            handler.RemovePowerEventHandler(new PowerEventHandler(OnPowerEvent));
            this.LogDebug("XMLTV import: unregistered for power events");
            _isRegisteredForPowerEvents = false;
          }
        }
        if (_isRegisteredForPowerEvents)
        {
          this.LogError("XMLTV import: failed to unregister for power events");
        }
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
          this.LogDebug("XMLTV import: resumed, status = {0}", powerStatus);
          ExecuteScheduledActionsOnStartupOrResumeInThread();
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
      if (!_isRegisteredForPowerSchedulerTrigger && GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        IEpgHandler handler = GlobalServiceProvider.Instance.Get<IEpgHandler>();
        if (handler != null)
        {
          handler.EPGScheduleDue += new EPGScheduleHandler(PowerSchedulerEpgGrab);
          this.LogDebug("XMLTV import: registered as PowerScheduler EPG handler");
          _isRegisteredForPowerSchedulerTrigger = true;
        }
      }
    }

    private void UnRegisterPowerSchedulerEpgHandler()
    {
      if (_isRegisteredForPowerSchedulerTrigger && GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        IEpgHandler handler = GlobalServiceProvider.Instance.Get<IEpgHandler>();
        if (handler != null)
        {
          handler.EPGScheduleDue -= new EPGScheduleHandler(PowerSchedulerEpgGrab);
          this.LogDebug("XMLTV import: unregistered as PowerScheduler EPG handler");
          _isRegisteredForPowerSchedulerTrigger = false;
        }
      }
    }

    private void SetStandbyAllowed(bool allowed)
    {
      if (_isRegisteredForPowerSchedulerTrigger && GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        IEpgHandler handler = GlobalServiceProvider.Instance.Get<IEpgHandler>();
        if (handler != null)
        {
          this.LogDebug("XMLTV import: update standby permission, allowed = {0}, timeout = 1 hour", allowed);
          handler.SetStandbyAllowed(this, allowed, 3600);
        }
      }
    }

    #endregion

    #region action/import triggers

    private void PowerSchedulerEpgGrab()
    {
      this.LogInfo("XMLTV import: PowerScheduler triggered EPG grab/update");
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
        ExecuteScheduledActions(true);
        if (_isScheduledActionsAndImportCancelled)
        {
          return;
        }
        ImportData(true);
      }
      catch (Exception ex)
      {
        // The functions that execute the scheduled actions and import the data
        // should catch their own exceptions. This is not expected.
        this.LogError(ex, "XMLTV import: failed to execute scheduled actions or import");
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
      this.LogDebug("XMLTV import: read channels from all data files");
      IDictionary<string, IDictionary<string, IList<string>>> fileChannels = new Dictionary<string, IDictionary<string, IList<string>>>(5);

      // Don't attempt to open files while the scheduled actions are running.
      if (!Monitor.TryEnter(_lockScheduledActionsAndImport, 5000))
      {
        this.LogWarn("XMLTV import: can't read channel names while scheduled actions are running");
        return fileChannels;
      }

      try
      {
        ICollection<XmlTvFileSettings> files = ReadImportSettings();
        if (files == null)
        {
          files = new List<XmlTvFileSettings>(0);
        }
        foreach (XmlTvFileSettings file in files)
        {
          file.Debug();
          fileChannels.Add(Path.GetFileNameWithoutExtension(file.FileLocation), ReadChannelsFromFile(file.FileLocation));
        }
        this.LogDebug("XMLTV import: file count = {0}, channel count = {1}", files.Count, fileChannels.Count);
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
                this.LogWarn("XMLTV import: found channel without ID in file \"{0}\"", fileName);
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
        this.LogError(ex, "XMLTV import: failed to read channels from file \"{0}\"", fileName);
      }
      return channels;
    }

    #endregion

    private void CancelScheduledActionsAndImport()
    {
      if (_isScheduledActionsRunning || _isImportRunning)
      {
        this.LogInfo("XMLTV import: cancelling import...");
      }
      _importer.CancelImport();
      _isScheduledActionsAndImportCancelled = true;
    }

    #region scheduled actions

    private void ExecuteScheduledActionsOnStartupOrResumeInThread()
    {
      ThreadPool.QueueUserWorkItem(
        delegate
        {
          this.LogDebug("XMLTV import: execute scheduled actions on startup/resume");
          try
          {
            lock (_lockScheduledActionsAndImport)
            {
              ExecuteScheduledActions(true);
            }
          }
          catch (Exception ex)
          {
            this.LogError(ex, "XMLTV import: failed to execute scheduled actions on startup/resume");
          }
        }
      );
    }

    internal void ExecuteScheduledActions(bool checkTiming = true)
    {
      // Don't do anything if the scheduled actions or import are currently
      // running. This is here to handle forced actions only.
      if (!Monitor.TryEnter(_lockScheduledActionsAndImport, 5000))
      {
        return;
      }

      _isScheduledActionsRunning = true;
      try
      {
        int minutesBetweenScheduledActions = 60 * SettingsManagement.GetValue(XmlTvImportSetting.ScheduledActionsTimeFrequency, 24);
        TimeSpan timeSincePreviousScheduledActionsFinish = DateTime.Now - SettingsManagement.GetValue(SETTING_NAME_SCHEDULED_ACTIONS_PREVIOUS_FINISH, DateTime.MinValue);
        if (!checkTiming)
        {
          this.LogInfo("XMLTV import: starting forced actions, frequency = {0} minutes, time since last run = {1}", minutesBetweenScheduledActions, (long)timeSincePreviousScheduledActionsFinish.TotalMinutes);
        }
        else
        {
          if (timeSincePreviousScheduledActionsFinish.TotalMinutes < minutesBetweenScheduledActions)
          {
            return;
          }
          if (SettingsManagement.GetValue(XmlTvImportSetting.ScheduledActionsTimeOnStartup, false))
          {
            // On startup/resume (or anytime while awake)...
            this.LogInfo("XMLTV import: time to execute scheduled actions (startup/resume), frequency = {0} minutes, time since last run = {1}", minutesBetweenScheduledActions, (long)timeSincePreviousScheduledActionsFinish.TotalMinutes);
          }
          else
          {
            // Only between certain times...
            TimeSpan scheduledTimeStart = SettingsManagement.GetValue(XmlTvImportSetting.ScheduledActionsTimeBetweenStart, DateTime.Now).TimeOfDay;
            TimeSpan scheduledTimeEnd = SettingsManagement.GetValue(XmlTvImportSetting.ScheduledActionsTimeBetweenEnd, DateTime.Now).TimeOfDay;
            if (DateTime.Now.TimeOfDay < scheduledTimeStart || DateTime.Now.TimeOfDay > scheduledTimeEnd)
            {
              return;
            }
            this.LogInfo("XMLTV import: time to execute scheduled actions (between {0} and {1}), frequency = {2} minutes, time since last run = {3}", scheduledTimeStart, scheduledTimeEnd, minutesBetweenScheduledActions, (long)timeSincePreviousScheduledActionsFinish.TotalMinutes);
          }
        }

        if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>() &&
            GlobalServiceProvider.Instance.Get<IPowerScheduler>().IsSuspendInProgress())
        {
          this.LogWarn("XMLTV import: scheduled actions prevented by suspend in progress");
          return;
        }

        SetStandbyAllowed(false);
        try
        {
          if (SettingsManagement.GetValue(XmlTvImportSetting.ScheduledActionsDownload, false) && !DownloadAndExtractFile())
          {
            return;
          }
          if (SettingsManagement.GetValue(XmlTvImportSetting.ScheduledActionsProgram, false) && !ExecuteProgram())
          {
            return;
          }

          // Give a little time for files to be flushed etc. Avoids duplicate
          // imports when file write timestamps are within the same second as
          // the import timestamp.
          Thread.Sleep(10000);

          SettingsManagement.SaveValue(SETTING_NAME_SCHEDULED_ACTIONS_PREVIOUS_FINISH, DateTime.Now);
        }
        finally
        {
          SetStandbyAllowed(true);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "XMLTV import: failed to execute scheduled actions");
      }
      finally
      {
        _isScheduledActionsRunning = false;
        Monitor.Exit(_lockScheduledActionsAndImport);
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

    private bool DownloadAndExtractFile()
    {
      this.LogDebug("XMLTV import: download and extract file");

      UpdateScheduledActionStatus("download pre-processing");
      string url = SettingsManagement.GetValue(XmlTvImportSetting.ScheduledActionsDownloadUrl, "http://www.mysite.com/tvguide.xml");
      if (string.IsNullOrWhiteSpace(url))
      {
        this.LogError("XMLTV import: download enabled but URL not specified");
        UpdateScheduledActionStatus("download failed, address not specified");
        return false;
      }
      string outputFolder = string.Empty;
      string outputFileName = SettingsManagement.GetValue(XmlTvImportSetting.File, string.Empty);
      if (!string.IsNullOrWhiteSpace(outputFileName))
      {
        outputFolder = Path.GetDirectoryName(outputFileName);
      }
      if (!Directory.Exists(outputFolder))
      {
        this.LogError("XMLTV import: output folder for download \"{0}\" is not valid or does not exist", outputFolder ?? "[null]");
        UpdateScheduledActionStatus("download failed, invalid save folder");
        return false;
      }
      this.LogDebug("XMLTV import: URL = {0}, output folder = {1}", url, outputFolder);

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
          this.LogDebug("XMLTV import: FTP with authentication, user name = {0}, password = {1}, URL = {2}", userName, password, url);
        }
      }

      Uri uri = new Uri(url);
      FileInfo fileInfo = new FileInfo(uri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped).ToLowerInvariant().Trim());
      outputFileName = fileInfo.Name;
      bool isZipFile = string.Equals(fileInfo.Extension, ".zip");
      if (isZipFile)
      {
        outputFileName = Path.Combine(outputFolder, outputFileName);
      }
      else
      {
        outputFileName = Path.Combine(outputFolder, "guide.xml");
      }

      if (_isScheduledActionsAndImportCancelled)
      {
        UpdateScheduledActionStatus("download cancelled");
        return false;
      }

      if (!DownloadFile(uri, userName, password, outputFileName))
      {
        return false;
      }

      if (_isScheduledActionsAndImportCancelled)
      {
        UpdateScheduledActionStatus("extract cancelled");
        return false;
      }

      if (isZipFile && !ExtractZip(outputFileName, outputFolder))
      {
        return false;
      }

      UpdateScheduledActionStatus("download and extract successful");
      return true;
    }

    private bool DownloadFile(Uri uri, string userName, string password, string outputFileName)
    {
      this.LogInfo("XMLTV import: starting download, URL = {0}, output file name = {1}", uri, outputFileName);
      UpdateScheduledActionStatus("starting download");
      using (var downloader = new FileDownloader(TIME_OUT_SCHEDULED_ACTION_DOWNLOAD))
      {
        if (userName != null)
        {
          downloader.Credentials = new NetworkCredential(userName, password);
        }
        downloader.Proxy.Credentials = CredentialCache.DefaultCredentials;

        int retryNumber = 0;
        while (retryNumber < RETRY_COUNT_SCHEDULED_ACTIONS)
        {
          if (retryNumber > 0)
          {
            Thread.Sleep(RETRY_DELAY_SCHEDULED_ACTIONS);
            UpdateScheduledActionStatus(string.Format("download retry {0}", retryNumber));
          }
          try
          {
            downloader.DownloadFile(uri, outputFileName);
            this.LogInfo("XMLTV import: download completed successfully");
            UpdateScheduledActionStatus("download completed");
            return true;
          }
          catch (Exception ex)
          {
            this.LogError(ex, "XMLTV import: failed to download and save file, retry = {0}, URI = {1}", retryNumber, uri);
          }
          retryNumber++;
        }
      }

      UpdateScheduledActionStatus("download failed, check error log for details");
      return false;
    }

    private bool ExtractZip(string fileName, string outputFolder)
    {
      this.LogDebug("XMLTV import: starting extraction");
      UpdateScheduledActionStatus("starting extraction");
      int retryNumber = 0;
      while (retryNumber < RETRY_COUNT_SCHEDULED_ACTIONS)
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
            this.LogInfo("XMLTV import: extraction completed successfully");
            UpdateScheduledActionStatus("extraction completed");
            return true;
          }
          finally
          {
            zipFile.Dispose();
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex, "XMLTV import: failed to extract downloaded zip file, retry = {0}, file = {1}, output folder = {2}", retryNumber, fileName, outputFolder);
        }
        retryNumber++;
      }

      UpdateScheduledActionStatus("extraction failed, check error log for details");
      return false;
    }

    #endregion

    private bool ExecuteProgram()
    {
      this.LogDebug("XMLTV import: execute program");

      string programFile = SettingsManagement.GetValue(XmlTvImportSetting.ScheduledActionsProgramLocation, @"c:\Program Files\My Program\MyProgram.exe");
      if (string.IsNullOrWhiteSpace(programFile) || !File.Exists(programFile))
      {
        this.LogError("XMLTV import: program to execute \"{0}\" is not valid or does not exist", programFile ?? "[null]");
        UpdateScheduledActionStatus("program failed, invalid program file");
        return false;
      }

      this.LogInfo("XMLTV import: starting execution, program = {0}", programFile);
      UpdateScheduledActionStatus("starting program");
      int retryNumber = 0;
      while (retryNumber < RETRY_COUNT_SCHEDULED_ACTIONS)
      {
        if (_isScheduledActionsAndImportCancelled)
        {
          UpdateScheduledActionStatus("program cancelled");
          return false;
        }

        if (retryNumber > 0)
        {
          Thread.Sleep(RETRY_DELAY_SCHEDULED_ACTIONS);
          UpdateScheduledActionStatus(string.Format("program retry {0}", retryNumber));
        }
        try
        {
          ProcessStartInfo startInfo = new ProcessStartInfo();
          startInfo.FileName = programFile;
          startInfo.WorkingDirectory = Path.GetDirectoryName(programFile);
          startInfo.WindowStyle = ProcessWindowStyle.Minimized;
          Process p = Process.Start(startInfo);
          if (p.WaitForExit(TIME_OUT_SCHEDULED_ACTION_PROGRAM))
          {
            if (p.ExitCode == 0)
            {
              this.LogInfo("XMLTV import: program completed");
              UpdateScheduledActionStatus(string.Format("program completed, success", p.ExitCode));
              return true;
            }
            this.LogError("XMLTV import: program \"{0}\" completed with failure exit code {1}", programFile, p.ExitCode);
            UpdateScheduledActionStatus(string.Format("program failed, completed with exit code {0}", p.ExitCode));
          }
          else
          {
            this.LogError("XMLTV import: program \"{0}\" failed to complete after {1} seconds", programFile, TIME_OUT_SCHEDULED_ACTION_PROGRAM);
            UpdateScheduledActionStatus(string.Format("program failed, not completed after {0} seconds", TIME_OUT_SCHEDULED_ACTION_PROGRAM / 1000));
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex, "XMLTV import: failed to execute program, retry = {0}, program = {1}", retryNumber, programFile);
        }
        retryNumber++;
      }

      UpdateScheduledActionStatus("program failed, check error log for details");
      return false;
    }

    #endregion

    #region import

    internal void ImportData(bool checkForNewData = true)
    {
      // Don't do anything if the scheduled actions or import are currently
      // running. This is here to handle forced imports only.
      if (!Monitor.TryEnter(_lockScheduledActionsAndImport, 5000))
      {
        return;
      }

      _isImportRunning = true;
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
          this.LogWarn("XMLTV import: import prevented by suspend in progress");
          return;
        }

        DateTime previousImportStarted = SettingsManagement.GetValue(SETTING_NAME_IMPORT_PREVIOUS_START, DateTime.MinValue);
        if (checkForNewData)
        {
          this.LogInfo("XMLTV import: detected new data, starting import, previous import was {0}", previousImportStarted);
        }
        else
        {
          this.LogInfo("XMLTV import: starting forced import, previous import was {0}", previousImportStarted);
        }

        ImportStats stats = new ImportStats();
        if (_isScheduledActionsAndImportCancelled)
        {
          UpdateImportStatus("import cancelled", stats);
          return;
        }

        UpdateImportStatus("starting import", stats);
        DateTime importDate = DateTime.Now;
        SetStandbyAllowed(false);

        try
        {
          bool success = true;
          foreach (XmlTvFileSettings file in settings)
          {
            try
            {
              file.Debug();

              int timeCorrection = 0;
              if (file.UseTimeCorrection)
              {
                timeCorrection = (60 * file.TimeCorrectionHours) + file.TimeCorrectionMinutes;
              }
              success &= _importer.Import(file.FileLocation, UpdateImportStatus, timeCorrection, ref stats);

              this.LogInfo("XMLTV import: finished importing \"{0}\", [file/unmapped/DB] channel count = {1}/{2}/{3}, program count = {4}/{5}/{6}", file.FileLocation,
                            stats.FileChannelCount, stats.FileChannelCountUnmapped, stats.FileChannelCountDb,
                            stats.FileProgramCount, stats.FileProgramCountUnmapped, stats.FileProgramCountDb);
              stats.ResetFileStats();
            }
            catch (Exception ex)
            {
              this.LogError(ex, "XMLTV import: failed to import from file \"{0}\"", file.FileLocation);
            }

            if (_isScheduledActionsAndImportCancelled)
            {
              UpdateImportStatus("import cancelled", stats);
              return;
            }
          }

          UpdateImportStatus("waiting for database import to complete", stats);
          SettingsManagement.SaveValue(SETTING_NAME_IMPORT_PREVIOUS_START, importDate);
          ProgramManagement.WaitForInsertProgramsToFinish();

          this.LogInfo("XMLTV import: import completed, result = {0}, [file/unmapped/DB] channel count = {1}/{2}/{3}, program count = {4}/{5}/{6}",
                        success, stats.TotalChannelCountFiles, stats.TotalChannelCountFilesUnmapped, stats.TotalChannelCountDb,
                        stats.TotalProgramCountFiles, stats.TotalProgramCountFilesUnmapped, stats.TotalProgramCountDb);
          if (!success)
          {
            UpdateImportStatus("import completed, errors occurred", stats);
          }
          else if (stats.TotalProgramCountDb > 0)
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
        _isImportRunning = false;
        Monitor.Exit(_lockScheduledActionsAndImport);
      }
    }

    private void UpdateImportStatus(string status, ImportStats stats = null)
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
      string fileName = SettingsManagement.GetValue(XmlTvImportSetting.File, string.Empty);
      bool configured = !string.IsNullOrWhiteSpace(fileName);
      if (!configured || !File.Exists(fileName))
      {
        if (configured)     // avoid verbose logging
        {
          this.LogError("XMLTV import: import file \"{0}\" is not valid or does not exist", fileName ?? "[null]");
        }
        UpdateImportStatus("no files to import");
        return null;
      }

      if (!fileName.EndsWith(".lst", StringComparison.InvariantCultureIgnoreCase))
      {
        XmlTvFileSettings settings = new XmlTvFileSettings();
        settings.FileLocation = fileName;
        settings.UseTimeCorrection = SettingsManagement.GetValue(XmlTvImportSetting.UseTimeCorrection, false);
        settings.TimeCorrectionHours = SettingsManagement.GetValue(XmlTvImportSetting.TimeCorrectionHours, 0);
        settings.TimeCorrectionMinutes = SettingsManagement.GetValue(XmlTvImportSetting.TimeCorrectionMinutes, 0);
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
              this.LogWarn("XMLTV import: failed to parse time corrections from list file \"{0}\" line {1}", fileName, line);
              s.TimeCorrectionHours = 0;
              s.TimeCorrectionMinutes = 0;
            }
          }

          s.FileLocation = parts[0].Trim();
          if (!Path.IsPathRooted(s.FileLocation))
          {
            s.FileLocation = Path.Combine(Path.GetDirectoryName(fileName), s.FileLocation);
          }
          if (!File.Exists(s.FileLocation))
          {
            this.LogWarn("XMLTV import: file \"{0}\" extracted from list file \"{1}\" line {2} does not exist", s.FileLocation, fileName, line);
            continue;
          }
          settings.Add(s);
        }
        return settings;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "XMLTV import: failed to parse list file \"{0}\"", fileName);
        return null;
      }
    }

    private bool HaveNewData(ICollection<XmlTvFileSettings> files)
    {
      DateTime previousImportStarted = SettingsManagement.GetValue(SETTING_NAME_IMPORT_PREVIOUS_START, DateTime.MinValue);
      bool newData = false;

      foreach (XmlTvFileSettings file in files)
      {
        try
        {
          if (!CanReadFile(file.FileLocation))
          {
            this.LogWarn("XMLTV import: not able to read data file \"{0}\", assuming update running now", file.FileLocation);
            return false;
          }

          DateTime previousFileChange = File.GetLastWriteTime(file.FileLocation);
          if (previousFileChange > previousImportStarted)
          {
            this.LogInfo("XMLTV import: data file \"{0}\" has been updated, changed {1}", file.FileLocation, previousFileChange);
            newData = true;
          }
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "XMLTV import: failed to check freshness for data file \"{0}\"", file.FileLocation);
        }
      }

      // Don't forget to check the list file itself. Files may have been added
      // or removed from the list.
      string listFile = string.Empty;
      try
      {
        listFile = SettingsManagement.GetValue(XmlTvImportSetting.File, string.Empty);
        if (listFile.EndsWith(".lst", StringComparison.InvariantCultureIgnoreCase) && File.Exists(listFile) && File.GetLastWriteTime(listFile) > previousImportStarted)
        {
          this.LogInfo("XMLTV import: list file \"{0}\" has been updated, changed {1}", listFile, File.GetLastWriteTime(listFile));
          newData = true;
        }
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "XMLTV import: failed to check freshness for list file \"{0}\"", listFile);
      }

      return newData;
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
        return _service;
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