namespace MpeMaker.Dialogs
{
    partial class ParamEdit
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
          this.cmb_params = new System.Windows.Forms.ComboBox();
          this.label1 = new System.Windows.Forms.Label();
          this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
          this.label_desc = new System.Windows.Forms.Label();
          this.panel1 = new System.Windows.Forms.Panel();
          this.SuspendLayout();
          // 
          // cmb_params
          // 
          this.cmb_params.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
          this.cmb_params.FormattingEnabled = true;
          this.cmb_params.Location = new System.Drawing.Point(12, 23);
          this.cmb_params.Name = "cmb_params";
          this.cmb_params.Size = new System.Drawing.Size(417, 21);
          this.cmb_params.TabIndex = 0;
          this.cmb_params.SelectedIndexChanged += new System.EventHandler(this.cmb_params_SelectedIndexChanged);
          // 
          // label1
          // 
          this.label1.AutoSize = true;
          this.label1.Location = new System.Drawing.Point(12, 7);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(55, 13);
          this.label1.TabIndex = 1;
          this.label1.Text = "Parameter";
          // 
          // openFileDialog1
          // 
          this.openFileDialog1.Filter = "All files|*.*";
          // 
          // label_desc
          // 
          this.label_desc.Location = new System.Drawing.Point(12, 212);
          this.label_desc.Name = "label_desc";
          this.label_desc.Size = new System.Drawing.Size(417, 81);
          this.label_desc.TabIndex = 6;
          this.label_desc.Text = "label3";
          // 
          // panel1
          // 
          this.panel1.Location = new System.Drawing.Point(12, 50);
          this.panel1.Name = "panel1";
          this.panel1.Size = new System.Drawing.Size(420, 160);
          this.panel1.TabIndex = 7;
          // 
          // ParamEdit
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(441, 302);
          this.Controls.Add(this.panel1);
          this.Controls.Add(this.label_desc);
          this.Controls.Add(this.label1);
          this.Controls.Add(this.cmb_params);
          this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
          this.MaximizeBox = false;
          this.MinimizeBox = false;
          this.Name = "ParamEdit";
          this.Text = "Customize ";
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmb_params;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label label_desc;
        private System.Windows.Forms.Panel panel1;
    }
}