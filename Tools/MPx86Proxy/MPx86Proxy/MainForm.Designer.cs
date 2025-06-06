﻿namespace MPx86Proxy
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitToolStripIconMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteControlAPIEnabledToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.eventTableControl = new MPx86Proxy.Controls.EventTableControl();
            this.extensiveLoggingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuIcon.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuIcon;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "MP x86 Proxy";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);
            // 
            // contextMenuIcon
            // 
            this.contextMenuIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripIconMenuItem});
            this.contextMenuIcon.Name = "contextMenuIcon";
            this.contextMenuIcon.Size = new System.Drawing.Size(105, 28);
            // 
            // exitToolStripIconMenuItem
            // 
            this.exitToolStripIconMenuItem.Name = "exitToolStripIconMenuItem";
            this.exitToolStripIconMenuItem.Size = new System.Drawing.Size(104, 24);
            this.exitToolStripIconMenuItem.Text = "Exit";
            this.exitToolStripIconMenuItem.Click += new System.EventHandler(this.exitToolStripIconMenuItem_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(686, 27);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(45, 23);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(104, 24);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.remoteControlAPIEnabledToolStripMenuItem,
            this.extensiveLoggingToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(76, 23);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // remoteControlAPIEnabledToolStripMenuItem
            // 
            this.remoteControlAPIEnabledToolStripMenuItem.Name = "remoteControlAPIEnabledToolStripMenuItem";
            this.remoteControlAPIEnabledToolStripMenuItem.Size = new System.Drawing.Size(281, 24);
            this.remoteControlAPIEnabledToolStripMenuItem.Text = "Remote Control API Enabled";
            this.remoteControlAPIEnabledToolStripMenuItem.Click += new System.EventHandler(this.remoteControlAPIEnabledToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(55, 23);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // logToolStripMenuItem
            // 
            this.logToolStripMenuItem.Name = "logToolStripMenuItem";
            this.logToolStripMenuItem.Size = new System.Drawing.Size(104, 24);
            this.logToolStripMenuItem.Text = "Log";
            this.logToolStripMenuItem.Click += new System.EventHandler(this.logToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(53, 23);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(121, 24);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 367);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(686, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // eventTableControl
            // 
            this.eventTableControl.BackgroundColor = System.Drawing.SystemColors.Window;
            this.eventTableControl.ButtonCloseEnable = true;
            this.eventTableControl.ButtonMinMaxEnable = true;
            this.eventTableControl.ButtonSystemEnable = false;
            this.eventTableControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.eventTableControl.Filter = ((MPx86Proxy.Controls.EventTableMessageTypeEnum)((((MPx86Proxy.Controls.EventTableMessageTypeEnum.Info | MPx86Proxy.Controls.EventTableMessageTypeEnum.Warning) 
            | MPx86Proxy.Controls.EventTableMessageTypeEnum.Error) 
            | MPx86Proxy.Controls.EventTableMessageTypeEnum.System)));
            this.eventTableControl.ForegroundColor = System.Drawing.SystemColors.ControlText;
            this.eventTableControl.Location = new System.Drawing.Point(0, 27);
            this.eventTableControl.Name = "eventTableControl";
            this.eventTableControl.ShowLastMessage = true;
            this.eventTableControl.Size = new System.Drawing.Size(686, 340);
            this.eventTableControl.TabIndex = 3;
            this.eventTableControl.TableRowsLimit = 50;
            // 
            // extensiveLoggingToolStripMenuItem
            // 
            this.extensiveLoggingToolStripMenuItem.Name = "extensiveLoggingToolStripMenuItem";
            this.extensiveLoggingToolStripMenuItem.Size = new System.Drawing.Size(281, 24);
            this.extensiveLoggingToolStripMenuItem.Text = "Extensive logging";
            this.extensiveLoggingToolStripMenuItem.Click += new System.EventHandler(this.extensiveLoggingToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 389);
            this.Controls.Add(this.eventTableControl);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "MP x86 proxy Server ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.cbMainFormClosing);
            this.Shown += new System.EventHandler(this.cbMainFormShown);
            this.Resize += new System.EventHandler(this.cbMainFormResize);
            this.contextMenuIcon.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuIcon;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripIconMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private Controls.EventTableControl eventTableControl;
    private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem logToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem remoteControlAPIEnabledToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem extensiveLoggingToolStripMenuItem;
  }
}

