using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;

namespace MediaPortal.Configuration.Sections
{
	public class MusicViews : MediaPortal.Configuration.SectionSettings
	{
    private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cbViews;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnDelete;
		private System.ComponentModel.IContainer components = null;
		
		ViewDefinition currentView ;
		ArrayList views;
		DataSet ds = new DataSet();

    public MusicViews() :  this("Music Views")
    {
    }

    public MusicViews(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			LoadViews();
		}

		void LoadViews()
		{
			views=new ArrayList();
			using(FileStream fileStream = new FileStream("musicViews.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				try
				{
					SoapFormatter formatter = new SoapFormatter();
					views = (ArrayList)formatter.Deserialize(fileStream);
					fileStream.Close();
				}
				catch
				{
				}
			}
			cbViews.Items.Clear();
			foreach (ViewDefinition view in views)
			{
				cbViews.Items.Add(view.Name);
			}
			if (cbViews.Items.Count>0)
				cbViews.SelectedIndex=0;
													
			UpdateView( );	
		}
		void UpdateView()
		{
			int index=cbViews.SelectedIndex;
			if (index < 0) return;
			currentView =(ViewDefinition) views[index]; 

			ds = new DataSet();
			ds.Tables.Add("filters");
			ds.Tables[0].Columns.Add("Selection",typeof(string));
			ds.Tables[0].Columns.Add("Operator",typeof(string));
			ds.Tables[0].Columns.Add("Restriction",typeof(string));
			ds.Tables[0].Columns.Add("Limit",typeof(int));
			ds.Tables[0].Columns.Add("Sort",typeof(bool));

			for (int i=0; i < currentView.Filters.Count;++i)
			{
				FilterDefinition def = (FilterDefinition)currentView.Filters[i];
				ds.Tables[0].Rows.Add( new object[] { def.Where,def.SqlOperator,def.Restriction,def.Limit,def.SortAscending});
			}
			ds.Tables[0].DefaultView.AllowNew = true;
			ds.Tables[0].DefaultView.AllowDelete = true; 
			dataGrid1.DataSource=ds.Tables[0];
		}


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.dataGrid1 = new System.Windows.Forms.DataGrid();
			this.label1 = new System.Windows.Forms.Label();
			this.cbViews = new System.Windows.Forms.ComboBox();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.btnDelete);
			this.groupBox1.Controls.Add(this.btnSave);
			this.groupBox1.Controls.Add(this.cbViews);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.dataGrid1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 432);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Music Views";
			// 
			// dataGrid1
			// 
			this.dataGrid1.DataMember = "";
			this.dataGrid1.FlatMode = true;
			this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid1.Location = new System.Drawing.Point(16, 64);
			this.dataGrid1.Name = "dataGrid1";
			this.dataGrid1.Size = new System.Drawing.Size(408, 312);
			this.dataGrid1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "View:";
			// 
			// cbViews
			// 
			this.cbViews.Location = new System.Drawing.Point(80, 24);
			this.cbViews.Name = "cbViews";
			this.cbViews.Size = new System.Drawing.Size(168, 21);
			this.cbViews.TabIndex = 2;
			this.cbViews.SelectedIndexChanged += new System.EventHandler(this.cbViews_SelectedIndexChanged);
			// 
			// btnSave
			// 
			this.btnSave.Location = new System.Drawing.Point(320, 384);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(48, 23);
			this.btnSave.TabIndex = 3;
			this.btnSave.Text = "Save";
			// 
			// btnDelete
			// 
			this.btnDelete.Location = new System.Drawing.Point(376, 384);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(48, 23);
			this.btnDelete.TabIndex = 4;
			this.btnDelete.Text = "Delete";
			// 
			// MusicViews
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "MusicViews";
			this.Size = new System.Drawing.Size(456, 448);
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void cbViews_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			UpdateView();
		}

	}
}

