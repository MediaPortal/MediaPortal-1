namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    partial class ImonLcd_AdvancedSetupForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOK = new MediaPortal.UserInterface.Controls.MPButton();
            this.gbxLineOptions = new System.Windows.Forms.GroupBox();
            this.gbxPlayback = new System.Windows.Forms.GroupBox();
            this.rbtnPlaybackSecondLine = new System.Windows.Forms.RadioButton();
            this.rbtnPlaybackFirstLine = new System.Windows.Forms.RadioButton();
            this.gbxGeneral = new System.Windows.Forms.GroupBox();
            this.rbtnGeneralSecondLine = new System.Windows.Forms.RadioButton();
            this.rbtnGeneralFirstLine = new System.Windows.Forms.RadioButton();
            this.gbxLineOptions.SuspendLayout();
            this.gbxPlayback.SuspendLayout();
            this.gbxGeneral.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(244, 141);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(78, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // gbxLineOptions
            // 
            this.gbxLineOptions.Controls.Add(this.gbxPlayback);
            this.gbxLineOptions.Controls.Add(this.gbxGeneral);
            this.gbxLineOptions.Location = new System.Drawing.Point(13, 13);
            this.gbxLineOptions.Name = "gbxLineOptions";
            this.gbxLineOptions.Size = new System.Drawing.Size(309, 123);
            this.gbxLineOptions.TabIndex = 8;
            this.gbxLineOptions.TabStop = false;
            this.gbxLineOptions.Text = "Line Options";
            // 
            // gbxPlayback
            // 
            this.gbxPlayback.Controls.Add(this.rbtnPlaybackSecondLine);
            this.gbxPlayback.Controls.Add(this.rbtnPlaybackFirstLine);
            this.gbxPlayback.Location = new System.Drawing.Point(7, 69);
            this.gbxPlayback.Name = "gbxPlayback";
            this.gbxPlayback.Size = new System.Drawing.Size(296, 43);
            this.gbxPlayback.TabIndex = 1;
            this.gbxPlayback.TabStop = false;
            this.gbxPlayback.Text = "During Playback";
            // 
            // rbtnPlaybackSecondLine
            // 
            this.rbtnPlaybackSecondLine.AutoSize = true;
            this.rbtnPlaybackSecondLine.Location = new System.Drawing.Point(145, 18);
            this.rbtnPlaybackSecondLine.Name = "rbtnPlaybackSecondLine";
            this.rbtnPlaybackSecondLine.Size = new System.Drawing.Size(116, 17);
            this.rbtnPlaybackSecondLine.TabIndex = 3;
            this.rbtnPlaybackSecondLine.TabStop = true;
            this.rbtnPlaybackSecondLine.Text = "Prefer Second Line";
            this.rbtnPlaybackSecondLine.UseVisualStyleBackColor = true;
            // 
            // rbtnPlaybackFirstLine
            // 
            this.rbtnPlaybackFirstLine.AutoSize = true;
            this.rbtnPlaybackFirstLine.Location = new System.Drawing.Point(7, 19);
            this.rbtnPlaybackFirstLine.Name = "rbtnPlaybackFirstLine";
            this.rbtnPlaybackFirstLine.Size = new System.Drawing.Size(98, 17);
            this.rbtnPlaybackFirstLine.TabIndex = 2;
            this.rbtnPlaybackFirstLine.TabStop = true;
            this.rbtnPlaybackFirstLine.Text = "Prefer First Line";
            this.rbtnPlaybackFirstLine.UseVisualStyleBackColor = true;
            // 
            // gbxGeneral
            // 
            this.gbxGeneral.Controls.Add(this.rbtnGeneralSecondLine);
            this.gbxGeneral.Controls.Add(this.rbtnGeneralFirstLine);
            this.gbxGeneral.Location = new System.Drawing.Point(7, 20);
            this.gbxGeneral.Name = "gbxGeneral";
            this.gbxGeneral.Size = new System.Drawing.Size(296, 43);
            this.gbxGeneral.TabIndex = 0;
            this.gbxGeneral.TabStop = false;
            this.gbxGeneral.Text = "General";
            // 
            // rbtnGeneralSecondLine
            // 
            this.rbtnGeneralSecondLine.AutoSize = true;
            this.rbtnGeneralSecondLine.Location = new System.Drawing.Point(145, 19);
            this.rbtnGeneralSecondLine.Name = "rbtnGeneralSecondLine";
            this.rbtnGeneralSecondLine.Size = new System.Drawing.Size(116, 17);
            this.rbtnGeneralSecondLine.TabIndex = 1;
            this.rbtnGeneralSecondLine.TabStop = true;
            this.rbtnGeneralSecondLine.Text = "Prefer Second Line";
            this.rbtnGeneralSecondLine.UseVisualStyleBackColor = true;
            // 
            // rbtnGeneralFirstLine
            // 
            this.rbtnGeneralFirstLine.AutoSize = true;
            this.rbtnGeneralFirstLine.Location = new System.Drawing.Point(7, 20);
            this.rbtnGeneralFirstLine.Name = "rbtnGeneralFirstLine";
            this.rbtnGeneralFirstLine.Size = new System.Drawing.Size(98, 17);
            this.rbtnGeneralFirstLine.TabIndex = 0;
            this.rbtnGeneralFirstLine.TabStop = true;
            this.rbtnGeneralFirstLine.Text = "Prefer First Line";
            this.rbtnGeneralFirstLine.UseVisualStyleBackColor = true;
            // 
            // ImonLcd_AdvancedSetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 176);
            this.Controls.Add(this.gbxLineOptions);
            this.Controls.Add(this.btnOK);
            this.Name = "ImonLcd_AdvancedSetupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MiniDisplay - Setup - Advanced Settings";
            this.gbxLineOptions.ResumeLayout(false);
            this.gbxPlayback.ResumeLayout(false);
            this.gbxPlayback.PerformLayout();
            this.gbxGeneral.ResumeLayout(false);
            this.gbxGeneral.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private UserInterface.Controls.MPButton btnOK;
        private System.Windows.Forms.GroupBox gbxLineOptions;
        private System.Windows.Forms.GroupBox gbxPlayback;
        private System.Windows.Forms.RadioButton rbtnPlaybackSecondLine;
        private System.Windows.Forms.RadioButton rbtnPlaybackFirstLine;
        private System.Windows.Forms.GroupBox gbxGeneral;
        private System.Windows.Forms.RadioButton rbtnGeneralSecondLine;
        private System.Windows.Forms.RadioButton rbtnGeneralFirstLine;
    }
}