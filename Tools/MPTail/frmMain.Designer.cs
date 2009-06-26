#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

namespace MPTail
{
  partial class frmMain
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
      this.PageCtrlCategory = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.MPTabCtrl = new System.Windows.Forms.TabControl();
      this.tabPage4 = new System.Windows.Forms.TabPage();
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.saveToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.TVETabCtrl = new System.Windows.Forms.TabControl();
      this.tabPageTvEngine = new System.Windows.Forms.TabPage();
      this.tabPage3 = new System.Windows.Forms.TabPage();
      this.CustomTabCtrl = new System.Windows.Forms.TabControl();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnRemoveLog = new System.Windows.Forms.Button();
      this.btnAddLogfile = new System.Windows.Forms.Button();
      this.cbClearOnCreate = new System.Windows.Forms.CheckBox();
      this.btnChooseFont = new System.Windows.Forms.Button();
      this.cbFollowTail = new System.Windows.Forms.CheckBox();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.richTextBoxMP = new MPTail.RingBufferedRichTextBox();
      this.richTextBoxTvEngine = new MPTail.RingBufferedRichTextBox();
      this.PageCtrlCategory.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.MPTabCtrl.SuspendLayout();
      this.tabPage4.SuspendLayout();
      this.contextMenuStrip1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.TVETabCtrl.SuspendLayout();
      this.tabPageTvEngine.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // PageCtrlCategory
      // 
      this.PageCtrlCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.PageCtrlCategory.Appearance = System.Windows.Forms.TabAppearance.Buttons;
      this.PageCtrlCategory.Controls.Add(this.tabPage1);
      this.PageCtrlCategory.Controls.Add(this.tabPage2);
      this.PageCtrlCategory.Controls.Add(this.tabPage3);
      this.PageCtrlCategory.Location = new System.Drawing.Point(2, 70);
      this.PageCtrlCategory.Name = "PageCtrlCategory";
      this.PageCtrlCategory.SelectedIndex = 0;
      this.PageCtrlCategory.Size = new System.Drawing.Size(772, 324);
      this.PageCtrlCategory.TabIndex = 0;
      this.PageCtrlCategory.Selected += new System.Windows.Forms.TabControlEventHandler(this.PageCtrlCategory_Selected);
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.MPTabCtrl);
      this.tabPage1.Location = new System.Drawing.Point(4, 27);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(764, 293);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "MediaPortal";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // MPTabCtrl
      // 
      this.MPTabCtrl.Controls.Add(this.tabPage4);
      this.MPTabCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MPTabCtrl.Location = new System.Drawing.Point(3, 3);
      this.MPTabCtrl.Name = "MPTabCtrl";
      this.MPTabCtrl.SelectedIndex = 0;
      this.MPTabCtrl.Size = new System.Drawing.Size(758, 287);
      this.MPTabCtrl.TabIndex = 0;
      this.MPTabCtrl.Selected += new System.Windows.Forms.TabControlEventHandler(this.MPTabCtrl_Selected);
      // 
      // tabPage4
      // 
      this.tabPage4.Controls.Add(this.richTextBoxMP);
      this.tabPage4.Location = new System.Drawing.Point(4, 24);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Size = new System.Drawing.Size(750, 259);
      this.tabPage4.TabIndex = 0;
      this.tabPage4.Text = "Combined view";
      this.tabPage4.UseVisualStyleBackColor = true;
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.searchToolStripMenuItem,
            this.toolStripMenuItem1,
            this.saveToFileToolStripMenuItem});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(125, 54);
      // 
      // searchToolStripMenuItem
      // 
      this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
      this.searchToolStripMenuItem.ShortcutKeyDisplayString = "CTRL+F";
      this.searchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
      this.searchToolStripMenuItem.ShowShortcutKeys = false;
      this.searchToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
      this.searchToolStripMenuItem.Text = "Find";
      this.searchToolStripMenuItem.Click += new System.EventHandler(this.searchToolStripMenuItem_Click);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(121, 6);
      // 
      // saveToFileToolStripMenuItem
      // 
      this.saveToFileToolStripMenuItem.Name = "saveToFileToolStripMenuItem";
      this.saveToFileToolStripMenuItem.ShortcutKeyDisplayString = "CTRL+S";
      this.saveToFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
      this.saveToFileToolStripMenuItem.ShowShortcutKeys = false;
      this.saveToFileToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
      this.saveToFileToolStripMenuItem.Text = "Save to file";
      this.saveToFileToolStripMenuItem.Click += new System.EventHandler(this.saveToFileToolStripMenuItem_Click);
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.TVETabCtrl);
      this.tabPage2.Location = new System.Drawing.Point(4, 27);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(764, 293);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "TvServer";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // TVETabCtrl
      // 
      this.TVETabCtrl.Controls.Add(this.tabPageTvEngine);
      this.TVETabCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TVETabCtrl.Location = new System.Drawing.Point(3, 3);
      this.TVETabCtrl.Name = "TVETabCtrl";
      this.TVETabCtrl.SelectedIndex = 0;
      this.TVETabCtrl.Size = new System.Drawing.Size(758, 289);
      this.TVETabCtrl.TabIndex = 1;
      this.TVETabCtrl.Selected += new System.Windows.Forms.TabControlEventHandler(this.MPTabCtrl_Selected);
      // 
      // tabPageTvEngine
      // 
      this.tabPageTvEngine.Controls.Add(this.richTextBoxTvEngine);
      this.tabPageTvEngine.Location = new System.Drawing.Point(4, 24);
      this.tabPageTvEngine.Name = "tabPageTvEngine";
      this.tabPageTvEngine.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageTvEngine.Size = new System.Drawing.Size(750, 261);
      this.tabPageTvEngine.TabIndex = 0;
      this.tabPageTvEngine.Text = "Combined view";
      this.tabPageTvEngine.UseVisualStyleBackColor = true;
      // 
      // tabPage3
      // 
      this.tabPage3.Controls.Add(this.CustomTabCtrl);
      this.tabPage3.Location = new System.Drawing.Point(4, 27);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage3.Size = new System.Drawing.Size(764, 293);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Custom";
      this.tabPage3.UseVisualStyleBackColor = true;
      // 
      // CustomTabCtrl
      // 
      this.CustomTabCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.CustomTabCtrl.Location = new System.Drawing.Point(3, 3);
      this.CustomTabCtrl.Name = "CustomTabCtrl";
      this.CustomTabCtrl.SelectedIndex = 0;
      this.CustomTabCtrl.Size = new System.Drawing.Size(758, 289);
      this.CustomTabCtrl.TabIndex = 0;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnRemoveLog);
      this.panel1.Controls.Add(this.btnAddLogfile);
      this.panel1.Controls.Add(this.cbClearOnCreate);
      this.panel1.Controls.Add(this.btnChooseFont);
      this.panel1.Controls.Add(this.cbFollowTail);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(773, 68);
      this.panel1.TabIndex = 1;
      // 
      // btnRemoveLog
      // 
      this.btnRemoveLog.AutoSize = true;
      this.btnRemoveLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnRemoveLog.Location = new System.Drawing.Point(380, 33);
      this.btnRemoveLog.Name = "btnRemoveLog";
      this.btnRemoveLog.Size = new System.Drawing.Size(117, 27);
      this.btnRemoveLog.TabIndex = 4;
      this.btnRemoveLog.Text = "Remove logfile";
      this.btnRemoveLog.UseVisualStyleBackColor = true;
      this.btnRemoveLog.Visible = false;
      this.btnRemoveLog.Click += new System.EventHandler(this.btnRemoveLog_Click);
      // 
      // btnAddLogfile
      // 
      this.btnAddLogfile.AutoSize = true;
      this.btnAddLogfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnAddLogfile.Location = new System.Drawing.Point(257, 33);
      this.btnAddLogfile.Name = "btnAddLogfile";
      this.btnAddLogfile.Size = new System.Drawing.Size(96, 27);
      this.btnAddLogfile.TabIndex = 1;
      this.btnAddLogfile.Text = "Add logfile";
      this.btnAddLogfile.UseVisualStyleBackColor = true;
      this.btnAddLogfile.Visible = false;
      this.btnAddLogfile.Click += new System.EventHandler(this.btnAddLogfile_Click);
      // 
      // cbClearOnCreate
      // 
      this.cbClearOnCreate.AutoSize = true;
      this.cbClearOnCreate.Checked = true;
      this.cbClearOnCreate.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbClearOnCreate.Location = new System.Drawing.Point(8, 36);
      this.cbClearOnCreate.Name = "cbClearOnCreate";
      this.cbClearOnCreate.Size = new System.Drawing.Size(243, 19);
      this.cbClearOnCreate.TabIndex = 3;
      this.cbClearOnCreate.Text = "Clear window if file is created";
      this.cbClearOnCreate.UseVisualStyleBackColor = true;
      this.cbClearOnCreate.CheckedChanged += new System.EventHandler(this.cbClearOnCreate_CheckedChanged);
      // 
      // btnChooseFont
      // 
      this.btnChooseFont.AutoSize = true;
      this.btnChooseFont.Location = new System.Drawing.Point(257, 4);
      this.btnChooseFont.Name = "btnChooseFont";
      this.btnChooseFont.Size = new System.Drawing.Size(94, 27);
      this.btnChooseFont.TabIndex = 2;
      this.btnChooseFont.Text = "Select Font";
      this.btnChooseFont.UseVisualStyleBackColor = true;
      this.btnChooseFont.Click += new System.EventHandler(this.button1_Click);
      // 
      // cbFollowTail
      // 
      this.cbFollowTail.AutoSize = true;
      this.cbFollowTail.Checked = true;
      this.cbFollowTail.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbFollowTail.Location = new System.Drawing.Point(10, 9);
      this.cbFollowTail.Name = "cbFollowTail";
      this.cbFollowTail.Size = new System.Drawing.Size(103, 19);
      this.cbFollowTail.TabIndex = 0;
      this.cbFollowTail.Text = "Follow Tail";
      this.cbFollowTail.UseVisualStyleBackColor = true;
      this.cbFollowTail.CheckedChanged += new System.EventHandler(this.cbFollowTail_CheckedChanged);
      // 
      // timer1
      // 
      this.timer1.Interval = 250;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // richTextBoxMP
      // 
      this.richTextBoxMP.ContextMenuStrip = this.contextMenuStrip1;
      this.richTextBoxMP.Dock = System.Windows.Forms.DockStyle.Fill;
      this.richTextBoxMP.Location = new System.Drawing.Point(0, 0);
      this.richTextBoxMP.Name = "richTextBoxMP";
      this.richTextBoxMP.RingBufferSizeInMB = ((long)(3));
      this.richTextBoxMP.Size = new System.Drawing.Size(750, 259);
      this.richTextBoxMP.TabIndex = 0;
      this.richTextBoxMP.Text = "";
      this.richTextBoxMP.WordWrap = false;
      // 
      // richTextBoxTvEngine
      // 
      this.richTextBoxTvEngine.ContextMenuStrip = this.contextMenuStrip1;
      this.richTextBoxTvEngine.Dock = System.Windows.Forms.DockStyle.Fill;
      this.richTextBoxTvEngine.Location = new System.Drawing.Point(3, 3);
      this.richTextBoxTvEngine.Name = "richTextBoxTvEngine";
      this.richTextBoxTvEngine.RingBufferSizeInMB = ((long)(3));
      this.richTextBoxTvEngine.Size = new System.Drawing.Size(744, 255);
      this.richTextBoxTvEngine.TabIndex = 0;
      this.richTextBoxTvEngine.Text = "";
      this.richTextBoxTvEngine.WordWrap = false;
      // 
      // frmMain
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(773, 390);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.PageCtrlCategory);
      this.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Name = "frmMain";
      this.Text = "MediaPortal Tail";
      this.Shown += new System.EventHandler(this.Form1_Shown);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmMain_DragDrop);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmMain_DragEnter);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
      this.PageCtrlCategory.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.MPTabCtrl.ResumeLayout(false);
      this.tabPage4.ResumeLayout(false);
      this.contextMenuStrip1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.TVETabCtrl.ResumeLayout(false);
      this.tabPageTvEngine.ResumeLayout(false);
      this.tabPage3.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl PageCtrlCategory;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabControl MPTabCtrl;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.TabControl TVETabCtrl;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.CheckBox cbFollowTail;
    private System.Windows.Forms.Button btnChooseFont;
    private System.Windows.Forms.CheckBox cbClearOnCreate;
    private System.Windows.Forms.Timer timer1;
    private System.Windows.Forms.TabPage tabPageTvEngine;
    private RingBufferedRichTextBox richTextBoxTvEngine;
    private System.Windows.Forms.TabPage tabPage3;
    private System.Windows.Forms.TabControl CustomTabCtrl;
    private System.Windows.Forms.TabPage tabPage4;
    private RingBufferedRichTextBox richTextBoxMP;
    private System.Windows.Forms.Button btnAddLogfile;
    private System.Windows.Forms.Button btnRemoveLog;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private System.Windows.Forms.ToolStripMenuItem saveToFileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
  }
}

