using System;
using System.Drawing.Design;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Mpe.Controls;

namespace Mpe.Controls.Design
{
	#region MpeStringEditorForm
	/// <summary>
	/// Summary description for StringSelector.
	/// </summary>
	public class MpeStringEditorForm : System.Windows.Forms.UserControl {
	
		#region Variables
		private System.Windows.Forms.ColumnHeader colId;
		private System.Windows.Forms.ColumnHeader colString;
		private System.Windows.Forms.ListView stringList;
		private IWindowsFormsEditorService editorService;
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.ListBox propertyList;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton simpleRadio;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.TextBox simpleTextbox;
		private System.Windows.Forms.RadioButton propRadio;
		private System.Windows.Forms.RadioButton stringRadio;
		private System.Windows.Forms.Panel simplePanel;
		private System.Windows.Forms.Panel propPanel;
		private System.Windows.Forms.Panel stringPanel;
		private static ArrayList properties;
		#endregion

		#region Constructors
		public MpeStringEditorForm(string currentValue, MpeParser parser, IWindowsFormsEditorService editorService) {
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			InitializeComponent();
			Height = 184;
			propPanel.Location = simplePanel.Location;
			propPanel.Size = simplePanel.Size;
			stringPanel.Location = simplePanel.Location;
			stringPanel.Size = simplePanel.Size;
			propPanel.Visible = false;
			stringPanel.Visible = false;
			simpleTextbox.Text = currentValue;
			if (properties == null) {
				properties = new ArrayList();
				properties.Add("itemcount");
				properties.Add("selecteditem");
				properties.Add("selecteditem2");
				properties.Add("selectedthumb");
				properties.Add("title");
				properties.Add("artist");
				properties.Add("album");
				properties.Add("track");
				properties.Add("year");
				properties.Add("comment");
				properties.Add("director");
				properties.Add("genre");
				properties.Add("cast");
				properties.Add("dvdlabel");
				properties.Add("imdbnumber");
				properties.Add("file");
				properties.Add("plot");
				properties.Add("plotoutline");
				properties.Add("rating");
				properties.Add("tagline");
				properties.Add("votes");
				properties.Add("credits");
				properties.Add("thumb");
				properties.Add("currentplaytime");
				properties.Add("shortcurrentplaytime");
				properties.Add("duration");
				properties.Add("shortduration");
				properties.Add("playlogo");
				properties.Add("playspeed");
				properties.Add("percentage");
				properties.Add("currentmodule");
				properties.Add("channel");
				properties.Add("TV.start");
				properties.Add("TV.stop");
				properties.Add("TV.current");
				properties.Add("TV.Record.channel");
				properties.Add("TV.Record.start");
				properties.Add("TV.Record.stop");
				properties.Add("TV.Record.genre");
				properties.Add("TV.Record.title");
				properties.Add("TV.Record.description");
				properties.Add("TV.Record.thumb");
				properties.Add("TV.View.channel");    
				properties.Add("TV.View.thumb");      
				properties.Add("TV.View.start");      
				properties.Add("TV.View.stop");       
				properties.Add("TV.View.genre");      
				properties.Add("TV.View.title");      
				properties.Add("TV.View.description");
				properties.Add("TV.View.Percentage");
				properties.Add("TV.Guide.Day");          
				properties.Add("TV.Guide.thumb");        
				properties.Add("TV.Guide.Title");        
				properties.Add("TV.Guide.Time");         
				properties.Add("TV.Guide.Duration");
				properties.Add("TV.Guide.TimeFromNow");
				properties.Add("TV.Guide.Description");  
				properties.Add("TV.Guide.Genre");        
				properties.Add("TV.Guide.EpisodeName");        
				properties.Add("TV.Guide.SeriesNumber");        
				properties.Add("TV.Guide.EpisodeNumber");        
				properties.Add("TV.Guide.EpisodePart");        
				properties.Add("TV.Guide.EpisodeDetail");        
				properties.Add("TV.Guide.Date");        
				properties.Add("TV.Guide.StarRating");        
				properties.Add("TV.Guide.Classification");
				properties.Add("TV.RecordedTV.Title");              
				properties.Add("TV.RecordedTV.Time");               
				properties.Add("TV.RecordedTV.Description");        
				properties.Add("TV.RecordedTV.thumb");              
				properties.Add("TV.RecordedTV.Genre");  
				properties.Add("TV.Scheduled.Title");              
				properties.Add("TV.Scheduled.Time");               
				properties.Add("TV.Scheduled.Description");        
				properties.Add("TV.Scheduled.thumb");              
				properties.Add("TV.Scheduled.Genre");       
				properties.Add("TV.Search.Title");              
				properties.Add("TV.Search.Time");               
				properties.Add("TV.Search.Description");        
				properties.Add("TV.Search.thumb");              
				properties.Add("TV.Search.Genre");   
			}
			this.editorService = editorService;
			MpeStringTable table = parser.GetStringTable("English");
			int[] keys = table.Keys;
			ListViewItem sel = null;
			for (int i = 0; i < keys.Length; i++) {
				string s = table[keys[i]];
				//ListViewItem item = stringList.Items.Add(s);
				ListViewItem item = stringList.Items.Add(keys[i].ToString("D6"));
				item.Tag = keys[i];
				//item.SubItems.Add(keys[i].ToString("D6"));
				item.SubItems.Add(s);
				if (currentValue.Equals(keys[i].ToString())) {
					item.Selected = true;
					sel = item;
				}
			}
			if (sel != null) {
				MpeLog.Debug("Is the selected string visible?");
				sel.EnsureVisible();
			}
			propertyList.DataSource = properties;
		}
		#endregion

