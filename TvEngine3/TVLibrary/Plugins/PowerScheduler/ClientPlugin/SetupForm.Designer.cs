namespace MediaPortal.Plugins.Process
{
  partial class SetupForm
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
      this.modeLabel = new System.Windows.Forms.Label();
      this.timeoutLabel = new System.Windows.Forms.Label();
      this.idleNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.generalGroupBox = new System.Windows.Forms.GroupBox();
      this.minutesLabel = new System.Windows.Forms.Label();
      this.extLogCheckBox = new System.Windows.Forms.CheckBox();
      this.homeOnlyCheckBox = new System.Windows.Forms.CheckBox();
      this.multiGroupBox = new System.Windows.Forms.GroupBox();
      this.forceCheckBox = new System.Windows.Forms.CheckBox();
      this.shutModeComboBox = new System.Windows.Forms.ComboBox();
      this.okButton = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.idleNumericUpDown)).BeginInit();
      this.generalGroupBox.SuspendLayout();
      this.multiGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // modeLabel
      // 
      this.modeLabel.AutoSize = true;
      this.modeLabel.Location = new System.Drawing.Point(19, 25);
      this.modeLabel.Name = "modeLabel";
      this.modeLabel.Size = new System.Drawing.Size(87, 13);
      this.modeLabel.TabIndex = 10;
      this.modeLabel.Text = "Shutdown mode:";
      // 
      // timeoutLabel
      // 
      this.timeoutLabel.AutoSize = true;
      this.timeoutLabel.Location = new System.Drawing.Point(19, 19);
      this.timeoutLabel.Name = "timeoutLabel";
      this.timeoutLabel.Size = new System.Drawing.Size(64, 13);
      this.timeoutLabel.TabIndex = 0;
      this.timeoutLabel.Text = "Idle timeout:";
      // 
      // idleNumericUpDown
      // 
      this.idleNumericUpDown.Location = new System.Drawing.Point(89, 16);
      this.idleNumericUpDown.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
      this.idleNumericUpDown.Name = "idleNumericUpDown";
      this.idleNumericUpDown.Size = new System.Drawing.Size(46, 20);
      this.idleNumericUpDown.TabIndex = 1;
      // 
      // generalGroupBox
      // 
      this.generalGroupBox.Controls.Add(this.minutesLabel);
      this.generalGroupBox.Controls.Add(this.extLogCheckBox);
      this.generalGroupBox.Controls.Add(this.homeOnlyCheckBox);
      this.generalGroupBox.Controls.Add(this.timeoutLabel);
      this.generalGroupBox.Controls.Add(this.idleNumericUpDown);
      this.generalGroupBox.Location = new System.Drawing.Point(13, 13);
      this.generalGroupBox.Name = "generalGroupBox";
      this.generalGroupBox.Size = new System.Drawing.Size(257, 91);
      this.generalGroupBox.TabIndex = 11;
      this.generalGroupBox.TabStop = false;
      this.generalGroupBox.Text = "General settings";
      // 
      // minutesLabel
      // 
      this.minutesLabel.AutoSize = true;
      this.minutesLabel.Location = new System.Drawing.Point(136, 20);
      this.minutesLabel.Name = "minutesLabel";
      this.minutesLabel.Size = new System.Drawing.Size(43, 13);
      this.minutesLabel.TabIndex = 15;
      this.minutesLabel.Text = "minutes";
      // 
      // extLogCheckBox
      // 
      this.extLogCheckBox.AutoSize = true;
      this.extLogCheckBox.Location = new System.Drawing.Point(22, 63);
      this.extLogCheckBox.Name = "extLogCheckBox";
      this.extLogCheckBox.Size = new System.Drawing.Size(144, 17);
      this.extLogCheckBox.TabIndex = 3;
      this.extLogCheckBox.Text = "Enable extensive logging";
      this.extLogCheckBox.UseVisualStyleBackColor = true;
      // 
      // homeOnlyCheckBox
      // 
      this.homeOnlyCheckBox.AutoSize = true;
      this.homeOnlyCheckBox.Checked = true;
      this.homeOnlyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.homeOnlyCheckBox.Location = new System.Drawing.Point(22, 42);
      this.homeOnlyCheckBox.Name = "homeOnlyCheckBox";
      this.homeOnlyCheckBox.Size = new System.Drawing.Size(226, 17);
      this.homeOnlyCheckBox.TabIndex = 2;
      this.homeOnlyCheckBox.Text = "Only allow standby when on home window";
      this.homeOnlyCheckBox.UseVisualStyleBackColor = true;
      // 
      // multiGroupBox
      // 
      this.multiGroupBox.Controls.Add(this.forceCheckBox);
      this.multiGroupBox.Controls.Add(this.shutModeComboBox);
      this.multiGroupBox.Controls.Add(this.modeLabel);
      this.multiGroupBox.Location = new System.Drawing.Point(13, 111);
      this.multiGroupBox.Name = "multiGroupBox";
      this.multiGroupBox.Size = new System.Drawing.Size(257, 83);
      this.multiGroupBox.TabIndex = 12;
      this.multiGroupBox.TabStop = false;
      this.multiGroupBox.Text = "multi-seat local settings";
      // 
      // forceCheckBox
      // 
      this.forceCheckBox.AutoSize = true;
      this.forceCheckBox.Location = new System.Drawing.Point(22, 52);
      this.forceCheckBox.Name = "forceCheckBox";
      this.forceCheckBox.Size = new System.Drawing.Size(195, 17);
      this.forceCheckBox.TabIndex = 12;
      this.forceCheckBox.Text = "Force the system into standby mode";
      this.forceCheckBox.UseVisualStyleBackColor = true;
      // 
      // shutModeComboBox
      // 
      this.shutModeComboBox.FormattingEnabled = true;
      this.shutModeComboBox.Items.AddRange(new object[] {
            "suspend",
            "hibernate"});
      this.shutModeComboBox.Location = new System.Drawing.Point(109, 22);
      this.shutModeComboBox.Name = "shutModeComboBox";
      this.shutModeComboBox.Size = new System.Drawing.Size(110, 21);
      this.shutModeComboBox.TabIndex = 11;
      this.shutModeComboBox.Text = "suspend";
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(104, 200);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 13;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // SetupForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 229);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.multiGroupBox);
      this.Controls.Add(this.generalGroupBox);
      this.Name = "SetupForm";
      this.Text = "PowerScheduler settings";
      ((System.ComponentModel.ISupportInitialize)(this.idleNumericUpDown)).EndInit();
      this.generalGroupBox.ResumeLayout(false);
      this.generalGroupBox.PerformLayout();
      this.multiGroupBox.ResumeLayout(false);
      this.multiGroupBox.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label modeLabel;
    private System.Windows.Forms.Label timeoutLabel;
    private System.Windows.Forms.NumericUpDown idleNumericUpDown;
    private System.Windows.Forms.GroupBox generalGroupBox;
    private System.Windows.Forms.CheckBox homeOnlyCheckBox;
    private System.Windows.Forms.Label minutesLabel;
    private System.Windows.Forms.CheckBox extLogCheckBox;
    private System.Windows.Forms.GroupBox multiGroupBox;
    private System.Windows.Forms.CheckBox forceCheckBox;
    private System.Windows.Forms.ComboBox shutModeComboBox;
    private System.Windows.Forms.Button okButton;
  }
}