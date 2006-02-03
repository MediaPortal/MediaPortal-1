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
      this.mpTextBoxPluginInfo = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.SuspendLayout();
      // 
      // listViewPlugins
      // 
      this.listViewPlugins.Location = new System.Drawing.Point(0, 0);
      this.listViewPlugins.Name = "listViewPlugins";
      this.listViewPlugins.Size = new System.Drawing.Size(472, 324);
      this.listViewPlugins.TabIndex = 0;
      this.listViewPlugins.UseCompatibleStateImageBehavior = false;
      // 
      // imageListPlugins
      // 
      this.imageListPlugins.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
      this.imageListPlugins.ImageSize = new System.Drawing.Size(32, 32);
      this.imageListPlugins.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // mpTextBoxPluginInfo
      // 
      this.mpTextBoxPluginInfo.Location = new System.Drawing.Point(0, 356);
      this.mpTextBoxPluginInfo.Multiline = true;
      this.mpTextBoxPluginInfo.Name = "mpTextBoxPluginInfo";
      this.mpTextBoxPluginInfo.Size = new System.Drawing.Size(472, 52);
      this.mpTextBoxPluginInfo.TabIndex = 1;
      // 
      // PluginsNew
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpTextBoxPluginInfo);
      this.Controls.Add(this.listViewPlugins);
      this.Name = "PluginsNew";
      this.Size = new System.Drawing.Size(472, 408);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPListView listViewPlugins;
    private System.Windows.Forms.ImageList imageListPlugins;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBoxPluginInfo;


  }
}
