/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using SetupTv;
using TvControl;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvEngine.PowerScheduler.Interfaces;
using System.Runtime.CompilerServices;

using Gentle.Common;
using Gentle.Framework;

namespace TvEngine
{
  public class XmlTvImporter : ITvServerPlugin, ITvServerPluginStartedAll, IWakeupHandler
  {
    #region constants
    private const int remoteFileDonwloadTimeoutSecs = 360; //6 minutes
    #endregion

    #region variables
    bool _workerThreadRunning = false;
    bool _remoteFileDownloadInProgress = false;
    DateTime _remoteFileDonwloadInProgressAt = DateTime.MinValue;

    System.Timers.Timer _timer1;
    #endregion

    #region properties
    /// <summary>
    /// returns the name of the plugin
    /// </summary>
    public string Name
    {
      get
      {
        return "XmlTv";
      }
    }

    /// <summary>
    /// returns the version of the plugin
    /// </summary>
    public string Version
    {
      get
      {
        return "1.0.0.0";
      }
    }

    /// <summary>
    /// returns the author of the plugin
    /// </summary>
    public string Author
    {
      get
      {
        return "Frodo";
      }
    }

    /// <summary>
    /// returns if the plugin should only run on the master server
    /// or also on slave servers
    /// </summary>
    public bool MasterOnly
    {
      get
      {
        return true;
      }
    }
    #endregion

    #region public methods
    /// <summary>
    /// Starts the plugin
    /// </summary>
    public void Start(IController controller)
    {
      Log.WriteFile("plugin: xmltv started");
      CheckNewTVGuide();
      //RetrieveRemoteTvGuide();
      _timer1 = new System.Timers.Timer();
      _timer1.Interval = 60000;
      _timer1.Enabled = true;
      _timer1.Elapsed += new System.Timers.ElapsedEventHandler(_timer1_Elapsed);
    }


    /// <summary>
    /// Stops the plugin
    /// </summary>
    public void Stop()
    {
      Log.WriteFile("plugin: xmltv stopped");
      if (_timer1 != null)
      {
        _timer1.Enabled = false;
        _timer1.Dispose();
        _timer1 = null;
      }
    }

    /// <summary>
    /// returns the setup sections for display in SetupTv
    /// </summary>
    public SetupTv.SectionSettings Setup
    {
      get
      {
        return new SetupTv.Sections.XmlTvSetup();
      }
    }

    private void DownloadFileCallback(object sender, DownloadDataCompletedEventArgs e)
    {
      //System.Diagnostics.Debugger.Launch();
      try
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        string info = "";
        byte[] result = null;

        try
        {
          if (e.Result != null || e.Result.Length > 0)
          {
            result = e.Result;
          }
        }
        catch (Exception ex)
        {
          info = "Download failed: (" + ex.InnerException.Message + ").";
        }

        if (result != null)
        {
          if (result.Length == 0)
          {
            info = "File empty.";
          }
          else
          {
            info = "File downloaded.";

            //check if file can be opened for writing....																		
            string path = layer.GetSetting("xmlTv", "").Value;
            path = path + @"\tvguide.xml";
            bool waitingForFileAccess = true;
            int retries = 0;

            //in case the destination file is locked by another process, retry each 30 secs, but max 5 min. before giving up
            while (waitingForFileAccess && retries < 10)
            {
              if (!_remoteFileDownloadInProgress)
              {
                return;
              }
              try
              {
                //IOUtil.CheckFileAccessRights(path, FileMode.Open, FileAccess.Write, FileShare.Write);
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                  fs.Write(e.Result, 0, e.Result.Length);
                  waitingForFileAccess = false;
                }
              }
              catch (Exception ex)
              {
                Log.Info("file is locked, retrying in 30secs. [" + ex.Message + "]");
                retries++;
                Thread.Sleep(30000); //wait 30 sec. before retrying.
              }
            }

            if (waitingForFileAccess)
            {
              info = "Trouble writing to file.";
            }

          }
        }

