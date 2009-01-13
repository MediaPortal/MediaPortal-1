#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

namespace GUIRSSFeed
{
  /// <summary>
  /// Details form for entering a site for My News Plugin
  /// </summary>
  public class DetailsForm : MPConfigForm
  {
    private MPLabel labelDescription;
    private MPLabel labelEncoding;
    public MPTextBox textName;
    private MPLabel labelThumbnail;
    private MPLabel labelFeedName;
    private MPButton buttonBrowse;
    private MPTextBox textImage;
    private MPLabel label2;
    private MPButton buttonCancel;
    private MPTextBox textDescription;
    private MPButton buttonSave;
    private MPTextBox textBoxURL;
    private MPTextBox textEncoding;
    private OpenFileDialog openFileDialog1;
    public int ID;
    //private SetupForm form;
    public bool isNew;

    public DetailsForm(SetupForm parent, int ID)
    {
      this.ID = ID;
      //this.form = form;
      isNew = false;
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();

      if (ID > -1)
      {
        LoadSettings();
        isNew = false;
      }
      else //find the next ID that is blank
      {
        string tempText;
        for (int i = 0; i < 20; i++)
        {
          using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
          {
            tempText = xmlreader.GetValueAsString("rss", "siteName" + i, "");
            if (tempText == "")
            {
              this.ID = i;
              i = 20;
              isNew = true;
            }
          }
        }
        if (this.ID == -1)
        {
          Console.WriteLine("No more open slots!");
        }
        //TODO: Need message box popup here if no more slots left
      }


      //
      // TODO: Add constructor code after the InitializeComponent() call.
      //
    }

    private void saveInfo(object obj, EventArgs ea)
    {
      SaveSettings();
    }

    private void browseFile(object obj, EventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.CheckFileExists = true;
      dlg.CheckPathExists = true;
      dlg.RestoreDirectory = true;
      dlg.Filter = "image files (*.png)|*.png";
      dlg.FilterIndex = 0;
      dlg.Title = "Select Site Icon";
      dlg.ShowDialog();
      if (!String.IsNullOrEmpty(dlg.FileName))
      {
        textImage.Text = dlg.FileName;
      }
    }

