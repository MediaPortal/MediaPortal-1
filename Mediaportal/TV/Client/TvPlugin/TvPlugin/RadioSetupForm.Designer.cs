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
      this.cbRememberLastGroup = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.comboBoxGroups = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbShowAllChannelsGroup = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpButtonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox5 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbTurnOnRadio = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox1.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.cbRememberLastGroup);
      this.groupBox1.Controls.Add(this.comboBoxGroups);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.cbShowAllChannelsGroup);
      this.groupBox1.Location = new System.Drawing.Point(13, 13);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(235, 118);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Channel groups";
      // 
      // cbRememberLastGroup
      // 
      this.cbRememberLastGroup.AutoSize = true;
      this.cbRememberLastGroup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbRememberLastGroup.Location = new System.Drawing.Point(13, 42);
      this.cbRememberLastGroup.Name = "cbRememberLastGroup";
      this.cbRememberLastGroup.Size = new System.Drawing.Size(124, 17);
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
      this.cbShowAllChannelsGroup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbShowAllChannelsGroup.Location = new System.Drawing.Point(13, 19);
      this.cbShowAllChannelsGroup.Name = "cbShowAllChannelsGroup";
      this.cbShowAllChannelsGroup.Size = new System.Drawing.Size(169, 17);
      this.cbShowAllChannelsGroup.TabIndex = 0;
      this.cbShowAllChannelsGroup.Text = "Show the \"All channels group\"";
      this.cbShowAllChannelsGroup.UseVisualStyleBackColor = true;
      this.cbShowAllChannelsGroup.Click += new System.EventHandler(this.cbShowAllChannelsGroup_Click);
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(173, 187);
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
      this.mpButtonOk.Location = new System.Drawing.Point(92, 187);
      this.mpButtonOk.Name = "mpButtonOk";
      this.mpButtonOk.Size = new System.Drawing.Size(75, 23);
      this.mpButtonOk.TabIndex = 8;
      this.mpButtonOk.Text = "&OK";
      this.mpButtonOk.UseVisualStyleBackColor = true;
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // groupBox5
      // 
      this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox5.Controls.Add(this.cbTurnOnRadio);
      this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox5.Location = new System.Drawing.Point(13, 137);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(235, 44);
      this.groupBox5.TabIndex = 10;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "When entering the Radio screen:";
      // 
      // cbTurnOnRadio
      // 
      this.cbTurnOnRadio.AutoSize = true;
      this.cbTurnOnRadio.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbTurnOnRadio.Location = new System.Drawing.Point(13, 19);
      this.cbTurnOnRadio.Name = "cbTurnOnRadio";
      this.cbTurnOnRadio.Size = new System.Drawing.Size(92, 17);
      this.cbTurnOnRadio.TabIndex = 0;
      this.cbTurnOnRadio.Text = "Turn on Radio";
      this.cbTurnOnRadio.UseVisualStyleBackColor = true;
      this.cbTurnOnRadio.CheckedChanged += new System.EventHandler(this.cbTurnOnRadio_CheckedChanged);
      // 
      // RadioSetupForm
      // 
      this.AcceptButton = this.mpButtonOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(260, 222);
      this.Controls.Add(this.groupBox5);
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
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbShowAllChannelsGroup;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonCancel;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonOk;
    private System.Windows.Forms.ComboBox comboBoxGroups;
    private System.Windows.Forms.Label label1;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbRememberLastGroup;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox5;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbTurnOnRadio;
  }
}