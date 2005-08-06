using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace WindowPlugins.GUIPrograms
{
	/// <summary>
	/// Summary description for AppSettingsPrePost.
	/// </summary>
	public class AppSettingsPrePost : System.Windows.Forms.Form
	{
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.GroupBox gbData;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.TextBox tbPre;
    private System.Windows.Forms.TextBox tbPost;
    private System.Windows.Forms.Label lblPre;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label lblTitleData;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AppSettingsPrePost()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

    public string Title
    {
      get {return lblTitleData.Text;}
      set {lblTitleData.Text = value;}
    }

    public string PreLaunch
    {
      get {return tbPre.Text;}
      set {tbPre.Text = value;}
    }

    public string PostLaunch
    {
      get {return tbPost.Text;}
      set {tbPost.Text = value;}
    }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AppSettingsPrePost));
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.gbData = new System.Windows.Forms.GroupBox();
      this.lblTitle = new System.Windows.Forms.Label();
      this.tbPre = new System.Windows.Forms.TextBox();
      this.tbPost = new System.Windows.Forms.TextBox();
      this.lblPre = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.lblTitleData = new System.Windows.Forms.Label();
      this.gbData.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(248, 376);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(80, 23);
      this.btnOk.TabIndex = 1;
      this.btnOk.Text = "OK";
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(336, 376);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Cancel";
      // 
      // gbData
      // 
      this.gbData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.gbData.Controls.Add(this.label1);
      this.gbData.Controls.Add(this.lblPre);
      this.gbData.Controls.Add(this.tbPost);
      this.gbData.Controls.Add(this.tbPre);
      this.gbData.Location = new System.Drawing.Point(8, 48);
      this.gbData.Name = "gbData";
      this.gbData.Size = new System.Drawing.Size(408, 312);
      this.gbData.TabIndex = 0;
      this.gbData.TabStop = false;
      this.gbData.Text = "Commands to execute before / after launching";
      // 
      // lblTitle
      // 
      this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.lblTitle.Location = new System.Drawing.Point(16, 16);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(72, 24);
      this.lblTitle.TabIndex = 6;
      this.lblTitle.Text = "Application: ";
      // 
      // tbPre
      // 
      this.tbPre.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbPre.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.tbPre.Location = new System.Drawing.Point(8, 48);
      this.tbPre.Multiline = true;
      this.tbPre.Name = "tbPre";
      this.tbPre.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.tbPre.Size = new System.Drawing.Size(392, 104);
      this.tbPre.TabIndex = 0;
      this.tbPre.Text = "tbPre";
      // 
      // tbPost
      // 
      this.tbPost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbPost.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.tbPost.Location = new System.Drawing.Point(8, 192);
      this.tbPost.Multiline = true;
      this.tbPost.Name = "tbPost";
      this.tbPost.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.tbPost.Size = new System.Drawing.Size(392, 112);
      this.tbPost.TabIndex = 1;
      this.tbPost.Text = "tbPost";
      // 
      // lblPre
      // 
      this.lblPre.Location = new System.Drawing.Point(8, 24);
      this.lblPre.Name = "lblPre";
      this.lblPre.Size = new System.Drawing.Size(100, 16);
      this.lblPre.TabIndex = 2;
      this.lblPre.Text = "Pre-Execute";
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(8, 169);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(112, 16);
      this.label1.TabIndex = 3;
      this.label1.Text = "Post-Execute";
      // 
      // lblTitleData
      // 
      this.lblTitleData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.lblTitleData.Location = new System.Drawing.Point(91, 16);
      this.lblTitleData.Name = "lblTitleData";
      this.lblTitleData.Size = new System.Drawing.Size(309, 16);
      this.lblTitleData.TabIndex = 7;
      this.lblTitleData.Text = "Title";
      // 
      // AppSettingsPrePost
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(424, 406);
      this.Controls.Add(this.lblTitleData);
      this.Controls.Add(this.lblTitle);
      this.Controls.Add(this.gbData);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.btnCancel);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "AppSettingsPrePost";
      this.Text = "Pre-Launch / Post-Launch Options";
      this.gbData.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion
	}
}
