using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class TunerSatellites
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TunerSatellites));
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpLabel2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpListViewChannels = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.mpLabel1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpComboBoxCard = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.mpLabelChannelCount = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.mpGroupBox1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.mpButtonAdd = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.mpButtonEdit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.mpButtonDel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.mpGroupBox2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.mpComboBoxSat = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.mpLabel3 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "radio_fta_.png");
      this.imageList1.Images.SetKeyName(1, "radio_scrambled.png");
      this.imageList1.Images.SetKeyName(2, "icon.radio_scrambled_and_fta.png");
      this.imageList1.Images.SetKeyName(3, "tv_fta_.png");
      this.imageList1.Images.SetKeyName(4, "tv_scrambled.png");
      this.imageList1.Images.SetKeyName(5, "icon.tv_scrambled_and_fta.png");
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Location = new System.Drawing.Point(3, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(454, 458);
      this.tabControl1.TabIndex = 18;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpGroupBox2);
      this.tabPage1.Controls.Add(this.mpGroupBox1);
      this.tabPage1.Controls.Add(this.mpLabel2);
      this.tabPage1.Controls.Add(this.mpListViewChannels);
      this.tabPage1.Controls.Add(this.mpLabelChannelCount);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(446, 432);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Tuner Satellites";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(3, 135);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(129, 13);
      this.mpLabel2.TabIndex = 21;
      this.mpLabel2.Text = "Available Tuner Satellites:";
      // 
      // mpListViewChannels
      // 
      this.mpListViewChannels.AllowDrop = true;
      this.mpListViewChannels.AllowRowReorder = false;
      this.mpListViewChannels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.mpListViewChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader5});
      this.mpListViewChannels.IsChannelListView = false;
      this.mpListViewChannels.LargeImageList = this.imageList1;
      this.mpListViewChannels.Location = new System.Drawing.Point(6, 153);
      this.mpListViewChannels.Name = "mpListViewChannels";
      this.mpListViewChannels.Size = new System.Drawing.Size(437, 219);
      this.mpListViewChannels.SmallImageList = this.imageList1;
      this.mpListViewChannels.TabIndex = 20;
      this.mpListViewChannels.UseCompatibleStateImageBehavior = false;
      this.mpListViewChannels.View = System.Windows.Forms.View.Details;
      
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Card";
      this.columnHeader4.Width = 73;
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(6, 22);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(32, 13);
      this.mpLabel1.TabIndex = 19;
      this.mpLabel1.Text = "Card:";
      // 
      // mpComboBoxCard
      // 
      this.mpComboBoxCard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mpComboBoxCard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxCard.FormattingEnabled = true;
      this.mpComboBoxCard.Location = new System.Drawing.Point(59, 19);
      this.mpComboBoxCard.Name = "mpComboBoxCard";
      this.mpComboBoxCard.Size = new System.Drawing.Size(369, 21);
      this.mpComboBoxCard.TabIndex = 18;
      
      // 
      // mpLabelChannelCount
      // 
      this.mpLabelChannelCount.AutoSize = true;
      this.mpLabelChannelCount.Location = new System.Drawing.Point(13, 14);
      this.mpLabelChannelCount.Name = "mpLabelChannelCount";
      this.mpLabelChannelCount.Size = new System.Drawing.Size(0, 13);
      this.mpLabelChannelCount.TabIndex = 2;
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpGroupBox1.Controls.Add(this.mpButtonAdd);
      this.mpGroupBox1.Controls.Add(this.mpButtonEdit);
      this.mpGroupBox1.Controls.Add(this.mpButtonDel);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(6, 379);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(214, 47);
      this.mpGroupBox1.TabIndex = 26;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Tuner Satellite";
      // 
      // mpButtonAdd
      // 
      this.mpButtonAdd.Location = new System.Drawing.Point(10, 13);
      this.mpButtonAdd.Name = "mpButtonAdd";
      this.mpButtonAdd.Size = new System.Drawing.Size(60, 23);
      this.mpButtonAdd.TabIndex = 21;
      this.mpButtonAdd.Text = "&Add";
      this.mpButtonAdd.UseVisualStyleBackColor = true;
      // 
      // mpButtonEdit
      // 
      this.mpButtonEdit.Location = new System.Drawing.Point(74, 13);
      this.mpButtonEdit.Name = "mpButtonEdit";
      this.mpButtonEdit.Size = new System.Drawing.Size(60, 23);
      this.mpButtonEdit.TabIndex = 22;
      this.mpButtonEdit.Text = "&Edit";
      this.mpButtonEdit.UseVisualStyleBackColor = true;
      // 
      // mpButtonDel
      // 
      this.mpButtonDel.Location = new System.Drawing.Point(140, 13);
      this.mpButtonDel.Name = "mpButtonDel";
      this.mpButtonDel.Size = new System.Drawing.Size(60, 23);
      this.mpButtonDel.TabIndex = 23;
      this.mpButtonDel.Text = "&Delete";
      this.mpButtonDel.UseVisualStyleBackColor = true;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Satellite";
      this.columnHeader1.Width = 138;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "LNB";
      this.columnHeader2.Width = 90;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "DiseqC Sw.";
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Motor pos.";
      this.columnHeader5.Width = 70;
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpGroupBox2.Controls.Add(this.mpComboBoxSat);
      this.mpGroupBox2.Controls.Add(this.mpLabel3);
      this.mpGroupBox2.Controls.Add(this.mpComboBoxCard);
      this.mpGroupBox2.Controls.Add(this.mpLabel1);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(6, 6);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(434, 87);
      this.mpGroupBox2.TabIndex = 27;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Filter";
      // 
      // mpComboBoxSat
      // 
      this.mpComboBoxSat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mpComboBoxSat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxSat.FormattingEnabled = true;
      this.mpComboBoxSat.Location = new System.Drawing.Point(59, 53);
      this.mpComboBoxSat.Name = "mpComboBoxSat";
      this.mpComboBoxSat.Size = new System.Drawing.Size(369, 21);
      this.mpComboBoxSat.TabIndex = 20;
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(6, 56);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(47, 13);
      this.mpLabel3.TabIndex = 21;
      this.mpLabel3.Text = "Satellite:";
      // 
      // TunerSatellites
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "TunerSatellites";
      this.Size = new System.Drawing.Size(457, 461);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
    private MPLabel mpLabelChannelCount;
		private MPLabel mpLabel2;
		private MPListView mpListViewChannels;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private MPLabel mpLabel1;
    private MPComboBox mpComboBoxCard;
    private MPGroupBox mpGroupBox1;
    private MPButton mpButtonAdd;
    private MPButton mpButtonEdit;
    private MPButton mpButtonDel;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private MPGroupBox mpGroupBox2;
    private MPComboBox mpComboBoxSat;
    private MPLabel mpLabel3;
  }
}
