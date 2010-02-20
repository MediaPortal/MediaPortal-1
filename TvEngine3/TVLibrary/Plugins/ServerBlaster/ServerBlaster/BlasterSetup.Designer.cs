namespace SetupTv.Sections
{
  partial class BlasterSetup
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    override protected void Dispose(bool disposing)
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
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpLabelAdditionalNotes = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkSendSelect = new System.Windows.Forms.CheckBox();
      this.checkBoxExtLog = new System.Windows.Forms.CheckBox();
      this.comboBoxBlaster2 = new System.Windows.Forms.ComboBox();
      this.comboBoxBlaster1 = new System.Windows.Forms.ComboBox();
      this.comboBoxSpeed = new System.Windows.Forms.ComboBox();
      this.labelUseBlaster2 = new System.Windows.Forms.Label();
      this.labelUseBlaster1 = new System.Windows.Forms.Label();
      this.labelBlasterSpeed = new System.Windows.Forms.Label();
      this.labelBlasterType = new System.Windows.Forms.Label();
      this.comboBoxType = new System.Windows.Forms.ComboBox();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.AccessibleRole = System.Windows.Forms.AccessibleRole.HelpBalloon;
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Location = new System.Drawing.Point(3, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(476, 308);
      this.tabControl1.TabIndex = 8;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpLabelAdditionalNotes);
      this.tabPage1.Controls.Add(this.checkSendSelect);
      this.tabPage1.Controls.Add(this.checkBoxExtLog);
      this.tabPage1.Controls.Add(this.comboBoxBlaster2);
      this.tabPage1.Controls.Add(this.comboBoxBlaster1);
      this.tabPage1.Controls.Add(this.comboBoxSpeed);
      this.tabPage1.Controls.Add(this.labelUseBlaster2);
      this.tabPage1.Controls.Add(this.labelUseBlaster1);
      this.tabPage1.Controls.Add(this.labelBlasterSpeed);
      this.tabPage1.Controls.Add(this.labelBlasterType);
      this.tabPage1.Controls.Add(this.comboBoxType);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(468, 282);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "General Setup";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpLabelAdditionalNotes
      // 
      this.mpLabelAdditionalNotes.Location = new System.Drawing.Point(130, 85);
      this.mpLabelAdditionalNotes.Name = "mpLabelAdditionalNotes";
      this.mpLabelAdditionalNotes.Size = new System.Drawing.Size(251, 43);
      this.mpLabelAdditionalNotes.TabIndex = 20;
      this.mpLabelAdditionalNotes.Text = "Additional notes";
      // 
      // checkSendSelect
      // 
      this.checkSendSelect.AutoSize = true;
      this.checkSendSelect.Location = new System.Drawing.Point(20, 195);
      this.checkSendSelect.Name = "checkSendSelect";
      this.checkSendSelect.Size = new System.Drawing.Size(203, 17);
      this.checkSendSelect.TabIndex = 19;
      this.checkSendSelect.Text = "Send SELECT after channel numbers";
      this.checkSendSelect.UseVisualStyleBackColor = true;
      // 
      // checkBoxExtLog
      // 
      this.checkBoxExtLog.AutoSize = true;
      this.checkBoxExtLog.Location = new System.Drawing.Point(20, 172);
      this.checkBoxExtLog.Name = "checkBoxExtLog";
      this.checkBoxExtLog.Size = new System.Drawing.Size(129, 17);
      this.checkBoxExtLog.TabIndex = 18;
      this.checkBoxExtLog.Text = "Use extended logging";
      this.checkBoxExtLog.UseVisualStyleBackColor = true;
      // 
      // comboBoxBlaster2
      // 
      this.comboBoxBlaster2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxBlaster2.FormattingEnabled = true;
      this.comboBoxBlaster2.Location = new System.Drawing.Point(133, 134);
      this.comboBoxBlaster2.Name = "comboBoxBlaster2";
      this.comboBoxBlaster2.Size = new System.Drawing.Size(210, 21);
      this.comboBoxBlaster2.TabIndex = 17;
      // 
      // comboBoxBlaster1
      // 
      this.comboBoxBlaster1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxBlaster1.FormattingEnabled = true;
      this.comboBoxBlaster1.Location = new System.Drawing.Point(133, 107);
      this.comboBoxBlaster1.Name = "comboBoxBlaster1";
      this.comboBoxBlaster1.Size = new System.Drawing.Size(210, 21);
      this.comboBoxBlaster1.TabIndex = 16;
      // 
      // comboBoxSpeed
      // 
      this.comboBoxSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSpeed.FormattingEnabled = true;
      this.comboBoxSpeed.Items.AddRange(new object[] {
            "Fast",
            "Medium",
            "Slow"});
      this.comboBoxSpeed.Location = new System.Drawing.Point(133, 82);
      this.comboBoxSpeed.Name = "comboBoxSpeed";
      this.comboBoxSpeed.Size = new System.Drawing.Size(121, 21);
      this.comboBoxSpeed.TabIndex = 13;
      // 
      // labelUseBlaster2
      // 
      this.labelUseBlaster2.AutoSize = true;
      this.labelUseBlaster2.Location = new System.Drawing.Point(17, 137);
      this.labelUseBlaster2.Name = "labelUseBlaster2";
      this.labelUseBlaster2.Size = new System.Drawing.Size(103, 13);
      this.labelUseBlaster2.TabIndex = 12;
      this.labelUseBlaster2.Text = "Card Using Blaster 2";
      // 
      // labelUseBlaster1
      // 
      this.labelUseBlaster1.AutoSize = true;
      this.labelUseBlaster1.Location = new System.Drawing.Point(17, 110);
      this.labelUseBlaster1.Name = "labelUseBlaster1";
      this.labelUseBlaster1.Size = new System.Drawing.Size(103, 13);
      this.labelUseBlaster1.TabIndex = 11;
      this.labelUseBlaster1.Text = "Card Using Blaster 1";
      // 
      // labelBlasterSpeed
      // 
      this.labelBlasterSpeed.AutoSize = true;
      this.labelBlasterSpeed.Location = new System.Drawing.Point(17, 85);
      this.labelBlasterSpeed.Name = "labelBlasterSpeed";
      this.labelBlasterSpeed.Size = new System.Drawing.Size(73, 13);
      this.labelBlasterSpeed.TabIndex = 10;
      this.labelBlasterSpeed.Text = "Blaster Speed";
      // 
      // labelBlasterType
      // 
      this.labelBlasterType.AutoSize = true;
      this.labelBlasterType.Location = new System.Drawing.Point(17, 58);
      this.labelBlasterType.Name = "labelBlasterType";
      this.labelBlasterType.Size = new System.Drawing.Size(66, 13);
      this.labelBlasterType.TabIndex = 9;
      this.labelBlasterType.Text = "Blaster Type";
      // 
      // comboBoxType
      // 
      this.comboBoxType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxType.FormattingEnabled = true;
      this.comboBoxType.Items.AddRange(new object[] {
            "Microsoft ",
            "SMK",
            "Hauppauge"});
      this.comboBoxType.Location = new System.Drawing.Point(133, 55);
      this.comboBoxType.Name = "comboBoxType";
      this.comboBoxType.Size = new System.Drawing.Size(236, 21);
      this.comboBoxType.TabIndex = 8;
      this.comboBoxType.SelectedIndexChanged += new System.EventHandler(this.ComboBox1SelectedIndexChanged);
      // 
      // BlasterSetup
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "BlasterSetup";
      this.Size = new System.Drawing.Size(483, 316);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.ResumeLayout(false);

    }
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelAdditionalNotes;

    #endregion

    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.ComboBox comboBoxSpeed;
    private System.Windows.Forms.Label labelUseBlaster2;
    private System.Windows.Forms.Label labelUseBlaster1;
    private System.Windows.Forms.Label labelBlasterSpeed;
    private System.Windows.Forms.Label labelBlasterType;
    private System.Windows.Forms.ComboBox comboBoxType;
    private System.Windows.Forms.ComboBox comboBoxBlaster1;
    private System.Windows.Forms.ComboBox comboBoxBlaster2;
    private System.Windows.Forms.CheckBox checkBoxExtLog;
    private System.Windows.Forms.CheckBox checkSendSelect;

  }
}