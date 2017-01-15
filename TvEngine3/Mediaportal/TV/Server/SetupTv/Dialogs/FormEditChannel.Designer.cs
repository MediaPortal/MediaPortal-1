using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
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
      this.buttonOkay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.checkBoxVisibleInGuide = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.textBoxName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.labelName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonCancel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.listViewTuningDetails = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderId = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderName = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderNumber = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderProvider = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderType = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderPriority = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.columnHeaderDetails = ((Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader)(new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader()));
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.groupBoxTuningDetails = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelPriority = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonTuningDetailPriorityUp = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonTuningDetailPriorityDown = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonTuningDetailDelete = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonTuningDetailEdit = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonTuningDetailAdd = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelIdValue = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelExternalId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.textBoxExternalId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.tableLayoutPanel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTableLayoutPanel();
      this.channelNumberUpDownNumber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPChannelNumberUpDown();
      this.groupBoxTuningDetails.SuspendLayout();
      this.tableLayoutPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).BeginInit();
      this.SuspendLayout();
      // 
      // buttonOkay
      // 
      this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOkay.Location = new System.Drawing.Point(346, 289);
      this.buttonOkay.Name = "buttonOkay";
      this.buttonOkay.Size = new System.Drawing.Size(75, 23);
      this.buttonOkay.TabIndex = 2;
      this.buttonOkay.Text = "&OK";
      this.buttonOkay.Click += new System.EventHandler(this.buttonOkay_Click);
      // 
      // checkBoxVisibleInGuide
      // 
      this.checkBoxVisibleInGuide.AutoSize = true;
      this.tableLayoutPanel.SetColumnSpan(this.checkBoxVisibleInGuide, 2);
      this.checkBoxVisibleInGuide.Location = new System.Drawing.Point(266, 55);
      this.checkBoxVisibleInGuide.Name = "checkBoxVisibleInGuide";
      this.checkBoxVisibleInGuide.Size = new System.Drawing.Size(172, 17);
      this.checkBoxVisibleInGuide.TabIndex = 8;
      this.checkBoxVisibleInGuide.Text = "Show this channel in the guide.";
      this.checkBoxVisibleInGuide.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // textBoxName
      // 
      this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxName.Location = new System.Drawing.Point(56, 29);
      this.textBoxName.MaxLength = 200;
      this.textBoxName.Name = "textBoxName";
      this.textBoxName.Size = new System.Drawing.Size(184, 20);
      this.textBoxName.TabIndex = 3;
      // 
      // labelName
      // 
      this.labelName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.labelName.AutoSize = true;
      this.labelName.Location = new System.Drawing.Point(3, 26);
      this.labelName.Name = "labelName";
      this.labelName.Size = new System.Drawing.Size(38, 26);
      this.labelName.TabIndex = 2;
      this.labelName.Text = "Name:";
      this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(453, 289);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 3;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // listViewTuningDetails
      // 
      this.listViewTuningDetails.AllowColumnReorder = true;
      this.listViewTuningDetails.AllowDrop = true;
      this.listViewTuningDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewTuningDetails.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderId,
            this.columnHeaderName,
            this.columnHeaderNumber,
            this.columnHeaderProvider,
            this.columnHeaderType,
            this.columnHeaderPriority,
            this.columnHeaderDetails});
      this.listViewTuningDetails.FullRowSelect = true;
      this.listViewTuningDetails.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.listViewTuningDetails.LargeImageList = this.imageList1;
      this.listViewTuningDetails.Location = new System.Drawing.Point(6, 19);
      this.listViewTuningDetails.Name = "listViewTuningDetails";
      this.listViewTuningDetails.Size = new System.Drawing.Size(510, 132);
      this.listViewTuningDetails.SmallImageList = this.imageList1;
      this.listViewTuningDetails.TabIndex = 0;
      this.listViewTuningDetails.UseCompatibleStateImageBehavior = false;
      this.listViewTuningDetails.View = System.Windows.Forms.View.Details;
      this.listViewTuningDetails.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listViewTuningDetails_ItemDrag);
      this.listViewTuningDetails.SelectedIndexChanged += new System.EventHandler(this.listViewTuningDetails_SelectedIndexChanged);
      this.listViewTuningDetails.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewTuningDetails_DragDrop);
      this.listViewTuningDetails.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewTuningDetails_DragEnter);
      this.listViewTuningDetails.DragOver += new System.Windows.Forms.DragEventHandler(this.listViewTuningDetails_DragOver);
      this.listViewTuningDetails.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewTuningDetails_KeyDown);
      this.listViewTuningDetails.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewTuningDetails_MouseDoubleClick);
      // 
      // columnHeaderId
      // 
      this.columnHeaderId.Text = "ID";
      this.columnHeaderId.Width = 40;
      // 
      // columnHeaderName
      // 
      this.columnHeaderName.Text = "Name";
      this.columnHeaderName.Width = 80;
      // 
      // columnHeaderNumber
      // 
      this.columnHeaderNumber.Text = "#";
      this.columnHeaderNumber.Width = 50;
      // 
      // columnHeaderProvider
      // 
      this.columnHeaderProvider.Text = "Provider";
      this.columnHeaderProvider.Width = 80;
      // 
      // columnHeaderType
      // 
      this.columnHeaderType.Text = "Type";
      this.columnHeaderType.Width = 50;
      // 
      // columnHeaderPriority
      // 
      this.columnHeaderPriority.Text = "Priority";
      this.columnHeaderPriority.Width = 50;
      // 
      // columnHeaderDetails
      // 
      this.columnHeaderDetails.Text = "Details";
      this.columnHeaderDetails.Width = 125;
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "tv_fta_.png");
      this.imageList1.Images.SetKeyName(1, "tv_scrambled.png");
      this.imageList1.Images.SetKeyName(2, "radio_fta_.png");
      this.imageList1.Images.SetKeyName(3, "radio_scrambled.png");
      // 
      // groupBoxTuningDetails
      // 
      this.groupBoxTuningDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxTuningDetails.Controls.Add(this.labelPriority);
      this.groupBoxTuningDetails.Controls.Add(this.buttonTuningDetailPriorityUp);
      this.groupBoxTuningDetails.Controls.Add(this.buttonTuningDetailPriorityDown);
      this.groupBoxTuningDetails.Controls.Add(this.buttonTuningDetailDelete);
      this.groupBoxTuningDetails.Controls.Add(this.buttonTuningDetailEdit);
      this.groupBoxTuningDetails.Controls.Add(this.buttonTuningDetailAdd);
      this.groupBoxTuningDetails.Controls.Add(this.listViewTuningDetails);
      this.groupBoxTuningDetails.Location = new System.Drawing.Point(12, 85);
      this.groupBoxTuningDetails.Name = "groupBoxTuningDetails";
      this.groupBoxTuningDetails.Size = new System.Drawing.Size(522, 189);
      this.groupBoxTuningDetails.TabIndex = 1;
      this.groupBoxTuningDetails.TabStop = false;
      this.groupBoxTuningDetails.Text = "Tuning Details";
      // 
      // labelPriority
      // 
      this.labelPriority.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.labelPriority.AutoSize = true;
      this.labelPriority.Location = new System.Drawing.Point(402, 162);
      this.labelPriority.Name = "labelPriority";
      this.labelPriority.Size = new System.Drawing.Size(41, 13);
      this.labelPriority.TabIndex = 4;
      this.labelPriority.Text = "Priority:";
      // 
      // buttonTuningDetailPriorityUp
      // 
      this.buttonTuningDetailPriorityUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonTuningDetailPriorityUp.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_up;
      this.buttonTuningDetailPriorityUp.Location = new System.Drawing.Point(450, 157);
      this.buttonTuningDetailPriorityUp.Name = "buttonTuningDetailPriorityUp";
      this.buttonTuningDetailPriorityUp.Size = new System.Drawing.Size(30, 23);
      this.buttonTuningDetailPriorityUp.TabIndex = 5;
      this.buttonTuningDetailPriorityUp.Click += new System.EventHandler(this.buttonTuningDetailPriorityUp_Click);
      // 
      // buttonTuningDetailPriorityDown
      // 
      this.buttonTuningDetailPriorityDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonTuningDetailPriorityDown.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_down;
      this.buttonTuningDetailPriorityDown.Location = new System.Drawing.Point(486, 157);
      this.buttonTuningDetailPriorityDown.Name = "buttonTuningDetailPriorityDown";
      this.buttonTuningDetailPriorityDown.Size = new System.Drawing.Size(30, 23);
      this.buttonTuningDetailPriorityDown.TabIndex = 6;
      this.buttonTuningDetailPriorityDown.Click += new System.EventHandler(this.buttonTuningDetailPriorityDown_Click);
      // 
      // buttonTuningDetailDelete
      // 
      this.buttonTuningDetailDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonTuningDetailDelete.Enabled = false;
      this.buttonTuningDetailDelete.Location = new System.Drawing.Point(128, 157);
      this.buttonTuningDetailDelete.Name = "buttonTuningDetailDelete";
      this.buttonTuningDetailDelete.Size = new System.Drawing.Size(55, 23);
      this.buttonTuningDetailDelete.TabIndex = 3;
      this.buttonTuningDetailDelete.Text = "&Delete";
      this.buttonTuningDetailDelete.Click += new System.EventHandler(this.buttonTuningDetailDelete_Click);
      // 
      // buttonTuningDetailEdit
      // 
      this.buttonTuningDetailEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonTuningDetailEdit.Enabled = false;
      this.buttonTuningDetailEdit.Location = new System.Drawing.Point(67, 157);
      this.buttonTuningDetailEdit.Name = "buttonTuningDetailEdit";
      this.buttonTuningDetailEdit.Size = new System.Drawing.Size(55, 23);
      this.buttonTuningDetailEdit.TabIndex = 2;
      this.buttonTuningDetailEdit.Text = "&Edit";
      this.buttonTuningDetailEdit.Click += new System.EventHandler(this.buttonTuningDetailEdit_Click);
      // 
      // buttonTuningDetailAdd
      // 
      this.buttonTuningDetailAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonTuningDetailAdd.Location = new System.Drawing.Point(6, 157);
      this.buttonTuningDetailAdd.Name = "buttonTuningDetailAdd";
      this.buttonTuningDetailAdd.Size = new System.Drawing.Size(55, 23);
      this.buttonTuningDetailAdd.TabIndex = 1;
      this.buttonTuningDetailAdd.Text = "&Add";
      this.buttonTuningDetailAdd.Click += new System.EventHandler(this.buttonTuningDetailAdd_Click);
      // 
      // labelNumber
      // 
      this.labelNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.labelNumber.AutoSize = true;
      this.labelNumber.Location = new System.Drawing.Point(3, 52);
      this.labelNumber.Name = "labelNumber";
      this.labelNumber.Size = new System.Drawing.Size(47, 26);
      this.labelNumber.TabIndex = 4;
      this.labelNumber.Text = "Number:";
      this.labelNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelId
      // 
      this.labelId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.labelId.AutoSize = true;
      this.labelId.Location = new System.Drawing.Point(3, 0);
      this.labelId.Name = "labelId";
      this.labelId.Size = new System.Drawing.Size(21, 26);
      this.labelId.TabIndex = 0;
      this.labelId.Text = "ID:";
      this.labelId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelIdValue
      // 
      this.labelIdValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.labelIdValue.AutoSize = true;
      this.labelIdValue.Location = new System.Drawing.Point(56, 0);
      this.labelIdValue.Name = "labelIdValue";
      this.labelIdValue.Size = new System.Drawing.Size(43, 26);
      this.labelIdValue.TabIndex = 1;
      this.labelIdValue.Text = "123456";
      this.labelIdValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelExternalId
      // 
      this.labelExternalId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.labelExternalId.AutoSize = true;
      this.labelExternalId.Location = new System.Drawing.Point(266, 26);
      this.labelExternalId.Name = "labelExternalId";
      this.labelExternalId.Size = new System.Drawing.Size(62, 26);
      this.labelExternalId.TabIndex = 6;
      this.labelExternalId.Text = "External ID:";
      this.labelExternalId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textBoxExternalId
      // 
      this.textBoxExternalId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxExternalId.Location = new System.Drawing.Point(334, 29);
      this.textBoxExternalId.MaxLength = 200;
      this.textBoxExternalId.Name = "textBoxExternalId";
      this.textBoxExternalId.Size = new System.Drawing.Size(185, 20);
      this.textBoxExternalId.TabIndex = 7;
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel.ColumnCount = 5;
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel.Controls.Add(this.labelExternalId, 3, 1);
      this.tableLayoutPanel.Controls.Add(this.labelNumber, 0, 2);
      this.tableLayoutPanel.Controls.Add(this.textBoxExternalId, 4, 1);
      this.tableLayoutPanel.Controls.Add(this.labelId, 0, 0);
      this.tableLayoutPanel.Controls.Add(this.labelIdValue, 1, 0);
      this.tableLayoutPanel.Controls.Add(this.labelName, 0, 1);
      this.tableLayoutPanel.Controls.Add(this.textBoxName, 1, 1);
      this.tableLayoutPanel.Controls.Add(this.checkBoxVisibleInGuide, 3, 2);
      this.tableLayoutPanel.Controls.Add(this.channelNumberUpDownNumber, 1, 2);
      this.tableLayoutPanel.Location = new System.Drawing.Point(12, 1);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.RowCount = 3;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.tableLayoutPanel.Size = new System.Drawing.Size(522, 78);
      this.tableLayoutPanel.TabIndex = 0;
      // 
      // channelNumberUpDownNumber
      // 
      this.channelNumberUpDownNumber.DecimalPlaces = 3;
      this.channelNumberUpDownNumber.Location = new System.Drawing.Point(56, 55);
      this.channelNumberUpDownNumber.Maximum = new decimal(new int[] {
            65535999,
            0,
            0,
            196608});
      this.channelNumberUpDownNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.channelNumberUpDownNumber.Name = "channelNumberUpDownNumber";
      this.channelNumberUpDownNumber.Size = new System.Drawing.Size(75, 20);
      this.channelNumberUpDownNumber.TabIndex = 4;
      this.channelNumberUpDownNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // FormEditChannel
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(546, 327);
      this.Controls.Add(this.tableLayoutPanel);
      this.Controls.Add(this.groupBoxTuningDetails);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOkay);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(400, 300);
      this.Name = "FormEditChannel";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Add/Edit Channel";
      this.Shown += new System.EventHandler(this.FormEditChannel_Load);
      this.groupBoxTuningDetails.ResumeLayout(false);
      this.groupBoxTuningDetails.PerformLayout();
      this.tableLayoutPanel.ResumeLayout(false);
      this.tableLayoutPanel.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownNumber)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MPButton buttonOkay;
    private MPTextBox textBoxName;
    private MPLabel labelName;
    private MPCheckBox checkBoxVisibleInGuide;
    private MPButton buttonCancel;
    private MPListView listViewTuningDetails;
    private MPColumnHeader columnHeaderId;
    private MPColumnHeader columnHeaderName;
    private MPColumnHeader columnHeaderProvider;
    private MPColumnHeader columnHeaderType;
    private System.Windows.Forms.ImageList imageList1;
    private MPColumnHeader columnHeaderDetails;
    private MPGroupBox groupBoxTuningDetails;
    private MPButton buttonTuningDetailDelete;
    private MPButton buttonTuningDetailEdit;
    private MPButton buttonTuningDetailAdd;
    private MPLabel labelNumber;
    private MPColumnHeader columnHeaderNumber;
    private MPLabel labelId;
    private MPLabel labelIdValue;
    private MPLabel labelExternalId;
    private MPTextBox textBoxExternalId;
    private MPButton buttonTuningDetailPriorityUp;
    private MPButton buttonTuningDetailPriorityDown;
    private MPLabel labelPriority;
    private MPTableLayoutPanel tableLayoutPanel;
    private MPChannelNumberUpDown channelNumberUpDownNumber;
    private MPColumnHeader columnHeaderPriority;
  }
}