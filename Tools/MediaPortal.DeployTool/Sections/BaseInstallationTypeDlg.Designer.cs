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
        this.labelOneClickCaption = new System.Windows.Forms.Label();
        this.labelOneClickDesc = new System.Windows.Forms.Label();
        this.labelAdvancedDesc = new System.Windows.Forms.Label();
        this.labelAdvancedCaption = new System.Windows.Forms.Label();
        this.pictureBox2 = new System.Windows.Forms.PictureBox();
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        this.rbOneClick = new System.Windows.Forms.RadioButton();
        this.rbAdvanced = new System.Windows.Forms.RadioButton();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Size = new System.Drawing.Size(273, 13);
        this.labelSectionHeader.Text = "Please choose which setup you want to install:";
        // 
        // labelOneClickCaption
        // 
        this.labelOneClickCaption.AutoSize = true;
        this.labelOneClickCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelOneClickCaption.Location = new System.Drawing.Point(88, 25);
        this.labelOneClickCaption.Name = "labelOneClickCaption";
        this.labelOneClickCaption.Size = new System.Drawing.Size(153, 16);
        this.labelOneClickCaption.TabIndex = 1;
        this.labelOneClickCaption.Text = "One Click Installation";
        // 
        // labelOneClickDesc
        // 
        this.labelOneClickDesc.Location = new System.Drawing.Point(91, 45);
        this.labelOneClickDesc.Name = "labelOneClickDesc";
        this.labelOneClickDesc.Size = new System.Drawing.Size(321, 46);
        this.labelOneClickDesc.TabIndex = 2;
        this.labelOneClickDesc.Text = "All required applications will be installed into their default locations and with" +
            " the default settings. The database password is \"MediaPortal\".";
        // 
        // labelAdvancedDesc
        // 
        this.labelAdvancedDesc.Location = new System.Drawing.Point(91, 160);
        this.labelAdvancedDesc.Name = "labelAdvancedDesc";
        this.labelAdvancedDesc.Size = new System.Drawing.Size(321, 46);
        this.labelAdvancedDesc.TabIndex = 6;
        this.labelAdvancedDesc.Text = "The advanced installation allows you to install Server/Client setups and to speci" +
            "fy installation locations and other settings";
        // 
        // labelAdvancedCaption
        // 
        this.labelAdvancedCaption.AutoSize = true;
        this.labelAdvancedCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelAdvancedCaption.Location = new System.Drawing.Point(88, 140);
        this.labelAdvancedCaption.Name = "labelAdvancedCaption";
        this.labelAdvancedCaption.Size = new System.Drawing.Size(157, 16);
        this.labelAdvancedCaption.TabIndex = 5;
        this.labelAdvancedCaption.Text = "Advanced Installation";
        // 
        // pictureBox2
        // 
        this.pictureBox2.Image = global::MediaPortal.DeployTool.Images.MePo_smart;
        this.pictureBox2.Location = new System.Drawing.Point(16, 140);
        this.pictureBox2.Name = "pictureBox2";
        this.pictureBox2.Size = new System.Drawing.Size(69, 82);
        this.pictureBox2.TabIndex = 8;
        this.pictureBox2.TabStop = false;
        // 
        // pictureBox1
        // 
        this.pictureBox1.Image = global::MediaPortal.DeployTool.Images.MePo_average;
        this.pictureBox1.Location = new System.Drawing.Point(16, 25);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(69, 82);
        this.pictureBox1.TabIndex = 4;
        this.pictureBox1.TabStop = false;
        // 
        // rbOneClick
        // 
        this.rbOneClick.AutoSize = true;
        this.rbOneClick.Location = new System.Drawing.Point(94, 96);
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
        this.rbAdvanced.Location = new System.Drawing.Point(94, 212);
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
        this.Controls.Add(this.labelAdvancedDesc);
        this.Controls.Add(this.labelAdvancedCaption);
        this.Controls.Add(this.pictureBox1);
        this.Controls.Add(this.labelOneClickDesc);
        this.Controls.Add(this.labelOneClickCaption);
        this.Name = "BaseInstallationTypeDlg";
        this.Size = new System.Drawing.Size(542, 266);
        this.Controls.SetChildIndex(this.labelOneClickCaption, 0);
        this.Controls.SetChildIndex(this.labelOneClickDesc, 0);
        this.Controls.SetChildIndex(this.pictureBox1, 0);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.labelAdvancedCaption, 0);
        this.Controls.SetChildIndex(this.labelAdvancedDesc, 0);
        this.Controls.SetChildIndex(this.pictureBox2, 0);
        this.Controls.SetChildIndex(this.rbOneClick, 0);
        this.Controls.SetChildIndex(this.rbAdvanced, 0);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelOneClickCaption;
    private System.Windows.Forms.Label labelOneClickDesc;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.PictureBox pictureBox2;
    private System.Windows.Forms.Label labelAdvancedDesc;
    private System.Windows.Forms.Label labelAdvancedCaption;
    private System.Windows.Forms.RadioButton rbOneClick;
    private System.Windows.Forms.RadioButton rbAdvanced;

  }
}
