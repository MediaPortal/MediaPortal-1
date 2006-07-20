namespace SetupTv.Sections
{
  partial class RadioEpgGrabber
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
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpListView2 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonAll = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonNone = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonClearChannels = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonAllChannels = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.CheckBoxes = true;
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3});
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.Location = new System.Drawing.Point(14, 41);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(208, 306);
      this.mpListView1.TabIndex = 1;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      this.mpListView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.mpListView1_ItemChecked);
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
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(11, 15);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(119, 13);
      this.mpLabel1.TabIndex = 2;
      this.mpLabel1.Text = "Grab EPG for channels:";
      // 
      // mpListView2
      // 
      this.mpListView2.AllowDrop = true;
      this.mpListView2.AllowRowReorder = true;
      this.mpListView2.CheckBoxes = true;
      this.mpListView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
      this.mpListView2.Location = new System.Drawing.Point(244, 41);
      this.mpListView2.Name = "mpListView2";
      this.mpListView2.Size = new System.Drawing.Size(205, 262);
      this.mpListView2.TabIndex = 3;
      this.mpListView2.UseCompatibleStateImageBehavior = false;
      this.mpListView2.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Language";
      this.columnHeader2.Width = 180;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(244, 15);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(63, 13);
      this.mpLabel2.TabIndex = 4;
      this.mpLabel2.Text = "Languages:";
      // 
      // mpButtonAll
      // 
      this.mpButtonAll.Location = new System.Drawing.Point(244, 309);
      this.mpButtonAll.Name = "mpButtonAll";
      this.mpButtonAll.Size = new System.Drawing.Size(75, 23);
      this.mpButtonAll.TabIndex = 5;
      this.mpButtonAll.Text = "All";
      this.mpButtonAll.UseVisualStyleBackColor = true;
      this.mpButtonAll.Click += new System.EventHandler(this.mpButtonAll_Click);
      // 
      // mpButtonNone
      // 
      this.mpButtonNone.Location = new System.Drawing.Point(374, 309);
      this.mpButtonNone.Name = "mpButtonNone";
      this.mpButtonNone.Size = new System.Drawing.Size(75, 23);
      this.mpButtonNone.TabIndex = 6;
      this.mpButtonNone.Text = "None";
      this.mpButtonNone.UseVisualStyleBackColor = true;
      this.mpButtonNone.Click += new System.EventHandler(this.mpButtonNone_Click);
      // 
      // mpButtonClearChannels
      // 
      this.mpButtonClearChannels.Location = new System.Drawing.Point(147, 353);
      this.mpButtonClearChannels.Name = "mpButtonClearChannels";
      this.mpButtonClearChannels.Size = new System.Drawing.Size(75, 23);
      this.mpButtonClearChannels.TabIndex = 8;
      this.mpButtonClearChannels.Text = "None";
      this.mpButtonClearChannels.UseVisualStyleBackColor = true;
      this.mpButtonClearChannels.Click += new System.EventHandler(this.mpButtonClearChannels_Click);
      // 
      // mpButtonAllChannels
      // 
      this.mpButtonAllChannels.Location = new System.Drawing.Point(17, 353);
      this.mpButtonAllChannels.Name = "mpButtonAllChannels";
      this.mpButtonAllChannels.Size = new System.Drawing.Size(75, 23);
      this.mpButtonAllChannels.TabIndex = 7;
      this.mpButtonAllChannels.Text = "All";
      this.mpButtonAllChannels.UseVisualStyleBackColor = true;
      this.mpButtonAllChannels.Click += new System.EventHandler(this.mpButtonAllChannels_Click);
      // 
      // RadioEpgGrabber
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpButtonClearChannels);
      this.Controls.Add(this.mpButtonAllChannels);
      this.Controls.Add(this.mpButtonNone);
      this.Controls.Add(this.mpButtonAll);
      this.Controls.Add(this.mpLabel2);
      this.Controls.Add(this.mpListView2);
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.mpListView1);
      this.Name = "RadioEpgGrabber";
      this.Size = new System.Drawing.Size(467, 388);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPListView mpListView2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonAll;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonNone;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonClearChannels;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonAllChannels;

  }
}