using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal
{
	/// <summary>
	/// Summary description for NewShare.
	/// </summary>
	public class formNewShare : System.Windows.Forms.Form
	{
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox shareName;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Label Folder;
    private System.Windows.Forms.Button btnSelectFolder;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnAdd;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
    string m_strPath=null;
    string m_strName=null;

		public formNewShare()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

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
      this.label1 = new System.Windows.Forms.Label();
      this.shareName = new System.Windows.Forms.TextBox();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.Folder = new System.Windows.Forms.Label();
      this.btnSelectFolder = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnAdd = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 16);
      this.label1.Name = "label1";
      this.label1.TabIndex = 0;
      this.label1.Text = "Name";
      // 
      // shareName
      // 
      this.shareName.Location = new System.Drawing.Point(64, 16);
      this.shareName.Name = "shareName";
      this.shareName.TabIndex = 0;
      this.shareName.Text = "";
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(64, 48);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(264, 20);
      this.textBox1.TabIndex = 1;
      this.textBox1.Text = "";
      // 
      // Folder
      // 
      this.Folder.Location = new System.Drawing.Point(16, 48);
      this.Folder.Name = "Folder";
      this.Folder.Size = new System.Drawing.Size(40, 23);
      this.Folder.TabIndex = 3;
      this.Folder.Text = "Folder";
      // 
      // btnSelectFolder
      // 
      this.btnSelectFolder.Location = new System.Drawing.Point(344, 48);
      this.btnSelectFolder.Name = "btnSelectFolder";
      this.btnSelectFolder.Size = new System.Drawing.Size(32, 23);
      this.btnSelectFolder.TabIndex = 2;
      this.btnSelectFolder.Text = "...";
      this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(208, 88);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.TabIndex = 3;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // btnAdd
      // 
      this.btnAdd.Location = new System.Drawing.Point(304, 88);
      this.btnAdd.Name = "btnAdd";
      this.btnAdd.TabIndex = 4;
      this.btnAdd.Text = "Add";
      this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
      // 
      // formNewShare
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(392, 125);
      this.Controls.Add(this.btnAdd);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnSelectFolder);
      this.Controls.Add(this.Folder);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.shareName);
      this.Controls.Add(this.label1);
      this.Name = "formNewShare";
      this.Text = "Select folder";
      this.Load += new System.EventHandler(this.formNewShare_Load);
      this.ResumeLayout(false);

    }
		#endregion

    private void btnSelectFolder_Click(object sender, System.EventArgs e)
    {
      FolderBrowserDialog dlg=new FolderBrowserDialog();
      dlg.ShowNewFolderButton=true;
      dlg.ShowDialog(this);
      if (dlg.SelectedPath==null) return;
      textBox1.Text=dlg.SelectedPath;
    }

    private void btnAdd_Click(object sender, System.EventArgs e)
    {
      if (textBox1.Text.Length==0) return;
      if (shareName.Text.Length==0) return;
      m_strPath=textBox1.Text;
      m_strName=shareName.Text;
      this.Close();
    }

    private void btnCancel_Click(object sender, System.EventArgs e)
    {
      this.Close();
    }

    private void formNewShare_Load(object sender, System.EventArgs e)
    {
      if (m_strPath!=null)
      {
        textBox1.Text=m_strPath;
      }
      if (m_strName!=null)
      {
        shareName.Text=m_strName;
      }
    
    }
  
    public string SelectedPath
    {
      get { return m_strPath;}
      set {m_strPath=value;}
    }
    public string ShareName
    {
      get { return m_strName;}
      set {m_strName=value;}
    }
	}
}
