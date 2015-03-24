namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    partial class RtspConnectionPreference
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
            this.buttonDown = new System.Windows.Forms.Button();
            this.buttonRtspUp = new System.Windows.Forms.Button();
            this.listBoxRtspConnectionPreference = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // buttonDown
            // 
            this.buttonDown.BackgroundImage = global::TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Properties.Resources.Down;
            this.buttonDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDown.Location = new System.Drawing.Point(106, 36);
            this.buttonDown.Name = "buttonDown";
            this.buttonDown.Size = new System.Drawing.Size(20, 20);
            this.buttonDown.TabIndex = 23;
            this.buttonDown.UseVisualStyleBackColor = true;
            this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
            // 
            // buttonRtspUp
            // 
            this.buttonRtspUp.BackgroundImage = global::TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Properties.Resources.Up;
            this.buttonRtspUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonRtspUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRtspUp.Location = new System.Drawing.Point(106, 0);
            this.buttonRtspUp.Name = "buttonRtspUp";
            this.buttonRtspUp.Size = new System.Drawing.Size(20, 20);
            this.buttonRtspUp.TabIndex = 22;
            this.buttonRtspUp.UseVisualStyleBackColor = true;
            this.buttonRtspUp.Click += new System.EventHandler(this.buttonRtspUp_Click);
            // 
            // listBoxRtspConnectionPreference
            // 
            this.listBoxRtspConnectionPreference.FormattingEnabled = true;
            this.listBoxRtspConnectionPreference.Location = new System.Drawing.Point(0, 0);
            this.listBoxRtspConnectionPreference.Name = "listBoxRtspConnectionPreference";
            this.listBoxRtspConnectionPreference.Size = new System.Drawing.Size(100, 56);
            this.listBoxRtspConnectionPreference.TabIndex = 21;
            // 
            // RtspConnectionPreference
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonDown);
            this.Controls.Add(this.buttonRtspUp);
            this.Controls.Add(this.listBoxRtspConnectionPreference);
            this.Name = "RtspConnectionPreference";
            this.Size = new System.Drawing.Size(126, 56);
            this.Load += new System.EventHandler(this.RtspConnectionPreference_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonDown;
        private System.Windows.Forms.Button buttonRtspUp;
        private System.Windows.Forms.ListBox listBoxRtspConnectionPreference;
    }
}
