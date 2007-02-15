namespace SetupTv.Sections
{
  partial class FormEditCard
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
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.textBoxDecryptLimit = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.buttonSave = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(24, 22);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(326, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "If your card has a CAM module then specify the number of channels";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(24, 44);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(171, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "this cam can decode simultanously";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(24, 88);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(111, 13);
      this.label3.TabIndex = 2;
      this.label3.Text = "This card can decode";
      // 
      // textBoxDecryptLimit
      // 
      this.textBoxDecryptLimit.Location = new System.Drawing.Point(141, 85);
      this.textBoxDecryptLimit.Name = "textBoxDecryptLimit";
      this.textBoxDecryptLimit.Size = new System.Drawing.Size(44, 20);
      this.textBoxDecryptLimit.TabIndex = 0;
      this.textBoxDecryptLimit.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(191, 88);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(121, 13);
      this.label4.TabIndex = 4;
      this.label4.Text = "channels simultaneously";
      // 
      // buttonSave
      // 
      this.buttonSave.Location = new System.Drawing.Point(271, 131);
      this.buttonSave.Name = "buttonSave";
      this.buttonSave.Size = new System.Drawing.Size(75, 23);
      this.buttonSave.TabIndex = 1;
      this.buttonSave.Text = "Save";
      this.buttonSave.UseVisualStyleBackColor = true;
      this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
      // 
      // FormEditCard
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(358, 166);
      this.Controls.Add(this.buttonSave);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.textBoxDecryptLimit);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Name = "FormEditCard";
      this.Text = "Edit card properties";
      this.Load += new System.EventHandler(this.FormEditCard_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox textBoxDecryptLimit;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Button buttonSave;
  }
}