/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System;
using System.IO;
using System.Text;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
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
		
		public override bool 	Load(string  fileName)
		{
      if (fileName==null) return false;
			string basePath;
			Clear();
      try
      {
        _playListName=System.IO.Path.GetFileName(fileName);
        basePath=System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(fileName));
				
				Encoding fileEncoding = Encoding.Default;
				FileStream stream = File.Open(fileName,FileMode.Open,FileAccess.Read,FileShare.Read);
				StreamReader file = new StreamReader(stream);
        //StreamReader file = File.OpenText(fileName);
        if (file==null) 
        {
          return false;
        }

        string line;
        line=file.ReadLine();
        if (line==null || line.Length==0)
        {
          file.Close();
          return false;
        }
        string strLine=line.Trim();
        //CUtil::RemoveCRLF(strLine);
        if (strLine != M3U_START_MARKER)
        {
          fileName=line;
          //CUtil::RemoveCRLF(fileName);
          if (fileName.Length>1)
          {
            Utils.GetQualifiedFilename(basePath,ref fileName);
            PlayListItem newItem = new PlayListItem(fileName, fileName, 0);
            newItem.Type = PlayListItem.PlayListItemType.Audio;
            string strDescription;
            strDescription=System.IO.Path.GetFileName(fileName);
            newItem.Description=strDescription;
            Add(newItem);
          }
        }

        line=file.ReadLine();
        while (line!=null  )
        {
          strLine=line.Trim();
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

              line=file.ReadLine();
              if (line!=null && line.Length>0 )
              {
                fileName=line.Trim();
                //CUtil::RemoveCRLF(fileName);
                if (fileName.Length>1)
                {
                  Utils.GetQualifiedFilename(basePath,ref fileName);
                  PlayListItem newItem=new PlayListItem(strInfo,fileName,lDuration);
                  newItem.Type = PlayListItem.PlayListItemType.Audio;
                  if (strInfo.Length==0)
                  {
                    strInfo=System.IO.Path.GetFileName(fileName);
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
            fileName=line.Trim();
            //CUtil::RemoveCRLF(fileName);
            if (fileName.Length>1)
            {
              Utils.GetQualifiedFilename(basePath,ref fileName);
              PlayListItem newItem = new PlayListItem(fileName, fileName, 0);
              newItem.Type = PlayListItem.PlayListItemType.Audio;
              string strDescription;
              strDescription=System.IO.Path.GetFileName(fileName);
              newItem.Description=strDescription;
              Add(newItem);
            }
          }
          line=file.ReadLine();
        }

        file.Close();
      }
      catch(Exception)
      {
        return false;
      }
			return true;
		}

		public override void 	Save(string fileName)  
		{
      try
      {
        using (StreamWriter writer = new StreamWriter(fileName,true))
        {
          writer.WriteLine(M3U_START_MARKER);
          for (int i=0; i < _listPlayListItems.Count;++i)
          {
            PlayListItem item=_listPlayListItems[i];
            writer.WriteLine("{0}:{1},{2}",M3U_INFO_MARKER, item.Duration/1000, item.Description);
            writer.WriteLine("{0}",item.FileName);
          }
          writer.Close();
        }
      }
      catch (Exception e)
      {
        Log.Write( "failed to save a playlist {0}. err: {1} stack: {2}", fileName, e.Message, e.StackTrace );
      }
		}

	}
}
