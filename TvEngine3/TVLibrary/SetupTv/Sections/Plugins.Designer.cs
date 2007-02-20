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
      this.SuspendLayout();
      // 
      // listView1
      // 
      this.listView1.CheckBoxes = true;
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmnEnabled,
            this.clmnName,
            this.clmnAuthor,
            this.clmnVersion});
      this.listView1.Location = new System.Drawing.Point(19, 14);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(447, 400);
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
      // Plugins
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listView1);
      this.Name = "Plugins";
      this.Size = new System.Drawing.Size(482, 449);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.ColumnHeader clmnEnabled;
    private System.Windows.Forms.ColumnHeader clmnName;
    private System.Windows.Forms.ColumnHeader clmnAuthor;
    private System.Windows.Forms.ColumnHeader clmnVersion;
  }
}