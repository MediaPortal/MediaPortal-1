namespace MediaPortal.DeployTool.Sections
{
  partial class CustomInstallationTypeDlg
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
      this.labelSingleSeat = new System.Windows.Forms.Label();
      this.labelMaster = new System.Windows.Forms.Label();
      this.labelClient = new System.Windows.Forms.Label();
      this.rbSingleSeat = new System.Windows.Forms.Label();
      this.rbTvServerMaster = new System.Windows.Forms.Label();
      this.rbClient = new System.Windows.Forms.Label();
      this.bSingle = new System.Windows.Forms.Button();
      this.bMaster = new System.Windows.Forms.Button();
      this.bClient = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // labelSectionHeader
      // 
      this.labelSectionHeader.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelSectionHeader.Location = new System.Drawing.Point(32, 28);
      this.labelSectionHeader.Size = new System.Drawing.Size(315, 14);
      this.labelSectionHeader.Text = "Please choose which setup you want to install:";
      // 
      // labelSingleSeat
      // 
      this.labelSingleSeat.AutoSize = true;
      this.labelSingleSeat.ForeColor = System.Drawing.Color.White;
      this.labelSingleSeat.Location = new System.Drawing.Point(73, 82);
      this.labelSingleSeat.MaximumSize = new System.Drawing.Size(429, 0);
      this.labelSingleSeat.Name = "labelSingleSeat";
      this.labelSingleSeat.Size = new System.Drawing.Size(199, 13);
      this.labelSingleSeat.TabIndex = 9;
      this.labelSingleSeat.Text = "This will install a single seat configuration";
      this.labelSingleSeat.Click += new System.EventHandler(this.bSingle_Click);
      // 
      // labelMaster
      // 
      this.labelMaster.AutoSize = true;
      this.labelMaster.ForeColor = System.Drawing.Color.White;
      this.labelMaster.Location = new System.Drawing.Point(73, 140);
      this.labelMaster.MaximumSize = new System.Drawing.Size(429, 0);
      this.labelMaster.Name = "labelMaster";
      this.labelMaster.Size = new System.Drawing.Size(228, 13);
      this.labelMaster.TabIndex = 10;
      this.labelMaster.Text = "This will install a dedicated server configuration";
      this.labelMaster.Click += new System.EventHandler(this.bMaster_Click);
      // 
      // labelClient
      // 
      this.labelClient.AutoSize = true;
      this.labelClient.ForeColor = System.Drawing.Color.White;
      this.labelClient.Location = new System.Drawing.Point(73, 201);
      this.labelClient.MaximumSize = new System.Drawing.Size(429, 0);
      this.labelClient.Name = "labelClient";
      this.labelClient.Size = new System.Drawing.Size(196, 13);
      this.labelClient.TabIndex = 12;
      this.labelClient.Text = "This will install a client only configuration";
      this.labelClient.Click += new System.EventHandler(this.bClient_Click);
      // 
      // rbSingleSeat
      // 
      this.rbSingleSeat.AutoSize = true;
      this.rbSingleSeat.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbSingleSeat.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbSingleSeat.ForeColor = System.Drawing.Color.White;
      this.rbSingleSeat.Location = new System.Drawing.Point(73, 69);
      this.rbSingleSeat.Name = "rbSingleSeat";
      this.rbSingleSeat.Size = new System.Drawing.Size(325, 13);
      this.rbSingleSeat.TabIndex = 24;
      this.rbSingleSeat.Text = "MediaPortal Singleseat installation (stand alone)";
      this.rbSingleSeat.Click += new System.EventHandler(this.bSingle_Click);
      // 
      // rbTvServerMaster
      // 
      this.rbTvServerMaster.AutoSize = true;
      this.rbTvServerMaster.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbTvServerMaster.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbTvServerMaster.ForeColor = System.Drawing.Color.White;
      this.rbTvServerMaster.Location = new System.Drawing.Point(73, 127);
      this.rbTvServerMaster.Name = "rbTvServerMaster";
      this.rbTvServerMaster.Size = new System.Drawing.Size(222, 13);
      this.rbTvServerMaster.TabIndex = 25;
      this.rbTvServerMaster.Text = "MediaPortal dedicated TV-Server";
      this.rbTvServerMaster.Click += new System.EventHandler(this.bMaster_Click);
      // 
      // rbClient
      // 
      this.rbClient.AutoSize = true;
      this.rbClient.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbClient.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbClient.ForeColor = System.Drawing.Color.White;
      this.rbClient.Location = new System.Drawing.Point(73, 186);
      this.rbClient.Name = "rbClient";
      this.rbClient.Size = new System.Drawing.Size(298, 13);
      this.rbClient.TabIndex = 27;
      this.rbClient.Text = "MediaPortal Client (connects to a TV-Server)";
      this.rbClient.Click += new System.EventHandler(this.bClient_Click);
      // 
      // bSingle
      // 
      this.bSingle.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bSingle.FlatAppearance.BorderSize = 0;
      this.bSingle.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bSingle.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bSingle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bSingle.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bSingle.Location = new System.Drawing.Point(35, 64);
      this.bSingle.Name = "bSingle";
      this.bSingle.Size = new System.Drawing.Size(32, 23);
      this.bSingle.TabIndex = 28;
      this.bSingle.UseVisualStyleBackColor = true;
      this.bSingle.Click += new System.EventHandler(this.bSingle_Click);
      // 
      // bMaster
      // 
      this.bMaster.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bMaster.FlatAppearance.BorderSize = 0;
      this.bMaster.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bMaster.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bMaster.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bMaster.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bMaster.Location = new System.Drawing.Point(35, 122);
      this.bMaster.Name = "bMaster";
      this.bMaster.Size = new System.Drawing.Size(32, 23);
      this.bMaster.TabIndex = 29;
      this.bMaster.UseVisualStyleBackColor = true;
      this.bMaster.Click += new System.EventHandler(this.bMaster_Click);
      // 
      // bClient
      // 
      this.bClient.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bClient.FlatAppearance.BorderSize = 0;
      this.bClient.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bClient.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bClient.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bClient.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bClient.Location = new System.Drawing.Point(35, 181);
      this.bClient.Name = "bClient";
      this.bClient.Size = new System.Drawing.Size(32, 23);
      this.bClient.TabIndex = 30;
      this.bClient.UseVisualStyleBackColor = true;
      this.bClient.Click += new System.EventHandler(this.bClient_Click);
      // 
      // CustomInstallationTypeDlg
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
      this.Controls.Add(this.bClient);
      this.Controls.Add(this.bMaster);
      this.Controls.Add(this.bSingle);
      this.Controls.Add(this.rbClient);
      this.Controls.Add(this.rbTvServerMaster);
      this.Controls.Add(this.rbSingleSeat);
      this.Controls.Add(this.labelClient);
      this.Controls.Add(this.labelMaster);
      this.Controls.Add(this.labelSingleSeat);
      this.Name = "CustomInstallationTypeDlg";
      this.Size = new System.Drawing.Size(666, 250);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.labelSingleSeat, 0);
      this.Controls.SetChildIndex(this.labelMaster, 0);
      this.Controls.SetChildIndex(this.labelClient, 0);
      this.Controls.SetChildIndex(this.rbSingleSeat, 0);
      this.Controls.SetChildIndex(this.rbTvServerMaster, 0);
      this.Controls.SetChildIndex(this.rbClient, 0);
      this.Controls.SetChildIndex(this.bSingle, 0);
      this.Controls.SetChildIndex(this.bMaster, 0);
      this.Controls.SetChildIndex(this.bClient, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelSingleSeat;
    private System.Windows.Forms.Label labelMaster;
    private System.Windows.Forms.Label labelClient;
    private System.Windows.Forms.Label rbSingleSeat;
    private System.Windows.Forms.Label rbTvServerMaster;
    private System.Windows.Forms.Label rbClient;
    private System.Windows.Forms.Button bSingle;
    private System.Windows.Forms.Button bMaster;
    private System.Windows.Forms.Button bClient;
  }
}