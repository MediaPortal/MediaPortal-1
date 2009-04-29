namespace SetupTv.Sections
{
  partial class GroupNameForm
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
      this.mpTextBox1 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpButtonSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(12, 9);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(199, 13);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "Please enter the name for the new group";
      // 
      // mpTextBox1
      // 
      this.mpTextBox1.Location = new System.Drawing.Point(15, 25);
      this.mpTextBox1.Name = "mpTextBox1";
      this.mpTextBox1.Size = new System.Drawing.Size(242, 20);
      this.mpTextBox1.TabIndex = 0;
      this.mpTextBox1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.mpTextBox1_KeyUp);
      // 
      // mpButtonSave
      // 
      this.mpButtonSave.Location = new System.Drawing.Point(15, 61);
      this.mpButtonSave.Name = "mpButtonSave";
      this.mpButtonSave.Size = new System.Drawing.Size(75, 23);
      this.mpButtonSave.TabIndex = 1;
      this.mpButtonSave.Text = "Save";
      this.mpButtonSave.UseVisualStyleBackColor = true;
      this.mpButtonSave.Click += new System.EventHandler(this.mpButton1_Click);
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(182, 61);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 2;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Click += new System.EventHandler(this.mpButton2_Click);
      // 
      // GroupNameForm
      // 
      this.AcceptButton = this.mpButtonSave;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(269, 96);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonSave);
      this.Controls.Add(this.mpTextBox1);
      this.Controls.Add(this.mpLabel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "GroupNameForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Enter name for new group";
      this.Load += new System.EventHandler(this.GroupName_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBox1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonSave;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonCancel;
  }
}