namespace MediaPortal.DeployTool
{
  partial class BaseInstallationTypeWithoutTvEngineDlg
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
        this.rbAdvanced = new System.Windows.Forms.Label();
        this.rbOneClick = new System.Windows.Forms.Label();
        this.imgOneClick = new System.Windows.Forms.PictureBox();
        this.labelAdvancedDesc = new System.Windows.Forms.Label();
        this.labelAdvancedCaption = new System.Windows.Forms.Label();
        this.labelOneClickDesc = new System.Windows.Forms.Label();
        this.labelOneClickCaption = new System.Windows.Forms.Label();
        this.imgAdvanced = new System.Windows.Forms.PictureBox();
        ((System.ComponentModel.ISupportInitialize)(this.imgOneClick)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.imgAdvanced)).BeginInit();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Location = new System.Drawing.Point(179, 0);
        this.labelSectionHeader.Size = new System.Drawing.Size(309, 13);
        this.labelSectionHeader.Text = "Please choose which setup you want to install:";
        this.labelSectionHeader.Visible = false;
        // 
        // rbAdvanced
        // 
        this.rbAdvanced.AutoSize = true;
        this.rbAdvanced.Cursor = System.Windows.Forms.Cursors.Hand;
        this.rbAdvanced.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbAdvanced.ForeColor = System.Drawing.Color.White;
        this.rbAdvanced.Location = new System.Drawing.Point(230, 199);
        this.rbAdvanced.Name = "rbAdvanced";
        this.rbAdvanced.Size = new System.Drawing.Size(153, 13);
        this.rbAdvanced.TabIndex = 20;
        this.rbAdvanced.Text = "Do a one click installation";
        this.rbAdvanced.Click += new System.EventHandler(this.rbAdvanced_Click);
        // 
        // rbOneClick
        // 
        this.rbOneClick.AutoSize = true;
        this.rbOneClick.Cursor = System.Windows.Forms.Cursors.Hand;
        this.rbOneClick.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbOneClick.ForeColor = System.Drawing.Color.White;
        this.rbOneClick.Location = new System.Drawing.Point(230, 85);
        this.rbOneClick.Name = "rbOneClick";
        this.rbOneClick.Size = new System.Drawing.Size(153, 13);
        this.rbOneClick.TabIndex = 21;
        this.rbOneClick.Text = "Do a one click installation";
        this.rbOneClick.Click += new System.EventHandler(this.rbOneClick_Click);
        // 
        // imgOneClick
        // 
        this.imgOneClick.Cursor = System.Windows.Forms.Cursors.Hand;
        this.imgOneClick.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
        this.imgOneClick.Location = new System.Drawing.Point(200, 81);
        this.imgOneClick.Name = "imgOneClick";
        this.imgOneClick.Size = new System.Drawing.Size(21, 21);
        this.imgOneClick.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.imgOneClick.TabIndex = 19;
        this.imgOneClick.TabStop = false;
        this.imgOneClick.Click += new System.EventHandler(this.imgOneClick_Click);
        // 
        // labelAdvancedDesc
        // 
        this.labelAdvancedDesc.BackColor = System.Drawing.Color.Transparent;
        this.labelAdvancedDesc.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelAdvancedDesc.ForeColor = System.Drawing.Color.White;
        this.labelAdvancedDesc.Location = new System.Drawing.Point(179, 155);
        this.labelAdvancedDesc.Name = "labelAdvancedDesc";
        this.labelAdvancedDesc.Size = new System.Drawing.Size(321, 46);
        this.labelAdvancedDesc.TabIndex = 18;
        this.labelAdvancedDesc.Text = "The advanced installation allows you to specify installation locations and other " +
            "settings";
        // 
        // labelAdvancedCaption
        // 
        this.labelAdvancedCaption.AutoSize = true;
        this.labelAdvancedCaption.BackColor = System.Drawing.Color.Transparent;
        this.labelAdvancedCaption.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelAdvancedCaption.ForeColor = System.Drawing.Color.White;
        this.labelAdvancedCaption.Location = new System.Drawing.Point(179, 132);
        this.labelAdvancedCaption.Name = "labelAdvancedCaption";
        this.labelAdvancedCaption.Size = new System.Drawing.Size(167, 16);
        this.labelAdvancedCaption.TabIndex = 17;
        this.labelAdvancedCaption.Text = "Advanced Installation";
        // 
        // labelOneClickDesc
        // 
        this.labelOneClickDesc.BackColor = System.Drawing.Color.Transparent;
        this.labelOneClickDesc.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelOneClickDesc.ForeColor = System.Drawing.Color.White;
        this.labelOneClickDesc.Location = new System.Drawing.Point(179, 36);
        this.labelOneClickDesc.Name = "labelOneClickDesc";
        this.labelOneClickDesc.Size = new System.Drawing.Size(321, 46);
        this.labelOneClickDesc.TabIndex = 16;
        this.labelOneClickDesc.Text = "All required applications will be installed into their default locations and with" +
            " the default settings.";
        // 
        // labelOneClickCaption
        // 
        this.labelOneClickCaption.AutoSize = true;
        this.labelOneClickCaption.BackColor = System.Drawing.Color.Transparent;
        this.labelOneClickCaption.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelOneClickCaption.ForeColor = System.Drawing.Color.White;
        this.labelOneClickCaption.Location = new System.Drawing.Point(179, 14);
        this.labelOneClickCaption.Name = "labelOneClickCaption";
        this.labelOneClickCaption.Size = new System.Drawing.Size(162, 16);
        this.labelOneClickCaption.TabIndex = 15;
        this.labelOneClickCaption.Text = "One Click Installation";
        // 
        // imgAdvanced
        // 
        this.imgAdvanced.Cursor = System.Windows.Forms.Cursors.Hand;
        this.imgAdvanced.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
        this.imgAdvanced.Location = new System.Drawing.Point(200, 195);
        this.imgAdvanced.Name = "imgAdvanced";
        this.imgAdvanced.Size = new System.Drawing.Size(21, 21);
        this.imgAdvanced.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.imgAdvanced.TabIndex = 22;
        this.imgAdvanced.TabStop = false;
        this.imgAdvanced.Click += new System.EventHandler(this.imgAdvanced_Click);
        // 
        // BaseInstallationTypeWithoutTvEngineDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_one_click_install_choose;
        this.Controls.Add(this.imgAdvanced);
        this.Controls.Add(this.rbAdvanced);
        this.Controls.Add(this.rbOneClick);
        this.Controls.Add(this.imgOneClick);
        this.Controls.Add(this.labelAdvancedDesc);
        this.Controls.Add(this.labelAdvancedCaption);
        this.Controls.Add(this.labelOneClickDesc);
        this.Controls.Add(this.labelOneClickCaption);
        this.Name = "BaseInstallationTypeWithoutTvEngineDlg";
        this.Size = new System.Drawing.Size(664, 252);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.labelOneClickCaption, 0);
        this.Controls.SetChildIndex(this.labelOneClickDesc, 0);
        this.Controls.SetChildIndex(this.labelAdvancedCaption, 0);
        this.Controls.SetChildIndex(this.labelAdvancedDesc, 0);
        this.Controls.SetChildIndex(this.imgOneClick, 0);
        this.Controls.SetChildIndex(this.rbOneClick, 0);
        this.Controls.SetChildIndex(this.rbAdvanced, 0);
        this.Controls.SetChildIndex(this.imgAdvanced, 0);
        ((System.ComponentModel.ISupportInitialize)(this.imgOneClick)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.imgAdvanced)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label rbAdvanced;
    private System.Windows.Forms.Label rbOneClick;
    private System.Windows.Forms.PictureBox imgOneClick;
    private System.Windows.Forms.Label labelAdvancedDesc;
    private System.Windows.Forms.Label labelAdvancedCaption;
    private System.Windows.Forms.Label labelOneClickDesc;
    private System.Windows.Forms.Label labelOneClickCaption;
    private System.Windows.Forms.PictureBox imgAdvanced;


  }
}
