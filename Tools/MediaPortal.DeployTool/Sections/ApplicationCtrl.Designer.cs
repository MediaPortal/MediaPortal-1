namespace MediaPortal.DeployTool.Sections
{
  partial class ApplicationCtrl
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
      this.components = new System.ComponentModel.Container();
      this.pbImage = new System.Windows.Forms.PictureBox();
      this.pbStatusIcon = new System.Windows.Forms.PictureBox();
      this.lbState = new System.Windows.Forms.Label();
      this.lbAction = new System.Windows.Forms.Label();
      this.lbApplication = new System.Windows.Forms.Label();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.pbImage)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbStatusIcon)).BeginInit();
      this.SuspendLayout();
      // 
      // pbImage
      // 
      this.pbImage.ErrorImage = null;
      this.pbImage.InitialImage = null;
      this.pbImage.Location = new System.Drawing.Point(6, 13);
      this.pbImage.Name = "pbImage";
      this.pbImage.Size = new System.Drawing.Size(70, 70);
      this.pbImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pbImage.TabIndex = 0;
      this.pbImage.TabStop = false;
      // 
      // pbStatusIcon
      // 
      this.pbStatusIcon.BackColor = System.Drawing.Color.Transparent;
      this.pbStatusIcon.ErrorImage = null;
      this.pbStatusIcon.InitialImage = null;
      this.pbStatusIcon.Location = new System.Drawing.Point(295, 61);
      this.pbStatusIcon.Name = "pbStatusIcon";
      this.pbStatusIcon.Size = new System.Drawing.Size(22, 22);
      this.pbStatusIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pbStatusIcon.TabIndex = 1;
      this.pbStatusIcon.TabStop = false;
      // 
      // lbState
      // 
      this.lbState.AutoEllipsis = true;
      this.lbState.BackColor = System.Drawing.Color.Transparent;
      this.lbState.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lbState.ForeColor = System.Drawing.Color.White;
      this.lbState.Location = new System.Drawing.Point(78, 48);
      this.lbState.Name = "lbState";
      this.lbState.Size = new System.Drawing.Size(219, 15);
      this.lbState.TabIndex = 2;
      this.lbState.Text = "State";
      // 
      // lbAction
      // 
      this.lbAction.AutoEllipsis = true;
      this.lbAction.BackColor = System.Drawing.Color.Transparent;
      this.lbAction.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lbAction.ForeColor = System.Drawing.Color.White;
      this.lbAction.Location = new System.Drawing.Point(78, 68);
      this.lbAction.Name = "lbAction";
      this.lbAction.Size = new System.Drawing.Size(219, 15);
      this.lbAction.TabIndex = 3;
      this.lbAction.Text = "Action";
      // 
      // lbApplication
      // 
      this.lbApplication.AutoEllipsis = true;
      this.lbApplication.BackColor = System.Drawing.Color.Transparent;
      this.lbApplication.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold);
      this.lbApplication.ForeColor = System.Drawing.Color.White;
      this.lbApplication.Location = new System.Drawing.Point(78, 13);
      this.lbApplication.Name = "lbApplication";
      this.lbApplication.Size = new System.Drawing.Size(239, 35);
      this.lbApplication.TabIndex = 4;
      this.lbApplication.Text = "Application";
      // 
      // ApplicationCtrl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(111)))), ((int)(((byte)(152)))));
      this.Controls.Add(this.lbApplication);
      this.Controls.Add(this.lbAction);
      this.Controls.Add(this.lbState);
      this.Controls.Add(this.pbStatusIcon);
      this.Controls.Add(this.pbImage);
      this.Name = "ApplicationCtrl";
      this.Size = new System.Drawing.Size(320, 95);
      ((System.ComponentModel.ISupportInitialize)(this.pbImage)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbStatusIcon)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox pbImage;
    private System.Windows.Forms.PictureBox pbStatusIcon;
    private System.Windows.Forms.Label lbState;
    private System.Windows.Forms.Label lbAction;
    private System.Windows.Forms.Label lbApplication;
    private System.Windows.Forms.ToolTip toolTip1;
  }
}
