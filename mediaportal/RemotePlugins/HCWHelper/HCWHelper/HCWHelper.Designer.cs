#region Copyright (C) 2005-2006 Team MediaPortal - Author: mPod

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: mPod
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

namespace MediaPortal.InputDevices.HCWHelper
{
  partial class HCWHelper
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HCWHelper));
      this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
      this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.statusOfflineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.peerStatusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.notifyIconGreen = new System.Windows.Forms.NotifyIcon(this.components);
      this.notifyIconRed = new System.Windows.Forms.NotifyIcon(this.components);
      this.notifyIconYellow = new System.Windows.Forms.NotifyIcon(this.components);
      this.contextMenuStrip.SuspendLayout();
      this.SuspendLayout();
      // 
      // notifyIcon
      // 
      this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
      this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
      this.notifyIcon.Text = "MediaPortal HCW Control";
      // 
      // contextMenuStrip
      // 
      this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusOfflineToolStripMenuItem,
            this.peerStatusToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
      this.contextMenuStrip.Name = "contextMenuStrip";
      this.contextMenuStrip.ShowImageMargin = false;
      this.contextMenuStrip.Size = new System.Drawing.Size(96, 76);
      this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
      // 
      // statusOfflineToolStripMenuItem
      // 
      this.statusOfflineToolStripMenuItem.Name = "statusOfflineToolStripMenuItem";
      this.statusOfflineToolStripMenuItem.Size = new System.Drawing.Size(95, 22);
      this.statusOfflineToolStripMenuItem.Text = "Status:";
      // 
      // peerStatusToolStripMenuItem
      // 
      this.peerStatusToolStripMenuItem.Name = "peerStatusToolStripMenuItem";
      this.peerStatusToolStripMenuItem.Size = new System.Drawing.Size(95, 22);
      this.peerStatusToolStripMenuItem.Text = "Peer   :";
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new System.Drawing.Size(92, 6);
      // 
      // exitToolStripMenuItem
      // 
      this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
      this.exitToolStripMenuItem.Size = new System.Drawing.Size(95, 22);
      this.exitToolStripMenuItem.Text = "Exit";
      this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
      // 
      // notifyIconGreen
      // 
      this.notifyIconGreen.ContextMenuStrip = this.contextMenuStrip;
      this.notifyIconGreen.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconGreen.Icon")));
      this.notifyIconGreen.Text = "MediaPortal HCW Control";
      // 
      // notifyIconRed
      // 
      this.notifyIconRed.ContextMenuStrip = this.contextMenuStrip;
      this.notifyIconRed.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconRed.Icon")));
      this.notifyIconRed.Text = "MediaPortal HCW Control";
      // 
      // notifyIconYellow
      // 
      this.notifyIconYellow.ContextMenuStrip = this.contextMenuStrip;
      this.notifyIconYellow.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconYellow.Icon")));
      this.notifyIconYellow.Text = "MediaPortal HCW Control";
      // 
      // HCWHelper
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 266);
      this.Name = "HCWHelper";
      this.Opacity = 0;
      this.ShowInTaskbar = false;
      this.Text = "MediaPortal HCW Control";
      this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
      this.contextMenuStrip.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.NotifyIcon notifyIcon;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
    private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem statusOfflineToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripMenuItem peerStatusToolStripMenuItem;
    private System.Windows.Forms.NotifyIcon notifyIconGreen;
    private System.Windows.Forms.NotifyIcon notifyIconRed;
    private System.Windows.Forms.NotifyIcon notifyIconYellow;
  }
}

