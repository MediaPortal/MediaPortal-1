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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Servers));
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.buttonDelete = new System.Windows.Forms.Button();
      this.buttonMaster = new System.Windows.Forms.Button();
      this.button1 = new System.Windows.Forms.Button();
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
      this.mpListView1.LargeImageList = this.imageList1;
      this.mpListView1.Location = new System.Drawing.Point(3, 3);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(458, 192);
      this.mpListView1.SmallImageList = this.imageList1;
      this.mpListView1.TabIndex = 1;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      this.mpListView1.SelectedIndexChanged += new System.EventHandler(this.mpListView1_SelectedIndexChanged);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Server";
      this.columnHeader1.Width = 200;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Type";
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "computer.gif");
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
      // buttonMaster
      // 
      this.buttonMaster.Location = new System.Drawing.Point(266, 201);
      this.buttonMaster.Name = "buttonMaster";
      this.buttonMaster.Size = new System.Drawing.Size(114, 23);
      this.buttonMaster.TabIndex = 4;
      this.buttonMaster.Text = "Set as master";
      this.buttonMaster.UseVisualStyleBackColor = true;
      this.buttonMaster.Click += new System.EventHandler(this.buttonMaster_Click);
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(206, 201);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(54, 23);
      this.button1.TabIndex = 5;
      this.button1.Text = "Edit";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // Servers
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.button1);
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
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.ImageList imageList1;
  }
}