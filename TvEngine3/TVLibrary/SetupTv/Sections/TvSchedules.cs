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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using TvControl;
using DirectShowLib;

using Gentle.Common;
using Gentle.Framework;
using TvDatabase;
using TvLibrary;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  public partial class TvSchedules : SectionSettings
  {
    public TvSchedules()
      : this("TV Schedules")
    {
    }

    public TvSchedules(string name)
      : base(name)
    {
      InitializeComponent();
    }
    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      LoadSchedules();
    }
    void LoadSchedules()
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      listView1.Items.Clear();
      IList schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules)
      {
        ListViewItem item = new ListViewItem(schedule.Priority.ToString());
        item.SubItems.Add(schedule.ReferencedChannel().DisplayName);
        item.Tag = schedule;
        switch ((ScheduleRecordingType)schedule.ScheduleType)
        {
          case ScheduleRecordingType.Daily:
            item.ImageIndex = 0;
            item.SubItems.Add("Daily");
            item.SubItems.Add(String.Format("{0}", schedule.StartTime.ToString("HH:mm:ss", mmddFormat)));
            break;
          case ScheduleRecordingType.Weekly:
            item.ImageIndex = 0;
            item.SubItems.Add("Weekly");
            item.SubItems.Add(String.Format("{0} {1}", schedule.StartTime.DayOfWeek.ToString(), schedule.StartTime.ToString("HH:mm:ss", mmddFormat)));
            break;
          case ScheduleRecordingType.Weekends:
            item.ImageIndex = 0;
            item.SubItems.Add("Weekends");
            item.SubItems.Add(String.Format("{0}", schedule.StartTime.ToString("HH:mm:ss", mmddFormat)));
            break;
          case ScheduleRecordingType.WorkingDays:
            item.ImageIndex = 0;
            item.SubItems.Add("Mon-Fri");
            item.SubItems.Add(String.Format("{0}", schedule.StartTime.ToString("HH:mm:ss", mmddFormat)));
            break;
          case ScheduleRecordingType.Once:
            item.ImageIndex = 1;
            item.SubItems.Add("Once");
            item.SubItems.Add(String.Format("{0}", schedule.StartTime.ToString("dd-MM-yyyy HH:mm:ss", mmddFormat)));
            break;
          case ScheduleRecordingType.EveryTimeOnThisChannel:
            item.ImageIndex = 0;
            item.SubItems.Add("Always");
            item.SubItems.Add("");
            break;
          case ScheduleRecordingType.EveryTimeOnEveryChannel:
            item.ImageIndex = 0;
            item.SubItems.Add("Always");
            item.SubItems.Add("All channels");
            break;
        }
        item.SubItems.Add(schedule.ProgramName);
        item.SubItems.Add(String.Format("{0} mins", schedule.PreRecordInterval));
        item.SubItems.Add(String.Format("{0} mins", schedule.PostRecordInterval));

        if (schedule.MaxAirings.ToString() == int.MaxValue.ToString())
          item.SubItems.Add("Keep all");
        else
          item.SubItems.Add(schedule.MaxAirings.ToString());

        if (schedule.IsSerieIsCanceled(schedule.StartTime))
        {
          item.Font = new Font(item.Font, FontStyle.Strikeout);
        }
        listView1.Items.Add(item);
      }

      listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
    {
      mpButtonDel_Click(null, null);
    }

    private void mpButtonDel_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in listView1.SelectedItems)
      {
        Schedule schedule = (Schedule)item.Tag;
        TvServer server = new TvServer();
        server.StopRecordingSchedule(schedule.IdSchedule);
        schedule.Delete();

        listView1.Items.Remove(item);
      }
      listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

  }
}
