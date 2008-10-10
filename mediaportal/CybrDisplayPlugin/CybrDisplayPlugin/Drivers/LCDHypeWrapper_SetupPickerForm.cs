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
        private readonly IContainer components;
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
            this.groupBox1 = new MPGroupBox();
            this.btnCybrDisplay = new MPButton();
            this.btnLCDHype = new MPButton();
            this.groupBox1.SuspendLayout();
            base.SuspendLayout();
            this.groupBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.groupBox1.Controls.Add(this.btnCybrDisplay);
            this.groupBox1.Controls.Add(this.btnLCDHype);
            this.groupBox1.FlatStyle = FlatStyle.Popup;
            this.groupBox1.Location = new Point(12, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(0x174, 0x48);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " Select the Advanced Configuration you wish to use ";
            this.btnCybrDisplay.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnCybrDisplay.Location = new Point(190, 0x13);
            this.btnCybrDisplay.Name = "btnCybrDisplay";
            this.btnCybrDisplay.Size = new Size(0xb0, 0x2f);
            this.btnCybrDisplay.TabIndex = 110;
            this.btnCybrDisplay.Text = "CybrDisplay Plugin";
            this.btnCybrDisplay.UseVisualStyleBackColor = true;
            this.btnCybrDisplay.Click += new EventHandler(this.btnCybrDisplay_Click);
            this.btnLCDHype.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnLCDHype.Location = new Point(6, 0x13);
            this.btnLCDHype.Name = "btnLCDHype";
            this.btnLCDHype.Size = new Size(0xb0, 0x2f);
            this.btnLCDHype.TabIndex = 0x6f;
            this.btnLCDHype.Text = "LCDHype Driver";
            this.btnLCDHype.UseVisualStyleBackColor = true;
            this.btnLCDHype.Click += new EventHandler(this.btnLCDHype_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x18c, 90);
            base.Controls.Add(this.groupBox1);
            base.Name = "LCDHypeWrapper_SetupPickerForm";
            base.StartPosition = FormStartPosition.CenterParent;
            base.Tag = "";
            this.Text = "Configuration Type Select";
            this.groupBox1.ResumeLayout(false);
            base.ResumeLayout(false);
        }
    }
}

