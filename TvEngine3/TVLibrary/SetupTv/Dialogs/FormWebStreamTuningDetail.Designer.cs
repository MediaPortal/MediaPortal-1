namespace SetupTv.Dialogs
{
  partial class FormWebStreamTuningDetail
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
      this.btnSearchSHOUTcast = new System.Windows.Forms.Button();
      this.edStreamURL = new System.Windows.Forms.TextBox();
      this.label45 = new System.Windows.Forms.Label();
      this.nudStreamBitrate = new System.Windows.Forms.NumericUpDown();
      this.label44 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.nudStreamBitrate)).BeginInit();
      this.SuspendLayout();
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Location = new System.Drawing.Point(311, 139);
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.Location = new System.Drawing.Point(230, 139);
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // btnSearchSHOUTcast
      // 
      this.btnSearchSHOUTcast.Location = new System.Drawing.Point(12, 74);
      this.btnSearchSHOUTcast.Name = "btnSearchSHOUTcast";
      this.btnSearchSHOUTcast.Size = new System.Drawing.Size(124, 23);
      this.btnSearchSHOUTcast.TabIndex = 10;
      this.btnSearchSHOUTcast.Text = "Search SHOUTcast";
      this.btnSearchSHOUTcast.UseVisualStyleBackColor = true;
      this.btnSearchSHOUTcast.Click += new System.EventHandler(this.btnSearchSHOUTcast_Click);
      // 
      // edStreamURL
      // 
      this.edStreamURL.Location = new System.Drawing.Point(52, 12);
      this.edStreamURL.Name = "edStreamURL";
      this.edStreamURL.Size = new System.Drawing.Size(335, 20);
      this.edStreamURL.TabIndex = 7;
      // 
      // label45
      // 
      this.label45.AutoSize = true;
      this.label45.Location = new System.Drawing.Point(9, 15);
      this.label45.Name = "label45";
      this.label45.Size = new System.Drawing.Size(32, 13);
      this.label45.TabIndex = 11;
      this.label45.Text = "URL:";
      // 
      // nudStreamBitrate
      // 
      this.nudStreamBitrate.Location = new System.Drawing.Point(52, 38);
      this.nudStreamBitrate.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
      this.nudStreamBitrate.Name = "nudStreamBitrate";
      this.nudStreamBitrate.Size = new System.Drawing.Size(53, 20);
      this.nudStreamBitrate.TabIndex = 8;
      // 
      // label44
      // 
      this.label44.AutoSize = true;
      this.label44.Location = new System.Drawing.Point(9, 41);
      this.label44.Name = "label44";
      this.label44.Size = new System.Drawing.Size(40, 13);
      this.label44.TabIndex = 9;
      this.label44.Text = "Bitrate:";
      // 
      // FormWebStreamTuningDetail
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(398, 174);
      this.Controls.Add(this.btnSearchSHOUTcast);
      this.Controls.Add(this.edStreamURL);
      this.Controls.Add(this.label45);
      this.Controls.Add(this.nudStreamBitrate);
      this.Controls.Add(this.label44);
      this.Name = "FormWebStreamTuningDetail";
      this.Text = "Add / Edit Web-Stream Tuningdetail";
      this.Load += new System.EventHandler(this.FormWebStreamTuningDetail_Load);
      this.Controls.SetChildIndex(this.mpButtonOk, 0);
      this.Controls.SetChildIndex(this.mpButtonCancel, 0);
      this.Controls.SetChildIndex(this.label44, 0);
      this.Controls.SetChildIndex(this.nudStreamBitrate, 0);
      this.Controls.SetChildIndex(this.label45, 0);
      this.Controls.SetChildIndex(this.edStreamURL, 0);
      this.Controls.SetChildIndex(this.btnSearchSHOUTcast, 0);
      ((System.ComponentModel.ISupportInitialize)(this.nudStreamBitrate)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnSearchSHOUTcast;
    private System.Windows.Forms.TextBox edStreamURL;
    private System.Windows.Forms.Label label45;
    private System.Windows.Forms.NumericUpDown nudStreamBitrate;
    private System.Windows.Forms.Label label44;
  }
}
