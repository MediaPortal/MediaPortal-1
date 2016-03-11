namespace SetupTv.Dialogs
{
  partial class FormEditChannel
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditChannel));
      this.mpButtonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxVisibleInTvGuide = new System.Windows.Forms.CheckBox();
      this.textBoxName = new System.Windows.Forms.TextBox();
      this.label25 = new System.Windows.Forms.Label();
      this.mpButtonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpListView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.id = new System.Windows.Forms.ColumnHeader();
      this.name = new System.Windows.Forms.ColumnHeader();
      this.provider = new System.Windows.Forms.ColumnHeader();
      this.type = new System.Windows.Forms.ColumnHeader();
      this.details = new System.Windows.Forms.ColumnHeader();
      this.tuningDetailContextMenu = new MediaPortal.UserInterface.Controls.MPContextMenuStrip();
      this.menuButtonAdd = new System.Windows.Forms.ToolStripMenuItem();
      this.menuButtonEdit = new System.Windows.Forms.ToolStripMenuItem();
      this.menuButtonRemove = new System.Windows.Forms.ToolStripMenuItem();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.gbTuningdetails = new System.Windows.Forms.GroupBox();
      this.btnRemoveTuningDetail = new System.Windows.Forms.Button();
      this.btnEditTuningDetail = new System.Windows.Forms.Button();
      this.btnAddTuningDetail = new System.Windows.Forms.Button();
      this.textBoxChannelNumber = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.tuningDetailContextMenu.SuspendLayout();
      this.gbTuningdetails.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpButtonOk
      // 
      this.mpButtonOk.Location = new System.Drawing.Point(278, 365);
      this.mpButtonOk.Name = "mpButtonOk";
      this.mpButtonOk.Size = new System.Drawing.Size(75, 23);
      this.mpButtonOk.TabIndex = 49;
      this.mpButtonOk.Text = "OK";
      this.mpButtonOk.UseVisualStyleBackColor = true;
      this.mpButtonOk.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // checkBoxVisibleInTvGuide
      // 
      this.checkBoxVisibleInTvGuide.AutoSize = true;
      this.checkBoxVisibleInTvGuide.Location = new System.Drawing.Point(31, 64);
      this.checkBoxVisibleInTvGuide.Name = "checkBoxVisibleInTvGuide";
      this.checkBoxVisibleInTvGuide.Size = new System.Drawing.Size(108, 17);
      this.checkBoxVisibleInTvGuide.TabIndex = 1;
      this.checkBoxVisibleInTvGuide.Text = "Visible in tv guide";
      this.checkBoxVisibleInTvGuide.TextAlign = System.Drawing.ContentAlignment.TopRight;
      this.checkBoxVisibleInTvGuide.UseVisualStyleBackColor = true;
      // 
      // textBoxName
      // 
      this.textBoxName.Location = new System.Drawing.Point(136, 12);
      this.textBoxName.Name = "textBoxName";
      this.textBoxName.Size = new System.Drawing.Size(100, 20);
      this.textBoxName.TabIndex = 0;
      this.textBoxName.Text = "4";
      // 
      // label25
      // 
      this.label25.AutoSize = true;
      this.label25.Location = new System.Drawing.Point(28, 15);
      this.label25.Name = "label25";
      this.label25.Size = new System.Drawing.Size(35, 13);
      this.label25.TabIndex = 8;
      this.label25.Text = "Name";
      // 
      // mpButtonCancel
      // 
      this.mpButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpButtonCancel.Location = new System.Drawing.Point(377, 365);
      this.mpButtonCancel.Name = "mpButtonCancel";
      this.mpButtonCancel.Size = new System.Drawing.Size(75, 23);
      this.mpButtonCancel.TabIndex = 50;
      this.mpButtonCancel.Text = "Cancel";
      this.mpButtonCancel.UseVisualStyleBackColor = true;
      this.mpButtonCancel.Click += new System.EventHandler(this.mpButtonCancel_Click);
      // 
      // mpListView1
      // 
      this.mpListView1.AllowDrop = true;
      this.mpListView1.AllowRowReorder = true;
      this.mpListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.id,
            this.name,
            this.provider,
            this.type,
            this.details});
      this.mpListView1.ContextMenuStrip = this.tuningDetailContextMenu;
      this.mpListView1.FullRowSelect = true;
      this.mpListView1.IsChannelListView = false;
      this.mpListView1.LargeImageList = this.imageList1;
      this.mpListView1.Location = new System.Drawing.Point(6, 19);
      this.mpListView1.Name = "mpListView1";
      this.mpListView1.Size = new System.Drawing.Size(432, 206);
      this.mpListView1.SmallImageList = this.imageList1;
      this.mpListView1.TabIndex = 51;
      this.mpListView1.UseCompatibleStateImageBehavior = false;
      this.mpListView1.View = System.Windows.Forms.View.Details;
      this.mpListView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mpListView1_MouseDoubleClick);
      this.mpListView1.SelectedIndexChanged += new System.EventHandler(this.mpListView1_SelectedIndexChanged);
      // 
      // id
      // 
      this.id.Text = "Id";
      // 
      // name
      // 
      this.name.Text = "Name";
      this.name.Width = 140;
      // 
      // provider
      // 
      this.provider.Text = "Provider";
      this.provider.Width = 123;
      // 
      // type
      // 
      this.type.Text = "Type";
      this.type.Width = 76;
      // 
      // details
      // 
      this.details.Text = "Details";
      // 
      // tuningDetailContextMenu
      // 
      this.tuningDetailContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuButtonAdd,
            this.menuButtonEdit,
            this.menuButtonRemove});
      this.tuningDetailContextMenu.Name = "tuningDetailContextMenu";
      this.tuningDetailContextMenu.Size = new System.Drawing.Size(118, 70);
      // 
      // menuButtonAdd
      // 
      this.menuButtonAdd.Name = "menuButtonAdd";
      this.menuButtonAdd.Size = new System.Drawing.Size(117, 22);
      this.menuButtonAdd.Text = "Add";
      this.menuButtonAdd.Click += new System.EventHandler(this.menuButtonAdd_Click);
      // 
      // menuButtonEdit
      // 
      this.menuButtonEdit.Name = "menuButtonEdit";
      this.menuButtonEdit.Size = new System.Drawing.Size(117, 22);
      this.menuButtonEdit.Text = "Edit";
      this.menuButtonEdit.Click += new System.EventHandler(this.menuButtonEdit_Click);
      // 
      // menuButtonRemove
      // 
      this.menuButtonRemove.Name = "menuButtonRemove";
      this.menuButtonRemove.Size = new System.Drawing.Size(117, 22);
      this.menuButtonRemove.Text = "Remove";
      this.menuButtonRemove.Click += new System.EventHandler(this.menuButtonRemove_Click);
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
      // gbTuningdetails
      // 
      this.gbTuningdetails.Controls.Add(this.btnRemoveTuningDetail);
      this.gbTuningdetails.Controls.Add(this.btnEditTuningDetail);
      this.gbTuningdetails.Controls.Add(this.btnAddTuningDetail);
      this.gbTuningdetails.Controls.Add(this.mpListView1);
      this.gbTuningdetails.Location = new System.Drawing.Point(12, 87);
      this.gbTuningdetails.Name = "gbTuningdetails";
      this.gbTuningdetails.Size = new System.Drawing.Size(440, 267);
      this.gbTuningdetails.TabIndex = 52;
      this.gbTuningdetails.TabStop = false;
      this.gbTuningdetails.Text = "Tuningdetails";
      // 
      // btnRemoveTuningDetail
      // 
      this.btnRemoveTuningDetail.Enabled = false;
      this.btnRemoveTuningDetail.Location = new System.Drawing.Point(168, 231);
      this.btnRemoveTuningDetail.Name = "btnRemoveTuningDetail";
      this.btnRemoveTuningDetail.Size = new System.Drawing.Size(75, 23);
      this.btnRemoveTuningDetail.TabIndex = 54;
      this.btnRemoveTuningDetail.Text = "Remove";
      this.btnRemoveTuningDetail.UseVisualStyleBackColor = true;
      this.btnRemoveTuningDetail.Click += new System.EventHandler(this.menuButtonRemove_Click);
      // 
      // btnEditTuningDetail
      // 
      this.btnEditTuningDetail.Enabled = false;
      this.btnEditTuningDetail.Location = new System.Drawing.Point(87, 231);
      this.btnEditTuningDetail.Name = "btnEditTuningDetail";
      this.btnEditTuningDetail.Size = new System.Drawing.Size(75, 23);
      this.btnEditTuningDetail.TabIndex = 53;
      this.btnEditTuningDetail.Text = "Edit";
      this.btnEditTuningDetail.UseVisualStyleBackColor = true;
      this.btnEditTuningDetail.Click += new System.EventHandler(this.menuButtonEdit_Click);
      // 
      // btnAddTuningDetail
      // 
      this.btnAddTuningDetail.Location = new System.Drawing.Point(6, 231);
      this.btnAddTuningDetail.Name = "btnAddTuningDetail";
      this.btnAddTuningDetail.Size = new System.Drawing.Size(75, 23);
      this.btnAddTuningDetail.TabIndex = 52;
      this.btnAddTuningDetail.Text = "Add";
      this.btnAddTuningDetail.UseVisualStyleBackColor = true;
      this.btnAddTuningDetail.Click += new System.EventHandler(this.menuButtonAdd_Click);
      // 
      // textBoxChannelNumber
      // 
      this.textBoxChannelNumber.Location = new System.Drawing.Point(136, 38);
      this.textBoxChannelNumber.Name = "textBoxChannelNumber";
      this.textBoxChannelNumber.Size = new System.Drawing.Size(100, 20);
      this.textBoxChannelNumber.TabIndex = 53;
      this.textBoxChannelNumber.Text = "4";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(28, 41);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(84, 13);
      this.label1.TabIndex = 54;
      this.label1.Text = "Channel number";
      // 
      // FormEditChannel
      // 
      this.AcceptButton = this.mpButtonOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpButtonCancel;
      this.ClientSize = new System.Drawing.Size(464, 400);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.textBoxChannelNumber);
      this.Controls.Add(this.gbTuningdetails);
      this.Controls.Add(this.textBoxName);
      this.Controls.Add(this.checkBoxVisibleInTvGuide);
      this.Controls.Add(this.label25);
      this.Controls.Add(this.mpButtonCancel);
      this.Controls.Add(this.mpButtonOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormEditChannel";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Edit Channel";
      this.Shown += new System.EventHandler(this.FormEditChannel_Load);
      this.tuningDetailContextMenu.ResumeLayout(false);
      this.gbTuningdetails.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPButton mpButtonOk;
    private System.Windows.Forms.TextBox textBoxName;
    private System.Windows.Forms.Label label25;
    private System.Windows.Forms.CheckBox checkBoxVisibleInTvGuide;
    private MediaPortal.UserInterface.Controls.MPButton mpButtonCancel;
    private MediaPortal.UserInterface.Controls.MPListView mpListView1;
    private System.Windows.Forms.ColumnHeader id;
    private System.Windows.Forms.ColumnHeader name;
    private System.Windows.Forms.ColumnHeader provider;
    private System.Windows.Forms.ColumnHeader type;
    private System.Windows.Forms.ImageList imageList1;
    private MediaPortal.UserInterface.Controls.MPContextMenuStrip tuningDetailContextMenu;
    private System.Windows.Forms.ToolStripMenuItem menuButtonAdd;
    private System.Windows.Forms.ToolStripMenuItem menuButtonRemove;
    private System.Windows.Forms.ToolStripMenuItem menuButtonEdit;
    private System.Windows.Forms.ColumnHeader details;
    private System.Windows.Forms.GroupBox gbTuningdetails;
    private System.Windows.Forms.Button btnRemoveTuningDetail;
    private System.Windows.Forms.Button btnEditTuningDetail;
    private System.Windows.Forms.Button btnAddTuningDetail;
    private System.Windows.Forms.TextBox textBoxChannelNumber;
    private System.Windows.Forms.Label label1;

  }
}