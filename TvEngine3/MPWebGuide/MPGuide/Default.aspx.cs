using System;
using System.Configuration;
using System.Collections;
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

public partial class _Default : System.Web.UI.Page
{
  IList _recordings;
  protected void Page_Load(object sender, EventArgs e)
  {
    try
    {
      _recordings = Recording.ListAll();
    }
    catch (Exception)
    {
      Response.Redirect("install/default.aspx");
      return;
    }
    labelDate.Text = DateTime.Now.ToLongDateString();
    labelTime.Text = DateTime.Now.ToShortTimeString();
    UpdateRecentRecordings();
    UpdateSchedule();
  }
  void UpdateRecentRecordings()
  {
    SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Recording));
    sb.AddOrderByField(false, "startTime");
    SqlStatement stmt = sb.GetStatement(true);
    IList _recordings = ObjectFactory.GetCollection(typeof(Recording), stmt.Execute());
    
    int count = 0;
    foreach (Recording rec in _recordings)
    {
      AddRecording(rec);
      count++;
      if (count > 6) break;
    }
  }

  void AddRecording(Recording rec)
  {
    HtmlTableRow baseRow = new HtmlTableRow();
    HtmlTableCell cellBase = new HtmlTableCell();
    cellBase.Attributes.Add("class", "recording_list_middle");

    HtmlTable subTable = new HtmlTable();
    subTable.Attributes.Add("class", "grid_default");
    subTable.CellPadding = 0;
    subTable.CellSpacing = 0;
    subTable.Style.Add("border-right", "#ffffff 1px solid");
    subTable.Style.Add("padding-right", "3px");
    subTable.Style.Add("border-top", "#ffffff 1px solid");
    subTable.Style.Add("padding-left", "3px");
    subTable.Style.Add("margin-bottom", "0px");
    subTable.Style.Add("border-left", "#ffffff 1px solid");
    subTable.Style.Add("width", "100%");
    subTable.Style.Add("padding-top", "2px");
    subTable.Style.Add("border-bottom", "blue 0px solid");

    HtmlTableRow subRow = new HtmlTableRow();
    subRow.VAlign = "top";
    HtmlTableCell td1 = new HtmlTableCell();
    HtmlTableCell td2 = new HtmlTableCell();
    HtmlTableCell td3 = new HtmlTableCell();
    td1.Attributes.Add("class", "recording_list_text");
    td1.Style.Add("padding-right", "4px");
    td1.Style.Add("padding-left", "4px");
    td1.Style.Add("padding-bottom", "4px");
    td1.Style.Add("width", "100%");
    td1.Style.Add("padding-top", "4px");
    td1.Style.Add("border-bottom", "#304a66 1px solid");
    td1.InnerHtml = String.Format("<a class=\"recording_list_text\" href=\"\">{0}</a>&nbsp;&nbsp;", rec.Title);

    td2.Style.Add("border-bottom", "#304a66 1px solid");
    td2.InnerHtml = "&nbsp;&nbsp;";
    td3.Style.Add("padding-right", "4px");
    td3.Style.Add("padding-left", "4px");
    td3.Style.Add("padding-bottom", "4px");
    td3.Style.Add("width", "100%");
    td3.Style.Add("padding-top", "4px");
    td3.Style.Add("border-bottom", "#304a66 1px solid");
    td3.InnerHtml = String.Format("<nobr>{0}</nobr>", rec.StartTime.ToShortDateString());


    subRow.Cells.Add(td1);
    subRow.Cells.Add(td2);
    subRow.Cells.Add(td3);
    subTable.Rows.Add(subRow);
    cellBase.Controls.Add(subTable);
    baseRow.Cells.Add(cellBase);
    tableRecordings.Rows.Add(baseRow);

  }
  void UpdateSchedule()
  {
    int count = 0;
    IList schedules = Schedule.ListAll();
    foreach (Schedule rec in schedules)
    {
      AddSchedule(rec);
      count++;
      if (count > 6) break;
    }
  }

  void AddSchedule(Schedule rec)
  {
    HtmlTableRow baseRow = new HtmlTableRow();
    HtmlTableCell cellBase = new HtmlTableCell();
    cellBase.Attributes.Add("class", "recording_list_middle");

    HtmlTable subTable = new HtmlTable();
    subTable.Attributes.Add("class", "grid_default");
    subTable.CellPadding = 0;
    subTable.CellSpacing = 0;
    subTable.Style.Add("border-right", "#ffffff 1px solid");
    subTable.Style.Add("padding-right", "3px");
    subTable.Style.Add("border-top", "#ffffff 1px solid");
    subTable.Style.Add("padding-left", "3px");
    subTable.Style.Add("margin-bottom", "0px");
    subTable.Style.Add("border-left", "#ffffff 1px solid");
    subTable.Style.Add("width", "100%");
    subTable.Style.Add("padding-top", "2px");
    subTable.Style.Add("border-bottom", "blue 0px solid");

    HtmlTableRow subRow = new HtmlTableRow();
    subRow.VAlign = "top";
    HtmlTableCell td1 = new HtmlTableCell();
    HtmlTableCell td2 = new HtmlTableCell();
    HtmlTableCell td3 = new HtmlTableCell();
    td1.Attributes.Add("class", "recording_list_text");
    td1.Style.Add("padding-right", "4px");
    td1.Style.Add("padding-left", "4px");
    td1.Style.Add("padding-bottom", "4px");
    td1.Style.Add("width", "100%");
    td1.Style.Add("padding-top", "4px");
    td1.Style.Add("border-bottom", "#304a66 1px solid");
    td1.InnerHtml = String.Format("<a class=\"recording_list_text\" href=\"\">{0}</a>&nbsp;&nbsp;", rec.ProgramName);

    td2.Style.Add("border-bottom", "#304a66 1px solid");
    td2.InnerHtml = "&nbsp;&nbsp;";
    td3.Style.Add("padding-right", "4px");
    td3.Style.Add("padding-left", "4px");
    td3.Style.Add("padding-bottom", "4px");
    td3.Style.Add("width", "100%");
    td3.Style.Add("padding-top", "4px");
    td3.Style.Add("border-bottom", "#304a66 1px solid");
    td3.InnerHtml = String.Format("<nobr>{0}</nobr>", rec.StartTime.ToShortDateString());


    subRow.Cells.Add(td1);
    subRow.Cells.Add(td2);
    subRow.Cells.Add(td3);
    subTable.Rows.Add(subRow);
    cellBase.Controls.Add(subTable);
    baseRow.Cells.Add(cellBase);
    tableSchedules.Rows.Add(baseRow);

  }
}
