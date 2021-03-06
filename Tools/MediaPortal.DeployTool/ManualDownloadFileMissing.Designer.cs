namespace MediaPortal.DeployTool
{
  partial class ManualDownloadFileMissing
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
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.labelHeading = new System.Windows.Forms.Label();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.buttonBrowse = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // labelHeading
      // 
      this.labelHeading.AutoSize = true;
      this.labelHeading.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelHeading.ForeColor = System.Drawing.Color.White;
      this.labelHeading.Location = new System.Drawing.Point(28, 20);
      this.labelHeading.MaximumSize = new System.Drawing.Size(500, 0);
      this.labelHeading.Name = "labelHeading";
      this.labelHeading.Size = new System.Drawing.Size(386, 17);
      this.labelHeading.TabIndex = 0;
      this.labelHeading.Text = "Downloaded file not found. Please browse for it:";
      // 
      // textBox1
      // 
      this.textBox1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.textBox1.Location = new System.Drawing.Point(31, 139);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(396, 21);
      this.textBox1.TabIndex = 1;
      this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Cursor = System.Windows.Forms.Cursors.Hand;
      this.buttonBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.buttonBrowse.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.buttonBrowse.ForeColor = System.Drawing.Color.White;
      this.buttonBrowse.Location = new System.Drawing.Point(433, 138);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(87, 23);
      this.buttonBrowse.TabIndex = 2;
      this.buttonBrowse.TabStop = false;
      this.buttonBrowse.Text = "Browse";
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new System.EventHandler(this.button1_Click);
      // 
      // ManualDownloadFileMissing
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(111)))), ((int)(((byte)(152)))));
      this.ClientSize = new System.Drawing.Size(548, 180);
      this.Controls.Add(this.buttonBrowse);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.labelHeading);
      this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ManualDownloadFileMissing";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Manual download";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.Label labelHeading;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Button buttonBrowse;
  }
}