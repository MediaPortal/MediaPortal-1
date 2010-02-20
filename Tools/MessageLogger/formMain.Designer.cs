namespace MessageLogger
{
  partial class MainForm
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
      this.listBoxLog = new System.Windows.Forms.ListBox();
      this.btnListen = new System.Windows.Forms.Button();
      this.btnClear = new System.Windows.Forms.Button();
      this.listBoxBlacklist = new System.Windows.Forms.ListBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.btnClose = new System.Windows.Forms.Button();
      this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.copyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.copyAllToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.contextMenu.SuspendLayout();
      this.SuspendLayout();
      // 
      // listBoxLog
      // 
      this.listBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listBoxLog.ContextMenuStrip = this.contextMenu;
      this.listBoxLog.FormattingEnabled = true;
      this.listBoxLog.Location = new System.Drawing.Point(12, 25);
      this.listBoxLog.Name = "listBoxLog";
      this.listBoxLog.Size = new System.Drawing.Size(533, 264);
      this.listBoxLog.TabIndex = 0;
      // 
      // btnListen
      // 
      this.btnListen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnListen.Location = new System.Drawing.Point(12, 295);
      this.btnListen.Name = "btnListen";
      this.btnListen.Size = new System.Drawing.Size(75, 23);
      this.btnListen.TabIndex = 1;
      this.btnListen.Text = "&Listen";
      this.btnListen.UseVisualStyleBackColor = true;
      this.btnListen.Click += new System.EventHandler(this.btnListen_Click);
      // 
      // btnClear
      // 
      this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnClear.Location = new System.Drawing.Point(93, 295);
      this.btnClear.Name = "btnClear";
      this.btnClear.Size = new System.Drawing.Size(75, 23);
      this.btnClear.TabIndex = 2;
      this.btnClear.Text = "Cl&ear";
      this.btnClear.UseVisualStyleBackColor = true;
      this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
      // 
      // listBoxBlacklist
      // 
      this.listBoxBlacklist.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listBoxBlacklist.FormattingEnabled = true;
      this.listBoxBlacklist.Items.AddRange(new object[] {
            "WM_CTLCOLORBTN",
            "WM_DRAWITEM",
            "WM_GETTEXT",
            "WM_GETTEXTLENGTH",
            "WM_CTLCOLORLISTBOX",
            "WM_PRINTCLIENT",
            "WM_ERASEBKGND",
            "WM_NCHITTEST",
            "WM_SETCURSOR",
            "WM_MOUSEMOVE",
            "WM_MOUSEACTIVATE",
            "WM_NCMOUSEMOVE",
            "WM_NCMOUSELEAVE",
            "WM_MOUSELEAVE",
            "WM_MOUSEHOVER",
            "WM_PAINT",
            "WM_SYNCPAINT",
            "WM_NCPAINT"});
      this.listBoxBlacklist.Location = new System.Drawing.Point(551, 25);
      this.listBoxBlacklist.Name = "listBoxBlacklist";
      this.listBoxBlacklist.Size = new System.Drawing.Size(301, 264);
      this.listBoxBlacklist.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(55, 13);
      this.label1.TabIndex = 4;
      this.label1.Text = "Messages";
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(548, 9);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(91, 13);
      this.label2.TabIndex = 5;
      this.label2.Text = "Message blacklist";
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnClose.Location = new System.Drawing.Point(777, 295);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(75, 23);
      this.btnClose.TabIndex = 6;
      this.btnClose.Text = "&Close";
      this.btnClose.UseVisualStyleBackColor = true;
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      // 
      // contextMenu
      // 
      this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToClipboardToolStripMenuItem,
            this.copyAllToClipboardToolStripMenuItem});
      this.contextMenu.Name = "contextMenu";
      this.contextMenu.Size = new System.Drawing.Size(189, 70);
      // 
      // copyToClipboardToolStripMenuItem
      // 
      this.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
      this.copyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
      this.copyToClipboardToolStripMenuItem.Text = "Copy line to clipboard";
      this.copyToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyToClipboardToolStripMenuItem_Click);
      // 
      // copyAllToClipboardToolStripMenuItem
      // 
      this.copyAllToClipboardToolStripMenuItem.Name = "copyAllToClipboardToolStripMenuItem";
      this.copyAllToClipboardToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
      this.copyAllToClipboardToolStripMenuItem.Text = "Copy all to clipboard";
      this.copyAllToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyAllToClipboardToolStripMenuItem_Click);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnClose;
      this.ClientSize = new System.Drawing.Size(864, 328);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.listBoxBlacklist);
      this.Controls.Add(this.btnClear);
      this.Controls.Add(this.btnListen);
      this.Controls.Add(this.listBoxLog);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "MainForm";
      this.Text = "Message logger";
      this.contextMenu.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ListBox listBoxLog;
    private System.Windows.Forms.Button btnListen;
    private System.Windows.Forms.Button btnClear;
    private System.Windows.Forms.ListBox listBoxBlacklist;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.ContextMenuStrip contextMenu;
    private System.Windows.Forms.ToolStripMenuItem copyToClipboardToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem copyAllToClipboardToolStripMenuItem;
  }
}

