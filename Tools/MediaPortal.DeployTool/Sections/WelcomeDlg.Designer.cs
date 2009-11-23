namespace MediaPortal.DeployTool.Sections
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
        this.labelHeading1 = new System.Windows.Forms.Label();
        this.labelHeading2 = new System.Windows.Forms.Label();
        this.labelHeading3 = new System.Windows.Forms.Label();
        this.cbLanguage = new System.Windows.Forms.ComboBox();
        this.SuspendLayout();
        // 
        // labelHeading1
        // 
        this.labelHeading1.AutoSize = true;
        this.labelHeading1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelHeading1.ForeColor = System.Drawing.Color.White;
        this.labelHeading1.Location = new System.Drawing.Point(157, 19);
        this.labelHeading1.MaximumSize = new System.Drawing.Size(405, 0);
        this.labelHeading1.Name = "labelHeading1";
        this.labelHeading1.Size = new System.Drawing.Size(402, 32);
        this.labelHeading1.TabIndex = 1;
        this.labelHeading1.Text = "This application will guide you through the installation of MediaPortal and all t" +
            "he required components.";
        // 
        // labelHeading2
        // 
        this.labelHeading2.AutoSize = true;
        this.labelHeading2.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelHeading2.ForeColor = System.Drawing.Color.White;
        this.labelHeading2.Location = new System.Drawing.Point(157, 75);
        this.labelHeading2.MaximumSize = new System.Drawing.Size(450, 0);
        this.labelHeading2.Name = "labelHeading2";
        this.labelHeading2.Size = new System.Drawing.Size(437, 14);
        this.labelHeading2.TabIndex = 2;
        this.labelHeading2.Text = "This deployment tool will guide you through the installation process.";
        // 
        // labelHeading3
        // 
        this.labelHeading3.AutoSize = true;
        this.labelHeading3.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelHeading3.ForeColor = System.Drawing.Color.White;
        this.labelHeading3.Location = new System.Drawing.Point(157, 131);
        this.labelHeading3.Name = "labelHeading3";
        this.labelHeading3.Size = new System.Drawing.Size(315, 14);
        this.labelHeading3.TabIndex = 3;
        this.labelHeading3.Text = "Please select your language before you continue";
        // 
        // cbLanguage
        // 
        this.cbLanguage.Cursor = System.Windows.Forms.Cursors.Hand;
        this.cbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cbLanguage.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.cbLanguage.FormattingEnabled = true;
        this.cbLanguage.Location = new System.Drawing.Point(160, 160);
        this.cbLanguage.Name = "cbLanguage";
        this.cbLanguage.Size = new System.Drawing.Size(248, 22);
        this.cbLanguage.TabIndex = 4;
        this.cbLanguage.SelectedIndexChanged += new System.EventHandler(this.cbLanguage_SelectedIndexChanged);
        // 
        // WelcomeDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_with_MP_Box;
        this.Controls.Add(this.cbLanguage);
        this.Controls.Add(this.labelHeading3);
        this.Controls.Add(this.labelHeading2);
        this.Controls.Add(this.labelHeading1);
        this.Name = "WelcomeDlg";
        this.Size = new System.Drawing.Size(666, 250);
        this.Controls.SetChildIndex(this.labelHeading1, 0);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.labelHeading2, 0);
        this.Controls.SetChildIndex(this.labelHeading3, 0);
        this.Controls.SetChildIndex(this.cbLanguage, 0);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelHeading1;
    private System.Windows.Forms.Label labelHeading2;
    private System.Windows.Forms.Label labelHeading3;
    private System.Windows.Forms.ComboBox cbLanguage;
  }
}