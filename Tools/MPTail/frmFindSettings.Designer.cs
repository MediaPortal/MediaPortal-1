namespace MPTail
{
  partial class frmFindSettings
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
      this.label1 = new System.Windows.Forms.Label();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.checkBoxMatchCase = new System.Windows.Forms.CheckBox();
      this.checkBoxWholeWord = new System.Windows.Forms.CheckBox();
      this.checkBoxReverse = new System.Windows.Forms.CheckBox();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 13);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(97, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "string to search for:";
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(113, 10);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(172, 20);
      this.textBox1.TabIndex = 1;
      // 
      // checkBoxMatchCase
      // 
      this.checkBoxMatchCase.AutoSize = true;
      this.checkBoxMatchCase.Location = new System.Drawing.Point(16, 40);
      this.checkBoxMatchCase.Name = "checkBoxMatchCase";
      this.checkBoxMatchCase.Size = new System.Drawing.Size(81, 17);
      this.checkBoxMatchCase.TabIndex = 2;
      this.checkBoxMatchCase.Text = "match case";
      this.checkBoxMatchCase.UseVisualStyleBackColor = true;
      // 
      // checkBoxWholeWord
      // 
      this.checkBoxWholeWord.AutoSize = true;
      this.checkBoxWholeWord.Location = new System.Drawing.Point(103, 40);
      this.checkBoxWholeWord.Name = "checkBoxWholeWord";
      this.checkBoxWholeWord.Size = new System.Drawing.Size(80, 17);
      this.checkBoxWholeWord.TabIndex = 3;
      this.checkBoxWholeWord.Text = "whole word";
      this.checkBoxWholeWord.UseVisualStyleBackColor = true;
      // 
      // checkBoxReverse
      // 
      this.checkBoxReverse.AutoSize = true;
      this.checkBoxReverse.Location = new System.Drawing.Point(189, 40);
      this.checkBoxReverse.Name = "checkBoxReverse";
      this.checkBoxReverse.Size = new System.Drawing.Size(96, 17);
      this.checkBoxReverse.TabIndex = 4;
      this.checkBoxReverse.Text = "begin from end";
      this.checkBoxReverse.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.Location = new System.Drawing.Point(48, 78);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(62, 23);
      this.btnOk.TabIndex = 5;
      this.btnOk.Text = "Ok";
      this.btnOk.UseVisualStyleBackColor = true;
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(170, 78);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(62, 23);
      this.btnCancel.TabIndex = 6;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // frmFindSettings
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(298, 120);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.checkBoxReverse);
      this.Controls.Add(this.checkBoxWholeWord);
      this.Controls.Add(this.checkBoxMatchCase);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frmFindSettings";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Find settings";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.CheckBox checkBoxMatchCase;
    private System.Windows.Forms.CheckBox checkBoxWholeWord;
    private System.Windows.Forms.CheckBox checkBoxReverse;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
  }
}