using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TvDatabase;
using TvLibrary;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using Gentle.Common;
using Gentle.Framework;
using TvControl;
public partial class TvGuide : System.Web.UI.Page
{
  const int PIX_PER_MINUTE = 5;
  const int OFFSET_Y = 60;
  const int OFFSET_X = 200;
  const int ROW_HEIGHT = 26;

  IList _schedules;
  protected void Page_Load(object sender, EventArgs e)
  {
    if (!Page.IsPostBack)
    {
      UpdateGuide();
      divInfoBox.Visible = false;
      FillCombo();
    }
  }

  void ShowProgramInfo(int id)
  {
    Program prog = Program.Retrieve(id);
    labelTitle.Text = prog.Title;
    labelDescription.Text = prog.Description;
    labelStartEnd.Text = String.Format("{0}-{1}", prog.StartTime.ToString("HH:mm"), prog.EndTime.ToString("HH:mm"));
    labelChannel.Text = prog.ReferencedChannel().Name;
    labelGenre.Text = prog.Genre;
    imgLogo.Src = String.Format("logos/{0}.png", labelChannel.Text);
    divInfoBox.Visible = true;
  }

  void UpdateGuide()
  {

    try
    {
      _schedules = Schedule.ListAll();
    }
    catch (Exception)
    {
      Response.Redirect("install/default.aspx");
      return;
    }
    ChannelGroup group;
    List<Channel> tvChannels = new List<Channel>();
    if (Session["idGroup"] != null)
    {
      int id = (int)Session["idGroup"] ;
      group = ChannelGroup.Retrieve(id);
    }
    else
    {
      group = (ChannelGroup)ChannelGroup.ListAll()[0];
      Session["idGroup"] = group.IdGroup;
    }
    foreach(GroupMap groupMap in group.ReferringGroupMap())
    {
      Channel ch = groupMap.ReferencedChannel();
      if (ch.IsTv)
      {
        tvChannels.Add(ch);
      }
    }

    UpdateGuide(tvChannels);
  }
  
