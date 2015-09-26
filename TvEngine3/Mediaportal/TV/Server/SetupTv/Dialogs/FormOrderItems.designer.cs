using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  partial class FormOrderItems
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
      this.labelItems = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonOkay = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonCancel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.listViewItems = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderItem = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.buttonOrderDown = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonOrderUp = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonOrderByName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.labelOrderManual = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelOrderByName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.SuspendLayout();
      // 
      // labelItems
      // 
      this.labelItems.AutoSize = true;
      this.labelItems.Location = new System.Drawing.Point(12, 9);
      this.labelItems.Name = "labelItems";
      this.labelItems.Size = new System.Drawing.Size(161, 13);
      this.labelItems.TabIndex = 0;
      this.labelItems.Text = "Please set the order of the items:";
      // 
      // buttonOkay
      // 
      this.buttonOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonOkay.Location = new System.Drawing.Point(15, 195);
      this.buttonOkay.Name = "buttonOkay";
      this.buttonOkay.Size = new System.Drawing.Size(75, 23);
      this.buttonOkay.TabIndex = 7;
      this.buttonOkay.Text = "&OK";
      this.buttonOkay.UseVisualStyleBackColor = true;
      this.buttonOkay.Click += new System.EventHandler(this.buttonOkay_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(182, 195);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 8;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // listViewItems
      // 
      this.listViewItems.AllowDrop = true;
      this.listViewItems.AllowRowReorder = true;
      this.listViewItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderItem});
      this.listViewItems.FullRowSelect = true;
      this.listViewItems.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.listViewItems.Location = new System.Drawing.Point(15, 25);
      this.listViewItems.Name = "listViewItems";
      this.listViewItems.Size = new System.Drawing.Size(183, 164);
      this.listViewItems.TabIndex = 1;
      this.listViewItems.UseCompatibleStateImageBehavior = false;
      this.listViewItems.View = System.Windows.Forms.View.Details;
      this.listViewItems.SelectedIndexChanged += new System.EventHandler(this.listViewItems_SelectedIndexChanged);
      this.listViewItems.SizeChanged += new System.EventHandler(this.listViewItems_SizeChanged);
      // 
      // columnHeaderItem
      // 
      this.columnHeaderItem.Text = "";
      this.columnHeaderItem.Width = 179;
      // 
      // buttonOrderDown
      // 
      this.buttonOrderDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOrderDown.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_down;
      this.buttonOrderDown.Location = new System.Drawing.Point(227, 126);
      this.buttonOrderDown.Name = "buttonOrderDown";
      this.buttonOrderDown.Size = new System.Drawing.Size(30, 23);
      this.buttonOrderDown.TabIndex = 6;
      this.buttonOrderDown.UseVisualStyleBackColor = true;
      this.buttonOrderDown.Click += new System.EventHandler(this.buttonOrderDown_Click);
      // 
      // buttonOrderUp
      // 
      this.buttonOrderUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOrderUp.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_up;
      this.buttonOrderUp.Location = new System.Drawing.Point(227, 101);
      this.buttonOrderUp.Name = "buttonOrderUp";
      this.buttonOrderUp.Size = new System.Drawing.Size(30, 23);
      this.buttonOrderUp.TabIndex = 5;
      this.buttonOrderUp.UseVisualStyleBackColor = true;
      this.buttonOrderUp.Click += new System.EventHandler(this.buttonOrderUp_Click);
      // 
      // buttonOrderByName
      // 
      this.buttonOrderByName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOrderByName.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_none;
      this.buttonOrderByName.Location = new System.Drawing.Point(227, 55);
      this.buttonOrderByName.Name = "buttonOrderByName";
      this.buttonOrderByName.Size = new System.Drawing.Size(30, 23);
      this.buttonOrderByName.TabIndex = 3;
      this.buttonOrderByName.UseVisualStyleBackColor = true;
      this.buttonOrderByName.Click += new System.EventHandler(this.buttonOrderByName_Click);
      // 
      // labelOrderManual
      // 
      this.labelOrderManual.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelOrderManual.AutoSize = true;
      this.labelOrderManual.Location = new System.Drawing.Point(204, 85);
      this.labelOrderManual.Name = "labelOrderManual";
      this.labelOrderManual.Size = new System.Drawing.Size(45, 13);
      this.labelOrderManual.TabIndex = 4;
      this.labelOrderManual.Text = "Manual:";
      // 
      // labelOrderByName
      // 
      this.labelOrderByName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelOrderByName.AutoSize = true;
      this.labelOrderByName.Location = new System.Drawing.Point(204, 39);
      this.labelOrderByName.Name = "labelOrderByName";
      this.labelOrderByName.Size = new System.Drawing.Size(53, 13);
      this.labelOrderByName.TabIndex = 2;
      this.labelOrderByName.Text = "By Name:";
      // 
      // FormOrderItems
      // 
      this.AcceptButton = this.buttonOkay;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(269, 230);
      this.Controls.Add(this.labelOrderByName);
      this.Controls.Add(this.labelOrderManual);
      this.Controls.Add(this.buttonOrderByName);
      this.Controls.Add(this.buttonOrderDown);
      this.Controls.Add(this.buttonOrderUp);
      this.Controls.Add(this.listViewItems);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOkay);
      this.Controls.Add(this.labelItems);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(200, 230);
      this.Name = "FormOrderItems";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Order Items";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MPLabel labelItems;
    private MPButton buttonOkay;
    private MPButton buttonCancel;
    private MPListView listViewItems;
    private MPColumnHeader columnHeaderItem;
    private MPButton buttonOrderDown;
    private MPButton buttonOrderUp;
    private MPButton buttonOrderByName;
    private MPLabel labelOrderManual;
    private MPLabel labelOrderByName;
  }
}