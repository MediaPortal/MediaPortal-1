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
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Mpe.Controls;
using SkinEditor;

namespace Mpe.Designers
{
  /// <summary>
  /// Summary description for MpeStringDesigner.
  /// </summary>
  public class MpeStringDesigner : UserControl, MpeDesigner
  {
    #region Variables

    private Container components = null;
    private MediaPortalEditor mpe;
    private MpeStringTable referenceTable;
    private DataGrid dataGrid;
    private MpeStringTable stringTable;
    private MpeData dataSet;
    private MpeData.MpeDbStringDataTable dataTable;

    #endregion

    #region Contructors

    public MpeStringDesigner(MediaPortalEditor mpe, MpeStringTable referenceTable, MpeStringTable stringTable)
    {
      this.mpe = mpe;
      InitializeComponent();
      if (referenceTable == null)
      {
        throw new DesignerException("Invalid reference string table");
      }
      this.referenceTable = referenceTable;
      this.stringTable = stringTable;
      dataSet = new MpeData();
      dataTable = new MpeData.MpeDbStringDataTable();
    }

    #endregion

    #region Methods - Designer

    public void Initialize()
    {
      dataTable.ReferenceColumn.ReadOnly = true;
      dataTable.RowChanged += new DataRowChangeEventHandler(OnStringTableRowChanged);

      int[] keys = referenceTable.Keys;
      for (int i = 0; i < keys.Length; i++)
      {
        string s1 = referenceTable[keys[i]];
        if (s1 == null)
        {
          s1 = "";
        }
        string s2 = stringTable[keys[i]];
        if (s2 == null)
        {
          s2 = "";
        }
        dataTable.AddMpeDbStringRow(keys[i], s1, s2);
      }
      dataGrid.DataSource = dataTable;
      dataGrid.CaptionVisible = false;
    }

    public void Save()
    {
      MpeLog.Debug("MpeStringDesigner.Save()");
      try
      {
        stringTable.Clear();
        DataRow[] rows = dataTable.Select("", "Id");
        for (int i = 0; rows != null && i < rows.Length; i++)
        {
          MpeData.MpeDbStringRow r = (MpeData.MpeDbStringRow) rows[i];
          stringTable.Add(r.Id, r.Value);
        }
        mpe.Parser.SaveStringTable(stringTable);
        mpe.ToggleDesignerStatus(ResourceName, false);
      }
      catch (Exception ee)
      {
        MpeLog.Debug(ee);
        MpeLog.Error(ee);
      }
    }

    public void Cancel()
    {
      //
    }

    public void Destroy()
    {
      dataTable.Dispose();
      dataSet.Dispose();
    }

    public void Pause()
    {
      //
    }

    public void Resume()
    {
      //
    }

    #endregion

    #region Properties - Designer

    public string ResourceName
    {
      get { return stringTable.Language; }
    }

    public bool AllowAdditions
    {
      get { return false; }
    }

    public bool AllowDeletions
    {
      get { return false; }
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
      dataGrid = new DataGrid();
      ((ISupportInitialize) (dataGrid)).BeginInit();
      SuspendLayout();
      // 
      // dataGrid
      // 
      dataGrid.AllowNavigation = false;
      dataGrid.AlternatingBackColor = SystemColors.Window;
      dataGrid.BackgroundColor = Color.LightGray;
//      dataGrid.BorderStyle = BorderStyle.None;
      dataGrid.DataMember = "";
      dataGrid.Dock = DockStyle.Fill;
      dataGrid.GridLineColor = SystemColors.Control;
      dataGrid.HeaderBackColor = SystemColors.Control;
      dataGrid.HeaderForeColor = SystemColors.ControlText;
      dataGrid.LinkColor = SystemColors.HotTrack;
      dataGrid.Location = new Point(4, 4);
      dataGrid.Name = "dataGrid";
      dataGrid.SelectionBackColor = SystemColors.ActiveCaption;
      dataGrid.SelectionForeColor = SystemColors.ActiveCaptionText;
      dataGrid.Size = new Size(368, 216);
      dataGrid.TabIndex = 0;
      // 
      // MpeStringDesigner
      // 
      Controls.Add(dataGrid);
      DockPadding.All = 4;
      Name = "MpeStringDesigner";
      //Size = new Size(376, 224);
      ((ISupportInitialize) (dataGrid)).EndInit();
      ResumeLayout(false);
    }

    #endregion

    private void OnStringTableRowChanged(object sender, DataRowChangeEventArgs e)
    {
      mpe.ToggleDesignerStatus(ResourceName, true);
    }
  }
}