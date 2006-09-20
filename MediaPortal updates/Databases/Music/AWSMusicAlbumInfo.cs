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
