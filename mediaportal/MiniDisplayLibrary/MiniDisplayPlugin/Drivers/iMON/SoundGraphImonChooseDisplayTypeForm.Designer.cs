namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    partial class SoundGraphImonChooseDisplayTypeForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SoundGraphImonChooseDisplayTypeForm));
            this.label1 = new System.Windows.Forms.Label();
            this.buttonLCD = new System.Windows.Forms.Button();
            this.buttonVFD = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(292, 43);
            this.label1.TabIndex = 0;
            this.label1.Text = "Your SoundGraph iMON display could not be initialiazed. Choose the type of displa" +
                "y you want to configure:";
            // 
            // buttonLCD
            // 
            this.buttonLCD.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.buttonLCD.Location = new System.Drawing.Point(12, 58);
            this.buttonLCD.Name = "buttonLCD";
            this.buttonLCD.Size = new System.Drawing.Size(116, 23);
            this.buttonLCD.TabIndex = 1;
            this.buttonLCD.Text = "LCD";
            this.buttonLCD.UseVisualStyleBackColor = true;
            // 
            // buttonVFD
            // 
            this.buttonVFD.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonVFD.Location = new System.Drawing.Point(188, 58);
            this.buttonVFD.Name = "buttonVFD";
            this.buttonVFD.Size = new System.Drawing.Size(116, 23);
            this.buttonVFD.TabIndex = 2;
            this.buttonVFD.Text = "VFD";
            this.buttonVFD.UseVisualStyleBackColor = true;
            // 
            // SoundGraphImonChooseDisplayTypeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 93);
            this.Controls.Add(this.buttonVFD);
            this.Controls.Add(this.buttonLCD);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SoundGraphImonChooseDisplayTypeForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose iMON display type";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonLCD;
        private System.Windows.Forms.Button buttonVFD;
    }
}