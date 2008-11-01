namespace CybrDisplayPlugin.Drivers
{
    using MediaPortal.GUI.Library;
    using MediaPortal.UserInterface.Controls;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class LCDHypeWrapper_SetupPickerForm : Form
    {
        private MPButton btnCybrDisplay;
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

        private void btnCybrDisplay_Click(object sender, EventArgs e)
        {
            Log.Debug("LCDHypeWrapper_SetupPickerForm.btnCybrDisplay(): started", new object[0]);
            base.Tag = "CybrDisplay";
            base.Hide();
            base.Close();
            Log.Debug("LCDHypeWrapper_SetupPickerForm.btnCybrDisplay(): Completed", new object[0]);
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
          this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
          this.btnCybrDisplay = new MediaPortal.UserInterface.Controls.MPButton();
          this.btnLCDHype = new MediaPortal.UserInterface.Controls.MPButton();
          this.groupBox1.SuspendLayout();
          this.SuspendLayout();
          // 
          // groupBox1
          // 
          this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                      | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.groupBox1.Controls.Add(this.btnCybrDisplay);
          this.groupBox1.Controls.Add(this.btnLCDHype);
          this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
          this.groupBox1.Location = new System.Drawing.Point(12, 6);
          this.groupBox1.Name = "groupBox1";
          this.groupBox1.Size = new System.Drawing.Size(372, 72);
          this.groupBox1.TabIndex = 4;
          this.groupBox1.TabStop = false;
          this.groupBox1.Text = " Select the Advanced Configuration you wish to use ";
          // 
          // btnCybrDisplay
          // 
          this.btnCybrDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.btnCybrDisplay.Location = new System.Drawing.Point(190, 19);
          this.btnCybrDisplay.Name = "btnCybrDisplay";
          this.btnCybrDisplay.Size = new System.Drawing.Size(176, 47);
          this.btnCybrDisplay.TabIndex = 110;
          this.btnCybrDisplay.Text = "CybrDisplay Plugin";
          this.btnCybrDisplay.UseVisualStyleBackColor = true;
          this.btnCybrDisplay.Click += new System.EventHandler(this.btnCybrDisplay_Click);
          // 
          // btnLCDHype
          // 
          this.btnLCDHype.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.btnLCDHype.Location = new System.Drawing.Point(6, 19);
          this.btnLCDHype.Name = "btnLCDHype";
          this.btnLCDHype.Size = new System.Drawing.Size(176, 47);
          this.btnLCDHype.TabIndex = 111;
          this.btnLCDHype.Text = "LCDHype Driver";
          this.btnLCDHype.UseVisualStyleBackColor = true;
          this.btnLCDHype.Click += new System.EventHandler(this.btnLCDHype_Click);
          // 
          // LCDHypeWrapper_SetupPickerForm
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(396, 90);
          this.Controls.Add(this.groupBox1);
          this.Name = "LCDHypeWrapper_SetupPickerForm";
          this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
          this.Tag = "";
          this.Text = "Configuration Type Select";
          this.groupBox1.ResumeLayout(false);
          this.ResumeLayout(false);

        }
    }
}

