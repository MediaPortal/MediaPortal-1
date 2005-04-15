using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using MediaPortal.GUI.Library;

namespace MediaPortal
{
	/// <summary>
	/// Summary description for PluginForm.
	/// </summary>
	public class PluginForm : System.Windows.Forms.Form
	{
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
    private System.Windows.Forms.Button btnConfigure;
		public System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.Button buttonEnable;

    ArrayList m_plugins = new ArrayList();

		public PluginForm()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PluginForm));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.buttonEnable = new System.Windows.Forms.Button();
			this.btnConfigure = new System.Windows.Forms.Button();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.buttonEnable);
			this.groupBox1.Controls.Add(this.btnConfigure);
			this.groupBox1.Controls.Add(this.listView1);
			this.groupBox1.Location = new System.Drawing.Point(16, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(592, 304);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Plugins";
			this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
			// 
			// buttonEnable
			// 
			this.buttonEnable.Location = new System.Drawing.Point(112, 264);
			this.buttonEnable.Name = "buttonEnable";
			this.buttonEnable.TabIndex = 2;
			this.buttonEnable.Text = "Enable";
			this.buttonEnable.Click += new System.EventHandler(this.buttonEnable_Click);
			// 
			// btnConfigure
			// 
			this.btnConfigure.Location = new System.Drawing.Point(16, 264);
			this.btnConfigure.Name = "btnConfigure";
			this.btnConfigure.TabIndex = 1;
			this.btnConfigure.Text = "Configure";
			this.btnConfigure.Click += new System.EventHandler(this.btnConfigure_Click);
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																								this.columnHeader4,
																																								this.columnHeader1,
																																								this.columnHeader2,
																																								this.columnHeader3});
			this.listView1.FullRowSelect = true;
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(16, 24);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(552, 232);
			this.listView1.TabIndex = 0;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
			this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Enabled";
			this.columnHeader4.Width = 52;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 107;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Description";
			this.columnHeader2.Width = 290;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Author";
			this.columnHeader3.Width = 99;
			// 
			// PluginForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(648, 325);
			this.Controls.Add(this.groupBox1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "PluginForm";
			this.Text = "PluginForm";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.PluginForm_Closing);
			this.Load += new System.EventHandler(this.PluginForm_Load);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

    private void PluginForm_Load(object sender, System.EventArgs e)
    {
      string[] strFiles=System.IO.Directory.GetFiles(@"plugins\windows", "*.dll");
      foreach (string strFile in strFiles)
      {
        LoadPlugin(strFile);
      }
      strFiles=System.IO.Directory.GetFiles(@"plugins\subtitle", "*.dll");
      foreach (string strFile in strFiles)
      {
        LoadPlugin(strFile);
      }
      strFiles=System.IO.Directory.GetFiles(@"plugins\tagreaders", "*.dll");
      foreach (string strFile in strFiles)
      {
        LoadPlugin(strFile);
      }
      
      strFiles=System.IO.Directory.GetFiles(@"plugins\ExternalPlayers", "*.dll");
      foreach (string strFile in strFiles)
      {
        LoadPlugin(strFile);
      }
      
      for (int i=0; i < m_plugins.Count;++i)
      {
				string strEnabled=" ";
        ISetupForm form = (ISetupForm)m_plugins[i];
				if  (PlugInEnabled(form.PluginName(),form.DefaultEnabled()) ) strEnabled="X";

        ListViewItem item = listView1.Items.Add(strEnabled);
				
				item.SubItems.Add(form.PluginName());
				item.SubItems.Add(form.Description());
        item.SubItems.Add(form.Author());
      }
			listView1_SelectedIndexChanged(null,null);
    }
    
    public static bool MyInterfaceFilter(Type typeObj,Object criteriaObj)
    {
      if( typeObj.ToString() .Equals( criteriaObj.ToString()))
        return true;
      else
        return false;
    }


    public void LoadPlugin(string strFile)
    {
      try
      {
        Assembly assem = Assembly.LoadFrom(strFile);
        if (assem!=null)
        {
          Type[] types = assem.GetExportedTypes();

          foreach (Type t in types)
          {
            try
            {
              if (t.IsClass)
              {
                TypeFilter myFilter = new TypeFilter(MyInterfaceFilter);
                Type[] foundInterfaces=t.FindInterfaces(myFilter,"MediaPortal.GUI.Library.ISetupForm");
                if (foundInterfaces.Length>0)
                {
                  object newObj=(object)Activator.CreateInstance(t);
                  ISetupForm  setup=(ISetupForm)newObj;
                  m_plugins.Add( setup);
                  
                }
              }
            }
            catch (System.NullReferenceException)
            {
							
            }
          }
        }
      }
      catch (Exception ex)
      {
        string strEx=ex.Message;
      }
    }

    private void listView1_DoubleClick(object sender, System.EventArgs e)
    {
      if (listView1.SelectedItems.Count==0) return;
      int iItem=listView1.SelectedIndices[0];
      ISetupForm form = (ISetupForm)m_plugins[iItem];
      form.ShowPlugin();
    }

    private void btnConfigure_Click(object sender, System.EventArgs e)
    {
        listView1_DoubleClick(sender,e);
    }

		private void groupBox1_Enter(object sender, System.EventArgs e)
		{
		
		}
		private bool PlugInEnabled(string strPlugin, bool bDefault)
		{
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				string strValue=xmlreader.GetValueAsString("plugins",strPlugin,"");
				if (strValue.Length==0)
				{
					return bDefault;
				}
				bool bEnabled=xmlreader.GetValueAsBool("plugins",strPlugin,false);
				return bEnabled;
			}
		}

		private void buttonEnable_Click(object sender, System.EventArgs e)
		{
			if (listView1.SelectedItems.Count==0) return;
			int iItem=listView1.SelectedIndices[0];
			string strText=		listView1.Items[iItem].SubItems[0].Text;
			if (strText.Equals(" ")) listView1.Items[iItem].SubItems[0].Text="X";
			else listView1.Items[iItem].SubItems[0].Text=" ";

			listView1_SelectedIndexChanged(sender,e);
		
		}

		private void listView1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (listView1.SelectedItems.Count==0) 
			{
				buttonEnable.Enabled=false;
				return;
			}
			int iItem=listView1.SelectedIndices[0];
			
			ISetupForm form = (ISetupForm )m_plugins[iItem];
			if (form.CanEnable()==false)
			{
				buttonEnable.Enabled=false;
				return;
			}

			buttonEnable.Enabled=true;
			string strText=		listView1.Items[iItem].SubItems[0].Text;
			if (strText.Equals(" ")) buttonEnable.Text="Enable";
			else buttonEnable.Text="Disable";
		}

		private void PluginForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			using (MediaPortal.Profile.Xml   xmlWriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				for (int i=0; i < listView1.Items.Count;++i)
				{
					ISetupForm form = (ISetupForm )m_plugins[i];
					if (form.CanEnable())
					{
						bool bEnabled=false;
						string strText=		listView1.Items[i].SubItems[0].Text;	
						if (strText=="X") bEnabled=true;

						string strPlugIn=listView1.Items[i].SubItems[1].Text;	
						xmlWriter.SetValueAsBool("plugins", strPlugIn,bEnabled);
					}
				}
			}
		}
	}
}
