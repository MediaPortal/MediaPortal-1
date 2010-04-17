namespace MediaPortal.Support
{
  partial class ProgressDialog
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
      this.label1 = new System.Windows.Forms.Label();
      this.labelCurrentAction = new System.Windows.Forms.Label();
      this.progressBar = new System.Windows.Forms.ProgressBar();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(13, 13);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(76, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Current action:";
      // 
      // labelCurrentAction
      // 
      this.labelCurrentAction.AutoSize = true;
      this.labelCurrentAction.Location = new System.Drawing.Point(89, 13);
      this.labelCurrentAction.Name = "labelCurrentAction";
      this.labelCurrentAction.Size = new System.Drawing.Size(35, 13);
      this.labelCurrentAction.TabIndex = 1;
      this.labelCurrentAction.Text = "label2";
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(16, 38);
      this.progressBar.Maximum = 5;
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(378, 23);
      this.progressBar.Step = 1;
      this.progressBar.TabIndex = 2;
      // 
      // ProgressDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(406, 77);
      this.ControlBox = false;
      this.Controls.Add(this.progressBar);
      this.Controls.Add(this.labelCurrentAction);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ProgressDialog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Progress";
      this.TopMost = true;
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label labelCurrentAction;
    private System.Windows.Forms.ProgressBar progressBar;
  }
}