using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormEnterText
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
      this.labelText = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.textBoxText = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.buttonOkay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonCancel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.SuspendLayout();
      // 
      // labelText
      // 
      this.labelText.AutoSize = true;
      this.labelText.Location = new System.Drawing.Point(12, 9);
      this.labelText.Name = "labelText";
      this.labelText.Size = new System.Drawing.Size(89, 13);
      this.labelText.TabIndex = 0;
      this.labelText.Text = "Please enter text:";
      // 
      // textBoxText
      // 
      this.textBoxText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxText.Location = new System.Drawing.Point(15, 25);
      this.textBoxText.Name = "textBoxText";
      this.textBoxText.Size = new System.Drawing.Size(242, 20);
      this.textBoxText.TabIndex = 0;
      // 
      // buttonOkay
      // 
      this.buttonOkay.Location = new System.Drawing.Point(15, 61);
      this.buttonOkay.Name = "buttonOkay";
      this.buttonOkay.Size = new System.Drawing.Size(75, 23);
      this.buttonOkay.TabIndex = 1;
      this.buttonOkay.Text = "&OK";
      this.buttonOkay.Click += new System.EventHandler(this.buttonOkay_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(182, 61);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 2;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // FormEnterText
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(269, 96);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOkay);
      this.Controls.Add(this.textBoxText);
      this.Controls.Add(this.labelText);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(200, 122);
      this.Name = "FormEnterText";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Enter Text";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPLabel labelText;
    private MPTextBox textBoxText;
    private MPButton buttonOkay;
    private MPButton buttonCancel;
  }
}