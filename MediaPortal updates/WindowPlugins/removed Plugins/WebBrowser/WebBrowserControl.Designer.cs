namespace MediaPortal.GUI.WebBrowser
{
    partial class WebBrowserControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebBrowserControl));
            this.axMozillaBrowser1 = new AxMOZILLACONTROLLib.AxMozillaBrowser();
            ((System.ComponentModel.ISupportInitialize)(this.axMozillaBrowser1)).BeginInit();
            this.SuspendLayout();
            // 
            // axMozillaBrowser1
            // 
            this.axMozillaBrowser1.Enabled = true;
            this.axMozillaBrowser1.Location = new System.Drawing.Point(0, 0);
            this.axMozillaBrowser1.Margin = new System.Windows.Forms.Padding(0);
            this.axMozillaBrowser1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axMozillaBrowser1.OcxState")));
            this.axMozillaBrowser1.Size = new System.Drawing.Size(720, 473);
            this.axMozillaBrowser1.TabIndex = 0;
            // 
            // WebBrowserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.axMozillaBrowser1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "WebBrowserControl";
            this.Size = new System.Drawing.Size(720, 473);
            this.Layout += new System.Windows.Forms.LayoutEventHandler(this.WebBrowserControl_Layout);
            ((System.ComponentModel.ISupportInitialize)(this.axMozillaBrowser1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxMOZILLACONTROLLib.AxMozillaBrowser axMozillaBrowser1;
    }
}
