namespace SetupTv.Sections
{

    partial class ComSkipSetup
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComSkipSetup));
      this.groupBoxTest = new System.Windows.Forms.GroupBox();
      this.textBoxTest = new System.Windows.Forms.TextBox();
      this.buttonTest = new System.Windows.Forms.Button();
      this.buttonFindTestFile = new System.Windows.Forms.Button();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.groupBoxWhen = new System.Windows.Forms.GroupBox();
      this.radioButtonStart = new System.Windows.Forms.RadioButton();
      this.radioButtonEnd = new System.Windows.Forms.RadioButton();
      this.groupBoxWhat = new System.Windows.Forms.GroupBox();
      this.labelPriority = new System.Windows.Forms.Label();
      this.comboBoxPriority = new System.Windows.Forms.ComboBox();
      this.labelParameters = new System.Windows.Forms.Label();
      this.textBoxParameters = new System.Windows.Forms.TextBox();
      this.buttonParameters = new System.Windows.Forms.Button();
      this.labelProgram = new System.Windows.Forms.Label();
      this.buttonProgram = new System.Windows.Forms.Button();
      this.textBoxProgram = new System.Windows.Forms.TextBox();
      this.linkLabel = new System.Windows.Forms.LinkLabel();
      this.labelComSkip = new System.Windows.Forms.Label();
      this.labelAboutComskip = new System.Windows.Forms.Label();
      this.groupBoxTest.SuspendLayout();
      this.groupBoxWhen.SuspendLayout();
      this.groupBoxWhat.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxTest
      // 
      this.groupBoxTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxTest.Controls.Add(this.textBoxTest);
      this.groupBoxTest.Controls.Add(this.buttonTest);
      this.groupBoxTest.Controls.Add(this.buttonFindTestFile);
      this.groupBoxTest.Location = new System.Drawing.Point(8, 179);
      this.groupBoxTest.Name = "groupBoxTest";
      this.groupBoxTest.Size = new System.Drawing.Size(344, 60);
      this.groupBoxTest.TabIndex = 2;
      this.groupBoxTest.TabStop = false;
      this.groupBoxTest.Text = "Test";
      // 
      // textBoxTest
      // 
      this.textBoxTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxTest.Location = new System.Drawing.Point(8, 24);
      this.textBoxTest.Name = "textBoxTest";
      this.textBoxTest.Size = new System.Drawing.Size(240, 20);
      this.textBoxTest.TabIndex = 0;
      // 
      // buttonTest
      // 
      this.buttonTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonTest.Location = new System.Drawing.Point(288, 24);
      this.buttonTest.Name = "buttonTest";
      this.buttonTest.Size = new System.Drawing.Size(48, 20);
      this.buttonTest.TabIndex = 2;
      this.buttonTest.Text = "Test";
      this.buttonTest.UseVisualStyleBackColor = true;
      this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
      // 
      // buttonFindTestFile
      // 
      this.buttonFindTestFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonFindTestFile.Location = new System.Drawing.Point(256, 24);
      this.buttonFindTestFile.Name = "buttonFindTestFile";
      this.buttonFindTestFile.Size = new System.Drawing.Size(24, 20);
      this.buttonFindTestFile.TabIndex = 1;
      this.buttonFindTestFile.Text = "...";
      this.buttonFindTestFile.UseVisualStyleBackColor = true;
      this.buttonFindTestFile.Click += new System.EventHandler(this.buttonFindTestFile_Click);
      // 
      // openFileDialog
      // 
      this.openFileDialog.Filter = "All files|*.*";
      // 
      // groupBoxWhen
      // 
      this.groupBoxWhen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxWhen.Controls.Add(this.radioButtonStart);
      this.groupBoxWhen.Controls.Add(this.radioButtonEnd);
      this.groupBoxWhen.Location = new System.Drawing.Point(8, 8);
      this.groupBoxWhen.Name = "groupBoxWhen";
      this.groupBoxWhen.Size = new System.Drawing.Size(344, 48);
      this.groupBoxWhen.TabIndex = 0;
      this.groupBoxWhen.TabStop = false;
      this.groupBoxWhen.Text = "When to launch";
      // 
      // radioButtonStart
      // 
      this.radioButtonStart.Checked = true;
      this.radioButtonStart.Location = new System.Drawing.Point(8, 16);
      this.radioButtonStart.Name = "radioButtonStart";
      this.radioButtonStart.Size = new System.Drawing.Size(120, 24);
      this.radioButtonStart.TabIndex = 0;
      this.radioButtonStart.TabStop = true;
      this.radioButtonStart.Text = "Recording Start";
      this.radioButtonStart.UseVisualStyleBackColor = true;
      // 
      // radioButtonEnd
      // 
      this.radioButtonEnd.Location = new System.Drawing.Point(152, 16);
      this.radioButtonEnd.Name = "radioButtonEnd";
      this.radioButtonEnd.Size = new System.Drawing.Size(120, 24);
      this.radioButtonEnd.TabIndex = 1;
      this.radioButtonEnd.Text = "Recording End";
      this.radioButtonEnd.UseVisualStyleBackColor = true;
      // 
      // groupBoxWhat
      // 
      this.groupBoxWhat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxWhat.Controls.Add(this.labelPriority);
      this.groupBoxWhat.Controls.Add(this.comboBoxPriority);
      this.groupBoxWhat.Controls.Add(this.labelParameters);
      this.groupBoxWhat.Controls.Add(this.textBoxParameters);
      this.groupBoxWhat.Controls.Add(this.buttonParameters);
      this.groupBoxWhat.Controls.Add(this.labelProgram);
      this.groupBoxWhat.Controls.Add(this.buttonProgram);
      this.groupBoxWhat.Controls.Add(this.textBoxProgram);
      this.groupBoxWhat.Location = new System.Drawing.Point(8, 64);
      this.groupBoxWhat.Name = "groupBoxWhat";
      this.groupBoxWhat.Size = new System.Drawing.Size(344, 109);
      this.groupBoxWhat.TabIndex = 1;
      this.groupBoxWhat.TabStop = false;
      this.groupBoxWhat.Text = "What to launch";
      // 
      // labelPriority
      // 
      this.labelPriority.Location = new System.Drawing.Point(8, 78);
      this.labelPriority.Name = "labelPriority";
      this.labelPriority.Size = new System.Drawing.Size(63, 13);
      this.labelPriority.TabIndex = 7;
      this.labelPriority.Text = "Priority:";
      this.labelPriority.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // comboBoxPriority
      // 
      this.comboBoxPriority.FormattingEnabled = true;
      this.comboBoxPriority.Location = new System.Drawing.Point(77, 75);
      this.comboBoxPriority.Name = "comboBoxPriority";
      this.comboBoxPriority.Size = new System.Drawing.Size(171, 21);
      this.comboBoxPriority.TabIndex = 6;
      // 
      // labelParameters
      // 
      this.labelParameters.Location = new System.Drawing.Point(8, 52);
      this.labelParameters.Name = "labelParameters";
      this.labelParameters.Size = new System.Drawing.Size(63, 13);
      this.labelParameters.TabIndex = 3;
      this.labelParameters.Text = "Parameters:";
      this.labelParameters.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // textBoxParameters
      // 
      this.textBoxParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxParameters.Location = new System.Drawing.Point(77, 49);
      this.textBoxParameters.Name = "textBoxParameters";
      this.textBoxParameters.Size = new System.Drawing.Size(227, 20);
      this.textBoxParameters.TabIndex = 4;
      // 
      // buttonParameters
      // 
      this.buttonParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonParameters.Location = new System.Drawing.Point(310, 49);
      this.buttonParameters.Name = "buttonParameters";
      this.buttonParameters.Size = new System.Drawing.Size(24, 20);
      this.buttonParameters.TabIndex = 5;
      this.buttonParameters.Text = "?";
      this.buttonParameters.UseVisualStyleBackColor = true;
      this.buttonParameters.Click += new System.EventHandler(this.buttonParamQuestion_Click);
      // 
      // labelProgram
      // 
      this.labelProgram.Location = new System.Drawing.Point(8, 27);
      this.labelProgram.Name = "labelProgram";
      this.labelProgram.Size = new System.Drawing.Size(63, 13);
      this.labelProgram.TabIndex = 0;
      this.labelProgram.Text = "Program:";
      this.labelProgram.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // buttonProgram
      // 
      this.buttonProgram.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonProgram.Location = new System.Drawing.Point(310, 23);
      this.buttonProgram.Name = "buttonProgram";
      this.buttonProgram.Size = new System.Drawing.Size(24, 20);
      this.buttonProgram.TabIndex = 2;
      this.buttonProgram.Text = "...";
      this.buttonProgram.UseVisualStyleBackColor = true;
      this.buttonProgram.Click += new System.EventHandler(this.buttonProgram_Click);
      // 
      // textBoxProgram
      // 
      this.textBoxProgram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxProgram.Location = new System.Drawing.Point(77, 23);
      this.textBoxProgram.Name = "textBoxProgram";
      this.textBoxProgram.Size = new System.Drawing.Size(227, 20);
      this.textBoxProgram.TabIndex = 1;
      // 
      // linkLabel
      // 
      this.linkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.linkLabel.Location = new System.Drawing.Point(8, 311);
      this.linkLabel.Name = "linkLabel";
      this.linkLabel.Size = new System.Drawing.Size(344, 16);
      this.linkLabel.TabIndex = 4;
      this.linkLabel.TabStop = true;
      this.linkLabel.Text = "http://www.kaashoek.com/comskip/";
      this.linkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.linkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
      // 
      // labelComSkip
      // 
      this.labelComSkip.Location = new System.Drawing.Point(8, 271);
      this.labelComSkip.Name = "labelComSkip";
      this.labelComSkip.Size = new System.Drawing.Size(344, 16);
      this.labelComSkip.TabIndex = 3;
      this.labelComSkip.Text = "Get ComSkip here:";
      this.labelComSkip.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelAboutComskip
      // 
      this.labelAboutComskip.Location = new System.Drawing.Point(8, 247);
      this.labelAboutComskip.Name = "labelAboutComskip";
      this.labelAboutComskip.Size = new System.Drawing.Size(344, 64);
      this.labelAboutComskip.TabIndex = 5;
      this.labelAboutComskip.Text = resources.GetString("labelAboutComskip.Text");
      this.labelAboutComskip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // ComSkipSetup
      // 
      this.Controls.Add(this.labelAboutComskip);
      this.Controls.Add(this.labelComSkip);
      this.Controls.Add(this.linkLabel);
      this.Controls.Add(this.groupBoxWhen);
      this.Controls.Add(this.groupBoxWhat);
      this.Controls.Add(this.groupBoxTest);
      this.Name = "ComSkipSetup";
      this.Size = new System.Drawing.Size(362, 345);
      this.groupBoxTest.ResumeLayout(false);
      this.groupBoxTest.PerformLayout();
      this.groupBoxWhen.ResumeLayout(false);
      this.groupBoxWhat.ResumeLayout(false);
      this.groupBoxWhat.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

      private System.Windows.Forms.GroupBox groupBoxTest;
      private System.Windows.Forms.TextBox textBoxTest;
      private System.Windows.Forms.Button buttonTest;
      private System.Windows.Forms.Button buttonFindTestFile;
      private System.Windows.Forms.OpenFileDialog openFileDialog;
      private System.Windows.Forms.GroupBox groupBoxWhen;
      private System.Windows.Forms.RadioButton radioButtonStart;
      private System.Windows.Forms.RadioButton radioButtonEnd;
      private System.Windows.Forms.GroupBox groupBoxWhat;
      private System.Windows.Forms.Label labelParameters;
      private System.Windows.Forms.TextBox textBoxParameters;
      private System.Windows.Forms.Button buttonParameters;
      private System.Windows.Forms.Label labelProgram;
      private System.Windows.Forms.Button buttonProgram;
      private System.Windows.Forms.TextBox textBoxProgram;
      private System.Windows.Forms.LinkLabel linkLabel;
      private System.Windows.Forms.Label labelComSkip;
      private System.Windows.Forms.Label labelAboutComskip;
      private System.Windows.Forms.Label labelPriority;
      private System.Windows.Forms.ComboBox comboBoxPriority;



      }
}
