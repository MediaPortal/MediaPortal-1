using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.Config
{
  partial class LearnSetTopBox
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
      this.buttonOkay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonCancel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.textBoxName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.labelName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelCommandDelay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownCommandDelay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.listViewCommands = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderCommand = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderStatus = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.numericUpDownPowerChangeDelay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelPowerChangeDelay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelCommandDelayUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelPowerChangeDelayUnit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonLearn = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonForget = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelDigitCount = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxDigitCount = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCommandDelay)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPowerChangeDelay)).BeginInit();
      this.SuspendLayout();
      // 
      // buttonOkay
      // 
      this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonOkay.Enabled = false;
      this.buttonOkay.Location = new System.Drawing.Point(16, 413);
      this.buttonOkay.Name = "buttonOkay";
      this.buttonOkay.Size = new System.Drawing.Size(75, 23);
      this.buttonOkay.TabIndex = 13;
      this.buttonOkay.Text = "&OK";
      this.buttonOkay.Click += new System.EventHandler(this.buttonOkay_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(182, 413);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 14;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // textBoxName
      // 
      this.textBoxName.Location = new System.Drawing.Point(57, 10);
      this.textBoxName.Name = "textBoxName";
      this.textBoxName.Size = new System.Drawing.Size(200, 20);
      this.textBoxName.TabIndex = 1;
      this.textBoxName.TextChanged += new System.EventHandler(this.textBoxName_TextChanged);
      // 
      // labelName
      // 
      this.labelName.AutoSize = true;
      this.labelName.Location = new System.Drawing.Point(13, 13);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size(38, 13);
      this.labelName.TabIndex = 0;
      this.labelName.Text = "Name:";
      // 
      // labelCommandDelay
      // 
      this.labelCommandDelay.AutoSize = true;
      this.labelCommandDelay.Location = new System.Drawing.Point(13, 326);
      this.labelCommandDelay.Name = "labelCommandDelay";
      this.labelCommandDelay.Size = new System.Drawing.Size(85, 13);
      this.labelCommandDelay.TabIndex = 5;
      this.labelCommandDelay.Text = "Command delay:";
      // 
      // numericUpDownCommandDelay
      // 
      this.numericUpDownCommandDelay.Location = new System.Drawing.Point(126, 324);
      this.numericUpDownCommandDelay.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
      this.numericUpDownCommandDelay.Name = "numericUpDownCommandDelay";
      this.numericUpDownCommandDelay.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownCommandDelay.TabIndex = 6;
      this.numericUpDownCommandDelay.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
      // 
      // listViewCommands
      // 
      this.listViewCommands.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderCommand,
            this.columnHeaderStatus});
      this.listViewCommands.FullRowSelect = true;
      this.listViewCommands.Location = new System.Drawing.Point(16, 36);
      this.listViewCommands.MultiSelect = false;
      this.listViewCommands.Name = "listViewCommands";
      this.listViewCommands.Size = new System.Drawing.Size(241, 244);
      this.listViewCommands.TabIndex = 2;
      this.listViewCommands.UseCompatibleStateImageBehavior = false;
      this.listViewCommands.View = System.Windows.Forms.View.Details;
      this.listViewCommands.SelectedIndexChanged += new System.EventHandler(this.listViewCommands_SelectedIndexChanged);
      // 
      // columnHeaderCommand
      // 
      this.columnHeaderCommand.Text = "Command";
      this.columnHeaderCommand.Width = 133;
      // 
      // columnHeaderStatus
      // 
      this.columnHeaderStatus.Text = "Learn Status";
      this.columnHeaderStatus.Width = 84;
      // 
      // numericUpDownPowerChangeDelay
      // 
      this.numericUpDownPowerChangeDelay.Location = new System.Drawing.Point(126, 350);
      this.numericUpDownPowerChangeDelay.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
      this.numericUpDownPowerChangeDelay.Name = "numericUpDownPowerChangeDelay";
      this.numericUpDownPowerChangeDelay.Size = new System.Drawing.Size(60, 20);
      this.numericUpDownPowerChangeDelay.TabIndex = 9;
      this.numericUpDownPowerChangeDelay.Value = new decimal(new int[] {
            3000,
            0,
            0,
            0});
      // 
      // labelPowerChangeDelay
      // 
      this.labelPowerChangeDelay.AutoSize = true;
      this.labelPowerChangeDelay.Location = new System.Drawing.Point(13, 352);
      this.labelPowerChangeDelay.Name = "labelPowerChangeDelay";
      this.labelPowerChangeDelay.Size = new System.Drawing.Size(107, 13);
      this.labelPowerChangeDelay.TabIndex = 8;
      this.labelPowerChangeDelay.Text = "Power change delay:";
      // 
      // labelCommandDelayUnit
      // 
      this.labelCommandDelayUnit.AutoSize = true;
      this.labelCommandDelayUnit.Location = new System.Drawing.Point(188, 326);
      this.labelCommandDelayUnit.Name = "labelCommandDelayUnit";
      this.labelCommandDelayUnit.Size = new System.Drawing.Size(20, 13);
      this.labelCommandDelayUnit.TabIndex = 7;
      this.labelCommandDelayUnit.Text = "ms";
      // 
      // labelPowerChangeDelayUnit
      // 
      this.labelPowerChangeDelayUnit.AutoSize = true;
      this.labelPowerChangeDelayUnit.Location = new System.Drawing.Point(188, 352);
      this.labelPowerChangeDelayUnit.Name = "labelPowerChangeDelayUnit";
      this.labelPowerChangeDelayUnit.Size = new System.Drawing.Size(20, 13);
      this.labelPowerChangeDelayUnit.TabIndex = 10;
      this.labelPowerChangeDelayUnit.Text = "ms";
      // 
      // buttonLearn
      // 
      this.buttonLearn.Location = new System.Drawing.Point(16, 286);
      this.buttonLearn.Name = "buttonLearn";
      this.buttonLearn.Size = new System.Drawing.Size(60, 23);
      this.buttonLearn.TabIndex = 3;
      this.buttonLearn.Text = "&Learn";
      this.buttonLearn.Click += new System.EventHandler(this.buttonLearn_Click);
      // 
      // buttonForget
      // 
      this.buttonForget.Location = new System.Drawing.Point(82, 286);
      this.buttonForget.Name = "buttonForget";
      this.buttonForget.Size = new System.Drawing.Size(60, 23);
      this.buttonForget.TabIndex = 4;
      this.buttonForget.Text = "&Forget";
      this.buttonForget.Click += new System.EventHandler(this.buttonForget_Click);
      // 
      // labelDigitCount
      // 
      this.labelDigitCount.AutoSize = true;
      this.labelDigitCount.Location = new System.Drawing.Point(13, 379);
      this.labelDigitCount.Name = "labelDigitCount";
      this.labelDigitCount.Size = new System.Drawing.Size(86, 13);
      this.labelDigitCount.TabIndex = 11;
      this.labelDigitCount.Text = "Channel # digits:";
      // 
      // comboBoxDigitCount
      // 
      this.comboBoxDigitCount.FormattingEnabled = true;
      this.comboBoxDigitCount.Items.AddRange(new object[] {
            "None",
            "2 Digits",
            "3 Digits",
            "4 Digits",
            "5 Digits"});
      this.comboBoxDigitCount.Location = new System.Drawing.Point(126, 376);
      this.comboBoxDigitCount.Name = "comboBoxDigitCount";
      this.comboBoxDigitCount.Size = new System.Drawing.Size(60, 21);
      this.comboBoxDigitCount.TabIndex = 12;
      // 
      // LearnSetTopBox
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(278, 448);
      this.Controls.Add(this.comboBoxDigitCount);
      this.Controls.Add(this.labelDigitCount);
      this.Controls.Add(this.buttonForget);
      this.Controls.Add(this.buttonLearn);
      this.Controls.Add(this.labelPowerChangeDelayUnit);
      this.Controls.Add(this.labelCommandDelayUnit);
      this.Controls.Add(this.numericUpDownPowerChangeDelay);
      this.Controls.Add(this.labelPowerChangeDelay);
      this.Controls.Add(this.listViewCommands);
      this.Controls.Add(this.numericUpDownCommandDelay);
      this.Controls.Add(this.labelCommandDelay);
      this.Controls.Add(this.labelName);
      this.Controls.Add(this.textBoxName);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOkay);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MinimumSize = new System.Drawing.Size(200, 150);
      this.Name = "LearnSetTopBox";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Learn Set Top Box";
      this.Shown += new System.EventHandler(this.Learn_Shown);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCommandDelay)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPowerChangeDelay)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPButton buttonOkay;
    private MPButton buttonCancel;
    private MPTextBox textBoxName;
    private MPLabel labelName;
    private MPLabel labelCommandDelay;
    private MPNumericUpDown numericUpDownCommandDelay;
    private MPListView listViewCommands;
    private MPColumnHeader columnHeaderCommand;
    private MPColumnHeader columnHeaderStatus;
    private MPNumericUpDown numericUpDownPowerChangeDelay;
    private MPLabel labelPowerChangeDelay;
    private MPLabel labelCommandDelayUnit;
    private MPLabel labelPowerChangeDelayUnit;
    private MPButton buttonLearn;
    private MPButton buttonForget;
    private MPLabel labelDigitCount;
    private MPComboBox comboBoxDigitCount;
  }
}