namespace CybrDisplayPlugin.Drivers
{
    using MediaPortal.GUI.Library;
    using MediaPortal.UserInterface.Controls;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class xPL_Connector_AdvancedSetupForm : Form
    {
        private MPButton btnOK;
        private MPButton btnReset;
        private readonly IContainer components;
        private object ControlStateLock = new object();
        private MPGroupBox groupBox1;

        public xPL_Connector_AdvancedSetupForm()
        {
            Log.Debug("xPL_Connector_AdvancedSetupForm(): Constructor started", new object[0]);
            this.InitializeComponent();
            this.Refresh();
            Log.Debug("xPL_Connector_AdvancedSetupForm(): Constructor completed", new object[0]);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Log.Debug("xPL_Connector_AdvancedSetupForm.btnOK_Click(): started", new object[0]);
            xPL_Connector.AdvancedSettings.Save();
            base.Hide();
            base.Close();
            Log.Debug("xPL_Connector_AdvancedSetupForm.btnOK_Click(): Completed", new object[0]);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Log.Debug("xPL_Connector_AdvancedSetupForm.btnReset_Click(): started", new object[0]);
            xPL_Connector.AdvancedSettings.SetDefaults();
            this.Refresh();
            Log.Debug("xPL_Connector_AdvancedSetupForm.btnReset_Click(): Completed", new object[0]);
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
            base.SuspendLayout();
            this.groupBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.groupBox1.FlatStyle = FlatStyle.Popup;
            this.groupBox1.Location = new Point(9, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(0x165, 0x7c);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " xPL_Connector Configuration ";
            this.btnOK.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnOK.Location = new Point(0x11e, 0x88);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(80, 0x17);
            this.btnOK.TabIndex = 0x6c;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            this.btnReset.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnReset.Location = new Point(200, 0x88);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new Size(80, 0x17);
            this.btnReset.TabIndex = 0x6d;
            this.btnReset.Text = "&RESET";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new EventHandler(this.btnReset_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x17a, 0xa5);
            base.Controls.Add(this.btnOK);
            base.Controls.Add(this.btnReset);
            base.Controls.Add(this.groupBox1);
            base.Name = "xPL_Connector_AdvancedSetupForm";
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Advanced Settings";
            base.ResumeLayout(false);
        }
    }
}

