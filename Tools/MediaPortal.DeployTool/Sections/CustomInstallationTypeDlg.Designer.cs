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
        this.rbClient = new System.Windows.Forms.RadioButton();
        this.rbTvServerSlave = new System.Windows.Forms.RadioButton();
        this.rbTvServerMaster = new System.Windows.Forms.RadioButton();
        this.rbSingleSeat = new System.Windows.Forms.RadioButton();
        this.labelSingleSeat = new System.Windows.Forms.Label();
        this.labelMaster = new System.Windows.Forms.Label();
        this.labelSlave = new System.Windows.Forms.Label();
        this.labelClient = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Size = new System.Drawing.Size(273, 13);
        this.labelSectionHeader.Text = "Please choose which setup you want to install:";
        // 
        // rbClient
        // 
        this.rbClient.AutoSize = true;
        this.rbClient.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbClient.Location = new System.Drawing.Point(7, 203);
        this.rbClient.Name = "rbClient";
        this.rbClient.Size = new System.Drawing.Size(279, 17);
        this.rbClient.TabIndex = 8;
        this.rbClient.TabStop = true;
        this.rbClient.Text = "MediaPortal Client (connects to a TV-Server)";
        this.rbClient.UseVisualStyleBackColor = true;
        // 
        // rbTvServerSlave
        // 
        this.rbTvServerSlave.AutoSize = true;
        this.rbTvServerSlave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbTvServerSlave.Location = new System.Drawing.Point(7, 146);
        this.rbTvServerSlave.Name = "rbTvServerSlave";
        this.rbTvServerSlave.Size = new System.Drawing.Size(335, 17);
        this.rbTvServerSlave.TabIndex = 7;
        this.rbTvServerSlave.TabStop = true;
        this.rbTvServerSlave.Text = "MediaPortal dedicated TV-Server (without SQL Server)";
        this.rbTvServerSlave.UseVisualStyleBackColor = true;
        // 
        // rbTvServerMaster
        // 
        this.rbTvServerMaster.AutoSize = true;
        this.rbTvServerMaster.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbTvServerMaster.Location = new System.Drawing.Point(7, 88);
        this.rbTvServerMaster.Name = "rbTvServerMaster";
        this.rbTvServerMaster.Size = new System.Drawing.Size(397, 17);
        this.rbTvServerMaster.TabIndex = 6;
        this.rbTvServerMaster.TabStop = true;
        this.rbTvServerMaster.Text = "MediaPortal dedicated TV-Server (master server with SQL Server)";
        this.rbTvServerMaster.UseVisualStyleBackColor = true;
        // 
        // rbSingleSeat
        // 
        this.rbSingleSeat.AutoSize = true;
        this.rbSingleSeat.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rbSingleSeat.Location = new System.Drawing.Point(7, 31);
        this.rbSingleSeat.Name = "rbSingleSeat";
        this.rbSingleSeat.Size = new System.Drawing.Size(298, 17);
        this.rbSingleSeat.TabIndex = 5;
        this.rbSingleSeat.TabStop = true;
        this.rbSingleSeat.Text = "MediaPortal Singleseat installation (stand alone)";
        this.rbSingleSeat.UseVisualStyleBackColor = true;
        // 
        // labelSingleSeat
        // 
        this.labelSingleSeat.AutoSize = true;
        this.labelSingleSeat.Location = new System.Drawing.Point(26, 51);
        this.labelSingleSeat.Name = "labelSingleSeat";
        this.labelSingleSeat.Size = new System.Drawing.Size(199, 13);
        this.labelSingleSeat.TabIndex = 9;
        this.labelSingleSeat.Text = "This will install a single seat configuration";
        // 
        // labelMaster
        // 
        this.labelMaster.AutoSize = true;
        this.labelMaster.Location = new System.Drawing.Point(26, 108);
        this.labelMaster.Name = "labelMaster";
        this.labelMaster.Size = new System.Drawing.Size(212, 13);
        this.labelMaster.TabIndex = 10;
        this.labelMaster.Text = "This will install a master server configuration";
        // 
        // labelSlave
        // 
        this.labelSlave.AutoSize = true;
        this.labelSlave.Location = new System.Drawing.Point(26, 166);
        this.labelSlave.Name = "labelSlave";
        this.labelSlave.Size = new System.Drawing.Size(206, 13);
        this.labelSlave.TabIndex = 11;
        this.labelSlave.Text = "This will install a slave server configuration";
        // 
        // labelClient
        // 
        this.labelClient.AutoSize = true;
        this.labelClient.Location = new System.Drawing.Point(26, 223);
        this.labelClient.Name = "labelClient";
        this.labelClient.Size = new System.Drawing.Size(196, 13);
        this.labelClient.TabIndex = 12;
        this.labelClient.Text = "This will install a client only configuration";
        // 
        // CustomInstallationTypeDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.labelClient);
        this.Controls.Add(this.labelSlave);
        this.Controls.Add(this.labelMaster);
        this.Controls.Add(this.labelSingleSeat);
        this.Controls.Add(this.rbClient);
        this.Controls.Add(this.rbTvServerSlave);
        this.Controls.Add(this.rbTvServerMaster);
        this.Controls.Add(this.rbSingleSeat);
        this.Name = "CustomInstallationTypeDlg";
        this.Size = new System.Drawing.Size(615, 270);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.rbSingleSeat, 0);
        this.Controls.SetChildIndex(this.rbTvServerMaster, 0);
        this.Controls.SetChildIndex(this.rbTvServerSlave, 0);
        this.Controls.SetChildIndex(this.rbClient, 0);
        this.Controls.SetChildIndex(this.labelSingleSeat, 0);
        this.Controls.SetChildIndex(this.labelMaster, 0);
        this.Controls.SetChildIndex(this.labelSlave, 0);
        this.Controls.SetChildIndex(this.labelClient, 0);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.RadioButton rbClient;
    private System.Windows.Forms.RadioButton rbTvServerSlave;
    private System.Windows.Forms.RadioButton rbTvServerMaster;
    private System.Windows.Forms.RadioButton rbSingleSeat;
      private System.Windows.Forms.Label labelSingleSeat;
      private System.Windows.Forms.Label labelMaster;
      private System.Windows.Forms.Label labelSlave;
      private System.Windows.Forms.Label labelClient;
  }
}
