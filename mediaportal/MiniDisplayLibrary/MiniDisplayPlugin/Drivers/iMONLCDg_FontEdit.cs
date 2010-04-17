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
  public class iMONLCDg_FontEdit : MPConfigForm
  {
    private static readonly byte[,] _FontBuffer = new byte[0x100,6];
    private readonly Bitmap[] IconGraphics = new Bitmap[0x100];
    private CheckBox C0_B0;
    private CheckBox C0_B1;
    private CheckBox C0_B2;
    private CheckBox C0_B3;
    private CheckBox C0_B4;
    private CheckBox C0_B5;
    private CheckBox C0_B6;
    private CheckBox C0_B7;
    private CheckBox C1_B0;
    private CheckBox C1_B1;
    private CheckBox C1_B2;
    private CheckBox C1_B3;
    private CheckBox C1_B4;
    private CheckBox C1_B5;
    private CheckBox C1_B6;
    private CheckBox C1_B7;
    private CheckBox C2_B0;
    private CheckBox C2_B1;
    private CheckBox C2_B2;
    private CheckBox C2_B3;
    private CheckBox C2_B4;
    private CheckBox C2_B5;
    private CheckBox C2_B6;
    private CheckBox C2_B7;
    private CheckBox C3_B0;
    private CheckBox C3_B1;
    private CheckBox C3_B2;
    private CheckBox C3_B3;
    private CheckBox C3_B4;
    private CheckBox C3_B5;
    private CheckBox C3_B6;
    private CheckBox C3_B7;
    private CheckBox C4_B0;
    private CheckBox C4_B1;
    private CheckBox C4_B2;
    private CheckBox C4_B3;
    private CheckBox C4_B4;
    private CheckBox C4_B5;
    private CheckBox C4_B6;
    private CheckBox C4_B7;
    private CheckBox C5_B0;
    private CheckBox C5_B1;
    private CheckBox C5_B2;
    private CheckBox C5_B3;
    private CheckBox C5_B4;
    private CheckBox C5_B5;
    private CheckBox C5_B6;
    private CheckBox C5_B7;
    private Button cmdCancelEdit;
    private Button cmdClearAll;
    private Button cmdExit;
    private Button cmdInvert;
    private Button cmdLoadCustom;
    private Button cmdLoadInternal;
    private Button cmdSave;
    private Button cmdSaveEdit;
    private Button cmdSetAll;
    // private IContainer components;
    private int EditIndex = -1;
    private PictureBox Icon0;
    private PictureBox Icon1;
    private PictureBox Icon10;
    private PictureBox Icon100;
    private PictureBox Icon101;
    private PictureBox Icon102;
    private PictureBox Icon103;
    private PictureBox Icon104;
    private PictureBox Icon105;
    private PictureBox Icon106;
    private PictureBox Icon107;
    private PictureBox Icon108;
    private PictureBox Icon109;
    private PictureBox Icon11;
    private PictureBox Icon110;
    private PictureBox Icon111;
    private PictureBox Icon112;
    private PictureBox Icon113;
    private PictureBox Icon114;
    private PictureBox Icon115;
    private PictureBox Icon116;
    private PictureBox Icon117;
    private PictureBox Icon118;
    private PictureBox Icon119;
    private PictureBox Icon12;
    private PictureBox Icon120;
    private PictureBox Icon121;
    private PictureBox Icon122;
    private PictureBox Icon123;
    private PictureBox Icon124;
    private PictureBox Icon125;
    private PictureBox Icon126;
    private PictureBox Icon127;
    private PictureBox Icon128;
    private PictureBox Icon129;
    private PictureBox Icon13;
    private PictureBox Icon130;
    private PictureBox Icon131;
    private PictureBox Icon132;
    private PictureBox Icon133;
    private PictureBox Icon134;
    private PictureBox Icon135;
    private PictureBox Icon136;
    private PictureBox Icon137;
    private PictureBox Icon138;
    private PictureBox Icon139;
    private PictureBox Icon14;
    private PictureBox Icon140;
    private PictureBox Icon141;
    private PictureBox Icon142;
    private PictureBox Icon143;
    private PictureBox Icon144;
    private PictureBox Icon145;
    private PictureBox Icon146;
    private PictureBox Icon147;
    private PictureBox Icon148;
    private PictureBox Icon149;
    private PictureBox Icon15;
    private PictureBox Icon150;
    private PictureBox Icon151;
    private PictureBox Icon152;
    private PictureBox Icon153;
    private PictureBox Icon154;
    private PictureBox Icon155;
    private PictureBox Icon156;
    private PictureBox Icon157;
    private PictureBox Icon158;
    private PictureBox Icon159;
    private PictureBox Icon16;
    private PictureBox Icon160;
    private PictureBox Icon161;
    private PictureBox Icon162;
    private PictureBox Icon163;
    private PictureBox Icon164;
    private PictureBox Icon165;
    private PictureBox Icon166;
    private PictureBox Icon167;
    private PictureBox Icon168;
    private PictureBox Icon169;
    private PictureBox Icon17;
    private PictureBox Icon170;
    private PictureBox Icon171;
    private PictureBox Icon172;
    private PictureBox Icon173;
    private PictureBox Icon174;
    private PictureBox Icon175;
    private PictureBox Icon176;
    private PictureBox Icon177;
    private PictureBox Icon178;
    private PictureBox Icon179;
    private PictureBox Icon18;
    private PictureBox Icon180;
    private PictureBox Icon181;
    private PictureBox Icon182;
    private PictureBox Icon183;
    private PictureBox Icon184;
    private PictureBox Icon185;
    private PictureBox Icon186;
    private PictureBox Icon187;
    private PictureBox Icon188;
    private PictureBox Icon189;
    private PictureBox Icon19;
    private PictureBox Icon190;
    private PictureBox Icon191;
    private PictureBox Icon192;
    private PictureBox Icon193;
    private PictureBox Icon194;
    private PictureBox Icon195;
    private PictureBox Icon196;
    private PictureBox Icon197;
    private PictureBox Icon198;
    private PictureBox Icon199;
    private PictureBox Icon2;
    private PictureBox Icon20;
    private PictureBox Icon200;
    private PictureBox Icon201;
    private PictureBox Icon202;
    private PictureBox Icon203;
    private PictureBox Icon204;
    private PictureBox Icon205;
    private PictureBox Icon206;
    private PictureBox Icon207;
    private PictureBox Icon208;
    private PictureBox Icon209;
    private PictureBox Icon21;
    private PictureBox Icon210;
    private PictureBox Icon211;
    private PictureBox Icon212;
    private PictureBox Icon213;
    private PictureBox Icon214;
    private PictureBox Icon215;
    private PictureBox Icon216;
    private PictureBox Icon217;
    private PictureBox Icon218;
    private PictureBox Icon219;
    private PictureBox Icon22;
    private PictureBox Icon220;
    private PictureBox Icon221;
    private PictureBox Icon222;
    private PictureBox Icon223;
    private PictureBox Icon224;
    private PictureBox Icon225;
    private PictureBox Icon226;
    private PictureBox Icon227;
    private PictureBox Icon228;
    private PictureBox Icon229;
    private PictureBox Icon23;
    private PictureBox Icon230;
    private PictureBox Icon231;
    private PictureBox Icon232;
    private PictureBox Icon233;
    private PictureBox Icon234;
    private PictureBox Icon235;
    private PictureBox Icon236;
    private PictureBox Icon237;
    private PictureBox Icon238;
    private PictureBox Icon239;
    private PictureBox Icon24;
    private PictureBox Icon240;
    private PictureBox Icon241;
    private PictureBox Icon242;
    private PictureBox Icon243;
    private PictureBox Icon244;
    private PictureBox Icon245;
    private PictureBox Icon246;
    private PictureBox Icon247;
    private PictureBox Icon248;
    private PictureBox Icon249;
    private PictureBox Icon25;
    private PictureBox Icon250;
    private PictureBox Icon251;
    private PictureBox Icon252;
    private PictureBox Icon253;
    private PictureBox Icon254;
    private PictureBox Icon255;
    private PictureBox Icon26;
    private PictureBox Icon27;
    private PictureBox Icon28;
    private PictureBox Icon29;
    private PictureBox Icon3;
    private PictureBox Icon30;
    private PictureBox Icon31;
    private PictureBox Icon32;
    private PictureBox Icon33;
    private PictureBox Icon34;
    private PictureBox Icon35;
    private PictureBox Icon36;
    private PictureBox Icon37;
    private PictureBox Icon38;
    private PictureBox Icon39;
    private PictureBox Icon4;
    private PictureBox Icon40;
    private PictureBox Icon41;
    private PictureBox Icon42;
    private PictureBox Icon43;
    private PictureBox Icon44;
    private PictureBox Icon45;
    private PictureBox Icon46;
    private PictureBox Icon47;
    private PictureBox Icon48;
    private PictureBox Icon49;
    private PictureBox Icon5;
    private PictureBox Icon50;
    private PictureBox Icon51;
    private PictureBox Icon52;
    private PictureBox Icon53;
    private PictureBox Icon54;
    private PictureBox Icon55;
    private PictureBox Icon56;
    private PictureBox Icon57;
    private PictureBox Icon58;
    private PictureBox Icon59;
    private PictureBox Icon6;
    private PictureBox Icon60;
    private PictureBox Icon61;
    private PictureBox Icon62;
    private PictureBox Icon63;
    private PictureBox Icon64;
    private PictureBox Icon65;
    private PictureBox Icon66;
    private PictureBox Icon67;
    private PictureBox Icon68;
    private PictureBox Icon69;
    private PictureBox Icon7;
    private PictureBox Icon70;
    private PictureBox Icon71;
    private PictureBox Icon72;
    private PictureBox Icon73;
    private PictureBox Icon74;
    private PictureBox Icon75;
    private PictureBox Icon76;
    private PictureBox Icon77;
    private PictureBox Icon78;
    private PictureBox Icon79;
    private PictureBox Icon8;
    private PictureBox Icon80;
    private PictureBox Icon81;
    private PictureBox Icon82;
    private PictureBox Icon83;
    private PictureBox Icon84;
    private PictureBox Icon85;
    private PictureBox Icon86;
    private PictureBox Icon87;
    private PictureBox Icon88;
    private PictureBox Icon89;
    private PictureBox Icon9;
    private PictureBox Icon90;
    private PictureBox Icon91;
    private PictureBox Icon92;
    private PictureBox Icon93;
    private PictureBox Icon94;
    private PictureBox Icon95;
    private PictureBox Icon96;
    private PictureBox Icon97;
    private PictureBox Icon98;
    private PictureBox Icon99;
    private bool IconsChanged;
    private Label lblCurrentIcon;
    private Label lblEditIndex;
    private const int NumChars = 0x100;
    private Panel panel1;

    public iMONLCDg_FontEdit()
    {
      InitializeComponent();
    }

    private static void ClearFontBuffer()
    {
      for (int i = 0; i < 0x100; i++)
      {
        for (int j = 0; j < 6; j++)
        {
          _FontBuffer[i, j] = 0;
        }
      }
    }

    private void ClearIconDisplay()
    {
      ClearFontBuffer();
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
      for (int i = 0; i < 6; i++)
      {
        for (int j = 0; j < 8; j++)
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
      for (int i = 0; i < 6; i++)
      {
        for (int j = 0; j < 8; j++)
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
      LoadCustomFont();
      EnableIconSelection(true);
    }

    private void cmdLoadInternal_Click(object sender, EventArgs e)
    {
      LoadInteralFont();
      EnableIconSelection(true);
    }

    private void cmdSave_Click(object sender, EventArgs e)
    {
      if (IconsChanged)
      {
        var o = new DataTable("Character");
        var column = new DataColumn("CharID");
        var column2 = new DataColumn("CData0");
        var column3 = new DataColumn("CData1");
        var column4 = new DataColumn("CData2");
        var column5 = new DataColumn("CData3");
        var column6 = new DataColumn("CData4");
        var column7 = new DataColumn("CData5");
        o.Rows.Clear();
        o.Columns.Clear();
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
        o.Clear();
        try
        {
          for (int i = 0; i < 0x100; i++)
          {
            DataRow row = o.NewRow();
            row[0] = i;
            for (int j = 0; j < 6; j++)
            {
              row[j + 1] = _FontBuffer[i, j];
            }
            o.Rows.Add(row);
          }
          var serializer = new XmlSerializer(typeof (DataTable));
          TextWriter textWriter = new StreamWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_font.xml"));
          serializer.Serialize(textWriter, o);
          textWriter.Close();
        }
        catch (Exception exception)
        {
          Log.Debug("CAUGHT EXCEPTION: {0}", new object[] {exception});
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
        for (int i = 0; i < 6; i++)
        {
          byte num2 = 0;
          for (int j = 0; j < 8; j++)
          {
            num2 =
              (byte)
              (num2 |
               ((byte)
                (((GetEditPixel(i, j) == CheckState.Indeterminate) ? (1) : (0)) *
                 Math.Pow(2.0, (7 - j)))));
          }
          _FontBuffer[EditIndex, i] = num2;
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
      for (int i = 0; i < 6; i++)
      {
        for (int j = 0; j < 8; j++)
        {
          SetEditPixel(i, j, true);
        }
      }
    }

    public void CopyBufferToGraphics()
    {
      for (int i = 0; i < NumChars; i++)
      {
        if (IconGraphics[i] == null)
        {
          IconGraphics[i] = new Bitmap(12, 0x10);
        }
        for (int j = 0; j < 6; j++)
        {
          for (int k = 0; k < 8; k++)
          {
            var num4 = (int)Math.Pow(2.0, k);
            bool flag = (_FontBuffer[i, j] & num4) > 0;
            int x = j * 2;
            int y = k * 2;
            Color black = flag ? Color.Black : Color.White;
            IconGraphics[i].SetPixel(x, y, black);
            IconGraphics[i].SetPixel(x + 1, y, black);
            IconGraphics[i].SetPixel(x, y + 1, black);
            IconGraphics[i].SetPixel(x + 1, y + 1, black);
          }
        }
        string key = "Icon" + i.ToString().Trim();
        Control[] controlArray = Controls.Find(key, false);
        if (controlArray.Length > 0)
        {
          var box = (PictureBox)controlArray[0];
          box.Image = IconGraphics[i];
        }
        else
        {
          Log.Debug("Could not find control \"{0}\"", new object[] {key});
        }
      }
    }

    private void DisplayIconForEditing(int IconIndex)
    {
      for (int i = 0; i < 6; i++)
      {
        for (int j = 0; j < 8; j++)
        {
          var num3 = (int)Math.Pow(2.0, (7 - j));
          bool setOn = (_FontBuffer[IconIndex, i] & num3) > 0;
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
      for (int i = 0; i < NumChars; i++)
      {
        string key = "Icon" + i.ToString().Trim();
        Control[] controlArray = Controls.Find(key, false);
        if (controlArray.Length > 0)
        {
          var box = (PictureBox)controlArray[0];
          box.Enabled = Enable;
          if (!Enable)
          {
            box.Image = null;
          }
        }
      }
    }

    private CheckState GetEditPixel(int Column, int Row)
    {
      string key = "C" + Column.ToString().Trim() + "_B" + Row.ToString().Trim();
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
        int charIndex = int.Parse(box.Name.Substring(4));
        SetIconEdit(charIndex);
      }
      catch (Exception exception)
      {
        Log.Debug("CAUGHT EXCEPTION: {0}", new object[] {exception});
      }
    }

    private void InitializeComponent()
    {
      panel1 = new Panel();
      lblEditIndex = new Label();
      cmdSaveEdit = new Button();
      cmdCancelEdit = new Button();
      cmdInvert = new Button();
      cmdSetAll = new Button();
      cmdClearAll = new Button();
      lblCurrentIcon = new Label();
      C4_B0 = new CheckBox();
      C4_B1 = new CheckBox();
      C4_B2 = new CheckBox();
      C4_B3 = new CheckBox();
      C4_B4 = new CheckBox();
      C4_B5 = new CheckBox();
      C4_B6 = new CheckBox();
      C4_B7 = new CheckBox();
      C5_B0 = new CheckBox();
      C5_B1 = new CheckBox();
      C5_B2 = new CheckBox();
      C5_B3 = new CheckBox();
      C5_B4 = new CheckBox();
      C5_B5 = new CheckBox();
      C5_B6 = new CheckBox();
      C5_B7 = new CheckBox();
      C2_B0 = new CheckBox();
      C2_B1 = new CheckBox();
      C2_B2 = new CheckBox();
      C2_B3 = new CheckBox();
      C2_B4 = new CheckBox();
      C2_B5 = new CheckBox();
      C2_B6 = new CheckBox();
      C2_B7 = new CheckBox();
      C3_B0 = new CheckBox();
      C3_B1 = new CheckBox();
      C3_B2 = new CheckBox();
      C3_B3 = new CheckBox();
      C3_B4 = new CheckBox();
      C3_B5 = new CheckBox();
      C3_B6 = new CheckBox();
      C3_B7 = new CheckBox();
      C0_B0 = new CheckBox();
      C0_B1 = new CheckBox();
      C0_B2 = new CheckBox();
      C0_B3 = new CheckBox();
      C0_B4 = new CheckBox();
      C0_B5 = new CheckBox();
      C0_B6 = new CheckBox();
      C0_B7 = new CheckBox();
      C1_B0 = new CheckBox();
      C1_B1 = new CheckBox();
      C1_B2 = new CheckBox();
      C1_B3 = new CheckBox();
      C1_B4 = new CheckBox();
      C1_B5 = new CheckBox();
      C1_B6 = new CheckBox();
      C1_B7 = new CheckBox();
      cmdLoadInternal = new Button();
      cmdLoadCustom = new Button();
      cmdSave = new Button();
      cmdExit = new Button();
      Icon0 = new PictureBox();
      Icon1 = new PictureBox();
      Icon2 = new PictureBox();
      Icon3 = new PictureBox();
      Icon4 = new PictureBox();
      Icon5 = new PictureBox();
      Icon6 = new PictureBox();
      Icon7 = new PictureBox();
      Icon8 = new PictureBox();
      Icon9 = new PictureBox();
      Icon10 = new PictureBox();
      Icon11 = new PictureBox();
      Icon12 = new PictureBox();
      Icon13 = new PictureBox();
      Icon14 = new PictureBox();
      Icon15 = new PictureBox();
      Icon16 = new PictureBox();
      Icon17 = new PictureBox();
      Icon18 = new PictureBox();
      Icon19 = new PictureBox();
      Icon20 = new PictureBox();
      Icon21 = new PictureBox();
      Icon22 = new PictureBox();
      Icon23 = new PictureBox();
      Icon24 = new PictureBox();
      Icon25 = new PictureBox();
      Icon26 = new PictureBox();
      Icon27 = new PictureBox();
      Icon28 = new PictureBox();
      Icon29 = new PictureBox();
      Icon30 = new PictureBox();
      Icon31 = new PictureBox();
      Icon32 = new PictureBox();
      Icon33 = new PictureBox();
      Icon34 = new PictureBox();
      Icon35 = new PictureBox();
      Icon36 = new PictureBox();
      Icon37 = new PictureBox();
      Icon38 = new PictureBox();
      Icon39 = new PictureBox();
      Icon40 = new PictureBox();
      Icon41 = new PictureBox();
      Icon42 = new PictureBox();
      Icon43 = new PictureBox();
      Icon44 = new PictureBox();
      Icon45 = new PictureBox();
      Icon46 = new PictureBox();
      Icon47 = new PictureBox();
      Icon48 = new PictureBox();
      Icon49 = new PictureBox();
      Icon50 = new PictureBox();
      Icon51 = new PictureBox();
      Icon52 = new PictureBox();
      Icon53 = new PictureBox();
      Icon54 = new PictureBox();
      Icon55 = new PictureBox();
      Icon56 = new PictureBox();
      Icon57 = new PictureBox();
      Icon58 = new PictureBox();
      Icon59 = new PictureBox();
      Icon60 = new PictureBox();
      Icon61 = new PictureBox();
      Icon62 = new PictureBox();
      Icon63 = new PictureBox();
      Icon64 = new PictureBox();
      Icon65 = new PictureBox();
      Icon66 = new PictureBox();
      Icon67 = new PictureBox();
      Icon68 = new PictureBox();
      Icon69 = new PictureBox();
      Icon70 = new PictureBox();
      Icon71 = new PictureBox();
      Icon72 = new PictureBox();
      Icon73 = new PictureBox();
      Icon74 = new PictureBox();
      Icon75 = new PictureBox();
      Icon76 = new PictureBox();
      Icon77 = new PictureBox();
      Icon78 = new PictureBox();
      Icon79 = new PictureBox();
      Icon80 = new PictureBox();
      Icon81 = new PictureBox();
      Icon82 = new PictureBox();
      Icon83 = new PictureBox();
      Icon84 = new PictureBox();
      Icon85 = new PictureBox();
      Icon86 = new PictureBox();
      Icon87 = new PictureBox();
      Icon88 = new PictureBox();
      Icon89 = new PictureBox();
      Icon90 = new PictureBox();
      Icon91 = new PictureBox();
      Icon92 = new PictureBox();
      Icon93 = new PictureBox();
      Icon94 = new PictureBox();
      Icon95 = new PictureBox();
      Icon96 = new PictureBox();
      Icon97 = new PictureBox();
      Icon98 = new PictureBox();
      Icon99 = new PictureBox();
      Icon100 = new PictureBox();
      Icon101 = new PictureBox();
      Icon102 = new PictureBox();
      Icon103 = new PictureBox();
      Icon104 = new PictureBox();
      Icon105 = new PictureBox();
      Icon106 = new PictureBox();
      Icon107 = new PictureBox();
      Icon108 = new PictureBox();
      Icon109 = new PictureBox();
      Icon110 = new PictureBox();
      Icon111 = new PictureBox();
      Icon112 = new PictureBox();
      Icon113 = new PictureBox();
      Icon114 = new PictureBox();
      Icon115 = new PictureBox();
      Icon116 = new PictureBox();
      Icon117 = new PictureBox();
      Icon118 = new PictureBox();
      Icon119 = new PictureBox();
      Icon120 = new PictureBox();
      Icon121 = new PictureBox();
      Icon122 = new PictureBox();
      Icon123 = new PictureBox();
      Icon124 = new PictureBox();
      Icon125 = new PictureBox();
      Icon126 = new PictureBox();
      Icon127 = new PictureBox();
      Icon128 = new PictureBox();
      Icon129 = new PictureBox();
      Icon130 = new PictureBox();
      Icon131 = new PictureBox();
      Icon132 = new PictureBox();
      Icon133 = new PictureBox();
      Icon134 = new PictureBox();
      Icon135 = new PictureBox();
      Icon136 = new PictureBox();
      Icon137 = new PictureBox();
      Icon138 = new PictureBox();
      Icon139 = new PictureBox();
      Icon140 = new PictureBox();
      Icon141 = new PictureBox();
      Icon142 = new PictureBox();
      Icon143 = new PictureBox();
      Icon144 = new PictureBox();
      Icon145 = new PictureBox();
      Icon146 = new PictureBox();
      Icon147 = new PictureBox();
      Icon148 = new PictureBox();
      Icon149 = new PictureBox();
      Icon150 = new PictureBox();
      Icon151 = new PictureBox();
      Icon152 = new PictureBox();
      Icon153 = new PictureBox();
      Icon154 = new PictureBox();
      Icon155 = new PictureBox();
      Icon156 = new PictureBox();
      Icon157 = new PictureBox();
      Icon158 = new PictureBox();
      Icon159 = new PictureBox();
      Icon160 = new PictureBox();
      Icon161 = new PictureBox();
      Icon162 = new PictureBox();
      Icon163 = new PictureBox();
      Icon164 = new PictureBox();
      Icon165 = new PictureBox();
      Icon166 = new PictureBox();
      Icon167 = new PictureBox();
      Icon168 = new PictureBox();
      Icon169 = new PictureBox();
      Icon170 = new PictureBox();
      Icon171 = new PictureBox();
      Icon172 = new PictureBox();
      Icon173 = new PictureBox();
      Icon174 = new PictureBox();
      Icon175 = new PictureBox();
      Icon176 = new PictureBox();
      Icon177 = new PictureBox();
      Icon178 = new PictureBox();
      Icon179 = new PictureBox();
      Icon180 = new PictureBox();
      Icon181 = new PictureBox();
      Icon182 = new PictureBox();
      Icon183 = new PictureBox();
      Icon184 = new PictureBox();
      Icon185 = new PictureBox();
      Icon186 = new PictureBox();
      Icon187 = new PictureBox();
      Icon188 = new PictureBox();
      Icon189 = new PictureBox();
      Icon190 = new PictureBox();
      Icon191 = new PictureBox();
      Icon192 = new PictureBox();
      Icon193 = new PictureBox();
      Icon194 = new PictureBox();
      Icon195 = new PictureBox();
      Icon196 = new PictureBox();
      Icon197 = new PictureBox();
      Icon198 = new PictureBox();
      Icon199 = new PictureBox();
      Icon200 = new PictureBox();
      Icon201 = new PictureBox();
      Icon202 = new PictureBox();
      Icon203 = new PictureBox();
      Icon204 = new PictureBox();
      Icon205 = new PictureBox();
      Icon206 = new PictureBox();
      Icon207 = new PictureBox();
      Icon208 = new PictureBox();
      Icon209 = new PictureBox();
      Icon210 = new PictureBox();
      Icon211 = new PictureBox();
      Icon212 = new PictureBox();
      Icon213 = new PictureBox();
      Icon214 = new PictureBox();
      Icon215 = new PictureBox();
      Icon216 = new PictureBox();
      Icon217 = new PictureBox();
      Icon218 = new PictureBox();
      Icon219 = new PictureBox();
      Icon220 = new PictureBox();
      Icon221 = new PictureBox();
      Icon222 = new PictureBox();
      Icon223 = new PictureBox();
      Icon224 = new PictureBox();
      Icon225 = new PictureBox();
      Icon226 = new PictureBox();
      Icon227 = new PictureBox();
      Icon228 = new PictureBox();
      Icon229 = new PictureBox();
      Icon230 = new PictureBox();
      Icon231 = new PictureBox();
      Icon232 = new PictureBox();
      Icon233 = new PictureBox();
      Icon234 = new PictureBox();
      Icon235 = new PictureBox();
      Icon236 = new PictureBox();
      Icon237 = new PictureBox();
      Icon238 = new PictureBox();
      Icon239 = new PictureBox();
      Icon240 = new PictureBox();
      Icon241 = new PictureBox();
      Icon242 = new PictureBox();
      Icon243 = new PictureBox();
      Icon244 = new PictureBox();
      Icon245 = new PictureBox();
      Icon246 = new PictureBox();
      Icon247 = new PictureBox();
      Icon248 = new PictureBox();
      Icon249 = new PictureBox();
      Icon250 = new PictureBox();
      Icon251 = new PictureBox();
      Icon252 = new PictureBox();
      Icon253 = new PictureBox();
      Icon254 = new PictureBox();
      Icon255 = new PictureBox();
      panel1.SuspendLayout();
      ((ISupportInitialize)(Icon0)).BeginInit();
      ((ISupportInitialize)(Icon1)).BeginInit();
      ((ISupportInitialize)(Icon2)).BeginInit();
      ((ISupportInitialize)(Icon3)).BeginInit();
      ((ISupportInitialize)(Icon4)).BeginInit();
      ((ISupportInitialize)(Icon5)).BeginInit();
      ((ISupportInitialize)(Icon6)).BeginInit();
      ((ISupportInitialize)(Icon7)).BeginInit();
      ((ISupportInitialize)(Icon8)).BeginInit();
      ((ISupportInitialize)(Icon9)).BeginInit();
      ((ISupportInitialize)(Icon10)).BeginInit();
      ((ISupportInitialize)(Icon11)).BeginInit();
      ((ISupportInitialize)(Icon12)).BeginInit();
      ((ISupportInitialize)(Icon13)).BeginInit();
      ((ISupportInitialize)(Icon14)).BeginInit();
      ((ISupportInitialize)(Icon15)).BeginInit();
      ((ISupportInitialize)(Icon16)).BeginInit();
      ((ISupportInitialize)(Icon17)).BeginInit();
      ((ISupportInitialize)(Icon18)).BeginInit();
      ((ISupportInitialize)(Icon19)).BeginInit();
      ((ISupportInitialize)(Icon20)).BeginInit();
      ((ISupportInitialize)(Icon21)).BeginInit();
      ((ISupportInitialize)(Icon22)).BeginInit();
      ((ISupportInitialize)(Icon23)).BeginInit();
      ((ISupportInitialize)(Icon24)).BeginInit();
      ((ISupportInitialize)(Icon25)).BeginInit();
      ((ISupportInitialize)(Icon26)).BeginInit();
      ((ISupportInitialize)(Icon27)).BeginInit();
      ((ISupportInitialize)(Icon28)).BeginInit();
      ((ISupportInitialize)(Icon29)).BeginInit();
      ((ISupportInitialize)(Icon30)).BeginInit();
      ((ISupportInitialize)(Icon31)).BeginInit();
      ((ISupportInitialize)(Icon32)).BeginInit();
      ((ISupportInitialize)(Icon33)).BeginInit();
      ((ISupportInitialize)(Icon34)).BeginInit();
      ((ISupportInitialize)(Icon35)).BeginInit();
      ((ISupportInitialize)(Icon36)).BeginInit();
      ((ISupportInitialize)(Icon37)).BeginInit();
      ((ISupportInitialize)(Icon38)).BeginInit();
      ((ISupportInitialize)(Icon39)).BeginInit();
      ((ISupportInitialize)(Icon40)).BeginInit();
      ((ISupportInitialize)(Icon41)).BeginInit();
      ((ISupportInitialize)(Icon42)).BeginInit();
      ((ISupportInitialize)(Icon43)).BeginInit();
      ((ISupportInitialize)(Icon44)).BeginInit();
      ((ISupportInitialize)(Icon45)).BeginInit();
      ((ISupportInitialize)(Icon46)).BeginInit();
      ((ISupportInitialize)(Icon47)).BeginInit();
      ((ISupportInitialize)(Icon48)).BeginInit();
      ((ISupportInitialize)(Icon49)).BeginInit();
      ((ISupportInitialize)(Icon50)).BeginInit();
      ((ISupportInitialize)(Icon51)).BeginInit();
      ((ISupportInitialize)(Icon52)).BeginInit();
      ((ISupportInitialize)(Icon53)).BeginInit();
      ((ISupportInitialize)(Icon54)).BeginInit();
      ((ISupportInitialize)(Icon55)).BeginInit();
      ((ISupportInitialize)(Icon56)).BeginInit();
      ((ISupportInitialize)(Icon57)).BeginInit();
      ((ISupportInitialize)(Icon58)).BeginInit();
      ((ISupportInitialize)(Icon59)).BeginInit();
      ((ISupportInitialize)(Icon60)).BeginInit();
      ((ISupportInitialize)(Icon61)).BeginInit();
      ((ISupportInitialize)(Icon62)).BeginInit();
      ((ISupportInitialize)(Icon63)).BeginInit();
      ((ISupportInitialize)(Icon64)).BeginInit();
      ((ISupportInitialize)(Icon65)).BeginInit();
      ((ISupportInitialize)(Icon66)).BeginInit();
      ((ISupportInitialize)(Icon67)).BeginInit();
      ((ISupportInitialize)(Icon68)).BeginInit();
      ((ISupportInitialize)(Icon69)).BeginInit();
      ((ISupportInitialize)(Icon70)).BeginInit();
      ((ISupportInitialize)(Icon71)).BeginInit();
      ((ISupportInitialize)(Icon72)).BeginInit();
      ((ISupportInitialize)(Icon73)).BeginInit();
      ((ISupportInitialize)(Icon74)).BeginInit();
      ((ISupportInitialize)(Icon75)).BeginInit();
      ((ISupportInitialize)(Icon76)).BeginInit();
      ((ISupportInitialize)(Icon77)).BeginInit();
      ((ISupportInitialize)(Icon78)).BeginInit();
      ((ISupportInitialize)(Icon79)).BeginInit();
      ((ISupportInitialize)(Icon80)).BeginInit();
      ((ISupportInitialize)(Icon81)).BeginInit();
      ((ISupportInitialize)(Icon82)).BeginInit();
      ((ISupportInitialize)(Icon83)).BeginInit();
      ((ISupportInitialize)(Icon84)).BeginInit();
      ((ISupportInitialize)(Icon85)).BeginInit();
      ((ISupportInitialize)(Icon86)).BeginInit();
      ((ISupportInitialize)(Icon87)).BeginInit();
      ((ISupportInitialize)(Icon88)).BeginInit();
      ((ISupportInitialize)(Icon89)).BeginInit();
      ((ISupportInitialize)(Icon90)).BeginInit();
      ((ISupportInitialize)(Icon91)).BeginInit();
      ((ISupportInitialize)(Icon92)).BeginInit();
      ((ISupportInitialize)(Icon93)).BeginInit();
      ((ISupportInitialize)(Icon94)).BeginInit();
      ((ISupportInitialize)(Icon95)).BeginInit();
      ((ISupportInitialize)(Icon96)).BeginInit();
      ((ISupportInitialize)(Icon97)).BeginInit();
      ((ISupportInitialize)(Icon98)).BeginInit();
      ((ISupportInitialize)(Icon99)).BeginInit();
      ((ISupportInitialize)(Icon100)).BeginInit();
      ((ISupportInitialize)(Icon101)).BeginInit();
      ((ISupportInitialize)(Icon102)).BeginInit();
      ((ISupportInitialize)(Icon103)).BeginInit();
      ((ISupportInitialize)(Icon104)).BeginInit();
      ((ISupportInitialize)(Icon105)).BeginInit();
      ((ISupportInitialize)(Icon106)).BeginInit();
      ((ISupportInitialize)(Icon107)).BeginInit();
      ((ISupportInitialize)(Icon108)).BeginInit();
      ((ISupportInitialize)(Icon109)).BeginInit();
      ((ISupportInitialize)(Icon110)).BeginInit();
      ((ISupportInitialize)(Icon111)).BeginInit();
      ((ISupportInitialize)(Icon112)).BeginInit();
      ((ISupportInitialize)(Icon113)).BeginInit();
      ((ISupportInitialize)(Icon114)).BeginInit();
      ((ISupportInitialize)(Icon115)).BeginInit();
      ((ISupportInitialize)(Icon116)).BeginInit();
      ((ISupportInitialize)(Icon117)).BeginInit();
      ((ISupportInitialize)(Icon118)).BeginInit();
      ((ISupportInitialize)(Icon119)).BeginInit();
      ((ISupportInitialize)(Icon120)).BeginInit();
      ((ISupportInitialize)(Icon121)).BeginInit();
      ((ISupportInitialize)(Icon122)).BeginInit();
      ((ISupportInitialize)(Icon123)).BeginInit();
      ((ISupportInitialize)(Icon124)).BeginInit();
      ((ISupportInitialize)(Icon125)).BeginInit();
      ((ISupportInitialize)(Icon126)).BeginInit();
      ((ISupportInitialize)(Icon127)).BeginInit();
      ((ISupportInitialize)(Icon128)).BeginInit();
      ((ISupportInitialize)(Icon129)).BeginInit();
      ((ISupportInitialize)(Icon130)).BeginInit();
      ((ISupportInitialize)(Icon131)).BeginInit();
      ((ISupportInitialize)(Icon132)).BeginInit();
      ((ISupportInitialize)(Icon133)).BeginInit();
      ((ISupportInitialize)(Icon134)).BeginInit();
      ((ISupportInitialize)(Icon135)).BeginInit();
      ((ISupportInitialize)(Icon136)).BeginInit();
      ((ISupportInitialize)(Icon137)).BeginInit();
      ((ISupportInitialize)(Icon138)).BeginInit();
      ((ISupportInitialize)(Icon139)).BeginInit();
      ((ISupportInitialize)(Icon140)).BeginInit();
      ((ISupportInitialize)(Icon141)).BeginInit();
      ((ISupportInitialize)(Icon142)).BeginInit();
      ((ISupportInitialize)(Icon143)).BeginInit();
      ((ISupportInitialize)(Icon144)).BeginInit();
      ((ISupportInitialize)(Icon145)).BeginInit();
      ((ISupportInitialize)(Icon146)).BeginInit();
      ((ISupportInitialize)(Icon147)).BeginInit();
      ((ISupportInitialize)(Icon148)).BeginInit();
      ((ISupportInitialize)(Icon149)).BeginInit();
      ((ISupportInitialize)(Icon150)).BeginInit();
      ((ISupportInitialize)(Icon151)).BeginInit();
      ((ISupportInitialize)(Icon152)).BeginInit();
      ((ISupportInitialize)(Icon153)).BeginInit();
      ((ISupportInitialize)(Icon154)).BeginInit();
      ((ISupportInitialize)(Icon155)).BeginInit();
      ((ISupportInitialize)(Icon156)).BeginInit();
      ((ISupportInitialize)(Icon157)).BeginInit();
      ((ISupportInitialize)(Icon158)).BeginInit();
      ((ISupportInitialize)(Icon159)).BeginInit();
      ((ISupportInitialize)(Icon160)).BeginInit();
      ((ISupportInitialize)(Icon161)).BeginInit();
      ((ISupportInitialize)(Icon162)).BeginInit();
      ((ISupportInitialize)(Icon163)).BeginInit();
      ((ISupportInitialize)(Icon164)).BeginInit();
      ((ISupportInitialize)(Icon165)).BeginInit();
      ((ISupportInitialize)(Icon166)).BeginInit();
      ((ISupportInitialize)(Icon167)).BeginInit();
      ((ISupportInitialize)(Icon168)).BeginInit();
      ((ISupportInitialize)(Icon169)).BeginInit();
      ((ISupportInitialize)(Icon170)).BeginInit();
      ((ISupportInitialize)(Icon171)).BeginInit();
      ((ISupportInitialize)(Icon172)).BeginInit();
      ((ISupportInitialize)(Icon173)).BeginInit();
      ((ISupportInitialize)(Icon174)).BeginInit();
      ((ISupportInitialize)(Icon175)).BeginInit();
      ((ISupportInitialize)(Icon176)).BeginInit();
      ((ISupportInitialize)(Icon177)).BeginInit();
      ((ISupportInitialize)(Icon178)).BeginInit();
      ((ISupportInitialize)(Icon179)).BeginInit();
      ((ISupportInitialize)(Icon180)).BeginInit();
      ((ISupportInitialize)(Icon181)).BeginInit();
      ((ISupportInitialize)(Icon182)).BeginInit();
      ((ISupportInitialize)(Icon183)).BeginInit();
      ((ISupportInitialize)(Icon184)).BeginInit();
      ((ISupportInitialize)(Icon185)).BeginInit();
      ((ISupportInitialize)(Icon186)).BeginInit();
      ((ISupportInitialize)(Icon187)).BeginInit();
      ((ISupportInitialize)(Icon188)).BeginInit();
      ((ISupportInitialize)(Icon189)).BeginInit();
      ((ISupportInitialize)(Icon190)).BeginInit();
      ((ISupportInitialize)(Icon191)).BeginInit();
      ((ISupportInitialize)(Icon192)).BeginInit();
      ((ISupportInitialize)(Icon193)).BeginInit();
      ((ISupportInitialize)(Icon194)).BeginInit();
      ((ISupportInitialize)(Icon195)).BeginInit();
      ((ISupportInitialize)(Icon196)).BeginInit();
      ((ISupportInitialize)(Icon197)).BeginInit();
      ((ISupportInitialize)(Icon198)).BeginInit();
      ((ISupportInitialize)(Icon199)).BeginInit();
      ((ISupportInitialize)(Icon200)).BeginInit();
      ((ISupportInitialize)(Icon201)).BeginInit();
      ((ISupportInitialize)(Icon202)).BeginInit();
      ((ISupportInitialize)(Icon203)).BeginInit();
      ((ISupportInitialize)(Icon204)).BeginInit();
      ((ISupportInitialize)(Icon205)).BeginInit();
      ((ISupportInitialize)(Icon206)).BeginInit();
      ((ISupportInitialize)(Icon207)).BeginInit();
      ((ISupportInitialize)(Icon208)).BeginInit();
      ((ISupportInitialize)(Icon209)).BeginInit();
      ((ISupportInitialize)(Icon210)).BeginInit();
      ((ISupportInitialize)(Icon211)).BeginInit();
      ((ISupportInitialize)(Icon212)).BeginInit();
      ((ISupportInitialize)(Icon213)).BeginInit();
      ((ISupportInitialize)(Icon214)).BeginInit();
      ((ISupportInitialize)(Icon215)).BeginInit();
      ((ISupportInitialize)(Icon216)).BeginInit();
      ((ISupportInitialize)(Icon217)).BeginInit();
      ((ISupportInitialize)(Icon218)).BeginInit();
      ((ISupportInitialize)(Icon219)).BeginInit();
      ((ISupportInitialize)(Icon220)).BeginInit();
      ((ISupportInitialize)(Icon221)).BeginInit();
      ((ISupportInitialize)(Icon222)).BeginInit();
      ((ISupportInitialize)(Icon223)).BeginInit();
      ((ISupportInitialize)(Icon224)).BeginInit();
      ((ISupportInitialize)(Icon225)).BeginInit();
      ((ISupportInitialize)(Icon226)).BeginInit();
      ((ISupportInitialize)(Icon227)).BeginInit();
      ((ISupportInitialize)(Icon228)).BeginInit();
      ((ISupportInitialize)(Icon229)).BeginInit();
      ((ISupportInitialize)(Icon230)).BeginInit();
      ((ISupportInitialize)(Icon231)).BeginInit();
      ((ISupportInitialize)(Icon232)).BeginInit();
      ((ISupportInitialize)(Icon233)).BeginInit();
      ((ISupportInitialize)(Icon234)).BeginInit();
      ((ISupportInitialize)(Icon235)).BeginInit();
      ((ISupportInitialize)(Icon236)).BeginInit();
      ((ISupportInitialize)(Icon237)).BeginInit();
      ((ISupportInitialize)(Icon238)).BeginInit();
      ((ISupportInitialize)(Icon239)).BeginInit();
      ((ISupportInitialize)(Icon240)).BeginInit();
      ((ISupportInitialize)(Icon241)).BeginInit();
      ((ISupportInitialize)(Icon242)).BeginInit();
      ((ISupportInitialize)(Icon243)).BeginInit();
      ((ISupportInitialize)(Icon244)).BeginInit();
      ((ISupportInitialize)(Icon245)).BeginInit();
      ((ISupportInitialize)(Icon246)).BeginInit();
      ((ISupportInitialize)(Icon247)).BeginInit();
      ((ISupportInitialize)(Icon248)).BeginInit();
      ((ISupportInitialize)(Icon249)).BeginInit();
      ((ISupportInitialize)(Icon250)).BeginInit();
      ((ISupportInitialize)(Icon251)).BeginInit();
      ((ISupportInitialize)(Icon252)).BeginInit();
      ((ISupportInitialize)(Icon253)).BeginInit();
      ((ISupportInitialize)(Icon254)).BeginInit();
      ((ISupportInitialize)(Icon255)).BeginInit();
      SuspendLayout();
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
      panel1.Controls.Add(C4_B0);
      panel1.Controls.Add(C4_B1);
      panel1.Controls.Add(C4_B2);
      panel1.Controls.Add(C4_B3);
      panel1.Controls.Add(C4_B4);
      panel1.Controls.Add(C4_B5);
      panel1.Controls.Add(C4_B6);
      panel1.Controls.Add(C4_B7);
      panel1.Controls.Add(C5_B0);
      panel1.Controls.Add(C5_B1);
      panel1.Controls.Add(C5_B2);
      panel1.Controls.Add(C5_B3);
      panel1.Controls.Add(C5_B4);
      panel1.Controls.Add(C5_B5);
      panel1.Controls.Add(C5_B6);
      panel1.Controls.Add(C5_B7);
      panel1.Controls.Add(C2_B0);
      panel1.Controls.Add(C2_B1);
      panel1.Controls.Add(C2_B2);
      panel1.Controls.Add(C2_B3);
      panel1.Controls.Add(C2_B4);
      panel1.Controls.Add(C2_B5);
      panel1.Controls.Add(C2_B6);
      panel1.Controls.Add(C2_B7);
      panel1.Controls.Add(C3_B0);
      panel1.Controls.Add(C3_B1);
      panel1.Controls.Add(C3_B2);
      panel1.Controls.Add(C3_B3);
      panel1.Controls.Add(C3_B4);
      panel1.Controls.Add(C3_B5);
      panel1.Controls.Add(C3_B6);
      panel1.Controls.Add(C3_B7);
      panel1.Controls.Add(C0_B0);
      panel1.Controls.Add(C0_B1);
      panel1.Controls.Add(C0_B2);
      panel1.Controls.Add(C0_B3);
      panel1.Controls.Add(C0_B4);
      panel1.Controls.Add(C0_B5);
      panel1.Controls.Add(C0_B6);
      panel1.Controls.Add(C0_B7);
      panel1.Controls.Add(C1_B0);
      panel1.Controls.Add(C1_B1);
      panel1.Controls.Add(C1_B2);
      panel1.Controls.Add(C1_B3);
      panel1.Controls.Add(C1_B4);
      panel1.Controls.Add(C1_B5);
      panel1.Controls.Add(C1_B6);
      panel1.Controls.Add(C1_B7);
      panel1.Enabled = false;
      panel1.Location = new Point(6, 7);
      panel1.Name = "panel1";
      panel1.Size = new Size(233, 144);
      panel1.TabIndex = 288;
      // 
      // lblEditIndex
      // 
      lblEditIndex.AutoSize = true;
      lblEditIndex.Location = new Point(95, 76);
      lblEditIndex.Name = "lblEditIndex";
      lblEditIndex.Size = new Size(61, 13);
      lblEditIndex.TabIndex = 550;
      lblEditIndex.Text = "lblEditIndex";
      lblEditIndex.Visible = false;
      // 
      // cmdSaveEdit
      // 
      cmdSaveEdit.Enabled = false;
      cmdSaveEdit.Location = new Point(170, 3);
      cmdSaveEdit.Name = "cmdSaveEdit";
      cmdSaveEdit.Size = new Size(58, 23);
      cmdSaveEdit.TabIndex = 549;
      cmdSaveEdit.Text = "Save";
      cmdSaveEdit.UseVisualStyleBackColor = true;
      cmdSaveEdit.Click += cmdSaveEdit_Click;
      // 
      // cmdCancelEdit
      // 
      cmdCancelEdit.Enabled = false;
      cmdCancelEdit.Location = new Point(170, 27);
      cmdCancelEdit.Name = "cmdCancelEdit";
      cmdCancelEdit.Size = new Size(58, 23);
      cmdCancelEdit.TabIndex = 548;
      cmdCancelEdit.Text = "Cancel";
      cmdCancelEdit.UseVisualStyleBackColor = true;
      cmdCancelEdit.Click += cmdCancelEdit_Click;
      // 
      // cmdInvert
      // 
      cmdInvert.Enabled = false;
      cmdInvert.Location = new Point(87, 49);
      cmdInvert.Name = "cmdInvert";
      cmdInvert.Size = new Size(58, 23);
      cmdInvert.TabIndex = 547;
      cmdInvert.Text = "Invert";
      cmdInvert.UseVisualStyleBackColor = true;
      cmdInvert.Click += cmdInvert_Click;
      // 
      // cmdSetAll
      // 
      cmdSetAll.Enabled = false;
      cmdSetAll.Location = new Point(87, 26);
      cmdSetAll.Name = "cmdSetAll";
      cmdSetAll.Size = new Size(58, 23);
      cmdSetAll.TabIndex = 546;
      cmdSetAll.Text = "Set All";
      cmdSetAll.UseVisualStyleBackColor = true;
      cmdSetAll.Click += cmdSetAll_Click;
      // 
      // cmdClearAll
      // 
      cmdClearAll.Enabled = false;
      cmdClearAll.Location = new Point(87, 3);
      cmdClearAll.Name = "cmdClearAll";
      cmdClearAll.Size = new Size(58, 23);
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
      lblCurrentIcon.Location = new Point(3, 104);
      lblCurrentIcon.Name = "lblCurrentIcon";
      lblCurrentIcon.Size = new Size(225, 33);
      lblCurrentIcon.TabIndex = 544;
      lblCurrentIcon.TextAlign = ContentAlignment.MiddleCenter;
      // 
      // C4_B0
      // 
      C4_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C4_B0.Location = new Point(53, 87);
      C4_B0.Name = "C4_B0";
      C4_B0.Size = new Size(14, 14);
      C4_B0.TabIndex = 335;
      C4_B0.TextAlign = ContentAlignment.MiddleCenter;
      C4_B0.ThreeState = true;
      C4_B0.UseVisualStyleBackColor = true;
      C4_B0.Click += Pixel_Click;
      // 
      // C4_B1
      // 
      C4_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C4_B1.Location = new Point(53, 75);
      C4_B1.Name = "C4_B1";
      C4_B1.Size = new Size(14, 14);
      C4_B1.TabIndex = 334;
      C4_B1.TextAlign = ContentAlignment.MiddleCenter;
      C4_B1.ThreeState = true;
      C4_B1.UseVisualStyleBackColor = true;
      C4_B1.Click += Pixel_Click;
      // 
      // C4_B2
      // 
      C4_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C4_B2.Location = new Point(53, 63);
      C4_B2.Name = "C4_B2";
      C4_B2.Size = new Size(14, 14);
      C4_B2.TabIndex = 333;
      C4_B2.TextAlign = ContentAlignment.MiddleCenter;
      C4_B2.ThreeState = true;
      C4_B2.UseVisualStyleBackColor = true;
      C4_B2.Click += Pixel_Click;
      // 
      // C4_B3
      // 
      C4_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C4_B3.Location = new Point(53, 51);
      C4_B3.Name = "C4_B3";
      C4_B3.Size = new Size(14, 14);
      C4_B3.TabIndex = 332;
      C4_B3.TextAlign = ContentAlignment.MiddleCenter;
      C4_B3.ThreeState = true;
      C4_B3.UseVisualStyleBackColor = true;
      C4_B3.Click += Pixel_Click;
      // 
      // C4_B4
      // 
      C4_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C4_B4.Location = new Point(53, 39);
      C4_B4.Name = "C4_B4";
      C4_B4.Size = new Size(14, 14);
      C4_B4.TabIndex = 331;
      C4_B4.TextAlign = ContentAlignment.MiddleCenter;
      C4_B4.ThreeState = true;
      C4_B4.UseVisualStyleBackColor = true;
      C4_B4.Click += Pixel_Click;
      // 
      // C4_B5
      // 
      C4_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C4_B5.Location = new Point(53, 27);
      C4_B5.Name = "C4_B5";
      C4_B5.Size = new Size(14, 14);
      C4_B5.TabIndex = 330;
      C4_B5.TextAlign = ContentAlignment.MiddleCenter;
      C4_B5.ThreeState = true;
      C4_B5.UseVisualStyleBackColor = true;
      C4_B5.Click += Pixel_Click;
      // 
      // C4_B6
      // 
      C4_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C4_B6.Location = new Point(53, 15);
      C4_B6.Name = "C4_B6";
      C4_B6.Size = new Size(14, 14);
      C4_B6.TabIndex = 329;
      C4_B6.TextAlign = ContentAlignment.MiddleCenter;
      C4_B6.ThreeState = true;
      C4_B6.UseVisualStyleBackColor = true;
      C4_B6.Click += Pixel_Click;
      // 
      // C4_B7
      // 
      C4_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C4_B7.Location = new Point(53, 3);
      C4_B7.Name = "C4_B7";
      C4_B7.Size = new Size(14, 14);
      C4_B7.TabIndex = 328;
      C4_B7.TextAlign = ContentAlignment.MiddleCenter;
      C4_B7.ThreeState = true;
      C4_B7.UseVisualStyleBackColor = true;
      C4_B7.Click += Pixel_Click;
      // 
      // C5_B0
      // 
      C5_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C5_B0.Location = new Point(66, 87);
      C5_B0.Name = "C5_B0";
      C5_B0.Size = new Size(14, 14);
      C5_B0.TabIndex = 327;
      C5_B0.TextAlign = ContentAlignment.MiddleCenter;
      C5_B0.ThreeState = true;
      C5_B0.UseVisualStyleBackColor = true;
      C5_B0.Click += Pixel_Click;
      // 
      // C5_B1
      // 
      C5_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C5_B1.Location = new Point(66, 75);
      C5_B1.Name = "C5_B1";
      C5_B1.Size = new Size(14, 14);
      C5_B1.TabIndex = 326;
      C5_B1.TextAlign = ContentAlignment.MiddleCenter;
      C5_B1.ThreeState = true;
      C5_B1.UseVisualStyleBackColor = true;
      C5_B1.Click += Pixel_Click;
      // 
      // C5_B2
      // 
      C5_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C5_B2.Location = new Point(66, 63);
      C5_B2.Name = "C5_B2";
      C5_B2.Size = new Size(14, 14);
      C5_B2.TabIndex = 325;
      C5_B2.TextAlign = ContentAlignment.MiddleCenter;
      C5_B2.ThreeState = true;
      C5_B2.UseVisualStyleBackColor = true;
      C5_B2.Click += Pixel_Click;
      // 
      // C5_B3
      // 
      C5_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C5_B3.Location = new Point(66, 51);
      C5_B3.Name = "C5_B3";
      C5_B3.Size = new Size(14, 14);
      C5_B3.TabIndex = 324;
      C5_B3.TextAlign = ContentAlignment.MiddleCenter;
      C5_B3.ThreeState = true;
      C5_B3.UseVisualStyleBackColor = true;
      C5_B3.Click += Pixel_Click;
      // 
      // C5_B4
      // 
      C5_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C5_B4.Location = new Point(66, 39);
      C5_B4.Name = "C5_B4";
      C5_B4.Size = new Size(14, 14);
      C5_B4.TabIndex = 323;
      C5_B4.TextAlign = ContentAlignment.MiddleCenter;
      C5_B4.ThreeState = true;
      C5_B4.UseVisualStyleBackColor = true;
      C5_B4.Click += Pixel_Click;
      // 
      // C5_B5
      // 
      C5_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C5_B5.Location = new Point(66, 27);
      C5_B5.Name = "C5_B5";
      C5_B5.Size = new Size(14, 14);
      C5_B5.TabIndex = 322;
      C5_B5.TextAlign = ContentAlignment.MiddleCenter;
      C5_B5.ThreeState = true;
      C5_B5.UseVisualStyleBackColor = true;
      C5_B5.Click += Pixel_Click;
      // 
      // C5_B6
      // 
      C5_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C5_B6.Location = new Point(66, 15);
      C5_B6.Name = "C5_B6";
      C5_B6.Size = new Size(14, 14);
      C5_B6.TabIndex = 321;
      C5_B6.TextAlign = ContentAlignment.MiddleCenter;
      C5_B6.ThreeState = true;
      C5_B6.UseVisualStyleBackColor = true;
      C5_B6.Click += Pixel_Click;
      // 
      // C5_B7
      // 
      C5_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C5_B7.Location = new Point(66, 3);
      C5_B7.Name = "C5_B7";
      C5_B7.Size = new Size(14, 14);
      C5_B7.TabIndex = 320;
      C5_B7.TextAlign = ContentAlignment.MiddleCenter;
      C5_B7.ThreeState = true;
      C5_B7.UseVisualStyleBackColor = true;
      C5_B7.Click += Pixel_Click;
      // 
      // C2_B0
      // 
      C2_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C2_B0.Location = new Point(28, 87);
      C2_B0.Name = "C2_B0";
      C2_B0.Size = new Size(14, 14);
      C2_B0.TabIndex = 319;
      C2_B0.TextAlign = ContentAlignment.MiddleCenter;
      C2_B0.ThreeState = true;
      C2_B0.UseVisualStyleBackColor = true;
      C2_B0.Click += Pixel_Click;
      // 
      // C2_B1
      // 
      C2_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C2_B1.Location = new Point(28, 75);
      C2_B1.Name = "C2_B1";
      C2_B1.Size = new Size(14, 14);
      C2_B1.TabIndex = 318;
      C2_B1.TextAlign = ContentAlignment.MiddleCenter;
      C2_B1.ThreeState = true;
      C2_B1.UseVisualStyleBackColor = true;
      C2_B1.Click += Pixel_Click;
      // 
      // C2_B2
      // 
      C2_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C2_B2.Location = new Point(28, 63);
      C2_B2.Name = "C2_B2";
      C2_B2.Size = new Size(14, 14);
      C2_B2.TabIndex = 317;
      C2_B2.TextAlign = ContentAlignment.MiddleCenter;
      C2_B2.ThreeState = true;
      C2_B2.UseVisualStyleBackColor = true;
      C2_B2.Click += Pixel_Click;
      // 
      // C2_B3
      // 
      C2_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C2_B3.Location = new Point(28, 51);
      C2_B3.Name = "C2_B3";
      C2_B3.Size = new Size(14, 14);
      C2_B3.TabIndex = 316;
      C2_B3.TextAlign = ContentAlignment.MiddleCenter;
      C2_B3.ThreeState = true;
      C2_B3.UseVisualStyleBackColor = true;
      C2_B3.Click += Pixel_Click;
      // 
      // C2_B4
      // 
      C2_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C2_B4.Location = new Point(28, 39);
      C2_B4.Name = "C2_B4";
      C2_B4.Size = new Size(14, 14);
      C2_B4.TabIndex = 315;
      C2_B4.TextAlign = ContentAlignment.MiddleCenter;
      C2_B4.ThreeState = true;
      C2_B4.UseVisualStyleBackColor = true;
      C2_B4.Click += Pixel_Click;
      // 
      // C2_B5
      // 
      C2_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C2_B5.Location = new Point(28, 27);
      C2_B5.Name = "C2_B5";
      C2_B5.Size = new Size(14, 14);
      C2_B5.TabIndex = 314;
      C2_B5.TextAlign = ContentAlignment.MiddleCenter;
      C2_B5.ThreeState = true;
      C2_B5.UseVisualStyleBackColor = true;
      C2_B5.Click += Pixel_Click;
      // 
      // C2_B6
      // 
      C2_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C2_B6.Location = new Point(28, 15);
      C2_B6.Name = "C2_B6";
      C2_B6.Size = new Size(14, 14);
      C2_B6.TabIndex = 313;
      C2_B6.TextAlign = ContentAlignment.MiddleCenter;
      C2_B6.ThreeState = true;
      C2_B6.UseVisualStyleBackColor = true;
      C2_B6.Click += Pixel_Click;
      // 
      // C2_B7
      // 
      C2_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C2_B7.Location = new Point(28, 3);
      C2_B7.Name = "C2_B7";
      C2_B7.Size = new Size(14, 14);
      C2_B7.TabIndex = 312;
      C2_B7.TextAlign = ContentAlignment.MiddleCenter;
      C2_B7.ThreeState = true;
      C2_B7.UseVisualStyleBackColor = true;
      C2_B7.Click += Pixel_Click;
      // 
      // C3_B0
      // 
      C3_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C3_B0.Location = new Point(41, 87);
      C3_B0.Name = "C3_B0";
      C3_B0.Size = new Size(14, 14);
      C3_B0.TabIndex = 311;
      C3_B0.TextAlign = ContentAlignment.MiddleCenter;
      C3_B0.ThreeState = true;
      C3_B0.UseVisualStyleBackColor = true;
      C3_B0.Click += Pixel_Click;
      // 
      // C3_B1
      // 
      C3_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C3_B1.Location = new Point(41, 75);
      C3_B1.Name = "C3_B1";
      C3_B1.Size = new Size(14, 14);
      C3_B1.TabIndex = 310;
      C3_B1.TextAlign = ContentAlignment.MiddleCenter;
      C3_B1.ThreeState = true;
      C3_B1.UseVisualStyleBackColor = true;
      C3_B1.Click += Pixel_Click;
      // 
      // C3_B2
      // 
      C3_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C3_B2.Location = new Point(41, 63);
      C3_B2.Name = "C3_B2";
      C3_B2.Size = new Size(14, 14);
      C3_B2.TabIndex = 309;
      C3_B2.TextAlign = ContentAlignment.MiddleCenter;
      C3_B2.ThreeState = true;
      C3_B2.UseVisualStyleBackColor = true;
      C3_B2.Click += Pixel_Click;
      // 
      // C3_B3
      // 
      C3_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C3_B3.Location = new Point(41, 51);
      C3_B3.Name = "C3_B3";
      C3_B3.Size = new Size(14, 14);
      C3_B3.TabIndex = 308;
      C3_B3.TextAlign = ContentAlignment.MiddleCenter;
      C3_B3.ThreeState = true;
      C3_B3.UseVisualStyleBackColor = true;
      C3_B3.Click += Pixel_Click;
      // 
      // C3_B4
      // 
      C3_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C3_B4.Location = new Point(41, 39);
      C3_B4.Name = "C3_B4";
      C3_B4.Size = new Size(14, 14);
      C3_B4.TabIndex = 307;
      C3_B4.TextAlign = ContentAlignment.MiddleCenter;
      C3_B4.ThreeState = true;
      C3_B4.UseVisualStyleBackColor = true;
      C3_B4.Click += Pixel_Click;
      // 
      // C3_B5
      // 
      C3_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C3_B5.Location = new Point(41, 27);
      C3_B5.Name = "C3_B5";
      C3_B5.Size = new Size(14, 14);
      C3_B5.TabIndex = 306;
      C3_B5.TextAlign = ContentAlignment.MiddleCenter;
      C3_B5.ThreeState = true;
      C3_B5.UseVisualStyleBackColor = true;
      C3_B5.Click += Pixel_Click;
      // 
      // C3_B6
      // 
      C3_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C3_B6.Location = new Point(41, 15);
      C3_B6.Name = "C3_B6";
      C3_B6.Size = new Size(14, 14);
      C3_B6.TabIndex = 305;
      C3_B6.TextAlign = ContentAlignment.MiddleCenter;
      C3_B6.ThreeState = true;
      C3_B6.UseVisualStyleBackColor = true;
      C3_B6.Click += Pixel_Click;
      // 
      // C3_B7
      // 
      C3_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C3_B7.Location = new Point(41, 3);
      C3_B7.Name = "C3_B7";
      C3_B7.Size = new Size(14, 14);
      C3_B7.TabIndex = 304;
      C3_B7.TextAlign = ContentAlignment.MiddleCenter;
      C3_B7.ThreeState = true;
      C3_B7.UseVisualStyleBackColor = true;
      C3_B7.Click += Pixel_Click;
      // 
      // C0_B0
      // 
      C0_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C0_B0.Location = new Point(3, 87);
      C0_B0.Name = "C0_B0";
      C0_B0.Size = new Size(14, 14);
      C0_B0.TabIndex = 303;
      C0_B0.TextAlign = ContentAlignment.MiddleCenter;
      C0_B0.ThreeState = true;
      C0_B0.UseVisualStyleBackColor = true;
      C0_B0.Click += Pixel_Click;
      // 
      // C0_B1
      // 
      C0_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C0_B1.Location = new Point(3, 75);
      C0_B1.Name = "C0_B1";
      C0_B1.Size = new Size(14, 14);
      C0_B1.TabIndex = 302;
      C0_B1.TextAlign = ContentAlignment.MiddleCenter;
      C0_B1.ThreeState = true;
      C0_B1.UseVisualStyleBackColor = true;
      C0_B1.Click += Pixel_Click;
      // 
      // C0_B2
      // 
      C0_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C0_B2.Location = new Point(3, 63);
      C0_B2.Name = "C0_B2";
      C0_B2.Size = new Size(14, 14);
      C0_B2.TabIndex = 301;
      C0_B2.TextAlign = ContentAlignment.MiddleCenter;
      C0_B2.ThreeState = true;
      C0_B2.UseVisualStyleBackColor = true;
      C0_B2.Click += Pixel_Click;
      // 
      // C0_B3
      // 
      C0_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C0_B3.Location = new Point(3, 51);
      C0_B3.Name = "C0_B3";
      C0_B3.Size = new Size(14, 14);
      C0_B3.TabIndex = 300;
      C0_B3.TextAlign = ContentAlignment.MiddleCenter;
      C0_B3.ThreeState = true;
      C0_B3.UseVisualStyleBackColor = true;
      C0_B3.Click += Pixel_Click;
      // 
      // C0_B4
      // 
      C0_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C0_B4.Location = new Point(3, 39);
      C0_B4.Name = "C0_B4";
      C0_B4.Size = new Size(14, 14);
      C0_B4.TabIndex = 299;
      C0_B4.TextAlign = ContentAlignment.MiddleCenter;
      C0_B4.ThreeState = true;
      C0_B4.UseVisualStyleBackColor = true;
      C0_B4.Click += Pixel_Click;
      // 
      // C0_B5
      // 
      C0_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C0_B5.Location = new Point(3, 27);
      C0_B5.Name = "C0_B5";
      C0_B5.Size = new Size(14, 14);
      C0_B5.TabIndex = 298;
      C0_B5.TextAlign = ContentAlignment.MiddleCenter;
      C0_B5.ThreeState = true;
      C0_B5.UseVisualStyleBackColor = true;
      C0_B5.Click += Pixel_Click;
      // 
      // C0_B6
      // 
      C0_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C0_B6.Location = new Point(3, 15);
      C0_B6.Name = "C0_B6";
      C0_B6.Size = new Size(14, 14);
      C0_B6.TabIndex = 297;
      C0_B6.TextAlign = ContentAlignment.MiddleCenter;
      C0_B6.ThreeState = true;
      C0_B6.UseVisualStyleBackColor = true;
      C0_B6.Click += Pixel_Click;
      // 
      // C0_B7
      // 
      C0_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C0_B7.Location = new Point(3, 3);
      C0_B7.Name = "C0_B7";
      C0_B7.Size = new Size(14, 14);
      C0_B7.TabIndex = 296;
      C0_B7.TextAlign = ContentAlignment.MiddleCenter;
      C0_B7.ThreeState = true;
      C0_B7.UseVisualStyleBackColor = true;
      C0_B7.Click += Pixel_Click;
      // 
      // C1_B0
      // 
      C1_B0.CheckAlign = ContentAlignment.MiddleCenter;
      C1_B0.Location = new Point(16, 87);
      C1_B0.Name = "C1_B0";
      C1_B0.Size = new Size(14, 14);
      C1_B0.TabIndex = 295;
      C1_B0.TextAlign = ContentAlignment.MiddleCenter;
      C1_B0.ThreeState = true;
      C1_B0.UseVisualStyleBackColor = true;
      C1_B0.Click += Pixel_Click;
      // 
      // C1_B1
      // 
      C1_B1.CheckAlign = ContentAlignment.MiddleCenter;
      C1_B1.Location = new Point(16, 75);
      C1_B1.Name = "C1_B1";
      C1_B1.Size = new Size(14, 14);
      C1_B1.TabIndex = 294;
      C1_B1.TextAlign = ContentAlignment.MiddleCenter;
      C1_B1.ThreeState = true;
      C1_B1.UseVisualStyleBackColor = true;
      C1_B1.Click += Pixel_Click;
      // 
      // C1_B2
      // 
      C1_B2.CheckAlign = ContentAlignment.MiddleCenter;
      C1_B2.Location = new Point(16, 63);
      C1_B2.Name = "C1_B2";
      C1_B2.Size = new Size(14, 14);
      C1_B2.TabIndex = 293;
      C1_B2.TextAlign = ContentAlignment.MiddleCenter;
      C1_B2.ThreeState = true;
      C1_B2.UseVisualStyleBackColor = true;
      C1_B2.Click += Pixel_Click;
      // 
      // C1_B3
      // 
      C1_B3.CheckAlign = ContentAlignment.MiddleCenter;
      C1_B3.Location = new Point(16, 51);
      C1_B3.Name = "C1_B3";
      C1_B3.Size = new Size(14, 14);
      C1_B3.TabIndex = 292;
      C1_B3.TextAlign = ContentAlignment.MiddleCenter;
      C1_B3.ThreeState = true;
      C1_B3.UseVisualStyleBackColor = true;
      C1_B3.Click += Pixel_Click;
      // 
      // C1_B4
      // 
      C1_B4.CheckAlign = ContentAlignment.MiddleCenter;
      C1_B4.Location = new Point(16, 39);
      C1_B4.Name = "C1_B4";
      C1_B4.Size = new Size(14, 14);
      C1_B4.TabIndex = 291;
      C1_B4.TextAlign = ContentAlignment.MiddleCenter;
      C1_B4.ThreeState = true;
      C1_B4.UseVisualStyleBackColor = true;
      C1_B4.Click += Pixel_Click;
      // 
      // C1_B5
      // 
      C1_B5.CheckAlign = ContentAlignment.MiddleCenter;
      C1_B5.Location = new Point(16, 27);
      C1_B5.Name = "C1_B5";
      C1_B5.Size = new Size(14, 14);
      C1_B5.TabIndex = 290;
      C1_B5.TextAlign = ContentAlignment.MiddleCenter;
      C1_B5.ThreeState = true;
      C1_B5.UseVisualStyleBackColor = true;
      C1_B5.Click += Pixel_Click;
      // 
      // C1_B6
      // 
      C1_B6.CheckAlign = ContentAlignment.MiddleCenter;
      C1_B6.Location = new Point(16, 15);
      C1_B6.Name = "C1_B6";
      C1_B6.Size = new Size(14, 14);
      C1_B6.TabIndex = 289;
      C1_B6.TextAlign = ContentAlignment.MiddleCenter;
      C1_B6.ThreeState = true;
      C1_B6.UseVisualStyleBackColor = true;
      C1_B6.Click += Pixel_Click;
      // 
      // C1_B7
      // 
      C1_B7.CheckAlign = ContentAlignment.MiddleCenter;
      C1_B7.Location = new Point(16, 3);
      C1_B7.Name = "C1_B7";
      C1_B7.Size = new Size(14, 14);
      C1_B7.TabIndex = 288;
      C1_B7.TextAlign = ContentAlignment.MiddleCenter;
      C1_B7.ThreeState = true;
      C1_B7.UseVisualStyleBackColor = true;
      C1_B7.Click += Pixel_Click;
      // 
      // cmdLoadInternal
      // 
      cmdLoadInternal.Anchor = (((AnchorStyles.Top | AnchorStyles.Right)));
      cmdLoadInternal.Location = new Point(314, 39);
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
      cmdLoadCustom.Location = new Point(314, 14);
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
      cmdSave.Location = new Point(233, 456);
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
      cmdExit.Location = new Point(314, 456);
      cmdExit.Name = "cmdExit";
      cmdExit.Size = new Size(75, 23);
      cmdExit.TabIndex = 551;
      cmdExit.Text = "Exit";
      cmdExit.UseVisualStyleBackColor = true;
      cmdExit.Click += cmdExit_Click;
      // 
      // Icon0
      // 
      Icon0.BorderStyle = BorderStyle.FixedSingle;
      Icon0.Enabled = false;
      Icon0.Location = new Point(20, 163);
      Icon0.Name = "Icon0";
      Icon0.Size = new Size(14, 18);
      Icon0.TabIndex = 1;
      Icon0.TabStop = false;
      Icon0.Click += Icon_Click;
      // 
      // Icon1
      // 
      Icon1.BorderStyle = BorderStyle.FixedSingle;
      Icon1.Enabled = false;
      Icon1.Location = new Point(33, 163);
      Icon1.Name = "Icon1";
      Icon1.Size = new Size(14, 18);
      Icon1.TabIndex = 2;
      Icon1.TabStop = false;
      Icon1.Click += Icon_Click;
      // 
      // Icon2
      // 
      Icon2.BorderStyle = BorderStyle.FixedSingle;
      Icon2.Enabled = false;
      Icon2.Location = new Point(46, 163);
      Icon2.Name = "Icon2";
      Icon2.Size = new Size(14, 18);
      Icon2.TabIndex = 3;
      Icon2.TabStop = false;
      Icon2.Click += Icon_Click;
      // 
      // Icon3
      // 
      Icon3.BorderStyle = BorderStyle.FixedSingle;
      Icon3.Enabled = false;
      Icon3.Location = new Point(59, 163);
      Icon3.Name = "Icon3";
      Icon3.Size = new Size(14, 18);
      Icon3.TabIndex = 4;
      Icon3.TabStop = false;
      Icon3.Click += Icon_Click;
      // 
      // Icon4
      // 
      Icon4.BorderStyle = BorderStyle.FixedSingle;
      Icon4.Enabled = false;
      Icon4.Location = new Point(72, 163);
      Icon4.Name = "Icon4";
      Icon4.Size = new Size(14, 18);
      Icon4.TabIndex = 5;
      Icon4.TabStop = false;
      Icon4.Click += Icon_Click;
      // 
      // Icon5
      // 
      Icon5.BorderStyle = BorderStyle.FixedSingle;
      Icon5.Enabled = false;
      Icon5.Location = new Point(85, 163);
      Icon5.Name = "Icon5";
      Icon5.Size = new Size(14, 18);
      Icon5.TabIndex = 6;
      Icon5.TabStop = false;
      Icon5.Click += Icon_Click;
      // 
      // Icon6
      // 
      Icon6.BorderStyle = BorderStyle.FixedSingle;
      Icon6.Enabled = false;
      Icon6.Location = new Point(98, 163);
      Icon6.Name = "Icon6";
      Icon6.Size = new Size(14, 18);
      Icon6.TabIndex = 7;
      Icon6.TabStop = false;
      Icon6.Click += Icon_Click;
      // 
      // Icon7
      // 
      Icon7.BorderStyle = BorderStyle.FixedSingle;
      Icon7.Enabled = false;
      Icon7.Location = new Point(111, 163);
      Icon7.Name = "Icon7";
      Icon7.Size = new Size(14, 18);
      Icon7.TabIndex = 8;
      Icon7.TabStop = false;
      Icon7.Click += Icon_Click;
      // 
      // Icon8
      // 
      Icon8.BorderStyle = BorderStyle.FixedSingle;
      Icon8.Enabled = false;
      Icon8.Location = new Point(124, 163);
      Icon8.Name = "Icon8";
      Icon8.Size = new Size(14, 18);
      Icon8.TabIndex = 9;
      Icon8.TabStop = false;
      Icon8.Click += Icon_Click;
      // 
      // Icon9
      // 
      Icon9.BorderStyle = BorderStyle.FixedSingle;
      Icon9.Enabled = false;
      Icon9.Location = new Point(137, 163);
      Icon9.Name = "Icon9";
      Icon9.Size = new Size(14, 18);
      Icon9.TabIndex = 10;
      Icon9.TabStop = false;
      Icon9.Click += Icon_Click;
      // 
      // Icon10
      // 
      Icon10.BorderStyle = BorderStyle.FixedSingle;
      Icon10.Enabled = false;
      Icon10.Location = new Point(150, 163);
      Icon10.Name = "Icon10";
      Icon10.Size = new Size(14, 18);
      Icon10.TabIndex = 552;
      Icon10.TabStop = false;
      Icon10.Click += Icon_Click;
      // 
      // Icon11
      // 
      Icon11.BorderStyle = BorderStyle.FixedSingle;
      Icon11.Enabled = false;
      Icon11.Location = new Point(163, 163);
      Icon11.Name = "Icon11";
      Icon11.Size = new Size(14, 18);
      Icon11.TabIndex = 553;
      Icon11.TabStop = false;
      Icon11.Click += Icon_Click;
      // 
      // Icon12
      // 
      Icon12.BorderStyle = BorderStyle.FixedSingle;
      Icon12.Enabled = false;
      Icon12.Location = new Point(176, 163);
      Icon12.Name = "Icon12";
      Icon12.Size = new Size(14, 18);
      Icon12.TabIndex = 554;
      Icon12.TabStop = false;
      Icon12.Click += Icon_Click;
      // 
      // Icon13
      // 
      Icon13.BorderStyle = BorderStyle.FixedSingle;
      Icon13.Enabled = false;
      Icon13.Location = new Point(189, 163);
      Icon13.Name = "Icon13";
      Icon13.Size = new Size(14, 18);
      Icon13.TabIndex = 555;
      Icon13.TabStop = false;
      Icon13.Click += Icon_Click;
      // 
      // Icon14
      // 
      Icon14.BorderStyle = BorderStyle.FixedSingle;
      Icon14.Enabled = false;
      Icon14.Location = new Point(202, 163);
      Icon14.Name = "Icon14";
      Icon14.Size = new Size(14, 18);
      Icon14.TabIndex = 556;
      Icon14.TabStop = false;
      Icon14.Click += Icon_Click;
      // 
      // Icon15
      // 
      Icon15.BorderStyle = BorderStyle.FixedSingle;
      Icon15.Enabled = false;
      Icon15.Location = new Point(215, 163);
      Icon15.Name = "Icon15";
      Icon15.Size = new Size(14, 18);
      Icon15.TabIndex = 557;
      Icon15.TabStop = false;
      Icon15.Click += Icon_Click;
      // 
      // Icon16
      // 
      Icon16.BorderStyle = BorderStyle.FixedSingle;
      Icon16.Enabled = false;
      Icon16.Location = new Point(20, 180);
      Icon16.Name = "Icon16";
      Icon16.Size = new Size(14, 18);
      Icon16.TabIndex = 558;
      Icon16.TabStop = false;
      Icon16.Click += Icon_Click;
      // 
      // Icon17
      // 
      Icon17.BorderStyle = BorderStyle.FixedSingle;
      Icon17.Enabled = false;
      Icon17.Location = new Point(33, 180);
      Icon17.Name = "Icon17";
      Icon17.Size = new Size(14, 18);
      Icon17.TabIndex = 559;
      Icon17.TabStop = false;
      Icon17.Click += Icon_Click;
      // 
      // Icon18
      // 
      Icon18.BorderStyle = BorderStyle.FixedSingle;
      Icon18.Enabled = false;
      Icon18.Location = new Point(46, 180);
      Icon18.Name = "Icon18";
      Icon18.Size = new Size(14, 18);
      Icon18.TabIndex = 560;
      Icon18.TabStop = false;
      Icon18.Click += Icon_Click;
      // 
      // Icon19
      // 
      Icon19.BorderStyle = BorderStyle.FixedSingle;
      Icon19.Enabled = false;
      Icon19.Location = new Point(59, 180);
      Icon19.Name = "Icon19";
      Icon19.Size = new Size(14, 18);
      Icon19.TabIndex = 561;
      Icon19.TabStop = false;
      Icon19.Click += Icon_Click;
      // 
      // Icon20
      // 
      Icon20.BorderStyle = BorderStyle.FixedSingle;
      Icon20.Enabled = false;
      Icon20.Location = new Point(72, 180);
      Icon20.Name = "Icon20";
      Icon20.Size = new Size(14, 18);
      Icon20.TabIndex = 562;
      Icon20.TabStop = false;
      Icon20.Click += Icon_Click;
      // 
      // Icon21
      // 
      Icon21.BorderStyle = BorderStyle.FixedSingle;
      Icon21.Enabled = false;
      Icon21.Location = new Point(85, 180);
      Icon21.Name = "Icon21";
      Icon21.Size = new Size(14, 18);
      Icon21.TabIndex = 563;
      Icon21.TabStop = false;
      Icon21.Click += Icon_Click;
      // 
      // Icon22
      // 
      Icon22.BorderStyle = BorderStyle.FixedSingle;
      Icon22.Enabled = false;
      Icon22.Location = new Point(98, 180);
      Icon22.Name = "Icon22";
      Icon22.Size = new Size(14, 18);
      Icon22.TabIndex = 564;
      Icon22.TabStop = false;
      Icon22.Click += Icon_Click;
      // 
      // Icon23
      // 
      Icon23.BorderStyle = BorderStyle.FixedSingle;
      Icon23.Enabled = false;
      Icon23.Location = new Point(111, 180);
      Icon23.Name = "Icon23";
      Icon23.Size = new Size(14, 18);
      Icon23.TabIndex = 565;
      Icon23.TabStop = false;
      Icon23.Click += Icon_Click;
      // 
      // Icon24
      // 
      Icon24.BorderStyle = BorderStyle.FixedSingle;
      Icon24.Enabled = false;
      Icon24.Location = new Point(124, 180);
      Icon24.Name = "Icon24";
      Icon24.Size = new Size(14, 18);
      Icon24.TabIndex = 566;
      Icon24.TabStop = false;
      Icon24.Click += Icon_Click;
      // 
      // Icon25
      // 
      Icon25.BorderStyle = BorderStyle.FixedSingle;
      Icon25.Enabled = false;
      Icon25.Location = new Point(137, 180);
      Icon25.Name = "Icon25";
      Icon25.Size = new Size(14, 18);
      Icon25.TabIndex = 567;
      Icon25.TabStop = false;
      Icon25.Click += Icon_Click;
      // 
      // Icon26
      // 
      Icon26.BorderStyle = BorderStyle.FixedSingle;
      Icon26.Enabled = false;
      Icon26.Location = new Point(150, 180);
      Icon26.Name = "Icon26";
      Icon26.Size = new Size(14, 18);
      Icon26.TabIndex = 568;
      Icon26.TabStop = false;
      Icon26.Click += Icon_Click;
      // 
      // Icon27
      // 
      Icon27.BorderStyle = BorderStyle.FixedSingle;
      Icon27.Enabled = false;
      Icon27.Location = new Point(163, 180);
      Icon27.Name = "Icon27";
      Icon27.Size = new Size(14, 18);
      Icon27.TabIndex = 569;
      Icon27.TabStop = false;
      Icon27.Click += Icon_Click;
      // 
      // Icon28
      // 
      Icon28.BorderStyle = BorderStyle.FixedSingle;
      Icon28.Enabled = false;
      Icon28.Location = new Point(176, 180);
      Icon28.Name = "Icon28";
      Icon28.Size = new Size(14, 18);
      Icon28.TabIndex = 570;
      Icon28.TabStop = false;
      Icon28.Click += Icon_Click;
      // 
      // Icon29
      // 
      Icon29.BorderStyle = BorderStyle.FixedSingle;
      Icon29.Enabled = false;
      Icon29.Location = new Point(189, 180);
      Icon29.Name = "Icon29";
      Icon29.Size = new Size(14, 18);
      Icon29.TabIndex = 571;
      Icon29.TabStop = false;
      Icon29.Click += Icon_Click;
      // 
      // Icon30
      // 
      Icon30.BorderStyle = BorderStyle.FixedSingle;
      Icon30.Enabled = false;
      Icon30.Location = new Point(202, 180);
      Icon30.Name = "Icon30";
      Icon30.Size = new Size(14, 18);
      Icon30.TabIndex = 572;
      Icon30.TabStop = false;
      Icon30.Click += Icon_Click;
      // 
      // Icon31
      // 
      Icon31.BorderStyle = BorderStyle.FixedSingle;
      Icon31.Enabled = false;
      Icon31.Location = new Point(215, 180);
      Icon31.Name = "Icon31";
      Icon31.Size = new Size(14, 18);
      Icon31.TabIndex = 573;
      Icon31.TabStop = false;
      Icon31.Click += Icon_Click;
      // 
      // Icon32
      // 
      Icon32.BorderStyle = BorderStyle.FixedSingle;
      Icon32.Enabled = false;
      Icon32.Location = new Point(20, 197);
      Icon32.Name = "Icon32";
      Icon32.Size = new Size(14, 18);
      Icon32.TabIndex = 1;
      Icon32.TabStop = false;
      Icon32.Click += Icon_Click;
      // 
      // Icon33
      // 
      Icon33.BorderStyle = BorderStyle.FixedSingle;
      Icon33.Enabled = false;
      Icon33.Location = new Point(33, 197);
      Icon33.Name = "Icon33";
      Icon33.Size = new Size(14, 18);
      Icon33.TabIndex = 2;
      Icon33.TabStop = false;
      Icon33.Click += Icon_Click;
      // 
      // Icon34
      // 
      Icon34.BorderStyle = BorderStyle.FixedSingle;
      Icon34.Enabled = false;
      Icon34.Location = new Point(46, 197);
      Icon34.Name = "Icon34";
      Icon34.Size = new Size(14, 18);
      Icon34.TabIndex = 3;
      Icon34.TabStop = false;
      Icon34.Click += Icon_Click;
      // 
      // Icon35
      // 
      Icon35.BorderStyle = BorderStyle.FixedSingle;
      Icon35.Enabled = false;
      Icon35.Location = new Point(59, 197);
      Icon35.Name = "Icon35";
      Icon35.Size = new Size(14, 18);
      Icon35.TabIndex = 4;
      Icon35.TabStop = false;
      Icon35.Click += Icon_Click;
      // 
      // Icon36
      // 
      Icon36.BorderStyle = BorderStyle.FixedSingle;
      Icon36.Enabled = false;
      Icon36.Location = new Point(72, 197);
      Icon36.Name = "Icon36";
      Icon36.Size = new Size(14, 18);
      Icon36.TabIndex = 5;
      Icon36.TabStop = false;
      Icon36.Click += Icon_Click;
      // 
      // Icon37
      // 
      Icon37.BorderStyle = BorderStyle.FixedSingle;
      Icon37.Enabled = false;
      Icon37.Location = new Point(85, 197);
      Icon37.Name = "Icon37";
      Icon37.Size = new Size(14, 18);
      Icon37.TabIndex = 6;
      Icon37.TabStop = false;
      Icon37.Click += Icon_Click;
      // 
      // Icon38
      // 
      Icon38.BorderStyle = BorderStyle.FixedSingle;
      Icon38.Enabled = false;
      Icon38.Location = new Point(98, 197);
      Icon38.Name = "Icon38";
      Icon38.Size = new Size(14, 18);
      Icon38.TabIndex = 7;
      Icon38.TabStop = false;
      Icon38.Click += Icon_Click;
      // 
      // Icon39
      // 
      Icon39.BorderStyle = BorderStyle.FixedSingle;
      Icon39.Enabled = false;
      Icon39.Location = new Point(111, 197);
      Icon39.Name = "Icon39";
      Icon39.Size = new Size(14, 18);
      Icon39.TabIndex = 8;
      Icon39.TabStop = false;
      Icon39.Click += Icon_Click;
      // 
      // Icon40
      // 
      Icon40.BorderStyle = BorderStyle.FixedSingle;
      Icon40.Enabled = false;
      Icon40.Location = new Point(124, 197);
      Icon40.Name = "Icon40";
      Icon40.Size = new Size(14, 18);
      Icon40.TabIndex = 9;
      Icon40.TabStop = false;
      Icon40.Click += Icon_Click;
      // 
      // Icon41
      // 
      Icon41.BorderStyle = BorderStyle.FixedSingle;
      Icon41.Enabled = false;
      Icon41.Location = new Point(137, 197);
      Icon41.Name = "Icon41";
      Icon41.Size = new Size(14, 18);
      Icon41.TabIndex = 10;
      Icon41.TabStop = false;
      Icon41.Click += Icon_Click;
      // 
      // Icon42
      // 
      Icon42.BorderStyle = BorderStyle.FixedSingle;
      Icon42.Enabled = false;
      Icon42.Location = new Point(150, 197);
      Icon42.Name = "Icon42";
      Icon42.Size = new Size(14, 18);
      Icon42.TabIndex = 552;
      Icon42.TabStop = false;
      Icon42.Click += Icon_Click;
      // 
      // Icon43
      // 
      Icon43.BorderStyle = BorderStyle.FixedSingle;
      Icon43.Enabled = false;
      Icon43.Location = new Point(163, 197);
      Icon43.Name = "Icon43";
      Icon43.Size = new Size(14, 18);
      Icon43.TabIndex = 553;
      Icon43.TabStop = false;
      Icon43.Click += Icon_Click;
      // 
      // Icon44
      // 
      Icon44.BorderStyle = BorderStyle.FixedSingle;
      Icon44.Enabled = false;
      Icon44.Location = new Point(176, 197);
      Icon44.Name = "Icon44";
      Icon44.Size = new Size(14, 18);
      Icon44.TabIndex = 554;
      Icon44.TabStop = false;
      Icon44.Click += Icon_Click;
      // 
      // Icon45
      // 
      Icon45.BorderStyle = BorderStyle.FixedSingle;
      Icon45.Enabled = false;
      Icon45.Location = new Point(189, 197);
      Icon45.Name = "Icon45";
      Icon45.Size = new Size(14, 18);
      Icon45.TabIndex = 555;
      Icon45.TabStop = false;
      Icon45.Click += Icon_Click;
      // 
      // Icon46
      // 
      Icon46.BorderStyle = BorderStyle.FixedSingle;
      Icon46.Enabled = false;
      Icon46.Location = new Point(202, 197);
      Icon46.Name = "Icon46";
      Icon46.Size = new Size(14, 18);
      Icon46.TabIndex = 556;
      Icon46.TabStop = false;
      Icon46.Click += Icon_Click;
      // 
      // Icon47
      // 
      Icon47.BorderStyle = BorderStyle.FixedSingle;
      Icon47.Enabled = false;
      Icon47.Location = new Point(215, 197);
      Icon47.Name = "Icon47";
      Icon47.Size = new Size(14, 18);
      Icon47.TabIndex = 557;
      Icon47.TabStop = false;
      Icon47.Click += Icon_Click;
      // 
      // Icon48
      // 
      Icon48.BorderStyle = BorderStyle.FixedSingle;
      Icon48.Enabled = false;
      Icon48.Location = new Point(20, 214);
      Icon48.Name = "Icon48";
      Icon48.Size = new Size(14, 18);
      Icon48.TabIndex = 558;
      Icon48.TabStop = false;
      Icon48.Click += Icon_Click;
      // 
      // Icon49
      // 
      Icon49.BorderStyle = BorderStyle.FixedSingle;
      Icon49.Enabled = false;
      Icon49.Location = new Point(33, 214);
      Icon49.Name = "Icon49";
      Icon49.Size = new Size(14, 18);
      Icon49.TabIndex = 559;
      Icon49.TabStop = false;
      Icon49.Click += Icon_Click;
      // 
      // Icon50
      // 
      Icon50.BorderStyle = BorderStyle.FixedSingle;
      Icon50.Enabled = false;
      Icon50.Location = new Point(46, 214);
      Icon50.Name = "Icon50";
      Icon50.Size = new Size(14, 18);
      Icon50.TabIndex = 560;
      Icon50.TabStop = false;
      Icon50.Click += Icon_Click;
      // 
      // Icon51
      // 
      Icon51.BorderStyle = BorderStyle.FixedSingle;
      Icon51.Enabled = false;
      Icon51.Location = new Point(59, 214);
      Icon51.Name = "Icon51";
      Icon51.Size = new Size(14, 18);
      Icon51.TabIndex = 561;
      Icon51.TabStop = false;
      Icon51.Click += Icon_Click;
      // 
      // Icon52
      // 
      Icon52.BorderStyle = BorderStyle.FixedSingle;
      Icon52.Enabled = false;
      Icon52.Location = new Point(72, 214);
      Icon52.Name = "Icon52";
      Icon52.Size = new Size(14, 18);
      Icon52.TabIndex = 562;
      Icon52.TabStop = false;
      Icon52.Click += Icon_Click;
      // 
      // Icon53
      // 
      Icon53.BorderStyle = BorderStyle.FixedSingle;
      Icon53.Enabled = false;
      Icon53.Location = new Point(85, 214);
      Icon53.Name = "Icon53";
      Icon53.Size = new Size(14, 18);
      Icon53.TabIndex = 563;
      Icon53.TabStop = false;
      Icon53.Click += Icon_Click;
      // 
      // Icon54
      // 
      Icon54.BorderStyle = BorderStyle.FixedSingle;
      Icon54.Enabled = false;
      Icon54.Location = new Point(98, 214);
      Icon54.Name = "Icon54";
      Icon54.Size = new Size(14, 18);
      Icon54.TabIndex = 564;
      Icon54.TabStop = false;
      Icon54.Click += Icon_Click;
      // 
      // Icon55
      // 
      Icon55.BorderStyle = BorderStyle.FixedSingle;
      Icon55.Enabled = false;
      Icon55.Location = new Point(111, 214);
      Icon55.Name = "Icon55";
      Icon55.Size = new Size(14, 18);
      Icon55.TabIndex = 565;
      Icon55.TabStop = false;
      Icon55.Click += Icon_Click;
      // 
      // Icon56
      // 
      Icon56.BorderStyle = BorderStyle.FixedSingle;
      Icon56.Enabled = false;
      Icon56.Location = new Point(124, 214);
      Icon56.Name = "Icon56";
      Icon56.Size = new Size(14, 18);
      Icon56.TabIndex = 566;
      Icon56.TabStop = false;
      Icon56.Click += Icon_Click;
      // 
      // Icon57
      // 
      Icon57.BorderStyle = BorderStyle.FixedSingle;
      Icon57.Enabled = false;
      Icon57.Location = new Point(137, 214);
      Icon57.Name = "Icon57";
      Icon57.Size = new Size(14, 18);
      Icon57.TabIndex = 567;
      Icon57.TabStop = false;
      Icon57.Click += Icon_Click;
      // 
      // Icon58
      // 
      Icon58.BorderStyle = BorderStyle.FixedSingle;
      Icon58.Enabled = false;
      Icon58.Location = new Point(150, 214);
      Icon58.Name = "Icon58";
      Icon58.Size = new Size(14, 18);
      Icon58.TabIndex = 568;
      Icon58.TabStop = false;
      Icon58.Click += Icon_Click;
      // 
      // Icon59
      // 
      Icon59.BorderStyle = BorderStyle.FixedSingle;
      Icon59.Enabled = false;
      Icon59.Location = new Point(163, 214);
      Icon59.Name = "Icon59";
      Icon59.Size = new Size(14, 18);
      Icon59.TabIndex = 569;
      Icon59.TabStop = false;
      Icon59.Click += Icon_Click;
      // 
      // Icon60
      // 
      Icon60.BorderStyle = BorderStyle.FixedSingle;
      Icon60.Enabled = false;
      Icon60.Location = new Point(176, 214);
      Icon60.Name = "Icon60";
      Icon60.Size = new Size(14, 18);
      Icon60.TabIndex = 570;
      Icon60.TabStop = false;
      Icon60.Click += Icon_Click;
      // 
      // Icon61
      // 
      Icon61.BorderStyle = BorderStyle.FixedSingle;
      Icon61.Enabled = false;
      Icon61.Location = new Point(189, 214);
      Icon61.Name = "Icon61";
      Icon61.Size = new Size(14, 18);
      Icon61.TabIndex = 571;
      Icon61.TabStop = false;
      Icon61.Click += Icon_Click;
      // 
      // Icon62
      // 
      Icon62.BorderStyle = BorderStyle.FixedSingle;
      Icon62.Enabled = false;
      Icon62.Location = new Point(202, 214);
      Icon62.Name = "Icon62";
      Icon62.Size = new Size(14, 18);
      Icon62.TabIndex = 572;
      Icon62.TabStop = false;
      Icon62.Click += Icon_Click;
      // 
      // Icon63
      // 
      Icon63.BorderStyle = BorderStyle.FixedSingle;
      Icon63.Enabled = false;
      Icon63.Location = new Point(215, 214);
      Icon63.Name = "Icon63";
      Icon63.Size = new Size(14, 18);
      Icon63.TabIndex = 573;
      Icon63.TabStop = false;
      Icon63.Click += Icon_Click;
      // 
      // Icon64
      // 
      Icon64.BorderStyle = BorderStyle.FixedSingle;
      Icon64.Enabled = false;
      Icon64.Location = new Point(20, 231);
      Icon64.Name = "Icon64";
      Icon64.Size = new Size(14, 18);
      Icon64.TabIndex = 1;
      Icon64.TabStop = false;
      Icon64.Click += Icon_Click;
      // 
      // Icon65
      // 
      Icon65.BorderStyle = BorderStyle.FixedSingle;
      Icon65.Enabled = false;
      Icon65.Location = new Point(33, 231);
      Icon65.Name = "Icon65";
      Icon65.Size = new Size(14, 18);
      Icon65.TabIndex = 2;
      Icon65.TabStop = false;
      Icon65.Click += Icon_Click;
      // 
      // Icon66
      // 
      Icon66.BorderStyle = BorderStyle.FixedSingle;
      Icon66.Enabled = false;
      Icon66.Location = new Point(46, 231);
      Icon66.Name = "Icon66";
      Icon66.Size = new Size(14, 18);
      Icon66.TabIndex = 3;
      Icon66.TabStop = false;
      Icon66.Click += Icon_Click;
      // 
      // Icon67
      // 
      Icon67.BorderStyle = BorderStyle.FixedSingle;
      Icon67.Enabled = false;
      Icon67.Location = new Point(59, 231);
      Icon67.Name = "Icon67";
      Icon67.Size = new Size(14, 18);
      Icon67.TabIndex = 4;
      Icon67.TabStop = false;
      Icon67.Click += Icon_Click;
      // 
      // Icon68
      // 
      Icon68.BorderStyle = BorderStyle.FixedSingle;
      Icon68.Enabled = false;
      Icon68.Location = new Point(72, 231);
      Icon68.Name = "Icon68";
      Icon68.Size = new Size(14, 18);
      Icon68.TabIndex = 5;
      Icon68.TabStop = false;
      Icon68.Click += Icon_Click;
      // 
      // Icon69
      // 
      Icon69.BorderStyle = BorderStyle.FixedSingle;
      Icon69.Enabled = false;
      Icon69.Location = new Point(85, 231);
      Icon69.Name = "Icon69";
      Icon69.Size = new Size(14, 18);
      Icon69.TabIndex = 6;
      Icon69.TabStop = false;
      Icon69.Click += Icon_Click;
      // 
      // Icon70
      // 
      Icon70.BorderStyle = BorderStyle.FixedSingle;
      Icon70.Enabled = false;
      Icon70.Location = new Point(98, 231);
      Icon70.Name = "Icon70";
      Icon70.Size = new Size(14, 18);
      Icon70.TabIndex = 7;
      Icon70.TabStop = false;
      Icon70.Click += Icon_Click;
      // 
      // Icon71
      // 
      Icon71.BorderStyle = BorderStyle.FixedSingle;
      Icon71.Enabled = false;
      Icon71.Location = new Point(111, 231);
      Icon71.Name = "Icon71";
      Icon71.Size = new Size(14, 18);
      Icon71.TabIndex = 8;
      Icon71.TabStop = false;
      Icon71.Click += Icon_Click;
      // 
      // Icon72
      // 
      Icon72.BorderStyle = BorderStyle.FixedSingle;
      Icon72.Enabled = false;
      Icon72.Location = new Point(124, 231);
      Icon72.Name = "Icon72";
      Icon72.Size = new Size(14, 18);
      Icon72.TabIndex = 9;
      Icon72.TabStop = false;
      Icon72.Click += Icon_Click;
      // 
      // Icon73
      // 
      Icon73.BorderStyle = BorderStyle.FixedSingle;
      Icon73.Enabled = false;
      Icon73.Location = new Point(137, 231);
      Icon73.Name = "Icon73";
      Icon73.Size = new Size(14, 18);
      Icon73.TabIndex = 10;
      Icon73.TabStop = false;
      Icon73.Click += Icon_Click;
      // 
      // Icon74
      // 
      Icon74.BorderStyle = BorderStyle.FixedSingle;
      Icon74.Enabled = false;
      Icon74.Location = new Point(150, 231);
      Icon74.Name = "Icon74";
      Icon74.Size = new Size(14, 18);
      Icon74.TabIndex = 552;
      Icon74.TabStop = false;
      Icon74.Click += Icon_Click;
      // 
      // Icon75
      // 
      Icon75.BorderStyle = BorderStyle.FixedSingle;
      Icon75.Enabled = false;
      Icon75.Location = new Point(163, 231);
      Icon75.Name = "Icon75";
      Icon75.Size = new Size(14, 18);
      Icon75.TabIndex = 553;
      Icon75.TabStop = false;
      Icon75.Click += Icon_Click;
      // 
      // Icon76
      // 
      Icon76.BorderStyle = BorderStyle.FixedSingle;
      Icon76.Enabled = false;
      Icon76.Location = new Point(176, 231);
      Icon76.Name = "Icon76";
      Icon76.Size = new Size(14, 18);
      Icon76.TabIndex = 554;
      Icon76.TabStop = false;
      Icon76.Click += Icon_Click;
      // 
      // Icon77
      // 
      Icon77.BorderStyle = BorderStyle.FixedSingle;
      Icon77.Enabled = false;
      Icon77.Location = new Point(189, 231);
      Icon77.Name = "Icon77";
      Icon77.Size = new Size(14, 18);
      Icon77.TabIndex = 555;
      Icon77.TabStop = false;
      Icon77.Click += Icon_Click;
      // 
      // Icon78
      // 
      Icon78.BorderStyle = BorderStyle.FixedSingle;
      Icon78.Enabled = false;
      Icon78.Location = new Point(202, 231);
      Icon78.Name = "Icon78";
      Icon78.Size = new Size(14, 18);
      Icon78.TabIndex = 556;
      Icon78.TabStop = false;
      Icon78.Click += Icon_Click;
      // 
      // Icon79
      // 
      Icon79.BorderStyle = BorderStyle.FixedSingle;
      Icon79.Enabled = false;
      Icon79.Location = new Point(215, 231);
      Icon79.Name = "Icon79";
      Icon79.Size = new Size(14, 18);
      Icon79.TabIndex = 557;
      Icon79.TabStop = false;
      Icon79.Click += Icon_Click;
      // 
      // Icon80
      // 
      Icon80.BorderStyle = BorderStyle.FixedSingle;
      Icon80.Enabled = false;
      Icon80.Location = new Point(20, 248);
      Icon80.Name = "Icon80";
      Icon80.Size = new Size(14, 18);
      Icon80.TabIndex = 558;
      Icon80.TabStop = false;
      Icon80.Click += Icon_Click;
      // 
      // Icon81
      // 
      Icon81.BorderStyle = BorderStyle.FixedSingle;
      Icon81.Enabled = false;
      Icon81.Location = new Point(33, 248);
      Icon81.Name = "Icon81";
      Icon81.Size = new Size(14, 18);
      Icon81.TabIndex = 559;
      Icon81.TabStop = false;
      Icon81.Click += Icon_Click;
      // 
      // Icon82
      // 
      Icon82.BorderStyle = BorderStyle.FixedSingle;
      Icon82.Enabled = false;
      Icon82.Location = new Point(46, 248);
      Icon82.Name = "Icon82";
      Icon82.Size = new Size(14, 18);
      Icon82.TabIndex = 560;
      Icon82.TabStop = false;
      Icon82.Click += Icon_Click;
      // 
      // Icon83
      // 
      Icon83.BorderStyle = BorderStyle.FixedSingle;
      Icon83.Enabled = false;
      Icon83.Location = new Point(59, 248);
      Icon83.Name = "Icon83";
      Icon83.Size = new Size(14, 18);
      Icon83.TabIndex = 561;
      Icon83.TabStop = false;
      Icon83.Click += Icon_Click;
      // 
      // Icon84
      // 
      Icon84.BorderStyle = BorderStyle.FixedSingle;
      Icon84.Enabled = false;
      Icon84.Location = new Point(72, 248);
      Icon84.Name = "Icon84";
      Icon84.Size = new Size(14, 18);
      Icon84.TabIndex = 562;
      Icon84.TabStop = false;
      Icon84.Click += Icon_Click;
      // 
      // Icon85
      // 
      Icon85.BorderStyle = BorderStyle.FixedSingle;
      Icon85.Enabled = false;
      Icon85.Location = new Point(85, 248);
      Icon85.Name = "Icon85";
      Icon85.Size = new Size(14, 18);
      Icon85.TabIndex = 563;
      Icon85.TabStop = false;
      Icon85.Click += Icon_Click;
      // 
      // Icon86
      // 
      Icon86.BorderStyle = BorderStyle.FixedSingle;
      Icon86.Enabled = false;
      Icon86.Location = new Point(98, 248);
      Icon86.Name = "Icon86";
      Icon86.Size = new Size(14, 18);
      Icon86.TabIndex = 564;
      Icon86.TabStop = false;
      Icon86.Click += Icon_Click;
      // 
      // Icon87
      // 
      Icon87.BorderStyle = BorderStyle.FixedSingle;
      Icon87.Enabled = false;
      Icon87.Location = new Point(111, 248);
      Icon87.Name = "Icon87";
      Icon87.Size = new Size(14, 18);
      Icon87.TabIndex = 565;
      Icon87.TabStop = false;
      Icon87.Click += Icon_Click;
      // 
      // Icon88
      // 
      Icon88.BorderStyle = BorderStyle.FixedSingle;
      Icon88.Enabled = false;
      Icon88.Location = new Point(124, 248);
      Icon88.Name = "Icon88";
      Icon88.Size = new Size(14, 18);
      Icon88.TabIndex = 566;
      Icon88.TabStop = false;
      Icon88.Click += Icon_Click;
      // 
      // Icon89
      // 
      Icon89.BorderStyle = BorderStyle.FixedSingle;
      Icon89.Enabled = false;
      Icon89.Location = new Point(137, 248);
      Icon89.Name = "Icon89";
      Icon89.Size = new Size(14, 18);
      Icon89.TabIndex = 567;
      Icon89.TabStop = false;
      Icon89.Click += Icon_Click;
      // 
      // Icon90
      // 
      Icon90.BorderStyle = BorderStyle.FixedSingle;
      Icon90.Enabled = false;
      Icon90.Location = new Point(150, 248);
      Icon90.Name = "Icon90";
      Icon90.Size = new Size(14, 18);
      Icon90.TabIndex = 568;
      Icon90.TabStop = false;
      Icon90.Click += Icon_Click;
      // 
      // Icon91
      // 
      Icon91.BorderStyle = BorderStyle.FixedSingle;
      Icon91.Enabled = false;
      Icon91.Location = new Point(163, 248);
      Icon91.Name = "Icon91";
      Icon91.Size = new Size(14, 18);
      Icon91.TabIndex = 569;
      Icon91.TabStop = false;
      Icon91.Click += Icon_Click;
      // 
      // Icon92
      // 
      Icon92.BorderStyle = BorderStyle.FixedSingle;
      Icon92.Enabled = false;
      Icon92.Location = new Point(176, 248);
      Icon92.Name = "Icon92";
      Icon92.Size = new Size(14, 18);
      Icon92.TabIndex = 570;
      Icon92.TabStop = false;
      Icon92.Click += Icon_Click;
      // 
      // Icon93
      // 
      Icon93.BorderStyle = BorderStyle.FixedSingle;
      Icon93.Enabled = false;
      Icon93.Location = new Point(189, 248);
      Icon93.Name = "Icon93";
      Icon93.Size = new Size(14, 18);
      Icon93.TabIndex = 571;
      Icon93.TabStop = false;
      Icon93.Click += Icon_Click;
      // 
      // Icon94
      // 
      Icon94.BorderStyle = BorderStyle.FixedSingle;
      Icon94.Enabled = false;
      Icon94.Location = new Point(202, 248);
      Icon94.Name = "Icon94";
      Icon94.Size = new Size(14, 18);
      Icon94.TabIndex = 572;
      Icon94.TabStop = false;
      Icon94.Click += Icon_Click;
      // 
      // Icon95
      // 
      Icon95.BorderStyle = BorderStyle.FixedSingle;
      Icon95.Enabled = false;
      Icon95.Location = new Point(215, 248);
      Icon95.Name = "Icon95";
      Icon95.Size = new Size(14, 18);
      Icon95.TabIndex = 573;
      Icon95.TabStop = false;
      Icon95.Click += Icon_Click;
      // 
      // Icon96
      // 
      Icon96.BorderStyle = BorderStyle.FixedSingle;
      Icon96.Enabled = false;
      Icon96.Location = new Point(20, 265);
      Icon96.Name = "Icon96";
      Icon96.Size = new Size(14, 18);
      Icon96.TabIndex = 1;
      Icon96.TabStop = false;
      Icon96.Click += Icon_Click;
      // 
      // Icon97
      // 
      Icon97.BorderStyle = BorderStyle.FixedSingle;
      Icon97.Enabled = false;
      Icon97.Location = new Point(33, 265);
      Icon97.Name = "Icon97";
      Icon97.Size = new Size(14, 18);
      Icon97.TabIndex = 2;
      Icon97.TabStop = false;
      Icon97.Click += Icon_Click;
      // 
      // Icon98
      // 
      Icon98.BorderStyle = BorderStyle.FixedSingle;
      Icon98.Enabled = false;
      Icon98.Location = new Point(46, 265);
      Icon98.Name = "Icon98";
      Icon98.Size = new Size(14, 18);
      Icon98.TabIndex = 3;
      Icon98.TabStop = false;
      Icon98.Click += Icon_Click;
      // 
      // Icon99
      // 
      Icon99.BorderStyle = BorderStyle.FixedSingle;
      Icon99.Enabled = false;
      Icon99.Location = new Point(59, 265);
      Icon99.Name = "Icon99";
      Icon99.Size = new Size(14, 18);
      Icon99.TabIndex = 4;
      Icon99.TabStop = false;
      Icon99.Click += Icon_Click;
      // 
      // Icon100
      // 
      Icon100.BorderStyle = BorderStyle.FixedSingle;
      Icon100.Enabled = false;
      Icon100.Location = new Point(72, 265);
      Icon100.Name = "Icon100";
      Icon100.Size = new Size(14, 18);
      Icon100.TabIndex = 5;
      Icon100.TabStop = false;
      Icon100.Click += Icon_Click;
      // 
      // Icon101
      // 
      Icon101.BorderStyle = BorderStyle.FixedSingle;
      Icon101.Enabled = false;
      Icon101.Location = new Point(85, 265);
      Icon101.Name = "Icon101";
      Icon101.Size = new Size(14, 18);
      Icon101.TabIndex = 6;
      Icon101.TabStop = false;
      Icon101.Click += Icon_Click;
      // 
      // Icon102
      // 
      Icon102.BorderStyle = BorderStyle.FixedSingle;
      Icon102.Enabled = false;
      Icon102.Location = new Point(98, 265);
      Icon102.Name = "Icon102";
      Icon102.Size = new Size(14, 18);
      Icon102.TabIndex = 7;
      Icon102.TabStop = false;
      Icon102.Click += Icon_Click;
      // 
      // Icon103
      // 
      Icon103.BorderStyle = BorderStyle.FixedSingle;
      Icon103.Enabled = false;
      Icon103.Location = new Point(111, 265);
      Icon103.Name = "Icon103";
      Icon103.Size = new Size(14, 18);
      Icon103.TabIndex = 8;
      Icon103.TabStop = false;
      Icon103.Click += Icon_Click;
      // 
      // Icon104
      // 
      Icon104.BorderStyle = BorderStyle.FixedSingle;
      Icon104.Enabled = false;
      Icon104.Location = new Point(124, 265);
      Icon104.Name = "Icon104";
      Icon104.Size = new Size(14, 18);
      Icon104.TabIndex = 9;
      Icon104.TabStop = false;
      Icon104.Click += Icon_Click;
      // 
      // Icon105
      // 
      Icon105.BorderStyle = BorderStyle.FixedSingle;
      Icon105.Enabled = false;
      Icon105.Location = new Point(137, 265);
      Icon105.Name = "Icon105";
      Icon105.Size = new Size(14, 18);
      Icon105.TabIndex = 10;
      Icon105.TabStop = false;
      Icon105.Click += Icon_Click;
      // 
      // Icon106
      // 
      Icon106.BorderStyle = BorderStyle.FixedSingle;
      Icon106.Enabled = false;
      Icon106.Location = new Point(150, 265);
      Icon106.Name = "Icon106";
      Icon106.Size = new Size(14, 18);
      Icon106.TabIndex = 552;
      Icon106.TabStop = false;
      Icon106.Click += Icon_Click;
      // 
      // Icon107
      // 
      Icon107.BorderStyle = BorderStyle.FixedSingle;
      Icon107.Enabled = false;
      Icon107.Location = new Point(163, 265);
      Icon107.Name = "Icon107";
      Icon107.Size = new Size(14, 18);
      Icon107.TabIndex = 553;
      Icon107.TabStop = false;
      Icon107.Click += Icon_Click;
      // 
      // Icon108
      // 
      Icon108.BorderStyle = BorderStyle.FixedSingle;
      Icon108.Enabled = false;
      Icon108.Location = new Point(176, 265);
      Icon108.Name = "Icon108";
      Icon108.Size = new Size(14, 18);
      Icon108.TabIndex = 554;
      Icon108.TabStop = false;
      Icon108.Click += Icon_Click;
      // 
      // Icon109
      // 
      Icon109.BorderStyle = BorderStyle.FixedSingle;
      Icon109.Enabled = false;
      Icon109.Location = new Point(189, 265);
      Icon109.Name = "Icon109";
      Icon109.Size = new Size(14, 18);
      Icon109.TabIndex = 555;
      Icon109.TabStop = false;
      Icon109.Click += Icon_Click;
      // 
      // Icon110
      // 
      Icon110.BorderStyle = BorderStyle.FixedSingle;
      Icon110.Enabled = false;
      Icon110.Location = new Point(202, 265);
      Icon110.Name = "Icon110";
      Icon110.Size = new Size(14, 18);
      Icon110.TabIndex = 556;
      Icon110.TabStop = false;
      Icon110.Click += Icon_Click;
      // 
      // Icon111
      // 
      Icon111.BorderStyle = BorderStyle.FixedSingle;
      Icon111.Enabled = false;
      Icon111.Location = new Point(215, 265);
      Icon111.Name = "Icon111";
      Icon111.Size = new Size(14, 18);
      Icon111.TabIndex = 557;
      Icon111.TabStop = false;
      Icon111.Click += Icon_Click;
      // 
      // Icon112
      // 
      Icon112.BorderStyle = BorderStyle.FixedSingle;
      Icon112.Enabled = false;
      Icon112.Location = new Point(20, 282);
      Icon112.Name = "Icon112";
      Icon112.Size = new Size(14, 18);
      Icon112.TabIndex = 558;
      Icon112.TabStop = false;
      Icon112.Click += Icon_Click;
      // 
      // Icon113
      // 
      Icon113.BorderStyle = BorderStyle.FixedSingle;
      Icon113.Enabled = false;
      Icon113.Location = new Point(33, 282);
      Icon113.Name = "Icon113";
      Icon113.Size = new Size(14, 18);
      Icon113.TabIndex = 559;
      Icon113.TabStop = false;
      Icon113.Click += Icon_Click;
      // 
      // Icon114
      // 
      Icon114.BorderStyle = BorderStyle.FixedSingle;
      Icon114.Enabled = false;
      Icon114.Location = new Point(46, 282);
      Icon114.Name = "Icon114";
      Icon114.Size = new Size(14, 18);
      Icon114.TabIndex = 560;
      Icon114.TabStop = false;
      Icon114.Click += Icon_Click;
      // 
      // Icon115
      // 
      Icon115.BorderStyle = BorderStyle.FixedSingle;
      Icon115.Enabled = false;
      Icon115.Location = new Point(59, 282);
      Icon115.Name = "Icon115";
      Icon115.Size = new Size(14, 18);
      Icon115.TabIndex = 561;
      Icon115.TabStop = false;
      Icon115.Click += Icon_Click;
      // 
      // Icon116
      // 
      Icon116.BorderStyle = BorderStyle.FixedSingle;
      Icon116.Enabled = false;
      Icon116.Location = new Point(72, 282);
      Icon116.Name = "Icon116";
      Icon116.Size = new Size(14, 18);
      Icon116.TabIndex = 562;
      Icon116.TabStop = false;
      Icon116.Click += Icon_Click;
      // 
      // Icon117
      // 
      Icon117.BorderStyle = BorderStyle.FixedSingle;
      Icon117.Enabled = false;
      Icon117.Location = new Point(85, 282);
      Icon117.Name = "Icon117";
      Icon117.Size = new Size(14, 18);
      Icon117.TabIndex = 563;
      Icon117.TabStop = false;
      Icon117.Click += Icon_Click;
      // 
      // Icon118
      // 
      Icon118.BorderStyle = BorderStyle.FixedSingle;
      Icon118.Enabled = false;
      Icon118.Location = new Point(98, 282);
      Icon118.Name = "Icon118";
      Icon118.Size = new Size(14, 18);
      Icon118.TabIndex = 564;
      Icon118.TabStop = false;
      Icon118.Click += Icon_Click;
      // 
      // Icon119
      // 
      Icon119.BorderStyle = BorderStyle.FixedSingle;
      Icon119.Enabled = false;
      Icon119.Location = new Point(111, 282);
      Icon119.Name = "Icon119";
      Icon119.Size = new Size(14, 18);
      Icon119.TabIndex = 565;
      Icon119.TabStop = false;
      Icon119.Click += Icon_Click;
      // 
      // Icon120
      // 
      Icon120.BorderStyle = BorderStyle.FixedSingle;
      Icon120.Enabled = false;
      Icon120.Location = new Point(124, 282);
      Icon120.Name = "Icon120";
      Icon120.Size = new Size(14, 18);
      Icon120.TabIndex = 566;
      Icon120.TabStop = false;
      Icon120.Click += Icon_Click;
      // 
      // Icon121
      // 
      Icon121.BorderStyle = BorderStyle.FixedSingle;
      Icon121.Enabled = false;
      Icon121.Location = new Point(137, 282);
      Icon121.Name = "Icon121";
      Icon121.Size = new Size(14, 18);
      Icon121.TabIndex = 567;
      Icon121.TabStop = false;
      Icon121.Click += Icon_Click;
      // 
      // Icon122
      // 
      Icon122.BorderStyle = BorderStyle.FixedSingle;
      Icon122.Enabled = false;
      Icon122.Location = new Point(150, 282);
      Icon122.Name = "Icon122";
      Icon122.Size = new Size(14, 18);
      Icon122.TabIndex = 568;
      Icon122.TabStop = false;
      Icon122.Click += Icon_Click;
      // 
      // Icon123
      // 
      Icon123.BorderStyle = BorderStyle.FixedSingle;
      Icon123.Enabled = false;
      Icon123.Location = new Point(163, 282);
      Icon123.Name = "Icon123";
      Icon123.Size = new Size(14, 18);
      Icon123.TabIndex = 569;
      Icon123.TabStop = false;
      Icon123.Click += Icon_Click;
      // 
      // Icon124
      // 
      Icon124.BorderStyle = BorderStyle.FixedSingle;
      Icon124.Enabled = false;
      Icon124.Location = new Point(176, 282);
      Icon124.Name = "Icon124";
      Icon124.Size = new Size(14, 18);
      Icon124.TabIndex = 570;
      Icon124.TabStop = false;
      Icon124.Click += Icon_Click;
      // 
      // Icon125
      // 
      Icon125.BorderStyle = BorderStyle.FixedSingle;
      Icon125.Enabled = false;
      Icon125.Location = new Point(189, 282);
      Icon125.Name = "Icon125";
      Icon125.Size = new Size(14, 18);
      Icon125.TabIndex = 571;
      Icon125.TabStop = false;
      Icon125.Click += Icon_Click;
      // 
      // Icon126
      // 
      Icon126.BorderStyle = BorderStyle.FixedSingle;
      Icon126.Enabled = false;
      Icon126.Location = new Point(202, 282);
      Icon126.Name = "Icon126";
      Icon126.Size = new Size(14, 18);
      Icon126.TabIndex = 572;
      Icon126.TabStop = false;
      Icon126.Click += Icon_Click;
      // 
      // Icon127
      // 
      Icon127.BorderStyle = BorderStyle.FixedSingle;
      Icon127.Enabled = false;
      Icon127.Location = new Point(215, 282);
      Icon127.Name = "Icon127";
      Icon127.Size = new Size(14, 18);
      Icon127.TabIndex = 573;
      Icon127.TabStop = false;
      Icon127.Click += Icon_Click;
      // 
      // Icon128
      // 
      Icon128.BorderStyle = BorderStyle.FixedSingle;
      Icon128.Enabled = false;
      Icon128.Location = new Point(20, 299);
      Icon128.Name = "Icon128";
      Icon128.Size = new Size(14, 18);
      Icon128.TabIndex = 1;
      Icon128.TabStop = false;
      Icon128.Click += Icon_Click;
      // 
      // Icon129
      // 
      Icon129.BorderStyle = BorderStyle.FixedSingle;
      Icon129.Enabled = false;
      Icon129.Location = new Point(33, 299);
      Icon129.Name = "Icon129";
      Icon129.Size = new Size(14, 18);
      Icon129.TabIndex = 2;
      Icon129.TabStop = false;
      Icon129.Click += Icon_Click;
      // 
      // Icon130
      // 
      Icon130.BorderStyle = BorderStyle.FixedSingle;
      Icon130.Enabled = false;
      Icon130.Location = new Point(46, 299);
      Icon130.Name = "Icon130";
      Icon130.Size = new Size(14, 18);
      Icon130.TabIndex = 3;
      Icon130.TabStop = false;
      Icon130.Click += Icon_Click;
      // 
      // Icon131
      // 
      Icon131.BorderStyle = BorderStyle.FixedSingle;
      Icon131.Enabled = false;
      Icon131.Location = new Point(59, 299);
      Icon131.Name = "Icon131";
      Icon131.Size = new Size(14, 18);
      Icon131.TabIndex = 4;
      Icon131.TabStop = false;
      Icon131.Click += Icon_Click;
      // 
      // Icon132
      // 
      Icon132.BorderStyle = BorderStyle.FixedSingle;
      Icon132.Enabled = false;
      Icon132.Location = new Point(72, 299);
      Icon132.Name = "Icon132";
      Icon132.Size = new Size(14, 18);
      Icon132.TabIndex = 5;
      Icon132.TabStop = false;
      Icon132.Click += Icon_Click;
      // 
      // Icon133
      // 
      Icon133.BorderStyle = BorderStyle.FixedSingle;
      Icon133.Enabled = false;
      Icon133.Location = new Point(85, 299);
      Icon133.Name = "Icon133";
      Icon133.Size = new Size(14, 18);
      Icon133.TabIndex = 6;
      Icon133.TabStop = false;
      Icon133.Click += Icon_Click;
      // 
      // Icon134
      // 
      Icon134.BorderStyle = BorderStyle.FixedSingle;
      Icon134.Enabled = false;
      Icon134.Location = new Point(98, 299);
      Icon134.Name = "Icon134";
      Icon134.Size = new Size(14, 18);
      Icon134.TabIndex = 7;
      Icon134.TabStop = false;
      Icon134.Click += Icon_Click;
      // 
      // Icon135
      // 
      Icon135.BorderStyle = BorderStyle.FixedSingle;
      Icon135.Enabled = false;
      Icon135.Location = new Point(111, 299);
      Icon135.Name = "Icon135";
      Icon135.Size = new Size(14, 18);
      Icon135.TabIndex = 8;
      Icon135.TabStop = false;
      Icon135.Click += Icon_Click;
      // 
      // Icon136
      // 
      Icon136.BorderStyle = BorderStyle.FixedSingle;
      Icon136.Enabled = false;
      Icon136.Location = new Point(124, 299);
      Icon136.Name = "Icon136";
      Icon136.Size = new Size(14, 18);
      Icon136.TabIndex = 9;
      Icon136.TabStop = false;
      Icon136.Click += Icon_Click;
      // 
      // Icon137
      // 
      Icon137.BorderStyle = BorderStyle.FixedSingle;
      Icon137.Enabled = false;
      Icon137.Location = new Point(137, 299);
      Icon137.Name = "Icon137";
      Icon137.Size = new Size(14, 18);
      Icon137.TabIndex = 10;
      Icon137.TabStop = false;
      Icon137.Click += Icon_Click;
      // 
      // Icon138
      // 
      Icon138.BorderStyle = BorderStyle.FixedSingle;
      Icon138.Enabled = false;
      Icon138.Location = new Point(150, 299);
      Icon138.Name = "Icon138";
      Icon138.Size = new Size(14, 18);
      Icon138.TabIndex = 552;
      Icon138.TabStop = false;
      Icon138.Click += Icon_Click;
      // 
      // Icon139
      // 
      Icon139.BorderStyle = BorderStyle.FixedSingle;
      Icon139.Enabled = false;
      Icon139.Location = new Point(163, 299);
      Icon139.Name = "Icon139";
      Icon139.Size = new Size(14, 18);
      Icon139.TabIndex = 553;
      Icon139.TabStop = false;
      Icon139.Click += Icon_Click;
      // 
      // Icon140
      // 
      Icon140.BorderStyle = BorderStyle.FixedSingle;
      Icon140.Enabled = false;
      Icon140.Location = new Point(176, 299);
      Icon140.Name = "Icon140";
      Icon140.Size = new Size(14, 18);
      Icon140.TabIndex = 554;
      Icon140.TabStop = false;
      Icon140.Click += Icon_Click;
      // 
      // Icon141
      // 
      Icon141.BorderStyle = BorderStyle.FixedSingle;
      Icon141.Enabled = false;
      Icon141.Location = new Point(189, 299);
      Icon141.Name = "Icon141";
      Icon141.Size = new Size(14, 18);
      Icon141.TabIndex = 555;
      Icon141.TabStop = false;
      Icon141.Click += Icon_Click;
      // 
      // Icon142
      // 
      Icon142.BorderStyle = BorderStyle.FixedSingle;
      Icon142.Enabled = false;
      Icon142.Location = new Point(202, 299);
      Icon142.Name = "Icon142";
      Icon142.Size = new Size(14, 18);
      Icon142.TabIndex = 556;
      Icon142.TabStop = false;
      Icon142.Click += Icon_Click;
      // 
      // Icon143
      // 
      Icon143.BorderStyle = BorderStyle.FixedSingle;
      Icon143.Enabled = false;
      Icon143.Location = new Point(215, 299);
      Icon143.Name = "Icon143";
      Icon143.Size = new Size(14, 18);
      Icon143.TabIndex = 557;
      Icon143.TabStop = false;
      Icon143.Click += Icon_Click;
      // 
      // Icon144
      // 
      Icon144.BorderStyle = BorderStyle.FixedSingle;
      Icon144.Enabled = false;
      Icon144.Location = new Point(20, 316);
      Icon144.Name = "Icon144";
      Icon144.Size = new Size(14, 18);
      Icon144.TabIndex = 558;
      Icon144.TabStop = false;
      Icon144.Click += Icon_Click;
      // 
      // Icon145
      // 
      Icon145.BorderStyle = BorderStyle.FixedSingle;
      Icon145.Enabled = false;
      Icon145.Location = new Point(33, 316);
      Icon145.Name = "Icon145";
      Icon145.Size = new Size(14, 18);
      Icon145.TabIndex = 559;
      Icon145.TabStop = false;
      Icon145.Click += Icon_Click;
      // 
      // Icon146
      // 
      Icon146.BorderStyle = BorderStyle.FixedSingle;
      Icon146.Enabled = false;
      Icon146.Location = new Point(46, 316);
      Icon146.Name = "Icon146";
      Icon146.Size = new Size(14, 18);
      Icon146.TabIndex = 560;
      Icon146.TabStop = false;
      Icon146.Click += Icon_Click;
      // 
      // Icon147
      // 
      Icon147.BorderStyle = BorderStyle.FixedSingle;
      Icon147.Enabled = false;
      Icon147.Location = new Point(59, 316);
      Icon147.Name = "Icon147";
      Icon147.Size = new Size(14, 18);
      Icon147.TabIndex = 561;
      Icon147.TabStop = false;
      Icon147.Click += Icon_Click;
      // 
      // Icon148
      // 
      Icon148.BorderStyle = BorderStyle.FixedSingle;
      Icon148.Enabled = false;
      Icon148.Location = new Point(72, 316);
      Icon148.Name = "Icon148";
      Icon148.Size = new Size(14, 18);
      Icon148.TabIndex = 562;
      Icon148.TabStop = false;
      Icon148.Click += Icon_Click;
      // 
      // Icon149
      // 
      Icon149.BorderStyle = BorderStyle.FixedSingle;
      Icon149.Enabled = false;
      Icon149.Location = new Point(85, 316);
      Icon149.Name = "Icon149";
      Icon149.Size = new Size(14, 18);
      Icon149.TabIndex = 563;
      Icon149.TabStop = false;
      Icon149.Click += Icon_Click;
      // 
      // Icon150
      // 
      Icon150.BorderStyle = BorderStyle.FixedSingle;
      Icon150.Enabled = false;
      Icon150.Location = new Point(98, 316);
      Icon150.Name = "Icon150";
      Icon150.Size = new Size(14, 18);
      Icon150.TabIndex = 564;
      Icon150.TabStop = false;
      Icon150.Click += Icon_Click;
      // 
      // Icon151
      // 
      Icon151.BorderStyle = BorderStyle.FixedSingle;
      Icon151.Enabled = false;
      Icon151.Location = new Point(111, 316);
      Icon151.Name = "Icon151";
      Icon151.Size = new Size(14, 18);
      Icon151.TabIndex = 565;
      Icon151.TabStop = false;
      Icon151.Click += Icon_Click;
      // 
      // Icon152
      // 
      Icon152.BorderStyle = BorderStyle.FixedSingle;
      Icon152.Enabled = false;
      Icon152.Location = new Point(124, 316);
      Icon152.Name = "Icon152";
      Icon152.Size = new Size(14, 18);
      Icon152.TabIndex = 566;
      Icon152.TabStop = false;
      Icon152.Click += Icon_Click;
      // 
      // Icon153
      // 
      Icon153.BorderStyle = BorderStyle.FixedSingle;
      Icon153.Enabled = false;
      Icon153.Location = new Point(137, 316);
      Icon153.Name = "Icon153";
      Icon153.Size = new Size(14, 18);
      Icon153.TabIndex = 567;
      Icon153.TabStop = false;
      Icon153.Click += Icon_Click;
      // 
      // Icon154
      // 
      Icon154.BorderStyle = BorderStyle.FixedSingle;
      Icon154.Enabled = false;
      Icon154.Location = new Point(150, 316);
      Icon154.Name = "Icon154";
      Icon154.Size = new Size(14, 18);
      Icon154.TabIndex = 568;
      Icon154.TabStop = false;
      Icon154.Click += Icon_Click;
      // 
      // Icon155
      // 
      Icon155.BorderStyle = BorderStyle.FixedSingle;
      Icon155.Enabled = false;
      Icon155.Location = new Point(163, 316);
      Icon155.Name = "Icon155";
      Icon155.Size = new Size(14, 18);
      Icon155.TabIndex = 569;
      Icon155.TabStop = false;
      Icon155.Click += Icon_Click;
      // 
      // Icon156
      // 
      Icon156.BorderStyle = BorderStyle.FixedSingle;
      Icon156.Enabled = false;
      Icon156.Location = new Point(176, 316);
      Icon156.Name = "Icon156";
      Icon156.Size = new Size(14, 18);
      Icon156.TabIndex = 570;
      Icon156.TabStop = false;
      Icon156.Click += Icon_Click;
      // 
      // Icon157
      // 
      Icon157.BorderStyle = BorderStyle.FixedSingle;
      Icon157.Enabled = false;
      Icon157.Location = new Point(189, 316);
      Icon157.Name = "Icon157";
      Icon157.Size = new Size(14, 18);
      Icon157.TabIndex = 571;
      Icon157.TabStop = false;
      Icon157.Click += Icon_Click;
      // 
      // Icon158
      // 
      Icon158.BorderStyle = BorderStyle.FixedSingle;
      Icon158.Enabled = false;
      Icon158.Location = new Point(202, 316);
      Icon158.Name = "Icon158";
      Icon158.Size = new Size(14, 18);
      Icon158.TabIndex = 572;
      Icon158.TabStop = false;
      Icon158.Click += Icon_Click;
      // 
      // Icon159
      // 
      Icon159.BorderStyle = BorderStyle.FixedSingle;
      Icon159.Enabled = false;
      Icon159.Location = new Point(215, 316);
      Icon159.Name = "Icon159";
      Icon159.Size = new Size(14, 18);
      Icon159.TabIndex = 573;
      Icon159.TabStop = false;
      Icon159.Click += Icon_Click;
      // 
      // Icon160
      // 
      Icon160.BorderStyle = BorderStyle.FixedSingle;
      Icon160.Enabled = false;
      Icon160.Location = new Point(20, 333);
      Icon160.Name = "Icon160";
      Icon160.Size = new Size(14, 18);
      Icon160.TabIndex = 1;
      Icon160.TabStop = false;
      Icon160.Click += Icon_Click;
      // 
      // Icon161
      // 
      Icon161.BorderStyle = BorderStyle.FixedSingle;
      Icon161.Enabled = false;
      Icon161.Location = new Point(33, 333);
      Icon161.Name = "Icon161";
      Icon161.Size = new Size(14, 18);
      Icon161.TabIndex = 2;
      Icon161.TabStop = false;
      Icon161.Click += Icon_Click;
      // 
      // Icon162
      // 
      Icon162.BorderStyle = BorderStyle.FixedSingle;
      Icon162.Enabled = false;
      Icon162.Location = new Point(46, 333);
      Icon162.Name = "Icon162";
      Icon162.Size = new Size(14, 18);
      Icon162.TabIndex = 3;
      Icon162.TabStop = false;
      Icon162.Click += Icon_Click;
      // 
      // Icon163
      // 
      Icon163.BorderStyle = BorderStyle.FixedSingle;
      Icon163.Enabled = false;
      Icon163.Location = new Point(59, 333);
      Icon163.Name = "Icon163";
      Icon163.Size = new Size(14, 18);
      Icon163.TabIndex = 4;
      Icon163.TabStop = false;
      Icon163.Click += Icon_Click;
      // 
      // Icon164
      // 
      Icon164.BorderStyle = BorderStyle.FixedSingle;
      Icon164.Enabled = false;
      Icon164.Location = new Point(72, 333);
      Icon164.Name = "Icon164";
      Icon164.Size = new Size(14, 18);
      Icon164.TabIndex = 5;
      Icon164.TabStop = false;
      Icon164.Click += Icon_Click;
      // 
      // Icon165
      // 
      Icon165.BorderStyle = BorderStyle.FixedSingle;
      Icon165.Enabled = false;
      Icon165.Location = new Point(85, 333);
      Icon165.Name = "Icon165";
      Icon165.Size = new Size(14, 18);
      Icon165.TabIndex = 6;
      Icon165.TabStop = false;
      Icon165.Click += Icon_Click;
      // 
      // Icon166
      // 
      Icon166.BorderStyle = BorderStyle.FixedSingle;
      Icon166.Enabled = false;
      Icon166.Location = new Point(98, 333);
      Icon166.Name = "Icon166";
      Icon166.Size = new Size(14, 18);
      Icon166.TabIndex = 7;
      Icon166.TabStop = false;
      Icon166.Click += Icon_Click;
      // 
      // Icon167
      // 
      Icon167.BorderStyle = BorderStyle.FixedSingle;
      Icon167.Enabled = false;
      Icon167.Location = new Point(111, 333);
      Icon167.Name = "Icon167";
      Icon167.Size = new Size(14, 18);
      Icon167.TabIndex = 8;
      Icon167.TabStop = false;
      Icon167.Click += Icon_Click;
      // 
      // Icon168
      // 
      Icon168.BorderStyle = BorderStyle.FixedSingle;
      Icon168.Enabled = false;
      Icon168.Location = new Point(124, 333);
      Icon168.Name = "Icon168";
      Icon168.Size = new Size(14, 18);
      Icon168.TabIndex = 9;
      Icon168.TabStop = false;
      Icon168.Click += Icon_Click;
      // 
      // Icon169
      // 
      Icon169.BorderStyle = BorderStyle.FixedSingle;
      Icon169.Enabled = false;
      Icon169.Location = new Point(137, 333);
      Icon169.Name = "Icon169";
      Icon169.Size = new Size(14, 18);
      Icon169.TabIndex = 10;
      Icon169.TabStop = false;
      Icon169.Click += Icon_Click;
      // 
      // Icon170
      // 
      Icon170.BorderStyle = BorderStyle.FixedSingle;
      Icon170.Enabled = false;
      Icon170.Location = new Point(150, 333);
      Icon170.Name = "Icon170";
      Icon170.Size = new Size(14, 18);
      Icon170.TabIndex = 552;
      Icon170.TabStop = false;
      Icon170.Click += Icon_Click;
      // 
      // Icon171
      // 
      Icon171.BorderStyle = BorderStyle.FixedSingle;
      Icon171.Enabled = false;
      Icon171.Location = new Point(163, 333);
      Icon171.Name = "Icon171";
      Icon171.Size = new Size(14, 18);
      Icon171.TabIndex = 553;
      Icon171.TabStop = false;
      Icon171.Click += Icon_Click;
      // 
      // Icon172
      // 
      Icon172.BorderStyle = BorderStyle.FixedSingle;
      Icon172.Enabled = false;
      Icon172.Location = new Point(176, 333);
      Icon172.Name = "Icon172";
      Icon172.Size = new Size(14, 18);
      Icon172.TabIndex = 554;
      Icon172.TabStop = false;
      Icon172.Click += Icon_Click;
      // 
      // Icon173
      // 
      Icon173.BorderStyle = BorderStyle.FixedSingle;
      Icon173.Enabled = false;
      Icon173.Location = new Point(189, 333);
      Icon173.Name = "Icon173";
      Icon173.Size = new Size(14, 18);
      Icon173.TabIndex = 555;
      Icon173.TabStop = false;
      Icon173.Click += Icon_Click;
      // 
      // Icon174
      // 
      Icon174.BorderStyle = BorderStyle.FixedSingle;
      Icon174.Enabled = false;
      Icon174.Location = new Point(202, 333);
      Icon174.Name = "Icon174";
      Icon174.Size = new Size(14, 18);
      Icon174.TabIndex = 556;
      Icon174.TabStop = false;
      Icon174.Click += Icon_Click;
      // 
      // Icon175
      // 
      Icon175.BorderStyle = BorderStyle.FixedSingle;
      Icon175.Enabled = false;
      Icon175.Location = new Point(215, 333);
      Icon175.Name = "Icon175";
      Icon175.Size = new Size(14, 18);
      Icon175.TabIndex = 557;
      Icon175.TabStop = false;
      Icon175.Click += Icon_Click;
      // 
      // Icon176
      // 
      Icon176.BorderStyle = BorderStyle.FixedSingle;
      Icon176.Enabled = false;
      Icon176.Location = new Point(20, 350);
      Icon176.Name = "Icon176";
      Icon176.Size = new Size(14, 18);
      Icon176.TabIndex = 558;
      Icon176.TabStop = false;
      Icon176.Click += Icon_Click;
      // 
      // Icon177
      // 
      Icon177.BorderStyle = BorderStyle.FixedSingle;
      Icon177.Enabled = false;
      Icon177.Location = new Point(33, 350);
      Icon177.Name = "Icon177";
      Icon177.Size = new Size(14, 18);
      Icon177.TabIndex = 559;
      Icon177.TabStop = false;
      Icon177.Click += Icon_Click;
      // 
      // Icon178
      // 
      Icon178.BorderStyle = BorderStyle.FixedSingle;
      Icon178.Enabled = false;
      Icon178.Location = new Point(46, 350);
      Icon178.Name = "Icon178";
      Icon178.Size = new Size(14, 18);
      Icon178.TabIndex = 560;
      Icon178.TabStop = false;
      Icon178.Click += Icon_Click;
      // 
      // Icon179
      // 
      Icon179.BorderStyle = BorderStyle.FixedSingle;
      Icon179.Enabled = false;
      Icon179.Location = new Point(59, 350);
      Icon179.Name = "Icon179";
      Icon179.Size = new Size(14, 18);
      Icon179.TabIndex = 561;
      Icon179.TabStop = false;
      Icon179.Click += Icon_Click;
      // 
      // Icon180
      // 
      Icon180.BorderStyle = BorderStyle.FixedSingle;
      Icon180.Enabled = false;
      Icon180.Location = new Point(72, 350);
      Icon180.Name = "Icon180";
      Icon180.Size = new Size(14, 18);
      Icon180.TabIndex = 562;
      Icon180.TabStop = false;
      Icon180.Click += Icon_Click;
      // 
      // Icon181
      // 
      Icon181.BorderStyle = BorderStyle.FixedSingle;
      Icon181.Enabled = false;
      Icon181.Location = new Point(85, 350);
      Icon181.Name = "Icon181";
      Icon181.Size = new Size(14, 18);
      Icon181.TabIndex = 563;
      Icon181.TabStop = false;
      Icon181.Click += Icon_Click;
      // 
      // Icon182
      // 
      Icon182.BorderStyle = BorderStyle.FixedSingle;
      Icon182.Enabled = false;
      Icon182.Location = new Point(98, 350);
      Icon182.Name = "Icon182";
      Icon182.Size = new Size(14, 18);
      Icon182.TabIndex = 564;
      Icon182.TabStop = false;
      Icon182.Click += Icon_Click;
      // 
      // Icon183
      // 
      Icon183.BorderStyle = BorderStyle.FixedSingle;
      Icon183.Enabled = false;
      Icon183.Location = new Point(111, 350);
      Icon183.Name = "Icon183";
      Icon183.Size = new Size(14, 18);
      Icon183.TabIndex = 565;
      Icon183.TabStop = false;
      Icon183.Click += Icon_Click;
      // 
      // Icon184
      // 
      Icon184.BorderStyle = BorderStyle.FixedSingle;
      Icon184.Enabled = false;
      Icon184.Location = new Point(124, 350);
      Icon184.Name = "Icon184";
      Icon184.Size = new Size(14, 18);
      Icon184.TabIndex = 566;
      Icon184.TabStop = false;
      Icon184.Click += Icon_Click;
      // 
      // Icon185
      // 
      Icon185.BorderStyle = BorderStyle.FixedSingle;
      Icon185.Enabled = false;
      Icon185.Location = new Point(137, 350);
      Icon185.Name = "Icon185";
      Icon185.Size = new Size(14, 18);
      Icon185.TabIndex = 567;
      Icon185.TabStop = false;
      Icon185.Click += Icon_Click;
      // 
      // Icon186
      // 
      Icon186.BorderStyle = BorderStyle.FixedSingle;
      Icon186.Enabled = false;
      Icon186.Location = new Point(150, 350);
      Icon186.Name = "Icon186";
      Icon186.Size = new Size(14, 18);
      Icon186.TabIndex = 568;
      Icon186.TabStop = false;
      Icon186.Click += Icon_Click;
      // 
      // Icon187
      // 
      Icon187.BorderStyle = BorderStyle.FixedSingle;
      Icon187.Enabled = false;
      Icon187.Location = new Point(163, 350);
      Icon187.Name = "Icon187";
      Icon187.Size = new Size(14, 18);
      Icon187.TabIndex = 569;
      Icon187.TabStop = false;
      Icon187.Click += Icon_Click;
      // 
      // Icon188
      // 
      Icon188.BorderStyle = BorderStyle.FixedSingle;
      Icon188.Enabled = false;
      Icon188.Location = new Point(176, 350);
      Icon188.Name = "Icon188";
      Icon188.Size = new Size(14, 18);
      Icon188.TabIndex = 570;
      Icon188.TabStop = false;
      Icon188.Click += Icon_Click;
      // 
      // Icon189
      // 
      Icon189.BorderStyle = BorderStyle.FixedSingle;
      Icon189.Enabled = false;
      Icon189.Location = new Point(189, 350);
      Icon189.Name = "Icon189";
      Icon189.Size = new Size(14, 18);
      Icon189.TabIndex = 571;
      Icon189.TabStop = false;
      Icon189.Click += Icon_Click;
      // 
      // Icon190
      // 
      Icon190.BorderStyle = BorderStyle.FixedSingle;
      Icon190.Enabled = false;
      Icon190.Location = new Point(202, 350);
      Icon190.Name = "Icon190";
      Icon190.Size = new Size(14, 18);
      Icon190.TabIndex = 572;
      Icon190.TabStop = false;
      Icon190.Click += Icon_Click;
      // 
      // Icon191
      // 
      Icon191.BorderStyle = BorderStyle.FixedSingle;
      Icon191.Enabled = false;
      Icon191.Location = new Point(215, 350);
      Icon191.Name = "Icon191";
      Icon191.Size = new Size(14, 18);
      Icon191.TabIndex = 573;
      Icon191.TabStop = false;
      Icon191.Click += Icon_Click;
      // 
      // Icon192
      // 
      Icon192.BorderStyle = BorderStyle.FixedSingle;
      Icon192.Enabled = false;
      Icon192.Location = new Point(20, 367);
      Icon192.Name = "Icon192";
      Icon192.Size = new Size(14, 18);
      Icon192.TabIndex = 1;
      Icon192.TabStop = false;
      Icon192.Click += Icon_Click;
      // 
      // Icon193
      // 
      Icon193.BorderStyle = BorderStyle.FixedSingle;
      Icon193.Enabled = false;
      Icon193.Location = new Point(33, 367);
      Icon193.Name = "Icon193";
      Icon193.Size = new Size(14, 18);
      Icon193.TabIndex = 2;
      Icon193.TabStop = false;
      Icon193.Click += Icon_Click;
      // 
      // Icon194
      // 
      Icon194.BorderStyle = BorderStyle.FixedSingle;
      Icon194.Enabled = false;
      Icon194.Location = new Point(46, 367);
      Icon194.Name = "Icon194";
      Icon194.Size = new Size(14, 18);
      Icon194.TabIndex = 3;
      Icon194.TabStop = false;
      Icon194.Click += Icon_Click;
      // 
      // Icon195
      // 
      Icon195.BorderStyle = BorderStyle.FixedSingle;
      Icon195.Enabled = false;
      Icon195.Location = new Point(59, 367);
      Icon195.Name = "Icon195";
      Icon195.Size = new Size(14, 18);
      Icon195.TabIndex = 4;
      Icon195.TabStop = false;
      Icon195.Click += Icon_Click;
      // 
      // Icon196
      // 
      Icon196.BorderStyle = BorderStyle.FixedSingle;
      Icon196.Enabled = false;
      Icon196.Location = new Point(72, 367);
      Icon196.Name = "Icon196";
      Icon196.Size = new Size(14, 18);
      Icon196.TabIndex = 5;
      Icon196.TabStop = false;
      Icon196.Click += Icon_Click;
      // 
      // Icon197
      // 
      Icon197.BorderStyle = BorderStyle.FixedSingle;
      Icon197.Enabled = false;
      Icon197.Location = new Point(85, 367);
      Icon197.Name = "Icon197";
      Icon197.Size = new Size(14, 18);
      Icon197.TabIndex = 6;
      Icon197.TabStop = false;
      Icon197.Click += Icon_Click;
      // 
      // Icon198
      // 
      Icon198.BorderStyle = BorderStyle.FixedSingle;
      Icon198.Enabled = false;
      Icon198.Location = new Point(98, 367);
      Icon198.Name = "Icon198";
      Icon198.Size = new Size(14, 18);
      Icon198.TabIndex = 7;
      Icon198.TabStop = false;
      Icon198.Click += Icon_Click;
      // 
      // Icon199
      // 
      Icon199.BorderStyle = BorderStyle.FixedSingle;
      Icon199.Enabled = false;
      Icon199.Location = new Point(111, 367);
      Icon199.Name = "Icon199";
      Icon199.Size = new Size(14, 18);
      Icon199.TabIndex = 8;
      Icon199.TabStop = false;
      Icon199.Click += Icon_Click;
      // 
      // Icon200
      // 
      Icon200.BorderStyle = BorderStyle.FixedSingle;
      Icon200.Enabled = false;
      Icon200.Location = new Point(124, 367);
      Icon200.Name = "Icon200";
      Icon200.Size = new Size(14, 18);
      Icon200.TabIndex = 9;
      Icon200.TabStop = false;
      Icon200.Click += Icon_Click;
      // 
      // Icon201
      // 
      Icon201.BorderStyle = BorderStyle.FixedSingle;
      Icon201.Enabled = false;
      Icon201.Location = new Point(137, 367);
      Icon201.Name = "Icon201";
      Icon201.Size = new Size(14, 18);
      Icon201.TabIndex = 10;
      Icon201.TabStop = false;
      Icon201.Click += Icon_Click;
      // 
      // Icon202
      // 
      Icon202.BorderStyle = BorderStyle.FixedSingle;
      Icon202.Enabled = false;
      Icon202.Location = new Point(150, 367);
      Icon202.Name = "Icon202";
      Icon202.Size = new Size(14, 18);
      Icon202.TabIndex = 552;
      Icon202.TabStop = false;
      Icon202.Click += Icon_Click;
      // 
      // Icon203
      // 
      Icon203.BorderStyle = BorderStyle.FixedSingle;
      Icon203.Enabled = false;
      Icon203.Location = new Point(163, 367);
      Icon203.Name = "Icon203";
      Icon203.Size = new Size(14, 18);
      Icon203.TabIndex = 553;
      Icon203.TabStop = false;
      Icon203.Click += Icon_Click;
      // 
      // Icon204
      // 
      Icon204.BorderStyle = BorderStyle.FixedSingle;
      Icon204.Enabled = false;
      Icon204.Location = new Point(176, 367);
      Icon204.Name = "Icon204";
      Icon204.Size = new Size(14, 18);
      Icon204.TabIndex = 554;
      Icon204.TabStop = false;
      Icon204.Click += Icon_Click;
      // 
      // Icon205
      // 
      Icon205.BorderStyle = BorderStyle.FixedSingle;
      Icon205.Enabled = false;
      Icon205.Location = new Point(189, 367);
      Icon205.Name = "Icon205";
      Icon205.Size = new Size(14, 18);
      Icon205.TabIndex = 555;
      Icon205.TabStop = false;
      Icon205.Click += Icon_Click;
      // 
      // Icon206
      // 
      Icon206.BorderStyle = BorderStyle.FixedSingle;
      Icon206.Enabled = false;
      Icon206.Location = new Point(202, 367);
      Icon206.Name = "Icon206";
      Icon206.Size = new Size(14, 18);
      Icon206.TabIndex = 556;
      Icon206.TabStop = false;
      Icon206.Click += Icon_Click;
      // 
      // Icon207
      // 
      Icon207.BorderStyle = BorderStyle.FixedSingle;
      Icon207.Enabled = false;
      Icon207.Location = new Point(215, 367);
      Icon207.Name = "Icon207";
      Icon207.Size = new Size(14, 18);
      Icon207.TabIndex = 557;
      Icon207.TabStop = false;
      Icon207.Click += Icon_Click;
      // 
      // Icon208
      // 
      Icon208.BorderStyle = BorderStyle.FixedSingle;
      Icon208.Enabled = false;
      Icon208.Location = new Point(20, 384);
      Icon208.Name = "Icon208";
      Icon208.Size = new Size(14, 18);
      Icon208.TabIndex = 558;
      Icon208.TabStop = false;
      Icon208.Click += Icon_Click;
      // 
      // Icon209
      // 
      Icon209.BorderStyle = BorderStyle.FixedSingle;
      Icon209.Enabled = false;
      Icon209.Location = new Point(33, 384);
      Icon209.Name = "Icon209";
      Icon209.Size = new Size(14, 18);
      Icon209.TabIndex = 559;
      Icon209.TabStop = false;
      Icon209.Click += Icon_Click;
      // 
      // Icon210
      // 
      Icon210.BorderStyle = BorderStyle.FixedSingle;
      Icon210.Enabled = false;
      Icon210.Location = new Point(46, 384);
      Icon210.Name = "Icon210";
      Icon210.Size = new Size(14, 18);
      Icon210.TabIndex = 560;
      Icon210.TabStop = false;
      Icon210.Click += Icon_Click;
      // 
      // Icon211
      // 
      Icon211.BorderStyle = BorderStyle.FixedSingle;
      Icon211.Enabled = false;
      Icon211.Location = new Point(59, 384);
      Icon211.Name = "Icon211";
      Icon211.Size = new Size(14, 18);
      Icon211.TabIndex = 561;
      Icon211.TabStop = false;
      Icon211.Click += Icon_Click;
      // 
      // Icon212
      // 
      Icon212.BorderStyle = BorderStyle.FixedSingle;
      Icon212.Enabled = false;
      Icon212.Location = new Point(72, 384);
      Icon212.Name = "Icon212";
      Icon212.Size = new Size(14, 18);
      Icon212.TabIndex = 562;
      Icon212.TabStop = false;
      Icon212.Click += Icon_Click;
      // 
      // Icon213
      // 
      Icon213.BorderStyle = BorderStyle.FixedSingle;
      Icon213.Enabled = false;
      Icon213.Location = new Point(85, 384);
      Icon213.Name = "Icon213";
      Icon213.Size = new Size(14, 18);
      Icon213.TabIndex = 563;
      Icon213.TabStop = false;
      Icon213.Click += Icon_Click;
      // 
      // Icon214
      // 
      Icon214.BorderStyle = BorderStyle.FixedSingle;
      Icon214.Enabled = false;
      Icon214.Location = new Point(98, 384);
      Icon214.Name = "Icon214";
      Icon214.Size = new Size(14, 18);
      Icon214.TabIndex = 564;
      Icon214.TabStop = false;
      Icon214.Click += Icon_Click;
      // 
      // Icon215
      // 
      Icon215.BorderStyle = BorderStyle.FixedSingle;
      Icon215.Enabled = false;
      Icon215.Location = new Point(111, 384);
      Icon215.Name = "Icon215";
      Icon215.Size = new Size(14, 18);
      Icon215.TabIndex = 565;
      Icon215.TabStop = false;
      Icon215.Click += Icon_Click;
      // 
      // Icon216
      // 
      Icon216.BorderStyle = BorderStyle.FixedSingle;
      Icon216.Enabled = false;
      Icon216.Location = new Point(124, 384);
      Icon216.Name = "Icon216";
      Icon216.Size = new Size(14, 18);
      Icon216.TabIndex = 566;
      Icon216.TabStop = false;
      Icon216.Click += Icon_Click;
      // 
      // Icon217
      // 
      Icon217.BorderStyle = BorderStyle.FixedSingle;
      Icon217.Enabled = false;
      Icon217.Location = new Point(137, 384);
      Icon217.Name = "Icon217";
      Icon217.Size = new Size(14, 18);
      Icon217.TabIndex = 567;
      Icon217.TabStop = false;
      Icon217.Click += Icon_Click;
      // 
      // Icon218
      // 
      Icon218.BorderStyle = BorderStyle.FixedSingle;
      Icon218.Enabled = false;
      Icon218.Location = new Point(150, 384);
      Icon218.Name = "Icon218";
      Icon218.Size = new Size(14, 18);
      Icon218.TabIndex = 568;
      Icon218.TabStop = false;
      Icon218.Click += Icon_Click;
      // 
      // Icon219
      // 
      Icon219.BorderStyle = BorderStyle.FixedSingle;
      Icon219.Enabled = false;
      Icon219.Location = new Point(163, 384);
      Icon219.Name = "Icon219";
      Icon219.Size = new Size(14, 18);
      Icon219.TabIndex = 569;
      Icon219.TabStop = false;
      Icon219.Click += Icon_Click;
      // 
      // Icon220
      // 
      Icon220.BorderStyle = BorderStyle.FixedSingle;
      Icon220.Enabled = false;
      Icon220.Location = new Point(176, 384);
      Icon220.Name = "Icon220";
      Icon220.Size = new Size(14, 18);
      Icon220.TabIndex = 570;
      Icon220.TabStop = false;
      Icon220.Click += Icon_Click;
      // 
      // Icon221
      // 
      Icon221.BorderStyle = BorderStyle.FixedSingle;
      Icon221.Enabled = false;
      Icon221.Location = new Point(189, 384);
      Icon221.Name = "Icon221";
      Icon221.Size = new Size(14, 18);
      Icon221.TabIndex = 571;
      Icon221.TabStop = false;
      Icon221.Click += Icon_Click;
      // 
      // Icon222
      // 
      Icon222.BorderStyle = BorderStyle.FixedSingle;
      Icon222.Enabled = false;
      Icon222.Location = new Point(202, 384);
      Icon222.Name = "Icon222";
      Icon222.Size = new Size(14, 18);
      Icon222.TabIndex = 572;
      Icon222.TabStop = false;
      Icon222.Click += Icon_Click;
      // 
      // Icon223
      // 
      Icon223.BorderStyle = BorderStyle.FixedSingle;
      Icon223.Enabled = false;
      Icon223.Location = new Point(215, 384);
      Icon223.Name = "Icon223";
      Icon223.Size = new Size(14, 18);
      Icon223.TabIndex = 573;
      Icon223.TabStop = false;
      Icon223.Click += Icon_Click;
      // 
      // Icon224
      // 
      Icon224.BorderStyle = BorderStyle.FixedSingle;
      Icon224.Enabled = false;
      Icon224.Location = new Point(20, 401);
      Icon224.Name = "Icon224";
      Icon224.Size = new Size(14, 18);
      Icon224.TabIndex = 1;
      Icon224.TabStop = false;
      Icon224.Click += Icon_Click;
      // 
      // Icon225
      // 
      Icon225.BorderStyle = BorderStyle.FixedSingle;
      Icon225.Enabled = false;
      Icon225.Location = new Point(33, 401);
      Icon225.Name = "Icon225";
      Icon225.Size = new Size(14, 18);
      Icon225.TabIndex = 2;
      Icon225.TabStop = false;
      Icon225.Click += Icon_Click;
      // 
      // Icon226
      // 
      Icon226.BorderStyle = BorderStyle.FixedSingle;
      Icon226.Enabled = false;
      Icon226.Location = new Point(46, 401);
      Icon226.Name = "Icon226";
      Icon226.Size = new Size(14, 18);
      Icon226.TabIndex = 3;
      Icon226.TabStop = false;
      Icon226.Click += Icon_Click;
      // 
      // Icon227
      // 
      Icon227.BorderStyle = BorderStyle.FixedSingle;
      Icon227.Enabled = false;
      Icon227.Location = new Point(59, 401);
      Icon227.Name = "Icon227";
      Icon227.Size = new Size(14, 18);
      Icon227.TabIndex = 4;
      Icon227.TabStop = false;
      Icon227.Click += Icon_Click;
      // 
      // Icon228
      // 
      Icon228.BorderStyle = BorderStyle.FixedSingle;
      Icon228.Enabled = false;
      Icon228.Location = new Point(72, 401);
      Icon228.Name = "Icon228";
      Icon228.Size = new Size(14, 18);
      Icon228.TabIndex = 5;
      Icon228.TabStop = false;
      Icon228.Click += Icon_Click;
      // 
      // Icon229
      // 
      Icon229.BorderStyle = BorderStyle.FixedSingle;
      Icon229.Enabled = false;
      Icon229.Location = new Point(85, 401);
      Icon229.Name = "Icon229";
      Icon229.Size = new Size(14, 18);
      Icon229.TabIndex = 6;
      Icon229.TabStop = false;
      Icon229.Click += Icon_Click;
      // 
      // Icon230
      // 
      Icon230.BorderStyle = BorderStyle.FixedSingle;
      Icon230.Enabled = false;
      Icon230.Location = new Point(98, 401);
      Icon230.Name = "Icon230";
      Icon230.Size = new Size(14, 18);
      Icon230.TabIndex = 7;
      Icon230.TabStop = false;
      Icon230.Click += Icon_Click;
      // 
      // Icon231
      // 
      Icon231.BorderStyle = BorderStyle.FixedSingle;
      Icon231.Enabled = false;
      Icon231.Location = new Point(111, 401);
      Icon231.Name = "Icon231";
      Icon231.Size = new Size(14, 18);
      Icon231.TabIndex = 8;
      Icon231.TabStop = false;
      Icon231.Click += Icon_Click;
      // 
      // Icon232
      // 
      Icon232.BorderStyle = BorderStyle.FixedSingle;
      Icon232.Enabled = false;
      Icon232.Location = new Point(124, 401);
      Icon232.Name = "Icon232";
      Icon232.Size = new Size(14, 18);
      Icon232.TabIndex = 9;
      Icon232.TabStop = false;
      Icon232.Click += Icon_Click;
      // 
      // Icon233
      // 
      Icon233.BorderStyle = BorderStyle.FixedSingle;
      Icon233.Enabled = false;
      Icon233.Location = new Point(137, 401);
      Icon233.Name = "Icon233";
      Icon233.Size = new Size(14, 18);
      Icon233.TabIndex = 10;
      Icon233.TabStop = false;
      Icon233.Click += Icon_Click;
      // 
      // Icon234
      // 
      Icon234.BorderStyle = BorderStyle.FixedSingle;
      Icon234.Enabled = false;
      Icon234.Location = new Point(150, 401);
      Icon234.Name = "Icon234";
      Icon234.Size = new Size(14, 18);
      Icon234.TabIndex = 552;
      Icon234.TabStop = false;
      Icon234.Click += Icon_Click;
      // 
      // Icon235
      // 
      Icon235.BorderStyle = BorderStyle.FixedSingle;
      Icon235.Enabled = false;
      Icon235.Location = new Point(163, 401);
      Icon235.Name = "Icon235";
      Icon235.Size = new Size(14, 18);
      Icon235.TabIndex = 553;
      Icon235.TabStop = false;
      Icon235.Click += Icon_Click;
      // 
      // Icon236
      // 
      Icon236.BorderStyle = BorderStyle.FixedSingle;
      Icon236.Enabled = false;
      Icon236.Location = new Point(176, 401);
      Icon236.Name = "Icon236";
      Icon236.Size = new Size(14, 18);
      Icon236.TabIndex = 554;
      Icon236.TabStop = false;
      Icon236.Click += Icon_Click;
      // 
      // Icon237
      // 
      Icon237.BorderStyle = BorderStyle.FixedSingle;
      Icon237.Enabled = false;
      Icon237.Location = new Point(189, 401);
      Icon237.Name = "Icon237";
      Icon237.Size = new Size(14, 18);
      Icon237.TabIndex = 555;
      Icon237.TabStop = false;
      Icon237.Click += Icon_Click;
      // 
      // Icon238
      // 
      Icon238.BorderStyle = BorderStyle.FixedSingle;
      Icon238.Enabled = false;
      Icon238.Location = new Point(202, 401);
      Icon238.Name = "Icon238";
      Icon238.Size = new Size(14, 18);
      Icon238.TabIndex = 556;
      Icon238.TabStop = false;
      Icon238.Click += Icon_Click;
      // 
      // Icon239
      // 
      Icon239.BorderStyle = BorderStyle.FixedSingle;
      Icon239.Enabled = false;
      Icon239.Location = new Point(215, 401);
      Icon239.Name = "Icon239";
      Icon239.Size = new Size(14, 18);
      Icon239.TabIndex = 557;
      Icon239.TabStop = false;
      Icon239.Click += Icon_Click;
      // 
      // Icon240
      // 
      Icon240.BorderStyle = BorderStyle.FixedSingle;
      Icon240.Enabled = false;
      Icon240.Location = new Point(20, 418);
      Icon240.Name = "Icon240";
      Icon240.Size = new Size(14, 18);
      Icon240.TabIndex = 558;
      Icon240.TabStop = false;
      Icon240.Click += Icon_Click;
      // 
      // Icon241
      // 
      Icon241.BorderStyle = BorderStyle.FixedSingle;
      Icon241.Enabled = false;
      Icon241.Location = new Point(33, 418);
      Icon241.Name = "Icon241";
      Icon241.Size = new Size(14, 18);
      Icon241.TabIndex = 559;
      Icon241.TabStop = false;
      Icon241.Click += Icon_Click;
      // 
      // Icon242
      // 
      Icon242.BorderStyle = BorderStyle.FixedSingle;
      Icon242.Enabled = false;
      Icon242.Location = new Point(46, 418);
      Icon242.Name = "Icon242";
      Icon242.Size = new Size(14, 18);
      Icon242.TabIndex = 560;
      Icon242.TabStop = false;
      Icon242.Click += Icon_Click;
      // 
      // Icon243
      // 
      Icon243.BorderStyle = BorderStyle.FixedSingle;
      Icon243.Enabled = false;
      Icon243.Location = new Point(59, 418);
      Icon243.Name = "Icon243";
      Icon243.Size = new Size(14, 18);
      Icon243.TabIndex = 561;
      Icon243.TabStop = false;
      Icon243.Click += Icon_Click;
      // 
      // Icon244
      // 
      Icon244.BorderStyle = BorderStyle.FixedSingle;
      Icon244.Enabled = false;
      Icon244.Location = new Point(72, 418);
      Icon244.Name = "Icon244";
      Icon244.Size = new Size(14, 18);
      Icon244.TabIndex = 562;
      Icon244.TabStop = false;
      Icon244.Click += Icon_Click;
      // 
      // Icon245
      // 
      Icon245.BorderStyle = BorderStyle.FixedSingle;
      Icon245.Enabled = false;
      Icon245.Location = new Point(85, 418);
      Icon245.Name = "Icon245";
      Icon245.Size = new Size(14, 18);
      Icon245.TabIndex = 563;
      Icon245.TabStop = false;
      Icon245.Click += Icon_Click;
      // 
      // Icon246
      // 
      Icon246.BorderStyle = BorderStyle.FixedSingle;
      Icon246.Enabled = false;
      Icon246.Location = new Point(98, 418);
      Icon246.Name = "Icon246";
      Icon246.Size = new Size(14, 18);
      Icon246.TabIndex = 564;
      Icon246.TabStop = false;
      Icon246.Click += Icon_Click;
      // 
      // Icon247
      // 
      Icon247.BorderStyle = BorderStyle.FixedSingle;
      Icon247.Enabled = false;
      Icon247.Location = new Point(111, 418);
      Icon247.Name = "Icon247";
      Icon247.Size = new Size(14, 18);
      Icon247.TabIndex = 565;
      Icon247.TabStop = false;
      Icon247.Click += Icon_Click;
      // 
      // Icon248
      // 
      Icon248.BorderStyle = BorderStyle.FixedSingle;
      Icon248.Enabled = false;
      Icon248.Location = new Point(124, 418);
      Icon248.Name = "Icon248";
      Icon248.Size = new Size(14, 18);
      Icon248.TabIndex = 566;
      Icon248.TabStop = false;
      Icon248.Click += Icon_Click;
      // 
      // Icon249
      // 
      Icon249.BorderStyle = BorderStyle.FixedSingle;
      Icon249.Enabled = false;
      Icon249.Location = new Point(137, 418);
      Icon249.Name = "Icon249";
      Icon249.Size = new Size(14, 18);
      Icon249.TabIndex = 567;
      Icon249.TabStop = false;
      Icon249.Click += Icon_Click;
      // 
      // Icon250
      // 
      Icon250.BorderStyle = BorderStyle.FixedSingle;
      Icon250.Enabled = false;
      Icon250.Location = new Point(150, 418);
      Icon250.Name = "Icon250";
      Icon250.Size = new Size(14, 18);
      Icon250.TabIndex = 568;
      Icon250.TabStop = false;
      Icon250.Click += Icon_Click;
      // 
      // Icon251
      // 
      Icon251.BorderStyle = BorderStyle.FixedSingle;
      Icon251.Enabled = false;
      Icon251.Location = new Point(163, 418);
      Icon251.Name = "Icon251";
      Icon251.Size = new Size(14, 18);
      Icon251.TabIndex = 569;
      Icon251.TabStop = false;
      Icon251.Click += Icon_Click;
      // 
      // Icon252
      // 
      Icon252.BorderStyle = BorderStyle.FixedSingle;
      Icon252.Enabled = false;
      Icon252.Location = new Point(176, 418);
      Icon252.Name = "Icon252";
      Icon252.Size = new Size(14, 18);
      Icon252.TabIndex = 570;
      Icon252.TabStop = false;
      Icon252.Click += Icon_Click;
      // 
      // Icon253
      // 
      Icon253.BorderStyle = BorderStyle.FixedSingle;
      Icon253.Enabled = false;
      Icon253.Location = new Point(189, 418);
      Icon253.Name = "Icon253";
      Icon253.Size = new Size(14, 18);
      Icon253.TabIndex = 571;
      Icon253.TabStop = false;
      Icon253.Click += Icon_Click;
      // 
      // Icon254
      // 
      Icon254.BorderStyle = BorderStyle.FixedSingle;
      Icon254.Enabled = false;
      Icon254.Location = new Point(202, 418);
      Icon254.Name = "Icon254";
      Icon254.Size = new Size(14, 18);
      Icon254.TabIndex = 572;
      Icon254.TabStop = false;
      Icon254.Click += Icon_Click;
      // 
      // Icon255
      // 
      Icon255.BorderStyle = BorderStyle.FixedSingle;
      Icon255.Enabled = false;
      Icon255.Location = new Point(215, 418);
      Icon255.Name = "Icon255";
      Icon255.Size = new Size(14, 18);
      Icon255.TabIndex = 573;
      Icon255.TabStop = false;
      Icon255.Click += Icon_Click;
      // 
      // iMONLCDg_FontEdit
      // 
      AutoScaleDimensions = new SizeF(6F, 13F);
      ClientSize = new Size(392, 481);
      Controls.Add(cmdExit);
      Controls.Add(cmdSave);
      Controls.Add(cmdLoadCustom);
      Controls.Add(cmdLoadInternal);
      Controls.Add(Icon0);
      Controls.Add(Icon1);
      Controls.Add(Icon2);
      Controls.Add(Icon3);
      Controls.Add(Icon4);
      Controls.Add(Icon5);
      Controls.Add(Icon6);
      Controls.Add(Icon7);
      Controls.Add(Icon8);
      Controls.Add(Icon9);
      Controls.Add(Icon10);
      Controls.Add(Icon11);
      Controls.Add(Icon12);
      Controls.Add(Icon13);
      Controls.Add(Icon14);
      Controls.Add(Icon15);
      Controls.Add(Icon16);
      Controls.Add(Icon17);
      Controls.Add(Icon18);
      Controls.Add(Icon19);
      Controls.Add(Icon20);
      Controls.Add(Icon21);
      Controls.Add(Icon22);
      Controls.Add(Icon23);
      Controls.Add(Icon24);
      Controls.Add(Icon25);
      Controls.Add(Icon26);
      Controls.Add(Icon27);
      Controls.Add(Icon28);
      Controls.Add(Icon29);
      Controls.Add(Icon30);
      Controls.Add(Icon31);
      Controls.Add(Icon32);
      Controls.Add(Icon33);
      Controls.Add(Icon34);
      Controls.Add(Icon35);
      Controls.Add(Icon36);
      Controls.Add(Icon37);
      Controls.Add(Icon38);
      Controls.Add(Icon39);
      Controls.Add(Icon40);
      Controls.Add(Icon41);
      Controls.Add(Icon42);
      Controls.Add(Icon43);
      Controls.Add(Icon44);
      Controls.Add(Icon45);
      Controls.Add(Icon46);
      Controls.Add(Icon47);
      Controls.Add(Icon48);
      Controls.Add(Icon49);
      Controls.Add(Icon50);
      Controls.Add(Icon51);
      Controls.Add(Icon52);
      Controls.Add(Icon53);
      Controls.Add(Icon54);
      Controls.Add(Icon55);
      Controls.Add(Icon56);
      Controls.Add(Icon57);
      Controls.Add(Icon58);
      Controls.Add(Icon59);
      Controls.Add(Icon60);
      Controls.Add(Icon61);
      Controls.Add(Icon62);
      Controls.Add(Icon63);
      Controls.Add(Icon64);
      Controls.Add(Icon65);
      Controls.Add(Icon66);
      Controls.Add(Icon67);
      Controls.Add(Icon68);
      Controls.Add(Icon69);
      Controls.Add(Icon70);
      Controls.Add(Icon71);
      Controls.Add(Icon72);
      Controls.Add(Icon73);
      Controls.Add(Icon74);
      Controls.Add(Icon75);
      Controls.Add(Icon76);
      Controls.Add(Icon77);
      Controls.Add(Icon78);
      Controls.Add(Icon79);
      Controls.Add(Icon80);
      Controls.Add(Icon81);
      Controls.Add(Icon82);
      Controls.Add(Icon83);
      Controls.Add(Icon84);
      Controls.Add(Icon85);
      Controls.Add(Icon86);
      Controls.Add(Icon87);
      Controls.Add(Icon88);
      Controls.Add(Icon89);
      Controls.Add(Icon90);
      Controls.Add(Icon91);
      Controls.Add(Icon92);
      Controls.Add(Icon93);
      Controls.Add(Icon94);
      Controls.Add(Icon95);
      Controls.Add(Icon96);
      Controls.Add(Icon97);
      Controls.Add(Icon98);
      Controls.Add(Icon99);
      Controls.Add(Icon100);
      Controls.Add(Icon101);
      Controls.Add(Icon102);
      Controls.Add(Icon103);
      Controls.Add(Icon104);
      Controls.Add(Icon105);
      Controls.Add(Icon106);
      Controls.Add(Icon107);
      Controls.Add(Icon108);
      Controls.Add(Icon109);
      Controls.Add(Icon110);
      Controls.Add(Icon111);
      Controls.Add(Icon112);
      Controls.Add(Icon113);
      Controls.Add(Icon114);
      Controls.Add(Icon115);
      Controls.Add(Icon116);
      Controls.Add(Icon117);
      Controls.Add(Icon118);
      Controls.Add(Icon119);
      Controls.Add(Icon120);
      Controls.Add(Icon121);
      Controls.Add(Icon122);
      Controls.Add(Icon123);
      Controls.Add(Icon124);
      Controls.Add(Icon125);
      Controls.Add(Icon126);
      Controls.Add(Icon127);
      Controls.Add(Icon128);
      Controls.Add(Icon129);
      Controls.Add(Icon130);
      Controls.Add(Icon131);
      Controls.Add(Icon132);
      Controls.Add(Icon133);
      Controls.Add(Icon134);
      Controls.Add(Icon135);
      Controls.Add(Icon136);
      Controls.Add(Icon137);
      Controls.Add(Icon138);
      Controls.Add(Icon139);
      Controls.Add(Icon140);
      Controls.Add(Icon141);
      Controls.Add(Icon142);
      Controls.Add(Icon143);
      Controls.Add(Icon144);
      Controls.Add(Icon145);
      Controls.Add(Icon146);
      Controls.Add(Icon147);
      Controls.Add(Icon148);
      Controls.Add(Icon149);
      Controls.Add(Icon150);
      Controls.Add(Icon151);
      Controls.Add(Icon152);
      Controls.Add(Icon153);
      Controls.Add(Icon154);
      Controls.Add(Icon155);
      Controls.Add(Icon156);
      Controls.Add(Icon157);
      Controls.Add(Icon158);
      Controls.Add(Icon159);
      Controls.Add(Icon160);
      Controls.Add(Icon161);
      Controls.Add(Icon162);
      Controls.Add(Icon163);
      Controls.Add(Icon164);
      Controls.Add(Icon165);
      Controls.Add(Icon166);
      Controls.Add(Icon167);
      Controls.Add(Icon168);
      Controls.Add(Icon169);
      Controls.Add(Icon170);
      Controls.Add(Icon171);
      Controls.Add(Icon172);
      Controls.Add(Icon173);
      Controls.Add(Icon174);
      Controls.Add(Icon175);
      Controls.Add(Icon176);
      Controls.Add(Icon177);
      Controls.Add(Icon178);
      Controls.Add(Icon179);
      Controls.Add(Icon180);
      Controls.Add(Icon181);
      Controls.Add(Icon182);
      Controls.Add(Icon183);
      Controls.Add(Icon184);
      Controls.Add(Icon185);
      Controls.Add(Icon186);
      Controls.Add(Icon187);
      Controls.Add(Icon188);
      Controls.Add(Icon189);
      Controls.Add(Icon190);
      Controls.Add(Icon191);
      Controls.Add(Icon192);
      Controls.Add(Icon193);
      Controls.Add(Icon194);
      Controls.Add(Icon195);
      Controls.Add(Icon196);
      Controls.Add(Icon197);
      Controls.Add(Icon198);
      Controls.Add(Icon199);
      Controls.Add(Icon200);
      Controls.Add(Icon201);
      Controls.Add(Icon202);
      Controls.Add(Icon203);
      Controls.Add(Icon204);
      Controls.Add(Icon205);
      Controls.Add(Icon206);
      Controls.Add(Icon207);
      Controls.Add(Icon208);
      Controls.Add(Icon209);
      Controls.Add(Icon210);
      Controls.Add(Icon211);
      Controls.Add(Icon212);
      Controls.Add(Icon213);
      Controls.Add(Icon214);
      Controls.Add(Icon215);
      Controls.Add(Icon216);
      Controls.Add(Icon217);
      Controls.Add(Icon218);
      Controls.Add(Icon219);
      Controls.Add(Icon220);
      Controls.Add(Icon221);
      Controls.Add(Icon222);
      Controls.Add(Icon223);
      Controls.Add(Icon224);
      Controls.Add(Icon225);
      Controls.Add(Icon226);
      Controls.Add(Icon227);
      Controls.Add(Icon228);
      Controls.Add(Icon229);
      Controls.Add(Icon230);
      Controls.Add(Icon231);
      Controls.Add(Icon232);
      Controls.Add(Icon233);
      Controls.Add(Icon234);
      Controls.Add(Icon235);
      Controls.Add(Icon236);
      Controls.Add(Icon237);
      Controls.Add(Icon238);
      Controls.Add(Icon239);
      Controls.Add(Icon240);
      Controls.Add(Icon241);
      Controls.Add(Icon242);
      Controls.Add(Icon243);
      Controls.Add(Icon244);
      Controls.Add(Icon245);
      Controls.Add(Icon246);
      Controls.Add(Icon247);
      Controls.Add(Icon248);
      Controls.Add(Icon249);
      Controls.Add(Icon250);
      Controls.Add(Icon251);
      Controls.Add(Icon252);
      Controls.Add(Icon253);
      Controls.Add(Icon254);
      Controls.Add(Icon255);
      Controls.Add(panel1);
      Name = "iMONLCDg_FontEdit";
      StartPosition = FormStartPosition.CenterParent;
      Text = "iMONLCDg_FontEdit";
      panel1.ResumeLayout(false);
      panel1.PerformLayout();
      ((ISupportInitialize)(Icon0)).EndInit();
      ((ISupportInitialize)(Icon1)).EndInit();
      ((ISupportInitialize)(Icon2)).EndInit();
      ((ISupportInitialize)(Icon3)).EndInit();
      ((ISupportInitialize)(Icon4)).EndInit();
      ((ISupportInitialize)(Icon5)).EndInit();
      ((ISupportInitialize)(Icon6)).EndInit();
      ((ISupportInitialize)(Icon7)).EndInit();
      ((ISupportInitialize)(Icon8)).EndInit();
      ((ISupportInitialize)(Icon9)).EndInit();
      ((ISupportInitialize)(Icon10)).EndInit();
      ((ISupportInitialize)(Icon11)).EndInit();
      ((ISupportInitialize)(Icon12)).EndInit();
      ((ISupportInitialize)(Icon13)).EndInit();
      ((ISupportInitialize)(Icon14)).EndInit();
      ((ISupportInitialize)(Icon15)).EndInit();
      ((ISupportInitialize)(Icon16)).EndInit();
      ((ISupportInitialize)(Icon17)).EndInit();
      ((ISupportInitialize)(Icon18)).EndInit();
      ((ISupportInitialize)(Icon19)).EndInit();
      ((ISupportInitialize)(Icon20)).EndInit();
      ((ISupportInitialize)(Icon21)).EndInit();
      ((ISupportInitialize)(Icon22)).EndInit();
      ((ISupportInitialize)(Icon23)).EndInit();
      ((ISupportInitialize)(Icon24)).EndInit();
      ((ISupportInitialize)(Icon25)).EndInit();
      ((ISupportInitialize)(Icon26)).EndInit();
      ((ISupportInitialize)(Icon27)).EndInit();
      ((ISupportInitialize)(Icon28)).EndInit();
      ((ISupportInitialize)(Icon29)).EndInit();
      ((ISupportInitialize)(Icon30)).EndInit();
      ((ISupportInitialize)(Icon31)).EndInit();
      ((ISupportInitialize)(Icon32)).EndInit();
      ((ISupportInitialize)(Icon33)).EndInit();
      ((ISupportInitialize)(Icon34)).EndInit();
      ((ISupportInitialize)(Icon35)).EndInit();
      ((ISupportInitialize)(Icon36)).EndInit();
      ((ISupportInitialize)(Icon37)).EndInit();
      ((ISupportInitialize)(Icon38)).EndInit();
      ((ISupportInitialize)(Icon39)).EndInit();
      ((ISupportInitialize)(Icon40)).EndInit();
      ((ISupportInitialize)(Icon41)).EndInit();
      ((ISupportInitialize)(Icon42)).EndInit();
      ((ISupportInitialize)(Icon43)).EndInit();
      ((ISupportInitialize)(Icon44)).EndInit();
      ((ISupportInitialize)(Icon45)).EndInit();
      ((ISupportInitialize)(Icon46)).EndInit();
      ((ISupportInitialize)(Icon47)).EndInit();
      ((ISupportInitialize)(Icon48)).EndInit();
      ((ISupportInitialize)(Icon49)).EndInit();
      ((ISupportInitialize)(Icon50)).EndInit();
      ((ISupportInitialize)(Icon51)).EndInit();
      ((ISupportInitialize)(Icon52)).EndInit();
      ((ISupportInitialize)(Icon53)).EndInit();
      ((ISupportInitialize)(Icon54)).EndInit();
      ((ISupportInitialize)(Icon55)).EndInit();
      ((ISupportInitialize)(Icon56)).EndInit();
      ((ISupportInitialize)(Icon57)).EndInit();
      ((ISupportInitialize)(Icon58)).EndInit();
      ((ISupportInitialize)(Icon59)).EndInit();
      ((ISupportInitialize)(Icon60)).EndInit();
      ((ISupportInitialize)(Icon61)).EndInit();
      ((ISupportInitialize)(Icon62)).EndInit();
      ((ISupportInitialize)(Icon63)).EndInit();
      ((ISupportInitialize)(Icon64)).EndInit();
      ((ISupportInitialize)(Icon65)).EndInit();
      ((ISupportInitialize)(Icon66)).EndInit();
      ((ISupportInitialize)(Icon67)).EndInit();
      ((ISupportInitialize)(Icon68)).EndInit();
      ((ISupportInitialize)(Icon69)).EndInit();
      ((ISupportInitialize)(Icon70)).EndInit();
      ((ISupportInitialize)(Icon71)).EndInit();
      ((ISupportInitialize)(Icon72)).EndInit();
      ((ISupportInitialize)(Icon73)).EndInit();
      ((ISupportInitialize)(Icon74)).EndInit();
      ((ISupportInitialize)(Icon75)).EndInit();
      ((ISupportInitialize)(Icon76)).EndInit();
      ((ISupportInitialize)(Icon77)).EndInit();
      ((ISupportInitialize)(Icon78)).EndInit();
      ((ISupportInitialize)(Icon79)).EndInit();
      ((ISupportInitialize)(Icon80)).EndInit();
      ((ISupportInitialize)(Icon81)).EndInit();
      ((ISupportInitialize)(Icon82)).EndInit();
      ((ISupportInitialize)(Icon83)).EndInit();
      ((ISupportInitialize)(Icon84)).EndInit();
      ((ISupportInitialize)(Icon85)).EndInit();
      ((ISupportInitialize)(Icon86)).EndInit();
      ((ISupportInitialize)(Icon87)).EndInit();
      ((ISupportInitialize)(Icon88)).EndInit();
      ((ISupportInitialize)(Icon89)).EndInit();
      ((ISupportInitialize)(Icon90)).EndInit();
      ((ISupportInitialize)(Icon91)).EndInit();
      ((ISupportInitialize)(Icon92)).EndInit();
      ((ISupportInitialize)(Icon93)).EndInit();
      ((ISupportInitialize)(Icon94)).EndInit();
      ((ISupportInitialize)(Icon95)).EndInit();
      ((ISupportInitialize)(Icon96)).EndInit();
      ((ISupportInitialize)(Icon97)).EndInit();
      ((ISupportInitialize)(Icon98)).EndInit();
      ((ISupportInitialize)(Icon99)).EndInit();
      ((ISupportInitialize)(Icon100)).EndInit();
      ((ISupportInitialize)(Icon101)).EndInit();
      ((ISupportInitialize)(Icon102)).EndInit();
      ((ISupportInitialize)(Icon103)).EndInit();
      ((ISupportInitialize)(Icon104)).EndInit();
      ((ISupportInitialize)(Icon105)).EndInit();
      ((ISupportInitialize)(Icon106)).EndInit();
      ((ISupportInitialize)(Icon107)).EndInit();
      ((ISupportInitialize)(Icon108)).EndInit();
      ((ISupportInitialize)(Icon109)).EndInit();
      ((ISupportInitialize)(Icon110)).EndInit();
      ((ISupportInitialize)(Icon111)).EndInit();
      ((ISupportInitialize)(Icon112)).EndInit();
      ((ISupportInitialize)(Icon113)).EndInit();
      ((ISupportInitialize)(Icon114)).EndInit();
      ((ISupportInitialize)(Icon115)).EndInit();
      ((ISupportInitialize)(Icon116)).EndInit();
      ((ISupportInitialize)(Icon117)).EndInit();
      ((ISupportInitialize)(Icon118)).EndInit();
      ((ISupportInitialize)(Icon119)).EndInit();
      ((ISupportInitialize)(Icon120)).EndInit();
      ((ISupportInitialize)(Icon121)).EndInit();
      ((ISupportInitialize)(Icon122)).EndInit();
      ((ISupportInitialize)(Icon123)).EndInit();
      ((ISupportInitialize)(Icon124)).EndInit();
      ((ISupportInitialize)(Icon125)).EndInit();
      ((ISupportInitialize)(Icon126)).EndInit();
      ((ISupportInitialize)(Icon127)).EndInit();
      ((ISupportInitialize)(Icon128)).EndInit();
      ((ISupportInitialize)(Icon129)).EndInit();
      ((ISupportInitialize)(Icon130)).EndInit();
      ((ISupportInitialize)(Icon131)).EndInit();
      ((ISupportInitialize)(Icon132)).EndInit();
      ((ISupportInitialize)(Icon133)).EndInit();
      ((ISupportInitialize)(Icon134)).EndInit();
      ((ISupportInitialize)(Icon135)).EndInit();
      ((ISupportInitialize)(Icon136)).EndInit();
      ((ISupportInitialize)(Icon137)).EndInit();
      ((ISupportInitialize)(Icon138)).EndInit();
      ((ISupportInitialize)(Icon139)).EndInit();
      ((ISupportInitialize)(Icon140)).EndInit();
      ((ISupportInitialize)(Icon141)).EndInit();
      ((ISupportInitialize)(Icon142)).EndInit();
      ((ISupportInitialize)(Icon143)).EndInit();
      ((ISupportInitialize)(Icon144)).EndInit();
      ((ISupportInitialize)(Icon145)).EndInit();
      ((ISupportInitialize)(Icon146)).EndInit();
      ((ISupportInitialize)(Icon147)).EndInit();
      ((ISupportInitialize)(Icon148)).EndInit();
      ((ISupportInitialize)(Icon149)).EndInit();
      ((ISupportInitialize)(Icon150)).EndInit();
      ((ISupportInitialize)(Icon151)).EndInit();
      ((ISupportInitialize)(Icon152)).EndInit();
      ((ISupportInitialize)(Icon153)).EndInit();
      ((ISupportInitialize)(Icon154)).EndInit();
      ((ISupportInitialize)(Icon155)).EndInit();
      ((ISupportInitialize)(Icon156)).EndInit();
      ((ISupportInitialize)(Icon157)).EndInit();
      ((ISupportInitialize)(Icon158)).EndInit();
      ((ISupportInitialize)(Icon159)).EndInit();
      ((ISupportInitialize)(Icon160)).EndInit();
      ((ISupportInitialize)(Icon161)).EndInit();
      ((ISupportInitialize)(Icon162)).EndInit();
      ((ISupportInitialize)(Icon163)).EndInit();
      ((ISupportInitialize)(Icon164)).EndInit();
      ((ISupportInitialize)(Icon165)).EndInit();
      ((ISupportInitialize)(Icon166)).EndInit();
      ((ISupportInitialize)(Icon167)).EndInit();
      ((ISupportInitialize)(Icon168)).EndInit();
      ((ISupportInitialize)(Icon169)).EndInit();
      ((ISupportInitialize)(Icon170)).EndInit();
      ((ISupportInitialize)(Icon171)).EndInit();
      ((ISupportInitialize)(Icon172)).EndInit();
      ((ISupportInitialize)(Icon173)).EndInit();
      ((ISupportInitialize)(Icon174)).EndInit();
      ((ISupportInitialize)(Icon175)).EndInit();
      ((ISupportInitialize)(Icon176)).EndInit();
      ((ISupportInitialize)(Icon177)).EndInit();
      ((ISupportInitialize)(Icon178)).EndInit();
      ((ISupportInitialize)(Icon179)).EndInit();
      ((ISupportInitialize)(Icon180)).EndInit();
      ((ISupportInitialize)(Icon181)).EndInit();
      ((ISupportInitialize)(Icon182)).EndInit();
      ((ISupportInitialize)(Icon183)).EndInit();
      ((ISupportInitialize)(Icon184)).EndInit();
      ((ISupportInitialize)(Icon185)).EndInit();
      ((ISupportInitialize)(Icon186)).EndInit();
      ((ISupportInitialize)(Icon187)).EndInit();
      ((ISupportInitialize)(Icon188)).EndInit();
      ((ISupportInitialize)(Icon189)).EndInit();
      ((ISupportInitialize)(Icon190)).EndInit();
      ((ISupportInitialize)(Icon191)).EndInit();
      ((ISupportInitialize)(Icon192)).EndInit();
      ((ISupportInitialize)(Icon193)).EndInit();
      ((ISupportInitialize)(Icon194)).EndInit();
      ((ISupportInitialize)(Icon195)).EndInit();
      ((ISupportInitialize)(Icon196)).EndInit();
      ((ISupportInitialize)(Icon197)).EndInit();
      ((ISupportInitialize)(Icon198)).EndInit();
      ((ISupportInitialize)(Icon199)).EndInit();
      ((ISupportInitialize)(Icon200)).EndInit();
      ((ISupportInitialize)(Icon201)).EndInit();
      ((ISupportInitialize)(Icon202)).EndInit();
      ((ISupportInitialize)(Icon203)).EndInit();
      ((ISupportInitialize)(Icon204)).EndInit();
      ((ISupportInitialize)(Icon205)).EndInit();
      ((ISupportInitialize)(Icon206)).EndInit();
      ((ISupportInitialize)(Icon207)).EndInit();
      ((ISupportInitialize)(Icon208)).EndInit();
      ((ISupportInitialize)(Icon209)).EndInit();
      ((ISupportInitialize)(Icon210)).EndInit();
      ((ISupportInitialize)(Icon211)).EndInit();
      ((ISupportInitialize)(Icon212)).EndInit();
      ((ISupportInitialize)(Icon213)).EndInit();
      ((ISupportInitialize)(Icon214)).EndInit();
      ((ISupportInitialize)(Icon215)).EndInit();
      ((ISupportInitialize)(Icon216)).EndInit();
      ((ISupportInitialize)(Icon217)).EndInit();
      ((ISupportInitialize)(Icon218)).EndInit();
      ((ISupportInitialize)(Icon219)).EndInit();
      ((ISupportInitialize)(Icon220)).EndInit();
      ((ISupportInitialize)(Icon221)).EndInit();
      ((ISupportInitialize)(Icon222)).EndInit();
      ((ISupportInitialize)(Icon223)).EndInit();
      ((ISupportInitialize)(Icon224)).EndInit();
      ((ISupportInitialize)(Icon225)).EndInit();
      ((ISupportInitialize)(Icon226)).EndInit();
      ((ISupportInitialize)(Icon227)).EndInit();
      ((ISupportInitialize)(Icon228)).EndInit();
      ((ISupportInitialize)(Icon229)).EndInit();
      ((ISupportInitialize)(Icon230)).EndInit();
      ((ISupportInitialize)(Icon231)).EndInit();
      ((ISupportInitialize)(Icon232)).EndInit();
      ((ISupportInitialize)(Icon233)).EndInit();
      ((ISupportInitialize)(Icon234)).EndInit();
      ((ISupportInitialize)(Icon235)).EndInit();
      ((ISupportInitialize)(Icon236)).EndInit();
      ((ISupportInitialize)(Icon237)).EndInit();
      ((ISupportInitialize)(Icon238)).EndInit();
      ((ISupportInitialize)(Icon239)).EndInit();
      ((ISupportInitialize)(Icon240)).EndInit();
      ((ISupportInitialize)(Icon241)).EndInit();
      ((ISupportInitialize)(Icon242)).EndInit();
      ((ISupportInitialize)(Icon243)).EndInit();
      ((ISupportInitialize)(Icon244)).EndInit();
      ((ISupportInitialize)(Icon245)).EndInit();
      ((ISupportInitialize)(Icon246)).EndInit();
      ((ISupportInitialize)(Icon247)).EndInit();
      ((ISupportInitialize)(Icon248)).EndInit();
      ((ISupportInitialize)(Icon249)).EndInit();
      ((ISupportInitialize)(Icon250)).EndInit();
      ((ISupportInitialize)(Icon251)).EndInit();
      ((ISupportInitialize)(Icon252)).EndInit();
      ((ISupportInitialize)(Icon253)).EndInit();
      ((ISupportInitialize)(Icon254)).EndInit();
      ((ISupportInitialize)(Icon255)).EndInit();
      ResumeLayout(false);
    }

    public void LoadCustomFont()
    {
      var table = new DataTable("Character");
      var column = new DataColumn("CharID");
      var column2 = new DataColumn("CData0");
      var column3 = new DataColumn("CData1");
      var column4 = new DataColumn("CData2");
      var column5 = new DataColumn("CData3");
      var column6 = new DataColumn("CData4");
      var column7 = new DataColumn("CData5");
      table.Rows.Clear();
      table.Columns.Clear();
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
      table.Clear();
      if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_font.xml")))
      {
        table.Rows.Clear();
        var serializer = new XmlSerializer(typeof (DataTable));
        var xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_font.xml"));
        table = (DataTable)serializer.Deserialize(xmlReader);
        xmlReader.Close();
        for (int i = 0; i < 0x100; i++)
        {
          DataRow row = table.Rows[i];
          for (int j = 0; j < 6; j++)
          {
            _FontBuffer[i, j] = (byte)row[j + 1];
          }
        }
        CopyBufferToGraphics();
        IconsChanged = false;
      }
      else
      {
        LoadInteralFont();
      }
    }

    public void LoadInteralFont()
    {
      for (int i = 0; i < 0x100; i++)
      {
        for (int j = 0; j < 6; j++)
        {
          _FontBuffer[i, j] = iMONLCDg._Font8x5[i, j];
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
      string key = "C" + Column.ToString().Trim() + "_B" + Row.ToString().Trim();
      Control[] controlArray = panel1.Controls.Find(key, false);
      if (controlArray.Length > 0)
      {
        var box = (CheckBox)controlArray[0];
        box.CheckState = SetOn ? CheckState.Indeterminate : CheckState.Unchecked;
      }
    }

    private void SetIconEdit(int CharIndex)
    {
      lblCurrentIcon.Text = "( Character " + CharIndex + " )";
      DisplayIconForEditing(CharIndex);
      EditIndex = CharIndex;
      EnableEditPanel(true);
    }
  }
}