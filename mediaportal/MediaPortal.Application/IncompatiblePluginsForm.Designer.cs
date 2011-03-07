namespace MediaPortal
{
  partial class IncompatiblePluginsForm
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
      this.components = new System.ComponentModel.Container();
      MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
      this.CloseTimer = new System.Windows.Forms.Timer(this.components);
      this.bClose = new System.Windows.Forms.Button();
      this.PluginsList = new System.Windows.Forms.ListBox();
      mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.SuspendLayout();
      // 
      // mpLabel1
      // 
      mpLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      mpLabel1.Location = new System.Drawing.Point(12, 13);
      mpLabel1.Name = "mpLabel1";
      mpLabel1.Size = new System.Drawing.Size(372, 33);
      mpLabel1.TabIndex = 0;
      mpLabel1.Text = "The following plugins are incompatible with this version of MediaPortal and were " +
          "automatically disabled:";
      // 
      // CloseTimer
      // 
      this.CloseTimer.Enabled = true;
      this.CloseTimer.Interval = 1000;
      this.CloseTimer.Tick += new System.EventHandler(this.CloseTimerTick);
      // 
      // bClose
      // 
      this.bClose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.bClose.Location = new System.Drawing.Point(149, 219);
      this.bClose.Name = "bClose";
      this.bClose.Size = new System.Drawing.Size(88, 20);
      this.bClose.TabIndex = 0;
      this.bClose.Text = "Continue (15)";
      this.bClose.UseVisualStyleBackColor = true;
      this.bClose.Click += new System.EventHandler(this.bClose_Click);
      // 
      // PluginsList
      // 
      this.PluginsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.PluginsList.BackColor = System.Drawing.Color.OrangeRed;
      this.PluginsList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.PluginsList.Location = new System.Drawing.Point(12, 49);
      this.PluginsList.Name = "PluginsList";
      this.PluginsList.Size = new System.Drawing.Size(372, 158);
      this.PluginsList.TabIndex = 2;
      this.PluginsList.Enter += new System.EventHandler(this.PluginsList_Enter);
      this.PluginsList.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.PluginsList_Format);
      // 
      // IncompatiblePluginsForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.BackColor = System.Drawing.Color.OrangeRed;
      this.ClientSize = new System.Drawing.Size(396, 251);
      this.Controls.Add(this.PluginsList);
      this.Controls.Add(this.bClose);
      this.Controls.Add(mpLabel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "IncompatiblePluginsForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Warning! Incompatible plugins";
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Timer CloseTimer;
    private System.Windows.Forms.Button bClose;
    private System.Windows.Forms.ListBox PluginsList;
  }
}