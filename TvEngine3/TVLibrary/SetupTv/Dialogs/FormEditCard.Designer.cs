namespace SetupTv.Sections
{
  partial class FormEditCard
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
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.numericUpDownDecryptLimit = new System.Windows.Forms.NumericUpDown();
      this.mpButtonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.ComboBoxCamType = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkBoxCAMenabled = new System.Windows.Forms.CheckBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.checkBoxAllowEpgGrab = new System.Windows.Forms.CheckBox();
      this.checkBoxPreloadCard = new System.Windows.Forms.CheckBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDecryptLimit)).BeginInit();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(15, 57);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(289, 58);
      this.label1.TabIndex = 0;
      this.label1.Text = "If your card has a CAM module then specify the number of channels this CAM can de" +
          "code simultaneously. \r\n\r\nSetting the value to 0 will disable the limit completel" +
          "y.";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(15, 125);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(111, 13);
      this.label3.TabIndex = 2;
      this.label3.Text = "This card can decode";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(176, 125);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(121, 13);
      this.label4.TabIndex = 4;
      this.label4.Text = "channels simultaneously";
      // 
      // mpButtonSave
      // 
      this.mpButtonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonSave.Location = new System.Drawing.Point(166, 316);
      this.mpButtonSave.Name = "mpButtonSave";
      this.mpButtonSave.Size = new System.Drawing.Size(75, 23);
      this.mpButtonSave.TabIndex = 1;
      this.mpButtonSave.Text = "Save";
      this.mpButtonSave.UseVisualStyleBackColor = true;
      this.mpButtonSave.Click += new System.EventHandler(this.mpButtonSave_Click);
      // 
      // numericUpDownDecryptLimit
      // 
      this.numericUpDownDecryptLimit.Location = new System.Drawing.Point(132, 123);
      this.numericUpDownDecryptLimit.Name = "numericUpDownDecryptLimit";
      this.numericUpDownDecryptLimit.Size = new System.Drawing.Size(38, 20);
      this.numericUpDownDecryptLimit.TabIndex = 5;
      this.numericUpDownDecryptLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownDecryptLimit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(247, 316);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 6;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Click += new System.EventHandler(this.mpButtonCancel_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.ComboBoxCamType);
      this.groupBox1.Controls.Add(this.checkBoxCAMenabled);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.numericUpDownDecryptLimit);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(310, 189);
      this.groupBox1.TabIndex = 7;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "CAM Setup";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(59, 152);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(67, 13);
      this.label5.TabIndex = 8;
      this.label5.Text = "CAM model :";
      // 
      // ComboBoxCamType
      // 
      this.ComboBoxCamType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.ComboBoxCamType.FormattingEnabled = true;
      this.ComboBoxCamType.Items.AddRange(new object[] {
            "default",
            "Astoncrypt 2"});
      this.ComboBoxCamType.Location = new System.Drawing.Point(132, 149);
      this.ComboBoxCamType.Name = "ComboBoxCamType";
      this.ComboBoxCamType.Size = new System.Drawing.Size(103, 21);
      this.ComboBoxCamType.TabIndex = 7;
      // 
      // checkBoxCAMenabled
      // 
      this.checkBoxCAMenabled.AutoSize = true;
      this.checkBoxCAMenabled.Location = new System.Drawing.Point(18, 28);
      this.checkBoxCAMenabled.Name = "checkBoxCAMenabled";
      this.checkBoxCAMenabled.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxCAMenabled.Size = new System.Drawing.Size(207, 17);
      this.checkBoxCAMenabled.TabIndex = 6;
      this.checkBoxCAMenabled.Text = "CAM enabled and present for this card";
      this.checkBoxCAMenabled.UseVisualStyleBackColor = true;
      this.checkBoxCAMenabled.CheckedChanged += new System.EventHandler(this.checkBoxCAMenabled_CheckedChanged);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.checkBoxAllowEpgGrab);
      this.groupBox2.Location = new System.Drawing.Point(12, 207);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(310, 44);
      this.groupBox2.TabIndex = 8;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Grab EPG";
      // 
      // checkBoxAllowEpgGrab
      // 
      this.checkBoxAllowEpgGrab.AutoSize = true;
      this.checkBoxAllowEpgGrab.Location = new System.Drawing.Point(18, 21);
      this.checkBoxAllowEpgGrab.Name = "checkBoxAllowEpgGrab";
      this.checkBoxAllowEpgGrab.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxAllowEpgGrab.Size = new System.Drawing.Size(231, 17);
      this.checkBoxAllowEpgGrab.TabIndex = 0;
      this.checkBoxAllowEpgGrab.Text = "Allow this card to be used for EPG grabbing";
      this.checkBoxAllowEpgGrab.UseVisualStyleBackColor = true;
      // 
      // checkBoxPreloadCard
      // 
      this.checkBoxPreloadCard.AutoSize = true;
      this.checkBoxPreloadCard.Location = new System.Drawing.Point(18, 21);
      this.checkBoxPreloadCard.Name = "checkBoxPreloadCard";
      this.checkBoxPreloadCard.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.checkBoxPreloadCard.Size = new System.Drawing.Size(171, 17);
      this.checkBoxPreloadCard.TabIndex = 0;
      this.checkBoxPreloadCard.Text = "Allow this card to be preloaded";
      this.checkBoxPreloadCard.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.checkBoxPreloadCard);
      this.groupBox3.Location = new System.Drawing.Point(12, 257);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(310, 44);
      this.groupBox3.TabIndex = 9;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Preload Card";
      // 
      // FormEditCard
      // 
      this.AcceptButton = this.mpButtonSave;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(334, 351);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonSave);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormEditCard";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Edit card properties";
      this.Load += new System.EventHandler(this.FormEditCard_Load);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDecryptLimit)).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonSave;
    private System.Windows.Forms.NumericUpDown numericUpDownDecryptLimit;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonCancel;
      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.GroupBox groupBox2;
      private System.Windows.Forms.CheckBox checkBoxAllowEpgGrab;
    private System.Windows.Forms.CheckBox checkBoxPreloadCard;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.CheckBox checkBoxCAMenabled;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPComboBox ComboBoxCamType;
  }
}
