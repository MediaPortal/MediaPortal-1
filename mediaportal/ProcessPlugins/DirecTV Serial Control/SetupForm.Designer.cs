namespace ProcessPlugins.DirectTVTunerPlugin
{
  partial class SetupForm
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
      this.globalBox = new System.Windows.Forms.GroupBox();
      this.cbAdvanced = new System.Windows.Forms.CheckBox();
      this.typeLabel = new System.Windows.Forms.Label();
      this.modelComboBox = new System.Windows.Forms.ComboBox();
      this.portComboBox = new System.Windows.Forms.ComboBox();
      this.serialPortLabel = new System.Windows.Forms.Label();
      this.keyMapLabel = new System.Windows.Forms.Label();
      this.keyMapComboBox = new System.Windows.Forms.ComboBox();
      this.brLabel = new System.Windows.Forms.Label();
      this.baudComboBox = new System.Windows.Forms.ComboBox();
      this.advancedBox = new System.Windows.Forms.GroupBox();
      this.cbAllowSubChannels = new System.Windows.Forms.CheckBox();
      this.cbHideOSD = new System.Windows.Forms.CheckBox();
      this.cbtwowaydisable = new System.Windows.Forms.CheckBox();
      this.debugBox = new System.Windows.Forms.CheckBox();
      this.powerOnBox = new System.Windows.Forms.CheckBox();
      this.channelSetBox = new System.Windows.Forms.CheckBox();
      this.commandSetBox = new System.Windows.Forms.CheckBox();
      this.communicationBox = new System.Windows.Forms.GroupBox();
      this.timeoutLabel = new System.Windows.Forms.Label();
      this.timeoutNumUpDown = new System.Windows.Forms.NumericUpDown();
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.globalBox.SuspendLayout();
      this.advancedBox.SuspendLayout();
      this.communicationBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.timeoutNumUpDown)).BeginInit();
      this.SuspendLayout();
      // 
      // globalBox
      // 
      this.globalBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.globalBox.Controls.Add(this.cbAdvanced);
      this.globalBox.Controls.Add(this.typeLabel);
      this.globalBox.Controls.Add(this.modelComboBox);
      this.globalBox.Location = new System.Drawing.Point(12, 12);
      this.globalBox.Name = "globalBox";
      this.globalBox.Size = new System.Drawing.Size(209, 70);
      this.globalBox.TabIndex = 0;
      this.globalBox.TabStop = false;
      this.globalBox.Text = "Global";
      // 
      // cbAdvanced
      // 
      this.cbAdvanced.AutoSize = true;
      this.cbAdvanced.Location = new System.Drawing.Point(35, 47);
      this.cbAdvanced.Name = "cbAdvanced";
      this.cbAdvanced.Size = new System.Drawing.Size(149, 17);
      this.cbAdvanced.TabIndex = 3;
      this.cbAdvanced.Text = "Enable advanced settings";
      this.cbAdvanced.UseVisualStyleBackColor = true;
      this.cbAdvanced.CheckedChanged += new System.EventHandler(this.cbAdvanded_CheckedChanged);
      // 
      // typeLabel
      // 
      this.typeLabel.AutoSize = true;
      this.typeLabel.Location = new System.Drawing.Point(6, 23);
      this.typeLabel.Name = "typeLabel";
      this.typeLabel.Size = new System.Drawing.Size(39, 13);
      this.typeLabel.TabIndex = 2;
      this.typeLabel.Text = "Model:";
      // 
      // modelComboBox
      // 
      this.modelComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.modelComboBox.FormattingEnabled = true;
      this.modelComboBox.Location = new System.Drawing.Point(51, 19);
      this.modelComboBox.Name = "modelComboBox";
      this.modelComboBox.Size = new System.Drawing.Size(152, 21);
      this.modelComboBox.TabIndex = 1;
      this.modelComboBox.SelectedIndexChanged += new System.EventHandler(this.modelComboBox_SelectedIndexChanged);
      // 
      // portComboBox
      // 
      this.portComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.portComboBox.FormattingEnabled = true;
      this.portComboBox.Location = new System.Drawing.Point(88, 19);
      this.portComboBox.Name = "portComboBox";
      this.portComboBox.Size = new System.Drawing.Size(115, 21);
      this.portComboBox.TabIndex = 3;
      // 
      // serialPortLabel
      // 
      this.serialPortLabel.AutoSize = true;
      this.serialPortLabel.Location = new System.Drawing.Point(28, 22);
      this.serialPortLabel.Name = "serialPortLabel";
      this.serialPortLabel.Size = new System.Drawing.Size(57, 13);
      this.serialPortLabel.TabIndex = 2;
      this.serialPortLabel.Text = "Serial port:";
      // 
      // keyMapLabel
      // 
      this.keyMapLabel.AutoSize = true;
      this.keyMapLabel.Location = new System.Drawing.Point(7, 23);
      this.keyMapLabel.Name = "keyMapLabel";
      this.keyMapLabel.Size = new System.Drawing.Size(48, 13);
      this.keyMapLabel.TabIndex = 0;
      this.keyMapLabel.Text = "Keymap:";
      // 
      // keyMapComboBox
      // 
      this.keyMapComboBox.FormattingEnabled = true;
      this.keyMapComboBox.Location = new System.Drawing.Point(55, 19);
      this.keyMapComboBox.Name = "keyMapComboBox";
      this.keyMapComboBox.Size = new System.Drawing.Size(128, 21);
      this.keyMapComboBox.TabIndex = 1;
      // 
      // brLabel
      // 
      this.brLabel.AutoSize = true;
      this.brLabel.Location = new System.Drawing.Point(29, 50);
      this.brLabel.Name = "brLabel";
      this.brLabel.Size = new System.Drawing.Size(56, 13);
      this.brLabel.TabIndex = 2;
      this.brLabel.Text = "Baud rate:";
      // 
      // baudComboBox
      // 
      this.baudComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.baudComboBox.FormattingEnabled = true;
      this.baudComboBox.Location = new System.Drawing.Point(88, 46);
      this.baudComboBox.Name = "baudComboBox";
      this.baudComboBox.Size = new System.Drawing.Size(115, 21);
      this.baudComboBox.TabIndex = 3;
      // 
      // advancedBox
      // 
      this.advancedBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.advancedBox.Controls.Add(this.cbAllowSubChannels);
      this.advancedBox.Controls.Add(this.cbHideOSD);
      this.advancedBox.Controls.Add(this.cbtwowaydisable);
      this.advancedBox.Controls.Add(this.debugBox);
      this.advancedBox.Controls.Add(this.powerOnBox);
      this.advancedBox.Controls.Add(this.channelSetBox);
      this.advancedBox.Controls.Add(this.commandSetBox);
      this.advancedBox.Controls.Add(this.keyMapComboBox);
      this.advancedBox.Controls.Add(this.keyMapLabel);
      this.advancedBox.Location = new System.Drawing.Point(227, 12);
      this.advancedBox.Name = "advancedBox";
      this.advancedBox.Size = new System.Drawing.Size(191, 216);
      this.advancedBox.TabIndex = 1;
      this.advancedBox.TabStop = false;
      this.advancedBox.Text = "Advanced settings";
      // 
      // cbAllowSubChannels
      // 
      this.cbAllowSubChannels.AutoSize = true;
      this.cbAllowSubChannels.Location = new System.Drawing.Point(10, 177);
      this.cbAllowSubChannels.Margin = new System.Windows.Forms.Padding(2);
      this.cbAllowSubChannels.Name = "cbAllowSubChannels";
      this.cbAllowSubChannels.Size = new System.Drawing.Size(149, 17);
      this.cbAllowSubChannels.TabIndex = 8;
      this.cbAllowSubChannels.Text = "Allow Digital SubChannels";
      this.toolTip1.SetToolTip(this.cbAllowSubChannels, "This may only work for HTL-HD boxes using the RCA New command Set.\r\nIt will allow" +
              " tuning to digital subchannels if the external channel is in the \r\nform 9-2, 000" +
              "902, 32-1, 003201");
      this.cbAllowSubChannels.UseVisualStyleBackColor = true;
      // 
      // cbHideOSD
      // 
      this.cbHideOSD.AutoSize = true;
      this.cbHideOSD.Location = new System.Drawing.Point(10, 156);
      this.cbHideOSD.Margin = new System.Windows.Forms.Padding(2);
      this.cbHideOSD.Name = "cbHideOSD";
      this.cbHideOSD.Size = new System.Drawing.Size(74, 17);
      this.cbHideOSD.TabIndex = 7;
      this.cbHideOSD.Text = "Hide OSD";
      this.toolTip1.SetToolTip(this.cbHideOSD, "This may only work for D10-100 boxes");
      this.cbHideOSD.UseVisualStyleBackColor = true;
      // 
      // cbtwowaydisable
      // 
      this.cbtwowaydisable.AutoSize = true;
      this.cbtwowaydisable.Location = new System.Drawing.Point(10, 135);
      this.cbtwowaydisable.Margin = new System.Windows.Forms.Padding(2);
      this.cbtwowaydisable.Name = "cbtwowaydisable";
      this.cbtwowaydisable.Size = new System.Drawing.Size(177, 17);
      this.cbtwowaydisable.TabIndex = 6;
      this.cbtwowaydisable.Text = "Disable two-way communication";
      this.toolTip1.SetToolTip(this.cbtwowaydisable, "For D10-100 Boxes");
      this.cbtwowaydisable.UseVisualStyleBackColor = true;
      // 
      // debugBox
      // 
      this.debugBox.AutoSize = true;
      this.debugBox.Location = new System.Drawing.Point(10, 113);
      this.debugBox.Name = "debugBox";
      this.debugBox.Size = new System.Drawing.Size(144, 17);
      this.debugBox.TabIndex = 5;
      this.debugBox.Text = "Enable extensive logging";
      this.debugBox.UseVisualStyleBackColor = true;
      // 
      // powerOnBox
      // 
      this.powerOnBox.AutoSize = true;
      this.powerOnBox.Location = new System.Drawing.Point(10, 91);
      this.powerOnBox.Name = "powerOnBox";
      this.powerOnBox.Size = new System.Drawing.Size(157, 17);
      this.powerOnBox.TabIndex = 4;
      this.powerOnBox.Text = "Use power-on before tuning";
      this.powerOnBox.UseVisualStyleBackColor = true;
      // 
      // channelSetBox
      // 
      this.channelSetBox.AutoSize = true;
      this.channelSetBox.Location = new System.Drawing.Point(10, 69);
      this.channelSetBox.Name = "channelSetBox";
      this.channelSetBox.Size = new System.Drawing.Size(160, 17);
      this.channelSetBox.TabIndex = 3;
      this.channelSetBox.Text = "Use \"channel set\" for tuning";
      this.channelSetBox.UseVisualStyleBackColor = true;
      // 
      // commandSetBox
      // 
      this.commandSetBox.AutoSize = true;
      this.commandSetBox.Location = new System.Drawing.Point(10, 47);
      this.commandSetBox.Name = "commandSetBox";
      this.commandSetBox.Size = new System.Drawing.Size(128, 17);
      this.commandSetBox.TabIndex = 2;
      this.commandSetBox.Text = "Use old command set";
      this.toolTip1.SetToolTip(this.commandSetBox, "If your receiver is more than two years old, you may need to check this box");
      this.commandSetBox.UseVisualStyleBackColor = true;
      // 
      // communicationBox
      // 
      this.communicationBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.communicationBox.Controls.Add(this.timeoutLabel);
      this.communicationBox.Controls.Add(this.timeoutNumUpDown);
      this.communicationBox.Controls.Add(this.baudComboBox);
      this.communicationBox.Controls.Add(this.serialPortLabel);
      this.communicationBox.Controls.Add(this.portComboBox);
      this.communicationBox.Controls.Add(this.brLabel);
      this.communicationBox.Location = new System.Drawing.Point(12, 88);
      this.communicationBox.Name = "communicationBox";
      this.communicationBox.Size = new System.Drawing.Size(209, 140);
      this.communicationBox.TabIndex = 2;
      this.communicationBox.TabStop = false;
      this.communicationBox.Text = "Communication settings";
      // 
      // timeoutLabel
      // 
      this.timeoutLabel.AutoSize = true;
      this.timeoutLabel.Location = new System.Drawing.Point(12, 77);
      this.timeoutLabel.Name = "timeoutLabel";
      this.timeoutLabel.Size = new System.Drawing.Size(73, 13);
      this.timeoutLabel.TabIndex = 5;
      this.timeoutLabel.Text = "Read timeout:";
      // 
      // timeoutNumUpDown
      // 
      this.timeoutNumUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.timeoutNumUpDown.Location = new System.Drawing.Point(88, 74);
      this.timeoutNumUpDown.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
      this.timeoutNumUpDown.Minimum = new decimal(new int[] {
            500,
            0,
            0,
            0});
      this.timeoutNumUpDown.Name = "timeoutNumUpDown";
      this.timeoutNumUpDown.Size = new System.Drawing.Size(115, 20);
      this.timeoutNumUpDown.TabIndex = 4;
      this.toolTip1.SetToolTip(this.timeoutNumUpDown, "Lowering this number may speed up channel changing");
      this.timeoutNumUpDown.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(262, 234);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 3;
      this.okButton.Text = "&OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(343, 234);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 4;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // SetupForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(430, 269);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.communicationBox);
      this.Controls.Add(this.advancedBox);
      this.Controls.Add(this.globalBox);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "SetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "DirecTV serial tuner - Setup";
      this.globalBox.ResumeLayout(false);
      this.globalBox.PerformLayout();
      this.advancedBox.ResumeLayout(false);
      this.advancedBox.PerformLayout();
      this.communicationBox.ResumeLayout(false);
      this.communicationBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.timeoutNumUpDown)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox globalBox;
    private System.Windows.Forms.Label typeLabel;
    private System.Windows.Forms.ComboBox modelComboBox;
    private System.Windows.Forms.ComboBox portComboBox;
    private System.Windows.Forms.Label serialPortLabel;
    private System.Windows.Forms.Label keyMapLabel;
    private System.Windows.Forms.ComboBox keyMapComboBox;
    private System.Windows.Forms.Label brLabel;
    private System.Windows.Forms.ComboBox baudComboBox;
    private System.Windows.Forms.GroupBox advancedBox;
    private System.Windows.Forms.GroupBox communicationBox;
    private System.Windows.Forms.Label timeoutLabel;
    private System.Windows.Forms.NumericUpDown timeoutNumUpDown;
    private System.Windows.Forms.CheckBox commandSetBox;
    private System.Windows.Forms.CheckBox channelSetBox;
    private System.Windows.Forms.CheckBox debugBox;
    private System.Windows.Forms.CheckBox powerOnBox;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.CheckBox cbAdvanced;
    private System.Windows.Forms.CheckBox cbtwowaydisable;
    private System.Windows.Forms.CheckBox cbHideOSD;
    private System.Windows.Forms.ToolTip toolTip1;
     private System.Windows.Forms.CheckBox cbAllowSubChannels;
  }
}