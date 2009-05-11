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
using System.Collections;
namespace MediaPortal.Util
{
	/// <summary>
	/// 
	/// </summary>
	public class HTMLTable
  {
    ArrayList m_rows=new ArrayList();

    public class HTMLRow
    {
      ArrayList m_colums=new ArrayList();
      public int		Columns
      {
        get { return m_colums.Count;}
      }
      public string GetColumValue(int iColumn) 
      {
        return (string)m_colums[iColumn];
      }
      public void  Parse(string strTable)
      {
        string strTag="";
        HTMLUtil util =new HTMLUtil();
        int iPosEnd=(int)strTable.Length+1;
        int iTableRowStart=0;
        do
        {
          iTableRowStart=util.FindTag(strTable,"<td",ref strTag,iTableRowStart);
          if (iTableRowStart>=0)
          {
            iTableRowStart+=(int)strTag.Length;
            int iTableRowEnd=util.FindClosingTag(strTable,"td",ref strTag,iTableRowStart)-1;
            if (iTableRowEnd<-1)
              break;
              
            string strRow=strTable.Substring(iTableRowStart,1+iTableRowEnd-iTableRowStart);			
            m_colums.Add(strRow);
            //OutputDebugString(strRow.c_str());
            //OutputDebugString("\n");
            iTableRowStart=iTableRowEnd+1;
          }
        } while (iTableRowStart>=0);
      }
    };

		public HTMLTable()
		{
    }
    public int Rows
    {
      get { return m_rows.Count;}
    }
    public HTMLRow GetRow(int iRow) 
    {
      return (HTMLRow)m_rows[iRow];
    }
    public void  Parse(string strHTML)
    {
      m_rows.Clear();
      HTMLUtil util = new HTMLUtil();
      string strTag="";
      int iPosStart=util.FindTag(strHTML,"<table",ref strTag,0);
      if (iPosStart>=0)
      {
        iPosStart+=(int)strTag.Length;
        int iPosEnd=util.FindClosingTag(strHTML,"table",ref  strTag,iPosStart)-1;
        if (iPosEnd < 0)
        {
          iPosEnd=(int)strHTML.Length;
        }
		
        string strTable=strHTML.Substring(iPosStart,1+iPosEnd-iPosStart);
        int iTableRowStart=0;
        do
        {
          iTableRowStart=util.FindTag(strTable,"<tr",ref strTag,iTableRowStart);
          if (iTableRowStart>=0)
          {
            iTableRowStart+=(int)strTag.Length;
            int iTableRowEnd=util.FindClosingTag(strTable,"tr",ref strTag,iTableRowStart)-1;
            if (iTableRowEnd<0)
              break;
            string strRow =strTable.Substring(iTableRowStart,1+iTableRowEnd-iTableRowStart);
            HTMLRow row=new HTMLRow();
            row.Parse(strRow);
            m_rows.Add(row);
            iTableRowStart=iTableRowEnd+1;
          }
        } while (iTableRowStart>=0);
      }
    }
	}
}
