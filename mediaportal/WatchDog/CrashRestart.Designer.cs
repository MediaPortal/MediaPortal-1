namespace WatchDog
{
  partial class CrashRestartDlg
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
      this.components = new System.ComponentModel.Container();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.lDelay = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.btnCancel = new System.Windows.Forms.Button();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.ForeColor = System.Drawing.Color.Red;
      this.label1.Location = new System.Drawing.Point(13, 13);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(210, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "MediaPortal crashed unexpectedly !";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(16, 39);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(167, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "It will be automatically restarted in ";
      // 
      // lDelay
      // 
      this.lDelay.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lDelay.Location = new System.Drawing.Point(16, 68);
      this.lDelay.Name = "lDelay";
      this.lDelay.Size = new System.Drawing.Size(207, 18);
      this.lDelay.TabIndex = 2;
      this.lDelay.Text = "label3";
      this.lDelay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 99);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(207, 30);
      this.label3.TabIndex = 3;
      this.label3.Text = "The logfiles will be automatically collected and saved to your desktop";
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(77, 148);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 4;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // timer1
      // 
      this.timer1.Enabled = true;
      this.timer1.Interval = 1000;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // CrashRestartDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(236, 198);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.lDelay);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.DoubleBuffered = true;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "CrashRestartDlg";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MediaPortal crashed!!!";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label lDelay;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Timer timer1;
  }
}