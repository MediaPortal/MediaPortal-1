namespace MediaPortal.DeployTool
{
  partial class WelcomeDlg
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
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.cbLanguage = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(21, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(264, 16);
      this.label1.TabIndex = 1;
      this.label1.Text = "Thank you for choosing MediaPortal!";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(21, 47);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(483, 16);
      this.label2.TabIndex = 2;
      this.label2.Text = "This deployment tool will guide you through the installation process";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(21, 80);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(352, 16);
      this.label3.TabIndex = 3;
      this.label3.Text = "Please select your language before you continue";
      // 
      // cbLanguage
      // 
      this.cbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbLanguage.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cbLanguage.FormattingEnabled = true;
      this.cbLanguage.Items.AddRange(new object[] {
            "english",
            "german"});
      this.cbLanguage.Location = new System.Drawing.Point(379, 76);
      this.cbLanguage.Name = "cbLanguage";
      this.cbLanguage.Size = new System.Drawing.Size(121, 24);
      this.cbLanguage.TabIndex = 4;
      // 
      // WelcomeDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.cbLanguage);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Name = "WelcomeDlg";
      this.Controls.SetChildIndex(this.label1, 0);
      this.Controls.SetChildIndex(this.HeaderLabel, 0);
      this.Controls.SetChildIndex(this.label2, 0);
      this.Controls.SetChildIndex(this.label3, 0);
      this.Controls.SetChildIndex(this.cbLanguage, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox cbLanguage;
  }
}
