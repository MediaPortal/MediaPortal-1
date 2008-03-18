namespace MediaPortal.DeployTool
{
  partial class FinishedDlg
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
        this.linkHomepage = new System.Windows.Forms.LinkLabel();
        this.linkForum = new System.Windows.Forms.LinkLabel();
        this.linkWiki = new System.Windows.Forms.LinkLabel();
        this.labelEbay = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // labelHeading1
        // 
        this.labelHeading1.AutoSize = true;
        this.labelHeading1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelHeading1.Location = new System.Drawing.Point(4, 30);
        this.labelHeading1.Name = "labelHeading1";
        this.labelHeading1.Size = new System.Drawing.Size(131, 17);
        this.labelHeading1.TabIndex = 11;
        this.labelHeading1.Text = "Congratulations !";
        // 
        // labelHeading2
        // 
        this.labelHeading2.AutoSize = true;
        this.labelHeading2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelHeading2.Location = new System.Drawing.Point(4, 54);
        this.labelHeading2.Name = "labelHeading2";
        this.labelHeading2.Size = new System.Drawing.Size(400, 17);
        this.labelHeading2.TabIndex = 12;
        this.labelHeading2.Text = "You successfully completed your setup of MediaPortal";
        // 
        // labelHeading3
        // 
        this.labelHeading3.AutoSize = true;
        this.labelHeading3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelHeading3.Location = new System.Drawing.Point(4, 87);
        this.labelHeading3.Name = "labelHeading3";
        this.labelHeading3.Size = new System.Drawing.Size(396, 13);
        this.labelHeading3.TabIndex = 13;
        this.labelHeading3.Text = "Below you may find some useful links that help you getting started with MediaPort" +
            "al";
        // 
        // linkHomepage
        // 
        this.linkHomepage.AutoSize = true;
        this.linkHomepage.Location = new System.Drawing.Point(7, 116);
        this.linkHomepage.Name = "linkHomepage";
        this.linkHomepage.Size = new System.Drawing.Size(118, 13);
        this.linkHomepage.TabIndex = 14;
        this.linkHomepage.TabStop = true;
        this.linkHomepage.Text = "MediaPortal Homepage";
        this.linkHomepage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHomepage_LinkClicked);
        // 
        // linkForum
        // 
        this.linkForum.AutoSize = true;
        this.linkForum.Location = new System.Drawing.Point(7, 140);
        this.linkForum.Name = "linkForum";
        this.linkForum.Size = new System.Drawing.Size(100, 13);
        this.linkForum.TabIndex = 15;
        this.linkForum.TabStop = true;
        this.linkForum.Text = "MediaPortal Forums";
        this.linkForum.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkForum_LinkClicked);
        // 
        // linkWiki
        // 
        this.linkWiki.AutoSize = true;
        this.linkWiki.Location = new System.Drawing.Point(7, 167);
        this.linkWiki.Name = "linkWiki";
        this.linkWiki.Size = new System.Drawing.Size(87, 13);
        this.linkWiki.TabIndex = 16;
        this.linkWiki.TabStop = true;
        this.linkWiki.Text = "MediaPortal Wiki";
        this.linkWiki.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkWiki_LinkClicked_1);
        // 
        // labelEbay
        // 
        this.labelEbay.AutoSize = true;
        this.labelEbay.BackColor = System.Drawing.Color.Transparent;
        this.labelEbay.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelEbay.ForeColor = System.Drawing.Color.Red;
        this.labelEbay.Location = new System.Drawing.Point(7, 242);
        this.labelEbay.Name = "labelEbay";
        this.labelEbay.Size = new System.Drawing.Size(441, 13);
        this.labelEbay.TabIndex = 17;
        this.labelEbay.Text = "MediaPortal is FREE, if you bought it on ebay, then you have been fooled ;-(";
        // 
        // FinishedDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.labelEbay);
        this.Controls.Add(this.linkWiki);
        this.Controls.Add(this.linkForum);
        this.Controls.Add(this.linkHomepage);
        this.Controls.Add(this.labelHeading3);
        this.Controls.Add(this.labelHeading2);
        this.Controls.Add(this.labelHeading1);
        this.Name = "FinishedDlg";
        this.Size = new System.Drawing.Size(620, 281);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.labelHeading1, 0);
        this.Controls.SetChildIndex(this.labelHeading2, 0);
        this.Controls.SetChildIndex(this.labelHeading3, 0);
        this.Controls.SetChildIndex(this.linkHomepage, 0);
        this.Controls.SetChildIndex(this.linkForum, 0);
        this.Controls.SetChildIndex(this.linkWiki, 0);
        this.Controls.SetChildIndex(this.labelEbay, 0);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelHeading1;
    private System.Windows.Forms.Label labelHeading2;
    private System.Windows.Forms.Label labelHeading3;
    private System.Windows.Forms.LinkLabel linkHomepage;
    private System.Windows.Forms.LinkLabel linkForum;
    private System.Windows.Forms.LinkLabel linkWiki;
      private System.Windows.Forms.Label labelEbay;
  }
}
