namespace MediaPortal
{
  partial class FullScreenSplashScreen
  {
    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Vom Windows Form-Designer generierter Code

    /// <summary>
    /// Erforderliche Methode für die Designerunterstützung.
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      this.lblMain = new System.Windows.Forms.Label();
      this.lblVersion = new System.Windows.Forms.Label();
      this.lblCVS = new System.Windows.Forms.Label();
      this.pbBackground = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.pbBackground)).BeginInit();
      this.SuspendLayout();
      // 
      // lblMain
      // 
      this.lblMain.AutoEllipsis = true;
      this.lblMain.BackColor = System.Drawing.Color.Transparent;
      this.lblMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblMain.Font = new System.Drawing.Font("Arial", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblMain.ForeColor = System.Drawing.Color.White;
      this.lblMain.Location = new System.Drawing.Point(0, 0);
      this.lblMain.Name = "lblMain";
      this.lblMain.Size = new System.Drawing.Size(284, 264);
      this.lblMain.TabIndex = 4;
      this.lblMain.Text = "...";
      this.lblMain.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // lblVersion
      // 
      this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblVersion.BackColor = System.Drawing.Color.Transparent;
      this.lblVersion.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblVersion.ForeColor = System.Drawing.Color.White;
      this.lblVersion.Location = new System.Drawing.Point(7, 234);
      this.lblVersion.Name = "lblVersion";
      this.lblVersion.Size = new System.Drawing.Size(128, 21);
      this.lblVersion.TabIndex = 5;
      this.lblVersion.Text = "Version";
      this.lblVersion.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // lblCVS
      // 
      this.lblCVS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblCVS.BackColor = System.Drawing.Color.Transparent;
      this.lblCVS.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblCVS.ForeColor = System.Drawing.Color.White;
      this.lblCVS.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
      this.lblCVS.Location = new System.Drawing.Point(141, 234);
      this.lblCVS.Name = "lblCVS";
      this.lblCVS.Size = new System.Drawing.Size(131, 21);
      this.lblCVS.TabIndex = 6;
      this.lblCVS.Text = "CVSVersion";
      this.lblCVS.TextAlign = System.Drawing.ContentAlignment.BottomRight;
      // 
      // pbBackground
      // 
      this.pbBackground.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
      this.pbBackground.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pbBackground.Location = new System.Drawing.Point(0, 0);
      this.pbBackground.Margin = new System.Windows.Forms.Padding(0);
      this.pbBackground.Name = "pbBackground";
      this.pbBackground.Size = new System.Drawing.Size(284, 264);
      this.pbBackground.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pbBackground.TabIndex = 7;
      this.pbBackground.TabStop = false;
      // 
      // FullScreenSplashScreen
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 264);
      this.Controls.Add(this.lblVersion);
      this.Controls.Add(this.lblCVS);
      this.Controls.Add(this.lblMain);
      this.Controls.Add(this.pbBackground);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "FullScreenSplashScreen";
      this.Opacity = 0;
      this.ShowInTaskbar = false;
      this.Text = "FullScreenSplashScreen";
      this.TopMost = true;
      this.Load += new System.EventHandler(this.FullScreenSplashScreen_Load);
      this.Activated += new System.EventHandler(this.FullScreenSplashScreen_Activated);
      ((System.ComponentModel.ISupportInitialize)(this.pbBackground)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    public System.Windows.Forms.Label lblMain;
    public System.Windows.Forms.PictureBox pbBackground;
    public System.Windows.Forms.Label lblVersion;
    public System.Windows.Forms.Label lblCVS;
  }
}
