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
      this.listView1 = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.btnConfigure = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.btnConfigure);
      this.groupBox1.Controls.Add(this.listView1);
      this.groupBox1.Location = new System.Drawing.Point(16, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(504, 304);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Plugins";
      // 
      // listView1
      // 
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                this.columnHeader1,
                                                                                this.columnHeader2,
                                                                                this.columnHeader3});
      this.listView1.FullRowSelect = true;
      this.listView1.Location = new System.Drawing.Point(16, 24);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(472, 232);
      this.listView1.TabIndex = 0;
      this.listView1.View = System.Windows.Forms.View.Details;
      this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Name";
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
      // btnConfigure
      // 
      this.btnConfigure.Location = new System.Drawing.Point(16, 264);
      this.btnConfigure.Name = "btnConfigure";
      this.btnConfigure.TabIndex = 1;
      this.btnConfigure.Text = "Configure";
      this.btnConfigure.Click += new System.EventHandler(this.btnConfigure_Click);
      // 
      // PluginForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(536, 325);
      this.Controls.Add(this.groupBox1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "PluginForm";
      this.Text = "PluginForm";
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
      

      
      for (int i=0; i < m_plugins.Count;++i)
      {
        ISetupForm form = (ISetupForm)m_plugins[i];
        ListViewItem item = listView1.Items.Add(form.PluginName());
        item.SubItems.Add(form.Description());
        item.SubItems.Add(form.Author());
      }
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
	}
}
