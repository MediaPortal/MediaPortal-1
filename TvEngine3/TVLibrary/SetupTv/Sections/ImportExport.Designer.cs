namespace SetupTv.Sections
{
    partial class ImportExport
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
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.exportTab = new System.Windows.Forms.TabPage();
      this.exCheckRadioGroups = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.exCheckTVGroups = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.exportButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.exCheckSchedules = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.exCheckRadioChannels = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.exCheckTVChannels = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.importTab = new System.Windows.Forms.TabPage();
      this.imCheckRadioGroups = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.imCheckTvGroups = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.importButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.imCheckSchedules = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.imCheckRadioChannels = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.imCheckTvChannels = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControl1.SuspendLayout();
      this.exportTab.SuspendLayout();
      this.importTab.SuspendLayout();
      this.SuspendLayout();
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.FileName = "openFileDialog1";
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.exportTab);
      this.tabControl1.Controls.Add(this.importTab);
      this.tabControl1.Location = new System.Drawing.Point(3, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(468, 406);
      this.tabControl1.TabIndex = 0;
      // 
      // exportTab
      // 
      this.exportTab.Controls.Add(this.exCheckRadioGroups);
      this.exportTab.Controls.Add(this.exCheckTVGroups);
      this.exportTab.Controls.Add(this.mpLabel3);
      this.exportTab.Controls.Add(this.exportButton);
      this.exportTab.Controls.Add(this.exCheckSchedules);
      this.exportTab.Controls.Add(this.exCheckRadioChannels);
      this.exportTab.Controls.Add(this.exCheckTVChannels);
      this.exportTab.Controls.Add(this.mpLabel1);
      this.exportTab.Location = new System.Drawing.Point(4, 22);
      this.exportTab.Name = "exportTab";
      this.exportTab.Padding = new System.Windows.Forms.Padding(3);
      this.exportTab.Size = new System.Drawing.Size(460, 380);
      this.exportTab.TabIndex = 0;
      this.exportTab.Text = "Export";
      this.exportTab.UseVisualStyleBackColor = true;
      // 
      // exCheckRadioGroups
      // 
      this.exCheckRadioGroups.AutoSize = true;
      this.exCheckRadioGroups.Checked = true;
      this.exCheckRadioGroups.CheckState = System.Windows.Forms.CheckState.Checked;
      this.exCheckRadioGroups.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.exCheckRadioGroups.Location = new System.Drawing.Point(10, 135);
      this.exCheckRadioGroups.Name = "exCheckRadioGroups";
      this.exCheckRadioGroups.Size = new System.Drawing.Size(87, 17);
      this.exCheckRadioGroups.TabIndex = 8;
      this.exCheckRadioGroups.Text = "Radio groups";
      this.exCheckRadioGroups.UseVisualStyleBackColor = true;
      // 
      // exCheckTVGroups
      // 
      this.exCheckTVGroups.AutoSize = true;
      this.exCheckTVGroups.Checked = true;
      this.exCheckTVGroups.CheckState = System.Windows.Forms.CheckState.Checked;
      this.exCheckTVGroups.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.exCheckTVGroups.Location = new System.Drawing.Point(10, 89);
      this.exCheckTVGroups.Name = "exCheckTVGroups";
      this.exCheckTVGroups.Size = new System.Drawing.Size(73, 17);
      this.exCheckTVGroups.TabIndex = 7;
      this.exCheckTVGroups.Text = "TV groups";
      this.exCheckTVGroups.UseVisualStyleBackColor = true;
      // 
      // mpLabel3
      // 
      this.mpLabel3.Location = new System.Drawing.Point(7, 36);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(304, 13);
      this.mpLabel3.TabIndex = 6;
      this.mpLabel3.Text = "Please chose, which items should be included in the export file.";
      // 
      // exportButton
      // 
      this.exportButton.Location = new System.Drawing.Point(10, 181);
      this.exportButton.Name = "exportButton";
      this.exportButton.Size = new System.Drawing.Size(75, 23);
      this.exportButton.TabIndex = 4;
      this.exportButton.Text = "Export now";
      this.exportButton.UseVisualStyleBackColor = true;
      this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
      // 
      // exCheckSchedules
      // 
      this.exCheckSchedules.AutoSize = true;
      this.exCheckSchedules.Checked = true;
      this.exCheckSchedules.CheckState = System.Windows.Forms.CheckState.Checked;
      this.exCheckSchedules.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.exCheckSchedules.Location = new System.Drawing.Point(10, 158);
      this.exCheckSchedules.Name = "exCheckSchedules";
      this.exCheckSchedules.Size = new System.Drawing.Size(74, 17);
      this.exCheckSchedules.TabIndex = 3;
      this.exCheckSchedules.Text = "Schedules";
      this.exCheckSchedules.UseVisualStyleBackColor = true;
      // 
      // exCheckRadioChannels
      // 
      this.exCheckRadioChannels.AutoSize = true;
      this.exCheckRadioChannels.Checked = true;
      this.exCheckRadioChannels.CheckState = System.Windows.Forms.CheckState.Checked;
      this.exCheckRadioChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.exCheckRadioChannels.Location = new System.Drawing.Point(10, 112);
      this.exCheckRadioChannels.Name = "exCheckRadioChannels";
      this.exCheckRadioChannels.Size = new System.Drawing.Size(91, 17);
      this.exCheckRadioChannels.TabIndex = 2;
      this.exCheckRadioChannels.Text = "Radio stations";
      this.exCheckRadioChannels.UseVisualStyleBackColor = true;
      // 
      // exCheckTVChannels
      // 
      this.exCheckTVChannels.AutoSize = true;
      this.exCheckTVChannels.Checked = true;
      this.exCheckTVChannels.CheckState = System.Windows.Forms.CheckState.Checked;
      this.exCheckTVChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.exCheckTVChannels.Location = new System.Drawing.Point(10, 66);
      this.exCheckTVChannels.Name = "exCheckTVChannels";
      this.exCheckTVChannels.Size = new System.Drawing.Size(84, 17);
      this.exCheckTVChannels.TabIndex = 1;
      this.exCheckTVChannels.Text = "TV channels";
      this.exCheckTVChannels.UseVisualStyleBackColor = true;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(7, 7);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(433, 18);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "MediaPortal TV Server can export your channels, groups and schedules to a xml fil" +
          "e.";
      // 
      // importTab
      // 
      this.importTab.Controls.Add(this.imCheckRadioGroups);
      this.importTab.Controls.Add(this.imCheckTvGroups);
      this.importTab.Controls.Add(this.mpLabel2);
      this.importTab.Controls.Add(this.importButton);
      this.importTab.Controls.Add(this.imCheckSchedules);
      this.importTab.Controls.Add(this.imCheckRadioChannels);
      this.importTab.Controls.Add(this.imCheckTvChannels);
      this.importTab.Controls.Add(this.mpLabel4);
      this.importTab.Location = new System.Drawing.Point(4, 22);
      this.importTab.Name = "importTab";
      this.importTab.Padding = new System.Windows.Forms.Padding(3);
      this.importTab.Size = new System.Drawing.Size(460, 380);
      this.importTab.TabIndex = 1;
      this.importTab.Text = "Import";
      this.importTab.UseVisualStyleBackColor = true;
      // 
      // imCheckRadioGroups
      // 
      this.imCheckRadioGroups.AutoSize = true;
      this.imCheckRadioGroups.Checked = true;
      this.imCheckRadioGroups.CheckState = System.Windows.Forms.CheckState.Checked;
      this.imCheckRadioGroups.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.imCheckRadioGroups.Location = new System.Drawing.Point(9, 140);
      this.imCheckRadioGroups.Name = "imCheckRadioGroups";
      this.imCheckRadioGroups.Size = new System.Drawing.Size(87, 17);
      this.imCheckRadioGroups.TabIndex = 14;
      this.imCheckRadioGroups.Text = "Radio groups";
      this.imCheckRadioGroups.UseVisualStyleBackColor = true;
      // 
      // imCheckTvGroups
      // 
      this.imCheckTvGroups.AutoSize = true;
      this.imCheckTvGroups.Checked = true;
      this.imCheckTvGroups.CheckState = System.Windows.Forms.CheckState.Checked;
      this.imCheckTvGroups.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.imCheckTvGroups.Location = new System.Drawing.Point(9, 94);
      this.imCheckTvGroups.Name = "imCheckTvGroups";
      this.imCheckTvGroups.Size = new System.Drawing.Size(73, 17);
      this.imCheckTvGroups.TabIndex = 13;
      this.imCheckTvGroups.Text = "TV groups";
      this.imCheckTvGroups.UseVisualStyleBackColor = true;
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(6, 41);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(331, 13);
      this.mpLabel2.TabIndex = 12;
      this.mpLabel2.Text = "Please chose, which items should be included during the import.";
      // 
      // importButton
      // 
      this.importButton.Location = new System.Drawing.Point(9, 186);
      this.importButton.Name = "importButton";
      this.importButton.Size = new System.Drawing.Size(75, 23);
      this.importButton.TabIndex = 11;
      this.importButton.Text = "Import now";
      this.importButton.UseVisualStyleBackColor = true;
      this.importButton.Click += new System.EventHandler(this.importButton_Click);
      // 
      // imCheckSchedules
      // 
      this.imCheckSchedules.AutoSize = true;
      this.imCheckSchedules.Checked = true;
      this.imCheckSchedules.CheckState = System.Windows.Forms.CheckState.Checked;
      this.imCheckSchedules.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.imCheckSchedules.Location = new System.Drawing.Point(9, 163);
      this.imCheckSchedules.Name = "imCheckSchedules";
      this.imCheckSchedules.Size = new System.Drawing.Size(74, 17);
      this.imCheckSchedules.TabIndex = 10;
      this.imCheckSchedules.Text = "Schedules";
      this.imCheckSchedules.UseVisualStyleBackColor = true;
      // 
      // imCheckRadioChannels
      // 
      this.imCheckRadioChannels.AutoSize = true;
      this.imCheckRadioChannels.Checked = true;
      this.imCheckRadioChannels.CheckState = System.Windows.Forms.CheckState.Checked;
      this.imCheckRadioChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.imCheckRadioChannels.Location = new System.Drawing.Point(9, 117);
      this.imCheckRadioChannels.Name = "imCheckRadioChannels";
      this.imCheckRadioChannels.Size = new System.Drawing.Size(91, 17);
      this.imCheckRadioChannels.TabIndex = 9;
      this.imCheckRadioChannels.Text = "Radio stations";
      this.imCheckRadioChannels.UseVisualStyleBackColor = true;
      // 
      // imCheckTvChannels
      // 
      this.imCheckTvChannels.AutoSize = true;
      this.imCheckTvChannels.Checked = true;
      this.imCheckTvChannels.CheckState = System.Windows.Forms.CheckState.Checked;
      this.imCheckTvChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.imCheckTvChannels.Location = new System.Drawing.Point(9, 71);
      this.imCheckTvChannels.Name = "imCheckTvChannels";
      this.imCheckTvChannels.Size = new System.Drawing.Size(84, 17);
      this.imCheckTvChannels.TabIndex = 8;
      this.imCheckTvChannels.Text = "TV channels";
      this.imCheckTvChannels.UseVisualStyleBackColor = true;
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(6, 12);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(414, 13);
      this.mpLabel4.TabIndex = 7;
      this.mpLabel4.Text = "MediaPortal TV Server can import your channels, groups and schedules from a xml f" +
          "ile.";
      // 
      // ImportExport
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "ImportExport";
      this.Size = new System.Drawing.Size(474, 412);
      this.tabControl1.ResumeLayout(false);
      this.exportTab.ResumeLayout(false);
      this.exportTab.PerformLayout();
      this.importTab.ResumeLayout(false);
      this.importTab.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage exportTab;
    private System.Windows.Forms.TabPage importTab;
    private MediaPortal.UserInterface.Controls.MPButton exportButton;
    private MediaPortal.UserInterface.Controls.MPCheckBox exCheckSchedules;
    private MediaPortal.UserInterface.Controls.MPCheckBox exCheckRadioChannels;
    private MediaPortal.UserInterface.Controls.MPCheckBox exCheckTVChannels;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPButton importButton;
    private MediaPortal.UserInterface.Controls.MPCheckBox imCheckSchedules;
    private MediaPortal.UserInterface.Controls.MPCheckBox imCheckRadioChannels;
    private MediaPortal.UserInterface.Controls.MPCheckBox imCheckTvChannels;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private MediaPortal.UserInterface.Controls.MPCheckBox exCheckRadioGroups;
    private MediaPortal.UserInterface.Controls.MPCheckBox exCheckTVGroups;
    private MediaPortal.UserInterface.Controls.MPCheckBox imCheckRadioGroups;
    private MediaPortal.UserInterface.Controls.MPCheckBox imCheckTvGroups;
  }
}
