namespace MediaPortal.DeployTool
{
  partial class DeployDialog
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
      this.HeaderLabel = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // HeaderLabel
      // 
      this.HeaderLabel.AutoSize = true;
      this.HeaderLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.HeaderLabel.Location = new System.Drawing.Point(4, 4);
      this.HeaderLabel.Name = "HeaderLabel";
      this.HeaderLabel.Size = new System.Drawing.Size(142, 13);
      this.HeaderLabel.TabIndex = 0;
      this.HeaderLabel.Text = "Choose installation type";
      // 
      // DeployDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.HeaderLabel);
      this.Name = "DeployDialog";
      this.Size = new System.Drawing.Size(620, 308);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    public System.Windows.Forms.Label HeaderLabel;
  }
}
