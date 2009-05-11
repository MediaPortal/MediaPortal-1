#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace MediaPortal.TVGuideScheduler
{
  class GetGuideData
  {
    public static void Main(string[] options)
    {
      Thread.CurrentThread.Name = "TvGuideScheduler";

      string grabber = null;
      int grabberDays;
      string multiGrab;
      string dayString;
      string path;
      string[] grabberSingleDays = null;
      bool LoadFromFile = false;
      bool runGrabberLowPriority = false;
      string FileToImport = null;
      bool RunConfig = false;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        grabber = xmlreader.GetValueAsString("xmltv", "grabber", "tv_grab_uk_rt");
        multiGrab = xmlreader.GetValueAsString("xmltv", "advanced", "yes");
        dayString = xmlreader.GetValueAsString("xmltv", "days", "1,2,3,5,10");
        path = xmlreader.GetValueAsString("xmltv", "folder", "xmltv");
        runGrabberLowPriority = xmlreader.GetValueAsBool("xmltv", "lowpriority", false);
      }
      //check any command line arguments:
      // /file (filename) - import xmltv data from file, and don't run grabber
      // /configure - run the grabber with the --configure option
      bool FileNext = false;
      foreach (string s in options)
      {
        if (FileNext)
        {
          if (File.Exists(s))
          {
            FileToImport = s;
          }
          else
          {
            //LoadFromFile=false;
            Log.Info("TVGuideScheduler: /file option but invalid file name " + s);
          }
          FileNext = false;
        }

        if (s.ToUpper() == "/FILE")
        {
          LoadFromFile = true;
          FileNext = true;
        }
        if (s.ToUpper() == "/CONFIGURE") RunConfig = true;
      }

      if (LoadFromFile)
      {
        if (File.Exists(FileToImport))
        {
          XMLTVImport importMulti = new XMLTVImport();
          importMulti.Import(FileToImport, false);
        }
        else Log.Info("TVGuideScheduler: /file option but no filename ");

      }
      else
      {
        //for users not using XMLTV so they can import their listings into database 
        if (grabber.ToLower() == "tvguide.xml file")
        {
          //check file is not empty then import file into database
          FileInfo file = new FileInfo(path + "\\TVguide.xml");
          if (file.Length > 250) //to indicate that it contains some data
          {
            XMLTVImport importFile = new XMLTVImport();
            importFile.Import(path + "\\TVGuide.xml", false);
          }
          else
          {
            Log.Info("TVGuideScheduler: XML file is empty - " + file);
          }
        }
        else
        {
          Grabber grab = new Grabber(grabber.ToLower());
          string strEXEpath = System.IO.Path.GetFullPath(grab.Output);

          //check if there is a .conf file for the grabber, or /configure on command line
          //if not found run xmltv.exe with the --configure to set channels
          if (grab.GrabberName != "tv_grab_nl_wolf")
          {
            if ((!File.Exists(grab.Output + "\\" + grab.GrabberName + ".conf")) || (RunConfig))
              XMLTVgrab.GrabberConfigure(grab.GrabberName, grab.Output);
            else
            { //check its size to see if its valid
              FileInfo conf = new FileInfo(grab.Output + "\\" + grab.GrabberName + ".conf");
              if (conf.Length < 10) //to indicate that it contains some data
              {
                File.Delete(grab.Output + "\\" + grab.GrabberName + ".conf");
                XMLTVgrab.GrabberConfigure(grab.GrabberName, grab.Output);
              }
            }
          }
          if (multiGrab == "yes")
          {
            grabberSingleDays = dayString.Split(new Char[] { ',' });
            XMLTVgrab.BuildThreads(grabberSingleDays, grab.GrabberName, grab.ConfigFile, strEXEpath, grab.Output, grab.Options, runGrabberLowPriority);
            // check file is not empty then import single day files into database
            foreach (string s in grabberSingleDays)
            {
              FileInfo file = new FileInfo(grab.Output + "\\TVguide" + System.Convert.ToInt32(s) + ".xml");
              if (file.Exists)
              {
                if (file.Length > 250) //to indicate that it contains some data
                {
                  XMLTVImport importSingle = new XMLTVImport();
                  importSingle.Import(grab.Output + "\\TVguide" + System.Convert.ToInt32(s) + ".xml", false);
                }
                else
                {
                  Log.Info("TVGuideScheduler: XML file is empty - " + file);
                }
              }
              else
              {
                Log.Error("TVGuideScheduler: No XML file is found in - " + grab.Output + "\\TVguide" + System.Convert.ToInt32(s) + ".xml");
              }
            }
          }
          else
          {
            TimeSpan daysInGuide;
            string lastGuideDate = TVDatabase.GetLastProgramEntry();
            if (lastGuideDate == "")
            {
              grabberDays = grab.GuideDays;
              daysInGuide = DateTime.Now - DateTime.Now;
            }
            else
            {
              DateTime lastProg = MediaPortal.Util.Utils.longtodate(System.Convert.ToInt64(lastGuideDate));
              daysInGuide = lastProg - DateTime.Now;
              grabberDays = grab.GuideDays - System.Convert.ToInt32(daysInGuide.Days);
            }
            int offset = 0;
            if (System.Convert.ToInt32(daysInGuide.Days) < 0)
            {
              Log.Info("TVGuideScheduler: /already more days in guide than configured, exiting");
              return;
            }
            if (System.Convert.ToInt32(daysInGuide.Days) == 0)
            {
              offset = System.Convert.ToInt32(daysInGuide.Days);
            }
            else
            {
              offset = System.Convert.ToInt32(daysInGuide.Days) + 1;
            }

            if (grab.GrabberName == "tv_grab_nl_wolf")
            {
              XMLTVgrab.RunGrabber(grab.GrabberName, strEXEpath, grab.Output, grabberDays, offset, runGrabberLowPriority);
            }
            else
            {
              XMLTVgrab.RunGrabber(grab.GrabberName, grab.ConfigFile, strEXEpath, grab.Output, grabberDays, offset, grab.Options, runGrabberLowPriority);
            }
            //check file is not empty then import multi day file into database
            FileInfo file = new FileInfo(grab.Output + "\\TVguide.xml");
            if (file.Length > 250) //to indicate that it contains some data
            {
              XMLTVImport importMulti = new XMLTVImport();
              importMulti.Import(grab.Output + "\\TVGuide.xml", false);
            }
            else
            {
              Log.Info("TVGuideScheduler: XML file is empty - " + file);
            }
          }
        }
      }
    }

  }

}
