#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace Mpe.Forms
{
  /// <summary>
  /// Summary description for MpeHelpManager.
  /// </summary>
  public class MpeHelpManager : UserControl
  {
    #region Variables

    private TreeView tree;
    private Panel treePanel;
    private ImageList treeImageList;
    private IContainer components;

    private MediaPortalEditor mpe;
    private MpePreferences preferences;
    private TreeNode rootNode;
    private TreeNode selectedNode;

    #endregion

    private delegate void UpdateHelpTreeDelegate(TreeNode root);

    #region Contructors

    public MpeHelpManager(MediaPortalEditor mpe)
    {
      // Initialize variables
      this.mpe = mpe;
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();
    }

    #endregion

    #region Methods

    public void LoadHelp()
    {
      while (mpe.Preferences == null)
      {
        Thread.Sleep(250);
      }
      preferences = mpe.Preferences;
      if (preferences.HelpDir == null || preferences.HelpDir.Exists == false)
      {
        MpeLog.Warn("Could not initialize HelpManager!");
        return;
      }
      rootNode = new TreeNode("Help", 0, 0);
      LoadHelpFiles(rootNode, preferences.HelpDir);
      Invoke(new UpdateHelpTreeDelegate(UpdateHelpTree), new object[] {rootNode});
    }

    private void LoadHelpFiles(TreeNode node, DirectoryInfo dir)
    {
      FileInfo[] files = dir.GetFiles("*.html");
      for (int i = 0; files != null && i < files.Length; i++)
      {
        if (files[i].Name.StartsWith("_") == false)
        {
          TreeNode n = new TreeNode(files[i].Name, 1, 1);
          n.Tag = files[i];
          node.Nodes.Add(n);
        }
      }
      DirectoryInfo[] dirs = dir.GetDirectories();
      for (int i = 0; dirs != null && i < dirs.Length; i++)
      {
        if (dirs[i].Name.StartsWith("_") == false)
        {
          TreeNode p = new TreeNode(dirs[i].Name, 2, 2);
          node.Nodes.Add(p);
          LoadHelpFiles(p, dirs[i]);
        }
      }
    }

    private void UpdateHelpTree(TreeNode root)
    {
      tree.Nodes.Clear();
      tree.Nodes.Add(rootNode);
      tree.Nodes[0].Expand();
    }

    #endregion

    #region Component Designer Generated Code

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new Container();
      ResourceManager resources = new ResourceManager(typeof(MpeHelpManager));
      tree = new TreeView();
      treeImageList = new ImageList(components);
      treePanel = new Panel();
      treePanel.SuspendLayout();
      SuspendLayout();
      // 
      // tree
      // 
      tree.BorderStyle = BorderStyle.None;
      tree.Dock = DockStyle.Fill;
      tree.ImageList = treeImageList;
      tree.Location = new Point(1, 1);
      tree.Name = "tree";
      tree.Size = new Size(262, 334);
      tree.Sorted = true;
      tree.TabIndex = 0;
      tree.MouseDown += new MouseEventHandler(OnMouseDown);
      tree.DoubleClick += new EventHandler(OnDoubleClick);
      // 
      // treeImageList
      // 
      treeImageList.ColorDepth = ColorDepth.Depth32Bit;
      treeImageList.ImageSize = new Size(16, 16);
      treeImageList.ImageStream = ((ImageListStreamer) (resources.GetObject("treeImageList.ImageStream")));
      treeImageList.TransparentColor = Color.White;
      // 
      // treePanel
      // 
      treePanel.BackColor = SystemColors.AppWorkspace;
      treePanel.Controls.Add(tree);
      treePanel.Dock = DockStyle.Fill;
      treePanel.DockPadding.All = 1;
      treePanel.Location = new Point(0, 0);
      treePanel.Name = "treePanel";
      treePanel.Size = new Size(264, 336);
      treePanel.TabIndex = 1;
      // 
      // MpeHelpManager
      // 
      Controls.Add(treePanel);
      Name = "MpeHelpManager";
      Size = new Size(264, 336);
      Load += new EventHandler(OnLoad);
      treePanel.ResumeLayout(false);
      ResumeLayout(false);
    }

    #endregion

    private void OnLoad(object sender, EventArgs e)
    {
      Thread thread = new Thread(new ThreadStart(LoadHelp));
      thread.Name = "HelpManagerLoader";
      thread.Start();
    }

    private void OnMouseDown(object sender, MouseEventArgs e)
    {
      selectedNode = tree.GetNodeAt(e.X, e.Y);
    }

    private void OnDoubleClick(object sender, EventArgs e)
    {
      if (selectedNode != null && selectedNode.Tag != null && selectedNode.Tag is FileInfo)
      {
        mpe.ShowHelp((FileInfo) selectedNode.Tag);
      }
    }
  }
}