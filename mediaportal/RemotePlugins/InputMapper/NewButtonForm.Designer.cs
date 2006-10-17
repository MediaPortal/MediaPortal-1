namespace MediaPortal.InputDevices
{
  partial class NewButtonForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewButtonForm));
      this.textBoxButton = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.buttonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.labelButton = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelName = new MediaPortal.UserInterface.Controls.MPLabel();
      this.SuspendLayout();
      // 
      // textBoxButton
      // 
      this.textBoxButton.BorderColor = System.Drawing.Color.Empty;
      this.textBoxButton.Location = new System.Drawing.Point(88, 48);
      this.textBoxButton.Name = "textBoxButton";
      this.textBoxButton.Size = new System.Drawing.Size(176, 20);
      this.textBoxButton.TabIndex = 0;
      // 
      // textBoxName
      // 
      this.textBoxName.BorderColor = System.Drawing.Color.Empty;
      this.textBoxName.Location = new System.Drawing.Point(88, 80);
      this.textBoxName.Name = "textBoxName";
      this.textBoxName.Size = new System.Drawing.Size(176, 20);
      this.textBoxName.TabIndex = 1;
      // 
      // buttonOk
      // 
      this.buttonOk.Location = new System.Drawing.Point(120, 136);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(75, 23);
      this.buttonOk.TabIndex = 2;
      this.buttonOk.Text = "&OK";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Location = new System.Drawing.Point(208, 136);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 3;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // labelButton
      // 
      this.labelButton.AutoSize = true;
      this.labelButton.Location = new System.Drawing.Point(24, 48);
      this.labelButton.Name = "labelButton";
      this.labelButton.Size = new System.Drawing.Size(41, 13);
      this.labelButton.TabIndex = 4;
      this.labelButton.Text = "Button:";
      // 
      // labelName
      // 
      this.labelName.AutoSize = true;
      this.labelName.Location = new System.Drawing.Point(24, 80);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size(38, 13);
      this.labelName.TabIndex = 5;
      this.labelName.Text = "Name:";
      // 
      // NewButtonForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 170);
      this.Controls.Add(this.labelName);
      this.Controls.Add(this.labelButton);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOk);
      this.Controls.Add(this.textBoxName);
      this.Controls.Add(this.textBoxButton);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "NewButtonForm";
      this.Text = "Add new button";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPTextBox textBoxButton;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxName;
    private MediaPortal.UserInterface.Controls.MPButton buttonOk;
    private MediaPortal.UserInterface.Controls.MPButton buttonCancel;
    private MediaPortal.UserInterface.Controls.MPLabel labelButton;
    private MediaPortal.UserInterface.Controls.MPLabel labelName;
  }
}