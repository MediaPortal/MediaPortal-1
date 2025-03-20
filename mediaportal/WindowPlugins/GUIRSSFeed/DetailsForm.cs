#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

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
    private MPLabel labelRefresh;
    private MPCheckBox checkAutoRefresh;
    private MPTextBox textRefreshInterval;

    private string m_PublishTime;

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
          using (Settings xmlreader = new MPSettings())
          {
            tempText = xmlreader.GetValueAsString("rss", "siteName" + i, string.Empty);
            if (tempText == string.Empty)
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

    private void browseFile(object obj, EventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog
      {
        CheckFileExists = true,
        CheckPathExists = true,
        RestoreDirectory = true,
        Filter = "image files (*.png)|*.png",
        FilterIndex = 0,
        Title = "Select Site Icon"
      };
      dlg.ShowDialog();
      if (!string.IsNullOrEmpty(dlg.FileName))
      {
        textImage.Text = dlg.FileName;
      }
    }

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        textName.Text = xmlreader.GetValueAsString("rss", "siteName" + ID, string.Empty);
        textBoxURL.Text = xmlreader.GetValueAsString("rss", "siteURL" + ID, string.Empty);
        textEncoding.Text = xmlreader.GetValueAsString("rss", "siteEncoding" + ID, "windows-1252");
        textDescription.Text = xmlreader.GetValueAsString("rss", "siteDescription" + ID, string.Empty);
        textImage.Text = xmlreader.GetValueAsString("rss", "siteImage" + ID, string.Empty);
        textRefreshInterval.Text = xmlreader.GetValueAsString("rss", "siteRefreshPeriod" + ID, "15");
        checkAutoRefresh.Checked = xmlreader.GetValueAsInt("rss", "siteAutoRefresh" + ID, 0) != 0;
        m_PublishTime = xmlreader.GetValueAsString("rss", "sitePublishTime" + ID, string.Empty);
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("rss", "siteName" + this.ID, textName.Text);
        xmlwriter.SetValue("rss", "siteURL" + this.ID, textBoxURL.Text);
        xmlwriter.SetValue("rss", "siteEncoding" + this.ID, textEncoding.Text);
        xmlwriter.SetValue("rss", "siteDescription" + this.ID, textDescription.Text);
        xmlwriter.SetValue("rss", "siteImage" + this.ID, textImage.Text);
        xmlwriter.SetValue("rss", "siteRefreshPeriod" + this.ID, textRefreshInterval.Text);
        xmlwriter.SetValue("rss", "siteAutoRefresh" + this.ID, checkAutoRefresh.Checked ? "1" : "0");
        xmlwriter.SetValue("rss", "sitePublishTime" + this.ID, this.m_PublishTime);
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.textBoxURL = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.textEncoding = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.buttonSave = new MediaPortal.UserInterface.Controls.MPButton();
            this.textDescription = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
            this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.textImage = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.buttonBrowse = new MediaPortal.UserInterface.Controls.MPButton();
            this.labelFeedName = new MediaPortal.UserInterface.Controls.MPLabel();
            this.labelThumbnail = new MediaPortal.UserInterface.Controls.MPLabel();
            this.textName = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.labelDescription = new MediaPortal.UserInterface.Controls.MPLabel();
            this.labelEncoding = new MediaPortal.UserInterface.Controls.MPLabel();
            this.labelRefresh = new MediaPortal.UserInterface.Controls.MPLabel();
            this.checkAutoRefresh = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.textRefreshInterval = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.SuspendLayout();
            // 
            // textBoxURL
            // 
            this.textBoxURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxURL.BorderColor = System.Drawing.Color.Empty;
            this.textBoxURL.Location = new System.Drawing.Point(80, 58);
            this.textBoxURL.Name = "textBoxURL";
            this.textBoxURL.Size = new System.Drawing.Size(396, 20);
            this.textBoxURL.TabIndex = 4;
            // 
            // textEncoding
            // 
            this.textEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textEncoding.BorderColor = System.Drawing.Color.Empty;
            this.textEncoding.Location = new System.Drawing.Point(80, 93);
            this.textEncoding.Name = "textEncoding";
            this.textEncoding.Size = new System.Drawing.Size(136, 20);
            this.textEncoding.TabIndex = 6;
            this.textEncoding.Text = "windows-1252";
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(320, 180);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 7;
            this.buttonSave.Text = "&OK";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // textDescription
            // 
            this.textDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textDescription.BorderColor = System.Drawing.Color.Empty;
            this.textDescription.Location = new System.Drawing.Point(340, 24);
            this.textDescription.Name = "textDescription";
            this.textDescription.Size = new System.Drawing.Size(136, 20);
            this.textDescription.TabIndex = 5;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(401, 180);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "RSS URL";
            // 
            // textImage
            // 
            this.textImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textImage.BorderColor = System.Drawing.Color.Empty;
            this.textImage.Location = new System.Drawing.Point(80, 128);
            this.textImage.Name = "textImage";
            this.textImage.Size = new System.Drawing.Size(315, 20);
            this.textImage.TabIndex = 9;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(401, 126);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 10;
            this.buttonBrowse.Text = "Browse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.browseFile);
            // 
            // labelFeedName
            // 
            this.labelFeedName.AutoSize = true;
            this.labelFeedName.Location = new System.Drawing.Point(12, 27);
            this.labelFeedName.Name = "labelFeedName";
            this.labelFeedName.Size = new System.Drawing.Size(60, 13);
            this.labelFeedName.TabIndex = 0;
            this.labelFeedName.Text = "Feed name";
            // 
            // labelThumbnail
            // 
            this.labelThumbnail.AutoSize = true;
            this.labelThumbnail.Location = new System.Drawing.Point(12, 131);
            this.labelThumbnail.Name = "labelThumbnail";
            this.labelThumbnail.Size = new System.Drawing.Size(56, 13);
            this.labelThumbnail.TabIndex = 8;
            this.labelThumbnail.Text = "Thumbnail";
            // 
            // textName
            // 
            this.textName.BorderColor = System.Drawing.Color.Empty;
            this.textName.Location = new System.Drawing.Point(80, 24);
            this.textName.Name = "textName";
            this.textName.Size = new System.Drawing.Size(136, 20);
            this.textName.TabIndex = 3;
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(253, 27);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(81, 13);
            this.labelDescription.TabIndex = 2;
            this.labelDescription.Text = "Title description";
            // 
            // labelEncoding
            // 
            this.labelEncoding.AutoSize = true;
            this.labelEncoding.Location = new System.Drawing.Point(12, 96);
            this.labelEncoding.Name = "labelEncoding";
            this.labelEncoding.Size = new System.Drawing.Size(52, 13);
            this.labelEncoding.TabIndex = 1;
            this.labelEncoding.Text = "Encoding";
            // 
            // labelRefresh
            // 
            this.labelRefresh.AutoSize = true;
            this.labelRefresh.Location = new System.Drawing.Point(31, 188);
            this.labelRefresh.Name = "labelRefresh";
            this.labelRefresh.Size = new System.Drawing.Size(109, 13);
            this.labelRefresh.TabIndex = 16;
            this.labelRefresh.Text = "Refresh interval (min):";
            this.labelRefresh.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkAutoRefresh
            // 
            this.checkAutoRefresh.AutoSize = true;
            this.checkAutoRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.checkAutoRefresh.Location = new System.Drawing.Point(16, 164);
            this.checkAutoRefresh.Name = "checkAutoRefresh";
            this.checkAutoRefresh.Size = new System.Drawing.Size(122, 17);
            this.checkAutoRefresh.TabIndex = 15;
            this.checkAutoRefresh.Text = "Auto refresh enabled";
            this.checkAutoRefresh.UseVisualStyleBackColor = true;
            // 
            // textRefreshInterval
            // 
            this.textRefreshInterval.BorderColor = System.Drawing.Color.Empty;
            this.textRefreshInterval.Location = new System.Drawing.Point(152, 185);
            this.textRefreshInterval.Name = "textRefreshInterval";
            this.textRefreshInterval.Size = new System.Drawing.Size(53, 20);
            this.textRefreshInterval.TabIndex = 17;
            this.textRefreshInterval.Text = "15";
            this.textRefreshInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // DetailsForm
            // 
            this.AcceptButton = this.buttonSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(488, 215);
            this.Controls.Add(this.labelRefresh);
            this.Controls.Add(this.checkAutoRefresh);
            this.Controls.Add(this.textRefreshInterval);
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
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "DetailsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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