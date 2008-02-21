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
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.TVETabCtrl = new System.Windows.Forms.TabControl();
      this.panel1 = new System.Windows.Forms.Panel();
      this.cbClearOnCreate = new System.Windows.Forms.CheckBox();
      this.btnChooseFont = new System.Windows.Forms.Button();
      this.cbFollowTail = new System.Windows.Forms.CheckBox();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.tabPageTvEngine = new System.Windows.Forms.TabPage();
      this.richTextBoxTvEngine = new System.Windows.Forms.RichTextBox();
      this.tabPage3 = new System.Windows.Forms.TabPage();
      this.CustomTabCtrl = new System.Windows.Forms.TabControl();
      this.PageCtrlCategory.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.TVETabCtrl.SuspendLayout();
      this.panel1.SuspendLayout();
      this.tabPageTvEngine.SuspendLayout();
      this.tabPage3.SuspendLayout();
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
      this.PageCtrlCategory.Location = new System.Drawing.Point(14, 44);
      this.PageCtrlCategory.Name = "PageCtrlCategory";
      this.PageCtrlCategory.SelectedIndex = 0;
      this.PageCtrlCategory.Size = new System.Drawing.Size(1004, 533);
      this.PageCtrlCategory.TabIndex = 0;
      this.PageCtrlCategory.Selected += new System.Windows.Forms.TabControlEventHandler(this.PageCtrlCategory_Selected);
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.MPTabCtrl);
      this.tabPage1.Location = new System.Drawing.Point(4, 27);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(996, 502);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "MediaPortal";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // MPTabCtrl
      // 
      this.MPTabCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MPTabCtrl.Location = new System.Drawing.Point(3, 3);
      this.MPTabCtrl.Name = "MPTabCtrl";
      this.MPTabCtrl.SelectedIndex = 0;
      this.MPTabCtrl.Size = new System.Drawing.Size(990, 496);
      this.MPTabCtrl.TabIndex = 0;
      this.MPTabCtrl.Selected += new System.Windows.Forms.TabControlEventHandler(this.MPTabCtrl_Selected);
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.TVETabCtrl);
      this.tabPage2.Location = new System.Drawing.Point(4, 27);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(996, 502);
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
      this.TVETabCtrl.Size = new System.Drawing.Size(990, 496);
      this.TVETabCtrl.TabIndex = 1;
      this.TVETabCtrl.Selected += new System.Windows.Forms.TabControlEventHandler(this.MPTabCtrl_Selected);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.cbClearOnCreate);
      this.panel1.Controls.Add(this.btnChooseFont);
      this.panel1.Controls.Add(this.cbFollowTail);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(1018, 37);
      this.panel1.TabIndex = 1;
      // 
      // cbClearOnCreate
      // 
      this.cbClearOnCreate.AutoSize = true;
      this.cbClearOnCreate.Checked = true;
      this.cbClearOnCreate.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbClearOnCreate.Location = new System.Drawing.Point(134, 9);
      this.cbClearOnCreate.Name = "cbClearOnCreate";
      this.cbClearOnCreate.Size = new System.Drawing.Size(243, 19);
      this.cbClearOnCreate.TabIndex = 3;
      this.cbClearOnCreate.Text = "Clear window if file is created";
      this.cbClearOnCreate.UseVisualStyleBackColor = true;
      this.cbClearOnCreate.CheckedChanged += new System.EventHandler(this.cbClearOnCreate_CheckedChanged);
      // 
      // btnChooseFont
      // 
      this.btnChooseFont.Location = new System.Drawing.Point(400, 4);
      this.btnChooseFont.Name = "btnChooseFont";
      this.btnChooseFont.Size = new System.Drawing.Size(87, 27);
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
      // tabPageTvEngine
      // 
      this.tabPageTvEngine.Controls.Add(this.richTextBoxTvEngine);
      this.tabPageTvEngine.Location = new System.Drawing.Point(4, 24);
      this.tabPageTvEngine.Name = "tabPageTvEngine";
      this.tabPageTvEngine.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageTvEngine.Size = new System.Drawing.Size(982, 468);
      this.tabPageTvEngine.TabIndex = 0;
      this.tabPageTvEngine.Text = "Combined view";
      this.tabPageTvEngine.UseVisualStyleBackColor = true;
      // 
      // richTextBoxTvEngine
      // 
      this.richTextBoxTvEngine.Dock = System.Windows.Forms.DockStyle.Fill;
      this.richTextBoxTvEngine.Location = new System.Drawing.Point(3, 3);
      this.richTextBoxTvEngine.Name = "richTextBoxTvEngine";
      this.richTextBoxTvEngine.Size = new System.Drawing.Size(976, 462);
      this.richTextBoxTvEngine.TabIndex = 0;
      this.richTextBoxTvEngine.Text = "";
      this.richTextBoxTvEngine.WordWrap = false;
      // 
      // tabPage3
      // 
      this.tabPage3.Controls.Add(this.CustomTabCtrl);
      this.tabPage3.Location = new System.Drawing.Point(4, 27);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage3.Size = new System.Drawing.Size(996, 502);
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
      this.CustomTabCtrl.Size = new System.Drawing.Size(990, 496);
      this.CustomTabCtrl.TabIndex = 0;
      // 
      // frmMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1018, 573);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.PageCtrlCategory);
      this.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Name = "frmMain";
      this.Text = "MediaPortal Tail";
      this.Shown += new System.EventHandler(this.Form1_Shown);
      this.PageCtrlCategory.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.TVETabCtrl.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.tabPageTvEngine.ResumeLayout(false);
      this.tabPage3.ResumeLayout(false);
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
    private System.Windows.Forms.RichTextBox richTextBoxTvEngine;
    private System.Windows.Forms.TabPage tabPage3;
    private System.Windows.Forms.TabControl CustomTabCtrl;
  }
}

