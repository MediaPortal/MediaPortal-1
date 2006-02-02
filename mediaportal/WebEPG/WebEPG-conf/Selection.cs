#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using MediaPortal.Webepg.Profile;

namespace WebEPG_conf
{
  /// <summary>
  /// Summary description for Form1.
  /// </summary>
  public class fSelection : System.Windows.Forms.Form
  {
    private TreeNode tChannels;
    private TreeNode tGrabbers;
    private EventHandler handler;
    private System.Windows.Forms.TreeView treeView1;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbSelection;
    private MediaPortal.UserInterface.Controls.MPRadioButton rbWebsites;
    private MediaPortal.UserInterface.Controls.MPRadioButton rbChannels;
    private MediaPortal.UserInterface.Controls.MPTextBox tbAInfo;
    private MediaPortal.UserInterface.Controls.MPLabel lList;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPButton bSelect;
    private MediaPortal.UserInterface.Controls.MPButton bClose;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public fSelection(TreeNode channels, TreeNode grabbers, bool bChanGrab)
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //

      tChannels = channels;
      tGrabbers = grabbers;

      rbChannels.Checked = bChanGrab;
      rbWebsites.Checked = !bChanGrab;

      treeView1.Nodes.Clear();
      if (rbChannels.Checked)
      {
        treeView1.Nodes.Add((TreeNode)tChannels.Clone());
      }
      else
      {
        treeView1.Nodes.Add((TreeNode)tGrabbers.Clone());
      }

      handler = new EventHandler(DoEvent);
      rbChannels.Click += handler;
      rbWebsites.Click += handler;
      bSelect.Click += handler;
      bClose.Click += handler;
      treeView1.DoubleClick += handler;
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
      this.rbWebsites = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.rbChannels = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.tbAInfo = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lList = new MediaPortal.UserInterface.Controls.MPLabel();
      this.gbSelection.SuspendLayout();
      this.SuspendLayout();
      // 
      // treeView1
      // 
      this.treeView1.ImageIndex = -1;
      this.treeView1.Location = new System.Drawing.Point(16, 48);
      this.treeView1.Name = "treeView1";
      this.treeView1.SelectedImageIndex = -1;
      this.treeView1.Size = new System.Drawing.Size(256, 272);
      this.treeView1.TabIndex = 0;
      // 
      // gbSelection
      // 
      this.gbSelection.Controls.Add(this.bClose);
      this.gbSelection.Controls.Add(this.bSelect);
      this.gbSelection.Controls.Add(this.rbWebsites);
      this.gbSelection.Controls.Add(this.rbChannels);
      this.gbSelection.Controls.Add(this.tbAInfo);
      this.gbSelection.Controls.Add(this.label5);
      this.gbSelection.Controls.Add(this.lList);
      this.gbSelection.Location = new System.Drawing.Point(0, 8);
      this.gbSelection.Name = "gbSelection";
      this.gbSelection.Size = new System.Drawing.Size(424, 352);
      this.gbSelection.TabIndex = 6;
      this.gbSelection.TabStop = false;
      this.gbSelection.Text = "Channel Selection";
      // 
      // bClose
      // 
      this.bClose.Location = new System.Drawing.Point(336, 320);
      this.bClose.Name = "bClose";
      this.bClose.Size = new System.Drawing.Size(72, 24);
      this.bClose.TabIndex = 14;
      this.bClose.Text = "Close";
      // 
      // bSelect
      // 
      this.bSelect.Location = new System.Drawing.Point(16, 320);
      this.bSelect.Name = "bSelect";
      this.bSelect.Size = new System.Drawing.Size(72, 24);
      this.bSelect.TabIndex = 13;
      this.bSelect.Text = "Select";
      // 
      // rbWebsites
      // 
      this.rbWebsites.Location = new System.Drawing.Point(160, 16);
      this.rbWebsites.Name = "rbWebsites";
      this.rbWebsites.TabIndex = 12;
      this.rbWebsites.Text = "Websites";
      // 
      // rbChannels
      // 
      this.rbChannels.Location = new System.Drawing.Point(64, 16);
      this.rbChannels.Name = "rbChannels";
      this.rbChannels.TabIndex = 11;
      this.rbChannels.Text = "Channels";
      // 
      // tbAInfo
      // 
      this.tbAInfo.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
      this.tbAInfo.Location = new System.Drawing.Point(272, 40);
      this.tbAInfo.Multiline = true;
      this.tbAInfo.Name = "tbAInfo";
      this.tbAInfo.ReadOnly = true;
      this.tbAInfo.Size = new System.Drawing.Size(136, 272);
      this.tbAInfo.TabIndex = 10;
      this.tbAInfo.Text = " .. future ..";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(272, 24);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(120, 16);
      this.label5.TabIndex = 7;
      this.label5.Text = "Available Information";
      // 
      // lList
      // 
      this.lList.Location = new System.Drawing.Point(16, 20);
      this.lList.Name = "lList";
      this.lList.Size = new System.Drawing.Size(48, 24);
      this.lList.TabIndex = 5;
      this.lList.Text = "List by:";
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


