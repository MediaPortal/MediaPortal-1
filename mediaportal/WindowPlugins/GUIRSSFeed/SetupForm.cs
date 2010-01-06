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
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

namespace GUIRSSFeed
{
  [PluginIcons("WindowPlugins.GUIRSSFeed.rssicon.png", "WindowPlugins.GUIRSSFeed.rssicon_disabled.png")]
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
    private MPCheckBox checkAutoRefresh;
    private MPLabel labelRefresh;
    private MPTextBox textRefreshInterval;
    private MPGroupBox mpGroupBox1;
    private MPGroupBox mpGroupBox2;
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
      string tempText;
      for (int i = 0; i < 100; i++)
      {
        using (Settings xmlreader = new MPSettings())
        {
          tempText = xmlreader.GetValueAsString("rss", "siteName" + i, "");
          if (tempText == (string)listBox.Items[iItem])
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
      int ID = 0;
      //find the index of the item in question
      string tempText;
      for (int i = 0; i < 100; i++)
      {
        using (Settings xmlreader = new MPSettings())
        {
          tempText = xmlreader.GetValueAsString("rss", "siteName" + i, "");
          if (tempText == (string)listBox.Items[iItem])
          {
            ID = i;
            break;
          }
        }
      }

      string strNameTag = String.Format("siteName{0}", ID);
      string strURLTag = String.Format("siteURL{0}", ID);
      string strDescriptionTag = String.Format("siteDescription{0}", ID);
      string strEncodingTag = String.Format("siteEncoding{0}", ID);
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("rss", strNameTag, "");
        xmlwriter.SetValue("rss", strURLTag, "");
        xmlwriter.SetValue("rss", strDescriptionTag, "");
        xmlwriter.SetValue("rss", strEncodingTag, "windows-1252");
      }

      listBox.Items.Clear();
      PopulateFields();

      if (GUIWindowManager.ActiveWindow == (int)MediaPortal.GUI.RSS.GUIRSSFeed.WINDOW_RSS)
      {
        Console.WriteLine("RSS active window, need to somehow refresh");
        MediaPortal.GUI.RSS.GUIRSSFeed rss =
          (MediaPortal.GUI.RSS.GUIRSSFeed)GUIWindowManager.GetWindow((int)MediaPortal.GUI.RSS.GUIRSSFeed.WINDOW_RSS);
        rss.refreshFeeds();
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
          string strNameTag = String.Format("siteName{0}", i);
          string strURLTag = String.Format("siteURL{0}", i);
          string strEncodingTag = String.Format("siteEncoding{0}", i);

          string strName = xmlreader.GetValueAsString("rss", strNameTag, "");
          string strURL = xmlreader.GetValueAsString("rss", strURLTag, "");
          string strEncoding = xmlreader.GetValueAsString("rss", strEncodingTag, "windows-1252");

          if (strName.Length > 0 && strURL.Length > 0)
          {
            this.listBox.Items.Add(strName);
          }
        }

        // general settings
        textRefreshInterval.Text = xmlreader.GetValueAsString("rss", "iRefreshTime", "15");
        checkAutoRefresh.Checked = false;
        if (xmlreader.GetValueAsInt("rss", "bAutoRefresh", 0) != 0)
        {
          checkAutoRefresh.Checked = true;
        }
      }
    }

    private void button3_Click(object sender, EventArgs e)
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("rss", "iRefreshTime", textRefreshInterval.Text);

        int iAutoRefresh = 0;
        if (checkAutoRefresh.Checked)
        {
          iAutoRefresh = 1;
        }
        xmlwriter.SetValue("rss", "bAutoRefresh", iAutoRefresh.ToString());
      }
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
      strButtonImage = "";
      strButtonImageFocus = "";
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
      this.buttonDelete = new MPButton();
      this.buttonEdit = new MPButton();
      this.listBox = new ListBox();
      this.buttonAdd = new MPButton();
      this.button3 = new MPButton();
      this.checkAutoRefresh = new MPCheckBox();
      this.labelRefresh = new MPLabel();
      this.textRefreshInterval = new MPTextBox();
      this.mpGroupBox1 = new MPGroupBox();
      this.mpGroupBox2 = new MPGroupBox();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonDelete
      // 
      this.buttonDelete.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.buttonDelete.Location = new Point(368, 185);
      this.buttonDelete.Name = "buttonDelete";
      this.buttonDelete.Size = new Size(88, 23);
      this.buttonDelete.TabIndex = 3;
      this.buttonDelete.Text = "Delete Site";
      this.buttonDelete.UseVisualStyleBackColor = true;
      this.buttonDelete.Click += new EventHandler(this.deleteSite);
      // 
      // buttonEdit
      // 
      this.buttonEdit.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.buttonEdit.Location = new Point(274, 185);
      this.buttonEdit.Name = "buttonEdit";
      this.buttonEdit.Size = new Size(88, 23);
      this.buttonEdit.TabIndex = 2;
      this.buttonEdit.Text = "Edit Site";
      this.buttonEdit.UseVisualStyleBackColor = true;
      this.buttonEdit.Click += new EventHandler(this.editSite);
      // 
      // listBox
      // 
      this.listBox.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                                              | AnchorStyles.Left)
                                             | AnchorStyles.Right)));
      this.listBox.Location = new Point(6, 19);
      this.listBox.Name = "listBox";
      this.listBox.Size = new Size(450, 147);
      this.listBox.TabIndex = 5;
      // 
      // buttonAdd
      // 
      this.buttonAdd.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.buttonAdd.Location = new Point(180, 185);
      this.buttonAdd.Name = "buttonAdd";
      this.buttonAdd.Size = new Size(88, 23);
      this.buttonAdd.TabIndex = 1;
      this.buttonAdd.Text = "Add Site";
      this.buttonAdd.UseVisualStyleBackColor = true;
      this.buttonAdd.Click += new EventHandler(this.addSite);
      // 
      // button3
      // 
      this.button3.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.button3.DialogResult = DialogResult.Cancel;
      this.button3.Location = new Point(402, 279);
      this.button3.Name = "button3";
      this.button3.Size = new Size(72, 22);
      this.button3.TabIndex = 12;
      this.button3.Text = "&Done";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new EventHandler(this.button3_Click);
      // 
      // checkAutoRefresh
      // 
      this.checkAutoRefresh.AutoSize = true;
      this.checkAutoRefresh.FlatStyle = FlatStyle.Popup;
      this.checkAutoRefresh.Location = new Point(6, 19);
      this.checkAutoRefresh.Name = "checkAutoRefresh";
      this.checkAutoRefresh.Size = new Size(122, 17);
      this.checkAutoRefresh.TabIndex = 4;
      this.checkAutoRefresh.Text = "Auto refresh enabled";
      this.checkAutoRefresh.UseVisualStyleBackColor = true;
      // 
      // labelRefresh
      // 
      this.labelRefresh.AutoSize = true;
      this.labelRefresh.Location = new Point(21, 43);
      this.labelRefresh.Name = "labelRefresh";
      this.labelRefresh.Size = new Size(109, 13);
      this.labelRefresh.TabIndex = 13;
      this.labelRefresh.Text = "Refresh interval (min):";
      this.labelRefresh.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // textRefreshInterval
      // 
      this.textRefreshInterval.BorderColor = Color.Empty;
      this.textRefreshInterval.Location = new Point(142, 40);
      this.textRefreshInterval.Name = "textRefreshInterval";
      this.textRefreshInterval.Size = new Size(53, 20);
      this.textRefreshInterval.TabIndex = 14;
      this.textRefreshInterval.Text = "15";
      this.textRefreshInterval.TextAlign = HorizontalAlignment.Center;
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                  | AnchorStyles.Left)
                                                 | AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.listBox);
      this.mpGroupBox1.Controls.Add(this.buttonAdd);
      this.mpGroupBox1.Controls.Add(this.buttonEdit);
      this.mpGroupBox1.Controls.Add(this.buttonDelete);
      this.mpGroupBox1.FlatStyle = FlatStyle.Popup;
      this.mpGroupBox1.Location = new Point(12, 11);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new Size(462, 213);
      this.mpGroupBox1.TabIndex = 15;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Add news sites here and edit their options";
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
      this.mpGroupBox2.Controls.Add(this.labelRefresh);
      this.mpGroupBox2.Controls.Add(this.checkAutoRefresh);
      this.mpGroupBox2.Controls.Add(this.textRefreshInterval);
      this.mpGroupBox2.FlatStyle = FlatStyle.Popup;
      this.mpGroupBox2.Location = new Point(12, 230);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new Size(215, 71);
      this.mpGroupBox2.TabIndex = 16;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Auto refresh settings";
      // 
      // SetupForm
      // 
      this.AcceptButton = this.button3;
      this.AutoScaleDimensions = new SizeF(6F, 13F);
      this.CancelButton = this.button3;
      this.ClientSize = new Size(486, 313);
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.button3);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.Name = "SetupForm";
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "RSS News - Setup";
      this.Load += new EventHandler(this.SetupForm_Load);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion
  }
}