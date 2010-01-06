using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class xPL_Connector_AdvancedSetupForm : MPConfigForm
  {
    private MPButton btnOK;
    private MPButton btnReset;
    private readonly IContainer components = null;
    private object ControlStateLock = new object();
    private MPGroupBox groupBox1;

    public xPL_Connector_AdvancedSetupForm()
    {
      Log.Debug("xPL_Connector_AdvancedSetupForm(): Constructor started");
      this.InitializeComponent();
      this.Refresh();
      Log.Debug("xPL_Connector_AdvancedSetupForm(): Constructor completed");
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      Log.Debug("xPL_Connector_AdvancedSetupForm.btnOK_Click(): started");
      xPL_Connector.AdvancedSettings.Save();
      base.Hide();
      base.Close();
      Log.Debug("xPL_Connector_AdvancedSetupForm.btnOK_Click(): Completed");
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
      Log.Debug("xPL_Connector_AdvancedSetupForm.btnReset_Click(): started");
      xPL_Connector.AdvancedSettings.SetDefaults();
      this.Refresh();
      Log.Debug("xPL_Connector_AdvancedSetupForm.btnReset_Click(): Completed");
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && (this.components != null))
      {
        this.components.Dispose();
      }
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.groupBox1 = new MPGroupBox();
      this.btnOK = new MPButton();
      this.btnReset = new MPButton();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                | AnchorStyles.Left)
                                               | AnchorStyles.Right)));
      this.groupBox1.FlatStyle = FlatStyle.Popup;
      this.groupBox1.Location = new Point(9, 6);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(357, 124);
      this.groupBox1.TabIndex = 4;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = " xPL_Connector Configuration ";
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.btnOK.Location = new Point(286, 136);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new Size(80, 23);
      this.btnOK.TabIndex = 108;
      this.btnOK.Text = "&OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new EventHandler(this.btnOK_Click);
      // 
      // btnReset
      // 
      this.btnReset.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.btnReset.Location = new Point(200, 136);
      this.btnReset.Name = "btnReset";
      this.btnReset.Size = new Size(80, 23);
      this.btnReset.TabIndex = 109;
      this.btnReset.Text = "&RESET";
      this.btnReset.UseVisualStyleBackColor = true;
      this.btnReset.Click += new EventHandler(this.btnReset_Click);
      // 
      // xPL_Connector_AdvancedSetupForm
      // 
      this.AutoScaleDimensions = new SizeF(6F, 13F);
      this.ClientSize = new Size(378, 165);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnReset);
      this.Controls.Add(this.groupBox1);
      this.Name = "xPL_Connector_AdvancedSetupForm";
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "MiniDisplay - Setup - Advanced Settings";
      this.ResumeLayout(false);
    }
  }
}