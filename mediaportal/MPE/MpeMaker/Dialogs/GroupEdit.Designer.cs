namespace MpeMaker.Dialogs
{
    partial class GroupEdit
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
          this.txt_name = new System.Windows.Forms.TextBox();
          this.label1 = new System.Windows.Forms.Label();
          this.btn_cancel = new System.Windows.Forms.Button();
          this.btn_ok = new System.Windows.Forms.Button();
          this.label2 = new System.Windows.Forms.Label();
          this.txt_displayname = new System.Windows.Forms.TextBox();
          this.SuspendLayout();
          // 
          // txt_name
          // 
          this.txt_name.Location = new System.Drawing.Point(12, 22);
          this.txt_name.Name = "txt_name";
          this.txt_name.Size = new System.Drawing.Size(359, 20);
          this.txt_name.TabIndex = 0;
          this.txt_name.TextChanged += new System.EventHandler(this.txt_name_TextChanged);
          // 
          // label1
          // 
          this.label1.AutoSize = true;
          this.label1.Location = new System.Drawing.Point(12, 6);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(65, 13);
          this.label1.TabIndex = 1;
          this.label1.Text = "Group name";
          // 
          // btn_cancel
          // 
          this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
          this.btn_cancel.Location = new System.Drawing.Point(296, 95);
          this.btn_cancel.Name = "btn_cancel";
          this.btn_cancel.Size = new System.Drawing.Size(75, 23);
          this.btn_cancel.TabIndex = 2;
          this.btn_cancel.Text = "Cancel";
          this.btn_cancel.UseVisualStyleBackColor = true;
          // 
          // btn_ok
          // 
          this.btn_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
          this.btn_ok.Location = new System.Drawing.Point(215, 95);
          this.btn_ok.Name = "btn_ok";
          this.btn_ok.Size = new System.Drawing.Size(75, 23);
          this.btn_ok.TabIndex = 3;
          this.btn_ok.Text = "Ok";
          this.btn_ok.UseVisualStyleBackColor = true;
          // 
          // label2
          // 
          this.label2.AutoSize = true;
          this.label2.Location = new System.Drawing.Point(12, 45);
          this.label2.Name = "label2";
          this.label2.Size = new System.Drawing.Size(70, 13);
          this.label2.TabIndex = 4;
          this.label2.Text = "Display name";
          // 
          // txt_displayname
          // 
          this.txt_displayname.Location = new System.Drawing.Point(12, 61);
          this.txt_displayname.Name = "txt_displayname";
          this.txt_displayname.Size = new System.Drawing.Size(359, 20);
          this.txt_displayname.TabIndex = 5;
          // 
          // GroupEdit
          // 
          this.AcceptButton = this.btn_ok;
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.CancelButton = this.btn_cancel;
          this.ClientSize = new System.Drawing.Size(383, 130);
          this.Controls.Add(this.txt_displayname);
          this.Controls.Add(this.label2);
          this.Controls.Add(this.btn_ok);
          this.Controls.Add(this.btn_cancel);
          this.Controls.Add(this.txt_name);
          this.Controls.Add(this.label1);
          this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
          this.MaximizeBox = false;
          this.MinimizeBox = false;
          this.Name = "GroupEdit";
          this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
          this.Text = "Group";
          this.Load += new System.EventHandler(this.GroupEdit_Load);
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_name;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_displayname;
    }
}