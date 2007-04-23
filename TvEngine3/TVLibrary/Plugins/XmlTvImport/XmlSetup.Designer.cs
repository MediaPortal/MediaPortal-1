namespace SetupTv.Sections
{
  partial class XmlSetup
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
      this.buttonBrowse = new System.Windows.Forms.Button();
      this.label13 = new System.Windows.Forms.Label();
      this.textBoxFolder = new System.Windows.Forms.TextBox();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.textBoxHours = new System.Windows.Forms.TextBox();
      this.textBoxMinutes = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.labelStatus = new System.Windows.Forms.Label();
      this.labelPrograms = new System.Windows.Forms.Label();
      this.labelChannels = new System.Windows.Forms.Label();
      this.labelLastImport = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.cbImportXML = new System.Windows.Forms.CheckBox();
      this.cbImportLST = new System.Windows.Forms.CheckBox();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Location = new System.Drawing.Point(360, 46);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(23, 23);
      this.buttonBrowse.TabIndex = 7;
      this.buttonBrowse.Text = "...";
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(21, 13);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(200, 13);
      this.label13.TabIndex = 6;
      this.label13.Text = "Folder where the tvguide.xml file is stored";
      // 
      // textBoxFolder
      // 
      this.textBoxFolder.Location = new System.Drawing.Point(70, 49);
      this.textBoxFolder.Name = "textBoxFolder";
      this.textBoxFolder.Size = new System.Drawing.Size(284, 20);
      this.textBoxFolder.TabIndex = 5;
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.checkBox1.Location = new System.Drawing.Point(24, 153);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(288, 17);
      this.checkBox1.TabIndex = 8;
      this.checkBox1.Text = "Apply timezone compensation when loading tvguide.xml";
      this.checkBox1.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(47, 182);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(38, 13);
      this.label1.TabIndex = 9;
      this.label1.Text = "Hours:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(172, 182);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(47, 13);
      this.label2.TabIndex = 10;
      this.label2.Text = "Minutes:";
      // 
      // textBoxHours
      // 
      this.textBoxHours.Location = new System.Drawing.Point(91, 179);
      this.textBoxHours.Name = "textBoxHours";
      this.textBoxHours.Size = new System.Drawing.Size(50, 20);
      this.textBoxHours.TabIndex = 11;
      // 
      // textBoxMinutes
      // 
      this.textBoxMinutes.Location = new System.Drawing.Point(225, 179);
      this.textBoxMinutes.Name = "textBoxMinutes";
      this.textBoxMinutes.Size = new System.Drawing.Size(50, 20);
      this.textBoxMinutes.TabIndex = 12;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(21, 230);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(357, 13);
      this.label3.TabIndex = 13;
      this.label3.Text = "The server will check every minute if there is a new tvguide.xml/tvguide.lst";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(21, 243);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(405, 13);
      this.label4.TabIndex = 14;
      this.label4.Text = "When  it detects a new file, it will be import the EPG and tv channels in the dat" +
          "abase";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.Location = new System.Drawing.Point(21, 30);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(371, 13);
      this.label5.TabIndex = 15;
      this.label5.Text = "Please note that this folder should also include the xmltv.dtd file";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.labelStatus);
      this.groupBox1.Controls.Add(this.labelPrograms);
      this.groupBox1.Controls.Add(this.labelChannels);
      this.groupBox1.Controls.Add(this.labelLastImport);
      this.groupBox1.Controls.Add(this.label9);
      this.groupBox1.Controls.Add(this.label8);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Location = new System.Drawing.Point(21, 281);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(426, 153);
      this.groupBox1.TabIndex = 16;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Import status report:";
      // 
      // labelStatus
      // 
      this.labelStatus.AutoSize = true;
      this.labelStatus.Location = new System.Drawing.Point(163, 97);
      this.labelStatus.Name = "labelStatus";
      this.labelStatus.Size = new System.Drawing.Size(0, 13);
      this.labelStatus.TabIndex = 24;
      // 
      // labelPrograms
      // 
      this.labelPrograms.AutoSize = true;
      this.labelPrograms.Location = new System.Drawing.Point(163, 75);
      this.labelPrograms.Name = "labelPrograms";
      this.labelPrograms.Size = new System.Drawing.Size(0, 13);
      this.labelPrograms.TabIndex = 23;
      // 
      // labelChannels
      // 
      this.labelChannels.AutoSize = true;
      this.labelChannels.Location = new System.Drawing.Point(163, 52);
      this.labelChannels.Name = "labelChannels";
      this.labelChannels.Size = new System.Drawing.Size(0, 13);
      this.labelChannels.TabIndex = 22;
      // 
      // labelLastImport
      // 
      this.labelLastImport.AutoSize = true;
      this.labelLastImport.Location = new System.Drawing.Point(163, 28);
      this.labelLastImport.Name = "labelLastImport";
      this.labelLastImport.Size = new System.Drawing.Size(0, 13);
      this.labelLastImport.TabIndex = 21;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(17, 97);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(70, 13);
      this.label9.TabIndex = 20;
      this.label9.Text = "Import status:";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(17, 75);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(123, 13);
      this.label8.TabIndex = 19;
      this.label8.Text = "Total programs imported:";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(17, 52);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(123, 13);
      this.label7.TabIndex = 18;
      this.label7.Text = "Total channels imported:";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(17, 28);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(91, 13);
      this.label6.TabIndex = 17;
      this.label6.Text = "Last import run at:";
      // 
      // cbImportXML
      // 
      this.cbImportXML.AutoSize = true;
      this.cbImportXML.Location = new System.Drawing.Point(24, 93);
      this.cbImportXML.Name = "cbImportXML";
      this.cbImportXML.Size = new System.Drawing.Size(134, 17);
      this.cbImportXML.TabIndex = 17;
      this.cbImportXML.Text = "Import new tvguide.xml";
      this.cbImportXML.UseVisualStyleBackColor = true;
      // 
      // cbImportLST
      // 
      this.cbImportLST.AutoSize = true;
      this.cbImportLST.Location = new System.Drawing.Point(24, 116);
      this.cbImportLST.Name = "cbImportLST";
      this.cbImportLST.Size = new System.Drawing.Size(161, 17);
      this.cbImportLST.TabIndex = 18;
      this.cbImportLST.Text = "Import files in new tvguide.lst";
      this.cbImportLST.UseVisualStyleBackColor = true;
      // 
      // XmlSetup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.cbImportLST);
      this.Controls.Add(this.cbImportXML);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.textBoxMinutes);
      this.Controls.Add(this.textBoxHours);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.checkBox1);
      this.Controls.Add(this.buttonBrowse);
      this.Controls.Add(this.label13);
      this.Controls.Add(this.textBoxFolder);
      this.Name = "XmlSetup";
      this.Size = new System.Drawing.Size(467, 454);
      this.Load += new System.EventHandler(this.XmlSetup_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonBrowse;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.TextBox textBoxFolder;
    private System.Windows.Forms.CheckBox checkBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox textBoxHours;
    private System.Windows.Forms.TextBox textBoxMinutes;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label labelStatus;
    private System.Windows.Forms.Label labelPrograms;
    private System.Windows.Forms.Label labelChannels;
    private System.Windows.Forms.Label labelLastImport;
    private System.Windows.Forms.CheckBox cbImportXML;
    private System.Windows.Forms.CheckBox cbImportLST;

  }
}