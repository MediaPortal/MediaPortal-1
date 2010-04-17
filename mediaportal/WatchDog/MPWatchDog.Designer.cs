namespace WatchDog
{
  partial class MPWatchDog
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
      this.components = new System.ComponentModel.Container();
      this.settingsGroup = new System.Windows.Forms.GroupBox();
      this.btnZipFile = new System.Windows.Forms.Button();
      this.tbZipFile = new System.Windows.Forms.TextBox();
      this.logDirLabel = new System.Windows.Forms.Label();
      this.step2Group = new System.Windows.Forms.GroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.step2Label4 = new System.Windows.Forms.Label();
      this.step2Label3 = new System.Windows.Forms.Label();
      this.step2Label2 = new System.Windows.Forms.Label();
      this.step2Label1 = new System.Windows.Forms.Label();
      this.postTestButton = new System.Windows.Forms.Button();
      this.step1Group = new System.Windows.Forms.GroupBox();
      this.step1Label2 = new System.Windows.Forms.Label();
      this.step1Label1 = new System.Windows.Forms.Label();
      this.preTestButton = new System.Windows.Forms.Button();
      this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
      this.menuItem1 = new System.Windows.Forms.MenuItem();
      this.menuItem2 = new System.Windows.Forms.MenuItem();
      this.menuItem3 = new System.Windows.Forms.MenuItem();
      this.menuItem4 = new System.Windows.Forms.MenuItem();
      this.menuItem8 = new System.Windows.Forms.MenuItem();
      this.menuItem5 = new System.Windows.Forms.MenuItem();
      this.menuItem6 = new System.Windows.Forms.MenuItem();
      this.menuItem7 = new System.Windows.Forms.MenuItem();
      this.statusBar = new System.Windows.Forms.StatusBar();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label4 = new System.Windows.Forms.Label();
      this.LaunchMPButton = new System.Windows.Forms.Button();
      this.tmrUnAttended = new System.Windows.Forms.Timer(this.components);
      this.tmrMPWatcher = new System.Windows.Forms.Timer(this.components);
      this.tmrWatchdog = new System.Windows.Forms.Timer(this.components);
      this.label3 = new System.Windows.Forms.Label();
      this.settingsGroup.SuspendLayout();
      this.step2Group.SuspendLayout();
      this.step1Group.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // settingsGroup
      // 
      this.settingsGroup.Controls.Add(this.btnZipFile);
      this.settingsGroup.Controls.Add(this.tbZipFile);
      this.settingsGroup.Controls.Add(this.logDirLabel);
      this.settingsGroup.Location = new System.Drawing.Point(12, 12);
      this.settingsGroup.Name = "settingsGroup";
      this.settingsGroup.Size = new System.Drawing.Size(408, 86);
      this.settingsGroup.TabIndex = 2;
      this.settingsGroup.TabStop = false;
      this.settingsGroup.Text = "Settings";
      // 
      // btnZipFile
      // 
      this.btnZipFile.Location = new System.Drawing.Point(334, 35);
      this.btnZipFile.Name = "btnZipFile";
      this.btnZipFile.Size = new System.Drawing.Size(64, 23);
      this.btnZipFile.TabIndex = 3;
      this.btnZipFile.Text = "Browse";
      this.btnZipFile.Click += new System.EventHandler(this.btnZipFile_Click);
      // 
      // tbZipFile
      // 
      this.tbZipFile.Location = new System.Drawing.Point(6, 37);
      this.tbZipFile.Name = "tbZipFile";
      this.tbZipFile.Size = new System.Drawing.Size(320, 20);
      this.tbZipFile.TabIndex = 2;
      // 
      // logDirLabel
      // 
      this.logDirLabel.Location = new System.Drawing.Point(6, 22);
      this.logDirLabel.Name = "logDirLabel";
      this.logDirLabel.Size = new System.Drawing.Size(152, 15);
      this.logDirLabel.TabIndex = 2;
      this.logDirLabel.Text = "Resulting ZIP of logs";
      // 
      // step2Group
      // 
      this.step2Group.Controls.Add(this.label2);
      this.step2Group.Controls.Add(this.label1);
      this.step2Group.Controls.Add(this.step2Label4);
      this.step2Group.Controls.Add(this.step2Label3);
      this.step2Group.Controls.Add(this.step2Label2);
      this.step2Group.Controls.Add(this.step2Label1);
      this.step2Group.Controls.Add(this.postTestButton);
      this.step2Group.Location = new System.Drawing.Point(12, 257);
      this.step2Group.Name = "step2Group";
      this.step2Group.Size = new System.Drawing.Size(408, 120);
      this.step2Group.TabIndex = 5;
      this.step2Group.TabStop = false;
      this.step2Group.Text = "Step 3 (after testing)";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(7, 82);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(296, 15);
      this.label2.TabIndex = 8;
      this.label2.Text = "- gather TvServer logfiles (if installed)";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(7, 67);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(296, 15);
      this.label1.TabIndex = 7;
      this.label1.Text = "- gather platform informations (OS version,...)";
      // 
      // step2Label4
      // 
      this.step2Label4.Location = new System.Drawing.Point(6, 97);
      this.step2Label4.Name = "step2Label4";
      this.step2Label4.Size = new System.Drawing.Size(296, 15);
      this.step2Label4.TabIndex = 4;
      this.step2Label4.Text = "- create ZIP file with all gathered information";
      // 
      // step2Label3
      // 
      this.step2Label3.Location = new System.Drawing.Point(8, 52);
      this.step2Label3.Name = "step2Label3";
      this.step2Label3.Size = new System.Drawing.Size(296, 15);
      this.step2Label3.TabIndex = 3;
      this.step2Label3.Text = "- gather system information (dxdiag / Windows hotfixes)";
      // 
      // step2Label2
      // 
      this.step2Label2.Location = new System.Drawing.Point(8, 37);
      this.step2Label2.Name = "step2Label2";
      this.step2Label2.Size = new System.Drawing.Size(296, 15);
      this.step2Label2.TabIndex = 2;
      this.step2Label2.Text = "- gather all events from System / Application logbooks";
      // 
      // step2Label1
      // 
      this.step2Label1.Location = new System.Drawing.Point(8, 22);
      this.step2Label1.Name = "step2Label1";
      this.step2Label1.Size = new System.Drawing.Size(296, 15);
      this.step2Label1.TabIndex = 1;
      this.step2Label1.Text = "- gather logfiles generated by MediaPortal";
      // 
      // postTestButton
      // 
      this.postTestButton.Location = new System.Drawing.Point(312, 29);
      this.postTestButton.Name = "postTestButton";
      this.postTestButton.Size = new System.Drawing.Size(88, 59);
      this.postTestButton.TabIndex = 6;
      this.postTestButton.Text = "Perform actions necessary after testing";
      this.postTestButton.Click += new System.EventHandler(this.postTestButton_Click);
      // 
      // step1Group
      // 
      this.step1Group.AccessibleDescription = "";
      this.step1Group.AccessibleName = "Step 1 (before testing)";
      this.step1Group.BackColor = System.Drawing.SystemColors.Control;
      this.step1Group.Controls.Add(this.step1Label2);
      this.step1Group.Controls.Add(this.step1Label1);
      this.step1Group.Controls.Add(this.preTestButton);
      this.step1Group.Location = new System.Drawing.Point(12, 104);
      this.step1Group.Name = "step1Group";
      this.step1Group.Size = new System.Drawing.Size(408, 82);
      this.step1Group.TabIndex = 4;
      this.step1Group.TabStop = false;
      this.step1Group.Text = "Step 1 (before testing)";
      // 
      // step1Label2
      // 
      this.step1Label2.Location = new System.Drawing.Point(8, 38);
      this.step1Label2.Name = "step1Label2";
      this.step1Label2.Size = new System.Drawing.Size(304, 15);
      this.step1Label2.TabIndex = 6;
      this.step1Label2.Text = "- clear all events in the System / Application event logbooks";
      // 
      // step1Label1
      // 
      this.step1Label1.Location = new System.Drawing.Point(8, 22);
      this.step1Label1.Name = "step1Label1";
      this.step1Label1.Size = new System.Drawing.Size(304, 15);
      this.step1Label1.TabIndex = 1;
      this.step1Label1.Text = "- remove all files in the MediaPortal installation log directory";
      // 
      // preTestButton
      // 
      this.preTestButton.Location = new System.Drawing.Point(312, 15);
      this.preTestButton.Name = "preTestButton";
      this.preTestButton.Size = new System.Drawing.Size(88, 59);
      this.preTestButton.TabIndex = 5;
      this.preTestButton.Text = "Perform actions necessary before testing";
      this.preTestButton.Click += new System.EventHandler(this.preTestButton_Click);
      // 
      // mainMenu
      // 
      this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem3,
            this.menuItem6});
      // 
      // menuItem1
      // 
      this.menuItem1.Index = 0;
      this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2});
      this.menuItem1.Text = "File";
      // 
      // menuItem2
      // 
      this.menuItem2.Index = 0;
      this.menuItem2.Text = "Exit";
      this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
      // 
      // menuItem3
      // 
      this.menuItem3.Index = 1;
      this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem4,
            this.menuItem8,
            this.menuItem5});
      this.menuItem3.Text = "Action";
      // 
      // menuItem4
      // 
      this.menuItem4.Index = 0;
      this.menuItem4.Text = "1. Perform pre-test actions";
      this.menuItem4.Click += new System.EventHandler(this.preTestButton_Click);
      // 
      // menuItem8
      // 
      this.menuItem8.Index = 1;
      this.menuItem8.Text = "2. Launch MediaPortal";
      // 
      // menuItem5
      // 
      this.menuItem5.Index = 2;
      this.menuItem5.Text = "3. Perform post-test actions";
      this.menuItem5.Click += new System.EventHandler(this.postTestButton_Click);
      // 
      // menuItem6
      // 
      this.menuItem6.Index = 2;
      this.menuItem6.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem7});
      this.menuItem6.Text = "Help";
      // 
      // menuItem7
      // 
      this.menuItem7.Index = 0;
      this.menuItem7.Text = "About";
      this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
      // 
      // statusBar
      // 
      this.statusBar.Location = new System.Drawing.Point(0, 391);
      this.statusBar.Name = "statusBar";
      this.statusBar.Size = new System.Drawing.Size(427, 20);
      this.statusBar.TabIndex = 6;
      this.statusBar.Text = "Status: Idle";
      // 
      // groupBox1
      // 
      this.groupBox1.AccessibleDescription = "";
      this.groupBox1.AccessibleName = "Step 1 (before testing)";
      this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.LaunchMPButton);
      this.groupBox1.Location = new System.Drawing.Point(12, 192);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(408, 59);
      this.groupBox1.TabIndex = 7;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Step 2 (do tests)";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(8, 22);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(304, 15);
      this.label4.TabIndex = 1;
      this.label4.Text = "- start MediaPortal";
      // 
      // LaunchMPButton
      // 
      this.LaunchMPButton.Location = new System.Drawing.Point(312, 15);
      this.LaunchMPButton.Name = "LaunchMPButton";
      this.LaunchMPButton.Size = new System.Drawing.Size(88, 22);
      this.LaunchMPButton.TabIndex = 5;
      this.LaunchMPButton.Text = "Launch MP";
      this.LaunchMPButton.Click += new System.EventHandler(this.LaunchMPButton_Click);
      // 
      // tmrUnAttended
      // 
      this.tmrUnAttended.Interval = 1000;
      this.tmrUnAttended.Tick += new System.EventHandler(this.tmrUnAttended_Tick);
      // 
      // tmrMPWatcher
      // 
      this.tmrMPWatcher.Interval = 5000;
      this.tmrMPWatcher.Tick += new System.EventHandler(this.tmrMPWatcher_Tick);
      // 
      // tmrWatchdog
      // 
      this.tmrWatchdog.Interval = 1000;
      this.tmrWatchdog.Tick += new System.EventHandler(this.tmrWatchdog_Tick);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(8, 37);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(217, 13);
      this.label3.TabIndex = 6;
      this.label3.Text = "- execute Step 3 when MediaPortal is closed";
      // 
      // MPWatchDog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(427, 411);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.statusBar);
      this.Controls.Add(this.step2Group);
      this.Controls.Add(this.step1Group);
      this.Controls.Add(this.settingsGroup);
      this.Menu = this.mainMenu;
      this.Name = "MPWatchDog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MediaPortal Watchdog";
      this.settingsGroup.ResumeLayout(false);
      this.settingsGroup.PerformLayout();
      this.step2Group.ResumeLayout(false);
      this.step1Group.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox settingsGroup;
    private System.Windows.Forms.Button btnZipFile;
    private System.Windows.Forms.TextBox tbZipFile;
    private System.Windows.Forms.Label logDirLabel;
    private System.Windows.Forms.GroupBox step2Group;
    private System.Windows.Forms.Label step2Label4;
    private System.Windows.Forms.Label step2Label3;
    private System.Windows.Forms.Label step2Label2;
    private System.Windows.Forms.Label step2Label1;
    private System.Windows.Forms.Button postTestButton;
    private System.Windows.Forms.GroupBox step1Group;
    private System.Windows.Forms.Label step1Label1;
    private System.Windows.Forms.Button preTestButton;
    private System.Windows.Forms.MainMenu mainMenu;
    private System.Windows.Forms.MenuItem menuItem1;
    private System.Windows.Forms.MenuItem menuItem2;
    private System.Windows.Forms.MenuItem menuItem3;
    private System.Windows.Forms.MenuItem menuItem4;
    private System.Windows.Forms.MenuItem menuItem5;
    private System.Windows.Forms.MenuItem menuItem6;
    private System.Windows.Forms.MenuItem menuItem7;
    private System.Windows.Forms.StatusBar statusBar;
    private System.Windows.Forms.Label step1Label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.MenuItem menuItem8;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Button LaunchMPButton;
    private System.Windows.Forms.Timer tmrUnAttended;
    private System.Windows.Forms.Timer tmrMPWatcher;
    private System.Windows.Forms.Timer tmrWatchdog;
    private System.Windows.Forms.Label label3;
  }
}