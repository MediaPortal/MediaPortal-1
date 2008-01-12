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
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Controls
{
	
  #region Formattable TextBox Column

  public class FormattableTextBoxColumn : DataGridTextBoxColumn
  {
    public event FormatCellEventHandler SetCellFormat;

    //used to fire an event to retrieve formatting info
    //and then draw the cell with this formatting info
    protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
    {
      DataGridFormatCellEventArgs e = null;

      bool callBaseClass = true;

      //fire the formatting event
      if(SetCellFormat != null)
      {
        int col = this.DataGridTableStyle.GridColumnStyles.IndexOf(this);
        e = new DataGridFormatCellEventArgs(rowNum, col, this.GetColumnValueAtRow(source, rowNum));
        SetCellFormat(this, e);
        if(e.BackBrush != null)
          backBrush = e.BackBrush;

        //if these properties set, then must call drawstring
        if(e.ForeBrush != null || e.TextFont != null)
        {
          if(e.ForeBrush == null)
            e.ForeBrush = foreBrush;
          if(e.TextFont == null)
            e.TextFont = this.DataGridTableStyle.DataGrid.Font;
          g.FillRectangle(backBrush, bounds);
          Region saveRegion = g.Clip;
          Rectangle rect = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
          using(Region newRegion = new Region(rect))
          {
            g.Clip = newRegion;
            int charWidth = (int) Math.Ceiling(g.MeasureString("c", e.TextFont, 20, StringFormat.GenericTypographic).Width);

            string s = this.GetColumnValueAtRow(source, rowNum).ToString();
            int maxChars = Math.Min(s.Length,  (bounds.Width / charWidth));

            try
            {
              g.DrawString(s.Substring(0, maxChars), e.TextFont, e.ForeBrush, bounds.X, bounds.Y + 2);
            }
            catch(Exception ex)
            {
              Console.WriteLine(ex.Message.ToString());
            } //empty catch
            finally
            {
              g.Clip = saveRegion;
            }
          }
          callBaseClass = false;
        }
					
        if(!e.UseBaseClassDrawing)
        {
          callBaseClass = false;
        }
      }
      if(callBaseClass)
        base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);

      //clean up
      if(e != null)
      {
        if(e.BackBrushDispose)
          e.BackBrush.Dispose();
        if(e.ForeBrushDispose)
          e.ForeBrush.Dispose();
        if(e.TextFontDispose)
          e.TextFont.Dispose();
      }
    }
  }

  #endregion

  #region Formattable Bool Column

  public class FormattableBooleanColumn : DataGridBoolColumn
  {
    public event FormatCellEventHandler SetCellFormat;

    public FormattableBooleanColumn()
    {
      AllowNull=false;
    }
    //overridden to fire BoolChange event and Formatting event
    protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
    {
      int colNum = this.DataGridTableStyle.GridColumnStyles.IndexOf(this);
			
      //used to handle the boolchanging
      ManageBoolValueChanging(rowNum, colNum);
			
      //fire formatting event
      DataGridFormatCellEventArgs e = null;
      bool callBaseClass = true;
      if(SetCellFormat != null)
      {
        e = new DataGridFormatCellEventArgs(rowNum, colNum, this.GetColumnValueAtRow(source, rowNum));
        SetCellFormat(this, e);
        if(e.BackBrush != null)
          backBrush = e.BackBrush;
        callBaseClass = e.UseBaseClassDrawing;
      }
      if(callBaseClass)
        base.Paint(g, bounds, source, rowNum, backBrush, new SolidBrush(Color.Red), alignToRight);

      //clean up
      if(e != null)
      {
        if(e.BackBrushDispose)
          e.BackBrush.Dispose();
        if(e.ForeBrushDispose)
          e.ForeBrush.Dispose();
        if(e.TextFontDispose)
          e.TextFont.Dispose();
      }
    }

    //changed event
    public event BoolValueChangedEventHandler BoolValueChanged;
		
    bool saveValue = false;
    int saveRow = -1;
    bool lockValue = false;
    bool beingEdited = false;
    public const int VK_SPACE = 32 ;// 0x20

    //needed to get the space bar changing of the bool value
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern short GetKeyState(int nVirtKey);

    //set variables to start tracking bool changes
    protected override void Edit(System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
    {	
      lockValue = true;
      beingEdited = true;
      saveRow = rowNum;
      saveValue = (bool) base.GetColumnValueAtRow(source, rowNum);
      base.Edit(source, rowNum,  bounds, readOnly, instantText, cellIsVisible);
    }

    //turn off tracking bool changes
    protected override bool Commit(System.Windows.Forms.CurrencyManager dataSource, int rowNum)
    {
      lockValue = true;
      beingEdited = false;
      return base.Commit(dataSource, rowNum);
    }

    //fire the bool change event if the value changes
    private void ManageBoolValueChanging(int rowNum, int colNum)
    {
      Point mousePos = this.DataGridTableStyle.DataGrid.PointToClient(Control.MousePosition);
      DataGrid dg = this.DataGridTableStyle.DataGrid;
      bool isClickInCell = ((Control.MouseButtons == MouseButtons.Left) && 
        dg.GetCellBounds(dg.CurrentCell).Contains(mousePos) );

      bool changing = dg.Focused && ( isClickInCell 
        || GetKeyState(VK_SPACE) < 0 ); // or spacebar
			
      if(!lockValue && beingEdited && changing && saveRow == rowNum)
      {
        saveValue = !saveValue;
        lockValue = false;

        //fire the event
        if(BoolValueChanged != null)
        {
          BoolValueChangedEventArgs e = new BoolValueChangedEventArgs(rowNum, colNum, saveValue);
          BoolValueChanged(this, e);
        }
      }
      if(saveRow == rowNum)
        lockValue = false;	
    }
  }

  #endregion

  #region CellFormatting Event

  public delegate void FormatCellEventHandler(object sender, DataGridFormatCellEventArgs e);

  public class DataGridFormatCellEventArgs : EventArgs
  {
    private int colNum;
    private int rowNum;
    private Font fontVal;
    private Brush backBrushVal;
    private Brush foreBrushVal;
    private bool fontDispose;
    private bool backBrushDispose;
    private bool foreBrushDispose;
    private bool useBaseClassDrawingVal;
    private object currentCellValue;

    public DataGridFormatCellEventArgs(int row, int col, object cellValue)
    {
      rowNum = row;
      colNum = col;
      fontVal = null;
      backBrushVal = null;
      foreBrushVal = null;
      fontDispose = false;
      backBrushDispose = false;
      foreBrushDispose = false;
      useBaseClassDrawingVal = true;
      currentCellValue = cellValue;
    }

	
    //column being painted
    public int Column
    {
      get{ return colNum;}
      set{ colNum = value;}
    }

    //row being painted
    public int Row
    {
      get{ return rowNum;}
      set{ rowNum = value;}
    }

    //font used for drawing the text
    public Font TextFont
    {
      get{ return fontVal;}
      set{ fontVal = value;}
    }

    //background brush
    public Brush BackBrush
    {
      get{ return backBrushVal;}
      set{ backBrushVal = value;}
    }

    //foreground brush
    public Brush ForeBrush
    {
      get{ return foreBrushVal;}
      set{ foreBrushVal = value;}
    }

    //set true if you want the Paint method to call Dispose on the font
    public bool TextFontDispose
    {
      get{ return fontDispose;}
      set{ fontDispose = value;}
    }
		
    //set true if you want the Paint method to call Dispose on the brush
    public bool BackBrushDispose
    {
      get{ return backBrushDispose;}
      set{ backBrushDispose = value;}
    }

    //set true if you want the Paint method to call Dispose on the brush
    public bool ForeBrushDispose
    {
      get{ return foreBrushDispose;}
      set{ foreBrushDispose = value;}
    }

    //set true if you want the Paint method to call base class
    public bool UseBaseClassDrawing
    {
      get{ return useBaseClassDrawingVal;}
      set{ useBaseClassDrawingVal = value;}
    }
		
    //contains the current cell value
    public object CurrentCellValue
    {
      get{ return currentCellValue;}
    }
  }
  #endregion

  #region BoolValueChanging Event
  public delegate void BoolValueChangedEventHandler(object sender, BoolValueChangedEventArgs e);

  public class BoolValueChangedEventArgs : EventArgs
  {
    private int columnVal;
    private int rowVal;
    private bool boolVal;

    public BoolValueChangedEventArgs(int row, int col, bool val)
    {
      rowVal = row;
      columnVal = col;
      boolVal = val;
    }
		
    //column to be painted
    public int Column
    {
      get{ return columnVal;}
      set{ columnVal = value;}
    }

    //row to be painted
    public int Row
    {
      get{ return rowVal;}
      set{ rowVal = value;}
    }

    //current value to be painted
    public bool BoolValue
    {
      get{return boolVal;}
    }
  }
  #endregion
}
