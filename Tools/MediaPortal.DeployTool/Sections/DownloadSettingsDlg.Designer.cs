namespace MediaPortal.DeployTool.Sections
{
  partial class DownloadSettingsDlg
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.rb32bit = new System.Windows.Forms.Label();
        this.rb64bit = new System.Windows.Forms.Label();
        this.listViewLang = new MediaPortal.DeployTool.MPListView();
        this.columnCountry = new System.Windows.Forms.ColumnHeader();
        this.columnID = new System.Windows.Forms.ColumnHeader();
        this.columnLang3 = new System.Windows.Forms.ColumnHeader();
        this.b32bit = new System.Windows.Forms.Button();
        this.b64bit = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelSectionHeader.Location = new System.Drawing.Point(200, 14);
        this.labelSectionHeader.MaximumSize = new System.Drawing.Size(350, 0);
        this.labelSectionHeader.Size = new System.Drawing.Size(253, 14);
        this.labelSectionHeader.Text = "Please select settings for downloads:";
        // 
        // rb32bit
        // 
        this.rb32bit.AutoSize = true;
        this.rb32bit.Cursor = System.Windows.Forms.Cursors.Hand;
        this.rb32bit.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rb32bit.ForeColor = System.Drawing.Color.White;
        this.rb32bit.Location = new System.Drawing.Point(245, 193);
        this.rb32bit.Name = "rb32bit";
        this.rb32bit.Size = new System.Drawing.Size(70, 13);
        this.rb32bit.TabIndex = 13;
        this.rb32bit.Text = "32bit (x86)";
        this.rb32bit.Click += new System.EventHandler(this.b32bit_Click);
        // 
        // rb64bit
        // 
        this.rb64bit.AutoSize = true;
        this.rb64bit.Cursor = System.Windows.Forms.Cursors.Hand;
        this.rb64bit.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.rb64bit.ForeColor = System.Drawing.Color.White;
        this.rb64bit.Location = new System.Drawing.Point(437, 193);
        this.rb64bit.Name = "rb64bit";
        this.rb64bit.Size = new System.Drawing.Size(70, 13);
        this.rb64bit.TabIndex = 14;
        this.rb64bit.Text = "64bit (x64)";
        this.rb64bit.Click += new System.EventHandler(this.b64bit_Click);
        // 
        // listViewLang
        // 
        this.listViewLang.BackColor = System.Drawing.SystemColors.Window;
        this.listViewLang.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnCountry,
            this.columnID,
            this.columnLang3});
        this.listViewLang.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.listViewLang.FullRowSelect = true;
        this.listViewLang.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
        this.listViewLang.HideSelection = false;
        this.listViewLang.Location = new System.Drawing.Point(201, 43);
        this.listViewLang.MultiSelect = false;
        this.listViewLang.Name = "listViewLang";
        this.listViewLang.Size = new System.Drawing.Size(327, 115);
        this.listViewLang.Sorting = System.Windows.Forms.SortOrder.Ascending;
        this.listViewLang.TabIndex = 1;
        this.listViewLang.UseCompatibleStateImageBehavior = false;
        this.listViewLang.View = System.Windows.Forms.View.Details;
        // 
        // columnCountry
        // 
        this.columnCountry.Text = "Country";
        // 
        // columnID
        // 
        this.columnID.Text = "ID";
        // 
        // columnLang3
        // 
        this.columnLang3.Text = "Code";
        this.columnLang3.Width = 102;
        // 
        // b32bit
        // 
        this.b32bit.FlatAppearance.BorderSize = 0;
        this.b32bit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        this.b32bit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        this.b32bit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.b32bit.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
        this.b32bit.Location = new System.Drawing.Point(202, 188);
        this.b32bit.Name = "b32bit";
        this.b32bit.Size = new System.Drawing.Size(37, 23);
        this.b32bit.TabIndex = 2;
        this.b32bit.UseVisualStyleBackColor = true;
        this.b32bit.Click += new System.EventHandler(this.b32bit_Click);
        // 
        // b64bit
        // 
        this.b64bit.FlatAppearance.BorderSize = 0;
        this.b64bit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        this.b64bit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        this.b64bit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.b64bit.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
        this.b64bit.Location = new System.Drawing.Point(394, 188);
        this.b64bit.Name = "b64bit";
        this.b64bit.Size = new System.Drawing.Size(37, 23);
        this.b64bit.TabIndex = 3;
        this.b64bit.UseVisualStyleBackColor = true;
        this.b64bit.Click += new System.EventHandler(this.b64bit_Click);
        // 
        // DownloadSettingsDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_internet_connection;
        this.Controls.Add(this.b64bit);
        this.Controls.Add(this.b32bit);
        this.Controls.Add(this.listViewLang);
        this.Controls.Add(this.rb64bit);
        this.Controls.Add(this.rb32bit);
        this.Name = "DownloadSettingsDlg";
        this.Size = new System.Drawing.Size(666, 250);
        this.Controls.SetChildIndex(this.rb32bit, 0);
        this.Controls.SetChildIndex(this.rb64bit, 0);
        this.Controls.SetChildIndex(this.listViewLang, 0);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.b32bit, 0);
        this.Controls.SetChildIndex(this.b64bit, 0);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label rb32bit;
    private System.Windows.Forms.Label rb64bit;
    private MPListView listViewLang;
    private System.Windows.Forms.ColumnHeader columnCountry;
    private System.Windows.Forms.ColumnHeader columnID;
    private System.Windows.Forms.ColumnHeader columnLang3;
    private System.Windows.Forms.Button b32bit;
    private System.Windows.Forms.Button b64bit;

  }
}
