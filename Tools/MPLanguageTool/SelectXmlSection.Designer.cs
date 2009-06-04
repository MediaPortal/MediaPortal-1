namespace MPLanguageTool
{
  partial class SelectXmlSection
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
      this.cbXmlSection = new System.Windows.Forms.ComboBox();
      this.labelSection = new System.Windows.Forms.Label();
      this.buttonOk = new System.Windows.Forms.Button();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // cbXmlSection
      // 
      this.cbXmlSection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbXmlSection.FormattingEnabled = true;
      this.cbXmlSection.Location = new System.Drawing.Point(80, 24);
      this.cbXmlSection.Name = "cbXmlSection";
      this.cbXmlSection.Size = new System.Drawing.Size(151, 21);
      this.cbXmlSection.TabIndex = 0;
      // 
      // labelSection
      // 
      this.labelSection.AutoSize = true;
      this.labelSection.Location = new System.Drawing.Point(3, 27);
      this.labelSection.Name = "labelSection";
      this.labelSection.Size = new System.Drawing.Size(71, 13);
      this.labelSection.TabIndex = 1;
      this.labelSection.Text = "XML Section:";
      // 
      // buttonOk
      // 
      this.buttonOk.Location = new System.Drawing.Point(25, 67);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(75, 23);
      this.buttonOk.TabIndex = 2;
      this.buttonOk.Text = "Ok";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Location = new System.Drawing.Point(154, 67);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 3;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // SelectXmlSection
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(259, 107);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOk);
      this.Controls.Add(this.labelSection);
      this.Controls.Add(this.cbXmlSection);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SelectXmlSection";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Select";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox cbXmlSection;
    private System.Windows.Forms.Label labelSection;
    private System.Windows.Forms.Button buttonOk;
    private System.Windows.Forms.Button buttonCancel;
  }
}