namespace MediaPortal.MPInstaller
{
  partial class ScriptEditorForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptEditorForm));
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.textBox_erro = new System.Windows.Forms.TextBox();
      this.richTextBox1 = new System.Windows.Forms.RichTextBox();
      this.textBox_code = new MediaPortal.MPInstaller.SyntaxRichTextBox();
      this.menuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // menuStrip1
      // 
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.testToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(702, 24);
      this.menuStrip1.TabIndex = 1;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
      this.fileToolStripMenuItem.Text = "File";
      // 
      // resetToolStripMenuItem
      // 
      this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
      this.resetToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
      this.resetToolStripMenuItem.Text = "Reset";
      this.resetToolStripMenuItem.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);
      // 
      // testToolStripMenuItem
      // 
      this.testToolStripMenuItem.Name = "testToolStripMenuItem";
      this.testToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
      this.testToolStripMenuItem.Text = "Test";
      this.testToolStripMenuItem.Click += new System.EventHandler(this.testToolStripMenuItem_Click);
      // 
      // textBox_erro
      // 
      this.textBox_erro.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBox_erro.Location = new System.Drawing.Point(0, 440);
      this.textBox_erro.Multiline = true;
      this.textBox_erro.Name = "textBox_erro";
      this.textBox_erro.ReadOnly = true;
      this.textBox_erro.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.textBox_erro.Size = new System.Drawing.Size(702, 92);
      this.textBox_erro.TabIndex = 2;
      this.textBox_erro.TextChanged += new System.EventHandler(this.textBox_erro_TextChanged);
      // 
      // richTextBox1
      // 
      this.richTextBox1.Location = new System.Drawing.Point(581, 456);
      this.richTextBox1.Name = "richTextBox1";
      this.richTextBox1.Size = new System.Drawing.Size(100, 96);
      this.richTextBox1.TabIndex = 4;
      this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
      this.richTextBox1.Visible = false;
      // 
      // textBox_code
      // 
      this.textBox_code.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBox_code.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.textBox_code.Location = new System.Drawing.Point(0, 27);
      this.textBox_code.Name = "textBox_code";
      this.textBox_code.Size = new System.Drawing.Size(702, 407);
      this.textBox_code.TabIndex = 3;
      this.textBox_code.Text = "";
      // 
      // ScriptEditorForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(702, 530);
      this.Controls.Add(this.richTextBox1);
      this.Controls.Add(this.textBox_code);
      this.Controls.Add(this.textBox_erro);
      this.Controls.Add(this.menuStrip1);
      this.MainMenuStrip = this.menuStrip1;
      this.Name = "ScriptEditorForm";
      this.Text = "Script Editor";
      this.Load += new System.EventHandler(this.ScriptEditorForm_Load);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScriptEditorForm_FormClosing);
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.TextBox textBox_erro;
    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
    public SyntaxRichTextBox textBox_code;
    private System.Windows.Forms.RichTextBox richTextBox1;
    private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;

  }
}