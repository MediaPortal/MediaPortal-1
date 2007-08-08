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
      this.rbClient = new System.Windows.Forms.RadioButton();
      this.rbTvServerSlave = new System.Windows.Forms.RadioButton();
      this.rbTvServerMaster = new System.Windows.Forms.RadioButton();
      this.rbSingleSeat = new System.Windows.Forms.RadioButton();
      this.SuspendLayout();
      // 
      // HeaderLabel
      // 
      this.HeaderLabel.Size = new System.Drawing.Size(273, 13);
      this.HeaderLabel.Text = "Please choose which setup you want to install:";
      // 
      // rbClient
      // 
      this.rbClient.AutoSize = true;
      this.rbClient.Location = new System.Drawing.Point(7, 100);
      this.rbClient.Name = "rbClient";
      this.rbClient.Size = new System.Drawing.Size(235, 17);
      this.rbClient.TabIndex = 8;
      this.rbClient.TabStop = true;
      this.rbClient.Text = "MediaPortal Client (connects to a TV-Server)";
      this.rbClient.UseVisualStyleBackColor = true;
      // 
      // rbTvServerSlave
      // 
      this.rbTvServerSlave.AutoSize = true;
      this.rbTvServerSlave.Location = new System.Drawing.Point(7, 77);
      this.rbTvServerSlave.Name = "rbTvServerSlave";
      this.rbTvServerSlave.Size = new System.Drawing.Size(283, 17);
      this.rbTvServerSlave.TabIndex = 7;
      this.rbTvServerSlave.TabStop = true;
      this.rbTvServerSlave.Text = "MediaPortal dedicated TV-Server (without SQL Server)";
      this.rbTvServerSlave.UseVisualStyleBackColor = true;
      // 
      // rbTvServerMaster
      // 
      this.rbTvServerMaster.AutoSize = true;
      this.rbTvServerMaster.Location = new System.Drawing.Point(7, 54);
      this.rbTvServerMaster.Name = "rbTvServerMaster";
      this.rbTvServerMaster.Size = new System.Drawing.Size(334, 17);
      this.rbTvServerMaster.TabIndex = 6;
      this.rbTvServerMaster.TabStop = true;
      this.rbTvServerMaster.Text = "MediaPortal dedicated TV-Server (master server with SQL Server)";
      this.rbTvServerMaster.UseVisualStyleBackColor = true;
      // 
      // rbSingleSeat
      // 
      this.rbSingleSeat.AutoSize = true;
      this.rbSingleSeat.Location = new System.Drawing.Point(7, 31);
      this.rbSingleSeat.Name = "rbSingleSeat";
      this.rbSingleSeat.Size = new System.Drawing.Size(249, 17);
      this.rbSingleSeat.TabIndex = 5;
      this.rbSingleSeat.TabStop = true;
      this.rbSingleSeat.Text = "MediaPortal Singleseat installation (stand alone)";
      this.rbSingleSeat.UseVisualStyleBackColor = true;
      // 
      // BaseInstallationTypeDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.rbClient);
      this.Controls.Add(this.rbTvServerSlave);
      this.Controls.Add(this.rbTvServerMaster);
      this.Controls.Add(this.rbSingleSeat);
      this.Name = "BaseInstallationTypeDlg";
      this.Size = new System.Drawing.Size(359, 172);
      this.Controls.SetChildIndex(this.HeaderLabel, 0);
      this.Controls.SetChildIndex(this.rbSingleSeat, 0);
      this.Controls.SetChildIndex(this.rbTvServerMaster, 0);
      this.Controls.SetChildIndex(this.rbTvServerSlave, 0);
      this.Controls.SetChildIndex(this.rbClient, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.RadioButton rbClient;
    private System.Windows.Forms.RadioButton rbTvServerSlave;
    private System.Windows.Forms.RadioButton rbTvServerMaster;
    private System.Windows.Forms.RadioButton rbSingleSeat;
  }
}
