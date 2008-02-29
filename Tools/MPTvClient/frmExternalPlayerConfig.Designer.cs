namespace MPTvClient
{
    partial class frmExternalPlayerConfig
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
          this.label1 = new System.Windows.Forms.Label();
          this.edExe = new System.Windows.Forms.TextBox();
          this.edParams = new System.Windows.Forms.TextBox();
          this.label2 = new System.Windows.Forms.Label();
          this.btnBrowse = new System.Windows.Forms.Button();
          this.btnOk = new System.Windows.Forms.Button();
          this.btnCancel = new System.Windows.Forms.Button();
          this.OpenDlg = new System.Windows.Forms.OpenFileDialog();
          this.cbURLOverride = new System.Windows.Forms.CheckBox();
          this.edVLCURL = new System.Windows.Forms.TextBox();
          this.label3 = new System.Windows.Forms.Label();
          this.SuspendLayout();
          // 
          // label1
          // 
          this.label1.AutoSize = true;
          this.label1.Location = new System.Drawing.Point(13, 16);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(75, 13);
          this.label1.TabIndex = 0;
          this.label1.Text = "Path to player:";
          // 
          // edExe
          // 
          this.edExe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.edExe.Location = new System.Drawing.Point(95, 13);
          this.edExe.Name = "edExe";
          this.edExe.Size = new System.Drawing.Size(176, 20);
          this.edExe.TabIndex = 2;
          // 
          // edParams
          // 
          this.edParams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.edParams.Location = new System.Drawing.Point(95, 39);
          this.edParams.Name = "edParams";
          this.edParams.Size = new System.Drawing.Size(207, 20);
          this.edParams.TabIndex = 4;
          // 
          // label2
          // 
          this.label2.AutoSize = true;
          this.label2.Location = new System.Drawing.Point(13, 42);
          this.label2.Name = "label2";
          this.label2.Size = new System.Drawing.Size(63, 13);
          this.label2.TabIndex = 3;
          this.label2.Text = "Parameters:";
          // 
          // btnBrowse
          // 
          this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.btnBrowse.Location = new System.Drawing.Point(274, 13);
          this.btnBrowse.Name = "btnBrowse";
          this.btnBrowse.Size = new System.Drawing.Size(27, 20);
          this.btnBrowse.TabIndex = 5;
          this.btnBrowse.Text = "...";
          this.btnBrowse.UseVisualStyleBackColor = true;
          this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
          // 
          // btnOk
          // 
          this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
          this.btnOk.Location = new System.Drawing.Point(34, 125);
          this.btnOk.Name = "btnOk";
          this.btnOk.Size = new System.Drawing.Size(75, 23);
          this.btnOk.TabIndex = 6;
          this.btnOk.Text = "Ok";
          this.btnOk.UseVisualStyleBackColor = true;
          // 
          // btnCancel
          // 
          this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
          this.btnCancel.Location = new System.Drawing.Point(174, 125);
          this.btnCancel.Name = "btnCancel";
          this.btnCancel.Size = new System.Drawing.Size(75, 23);
          this.btnCancel.TabIndex = 7;
          this.btnCancel.Text = "Cancel";
          this.btnCancel.UseVisualStyleBackColor = true;
          // 
          // OpenDlg
          // 
          this.OpenDlg.DefaultExt = "exe";
          this.OpenDlg.Filter = "Applications|*.exe";
          // 
          // cbURLOverride
          // 
          this.cbURLOverride.AutoSize = true;
          this.cbURLOverride.Location = new System.Drawing.Point(13, 68);
          this.cbURLOverride.Name = "cbURLOverride";
          this.cbURLOverride.Size = new System.Drawing.Size(277, 17);
          this.cbURLOverride.TabIndex = 8;
          this.cbURLOverride.Text = "Override streaming URL (useful if VLC Server is used)";
          this.cbURLOverride.UseVisualStyleBackColor = true;
          // 
          // edVLCURL
          // 
          this.edVLCURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.edVLCURL.Location = new System.Drawing.Point(91, 91);
          this.edVLCURL.Name = "edVLCURL";
          this.edVLCURL.Size = new System.Drawing.Size(210, 20);
          this.edVLCURL.TabIndex = 10;
          // 
          // label3
          // 
          this.label3.AutoSize = true;
          this.label3.Location = new System.Drawing.Point(36, 94);
          this.label3.Name = "label3";
          this.label3.Size = new System.Drawing.Size(55, 13);
          this.label3.TabIndex = 9;
          this.label3.Text = "VLC URL:";
          // 
          // frmExternalPlayerConfig
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(307, 163);
          this.Controls.Add(this.edVLCURL);
          this.Controls.Add(this.label3);
          this.Controls.Add(this.cbURLOverride);
          this.Controls.Add(this.btnCancel);
          this.Controls.Add(this.btnOk);
          this.Controls.Add(this.btnBrowse);
          this.Controls.Add(this.edParams);
          this.Controls.Add(this.label2);
          this.Controls.Add(this.edExe);
          this.Controls.Add(this.label1);
          this.Name = "frmExternalPlayerConfig";
          this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
          this.Text = "Configure external player";
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox edExe;
        private System.Windows.Forms.TextBox edParams;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.OpenFileDialog OpenDlg;
      private System.Windows.Forms.CheckBox cbURLOverride;
      private System.Windows.Forms.TextBox edVLCURL;
      private System.Windows.Forms.Label label3;
    }
}