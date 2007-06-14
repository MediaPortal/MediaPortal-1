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
    #endregion

    #region private members

    void _timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      CheckNewTVGuide();
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
        if (lastTime  < fileTime)
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
          Encoding fileEncoding = Encoding.Default;
          FileStream streamIn = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
          StreamReader fileIn = new StreamReader(streamIn, fileEncoding, true);
          fileIn.Close();
          streamIn.Close();
        }
        catch (Exception)
        {
          Log.Error(@"plugin:xmltv StartImport - Exception " + fileName);
          return;
        }
      }

      if (importLST)
      {
        string fileName = folder + @"\tvguide.lst";
        try
        {
          //check if file can be opened for reading....
          Encoding fileEncoding = Encoding.Default;
          FileStream streamIn = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
          StreamReader fileIn = new StreamReader(streamIn, fileEncoding, true);
          fileIn.Close();
          streamIn.Close();
        }
        catch (Exception)
        {
          Log.Error(@"plugin:xmltv StartImport - Exception " + fileName);
          return;
        }
      }


      _workerThreadRunning = true;
      ThreadParams param = new ThreadParams();
      param._importXML= importXML;
      param._importLST= importLST;
      param._importDate = importDate;
      Thread workerThread = new Thread(new ParameterizedThreadStart(ThreadFunctionImportTVGuide));
      workerThread.Priority = ThreadPriority.Lowest;
      workerThread.Start( param );
    }

    private class ThreadParams
    {
      public bool _importXML;
      public bool _importLST;
      public DateTime _importDate;
    };

    void ThreadFunctionImportTVGuide( object aparam )
    {
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
