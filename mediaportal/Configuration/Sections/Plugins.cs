using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration.Sections
{
	public class Plugins : MediaPortal.Configuration.SectionSettings
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private MediaPortal.UserInterface.Controls.MPListView pluginsListView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.Button setupButton;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.ComponentModel.IContainer components = null;

		ArrayList availablePlugins = new ArrayList();
		ArrayList loadedPlugins = new ArrayList();
		
		public Plugins() : this("Plugins")
		{
		}

		public Plugins(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			//
			// Enumerate available plugins
			//
			EnumeratePlugins();

			//
			// Load plugins
			//
			LoadPlugins();

			//
			// Populate our list
			//
			PopulateListView();
		}

		/// <summary>
		/// 
		/// </summary>
		private void PopulateListView()
		{
			foreach(ISetupForm setupForm in loadedPlugins)
			{
				ListViewItem listItem = new ListViewItem(new string[] { setupForm.PluginName(), setupForm.Description(), setupForm.Author() } );
				listItem.Tag = setupForm;

				pluginsListView.Items.Add(listItem);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="directory"></param>
		private void EnumeratePluginDirectory(string directory)
		{
			if(Directory.Exists(directory))
			{
				//
				// Enumerate files
				//
				string[] files = Directory.GetFiles(directory, "*.dll");

				//
				// Add to list
				//
				foreach (string file in files)
				{
					availablePlugins.Add(file);
				}
			}
		}

		private void EnumeratePlugins()
		{
			EnumeratePluginDirectory(@"plugins\windows");
			EnumeratePluginDirectory(@"plugins\subtitle");
			EnumeratePluginDirectory(@"plugins\tagreaders");
		}

		private void LoadPlugins()
		{
			foreach(string pluginFile in availablePlugins)
			{
				try
				{
					Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);

					if(pluginAssembly != null)
					{
						Type[] exportedTypes = pluginAssembly.GetExportedTypes();

						foreach(Type type in exportedTypes)
						{
							//
							// Try to locate the interface we're interested in
							//
							if(type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null)
							{
								//
								// Create instance of the current type
								//
								object pluginObject = (object)Activator.CreateInstance(type);
								ISetupForm pluginForm = pluginObject as ISetupForm;

								if(pluginForm != null)
								{
									loadedPlugins.Add(pluginForm);
								}
							}
						}
					}
				}
				catch
				{
				}
			}
		}

		public override void LoadSettings()
		{
			using(AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				foreach(ListViewItem listItem in pluginsListView.Items)
				{
					ISetupForm setupForm = listItem.Tag as ISetupForm;

					if(setupForm != null && setupForm.CanEnable())
					{
						listItem.Checked = xmlreader.GetValueAsBool("plugins", setupForm.PluginName(), true);
					}
				}
			}			
		}


		public override void SaveSettings()
		{
			using(AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				foreach(ListViewItem listItem in pluginsListView.Items)
				{
					ISetupForm setupForm = listItem.Tag as ISetupForm;

					if(setupForm != null && setupForm.CanEnable())
					{
						xmlwriter.SetValueAsBool("plugins", setupForm.PluginName(), listItem.Checked);
					}
				}
			}			
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
			this.setupButton = new System.Windows.Forms.Button();
			this.pluginsListView = new MediaPortal.UserInterface.Controls.MPListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.setupButton);
			this.groupBox1.Controls.Add(this.pluginsListView);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 432);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Plugin Settings";
			// 
			// setupButton
			// 
			this.setupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.setupButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.setupButton.Location = new System.Drawing.Point(16, 392);
			this.setupButton.Name = "setupButton";
			this.setupButton.TabIndex = 1;
			this.setupButton.Text = "Setup";
			this.setupButton.Click += new System.EventHandler(this.setupButton_Click);
			// 
			// pluginsListView
			// 
			this.pluginsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.pluginsListView.CheckBoxes = true;
			this.pluginsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							  this.columnHeader1,
																							  this.columnHeader2,
																							  this.columnHeader3});
			this.pluginsListView.FullRowSelect = true;
			this.pluginsListView.HideSelection = false;
			this.pluginsListView.Location = new System.Drawing.Point(16, 24);
			this.pluginsListView.MultiSelect = false;
			this.pluginsListView.Name = "pluginsListView";
			this.pluginsListView.Size = new System.Drawing.Size(408, 360);
			this.pluginsListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.pluginsListView.TabIndex = 0;
			this.pluginsListView.View = System.Windows.Forms.View.Details;
			this.pluginsListView.DoubleClick += new System.EventHandler(this.pluginsListView_DoubleClick);
			this.pluginsListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.pluginsListView_ItemCheck);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Plugin Name";
			this.columnHeader1.Width = 119;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Description";
			this.columnHeader2.Width = 198;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Author";
			this.columnHeader3.Width = 87;
			// 
			// Plugins
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "Plugins";
			this.Size = new System.Drawing.Size(456, 448);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void setupButton_Click(object sender, System.EventArgs e)
		{
			if(pluginsListView.SelectedItems != null)
			{
				foreach(ListViewItem listItem in pluginsListView.SelectedItems)
				{
					ISetupForm setupForm = listItem.Tag as ISetupForm;

					if(setupForm != null)
					{
						setupForm.ShowPlugin();
					}
				}
			}
		}

		private void pluginsListView_DoubleClick(object sender, System.EventArgs e)
		{
			setupButton_Click(sender, e);
		}

		private void pluginsListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			ListViewItem listItem = pluginsListView.Items[e.Index];

			if(listItem != null)
			{
				ISetupForm setupForm = listItem.Tag as ISetupForm;

				if(setupForm != null)
				{
					if(setupForm.CanEnable())
					{
						//
						// Do nothing
						//
					}
					else
					{
						MessageBox.Show("The selected plugin does not support enabling/disabling", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
						e.NewValue = e.CurrentValue;
					}
				}
			}
		}
	}
}

