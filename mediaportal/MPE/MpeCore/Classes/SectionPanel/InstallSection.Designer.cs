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
      this.timer2 = new System.Windows.Forms.Timer(this.components);
      this.panel4 = new System.Windows.Forms.Panel();
      this.panel5 = new System.Windows.Forms.Panel();
      this.panel4.SuspendLayout();
      this.panel5.SuspendLayout();
      this.SuspendLayout();
      // 
      // button_next
      // 
      this.button_next.Text = "Next >";
      // 
      // timer1
      // 
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar1.Location = new System.Drawing.Point(7, 6);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(475, 18);
      this.progressBar1.TabIndex = 23;
      // 
      // lbl_curr_file
      // 
      this.lbl_curr_file.AutoSize = true;
      this.lbl_curr_file.Location = new System.Drawing.Point(12, 102);
      this.lbl_curr_file.Name = "lbl_curr_file";
      this.lbl_curr_file.Size = new System.Drawing.Size(0, 13);
      this.lbl_curr_file.TabIndex = 24;
      // 
      // lst_changes
      // 
      this.lst_changes.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lst_changes.FormattingEnabled = true;
      this.lst_changes.HorizontalScrollbar = true;
      this.lst_changes.Location = new System.Drawing.Point(0, 0);
      this.lst_changes.Name = "lst_changes";
      this.lst_changes.Size = new System.Drawing.Size(494, 185);
      this.lst_changes.TabIndex = 25;
      // 
      // timer2
      // 
      this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
      // 
      // panel4
      // 
      this.panel4.Controls.Add(this.progressBar1);
      this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel4.Location = new System.Drawing.Point(0, 65);
      this.panel4.Name = "panel4";
      this.panel4.Size = new System.Drawing.Size(494, 34);
      this.panel4.TabIndex = 26;
      // 
      // panel5
      // 
      this.panel5.Controls.Add(this.lst_changes);
      this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel5.Location = new System.Drawing.Point(0, 121);
      this.panel5.Name = "panel5";
      this.panel5.Size = new System.Drawing.Size(494, 185);
      this.panel5.TabIndex = 27;
      // 
      // InstallSection
      // 
      this.AcceptButton = this.button_next;
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.ClientSize = new System.Drawing.Size(494, 350);
      this.Controls.Add(this.panel5);
      this.Controls.Add(this.panel4);
      this.Controls.Add(this.lbl_curr_file);
      this.Name = "InstallSection";
      this.Text = "Extension Installer for   - 0.0.0.0";
      this.Shown += new System.EventHandler(this.InstallSection_Shown);
      this.Controls.SetChildIndex(this.lbl_curr_file, 0);
      this.Controls.SetChildIndex(this.panel4, 0);
      this.Controls.SetChildIndex(this.panel5, 0);
      this.panel4.ResumeLayout(false);
      this.panel5.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lbl_curr_file;
        private System.Windows.Forms.ListBox lst_changes;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
    }
}