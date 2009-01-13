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
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;

namespace MediaPortal.Configuration.Sections
{
  public partial class GeneralCDSpeed : SectionSettings
  {
    public class SyncedComboBox : ComboBox
    {
      private DataGrid grid;
      private int cell;

      public int Cell
      {
        get { return cell; }
        set { cell = value; }
      }

      public DataGrid Grid
      {
        get { return grid; }
        set { grid = value; }
      }

      protected override void OnLayout(LayoutEventArgs levent)
      {
        try
        {
          DataGridCell currentCell = Grid.CurrentCell;

          if (currentCell.ColumnNumber == Cell)
          {
            DataTable ds = Grid.DataSource as DataTable;
            if (ds != null)
            {
              foreach (string item in Items)
              {
                if (currentCell.RowNumber < ds.Rows.Count)
                {
                  DataRow row = ds.Rows[currentCell.RowNumber];
                  string currentValue = (string) row.ItemArray[Cell];
                  if (currentValue == item)
                  {
                    SelectedItem = item;
                    break;
                  }
                }
              }
            }
          }
          base.OnLayout(levent);
        }
        catch (Exception)
        {
        }
      }
    }

    public class SyncedCheckBox : CheckBox
    {
      private DataGrid grid;
      private int cell;

      public int Cell
      {
        get { return cell; }
        set { cell = value; }
      }

      public DataGrid Grid
      {
        get { return grid; }
        set { grid = value; }
      }

      protected override void OnLayout(LayoutEventArgs levent)
      {
        DataGridCell currentCell = Grid.CurrentCell;

        if (currentCell.ColumnNumber == Cell)
        {
          DataTable ds = Grid.DataSource as DataTable;
          if (ds != null)
          {
            if (currentCell.RowNumber < ds.Rows.Count)
            {
              DataRow row = ds.Rows[currentCell.RowNumber];
              this.Checked = (bool) row.ItemArray[Cell];
            }
          }
        }
        base.OnLayout(levent);
      }
    }

    private DataTable datasetFilters;
    private string _speedTableCD;
    private string _disableCD;
    private string _speedTableDVD;
    private string _disableDVD;
    private int _driveCount;
    private bool updating = false;

    private string[] speeds = new string[]
                                {
                                  "2",
                                  "4",
                                  "6",
                                  "8",
                                  "12",
                                  "16",
                                  "24",
                                  "32",
                                  "48",
                                  "52",
                                };

    public GeneralCDSpeed()
      : this("CD/DVD Speed")
    {
    }

    public GeneralCDSpeed(string name)
      : base(name)
    {
      InitializeComponent();
      // Load the CD Plugin
      string appPath = Application.StartupPath;
      string decoderFolderPath = Path.Combine(appPath, @"musicplayer\plugins\audio decoders");

      if (!Directory.Exists(decoderFolderPath))
      {
        Log.Error(@"BASS: Unable to find \musicplayer\plugins\audio decoders folder in MediaPortal.exe path.");
        return;
      }

      int pluginHandle = Bass.BASS_PluginLoad(decoderFolderPath + "\\basscd.dll");

      _driveCount = BassCd.BASS_CD_GetDriveCount();
    }

