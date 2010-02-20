#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore.Interfaces;

namespace MpeCore.Classes.SectionPanel
{
  public partial class TreeViewSelector : BaseHorizontalLayout, ISectionPanel
  {
    //private ShowModeEnum Mode = ShowModeEnum.Preview;
    //private SectionItem Section = new SectionItem();
    //private PackageClass Package;
    //private SectionResponseEnum _resp = SectionResponseEnum.Cancel;

    private const string CONST_TEXT = "Description ";

    public TreeViewSelector()
    {
      InitializeComponent();
    }

    #region ISectionPanel Members

    public bool Unique
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public SectionParamCollection Init()
    {
      throw new NotImplementedException();
    }

    public SectionParamCollection GetDefaultParams()
    {
      SectionParamCollection _param = new SectionParamCollection(Params);

      _param.Add(new SectionParam(CONST_TEXT, "", ValueTypeEnum.String,
                                  "Description of this operation"));
      return _param;
    }

    public void Preview(PackageClass packageClass, SectionItem sectionItem)
    {
      Mode = ShowModeEnum.Preview;
      Package = packageClass;
      Section = sectionItem;
      SetValues();
      ShowDialog();
    }

    public SectionResponseEnum Execute(PackageClass packageClass, SectionItem sectionItem)
    {
      Mode = ShowModeEnum.Real;
      Package = packageClass;
      Section = sectionItem;
      SetValues();
      Base.ActionExecute(Package, Section, ActionExecuteLocationEnum.BeforPanelShow);
      Base.ActionExecute(Package, Section, ActionExecuteLocationEnum.AfterPanelShow);
      if (!packageClass.Silent)
        ShowDialog();
      else
        base.Resp = SectionResponseEnum.Next;
      Base.ActionExecute(Package, Section, ActionExecuteLocationEnum.AfterPanelHide);
      return base.Resp;
    }

    #endregion

    private TreeNode CreateNode(GroupItem item)
    {
      TreeNode node = new TreeNode(item.DisplayName);
      node.Name = item.DisplayName;
      node.Checked = Mode == ShowModeEnum.Preview ? item.DefaulChecked : item.Checked;
      node.Tag = item;
      return node;
    }

    private void SetValues()
    {
      label1.Text = Section.Params[CONST_TEXT].Value;
      treeView1.Nodes.Clear();
      foreach (string includedGroup in Section.IncludedGroups)
      {
        GroupItem groupItem = Package.Groups[includedGroup];
        if (string.IsNullOrEmpty(groupItem.ParentGroup))
        {
          treeView1.Nodes.Add(CreateNode(groupItem));
        }
        else
        {
          GroupItem parent = Package.Groups[groupItem.ParentGroup];
          if (!treeView1.Nodes.ContainsKey(parent.DisplayName))
            treeView1.Nodes.Add(CreateNode(parent));
          treeView1.Nodes[parent.DisplayName].Nodes.Add(CreateNode(groupItem));
        }
      }
      treeView1.Sort();
      treeView1.ExpandAll();
    }

    private void TreeViewSelector_Load(object sender, EventArgs e) {}

    private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
    {
      if (Mode == ShowModeEnum.Preview)
        return;
      GroupItem groupItem = e.Node.Tag as GroupItem;
      groupItem.Checked = e.Node.Checked;
    }

    private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
    {
      GroupItem groupItem = e.Node.Tag as GroupItem;
      lbl_description.Text = groupItem.Description;
    }

    #region ISectionPanel Members

    public string DisplayName
    {
      get { return "Tree View Selector"; }
    }


    public string Guid
    {
      get { return "{1A637F22-CBA1-480a-9B89-3994F8DF1700}"; }
    }

    #endregion
  }
}