		#region Properties
		public string SelectedValue {
			get {
				return ((int)stringList.SelectedItems[0].Tag).ToString();
			}
		}
		#endregion

		#region Methods
		public void Close() {
			if (editorService != null)
				editorService.CloseDropDown();
		}
		#endregion

		#region Event Handlers
		private void OnMouseWheel(object sender, MouseEventArgs e) {
			int i = 0;
			if (e.Delta > 0)
				i = stringList.SelectedIndices[0] - 1;
			else
				i = stringList.SelectedIndices[0] + 1;
			if (i < 0)
				i = 0;
			else if (i >= stringList.Items.Count)
				i = stringList.Items.Count - 1;
			stringList.Items[i].Selected = true;
			stringList.Items[i].EnsureVisible();
		}
		private void OnDoubleClick(object sender, System.EventArgs e) {
			Close();
		}
		#endregion

		#region Component Designer Generated Code
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.stringList = new System.Windows.Forms.ListView();
			this.colId = new System.Windows.Forms.ColumnHeader();
			this.colString = new System.Windows.Forms.ColumnHeader();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.propertyList = new System.Windows.Forms.ListBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.propRadio = new System.Windows.Forms.RadioButton();
			this.stringRadio = new System.Windows.Forms.RadioButton();
			this.simpleRadio = new System.Windows.Forms.RadioButton();
			this.simplePanel = new System.Windows.Forms.Panel();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.simpleTextbox = new System.Windows.Forms.TextBox();
			this.propPanel = new System.Windows.Forms.Panel();
			this.stringPanel = new System.Windows.Forms.Panel();
			this.groupBox1.SuspendLayout();
			this.simplePanel.SuspendLayout();
			this.propPanel.SuspendLayout();
			this.stringPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// stringList
			// 
			this.stringList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																												 this.colId,
																												 this.colString});
			this.stringList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stringList.FullRowSelect = true;
			this.stringList.Location = new System.Drawing.Point(0, 0);
			this.stringList.MultiSelect = false;
			this.stringList.Name = "stringList";
			this.stringList.Size = new System.Drawing.Size(272, 100);
			this.stringList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.stringList.TabIndex = 0;
			this.stringList.View = System.Windows.Forms.View.Details;
			this.stringList.DoubleClick += new System.EventHandler(this.OnDoubleClick);
			this.stringList.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.OnMouseWheel);
			// 
			// colId
			// 
			this.colId.Text = "Id";
			this.colId.Width = 46;
			// 
			// colString
			// 
			this.colString.Text = "Value";
			this.colString.Width = 170;
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(120, 156);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 2;
			this.okButton.Text = "OK";
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(200, 156);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			// 
			// propertyList
			// 
			this.propertyList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyList.Location = new System.Drawing.Point(0, 0);
			this.propertyList.Name = "propertyList";
			this.propertyList.Size = new System.Drawing.Size(272, 69);
			this.propertyList.TabIndex = 4;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.propRadio);
			this.groupBox1.Controls.Add(this.stringRadio);
			this.groupBox1.Controls.Add(this.simpleRadio);
			this.groupBox1.Location = new System.Drawing.Point(8, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(272, 48);
			this.groupBox1.TabIndex = 5;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Selection";
			// 
			// propRadio
			// 
			this.propRadio.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.propRadio.Location = new System.Drawing.Point(184, 16);
			this.propRadio.Name = "propRadio";
			this.propRadio.Size = new System.Drawing.Size(80, 24);
			this.propRadio.TabIndex = 2;
			this.propRadio.Text = "Properties";
			this.propRadio.Click += new System.EventHandler(this.OnPropRadioClicked);
			// 
			// stringRadio
			// 
			this.stringRadio.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.stringRadio.Location = new System.Drawing.Point(96, 16);
			this.stringRadio.Name = "stringRadio";
			this.stringRadio.Size = new System.Drawing.Size(64, 24);
			this.stringRadio.TabIndex = 1;
			this.stringRadio.Text = "Strings";
			this.stringRadio.Click += new System.EventHandler(this.OnStringRadioClicked);
			// 
			// simpleRadio
			// 
			this.simpleRadio.Checked = true;
			this.simpleRadio.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.simpleRadio.Location = new System.Drawing.Point(8, 16);
			this.simpleRadio.Name = "simpleRadio";
			this.simpleRadio.Size = new System.Drawing.Size(64, 24);
			this.simpleRadio.TabIndex = 0;
			this.simpleRadio.TabStop = true;
			this.simpleRadio.Text = "Simple";
			this.simpleRadio.Click += new System.EventHandler(this.OnSimpleRadioClicked);
			// 
			// simplePanel
			// 
			this.simplePanel.Controls.Add(this.checkBox1);
			this.simplePanel.Controls.Add(this.label1);
			this.simplePanel.Controls.Add(this.simpleTextbox);
			this.simplePanel.Location = new System.Drawing.Point(8, 48);
			this.simplePanel.Name = "simplePanel";
			this.simplePanel.Size = new System.Drawing.Size(272, 104);
			this.simplePanel.TabIndex = 6;
			// 
			// checkBox1
			// 
			this.checkBox1.Enabled = false;
			this.checkBox1.Location = new System.Drawing.Point(48, 32);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(216, 16);
			this.checkBox1.TabIndex = 2;
			this.checkBox1.Text = "Add to String Table";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(26, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Text";
			// 
			// simpleTextbox
			// 
			this.simpleTextbox.AutoSize = false;
			this.simpleTextbox.Location = new System.Drawing.Point(48, 5);
			this.simpleTextbox.Name = "simpleTextbox";
			this.simpleTextbox.Size = new System.Drawing.Size(216, 20);
			this.simpleTextbox.TabIndex = 0;
			this.simpleTextbox.Text = "";
			// 
			// propPanel
			// 
			this.propPanel.Controls.Add(this.propertyList);
			this.propPanel.Location = new System.Drawing.Point(8, 232);
			this.propPanel.Name = "propPanel";
			this.propPanel.Size = new System.Drawing.Size(272, 72);
			this.propPanel.TabIndex = 7;
			// 
			// stringPanel
			// 
			this.stringPanel.Controls.Add(this.stringList);
			this.stringPanel.Location = new System.Drawing.Point(8, 312);
			this.stringPanel.Name = "stringPanel";
			this.stringPanel.Size = new System.Drawing.Size(272, 100);
			this.stringPanel.TabIndex = 8;
			// 
			// MpeStringEditorForm
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.stringPanel);
			this.Controls.Add(this.simplePanel);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.propPanel);
			this.Name = "MpeStringEditorForm";
			this.Size = new System.Drawing.Size(288, 440);
			this.groupBox1.ResumeLayout(false);
			this.simplePanel.ResumeLayout(false);
			this.propPanel.ResumeLayout(false);
			this.stringPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void OnSimpleRadioClicked(object sender, System.EventArgs e) {
			if (simpleRadio.Checked) {
				simplePanel.Visible = true;
				propPanel.Visible = false;
				stringPanel.Visible = false;
			}
		}

		private void OnPropRadioClicked(object sender, System.EventArgs e) {
			if (propRadio.Checked) {
				propPanel.Visible = true;
				simplePanel.Visible = false;
				stringPanel.Visible = false;
			}
		}

		private void OnStringRadioClicked(object sender, System.EventArgs e) {
			if (stringRadio.Checked) {
				stringPanel.Visible = true;
				simplePanel.Visible = false;
				propPanel.Visible = false;
			}
		}



	}
	#endregion

	#region MpeStringEditor
	public class MpeStringEditor : UITypeEditor {
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			return UITypeEditorEditStyle.DropDown;
		}
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (context.Instance is MpeControl) {
				try {
					MpeControl mpc = (MpeControl)context.Instance;
					IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
					MpeStringEditorForm selector = new MpeStringEditorForm((string)value, mpc.Parser, editorService);
					editorService.DropDownControl(selector);
					return selector.SelectedValue;
				} catch (Exception ee) {
					MpeLog.Debug(ee);
					MpeLog.Error(ee);
				}
			} else if (context.Instance is MpeItem) {
				try {
					IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
					MpeStringEditorForm selector = new MpeStringEditorForm((string)value, MediaPortalEditor.Global.Parser, editorService);
					editorService.DropDownControl(selector);
					return selector.SelectedValue;
				} catch (Exception ee) {
					MpeLog.Debug(ee);
					MpeLog.Error(ee);
				}
			}
			return base.EditValue (context, provider, value);
		}
	}
	#endregion
}
