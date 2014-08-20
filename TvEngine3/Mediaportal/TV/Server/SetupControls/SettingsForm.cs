#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Mediaportal.TV.Server.SetupControls
{
  public partial class SettingsForm : MPForm
  {
    protected SectionSettings _previousSection;
    protected static IDictionary<string, SectionTreeNode> _sections = new Dictionary<string, SectionTreeNode>(100);

    public SettingsForm(bool isRestrictedMode)
    {
      InitializeComponent();
      linkLabel1.Links.Add(0, linkLabel1.Text.Length, "http://www.team-mediaportal.com/donate.html");
      btnRestrictedMode.Visible = isRestrictedMode;
    }

    public virtual void AddSection(SectionSettings section)
    {
      AddChildSection(null, section);
    }

    public virtual void AddChildSection(SectionSettings parentSection, SectionSettings section)
    {
      SectionTreeNode node = new SectionTreeNode(section);
      if (parentSection == null)
      {
        // Add to the root.
        sectionTree.Nodes.Add(node);
      }
      else
      {
        // Add to the parent node.
        _sections[parentSection.Text].Nodes.Add(node);
      }

      _sections.Add(section.Text, node);
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


    public virtual void SettingsForm_Closed(object sender, EventArgs e) {}

    public virtual void SettingsForm_Load(object sender, EventArgs e) {}

    public virtual void LoadSectionSettings(TreeNode currentNode) {}

    public virtual void SaveSectionSettings(TreeNode currentNode) {}

    public virtual void cancelButton_Click(object sender, EventArgs e) {}

    public virtual void okButton_Click(object sender, EventArgs e) {}

    private void holderPanel_Paint(object sender, PaintEventArgs e) {}

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start((string)e.Link.LinkData);
    }

    public virtual void helpToolStripSplitButton_ButtonClick(object sender, EventArgs e) {}

    public virtual void configToolStripSplitButton_ButtonClick(object sender, EventArgs e) {}
   
    private void btnRestrictedMode_Click(object sender, EventArgs e)
    {
      MessageBox.Show("There are a few requirements that must be met in order for SetupTV to interact with the TV service when running in a multi-seat environment.\n\n" +
        "Interacting with a remote Windows service in a workgroup environment (not joined in a domain)\n" +
        "requires that the user is logged on the machine hosting the TV service using an administrative user account that has a password.\n" +
        "Create an administrator user account on the TV service host and make sure you match both the username and password.\n\n" +
        "If these prerequisite cannot be met then the SetupTV application will run in a so called restricted mode.\n" +
        "In this mode you will be unable to restart/stop the TV service."
        , "What is restricted mode?", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
  }
}