namespace MPTail
{
  partial class frmSearchParams
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
      this.edSearch = new System.Windows.Forms.TextBox();
      this.cbCase = new System.Windows.Forms.CheckBox();
      this.colorDialog1 = new System.Windows.Forms.ColorDialog();
      this.edColor = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.btnChooseColor = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 15);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(99, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "String to search for:";
      // 
      // edSearch
      // 
      this.edSearch.Location = new System.Drawing.Point(118, 12);
      this.edSearch.Name = "edSearch";
      this.edSearch.Size = new System.Drawing.Size(165, 20);
      this.edSearch.TabIndex = 1;
      // 
      // cbCase
      // 
      this.cbCase.AutoSize = true;
      this.cbCase.Location = new System.Drawing.Point(118, 67);
      this.cbCase.Name = "cbCase";
      this.cbCase.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.cbCase.Size = new System.Drawing.Size(15, 14);
      this.cbCase.TabIndex = 2;
      this.cbCase.UseVisualStyleBackColor = true;
      // 
      // edColor
      // 
      this.edColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edColor.Location = new System.Drawing.Point(118, 39);
      this.edColor.Name = "edColor";
      this.edColor.Size = new System.Drawing.Size(84, 20);
      this.edColor.TabIndex = 4;
      this.edColor.TabStop = false;
      this.edColor.Text = " TEST";
      this.edColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 42);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(77, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "Highlight color:";
      // 
      // btnChooseColor
      // 
      this.btnChooseColor.Location = new System.Drawing.Point(208, 37);
      this.btnChooseColor.Name = "btnChooseColor";
      this.btnChooseColor.Size = new System.Drawing.Size(75, 23);
      this.btnChooseColor.TabIndex = 5;
      this.btnChooseColor.Text = "select";
      this.btnChooseColor.UseVisualStyleBackColor = true;
      this.btnChooseColor.Click += new System.EventHandler(this.btnChooseColor_Click);
      // 
      // btnOk
      // 
      this.btnOk.Location = new System.Drawing.Point(42, 103);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(75, 23);
      this.btnOk.TabIndex = 6;
      this.btnOk.Text = "Ok";
      this.btnOk.UseVisualStyleBackColor = true;
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(180, 103);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 7;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(12, 67);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(78, 13);
      this.label3.TabIndex = 8;
      this.label3.Text = "Case sensitive:";
      // 
      // frmSearchParams
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(300, 148);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.btnChooseColor);
      this.Controls.Add(this.edColor);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.cbCase);
      this.Controls.Add(this.edSearch);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frmSearchParams";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "frmSearchParams";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox edSearch;
    private System.Windows.Forms.CheckBox cbCase;
    private System.Windows.Forms.ColorDialog colorDialog1;
    private System.Windows.Forms.TextBox edColor;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btnChooseColor;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Label label3;
  }
}