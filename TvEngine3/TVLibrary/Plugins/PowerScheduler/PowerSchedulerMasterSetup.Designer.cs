namespace TvEngine.PowerScheduler
{
  partial class PowerSchedulerMasterSetup
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
      this.checkBox2 = new System.Windows.Forms.CheckBox();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.checkBox4 = new System.Windows.Forms.CheckBox();
      this.label4 = new System.Windows.Forms.Label();
      this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
      this.label3 = new System.Windows.Forms.Label();
      this.comboBox1 = new System.Windows.Forms.ComboBox();
      this.checkBox3 = new System.Windows.Forms.CheckBox();
      this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
      this.label2 = new System.Windows.Forms.Label();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.checkBox5 = new System.Windows.Forms.CheckBox();
      this.checkBox6 = new System.Windows.Forms.CheckBox();
      this.checkBox7 = new System.Windows.Forms.CheckBox();
      this.checkBox8 = new System.Windows.Forms.CheckBox();
      this.checkBox9 = new System.Windows.Forms.CheckBox();
      this.checkBox10 = new System.Windows.Forms.CheckBox();
      this.checkBox11 = new System.Windows.Forms.CheckBox();
      this.checkBox12 = new System.Windows.Forms.CheckBox();
      this.checkBox13 = new System.Windows.Forms.CheckBox();
      this.label5 = new System.Windows.Forms.Label();
      this.maskedTextBox1 = new System.Windows.Forms.MaskedTextBox();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.numericUpDown1);
      this.groupBox1.Controls.Add(this.checkBox2);
      this.groupBox1.Controls.Add(this.checkBox1);
      this.groupBox1.Location = new System.Drawing.Point(4, 4);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(460, 75);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "General";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(222, 21);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(49, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "minute(s)";
      // 
      // numericUpDown1
      // 
      this.numericUpDown1.Location = new System.Drawing.Point(185, 19);
      this.numericUpDown1.Name = "numericUpDown1";
      this.numericUpDown1.Size = new System.Drawing.Size(35, 20);
      this.numericUpDown1.TabIndex = 2;
      this.numericUpDown1.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
      // 
      // checkBox2
      // 
      this.checkBox2.AutoSize = true;
      this.checkBox2.Location = new System.Drawing.Point(7, 44);
      this.checkBox2.Name = "checkBox2";
      this.checkBox2.Size = new System.Drawing.Size(227, 17);
      this.checkBox2.TabIndex = 1;
      this.checkBox2.Text = "Wakeup server for various wakeup events";
      this.checkBox2.UseVisualStyleBackColor = true;
      this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.checkBox1.Location = new System.Drawing.Point(7, 20);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(178, 17);
      this.checkBox1.TabIndex = 0;
      this.checkBox1.Text = "Shutdown server after being idle";
      this.checkBox1.UseVisualStyleBackColor = true;
      this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.checkBox4);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.numericUpDown3);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.comboBox1);
      this.groupBox2.Controls.Add(this.checkBox3);
      this.groupBox2.Controls.Add(this.numericUpDown2);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Location = new System.Drawing.Point(4, 85);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(460, 103);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Advanced settings";
      // 
      // checkBox4
      // 
      this.checkBox4.AutoSize = true;
      this.checkBox4.Location = new System.Drawing.Point(10, 75);
      this.checkBox4.Name = "checkBox4";
      this.checkBox4.Size = new System.Drawing.Size(144, 17);
      this.checkBox4.TabIndex = 7;
      this.checkBox4.Text = "Enable extensive logging";
      this.checkBox4.UseVisualStyleBackColor = true;
      this.checkBox4.CheckedChanged += new System.EventHandler(this.checkBox4_CheckedChanged);
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(268, 76);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(129, 13);
      this.label4.TabIndex = 6;
      this.label4.Text = "Check interval in seconds";
      // 
      // numericUpDown3
      // 
      this.numericUpDown3.Location = new System.Drawing.Point(399, 73);
      this.numericUpDown3.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
      this.numericUpDown3.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.numericUpDown3.Name = "numericUpDown3";
      this.numericUpDown3.Size = new System.Drawing.Size(45, 20);
      this.numericUpDown3.TabIndex = 5;
      this.numericUpDown3.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
      this.numericUpDown3.ValueChanged += new System.EventHandler(this.numericUpDown3_ValueChanged);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(7, 49);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(84, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Shutdown mode";
      // 
      // comboBox1
      // 
      this.comboBox1.FormattingEnabled = true;
      this.comboBox1.Items.AddRange(new object[] {
            "Suspend",
            "Hibernate",
            "Stay On"});
      this.comboBox1.Location = new System.Drawing.Point(97, 45);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(121, 21);
      this.comboBox1.TabIndex = 3;
      this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
      // 
      // checkBox3
      // 
      this.checkBox3.AutoSize = true;
      this.checkBox3.Location = new System.Drawing.Point(9, 20);
      this.checkBox3.Name = "checkBox3";
      this.checkBox3.Size = new System.Drawing.Size(363, 17);
      this.checkBox3.TabIndex = 2;
      this.checkBox3.Text = "Forced shutdown (shutdown even when prevented by another process)";
      this.checkBox3.UseVisualStyleBackColor = true;
      this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
      // 
      // numericUpDown2
      // 
      this.numericUpDown2.Location = new System.Drawing.Point(399, 46);
      this.numericUpDown2.Name = "numericUpDown2";
      this.numericUpDown2.Size = new System.Drawing.Size(46, 20);
      this.numericUpDown2.TabIndex = 1;
      this.numericUpDown2.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
      this.numericUpDown2.ValueChanged += new System.EventHandler(this.numericUpDown2_ValueChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(256, 49);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(140, 13);
      this.label2.TabIndex = 0;
      this.label2.Text = "Pre-wakeup time in seconds";
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.maskedTextBox1);
      this.groupBox3.Controls.Add(this.label5);
      this.groupBox3.Controls.Add(this.checkBox13);
      this.groupBox3.Controls.Add(this.checkBox12);
      this.groupBox3.Controls.Add(this.checkBox11);
      this.groupBox3.Controls.Add(this.checkBox10);
      this.groupBox3.Controls.Add(this.checkBox9);
      this.groupBox3.Controls.Add(this.checkBox8);
      this.groupBox3.Controls.Add(this.checkBox7);
      this.groupBox3.Controls.Add(this.checkBox6);
      this.groupBox3.Controls.Add(this.checkBox5);
      this.groupBox3.Location = new System.Drawing.Point(4, 195);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(460, 86);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "EPG grabber settings";
      // 
      // checkBox5
      // 
      this.checkBox5.AutoSize = true;
      this.checkBox5.Location = new System.Drawing.Point(238, 19);
      this.checkBox5.Name = "checkBox5";
      this.checkBox5.Size = new System.Drawing.Size(201, 17);
      this.checkBox5.TabIndex = 0;
      this.checkBox5.Text = "Prevent standby when grabbing EPG";
      this.checkBox5.UseVisualStyleBackColor = true;
      // 
      // checkBox6
      // 
      this.checkBox6.AutoSize = true;
      this.checkBox6.Location = new System.Drawing.Point(7, 19);
      this.checkBox6.Name = "checkBox6";
      this.checkBox6.Size = new System.Drawing.Size(204, 17);
      this.checkBox6.TabIndex = 1;
      this.checkBox6.Text = "Wakeup system for EPG grabbing on:";
      this.checkBox6.UseVisualStyleBackColor = true;
      // 
      // checkBox7
      // 
      this.checkBox7.AutoSize = true;
      this.checkBox7.Location = new System.Drawing.Point(7, 40);
      this.checkBox7.Name = "checkBox7";
      this.checkBox7.Size = new System.Drawing.Size(64, 17);
      this.checkBox7.TabIndex = 2;
      this.checkBox7.Text = "Monday";
      this.checkBox7.UseVisualStyleBackColor = true;
      // 
      // checkBox8
      // 
      this.checkBox8.AutoSize = true;
      this.checkBox8.Location = new System.Drawing.Point(7, 59);
      this.checkBox8.Name = "checkBox8";
      this.checkBox8.Size = new System.Drawing.Size(67, 17);
      this.checkBox8.TabIndex = 3;
      this.checkBox8.Text = "Tuesday";
      this.checkBox8.UseVisualStyleBackColor = true;
      // 
      // checkBox9
      // 
      this.checkBox9.AutoSize = true;
      this.checkBox9.Location = new System.Drawing.Point(77, 40);
      this.checkBox9.Name = "checkBox9";
      this.checkBox9.Size = new System.Drawing.Size(83, 17);
      this.checkBox9.TabIndex = 4;
      this.checkBox9.Text = "Wednesday";
      this.checkBox9.UseVisualStyleBackColor = true;
      // 
      // checkBox10
      // 
      this.checkBox10.AutoSize = true;
      this.checkBox10.Location = new System.Drawing.Point(77, 59);
      this.checkBox10.Name = "checkBox10";
      this.checkBox10.Size = new System.Drawing.Size(70, 17);
      this.checkBox10.TabIndex = 5;
      this.checkBox10.Text = "Thursday";
      this.checkBox10.UseVisualStyleBackColor = true;
      // 
      // checkBox11
      // 
      this.checkBox11.AutoSize = true;
      this.checkBox11.Location = new System.Drawing.Point(164, 40);
      this.checkBox11.Name = "checkBox11";
      this.checkBox11.Size = new System.Drawing.Size(54, 17);
      this.checkBox11.TabIndex = 6;
      this.checkBox11.Text = "Friday";
      this.checkBox11.UseVisualStyleBackColor = true;
      // 
      // checkBox12
      // 
      this.checkBox12.AutoSize = true;
      this.checkBox12.Location = new System.Drawing.Point(164, 59);
      this.checkBox12.Name = "checkBox12";
      this.checkBox12.Size = new System.Drawing.Size(68, 17);
      this.checkBox12.TabIndex = 7;
      this.checkBox12.Text = "Saturday";
      this.checkBox12.UseVisualStyleBackColor = true;
      // 
      // checkBox13
      // 
      this.checkBox13.AutoSize = true;
      this.checkBox13.Location = new System.Drawing.Point(238, 59);
      this.checkBox13.Name = "checkBox13";
      this.checkBox13.Size = new System.Drawing.Size(62, 17);
      this.checkBox13.TabIndex = 8;
      this.checkBox13.Text = "Sunday";
      this.checkBox13.UseVisualStyleBackColor = true;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(333, 60);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(73, 13);
      this.label5.TabIndex = 9;
      this.label5.Text = "Wakeup time:";
      // 
      // maskedTextBox1
      // 
      this.maskedTextBox1.Location = new System.Drawing.Point(410, 56);
      this.maskedTextBox1.Mask = "00:00";
      this.maskedTextBox1.Name = "maskedTextBox1";
      this.maskedTextBox1.Size = new System.Drawing.Size(35, 20);
      this.maskedTextBox1.TabIndex = 10;
      this.maskedTextBox1.Text = "0000";
      this.maskedTextBox1.ValidatingType = typeof(System.DateTime);
      // 
      // PowerSchedulerMasterSetup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "PowerSchedulerMasterSetup";
      this.Size = new System.Drawing.Size(467, 388);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.NumericUpDown numericUpDown1;
    private System.Windows.Forms.CheckBox checkBox2;
    private System.Windows.Forms.CheckBox checkBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.NumericUpDown numericUpDown2;
    private System.Windows.Forms.CheckBox checkBox3;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox comboBox1;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.NumericUpDown numericUpDown3;
    private System.Windows.Forms.CheckBox checkBox4;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.CheckBox checkBox5;
    private System.Windows.Forms.CheckBox checkBox13;
    private System.Windows.Forms.CheckBox checkBox12;
    private System.Windows.Forms.CheckBox checkBox11;
    private System.Windows.Forms.CheckBox checkBox10;
    private System.Windows.Forms.CheckBox checkBox9;
    private System.Windows.Forms.CheckBox checkBox8;
    private System.Windows.Forms.CheckBox checkBox7;
    private System.Windows.Forms.CheckBox checkBox6;
    private System.Windows.Forms.MaskedTextBox maskedTextBox1;
    private System.Windows.Forms.Label label5;
  }
}
