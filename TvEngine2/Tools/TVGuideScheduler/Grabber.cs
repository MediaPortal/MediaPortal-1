#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System.IO;
using MediaPortal.Configuration;
using MediaPortal.Profile;

namespace MediaPortal.TVGuideScheduler
{
  public class Grabber
  {
    private string m_strGrabberName = "";
    private string m_strConfigFile = "";
    private string m_strOutput = "";
    private int m_intDays;
    private string m_strOptions = "";
    private int m_intOffset;
    private int m_intGuidedays;


    public Grabber(string xmltvGrabberName)
    {
      m_strGrabberName = xmltvGrabberName;
      GetSettings();
      SetDefaults(m_strGrabberName);
    }

    public string GrabberName
    {
      get { return m_strGrabberName; }
    }

    public string ConfigFile
    {
      get { return m_strConfigFile; }
      set { m_strConfigFile = value; }
    }

    public string Output
    {
      get { return m_strOutput; }
      set { m_strOutput = value; }
    }

    public int Days
    {
      get { return m_intDays; }
      set { m_intDays = value; }
    }

    public int Offset
    {
      get { return m_intOffset; }
      set { m_intOffset = value; }
    }

    public string Options
    {
      get { return m_strOptions; }
      set { m_strOptions = value; }
    }

    public int GuideDays
    {
      get { return m_intGuidedays; }
      set { m_intGuidedays = value; }
    }

    private void SetDefaults(string Grabber)
    {
      switch (Grabber)
      {
        case "tv_grab_de_tvtoday":
          //    tv_grab_de_tvtoday [--config-file FILE] --configure
          //    tv_grab_de_tvtoday [--config-file FILE] [--output FILE] [--days N] [--offset N] [--quiet] [--slow] [--nosqueezeout]
          //    tv_grab_de_tvtoday --list-channels
          m_strConfigFile = "tv_grab_de_tvtoday.conf";
          m_intDays = 7;
          m_intOffset = 0;
          break;
        case "tv_grab_dk":
          //    tv_grab_dk [--config-file FILE] --configure
          //    tv_grab_dk [--config-file FILE] [--output FILE] [--days N] [--offset N] [--quiet]
          m_strConfigFile = "tv_grab_dk.conf";
          m_intDays = 7;
          m_intOffset = 0;
          break;
        case "tv_grab_es":
          m_strConfigFile = "tv_grab_es.conf";
          m_intDays = 3;
          m_intOffset = 0;
          break;
        case "tv_grab_es_digital":
          m_strConfigFile = "tv_grab_es_digital.conf";
          m_intDays = 3;
          m_intOffset = 0;
          break;
        case "tv_grab_fi":
          m_strConfigFile = "tv_grab_fi.conf";
          m_intDays = 10;
          m_intOffset = 0;
          break;
        case "tv_grab_fr":
          //    tv_grab_fr --configure [--config-file FILE] To grab
          //    tv_grab_fr [--config-file FILE] [--output FILE] [--days N] [--offset N] [--quiet] [--slow]
          m_strConfigFile = "tv_grab_fr.conf";
          m_intDays = 7;
          m_intOffset = 0;
          break;
        case "tv_grab_hr":
          m_strConfigFile = "tv_grab_hr.conf";
          m_intDays = 7;
          m_intOffset = 0;
          break;
        case "tv_grab_huro":
          m_strConfigFile = "tv_grab_huro.conf";
          m_intDays = 8;
          m_intOffset = 0;
          break;
        case "tv_grab_it":
          m_strConfigFile = "tv_grab_it.conf";
          m_intDays = 7;
          m_intOffset = 0;
          break;
        case "tv_grab_it_lt":
          m_strConfigFile = "tv_grab_it_lt.conf";
          m_intDays = 7;
          m_intOffset = 0;
          if (m_strOptions.IndexOf("password", 0) > 0)
          {
            m_strOptions = m_strOptions + " " + m_strOutput + "tv_grab_it_lt_password.txt";
          }
          break;
        case "tv_grab_na_dd":
          m_strConfigFile = "tv_grab_na_dd.conf";
          m_intDays = 7;
          m_intOffset = 0;
          break;
        case "tv_grab_nl":
          //    tv_grab_nl [--config-file FILE] --configure
          //    tv_grab_nl [--config-file FILE] [--output FILE] [--days N] [--offset N] [--quiet] [--slow]
          m_strConfigFile = "tv_grab_nl.conf";
          m_intDays = 7;
          m_intOffset = 0;
          break;
        case "tv_grab_nl_wolf":

          break;
        case "tv_grab_no_gfeed":
          m_strConfigFile = "tv_grab_no_gfeed.conf";
          m_intDays = 7;
          m_intOffset = 0;
          break;
        case "tv_grab_pt":
          m_strConfigFile = "tv_grab_pt.conf";
          m_intDays = 7;
          m_intOffset = 0;
          break;
        case "tv_grab_se":
          m_strConfigFile = "tv_grab_se.conf";
          m_intDays = 5;
          m_intOffset = 0;
          break;
        case "tv_grab_se_swedb":
          m_strConfigFile = "tv_grab_se_swedb.conf";
          m_intDays = 5;
          m_intOffset = 0;
          break;
        case "tv_grab_uk_rt":
          //    tv_grab_uk_rt [--config-file FILE] --configure
          //    tv_grab_uk_rt [--config-file FILE] [--output FILE] [--days N] [--offset N] [--slow] [--limit-details HH:MM-HH:MM]
          //                  [--get-categories] [--quiet]
          m_strConfigFile = "tv_grab_uk_rt.conf";
          m_intDays = 14;
          m_intOffset = 0;
          break;
        case "tv_grab_uk_bleb":
          m_strConfigFile = "tv_grab_uk_bleb.conf";
          m_intDays = 7;
          m_intOffset = 0;
          break;
      }
    }

    private void GetSettings()
    {
      string grabberOutput = null;
      using (Settings xmlreader = new MPSettings())
      {
        grabberOutput = xmlreader.GetValueAsString("xmltv", "folder", "xmltv");
        m_intGuidedays = xmlreader.GetValueAsInt("xmltv", "daystokeep", 7);
        m_strOptions = xmlreader.GetValueAsString("xmltv", "args", "");
      }
      m_strOutput = Path.GetFullPath(grabberOutput);
    }
  }
}