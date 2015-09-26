using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormSelectItems
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
      this.labelItems = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonOkay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonCancel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.listBoxItems = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListBox();
      this.SuspendLayout();
      // 
      // labelItems
      // 
      this.labelItems.AutoSize = true;
      this.labelItems.Location = new System.Drawing.Point(12, 9);
      this.labelItems.Name = "labelItems";
      this.labelItems.Size = new System.Drawing.Size(110, 13);
      this.labelItems.TabIndex = 0;
      this.labelItems.Text = "Please select an item:";
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
      // listBoxItems
      // 
      this.listBoxItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listBoxItems.FormattingEnabled = true;
      this.listBoxItems.Location = new System.Drawing.Point(15, 25);
      this.listBoxItems.Name = "listBoxItems";
      this.listBoxItems.Size = new System.Drawing.Size(241, 160);
      this.listBoxItems.TabIndex = 0;
      this.listBoxItems.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxItems_MouseDoubleClick);
      // 
      // FormSelectItems
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(269, 230);
      this.Controls.Add(this.listBoxItems);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOkay);
      this.Controls.Add(this.labelItems);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(200, 150);
      this.Name = "FormSelectItems";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Select Item";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPLabel labelItems;
    private MPButton buttonOkay;
    private MPButton buttonCancel;
    private MPListBox listBoxItems;
  }
}