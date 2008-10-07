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
        // labelSectionHeader
        // 
        this.labelSectionHeader.Location = new System.Drawing.Point(5, 4);
        // 
        // labelHeading1
        // 
        this.labelHeading1.AutoSize = true;
        this.labelHeading1.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelHeading1.ForeColor = System.Drawing.Color.White;
        this.labelHeading1.Location = new System.Drawing.Point(160, 17);
        this.labelHeading1.Name = "labelHeading1";
        this.labelHeading1.Size = new System.Drawing.Size(143, 17);
        this.labelHeading1.TabIndex = 11;
        this.labelHeading1.Text = "Congratulations !";
        // 
        // labelHeading2
        // 
        this.labelHeading2.AutoSize = true;
        this.labelHeading2.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelHeading2.ForeColor = System.Drawing.Color.White;
        this.labelHeading2.Location = new System.Drawing.Point(160, 45);
        this.labelHeading2.MaximumSize = new System.Drawing.Size(450, 0);
        this.labelHeading2.Name = "labelHeading2";
        this.labelHeading2.Size = new System.Drawing.Size(426, 17);
        this.labelHeading2.TabIndex = 12;
        this.labelHeading2.Text = "You successfully completed your setup of MediaPortal";
        // 
        // labelHeading3
        // 
        this.labelHeading3.AutoSize = true;
        this.labelHeading3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelHeading3.ForeColor = System.Drawing.Color.White;
        this.labelHeading3.Location = new System.Drawing.Point(160, 78);
        this.labelHeading3.MaximumSize = new System.Drawing.Size(450, 0);
        this.labelHeading3.Name = "labelHeading3";
        this.labelHeading3.Size = new System.Drawing.Size(420, 26);
        this.labelHeading3.TabIndex = 13;
        this.labelHeading3.Text = "Below you may find some useful links that help you getting started with MediaPort" +
            "al";
        // 
        // linkHomepage
        // 
        this.linkHomepage.AutoSize = true;
        this.linkHomepage.LinkColor = System.Drawing.Color.White;
        this.linkHomepage.Location = new System.Drawing.Point(160, 120);
        this.linkHomepage.Name = "linkHomepage";
        this.linkHomepage.Size = new System.Drawing.Size(138, 13);
        this.linkHomepage.TabIndex = 14;
        this.linkHomepage.TabStop = true;
        this.linkHomepage.Text = "MediaPortal Homepage";
        this.linkHomepage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHomepage_LinkClicked);
        // 
        // linkForum
        // 
        this.linkForum.AutoSize = true;
        this.linkForum.LinkColor = System.Drawing.Color.White;
        this.linkForum.Location = new System.Drawing.Point(160, 145);
        this.linkForum.Name = "linkForum";
        this.linkForum.Size = new System.Drawing.Size(119, 13);
        this.linkForum.TabIndex = 15;
        this.linkForum.TabStop = true;
        this.linkForum.Text = "MediaPortal Forums";
        this.linkForum.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkForum_LinkClicked);
        // 
        // linkWiki
        // 
        this.linkWiki.AutoSize = true;
        this.linkWiki.LinkColor = System.Drawing.Color.White;
        this.linkWiki.Location = new System.Drawing.Point(160, 170);
        this.linkWiki.Name = "linkWiki";
        this.linkWiki.Size = new System.Drawing.Size(101, 13);
        this.linkWiki.TabIndex = 16;
        this.linkWiki.TabStop = true;
        this.linkWiki.Text = "MediaPortal Wiki";
        this.linkWiki.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkWiki_LinkClicked_1);
        // 
        // labelEbay
        // 
        this.labelEbay.AutoSize = true;
        this.labelEbay.BackColor = System.Drawing.Color.Transparent;
        this.labelEbay.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelEbay.ForeColor = System.Drawing.Color.White;
        this.labelEbay.Location = new System.Drawing.Point(160, 200);
        this.labelEbay.MaximumSize = new System.Drawing.Size(450, 0);
        this.labelEbay.Name = "labelEbay";
        this.labelEbay.Size = new System.Drawing.Size(446, 26);
        this.labelEbay.TabIndex = 17;
        this.labelEbay.Text = "*Mediaportal is dedicated to always remaining a free program and not for sale";
        // 
        // FinishedDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_with_MP_Box;
        this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        this.Controls.Add(this.labelEbay);
        this.Controls.Add(this.linkWiki);
        this.Controls.Add(this.linkForum);
        this.Controls.Add(this.linkHomepage);
        this.Controls.Add(this.labelHeading3);
        this.Controls.Add(this.labelHeading2);
        this.Controls.Add(this.labelHeading1);
        this.DoubleBuffered = true;
        this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.Name = "FinishedDlg";
        this.Size = new System.Drawing.Size(666, 252);
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
