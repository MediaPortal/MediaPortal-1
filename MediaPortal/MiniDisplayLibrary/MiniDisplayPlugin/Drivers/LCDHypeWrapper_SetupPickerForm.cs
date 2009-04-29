using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class LCDHypeWrapper_SetupPickerForm : MPConfigForm
  {
    private MPButton btnMiniDisplay;
    private MPButton btnLCDHype;
    private readonly IContainer components = null;
    private object ControlStateLock = new object();
    private MPGroupBox groupBox1;

    public LCDHypeWrapper_SetupPickerForm()
    {
      Log.Debug("LCDHypeWrapper_AdvancedSetupForm(): Constructor started", new object[0]);
      this.InitializeComponent();
      Log.Debug("LCDHypeWrapper_AdvancedSetupForm(): Constructor completed", new object[0]);
    }

    private void btnMiniDisplay_Click(object sender, EventArgs e)
    {
      Log.Debug("LCDHypeWrapper_SetupPickerForm.btnMiniDisplay(): started", new object[0]);
      base.Tag = "MiniDisplay";
      base.Hide();
      base.Close();
      Log.Debug("LCDHypeWrapper_SetupPickerForm.btnMiniDisplay(): Completed", new object[0]);
    }

    private void btnLCDHype_Click(object sender, EventArgs e)
    {
      Log.Debug("LCDHypeWrapper_SetupPickerForm.btnLCDHype(): started", new object[0]);
      base.Tag = "LCDHype";
      base.Hide();
      base.Close();
      Log.Debug("LCDHypeWrapper_SetupPickerForm.btnLCDHype(): Completed", new object[0]);
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
      this.btnMiniDisplay = new MPButton();
      this.btnLCDHype = new MPButton();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((AnchorStyles) ((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                 | AnchorStyles.Left)
                                                | AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.btnMiniDisplay);
      this.groupBox1.Controls.Add(this.btnLCDHype);
      this.groupBox1.FlatStyle = FlatStyle.Popup;
      this.groupBox1.Location = new Point(12, 6);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(372, 72);
      this.groupBox1.TabIndex = 4;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = " Select the Advanced Configuration you wish to use ";
      // 
      // btnMiniDisplay
      // 
      this.btnMiniDisplay.Location = new Point(190, 19);
      this.btnMiniDisplay.Name = "btnMiniDisplay";
      this.btnMiniDisplay.Size = new Size(176, 47);
      this.btnMiniDisplay.TabIndex = 110;
      this.btnMiniDisplay.Text = "MiniDisplay Plugin";
      this.btnMiniDisplay.UseVisualStyleBackColor = true;
      this.btnMiniDisplay.Click += new EventHandler(this.btnMiniDisplay_Click);
      // 
      // btnLCDHype
      // 
      this.btnLCDHype.Location = new Point(6, 19);
      this.btnLCDHype.Name = "btnLCDHype";
      this.btnLCDHype.Size = new Size(176, 47);
      this.btnLCDHype.TabIndex = 111;
      this.btnLCDHype.Text = "LCDHype Driver";
      this.btnLCDHype.UseVisualStyleBackColor = true;
      this.btnLCDHype.Click += new EventHandler(this.btnLCDHype_Click);
      // 
      // LCDHypeWrapper_SetupPickerForm
      // 
      this.AutoScaleDimensions = new SizeF(6F, 13F);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(396, 90);
      this.Controls.Add(this.groupBox1);
      this.Name = "LCDHypeWrapper_SetupPickerForm";
      this.StartPosition = FormStartPosition.CenterParent;
      this.Tag = "";
      this.Text = "MiniDisplay - Setup - Configuration Type Select";
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);
    }
  }
}