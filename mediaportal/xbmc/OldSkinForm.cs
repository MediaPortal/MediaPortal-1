using System;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal
{
	/// <summary>
	/// Summary description for OldSkinForm.
	/// </summary>
	public class OldSkinForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.Button button1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public OldSkinForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(272, 32);
			this.label1.TabIndex = 0;
			this.label1.Text = "The current skin is not up-2-date. This can cause problems when using MP.";
			// 
			// checkBox1
			// 
			this.checkBox1.Location = new System.Drawing.Point(16, 96);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(120, 40);
			this.checkBox1.TabIndex = 0;
			this.checkBox1.Text = "Dont show this message again";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(216, 104);
			this.button1.Name = "button1";
			this.button1.TabIndex = 1;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// OldSkinForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(304, 141);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.label1);
			this.Name = "OldSkinForm";
			this.Text = "Warning! Old skin in use";
			this.ResumeLayout(false);

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e)
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlreader.SetValueAsBool("general", "dontshowskinversion", checkBox1.Checked);
			}
			this.Close();
		}
		
		public bool CheckSkinVersion(string skin)
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				bool ignoreErrors=false;
				ignoreErrors=xmlreader.GetValueAsBool("general", "dontshowskinversion", false);
				if (ignoreErrors) return true;
			}

			string versionMCE="";
			string versionSkin="";
			string filename=@"skin\mce\references.xml";
			if(File.Exists(filename))
			{	
				XmlDocument doc=new XmlDocument();
				doc.Load(filename);
				XmlNode node=doc.SelectSingleNode("/controls/skin/version");
				if (node!=null && node.InnerText!=null)
					versionMCE=node.InnerText;
			}
			filename=String.Format(@"skin\{0}\references.xml",skin);
			if(File.Exists(filename))
			{	
				XmlDocument doc=new XmlDocument();
				doc.Load(filename);
				XmlNode node=doc.SelectSingleNode("/controls/skin/version");
				if (node!=null && node.InnerText!=null)
					versionSkin=node.InnerText;
			}
			if (versionMCE==versionSkin) return true;
			return false;
		}
	}
}
