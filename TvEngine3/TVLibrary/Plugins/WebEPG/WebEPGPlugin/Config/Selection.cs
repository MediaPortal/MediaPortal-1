#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections.WebEPGConfig
{
  public class GrabberSelectionInfo
  {
    private string channelId;
    private string grabberId;

    public GrabberSelectionInfo(string channelId, string grabberId)
    {
      this.channelId = channelId;
      this.grabberId = grabberId;
    }

    public string ChannelId
    {
      get { return channelId; }
      set { channelId = value; }
    }

    public string GrabberId
    {
      get { return grabberId; }
      set { grabberId = value; }
    }
  }

  public class GrabberSelectedEventArgs : EventArgs
  {
    public readonly GrabberSelectionInfo Selection;

    public GrabberSelectedEventArgs(GrabberSelectionInfo selection)
    {
      Selection = selection;
    }

    public GrabberSelectedEventArgs(string channelId, string grabberId)
    {
      Selection = new GrabberSelectionInfo(channelId, grabberId);
    }
  }


  /// <summary>
  /// Summary description for Form1.
  /// </summary>
  public class fSelection : Form
  {
    private TreeNode tGrabbers;
    private EventHandler handler;
    private TreeView treeView1;
    private MPGroupBox gbSelection;
    private MPButton bSelect;
    private MPButton bClose;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    public delegate void GrabberSelectedEventHandler(object sender, GrabberSelectedEventArgs e);

    public event GrabberSelectedEventHandler GrabberSelected;

    public fSelection(TreeNode grabbers) //, bool bChanGrab, EventHandler select_click)
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //

      tGrabbers = grabbers;

      treeView1.Nodes.Clear();
      treeView1.Nodes.Add((TreeNode)tGrabbers.Clone());

      treeView1.TreeViewNodeSorter = new NodeSorter();

      handler = new EventHandler(DoEvent);
      bSelect.Click += DoSelect; // select_click;
      bClose.Click += handler;
      treeView1.DoubleClick += DoSelect; //select_click;
    }

    public GrabberSelectionInfo Selected
    {
      get { return (GrabberSelectionInfo)treeView1.SelectedNode.Tag; }
    }

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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.treeView1 = new System.Windows.Forms.TreeView();
      this.gbSelection = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.bClose = new MediaPortal.UserInterface.Controls.MPButton();
      this.bSelect = new MediaPortal.UserInterface.Controls.MPButton();
      this.gbSelection.SuspendLayout();
      this.SuspendLayout();
      // 
      // treeView1
      // 
      this.treeView1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.treeView1.Location = new System.Drawing.Point(16, 27);
      this.treeView1.Name = "treeView1";
      this.treeView1.Size = new System.Drawing.Size(392, 293);
      this.treeView1.TabIndex = 0;
      // 
      // gbSelection
      // 
      this.gbSelection.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.gbSelection.Controls.Add(this.bClose);
      this.gbSelection.Controls.Add(this.bSelect);
      this.gbSelection.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbSelection.Location = new System.Drawing.Point(0, 8);
      this.gbSelection.Name = "gbSelection";
      this.gbSelection.Size = new System.Drawing.Size(424, 352);
      this.gbSelection.TabIndex = 6;
      this.gbSelection.TabStop = false;
      this.gbSelection.Text = "Channel Selection";
      // 
      // bClose
      // 
      this.bClose.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bClose.Location = new System.Drawing.Point(336, 320);
      this.bClose.Name = "bClose";
      this.bClose.Size = new System.Drawing.Size(72, 24);
      this.bClose.TabIndex = 14;
      this.bClose.Text = "Close";
      this.bClose.UseVisualStyleBackColor = true;
      // 
      // bSelect
      // 
      this.bSelect.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.bSelect.Location = new System.Drawing.Point(16, 320);
      this.bSelect.Name = "bSelect";
      this.bSelect.Size = new System.Drawing.Size(72, 24);
      this.bSelect.TabIndex = 13;
      this.bSelect.Text = "Select";
      this.bSelect.UseVisualStyleBackColor = true;
      // 
      // fSelection
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(424, 365);
      this.Controls.Add(this.treeView1);
      this.Controls.Add(this.gbSelection);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "fSelection";
      this.Text = "Selection";
      this.gbSelection.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    private void DoSelect(Object source, EventArgs e)
    {
      if (GrabberSelected != null)
      {
        GrabberSelectionInfo selection = Selected;
        if (selection != null)
        {
          GrabberSelected(this, new GrabberSelectedEventArgs(Selected));
        }
      }
    }

    private void DoEvent(Object source, EventArgs e)
    {
      if (source == bClose)
      {
        this.Close();
        return;
      }
    }

    private void UpdateList()
    {
      //			TreeNode sNode = treeView1.SelectedNode;
      this.treeView1.Nodes.Clear();
      this.treeView1.Nodes.Add((TreeNode)tGrabbers.Clone());
    }

    private TreeNode FindNode(TreeNode tNode, GrabberSelectionInfo tag)
    {
      tNode.Expand();
      foreach (TreeNode cNode in tNode.Nodes)
      {
        TreeNode rNode = FindNode(cNode, tag);
        if (rNode != null)
        {
          return rNode;
        }
      }

      GrabberSelectionInfo ntag = (GrabberSelectionInfo)tNode.Tag;
      if (ntag != null && ntag.ChannelId == tag.ChannelId && ntag.GrabberId == tag.GrabberId)
      {
        return tNode;
      }

      tNode.Collapse();
      return null;
    }
  }

  // Create a node sorter that implements the IComparer interface.
  public class NodeSorter : IComparer
  {
    // Compare the length of the strings, or the strings
    // themselves, if they are the same length.
    public int Compare(object x, object y)
    {
      TreeNode tx = x as TreeNode;
      TreeNode ty = y as TreeNode;
      return string.Compare(tx.Text, ty.Text);
    }
  }
}