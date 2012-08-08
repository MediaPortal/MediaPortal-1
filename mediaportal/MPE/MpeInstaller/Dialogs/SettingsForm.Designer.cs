namespace MpeInstaller.Dialogs
{
  partial class SettingsForm
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.chk_updateExtension = new System.Windows.Forms.CheckBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.numeric_Days = new System.Windows.Forms.NumericUpDown();
      this.chk_update = new System.Windows.Forms.CheckBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numeric_Days)).BeginInit();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.chk_updateExtension);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.numeric_Days);
      this.groupBox1.Controls.Add(this.chk_update);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(1, 1);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(543, 78);
      this.groupBox1.TabIndex = 3;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Startup";
      this.groupBox1.UseCompatibleTextRendering = true;
      // 
      // chk_updateExtension
      // 
      this.chk_updateExtension.AutoSize = true;
      this.chk_updateExtension.Location = new System.Drawing.Point(6, 42);
      this.chk_updateExtension.Name = "chk_updateExtension";
      this.chk_updateExtension.Size = new System.Drawing.Size(218, 17);
      this.chk_updateExtension.TabIndex = 4;
      this.chk_updateExtension.Text = "Automatically update installed extensions";
      this.toolTip1.SetToolTip(this.chk_updateExtension, "If enabled, this option will automatically download and install an update for you" +
        "r installed extensions.");
      this.chk_updateExtension.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(507, 20);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(29, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "days";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(421, 20);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(34, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "Every";
      // 
      // numeric_Days
      // 
      this.numeric_Days.Location = new System.Drawing.Point(461, 16);
      this.numeric_Days.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
      this.numeric_Days.Name = "numeric_Days";
      this.numeric_Days.Size = new System.Drawing.Size(39, 20);
      this.numeric_Days.TabIndex = 1;
      this.numeric_Days.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // chk_update
      // 
      this.chk_update.AutoSize = true;
      this.chk_update.Location = new System.Drawing.Point(6, 19);
      this.chk_update.Name = "chk_update";
      this.chk_update.Size = new System.Drawing.Size(345, 17);
      this.chk_update.TabIndex = 0;
      this.chk_update.Text = "Automatically download update information for extensions on startup";
      this.toolTip1.SetToolTip(this.chk_update, resources.GetString("chk_update.ToolTip"));
      this.chk_update.UseVisualStyleBackColor = true;
      this.chk_update.CheckedChanged += new System.EventHandler(this.chk_update_CheckedChanged);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnOK);
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(1, 79);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(543, 32);
      this.panel1.TabIndex = 4;
      // 
      // btnOK
      // 
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(380, 4);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(75, 23);
      this.btnOK.TabIndex = 1;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(461, 4);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 0;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // toolTip1
      // 
      this.toolTip1.IsBalloon = true;
      // 
      // SettingsForm
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(545, 112);
      this.ControlBox = false;
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "SettingsForm";
      this.Padding = new System.Windows.Forms.Padding(1);
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Settings";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numeric_Days)).EndInit();
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    public System.Windows.Forms.CheckBox chk_updateExtension;
    public System.Windows.Forms.NumericUpDown numeric_Days;
    public System.Windows.Forms.CheckBox chk_update;
    private System.Windows.Forms.ToolTip toolTip1;
  }
}