    private void DoEvent(Object source, EventArgs e)
    {
      if (source == bClose)
      {
        this.Close();
        return;
      }

      if (source == treeView1 || source == bSelect)
      {
        if (treeView1.SelectedNode != null)
        {
          string[] id = (string[])treeView1.SelectedNode.Tag;
          if (id != null)
          {
            this.Tag = id;
            this.Text = "Selection ";
          }
        }
      }

      if (source == rbChannels)
      {
        rbChannels.Checked = true;
        rbWebsites.Checked = false;
        UpdateList();
      }

      if (source == rbWebsites)
      {
        rbChannels.Checked = false;
        rbWebsites.Checked = true;
        UpdateList();
      }
    }

    private void UpdateList()
    {
      //			TreeNode sNode = treeView1.SelectedNode;
      this.treeView1.Nodes.Clear();
      if (rbChannels.Checked)
      {
        this.treeView1.Nodes.Add(tChannels);
        //				if(sNode.Tag != null)
        //				{
        //					TreeNode nNode = FindNode(tChannels, (string []) sNode.Tag);
        //					treeView1.SelectedNode = nNode;
        //				}
      }
      else
      {
        this.treeView1.Nodes.Add(tGrabbers);
      }
    }


    //		private bool SetSelect(TreeViewNode cNodes, string[] tag)
    //		{
    //			cNodes.Expand();
    //			foreach(TreeNode tNode in cNodes)
    //			{
    //				bool ret = SetSelect(cNodes.Nodes, tag);
    //				if(ret)
    //					return ret; 
    //			}
    //
    //			string[] ntag = (string[]) tNode.Tag;
    //			if(ntag != null && ntag[0] == tag[0] && ntag[1] == tag[1])
    //			{
    //				cNodes[1].
    //				return true;
    //			}
    //
    //			cNodes.Colapse();
    //			return false;
    //
    //		}

    private TreeNode FindNode(TreeNode tNode, string[] tag)
    {
      tNode.Expand();
      foreach (TreeNode cNode in tNode.Nodes)
      {
        TreeNode rNode = FindNode(cNode, tag);
        if (rNode != null)
          return rNode;
      }

      string[] ntag = (string[])tNode.Tag;
      if (ntag != null && ntag[0] == tag[0] && ntag[1] == tag[1])
      {
        //tNode.Selected=true;
        return tNode;
      }

      tNode.Collapse();
      return null;
    }

    //		void FindNodeByVal(TreeNodeCollection nodes, string SearchValue)
    //		{
    //			// step 1
    //			for (int i = 0; i < nodes.Count; i++)
    //			{
    //				// step 2
    //				if (nodes[i].Value == SearchValue)
    //				{
    //					// step 3
    //					nodes[i].Select();
    //
    //					// step 4
    //					NodeFound = true;
    //					return;
    //				}
    //				else
    //				{
    //					// step 5
    //					NodeFound = false;
    //				}
    //
    //				// step 6
    //				nodes[i].Expand();
    //
    //				// step 7
    //				FindNodeByVal(nodes[i].ChildNodes, SearchValue);
    //
    //				// step 8
    //				if (NodeFound)
    //				{
    //					return;
    //				}
    //
    //				// step 9
    //				nodes[i].Collapse();
    //
    //				return;
    //			}
    //		}

  }
}
