namespace MediaPortal.VideoLanPlugin
{
    partial class VideoLanControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoLanControl));
            this.axVLCPlugin1 = new AxAXVLC.AxVLCPlugin();
            ((System.ComponentModel.ISupportInitialize)(this.axVLCPlugin1)).BeginInit();
            this.SuspendLayout();
            // 
            // axVLCPlugin1
            // 
            this.axVLCPlugin1.Enabled = true;
            this.axVLCPlugin1.Location = new System.Drawing.Point(0, 0);
            this.axVLCPlugin1.Name = "axVLCPlugin1";
            this.axVLCPlugin1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axVLCPlugin1.OcxState")));
            this.axVLCPlugin1.Size = new System.Drawing.Size(100, 100);
            this.axVLCPlugin1.TabIndex = 0;
            // 
            // VideoLanControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.axVLCPlugin1);
            this.Name = "VideoLanControl";
            this.Size = new System.Drawing.Size(100, 100);
            ((System.ComponentModel.ISupportInitialize)(this.axVLCPlugin1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxAXVLC.AxVLCPlugin axVLCPlugin1;

    }
}
