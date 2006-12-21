namespace MediaPortal.Remotes.X10Remote
{
  partial class X10Learn
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(X10Learn));
      this.label1 = new System.Windows.Forms.Label();
      this.mpChannelReadout = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.ButtonSetChannel = new MediaPortal.UserInterface.Controls.MPButton();
      this.ButtonStartLearn = new MediaPortal.UserInterface.Controls.MPButton();
      this.ButtonEndLearn = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpBeveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.mpApply = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeaderMPAction = new System.Windows.Forms.ColumnHeader();
      this.columnHeaderX10Key = new System.Windows.Forms.ColumnHeader();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(322, 219);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(86, 13);
      this.label1.TabIndex = 9;
      this.label1.Text = "Channel Number";
      // 
      // mpChannelReadout
      // 
      this.mpChannelReadout.Location = new System.Drawing.Point(325, 236);
      this.mpChannelReadout.Name = "mpChannelReadout";
      this.mpChannelReadout.Size = new System.Drawing.Size(100, 20);
      this.mpChannelReadout.TabIndex = 8;
      this.mpChannelReadout.Text = "128";
      this.mpChannelReadout.Value = 128;
      // 
      // ButtonSetChannel
      // 
      this.ButtonSetChannel.Location = new System.Drawing.Point(325, 169);
      this.ButtonSetChannel.Name = "ButtonSetChannel";
      this.ButtonSetChannel.Size = new System.Drawing.Size(84, 23);
      this.ButtonSetChannel.TabIndex = 7;
      this.ButtonSetChannel.Text = "Set Channel";
      this.ButtonSetChannel.UseVisualStyleBackColor = true;
      this.ButtonSetChannel.Click += new System.EventHandler(this.ButtonSetChannel_Click);
      // 
      // ButtonStartLearn
      // 
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
      this.mpCancel.Location = new System.Drawing.Point(338, 339);
      this.mpCancel.Name = "mpCancel";
      this.mpCancel.Size = new System.Drawing.Size(75, 23);
      this.mpCancel.TabIndex = 3;
      this.mpCancel.Text = "Cancel";
      this.mpCancel.UseVisualStyleBackColor = true;
      this.mpCancel.Click += new System.EventHandler(this.mpCancel_Click);
      // 
      // buttonOK
      // 
      this.buttonOK.Location = new System.Drawing.Point(245, 339);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(75, 23);
      this.buttonOK.TabIndex = 2;
      this.buttonOK.Text = "OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.mpOK_Click);
      // 
      // mpBeveledLine1
      // 
      this.mpBeveledLine1.Location = new System.Drawing.Point(8, 316);
      this.mpBeveledLine1.Name = "mpBeveledLine1";
      this.mpBeveledLine1.Size = new System.Drawing.Size(424, 2);
      this.mpBeveledLine1.TabIndex = 0;
      this.mpBeveledLine1.TabStop = false;
      // 
      // mpApply
      // 
      this.mpApply.Location = new System.Drawing.Point(153, 338);
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
      this.mpListView.AllowRowReorder = true;
      this.mpListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderMPAction,
            this.columnHeaderX10Key});
      this.mpListView.Location = new System.Drawing.Point(12, 22);
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
      this.columnHeaderX10Key.Width = 153;
      // 
      // X10Learn
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(441, 374);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.mpChannelReadout);
      this.Controls.Add(this.ButtonSetChannel);
      this.Controls.Add(this.ButtonStartLearn);
      this.Controls.Add(this.ButtonEndLearn);
      this.Controls.Add(this.mpCancel);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.mpBeveledLine1);
      this.Controls.Add(this.mpApply);
      this.Controls.Add(this.mpListView);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "X10Learn";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Teach X10 Remote Control";
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
    private MediaPortal.UserInterface.Controls.MPButton ButtonSetChannel;
    private MediaPortal.UserInterface.Controls.MPNumericTextBox mpChannelReadout;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ColumnHeader columnHeaderMPAction;
    private System.Windows.Forms.ColumnHeader columnHeaderX10Key;
  }
}