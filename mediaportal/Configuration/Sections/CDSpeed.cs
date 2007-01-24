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
using System.IO;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using Un4seen.Bass.AddOn.Cd;

namespace MediaPortal.Configuration.Sections
{
  public partial class CDSpeed : SectionSettings
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
                  string currentValue = (string)row.ItemArray[Cell];
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
        { }
      }
    }

    private DataTable datasetFilters;
    private string _speedTable;
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

    public CDSpeed()
      : this("CD/DVD Speed")
    { }

    public CDSpeed(string name)
      : base(name)
    {
      InitializeComponent();
      // Load the CD Plugin
      string appPath = System.Windows.Forms.Application.StartupPath;
      string decoderFolderPath = Path.Combine(appPath, @"musicplayer\plugins\audio decoders");

      if (!Directory.Exists(decoderFolderPath))
      {
        Log.Error(@"BASS: Unable to find \musicplayer\plugins\audio decoders folder in MediaPortal.exe path.");
        return;
      }

      int pluginHandle = Un4seen.Bass.Bass.BASS_PluginLoad(decoderFolderPath + "\\basscd.dll");

      _driveCount = BassCd.BASS_CD_GetDriveCount();
    }

    private void FillGrid()
    {
      updating = true;
      //Declare and initialize local variables used
      DataColumn dtCol = null;                  //Data Column variable
      string[] arrColumnNames = null;           //string array variable
      SyncedComboBox cbSpeed;                   //combo box var         

      //Create the String array object, initialize the array with the column
      //names to be displayed
      arrColumnNames = new string[3];
      arrColumnNames[0] = "Drive";
      arrColumnNames[1] = "Name";
      arrColumnNames[2] = "Speed";

      //Create the Data Table object which will then be used to hold
      //columns and rows
      datasetFilters = new DataTable("Drive");
      //Add the string array of columns to the DataColumn object       
      for (int i = 0; i < arrColumnNames.Length; i++)
      {
        string str = arrColumnNames[i];
        dtCol = new DataColumn(str);
        dtCol.DataType = Type.GetType("System.String");
        dtCol.DefaultValue = "";
        datasetFilters.Columns.Add(dtCol);
      }

      cbSpeed = new SyncedComboBox();
      cbSpeed.Cursor = Cursors.Arrow;
      cbSpeed.DropDownStyle = ComboBoxStyle.DropDownList;
      cbSpeed.Dock = DockStyle.Fill;
      cbSpeed.DisplayMember = "Speed";
      cbSpeed.Grid = dataGrid1;
      cbSpeed.Cell = 1;
      cbSpeed.Items.AddRange(speeds);

      //Event that will be fired when selected index in the combo box is changed
      cbSpeed.SelectionChangeCommitted += new EventHandler(cbSpeed_SelectionChangeCommitted);


      //fill in all rows...
      string[] drivespeed = _speedTable.Split(',');
      for (int i = 0; i < _driveCount; ++i)
      {
        datasetFilters.Rows.Add(
            new object[] {
                           BassCd.BASS_CD_GetDriveLetterChar(i), BassCd.BASS_CD_GetDriveDescription(i),
                           drivespeed[i]
												 }
                               );
      }

      //Set the Data Grid Source as the Data Table created above
      dataGrid1.CaptionText = String.Empty;
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
        dgdtblStyle.HeaderFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, ((Byte)(0)));
        dgdtblStyle.GridLineColor = Color.DarkGray;
        dgdtblStyle.PreferredRowHeight = 22;
        dataGrid1.BackgroundColor = Color.White;

        //Take the columns in a GridColumnStylesCollection object and set //the size of the
        //individual columns   
        GridColumnStylesCollection colStyle;
        colStyle = dataGrid1.TableStyles[0].GridColumnStyles;
        colStyle[0].Width = 60;
        colStyle[0].ReadOnly = true;
        colStyle[1].Width = 300;
        colStyle[1].ReadOnly = true;
        colStyle[2].Width = 60;
      }
      DataGridTextBoxColumn dgtb = (DataGridTextBoxColumn)dataGrid1.TableStyles[0].GridColumnStyles[2];
      //Add the combo box to the text box taken in the above step 
      dgtb.TextBox.Controls.Add(cbSpeed);
      updating = false;
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

      table.Rows[currentCell.RowNumber][currentCell.ColumnNumber] = (string)box.SelectedItem;
    }


    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings reader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _speedTable = reader.GetValueAsString("cdspeed", "drivespeed", String.Empty);
      }

      if (_speedTable == String.Empty)
      {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < _driveCount; i++)
        {
          if (builder.Length != 0)
            builder.Append(", ");

          float maxspeed = BassCd.BASS_CD_GetSpeedFactor(i);
          builder.Append(Convert.ToString(maxspeed));
        }
        _speedTable = builder.ToString();
      }
      FillGrid();
    }

    public override void SaveSettings()
    {
      DataTable dt = dataGrid1.DataSource as DataTable;

      StringBuilder builder = new StringBuilder();
      BASS_CD_INFO cdinfo = new BASS_CD_INFO();

      int i = 0;
      foreach (DataRow row in dt.Rows)
      {
        if (builder.Length != 0)
          builder.Append(",");

        BassCd.BASS_CD_GetInfo(i, cdinfo);
        int selectedSpeed = Convert.ToInt32(row[2].ToString());
        int maxspeed = (int)(cdinfo.maxspeed / 176.4);

        if (selectedSpeed > maxspeed)
          selectedSpeed = maxspeed;

        builder.Append(selectedSpeed.ToString());
        i++;
      }

      using (MediaPortal.Profile.Settings writer = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        writer.SetValue("cdspeed", "drivespeed", builder.ToString());
      }
    }


  }
}
