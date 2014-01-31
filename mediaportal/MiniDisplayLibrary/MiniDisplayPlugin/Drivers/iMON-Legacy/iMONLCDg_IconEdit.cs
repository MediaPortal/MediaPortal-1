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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class iMONLCDg_IconEdit : MPConfigForm
  {
    private static readonly byte[,] _IconBuffer = new byte[10,0x20];

    private readonly string[] IconFunction = new[]
                                               {
                                                 "( Idle )", "( TV )", "( MOVIE )", "( MUSIC )", "( VIDEO )",
                                                 "( RECORDING )",
                                                 "( PAUSED )", "", "", ""
                                               };

    private readonly Bitmap[] IconGraphics = new Bitmap[10];
    private CheckBox C0R0_B0;
    private CheckBox C0R0_B1;
    private CheckBox C0R0_B2;
    private CheckBox C0R0_B3;
    private CheckBox C0R0_B4;
    private CheckBox C0R0_B5;
    private CheckBox C0R0_B6;
    private CheckBox C0R0_B7;
    private CheckBox C0R1_B0;
    private CheckBox C0R1_B1;
    private CheckBox C0R1_B2;
    private CheckBox C0R1_B3;
    private CheckBox C0R1_B4;
    private CheckBox C0R1_B5;
    private CheckBox C0R1_B6;
    private CheckBox C0R1_B7;
    private CheckBox C10R0_B0;
    private CheckBox C10R0_B1;
    private CheckBox C10R0_B2;
    private CheckBox C10R0_B3;
    private CheckBox C10R0_B4;
    private CheckBox C10R0_B5;
    private CheckBox C10R0_B6;
    private CheckBox C10R0_B7;
    private CheckBox C10R1_B0;
    private CheckBox C10R1_B1;
    private CheckBox C10R1_B2;
    private CheckBox C10R1_B3;
    private CheckBox C10R1_B4;
    private CheckBox C10R1_B5;
    private CheckBox C10R1_B6;
    private CheckBox C10R1_B7;
    private CheckBox C11R0_B0;
    private CheckBox C11R0_B1;
    private CheckBox C11R0_B2;
    private CheckBox C11R0_B3;
    private CheckBox C11R0_B4;
    private CheckBox C11R0_B5;
    private CheckBox C11R0_B6;
    private CheckBox C11R0_B7;
    private CheckBox C11R1_B0;
    private CheckBox C11R1_B1;
    private CheckBox C11R1_B2;
    private CheckBox C11R1_B3;
    private CheckBox C11R1_B4;
    private CheckBox C11R1_B5;
    private CheckBox C11R1_B6;
    private CheckBox C11R1_B7;
    private CheckBox C12R0_B0;
    private CheckBox C12R0_B1;
    private CheckBox C12R0_B2;
    private CheckBox C12R0_B3;
    private CheckBox C12R0_B4;
    private CheckBox C12R0_B5;
    private CheckBox C12R0_B6;
    private CheckBox C12R0_B7;
    private CheckBox C12R1_B0;
    private CheckBox C12R1_B1;
    private CheckBox C12R1_B2;
    private CheckBox C12R1_B3;
    private CheckBox C12R1_B4;
    private CheckBox C12R1_B5;
    private CheckBox C12R1_B6;
    private CheckBox C12R1_B7;
    private CheckBox C13R0_B0;
    private CheckBox C13R0_B1;
    private CheckBox C13R0_B2;
    private CheckBox C13R0_B3;
    private CheckBox C13R0_B4;
    private CheckBox C13R0_B5;
    private CheckBox C13R0_B6;
    private CheckBox C13R0_B7;
    private CheckBox C13R1_B0;
    private CheckBox C13R1_B1;
    private CheckBox C13R1_B2;
    private CheckBox C13R1_B3;
    private CheckBox C13R1_B4;
    private CheckBox C13R1_B5;
    private CheckBox C13R1_B6;
    private CheckBox C13R1_B7;
    private CheckBox C14R0_B0;
    private CheckBox C14R0_B1;
    private CheckBox C14R0_B2;
    private CheckBox C14R0_B3;
    private CheckBox C14R0_B4;
    private CheckBox C14R0_B5;
    private CheckBox C14R0_B6;
    private CheckBox C14R0_B7;
    private CheckBox C14R1_B0;
    private CheckBox C14R1_B1;
    private CheckBox C14R1_B2;
    private CheckBox C14R1_B3;
    private CheckBox C14R1_B4;
    private CheckBox C14R1_B5;
    private CheckBox C14R1_B6;
    private CheckBox C14R1_B7;
    private CheckBox C15R0_B0;
    private CheckBox C15R0_B1;
    private CheckBox C15R0_B2;
    private CheckBox C15R0_B3;
    private CheckBox C15R0_B4;
    private CheckBox C15R0_B5;
    private CheckBox C15R0_B6;
    private CheckBox C15R0_B7;
    private CheckBox C15R1_B0;
    private CheckBox C15R1_B1;
    private CheckBox C15R1_B2;
    private CheckBox C15R1_B3;
    private CheckBox C15R1_B4;
    private CheckBox C15R1_B5;
    private CheckBox C15R1_B6;
    private CheckBox C15R1_B7;
    private CheckBox C1R0_B0;
    private CheckBox C1R0_B1;
    private CheckBox C1R0_B2;
    private CheckBox C1R0_B3;
    private CheckBox C1R0_B4;
    private CheckBox C1R0_B5;
    private CheckBox C1R0_B6;
    private CheckBox C1R0_B7;
    private CheckBox C1R1_B0;
    private CheckBox C1R1_B1;
    private CheckBox C1R1_B2;
    private CheckBox C1R1_B3;
    private CheckBox C1R1_B4;
    private CheckBox C1R1_B5;
    private CheckBox C1R1_B6;
    private CheckBox C1R1_B7;
    private CheckBox C2R0_B0;
    private CheckBox C2R0_B1;
    private CheckBox C2R0_B2;
    private CheckBox C2R0_B3;
    private CheckBox C2R0_B4;
    private CheckBox C2R0_B5;
    private CheckBox C2R0_B6;
    private CheckBox C2R0_B7;
    private CheckBox C2R1_B0;
    private CheckBox C2R1_B1;
    private CheckBox C2R1_B2;
    private CheckBox C2R1_B3;
    private CheckBox C2R1_B4;
    private CheckBox C2R1_B5;
    private CheckBox C2R1_B6;
    private CheckBox C2R1_B7;
    private CheckBox C3R0_B0;
    private CheckBox C3R0_B1;
    private CheckBox C3R0_B2;
    private CheckBox C3R0_B3;
    private CheckBox C3R0_B4;
    private CheckBox C3R0_B5;
    private CheckBox C3R0_B6;
    private CheckBox C3R0_B7;
    private CheckBox C3R1_B0;
    private CheckBox C3R1_B1;
    private CheckBox C3R1_B2;
    private CheckBox C3R1_B3;
    private CheckBox C3R1_B4;
    private CheckBox C3R1_B5;
    private CheckBox C3R1_B6;
    private CheckBox C3R1_B7;
    private CheckBox C4R0_B0;
    private CheckBox C4R0_B1;
    private CheckBox C4R0_B2;
    private CheckBox C4R0_B3;
    private CheckBox C4R0_B4;
    private CheckBox C4R0_B5;
    private CheckBox C4R0_B6;
    private CheckBox C4R0_B7;
    private CheckBox C4R1_B0;
    private CheckBox C4R1_B1;
    private CheckBox C4R1_B2;
    private CheckBox C4R1_B3;
    private CheckBox C4R1_B4;
    private CheckBox C4R1_B5;
    private CheckBox C4R1_B6;
    private CheckBox C4R1_B7;
    private CheckBox C5R0_B0;
    private CheckBox C5R0_B1;
    private CheckBox C5R0_B2;
    private CheckBox C5R0_B3;
    private CheckBox C5R0_B4;
    private CheckBox C5R0_B5;
    private CheckBox C5R0_B6;
    private CheckBox C5R0_B7;
    private CheckBox C5R1_B0;
    private CheckBox C5R1_B1;
    private CheckBox C5R1_B2;
    private CheckBox C5R1_B3;
    private CheckBox C5R1_B4;
    private CheckBox C5R1_B5;
    private CheckBox C5R1_B6;
    private CheckBox C5R1_B7;
    private CheckBox C6R0_B0;
    private CheckBox C6R0_B1;
    private CheckBox C6R0_B2;
    private CheckBox C6R0_B3;
    private CheckBox C6R0_B4;
    private CheckBox C6R0_B5;
    private CheckBox C6R0_B6;
    private CheckBox C6R0_B7;
    private CheckBox C6R1_B0;
    private CheckBox C6R1_B1;
    private CheckBox C6R1_B2;
    private CheckBox C6R1_B3;
    private CheckBox C6R1_B4;
    private CheckBox C6R1_B5;
    private CheckBox C6R1_B6;
    private CheckBox C6R1_B7;
    private CheckBox C7R0_B0;
    private CheckBox C7R0_B1;
    private CheckBox C7R0_B2;
    private CheckBox C7R0_B3;
    private CheckBox C7R0_B4;
    private CheckBox C7R0_B5;
    private CheckBox C7R0_B6;
    private CheckBox C7R0_B7;
    private CheckBox C7R1_B0;
    private CheckBox C7R1_B1;
    private CheckBox C7R1_B2;
    private CheckBox C7R1_B3;
    private CheckBox C7R1_B4;
    private CheckBox C7R1_B5;
    private CheckBox C7R1_B6;
    private CheckBox C7R1_B7;
    private CheckBox C8R0_B0;
    private CheckBox C8R0_B1;
    private CheckBox C8R0_B2;
    private CheckBox C8R0_B3;
    private CheckBox C8R0_B4;
    private CheckBox C8R0_B5;
    private CheckBox C8R0_B6;
    private CheckBox C8R0_B7;
    private CheckBox C8R1_B0;
    private CheckBox C8R1_B1;
    private CheckBox C8R1_B2;
    private CheckBox C8R1_B3;
    private CheckBox C8R1_B4;
    private CheckBox C8R1_B5;
    private CheckBox C8R1_B6;
    private CheckBox C8R1_B7;
    private CheckBox C9R0_B0;
    private CheckBox C9R0_B1;
    private CheckBox C9R0_B2;
    private CheckBox C9R0_B3;
    private CheckBox C9R0_B4;
    private CheckBox C9R0_B5;
    private CheckBox C9R0_B6;
    private CheckBox C9R0_B7;
    private CheckBox C9R1_B0;
    private CheckBox C9R1_B1;
    private CheckBox C9R1_B2;
    private CheckBox C9R1_B3;
    private CheckBox C9R1_B4;
    private CheckBox C9R1_B5;
    private CheckBox C9R1_B6;
    private CheckBox C9R1_B7;
    private Button cmdCancelEdit;
    private Button cmdClearAll;
    private Button cmdExit;
    private Button cmdInvert;
    private Button cmdLoadCustom;
    private Button cmdLoadInternal;
    private Button cmdSave;
    private Button cmdSaveEdit;
    private Button cmdSetAll;
    private int EditIndex = -1;
    private PictureBox Icon0;
    private PictureBox Icon1;
    private PictureBox Icon2;
    private PictureBox Icon3;
    private PictureBox Icon4;
    private PictureBox Icon5;
    private PictureBox Icon6;
    private PictureBox Icon7;
    private PictureBox Icon8;
    private PictureBox Icon9;

    private bool IconsChanged;
    private Label label1;
    private Label label10;
    private Label label11;
    private Label label12;
    private Label label13;
    private Label label14;
    private Label label15;
    private Label label16;
    private Label label17;
    private Label label18;
    private Label label19;
    private Label label2;
    private Label label20;
    private Label label21;
    private Label label3;
    private Label label4;
    private Label label5;
    private Label label6;
    private Label label7;
    private Label label8;
    private Label label9;
    private Label lblCurrentIcon;
    private Label lblEditIndex;
    private Panel panel1;

    public iMONLCDg_IconEdit()
    {
      InitializeComponent();
    }

    private static void ClearIconBuffer()
    {
      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 0x20; j++)
        {
          _IconBuffer[i, j] = 0;
        }
      }
    }

    private void ClearIconDisplay()
    {
      ClearIconBuffer();
      CopyBufferToGraphics();
      EnableIconSelection(false);
    }

    private void cmdCancelEdit_Click(object sender, EventArgs e)
    {
      cmdClearAll_Click(null, null);
      lblCurrentIcon.Text = "";
      EditIndex = -1;
      EnableEditPanel(false);
    }

    private void cmdClearAll_Click(object sender, EventArgs e)
    {
      for (int i = 0; i < 0x10; i++)
      {
        for (int j = 0; j < 0x10; j++)
        {
          SetEditPixel(i, j, false);
        }
      }
    }

    private void cmdExit_Click(object sender, EventArgs e)
    {
      Hide();
      Close();
    }

    private void cmdInvert_Click(object sender, EventArgs e)
    {
      for (int i = 0; i < 0x10; i++)
      {
        for (int j = 0; j < 0x10; j++)
        {
          switch (GetEditPixel(i, j))
          {
            case CheckState.Unchecked:
              SetEditPixel(i, j, true);
              break;

            case CheckState.Checked:
            case CheckState.Indeterminate:
              SetEditPixel(i, j, false);
              break;
          }
        }
      }
    }

    private void cmdLoadCustom_Click(object sender, EventArgs e)
    {
      LoadCustomIcons();
      EnableIconSelection(true);
    }

    private void cmdLoadInternal_Click(object sender, EventArgs e)
    {
      LoadInteralIcons();
      EnableIconSelection(true);
    }

    private void cmdSave_Click(object sender, EventArgs e)
    {
      if (IconsChanged)
      {
        var o = new DataTable("LargeIcons");
        var column = new DataColumn("IconID");
        var column2 = new DataColumn("IData0");
        var column3 = new DataColumn("IData1");
        var column4 = new DataColumn("IData2");
        var column5 = new DataColumn("IData3");
        var column6 = new DataColumn("IData4");
        var column7 = new DataColumn("IData5");
        var column8 = new DataColumn("IData6");
        var column9 = new DataColumn("IData7");
        var column10 = new DataColumn("IData8");
        var column11 = new DataColumn("IData9");
        var column12 = new DataColumn("IData10");
        var column13 = new DataColumn("IData11");
        var column14 = new DataColumn("IData12");
        var column15 = new DataColumn("IData13");
        var column16 = new DataColumn("IData14");
        var column17 = new DataColumn("IData15");
        var column18 = new DataColumn("IData16");
        var column19 = new DataColumn("IData17");
        var column20 = new DataColumn("IData18");
        var column21 = new DataColumn("IData19");
        var column22 = new DataColumn("IData20");
        var column23 = new DataColumn("IData21");
        var column24 = new DataColumn("IData22");
        var column25 = new DataColumn("IData23");
        var column26 = new DataColumn("IData24");
        var column27 = new DataColumn("IData25");
        var column28 = new DataColumn("IData26");
        var column29 = new DataColumn("IData27");
        var column30 = new DataColumn("IData28");
        var column31 = new DataColumn("IData29");
        var column32 = new DataColumn("IData30");
        var column33 = new DataColumn("IData31");
        column.DataType = typeof (byte);
        o.Columns.Add(column);
        column2.DataType = typeof (byte);
        o.Columns.Add(column2);
        column3.DataType = typeof (byte);
        o.Columns.Add(column3);
        column4.DataType = typeof (byte);
        o.Columns.Add(column4);
        column5.DataType = typeof (byte);
        o.Columns.Add(column5);
        column6.DataType = typeof (byte);
        o.Columns.Add(column6);
        column7.DataType = typeof (byte);
        o.Columns.Add(column7);
        column8.DataType = typeof (byte);
        o.Columns.Add(column8);
        column9.DataType = typeof (byte);
        o.Columns.Add(column9);
        column10.DataType = typeof (byte);
        o.Columns.Add(column10);
        column11.DataType = typeof (byte);
        o.Columns.Add(column11);
        column12.DataType = typeof (byte);
        o.Columns.Add(column12);
        column13.DataType = typeof (byte);
        o.Columns.Add(column13);
        column14.DataType = typeof (byte);
        o.Columns.Add(column14);
        column15.DataType = typeof (byte);
        o.Columns.Add(column15);
        column16.DataType = typeof (byte);
        o.Columns.Add(column16);
        column17.DataType = typeof (byte);
        o.Columns.Add(column17);
        column18.DataType = typeof (byte);
        o.Columns.Add(column18);
        column19.DataType = typeof (byte);
        o.Columns.Add(column19);
        column20.DataType = typeof (byte);
        o.Columns.Add(column20);
        column21.DataType = typeof (byte);
        o.Columns.Add(column21);
        column22.DataType = typeof (byte);
        o.Columns.Add(column22);
        column23.DataType = typeof (byte);
        o.Columns.Add(column23);
        column24.DataType = typeof (byte);
        o.Columns.Add(column24);
        column25.DataType = typeof (byte);
        o.Columns.Add(column25);
        column26.DataType = typeof (byte);
        o.Columns.Add(column26);
        column27.DataType = typeof (byte);
        o.Columns.Add(column27);
        column28.DataType = typeof (byte);
        o.Columns.Add(column28);
        column29.DataType = typeof (byte);
        o.Columns.Add(column29);
        column30.DataType = typeof (byte);
        o.Columns.Add(column30);
        column31.DataType = typeof (byte);
        o.Columns.Add(column31);
        column32.DataType = typeof (byte);
        o.Columns.Add(column32);
        column33.DataType = typeof (byte);
        o.Columns.Add(column33);
        o.Clear();
        o.Rows.Clear();
        for (int i = 0; i < 10; i++)
        {
          DataRow row = o.NewRow();
          row[0] = i;
          for (int j = 1; j < 0x21; j++)
          {
            row[j] = _IconBuffer[i, j - 1];
          }
          o.Rows.Add(row);
        }
        var serializer = new XmlSerializer(typeof (DataTable));
        using (
          TextWriter textWriter = new StreamWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_icons.xml")))
        {
          serializer.Serialize(textWriter, o);
          textWriter.Close();
        }

        ClearIconDisplay();
        EnableIconSelection(false);
        cmdSave.Enabled = false;
      }
    }

    private void cmdSaveEdit_Click(object sender, EventArgs e)
    {
      if (EditIndex >= 0)
      {
        for (int i = 0; i < 0x10; i++)
        {
          byte num2 = 0;
          byte num3 = 0;
          for (int j = 0; j < 8; j++)
          {
            num2 =
              (byte)
              (num2 |
               ((byte)
                (((GetEditPixel(i, j + 8) == CheckState.Indeterminate) ? (1) : (0)) *
                 Math.Pow(2.0, j))));
            num3 =
              (byte)
              (num3 |
               ((byte)
                (((GetEditPixel(i, j) == CheckState.Indeterminate) ? (1) : (0)) *
                 Math.Pow(2.0, j))));
          }
          _IconBuffer[EditIndex, i] = num2;
          _IconBuffer[EditIndex, i + 0x10] = num3;
        }
        CopyBufferToGraphics();
        IconsChanged = true;
        cmdSave.Enabled = true;
        cmdClearAll_Click(null, null);
        lblCurrentIcon.Text = "";
        EnableEditPanel(false);
      }
    }

    private void cmdSetAll_Click(object sender, EventArgs e)
    {
      for (int i = 0; i < 0x10; i++)
      {
        for (int j = 0; j < 0x10; j++)
        {
          SetEditPixel(i, j, true);
        }
      }
    }

    public void CopyBufferToGraphics()
    {
      for (int i = 0; i < 10; i++)
      {
        if (IconGraphics[i] == null)
        {
          IconGraphics[i] = new Bitmap(0x20, 0x20);
        }
        for (int j = 0; j < 0x10; j++)
        {
          var buffer = new[] {_IconBuffer[i, j], _IconBuffer[i, j + 0x10]};
          for (int k = 0; k < 0x10; k++)
          {
            int index = (k < 8) ? 1 : 0;
            int num5 = (k < 8) ? k : (k - 8);
            var num6 = (int)Math.Pow(2.0, num5);
            bool flag = (buffer[index] & num6) > 0;
            int x = j * 2;
            int y = 0x1f - (k * 2);
            Color black = flag ? Color.Black : Color.White;
            IconGraphics[i].SetPixel(x, y, black);
            IconGraphics[i].SetPixel(x + 1, y, black);
            IconGraphics[i].SetPixel(x, y - 1, black);
            IconGraphics[i].SetPixel(x + 1, y - 1, black);
          }
        }
        switch (i)
        {
          case 0:
            Icon0.Image = IconGraphics[i];
            break;

          case 1:
            Icon1.Image = IconGraphics[i];
            break;

          case 2:
            Icon2.Image = IconGraphics[i];
            break;

          case 3:
            Icon3.Image = IconGraphics[i];
            break;

          case 4:
            Icon4.Image = IconGraphics[i];
            break;

          case 5:
            Icon5.Image = IconGraphics[i];
            break;

          case 6:
            Icon6.Image = IconGraphics[i];
            break;

          case 7:
            Icon7.Image = IconGraphics[i];
            break;

          case 8:
            Icon8.Image = IconGraphics[i];
            break;

          case 9:
            Icon9.Image = IconGraphics[i];
            break;
        }
      }
    }

    private void DisplayIconForEditing(int IconIndex)
    {
      for (int i = 0; i < 0x10; i++)
      {
        var buffer = new[] {_IconBuffer[IconIndex, i], _IconBuffer[IconIndex, i + 0x10]};
        for (int j = 0; j < 0x10; j++)
        {
          int index = (j < 8) ? 1 : 0;
          int num4 = (j < 8) ? j : (j - 8);
          var num5 = (int)Math.Pow(2.0, num4);
          bool setOn = (buffer[index] & num5) > 0;
          SetEditPixel(i, j, setOn);
        }
      }
    }

    private void EnableEditPanel(bool Enable)
    {
      panel1.Enabled = Enable;
      lblCurrentIcon.Enabled = Enable;
      cmdClearAll.Enabled = Enable;
      cmdInvert.Enabled = Enable;
      cmdSetAll.Enabled = Enable;
      cmdSaveEdit.Enabled = Enable;
      cmdCancelEdit.Enabled = Enable;
    }

    private void EnableIconSelection(bool Enable)
    {
      Icon0.Enabled = Enable;
      Icon1.Enabled = Enable;
      Icon2.Enabled = Enable;
      Icon3.Enabled = Enable;
      Icon4.Enabled = Enable;
      Icon5.Enabled = Enable;
      Icon6.Enabled = Enable;
      Icon7.Enabled = Enable;
      Icon8.Enabled = Enable;
      Icon9.Enabled = Enable;
      if (!Enable)
      {
        Icon0.Image = null;
        Icon1.Image = null;
        Icon2.Image = null;
        Icon3.Image = null;
        Icon4.Image = null;
        Icon5.Image = null;
        Icon6.Image = null;
        Icon7.Image = null;
        Icon8.Image = null;
        Icon9.Image = null;
      }
    }

    private CheckState GetEditPixel(int Column, int Row)
    {
      int num = (Row > 7) ? 0 : 1;
      int num2 = (Row < 8) ? Row : (Row - 8);
      string key = "C" + Column.ToString().Trim() + "R" + num.ToString().Trim() + "_B" + num2.ToString().Trim();
      Control[] controlArray = panel1.Controls.Find(key, false);
      if (controlArray.Length > 0)
      {
        var box = (CheckBox)controlArray[0];
        return box.CheckState;
      }
      return CheckState.Unchecked;
    }

    private void Icon_Click(object sender, EventArgs e)
    {
      try
      {
        var box = (PictureBox)sender;
        int index = int.Parse(box.Name.Substring(4));
        lblCurrentIcon.Text = IconFunction[index];
        DisplayIconForEditing(index);
        EditIndex = index;
        EnableEditPanel(true);
      }
      catch (Exception exception)
      {
        Log.Debug("CAUGHT EXCEPTION: {0}", new object[] {exception});
      }
    }

    private void InitializeComponent()
    {
      Icon0 = new PictureBox();
      Icon1 = new PictureBox();
      Icon2 = new PictureBox();
      Icon3 = new PictureBox();
      Icon4 = new PictureBox();
      Icon9 = new PictureBox();
      Icon8 = new PictureBox();
      Icon7 = new PictureBox();
      Icon6 = new PictureBox();
      Icon5 = new PictureBox();
      label1 = new Label();
      label2 = new Label();
      label3 = new Label();
      label4 = new Label();
      label5 = new Label();
      label6 = new Label();
      label7 = new Label();
      label8 = new Label();
      label9 = new Label();
      label10 = new Label();
      label11 = new Label();
      label12 = new Label();
      label13 = new Label();
      label14 = new Label();
      label15 = new Label();
      label16 = new Label();
      label17 = new Label();
      label18 = new Label();
      label19 = new Label();
      label20 = new Label();
      panel1 = new Panel();
      lblEditIndex = new Label();
      cmdSaveEdit = new Button();
      cmdCancelEdit = new Button();
      cmdInvert = new Button();
      cmdSetAll = new Button();
      cmdClearAll = new Button();
      lblCurrentIcon = new Label();
      C15R1_B0 = new CheckBox();
      C15R1_B1 = new CheckBox();
      C15R1_B2 = new CheckBox();
      C15R1_B3 = new CheckBox();
      C15R1_B4 = new CheckBox();
      C15R1_B5 = new CheckBox();
      C15R1_B6 = new CheckBox();
      C15R1_B7 = new CheckBox();
      C15R0_B0 = new CheckBox();
      C15R0_B1 = new CheckBox();
      C15R0_B2 = new CheckBox();
      C15R0_B3 = new CheckBox();
      C15R0_B4 = new CheckBox();
      C15R0_B5 = new CheckBox();
      C15R0_B6 = new CheckBox();
      C15R0_B7 = new CheckBox();
      C14R1_B0 = new CheckBox();
      C14R1_B1 = new CheckBox();
      C14R1_B2 = new CheckBox();
      C14R1_B3 = new CheckBox();
      C14R1_B4 = new CheckBox();
      C14R1_B5 = new CheckBox();
      C14R1_B6 = new CheckBox();
      C14R1_B7 = new CheckBox();
      C14R0_B0 = new CheckBox();
      C14R0_B1 = new CheckBox();
      C14R0_B2 = new CheckBox();
      C14R0_B3 = new CheckBox();
      C14R0_B4 = new CheckBox();
      C14R0_B5 = new CheckBox();
      C14R0_B6 = new CheckBox();
      C14R0_B7 = new CheckBox();
      C13R1_B0 = new CheckBox();
      C13R1_B1 = new CheckBox();
      C13R1_B2 = new CheckBox();
      C13R1_B3 = new CheckBox();
      C13R1_B4 = new CheckBox();
      C13R1_B5 = new CheckBox();
      C13R1_B6 = new CheckBox();
      C13R1_B7 = new CheckBox();
      C13R0_B0 = new CheckBox();
      C13R0_B1 = new CheckBox();
      C13R0_B2 = new CheckBox();
      C13R0_B3 = new CheckBox();
      C13R0_B4 = new CheckBox();
      C13R0_B5 = new CheckBox();
      C13R0_B6 = new CheckBox();
      C13R0_B7 = new CheckBox();
      C12R1_B0 = new CheckBox();
      C12R1_B1 = new CheckBox();
      C12R1_B2 = new CheckBox();
      C12R1_B3 = new CheckBox();
      C12R1_B4 = new CheckBox();
      C12R1_B5 = new CheckBox();
      C12R1_B6 = new CheckBox();
      C12R1_B7 = new CheckBox();
      C12R0_B0 = new CheckBox();
      C12R0_B1 = new CheckBox();
      C12R0_B2 = new CheckBox();
      C12R0_B3 = new CheckBox();
      C12R0_B4 = new CheckBox();
      C12R0_B5 = new CheckBox();
      C12R0_B6 = new CheckBox();
      C12R0_B7 = new CheckBox();
      C11R1_B0 = new CheckBox();
      C11R1_B1 = new CheckBox();
      C11R1_B2 = new CheckBox();
      C11R1_B3 = new CheckBox();
      C11R1_B4 = new CheckBox();
      C11R1_B5 = new CheckBox();
      C11R1_B6 = new CheckBox();
      C11R1_B7 = new CheckBox();
      C11R0_B0 = new CheckBox();
      C11R0_B1 = new CheckBox();
      C11R0_B2 = new CheckBox();
      C11R0_B3 = new CheckBox();
      C11R0_B4 = new CheckBox();
      C11R0_B5 = new CheckBox();
      C11R0_B6 = new CheckBox();
      C11R0_B7 = new CheckBox();
      C10R1_B0 = new CheckBox();
      C10R1_B1 = new CheckBox();
      C10R1_B2 = new CheckBox();
      C10R1_B3 = new CheckBox();
      C10R1_B4 = new CheckBox();
      C10R1_B5 = new CheckBox();
      C10R1_B6 = new CheckBox();
      C10R1_B7 = new CheckBox();
      C10R0_B0 = new CheckBox();
      C10R0_B1 = new CheckBox();
      C10R0_B2 = new CheckBox();
      C10R0_B3 = new CheckBox();
      C10R0_B4 = new CheckBox();
      C10R0_B5 = new CheckBox();
      C10R0_B6 = new CheckBox();
      C10R0_B7 = new CheckBox();
      C9R1_B0 = new CheckBox();
      C9R1_B1 = new CheckBox();
      C9R1_B2 = new CheckBox();
      C9R1_B3 = new CheckBox();
      C9R1_B4 = new CheckBox();
      C9R1_B5 = new CheckBox();
      C9R1_B6 = new CheckBox();
      C9R1_B7 = new CheckBox();
      C9R0_B0 = new CheckBox();
      C9R0_B1 = new CheckBox();
      C9R0_B2 = new CheckBox();
      C9R0_B3 = new CheckBox();
      C9R0_B4 = new CheckBox();
      C9R0_B5 = new CheckBox();
      C9R0_B6 = new CheckBox();
      C9R0_B7 = new CheckBox();
      C8R1_B0 = new CheckBox();
      C8R1_B1 = new CheckBox();
      C8R1_B2 = new CheckBox();
      C8R1_B3 = new CheckBox();
      C8R1_B4 = new CheckBox();
      C8R1_B5 = new CheckBox();
      C8R1_B6 = new CheckBox();
      C8R1_B7 = new CheckBox();
      C8R0_B0 = new CheckBox();
      C8R0_B1 = new CheckBox();
      C8R0_B2 = new CheckBox();
      C8R0_B3 = new CheckBox();
      C8R0_B4 = new CheckBox();
      C8R0_B5 = new CheckBox();
      C8R0_B6 = new CheckBox();
      C8R0_B7 = new CheckBox();
      C7R1_B0 = new CheckBox();
      C7R1_B1 = new CheckBox();
      C7R1_B2 = new CheckBox();
      C7R1_B3 = new CheckBox();
      C7R1_B4 = new CheckBox();
      C7R1_B5 = new CheckBox();
      C7R1_B6 = new CheckBox();
      C7R1_B7 = new CheckBox();
      C7R0_B0 = new CheckBox();
      C7R0_B1 = new CheckBox();
      C7R0_B2 = new CheckBox();
      C7R0_B3 = new CheckBox();
      C7R0_B4 = new CheckBox();
      C7R0_B5 = new CheckBox();
      C7R0_B6 = new CheckBox();
      C7R0_B7 = new CheckBox();
      C6R1_B0 = new CheckBox();
      C6R1_B1 = new CheckBox();
      C6R1_B2 = new CheckBox();
      C6R1_B3 = new CheckBox();
      C6R1_B4 = new CheckBox();
      C6R1_B5 = new CheckBox();
      C6R1_B6 = new CheckBox();
      C6R1_B7 = new CheckBox();
      C6R0_B0 = new CheckBox();
      C6R0_B1 = new CheckBox();
      C6R0_B2 = new CheckBox();
      C6R0_B3 = new CheckBox();
      C6R0_B4 = new CheckBox();
      C6R0_B5 = new CheckBox();
      C6R0_B6 = new CheckBox();
      C6R0_B7 = new CheckBox();
      C5R1_B0 = new CheckBox();
      C5R1_B1 = new CheckBox();
      C5R1_B2 = new CheckBox();
      C5R1_B3 = new CheckBox();
      C5R1_B4 = new CheckBox();
      C5R1_B5 = new CheckBox();
      C5R1_B6 = new CheckBox();
      C5R1_B7 = new CheckBox();
      C5R0_B0 = new CheckBox();
      C5R0_B1 = new CheckBox();
      C5R0_B2 = new CheckBox();
      C5R0_B3 = new CheckBox();
      C5R0_B4 = new CheckBox();
      C5R0_B5 = new CheckBox();
      C5R0_B6 = new CheckBox();
      C5R0_B7 = new CheckBox();
      C4R1_B0 = new CheckBox();
      C4R1_B1 = new CheckBox();
      C4R1_B2 = new CheckBox();
      C4R1_B3 = new CheckBox();
      C4R1_B4 = new CheckBox();
      C4R1_B5 = new CheckBox();
      C4R1_B6 = new CheckBox();
      C4R1_B7 = new CheckBox();
      C4R0_B0 = new CheckBox();
      C4R0_B1 = new CheckBox();
      C4R0_B2 = new CheckBox();
      C4R0_B3 = new CheckBox();
      C4R0_B4 = new CheckBox();
      C4R0_B5 = new CheckBox();
      C4R0_B6 = new CheckBox();
      C4R0_B7 = new CheckBox();
      C3R1_B0 = new CheckBox();
      C3R1_B1 = new CheckBox();
      C3R1_B2 = new CheckBox();
      C3R1_B3 = new CheckBox();
      C3R1_B4 = new CheckBox();
      C3R1_B5 = new CheckBox();
      C3R1_B6 = new CheckBox();
      C3R1_B7 = new CheckBox();
      C3R0_B0 = new CheckBox();
      C3R0_B1 = new CheckBox();
      C3R0_B2 = new CheckBox();
      C3R0_B3 = new CheckBox();
      C3R0_B4 = new CheckBox();
      C3R0_B5 = new CheckBox();
      C3R0_B6 = new CheckBox();
      C3R0_B7 = new CheckBox();
      C2R1_B0 = new CheckBox();
      C2R1_B1 = new CheckBox();
      C2R1_B2 = new CheckBox();
      C2R1_B3 = new CheckBox();
      C2R1_B4 = new CheckBox();
      C2R1_B5 = new CheckBox();
      C2R1_B6 = new CheckBox();
      C2R1_B7 = new CheckBox();
      C2R0_B0 = new CheckBox();
      C2R0_B1 = new CheckBox();
      C2R0_B2 = new CheckBox();
      C2R0_B3 = new CheckBox();
      C2R0_B4 = new CheckBox();
      C2R0_B5 = new CheckBox();
      C2R0_B6 = new CheckBox();
      C2R0_B7 = new CheckBox();
      C1R1_B0 = new CheckBox();
      C1R1_B1 = new CheckBox();
      C1R1_B2 = new CheckBox();
      C1R1_B3 = new CheckBox();
      C1R1_B4 = new CheckBox();
      C1R1_B5 = new CheckBox();
      C1R1_B6 = new CheckBox();
      C1R1_B7 = new CheckBox();
      C1R0_B0 = new CheckBox();
      C1R0_B1 = new CheckBox();
      C1R0_B2 = new CheckBox();
      C1R0_B3 = new CheckBox();
      C1R0_B4 = new CheckBox();
      C1R0_B5 = new CheckBox();
      C1R0_B6 = new CheckBox();
      C1R0_B7 = new CheckBox();
      C0R1_B0 = new CheckBox();
      C0R1_B1 = new CheckBox();
      C0R1_B2 = new CheckBox();
      C0R1_B3 = new CheckBox();
      C0R1_B4 = new CheckBox();
      C0R1_B5 = new CheckBox();
      C0R1_B6 = new CheckBox();
      C0R1_B7 = new CheckBox();
      C0R0_B0 = new CheckBox();
      C0R0_B1 = new CheckBox();
      C0R0_B2 = new CheckBox();
      C0R0_B3 = new CheckBox();
      C0R0_B4 = new CheckBox();
      C0R0_B5 = new CheckBox();
      C0R0_B6 = new CheckBox();
      C0R0_B7 = new CheckBox();
      cmdLoadInternal = new Button();
      cmdLoadCustom = new Button();
      cmdSave = new Button();
      cmdExit = new Button();
      label21 = new Label();
      ((ISupportInitialize)(Icon0)).BeginInit();
      ((ISupportInitialize)(Icon1)).BeginInit();
      ((ISupportInitialize)(Icon2)).BeginInit();
      ((ISupportInitialize)(Icon3)).BeginInit();
      ((ISupportInitialize)(Icon4)).BeginInit();
      ((ISupportInitialize)(Icon9)).BeginInit();
      ((ISupportInitialize)(Icon8)).BeginInit();
      ((ISupportInitialize)(Icon7)).BeginInit();
      ((ISupportInitialize)(Icon6)).BeginInit();
      ((ISupportInitialize)(Icon5)).BeginInit();
      panel1.SuspendLayout();
      SuspendLayout();
      // 
      // Icon0
      // 
      Icon0.BorderStyle = BorderStyle.FixedSingle;
      Icon0.Enabled = false;
      Icon0.Location = new Point(29, 255);
      Icon0.Name = "Icon0";
      Icon0.Size = new Size(34, 34);
      Icon0.TabIndex = 1;
      Icon0.TabStop = false;
      Icon0.Click += Icon_Click;
      // 
      // Icon1
      // 
      Icon1.BorderStyle = BorderStyle.FixedSingle;
      Icon1.Enabled = false;
      Icon1.Location = new Point(104, 255);
      Icon1.Name = "Icon1";
      Icon1.Size = new Size(34, 34);
      Icon1.TabIndex = 2;
      Icon1.TabStop = false;
      Icon1.Click += Icon_Click;
      // 
      // Icon2
      // 
      Icon2.BorderStyle = BorderStyle.FixedSingle;
      Icon2.Enabled = false;
      Icon2.Location = new Point(179, 255);
      Icon2.Name = "Icon2";
      Icon2.Size = new Size(34, 34);
      Icon2.TabIndex = 3;
      Icon2.TabStop = false;
      Icon2.Click += Icon_Click;
      // 
      // Icon3
      // 
      Icon3.BorderStyle = BorderStyle.FixedSingle;
      Icon3.Enabled = false;
      Icon3.Location = new Point(254, 255);
      Icon3.Name = "Icon3";
      Icon3.Size = new Size(34, 34);
      Icon3.TabIndex = 4;
      Icon3.TabStop = false;
      Icon3.Click += Icon_Click;
      // 
      // Icon4
      // 
      Icon4.BorderStyle = BorderStyle.FixedSingle;
      Icon4.Enabled = false;
      Icon4.Location = new Point(329, 255);
      Icon4.Name = "Icon4";
      Icon4.Size = new Size(34, 34);
      Icon4.TabIndex = 5;
      Icon4.TabStop = false;
      Icon4.Click += Icon_Click;
      // 
      // Icon9
      // 
      Icon9.BorderStyle = BorderStyle.FixedSingle;
      Icon9.Enabled = false;
      Icon9.Location = new Point(329, 326);
      Icon9.Name = "Icon9";
      Icon9.Size = new Size(34, 34);
      Icon9.TabIndex = 10;
      Icon9.TabStop = false;
      Icon9.Click += Icon_Click;
      // 
      // Icon8
      // 
      Icon8.BorderStyle = BorderStyle.FixedSingle;
      Icon8.Enabled = false;
      Icon8.Location = new Point(254, 326);
      Icon8.Name = "Icon8";
      Icon8.Size = new Size(34, 34);
      Icon8.TabIndex = 9;
      Icon8.TabStop = false;
      Icon8.Click += Icon_Click;
      // 
      // Icon7
      // 
      Icon7.BorderStyle = BorderStyle.FixedSingle;
      Icon7.Enabled = false;
      Icon7.Location = new Point(179, 326);
      Icon7.Name = "Icon7";
      Icon7.Size = new Size(34, 34);
      Icon7.TabIndex = 8;
      Icon7.TabStop = false;
      Icon7.Click += Icon_Click;
      // 
      // Icon6
      // 
      Icon6.BorderStyle = BorderStyle.FixedSingle;
      Icon6.Enabled = false;
      Icon6.Location = new Point(104, 326);
      Icon6.Name = "Icon6";
      Icon6.Size = new Size(34, 34);
      Icon6.TabIndex = 7;
      Icon6.TabStop = false;
      Icon6.Click += Icon_Click;
      // 
      // Icon5
      // 
      Icon5.BorderStyle = BorderStyle.FixedSingle;
      Icon5.Enabled = false;
      Icon5.Location = new Point(29, 326);
      Icon5.Name = "Icon5";
      Icon5.Size = new Size(34, 34);
      Icon5.TabIndex = 6;
      Icon5.TabStop = false;
      Icon5.Click += Icon_Click;
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Location = new Point(27, 290);
      label1.Name = "label1";
      label1.Size = new Size(37, 13);
      label1.TabIndex = 267;
      label1.Text = "Icon 1";
      label1.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label2
      // 
      label2.AutoSize = true;
      label2.Location = new Point(102, 290);
      label2.Name = "label2";
      label2.Size = new Size(37, 13);
      label2.TabIndex = 268;
      label2.Text = "Icon 2";
      label2.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label3
      // 
      label3.AutoSize = true;
      label3.Location = new Point(252, 290);
      label3.Name = "label3";
      label3.Size = new Size(37, 13);
      label3.TabIndex = 270;
      label3.Text = "Icon 4";
      label3.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label4
      // 
      label4.AutoSize = true;
      label4.Location = new Point(177, 290);
      label4.Name = "label4";
      label4.Size = new Size(37, 13);
      label4.TabIndex = 269;
      label4.Text = "Icon 3";
      label4.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label5
      // 
      label5.AutoSize = true;
      label5.Location = new Point(327, 290);
      label5.Name = "label5";
      label5.Size = new Size(37, 13);
      label5.TabIndex = 271;
      label5.Text = "Icon 5";
      label5.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label6
      // 
      label6.AutoSize = true;
      label6.Location = new Point(324, 361);
      label6.Name = "label6";
      label6.Size = new Size(43, 13);
      label6.TabIndex = 276;
      label6.Text = "Icon 10";
      label6.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label7
      // 
      label7.AutoSize = true;
      label7.Location = new Point(252, 361);
      label7.Name = "label7";
      label7.Size = new Size(37, 13);
      label7.TabIndex = 275;
      label7.Text = "Icon 9";
      label7.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label8
      // 
      label8.AutoSize = true;
      label8.Location = new Point(177, 361);
      label8.Name = "label8";
      label8.Size = new Size(37, 13);
      label8.TabIndex = 274;
      label8.Text = "Icon 8";
      label8.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label9
      // 
      label9.AutoSize = true;
      label9.Location = new Point(102, 361);
      label9.Name = "label9";
      label9.Size = new Size(37, 13);
      label9.TabIndex = 273;
      label9.Text = "Icon 7";
      label9.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label10
      // 
      label10.AutoSize = true;
      label10.Location = new Point(27, 361);
      label10.Name = "label10";
      label10.Size = new Size(37, 13);
      label10.TabIndex = 272;
      label10.Text = "Icon 6";
      label10.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label11
      // 
      label11.Anchor = (((((AnchorStyles.Top | AnchorStyles.Bottom)
                           | AnchorStyles.Left)
                          | AnchorStyles.Right)));
      label11.AutoSize = true;
      label11.Location = new Point(319, 303);
      label11.Name = "label11";
      label11.Size = new Size(52, 13);
      label11.TabIndex = 281;
      label11.Text = "( VIDEO )";
      label11.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label12
      // 
      label12.AutoSize = true;
      label12.Location = new Point(244, 303);
      label12.Name = "label12";
      label12.Size = new Size(53, 13);
      label12.TabIndex = 280;
      label12.Text = "( MUSIC )";
      label12.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label13
      // 
      label13.AutoSize = true;
      label13.Location = new Point(169, 303);
      label13.Name = "label13";
      label13.Size = new Size(53, 13);
      label13.TabIndex = 279;
      label13.Text = "( MOVIE )";
      label13.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label14
      // 
      label14.AutoSize = true;
      label14.Location = new Point(104, 303);
      label14.Name = "label14";
      label14.Size = new Size(33, 13);
      label14.TabIndex = 278;
      label14.Text = "( TV )";
      label14.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label15
      // 
      label15.AutoSize = true;
      label15.Location = new Point(27, 303);
      label15.Name = "label15";
      label15.Size = new Size(36, 13);
      label15.TabIndex = 277;
      label15.Text = "( Idle )";
      label15.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label16
      // 
      label16.AutoSize = true;
      label16.Location = new Point(317, 374);
      label16.Name = "label16";
      label16.Size = new Size(56, 13);
      label16.TabIndex = 286;
      label16.Text = "( Unused )";
      label16.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label17
      // 
      label17.AutoSize = true;
      label17.Location = new Point(242, 374);
      label17.Name = "label17";
      label17.Size = new Size(56, 13);
      label17.TabIndex = 285;
      label17.Text = "( Unused )";
      label17.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label18
      // 
      label18.AutoSize = true;
      label18.Location = new Point(167, 374);
      label18.Name = "label18";
      label18.Size = new Size(56, 13);
      label18.TabIndex = 284;
      label18.Text = "( Unused )";
      label18.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label19
      // 
      label19.AutoSize = true;
      label19.Location = new Point(89, 374);
      label19.Name = "label19";
      label19.Size = new Size(63, 13);
      label19.TabIndex = 283;
      label19.Text = "( PAUSED )";
      label19.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // label20
      // 
      label20.AutoSize = true;
      label20.Location = new Point(3, 374);
      label20.Name = "label20";
      label20.Size = new Size(84, 13);
      label20.TabIndex = 282;
      label20.Text = "( RECORDING )";
      label20.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // panel1
      // 
      panel1.BorderStyle = BorderStyle.FixedSingle;
      panel1.Controls.Add(lblEditIndex);
      panel1.Controls.Add(cmdSaveEdit);
      panel1.Controls.Add(cmdCancelEdit);
      panel1.Controls.Add(cmdInvert);
      panel1.Controls.Add(cmdSetAll);
      panel1.Controls.Add(cmdClearAll);
      panel1.Controls.Add(lblCurrentIcon);
      panel1.Controls.Add(C15R1_B0);
      panel1.Controls.Add(C15R1_B1);
      panel1.Controls.Add(C15R1_B2);
      panel1.Controls.Add(C15R1_B3);
      panel1.Controls.Add(C15R1_B4);
      panel1.Controls.Add(C15R1_B5);
      panel1.Controls.Add(C15R1_B6);
      panel1.Controls.Add(C15R1_B7);
      panel1.Controls.Add(C15R0_B0);
      panel1.Controls.Add(C15R0_B1);
      panel1.Controls.Add(C15R0_B2);
      panel1.Controls.Add(C15R0_B3);
      panel1.Controls.Add(C15R0_B4);
      panel1.Controls.Add(C15R0_B5);
      panel1.Controls.Add(C15R0_B6);
      panel1.Controls.Add(C15R0_B7);
      panel1.Controls.Add(C14R1_B0);
      panel1.Controls.Add(C14R1_B1);
      panel1.Controls.Add(C14R1_B2);
      panel1.Controls.Add(C14R1_B3);
      panel1.Controls.Add(C14R1_B4);
      panel1.Controls.Add(C14R1_B5);
      panel1.Controls.Add(C14R1_B6);
      panel1.Controls.Add(C14R1_B7);
      panel1.Controls.Add(C14R0_B0);
      panel1.Controls.Add(C14R0_B1);
      panel1.Controls.Add(C14R0_B2);
      panel1.Controls.Add(C14R0_B3);
      panel1.Controls.Add(C14R0_B4);
      panel1.Controls.Add(C14R0_B5);
      panel1.Controls.Add(C14R0_B6);
      panel1.Controls.Add(C14R0_B7);
      panel1.Controls.Add(C13R1_B0);
      panel1.Controls.Add(C13R1_B1);
      panel1.Controls.Add(C13R1_B2);
      panel1.Controls.Add(C13R1_B3);
      panel1.Controls.Add(C13R1_B4);
      panel1.Controls.Add(C13R1_B5);
      panel1.Controls.Add(C13R1_B6);
      panel1.Controls.Add(C13R1_B7);
      panel1.Controls.Add(C13R0_B0);
      panel1.Controls.Add(C13R0_B1);
      panel1.Controls.Add(C13R0_B2);
      panel1.Controls.Add(C13R0_B3);
      panel1.Controls.Add(C13R0_B4);
      panel1.Controls.Add(C13R0_B5);
      panel1.Controls.Add(C13R0_B6);
      panel1.Controls.Add(C13R0_B7);
      panel1.Controls.Add(C12R1_B0);
      panel1.Controls.Add(C12R1_B1);
      panel1.Controls.Add(C12R1_B2);
      panel1.Controls.Add(C12R1_B3);
      panel1.Controls.Add(C12R1_B4);
      panel1.Controls.Add(C12R1_B5);
      panel1.Controls.Add(C12R1_B6);
      panel1.Controls.Add(C12R1_B7);
      panel1.Controls.Add(C12R0_B0);
      panel1.Controls.Add(C12R0_B1);
      panel1.Controls.Add(C12R0_B2);
      panel1.Controls.Add(C12R0_B3);
      panel1.Controls.Add(C12R0_B4);
      panel1.Controls.Add(C12R0_B5);
      panel1.Controls.Add(C12R0_B6);
      panel1.Controls.Add(C12R0_B7);
      panel1.Controls.Add(C11R1_B0);
      panel1.Controls.Add(C11R1_B1);
      panel1.Controls.Add(C11R1_B2);
      panel1.Controls.Add(C11R1_B3);
      panel1.Controls.Add(C11R1_B4);
      panel1.Controls.Add(C11R1_B5);
      panel1.Controls.Add(C11R1_B6);
      panel1.Controls.Add(C11R1_B7);
      panel1.Controls.Add(C11R0_B0);
      panel1.Controls.Add(C11R0_B1);
      panel1.Controls.Add(C11R0_B2);
      panel1.Controls.Add(C11R0_B3);
      panel1.Controls.Add(C11R0_B4);
      panel1.Controls.Add(C11R0_B5);
      panel1.Controls.Add(C11R0_B6);
      panel1.Controls.Add(C11R0_B7);
      panel1.Controls.Add(C10R1_B0);
      panel1.Controls.Add(C10R1_B1);
      panel1.Controls.Add(C10R1_B2);
      panel1.Controls.Add(C10R1_B3);
      panel1.Controls.Add(C10R1_B4);
      panel1.Controls.Add(C10R1_B5);
      panel1.Controls.Add(C10R1_B6);
      panel1.Controls.Add(C10R1_B7);
      panel1.Controls.Add(C10R0_B0);
      panel1.Controls.Add(C10R0_B1);
      panel1.Controls.Add(C10R0_B2);
      panel1.Controls.Add(C10R0_B3);
      panel1.Controls.Add(C10R0_B4);
      panel1.Controls.Add(C10R0_B5);
      panel1.Controls.Add(C10R0_B6);
      panel1.Controls.Add(C10R0_B7);
      panel1.Controls.Add(C9R1_B0);
      panel1.Controls.Add(C9R1_B1);
      panel1.Controls.Add(C9R1_B2);
      panel1.Controls.Add(C9R1_B3);
      panel1.Controls.Add(C9R1_B4);
      panel1.Controls.Add(C9R1_B5);
      panel1.Controls.Add(C9R1_B6);
      panel1.Controls.Add(C9R1_B7);
      panel1.Controls.Add(C9R0_B0);
      panel1.Controls.Add(C9R0_B1);
      panel1.Controls.Add(C9R0_B2);
      panel1.Controls.Add(C9R0_B3);
      panel1.Controls.Add(C9R0_B4);
      panel1.Controls.Add(C9R0_B5);
      panel1.Controls.Add(C9R0_B6);
      panel1.Controls.Add(C9R0_B7);
      panel1.Controls.Add(C8R1_B0);
      panel1.Controls.Add(C8R1_B1);
      panel1.Controls.Add(C8R1_B2);
      panel1.Controls.Add(C8R1_B3);
      panel1.Controls.Add(C8R1_B4);
      panel1.Controls.Add(C8R1_B5);
      panel1.Controls.Add(C8R1_B6);
      panel1.Controls.Add(C8R1_B7);
      panel1.Controls.Add(C8R0_B0);
      panel1.Controls.Add(C8R0_B1);
      panel1.Controls.Add(C8R0_B2);
      panel1.Controls.Add(C8R0_B3);
      panel1.Controls.Add(C8R0_B4);
      panel1.Controls.Add(C8R0_B5);
      panel1.Controls.Add(C8R0_B6);
      panel1.Controls.Add(C8R0_B7);
      panel1.Controls.Add(C7R1_B0);
      panel1.Controls.Add(C7R1_B1);
      panel1.Controls.Add(C7R1_B2);
      panel1.Controls.Add(C7R1_B3);
      panel1.Controls.Add(C7R1_B4);
      panel1.Controls.Add(C7R1_B5);
      panel1.Controls.Add(C7R1_B6);
      panel1.Controls.Add(C7R1_B7);
      panel1.Controls.Add(C7R0_B0);
      panel1.Controls.Add(C7R0_B1);
      panel1.Controls.Add(C7R0_B2);
      panel1.Controls.Add(C7R0_B3);
      panel1.Controls.Add(C7R0_B4);
      panel1.Controls.Add(C7R0_B5);
      panel1.Controls.Add(C7R0_B6);
      panel1.Controls.Add(C7R0_B7);
      panel1.Controls.Add(C6R1_B0);
      panel1.Controls.Add(C6R1_B1);
      panel1.Controls.Add(C6R1_B2);
      panel1.Controls.Add(C6R1_B3);
      panel1.Controls.Add(C6R1_B4);
      panel1.Controls.Add(C6R1_B5);
      panel1.Controls.Add(C6R1_B6);
      panel1.Controls.Add(C6R1_B7);
      panel1.Controls.Add(C6R0_B0);
      panel1.Controls.Add(C6R0_B1);
      panel1.Controls.Add(C6R0_B2);
      panel1.Controls.Add(C6R0_B3);
      panel1.Controls.Add(C6R0_B4);
      panel1.Controls.Add(C6R0_B5);
      panel1.Controls.Add(C6R0_B6);
      panel1.Controls.Add(C6R0_B7);
      panel1.Controls.Add(C5R1_B0);
      panel1.Controls.Add(C5R1_B1);
      panel1.Controls.Add(C5R1_B2);
      panel1.Controls.Add(C5R1_B3);
      panel1.Controls.Add(C5R1_B4);
      panel1.Controls.Add(C5R1_B5);
      panel1.Controls.Add(C5R1_B6);
      panel1.Controls.Add(C5R1_B7);
      panel1.Controls.Add(C5R0_B0);
      panel1.Controls.Add(C5R0_B1);
      panel1.Controls.Add(C5R0_B2);
      panel1.Controls.Add(C5R0_B3);
      panel1.Controls.Add(C5R0_B4);
      panel1.Controls.Add(C5R0_B5);
      panel1.Controls.Add(C5R0_B6);
      panel1.Controls.Add(C5R0_B7);
      panel1.Controls.Add(C4R1_B0);
      panel1.Controls.Add(C4R1_B1);
      panel1.Controls.Add(C4R1_B2);
      panel1.Controls.Add(C4R1_B3);
      panel1.Controls.Add(C4R1_B4);
      panel1.Controls.Add(C4R1_B5);
      panel1.Controls.Add(C4R1_B6);
      panel1.Controls.Add(C4R1_B7);
      panel1.Controls.Add(C4R0_B0);
      panel1.Controls.Add(C4R0_B1);
      panel1.Controls.Add(C4R0_B2);
      panel1.Controls.Add(C4R0_B3);
      panel1.Controls.Add(C4R0_B4);
      panel1.Controls.Add(C4R0_B5);
      panel1.Controls.Add(C4R0_B6);
      panel1.Controls.Add(C4R0_B7);
      panel1.Controls.Add(C3R1_B0);
      panel1.Controls.Add(C3R1_B1);
      panel1.Controls.Add(C3R1_B2);
      panel1.Controls.Add(C3R1_B3);
      panel1.Controls.Add(C3R1_B4);
      panel1.Controls.Add(C3R1_B5);
      panel1.Controls.Add(C3R1_B6);
      panel1.Controls.Add(C3R1_B7);
      panel1.Controls.Add(C3R0_B0);
      panel1.Controls.Add(C3R0_B1);
      panel1.Controls.Add(C3R0_B2);
      panel1.Controls.Add(C3R0_B3);
      panel1.Controls.Add(C3R0_B4);
      panel1.Controls.Add(C3R0_B5);
      panel1.Controls.Add(C3R0_B6);
      panel1.Controls.Add(C3R0_B7);
      panel1.Controls.Add(C2R1_B0);
      panel1.Controls.Add(C2R1_B1);
      panel1.Controls.Add(C2R1_B2);
      panel1.Controls.Add(C2R1_B3);
      panel1.Controls.Add(C2R1_B4);
      panel1.Controls.Add(C2R1_B5);
      panel1.Controls.Add(C2R1_B6);
      panel1.Controls.Add(C2R1_B7);
      panel1.Controls.Add(C2R0_B0);
      panel1.Controls.Add(C2R0_B1);
      panel1.Controls.Add(C2R0_B2);
      panel1.Controls.Add(C2R0_B3);
      panel1.Controls.Add(C2R0_B4);
      panel1.Controls.Add(C2R0_B5);
      panel1.Controls.Add(C2R0_B6);
      panel1.Controls.Add(C2R0_B7);
      panel1.Controls.Add(C1R1_B0);
      panel1.Controls.Add(C1R1_B1);
      panel1.Controls.Add(C1R1_B2);
      panel1.Controls.Add(C1R1_B3);
      panel1.Controls.Add(C1R1_B4);
      panel1.Controls.Add(C1R1_B5);
      panel1.Controls.Add(C1R1_B6);
      panel1.Controls.Add(C1R1_B7);
      panel1.Controls.Add(C1R0_B0);
      panel1.Controls.Add(C1R0_B1);
      panel1.Controls.Add(C1R0_B2);
      panel1.Controls.Add(C1R0_B3);
      panel1.Controls.Add(C1R0_B4);
      panel1.Controls.Add(C1R0_B5);
      panel1.Controls.Add(C1R0_B6);
      panel1.Controls.Add(C1R0_B7);
      panel1.Controls.Add(C0R1_B0);
      panel1.Controls.Add(C0R1_B1);
      panel1.Controls.Add(C0R1_B2);
      panel1.Controls.Add(C0R1_B3);
      panel1.Controls.Add(C0R1_B4);
      panel1.Controls.Add(C0R1_B5);
      panel1.Controls.Add(C0R1_B6);
      panel1.Controls.Add(C0R1_B7);
      panel1.Controls.Add(C0R0_B0);
      panel1.Controls.Add(C0R0_B1);
      panel1.Controls.Add(C0R0_B2);
      panel1.Controls.Add(C0R0_B3);
      panel1.Controls.Add(C0R0_B4);
      panel1.Controls.Add(C0R0_B5);
      panel1.Controls.Add(C0R0_B6);
      panel1.Controls.Add(C0R0_B7);
      panel1.Enabled = false;
      panel1.Location = new Point(6, 7);
      panel1.Name = "panel1";
      panel1.Size = new Size(305, 242);
      panel1.TabIndex = 288;
      // 
      // lblEditIndex
      // 
      lblEditIndex.AutoSize = true;
      lblEditIndex.Location = new Point(221, 115);
      lblEditIndex.Name = "lblEditIndex";
      lblEditIndex.Size = new Size(61, 13);
      lblEditIndex.TabIndex = 550;
      lblEditIndex.Text = "lblEditIndex";
      lblEditIndex.Visible = false;
      // 
      // cmdSaveEdit
      // 
      cmdSaveEdit.Enabled = false;
      cmdSaveEdit.Location = new Point(214, 181);
      cmdSaveEdit.Name = "cmdSaveEdit";
      cmdSaveEdit.Size = new Size(75, 23);
      cmdSaveEdit.TabIndex = 549;
      cmdSaveEdit.Text = "Save";
      cmdSaveEdit.UseVisualStyleBackColor = true;
      cmdSaveEdit.Click += cmdSaveEdit_Click;
      // 
      // cmdCancelEdit
      // 
      cmdCancelEdit.Enabled = false;
      cmdCancelEdit.Location = new Point(214, 209);
      cmdCancelEdit.Name = "cmdCancelEdit";
      cmdCancelEdit.Size = new Size(75, 23);
      cmdCancelEdit.TabIndex = 548;
      cmdCancelEdit.Text = "Cancel";
      cmdCancelEdit.UseVisualStyleBackColor = true;
      cmdCancelEdit.Click += cmdCancelEdit_Click;
      // 
      // cmdInvert
      // 
      cmdInvert.Enabled = false;
      cmdInvert.Location = new Point(214, 61);
      cmdInvert.Name = "cmdInvert";
      cmdInvert.Size = new Size(75, 23);
      cmdInvert.TabIndex = 547;
      cmdInvert.Text = "Invert";
      cmdInvert.UseVisualStyleBackColor = true;
      cmdInvert.Click += cmdInvert_Click;
      // 
      // cmdSetAll
      // 
      cmdSetAll.Enabled = false;
      cmdSetAll.Location = new Point(214, 33);
      cmdSetAll.Name = "cmdSetAll";
      cmdSetAll.Size = new Size(75, 23);
      cmdSetAll.TabIndex = 546;
      cmdSetAll.Text = "Set All";
      cmdSetAll.UseVisualStyleBackColor = true;
      cmdSetAll.Click += cmdSetAll_Click;
      // 
      // cmdClearAll
      // 
      cmdClearAll.Enabled = false;
      cmdClearAll.Location = new Point(214, 6);
      cmdClearAll.Name = "cmdClearAll";
      cmdClearAll.Size = new Size(75, 23);
      cmdClearAll.TabIndex = 545;
      cmdClearAll.Text = "Clear All";
      cmdClearAll.UseVisualStyleBackColor = true;
      cmdClearAll.Click += cmdClearAll_Click;
      // 
      // lblCurrentIcon
      // 
      lblCurrentIcon.BorderStyle = BorderStyle.FixedSingle;
      lblCurrentIcon.Font = new Font("Microsoft Sans Serif", 15.75F, FontStyle.Bold, GraphicsUnit.Point,
                                     ((0)));
      lblCurrentIcon.Location = new Point(3, 201);
      lblCurrentIcon.Name = "lblCurrentIcon";
      lblCurrentIcon.Size = new Size(197, 33);
      lblCurrentIcon.TabIndex = 544;
      lblCurrentIcon.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // C15R1_B0
      // 
      C15R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C15R1_B0.Location = new Point(186, 186);
      C15R1_B0.Name = "C15R1_B0";
      C15R1_B0.Size = new Size(14, 14);
      C15R1_B0.TabIndex = 543;
      C15R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C15R1_B0.ThreeState = true;
      C15R1_B0.UseVisualStyleBackColor = true;
      C15R1_B0.Click += Pixel_Click;
      // 
      // C15R1_B1
      // 
      C15R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C15R1_B1.Location = new Point(186, 174);
      C15R1_B1.Name = "C15R1_B1";
      C15R1_B1.Size = new Size(14, 14);
      C15R1_B1.TabIndex = 542;
      C15R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C15R1_B1.ThreeState = true;
      C15R1_B1.UseVisualStyleBackColor = true;
      C15R1_B1.Click += Pixel_Click;
      // 
      // C15R1_B2
      // 
      C15R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C15R1_B2.Location = new Point(186, 162);
      C15R1_B2.Name = "C15R1_B2";
      C15R1_B2.Size = new Size(14, 14);
      C15R1_B2.TabIndex = 541;
      C15R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C15R1_B2.ThreeState = true;
      C15R1_B2.UseVisualStyleBackColor = true;
      C15R1_B2.Click += Pixel_Click;
      // 
      // C15R1_B3
      // 
      C15R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C15R1_B3.Location = new Point(186, 150);
      C15R1_B3.Name = "C15R1_B3";
      C15R1_B3.Size = new Size(14, 14);
      C15R1_B3.TabIndex = 540;
      C15R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C15R1_B3.ThreeState = true;
      C15R1_B3.UseVisualStyleBackColor = true;
      C15R1_B3.Click += Pixel_Click;
      // 
      // C15R1_B4
      // 
      C15R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C15R1_B4.Location = new Point(186, 138);
      C15R1_B4.Name = "C15R1_B4";
      C15R1_B4.Size = new Size(14, 14);
      C15R1_B4.TabIndex = 539;
      C15R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C15R1_B4.ThreeState = true;
      C15R1_B4.UseVisualStyleBackColor = true;
      C15R1_B4.Click += Pixel_Click;
      // 
      // C15R1_B5
      // 
      C15R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C15R1_B5.Location = new Point(186, 126);
      C15R1_B5.Name = "C15R1_B5";
      C15R1_B5.Size = new Size(14, 14);
      C15R1_B5.TabIndex = 538;
      C15R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C15R1_B5.ThreeState = true;
      C15R1_B5.UseVisualStyleBackColor = true;
      C15R1_B5.Click += Pixel_Click;
      // 
      // C15R1_B6
      // 
      C15R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C15R1_B6.Location = new Point(186, 114);
      C15R1_B6.Name = "C15R1_B6";
      C15R1_B6.Size = new Size(14, 14);
      C15R1_B6.TabIndex = 537;
      C15R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C15R1_B6.ThreeState = true;
      C15R1_B6.UseVisualStyleBackColor = true;
      C15R1_B6.Click += Pixel_Click;
      // 
      // C15R1_B7
      // 
      C15R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C15R1_B7.Location = new Point(186, 102);
      C15R1_B7.Name = "C15R1_B7";
      C15R1_B7.Size = new Size(14, 14);
      C15R1_B7.TabIndex = 536;
      C15R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C15R1_B7.ThreeState = true;
      C15R1_B7.UseVisualStyleBackColor = true;
      C15R1_B7.Click += Pixel_Click;
      // 
      // C15R0_B0
      // 
      C15R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C15R0_B0.Location = new Point(186, 90);
      C15R0_B0.Name = "C15R0_B0";
      C15R0_B0.Size = new Size(14, 14);
      C15R0_B0.TabIndex = 535;
      C15R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C15R0_B0.ThreeState = true;
      C15R0_B0.UseVisualStyleBackColor = true;
      C15R0_B0.Click += Pixel_Click;
      // 
      // C15R0_B1
      // 
      C15R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C15R0_B1.Location = new Point(186, 78);
      C15R0_B1.Name = "C15R0_B1";
      C15R0_B1.Size = new Size(14, 14);
      C15R0_B1.TabIndex = 534;
      C15R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C15R0_B1.ThreeState = true;
      C15R0_B1.UseVisualStyleBackColor = true;
      C15R0_B1.Click += Pixel_Click;
      // 
      // C15R0_B2
      // 
      C15R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C15R0_B2.Location = new Point(186, 66);
      C15R0_B2.Name = "C15R0_B2";
      C15R0_B2.Size = new Size(14, 14);
      C15R0_B2.TabIndex = 533;
      C15R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C15R0_B2.ThreeState = true;
      C15R0_B2.UseVisualStyleBackColor = true;
      C15R0_B2.Click += Pixel_Click;
      // 
      // C15R0_B3
      // 
      C15R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C15R0_B3.Location = new Point(186, 54);
      C15R0_B3.Name = "C15R0_B3";
      C15R0_B3.Size = new Size(14, 14);
      C15R0_B3.TabIndex = 532;
      C15R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C15R0_B3.ThreeState = true;
      C15R0_B3.UseVisualStyleBackColor = true;
      C15R0_B3.Click += Pixel_Click;
      // 
      // C15R0_B4
      // 
      C15R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C15R0_B4.Location = new Point(186, 42);
      C15R0_B4.Name = "C15R0_B4";
      C15R0_B4.Size = new Size(14, 14);
      C15R0_B4.TabIndex = 531;
      C15R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C15R0_B4.ThreeState = true;
      C15R0_B4.UseVisualStyleBackColor = true;
      C15R0_B4.Click += Pixel_Click;
      // 
      // C15R0_B5
      // 
      C15R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C15R0_B5.Location = new Point(186, 30);
      C15R0_B5.Name = "C15R0_B5";
      C15R0_B5.Size = new Size(14, 14);
      C15R0_B5.TabIndex = 530;
      C15R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C15R0_B5.ThreeState = true;
      C15R0_B5.UseVisualStyleBackColor = true;
      C15R0_B5.Click += Pixel_Click;
      // 
      // C15R0_B6
      // 
      C15R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C15R0_B6.Location = new Point(186, 18);
      C15R0_B6.Name = "C15R0_B6";
      C15R0_B6.Size = new Size(14, 14);
      C15R0_B6.TabIndex = 529;
      C15R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C15R0_B6.ThreeState = true;
      C15R0_B6.UseVisualStyleBackColor = true;
      C15R0_B6.Click += Pixel_Click;
      // 
      // C15R0_B7
      // 
      C15R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C15R0_B7.Location = new Point(186, 6);
      C15R0_B7.Name = "C15R0_B7";
      C15R0_B7.Size = new Size(14, 14);
      C15R0_B7.TabIndex = 528;
      C15R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C15R0_B7.ThreeState = true;
      C15R0_B7.UseVisualStyleBackColor = true;
      C15R0_B7.Click += Pixel_Click;
      // 
      // C14R1_B0
      // 
      C14R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C14R1_B0.Location = new Point(174, 186);
      C14R1_B0.Name = "C14R1_B0";
      C14R1_B0.Size = new Size(14, 14);
      C14R1_B0.TabIndex = 527;
      C14R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C14R1_B0.ThreeState = true;
      C14R1_B0.UseVisualStyleBackColor = true;
      C14R1_B0.Click += Pixel_Click;
      // 
      // C14R1_B1
      // 
      C14R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C14R1_B1.Location = new Point(174, 174);
      C14R1_B1.Name = "C14R1_B1";
      C14R1_B1.Size = new Size(14, 14);
      C14R1_B1.TabIndex = 526;
      C14R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C14R1_B1.ThreeState = true;
      C14R1_B1.UseVisualStyleBackColor = true;
      C14R1_B1.Click += Pixel_Click;
      // 
      // C14R1_B2
      // 
      C14R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C14R1_B2.Location = new Point(174, 162);
      C14R1_B2.Name = "C14R1_B2";
      C14R1_B2.Size = new Size(14, 14);
      C14R1_B2.TabIndex = 525;
      C14R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C14R1_B2.ThreeState = true;
      C14R1_B2.UseVisualStyleBackColor = true;
      C14R1_B2.Click += Pixel_Click;
      // 
      // C14R1_B3
      // 
      C14R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C14R1_B3.Location = new Point(174, 150);
      C14R1_B3.Name = "C14R1_B3";
      C14R1_B3.Size = new Size(14, 14);
      C14R1_B3.TabIndex = 524;
      C14R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C14R1_B3.ThreeState = true;
      C14R1_B3.UseVisualStyleBackColor = true;
      C14R1_B3.Click += Pixel_Click;
      // 
      // C14R1_B4
      // 
      C14R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C14R1_B4.Location = new Point(174, 138);
      C14R1_B4.Name = "C14R1_B4";
      C14R1_B4.Size = new Size(14, 14);
      C14R1_B4.TabIndex = 523;
      C14R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C14R1_B4.ThreeState = true;
      C14R1_B4.UseVisualStyleBackColor = true;
      C14R1_B4.Click += Pixel_Click;
      // 
      // C14R1_B5
      // 
      C14R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C14R1_B5.Location = new Point(174, 126);
      C14R1_B5.Name = "C14R1_B5";
      C14R1_B5.Size = new Size(14, 14);
      C14R1_B5.TabIndex = 522;
      C14R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C14R1_B5.ThreeState = true;
      C14R1_B5.UseVisualStyleBackColor = true;
      C14R1_B5.Click += Pixel_Click;
      // 
      // C14R1_B6
      // 
      C14R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C14R1_B6.Location = new Point(174, 114);
      C14R1_B6.Name = "C14R1_B6";
      C14R1_B6.Size = new Size(14, 14);
      C14R1_B6.TabIndex = 521;
      C14R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C14R1_B6.ThreeState = true;
      C14R1_B6.UseVisualStyleBackColor = true;
      C14R1_B6.Click += Pixel_Click;
      // 
      // C14R1_B7
      // 
      C14R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C14R1_B7.Location = new Point(174, 102);
      C14R1_B7.Name = "C14R1_B7";
      C14R1_B7.Size = new Size(14, 14);
      C14R1_B7.TabIndex = 520;
      C14R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C14R1_B7.ThreeState = true;
      C14R1_B7.UseVisualStyleBackColor = true;
      C14R1_B7.Click += Pixel_Click;
      // 
      // C14R0_B0
      // 
      C14R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C14R0_B0.Location = new Point(174, 90);
      C14R0_B0.Name = "C14R0_B0";
      C14R0_B0.Size = new Size(14, 14);
      C14R0_B0.TabIndex = 519;
      C14R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C14R0_B0.ThreeState = true;
      C14R0_B0.UseVisualStyleBackColor = true;
      C14R0_B0.Click += Pixel_Click;
      // 
      // C14R0_B1
      // 
      C14R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C14R0_B1.Location = new Point(174, 78);
      C14R0_B1.Name = "C14R0_B1";
      C14R0_B1.Size = new Size(14, 14);
      C14R0_B1.TabIndex = 518;
      C14R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C14R0_B1.ThreeState = true;
      C14R0_B1.UseVisualStyleBackColor = true;
      C14R0_B1.Click += Pixel_Click;
      // 
      // C14R0_B2
      // 
      C14R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C14R0_B2.Location = new Point(174, 66);
      C14R0_B2.Name = "C14R0_B2";
      C14R0_B2.Size = new Size(14, 14);
      C14R0_B2.TabIndex = 517;
      C14R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C14R0_B2.ThreeState = true;
      C14R0_B2.UseVisualStyleBackColor = true;
      C14R0_B2.Click += Pixel_Click;
      // 
      // C14R0_B3
      // 
      C14R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C14R0_B3.Location = new Point(174, 54);
      C14R0_B3.Name = "C14R0_B3";
      C14R0_B3.Size = new Size(14, 14);
      C14R0_B3.TabIndex = 516;
      C14R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C14R0_B3.ThreeState = true;
      C14R0_B3.UseVisualStyleBackColor = true;
      C14R0_B3.Click += Pixel_Click;
      // 
      // C14R0_B4
      // 
      C14R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C14R0_B4.Location = new Point(174, 42);
      C14R0_B4.Name = "C14R0_B4";
      C14R0_B4.Size = new Size(14, 14);
      C14R0_B4.TabIndex = 515;
      C14R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C14R0_B4.ThreeState = true;
      C14R0_B4.UseVisualStyleBackColor = true;
      C14R0_B4.Click += Pixel_Click;
      // 
      // C14R0_B5
      // 
      C14R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C14R0_B5.Location = new Point(174, 30);
      C14R0_B5.Name = "C14R0_B5";
      C14R0_B5.Size = new Size(14, 14);
      C14R0_B5.TabIndex = 514;
      C14R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C14R0_B5.ThreeState = true;
      C14R0_B5.UseVisualStyleBackColor = true;
      C14R0_B5.Click += Pixel_Click;
      // 
      // C14R0_B6
      // 
      C14R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C14R0_B6.Location = new Point(174, 18);
      C14R0_B6.Name = "C14R0_B6";
      C14R0_B6.Size = new Size(14, 14);
      C14R0_B6.TabIndex = 513;
      C14R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C14R0_B6.ThreeState = true;
      C14R0_B6.UseVisualStyleBackColor = true;
      C14R0_B6.Click += Pixel_Click;
      // 
      // C14R0_B7
      // 
      C14R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C14R0_B7.Location = new Point(174, 6);
      C14R0_B7.Name = "C14R0_B7";
      C14R0_B7.Size = new Size(14, 14);
      C14R0_B7.TabIndex = 512;
      C14R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C14R0_B7.ThreeState = true;
      C14R0_B7.UseVisualStyleBackColor = true;
      C14R0_B7.Click += Pixel_Click;
      // 
      // C13R1_B0
      // 
      C13R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C13R1_B0.Location = new Point(162, 186);
      C13R1_B0.Name = "C13R1_B0";
      C13R1_B0.Size = new Size(14, 14);
      C13R1_B0.TabIndex = 511;
      C13R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C13R1_B0.ThreeState = true;
      C13R1_B0.UseVisualStyleBackColor = true;
      C13R1_B0.Click += Pixel_Click;
      // 
      // C13R1_B1
      // 
      C13R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C13R1_B1.Location = new Point(162, 174);
      C13R1_B1.Name = "C13R1_B1";
      C13R1_B1.Size = new Size(14, 14);
      C13R1_B1.TabIndex = 510;
      C13R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C13R1_B1.ThreeState = true;
      C13R1_B1.UseVisualStyleBackColor = true;
      C13R1_B1.Click += Pixel_Click;
      // 
      // C13R1_B2
      // 
      C13R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C13R1_B2.Location = new Point(162, 162);
      C13R1_B2.Name = "C13R1_B2";
      C13R1_B2.Size = new Size(14, 14);
      C13R1_B2.TabIndex = 509;
      C13R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C13R1_B2.ThreeState = true;
      C13R1_B2.UseVisualStyleBackColor = true;
      C13R1_B2.Click += Pixel_Click;
      // 
      // C13R1_B3
      // 
      C13R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C13R1_B3.Location = new Point(162, 150);
      C13R1_B3.Name = "C13R1_B3";
      C13R1_B3.Size = new Size(14, 14);
      C13R1_B3.TabIndex = 508;
      C13R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C13R1_B3.ThreeState = true;
      C13R1_B3.UseVisualStyleBackColor = true;
      C13R1_B3.Click += Pixel_Click;
      // 
      // C13R1_B4
      // 
      C13R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C13R1_B4.Location = new Point(162, 138);
      C13R1_B4.Name = "C13R1_B4";
      C13R1_B4.Size = new Size(14, 14);
      C13R1_B4.TabIndex = 507;
      C13R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C13R1_B4.ThreeState = true;
      C13R1_B4.UseVisualStyleBackColor = true;
      C13R1_B4.Click += Pixel_Click;
      // 
      // C13R1_B5
      // 
      C13R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C13R1_B5.Location = new Point(162, 126);
      C13R1_B5.Name = "C13R1_B5";
      C13R1_B5.Size = new Size(14, 14);
      C13R1_B5.TabIndex = 506;
      C13R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C13R1_B5.ThreeState = true;
      C13R1_B5.UseVisualStyleBackColor = true;
      C13R1_B5.Click += Pixel_Click;
      // 
      // C13R1_B6
      // 
      C13R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C13R1_B6.Location = new Point(162, 114);
      C13R1_B6.Name = "C13R1_B6";
      C13R1_B6.Size = new Size(14, 14);
      C13R1_B6.TabIndex = 505;
      C13R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C13R1_B6.ThreeState = true;
      C13R1_B6.UseVisualStyleBackColor = true;
      C13R1_B6.Click += Pixel_Click;
      // 
      // C13R1_B7
      // 
      C13R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C13R1_B7.Location = new Point(162, 102);
      C13R1_B7.Name = "C13R1_B7";
      C13R1_B7.Size = new Size(14, 14);
      C13R1_B7.TabIndex = 504;
      C13R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C13R1_B7.ThreeState = true;
      C13R1_B7.UseVisualStyleBackColor = true;
      C13R1_B7.Click += Pixel_Click;
      // 
      // C13R0_B0
      // 
      C13R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C13R0_B0.Location = new Point(162, 90);
      C13R0_B0.Name = "C13R0_B0";
      C13R0_B0.Size = new Size(14, 14);
      C13R0_B0.TabIndex = 503;
      C13R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C13R0_B0.ThreeState = true;
      C13R0_B0.UseVisualStyleBackColor = true;
      C13R0_B0.Click += Pixel_Click;
      // 
      // C13R0_B1
      // 
      C13R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C13R0_B1.Location = new Point(162, 78);
      C13R0_B1.Name = "C13R0_B1";
      C13R0_B1.Size = new Size(14, 14);
      C13R0_B1.TabIndex = 502;
      C13R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C13R0_B1.ThreeState = true;
      C13R0_B1.UseVisualStyleBackColor = true;
      C13R0_B1.Click += Pixel_Click;
      // 
      // C13R0_B2
      // 
      C13R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C13R0_B2.Location = new Point(162, 66);
      C13R0_B2.Name = "C13R0_B2";
      C13R0_B2.Size = new Size(14, 14);
      C13R0_B2.TabIndex = 501;
      C13R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C13R0_B2.ThreeState = true;
      C13R0_B2.UseVisualStyleBackColor = true;
      C13R0_B2.Click += Pixel_Click;
      // 
      // C13R0_B3
      // 
      C13R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C13R0_B3.Location = new Point(162, 54);
      C13R0_B3.Name = "C13R0_B3";
      C13R0_B3.Size = new Size(14, 14);
      C13R0_B3.TabIndex = 500;
      C13R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C13R0_B3.ThreeState = true;
      C13R0_B3.UseVisualStyleBackColor = true;
      C13R0_B3.Click += Pixel_Click;
      // 
      // C13R0_B4
      // 
      C13R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C13R0_B4.Location = new Point(162, 42);
      C13R0_B4.Name = "C13R0_B4";
      C13R0_B4.Size = new Size(14, 14);
      C13R0_B4.TabIndex = 499;
      C13R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C13R0_B4.ThreeState = true;
      C13R0_B4.UseVisualStyleBackColor = true;
      C13R0_B4.Click += Pixel_Click;
      // 
      // C13R0_B5
      // 
      C13R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C13R0_B5.Location = new Point(162, 30);
      C13R0_B5.Name = "C13R0_B5";
      C13R0_B5.Size = new Size(14, 14);
      C13R0_B5.TabIndex = 498;
      C13R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C13R0_B5.ThreeState = true;
      C13R0_B5.UseVisualStyleBackColor = true;
      C13R0_B5.Click += Pixel_Click;
      // 
      // C13R0_B6
      // 
      C13R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C13R0_B6.Location = new Point(162, 18);
      C13R0_B6.Name = "C13R0_B6";
      C13R0_B6.Size = new Size(14, 14);
      C13R0_B6.TabIndex = 497;
      C13R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C13R0_B6.ThreeState = true;
      C13R0_B6.UseVisualStyleBackColor = true;
      C13R0_B6.Click += Pixel_Click;
      // 
      // C13R0_B7
      // 
      C13R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C13R0_B7.Location = new Point(162, 6);
      C13R0_B7.Name = "C13R0_B7";
      C13R0_B7.Size = new Size(14, 14);
      C13R0_B7.TabIndex = 496;
      C13R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C13R0_B7.ThreeState = true;
      C13R0_B7.UseVisualStyleBackColor = true;
      C13R0_B7.Click += Pixel_Click;
      // 
      // C12R1_B0
      // 
      C12R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C12R1_B0.Location = new Point(150, 186);
      C12R1_B0.Name = "C12R1_B0";
      C12R1_B0.Size = new Size(14, 14);
      C12R1_B0.TabIndex = 495;
      C12R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C12R1_B0.ThreeState = true;
      C12R1_B0.UseVisualStyleBackColor = true;
      C12R1_B0.Click += Pixel_Click;
      // 
      // C12R1_B1
      // 
      C12R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C12R1_B1.Location = new Point(150, 174);
      C12R1_B1.Name = "C12R1_B1";
      C12R1_B1.Size = new Size(14, 14);
      C12R1_B1.TabIndex = 494;
      C12R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C12R1_B1.ThreeState = true;
      C12R1_B1.UseVisualStyleBackColor = true;
      C12R1_B1.Click += Pixel_Click;
      // 
      // C12R1_B2
      // 
      C12R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C12R1_B2.Location = new Point(150, 162);
      C12R1_B2.Name = "C12R1_B2";
      C12R1_B2.Size = new Size(14, 14);
      C12R1_B2.TabIndex = 493;
      C12R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C12R1_B2.ThreeState = true;
      C12R1_B2.UseVisualStyleBackColor = true;
      C12R1_B2.Click += Pixel_Click;
      // 
      // C12R1_B3
      // 
      C12R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C12R1_B3.Location = new Point(150, 150);
      C12R1_B3.Name = "C12R1_B3";
      C12R1_B3.Size = new Size(14, 14);
      C12R1_B3.TabIndex = 492;
      C12R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C12R1_B3.ThreeState = true;
      C12R1_B3.UseVisualStyleBackColor = true;
      C12R1_B3.Click += Pixel_Click;
      // 
      // C12R1_B4
      // 
      C12R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C12R1_B4.Location = new Point(150, 138);
      C12R1_B4.Name = "C12R1_B4";
      C12R1_B4.Size = new Size(14, 14);
      C12R1_B4.TabIndex = 491;
      C12R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C12R1_B4.ThreeState = true;
      C12R1_B4.UseVisualStyleBackColor = true;
      C12R1_B4.Click += Pixel_Click;
      // 
      // C12R1_B5
      // 
      C12R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C12R1_B5.Location = new Point(150, 126);
      C12R1_B5.Name = "C12R1_B5";
      C12R1_B5.Size = new Size(14, 14);
      C12R1_B5.TabIndex = 490;
      C12R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C12R1_B5.ThreeState = true;
      C12R1_B5.UseVisualStyleBackColor = true;
      C12R1_B5.Click += Pixel_Click;
      // 
      // C12R1_B6
      // 
      C12R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C12R1_B6.Location = new Point(150, 114);
      C12R1_B6.Name = "C12R1_B6";
      C12R1_B6.Size = new Size(14, 14);
      C12R1_B6.TabIndex = 489;
      C12R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C12R1_B6.ThreeState = true;
      C12R1_B6.UseVisualStyleBackColor = true;
      C12R1_B6.Click += Pixel_Click;
      // 
      // C12R1_B7
      // 
      C12R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C12R1_B7.Location = new Point(150, 102);
      C12R1_B7.Name = "C12R1_B7";
      C12R1_B7.Size = new Size(14, 14);
      C12R1_B7.TabIndex = 488;
      C12R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C12R1_B7.ThreeState = true;
      C12R1_B7.UseVisualStyleBackColor = true;
      C12R1_B7.Click += Pixel_Click;
      // 
      // C12R0_B0
      // 
      C12R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C12R0_B0.Location = new Point(150, 90);
      C12R0_B0.Name = "C12R0_B0";
      C12R0_B0.Size = new Size(14, 14);
      C12R0_B0.TabIndex = 487;
      C12R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C12R0_B0.ThreeState = true;
      C12R0_B0.UseVisualStyleBackColor = true;
      C12R0_B0.Click += Pixel_Click;
      // 
      // C12R0_B1
      // 
      C12R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C12R0_B1.Location = new Point(150, 78);
      C12R0_B1.Name = "C12R0_B1";
      C12R0_B1.Size = new Size(14, 14);
      C12R0_B1.TabIndex = 486;
      C12R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C12R0_B1.ThreeState = true;
      C12R0_B1.UseVisualStyleBackColor = true;
      C12R0_B1.Click += Pixel_Click;
      // 
      // C12R0_B2
      // 
      C12R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C12R0_B2.Location = new Point(150, 66);
      C12R0_B2.Name = "C12R0_B2";
      C12R0_B2.Size = new Size(14, 14);
      C12R0_B2.TabIndex = 485;
      C12R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C12R0_B2.ThreeState = true;
      C12R0_B2.UseVisualStyleBackColor = true;
      C12R0_B2.Click += Pixel_Click;
      // 
      // C12R0_B3
      // 
      C12R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C12R0_B3.Location = new Point(150, 54);
      C12R0_B3.Name = "C12R0_B3";
      C12R0_B3.Size = new Size(14, 14);
      C12R0_B3.TabIndex = 484;
      C12R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C12R0_B3.ThreeState = true;
      C12R0_B3.UseVisualStyleBackColor = true;
      C12R0_B3.Click += Pixel_Click;
      // 
      // C12R0_B4
      // 
      C12R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C12R0_B4.Location = new Point(150, 42);
      C12R0_B4.Name = "C12R0_B4";
      C12R0_B4.Size = new Size(14, 14);
      C12R0_B4.TabIndex = 483;
      C12R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C12R0_B4.ThreeState = true;
      C12R0_B4.UseVisualStyleBackColor = true;
      C12R0_B4.Click += Pixel_Click;
      // 
      // C12R0_B5
      // 
      C12R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C12R0_B5.Location = new Point(150, 30);
      C12R0_B5.Name = "C12R0_B5";
      C12R0_B5.Size = new Size(14, 14);
      C12R0_B5.TabIndex = 482;
      C12R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C12R0_B5.ThreeState = true;
      C12R0_B5.UseVisualStyleBackColor = true;
      C12R0_B5.Click += Pixel_Click;
      // 
      // C12R0_B6
      // 
      C12R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C12R0_B6.Location = new Point(150, 18);
      C12R0_B6.Name = "C12R0_B6";
      C12R0_B6.Size = new Size(14, 14);
      C12R0_B6.TabIndex = 481;
      C12R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C12R0_B6.ThreeState = true;
      C12R0_B6.UseVisualStyleBackColor = true;
      C12R0_B6.Click += Pixel_Click;
      // 
      // C12R0_B7
      // 
      C12R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C12R0_B7.Location = new Point(150, 6);
      C12R0_B7.Name = "C12R0_B7";
      C12R0_B7.Size = new Size(14, 14);
      C12R0_B7.TabIndex = 480;
      C12R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C12R0_B7.ThreeState = true;
      C12R0_B7.UseVisualStyleBackColor = true;
      C12R0_B7.Click += Pixel_Click;
      // 
      // C11R1_B0
      // 
      C11R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C11R1_B0.Location = new Point(138, 186);
      C11R1_B0.Name = "C11R1_B0";
      C11R1_B0.Size = new Size(14, 14);
      C11R1_B0.TabIndex = 479;
      C11R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C11R1_B0.ThreeState = true;
      C11R1_B0.UseVisualStyleBackColor = true;
      C11R1_B0.Click += Pixel_Click;
      // 
      // C11R1_B1
      // 
      C11R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C11R1_B1.Location = new Point(138, 174);
      C11R1_B1.Name = "C11R1_B1";
      C11R1_B1.Size = new Size(14, 14);
      C11R1_B1.TabIndex = 478;
      C11R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C11R1_B1.ThreeState = true;
      C11R1_B1.UseVisualStyleBackColor = true;
      C11R1_B1.Click += Pixel_Click;
      // 
      // C11R1_B2
      // 
      C11R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C11R1_B2.Location = new Point(138, 162);
      C11R1_B2.Name = "C11R1_B2";
      C11R1_B2.Size = new Size(14, 14);
      C11R1_B2.TabIndex = 477;
      C11R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C11R1_B2.ThreeState = true;
      C11R1_B2.UseVisualStyleBackColor = true;
      C11R1_B2.Click += Pixel_Click;
      // 
      // C11R1_B3
      // 
      C11R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C11R1_B3.Location = new Point(138, 150);
      C11R1_B3.Name = "C11R1_B3";
      C11R1_B3.Size = new Size(14, 14);
      C11R1_B3.TabIndex = 476;
      C11R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C11R1_B3.ThreeState = true;
      C11R1_B3.UseVisualStyleBackColor = true;
      C11R1_B3.Click += Pixel_Click;
      // 
      // C11R1_B4
      // 
      C11R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C11R1_B4.Location = new Point(138, 138);
      C11R1_B4.Name = "C11R1_B4";
      C11R1_B4.Size = new Size(14, 14);
      C11R1_B4.TabIndex = 475;
      C11R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C11R1_B4.ThreeState = true;
      C11R1_B4.UseVisualStyleBackColor = true;
      C11R1_B4.Click += Pixel_Click;
      // 
      // C11R1_B5
      // 
      C11R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C11R1_B5.Location = new Point(138, 126);
      C11R1_B5.Name = "C11R1_B5";
      C11R1_B5.Size = new Size(14, 14);
      C11R1_B5.TabIndex = 474;
      C11R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C11R1_B5.ThreeState = true;
      C11R1_B5.UseVisualStyleBackColor = true;
      C11R1_B5.Click += Pixel_Click;
      // 
      // C11R1_B6
      // 
      C11R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C11R1_B6.Location = new Point(138, 114);
      C11R1_B6.Name = "C11R1_B6";
      C11R1_B6.Size = new Size(14, 14);
      C11R1_B6.TabIndex = 473;
      C11R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C11R1_B6.ThreeState = true;
      C11R1_B6.UseVisualStyleBackColor = true;
      C11R1_B6.Click += Pixel_Click;
      // 
      // C11R1_B7
      // 
      C11R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C11R1_B7.Location = new Point(138, 102);
      C11R1_B7.Name = "C11R1_B7";
      C11R1_B7.Size = new Size(14, 14);
      C11R1_B7.TabIndex = 472;
      C11R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C11R1_B7.ThreeState = true;
      C11R1_B7.UseVisualStyleBackColor = true;
      C11R1_B7.Click += Pixel_Click;
      // 
      // C11R0_B0
      // 
      C11R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C11R0_B0.Location = new Point(138, 90);
      C11R0_B0.Name = "C11R0_B0";
      C11R0_B0.Size = new Size(14, 14);
      C11R0_B0.TabIndex = 471;
      C11R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C11R0_B0.ThreeState = true;
      C11R0_B0.UseVisualStyleBackColor = true;
      C11R0_B0.Click += Pixel_Click;
      // 
      // C11R0_B1
      // 
      C11R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C11R0_B1.Location = new Point(138, 78);
      C11R0_B1.Name = "C11R0_B1";
      C11R0_B1.Size = new Size(14, 14);
      C11R0_B1.TabIndex = 470;
      C11R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C11R0_B1.ThreeState = true;
      C11R0_B1.UseVisualStyleBackColor = true;
      C11R0_B1.Click += Pixel_Click;
      // 
      // C11R0_B2
      // 
      C11R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C11R0_B2.Location = new Point(138, 66);
      C11R0_B2.Name = "C11R0_B2";
      C11R0_B2.Size = new Size(14, 14);
      C11R0_B2.TabIndex = 469;
      C11R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C11R0_B2.ThreeState = true;
      C11R0_B2.UseVisualStyleBackColor = true;
      C11R0_B2.Click += Pixel_Click;
      // 
      // C11R0_B3
      // 
      C11R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C11R0_B3.Location = new Point(138, 54);
      C11R0_B3.Name = "C11R0_B3";
      C11R0_B3.Size = new Size(14, 14);
      C11R0_B3.TabIndex = 468;
      C11R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C11R0_B3.ThreeState = true;
      C11R0_B3.UseVisualStyleBackColor = true;
      C11R0_B3.Click += Pixel_Click;
      // 
      // C11R0_B4
      // 
      C11R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C11R0_B4.Location = new Point(138, 42);
      C11R0_B4.Name = "C11R0_B4";
      C11R0_B4.Size = new Size(14, 14);
      C11R0_B4.TabIndex = 467;
      C11R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C11R0_B4.ThreeState = true;
      C11R0_B4.UseVisualStyleBackColor = true;
      C11R0_B4.Click += Pixel_Click;
      // 
      // C11R0_B5
      // 
      C11R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C11R0_B5.Location = new Point(138, 30);
      C11R0_B5.Name = "C11R0_B5";
      C11R0_B5.Size = new Size(14, 14);
      C11R0_B5.TabIndex = 466;
      C11R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C11R0_B5.ThreeState = true;
      C11R0_B5.UseVisualStyleBackColor = true;
      C11R0_B5.Click += Pixel_Click;
      // 
      // C11R0_B6
      // 
      C11R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C11R0_B6.Location = new Point(138, 18);
      C11R0_B6.Name = "C11R0_B6";
      C11R0_B6.Size = new Size(14, 14);
      C11R0_B6.TabIndex = 465;
      C11R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C11R0_B6.ThreeState = true;
      C11R0_B6.UseVisualStyleBackColor = true;
      C11R0_B6.Click += Pixel_Click;
      // 
      // C11R0_B7
      // 
      C11R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C11R0_B7.Location = new Point(138, 6);
      C11R0_B7.Name = "C11R0_B7";
      C11R0_B7.Size = new Size(14, 14);
      C11R0_B7.TabIndex = 464;
      C11R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C11R0_B7.ThreeState = true;
      C11R0_B7.UseVisualStyleBackColor = true;
      C11R0_B7.Click += Pixel_Click;
      // 
      // C10R1_B0
      // 
      C10R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C10R1_B0.Location = new Point(126, 186);
      C10R1_B0.Name = "C10R1_B0";
      C10R1_B0.Size = new Size(14, 14);
      C10R1_B0.TabIndex = 463;
      C10R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C10R1_B0.ThreeState = true;
      C10R1_B0.UseVisualStyleBackColor = true;
      C10R1_B0.Click += Pixel_Click;
      // 
      // C10R1_B1
      // 
      C10R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C10R1_B1.Location = new Point(126, 174);
      C10R1_B1.Name = "C10R1_B1";
      C10R1_B1.Size = new Size(14, 14);
      C10R1_B1.TabIndex = 462;
      C10R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C10R1_B1.ThreeState = true;
      C10R1_B1.UseVisualStyleBackColor = true;
      C10R1_B1.Click += Pixel_Click;
      // 
      // C10R1_B2
      // 
      C10R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C10R1_B2.Location = new Point(126, 162);
      C10R1_B2.Name = "C10R1_B2";
      C10R1_B2.Size = new Size(14, 14);
      C10R1_B2.TabIndex = 461;
      C10R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C10R1_B2.ThreeState = true;
      C10R1_B2.UseVisualStyleBackColor = true;
      C10R1_B2.Click += Pixel_Click;
      // 
      // C10R1_B3
      // 
      C10R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C10R1_B3.Location = new Point(126, 150);
      C10R1_B3.Name = "C10R1_B3";
      C10R1_B3.Size = new Size(14, 14);
      C10R1_B3.TabIndex = 460;
      C10R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C10R1_B3.ThreeState = true;
      C10R1_B3.UseVisualStyleBackColor = true;
      C10R1_B3.Click += Pixel_Click;
      // 
      // C10R1_B4
      // 
      C10R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C10R1_B4.Location = new Point(126, 138);
      C10R1_B4.Name = "C10R1_B4";
      C10R1_B4.Size = new Size(14, 14);
      C10R1_B4.TabIndex = 459;
      C10R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C10R1_B4.ThreeState = true;
      C10R1_B4.UseVisualStyleBackColor = true;
      C10R1_B4.Click += Pixel_Click;
      // 
      // C10R1_B5
      // 
      C10R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C10R1_B5.Location = new Point(126, 126);
      C10R1_B5.Name = "C10R1_B5";
      C10R1_B5.Size = new Size(14, 14);
      C10R1_B5.TabIndex = 458;
      C10R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C10R1_B5.ThreeState = true;
      C10R1_B5.UseVisualStyleBackColor = true;
      C10R1_B5.Click += Pixel_Click;
      // 
      // C10R1_B6
      // 
      C10R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C10R1_B6.Location = new Point(126, 114);
      C10R1_B6.Name = "C10R1_B6";
      C10R1_B6.Size = new Size(14, 14);
      C10R1_B6.TabIndex = 457;
      C10R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C10R1_B6.ThreeState = true;
      C10R1_B6.UseVisualStyleBackColor = true;
      C10R1_B6.Click += Pixel_Click;
      // 
      // C10R1_B7
      // 
      C10R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C10R1_B7.Location = new Point(126, 102);
      C10R1_B7.Name = "C10R1_B7";
      C10R1_B7.Size = new Size(14, 14);
      C10R1_B7.TabIndex = 456;
      C10R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C10R1_B7.ThreeState = true;
      C10R1_B7.UseVisualStyleBackColor = true;
      C10R1_B7.Click += Pixel_Click;
      // 
      // C10R0_B0
      // 
      C10R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C10R0_B0.Location = new Point(126, 90);
      C10R0_B0.Name = "C10R0_B0";
      C10R0_B0.Size = new Size(14, 14);
      C10R0_B0.TabIndex = 455;
      C10R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C10R0_B0.ThreeState = true;
      C10R0_B0.UseVisualStyleBackColor = true;
      C10R0_B0.Click += Pixel_Click;
      // 
      // C10R0_B1
      // 
      C10R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C10R0_B1.Location = new Point(126, 78);
      C10R0_B1.Name = "C10R0_B1";
      C10R0_B1.Size = new Size(14, 14);
      C10R0_B1.TabIndex = 454;
      C10R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C10R0_B1.ThreeState = true;
      C10R0_B1.UseVisualStyleBackColor = true;
      C10R0_B1.Click += Pixel_Click;
      // 
      // C10R0_B2
      // 
      C10R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C10R0_B2.Location = new Point(126, 66);
      C10R0_B2.Name = "C10R0_B2";
      C10R0_B2.Size = new Size(14, 14);
      C10R0_B2.TabIndex = 453;
      C10R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C10R0_B2.ThreeState = true;
      C10R0_B2.UseVisualStyleBackColor = true;
      C10R0_B2.Click += Pixel_Click;
      // 
      // C10R0_B3
      // 
      C10R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C10R0_B3.Location = new Point(126, 54);
      C10R0_B3.Name = "C10R0_B3";
      C10R0_B3.Size = new Size(14, 14);
      C10R0_B3.TabIndex = 452;
      C10R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C10R0_B3.ThreeState = true;
      C10R0_B3.UseVisualStyleBackColor = true;
      C10R0_B3.Click += Pixel_Click;
      // 
      // C10R0_B4
      // 
      C10R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C10R0_B4.Location = new Point(126, 42);
      C10R0_B4.Name = "C10R0_B4";
      C10R0_B4.Size = new Size(14, 14);
      C10R0_B4.TabIndex = 451;
      C10R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C10R0_B4.ThreeState = true;
      C10R0_B4.UseVisualStyleBackColor = true;
      C10R0_B4.Click += Pixel_Click;
      // 
      // C10R0_B5
      // 
      C10R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C10R0_B5.Location = new Point(126, 30);
      C10R0_B5.Name = "C10R0_B5";
      C10R0_B5.Size = new Size(14, 14);
      C10R0_B5.TabIndex = 450;
      C10R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C10R0_B5.ThreeState = true;
      C10R0_B5.UseVisualStyleBackColor = true;
      C10R0_B5.Click += Pixel_Click;
      // 
      // C10R0_B6
      // 
      C10R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C10R0_B6.Location = new Point(126, 18);
      C10R0_B6.Name = "C10R0_B6";
      C10R0_B6.Size = new Size(14, 14);
      C10R0_B6.TabIndex = 449;
      C10R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C10R0_B6.ThreeState = true;
      C10R0_B6.UseVisualStyleBackColor = true;
      C10R0_B6.Click += Pixel_Click;
      // 
      // C10R0_B7
      // 
      C10R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C10R0_B7.Location = new Point(126, 6);
      C10R0_B7.Name = "C10R0_B7";
      C10R0_B7.Size = new Size(14, 14);
      C10R0_B7.TabIndex = 448;
      C10R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C10R0_B7.ThreeState = true;
      C10R0_B7.UseVisualStyleBackColor = true;
      C10R0_B7.Click += Pixel_Click;
      // 
      // C9R1_B0
      // 
      C9R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C9R1_B0.Location = new Point(114, 186);
      C9R1_B0.Name = "C9R1_B0";
      C9R1_B0.Size = new Size(14, 14);
      C9R1_B0.TabIndex = 447;
      C9R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C9R1_B0.ThreeState = true;
      C9R1_B0.UseVisualStyleBackColor = true;
      C9R1_B0.Click += Pixel_Click;
      // 
      // C9R1_B1
      // 
      C9R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C9R1_B1.Location = new Point(114, 174);
      C9R1_B1.Name = "C9R1_B1";
      C9R1_B1.Size = new Size(14, 14);
      C9R1_B1.TabIndex = 446;
      C9R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C9R1_B1.ThreeState = true;
      C9R1_B1.UseVisualStyleBackColor = true;
      C9R1_B1.Click += Pixel_Click;
      // 
      // C9R1_B2
      // 
      C9R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C9R1_B2.Location = new Point(114, 162);
      C9R1_B2.Name = "C9R1_B2";
      C9R1_B2.Size = new Size(14, 14);
      C9R1_B2.TabIndex = 445;
      C9R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C9R1_B2.ThreeState = true;
      C9R1_B2.UseVisualStyleBackColor = true;
      C9R1_B2.Click += Pixel_Click;
      // 
      // C9R1_B3
      // 
      C9R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C9R1_B3.Location = new Point(114, 150);
      C9R1_B3.Name = "C9R1_B3";
      C9R1_B3.Size = new Size(14, 14);
      C9R1_B3.TabIndex = 444;
      C9R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C9R1_B3.ThreeState = true;
      C9R1_B3.UseVisualStyleBackColor = true;
      C9R1_B3.Click += Pixel_Click;
      // 
      // C9R1_B4
      // 
      C9R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C9R1_B4.Location = new Point(114, 138);
      C9R1_B4.Name = "C9R1_B4";
      C9R1_B4.Size = new Size(14, 14);
      C9R1_B4.TabIndex = 443;
      C9R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C9R1_B4.ThreeState = true;
      C9R1_B4.UseVisualStyleBackColor = true;
      C9R1_B4.Click += Pixel_Click;
      // 
      // C9R1_B5
      // 
      C9R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C9R1_B5.Location = new Point(114, 126);
      C9R1_B5.Name = "C9R1_B5";
      C9R1_B5.Size = new Size(14, 14);
      C9R1_B5.TabIndex = 442;
      C9R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C9R1_B5.ThreeState = true;
      C9R1_B5.UseVisualStyleBackColor = true;
      C9R1_B5.Click += Pixel_Click;
      // 
      // C9R1_B6
      // 
      C9R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C9R1_B6.Location = new Point(114, 114);
      C9R1_B6.Name = "C9R1_B6";
      C9R1_B6.Size = new Size(14, 14);
      C9R1_B6.TabIndex = 441;
      C9R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C9R1_B6.ThreeState = true;
      C9R1_B6.UseVisualStyleBackColor = true;
      C9R1_B6.Click += Pixel_Click;
      // 
      // C9R1_B7
      // 
      C9R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C9R1_B7.Location = new Point(114, 102);
      C9R1_B7.Name = "C9R1_B7";
      C9R1_B7.Size = new Size(14, 14);
      C9R1_B7.TabIndex = 440;
      C9R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C9R1_B7.ThreeState = true;
      C9R1_B7.UseVisualStyleBackColor = true;
      C9R1_B7.Click += Pixel_Click;
      // 
      // C9R0_B0
      // 
      C9R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C9R0_B0.Location = new Point(114, 90);
      C9R0_B0.Name = "C9R0_B0";
      C9R0_B0.Size = new Size(14, 14);
      C9R0_B0.TabIndex = 439;
      C9R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C9R0_B0.ThreeState = true;
      C9R0_B0.UseVisualStyleBackColor = true;
      C9R0_B0.Click += Pixel_Click;
      // 
      // C9R0_B1
      // 
      C9R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C9R0_B1.Location = new Point(114, 78);
      C9R0_B1.Name = "C9R0_B1";
      C9R0_B1.Size = new Size(14, 14);
      C9R0_B1.TabIndex = 438;
      C9R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C9R0_B1.ThreeState = true;
      C9R0_B1.UseVisualStyleBackColor = true;
      C9R0_B1.Click += Pixel_Click;
      // 
      // C9R0_B2
      // 
      C9R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C9R0_B2.Location = new Point(114, 66);
      C9R0_B2.Name = "C9R0_B2";
      C9R0_B2.Size = new Size(14, 14);
      C9R0_B2.TabIndex = 437;
      C9R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C9R0_B2.ThreeState = true;
      C9R0_B2.UseVisualStyleBackColor = true;
      C9R0_B2.Click += Pixel_Click;
      // 
      // C9R0_B3
      // 
      C9R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C9R0_B3.Location = new Point(114, 54);
      C9R0_B3.Name = "C9R0_B3";
      C9R0_B3.Size = new Size(14, 14);
      C9R0_B3.TabIndex = 436;
      C9R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C9R0_B3.ThreeState = true;
      C9R0_B3.UseVisualStyleBackColor = true;
      C9R0_B3.Click += Pixel_Click;
      // 
      // C9R0_B4
      // 
      C9R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C9R0_B4.Location = new Point(114, 42);
      C9R0_B4.Name = "C9R0_B4";
      C9R0_B4.Size = new Size(14, 14);
      C9R0_B4.TabIndex = 435;
      C9R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C9R0_B4.ThreeState = true;
      C9R0_B4.UseVisualStyleBackColor = true;
      C9R0_B4.Click += Pixel_Click;
      // 
      // C9R0_B5
      // 
      C9R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C9R0_B5.Location = new Point(114, 30);
      C9R0_B5.Name = "C9R0_B5";
      C9R0_B5.Size = new Size(14, 14);
      C9R0_B5.TabIndex = 434;
      C9R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C9R0_B5.ThreeState = true;
      C9R0_B5.UseVisualStyleBackColor = true;
      C9R0_B5.Click += Pixel_Click;
      // 
      // C9R0_B6
      // 
      C9R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C9R0_B6.Location = new Point(114, 18);
      C9R0_B6.Name = "C9R0_B6";
      C9R0_B6.Size = new Size(14, 14);
      C9R0_B6.TabIndex = 433;
      C9R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C9R0_B6.ThreeState = true;
      C9R0_B6.UseVisualStyleBackColor = true;
      C9R0_B6.Click += Pixel_Click;
      // 
      // C9R0_B7
      // 
      C9R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C9R0_B7.Location = new Point(114, 6);
      C9R0_B7.Name = "C9R0_B7";
      C9R0_B7.Size = new Size(14, 14);
      C9R0_B7.TabIndex = 432;
      C9R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C9R0_B7.ThreeState = true;
      C9R0_B7.UseVisualStyleBackColor = true;
      C9R0_B7.Click += Pixel_Click;
      // 
      // C8R1_B0
      // 
      C8R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C8R1_B0.Location = new Point(102, 186);
      C8R1_B0.Name = "C8R1_B0";
      C8R1_B0.Size = new Size(14, 14);
      C8R1_B0.TabIndex = 431;
      C8R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C8R1_B0.ThreeState = true;
      C8R1_B0.UseVisualStyleBackColor = true;
      C8R1_B0.Click += Pixel_Click;
      // 
      // C8R1_B1
      // 
      C8R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C8R1_B1.Location = new Point(102, 174);
      C8R1_B1.Name = "C8R1_B1";
      C8R1_B1.Size = new Size(14, 14);
      C8R1_B1.TabIndex = 430;
      C8R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C8R1_B1.ThreeState = true;
      C8R1_B1.UseVisualStyleBackColor = true;
      C8R1_B1.Click += Pixel_Click;
      // 
      // C8R1_B2
      // 
      C8R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C8R1_B2.Location = new Point(102, 162);
      C8R1_B2.Name = "C8R1_B2";
      C8R1_B2.Size = new Size(14, 14);
      C8R1_B2.TabIndex = 429;
      C8R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C8R1_B2.ThreeState = true;
      C8R1_B2.UseVisualStyleBackColor = true;
      C8R1_B2.Click += Pixel_Click;
      // 
      // C8R1_B3
      // 
      C8R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C8R1_B3.Location = new Point(102, 150);
      C8R1_B3.Name = "C8R1_B3";
      C8R1_B3.Size = new Size(14, 14);
      C8R1_B3.TabIndex = 428;
      C8R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C8R1_B3.ThreeState = true;
      C8R1_B3.UseVisualStyleBackColor = true;
      C8R1_B3.Click += Pixel_Click;
      // 
      // C8R1_B4
      // 
      C8R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C8R1_B4.Location = new Point(102, 138);
      C8R1_B4.Name = "C8R1_B4";
      C8R1_B4.Size = new Size(14, 14);
      C8R1_B4.TabIndex = 427;
      C8R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C8R1_B4.ThreeState = true;
      C8R1_B4.UseVisualStyleBackColor = true;
      C8R1_B4.Click += Pixel_Click;
      // 
      // C8R1_B5
      // 
      C8R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C8R1_B5.Location = new Point(102, 126);
      C8R1_B5.Name = "C8R1_B5";
      C8R1_B5.Size = new Size(14, 14);
      C8R1_B5.TabIndex = 426;
      C8R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C8R1_B5.ThreeState = true;
      C8R1_B5.UseVisualStyleBackColor = true;
      C8R1_B5.Click += Pixel_Click;
      // 
      // C8R1_B6
      // 
      C8R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C8R1_B6.Location = new Point(102, 114);
      C8R1_B6.Name = "C8R1_B6";
      C8R1_B6.Size = new Size(14, 14);
      C8R1_B6.TabIndex = 425;
      C8R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C8R1_B6.ThreeState = true;
      C8R1_B6.UseVisualStyleBackColor = true;
      C8R1_B6.Click += Pixel_Click;
      // 
      // C8R1_B7
      // 
      C8R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C8R1_B7.Location = new Point(102, 102);
      C8R1_B7.Name = "C8R1_B7";
      C8R1_B7.Size = new Size(14, 14);
      C8R1_B7.TabIndex = 424;
      C8R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C8R1_B7.ThreeState = true;
      C8R1_B7.UseVisualStyleBackColor = true;
      C8R1_B7.Click += Pixel_Click;
      // 
      // C8R0_B0
      // 
      C8R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C8R0_B0.Location = new Point(102, 90);
      C8R0_B0.Name = "C8R0_B0";
      C8R0_B0.Size = new Size(14, 14);
      C8R0_B0.TabIndex = 423;
      C8R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C8R0_B0.ThreeState = true;
      C8R0_B0.UseVisualStyleBackColor = true;
      C8R0_B0.Click += Pixel_Click;
      // 
      // C8R0_B1
      // 
      C8R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C8R0_B1.Location = new Point(102, 78);
      C8R0_B1.Name = "C8R0_B1";
      C8R0_B1.Size = new Size(14, 14);
      C8R0_B1.TabIndex = 422;
      C8R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C8R0_B1.ThreeState = true;
      C8R0_B1.UseVisualStyleBackColor = true;
      C8R0_B1.Click += Pixel_Click;
      // 
      // C8R0_B2
      // 
      C8R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C8R0_B2.Location = new Point(102, 66);
      C8R0_B2.Name = "C8R0_B2";
      C8R0_B2.Size = new Size(14, 14);
      C8R0_B2.TabIndex = 421;
      C8R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C8R0_B2.ThreeState = true;
      C8R0_B2.UseVisualStyleBackColor = true;
      C8R0_B2.Click += Pixel_Click;
      // 
      // C8R0_B3
      // 
      C8R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C8R0_B3.Location = new Point(102, 54);
      C8R0_B3.Name = "C8R0_B3";
      C8R0_B3.Size = new Size(14, 14);
      C8R0_B3.TabIndex = 420;
      C8R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C8R0_B3.ThreeState = true;
      C8R0_B3.UseVisualStyleBackColor = true;
      C8R0_B3.Click += Pixel_Click;
      // 
      // C8R0_B4
      // 
      C8R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C8R0_B4.Location = new Point(102, 42);
      C8R0_B4.Name = "C8R0_B4";
      C8R0_B4.Size = new Size(14, 14);
      C8R0_B4.TabIndex = 419;
      C8R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C8R0_B4.ThreeState = true;
      C8R0_B4.UseVisualStyleBackColor = true;
      C8R0_B4.Click += Pixel_Click;
      // 
      // C8R0_B5
      // 
      C8R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C8R0_B5.Location = new Point(102, 30);
      C8R0_B5.Name = "C8R0_B5";
      C8R0_B5.Size = new Size(14, 14);
      C8R0_B5.TabIndex = 418;
      C8R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C8R0_B5.ThreeState = true;
      C8R0_B5.UseVisualStyleBackColor = true;
      C8R0_B5.Click += Pixel_Click;
      // 
      // C8R0_B6
      // 
      C8R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C8R0_B6.Location = new Point(102, 18);
      C8R0_B6.Name = "C8R0_B6";
      C8R0_B6.Size = new Size(14, 14);
      C8R0_B6.TabIndex = 417;
      C8R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C8R0_B6.ThreeState = true;
      C8R0_B6.UseVisualStyleBackColor = true;
      C8R0_B6.Click += Pixel_Click;
      // 
      // C8R0_B7
      // 
      C8R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C8R0_B7.Location = new Point(102, 6);
      C8R0_B7.Name = "C8R0_B7";
      C8R0_B7.Size = new Size(14, 14);
      C8R0_B7.TabIndex = 416;
      C8R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C8R0_B7.ThreeState = true;
      C8R0_B7.UseVisualStyleBackColor = true;
      C8R0_B7.Click += Pixel_Click;
      // 
      // C7R1_B0
      // 
      C7R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C7R1_B0.Location = new Point(90, 186);
      C7R1_B0.Name = "C7R1_B0";
      C7R1_B0.Size = new Size(14, 14);
      C7R1_B0.TabIndex = 415;
      C7R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C7R1_B0.ThreeState = true;
      C7R1_B0.UseVisualStyleBackColor = true;
      C7R1_B0.Click += Pixel_Click;
      // 
      // C7R1_B1
      // 
      C7R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C7R1_B1.Location = new Point(90, 174);
      C7R1_B1.Name = "C7R1_B1";
      C7R1_B1.Size = new Size(14, 14);
      C7R1_B1.TabIndex = 414;
      C7R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C7R1_B1.ThreeState = true;
      C7R1_B1.UseVisualStyleBackColor = true;
      C7R1_B1.Click += Pixel_Click;
      // 
      // C7R1_B2
      // 
      C7R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C7R1_B2.Location = new Point(90, 162);
      C7R1_B2.Name = "C7R1_B2";
      C7R1_B2.Size = new Size(14, 14);
      C7R1_B2.TabIndex = 413;
      C7R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C7R1_B2.ThreeState = true;
      C7R1_B2.UseVisualStyleBackColor = true;
      C7R1_B2.Click += Pixel_Click;
      // 
      // C7R1_B3
      // 
      C7R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C7R1_B3.Location = new Point(90, 150);
      C7R1_B3.Name = "C7R1_B3";
      C7R1_B3.Size = new Size(14, 14);
      C7R1_B3.TabIndex = 412;
      C7R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C7R1_B3.ThreeState = true;
      C7R1_B3.UseVisualStyleBackColor = true;
      C7R1_B3.Click += Pixel_Click;
      // 
      // C7R1_B4
      // 
      C7R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C7R1_B4.Location = new Point(90, 138);
      C7R1_B4.Name = "C7R1_B4";
      C7R1_B4.Size = new Size(14, 14);
      C7R1_B4.TabIndex = 411;
      C7R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C7R1_B4.ThreeState = true;
      C7R1_B4.UseVisualStyleBackColor = true;
      C7R1_B4.Click += Pixel_Click;
      // 
      // C7R1_B5
      // 
      C7R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C7R1_B5.Location = new Point(90, 126);
      C7R1_B5.Name = "C7R1_B5";
      C7R1_B5.Size = new Size(14, 14);
      C7R1_B5.TabIndex = 410;
      C7R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C7R1_B5.ThreeState = true;
      C7R1_B5.UseVisualStyleBackColor = true;
      C7R1_B5.Click += Pixel_Click;
      // 
      // C7R1_B6
      // 
      C7R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C7R1_B6.Location = new Point(90, 114);
      C7R1_B6.Name = "C7R1_B6";
      C7R1_B6.Size = new Size(14, 14);
      C7R1_B6.TabIndex = 409;
      C7R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C7R1_B6.ThreeState = true;
      C7R1_B6.UseVisualStyleBackColor = true;
      C7R1_B6.Click += Pixel_Click;
      // 
      // C7R1_B7
      // 
      C7R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C7R1_B7.Location = new Point(90, 102);
      C7R1_B7.Name = "C7R1_B7";
      C7R1_B7.Size = new Size(14, 14);
      C7R1_B7.TabIndex = 408;
      C7R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C7R1_B7.ThreeState = true;
      C7R1_B7.UseVisualStyleBackColor = true;
      C7R1_B7.Click += Pixel_Click;
      // 
      // C7R0_B0
      // 
      C7R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C7R0_B0.Location = new Point(90, 90);
      C7R0_B0.Name = "C7R0_B0";
      C7R0_B0.Size = new Size(14, 14);
      C7R0_B0.TabIndex = 407;
      C7R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C7R0_B0.ThreeState = true;
      C7R0_B0.UseVisualStyleBackColor = true;
      C7R0_B0.Click += Pixel_Click;
      // 
      // C7R0_B1
      // 
      C7R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C7R0_B1.Location = new Point(90, 78);
      C7R0_B1.Name = "C7R0_B1";
      C7R0_B1.Size = new Size(14, 14);
      C7R0_B1.TabIndex = 406;
      C7R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C7R0_B1.ThreeState = true;
      C7R0_B1.UseVisualStyleBackColor = true;
      C7R0_B1.Click += Pixel_Click;
      // 
      // C7R0_B2
      // 
      C7R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C7R0_B2.Location = new Point(90, 66);
      C7R0_B2.Name = "C7R0_B2";
      C7R0_B2.Size = new Size(14, 14);
      C7R0_B2.TabIndex = 405;
      C7R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C7R0_B2.ThreeState = true;
      C7R0_B2.UseVisualStyleBackColor = true;
      C7R0_B2.Click += Pixel_Click;
      // 
      // C7R0_B3
      // 
      C7R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C7R0_B3.Location = new Point(90, 54);
      C7R0_B3.Name = "C7R0_B3";
      C7R0_B3.Size = new Size(14, 14);
      C7R0_B3.TabIndex = 404;
      C7R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C7R0_B3.ThreeState = true;
      C7R0_B3.UseVisualStyleBackColor = true;
      C7R0_B3.Click += Pixel_Click;
      // 
      // C7R0_B4
      // 
      C7R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C7R0_B4.Location = new Point(90, 42);
      C7R0_B4.Name = "C7R0_B4";
      C7R0_B4.Size = new Size(14, 14);
      C7R0_B4.TabIndex = 403;
      C7R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C7R0_B4.ThreeState = true;
      C7R0_B4.UseVisualStyleBackColor = true;
      C7R0_B4.Click += Pixel_Click;
      // 
      // C7R0_B5
      // 
      C7R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C7R0_B5.Location = new Point(90, 30);
      C7R0_B5.Name = "C7R0_B5";
      C7R0_B5.Size = new Size(14, 14);
      C7R0_B5.TabIndex = 402;
      C7R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C7R0_B5.ThreeState = true;
      C7R0_B5.UseVisualStyleBackColor = true;
      C7R0_B5.Click += Pixel_Click;
      // 
      // C7R0_B6
      // 
      C7R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C7R0_B6.Location = new Point(90, 18);
      C7R0_B6.Name = "C7R0_B6";
      C7R0_B6.Size = new Size(14, 14);
      C7R0_B6.TabIndex = 401;
      C7R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C7R0_B6.ThreeState = true;
      C7R0_B6.UseVisualStyleBackColor = true;
      C7R0_B6.Click += Pixel_Click;
      // 
      // C7R0_B7
      // 
      C7R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C7R0_B7.Location = new Point(90, 6);
      C7R0_B7.Name = "C7R0_B7";
      C7R0_B7.Size = new Size(14, 14);
      C7R0_B7.TabIndex = 400;
      C7R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C7R0_B7.ThreeState = true;
      C7R0_B7.UseVisualStyleBackColor = true;
      C7R0_B7.Click += Pixel_Click;
      // 
      // C6R1_B0
      // 
      C6R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C6R1_B0.Location = new Point(78, 186);
      C6R1_B0.Name = "C6R1_B0";
      C6R1_B0.Size = new Size(14, 14);
      C6R1_B0.TabIndex = 399;
      C6R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C6R1_B0.ThreeState = true;
      C6R1_B0.UseVisualStyleBackColor = true;
      C6R1_B0.Click += Pixel_Click;
      // 
      // C6R1_B1
      // 
      C6R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C6R1_B1.Location = new Point(78, 174);
      C6R1_B1.Name = "C6R1_B1";
      C6R1_B1.Size = new Size(14, 14);
      C6R1_B1.TabIndex = 398;
      C6R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C6R1_B1.ThreeState = true;
      C6R1_B1.UseVisualStyleBackColor = true;
      C6R1_B1.Click += Pixel_Click;
      // 
      // C6R1_B2
      // 
      C6R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C6R1_B2.Location = new Point(78, 162);
      C6R1_B2.Name = "C6R1_B2";
      C6R1_B2.Size = new Size(14, 14);
      C6R1_B2.TabIndex = 397;
      C6R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C6R1_B2.ThreeState = true;
      C6R1_B2.UseVisualStyleBackColor = true;
      C6R1_B2.Click += Pixel_Click;
      // 
      // C6R1_B3
      // 
      C6R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C6R1_B3.Location = new Point(78, 150);
      C6R1_B3.Name = "C6R1_B3";
      C6R1_B3.Size = new Size(14, 14);
      C6R1_B3.TabIndex = 396;
      C6R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C6R1_B3.ThreeState = true;
      C6R1_B3.UseVisualStyleBackColor = true;
      C6R1_B3.Click += Pixel_Click;
      // 
      // C6R1_B4
      // 
      C6R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C6R1_B4.Location = new Point(78, 138);
      C6R1_B4.Name = "C6R1_B4";
      C6R1_B4.Size = new Size(14, 14);
      C6R1_B4.TabIndex = 395;
      C6R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C6R1_B4.ThreeState = true;
      C6R1_B4.UseVisualStyleBackColor = true;
      C6R1_B4.Click += Pixel_Click;
      // 
      // C6R1_B5
      // 
      C6R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C6R1_B5.Location = new Point(78, 126);
      C6R1_B5.Name = "C6R1_B5";
      C6R1_B5.Size = new Size(14, 14);
      C6R1_B5.TabIndex = 394;
      C6R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C6R1_B5.ThreeState = true;
      C6R1_B5.UseVisualStyleBackColor = true;
      C6R1_B5.Click += Pixel_Click;
      // 
      // C6R1_B6
      // 
      C6R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C6R1_B6.Location = new Point(78, 114);
      C6R1_B6.Name = "C6R1_B6";
      C6R1_B6.Size = new Size(14, 14);
      C6R1_B6.TabIndex = 393;
      C6R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C6R1_B6.ThreeState = true;
      C6R1_B6.UseVisualStyleBackColor = true;
      C6R1_B6.Click += Pixel_Click;
      // 
      // C6R1_B7
      // 
      C6R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C6R1_B7.Location = new Point(78, 102);
      C6R1_B7.Name = "C6R1_B7";
      C6R1_B7.Size = new Size(14, 14);
      C6R1_B7.TabIndex = 392;
      C6R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C6R1_B7.ThreeState = true;
      C6R1_B7.UseVisualStyleBackColor = true;
      C6R1_B7.Click += Pixel_Click;
      // 
      // C6R0_B0
      // 
      C6R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C6R0_B0.Location = new Point(78, 90);
      C6R0_B0.Name = "C6R0_B0";
      C6R0_B0.Size = new Size(14, 14);
      C6R0_B0.TabIndex = 391;
      C6R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C6R0_B0.ThreeState = true;
      C6R0_B0.UseVisualStyleBackColor = true;
      C6R0_B0.Click += Pixel_Click;
      // 
      // C6R0_B1
      // 
      C6R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C6R0_B1.Location = new Point(78, 78);
      C6R0_B1.Name = "C6R0_B1";
      C6R0_B1.Size = new Size(14, 14);
      C6R0_B1.TabIndex = 390;
      C6R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C6R0_B1.ThreeState = true;
      C6R0_B1.UseVisualStyleBackColor = true;
      C6R0_B1.Click += Pixel_Click;
      // 
      // C6R0_B2
      // 
      C6R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C6R0_B2.Location = new Point(78, 66);
      C6R0_B2.Name = "C6R0_B2";
      C6R0_B2.Size = new Size(14, 14);
      C6R0_B2.TabIndex = 389;
      C6R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C6R0_B2.ThreeState = true;
      C6R0_B2.UseVisualStyleBackColor = true;
      C6R0_B2.Click += Pixel_Click;
      // 
      // C6R0_B3
      // 
      C6R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C6R0_B3.Location = new Point(78, 54);
      C6R0_B3.Name = "C6R0_B3";
      C6R0_B3.Size = new Size(14, 14);
      C6R0_B3.TabIndex = 388;
      C6R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C6R0_B3.ThreeState = true;
      C6R0_B3.UseVisualStyleBackColor = true;
      C6R0_B3.Click += Pixel_Click;
      // 
      // C6R0_B4
      // 
      C6R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C6R0_B4.Location = new Point(78, 42);
      C6R0_B4.Name = "C6R0_B4";
      C6R0_B4.Size = new Size(14, 14);
      C6R0_B4.TabIndex = 387;
      C6R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C6R0_B4.ThreeState = true;
      C6R0_B4.UseVisualStyleBackColor = true;
      C6R0_B4.Click += Pixel_Click;
      // 
      // C6R0_B5
      // 
      C6R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C6R0_B5.Location = new Point(78, 30);
      C6R0_B5.Name = "C6R0_B5";
      C6R0_B5.Size = new Size(14, 14);
      C6R0_B5.TabIndex = 386;
      C6R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C6R0_B5.ThreeState = true;
      C6R0_B5.UseVisualStyleBackColor = true;
      C6R0_B5.Click += Pixel_Click;
      // 
      // C6R0_B6
      // 
      C6R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C6R0_B6.Location = new Point(78, 18);
      C6R0_B6.Name = "C6R0_B6";
      C6R0_B6.Size = new Size(14, 14);
      C6R0_B6.TabIndex = 385;
      C6R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C6R0_B6.ThreeState = true;
      C6R0_B6.UseVisualStyleBackColor = true;
      C6R0_B6.Click += Pixel_Click;
      // 
      // C6R0_B7
      // 
      C6R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C6R0_B7.Location = new Point(78, 6);
      C6R0_B7.Name = "C6R0_B7";
      C6R0_B7.Size = new Size(14, 14);
      C6R0_B7.TabIndex = 384;
      C6R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C6R0_B7.ThreeState = true;
      C6R0_B7.UseVisualStyleBackColor = true;
      C6R0_B7.Click += Pixel_Click;
      // 
      // C5R1_B0
      // 
      C5R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C5R1_B0.Location = new Point(66, 186);
      C5R1_B0.Name = "C5R1_B0";
      C5R1_B0.Size = new Size(14, 14);
      C5R1_B0.TabIndex = 383;
      C5R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C5R1_B0.ThreeState = true;
      C5R1_B0.UseVisualStyleBackColor = true;
      C5R1_B0.Click += Pixel_Click;
      // 
      // C5R1_B1
      // 
      C5R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C5R1_B1.Location = new Point(66, 174);
      C5R1_B1.Name = "C5R1_B1";
      C5R1_B1.Size = new Size(14, 14);
      C5R1_B1.TabIndex = 382;
      C5R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C5R1_B1.ThreeState = true;
      C5R1_B1.UseVisualStyleBackColor = true;
      C5R1_B1.Click += Pixel_Click;
      // 
      // C5R1_B2
      // 
      C5R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C5R1_B2.Location = new Point(66, 162);
      C5R1_B2.Name = "C5R1_B2";
      C5R1_B2.Size = new Size(14, 14);
      C5R1_B2.TabIndex = 381;
      C5R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C5R1_B2.ThreeState = true;
      C5R1_B2.UseVisualStyleBackColor = true;
      C5R1_B2.Click += Pixel_Click;
      // 
      // C5R1_B3
      // 
      C5R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C5R1_B3.Location = new Point(66, 150);
      C5R1_B3.Name = "C5R1_B3";
      C5R1_B3.Size = new Size(14, 14);
      C5R1_B3.TabIndex = 380;
      C5R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C5R1_B3.ThreeState = true;
      C5R1_B3.UseVisualStyleBackColor = true;
      C5R1_B3.Click += Pixel_Click;
      // 
      // C5R1_B4
      // 
      C5R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C5R1_B4.Location = new Point(66, 138);
      C5R1_B4.Name = "C5R1_B4";
      C5R1_B4.Size = new Size(14, 14);
      C5R1_B4.TabIndex = 379;
      C5R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C5R1_B4.ThreeState = true;
      C5R1_B4.UseVisualStyleBackColor = true;
      C5R1_B4.Click += Pixel_Click;
      // 
      // C5R1_B5
      // 
      C5R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C5R1_B5.Location = new Point(66, 126);
      C5R1_B5.Name = "C5R1_B5";
      C5R1_B5.Size = new Size(14, 14);
      C5R1_B5.TabIndex = 378;
      C5R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C5R1_B5.ThreeState = true;
      C5R1_B5.UseVisualStyleBackColor = true;
      C5R1_B5.Click += Pixel_Click;
      // 
      // C5R1_B6
      // 
      C5R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C5R1_B6.Location = new Point(66, 114);
      C5R1_B6.Name = "C5R1_B6";
      C5R1_B6.Size = new Size(14, 14);
      C5R1_B6.TabIndex = 377;
      C5R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C5R1_B6.ThreeState = true;
      C5R1_B6.UseVisualStyleBackColor = true;
      C5R1_B6.Click += Pixel_Click;
      // 
      // C5R1_B7
      // 
      C5R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C5R1_B7.Location = new Point(66, 102);
      C5R1_B7.Name = "C5R1_B7";
      C5R1_B7.Size = new Size(14, 14);
      C5R1_B7.TabIndex = 376;
      C5R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C5R1_B7.ThreeState = true;
      C5R1_B7.UseVisualStyleBackColor = true;
      C5R1_B7.Click += Pixel_Click;
      // 
      // C5R0_B0
      // 
      C5R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C5R0_B0.Location = new Point(66, 90);
      C5R0_B0.Name = "C5R0_B0";
      C5R0_B0.Size = new Size(14, 14);
      C5R0_B0.TabIndex = 375;
      C5R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C5R0_B0.ThreeState = true;
      C5R0_B0.UseVisualStyleBackColor = true;
      C5R0_B0.Click += Pixel_Click;
      // 
      // C5R0_B1
      // 
      C5R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C5R0_B1.Location = new Point(66, 78);
      C5R0_B1.Name = "C5R0_B1";
      C5R0_B1.Size = new Size(14, 14);
      C5R0_B1.TabIndex = 374;
      C5R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C5R0_B1.ThreeState = true;
      C5R0_B1.UseVisualStyleBackColor = true;
      C5R0_B1.Click += Pixel_Click;
      // 
      // C5R0_B2
      // 
      C5R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C5R0_B2.Location = new Point(66, 66);
      C5R0_B2.Name = "C5R0_B2";
      C5R0_B2.Size = new Size(14, 14);
      C5R0_B2.TabIndex = 373;
      C5R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C5R0_B2.ThreeState = true;
      C5R0_B2.UseVisualStyleBackColor = true;
      C5R0_B2.Click += Pixel_Click;
      // 
      // C5R0_B3
      // 
      C5R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C5R0_B3.Location = new Point(66, 54);
      C5R0_B3.Name = "C5R0_B3";
      C5R0_B3.Size = new Size(14, 14);
      C5R0_B3.TabIndex = 372;
      C5R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C5R0_B3.ThreeState = true;
      C5R0_B3.UseVisualStyleBackColor = true;
      C5R0_B3.Click += Pixel_Click;
      // 
      // C5R0_B4
      // 
      C5R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C5R0_B4.Location = new Point(66, 42);
      C5R0_B4.Name = "C5R0_B4";
      C5R0_B4.Size = new Size(14, 14);
      C5R0_B4.TabIndex = 371;
      C5R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C5R0_B4.ThreeState = true;
      C5R0_B4.UseVisualStyleBackColor = true;
      C5R0_B4.Click += Pixel_Click;
      // 
      // C5R0_B5
      // 
      C5R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C5R0_B5.Location = new Point(66, 30);
      C5R0_B5.Name = "C5R0_B5";
      C5R0_B5.Size = new Size(14, 14);
      C5R0_B5.TabIndex = 370;
      C5R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C5R0_B5.ThreeState = true;
      C5R0_B5.UseVisualStyleBackColor = true;
      C5R0_B5.Click += Pixel_Click;
      // 
      // C5R0_B6
      // 
      C5R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C5R0_B6.Location = new Point(66, 18);
      C5R0_B6.Name = "C5R0_B6";
      C5R0_B6.Size = new Size(14, 14);
      C5R0_B6.TabIndex = 369;
      C5R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C5R0_B6.ThreeState = true;
      C5R0_B6.UseVisualStyleBackColor = true;
      C5R0_B6.Click += Pixel_Click;
      // 
      // C5R0_B7
      // 
      C5R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C5R0_B7.Location = new Point(66, 6);
      C5R0_B7.Name = "C5R0_B7";
      C5R0_B7.Size = new Size(14, 14);
      C5R0_B7.TabIndex = 368;
      C5R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C5R0_B7.ThreeState = true;
      C5R0_B7.UseVisualStyleBackColor = true;
      C5R0_B7.Click += Pixel_Click;
      // 
      // C4R1_B0
      // 
      C4R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C4R1_B0.Location = new Point(54, 186);
      C4R1_B0.Name = "C4R1_B0";
      C4R1_B0.Size = new Size(14, 14);
      C4R1_B0.TabIndex = 367;
      C4R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C4R1_B0.ThreeState = true;
      C4R1_B0.UseVisualStyleBackColor = true;
      C4R1_B0.Click += Pixel_Click;
      // 
      // C4R1_B1
      // 
      C4R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C4R1_B1.Location = new Point(54, 174);
      C4R1_B1.Name = "C4R1_B1";
      C4R1_B1.Size = new Size(14, 14);
      C4R1_B1.TabIndex = 366;
      C4R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C4R1_B1.ThreeState = true;
      C4R1_B1.UseVisualStyleBackColor = true;
      C4R1_B1.Click += Pixel_Click;
      // 
      // C4R1_B2
      // 
      C4R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C4R1_B2.Location = new Point(54, 162);
      C4R1_B2.Name = "C4R1_B2";
      C4R1_B2.Size = new Size(14, 14);
      C4R1_B2.TabIndex = 365;
      C4R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C4R1_B2.ThreeState = true;
      C4R1_B2.UseVisualStyleBackColor = true;
      C4R1_B2.Click += Pixel_Click;
      // 
      // C4R1_B3
      // 
      C4R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C4R1_B3.Location = new Point(54, 150);
      C4R1_B3.Name = "C4R1_B3";
      C4R1_B3.Size = new Size(14, 14);
      C4R1_B3.TabIndex = 364;
      C4R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C4R1_B3.ThreeState = true;
      C4R1_B3.UseVisualStyleBackColor = true;
      C4R1_B3.Click += Pixel_Click;
      // 
      // C4R1_B4
      // 
      C4R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C4R1_B4.Location = new Point(54, 138);
      C4R1_B4.Name = "C4R1_B4";
      C4R1_B4.Size = new Size(14, 14);
      C4R1_B4.TabIndex = 363;
      C4R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C4R1_B4.ThreeState = true;
      C4R1_B4.UseVisualStyleBackColor = true;
      C4R1_B4.Click += Pixel_Click;
      // 
      // C4R1_B5
      // 
      C4R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C4R1_B5.Location = new Point(54, 126);
      C4R1_B5.Name = "C4R1_B5";
      C4R1_B5.Size = new Size(14, 14);
      C4R1_B5.TabIndex = 362;
      C4R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C4R1_B5.ThreeState = true;
      C4R1_B5.UseVisualStyleBackColor = true;
      C4R1_B5.Click += Pixel_Click;
      // 
      // C4R1_B6
      // 
      C4R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C4R1_B6.Location = new Point(54, 114);
      C4R1_B6.Name = "C4R1_B6";
      C4R1_B6.Size = new Size(14, 14);
      C4R1_B6.TabIndex = 361;
      C4R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C4R1_B6.ThreeState = true;
      C4R1_B6.UseVisualStyleBackColor = true;
      C4R1_B6.Click += Pixel_Click;
      // 
      // C4R1_B7
      // 
      C4R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C4R1_B7.Location = new Point(54, 102);
      C4R1_B7.Name = "C4R1_B7";
      C4R1_B7.Size = new Size(14, 14);
      C4R1_B7.TabIndex = 360;
      C4R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C4R1_B7.ThreeState = true;
      C4R1_B7.UseVisualStyleBackColor = true;
      C4R1_B7.Click += Pixel_Click;
      // 
      // C4R0_B0
      // 
      C4R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C4R0_B0.Location = new Point(54, 90);
      C4R0_B0.Name = "C4R0_B0";
      C4R0_B0.Size = new Size(14, 14);
      C4R0_B0.TabIndex = 359;
      C4R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C4R0_B0.ThreeState = true;
      C4R0_B0.UseVisualStyleBackColor = true;
      C4R0_B0.Click += Pixel_Click;
      // 
      // C4R0_B1
      // 
      C4R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C4R0_B1.Location = new Point(54, 78);
      C4R0_B1.Name = "C4R0_B1";
      C4R0_B1.Size = new Size(14, 14);
      C4R0_B1.TabIndex = 358;
      C4R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C4R0_B1.ThreeState = true;
      C4R0_B1.UseVisualStyleBackColor = true;
      C4R0_B1.Click += Pixel_Click;
      // 
      // C4R0_B2
      // 
      C4R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C4R0_B2.Location = new Point(54, 66);
      C4R0_B2.Name = "C4R0_B2";
      C4R0_B2.Size = new Size(14, 14);
      C4R0_B2.TabIndex = 357;
      C4R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C4R0_B2.ThreeState = true;
      C4R0_B2.UseVisualStyleBackColor = true;
      C4R0_B2.Click += Pixel_Click;
      // 
      // C4R0_B3
      // 
      C4R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C4R0_B3.Location = new Point(54, 54);
      C4R0_B3.Name = "C4R0_B3";
      C4R0_B3.Size = new Size(14, 14);
      C4R0_B3.TabIndex = 356;
      C4R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C4R0_B3.ThreeState = true;
      C4R0_B3.UseVisualStyleBackColor = true;
      C4R0_B3.Click += Pixel_Click;
      // 
      // C4R0_B4
      // 
      C4R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C4R0_B4.Location = new Point(54, 42);
      C4R0_B4.Name = "C4R0_B4";
      C4R0_B4.Size = new Size(14, 14);
      C4R0_B4.TabIndex = 355;
      C4R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C4R0_B4.ThreeState = true;
      C4R0_B4.UseVisualStyleBackColor = true;
      C4R0_B4.Click += Pixel_Click;
      // 
      // C4R0_B5
      // 
      C4R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C4R0_B5.Location = new Point(54, 30);
      C4R0_B5.Name = "C4R0_B5";
      C4R0_B5.Size = new Size(14, 14);
      C4R0_B5.TabIndex = 354;
      C4R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C4R0_B5.ThreeState = true;
      C4R0_B5.UseVisualStyleBackColor = true;
      C4R0_B5.Click += Pixel_Click;
      // 
      // C4R0_B6
      // 
      C4R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C4R0_B6.Location = new Point(54, 18);
      C4R0_B6.Name = "C4R0_B6";
      C4R0_B6.Size = new Size(14, 14);
      C4R0_B6.TabIndex = 353;
      C4R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C4R0_B6.ThreeState = true;
      C4R0_B6.UseVisualStyleBackColor = true;
      C4R0_B6.Click += Pixel_Click;
      // 
      // C4R0_B7
      // 
      C4R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C4R0_B7.Location = new Point(54, 6);
      C4R0_B7.Name = "C4R0_B7";
      C4R0_B7.Size = new Size(14, 14);
      C4R0_B7.TabIndex = 352;
      C4R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C4R0_B7.ThreeState = true;
      C4R0_B7.UseVisualStyleBackColor = true;
      C4R0_B7.Click += Pixel_Click;
      // 
      // C3R1_B0
      // 
      C3R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C3R1_B0.Location = new Point(42, 186);
      C3R1_B0.Name = "C3R1_B0";
      C3R1_B0.Size = new Size(14, 14);
      C3R1_B0.TabIndex = 351;
      C3R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C3R1_B0.ThreeState = true;
      C3R1_B0.UseVisualStyleBackColor = true;
      C3R1_B0.Click += Pixel_Click;
      // 
      // C3R1_B1
      // 
      C3R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C3R1_B1.Location = new Point(42, 174);
      C3R1_B1.Name = "C3R1_B1";
      C3R1_B1.Size = new Size(14, 14);
      C3R1_B1.TabIndex = 350;
      C3R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C3R1_B1.ThreeState = true;
      C3R1_B1.UseVisualStyleBackColor = true;
      C3R1_B1.Click += Pixel_Click;
      // 
      // C3R1_B2
      // 
      C3R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C3R1_B2.Location = new Point(42, 162);
      C3R1_B2.Name = "C3R1_B2";
      C3R1_B2.Size = new Size(14, 14);
      C3R1_B2.TabIndex = 349;
      C3R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C3R1_B2.ThreeState = true;
      C3R1_B2.UseVisualStyleBackColor = true;
      C3R1_B2.Click += Pixel_Click;
      // 
      // C3R1_B3
      // 
      C3R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C3R1_B3.Location = new Point(42, 150);
      C3R1_B3.Name = "C3R1_B3";
      C3R1_B3.Size = new Size(14, 14);
      C3R1_B3.TabIndex = 348;
      C3R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C3R1_B3.ThreeState = true;
      C3R1_B3.UseVisualStyleBackColor = true;
      C3R1_B3.Click += Pixel_Click;
      // 
      // C3R1_B4
      // 
      C3R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C3R1_B4.Location = new Point(42, 138);
      C3R1_B4.Name = "C3R1_B4";
      C3R1_B4.Size = new Size(14, 14);
      C3R1_B4.TabIndex = 347;
      C3R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C3R1_B4.ThreeState = true;
      C3R1_B4.UseVisualStyleBackColor = true;
      C3R1_B4.Click += Pixel_Click;
      // 
      // C3R1_B5
      // 
      C3R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C3R1_B5.Location = new Point(42, 126);
      C3R1_B5.Name = "C3R1_B5";
      C3R1_B5.Size = new Size(14, 14);
      C3R1_B5.TabIndex = 346;
      C3R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C3R1_B5.ThreeState = true;
      C3R1_B5.UseVisualStyleBackColor = true;
      C3R1_B5.Click += Pixel_Click;
      // 
      // C3R1_B6
      // 
      C3R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C3R1_B6.Location = new Point(42, 114);
      C3R1_B6.Name = "C3R1_B6";
      C3R1_B6.Size = new Size(14, 14);
      C3R1_B6.TabIndex = 345;
      C3R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C3R1_B6.ThreeState = true;
      C3R1_B6.UseVisualStyleBackColor = true;
      C3R1_B6.Click += Pixel_Click;
      // 
      // C3R1_B7
      // 
      C3R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C3R1_B7.Location = new Point(42, 102);
      C3R1_B7.Name = "C3R1_B7";
      C3R1_B7.Size = new Size(14, 14);
      C3R1_B7.TabIndex = 344;
      C3R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C3R1_B7.ThreeState = true;
      C3R1_B7.UseVisualStyleBackColor = true;
      C3R1_B7.Click += Pixel_Click;
      // 
      // C3R0_B0
      // 
      C3R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C3R0_B0.Location = new Point(42, 90);
      C3R0_B0.Name = "C3R0_B0";
      C3R0_B0.Size = new Size(14, 14);
      C3R0_B0.TabIndex = 343;
      C3R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C3R0_B0.ThreeState = true;
      C3R0_B0.UseVisualStyleBackColor = true;
      C3R0_B0.Click += Pixel_Click;
      // 
      // C3R0_B1
      // 
      C3R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C3R0_B1.Location = new Point(42, 78);
      C3R0_B1.Name = "C3R0_B1";
      C3R0_B1.Size = new Size(14, 14);
      C3R0_B1.TabIndex = 342;
      C3R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C3R0_B1.ThreeState = true;
      C3R0_B1.UseVisualStyleBackColor = true;
      C3R0_B1.Click += Pixel_Click;
      // 
      // C3R0_B2
      // 
      C3R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C3R0_B2.Location = new Point(42, 66);
      C3R0_B2.Name = "C3R0_B2";
      C3R0_B2.Size = new Size(14, 14);
      C3R0_B2.TabIndex = 341;
      C3R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C3R0_B2.ThreeState = true;
      C3R0_B2.UseVisualStyleBackColor = true;
      C3R0_B2.Click += Pixel_Click;
      // 
      // C3R0_B3
      // 
      C3R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C3R0_B3.Location = new Point(42, 54);
      C3R0_B3.Name = "C3R0_B3";
      C3R0_B3.Size = new Size(14, 14);
      C3R0_B3.TabIndex = 340;
      C3R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C3R0_B3.ThreeState = true;
      C3R0_B3.UseVisualStyleBackColor = true;
      C3R0_B3.Click += Pixel_Click;
      // 
      // C3R0_B4
      // 
      C3R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C3R0_B4.Location = new Point(42, 42);
      C3R0_B4.Name = "C3R0_B4";
      C3R0_B4.Size = new Size(14, 14);
      C3R0_B4.TabIndex = 339;
      C3R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C3R0_B4.ThreeState = true;
      C3R0_B4.UseVisualStyleBackColor = true;
      C3R0_B4.Click += Pixel_Click;
      // 
      // C3R0_B5
      // 
      C3R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C3R0_B5.Location = new Point(42, 30);
      C3R0_B5.Name = "C3R0_B5";
      C3R0_B5.Size = new Size(14, 14);
      C3R0_B5.TabIndex = 338;
      C3R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C3R0_B5.ThreeState = true;
      C3R0_B5.UseVisualStyleBackColor = true;
      C3R0_B5.Click += Pixel_Click;
      // 
      // C3R0_B6
      // 
      C3R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C3R0_B6.Location = new Point(42, 18);
      C3R0_B6.Name = "C3R0_B6";
      C3R0_B6.Size = new Size(14, 14);
      C3R0_B6.TabIndex = 337;
      C3R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C3R0_B6.ThreeState = true;
      C3R0_B6.UseVisualStyleBackColor = true;
      C3R0_B6.Click += Pixel_Click;
      // 
      // C3R0_B7
      // 
      C3R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C3R0_B7.Location = new Point(42, 6);
      C3R0_B7.Name = "C3R0_B7";
      C3R0_B7.Size = new Size(14, 14);
      C3R0_B7.TabIndex = 336;
      C3R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C3R0_B7.ThreeState = true;
      C3R0_B7.UseVisualStyleBackColor = true;
      C3R0_B7.Click += Pixel_Click;
      // 
      // C2R1_B0
      // 
      C2R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C2R1_B0.Location = new Point(30, 186);
      C2R1_B0.Name = "C2R1_B0";
      C2R1_B0.Size = new Size(14, 14);
      C2R1_B0.TabIndex = 335;
      C2R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C2R1_B0.ThreeState = true;
      C2R1_B0.UseVisualStyleBackColor = true;
      C2R1_B0.Click += Pixel_Click;
      // 
      // C2R1_B1
      // 
      C2R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C2R1_B1.Location = new Point(30, 174);
      C2R1_B1.Name = "C2R1_B1";
      C2R1_B1.Size = new Size(14, 14);
      C2R1_B1.TabIndex = 334;
      C2R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C2R1_B1.ThreeState = true;
      C2R1_B1.UseVisualStyleBackColor = true;
      C2R1_B1.Click += Pixel_Click;
      // 
      // C2R1_B2
      // 
      C2R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C2R1_B2.Location = new Point(30, 162);
      C2R1_B2.Name = "C2R1_B2";
      C2R1_B2.Size = new Size(14, 14);
      C2R1_B2.TabIndex = 333;
      C2R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C2R1_B2.ThreeState = true;
      C2R1_B2.UseVisualStyleBackColor = true;
      C2R1_B2.Click += Pixel_Click;
      // 
      // C2R1_B3
      // 
      C2R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C2R1_B3.Location = new Point(30, 150);
      C2R1_B3.Name = "C2R1_B3";
      C2R1_B3.Size = new Size(14, 14);
      C2R1_B3.TabIndex = 332;
      C2R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C2R1_B3.ThreeState = true;
      C2R1_B3.UseVisualStyleBackColor = true;
      C2R1_B3.Click += Pixel_Click;
      // 
      // C2R1_B4
      // 
      C2R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C2R1_B4.Location = new Point(30, 138);
      C2R1_B4.Name = "C2R1_B4";
      C2R1_B4.Size = new Size(14, 14);
      C2R1_B4.TabIndex = 331;
      C2R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C2R1_B4.ThreeState = true;
      C2R1_B4.UseVisualStyleBackColor = true;
      C2R1_B4.Click += Pixel_Click;
      // 
      // C2R1_B5
      // 
      C2R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C2R1_B5.Location = new Point(30, 126);
      C2R1_B5.Name = "C2R1_B5";
      C2R1_B5.Size = new Size(14, 14);
      C2R1_B5.TabIndex = 330;
      C2R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C2R1_B5.ThreeState = true;
      C2R1_B5.UseVisualStyleBackColor = true;
      C2R1_B5.Click += Pixel_Click;
      // 
      // C2R1_B6
      // 
      C2R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C2R1_B6.Location = new Point(30, 114);
      C2R1_B6.Name = "C2R1_B6";
      C2R1_B6.Size = new Size(14, 14);
      C2R1_B6.TabIndex = 329;
      C2R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C2R1_B6.ThreeState = true;
      C2R1_B6.UseVisualStyleBackColor = true;
      C2R1_B6.Click += Pixel_Click;
      // 
      // C2R1_B7
      // 
      C2R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C2R1_B7.Location = new Point(30, 102);
      C2R1_B7.Name = "C2R1_B7";
      C2R1_B7.Size = new Size(14, 14);
      C2R1_B7.TabIndex = 328;
      C2R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C2R1_B7.ThreeState = true;
      C2R1_B7.UseVisualStyleBackColor = true;
      C2R1_B7.Click += Pixel_Click;
      // 
      // C2R0_B0
      // 
      C2R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C2R0_B0.Location = new Point(30, 90);
      C2R0_B0.Name = "C2R0_B0";
      C2R0_B0.Size = new Size(14, 14);
      C2R0_B0.TabIndex = 327;
      C2R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C2R0_B0.ThreeState = true;
      C2R0_B0.UseVisualStyleBackColor = true;
      C2R0_B0.Click += Pixel_Click;
      // 
      // C2R0_B1
      // 
      C2R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C2R0_B1.Location = new Point(30, 78);
      C2R0_B1.Name = "C2R0_B1";
      C2R0_B1.Size = new Size(14, 14);
      C2R0_B1.TabIndex = 326;
      C2R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C2R0_B1.ThreeState = true;
      C2R0_B1.UseVisualStyleBackColor = true;
      C2R0_B1.Click += Pixel_Click;
      // 
      // C2R0_B2
      // 
      C2R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C2R0_B2.Location = new Point(30, 66);
      C2R0_B2.Name = "C2R0_B2";
      C2R0_B2.Size = new Size(14, 14);
      C2R0_B2.TabIndex = 325;
      C2R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C2R0_B2.ThreeState = true;
      C2R0_B2.UseVisualStyleBackColor = true;
      C2R0_B2.Click += Pixel_Click;
      // 
      // C2R0_B3
      // 
      C2R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C2R0_B3.Location = new Point(30, 54);
      C2R0_B3.Name = "C2R0_B3";
      C2R0_B3.Size = new Size(14, 14);
      C2R0_B3.TabIndex = 324;
      C2R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C2R0_B3.ThreeState = true;
      C2R0_B3.UseVisualStyleBackColor = true;
      C2R0_B3.Click += Pixel_Click;
      // 
      // C2R0_B4
      // 
      C2R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C2R0_B4.Location = new Point(30, 42);
      C2R0_B4.Name = "C2R0_B4";
      C2R0_B4.Size = new Size(14, 14);
      C2R0_B4.TabIndex = 323;
      C2R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C2R0_B4.ThreeState = true;
      C2R0_B4.UseVisualStyleBackColor = true;
      C2R0_B4.Click += Pixel_Click;
      // 
      // C2R0_B5
      // 
      C2R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C2R0_B5.Location = new Point(30, 30);
      C2R0_B5.Name = "C2R0_B5";
      C2R0_B5.Size = new Size(14, 14);
      C2R0_B5.TabIndex = 322;
      C2R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C2R0_B5.ThreeState = true;
      C2R0_B5.UseVisualStyleBackColor = true;
      C2R0_B5.Click += Pixel_Click;
      // 
      // C2R0_B6
      // 
      C2R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C2R0_B6.Location = new Point(30, 18);
      C2R0_B6.Name = "C2R0_B6";
      C2R0_B6.Size = new Size(14, 14);
      C2R0_B6.TabIndex = 321;
      C2R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C2R0_B6.ThreeState = true;
      C2R0_B6.UseVisualStyleBackColor = true;
      C2R0_B6.Click += Pixel_Click;
      // 
      // C2R0_B7
      // 
      C2R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C2R0_B7.Location = new Point(30, 6);
      C2R0_B7.Name = "C2R0_B7";
      C2R0_B7.Size = new Size(14, 14);
      C2R0_B7.TabIndex = 320;
      C2R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C2R0_B7.ThreeState = true;
      C2R0_B7.UseVisualStyleBackColor = true;
      C2R0_B7.Click += Pixel_Click;
      // 
      // C1R1_B0
      // 
      C1R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C1R1_B0.Location = new Point(18, 186);
      C1R1_B0.Name = "C1R1_B0";
      C1R1_B0.Size = new Size(14, 14);
      C1R1_B0.TabIndex = 319;
      C1R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C1R1_B0.ThreeState = true;
      C1R1_B0.UseVisualStyleBackColor = true;
      C1R1_B0.Click += Pixel_Click;
      // 
      // C1R1_B1
      // 
      C1R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C1R1_B1.Location = new Point(18, 174);
      C1R1_B1.Name = "C1R1_B1";
      C1R1_B1.Size = new Size(14, 14);
      C1R1_B1.TabIndex = 318;
      C1R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C1R1_B1.ThreeState = true;
      C1R1_B1.UseVisualStyleBackColor = true;
      C1R1_B1.Click += Pixel_Click;
      // 
      // C1R1_B2
      // 
      C1R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C1R1_B2.Location = new Point(18, 162);
      C1R1_B2.Name = "C1R1_B2";
      C1R1_B2.Size = new Size(14, 14);
      C1R1_B2.TabIndex = 317;
      C1R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C1R1_B2.ThreeState = true;
      C1R1_B2.UseVisualStyleBackColor = true;
      C1R1_B2.Click += Pixel_Click;
      // 
      // C1R1_B3
      // 
      C1R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C1R1_B3.Location = new Point(18, 150);
      C1R1_B3.Name = "C1R1_B3";
      C1R1_B3.Size = new Size(14, 14);
      C1R1_B3.TabIndex = 316;
      C1R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C1R1_B3.ThreeState = true;
      C1R1_B3.UseVisualStyleBackColor = true;
      C1R1_B3.Click += Pixel_Click;
      // 
      // C1R1_B4
      // 
      C1R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C1R1_B4.Location = new Point(18, 138);
      C1R1_B4.Name = "C1R1_B4";
      C1R1_B4.Size = new Size(14, 14);
      C1R1_B4.TabIndex = 315;
      C1R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C1R1_B4.ThreeState = true;
      C1R1_B4.UseVisualStyleBackColor = true;
      C1R1_B4.Click += Pixel_Click;
      // 
      // C1R1_B5
      // 
      C1R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C1R1_B5.Location = new Point(18, 126);
      C1R1_B5.Name = "C1R1_B5";
      C1R1_B5.Size = new Size(14, 14);
      C1R1_B5.TabIndex = 314;
      C1R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C1R1_B5.ThreeState = true;
      C1R1_B5.UseVisualStyleBackColor = true;
      C1R1_B5.Click += Pixel_Click;
      // 
      // C1R1_B6
      // 
      C1R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C1R1_B6.Location = new Point(18, 114);
      C1R1_B6.Name = "C1R1_B6";
      C1R1_B6.Size = new Size(14, 14);
      C1R1_B6.TabIndex = 313;
      C1R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C1R1_B6.ThreeState = true;
      C1R1_B6.UseVisualStyleBackColor = true;
      C1R1_B6.Click += Pixel_Click;
      // 
      // C1R1_B7
      // 
      C1R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C1R1_B7.Location = new Point(18, 102);
      C1R1_B7.Name = "C1R1_B7";
      C1R1_B7.Size = new Size(14, 14);
      C1R1_B7.TabIndex = 312;
      C1R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C1R1_B7.ThreeState = true;
      C1R1_B7.UseVisualStyleBackColor = true;
      C1R1_B7.Click += Pixel_Click;
      // 
      // C1R0_B0
      // 
      C1R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C1R0_B0.Location = new Point(18, 90);
      C1R0_B0.Name = "C1R0_B0";
      C1R0_B0.Size = new Size(14, 14);
      C1R0_B0.TabIndex = 311;
      C1R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C1R0_B0.ThreeState = true;
      C1R0_B0.UseVisualStyleBackColor = true;
      C1R0_B0.Click += Pixel_Click;
      // 
      // C1R0_B1
      // 
      C1R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C1R0_B1.Location = new Point(18, 78);
      C1R0_B1.Name = "C1R0_B1";
      C1R0_B1.Size = new Size(14, 14);
      C1R0_B1.TabIndex = 310;
      C1R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C1R0_B1.ThreeState = true;
      C1R0_B1.UseVisualStyleBackColor = true;
      C1R0_B1.Click += Pixel_Click;
      // 
      // C1R0_B2
      // 
      C1R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C1R0_B2.Location = new Point(18, 66);
      C1R0_B2.Name = "C1R0_B2";
      C1R0_B2.Size = new Size(14, 14);
      C1R0_B2.TabIndex = 309;
      C1R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C1R0_B2.ThreeState = true;
      C1R0_B2.UseVisualStyleBackColor = true;
      C1R0_B2.Click += Pixel_Click;
      // 
      // C1R0_B3
      // 
      C1R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C1R0_B3.Location = new Point(18, 54);
      C1R0_B3.Name = "C1R0_B3";
      C1R0_B3.Size = new Size(14, 14);
      C1R0_B3.TabIndex = 308;
      C1R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C1R0_B3.ThreeState = true;
      C1R0_B3.UseVisualStyleBackColor = true;
      C1R0_B3.Click += Pixel_Click;
      // 
      // C1R0_B4
      // 
      C1R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C1R0_B4.Location = new Point(18, 42);
      C1R0_B4.Name = "C1R0_B4";
      C1R0_B4.Size = new Size(14, 14);
      C1R0_B4.TabIndex = 307;
      C1R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C1R0_B4.ThreeState = true;
      C1R0_B4.UseVisualStyleBackColor = true;
      C1R0_B4.Click += Pixel_Click;
      // 
      // C1R0_B5
      // 
      C1R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C1R0_B5.Location = new Point(18, 30);
      C1R0_B5.Name = "C1R0_B5";
      C1R0_B5.Size = new Size(14, 14);
      C1R0_B5.TabIndex = 306;
      C1R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C1R0_B5.ThreeState = true;
      C1R0_B5.UseVisualStyleBackColor = true;
      C1R0_B5.Click += Pixel_Click;
      // 
      // C1R0_B6
      // 
      C1R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C1R0_B6.Location = new Point(18, 18);
      C1R0_B6.Name = "C1R0_B6";
      C1R0_B6.Size = new Size(14, 14);
      C1R0_B6.TabIndex = 305;
      C1R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C1R0_B6.ThreeState = true;
      C1R0_B6.UseVisualStyleBackColor = true;
      C1R0_B6.Click += Pixel_Click;
      // 
      // C1R0_B7
      // 
      C1R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C1R0_B7.Location = new Point(18, 6);
      C1R0_B7.Name = "C1R0_B7";
      C1R0_B7.Size = new Size(14, 14);
      C1R0_B7.TabIndex = 304;
      C1R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C1R0_B7.ThreeState = true;
      C1R0_B7.UseVisualStyleBackColor = true;
      C1R0_B7.Click += Pixel_Click;
      // 
      // C0R1_B0
      // 
      C0R1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C0R1_B0.Location = new Point(6, 186);
      C0R1_B0.Name = "C0R1_B0";
      C0R1_B0.Size = new Size(14, 14);
      C0R1_B0.TabIndex = 303;
      C0R1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C0R1_B0.ThreeState = true;
      C0R1_B0.UseVisualStyleBackColor = true;
      C0R1_B0.Click += Pixel_Click;
      // 
      // C0R1_B1
      // 
      C0R1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C0R1_B1.Location = new Point(6, 174);
      C0R1_B1.Name = "C0R1_B1";
      C0R1_B1.Size = new Size(14, 14);
      C0R1_B1.TabIndex = 302;
      C0R1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C0R1_B1.ThreeState = true;
      C0R1_B1.UseVisualStyleBackColor = true;
      C0R1_B1.Click += Pixel_Click;
      // 
      // C0R1_B2
      // 
      C0R1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C0R1_B2.Location = new Point(6, 162);
      C0R1_B2.Name = "C0R1_B2";
      C0R1_B2.Size = new Size(14, 14);
      C0R1_B2.TabIndex = 301;
      C0R1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C0R1_B2.ThreeState = true;
      C0R1_B2.UseVisualStyleBackColor = true;
      C0R1_B2.Click += Pixel_Click;
      // 
      // C0R1_B3
      // 
      C0R1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C0R1_B3.Location = new Point(6, 150);
      C0R1_B3.Name = "C0R1_B3";
      C0R1_B3.Size = new Size(14, 14);
      C0R1_B3.TabIndex = 300;
      C0R1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C0R1_B3.ThreeState = true;
      C0R1_B3.UseVisualStyleBackColor = true;
      C0R1_B3.Click += Pixel_Click;
      // 
      // C0R1_B4
      // 
      C0R1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C0R1_B4.Location = new Point(6, 138);
      C0R1_B4.Name = "C0R1_B4";
      C0R1_B4.Size = new Size(14, 14);
      C0R1_B4.TabIndex = 299;
      C0R1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C0R1_B4.ThreeState = true;
      C0R1_B4.UseVisualStyleBackColor = true;
      C0R1_B4.Click += Pixel_Click;
      // 
      // C0R1_B5
      // 
      C0R1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C0R1_B5.Location = new Point(6, 126);
      C0R1_B5.Name = "C0R1_B5";
      C0R1_B5.Size = new Size(14, 14);
      C0R1_B5.TabIndex = 298;
      C0R1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C0R1_B5.ThreeState = true;
      C0R1_B5.UseVisualStyleBackColor = true;
      C0R1_B5.Click += Pixel_Click;
      // 
      // C0R1_B6
      // 
      C0R1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C0R1_B6.Location = new Point(6, 114);
      C0R1_B6.Name = "C0R1_B6";
      C0R1_B6.Size = new Size(14, 14);
      C0R1_B6.TabIndex = 297;
      C0R1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C0R1_B6.ThreeState = true;
      C0R1_B6.UseVisualStyleBackColor = true;
      C0R1_B6.Click += Pixel_Click;
      // 
      // C0R1_B7
      // 
      C0R1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C0R1_B7.Location = new Point(6, 102);
      C0R1_B7.Name = "C0R1_B7";
      C0R1_B7.Size = new Size(14, 14);
      C0R1_B7.TabIndex = 296;
      C0R1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C0R1_B7.ThreeState = true;
      C0R1_B7.UseVisualStyleBackColor = true;
      C0R1_B7.Click += Pixel_Click;
      // 
      // C0R0_B0
      // 
      C0R0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C0R0_B0.Location = new Point(6, 90);
      C0R0_B0.Name = "C0R0_B0";
      C0R0_B0.Size = new Size(14, 14);
      C0R0_B0.TabIndex = 295;
      C0R0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C0R0_B0.ThreeState = true;
      C0R0_B0.UseVisualStyleBackColor = true;
      C0R0_B0.Click += Pixel_Click;
      // 
      // C0R0_B1
      // 
      C0R0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C0R0_B1.Location = new Point(6, 78);
      C0R0_B1.Name = "C0R0_B1";
      C0R0_B1.Size = new Size(14, 14);
      C0R0_B1.TabIndex = 294;
      C0R0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C0R0_B1.ThreeState = true;
      C0R0_B1.UseVisualStyleBackColor = true;
      C0R0_B1.Click += Pixel_Click;
      // 
      // C0R0_B2
      // 
      C0R0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C0R0_B2.Location = new Point(6, 66);
      C0R0_B2.Name = "C0R0_B2";
      C0R0_B2.Size = new Size(14, 14);
      C0R0_B2.TabIndex = 293;
      C0R0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C0R0_B2.ThreeState = true;
      C0R0_B2.UseVisualStyleBackColor = true;
      C0R0_B2.Click += Pixel_Click;
      // 
      // C0R0_B3
      // 
      C0R0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C0R0_B3.Location = new Point(6, 54);
      C0R0_B3.Name = "C0R0_B3";
      C0R0_B3.Size = new Size(14, 14);
      C0R0_B3.TabIndex = 292;
      C0R0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C0R0_B3.ThreeState = true;
      C0R0_B3.UseVisualStyleBackColor = true;
      C0R0_B3.Click += Pixel_Click;
      // 
      // C0R0_B4
      // 
      C0R0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C0R0_B4.Location = new Point(6, 42);
      C0R0_B4.Name = "C0R0_B4";
      C0R0_B4.Size = new Size(14, 14);
      C0R0_B4.TabIndex = 291;
      C0R0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C0R0_B4.ThreeState = true;
      C0R0_B4.UseVisualStyleBackColor = true;
      C0R0_B4.Click += Pixel_Click;
      // 
      // C0R0_B5
      // 
      C0R0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C0R0_B5.Location = new Point(6, 30);
      C0R0_B5.Name = "C0R0_B5";
      C0R0_B5.Size = new Size(14, 14);
      C0R0_B5.TabIndex = 290;
      C0R0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C0R0_B5.ThreeState = true;
      C0R0_B5.UseVisualStyleBackColor = true;
      C0R0_B5.Click += Pixel_Click;
      // 
      // C0R0_B6
      // 
      C0R0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C0R0_B6.Location = new Point(6, 18);
      C0R0_B6.Name = "C0R0_B6";
      C0R0_B6.Size = new Size(14, 14);
      C0R0_B6.TabIndex = 289;
      C0R0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C0R0_B6.ThreeState = true;
      C0R0_B6.UseVisualStyleBackColor = true;
      C0R0_B6.Click += Pixel_Click;
      // 
      // C0R0_B7
      // 
      C0R0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C0R0_B7.Location = new Point(6, 6);
      C0R0_B7.Name = "C0R0_B7";
      C0R0_B7.Size = new Size(14, 14);
      C0R0_B7.TabIndex = 288;
      C0R0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C0R0_B7.ThreeState = true;
      C0R0_B7.UseVisualStyleBackColor = true;
      C0R0_B7.Click += Pixel_Click;
      // 
      // cmdLoadInternal
      // 
      cmdLoadInternal.Anchor = (((AnchorStyles.Top | AnchorStyles.Right)));
      cmdLoadInternal.Location = new Point(314, 221);
      cmdLoadInternal.Name = "cmdLoadInternal";
      cmdLoadInternal.Size = new Size(75, 23);
      cmdLoadInternal.TabIndex = 548;
      cmdLoadInternal.Text = "Internal";
      cmdLoadInternal.UseVisualStyleBackColor = true;
      cmdLoadInternal.Click += cmdLoadInternal_Click;
      // 
      // cmdLoadCustom
      // 
      cmdLoadCustom.Anchor = (((AnchorStyles.Top | AnchorStyles.Right)));
      cmdLoadCustom.Location = new Point(314, 196);
      cmdLoadCustom.Name = "cmdLoadCustom";
      cmdLoadCustom.Size = new Size(75, 23);
      cmdLoadCustom.TabIndex = 549;
      cmdLoadCustom.Text = "Custom";
      cmdLoadCustom.UseVisualStyleBackColor = true;
      cmdLoadCustom.Click += cmdLoadCustom_Click;
      // 
      // cmdSave
      // 
      cmdSave.Anchor = (((AnchorStyles.Bottom | AnchorStyles.Right)));
      cmdSave.Enabled = false;
      cmdSave.Location = new Point(233, 399);
      cmdSave.Name = "cmdSave";
      cmdSave.Size = new Size(75, 23);
      cmdSave.TabIndex = 550;
      cmdSave.Text = "Save";
      cmdSave.UseVisualStyleBackColor = true;
      cmdSave.Click += cmdSave_Click;
      // 
      // cmdExit
      // 
      cmdExit.Anchor = (((AnchorStyles.Bottom | AnchorStyles.Right)));
      cmdExit.Location = new Point(314, 399);
      cmdExit.Name = "cmdExit";
      cmdExit.Size = new Size(75, 23);
      cmdExit.TabIndex = 551;
      cmdExit.Text = "Exit";
      cmdExit.UseVisualStyleBackColor = true;
      cmdExit.Click += cmdExit_Click;
      // 
      // label21
      // 
      label21.Anchor = (((AnchorStyles.Top | AnchorStyles.Right)));
      label21.AutoSize = true;
      label21.Location = new Point(311, 178);
      label21.Name = "label21";
      label21.Size = new Size(79, 13);
      label21.TabIndex = 552;
      label21.Text = "Load ICON Set";
      label21.Visible = false;
      // 
      // iMONLCDg_IconEdit
      // 
      AutoScaleDimensions = new SizeF(6F, 13F);
      ClientSize = new Size(392, 424);
      Controls.Add(label21);
      Controls.Add(cmdExit);
      Controls.Add(cmdSave);
      Controls.Add(cmdLoadCustom);
      Controls.Add(cmdLoadInternal);
      Controls.Add(panel1);
      Controls.Add(label16);
      Controls.Add(label17);
      Controls.Add(label18);
      Controls.Add(label19);
      Controls.Add(label20);
      Controls.Add(label11);
      Controls.Add(label12);
      Controls.Add(label13);
      Controls.Add(label14);
      Controls.Add(label15);
      Controls.Add(label6);
      Controls.Add(label7);
      Controls.Add(label8);
      Controls.Add(label9);
      Controls.Add(label10);
      Controls.Add(label5);
      Controls.Add(label3);
      Controls.Add(label4);
      Controls.Add(label2);
      Controls.Add(label1);
      Controls.Add(Icon9);
      Controls.Add(Icon8);
      Controls.Add(Icon7);
      Controls.Add(Icon6);
      Controls.Add(Icon5);
      Controls.Add(Icon4);
      Controls.Add(Icon3);
      Controls.Add(Icon2);
      Controls.Add(Icon1);
      Controls.Add(Icon0);
      Name = "iMONLCDg_IconEdit";
      Text = "iMONLCDg_IconEdit";
      ((ISupportInitialize)(Icon0)).EndInit();
      ((ISupportInitialize)(Icon1)).EndInit();
      ((ISupportInitialize)(Icon2)).EndInit();
      ((ISupportInitialize)(Icon3)).EndInit();
      ((ISupportInitialize)(Icon4)).EndInit();
      ((ISupportInitialize)(Icon9)).EndInit();
      ((ISupportInitialize)(Icon8)).EndInit();
      ((ISupportInitialize)(Icon7)).EndInit();
      ((ISupportInitialize)(Icon6)).EndInit();
      ((ISupportInitialize)(Icon5)).EndInit();
      panel1.ResumeLayout(false);
      panel1.PerformLayout();
      ResumeLayout(false);
      PerformLayout();
    }

    public void LoadCustomIcons()
    {
      var table = new DataTable("LargeIcons");
      var column = new DataColumn("IconID");
      var column2 = new DataColumn("IData0");
      var column3 = new DataColumn("IData1");
      var column4 = new DataColumn("IData2");
      var column5 = new DataColumn("IData3");
      var column6 = new DataColumn("IData4");
      var column7 = new DataColumn("IData5");
      var column8 = new DataColumn("IData6");
      var column9 = new DataColumn("IData7");
      var column10 = new DataColumn("IData8");
      var column11 = new DataColumn("IData9");
      var column12 = new DataColumn("IData10");
      var column13 = new DataColumn("IData11");
      var column14 = new DataColumn("IData12");
      var column15 = new DataColumn("IData13");
      var column16 = new DataColumn("IData14");
      var column17 = new DataColumn("IData15");
      var column18 = new DataColumn("IData16");
      var column19 = new DataColumn("IData17");
      var column20 = new DataColumn("IData18");
      var column21 = new DataColumn("IData19");
      var column22 = new DataColumn("IData20");
      var column23 = new DataColumn("IData21");
      var column24 = new DataColumn("IData22");
      var column25 = new DataColumn("IData23");
      var column26 = new DataColumn("IData24");
      var column27 = new DataColumn("IData25");
      var column28 = new DataColumn("IData26");
      var column29 = new DataColumn("IData27");
      var column30 = new DataColumn("IData28");
      var column31 = new DataColumn("IData29");
      var column32 = new DataColumn("IData30");
      var column33 = new DataColumn("IData31");
      column.DataType = typeof (byte);
      table.Columns.Add(column);
      column2.DataType = typeof (byte);
      table.Columns.Add(column2);
      column3.DataType = typeof (byte);
      table.Columns.Add(column3);
      column4.DataType = typeof (byte);
      table.Columns.Add(column4);
      column5.DataType = typeof (byte);
      table.Columns.Add(column5);
      column6.DataType = typeof (byte);
      table.Columns.Add(column6);
      column7.DataType = typeof (byte);
      table.Columns.Add(column7);
      column8.DataType = typeof (byte);
      table.Columns.Add(column8);
      column9.DataType = typeof (byte);
      table.Columns.Add(column9);
      column10.DataType = typeof (byte);
      table.Columns.Add(column10);
      column11.DataType = typeof (byte);
      table.Columns.Add(column11);
      column12.DataType = typeof (byte);
      table.Columns.Add(column12);
      column13.DataType = typeof (byte);
      table.Columns.Add(column13);
      column14.DataType = typeof (byte);
      table.Columns.Add(column14);
      column15.DataType = typeof (byte);
      table.Columns.Add(column15);
      column16.DataType = typeof (byte);
      table.Columns.Add(column16);
      column17.DataType = typeof (byte);
      table.Columns.Add(column17);
      column18.DataType = typeof (byte);
      table.Columns.Add(column18);
      column19.DataType = typeof (byte);
      table.Columns.Add(column19);
      column20.DataType = typeof (byte);
      table.Columns.Add(column20);
      column21.DataType = typeof (byte);
      table.Columns.Add(column21);
      column22.DataType = typeof (byte);
      table.Columns.Add(column22);
      column23.DataType = typeof (byte);
      table.Columns.Add(column23);
      column24.DataType = typeof (byte);
      table.Columns.Add(column24);
      column25.DataType = typeof (byte);
      table.Columns.Add(column25);
      column26.DataType = typeof (byte);
      table.Columns.Add(column26);
      column27.DataType = typeof (byte);
      table.Columns.Add(column27);
      column28.DataType = typeof (byte);
      table.Columns.Add(column28);
      column29.DataType = typeof (byte);
      table.Columns.Add(column29);
      column30.DataType = typeof (byte);
      table.Columns.Add(column30);
      column31.DataType = typeof (byte);
      table.Columns.Add(column31);
      column32.DataType = typeof (byte);
      table.Columns.Add(column32);
      column33.DataType = typeof (byte);
      table.Columns.Add(column33);
      table.Clear();
      if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_icons.xml")))
      {
        table.Rows.Clear();
        var serializer = new XmlSerializer(typeof (DataTable));
        var xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_icons.xml"));
        table = (DataTable)serializer.Deserialize(xmlReader);
        xmlReader.Close();
        for (int i = 0; i < 10; i++)
        {
          DataRow row = table.Rows[i];
          for (int j = 1; j < 0x21; j++)
          {
            _IconBuffer[i, j - 1] = (byte)row[j];
          }
        }
        Log.Debug("LoadLargeIconData() - completed");
        CopyBufferToGraphics();
        IconsChanged = false;
      }
      else
      {
        LoadInteralIcons();
      }
    }

    public void LoadInteralIcons()
    {
      for (int i = 0; i < 10; i++)
      {
        for (int j = 0; j < 0x20; j++)
        {
          _IconBuffer[i, j] = iMONLCDg._InternalLargeIcons[i, j];
        }
      }
      CopyBufferToGraphics();
      IconsChanged = false;
      EnableIconSelection(true);
    }

    private static void Pixel_Click(object sender, EventArgs e)
    {
      try
      {
        var box = (CheckBox)sender;
        box.CheckState = box.Checked ? CheckState.Indeterminate : CheckState.Unchecked;
      }
      catch (Exception exception)
      {
        Log.Debug("CAUGHT EXCEPTION: {0}", new object[] {exception});
      }
    }

    private void SetEditPixel(int Column, int Row, bool SetOn)
    {
      int num = (Row > 7) ? 0 : 1;
      int num2 = (Row < 8) ? Row : (Row - 8);
      string key = "C" + Column.ToString().Trim() + "R" + num.ToString().Trim() + "_B" + num2.ToString().Trim();
      Control[] controlArray = panel1.Controls.Find(key, false);
      if (controlArray.Length > 0)
      {
        var box = (CheckBox)controlArray[0];
        box.CheckState = SetOn ? CheckState.Indeterminate : CheckState.Unchecked;
      }
    }
  }
}