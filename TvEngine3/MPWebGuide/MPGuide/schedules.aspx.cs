using System;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TvLibrary;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using Gentle.Common;
using Gentle.Framework;

public partial class schedules : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
    if (!Page.IsPostBack)
    {
      radioTitle.Checked = true;
    }
    UpdateSchedules();
  }

  void UpdateSchedules()
  {
    SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Schedule));
    if (radioTitle.Checked)
      sb.AddOrderByField(true, "programName");
    else
      sb.AddOrderByField(true, "startTime");
    SqlStatement stmt = sb.GetStatement(true);
    IList schedules = ObjectFactory.GetCollection(typeof(Schedule), stmt.Execute());
    TvBusinessLayer layer = new TvBusinessLayer();
    foreach (Schedule schedule in schedules)
    {
      AddSchedule(layer, schedule);
    }
  }

  void AddSchedule(TvBusinessLayer layer, Schedule schedule)
  {
    AddHeader(schedule);
    List<Schedule> series = layer.GetRecordingTimes(schedule, 14);
    for (int i = 0; i < series.Count; i++)
    {
      string title = schedule.ProgramName;
      IList programs = layer.GetPrograms(series[i].ReferencedChannel(), series[i].StartTime, series[i].EndTime);
      if (programs.Count > 0)
      {
        Program p = (Program)programs[0];
        if (p.Title == schedule.ProgramName)
        {
          AddRow(series[i], p, i == series.Count - 1);
        }
      }

    }
  }

  void AddHeader(Schedule rec)
  {
    HtmlTableRow row = new HtmlTableRow();
    HtmlTableCell cell = new HtmlTableCell();
    cell.Attributes.Add("class", "info_box_middle");
    cell.ColSpan = 2;
    string title = "";
    switch ((ScheduleRecordingType)rec.ScheduleType)
    {
      case ScheduleRecordingType.Once:
        title = String.Format("{0}", rec.ProgramName);
        break;
      case ScheduleRecordingType.Daily:
        title = String.Format("{0} daily", rec.ProgramName);
        break;
      case ScheduleRecordingType.Weekly:
        title = String.Format("{0} weekly", rec.ProgramName);
        break;
      case ScheduleRecordingType.Weekends:
        title = String.Format("{0} weekends", rec.ProgramName);
        break;
      case ScheduleRecordingType.WorkingDays:
        title = String.Format("{0} mon-fri", rec.ProgramName);
        break;
      case ScheduleRecordingType.EveryTimeOnEveryChannel:
        title = String.Format("{0} every time", rec.ProgramName);
        break;
      case ScheduleRecordingType.EveryTimeOnThisChannel:
        title = String.Format("{0} every time on {1}", rec.ProgramName, rec.ReferencedChannel().Name);
        break;
    }
    cell.InnerHtml = String.Format("<div style=\"padding-right: 4px; padding-left: 4px; padding-bottom: 4px; width: 100%; padding-top: 4px\"><span class=\"info_box_title_text\">{0} </span></div>", title);
    row.Cells.Add(cell);
    tableList.Rows.Add(row);
  }

  void AddRow(Schedule rec,Program prog, bool last )
  {
    HtmlTableRow rowBase = new HtmlTableRow();
    HtmlTableCell cellBase = new HtmlTableCell();
    cellBase.Attributes.Add("class", "info_box_middle");


    HtmlTable table = new HtmlTable();
    table.Attributes.Add("class", "grid_default");
    table.Style.Add("border-right", "#ffffff 1px solid");
    table.Style.Add("padding-right", "px");
    if (last)
    {
      table.Style.Add("border-top", "border-top: blue 0px solid");
      table.Style.Add("margin-bottom", "5px");
      table.Style.Add("padding-top", "0px");
      table.Style.Add("border-bottom", "#ffffff 1px solid");
    }
    else
    {
      table.Style.Add("border-top", "#ffffff 1px solid");
      table.Style.Add("margin-bottom", "0px");
      table.Style.Add("padding-top", "2px");
      table.Style.Add("border-bottom", "blue 0px solid");
    }
    table.Style.Add("padding-left", "3px");
    table.Style.Add("border-left", "#ffffff 1px solid");
    table.Style.Add("width", "100%");
    table.CellPadding = 0;
    table.CellSpacing = 0;

    HtmlTableRow row = new HtmlTableRow();
    row.Align = "top";

    HtmlTableCell td1 = new HtmlTableCell();
    HtmlTableCell td2 = new HtmlTableCell();
    HtmlTableCell td3 = new HtmlTableCell();

    td1.Attributes.Add("class", "recording_list_text");
    td1.Style.Add("padding-right", "4px");
    td1.Style.Add("padding-left", "4px");
    td1.Style.Add("padding-bottom", "4px");
    td1.Style.Add("padding-top", "4px");
    td1.Style.Add("width", "85%");
    td1.Style.Add("border-bottom", "#304a66 1px solid%");



    td1.InnerHtml = string.Format("<a class=\"recording_list_text\" href=\"javascript:loadInfo('recorded','1397')\">\"{0} - \"{1}</a>&nbsp;", rec.ProgramName, prog.Description);


    td2.Style.Add("width", "40px");
    td2.Style.Add("border-bottom", "#304a66 1px solid");
    td2.InnerHtml = "&nbsp; &nbsp;";


    td3.Attributes.Add("class", "recording_list_text");
    td3.Style.Add("padding-right", "4px");
    td3.Style.Add("padding-left", "4px");
    td3.Style.Add("padding-bottom", "4px");
    td3.Style.Add("padding-top", "4px");
    td3.Style.Add("width", "50px");
    td3.Style.Add("border-bottom", "#304a66 1px solid%");
    td3.Align = "right";
    td3.InnerHtml = String.Format("<nobr>{0}</nobr>", rec.StartTime.ToString("ddd d MMM"));
    row.Cells.Add(td1);
    row.Cells.Add(td2);
    row.Cells.Add(td3);
    table.Rows.Add(row);

    cellBase.Controls.Add(table);
    rowBase.Cells.Add(cellBase);
    tableList.Rows.Add(rowBase);
  }
  protected void radioTitle_CheckedChanged(object sender, EventArgs e)
  {
    radioDate.Checked = false;

  }
  protected void radioDate_CheckedChanged(object sender, EventArgs e)
  {
    radioTitle.Checked = false;

  }
}
