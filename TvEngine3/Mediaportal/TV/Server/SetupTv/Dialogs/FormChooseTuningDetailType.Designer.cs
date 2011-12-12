namespace SetupTv.Dialogs
{
  partial class FormChooseTuningDetailType
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
      this.mpButtonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpRadioButton7 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpRadioButton6 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpRadioButton5 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpRadioButton4 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpRadioButton3 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpRadioButton2 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpRadioButton1 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(197, 227);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 52;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Click += new System.EventHandler(this.mpButtonCancel_Click);
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.Location = new System.Drawing.Point(116, 227);
      this.mpButtonOk.Name = "mpButtonOk";
      this.mpButtonOk.Size = new System.Drawing.Size(75, 23);
      this.mpButtonOk.TabIndex = 51;
      this.mpButtonOk.Text = "OK";
      this.mpButtonOk.UseVisualStyleBackColor = true;
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.mpRadioButton7);
      this.mpGroupBox1.Controls.Add(this.mpRadioButton6);
      this.mpGroupBox1.Controls.Add(this.mpRadioButton5);
      this.mpGroupBox1.Controls.Add(this.mpRadioButton4);
      this.mpGroupBox1.Controls.Add(this.mpRadioButton3);
      this.mpGroupBox1.Controls.Add(this.mpRadioButton2);
      this.mpGroupBox1.Controls.Add(this.mpRadioButton1);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(13, 13);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(259, 186);
      this.mpGroupBox1.TabIndex = 53;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Types";
      // 
      // mpRadioButton7
      // 
      this.mpRadioButton7.AutoSize = true;
      this.mpRadioButton7.Checked = true;
      this.mpRadioButton7.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRadioButton7.Location = new System.Drawing.Point(7, 19);
      this.mpRadioButton7.Name = "mpRadioButton7";
      this.mpRadioButton7.Size = new System.Drawing.Size(57, 17);
      this.mpRadioButton7.TabIndex = 6;
      this.mpRadioButton7.TabStop = true;
      this.mpRadioButton7.Text = "Analog";
      this.mpRadioButton7.UseVisualStyleBackColor = true;
      // 
      // mpRadioButton6
      // 
      this.mpRadioButton6.AutoSize = true;
      this.mpRadioButton6.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRadioButton6.Location = new System.Drawing.Point(6, 135);
      this.mpRadioButton6.Name = "mpRadioButton6";
      this.mpRadioButton6.Size = new System.Drawing.Size(59, 17);
      this.mpRadioButton6.TabIndex = 5;
      this.mpRadioButton6.Text = "DVB-IP";
      this.mpRadioButton6.UseVisualStyleBackColor = true;
      // 
      // mpRadioButton5
      // 
      this.mpRadioButton5.AutoSize = true;
      this.mpRadioButton5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRadioButton5.Location = new System.Drawing.Point(7, 112);
      this.mpRadioButton5.Name = "mpRadioButton5";
      this.mpRadioButton5.Size = new System.Drawing.Size(56, 17);
      this.mpRadioButton5.TabIndex = 4;
      this.mpRadioButton5.Text = "DVB-T";
      this.mpRadioButton5.UseVisualStyleBackColor = true;
      // 
      // mpRadioButton4
      // 
      this.mpRadioButton4.AutoSize = true;
      this.mpRadioButton4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRadioButton4.Location = new System.Drawing.Point(7, 89);
      this.mpRadioButton4.Name = "mpRadioButton4";
      this.mpRadioButton4.Size = new System.Drawing.Size(56, 17);
      this.mpRadioButton4.TabIndex = 3;
      this.mpRadioButton4.Text = "DVB-S";
      this.mpRadioButton4.UseVisualStyleBackColor = true;
      // 
      // mpRadioButton3
      // 
      this.mpRadioButton3.AutoSize = true;
      this.mpRadioButton3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRadioButton3.Location = new System.Drawing.Point(7, 66);
      this.mpRadioButton3.Name = "mpRadioButton3";
      this.mpRadioButton3.Size = new System.Drawing.Size(56, 17);
      this.mpRadioButton3.TabIndex = 2;
      this.mpRadioButton3.Text = "DVB-C";
      this.mpRadioButton3.UseVisualStyleBackColor = true;
      // 
      // mpRadioButton2
      // 
      this.mpRadioButton2.AutoSize = true;
      this.mpRadioButton2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRadioButton2.Location = new System.Drawing.Point(7, 43);
      this.mpRadioButton2.Name = "mpRadioButton2";
      this.mpRadioButton2.Size = new System.Drawing.Size(52, 17);
      this.mpRadioButton2.TabIndex = 1;
      this.mpRadioButton2.Text = "ATSC";
      this.mpRadioButton2.UseVisualStyleBackColor = true;
      // 
      // mpRadioButton1
      // 
      this.mpRadioButton1.AutoSize = true;
      this.mpRadioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRadioButton1.Location = new System.Drawing.Point(8, 158);
      this.mpRadioButton1.Name = "mpRadioButton1";
      this.mpRadioButton1.Size = new System.Drawing.Size(83, 17);
      this.mpRadioButton1.TabIndex = 0;
      this.mpRadioButton1.Text = "Web-Stream";
      this.mpRadioButton1.UseVisualStyleBackColor = true;
      // 
      // FormChooseTuningDetailType
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 262);
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormChooseTuningDetailType";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Select tuningdetail type";
      this.Load += new System.EventHandler(this.FormChooseTuningDetailType_Load);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPButton mpButtonCancel;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonOk;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPRadioButton mpRadioButton6;
    private MediaPortal.UserInterface.Controls.MPRadioButton mpRadioButton5;
    private MediaPortal.UserInterface.Controls.MPRadioButton mpRadioButton4;
    private MediaPortal.UserInterface.Controls.MPRadioButton mpRadioButton3;
    private MediaPortal.UserInterface.Controls.MPRadioButton mpRadioButton2;
    private MediaPortal.UserInterface.Controls.MPRadioButton mpRadioButton1;
    private MediaPortal.UserInterface.Controls.MPRadioButton mpRadioButton7;
  }
}
