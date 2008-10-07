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
        this.labelSectionHeader = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.AutoSize = true;
        this.labelSectionHeader.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelSectionHeader.ForeColor = System.Drawing.Color.White;
        this.labelSectionHeader.Location = new System.Drawing.Point(4, 4);
        this.labelSectionHeader.Name = "labelSectionHeader";
        this.labelSectionHeader.Size = new System.Drawing.Size(164, 13);
        this.labelSectionHeader.TabIndex = 0;
        this.labelSectionHeader.Text = "Choose installation type";
        // 
        // DeployDialog
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.Transparent;
        this.Controls.Add(this.labelSectionHeader);
        this.Name = "DeployDialog";
        this.Size = new System.Drawing.Size(664, 254);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    public System.Windows.Forms.Label labelSectionHeader;
  }
}
