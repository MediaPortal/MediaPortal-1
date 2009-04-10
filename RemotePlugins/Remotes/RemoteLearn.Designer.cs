namespace MediaPortal.InputDevices
{
  partial class RemoteLearn
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
      this.ButtonStartLearn = new MediaPortal.UserInterface.Controls.MPButton();
      this.ButtonEndLearn = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpBeveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.mpApply = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeaderMPAction = new System.Windows.Forms.ColumnHeader();
      this.columnHeaderX10Key = new System.Windows.Forms.ColumnHeader();
      this.InputMapperButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpRemotenumber = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.SuspendLayout();
      // 
      // ButtonStartLearn
      // 
      this.ButtonStartLearn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.ButtonStartLearn.Location = new System.Drawing.Point(325, 22);
      this.ButtonStartLearn.Name = "ButtonStartLearn";
      this.ButtonStartLearn.Size = new System.Drawing.Size(84, 23);
      this.ButtonStartLearn.TabIndex = 6;
      this.ButtonStartLearn.Text = "Start Learning";
      this.ButtonStartLearn.UseVisualStyleBackColor = true;
      this.ButtonStartLearn.Click += new System.EventHandler(this.ButtonStartLearn_Click);
      // 
      // ButtonEndLearn
      // 
      this.ButtonEndLearn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.ButtonEndLearn.Location = new System.Drawing.Point(325, 68);
      this.ButtonEndLearn.Name = "ButtonEndLearn";
      this.ButtonEndLearn.Size = new System.Drawing.Size(84, 23);
      this.ButtonEndLearn.TabIndex = 5;
      this.ButtonEndLearn.Text = "End Learning";
      this.ButtonEndLearn.UseVisualStyleBackColor = true;
      this.ButtonEndLearn.Click += new System.EventHandler(this.ButtonEndLearn_Click);
      // 
      // mpCancel
      // 
      this.mpCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.mpCancel.Location = new System.Drawing.Point(334, 401);
      this.mpCancel.Name = "mpCancel";
      this.mpCancel.Size = new System.Drawing.Size(75, 23);
      this.mpCancel.TabIndex = 3;
      this.mpCancel.Text = "Cancel";
      this.mpCancel.UseVisualStyleBackColor = true;
      this.mpCancel.Click += new System.EventHandler(this.mpCancel_Click);
      // 
      // buttonOK
      // 
      this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOK.Location = new System.Drawing.Point(172, 401);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(75, 23);
      this.buttonOK.TabIndex = 2;
      this.buttonOK.Text = "OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.mpOK_Click);
      // 
      // mpBeveledLine1
      // 
      this.mpBeveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpBeveledLine1.Location = new System.Drawing.Point(5, 370);
      this.mpBeveledLine1.Name = "mpBeveledLine1";
      this.mpBeveledLine1.Size = new System.Drawing.Size(424, 2);
      this.mpBeveledLine1.TabIndex = 0;
      this.mpBeveledLine1.TabStop = false;
      // 
      // mpApply
      // 
      this.mpApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpApply.Location = new System.Drawing.Point(253, 401);
      this.mpApply.Name = "mpApply";
      this.mpApply.Size = new System.Drawing.Size(75, 23);
      this.mpApply.TabIndex = 1;
      this.mpApply.Text = "Apply";
      this.mpApply.UseVisualStyleBackColor = true;
      this.mpApply.Click += new System.EventHandler(this.mpApply_Click);
      // 
      // mpListView
      // 
      this.mpListView.AllowDrop = true;
      this.mpListView.AllowRowReorder = false;
      this.mpListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderMPAction,
            this.columnHeaderX10Key});
      this.mpListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.mpListView.Location = new System.Drawing.Point(12, 22);
      this.mpListView.MultiSelect = false;
      this.mpListView.Name = "mpListView";
      this.mpListView.Size = new System.Drawing.Size(283, 273);
      this.mpListView.TabIndex = 0;
      this.mpListView.UseCompatibleStateImageBehavior = false;
      this.mpListView.View = System.Windows.Forms.View.Details;
      // 
      // columnHeaderMPAction
      // 
      this.columnHeaderMPAction.Text = "MediaPortal Action";
      this.columnHeaderMPAction.Width = 126;
      // 
      // columnHeaderX10Key
      // 
      this.columnHeaderX10Key.Text = "Current X10 Key";
      this.columnHeaderX10Key.Width = 152;
      // 
      // InputMapperButton
      // 
      this.InputMapperButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.InputMapperButton.Location = new System.Drawing.Point(325, 194);
      this.InputMapperButton.Name = "InputMapperButton";
      this.InputMapperButton.Size = new System.Drawing.Size(84, 23);
      this.InputMapperButton.TabIndex = 7;
      this.InputMapperButton.Text = "Mapping";
      this.InputMapperButton.UseVisualStyleBackColor = true;
      this.InputMapperButton.Click += new System.EventHandler(this.InputMapperButton_Click);
      // 
      // mpRemotenumber
      // 
      this.mpRemotenumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpRemotenumber.BorderColor = System.Drawing.Color.Empty;
      this.mpRemotenumber.FormattingEnabled = true;
      this.mpRemotenumber.Location = new System.Drawing.Point(12, 330);
      this.mpRemotenumber.Name = "mpRemotenumber";
      this.mpRemotenumber.Size = new System.Drawing.Size(283, 21);
      this.mpRemotenumber.TabIndex = 8;
      this.mpRemotenumber.SelectedIndexChanged += new System.EventHandler(this.mpRemotenumber_SelectedIndexChanged);
      // 
      // mpLabel1
      // 
      this.mpLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(9, 311);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(107, 13);
      this.mpLabel1.TabIndex = 9;
      this.mpLabel1.Text = "Remote Control Type";
      // 
      // RemoteLearn
      // 
      this.AcceptButton = this.buttonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.mpCancel;
      this.ClientSize = new System.Drawing.Size(441, 436);
      this.Controls.Add(this.mpLabel1);
      this.Controls.Add(this.mpRemotenumber);
      this.Controls.Add(this.InputMapperButton);
      this.Controls.Add(this.ButtonStartLearn);
      this.Controls.Add(this.ButtonEndLearn);
      this.Controls.Add(this.mpCancel);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.mpBeveledLine1);
      this.Controls.Add(this.mpApply);
      this.Controls.Add(this.mpListView);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "RemoteLearn";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Teach X10 Remote Control";
      this.Shown += new System.EventHandler(this.RemoteLearn_Shown);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView mpListView;
    private MediaPortal.UserInterface.Controls.MPButton mpApply;
    private MediaPortal.UserInterface.Controls.MPBeveledLine mpBeveledLine1;
    private MediaPortal.UserInterface.Controls.MPButton buttonOK;
    private MediaPortal.UserInterface.Controls.MPButton mpCancel;
    private MediaPortal.UserInterface.Controls.MPButton ButtonEndLearn;
    private MediaPortal.UserInterface.Controls.MPButton ButtonStartLearn;
    private System.Windows.Forms.ColumnHeader columnHeaderMPAction;
    private System.Windows.Forms.ColumnHeader columnHeaderX10Key;
    private MediaPortal.UserInterface.Controls.MPButton InputMapperButton;
    private MediaPortal.UserInterface.Controls.MPComboBox mpRemotenumber;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
  }
}