namespace MediaPortal.Configuration.Sections
{
  partial class MusicWinampPreview
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
      this.pictureBoxVis = new System.Windows.Forms.PictureBox();
      this.btStop = new MediaPortal.UserInterface.Controls.MPButton();
      this.btConfig = new MediaPortal.UserInterface.Controls.MPButton();
      this.btStart = new MediaPortal.UserInterface.Controls.MPButton();
      this.btClose = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVis)).BeginInit();
      this.SuspendLayout();
      // 
      // pictureBoxVis
      // 
      this.pictureBoxVis.Location = new System.Drawing.Point(13, 13);
      this.pictureBoxVis.Name = "pictureBoxVis";
      this.pictureBoxVis.Size = new System.Drawing.Size(437, 273);
      this.pictureBoxVis.TabIndex = 1;
      this.pictureBoxVis.TabStop = false;
      // 
      // btStop
      // 
      this.btStop.Location = new System.Drawing.Point(238, 331);
      this.btStop.Name = "btStop";
      this.btStop.Size = new System.Drawing.Size(75, 23);
      this.btStop.TabIndex = 4;
      this.btStop.Text = "Stop";
      this.btStop.UseVisualStyleBackColor = true;
      this.btStop.Click += new System.EventHandler(this.btStop_Click);
      // 
      // btConfig
      // 
      this.btConfig.Location = new System.Drawing.Point(136, 331);
      this.btConfig.Name = "btConfig";
      this.btConfig.Size = new System.Drawing.Size(75, 23);
      this.btConfig.TabIndex = 3;
      this.btConfig.Text = "Config";
      this.btConfig.UseVisualStyleBackColor = true;
      this.btConfig.Click += new System.EventHandler(this.btConfig_Click);
      // 
      // btStart
      // 
      this.btStart.Location = new System.Drawing.Point(34, 331);
      this.btStart.Name = "btStart";
      this.btStart.Size = new System.Drawing.Size(75, 23);
      this.btStart.TabIndex = 2;
      this.btStart.Text = "Start";
      this.btStart.UseVisualStyleBackColor = true;
      this.btStart.Click += new System.EventHandler(this.btStart_Click);
      // 
      // btClose
      // 
      this.btClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btClose.Location = new System.Drawing.Point(340, 331);
      this.btClose.Name = "btClose";
      this.btClose.Size = new System.Drawing.Size(75, 23);
      this.btClose.TabIndex = 0;
      this.btClose.Text = "Close";
      this.btClose.UseVisualStyleBackColor = true;
      this.btClose.Click += new System.EventHandler(this.btClose_Click);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(13, 293);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(293, 26);
      this.mpLabel1.TabIndex = 5;
      this.mpLabel1.Text = "Some Plugins must be stopped for the configuration to work. \r\nUse the Stop Button" +
          " in this case before pressing Config.";
      // 
      // MusicWinampPreview
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btClose;
      this.ClientSize = new System.Drawing.Size(462, 366);
      this.ControlBox = false;
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.btStop);
      this.Controls.Add(this.btConfig);
      this.Controls.Add(this.btStart);
      this.Controls.Add(this.pictureBoxVis);
      this.Controls.Add(this.btClose);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "MusicWinampPreview";
      this.Text = "MusicWinampPreview";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVis)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPButton btClose;
    private System.Windows.Forms.PictureBox pictureBoxVis;
    private MediaPortal.UserInterface.Controls.MPButton btStart;
    private MediaPortal.UserInterface.Controls.MPButton btConfig;
    private MediaPortal.UserInterface.Controls.MPButton btStop;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
  }
}