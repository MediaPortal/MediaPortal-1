namespace MediaPortal.DeployTool
{
  partial class BaseInstallationTypeDlg
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.pictureBox2 = new System.Windows.Forms.PictureBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.rbOneClick = new System.Windows.Forms.RadioButton();
      this.rbAdvanced = new System.Windows.Forms.RadioButton();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // HeaderLabel
      // 
      this.HeaderLabel.Size = new System.Drawing.Size(273, 13);
      this.HeaderLabel.Text = "Please choose which setup you want to install:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(88, 44);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(156, 16);
      this.label1.TabIndex = 1;
      this.label1.Text = "One Click Installation";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(91, 64);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(321, 46);
      this.label2.TabIndex = 2;
      this.label2.Text = "All required applications will be installed into their default locations and with" +
          " the default settings. The database password is \"MediaPortal\".";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(91, 179);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(321, 46);
      this.label3.TabIndex = 6;
      this.label3.Text = "The advanced installation allows you to install Server/Client setups and to speci" +
          "fy installation locations and other settings";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.Location = new System.Drawing.Point(88, 159);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(159, 16);
      this.label4.TabIndex = 5;
      this.label4.Text = "Advanced Installation";
      // 
      // pictureBox2
      // 
      this.pictureBox2.Image = global::MediaPortal.DeployTool.Properties.Resources.smart_small;
      this.pictureBox2.Location = new System.Drawing.Point(16, 159);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new System.Drawing.Size(69, 93);
      this.pictureBox2.TabIndex = 8;
      this.pictureBox2.TabStop = false;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::MediaPortal.DeployTool.Properties.Resources.average_small;
      this.pictureBox1.Location = new System.Drawing.Point(16, 44);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(69, 93);
      this.pictureBox1.TabIndex = 4;
      this.pictureBox1.TabStop = false;
      // 
      // rbOneClick
      // 
      this.rbOneClick.AutoSize = true;
      this.rbOneClick.Location = new System.Drawing.Point(94, 115);
      this.rbOneClick.Name = "rbOneClick";
      this.rbOneClick.Size = new System.Drawing.Size(146, 17);
      this.rbOneClick.TabIndex = 9;
      this.rbOneClick.TabStop = true;
      this.rbOneClick.Text = "Do a one click installation";
      this.rbOneClick.UseVisualStyleBackColor = true;
      // 
      // rbAdvanced
      // 
      this.rbAdvanced.AutoSize = true;
      this.rbAdvanced.Location = new System.Drawing.Point(94, 231);
      this.rbAdvanced.Name = "rbAdvanced";
      this.rbAdvanced.Size = new System.Drawing.Size(157, 17);
      this.rbAdvanced.TabIndex = 10;
      this.rbAdvanced.TabStop = true;
      this.rbAdvanced.Text = "Do an advanced installation";
      this.rbAdvanced.UseVisualStyleBackColor = true;
      // 
      // BaseInstallationTypeDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.rbAdvanced);
      this.Controls.Add(this.rbOneClick);
      this.Controls.Add(this.pictureBox2);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Name = "BaseInstallationTypeDlg";
      this.Size = new System.Drawing.Size(542, 266);
      this.Controls.SetChildIndex(this.label1, 0);
      this.Controls.SetChildIndex(this.label2, 0);
      this.Controls.SetChildIndex(this.pictureBox1, 0);
      this.Controls.SetChildIndex(this.HeaderLabel, 0);
      this.Controls.SetChildIndex(this.label4, 0);
      this.Controls.SetChildIndex(this.label3, 0);
      this.Controls.SetChildIndex(this.pictureBox2, 0);
      this.Controls.SetChildIndex(this.rbOneClick, 0);
      this.Controls.SetChildIndex(this.rbAdvanced, 0);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.PictureBox pictureBox2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.RadioButton rbOneClick;
    private System.Windows.Forms.RadioButton rbAdvanced;

  }
}
