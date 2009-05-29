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

using System;
using System.DirectoryServices;
using System.Windows.Forms;
using TaskScheduler;

namespace MediaPortal.Configuration
{
  internal class TaskScheduler
  {
    public static bool GetTask(ref short[] taskSettings, ref string userAccount)
    {
      bool taskfound = false;
      ScheduledTasks st = new ScheduledTasks();
      string[] taskNames = st.GetTaskNames();
      foreach (string name in taskNames)
      {
        Task t = st.OpenTask(name);
        if (t != null && t.Name.StartsWith("MPGuideScheduler"))
        {
          taskfound = true;
          try
          {
            userAccount = t.AccountName;
          }
          catch (Exception)
          {
            userAccount = "";
          }


          foreach (Trigger tr in t.Triggers)
          {
            //Console.WriteLine("    " + tr.ToString());
            if (tr is DailyTrigger)
            {
              taskSettings[0] = (tr as DailyTrigger).StartHour;
              taskSettings[1] = (tr as DailyTrigger).StartMinute;
              taskSettings[2] = (tr as DailyTrigger).DaysInterval;
            }
          }
          t.Close();
        }
      }
      if (!taskfound)
      {
        taskSettings[0] = 01;
        taskSettings[1] = 00;
        taskSettings[2] = 1;
        st.Dispose();
        return false;
      }
      else
      {
        st.Dispose();
        return true;
      }
    }

    public static void CreateTask(short hour, short minute, short frequency, string user, string password)
    {
      //check if the user exists
      try
      {
        // maybe a bug, but the DirectoryEntry.exists does not seem to work
        // and instead of returning when user isn't found throws exception?
        if (!DirectoryEntry.Exists("WinNT://" + Environment.MachineName + "/" + user + ",user"))
        {
          //this should run if user not found, instead goes to catch
          user = "";
          password = "";
        }
      }
      catch (Exception ex)
      {
        if (ex.Message == "The user name could not be found")
        {
          user = "";
          password = "";
        }
        else
        {
          Console.WriteLine(ex.Message);
          Console.ReadLine();
        }
      }
      ScheduledTasks st = new ScheduledTasks();
      Task t = st.OpenTask("MPGuideScheduler");
      // if the task exists - modify it
      if (t != null)
      {
        if (user != null && user != "" && password != null && password != "")
        {
          t.SetAccountInformation(user, password);
        }
        t.Comment = "MediaPortal TV Guide Download Scheduler";
        foreach (Trigger tr in t.Triggers)
        {
          (tr as DailyTrigger).StartHour = hour;
          (tr as DailyTrigger).StartMinute = minute;
          (tr as DailyTrigger).DaysInterval = frequency;
        }
        t.Save();
        t.Close();
      }
        // or create a new scheduled task
      else
      {
        try
        {
          string path = Application.StartupPath;
          t = st.CreateTask("MPGuideScheduler");
          t.ApplicationName = path + @"\TVGuideScheduler.exe";
          t.WorkingDirectory = path;
          t.Comment = "MediaPortal TV Guide Download Scheduler";
          t.SetAccountInformation(user, password);
          t.MaxRunTime = new TimeSpan(72, 0, 0);
          t.Triggers.Add(new DailyTrigger(hour, minute, frequency));
          t.Save();
          t.Close();
        }
        catch (Exception)
        {
        }
      }
      st.Dispose();
    }

    public static void DeleteTask()
    {
      ScheduledTasks st = new ScheduledTasks();
      Task t = st.OpenTask("MPGuideScheduler");
      // if the task exists - delete it
      if (t != null)
      {
        st.DeleteTask("MPGuideScheduler");
      }
      st.Dispose();
    }
  }
}