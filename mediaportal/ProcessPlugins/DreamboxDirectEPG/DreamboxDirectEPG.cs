#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using System.Data;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using DreamBox;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Util;
using MediaPortal.GUI.Library;


namespace ProcessPlugins.DreamboxDirectEPG
{
  public class DreamboxDirectEPG : IPlugin, ISetupForm
  {
    private System.ComponentModel.BackgroundWorker _EPGbackgroundWorker = new System.ComponentModel.BackgroundWorker();
    System.Windows.Forms.Timer _timer;
    private int _Hour = 0;
    private int _Minute = 0;
    private ArrayList captureCards = new ArrayList();
    private string _DreamboxIP = "";
    private string _DreamboxUserName = "";
    private string _DreamboxPassword = "";

    public DreamboxDirectEPG()
    {
      _EPGbackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(_EPGbackgroundWorker_RunWorkerCompleted);
      _EPGbackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(_EPGbackgroundWorker_DoWork);
      _timer = new System.Windows.Forms.Timer();
      // check every 60 seconds for changes
      _timer.Interval = 60000;
      _timer.Enabled = false;
      _timer.Tick += new EventHandler(_timer_Tick);
    }

    void _EPGbackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
    {
      // Import Dreambox EPG Data
      ImportEPG();
    }

    void _EPGbackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
    {
      // Done
    }

    public void Start()
    {
      LoadSettings();
      _timer.Enabled = true;
    }

    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _Hour = xmlreader.GetValueAsInt("DreamboxDirectEPG", "Hour", 0);
        _Minute = xmlreader.GetValueAsInt("DreamboxDirectEPG", "Minute", 0);

        _DreamboxIP = xmlreader.GetValueAsString("DreamboxDirectEPG", "IP", "dreambox");
        _DreamboxUserName = xmlreader.GetValueAsString("DreamboxDirectEPG", "UserName", "root");
        _DreamboxPassword = xmlreader.GetValueAsString("DreamboxDirectEPG", "Password", "dreambox");
      }
    }

    public void Stop()
    {
      Log.Info("DreamBoxDirectEPG: plugin stopping.");
      _timer.Enabled = false;
      return;
    }


    void _timer_Tick(object sender, EventArgs e)
    {
      // check time
      System.DateTime now = System.DateTime.Now;
      if (now.Hour == _Hour && now.Minute == _Minute)
      {
        // if time ok then start import (threaded)
        if (!_EPGbackgroundWorker.IsBusy)
          _EPGbackgroundWorker.RunWorkerAsync();
      }

    }

    #region Import EPG
    void ImportEPG()
    {
      DreamBox.Core box = new DreamBox.Core("http://" + _DreamboxIP, _DreamboxUserName, _DreamboxPassword);
      DataTable dt = box.Data.UserTVBouquets.Tables[0];

      for (int i = 0; i < dt.Rows.Count; i++)
      {
        string reference = dt.Rows[i]["Ref"].ToString();
        ImportEPGChannels(reference);

      }
    }
    private void ImportEPGChannels(string reference)
    {
      DreamBox.Core box = new DreamBox.Core("http://" + _DreamboxIP, _DreamboxUserName, _DreamboxPassword);
      DataTable dt = box.Data.Channels(reference).Tables[0];

      for (int i = 0; i < dt.Rows.Count; i++)
      {
        string channelreference = dt.Rows[i]["Ref"].ToString();
        ImportChannelEPG(channelreference);

      }
    }
    private void ImportChannelEPG(string reference)
    {
      DreamBox.Core box = new DreamBox.Core("http://" + _DreamboxIP, _DreamboxUserName, _DreamboxPassword);
      ServiceEpgData epg = box.XML.EPG(reference);

      ArrayList programs = new ArrayList();
      TVDatabase.GetAllPrograms(out programs);

      foreach (EpgEvent tvprogram in epg.Events)
      {
        bool found = false;
        foreach (TVProgram p in programs)
        {
          if (p.Title == tvprogram.Description)
          {
            found = true;
            break;
          }
        }
        if (!found)
        {
          // add start date
          int year = Convert.ToInt32(tvprogram.Date.Split('.')[2].ToString());
          int month = Convert.ToInt32(tvprogram.Date.Split('.')[1].ToString());
          int day = Convert.ToInt32(tvprogram.Date.Split('.')[0].ToString());
          int hour = Convert.ToInt32(tvprogram.Time.Split(':')[0].ToString());
          int minutes = Convert.ToInt32(tvprogram.Time.Split(':')[1].ToString());
          System.DateTime start = new DateTime(year, month, day, hour, minutes, 0);

          double duration = Convert.ToDouble(tvprogram.Duration);
          System.DateTime end = start.AddSeconds(duration); ;

          TVProgram program = new TVProgram(epg.ServiceName, start, end, tvprogram.Description);
          program.Description = tvprogram.Details;
          program.Genre = tvprogram.Genre;
          program.Date = start.ToString();

          TVDatabase.AddProgram(program);
        }

      }

    }
    public void LoadCaptureCards()
    {

      if (File.Exists(Config.GetFile(Config.Dir.Config, "capturecards.xml")))
      {
        using (FileStream fileStream = new FileStream(Config.GetFile(Config.Dir.Config, "capturecards.xml"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
          try
          {
            //
            // Create Soap Formatter
            //
            SoapFormatter formatter = new SoapFormatter();

            //
            // Serialize
            //
            captureCards = new ArrayList();
            captureCards = (ArrayList)formatter.Deserialize(fileStream);
            for (int i = 0; i < captureCards.Count; i++)
            {
              ((TVCaptureDevice)captureCards[i]).ID = (i + 1);

            }
            //
            // Finally close our file stream
            //
            fileStream.Close();
          }
          catch
          {
            MessageBox.Show("Failed to load previously configured capture card(s), you will need to re-configure your device(s).", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
        }
      }
    }


    #endregion

    #region IPlugin Members



    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "Will run EPG import from Dreambox once a day";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      // TODO:  Add CallerIdPlugin.GetWindowId implementation
      return -1;
    }


    public string Author()
    {
      return "Gary Wenneker";
    }

    public string PluginName()
    {
      return "Dreambox Direct EPG Reader";
    }

    public bool HasSetup()
    {
      return true;
    }

    // show the setup dialog
    public void ShowPlugin()
    {
      Form setup = new DreamboxDirectEPGSettings();
      setup.ShowDialog();
    }

    /// <summary>
    /// If the plugin should have its own button on the main menu of MediaPortal then it
    /// should return true to this method, otherwise if it should not be on home
    /// it should return false
    /// </summary>
    /// <param name="strButtonText">text the button should have</param>
    /// <param name="strButtonImage">image for the button, or empty for default</param>
    /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
    /// <param name="strPictureImage">subpicture for the button or empty for none</param>
    /// <returns>true  : plugin needs its own button on home
    ///          false : plugin does not need its own button on home</returns>
    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = String.Empty;
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = String.Empty;
      return false;
    }

    #endregion
  }
}