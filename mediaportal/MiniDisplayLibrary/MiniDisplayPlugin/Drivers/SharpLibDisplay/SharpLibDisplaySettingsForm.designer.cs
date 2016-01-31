namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  partial class SharpLibDisplaySettingsForm
    {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

 
      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
      this.checkBoxSingleLine = new System.Windows.Forms.CheckBox();
      this.buttonOk = new System.Windows.Forms.Button();
      this.linkLabelDocumentation = new System.Windows.Forms.LinkLabel();
      this.groupBoxSingleLineOptions = new System.Windows.Forms.GroupBox();
      this.textBoxSingleLineSeparator = new System.Windows.Forms.TextBox();
      this.labelSingleLineSeparator = new System.Windows.Forms.Label();
      this.labelSingleLineMode = new System.Windows.Forms.Label();
      this.comboBoxSingleLineOptions = new System.Windows.Forms.ComboBox();
      this.groupBoxSingleLineOptions.SuspendLayout();
      this.SuspendLayout();
      // 
      // checkBoxSingleLine
      // 
      this.checkBoxSingleLine.AutoSize = true;
      this.checkBoxSingleLine.Location = new System.Drawing.Point(11, 11);
      this.checkBoxSingleLine.Margin = new System.Windows.Forms.Padding(2);
      this.checkBoxSingleLine.Name = "checkBoxSingleLine";
      this.checkBoxSingleLine.Size = new System.Drawing.Size(245, 17);
      this.checkBoxSingleLine.TabIndex = 0;
      this.checkBoxSingleLine.Text = "Single line - better readability on smaller display";
      this.checkBoxSingleLine.UseVisualStyleBackColor = true;
      // 
      // buttonOk
      // 
      this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOk.AutoSize = true;
      this.buttonOk.Location = new System.Drawing.Point(330, 117);
      this.buttonOk.Margin = new System.Windows.Forms.Padding(2);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(92, 29);
      this.buttonOk.TabIndex = 1;
      this.buttonOk.Text = "&OK";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // linkLabelDocumentation
      // 
      this.linkLabelDocumentation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabelDocumentation.AutoSize = true;
      this.linkLabelDocumentation.Location = new System.Drawing.Point(8, 134);
      this.linkLabelDocumentation.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.linkLabelDocumentation.Name = "linkLabelDocumentation";
      this.linkLabelDocumentation.Size = new System.Drawing.Size(112, 13);
      this.linkLabelDocumentation.TabIndex = 6;
      this.linkLabelDocumentation.TabStop = true;
      this.linkLabelDocumentation.Text = "Online Documentation";
      this.linkLabelDocumentation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDocumentation_LinkClicked);
      // 
      // groupBoxSingleLineOptions
      // 
      this.groupBoxSingleLineOptions.Controls.Add(this.textBoxSingleLineSeparator);
      this.groupBoxSingleLineOptions.Controls.Add(this.labelSingleLineSeparator);
      this.groupBoxSingleLineOptions.Controls.Add(this.labelSingleLineMode);
      this.groupBoxSingleLineOptions.Controls.Add(this.comboBoxSingleLineOptions);
      this.groupBoxSingleLineOptions.Location = new System.Drawing.Point(17, 12);
      this.groupBoxSingleLineOptions.Name = "groupBoxSingleLineOptions";
      this.groupBoxSingleLineOptions.Size = new System.Drawing.Size(401, 88);
      this.groupBoxSingleLineOptions.TabIndex = 7;
      this.groupBoxSingleLineOptions.TabStop = false;
      this.groupBoxSingleLineOptions.Text = "groupBox1";
      // 
      // textBoxSingleLineSeparator
      // 
      this.textBoxSingleLineSeparator.Location = new System.Drawing.Point(78, 51);
      this.textBoxSingleLineSeparator.Name = "textBoxSingleLineSeparator";
      this.textBoxSingleLineSeparator.Size = new System.Drawing.Size(100, 20);
      this.textBoxSingleLineSeparator.TabIndex = 13;
      // 
      // labelSingleLineSeparator
      // 
      this.labelSingleLineSeparator.AutoSize = true;
      this.labelSingleLineSeparator.Location = new System.Drawing.Point(15, 54);
      this.labelSingleLineSeparator.Name = "labelSingleLineSeparator";
      this.labelSingleLineSeparator.Size = new System.Drawing.Size(56, 13);
      this.labelSingleLineSeparator.TabIndex = 12;
      this.labelSingleLineSeparator.Text = "Separator:";
      // 
      // labelSingleLineMode
      // 
      this.labelSingleLineMode.AutoSize = true;
      this.labelSingleLineMode.Location = new System.Drawing.Point(16, 24);
      this.labelSingleLineMode.Name = "labelSingleLineMode";
      this.labelSingleLineMode.Size = new System.Drawing.Size(37, 13);
      this.labelSingleLineMode.TabIndex = 11;
      this.labelSingleLineMode.Text = "Mode:";
      // 
      // comboBoxSingleLineOptions
      // 
      this.comboBoxSingleLineOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSingleLineOptions.FormattingEnabled = true;
      this.comboBoxSingleLineOptions.Items.AddRange(new object[] {
            "Top line only",
            "Bottom line only",
            "Top and bottom lines concatenated",
            "Bottom and top lines concatenated"});
      this.comboBoxSingleLineOptions.Location = new System.Drawing.Point(78, 21);
      this.comboBoxSingleLineOptions.Name = "comboBoxSingleLineOptions";
      this.comboBoxSingleLineOptions.Size = new System.Drawing.Size(211, 21);
      this.comboBoxSingleLineOptions.TabIndex = 10;
      // 
      // SharpLibDisplaySettingsForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(430, 154);
      this.Controls.Add(this.linkLabelDocumentation);
      this.Controls.Add(this.buttonOk);
      this.Controls.Add(this.checkBoxSingleLine);
      this.Controls.Add(this.groupBoxSingleLineOptions);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Margin = new System.Windows.Forms.Padding(2);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SharpLibDisplaySettingsForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "MiniDisplay - Setup - Advanced Settings";
      this.groupBoxSingleLineOptions.ResumeLayout(false);
      this.groupBoxSingleLineOptions.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

      }

      #endregion

    private System.Windows.Forms.CheckBox checkBoxSingleLine;
    private System.Windows.Forms.Button buttonOk;
    private System.Windows.Forms.LinkLabel linkLabelDocumentation;
    private System.Windows.Forms.GroupBox groupBoxSingleLineOptions;
    private System.Windows.Forms.TextBox textBoxSingleLineSeparator;
    private System.Windows.Forms.Label labelSingleLineSeparator;
    private System.Windows.Forms.Label labelSingleLineMode;
    private System.Windows.Forms.ComboBox comboBoxSingleLineOptions;
    }
}