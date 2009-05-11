namespace TvPlugin
{
  partial class RadioSetupForm
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbRememberLastGroup = new System.Windows.Forms.CheckBox();
      this.comboBoxGroups = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbShowAllChannelsGroup = new System.Windows.Forms.CheckBox();
      this.mpButtonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.cbRememberLastGroup);
      this.groupBox1.Controls.Add(this.comboBoxGroups);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.cbShowAllChannelsGroup);
      this.groupBox1.Location = new System.Drawing.Point(13, 13);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(235, 110);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Channel groups";
      // 
      // cbRememberLastGroup
      // 
      this.cbRememberLastGroup.AutoSize = true;
      this.cbRememberLastGroup.Location = new System.Drawing.Point(13, 42);
      this.cbRememberLastGroup.Name = "cbRememberLastGroup";
      this.cbRememberLastGroup.Size = new System.Drawing.Size(126, 17);
      this.cbRememberLastGroup.TabIndex = 3;
      this.cbRememberLastGroup.Text = "Remember last group";
      this.cbRememberLastGroup.UseVisualStyleBackColor = true;
      this.cbRememberLastGroup.CheckedChanged += new System.EventHandler(this.cbRememberLastGroup_CheckedChanged);
      // 
      // comboBoxGroups
      // 
      this.comboBoxGroups.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxGroups.FormattingEnabled = true;
      this.comboBoxGroups.Location = new System.Drawing.Point(13, 83);
      this.comboBoxGroups.Name = "comboBoxGroups";
      this.comboBoxGroups.Size = new System.Drawing.Size(204, 21);
      this.comboBoxGroups.TabIndex = 2;
      this.comboBoxGroups.SelectedIndexChanged += new System.EventHandler(this.comboBoxGroups_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(10, 67);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(140, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Group to show in root menu:";
      // 
      // cbShowAllChannelsGroup
      // 
      this.cbShowAllChannelsGroup.AutoSize = true;
      this.cbShowAllChannelsGroup.Location = new System.Drawing.Point(13, 19);
      this.cbShowAllChannelsGroup.Name = "cbShowAllChannelsGroup";
      this.cbShowAllChannelsGroup.Size = new System.Drawing.Size(171, 17);
      this.cbShowAllChannelsGroup.TabIndex = 0;
      this.cbShowAllChannelsGroup.Text = "Show the \"All channels group\"";
      this.cbShowAllChannelsGroup.UseVisualStyleBackColor = true;
      this.cbShowAllChannelsGroup.Click += new System.EventHandler(this.cbShowAllChannelsGroup_Click);
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(173, 129);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 9;
      this.mpButtonCancel.Text = "&Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.mpButtonOk.Location = new System.Drawing.Point(92, 129);
      this.mpButtonOk.Name = "mpButtonOk";
      this.mpButtonOk.Size = new System.Drawing.Size(75, 23);
      this.mpButtonOk.TabIndex = 8;
      this.mpButtonOk.Text = "&OK";
      this.mpButtonOk.UseVisualStyleBackColor = true;
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // RadioSetupForm
      // 
      this.AcceptButton = this.mpButtonOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(260, 164);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonOk);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "RadioSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "MyRadio - Setup";
      this.Load += new System.EventHandler(this.RadioSetupForm_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox cbShowAllChannelsGroup;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonCancel;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonOk;
    private System.Windows.Forms.ComboBox comboBoxGroups;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckBox cbRememberLastGroup;
  }
}