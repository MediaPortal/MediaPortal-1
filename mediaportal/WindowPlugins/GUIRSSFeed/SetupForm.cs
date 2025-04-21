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
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

namespace GUIRSSFeed
{
  [PluginIcons("GUIRSSFeed.rssicon.png", "GUIRSSFeed.rssicon_disabled.png")]
  /// <summary>
    /// A setup form for the My News Plugin
    /// </summary>
  public class SetupForm : MPConfigForm, ISetupForm, IShowPlugin
  {
    private MPButton buttonAdd;
    private ListBox listBox;
    private MPButton buttonEdit;
    private MPButton buttonDelete;
    private MPButton button3;
    private MPGroupBox mpGroupBox1;
    private DetailsForm form;

    public SetupForm()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();

      //
      // TODO: Add constructor code after the InitializeComponent() call.
      //
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
    }

    ~SetupForm()
    {
      listBox.Items.Clear();
    }

    #region ButtonHandlers

    /// <summary>
    /// Opens up the Details for so you can add a new feed.
    /// </summary>
    private void addSite(object obj, EventArgs ea)
    {
      form = new DetailsForm(this, -1);
      form.ShowDialog(this.Parent);
      listBox.Items.Clear();
      PopulateFields();
    }

    /// <summary>
    /// Opens up the DetailsForm so you can edit a feed
    /// </summary>
    private void editSite(object obj, EventArgs ea)
    {
      if (listBox.SelectedIndices == null)
      {
        return;
      }
      if (listBox.SelectedIndices.Count <= 0)
      {
        return;
      }
      int iItem = listBox.SelectedIndices[0];
      int ID = 0;
      //find the index of the item in question
      string strTempText;
      for (int i = 0; i < 100; i++)
      {
        using (Settings xmlreader = new MPSettings())
        {
          strTempText = xmlreader.GetValueAsString("rss", "siteName" + i, string.Empty);
          if (strTempText == (string)listBox.Items[iItem])
          {
            ID = i;
            break;
          }
        }
      }
      DetailsForm form = new DetailsForm(this, ID);
      form.ShowDialog(this.Parent);
      listBox.Items.Clear();
      PopulateFields();
    }

    /// <summary>
    /// Deletes the feed that is currently selected in the listbox
    /// </summary>
    private void deleteSite(object obj, EventArgs ea)
    {
      if (listBox.SelectedIndices == null)
      {
        return;
      }
      if (listBox.SelectedIndices.Count == 0)
      {
        return;
      }
      int iItem = listBox.SelectedIndices[0];
      int iID = 0;
      //find the index of the item in question
      string strTempText;
      for (int i = 0; i < 100; i++)
      {
        using (Settings xmlreader = new MPSettings())
        {
          strTempText = xmlreader.GetValueAsString("rss", "siteName" + i, string.Empty);
          if (strTempText == (string)listBox.Items[iItem])
          {
            iID = i;
            break;
          }
        }
      }
      
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("rss", "siteName" + iID, string.Empty);
        xmlwriter.SetValue("rss", "siteURL" + iID, string.Empty);
        xmlwriter.SetValue("rss", "siteDescription" + iID, string.Empty);
        xmlwriter.SetValue("rss", "siteEncoding" + iID, "windows-1252");
        xmlwriter.SetValue("rss", "siteImage" + iID, string.Empty);
        xmlwriter.SetValue("rss", "siteRefreshPeriod" + iID, string.Empty);
        xmlwriter.SetValue("rss", "siteAutoRefresh" + iID, string.Empty);
        xmlwriter.SetValue("rss", "siteRefreshTime" + iID, string.Empty);
        xmlwriter.SetValue("rss", "sitePublishTime" + iID, string.Empty);
      }

      listBox.Items.Clear();
      PopulateFields();

