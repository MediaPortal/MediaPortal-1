using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Xml;
using System.Text;
namespace MediaPortal
{
	/// <summary>
	/// Summary description for SearchCity.
	/// </summary>
	public class SearchCity : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxCity;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.Button btnCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private string m_strCity="";
		private System.Windows.Forms.Label labelCode;
		private string m_strCode="";

		public SearchCity()
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
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SearchCity));
      this.label1 = new System.Windows.Forms.Label();
      this.textBoxCity = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.labelCode = new System.Windows.Forms.Label();
      this.btnSearch = new System.Windows.Forms.Button();
      this.btnAdd = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 16);
      this.label1.Name = "label1";
      this.label1.TabIndex = 0;
      this.label1.Text = "City:";
      // 
      // textBoxCity
      // 
      this.textBoxCity.Location = new System.Drawing.Point(48, 16);
      this.textBoxCity.Name = "textBoxCity";
      this.textBoxCity.Size = new System.Drawing.Size(256, 20);
      this.textBoxCity.TabIndex = 0;
      this.textBoxCity.Text = "";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 48);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 16);
      this.label2.TabIndex = 2;
      this.label2.Text = "Shortcode:";
      // 
      // labelCode
      // 
      this.labelCode.Location = new System.Drawing.Point(96, 48);
      this.labelCode.Name = "labelCode";
      this.labelCode.Size = new System.Drawing.Size(112, 23);
      this.labelCode.TabIndex = 3;
      // 
      // btnSearch
      // 
      this.btnSearch.Location = new System.Drawing.Point(328, 16);
      this.btnSearch.Name = "btnSearch";
      this.btnSearch.Size = new System.Drawing.Size(56, 23);
      this.btnSearch.TabIndex = 1;
      this.btnSearch.Text = "Search";
      this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
      // 
      // btnAdd
      // 
      this.btnAdd.Location = new System.Drawing.Point(352, 64);
      this.btnAdd.Name = "btnAdd";
      this.btnAdd.Size = new System.Drawing.Size(48, 23);
      this.btnAdd.TabIndex = 3;
      this.btnAdd.Text = "Add";
      this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(288, 64);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(48, 23);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // SearchCity
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(416, 101);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnAdd);
      this.Controls.Add(this.btnSearch);
      this.Controls.Add(this.labelCode);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.textBoxCity);
      this.Controls.Add(this.label1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "SearchCity";
      this.Text = "SearchCity";
      this.Load += new System.EventHandler(this.SearchCity_Load);
      this.ResumeLayout(false);

    }
		#endregion


		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			m_strCity="";
			m_strCode="";
			Close();
		}

		private void btnAdd_Click(object sender, System.EventArgs e)
		{
			if (m_strCity.Length== 0) return;
			if (m_strCode.Length== 0) return;
			m_strCity=textBoxCity.Text;
			Close();
		}

		private void SearchCity_Load(object sender, System.EventArgs e)
		{
			btnAdd.Visible=false;
		}
		public string City
		{
			get { return m_strCity;}
		}
		public string ShortCode
		{
			get { return m_strCode;}
		}

		private void btnSearch_Click(object sender, System.EventArgs e)
		{
			if (textBoxCity.Text.Length==0) return;
			try
			{
				string strBody;
				string strURL=String.Format("http://xoap.weather.com/search/search?where={0}", textBoxCity.Text);
				WebRequest req = WebRequest.Create(strURL);
				WebResponse result = req.GetResponse();
				Stream ReceiveStream = result.GetResponseStream();
				Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
				StreamReader sr = new StreamReader( ReceiveStream, encode );
				strBody=sr.ReadToEnd();
				XmlDocument doc= new XmlDocument();
				doc.LoadXml(strBody);
				XmlNode nodeLoc = doc.DocumentElement.SelectSingleNode("/search/loc");
				if (nodeLoc!=null)
				{
					XmlNode nodeId = nodeLoc.Attributes.GetNamedItem("id");
					if (nodeId!=null)
					{
						textBoxCity.Text=nodeLoc.InnerText;
						m_strCity=textBoxCity.Text;
						m_strCode=nodeId.InnerText;
						labelCode.Text=m_strCode;
						btnAdd.Visible=true;
						return ;
					}
				}
			}
			catch (Exception)
			{
			}
			MessageBox.Show(this.Parent,"City not found", "Error",MessageBoxButtons.OK);
		}
	}
}
