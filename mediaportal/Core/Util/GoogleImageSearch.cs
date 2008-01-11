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
using System.IO;
using System.Collections;
using System.Net;
using MediaPortal.GUI.Library;

namespace MediaPortal.Util
{
	/// <summary>
	/// 
	/// </summary>
	public class GoogleImageSearch
	{
    ArrayList m_imageList = new ArrayList();

		public GoogleImageSearch()
		{
		}

    public int Count
    {
      get { return m_imageList.Count;}
    }

    public string this[int index]
    {
      get { 
				if (index<0 || index>m_imageList.Count) return string.Empty;
				return (string)m_imageList[index];
			}
    }
    
    public void Search(string searchtag)
    {
			if (searchtag==null) return;
			if (searchtag==string.Empty) return;
      m_imageList.Clear();
      try
      {
        UriBuilder builder=new UriBuilder();
        builder.Host="images.google.nl";
        builder.Path="/images";
        builder.Query=String.Format("q={0}", searchtag);

        // Make the Webrequest
        WebRequest req = WebRequest.Create(builder.Uri.AbsoluteUri);
        WebResponse result = req.GetResponse();
        using (Stream ReceiveStream = result.GetResponseStream())
        {
          StreamReader sr = new StreamReader( ReceiveStream);
          string strBody = sr.ReadToEnd();
          Parse(strBody);
        }
      }
      catch(Exception ex)
      {
        Log.Warn("get failed:{0}", ex.Message);
      }
    }

    void Parse(string body)
		{
			if (body==null) return;
			if (body==string.Empty) return;
      int pos=body.IndexOf("imgurl=");
      while (pos>=0)
      {
        int endpos1=body.IndexOf("&imgrefurl=",pos);
        int endpos2=body.IndexOf(">",pos);
        if (endpos1 >=0 && endpos2 >=0)
        {
          if (endpos2 < endpos1) endpos1=endpos2;
        }
        if (endpos1 < 0) return;
        pos+="imgurl=".Length;
        string url=body.Substring(pos,endpos1-pos);
        m_imageList.Add(url);
        pos=body.IndexOf("imgurl=",endpos1);
      }
    }

	}
}
