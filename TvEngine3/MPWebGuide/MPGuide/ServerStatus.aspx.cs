using System;
using System.Data;
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
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using TvLibrary.Streaming;
using Gentle.Common;
using Gentle.Framework;

public partial class ServerStatus : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
    UpdateStatusBox();
    UpdateClientsBox();
  }

  void UpdateStatusBox()
  {
    ConnectToTvServer();
    TvServer server = new TvServer();
    IList cards = Card.ListAll();
    int cardNo = 0;
    string[] items = new string[10];
    foreach (Card card in cards)
    {
      cardNo++;
      AddCardRows(cardNo, card);
    }
    HtmlTableRow row = new HtmlTableRow();
    HtmlTableCell cell = new HtmlTableCell();
    cell.Attributes.Add("class", "recording_list_bottom");
    row.Cells.Add(cell);
    tableStatus.Rows.Add(row);
  }


  void AddCardRows(int cardNo, Card card)
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
    td1.Attributes.Add("class", "recording_list_text");
    td1.Style.Add("padding-right", "4px");
    td1.Style.Add("padding-left", "4px");
    td1.Style.Add("padding-bottom", "4px");
    td1.Style.Add("width", "100%");
    td1.Style.Add("padding-top", "4px");
    td1.Style.Add("border-bottom", "#304a66 1px solid");
    td1.Align = "left";


    td1.InnerHtml = string.Format("<span class=\"recording_list_text\"><table width=\"100%\"><tr><td>State</td><td>Channel</td><td>Scrambled</td><td>User</td><td>Card</td></tr>");

    if (card.Enabled == false)
    {
      td1.InnerHtml += String.Format("<tr><td>disabled</td><td>-</td><td>-</td><td>-</td><td>{0}</td></tr>", card.Name);
    }
    else
    {
      User[] users = RemoteControl.Instance.GetUsersForCard(card.IdCard);
      if (users.Length == 0)
      {
        User user = new User();
        user.CardId = card.IdCard;
        VirtualCard vcard = new VirtualCard(user);
        string tmp = "idle";
        if (vcard.IsScanning) tmp = "Scanning";
        if (vcard.IsGrabbingEpg) tmp = "Grabbing EPG";
        td1.InnerHtml += string.Format("<tr><td>{0}</td><td>-</td><td>-</td><td>-</td><td>{1}</td></tr>", tmp, card.Name);
      }
      else
      {
        for (int i = 0; i < users.Length; ++i)
        {
          VirtualCard vcard = new VirtualCard(users[i]);
          string tmp = "idle";
          if (vcard.IsTimeShifting) tmp = "Timeshifting";
          if (vcard.IsRecording) tmp = "Recording";
          if (vcard.IsScanning) tmp = "Scanning";
          if (vcard.IsGrabbingEpg) tmp = "Grabbing EPG";

          string scrambled = "idle";
          if (vcard.IsScrambled) scrambled = "yes";
          else scrambled = "no";
          td1.InnerHtml += string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>", tmp, vcard.ChannelName, scrambled, users[i].Name, card.Name);
        }
      }
    }
    td1.InnerHtml += "</table><span>";
    //<tr><td>state</td><td>Channel</td><td>Channel</td><td>Scrambled</td><td>User</td></tr>

    subRow.Cells.Add(td1);
    subTable.Rows.Add(subRow);
    cellBase.Controls.Add(subTable);
    baseRow.Cells.Add(cellBase);
    tableStatus.Rows.Add(baseRow);
  }
  void UpdateClientsBox()
  {

    ConnectToTvServer();
    List<RtspClient> clients = RemoteControl.Instance.StreamingClients;

    HtmlTableRow row = new HtmlTableRow();
    HtmlTableCell cell = new HtmlTableCell();
    cell.Attributes.Add("class", "recording_list_bottom");
    row.Cells.Add(cell);
    tableClients.Rows.Add(row);
  }


  void AddClientRows(List<RtspClient> clients)
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
    td1.Attributes.Add("class", "recording_list_text");
    td1.Style.Add("padding-right", "4px");
    td1.Style.Add("padding-left", "4px");
    td1.Style.Add("padding-bottom", "4px");
    td1.Style.Add("width", "100%");
    td1.Style.Add("padding-top", "4px");
    td1.Style.Add("border-bottom", "#304a66 1px solid");
    td1.Align = "left";


    td1.InnerHtml = string.Format("<span class=\"recording_list_text\"><table width=\"100%\"><tr><td>Stream</td><td>IP</td><td>Active</td><td>Started</td><td>Description</td></tr>");

    for (int i = 0; i < clients.Count; ++i)
    {
      string tmp = "no";
      if (clients[i].IsActive) tmp = "yes";
      td1.InnerHtml += string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>",
                clients[i].StreamName, clients[i].IpAdress, tmp, clients[i].DateTimeStarted.ToString("yyyy-MM-dd HH:mm:ss"), clients[i].Description);
    }
    td1.InnerHtml += "</table><span>";
    //<tr><td>state</td><td>Channel</td><td>Channel</td><td>Scrambled</td><td>User</td></tr>

    subRow.Cells.Add(td1);
    subTable.Rows.Add(subRow);
    cellBase.Controls.Add(subTable);
    baseRow.Cells.Add(cellBase);
    tableClients.Rows.Add(baseRow);
  }
  void ConnectToTvServer()
  {
    IList servers = TvDatabase.Server.ListAll();
    foreach (TvDatabase.Server server in servers)
    {
      if (!server.IsMaster) continue;
      RemoteControl.Clear();
      RemoteControl.HostName = server.HostName;
      return;

    }
  }
}
