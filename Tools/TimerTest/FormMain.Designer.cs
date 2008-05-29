namespace TimerTest
{
  partial class FormMain
  {
    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Vom Windows Form-Designer generierter Code

    /// <summary>
    /// Erforderliche Methode für die Designerunterstützung.
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      this.buttonStart = new System.Windows.Forms.Button();
      this.listBoxResults = new System.Windows.Forms.ListBox();
      this.buttonClose = new System.Windows.Forms.Button();
      this.checkBoxClearValues = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // buttonStart
      // 
      this.buttonStart.Location = new System.Drawing.Point(251, 235);
      this.buttonStart.Name = "buttonStart";
      this.buttonStart.Size = new System.Drawing.Size(75, 23);
      this.buttonStart.TabIndex = 0;
      this.buttonStart.Text = "&Start";
      this.buttonStart.UseVisualStyleBackColor = true;
      this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
      // 
      // listBoxResults
      // 
      this.listBoxResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listBoxResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.listBoxResults.FormattingEnabled = true;
      this.listBoxResults.Location = new System.Drawing.Point(12, 12);
      this.listBoxResults.Name = "listBoxResults";
      this.listBoxResults.Size = new System.Drawing.Size(395, 208);
      this.listBoxResults.TabIndex = 1;
      // 
      // buttonClose
      // 
      this.buttonClose.Location = new System.Drawing.Point(332, 235);
      this.buttonClose.Name = "buttonClose";
      this.buttonClose.Size = new System.Drawing.Size(75, 23);
      this.buttonClose.TabIndex = 2;
      this.buttonClose.Text = "&Close";
      this.buttonClose.UseVisualStyleBackColor = true;
      this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
      // 
      // checkBoxClearValues
      // 
      this.checkBoxClearValues.AutoSize = true;
      this.checkBoxClearValues.Location = new System.Drawing.Point(12, 239);
      this.checkBoxClearValues.Name = "checkBoxClearValues";
      this.checkBoxClearValues.Size = new System.Drawing.Size(101, 17);
      this.checkBoxClearValues.TabIndex = 3;
      this.checkBoxClearValues.Text = "Clear old values";
      this.checkBoxClearValues.UseVisualStyleBackColor = true;
      // 
      // FormMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(419, 270);
      this.Controls.Add(this.checkBoxClearValues);
      this.Controls.Add(this.buttonClose);
      this.Controls.Add(this.listBoxResults);
      this.Controls.Add(this.buttonStart);
      this.Name = "FormMain";
      this.Text = "TimerTest";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonStart;
    private System.Windows.Forms.ListBox listBoxResults;
    private System.Windows.Forms.Button buttonClose;
    private System.Windows.Forms.CheckBox checkBoxClearValues;
  }
}

