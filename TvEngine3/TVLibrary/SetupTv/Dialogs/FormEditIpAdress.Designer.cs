namespace SetupTv.Sections
{
  partial class FormEditIpAdress
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
      this.mpComboBox1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.PortNoNumericTextBox = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.SuspendLayout();
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.BackColor = System.Drawing.SystemColors.Control;
      this.mpLabel1.Location = new System.Drawing.Point(12, 14);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(329, 13);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "Please select the IP address used by the server for RTSP streaming:";
      // 
      // mpButtonOK
      // 
      this.mpButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonOK.Location = new System.Drawing.Point(189, 140);
      this.mpButtonOK.Name = "mpButtonOK";
      this.mpButtonOK.Size = new System.Drawing.Size(75, 23);
      this.mpButtonOK.TabIndex = 1;
      this.mpButtonOK.Text = "OK";
      this.mpButtonOK.UseVisualStyleBackColor = true;
      this.mpButtonOK.Click += new System.EventHandler(this.button1_Click);
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(270, 140);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 2;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Click += new System.EventHandler(this.button2_Click);
      // 
      // mpComboBox1
      // 
      this.mpComboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBox1.FormattingEnabled = true;
      this.mpComboBox1.Location = new System.Drawing.Point(15, 41);
      this.mpComboBox1.Name = "mpComboBox1";
      this.mpComboBox1.Size = new System.Drawing.Size(330, 21);
      this.mpComboBox1.TabIndex = 0;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(12, 75);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(269, 13);
      this.mpLabel2.TabIndex = 3;
      this.mpLabel2.Text = "Please select the port for RTSP streaming (default 554):";
      // 
      // PortNoNumericTextBox
      // 
      this.PortNoNumericTextBox.Location = new System.Drawing.Point(15, 101);
      this.PortNoNumericTextBox.Name = "PortNoNumericTextBox";
      this.PortNoNumericTextBox.Size = new System.Drawing.Size(100, 20);
      this.PortNoNumericTextBox.TabIndex = 5;
      this.PortNoNumericTextBox.Text = "554";
      this.PortNoNumericTextBox.Value = 554;
      // 
      // FormEditIpAdress
      // 
      this.AcceptButton = this.mpButtonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(365, 175);
      this.Controls.Add(this.PortNoNumericTextBox);
      this.Controls.Add(this.mpLabel2);
      this.Controls.Add(this.mpComboBox1);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonOK);
      this.Controls.Add(this.mpLabel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormEditIpAdress";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Assign network interface for streaming";
      this.Load += new System.EventHandler(this.FormEditIpAdress_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBox1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonOK;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonCancel;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox PortNoNumericTextBox;
  }
}