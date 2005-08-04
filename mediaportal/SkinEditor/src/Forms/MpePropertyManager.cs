using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

using Crownwood.Magic.Menus;

using Mpe.Controls;
using Mpe.Forms;

namespace Mpe.Forms
{
	/// <summary>
	/// Summary description for SkinPropertiesControl.
	/// </summary>
	public class MpePropertyManager : UserControl {
	
		#region Variables
		private System.Windows.Forms.PropertyGrid propertyGrid;
		//private MpePropertyGrid propertyGrid;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ComboBox resourceList;
		private MediaPortalEditor mpe;
		private System.Windows.Forms.ImageList menuImageList;
		private Crownwood.Magic.Menus.PopupMenu contextMenu;
		#endregion
		
		#region Events and Delegates
		public delegate void ResourceListSelectionChangedHandler(MpeResource selectedControl);
		public event ResourceListSelectionChangedHandler ResourceListSelectionChanged;
		#endregion

		#region Contructors
		public MpePropertyManager(MediaPortalEditor mpe) {
			SetStyle(ControlStyles.EnableNotifyMessage, true);
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			this.mpe = mpe;
			contextMenu = new PopupMenu();
			MenuCommand category = new MenuCommand("Sort By Category", menuImageList, 0, new EventHandler(this.OnMenuCategoryClicked));
			MenuCommand name = new MenuCommand("Sort By Name", menuImageList, 1, new EventHandler(this.OnMenuNameClicked));
			contextMenu.MenuCommands.AddRange(new MenuCommand[] { category, name } );
			propertyGrid.CommandsVisibleIfAvailable = true;
		}
		#endregion

		#region Methods
		public override void Refresh() {
			base.Refresh();
			propertyGrid.Refresh();
		}
		public void EnableResourceList() {
			resourceList.Enabled = true;
		}
		public void DisableResourceList() {
			resourceList.Enabled = false;
		}
		public void HideResourceList() {
			resourceList.Visible = false;
			OnResize(null,null);
		}
		public void ShowResourceList() {
			resourceList.Visible = true;
			OnResize(null,null);
		}
		public override string ToString() {
			string s = "MpePropertyManager";
			s += "(ResourceListCount=[" + resourceList.Items.Count + "]";
			s += ",Selected=[";
			if (SelectedResource != null)
				s += SelectedResource.Id;
			else
				s += null;
			s += "])";
         return s;
		}
		#endregion

		#region Properties
		protected MpeStatusBar StatusBar {
			get {
				return mpe.StatusBar;
			}
		}
		public MpeResource SelectedResource {
			get {
				if (propertyGrid.SelectedObject == null)
					return null;
				return (MpeResource)propertyGrid.SelectedObject;
			}
			set {
				if (SelectedResource != value) {
					if (SelectedResource != null) {
						// Reset currently selected resource
						SelectedResource.Masked = false;
					}
					propertyGrid.SelectedObject = value;
					if (value != null && resourceList != null && resourceList.DataSource != null) {
						value.Masked = true;
						try {
							for (int i = 0; i < resourceList.Items.Count; i++) {
								MpeResource resource = (MpeResource)resourceList.Items[i];
								if (resource.Id == value.Id) {
									MpeLog.Debug("Setting resource list index to " + i.ToString());
									resourceList.SelectedIndex = i;
									return;
								}
							}
							resourceList.SelectedIndex = -1;
						} catch (Exception ee) {
							MpeLog.Debug(ee);
							//MpeLog.Warn(ee);
						}
					} else {
						resourceList.SelectedIndex = -1;
					}
				} else {
					propertyGrid.Refresh();
				}
			}
		}
		public MpeResourceCollection ResourceList {
			set {
				resourceList.DataSource = null;
				if (value != null) {
					resourceList.DataSource = value.DataSource;
					resourceList.DisplayMember = "DisplayName";
				}
			}
		}
		#endregion

		#region Event Handlers
		
