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
using System.ComponentModel;

namespace MPLanguageTool
{
  partial class frmMain
  {
    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.openMp1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.openDeployToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this.openTagThatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.openMovingPicturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.openTvSeriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.gv = new System.Windows.Forms.DataGridView();
      this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Translated = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.gv2 = new System.Windows.Forms.DataGridView();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
      this.openMpIIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.menuStrip1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.gv)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.gv2)).BeginInit();
      this.statusStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // menuStrip1
      // 
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(583, 24);
      this.menuStrip1.TabIndex = 0;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openMp1ToolStripMenuItem,
            this.openMpIIToolStripMenuItem,
            this.openDeployToolToolStripMenuItem,
            this.toolStripSeparator1,
            this.openTagThatToolStripMenuItem,
            this.openMovingPicturesToolStripMenuItem,
            this.openTvSeriesToolStripMenuItem,
            this.toolStripSeparator2,
            this.saveToolStripMenuItem,
            this.quitToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(116, 20);
      this.fileToolStripMenuItem.Text = "Load language file";
      // 
      // openMp1ToolStripMenuItem
      // 
      this.openMp1ToolStripMenuItem.Name = "openMp1ToolStripMenuItem";
      this.openMp1ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
      this.openMp1ToolStripMenuItem.Text = "MediaPortal 1";
      this.openMp1ToolStripMenuItem.Click += new System.EventHandler(this.openMpToolStripMenuItem_Click);
      // 
      // openDeployToolToolStripMenuItem
      // 
      this.openDeployToolToolStripMenuItem.Name = "openDeployToolToolStripMenuItem";
      this.openDeployToolToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
      this.openDeployToolToolStripMenuItem.Text = "DeployTool";
      this.openDeployToolToolStripMenuItem.Click += new System.EventHandler(this.openDeployToolToolStripMenuItem_Click);
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new System.Drawing.Size(157, 6);
      // 
      // openTagThatToolStripMenuItem
      // 
      this.openTagThatToolStripMenuItem.Name = "openTagThatToolStripMenuItem";
      this.openTagThatToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
      this.openTagThatToolStripMenuItem.Text = "MPTagThat";
      this.openTagThatToolStripMenuItem.Click += new System.EventHandler(this.openTagThatToolStripMenuItem_Click);
      // 
      // openMovingPicturesToolStripMenuItem
      // 
      this.openMovingPicturesToolStripMenuItem.Name = "openMovingPicturesToolStripMenuItem";
      this.openMovingPicturesToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
      this.openMovingPicturesToolStripMenuItem.Text = "Moving Pictures";
      this.openMovingPicturesToolStripMenuItem.Click += new System.EventHandler(this.openMovingPicturesToolStripMenuItem_Click);
      // 
      // openTvSeriesToolStripMenuItem
      // 
      this.openTvSeriesToolStripMenuItem.Name = "openTvSeriesToolStripMenuItem";
      this.openTvSeriesToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
      this.openTvSeriesToolStripMenuItem.Text = "Tv-Series";
      this.openTvSeriesToolStripMenuItem.Click += new System.EventHandler(this.openTvSeriesToolStripMenuItem_Click);
      // 
      // toolStripSeparator2
      // 
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new System.Drawing.Size(157, 6);
      // 
      // saveToolStripMenuItem
      // 
      this.saveToolStripMenuItem.Enabled = false;
      this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
      this.saveToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
      this.saveToolStripMenuItem.Text = "Save";
      this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
      // 
      // quitToolStripMenuItem
      // 
      this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
      this.quitToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
      this.quitToolStripMenuItem.Text = "Quit";
      this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
      // 
      // gv
      // 
      this.gv.AllowUserToAddRows = false;
      this.gv.AllowUserToDeleteRows = false;
      this.gv.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
      this.gv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.gv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.Translated});
      dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.gv.DefaultCellStyle = dataGridViewCellStyle4;
      this.gv.Dock = System.Windows.Forms.DockStyle.Fill;
      this.gv.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
      this.gv.Location = new System.Drawing.Point(0, 24);
      this.gv.MultiSelect = false;
      this.gv.Name = "gv";
      this.gv.ReadOnly = true;
      this.gv.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.gv.RowTemplate.Height = 30;
      this.gv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.gv.Size = new System.Drawing.Size(583, 345);
      this.gv.TabIndex = 1;
      this.gv.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.gv_CellMouseDoubleClick);
      // 
      // ID
      // 
      this.ID.HeaderText = "ID";
      this.ID.Name = "ID";
      this.ID.ReadOnly = true;
      this.ID.Width = 43;
      // 
      // Translated
      // 
      this.Translated.HeaderText = "Translated";
      this.Translated.Name = "Translated";
      this.Translated.ReadOnly = true;
      this.Translated.Width = 82;
      // 
      // gv2
      // 
      this.gv2.AllowUserToAddRows = false;
      this.gv2.AllowUserToDeleteRows = false;
      this.gv2.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
      this.gv2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.gv2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.gv2.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
      this.gv2.Location = new System.Drawing.Point(0, 24);
      this.gv2.MultiSelect = false;
      this.gv2.Name = "gv2";
      this.gv2.ReadOnly = true;
      this.gv2.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.gv2.RowTemplate.Height = 30;
      this.gv2.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.gv2.Size = new System.Drawing.Size(583, 323);
      this.gv2.TabIndex = 3;
      this.gv2.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.gv2_CellMouseDoubleClick);
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
      this.statusStrip1.Location = new System.Drawing.Point(0, 347);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(583, 22);
      this.statusStrip1.TabIndex = 2;
      // 
      // toolStripStatusLabel1
      // 
      this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
      // 
      // folderBrowserDialog1
      // 
      this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
      this.folderBrowserDialog1.ShowNewFolderButton = false;
      // 
      // openMpIIToolStripMenuItem
      // 
      this.openMpIIToolStripMenuItem.Enabled = false;
      this.openMpIIToolStripMenuItem.Name = "openMpIIToolStripMenuItem";
      this.openMpIIToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
      this.openMpIIToolStripMenuItem.Text = "MediaPortal II";
      this.openMpIIToolStripMenuItem.Click += new System.EventHandler(this.openMpIIToolStripMenuItem_Click);
      // 
      // frmMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(583, 369);
      this.Controls.Add(this.gv2);
      this.Controls.Add(this.statusStrip1);
      this.Controls.Add(this.gv);
      this.Controls.Add(this.menuStrip1);
      this.MainMenuStrip = this.menuStrip1;
      this.Name = "frmMain";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MPLanguageTool";
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.gv)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.gv2)).EndInit();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem openDeployToolToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
    private System.Windows.Forms.DataGridView gv;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    private System.Windows.Forms.ToolStripMenuItem openMp1ToolStripMenuItem;
    private System.Windows.Forms.DataGridViewTextBoxColumn ID;
    private System.Windows.Forms.DataGridViewTextBoxColumn Translated;
    private System.Windows.Forms.DataGridView gv2;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripMenuItem openTagThatToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    private System.Windows.Forms.ToolStripMenuItem openMovingPicturesToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem openTvSeriesToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem openMpIIToolStripMenuItem;
  }
}

