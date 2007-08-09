namespace MediaPortal.DeployTool
{
  partial class InstallDlg
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
      this.buttonInstall = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.listView = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.SuspendLayout();
      // 
      // buttonInstall
      // 
      this.buttonInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonInstall.Location = new System.Drawing.Point(377, 25);
      this.buttonInstall.Name = "buttonInstall";
      this.buttonInstall.Size = new System.Drawing.Size(75, 23);
      this.buttonInstall.TabIndex = 14;
      this.buttonInstall.Text = "install";
      this.buttonInstall.UseVisualStyleBackColor = true;
      this.buttonInstall.Click += new System.EventHandler(this.buttonInstall_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(4, 30);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(367, 13);
      this.label1.TabIndex = 11;
      this.label1.Text = "Press the \"install\" button to perform all necessary actions to install your setup" +
          "";
      // 
      // listView
      // 
      this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
      this.listView.Location = new System.Drawing.Point(3, 54);
      this.listView.Name = "listView";
      this.listView.Size = new System.Drawing.Size(614, 141);
      this.listView.TabIndex = 15;
      this.listView.UseCompatibleStateImageBehavior = false;
      this.listView.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Component";
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Status";
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Action";
      // 
      // RequirementsDlg
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listView);
      this.Controls.Add(this.buttonInstall);
      this.Controls.Add(this.label1);
      this.Name = "RequirementsDlg";
      this.Size = new System.Drawing.Size(620, 241);
      this.ParentChanged += new System.EventHandler(this.RequirementsDlg_ParentChanged);
      this.Controls.SetChildIndex(this.HeaderLabel, 0);
      this.Controls.SetChildIndex(this.label1, 0);
      this.Controls.SetChildIndex(this.buttonInstall, 0);
      this.Controls.SetChildIndex(this.listView, 0);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonInstall;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ListView listView;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
  }
}
