using System;
using System.Xml;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TvControl;

public partial class install_Default : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {

  }
  protected void idTest_Click(object sender, EventArgs e)
  {
    try
    {
      tableResult.Visible = true;
      RemoteControl.Clear();
      RemoteControl.HostName = idTvserver.Text;
      int cards = RemoteControl.Instance.Cards;
      textBoxResult.Text = "Connected to tvserver";

      try
      {
        string connectionString, provider;
        RemoteControl.Instance.GetDatabaseConnectionString(out connectionString, out provider);
        Gentle.Framework.ProviderFactory.ResetGentle(true);
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connectionString);
        textBoxDatabaseResult.Text = "Connected to SQL server";
        idSave.Visible = true;
      }
      catch (Exception)
      {
        textBoxDatabaseResult.Text = "Unable to connect to SQL server" + ex.ToString();
      }
    }
    catch (Exception ex)
    {
      textBoxResult.Text = "Unable to connect to tv server "+ex.ToString();
    }
  }
  protected void Button1_Click(object sender, EventArgs e)
  {

    try
    {
      string connectionString, provider;
      RemoteControl.Instance.GetDatabaseConnectionString(out connectionString, out provider);
      XmlDocument doc = new XmlDocument();
      doc.Load(Server.MapPath("../gentle.config"));
      XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
      XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString");
      XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
      node.InnerText = connectionString;
      nodeProvider.InnerText = provider;
      doc.Save(Server.MapPath("../gentle.config"));
    }
    catch (Exception)
    {
      textBoxResult.Text = "Unable to create/modify gentle.config !!!";
      return;
    }
    Response.Redirect("../default.aspx");
  }
}
