using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormLNBType
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
      this.mpButtonOK = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.mpButtonCancel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.groupBoxGeneralSettings = new System.Windows.Forms.GroupBox();
      this.mpComboBox22KhzControl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.mpLabel4 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpRadioButtonSingle = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.mpRadioButtonBand = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.checkBoxTorodial = new System.Windows.Forms.CheckBox();
      this.label6 = new System.Windows.Forms.Label();
      this.mpRadioButtonDual = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.label2 = new System.Windows.Forms.Label();
      this.txtFreq1 = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.txtFreq2 = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.txtSwitchFreq = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label47 = new System.Windows.Forms.Label();
      this.txtLnbTypeName = new System.Windows.Forms.TextBox();
      this.groupBoxGeneralSettings.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpButtonOK
      // 
      this.mpButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonOK.Location = new System.Drawing.Point(187, 266);
      this.mpButtonOK.Name = "mpButtonOK";
      this.mpButtonOK.Size = new System.Drawing.Size(75, 25);
      this.mpButtonOK.TabIndex = 1;
      this.mpButtonOK.Text = "OK";
      this.mpButtonOK.UseVisualStyleBackColor = true;
      this.mpButtonOK.Click += new System.EventHandler(this.button1_Click);
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(268, 266);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 25);
      this.mpButtonCancel.TabIndex = 2;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Click += new System.EventHandler(this.button2_Click);
      // 
      // groupBoxGeneralSettings
      // 
      this.groupBoxGeneralSettings.Controls.Add(this.mpComboBox22KhzControl);
      this.groupBoxGeneralSettings.Controls.Add(this.mpLabel4);
      this.groupBoxGeneralSettings.Controls.Add(this.mpRadioButtonSingle);
      this.groupBoxGeneralSettings.Controls.Add(this.mpRadioButtonBand);
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxTorodial);
      this.groupBoxGeneralSettings.Controls.Add(this.label6);
      this.groupBoxGeneralSettings.Controls.Add(this.mpRadioButtonDual);
      this.groupBoxGeneralSettings.Controls.Add(this.label2);
      this.groupBoxGeneralSettings.Controls.Add(this.txtFreq1);
      this.groupBoxGeneralSettings.Controls.Add(this.label5);
      this.groupBoxGeneralSettings.Controls.Add(this.label4);
      this.groupBoxGeneralSettings.Controls.Add(this.txtFreq2);
      this.groupBoxGeneralSettings.Controls.Add(this.label3);
      this.groupBoxGeneralSettings.Controls.Add(this.txtSwitchFreq);
      this.groupBoxGeneralSettings.Controls.Add(this.label1);
      this.groupBoxGeneralSettings.Location = new System.Drawing.Point(12, 32);
      this.groupBoxGeneralSettings.Name = "groupBoxGeneralSettings";
      this.groupBoxGeneralSettings.Size = new System.Drawing.Size(330, 227);
      this.groupBoxGeneralSettings.TabIndex = 122;
      this.groupBoxGeneralSettings.TabStop = false;
      this.groupBoxGeneralSettings.Text = "Frequencies";
      // 
      // mpComboBox22KhzControl
      // 
      this.mpComboBox22KhzControl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBox22KhzControl.FormattingEnabled = true;
      this.mpComboBox22KhzControl.Items.AddRange(new object[] {
            "Auto",
            "Off",
            "On"});
      this.mpComboBox22KhzControl.Location = new System.Drawing.Point(105, 172);
      this.mpComboBox22KhzControl.Name = "mpComboBox22KhzControl";
      this.mpComboBox22KhzControl.Size = new System.Drawing.Size(103, 21);
      this.mpComboBox22KhzControl.TabIndex = 126;
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(13, 175);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(81, 13);
      this.mpLabel4.TabIndex = 125;
      this.mpLabel4.Text = "22 KHz Control:";
      // 
      // mpRadioButtonSingle
      // 
      this.mpRadioButtonSingle.AutoSize = true;
      this.mpRadioButtonSingle.Checked = true;
      this.mpRadioButtonSingle.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRadioButtonSingle.Location = new System.Drawing.Point(19, 19);
      this.mpRadioButtonSingle.Name = "mpRadioButtonSingle";
      this.mpRadioButtonSingle.Size = new System.Drawing.Size(53, 17);
      this.mpRadioButtonSingle.TabIndex = 129;
      this.mpRadioButtonSingle.TabStop = true;
      this.mpRadioButtonSingle.Text = "Single";
      this.mpRadioButtonSingle.UseVisualStyleBackColor = true;
      this.mpRadioButtonSingle.CheckedChanged += new System.EventHandler(this.mpRadioButtonSingle_CheckedChanged);
      // 
      // mpRadioButtonBand
      // 
      this.mpRadioButtonBand.AutoSize = true;
      this.mpRadioButtonBand.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRadioButtonBand.Location = new System.Drawing.Point(19, 66);
      this.mpRadioButtonBand.Name = "mpRadioButtonBand";
      this.mpRadioButtonBand.Size = new System.Drawing.Size(93, 17);
      this.mpRadioButtonBand.TabIndex = 128;
      this.mpRadioButtonBand.Text = "Band stacked ";
      this.mpRadioButtonBand.UseVisualStyleBackColor = true;
      this.mpRadioButtonBand.CheckedChanged += new System.EventHandler(this.mpRadioButtonBand_CheckedChanged);
      // 
      // checkBoxTorodial
      // 
      this.checkBoxTorodial.AutoSize = true;
      this.checkBoxTorodial.Location = new System.Drawing.Point(16, 200);
      this.checkBoxTorodial.Name = "checkBoxTorodial";
      this.checkBoxTorodial.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxTorodial.Size = new System.Drawing.Size(70, 17);
      this.checkBoxTorodial.TabIndex = 133;
      this.checkBoxTorodial.Text = "Toroidal?";
      this.checkBoxTorodial.UseVisualStyleBackColor = true;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(154, 153);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(29, 13);
      this.label6.TabIndex = 136;
      this.label6.Text = "MHz";
      // 
      // mpRadioButtonDual
      // 
      this.mpRadioButtonDual.AutoSize = true;
      this.mpRadioButtonDual.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRadioButtonDual.Location = new System.Drawing.Point(19, 43);
      this.mpRadioButtonDual.Name = "mpRadioButtonDual";
      this.mpRadioButtonDual.Size = new System.Drawing.Size(46, 17);
      this.mpRadioButtonDual.TabIndex = 127;
      this.mpRadioButtonDual.Text = "Dual";
      this.mpRadioButtonDual.UseVisualStyleBackColor = true;
      this.mpRadioButtonDual.CheckedChanged += new System.EventHandler(this.mpRadioButtonDual_CheckedChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(15, 97);
      this.label2.Name = "label2";
      this.label2.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.label2.Size = new System.Drawing.Size(69, 13);
      this.label2.TabIndex = 127;
      this.label2.Text = "Frequency 1:";
      // 
      // txtFreq1
      // 
      this.txtFreq1.Location = new System.Drawing.Point(105, 94);
      this.txtFreq1.Name = "txtFreq1";
      this.txtFreq1.Size = new System.Drawing.Size(43, 20);
      this.txtFreq1.TabIndex = 126;
      this.txtFreq1.Text = "0";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(154, 123);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(29, 13);
      this.label5.TabIndex = 135;
      this.label5.Text = "MHz";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(154, 97);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(29, 13);
      this.label4.TabIndex = 134;
      this.label4.Text = "MHz";
      // 
      // txtFreq2
      // 
      this.txtFreq2.Location = new System.Drawing.Point(105, 120);
      this.txtFreq2.Name = "txtFreq2";
      this.txtFreq2.Size = new System.Drawing.Size(43, 20);
      this.txtFreq2.TabIndex = 128;
      this.txtFreq2.Text = "0";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(15, 123);
      this.label3.Name = "label3";
      this.label3.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.label3.Size = new System.Drawing.Size(69, 13);
      this.label3.TabIndex = 129;
      this.label3.Text = "Frequency 2:";
      // 
      // txtSwitchFreq
      // 
      this.txtSwitchFreq.Location = new System.Drawing.Point(105, 146);
      this.txtSwitchFreq.Name = "txtSwitchFreq";
      this.txtSwitchFreq.Size = new System.Drawing.Size(43, 20);
      this.txtSwitchFreq.TabIndex = 122;
      this.txtSwitchFreq.Text = "0";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(15, 149);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(92, 13);
      this.label1.TabIndex = 123;
      this.label1.Text = "Switch frequency:";
      this.label1.Click += new System.EventHandler(this.label1_Click);
      // 
      // label47
      // 
      this.label47.AutoSize = true;
      this.label47.Location = new System.Drawing.Point(12, 9);
      this.label47.Name = "label47";
      this.label47.Size = new System.Drawing.Size(38, 13);
      this.label47.TabIndex = 124;
      this.label47.Text = "Name:";
      // 
      // txtLnbTypeName
      // 
      this.txtLnbTypeName.Location = new System.Drawing.Point(99, 6);
      this.txtLnbTypeName.Name = "txtLnbTypeName";
      this.txtLnbTypeName.Size = new System.Drawing.Size(244, 20);
      this.txtLnbTypeName.TabIndex = 123;
      // 
      // FormLNBType
      // 
      this.AcceptButton = this.mpButtonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(365, 322);
      this.Controls.Add(this.label47);
      this.Controls.Add(this.txtLnbTypeName);
      this.Controls.Add(this.groupBoxGeneralSettings);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormLNBType";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Edit LNB Type: <XYZ>";
      this.Load += new System.EventHandler(this.FormLNBType_Load);
      this.groupBoxGeneralSettings.ResumeLayout(false);
      this.groupBoxGeneralSettings.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPButton mpButtonOK;
    private MPButton mpButtonCancel;
    private System.Windows.Forms.GroupBox groupBoxGeneralSettings;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox txtSwitchFreq;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox txtFreq1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox txtFreq2;
    private System.Windows.Forms.CheckBox checkBoxTorodial;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label47;
    private System.Windows.Forms.TextBox txtLnbTypeName;
    private MPComboBox mpComboBox22KhzControl;
    private MPLabel mpLabel4;
    private MPRadioButton mpRadioButtonSingle;
    private MPRadioButton mpRadioButtonBand;
    private MPRadioButton mpRadioButtonDual;
  }
}