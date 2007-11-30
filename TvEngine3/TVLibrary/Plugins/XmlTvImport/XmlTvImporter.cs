/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
  public class XmlTvImporter : ITvServerPlugin, ITvServerPluginStartedAll
  {
    #region variables
    bool _workerThreadRunning = false;
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
        return new SetupTv.Sections.XmlSetup();
      }
    }

		private void DownloadFileCallback(object sender, DownloadStringCompletedEventArgs e)
		{
			// funny, even though the file has failed to download, this event is still called???
			// no way to check for failed downloads

			//System.Diagnostics.Debugger.Launch();

			try
			{
				TvBusinessLayer layer = new TvBusinessLayer();
				string info = "";
				string result = null;
				
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
						TextWriter tw = null;
						try
						{
							string path = layer.GetSetting("xmlTv", "").Value;
							path = path + @"\tvguide.xml";
							tw = new StreamWriter(path);
							tw.Write(e.Result);
						}
						catch (Exception ex)
						{
							info += " Trouble writing to file. [" + ex.Message + "]";
						}
						finally
						{
							if (tw != null) tw.Close();
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
			catch(Exception ex)
			{
			}
			finally
			{
				//always remember to turn on the timer again.
				_timer1.Enabled = true;
			}
		}

		public bool RetrieveRemoteFile(String folder, string URL)
		{
			//System.Diagnostics.Debugger.Launch();
			bool status = false;
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
				return status;
			}

			if (folder.Length == 0)
			{
				errMsg = "No tvguide.xml path defined.";
				Log.Error(errMsg);
				setting = layer.GetSetting("xmlTvRemoteScheduleTransferStatus", "");
				setting.Value = errMsg;
				setting.Persist();
				return status;
			}

			lastTransferAt = DateTime.Now.ToString();
			transferStatus = "downloading...";			

			WebClient Client = new WebClient();
			

			bool isHTTP = (URL.ToLower().IndexOf("http://") == 0);
			bool isFTP = (URL.ToLower().IndexOf("ftp://") == 0);

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
				Log.Info ("HTTP URL detected.");
			}

			Log.Info("initiating download of remote file from " + URL);
			Uri uri = new Uri(URL);
			//Client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(DownloadFileCallback);
			Client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadFileCallback);
			
			try
			{
				//Client.DownloadFileAsync(uri, folder + @"\tvguide.xml");				
				Client.DownloadStringAsync(uri);
				status = true;
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

			return status;
		}

    /// <summary>
    /// Forces the import of the tvguide. Usable when testing the grabber
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void ForceImport(String folder,bool importXML,bool importLST)
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
			bool inProgress = RetrieveRemoteTvGuide();

			if (inProgress) // we are downloading a remove tvguide.xml, wait for it to complete, before trying to read it (avoiding file locks)
			{
				_timer1.Enabled = false;
			}

      CheckNewTVGuide();
    }

		[MethodImpl(MethodImplOptions.Synchronized)]
		protected bool RetrieveRemoteTvGuide()
		{
			//System.Diagnostics.Debugger.Launch();
			TvBusinessLayer layer = new TvBusinessLayer();

			bool remoteSchedulerEnabled = (layer.GetSetting("xmlTvRemoteSchedulerEnabled", "false").Value == "true");
			if (!remoteSchedulerEnabled)
			{
				return false;
			}

			DateTime defaultRemoteScheduleTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 30, 0);
			string remoteScheduleTimeStr = layer.GetSetting("xmlTvRemoteScheduleTime", defaultRemoteScheduleTime.ToString()).Value;
			DateTime remoteScheduleTime = (DateTime)(System.ComponentModel.TypeDescriptor.GetConverter(new DateTime(1990, 5, 6)).ConvertFrom(remoteScheduleTimeStr));

			DateTime now = DateTime.Now;
			if (now.Hour == remoteScheduleTime.Hour && now.Minute == remoteScheduleTime.Minute)
			{				
				string folder = layer.GetSetting("xmlTv", System.IO.Directory.GetCurrentDirectory()).Value;
				string URL = layer.GetSetting("xmlTvRemoteURL", "").Value;
				return RetrieveRemoteFile(folder, URL);				
			}
			else
			{
				//Log.Info("Not the time to fetch remote file yet");
			}
			return false;
		}

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected void CheckNewTVGuide()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      string folder = layer.GetSetting("xmlTv", System.IO.Directory.GetCurrentDirectory()).Value;
      DateTime lastTime;

      try
      {
        lastTime = DateTime.Parse(layer.GetSetting("xmlTvLastUpdate", "").Value);
      }
      catch (Exception e)
      {
        Log.Info("xmlTvLastUpdate not found, forcing import {0}",e.Message);
        lastTime = DateTime.MinValue;
      }


      bool importXML = false;
      bool importLST = false;
      DateTime importDate = DateTime.MinValue;  // gets the date of the newest file

      string fileName = folder + @"\tvguide.xml";

      if (System.IO.File.Exists(fileName))
      {
        DateTime fileTime = DateTime.Parse(System.IO.File.GetLastWriteTime(fileName).ToString()); // for rounding errors!!!
        if (lastTime < fileTime)
        {
          importXML = true;
          importDate = fileTime;
        }
      }

      fileName = folder + @"\tvguide.lst";

      if (layer.GetSetting("xmlTvImportLST", "true").Value == "true" && System.IO.File.Exists(fileName))
      {
        DateTime fileTime = DateTime.Parse(System.IO.File.GetLastWriteTime(fileName).ToString()); // for rounding errors!!!
        if (lastTime < fileTime)
        {
          importLST = true;
          if (fileTime > importDate) importDate = fileTime;
        }
      }
 
      if (importXML || importLST)
      {
      
        StartImport(importXML, importLST, importDate);
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected void StartImport(bool importXML, bool importLST, DateTime importDate)
    {
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
          IOUtil.CheckFileAccessRights(fileName, FileMode.Open,FileAccess.Read, FileShare.Read);
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
          IOUtil.CheckFileAccessRights(fileName, FileMode.Open,FileAccess.Read, FileShare.Read);
        }
        catch (Exception e)
        {
          Log.Error(@"plugin:xmltv StartImport - File [" + fileName + "] doesn't have read access : " + e.Message);
          return;
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
						FileStream streamIn = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
						StreamReader fileIn = new StreamReader(streamIn, fileEncoding, true);

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
  }
}
