namespace SetupTv.Sections
{
  partial class TvChannels
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
      this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.mpButtonClear = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonClearEncrypted = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabelChannelCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonDel = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonUtp = new System.Windows.Forms.Button();
      this.buttonDown = new System.Windows.Forms.Button();
      this.mpButtonEdit = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.CheckBoxes = true;
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader1,
            this.columnHeader3,
            this.columnHeader2,
            this.columnHeader4});
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.LabelEdit = true;
      this.mpListView1.Location = new System.Drawing.Point(16, 37);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(438, 306);
      this.mpListView1.TabIndex = 0;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      this.mpListView1.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.mpListView1_AfterLabelEdit);
      // 
      // columnHeader7
      // 
      this.columnHeader7.Text = "#";
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
      // columnHeader2
      // 
      this.columnHeader2.Text = "Details";
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Details";
      // 
      // mpButtonClear
      // 
      this.mpButtonClear.Location = new System.Drawing.Point(402, 347);
      this.mpButtonClear.Name = "mpButtonClear";
      this.mpButtonClear.Size = new System.Drawing.Size(51, 23);
      this.mpButtonClear.TabIndex = 1;
      this.mpButtonClear.Text = "Clear";
      this.mpButtonClear.UseVisualStyleBackColor = true;
      this.mpButtonClear.Click += new System.EventHandler(this.mpButtonClear_Click);
      // 
      // mpButtonClearEncrypted
      // 
      this.mpButtonClearEncrypted.Location = new System.Drawing.Point(297, 347);
      this.mpButtonClearEncrypted.Name = "mpButtonClearEncrypted";
      this.mpButtonClearEncrypted.Size = new System.Drawing.Size(100, 23);
      this.mpButtonClearEncrypted.TabIndex = 1;
      this.mpButtonClearEncrypted.Text = "Delete Encrypted";
      this.mpButtonClearEncrypted.UseVisualStyleBackColor = true;
      this.mpButtonClearEncrypted.Click += new System.EventHandler(this.mpButtonClearEncrypted_Click);
      // 
      // mpLabelChannelCount
      // 
      this.mpLabelChannelCount.AutoSize = true;
      this.mpLabelChannelCount.Location = new System.Drawing.Point(16, 10);
      this.mpLabelChannelCount.Name = "mpLabelChannelCount";
      this.mpLabelChannelCount.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannelCount.TabIndex = 2;
      // 
      // mpButtonDel
      // 
      this.mpButtonDel.Location = new System.Drawing.Point(237, 347);
      this.mpButtonDel.Name = "mpButtonDel";
      this.mpButtonDel.Size = new System.Drawing.Size(54, 23);
      this.mpButtonDel.TabIndex = 1;
      this.mpButtonDel.Text = "Delete";
      this.mpButtonDel.UseVisualStyleBackColor = true;
      this.mpButtonDel.Click += new System.EventHandler(this.mpButtonDel_Click);
      // 
      // buttonUtp
      // 
      this.buttonUtp.Location = new System.Drawing.Point(19, 350);
      this.buttonUtp.Name = "buttonUtp";
      this.buttonUtp.Size = new System.Drawing.Size(38, 23);
      this.buttonUtp.TabIndex = 3;
      this.buttonUtp.Text = "Up";
      this.buttonUtp.UseVisualStyleBackColor = true;
      this.buttonUtp.Click += new System.EventHandler(this.buttonUtp_Click);
      // 
      // buttonDown
      // 
      this.buttonDown.Location = new System.Drawing.Point(63, 350);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(44, 23);
      this.buttonDown.TabIndex = 4;
      this.buttonDown.Text = "Down";
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // mpButtonEdit
      // 
      this.mpButtonEdit.Location = new System.Drawing.Point(173, 347);
      this.mpButtonEdit.Name = "mpButtonEdit";
      this.mpButtonEdit.Size = new System.Drawing.Size(58, 23);
      this.mpButtonEdit.TabIndex = 5;
      this.mpButtonEdit.Text = "Edit";
      this.mpButtonEdit.UseVisualStyleBackColor = true;
      this.mpButtonEdit.Click += new System.EventHandler(this.mpButtonEdit_Click);
      // 
      // TvChannels
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpButtonEdit);
      this.Controls.Add(this.buttonDown);
      this.Controls.Add(this.buttonUtp);
      this.Controls.Add(this.mpLabelChannelCount);
      this.Controls.Add(this.mpButtonDel);
      this.Controls.Add(this.mpButtonClearEncrypted);
      this.Controls.Add(this.mpButtonClear);
      this.Controls.Add(this.mpListView1);
      this.Name = "TvChannels";
      this.Size = new System.Drawing.Size(467, 388);
      this.Load += new System.EventHandler(this.TvChannels_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonClear;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonClearEncrypted;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabelChannelCount;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonDel;
    private System.Windows.Forms.Button buttonUtp;
    private System.Windows.Forms.Button buttonDown;
    private System.Windows.Forms.ColumnHeader columnHeader7;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonEdit;
  }
}