      if (GUIWindowManager.ActiveWindow == MediaPortal.GUI.RSS.GUIRSSFeed.WINDOW_RSS)
      {
        Console.WriteLine("RSS active window, need to somehow refresh");
        MediaPortal.GUI.RSS.GUIRSSFeed rss =
          (MediaPortal.GUI.RSS.GUIRSSFeed)GUIWindowManager.GetWindow(MediaPortal.GUI.RSS.GUIRSSFeed.WINDOW_RSS);
        rss.RefreshFeeds();
      }
    }

    #endregion

    /// <summary>
    /// Initialise the form
    /// </summary>
    private void SetupForm_Load(object sender, EventArgs e)
    {
      listBox.Items.Clear();
      PopulateFields();
    }

    /// <summary>
    /// Adds the names of available newsfeeds to the listbox
    /// </summary>
    public void PopulateFields()
    {
      using (Settings xmlreader = new MPSettings())
      {
        for (int i = 0; i < 100; i++)
        {
          string strNameTag = string.Format("siteName{0}", i);
          string strURLTag = string.Format("siteURL{0}", i);
          //string strEncodingTag = String.Format("siteEncoding{0}", i);

          string strName = xmlreader.GetValueAsString("rss", strNameTag, string.Empty);
          string strURL = xmlreader.GetValueAsString("rss", strURLTag, string.Empty);
          //string strEncoding = xmlreader.GetValueAsString("rss", strEncodingTag, "windows-1252");

          if (strName.Length > 0 && strURL.Length > 0)
          {
            this.listBox.Items.Add(strName);
          }
        }
      }
    }

    private void button3_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    #region ISetup Interface methods

    /// <summary>
    /// See ISetupForm interface
    /// </summary>
    public bool CanEnable() // Indicates whether plugin can be enabled/disabled
    {
      return true;
    }

    public bool HasSetup()
    {
      return true;
    }

    /// <summary>
    /// See ISetupForm interface
    /// </summary>
    public bool DefaultEnabled()
    {
      return false;
    }

    /// <summary>
    /// See ISetupForm interface
    /// </summary>
    public int GetWindowId()
    {
      return (int)MediaPortal.GUI.RSS.GUIRSSFeed.WINDOW_RSS;
    }

    /// <summary>
    /// See ISetupForm interface
    /// </summary>
    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(9); // My News
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = @"hover_my news.png"; //the big image seen when hovering over My Radio
      return true;
    }

    /// <summary>
    /// See ISetupForm interface
    /// </summary>
    public string PluginName()
    {
      return "RSS news";
    }

    /// <summary>
    /// See ISetupForm interface
    /// </summary>
    public string Description()
    {
      return "Read RSS news feeds in MediaPortal";
    }

    /// <summary>
    /// See ISetupForm interface
    /// </summary>
    public string Author()
    {
      return "Gilthalas";
    }

    /// <summary>
    /// See ISetupForm interface
    /// </summary>
    public void ShowPlugin()
    {
      ShowDialog();
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion

    #region Windows Forms Designer generated code

    /// <summary>
    /// This method is required for Windows Forms designer support.
    /// Do not change the method contents inside the source code editor. The Forms designer might
    /// not be able to load this method if it was changed manually.
    /// </summary>
    private void InitializeComponent()
    {
            this.buttonDelete = new MediaPortal.UserInterface.Controls.MPButton();
            this.buttonEdit = new MediaPortal.UserInterface.Controls.MPButton();
            this.listBox = new System.Windows.Forms.ListBox();
            this.buttonAdd = new MediaPortal.UserInterface.Controls.MPButton();
            this.button3 = new MediaPortal.UserInterface.Controls.MPButton();
            this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.mpGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonDelete
            // 
            this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDelete.Location = new System.Drawing.Point(368, 185);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(88, 23);
            this.buttonDelete.TabIndex = 3;
            this.buttonDelete.Text = "Delete Site";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.deleteSite);
            // 
            // buttonEdit
            // 
            this.buttonEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEdit.Location = new System.Drawing.Point(274, 185);
            this.buttonEdit.Name = "buttonEdit";
            this.buttonEdit.Size = new System.Drawing.Size(88, 23);
            this.buttonEdit.TabIndex = 2;
            this.buttonEdit.Text = "Edit Site";
            this.buttonEdit.UseVisualStyleBackColor = true;
            this.buttonEdit.Click += new System.EventHandler(this.editSite);
            // 
            // listBox
            // 
            this.listBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox.Location = new System.Drawing.Point(6, 19);
            this.listBox.Name = "listBox";
            this.listBox.Size = new System.Drawing.Size(450, 147);
            this.listBox.TabIndex = 5;
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.Location = new System.Drawing.Point(180, 185);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(88, 23);
            this.buttonAdd.TabIndex = 1;
            this.buttonAdd.Text = "Add Site";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.addSite);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button3.Location = new System.Drawing.Point(402, 279);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(72, 22);
            this.button3.TabIndex = 12;
            this.button3.Text = "&Done";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // mpGroupBox1
            // 
            this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mpGroupBox1.Controls.Add(this.listBox);
            this.mpGroupBox1.Controls.Add(this.buttonAdd);
            this.mpGroupBox1.Controls.Add(this.buttonEdit);
            this.mpGroupBox1.Controls.Add(this.buttonDelete);
            this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBox1.Location = new System.Drawing.Point(12, 11);
            this.mpGroupBox1.Name = "mpGroupBox1";
            this.mpGroupBox1.Size = new System.Drawing.Size(462, 213);
            this.mpGroupBox1.TabIndex = 15;
            this.mpGroupBox1.TabStop = false;
            this.mpGroupBox1.Text = "Add news sites here and edit their options";
            // 
            // SetupForm
            // 
            this.AcceptButton = this.button3;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.CancelButton = this.button3;
            this.ClientSize = new System.Drawing.Size(486, 313);
            this.Controls.Add(this.mpGroupBox1);
            this.Controls.Add(this.button3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SetupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RSS News - Setup";
            this.Load += new System.EventHandler(this.SetupForm_Load);
            this.mpGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

    }

    #endregion
  }
}