        Setting setting;
        setting = layer.GetSetting("xmlTvRemoteScheduleTransferStatus", "");
        setting.Value = info;
        setting.Persist();

        setting = layer.GetSetting("xmlTvRemoteScheduleLastTransfer", "");
        setting.Value = DateTime.Now.ToString();
        setting.Persist();

        Log.Info(info);
      }
      catch (Exception)
      {
      }
      finally
      {
        _remoteFileDownloadInProgress = false; //signal that we are done downloading.
      }
    }

    public void RetrieveRemoteFile(String folder, string URL)
    {
      //System.Diagnostics.Debugger.Launch();			
      if (_remoteFileDownloadInProgress)
      {
        return;
      }
      string lastTransferAt = "";
      string transferStatus = "";

      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting;

      string errMsg = "";
      if (URL.Length == 0)
      {
        errMsg = "No URL defined.";
        Log.Error(errMsg);
        setting = layer.GetSetting("xmlTvRemoteScheduleTransferStatus", "");
        setting.Value = errMsg;
        setting.Persist();
        _remoteFileDownloadInProgress = false;
        return;
      }

      if (folder.Length == 0)
      {
        errMsg = "No tvguide.xml path defined.";
        Log.Error(errMsg);
        setting = layer.GetSetting("xmlTvRemoteScheduleTransferStatus", "");
        setting.Value = errMsg;
        setting.Persist();
        _remoteFileDownloadInProgress = false;
        return;
      }

      lastTransferAt = DateTime.Now.ToString();
      transferStatus = "downloading...";

      WebClient Client = new WebClient();


      bool isHTTP = (URL.ToLowerInvariant().IndexOf("http://") == 0);
      bool isFTP = (URL.ToLowerInvariant().IndexOf("ftp://") == 0);

      if (isFTP)
      {
        // grab username, password and server from the URL
        // ftp://user:pass@www.somesite.com/TVguide.xml

        Log.Info("FTP URL detected.");

        int passwordEndIdx = URL.IndexOf("@");

        if (passwordEndIdx > -1)
        {
          Log.Info("FTP username/password detected.");

          int userStartIdx = 6; //6 is the length of chars in  --> "ftp://"
          int userEndIdx = URL.IndexOf(":", userStartIdx);

          string user = URL.Substring(userStartIdx, (userEndIdx - userStartIdx));
          string pass = URL.Substring(userEndIdx + 1, (passwordEndIdx - userEndIdx - 1));
          URL = "ftp://" + URL.Substring(passwordEndIdx + 1);

          Client.Credentials = new NetworkCredential(user, pass);
        }
        else
        {
          Log.Info("no FTP username/password detected. Using anonymous access.");
        }
      }
      else
      {
        Log.Info("HTTP URL detected.");
      }

      Log.Info("initiating download of remote file from " + URL);
      Uri uri = new Uri(URL);
      Client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadFileCallback);

      try
      {
        _remoteFileDownloadInProgress = true;
        _remoteFileDonwloadInProgressAt = DateTime.Now;
        Client.DownloadDataAsync(uri);
      }
      catch (WebException ex)
      {
        errMsg = "An error occurred while downloading the file: " + URL + " (" + ex.Message + ").";
        Log.Error(errMsg);
        lastTransferAt = errMsg;
      }
      catch (InvalidOperationException ex)
      {
        errMsg = "The " + folder + @"\tvguide.xml file is in use by another thread (" + ex.Message + ").";
        Log.Error(errMsg);
        lastTransferAt = errMsg;
      }
      catch (Exception ex)
      {
        errMsg = "Unknown error @ " + URL + "(" + ex.Message + ").";
        Log.Error(errMsg);
        lastTransferAt = errMsg;
      }

      setting = layer.GetSetting("xmlTvRemoteScheduleTransferStatus", "");
      setting.Value = transferStatus;
      setting.Persist();

      setting = layer.GetSetting("xmlTvRemoteScheduleLastTransfer", "");
      setting.Value = lastTransferAt;
      setting.Persist();
    }

    /// <summary>
    /// Forces the import of the tvguide. Usable when testing the grabber
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void ForceImport(String folder, bool importXML, bool importLST)
    {
      string fileName = folder + @"\tvguide.xml";

      if (System.IO.File.Exists(fileName) && importXML)
      {
        importXML = true;
      }

      fileName = folder + @"\tvguide.lst";

      if (importLST && System.IO.File.Exists(fileName))
      {
        DateTime fileTime = DateTime.Parse(System.IO.File.GetLastWriteTime(fileName).ToString()); // for rounding errors!!!
        importLST = true;
      }

      if (importXML || importLST)
      {
        // Allow for deleting of all existing programs before adding the new ones. 
        // Already imported programs might incorrect data depending on the grabber & setup
        TvBusinessLayer layer = new TvBusinessLayer();
        if (layer.GetSetting("xmlTvDeleteBeforeImport", "true").Value == "true")
        {
          SqlBuilder sb = new SqlBuilder(StatementType.Delete, typeof(Program));
          SqlStatement stmt = sb.GetStatement();
          stmt.Execute();
        }
        ThreadParams tp = new ThreadParams();
        tp._importDate = DateTime.MinValue;
        tp._importLST = importLST;
        tp._importXML = importXML;

        this.ThreadFunctionImportTVGuide(tp);
      }
    }

    #endregion

    #region private members

    void _timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      RetrieveRemoteTvGuide();

      DateTime now = DateTime.Now;

      if (_remoteFileDownloadInProgress) // we are downloading a remote tvguide.xml, wait for it to complete, before trying to read it (avoiding file locks)
      {
        // check if the download has been going on for too long, then flag it as failed.
        TimeSpan ts = now - _remoteFileDonwloadInProgressAt;
        if (ts.TotalSeconds > remoteFileDonwloadTimeoutSecs)
        {
          //timed out;
          _remoteFileDownloadInProgress = false;
          TvBusinessLayer layer = new TvBusinessLayer();
          Setting setting;
          setting = layer.GetSetting("xmlTvRemoteScheduleTransferStatus", "");
          setting.Value = "File transfer timed out.";
          setting.Persist();

          setting = layer.GetSetting("xmlTvRemoteScheduleLastTransfer", "");
          setting.Value = now.ToString();
          setting.Persist();

          Log.Info("File transfer timed out.");
        }
        else
        {
          Log.Info("File transfer is in progress. Waiting...");
          return;
        }
      }
      else
      {
        CheckNewTVGuide();
      }


    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected void RetrieveRemoteTvGuide()
    {
      //System.Diagnostics.Debugger.Launch();
      if (_remoteFileDownloadInProgress)
      {
        return;
      }

      TvBusinessLayer layer = new TvBusinessLayer();

      bool remoteSchedulerEnabled = (layer.GetSetting("xmlTvRemoteSchedulerEnabled", "false").Value == "true");
      if (!remoteSchedulerEnabled)
      {
        _remoteFileDownloadInProgress = false;
        return;
      }

      DateTime defaultRemoteScheduleTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 30, 0);
      string remoteScheduleTimeStr = layer.GetSetting("xmlTvRemoteScheduleTime", defaultRemoteScheduleTime.ToString()).Value;
      DateTime remoteScheduleTime = (DateTime)(System.ComponentModel.TypeDescriptor.GetConverter(new DateTime(1990, 5, 6)).ConvertFrom(remoteScheduleTimeStr));

      DateTime now = DateTime.Now;
      if (now.Hour == remoteScheduleTime.Hour && now.Minute == remoteScheduleTime.Minute)
      {
        string folder = layer.GetSetting("xmlTv", System.IO.Directory.GetCurrentDirectory()).Value;
        string URL = layer.GetSetting("xmlTvRemoteURL", "").Value;
        RetrieveRemoteFile(folder, URL);
      }
      else
      {
        //Log.Info("Not the time to fetch remote file yet");
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected void CheckNewTVGuide()
    {
      FileStream streamIn = null;
      StreamReader fileIn = null;
      TvBusinessLayer layer = new TvBusinessLayer();
      string folder = layer.GetSetting("xmlTv", System.IO.Directory.GetCurrentDirectory()).Value;
      DateTime lastTime;

      try
      {
        lastTime = DateTime.Parse(layer.GetSetting("xmlTvLastUpdate", "").Value);
      }
      catch (Exception e)
      {
        Log.Info("xmlTvLastUpdate not found, forcing import {0}", e.Message);
        lastTime = DateTime.MinValue;
      }


      bool importXML = layer.GetSetting("xmlTvImportXML", "true").Value == "true";
      bool importLST = layer.GetSetting("xmlTvImportLST", "true").Value == "true";
      DateTime importDate = DateTime.MinValue;  // gets the date of the newest file

      string fileName = folder + @"\tvguide.xml";

      if (importXML && System.IO.File.Exists(fileName))
      {
        DateTime fileTime = System.IO.File.GetLastWriteTime(fileName);
        if (importDate < fileTime) { importDate = fileTime; }
      }

      fileName = folder + @"\tvguide.lst";

      if (importLST && System.IO.File.Exists(fileName))  // check if any files contained in tvguide.lst are newer than time of last import
      {
        try
        { 
          DateTime fileTime = System.IO.File.GetLastWriteTime(fileName); 
          if (importDate < fileTime) { importDate = fileTime; }   // A new tvguide.lst should give an import, to retain compatibility with previous version
          Encoding fileEncoding = Encoding.Default;
          streamIn = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
          fileIn = new StreamReader(streamIn, fileEncoding, true);
          while (!fileIn.EndOfStream)
          {
            string tvguideFileName = fileIn.ReadLine();
            if (tvguideFileName.Length == 0) continue;
            if (!System.IO.Path.IsPathRooted(tvguideFileName))
            {
              // extend by directory
              tvguideFileName = System.IO.Path.Combine(folder, tvguideFileName);
            }
            if (System.IO.File.Exists(tvguideFileName))
            {
              DateTime tvfileTime = System.IO.File.GetLastWriteTime(tvguideFileName);
              
              if (tvfileTime > lastTime)
              {
                if (importDate < tvfileTime) { importDate = tvfileTime; } 
              }
            }
          }        
        }                      
        finally
        {
          if (streamIn != null)
          {
            streamIn.Close();
            streamIn.Dispose();
          }
          if (fileIn != null)
          {
            fileIn.Close();
            fileIn.Dispose();
          }
        }
      }
      if ((importXML || importLST) && (DateTime.Parse(importDate.ToString()) > lastTime)) // To string and back to avoid rounding errors leading to continous reimports!!!
      {
        StartImport(importXML, importLST, importDate);
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected void StartImport(bool importXML, bool importLST, DateTime importDate)
    {
      FileStream streamIn = null;
      StreamReader fileIn = null;
      if (_workerThreadRunning)
        return;

      TvBusinessLayer layer = new TvBusinessLayer();
      string folder = layer.GetSetting("xmlTv", System.IO.Directory.GetCurrentDirectory()).Value;

      Thread.Sleep(500); // give time to the external prog to close file handle

      if (importXML)
      {
        string fileName = folder + @"\tvguide.xml";

        try
        {
          //check if file can be opened for reading....
          IOUtil.CheckFileAccessRights(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (Exception e)
        {
          Log.Error(@"plugin:xmltv StartImport - File [" + fileName + "] doesn't have read access : " + e.Message);
          return;
        }
      }

      if (importLST)
      {
        string fileName = folder + @"\tvguide.lst";

        try
        {
          IOUtil.CheckFileAccessRights(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (Exception e)
        {
          Log.Error(@"plugin:xmltv StartImport - File [" + fileName + "] doesn't have read access : " + e.Message);
          return;
        }
        try  //Check that all listed files can be read before starting import (and deleting programs list)
        {
          Encoding fileEncoding = Encoding.Default;
          streamIn = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
          fileIn = new StreamReader(streamIn, fileEncoding, true);
          while (!fileIn.EndOfStream)
          {
            string tvguideFileName = fileIn.ReadLine();
            if (tvguideFileName.Length == 0) continue;

            if (!System.IO.Path.IsPathRooted(tvguideFileName))
            {
              // extend by directory
              tvguideFileName = System.IO.Path.Combine(folder, tvguideFileName);
            }
            try
            {
              IOUtil.CheckFileAccessRights(tvguideFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception e)
            {
              Log.Error(@"plugin:xmltv StartImport - File [" + tvguideFileName + "] doesn't have read access : " + e.Message);
              return;  
            }
          }
        }
        finally
        {
          if (streamIn != null)
          {
            streamIn.Close();
            streamIn.Dispose();
          }
          if (fileIn != null)
          {
            fileIn.Close();
            fileIn.Dispose();
          }
        }
      }

      // Allow for deleting of all existing programs before adding the new ones. 
      // Already imported programs might have incorrect data depending on the grabber & setup
      // f.e when grabbing programs many days ahead
      if (layer.GetSetting("xmlTvDeleteBeforeImport", "true").Value == "true")
      {
        SqlBuilder sb = new SqlBuilder(StatementType.Delete, typeof(Program));
        SqlStatement stmt = sb.GetStatement();
        stmt.Execute();
      }

      _workerThreadRunning = true;
      ThreadParams param = new ThreadParams();
      param._importXML = importXML;
      param._importLST = importLST;
      param._importDate = importDate;
      Thread workerThread = new Thread(new ParameterizedThreadStart(ThreadFunctionImportTVGuide));
      workerThread.Name = "XmlTvImporter";
      workerThread.IsBackground = true;
      workerThread.Priority = ThreadPriority.Lowest;
      workerThread.Start(param);
    }


    private class ThreadParams
    {
      public bool _importXML;
      public bool _importLST;
      public DateTime _importDate;
    };

    void ThreadFunctionImportTVGuide(object aparam)
    {
      //System.Diagnostics.Debugger.Launch();

      SetStandbyAllowed(false);
      FileStream streamIn = null;
      StreamReader fileIn = null;

      try
      {
        ThreadParams param = (ThreadParams)aparam;

        Setting setting;
        TvBusinessLayer layer = new TvBusinessLayer();
        string folder = layer.GetSetting("xmlTv", System.IO.Directory.GetCurrentDirectory()).Value;

        int numChannels = 0, numPrograms = 0;
        string errors = "";

        try
        {
          if (param._importXML)
          {
            string fileName = folder + @"\tvguide.xml";
            Log.Write("plugin:xmltv importing " + fileName);

            XMLTVImport import = new XMLTVImport(10);  // add 10 msec dely to the background thread
            import.Import(fileName, false);

            numChannels += import.ImportStats.Channels;
            numPrograms += import.ImportStats.Programs;

            if (import.ErrorMessage.Length != 0)
              errors += "tvguide.xml:" + import.ErrorMessage + "; ";
          }

          if (param._importLST)
          {
            string fileName = folder + @"\tvguide.lst";
            Log.Write("plugin:xmltv importing files in " + fileName);

            Encoding fileEncoding = Encoding.Default;
            streamIn = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            fileIn = new StreamReader(streamIn, fileEncoding, true);

            while (!fileIn.EndOfStream)
            {
              string tvguideFileName = fileIn.ReadLine();
              if (tvguideFileName.Length == 0) continue;

              if (!System.IO.Path.IsPathRooted(tvguideFileName))
              {
                // extend by directory
                tvguideFileName = System.IO.Path.Combine(folder, tvguideFileName);
              }

              Log.WriteFile(@"plugin:xmltv importing " + tvguideFileName);

              XMLTVImport import = new XMLTVImport(10);  // add 10 msec dely to the background thread

              import.Import(tvguideFileName, false);

              numChannels += import.ImportStats.Channels;
              numPrograms += import.ImportStats.Programs;

              if (import.ErrorMessage.Length != 0)
                errors += tvguideFileName + ": " + import.ErrorMessage + "; ";
            }
          }

          setting = layer.GetSetting("xmlTvResultLastImport", "");
          setting.Value = DateTime.Now.ToString();
          setting.Persist();
          setting = layer.GetSetting("xmlTvResultChannels", "");
          setting.Value = numChannels.ToString();
          setting.Persist();
          setting = layer.GetSetting("xmlTvResultPrograms", "");
          setting.Value = numPrograms.ToString();
          setting.Persist();
          setting = layer.GetSetting("xmlTvResultStatus", "");
          setting.Value = errors;
          setting.Persist();
          Log.Write("Xmltv: imported {0} channels, {1} programs status:{2}", numChannels, numPrograms, errors);

        }
        catch (Exception ex)
        {
          Log.Error(@"plugin:xmltv import failed");
          Log.Write(ex);
        }

        setting = layer.GetSetting("xmlTvLastUpdate", "");
        setting.Value = param._importDate.ToString();
        setting.Persist();
      }
      finally
      {
        Log.WriteFile(@"plugin:xmltv import done");
        if (streamIn != null)
        {
          streamIn.Close();
          streamIn.Dispose();
        }
        if (fileIn != null)
        {
          fileIn.Close();
          fileIn.Dispose();
        }
        _workerThreadRunning = false;
        SetStandbyAllowed(true);
      }
    }

    private void EPGScheduleDue()
    {
      CheckNewTVGuide();
    }

    private void RegisterForEPGSchedule()
    {
      // Register with the EPGScheduleDue event so we are informed when
      // the EPG wakeup schedule is due.
      if (GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        IEpgHandler handler = GlobalServiceProvider.Instance.Get<IEpgHandler>();
        if (handler != null)
        {
          handler.EPGScheduleDue += new EPGScheduleHandler(EPGScheduleDue);
          Log.Debug("XmlTvImporter: registered with PowerScheduler EPG handler");
          return;
        }
      }
      Log.Debug("XmlTvImporter: NOT registered with PowerScheduler EPG handler");
    }


    private void SetStandbyAllowed(bool allowed)
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        Log.Debug("plugin:xmltv: Telling PowerScheduler standby is allowed: {0}, timeout is one hour", allowed);
        GlobalServiceProvider.Instance.Get<IEpgHandler>().SetStandbyAllowed(this, allowed, 3600);
      }
    }
    #endregion

    #region ITvServerPluginStartedAll Members

    public void StartedAll()
    {
      RegisterForEPGSchedule();
    }

    #endregion

    #region IWakeupHandler Members

    DateTime IWakeupHandler.GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      DateTime now = DateTime.Now;
      TvBusinessLayer layer = new TvBusinessLayer();
      DateTime defaultRemoteScheduleTime = new DateTime(now.Year, now.Month, now.Day, 6, 30, 0);
      string remoteScheduleTimeStr = layer.GetSetting("xmlTvRemoteScheduleTime", defaultRemoteScheduleTime.ToString()).Value;

      DateTime remoteScheduleTime = (DateTime)(System.ComponentModel.TypeDescriptor.GetConverter(new DateTime(now.Year, now.Month, now.Day)).ConvertFrom(remoteScheduleTimeStr));

      if (now < remoteScheduleTime)
      {
        remoteScheduleTime.AddDays(1);
      }

      Log.Debug("plugin:xmltv: IWakeupHandler.GetNextWakeupTime {0}", remoteScheduleTime);

      return remoteScheduleTime;
    }

    string IWakeupHandler.HandlerName
    {
      get { return "XmlTvImporter Remote Download Job"; }
    }

    #endregion
  }
}
