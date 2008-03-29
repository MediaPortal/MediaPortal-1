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
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using MediaPortal.UserInterface.Controls;

namespace SetupTv
{
  public partial class SettingsForm : Form
  {
    protected SectionSettings _previousSection = null;
    protected static Hashtable settingSections = new Hashtable();

    /// <summary>
    /// Hashtable where we store each added tree node/section for faster access
    /// </summary>
    public static Hashtable SettingSections
    {
      get { return settingSections; }
    }


    public SettingsForm()
    {

    }


    public virtual void AddSection(SectionSettings section)
    {
      AddChildSection(null, section);
    }

    public virtual void AddChildSection(SectionSettings parentSection, SectionSettings section)
    {
      //
      // Make sure this section doesn't already exist
      //

      //
      // Add section to tree
      //
      SectionTreeNode treeNode = new SectionTreeNode(section);

      if (parentSection == null)
      {
        //
        // Add to the root
        //
        sectionTree.Nodes.Add(treeNode);
      }
      else
      {
        //
        // Add to the parent node
        //
        SectionTreeNode parentTreeNode = (SectionTreeNode)settingSections[parentSection.Text];
        parentTreeNode.Nodes.Add(treeNode);
      }

      settingSections.Add(section.Text, treeNode);

      //treeNode.EnsureVisible();
    }
    
    public virtual void sectionTree_BeforeSelect(object sender, TreeViewCancelEventArgs e)
    {
      SectionTreeNode treeNode = e.Node as SectionTreeNode;

      if (treeNode != null)
      {
        e.Cancel = !treeNode.Section.CanActivate;
      }
    }

    public virtual void sectionTree_AfterSelect(object sender, TreeViewEventArgs e)
    {
      SectionTreeNode treeNode = e.Node as SectionTreeNode;

      if (treeNode != null)
      {
        if (ActivateSection(treeNode.Section))
        {
          headerLabel.Caption = treeNode.Section.Text;
        }
      }
    }

    public virtual bool ActivateSection(SectionSettings section)
    {
      return true;
    }


    public virtual void SettingsForm_Closed(object sender, EventArgs e)
    {

    }

    public virtual void SettingsForm_Load(object sender, EventArgs e)
    {
    }

    public virtual void LoadSectionSettings(TreeNode currentNode)
    {
    }

    public virtual void SaveSectionSettings(TreeNode currentNode)
    {
    }

    public virtual void SaveAllSettings()
    {
    }


    public virtual void cancelButton_Click(object sender, EventArgs e)
    {
    }

    public virtual void okButton_Click(object sender, EventArgs e)
    {
    }

    public virtual void applyButton_Click(object sender, EventArgs e)
    {
    }

    public virtual void buttonViewLog_Click(object sender, EventArgs e)
    {
    }

    public virtual void helpButton_Click(object sender, EventArgs e)
    {

    }


    private void holderPanel_Paint(object sender, PaintEventArgs e)
    {

    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start((string)e.Link.LinkData);
    }
  }
}