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
using System.Collections;
using System.Collections.Generic;
using Iesi.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using MPRepository.Controller;
using MPRepository.Items;
using MPRepository.Users;
using MPRepository.Support;
using MPRepository.Storage;
using MPRepository.Web.Support;


namespace MPRepository.Web
{
  public partial class list_items : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {

      if (!IsPostBack)
      {
        MPRSession session = MPRController.StartSession();

        IList<DisplayItem> items = DisplayController.RetrieveAll<DisplayItem>(session, "LastUpdated", DisplayController.SortDirection.Descending);

        itemsGridView.DataSource = items;
        string[] keys = { "Id" };
        itemsGridView.DataKeyNames = keys;
        itemsGridView.DataBind();

        MPRController.EndSession(session, true);
      }
    }

    protected void itemsGridView_OnRowDataBound(object sender, GridViewRowEventArgs e)
    {
      {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
          e.Row.Attributes["onmouseover"] = "this.style.backgroundColor='yellow';";
          e.Row.Attributes["onmouseout"] = "this.style.backgroundColor='white';";
          e.Row.Attributes["onclick"] = ClientScript.GetPostBackClientHyperlink(itemsGridView, "Select$" + e.Row.RowIndex.ToString());
        }
      }
    }

    protected void itemsGridView_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (itemsGridView.SelectedRow != null)
      {
        MPRSession session = MPRController.StartSession();

        MPItem item = MPRController.RetrieveById<MPItem>(session, (Int64)itemsGridView.SelectedValue);

        if (item == null)
        {
          throw new InvalidFormDataException("Asked to provide details for invalid item id");
        }

        // TODO: clean up binding to a single object
        List<MPItem> items = new List<MPItem>();
        items.Add(item);
        itemDetailsView.DataSource = items;
        itemDetailsView.DataBind();

        versionDetailsView.DataSource = item.Versions;
        versionDetailsView.DataBind();

        commentsRepeater.DataSource = item.Comments;
        commentsRepeater.DataBind();

        // TODO: Only if submitter or has permission
        singleItemHyperLink.NavigateUrl = "~/single_item.aspx?itemid=" + item.Id.ToString();
        singleItemHyperLink.Visible = true;

        commentAddTextBox.Text = "";
        commentAddTextBox.Visible = true;
        commentAddButton.Visible = true;

        MPRController.EndSession(session, true);

      }
      else
      {
        singleItemHyperLink.NavigateUrl = "";
        singleItemHyperLink.Visible = false;
        commentAddTextBox.Visible = false;
        commentAddButton.Visible = false;
        commentAddLabel.Visible = false;
      }
    }

    protected void commentAddButton_Click(object sender, EventArgs e)
    {
      if (commentAddTextBox.Text.Length == 0)
      {
        commentAddLabel.Text = "Please enter a comment";
        commentAddLabel.Visible = true;
        return;
      }

      if (itemsGridView.SelectedRow == null)
      {
        throw new InvalidFormDataException("Comment can only be added to a selected item");
      }

      MPRSession session = MPRController.StartSession();

      MPItem item = MPRController.RetrieveById<MPItem>(session, (Int64)itemsGridView.SelectedValue);

      if (item == null)
      {
        throw new InvalidFormDataException("Asked to provide details for invalid item id");
      }

      MPItemComment comment = new MPItemComment();
      comment.User = SessionUtils.GetCurrentUser();
      comment.Text = commentAddTextBox.Text;
      item.Comments.Add(comment);
      MPRController.Save<MPItem>(session, item);

      MPRController.EndSession(session, true);

      commentAddTextBox.Text = "";
      commentAddLabel.Text = "Comment added. Thank you!";
      commentAddLabel.Visible = true;

    }

  }
}