    private void FillGrid()
    {
      try
      {
        updating = true;
        //Declare and initialize local variables used
        DataColumn dtCol = null; //Data Column variable
        SyncedComboBox cbSpeed; //combo box var         
        SyncedComboBox cbSpeedDVD; //combo box var  
        SyncedCheckBox ckDisableDVD;

        //Create the Data Table object which will then be used to hold
        //columns and rows
        datasetFilters = new DataTable("Drive");
        //Add the columns to the DataColumn object
        dtCol = new DataColumn("Drive");
        dtCol.DataType = Type.GetType("System.String");
        dtCol.DefaultValue = "";
        datasetFilters.Columns.Add(dtCol);

        dtCol = new DataColumn("Name");
        dtCol.DataType = Type.GetType("System.String");
        dtCol.DefaultValue = "";
        datasetFilters.Columns.Add(dtCol);

        dtCol = new DataColumn("CD");
        dtCol.DataType = Type.GetType("System.String");
        dtCol.DefaultValue = "";
        datasetFilters.Columns.Add(dtCol);

        dtCol = new DataColumn("DisableCD");
        dtCol.DataType = Type.GetType("System.Boolean");
        dtCol.DefaultValue = false;
        dtCol.AllowDBNull = false;
        datasetFilters.Columns.Add(dtCol);

        dtCol = new DataColumn("DVD");
        dtCol.DataType = Type.GetType("System.String");
        dtCol.DefaultValue = "";
        datasetFilters.Columns.Add(dtCol);

        dtCol = new DataColumn("DisableDVD");
        dtCol.DataType = Type.GetType("System.Boolean");
        dtCol.DefaultValue = false;
        dtCol.AllowDBNull = false;
        datasetFilters.Columns.Add(dtCol);

        cbSpeed = new SyncedComboBox();
        cbSpeed.Cursor = Cursors.Arrow;
        cbSpeed.DropDownStyle = ComboBoxStyle.DropDownList;
        cbSpeed.Dock = DockStyle.Fill;
        cbSpeed.DisplayMember = "CD";
        cbSpeed.Grid = dataGrid1;
        cbSpeed.Cell = 1;
        cbSpeed.Items.AddRange(speeds);

        cbSpeedDVD = new SyncedComboBox();
        cbSpeedDVD.Cursor = Cursors.Arrow;
        cbSpeedDVD.DropDownStyle = ComboBoxStyle.DropDownList;
        cbSpeedDVD.Dock = DockStyle.Fill;
        cbSpeedDVD.DisplayMember = "DVD";
        cbSpeedDVD.Grid = dataGrid1;
        cbSpeedDVD.Cell = 1;
        cbSpeedDVD.Items.AddRange(speeds);

        ckDisableDVD = new SyncedCheckBox();
        ckDisableDVD.Cursor = Cursors.Arrow;
        ckDisableDVD.Cell = 1;
        ckDisableDVD.Grid = dataGrid1;

        //Event that will be fired when selected index in the combo box is changed
        cbSpeed.SelectionChangeCommitted += new EventHandler(cbSpeed_SelectionChangeCommitted);
        cbSpeedDVD.SelectionChangeCommitted += new EventHandler(cbSpeed_SelectionChangeCommitted);


        //fill in all rows...
        string[] drivespeedCD = _speedTableCD.Split(',');
        string[] drivespeedDVD = _speedTableDVD.Split(',');
        string[] disableCD = _disableCD.Split(',');
        string[] disableDVD = _disableDVD.Split(',');
        for (int i = 0; i < _driveCount; ++i)
        {
          bool disCD = (disableCD[i] == "Y");
          bool disDVD = (disableDVD[i] == "Y");
          datasetFilters.Rows.Add(
            new object[]
              {
                BassCd.BASS_CD_GetDriveLetterChar(i), BassCd.BASS_CD_GetDriveDescription(i),
                drivespeedCD[i], disCD, drivespeedDVD[i], disDVD
              }
            );
        }

        //Set the Data Grid Source as the Data Table created above
        dataGrid1.CaptionText = string.Empty;
        dataGrid1.DataSource = datasetFilters;

        //set style property when first time the grid loads, next time onwards it //will maintain its property
        if (!dataGrid1.TableStyles.Contains("Drive"))
        {
          //Create a DataGridTableStyle object     
          DataGridTableStyle dgdtblStyle = new DataGridTableStyle();
          //Set its properties
          dgdtblStyle.MappingName = datasetFilters.TableName; //its table name of dataset
          dataGrid1.TableStyles.Add(dgdtblStyle);
          dgdtblStyle.RowHeadersVisible = false;
          dgdtblStyle.HeaderBackColor = Color.LightSteelBlue;
          dgdtblStyle.AllowSorting = false;
          dgdtblStyle.HeaderBackColor = Color.FromArgb(8, 36, 107);
          dgdtblStyle.RowHeadersVisible = false;
          dgdtblStyle.HeaderForeColor = Color.White;
          dgdtblStyle.HeaderFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, ((Byte) (0)));
          dgdtblStyle.GridLineColor = Color.DarkGray;
          dgdtblStyle.PreferredRowHeight = 22;
          dataGrid1.BackgroundColor = Color.White;

          //Take the columns in a GridColumnStylesCollection object and set //the size of the
          //individual columns   
          GridColumnStylesCollection colStyle;
          colStyle = dataGrid1.TableStyles[0].GridColumnStyles;
          colStyle[0].Width = 40;
          colStyle[0].ReadOnly = true;
          colStyle[1].Width = 265;
          colStyle[1].ReadOnly = true;
          colStyle[2].Width = 50;
          colStyle[3].Width = 15;
          colStyle[4].Width = 50;
          colStyle[5].Width = 15;
        }
        DataGridTextBoxColumn dgtb = (DataGridTextBoxColumn) dataGrid1.TableStyles[0].GridColumnStyles[2];
        //Add the combo box to the text box taken in the above step 
        dgtb.TextBox.Controls.Add(cbSpeed);
        dgtb = (DataGridTextBoxColumn) dataGrid1.TableStyles[0].GridColumnStyles[4];
        dgtb.TextBox.Controls.Add(cbSpeedDVD);
        updating = false;
      }
      catch (Exception ex)
      {
        Log.Error("Exception in CDSpeed FillGrid");
        Log.Error(ex);
      }
    }

