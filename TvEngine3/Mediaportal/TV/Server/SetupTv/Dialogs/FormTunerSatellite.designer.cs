using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormTunerSatellite
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
      this.textBoxChannel = new System.Windows.Forms.TextBox();
      this.label47 = new System.Windows.Forms.Label();
      this.groupBoxGeneralSettings = new System.Windows.Forms.GroupBox();
      this.comboBoxLnb = new System.Windows.Forms.ComboBox();
      this.label42 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.comboBoxCard = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.comboBoxSatellite = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.groupBoxGeneralSettings.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpButtonOK
      // 
      this.mpButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonOK.Location = new System.Drawing.Point(189, 222);
      this.mpButtonOK.Name = "mpButtonOK";
      this.mpButtonOK.Size = new System.Drawing.Size(75, 23);
      this.mpButtonOK.TabIndex = 1;
      this.mpButtonOK.Text = "OK";
      this.mpButtonOK.UseVisualStyleBackColor = true;
      this.mpButtonOK.Click += new System.EventHandler(this.button1_Click);
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(270, 222);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 2;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Click += new System.EventHandler(this.button2_Click);
      // 
      // textBoxChannel
      // 
      this.textBoxChannel.Location = new System.Drawing.Point(85, 19);
      this.textBoxChannel.Name = "textBoxChannel";
      this.textBoxChannel.Size = new System.Drawing.Size(43, 20);
      this.textBoxChannel.TabIndex = 120;
      this.textBoxChannel.Text = "0";
      // 
      // label47
      // 
      this.label47.AutoSize = true;
      this.label47.Location = new System.Drawing.Point(8, 22);
      this.label47.Name = "label47";
      this.label47.Size = new System.Drawing.Size(76, 13);
      this.label47.TabIndex = 121;
      this.label47.Text = "Motor position:";
      // 
      // groupBoxGeneralSettings
      // 
      this.groupBoxGeneralSettings.Controls.Add(this.comboBoxLnb);
      this.groupBoxGeneralSettings.Controls.Add(this.label42);
      this.groupBoxGeneralSettings.Controls.Add(this.label1);
      this.groupBoxGeneralSettings.Controls.Add(this.textBox1);
      this.groupBoxGeneralSettings.Controls.Add(this.label47);
      this.groupBoxGeneralSettings.Controls.Add(this.textBoxChannel);
      this.groupBoxGeneralSettings.Location = new System.Drawing.Point(12, 12);
      this.groupBoxGeneralSettings.Name = "groupBoxGeneralSettings";
      this.groupBoxGeneralSettings.Size = new System.Drawing.Size(330, 110);
      this.groupBoxGeneralSettings.TabIndex = 122;
      this.groupBoxGeneralSettings.TabStop = false;
      this.groupBoxGeneralSettings.Text = "DiseqC Settings";
      // 
      // comboBoxLnb
      // 
      this.comboBoxLnb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxLnb.FormattingEnabled = true;
      this.comboBoxLnb.Location = new System.Drawing.Point(85, 71);
      this.comboBoxLnb.Name = "comboBoxLnb";
      this.comboBoxLnb.Size = new System.Drawing.Size(147, 21);
      this.comboBoxLnb.TabIndex = 124;
      // 
      // label42
      // 
      this.label42.AutoSize = true;
      this.label42.Location = new System.Drawing.Point(8, 74);
      this.label42.Name = "label42";
      this.label42.Size = new System.Drawing.Size(31, 13);
      this.label42.TabIndex = 125;
      this.label42.Text = "LNB:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(8, 48);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(76, 13);
      this.label1.TabIndex = 123;
      this.label1.Text = "Switch setting:";
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(85, 45);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(43, 20);
      this.textBox1.TabIndex = 122;
      this.textBox1.Text = "0";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.comboBoxSatellite);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.comboBoxCard);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Location = new System.Drawing.Point(12, 128);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(330, 85);
      this.groupBox1.TabIndex = 128;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "General Settings";
      // 
      // comboBoxCard
      // 
      this.comboBoxCard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCard.FormattingEnabled = true;
      this.comboBoxCard.Location = new System.Drawing.Point(85, 19);
      this.comboBoxCard.Name = "comboBoxCard";
      this.comboBoxCard.Size = new System.Drawing.Size(147, 21);
      this.comboBoxCard.TabIndex = 124;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(8, 22);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(32, 13);
      this.label3.TabIndex = 125;
      this.label3.Text = "Card:";
      // 
      // comboBoxSatellite
      // 
      this.comboBoxSatellite.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSatellite.FormattingEnabled = true;
      this.comboBoxSatellite.Location = new System.Drawing.Point(85, 46);
      this.comboBoxSatellite.Name = "comboBoxSatellite";
      this.comboBoxSatellite.Size = new System.Drawing.Size(147, 21);
      this.comboBoxSatellite.TabIndex = 126;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(8, 49);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(47, 13);
      this.label2.TabIndex = 127;
      this.label2.Text = "Satellite:";
      // 
      // FormTunerSatellite
      // 
      this.AcceptButton = this.mpButtonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(365, 257);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.groupBoxGeneralSettings);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormTunerSatellite";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Edit Tuner Satellite";
      this.Load += new System.EventHandler(this.FormTunerSatellite_Load);
      this.groupBoxGeneralSettings.ResumeLayout(false);
      this.groupBoxGeneralSettings.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MPButton mpButtonOK;
    private MPButton mpButtonCancel;
    private System.Windows.Forms.TextBox textBoxChannel;
    private System.Windows.Forms.Label label47;
    private System.Windows.Forms.GroupBox groupBoxGeneralSettings;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.ComboBox comboBoxLnb;
    private System.Windows.Forms.Label label42;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.ComboBox comboBoxCard;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox comboBoxSatellite;
    private System.Windows.Forms.Label label2;
  }
}