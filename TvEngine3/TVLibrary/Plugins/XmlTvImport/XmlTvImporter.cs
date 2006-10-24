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
using System.Text;
using System.Threading;
using System.IO;
using SetupTv;
using TvControl;
using TvDatabase;
using TvLibrary.Log;

namespace TvEngine
{
  public class XmlTvImporter : ITvServerPlugin
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

    protected void CheckNewTVGuide()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      string folder = layer.GetSetting("xmlTv", System.IO.Directory.GetCurrentDirectory()).Value;
      string lastTime = layer.GetSetting("xmlTvLastUpdate", System.IO.Directory.GetCurrentDirectory()).Value;
      string fileName = folder + @"\tvguide.xml";
      bool shouldImportTvGuide = false;

      if (System.IO.File.Exists(fileName))
      {
        string strFileTime = System.IO.File.GetLastWriteTime(fileName).ToString();
        if (lastTime != strFileTime)
        {
          shouldImportTvGuide = true;
        }
      }
      if (shouldImportTvGuide)
      {
        StartImportXML();
      }
    }
    protected void StartImportXML()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      string folder = layer.GetSetting("xmlTv", System.IO.Directory.GetCurrentDirectory()).Value;
      string fileName = folder + @"\tvguide.xml";
      Thread.Sleep(500); // give time to the external prog to close file handle
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
        Log.WriteFile(@"plugin:xmltv StartImportXML - Exception " + fileName);
        return;
      }
      if (!_workerThreadRunning)
      {
        _workerThreadRunning = true;
        Thread workerThread = new Thread(new ThreadStart(ThreadFunctionImportTVGuide));
        workerThread.Priority = ThreadPriority.Lowest;
        workerThread.Start();
      }
    }
    
    void ThreadFunctionImportTVGuide()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      string folder = layer.GetSetting("xmlTv", System.IO.Directory.GetCurrentDirectory()).Value;
      string fileName = folder + @"\tvguide.xml";
      Log.WriteFile(@"plugin:xmltv detected new tvguide ->import new tvguide");
      Thread.Sleep(500);
      try
      {
        XMLTVImport import = new XMLTVImport(10);  // add 10 msec dely to the background thread
        import.Import(fileName, false);
      }
      catch (Exception)
      {
      }

      try
      {
        //
        // Make sure the file exists before we try to do any processing, thus if the file doesn't
        // exist we we'll save ourselves from getting a file not found exception.
        //
        if (File.Exists(fileName))
        {
          Setting setting=layer.GetSetting("xmlTvLastUpdate", System.IO.Directory.GetCurrentDirectory());

          string strFileTime = System.IO.File.GetLastWriteTime(fileName).ToString();
          setting.Value= strFileTime;
          setting.Persist();
        }

      }
      catch (Exception)
      {
      }
      _workerThreadRunning = false;
      Log.WriteFile(@"plugin:xmltv import done");
    }
    #endregion
  }
}
