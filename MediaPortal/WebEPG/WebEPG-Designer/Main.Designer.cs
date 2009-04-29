namespace WebEPG_Designer
{
  partial class Main
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
      this.tbUrl = new System.Windows.Forms.TextBox();
      this.Get = new System.Windows.Forms.Label();
      this.bLoad = new System.Windows.Forms.Button();
      this.tbSource = new System.Windows.Forms.TextBox();
      this.gbRequest = new System.Windows.Forms.GroupBox();
      this.cbExternal = new System.Windows.Forms.CheckBox();
      this.lPost = new System.Windows.Forms.Label();
      this.tbPost = new System.Windows.Forms.TextBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label5 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.tbEnd = new System.Windows.Forms.TextBox();
      this.tbStart = new System.Windows.Forms.TextBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.rbXml = new System.Windows.Forms.RadioButton();
      this.rbNormal = new System.Windows.Forms.RadioButton();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.tbTags = new System.Windows.Forms.TextBox();
      this.tbTemplate = new System.Windows.Forms.TextBox();
      this.bParse = new System.Windows.Forms.Button();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.tbCount = new System.Windows.Forms.TextBox();
      this.lvFields = new System.Windows.Forms.ListView();
      this.lvFound = new System.Windows.Forms.ListView();
      this.gbRequest.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // tbUrl
      // 
      this.tbUrl.Location = new System.Drawing.Point(54, 19);
      this.tbUrl.Name = "tbUrl";
      this.tbUrl.Size = new System.Drawing.Size(602, 20);
      this.tbUrl.TabIndex = 0;
      // 
      // Get
      // 
      this.Get.AutoSize = true;
      this.Get.Location = new System.Drawing.Point(18, 22);
      this.Get.Name = "Get";
      this.Get.Size = new System.Drawing.Size(24, 13);
      this.Get.TabIndex = 1;
      this.Get.Text = "Get";
      // 
      // bLoad
      // 
      this.bLoad.Location = new System.Drawing.Point(12, 118);
      this.bLoad.Name = "bLoad";
      this.bLoad.Size = new System.Drawing.Size(390, 39);
      this.bLoad.TabIndex = 4;
      this.bLoad.Text = "load";
      this.bLoad.UseVisualStyleBackColor = true;
      this.bLoad.Click += new System.EventHandler(this.bLoad_Click);
      // 
      // tbSource
      // 
      this.tbSource.Location = new System.Drawing.Point(6, 92);
      this.tbSource.MaxLength = 256000;
      this.tbSource.Multiline = true;
      this.tbSource.Name = "tbSource";
      this.tbSource.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.tbSource.Size = new System.Drawing.Size(384, 116);
      this.tbSource.TabIndex = 5;
      this.tbSource.WordWrap = false;
      // 
      // gbRequest
      // 
      this.gbRequest.Controls.Add(this.cbExternal);
      this.gbRequest.Controls.Add(this.lPost);
      this.gbRequest.Controls.Add(this.tbPost);
      this.gbRequest.Controls.Add(this.tbUrl);
      this.gbRequest.Controls.Add(this.Get);
      this.gbRequest.Location = new System.Drawing.Point(12, 12);
      this.gbRequest.Name = "gbRequest";
      this.gbRequest.Size = new System.Drawing.Size(675, 100);
      this.gbRequest.TabIndex = 6;
      this.gbRequest.TabStop = false;
      this.gbRequest.Text = "Site";
      // 
      // cbExternal
      // 
      this.cbExternal.AutoSize = true;
      this.cbExternal.Location = new System.Drawing.Point(55, 77);
      this.cbExternal.Name = "cbExternal";
      this.cbExternal.Size = new System.Drawing.Size(105, 17);
      this.cbExternal.TabIndex = 5;
      this.cbExternal.Text = "External (Use IE)";
      this.cbExternal.UseVisualStyleBackColor = true;
      // 
      // lPost
      // 
      this.lPost.AutoSize = true;
      this.lPost.Location = new System.Drawing.Point(18, 51);
      this.lPost.Name = "lPost";
      this.lPost.Size = new System.Drawing.Size(28, 13);
      this.lPost.TabIndex = 3;
      this.lPost.Text = "Post";
      // 
      // tbPost
      // 
      this.tbPost.Location = new System.Drawing.Point(55, 48);
      this.tbPost.Multiline = true;
      this.tbPost.Name = "tbPost";
      this.tbPost.Size = new System.Drawing.Size(601, 20);
      this.tbPost.TabIndex = 2;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.tbEnd);
      this.groupBox1.Controls.Add(this.tbStart);
      this.groupBox1.Controls.Add(this.tbSource);
      this.groupBox1.Location = new System.Drawing.Point(12, 163);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(396, 237);
      this.groupBox1.TabIndex = 8;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Html Source";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(233, 22);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(26, 13);
      this.label5.TabIndex = 10;
      this.label5.Text = "End";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(13, 22);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(29, 13);
      this.label4.TabIndex = 9;
      this.label4.Text = "Start";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(13, 76);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(41, 13);
      this.label3.TabIndex = 8;
      this.label3.Text = "Source";
      // 
      // tbEnd
      // 
      this.tbEnd.Location = new System.Drawing.Point(227, 38);
      this.tbEnd.Name = "tbEnd";
      this.tbEnd.Size = new System.Drawing.Size(163, 20);
      this.tbEnd.TabIndex = 7;
      // 
      // tbStart
      // 
      this.tbStart.Location = new System.Drawing.Point(6, 38);
      this.tbStart.Name = "tbStart";
      this.tbStart.Size = new System.Drawing.Size(171, 20);
      this.tbStart.TabIndex = 6;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.rbXml);
      this.groupBox2.Controls.Add(this.rbNormal);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Controls.Add(this.tbTags);
      this.groupBox2.Controls.Add(this.tbTemplate);
      this.groupBox2.Location = new System.Drawing.Point(414, 163);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(273, 237);
      this.groupBox2.TabIndex = 9;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Template";
      // 
      // rbXml
      // 
      this.rbXml.AutoSize = true;
      this.rbXml.Location = new System.Drawing.Point(187, 214);
      this.rbXml.Name = "rbXml";
      this.rbXml.Size = new System.Drawing.Size(42, 17);
      this.rbXml.TabIndex = 5;
      this.rbXml.TabStop = true;
      this.rbXml.Text = "Xml";
      this.rbXml.UseVisualStyleBackColor = true;
      this.rbXml.CheckedChanged += new System.EventHandler(this.rbXml_CheckedChanged);
      // 
      // rbNormal
      // 
      this.rbNormal.AutoSize = true;
      this.rbNormal.Location = new System.Drawing.Point(33, 214);
      this.rbNormal.Name = "rbNormal";
      this.rbNormal.Size = new System.Drawing.Size(58, 17);
      this.rbNormal.TabIndex = 4;
      this.rbNormal.TabStop = true;
      this.rbNormal.Text = "Normal";
      this.rbNormal.UseVisualStyleBackColor = true;
      this.rbNormal.CheckedChanged += new System.EventHandler(this.rbNormal_CheckedChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 76);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(28, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "Text";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(9, 22);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(31, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "Tags";
      // 
      // tbTags
      // 
      this.tbTags.Location = new System.Drawing.Point(11, 38);
      this.tbTags.Name = "tbTags";
      this.tbTags.Size = new System.Drawing.Size(243, 20);
      this.tbTags.TabIndex = 1;
      // 
      // tbTemplate
      // 
      this.tbTemplate.Location = new System.Drawing.Point(11, 92);
      this.tbTemplate.Multiline = true;
      this.tbTemplate.Name = "tbTemplate";
      this.tbTemplate.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.tbTemplate.Size = new System.Drawing.Size(243, 116);
      this.tbTemplate.TabIndex = 0;
      this.tbTemplate.TextChanged += new System.EventHandler(this.tbTemplate_TextChanged);
      // 
      // bParse
      // 
      this.bParse.Location = new System.Drawing.Point(12, 406);
      this.bParse.Name = "bParse";
      this.bParse.Size = new System.Drawing.Size(675, 34);
      this.bParse.TabIndex = 10;
      this.bParse.Text = "Parse";
      this.bParse.UseVisualStyleBackColor = true;
      this.bParse.Click += new System.EventHandler(this.bParse_Click);
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.tbCount);
      this.groupBox3.Controls.Add(this.lvFields);
      this.groupBox3.Controls.Add(this.lvFound);
      this.groupBox3.Location = new System.Drawing.Point(12, 446);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(671, 209);
      this.groupBox3.TabIndex = 11;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Parser Results";
      // 
      // tbCount
      // 
      this.tbCount.Location = new System.Drawing.Point(17, 179);
      this.tbCount.Name = "tbCount";
      this.tbCount.Size = new System.Drawing.Size(36, 20);
      this.tbCount.TabIndex = 2;
      // 
      // lvFields
      // 
      this.lvFields.Location = new System.Drawing.Point(409, 19);
      this.lvFields.Name = "lvFields";
      this.lvFields.Size = new System.Drawing.Size(242, 152);
      this.lvFields.TabIndex = 1;
      this.lvFields.UseCompatibleStateImageBehavior = false;
      this.lvFields.View = System.Windows.Forms.View.Details;
      // 
      // lvFound
      // 
      this.lvFound.FullRowSelect = true;
      this.lvFound.HideSelection = false;
      this.lvFound.Location = new System.Drawing.Point(16, 19);
      this.lvFound.MultiSelect = false;
      this.lvFound.Name = "lvFound";
      this.lvFound.Size = new System.Drawing.Size(370, 153);
      this.lvFound.TabIndex = 0;
      this.lvFound.UseCompatibleStateImageBehavior = false;
      this.lvFound.View = System.Windows.Forms.View.Details;
      this.lvFound.SelectedIndexChanged += new System.EventHandler(this.lvFound_SelectedIndexChanged);
      // 
      // Main
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(699, 683);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.bParse);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.gbRequest);
      this.Controls.Add(this.bLoad);
      this.Name = "Main";
      this.Text = "WebEPG Designer";
      this.gbRequest.ResumeLayout(false);
      this.gbRequest.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TextBox tbUrl;
    private System.Windows.Forms.Label Get;
    private System.Windows.Forms.Button bLoad;
    private System.Windows.Forms.TextBox tbSource;
    private System.Windows.Forms.GroupBox gbRequest;
    private System.Windows.Forms.CheckBox cbExternal;
    private System.Windows.Forms.Label lPost;
    private System.Windows.Forms.TextBox tbPost;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox tbTags;
    private System.Windows.Forms.TextBox tbTemplate;
    private System.Windows.Forms.Button bParse;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.ListView lvFields;
    private System.Windows.Forms.ListView lvFound;
    private System.Windows.Forms.TextBox tbCount;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox tbEnd;
    private System.Windows.Forms.TextBox tbStart;
    private System.Windows.Forms.RadioButton rbNormal;
    private System.Windows.Forms.RadioButton rbXml;
  }
}

