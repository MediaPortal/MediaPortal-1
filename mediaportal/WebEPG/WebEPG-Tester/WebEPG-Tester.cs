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

using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using MediaPortal.Utils.Web;
using MediaPortal.Utils.Services;
using MediaPortal.EPG;
using MediaPortal.WebEPG;
using MediaPortal.TV.Database;
using MediaPortal.EPG.config;

namespace MediaPortal.EPG.WebEPGTester
{
  public class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]

    static void Main()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      ILog log = new Log("WebEPG-Tester", Log.Level.Debug);

      StringBuilder sb = new StringBuilder();
      StringWriter logString = new StringWriter(sb);
      ILog webepgLog = new Log(logString, Log.Level.Debug);
      services.Add<ILog>(webepgLog);
      ChannelsList config = new ChannelsList(Environment.CurrentDirectory + "\\WebEPG");

      string testDir = Environment.CurrentDirectory + "\\test";

      MediaPortal.Webepg.Profile.Xml xmlreader = new MediaPortal.Webepg.Profile.Xml(testDir + "\\GrabberTests.xml");

      HTMLCache.WebCacheInitialise();

      if (!System.IO.Directory.Exists(testDir))
        System.IO.Directory.CreateDirectory(testDir);

      string[] countries = config.GetCountries();

      if (countries != null)
      {
        for (int c = 0; c < countries.Length; c++)
        {
          string countryDir = testDir + "\\" + countries[c];
          if (!System.IO.Directory.Exists(countryDir))
            System.IO.Directory.CreateDirectory(countryDir);

          string[] grabbers = config.GrabberList(countries[c]);

          for (int g = 0; g < grabbers.Length; g++)
          {
            string grabberDir = countryDir + "\\" + grabbers[g];
            //if (!System.IO.Directory.Exists(grabberDir))
            //  System.IO.Directory.CreateDirectory(grabberDir);

            SortedList channels = config.GetChannelList(countries[c], grabbers[g]);

            IDictionaryEnumerator enumerator = channels.GetEnumerator();
            WebListingGrabber m_EPGGrabber = new WebListingGrabber(14, Environment.CurrentDirectory + "\\WebEPG\\grabbers\\");

            log.Info("WebEPG: Grabber {0}\\{1}", countries[c], grabbers[g]);

            //MediaPortal.Webepg.Profile.Xml grabReader = new MediaPortal.Webepg.Profile.Xml(Environment.CurrentDirectory + "\\WebEPG\\grabbers\\" + countries[c] + "\\" + grabbers[g]);
            //int siteGuideDays = grabReader.GetValueAsInt("Info", "GuideDays", 0);

            XMLTVExport xmltv = new XMLTVExport(grabberDir);
            xmltv.Open();

            LogFileWriter countryLog = new LogFileWriter("log", countries[c] + "-" + grabbers[g]);
            while (enumerator.MoveNext())
            {
            //if (enumerator.MoveNext())
            //{
              ChannelInfo channel = (ChannelInfo)enumerator.Value;
              xmltv.WriteChannel(channel.ChannelID, channel.FullName);
              log.Info("WebEPG: Getting Channel {0}", channel.ChannelID);

              string countryGrabber = countries[c] + "\\" + grabbers[g];
              string grabTimeStr = xmlreader.GetValueAsString("Grabbers", countryGrabber, "");
              DateTime grabDateTime;
              //grabDateTime = DateTime.Now.AddDays(siteGuideDays);
              if (grabTimeStr == "")
              {
                grabDateTime = DateTime.Now;
                long dtLong = GetLongDateTime(grabDateTime);
                xmlreader.SetValue("Grabbers", countryGrabber, dtLong.ToString());
                xmlreader.Save();
                HTMLCache.CacheMode = HTMLCache.Mode.Replace;
              }
              else
              {
                grabDateTime = GetDateTime(long.Parse(grabTimeStr));
                HTMLCache.CacheMode = HTMLCache.Mode.Enabled;
              }

              if (m_EPGGrabber.Initalise(countryGrabber))
              {
                ArrayList programs = m_EPGGrabber.GetGuide(channel.ChannelID, false, 0, 23, grabDateTime);
                if (programs != null)
                {
                  for (int p = 0; p < programs.Count; p++)
                  {
                    xmltv.WriteProgram((TVProgram)programs[p], 0);
                  }

                }
              }
              else
              {
                log.Error("WebEPG: Grabber failed for: {0}", channel.ChannelID);
              }
              if (logString.ToString().IndexOf("[ERROR]") != -1)
              {
                log.Error("WebEPG: Grabber error for: {0}", channel.ChannelID);
                logString.Flush();
                countryLog.Write(sb.ToString());
                countryLog.Flush();
              }
              sb.Remove(0, sb.Length);
            }
            xmltv.Close();
            countryLog.Close();
          }
        }
      }
    }

    static private long GetLongDateTime(DateTime dt)
    {
      long lDatetime;

      lDatetime = dt.Year;
      lDatetime *= 100;
      lDatetime += dt.Month;
      lDatetime *= 100;
      lDatetime += dt.Day;
      lDatetime *= 100;
      lDatetime += dt.Hour;
      lDatetime *= 100;
      lDatetime += dt.Minute;
      lDatetime *= 100;
      // no seconds

      return lDatetime;
    }

    static private DateTime GetDateTime(long ldatetime)
    {
      int sec = (int)(ldatetime % 100L); ldatetime /= 100L;
      int minute = (int)(ldatetime % 100L); ldatetime /= 100L;
      int hour = (int)(ldatetime % 100L); ldatetime /= 100L;
      int day = (int)(ldatetime % 100L); ldatetime /= 100L;
      int month = (int)(ldatetime % 100L); ldatetime /= 100L;
      int year = (int)ldatetime;

      if (day < 0 || day > 31) throw new ArgumentOutOfRangeException();
      if (month < 0 || month > 12) throw new ArgumentOutOfRangeException();
      if (year < 1900 || year > 2100) throw new ArgumentOutOfRangeException();
      if (sec < 0 || sec > 59) throw new ArgumentOutOfRangeException();
      if (minute < 0 || minute > 59) throw new ArgumentOutOfRangeException();
      if (hour < 0 || hour > 23) throw new ArgumentOutOfRangeException();

      return new DateTime(year, month, day, hour, minute, 0, 0);
    }
  }
}