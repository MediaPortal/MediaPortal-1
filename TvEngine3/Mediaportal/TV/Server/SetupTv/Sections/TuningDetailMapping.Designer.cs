using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class TuningDetailMapping
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TuningDetailMapping));
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.pictureBoxTuner = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPPictureBox();
      this.buttonUnmap = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonMapAll = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelMapped = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.listViewMapped = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeader2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.labelNotMapped = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.listViewNotMapped = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeader4 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.labelTuner = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxTuner = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.tableLayoutPanel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTableLayoutPanel();
      this.buttonUnmapAll = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonMap = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTuner)).BeginInit();
      this.tableLayoutPanel.SuspendLayout();
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
      // pictureBoxTuner
      // 
      this.pictureBoxTuner.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxTuner.Image")));
      this.pictureBoxTuner.Location = new System.Drawing.Point(9, 6);
      this.pictureBoxTuner.Name = "pictureBoxTuner";
      this.pictureBoxTuner.Size = new System.Drawing.Size(33, 23);
      this.pictureBoxTuner.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pictureBoxTuner.TabIndex = 35;
      this.pictureBoxTuner.TabStop = false;
      // 
      // buttonUnmap
      // 
      this.buttonUnmap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonUnmap.Location = new System.Drawing.Point(226, 75);
      this.buttonUnmap.Name = "buttonUnmap";
      this.buttonUnmap.Size = new System.Drawing.Size(27, 23);
      this.buttonUnmap.TabIndex = 5;
      this.buttonUnmap.Text = "<";
      this.buttonUnmap.UseVisualStyleBackColor = true;
      this.buttonUnmap.Click += new System.EventHandler(this.buttonUnmap_Click);
      // 
      // buttonMapAll
      // 
      this.buttonMapAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonMapAll.Location = new System.Drawing.Point(226, 125);
      this.buttonMapAll.Name = "buttonMapAll";
      this.buttonMapAll.Size = new System.Drawing.Size(27, 23);
      this.buttonMapAll.TabIndex = 4;
      this.buttonMapAll.Text = ">>";
      this.buttonMapAll.UseVisualStyleBackColor = true;
      this.buttonMapAll.Click += new System.EventHandler(this.buttonMapAll_Click);
      // 
      // labelMapped
      // 
      this.labelMapped.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.labelMapped.AutoSize = true;
      this.labelMapped.Location = new System.Drawing.Point(259, 5);
      this.labelMapped.Name = "labelMapped";
      this.labelMapped.Size = new System.Drawing.Size(134, 13);
      this.labelMapped.TabIndex = 6;
      this.labelMapped.Text = "Channels mapped to tuner:";
      // 
      // listViewMapped
      // 
      this.listViewMapped.AllowDrop = true;
      this.listViewMapped.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewMapped.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
      this.listViewMapped.LargeImageList = this.imageList1;
      this.listViewMapped.Location = new System.Drawing.Point(259, 21);
      this.listViewMapped.Name = "listViewMapped";
      this.tableLayoutPanel.SetRowSpan(this.listViewMapped, 5);
      this.listViewMapped.Size = new System.Drawing.Size(213, 356);
      this.listViewMapped.SmallImageList = this.imageList1;
      this.listViewMapped.TabIndex = 7;
      this.listViewMapped.UseCompatibleStateImageBehavior = false;
      this.listViewMapped.View = System.Windows.Forms.View.Details;
      this.listViewMapped.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewMapped_ColumnClick);
      this.listViewMapped.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listViewMapped_ItemDrag);
      this.listViewMapped.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewMapped_DragDrop);
      this.listViewMapped.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewMapped_DragEnter);
      this.listViewMapped.DragOver += new System.Windows.Forms.DragEventHandler(this.listViewMapped_DragOver);
      this.listViewMapped.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewMapped_MouseDoubleClick);
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Name";
      this.columnHeader2.Width = 180;
      // 
      // labelNotMapped
      // 
      this.labelNotMapped.AutoSize = true;
      this.labelNotMapped.Location = new System.Drawing.Point(8, 5);
      this.labelNotMapped.Name = "labelNotMapped";
      this.labelNotMapped.Size = new System.Drawing.Size(152, 13);
      this.labelNotMapped.TabIndex = 2;
      this.labelNotMapped.Text = "Channels not mapped to tuner:";
      // 
      // listViewNotMapped
      // 
      this.listViewNotMapped.AllowDrop = true;
      this.listViewNotMapped.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewNotMapped.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
      this.listViewNotMapped.LargeImageList = this.imageList1;
      this.listViewNotMapped.Location = new System.Drawing.Point(8, 21);
      this.listViewNotMapped.Name = "listViewNotMapped";
      this.tableLayoutPanel.SetRowSpan(this.listViewNotMapped, 5);
      this.listViewNotMapped.Size = new System.Drawing.Size(212, 356);
      this.listViewNotMapped.SmallImageList = this.imageList1;
      this.listViewNotMapped.TabIndex = 3;
      this.listViewNotMapped.UseCompatibleStateImageBehavior = false;
      this.listViewNotMapped.View = System.Windows.Forms.View.Details;
      this.listViewNotMapped.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewNotMapped_ColumnClick);
      this.listViewNotMapped.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listViewNotMapped_ItemDrag);
      this.listViewNotMapped.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewNotMapped_DragDrop);
      this.listViewNotMapped.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewNotMapped_DragEnter);
      this.listViewNotMapped.DragOver += new System.Windows.Forms.DragEventHandler(this.listViewNotMapped_DragOver);
      this.listViewNotMapped.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewNotMapped_MouseDoubleClick);
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Name";
      this.columnHeader4.Width = 180;
      // 
      // labelTuner
      // 
      this.labelTuner.AutoSize = true;
      this.labelTuner.Location = new System.Drawing.Point(48, 11);
      this.labelTuner.Name = "labelTuner";
      this.labelTuner.Size = new System.Drawing.Size(38, 13);
      this.labelTuner.TabIndex = 0;
      this.labelTuner.Text = "Tuner:";
      // 
      // comboBoxTuner
      // 
      this.comboBoxTuner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxTuner.FormattingEnabled = true;
      this.comboBoxTuner.Location = new System.Drawing.Point(92, 8);
      this.comboBoxTuner.Name = "comboBoxTuner";
      this.comboBoxTuner.Size = new System.Drawing.Size(380, 21);
      this.comboBoxTuner.TabIndex = 1;
      this.comboBoxTuner.SelectedIndexChanged += new System.EventHandler(this.comboBoxTuner_SelectedIndexChanged);
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel.ColumnCount = 3;
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel.Controls.Add(this.buttonUnmapAll, 1, 5);
      this.tableLayoutPanel.Controls.Add(this.listViewNotMapped, 0, 1);
      this.tableLayoutPanel.Controls.Add(this.labelNotMapped, 0, 0);
      this.tableLayoutPanel.Controls.Add(this.labelMapped, 2, 0);
      this.tableLayoutPanel.Controls.Add(this.listViewMapped, 2, 1);
      this.tableLayoutPanel.Controls.Add(this.buttonMapAll, 1, 4);
      this.tableLayoutPanel.Controls.Add(this.buttonMap, 1, 2);
      this.tableLayoutPanel.Controls.Add(this.buttonUnmap, 1, 3);
      this.tableLayoutPanel.Location = new System.Drawing.Point(0, 35);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.Padding = new System.Windows.Forms.Padding(5);
      this.tableLayoutPanel.RowCount = 6;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel.Size = new System.Drawing.Size(480, 385);
      this.tableLayoutPanel.TabIndex = 36;
      // 
      // buttonUnmapAll
      // 
      this.buttonUnmapAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonUnmapAll.Location = new System.Drawing.Point(226, 156);
      this.buttonUnmapAll.Name = "buttonUnmapAll";
      this.buttonUnmapAll.Size = new System.Drawing.Size(27, 23);
      this.buttonUnmapAll.TabIndex = 9;
      this.buttonUnmapAll.Text = "<<";
      this.buttonUnmapAll.UseVisualStyleBackColor = true;
      this.buttonUnmapAll.Click += new System.EventHandler(this.buttonUnmapAll_Click);
      // 
      // buttonMap
      // 
      this.buttonMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonMap.Location = new System.Drawing.Point(226, 44);
      this.buttonMap.Name = "buttonMap";
      this.buttonMap.Size = new System.Drawing.Size(27, 23);
      this.buttonMap.TabIndex = 8;
      this.buttonMap.Text = ">";
      this.buttonMap.UseVisualStyleBackColor = true;
      this.buttonMap.Click += new System.EventHandler(this.buttonMap_Click);
      // 
      // ChannelMapping
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.tableLayoutPanel);
      this.Controls.Add(this.pictureBoxTuner);
      this.Controls.Add(this.labelTuner);
      this.Controls.Add(this.comboBoxTuner);
      this.Name = "ChannelMapping";
      this.Size = new System.Drawing.Size(480, 420);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTuner)).EndInit();
      this.tableLayoutPanel.ResumeLayout(false);
      this.tableLayoutPanel.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ImageList imageList1;
    private MPPictureBox pictureBoxTuner;
    private MPButton buttonUnmap;
    private MPButton buttonMapAll;
    private MPLabel labelMapped;
    private MPListView listViewMapped;
    private MPColumnHeader columnHeader2;
    private MPLabel labelNotMapped;
    private MPListView listViewNotMapped;
    private MPColumnHeader columnHeader4;
    private MPLabel labelTuner;
    private MPComboBox comboBoxTuner;
    private MPTableLayoutPanel tableLayoutPanel;
    private MPButton buttonMap;
    private MPButton buttonUnmapAll;
  }
}
