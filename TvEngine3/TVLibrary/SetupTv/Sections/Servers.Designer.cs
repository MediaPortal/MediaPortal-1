namespace SetupTv.Sections
{
  partial class Servers
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
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.buttonDelete = new System.Windows.Forms.Button();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.buttonMaster = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.Location = new System.Drawing.Point(3, 3);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(458, 192);
      this.mpListView1.TabIndex = 1;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Server";
      this.columnHeader1.Width = 200;
      // 
      // buttonDelete
      // 
      this.buttonDelete.Location = new System.Drawing.Point(386, 201);
      this.buttonDelete.Name = "buttonDelete";
      this.buttonDelete.Size = new System.Drawing.Size(75, 23);
      this.buttonDelete.TabIndex = 2;
      this.buttonDelete.Text = "Delete";
      this.buttonDelete.UseVisualStyleBackColor = true;
      this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Type";
      // 
      // buttonMaster
      // 
      this.buttonMaster.Location = new System.Drawing.Point(252, 201);
      this.buttonMaster.Name = "buttonMaster";
      this.buttonMaster.Size = new System.Drawing.Size(114, 23);
      this.buttonMaster.TabIndex = 4;
      this.buttonMaster.Text = "Set as master";
      this.buttonMaster.UseVisualStyleBackColor = true;
      this.buttonMaster.Click += new System.EventHandler(this.buttonMaster_Click);
      // 
      // Servers
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.buttonMaster);
      this.Controls.Add(this.buttonDelete);
      this.Controls.Add(this.mpListView1);
      this.Name = "Servers";
      this.Size = new System.Drawing.Size(469, 449);
      this.Load += new System.EventHandler(this.Servers_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.Button buttonDelete;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.Button buttonMaster;
  }
}