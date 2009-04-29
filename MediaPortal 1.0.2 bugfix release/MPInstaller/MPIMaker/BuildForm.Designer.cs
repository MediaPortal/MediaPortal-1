namespace MediaPortal.MPInstaller
{
  partial class Build_dialog
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
      this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.button3 = new System.Windows.Forms.Button();
      this.listBox1 = new System.Windows.Forms.ListBox();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.button4 = new System.Windows.Forms.Button();
      this.textBox2 = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.textBox3 = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.textBox4 = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // saveFileDialog1
      // 
      this.saveFileDialog1.DefaultExt = "mpi";
      this.saveFileDialog1.Filter = "MPE1 files|*.MPE1|ZIP files|*.zip|All files|*.*";
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(12, 23);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(334, 20);
      this.textBox1.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(9, 7);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(58, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "File name :";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(354, 20);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(27, 23);
      this.button1.TabIndex = 2;
      this.button1.Text = "...";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(12, 220);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 23);
      this.button2.TabIndex = 3;
      this.button2.Text = "Build";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Visible = false;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(306, 220);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(75, 23);
      this.button3.TabIndex = 4;
      this.button3.Text = "Close";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // listBox1
      // 
      this.listBox1.FormattingEnabled = true;
      this.listBox1.HorizontalScrollbar = true;
      this.listBox1.Location = new System.Drawing.Point(12, 54);
      this.listBox1.Name = "listBox1";
      this.listBox1.Size = new System.Drawing.Size(369, 134);
      this.listBox1.TabIndex = 5;
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.progressBar1.Location = new System.Drawing.Point(12, 198);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(369, 16);
      this.progressBar1.TabIndex = 6;
      // 
      // button4
      // 
      this.button4.Location = new System.Drawing.Point(147, 220);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(95, 23);
      this.button4.TabIndex = 7;
      this.button4.Text = "Build ";
      this.button4.UseVisualStyleBackColor = true;
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // textBox2
      // 
      this.textBox2.Location = new System.Drawing.Point(403, 54);
      this.textBox2.Name = "textBox2";
      this.textBox2.Size = new System.Drawing.Size(249, 20);
      this.textBox2.TabIndex = 8;
      this.textBox2.Text = "ftp://ftp.extra.hu/wwwroot/upload";
      this.textBox2.Visible = false;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(400, 38);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(41, 13);
      this.label2.TabIndex = 9;
      this.label2.Text = "Server ";
      this.label2.Visible = false;
      // 
      // textBox3
      // 
      this.textBox3.Location = new System.Drawing.Point(403, 122);
      this.textBox3.Name = "textBox3";
      this.textBox3.Size = new System.Drawing.Size(249, 20);
      this.textBox3.TabIndex = 10;
      this.textBox3.Visible = false;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(400, 106);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(58, 13);
      this.label3.TabIndex = 11;
      this.label3.Text = "User name";
      this.label3.Visible = false;
      // 
      // textBox4
      // 
      this.textBox4.Location = new System.Drawing.Point(403, 163);
      this.textBox4.Name = "textBox4";
      this.textBox4.Size = new System.Drawing.Size(249, 20);
      this.textBox4.TabIndex = 12;
      this.textBox4.Visible = false;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(400, 147);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(53, 13);
      this.label4.TabIndex = 13;
      this.label4.Text = "Password";
      this.label4.Visible = false;
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.checkBox1.Checked = true;
      this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBox1.Location = new System.Drawing.Point(403, 80);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(74, 17);
      this.checkBox1.TabIndex = 14;
      this.checkBox1.Text = "Local disc";
      this.checkBox1.UseVisualStyleBackColor = true;
      this.checkBox1.Visible = false;
      this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
      // 
      // Build_dialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(677, 255);
      this.Controls.Add(this.checkBox1);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.textBox4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.textBox3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.textBox2);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.listBox1);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.textBox1);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "Build_dialog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Build";
      this.Load += new System.EventHandler(this.Build_dialog_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;
      private System.Windows.Forms.ListBox listBox1;
      private System.Windows.Forms.ProgressBar progressBar1;
      private System.Windows.Forms.Button button4;
      private System.Windows.Forms.TextBox textBox2;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.TextBox textBox3;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.TextBox textBox4;
      private System.Windows.Forms.Label label4;
    private System.Windows.Forms.CheckBox checkBox1;
  }
}