/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System;
using System.IO;
using System.Text;
using MediaPortal.Util;
// example m3u file:
//			#EXTM3U
//			#EXTINF:5,demo1
//			E:\Program Files\Winamp3\demo1.mp3
//			#EXTINF:5,demo2
//			E:\Program Files\Winamp3\demo2.mp3


namespace MediaPortal.Playlists
{
	/// <summary>
	/// 
	/// </summary>
	public class PlayListM3U : PlayList
	{
		const string M3U_START_MARKER	="#EXTM3U";
		const string M3U_INFO_MARKER	="#EXTINF";

		public PlayListM3U()
		{
		}
		
		public override bool 	Load(string  strFileName)
		{
      if (strFileName==null) return false;
			string strBasePath;
			Clear();
      try
      {
        m_strPlayListName=System.IO.Path.GetFileName(strFileName);
        strBasePath=System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(strFileName));
				
				Encoding fileEncoding = Encoding.Default;
				FileStream stream = File.Open(strFileName,FileMode.Open,FileAccess.Read,FileShare.Read);
				StreamReader file = new StreamReader(stream);
        //StreamReader file = File.OpenText(strFileName);
        if (file==null) 
        {
          return false;
        }

        string szLine;
        szLine=file.ReadLine();
        if (szLine==null || szLine.Length==0)
        {
          file.Close();
          return false;
        }
        string strLine=szLine.Trim();
        //CUtil::RemoveCRLF(strLine);
        if (strLine != M3U_START_MARKER)
        {
          strFileName=szLine;
          //CUtil::RemoveCRLF(strFileName);
          if (strFileName.Length>1)
          {
            Utils.GetQualifiedFilename(strBasePath,ref strFileName);
            PlayList.PlayListItem newItem = new PlayListItem(strFileName, strFileName, 0);
            newItem.Type = PlayListItem.PlayListItemType.Audio;
            string strDescription;
            strDescription=System.IO.Path.GetFileName(strFileName);
            newItem.Description=strDescription;
            Add(newItem);
          }
        }

        szLine=file.ReadLine();
        while (szLine!=null  )
        {
          strLine=szLine.Trim();
          //CUtil::RemoveCRLF(strLine);
          if (strLine.StartsWith( M3U_INFO_MARKER) )
          {
            // start of info 
            int iColon=(int)strLine.IndexOf(":");
            int iComma=(int)strLine.IndexOf(",");
            if (iColon >=0 && iComma >= 0 && iComma > iColon)
            {
              iColon++;
              string strLength=strLine.Substring(iColon, iComma-iColon);
              iComma++;
              string strInfo=strLine.Substring(iComma);
              int lDuration=System.Int32.Parse(strLength);
              //lDuration*=1000;

              szLine=file.ReadLine();
              if (szLine!=null && szLine.Length>0 )
              {
                strFileName=szLine.Trim();
                //CUtil::RemoveCRLF(strFileName);
                if (strFileName.Length>1)
                {
                  Utils.GetQualifiedFilename(strBasePath,ref strFileName);
                  PlayListItem newItem=new PlayListItem(strInfo,strFileName,lDuration);
                  newItem.Type = PlayListItem.PlayListItemType.Audio;
                  if (strInfo.Length==0)
                  {
                    strInfo=System.IO.Path.GetFileName(strFileName);
                    newItem.Description=strInfo;
                  }
                  Add(newItem);
                }
              }
              else
              {
                // eof
                break;
              }
            }
          }
          else
          {
            strFileName=szLine.Trim();
            //CUtil::RemoveCRLF(strFileName);
            if (strFileName.Length>1)
            {
              Utils.GetQualifiedFilename(strBasePath,ref strFileName);
              PlayListItem newItem = new PlayListItem(strFileName, strFileName, 0);
              newItem.Type = PlayListItem.PlayListItemType.Audio;
              string strDescription;
              strDescription=System.IO.Path.GetFileName(strFileName);
              newItem.Description=strDescription;
              Add(newItem);
            }
          }
          szLine=file.ReadLine();
        }

        file.Close();
      }
      catch(Exception)
      {
        return false;
      }
			return true;
		}

		public override void 	Save(string strFileName)  
		{
      try
      {
        using (StreamWriter writer = new StreamWriter(strFileName,true))
        {
          writer.WriteLine(M3U_START_MARKER);
          for (int i=0; i < m_items.Count;++i)
          {
            PlayListItem item=(PlayListItem)m_items[i];
            writer.WriteLine("{0}:{1},{2}",M3U_INFO_MARKER, item.Duration/1000, item.Description);
            writer.WriteLine("{0}",item.FileName);
          }
          writer.Close();
        }
      }
      catch (Exception)
      {
      }
		}

	}
}
