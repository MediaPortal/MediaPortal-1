
namespace LogoAutoRenamer
{
  partial class MainForm
  {
    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.buttonStart = new System.Windows.Forms.Button();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.textBoxSrc = new System.Windows.Forms.TextBox();
      this.textBoxDst = new System.Windows.Forms.TextBox();
      this.textboxXml = new System.Windows.Forms.TextBox();
      this.buttonSrc = new System.Windows.Forms.Button();
      this.buttonDst = new System.Windows.Forms.Button();
      this.buttonXml = new System.Windows.Forms.Button();
      this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
      this.progressBarStatus = new System.Windows.Forms.ProgressBar();
      this.label3 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // buttonStart
      // 
      this.buttonStart.Location = new System.Drawing.Point(189, 146);
      this.buttonStart.Name = "buttonStart";
      this.buttonStart.Size = new System.Drawing.Size(110, 27);
      this.buttonStart.TabIndex = 0;
      this.buttonStart.Text = "Start";
      this.buttonStart.UseVisualStyleBackColor = true;
      this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.FileName = "export.xml";
      this.openFileDialog1.Filter = "Channel list|*.xml";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(21, 23);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(87, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Source directory:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(21, 63);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(106, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Destination directory:";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(21, 103);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(119, 13);
      this.label4.TabIndex = 4;
      this.label4.Text = "TV Server channels list:";
      // 
      // textBoxSrc
      // 
      this.textBoxSrc.Location = new System.Drawing.Point(154, 20);
      this.textBoxSrc.Name = "textBoxSrc";
      this.textBoxSrc.Size = new System.Drawing.Size(243, 20);
      this.textBoxSrc.TabIndex = 5;
      // 
      // textBoxDst
      // 
      this.textBoxDst.Location = new System.Drawing.Point(154, 60);
      this.textBoxDst.Name = "textBoxDst";
      this.textBoxDst.Size = new System.Drawing.Size(243, 20);
      this.textBoxDst.TabIndex = 6;
      // 
      // textboxXml
      // 
      this.textboxXml.Location = new System.Drawing.Point(154, 100);
      this.textboxXml.Name = "textboxXml";
      this.textboxXml.Size = new System.Drawing.Size(243, 20);
      this.textboxXml.TabIndex = 8;
      // 
      // buttonSrc
      // 
      this.buttonSrc.Location = new System.Drawing.Point(397, 20);
      this.buttonSrc.Name = "buttonSrc";
      this.buttonSrc.Size = new System.Drawing.Size(21, 19);
      this.buttonSrc.TabIndex = 9;
      this.buttonSrc.Text = "...";
      this.buttonSrc.UseVisualStyleBackColor = true;
      this.buttonSrc.Click += new System.EventHandler(this.buttonSrc_Click);
      // 
      // buttonDst
      // 
      this.buttonDst.Location = new System.Drawing.Point(397, 60);
      this.buttonDst.Name = "buttonDst";
      this.buttonDst.Size = new System.Drawing.Size(21, 19);
      this.buttonDst.TabIndex = 10;
      this.buttonDst.Text = "...";
      this.buttonDst.UseVisualStyleBackColor = true;
      this.buttonDst.Click += new System.EventHandler(this.buttonDst_Click);
      // 
      // buttonXml
      // 
      this.buttonXml.Location = new System.Drawing.Point(397, 100);
      this.buttonXml.Name = "buttonXml";
      this.buttonXml.Size = new System.Drawing.Size(21, 19);
      this.buttonXml.TabIndex = 12;
      this.buttonXml.Text = "...";
      this.buttonXml.UseVisualStyleBackColor = true;
      this.buttonXml.Click += new System.EventHandler(this.buttonXml_Click);
      // 
      // folderBrowserDialog1
      // 
      this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
      // 
      // progressBarStatus
      // 
      this.progressBarStatus.Location = new System.Drawing.Point(-1, 190);
      this.progressBarStatus.Name = "progressBarStatus";
      this.progressBarStatus.Size = new System.Drawing.Size(481, 23);
      this.progressBarStatus.Step = 1;
      this.progressBarStatus.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
      this.progressBarStatus.TabIndex = 13;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(21, 36);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(92, 13);
      this.label3.TabIndex = 14;
      this.label3.Text = "( png, jpg and gif )";
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(480, 213);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.progressBarStatus);
      this.Controls.Add(this.buttonXml);
      this.Controls.Add(this.buttonDst);
      this.Controls.Add(this.buttonSrc);
      this.Controls.Add(this.textboxXml);
      this.Controls.Add(this.textBoxDst);
      this.Controls.Add(this.textBoxSrc);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.buttonStart);
      this.Name = "MainForm";
      this.Text = "Logo auto renamer";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonStart;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox textBoxSrc;
    private System.Windows.Forms.TextBox textBoxDst;
    private System.Windows.Forms.TextBox textboxXml;
    private System.Windows.Forms.Button buttonSrc;
    private System.Windows.Forms.Button buttonDst;
    private System.Windows.Forms.Button buttonXml;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    private System.Windows.Forms.ProgressBar progressBarStatus;
    private System.Windows.Forms.Label label3;

  }
}

