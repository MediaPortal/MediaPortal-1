namespace MediaPortal.Configuration.Sections
{
  partial class PluginsNew
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
      this.listViewPlugins = new MediaPortal.UserInterface.Controls.MPListView();
      this.imageListPlugins = new System.Windows.Forms.ImageList(this.components);
      this.SuspendLayout();
      // 
      // listViewPlugins
      // 
      this.listViewPlugins.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listViewPlugins.Location = new System.Drawing.Point(0, 0);
      this.listViewPlugins.Name = "listViewPlugins";
      this.listViewPlugins.Size = new System.Drawing.Size(472, 408);
      this.listViewPlugins.TabIndex = 0;
      this.listViewPlugins.TileSize = new System.Drawing.Size(168, 64);
      this.listViewPlugins.UseCompatibleStateImageBehavior = false;
      // 
      // imageListPlugins
      // 
      this.imageListPlugins.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
      this.imageListPlugins.ImageSize = new System.Drawing.Size(32, 32);
      this.imageListPlugins.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // PluginsNew
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listViewPlugins);
      this.Name = "PluginsNew";
      this.Size = new System.Drawing.Size(472, 408);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView listViewPlugins;
    private System.Windows.Forms.ImageList imageListPlugins;


  }
}
