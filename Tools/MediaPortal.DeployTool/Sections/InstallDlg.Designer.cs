namespace MediaPortal.DeployTool.Sections
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
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallDlg));
        this.labelHeading = new System.Windows.Forms.Label();
        this.listView = new System.Windows.Forms.ListView();
        this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
        this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
        this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
        this.iconsList = new System.Windows.Forms.ImageList(this.components);
        this.SuspendLayout();
        // 
        // labelSectionHeader
        // 
        this.labelSectionHeader.Location = new System.Drawing.Point(5, 4);
        // 
        // labelHeading
        // 
        this.labelHeading.AutoSize = true;
        this.labelHeading.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.labelHeading.ForeColor = System.Drawing.Color.White;
        this.labelHeading.Location = new System.Drawing.Point(5, 30);
        this.labelHeading.Name = "labelHeading";
        this.labelHeading.Size = new System.Drawing.Size(452, 13);
        this.labelHeading.TabIndex = 11;
        this.labelHeading.Text = "Press the \"install\" button to perform all necessary actions to install your setup" +
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
        this.listView.Location = new System.Drawing.Point(10, 55);
        this.listView.Name = "listView";
        this.listView.Size = new System.Drawing.Size(638, 184);
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
        // iconsList
        // 
        this.iconsList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("iconsList.ImageStream")));
        this.iconsList.TransparentColor = System.Drawing.Color.Transparent;
        this.iconsList.Images.SetKeyName(0, "0_nothing_to_do.gif");
        this.iconsList.Images.SetKeyName(1, "1_install_needed.ico");
        this.iconsList.Images.SetKeyName(2, "2_version_mismatch.ico");
        // 
        // InstallDlg
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
        this.Controls.Add(this.listView);
        this.Controls.Add(this.labelHeading);
        this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.Name = "InstallDlg";
        this.Size = new System.Drawing.Size(666, 241);
        this.ParentChanged += new System.EventHandler(this.RequirementsDlg_ParentChanged);
        this.Controls.SetChildIndex(this.labelSectionHeader, 0);
        this.Controls.SetChildIndex(this.labelHeading, 0);
        this.Controls.SetChildIndex(this.listView, 0);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelHeading;
    private System.Windows.Forms.ListView listView;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.ImageList iconsList;
  }
}