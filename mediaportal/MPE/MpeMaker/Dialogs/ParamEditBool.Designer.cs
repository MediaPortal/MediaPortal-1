namespace MpeMaker.Dialogs
{
    partial class ParamEditBool
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
            this.radio_Yes = new System.Windows.Forms.RadioButton();
            this.radio_No = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // radio_Yes
            // 
            this.radio_Yes.AutoSize = true;
            this.radio_Yes.Location = new System.Drawing.Point(120, 31);
            this.radio_Yes.Name = "radio_Yes";
            this.radio_Yes.Size = new System.Drawing.Size(43, 17);
            this.radio_Yes.TabIndex = 0;
            this.radio_Yes.TabStop = true;
            this.radio_Yes.Text = "Yes";
            this.radio_Yes.UseVisualStyleBackColor = true;
            this.radio_Yes.CheckedChanged += new System.EventHandler(this.radio_No_CheckedChanged);
            // 
            // radio_No
            // 
            this.radio_No.AutoSize = true;
            this.radio_No.Location = new System.Drawing.Point(120, 68);
            this.radio_No.Name = "radio_No";
            this.radio_No.Size = new System.Drawing.Size(39, 17);
            this.radio_No.TabIndex = 1;
            this.radio_No.TabStop = true;
            this.radio_No.Text = "No";
            this.radio_No.UseVisualStyleBackColor = true;
            this.radio_No.CheckedChanged += new System.EventHandler(this.radio_No_CheckedChanged);
            // 
            // ParamEditBool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.radio_No);
            this.Controls.Add(this.radio_Yes);
            this.Name = "ParamEditBool";
            this.Size = new System.Drawing.Size(341, 150);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radio_Yes;
        private System.Windows.Forms.RadioButton radio_No;
    }
}
