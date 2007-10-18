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
      this.cbShowAllChannelsGroup = new System.Windows.Forms.CheckBox();
      this.mpButtonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.label1 = new System.Windows.Forms.Label();
      this.comboBoxGroups = new System.Windows.Forms.ComboBox();
      this.cbRememberLastGroup = new System.Windows.Forms.CheckBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.cbRememberLastGroup);
      this.groupBox1.Controls.Add(this.comboBoxGroups);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.cbShowAllChannelsGroup);
      this.groupBox1.Location = new System.Drawing.Point(13, 13);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(374, 119);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Channel groups";
      // 
      // cbShowAllChannelsGroup
      // 
      this.cbShowAllChannelsGroup.AutoSize = true;
      this.cbShowAllChannelsGroup.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.cbShowAllChannelsGroup.Location = new System.Drawing.Point(165, 32);
      this.cbShowAllChannelsGroup.Name = "cbShowAllChannelsGroup";
      this.cbShowAllChannelsGroup.Size = new System.Drawing.Size(15, 14);
      this.cbShowAllChannelsGroup.TabIndex = 0;
      this.cbShowAllChannelsGroup.UseVisualStyleBackColor = true;
      this.cbShowAllChannelsGroup.Click += new System.EventHandler(this.cbShowAllChannelsGroup_Click);
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(237, 150);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 9;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.mpButtonOk.Location = new System.Drawing.Point(69, 150);
      this.mpButtonOk.Name = "mpButtonOk";
      this.mpButtonOk.Size = new System.Drawing.Size(75, 23);
      this.mpButtonOk.TabIndex = 8;
      this.mpButtonOk.Text = "Ok";
      this.mpButtonOk.UseVisualStyleBackColor = true;
      this.mpButtonOk.Click += new System.EventHandler(this.mpButtonOk_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(7, 78);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(108, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Group to show in root";
      // 
      // comboBoxGroups
      // 
      this.comboBoxGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxGroups.FormattingEnabled = true;
      this.comboBoxGroups.Location = new System.Drawing.Point(121, 75);
      this.comboBoxGroups.Name = "comboBoxGroups";
      this.comboBoxGroups.Size = new System.Drawing.Size(194, 21);
      this.comboBoxGroups.TabIndex = 2;
      this.comboBoxGroups.SelectedIndexChanged += new System.EventHandler(this.comboBoxGroups_SelectedIndexChanged);
      // 
      // cbRememberLastGroup
      // 
      this.cbRememberLastGroup.AutoSize = true;
      this.cbRememberLastGroup.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.cbRememberLastGroup.Location = new System.Drawing.Point(165, 52);
      this.cbRememberLastGroup.Name = "cbRememberLastGroup";
      this.cbRememberLastGroup.Size = new System.Drawing.Size(15, 14);
      this.cbRememberLastGroup.TabIndex = 3;
      this.cbRememberLastGroup.UseVisualStyleBackColor = true;
      this.cbRememberLastGroup.CheckedChanged += new System.EventHandler(this.cbRememberLastGroup_CheckedChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(10, 32);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(152, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "Show the \"All channels group\"";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(10, 52);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(107, 13);
      this.label3.TabIndex = 5;
      this.label3.Text = "Remember last group";
      // 
      // RadioSetupForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(399, 203);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonOk);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "RadioSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "MyRadio Setup";
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
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
  }
}