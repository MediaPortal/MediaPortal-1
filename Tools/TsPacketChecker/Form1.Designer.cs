namespace TsPacketChecker
{
  partial class Form1
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
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.TrSections = new System.Windows.Forms.TreeView();
      this.splitContainer3 = new System.Windows.Forms.SplitContainer();
      this.PrBar = new System.Windows.Forms.ProgressBar();
      this.btnStop = new System.Windows.Forms.Button();
      this.edPcrDiff = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.btnAnalyze = new System.Windows.Forms.Button();
      this.edLog = new System.Windows.Forms.TextBox();
      this.openDlg = new System.Windows.Forms.OpenFileDialog();
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.opentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.importFromXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.exportToXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
      this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.saveDlg = new System.Windows.Forms.SaveFileDialog();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.splitContainer3.Panel1.SuspendLayout();
      this.splitContainer3.Panel2.SuspendLayout();
      this.splitContainer3.SuspendLayout();
      this.menuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer2
      // 
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.Location = new System.Drawing.Point(0, 24);
      this.splitContainer2.Name = "splitContainer2";
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.TrSections);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
      this.splitContainer2.Size = new System.Drawing.Size(1038, 654);
      this.splitContainer2.SplitterDistance = 167;
      this.splitContainer2.TabIndex = 0;
      // 
      // TrSections
      // 
      this.TrSections.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TrSections.Location = new System.Drawing.Point(0, 0);
      this.TrSections.Name = "TrSections";
      this.TrSections.Size = new System.Drawing.Size(167, 654);
      this.TrSections.TabIndex = 0;
      // 
      // splitContainer3
      // 
      this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer3.Location = new System.Drawing.Point(0, 0);
      this.splitContainer3.Name = "splitContainer3";
      this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer3.Panel1
      // 
      this.splitContainer3.Panel1.Controls.Add(this.PrBar);
      this.splitContainer3.Panel1.Controls.Add(this.btnStop);
      this.splitContainer3.Panel1.Controls.Add(this.edPcrDiff);
      this.splitContainer3.Panel1.Controls.Add(this.label2);
      this.splitContainer3.Panel1.Controls.Add(this.btnAnalyze);
      // 
      // splitContainer3.Panel2
      // 
      this.splitContainer3.Panel2.Controls.Add(this.edLog);
      this.splitContainer3.Size = new System.Drawing.Size(867, 654);
      this.splitContainer3.SplitterDistance = 77;
      this.splitContainer3.TabIndex = 0;
      // 
      // PrBar
      // 
      this.PrBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.PrBar.Location = new System.Drawing.Point(109, 43);
      this.PrBar.Name = "PrBar";
      this.PrBar.Size = new System.Drawing.Size(746, 23);
      this.PrBar.Step = 1;
      this.PrBar.TabIndex = 4;
      // 
      // btnStop
      // 
      this.btnStop.Location = new System.Drawing.Point(13, 43);
      this.btnStop.Name = "btnStop";
      this.btnStop.Size = new System.Drawing.Size(75, 23);
      this.btnStop.TabIndex = 3;
      this.btnStop.Text = "Stop";
      this.btnStop.UseVisualStyleBackColor = true;
      this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
      // 
      // edPcrDiff
      // 
      this.edPcrDiff.Location = new System.Drawing.Point(299, 13);
      this.edPcrDiff.Name = "edPcrDiff";
      this.edPcrDiff.Size = new System.Drawing.Size(100, 20);
      this.edPcrDiff.TabIndex = 2;
      this.edPcrDiff.Text = "10";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(106, 13);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(187, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "Max Pcr diff (as clock value in double)";
      // 
      // btnAnalyze
      // 
      this.btnAnalyze.Location = new System.Drawing.Point(13, 13);
      this.btnAnalyze.Name = "btnAnalyze";
      this.btnAnalyze.Size = new System.Drawing.Size(75, 23);
      this.btnAnalyze.TabIndex = 0;
      this.btnAnalyze.Text = "Analyze";
      this.btnAnalyze.UseVisualStyleBackColor = true;
      this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
      // 
      // edLog
      // 
      this.edLog.Dock = System.Windows.Forms.DockStyle.Fill;
      this.edLog.Location = new System.Drawing.Point(0, 0);
      this.edLog.Multiline = true;
      this.edLog.Name = "edLog";
      this.edLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.edLog.Size = new System.Drawing.Size(867, 573);
      this.edLog.TabIndex = 0;
      // 
      // openDlg
      // 
      this.openDlg.FileName = "openFileDialog1";
      this.openDlg.Filter = "Ts Files (*.ts)|*.ts";
      this.openDlg.RestoreDirectory = true;
      // 
      // menuStrip1
      // 
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(1038, 24);
      this.menuStrip1.TabIndex = 1;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.opentsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.importFromXMLToolStripMenuItem,
            this.exportToXMLToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exitToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
      this.fileToolStripMenuItem.Text = "File";
      // 
      // opentsToolStripMenuItem
      // 
      this.opentsToolStripMenuItem.Name = "opentsToolStripMenuItem";
      this.opentsToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
      this.opentsToolStripMenuItem.Text = "Open .ts  file";
      this.opentsToolStripMenuItem.Click += new System.EventHandler(this.opentsToolStripMenuItem_Click);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(161, 6);
      // 
      // importFromXMLToolStripMenuItem
      // 
      this.importFromXMLToolStripMenuItem.Name = "importFromXMLToolStripMenuItem";
      this.importFromXMLToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
      this.importFromXMLToolStripMenuItem.Text = "Import from XML";
      this.importFromXMLToolStripMenuItem.Click += new System.EventHandler(this.importFromXMLToolStripMenuItem_Click);
      // 
      // exportToXMLToolStripMenuItem
      // 
      this.exportToXMLToolStripMenuItem.Name = "exportToXMLToolStripMenuItem";
      this.exportToXMLToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
      this.exportToXMLToolStripMenuItem.Text = "Export to XML";
      this.exportToXMLToolStripMenuItem.Click += new System.EventHandler(this.exportToXMLToolStripMenuItem_Click);
      // 
      // toolStripMenuItem2
      // 
      this.toolStripMenuItem2.Name = "toolStripMenuItem2";
      this.toolStripMenuItem2.Size = new System.Drawing.Size(161, 6);
      // 
      // exitToolStripMenuItem
      // 
      this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
      this.exitToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
      this.exitToolStripMenuItem.Text = "Exit";
      this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
      // 
      // saveDlg
      // 
      this.saveDlg.DefaultExt = "xml";
      this.saveDlg.Filter = "XML Files (*.xml)|*.xml";
      this.saveDlg.RestoreDirectory = true;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1038, 678);
      this.Controls.Add(this.splitContainer2);
      this.Controls.Add(this.menuStrip1);
      this.Name = "Form1";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "TsPacketChecker (by gemx)";
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      this.splitContainer2.ResumeLayout(false);
      this.splitContainer3.Panel1.ResumeLayout(false);
      this.splitContainer3.Panel1.PerformLayout();
      this.splitContainer3.Panel2.ResumeLayout(false);
      this.splitContainer3.Panel2.PerformLayout();
      this.splitContainer3.ResumeLayout(false);
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.OpenFileDialog openDlg;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.SplitContainer splitContainer3;
    private System.Windows.Forms.TextBox edPcrDiff;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btnAnalyze;
    private System.Windows.Forms.TextBox edLog;
    private System.Windows.Forms.Button btnStop;
    private System.Windows.Forms.ProgressBar PrBar;
    private System.Windows.Forms.TreeView TrSections;
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem opentsToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem importFromXMLToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem exportToXMLToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
    private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    private System.Windows.Forms.SaveFileDialog saveDlg;

  }
}

