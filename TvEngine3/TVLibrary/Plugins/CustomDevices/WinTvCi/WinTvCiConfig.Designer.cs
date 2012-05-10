namespace SetupTv.Sections
{
  partial class WinTvCiConfig
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WinTvCiConfig));
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.installStateLabel = new System.Windows.Forms.Label();
      this.tipsLabel = new System.Windows.Forms.Label();
      this.tunerSelectionCombo = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.tunerSelectionPicture = new System.Windows.Forms.PictureBox();
      this.tunerSelectionLabel = new System.Windows.Forms.Label();
      this.tipHeadingLabel = new System.Windows.Forms.Label();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tunerSelectionPicture)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.installStateLabel);
      this.groupBox2.Controls.Add(this.tipsLabel);
      this.groupBox2.Controls.Add(this.tunerSelectionCombo);
      this.groupBox2.Controls.Add(this.tunerSelectionPicture);
      this.groupBox2.Controls.Add(this.tunerSelectionLabel);
      this.groupBox2.Controls.Add(this.tipHeadingLabel);
      this.groupBox2.Location = new System.Drawing.Point(3, 3);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(477, 444);
      this.groupBox2.TabIndex = 40;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "WinTV-CI Configuration";
      // 
      // installStateLabel
      // 
      this.installStateLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.installStateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.installStateLabel.ForeColor = System.Drawing.Color.Orange;
      this.installStateLabel.Location = new System.Drawing.Point(6, 29);
      this.installStateLabel.Name = "installStateLabel";
      this.installStateLabel.Size = new System.Drawing.Size(412, 16);
      this.installStateLabel.TabIndex = 1;
      this.installStateLabel.Text = "The WinTV-CI is installed correctly.";
      // 
      // tipsLabel
      // 
      this.tipsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.tipsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tipsLabel.ForeColor = System.Drawing.Color.Black;
      this.tipsLabel.Location = new System.Drawing.Point(6, 137);
      this.tipsLabel.Name = "tipsLabel";
      this.tipsLabel.Size = new System.Drawing.Size(465, 102);
      this.tipsLabel.TabIndex = 5;
      this.tipsLabel.Text = resources.GetString("tipsLabel.Text");
      // 
      // tunerSelectionCombo
      // 
      this.tunerSelectionCombo.FormattingEnabled = true;
      this.tunerSelectionCombo.Location = new System.Drawing.Point(79, 80);
      this.tunerSelectionCombo.Name = "tunerSelectionCombo";
      this.tunerSelectionCombo.Size = new System.Drawing.Size(339, 21);
      this.tunerSelectionCombo.TabIndex = 3;
      // 
      // tunerSelectionPicture
      // 
      this.tunerSelectionPicture.Image = ((System.Drawing.Image)(resources.GetObject("tunerSelectionPicture.Image")));
      this.tunerSelectionPicture.Location = new System.Drawing.Point(24, 80);
      this.tunerSelectionPicture.Name = "tunerSelectionPicture";
      this.tunerSelectionPicture.Size = new System.Drawing.Size(33, 23);
      this.tunerSelectionPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.tunerSelectionPicture.TabIndex = 18;
      this.tunerSelectionPicture.TabStop = false;
      // 
      // tunerSelectionLabel
      // 
      this.tunerSelectionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.tunerSelectionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tunerSelectionLabel.Location = new System.Drawing.Point(6, 59);
      this.tunerSelectionLabel.Name = "tunerSelectionLabel";
      this.tunerSelectionLabel.Size = new System.Drawing.Size(412, 18);
      this.tunerSelectionLabel.TabIndex = 2;
      this.tunerSelectionLabel.Text = "Select a DVB tuner to use the CI module with:";
      // 
      // tipHeadingLabel
      // 
      this.tipHeadingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.tipHeadingLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tipHeadingLabel.ForeColor = System.Drawing.Color.Black;
      this.tipHeadingLabel.Location = new System.Drawing.Point(6, 121);
      this.tipHeadingLabel.Name = "tipHeadingLabel";
      this.tipHeadingLabel.Size = new System.Drawing.Size(412, 16);
      this.tipHeadingLabel.TabIndex = 4;
      this.tipHeadingLabel.Text = "Tips";
      // 
      // WinTvCiConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBox2);
      this.Name = "WinTvCiConfig";
      this.Size = new System.Drawing.Size(483, 450);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tunerSelectionPicture)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.PictureBox tunerSelectionPicture;
    private System.Windows.Forms.Label tunerSelectionLabel;
    private System.Windows.Forms.Label tipHeadingLabel;
    private System.Windows.Forms.Label tipsLabel;
    private MediaPortal.UserInterface.Controls.MPComboBox tunerSelectionCombo;
    private System.Windows.Forms.Label installStateLabel;
  }
}
