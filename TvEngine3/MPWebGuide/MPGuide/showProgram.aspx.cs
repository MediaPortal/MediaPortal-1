using System;
using System.Data;
using System.Configuration;
using System.Collections;
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

public partial class showProgram : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
    ShowProgramInfo();
  }
  void ShowProgramInfo()
  {
    int id = Int32.Parse(Request.Params["id"]);
    Program program = Program.Retrieve(id);
    textBoxChannel.Text = program.ReferencedChannel().Name;
    textBoxTitle.Text = program.Title;
    textBoxDescription.Text = program.Description;
    textBoxGenre.Text = program.Genre;
    textBoxStart.Text = program.StartTime.ToShortTimeString();
    textEnd.Text = program.EndTime.ToShortTimeString() + "  " + program.StartTime.ToLongDateString();
  }
}