    private void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        textName.Text = xmlreader.GetValueAsString("rss", "siteName" + ID, "");
        textBoxURL.Text = xmlreader.GetValueAsString("rss", "siteURL" + ID, "");
        textEncoding.Text = xmlreader.GetValueAsString("rss", "siteEncoding" + ID, "windows-1252");
        textDescription.Text = xmlreader.GetValueAsString("rss", "siteDescription" + ID, "");
        textImage.Text = xmlreader.GetValueAsString("rss", "siteImage" + ID, "");
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("rss", "siteName" + this.ID, textName.Text);
        xmlwriter.SetValue("rss", "siteURL" + this.ID, textBoxURL.Text);
        xmlwriter.SetValue("rss", "siteEncoding" + this.ID, textEncoding.Text);
        xmlwriter.SetValue("rss", "siteDescription" + this.ID, textDescription.Text);
        xmlwriter.SetValue("rss", "siteImage" + this.ID, textImage.Text);
      }
      this.Close();
    }

    #region Windows Forms Designer generated code

    /// <summary>
    /// This method is required for Windows Forms designer support.
    /// Do not change the method contents inside the source code editor. The Forms designer might
    /// not be able to load this method if it was changed manually.
    /// </summary>
    private void InitializeComponent()
    {
      this.openFileDialog1 = new OpenFileDialog();
      this.textBoxURL = new MPTextBox();
      this.textEncoding = new MPTextBox();
      this.buttonSave = new MPButton();
      this.textDescription = new MPTextBox();
      this.buttonCancel = new MPButton();
      this.label2 = new MPLabel();
      this.textImage = new MPTextBox();
      this.buttonBrowse = new MPButton();
      this.labelFeedName = new MPLabel();
      this.labelThumbnail = new MPLabel();
      this.textName = new MPTextBox();
      this.labelDescription = new MPLabel();
      this.labelEncoding = new MPLabel();
      this.SuspendLayout();
      // 
      // textBoxURL
      // 
      this.textBoxURL.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                 | AnchorStyles.Right)));
      this.textBoxURL.BorderColor = Color.Empty;
      this.textBoxURL.Location = new Point(80, 58);
      this.textBoxURL.Name = "textBoxURL";
      this.textBoxURL.Size = new Size(396, 20);
      this.textBoxURL.TabIndex = 4;
      // 
      // textEncoding
      // 
      this.textEncoding.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                   | AnchorStyles.Right)));
      this.textEncoding.BorderColor = Color.Empty;
      this.textEncoding.Location = new Point(80, 93);
      this.textEncoding.Name = "textEncoding";
      this.textEncoding.Size = new Size(136, 20);
      this.textEncoding.TabIndex = 6;
      this.textEncoding.Text = "windows-1252";
      // 
      // buttonSave
      // 
      this.buttonSave.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.buttonSave.Location = new Point(320, 180);
      this.buttonSave.Name = "buttonSave";
      this.buttonSave.Size = new Size(75, 23);
      this.buttonSave.TabIndex = 7;
      this.buttonSave.Text = "&OK";
      this.buttonSave.UseVisualStyleBackColor = true;
      this.buttonSave.Click += new EventHandler(this.buttonSave_Click);
      // 
      // textDescription
      // 
      this.textDescription.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                      | AnchorStyles.Right)));
      this.textDescription.BorderColor = Color.Empty;
      this.textDescription.Location = new Point(340, 24);
      this.textDescription.Name = "textDescription";
      this.textDescription.Size = new Size(136, 20);
      this.textDescription.TabIndex = 5;
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.buttonCancel.DialogResult = DialogResult.Cancel;
      this.buttonCancel.Location = new Point(401, 180);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new Size(75, 23);
      this.buttonCancel.TabIndex = 8;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new Point(12, 61);
      this.label2.Name = "label2";
      this.label2.Size = new Size(54, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "RSS URL";
      // 
      // textImage
      // 
      this.textImage.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                | AnchorStyles.Right)));
      this.textImage.BorderColor = Color.Empty;
      this.textImage.Location = new Point(80, 128);
      this.textImage.Name = "textImage";
      this.textImage.Size = new Size(315, 20);
      this.textImage.TabIndex = 9;
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Anchor = ((AnchorStyles) ((AnchorStyles.Top | AnchorStyles.Right)));
      this.buttonBrowse.Location = new Point(401, 126);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new Size(75, 23);
      this.buttonBrowse.TabIndex = 10;
      this.buttonBrowse.Text = "Browse";
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new EventHandler(this.browseFile);
      // 
      // labelFeedName
      // 
      this.labelFeedName.AutoSize = true;
      this.labelFeedName.Location = new Point(12, 27);
      this.labelFeedName.Name = "labelFeedName";
      this.labelFeedName.Size = new Size(60, 13);
      this.labelFeedName.TabIndex = 0;
      this.labelFeedName.Text = "Feed name";
      // 
      // labelThumbnail
      // 
      this.labelThumbnail.AutoSize = true;
      this.labelThumbnail.Location = new Point(12, 131);
      this.labelThumbnail.Name = "labelThumbnail";
      this.labelThumbnail.Size = new Size(56, 13);
      this.labelThumbnail.TabIndex = 8;
      this.labelThumbnail.Text = "Thumbnail";
      // 
      // textName
      // 
      this.textName.BorderColor = Color.Empty;
      this.textName.Location = new Point(80, 24);
      this.textName.Name = "textName";
      this.textName.Size = new Size(136, 20);
      this.textName.TabIndex = 3;
      // 
      // labelDescription
      // 
      this.labelDescription.AutoSize = true;
      this.labelDescription.Location = new Point(253, 27);
      this.labelDescription.Name = "labelDescription";
      this.labelDescription.Size = new Size(81, 13);
      this.labelDescription.TabIndex = 2;
      this.labelDescription.Text = "Title description";
      // 
      // labelEncoding
      // 
      this.labelEncoding.AutoSize = true;
      this.labelEncoding.Location = new Point(12, 96);
      this.labelEncoding.Name = "labelEncoding";
      this.labelEncoding.Size = new Size(52, 13);
      this.labelEncoding.TabIndex = 1;
      this.labelEncoding.Text = "Encoding";
      // 
      // DetailsForm
      // 
      this.AcceptButton = this.buttonSave;
      this.AutoScaleDimensions = new SizeF(6F, 13F);
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new Size(488, 215);
      this.Controls.Add(this.buttonBrowse);
      this.Controls.Add(this.textImage);
      this.Controls.Add(this.textEncoding);
      this.Controls.Add(this.textDescription);
      this.Controls.Add(this.textBoxURL);
      this.Controls.Add(this.textName);
      this.Controls.Add(this.labelThumbnail);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonSave);
      this.Controls.Add(this.labelEncoding);
      this.Controls.Add(this.labelDescription);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.labelFeedName);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.Name = "DetailsForm";
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "RSS News - Setup - DetailsForm";
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    #endregion

    private void buttonSave_Click(object sender, EventArgs e)
    {
      SaveSettings();
      this.Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }
  }
}