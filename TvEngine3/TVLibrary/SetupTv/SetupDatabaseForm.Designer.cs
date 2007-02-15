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
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.radioButton2 = new System.Windows.Forms.RadioButton();
      this.radioButton1 = new System.Windows.Forms.RadioButton();
      this.groupBox1.SuspendLayout();
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
      this.mpTextBoxServer.Location = new System.Drawing.Point(77, 80);
      this.mpTextBoxServer.Name = "mpTextBoxServer";
      this.mpTextBoxServer.Size = new System.Drawing.Size(174, 20);
      this.mpTextBoxServer.TabIndex = 2;
      this.mpTextBoxServer.Text = "localhost\\SQLEXPRESS";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(16, 119);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(33, 13);
      this.mpLabel2.TabIndex = 2;
      this.mpLabel2.Text = "Login";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(16, 146);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(56, 13);
      this.mpLabel3.TabIndex = 3;
      this.mpLabel3.Text = "Password:";
      this.mpLabel3.Click += new System.EventHandler(this.mpLabel3_Click);
      // 
      // mpTextBoxUserId
      // 
      this.mpTextBoxUserId.Location = new System.Drawing.Point(77, 116);
      this.mpTextBoxUserId.Name = "mpTextBoxUserId";
      this.mpTextBoxUserId.Size = new System.Drawing.Size(100, 20);
      this.mpTextBoxUserId.TabIndex = 3;
      this.mpTextBoxUserId.Text = "sa";
      // 
      // mpTextBoxPassword
      // 
      this.mpTextBoxPassword.Location = new System.Drawing.Point(77, 143);
      this.mpTextBoxPassword.Name = "mpTextBoxPassword";
      this.mpTextBoxPassword.Size = new System.Drawing.Size(100, 20);
      this.mpTextBoxPassword.TabIndex = 4;
      // 
      // mpButtonSave
      // 
      this.mpButtonSave.Location = new System.Drawing.Point(208, 219);
      this.mpButtonSave.Name = "mpButtonSave";
      this.mpButtonSave.Size = new System.Drawing.Size(75, 23);
      this.mpButtonSave.TabIndex = 1;
      this.mpButtonSave.Text = "Save";
      this.mpButtonSave.UseVisualStyleBackColor = true;
      this.mpButtonSave.Click += new System.EventHandler(this.mpButtonSave_Click);
      // 
      // mpButtonTest
      // 
      this.mpButtonTest.Location = new System.Drawing.Point(127, 219);
      this.mpButtonTest.Name = "mpButtonTest";
      this.mpButtonTest.Size = new System.Drawing.Size(75, 23);
      this.mpButtonTest.TabIndex = 0;
      this.mpButtonTest.Text = "Test";
      this.mpButtonTest.UseVisualStyleBackColor = true;
      this.mpButtonTest.Click += new System.EventHandler(this.mpButtonTest_Click);
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(16, 83);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(58, 13);
      this.mpLabel4.TabIndex = 8;
      this.mpLabel4.Text = "Hostname:";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.radioButton2);
      this.groupBox1.Controls.Add(this.mpLabel4);
      this.groupBox1.Controls.Add(this.radioButton1);
      this.groupBox1.Controls.Add(this.mpTextBoxServer);
      this.groupBox1.Controls.Add(this.mpLabel2);
      this.groupBox1.Controls.Add(this.mpTextBoxPassword);
      this.groupBox1.Controls.Add(this.mpLabel3);
      this.groupBox1.Controls.Add(this.mpTextBoxUserId);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(270, 201);
      this.groupBox1.TabIndex = 9;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Database Server settings";
      this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
      // 
      // radioButton2
      // 
      this.radioButton2.AutoSize = true;
      this.radioButton2.Location = new System.Drawing.Point(23, 45);
      this.radioButton2.Name = "radioButton2";
      this.radioButton2.Size = new System.Drawing.Size(72, 17);
      this.radioButton2.TabIndex = 1;
      this.radioButton2.TabStop = true;
      this.radioButton2.Text = "My SQL 5";
      this.radioButton2.UseVisualStyleBackColor = true;
      this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
      // 
      // radioButton1
      // 
      this.radioButton1.AutoSize = true;
      this.radioButton1.Location = new System.Drawing.Point(23, 22);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(153, 17);
      this.radioButton1.TabIndex = 0;
      this.radioButton1.TabStop = true;
      this.radioButton1.Text = "Microsoft SQL Server 2005";
      this.radioButton1.UseVisualStyleBackColor = true;
      this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
      // 
      // SetupDatabaseForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(312, 270);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.mpButtonTest);
      this.Controls.Add(this.mpButtonSave);
      this.Controls.Add(this.mpLabel1);
      this.Name = "SetupDatabaseForm";
      this.Text = "Setup database";
      this.Load += new System.EventHandler(this.SetupDatabaseForm_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
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
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton radioButton1;
    private System.Windows.Forms.RadioButton radioButton2;
  }
}