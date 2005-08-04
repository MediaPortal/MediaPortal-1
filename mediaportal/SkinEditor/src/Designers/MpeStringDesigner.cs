using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using Mpe.Controls;

namespace Mpe.Designers
{
	/// <summary>
	/// Summary description for MpeStringDesigner.
	/// </summary>
	public class MpeStringDesigner : UserControl, MpeDesigner {
		#region Variables
		private System.ComponentModel.Container components = null;
		private MediaPortalEditor mpe;
		private MpeStringTable referenceTable;
		private System.Windows.Forms.DataGrid dataGrid;
		private MpeStringTable stringTable;
		private MpeData dataSet;
		private MpeData.MpeDbStringDataTable dataTable;
		#endregion

		#region Contructors
		public MpeStringDesigner(MediaPortalEditor mpe, MpeStringTable referenceTable, MpeStringTable stringTable) {
			this.mpe = mpe;
			InitializeComponent();
			if (referenceTable == null)
				throw new DesignerException("Invalid reference string table");
			this.referenceTable = referenceTable;
			this.stringTable = stringTable;
			this.dataSet = new MpeData();
			this.dataTable = new MpeData.MpeDbStringDataTable();
		}
		#endregion
		
		#region Methods - Designer
		public void Initialize() {
			dataTable.ReferenceColumn.ReadOnly = true;
			dataTable.RowChanged += new DataRowChangeEventHandler(OnStringTableRowChanged);

			int[] keys = referenceTable.Keys;
			for (int i = 0; i < keys.Length; i++) {
				string s1 = referenceTable[keys[i]];
				if (s1 == null)
					s1 = "";
				string s2 = stringTable[keys[i]];
				if (s2 == null)
					s2 = "";
				dataTable.AddMpeDbStringRow(keys[i],s1,s2);
			}
			dataGrid.DataSource = dataTable;		
			dataGrid.CaptionVisible = false;			
		}
		public void Save() {
			MpeLog.Debug("MpeStringDesigner.Save()");
			try {
				stringTable.Clear();
				DataRow[] rows = dataTable.Select("","Id");
				for (int i = 0; rows != null && i < rows.Length; i++) {
					MpeData.MpeDbStringRow r = (MpeData.MpeDbStringRow)rows[i];
					stringTable.Add(r.Id, r.Value);
				}
				mpe.Parser.SaveStringTable(stringTable);
				mpe.ToggleDesignerStatus(ResourceName, false);
			} catch (Exception ee) {
				MpeLog.Debug(ee);
				MpeLog.Error(ee);
			}
		}
		public void Cancel() {
			//
		}
		public void Destroy() {
			dataTable.Dispose();
			dataSet.Dispose();
		}
		public void Pause() {
			//
		}
		public void Resume() {
			//
		}
		#endregion

		#region Properties - Designer
		public string ResourceName {
			get {
				return stringTable.Language;
			}
		}
		public bool AllowAdditions {
			get {
				return false;
			}
		}
		public bool AllowDeletions {
			get {
				return false;
			}
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
		private void InitializeComponent() {
			this.dataGrid = new System.Windows.Forms.DataGrid();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGrid
			// 
			this.dataGrid.AllowNavigation = false;
			this.dataGrid.AlternatingBackColor = System.Drawing.SystemColors.Window;
			this.dataGrid.BackgroundColor = System.Drawing.Color.LightGray;
			this.dataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dataGrid.DataMember = "";
			this.dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGrid.GridLineColor = System.Drawing.SystemColors.Control;
			this.dataGrid.HeaderBackColor = System.Drawing.SystemColors.Control;
			this.dataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.dataGrid.Location = new System.Drawing.Point(4, 4);
			this.dataGrid.Name = "dataGrid";
			this.dataGrid.SelectionBackColor = System.Drawing.SystemColors.ActiveCaption;
			this.dataGrid.SelectionForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.dataGrid.Size = new System.Drawing.Size(368, 216);
			this.dataGrid.TabIndex = 0;
			// 
			// MpeStringDesigner
			// 
			this.Controls.Add(this.dataGrid);
			this.DockPadding.All = 4;
			this.Name = "MpeStringDesigner";
			this.Size = new System.Drawing.Size(376, 224);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void OnStringTableRowChanged(object sender, DataRowChangeEventArgs e) {
			mpe.ToggleDesignerStatus(ResourceName, true);
		}
	}
}
