namespace ProcessPlugins.ComSkipLauncher
{
  partial class Configuration
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
      this.buttonOK = new System.Windows.Forms.Button();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.buttonTest = new System.Windows.Forms.Button();
      this.textBoxTest = new System.Windows.Forms.TextBox();
      this.buttonFindTestFile = new System.Windows.Forms.Button();
      this.radioButtonStart = new System.Windows.Forms.RadioButton();
      this.radioButtonEnd = new System.Windows.Forms.RadioButton();
      this.toolTips = new System.Windows.Forms.ToolTip(this.components);
      this.textBoxParameters = new System.Windows.Forms.TextBox();
      this.buttonParameters = new System.Windows.Forms.Button();
      this.buttonProgram = new System.Windows.Forms.Button();
      this.textBoxProgram = new System.Windows.Forms.TextBox();
      this.groupBoxWhat = new System.Windows.Forms.GroupBox();
      this.labelParameters = new System.Windows.Forms.Label();
      this.labelProgram = new System.Windows.Forms.Label();
      this.groupBoxTest = new System.Windows.Forms.GroupBox();
      this.groupBoxWhen = new System.Windows.Forms.GroupBox();
      this.labelComSkip = new System.Windows.Forms.Label();
      this.linkLabel = new System.Windows.Forms.LinkLabel();
      this.groupBoxWhat.SuspendLayout();
      this.groupBoxTest.SuspendLayout();
      this.groupBoxWhen.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonOK
      // 
      this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOK.Location = new System.Drawing.Point(216, 304);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(56, 24);
      this.buttonOK.TabIndex = 5;
      this.buttonOK.Text = "&OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(280, 304);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(56, 24);
      this.buttonCancel.TabIndex = 6;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // openFileDialog
      // 
      this.openFileDialog.Filter = "All files|*.*";
      // 
      // buttonTest
      // 
      this.buttonTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonTest.Location = new System.Drawing.Point(272, 24);
      this.buttonTest.Name = "buttonTest";
      this.buttonTest.Size = new System.Drawing.Size(48, 20);
      this.buttonTest.TabIndex = 2;
      this.buttonTest.Text = "Test";
      this.toolTips.SetToolTip(this.buttonTest, "Click here to test the above settings");
      this.buttonTest.UseVisualStyleBackColor = true;
      this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
      // 
      // textBoxTest
      // 
      this.textBoxTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxTest.Location = new System.Drawing.Point(8, 24);
      this.textBoxTest.Name = "textBoxTest";
      this.textBoxTest.Size = new System.Drawing.Size(224, 20);
      this.textBoxTest.TabIndex = 0;
      this.toolTips.SetToolTip(this.textBoxTest, "File to test launch with");
      // 
      // buttonFindTestFile
      // 
      this.buttonFindTestFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonFindTestFile.Location = new System.Drawing.Point(240, 24);
      this.buttonFindTestFile.Name = "buttonFindTestFile";
      this.buttonFindTestFile.Size = new System.Drawing.Size(24, 20);
      this.buttonFindTestFile.TabIndex = 1;
      this.buttonFindTestFile.Text = "...";
      this.toolTips.SetToolTip(this.buttonFindTestFile, "Click here to locate a file to test with");
      this.buttonFindTestFile.UseVisualStyleBackColor = true;
      this.buttonFindTestFile.Click += new System.EventHandler(this.buttonFindTestFile_Click);
      // 
      // radioButtonStart
      // 
      this.radioButtonStart.Location = new System.Drawing.Point(8, 16);
      this.radioButtonStart.Name = "radioButtonStart";
      this.radioButtonStart.Size = new System.Drawing.Size(120, 24);
      this.radioButtonStart.TabIndex = 0;
      this.radioButtonStart.TabStop = true;
      this.radioButtonStart.Text = "Recording Start";
      this.toolTips.SetToolTip(this.radioButtonStart, "Launch ComSkip as soon as the recording starts (this requires the LiveTV setting " +
              "in comskip.ini)");
      this.radioButtonStart.UseVisualStyleBackColor = true;
      // 
      // radioButtonEnd
      // 
      this.radioButtonEnd.Checked = true;
      this.radioButtonEnd.Location = new System.Drawing.Point(144, 16);
      this.radioButtonEnd.Name = "radioButtonEnd";
      this.radioButtonEnd.Size = new System.Drawing.Size(120, 24);
      this.radioButtonEnd.TabIndex = 1;
      this.radioButtonEnd.TabStop = true;
      this.radioButtonEnd.Text = "Recording End";
      this.toolTips.SetToolTip(this.radioButtonEnd, "Launch ComSkip to process the recording after it has finished");
      this.radioButtonEnd.UseVisualStyleBackColor = true;
      // 
      // textBoxParameters
      // 
      this.textBoxParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxParameters.Location = new System.Drawing.Point(8, 80);
      this.textBoxParameters.Name = "textBoxParameters";
      this.textBoxParameters.Size = new System.Drawing.Size(280, 20);
      this.textBoxParameters.TabIndex = 4;
      this.toolTips.SetToolTip(this.textBoxParameters, "Provide command line parameters for the program");
      // 
      // buttonParameters
      // 
      this.buttonParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonParameters.Location = new System.Drawing.Point(296, 80);
      this.buttonParameters.Name = "buttonParameters";
      this.buttonParameters.Size = new System.Drawing.Size(24, 20);
      this.buttonParameters.TabIndex = 5;
      this.buttonParameters.Text = "?";
      this.toolTips.SetToolTip(this.buttonParameters, "Click here for a list of additional command line parameters");
      this.buttonParameters.UseVisualStyleBackColor = true;
      this.buttonParameters.Click += new System.EventHandler(this.buttonParamQuestion_Click);
      // 
      // buttonProgram
      // 
      this.buttonProgram.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonProgram.Location = new System.Drawing.Point(296, 40);
      this.buttonProgram.Name = "buttonProgram";
      this.buttonProgram.Size = new System.Drawing.Size(24, 20);
      this.buttonProgram.TabIndex = 2;
      this.buttonProgram.Text = "...";
      this.toolTips.SetToolTip(this.buttonProgram, "Locate the program you wish to launch");
      this.buttonProgram.UseVisualStyleBackColor = true;
      this.buttonProgram.Click += new System.EventHandler(this.buttonProgram_Click);
      // 
      // textBoxProgram
      // 
      this.textBoxProgram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxProgram.Location = new System.Drawing.Point(8, 40);
      this.textBoxProgram.Name = "textBoxProgram";
      this.textBoxProgram.Size = new System.Drawing.Size(280, 20);
      this.textBoxProgram.TabIndex = 1;
      this.toolTips.SetToolTip(this.textBoxProgram, "The full location of the program to launch");
      // 
      // groupBoxWhat
      // 
      this.groupBoxWhat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxWhat.Controls.Add(this.labelParameters);
      this.groupBoxWhat.Controls.Add(this.textBoxParameters);
      this.groupBoxWhat.Controls.Add(this.buttonParameters);
      this.groupBoxWhat.Controls.Add(this.labelProgram);
      this.groupBoxWhat.Controls.Add(this.buttonProgram);
      this.groupBoxWhat.Controls.Add(this.textBoxProgram);
      this.groupBoxWhat.Location = new System.Drawing.Point(8, 64);
      this.groupBoxWhat.Name = "groupBoxWhat";
      this.groupBoxWhat.Size = new System.Drawing.Size(328, 112);
      this.groupBoxWhat.TabIndex = 1;
      this.groupBoxWhat.TabStop = false;
      this.groupBoxWhat.Text = "What to launch";
      // 
      // labelParameters
      // 
      this.labelParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelParameters.Location = new System.Drawing.Point(8, 64);
      this.labelParameters.Name = "labelParameters";
      this.labelParameters.Size = new System.Drawing.Size(280, 16);
      this.labelParameters.TabIndex = 3;
      this.labelParameters.Text = "Parameters:";
      this.labelParameters.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelProgram
      // 
      this.labelProgram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelProgram.Location = new System.Drawing.Point(8, 24);
      this.labelProgram.Name = "labelProgram";
      this.labelProgram.Size = new System.Drawing.Size(280, 16);
      this.labelProgram.TabIndex = 0;
      this.labelProgram.Text = "Program:";
      this.labelProgram.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBoxTest
      // 
      this.groupBoxTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxTest.Controls.Add(this.textBoxTest);
      this.groupBoxTest.Controls.Add(this.buttonTest);
      this.groupBoxTest.Controls.Add(this.buttonFindTestFile);
      this.groupBoxTest.Location = new System.Drawing.Point(8, 184);
      this.groupBoxTest.Name = "groupBoxTest";
      this.groupBoxTest.Size = new System.Drawing.Size(328, 56);
      this.groupBoxTest.TabIndex = 2;
      this.groupBoxTest.TabStop = false;
      this.groupBoxTest.Text = "Test launch";
      // 
      // groupBoxWhen
      // 
      this.groupBoxWhen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxWhen.Controls.Add(this.radioButtonStart);
      this.groupBoxWhen.Controls.Add(this.radioButtonEnd);
      this.groupBoxWhen.Location = new System.Drawing.Point(8, 8);
      this.groupBoxWhen.Name = "groupBoxWhen";
      this.groupBoxWhen.Size = new System.Drawing.Size(328, 48);
      this.groupBoxWhen.TabIndex = 0;
      this.groupBoxWhen.TabStop = false;
      this.groupBoxWhen.Text = "When to launch";
      // 
      // labelComSkip
      // 
      this.labelComSkip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelComSkip.Location = new System.Drawing.Point(8, 248);
      this.labelComSkip.Name = "labelComSkip";
      this.labelComSkip.Size = new System.Drawing.Size(328, 16);
      this.labelComSkip.TabIndex = 3;
      this.labelComSkip.Text = "Get ComSkip here:";
      this.labelComSkip.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // linkLabel
      // 
      this.linkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.linkLabel.Location = new System.Drawing.Point(8, 272);
      this.linkLabel.Name = "linkLabel";
      this.linkLabel.Size = new System.Drawing.Size(328, 16);
      this.linkLabel.TabIndex = 4;
      this.linkLabel.TabStop = true;
      this.linkLabel.Text = "http://www.kaashoek.com/comskip/";
      this.linkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // Configuration
      // 
      this.AcceptButton = this.buttonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(352, 345);
      this.Controls.Add(this.labelComSkip);
      this.Controls.Add(this.linkLabel);
      this.Controls.Add(this.groupBoxWhen);
      this.Controls.Add(this.groupBoxWhat);
      this.Controls.Add(this.groupBoxTest);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOK);
      this.MinimumSize = new System.Drawing.Size(360, 372);
      this.Name = "Configuration";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "ComSkip Launcher - Setup";
      this.groupBoxWhat.ResumeLayout(false);
      this.groupBoxWhat.PerformLayout();
      this.groupBoxTest.ResumeLayout(false);
      this.groupBoxTest.PerformLayout();
      this.groupBoxWhen.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.Button buttonTest;
    private System.Windows.Forms.TextBox textBoxTest;
    private System.Windows.Forms.Button buttonFindTestFile;
    private System.Windows.Forms.RadioButton radioButtonStart;
    private System.Windows.Forms.RadioButton radioButtonEnd;
    private System.Windows.Forms.ToolTip toolTips;
    private System.Windows.Forms.GroupBox groupBoxTest;
    private System.Windows.Forms.GroupBox groupBoxWhat;
    private System.Windows.Forms.Label labelParameters;
    private System.Windows.Forms.TextBox textBoxParameters;
    private System.Windows.Forms.Button buttonParameters;
    private System.Windows.Forms.Label labelProgram;
    private System.Windows.Forms.Button buttonProgram;
    private System.Windows.Forms.TextBox textBoxProgram;
    private System.Windows.Forms.GroupBox groupBoxWhen;
    private System.Windows.Forms.Label labelComSkip;
    private System.Windows.Forms.LinkLabel linkLabel;
  }
}