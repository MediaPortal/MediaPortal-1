namespace MpeCore.Classes.SectionPanel
{
    partial class InstallSection
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
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lbl_curr_file = new System.Windows.Forms.Label();
            this.lst_changes = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // button_back
            // 
            this.button_back.TabIndex = 1;
            // 
            // button_next
            // 
            this.button_next.TabIndex = 0;
            this.button_next.Text = "Next>";
            // 
            // button_cancel
            // 
            this.button_cancel.TabIndex = 2;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(8, 81);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(475, 18);
            this.progressBar1.TabIndex = 23;
            // 
            // lbl_curr_file
            // 
            this.lbl_curr_file.AutoSize = true;
            this.lbl_curr_file.Location = new System.Drawing.Point(12, 102);
            this.lbl_curr_file.Name = "lbl_curr_file";
            this.lbl_curr_file.Size = new System.Drawing.Size(35, 13);
            this.lbl_curr_file.TabIndex = 24;
            this.lbl_curr_file.Text = "label1";
            // 
            // lst_changes
            // 
            this.lst_changes.FormattingEnabled = true;
            this.lst_changes.HorizontalScrollbar = true;
            this.lst_changes.Location = new System.Drawing.Point(8, 118);
            this.lst_changes.Name = "lst_changes";
            this.lst_changes.Size = new System.Drawing.Size(475, 173);
            this.lst_changes.TabIndex = 25;
            // 
            // InstallSection
            // 
            this.AcceptButton = this.button_next;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 350);
            this.ControlBox = false;
            this.Controls.Add(this.lst_changes);
            this.Controls.Add(this.lbl_curr_file);
            this.Controls.Add(this.progressBar1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "InstallSection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Extension Installer for   - 0.0.0.0";
            this.Load += new System.EventHandler(this.InstallSection_Load);
            this.Shown += new System.EventHandler(this.InstallSection_Shown);
            this.Controls.SetChildIndex(this.progressBar1, 0);
            this.Controls.SetChildIndex(this.lbl_curr_file, 0);
            this.Controls.SetChildIndex(this.lst_changes, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lbl_curr_file;
        private System.Windows.Forms.ListBox lst_changes;
    }
}