namespace MediaPortal.Configuration.Sections
{
  partial class Gui
  {
    /// <summary> 
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Vom Komponenten-Designer generierter Code

    /// <summary> 
    /// Erforderliche Methode für die Designerunterstützung. 
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBoxSkin = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpButtonEditSkinSettings = new System.Windows.Forms.Button();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.panelFitImage = new System.Windows.Forms.Panel();
      this.previewPictureBox = new System.Windows.Forms.PictureBox();
      this.listViewAvailableSkins = new System.Windows.Forms.ListView();
      this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.colVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.groupBoxGuiSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.homeComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.settingsCheckedListBox = new System.Windows.Forms.CheckedListBox();
      this.groupBoxSkin.SuspendLayout();
      this.panelFitImage.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).BeginInit();
      this.groupBoxGuiSettings.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxSkin
      // 
      this.groupBoxSkin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSkin.Controls.Add(this.mpButtonEditSkinSettings);
      this.groupBoxSkin.Controls.Add(this.linkLabel1);
      this.groupBoxSkin.Controls.Add(this.panelFitImage);
      this.groupBoxSkin.Controls.Add(this.listViewAvailableSkins);
      this.groupBoxSkin.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxSkin.Location = new System.Drawing.Point(6, 0);
      this.groupBoxSkin.Name = "groupBoxSkin";
      this.groupBoxSkin.Size = new System.Drawing.Size(462, 197);
      this.groupBoxSkin.TabIndex = 4;
      this.groupBoxSkin.TabStop = false;
      this.groupBoxSkin.Text = "Skin selection";
      // 
      // mpButtonEditSkinSettings
      //
      this.mpButtonEditSkinSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonEditSkinSettings.Location = new System.Drawing.Point(263, 159);
      this.mpButtonEditSkinSettings.Name = "mpButtonEditSkinSettings";
      this.mpButtonEditSkinSettings.Size = new System.Drawing.Size(150, 23);
      this.mpButtonEditSkinSettings.TabIndex = 11;
      this.mpButtonEditSkinSettings.Text = "Edit Skin Settings";
      this.mpButtonEditSkinSettings.UseVisualStyleBackColor = true;
      this.mpButtonEditSkinSettings.Click += new System.EventHandler(this.mpButtonEditSkinSettings_Click);
      // 
      // linkLabel1
      // 
      this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(16, 167);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(131, 13);
      this.linkLabel1.TabIndex = 10;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "more new and hot skins ...";
      this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // panelFitImage
      // 
      this.panelFitImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.panelFitImage.Controls.Add(this.previewPictureBox);
      this.panelFitImage.Location = new System.Drawing.Point(223, 22);
      this.panelFitImage.Name = "panelFitImage";
      this.panelFitImage.Size = new System.Drawing.Size(222, 123);
      this.panelFitImage.TabIndex = 5;
      // 
      // previewPictureBox
      // 
      this.previewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.previewPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.previewPictureBox.Image = global::MediaPortal.Configuration.Properties.Resources.mplogo;
      this.previewPictureBox.Location = new System.Drawing.Point(0, 0);
      this.previewPictureBox.Name = "previewPictureBox";
      this.previewPictureBox.Size = new System.Drawing.Size(222, 123);
      this.previewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.previewPictureBox.TabIndex = 5;
      this.previewPictureBox.TabStop = false;
      // 
      // listViewAvailableSkins
      // 
      this.listViewAvailableSkins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewAvailableSkins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colVersion});
      this.listViewAvailableSkins.FullRowSelect = true;
      this.listViewAvailableSkins.HideSelection = false;
      this.listViewAvailableSkins.Location = new System.Drawing.Point(15, 22);
      this.listViewAvailableSkins.MultiSelect = false;
      this.listViewAvailableSkins.Name = "listViewAvailableSkins";
      this.listViewAvailableSkins.Size = new System.Drawing.Size(200, 142);
      this.listViewAvailableSkins.TabIndex = 3;
      this.listViewAvailableSkins.UseCompatibleStateImageBehavior = false;
      this.listViewAvailableSkins.View = System.Windows.Forms.View.Details;
      this.listViewAvailableSkins.SelectedIndexChanged += new System.EventHandler(this.listViewAvailableSkins_SelectedIndexChanged);
      // 
      // colName
      // 
      this.colName.Text = "Name";
      this.colName.Width = 140;
      // 
      // colVersion
      // 
      this.colVersion.Text = "Version";
      this.colVersion.Width = 56;
      // 
      // groupBoxGuiSettings
      // 
      this.groupBoxGuiSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGuiSettings.Controls.Add(this.homeComboBox);
      this.groupBoxGuiSettings.Controls.Add(this.mpLabel1);
      this.groupBoxGuiSettings.Controls.Add(this.settingsCheckedListBox);
      this.groupBoxGuiSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGuiSettings.Location = new System.Drawing.Point(6, 203);
      this.groupBoxGuiSettings.Name = "groupBoxGuiSettings";
      this.groupBoxGuiSettings.Size = new System.Drawing.Size(462, 190);
      this.groupBoxGuiSettings.TabIndex = 1;
      this.groupBoxGuiSettings.TabStop = false;
      this.groupBoxGuiSettings.Text = "GUI settings";
      // 
      // homeComboBox
      // 
      this.homeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.homeComboBox.BorderColor = System.Drawing.Color.Empty;
      this.homeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.homeComboBox.Items.AddRange(new object[] {
            "Classic and Basic, prefer Classic",
            "Classic and Basic, prefer Basic",
            "only Classic Home",
            "only Basic Home"});
      this.homeComboBox.Location = new System.Drawing.Point(108, 142);
      this.homeComboBox.Name = "homeComboBox";
      this.homeComboBox.Size = new System.Drawing.Size(315, 21);
      this.homeComboBox.TabIndex = 11;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(6, 142);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(96, 16);
      this.mpLabel1.TabIndex = 10;
      this.mpLabel1.Text = "Home Screen:";
      // 
      // settingsCheckedListBox
      // 
      this.settingsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.settingsCheckedListBox.CheckOnClick = true;
      this.settingsCheckedListBox.Items.AddRange(new object[] {
            "Allow remember last focused item on supported window/skin",
            "Hide file extensions like .mp3, .avi, .mpg,...",
            "Enable file existence cache (improves performance on some systems)",
            "Enable skin sound effects",
            "Show special mouse controls (scrollbars, etc)",
            "Reduce frame rate when MediaPortal is not in focus",
            "Add media info to the video database for use in share view"});
      this.settingsCheckedListBox.Location = new System.Drawing.Point(6, 19);
      this.settingsCheckedListBox.Name = "settingsCheckedListBox";
      this.settingsCheckedListBox.Size = new System.Drawing.Size(450, 109);
      this.settingsCheckedListBox.TabIndex = 0;
      // 
      // Gui
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBoxSkin);
      this.Controls.Add(this.groupBoxGuiSettings);
      this.Name = "Gui";
      this.Size = new System.Drawing.Size(472, 402);
      this.groupBoxSkin.ResumeLayout(false);
      this.groupBoxSkin.PerformLayout();
      this.panelFitImage.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).EndInit();
      this.groupBoxGuiSettings.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxGuiSettings;
    private System.Windows.Forms.CheckedListBox settingsCheckedListBox;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxSkin;
    private System.Windows.Forms.LinkLabel linkLabel1;
    private System.Windows.Forms.Panel panelFitImage;
    private System.Windows.Forms.PictureBox previewPictureBox;
    private System.Windows.Forms.ListView listViewAvailableSkins;
    private System.Windows.Forms.ColumnHeader colName;
    private System.Windows.Forms.ColumnHeader colVersion;
    private MediaPortal.UserInterface.Controls.MPComboBox homeComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private System.Windows.Forms.Button mpButtonEditSkinSettings;
  }
}
