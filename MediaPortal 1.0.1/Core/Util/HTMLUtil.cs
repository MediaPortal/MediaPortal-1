#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Text;
using System.IO;
using System.Web;

namespace MediaPortal.Util
{
	/// <summary>
	/// 
	/// </summary>
	public class HTMLUtil
	{
		public HTMLUtil()
		{

		}

    public int FindTag( string strHTML, string strTag, ref string strtagFound, int iPos) 
    {
      if (iPos < 0 || iPos>= strHTML.Length) return -1;
	    string strHTMLLow=strHTML;
	    string strTagLow=strTag;
	    strHTMLLow=strHTMLLow.ToLower();
	    strTagLow=strTagLow.ToLower();
	    strtagFound="";
	    int iStart=strHTMLLow.IndexOf(strTag,iPos);
	    if (iStart < 0) return -1;
	    int iEnd=strHTMLLow.IndexOf(">",iStart);
	    if (iEnd < 0) iEnd=(int)strHTMLLow.Length;
	    strtagFound=strHTMLLow.Substring(iStart,(iEnd+1)-iStart);
	    return iStart;
    }
    public int FindClosingTag(string  strHTML,string strTag, ref string strtagFound, int iPos) 
    {                                                        
      string strHTMLLow=strHTML.ToLower();
      string strTagLow=strTag.ToLower();
      strtagFound="";
      int iStart=strHTMLLow.IndexOf("</"+strTag,iPos);
      if (iStart < 0) return -1;
      int iOpenStart=strHTMLLow.IndexOf("<"+strTag, iPos);
      while (iOpenStart<iStart && iOpenStart!=-1)
      {
        iStart=strHTMLLow.IndexOf("</"+strTag,iStart+1);
        iOpenStart=strHTMLLow.IndexOf("<"+strTag, iOpenStart+1);
      }
  	 
      int iEnd=strHTMLLow.IndexOf(">",iStart);
      if (iEnd < 0) iEnd=(int)strHTMLLow.Length;
      strtagFound=strHTMLLow.Substring(iStart,(iEnd+1)-iStart);
      return iStart;
    }
  	 

    public void getValueOfTag(string strTagAndValue, out string strValue)
    {
	    // strTagAndValue contains:
	    // like <a href=blablabla.....>value</a>
	    strValue=strTagAndValue;
	    int iStart=strTagAndValue.IndexOf(">");
	    int iEnd=strTagAndValue.IndexOf("<",iStart+1);
	    if (iStart>=0 && iEnd>=0)
	    {
		    iStart++;
		    strValue=strTagAndValue.Substring(iStart,iEnd-iStart);
	    }
    }

    public void  getAttributeOfTag(string strTagAndValue, string strTag,ref string strValue)
    {
	    // strTagAndValue contains:
	    // like <a href=""value".....
	    strValue=strTagAndValue;
	    int iStart=strTagAndValue.IndexOf(strTag);
	    if (iStart< 0) return;
	    iStart+=(int)strTag.Length;
	    while (strTagAndValue[iStart+1] == 0x20 || strTagAndValue[iStart+1] == 0x27 || strTagAndValue[iStart+1] == 34) iStart++;
	    int iEnd=iStart+1;
	    while (strTagAndValue[iEnd] != 0x27 && strTagAndValue[iEnd] != 0x20 && strTagAndValue[iEnd] != 34&& strTagAndValue[iEnd] != '>') iEnd++;
	    if (iStart>=0 && iEnd>=0)
	    {
		    strValue=strTagAndValue.Substring(iStart,iEnd-iStart);
	    }
    }

    public void RemoveTags(ref string strHTML)
    {
	    int iNested=0;
	    string strReturn="";
	    for (int i=0; i < (int) strHTML.Length; ++i)
	    {
		    if (strHTML[i] == '<') iNested++;
		    else if (strHTML[i] == '>') iNested--;
		    else
		    {
			    if (0==iNested)
			    {
				    strReturn+=strHTML[i];
			    }
		    }
	    }
	    strHTML=strReturn;
    }
		public string ConvertHTMLToAnsi(string strHTML)
		{
			string strippedHtml=string.Empty;
			ConvertHTMLToAnsi( strHTML, out strippedHtml);
			return strippedHtml;
		}
    public void ConvertHTMLToAnsi(string strHTML, out string strStripped)
    {
      strStripped="";
//	    int i=0; 
	    if (strHTML.Length==0)
	    {
		    strStripped="";
		    return;
	    }
	    //int iAnsiPos=0;
      StringWriter writer = new StringWriter ( );

      System.Web.HttpUtility.HtmlDecode( strHTML, writer );

      String DecodedString = writer.ToString ( );
      strStripped = DecodedString.Replace("<br>","\n");
      if(true)
        return;
/*
	    string szAnsi = "";

	    while (i < (int)strHTML.Length )
	    {
		    char kar=strHTML[i];
		    if (kar=='&')
		    {
			    if (strHTML[i+1]=='#')
			    {
				    int ipos=0;
				    i+=2;
				    string szDigit="";
				    while ( ipos < 12 && i<strHTML.Length && Char.IsDigit(strHTML[i])) 
				    {
					    szDigit+=strHTML[i];
					    ipos++;
					    i++;
				    }
				    szAnsi+= (char)(Int32.Parse(szDigit));
				    i++;
			    }
			    else
			    {
				    i++;
				    int ipos=0;
				    string szKey="";
				    while (i<strHTML.Length && strHTML[i] != ';' && ipos < 12)
				    {
					    szKey+=Char.ToLower(strHTML[i]);
					    ipos++;
					    i++;
				    }
				    i++;
				    if (String.Compare(szKey,"amp")==0) szAnsi+='&';
				    if (String.Compare(szKey,"nbsp")==0) szAnsi+=' ';
			    }
		    }
		    else
		    {
			    szAnsi+=kar;
			    i++;
		    }
	    }
	    strStripped=szAnsi;*/
    }
		public void ParseAHREF(string ahref, out string title, out string url)
		{
			title="";
			url="";
			int pos1=ahref.IndexOf("\"");
			if (pos1 < 0) return;
			int pos2=ahref.IndexOf("\"",pos1+1);
			if (pos2 < 0) return;
			url=ahref.Substring(pos1+1,pos2-pos1-1);

			pos1=ahref.IndexOf(">");
			if (pos1 < 0) return;
			pos2=ahref.IndexOf("<",pos1);
			if (pos2 < 0) return;
			title=ahref.Substring(pos1+1,pos2-pos1-1);
			
		}
	}
}
