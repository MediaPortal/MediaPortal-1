#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using MediaPortal.Music.Database;

namespace MediaPortal.Music.Amazon
{
    public class AWSMusicAlbumInfo : IDisposable   
    {
        private bool _Disposed = false;
        private string _AlbumArtist = string.Empty;
        private string _AlbumTitle = string.Empty;
        private string _ReleaseDate = string.Empty;
        private string _Genre = string.Empty;
        private string _Review = string.Empty;

        private Image _AlbumArtImage = null;

        #region Properties

        public string AlbumArtist
        {
            get{return _AlbumArtist;}
        }

        public string AlbumTitle
        {
            get{return _AlbumTitle;}
        }

        public string ReleaseDate
        {
            get { return _ReleaseDate; }
        }

        public string Genre
        {
            get { return _Genre; }
        }

        public string Review
        {
            get { return _Review; }
        } 
        
        public Image AlbumArtImage
        {
            get{return _AlbumArtImage;}
        }

        #endregion

        public AWSMusicAlbumInfo(string artist, string album, string releaseDate, string genre, string review, Image image)
        {
            _AlbumArtist = artist;
            _AlbumTitle = album;
            _ReleaseDate = releaseDate;
            _Genre = genre;
            _Review = review;

            _AlbumArtImage = image;
        }

        public AWSMusicAlbumInfo(string artist, string album, string imageURL)
        {
            _AlbumArtist = artist;
            _AlbumTitle = album;
            _AlbumArtImage = GetImageFromURL(imageURL);
        }

        #region IDisposable Members
   
        public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

        private void Dispose(bool disposeManagedResources)
		{
			if(!_Disposed)
			{
                _Disposed = true;

                if(disposeManagedResources)
				{
                    if (_AlbumArtImage != null)
                    {
                        _AlbumArtImage.Dispose();
                        _AlbumArtImage = null;
                    }
				}
			}
		}

		#endregion

        public static System.Drawing.Image GetImageFromURL(string sURL)
        {
            System.Net.WebRequest webReq = null;
            webReq = System.Net.WebRequest.Create(sURL);
            System.Net.WebResponse webResp = webReq.GetResponse();
            System.Drawing.Image img = System.Drawing.Image.FromStream(webResp.GetResponseStream());

            return img;
        }
    }
}
