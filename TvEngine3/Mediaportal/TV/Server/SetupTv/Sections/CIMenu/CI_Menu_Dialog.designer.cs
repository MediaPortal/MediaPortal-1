namespace SetupTv.Sections
{
    partial class CI_Menu_Dialog
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
          this.btnOk = new MediaPortal.UserInterface.Controls.MPButton();
          this.Title = new MediaPortal.UserInterface.Controls.MPLabel();
          this.Subtitle = new MediaPortal.UserInterface.Controls.MPLabel();
          this.BottomText = new MediaPortal.UserInterface.Controls.MPLabel();
          this.Choices = new System.Windows.Forms.ListBox();
          this.btnCloseMenu = new MediaPortal.UserInterface.Controls.MPButton();
          this.btnSendAnswer = new MediaPortal.UserInterface.Controls.MPButton();
          this.grpCIMenu = new System.Windows.Forms.GroupBox();
          this.CiAnswer = new MediaPortal.UserInterface.Controls.MPTextBox();
          this.CiRequest = new MediaPortal.UserInterface.Controls.MPLabel();
          this.lblComment = new MediaPortal.UserInterface.Controls.MPLabel();
          this.grpCIMenu.SuspendLayout();
          this.SuspendLayout();
          // 
          // btnOk
          // 
          this.btnOk.Location = new System.Drawing.Point(324, 17);
          this.btnOk.Name = "btnOk";
          this.btnOk.Size = new System.Drawing.Size(88, 23);
          this.btnOk.TabIndex = 0;
          this.btnOk.Text = "Open Menu";
          this.btnOk.UseVisualStyleBackColor = true;
          this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
          // 
          // Title
          // 
          this.Title.AutoSize = true;
          this.Title.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.Title.Location = new System.Drawing.Point(16, 16);
          this.Title.Name = "Title";
          this.Title.Size = new System.Drawing.Size(35, 15);
          this.Title.TabIndex = 5;
          this.Title.Text = "Title";
          // 
          // Subtitle
          // 
          this.Subtitle.AutoSize = true;
          this.Subtitle.Location = new System.Drawing.Point(19, 45);
          this.Subtitle.Name = "Subtitle";
          this.Subtitle.Size = new System.Drawing.Size(42, 13);
          this.Subtitle.TabIndex = 5;
          this.Subtitle.Text = "Subtitle";
          // 
          // BottomText
          // 
          this.BottomText.AutoSize = true;
          this.BottomText.Location = new System.Drawing.Point(18, 178);
          this.BottomText.Name = "BottomText";
          this.BottomText.Size = new System.Drawing.Size(40, 13);
          this.BottomText.TabIndex = 5;
          this.BottomText.Text = "Bottom";
          // 
          // Choices
          // 
          this.Choices.FormattingEnabled = true;
          this.Choices.Location = new System.Drawing.Point(12, 61);
          this.Choices.Name = "Choices";
          this.Choices.Size = new System.Drawing.Size(300, 121);
          this.Choices.TabIndex = 6;
          this.Choices.DoubleClick += new System.EventHandler(this.btnSendAnswer_Click);
          // 
          // btnCloseMenu
          // 
          this.btnCloseMenu.Enabled = false;
          this.btnCloseMenu.Location = new System.Drawing.Point(324, 90);
          this.btnCloseMenu.Name = "btnCloseMenu";
          this.btnCloseMenu.Size = new System.Drawing.Size(88, 23);
          this.btnCloseMenu.TabIndex = 0;
          this.btnCloseMenu.Text = "Back / Close";
          this.btnCloseMenu.UseVisualStyleBackColor = true;
          this.btnCloseMenu.Click += new System.EventHandler(this.btnCloseMenu_Click);
          // 
          // btnSendAnswer
          // 
          this.btnSendAnswer.Enabled = false;
          this.btnSendAnswer.Location = new System.Drawing.Point(324, 61);
          this.btnSendAnswer.Name = "btnSendAnswer";
          this.btnSendAnswer.Size = new System.Drawing.Size(88, 23);
          this.btnSendAnswer.TabIndex = 0;
          this.btnSendAnswer.Text = "Ok";
          this.btnSendAnswer.UseVisualStyleBackColor = true;
          this.btnSendAnswer.Click += new System.EventHandler(this.btnSendAnswer_Click);
          // 
          // grpCIMenu
          // 
          this.grpCIMenu.Controls.Add(this.CiAnswer);
          this.grpCIMenu.Controls.Add(this.BottomText);
          this.grpCIMenu.Controls.Add(this.CiRequest);
          this.grpCIMenu.Controls.Add(this.Title);
          this.grpCIMenu.Location = new System.Drawing.Point(3, 9);
          this.grpCIMenu.Name = "grpCIMenu";
          this.grpCIMenu.Size = new System.Drawing.Size(315, 273);
          this.grpCIMenu.TabIndex = 7;
          this.grpCIMenu.TabStop = false;
          this.grpCIMenu.Text = "CI Menu";
          // 
          // CiAnswer
          // 
          this.CiAnswer.Location = new System.Drawing.Point(9, 225);
          this.CiAnswer.Name = "CiAnswer";
          this.CiAnswer.Size = new System.Drawing.Size(300, 20);
          this.CiAnswer.TabIndex = 7;
          // 
          // CiRequest
          // 
          this.CiRequest.AutoSize = true;
          this.CiRequest.Location = new System.Drawing.Point(19, 208);
          this.CiRequest.Name = "CiRequest";
          this.CiRequest.Size = new System.Drawing.Size(56, 13);
          this.CiRequest.TabIndex = 6;
          this.CiRequest.Text = "CiRequest";
          // 
          // lblComment
          // 
          this.lblComment.AutoSize = true;
          this.lblComment.Location = new System.Drawing.Point(3, 289);
          this.lblComment.Name = "lblComment";
          this.lblComment.Size = new System.Drawing.Size(313, 13);
          this.lblComment.TabIndex = 8;
          this.lblComment.Text = "*) for a list of supported DVB cards refer to Team MediaPortal wiki";
          // 
          // CI_Menu_Dialog
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.Controls.Add(this.lblComment);
          this.Controls.Add(this.Choices);
          this.Controls.Add(this.Subtitle);
          this.Controls.Add(this.btnSendAnswer);
          this.Controls.Add(this.btnCloseMenu);
          this.Controls.Add(this.btnOk);
          this.Controls.Add(this.grpCIMenu);
          this.Name = "CI_Menu_Dialog";
          this.Size = new System.Drawing.Size(433, 329);
          this.Load += new System.EventHandler(this.CI_Menu_Dialog_Load);
          this.grpCIMenu.ResumeLayout(false);
          this.grpCIMenu.PerformLayout();
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private MediaPortal.UserInterface.Controls.MPButton btnOk;
        private MediaPortal.UserInterface.Controls.MPLabel Title;
        private MediaPortal.UserInterface.Controls.MPLabel Subtitle;
        private MediaPortal.UserInterface.Controls.MPLabel BottomText;
        private System.Windows.Forms.ListBox Choices;
        private MediaPortal.UserInterface.Controls.MPButton btnCloseMenu;
        private MediaPortal.UserInterface.Controls.MPButton btnSendAnswer;
        private System.Windows.Forms.GroupBox grpCIMenu;
        private MediaPortal.UserInterface.Controls.MPTextBox CiAnswer;
        private MediaPortal.UserInterface.Controls.MPLabel CiRequest;
        private MediaPortal.UserInterface.Controls.MPLabel lblComment;
    }
}