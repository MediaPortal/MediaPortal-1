namespace FullscreenMsg
{
  partial class frmFullScreen
  {
    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    //protected override void Dispose(bool disposing)
    //{
    //  if (disposing && (components != null))
    //  {
    //    components.Dispose();
    //  }
    //  base.Dispose(disposing);
    //}

    #region Vom Windows Form-Designer generierter Code

    /// <summary>
    /// Erforderliche Methode für die Designerunterstützung.
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.pbBackground = new System.Windows.Forms.PictureBox();
      this.lblMainLable = new System.Windows.Forms.Label();
      this.timerFullscreen = new System.Windows.Forms.Timer(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.pbBackground)).BeginInit();
      this.SuspendLayout();
      // 
      // pbBackground
      // 
      this.pbBackground.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
      this.pbBackground.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pbBackground.Location = new System.Drawing.Point(0, 0);
      this.pbBackground.Name = "pbBackground";
      this.pbBackground.Size = new System.Drawing.Size(320, 273);
      this.pbBackground.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pbBackground.TabIndex = 2;
      this.pbBackground.TabStop = false;
      // 
      // lblMainLable
      // 
      this.lblMainLable.AutoEllipsis = true;
      this.lblMainLable.BackColor = System.Drawing.Color.Transparent;
      this.lblMainLable.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblMainLable.Font = new System.Drawing.Font("Nina", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblMainLable.ForeColor = System.Drawing.Color.White;
      this.lblMainLable.Location = new System.Drawing.Point(0, 0);
      this.lblMainLable.Name = "lblMainLable";
      this.lblMainLable.Size = new System.Drawing.Size(320, 273);
      this.lblMainLable.TabIndex = 3;
      this.lblMainLable.Text = "...";
      this.lblMainLable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.lblMainLable.DoubleClick += new System.EventHandler(this.lblMainLable_DoubleClick);
      // 
      // timerFullscreen
      // 
      this.timerFullscreen.Enabled = true;
      this.timerFullscreen.Tick += new System.EventHandler(this.timerFullscreen_Tick);
      // 
      // frmFullScreen
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
      this.ClientSize = new System.Drawing.Size(320, 273);
      this.Controls.Add(this.lblMainLable);
      this.Controls.Add(this.pbBackground);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frmFullScreen";
      this.Opacity = 0;
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "FullScreenForm";
      this.TopMost = true;
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      ((System.ComponentModel.ISupportInitialize)(this.pbBackground)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    public System.Windows.Forms.PictureBox pbBackground;
    public System.Windows.Forms.Label lblMainLable;
    private System.Windows.Forms.Timer timerFullscreen;


  }
}