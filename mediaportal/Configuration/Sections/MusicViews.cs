using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Configuration.Controls;
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
		private DataGridTableStyle GridTableStyle;

		ViewDefinition currentView ;
		ArrayList views;
		DataSet ds = new DataSet();

		enum WhereTypes
		{
			album,
			artist,
			title,
			genre,
			year,
			track,
			timesplayed,
			rating,
			favorites 
		}
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

			string[] selections= new string[]
			{
					"album",
					"artist",
					"title",
					"genre",
					"year",
					"track",
					"timesplayed",
					"rating",
					"favorites"
			};
			string[] sqloperators = new string[]
			{
				"",
				"=",
				">",
			  "<",
			  ">=",
				"<=",
				"<>",
				"like",

			};
			
			//Declare and initialize local variables used
			DataColumn dtCol = null;//Data Column variable
			string[]   arrColumnNames = null;//string array variable
			System.Windows.Forms.ComboBox cbSelection, cbOperators;  //combo box var              
			DataTable datasetFilters;//Data Table var
           
			//Create the combo box object and set its properties
			cbSelection               = new ComboBox();
			cbSelection.Cursor        = System.Windows.Forms.Cursors.Arrow;
			cbSelection.DropDownStyle=System.Windows.Forms.ComboBoxStyle.DropDownList;
			cbSelection.Dock          = DockStyle.Fill;
			foreach (string strText in selections)
				cbSelection.Items.Add(strText);
			//Event that will be fired when selected index in the combo box is changed
			cbSelection.SelectionChangeCommitted +=new EventHandler(cbSelection_SelectionChangeCommitted);

			
			//Create the combo box object and set its properties
			cbOperators               = new ComboBox();
			cbOperators.Cursor        = System.Windows.Forms.Cursors.Arrow;
			cbOperators.DropDownStyle=System.Windows.Forms.ComboBoxStyle.DropDownList;
			cbOperators.Dock          = DockStyle.Fill;
			foreach (string strText in sqloperators)
				cbOperators.Items.Add(strText);
     
			//Create the String array object, initialize the array with the column
			//names to be displayed
			arrColumnNames          = new string [4];
			arrColumnNames[0]       = "Selection";
			arrColumnNames[1]       = "Operator";
			arrColumnNames[2]       = "Restriction";
			arrColumnNames[3]       = "Limit";
     
			//Create the Data Table object which will then be used to hold
			//columns and rows
			datasetFilters            = new DataTable ("FunctionArea");
			//Add the string array of columns to the DataColumn object       
			for(int i=0; i< arrColumnNames.Length;i++)
			{    
				string str        = arrColumnNames[i];
				dtCol             = new DataColumn(str);
				dtCol.DataType          = System.Type.GetType("System.String");
				dtCol.DefaultValue      = "";
				datasetFilters.Columns.Add(dtCol);               
			}     

			//Add a Column with checkbox at last in the Grid     
			DataColumn dtcCheck    = new DataColumn("Sort Ascending");//create the data          //column object with the name 
			dtcCheck.DataType      = System.Type.GetType("System.Boolean");//Set its //data Type
			dtcCheck.DefaultValue  = false;//Set the default value
			datasetFilters.Columns.Add(dtcCheck);//Add the above column to the //Data Table
  
			//fill in all rows...
			for (int i=0; i < currentView.Filters.Count;++i)
			{
				FilterDefinition def = (FilterDefinition)currentView.Filters[i];
				datasetFilters.Rows.Add( new object[] { def.Where,def.SqlOperator,def.Restriction,def.Limit,def.SortAscending});
			}	

			//Set the Data Grid Source as the Data Table created above
			dataGrid1.CaptionText=String.Empty;
			dataGrid1.DataSource    = datasetFilters; 

			//set style property when first time the grid loads, next time onwards it //will maintain its property
			if(!dataGrid1.TableStyles.Contains("FunctionArea"))
			{
				//Create a DataGridTableStyle object     
				DataGridTableStyle dgdtblStyle      = new DataGridTableStyle();
				//Set its properties
				dgdtblStyle.MappingName            = datasetFilters.TableName;//its table name of dataset
				dataGrid1.TableStyles.Add(dgdtblStyle);
				dgdtblStyle.RowHeadersVisible       = false;
				dgdtblStyle.HeaderBackColor         = Color.LightSteelBlue;
				dgdtblStyle.AllowSorting            = false;
				dgdtblStyle.HeaderBackColor         = Color.FromArgb(8,36,107);
				dgdtblStyle.RowHeadersVisible       = false;
				dgdtblStyle.HeaderForeColor         = Color.White;
				dgdtblStyle.HeaderFont              = new System.Drawing.Font("Microsoft Sans Serif", 9F,  System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				dgdtblStyle.GridLineColor           = Color.DarkGray;
				dgdtblStyle.PreferredRowHeight            = 22;
				dataGrid1.BackgroundColor           = Color.White; 

				//Take the columns in a GridColumnStylesCollection object and set //the size of the
				//individual columns   
				GridColumnStylesCollection    colStyle;
				colStyle                = dataGrid1.TableStyles[0].GridColumnStyles;
				colStyle[0].Width       = 100;
				colStyle[1].Width       = 50;
				colStyle[2].Width       = 50;
				colStyle[3].Width       = 80;
			}
			DataGridTextBoxColumn dgtb    =     (DataGridTextBoxColumn)dataGrid1.TableStyles[0].GridColumnStyles[0];
			//Add the combo box to the text box taken in the above step 
			dgtb.TextBox.Controls.Add (cbSelection);        

			dgtb    =     (DataGridTextBoxColumn)dataGrid1.TableStyles[0].GridColumnStyles[1];
			dgtb.TextBox.Controls.Add (cbOperators);        
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

		private void cbSelect_TextChanged(object sender, EventArgs e)
		{

		}

		private void cbSelection_SelectionChangeCommitted(object sender, EventArgs e)
		{

		}
	}
}

