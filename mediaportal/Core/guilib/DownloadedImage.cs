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
using System.Net;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;



namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for DownloadedImage.
	/// </summary>
	class DownloadedImage
	{
		string    _FileName;
		string    _URL;
		DateTime  _DateDownloaded=DateTime.MinValue;
		int       _CacheTime = 60*30; //30minutes

		public DownloadedImage(string url)
		{
			URL=url;
			int pos=url.LastIndexOf("/");
        
			_FileName=GetTempFileName();
		}

		string GetTempFileName()
		{
			int x=0;
			while (true)
			{
				string tempFile=String.Format(@"thumbs\MPTemp{0}.gif",x);
				string tempFile2=String.Format(@"thumbs\MPTemp{0}.jpg",x);
				string tempFile3=String.Format(@"thumbs\MPTemp{0}.bmp",x);
				if (!System.IO.File.Exists(tempFile) && 
					!System.IO.File.Exists(tempFile2) &&
					!System.IO.File.Exists(tempFile3))
				{
					return tempFile;
				}
				++x;
			}
		}
      
      
		public string FileName
		{
			get {return _FileName;}
			set {_FileName=value;}
		}
      
		public string URL
		{
			get { return _URL;}
			set {_URL=value;}
		}

		public int CacheTime
		{
			get { return _CacheTime;}
			set { _CacheTime=value;}
		}

		public bool ShouldDownLoad
		{
			get 
			{
				TimeSpan ts=DateTime.Now - _DateDownloaded;
				if (ts.TotalSeconds > CacheTime)
				{
					return true;
				}
				return false;
			}
		}

		public bool Download()
		{
			using (WebClient client = new WebClient())
			{
				try
				{
					try
					{
						System.IO.File.Delete(FileName);
					}
					catch(Exception)
					{
						Log.Write("DownloadedImage:Download() Delete failed:{0}", FileName);
					}

					client.DownloadFile(URL, FileName);
					try
					{
						string strExt="";
						string strContentType=client.ResponseHeaders["Content-type"].ToLower();
						if (strContentType.IndexOf("gif")>=0) strExt=".gif";
						if (strContentType.IndexOf("jpg")>=0) strExt=".jpg";
						if (strContentType.IndexOf("jpeg")>=0) strExt=".jpg";
						if (strContentType.IndexOf("bmp")>=0) strExt=".bmp";
						if (strExt.Length>0)
						{
							string strNewFile=System.IO.Path.ChangeExtension(FileName,strExt);
							if (!strNewFile.ToLower().Equals(FileName.ToLower()))
							{
								try
								{
									System.IO.File.Delete(strNewFile);
								}
								catch(Exception)
								{
									Log.Write("DownloadedImage:Download() Delete failed:{0}", strNewFile);
								}
								System.IO.File.Move(FileName,strNewFile);
								FileName=strNewFile;
							}
						}
					}
					catch(Exception)
					{
						Log.Write("DownloadedImage:Download() DownloadFile failed:{0}->{1}", URL,FileName);

					}
					_DateDownloaded=DateTime.Now;
					return true;
				} 
				catch(Exception ex)
				{
					Log.Write("download failed:{0}", ex.Message);
				}
			}
			return false;
		}
	}
}