    private void cbSpeed_SelectionChangeCommitted(object sender, EventArgs e)
    {
      if (updating)
      {
        return;
      }
      SyncedComboBox box = sender as SyncedComboBox;
      if (box == null)
      {
        return;
      }
      DataGridCell currentCell = dataGrid1.CurrentCell;
      DataTable table = dataGrid1.DataSource as DataTable;

      table.Rows[currentCell.RowNumber][currentCell.ColumnNumber] = (string) box.SelectedItem;
    }


    public override void LoadSettings()
    {
      using (Settings reader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        ckEnableCDSpeed.Checked = reader.GetValueAsBool("cdspeed", "enabled", false);
        _speedTableCD = reader.GetValueAsString("cdspeed", "drivespeedCD", string.Empty);
        _disableCD = reader.GetValueAsString("cdspeed", "disableCD", string.Empty);
        _speedTableDVD = reader.GetValueAsString("cdspeed", "drivespeedDVD", string.Empty);
        _disableDVD = reader.GetValueAsString("cdspeed", "disableDVD", string.Empty);
      }

      // On first use, the table are empty and need to be filled with the max speed
      if (_speedTableCD == string.Empty || _speedTableDVD == string.Empty)
      {
        BASS_CD_INFO cdinfo = new BASS_CD_INFO();
        StringBuilder builder = new StringBuilder();
        StringBuilder builderDisable = new StringBuilder();
        for (int i = 0; i < _driveCount; i++)
        {
          if (builder.Length != 0)
          {
            builder.Append(", ");
          }

          if (builderDisable.Length != 0)
          {
            builderDisable.Append(", ");
          }

          BassCd.BASS_CD_GetInfo(i, cdinfo);
          int maxspeed = (int) (cdinfo.maxspeed/176.4);
          builder.Append(Convert.ToString(maxspeed));
          builderDisable.Append("N");
        }
        _speedTableCD = builder.ToString();
        _speedTableDVD = builder.ToString();
        _disableCD = builderDisable.ToString();
        _disableDVD = builderDisable.ToString();
      }
      FillGrid();
    }

    public override void SaveSettings()
    {
      DataTable dt = dataGrid1.DataSource as DataTable;

      StringBuilder builderCD = new StringBuilder();
      StringBuilder builderDVD = new StringBuilder();
      StringBuilder builderDisableCD = new StringBuilder();
      StringBuilder builderDisableDVD = new StringBuilder();
      BASS_CD_INFO cdinfo = new BASS_CD_INFO();

      int i = 0;
      try
      {
        foreach (DataRow row in dt.Rows)
        {
          if (builderCD.Length != 0)
          {
            builderCD.Append(",");
          }

          if (builderDVD.Length != 0)
          {
            builderDVD.Append(",");
          }

          if (builderDisableCD.Length != 0)
          {
            builderDisableCD.Append(",");
          }

          if (builderDisableDVD.Length != 0)
          {
            builderDisableDVD.Append(",");
          }

          BassCd.BASS_CD_GetInfo(i, cdinfo);
          int maxspeed = (int) (cdinfo.maxspeed/176.4);
          int selectedSpeedCD = int.Parse((row[2].ToString()));
          int selectedSpeedDVD = int.Parse((row[4].ToString()));

          if (selectedSpeedCD > maxspeed)
          {
            selectedSpeedCD = maxspeed;
          }

          if (selectedSpeedDVD > maxspeed)
          {
            selectedSpeedDVD = maxspeed;
          }

          builderCD.Append(selectedSpeedCD.ToString());
          builderDVD.Append(selectedSpeedDVD.ToString());

          string sel = (bool) row[3] ? "Y" : "N";
          builderDisableCD.Append(sel);

          sel = (bool) row[5] ? "Y" : "N";
          builderDisableDVD.Append(sel);

          i++;
        }
      }
      catch (Exception)
      {
      }
      using (Settings writer = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        writer.SetValueAsBool("cdspeed", "enabled", ckEnableCDSpeed.Checked);
        writer.SetValue("cdspeed", "drivespeedCD", builderCD.ToString());
        writer.SetValue("cdspeed", "drivespeedDVD", builderDVD.ToString());
        writer.SetValue("cdspeed", "disableCD", builderDisableCD.ToString());
        writer.SetValue("cdspeed", "disableDVD", builderDisableDVD.ToString());
      }
    }
  }
}