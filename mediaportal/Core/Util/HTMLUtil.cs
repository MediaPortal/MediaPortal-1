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
      strStripped = DecodedString;
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
	}
}
