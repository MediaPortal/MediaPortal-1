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
using MediaPortal.WebEPG;
using System.Runtime.CompilerServices;

using Gentle.Common;
using Gentle.Framework;
using MediaPortal.EPG;

namespace TvEngine
{
  public class WebEPGImport : ITvServerPlugin, ITvServerPluginStartedAll//, IWakeupHandler, IStandbyHandler
  {
    #region constants

    #endregion

    #region variables

    private bool _workerThreadRunning = false;

    #endregion
    
    #region Constructor
    /// <summary>
    /// Create a new instance of a generic standby handler
    /// </summary>
    public WebEPGImport()
    {          
    }
    #endregion

    #region properties

    /// <summary>
    /// returns the name of the plugin
    /// </summary>
    public string Name
    {
      get
      {
        return "WebEPG";
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
        return "Arion_p - James";
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
      Log.WriteFile("plugin: webepg started");      
      
      //CheckNewTVGuide();
    }


    /// <summary>
    /// Stops the plugin
    /// </summary>
    public void Stop()
    {
      Log.WriteFile("plugin: webepg stopped");
    }

    /// <summary>
    /// returns the setup sections for display in SetupTv
    /// </summary>
    public SetupTv.SectionSettings Setup
    {
      get
      {
        return new SetupTv.Sections.WebEPGConfigControl();
      }
    }   

    //private void DownloadFileCallback(object sender, DownloadDataCompletedEventArgs e)
    //{      
    //  //System.Diagnostics.Debugger.Launch();
    //  try
    //  {
    //    TvBusinessLayer layer = new TvBusinessLayer();
    //    string info = "";
    //    byte[] result = null;

    //    try
    //    {
    //      if (e.Result != null || e.Result.Length > 0)
    //      {
    //        result = e.Result;            
    //      }
    //    }
    //    catch (Exception ex)
    //    {
    //      info = "Download failed: (" + ex.InnerException.Message + ").";
    //    }

    //    if (result != null)
    //    {
    //      if (result.Length == 0)
    //      {
    //        info = "File empty.";
    //      }
    //      else
    //      {
    //        info = "File downloaded.";

    //        if (_remoteURL.Length == 0)
    //        {
    //          return;
    //        }

    //        Uri uri = new Uri(_remoteURL);
    //        string filename = uri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
    //        filename = filename.ToLower().Trim();

    //        bool isZip = (filename.IndexOf(".zip") > -1);
    //        bool isTvGuide = (filename.IndexOf("tvguide.xml") > -1);

    //        FileInfo fI = new FileInfo(filename);
    //        filename = fI.Name;
         
    //        //check if file can be opened for writing....																		
    //        string path = layer.GetSetting("xmlTv", "").Value;

    //        if (isTvGuide || isZip)
    //        {
    //          path = path + @"\" + filename;
    //        }
    //        else
    //        {
    //          path = path + @"\tvguide.xml";
    //        }
            
    //        bool waitingForFileAccess = true;
    //        int retries = 0;
    //        bool fileWritten = false;
    //        //in case the destination file is locked by another process, retry each 30 secs, but max 5 min. before giving up
    //        while (waitingForFileAccess && retries < 10)
    //        {
    //          if (!_remoteFileDownloadInProgress)
    //          {
    //            return;
    //          }
    //          try
    //          {
    //            //IOUtil.CheckFileAccessRights(path, FileMode.Open, FileAccess.Write, FileShare.Write);
    //            if (!fileWritten)
    //            {
    //              using (FileStream fs = new FileStream(path, FileMode.Create))
    //              {
    //                fs.Write(e.Result, 0, e.Result.Length);
    //                fs.Close();
    //                fileWritten = true;
    //              }
    //            }
    //          }
    //          catch (Exception ex)
    //          {
    //            Log.Info("file is locked, retrying in 30secs. [" + ex.Message + "]");
    //            retries++;
    //            Thread.Sleep(30000); //wait 30 sec. before retrying.
    //          }

    //          if (isZip)
    //          {
    //            try
    //            {
    //              string newLoc = layer.GetSetting("xmlTv", "").Value + @"\";
    //              Log.Info("extracting zip file {0} to location {1}", path, newLoc);
    //              ZipFile zip = new ZipFile(path);
    //              zip.ExtractAll(newLoc, true);
    //            }
    //            catch (Exception ex2)
    //            {
    //              Log.Info("file is locked, retrying in 30secs. [" + ex2.Message + "]");
    //              retries++;
    //              Thread.Sleep(30000); //wait 30 sec. before retrying.
    //            }
    //          }
              
    //          waitingForFileAccess = false;

    //        }

    //        if (waitingForFileAccess)
    //        {              
    //          info = "Trouble writing to file.";
    //        }

    //      }
    //    }

    //    Setting setting;
    //    setting = layer.GetSetting("xmlTvRemoteScheduleTransferStatus", "");
    //    setting.Value = info;
    //    setting.Persist();

    //    setting = layer.GetSetting("xmlTvRemoteScheduleLastTransfer", "");
    //    setting.Value = DateTime.Now.ToString();
    //    setting.Persist();

    //    Log.Info(info);
    //  }
    //  catch (Exception)
    //  {
    //  }
    //  finally
    //  {
    //    _remoteFileDownloadInProgress = false; //signal that we are done downloading.
    //    SetStandbyAllowed(true);
    //  }
    //}

    //public void RetrieveRemoteFile(String folder, string URL)
    //{
    //  //System.Diagnostics.Debugger.Launch();			
    //  if (_remoteFileDownloadInProgress)
    //  {
    //    return;
    //  }
    //  string lastTransferAt = "";
    //  string transferStatus = "";

    //  _remoteURL = URL;

    //  TvBusinessLayer layer = new TvBusinessLayer();
    //  Setting setting;

    //  string errMsg = "";
    //  if (URL.Length == 0)
    //  {
    //    errMsg = "No URL defined.";
    //    Log.Error(errMsg);
    //    setting = layer.GetSetting("xmlTvRemoteScheduleTransferStatus", "");
    //    setting.Value = errMsg;
    //    setting.Persist();
    //    _remoteFileDownloadInProgress = false;
    //    SetStandbyAllowed(true);
    //    return;
    //  }

    //  if (folder.Length == 0)
    //  {
    //    errMsg = "No tvguide.xml path defined.";
    //    Log.Error(errMsg);
    //    setting = layer.GetSetting("xmlTvRemoteScheduleTransferStatus", "");
    //    setting.Value = errMsg;
    //    setting.Persist();
    //    _remoteFileDownloadInProgress = false;
    //    SetStandbyAllowed(true);
    //    return;
    //  }

    //  lastTransferAt = DateTime.Now.ToString();
    //  transferStatus = "downloading...";

    //  WebClient Client = new WebClient();


    //  bool isHTTP = (URL.ToLowerInvariant().IndexOf("http://") == 0);
    //  bool isFTP = (URL.ToLowerInvariant().IndexOf("ftp://") == 0);

    //  if (isFTP)
    //  {
    //    // grab username, password and server from the URL
    //    // ftp://user:pass@www.somesite.com/TVguide.xml

    //    Log.Info("FTP URL detected.");

    //    int passwordEndIdx = URL.IndexOf("@");

    //    if (passwordEndIdx > -1)
    //    {
    //      Log.Info("FTP username/password detected.");

    //      int userStartIdx = 6; //6 is the length of chars in  --> "ftp://"
    //      int userEndIdx = URL.IndexOf(":", userStartIdx);

    //      string user = URL.Substring(userStartIdx, (userEndIdx - userStartIdx));
    //      string pass = URL.Substring(userEndIdx + 1, (passwordEndIdx - userEndIdx - 1));
    //      URL = "ftp://" + URL.Substring(passwordEndIdx + 1);

    //      Client.Credentials = new NetworkCredential(user, pass);
    //      Client.Proxy.Credentials = CredentialCache.DefaultCredentials;
    //    }
    //    else
    //    {
    //      Log.Info("no FTP username/password detected. Using anonymous access.");
    //    }
    //  }
    //  else
    //  {
    //    Log.Info("HTTP URL detected.");
    //  }

    //  Log.Info("initiating download of remote file from " + URL);
    //  Uri uri = new Uri(URL);
    //  Client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadFileCallback);

    //  try
    //  {
    //    SetStandbyAllowed(false);
    //    _remoteFileDownloadInProgress = true;
    //    _remoteFileDonwloadInProgressAt = DateTime.Now;
    //    Client.DownloadDataAsync(uri);
    //  }
    //  catch (WebException ex)
    //  {
    //    errMsg = "An error occurred while downloading the file: " + URL + " (" + ex.Message + ").";
    //    Log.Error(errMsg);
    //    lastTransferAt = errMsg;
    //  }
    //  catch (InvalidOperationException ex)
    //  {
    //    errMsg = "The " + folder + @"\tvguide.xml file is in use by another thread (" + ex.Message + ").";
    //    Log.Error(errMsg);
    //    lastTransferAt = errMsg;
    //  }
    //  catch (Exception ex)
    //  {
    //    errMsg = "Unknown error @ " + URL + "(" + ex.Message + ").";
    //    Log.Error(errMsg);
    //    lastTransferAt = errMsg;
    //  }
      
    //  setting = layer.GetSetting("xmlTvRemoteScheduleTransferStatus", "");
    //  setting.Value = transferStatus;
    //  setting.Persist();

    //  setting = layer.GetSetting("xmlTvRemoteScheduleLastTransfer", "");
    //  setting.Value = lastTransferAt;
    //  setting.Persist();
    //}

    /// <summary>
    /// Forces the import of the tvguide. Usable when testing the grabber
    /// </summary>
    public void ForceImport()
    {
      ForceImport(null);
    }

    /// <summary>
    /// Forces the import of the tvguide. Usable when testing the grabber
    /// </summary>
    public void ForceImport(WebEPG.ShowProgressHandler showProgress)
    {
      StartImport(showProgress);
    }

    #endregion

    #region private members
    
    [MethodImpl(MethodImplOptions.Synchronized)]
    protected void StartImport(WebEPG.ShowProgressHandler showProgress)
    {
      if (_workerThreadRunning)
        return;

      
      _workerThreadRunning = true;
      ThreadParams param = new ThreadParams();
      param.showProgress = showProgress;
      Thread workerThread = new Thread(new ParameterizedThreadStart(ThreadFunctionImportTVGuide));
      workerThread.Name = "WebEPGImporter";
      workerThread.IsBackground = true;
      workerThread.Priority = ThreadPriority.Lowest;
      workerThread.Start(param);
    }

    private class ThreadParams
    {
      public WebEPG.ShowProgressHandler showProgress;
    };

    void ThreadFunctionImportTVGuide(object aparam)
    {
      SetStandbyAllowed(false);

      try
      {
        ThreadParams param = (ThreadParams)aparam;

        Setting setting;

        TvBusinessLayer layer = new TvBusinessLayer();
        string destination = layer.GetSetting("webepgDestination", "db").Value;
        string webepgDirectory = Log.GetPathName();
        string configFile = webepgDirectory + @"\WebEPG\WebEPG.xml";

        //int numChannels = 0, numPrograms = 0;
        //string errors = "";

        try
        {
          Log.Write("plugin:webepg importing");
          Log.Info("WebEPG: Using directory {0}", webepgDirectory);


          IEpgDataSink epgSink;

          if (destination == "db")
          {
            bool deleteBeforeImport = layer.GetSetting("webepgDeleteBeforeImport", "true").Value == "true";
            bool deleteOnlyOverlapping = (layer.GetSetting("webepgDeleteOnlyOverlapping", "true").Value == "true") 
                                          && deleteBeforeImport;
            // Allow for deleting of all existing programs before adding the new ones. 
            // Already imported programs might have incorrect data depending on the grabber & setup
            // f.e when grabbing programs many days ahead
            if (deleteBeforeImport && ! deleteOnlyOverlapping)
            {
              SqlBuilder sb = new SqlBuilder(StatementType.Delete, typeof(Program));
              SqlStatement stmt = sb.GetStatement();
              stmt.Execute();
            }
            epgSink = new DatabaseEPGDataSink(deleteOnlyOverlapping);
            Log.Info("Writing to TVServer database");
          }
          else
          {
            string xmltvDirectory = string.Empty;
            if (destination == "xmltv")
            {
              xmltvDirectory = layer.GetSetting("webepgDestinationFolder", string.Empty).Value;
            }
            if (xmltvDirectory == string.Empty)
            {
              xmltvDirectory = layer.GetSetting("xmlTv", System.IO.Directory.GetCurrentDirectory()).Value;
            }
            Log.Info("Writing to tvguide.xml in {0}", xmltvDirectory);
            // Open XMLTV output file
            if (!Directory.Exists(xmltvDirectory))
            {
              Directory.CreateDirectory(xmltvDirectory);
            }
            epgSink = new XMLTVExport(xmltvDirectory);
          }

          WebEPG epg = new WebEPG(configFile, epgSink, webepgDirectory);
          if (param.showProgress != null)
          {
            epg.ShowProgress += param.showProgress;
          }
          epg.Import();
          if (param.showProgress != null)
          {
            epg.ShowProgress -= param.showProgress;
          }

          setting = layer.GetSetting("webepgResultLastImport", "");
          setting.Value = DateTime.Now.ToString();
          setting.Persist();
          setting = layer.GetSetting("webepgResultChannels", "");
          setting.Value = epg.ImportStats.Channels.ToString();
          setting.Persist();
          setting = layer.GetSetting("webepgResultPrograms", "");
          setting.Value = epg.ImportStats.Programs.ToString();
          setting.Persist();
          setting = layer.GetSetting("webepgResultStatus", "");
          setting.Value = epg.ImportStats.Status;
          setting.Persist();
          //Log.Write("Xmltv: imported {0} channels, {1} programs status:{2}", numChannels, numPrograms, errors);

        }
        catch (Exception ex)
        {
          Log.Error(@"plugin:webepg import failed");
          Log.Write(ex);
        }

        setting = layer.GetSetting("webepgResultLastImport", "");
        setting.Value = DateTime.Now.ToString();
        setting.Persist();
      }
      finally
      {
        Log.WriteFile(@"plugin:webepg import done");
        _workerThreadRunning = false;
        SetStandbyAllowed(true);
      }
    }

    private void EPGScheduleDue()
    {
      StartImport(null);
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
          Log.Debug("WebEPGImporter: registered with PowerScheduler EPG handler");
          return;
        }
      }
      Log.Debug("WebEPGImporter: NOT registered with PowerScheduler EPG handler");
    }


    private void SetStandbyAllowed(bool allowed)
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IEpgHandler>())
      {
        Log.Debug("plugin:webepg: Telling PowerScheduler standby is allowed: {0}, timeout is one hour", allowed);
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
   
  }
}
