namespace SetupTv.Sections
{
  partial class Epg
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.mpGroupBoxEpg = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBox7 = new System.Windows.Forms.GroupBox();
      this.checkBoxAlwaysUpdate = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxAlwaysFillHoles = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox5 = new System.Windows.Forms.GroupBox();
      this.numericUpDownTSEpgTimeout = new System.Windows.Forms.NumericUpDown();
      this.checkBoxEnableEpgWhileTimeshifting = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label22 = new System.Windows.Forms.Label();
      this.label23 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.numericUpDownEpgRefresh = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownEpgTimeOut = new System.Windows.Forms.NumericUpDown();
      this.checkBoxEnableEPGWhileIdle = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label15 = new System.Windows.Forms.Label();
      this.label14 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.groupBox9 = new System.Windows.Forms.GroupBox();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.edTitleTemplate = new System.Windows.Forms.TextBox();
      this.label27 = new System.Windows.Forms.Label();
      this.label28 = new System.Windows.Forms.Label();
      this.label38 = new System.Windows.Forms.Label();
      this.edDescriptionTemplate = new System.Windows.Forms.TextBox();
      this.label30 = new System.Windows.Forms.Label();
      this.edTitleTest = new System.Windows.Forms.TextBox();
      this.label29 = new System.Windows.Forms.Label();
      this.edDescriptionTest = new System.Windows.Forms.TextBox();
      this.btnTest = new System.Windows.Forms.Button();
      this.mpGroupBoxEpg.SuspendLayout();
      this.groupBox7.SuspendLayout();
      this.groupBox5.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTSEpgTimeout)).BeginInit();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpgRefresh)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpgTimeOut)).BeginInit();
      this.groupBox9.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBoxEpg
      // 
      this.mpGroupBoxEpg.Controls.Add(this.groupBox9);
      this.mpGroupBoxEpg.Controls.Add(this.groupBox2);
      this.mpGroupBoxEpg.Controls.Add(this.groupBox5);
      this.mpGroupBoxEpg.Controls.Add(this.groupBox7);
      this.mpGroupBoxEpg.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxEpg.Location = new System.Drawing.Point(3, 3);
      this.mpGroupBoxEpg.Name = "mpGroupBoxEpg";
      this.mpGroupBoxEpg.Size = new System.Drawing.Size(468, 411);
      this.mpGroupBoxEpg.TabIndex = 0;
      this.mpGroupBoxEpg.TabStop = false;
      this.mpGroupBoxEpg.Text = "Main settings";
      // 
      // groupBox7
      // 
      this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox7.Controls.Add(this.checkBoxAlwaysUpdate);
      this.groupBox7.Controls.Add(this.checkBoxAlwaysFillHoles);
      this.groupBox7.Location = new System.Drawing.Point(6, 19);
      this.groupBox7.Name = "groupBox7";
      this.groupBox7.Size = new System.Drawing.Size(454, 48);
      this.groupBox7.TabIndex = 38;
      this.groupBox7.TabStop = false;
      this.groupBox7.Text = "General";
      // 
      // checkBoxAlwaysUpdate
      // 
      this.checkBoxAlwaysUpdate.AutoSize = true;
      this.checkBoxAlwaysUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAlwaysUpdate.Location = new System.Drawing.Point(140, 19);
      this.checkBoxAlwaysUpdate.Name = "checkBoxAlwaysUpdate";
      this.checkBoxAlwaysUpdate.Size = new System.Drawing.Size(310, 17);
      this.checkBoxAlwaysUpdate.TabIndex = 11;
      this.checkBoxAlwaysUpdate.Text = "Always try to update existing entries (might raise CPU usage!)";
      this.checkBoxAlwaysUpdate.UseVisualStyleBackColor = true;
      // 
      // checkBoxAlwaysFillHoles
      // 
      this.checkBoxAlwaysFillHoles.AutoSize = true;
      this.checkBoxAlwaysFillHoles.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAlwaysFillHoles.Location = new System.Drawing.Point(11, 19);
      this.checkBoxAlwaysFillHoles.Name = "checkBoxAlwaysFillHoles";
      this.checkBoxAlwaysFillHoles.Size = new System.Drawing.Size(123, 17);
      this.checkBoxAlwaysFillHoles.TabIndex = 9;
      this.checkBoxAlwaysFillHoles.Text = "Always try to fill holes";
      this.checkBoxAlwaysFillHoles.UseVisualStyleBackColor = true;
      // 
      // groupBox5
      // 
      this.groupBox5.Controls.Add(this.numericUpDownTSEpgTimeout);
      this.groupBox5.Controls.Add(this.checkBoxEnableEpgWhileTimeshifting);
      this.groupBox5.Controls.Add(this.label22);
      this.groupBox5.Controls.Add(this.label23);
      this.groupBox5.Location = new System.Drawing.Point(6, 73);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(226, 98);
      this.groupBox5.TabIndex = 39;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "EPG grabbing while timeshifting/recording";
      // 
      // numericUpDownTSEpgTimeout
      // 
      this.numericUpDownTSEpgTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.numericUpDownTSEpgTimeout.Location = new System.Drawing.Point(70, 42);
      this.numericUpDownTSEpgTimeout.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownTSEpgTimeout.Name = "numericUpDownTSEpgTimeout";
      this.numericUpDownTSEpgTimeout.Size = new System.Drawing.Size(86, 20);
      this.numericUpDownTSEpgTimeout.TabIndex = 10;
      this.numericUpDownTSEpgTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownTSEpgTimeout.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
      // 
      // checkBoxEnableEpgWhileTimeshifting
      // 
      this.checkBoxEnableEpgWhileTimeshifting.AutoSize = true;
      this.checkBoxEnableEpgWhileTimeshifting.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxEnableEpgWhileTimeshifting.Location = new System.Drawing.Point(11, 19);
      this.checkBoxEnableEpgWhileTimeshifting.Name = "checkBoxEnableEpgWhileTimeshifting";
      this.checkBoxEnableEpgWhileTimeshifting.Size = new System.Drawing.Size(63, 17);
      this.checkBoxEnableEpgWhileTimeshifting.TabIndex = 9;
      this.checkBoxEnableEpgWhileTimeshifting.Text = "Enabled";
      this.checkBoxEnableEpgWhileTimeshifting.UseVisualStyleBackColor = true;
      // 
      // label22
      // 
      this.label22.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label22.AutoSize = true;
      this.label22.Location = new System.Drawing.Point(173, 42);
      this.label22.Name = "label22";
      this.label22.Size = new System.Drawing.Size(28, 13);
      this.label22.TabIndex = 7;
      this.label22.Text = "mins";
      // 
      // label23
      // 
      this.label23.AutoSize = true;
      this.label23.Location = new System.Drawing.Point(6, 44);
      this.label23.Name = "label23";
      this.label23.Size = new System.Drawing.Size(48, 13);
      this.label23.TabIndex = 5;
      this.label23.Text = "Timeout:";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.numericUpDownEpgRefresh);
      this.groupBox2.Controls.Add(this.numericUpDownEpgTimeOut);
      this.groupBox2.Controls.Add(this.checkBoxEnableEPGWhileIdle);
      this.groupBox2.Controls.Add(this.label15);
      this.groupBox2.Controls.Add(this.label14);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Location = new System.Drawing.Point(238, 73);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(219, 98);
      this.groupBox2.TabIndex = 40;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "EPG grabbing while idle";
      // 
      // numericUpDownEpgRefresh
      // 
      this.numericUpDownEpgRefresh.Location = new System.Drawing.Point(86, 68);
      this.numericUpDownEpgRefresh.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownEpgRefresh.Name = "numericUpDownEpgRefresh";
      this.numericUpDownEpgRefresh.Size = new System.Drawing.Size(85, 20);
      this.numericUpDownEpgRefresh.TabIndex = 10;
      this.numericUpDownEpgRefresh.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownEpgRefresh.Value = new decimal(new int[] {
            240,
            0,
            0,
            0});
      // 
      // numericUpDownEpgTimeOut
      // 
      this.numericUpDownEpgTimeOut.Location = new System.Drawing.Point(86, 42);
      this.numericUpDownEpgTimeOut.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownEpgTimeOut.Name = "numericUpDownEpgTimeOut";
      this.numericUpDownEpgTimeOut.Size = new System.Drawing.Size(85, 20);
      this.numericUpDownEpgTimeOut.TabIndex = 10;
      this.numericUpDownEpgTimeOut.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownEpgTimeOut.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      // 
      // checkBoxEnableEPGWhileIdle
      // 
      this.checkBoxEnableEPGWhileIdle.AutoSize = true;
      this.checkBoxEnableEPGWhileIdle.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxEnableEPGWhileIdle.Location = new System.Drawing.Point(10, 19);
      this.checkBoxEnableEPGWhileIdle.Name = "checkBoxEnableEPGWhileIdle";
      this.checkBoxEnableEPGWhileIdle.Size = new System.Drawing.Size(63, 17);
      this.checkBoxEnableEPGWhileIdle.TabIndex = 11;
      this.checkBoxEnableEPGWhileIdle.Text = "Enabled";
      this.checkBoxEnableEPGWhileIdle.UseVisualStyleBackColor = true;
      // 
      // label15
      // 
      this.label15.AutoSize = true;
      this.label15.Location = new System.Drawing.Point(177, 68);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(28, 13);
      this.label15.TabIndex = 7;
      this.label15.Text = "mins";
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Location = new System.Drawing.Point(7, 70);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(73, 13);
      this.label14.TabIndex = 5;
      this.label14.Text = "Refresh every";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(177, 44);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(28, 13);
      this.label8.TabIndex = 4;
      this.label8.Text = "mins";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(7, 44);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(48, 13);
      this.label7.TabIndex = 2;
      this.label7.Text = "Timeout:";
      // 
      // groupBox9
      // 
      this.groupBox9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox9.Controls.Add(this.textBox1);
      this.groupBox9.Controls.Add(this.edTitleTemplate);
      this.groupBox9.Controls.Add(this.label27);
      this.groupBox9.Controls.Add(this.label28);
      this.groupBox9.Controls.Add(this.label38);
      this.groupBox9.Controls.Add(this.edDescriptionTemplate);
      this.groupBox9.Controls.Add(this.label30);
      this.groupBox9.Controls.Add(this.edTitleTest);
      this.groupBox9.Controls.Add(this.label29);
      this.groupBox9.Controls.Add(this.edDescriptionTest);
      this.groupBox9.Controls.Add(this.btnTest);
      this.groupBox9.Location = new System.Drawing.Point(6, 177);
      this.groupBox9.Name = "groupBox9";
      this.groupBox9.Size = new System.Drawing.Size(454, 228);
      this.groupBox9.TabIndex = 41;
      this.groupBox9.TabStop = false;
      this.groupBox9.Text = "Display options";
      // 
      // textBox1
      // 
      this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.textBox1.Location = new System.Drawing.Point(323, 80);
      this.textBox1.Multiline = true;
      this.textBox1.Name = "textBox1";
      this.textBox1.ReadOnly = true;
      this.textBox1.Size = new System.Drawing.Size(125, 142);
      this.textBox1.TabIndex = 37;
      this.textBox1.Text = "%TITLE%\r\n%DESCRIPTION%\r\n%GENRE%\r\n%STARRATING%\r\n%STARRATING_STR%\r\n%CLASSIFICATION%" +
          "\r\n%PARENTALRATING%\r\n%NEWLINE%";
      this.textBox1.WordWrap = false;
      // 
      // edTitleTemplate
      // 
      this.edTitleTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edTitleTemplate.Location = new System.Drawing.Point(75, 19);
      this.edTitleTemplate.Name = "edTitleTemplate";
      this.edTitleTemplate.Size = new System.Drawing.Size(242, 20);
      this.edTitleTemplate.TabIndex = 20;
      // 
      // label27
      // 
      this.label27.AutoSize = true;
      this.label27.Location = new System.Drawing.Point(6, 22);
      this.label27.Name = "label27";
      this.label27.Size = new System.Drawing.Size(30, 13);
      this.label27.TabIndex = 19;
      this.label27.Text = "Title:";
      // 
      // label28
      // 
      this.label28.AutoSize = true;
      this.label28.Location = new System.Drawing.Point(6, 48);
      this.label28.Name = "label28";
      this.label28.Size = new System.Drawing.Size(63, 13);
      this.label28.TabIndex = 21;
      this.label28.Text = "Description:";
      // 
      // label38
      // 
      this.label38.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label38.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label38.Location = new System.Drawing.Point(323, 16);
      this.label38.Name = "label38";
      this.label38.Size = new System.Drawing.Size(125, 61);
      this.label38.TabIndex = 35;
      this.label38.Text = "You can use any combination of the placeholders shown below";
      // 
      // edDescriptionTemplate
      // 
      this.edDescriptionTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edDescriptionTemplate.Location = new System.Drawing.Point(75, 45);
      this.edDescriptionTemplate.Name = "edDescriptionTemplate";
      this.edDescriptionTemplate.Size = new System.Drawing.Size(242, 20);
      this.edDescriptionTemplate.TabIndex = 22;
      // 
      // label30
      // 
      this.label30.AutoSize = true;
      this.label30.Location = new System.Drawing.Point(6, 103);
      this.label30.Name = "label30";
      this.label30.Size = new System.Drawing.Size(30, 13);
      this.label30.TabIndex = 23;
      this.label30.Text = "Title:";
      // 
      // edTitleTest
      // 
      this.edTitleTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edTitleTest.Location = new System.Drawing.Point(75, 100);
      this.edTitleTest.Name = "edTitleTest";
      this.edTitleTest.ReadOnly = true;
      this.edTitleTest.Size = new System.Drawing.Size(242, 20);
      this.edTitleTest.TabIndex = 24;
      // 
      // label29
      // 
      this.label29.AutoSize = true;
      this.label29.Location = new System.Drawing.Point(6, 129);
      this.label29.Name = "label29";
      this.label29.Size = new System.Drawing.Size(63, 13);
      this.label29.TabIndex = 25;
      this.label29.Text = "Description:";
      // 
      // edDescriptionTest
      // 
      this.edDescriptionTest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edDescriptionTest.Location = new System.Drawing.Point(75, 126);
      this.edDescriptionTest.Multiline = true;
      this.edDescriptionTest.Name = "edDescriptionTest";
      this.edDescriptionTest.ReadOnly = true;
      this.edDescriptionTest.Size = new System.Drawing.Size(242, 96);
      this.edDescriptionTest.TabIndex = 26;
      // 
      // btnTest
      // 
      this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnTest.Location = new System.Drawing.Point(242, 71);
      this.btnTest.Name = "btnTest";
      this.btnTest.Size = new System.Drawing.Size(75, 23);
      this.btnTest.TabIndex = 27;
      this.btnTest.Text = "Test";
      this.btnTest.UseVisualStyleBackColor = true;
      this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // Epg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpGroupBoxEpg);
      this.Name = "Epg";
      this.Size = new System.Drawing.Size(474, 417);
      this.mpGroupBoxEpg.ResumeLayout(false);
      this.groupBox7.ResumeLayout(false);
      this.groupBox7.PerformLayout();
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTSEpgTimeout)).EndInit();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpgRefresh)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpgTimeOut)).EndInit();
      this.groupBox9.ResumeLayout(false);
      this.groupBox9.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBoxEpg;
    private System.Windows.Forms.GroupBox groupBox7;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAlwaysUpdate;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAlwaysFillHoles;
    private System.Windows.Forms.GroupBox groupBox5;
    private System.Windows.Forms.NumericUpDown numericUpDownTSEpgTimeout;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxEnableEpgWhileTimeshifting;
    private System.Windows.Forms.Label label22;
    private System.Windows.Forms.Label label23;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.NumericUpDown numericUpDownEpgRefresh;
    private System.Windows.Forms.NumericUpDown numericUpDownEpgTimeOut;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxEnableEPGWhileIdle;
    private System.Windows.Forms.Label label15;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.GroupBox groupBox9;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.TextBox edTitleTemplate;
    private System.Windows.Forms.Label label27;
    private System.Windows.Forms.Label label28;
    private System.Windows.Forms.Label label38;
    private System.Windows.Forms.TextBox edDescriptionTemplate;
    private System.Windows.Forms.Label label30;
    private System.Windows.Forms.TextBox edTitleTest;
    private System.Windows.Forms.Label label29;
    private System.Windows.Forms.TextBox edDescriptionTest;
    private System.Windows.Forms.Button btnTest;

  }
}
