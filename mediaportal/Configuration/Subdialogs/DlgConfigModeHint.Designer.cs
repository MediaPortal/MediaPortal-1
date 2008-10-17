namespace MediaPortal.Configuration
{
  partial class DlgConfigModeHint
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
      this.btnContinue = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxConfirmed = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
      this.radioButtonNormal = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonAdvanced = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.lblNormal = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
      this.SuspendLayout();
      // 
      // btnContinue
      // 
      this.btnContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnContinue.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnContinue.Location = new System.Drawing.Point(220, 195);
      this.btnContinue.Name = "btnContinue";
      this.btnContinue.Size = new System.Drawing.Size(75, 23);
      this.btnContinue.TabIndex = 4;
      this.btnContinue.Text = "&Continue";
      this.btnContinue.UseVisualStyleBackColor = true;
      this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
      // 
      // checkBoxConfirmed
      // 
      this.checkBoxConfirmed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.checkBoxConfirmed.AutoSize = true;
      this.checkBoxConfirmed.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxConfirmed.Location = new System.Drawing.Point(112, 198);
      this.checkBoxConfirmed.Name = "checkBoxConfirmed";
      this.checkBoxConfirmed.Size = new System.Drawing.Size(105, 17);
      this.checkBoxConfirmed.TabIndex = 3;
      this.checkBoxConfirmed.Text = "Do not ask again";
      this.checkBoxConfirmed.UseVisualStyleBackColor = true;
      // 
      // pictureBoxLogo
      // 
      this.pictureBoxLogo.Dock = System.Windows.Forms.DockStyle.Top;
      this.pictureBoxLogo.Image = global::MediaPortal.Configuration.Properties.Resources.splashscreen_configuration;
      this.pictureBoxLogo.Location = new System.Drawing.Point(0, 0);
      this.pictureBoxLogo.Name = "pictureBoxLogo";
      this.pictureBoxLogo.Size = new System.Drawing.Size(307, 65);
      this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBoxLogo.TabIndex = 2;
      this.pictureBoxLogo.TabStop = false;
      // 
      // radioButtonNormal
      // 
      this.radioButtonNormal.AutoSize = true;
      this.radioButtonNormal.Checked = true;
      this.radioButtonNormal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonNormal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.radioButtonNormal.Location = new System.Drawing.Point(28, 91);
      this.radioButtonNormal.Name = "radioButtonNormal";
      this.radioButtonNormal.Size = new System.Drawing.Size(97, 17);
      this.radioButtonNormal.TabIndex = 1;
      this.radioButtonNormal.TabStop = true;
      this.radioButtonNormal.Text = "Normal mode";
      this.radioButtonNormal.UseVisualStyleBackColor = true;
      // 
      // radioButtonAdvanced
      // 
      this.radioButtonAdvanced.AutoSize = true;
      this.radioButtonAdvanced.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonAdvanced.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.radioButtonAdvanced.Location = new System.Drawing.Point(28, 144);
      this.radioButtonAdvanced.Name = "radioButtonAdvanced";
      this.radioButtonAdvanced.Size = new System.Drawing.Size(115, 17);
      this.radioButtonAdvanced.TabIndex = 2;
      this.radioButtonAdvanced.Text = "Advanced mode";
      this.radioButtonAdvanced.UseVisualStyleBackColor = true;
      // 
      // lblNormal
      // 
      this.lblNormal.AutoSize = true;
      this.lblNormal.Location = new System.Drawing.Point(43, 111);
      this.lblNormal.Name = "lblNormal";
      this.lblNormal.Size = new System.Drawing.Size(236, 13);
      this.lblNormal.TabIndex = 5;
      this.lblNormal.Text = "Display common options which are often needed";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(43, 164);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(186, 13);
      this.mpLabel1.TabIndex = 6;
      this.mpLabel1.Text = "Display all options (even experimental)";
      // 
      // DlgConfigModeHint
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(307, 230);
      this.ControlBox = false;
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.lblNormal);
      this.Controls.Add(this.radioButtonAdvanced);
      this.Controls.Add(this.radioButtonNormal);
      this.Controls.Add(this.checkBoxConfirmed);
      this.Controls.Add(this.pictureBoxLogo);
      this.Controls.Add(this.btnContinue);
      this.DoubleBuffered = true;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "DlgConfigModeHint";
      this.Text = "MediaPortal Configuration";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPButton btnContinue;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxConfirmed;
    private System.Windows.Forms.PictureBox pictureBoxLogo;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonNormal;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonAdvanced;
    private MediaPortal.UserInterface.Controls.MPLabel lblNormal;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
  }
}