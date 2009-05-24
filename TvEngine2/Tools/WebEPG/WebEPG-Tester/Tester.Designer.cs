namespace MediaPortal.EPG.WebEPGTester
{
  partial class fTester
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fTester));
      this.tvGrabbers = new System.Windows.Forms.TreeView();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.cbCache = new System.Windows.Forms.CheckBox();
      this.bRun = new System.Windows.Forms.Button();
      this.gbSetup = new System.Windows.Forms.GroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.numDays = new System.Windows.Forms.NumericUpDown();
      this.rbFirstLast = new System.Windows.Forms.RadioButton();
      this.rbNumb = new System.Windows.Forms.RadioButton();
      this.rbAllDays = new System.Windows.Forms.RadioButton();
      this.gbView = new System.Windows.Forms.GroupBox();
      this.bScan = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.tbGrabberDir = new System.Windows.Forms.TextBox();
      this.gbLog = new System.Windows.Forms.GroupBox();
      this.tbLog = new System.Windows.Forms.TextBox();
      this.gbSetup.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numDays)).BeginInit();
      this.gbView.SuspendLayout();
      this.gbLog.SuspendLayout();
      this.SuspendLayout();
      // 
      // tvGrabbers
      // 
      this.tvGrabbers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.tvGrabbers.CheckBoxes = true;
      this.tvGrabbers.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tvGrabbers.ImageIndex = 0;
      this.tvGrabbers.ImageList = this.imageList1;
      this.tvGrabbers.Location = new System.Drawing.Point(6, 19);
      this.tvGrabbers.Name = "tvGrabbers";
      this.tvGrabbers.SelectedImageIndex = 0;
      this.tvGrabbers.Size = new System.Drawing.Size(294, 489);
      this.tvGrabbers.TabIndex = 0;
      this.tvGrabbers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvGrabbers_AfterSelect);
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "help-256x256.png");
      this.imageList1.Images.SetKeyName(1, "right.gif");
      this.imageList1.Images.SetKeyName(2, "icon_success_sml.gif");
      this.imageList1.Images.SetKeyName(3, "icon_warning_sml.gif");
      this.imageList1.Images.SetKeyName(4, "icon_error_sml.gif");
      // 
      // cbCache
      // 
      this.cbCache.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.cbCache.AutoSize = true;
      this.cbCache.Checked = true;
      this.cbCache.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbCache.Location = new System.Drawing.Point(292, 15);
      this.cbCache.Name = "cbCache";
      this.cbCache.Size = new System.Drawing.Size(57, 17);
      this.cbCache.TabIndex = 1;
      this.cbCache.Text = "Cache";
      this.cbCache.UseVisualStyleBackColor = true;
      // 
      // bRun
      // 
      this.bRun.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.bRun.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.bRun.Location = new System.Drawing.Point(279, 45);
      this.bRun.Name = "bRun";
      this.bRun.Size = new System.Drawing.Size(97, 31);
      this.bRun.TabIndex = 4;
      this.bRun.Text = "Run Tests";
      this.bRun.UseVisualStyleBackColor = true;
      this.bRun.Click += new System.EventHandler(this.bRun_Click);
      // 
      // gbSetup
      // 
      this.gbSetup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbSetup.Controls.Add(this.label1);
      this.gbSetup.Controls.Add(this.numDays);
      this.gbSetup.Controls.Add(this.rbFirstLast);
      this.gbSetup.Controls.Add(this.rbNumb);
      this.gbSetup.Controls.Add(this.rbAllDays);
      this.gbSetup.Controls.Add(this.bRun);
      this.gbSetup.Controls.Add(this.cbCache);
      this.gbSetup.Location = new System.Drawing.Point(324, 5);
      this.gbSetup.Name = "gbSetup";
      this.gbSetup.Size = new System.Drawing.Size(407, 82);
      this.gbSetup.TabIndex = 5;
      this.gbSetup.TabStop = false;
      this.gbSetup.Text = "Test Setup";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(20, 19);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(31, 13);
      this.label1.TabIndex = 9;
      this.label1.Text = "Days";
      // 
      // numDays
      // 
      this.numDays.Location = new System.Drawing.Point(77, 12);
      this.numDays.Maximum = new decimal(new int[] {
            7,
            0,
            0,
            0});
      this.numDays.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numDays.Name = "numDays";
      this.numDays.Size = new System.Drawing.Size(40, 20);
      this.numDays.TabIndex = 8;
      this.numDays.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // rbFirstLast
      // 
      this.rbFirstLast.AutoSize = true;
      this.rbFirstLast.ForeColor = System.Drawing.SystemColors.GrayText;
      this.rbFirstLast.Location = new System.Drawing.Point(57, 38);
      this.rbFirstLast.Name = "rbFirstLast";
      this.rbFirstLast.Size = new System.Drawing.Size(76, 17);
      this.rbFirstLast.TabIndex = 7;
      this.rbFirstLast.Text = "First + Last";
      this.rbFirstLast.UseVisualStyleBackColor = true;
      // 
      // rbNumb
      // 
      this.rbNumb.AutoSize = true;
      this.rbNumb.Checked = true;
      this.rbNumb.Location = new System.Drawing.Point(57, 19);
      this.rbNumb.Name = "rbNumb";
      this.rbNumb.Size = new System.Drawing.Size(14, 13);
      this.rbNumb.TabIndex = 6;
      this.rbNumb.TabStop = true;
      this.rbNumb.UseVisualStyleBackColor = true;
      // 
      // rbAllDays
      // 
      this.rbAllDays.AutoSize = true;
      this.rbAllDays.ForeColor = System.Drawing.SystemColors.GrayText;
      this.rbAllDays.Location = new System.Drawing.Point(57, 59);
      this.rbAllDays.Name = "rbAllDays";
      this.rbAllDays.Size = new System.Drawing.Size(36, 17);
      this.rbAllDays.TabIndex = 5;
      this.rbAllDays.Text = "All";
      this.rbAllDays.UseVisualStyleBackColor = true;
      // 
      // gbView
      // 
      this.gbView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.gbView.Controls.Add(this.bScan);
      this.gbView.Controls.Add(this.tvGrabbers);
      this.gbView.Controls.Add(this.label2);
      this.gbView.Controls.Add(this.tbGrabberDir);
      this.gbView.Location = new System.Drawing.Point(12, 5);
      this.gbView.Name = "gbView";
      this.gbView.Size = new System.Drawing.Size(306, 540);
      this.gbView.TabIndex = 8;
      this.gbView.TabStop = false;
      this.gbView.Text = "Sites / Channels";
      // 
      // bScan
      // 
      this.bScan.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      this.bScan.Location = new System.Drawing.Point(237, 515);
      this.bScan.Name = "bScan";
      this.bScan.Size = new System.Drawing.Size(63, 19);
      this.bScan.TabIndex = 12;
      this.bScan.Text = "Scan";
      this.bScan.UseVisualStyleBackColor = true;
      this.bScan.Click += new System.EventHandler(this.bScan_Click);
      // 
      // label2
      // 
      this.label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(6, 517);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(51, 13);
      this.label2.TabIndex = 11;
      this.label2.Text = "Location:";
      // 
      // tbGrabberDir
      // 
      this.tbGrabberDir.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      this.tbGrabberDir.Location = new System.Drawing.Point(63, 514);
      this.tbGrabberDir.Name = "tbGrabberDir";
      this.tbGrabberDir.Size = new System.Drawing.Size(167, 20);
      this.tbGrabberDir.TabIndex = 10;
      // 
      // gbLog
      // 
      this.gbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbLog.Controls.Add(this.tbLog);
      this.gbLog.Location = new System.Drawing.Point(324, 93);
      this.gbLog.Name = "gbLog";
      this.gbLog.Size = new System.Drawing.Size(407, 452);
      this.gbLog.TabIndex = 9;
      this.gbLog.TabStop = false;
      this.gbLog.Text = "Log";
      // 
      // tbLog
      // 
      this.tbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbLog.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbLog.Location = new System.Drawing.Point(6, 19);
      this.tbLog.Multiline = true;
      this.tbLog.Name = "tbLog";
      this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.tbLog.Size = new System.Drawing.Size(395, 427);
      this.tbLog.TabIndex = 0;
      // 
      // fTester
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(743, 557);
      this.Controls.Add(this.gbView);
      this.Controls.Add(this.gbSetup);
      this.Controls.Add(this.gbLog);
      this.Name = "fTester";
      this.Text = "Tester";
      this.gbSetup.ResumeLayout(false);
      this.gbSetup.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numDays)).EndInit();
      this.gbView.ResumeLayout(false);
      this.gbView.PerformLayout();
      this.gbLog.ResumeLayout(false);
      this.gbLog.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TreeView tvGrabbers;
    private System.Windows.Forms.CheckBox cbCache;
    private System.Windows.Forms.Button bRun;
    private System.Windows.Forms.GroupBox gbSetup;
    private System.Windows.Forms.GroupBox gbView;
    private System.Windows.Forms.GroupBox gbLog;
    private System.Windows.Forms.TextBox tbLog;
    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.RadioButton rbFirstLast;
    private System.Windows.Forms.RadioButton rbNumb;
    private System.Windows.Forms.RadioButton rbAllDays;
    private System.Windows.Forms.NumericUpDown numDays;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox tbGrabberDir;
    private System.Windows.Forms.Button bScan;
  }
}