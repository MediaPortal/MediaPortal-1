#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.IO;

namespace MediaPortal.DeployTool
{
  public partial class RequirementsDlg : DeployDialog, IDeployDialog
  {
    public RequirementsDlg()
    {
      InitializeComponent();
      type=DialogType.Requirements;
      PopulateListView();
    }

    #region IDeplayDialog interface
    public override DeployDialog GetNextDialog()
    {
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.InstallProgress);
    }
    public override bool SettingsValid()
    {
      Utils.ErrorDlg("Not completely implemented.");
      return false;
    }
    public override void SetProperties()
    {
 
    }
    #endregion

    private void PopulateListView()
    {
      listView.Items.Clear();
      listView.Items.Add(".NET Framework 2.0");
      if (InstallationProperties.Instance["InstallType"] == "singleseat")
      {
        listView.Items.Add("DirectX 9");
        if (InstallationProperties.Instance["DBMSType"] == "mssql")
          listView.Items.Add("MS-SQL Server");
        else
          listView.Items.Add("MySQL");
        listView.Items.Add("MediaPortal");
        listView.Items.Add("TV-Server");
        listView.Items.Add("TV-Client plugin");
      }
      else if (InstallationProperties.Instance["InstallType"] == "tvserver_master")
      {
        if (InstallationProperties.Instance["DBMSType"] == "mssql")
          listView.Items.Add("MS-SQL Server");
        else
          listView.Items.Add("MySQL");
        listView.Items.Add("TV-Server");
      }
      else if (InstallationProperties.Instance["InstallType"] == "tvserver_slave")
      {
        listView.Items.Add("TV-Server");
      }
      else if (InstallationProperties.Instance["InstallType"] == "client")
      {
        listView.Items.Add("DirectX 9");
        listView.Items.Add("MediaPortal");
        listView.Items.Add("TV-Client plugin");
      }
      listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
    }

    private void RequirementsDlg_ParentChanged(object sender, EventArgs e)
    {
      if (Parent != null)
        PopulateListView();
    }

  }
}
