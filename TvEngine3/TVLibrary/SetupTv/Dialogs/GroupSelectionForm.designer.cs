namespace SetupTv.Sections
{
  partial class GroupSelectionForm
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
      this.mpButtonOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.listBox1 = new System.Windows.Forms.ListBox();
      this.SuspendLayout();
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(12, 9);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(109, 13);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "Please select a group";
      // 
      // mpButtonOK
      // 
      this.mpButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonOK.Location = new System.Drawing.Point(15, 195);
      this.mpButtonOK.Name = "mpButtonOK";
      this.mpButtonOK.Size = new System.Drawing.Size(75, 23);
      this.mpButtonOK.TabIndex = 1;
      this.mpButtonOK.Text = "OK";
      this.mpButtonOK.UseVisualStyleBackColor = true;
      this.mpButtonOK.Click += new System.EventHandler(this.mpButton1_Click);
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(182, 195);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 2;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Click += new System.EventHandler(this.mpButton2_Click);
      // 
      // listBox1
      // 
      this.listBox1.FormattingEnabled = true;
      this.listBox1.Location = new System.Drawing.Point(15, 25);
      this.listBox1.Name = "listBox1";
      this.listBox1.Size = new System.Drawing.Size(241, 160);
      this.listBox1.TabIndex = 0;
      // 
      // GroupSelectionForm
      // 
      this.AcceptButton = this.mpButtonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(269, 230);
      this.Controls.Add(this.listBox1);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonOK);
      this.Controls.Add(this.mpLabel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "GroupSelectionForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Group selection";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonOK;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonCancel;
    private System.Windows.Forms.ListBox listBox1;
  }
}