		protected override void OnNotifyMessage(Message m) {
			if (m.Msg == (int)Crownwood.Magic.Win32.Msgs.WM_CONTEXTMENU) {
				//short x = (short)(m.LParam.ToInt32());
				//short y = (short)(m.LParam.ToInt32() >> 16);
				contextMenu.TrackPopup(new Point(m.LParam.ToInt32()));
			}
			base.OnNotifyMessage(m);
		}
		
		private void OnMenuCategoryClicked(object sender, EventArgs e) {
			try {
				propertyGrid.PropertySort = PropertySort.Categorized;
			} catch (Exception ee) {
				StatusBar.Debug(ee);
			}
		}
		private void OnMenuNameClicked(object sender, EventArgs e) {
			try {
				propertyGrid.PropertySort = PropertySort.Alphabetical;
			} catch (Exception ee) {
				StatusBar.Debug(ee);
			}
		}
		private void OnResourceListSelectionChanged(object sender, System.EventArgs e) {
			SelectedResource = (MpeResource)resourceList.SelectedItem;
			MpeLog.Debug("ResourceListSelectionChange = " + (SelectedResource != null ? SelectedResource.Id.ToString() : "null"));
			if (ResourceListSelectionChanged != null)
				ResourceListSelectionChanged(SelectedResource);
		}
		private void OnResize(object sender, System.EventArgs e) {
			if (resourceList.Visible) {
				resourceList.Location = new Point(1,1);
				resourceList.Width = Width - 3;
				propertyGrid.Location = new Point(1,24);
				propertyGrid.Width = Width - 3;
				propertyGrid.Height = Height - 24;
			} else {
				propertyGrid.Location = new Point(1,1);
				propertyGrid.Width = Width - 3;
				propertyGrid.Height = Height - 1;
			}
		}
		private void OnFocusEnter(object sender, System.EventArgs e) {
			OnResize(sender, e);
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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MpePropertyManager));
			this.propertyGrid = new System.Windows.Forms.PropertyGrid();
			this.resourceList = new System.Windows.Forms.ComboBox();
			this.menuImageList = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// propertyGrid
			// 
			this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.propertyGrid.CommandsVisibleIfAvailable = true;
			this.propertyGrid.LargeButtons = false;
			this.propertyGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
			this.propertyGrid.Location = new System.Drawing.Point(1, 25);
			this.propertyGrid.Name = "propertyGrid";
			this.propertyGrid.Size = new System.Drawing.Size(414, 321);
			this.propertyGrid.TabIndex = 0;
			this.propertyGrid.Text = "PropertyGrid";
			this.propertyGrid.ToolbarVisible = false;
			this.propertyGrid.ViewBackColor = System.Drawing.SystemColors.Window;
			this.propertyGrid.ViewForeColor = System.Drawing.SystemColors.WindowText;
			// 
			// resourceList
			// 
			this.resourceList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.resourceList.Location = new System.Drawing.Point(1, 2);
			this.resourceList.Name = "resourceList";
			this.resourceList.Size = new System.Drawing.Size(413, 21);
			this.resourceList.TabIndex = 1;
			this.resourceList.SelectionChangeCommitted += new System.EventHandler(this.OnResourceListSelectionChanged);
			// 
			// menuImageList
			// 
			this.menuImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.menuImageList.ImageSize = new System.Drawing.Size(20, 20);
			this.menuImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("menuImageList.ImageStream")));
			this.menuImageList.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// MpePropertyManager
			// 
			this.Controls.Add(this.resourceList);
			this.Controls.Add(this.propertyGrid);
			this.DockPadding.All = 1;
			this.Name = "MpePropertyManager";
			this.Size = new System.Drawing.Size(416, 328);
			this.Resize += new System.EventHandler(this.OnResize);
			this.Enter += new System.EventHandler(this.OnFocusEnter);
			this.ResumeLayout(false);

		}
		#endregion

	}
}
