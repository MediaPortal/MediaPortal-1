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
using System.Text;
using System.IO;
using MediaPortal.Util;
/*----------------------------------------------------------------------
[playlist]
PlaylistName=Playlist 001
File1=E:\Program Files\Winamp3\demo.mp3
Title1=demo
Length1=5
File2=E:\Program Files\Winamp3\demo.mp3
Title2=demo
Length2=5
NumberOfEntries=2
Version=2
----------------------------------------------------------------------*/

namespace MediaPortal.Playlists
{
	/// <summary>
	/// 
	/// </summary>
	public class PlayListPLS : PlayList
	{
		const string START_PLAYLIST_MARKER= "[playlist]";
		const string PLAYLIST_NAME				=	"PlaylistName";

		public PlayListPLS()
		{
			// 
			// TODO: Add constructor logic here
			//
		}
		public override bool Load(string fileName)
		{
			string basePath;
			string extension=System.IO.Path.GetExtension(fileName);
			extension.ToLower();

			Clear();
			_playListName=System.IO.Path.GetFileName(fileName);
			basePath=System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(fileName));
			Encoding fileEncoding = Encoding.Default;
			FileStream stream = File.Open(fileName,FileMode.Open,FileAccess.Read,FileShare.Read);
			StreamReader file = new StreamReader(stream, fileEncoding, true);
			if (file==null ) 
			{
				return false;
			}

			string line;
			line=file.ReadLine();
			if (line==null)
			{
				file.Close();
				return false;
			}

			string strLine=line.Trim();
			//CUtil::RemoveCRLF(strLine);
			if (strLine != START_PLAYLIST_MARKER)
			{
				if (strLine.StartsWith("http") || strLine.StartsWith("HTTP") ||
					  strLine.StartsWith("mms") || strLine.StartsWith("MMS") ||
					  strLine.StartsWith("rtp") || strLine.StartsWith("RTP")  )
				{
					PlayListItem newItem=new PlayListItem(strLine,strLine,0);
          newItem.Type = PlayListItem.PlayListItemType.AudioStream;
					Add(newItem);
					file.Close();
					return true;
				}
				fileEncoding = Encoding.Default;
				stream = File.Open(fileName,FileMode.Open,FileAccess.Read,FileShare.Read);
				file = new StreamReader(stream, fileEncoding, true);

				//file.Close();
				//return false;
			}
			string infoLine="";
			string durationLine="";
			fileName="";
			line=file.ReadLine();
			while (line!=null)
			{
				strLine=line.Trim();
			//CUtil::RemoveCRLF(strLine);
				int equalPos=strLine.IndexOf("=");
				if (equalPos>0)
				{
					string leftPart =strLine.Substring(0,equalPos);
					equalPos++;
					string valuePart=strLine.Substring(equalPos);
					leftPart=leftPart.ToLower();
					if (leftPart.StartsWith("file"))
					{	
						if(valuePart.Length > 0 && valuePart[0] == '#')
						{
							line=file.ReadLine();
							continue;
						}

						if(fileName.Length!=0)
						{
							PlayListItem newItem=new PlayListItem(infoLine,fileName,0);
							Add(newItem);
							fileName="";
							infoLine="";
							durationLine="";
						}
						fileName=valuePart;
					}
					if (leftPart.StartsWith("title"))
					{	
						infoLine=valuePart;
					}
					else 
					{
						if (infoLine=="") infoLine=System.IO.Path.GetFileName(fileName);
					}
					if (leftPart.StartsWith("length"))
					{	
						durationLine=valuePart;
					}
					if (leftPart=="playlistname")
					{
						_playListName=valuePart;
					}

					if (durationLine.Length>0 && infoLine.Length>0 && fileName.Length>0) 
					{
						int duration=System.Int32.Parse(durationLine);
						duration*=1000;

            string tmp=fileName.ToLower();
            PlayListItem newItem=new PlayListItem(infoLine,fileName,duration);
						if (tmp.IndexOf("http:")<0 && tmp.IndexOf("mms:")<0 && tmp.IndexOf("rtp:")<0)
						{
							Utils.GetQualifiedFilename(basePath,ref fileName);
							newItem.Type = PlayListItem.PlayListItemType.AudioStream;
						}
						Add(newItem);
						fileName="";
						infoLine="";
						durationLine="";
					}
				}		
				line=file.ReadLine();
			}
			file.Close();

			if (fileName.Length>0)
			{
				PlayListItem newItem=new PlayListItem(infoLine,fileName,0);
			}


			return true;

		}

		public override void 	Save(string fileName)  
		{
			using (StreamWriter writer = new StreamWriter(fileName,true))
			{
				writer.WriteLine(START_PLAYLIST_MARKER);
				for (int i=0; i < _listPlayListItems.Count;++i)
				{
					PlayListItem item=_listPlayListItems[i];
					writer.WriteLine("File{0}={1}",i+1, item.FileName );
					writer.WriteLine("Title{0}={1}",i+1, item.Description );
					writer.WriteLine("Length{0}={1}",i+1, item.Duration/1000 );

				}
        writer.WriteLine("NumberOfEntries={0}", _listPlayListItems.Count);
				writer.WriteLine("Version=2");
				writer.Close();
			}
		}

	}
}
