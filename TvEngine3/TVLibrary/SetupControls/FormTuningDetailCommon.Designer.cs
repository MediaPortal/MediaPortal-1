namespace SetupControls
{
  partial class FormTuningDetailCommon
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
      this.SuspendLayout();
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(197, 227);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 117;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Click += new System.EventHandler(this.mpButtonCancel_Click);
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonOk.Location = new System.Drawing.Point(116, 227);
      this.mpButtonOk.Name = "mpButtonOk";
      this.mpButtonOk.Size = new System.Drawing.Size(75, 23);
      this.mpButtonOk.TabIndex = 116;
      this.mpButtonOk.Text = "OK";
      this.mpButtonOk.UseVisualStyleBackColor = true;
      // 
      // FormTuningDetailCommon
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 262);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormTuningDetailCommon";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "FormTuningDetailCommon";
      this.ResumeLayout(false);

    }

    #endregion

    protected MediaPortal.UserInterface.Controls.MPButton mpButtonCancel;
    protected MediaPortal.UserInterface.Controls.MPButton mpButtonOk;
  }
}
