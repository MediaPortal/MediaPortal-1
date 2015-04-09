namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    partial class FilterProgramMapPIDControl
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
            this.checkBoxAllowFilteringProgramElements = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBoxAllowFilteringProgramElements
            // 
            this.checkBoxAllowFilteringProgramElements.AutoSize = true;
            this.checkBoxAllowFilteringProgramElements.Location = new System.Drawing.Point(3, 3);
            this.checkBoxAllowFilteringProgramElements.Name = "checkBoxAllowFilteringProgramElements";
            this.checkBoxAllowFilteringProgramElements.Size = new System.Drawing.Size(173, 17);
            this.checkBoxAllowFilteringProgramElements.TabIndex = 0;
            this.checkBoxAllowFilteringProgramElements.Text = "Allow filtering program elements";
            this.checkBoxAllowFilteringProgramElements.UseVisualStyleBackColor = true;
            // 
            // FilterProgramMapPIDControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkBoxAllowFilteringProgramElements);
            this.Name = "FilterProgramMapPIDControl";
            this.Size = new System.Drawing.Size(422, 148);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.CheckBox checkBoxAllowFilteringProgramElements;

    }
}
