namespace SetupTv.Sections
{
  partial class TvSchedules
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
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.PreRecord = new System.Windows.Forms.ColumnHeader();
      this.PostRecord = new System.Windows.Forms.ColumnHeader();
      this.SuspendLayout();
      // 
      // listView1
      // 
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader1,
            this.columnHeader4,
            this.columnHeader2,
            this.columnHeader3,
            this.PreRecord,
            this.PostRecord});
      this.listView1.Location = new System.Drawing.Point(13, 19);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(455, 399);
      this.listView1.TabIndex = 0;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Priority";
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Channel";
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Type";
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Date";
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Title";
      // 
      // PreRecord
      // 
      this.PreRecord.Text = "PreRecord";
      // 
      // PostRecord
      // 
      this.PostRecord.Text = "PostRecord";
      // 
      // TvSchedules
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listView1);
      this.Name = "TvSchedules";
      this.Size = new System.Drawing.Size(482, 449);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.ColumnHeader PreRecord;
    private System.Windows.Forms.ColumnHeader PostRecord;
  }
}