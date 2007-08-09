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
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.linkHomepage = new System.Windows.Forms.LinkLabel();
      this.linkForum = new System.Windows.Forms.LinkLabel();
      this.linkWiki = new System.Windows.Forms.LinkLabel();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(4, 30);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(131, 17);
      this.label1.TabIndex = 11;
      this.label1.Text = "Congratulations !";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(4, 54);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(400, 17);
      this.label2.TabIndex = 12;
      this.label2.Text = "You successfully completed your setup of MediaPortal";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(4, 87);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(396, 13);
      this.label3.TabIndex = 13;
      this.label3.Text = "Below you may find some useful links that help you getting started with MediaPort" +
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
      // FinishedDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.linkWiki);
      this.Controls.Add(this.linkForum);
      this.Controls.Add(this.linkHomepage);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Name = "FinishedDlg";
      this.Size = new System.Drawing.Size(620, 192);
      this.Controls.SetChildIndex(this.HeaderLabel, 0);
      this.Controls.SetChildIndex(this.label1, 0);
      this.Controls.SetChildIndex(this.label2, 0);
      this.Controls.SetChildIndex(this.label3, 0);
      this.Controls.SetChildIndex(this.linkHomepage, 0);
      this.Controls.SetChildIndex(this.linkForum, 0);
      this.Controls.SetChildIndex(this.linkWiki, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.LinkLabel linkHomepage;
    private System.Windows.Forms.LinkLabel linkForum;
    private System.Windows.Forms.LinkLabel linkWiki;
  }
}
