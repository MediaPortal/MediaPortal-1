namespace SetupTv.Sections
{
  partial class CMSetup
  {
    /// <summary>
    /// Variable nécessaire au concepteur.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Nettoyage des ressources utilisées.
    /// </summary>
    /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Code généré par le Concepteur Windows Form

    /// <summary>
    /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
    /// le contenu de cette méthode avec l'éditeur de code.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.analyzeMode = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.debug = new System.Windows.Forms.CheckBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.analyzeMode);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Location = new System.Drawing.Point(3, 3);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(240, 52);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Analyzer settings";
      // 
      // analyzeMode
      // 
      this.analyzeMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.analyzeMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.analyzeMode.FormattingEnabled = true;
      this.analyzeMode.Items.AddRange(new object[] {
            "Fast",
            "Normal",
            "Resolve"});
      this.analyzeMode.Location = new System.Drawing.Point(100, 19);
      this.analyzeMode.Name = "analyzeMode";
      this.analyzeMode.Size = new System.Drawing.Size(134, 21);
      this.analyzeMode.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(6, 22);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(73, 13);
      this.label2.TabIndex = 0;
      this.label2.Text = "Analyze mode";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.debug);
      this.groupBox2.Location = new System.Drawing.Point(3, 61);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(240, 47);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Debug section";
      // 
      // debug
      // 
      this.debug.AutoSize = true;
      this.debug.Location = new System.Drawing.Point(6, 19);
      this.debug.Name = "debug";
      this.debug.Size = new System.Drawing.Size(143, 17);
      this.debug.TabIndex = 0;
      this.debug.Text = "Enable extended logging";
      this.debug.UseVisualStyleBackColor = true;
      // 
      // CMSetup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "CMSetup";
      this.Size = new System.Drawing.Size(460, 380);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.ComboBox analyzeMode;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.CheckBox debug;
  }
}