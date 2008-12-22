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

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class iMONLCDg_FontEdit : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    private static byte[,] _FontBuffer = new byte[0x100, 6];
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
    private IContainer components = null;
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
    private Bitmap[] IconGraphics = new Bitmap[0x100];
    private bool IconsChanged;
    private Label lblCurrentIcon;
    private Label lblEditIndex;
    private int NumChars = 0x100;
    private Panel panel1;

    public iMONLCDg_FontEdit()
    {
      this.InitializeComponent();
    }

    private void ClearFontBuffer()
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
      this.ClearFontBuffer();
      this.CopyBufferToGraphics();
      this.EnableIconSelection(false);
    }

    private void cmdCancelEdit_Click(object sender, EventArgs e)
    {
      this.cmdClearAll_Click(null, null);
      this.lblCurrentIcon.Text = "";
      this.EditIndex = -1;
      this.EnableEditPanel(false);
    }

    private void cmdClearAll_Click(object sender, EventArgs e)
    {
      for (int i = 0; i < 6; i++)
      {
        for (int j = 0; j < 8; j++)
        {
          this.SetEditPixel(i, j, false);
        }
      }
    }

    private void cmdExit_Click(object sender, EventArgs e)
    {
      base.Hide();
      base.Close();
    }

    private void cmdInvert_Click(object sender, EventArgs e)
    {
      for (int i = 0; i < 6; i++)
      {
        for (int j = 0; j < 8; j++)
        {
          switch (this.GetEditPixel(i, j))
          {
            case CheckState.Unchecked:
              this.SetEditPixel(i, j, true);
              break;

            case CheckState.Checked:
            case CheckState.Indeterminate:
              this.SetEditPixel(i, j, false);
              break;
          }
        }
      }
    }

    private void cmdLoadCustom_Click(object sender, EventArgs e)
    {
      this.LoadCustomFont();
      this.EnableIconSelection(true);
    }

    private void cmdLoadInternal_Click(object sender, EventArgs e)
    {
      this.LoadInteralFont();
      this.EnableIconSelection(true);
    }

    private void cmdSave_Click(object sender, EventArgs e)
    {
      if (this.IconsChanged)
      {
        DataTable o = new DataTable("Character");
        DataColumn column = new DataColumn("CharID");
        DataColumn column2 = new DataColumn("CData0");
        DataColumn column3 = new DataColumn("CData1");
        DataColumn column4 = new DataColumn("CData2");
        DataColumn column5 = new DataColumn("CData3");
        DataColumn column6 = new DataColumn("CData4");
        DataColumn column7 = new DataColumn("CData5");
        o.Rows.Clear();
        o.Columns.Clear();
        column.DataType = typeof(byte);
        o.Columns.Add(column);
        column2.DataType = typeof(byte);
        o.Columns.Add(column2);
        column3.DataType = typeof(byte);
        o.Columns.Add(column3);
        column4.DataType = typeof(byte);
        o.Columns.Add(column4);
        column5.DataType = typeof(byte);
        o.Columns.Add(column5);
        column6.DataType = typeof(byte);
        o.Columns.Add(column6);
        column7.DataType = typeof(byte);
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
          XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
          TextWriter textWriter = new StreamWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_font.xml"));
          serializer.Serialize(textWriter, o);
          textWriter.Close();
        } catch (Exception exception)
        {
          Log.Debug("CAUGHT EXCEPTION: {0}", new object[] { exception });
        }
        this.ClearIconDisplay();
        this.EnableIconSelection(false);
        this.cmdSave.Enabled = false;
      }
    }

    private void cmdSaveEdit_Click(object sender, EventArgs e)
    {
      if (this.EditIndex >= 0)
      {
        for (int i = 0; i < 6; i++)
        {
          byte num2 = 0;
          for (int j = 0; j < 8; j++)
          {
            num2 = (byte)(num2 | ((byte)(((this.GetEditPixel(i, j) == CheckState.Indeterminate) ? ((double)1) : ((double)0)) * Math.Pow(2.0, (double)(7 - j)))));
          }
          _FontBuffer[this.EditIndex, i] = num2;
        }
        this.CopyBufferToGraphics();
        this.IconsChanged = true;
        this.cmdSave.Enabled = true;
        this.cmdClearAll_Click(null, null);
        this.lblCurrentIcon.Text = "";
        this.EnableEditPanel(false);
      }
    }

    private void cmdSetAll_Click(object sender, EventArgs e)
    {
      for (int i = 0; i < 6; i++)
      {
        for (int j = 0; j < 8; j++)
        {
          this.SetEditPixel(i, j, true);
        }
      }
    }

    public void CopyBufferToGraphics()
    {
      for (int i = 0; i < this.NumChars; i++)
      {
        if (this.IconGraphics[i] == null)
        {
          this.IconGraphics[i] = new Bitmap(12, 0x10);
        }
        for (int j = 0; j < 6; j++)
        {
          for (int k = 0; k < 8; k++)
          {
            Color black;
            int num4 = (int)Math.Pow(2.0, (double)k);
            bool flag = (_FontBuffer[i, j] & num4) > 0;
            int x = j * 2;
            int y = k * 2;
            if (flag)
            {
              black = Color.Black;
            }
            else
            {
              black = Color.White;
            }
            this.IconGraphics[i].SetPixel(x, y, black);
            this.IconGraphics[i].SetPixel(x + 1, y, black);
            this.IconGraphics[i].SetPixel(x, y + 1, black);
            this.IconGraphics[i].SetPixel(x + 1, y + 1, black);
          }
        }
        string key = "Icon" + i.ToString().Trim();
        Control[] controlArray = base.Controls.Find(key, false);
        if (controlArray.Length > 0)
        {
          PictureBox box = (PictureBox)controlArray[0];
          box.Image = this.IconGraphics[i];
        }
        else
        {
          Log.Debug("Could not find control \"{0}\"", new object[] { key });
        }
      }
    }

    private void DisplayIconForEditing(int IconIndex)
    {
      for (int i = 0; i < 6; i++)
      {
        for (int j = 0; j < 8; j++)
        {
          int num3 = (int)Math.Pow(2.0, (double)(7 - j));
          bool setOn = (_FontBuffer[IconIndex, i] & num3) > 0;
          this.SetEditPixel(i, j, setOn);
        }
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && (this.components != null))
      {
        this.components.Dispose();
      }
      base.Dispose(disposing);
    }

    private void EnableEditPanel(bool Enable)
    {
      this.panel1.Enabled = Enable;
      this.lblCurrentIcon.Enabled = Enable;
      this.cmdClearAll.Enabled = Enable;
      this.cmdInvert.Enabled = Enable;
      this.cmdSetAll.Enabled = Enable;
      this.cmdSaveEdit.Enabled = Enable;
      this.cmdCancelEdit.Enabled = Enable;
    }

    private void EnableIconSelection(bool Enable)
    {
      for (int i = 0; i < this.NumChars; i++)
      {
        string key = "Icon" + i.ToString().Trim();
        Control[] controlArray = base.Controls.Find(key, false);
        if (controlArray.Length > 0)
        {
          PictureBox box = (PictureBox)controlArray[0];
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
      Control[] controlArray = this.panel1.Controls.Find(key, false);
      if (controlArray.Length > 0)
      {
        CheckBox box = (CheckBox)controlArray[0];
        return box.CheckState;
      }
      return CheckState.Unchecked;
    }

    private void Icon_Click(object sender, EventArgs e)
    {
      try
      {
        PictureBox box = (PictureBox)sender;
        int charIndex = int.Parse(box.Name.Substring(4));
        this.SetIconEdit(charIndex);
      } catch (Exception exception)
      {
        Log.Debug("CAUGHT EXCEPTION: {0}", new object[] { exception });
      }
    }

    private void InitializeComponent()
    {
      this.panel1 = new System.Windows.Forms.Panel();
      this.lblEditIndex = new System.Windows.Forms.Label();
      this.cmdSaveEdit = new System.Windows.Forms.Button();
      this.cmdCancelEdit = new System.Windows.Forms.Button();
      this.cmdInvert = new System.Windows.Forms.Button();
      this.cmdSetAll = new System.Windows.Forms.Button();
      this.cmdClearAll = new System.Windows.Forms.Button();
      this.lblCurrentIcon = new System.Windows.Forms.Label();
      this.C4_B0 = new System.Windows.Forms.CheckBox();
      this.C4_B1 = new System.Windows.Forms.CheckBox();
      this.C4_B2 = new System.Windows.Forms.CheckBox();
      this.C4_B3 = new System.Windows.Forms.CheckBox();
      this.C4_B4 = new System.Windows.Forms.CheckBox();
      this.C4_B5 = new System.Windows.Forms.CheckBox();
      this.C4_B6 = new System.Windows.Forms.CheckBox();
      this.C4_B7 = new System.Windows.Forms.CheckBox();
      this.C5_B0 = new System.Windows.Forms.CheckBox();
      this.C5_B1 = new System.Windows.Forms.CheckBox();
      this.C5_B2 = new System.Windows.Forms.CheckBox();
      this.C5_B3 = new System.Windows.Forms.CheckBox();
      this.C5_B4 = new System.Windows.Forms.CheckBox();
      this.C5_B5 = new System.Windows.Forms.CheckBox();
      this.C5_B6 = new System.Windows.Forms.CheckBox();
      this.C5_B7 = new System.Windows.Forms.CheckBox();
      this.C2_B0 = new System.Windows.Forms.CheckBox();
      this.C2_B1 = new System.Windows.Forms.CheckBox();
      this.C2_B2 = new System.Windows.Forms.CheckBox();
      this.C2_B3 = new System.Windows.Forms.CheckBox();
      this.C2_B4 = new System.Windows.Forms.CheckBox();
      this.C2_B5 = new System.Windows.Forms.CheckBox();
      this.C2_B6 = new System.Windows.Forms.CheckBox();
      this.C2_B7 = new System.Windows.Forms.CheckBox();
      this.C3_B0 = new System.Windows.Forms.CheckBox();
      this.C3_B1 = new System.Windows.Forms.CheckBox();
      this.C3_B2 = new System.Windows.Forms.CheckBox();
      this.C3_B3 = new System.Windows.Forms.CheckBox();
      this.C3_B4 = new System.Windows.Forms.CheckBox();
      this.C3_B5 = new System.Windows.Forms.CheckBox();
      this.C3_B6 = new System.Windows.Forms.CheckBox();
      this.C3_B7 = new System.Windows.Forms.CheckBox();
      this.C0_B0 = new System.Windows.Forms.CheckBox();
      this.C0_B1 = new System.Windows.Forms.CheckBox();
      this.C0_B2 = new System.Windows.Forms.CheckBox();
      this.C0_B3 = new System.Windows.Forms.CheckBox();
      this.C0_B4 = new System.Windows.Forms.CheckBox();
      this.C0_B5 = new System.Windows.Forms.CheckBox();
      this.C0_B6 = new System.Windows.Forms.CheckBox();
      this.C0_B7 = new System.Windows.Forms.CheckBox();
      this.C1_B0 = new System.Windows.Forms.CheckBox();
      this.C1_B1 = new System.Windows.Forms.CheckBox();
      this.C1_B2 = new System.Windows.Forms.CheckBox();
      this.C1_B3 = new System.Windows.Forms.CheckBox();
      this.C1_B4 = new System.Windows.Forms.CheckBox();
      this.C1_B5 = new System.Windows.Forms.CheckBox();
      this.C1_B6 = new System.Windows.Forms.CheckBox();
      this.C1_B7 = new System.Windows.Forms.CheckBox();
      this.cmdLoadInternal = new System.Windows.Forms.Button();
      this.cmdLoadCustom = new System.Windows.Forms.Button();
      this.cmdSave = new System.Windows.Forms.Button();
      this.cmdExit = new System.Windows.Forms.Button();
      this.Icon0 = new System.Windows.Forms.PictureBox();
      this.Icon1 = new System.Windows.Forms.PictureBox();
      this.Icon2 = new System.Windows.Forms.PictureBox();
      this.Icon3 = new System.Windows.Forms.PictureBox();
      this.Icon4 = new System.Windows.Forms.PictureBox();
      this.Icon5 = new System.Windows.Forms.PictureBox();
      this.Icon6 = new System.Windows.Forms.PictureBox();
      this.Icon7 = new System.Windows.Forms.PictureBox();
      this.Icon8 = new System.Windows.Forms.PictureBox();
      this.Icon9 = new System.Windows.Forms.PictureBox();
      this.Icon10 = new System.Windows.Forms.PictureBox();
      this.Icon11 = new System.Windows.Forms.PictureBox();
      this.Icon12 = new System.Windows.Forms.PictureBox();
      this.Icon13 = new System.Windows.Forms.PictureBox();
      this.Icon14 = new System.Windows.Forms.PictureBox();
      this.Icon15 = new System.Windows.Forms.PictureBox();
      this.Icon16 = new System.Windows.Forms.PictureBox();
      this.Icon17 = new System.Windows.Forms.PictureBox();
      this.Icon18 = new System.Windows.Forms.PictureBox();
      this.Icon19 = new System.Windows.Forms.PictureBox();
      this.Icon20 = new System.Windows.Forms.PictureBox();
      this.Icon21 = new System.Windows.Forms.PictureBox();
      this.Icon22 = new System.Windows.Forms.PictureBox();
      this.Icon23 = new System.Windows.Forms.PictureBox();
      this.Icon24 = new System.Windows.Forms.PictureBox();
      this.Icon25 = new System.Windows.Forms.PictureBox();
      this.Icon26 = new System.Windows.Forms.PictureBox();
      this.Icon27 = new System.Windows.Forms.PictureBox();
      this.Icon28 = new System.Windows.Forms.PictureBox();
      this.Icon29 = new System.Windows.Forms.PictureBox();
      this.Icon30 = new System.Windows.Forms.PictureBox();
      this.Icon31 = new System.Windows.Forms.PictureBox();
      this.Icon32 = new System.Windows.Forms.PictureBox();
      this.Icon33 = new System.Windows.Forms.PictureBox();
      this.Icon34 = new System.Windows.Forms.PictureBox();
      this.Icon35 = new System.Windows.Forms.PictureBox();
      this.Icon36 = new System.Windows.Forms.PictureBox();
      this.Icon37 = new System.Windows.Forms.PictureBox();
      this.Icon38 = new System.Windows.Forms.PictureBox();
      this.Icon39 = new System.Windows.Forms.PictureBox();
      this.Icon40 = new System.Windows.Forms.PictureBox();
      this.Icon41 = new System.Windows.Forms.PictureBox();
      this.Icon42 = new System.Windows.Forms.PictureBox();
      this.Icon43 = new System.Windows.Forms.PictureBox();
      this.Icon44 = new System.Windows.Forms.PictureBox();
      this.Icon45 = new System.Windows.Forms.PictureBox();
      this.Icon46 = new System.Windows.Forms.PictureBox();
      this.Icon47 = new System.Windows.Forms.PictureBox();
      this.Icon48 = new System.Windows.Forms.PictureBox();
      this.Icon49 = new System.Windows.Forms.PictureBox();
      this.Icon50 = new System.Windows.Forms.PictureBox();
      this.Icon51 = new System.Windows.Forms.PictureBox();
      this.Icon52 = new System.Windows.Forms.PictureBox();
      this.Icon53 = new System.Windows.Forms.PictureBox();
      this.Icon54 = new System.Windows.Forms.PictureBox();
      this.Icon55 = new System.Windows.Forms.PictureBox();
      this.Icon56 = new System.Windows.Forms.PictureBox();
      this.Icon57 = new System.Windows.Forms.PictureBox();
      this.Icon58 = new System.Windows.Forms.PictureBox();
      this.Icon59 = new System.Windows.Forms.PictureBox();
      this.Icon60 = new System.Windows.Forms.PictureBox();
      this.Icon61 = new System.Windows.Forms.PictureBox();
      this.Icon62 = new System.Windows.Forms.PictureBox();
      this.Icon63 = new System.Windows.Forms.PictureBox();
      this.Icon64 = new System.Windows.Forms.PictureBox();
      this.Icon65 = new System.Windows.Forms.PictureBox();
      this.Icon66 = new System.Windows.Forms.PictureBox();
      this.Icon67 = new System.Windows.Forms.PictureBox();
      this.Icon68 = new System.Windows.Forms.PictureBox();
      this.Icon69 = new System.Windows.Forms.PictureBox();
      this.Icon70 = new System.Windows.Forms.PictureBox();
      this.Icon71 = new System.Windows.Forms.PictureBox();
      this.Icon72 = new System.Windows.Forms.PictureBox();
      this.Icon73 = new System.Windows.Forms.PictureBox();
      this.Icon74 = new System.Windows.Forms.PictureBox();
      this.Icon75 = new System.Windows.Forms.PictureBox();
      this.Icon76 = new System.Windows.Forms.PictureBox();
      this.Icon77 = new System.Windows.Forms.PictureBox();
      this.Icon78 = new System.Windows.Forms.PictureBox();
      this.Icon79 = new System.Windows.Forms.PictureBox();
      this.Icon80 = new System.Windows.Forms.PictureBox();
      this.Icon81 = new System.Windows.Forms.PictureBox();
      this.Icon82 = new System.Windows.Forms.PictureBox();
      this.Icon83 = new System.Windows.Forms.PictureBox();
      this.Icon84 = new System.Windows.Forms.PictureBox();
      this.Icon85 = new System.Windows.Forms.PictureBox();
      this.Icon86 = new System.Windows.Forms.PictureBox();
      this.Icon87 = new System.Windows.Forms.PictureBox();
      this.Icon88 = new System.Windows.Forms.PictureBox();
      this.Icon89 = new System.Windows.Forms.PictureBox();
      this.Icon90 = new System.Windows.Forms.PictureBox();
      this.Icon91 = new System.Windows.Forms.PictureBox();
      this.Icon92 = new System.Windows.Forms.PictureBox();
      this.Icon93 = new System.Windows.Forms.PictureBox();
      this.Icon94 = new System.Windows.Forms.PictureBox();
      this.Icon95 = new System.Windows.Forms.PictureBox();
      this.Icon96 = new System.Windows.Forms.PictureBox();
      this.Icon97 = new System.Windows.Forms.PictureBox();
      this.Icon98 = new System.Windows.Forms.PictureBox();
      this.Icon99 = new System.Windows.Forms.PictureBox();
      this.Icon100 = new System.Windows.Forms.PictureBox();
      this.Icon101 = new System.Windows.Forms.PictureBox();
      this.Icon102 = new System.Windows.Forms.PictureBox();
      this.Icon103 = new System.Windows.Forms.PictureBox();
      this.Icon104 = new System.Windows.Forms.PictureBox();
      this.Icon105 = new System.Windows.Forms.PictureBox();
      this.Icon106 = new System.Windows.Forms.PictureBox();
      this.Icon107 = new System.Windows.Forms.PictureBox();
      this.Icon108 = new System.Windows.Forms.PictureBox();
      this.Icon109 = new System.Windows.Forms.PictureBox();
      this.Icon110 = new System.Windows.Forms.PictureBox();
      this.Icon111 = new System.Windows.Forms.PictureBox();
      this.Icon112 = new System.Windows.Forms.PictureBox();
      this.Icon113 = new System.Windows.Forms.PictureBox();
      this.Icon114 = new System.Windows.Forms.PictureBox();
      this.Icon115 = new System.Windows.Forms.PictureBox();
      this.Icon116 = new System.Windows.Forms.PictureBox();
      this.Icon117 = new System.Windows.Forms.PictureBox();
      this.Icon118 = new System.Windows.Forms.PictureBox();
      this.Icon119 = new System.Windows.Forms.PictureBox();
      this.Icon120 = new System.Windows.Forms.PictureBox();
      this.Icon121 = new System.Windows.Forms.PictureBox();
      this.Icon122 = new System.Windows.Forms.PictureBox();
      this.Icon123 = new System.Windows.Forms.PictureBox();
      this.Icon124 = new System.Windows.Forms.PictureBox();
      this.Icon125 = new System.Windows.Forms.PictureBox();
      this.Icon126 = new System.Windows.Forms.PictureBox();
      this.Icon127 = new System.Windows.Forms.PictureBox();
      this.Icon128 = new System.Windows.Forms.PictureBox();
      this.Icon129 = new System.Windows.Forms.PictureBox();
      this.Icon130 = new System.Windows.Forms.PictureBox();
      this.Icon131 = new System.Windows.Forms.PictureBox();
      this.Icon132 = new System.Windows.Forms.PictureBox();
      this.Icon133 = new System.Windows.Forms.PictureBox();
      this.Icon134 = new System.Windows.Forms.PictureBox();
      this.Icon135 = new System.Windows.Forms.PictureBox();
      this.Icon136 = new System.Windows.Forms.PictureBox();
      this.Icon137 = new System.Windows.Forms.PictureBox();
      this.Icon138 = new System.Windows.Forms.PictureBox();
      this.Icon139 = new System.Windows.Forms.PictureBox();
      this.Icon140 = new System.Windows.Forms.PictureBox();
      this.Icon141 = new System.Windows.Forms.PictureBox();
      this.Icon142 = new System.Windows.Forms.PictureBox();
      this.Icon143 = new System.Windows.Forms.PictureBox();
      this.Icon144 = new System.Windows.Forms.PictureBox();
      this.Icon145 = new System.Windows.Forms.PictureBox();
      this.Icon146 = new System.Windows.Forms.PictureBox();
      this.Icon147 = new System.Windows.Forms.PictureBox();
      this.Icon148 = new System.Windows.Forms.PictureBox();
      this.Icon149 = new System.Windows.Forms.PictureBox();
      this.Icon150 = new System.Windows.Forms.PictureBox();
      this.Icon151 = new System.Windows.Forms.PictureBox();
      this.Icon152 = new System.Windows.Forms.PictureBox();
      this.Icon153 = new System.Windows.Forms.PictureBox();
      this.Icon154 = new System.Windows.Forms.PictureBox();
      this.Icon155 = new System.Windows.Forms.PictureBox();
      this.Icon156 = new System.Windows.Forms.PictureBox();
      this.Icon157 = new System.Windows.Forms.PictureBox();
      this.Icon158 = new System.Windows.Forms.PictureBox();
      this.Icon159 = new System.Windows.Forms.PictureBox();
      this.Icon160 = new System.Windows.Forms.PictureBox();
      this.Icon161 = new System.Windows.Forms.PictureBox();
      this.Icon162 = new System.Windows.Forms.PictureBox();
      this.Icon163 = new System.Windows.Forms.PictureBox();
      this.Icon164 = new System.Windows.Forms.PictureBox();
      this.Icon165 = new System.Windows.Forms.PictureBox();
      this.Icon166 = new System.Windows.Forms.PictureBox();
      this.Icon167 = new System.Windows.Forms.PictureBox();
      this.Icon168 = new System.Windows.Forms.PictureBox();
      this.Icon169 = new System.Windows.Forms.PictureBox();
      this.Icon170 = new System.Windows.Forms.PictureBox();
      this.Icon171 = new System.Windows.Forms.PictureBox();
      this.Icon172 = new System.Windows.Forms.PictureBox();
      this.Icon173 = new System.Windows.Forms.PictureBox();
      this.Icon174 = new System.Windows.Forms.PictureBox();
      this.Icon175 = new System.Windows.Forms.PictureBox();
      this.Icon176 = new System.Windows.Forms.PictureBox();
      this.Icon177 = new System.Windows.Forms.PictureBox();
      this.Icon178 = new System.Windows.Forms.PictureBox();
      this.Icon179 = new System.Windows.Forms.PictureBox();
      this.Icon180 = new System.Windows.Forms.PictureBox();
      this.Icon181 = new System.Windows.Forms.PictureBox();
      this.Icon182 = new System.Windows.Forms.PictureBox();
      this.Icon183 = new System.Windows.Forms.PictureBox();
      this.Icon184 = new System.Windows.Forms.PictureBox();
      this.Icon185 = new System.Windows.Forms.PictureBox();
      this.Icon186 = new System.Windows.Forms.PictureBox();
      this.Icon187 = new System.Windows.Forms.PictureBox();
      this.Icon188 = new System.Windows.Forms.PictureBox();
      this.Icon189 = new System.Windows.Forms.PictureBox();
      this.Icon190 = new System.Windows.Forms.PictureBox();
      this.Icon191 = new System.Windows.Forms.PictureBox();
      this.Icon192 = new System.Windows.Forms.PictureBox();
      this.Icon193 = new System.Windows.Forms.PictureBox();
      this.Icon194 = new System.Windows.Forms.PictureBox();
      this.Icon195 = new System.Windows.Forms.PictureBox();
      this.Icon196 = new System.Windows.Forms.PictureBox();
      this.Icon197 = new System.Windows.Forms.PictureBox();
      this.Icon198 = new System.Windows.Forms.PictureBox();
      this.Icon199 = new System.Windows.Forms.PictureBox();
      this.Icon200 = new System.Windows.Forms.PictureBox();
      this.Icon201 = new System.Windows.Forms.PictureBox();
      this.Icon202 = new System.Windows.Forms.PictureBox();
      this.Icon203 = new System.Windows.Forms.PictureBox();
      this.Icon204 = new System.Windows.Forms.PictureBox();
      this.Icon205 = new System.Windows.Forms.PictureBox();
      this.Icon206 = new System.Windows.Forms.PictureBox();
      this.Icon207 = new System.Windows.Forms.PictureBox();
      this.Icon208 = new System.Windows.Forms.PictureBox();
      this.Icon209 = new System.Windows.Forms.PictureBox();
      this.Icon210 = new System.Windows.Forms.PictureBox();
      this.Icon211 = new System.Windows.Forms.PictureBox();
      this.Icon212 = new System.Windows.Forms.PictureBox();
      this.Icon213 = new System.Windows.Forms.PictureBox();
      this.Icon214 = new System.Windows.Forms.PictureBox();
      this.Icon215 = new System.Windows.Forms.PictureBox();
      this.Icon216 = new System.Windows.Forms.PictureBox();
      this.Icon217 = new System.Windows.Forms.PictureBox();
      this.Icon218 = new System.Windows.Forms.PictureBox();
      this.Icon219 = new System.Windows.Forms.PictureBox();
      this.Icon220 = new System.Windows.Forms.PictureBox();
      this.Icon221 = new System.Windows.Forms.PictureBox();
      this.Icon222 = new System.Windows.Forms.PictureBox();
      this.Icon223 = new System.Windows.Forms.PictureBox();
      this.Icon224 = new System.Windows.Forms.PictureBox();
      this.Icon225 = new System.Windows.Forms.PictureBox();
      this.Icon226 = new System.Windows.Forms.PictureBox();
      this.Icon227 = new System.Windows.Forms.PictureBox();
      this.Icon228 = new System.Windows.Forms.PictureBox();
      this.Icon229 = new System.Windows.Forms.PictureBox();
      this.Icon230 = new System.Windows.Forms.PictureBox();
      this.Icon231 = new System.Windows.Forms.PictureBox();
      this.Icon232 = new System.Windows.Forms.PictureBox();
      this.Icon233 = new System.Windows.Forms.PictureBox();
      this.Icon234 = new System.Windows.Forms.PictureBox();
      this.Icon235 = new System.Windows.Forms.PictureBox();
      this.Icon236 = new System.Windows.Forms.PictureBox();
      this.Icon237 = new System.Windows.Forms.PictureBox();
      this.Icon238 = new System.Windows.Forms.PictureBox();
      this.Icon239 = new System.Windows.Forms.PictureBox();
      this.Icon240 = new System.Windows.Forms.PictureBox();
      this.Icon241 = new System.Windows.Forms.PictureBox();
      this.Icon242 = new System.Windows.Forms.PictureBox();
      this.Icon243 = new System.Windows.Forms.PictureBox();
      this.Icon244 = new System.Windows.Forms.PictureBox();
      this.Icon245 = new System.Windows.Forms.PictureBox();
      this.Icon246 = new System.Windows.Forms.PictureBox();
      this.Icon247 = new System.Windows.Forms.PictureBox();
      this.Icon248 = new System.Windows.Forms.PictureBox();
      this.Icon249 = new System.Windows.Forms.PictureBox();
      this.Icon250 = new System.Windows.Forms.PictureBox();
      this.Icon251 = new System.Windows.Forms.PictureBox();
      this.Icon252 = new System.Windows.Forms.PictureBox();
      this.Icon253 = new System.Windows.Forms.PictureBox();
      this.Icon254 = new System.Windows.Forms.PictureBox();
      this.Icon255 = new System.Windows.Forms.PictureBox();
      this.panel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.Icon0)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon3)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon4)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon5)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon6)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon7)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon8)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon9)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon10)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon11)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon12)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon13)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon14)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon15)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon16)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon17)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon18)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon19)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon20)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon21)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon22)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon23)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon24)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon25)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon26)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon27)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon28)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon29)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon30)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon31)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon32)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon33)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon34)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon35)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon36)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon37)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon38)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon39)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon40)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon41)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon42)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon43)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon44)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon45)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon46)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon47)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon48)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon49)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon50)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon51)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon52)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon53)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon54)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon55)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon56)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon57)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon58)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon59)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon60)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon61)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon62)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon63)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon64)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon65)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon66)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon67)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon68)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon69)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon70)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon71)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon72)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon73)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon74)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon75)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon76)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon77)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon78)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon79)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon80)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon81)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon82)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon83)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon84)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon85)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon86)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon87)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon88)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon89)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon90)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon91)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon92)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon93)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon94)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon95)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon96)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon97)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon98)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon99)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon100)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon101)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon102)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon103)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon104)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon105)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon106)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon107)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon108)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon109)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon110)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon111)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon112)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon113)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon114)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon115)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon116)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon117)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon118)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon119)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon120)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon121)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon122)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon123)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon124)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon125)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon126)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon127)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon128)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon129)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon130)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon131)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon132)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon133)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon134)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon135)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon136)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon137)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon138)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon139)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon140)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon141)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon142)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon143)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon144)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon145)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon146)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon147)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon148)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon149)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon150)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon151)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon152)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon153)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon154)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon155)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon156)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon157)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon158)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon159)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon160)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon161)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon162)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon163)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon164)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon165)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon166)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon167)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon168)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon169)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon170)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon171)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon172)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon173)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon174)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon175)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon176)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon177)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon178)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon179)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon180)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon181)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon182)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon183)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon184)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon185)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon186)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon187)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon188)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon189)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon190)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon191)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon192)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon193)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon194)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon195)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon196)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon197)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon198)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon199)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon200)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon201)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon202)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon203)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon204)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon205)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon206)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon207)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon208)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon209)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon210)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon211)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon212)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon213)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon214)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon215)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon216)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon217)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon218)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon219)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon220)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon221)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon222)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon223)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon224)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon225)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon226)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon227)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon228)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon229)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon230)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon231)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon232)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon233)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon234)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon235)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon236)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon237)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon238)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon239)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon240)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon241)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon242)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon243)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon244)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon245)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon246)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon247)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon248)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon249)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon250)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon251)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon252)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon253)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon254)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon255)).BeginInit();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.panel1.Controls.Add(this.lblEditIndex);
      this.panel1.Controls.Add(this.cmdSaveEdit);
      this.panel1.Controls.Add(this.cmdCancelEdit);
      this.panel1.Controls.Add(this.cmdInvert);
      this.panel1.Controls.Add(this.cmdSetAll);
      this.panel1.Controls.Add(this.cmdClearAll);
      this.panel1.Controls.Add(this.lblCurrentIcon);
      this.panel1.Controls.Add(this.C4_B0);
      this.panel1.Controls.Add(this.C4_B1);
      this.panel1.Controls.Add(this.C4_B2);
      this.panel1.Controls.Add(this.C4_B3);
      this.panel1.Controls.Add(this.C4_B4);
      this.panel1.Controls.Add(this.C4_B5);
      this.panel1.Controls.Add(this.C4_B6);
      this.panel1.Controls.Add(this.C4_B7);
      this.panel1.Controls.Add(this.C5_B0);
      this.panel1.Controls.Add(this.C5_B1);
      this.panel1.Controls.Add(this.C5_B2);
      this.panel1.Controls.Add(this.C5_B3);
      this.panel1.Controls.Add(this.C5_B4);
      this.panel1.Controls.Add(this.C5_B5);
      this.panel1.Controls.Add(this.C5_B6);
      this.panel1.Controls.Add(this.C5_B7);
      this.panel1.Controls.Add(this.C2_B0);
      this.panel1.Controls.Add(this.C2_B1);
      this.panel1.Controls.Add(this.C2_B2);
      this.panel1.Controls.Add(this.C2_B3);
      this.panel1.Controls.Add(this.C2_B4);
      this.panel1.Controls.Add(this.C2_B5);
      this.panel1.Controls.Add(this.C2_B6);
      this.panel1.Controls.Add(this.C2_B7);
      this.panel1.Controls.Add(this.C3_B0);
      this.panel1.Controls.Add(this.C3_B1);
      this.panel1.Controls.Add(this.C3_B2);
      this.panel1.Controls.Add(this.C3_B3);
      this.panel1.Controls.Add(this.C3_B4);
      this.panel1.Controls.Add(this.C3_B5);
      this.panel1.Controls.Add(this.C3_B6);
      this.panel1.Controls.Add(this.C3_B7);
      this.panel1.Controls.Add(this.C0_B0);
      this.panel1.Controls.Add(this.C0_B1);
      this.panel1.Controls.Add(this.C0_B2);
      this.panel1.Controls.Add(this.C0_B3);
      this.panel1.Controls.Add(this.C0_B4);
      this.panel1.Controls.Add(this.C0_B5);
      this.panel1.Controls.Add(this.C0_B6);
      this.panel1.Controls.Add(this.C0_B7);
      this.panel1.Controls.Add(this.C1_B0);
      this.panel1.Controls.Add(this.C1_B1);
      this.panel1.Controls.Add(this.C1_B2);
      this.panel1.Controls.Add(this.C1_B3);
      this.panel1.Controls.Add(this.C1_B4);
      this.panel1.Controls.Add(this.C1_B5);
      this.panel1.Controls.Add(this.C1_B6);
      this.panel1.Controls.Add(this.C1_B7);
      this.panel1.Enabled = false;
      this.panel1.Location = new System.Drawing.Point(6, 7);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(233, 144);
      this.panel1.TabIndex = 288;
      // 
      // lblEditIndex
      // 
      this.lblEditIndex.AutoSize = true;
      this.lblEditIndex.Location = new System.Drawing.Point(95, 76);
      this.lblEditIndex.Name = "lblEditIndex";
      this.lblEditIndex.Size = new System.Drawing.Size(61, 13);
      this.lblEditIndex.TabIndex = 550;
      this.lblEditIndex.Text = "lblEditIndex";
      this.lblEditIndex.Visible = false;
      // 
      // cmdSaveEdit
      // 
      this.cmdSaveEdit.Enabled = false;
      this.cmdSaveEdit.Location = new System.Drawing.Point(170, 3);
      this.cmdSaveEdit.Name = "cmdSaveEdit";
      this.cmdSaveEdit.Size = new System.Drawing.Size(58, 23);
      this.cmdSaveEdit.TabIndex = 549;
      this.cmdSaveEdit.Text = "Save";
      this.cmdSaveEdit.UseVisualStyleBackColor = true;
      this.cmdSaveEdit.Click += new System.EventHandler(this.cmdSaveEdit_Click);
      // 
      // cmdCancelEdit
      // 
      this.cmdCancelEdit.Enabled = false;
      this.cmdCancelEdit.Location = new System.Drawing.Point(170, 27);
      this.cmdCancelEdit.Name = "cmdCancelEdit";
      this.cmdCancelEdit.Size = new System.Drawing.Size(58, 23);
      this.cmdCancelEdit.TabIndex = 548;
      this.cmdCancelEdit.Text = "Cancel";
      this.cmdCancelEdit.UseVisualStyleBackColor = true;
      this.cmdCancelEdit.Click += new System.EventHandler(this.cmdCancelEdit_Click);
      // 
      // cmdInvert
      // 
      this.cmdInvert.Enabled = false;
      this.cmdInvert.Location = new System.Drawing.Point(87, 49);
      this.cmdInvert.Name = "cmdInvert";
      this.cmdInvert.Size = new System.Drawing.Size(58, 23);
      this.cmdInvert.TabIndex = 547;
      this.cmdInvert.Text = "Invert";
      this.cmdInvert.UseVisualStyleBackColor = true;
      this.cmdInvert.Click += new System.EventHandler(this.cmdInvert_Click);
      // 
      // cmdSetAll
      // 
      this.cmdSetAll.Enabled = false;
      this.cmdSetAll.Location = new System.Drawing.Point(87, 26);
      this.cmdSetAll.Name = "cmdSetAll";
      this.cmdSetAll.Size = new System.Drawing.Size(58, 23);
      this.cmdSetAll.TabIndex = 546;
      this.cmdSetAll.Text = "Set All";
      this.cmdSetAll.UseVisualStyleBackColor = true;
      this.cmdSetAll.Click += new System.EventHandler(this.cmdSetAll_Click);
      // 
      // cmdClearAll
      // 
      this.cmdClearAll.Enabled = false;
      this.cmdClearAll.Location = new System.Drawing.Point(87, 3);
      this.cmdClearAll.Name = "cmdClearAll";
      this.cmdClearAll.Size = new System.Drawing.Size(58, 23);
      this.cmdClearAll.TabIndex = 545;
      this.cmdClearAll.Text = "Clear All";
      this.cmdClearAll.UseVisualStyleBackColor = true;
      this.cmdClearAll.Click += new System.EventHandler(this.cmdClearAll_Click);
      // 
      // lblCurrentIcon
      // 
      this.lblCurrentIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.lblCurrentIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblCurrentIcon.Location = new System.Drawing.Point(3, 104);
      this.lblCurrentIcon.Name = "lblCurrentIcon";
      this.lblCurrentIcon.Size = new System.Drawing.Size(225, 33);
      this.lblCurrentIcon.TabIndex = 544;
      this.lblCurrentIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // C4_B0
      // 
      this.C4_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B0.Location = new System.Drawing.Point(53, 87);
      this.C4_B0.Name = "C4_B0";
      this.C4_B0.Size = new System.Drawing.Size(14, 14);
      this.C4_B0.TabIndex = 335;
      this.C4_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B0.ThreeState = true;
      this.C4_B0.UseVisualStyleBackColor = true;
      this.C4_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4_B1
      // 
      this.C4_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B1.Location = new System.Drawing.Point(53, 75);
      this.C4_B1.Name = "C4_B1";
      this.C4_B1.Size = new System.Drawing.Size(14, 14);
      this.C4_B1.TabIndex = 334;
      this.C4_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B1.ThreeState = true;
      this.C4_B1.UseVisualStyleBackColor = true;
      this.C4_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4_B2
      // 
      this.C4_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B2.Location = new System.Drawing.Point(53, 63);
      this.C4_B2.Name = "C4_B2";
      this.C4_B2.Size = new System.Drawing.Size(14, 14);
      this.C4_B2.TabIndex = 333;
      this.C4_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B2.ThreeState = true;
      this.C4_B2.UseVisualStyleBackColor = true;
      this.C4_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4_B3
      // 
      this.C4_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B3.Location = new System.Drawing.Point(53, 51);
      this.C4_B3.Name = "C4_B3";
      this.C4_B3.Size = new System.Drawing.Size(14, 14);
      this.C4_B3.TabIndex = 332;
      this.C4_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B3.ThreeState = true;
      this.C4_B3.UseVisualStyleBackColor = true;
      this.C4_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4_B4
      // 
      this.C4_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B4.Location = new System.Drawing.Point(53, 39);
      this.C4_B4.Name = "C4_B4";
      this.C4_B4.Size = new System.Drawing.Size(14, 14);
      this.C4_B4.TabIndex = 331;
      this.C4_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B4.ThreeState = true;
      this.C4_B4.UseVisualStyleBackColor = true;
      this.C4_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4_B5
      // 
      this.C4_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B5.Location = new System.Drawing.Point(53, 27);
      this.C4_B5.Name = "C4_B5";
      this.C4_B5.Size = new System.Drawing.Size(14, 14);
      this.C4_B5.TabIndex = 330;
      this.C4_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B5.ThreeState = true;
      this.C4_B5.UseVisualStyleBackColor = true;
      this.C4_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4_B6
      // 
      this.C4_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B6.Location = new System.Drawing.Point(53, 15);
      this.C4_B6.Name = "C4_B6";
      this.C4_B6.Size = new System.Drawing.Size(14, 14);
      this.C4_B6.TabIndex = 329;
      this.C4_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B6.ThreeState = true;
      this.C4_B6.UseVisualStyleBackColor = true;
      this.C4_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4_B7
      // 
      this.C4_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B7.Location = new System.Drawing.Point(53, 3);
      this.C4_B7.Name = "C4_B7";
      this.C4_B7.Size = new System.Drawing.Size(14, 14);
      this.C4_B7.TabIndex = 328;
      this.C4_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4_B7.ThreeState = true;
      this.C4_B7.UseVisualStyleBackColor = true;
      this.C4_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5_B0
      // 
      this.C5_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B0.Location = new System.Drawing.Point(66, 87);
      this.C5_B0.Name = "C5_B0";
      this.C5_B0.Size = new System.Drawing.Size(14, 14);
      this.C5_B0.TabIndex = 327;
      this.C5_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B0.ThreeState = true;
      this.C5_B0.UseVisualStyleBackColor = true;
      this.C5_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5_B1
      // 
      this.C5_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B1.Location = new System.Drawing.Point(66, 75);
      this.C5_B1.Name = "C5_B1";
      this.C5_B1.Size = new System.Drawing.Size(14, 14);
      this.C5_B1.TabIndex = 326;
      this.C5_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B1.ThreeState = true;
      this.C5_B1.UseVisualStyleBackColor = true;
      this.C5_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5_B2
      // 
      this.C5_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B2.Location = new System.Drawing.Point(66, 63);
      this.C5_B2.Name = "C5_B2";
      this.C5_B2.Size = new System.Drawing.Size(14, 14);
      this.C5_B2.TabIndex = 325;
      this.C5_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B2.ThreeState = true;
      this.C5_B2.UseVisualStyleBackColor = true;
      this.C5_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5_B3
      // 
      this.C5_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B3.Location = new System.Drawing.Point(66, 51);
      this.C5_B3.Name = "C5_B3";
      this.C5_B3.Size = new System.Drawing.Size(14, 14);
      this.C5_B3.TabIndex = 324;
      this.C5_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B3.ThreeState = true;
      this.C5_B3.UseVisualStyleBackColor = true;
      this.C5_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5_B4
      // 
      this.C5_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B4.Location = new System.Drawing.Point(66, 39);
      this.C5_B4.Name = "C5_B4";
      this.C5_B4.Size = new System.Drawing.Size(14, 14);
      this.C5_B4.TabIndex = 323;
      this.C5_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B4.ThreeState = true;
      this.C5_B4.UseVisualStyleBackColor = true;
      this.C5_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5_B5
      // 
      this.C5_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B5.Location = new System.Drawing.Point(66, 27);
      this.C5_B5.Name = "C5_B5";
      this.C5_B5.Size = new System.Drawing.Size(14, 14);
      this.C5_B5.TabIndex = 322;
      this.C5_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B5.ThreeState = true;
      this.C5_B5.UseVisualStyleBackColor = true;
      this.C5_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5_B6
      // 
      this.C5_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B6.Location = new System.Drawing.Point(66, 15);
      this.C5_B6.Name = "C5_B6";
      this.C5_B6.Size = new System.Drawing.Size(14, 14);
      this.C5_B6.TabIndex = 321;
      this.C5_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B6.ThreeState = true;
      this.C5_B6.UseVisualStyleBackColor = true;
      this.C5_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5_B7
      // 
      this.C5_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B7.Location = new System.Drawing.Point(66, 3);
      this.C5_B7.Name = "C5_B7";
      this.C5_B7.Size = new System.Drawing.Size(14, 14);
      this.C5_B7.TabIndex = 320;
      this.C5_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5_B7.ThreeState = true;
      this.C5_B7.UseVisualStyleBackColor = true;
      this.C5_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2_B0
      // 
      this.C2_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B0.Location = new System.Drawing.Point(28, 87);
      this.C2_B0.Name = "C2_B0";
      this.C2_B0.Size = new System.Drawing.Size(14, 14);
      this.C2_B0.TabIndex = 319;
      this.C2_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B0.ThreeState = true;
      this.C2_B0.UseVisualStyleBackColor = true;
      this.C2_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2_B1
      // 
      this.C2_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B1.Location = new System.Drawing.Point(28, 75);
      this.C2_B1.Name = "C2_B1";
      this.C2_B1.Size = new System.Drawing.Size(14, 14);
      this.C2_B1.TabIndex = 318;
      this.C2_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B1.ThreeState = true;
      this.C2_B1.UseVisualStyleBackColor = true;
      this.C2_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2_B2
      // 
      this.C2_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B2.Location = new System.Drawing.Point(28, 63);
      this.C2_B2.Name = "C2_B2";
      this.C2_B2.Size = new System.Drawing.Size(14, 14);
      this.C2_B2.TabIndex = 317;
      this.C2_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B2.ThreeState = true;
      this.C2_B2.UseVisualStyleBackColor = true;
      this.C2_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2_B3
      // 
      this.C2_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B3.Location = new System.Drawing.Point(28, 51);
      this.C2_B3.Name = "C2_B3";
      this.C2_B3.Size = new System.Drawing.Size(14, 14);
      this.C2_B3.TabIndex = 316;
      this.C2_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B3.ThreeState = true;
      this.C2_B3.UseVisualStyleBackColor = true;
      this.C2_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2_B4
      // 
      this.C2_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B4.Location = new System.Drawing.Point(28, 39);
      this.C2_B4.Name = "C2_B4";
      this.C2_B4.Size = new System.Drawing.Size(14, 14);
      this.C2_B4.TabIndex = 315;
      this.C2_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B4.ThreeState = true;
      this.C2_B4.UseVisualStyleBackColor = true;
      this.C2_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2_B5
      // 
      this.C2_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B5.Location = new System.Drawing.Point(28, 27);
      this.C2_B5.Name = "C2_B5";
      this.C2_B5.Size = new System.Drawing.Size(14, 14);
      this.C2_B5.TabIndex = 314;
      this.C2_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B5.ThreeState = true;
      this.C2_B5.UseVisualStyleBackColor = true;
      this.C2_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2_B6
      // 
      this.C2_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B6.Location = new System.Drawing.Point(28, 15);
      this.C2_B6.Name = "C2_B6";
      this.C2_B6.Size = new System.Drawing.Size(14, 14);
      this.C2_B6.TabIndex = 313;
      this.C2_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B6.ThreeState = true;
      this.C2_B6.UseVisualStyleBackColor = true;
      this.C2_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2_B7
      // 
      this.C2_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B7.Location = new System.Drawing.Point(28, 3);
      this.C2_B7.Name = "C2_B7";
      this.C2_B7.Size = new System.Drawing.Size(14, 14);
      this.C2_B7.TabIndex = 312;
      this.C2_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2_B7.ThreeState = true;
      this.C2_B7.UseVisualStyleBackColor = true;
      this.C2_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3_B0
      // 
      this.C3_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B0.Location = new System.Drawing.Point(41, 87);
      this.C3_B0.Name = "C3_B0";
      this.C3_B0.Size = new System.Drawing.Size(14, 14);
      this.C3_B0.TabIndex = 311;
      this.C3_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B0.ThreeState = true;
      this.C3_B0.UseVisualStyleBackColor = true;
      this.C3_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3_B1
      // 
      this.C3_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B1.Location = new System.Drawing.Point(41, 75);
      this.C3_B1.Name = "C3_B1";
      this.C3_B1.Size = new System.Drawing.Size(14, 14);
      this.C3_B1.TabIndex = 310;
      this.C3_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B1.ThreeState = true;
      this.C3_B1.UseVisualStyleBackColor = true;
      this.C3_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3_B2
      // 
      this.C3_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B2.Location = new System.Drawing.Point(41, 63);
      this.C3_B2.Name = "C3_B2";
      this.C3_B2.Size = new System.Drawing.Size(14, 14);
      this.C3_B2.TabIndex = 309;
      this.C3_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B2.ThreeState = true;
      this.C3_B2.UseVisualStyleBackColor = true;
      this.C3_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3_B3
      // 
      this.C3_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B3.Location = new System.Drawing.Point(41, 51);
      this.C3_B3.Name = "C3_B3";
      this.C3_B3.Size = new System.Drawing.Size(14, 14);
      this.C3_B3.TabIndex = 308;
      this.C3_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B3.ThreeState = true;
      this.C3_B3.UseVisualStyleBackColor = true;
      this.C3_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3_B4
      // 
      this.C3_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B4.Location = new System.Drawing.Point(41, 39);
      this.C3_B4.Name = "C3_B4";
      this.C3_B4.Size = new System.Drawing.Size(14, 14);
      this.C3_B4.TabIndex = 307;
      this.C3_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B4.ThreeState = true;
      this.C3_B4.UseVisualStyleBackColor = true;
      this.C3_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3_B5
      // 
      this.C3_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B5.Location = new System.Drawing.Point(41, 27);
      this.C3_B5.Name = "C3_B5";
      this.C3_B5.Size = new System.Drawing.Size(14, 14);
      this.C3_B5.TabIndex = 306;
      this.C3_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B5.ThreeState = true;
      this.C3_B5.UseVisualStyleBackColor = true;
      this.C3_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3_B6
      // 
      this.C3_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B6.Location = new System.Drawing.Point(41, 15);
      this.C3_B6.Name = "C3_B6";
      this.C3_B6.Size = new System.Drawing.Size(14, 14);
      this.C3_B6.TabIndex = 305;
      this.C3_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B6.ThreeState = true;
      this.C3_B6.UseVisualStyleBackColor = true;
      this.C3_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3_B7
      // 
      this.C3_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B7.Location = new System.Drawing.Point(41, 3);
      this.C3_B7.Name = "C3_B7";
      this.C3_B7.Size = new System.Drawing.Size(14, 14);
      this.C3_B7.TabIndex = 304;
      this.C3_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3_B7.ThreeState = true;
      this.C3_B7.UseVisualStyleBackColor = true;
      this.C3_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0_B0
      // 
      this.C0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B0.Location = new System.Drawing.Point(3, 87);
      this.C0_B0.Name = "C0_B0";
      this.C0_B0.Size = new System.Drawing.Size(14, 14);
      this.C0_B0.TabIndex = 303;
      this.C0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B0.ThreeState = true;
      this.C0_B0.UseVisualStyleBackColor = true;
      this.C0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0_B1
      // 
      this.C0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B1.Location = new System.Drawing.Point(3, 75);
      this.C0_B1.Name = "C0_B1";
      this.C0_B1.Size = new System.Drawing.Size(14, 14);
      this.C0_B1.TabIndex = 302;
      this.C0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B1.ThreeState = true;
      this.C0_B1.UseVisualStyleBackColor = true;
      this.C0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0_B2
      // 
      this.C0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B2.Location = new System.Drawing.Point(3, 63);
      this.C0_B2.Name = "C0_B2";
      this.C0_B2.Size = new System.Drawing.Size(14, 14);
      this.C0_B2.TabIndex = 301;
      this.C0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B2.ThreeState = true;
      this.C0_B2.UseVisualStyleBackColor = true;
      this.C0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0_B3
      // 
      this.C0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B3.Location = new System.Drawing.Point(3, 51);
      this.C0_B3.Name = "C0_B3";
      this.C0_B3.Size = new System.Drawing.Size(14, 14);
      this.C0_B3.TabIndex = 300;
      this.C0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B3.ThreeState = true;
      this.C0_B3.UseVisualStyleBackColor = true;
      this.C0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0_B4
      // 
      this.C0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B4.Location = new System.Drawing.Point(3, 39);
      this.C0_B4.Name = "C0_B4";
      this.C0_B4.Size = new System.Drawing.Size(14, 14);
      this.C0_B4.TabIndex = 299;
      this.C0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B4.ThreeState = true;
      this.C0_B4.UseVisualStyleBackColor = true;
      this.C0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0_B5
      // 
      this.C0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B5.Location = new System.Drawing.Point(3, 27);
      this.C0_B5.Name = "C0_B5";
      this.C0_B5.Size = new System.Drawing.Size(14, 14);
      this.C0_B5.TabIndex = 298;
      this.C0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B5.ThreeState = true;
      this.C0_B5.UseVisualStyleBackColor = true;
      this.C0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0_B6
      // 
      this.C0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B6.Location = new System.Drawing.Point(3, 15);
      this.C0_B6.Name = "C0_B6";
      this.C0_B6.Size = new System.Drawing.Size(14, 14);
      this.C0_B6.TabIndex = 297;
      this.C0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B6.ThreeState = true;
      this.C0_B6.UseVisualStyleBackColor = true;
      this.C0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0_B7
      // 
      this.C0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B7.Location = new System.Drawing.Point(3, 3);
      this.C0_B7.Name = "C0_B7";
      this.C0_B7.Size = new System.Drawing.Size(14, 14);
      this.C0_B7.TabIndex = 296;
      this.C0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0_B7.ThreeState = true;
      this.C0_B7.UseVisualStyleBackColor = true;
      this.C0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1_B0
      // 
      this.C1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B0.Location = new System.Drawing.Point(16, 87);
      this.C1_B0.Name = "C1_B0";
      this.C1_B0.Size = new System.Drawing.Size(14, 14);
      this.C1_B0.TabIndex = 295;
      this.C1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B0.ThreeState = true;
      this.C1_B0.UseVisualStyleBackColor = true;
      this.C1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1_B1
      // 
      this.C1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B1.Location = new System.Drawing.Point(16, 75);
      this.C1_B1.Name = "C1_B1";
      this.C1_B1.Size = new System.Drawing.Size(14, 14);
      this.C1_B1.TabIndex = 294;
      this.C1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B1.ThreeState = true;
      this.C1_B1.UseVisualStyleBackColor = true;
      this.C1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1_B2
      // 
      this.C1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B2.Location = new System.Drawing.Point(16, 63);
      this.C1_B2.Name = "C1_B2";
      this.C1_B2.Size = new System.Drawing.Size(14, 14);
      this.C1_B2.TabIndex = 293;
      this.C1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B2.ThreeState = true;
      this.C1_B2.UseVisualStyleBackColor = true;
      this.C1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1_B3
      // 
      this.C1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B3.Location = new System.Drawing.Point(16, 51);
      this.C1_B3.Name = "C1_B3";
      this.C1_B3.Size = new System.Drawing.Size(14, 14);
      this.C1_B3.TabIndex = 292;
      this.C1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B3.ThreeState = true;
      this.C1_B3.UseVisualStyleBackColor = true;
      this.C1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1_B4
      // 
      this.C1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B4.Location = new System.Drawing.Point(16, 39);
      this.C1_B4.Name = "C1_B4";
      this.C1_B4.Size = new System.Drawing.Size(14, 14);
      this.C1_B4.TabIndex = 291;
      this.C1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B4.ThreeState = true;
      this.C1_B4.UseVisualStyleBackColor = true;
      this.C1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1_B5
      // 
      this.C1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B5.Location = new System.Drawing.Point(16, 27);
      this.C1_B5.Name = "C1_B5";
      this.C1_B5.Size = new System.Drawing.Size(14, 14);
      this.C1_B5.TabIndex = 290;
      this.C1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B5.ThreeState = true;
      this.C1_B5.UseVisualStyleBackColor = true;
      this.C1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1_B6
      // 
      this.C1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B6.Location = new System.Drawing.Point(16, 15);
      this.C1_B6.Name = "C1_B6";
      this.C1_B6.Size = new System.Drawing.Size(14, 14);
      this.C1_B6.TabIndex = 289;
      this.C1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B6.ThreeState = true;
      this.C1_B6.UseVisualStyleBackColor = true;
      this.C1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1_B7
      // 
      this.C1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B7.Location = new System.Drawing.Point(16, 3);
      this.C1_B7.Name = "C1_B7";
      this.C1_B7.Size = new System.Drawing.Size(14, 14);
      this.C1_B7.TabIndex = 288;
      this.C1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1_B7.ThreeState = true;
      this.C1_B7.UseVisualStyleBackColor = true;
      this.C1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // cmdLoadInternal
      // 
      this.cmdLoadInternal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdLoadInternal.Location = new System.Drawing.Point(314, 39);
      this.cmdLoadInternal.Name = "cmdLoadInternal";
      this.cmdLoadInternal.Size = new System.Drawing.Size(75, 23);
      this.cmdLoadInternal.TabIndex = 548;
      this.cmdLoadInternal.Text = "Internal";
      this.cmdLoadInternal.UseVisualStyleBackColor = true;
      this.cmdLoadInternal.Click += new System.EventHandler(this.cmdLoadInternal_Click);
      // 
      // cmdLoadCustom
      // 
      this.cmdLoadCustom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdLoadCustom.Location = new System.Drawing.Point(314, 14);
      this.cmdLoadCustom.Name = "cmdLoadCustom";
      this.cmdLoadCustom.Size = new System.Drawing.Size(75, 23);
      this.cmdLoadCustom.TabIndex = 549;
      this.cmdLoadCustom.Text = "Custom";
      this.cmdLoadCustom.UseVisualStyleBackColor = true;
      this.cmdLoadCustom.Click += new System.EventHandler(this.cmdLoadCustom_Click);
      // 
      // cmdSave
      // 
      this.cmdSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdSave.Enabled = false;
      this.cmdSave.Location = new System.Drawing.Point(233, 456);
      this.cmdSave.Name = "cmdSave";
      this.cmdSave.Size = new System.Drawing.Size(75, 23);
      this.cmdSave.TabIndex = 550;
      this.cmdSave.Text = "Save";
      this.cmdSave.UseVisualStyleBackColor = true;
      this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
      // 
      // cmdExit
      // 
      this.cmdExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdExit.Location = new System.Drawing.Point(314, 456);
      this.cmdExit.Name = "cmdExit";
      this.cmdExit.Size = new System.Drawing.Size(75, 23);
      this.cmdExit.TabIndex = 551;
      this.cmdExit.Text = "Exit";
      this.cmdExit.UseVisualStyleBackColor = true;
      this.cmdExit.Click += new System.EventHandler(this.cmdExit_Click);
      // 
      // Icon0
      // 
      this.Icon0.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon0.Enabled = false;
      this.Icon0.Location = new System.Drawing.Point(20, 163);
      this.Icon0.Name = "Icon0";
      this.Icon0.Size = new System.Drawing.Size(14, 18);
      this.Icon0.TabIndex = 1;
      this.Icon0.TabStop = false;
      this.Icon0.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon1
      // 
      this.Icon1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon1.Enabled = false;
      this.Icon1.Location = new System.Drawing.Point(33, 163);
      this.Icon1.Name = "Icon1";
      this.Icon1.Size = new System.Drawing.Size(14, 18);
      this.Icon1.TabIndex = 2;
      this.Icon1.TabStop = false;
      this.Icon1.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon2
      // 
      this.Icon2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon2.Enabled = false;
      this.Icon2.Location = new System.Drawing.Point(46, 163);
      this.Icon2.Name = "Icon2";
      this.Icon2.Size = new System.Drawing.Size(14, 18);
      this.Icon2.TabIndex = 3;
      this.Icon2.TabStop = false;
      this.Icon2.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon3
      // 
      this.Icon3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon3.Enabled = false;
      this.Icon3.Location = new System.Drawing.Point(59, 163);
      this.Icon3.Name = "Icon3";
      this.Icon3.Size = new System.Drawing.Size(14, 18);
      this.Icon3.TabIndex = 4;
      this.Icon3.TabStop = false;
      this.Icon3.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon4
      // 
      this.Icon4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon4.Enabled = false;
      this.Icon4.Location = new System.Drawing.Point(72, 163);
      this.Icon4.Name = "Icon4";
      this.Icon4.Size = new System.Drawing.Size(14, 18);
      this.Icon4.TabIndex = 5;
      this.Icon4.TabStop = false;
      this.Icon4.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon5
      // 
      this.Icon5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon5.Enabled = false;
      this.Icon5.Location = new System.Drawing.Point(85, 163);
      this.Icon5.Name = "Icon5";
      this.Icon5.Size = new System.Drawing.Size(14, 18);
      this.Icon5.TabIndex = 6;
      this.Icon5.TabStop = false;
      this.Icon5.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon6
      // 
      this.Icon6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon6.Enabled = false;
      this.Icon6.Location = new System.Drawing.Point(98, 163);
      this.Icon6.Name = "Icon6";
      this.Icon6.Size = new System.Drawing.Size(14, 18);
      this.Icon6.TabIndex = 7;
      this.Icon6.TabStop = false;
      this.Icon6.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon7
      // 
      this.Icon7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon7.Enabled = false;
      this.Icon7.Location = new System.Drawing.Point(111, 163);
      this.Icon7.Name = "Icon7";
      this.Icon7.Size = new System.Drawing.Size(14, 18);
      this.Icon7.TabIndex = 8;
      this.Icon7.TabStop = false;
      this.Icon7.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon8
      // 
      this.Icon8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon8.Enabled = false;
      this.Icon8.Location = new System.Drawing.Point(124, 163);
      this.Icon8.Name = "Icon8";
      this.Icon8.Size = new System.Drawing.Size(14, 18);
      this.Icon8.TabIndex = 9;
      this.Icon8.TabStop = false;
      this.Icon8.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon9
      // 
      this.Icon9.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon9.Enabled = false;
      this.Icon9.Location = new System.Drawing.Point(137, 163);
      this.Icon9.Name = "Icon9";
      this.Icon9.Size = new System.Drawing.Size(14, 18);
      this.Icon9.TabIndex = 10;
      this.Icon9.TabStop = false;
      this.Icon9.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon10
      // 
      this.Icon10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon10.Enabled = false;
      this.Icon10.Location = new System.Drawing.Point(150, 163);
      this.Icon10.Name = "Icon10";
      this.Icon10.Size = new System.Drawing.Size(14, 18);
      this.Icon10.TabIndex = 552;
      this.Icon10.TabStop = false;
      this.Icon10.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon11
      // 
      this.Icon11.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon11.Enabled = false;
      this.Icon11.Location = new System.Drawing.Point(163, 163);
      this.Icon11.Name = "Icon11";
      this.Icon11.Size = new System.Drawing.Size(14, 18);
      this.Icon11.TabIndex = 553;
      this.Icon11.TabStop = false;
      this.Icon11.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon12
      // 
      this.Icon12.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon12.Enabled = false;
      this.Icon12.Location = new System.Drawing.Point(176, 163);
      this.Icon12.Name = "Icon12";
      this.Icon12.Size = new System.Drawing.Size(14, 18);
      this.Icon12.TabIndex = 554;
      this.Icon12.TabStop = false;
      this.Icon12.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon13
      // 
      this.Icon13.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon13.Enabled = false;
      this.Icon13.Location = new System.Drawing.Point(189, 163);
      this.Icon13.Name = "Icon13";
      this.Icon13.Size = new System.Drawing.Size(14, 18);
      this.Icon13.TabIndex = 555;
      this.Icon13.TabStop = false;
      this.Icon13.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon14
      // 
      this.Icon14.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon14.Enabled = false;
      this.Icon14.Location = new System.Drawing.Point(202, 163);
      this.Icon14.Name = "Icon14";
      this.Icon14.Size = new System.Drawing.Size(14, 18);
      this.Icon14.TabIndex = 556;
      this.Icon14.TabStop = false;
      this.Icon14.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon15
      // 
      this.Icon15.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon15.Enabled = false;
      this.Icon15.Location = new System.Drawing.Point(215, 163);
      this.Icon15.Name = "Icon15";
      this.Icon15.Size = new System.Drawing.Size(14, 18);
      this.Icon15.TabIndex = 557;
      this.Icon15.TabStop = false;
      this.Icon15.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon16
      // 
      this.Icon16.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon16.Enabled = false;
      this.Icon16.Location = new System.Drawing.Point(20, 180);
      this.Icon16.Name = "Icon16";
      this.Icon16.Size = new System.Drawing.Size(14, 18);
      this.Icon16.TabIndex = 558;
      this.Icon16.TabStop = false;
      this.Icon16.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon17
      // 
      this.Icon17.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon17.Enabled = false;
      this.Icon17.Location = new System.Drawing.Point(33, 180);
      this.Icon17.Name = "Icon17";
      this.Icon17.Size = new System.Drawing.Size(14, 18);
      this.Icon17.TabIndex = 559;
      this.Icon17.TabStop = false;
      this.Icon17.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon18
      // 
      this.Icon18.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon18.Enabled = false;
      this.Icon18.Location = new System.Drawing.Point(46, 180);
      this.Icon18.Name = "Icon18";
      this.Icon18.Size = new System.Drawing.Size(14, 18);
      this.Icon18.TabIndex = 560;
      this.Icon18.TabStop = false;
      this.Icon18.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon19
      // 
      this.Icon19.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon19.Enabled = false;
      this.Icon19.Location = new System.Drawing.Point(59, 180);
      this.Icon19.Name = "Icon19";
      this.Icon19.Size = new System.Drawing.Size(14, 18);
      this.Icon19.TabIndex = 561;
      this.Icon19.TabStop = false;
      this.Icon19.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon20
      // 
      this.Icon20.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon20.Enabled = false;
      this.Icon20.Location = new System.Drawing.Point(72, 180);
      this.Icon20.Name = "Icon20";
      this.Icon20.Size = new System.Drawing.Size(14, 18);
      this.Icon20.TabIndex = 562;
      this.Icon20.TabStop = false;
      this.Icon20.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon21
      // 
      this.Icon21.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon21.Enabled = false;
      this.Icon21.Location = new System.Drawing.Point(85, 180);
      this.Icon21.Name = "Icon21";
      this.Icon21.Size = new System.Drawing.Size(14, 18);
      this.Icon21.TabIndex = 563;
      this.Icon21.TabStop = false;
      this.Icon21.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon22
      // 
      this.Icon22.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon22.Enabled = false;
      this.Icon22.Location = new System.Drawing.Point(98, 180);
      this.Icon22.Name = "Icon22";
      this.Icon22.Size = new System.Drawing.Size(14, 18);
      this.Icon22.TabIndex = 564;
      this.Icon22.TabStop = false;
      this.Icon22.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon23
      // 
      this.Icon23.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon23.Enabled = false;
      this.Icon23.Location = new System.Drawing.Point(111, 180);
      this.Icon23.Name = "Icon23";
      this.Icon23.Size = new System.Drawing.Size(14, 18);
      this.Icon23.TabIndex = 565;
      this.Icon23.TabStop = false;
      this.Icon23.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon24
      // 
      this.Icon24.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon24.Enabled = false;
      this.Icon24.Location = new System.Drawing.Point(124, 180);
      this.Icon24.Name = "Icon24";
      this.Icon24.Size = new System.Drawing.Size(14, 18);
      this.Icon24.TabIndex = 566;
      this.Icon24.TabStop = false;
      this.Icon24.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon25
      // 
      this.Icon25.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon25.Enabled = false;
      this.Icon25.Location = new System.Drawing.Point(137, 180);
      this.Icon25.Name = "Icon25";
      this.Icon25.Size = new System.Drawing.Size(14, 18);
      this.Icon25.TabIndex = 567;
      this.Icon25.TabStop = false;
      this.Icon25.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon26
      // 
      this.Icon26.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon26.Enabled = false;
      this.Icon26.Location = new System.Drawing.Point(150, 180);
      this.Icon26.Name = "Icon26";
      this.Icon26.Size = new System.Drawing.Size(14, 18);
      this.Icon26.TabIndex = 568;
      this.Icon26.TabStop = false;
      this.Icon26.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon27
      // 
      this.Icon27.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon27.Enabled = false;
      this.Icon27.Location = new System.Drawing.Point(163, 180);
      this.Icon27.Name = "Icon27";
      this.Icon27.Size = new System.Drawing.Size(14, 18);
      this.Icon27.TabIndex = 569;
      this.Icon27.TabStop = false;
      this.Icon27.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon28
      // 
      this.Icon28.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon28.Enabled = false;
      this.Icon28.Location = new System.Drawing.Point(176, 180);
      this.Icon28.Name = "Icon28";
      this.Icon28.Size = new System.Drawing.Size(14, 18);
      this.Icon28.TabIndex = 570;
      this.Icon28.TabStop = false;
      this.Icon28.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon29
      // 
      this.Icon29.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon29.Enabled = false;
      this.Icon29.Location = new System.Drawing.Point(189, 180);
      this.Icon29.Name = "Icon29";
      this.Icon29.Size = new System.Drawing.Size(14, 18);
      this.Icon29.TabIndex = 571;
      this.Icon29.TabStop = false;
      this.Icon29.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon30
      // 
      this.Icon30.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon30.Enabled = false;
      this.Icon30.Location = new System.Drawing.Point(202, 180);
      this.Icon30.Name = "Icon30";
      this.Icon30.Size = new System.Drawing.Size(14, 18);
      this.Icon30.TabIndex = 572;
      this.Icon30.TabStop = false;
      this.Icon30.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon31
      // 
      this.Icon31.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon31.Enabled = false;
      this.Icon31.Location = new System.Drawing.Point(215, 180);
      this.Icon31.Name = "Icon31";
      this.Icon31.Size = new System.Drawing.Size(14, 18);
      this.Icon31.TabIndex = 573;
      this.Icon31.TabStop = false;
      this.Icon31.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon32
      // 
      this.Icon32.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon32.Enabled = false;
      this.Icon32.Location = new System.Drawing.Point(20, 197);
      this.Icon32.Name = "Icon32";
      this.Icon32.Size = new System.Drawing.Size(14, 18);
      this.Icon32.TabIndex = 1;
      this.Icon32.TabStop = false;
      this.Icon32.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon33
      // 
      this.Icon33.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon33.Enabled = false;
      this.Icon33.Location = new System.Drawing.Point(33, 197);
      this.Icon33.Name = "Icon33";
      this.Icon33.Size = new System.Drawing.Size(14, 18);
      this.Icon33.TabIndex = 2;
      this.Icon33.TabStop = false;
      this.Icon33.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon34
      // 
      this.Icon34.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon34.Enabled = false;
      this.Icon34.Location = new System.Drawing.Point(46, 197);
      this.Icon34.Name = "Icon34";
      this.Icon34.Size = new System.Drawing.Size(14, 18);
      this.Icon34.TabIndex = 3;
      this.Icon34.TabStop = false;
      this.Icon34.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon35
      // 
      this.Icon35.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon35.Enabled = false;
      this.Icon35.Location = new System.Drawing.Point(59, 197);
      this.Icon35.Name = "Icon35";
      this.Icon35.Size = new System.Drawing.Size(14, 18);
      this.Icon35.TabIndex = 4;
      this.Icon35.TabStop = false;
      this.Icon35.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon36
      // 
      this.Icon36.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon36.Enabled = false;
      this.Icon36.Location = new System.Drawing.Point(72, 197);
      this.Icon36.Name = "Icon36";
      this.Icon36.Size = new System.Drawing.Size(14, 18);
      this.Icon36.TabIndex = 5;
      this.Icon36.TabStop = false;
      this.Icon36.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon37
      // 
      this.Icon37.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon37.Enabled = false;
      this.Icon37.Location = new System.Drawing.Point(85, 197);
      this.Icon37.Name = "Icon37";
      this.Icon37.Size = new System.Drawing.Size(14, 18);
      this.Icon37.TabIndex = 6;
      this.Icon37.TabStop = false;
      this.Icon37.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon38
      // 
      this.Icon38.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon38.Enabled = false;
      this.Icon38.Location = new System.Drawing.Point(98, 197);
      this.Icon38.Name = "Icon38";
      this.Icon38.Size = new System.Drawing.Size(14, 18);
      this.Icon38.TabIndex = 7;
      this.Icon38.TabStop = false;
      this.Icon38.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon39
      // 
      this.Icon39.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon39.Enabled = false;
      this.Icon39.Location = new System.Drawing.Point(111, 197);
      this.Icon39.Name = "Icon39";
      this.Icon39.Size = new System.Drawing.Size(14, 18);
      this.Icon39.TabIndex = 8;
      this.Icon39.TabStop = false;
      this.Icon39.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon40
      // 
      this.Icon40.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon40.Enabled = false;
      this.Icon40.Location = new System.Drawing.Point(124, 197);
      this.Icon40.Name = "Icon40";
      this.Icon40.Size = new System.Drawing.Size(14, 18);
      this.Icon40.TabIndex = 9;
      this.Icon40.TabStop = false;
      this.Icon40.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon41
      // 
      this.Icon41.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon41.Enabled = false;
      this.Icon41.Location = new System.Drawing.Point(137, 197);
      this.Icon41.Name = "Icon41";
      this.Icon41.Size = new System.Drawing.Size(14, 18);
      this.Icon41.TabIndex = 10;
      this.Icon41.TabStop = false;
      this.Icon41.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon42
      // 
      this.Icon42.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon42.Enabled = false;
      this.Icon42.Location = new System.Drawing.Point(150, 197);
      this.Icon42.Name = "Icon42";
      this.Icon42.Size = new System.Drawing.Size(14, 18);
      this.Icon42.TabIndex = 552;
      this.Icon42.TabStop = false;
      this.Icon42.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon43
      // 
      this.Icon43.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon43.Enabled = false;
      this.Icon43.Location = new System.Drawing.Point(163, 197);
      this.Icon43.Name = "Icon43";
      this.Icon43.Size = new System.Drawing.Size(14, 18);
      this.Icon43.TabIndex = 553;
      this.Icon43.TabStop = false;
      this.Icon43.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon44
      // 
      this.Icon44.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon44.Enabled = false;
      this.Icon44.Location = new System.Drawing.Point(176, 197);
      this.Icon44.Name = "Icon44";
      this.Icon44.Size = new System.Drawing.Size(14, 18);
      this.Icon44.TabIndex = 554;
      this.Icon44.TabStop = false;
      this.Icon44.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon45
      // 
      this.Icon45.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon45.Enabled = false;
      this.Icon45.Location = new System.Drawing.Point(189, 197);
      this.Icon45.Name = "Icon45";
      this.Icon45.Size = new System.Drawing.Size(14, 18);
      this.Icon45.TabIndex = 555;
      this.Icon45.TabStop = false;
      this.Icon45.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon46
      // 
      this.Icon46.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon46.Enabled = false;
      this.Icon46.Location = new System.Drawing.Point(202, 197);
      this.Icon46.Name = "Icon46";
      this.Icon46.Size = new System.Drawing.Size(14, 18);
      this.Icon46.TabIndex = 556;
      this.Icon46.TabStop = false;
      this.Icon46.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon47
      // 
      this.Icon47.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon47.Enabled = false;
      this.Icon47.Location = new System.Drawing.Point(215, 197);
      this.Icon47.Name = "Icon47";
      this.Icon47.Size = new System.Drawing.Size(14, 18);
      this.Icon47.TabIndex = 557;
      this.Icon47.TabStop = false;
      this.Icon47.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon48
      // 
      this.Icon48.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon48.Enabled = false;
      this.Icon48.Location = new System.Drawing.Point(20, 214);
      this.Icon48.Name = "Icon48";
      this.Icon48.Size = new System.Drawing.Size(14, 18);
      this.Icon48.TabIndex = 558;
      this.Icon48.TabStop = false;
      this.Icon48.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon49
      // 
      this.Icon49.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon49.Enabled = false;
      this.Icon49.Location = new System.Drawing.Point(33, 214);
      this.Icon49.Name = "Icon49";
      this.Icon49.Size = new System.Drawing.Size(14, 18);
      this.Icon49.TabIndex = 559;
      this.Icon49.TabStop = false;
      this.Icon49.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon50
      // 
      this.Icon50.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon50.Enabled = false;
      this.Icon50.Location = new System.Drawing.Point(46, 214);
      this.Icon50.Name = "Icon50";
      this.Icon50.Size = new System.Drawing.Size(14, 18);
      this.Icon50.TabIndex = 560;
      this.Icon50.TabStop = false;
      this.Icon50.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon51
      // 
      this.Icon51.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon51.Enabled = false;
      this.Icon51.Location = new System.Drawing.Point(59, 214);
      this.Icon51.Name = "Icon51";
      this.Icon51.Size = new System.Drawing.Size(14, 18);
      this.Icon51.TabIndex = 561;
      this.Icon51.TabStop = false;
      this.Icon51.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon52
      // 
      this.Icon52.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon52.Enabled = false;
      this.Icon52.Location = new System.Drawing.Point(72, 214);
      this.Icon52.Name = "Icon52";
      this.Icon52.Size = new System.Drawing.Size(14, 18);
      this.Icon52.TabIndex = 562;
      this.Icon52.TabStop = false;
      this.Icon52.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon53
      // 
      this.Icon53.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon53.Enabled = false;
      this.Icon53.Location = new System.Drawing.Point(85, 214);
      this.Icon53.Name = "Icon53";
      this.Icon53.Size = new System.Drawing.Size(14, 18);
      this.Icon53.TabIndex = 563;
      this.Icon53.TabStop = false;
      this.Icon53.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon54
      // 
      this.Icon54.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon54.Enabled = false;
      this.Icon54.Location = new System.Drawing.Point(98, 214);
      this.Icon54.Name = "Icon54";
      this.Icon54.Size = new System.Drawing.Size(14, 18);
      this.Icon54.TabIndex = 564;
      this.Icon54.TabStop = false;
      this.Icon54.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon55
      // 
      this.Icon55.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon55.Enabled = false;
      this.Icon55.Location = new System.Drawing.Point(111, 214);
      this.Icon55.Name = "Icon55";
      this.Icon55.Size = new System.Drawing.Size(14, 18);
      this.Icon55.TabIndex = 565;
      this.Icon55.TabStop = false;
      this.Icon55.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon56
      // 
      this.Icon56.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon56.Enabled = false;
      this.Icon56.Location = new System.Drawing.Point(124, 214);
      this.Icon56.Name = "Icon56";
      this.Icon56.Size = new System.Drawing.Size(14, 18);
      this.Icon56.TabIndex = 566;
      this.Icon56.TabStop = false;
      this.Icon56.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon57
      // 
      this.Icon57.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon57.Enabled = false;
      this.Icon57.Location = new System.Drawing.Point(137, 214);
      this.Icon57.Name = "Icon57";
      this.Icon57.Size = new System.Drawing.Size(14, 18);
      this.Icon57.TabIndex = 567;
      this.Icon57.TabStop = false;
      this.Icon57.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon58
      // 
      this.Icon58.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon58.Enabled = false;
      this.Icon58.Location = new System.Drawing.Point(150, 214);
      this.Icon58.Name = "Icon58";
      this.Icon58.Size = new System.Drawing.Size(14, 18);
      this.Icon58.TabIndex = 568;
      this.Icon58.TabStop = false;
      this.Icon58.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon59
      // 
      this.Icon59.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon59.Enabled = false;
      this.Icon59.Location = new System.Drawing.Point(163, 214);
      this.Icon59.Name = "Icon59";
      this.Icon59.Size = new System.Drawing.Size(14, 18);
      this.Icon59.TabIndex = 569;
      this.Icon59.TabStop = false;
      this.Icon59.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon60
      // 
      this.Icon60.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon60.Enabled = false;
      this.Icon60.Location = new System.Drawing.Point(176, 214);
      this.Icon60.Name = "Icon60";
      this.Icon60.Size = new System.Drawing.Size(14, 18);
      this.Icon60.TabIndex = 570;
      this.Icon60.TabStop = false;
      this.Icon60.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon61
      // 
      this.Icon61.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon61.Enabled = false;
      this.Icon61.Location = new System.Drawing.Point(189, 214);
      this.Icon61.Name = "Icon61";
      this.Icon61.Size = new System.Drawing.Size(14, 18);
      this.Icon61.TabIndex = 571;
      this.Icon61.TabStop = false;
      this.Icon61.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon62
      // 
      this.Icon62.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon62.Enabled = false;
      this.Icon62.Location = new System.Drawing.Point(202, 214);
      this.Icon62.Name = "Icon62";
      this.Icon62.Size = new System.Drawing.Size(14, 18);
      this.Icon62.TabIndex = 572;
      this.Icon62.TabStop = false;
      this.Icon62.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon63
      // 
      this.Icon63.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon63.Enabled = false;
      this.Icon63.Location = new System.Drawing.Point(215, 214);
      this.Icon63.Name = "Icon63";
      this.Icon63.Size = new System.Drawing.Size(14, 18);
      this.Icon63.TabIndex = 573;
      this.Icon63.TabStop = false;
      this.Icon63.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon64
      // 
      this.Icon64.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon64.Enabled = false;
      this.Icon64.Location = new System.Drawing.Point(20, 231);
      this.Icon64.Name = "Icon64";
      this.Icon64.Size = new System.Drawing.Size(14, 18);
      this.Icon64.TabIndex = 1;
      this.Icon64.TabStop = false;
      this.Icon64.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon65
      // 
      this.Icon65.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon65.Enabled = false;
      this.Icon65.Location = new System.Drawing.Point(33, 231);
      this.Icon65.Name = "Icon65";
      this.Icon65.Size = new System.Drawing.Size(14, 18);
      this.Icon65.TabIndex = 2;
      this.Icon65.TabStop = false;
      this.Icon65.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon66
      // 
      this.Icon66.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon66.Enabled = false;
      this.Icon66.Location = new System.Drawing.Point(46, 231);
      this.Icon66.Name = "Icon66";
      this.Icon66.Size = new System.Drawing.Size(14, 18);
      this.Icon66.TabIndex = 3;
      this.Icon66.TabStop = false;
      this.Icon66.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon67
      // 
      this.Icon67.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon67.Enabled = false;
      this.Icon67.Location = new System.Drawing.Point(59, 231);
      this.Icon67.Name = "Icon67";
      this.Icon67.Size = new System.Drawing.Size(14, 18);
      this.Icon67.TabIndex = 4;
      this.Icon67.TabStop = false;
      this.Icon67.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon68
      // 
      this.Icon68.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon68.Enabled = false;
      this.Icon68.Location = new System.Drawing.Point(72, 231);
      this.Icon68.Name = "Icon68";
      this.Icon68.Size = new System.Drawing.Size(14, 18);
      this.Icon68.TabIndex = 5;
      this.Icon68.TabStop = false;
      this.Icon68.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon69
      // 
      this.Icon69.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon69.Enabled = false;
      this.Icon69.Location = new System.Drawing.Point(85, 231);
      this.Icon69.Name = "Icon69";
      this.Icon69.Size = new System.Drawing.Size(14, 18);
      this.Icon69.TabIndex = 6;
      this.Icon69.TabStop = false;
      this.Icon69.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon70
      // 
      this.Icon70.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon70.Enabled = false;
      this.Icon70.Location = new System.Drawing.Point(98, 231);
      this.Icon70.Name = "Icon70";
      this.Icon70.Size = new System.Drawing.Size(14, 18);
      this.Icon70.TabIndex = 7;
      this.Icon70.TabStop = false;
      this.Icon70.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon71
      // 
      this.Icon71.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon71.Enabled = false;
      this.Icon71.Location = new System.Drawing.Point(111, 231);
      this.Icon71.Name = "Icon71";
      this.Icon71.Size = new System.Drawing.Size(14, 18);
      this.Icon71.TabIndex = 8;
      this.Icon71.TabStop = false;
      this.Icon71.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon72
      // 
      this.Icon72.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon72.Enabled = false;
      this.Icon72.Location = new System.Drawing.Point(124, 231);
      this.Icon72.Name = "Icon72";
      this.Icon72.Size = new System.Drawing.Size(14, 18);
      this.Icon72.TabIndex = 9;
      this.Icon72.TabStop = false;
      this.Icon72.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon73
      // 
      this.Icon73.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon73.Enabled = false;
      this.Icon73.Location = new System.Drawing.Point(137, 231);
      this.Icon73.Name = "Icon73";
      this.Icon73.Size = new System.Drawing.Size(14, 18);
      this.Icon73.TabIndex = 10;
      this.Icon73.TabStop = false;
      this.Icon73.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon74
      // 
      this.Icon74.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon74.Enabled = false;
      this.Icon74.Location = new System.Drawing.Point(150, 231);
      this.Icon74.Name = "Icon74";
      this.Icon74.Size = new System.Drawing.Size(14, 18);
      this.Icon74.TabIndex = 552;
      this.Icon74.TabStop = false;
      this.Icon74.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon75
      // 
      this.Icon75.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon75.Enabled = false;
      this.Icon75.Location = new System.Drawing.Point(163, 231);
      this.Icon75.Name = "Icon75";
      this.Icon75.Size = new System.Drawing.Size(14, 18);
      this.Icon75.TabIndex = 553;
      this.Icon75.TabStop = false;
      this.Icon75.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon76
      // 
      this.Icon76.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon76.Enabled = false;
      this.Icon76.Location = new System.Drawing.Point(176, 231);
      this.Icon76.Name = "Icon76";
      this.Icon76.Size = new System.Drawing.Size(14, 18);
      this.Icon76.TabIndex = 554;
      this.Icon76.TabStop = false;
      this.Icon76.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon77
      // 
      this.Icon77.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon77.Enabled = false;
      this.Icon77.Location = new System.Drawing.Point(189, 231);
      this.Icon77.Name = "Icon77";
      this.Icon77.Size = new System.Drawing.Size(14, 18);
      this.Icon77.TabIndex = 555;
      this.Icon77.TabStop = false;
      this.Icon77.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon78
      // 
      this.Icon78.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon78.Enabled = false;
      this.Icon78.Location = new System.Drawing.Point(202, 231);
      this.Icon78.Name = "Icon78";
      this.Icon78.Size = new System.Drawing.Size(14, 18);
      this.Icon78.TabIndex = 556;
      this.Icon78.TabStop = false;
      this.Icon78.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon79
      // 
      this.Icon79.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon79.Enabled = false;
      this.Icon79.Location = new System.Drawing.Point(215, 231);
      this.Icon79.Name = "Icon79";
      this.Icon79.Size = new System.Drawing.Size(14, 18);
      this.Icon79.TabIndex = 557;
      this.Icon79.TabStop = false;
      this.Icon79.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon80
      // 
      this.Icon80.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon80.Enabled = false;
      this.Icon80.Location = new System.Drawing.Point(20, 248);
      this.Icon80.Name = "Icon80";
      this.Icon80.Size = new System.Drawing.Size(14, 18);
      this.Icon80.TabIndex = 558;
      this.Icon80.TabStop = false;
      this.Icon80.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon81
      // 
      this.Icon81.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon81.Enabled = false;
      this.Icon81.Location = new System.Drawing.Point(33, 248);
      this.Icon81.Name = "Icon81";
      this.Icon81.Size = new System.Drawing.Size(14, 18);
      this.Icon81.TabIndex = 559;
      this.Icon81.TabStop = false;
      this.Icon81.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon82
      // 
      this.Icon82.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon82.Enabled = false;
      this.Icon82.Location = new System.Drawing.Point(46, 248);
      this.Icon82.Name = "Icon82";
      this.Icon82.Size = new System.Drawing.Size(14, 18);
      this.Icon82.TabIndex = 560;
      this.Icon82.TabStop = false;
      this.Icon82.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon83
      // 
      this.Icon83.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon83.Enabled = false;
      this.Icon83.Location = new System.Drawing.Point(59, 248);
      this.Icon83.Name = "Icon83";
      this.Icon83.Size = new System.Drawing.Size(14, 18);
      this.Icon83.TabIndex = 561;
      this.Icon83.TabStop = false;
      this.Icon83.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon84
      // 
      this.Icon84.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon84.Enabled = false;
      this.Icon84.Location = new System.Drawing.Point(72, 248);
      this.Icon84.Name = "Icon84";
      this.Icon84.Size = new System.Drawing.Size(14, 18);
      this.Icon84.TabIndex = 562;
      this.Icon84.TabStop = false;
      this.Icon84.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon85
      // 
      this.Icon85.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon85.Enabled = false;
      this.Icon85.Location = new System.Drawing.Point(85, 248);
      this.Icon85.Name = "Icon85";
      this.Icon85.Size = new System.Drawing.Size(14, 18);
      this.Icon85.TabIndex = 563;
      this.Icon85.TabStop = false;
      this.Icon85.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon86
      // 
      this.Icon86.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon86.Enabled = false;
      this.Icon86.Location = new System.Drawing.Point(98, 248);
      this.Icon86.Name = "Icon86";
      this.Icon86.Size = new System.Drawing.Size(14, 18);
      this.Icon86.TabIndex = 564;
      this.Icon86.TabStop = false;
      this.Icon86.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon87
      // 
      this.Icon87.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon87.Enabled = false;
      this.Icon87.Location = new System.Drawing.Point(111, 248);
      this.Icon87.Name = "Icon87";
      this.Icon87.Size = new System.Drawing.Size(14, 18);
      this.Icon87.TabIndex = 565;
      this.Icon87.TabStop = false;
      this.Icon87.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon88
      // 
      this.Icon88.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon88.Enabled = false;
      this.Icon88.Location = new System.Drawing.Point(124, 248);
      this.Icon88.Name = "Icon88";
      this.Icon88.Size = new System.Drawing.Size(14, 18);
      this.Icon88.TabIndex = 566;
      this.Icon88.TabStop = false;
      this.Icon88.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon89
      // 
      this.Icon89.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon89.Enabled = false;
      this.Icon89.Location = new System.Drawing.Point(137, 248);
      this.Icon89.Name = "Icon89";
      this.Icon89.Size = new System.Drawing.Size(14, 18);
      this.Icon89.TabIndex = 567;
      this.Icon89.TabStop = false;
      this.Icon89.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon90
      // 
      this.Icon90.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon90.Enabled = false;
      this.Icon90.Location = new System.Drawing.Point(150, 248);
      this.Icon90.Name = "Icon90";
      this.Icon90.Size = new System.Drawing.Size(14, 18);
      this.Icon90.TabIndex = 568;
      this.Icon90.TabStop = false;
      this.Icon90.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon91
      // 
      this.Icon91.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon91.Enabled = false;
      this.Icon91.Location = new System.Drawing.Point(163, 248);
      this.Icon91.Name = "Icon91";
      this.Icon91.Size = new System.Drawing.Size(14, 18);
      this.Icon91.TabIndex = 569;
      this.Icon91.TabStop = false;
      this.Icon91.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon92
      // 
      this.Icon92.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon92.Enabled = false;
      this.Icon92.Location = new System.Drawing.Point(176, 248);
      this.Icon92.Name = "Icon92";
      this.Icon92.Size = new System.Drawing.Size(14, 18);
      this.Icon92.TabIndex = 570;
      this.Icon92.TabStop = false;
      this.Icon92.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon93
      // 
      this.Icon93.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon93.Enabled = false;
      this.Icon93.Location = new System.Drawing.Point(189, 248);
      this.Icon93.Name = "Icon93";
      this.Icon93.Size = new System.Drawing.Size(14, 18);
      this.Icon93.TabIndex = 571;
      this.Icon93.TabStop = false;
      this.Icon93.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon94
      // 
      this.Icon94.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon94.Enabled = false;
      this.Icon94.Location = new System.Drawing.Point(202, 248);
      this.Icon94.Name = "Icon94";
      this.Icon94.Size = new System.Drawing.Size(14, 18);
      this.Icon94.TabIndex = 572;
      this.Icon94.TabStop = false;
      this.Icon94.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon95
      // 
      this.Icon95.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon95.Enabled = false;
      this.Icon95.Location = new System.Drawing.Point(215, 248);
      this.Icon95.Name = "Icon95";
      this.Icon95.Size = new System.Drawing.Size(14, 18);
      this.Icon95.TabIndex = 573;
      this.Icon95.TabStop = false;
      this.Icon95.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon96
      // 
      this.Icon96.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon96.Enabled = false;
      this.Icon96.Location = new System.Drawing.Point(20, 265);
      this.Icon96.Name = "Icon96";
      this.Icon96.Size = new System.Drawing.Size(14, 18);
      this.Icon96.TabIndex = 1;
      this.Icon96.TabStop = false;
      this.Icon96.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon97
      // 
      this.Icon97.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon97.Enabled = false;
      this.Icon97.Location = new System.Drawing.Point(33, 265);
      this.Icon97.Name = "Icon97";
      this.Icon97.Size = new System.Drawing.Size(14, 18);
      this.Icon97.TabIndex = 2;
      this.Icon97.TabStop = false;
      this.Icon97.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon98
      // 
      this.Icon98.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon98.Enabled = false;
      this.Icon98.Location = new System.Drawing.Point(46, 265);
      this.Icon98.Name = "Icon98";
      this.Icon98.Size = new System.Drawing.Size(14, 18);
      this.Icon98.TabIndex = 3;
      this.Icon98.TabStop = false;
      this.Icon98.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon99
      // 
      this.Icon99.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon99.Enabled = false;
      this.Icon99.Location = new System.Drawing.Point(59, 265);
      this.Icon99.Name = "Icon99";
      this.Icon99.Size = new System.Drawing.Size(14, 18);
      this.Icon99.TabIndex = 4;
      this.Icon99.TabStop = false;
      this.Icon99.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon100
      // 
      this.Icon100.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon100.Enabled = false;
      this.Icon100.Location = new System.Drawing.Point(72, 265);
      this.Icon100.Name = "Icon100";
      this.Icon100.Size = new System.Drawing.Size(14, 18);
      this.Icon100.TabIndex = 5;
      this.Icon100.TabStop = false;
      this.Icon100.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon101
      // 
      this.Icon101.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon101.Enabled = false;
      this.Icon101.Location = new System.Drawing.Point(85, 265);
      this.Icon101.Name = "Icon101";
      this.Icon101.Size = new System.Drawing.Size(14, 18);
      this.Icon101.TabIndex = 6;
      this.Icon101.TabStop = false;
      this.Icon101.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon102
      // 
      this.Icon102.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon102.Enabled = false;
      this.Icon102.Location = new System.Drawing.Point(98, 265);
      this.Icon102.Name = "Icon102";
      this.Icon102.Size = new System.Drawing.Size(14, 18);
      this.Icon102.TabIndex = 7;
      this.Icon102.TabStop = false;
      this.Icon102.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon103
      // 
      this.Icon103.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon103.Enabled = false;
      this.Icon103.Location = new System.Drawing.Point(111, 265);
      this.Icon103.Name = "Icon103";
      this.Icon103.Size = new System.Drawing.Size(14, 18);
      this.Icon103.TabIndex = 8;
      this.Icon103.TabStop = false;
      this.Icon103.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon104
      // 
      this.Icon104.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon104.Enabled = false;
      this.Icon104.Location = new System.Drawing.Point(124, 265);
      this.Icon104.Name = "Icon104";
      this.Icon104.Size = new System.Drawing.Size(14, 18);
      this.Icon104.TabIndex = 9;
      this.Icon104.TabStop = false;
      this.Icon104.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon105
      // 
      this.Icon105.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon105.Enabled = false;
      this.Icon105.Location = new System.Drawing.Point(137, 265);
      this.Icon105.Name = "Icon105";
      this.Icon105.Size = new System.Drawing.Size(14, 18);
      this.Icon105.TabIndex = 10;
      this.Icon105.TabStop = false;
      this.Icon105.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon106
      // 
      this.Icon106.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon106.Enabled = false;
      this.Icon106.Location = new System.Drawing.Point(150, 265);
      this.Icon106.Name = "Icon106";
      this.Icon106.Size = new System.Drawing.Size(14, 18);
      this.Icon106.TabIndex = 552;
      this.Icon106.TabStop = false;
      this.Icon106.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon107
      // 
      this.Icon107.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon107.Enabled = false;
      this.Icon107.Location = new System.Drawing.Point(163, 265);
      this.Icon107.Name = "Icon107";
      this.Icon107.Size = new System.Drawing.Size(14, 18);
      this.Icon107.TabIndex = 553;
      this.Icon107.TabStop = false;
      this.Icon107.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon108
      // 
      this.Icon108.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon108.Enabled = false;
      this.Icon108.Location = new System.Drawing.Point(176, 265);
      this.Icon108.Name = "Icon108";
      this.Icon108.Size = new System.Drawing.Size(14, 18);
      this.Icon108.TabIndex = 554;
      this.Icon108.TabStop = false;
      this.Icon108.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon109
      // 
      this.Icon109.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon109.Enabled = false;
      this.Icon109.Location = new System.Drawing.Point(189, 265);
      this.Icon109.Name = "Icon109";
      this.Icon109.Size = new System.Drawing.Size(14, 18);
      this.Icon109.TabIndex = 555;
      this.Icon109.TabStop = false;
      this.Icon109.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon110
      // 
      this.Icon110.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon110.Enabled = false;
      this.Icon110.Location = new System.Drawing.Point(202, 265);
      this.Icon110.Name = "Icon110";
      this.Icon110.Size = new System.Drawing.Size(14, 18);
      this.Icon110.TabIndex = 556;
      this.Icon110.TabStop = false;
      this.Icon110.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon111
      // 
      this.Icon111.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon111.Enabled = false;
      this.Icon111.Location = new System.Drawing.Point(215, 265);
      this.Icon111.Name = "Icon111";
      this.Icon111.Size = new System.Drawing.Size(14, 18);
      this.Icon111.TabIndex = 557;
      this.Icon111.TabStop = false;
      this.Icon111.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon112
      // 
      this.Icon112.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon112.Enabled = false;
      this.Icon112.Location = new System.Drawing.Point(20, 282);
      this.Icon112.Name = "Icon112";
      this.Icon112.Size = new System.Drawing.Size(14, 18);
      this.Icon112.TabIndex = 558;
      this.Icon112.TabStop = false;
      this.Icon112.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon113
      // 
      this.Icon113.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon113.Enabled = false;
      this.Icon113.Location = new System.Drawing.Point(33, 282);
      this.Icon113.Name = "Icon113";
      this.Icon113.Size = new System.Drawing.Size(14, 18);
      this.Icon113.TabIndex = 559;
      this.Icon113.TabStop = false;
      this.Icon113.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon114
      // 
      this.Icon114.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon114.Enabled = false;
      this.Icon114.Location = new System.Drawing.Point(46, 282);
      this.Icon114.Name = "Icon114";
      this.Icon114.Size = new System.Drawing.Size(14, 18);
      this.Icon114.TabIndex = 560;
      this.Icon114.TabStop = false;
      this.Icon114.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon115
      // 
      this.Icon115.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon115.Enabled = false;
      this.Icon115.Location = new System.Drawing.Point(59, 282);
      this.Icon115.Name = "Icon115";
      this.Icon115.Size = new System.Drawing.Size(14, 18);
      this.Icon115.TabIndex = 561;
      this.Icon115.TabStop = false;
      this.Icon115.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon116
      // 
      this.Icon116.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon116.Enabled = false;
      this.Icon116.Location = new System.Drawing.Point(72, 282);
      this.Icon116.Name = "Icon116";
      this.Icon116.Size = new System.Drawing.Size(14, 18);
      this.Icon116.TabIndex = 562;
      this.Icon116.TabStop = false;
      this.Icon116.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon117
      // 
      this.Icon117.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon117.Enabled = false;
      this.Icon117.Location = new System.Drawing.Point(85, 282);
      this.Icon117.Name = "Icon117";
      this.Icon117.Size = new System.Drawing.Size(14, 18);
      this.Icon117.TabIndex = 563;
      this.Icon117.TabStop = false;
      this.Icon117.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon118
      // 
      this.Icon118.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon118.Enabled = false;
      this.Icon118.Location = new System.Drawing.Point(98, 282);
      this.Icon118.Name = "Icon118";
      this.Icon118.Size = new System.Drawing.Size(14, 18);
      this.Icon118.TabIndex = 564;
      this.Icon118.TabStop = false;
      this.Icon118.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon119
      // 
      this.Icon119.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon119.Enabled = false;
      this.Icon119.Location = new System.Drawing.Point(111, 282);
      this.Icon119.Name = "Icon119";
      this.Icon119.Size = new System.Drawing.Size(14, 18);
      this.Icon119.TabIndex = 565;
      this.Icon119.TabStop = false;
      this.Icon119.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon120
      // 
      this.Icon120.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon120.Enabled = false;
      this.Icon120.Location = new System.Drawing.Point(124, 282);
      this.Icon120.Name = "Icon120";
      this.Icon120.Size = new System.Drawing.Size(14, 18);
      this.Icon120.TabIndex = 566;
      this.Icon120.TabStop = false;
      this.Icon120.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon121
      // 
      this.Icon121.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon121.Enabled = false;
      this.Icon121.Location = new System.Drawing.Point(137, 282);
      this.Icon121.Name = "Icon121";
      this.Icon121.Size = new System.Drawing.Size(14, 18);
      this.Icon121.TabIndex = 567;
      this.Icon121.TabStop = false;
      this.Icon121.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon122
      // 
      this.Icon122.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon122.Enabled = false;
      this.Icon122.Location = new System.Drawing.Point(150, 282);
      this.Icon122.Name = "Icon122";
      this.Icon122.Size = new System.Drawing.Size(14, 18);
      this.Icon122.TabIndex = 568;
      this.Icon122.TabStop = false;
      this.Icon122.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon123
      // 
      this.Icon123.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon123.Enabled = false;
      this.Icon123.Location = new System.Drawing.Point(163, 282);
      this.Icon123.Name = "Icon123";
      this.Icon123.Size = new System.Drawing.Size(14, 18);
      this.Icon123.TabIndex = 569;
      this.Icon123.TabStop = false;
      this.Icon123.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon124
      // 
      this.Icon124.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon124.Enabled = false;
      this.Icon124.Location = new System.Drawing.Point(176, 282);
      this.Icon124.Name = "Icon124";
      this.Icon124.Size = new System.Drawing.Size(14, 18);
      this.Icon124.TabIndex = 570;
      this.Icon124.TabStop = false;
      this.Icon124.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon125
      // 
      this.Icon125.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon125.Enabled = false;
      this.Icon125.Location = new System.Drawing.Point(189, 282);
      this.Icon125.Name = "Icon125";
      this.Icon125.Size = new System.Drawing.Size(14, 18);
      this.Icon125.TabIndex = 571;
      this.Icon125.TabStop = false;
      this.Icon125.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon126
      // 
      this.Icon126.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon126.Enabled = false;
      this.Icon126.Location = new System.Drawing.Point(202, 282);
      this.Icon126.Name = "Icon126";
      this.Icon126.Size = new System.Drawing.Size(14, 18);
      this.Icon126.TabIndex = 572;
      this.Icon126.TabStop = false;
      this.Icon126.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon127
      // 
      this.Icon127.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon127.Enabled = false;
      this.Icon127.Location = new System.Drawing.Point(215, 282);
      this.Icon127.Name = "Icon127";
      this.Icon127.Size = new System.Drawing.Size(14, 18);
      this.Icon127.TabIndex = 573;
      this.Icon127.TabStop = false;
      this.Icon127.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon128
      // 
      this.Icon128.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon128.Enabled = false;
      this.Icon128.Location = new System.Drawing.Point(20, 299);
      this.Icon128.Name = "Icon128";
      this.Icon128.Size = new System.Drawing.Size(14, 18);
      this.Icon128.TabIndex = 1;
      this.Icon128.TabStop = false;
      this.Icon128.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon129
      // 
      this.Icon129.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon129.Enabled = false;
      this.Icon129.Location = new System.Drawing.Point(33, 299);
      this.Icon129.Name = "Icon129";
      this.Icon129.Size = new System.Drawing.Size(14, 18);
      this.Icon129.TabIndex = 2;
      this.Icon129.TabStop = false;
      this.Icon129.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon130
      // 
      this.Icon130.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon130.Enabled = false;
      this.Icon130.Location = new System.Drawing.Point(46, 299);
      this.Icon130.Name = "Icon130";
      this.Icon130.Size = new System.Drawing.Size(14, 18);
      this.Icon130.TabIndex = 3;
      this.Icon130.TabStop = false;
      this.Icon130.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon131
      // 
      this.Icon131.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon131.Enabled = false;
      this.Icon131.Location = new System.Drawing.Point(59, 299);
      this.Icon131.Name = "Icon131";
      this.Icon131.Size = new System.Drawing.Size(14, 18);
      this.Icon131.TabIndex = 4;
      this.Icon131.TabStop = false;
      this.Icon131.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon132
      // 
      this.Icon132.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon132.Enabled = false;
      this.Icon132.Location = new System.Drawing.Point(72, 299);
      this.Icon132.Name = "Icon132";
      this.Icon132.Size = new System.Drawing.Size(14, 18);
      this.Icon132.TabIndex = 5;
      this.Icon132.TabStop = false;
      this.Icon132.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon133
      // 
      this.Icon133.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon133.Enabled = false;
      this.Icon133.Location = new System.Drawing.Point(85, 299);
      this.Icon133.Name = "Icon133";
      this.Icon133.Size = new System.Drawing.Size(14, 18);
      this.Icon133.TabIndex = 6;
      this.Icon133.TabStop = false;
      this.Icon133.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon134
      // 
      this.Icon134.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon134.Enabled = false;
      this.Icon134.Location = new System.Drawing.Point(98, 299);
      this.Icon134.Name = "Icon134";
      this.Icon134.Size = new System.Drawing.Size(14, 18);
      this.Icon134.TabIndex = 7;
      this.Icon134.TabStop = false;
      this.Icon134.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon135
      // 
      this.Icon135.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon135.Enabled = false;
      this.Icon135.Location = new System.Drawing.Point(111, 299);
      this.Icon135.Name = "Icon135";
      this.Icon135.Size = new System.Drawing.Size(14, 18);
      this.Icon135.TabIndex = 8;
      this.Icon135.TabStop = false;
      this.Icon135.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon136
      // 
      this.Icon136.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon136.Enabled = false;
      this.Icon136.Location = new System.Drawing.Point(124, 299);
      this.Icon136.Name = "Icon136";
      this.Icon136.Size = new System.Drawing.Size(14, 18);
      this.Icon136.TabIndex = 9;
      this.Icon136.TabStop = false;
      this.Icon136.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon137
      // 
      this.Icon137.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon137.Enabled = false;
      this.Icon137.Location = new System.Drawing.Point(137, 299);
      this.Icon137.Name = "Icon137";
      this.Icon137.Size = new System.Drawing.Size(14, 18);
      this.Icon137.TabIndex = 10;
      this.Icon137.TabStop = false;
      this.Icon137.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon138
      // 
      this.Icon138.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon138.Enabled = false;
      this.Icon138.Location = new System.Drawing.Point(150, 299);
      this.Icon138.Name = "Icon138";
      this.Icon138.Size = new System.Drawing.Size(14, 18);
      this.Icon138.TabIndex = 552;
      this.Icon138.TabStop = false;
      this.Icon138.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon139
      // 
      this.Icon139.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon139.Enabled = false;
      this.Icon139.Location = new System.Drawing.Point(163, 299);
      this.Icon139.Name = "Icon139";
      this.Icon139.Size = new System.Drawing.Size(14, 18);
      this.Icon139.TabIndex = 553;
      this.Icon139.TabStop = false;
      this.Icon139.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon140
      // 
      this.Icon140.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon140.Enabled = false;
      this.Icon140.Location = new System.Drawing.Point(176, 299);
      this.Icon140.Name = "Icon140";
      this.Icon140.Size = new System.Drawing.Size(14, 18);
      this.Icon140.TabIndex = 554;
      this.Icon140.TabStop = false;
      this.Icon140.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon141
      // 
      this.Icon141.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon141.Enabled = false;
      this.Icon141.Location = new System.Drawing.Point(189, 299);
      this.Icon141.Name = "Icon141";
      this.Icon141.Size = new System.Drawing.Size(14, 18);
      this.Icon141.TabIndex = 555;
      this.Icon141.TabStop = false;
      this.Icon141.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon142
      // 
      this.Icon142.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon142.Enabled = false;
      this.Icon142.Location = new System.Drawing.Point(202, 299);
      this.Icon142.Name = "Icon142";
      this.Icon142.Size = new System.Drawing.Size(14, 18);
      this.Icon142.TabIndex = 556;
      this.Icon142.TabStop = false;
      this.Icon142.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon143
      // 
      this.Icon143.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon143.Enabled = false;
      this.Icon143.Location = new System.Drawing.Point(215, 299);
      this.Icon143.Name = "Icon143";
      this.Icon143.Size = new System.Drawing.Size(14, 18);
      this.Icon143.TabIndex = 557;
      this.Icon143.TabStop = false;
      this.Icon143.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon144
      // 
      this.Icon144.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon144.Enabled = false;
      this.Icon144.Location = new System.Drawing.Point(20, 316);
      this.Icon144.Name = "Icon144";
      this.Icon144.Size = new System.Drawing.Size(14, 18);
      this.Icon144.TabIndex = 558;
      this.Icon144.TabStop = false;
      this.Icon144.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon145
      // 
      this.Icon145.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon145.Enabled = false;
      this.Icon145.Location = new System.Drawing.Point(33, 316);
      this.Icon145.Name = "Icon145";
      this.Icon145.Size = new System.Drawing.Size(14, 18);
      this.Icon145.TabIndex = 559;
      this.Icon145.TabStop = false;
      this.Icon145.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon146
      // 
      this.Icon146.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon146.Enabled = false;
      this.Icon146.Location = new System.Drawing.Point(46, 316);
      this.Icon146.Name = "Icon146";
      this.Icon146.Size = new System.Drawing.Size(14, 18);
      this.Icon146.TabIndex = 560;
      this.Icon146.TabStop = false;
      this.Icon146.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon147
      // 
      this.Icon147.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon147.Enabled = false;
      this.Icon147.Location = new System.Drawing.Point(59, 316);
      this.Icon147.Name = "Icon147";
      this.Icon147.Size = new System.Drawing.Size(14, 18);
      this.Icon147.TabIndex = 561;
      this.Icon147.TabStop = false;
      this.Icon147.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon148
      // 
      this.Icon148.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon148.Enabled = false;
      this.Icon148.Location = new System.Drawing.Point(72, 316);
      this.Icon148.Name = "Icon148";
      this.Icon148.Size = new System.Drawing.Size(14, 18);
      this.Icon148.TabIndex = 562;
      this.Icon148.TabStop = false;
      this.Icon148.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon149
      // 
      this.Icon149.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon149.Enabled = false;
      this.Icon149.Location = new System.Drawing.Point(85, 316);
      this.Icon149.Name = "Icon149";
      this.Icon149.Size = new System.Drawing.Size(14, 18);
      this.Icon149.TabIndex = 563;
      this.Icon149.TabStop = false;
      this.Icon149.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon150
      // 
      this.Icon150.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon150.Enabled = false;
      this.Icon150.Location = new System.Drawing.Point(98, 316);
      this.Icon150.Name = "Icon150";
      this.Icon150.Size = new System.Drawing.Size(14, 18);
      this.Icon150.TabIndex = 564;
      this.Icon150.TabStop = false;
      this.Icon150.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon151
      // 
      this.Icon151.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon151.Enabled = false;
      this.Icon151.Location = new System.Drawing.Point(111, 316);
      this.Icon151.Name = "Icon151";
      this.Icon151.Size = new System.Drawing.Size(14, 18);
      this.Icon151.TabIndex = 565;
      this.Icon151.TabStop = false;
      this.Icon151.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon152
      // 
      this.Icon152.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon152.Enabled = false;
      this.Icon152.Location = new System.Drawing.Point(124, 316);
      this.Icon152.Name = "Icon152";
      this.Icon152.Size = new System.Drawing.Size(14, 18);
      this.Icon152.TabIndex = 566;
      this.Icon152.TabStop = false;
      this.Icon152.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon153
      // 
      this.Icon153.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon153.Enabled = false;
      this.Icon153.Location = new System.Drawing.Point(137, 316);
      this.Icon153.Name = "Icon153";
      this.Icon153.Size = new System.Drawing.Size(14, 18);
      this.Icon153.TabIndex = 567;
      this.Icon153.TabStop = false;
      this.Icon153.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon154
      // 
      this.Icon154.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon154.Enabled = false;
      this.Icon154.Location = new System.Drawing.Point(150, 316);
      this.Icon154.Name = "Icon154";
      this.Icon154.Size = new System.Drawing.Size(14, 18);
      this.Icon154.TabIndex = 568;
      this.Icon154.TabStop = false;
      this.Icon154.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon155
      // 
      this.Icon155.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon155.Enabled = false;
      this.Icon155.Location = new System.Drawing.Point(163, 316);
      this.Icon155.Name = "Icon155";
      this.Icon155.Size = new System.Drawing.Size(14, 18);
      this.Icon155.TabIndex = 569;
      this.Icon155.TabStop = false;
      this.Icon155.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon156
      // 
      this.Icon156.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon156.Enabled = false;
      this.Icon156.Location = new System.Drawing.Point(176, 316);
      this.Icon156.Name = "Icon156";
      this.Icon156.Size = new System.Drawing.Size(14, 18);
      this.Icon156.TabIndex = 570;
      this.Icon156.TabStop = false;
      this.Icon156.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon157
      // 
      this.Icon157.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon157.Enabled = false;
      this.Icon157.Location = new System.Drawing.Point(189, 316);
      this.Icon157.Name = "Icon157";
      this.Icon157.Size = new System.Drawing.Size(14, 18);
      this.Icon157.TabIndex = 571;
      this.Icon157.TabStop = false;
      this.Icon157.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon158
      // 
      this.Icon158.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon158.Enabled = false;
      this.Icon158.Location = new System.Drawing.Point(202, 316);
      this.Icon158.Name = "Icon158";
      this.Icon158.Size = new System.Drawing.Size(14, 18);
      this.Icon158.TabIndex = 572;
      this.Icon158.TabStop = false;
      this.Icon158.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon159
      // 
      this.Icon159.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon159.Enabled = false;
      this.Icon159.Location = new System.Drawing.Point(215, 316);
      this.Icon159.Name = "Icon159";
      this.Icon159.Size = new System.Drawing.Size(14, 18);
      this.Icon159.TabIndex = 573;
      this.Icon159.TabStop = false;
      this.Icon159.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon160
      // 
      this.Icon160.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon160.Enabled = false;
      this.Icon160.Location = new System.Drawing.Point(20, 333);
      this.Icon160.Name = "Icon160";
      this.Icon160.Size = new System.Drawing.Size(14, 18);
      this.Icon160.TabIndex = 1;
      this.Icon160.TabStop = false;
      this.Icon160.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon161
      // 
      this.Icon161.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon161.Enabled = false;
      this.Icon161.Location = new System.Drawing.Point(33, 333);
      this.Icon161.Name = "Icon161";
      this.Icon161.Size = new System.Drawing.Size(14, 18);
      this.Icon161.TabIndex = 2;
      this.Icon161.TabStop = false;
      this.Icon161.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon162
      // 
      this.Icon162.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon162.Enabled = false;
      this.Icon162.Location = new System.Drawing.Point(46, 333);
      this.Icon162.Name = "Icon162";
      this.Icon162.Size = new System.Drawing.Size(14, 18);
      this.Icon162.TabIndex = 3;
      this.Icon162.TabStop = false;
      this.Icon162.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon163
      // 
      this.Icon163.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon163.Enabled = false;
      this.Icon163.Location = new System.Drawing.Point(59, 333);
      this.Icon163.Name = "Icon163";
      this.Icon163.Size = new System.Drawing.Size(14, 18);
      this.Icon163.TabIndex = 4;
      this.Icon163.TabStop = false;
      this.Icon163.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon164
      // 
      this.Icon164.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon164.Enabled = false;
      this.Icon164.Location = new System.Drawing.Point(72, 333);
      this.Icon164.Name = "Icon164";
      this.Icon164.Size = new System.Drawing.Size(14, 18);
      this.Icon164.TabIndex = 5;
      this.Icon164.TabStop = false;
      this.Icon164.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon165
      // 
      this.Icon165.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon165.Enabled = false;
      this.Icon165.Location = new System.Drawing.Point(85, 333);
      this.Icon165.Name = "Icon165";
      this.Icon165.Size = new System.Drawing.Size(14, 18);
      this.Icon165.TabIndex = 6;
      this.Icon165.TabStop = false;
      this.Icon165.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon166
      // 
      this.Icon166.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon166.Enabled = false;
      this.Icon166.Location = new System.Drawing.Point(98, 333);
      this.Icon166.Name = "Icon166";
      this.Icon166.Size = new System.Drawing.Size(14, 18);
      this.Icon166.TabIndex = 7;
      this.Icon166.TabStop = false;
      this.Icon166.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon167
      // 
      this.Icon167.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon167.Enabled = false;
      this.Icon167.Location = new System.Drawing.Point(111, 333);
      this.Icon167.Name = "Icon167";
      this.Icon167.Size = new System.Drawing.Size(14, 18);
      this.Icon167.TabIndex = 8;
      this.Icon167.TabStop = false;
      this.Icon167.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon168
      // 
      this.Icon168.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon168.Enabled = false;
      this.Icon168.Location = new System.Drawing.Point(124, 333);
      this.Icon168.Name = "Icon168";
      this.Icon168.Size = new System.Drawing.Size(14, 18);
      this.Icon168.TabIndex = 9;
      this.Icon168.TabStop = false;
      this.Icon168.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon169
      // 
      this.Icon169.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon169.Enabled = false;
      this.Icon169.Location = new System.Drawing.Point(137, 333);
      this.Icon169.Name = "Icon169";
      this.Icon169.Size = new System.Drawing.Size(14, 18);
      this.Icon169.TabIndex = 10;
      this.Icon169.TabStop = false;
      this.Icon169.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon170
      // 
      this.Icon170.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon170.Enabled = false;
      this.Icon170.Location = new System.Drawing.Point(150, 333);
      this.Icon170.Name = "Icon170";
      this.Icon170.Size = new System.Drawing.Size(14, 18);
      this.Icon170.TabIndex = 552;
      this.Icon170.TabStop = false;
      this.Icon170.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon171
      // 
      this.Icon171.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon171.Enabled = false;
      this.Icon171.Location = new System.Drawing.Point(163, 333);
      this.Icon171.Name = "Icon171";
      this.Icon171.Size = new System.Drawing.Size(14, 18);
      this.Icon171.TabIndex = 553;
      this.Icon171.TabStop = false;
      this.Icon171.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon172
      // 
      this.Icon172.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon172.Enabled = false;
      this.Icon172.Location = new System.Drawing.Point(176, 333);
      this.Icon172.Name = "Icon172";
      this.Icon172.Size = new System.Drawing.Size(14, 18);
      this.Icon172.TabIndex = 554;
      this.Icon172.TabStop = false;
      this.Icon172.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon173
      // 
      this.Icon173.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon173.Enabled = false;
      this.Icon173.Location = new System.Drawing.Point(189, 333);
      this.Icon173.Name = "Icon173";
      this.Icon173.Size = new System.Drawing.Size(14, 18);
      this.Icon173.TabIndex = 555;
      this.Icon173.TabStop = false;
      this.Icon173.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon174
      // 
      this.Icon174.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon174.Enabled = false;
      this.Icon174.Location = new System.Drawing.Point(202, 333);
      this.Icon174.Name = "Icon174";
      this.Icon174.Size = new System.Drawing.Size(14, 18);
      this.Icon174.TabIndex = 556;
      this.Icon174.TabStop = false;
      this.Icon174.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon175
      // 
      this.Icon175.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon175.Enabled = false;
      this.Icon175.Location = new System.Drawing.Point(215, 333);
      this.Icon175.Name = "Icon175";
      this.Icon175.Size = new System.Drawing.Size(14, 18);
      this.Icon175.TabIndex = 557;
      this.Icon175.TabStop = false;
      this.Icon175.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon176
      // 
      this.Icon176.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon176.Enabled = false;
      this.Icon176.Location = new System.Drawing.Point(20, 350);
      this.Icon176.Name = "Icon176";
      this.Icon176.Size = new System.Drawing.Size(14, 18);
      this.Icon176.TabIndex = 558;
      this.Icon176.TabStop = false;
      this.Icon176.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon177
      // 
      this.Icon177.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon177.Enabled = false;
      this.Icon177.Location = new System.Drawing.Point(33, 350);
      this.Icon177.Name = "Icon177";
      this.Icon177.Size = new System.Drawing.Size(14, 18);
      this.Icon177.TabIndex = 559;
      this.Icon177.TabStop = false;
      this.Icon177.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon178
      // 
      this.Icon178.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon178.Enabled = false;
      this.Icon178.Location = new System.Drawing.Point(46, 350);
      this.Icon178.Name = "Icon178";
      this.Icon178.Size = new System.Drawing.Size(14, 18);
      this.Icon178.TabIndex = 560;
      this.Icon178.TabStop = false;
      this.Icon178.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon179
      // 
      this.Icon179.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon179.Enabled = false;
      this.Icon179.Location = new System.Drawing.Point(59, 350);
      this.Icon179.Name = "Icon179";
      this.Icon179.Size = new System.Drawing.Size(14, 18);
      this.Icon179.TabIndex = 561;
      this.Icon179.TabStop = false;
      this.Icon179.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon180
      // 
      this.Icon180.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon180.Enabled = false;
      this.Icon180.Location = new System.Drawing.Point(72, 350);
      this.Icon180.Name = "Icon180";
      this.Icon180.Size = new System.Drawing.Size(14, 18);
      this.Icon180.TabIndex = 562;
      this.Icon180.TabStop = false;
      this.Icon180.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon181
      // 
      this.Icon181.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon181.Enabled = false;
      this.Icon181.Location = new System.Drawing.Point(85, 350);
      this.Icon181.Name = "Icon181";
      this.Icon181.Size = new System.Drawing.Size(14, 18);
      this.Icon181.TabIndex = 563;
      this.Icon181.TabStop = false;
      this.Icon181.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon182
      // 
      this.Icon182.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon182.Enabled = false;
      this.Icon182.Location = new System.Drawing.Point(98, 350);
      this.Icon182.Name = "Icon182";
      this.Icon182.Size = new System.Drawing.Size(14, 18);
      this.Icon182.TabIndex = 564;
      this.Icon182.TabStop = false;
      this.Icon182.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon183
      // 
      this.Icon183.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon183.Enabled = false;
      this.Icon183.Location = new System.Drawing.Point(111, 350);
      this.Icon183.Name = "Icon183";
      this.Icon183.Size = new System.Drawing.Size(14, 18);
      this.Icon183.TabIndex = 565;
      this.Icon183.TabStop = false;
      this.Icon183.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon184
      // 
      this.Icon184.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon184.Enabled = false;
      this.Icon184.Location = new System.Drawing.Point(124, 350);
      this.Icon184.Name = "Icon184";
      this.Icon184.Size = new System.Drawing.Size(14, 18);
      this.Icon184.TabIndex = 566;
      this.Icon184.TabStop = false;
      this.Icon184.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon185
      // 
      this.Icon185.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon185.Enabled = false;
      this.Icon185.Location = new System.Drawing.Point(137, 350);
      this.Icon185.Name = "Icon185";
      this.Icon185.Size = new System.Drawing.Size(14, 18);
      this.Icon185.TabIndex = 567;
      this.Icon185.TabStop = false;
      this.Icon185.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon186
      // 
      this.Icon186.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon186.Enabled = false;
      this.Icon186.Location = new System.Drawing.Point(150, 350);
      this.Icon186.Name = "Icon186";
      this.Icon186.Size = new System.Drawing.Size(14, 18);
      this.Icon186.TabIndex = 568;
      this.Icon186.TabStop = false;
      this.Icon186.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon187
      // 
      this.Icon187.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon187.Enabled = false;
      this.Icon187.Location = new System.Drawing.Point(163, 350);
      this.Icon187.Name = "Icon187";
      this.Icon187.Size = new System.Drawing.Size(14, 18);
      this.Icon187.TabIndex = 569;
      this.Icon187.TabStop = false;
      this.Icon187.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon188
      // 
      this.Icon188.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon188.Enabled = false;
      this.Icon188.Location = new System.Drawing.Point(176, 350);
      this.Icon188.Name = "Icon188";
      this.Icon188.Size = new System.Drawing.Size(14, 18);
      this.Icon188.TabIndex = 570;
      this.Icon188.TabStop = false;
      this.Icon188.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon189
      // 
      this.Icon189.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon189.Enabled = false;
      this.Icon189.Location = new System.Drawing.Point(189, 350);
      this.Icon189.Name = "Icon189";
      this.Icon189.Size = new System.Drawing.Size(14, 18);
      this.Icon189.TabIndex = 571;
      this.Icon189.TabStop = false;
      this.Icon189.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon190
      // 
      this.Icon190.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon190.Enabled = false;
      this.Icon190.Location = new System.Drawing.Point(202, 350);
      this.Icon190.Name = "Icon190";
      this.Icon190.Size = new System.Drawing.Size(14, 18);
      this.Icon190.TabIndex = 572;
      this.Icon190.TabStop = false;
      this.Icon190.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon191
      // 
      this.Icon191.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon191.Enabled = false;
      this.Icon191.Location = new System.Drawing.Point(215, 350);
      this.Icon191.Name = "Icon191";
      this.Icon191.Size = new System.Drawing.Size(14, 18);
      this.Icon191.TabIndex = 573;
      this.Icon191.TabStop = false;
      this.Icon191.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon192
      // 
      this.Icon192.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon192.Enabled = false;
      this.Icon192.Location = new System.Drawing.Point(20, 367);
      this.Icon192.Name = "Icon192";
      this.Icon192.Size = new System.Drawing.Size(14, 18);
      this.Icon192.TabIndex = 1;
      this.Icon192.TabStop = false;
      this.Icon192.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon193
      // 
      this.Icon193.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon193.Enabled = false;
      this.Icon193.Location = new System.Drawing.Point(33, 367);
      this.Icon193.Name = "Icon193";
      this.Icon193.Size = new System.Drawing.Size(14, 18);
      this.Icon193.TabIndex = 2;
      this.Icon193.TabStop = false;
      this.Icon193.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon194
      // 
      this.Icon194.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon194.Enabled = false;
      this.Icon194.Location = new System.Drawing.Point(46, 367);
      this.Icon194.Name = "Icon194";
      this.Icon194.Size = new System.Drawing.Size(14, 18);
      this.Icon194.TabIndex = 3;
      this.Icon194.TabStop = false;
      this.Icon194.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon195
      // 
      this.Icon195.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon195.Enabled = false;
      this.Icon195.Location = new System.Drawing.Point(59, 367);
      this.Icon195.Name = "Icon195";
      this.Icon195.Size = new System.Drawing.Size(14, 18);
      this.Icon195.TabIndex = 4;
      this.Icon195.TabStop = false;
      this.Icon195.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon196
      // 
      this.Icon196.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon196.Enabled = false;
      this.Icon196.Location = new System.Drawing.Point(72, 367);
      this.Icon196.Name = "Icon196";
      this.Icon196.Size = new System.Drawing.Size(14, 18);
      this.Icon196.TabIndex = 5;
      this.Icon196.TabStop = false;
      this.Icon196.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon197
      // 
      this.Icon197.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon197.Enabled = false;
      this.Icon197.Location = new System.Drawing.Point(85, 367);
      this.Icon197.Name = "Icon197";
      this.Icon197.Size = new System.Drawing.Size(14, 18);
      this.Icon197.TabIndex = 6;
      this.Icon197.TabStop = false;
      this.Icon197.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon198
      // 
      this.Icon198.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon198.Enabled = false;
      this.Icon198.Location = new System.Drawing.Point(98, 367);
      this.Icon198.Name = "Icon198";
      this.Icon198.Size = new System.Drawing.Size(14, 18);
      this.Icon198.TabIndex = 7;
      this.Icon198.TabStop = false;
      this.Icon198.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon199
      // 
      this.Icon199.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon199.Enabled = false;
      this.Icon199.Location = new System.Drawing.Point(111, 367);
      this.Icon199.Name = "Icon199";
      this.Icon199.Size = new System.Drawing.Size(14, 18);
      this.Icon199.TabIndex = 8;
      this.Icon199.TabStop = false;
      this.Icon199.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon200
      // 
      this.Icon200.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon200.Enabled = false;
      this.Icon200.Location = new System.Drawing.Point(124, 367);
      this.Icon200.Name = "Icon200";
      this.Icon200.Size = new System.Drawing.Size(14, 18);
      this.Icon200.TabIndex = 9;
      this.Icon200.TabStop = false;
      this.Icon200.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon201
      // 
      this.Icon201.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon201.Enabled = false;
      this.Icon201.Location = new System.Drawing.Point(137, 367);
      this.Icon201.Name = "Icon201";
      this.Icon201.Size = new System.Drawing.Size(14, 18);
      this.Icon201.TabIndex = 10;
      this.Icon201.TabStop = false;
      this.Icon201.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon202
      // 
      this.Icon202.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon202.Enabled = false;
      this.Icon202.Location = new System.Drawing.Point(150, 367);
      this.Icon202.Name = "Icon202";
      this.Icon202.Size = new System.Drawing.Size(14, 18);
      this.Icon202.TabIndex = 552;
      this.Icon202.TabStop = false;
      this.Icon202.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon203
      // 
      this.Icon203.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon203.Enabled = false;
      this.Icon203.Location = new System.Drawing.Point(163, 367);
      this.Icon203.Name = "Icon203";
      this.Icon203.Size = new System.Drawing.Size(14, 18);
      this.Icon203.TabIndex = 553;
      this.Icon203.TabStop = false;
      this.Icon203.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon204
      // 
      this.Icon204.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon204.Enabled = false;
      this.Icon204.Location = new System.Drawing.Point(176, 367);
      this.Icon204.Name = "Icon204";
      this.Icon204.Size = new System.Drawing.Size(14, 18);
      this.Icon204.TabIndex = 554;
      this.Icon204.TabStop = false;
      this.Icon204.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon205
      // 
      this.Icon205.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon205.Enabled = false;
      this.Icon205.Location = new System.Drawing.Point(189, 367);
      this.Icon205.Name = "Icon205";
      this.Icon205.Size = new System.Drawing.Size(14, 18);
      this.Icon205.TabIndex = 555;
      this.Icon205.TabStop = false;
      this.Icon205.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon206
      // 
      this.Icon206.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon206.Enabled = false;
      this.Icon206.Location = new System.Drawing.Point(202, 367);
      this.Icon206.Name = "Icon206";
      this.Icon206.Size = new System.Drawing.Size(14, 18);
      this.Icon206.TabIndex = 556;
      this.Icon206.TabStop = false;
      this.Icon206.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon207
      // 
      this.Icon207.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon207.Enabled = false;
      this.Icon207.Location = new System.Drawing.Point(215, 367);
      this.Icon207.Name = "Icon207";
      this.Icon207.Size = new System.Drawing.Size(14, 18);
      this.Icon207.TabIndex = 557;
      this.Icon207.TabStop = false;
      this.Icon207.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon208
      // 
      this.Icon208.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon208.Enabled = false;
      this.Icon208.Location = new System.Drawing.Point(20, 384);
      this.Icon208.Name = "Icon208";
      this.Icon208.Size = new System.Drawing.Size(14, 18);
      this.Icon208.TabIndex = 558;
      this.Icon208.TabStop = false;
      this.Icon208.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon209
      // 
      this.Icon209.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon209.Enabled = false;
      this.Icon209.Location = new System.Drawing.Point(33, 384);
      this.Icon209.Name = "Icon209";
      this.Icon209.Size = new System.Drawing.Size(14, 18);
      this.Icon209.TabIndex = 559;
      this.Icon209.TabStop = false;
      this.Icon209.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon210
      // 
      this.Icon210.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon210.Enabled = false;
      this.Icon210.Location = new System.Drawing.Point(46, 384);
      this.Icon210.Name = "Icon210";
      this.Icon210.Size = new System.Drawing.Size(14, 18);
      this.Icon210.TabIndex = 560;
      this.Icon210.TabStop = false;
      this.Icon210.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon211
      // 
      this.Icon211.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon211.Enabled = false;
      this.Icon211.Location = new System.Drawing.Point(59, 384);
      this.Icon211.Name = "Icon211";
      this.Icon211.Size = new System.Drawing.Size(14, 18);
      this.Icon211.TabIndex = 561;
      this.Icon211.TabStop = false;
      this.Icon211.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon212
      // 
      this.Icon212.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon212.Enabled = false;
      this.Icon212.Location = new System.Drawing.Point(72, 384);
      this.Icon212.Name = "Icon212";
      this.Icon212.Size = new System.Drawing.Size(14, 18);
      this.Icon212.TabIndex = 562;
      this.Icon212.TabStop = false;
      this.Icon212.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon213
      // 
      this.Icon213.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon213.Enabled = false;
      this.Icon213.Location = new System.Drawing.Point(85, 384);
      this.Icon213.Name = "Icon213";
      this.Icon213.Size = new System.Drawing.Size(14, 18);
      this.Icon213.TabIndex = 563;
      this.Icon213.TabStop = false;
      this.Icon213.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon214
      // 
      this.Icon214.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon214.Enabled = false;
      this.Icon214.Location = new System.Drawing.Point(98, 384);
      this.Icon214.Name = "Icon214";
      this.Icon214.Size = new System.Drawing.Size(14, 18);
      this.Icon214.TabIndex = 564;
      this.Icon214.TabStop = false;
      this.Icon214.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon215
      // 
      this.Icon215.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon215.Enabled = false;
      this.Icon215.Location = new System.Drawing.Point(111, 384);
      this.Icon215.Name = "Icon215";
      this.Icon215.Size = new System.Drawing.Size(14, 18);
      this.Icon215.TabIndex = 565;
      this.Icon215.TabStop = false;
      this.Icon215.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon216
      // 
      this.Icon216.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon216.Enabled = false;
      this.Icon216.Location = new System.Drawing.Point(124, 384);
      this.Icon216.Name = "Icon216";
      this.Icon216.Size = new System.Drawing.Size(14, 18);
      this.Icon216.TabIndex = 566;
      this.Icon216.TabStop = false;
      this.Icon216.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon217
      // 
      this.Icon217.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon217.Enabled = false;
      this.Icon217.Location = new System.Drawing.Point(137, 384);
      this.Icon217.Name = "Icon217";
      this.Icon217.Size = new System.Drawing.Size(14, 18);
      this.Icon217.TabIndex = 567;
      this.Icon217.TabStop = false;
      this.Icon217.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon218
      // 
      this.Icon218.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon218.Enabled = false;
      this.Icon218.Location = new System.Drawing.Point(150, 384);
      this.Icon218.Name = "Icon218";
      this.Icon218.Size = new System.Drawing.Size(14, 18);
      this.Icon218.TabIndex = 568;
      this.Icon218.TabStop = false;
      this.Icon218.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon219
      // 
      this.Icon219.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon219.Enabled = false;
      this.Icon219.Location = new System.Drawing.Point(163, 384);
      this.Icon219.Name = "Icon219";
      this.Icon219.Size = new System.Drawing.Size(14, 18);
      this.Icon219.TabIndex = 569;
      this.Icon219.TabStop = false;
      this.Icon219.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon220
      // 
      this.Icon220.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon220.Enabled = false;
      this.Icon220.Location = new System.Drawing.Point(176, 384);
      this.Icon220.Name = "Icon220";
      this.Icon220.Size = new System.Drawing.Size(14, 18);
      this.Icon220.TabIndex = 570;
      this.Icon220.TabStop = false;
      this.Icon220.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon221
      // 
      this.Icon221.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon221.Enabled = false;
      this.Icon221.Location = new System.Drawing.Point(189, 384);
      this.Icon221.Name = "Icon221";
      this.Icon221.Size = new System.Drawing.Size(14, 18);
      this.Icon221.TabIndex = 571;
      this.Icon221.TabStop = false;
      this.Icon221.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon222
      // 
      this.Icon222.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon222.Enabled = false;
      this.Icon222.Location = new System.Drawing.Point(202, 384);
      this.Icon222.Name = "Icon222";
      this.Icon222.Size = new System.Drawing.Size(14, 18);
      this.Icon222.TabIndex = 572;
      this.Icon222.TabStop = false;
      this.Icon222.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon223
      // 
      this.Icon223.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon223.Enabled = false;
      this.Icon223.Location = new System.Drawing.Point(215, 384);
      this.Icon223.Name = "Icon223";
      this.Icon223.Size = new System.Drawing.Size(14, 18);
      this.Icon223.TabIndex = 573;
      this.Icon223.TabStop = false;
      this.Icon223.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon224
      // 
      this.Icon224.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon224.Enabled = false;
      this.Icon224.Location = new System.Drawing.Point(20, 401);
      this.Icon224.Name = "Icon224";
      this.Icon224.Size = new System.Drawing.Size(14, 18);
      this.Icon224.TabIndex = 1;
      this.Icon224.TabStop = false;
      this.Icon224.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon225
      // 
      this.Icon225.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon225.Enabled = false;
      this.Icon225.Location = new System.Drawing.Point(33, 401);
      this.Icon225.Name = "Icon225";
      this.Icon225.Size = new System.Drawing.Size(14, 18);
      this.Icon225.TabIndex = 2;
      this.Icon225.TabStop = false;
      this.Icon225.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon226
      // 
      this.Icon226.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon226.Enabled = false;
      this.Icon226.Location = new System.Drawing.Point(46, 401);
      this.Icon226.Name = "Icon226";
      this.Icon226.Size = new System.Drawing.Size(14, 18);
      this.Icon226.TabIndex = 3;
      this.Icon226.TabStop = false;
      this.Icon226.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon227
      // 
      this.Icon227.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon227.Enabled = false;
      this.Icon227.Location = new System.Drawing.Point(59, 401);
      this.Icon227.Name = "Icon227";
      this.Icon227.Size = new System.Drawing.Size(14, 18);
      this.Icon227.TabIndex = 4;
      this.Icon227.TabStop = false;
      this.Icon227.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon228
      // 
      this.Icon228.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon228.Enabled = false;
      this.Icon228.Location = new System.Drawing.Point(72, 401);
      this.Icon228.Name = "Icon228";
      this.Icon228.Size = new System.Drawing.Size(14, 18);
      this.Icon228.TabIndex = 5;
      this.Icon228.TabStop = false;
      this.Icon228.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon229
      // 
      this.Icon229.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon229.Enabled = false;
      this.Icon229.Location = new System.Drawing.Point(85, 401);
      this.Icon229.Name = "Icon229";
      this.Icon229.Size = new System.Drawing.Size(14, 18);
      this.Icon229.TabIndex = 6;
      this.Icon229.TabStop = false;
      this.Icon229.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon230
      // 
      this.Icon230.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon230.Enabled = false;
      this.Icon230.Location = new System.Drawing.Point(98, 401);
      this.Icon230.Name = "Icon230";
      this.Icon230.Size = new System.Drawing.Size(14, 18);
      this.Icon230.TabIndex = 7;
      this.Icon230.TabStop = false;
      this.Icon230.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon231
      // 
      this.Icon231.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon231.Enabled = false;
      this.Icon231.Location = new System.Drawing.Point(111, 401);
      this.Icon231.Name = "Icon231";
      this.Icon231.Size = new System.Drawing.Size(14, 18);
      this.Icon231.TabIndex = 8;
      this.Icon231.TabStop = false;
      this.Icon231.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon232
      // 
      this.Icon232.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon232.Enabled = false;
      this.Icon232.Location = new System.Drawing.Point(124, 401);
      this.Icon232.Name = "Icon232";
      this.Icon232.Size = new System.Drawing.Size(14, 18);
      this.Icon232.TabIndex = 9;
      this.Icon232.TabStop = false;
      this.Icon232.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon233
      // 
      this.Icon233.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon233.Enabled = false;
      this.Icon233.Location = new System.Drawing.Point(137, 401);
      this.Icon233.Name = "Icon233";
      this.Icon233.Size = new System.Drawing.Size(14, 18);
      this.Icon233.TabIndex = 10;
      this.Icon233.TabStop = false;
      this.Icon233.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon234
      // 
      this.Icon234.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon234.Enabled = false;
      this.Icon234.Location = new System.Drawing.Point(150, 401);
      this.Icon234.Name = "Icon234";
      this.Icon234.Size = new System.Drawing.Size(14, 18);
      this.Icon234.TabIndex = 552;
      this.Icon234.TabStop = false;
      this.Icon234.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon235
      // 
      this.Icon235.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon235.Enabled = false;
      this.Icon235.Location = new System.Drawing.Point(163, 401);
      this.Icon235.Name = "Icon235";
      this.Icon235.Size = new System.Drawing.Size(14, 18);
      this.Icon235.TabIndex = 553;
      this.Icon235.TabStop = false;
      this.Icon235.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon236
      // 
      this.Icon236.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon236.Enabled = false;
      this.Icon236.Location = new System.Drawing.Point(176, 401);
      this.Icon236.Name = "Icon236";
      this.Icon236.Size = new System.Drawing.Size(14, 18);
      this.Icon236.TabIndex = 554;
      this.Icon236.TabStop = false;
      this.Icon236.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon237
      // 
      this.Icon237.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon237.Enabled = false;
      this.Icon237.Location = new System.Drawing.Point(189, 401);
      this.Icon237.Name = "Icon237";
      this.Icon237.Size = new System.Drawing.Size(14, 18);
      this.Icon237.TabIndex = 555;
      this.Icon237.TabStop = false;
      this.Icon237.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon238
      // 
      this.Icon238.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon238.Enabled = false;
      this.Icon238.Location = new System.Drawing.Point(202, 401);
      this.Icon238.Name = "Icon238";
      this.Icon238.Size = new System.Drawing.Size(14, 18);
      this.Icon238.TabIndex = 556;
      this.Icon238.TabStop = false;
      this.Icon238.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon239
      // 
      this.Icon239.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon239.Enabled = false;
      this.Icon239.Location = new System.Drawing.Point(215, 401);
      this.Icon239.Name = "Icon239";
      this.Icon239.Size = new System.Drawing.Size(14, 18);
      this.Icon239.TabIndex = 557;
      this.Icon239.TabStop = false;
      this.Icon239.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon240
      // 
      this.Icon240.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon240.Enabled = false;
      this.Icon240.Location = new System.Drawing.Point(20, 418);
      this.Icon240.Name = "Icon240";
      this.Icon240.Size = new System.Drawing.Size(14, 18);
      this.Icon240.TabIndex = 558;
      this.Icon240.TabStop = false;
      this.Icon240.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon241
      // 
      this.Icon241.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon241.Enabled = false;
      this.Icon241.Location = new System.Drawing.Point(33, 418);
      this.Icon241.Name = "Icon241";
      this.Icon241.Size = new System.Drawing.Size(14, 18);
      this.Icon241.TabIndex = 559;
      this.Icon241.TabStop = false;
      this.Icon241.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon242
      // 
      this.Icon242.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon242.Enabled = false;
      this.Icon242.Location = new System.Drawing.Point(46, 418);
      this.Icon242.Name = "Icon242";
      this.Icon242.Size = new System.Drawing.Size(14, 18);
      this.Icon242.TabIndex = 560;
      this.Icon242.TabStop = false;
      this.Icon242.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon243
      // 
      this.Icon243.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon243.Enabled = false;
      this.Icon243.Location = new System.Drawing.Point(59, 418);
      this.Icon243.Name = "Icon243";
      this.Icon243.Size = new System.Drawing.Size(14, 18);
      this.Icon243.TabIndex = 561;
      this.Icon243.TabStop = false;
      this.Icon243.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon244
      // 
      this.Icon244.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon244.Enabled = false;
      this.Icon244.Location = new System.Drawing.Point(72, 418);
      this.Icon244.Name = "Icon244";
      this.Icon244.Size = new System.Drawing.Size(14, 18);
      this.Icon244.TabIndex = 562;
      this.Icon244.TabStop = false;
      this.Icon244.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon245
      // 
      this.Icon245.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon245.Enabled = false;
      this.Icon245.Location = new System.Drawing.Point(85, 418);
      this.Icon245.Name = "Icon245";
      this.Icon245.Size = new System.Drawing.Size(14, 18);
      this.Icon245.TabIndex = 563;
      this.Icon245.TabStop = false;
      this.Icon245.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon246
      // 
      this.Icon246.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon246.Enabled = false;
      this.Icon246.Location = new System.Drawing.Point(98, 418);
      this.Icon246.Name = "Icon246";
      this.Icon246.Size = new System.Drawing.Size(14, 18);
      this.Icon246.TabIndex = 564;
      this.Icon246.TabStop = false;
      this.Icon246.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon247
      // 
      this.Icon247.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon247.Enabled = false;
      this.Icon247.Location = new System.Drawing.Point(111, 418);
      this.Icon247.Name = "Icon247";
      this.Icon247.Size = new System.Drawing.Size(14, 18);
      this.Icon247.TabIndex = 565;
      this.Icon247.TabStop = false;
      this.Icon247.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon248
      // 
      this.Icon248.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon248.Enabled = false;
      this.Icon248.Location = new System.Drawing.Point(124, 418);
      this.Icon248.Name = "Icon248";
      this.Icon248.Size = new System.Drawing.Size(14, 18);
      this.Icon248.TabIndex = 566;
      this.Icon248.TabStop = false;
      this.Icon248.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon249
      // 
      this.Icon249.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon249.Enabled = false;
      this.Icon249.Location = new System.Drawing.Point(137, 418);
      this.Icon249.Name = "Icon249";
      this.Icon249.Size = new System.Drawing.Size(14, 18);
      this.Icon249.TabIndex = 567;
      this.Icon249.TabStop = false;
      this.Icon249.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon250
      // 
      this.Icon250.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon250.Enabled = false;
      this.Icon250.Location = new System.Drawing.Point(150, 418);
      this.Icon250.Name = "Icon250";
      this.Icon250.Size = new System.Drawing.Size(14, 18);
      this.Icon250.TabIndex = 568;
      this.Icon250.TabStop = false;
      this.Icon250.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon251
      // 
      this.Icon251.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon251.Enabled = false;
      this.Icon251.Location = new System.Drawing.Point(163, 418);
      this.Icon251.Name = "Icon251";
      this.Icon251.Size = new System.Drawing.Size(14, 18);
      this.Icon251.TabIndex = 569;
      this.Icon251.TabStop = false;
      this.Icon251.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon252
      // 
      this.Icon252.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon252.Enabled = false;
      this.Icon252.Location = new System.Drawing.Point(176, 418);
      this.Icon252.Name = "Icon252";
      this.Icon252.Size = new System.Drawing.Size(14, 18);
      this.Icon252.TabIndex = 570;
      this.Icon252.TabStop = false;
      this.Icon252.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon253
      // 
      this.Icon253.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon253.Enabled = false;
      this.Icon253.Location = new System.Drawing.Point(189, 418);
      this.Icon253.Name = "Icon253";
      this.Icon253.Size = new System.Drawing.Size(14, 18);
      this.Icon253.TabIndex = 571;
      this.Icon253.TabStop = false;
      this.Icon253.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon254
      // 
      this.Icon254.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon254.Enabled = false;
      this.Icon254.Location = new System.Drawing.Point(202, 418);
      this.Icon254.Name = "Icon254";
      this.Icon254.Size = new System.Drawing.Size(14, 18);
      this.Icon254.TabIndex = 572;
      this.Icon254.TabStop = false;
      this.Icon254.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon255
      // 
      this.Icon255.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon255.Enabled = false;
      this.Icon255.Location = new System.Drawing.Point(215, 418);
      this.Icon255.Name = "Icon255";
      this.Icon255.Size = new System.Drawing.Size(14, 18);
      this.Icon255.TabIndex = 573;
      this.Icon255.TabStop = false;
      this.Icon255.Click += new System.EventHandler(this.Icon_Click);
      // 
      // iMONLCDg_FontEdit
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(392, 481);
      this.Controls.Add(this.cmdExit);
      this.Controls.Add(this.cmdSave);
      this.Controls.Add(this.cmdLoadCustom);
      this.Controls.Add(this.cmdLoadInternal);
      this.Controls.Add(this.Icon0);
      this.Controls.Add(this.Icon1);
      this.Controls.Add(this.Icon2);
      this.Controls.Add(this.Icon3);
      this.Controls.Add(this.Icon4);
      this.Controls.Add(this.Icon5);
      this.Controls.Add(this.Icon6);
      this.Controls.Add(this.Icon7);
      this.Controls.Add(this.Icon8);
      this.Controls.Add(this.Icon9);
      this.Controls.Add(this.Icon10);
      this.Controls.Add(this.Icon11);
      this.Controls.Add(this.Icon12);
      this.Controls.Add(this.Icon13);
      this.Controls.Add(this.Icon14);
      this.Controls.Add(this.Icon15);
      this.Controls.Add(this.Icon16);
      this.Controls.Add(this.Icon17);
      this.Controls.Add(this.Icon18);
      this.Controls.Add(this.Icon19);
      this.Controls.Add(this.Icon20);
      this.Controls.Add(this.Icon21);
      this.Controls.Add(this.Icon22);
      this.Controls.Add(this.Icon23);
      this.Controls.Add(this.Icon24);
      this.Controls.Add(this.Icon25);
      this.Controls.Add(this.Icon26);
      this.Controls.Add(this.Icon27);
      this.Controls.Add(this.Icon28);
      this.Controls.Add(this.Icon29);
      this.Controls.Add(this.Icon30);
      this.Controls.Add(this.Icon31);
      this.Controls.Add(this.Icon32);
      this.Controls.Add(this.Icon33);
      this.Controls.Add(this.Icon34);
      this.Controls.Add(this.Icon35);
      this.Controls.Add(this.Icon36);
      this.Controls.Add(this.Icon37);
      this.Controls.Add(this.Icon38);
      this.Controls.Add(this.Icon39);
      this.Controls.Add(this.Icon40);
      this.Controls.Add(this.Icon41);
      this.Controls.Add(this.Icon42);
      this.Controls.Add(this.Icon43);
      this.Controls.Add(this.Icon44);
      this.Controls.Add(this.Icon45);
      this.Controls.Add(this.Icon46);
      this.Controls.Add(this.Icon47);
      this.Controls.Add(this.Icon48);
      this.Controls.Add(this.Icon49);
      this.Controls.Add(this.Icon50);
      this.Controls.Add(this.Icon51);
      this.Controls.Add(this.Icon52);
      this.Controls.Add(this.Icon53);
      this.Controls.Add(this.Icon54);
      this.Controls.Add(this.Icon55);
      this.Controls.Add(this.Icon56);
      this.Controls.Add(this.Icon57);
      this.Controls.Add(this.Icon58);
      this.Controls.Add(this.Icon59);
      this.Controls.Add(this.Icon60);
      this.Controls.Add(this.Icon61);
      this.Controls.Add(this.Icon62);
      this.Controls.Add(this.Icon63);
      this.Controls.Add(this.Icon64);
      this.Controls.Add(this.Icon65);
      this.Controls.Add(this.Icon66);
      this.Controls.Add(this.Icon67);
      this.Controls.Add(this.Icon68);
      this.Controls.Add(this.Icon69);
      this.Controls.Add(this.Icon70);
      this.Controls.Add(this.Icon71);
      this.Controls.Add(this.Icon72);
      this.Controls.Add(this.Icon73);
      this.Controls.Add(this.Icon74);
      this.Controls.Add(this.Icon75);
      this.Controls.Add(this.Icon76);
      this.Controls.Add(this.Icon77);
      this.Controls.Add(this.Icon78);
      this.Controls.Add(this.Icon79);
      this.Controls.Add(this.Icon80);
      this.Controls.Add(this.Icon81);
      this.Controls.Add(this.Icon82);
      this.Controls.Add(this.Icon83);
      this.Controls.Add(this.Icon84);
      this.Controls.Add(this.Icon85);
      this.Controls.Add(this.Icon86);
      this.Controls.Add(this.Icon87);
      this.Controls.Add(this.Icon88);
      this.Controls.Add(this.Icon89);
      this.Controls.Add(this.Icon90);
      this.Controls.Add(this.Icon91);
      this.Controls.Add(this.Icon92);
      this.Controls.Add(this.Icon93);
      this.Controls.Add(this.Icon94);
      this.Controls.Add(this.Icon95);
      this.Controls.Add(this.Icon96);
      this.Controls.Add(this.Icon97);
      this.Controls.Add(this.Icon98);
      this.Controls.Add(this.Icon99);
      this.Controls.Add(this.Icon100);
      this.Controls.Add(this.Icon101);
      this.Controls.Add(this.Icon102);
      this.Controls.Add(this.Icon103);
      this.Controls.Add(this.Icon104);
      this.Controls.Add(this.Icon105);
      this.Controls.Add(this.Icon106);
      this.Controls.Add(this.Icon107);
      this.Controls.Add(this.Icon108);
      this.Controls.Add(this.Icon109);
      this.Controls.Add(this.Icon110);
      this.Controls.Add(this.Icon111);
      this.Controls.Add(this.Icon112);
      this.Controls.Add(this.Icon113);
      this.Controls.Add(this.Icon114);
      this.Controls.Add(this.Icon115);
      this.Controls.Add(this.Icon116);
      this.Controls.Add(this.Icon117);
      this.Controls.Add(this.Icon118);
      this.Controls.Add(this.Icon119);
      this.Controls.Add(this.Icon120);
      this.Controls.Add(this.Icon121);
      this.Controls.Add(this.Icon122);
      this.Controls.Add(this.Icon123);
      this.Controls.Add(this.Icon124);
      this.Controls.Add(this.Icon125);
      this.Controls.Add(this.Icon126);
      this.Controls.Add(this.Icon127);
      this.Controls.Add(this.Icon128);
      this.Controls.Add(this.Icon129);
      this.Controls.Add(this.Icon130);
      this.Controls.Add(this.Icon131);
      this.Controls.Add(this.Icon132);
      this.Controls.Add(this.Icon133);
      this.Controls.Add(this.Icon134);
      this.Controls.Add(this.Icon135);
      this.Controls.Add(this.Icon136);
      this.Controls.Add(this.Icon137);
      this.Controls.Add(this.Icon138);
      this.Controls.Add(this.Icon139);
      this.Controls.Add(this.Icon140);
      this.Controls.Add(this.Icon141);
      this.Controls.Add(this.Icon142);
      this.Controls.Add(this.Icon143);
      this.Controls.Add(this.Icon144);
      this.Controls.Add(this.Icon145);
      this.Controls.Add(this.Icon146);
      this.Controls.Add(this.Icon147);
      this.Controls.Add(this.Icon148);
      this.Controls.Add(this.Icon149);
      this.Controls.Add(this.Icon150);
      this.Controls.Add(this.Icon151);
      this.Controls.Add(this.Icon152);
      this.Controls.Add(this.Icon153);
      this.Controls.Add(this.Icon154);
      this.Controls.Add(this.Icon155);
      this.Controls.Add(this.Icon156);
      this.Controls.Add(this.Icon157);
      this.Controls.Add(this.Icon158);
      this.Controls.Add(this.Icon159);
      this.Controls.Add(this.Icon160);
      this.Controls.Add(this.Icon161);
      this.Controls.Add(this.Icon162);
      this.Controls.Add(this.Icon163);
      this.Controls.Add(this.Icon164);
      this.Controls.Add(this.Icon165);
      this.Controls.Add(this.Icon166);
      this.Controls.Add(this.Icon167);
      this.Controls.Add(this.Icon168);
      this.Controls.Add(this.Icon169);
      this.Controls.Add(this.Icon170);
      this.Controls.Add(this.Icon171);
      this.Controls.Add(this.Icon172);
      this.Controls.Add(this.Icon173);
      this.Controls.Add(this.Icon174);
      this.Controls.Add(this.Icon175);
      this.Controls.Add(this.Icon176);
      this.Controls.Add(this.Icon177);
      this.Controls.Add(this.Icon178);
      this.Controls.Add(this.Icon179);
      this.Controls.Add(this.Icon180);
      this.Controls.Add(this.Icon181);
      this.Controls.Add(this.Icon182);
      this.Controls.Add(this.Icon183);
      this.Controls.Add(this.Icon184);
      this.Controls.Add(this.Icon185);
      this.Controls.Add(this.Icon186);
      this.Controls.Add(this.Icon187);
      this.Controls.Add(this.Icon188);
      this.Controls.Add(this.Icon189);
      this.Controls.Add(this.Icon190);
      this.Controls.Add(this.Icon191);
      this.Controls.Add(this.Icon192);
      this.Controls.Add(this.Icon193);
      this.Controls.Add(this.Icon194);
      this.Controls.Add(this.Icon195);
      this.Controls.Add(this.Icon196);
      this.Controls.Add(this.Icon197);
      this.Controls.Add(this.Icon198);
      this.Controls.Add(this.Icon199);
      this.Controls.Add(this.Icon200);
      this.Controls.Add(this.Icon201);
      this.Controls.Add(this.Icon202);
      this.Controls.Add(this.Icon203);
      this.Controls.Add(this.Icon204);
      this.Controls.Add(this.Icon205);
      this.Controls.Add(this.Icon206);
      this.Controls.Add(this.Icon207);
      this.Controls.Add(this.Icon208);
      this.Controls.Add(this.Icon209);
      this.Controls.Add(this.Icon210);
      this.Controls.Add(this.Icon211);
      this.Controls.Add(this.Icon212);
      this.Controls.Add(this.Icon213);
      this.Controls.Add(this.Icon214);
      this.Controls.Add(this.Icon215);
      this.Controls.Add(this.Icon216);
      this.Controls.Add(this.Icon217);
      this.Controls.Add(this.Icon218);
      this.Controls.Add(this.Icon219);
      this.Controls.Add(this.Icon220);
      this.Controls.Add(this.Icon221);
      this.Controls.Add(this.Icon222);
      this.Controls.Add(this.Icon223);
      this.Controls.Add(this.Icon224);
      this.Controls.Add(this.Icon225);
      this.Controls.Add(this.Icon226);
      this.Controls.Add(this.Icon227);
      this.Controls.Add(this.Icon228);
      this.Controls.Add(this.Icon229);
      this.Controls.Add(this.Icon230);
      this.Controls.Add(this.Icon231);
      this.Controls.Add(this.Icon232);
      this.Controls.Add(this.Icon233);
      this.Controls.Add(this.Icon234);
      this.Controls.Add(this.Icon235);
      this.Controls.Add(this.Icon236);
      this.Controls.Add(this.Icon237);
      this.Controls.Add(this.Icon238);
      this.Controls.Add(this.Icon239);
      this.Controls.Add(this.Icon240);
      this.Controls.Add(this.Icon241);
      this.Controls.Add(this.Icon242);
      this.Controls.Add(this.Icon243);
      this.Controls.Add(this.Icon244);
      this.Controls.Add(this.Icon245);
      this.Controls.Add(this.Icon246);
      this.Controls.Add(this.Icon247);
      this.Controls.Add(this.Icon248);
      this.Controls.Add(this.Icon249);
      this.Controls.Add(this.Icon250);
      this.Controls.Add(this.Icon251);
      this.Controls.Add(this.Icon252);
      this.Controls.Add(this.Icon253);
      this.Controls.Add(this.Icon254);
      this.Controls.Add(this.Icon255);
      this.Controls.Add(this.panel1);
      this.Name = "iMONLCDg_FontEdit";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "iMONLCDg_FontEdit";
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.Icon0)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon3)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon4)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon5)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon6)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon7)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon8)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon9)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon10)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon11)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon12)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon13)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon14)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon15)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon16)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon17)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon18)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon19)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon20)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon21)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon22)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon23)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon24)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon25)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon26)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon27)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon28)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon29)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon30)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon31)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon32)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon33)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon34)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon35)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon36)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon37)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon38)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon39)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon40)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon41)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon42)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon43)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon44)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon45)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon46)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon47)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon48)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon49)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon50)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon51)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon52)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon53)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon54)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon55)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon56)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon57)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon58)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon59)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon60)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon61)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon62)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon63)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon64)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon65)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon66)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon67)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon68)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon69)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon70)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon71)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon72)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon73)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon74)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon75)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon76)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon77)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon78)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon79)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon80)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon81)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon82)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon83)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon84)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon85)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon86)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon87)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon88)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon89)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon90)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon91)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon92)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon93)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon94)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon95)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon96)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon97)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon98)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon99)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon100)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon101)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon102)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon103)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon104)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon105)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon106)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon107)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon108)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon109)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon110)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon111)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon112)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon113)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon114)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon115)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon116)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon117)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon118)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon119)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon120)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon121)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon122)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon123)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon124)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon125)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon126)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon127)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon128)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon129)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon130)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon131)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon132)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon133)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon134)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon135)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon136)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon137)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon138)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon139)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon140)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon141)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon142)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon143)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon144)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon145)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon146)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon147)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon148)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon149)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon150)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon151)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon152)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon153)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon154)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon155)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon156)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon157)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon158)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon159)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon160)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon161)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon162)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon163)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon164)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon165)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon166)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon167)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon168)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon169)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon170)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon171)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon172)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon173)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon174)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon175)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon176)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon177)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon178)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon179)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon180)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon181)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon182)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon183)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon184)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon185)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon186)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon187)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon188)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon189)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon190)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon191)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon192)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon193)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon194)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon195)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon196)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon197)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon198)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon199)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon200)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon201)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon202)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon203)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon204)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon205)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon206)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon207)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon208)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon209)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon210)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon211)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon212)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon213)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon214)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon215)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon216)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon217)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon218)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon219)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon220)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon221)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon222)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon223)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon224)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon225)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon226)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon227)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon228)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon229)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon230)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon231)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon232)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon233)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon234)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon235)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon236)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon237)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon238)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon239)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon240)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon241)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon242)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon243)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon244)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon245)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon246)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon247)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon248)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon249)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon250)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon251)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon252)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon253)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon254)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon255)).EndInit();
      this.ResumeLayout(false);

    }

    public void LoadCustomFont()
    {
      DataTable table = new DataTable("Character");
      DataColumn column = new DataColumn("CharID");
      DataColumn column2 = new DataColumn("CData0");
      DataColumn column3 = new DataColumn("CData1");
      DataColumn column4 = new DataColumn("CData2");
      DataColumn column5 = new DataColumn("CData3");
      DataColumn column6 = new DataColumn("CData4");
      DataColumn column7 = new DataColumn("CData5");
      table.Rows.Clear();
      table.Columns.Clear();
      column.DataType = typeof(byte);
      table.Columns.Add(column);
      column2.DataType = typeof(byte);
      table.Columns.Add(column2);
      column3.DataType = typeof(byte);
      table.Columns.Add(column3);
      column4.DataType = typeof(byte);
      table.Columns.Add(column4);
      column5.DataType = typeof(byte);
      table.Columns.Add(column5);
      column6.DataType = typeof(byte);
      table.Columns.Add(column6);
      column7.DataType = typeof(byte);
      table.Columns.Add(column7);
      table.Clear();
      if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_font.xml")))
      {
        table.Rows.Clear();
        XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
        XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_font.xml"));
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
        this.CopyBufferToGraphics();
        this.IconsChanged = false;
      }
      else
      {
        this.LoadInteralFont();
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
      this.CopyBufferToGraphics();
      this.IconsChanged = false;
      this.EnableIconSelection(true);
    }

    private void Pixel_Click(object sender, EventArgs e)
    {
      try
      {
        CheckBox box = (CheckBox)sender;
        if (box.Checked)
        {
          box.CheckState = CheckState.Indeterminate;
        }
        else
        {
          box.CheckState = CheckState.Unchecked;
        }
      } catch (Exception exception)
      {
        Log.Debug("CAUGHT EXCEPTION: {0}", new object[] { exception });
      }
    }

    private void SetEditPixel(int Column, int Row, bool SetOn)
    {
      string key = "C" + Column.ToString().Trim() + "_B" + Row.ToString().Trim();
      Control[] controlArray = this.panel1.Controls.Find(key, false);
      if (controlArray.Length > 0)
      {
        CheckBox box = (CheckBox)controlArray[0];
        if (SetOn)
        {
          box.CheckState = CheckState.Indeterminate;
        }
        else
        {
          box.CheckState = CheckState.Unchecked;
        }
      }
    }

    private void SetIconEdit(int CharIndex)
    {
      this.lblCurrentIcon.Text = "( Character " + CharIndex.ToString() + " )";
      this.DisplayIconForEditing(CharIndex);
      this.EditIndex = CharIndex;
      this.EnableEditPanel(true);
    }
  }
}

