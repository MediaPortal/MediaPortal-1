#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

using Mediaportal.TV.Server.RuleBasedScheduler;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.SetupTV.Dialogs;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVService.ServiceAgents;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class TvSchedules : SectionSettings
  {
    private readonly MPListViewStringColumnSorter lvwColumnSorter;

    private bool _ignoreRefreshEPG = true;

    public TvSchedules()
      : this("Schedules") { }

    public TvSchedules(string name)
      : base(name)
    {
      InitializeComponent();
      lvwColumnSorter = new MPListViewStringColumnSorter();
      lvwColumnSorter.Order = SortOrder.None;
      listView1.ListViewItemSorter = lvwColumnSorter;
    }

    public override void OnSectionActivated()
    {
      try
      {
        _ignoreRefreshEPG = true;
        base.OnSectionActivated();
        contextMenuStrip1.Items[0].Visible = true;
        contextMenuStrip1.Items[1].Visible = false;
        LoadSchedules();
        AddGroups();
        RefreshEPG();
        RefreshTemplates();
        SetupScheduleTemplatesMenuItems();
      }
      finally
      {
        _ignoreRefreshEPG = false;        
      }      
    }

    private void SetupScheduleTemplatesMenuItems()
    {
      IList<ScheduleRulesTemplate> templates = ServiceAgents.Instance.ScheduleServiceAgent.ListAllScheduleRules();
      foreach (var template in templates)
      {
        ToolStripItem toolStripItem = new ToolStripButton();
        toolStripItem.Tag = template;
        toolStripItem.Text = template.ToString();
        toolStripItem.Click += new EventHandler(toolStripItem_Click);
        addScheduleByTemplateToolStripMenuItem.DropDown.Items.Add(toolStripItem); 
      }      
    }

    private void toolStripItem_Click(object sender, EventArgs e)
    {
      if (listView2.SelectedItems.Count > 0)
      {
        var program = listView2.SelectedItems[0].Tag as Program;
        if (program != null)
        {
          var item = sender as ToolStripItem;
          if (item != null)
          {
            var template = item.Tag as ScheduleRulesTemplate;
            ShowEditScheduleDialogue(program, null, template);
          }
        }
      }
    }

    private void AddGroups()
    {
      comboBoxGroups.Items.Clear();
      IList<ChannelGroup> groups = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroups();
      foreach (ChannelGroup group in groups)
        comboBoxGroups.Items.Add(new ComboBoxExItem(group.groupName, -1, group.idGroup));
      if (comboBoxGroups.Items.Count == 0)
        comboBoxGroups.Items.Add(new ComboBoxExItem("(no groups defined)", -1, -1));
      comboBoxGroups.SelectedIndex = 0;
    }

    private void LoadSchedules()
    {

      listView1.BeginUpdate();

      try
      {
        IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
        listView1.Items.Clear();
        IList<Schedule> schedules = ServiceAgents.Instance.ScheduleServiceAgent.ListAllSchedules();
        foreach (Schedule schedule in schedules)
        {
          ListViewItem item = new ListViewItem(schedule.priority.ToString());
          item.SubItems.Add(schedule.Channel.displayName);
          item.Tag = schedule;
          switch ((ScheduleRecordingType)schedule.scheduleType)
          {
            case ScheduleRecordingType.Daily:
              item.ImageIndex = 0;
              item.SubItems.Add("Daily");
              item.SubItems.Add(String.Format("{0}", schedule.startTime.ToString("HH:mm:ss", mmddFormat)));
              break;
            case ScheduleRecordingType.Weekly:
              item.ImageIndex = 0;
              item.SubItems.Add("Weekly");
              item.SubItems.Add(String.Format("{0} {1}", schedule.startTime.DayOfWeek,
                                              schedule.startTime.ToString("HH:mm:ss", mmddFormat)));
              break;
            case ScheduleRecordingType.Weekends:
              item.ImageIndex = 0;
              item.SubItems.Add("Weekends");
              item.SubItems.Add(String.Format("{0}", schedule.startTime.ToString("HH:mm:ss", mmddFormat)));
              break;
            case ScheduleRecordingType.WorkingDays:
              item.ImageIndex = 0;
              item.SubItems.Add("WorkingDays");
              item.SubItems.Add(String.Format("{0}", schedule.startTime.ToString("HH:mm:ss", mmddFormat)));
              break;
            case ScheduleRecordingType.Once:
              item.ImageIndex = 1;
              item.SubItems.Add("Once");
              item.SubItems.Add(String.Format("{0}", schedule.startTime.ToString("dd-MM-yyyy HH:mm:ss", mmddFormat)));
              break;
            case ScheduleRecordingType.WeeklyEveryTimeOnThisChannel:
              item.ImageIndex = 0;
              item.SubItems.Add("Weekly Always");
              item.SubItems.Add(schedule.startTime.DayOfWeek.ToString());
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
          item.SubItems.Add(schedule.programName);
          item.SubItems.Add(String.Format("{0} mins", schedule.preRecordInterval));
          item.SubItems.Add(String.Format("{0} mins", schedule.postRecordInterval));

          if (schedule.maxAirings.ToString() == int.MaxValue.ToString())
            item.SubItems.Add("Keep all");
          else
            item.SubItems.Add(schedule.maxAirings.ToString());

          var scheduleBLL = new ScheduleBLL(schedule);
          if (scheduleBLL.IsSerieIsCanceled(schedule.startTime))
          {
            item.Font = new Font(item.Font, FontStyle.Strikeout);
          }
          listView1.Items.Add(item);
        }
        listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
      }
      finally 
      {
        listView1.EndUpdate();
      }
      
    }

    private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == lvwColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        lvwColumnSorter.Order = lvwColumnSorter.Order == SortOrder.Ascending
                                  ? SortOrder.Descending
                                  : SortOrder.Ascending;
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter.SortColumn = e.Column;
        lvwColumnSorter.Order = SortOrder.Ascending;
      }
      // Perform the sort with these new sort options.
      listView1.Sort();
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
        ServiceAgents.Instance.ControllerServiceAgent.StopRecordingSchedule(schedule.id_Schedule);
        ServiceAgents.Instance.ScheduleServiceAgent.DeleteSchedule(schedule.id_Schedule);

        listView1.Items.Remove(item);
      }
      listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
    {
      RefreshEPG();
  }

    private void RefreshTemplates()
    {
      IList<ScheduleRulesTemplate> templates = ServiceAgents.Instance.ScheduleServiceAgent.ListAllScheduleRulesTemplates();

      if (templates != null)
      {
        listViewTemplates.BeginUpdate();

        try
        {
          listViewTemplates.Items.Clear();
          foreach (ScheduleRulesTemplate template in templates)
          {
            var item = new ListViewItem();
            item.Tag = template;

            item.SubItems.Add(Convert.ToString(template.name));
            item.SubItems.Add(Convert.ToString(template.usages));
            item.SubItems.Add(Convert.ToString(template.editable));
            ScheduleConditionList rules = ScheduleConditionHelper.Deserialize<ScheduleConditionList>(template.rules);
            if (rules != null)
            {
              item.SubItems.Add(template.rules.ToString());
            }
            else
            {
              item.SubItems.Add("n/a");
            }


            item.Checked = template.enabled;

            listViewTemplates.Items.Add(item);
            listViewTemplates.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
          }
        }
        finally
        {
          listViewTemplates.EndUpdate();          
        }
      }
    }

    private void RefreshEPG()
    {

      /*using (var ctx = new TVDatabaseEntities())
      {
        ObjectSet<TVDatabaseModel.Channel> channels = ctx.Channels;        
        foreach (var channel in channels)
        {
          string a = channel.displayName;
        }


        IQueryable<TVDatabaseModel.Program> query = GetProgramsMatchingChannelName(ctx, "DR1");

        foreach (var program in query)
        {
          string a = program.title;
        }
      }*/

      if (_ignoreRefreshEPG)
      {
        return;
      }

      if (comboBoxChannels.Items.Count == 0)
      {
        return;
      }

      listView2.BeginUpdate();

      try
      {
        IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
        int channelId = ((ComboBoxExItem)comboBoxChannels.SelectedItem).Id;
        IList<Program> prgs = ServiceAgents.Instance.ProgramServiceAgent.RetrieveByChannelAndTimesInterval(channelId, dateTimePicker1.Value, dateTimePicker1.Value.AddDays(1));

        if (prgs != null)
        {
          listView2.Items.Clear();
          foreach (var prg in prgs)
          {
            var item = new ListViewItem(prg.title);
            item.SubItems.Add(prg.startTime.ToString("HH:mm:ss", mmddFormat));
            item.Tag = prg;

            item.SubItems.Add(prg.endTime.ToString("HH:mm:ss", mmddFormat));
            item.SubItems.Add(prg.description);

            item.SubItems.Add(prg.seriesNum);
            item.SubItems.Add(prg.episodeNum);
            item.SubItems.Add(prg.ProgramCategory.category);
            item.SubItems.Add(prg.originalAirDate.GetValueOrDefault(DateTime.MinValue).ToString("HH:mm:ss", mmddFormat));
            item.SubItems.Add(prg.classification);
            item.SubItems.Add(Convert.ToString(prg.starRating));
            item.SubItems.Add(Convert.ToString(prg.parentalRating));
            item.SubItems.Add(prg.episodeName);
            item.SubItems.Add(prg.episodePart);
            item.SubItems.Add("state");

            listView2.Items.Add(item);
            listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
          }
        }
      }
      finally
      {
        listView2.EndUpdate();
      }
    }

    /*public static IQueryable<TVDatabaseModel.Program> GetProgramsMatchingChannelName(TVDatabaseEntities ctx, string name)
    {
      var query = from s in ctx.Programs
                  where s.Channel.displayName.Equals(name)
                  select s;
      return query;
    }*/

    private void mpComboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      RefreshEPG();
    }

    private void comboBoxGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
      ComboBoxExItem idItem = (ComboBoxExItem)comboBoxGroups.Items[comboBoxGroups.SelectedIndex];
      comboBoxChannels.Items.Clear();
      if (idItem.Id == -1)
      {
        IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannels();
        foreach (Channel ch in channels)
        {
          if (ch.mediaType != (decimal) MediaTypeEnum.TV) continue;
          bool hasFta = false;
          bool hasScrambled = false;
          IList<TuningDetail> tuningDetails = ch.TuningDetails;
          foreach (TuningDetail detail in tuningDetails)
          {
            if (detail.freeToAir)
            {
              hasFta = true;
            }
            if (!detail.freeToAir)
            {
              hasScrambled = true;
            }
          }

          int imageIndex;
          if (hasFta && hasScrambled)
          {
            imageIndex = 5;
          }
          else if (hasScrambled)
          {
            imageIndex = 4;
          }
          else
          {
            imageIndex = 3;
          }
          ComboBoxExItem item = new ComboBoxExItem(ch.displayName, imageIndex, ch.idChannel);

          comboBoxChannels.Items.Add(item);
        }
      }
      else
      {
        ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetChannelGroup(idItem.Id);
        IList<GroupMap> maps = group.GroupMaps;
        bool hasScrambled = false;
        foreach (GroupMap map in maps)
        {
          Channel ch = map.Channel;
          bool hasFta = false;
          if (ch.mediaType != (decimal) MediaTypeEnum.TV)          
          hasScrambled = false;
          IList<TuningDetail> tuningDetails = ch.TuningDetails;
          foreach (TuningDetail detail in tuningDetails)
          {
            if (detail.freeToAir)
            {
              hasFta = true;
            }
            if (!detail.freeToAir)
            {
              hasScrambled = true;
            }
          }

          int imageIndex;
          if (hasFta && hasScrambled)
          {
            imageIndex = 5;
          }
          else if (hasScrambled)
          {
            imageIndex = 4;
          }
          else
          {
            imageIndex = 3;
          }
          ComboBoxExItem item = new ComboBoxExItem(ch.displayName, imageIndex, ch.idChannel);
          comboBoxChannels.Items.Add(item);
        }
      }
      if (comboBoxChannels.Items.Count > 0)
        comboBoxChannels.SelectedIndex = 0;
    }

    private void mpButton1_Click(object sender, EventArgs e)
    {

    }

    private void tabControl1_Selected(object sender, TabControlEventArgs e)
    {
      contextMenuStrip1.Items[0].Visible = (e.TabPageIndex == 0);
      contextMenuStrip1.Items[1].Visible = (e.TabPageIndex == 1);
      contextMenuStrip1.Items[2].Visible = (e.TabPageIndex == 1);
      contextMenuStrip1.Items[3].Visible = (e.TabPageIndex == 2);   
    }    

    private void addScheduleToolStripMenuItem_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in listView2.SelectedItems)
      {
        var program = item.Tag as Program;
        if (program != null)
        {
          ShowEditScheduleDialogue(program, null, null);
          break;
        }
      }
    }

    private static void ShowEditScheduleDialogue(Program program, Schedule schedule, ScheduleRulesTemplate template)
    {
      var dlg = new FormEditSchedule {Schedule = schedule, Program = program, ScheduleRulesTemplate = template};
      dlg.ShowDialog();
    }

    private void editScheduleTemplateToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
      foreach (ListViewItem item in listViewTemplates.SelectedItems)
      {
        var ruleBasedSchedule = item.Tag as RuleBasedSchedule;
        if (ruleBasedSchedule != null)
        {
          /*var dlg = new FormEditScheduleTemplate();
          dlg.Schedule = null;
          dlg.Program = ruleBasedSchedule;
          dlg.ShowDialog();
           */
          break;
        }
      }
    }

    private void mpButtonAddNewTemplate_Click(object sender, EventArgs e)
    {

    }
  }
}