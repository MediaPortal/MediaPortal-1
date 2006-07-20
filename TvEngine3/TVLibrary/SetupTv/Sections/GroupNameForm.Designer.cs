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
      this.mpButton1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(23, 13);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(199, 13);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "Please enter the name for the new group";
      // 
      // mpTextBox1
      // 
      this.mpTextBox1.Location = new System.Drawing.Point(26, 48);
      this.mpTextBox1.Name = "mpTextBox1";
      this.mpTextBox1.Size = new System.Drawing.Size(238, 20);
      this.mpTextBox1.TabIndex = 1;
      // 
      // mpButton1
      // 
      this.mpButton1.Location = new System.Drawing.Point(189, 92);
      this.mpButton1.Name = "mpButton1";
      this.mpButton1.Size = new System.Drawing.Size(75, 23);
      this.mpButton1.TabIndex = 2;
      this.mpButton1.Text = "Save";
      this.mpButton1.UseVisualStyleBackColor = true;
      this.mpButton1.Click += new System.EventHandler(this.mpButton1_Click);
      // 
      // GroupName
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(282, 138);
      this.Controls.Add(this.mpButton1);
      this.Controls.Add(this.mpTextBox1);
      this.Controls.Add(this.mpLabel1);
      this.Name = "GroupName";
      this.Text = "Enter name for new group";
      this.Load += new System.EventHandler(this.GroupName_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBox1;
    private MediaPortal.UserInterface.Controls.MPButton mpButton1;
  }
}