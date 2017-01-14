using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV
{
  partial class SettingsForm
  {
    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
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
    protected void InitializeComponent()
    {
      this.sectionTree = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTreeView();
      this.closeButton = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.headerLabel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGradientLabel();
      this.holderPanel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPPanel();
      this.beveledLine1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPBeveledLine();
      this.linkLabelDonate = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLinkLabel();
      this.toolStrip1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPToolStrip();
      this.helpToolStripSplitButton = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPToolStripButton();
      this.configToolStripSplitButton = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPToolStripButton();
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // sectionTree
      // 
      this.sectionTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.sectionTree.FullRowSelect = true;
      this.sectionTree.HideSelection = false;
      this.sectionTree.HotTracking = true;
      this.sectionTree.Indent = 19;
      this.sectionTree.ItemHeight = 16;
      this.sectionTree.Location = new System.Drawing.Point(16, 28);
      this.sectionTree.Name = "sectionTree";
      this.sectionTree.Size = new System.Drawing.Size(184, 464);
      this.sectionTree.TabIndex = 1;
      this.sectionTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.sectionTree_BeforeSelect);
      this.sectionTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.sectionTree_AfterSelect);
      // 
      // closeButton
      // 
      this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.closeButton.Location = new System.Drawing.Point(626, 515);
      this.closeButton.Name = "closeButton";
      this.closeButton.Size = new System.Drawing.Size(75, 23);
      this.closeButton.TabIndex = 6;
      this.closeButton.Text = "&Close";
      this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Caption = "";
      this.headerLabel.FirstColor = System.Drawing.SystemColors.InactiveCaption;
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.headerLabel.LastColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.Location = new System.Drawing.Point(216, 28);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.PaddingLeft = 2;
      this.headerLabel.Size = new System.Drawing.Size(485, 24);
      this.headerLabel.TabIndex = 2;
      this.headerLabel.TabStop = false;
      this.headerLabel.TextColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.TextFont = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      // 
      // holderPanel
      // 
      this.holderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.holderPanel.AutoScroll = true;
      this.holderPanel.BackColor = System.Drawing.SystemColors.Control;
      this.holderPanel.Location = new System.Drawing.Point(216, 58);
      this.holderPanel.Name = "holderPanel";
      this.holderPanel.Size = new System.Drawing.Size(485, 434);
      this.holderPanel.TabIndex = 3;
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(8, 505);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(701, 2);
      this.beveledLine1.TabIndex = 4;
      this.beveledLine1.TabStop = false;
      // 
      // linkLabelDonate
      // 
      this.linkLabelDonate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabelDonate.AutoSize = true;
      this.linkLabelDonate.Location = new System.Drawing.Point(13, 520);
      this.linkLabelDonate.Name = "linkLabelDonate";
      this.linkLabelDonate.Size = new System.Drawing.Size(113, 13);
      this.linkLabelDonate.TabIndex = 5;
      this.linkLabelDonate.TabStop = true;
      this.linkLabelDonate.Text = "Donate to MediaPortal";
      this.linkLabelDonate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDonate_LinkClicked);
      // 
      // toolStrip1
      // 
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripSplitButton,
            this.configToolStripSplitButton});
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(717, 25);
      this.toolStrip1.TabIndex = 0;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // helpToolStripSplitButton
      // 
      this.helpToolStripSplitButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.helpToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.helpToolStripSplitButton.Name = "helpToolStripSplitButton";
      this.helpToolStripSplitButton.Size = new System.Drawing.Size(32, 22);
      this.helpToolStripSplitButton.Text = "Help";
      this.helpToolStripSplitButton.ToolTipText = "Opens the online wiki page for the active configuration section.";
      this.helpToolStripSplitButton.Click += new System.EventHandler(this.helpToolStripSplitButton_ButtonClick);
      // 
      // configToolStripSplitButton
      // 
      this.configToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.configToolStripSplitButton.Name = "configToolStripSplitButton";
      this.configToolStripSplitButton.Size = new System.Drawing.Size(100, 22);
      this.configToolStripSplitButton.Text = "Open log directory";
      this.configToolStripSplitButton.Click += new System.EventHandler(this.configToolStripSplitButton_ButtonClick);
      // 
      // SettingsForm
      // 
      this.AcceptButton = this.closeButton;
      this.AutoScroll = true;
      this.ClientSize = new System.Drawing.Size(717, 546);
      this.Controls.Add(this.toolStrip1);
      this.Controls.Add(this.linkLabelDonate);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.holderPanel);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.closeButton);
      this.Controls.Add(this.sectionTree);
      this.MinimumSize = new System.Drawing.Size(725, 580);
      this.Name = "SettingsForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Settings";
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPButton closeButton;
    private MPBeveledLine beveledLine1;
    protected MPTreeView sectionTree;
    protected MPPanel holderPanel;
    private MPGradientLabel headerLabel;
    private MPLinkLabel linkLabelDonate;
    protected MPToolStrip toolStrip1;
    protected MPToolStripButton helpToolStripSplitButton;
    protected MPToolStripButton configToolStripSplitButton;
  }
}