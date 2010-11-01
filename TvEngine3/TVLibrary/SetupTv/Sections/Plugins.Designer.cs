namespace SetupTv.Sections
{
  partial class Plugins
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
      this.listView1 = new System.Windows.Forms.ListView();
      this.clmnEnabled = new System.Windows.Forms.ColumnHeader();
      this.clmnName = new System.Windows.Forms.ColumnHeader();
      this.clmnAuthor = new System.Windows.Forms.ColumnHeader();
      this.clmnVersion = new System.Windows.Forms.ColumnHeader();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpLabelChannelCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SuspendLayout();
      // 
      // listView1
      // 
      this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listView1.CheckBoxes = true;
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmnEnabled,
            this.clmnName,
            this.clmnAuthor,
            this.clmnVersion});
      this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.listView1.Location = new System.Drawing.Point(0, 0);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(476, 428);
      this.listView1.TabIndex = 0;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      this.listView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView1_ItemChecked);
      // 
      // clmnEnabled
      // 
      this.clmnEnabled.Text = "Enabled";
      // 
      // clmnName
      // 
      this.clmnName.Text = "Name";
      this.clmnName.Width = 140;
      // 
      // clmnAuthor
      // 
      this.clmnAuthor.Text = "Author";
      this.clmnAuthor.Width = 120;
      // 
      // clmnVersion
      // 
      this.clmnVersion.Text = "Version";
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(484, 454);
      this.tabControl1.TabIndex = 10;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpLabelChannelCount);
      this.tabPage1.Controls.Add(this.listView1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(476, 428);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Plugins";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpLabelChannelCount
      // 
      this.mpLabelChannelCount.AutoSize = true;
      this.mpLabelChannelCount.Location = new System.Drawing.Point(13, 14);
      this.mpLabelChannelCount.Name = "mpLabelChannelCount";
      this.mpLabelChannelCount.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannelCount.TabIndex = 2;
      // 
      // Plugins
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "Plugins";
      this.Size = new System.Drawing.Size(484, 454);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.ColumnHeader clmnEnabled;
    private System.Windows.Forms.ColumnHeader clmnName;
    private System.Windows.Forms.ColumnHeader clmnAuthor;
    private System.Windows.Forms.ColumnHeader clmnVersion;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannelCount;
  }
}