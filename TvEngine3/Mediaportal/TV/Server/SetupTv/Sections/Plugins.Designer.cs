using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class Plugins
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
      System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Available Plugins", System.Windows.Forms.HorizontalAlignment.Left);
      System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Incompatible Plugins", System.Windows.Forms.HorizontalAlignment.Left);
      this.listViewPlugins = new System.Windows.Forms.ListView();
      this.columnHeaderEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeaderAuthor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeaderVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.SuspendLayout();
      // 
      // listViewPlugins
      // 
      this.listViewPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewPlugins.CheckBoxes = true;
      this.listViewPlugins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderEnabled,
            this.columnHeaderName,
            this.columnHeaderVersion,
            this.columnHeaderAuthor});
      listViewGroup1.Header = "Available Plugins";
      listViewGroup1.Name = "listViewGroupAvailable";
      listViewGroup2.Header = "Incompatible Plugins";
      listViewGroup2.Name = "listViewGroupIncompatible";
      this.listViewPlugins.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
      this.listViewPlugins.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.listViewPlugins.Location = new System.Drawing.Point(6, 6);
      this.listViewPlugins.Name = "listViewPlugins";
      this.listViewPlugins.Size = new System.Drawing.Size(468, 408);
      this.listViewPlugins.TabIndex = 0;
      this.listViewPlugins.UseCompatibleStateImageBehavior = false;
      this.listViewPlugins.View = System.Windows.Forms.View.Details;
      this.listViewPlugins.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewPlugins_ItemChecked);
      // 
      // columnHeaderEnabled
      // 
      this.columnHeaderEnabled.Text = "Enabled";
      // 
      // columnHeaderName
      // 
      this.columnHeaderName.Text = "Name";
      this.columnHeaderName.Width = 140;
      // 
      // columnHeaderAuthor
      // 
      this.columnHeaderAuthor.Text = "Author";
      this.columnHeaderAuthor.Width = 141;
      // 
      // columnHeaderVersion
      // 
      this.columnHeaderVersion.Text = "Version";
      this.columnHeaderVersion.Width = 98;
      // 
      // Plugins
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.listViewPlugins);
      this.Name = "Plugins";
      this.Size = new System.Drawing.Size(480, 420);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ListView listViewPlugins;
    private System.Windows.Forms.ColumnHeader columnHeaderEnabled;
    private System.Windows.Forms.ColumnHeader columnHeaderName;
    private System.Windows.Forms.ColumnHeader columnHeaderAuthor;
    private System.Windows.Forms.ColumnHeader columnHeaderVersion;

  }
}