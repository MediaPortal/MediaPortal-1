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
  public class iMONLCDg_IconEdit : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    private static readonly byte[,] _IconBuffer = new byte[10, 0x20];
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
    private IContainer components = null;
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
    private string[] IconFunction = new string[] { "( Idle )", "( TV )", "( MOVIE )", "( MUSIC )", "( VIDEO )", "( RECORDING )", "( PAUSED )", "", "", "" };
    private Bitmap[] IconGraphics = new Bitmap[10];
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
      this.InitializeComponent();
    }

    private void ClearIconBuffer()
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
      this.ClearIconBuffer();
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
      for (int i = 0; i < 0x10; i++)
      {
        for (int j = 0; j < 0x10; j++)
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
      for (int i = 0; i < 0x10; i++)
      {
        for (int j = 0; j < 0x10; j++)
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
      this.LoadCustomIcons();
      this.EnableIconSelection(true);
    }

    private void cmdLoadInternal_Click(object sender, EventArgs e)
    {
      this.LoadInteralIcons();
      this.EnableIconSelection(true);
    }

    private void cmdSave_Click(object sender, EventArgs e)
    {
      if (this.IconsChanged)
      {
        DataTable o = new DataTable("LargeIcons");
        DataColumn column = new DataColumn("IconID");
        DataColumn column2 = new DataColumn("IData0");
        DataColumn column3 = new DataColumn("IData1");
        DataColumn column4 = new DataColumn("IData2");
        DataColumn column5 = new DataColumn("IData3");
        DataColumn column6 = new DataColumn("IData4");
        DataColumn column7 = new DataColumn("IData5");
        DataColumn column8 = new DataColumn("IData6");
        DataColumn column9 = new DataColumn("IData7");
        DataColumn column10 = new DataColumn("IData8");
        DataColumn column11 = new DataColumn("IData9");
        DataColumn column12 = new DataColumn("IData10");
        DataColumn column13 = new DataColumn("IData11");
        DataColumn column14 = new DataColumn("IData12");
        DataColumn column15 = new DataColumn("IData13");
        DataColumn column16 = new DataColumn("IData14");
        DataColumn column17 = new DataColumn("IData15");
        DataColumn column18 = new DataColumn("IData16");
        DataColumn column19 = new DataColumn("IData17");
        DataColumn column20 = new DataColumn("IData18");
        DataColumn column21 = new DataColumn("IData19");
        DataColumn column22 = new DataColumn("IData20");
        DataColumn column23 = new DataColumn("IData21");
        DataColumn column24 = new DataColumn("IData22");
        DataColumn column25 = new DataColumn("IData23");
        DataColumn column26 = new DataColumn("IData24");
        DataColumn column27 = new DataColumn("IData25");
        DataColumn column28 = new DataColumn("IData26");
        DataColumn column29 = new DataColumn("IData27");
        DataColumn column30 = new DataColumn("IData28");
        DataColumn column31 = new DataColumn("IData29");
        DataColumn column32 = new DataColumn("IData30");
        DataColumn column33 = new DataColumn("IData31");
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
        column8.DataType = typeof(byte);
        o.Columns.Add(column8);
        column9.DataType = typeof(byte);
        o.Columns.Add(column9);
        column10.DataType = typeof(byte);
        o.Columns.Add(column10);
        column11.DataType = typeof(byte);
        o.Columns.Add(column11);
        column12.DataType = typeof(byte);
        o.Columns.Add(column12);
        column13.DataType = typeof(byte);
        o.Columns.Add(column13);
        column14.DataType = typeof(byte);
        o.Columns.Add(column14);
        column15.DataType = typeof(byte);
        o.Columns.Add(column15);
        column16.DataType = typeof(byte);
        o.Columns.Add(column16);
        column17.DataType = typeof(byte);
        o.Columns.Add(column17);
        column18.DataType = typeof(byte);
        o.Columns.Add(column18);
        column19.DataType = typeof(byte);
        o.Columns.Add(column19);
        column20.DataType = typeof(byte);
        o.Columns.Add(column20);
        column21.DataType = typeof(byte);
        o.Columns.Add(column21);
        column22.DataType = typeof(byte);
        o.Columns.Add(column22);
        column23.DataType = typeof(byte);
        o.Columns.Add(column23);
        column24.DataType = typeof(byte);
        o.Columns.Add(column24);
        column25.DataType = typeof(byte);
        o.Columns.Add(column25);
        column26.DataType = typeof(byte);
        o.Columns.Add(column26);
        column27.DataType = typeof(byte);
        o.Columns.Add(column27);
        column28.DataType = typeof(byte);
        o.Columns.Add(column28);
        column29.DataType = typeof(byte);
        o.Columns.Add(column29);
        column30.DataType = typeof(byte);
        o.Columns.Add(column30);
        column31.DataType = typeof(byte);
        o.Columns.Add(column31);
        column32.DataType = typeof(byte);
        o.Columns.Add(column32);
        column33.DataType = typeof(byte);
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
        XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
        TextWriter textWriter = new StreamWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_icons.xml"));
        serializer.Serialize(textWriter, o);
        textWriter.Close();
        this.ClearIconDisplay();
        this.EnableIconSelection(false);
        this.cmdSave.Enabled = false;
      }
    }

    private void cmdSaveEdit_Click(object sender, EventArgs e)
    {
      if (this.EditIndex >= 0)
      {
        for (int i = 0; i < 0x10; i++)
        {
          byte num2 = 0;
          byte num3 = 0;
          for (int j = 0; j < 8; j++)
          {
            num2 = (byte)(num2 | ((byte)(((this.GetEditPixel(i, j + 8) == CheckState.Indeterminate) ? ((double)1) : ((double)0)) * Math.Pow(2.0, (double)j))));
            num3 = (byte)(num3 | ((byte)(((this.GetEditPixel(i, j) == CheckState.Indeterminate) ? ((double)1) : ((double)0)) * Math.Pow(2.0, (double)j))));
          }
          _IconBuffer[this.EditIndex, i] = num2;
          _IconBuffer[this.EditIndex, i + 0x10] = num3;
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
      for (int i = 0; i < 0x10; i++)
      {
        for (int j = 0; j < 0x10; j++)
        {
          this.SetEditPixel(i, j, true);
        }
      }
    }

    public void CopyBufferToGraphics()
    {
      for (int i = 0; i < 10; i++)
      {
        if (this.IconGraphics[i] == null)
        {
          this.IconGraphics[i] = new Bitmap(0x20, 0x20);
        }
        for (int j = 0; j < 0x10; j++)
        {
          byte[] buffer = new byte[] { _IconBuffer[i, j], _IconBuffer[i, j + 0x10] };
          for (int k = 0; k < 0x10; k++)
          {
            Color black;
            int index = (k < 8) ? 1 : 0;
            int num5 = (k < 8) ? k : (k - 8);
            int num6 = (int)Math.Pow(2.0, (double)num5);
            bool flag = (buffer[index] & num6) > 0;
            int x = j * 2;
            int y = 0x1f - (k * 2);
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
            this.IconGraphics[i].SetPixel(x, y - 1, black);
            this.IconGraphics[i].SetPixel(x + 1, y - 1, black);
          }
        }
        switch (i)
        {
          case 0:
            this.Icon0.Image = this.IconGraphics[i];
            break;

          case 1:
            this.Icon1.Image = this.IconGraphics[i];
            break;

          case 2:
            this.Icon2.Image = this.IconGraphics[i];
            break;

          case 3:
            this.Icon3.Image = this.IconGraphics[i];
            break;

          case 4:
            this.Icon4.Image = this.IconGraphics[i];
            break;

          case 5:
            this.Icon5.Image = this.IconGraphics[i];
            break;

          case 6:
            this.Icon6.Image = this.IconGraphics[i];
            break;

          case 7:
            this.Icon7.Image = this.IconGraphics[i];
            break;

          case 8:
            this.Icon8.Image = this.IconGraphics[i];
            break;

          case 9:
            this.Icon9.Image = this.IconGraphics[i];
            break;
        }
      }
    }

    private void DisplayIconForEditing(int IconIndex)
    {
      for (int i = 0; i < 0x10; i++)
      {
        byte[] buffer = new byte[] { _IconBuffer[IconIndex, i], _IconBuffer[IconIndex, i + 0x10] };
        for (int j = 0; j < 0x10; j++)
        {
          int index = (j < 8) ? 1 : 0;
          int num4 = (j < 8) ? j : (j - 8);
          int num5 = (int)Math.Pow(2.0, (double)num4);
          bool setOn = (buffer[index] & num5) > 0;
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
      this.Icon0.Enabled = Enable;
      this.Icon1.Enabled = Enable;
      this.Icon2.Enabled = Enable;
      this.Icon3.Enabled = Enable;
      this.Icon4.Enabled = Enable;
      this.Icon5.Enabled = Enable;
      this.Icon6.Enabled = Enable;
      this.Icon7.Enabled = Enable;
      this.Icon8.Enabled = Enable;
      this.Icon9.Enabled = Enable;
      if (!Enable)
      {
        this.Icon0.Image = null;
        this.Icon1.Image = null;
        this.Icon2.Image = null;
        this.Icon3.Image = null;
        this.Icon4.Image = null;
        this.Icon5.Image = null;
        this.Icon6.Image = null;
        this.Icon7.Image = null;
        this.Icon8.Image = null;
        this.Icon9.Image = null;
      }
    }

    private CheckState GetEditPixel(int Column, int Row)
    {
      int num = (Row > 7) ? 0 : 1;
      int num2 = (Row < 8) ? Row : (Row - 8);
      string key = "C" + Column.ToString().Trim() + "R" + num.ToString().Trim() + "_B" + num2.ToString().Trim();
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
        int index = int.Parse(box.Name.Substring(4));
        this.lblCurrentIcon.Text = this.IconFunction[index];
        this.DisplayIconForEditing(index);
        this.EditIndex = index;
        this.EnableEditPanel(true);
      } catch (Exception exception)
      {
        Log.Debug("CAUGHT EXCEPTION: {0}", new object[] { exception });
      }
    }

    private void InitializeComponent()
    {
      this.Icon0 = new System.Windows.Forms.PictureBox();
      this.Icon1 = new System.Windows.Forms.PictureBox();
      this.Icon2 = new System.Windows.Forms.PictureBox();
      this.Icon3 = new System.Windows.Forms.PictureBox();
      this.Icon4 = new System.Windows.Forms.PictureBox();
      this.Icon9 = new System.Windows.Forms.PictureBox();
      this.Icon8 = new System.Windows.Forms.PictureBox();
      this.Icon7 = new System.Windows.Forms.PictureBox();
      this.Icon6 = new System.Windows.Forms.PictureBox();
      this.Icon5 = new System.Windows.Forms.PictureBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.label10 = new System.Windows.Forms.Label();
      this.label11 = new System.Windows.Forms.Label();
      this.label12 = new System.Windows.Forms.Label();
      this.label13 = new System.Windows.Forms.Label();
      this.label14 = new System.Windows.Forms.Label();
      this.label15 = new System.Windows.Forms.Label();
      this.label16 = new System.Windows.Forms.Label();
      this.label17 = new System.Windows.Forms.Label();
      this.label18 = new System.Windows.Forms.Label();
      this.label19 = new System.Windows.Forms.Label();
      this.label20 = new System.Windows.Forms.Label();
      this.panel1 = new System.Windows.Forms.Panel();
      this.lblEditIndex = new System.Windows.Forms.Label();
      this.cmdSaveEdit = new System.Windows.Forms.Button();
      this.cmdCancelEdit = new System.Windows.Forms.Button();
      this.cmdInvert = new System.Windows.Forms.Button();
      this.cmdSetAll = new System.Windows.Forms.Button();
      this.cmdClearAll = new System.Windows.Forms.Button();
      this.lblCurrentIcon = new System.Windows.Forms.Label();
      this.C15R1_B0 = new System.Windows.Forms.CheckBox();
      this.C15R1_B1 = new System.Windows.Forms.CheckBox();
      this.C15R1_B2 = new System.Windows.Forms.CheckBox();
      this.C15R1_B3 = new System.Windows.Forms.CheckBox();
      this.C15R1_B4 = new System.Windows.Forms.CheckBox();
      this.C15R1_B5 = new System.Windows.Forms.CheckBox();
      this.C15R1_B6 = new System.Windows.Forms.CheckBox();
      this.C15R1_B7 = new System.Windows.Forms.CheckBox();
      this.C15R0_B0 = new System.Windows.Forms.CheckBox();
      this.C15R0_B1 = new System.Windows.Forms.CheckBox();
      this.C15R0_B2 = new System.Windows.Forms.CheckBox();
      this.C15R0_B3 = new System.Windows.Forms.CheckBox();
      this.C15R0_B4 = new System.Windows.Forms.CheckBox();
      this.C15R0_B5 = new System.Windows.Forms.CheckBox();
      this.C15R0_B6 = new System.Windows.Forms.CheckBox();
      this.C15R0_B7 = new System.Windows.Forms.CheckBox();
      this.C14R1_B0 = new System.Windows.Forms.CheckBox();
      this.C14R1_B1 = new System.Windows.Forms.CheckBox();
      this.C14R1_B2 = new System.Windows.Forms.CheckBox();
      this.C14R1_B3 = new System.Windows.Forms.CheckBox();
      this.C14R1_B4 = new System.Windows.Forms.CheckBox();
      this.C14R1_B5 = new System.Windows.Forms.CheckBox();
      this.C14R1_B6 = new System.Windows.Forms.CheckBox();
      this.C14R1_B7 = new System.Windows.Forms.CheckBox();
      this.C14R0_B0 = new System.Windows.Forms.CheckBox();
      this.C14R0_B1 = new System.Windows.Forms.CheckBox();
      this.C14R0_B2 = new System.Windows.Forms.CheckBox();
      this.C14R0_B3 = new System.Windows.Forms.CheckBox();
      this.C14R0_B4 = new System.Windows.Forms.CheckBox();
      this.C14R0_B5 = new System.Windows.Forms.CheckBox();
      this.C14R0_B6 = new System.Windows.Forms.CheckBox();
      this.C14R0_B7 = new System.Windows.Forms.CheckBox();
      this.C13R1_B0 = new System.Windows.Forms.CheckBox();
      this.C13R1_B1 = new System.Windows.Forms.CheckBox();
      this.C13R1_B2 = new System.Windows.Forms.CheckBox();
      this.C13R1_B3 = new System.Windows.Forms.CheckBox();
      this.C13R1_B4 = new System.Windows.Forms.CheckBox();
      this.C13R1_B5 = new System.Windows.Forms.CheckBox();
      this.C13R1_B6 = new System.Windows.Forms.CheckBox();
      this.C13R1_B7 = new System.Windows.Forms.CheckBox();
      this.C13R0_B0 = new System.Windows.Forms.CheckBox();
      this.C13R0_B1 = new System.Windows.Forms.CheckBox();
      this.C13R0_B2 = new System.Windows.Forms.CheckBox();
      this.C13R0_B3 = new System.Windows.Forms.CheckBox();
      this.C13R0_B4 = new System.Windows.Forms.CheckBox();
      this.C13R0_B5 = new System.Windows.Forms.CheckBox();
      this.C13R0_B6 = new System.Windows.Forms.CheckBox();
      this.C13R0_B7 = new System.Windows.Forms.CheckBox();
      this.C12R1_B0 = new System.Windows.Forms.CheckBox();
      this.C12R1_B1 = new System.Windows.Forms.CheckBox();
      this.C12R1_B2 = new System.Windows.Forms.CheckBox();
      this.C12R1_B3 = new System.Windows.Forms.CheckBox();
      this.C12R1_B4 = new System.Windows.Forms.CheckBox();
      this.C12R1_B5 = new System.Windows.Forms.CheckBox();
      this.C12R1_B6 = new System.Windows.Forms.CheckBox();
      this.C12R1_B7 = new System.Windows.Forms.CheckBox();
      this.C12R0_B0 = new System.Windows.Forms.CheckBox();
      this.C12R0_B1 = new System.Windows.Forms.CheckBox();
      this.C12R0_B2 = new System.Windows.Forms.CheckBox();
      this.C12R0_B3 = new System.Windows.Forms.CheckBox();
      this.C12R0_B4 = new System.Windows.Forms.CheckBox();
      this.C12R0_B5 = new System.Windows.Forms.CheckBox();
      this.C12R0_B6 = new System.Windows.Forms.CheckBox();
      this.C12R0_B7 = new System.Windows.Forms.CheckBox();
      this.C11R1_B0 = new System.Windows.Forms.CheckBox();
      this.C11R1_B1 = new System.Windows.Forms.CheckBox();
      this.C11R1_B2 = new System.Windows.Forms.CheckBox();
      this.C11R1_B3 = new System.Windows.Forms.CheckBox();
      this.C11R1_B4 = new System.Windows.Forms.CheckBox();
      this.C11R1_B5 = new System.Windows.Forms.CheckBox();
      this.C11R1_B6 = new System.Windows.Forms.CheckBox();
      this.C11R1_B7 = new System.Windows.Forms.CheckBox();
      this.C11R0_B0 = new System.Windows.Forms.CheckBox();
      this.C11R0_B1 = new System.Windows.Forms.CheckBox();
      this.C11R0_B2 = new System.Windows.Forms.CheckBox();
      this.C11R0_B3 = new System.Windows.Forms.CheckBox();
      this.C11R0_B4 = new System.Windows.Forms.CheckBox();
      this.C11R0_B5 = new System.Windows.Forms.CheckBox();
      this.C11R0_B6 = new System.Windows.Forms.CheckBox();
      this.C11R0_B7 = new System.Windows.Forms.CheckBox();
      this.C10R1_B0 = new System.Windows.Forms.CheckBox();
      this.C10R1_B1 = new System.Windows.Forms.CheckBox();
      this.C10R1_B2 = new System.Windows.Forms.CheckBox();
      this.C10R1_B3 = new System.Windows.Forms.CheckBox();
      this.C10R1_B4 = new System.Windows.Forms.CheckBox();
      this.C10R1_B5 = new System.Windows.Forms.CheckBox();
      this.C10R1_B6 = new System.Windows.Forms.CheckBox();
      this.C10R1_B7 = new System.Windows.Forms.CheckBox();
      this.C10R0_B0 = new System.Windows.Forms.CheckBox();
      this.C10R0_B1 = new System.Windows.Forms.CheckBox();
      this.C10R0_B2 = new System.Windows.Forms.CheckBox();
      this.C10R0_B3 = new System.Windows.Forms.CheckBox();
      this.C10R0_B4 = new System.Windows.Forms.CheckBox();
      this.C10R0_B5 = new System.Windows.Forms.CheckBox();
      this.C10R0_B6 = new System.Windows.Forms.CheckBox();
      this.C10R0_B7 = new System.Windows.Forms.CheckBox();
      this.C9R1_B0 = new System.Windows.Forms.CheckBox();
      this.C9R1_B1 = new System.Windows.Forms.CheckBox();
      this.C9R1_B2 = new System.Windows.Forms.CheckBox();
      this.C9R1_B3 = new System.Windows.Forms.CheckBox();
      this.C9R1_B4 = new System.Windows.Forms.CheckBox();
      this.C9R1_B5 = new System.Windows.Forms.CheckBox();
      this.C9R1_B6 = new System.Windows.Forms.CheckBox();
      this.C9R1_B7 = new System.Windows.Forms.CheckBox();
      this.C9R0_B0 = new System.Windows.Forms.CheckBox();
      this.C9R0_B1 = new System.Windows.Forms.CheckBox();
      this.C9R0_B2 = new System.Windows.Forms.CheckBox();
      this.C9R0_B3 = new System.Windows.Forms.CheckBox();
      this.C9R0_B4 = new System.Windows.Forms.CheckBox();
      this.C9R0_B5 = new System.Windows.Forms.CheckBox();
      this.C9R0_B6 = new System.Windows.Forms.CheckBox();
      this.C9R0_B7 = new System.Windows.Forms.CheckBox();
      this.C8R1_B0 = new System.Windows.Forms.CheckBox();
      this.C8R1_B1 = new System.Windows.Forms.CheckBox();
      this.C8R1_B2 = new System.Windows.Forms.CheckBox();
      this.C8R1_B3 = new System.Windows.Forms.CheckBox();
      this.C8R1_B4 = new System.Windows.Forms.CheckBox();
      this.C8R1_B5 = new System.Windows.Forms.CheckBox();
      this.C8R1_B6 = new System.Windows.Forms.CheckBox();
      this.C8R1_B7 = new System.Windows.Forms.CheckBox();
      this.C8R0_B0 = new System.Windows.Forms.CheckBox();
      this.C8R0_B1 = new System.Windows.Forms.CheckBox();
      this.C8R0_B2 = new System.Windows.Forms.CheckBox();
      this.C8R0_B3 = new System.Windows.Forms.CheckBox();
      this.C8R0_B4 = new System.Windows.Forms.CheckBox();
      this.C8R0_B5 = new System.Windows.Forms.CheckBox();
      this.C8R0_B6 = new System.Windows.Forms.CheckBox();
      this.C8R0_B7 = new System.Windows.Forms.CheckBox();
      this.C7R1_B0 = new System.Windows.Forms.CheckBox();
      this.C7R1_B1 = new System.Windows.Forms.CheckBox();
      this.C7R1_B2 = new System.Windows.Forms.CheckBox();
      this.C7R1_B3 = new System.Windows.Forms.CheckBox();
      this.C7R1_B4 = new System.Windows.Forms.CheckBox();
      this.C7R1_B5 = new System.Windows.Forms.CheckBox();
      this.C7R1_B6 = new System.Windows.Forms.CheckBox();
      this.C7R1_B7 = new System.Windows.Forms.CheckBox();
      this.C7R0_B0 = new System.Windows.Forms.CheckBox();
      this.C7R0_B1 = new System.Windows.Forms.CheckBox();
      this.C7R0_B2 = new System.Windows.Forms.CheckBox();
      this.C7R0_B3 = new System.Windows.Forms.CheckBox();
      this.C7R0_B4 = new System.Windows.Forms.CheckBox();
      this.C7R0_B5 = new System.Windows.Forms.CheckBox();
      this.C7R0_B6 = new System.Windows.Forms.CheckBox();
      this.C7R0_B7 = new System.Windows.Forms.CheckBox();
      this.C6R1_B0 = new System.Windows.Forms.CheckBox();
      this.C6R1_B1 = new System.Windows.Forms.CheckBox();
      this.C6R1_B2 = new System.Windows.Forms.CheckBox();
      this.C6R1_B3 = new System.Windows.Forms.CheckBox();
      this.C6R1_B4 = new System.Windows.Forms.CheckBox();
      this.C6R1_B5 = new System.Windows.Forms.CheckBox();
      this.C6R1_B6 = new System.Windows.Forms.CheckBox();
      this.C6R1_B7 = new System.Windows.Forms.CheckBox();
      this.C6R0_B0 = new System.Windows.Forms.CheckBox();
      this.C6R0_B1 = new System.Windows.Forms.CheckBox();
      this.C6R0_B2 = new System.Windows.Forms.CheckBox();
      this.C6R0_B3 = new System.Windows.Forms.CheckBox();
      this.C6R0_B4 = new System.Windows.Forms.CheckBox();
      this.C6R0_B5 = new System.Windows.Forms.CheckBox();
      this.C6R0_B6 = new System.Windows.Forms.CheckBox();
      this.C6R0_B7 = new System.Windows.Forms.CheckBox();
      this.C5R1_B0 = new System.Windows.Forms.CheckBox();
      this.C5R1_B1 = new System.Windows.Forms.CheckBox();
      this.C5R1_B2 = new System.Windows.Forms.CheckBox();
      this.C5R1_B3 = new System.Windows.Forms.CheckBox();
      this.C5R1_B4 = new System.Windows.Forms.CheckBox();
      this.C5R1_B5 = new System.Windows.Forms.CheckBox();
      this.C5R1_B6 = new System.Windows.Forms.CheckBox();
      this.C5R1_B7 = new System.Windows.Forms.CheckBox();
      this.C5R0_B0 = new System.Windows.Forms.CheckBox();
      this.C5R0_B1 = new System.Windows.Forms.CheckBox();
      this.C5R0_B2 = new System.Windows.Forms.CheckBox();
      this.C5R0_B3 = new System.Windows.Forms.CheckBox();
      this.C5R0_B4 = new System.Windows.Forms.CheckBox();
      this.C5R0_B5 = new System.Windows.Forms.CheckBox();
      this.C5R0_B6 = new System.Windows.Forms.CheckBox();
      this.C5R0_B7 = new System.Windows.Forms.CheckBox();
      this.C4R1_B0 = new System.Windows.Forms.CheckBox();
      this.C4R1_B1 = new System.Windows.Forms.CheckBox();
      this.C4R1_B2 = new System.Windows.Forms.CheckBox();
      this.C4R1_B3 = new System.Windows.Forms.CheckBox();
      this.C4R1_B4 = new System.Windows.Forms.CheckBox();
      this.C4R1_B5 = new System.Windows.Forms.CheckBox();
      this.C4R1_B6 = new System.Windows.Forms.CheckBox();
      this.C4R1_B7 = new System.Windows.Forms.CheckBox();
      this.C4R0_B0 = new System.Windows.Forms.CheckBox();
      this.C4R0_B1 = new System.Windows.Forms.CheckBox();
      this.C4R0_B2 = new System.Windows.Forms.CheckBox();
      this.C4R0_B3 = new System.Windows.Forms.CheckBox();
      this.C4R0_B4 = new System.Windows.Forms.CheckBox();
      this.C4R0_B5 = new System.Windows.Forms.CheckBox();
      this.C4R0_B6 = new System.Windows.Forms.CheckBox();
      this.C4R0_B7 = new System.Windows.Forms.CheckBox();
      this.C3R1_B0 = new System.Windows.Forms.CheckBox();
      this.C3R1_B1 = new System.Windows.Forms.CheckBox();
      this.C3R1_B2 = new System.Windows.Forms.CheckBox();
      this.C3R1_B3 = new System.Windows.Forms.CheckBox();
      this.C3R1_B4 = new System.Windows.Forms.CheckBox();
      this.C3R1_B5 = new System.Windows.Forms.CheckBox();
      this.C3R1_B6 = new System.Windows.Forms.CheckBox();
      this.C3R1_B7 = new System.Windows.Forms.CheckBox();
      this.C3R0_B0 = new System.Windows.Forms.CheckBox();
      this.C3R0_B1 = new System.Windows.Forms.CheckBox();
      this.C3R0_B2 = new System.Windows.Forms.CheckBox();
      this.C3R0_B3 = new System.Windows.Forms.CheckBox();
      this.C3R0_B4 = new System.Windows.Forms.CheckBox();
      this.C3R0_B5 = new System.Windows.Forms.CheckBox();
      this.C3R0_B6 = new System.Windows.Forms.CheckBox();
      this.C3R0_B7 = new System.Windows.Forms.CheckBox();
      this.C2R1_B0 = new System.Windows.Forms.CheckBox();
      this.C2R1_B1 = new System.Windows.Forms.CheckBox();
      this.C2R1_B2 = new System.Windows.Forms.CheckBox();
      this.C2R1_B3 = new System.Windows.Forms.CheckBox();
      this.C2R1_B4 = new System.Windows.Forms.CheckBox();
      this.C2R1_B5 = new System.Windows.Forms.CheckBox();
      this.C2R1_B6 = new System.Windows.Forms.CheckBox();
      this.C2R1_B7 = new System.Windows.Forms.CheckBox();
      this.C2R0_B0 = new System.Windows.Forms.CheckBox();
      this.C2R0_B1 = new System.Windows.Forms.CheckBox();
      this.C2R0_B2 = new System.Windows.Forms.CheckBox();
      this.C2R0_B3 = new System.Windows.Forms.CheckBox();
      this.C2R0_B4 = new System.Windows.Forms.CheckBox();
      this.C2R0_B5 = new System.Windows.Forms.CheckBox();
      this.C2R0_B6 = new System.Windows.Forms.CheckBox();
      this.C2R0_B7 = new System.Windows.Forms.CheckBox();
      this.C1R1_B0 = new System.Windows.Forms.CheckBox();
      this.C1R1_B1 = new System.Windows.Forms.CheckBox();
      this.C1R1_B2 = new System.Windows.Forms.CheckBox();
      this.C1R1_B3 = new System.Windows.Forms.CheckBox();
      this.C1R1_B4 = new System.Windows.Forms.CheckBox();
      this.C1R1_B5 = new System.Windows.Forms.CheckBox();
      this.C1R1_B6 = new System.Windows.Forms.CheckBox();
      this.C1R1_B7 = new System.Windows.Forms.CheckBox();
      this.C1R0_B0 = new System.Windows.Forms.CheckBox();
      this.C1R0_B1 = new System.Windows.Forms.CheckBox();
      this.C1R0_B2 = new System.Windows.Forms.CheckBox();
      this.C1R0_B3 = new System.Windows.Forms.CheckBox();
      this.C1R0_B4 = new System.Windows.Forms.CheckBox();
      this.C1R0_B5 = new System.Windows.Forms.CheckBox();
      this.C1R0_B6 = new System.Windows.Forms.CheckBox();
      this.C1R0_B7 = new System.Windows.Forms.CheckBox();
      this.C0R1_B0 = new System.Windows.Forms.CheckBox();
      this.C0R1_B1 = new System.Windows.Forms.CheckBox();
      this.C0R1_B2 = new System.Windows.Forms.CheckBox();
      this.C0R1_B3 = new System.Windows.Forms.CheckBox();
      this.C0R1_B4 = new System.Windows.Forms.CheckBox();
      this.C0R1_B5 = new System.Windows.Forms.CheckBox();
      this.C0R1_B6 = new System.Windows.Forms.CheckBox();
      this.C0R1_B7 = new System.Windows.Forms.CheckBox();
      this.C0R0_B0 = new System.Windows.Forms.CheckBox();
      this.C0R0_B1 = new System.Windows.Forms.CheckBox();
      this.C0R0_B2 = new System.Windows.Forms.CheckBox();
      this.C0R0_B3 = new System.Windows.Forms.CheckBox();
      this.C0R0_B4 = new System.Windows.Forms.CheckBox();
      this.C0R0_B5 = new System.Windows.Forms.CheckBox();
      this.C0R0_B6 = new System.Windows.Forms.CheckBox();
      this.C0R0_B7 = new System.Windows.Forms.CheckBox();
      this.cmdLoadInternal = new System.Windows.Forms.Button();
      this.cmdLoadCustom = new System.Windows.Forms.Button();
      this.cmdSave = new System.Windows.Forms.Button();
      this.cmdExit = new System.Windows.Forms.Button();
      this.label21 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.Icon0)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon3)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon4)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon9)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon8)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon7)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon6)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon5)).BeginInit();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // Icon0
      // 
      this.Icon0.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon0.Enabled = false;
      this.Icon0.Location = new System.Drawing.Point(29, 255);
      this.Icon0.Name = "Icon0";
      this.Icon0.Size = new System.Drawing.Size(34, 34);
      this.Icon0.TabIndex = 1;
      this.Icon0.TabStop = false;
      this.Icon0.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon1
      // 
      this.Icon1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon1.Enabled = false;
      this.Icon1.Location = new System.Drawing.Point(104, 255);
      this.Icon1.Name = "Icon1";
      this.Icon1.Size = new System.Drawing.Size(34, 34);
      this.Icon1.TabIndex = 2;
      this.Icon1.TabStop = false;
      this.Icon1.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon2
      // 
      this.Icon2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon2.Enabled = false;
      this.Icon2.Location = new System.Drawing.Point(179, 255);
      this.Icon2.Name = "Icon2";
      this.Icon2.Size = new System.Drawing.Size(34, 34);
      this.Icon2.TabIndex = 3;
      this.Icon2.TabStop = false;
      this.Icon2.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon3
      // 
      this.Icon3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon3.Enabled = false;
      this.Icon3.Location = new System.Drawing.Point(254, 255);
      this.Icon3.Name = "Icon3";
      this.Icon3.Size = new System.Drawing.Size(34, 34);
      this.Icon3.TabIndex = 4;
      this.Icon3.TabStop = false;
      this.Icon3.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon4
      // 
      this.Icon4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon4.Enabled = false;
      this.Icon4.Location = new System.Drawing.Point(329, 255);
      this.Icon4.Name = "Icon4";
      this.Icon4.Size = new System.Drawing.Size(34, 34);
      this.Icon4.TabIndex = 5;
      this.Icon4.TabStop = false;
      this.Icon4.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon9
      // 
      this.Icon9.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon9.Enabled = false;
      this.Icon9.Location = new System.Drawing.Point(329, 326);
      this.Icon9.Name = "Icon9";
      this.Icon9.Size = new System.Drawing.Size(34, 34);
      this.Icon9.TabIndex = 10;
      this.Icon9.TabStop = false;
      this.Icon9.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon8
      // 
      this.Icon8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon8.Enabled = false;
      this.Icon8.Location = new System.Drawing.Point(254, 326);
      this.Icon8.Name = "Icon8";
      this.Icon8.Size = new System.Drawing.Size(34, 34);
      this.Icon8.TabIndex = 9;
      this.Icon8.TabStop = false;
      this.Icon8.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon7
      // 
      this.Icon7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon7.Enabled = false;
      this.Icon7.Location = new System.Drawing.Point(179, 326);
      this.Icon7.Name = "Icon7";
      this.Icon7.Size = new System.Drawing.Size(34, 34);
      this.Icon7.TabIndex = 8;
      this.Icon7.TabStop = false;
      this.Icon7.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon6
      // 
      this.Icon6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon6.Enabled = false;
      this.Icon6.Location = new System.Drawing.Point(104, 326);
      this.Icon6.Name = "Icon6";
      this.Icon6.Size = new System.Drawing.Size(34, 34);
      this.Icon6.TabIndex = 7;
      this.Icon6.TabStop = false;
      this.Icon6.Click += new System.EventHandler(this.Icon_Click);
      // 
      // Icon5
      // 
      this.Icon5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Icon5.Enabled = false;
      this.Icon5.Location = new System.Drawing.Point(29, 326);
      this.Icon5.Name = "Icon5";
      this.Icon5.Size = new System.Drawing.Size(34, 34);
      this.Icon5.TabIndex = 6;
      this.Icon5.TabStop = false;
      this.Icon5.Click += new System.EventHandler(this.Icon_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(27, 290);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(37, 13);
      this.label1.TabIndex = 267;
      this.label1.Text = "Icon 1";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(102, 290);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(37, 13);
      this.label2.TabIndex = 268;
      this.label2.Text = "Icon 2";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(252, 290);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(37, 13);
      this.label3.TabIndex = 270;
      this.label3.Text = "Icon 4";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(177, 290);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(37, 13);
      this.label4.TabIndex = 269;
      this.label4.Text = "Icon 3";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(327, 290);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(37, 13);
      this.label5.TabIndex = 271;
      this.label5.Text = "Icon 5";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(324, 361);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(43, 13);
      this.label6.TabIndex = 276;
      this.label6.Text = "Icon 10";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(252, 361);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(37, 13);
      this.label7.TabIndex = 275;
      this.label7.Text = "Icon 9";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(177, 361);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(37, 13);
      this.label8.TabIndex = 274;
      this.label8.Text = "Icon 8";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(102, 361);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(37, 13);
      this.label9.TabIndex = 273;
      this.label9.Text = "Icon 7";
      this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(27, 361);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(37, 13);
      this.label10.TabIndex = 272;
      this.label10.Text = "Icon 6";
      this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label11
      // 
      this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(319, 303);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(52, 13);
      this.label11.TabIndex = 281;
      this.label11.Text = "( VIDEO )";
      this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(244, 303);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(53, 13);
      this.label12.TabIndex = 280;
      this.label12.Text = "( MUSIC )";
      this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(169, 303);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(53, 13);
      this.label13.TabIndex = 279;
      this.label13.Text = "( MOVIE )";
      this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Location = new System.Drawing.Point(104, 303);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(33, 13);
      this.label14.TabIndex = 278;
      this.label14.Text = "( TV )";
      this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label15
      // 
      this.label15.AutoSize = true;
      this.label15.Location = new System.Drawing.Point(27, 303);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(36, 13);
      this.label15.TabIndex = 277;
      this.label15.Text = "( Idle )";
      this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label16
      // 
      this.label16.AutoSize = true;
      this.label16.Location = new System.Drawing.Point(317, 374);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(56, 13);
      this.label16.TabIndex = 286;
      this.label16.Text = "( Unused )";
      this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label17
      // 
      this.label17.AutoSize = true;
      this.label17.Location = new System.Drawing.Point(242, 374);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(56, 13);
      this.label17.TabIndex = 285;
      this.label17.Text = "( Unused )";
      this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label18
      // 
      this.label18.AutoSize = true;
      this.label18.Location = new System.Drawing.Point(167, 374);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(56, 13);
      this.label18.TabIndex = 284;
      this.label18.Text = "( Unused )";
      this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label19
      // 
      this.label19.AutoSize = true;
      this.label19.Location = new System.Drawing.Point(89, 374);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(63, 13);
      this.label19.TabIndex = 283;
      this.label19.Text = "( PAUSED )";
      this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label20
      // 
      this.label20.AutoSize = true;
      this.label20.Location = new System.Drawing.Point(3, 374);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(84, 13);
      this.label20.TabIndex = 282;
      this.label20.Text = "( RECORDING )";
      this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
      this.panel1.Controls.Add(this.C15R1_B0);
      this.panel1.Controls.Add(this.C15R1_B1);
      this.panel1.Controls.Add(this.C15R1_B2);
      this.panel1.Controls.Add(this.C15R1_B3);
      this.panel1.Controls.Add(this.C15R1_B4);
      this.panel1.Controls.Add(this.C15R1_B5);
      this.panel1.Controls.Add(this.C15R1_B6);
      this.panel1.Controls.Add(this.C15R1_B7);
      this.panel1.Controls.Add(this.C15R0_B0);
      this.panel1.Controls.Add(this.C15R0_B1);
      this.panel1.Controls.Add(this.C15R0_B2);
      this.panel1.Controls.Add(this.C15R0_B3);
      this.panel1.Controls.Add(this.C15R0_B4);
      this.panel1.Controls.Add(this.C15R0_B5);
      this.panel1.Controls.Add(this.C15R0_B6);
      this.panel1.Controls.Add(this.C15R0_B7);
      this.panel1.Controls.Add(this.C14R1_B0);
      this.panel1.Controls.Add(this.C14R1_B1);
      this.panel1.Controls.Add(this.C14R1_B2);
      this.panel1.Controls.Add(this.C14R1_B3);
      this.panel1.Controls.Add(this.C14R1_B4);
      this.panel1.Controls.Add(this.C14R1_B5);
      this.panel1.Controls.Add(this.C14R1_B6);
      this.panel1.Controls.Add(this.C14R1_B7);
      this.panel1.Controls.Add(this.C14R0_B0);
      this.panel1.Controls.Add(this.C14R0_B1);
      this.panel1.Controls.Add(this.C14R0_B2);
      this.panel1.Controls.Add(this.C14R0_B3);
      this.panel1.Controls.Add(this.C14R0_B4);
      this.panel1.Controls.Add(this.C14R0_B5);
      this.panel1.Controls.Add(this.C14R0_B6);
      this.panel1.Controls.Add(this.C14R0_B7);
      this.panel1.Controls.Add(this.C13R1_B0);
      this.panel1.Controls.Add(this.C13R1_B1);
      this.panel1.Controls.Add(this.C13R1_B2);
      this.panel1.Controls.Add(this.C13R1_B3);
      this.panel1.Controls.Add(this.C13R1_B4);
      this.panel1.Controls.Add(this.C13R1_B5);
      this.panel1.Controls.Add(this.C13R1_B6);
      this.panel1.Controls.Add(this.C13R1_B7);
      this.panel1.Controls.Add(this.C13R0_B0);
      this.panel1.Controls.Add(this.C13R0_B1);
      this.panel1.Controls.Add(this.C13R0_B2);
      this.panel1.Controls.Add(this.C13R0_B3);
      this.panel1.Controls.Add(this.C13R0_B4);
      this.panel1.Controls.Add(this.C13R0_B5);
      this.panel1.Controls.Add(this.C13R0_B6);
      this.panel1.Controls.Add(this.C13R0_B7);
      this.panel1.Controls.Add(this.C12R1_B0);
      this.panel1.Controls.Add(this.C12R1_B1);
      this.panel1.Controls.Add(this.C12R1_B2);
      this.panel1.Controls.Add(this.C12R1_B3);
      this.panel1.Controls.Add(this.C12R1_B4);
      this.panel1.Controls.Add(this.C12R1_B5);
      this.panel1.Controls.Add(this.C12R1_B6);
      this.panel1.Controls.Add(this.C12R1_B7);
      this.panel1.Controls.Add(this.C12R0_B0);
      this.panel1.Controls.Add(this.C12R0_B1);
      this.panel1.Controls.Add(this.C12R0_B2);
      this.panel1.Controls.Add(this.C12R0_B3);
      this.panel1.Controls.Add(this.C12R0_B4);
      this.panel1.Controls.Add(this.C12R0_B5);
      this.panel1.Controls.Add(this.C12R0_B6);
      this.panel1.Controls.Add(this.C12R0_B7);
      this.panel1.Controls.Add(this.C11R1_B0);
      this.panel1.Controls.Add(this.C11R1_B1);
      this.panel1.Controls.Add(this.C11R1_B2);
      this.panel1.Controls.Add(this.C11R1_B3);
      this.panel1.Controls.Add(this.C11R1_B4);
      this.panel1.Controls.Add(this.C11R1_B5);
      this.panel1.Controls.Add(this.C11R1_B6);
      this.panel1.Controls.Add(this.C11R1_B7);
      this.panel1.Controls.Add(this.C11R0_B0);
      this.panel1.Controls.Add(this.C11R0_B1);
      this.panel1.Controls.Add(this.C11R0_B2);
      this.panel1.Controls.Add(this.C11R0_B3);
      this.panel1.Controls.Add(this.C11R0_B4);
      this.panel1.Controls.Add(this.C11R0_B5);
      this.panel1.Controls.Add(this.C11R0_B6);
      this.panel1.Controls.Add(this.C11R0_B7);
      this.panel1.Controls.Add(this.C10R1_B0);
      this.panel1.Controls.Add(this.C10R1_B1);
      this.panel1.Controls.Add(this.C10R1_B2);
      this.panel1.Controls.Add(this.C10R1_B3);
      this.panel1.Controls.Add(this.C10R1_B4);
      this.panel1.Controls.Add(this.C10R1_B5);
      this.panel1.Controls.Add(this.C10R1_B6);
      this.panel1.Controls.Add(this.C10R1_B7);
      this.panel1.Controls.Add(this.C10R0_B0);
      this.panel1.Controls.Add(this.C10R0_B1);
      this.panel1.Controls.Add(this.C10R0_B2);
      this.panel1.Controls.Add(this.C10R0_B3);
      this.panel1.Controls.Add(this.C10R0_B4);
      this.panel1.Controls.Add(this.C10R0_B5);
      this.panel1.Controls.Add(this.C10R0_B6);
      this.panel1.Controls.Add(this.C10R0_B7);
      this.panel1.Controls.Add(this.C9R1_B0);
      this.panel1.Controls.Add(this.C9R1_B1);
      this.panel1.Controls.Add(this.C9R1_B2);
      this.panel1.Controls.Add(this.C9R1_B3);
      this.panel1.Controls.Add(this.C9R1_B4);
      this.panel1.Controls.Add(this.C9R1_B5);
      this.panel1.Controls.Add(this.C9R1_B6);
      this.panel1.Controls.Add(this.C9R1_B7);
      this.panel1.Controls.Add(this.C9R0_B0);
      this.panel1.Controls.Add(this.C9R0_B1);
      this.panel1.Controls.Add(this.C9R0_B2);
      this.panel1.Controls.Add(this.C9R0_B3);
      this.panel1.Controls.Add(this.C9R0_B4);
      this.panel1.Controls.Add(this.C9R0_B5);
      this.panel1.Controls.Add(this.C9R0_B6);
      this.panel1.Controls.Add(this.C9R0_B7);
      this.panel1.Controls.Add(this.C8R1_B0);
      this.panel1.Controls.Add(this.C8R1_B1);
      this.panel1.Controls.Add(this.C8R1_B2);
      this.panel1.Controls.Add(this.C8R1_B3);
      this.panel1.Controls.Add(this.C8R1_B4);
      this.panel1.Controls.Add(this.C8R1_B5);
      this.panel1.Controls.Add(this.C8R1_B6);
      this.panel1.Controls.Add(this.C8R1_B7);
      this.panel1.Controls.Add(this.C8R0_B0);
      this.panel1.Controls.Add(this.C8R0_B1);
      this.panel1.Controls.Add(this.C8R0_B2);
      this.panel1.Controls.Add(this.C8R0_B3);
      this.panel1.Controls.Add(this.C8R0_B4);
      this.panel1.Controls.Add(this.C8R0_B5);
      this.panel1.Controls.Add(this.C8R0_B6);
      this.panel1.Controls.Add(this.C8R0_B7);
      this.panel1.Controls.Add(this.C7R1_B0);
      this.panel1.Controls.Add(this.C7R1_B1);
      this.panel1.Controls.Add(this.C7R1_B2);
      this.panel1.Controls.Add(this.C7R1_B3);
      this.panel1.Controls.Add(this.C7R1_B4);
      this.panel1.Controls.Add(this.C7R1_B5);
      this.panel1.Controls.Add(this.C7R1_B6);
      this.panel1.Controls.Add(this.C7R1_B7);
      this.panel1.Controls.Add(this.C7R0_B0);
      this.panel1.Controls.Add(this.C7R0_B1);
      this.panel1.Controls.Add(this.C7R0_B2);
      this.panel1.Controls.Add(this.C7R0_B3);
      this.panel1.Controls.Add(this.C7R0_B4);
      this.panel1.Controls.Add(this.C7R0_B5);
      this.panel1.Controls.Add(this.C7R0_B6);
      this.panel1.Controls.Add(this.C7R0_B7);
      this.panel1.Controls.Add(this.C6R1_B0);
      this.panel1.Controls.Add(this.C6R1_B1);
      this.panel1.Controls.Add(this.C6R1_B2);
      this.panel1.Controls.Add(this.C6R1_B3);
      this.panel1.Controls.Add(this.C6R1_B4);
      this.panel1.Controls.Add(this.C6R1_B5);
      this.panel1.Controls.Add(this.C6R1_B6);
      this.panel1.Controls.Add(this.C6R1_B7);
      this.panel1.Controls.Add(this.C6R0_B0);
      this.panel1.Controls.Add(this.C6R0_B1);
      this.panel1.Controls.Add(this.C6R0_B2);
      this.panel1.Controls.Add(this.C6R0_B3);
      this.panel1.Controls.Add(this.C6R0_B4);
      this.panel1.Controls.Add(this.C6R0_B5);
      this.panel1.Controls.Add(this.C6R0_B6);
      this.panel1.Controls.Add(this.C6R0_B7);
      this.panel1.Controls.Add(this.C5R1_B0);
      this.panel1.Controls.Add(this.C5R1_B1);
      this.panel1.Controls.Add(this.C5R1_B2);
      this.panel1.Controls.Add(this.C5R1_B3);
      this.panel1.Controls.Add(this.C5R1_B4);
      this.panel1.Controls.Add(this.C5R1_B5);
      this.panel1.Controls.Add(this.C5R1_B6);
      this.panel1.Controls.Add(this.C5R1_B7);
      this.panel1.Controls.Add(this.C5R0_B0);
      this.panel1.Controls.Add(this.C5R0_B1);
      this.panel1.Controls.Add(this.C5R0_B2);
      this.panel1.Controls.Add(this.C5R0_B3);
      this.panel1.Controls.Add(this.C5R0_B4);
      this.panel1.Controls.Add(this.C5R0_B5);
      this.panel1.Controls.Add(this.C5R0_B6);
      this.panel1.Controls.Add(this.C5R0_B7);
      this.panel1.Controls.Add(this.C4R1_B0);
      this.panel1.Controls.Add(this.C4R1_B1);
      this.panel1.Controls.Add(this.C4R1_B2);
      this.panel1.Controls.Add(this.C4R1_B3);
      this.panel1.Controls.Add(this.C4R1_B4);
      this.panel1.Controls.Add(this.C4R1_B5);
      this.panel1.Controls.Add(this.C4R1_B6);
      this.panel1.Controls.Add(this.C4R1_B7);
      this.panel1.Controls.Add(this.C4R0_B0);
      this.panel1.Controls.Add(this.C4R0_B1);
      this.panel1.Controls.Add(this.C4R0_B2);
      this.panel1.Controls.Add(this.C4R0_B3);
      this.panel1.Controls.Add(this.C4R0_B4);
      this.panel1.Controls.Add(this.C4R0_B5);
      this.panel1.Controls.Add(this.C4R0_B6);
      this.panel1.Controls.Add(this.C4R0_B7);
      this.panel1.Controls.Add(this.C3R1_B0);
      this.panel1.Controls.Add(this.C3R1_B1);
      this.panel1.Controls.Add(this.C3R1_B2);
      this.panel1.Controls.Add(this.C3R1_B3);
      this.panel1.Controls.Add(this.C3R1_B4);
      this.panel1.Controls.Add(this.C3R1_B5);
      this.panel1.Controls.Add(this.C3R1_B6);
      this.panel1.Controls.Add(this.C3R1_B7);
      this.panel1.Controls.Add(this.C3R0_B0);
      this.panel1.Controls.Add(this.C3R0_B1);
      this.panel1.Controls.Add(this.C3R0_B2);
      this.panel1.Controls.Add(this.C3R0_B3);
      this.panel1.Controls.Add(this.C3R0_B4);
      this.panel1.Controls.Add(this.C3R0_B5);
      this.panel1.Controls.Add(this.C3R0_B6);
      this.panel1.Controls.Add(this.C3R0_B7);
      this.panel1.Controls.Add(this.C2R1_B0);
      this.panel1.Controls.Add(this.C2R1_B1);
      this.panel1.Controls.Add(this.C2R1_B2);
      this.panel1.Controls.Add(this.C2R1_B3);
      this.panel1.Controls.Add(this.C2R1_B4);
      this.panel1.Controls.Add(this.C2R1_B5);
      this.panel1.Controls.Add(this.C2R1_B6);
      this.panel1.Controls.Add(this.C2R1_B7);
      this.panel1.Controls.Add(this.C2R0_B0);
      this.panel1.Controls.Add(this.C2R0_B1);
      this.panel1.Controls.Add(this.C2R0_B2);
      this.panel1.Controls.Add(this.C2R0_B3);
      this.panel1.Controls.Add(this.C2R0_B4);
      this.panel1.Controls.Add(this.C2R0_B5);
      this.panel1.Controls.Add(this.C2R0_B6);
      this.panel1.Controls.Add(this.C2R0_B7);
      this.panel1.Controls.Add(this.C1R1_B0);
      this.panel1.Controls.Add(this.C1R1_B1);
      this.panel1.Controls.Add(this.C1R1_B2);
      this.panel1.Controls.Add(this.C1R1_B3);
      this.panel1.Controls.Add(this.C1R1_B4);
      this.panel1.Controls.Add(this.C1R1_B5);
      this.panel1.Controls.Add(this.C1R1_B6);
      this.panel1.Controls.Add(this.C1R1_B7);
      this.panel1.Controls.Add(this.C1R0_B0);
      this.panel1.Controls.Add(this.C1R0_B1);
      this.panel1.Controls.Add(this.C1R0_B2);
      this.panel1.Controls.Add(this.C1R0_B3);
      this.panel1.Controls.Add(this.C1R0_B4);
      this.panel1.Controls.Add(this.C1R0_B5);
      this.panel1.Controls.Add(this.C1R0_B6);
      this.panel1.Controls.Add(this.C1R0_B7);
      this.panel1.Controls.Add(this.C0R1_B0);
      this.panel1.Controls.Add(this.C0R1_B1);
      this.panel1.Controls.Add(this.C0R1_B2);
      this.panel1.Controls.Add(this.C0R1_B3);
      this.panel1.Controls.Add(this.C0R1_B4);
      this.panel1.Controls.Add(this.C0R1_B5);
      this.panel1.Controls.Add(this.C0R1_B6);
      this.panel1.Controls.Add(this.C0R1_B7);
      this.panel1.Controls.Add(this.C0R0_B0);
      this.panel1.Controls.Add(this.C0R0_B1);
      this.panel1.Controls.Add(this.C0R0_B2);
      this.panel1.Controls.Add(this.C0R0_B3);
      this.panel1.Controls.Add(this.C0R0_B4);
      this.panel1.Controls.Add(this.C0R0_B5);
      this.panel1.Controls.Add(this.C0R0_B6);
      this.panel1.Controls.Add(this.C0R0_B7);
      this.panel1.Enabled = false;
      this.panel1.Location = new System.Drawing.Point(6, 7);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(305, 242);
      this.panel1.TabIndex = 288;
      // 
      // lblEditIndex
      // 
      this.lblEditIndex.AutoSize = true;
      this.lblEditIndex.Location = new System.Drawing.Point(221, 115);
      this.lblEditIndex.Name = "lblEditIndex";
      this.lblEditIndex.Size = new System.Drawing.Size(61, 13);
      this.lblEditIndex.TabIndex = 550;
      this.lblEditIndex.Text = "lblEditIndex";
      this.lblEditIndex.Visible = false;
      // 
      // cmdSaveEdit
      // 
      this.cmdSaveEdit.Enabled = false;
      this.cmdSaveEdit.Location = new System.Drawing.Point(214, 181);
      this.cmdSaveEdit.Name = "cmdSaveEdit";
      this.cmdSaveEdit.Size = new System.Drawing.Size(75, 23);
      this.cmdSaveEdit.TabIndex = 549;
      this.cmdSaveEdit.Text = "Save";
      this.cmdSaveEdit.UseVisualStyleBackColor = true;
      this.cmdSaveEdit.Click += new System.EventHandler(this.cmdSaveEdit_Click);
      // 
      // cmdCancelEdit
      // 
      this.cmdCancelEdit.Enabled = false;
      this.cmdCancelEdit.Location = new System.Drawing.Point(214, 209);
      this.cmdCancelEdit.Name = "cmdCancelEdit";
      this.cmdCancelEdit.Size = new System.Drawing.Size(75, 23);
      this.cmdCancelEdit.TabIndex = 548;
      this.cmdCancelEdit.Text = "Cancel";
      this.cmdCancelEdit.UseVisualStyleBackColor = true;
      this.cmdCancelEdit.Click += new System.EventHandler(this.cmdCancelEdit_Click);
      // 
      // cmdInvert
      // 
      this.cmdInvert.Enabled = false;
      this.cmdInvert.Location = new System.Drawing.Point(214, 61);
      this.cmdInvert.Name = "cmdInvert";
      this.cmdInvert.Size = new System.Drawing.Size(75, 23);
      this.cmdInvert.TabIndex = 547;
      this.cmdInvert.Text = "Invert";
      this.cmdInvert.UseVisualStyleBackColor = true;
      this.cmdInvert.Click += new System.EventHandler(this.cmdInvert_Click);
      // 
      // cmdSetAll
      // 
      this.cmdSetAll.Enabled = false;
      this.cmdSetAll.Location = new System.Drawing.Point(214, 33);
      this.cmdSetAll.Name = "cmdSetAll";
      this.cmdSetAll.Size = new System.Drawing.Size(75, 23);
      this.cmdSetAll.TabIndex = 546;
      this.cmdSetAll.Text = "Set All";
      this.cmdSetAll.UseVisualStyleBackColor = true;
      this.cmdSetAll.Click += new System.EventHandler(this.cmdSetAll_Click);
      // 
      // cmdClearAll
      // 
      this.cmdClearAll.Enabled = false;
      this.cmdClearAll.Location = new System.Drawing.Point(214, 6);
      this.cmdClearAll.Name = "cmdClearAll";
      this.cmdClearAll.Size = new System.Drawing.Size(75, 23);
      this.cmdClearAll.TabIndex = 545;
      this.cmdClearAll.Text = "Clear All";
      this.cmdClearAll.UseVisualStyleBackColor = true;
      this.cmdClearAll.Click += new System.EventHandler(this.cmdClearAll_Click);
      // 
      // lblCurrentIcon
      // 
      this.lblCurrentIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.lblCurrentIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblCurrentIcon.Location = new System.Drawing.Point(3, 201);
      this.lblCurrentIcon.Name = "lblCurrentIcon";
      this.lblCurrentIcon.Size = new System.Drawing.Size(197, 33);
      this.lblCurrentIcon.TabIndex = 544;
      this.lblCurrentIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // C15R1_B0
      // 
      this.C15R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B0.Location = new System.Drawing.Point(186, 186);
      this.C15R1_B0.Name = "C15R1_B0";
      this.C15R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C15R1_B0.TabIndex = 543;
      this.C15R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B0.ThreeState = true;
      this.C15R1_B0.UseVisualStyleBackColor = true;
      this.C15R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R1_B1
      // 
      this.C15R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B1.Location = new System.Drawing.Point(186, 174);
      this.C15R1_B1.Name = "C15R1_B1";
      this.C15R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C15R1_B1.TabIndex = 542;
      this.C15R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B1.ThreeState = true;
      this.C15R1_B1.UseVisualStyleBackColor = true;
      this.C15R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R1_B2
      // 
      this.C15R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B2.Location = new System.Drawing.Point(186, 162);
      this.C15R1_B2.Name = "C15R1_B2";
      this.C15R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C15R1_B2.TabIndex = 541;
      this.C15R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B2.ThreeState = true;
      this.C15R1_B2.UseVisualStyleBackColor = true;
      this.C15R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R1_B3
      // 
      this.C15R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B3.Location = new System.Drawing.Point(186, 150);
      this.C15R1_B3.Name = "C15R1_B3";
      this.C15R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C15R1_B3.TabIndex = 540;
      this.C15R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B3.ThreeState = true;
      this.C15R1_B3.UseVisualStyleBackColor = true;
      this.C15R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R1_B4
      // 
      this.C15R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B4.Location = new System.Drawing.Point(186, 138);
      this.C15R1_B4.Name = "C15R1_B4";
      this.C15R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C15R1_B4.TabIndex = 539;
      this.C15R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B4.ThreeState = true;
      this.C15R1_B4.UseVisualStyleBackColor = true;
      this.C15R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R1_B5
      // 
      this.C15R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B5.Location = new System.Drawing.Point(186, 126);
      this.C15R1_B5.Name = "C15R1_B5";
      this.C15R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C15R1_B5.TabIndex = 538;
      this.C15R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B5.ThreeState = true;
      this.C15R1_B5.UseVisualStyleBackColor = true;
      this.C15R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R1_B6
      // 
      this.C15R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B6.Location = new System.Drawing.Point(186, 114);
      this.C15R1_B6.Name = "C15R1_B6";
      this.C15R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C15R1_B6.TabIndex = 537;
      this.C15R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B6.ThreeState = true;
      this.C15R1_B6.UseVisualStyleBackColor = true;
      this.C15R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R1_B7
      // 
      this.C15R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B7.Location = new System.Drawing.Point(186, 102);
      this.C15R1_B7.Name = "C15R1_B7";
      this.C15R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C15R1_B7.TabIndex = 536;
      this.C15R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R1_B7.ThreeState = true;
      this.C15R1_B7.UseVisualStyleBackColor = true;
      this.C15R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R0_B0
      // 
      this.C15R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B0.Location = new System.Drawing.Point(186, 90);
      this.C15R0_B0.Name = "C15R0_B0";
      this.C15R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C15R0_B0.TabIndex = 535;
      this.C15R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B0.ThreeState = true;
      this.C15R0_B0.UseVisualStyleBackColor = true;
      this.C15R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R0_B1
      // 
      this.C15R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B1.Location = new System.Drawing.Point(186, 78);
      this.C15R0_B1.Name = "C15R0_B1";
      this.C15R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C15R0_B1.TabIndex = 534;
      this.C15R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B1.ThreeState = true;
      this.C15R0_B1.UseVisualStyleBackColor = true;
      this.C15R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R0_B2
      // 
      this.C15R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B2.Location = new System.Drawing.Point(186, 66);
      this.C15R0_B2.Name = "C15R0_B2";
      this.C15R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C15R0_B2.TabIndex = 533;
      this.C15R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B2.ThreeState = true;
      this.C15R0_B2.UseVisualStyleBackColor = true;
      this.C15R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R0_B3
      // 
      this.C15R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B3.Location = new System.Drawing.Point(186, 54);
      this.C15R0_B3.Name = "C15R0_B3";
      this.C15R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C15R0_B3.TabIndex = 532;
      this.C15R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B3.ThreeState = true;
      this.C15R0_B3.UseVisualStyleBackColor = true;
      this.C15R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R0_B4
      // 
      this.C15R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B4.Location = new System.Drawing.Point(186, 42);
      this.C15R0_B4.Name = "C15R0_B4";
      this.C15R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C15R0_B4.TabIndex = 531;
      this.C15R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B4.ThreeState = true;
      this.C15R0_B4.UseVisualStyleBackColor = true;
      this.C15R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R0_B5
      // 
      this.C15R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B5.Location = new System.Drawing.Point(186, 30);
      this.C15R0_B5.Name = "C15R0_B5";
      this.C15R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C15R0_B5.TabIndex = 530;
      this.C15R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B5.ThreeState = true;
      this.C15R0_B5.UseVisualStyleBackColor = true;
      this.C15R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R0_B6
      // 
      this.C15R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B6.Location = new System.Drawing.Point(186, 18);
      this.C15R0_B6.Name = "C15R0_B6";
      this.C15R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C15R0_B6.TabIndex = 529;
      this.C15R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B6.ThreeState = true;
      this.C15R0_B6.UseVisualStyleBackColor = true;
      this.C15R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C15R0_B7
      // 
      this.C15R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B7.Location = new System.Drawing.Point(186, 6);
      this.C15R0_B7.Name = "C15R0_B7";
      this.C15R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C15R0_B7.TabIndex = 528;
      this.C15R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C15R0_B7.ThreeState = true;
      this.C15R0_B7.UseVisualStyleBackColor = true;
      this.C15R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R1_B0
      // 
      this.C14R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B0.Location = new System.Drawing.Point(174, 186);
      this.C14R1_B0.Name = "C14R1_B0";
      this.C14R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C14R1_B0.TabIndex = 527;
      this.C14R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B0.ThreeState = true;
      this.C14R1_B0.UseVisualStyleBackColor = true;
      this.C14R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R1_B1
      // 
      this.C14R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B1.Location = new System.Drawing.Point(174, 174);
      this.C14R1_B1.Name = "C14R1_B1";
      this.C14R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C14R1_B1.TabIndex = 526;
      this.C14R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B1.ThreeState = true;
      this.C14R1_B1.UseVisualStyleBackColor = true;
      this.C14R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R1_B2
      // 
      this.C14R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B2.Location = new System.Drawing.Point(174, 162);
      this.C14R1_B2.Name = "C14R1_B2";
      this.C14R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C14R1_B2.TabIndex = 525;
      this.C14R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B2.ThreeState = true;
      this.C14R1_B2.UseVisualStyleBackColor = true;
      this.C14R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R1_B3
      // 
      this.C14R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B3.Location = new System.Drawing.Point(174, 150);
      this.C14R1_B3.Name = "C14R1_B3";
      this.C14R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C14R1_B3.TabIndex = 524;
      this.C14R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B3.ThreeState = true;
      this.C14R1_B3.UseVisualStyleBackColor = true;
      this.C14R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R1_B4
      // 
      this.C14R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B4.Location = new System.Drawing.Point(174, 138);
      this.C14R1_B4.Name = "C14R1_B4";
      this.C14R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C14R1_B4.TabIndex = 523;
      this.C14R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B4.ThreeState = true;
      this.C14R1_B4.UseVisualStyleBackColor = true;
      this.C14R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R1_B5
      // 
      this.C14R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B5.Location = new System.Drawing.Point(174, 126);
      this.C14R1_B5.Name = "C14R1_B5";
      this.C14R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C14R1_B5.TabIndex = 522;
      this.C14R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B5.ThreeState = true;
      this.C14R1_B5.UseVisualStyleBackColor = true;
      this.C14R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R1_B6
      // 
      this.C14R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B6.Location = new System.Drawing.Point(174, 114);
      this.C14R1_B6.Name = "C14R1_B6";
      this.C14R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C14R1_B6.TabIndex = 521;
      this.C14R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B6.ThreeState = true;
      this.C14R1_B6.UseVisualStyleBackColor = true;
      this.C14R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R1_B7
      // 
      this.C14R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B7.Location = new System.Drawing.Point(174, 102);
      this.C14R1_B7.Name = "C14R1_B7";
      this.C14R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C14R1_B7.TabIndex = 520;
      this.C14R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R1_B7.ThreeState = true;
      this.C14R1_B7.UseVisualStyleBackColor = true;
      this.C14R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R0_B0
      // 
      this.C14R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B0.Location = new System.Drawing.Point(174, 90);
      this.C14R0_B0.Name = "C14R0_B0";
      this.C14R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C14R0_B0.TabIndex = 519;
      this.C14R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B0.ThreeState = true;
      this.C14R0_B0.UseVisualStyleBackColor = true;
      this.C14R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R0_B1
      // 
      this.C14R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B1.Location = new System.Drawing.Point(174, 78);
      this.C14R0_B1.Name = "C14R0_B1";
      this.C14R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C14R0_B1.TabIndex = 518;
      this.C14R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B1.ThreeState = true;
      this.C14R0_B1.UseVisualStyleBackColor = true;
      this.C14R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R0_B2
      // 
      this.C14R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B2.Location = new System.Drawing.Point(174, 66);
      this.C14R0_B2.Name = "C14R0_B2";
      this.C14R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C14R0_B2.TabIndex = 517;
      this.C14R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B2.ThreeState = true;
      this.C14R0_B2.UseVisualStyleBackColor = true;
      this.C14R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R0_B3
      // 
      this.C14R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B3.Location = new System.Drawing.Point(174, 54);
      this.C14R0_B3.Name = "C14R0_B3";
      this.C14R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C14R0_B3.TabIndex = 516;
      this.C14R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B3.ThreeState = true;
      this.C14R0_B3.UseVisualStyleBackColor = true;
      this.C14R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R0_B4
      // 
      this.C14R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B4.Location = new System.Drawing.Point(174, 42);
      this.C14R0_B4.Name = "C14R0_B4";
      this.C14R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C14R0_B4.TabIndex = 515;
      this.C14R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B4.ThreeState = true;
      this.C14R0_B4.UseVisualStyleBackColor = true;
      this.C14R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R0_B5
      // 
      this.C14R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B5.Location = new System.Drawing.Point(174, 30);
      this.C14R0_B5.Name = "C14R0_B5";
      this.C14R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C14R0_B5.TabIndex = 514;
      this.C14R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B5.ThreeState = true;
      this.C14R0_B5.UseVisualStyleBackColor = true;
      this.C14R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R0_B6
      // 
      this.C14R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B6.Location = new System.Drawing.Point(174, 18);
      this.C14R0_B6.Name = "C14R0_B6";
      this.C14R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C14R0_B6.TabIndex = 513;
      this.C14R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B6.ThreeState = true;
      this.C14R0_B6.UseVisualStyleBackColor = true;
      this.C14R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C14R0_B7
      // 
      this.C14R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B7.Location = new System.Drawing.Point(174, 6);
      this.C14R0_B7.Name = "C14R0_B7";
      this.C14R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C14R0_B7.TabIndex = 512;
      this.C14R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C14R0_B7.ThreeState = true;
      this.C14R0_B7.UseVisualStyleBackColor = true;
      this.C14R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R1_B0
      // 
      this.C13R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B0.Location = new System.Drawing.Point(162, 186);
      this.C13R1_B0.Name = "C13R1_B0";
      this.C13R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C13R1_B0.TabIndex = 511;
      this.C13R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B0.ThreeState = true;
      this.C13R1_B0.UseVisualStyleBackColor = true;
      this.C13R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R1_B1
      // 
      this.C13R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B1.Location = new System.Drawing.Point(162, 174);
      this.C13R1_B1.Name = "C13R1_B1";
      this.C13R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C13R1_B1.TabIndex = 510;
      this.C13R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B1.ThreeState = true;
      this.C13R1_B1.UseVisualStyleBackColor = true;
      this.C13R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R1_B2
      // 
      this.C13R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B2.Location = new System.Drawing.Point(162, 162);
      this.C13R1_B2.Name = "C13R1_B2";
      this.C13R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C13R1_B2.TabIndex = 509;
      this.C13R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B2.ThreeState = true;
      this.C13R1_B2.UseVisualStyleBackColor = true;
      this.C13R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R1_B3
      // 
      this.C13R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B3.Location = new System.Drawing.Point(162, 150);
      this.C13R1_B3.Name = "C13R1_B3";
      this.C13R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C13R1_B3.TabIndex = 508;
      this.C13R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B3.ThreeState = true;
      this.C13R1_B3.UseVisualStyleBackColor = true;
      this.C13R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R1_B4
      // 
      this.C13R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B4.Location = new System.Drawing.Point(162, 138);
      this.C13R1_B4.Name = "C13R1_B4";
      this.C13R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C13R1_B4.TabIndex = 507;
      this.C13R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B4.ThreeState = true;
      this.C13R1_B4.UseVisualStyleBackColor = true;
      this.C13R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R1_B5
      // 
      this.C13R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B5.Location = new System.Drawing.Point(162, 126);
      this.C13R1_B5.Name = "C13R1_B5";
      this.C13R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C13R1_B5.TabIndex = 506;
      this.C13R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B5.ThreeState = true;
      this.C13R1_B5.UseVisualStyleBackColor = true;
      this.C13R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R1_B6
      // 
      this.C13R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B6.Location = new System.Drawing.Point(162, 114);
      this.C13R1_B6.Name = "C13R1_B6";
      this.C13R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C13R1_B6.TabIndex = 505;
      this.C13R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B6.ThreeState = true;
      this.C13R1_B6.UseVisualStyleBackColor = true;
      this.C13R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R1_B7
      // 
      this.C13R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B7.Location = new System.Drawing.Point(162, 102);
      this.C13R1_B7.Name = "C13R1_B7";
      this.C13R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C13R1_B7.TabIndex = 504;
      this.C13R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R1_B7.ThreeState = true;
      this.C13R1_B7.UseVisualStyleBackColor = true;
      this.C13R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R0_B0
      // 
      this.C13R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B0.Location = new System.Drawing.Point(162, 90);
      this.C13R0_B0.Name = "C13R0_B0";
      this.C13R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C13R0_B0.TabIndex = 503;
      this.C13R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B0.ThreeState = true;
      this.C13R0_B0.UseVisualStyleBackColor = true;
      this.C13R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R0_B1
      // 
      this.C13R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B1.Location = new System.Drawing.Point(162, 78);
      this.C13R0_B1.Name = "C13R0_B1";
      this.C13R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C13R0_B1.TabIndex = 502;
      this.C13R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B1.ThreeState = true;
      this.C13R0_B1.UseVisualStyleBackColor = true;
      this.C13R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R0_B2
      // 
      this.C13R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B2.Location = new System.Drawing.Point(162, 66);
      this.C13R0_B2.Name = "C13R0_B2";
      this.C13R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C13R0_B2.TabIndex = 501;
      this.C13R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B2.ThreeState = true;
      this.C13R0_B2.UseVisualStyleBackColor = true;
      this.C13R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R0_B3
      // 
      this.C13R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B3.Location = new System.Drawing.Point(162, 54);
      this.C13R0_B3.Name = "C13R0_B3";
      this.C13R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C13R0_B3.TabIndex = 500;
      this.C13R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B3.ThreeState = true;
      this.C13R0_B3.UseVisualStyleBackColor = true;
      this.C13R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R0_B4
      // 
      this.C13R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B4.Location = new System.Drawing.Point(162, 42);
      this.C13R0_B4.Name = "C13R0_B4";
      this.C13R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C13R0_B4.TabIndex = 499;
      this.C13R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B4.ThreeState = true;
      this.C13R0_B4.UseVisualStyleBackColor = true;
      this.C13R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R0_B5
      // 
      this.C13R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B5.Location = new System.Drawing.Point(162, 30);
      this.C13R0_B5.Name = "C13R0_B5";
      this.C13R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C13R0_B5.TabIndex = 498;
      this.C13R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B5.ThreeState = true;
      this.C13R0_B5.UseVisualStyleBackColor = true;
      this.C13R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R0_B6
      // 
      this.C13R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B6.Location = new System.Drawing.Point(162, 18);
      this.C13R0_B6.Name = "C13R0_B6";
      this.C13R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C13R0_B6.TabIndex = 497;
      this.C13R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B6.ThreeState = true;
      this.C13R0_B6.UseVisualStyleBackColor = true;
      this.C13R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C13R0_B7
      // 
      this.C13R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B7.Location = new System.Drawing.Point(162, 6);
      this.C13R0_B7.Name = "C13R0_B7";
      this.C13R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C13R0_B7.TabIndex = 496;
      this.C13R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C13R0_B7.ThreeState = true;
      this.C13R0_B7.UseVisualStyleBackColor = true;
      this.C13R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R1_B0
      // 
      this.C12R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B0.Location = new System.Drawing.Point(150, 186);
      this.C12R1_B0.Name = "C12R1_B0";
      this.C12R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C12R1_B0.TabIndex = 495;
      this.C12R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B0.ThreeState = true;
      this.C12R1_B0.UseVisualStyleBackColor = true;
      this.C12R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R1_B1
      // 
      this.C12R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B1.Location = new System.Drawing.Point(150, 174);
      this.C12R1_B1.Name = "C12R1_B1";
      this.C12R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C12R1_B1.TabIndex = 494;
      this.C12R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B1.ThreeState = true;
      this.C12R1_B1.UseVisualStyleBackColor = true;
      this.C12R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R1_B2
      // 
      this.C12R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B2.Location = new System.Drawing.Point(150, 162);
      this.C12R1_B2.Name = "C12R1_B2";
      this.C12R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C12R1_B2.TabIndex = 493;
      this.C12R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B2.ThreeState = true;
      this.C12R1_B2.UseVisualStyleBackColor = true;
      this.C12R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R1_B3
      // 
      this.C12R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B3.Location = new System.Drawing.Point(150, 150);
      this.C12R1_B3.Name = "C12R1_B3";
      this.C12R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C12R1_B3.TabIndex = 492;
      this.C12R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B3.ThreeState = true;
      this.C12R1_B3.UseVisualStyleBackColor = true;
      this.C12R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R1_B4
      // 
      this.C12R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B4.Location = new System.Drawing.Point(150, 138);
      this.C12R1_B4.Name = "C12R1_B4";
      this.C12R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C12R1_B4.TabIndex = 491;
      this.C12R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B4.ThreeState = true;
      this.C12R1_B4.UseVisualStyleBackColor = true;
      this.C12R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R1_B5
      // 
      this.C12R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B5.Location = new System.Drawing.Point(150, 126);
      this.C12R1_B5.Name = "C12R1_B5";
      this.C12R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C12R1_B5.TabIndex = 490;
      this.C12R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B5.ThreeState = true;
      this.C12R1_B5.UseVisualStyleBackColor = true;
      this.C12R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R1_B6
      // 
      this.C12R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B6.Location = new System.Drawing.Point(150, 114);
      this.C12R1_B6.Name = "C12R1_B6";
      this.C12R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C12R1_B6.TabIndex = 489;
      this.C12R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B6.ThreeState = true;
      this.C12R1_B6.UseVisualStyleBackColor = true;
      this.C12R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R1_B7
      // 
      this.C12R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B7.Location = new System.Drawing.Point(150, 102);
      this.C12R1_B7.Name = "C12R1_B7";
      this.C12R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C12R1_B7.TabIndex = 488;
      this.C12R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R1_B7.ThreeState = true;
      this.C12R1_B7.UseVisualStyleBackColor = true;
      this.C12R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R0_B0
      // 
      this.C12R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B0.Location = new System.Drawing.Point(150, 90);
      this.C12R0_B0.Name = "C12R0_B0";
      this.C12R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C12R0_B0.TabIndex = 487;
      this.C12R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B0.ThreeState = true;
      this.C12R0_B0.UseVisualStyleBackColor = true;
      this.C12R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R0_B1
      // 
      this.C12R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B1.Location = new System.Drawing.Point(150, 78);
      this.C12R0_B1.Name = "C12R0_B1";
      this.C12R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C12R0_B1.TabIndex = 486;
      this.C12R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B1.ThreeState = true;
      this.C12R0_B1.UseVisualStyleBackColor = true;
      this.C12R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R0_B2
      // 
      this.C12R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B2.Location = new System.Drawing.Point(150, 66);
      this.C12R0_B2.Name = "C12R0_B2";
      this.C12R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C12R0_B2.TabIndex = 485;
      this.C12R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B2.ThreeState = true;
      this.C12R0_B2.UseVisualStyleBackColor = true;
      this.C12R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R0_B3
      // 
      this.C12R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B3.Location = new System.Drawing.Point(150, 54);
      this.C12R0_B3.Name = "C12R0_B3";
      this.C12R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C12R0_B3.TabIndex = 484;
      this.C12R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B3.ThreeState = true;
      this.C12R0_B3.UseVisualStyleBackColor = true;
      this.C12R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R0_B4
      // 
      this.C12R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B4.Location = new System.Drawing.Point(150, 42);
      this.C12R0_B4.Name = "C12R0_B4";
      this.C12R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C12R0_B4.TabIndex = 483;
      this.C12R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B4.ThreeState = true;
      this.C12R0_B4.UseVisualStyleBackColor = true;
      this.C12R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R0_B5
      // 
      this.C12R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B5.Location = new System.Drawing.Point(150, 30);
      this.C12R0_B5.Name = "C12R0_B5";
      this.C12R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C12R0_B5.TabIndex = 482;
      this.C12R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B5.ThreeState = true;
      this.C12R0_B5.UseVisualStyleBackColor = true;
      this.C12R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R0_B6
      // 
      this.C12R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B6.Location = new System.Drawing.Point(150, 18);
      this.C12R0_B6.Name = "C12R0_B6";
      this.C12R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C12R0_B6.TabIndex = 481;
      this.C12R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B6.ThreeState = true;
      this.C12R0_B6.UseVisualStyleBackColor = true;
      this.C12R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C12R0_B7
      // 
      this.C12R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B7.Location = new System.Drawing.Point(150, 6);
      this.C12R0_B7.Name = "C12R0_B7";
      this.C12R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C12R0_B7.TabIndex = 480;
      this.C12R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C12R0_B7.ThreeState = true;
      this.C12R0_B7.UseVisualStyleBackColor = true;
      this.C12R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R1_B0
      // 
      this.C11R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B0.Location = new System.Drawing.Point(138, 186);
      this.C11R1_B0.Name = "C11R1_B0";
      this.C11R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C11R1_B0.TabIndex = 479;
      this.C11R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B0.ThreeState = true;
      this.C11R1_B0.UseVisualStyleBackColor = true;
      this.C11R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R1_B1
      // 
      this.C11R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B1.Location = new System.Drawing.Point(138, 174);
      this.C11R1_B1.Name = "C11R1_B1";
      this.C11R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C11R1_B1.TabIndex = 478;
      this.C11R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B1.ThreeState = true;
      this.C11R1_B1.UseVisualStyleBackColor = true;
      this.C11R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R1_B2
      // 
      this.C11R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B2.Location = new System.Drawing.Point(138, 162);
      this.C11R1_B2.Name = "C11R1_B2";
      this.C11R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C11R1_B2.TabIndex = 477;
      this.C11R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B2.ThreeState = true;
      this.C11R1_B2.UseVisualStyleBackColor = true;
      this.C11R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R1_B3
      // 
      this.C11R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B3.Location = new System.Drawing.Point(138, 150);
      this.C11R1_B3.Name = "C11R1_B3";
      this.C11R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C11R1_B3.TabIndex = 476;
      this.C11R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B3.ThreeState = true;
      this.C11R1_B3.UseVisualStyleBackColor = true;
      this.C11R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R1_B4
      // 
      this.C11R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B4.Location = new System.Drawing.Point(138, 138);
      this.C11R1_B4.Name = "C11R1_B4";
      this.C11R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C11R1_B4.TabIndex = 475;
      this.C11R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B4.ThreeState = true;
      this.C11R1_B4.UseVisualStyleBackColor = true;
      this.C11R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R1_B5
      // 
      this.C11R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B5.Location = new System.Drawing.Point(138, 126);
      this.C11R1_B5.Name = "C11R1_B5";
      this.C11R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C11R1_B5.TabIndex = 474;
      this.C11R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B5.ThreeState = true;
      this.C11R1_B5.UseVisualStyleBackColor = true;
      this.C11R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R1_B6
      // 
      this.C11R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B6.Location = new System.Drawing.Point(138, 114);
      this.C11R1_B6.Name = "C11R1_B6";
      this.C11R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C11R1_B6.TabIndex = 473;
      this.C11R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B6.ThreeState = true;
      this.C11R1_B6.UseVisualStyleBackColor = true;
      this.C11R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R1_B7
      // 
      this.C11R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B7.Location = new System.Drawing.Point(138, 102);
      this.C11R1_B7.Name = "C11R1_B7";
      this.C11R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C11R1_B7.TabIndex = 472;
      this.C11R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R1_B7.ThreeState = true;
      this.C11R1_B7.UseVisualStyleBackColor = true;
      this.C11R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R0_B0
      // 
      this.C11R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B0.Location = new System.Drawing.Point(138, 90);
      this.C11R0_B0.Name = "C11R0_B0";
      this.C11R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C11R0_B0.TabIndex = 471;
      this.C11R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B0.ThreeState = true;
      this.C11R0_B0.UseVisualStyleBackColor = true;
      this.C11R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R0_B1
      // 
      this.C11R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B1.Location = new System.Drawing.Point(138, 78);
      this.C11R0_B1.Name = "C11R0_B1";
      this.C11R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C11R0_B1.TabIndex = 470;
      this.C11R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B1.ThreeState = true;
      this.C11R0_B1.UseVisualStyleBackColor = true;
      this.C11R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R0_B2
      // 
      this.C11R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B2.Location = new System.Drawing.Point(138, 66);
      this.C11R0_B2.Name = "C11R0_B2";
      this.C11R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C11R0_B2.TabIndex = 469;
      this.C11R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B2.ThreeState = true;
      this.C11R0_B2.UseVisualStyleBackColor = true;
      this.C11R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R0_B3
      // 
      this.C11R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B3.Location = new System.Drawing.Point(138, 54);
      this.C11R0_B3.Name = "C11R0_B3";
      this.C11R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C11R0_B3.TabIndex = 468;
      this.C11R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B3.ThreeState = true;
      this.C11R0_B3.UseVisualStyleBackColor = true;
      this.C11R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R0_B4
      // 
      this.C11R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B4.Location = new System.Drawing.Point(138, 42);
      this.C11R0_B4.Name = "C11R0_B4";
      this.C11R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C11R0_B4.TabIndex = 467;
      this.C11R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B4.ThreeState = true;
      this.C11R0_B4.UseVisualStyleBackColor = true;
      this.C11R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R0_B5
      // 
      this.C11R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B5.Location = new System.Drawing.Point(138, 30);
      this.C11R0_B5.Name = "C11R0_B5";
      this.C11R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C11R0_B5.TabIndex = 466;
      this.C11R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B5.ThreeState = true;
      this.C11R0_B5.UseVisualStyleBackColor = true;
      this.C11R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R0_B6
      // 
      this.C11R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B6.Location = new System.Drawing.Point(138, 18);
      this.C11R0_B6.Name = "C11R0_B6";
      this.C11R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C11R0_B6.TabIndex = 465;
      this.C11R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B6.ThreeState = true;
      this.C11R0_B6.UseVisualStyleBackColor = true;
      this.C11R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C11R0_B7
      // 
      this.C11R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B7.Location = new System.Drawing.Point(138, 6);
      this.C11R0_B7.Name = "C11R0_B7";
      this.C11R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C11R0_B7.TabIndex = 464;
      this.C11R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C11R0_B7.ThreeState = true;
      this.C11R0_B7.UseVisualStyleBackColor = true;
      this.C11R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R1_B0
      // 
      this.C10R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B0.Location = new System.Drawing.Point(126, 186);
      this.C10R1_B0.Name = "C10R1_B0";
      this.C10R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C10R1_B0.TabIndex = 463;
      this.C10R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B0.ThreeState = true;
      this.C10R1_B0.UseVisualStyleBackColor = true;
      this.C10R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R1_B1
      // 
      this.C10R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B1.Location = new System.Drawing.Point(126, 174);
      this.C10R1_B1.Name = "C10R1_B1";
      this.C10R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C10R1_B1.TabIndex = 462;
      this.C10R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B1.ThreeState = true;
      this.C10R1_B1.UseVisualStyleBackColor = true;
      this.C10R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R1_B2
      // 
      this.C10R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B2.Location = new System.Drawing.Point(126, 162);
      this.C10R1_B2.Name = "C10R1_B2";
      this.C10R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C10R1_B2.TabIndex = 461;
      this.C10R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B2.ThreeState = true;
      this.C10R1_B2.UseVisualStyleBackColor = true;
      this.C10R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R1_B3
      // 
      this.C10R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B3.Location = new System.Drawing.Point(126, 150);
      this.C10R1_B3.Name = "C10R1_B3";
      this.C10R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C10R1_B3.TabIndex = 460;
      this.C10R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B3.ThreeState = true;
      this.C10R1_B3.UseVisualStyleBackColor = true;
      this.C10R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R1_B4
      // 
      this.C10R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B4.Location = new System.Drawing.Point(126, 138);
      this.C10R1_B4.Name = "C10R1_B4";
      this.C10R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C10R1_B4.TabIndex = 459;
      this.C10R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B4.ThreeState = true;
      this.C10R1_B4.UseVisualStyleBackColor = true;
      this.C10R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R1_B5
      // 
      this.C10R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B5.Location = new System.Drawing.Point(126, 126);
      this.C10R1_B5.Name = "C10R1_B5";
      this.C10R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C10R1_B5.TabIndex = 458;
      this.C10R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B5.ThreeState = true;
      this.C10R1_B5.UseVisualStyleBackColor = true;
      this.C10R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R1_B6
      // 
      this.C10R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B6.Location = new System.Drawing.Point(126, 114);
      this.C10R1_B6.Name = "C10R1_B6";
      this.C10R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C10R1_B6.TabIndex = 457;
      this.C10R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B6.ThreeState = true;
      this.C10R1_B6.UseVisualStyleBackColor = true;
      this.C10R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R1_B7
      // 
      this.C10R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B7.Location = new System.Drawing.Point(126, 102);
      this.C10R1_B7.Name = "C10R1_B7";
      this.C10R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C10R1_B7.TabIndex = 456;
      this.C10R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R1_B7.ThreeState = true;
      this.C10R1_B7.UseVisualStyleBackColor = true;
      this.C10R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R0_B0
      // 
      this.C10R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B0.Location = new System.Drawing.Point(126, 90);
      this.C10R0_B0.Name = "C10R0_B0";
      this.C10R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C10R0_B0.TabIndex = 455;
      this.C10R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B0.ThreeState = true;
      this.C10R0_B0.UseVisualStyleBackColor = true;
      this.C10R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R0_B1
      // 
      this.C10R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B1.Location = new System.Drawing.Point(126, 78);
      this.C10R0_B1.Name = "C10R0_B1";
      this.C10R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C10R0_B1.TabIndex = 454;
      this.C10R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B1.ThreeState = true;
      this.C10R0_B1.UseVisualStyleBackColor = true;
      this.C10R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R0_B2
      // 
      this.C10R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B2.Location = new System.Drawing.Point(126, 66);
      this.C10R0_B2.Name = "C10R0_B2";
      this.C10R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C10R0_B2.TabIndex = 453;
      this.C10R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B2.ThreeState = true;
      this.C10R0_B2.UseVisualStyleBackColor = true;
      this.C10R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R0_B3
      // 
      this.C10R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B3.Location = new System.Drawing.Point(126, 54);
      this.C10R0_B3.Name = "C10R0_B3";
      this.C10R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C10R0_B3.TabIndex = 452;
      this.C10R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B3.ThreeState = true;
      this.C10R0_B3.UseVisualStyleBackColor = true;
      this.C10R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R0_B4
      // 
      this.C10R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B4.Location = new System.Drawing.Point(126, 42);
      this.C10R0_B4.Name = "C10R0_B4";
      this.C10R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C10R0_B4.TabIndex = 451;
      this.C10R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B4.ThreeState = true;
      this.C10R0_B4.UseVisualStyleBackColor = true;
      this.C10R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R0_B5
      // 
      this.C10R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B5.Location = new System.Drawing.Point(126, 30);
      this.C10R0_B5.Name = "C10R0_B5";
      this.C10R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C10R0_B5.TabIndex = 450;
      this.C10R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B5.ThreeState = true;
      this.C10R0_B5.UseVisualStyleBackColor = true;
      this.C10R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R0_B6
      // 
      this.C10R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B6.Location = new System.Drawing.Point(126, 18);
      this.C10R0_B6.Name = "C10R0_B6";
      this.C10R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C10R0_B6.TabIndex = 449;
      this.C10R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B6.ThreeState = true;
      this.C10R0_B6.UseVisualStyleBackColor = true;
      this.C10R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C10R0_B7
      // 
      this.C10R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B7.Location = new System.Drawing.Point(126, 6);
      this.C10R0_B7.Name = "C10R0_B7";
      this.C10R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C10R0_B7.TabIndex = 448;
      this.C10R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C10R0_B7.ThreeState = true;
      this.C10R0_B7.UseVisualStyleBackColor = true;
      this.C10R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R1_B0
      // 
      this.C9R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B0.Location = new System.Drawing.Point(114, 186);
      this.C9R1_B0.Name = "C9R1_B0";
      this.C9R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C9R1_B0.TabIndex = 447;
      this.C9R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B0.ThreeState = true;
      this.C9R1_B0.UseVisualStyleBackColor = true;
      this.C9R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R1_B1
      // 
      this.C9R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B1.Location = new System.Drawing.Point(114, 174);
      this.C9R1_B1.Name = "C9R1_B1";
      this.C9R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C9R1_B1.TabIndex = 446;
      this.C9R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B1.ThreeState = true;
      this.C9R1_B1.UseVisualStyleBackColor = true;
      this.C9R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R1_B2
      // 
      this.C9R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B2.Location = new System.Drawing.Point(114, 162);
      this.C9R1_B2.Name = "C9R1_B2";
      this.C9R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C9R1_B2.TabIndex = 445;
      this.C9R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B2.ThreeState = true;
      this.C9R1_B2.UseVisualStyleBackColor = true;
      this.C9R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R1_B3
      // 
      this.C9R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B3.Location = new System.Drawing.Point(114, 150);
      this.C9R1_B3.Name = "C9R1_B3";
      this.C9R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C9R1_B3.TabIndex = 444;
      this.C9R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B3.ThreeState = true;
      this.C9R1_B3.UseVisualStyleBackColor = true;
      this.C9R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R1_B4
      // 
      this.C9R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B4.Location = new System.Drawing.Point(114, 138);
      this.C9R1_B4.Name = "C9R1_B4";
      this.C9R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C9R1_B4.TabIndex = 443;
      this.C9R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B4.ThreeState = true;
      this.C9R1_B4.UseVisualStyleBackColor = true;
      this.C9R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R1_B5
      // 
      this.C9R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B5.Location = new System.Drawing.Point(114, 126);
      this.C9R1_B5.Name = "C9R1_B5";
      this.C9R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C9R1_B5.TabIndex = 442;
      this.C9R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B5.ThreeState = true;
      this.C9R1_B5.UseVisualStyleBackColor = true;
      this.C9R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R1_B6
      // 
      this.C9R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B6.Location = new System.Drawing.Point(114, 114);
      this.C9R1_B6.Name = "C9R1_B6";
      this.C9R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C9R1_B6.TabIndex = 441;
      this.C9R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B6.ThreeState = true;
      this.C9R1_B6.UseVisualStyleBackColor = true;
      this.C9R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R1_B7
      // 
      this.C9R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B7.Location = new System.Drawing.Point(114, 102);
      this.C9R1_B7.Name = "C9R1_B7";
      this.C9R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C9R1_B7.TabIndex = 440;
      this.C9R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R1_B7.ThreeState = true;
      this.C9R1_B7.UseVisualStyleBackColor = true;
      this.C9R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R0_B0
      // 
      this.C9R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B0.Location = new System.Drawing.Point(114, 90);
      this.C9R0_B0.Name = "C9R0_B0";
      this.C9R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C9R0_B0.TabIndex = 439;
      this.C9R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B0.ThreeState = true;
      this.C9R0_B0.UseVisualStyleBackColor = true;
      this.C9R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R0_B1
      // 
      this.C9R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B1.Location = new System.Drawing.Point(114, 78);
      this.C9R0_B1.Name = "C9R0_B1";
      this.C9R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C9R0_B1.TabIndex = 438;
      this.C9R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B1.ThreeState = true;
      this.C9R0_B1.UseVisualStyleBackColor = true;
      this.C9R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R0_B2
      // 
      this.C9R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B2.Location = new System.Drawing.Point(114, 66);
      this.C9R0_B2.Name = "C9R0_B2";
      this.C9R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C9R0_B2.TabIndex = 437;
      this.C9R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B2.ThreeState = true;
      this.C9R0_B2.UseVisualStyleBackColor = true;
      this.C9R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R0_B3
      // 
      this.C9R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B3.Location = new System.Drawing.Point(114, 54);
      this.C9R0_B3.Name = "C9R0_B3";
      this.C9R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C9R0_B3.TabIndex = 436;
      this.C9R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B3.ThreeState = true;
      this.C9R0_B3.UseVisualStyleBackColor = true;
      this.C9R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R0_B4
      // 
      this.C9R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B4.Location = new System.Drawing.Point(114, 42);
      this.C9R0_B4.Name = "C9R0_B4";
      this.C9R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C9R0_B4.TabIndex = 435;
      this.C9R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B4.ThreeState = true;
      this.C9R0_B4.UseVisualStyleBackColor = true;
      this.C9R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R0_B5
      // 
      this.C9R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B5.Location = new System.Drawing.Point(114, 30);
      this.C9R0_B5.Name = "C9R0_B5";
      this.C9R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C9R0_B5.TabIndex = 434;
      this.C9R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B5.ThreeState = true;
      this.C9R0_B5.UseVisualStyleBackColor = true;
      this.C9R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R0_B6
      // 
      this.C9R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B6.Location = new System.Drawing.Point(114, 18);
      this.C9R0_B6.Name = "C9R0_B6";
      this.C9R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C9R0_B6.TabIndex = 433;
      this.C9R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B6.ThreeState = true;
      this.C9R0_B6.UseVisualStyleBackColor = true;
      this.C9R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C9R0_B7
      // 
      this.C9R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B7.Location = new System.Drawing.Point(114, 6);
      this.C9R0_B7.Name = "C9R0_B7";
      this.C9R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C9R0_B7.TabIndex = 432;
      this.C9R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C9R0_B7.ThreeState = true;
      this.C9R0_B7.UseVisualStyleBackColor = true;
      this.C9R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R1_B0
      // 
      this.C8R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B0.Location = new System.Drawing.Point(102, 186);
      this.C8R1_B0.Name = "C8R1_B0";
      this.C8R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C8R1_B0.TabIndex = 431;
      this.C8R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B0.ThreeState = true;
      this.C8R1_B0.UseVisualStyleBackColor = true;
      this.C8R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R1_B1
      // 
      this.C8R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B1.Location = new System.Drawing.Point(102, 174);
      this.C8R1_B1.Name = "C8R1_B1";
      this.C8R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C8R1_B1.TabIndex = 430;
      this.C8R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B1.ThreeState = true;
      this.C8R1_B1.UseVisualStyleBackColor = true;
      this.C8R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R1_B2
      // 
      this.C8R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B2.Location = new System.Drawing.Point(102, 162);
      this.C8R1_B2.Name = "C8R1_B2";
      this.C8R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C8R1_B2.TabIndex = 429;
      this.C8R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B2.ThreeState = true;
      this.C8R1_B2.UseVisualStyleBackColor = true;
      this.C8R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R1_B3
      // 
      this.C8R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B3.Location = new System.Drawing.Point(102, 150);
      this.C8R1_B3.Name = "C8R1_B3";
      this.C8R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C8R1_B3.TabIndex = 428;
      this.C8R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B3.ThreeState = true;
      this.C8R1_B3.UseVisualStyleBackColor = true;
      this.C8R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R1_B4
      // 
      this.C8R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B4.Location = new System.Drawing.Point(102, 138);
      this.C8R1_B4.Name = "C8R1_B4";
      this.C8R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C8R1_B4.TabIndex = 427;
      this.C8R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B4.ThreeState = true;
      this.C8R1_B4.UseVisualStyleBackColor = true;
      this.C8R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R1_B5
      // 
      this.C8R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B5.Location = new System.Drawing.Point(102, 126);
      this.C8R1_B5.Name = "C8R1_B5";
      this.C8R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C8R1_B5.TabIndex = 426;
      this.C8R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B5.ThreeState = true;
      this.C8R1_B5.UseVisualStyleBackColor = true;
      this.C8R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R1_B6
      // 
      this.C8R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B6.Location = new System.Drawing.Point(102, 114);
      this.C8R1_B6.Name = "C8R1_B6";
      this.C8R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C8R1_B6.TabIndex = 425;
      this.C8R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B6.ThreeState = true;
      this.C8R1_B6.UseVisualStyleBackColor = true;
      this.C8R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R1_B7
      // 
      this.C8R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B7.Location = new System.Drawing.Point(102, 102);
      this.C8R1_B7.Name = "C8R1_B7";
      this.C8R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C8R1_B7.TabIndex = 424;
      this.C8R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R1_B7.ThreeState = true;
      this.C8R1_B7.UseVisualStyleBackColor = true;
      this.C8R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R0_B0
      // 
      this.C8R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B0.Location = new System.Drawing.Point(102, 90);
      this.C8R0_B0.Name = "C8R0_B0";
      this.C8R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C8R0_B0.TabIndex = 423;
      this.C8R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B0.ThreeState = true;
      this.C8R0_B0.UseVisualStyleBackColor = true;
      this.C8R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R0_B1
      // 
      this.C8R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B1.Location = new System.Drawing.Point(102, 78);
      this.C8R0_B1.Name = "C8R0_B1";
      this.C8R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C8R0_B1.TabIndex = 422;
      this.C8R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B1.ThreeState = true;
      this.C8R0_B1.UseVisualStyleBackColor = true;
      this.C8R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R0_B2
      // 
      this.C8R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B2.Location = new System.Drawing.Point(102, 66);
      this.C8R0_B2.Name = "C8R0_B2";
      this.C8R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C8R0_B2.TabIndex = 421;
      this.C8R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B2.ThreeState = true;
      this.C8R0_B2.UseVisualStyleBackColor = true;
      this.C8R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R0_B3
      // 
      this.C8R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B3.Location = new System.Drawing.Point(102, 54);
      this.C8R0_B3.Name = "C8R0_B3";
      this.C8R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C8R0_B3.TabIndex = 420;
      this.C8R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B3.ThreeState = true;
      this.C8R0_B3.UseVisualStyleBackColor = true;
      this.C8R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R0_B4
      // 
      this.C8R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B4.Location = new System.Drawing.Point(102, 42);
      this.C8R0_B4.Name = "C8R0_B4";
      this.C8R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C8R0_B4.TabIndex = 419;
      this.C8R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B4.ThreeState = true;
      this.C8R0_B4.UseVisualStyleBackColor = true;
      this.C8R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R0_B5
      // 
      this.C8R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B5.Location = new System.Drawing.Point(102, 30);
      this.C8R0_B5.Name = "C8R0_B5";
      this.C8R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C8R0_B5.TabIndex = 418;
      this.C8R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B5.ThreeState = true;
      this.C8R0_B5.UseVisualStyleBackColor = true;
      this.C8R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R0_B6
      // 
      this.C8R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B6.Location = new System.Drawing.Point(102, 18);
      this.C8R0_B6.Name = "C8R0_B6";
      this.C8R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C8R0_B6.TabIndex = 417;
      this.C8R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B6.ThreeState = true;
      this.C8R0_B6.UseVisualStyleBackColor = true;
      this.C8R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C8R0_B7
      // 
      this.C8R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B7.Location = new System.Drawing.Point(102, 6);
      this.C8R0_B7.Name = "C8R0_B7";
      this.C8R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C8R0_B7.TabIndex = 416;
      this.C8R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C8R0_B7.ThreeState = true;
      this.C8R0_B7.UseVisualStyleBackColor = true;
      this.C8R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R1_B0
      // 
      this.C7R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B0.Location = new System.Drawing.Point(90, 186);
      this.C7R1_B0.Name = "C7R1_B0";
      this.C7R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C7R1_B0.TabIndex = 415;
      this.C7R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B0.ThreeState = true;
      this.C7R1_B0.UseVisualStyleBackColor = true;
      this.C7R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R1_B1
      // 
      this.C7R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B1.Location = new System.Drawing.Point(90, 174);
      this.C7R1_B1.Name = "C7R1_B1";
      this.C7R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C7R1_B1.TabIndex = 414;
      this.C7R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B1.ThreeState = true;
      this.C7R1_B1.UseVisualStyleBackColor = true;
      this.C7R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R1_B2
      // 
      this.C7R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B2.Location = new System.Drawing.Point(90, 162);
      this.C7R1_B2.Name = "C7R1_B2";
      this.C7R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C7R1_B2.TabIndex = 413;
      this.C7R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B2.ThreeState = true;
      this.C7R1_B2.UseVisualStyleBackColor = true;
      this.C7R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R1_B3
      // 
      this.C7R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B3.Location = new System.Drawing.Point(90, 150);
      this.C7R1_B3.Name = "C7R1_B3";
      this.C7R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C7R1_B3.TabIndex = 412;
      this.C7R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B3.ThreeState = true;
      this.C7R1_B3.UseVisualStyleBackColor = true;
      this.C7R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R1_B4
      // 
      this.C7R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B4.Location = new System.Drawing.Point(90, 138);
      this.C7R1_B4.Name = "C7R1_B4";
      this.C7R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C7R1_B4.TabIndex = 411;
      this.C7R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B4.ThreeState = true;
      this.C7R1_B4.UseVisualStyleBackColor = true;
      this.C7R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R1_B5
      // 
      this.C7R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B5.Location = new System.Drawing.Point(90, 126);
      this.C7R1_B5.Name = "C7R1_B5";
      this.C7R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C7R1_B5.TabIndex = 410;
      this.C7R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B5.ThreeState = true;
      this.C7R1_B5.UseVisualStyleBackColor = true;
      this.C7R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R1_B6
      // 
      this.C7R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B6.Location = new System.Drawing.Point(90, 114);
      this.C7R1_B6.Name = "C7R1_B6";
      this.C7R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C7R1_B6.TabIndex = 409;
      this.C7R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B6.ThreeState = true;
      this.C7R1_B6.UseVisualStyleBackColor = true;
      this.C7R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R1_B7
      // 
      this.C7R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B7.Location = new System.Drawing.Point(90, 102);
      this.C7R1_B7.Name = "C7R1_B7";
      this.C7R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C7R1_B7.TabIndex = 408;
      this.C7R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R1_B7.ThreeState = true;
      this.C7R1_B7.UseVisualStyleBackColor = true;
      this.C7R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R0_B0
      // 
      this.C7R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B0.Location = new System.Drawing.Point(90, 90);
      this.C7R0_B0.Name = "C7R0_B0";
      this.C7R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C7R0_B0.TabIndex = 407;
      this.C7R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B0.ThreeState = true;
      this.C7R0_B0.UseVisualStyleBackColor = true;
      this.C7R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R0_B1
      // 
      this.C7R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B1.Location = new System.Drawing.Point(90, 78);
      this.C7R0_B1.Name = "C7R0_B1";
      this.C7R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C7R0_B1.TabIndex = 406;
      this.C7R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B1.ThreeState = true;
      this.C7R0_B1.UseVisualStyleBackColor = true;
      this.C7R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R0_B2
      // 
      this.C7R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B2.Location = new System.Drawing.Point(90, 66);
      this.C7R0_B2.Name = "C7R0_B2";
      this.C7R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C7R0_B2.TabIndex = 405;
      this.C7R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B2.ThreeState = true;
      this.C7R0_B2.UseVisualStyleBackColor = true;
      this.C7R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R0_B3
      // 
      this.C7R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B3.Location = new System.Drawing.Point(90, 54);
      this.C7R0_B3.Name = "C7R0_B3";
      this.C7R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C7R0_B3.TabIndex = 404;
      this.C7R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B3.ThreeState = true;
      this.C7R0_B3.UseVisualStyleBackColor = true;
      this.C7R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R0_B4
      // 
      this.C7R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B4.Location = new System.Drawing.Point(90, 42);
      this.C7R0_B4.Name = "C7R0_B4";
      this.C7R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C7R0_B4.TabIndex = 403;
      this.C7R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B4.ThreeState = true;
      this.C7R0_B4.UseVisualStyleBackColor = true;
      this.C7R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R0_B5
      // 
      this.C7R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B5.Location = new System.Drawing.Point(90, 30);
      this.C7R0_B5.Name = "C7R0_B5";
      this.C7R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C7R0_B5.TabIndex = 402;
      this.C7R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B5.ThreeState = true;
      this.C7R0_B5.UseVisualStyleBackColor = true;
      this.C7R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R0_B6
      // 
      this.C7R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B6.Location = new System.Drawing.Point(90, 18);
      this.C7R0_B6.Name = "C7R0_B6";
      this.C7R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C7R0_B6.TabIndex = 401;
      this.C7R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B6.ThreeState = true;
      this.C7R0_B6.UseVisualStyleBackColor = true;
      this.C7R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C7R0_B7
      // 
      this.C7R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B7.Location = new System.Drawing.Point(90, 6);
      this.C7R0_B7.Name = "C7R0_B7";
      this.C7R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C7R0_B7.TabIndex = 400;
      this.C7R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C7R0_B7.ThreeState = true;
      this.C7R0_B7.UseVisualStyleBackColor = true;
      this.C7R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R1_B0
      // 
      this.C6R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B0.Location = new System.Drawing.Point(78, 186);
      this.C6R1_B0.Name = "C6R1_B0";
      this.C6R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C6R1_B0.TabIndex = 399;
      this.C6R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B0.ThreeState = true;
      this.C6R1_B0.UseVisualStyleBackColor = true;
      this.C6R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R1_B1
      // 
      this.C6R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B1.Location = new System.Drawing.Point(78, 174);
      this.C6R1_B1.Name = "C6R1_B1";
      this.C6R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C6R1_B1.TabIndex = 398;
      this.C6R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B1.ThreeState = true;
      this.C6R1_B1.UseVisualStyleBackColor = true;
      this.C6R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R1_B2
      // 
      this.C6R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B2.Location = new System.Drawing.Point(78, 162);
      this.C6R1_B2.Name = "C6R1_B2";
      this.C6R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C6R1_B2.TabIndex = 397;
      this.C6R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B2.ThreeState = true;
      this.C6R1_B2.UseVisualStyleBackColor = true;
      this.C6R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R1_B3
      // 
      this.C6R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B3.Location = new System.Drawing.Point(78, 150);
      this.C6R1_B3.Name = "C6R1_B3";
      this.C6R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C6R1_B3.TabIndex = 396;
      this.C6R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B3.ThreeState = true;
      this.C6R1_B3.UseVisualStyleBackColor = true;
      this.C6R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R1_B4
      // 
      this.C6R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B4.Location = new System.Drawing.Point(78, 138);
      this.C6R1_B4.Name = "C6R1_B4";
      this.C6R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C6R1_B4.TabIndex = 395;
      this.C6R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B4.ThreeState = true;
      this.C6R1_B4.UseVisualStyleBackColor = true;
      this.C6R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R1_B5
      // 
      this.C6R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B5.Location = new System.Drawing.Point(78, 126);
      this.C6R1_B5.Name = "C6R1_B5";
      this.C6R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C6R1_B5.TabIndex = 394;
      this.C6R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B5.ThreeState = true;
      this.C6R1_B5.UseVisualStyleBackColor = true;
      this.C6R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R1_B6
      // 
      this.C6R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B6.Location = new System.Drawing.Point(78, 114);
      this.C6R1_B6.Name = "C6R1_B6";
      this.C6R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C6R1_B6.TabIndex = 393;
      this.C6R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B6.ThreeState = true;
      this.C6R1_B6.UseVisualStyleBackColor = true;
      this.C6R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R1_B7
      // 
      this.C6R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B7.Location = new System.Drawing.Point(78, 102);
      this.C6R1_B7.Name = "C6R1_B7";
      this.C6R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C6R1_B7.TabIndex = 392;
      this.C6R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R1_B7.ThreeState = true;
      this.C6R1_B7.UseVisualStyleBackColor = true;
      this.C6R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R0_B0
      // 
      this.C6R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B0.Location = new System.Drawing.Point(78, 90);
      this.C6R0_B0.Name = "C6R0_B0";
      this.C6R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C6R0_B0.TabIndex = 391;
      this.C6R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B0.ThreeState = true;
      this.C6R0_B0.UseVisualStyleBackColor = true;
      this.C6R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R0_B1
      // 
      this.C6R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B1.Location = new System.Drawing.Point(78, 78);
      this.C6R0_B1.Name = "C6R0_B1";
      this.C6R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C6R0_B1.TabIndex = 390;
      this.C6R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B1.ThreeState = true;
      this.C6R0_B1.UseVisualStyleBackColor = true;
      this.C6R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R0_B2
      // 
      this.C6R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B2.Location = new System.Drawing.Point(78, 66);
      this.C6R0_B2.Name = "C6R0_B2";
      this.C6R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C6R0_B2.TabIndex = 389;
      this.C6R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B2.ThreeState = true;
      this.C6R0_B2.UseVisualStyleBackColor = true;
      this.C6R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R0_B3
      // 
      this.C6R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B3.Location = new System.Drawing.Point(78, 54);
      this.C6R0_B3.Name = "C6R0_B3";
      this.C6R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C6R0_B3.TabIndex = 388;
      this.C6R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B3.ThreeState = true;
      this.C6R0_B3.UseVisualStyleBackColor = true;
      this.C6R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R0_B4
      // 
      this.C6R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B4.Location = new System.Drawing.Point(78, 42);
      this.C6R0_B4.Name = "C6R0_B4";
      this.C6R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C6R0_B4.TabIndex = 387;
      this.C6R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B4.ThreeState = true;
      this.C6R0_B4.UseVisualStyleBackColor = true;
      this.C6R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R0_B5
      // 
      this.C6R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B5.Location = new System.Drawing.Point(78, 30);
      this.C6R0_B5.Name = "C6R0_B5";
      this.C6R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C6R0_B5.TabIndex = 386;
      this.C6R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B5.ThreeState = true;
      this.C6R0_B5.UseVisualStyleBackColor = true;
      this.C6R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R0_B6
      // 
      this.C6R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B6.Location = new System.Drawing.Point(78, 18);
      this.C6R0_B6.Name = "C6R0_B6";
      this.C6R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C6R0_B6.TabIndex = 385;
      this.C6R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B6.ThreeState = true;
      this.C6R0_B6.UseVisualStyleBackColor = true;
      this.C6R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C6R0_B7
      // 
      this.C6R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B7.Location = new System.Drawing.Point(78, 6);
      this.C6R0_B7.Name = "C6R0_B7";
      this.C6R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C6R0_B7.TabIndex = 384;
      this.C6R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C6R0_B7.ThreeState = true;
      this.C6R0_B7.UseVisualStyleBackColor = true;
      this.C6R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R1_B0
      // 
      this.C5R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B0.Location = new System.Drawing.Point(66, 186);
      this.C5R1_B0.Name = "C5R1_B0";
      this.C5R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C5R1_B0.TabIndex = 383;
      this.C5R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B0.ThreeState = true;
      this.C5R1_B0.UseVisualStyleBackColor = true;
      this.C5R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R1_B1
      // 
      this.C5R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B1.Location = new System.Drawing.Point(66, 174);
      this.C5R1_B1.Name = "C5R1_B1";
      this.C5R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C5R1_B1.TabIndex = 382;
      this.C5R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B1.ThreeState = true;
      this.C5R1_B1.UseVisualStyleBackColor = true;
      this.C5R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R1_B2
      // 
      this.C5R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B2.Location = new System.Drawing.Point(66, 162);
      this.C5R1_B2.Name = "C5R1_B2";
      this.C5R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C5R1_B2.TabIndex = 381;
      this.C5R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B2.ThreeState = true;
      this.C5R1_B2.UseVisualStyleBackColor = true;
      this.C5R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R1_B3
      // 
      this.C5R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B3.Location = new System.Drawing.Point(66, 150);
      this.C5R1_B3.Name = "C5R1_B3";
      this.C5R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C5R1_B3.TabIndex = 380;
      this.C5R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B3.ThreeState = true;
      this.C5R1_B3.UseVisualStyleBackColor = true;
      this.C5R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R1_B4
      // 
      this.C5R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B4.Location = new System.Drawing.Point(66, 138);
      this.C5R1_B4.Name = "C5R1_B4";
      this.C5R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C5R1_B4.TabIndex = 379;
      this.C5R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B4.ThreeState = true;
      this.C5R1_B4.UseVisualStyleBackColor = true;
      this.C5R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R1_B5
      // 
      this.C5R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B5.Location = new System.Drawing.Point(66, 126);
      this.C5R1_B5.Name = "C5R1_B5";
      this.C5R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C5R1_B5.TabIndex = 378;
      this.C5R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B5.ThreeState = true;
      this.C5R1_B5.UseVisualStyleBackColor = true;
      this.C5R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R1_B6
      // 
      this.C5R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B6.Location = new System.Drawing.Point(66, 114);
      this.C5R1_B6.Name = "C5R1_B6";
      this.C5R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C5R1_B6.TabIndex = 377;
      this.C5R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B6.ThreeState = true;
      this.C5R1_B6.UseVisualStyleBackColor = true;
      this.C5R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R1_B7
      // 
      this.C5R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B7.Location = new System.Drawing.Point(66, 102);
      this.C5R1_B7.Name = "C5R1_B7";
      this.C5R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C5R1_B7.TabIndex = 376;
      this.C5R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R1_B7.ThreeState = true;
      this.C5R1_B7.UseVisualStyleBackColor = true;
      this.C5R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R0_B0
      // 
      this.C5R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B0.Location = new System.Drawing.Point(66, 90);
      this.C5R0_B0.Name = "C5R0_B0";
      this.C5R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C5R0_B0.TabIndex = 375;
      this.C5R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B0.ThreeState = true;
      this.C5R0_B0.UseVisualStyleBackColor = true;
      this.C5R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R0_B1
      // 
      this.C5R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B1.Location = new System.Drawing.Point(66, 78);
      this.C5R0_B1.Name = "C5R0_B1";
      this.C5R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C5R0_B1.TabIndex = 374;
      this.C5R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B1.ThreeState = true;
      this.C5R0_B1.UseVisualStyleBackColor = true;
      this.C5R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R0_B2
      // 
      this.C5R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B2.Location = new System.Drawing.Point(66, 66);
      this.C5R0_B2.Name = "C5R0_B2";
      this.C5R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C5R0_B2.TabIndex = 373;
      this.C5R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B2.ThreeState = true;
      this.C5R0_B2.UseVisualStyleBackColor = true;
      this.C5R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R0_B3
      // 
      this.C5R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B3.Location = new System.Drawing.Point(66, 54);
      this.C5R0_B3.Name = "C5R0_B3";
      this.C5R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C5R0_B3.TabIndex = 372;
      this.C5R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B3.ThreeState = true;
      this.C5R0_B3.UseVisualStyleBackColor = true;
      this.C5R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R0_B4
      // 
      this.C5R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B4.Location = new System.Drawing.Point(66, 42);
      this.C5R0_B4.Name = "C5R0_B4";
      this.C5R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C5R0_B4.TabIndex = 371;
      this.C5R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B4.ThreeState = true;
      this.C5R0_B4.UseVisualStyleBackColor = true;
      this.C5R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R0_B5
      // 
      this.C5R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B5.Location = new System.Drawing.Point(66, 30);
      this.C5R0_B5.Name = "C5R0_B5";
      this.C5R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C5R0_B5.TabIndex = 370;
      this.C5R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B5.ThreeState = true;
      this.C5R0_B5.UseVisualStyleBackColor = true;
      this.C5R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R0_B6
      // 
      this.C5R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B6.Location = new System.Drawing.Point(66, 18);
      this.C5R0_B6.Name = "C5R0_B6";
      this.C5R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C5R0_B6.TabIndex = 369;
      this.C5R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B6.ThreeState = true;
      this.C5R0_B6.UseVisualStyleBackColor = true;
      this.C5R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C5R0_B7
      // 
      this.C5R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B7.Location = new System.Drawing.Point(66, 6);
      this.C5R0_B7.Name = "C5R0_B7";
      this.C5R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C5R0_B7.TabIndex = 368;
      this.C5R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C5R0_B7.ThreeState = true;
      this.C5R0_B7.UseVisualStyleBackColor = true;
      this.C5R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R1_B0
      // 
      this.C4R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B0.Location = new System.Drawing.Point(54, 186);
      this.C4R1_B0.Name = "C4R1_B0";
      this.C4R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C4R1_B0.TabIndex = 367;
      this.C4R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B0.ThreeState = true;
      this.C4R1_B0.UseVisualStyleBackColor = true;
      this.C4R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R1_B1
      // 
      this.C4R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B1.Location = new System.Drawing.Point(54, 174);
      this.C4R1_B1.Name = "C4R1_B1";
      this.C4R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C4R1_B1.TabIndex = 366;
      this.C4R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B1.ThreeState = true;
      this.C4R1_B1.UseVisualStyleBackColor = true;
      this.C4R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R1_B2
      // 
      this.C4R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B2.Location = new System.Drawing.Point(54, 162);
      this.C4R1_B2.Name = "C4R1_B2";
      this.C4R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C4R1_B2.TabIndex = 365;
      this.C4R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B2.ThreeState = true;
      this.C4R1_B2.UseVisualStyleBackColor = true;
      this.C4R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R1_B3
      // 
      this.C4R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B3.Location = new System.Drawing.Point(54, 150);
      this.C4R1_B3.Name = "C4R1_B3";
      this.C4R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C4R1_B3.TabIndex = 364;
      this.C4R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B3.ThreeState = true;
      this.C4R1_B3.UseVisualStyleBackColor = true;
      this.C4R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R1_B4
      // 
      this.C4R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B4.Location = new System.Drawing.Point(54, 138);
      this.C4R1_B4.Name = "C4R1_B4";
      this.C4R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C4R1_B4.TabIndex = 363;
      this.C4R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B4.ThreeState = true;
      this.C4R1_B4.UseVisualStyleBackColor = true;
      this.C4R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R1_B5
      // 
      this.C4R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B5.Location = new System.Drawing.Point(54, 126);
      this.C4R1_B5.Name = "C4R1_B5";
      this.C4R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C4R1_B5.TabIndex = 362;
      this.C4R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B5.ThreeState = true;
      this.C4R1_B5.UseVisualStyleBackColor = true;
      this.C4R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R1_B6
      // 
      this.C4R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B6.Location = new System.Drawing.Point(54, 114);
      this.C4R1_B6.Name = "C4R1_B6";
      this.C4R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C4R1_B6.TabIndex = 361;
      this.C4R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B6.ThreeState = true;
      this.C4R1_B6.UseVisualStyleBackColor = true;
      this.C4R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R1_B7
      // 
      this.C4R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B7.Location = new System.Drawing.Point(54, 102);
      this.C4R1_B7.Name = "C4R1_B7";
      this.C4R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C4R1_B7.TabIndex = 360;
      this.C4R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R1_B7.ThreeState = true;
      this.C4R1_B7.UseVisualStyleBackColor = true;
      this.C4R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R0_B0
      // 
      this.C4R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B0.Location = new System.Drawing.Point(54, 90);
      this.C4R0_B0.Name = "C4R0_B0";
      this.C4R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C4R0_B0.TabIndex = 359;
      this.C4R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B0.ThreeState = true;
      this.C4R0_B0.UseVisualStyleBackColor = true;
      this.C4R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R0_B1
      // 
      this.C4R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B1.Location = new System.Drawing.Point(54, 78);
      this.C4R0_B1.Name = "C4R0_B1";
      this.C4R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C4R0_B1.TabIndex = 358;
      this.C4R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B1.ThreeState = true;
      this.C4R0_B1.UseVisualStyleBackColor = true;
      this.C4R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R0_B2
      // 
      this.C4R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B2.Location = new System.Drawing.Point(54, 66);
      this.C4R0_B2.Name = "C4R0_B2";
      this.C4R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C4R0_B2.TabIndex = 357;
      this.C4R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B2.ThreeState = true;
      this.C4R0_B2.UseVisualStyleBackColor = true;
      this.C4R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R0_B3
      // 
      this.C4R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B3.Location = new System.Drawing.Point(54, 54);
      this.C4R0_B3.Name = "C4R0_B3";
      this.C4R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C4R0_B3.TabIndex = 356;
      this.C4R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B3.ThreeState = true;
      this.C4R0_B3.UseVisualStyleBackColor = true;
      this.C4R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R0_B4
      // 
      this.C4R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B4.Location = new System.Drawing.Point(54, 42);
      this.C4R0_B4.Name = "C4R0_B4";
      this.C4R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C4R0_B4.TabIndex = 355;
      this.C4R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B4.ThreeState = true;
      this.C4R0_B4.UseVisualStyleBackColor = true;
      this.C4R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R0_B5
      // 
      this.C4R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B5.Location = new System.Drawing.Point(54, 30);
      this.C4R0_B5.Name = "C4R0_B5";
      this.C4R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C4R0_B5.TabIndex = 354;
      this.C4R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B5.ThreeState = true;
      this.C4R0_B5.UseVisualStyleBackColor = true;
      this.C4R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R0_B6
      // 
      this.C4R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B6.Location = new System.Drawing.Point(54, 18);
      this.C4R0_B6.Name = "C4R0_B6";
      this.C4R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C4R0_B6.TabIndex = 353;
      this.C4R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B6.ThreeState = true;
      this.C4R0_B6.UseVisualStyleBackColor = true;
      this.C4R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C4R0_B7
      // 
      this.C4R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B7.Location = new System.Drawing.Point(54, 6);
      this.C4R0_B7.Name = "C4R0_B7";
      this.C4R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C4R0_B7.TabIndex = 352;
      this.C4R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C4R0_B7.ThreeState = true;
      this.C4R0_B7.UseVisualStyleBackColor = true;
      this.C4R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R1_B0
      // 
      this.C3R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B0.Location = new System.Drawing.Point(42, 186);
      this.C3R1_B0.Name = "C3R1_B0";
      this.C3R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C3R1_B0.TabIndex = 351;
      this.C3R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B0.ThreeState = true;
      this.C3R1_B0.UseVisualStyleBackColor = true;
      this.C3R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R1_B1
      // 
      this.C3R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B1.Location = new System.Drawing.Point(42, 174);
      this.C3R1_B1.Name = "C3R1_B1";
      this.C3R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C3R1_B1.TabIndex = 350;
      this.C3R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B1.ThreeState = true;
      this.C3R1_B1.UseVisualStyleBackColor = true;
      this.C3R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R1_B2
      // 
      this.C3R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B2.Location = new System.Drawing.Point(42, 162);
      this.C3R1_B2.Name = "C3R1_B2";
      this.C3R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C3R1_B2.TabIndex = 349;
      this.C3R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B2.ThreeState = true;
      this.C3R1_B2.UseVisualStyleBackColor = true;
      this.C3R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R1_B3
      // 
      this.C3R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B3.Location = new System.Drawing.Point(42, 150);
      this.C3R1_B3.Name = "C3R1_B3";
      this.C3R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C3R1_B3.TabIndex = 348;
      this.C3R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B3.ThreeState = true;
      this.C3R1_B3.UseVisualStyleBackColor = true;
      this.C3R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R1_B4
      // 
      this.C3R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B4.Location = new System.Drawing.Point(42, 138);
      this.C3R1_B4.Name = "C3R1_B4";
      this.C3R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C3R1_B4.TabIndex = 347;
      this.C3R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B4.ThreeState = true;
      this.C3R1_B4.UseVisualStyleBackColor = true;
      this.C3R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R1_B5
      // 
      this.C3R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B5.Location = new System.Drawing.Point(42, 126);
      this.C3R1_B5.Name = "C3R1_B5";
      this.C3R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C3R1_B5.TabIndex = 346;
      this.C3R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B5.ThreeState = true;
      this.C3R1_B5.UseVisualStyleBackColor = true;
      this.C3R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R1_B6
      // 
      this.C3R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B6.Location = new System.Drawing.Point(42, 114);
      this.C3R1_B6.Name = "C3R1_B6";
      this.C3R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C3R1_B6.TabIndex = 345;
      this.C3R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B6.ThreeState = true;
      this.C3R1_B6.UseVisualStyleBackColor = true;
      this.C3R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R1_B7
      // 
      this.C3R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B7.Location = new System.Drawing.Point(42, 102);
      this.C3R1_B7.Name = "C3R1_B7";
      this.C3R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C3R1_B7.TabIndex = 344;
      this.C3R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R1_B7.ThreeState = true;
      this.C3R1_B7.UseVisualStyleBackColor = true;
      this.C3R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R0_B0
      // 
      this.C3R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B0.Location = new System.Drawing.Point(42, 90);
      this.C3R0_B0.Name = "C3R0_B0";
      this.C3R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C3R0_B0.TabIndex = 343;
      this.C3R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B0.ThreeState = true;
      this.C3R0_B0.UseVisualStyleBackColor = true;
      this.C3R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R0_B1
      // 
      this.C3R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B1.Location = new System.Drawing.Point(42, 78);
      this.C3R0_B1.Name = "C3R0_B1";
      this.C3R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C3R0_B1.TabIndex = 342;
      this.C3R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B1.ThreeState = true;
      this.C3R0_B1.UseVisualStyleBackColor = true;
      this.C3R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R0_B2
      // 
      this.C3R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B2.Location = new System.Drawing.Point(42, 66);
      this.C3R0_B2.Name = "C3R0_B2";
      this.C3R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C3R0_B2.TabIndex = 341;
      this.C3R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B2.ThreeState = true;
      this.C3R0_B2.UseVisualStyleBackColor = true;
      this.C3R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R0_B3
      // 
      this.C3R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B3.Location = new System.Drawing.Point(42, 54);
      this.C3R0_B3.Name = "C3R0_B3";
      this.C3R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C3R0_B3.TabIndex = 340;
      this.C3R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B3.ThreeState = true;
      this.C3R0_B3.UseVisualStyleBackColor = true;
      this.C3R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R0_B4
      // 
      this.C3R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B4.Location = new System.Drawing.Point(42, 42);
      this.C3R0_B4.Name = "C3R0_B4";
      this.C3R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C3R0_B4.TabIndex = 339;
      this.C3R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B4.ThreeState = true;
      this.C3R0_B4.UseVisualStyleBackColor = true;
      this.C3R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R0_B5
      // 
      this.C3R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B5.Location = new System.Drawing.Point(42, 30);
      this.C3R0_B5.Name = "C3R0_B5";
      this.C3R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C3R0_B5.TabIndex = 338;
      this.C3R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B5.ThreeState = true;
      this.C3R0_B5.UseVisualStyleBackColor = true;
      this.C3R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R0_B6
      // 
      this.C3R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B6.Location = new System.Drawing.Point(42, 18);
      this.C3R0_B6.Name = "C3R0_B6";
      this.C3R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C3R0_B6.TabIndex = 337;
      this.C3R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B6.ThreeState = true;
      this.C3R0_B6.UseVisualStyleBackColor = true;
      this.C3R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C3R0_B7
      // 
      this.C3R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B7.Location = new System.Drawing.Point(42, 6);
      this.C3R0_B7.Name = "C3R0_B7";
      this.C3R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C3R0_B7.TabIndex = 336;
      this.C3R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C3R0_B7.ThreeState = true;
      this.C3R0_B7.UseVisualStyleBackColor = true;
      this.C3R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R1_B0
      // 
      this.C2R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B0.Location = new System.Drawing.Point(30, 186);
      this.C2R1_B0.Name = "C2R1_B0";
      this.C2R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C2R1_B0.TabIndex = 335;
      this.C2R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B0.ThreeState = true;
      this.C2R1_B0.UseVisualStyleBackColor = true;
      this.C2R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R1_B1
      // 
      this.C2R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B1.Location = new System.Drawing.Point(30, 174);
      this.C2R1_B1.Name = "C2R1_B1";
      this.C2R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C2R1_B1.TabIndex = 334;
      this.C2R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B1.ThreeState = true;
      this.C2R1_B1.UseVisualStyleBackColor = true;
      this.C2R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R1_B2
      // 
      this.C2R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B2.Location = new System.Drawing.Point(30, 162);
      this.C2R1_B2.Name = "C2R1_B2";
      this.C2R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C2R1_B2.TabIndex = 333;
      this.C2R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B2.ThreeState = true;
      this.C2R1_B2.UseVisualStyleBackColor = true;
      this.C2R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R1_B3
      // 
      this.C2R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B3.Location = new System.Drawing.Point(30, 150);
      this.C2R1_B3.Name = "C2R1_B3";
      this.C2R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C2R1_B3.TabIndex = 332;
      this.C2R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B3.ThreeState = true;
      this.C2R1_B3.UseVisualStyleBackColor = true;
      this.C2R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R1_B4
      // 
      this.C2R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B4.Location = new System.Drawing.Point(30, 138);
      this.C2R1_B4.Name = "C2R1_B4";
      this.C2R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C2R1_B4.TabIndex = 331;
      this.C2R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B4.ThreeState = true;
      this.C2R1_B4.UseVisualStyleBackColor = true;
      this.C2R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R1_B5
      // 
      this.C2R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B5.Location = new System.Drawing.Point(30, 126);
      this.C2R1_B5.Name = "C2R1_B5";
      this.C2R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C2R1_B5.TabIndex = 330;
      this.C2R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B5.ThreeState = true;
      this.C2R1_B5.UseVisualStyleBackColor = true;
      this.C2R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R1_B6
      // 
      this.C2R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B6.Location = new System.Drawing.Point(30, 114);
      this.C2R1_B6.Name = "C2R1_B6";
      this.C2R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C2R1_B6.TabIndex = 329;
      this.C2R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B6.ThreeState = true;
      this.C2R1_B6.UseVisualStyleBackColor = true;
      this.C2R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R1_B7
      // 
      this.C2R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B7.Location = new System.Drawing.Point(30, 102);
      this.C2R1_B7.Name = "C2R1_B7";
      this.C2R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C2R1_B7.TabIndex = 328;
      this.C2R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R1_B7.ThreeState = true;
      this.C2R1_B7.UseVisualStyleBackColor = true;
      this.C2R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R0_B0
      // 
      this.C2R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B0.Location = new System.Drawing.Point(30, 90);
      this.C2R0_B0.Name = "C2R0_B0";
      this.C2R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C2R0_B0.TabIndex = 327;
      this.C2R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B0.ThreeState = true;
      this.C2R0_B0.UseVisualStyleBackColor = true;
      this.C2R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R0_B1
      // 
      this.C2R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B1.Location = new System.Drawing.Point(30, 78);
      this.C2R0_B1.Name = "C2R0_B1";
      this.C2R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C2R0_B1.TabIndex = 326;
      this.C2R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B1.ThreeState = true;
      this.C2R0_B1.UseVisualStyleBackColor = true;
      this.C2R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R0_B2
      // 
      this.C2R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B2.Location = new System.Drawing.Point(30, 66);
      this.C2R0_B2.Name = "C2R0_B2";
      this.C2R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C2R0_B2.TabIndex = 325;
      this.C2R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B2.ThreeState = true;
      this.C2R0_B2.UseVisualStyleBackColor = true;
      this.C2R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R0_B3
      // 
      this.C2R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B3.Location = new System.Drawing.Point(30, 54);
      this.C2R0_B3.Name = "C2R0_B3";
      this.C2R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C2R0_B3.TabIndex = 324;
      this.C2R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B3.ThreeState = true;
      this.C2R0_B3.UseVisualStyleBackColor = true;
      this.C2R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R0_B4
      // 
      this.C2R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B4.Location = new System.Drawing.Point(30, 42);
      this.C2R0_B4.Name = "C2R0_B4";
      this.C2R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C2R0_B4.TabIndex = 323;
      this.C2R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B4.ThreeState = true;
      this.C2R0_B4.UseVisualStyleBackColor = true;
      this.C2R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R0_B5
      // 
      this.C2R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B5.Location = new System.Drawing.Point(30, 30);
      this.C2R0_B5.Name = "C2R0_B5";
      this.C2R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C2R0_B5.TabIndex = 322;
      this.C2R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B5.ThreeState = true;
      this.C2R0_B5.UseVisualStyleBackColor = true;
      this.C2R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R0_B6
      // 
      this.C2R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B6.Location = new System.Drawing.Point(30, 18);
      this.C2R0_B6.Name = "C2R0_B6";
      this.C2R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C2R0_B6.TabIndex = 321;
      this.C2R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B6.ThreeState = true;
      this.C2R0_B6.UseVisualStyleBackColor = true;
      this.C2R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C2R0_B7
      // 
      this.C2R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B7.Location = new System.Drawing.Point(30, 6);
      this.C2R0_B7.Name = "C2R0_B7";
      this.C2R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C2R0_B7.TabIndex = 320;
      this.C2R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C2R0_B7.ThreeState = true;
      this.C2R0_B7.UseVisualStyleBackColor = true;
      this.C2R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R1_B0
      // 
      this.C1R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B0.Location = new System.Drawing.Point(18, 186);
      this.C1R1_B0.Name = "C1R1_B0";
      this.C1R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C1R1_B0.TabIndex = 319;
      this.C1R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B0.ThreeState = true;
      this.C1R1_B0.UseVisualStyleBackColor = true;
      this.C1R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R1_B1
      // 
      this.C1R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B1.Location = new System.Drawing.Point(18, 174);
      this.C1R1_B1.Name = "C1R1_B1";
      this.C1R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C1R1_B1.TabIndex = 318;
      this.C1R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B1.ThreeState = true;
      this.C1R1_B1.UseVisualStyleBackColor = true;
      this.C1R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R1_B2
      // 
      this.C1R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B2.Location = new System.Drawing.Point(18, 162);
      this.C1R1_B2.Name = "C1R1_B2";
      this.C1R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C1R1_B2.TabIndex = 317;
      this.C1R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B2.ThreeState = true;
      this.C1R1_B2.UseVisualStyleBackColor = true;
      this.C1R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R1_B3
      // 
      this.C1R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B3.Location = new System.Drawing.Point(18, 150);
      this.C1R1_B3.Name = "C1R1_B3";
      this.C1R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C1R1_B3.TabIndex = 316;
      this.C1R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B3.ThreeState = true;
      this.C1R1_B3.UseVisualStyleBackColor = true;
      this.C1R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R1_B4
      // 
      this.C1R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B4.Location = new System.Drawing.Point(18, 138);
      this.C1R1_B4.Name = "C1R1_B4";
      this.C1R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C1R1_B4.TabIndex = 315;
      this.C1R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B4.ThreeState = true;
      this.C1R1_B4.UseVisualStyleBackColor = true;
      this.C1R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R1_B5
      // 
      this.C1R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B5.Location = new System.Drawing.Point(18, 126);
      this.C1R1_B5.Name = "C1R1_B5";
      this.C1R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C1R1_B5.TabIndex = 314;
      this.C1R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B5.ThreeState = true;
      this.C1R1_B5.UseVisualStyleBackColor = true;
      this.C1R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R1_B6
      // 
      this.C1R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B6.Location = new System.Drawing.Point(18, 114);
      this.C1R1_B6.Name = "C1R1_B6";
      this.C1R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C1R1_B6.TabIndex = 313;
      this.C1R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B6.ThreeState = true;
      this.C1R1_B6.UseVisualStyleBackColor = true;
      this.C1R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R1_B7
      // 
      this.C1R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B7.Location = new System.Drawing.Point(18, 102);
      this.C1R1_B7.Name = "C1R1_B7";
      this.C1R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C1R1_B7.TabIndex = 312;
      this.C1R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R1_B7.ThreeState = true;
      this.C1R1_B7.UseVisualStyleBackColor = true;
      this.C1R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R0_B0
      // 
      this.C1R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B0.Location = new System.Drawing.Point(18, 90);
      this.C1R0_B0.Name = "C1R0_B0";
      this.C1R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C1R0_B0.TabIndex = 311;
      this.C1R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B0.ThreeState = true;
      this.C1R0_B0.UseVisualStyleBackColor = true;
      this.C1R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R0_B1
      // 
      this.C1R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B1.Location = new System.Drawing.Point(18, 78);
      this.C1R0_B1.Name = "C1R0_B1";
      this.C1R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C1R0_B1.TabIndex = 310;
      this.C1R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B1.ThreeState = true;
      this.C1R0_B1.UseVisualStyleBackColor = true;
      this.C1R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R0_B2
      // 
      this.C1R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B2.Location = new System.Drawing.Point(18, 66);
      this.C1R0_B2.Name = "C1R0_B2";
      this.C1R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C1R0_B2.TabIndex = 309;
      this.C1R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B2.ThreeState = true;
      this.C1R0_B2.UseVisualStyleBackColor = true;
      this.C1R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R0_B3
      // 
      this.C1R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B3.Location = new System.Drawing.Point(18, 54);
      this.C1R0_B3.Name = "C1R0_B3";
      this.C1R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C1R0_B3.TabIndex = 308;
      this.C1R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B3.ThreeState = true;
      this.C1R0_B3.UseVisualStyleBackColor = true;
      this.C1R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R0_B4
      // 
      this.C1R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B4.Location = new System.Drawing.Point(18, 42);
      this.C1R0_B4.Name = "C1R0_B4";
      this.C1R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C1R0_B4.TabIndex = 307;
      this.C1R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B4.ThreeState = true;
      this.C1R0_B4.UseVisualStyleBackColor = true;
      this.C1R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R0_B5
      // 
      this.C1R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B5.Location = new System.Drawing.Point(18, 30);
      this.C1R0_B5.Name = "C1R0_B5";
      this.C1R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C1R0_B5.TabIndex = 306;
      this.C1R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B5.ThreeState = true;
      this.C1R0_B5.UseVisualStyleBackColor = true;
      this.C1R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R0_B6
      // 
      this.C1R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B6.Location = new System.Drawing.Point(18, 18);
      this.C1R0_B6.Name = "C1R0_B6";
      this.C1R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C1R0_B6.TabIndex = 305;
      this.C1R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B6.ThreeState = true;
      this.C1R0_B6.UseVisualStyleBackColor = true;
      this.C1R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C1R0_B7
      // 
      this.C1R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B7.Location = new System.Drawing.Point(18, 6);
      this.C1R0_B7.Name = "C1R0_B7";
      this.C1R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C1R0_B7.TabIndex = 304;
      this.C1R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C1R0_B7.ThreeState = true;
      this.C1R0_B7.UseVisualStyleBackColor = true;
      this.C1R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R1_B0
      // 
      this.C0R1_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B0.Location = new System.Drawing.Point(6, 186);
      this.C0R1_B0.Name = "C0R1_B0";
      this.C0R1_B0.Size = new System.Drawing.Size(14, 14);
      this.C0R1_B0.TabIndex = 303;
      this.C0R1_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B0.ThreeState = true;
      this.C0R1_B0.UseVisualStyleBackColor = true;
      this.C0R1_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R1_B1
      // 
      this.C0R1_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B1.Location = new System.Drawing.Point(6, 174);
      this.C0R1_B1.Name = "C0R1_B1";
      this.C0R1_B1.Size = new System.Drawing.Size(14, 14);
      this.C0R1_B1.TabIndex = 302;
      this.C0R1_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B1.ThreeState = true;
      this.C0R1_B1.UseVisualStyleBackColor = true;
      this.C0R1_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R1_B2
      // 
      this.C0R1_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B2.Location = new System.Drawing.Point(6, 162);
      this.C0R1_B2.Name = "C0R1_B2";
      this.C0R1_B2.Size = new System.Drawing.Size(14, 14);
      this.C0R1_B2.TabIndex = 301;
      this.C0R1_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B2.ThreeState = true;
      this.C0R1_B2.UseVisualStyleBackColor = true;
      this.C0R1_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R1_B3
      // 
      this.C0R1_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B3.Location = new System.Drawing.Point(6, 150);
      this.C0R1_B3.Name = "C0R1_B3";
      this.C0R1_B3.Size = new System.Drawing.Size(14, 14);
      this.C0R1_B3.TabIndex = 300;
      this.C0R1_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B3.ThreeState = true;
      this.C0R1_B3.UseVisualStyleBackColor = true;
      this.C0R1_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R1_B4
      // 
      this.C0R1_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B4.Location = new System.Drawing.Point(6, 138);
      this.C0R1_B4.Name = "C0R1_B4";
      this.C0R1_B4.Size = new System.Drawing.Size(14, 14);
      this.C0R1_B4.TabIndex = 299;
      this.C0R1_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B4.ThreeState = true;
      this.C0R1_B4.UseVisualStyleBackColor = true;
      this.C0R1_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R1_B5
      // 
      this.C0R1_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B5.Location = new System.Drawing.Point(6, 126);
      this.C0R1_B5.Name = "C0R1_B5";
      this.C0R1_B5.Size = new System.Drawing.Size(14, 14);
      this.C0R1_B5.TabIndex = 298;
      this.C0R1_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B5.ThreeState = true;
      this.C0R1_B5.UseVisualStyleBackColor = true;
      this.C0R1_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R1_B6
      // 
      this.C0R1_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B6.Location = new System.Drawing.Point(6, 114);
      this.C0R1_B6.Name = "C0R1_B6";
      this.C0R1_B6.Size = new System.Drawing.Size(14, 14);
      this.C0R1_B6.TabIndex = 297;
      this.C0R1_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B6.ThreeState = true;
      this.C0R1_B6.UseVisualStyleBackColor = true;
      this.C0R1_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R1_B7
      // 
      this.C0R1_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B7.Location = new System.Drawing.Point(6, 102);
      this.C0R1_B7.Name = "C0R1_B7";
      this.C0R1_B7.Size = new System.Drawing.Size(14, 14);
      this.C0R1_B7.TabIndex = 296;
      this.C0R1_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R1_B7.ThreeState = true;
      this.C0R1_B7.UseVisualStyleBackColor = true;
      this.C0R1_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R0_B0
      // 
      this.C0R0_B0.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B0.Location = new System.Drawing.Point(6, 90);
      this.C0R0_B0.Name = "C0R0_B0";
      this.C0R0_B0.Size = new System.Drawing.Size(14, 14);
      this.C0R0_B0.TabIndex = 295;
      this.C0R0_B0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B0.ThreeState = true;
      this.C0R0_B0.UseVisualStyleBackColor = true;
      this.C0R0_B0.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R0_B1
      // 
      this.C0R0_B1.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B1.Location = new System.Drawing.Point(6, 78);
      this.C0R0_B1.Name = "C0R0_B1";
      this.C0R0_B1.Size = new System.Drawing.Size(14, 14);
      this.C0R0_B1.TabIndex = 294;
      this.C0R0_B1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B1.ThreeState = true;
      this.C0R0_B1.UseVisualStyleBackColor = true;
      this.C0R0_B1.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R0_B2
      // 
      this.C0R0_B2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B2.Location = new System.Drawing.Point(6, 66);
      this.C0R0_B2.Name = "C0R0_B2";
      this.C0R0_B2.Size = new System.Drawing.Size(14, 14);
      this.C0R0_B2.TabIndex = 293;
      this.C0R0_B2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B2.ThreeState = true;
      this.C0R0_B2.UseVisualStyleBackColor = true;
      this.C0R0_B2.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R0_B3
      // 
      this.C0R0_B3.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B3.Location = new System.Drawing.Point(6, 54);
      this.C0R0_B3.Name = "C0R0_B3";
      this.C0R0_B3.Size = new System.Drawing.Size(14, 14);
      this.C0R0_B3.TabIndex = 292;
      this.C0R0_B3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B3.ThreeState = true;
      this.C0R0_B3.UseVisualStyleBackColor = true;
      this.C0R0_B3.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R0_B4
      // 
      this.C0R0_B4.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B4.Location = new System.Drawing.Point(6, 42);
      this.C0R0_B4.Name = "C0R0_B4";
      this.C0R0_B4.Size = new System.Drawing.Size(14, 14);
      this.C0R0_B4.TabIndex = 291;
      this.C0R0_B4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B4.ThreeState = true;
      this.C0R0_B4.UseVisualStyleBackColor = true;
      this.C0R0_B4.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R0_B5
      // 
      this.C0R0_B5.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B5.Location = new System.Drawing.Point(6, 30);
      this.C0R0_B5.Name = "C0R0_B5";
      this.C0R0_B5.Size = new System.Drawing.Size(14, 14);
      this.C0R0_B5.TabIndex = 290;
      this.C0R0_B5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B5.ThreeState = true;
      this.C0R0_B5.UseVisualStyleBackColor = true;
      this.C0R0_B5.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R0_B6
      // 
      this.C0R0_B6.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B6.Location = new System.Drawing.Point(6, 18);
      this.C0R0_B6.Name = "C0R0_B6";
      this.C0R0_B6.Size = new System.Drawing.Size(14, 14);
      this.C0R0_B6.TabIndex = 289;
      this.C0R0_B6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B6.ThreeState = true;
      this.C0R0_B6.UseVisualStyleBackColor = true;
      this.C0R0_B6.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // C0R0_B7
      // 
      this.C0R0_B7.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B7.Location = new System.Drawing.Point(6, 6);
      this.C0R0_B7.Name = "C0R0_B7";
      this.C0R0_B7.Size = new System.Drawing.Size(14, 14);
      this.C0R0_B7.TabIndex = 288;
      this.C0R0_B7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.C0R0_B7.ThreeState = true;
      this.C0R0_B7.UseVisualStyleBackColor = true;
      this.C0R0_B7.Click += new System.EventHandler(this.Pixel_Click);
      // 
      // cmdLoadInternal
      // 
      this.cmdLoadInternal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdLoadInternal.Location = new System.Drawing.Point(314, 221);
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
      this.cmdLoadCustom.Location = new System.Drawing.Point(314, 196);
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
      this.cmdSave.Location = new System.Drawing.Point(233, 399);
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
      this.cmdExit.Location = new System.Drawing.Point(314, 399);
      this.cmdExit.Name = "cmdExit";
      this.cmdExit.Size = new System.Drawing.Size(75, 23);
      this.cmdExit.TabIndex = 551;
      this.cmdExit.Text = "Exit";
      this.cmdExit.UseVisualStyleBackColor = true;
      this.cmdExit.Click += new System.EventHandler(this.cmdExit_Click);
      // 
      // label21
      // 
      this.label21.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label21.AutoSize = true;
      this.label21.Location = new System.Drawing.Point(311, 178);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(79, 13);
      this.label21.TabIndex = 552;
      this.label21.Text = "Load ICON Set";
      this.label21.Visible = false;
      // 
      // iMONLCDg_IconEdit
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(392, 424);
      this.Controls.Add(this.label21);
      this.Controls.Add(this.cmdExit);
      this.Controls.Add(this.cmdSave);
      this.Controls.Add(this.cmdLoadCustom);
      this.Controls.Add(this.cmdLoadInternal);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.label16);
      this.Controls.Add(this.label17);
      this.Controls.Add(this.label18);
      this.Controls.Add(this.label19);
      this.Controls.Add(this.label20);
      this.Controls.Add(this.label11);
      this.Controls.Add(this.label12);
      this.Controls.Add(this.label13);
      this.Controls.Add(this.label14);
      this.Controls.Add(this.label15);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.label8);
      this.Controls.Add(this.label9);
      this.Controls.Add(this.label10);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.Icon9);
      this.Controls.Add(this.Icon8);
      this.Controls.Add(this.Icon7);
      this.Controls.Add(this.Icon6);
      this.Controls.Add(this.Icon5);
      this.Controls.Add(this.Icon4);
      this.Controls.Add(this.Icon3);
      this.Controls.Add(this.Icon2);
      this.Controls.Add(this.Icon1);
      this.Controls.Add(this.Icon0);
      this.Name = "iMONLCDg_IconEdit";
      this.Text = "iMONLCDg_IconEdit";
      ((System.ComponentModel.ISupportInitialize)(this.Icon0)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon3)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon4)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon9)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon8)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon7)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon6)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.Icon5)).EndInit();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    public void LoadCustomIcons()
    {
      DataTable table = new DataTable("LargeIcons");
      DataColumn column = new DataColumn("IconID");
      DataColumn column2 = new DataColumn("IData0");
      DataColumn column3 = new DataColumn("IData1");
      DataColumn column4 = new DataColumn("IData2");
      DataColumn column5 = new DataColumn("IData3");
      DataColumn column6 = new DataColumn("IData4");
      DataColumn column7 = new DataColumn("IData5");
      DataColumn column8 = new DataColumn("IData6");
      DataColumn column9 = new DataColumn("IData7");
      DataColumn column10 = new DataColumn("IData8");
      DataColumn column11 = new DataColumn("IData9");
      DataColumn column12 = new DataColumn("IData10");
      DataColumn column13 = new DataColumn("IData11");
      DataColumn column14 = new DataColumn("IData12");
      DataColumn column15 = new DataColumn("IData13");
      DataColumn column16 = new DataColumn("IData14");
      DataColumn column17 = new DataColumn("IData15");
      DataColumn column18 = new DataColumn("IData16");
      DataColumn column19 = new DataColumn("IData17");
      DataColumn column20 = new DataColumn("IData18");
      DataColumn column21 = new DataColumn("IData19");
      DataColumn column22 = new DataColumn("IData20");
      DataColumn column23 = new DataColumn("IData21");
      DataColumn column24 = new DataColumn("IData22");
      DataColumn column25 = new DataColumn("IData23");
      DataColumn column26 = new DataColumn("IData24");
      DataColumn column27 = new DataColumn("IData25");
      DataColumn column28 = new DataColumn("IData26");
      DataColumn column29 = new DataColumn("IData27");
      DataColumn column30 = new DataColumn("IData28");
      DataColumn column31 = new DataColumn("IData29");
      DataColumn column32 = new DataColumn("IData30");
      DataColumn column33 = new DataColumn("IData31");
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
      column8.DataType = typeof(byte);
      table.Columns.Add(column8);
      column9.DataType = typeof(byte);
      table.Columns.Add(column9);
      column10.DataType = typeof(byte);
      table.Columns.Add(column10);
      column11.DataType = typeof(byte);
      table.Columns.Add(column11);
      column12.DataType = typeof(byte);
      table.Columns.Add(column12);
      column13.DataType = typeof(byte);
      table.Columns.Add(column13);
      column14.DataType = typeof(byte);
      table.Columns.Add(column14);
      column15.DataType = typeof(byte);
      table.Columns.Add(column15);
      column16.DataType = typeof(byte);
      table.Columns.Add(column16);
      column17.DataType = typeof(byte);
      table.Columns.Add(column17);
      column18.DataType = typeof(byte);
      table.Columns.Add(column18);
      column19.DataType = typeof(byte);
      table.Columns.Add(column19);
      column20.DataType = typeof(byte);
      table.Columns.Add(column20);
      column21.DataType = typeof(byte);
      table.Columns.Add(column21);
      column22.DataType = typeof(byte);
      table.Columns.Add(column22);
      column23.DataType = typeof(byte);
      table.Columns.Add(column23);
      column24.DataType = typeof(byte);
      table.Columns.Add(column24);
      column25.DataType = typeof(byte);
      table.Columns.Add(column25);
      column26.DataType = typeof(byte);
      table.Columns.Add(column26);
      column27.DataType = typeof(byte);
      table.Columns.Add(column27);
      column28.DataType = typeof(byte);
      table.Columns.Add(column28);
      column29.DataType = typeof(byte);
      table.Columns.Add(column29);
      column30.DataType = typeof(byte);
      table.Columns.Add(column30);
      column31.DataType = typeof(byte);
      table.Columns.Add(column31);
      column32.DataType = typeof(byte);
      table.Columns.Add(column32);
      column33.DataType = typeof(byte);
      table.Columns.Add(column33);
      table.Clear();
      if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_icons.xml")))
      {
        table.Rows.Clear();
        XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
        XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_icons.xml"));
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
        Log.Debug("LoadLargeIconData() - completed", new object[0]);
        this.CopyBufferToGraphics();
        this.IconsChanged = false;
      }
      else
      {
        this.LoadInteralIcons();
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
      int num = (Row > 7) ? 0 : 1;
      int num2 = (Row < 8) ? Row : (Row - 8);
      string key = "C" + Column.ToString().Trim() + "R" + num.ToString().Trim() + "_B" + num2.ToString().Trim();
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
  }
}

