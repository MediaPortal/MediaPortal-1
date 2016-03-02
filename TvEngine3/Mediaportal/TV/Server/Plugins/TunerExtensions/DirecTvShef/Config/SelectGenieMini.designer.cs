using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Config
{
  partial class SelectGenieMini
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
      this.labelInstructions = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonOkay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonCancel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.listBoxGenieMinis = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListBox();
      this.SuspendLayout();
      // 
      // labelInstructions
      // 
      this.labelInstructions.AutoSize = true;
      this.labelInstructions.Location = new System.Drawing.Point(12, 9);
      this.labelInstructions.Name = "labelInstructions";
      this.labelInstructions.Size = new System.Drawing.Size(144, 13);
      this.labelInstructions.TabIndex = 0;
      this.labelInstructions.Text = "Please select the Genie Mini:";
      // 
      // buttonOkay
      // 
      this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonOkay.Location = new System.Drawing.Point(15, 195);
      this.buttonOkay.Name = "buttonOkay";
      this.buttonOkay.Size = new System.Drawing.Size(75, 23);
      this.buttonOkay.TabIndex = 1;
      this.buttonOkay.Text = "&OK";
      this.buttonOkay.UseVisualStyleBackColor = true;
      this.buttonOkay.Click += new System.EventHandler(this.buttonOkay_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(182, 195);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 2;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // listBoxGenieMinis
      // 
      this.listBoxGenieMinis.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listBoxGenieMinis.FormattingEnabled = true;
      this.listBoxGenieMinis.Location = new System.Drawing.Point(15, 25);
      this.listBoxGenieMinis.Name = "listBoxGenieMinis";
      this.listBoxGenieMinis.Size = new System.Drawing.Size(241, 160);
      this.listBoxGenieMinis.TabIndex = 0;
      this.listBoxGenieMinis.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxItems_MouseDoubleClick);
      // 
      // SelectGenieMini
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(269, 230);
      this.Controls.Add(this.listBoxGenieMinis);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOkay);
      this.Controls.Add(this.labelInstructions);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(200, 150);
      this.Name = "SelectGenieMini";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Select Genie Mini";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPLabel labelInstructions;
    private MPButton buttonOkay;
    private MPButton buttonCancel;
    private MPListBox listBoxGenieMinis;
  }
}