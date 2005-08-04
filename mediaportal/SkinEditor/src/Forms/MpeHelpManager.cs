using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Mpe.Designers;

namespace Mpe.Forms
{
	/// <summary>
	/// Summary description for MpeHelpManager.
	/// </summary>
	public class MpeHelpManager : System.Windows.Forms.UserControl {
	
		#region Variables
		private System.Windows.Forms.TreeView tree;
		private System.Windows.Forms.Panel treePanel;
		private System.Windows.Forms.ImageList treeImageList;
		private System.ComponentModel.IContainer components;

		private MediaPortalEditor mpe;
		private MpePreferences preferences;
		private TreeNode rootNode;
		private TreeNode selectedNode;
		#endregion

		private delegate void UpdateHelpTreeDelegate(TreeNode root);

		#region Contructors
		public MpeHelpManager(MediaPortalEditor mpe) {
			// Initialize variables
			this.mpe = mpe;
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}
		#endregion

		#region Methods
		public void LoadHelp() {
			while (mpe.Preferences == null) {
				Thread.Sleep(250);
			}
			preferences = mpe.Preferences;
			if (preferences.HelpDir == null || preferences.HelpDir.Exists == false) {
				MpeLog.Warn("Could not initialize HelpManager!");
				return;
			}
			rootNode = new TreeNode("Help", 0, 0);
			LoadHelpFiles(rootNode, preferences.HelpDir);
			Invoke(new UpdateHelpTreeDelegate(UpdateHelpTree), new object[] { rootNode });
		}
		private void LoadHelpFiles(TreeNode node, DirectoryInfo dir) {
			FileInfo[] files = dir.GetFiles("*.html");
			for (int i = 0; files != null && i < files.Length; i++) {
				if (files[i].Name.StartsWith("_") == false) {
					TreeNode n = new TreeNode(files[i].Name, 1, 1);
					n.Tag = files[i];
					node.Nodes.Add(n);
				}
			}
			DirectoryInfo[] dirs = dir.GetDirectories();
			for (int i = 0; dirs != null && i < dirs.Length; i++) {
				if (dirs[i].Name.StartsWith("_") == false) {
					TreeNode p = new TreeNode(dirs[i].Name, 2, 2);
					node.Nodes.Add(p);
					LoadHelpFiles(p, dirs[i]);
				}
			}
		}
		private void UpdateHelpTree(TreeNode root) {
			tree.Nodes.Clear();
			tree.Nodes.Add(rootNode);
			tree.Nodes[0].Expand();
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MpeHelpManager));
			this.tree = new System.Windows.Forms.TreeView();
			this.treeImageList = new System.Windows.Forms.ImageList(this.components);
			this.treePanel = new System.Windows.Forms.Panel();
			this.treePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// tree
			// 
			this.tree.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tree.ImageList = this.treeImageList;
			this.tree.Location = new System.Drawing.Point(1, 1);
			this.tree.Name = "tree";
			this.tree.Size = new System.Drawing.Size(262, 334);
			this.tree.Sorted = true;
			this.tree.TabIndex = 0;
			this.tree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
			this.tree.DoubleClick += new System.EventHandler(this.OnDoubleClick);
			// 
			// treeImageList
			// 
			this.treeImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.treeImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.treeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treeImageList.ImageStream")));
			this.treeImageList.TransparentColor = System.Drawing.Color.White;
			// 
			// treePanel
			// 
			this.treePanel.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.treePanel.Controls.Add(this.tree);
			this.treePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treePanel.DockPadding.All = 1;
			this.treePanel.Location = new System.Drawing.Point(0, 0);
			this.treePanel.Name = "treePanel";
			this.treePanel.Size = new System.Drawing.Size(264, 336);
			this.treePanel.TabIndex = 1;
			// 
			// MpeHelpManager
			// 
			this.Controls.Add(this.treePanel);
			this.Name = "MpeHelpManager";
			this.Size = new System.Drawing.Size(264, 336);
			this.Load += new System.EventHandler(this.OnLoad);
			this.treePanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void OnLoad(object sender, System.EventArgs e) {
			Thread thread = new Thread(new ThreadStart(LoadHelp));
			thread.Name = "HelpManagerLoader";
			thread.Start();
		}
		private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
			selectedNode = tree.GetNodeAt(e.X,e.Y);
		}

		private void OnDoubleClick(object sender, System.EventArgs e) {
			if (selectedNode != null && selectedNode.Tag != null && selectedNode.Tag is FileInfo) {
				mpe.ShowHelp((FileInfo)selectedNode.Tag);
			}
		}

	}
}