  void FillCombo()
  {
    ChannelGroup group;
    if (Session["idGroup"] != null)
    {
      int id = (int)Session["idGroup"] ;
      group = ChannelGroup.Retrieve(id);
    }
    else
    {
      group = (ChannelGroup)ChannelGroup.ListAll()[0];
      Session["idGroup"] = group.IdGroup;
    }

    IList groups = ChannelGroup.ListAll();
    int selected = 0;
    foreach (ChannelGroup group2 in groups)
    {
      DropDownListGroup.Items.Add(group2.GroupName);
      if (group2.GroupName == group.GroupName) selected = DropDownListGroup.Items.Count - 1;
    }
    DropDownListGroup.SelectedIndex = selected;

    DateTime now;
    if (Session["currentTime"] == null)
    {
      now = DateTime.Now;
      Session["currentTime"] = now;
    }
    now = (DateTime)Session["currentTime"];

    int min = now.Minute;
    if (min < 30) min = 0;
    else min = 30;
    now = now.AddMinutes(-now.Minute + min);
    now = now.AddSeconds(-now.Second);
    now = now.AddMilliseconds(-now.Millisecond);



    DateTime dt = new DateTime(2000, 1, 1, 0, 0, 0);
    DateTime dtEnd = dt.AddDays(1);
    while (dt < dtEnd)
    {
      dropDownTime.Items.Add(dt.ToShortTimeString());
      if (dt.Hour == now.Hour && dt.Minute == now.Minute) dropDownTime.SelectedIndex = dropDownTime.Items.Count - 1;
      dt = dt.AddMinutes(30);
    }
    DateTime dateNow = DateTime.Now;
    DateTime dateEnd = now.AddDays(14);
    while (dateNow < dateEnd)
    {
      dropDownDate.Items.Add(dateNow.ToLongDateString());
      if (dt.Date == now.Date) dropDownDate.SelectedIndex = dropDownDate.Items.Count - 1;
      dateNow = dateNow.AddDays(1);
    }
  }
  void UpdateGuide(List<Channel> tvChannels)
  {

    DateTime now;
    if (Session["currentTime"] == null)
    {
      now = DateTime.Now;
      Session["currentTime"] = now;
    }
    now = (DateTime)Session["currentTime"];

    int min = now.Minute;
    if (min < 30) min = 0;
    else min = 30;
    now = now.AddMinutes(-now.Minute + min);
    now = now.AddSeconds(-now.Second);
    now = now.AddMilliseconds(-now.Millisecond);



    spanClock.InnerText = DateTime.Now.ToShortTimeString();
    TvBusinessLayer layer = new TvBusinessLayer();


    DateTime end = now.AddHours(2);
    Dictionary<int, List<Program>> programs = layer.GetProgramsForAllChannels(now, end, tvChannels);

    List<Program> headers = new List<Program>();
    Channel ch = new Channel(now.ToShortDateString(), false, true, 0, DateTime.MinValue, false, DateTime.MinValue, 0, false, "", false);
    headers.Add(new Program(1, now.AddMinutes(0), now.AddMinutes(30), now.ToShortTimeString(), "", "", false));
    headers.Add(new Program(1, now.AddMinutes(30), now.AddMinutes(60), now.AddMinutes(30).ToShortTimeString(), "", "", false));
    headers.Add(new Program(1, now.AddMinutes(60), now.AddMinutes(90), now.AddMinutes(60).ToShortTimeString(), "", "", false));
    headers.Add(new Program(1, now.AddMinutes(90), now.AddMinutes(120), now.AddMinutes(90).ToShortTimeString(), "", "", false));
    ShowGuideForChannel(0, now, end, ch, headers);
    int count = 0;
    foreach (Channel channel in tvChannels)
    {
      if (programs.ContainsKey(channel.IdChannel))
      {
        ShowGuideForChannel(count + 1, now, end, channel, programs[channel.IdChannel]);
        count++;
      }
      //if (count > 15) break;
    }
  }

  void ShowGuideForChannel(int nr, DateTime startTime, DateTime endTime, Channel channel, List<Program> programs)
  {
    HtmlTableRow row = new HtmlTableRow();
    AddChannelRow(nr, startTime, endTime, channel, row, programs);

  }
  void AddChannelRow(int nr, DateTime now, DateTime end, Channel channel, HtmlTableRow row, List<Program> programs)
  {
    int posy = OFFSET_Y + nr * ROW_HEIGHT;
    HtmlGenericControl cellBase = new HtmlGenericControl();
    cellBase.Style.Add("background-color", "#0d4798");
    cellBase.Style.Add("position", "absolute");
    cellBase.Style.Add("left", "25px");
    cellBase.Style.Add("height", "25px");
    cellBase.Style.Add("width", "175px");
    cellBase.Style.Add("top", String.Format("{0}px", posy));

    HtmlGenericControl subTable = new HtmlGenericControl();
    if (nr == 0)
    {
      subTable.InnerHtml = String.Format("<nobr><span class=\"guide_title_text\">{0}</span></nobr>", channel.Name);
    }
    else
    {
      subTable.InnerHtml = String.Format("<span class=\"grid_channel_num\"  style=\"cursor: pointer\">{0}</span>", nr);
      subTable.InnerHtml += String.Format("<nobr><A class=grid_channel style=\"CURSOR: pointer\" href=\"ShowChannel.aspx?id={0}\">{1}</A></nobr>", channel.IdChannel, channel.Name);
    }

    cellBase.Controls.Add(subTable);
    divGuide.Controls.Add(cellBase);

    AddPrograms(nr, now, end, row, programs);
  }

