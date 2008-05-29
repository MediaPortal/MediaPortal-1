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
      this.lblMaxDesc = new System.Windows.Forms.Label();
      this.lblMaxAccurary = new System.Windows.Forms.Label();
      this.numLoopCount = new System.Windows.Forms.NumericUpDown();
      this.lblLoopCountDesc = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.numLoopCount)).BeginInit();
      this.SuspendLayout();
      // 
      // buttonStart
      // 
      this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonStart.Location = new System.Drawing.Point(321, 496);
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
      this.listBoxResults.Location = new System.Drawing.Point(12, 28);
      this.listBoxResults.Name = "listBoxResults";
      this.listBoxResults.Size = new System.Drawing.Size(465, 455);
      this.listBoxResults.TabIndex = 1;
      // 
      // buttonClose
      // 
      this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonClose.Location = new System.Drawing.Point(402, 496);
      this.buttonClose.Name = "buttonClose";
      this.buttonClose.Size = new System.Drawing.Size(75, 23);
      this.buttonClose.TabIndex = 2;
      this.buttonClose.Text = "&Close";
      this.buttonClose.UseVisualStyleBackColor = true;
      this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
      // 
      // checkBoxClearValues
      // 
      this.checkBoxClearValues.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.checkBoxClearValues.AutoSize = true;
      this.checkBoxClearValues.Checked = true;
      this.checkBoxClearValues.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxClearValues.Location = new System.Drawing.Point(12, 500);
      this.checkBoxClearValues.Name = "checkBoxClearValues";
      this.checkBoxClearValues.Size = new System.Drawing.Size(101, 17);
      this.checkBoxClearValues.TabIndex = 3;
      this.checkBoxClearValues.Text = "Clear old values";
      this.checkBoxClearValues.UseVisualStyleBackColor = true;
      // 
      // lblMaxDesc
      // 
      this.lblMaxDesc.AutoSize = true;
      this.lblMaxDesc.Location = new System.Drawing.Point(9, 9);
      this.lblMaxDesc.Name = "lblMaxDesc";
      this.lblMaxDesc.Size = new System.Drawing.Size(101, 13);
      this.lblMaxDesc.TabIndex = 4;
      this.lblMaxDesc.Text = "Maximum accuracy:";
      this.lblMaxDesc.Visible = false;
      // 
      // lblMaxAccurary
      // 
      this.lblMaxAccurary.AutoSize = true;
      this.lblMaxAccurary.Location = new System.Drawing.Point(116, 9);
      this.lblMaxAccurary.Name = "lblMaxAccurary";
      this.lblMaxAccurary.Size = new System.Drawing.Size(0, 13);
      this.lblMaxAccurary.TabIndex = 5;
      // 
      // numLoopCount
      // 
      this.numLoopCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.numLoopCount.Location = new System.Drawing.Point(119, 499);
      this.numLoopCount.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
      this.numLoopCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numLoopCount.Name = "numLoopCount";
      this.numLoopCount.Size = new System.Drawing.Size(64, 20);
      this.numLoopCount.TabIndex = 6;
      this.numLoopCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numLoopCount.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
      // 
      // lblLoopCountDesc
      // 
      this.lblLoopCountDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblLoopCountDesc.AutoSize = true;
      this.lblLoopCountDesc.Location = new System.Drawing.Point(189, 501);
      this.lblLoopCountDesc.Name = "lblLoopCountDesc";
      this.lblLoopCountDesc.Size = new System.Drawing.Size(101, 13);
      this.lblLoopCountDesc.TabIndex = 7;
      this.lblLoopCountDesc.Text = "Run this many times";
      // 
      // FormMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(489, 531);
      this.Controls.Add(this.lblLoopCountDesc);
      this.Controls.Add(this.numLoopCount);
      this.Controls.Add(this.lblMaxAccurary);
      this.Controls.Add(this.lblMaxDesc);
      this.Controls.Add(this.checkBoxClearValues);
      this.Controls.Add(this.buttonClose);
      this.Controls.Add(this.listBoxResults);
      this.Controls.Add(this.buttonStart);
      this.Name = "FormMain";
      this.Text = "TimerTest";
      ((System.ComponentModel.ISupportInitialize)(this.numLoopCount)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonStart;
    private System.Windows.Forms.ListBox listBoxResults;
    private System.Windows.Forms.Button buttonClose;
    private System.Windows.Forms.CheckBox checkBoxClearValues;
    private System.Windows.Forms.Label lblMaxDesc;
    private System.Windows.Forms.Label lblMaxAccurary;
    private System.Windows.Forms.NumericUpDown numLoopCount;
    private System.Windows.Forms.Label lblLoopCountDesc;
  }
}

