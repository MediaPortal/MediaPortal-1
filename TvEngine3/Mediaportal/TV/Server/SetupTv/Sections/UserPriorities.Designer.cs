using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class UserPriorities
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
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
      this.groupBoxSystemUsers = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.numericUpDownScheduler = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelScheduler = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelEpgGrabber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownEpgGrabber = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.numericUpDownOtherDefault = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelOtherDefault = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxOtherUsers = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.dataGridViewUserPriorities = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridView();
      this.dataGridViewColumnUser = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn();
      this.dataGridViewColumnPriority = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn();
      this.groupBoxSystemUsers.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScheduler)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpgGrabber)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOtherDefault)).BeginInit();
      this.groupBoxOtherUsers.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUserPriorities)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxSystemUsers
      // 
      this.groupBoxSystemUsers.Controls.Add(this.numericUpDownScheduler);
      this.groupBoxSystemUsers.Controls.Add(this.labelScheduler);
      this.groupBoxSystemUsers.Controls.Add(this.labelEpgGrabber);
      this.groupBoxSystemUsers.Controls.Add(this.numericUpDownEpgGrabber);
      this.groupBoxSystemUsers.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxSystemUsers.Location = new System.Drawing.Point(6, 6);
      this.groupBoxSystemUsers.Name = "groupBoxSystemUsers";
      this.groupBoxSystemUsers.Size = new System.Drawing.Size(287, 51);
      this.groupBoxSystemUsers.TabIndex = 0;
      this.groupBoxSystemUsers.TabStop = false;
      this.groupBoxSystemUsers.Text = "System Users";
      // 
      // numericUpDownScheduler
      // 
      this.numericUpDownScheduler.Location = new System.Drawing.Point(70, 19);
      this.numericUpDownScheduler.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownScheduler.Name = "numericUpDownScheduler";
      this.numericUpDownScheduler.Size = new System.Drawing.Size(50, 20);
      this.numericUpDownScheduler.TabIndex = 1;
      this.numericUpDownScheduler.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownScheduler.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
      // 
      // labelScheduler
      // 
      this.labelScheduler.AutoSize = true;
      this.labelScheduler.Location = new System.Drawing.Point(6, 21);
      this.labelScheduler.Name = "labelScheduler";
      this.labelScheduler.Size = new System.Drawing.Size(58, 13);
      this.labelScheduler.TabIndex = 0;
      this.labelScheduler.Text = "Scheduler:";
      // 
      // labelEpgGrabber
      // 
      this.labelEpgGrabber.AutoSize = true;
      this.labelEpgGrabber.Location = new System.Drawing.Point(145, 21);
      this.labelEpgGrabber.Name = "labelEpgGrabber";
      this.labelEpgGrabber.Size = new System.Drawing.Size(71, 13);
      this.labelEpgGrabber.TabIndex = 2;
      this.labelEpgGrabber.Text = "EPG grabber:";
      // 
      // numericUpDownEpgGrabber
      // 
      this.numericUpDownEpgGrabber.Location = new System.Drawing.Point(222, 19);
      this.numericUpDownEpgGrabber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownEpgGrabber.Name = "numericUpDownEpgGrabber";
      this.numericUpDownEpgGrabber.Size = new System.Drawing.Size(50, 20);
      this.numericUpDownEpgGrabber.TabIndex = 3;
      this.numericUpDownEpgGrabber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownEpgGrabber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // numericUpDownOtherDefault
      // 
      this.numericUpDownOtherDefault.Location = new System.Drawing.Point(70, 19);
      this.numericUpDownOtherDefault.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownOtherDefault.Name = "numericUpDownOtherDefault";
      this.numericUpDownOtherDefault.Size = new System.Drawing.Size(50, 20);
      this.numericUpDownOtherDefault.TabIndex = 1;
      this.numericUpDownOtherDefault.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownOtherDefault.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
      // 
      // labelOtherDefault
      // 
      this.labelOtherDefault.AutoSize = true;
      this.labelOtherDefault.Location = new System.Drawing.Point(6, 21);
      this.labelOtherDefault.Name = "labelOtherDefault";
      this.labelOtherDefault.Size = new System.Drawing.Size(44, 13);
      this.labelOtherDefault.TabIndex = 0;
      this.labelOtherDefault.Text = "Default:";
      // 
      // groupBoxOtherUsers
      // 
      this.groupBoxOtherUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxOtherUsers.Controls.Add(this.numericUpDownOtherDefault);
      this.groupBoxOtherUsers.Controls.Add(this.labelOtherDefault);
      this.groupBoxOtherUsers.Controls.Add(this.dataGridViewUserPriorities);
      this.groupBoxOtherUsers.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxOtherUsers.Location = new System.Drawing.Point(6, 63);
      this.groupBoxOtherUsers.Name = "groupBoxOtherUsers";
      this.groupBoxOtherUsers.Size = new System.Drawing.Size(468, 348);
      this.groupBoxOtherUsers.TabIndex = 1;
      this.groupBoxOtherUsers.TabStop = false;
      this.groupBoxOtherUsers.Text = "Other Users";
      // 
      // dataGridViewUserPriorities
      // 
      this.dataGridViewUserPriorities.AllowUserToOrderColumns = true;
      this.dataGridViewUserPriorities.AllowUserToResizeRows = false;
      this.dataGridViewUserPriorities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.dataGridViewUserPriorities.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
      this.dataGridViewUserPriorities.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.dataGridViewUserPriorities.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewColumnUser,
            this.dataGridViewColumnPriority});
      dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.dataGridViewUserPriorities.DefaultCellStyle = dataGridViewCellStyle8;
      this.dataGridViewUserPriorities.Location = new System.Drawing.Point(6, 48);
      this.dataGridViewUserPriorities.Name = "dataGridViewUserPriorities";
      dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.dataGridViewUserPriorities.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
      this.dataGridViewUserPriorities.Size = new System.Drawing.Size(456, 290);
      this.dataGridViewUserPriorities.TabIndex = 3;
      // 
      // dataGridViewColumnUser
      // 
      this.dataGridViewColumnUser.HeaderText = "User";
      this.dataGridViewColumnUser.Name = "dataGridViewColumnUser";
      this.dataGridViewColumnUser.Width = 285;
      // 
      // dataGridViewColumnPriority
      // 
      this.dataGridViewColumnPriority.HeaderText = "Priority";
      this.dataGridViewColumnPriority.Name = "dataGridViewColumnPriority";
      // 
      // UserPriorities
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.groupBoxSystemUsers);
      this.Controls.Add(this.groupBoxOtherUsers);
      this.Name = "UserPriorities";
      this.Size = new System.Drawing.Size(480, 420);
      this.groupBoxSystemUsers.ResumeLayout(false);
      this.groupBoxSystemUsers.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScheduler)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEpgGrabber)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOtherDefault)).EndInit();
      this.groupBoxOtherUsers.ResumeLayout(false);
      this.groupBoxOtherUsers.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUserPriorities)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MPGroupBox groupBoxOtherUsers;
    private MPDataGridView dataGridViewUserPriorities;
    private MPGroupBox groupBoxSystemUsers;
    private MPNumericUpDown numericUpDownOtherDefault;
    private MPLabel labelOtherDefault;
    private MPNumericUpDown numericUpDownScheduler;
    private MPLabel labelScheduler;
    private MPNumericUpDown numericUpDownEpgGrabber;
    private MPLabel labelEpgGrabber;
    private MPDataGridViewTextBoxColumn dataGridViewColumnUser;
    private MPDataGridViewTextBoxColumn dataGridViewColumnPriority;
  }
}