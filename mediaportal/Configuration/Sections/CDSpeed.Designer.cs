namespace MediaPortal.Configuration.Sections
{
  partial class CDSpeed
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
      this.dataGrid1 = new System.Windows.Forms.DataGrid();
      this.ckEnableCDSpeed = new MediaPortal.UserInterface.Controls.MPCheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
      this.SuspendLayout();
      // 
      // dataGrid1
      // 
      this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGrid1.DataMember = "";
      this.dataGrid1.FlatMode = true;
      this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.dataGrid1.Location = new System.Drawing.Point(14, 55);
      this.dataGrid1.Name = "dataGrid1";
      this.dataGrid1.Size = new System.Drawing.Size(440, 293);
      this.dataGrid1.TabIndex = 5;
      // 
      // ckEnableCDSpeed
      // 
      this.ckEnableCDSpeed.AutoSize = true;
      this.ckEnableCDSpeed.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ckEnableCDSpeed.Location = new System.Drawing.Point(14, 25);
      this.ckEnableCDSpeed.Name = "ckEnableCDSpeed";
      this.ckEnableCDSpeed.Size = new System.Drawing.Size(336, 17);
      this.ckEnableCDSpeed.TabIndex = 7;
      this.ckEnableCDSpeed.Text = "Set Maximum CD/DVD Speed, when playing  a DVD or Audio CD.";
      this.ckEnableCDSpeed.UseVisualStyleBackColor = true;
      // 
      // CDSpeed
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.ckEnableCDSpeed);
      this.Controls.Add(this.dataGrid1);
      this.Name = "CDSpeed";
      this.Size = new System.Drawing.Size(472, 408);
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.DataGrid dataGrid1;
    private MediaPortal.UserInterface.Controls.MPCheckBox ckEnableCDSpeed;
  }
}
