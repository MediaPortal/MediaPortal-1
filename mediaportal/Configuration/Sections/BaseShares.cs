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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class BaseShares : MediaPortal.Configuration.SectionSettings
  {
    public class ShareData
    {
      public enum Views
      {
        List,
        Icons,
        BigIcons,
        Filmstrip
      }
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
      public Views DefaultView = Views.List;

      public bool HasPinCode
      {
        get { return (PinCode.Length > 0); }
      }

      public ShareData(string name, string folder, string pinCode)
      {
        this.Name = name;
        this.Folder = folder;
        this.PinCode = pinCode;
      }
    }

    protected const int MaximumShares = 128;

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPButton deleteButton;
    private MediaPortal.UserInterface.Controls.MPButton editButton;
    private MediaPortal.UserInterface.Controls.MPButton addButton;
    private MediaPortal.UserInterface.Controls.MPListView sharesListView;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxRemember;
    private System.ComponentModel.IContainer components = null;

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
      this.checkBoxRemember = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.deleteButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.editButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.addButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.sharesListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.checkBoxRemember);
      this.groupBox1.Controls.Add(this.deleteButton);
      this.groupBox1.Controls.Add(this.editButton);
      this.groupBox1.Controls.Add(this.addButton);
      this.groupBox1.Controls.Add(this.sharesListView);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // checkBoxRemember
      // 
      this.checkBoxRemember.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.checkBoxRemember.AutoSize = true;
      this.checkBoxRemember.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxRemember.Location = new System.Drawing.Point(16, 378);
      this.checkBoxRemember.Name = "checkBoxRemember";
      this.checkBoxRemember.Size = new System.Drawing.Size(152, 17);
      this.checkBoxRemember.TabIndex = 1;
      this.checkBoxRemember.Text = "Remember last used folder";
      this.checkBoxRemember.UseVisualStyleBackColor = true;
      // 
      // deleteButton
      // 
      this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.deleteButton.Enabled = false;
      this.deleteButton.Location = new System.Drawing.Point(384, 376);
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
      this.editButton.Location = new System.Drawing.Point(304, 376);
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
      this.addButton.Location = new System.Drawing.Point(224, 376);
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
            this.columnHeader2});
      this.sharesListView.FullRowSelect = true;
      this.sharesListView.Location = new System.Drawing.Point(16, 24);
      this.sharesListView.Name = "sharesListView";
      this.sharesListView.Size = new System.Drawing.Size(440, 344);
      this.sharesListView.TabIndex = 0;
      this.sharesListView.UseCompatibleStateImageBehavior = false;
      this.sharesListView.View = System.Windows.Forms.View.Details;
      this.sharesListView.SelectedIndexChanged += new System.EventHandler(this.sharesListView_SelectedIndexChanged);
      this.sharesListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.sharesListView_ItemCheck);
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
      this.columnHeader2.Width = 273;
      // 
      // Shares
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "Shares";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    ListViewItem currentlyCheckedItem = null;

    private void addButton_Click(object sender, System.EventArgs e)
    {
      EditShareForm editShare = new EditShareForm();

      DialogResult dialogResult = editShare.ShowDialog(this);

      if (dialogResult == DialogResult.OK)
      {
        ShareData shareData = new ShareData(editShare.ShareName, editShare.Folder, editShare.PinCode);
        shareData.IsRemote = editShare.IsRemote;
        shareData.Server = editShare.Server;
        shareData.LoginName = editShare.LoginName;
        shareData.PassWord = editShare.PassWord;
        shareData.Port = editShare.Port;
        shareData.ActiveConnection = editShare.ActiveConnection;
        shareData.RemoteFolder = editShare.RemoteFolder;
        shareData.DefaultView = (ShareData.Views)editShare.View;

        AddShare(shareData, currentlyCheckedItem == null);
      }
    }

    protected void AddShare(ShareData shareData, bool check)
    {
      ListViewItem listItem = new ListViewItem(new string[] { shareData.Name, shareData.HasPinCode ? "Yes" : "No", shareData.Folder, shareData.ActiveConnection ? "Yes" : "No" });

      if (shareData.IsRemote)
      {
        listItem.SubItems[2].Text = String.Format("ftp://{0}:{1}{2}", shareData.Server, shareData.Port, shareData.RemoteFolder);
      }
      listItem.Tag = shareData;
      listItem.Checked = check;
      if (check) currentlyCheckedItem = listItem;

      sharesListView.Items.Add(listItem);
    }

    private void editButton_Click(object sender, System.EventArgs e)
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
          editShare.View = (int)shareData.DefaultView;

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
            shareData.DefaultView = (ShareData.Views)editShare.View;
            
            selectedItem.Tag = shareData;

            selectedItem.SubItems[0].Text = shareData.Name;
            selectedItem.SubItems[1].Text = shareData.HasPinCode ? "Yes" : "No";
            selectedItem.SubItems[2].Text = shareData.Folder;
            if (shareData.IsRemote) selectedItem.SubItems[2].Text = String.Format("ftp://{0}:{1}{2}", shareData.Server, shareData.Port, shareData.RemoteFolder);

          }
        }
      }
    }

    private void deleteButton_Click(object sender, System.EventArgs e)
    {
      int selectedItems = sharesListView.SelectedIndices.Count;

      for (int index = 0; index < selectedItems; index++)
      {
        sharesListView.Items.RemoveAt(sharesListView.SelectedIndices[0]);
      }
    }

    private void sharesListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
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
          if (currentlyCheckedItem != null)
            currentlyCheckedItem.Checked = false;
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
      get
      {
        return sharesListView.Items;
      }
    }

    public ListViewItem DefaultShare
    {
      get
      {
        return currentlyCheckedItem;
      }
    }

    public bool RememberLastFolder
    {
      get
      {
        return checkBoxRemember.Checked;
      }
      set
      {
        checkBoxRemember.Checked = value;
      }
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
            string driveLetter = drive.Substring(0, 1).ToUpper();
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
                name = String.Format("({0}:) Removable", drive.Substring(0, 1).ToUpper());
                break;
              case DriveType.Fixed:
                name = String.Format("({0}:) Fixed", drive.Substring(0, 1).ToUpper());
                break;
              case DriveType.RemoteDisk:
                name = String.Format("({0}:) Remote", drive.Substring(0, 1).ToUpper());
                break;
              case DriveType.DVD: // or cd
                name = String.Format("({0}:) CD/DVD", drive.Substring(0, 1).ToUpper());
                break;
              case DriveType.RamDisk:
                name = String.Format("({0}:) Ram", drive.Substring(0, 1).ToUpper());
                break;
            }
            AddShare(new ShareData(name, drive, string.Empty), false);
          }
        }
      }
    }

    private void sharesListView_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      editButton.Enabled = deleteButton.Enabled = (sharesListView.SelectedItems.Count > 0);
    }

    public override object GetSetting(string name)
    {
      switch (name.ToLower())
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
      }

      return null;
    }

    protected void LoadSettings(string section, string defaultSharePath)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string defaultShare = xmlreader.GetValueAsString(section, "default", "");
        RememberLastFolder = xmlreader.GetValueAsBool(section, "rememberlastfolder", false);

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

          string shareNameData = xmlreader.GetValueAsString(section, shareName, "");
          string sharePathData = xmlreader.GetValueAsString(section, sharePath, "");
          string sharePinData = MediaPortal.Util.Utils.DecryptPin(xmlreader.GetValueAsString(section, sharePin, ""));

          // provide default shares
          if (index == 0 && shareNameData == string.Empty)
          {
            shareNameData = Util.VirtualDirectory.GetShareNameDefault(defaultSharePath);
            sharePathData = defaultSharePath;
            sharePinData = string.Empty;

            AddStaticShares(DriveType.DVD, "DVD");
          }

          bool shareTypeData = xmlreader.GetValueAsBool(section, shareType, false);
          string shareServerData = xmlreader.GetValueAsString(section, shareServer, "");
          string shareLoginData = xmlreader.GetValueAsString(section, shareLogin, "");
          string sharePwdData = xmlreader.GetValueAsString(section, sharePwd, "");
          int sharePortData = xmlreader.GetValueAsInt(section, sharePort, 21);
          string shareRemotePathData = xmlreader.GetValueAsString(section, shareRemotePath, "/");
          int shareView = xmlreader.GetValueAsInt(section, shareViewPath, (int)ShareData.Views.List);

          if (shareNameData != null && shareNameData.Length > 0)
          {
            ShareData newShare = new ShareData(shareNameData, sharePathData, sharePinData);
            newShare.IsRemote = shareTypeData;
            newShare.Server = shareServerData;
            newShare.LoginName = shareLoginData;
            newShare.PassWord = sharePwdData;
            newShare.Port = sharePortData;
            newShare.RemoteFolder = shareRemotePathData;
            newShare.DefaultView = (ShareData.Views)shareView;

            AddShare(newShare, shareNameData.Equals(defaultShare));
          }
        }
      }
    }

    protected void SaveSettings(string section)
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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

          string shareNameData = string.Empty;
          string sharePathData = string.Empty;
          string sharePinData = string.Empty;

          bool shareTypeData = false;
          string shareServerData = string.Empty;
          string shareLoginData = string.Empty;
          string sharePwdData = string.Empty;
          int sharePortData = 21;
          string shareRemotePathData = string.Empty;
          int shareView = (int)ShareData.Views.List;

          if (CurrentShares != null && CurrentShares.Count > index)
          {
            ShareData shareData = CurrentShares[index].Tag as ShareData;

            if (shareData != null)
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
              shareView = (int)shareData.DefaultView;

              if (CurrentShares[index] == DefaultShare)
                defaultShare = shareNameData;
            }
          }
          xmlwriter.SetValue(section, shareName, shareNameData);
          xmlwriter.SetValue(section, sharePath, sharePathData);
          xmlwriter.SetValue(section, sharePin, MediaPortal.Util.Utils.EncryptPin(sharePinData));

          xmlwriter.SetValueAsBool(section, shareType, shareTypeData);
          xmlwriter.SetValue(section, shareServer, shareServerData);
          xmlwriter.SetValue(section, shareLogin, shareLoginData);
          xmlwriter.SetValue(section, sharePwd, sharePwdData);
          xmlwriter.SetValue(section, sharePort, sharePortData.ToString());
          xmlwriter.SetValue(section, shareRemotePath, shareRemotePathData);
          xmlwriter.SetValue(section, shareViewPath, shareView);
        }
        xmlwriter.SetValue(section, "default", defaultShare);
        xmlwriter.SetValueAsBool(section, "rememberlastfolder", RememberLastFolder);
      }
    }
  }
}
