using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class TimeShifting
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
      this.groupBoxOther = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelTunerLimit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelParkTimeLimitUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelParkTimeLimit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownTunerLimit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.numericUpDownParkTimeLimit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelBufferSizeDescription = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxBuffer = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelBufferSizePausedDescription = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelBufferLocation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonBufferLocationBrowse = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.textBoxBufferLocation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.numericUpDownBufferSizePaused = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.numericUpDownBufferSize = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelBufferSizePaused = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelBufferSize = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxOther.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTunerLimit)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownParkTimeLimit)).BeginInit();
      this.groupBoxBuffer.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBufferSizePaused)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBufferSize)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxOther
      // 
      this.groupBoxOther.Controls.Add(this.labelTunerLimit);
      this.groupBoxOther.Controls.Add(this.labelParkTimeLimitUnit);
      this.groupBoxOther.Controls.Add(this.labelParkTimeLimit);
      this.groupBoxOther.Controls.Add(this.numericUpDownTunerLimit);
      this.groupBoxOther.Controls.Add(this.numericUpDownParkTimeLimit);
      this.groupBoxOther.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxOther.Location = new System.Drawing.Point(3, 110);
      this.groupBoxOther.Name = "groupBoxOther";
      this.groupBoxOther.Size = new System.Drawing.Size(213, 75);
      this.groupBoxOther.TabIndex = 1;
      this.groupBoxOther.TabStop = false;
      this.groupBoxOther.Text = "Other";
      // 
      // labelTunerLimit
      // 
      this.labelTunerLimit.AutoSize = true;
      this.labelTunerLimit.Location = new System.Drawing.Point(6, 21);
      this.labelTunerLimit.Name = "labelTunerLimit";
      this.labelTunerLimit.Size = new System.Drawing.Size(69, 13);
      this.labelTunerLimit.TabIndex = 0;
      this.labelTunerLimit.Text = "Tuners to try:";
      // 
      // labelParkTimeLimitUnit
      // 
      this.labelParkTimeLimitUnit.AutoSize = true;
      this.labelParkTimeLimitUnit.Location = new System.Drawing.Point(148, 47);
      this.labelParkTimeLimitUnit.Name = "labelParkTimeLimitUnit";
      this.labelParkTimeLimitUnit.Size = new System.Drawing.Size(43, 13);
      this.labelParkTimeLimitUnit.TabIndex = 4;
      this.labelParkTimeLimitUnit.Text = "minutes";
      // 
      // labelParkTimeLimit
      // 
      this.labelParkTimeLimit.AutoSize = true;
      this.labelParkTimeLimit.Location = new System.Drawing.Point(6, 47);
      this.labelParkTimeLimit.Name = "labelParkTimeLimit";
      this.labelParkTimeLimit.Size = new System.Drawing.Size(74, 13);
      this.labelParkTimeLimit.TabIndex = 2;
      this.labelParkTimeLimit.Text = "Park time limit:";
      // 
      // numericUpDownTunerLimit
      // 
      this.numericUpDownTunerLimit.Location = new System.Drawing.Point(86, 19);
      this.numericUpDownTunerLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownTunerLimit.Name = "numericUpDownTunerLimit";
      this.numericUpDownTunerLimit.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownTunerLimit.TabIndex = 1;
      this.numericUpDownTunerLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownTunerLimit.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
      // 
      // numericUpDownParkTimeLimit
      // 
      this.numericUpDownParkTimeLimit.Location = new System.Drawing.Point(86, 45);
      this.numericUpDownParkTimeLimit.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
      this.numericUpDownParkTimeLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownParkTimeLimit.Name = "numericUpDownParkTimeLimit";
      this.numericUpDownParkTimeLimit.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownParkTimeLimit.TabIndex = 3;
      this.numericUpDownParkTimeLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownParkTimeLimit.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      // 
      // labelBufferSizeDescription
      // 
      this.labelBufferSizeDescription.AutoSize = true;
      this.labelBufferSizeDescription.Location = new System.Drawing.Point(148, 47);
      this.labelBufferSizeDescription.Name = "labelBufferSizeDescription";
      this.labelBufferSizeDescription.Size = new System.Drawing.Size(216, 13);
      this.labelBufferSizeDescription.TabIndex = 5;
      this.labelBufferSizeDescription.Text = "GB  (approx. X minutes SD or Y minutes HD)";
      // 
      // groupBoxBuffer
      // 
      this.groupBoxBuffer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxBuffer.Controls.Add(this.labelBufferSizePausedDescription);
      this.groupBoxBuffer.Controls.Add(this.labelBufferLocation);
      this.groupBoxBuffer.Controls.Add(this.buttonBufferLocationBrowse);
      this.groupBoxBuffer.Controls.Add(this.labelBufferSizeDescription);
      this.groupBoxBuffer.Controls.Add(this.textBoxBufferLocation);
      this.groupBoxBuffer.Controls.Add(this.numericUpDownBufferSizePaused);
      this.groupBoxBuffer.Controls.Add(this.numericUpDownBufferSize);
      this.groupBoxBuffer.Controls.Add(this.labelBufferSizePaused);
      this.groupBoxBuffer.Controls.Add(this.labelBufferSize);
      this.groupBoxBuffer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxBuffer.Location = new System.Drawing.Point(3, 3);
      this.groupBoxBuffer.Name = "groupBoxBuffer";
      this.groupBoxBuffer.Size = new System.Drawing.Size(474, 101);
      this.groupBoxBuffer.TabIndex = 0;
      this.groupBoxBuffer.TabStop = false;
      this.groupBoxBuffer.Text = "Buffer";
      // 
      // labelBufferSizePausedDescription
      // 
      this.labelBufferSizePausedDescription.AutoSize = true;
      this.labelBufferSizePausedDescription.Location = new System.Drawing.Point(148, 73);
      this.labelBufferSizePausedDescription.Name = "labelBufferSizePausedDescription";
      this.labelBufferSizePausedDescription.Size = new System.Drawing.Size(216, 13);
      this.labelBufferSizePausedDescription.TabIndex = 8;
      this.labelBufferSizePausedDescription.Text = "GB  (approx. X minutes SD or Y minutes HD)";
      // 
      // labelBufferLocation
      // 
      this.labelBufferLocation.AutoSize = true;
      this.labelBufferLocation.Location = new System.Drawing.Point(6, 22);
      this.labelBufferLocation.Name = "labelBufferLocation";
      this.labelBufferLocation.Size = new System.Drawing.Size(51, 13);
      this.labelBufferLocation.TabIndex = 0;
      this.labelBufferLocation.Text = "Location:";
      // 
      // buttonBufferLocationBrowse
      // 
      this.buttonBufferLocationBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBufferLocationBrowse.Location = new System.Drawing.Point(444, 17);
      this.buttonBufferLocationBrowse.Name = "buttonBufferLocationBrowse";
      this.buttonBufferLocationBrowse.Size = new System.Drawing.Size(24, 23);
      this.buttonBufferLocationBrowse.TabIndex = 2;
      this.buttonBufferLocationBrowse.Text = "...";
      this.buttonBufferLocationBrowse.UseVisualStyleBackColor = true;
      this.buttonBufferLocationBrowse.Click += new System.EventHandler(this.buttonBufferLocationBrowse_Click);
      // 
      // textBoxBufferLocation
      // 
      this.textBoxBufferLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxBufferLocation.Location = new System.Drawing.Point(86, 19);
      this.textBoxBufferLocation.Name = "textBoxBufferLocation";
      this.textBoxBufferLocation.Size = new System.Drawing.Size(352, 20);
      this.textBoxBufferLocation.TabIndex = 1;
      // 
      // numericUpDownBufferSizePaused
      // 
      this.numericUpDownBufferSizePaused.DecimalPlaces = 1;
      this.numericUpDownBufferSizePaused.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
      this.numericUpDownBufferSizePaused.Location = new System.Drawing.Point(86, 71);
      this.numericUpDownBufferSizePaused.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.numericUpDownBufferSizePaused.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            65536});
      this.numericUpDownBufferSizePaused.Name = "numericUpDownBufferSizePaused";
      this.numericUpDownBufferSizePaused.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownBufferSizePaused.TabIndex = 7;
      this.numericUpDownBufferSizePaused.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownBufferSizePaused.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.numericUpDownBufferSizePaused.ValueChanged += new System.EventHandler(this.numericUpDownBufferFileCountMaximum_ValueChanged);
      // 
      // numericUpDownBufferSize
      // 
      this.numericUpDownBufferSize.DecimalPlaces = 1;
      this.numericUpDownBufferSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
      this.numericUpDownBufferSize.Location = new System.Drawing.Point(86, 45);
      this.numericUpDownBufferSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.numericUpDownBufferSize.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            65536});
      this.numericUpDownBufferSize.Name = "numericUpDownBufferSize";
      this.numericUpDownBufferSize.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownBufferSize.TabIndex = 4;
      this.numericUpDownBufferSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownBufferSize.Value = new decimal(new int[] {
            15,
            0,
            0,
            65536});
      this.numericUpDownBufferSize.ValueChanged += new System.EventHandler(this.numericUpDownBufferFileCount_ValueChanged);
      // 
      // labelBufferSizePaused
      // 
      this.labelBufferSizePaused.AutoSize = true;
      this.labelBufferSizePaused.Location = new System.Drawing.Point(5, 73);
      this.labelBufferSizePaused.Name = "labelBufferSizePaused";
      this.labelBufferSizePaused.Size = new System.Drawing.Size(67, 13);
      this.labelBufferSizePaused.TabIndex = 6;
      this.labelBufferSizePaused.Text = "Paused size:";
      // 
      // labelBufferSize
      // 
      this.labelBufferSize.AutoSize = true;
      this.labelBufferSize.Location = new System.Drawing.Point(5, 47);
      this.labelBufferSize.Name = "labelBufferSize";
      this.labelBufferSize.Size = new System.Drawing.Size(74, 13);
      this.labelBufferSize.TabIndex = 3;
      this.labelBufferSize.Text = "Standard size:";
      // 
      // TvTimeshifting
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.groupBoxOther);
      this.Controls.Add(this.groupBoxBuffer);
      this.Name = "TvTimeshifting";
      this.Size = new System.Drawing.Size(480, 420);
      this.groupBoxOther.ResumeLayout(false);
      this.groupBoxOther.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTunerLimit)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownParkTimeLimit)).EndInit();
      this.groupBoxBuffer.ResumeLayout(false);
      this.groupBoxBuffer.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBufferSizePaused)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBufferSize)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MPGroupBox groupBoxBuffer;
    private MPNumericUpDown numericUpDownBufferSizePaused;
    private MPNumericUpDown numericUpDownBufferSize;
    private MPLabel labelBufferSizePaused;
    private MPLabel labelBufferSize;
    private MPGroupBox groupBoxOther;
    private MPLabel labelBufferSizeDescription;
    private MPNumericUpDown numericUpDownTunerLimit;
    private MPLabel labelTunerLimit;
    private MPLabel labelParkTimeLimitUnit;
    private MPNumericUpDown numericUpDownParkTimeLimit;
    private MPLabel labelParkTimeLimit;
    private MPButton buttonBufferLocationBrowse;
    private MPTextBox textBoxBufferLocation;
    private MPLabel labelBufferLocation;
    private MPLabel labelBufferSizePausedDescription;
  }
}