using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class TvTimeshifting
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
      this.numericUpDownTunerLimit = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownParkTimeLimit = new System.Windows.Forms.NumericUpDown();
      this.labelBufferFileCountDescription = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxBuffer = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelBufferFileCountMaximumDescription = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelBufferLocation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonBufferLocationBrowse = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.textBoxBufferLocation = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.numericUpDownBufferFileSize = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownBufferFileCountMaximum = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownBufferFileCount = new System.Windows.Forms.NumericUpDown();
      this.labelBufferFileSizeUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelBufferFileCountMaximum = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelBufferFileCount = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelBufferFileSize = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxOther.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTunerLimit)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownParkTimeLimit)).BeginInit();
      this.groupBoxBuffer.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBufferFileSize)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBufferFileCountMaximum)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBufferFileCount)).BeginInit();
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
      this.groupBoxOther.Location = new System.Drawing.Point(3, 139);
      this.groupBoxOther.Name = "groupBoxOther";
      this.groupBoxOther.Size = new System.Drawing.Size(234, 75);
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
      this.labelParkTimeLimitUnit.Location = new System.Drawing.Point(173, 47);
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
      this.numericUpDownTunerLimit.Location = new System.Drawing.Point(111, 19);
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
      this.numericUpDownParkTimeLimit.Location = new System.Drawing.Point(111, 45);
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
      // labelBufferFileCountDescription
      // 
      this.labelBufferFileCountDescription.AutoSize = true;
      this.labelBufferFileCountDescription.Location = new System.Drawing.Point(177, 73);
      this.labelBufferFileCountDescription.Name = "labelBufferFileCountDescription";
      this.labelBufferFileCountDescription.Size = new System.Drawing.Size(275, 13);
      this.labelBufferFileCountDescription.TabIndex = 8;
      this.labelBufferFileCountDescription.Text = "=> X.XX GB (approx. Y.Y minutes SD or Z.Z minutes HD)";
      // 
      // groupBoxBuffer
      // 
      this.groupBoxBuffer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxBuffer.Controls.Add(this.labelBufferFileCountMaximumDescription);
      this.groupBoxBuffer.Controls.Add(this.labelBufferLocation);
      this.groupBoxBuffer.Controls.Add(this.buttonBufferLocationBrowse);
      this.groupBoxBuffer.Controls.Add(this.labelBufferFileCountDescription);
      this.groupBoxBuffer.Controls.Add(this.textBoxBufferLocation);
      this.groupBoxBuffer.Controls.Add(this.numericUpDownBufferFileSize);
      this.groupBoxBuffer.Controls.Add(this.numericUpDownBufferFileCountMaximum);
      this.groupBoxBuffer.Controls.Add(this.numericUpDownBufferFileCount);
      this.groupBoxBuffer.Controls.Add(this.labelBufferFileSizeUnit);
      this.groupBoxBuffer.Controls.Add(this.labelBufferFileCountMaximum);
      this.groupBoxBuffer.Controls.Add(this.labelBufferFileCount);
      this.groupBoxBuffer.Controls.Add(this.labelBufferFileSize);
      this.groupBoxBuffer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxBuffer.Location = new System.Drawing.Point(3, 3);
      this.groupBoxBuffer.Name = "groupBoxBuffer";
      this.groupBoxBuffer.Size = new System.Drawing.Size(474, 130);
      this.groupBoxBuffer.TabIndex = 0;
      this.groupBoxBuffer.TabStop = false;
      this.groupBoxBuffer.Text = "Buffer";
      // 
      // labelBufferFileCountMaximumDescription
      // 
      this.labelBufferFileCountMaximumDescription.AutoSize = true;
      this.labelBufferFileCountMaximumDescription.Location = new System.Drawing.Point(177, 99);
      this.labelBufferFileCountMaximumDescription.Name = "labelBufferFileCountMaximumDescription";
      this.labelBufferFileCountMaximumDescription.Size = new System.Drawing.Size(275, 13);
      this.labelBufferFileCountMaximumDescription.TabIndex = 11;
      this.labelBufferFileCountMaximumDescription.Text = "=> X.XX GB (approx. Y.Y minutes SD or Z.Z minutes HD)";
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
      this.textBoxBufferLocation.Location = new System.Drawing.Point(111, 19);
      this.textBoxBufferLocation.Name = "textBoxBufferLocation";
      this.textBoxBufferLocation.Size = new System.Drawing.Size(327, 20);
      this.textBoxBufferLocation.TabIndex = 1;
      // 
      // numericUpDownBufferFileSize
      // 
      this.numericUpDownBufferFileSize.Location = new System.Drawing.Point(111, 45);
      this.numericUpDownBufferFileSize.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
      this.numericUpDownBufferFileSize.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
      this.numericUpDownBufferFileSize.Name = "numericUpDownBufferFileSize";
      this.numericUpDownBufferFileSize.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownBufferFileSize.TabIndex = 4;
      this.numericUpDownBufferFileSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownBufferFileSize.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
      this.numericUpDownBufferFileSize.ValueChanged += new System.EventHandler(this.numericUpDownBufferFileSize_ValueChanged);
      // 
      // numericUpDownBufferFileCountMaximum
      // 
      this.numericUpDownBufferFileCountMaximum.Location = new System.Drawing.Point(111, 97);
      this.numericUpDownBufferFileCountMaximum.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
      this.numericUpDownBufferFileCountMaximum.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
      this.numericUpDownBufferFileCountMaximum.Name = "numericUpDownBufferFileCountMaximum";
      this.numericUpDownBufferFileCountMaximum.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownBufferFileCountMaximum.TabIndex = 10;
      this.numericUpDownBufferFileCountMaximum.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownBufferFileCountMaximum.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
      this.numericUpDownBufferFileCountMaximum.ValueChanged += new System.EventHandler(this.numericUpDownBufferFileCountMaximum_ValueChanged);
      // 
      // numericUpDownBufferFileCount
      // 
      this.numericUpDownBufferFileCount.Location = new System.Drawing.Point(111, 71);
      this.numericUpDownBufferFileCount.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
      this.numericUpDownBufferFileCount.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
      this.numericUpDownBufferFileCount.Name = "numericUpDownBufferFileCount";
      this.numericUpDownBufferFileCount.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownBufferFileCount.TabIndex = 7;
      this.numericUpDownBufferFileCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownBufferFileCount.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
      this.numericUpDownBufferFileCount.ValueChanged += new System.EventHandler(this.numericUpDownBufferFileCount_ValueChanged);
      // 
      // labelBufferFileSizeUnit
      // 
      this.labelBufferFileSizeUnit.AutoSize = true;
      this.labelBufferFileSizeUnit.Location = new System.Drawing.Point(173, 47);
      this.labelBufferFileSizeUnit.Name = "labelBufferFileSizeUnit";
      this.labelBufferFileSizeUnit.Size = new System.Drawing.Size(23, 13);
      this.labelBufferFileSizeUnit.TabIndex = 5;
      this.labelBufferFileSizeUnit.Text = "MB";
      // 
      // labelBufferFileCountMaximum
      // 
      this.labelBufferFileCountMaximum.AutoSize = true;
      this.labelBufferFileCountMaximum.Location = new System.Drawing.Point(5, 99);
      this.labelBufferFileCountMaximum.Name = "labelBufferFileCountMaximum";
      this.labelBufferFileCountMaximum.Size = new System.Drawing.Size(100, 13);
      this.labelBufferFileCountMaximum.TabIndex = 9;
      this.labelBufferFileCountMaximum.Text = "Maximum file count:";
      // 
      // labelBufferFileCount
      // 
      this.labelBufferFileCount.AutoSize = true;
      this.labelBufferFileCount.Location = new System.Drawing.Point(5, 73);
      this.labelBufferFileCount.Name = "labelBufferFileCount";
      this.labelBufferFileCount.Size = new System.Drawing.Size(99, 13);
      this.labelBufferFileCount.TabIndex = 6;
      this.labelBufferFileCount.Text = "Standard file count:";
      // 
      // labelBufferFileSize
      // 
      this.labelBufferFileSize.AutoSize = true;
      this.labelBufferFileSize.Location = new System.Drawing.Point(5, 47);
      this.labelBufferFileSize.Name = "labelBufferFileSize";
      this.labelBufferFileSize.Size = new System.Drawing.Size(47, 13);
      this.labelBufferFileSize.TabIndex = 3;
      this.labelBufferFileSize.Text = "File size:";
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
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBufferFileSize)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBufferFileCountMaximum)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBufferFileCount)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MPGroupBox groupBoxBuffer;
    private System.Windows.Forms.NumericUpDown numericUpDownBufferFileSize;
    private System.Windows.Forms.NumericUpDown numericUpDownBufferFileCountMaximum;
    private System.Windows.Forms.NumericUpDown numericUpDownBufferFileCount;
    private MPLabel labelBufferFileSizeUnit;
    private MPLabel labelBufferFileSize;
    private MPLabel labelBufferFileCountMaximum;
    private MPLabel labelBufferFileCount;
    private MPGroupBox groupBoxOther;
    private MPLabel labelBufferFileCountDescription;
    private System.Windows.Forms.NumericUpDown numericUpDownTunerLimit;
    private MPLabel labelTunerLimit;
    private MPLabel labelParkTimeLimitUnit;
    private System.Windows.Forms.NumericUpDown numericUpDownParkTimeLimit;
    private MPLabel labelParkTimeLimit;
    private MPButton buttonBufferLocationBrowse;
    private MPTextBox textBoxBufferLocation;
    private MPLabel labelBufferLocation;
    private MPLabel labelBufferFileCountMaximumDescription;
  }
}