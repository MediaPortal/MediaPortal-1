namespace MediaPortal.Configuration
{
  partial class SettingsForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.Windows.Forms.LinkLabel linkLabel1;
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripButton helpToolStripSplitButton;
    private System.Windows.Forms.ToolStripSplitButton configToolStripSplitButton;
    private System.Windows.Forms.ToolStripMenuItem thumbsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem logsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem databaseToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem skinsToolStripMenuItem;
    private MediaPortal.UserInterface.Controls.MPButton cancelButton;
    private MediaPortal.UserInterface.Controls.MPButton okButton;
    private MediaPortal.UserInterface.Controls.MPBeveledLine beveledLine1;
    private System.Windows.Forms.TreeView sectionTree;
    private System.Windows.Forms.Panel holderPanel;
    private MediaPortal.UserInterface.Controls.MPGradientLabel headerLabel;
    private MediaPortal.UserInterface.Controls.MPButton applyButton;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        /*if(components != null)
				{
					components.Dispose();
				}*/
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
      System.ComponentModel.ComponentResourceManager resources =
        new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
      this.sectionTree = new System.Windows.Forms.TreeView();
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPGradientLabel();
      this.holderPanel = new System.Windows.Forms.Panel();
      this.beveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.applyButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.helpToolStripSplitButton = new System.Windows.Forms.ToolStripButton();
      this.configToolStripSplitButton = new System.Windows.Forms.ToolStripSplitButton();
      this.thumbsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.logsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.databaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.skinsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripButtonSwitchAdvanced = new System.Windows.Forms.ToolStripButton();
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // sectionTree
      // 
      this.sectionTree.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
           | System.Windows.Forms.AnchorStyles.Left)));
      this.sectionTree.FullRowSelect = true;
      this.sectionTree.HideSelection = false;
      this.sectionTree.HotTracking = true;
      this.sectionTree.Indent = 19;
      this.sectionTree.ItemHeight = 16;
      this.sectionTree.Location = new System.Drawing.Point(16, 28);
      this.sectionTree.Name = "sectionTree";
      this.sectionTree.Size = new System.Drawing.Size(184, 486);
      this.sectionTree.TabIndex = 2;
      this.sectionTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.sectionTree_AfterSelect);
      this.sectionTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.sectionTree_BeforeSelect);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(621, 537);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(540, 537);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 0;
      this.okButton.Text = "&OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Caption = "";
      this.headerLabel.FirstColor = System.Drawing.SystemColors.InactiveCaption;
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular,
                                                      System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.headerLabel.LastColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.Location = new System.Drawing.Point(216, 28);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.PaddingLeft = 2;
      this.headerLabel.Size = new System.Drawing.Size(480, 24);
      this.headerLabel.TabIndex = 3;
      this.headerLabel.TabStop = false;
      this.headerLabel.TextColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.TextFont = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular,
                                                          System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      // 
      // holderPanel
      // 
      this.holderPanel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.holderPanel.AutoScroll = true;
      this.holderPanel.BackColor = System.Drawing.SystemColors.Control;
      this.holderPanel.Location = new System.Drawing.Point(216, 58);
      this.holderPanel.Name = "holderPanel";
      this.holderPanel.Size = new System.Drawing.Size(480, 456);
      this.holderPanel.TabIndex = 4;
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(8, 527);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(696, 2);
      this.beveledLine1.TabIndex = 5;
      this.beveledLine1.TabStop = false;
      // 
      // applyButton
      // 
      this.applyButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.applyButton.Location = new System.Drawing.Point(459, 537);
      this.applyButton.Name = "applyButton";
      this.applyButton.Size = new System.Drawing.Size(75, 23);
      this.applyButton.TabIndex = 6;
      this.applyButton.TabStop = false;
      this.applyButton.Text = "&Apply";
      this.applyButton.UseVisualStyleBackColor = true;
      this.applyButton.Visible = false;
      this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
      // 
      // linkLabel1
      // 
      this.linkLabel1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(12, 542);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(113, 13);
      this.linkLabel1.TabIndex = 9;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "Donate to MediaPortal";
      this.linkLabel1.LinkClicked +=
        new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // toolStrip1
      // 
      this.toolStrip1.ImeMode = System.Windows.Forms.ImeMode.On;
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
                                       {
                                         this.helpToolStripSplitButton,
                                         this.configToolStripSplitButton,
                                         this.toolStripButtonSwitchAdvanced
                                       });
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(712, 25);
      this.toolStrip1.TabIndex = 10;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // helpToolStripSplitButton
      // 
      this.helpToolStripSplitButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.helpToolStripSplitButton.Image = global::MediaPortal.Configuration.Properties.Resources.icon_help;
      this.helpToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.helpToolStripSplitButton.Name = "helpToolStripSplitButton";
      this.helpToolStripSplitButton.Size = new System.Drawing.Size(52, 22);
      this.helpToolStripSplitButton.Text = "Help";
      this.helpToolStripSplitButton.ToolTipText = "Opens the online wiki page for the active configuration section.";
      this.helpToolStripSplitButton.Click += new System.EventHandler(this.helpToolStripSplitButton_Click);
      // 
      // configToolStripSplitButton
      // 
      this.configToolStripSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
                                                               {
                                                                 this.thumbsToolStripMenuItem,
                                                                 this.logsToolStripMenuItem,
                                                                 this.databaseToolStripMenuItem,
                                                                 this.skinsToolStripMenuItem
                                                               });
      this.configToolStripSplitButton.Image = global::MediaPortal.Configuration.Properties.Resources.icon_folder;
      this.configToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.configToolStripSplitButton.Name = "configToolStripSplitButton";
      this.configToolStripSplitButton.Size = new System.Drawing.Size(125, 22);
      this.configToolStripSplitButton.Text = "User Config files";
      this.configToolStripSplitButton.ButtonClick += new System.EventHandler(this.configToolStripSplitButton_ButtonClick);
      // 
      // thumbsToolStripMenuItem
      // 
      this.thumbsToolStripMenuItem.Image = global::MediaPortal.Configuration.Properties.Resources.icon_folder;
      this.thumbsToolStripMenuItem.Name = "thumbsToolStripMenuItem";
      this.thumbsToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
      this.thumbsToolStripMenuItem.Text = "Open Thumbs directory";
      this.thumbsToolStripMenuItem.Click += new System.EventHandler(this.thumbsToolStripMenuItem_Click);
      // 
      // logsToolStripMenuItem
      // 
      this.logsToolStripMenuItem.Image = global::MediaPortal.Configuration.Properties.Resources.icon_folder;
      this.logsToolStripMenuItem.Name = "logsToolStripMenuItem";
      this.logsToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
      this.logsToolStripMenuItem.Text = "Open Log directory";
      this.logsToolStripMenuItem.Click += new System.EventHandler(this.logsToolStripMenuItem_Click);
      // 
      // databaseToolStripMenuItem
      // 
      this.databaseToolStripMenuItem.Image = global::MediaPortal.Configuration.Properties.Resources.icon_folder;
      this.databaseToolStripMenuItem.Name = "databaseToolStripMenuItem";
      this.databaseToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
      this.databaseToolStripMenuItem.Text = "Open Database directory";
      this.databaseToolStripMenuItem.Click += new System.EventHandler(this.databaseToolStripMenuItem_Click);
      // 
      // skinsToolStripMenuItem
      // 
      this.skinsToolStripMenuItem.Image = global::MediaPortal.Configuration.Properties.Resources.icon_folder;
      this.skinsToolStripMenuItem.Name = "skinsToolStripMenuItem";
      this.skinsToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
      this.skinsToolStripMenuItem.Text = "Open Skins directory";
      this.skinsToolStripMenuItem.Click += new System.EventHandler(this.skinsToolStripMenuItem_Click);
      // 
      // toolStripButtonSwitchAdvanced
      // 
      this.toolStripButtonSwitchAdvanced.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.toolStripButtonSwitchAdvanced.AutoSize = false;
      this.toolStripButtonSwitchAdvanced.CheckOnClick = true;
      this.toolStripButtonSwitchAdvanced.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.toolStripButtonSwitchAdvanced.Image =
        ((System.Drawing.Image)(resources.GetObject("toolStripButtonSwitchAdvanced.Image")));
      this.toolStripButtonSwitchAdvanced.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButtonSwitchAdvanced.Name = "toolStripButtonSwitchAdvanced";
      this.toolStripButtonSwitchAdvanced.Size = new System.Drawing.Size(135, 22);
      this.toolStripButtonSwitchAdvanced.Text = "Switch to expert mode";
      this.toolStripButtonSwitchAdvanced.Click += new System.EventHandler(this.toolStripButtonSwitchAdvanced_Click);
      // 
      // SettingsForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScroll = true;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(712, 568);
      this.Controls.Add(this.toolStrip1);
      this.Controls.Add(this.linkLabel1);
      this.Controls.Add(this.applyButton);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.holderPanel);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.sectionTree);
      this.Name = "SettingsForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MediaPortal - Configuration";
      this.Load += new System.EventHandler(this.SettingsForm_Load);
      this.Closed += new System.EventHandler(this.SettingsForm_Closed);
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    #endregion
  }
}