  void AddPrograms(int nr, DateTime now, DateTime end, HtmlTableRow row, List<Program> programs)
  {
    //695 = 120min = 24x5min
    //
    int cellCount = 0;
    foreach (Program program in programs)
    {
      DateTime startTime = program.StartTime;
      if (startTime < now) startTime = now;
      DateTime endTime = program.EndTime;
      if (endTime > end) endTime = end;
      HtmlGenericControl cellBase = new HtmlGenericControl();
      if (DateTime.Now >= program.StartTime && DateTime.Now <= program.EndTime && nr > 0)
      {
        cellBase.Attributes.Add("class", "grid_kids");
      }
      else
      {
        cellBase.Attributes.Add("class", "grid_default");
      }
      cellBase.Attributes.Add("title", String.Format("{0} {1}-{2}\r\n{3}", program.Title, program.StartTime.ToShortTimeString(), program.EndTime.ToShortTimeString(), program.Description));

      TimeSpan ts = endTime - startTime;
      int width = (int)(ts.TotalMinutes * 6.5);
      ts = startTime - now;
      int posx = OFFSET_X + (int)(ts.TotalMinutes * 6.5);
      int posy = OFFSET_Y + nr * ROW_HEIGHT;
      cellBase.Style.Add("position", "absolute");
      cellBase.Style.Add("left", String.Format("{0}px", posx));
      cellBase.Style.Add("top", String.Format("{0}px", posy));
      cellBase.Style.Add("height", "25px");
      cellBase.Style.Add("width", String.Format("{0}px", width));
      cellBase.Style.Add("vertical-align", "middle");
      //HtmlGenericControl subRow = new HtmlGenericControl();
      //subRow.Style.Add("vertical-align", "middle");
      //HtmlGenericControl td1 = new HtmlGenericControl();
      //HtmlGenericControl td2 = new HtmlGenericControl();
      string html = "";
      if (program.StartTime < now)
      {
        //td1.InnerHtml = String.Format("<img height=\"25\" src=\"images/leftcontinue.gif\" width=\"12\">");
        html += String.Format("<img align=\"middle\" height=\"25\" src=\"images/leftcontinue.gif\" width=\"12\">");

      }
      else
      {
        if (cellCount == 0)
        {
          //td1.InnerHtml = String.Format("<img height=\"25\" src=\"images/leftblock.gif\" width=\"12\">");
          html += String.Format("<img align=\"middle\" height=\"25\" src=\"images/leftblock.gif\" width=\"12\">");

        }
        else
        {
          //td1.InnerHtml = String.Format("<img height=\"25\" src=\"images/leftblock.gif\" width=\"2\">");
          html += String.Format("<img align=\"middle\" height=\"25\" src=\"images/leftblock.gif\" width=\"2\">");

        }
      }
      cellCount++;
      //td2.VAlign = "center";
      //td2.Width = "100%";
      int length = width / 8;
      if (length > program.Title.Length) length = program.Title.Length;
      string title = "";
      if (length > 3)
      {
        if (length != program.Title.Length)
          title = program.Title.Substring(0, length - 3) + "...";
        else
          title = program.Title.Substring(0, length);
      }
      //title = ".";
      if (nr == 0)
      {
        //td2.InnerHtml = String.Format("<A class=guide_title_text>{0}</A>", title);
        html += String.Format("<span class=guide_title_text>{0}</span>", title); ;
      }
      else
      {
        //td2.InnerHtml = String.Format("<nobr>&nbsp;<A class=white style=\"CURSOR: pointer\" href=\"showProgram.aspx?id={1}\">{0}</A></nobr>", title, program.IdProgram);
        html += String.Format("<span class=white style=\"CURSOR: pointer\" onclick=\"onProgramClicked({1})\"\">{0}</span>", title, program.IdProgram); ;
      }
      //subRow.Controls.Add(td1);
      //subRow.Controls.Add(td2);
      bool isSeries;
      if (IsRecording(program, out isSeries))
      {
        HtmlGenericControl tdRec = new HtmlGenericControl();
        if (isSeries)
        {
          //tdRec.InnerHtml = String.Format("<img align=\"right\" src=\"images/icon_record_series.png\">");
          html += String.Format("<img align=\"middle\"  src=\"images/icon_record_series.png\">"); ;
        }
        else
        {
          //tdRec.InnerHtml = String.Format("<img align=\"right\" src=\"images/icon_record_single.png\">");
          html += String.Format("<img align=\"middle\"  src=\"images/icon_record_single.png\">");
        }
        //subRow.Controls.Add(tdRec);

      }
      cellBase.InnerHtml = html;
      divGuide.Controls.Add(cellBase);
      if (program.EndTime > end)
      {
        HtmlGenericControl divCtl = new HtmlGenericControl();

        string style = String.Format("style=\"position:absolute;left:{0}px;top:{1}px;\"", posx + width, posy);
        //HtmlGenericControl td3 = new HtmlGenericControl();
        //td3.InnerHtml = "<img height=\"25\" src=\"images/rightcontinue.gif\" width=\"12\">";
        html = String.Format("<img {0} height=\"25\" src=\"images/rightcontinue.gif\" width=\"12\">", style);
        divCtl.InnerHtml = html;
        divGuide.Controls.Add(divCtl);
      }


    }
  }
  protected void idForward_Click(object sender, EventArgs e)
  {
    DateTime now;
    if (Session["currentTime"] == null)
    {
      now = DateTime.Now;
      Session["currentTime"] = now;
    }
    now = (DateTime)Session["currentTime"];
    now = now.AddMinutes(30);
    Session["currentTime"] = now;
    UpdateGuide();
  }
  protected void idBack_Click(object sender, EventArgs e)
  {
    DateTime now;
    if (Session["currentTime"] == null)
    {
      now = DateTime.Now;
      Session["currentTime"] = now;
    }
    now = (DateTime)Session["currentTime"];
    now = now.AddMinutes(-30);
    Session["currentTime"] = now;
    UpdateGuide();
  }
  protected void dropDownTime_SelectedIndexChanged(object sender, EventArgs e)
  {
    DateTime now = (DateTime)Session["currentTime"];
    DateTime dateNow = DateTime.Now;
    dateNow = dateNow.AddDays(dropDownDate.SelectedIndex);
    now = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, now.Hour, now.Minute, 0);
    now = now.AddHours(-now.Hour);
    now = now.AddMinutes(-now.Minute);
    now = now.AddMinutes(dropDownTime.SelectedIndex * 30);
    Session["currentTime"] = now;
    UpdateGuide();

  }
  protected void dropDownDate_SelectedIndexChanged(object sender, EventArgs e)
  {
    DateTime now = (DateTime)Session["currentTime"];
    DateTime dateNow = DateTime.Now;
    dateNow = dateNow.AddDays(dropDownDate.SelectedIndex);
    now = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, now.Hour, now.Minute, 0);
    now = now.AddHours(-now.Hour);
    now = now.AddMinutes(-now.Minute);
    now = now.AddMinutes(dropDownTime.SelectedIndex * 30);
    Session["currentTime"] = now;
    UpdateGuide();

  }
  bool IsRecording(Program program, out bool isSeries)
  {
    isSeries = false;
    foreach (Schedule schedule in _schedules)
    {
      if (schedule.IsRecordingProgram(program, true))
      {
        if (schedule.ScheduleType != 0) isSeries = true;
        return true;
      }
    }
    return false;
  }
  protected void showProgram_Click(object sender, EventArgs e)
  {

    if (idProgram.Value.Length > 0)
    {
      ShowProgramInfo(Int32.Parse(idProgram.Value));
    }
    else
    {
      divInfoBox.Visible = false;
    }
    UpdateGuide();
  }
  protected void buttonDontRecord_Click(object sender, EventArgs e)
  {
    Program prog = Program.Retrieve(Int32.Parse(idProgram.Value));

    foreach (Schedule schedule in _schedules)
    {
      if (schedule.IsRecordingProgram(prog, true))
      {
        if (schedule.ScheduleType == (int)ScheduleRecordingType.Once)
        {
          schedule.Delete();
          UpdateServer();
          break;
        }
        else
        {
          CanceledSchedule canceledSchedule = new CanceledSchedule(schedule.IdSchedule, prog.StartTime);
          canceledSchedule.Persist();
          UpdateServer();
          break;
        }
      }
    }
    UpdateGuide();
  }

  protected void buttonRecordOnce_Click(object sender, EventArgs e)
  {
    Program program = Program.Retrieve(Int32.Parse(idProgram.Value));
    bool isSeries;
    if (IsRecording(program, out isSeries) == false)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Schedule rec = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
      rec.Persist();
      UpdateServer();
    }
    UpdateGuide();
  }

  protected void buttonRecordDaily_Click(object sender, EventArgs e)
  {
    Program program = Program.Retrieve(Int32.Parse(idProgram.Value));
    bool isSeries;
    if (IsRecording(program, out isSeries) == false)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Schedule rec = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
      rec.ScheduleType = (int)ScheduleRecordingType.Daily;
      rec.Persist();
      UpdateServer();
    }
    UpdateGuide();
  }
  protected void buttonRecordWeekly_Click(object sender, EventArgs e)
  {
    Program program = Program.Retrieve(Int32.Parse(idProgram.Value));
    bool isSeries;
    if (IsRecording(program, out isSeries) == false)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Schedule rec = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
      rec.ScheduleType = (int)ScheduleRecordingType.Weekly;
      rec.Persist();
      UpdateServer();
    }
    UpdateGuide();
  }
  protected void buttonRecordMonFri_Click(object sender, EventArgs e)
  {
    Program program = Program.Retrieve(Int32.Parse(idProgram.Value));
    bool isSeries;
    if (IsRecording(program, out isSeries) == false)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Schedule rec = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
      rec.ScheduleType = (int)ScheduleRecordingType.WorkingDays;
      rec.Persist();
      UpdateServer();
    }
    UpdateGuide();
  }
  protected void buttonRecordEveryThis_Click(object sender, EventArgs e)
  {
    Program program = Program.Retrieve(Int32.Parse(idProgram.Value));
    bool isSeries;
    if (IsRecording(program, out isSeries) == false)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Schedule rec = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
      rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnThisChannel;
      rec.Persist();
      UpdateServer();
    }
    UpdateGuide();
  }
  protected void buttonRecordEveryAll_Click(object sender, EventArgs e)
  {
    Program program = Program.Retrieve(Int32.Parse(idProgram.Value));
    bool isSeries;
    if (IsRecording(program, out isSeries) == false)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Schedule rec = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
      rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnEveryChannel;
      rec.Persist();
      UpdateServer();
    }
    UpdateGuide();
  }
  protected void buttonRecordWeekends_Click(object sender, EventArgs e)
  {
    Program program = Program.Retrieve(Int32.Parse(idProgram.Value));
    bool isSeries;
    if (IsRecording(program, out isSeries) == false)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Schedule rec = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
      rec.ScheduleType = (int)ScheduleRecordingType.Weekends;
      rec.Persist();
      UpdateServer();
    }
    UpdateGuide();
  }

  void UpdateServer()
  {
    IList servers = TvDatabase.Server.ListAll();
    foreach (TvDatabase.Server server in servers)
    {
      if (!server.IsMaster) continue;
      RemoteControl.Clear();
      RemoteControl.HostName = server.HostName;
      RemoteControl.Instance.OnNewSchedule();
      return;

    }
  }
  protected void DropDownListGroup_SelectedIndexChanged(object sender, EventArgs e)
  {
    IList groups = ChannelGroup.ListAll();
    foreach (ChannelGroup group in groups)
    {
      if (group.GroupName == DropDownListGroup.SelectedItem.ToString())
      {
        Session["idGroup"] = group.IdGroup;
        UpdateGuide();
        return;
      }
    }
    Session["idGroup"] = ((ChannelGroup)groups[0]).IdGroup;
    UpdateGuide();
  }
}