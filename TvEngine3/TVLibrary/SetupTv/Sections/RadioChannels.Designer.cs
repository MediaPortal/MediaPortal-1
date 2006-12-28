namespace SetupTv.Sections
{
  partial class RadioChannels
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RadioChannels));
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.buttonDelete = new System.Windows.Forms.Button();
      this.mpButtonAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonDel = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonEdit = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonDown = new System.Windows.Forms.Button();
      this.buttonUtp = new System.Windows.Forms.Button();
      this.mpButtonDeleteEncrypted = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Details";
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Details";
      // 
      // columnHeader6
      // 
      this.columnHeader6.Text = "Details";
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Details";
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3,
            this.columnHeader2,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.LargeImageList = this.imageList1;
      this.mpListView1.Location = new System.Drawing.Point(14, 30);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(438, 302);
      this.mpListView1.SmallImageList = this.imageList1;
      this.mpListView1.TabIndex = 1;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      this.mpListView1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.mpListView1_ItemDrag);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Name";
      this.columnHeader1.Width = 100;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Types";
      this.columnHeader3.Width = 90;
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "radio_scrambled.png");
      this.imageList1.Images.SetKeyName(1, "tv_fta_.png");
      this.imageList1.Images.SetKeyName(2, "tv_scrambled.png");
      this.imageList1.Images.SetKeyName(3, "radio_fta_.png");
      // 
      // buttonDelete
      // 
      this.buttonDelete.Location = new System.Drawing.Point(131, 338);
      this.buttonDelete.Name = "buttonDelete";
      this.buttonDelete.Size = new System.Drawing.Size(75, 23);
      this.buttonDelete.TabIndex = 2;
      this.buttonDelete.Text = "Delete";
      this.buttonDelete.UseVisualStyleBackColor = true;
      this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
      // 
      // mpButtonAdd
      // 
      this.mpButtonAdd.Location = new System.Drawing.Point(131, 363);
      this.mpButtonAdd.Name = "mpButtonAdd";
      this.mpButtonAdd.Size = new System.Drawing.Size(54, 23);
      this.mpButtonAdd.TabIndex = 14;
      this.mpButtonAdd.Text = "Add";
      this.mpButtonAdd.UseVisualStyleBackColor = true;
      this.mpButtonAdd.Click += new System.EventHandler(this.mpButtonAdd_Click);
      // 
      // mpButtonDel
      // 
      this.mpButtonDel.Location = new System.Drawing.Point(67, 362);
      this.mpButtonDel.Name = "mpButtonDel";
      this.mpButtonDel.Size = new System.Drawing.Size(54, 23);
      this.mpButtonDel.TabIndex = 10;
      this.mpButtonDel.Text = "Delete";
      this.mpButtonDel.UseVisualStyleBackColor = true;
      this.mpButtonDel.Click += new System.EventHandler(this.mpButtonDel_Click);
      // 
      // mpButtonEdit
      // 
      this.mpButtonEdit.Location = new System.Drawing.Point(67, 338);
      this.mpButtonEdit.Name = "mpButtonEdit";
      this.mpButtonEdit.Size = new System.Drawing.Size(58, 23);
      this.mpButtonEdit.TabIndex = 13;
      this.mpButtonEdit.Text = "Edit";
      this.mpButtonEdit.UseVisualStyleBackColor = true;
      this.mpButtonEdit.Click += new System.EventHandler(this.mpButtonEdit_Click);
      // 
      // buttonDown
      // 
      this.buttonDown.Location = new System.Drawing.Point(17, 362);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(44, 23);
      this.buttonDown.TabIndex = 12;
      this.buttonDown.Text = "Down";
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // buttonUtp
      // 
      this.buttonUtp.Location = new System.Drawing.Point(17, 338);
      this.buttonUtp.Name = "buttonUtp";
      this.buttonUtp.Size = new System.Drawing.Size(44, 23);
      this.buttonUtp.TabIndex = 11;
      this.buttonUtp.Text = "Up";
      this.buttonUtp.UseVisualStyleBackColor = true;
      this.buttonUtp.Click += new System.EventHandler(this.buttonUtp_Click);
      // 
      // mpButtonDeleteEncrypted
      // 
      this.mpButtonDeleteEncrypted.Location = new System.Drawing.Point(212, 338);
      this.mpButtonDeleteEncrypted.Name = "mpButtonDeleteEncrypted";
      this.mpButtonDeleteEncrypted.Size = new System.Drawing.Size(103, 23);
      this.mpButtonDeleteEncrypted.TabIndex = 15;
      this.mpButtonDeleteEncrypted.Text = "Delete Scrambled";
      this.mpButtonDeleteEncrypted.UseVisualStyleBackColor = true;
      this.mpButtonDeleteEncrypted.Click += new System.EventHandler(this.mpButtonDeleteEncrypted_Click);
      // 
      // RadioChannels
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpButtonDeleteEncrypted);
      this.Controls.Add(this.mpButtonAdd);
      this.Controls.Add(this.mpButtonDel);
      this.Controls.Add(this.mpButtonEdit);
      this.Controls.Add(this.buttonDown);
      this.Controls.Add(this.buttonUtp);
      this.Controls.Add(this.buttonDelete);
      this.Controls.Add(this.mpListView1);
      this.Name = "RadioChannels";
      this.Size = new System.Drawing.Size(467, 388);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ColumnHeader columnHeader4;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.ColumnHeader columnHeader6;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.Button buttonDelete;
    private System.Windows.Forms.ImageList imageList1;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonAdd;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonDel;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonEdit;
    private System.Windows.Forms.Button buttonDown;
    private System.Windows.Forms.Button buttonUtp;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonDeleteEncrypted;
  }
}