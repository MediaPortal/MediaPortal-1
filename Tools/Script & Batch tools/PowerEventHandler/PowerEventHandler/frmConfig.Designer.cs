namespace PowerEventHandler
{
  partial class frmConfig
  {
    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

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

    #region Vom Windows Form-Designer generierter Code

    /// <summary>
    /// Erforderliche Methode für die Designerunterstützung.
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConfig));
      this.txbLog = new System.Windows.Forms.TextBox();
      this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.btnBrowseOnSuspend = new System.Windows.Forms.Button();
      this.btnBrowseOnResume = new System.Windows.Forms.Button();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.StartTimer = new System.Windows.Forms.Timer(this.components);
      this.cbMinimizeAtStartup = new System.Windows.Forms.CheckBox();
      this.txbOnResume = new System.Windows.Forms.TextBox();
      this.txbOnSuspend = new System.Windows.Forms.TextBox();
      this.cbTurnTVOn = new System.Windows.Forms.CheckBox();
      this.txbLaunchTimeout = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.btnTestOnSuspend = new System.Windows.Forms.Button();
      this.btnTestOnResume = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // txbLog
      // 
      this.txbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txbLog.Location = new System.Drawing.Point(12, 12);
      this.txbLog.Multiline = true;
      this.txbLog.Name = "txbLog";
      this.txbLog.Size = new System.Drawing.Size(522, 248);
      this.txbLog.TabIndex = 0;
      // 
      // notifyIcon
      // 
      this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
      this.notifyIcon.Text = "PowerEventHandler";
      this.notifyIcon.Visible = true;
      this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(167, 268);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(66, 13);
      this.label1.TabIndex = 4;
      this.label1.Text = "OnSuspend:";
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(167, 317);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(63, 13);
      this.label2.TabIndex = 5;
      this.label2.Text = "OnResume:";
      // 
      // btnBrowseOnSuspend
      // 
      this.btnBrowseOnSuspend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBrowseOnSuspend.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnBrowseOnSuspend.Location = new System.Drawing.Point(470, 284);
      this.btnBrowseOnSuspend.Name = "btnBrowseOnSuspend";
      this.btnBrowseOnSuspend.Size = new System.Drawing.Size(29, 19);
      this.btnBrowseOnSuspend.TabIndex = 6;
      this.btnBrowseOnSuspend.Text = "...";
      this.btnBrowseOnSuspend.UseVisualStyleBackColor = true;
      this.btnBrowseOnSuspend.Click += new System.EventHandler(this.btnBrowseOnSuspend_Click);
      // 
      // btnBrowseOnResume
      // 
      this.btnBrowseOnResume.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBrowseOnResume.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnBrowseOnResume.Location = new System.Drawing.Point(470, 333);
      this.btnBrowseOnResume.Name = "btnBrowseOnResume";
      this.btnBrowseOnResume.Size = new System.Drawing.Size(29, 19);
      this.btnBrowseOnResume.TabIndex = 7;
      this.btnBrowseOnResume.Text = "...";
      this.btnBrowseOnResume.UseVisualStyleBackColor = true;
      this.btnBrowseOnResume.Click += new System.EventHandler(this.btnBrowseOnResume_Click);
      // 
      // openFileDialog
      // 
      this.openFileDialog.FileName = "openFileDialog1";
      // 
      // StartTimer
      // 
      this.StartTimer.Interval = 500;
      this.StartTimer.Tick += new System.EventHandler(this.StartTimer_Tick);
      // 
      // cbMinimizeAtStartup
      // 
      this.cbMinimizeAtStartup.AutoSize = true;
      this.cbMinimizeAtStartup.CheckState = global::PowerEventHandler.Properties.Settings.Default.MinimizeAtStartup;
      this.cbMinimizeAtStartup.DataBindings.Add(new System.Windows.Forms.Binding("CheckState", global::PowerEventHandler.Properties.Settings.Default, "MinimizeAtStartup", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.cbMinimizeAtStartup.Location = new System.Drawing.Point(12, 287);
      this.cbMinimizeAtStartup.Name = "cbMinimizeAtStartup";
      this.cbMinimizeAtStartup.Size = new System.Drawing.Size(152, 17);
      this.cbMinimizeAtStartup.TabIndex = 8;
      this.cbMinimizeAtStartup.Text = "Minimize this tool at startup";
      this.cbMinimizeAtStartup.UseVisualStyleBackColor = true;
      // 
      // txbOnResume
      // 
      this.txbOnResume.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txbOnResume.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::PowerEventHandler.Properties.Settings.Default, "PathOnResume", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.txbOnResume.Location = new System.Drawing.Point(170, 333);
      this.txbOnResume.Name = "txbOnResume";
      this.txbOnResume.Size = new System.Drawing.Size(291, 20);
      this.txbOnResume.TabIndex = 3;
      this.txbOnResume.Text = global::PowerEventHandler.Properties.Settings.Default.PathOnResume;
      // 
      // txbOnSuspend
      // 
      this.txbOnSuspend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txbOnSuspend.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::PowerEventHandler.Properties.Settings.Default, "PathOnSuspend", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.txbOnSuspend.Location = new System.Drawing.Point(170, 284);
      this.txbOnSuspend.Name = "txbOnSuspend";
      this.txbOnSuspend.Size = new System.Drawing.Size(291, 20);
      this.txbOnSuspend.TabIndex = 2;
      this.txbOnSuspend.Text = global::PowerEventHandler.Properties.Settings.Default.PathOnSuspend;
      // 
      // cbTurnTVOn
      // 
      this.cbTurnTVOn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cbTurnTVOn.AutoSize = true;
      this.cbTurnTVOn.Checked = true;
      this.cbTurnTVOn.CheckState = global::PowerEventHandler.Properties.Settings.Default.TurnTVOnAtResume;
      this.cbTurnTVOn.DataBindings.Add(new System.Windows.Forms.Binding("CheckState", global::PowerEventHandler.Properties.Settings.Default, "TurnTVOnAtResume", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.cbTurnTVOn.Location = new System.Drawing.Point(12, 267);
      this.cbTurnTVOn.Name = "cbTurnTVOn";
      this.cbTurnTVOn.Size = new System.Drawing.Size(145, 17);
      this.cbTurnTVOn.TabIndex = 1;
      this.cbTurnTVOn.Text = "Turn TV ON after resume";
      this.cbTurnTVOn.UseVisualStyleBackColor = true;
      this.cbTurnTVOn.CheckedChanged += new System.EventHandler(this.cbTurnTVOn_CheckedChanged);
      // 
      // txbLaunchTimeout
      // 
      this.txbLaunchTimeout.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::PowerEventHandler.Properties.Settings.Default, "LaunchTimeOut", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.txbLaunchTimeout.Location = new System.Drawing.Point(95, 314);
      this.txbLaunchTimeout.Name = "txbLaunchTimeout";
      this.txbLaunchTimeout.Size = new System.Drawing.Size(37, 20);
      this.txbLaunchTimeout.TabIndex = 9;
      this.txbLaunchTimeout.Text = global::PowerEventHandler.Properties.Settings.Default.LaunchTimeOut;
      this.txbLaunchTimeout.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txbLaunchTimeout_KeyDown);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(9, 317);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(86, 13);
      this.label3.TabIndex = 10;
      this.label3.Text = "Launch TimeOut";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(133, 317);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(24, 13);
      this.label4.TabIndex = 11;
      this.label4.Text = "sec";
      // 
      // btnTestOnSuspend
      // 
      this.btnTestOnSuspend.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnTestOnSuspend.Location = new System.Drawing.Point(503, 284);
      this.btnTestOnSuspend.Name = "btnTestOnSuspend";
      this.btnTestOnSuspend.Size = new System.Drawing.Size(31, 19);
      this.btnTestOnSuspend.TabIndex = 12;
      this.btnTestOnSuspend.Text = "Test";
      this.btnTestOnSuspend.UseVisualStyleBackColor = true;
      this.btnTestOnSuspend.Click += new System.EventHandler(this.btnTestOnSuspend_Click);
      // 
      // btnTestOnResume
      // 
      this.btnTestOnResume.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnTestOnResume.Location = new System.Drawing.Point(503, 333);
      this.btnTestOnResume.Name = "btnTestOnResume";
      this.btnTestOnResume.Size = new System.Drawing.Size(31, 19);
      this.btnTestOnResume.TabIndex = 13;
      this.btnTestOnResume.Text = "Test";
      this.btnTestOnResume.UseVisualStyleBackColor = true;
      this.btnTestOnResume.Click += new System.EventHandler(this.btnTestOnResume_Click);
      // 
      // frmConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(546, 365);
      this.Controls.Add(this.btnTestOnResume);
      this.Controls.Add(this.btnTestOnSuspend);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.txbLaunchTimeout);
      this.Controls.Add(this.cbMinimizeAtStartup);
      this.Controls.Add(this.btnBrowseOnResume);
      this.Controls.Add(this.btnBrowseOnSuspend);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.txbOnResume);
      this.Controls.Add(this.txbOnSuspend);
      this.Controls.Add(this.cbTurnTVOn);
      this.Controls.Add(this.txbLog);
      this.MaximizeBox = false;
      this.Name = "frmConfig";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Log & Config";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmConfig_FormClosed);
      this.SizeChanged += new System.EventHandler(this.frmConfig_SizeChanged);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmConfig_FormClosing);
      this.Load += new System.EventHandler(this.frmConfig_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txbLog;
    private System.Windows.Forms.CheckBox cbTurnTVOn;
    private System.Windows.Forms.NotifyIcon notifyIcon;
    private System.Windows.Forms.TextBox txbOnSuspend;
    private System.Windows.Forms.TextBox txbOnResume;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btnBrowseOnSuspend;
    private System.Windows.Forms.Button btnBrowseOnResume;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.Timer StartTimer;
    private System.Windows.Forms.CheckBox cbMinimizeAtStartup;
      private System.Windows.Forms.TextBox txbLaunchTimeout;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Button btnTestOnSuspend;
    private System.Windows.Forms.Button btnTestOnResume;
  }
}

