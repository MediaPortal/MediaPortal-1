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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class BaseShares : SectionSettings
  {
    public class ShareData
    {
      public string Name;
      public string Folder;
      public string PinCode;

      public bool IsRemote = false;
      public string Server = string.Empty;
      public string LoginName = string.Empty;
      public string PassWord = string.Empty;
      public string RemoteFolder = string.Empty;
      public int Port = 21;
      public bool ActiveConnection = true;
      public Layout DefaultLayout = MediaPortal.GUI.Library.GUIFacadeControl.Layout.List;
      public bool ScanShare = false;
      public bool CreateThumbs = true;
      public bool EachFolderIsMovie = false;
      public bool EnableWakeOnLan = false;
      public bool DonotFolderJpgIfPin = true;
      
      public bool HasPinCode
      {
        get { return (PinCode.Length > 0); }
      }

      public ShareData(string name, string folder, string pinCode, bool thumbs)
      {
        this.Name = name;
        this.Folder = folder;
        this.PinCode = pinCode;
        this.CreateThumbs = thumbs;
      }
    }

    protected const int MaximumShares = 128;

    private MPGroupBox groupBox1;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private MPButton deleteButton;
    private MPButton editButton;
    private MPButton addButton;
    private MPListView sharesListView;
    private ColumnHeader columnHeader3;
    private MPCheckBox checkBoxRemember;
    private MPCheckBox checkBoxAddOpticalDiskDrives;
    private MPCheckBox checkBoxSwitchRemovableDrive;
    private ColumnHeader columnHeader4;
    private IContainer components = null;
    private MPButton mpButtonWOL;
    private ColumnHeader columnHeader5;
    private ColumnHeader columnHeader6;

    private string selectedSection = string.Empty;

    public BaseShares()
      : base("<Unknown>")
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    public BaseShares(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpButtonWOL = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxSwitchRemovableDrive = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxAddOpticalDiskDrives = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxRemember = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.deleteButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.editButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.addButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.sharesListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.mpButtonWOL);
      this.groupBox1.Controls.Add(this.checkBoxSwitchRemovableDrive);
      this.groupBox1.Controls.Add(this.checkBoxAddOpticalDiskDrives);
      this.groupBox1.Controls.Add(this.checkBoxRemember);
      this.groupBox1.Controls.Add(this.deleteButton);
      this.groupBox1.Controls.Add(this.editButton);
      this.groupBox1.Controls.Add(this.addButton);
      this.groupBox1.Controls.Add(this.sharesListView);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(6, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(462, 408);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // mpButtonWOL
      // 
      this.mpButtonWOL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonWOL.Location = new System.Drawing.Point(342, 359);
      this.mpButtonWOL.Name = "mpButtonWOL";
      this.mpButtonWOL.Size = new System.Drawing.Size(104, 23);
      this.mpButtonWOL.TabIndex = 7;
      this.mpButtonWOL.Text = "WOL parameters";
      this.mpButtonWOL.UseVisualStyleBackColor = true;
      this.mpButtonWOL.Click += new System.EventHandler(this.mpButtonWOL_Click);
      // 
      // checkBoxSwitchRemovableDrive
      // 
      this.checkBoxSwitchRemovableDrive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.checkBoxSwitchRemovableDrive.AutoSize = true;
      this.checkBoxSwitchRemovableDrive.Checked = true;
      this.checkBoxSwitchRemovableDrive.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxSwitchRemovableDrive.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxSwitchRemovableDrive.Location = new System.Drawing.Point(16, 380);
      this.checkBoxSwitchRemovableDrive.Name = "checkBoxSwitchRemovableDrive";
      this.checkBoxSwitchRemovableDrive.Size = new System.Drawing.Size(254, 17);
      this.checkBoxSwitchRemovableDrive.TabIndex = 6;
      this.checkBoxSwitchRemovableDrive.Text = "Automatically switch to inserted removable drives";
      this.checkBoxSwitchRemovableDrive.UseVisualStyleBackColor = true;
      // 
      // checkBoxAddOpticalDiskDrives
      // 
      this.checkBoxAddOpticalDiskDrives.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.checkBoxAddOpticalDiskDrives.AutoSize = true;
      this.checkBoxAddOpticalDiskDrives.Checked = true;
      this.checkBoxAddOpticalDiskDrives.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxAddOpticalDiskDrives.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAddOpticalDiskDrives.Location = new System.Drawing.Point(16, 357);
      this.checkBoxAddOpticalDiskDrives.Name = "checkBoxAddOpticalDiskDrives";
      this.checkBoxAddOpticalDiskDrives.Size = new System.Drawing.Size(194, 17);
      this.checkBoxAddOpticalDiskDrives.TabIndex = 5;
      this.checkBoxAddOpticalDiskDrives.Text = "Automatically add optical disk drives";
      this.checkBoxAddOpticalDiskDrives.UseVisualStyleBackColor = true;
      // 
      // checkBoxRemember
      // 
      this.checkBoxRemember.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.checkBoxRemember.AutoSize = true;
      this.checkBoxRemember.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxRemember.Location = new System.Drawing.Point(16, 334);
      this.checkBoxRemember.Name = "checkBoxRemember";
      this.checkBoxRemember.Size = new System.Drawing.Size(149, 17);
      this.checkBoxRemember.TabIndex = 1;
      this.checkBoxRemember.Text = "Remember last used folder";
      this.checkBoxRemember.UseVisualStyleBackColor = true;
      // 
      // deleteButton
      // 
      this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.deleteButton.Enabled = false;
      this.deleteButton.Location = new System.Drawing.Point(374, 331);
      this.deleteButton.Name = "deleteButton";
      this.deleteButton.Size = new System.Drawing.Size(72, 22);
      this.deleteButton.TabIndex = 4;
      this.deleteButton.Text = "Delete";
      this.deleteButton.UseVisualStyleBackColor = true;
      this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
      // 
      // editButton
      // 
      this.editButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.editButton.Enabled = false;
      this.editButton.Location = new System.Drawing.Point(294, 331);
      this.editButton.Name = "editButton";
      this.editButton.Size = new System.Drawing.Size(72, 22);
      this.editButton.TabIndex = 3;
      this.editButton.Text = "Edit";
      this.editButton.UseVisualStyleBackColor = true;
      this.editButton.Click += new System.EventHandler(this.editButton_Click);
      // 
      // addButton
      // 
      this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.addButton.Location = new System.Drawing.Point(214, 331);
      this.addButton.Name = "addButton";
      this.addButton.Size = new System.Drawing.Size(72, 22);
      this.addButton.TabIndex = 2;
      this.addButton.Text = "Add";
      this.addButton.UseVisualStyleBackColor = true;
      this.addButton.Click += new System.EventHandler(this.addButton_Click);
      // 
      // sharesListView
      // 
      this.sharesListView.AllowDrop = true;
      this.sharesListView.AllowRowReorder = true;
      this.sharesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.sharesListView.CheckBoxes = true;
      this.sharesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3,
            this.columnHeader2,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
      this.sharesListView.FullRowSelect = true;
      this.sharesListView.Location = new System.Drawing.Point(16, 24);
      this.sharesListView.Name = "sharesListView";
      this.sharesListView.Size = new System.Drawing.Size(430, 301);
      this.sharesListView.TabIndex = 0;
      this.sharesListView.UseCompatibleStateImageBehavior = false;
      this.sharesListView.View = System.Windows.Forms.View.Details;
      this.sharesListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.sharesListView_ItemCheck);
      this.sharesListView.SelectedIndexChanged += new System.EventHandler(this.sharesListView_SelectedIndexChanged);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Name";
      this.columnHeader1.Width = 106;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Pin";
      this.columnHeader3.Width = 57;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Folder";
      this.columnHeader2.Width = 210;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Thumbs";
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "MAC Address";
      this.columnHeader5.Width = 120;
      // 
      // columnHeader6
      // 
      this.columnHeader6.Text = "WOL";
      // 
      // BaseShares
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "BaseShares";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private ListViewItem currentlyCheckedItem = null;

    private void addButton_Click(object sender, EventArgs e)
    {
      EditShareForm editShare = new EditShareForm();
      
      if (selectedSection == "movies")
      {
        editShare.labelCreateThumbs.Visible = true;
        editShare.cbCreateThumbs.Visible = true;
        editShare.CreateThumbs = true;

        editShare.cbEachFolderIsMovie.Visible = true;
        editShare.EachFolderIsMovie = false;
      }
      else
      {
        editShare.labelCreateThumbs.Visible = false;
        editShare.cbCreateThumbs.Visible = false;
        editShare.CreateThumbs = true;

        editShare.cbEachFolderIsMovie.Visible = false;
        editShare.EachFolderIsMovie = false;
      }

      editShare.DonotFolderJpgIfPin = true;
      DialogResult dialogResult = editShare.ShowDialog(this);

      if (dialogResult == DialogResult.OK)
      {
        ShareData shareData = new ShareData(editShare.ShareName, editShare.Folder, editShare.PinCode, editShare.CreateThumbs);
        shareData.IsRemote = editShare.IsRemote;
        shareData.Server = editShare.Server;
        shareData.LoginName = editShare.LoginName;
        shareData.PassWord = editShare.PassWord;
        shareData.Port = editShare.Port;
        shareData.ActiveConnection = editShare.ActiveConnection;
        shareData.RemoteFolder = editShare.RemoteFolder;
        shareData.DefaultLayout = ProperLayoutFromDefault(editShare.View);
        shareData.EnableWakeOnLan = editShare.EnableWakeOnLan;
        shareData.DonotFolderJpgIfPin = editShare.DonotFolderJpgIfPin;

        //CreateThumbs
        if (selectedSection == "movies")
        {
          //int drivetype = Util.Utils.getDriveType(shareData.Folder);
          //if (drivetype != 2 && 
          //    drivetype != 5)
          //{
          shareData.CreateThumbs = editShare.CreateThumbs;
          shareData.EachFolderIsMovie = editShare.EachFolderIsMovie;
          //}
          //else
          //{
          //  shareData.CreateThumbs = false;
          //}
        }
        AddShare(shareData, currentlyCheckedItem == null);
      }
    }

    protected void AddShare(ShareData shareData, bool check)
    {
      ListViewItem listItem =
        new ListViewItem(new string[]
                           {
                             shareData.Name, 
                             shareData.HasPinCode ? "Yes" : "No", 
                             shareData.Folder,
                             shareData.CreateThumbs ? "Yes" : "No", 
                             shareData.ActiveConnection ? "Yes" : "No",
                             shareData.EnableWakeOnLan ? "Yes" : "No"
                           });

      if (shareData.IsRemote)
      {
        listItem.SubItems[2].Text = String.Format("ftp://{0}:{1}{2}", shareData.Server, shareData.Port,
                                                  shareData.RemoteFolder);
      }
      listItem.Tag = shareData;
      listItem.Checked = check;
      if (check)
      {
        currentlyCheckedItem = listItem;
      }

      if (!Util.Utils.IsNetwork(shareData.Folder))
      {
        listItem.SubItems[4].Text = string.Empty;
      }
      else
      {
        using (Profile.Settings xmlreader = new MPSettings())
        {
          string detectedFolderName = "";
          if (!Util.Utils.IsUNCNetwork(shareData.Folder))
          {
            // Check if letter drive is a network drive
            detectedFolderName = Util.Utils.FindUNCPaths(shareData.Folder);
          }
          if (Util.Utils.IsUNCNetwork(detectedFolderName))
          {
            listItem.SubItems[4].Text = xmlreader.GetValueAsString("macAddress", Util.Utils.GetServerNameFromUNCPath(detectedFolderName), null);
          }
          else if (Util.Utils.IsUNCNetwork(shareData.Folder))
          {
            listItem.SubItems[4].Text = xmlreader.GetValueAsString("macAddress", Util.Utils.GetServerNameFromUNCPath(shareData.Folder), null);
          }
        }
      }

      sharesListView.Items.Add(listItem);
    }

    private void editButton_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem selectedItem in sharesListView.SelectedItems)
      {
        ShareData shareData = selectedItem.Tag as ShareData;

        if (shareData != null)
        {
          EditShareForm editShare = new EditShareForm();

          editShare.ShareName = shareData.Name;
          editShare.PinCode = shareData.PinCode;
          editShare.Folder = shareData.Folder;

          editShare.IsRemote = shareData.IsRemote;
          editShare.Server = shareData.Server;
          editShare.Port = shareData.Port;
          editShare.ActiveConnection = shareData.ActiveConnection;
          editShare.LoginName = shareData.LoginName;
          editShare.PassWord = shareData.PassWord;
          editShare.RemoteFolder = shareData.RemoteFolder;
          editShare.View = ProperDefaultFromLayout(shareData.DefaultLayout);
          editShare.EnableWakeOnLan = shareData.EnableWakeOnLan;
          editShare.DonotFolderJpgIfPin = shareData.DonotFolderJpgIfPin;

          // CreateThumbs
          int drivetype = 0;
          if (!shareData.EnableWakeOnLan)
          {
            if (UNCTools.UNCFileFolderExists(shareData.Folder))
            {
              drivetype = Util.Utils.getDriveType(shareData.Folder);
            }
          }
          else
          {
            drivetype = Util.Utils.getDriveType(shareData.Folder);
          }

          if (selectedSection == "movies") // && 
                           //drivetype != 2 && 
                           //drivetype != 5)
          {
            editShare.labelCreateThumbs.Visible = true;
            editShare.cbCreateThumbs.Visible = true;
            editShare.CreateThumbs = shareData.CreateThumbs;

            editShare.cbEachFolderIsMovie.Visible = true;
            editShare.EachFolderIsMovie = shareData.EachFolderIsMovie;
          }
          else
          {
            editShare.labelCreateThumbs.Visible = false;
            editShare.cbCreateThumbs.Visible = false;
            editShare.CreateThumbs = true;

            editShare.cbEachFolderIsMovie.Visible = false;
            editShare.EachFolderIsMovie = false;
          }

          DialogResult dialogResult = editShare.ShowDialog(this);

          if (dialogResult == DialogResult.OK)
          {
            shareData.Name = editShare.ShareName;
            shareData.Folder = editShare.Folder;
            shareData.PinCode = editShare.PinCode;

            shareData.IsRemote = editShare.IsRemote;
            shareData.Server = editShare.Server;
            shareData.LoginName = editShare.LoginName;
            shareData.PassWord = editShare.PassWord;
            shareData.Port = editShare.Port;
            shareData.ActiveConnection = editShare.ActiveConnection;
            shareData.RemoteFolder = editShare.RemoteFolder;
            shareData.DefaultLayout = ProperLayoutFromDefault(editShare.View);
            shareData.EnableWakeOnLan = editShare.EnableWakeOnLan;
            shareData.DonotFolderJpgIfPin = editShare.DonotFolderJpgIfPin;

            //CreateThumbs
            if (selectedSection == "movies")
            {
              //if (drivetype != 2 && 
              //    drivetype != 5)
              //{
              shareData.CreateThumbs = editShare.CreateThumbs;
              shareData.EachFolderIsMovie = editShare.EachFolderIsMovie;
              //}
              //else
              //{
              //  shareData.CreateThumbs = false;
              //}
            }
            selectedItem.Tag = shareData;

            selectedItem.SubItems[0].Text = shareData.Name;
            selectedItem.SubItems[1].Text = shareData.HasPinCode ? "Yes" : "No";
            selectedItem.SubItems[2].Text = shareData.Folder;
            selectedItem.SubItems[3].Text = shareData.CreateThumbs ? "Yes" : "No";
            selectedItem.SubItems[5].Text = shareData.EnableWakeOnLan ? "Yes" : "No";

            if (!Util.Utils.IsNetwork(shareData.Folder))
            {
              selectedItem.SubItems[4].Text = string.Empty;
            }
            else
            {
              using (Profile.Settings xmlreader = new MPSettings())
              {
                string detectedFolderName = "";
                if (!Util.Utils.IsUNCNetwork(shareData.Folder))
                {
                  // Check if letter drive is a network drive
                  detectedFolderName = Util.Utils.FindUNCPaths(shareData.Folder);
                }
                if (Util.Utils.IsUNCNetwork(detectedFolderName))
                {
                  selectedItem.SubItems[4].Text = xmlreader.GetValueAsString("macAddress", Util.Utils.GetServerNameFromUNCPath(detectedFolderName), null);
                }
                else if (Util.Utils.IsUNCNetwork(shareData.Folder))
                {
                  selectedItem.SubItems[4].Text = xmlreader.GetValueAsString("macAddress", Util.Utils.GetServerNameFromUNCPath(shareData.Folder), null);
                }
              }
            }

            if (shareData.IsRemote)
            {
              selectedItem.SubItems[2].Text = String.Format("ftp://{0}:{1}{2}", shareData.Server, shareData.Port,
                                                            shareData.RemoteFolder);
            }
          }
        }
      }
    }

    private void deleteButton_Click(object sender, EventArgs e)
    {
      int selectedItems = sharesListView.SelectedIndices.Count;

      for (int index = 0; index < selectedItems; index++)
      {
        sharesListView.Items.RemoveAt(sharesListView.SelectedIndices[0]);
      }
    }

    private void sharesListView_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      if (e.NewValue == CheckState.Checked)
      {
        //
        // Check if the new selected item is the same as the current one
        //
        if (sharesListView.Items[e.Index] != currentlyCheckedItem)
        {
          //
          // We have a new selection
          //
          if (currentlyCheckedItem != null && currentlyCheckedItem.Index != -1)
          {
            currentlyCheckedItem.Checked = false;
          }
          currentlyCheckedItem = sharesListView.Items[e.Index];
        }
      }

      if (e.NewValue == CheckState.Unchecked)
      {
        //
        // Check if the new selected item is the same as the current one
        //
        if (sharesListView.Items[e.Index] == currentlyCheckedItem)
        {
          currentlyCheckedItem = null;
        }
      }
    }

    public ListView.ListViewItemCollection CurrentShares
    {
      get { return sharesListView.Items; }
    }

    public ListViewItem DefaultShare
    {
      get { return currentlyCheckedItem; }
    }

    public bool RememberLastFolder
    {
      get { return checkBoxRemember.Checked; }
      set { checkBoxRemember.Checked = value; }
    }

    public bool AddOpticalDiskDrives
    {
      get { return checkBoxAddOpticalDiskDrives.Checked; }
      set { checkBoxAddOpticalDiskDrives.Checked = value; }
    }

    public bool SwitchRemovableDrives
    {
      get { return checkBoxSwitchRemovableDrive.Checked; }
      set { checkBoxSwitchRemovableDrive.Checked = value; }
    }

    public enum DriveType
    {
      Removable = 2,
      Fixed = 3,
      RemoteDisk = 4,
      CD = 5,
      DVD = 5,
      RamDisk = 6
    }

    public void AddStaticShares(DriveType driveType, string defaultName)
    {
      string[] drives = Environment.GetLogicalDrives();

      foreach (string drive in drives)
      {
        if (Util.Utils.getDriveType(drive) == (int)driveType)
        {
          bool driveFound = false;
          string driveName = Util.Utils.GetDriveName(drive);

          if (driveName.Length == 0)
          {
            string driveLetter = drive.Substring(0, 1).ToUpperInvariant();
            driveName = String.Format("{0} {1}:", defaultName, driveLetter);
          }

          //
          // Check if the share already exists
          //
          foreach (ListViewItem listItem in CurrentShares)
          {
            if (listItem.SubItems[2].Text == drive)
            {
              driveFound = true;
              break;
            }
          }

          if (driveFound == false)
          {
            //
            // Add share
            //
            string name = "";
            switch (driveType)
            {
              case DriveType.Removable:
                name = String.Format("({0}:) Removable", drive.Substring(0, 1).ToUpperInvariant());
                break;
              case DriveType.Fixed:
                name = String.Format("({0}:) Fixed", drive.Substring(0, 1).ToUpperInvariant());
                break;
              case DriveType.RemoteDisk:
                name = String.Format("({0}:) Remote", drive.Substring(0, 1).ToUpperInvariant());
                break;
              case DriveType.DVD: // or cd
                name = String.Format("({0}:) CD/DVD", drive.Substring(0, 1).ToUpperInvariant());
                break;
              case DriveType.RamDisk:
                name = String.Format("({0}:) Ram", drive.Substring(0, 1).ToUpperInvariant());
                break;
            }
            if (driveType == DriveType.Fixed || driveType == DriveType.RemoteDisk)
            {
              AddShare(new ShareData(name, drive, string.Empty, true), false);
            }
            else
            {
              AddShare(new ShareData(name, drive, string.Empty, false), false);
            }
          }
        }
      }
    }

    private void sharesListView_SelectedIndexChanged(object sender, EventArgs e)
    {
      editButton.Enabled = deleteButton.Enabled = (sharesListView.SelectedItems.Count > 0);
    }

    public override object GetSetting(string name)
    {
      switch (name.ToLowerInvariant())
      {
        case "shares.available":
          return CurrentShares.Count > 0;

        case "shares":
          ArrayList shares = new ArrayList();

          foreach (ListViewItem listItem in CurrentShares)
          {
            shares.Add(listItem.SubItems[2].Text);
          }
          return shares;

        case "sharesdata":
          List<ShareData> sharesdata = new List<ShareData>();

          foreach (ListViewItem listItem in CurrentShares)
          {
            sharesdata.Add((ShareData)listItem.Tag);
          }
          return sharesdata;
      }

      return null;
    }

    protected void LoadSettings(string section, string defaultSharePath)
    {
      selectedSection = section;
      using (Settings xmlreader = new MPSettings())
      {
        string defaultShare = xmlreader.GetValueAsString(section, "default", "");
        RememberLastFolder = xmlreader.GetValueAsBool(section, "rememberlastfolder", false);
        AddOpticalDiskDrives = xmlreader.GetValueAsBool(section, "AddOpticalDiskDrives", true);
        SwitchRemovableDrives = xmlreader.GetValueAsBool(section, "SwitchRemovableDrives", true);

        for (int index = 0; index < MaximumShares; index++)
        {
          string shareName = String.Format("sharename{0}", index);
          string sharePath = String.Format("sharepath{0}", index);
          string sharePin = String.Format("pincode{0}", index);

          string shareType = String.Format("sharetype{0}", index);
          string shareServer = String.Format("shareserver{0}", index);
          string shareLogin = String.Format("sharelogin{0}", index);
          string sharePwd = String.Format("sharepassword{0}", index);
          string sharePort = String.Format("shareport{0}", index);
          string shareRemotePath = String.Format("shareremotepath{0}", index);
          string shareViewPath = String.Format("shareview{0}", index);
          string sharewakeonlan = String.Format("sharewakeonlan{0}", index);
          string sharedonotfolderjpgifpin = String.Format("sharedonotfolderjpgifpin{0}", index);
          
          string shareNameData = xmlreader.GetValueAsString(section, shareName, "");
          string sharePathData = xmlreader.GetValueAsString(section, sharePath, "");
          string sharePinData = Util.Utils.DecryptPassword(xmlreader.GetValueAsString(section, sharePin, ""));

          // provide default shares
          if (index == 0 && shareNameData == string.Empty)
          {
            shareNameData = VirtualDirectory.GetShareNameDefault(defaultSharePath);
            sharePathData = defaultSharePath;
            sharePinData = string.Empty;

            AddStaticShares(DriveType.DVD, "DVD");
          }

          bool shareTypeData = xmlreader.GetValueAsBool(section, shareType, false);
          string shareServerData = xmlreader.GetValueAsString(section, shareServer, "");
          string shareLoginData = xmlreader.GetValueAsString(section, shareLogin, "");
          string sharePwdData = Util.Utils.DecryptPassword(xmlreader.GetValueAsString(section, sharePwd, ""));
          int sharePortData = xmlreader.GetValueAsInt(section, sharePort, 21);
          string shareRemotePathData = xmlreader.GetValueAsString(section, shareRemotePath, "/");
          int shareLayout = xmlreader.GetValueAsInt(section, shareViewPath,
                                                    (int)MediaPortal.GUI.Library.GUIFacadeControl.Layout.List);

          bool shareWakeOnLan = xmlreader.GetValueAsBool(section, sharewakeonlan, false);
          bool sharedonotFolderJpgIfPin = xmlreader.GetValueAsBool(section, sharedonotfolderjpgifpin, true);
          
          // For Music Shares, we can indicate, if we want to scan them every time
          bool shareScanData = false;
          if (section == "music" || section == "movies")
          {
            string shareScan = String.Format("sharescan{0}", index);
            shareScanData = xmlreader.GetValueAsBool(section, shareScan, true);
          }
          // For Movies Shares, we can indicate, if we want to create thumbs
          bool thumbs = true;
          bool folderIsMovie = false;

          if (section == "movies")
          {
            string thumbsCreate = String.Format("videothumbscreate{0}", index);
            thumbs = xmlreader.GetValueAsBool(section, thumbsCreate, true);
            string eachFolderIsMovie = String.Format("eachfolderismovie{0}", index);
            folderIsMovie = xmlreader.GetValueAsBool(section, eachFolderIsMovie, false);
          }

          if (!String.IsNullOrEmpty(shareNameData))
          {
            ShareData newShare = new ShareData(shareNameData, sharePathData, sharePinData, thumbs);
            newShare.IsRemote = shareTypeData;
            newShare.Server = shareServerData;
            newShare.LoginName = shareLoginData;
            newShare.PassWord = sharePwdData;
            newShare.Port = sharePortData;
            newShare.RemoteFolder = shareRemotePathData;
            newShare.DefaultLayout = (Layout)shareLayout;
            newShare.EnableWakeOnLan = shareWakeOnLan;
            newShare.DonotFolderJpgIfPin = sharedonotFolderJpgIfPin;
            
            if (section == "music" || section == "movies")
            {
              newShare.ScanShare = shareScanData;
            }
            // ThumbsCreate
            if (section == "movies")
            {
              newShare.CreateThumbs = thumbs;
              newShare.EachFolderIsMovie = folderIsMovie;
            }
            AddShare(newShare, shareNameData.Equals(defaultShare));
          }
        }
        if (AddOpticalDiskDrives)
        {
          AddStaticShares(DriveType.DVD, "DVD");
        }
        if (section == "movies")
        {
          sharesListView.Columns[2].Width = 210;
          if (!sharesListView.Columns.Contains(columnHeader4))
               sharesListView.Columns.Add(columnHeader4);
          if (!sharesListView.Columns.Contains(columnHeader6))
            sharesListView.Columns.Add(columnHeader6);
          sharesListView.Columns[3].Width = 60;
        }
        else
        {
          sharesListView.Columns[3].Width = 0;
         // if (sharesListView.Columns.Contains(columnHeader4))
         // sharesListView.Columns.Remove(columnHeader4);
        }
      }
    }

    protected void SaveSettings(string section)
    {
      if (AddOpticalDiskDrives)
      {
        AddStaticShares(DriveType.DVD, "DVD");
      }

      using (Settings xmlwriter = new MPSettings())
      {
        string defaultShare = string.Empty;

        for (int index = 0; index < MaximumShares; index++)
        {
          string shareName = String.Format("sharename{0}", index);
          string sharePath = String.Format("sharepath{0}", index);
          string sharePin = String.Format("pincode{0}", index);
          string shareType = String.Format("sharetype{0}", index);
          string shareServer = String.Format("shareserver{0}", index);
          string shareLogin = String.Format("sharelogin{0}", index);
          string sharePwd = String.Format("sharepassword{0}", index);
          string sharePort = String.Format("shareport{0}", index);
          string shareRemotePath = String.Format("shareremotepath{0}", index);
          string shareViewPath = String.Format("shareview{0}", index);
          string sharewakeonlan = String.Format("sharewakeonlan{0}", index);
          string sharedonotfolderjpgifpin = String.Format("sharedonotfolderjpgifpin{0}", index);
          
          xmlwriter.RemoveEntry(section, shareName);
          xmlwriter.RemoveEntry(section, sharePath);
          xmlwriter.RemoveEntry(section, sharePin);
          xmlwriter.RemoveEntry(section, shareType);
          xmlwriter.RemoveEntry(section, shareServer);
          xmlwriter.RemoveEntry(section, shareLogin);
          xmlwriter.RemoveEntry(section, sharePwd);
          xmlwriter.RemoveEntry(section, sharePort);
          xmlwriter.RemoveEntry(section, shareRemotePath);
          xmlwriter.RemoveEntry(section, shareViewPath);
          xmlwriter.RemoveEntry(section, sharewakeonlan);
          xmlwriter.RemoveEntry(section, sharedonotfolderjpgifpin);
          
          if (section == "music" || section == "movies")
          {
            string shareScan = String.Format("sharescan{0}", index);
            xmlwriter.RemoveEntry(section, shareScan);
          }

          if (section == "movies")
          {
            string thumbs = String.Format("videothumbscreate{0}", index);
            xmlwriter.RemoveEntry(section, thumbs);

            string movieFolder = String.Format("eachfolderismovie{0}", index);
            xmlwriter.RemoveEntry(section, movieFolder);
          }
          
          string shareNameData = string.Empty;
          string sharePathData = string.Empty;
          string sharePinData = string.Empty;
          bool shareTypeData = false;
          string shareServerData = string.Empty;
          string shareLoginData = string.Empty;
          string sharePwdData = string.Empty;
          int sharePortData = 21;
          string shareRemotePathData = string.Empty;
          int shareLayout = (int)MediaPortal.GUI.Library.GUIFacadeControl.Layout.List;
          bool shareScanData = false;
          //ThumbsCreate (default true)
          bool thumbsCreate = true;
          bool folderIsMovie = false;
          bool shareWakeOnLan = false;
          bool sharedonotFolderJpgIfPin = true;

          if (CurrentShares != null && CurrentShares.Count > index)
          {
            ShareData shareData = CurrentShares[index].Tag as ShareData;

            if (shareData != null && !String.IsNullOrEmpty(shareData.Name))
            {
              shareNameData = shareData.Name;
              sharePathData = shareData.Folder;
              sharePinData = shareData.PinCode;
              shareTypeData = shareData.IsRemote;
              shareServerData = shareData.Server;
              shareLoginData = shareData.LoginName;
              sharePwdData = shareData.PassWord;
              sharePortData = shareData.Port;
              shareRemotePathData = shareData.RemoteFolder;
              shareLayout = (int)shareData.DefaultLayout;
              shareScanData = shareData.ScanShare;
              // ThumbsCreate
              thumbsCreate = shareData.CreateThumbs;
              folderIsMovie = shareData.EachFolderIsMovie;
              shareWakeOnLan = shareData.EnableWakeOnLan;
              sharedonotFolderJpgIfPin = shareData.DonotFolderJpgIfPin;

              if (CurrentShares[index] == DefaultShare)
              {
                defaultShare = shareNameData;
              }

              xmlwriter.SetValue(section, shareName, shareNameData);
              xmlwriter.SetValue(section, sharePath, sharePathData);
              xmlwriter.SetValue(section, sharePin, Util.Utils.EncryptPassword(sharePinData));
              xmlwriter.SetValueAsBool(section, shareType, shareTypeData);
              xmlwriter.SetValue(section, shareServer, shareServerData);
              xmlwriter.SetValue(section, shareLogin, shareLoginData);
              xmlwriter.SetValue(section, sharePwd, Util.Utils.EncryptPassword(sharePwdData));
              xmlwriter.SetValue(section, sharePort, sharePortData.ToString());
              xmlwriter.SetValue(section, shareRemotePath, shareRemotePathData);
              xmlwriter.SetValue(section, shareViewPath, shareLayout);
              xmlwriter.SetValueAsBool(section, sharewakeonlan, shareWakeOnLan);
              xmlwriter.SetValueAsBool(section, sharedonotfolderjpgifpin, sharedonotFolderJpgIfPin);

              if (section == "music" || section == "movies")
              {
                string shareScan = String.Format("sharescan{0}", index);
                xmlwriter.SetValueAsBool(section, shareScan, shareScanData);
              }
              //ThumbsCreate
              if (section == "movies")
              {
                string thumbs = String.Format("videothumbscreate{0}", index);
                xmlwriter.SetValueAsBool(section, thumbs, thumbsCreate);

                string folderMovie = String.Format("eachfolderismovie{0}", index);
                xmlwriter.SetValueAsBool(section, folderMovie, folderIsMovie);
              }
            }
          }
        }
        xmlwriter.SetValue(section, "default", defaultShare);
        xmlwriter.SetValueAsBool(section, "rememberlastfolder", RememberLastFolder);
        xmlwriter.SetValueAsBool(section, "AddOpticalDiskDrives", AddOpticalDiskDrives);
        xmlwriter.SetValueAsBool(section, "SwitchRemovableDrives", SwitchRemovableDrives);
      }
    }

    public Layout ProperLayoutFromDefault(int defaultView)
    {
      switch (defaultView)
      {
        case 1: return MediaPortal.GUI.Library.GUIFacadeControl.Layout.SmallIcons;
        case 2: return MediaPortal.GUI.Library.GUIFacadeControl.Layout.LargeIcons;
        case 3: return MediaPortal.GUI.Library.GUIFacadeControl.Layout.AlbumView;
        case 4: return MediaPortal.GUI.Library.GUIFacadeControl.Layout.Filmstrip;
        case 5: return MediaPortal.GUI.Library.GUIFacadeControl.Layout.CoverFlow;
        default: return MediaPortal.GUI.Library.GUIFacadeControl.Layout.List;
      }
    }

    public int ProperDefaultFromLayout(Layout layout)
    {
      switch (layout)
      {
        case MediaPortal.GUI.Library.GUIFacadeControl.Layout.SmallIcons: return 1;
        case MediaPortal.GUI.Library.GUIFacadeControl.Layout.LargeIcons: return 2;
        case MediaPortal.GUI.Library.GUIFacadeControl.Layout.AlbumView: return 3;
        case MediaPortal.GUI.Library.GUIFacadeControl.Layout.Filmstrip: return 4;
        case MediaPortal.GUI.Library.GUIFacadeControl.Layout.CoverFlow: return 5;
        default: return 0;
      }
    }

    private void mpButtonWOL_Click(object sender, EventArgs e)
    {
      DlgWol dlg = new DlgWol();
      DialogResult dialogResult = dlg.ShowDialog();
    }
  }
}