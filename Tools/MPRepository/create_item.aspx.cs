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
using MPRepository.Storage;
using MPRepository.Web.Support;


namespace MPRepository.Web
{
  public partial class create_item : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      MPUser user = SessionUtils.GetCurrentUser();

      if (IsPostBack)
      {
        uploadStatusLabel.Text = null;

        if (! user.hasPermission("Add"))
        {
          uploadStatusLabel.Text = String.Format("User {0} has no permission to add items", user.Handle);
          return;
        }

        // TODO: Validate form
        // TODO: If file is MPE, try to fill fields from file
        
        // Handle upload
        if ((fileUpload.PostedFile != null) && (fileUpload.PostedFile.ContentLength > 0))
        {
          if (HandleUploadedFile())
          {
            uploadStatusLabel.Text = "Upload successful.";
          }
          else
          {
            // Upload failed. Status should already contain the cause.
            if (uploadStatusLabel.Text == null)
            {
              uploadStatusLabel.Text = "Upload failed. Reason unknown.";
            }
          }
        }
        else // no file. Just page update
        {
          MPRSession session = MPRController.StartSession();
          FillCategoriesForSelectedItemType(session);
          MPRController.EndSession(session, true);
        }
      }
      else // not PostBack - initial page setup
      {
        // Load user from session
        authorTextBox.Text = user.Handle;

        // Fill development status according to ENum
        developmentStatusDropDownList.DataSource = Enum.GetNames(typeof(MPItemVersion.MPDevelopmentStatus));
        developmentStatusDropDownList.DataBind();

        // Load item types
        MPRSession session = MPRController.StartSession();
        IList<MPItemType> types = MPRController.RetrieveAll<MPItemType>(session);
        foreach (MPItemType type in types)
        {
          typesList.Items.Add(new ListItem(type.Name, type.Id.ToString()));
        }

        FillCategoriesForSelectedItemType(session);

        MPRController.EndSession(session, true);
      }

    }



    protected bool HandleUploadedFile()
    {
      {
        // TODO: check file

        // Save the file to the local filesystem
        string targetFilename = FileManager.GetSaveLocation(fileUpload.PostedFile.FileName);
        try
        {
          fileUpload.PostedFile.SaveAs(targetFilename);
        }
        catch (Exception ex)
        {
          return UploadFail("Unable to save file to local system: " + ex.ToString());
        }

        return AddFileToRepository(targetFilename);       

      }
    }

    /// <summary>
    /// Handle the actual creation of the entity
    /// </summary>
    /// <param name="filename">the name of the local file</param>
    /// <returns>success or failure</returns>
    protected bool AddFileToRepository(string filename)
    {
      MPRSession session = MPRController.StartSession();
      MPUser user = SessionUtils.GetCurrentUser();

      MPItem item = new MPItem();
      item.Name = titleTextBox.Text;

      Int64 typeId;
      if (!Int64.TryParse(typesList.SelectedValue, out typeId))
      {
        return UploadFail(String.Format("Invalid item type {0}", typesList.SelectedValue));
      }

      item.Type = MPRController.RetrieveById<MPItemType>(session, typeId);
      if (item.Type == null)
      {
        return UploadFail(String.Format("Unable to find item type {0} ({1})", typesList.SelectedItem, typeId));
      }

      List<Int64> categoryIds = new List<Int64>();
      foreach (ListItem categoryItem in categoriesList.Items)
      {
        if (categoryItem.Selected)
        {
          Int64 id;
          if (Int64.TryParse(categoryItem.Value, out id))
          {
            categoryIds.Add(id);
          }
        }
      }
      IList<MPCategory> categories = MPRController.RetrieveByIdList<MPCategory>(session, categoryIds);
      foreach (MPCategory category in categories)
      {
        item.Categories.Add(category);
      }

      item.Description = descriptionTextBox.Text;
      item.DescriptionShort = descriptionShortTextBox.Text;
      item.License = licenseTextBox.Text;
      item.LicenseMustAccept = licenseMustAccessCheckBox.Checked;
      item.Author = authorTextBox.Text;
      item.Homepage = homepageTextbox.Text;

      item.Tags = MPRController.GetTags(session, tagsTextBox.Text);

      // create ItemVersion
      MPItemVersion itemVersion = new MPItemVersion();
      itemVersion.Item = item;
      itemVersion.Uploader = user;
      itemVersion.DevelopmentStatus = (MPItemVersion.MPDevelopmentStatus) Enum.Parse(typeof(MPItemVersion.MPDevelopmentStatus), developmentStatusDropDownList.SelectedValue);
      itemVersion.MPVersionMin = mpVersionMinTextBox.Text;
      itemVersion.MPVersionMax = mpVersionMaxTextBox.Text;
      itemVersion.Version = versionTextBox.Text;

      MPFile mpfile = new MPFile();
      mpfile.ItemVersion = itemVersion;
      mpfile.Filename = System.IO.Path.GetFileName(fileUpload.PostedFile.FileName);
      mpfile.Location = filename;

      itemVersion.Files.Add(mpfile);

      item.Versions.Add(itemVersion);

      // Save item (and sub-items) to database
      try
      {
        MPRController.Save<MPItem>(session, item);
        MPRController.EndSession(session, true);
      }
      catch (Exception ex)
      {
        MPRController.EndSession(session, false);
        return UploadFail("Unable to save item: " + ex.ToString());
      }

      return true;

    }

    /// <summary>
    /// Handle upload failure.
    /// </summary>
    /// <param name="explanation">The text explaining what happend</param>
    /// <returns>Always false</returns>
    protected bool UploadFail(string explanation)
    {
      uploadStatusLabel.Text = explanation;
      return false;
    }

    protected void FillCategoriesForSelectedItemType(MPRSession session)
    {
      if (typesList.SelectedValue == null)
      {
        return;
      }

      // Remove the previous categories
      categoriesList.Items.Clear();

      // Fill the categories for the selected type
      Int64 typeId;
      Int64.TryParse(typesList.SelectedValue, out typeId);
      IList<MPCategory> categories = MPRController.RetrieveByForeignKey<MPCategory>(session, "Type", typeId);

      foreach (MPCategory category in categories)
      {
        categoriesList.Items.Add(new ListItem(category.Name, category.Id.ToString()));
      }

    }

  }
}
