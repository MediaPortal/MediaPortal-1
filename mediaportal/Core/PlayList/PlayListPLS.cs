using System;
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
		public override bool Load(string strFileName)
		{
			string strBasePath;
			string strExt=System.IO.Path.GetExtension(strFileName);
			strExt.ToLower();

			Clear();
			m_strPlayListName=System.IO.Path.GetFileName(strFileName);
			strBasePath=System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(strFileName));
			Encoding fileEncoding = Encoding.Default;
			FileStream stream = File.Open(strFileName,FileMode.Open,FileAccess.Read,FileShare.Read);
			StreamReader file = new StreamReader(stream, fileEncoding, true);
			if (file==null ) 
			{
				return false;
			}

			string szLine;
			szLine=file.ReadLine();
			if (szLine==null)
			{
				file.Close();
				return false;
			}

			string strLine=szLine.Trim();
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
				Encoding fileEncoding = Encoding.Default;
				stream = File.Open(strFileName,FileMode.Open,FileAccess.Read,FileShare.Read);
				file = new StreamReader(stream, fileEncoding, true);

				//file.Close();
				//return false;
			}
			string strInfo="";
			string strDuration="";
			strFileName="";
			szLine=file.ReadLine();
			while (szLine!=null)
			{
				strLine=szLine.Trim();
			//CUtil::RemoveCRLF(strLine);
				int iPosEqual=strLine.IndexOf("=");
				if (iPosEqual>0)
				{
					string strLeft =strLine.Substring(0,iPosEqual);
					iPosEqual++;
					string strValue=strLine.Substring(iPosEqual);
					strLeft=strLeft.ToLower();
					if (strLeft.StartsWith("file"))
					{	
						if (strFileName.Length!=0)
						{
							PlayListItem newItem=new PlayListItem(strInfo,strFileName,0);
							Add(newItem);
							strFileName="";
							strInfo="";
							strDuration="";
						}
						strFileName=strValue;

					}
					if (strLeft.StartsWith("title"))
					{	
						strInfo=strValue;
					}
					else 
					{
						if (strInfo=="") strInfo=System.IO.Path.GetFileName(strFileName);
					}
					if (strLeft.StartsWith("length"))
					{	
						strDuration=strValue;
					}
					if (strLeft=="playlistname")
					{
						m_strPlayListName=strValue;
					}

					if (strDuration.Length>0 && strInfo.Length>0 && strFileName.Length>0) 
					{
						int lDuration=System.Int32.Parse(strDuration);
						lDuration*=1000;
            string strTmp=strFileName.ToLower();
            PlayListItem newItem=new PlayListItem(strInfo,strFileName,lDuration);
						if (strTmp.IndexOf("http:")<0 && strTmp.IndexOf("mms:")<0 && strTmp.IndexOf("rtp:")<0)
						{
							Utils.GetQualifiedFilename(strBasePath,ref strFileName);
              newItem.Type = PlayListItem.PlayListItemType.AudioStream;
						}
						Add(newItem);
						strFileName="";
						strInfo="";
						strDuration="";
					}
				}		
				szLine=file.ReadLine();
			}
			file.Close();

			if (strFileName.Length>0)
			{
				PlayListItem newItem=new PlayListItem(strInfo,strFileName,0);
			}


			return true;

		}

		public override void 	Save(string strFileName)  
		{
			using (StreamWriter writer = new StreamWriter(strFileName,true))
			{
				writer.WriteLine(START_PLAYLIST_MARKER);
				for (int i=0; i < m_items.Count;++i)
				{
					PlayListItem item=(PlayListItem)m_items[i];
					writer.WriteLine("File{0}={1}",i+1, item.FileName );
					writer.WriteLine("Title{0}={1}",i+1, item.Description );
					writer.WriteLine("Length{0}={1}",i+1, item.Duration/1000 );

				}
				writer.WriteLine("NumberOfEntries={0}",m_items.Count);
				writer.WriteLine("Version=2");
				writer.Close();
			}
		}

	}
}
