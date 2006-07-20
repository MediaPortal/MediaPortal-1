namespace SetupTv
{
  partial class SetupDatabaseForm
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
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTextBoxServer = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTextBoxUserId = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpTextBoxPassword = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpButtonSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonTest = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(23, 34);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(41, 13);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "Server:";
      // 
      // mpTextBoxServer
      // 
      this.mpTextBoxServer.Location = new System.Drawing.Point(83, 31);
      this.mpTextBoxServer.Name = "mpTextBoxServer";
      this.mpTextBoxServer.Size = new System.Drawing.Size(174, 20);
      this.mpTextBoxServer.TabIndex = 1;
      this.mpTextBoxServer.Text = "localhost\\SQLEXPRESS";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(22, 67);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(44, 13);
      this.mpLabel2.TabIndex = 2;
      this.mpLabel2.Text = "User Id:";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(22, 94);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(56, 13);
      this.mpLabel3.TabIndex = 3;
      this.mpLabel3.Text = "Password:";
      // 
      // mpTextBoxUserId
      // 
      this.mpTextBoxUserId.Location = new System.Drawing.Point(83, 64);
      this.mpTextBoxUserId.Name = "mpTextBoxUserId";
      this.mpTextBoxUserId.Size = new System.Drawing.Size(100, 20);
      this.mpTextBoxUserId.TabIndex = 4;
      this.mpTextBoxUserId.Text = "sa";
      // 
      // mpTextBoxPassword
      // 
      this.mpTextBoxPassword.Location = new System.Drawing.Point(83, 91);
      this.mpTextBoxPassword.Name = "mpTextBoxPassword";
      this.mpTextBoxPassword.Size = new System.Drawing.Size(100, 20);
      this.mpTextBoxPassword.TabIndex = 5;
      // 
      // mpButtonSave
      // 
      this.mpButtonSave.Location = new System.Drawing.Point(199, 157);
      this.mpButtonSave.Name = "mpButtonSave";
      this.mpButtonSave.Size = new System.Drawing.Size(75, 23);
      this.mpButtonSave.TabIndex = 6;
      this.mpButtonSave.Text = "Save";
      this.mpButtonSave.UseVisualStyleBackColor = true;
      this.mpButtonSave.Click += new System.EventHandler(this.mpButtonSave_Click);
      // 
      // mpButtonTest
      // 
      this.mpButtonTest.Location = new System.Drawing.Point(199, 128);
      this.mpButtonTest.Name = "mpButtonTest";
      this.mpButtonTest.Size = new System.Drawing.Size(75, 23);
      this.mpButtonTest.TabIndex = 7;
      this.mpButtonTest.Text = "Test";
      this.mpButtonTest.UseVisualStyleBackColor = true;
      this.mpButtonTest.Click += new System.EventHandler(this.mpButtonTest_Click);
      // 
      // SetupDatabaseForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(299, 203);
      this.Controls.Add(this.mpButtonTest);
      this.Controls.Add(this.mpButtonSave);
      this.Controls.Add(this.mpTextBoxPassword);
      this.Controls.Add(this.mpTextBoxUserId);
      this.Controls.Add(this.mpLabel3);
      this.Controls.Add(this.mpLabel2);
      this.Controls.Add(this.mpTextBoxServer);
      this.Controls.Add(this.mpLabel1);
      this.Name = "SetupDatabaseForm";
      this.Text = "Setup database";
      this.Load += new System.EventHandler(this.SetupDatabaseForm_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBoxServer;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBoxUserId;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBoxPassword;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonSave;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonTest;
  }
}