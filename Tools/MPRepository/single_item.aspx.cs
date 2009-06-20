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
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using MPRepository.Web.Support;
using MPRepository.Items;
using MPRepository.Controller;
using MPRepository.Storage;

namespace MPRepository.Web
{
  public partial class single_item : System.Web.UI.Page
  {

    #region log4net
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger("WebLog");
    #endregion //log4net
    
    protected void Page_Load(object sender, EventArgs e)
    {
      // TODO: why do I need to do this?
      if ((filesGridView.SelectedValue == null) && (versionDetailsView.SelectedValue != null))
      {
        // filesGridView lost its binding again
        filesDataSource.Select();
      }

      /*
      if (IsPostBack)
      {



      }
      else // Setting up the page
      {

      }
      */

    }

    protected void filesGridView_RowCommand(object sender, GridViewCommandEventArgs e)
    {
      // TODO: Download file
      System.Console.WriteLine("download");
    }

    protected string concatNames<T>(object collection)
      where T : MPRepository.Support.NamedComponent
    {
      string result = "";
      foreach (T item in ((IEnumerable<T>)collection))
      {
        if (result.Length > 0)
        {
          result += ",";
        }
        result += item.Name;
      }
      return result;
    }

    protected void itemDetailsView_ItemUpdating(object sender, DetailsViewUpdateEventArgs e)
    {
      // Because ObjectDataSource loses all the non-displayed values, as well as composite values,
      // we need to reload them here from the object.
      log.Debug(String.Format("Updating item {0}", e.Keys["Id"]));

      MPRSession session = MPRController.StartSession();
      MPItem item = MPRController.RetrieveById<MPItem>(session, (Int64)e.Keys["Id"]);
      item.Name = (string)e.NewValues["Name"];
      item.Description = (string)e.NewValues["Description"];
      item.DescriptionShort = (string)e.NewValues["DescriptionShort"];
      item.Author = (string)e.NewValues["Author"];
      item.Homepage = (string)e.NewValues["Homepage"];
      item.License = (string)e.NewValues["License"];
      item.LicenseMustAccept = (bool)e.NewValues["LicenseMustAccept"];    
      item.Tags = MPRController.GetTags(session, ((TextBox)itemDetailsView.FindControl("tagsTextBox")).Text);
      MPRController.Save<MPItem>(session, item);
      MPRController.EndSession(session, true);

      log.Info(String.Format("Updated item {0} ({1})", e.Keys["Id"], e.NewValues["Name"]));

      // Manually reset the form to view format
      e.Cancel = true;
      itemDetailsView.ChangeMode(DetailsViewMode.ReadOnly);

      
    }

    protected void versionDetailsView_ItemUpdating(object sender, DetailsViewUpdateEventArgs e)
    {
      // Because ObjectDataSource loses all the non-displayed values, as well as composite values,
      // we need to reload them here from the object.
      log.Debug(String.Format("Updating version {0}", e.Keys["Id"]));

      MPRSession session = MPRController.StartSession();
      MPItemVersion version = MPRController.RetrieveById<MPItemVersion>(session, (Int64)e.Keys["Id"]);
      version.Version = (string)e.NewValues["Version"];
      version.MPVersionMin = (string)e.NewValues["MPVersionMin"];
      version.MPVersionMax = (string)e.NewValues["MPVersionMax"];
      version.ReleaseNotes = (string)e.NewValues["ReleaseNotes"];
      version.UpdateDate = DateTime.Now;
      MPRController.Save<MPItemVersion>(session, version);
      MPRController.EndSession(session, true);

      log.Info(String.Format("Updated version {0} ({1})", e.Keys["Id"], e.NewValues["Version"]));

      // Manually reset the form to view format
      e.Cancel = true;
      versionDetailsView.ChangeMode(DetailsViewMode.ReadOnly);

    }

    protected void versionDetailsView_ItemInserting(object sender, DetailsViewInsertEventArgs e)
    {
      log.Debug(String.Format("Inserting version to item {0}", itemDetailsView.SelectedValue));

      FileUpload fileUpload = (FileUpload)versionDetailsView.FindControl("fileUpload");

      if ((fileUpload.PostedFile == null) || (fileUpload.PostedFile.ContentLength == 0)) 
      {
        statusLabel.Text = "Please upload a file for the new version";
        statusLabel.Visible = true;
        e.Cancel = true;
        return;
      }

      // Save the file
      string targetFilename = FileManager.GetSaveLocation(fileUpload.PostedFile.FileName);
      try
      {
        fileUpload.PostedFile.SaveAs(targetFilename);
      }
      catch (Exception ex)
      {
        log.Error("Unable to save file to local system: " + ex.ToString());
        statusLabel.Text = "Error while trying to save file";
        statusLabel.Visible = true;
        e.Cancel = true;
        return;
      }

      // TODO: Persist target filename somewhere
      ViewState["TargetFileName"] = System.IO.Path.GetFileName(fileUpload.PostedFile.FileName);
      ViewState["TargetFileLocation"] = targetFilename;

      MPRSession session = MPRController.StartSession();
      MPItem item = MPRController.RetrieveById<MPItem>(session, (Int64)itemDetailsView.SelectedValue);
      e.Values["Item"] = item;
      e.Values["Uploader"] = SessionUtils.GetCurrentUser();
      e.Values["UpdateDate"] = DateTime.Now;

      MPRController.EndSession(session, true);

      log.Info(String.Format("Added new version to item {0}", itemDetailsView.SelectedValue));

      statusLabel.Text = "Version added";
      statusLabel.Visible = true;

    }

    protected void versionDetailsView_ItemDeleting(object sender, DetailsViewDeleteEventArgs e)
    {
      e.Cancel = true;
      return;
    }

    protected void versionDetailsView_ItemInserted(object sender, DetailsViewInsertedEventArgs e)
    {
      Page.DataBind();
    }

    protected void versionDetailsView_PageIndexChanged(object sender, EventArgs e)
    {
      Page.DataBind();
    }

    protected void versionDetailsView_ModeChanging(object sender, DetailsViewModeEventArgs e)
    {
      if (e.NewMode == DetailsViewMode.Insert)
      {
        versionDetailsView.Fields[8].Visible = true; // FileUpload template field
      }
      else
      {
        versionDetailsView.Fields[8].Visible = false; // FileUpload template field
      }
    }

    protected void versionDataSource_Inserted(object sender, ObjectDataSourceStatusEventArgs e)
    {
      string filename = (string)ViewState["TargetFileName"];
      string location = (string)ViewState["TargetFileLocation"];


      MPRSession session = MPRController.StartSession();
      MPItemVersion version = MPRController.RetrieveById<MPItemVersion>(session, (Int64)e.ReturnValue);

      // Add the new file to the Repository
      MPFile mpfile = new MPFile();
      mpfile.ItemVersion = version;
      mpfile.Filename = filename;
      mpfile.Location = location;
      version.Files.Add(mpfile);

      MPRController.Save<MPItemVersion>(session, version);
      MPRController.EndSession(session, true);

      ViewState.Remove("TargetFileName");
      ViewState.Remove("TargetFileLocation");

    }

  }
}
