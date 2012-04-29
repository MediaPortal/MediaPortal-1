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
using System.Collections;
using System.Windows.Forms;

namespace Mediaportal.TV.Server.SetupControls
{
  public partial class SettingsForm : MPForm
  {
    protected SectionSettings _previousSection;
    protected static Hashtable settingSections = new Hashtable();

    /// <summary>
    /// Hashtable where we store each added tree node/section for faster access
    /// </summary>
    public static Hashtable SettingSections
    {
      get { return settingSections; }
    }

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


    public virtual void SettingsForm_Closed(object sender, EventArgs e) {}

    public virtual void SettingsForm_Load(object sender, EventArgs e) {}

    public virtual void LoadSectionSettings(TreeNode currentNode) {}

    public virtual void SaveSectionSettings(TreeNode currentNode) {}

    public virtual void SaveAllSettings() {}


    public virtual void cancelButton_Click(object sender, EventArgs e) {}

    public virtual void okButton_Click(object sender, EventArgs e) {}

    public virtual void applyButton_Click(object sender, EventArgs e) {}

    private void holderPanel_Paint(object sender, PaintEventArgs e) {}

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start((string)e.Link.LinkData);
    }

    public virtual void helpToolStripSplitButton_ButtonClick(object sender, EventArgs e) {}

    public virtual void configToolStripSplitButton_ButtonClick(object sender, EventArgs e) {}
   
    private void btnRestrictedMode_Click(object sender, EventArgs e)
    {
      MessageBox.Show("In order for SetupTV to interact with the TV Service when running SetupTV in a multiseat environment a few things are required...\n\n" +
        "Interacting with a remote windows service in a workgroup environment (not joined in a domain)\n" +
        "requires that the user is logged on the machine hosting the TV service using an administrative user account that has a password.\n" +
        "Create an administrator user account on the TV Service host and make sure you match both the username and password.\n\n" +
        "If these prerequisite cannot be met then the SetupTV application will run in a so called restricted mode.\n" +
        "In this mode you will be unable to restart/stop the tvservice."
        , "What is restricted mode ?", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
  }
}