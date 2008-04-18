namespace MediaPortal.DeployTool
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
      this.labelSlave = new System.Windows.Forms.Label();
      this.labelClient = new System.Windows.Forms.Label();
      this.imgSingle = new System.Windows.Forms.PictureBox();
      this.imgMaster = new System.Windows.Forms.PictureBox();
      this.imgSlave = new System.Windows.Forms.PictureBox();
      this.imgClient = new System.Windows.Forms.PictureBox();
      this.rbSingleSeat = new System.Windows.Forms.Label();
      this.rbTvServerMaster = new System.Windows.Forms.Label();
      this.rbTvServerSlave = new System.Windows.Forms.Label();
      this.rbClient = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.imgSingle)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgMaster)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgSlave)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgClient)).BeginInit();
      this.SuspendLayout();
      // 
      // labelSectionHeader
      // 
      this.labelSectionHeader.ForeColor = System.Drawing.Color.White;
      this.labelSectionHeader.Size = new System.Drawing.Size(273, 13);
      this.labelSectionHeader.Text = "Please choose which setup you want to install:";
      // 
      // labelSingleSeat
      // 
      this.labelSingleSeat.AutoSize = true;
      this.labelSingleSeat.ForeColor = System.Drawing.Color.White;
      this.labelSingleSeat.Location = new System.Drawing.Point(47, 49);
      this.labelSingleSeat.Name = "labelSingleSeat";
      this.labelSingleSeat.Size = new System.Drawing.Size(199, 13);
      this.labelSingleSeat.TabIndex = 9;
      this.labelSingleSeat.Text = "This will install a single seat configuration";
      this.labelSingleSeat.Click += new System.EventHandler(this.imgSingle_Click);
      // 
      // labelMaster
      // 
      this.labelMaster.AutoSize = true;
      this.labelMaster.ForeColor = System.Drawing.Color.White;
      this.labelMaster.Location = new System.Drawing.Point(47, 108);
      this.labelMaster.Name = "labelMaster";
      this.labelMaster.Size = new System.Drawing.Size(212, 13);
      this.labelMaster.TabIndex = 10;
      this.labelMaster.Text = "This will install a master server configuration";
      this.labelMaster.Click += new System.EventHandler(this.imgMaster_Click);
      // 
      // labelSlave
      // 
      this.labelSlave.AutoSize = true;
      this.labelSlave.ForeColor = System.Drawing.Color.White;
      this.labelSlave.Location = new System.Drawing.Point(47, 167);
      this.labelSlave.Name = "labelSlave";
      this.labelSlave.Size = new System.Drawing.Size(206, 13);
      this.labelSlave.TabIndex = 11;
      this.labelSlave.Text = "This will install a slave server configuration";
      this.labelSlave.Click += new System.EventHandler(this.imgSlave_Click);
      // 
      // labelClient
      // 
      this.labelClient.AutoSize = true;
      this.labelClient.ForeColor = System.Drawing.Color.White;
      this.labelClient.Location = new System.Drawing.Point(47, 224);
      this.labelClient.Name = "labelClient";
      this.labelClient.Size = new System.Drawing.Size(196, 13);
      this.labelClient.TabIndex = 12;
      this.labelClient.Text = "This will install a client only configuration";
      this.labelClient.Click += new System.EventHandler(this.imgClient_Click);
      // 
      // imgSingle
      // 
      this.imgSingle.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.imgSingle.Location = new System.Drawing.Point(11, 31);
      this.imgSingle.Name = "imgSingle";
      this.imgSingle.Size = new System.Drawing.Size(21, 21);
      this.imgSingle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.imgSingle.TabIndex = 20;
      this.imgSingle.TabStop = false;
      this.imgSingle.Click += new System.EventHandler(this.imgSingle_Click);
      // 
      // imgMaster
      // 
      this.imgMaster.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.imgMaster.Location = new System.Drawing.Point(11, 88);
      this.imgMaster.Name = "imgMaster";
      this.imgMaster.Size = new System.Drawing.Size(21, 21);
      this.imgMaster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.imgMaster.TabIndex = 21;
      this.imgMaster.TabStop = false;
      this.imgMaster.Click += new System.EventHandler(this.imgMaster_Click);
      // 
      // imgSlave
      // 
      this.imgSlave.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.imgSlave.Location = new System.Drawing.Point(11, 146);
      this.imgSlave.Name = "imgSlave";
      this.imgSlave.Size = new System.Drawing.Size(21, 21);
      this.imgSlave.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.imgSlave.TabIndex = 22;
      this.imgSlave.TabStop = false;
      this.imgSlave.Click += new System.EventHandler(this.imgSlave_Click);
      // 
      // imgClient
      // 
      this.imgClient.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.imgClient.Location = new System.Drawing.Point(11, 203);
      this.imgClient.Name = "imgClient";
      this.imgClient.Size = new System.Drawing.Size(21, 21);
      this.imgClient.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.imgClient.TabIndex = 23;
      this.imgClient.TabStop = false;
      this.imgClient.Click += new System.EventHandler(this.imgClient_Click);
      // 
      // rbSingleSeat
      // 
      this.rbSingleSeat.AutoSize = true;
      this.rbSingleSeat.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbSingleSeat.ForeColor = System.Drawing.Color.White;
      this.rbSingleSeat.Location = new System.Drawing.Point(47, 31);
      this.rbSingleSeat.Name = "rbSingleSeat";
      this.rbSingleSeat.Size = new System.Drawing.Size(280, 13);
      this.rbSingleSeat.TabIndex = 24;
      this.rbSingleSeat.Text = "MediaPortal Singleseat installation (stand alone)";
      this.rbSingleSeat.Click += new System.EventHandler(this.imgSingle_Click);
      // 
      // rbTvServerMaster
      // 
      this.rbTvServerMaster.AutoSize = true;
      this.rbTvServerMaster.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbTvServerMaster.ForeColor = System.Drawing.Color.White;
      this.rbTvServerMaster.Location = new System.Drawing.Point(47, 88);
      this.rbTvServerMaster.Name = "rbTvServerMaster";
      this.rbTvServerMaster.Size = new System.Drawing.Size(379, 13);
      this.rbTvServerMaster.TabIndex = 25;
      this.rbTvServerMaster.Text = "MediaPortal dedicated TV-Server (master server with SQL Server)";
      this.rbTvServerMaster.Click += new System.EventHandler(this.imgMaster_Click);
      // 
      // rbTvServerSlave
      // 
      this.rbTvServerSlave.AutoSize = true;
      this.rbTvServerSlave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbTvServerSlave.ForeColor = System.Drawing.Color.White;
      this.rbTvServerSlave.Location = new System.Drawing.Point(47, 146);
      this.rbTvServerSlave.Name = "rbTvServerSlave";
      this.rbTvServerSlave.Size = new System.Drawing.Size(317, 13);
      this.rbTvServerSlave.TabIndex = 26;
      this.rbTvServerSlave.Text = "MediaPortal dedicated TV-Server (without SQL Server)";
      this.rbTvServerSlave.Click += new System.EventHandler(this.imgSlave_Click);
      // 
      // rbClient
      // 
      this.rbClient.AutoSize = true;
      this.rbClient.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbClient.ForeColor = System.Drawing.Color.White;
      this.rbClient.Location = new System.Drawing.Point(47, 203);
      this.rbClient.Name = "rbClient";
      this.rbClient.Size = new System.Drawing.Size(261, 13);
      this.rbClient.TabIndex = 27;
      this.rbClient.Text = "MediaPortal Client (connects to a TV-Server)";
      this.rbClient.Click += new System.EventHandler(this.imgClient_Click);
      // 
      // CustomInstallationTypeDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
      this.Controls.Add(this.rbClient);
      this.Controls.Add(this.rbTvServerSlave);
      this.Controls.Add(this.rbTvServerMaster);
      this.Controls.Add(this.rbSingleSeat);
      this.Controls.Add(this.imgClient);
      this.Controls.Add(this.imgSlave);
      this.Controls.Add(this.imgMaster);
      this.Controls.Add(this.imgSingle);
      this.Controls.Add(this.labelClient);
      this.Controls.Add(this.labelSlave);
      this.Controls.Add(this.labelMaster);
      this.Controls.Add(this.labelSingleSeat);
      this.Name = "CustomInstallationTypeDlg";
      this.Size = new System.Drawing.Size(665, 250);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.labelSingleSeat, 0);
      this.Controls.SetChildIndex(this.labelMaster, 0);
      this.Controls.SetChildIndex(this.labelSlave, 0);
      this.Controls.SetChildIndex(this.labelClient, 0);
      this.Controls.SetChildIndex(this.imgSingle, 0);
      this.Controls.SetChildIndex(this.imgMaster, 0);
      this.Controls.SetChildIndex(this.imgSlave, 0);
      this.Controls.SetChildIndex(this.imgClient, 0);
      this.Controls.SetChildIndex(this.rbSingleSeat, 0);
      this.Controls.SetChildIndex(this.rbTvServerMaster, 0);
      this.Controls.SetChildIndex(this.rbTvServerSlave, 0);
      this.Controls.SetChildIndex(this.rbClient, 0);
      ((System.ComponentModel.ISupportInitialize)(this.imgSingle)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgMaster)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgSlave)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.imgClient)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelSingleSeat;
      private System.Windows.Forms.Label labelMaster;
      private System.Windows.Forms.Label labelSlave;
      private System.Windows.Forms.Label labelClient;
    private System.Windows.Forms.PictureBox imgSingle;
    private System.Windows.Forms.PictureBox imgMaster;
    private System.Windows.Forms.PictureBox imgSlave;
    private System.Windows.Forms.PictureBox imgClient;
    private System.Windows.Forms.Label rbSingleSeat;
    private System.Windows.Forms.Label rbTvServerMaster;
    private System.Windows.Forms.Label rbTvServerSlave;
    private System.Windows.Forms.Label rbClient;
  }
}
