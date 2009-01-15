namespace ArtistThumbGenerator
{
  partial class formMain
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.listBoxFolders = new System.Windows.Forms.ListBox();
      this.buttonClose = new System.Windows.Forms.Button();
      this.buttonScan = new System.Windows.Forms.Button();
      this.textBoxThumbPath = new System.Windows.Forms.TextBox();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.buttonLookupDir = new System.Windows.Forms.Button();
      this.buttonLookupArtistRoot = new System.Windows.Forms.Button();
      this.textBoxArtistRootFolder = new System.Windows.Forms.TextBox();
      this.folderBrowserDialogArtist = new System.Windows.Forms.FolderBrowserDialog();
      this.lblThumbFolder = new System.Windows.Forms.Label();
      this.lblArtistRoot = new System.Windows.Forms.Label();
      this.lblClickHint = new System.Windows.Forms.Label();
      this.lblArtistThumb = new System.Windows.Forms.Label();
      this.buttonLookupArtistDir = new System.Windows.Forms.Button();
      this.textBoxArtistThumbFolder = new System.Windows.Forms.TextBox();
      this.buttonSave = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // listBoxFolders
      // 
      this.listBoxFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listBoxFolders.FormattingEnabled = true;
      this.listBoxFolders.Location = new System.Drawing.Point(12, 142);
      this.listBoxFolders.Name = "listBoxFolders";
      this.listBoxFolders.Size = new System.Drawing.Size(743, 277);
      this.listBoxFolders.TabIndex = 0;
      this.listBoxFolders.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxFolders_MouseDoubleClick);
      // 
      // buttonClose
      // 
      this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonClose.Location = new System.Drawing.Point(680, 427);
      this.buttonClose.Name = "buttonClose";
      this.buttonClose.Size = new System.Drawing.Size(75, 23);
      this.buttonClose.TabIndex = 1;
      this.buttonClose.Text = "&Close";
      this.buttonClose.UseVisualStyleBackColor = true;
      this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
      // 
      // buttonScan
      // 
      this.buttonScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonScan.Location = new System.Drawing.Point(518, 427);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(75, 23);
      this.buttonScan.TabIndex = 2;
      this.buttonScan.Text = "&Scan";
      this.buttonScan.UseVisualStyleBackColor = true;
      this.buttonScan.Click += new System.EventHandler(this.buttonScan_Click);
      // 
      // textBoxThumbPath
      // 
      this.textBoxThumbPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxThumbPath.Enabled = false;
      this.textBoxThumbPath.Location = new System.Drawing.Point(12, 67);
      this.textBoxThumbPath.Name = "textBoxThumbPath";
      this.textBoxThumbPath.Size = new System.Drawing.Size(710, 20);
      this.textBoxThumbPath.TabIndex = 3;
      // 
      // folderBrowserDialog
      // 
      this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.CommonApplicationData;
      this.folderBrowserDialog.ShowNewFolderButton = false;
      // 
      // buttonLookupDir
      // 
      this.buttonLookupDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonLookupDir.Enabled = false;
      this.buttonLookupDir.Location = new System.Drawing.Point(728, 67);
      this.buttonLookupDir.Name = "buttonLookupDir";
      this.buttonLookupDir.Size = new System.Drawing.Size(27, 20);
      this.buttonLookupDir.TabIndex = 4;
      this.buttonLookupDir.Text = "...";
      this.buttonLookupDir.UseVisualStyleBackColor = true;
      this.buttonLookupDir.Click += new System.EventHandler(this.buttonLookupDir_Click);
      // 
      // buttonLookupArtistRoot
      // 
      this.buttonLookupArtistRoot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonLookupArtistRoot.Location = new System.Drawing.Point(728, 109);
      this.buttonLookupArtistRoot.Name = "buttonLookupArtistRoot";
      this.buttonLookupArtistRoot.Size = new System.Drawing.Size(27, 20);
      this.buttonLookupArtistRoot.TabIndex = 6;
      this.buttonLookupArtistRoot.Text = "...";
      this.buttonLookupArtistRoot.UseVisualStyleBackColor = true;
      this.buttonLookupArtistRoot.Click += new System.EventHandler(this.buttonLookupArtistRoot_Click);
      // 
      // textBoxArtistRootFolder
      // 
      this.textBoxArtistRootFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxArtistRootFolder.Location = new System.Drawing.Point(12, 109);
      this.textBoxArtistRootFolder.Name = "textBoxArtistRootFolder";
      this.textBoxArtistRootFolder.Size = new System.Drawing.Size(710, 20);
      this.textBoxArtistRootFolder.TabIndex = 5;
      // 
      // folderBrowserDialogArtist
      // 
      this.folderBrowserDialogArtist.RootFolder = System.Environment.SpecialFolder.MyComputer;
      this.folderBrowserDialogArtist.ShowNewFolderButton = false;
      // 
      // lblThumbFolder
      // 
      this.lblThumbFolder.AutoSize = true;
      this.lblThumbFolder.Enabled = false;
      this.lblThumbFolder.Location = new System.Drawing.Point(12, 51);
      this.lblThumbFolder.Name = "lblThumbFolder";
      this.lblThumbFolder.Size = new System.Drawing.Size(242, 13);
      this.lblThumbFolder.TabIndex = 7;
      this.lblThumbFolder.Text = "MediaPortal\'s \"..\\Thumbs\\Music\\Folder\" directory";
      // 
      // lblArtistRoot
      // 
      this.lblArtistRoot.AutoSize = true;
      this.lblArtistRoot.Location = new System.Drawing.Point(12, 93);
      this.lblArtistRoot.Name = "lblArtistRoot";
      this.lblArtistRoot.Size = new System.Drawing.Size(264, 13);
      this.lblArtistRoot.TabIndex = 8;
      this.lblArtistRoot.Text = "Root folder where all \"Artist\" subdirectories are located";
      // 
      // lblClickHint
      // 
      this.lblClickHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblClickHint.AutoSize = true;
      this.lblClickHint.Location = new System.Drawing.Point(12, 432);
      this.lblClickHint.Name = "lblClickHint";
      this.lblClickHint.Size = new System.Drawing.Size(265, 13);
      this.lblClickHint.TabIndex = 9;
      this.lblClickHint.Text = "Doubleclick to copy the selected filename to clipboard)";
      this.lblClickHint.Visible = false;
      // 
      // lblArtistThumb
      // 
      this.lblArtistThumb.AutoSize = true;
      this.lblArtistThumb.Location = new System.Drawing.Point(12, 12);
      this.lblArtistThumb.Name = "lblArtistThumb";
      this.lblArtistThumb.Size = new System.Drawing.Size(241, 13);
      this.lblArtistThumb.TabIndex = 12;
      this.lblArtistThumb.Text = "MediaPortal\'s \"..\\Thumbs\\Music\\Artists\" directory";
      // 
      // buttonLookupArtistDir
      // 
      this.buttonLookupArtistDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonLookupArtistDir.Location = new System.Drawing.Point(728, 28);
      this.buttonLookupArtistDir.Name = "buttonLookupArtistDir";
      this.buttonLookupArtistDir.Size = new System.Drawing.Size(27, 20);
      this.buttonLookupArtistDir.TabIndex = 11;
      this.buttonLookupArtistDir.Text = "...";
      this.buttonLookupArtistDir.UseVisualStyleBackColor = true;
      this.buttonLookupArtistDir.Click += new System.EventHandler(this.buttonLookupArtistDir_Click);
      // 
      // textBoxArtistThumbFolder
      // 
      this.textBoxArtistThumbFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxArtistThumbFolder.Location = new System.Drawing.Point(12, 28);
      this.textBoxArtistThumbFolder.Name = "textBoxArtistThumbFolder";
      this.textBoxArtistThumbFolder.Size = new System.Drawing.Size(710, 20);
      this.textBoxArtistThumbFolder.TabIndex = 10;
      // 
      // buttonSave
      // 
      this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonSave.Location = new System.Drawing.Point(599, 427);
      this.buttonSave.Name = "buttonSave";
      this.buttonSave.Size = new System.Drawing.Size(75, 23);
      this.buttonSave.TabIndex = 13;
      this.buttonSave.Text = "Sa&ve";
      this.buttonSave.UseVisualStyleBackColor = true;
      this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
      // 
      // formMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonClose;
      this.ClientSize = new System.Drawing.Size(767, 462);
      this.Controls.Add(this.buttonSave);
      this.Controls.Add(this.lblArtistThumb);
      this.Controls.Add(this.buttonLookupArtistDir);
      this.Controls.Add(this.textBoxArtistThumbFolder);
      this.Controls.Add(this.lblClickHint);
      this.Controls.Add(this.lblArtistRoot);
      this.Controls.Add(this.lblThumbFolder);
      this.Controls.Add(this.buttonLookupArtistRoot);
      this.Controls.Add(this.textBoxArtistRootFolder);
      this.Controls.Add(this.buttonLookupDir);
      this.Controls.Add(this.textBoxThumbPath);
      this.Controls.Add(this.buttonScan);
      this.Controls.Add(this.buttonClose);
      this.Controls.Add(this.listBoxFolders);
      this.Name = "formMain";
      this.Text = "Artist Thumb Creator";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ListBox listBoxFolders;
    private System.Windows.Forms.Button buttonClose;
    private System.Windows.Forms.Button buttonScan;
    private System.Windows.Forms.TextBox textBoxThumbPath;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private System.Windows.Forms.Button buttonLookupDir;
    private System.Windows.Forms.Button buttonLookupArtistRoot;
    private System.Windows.Forms.TextBox textBoxArtistRootFolder;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogArtist;
    private System.Windows.Forms.Label lblThumbFolder;
    private System.Windows.Forms.Label lblArtistRoot;
    private System.Windows.Forms.Label lblClickHint;
    private System.Windows.Forms.Label lblArtistThumb;
    private System.Windows.Forms.Button buttonLookupArtistDir;
    private System.Windows.Forms.TextBox textBoxArtistThumbFolder;
    private System.Windows.Forms.Button buttonSave;
  }
}

