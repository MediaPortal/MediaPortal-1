using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity;
using ProjectInfinity.Thumbnails;
using ProjectInfinity.Settings;
using ProjectInfinity.Playlist;
using MediaLibrary;
using System.ComponentModel;
using ProjectInfinity.Controls;

namespace MediaModule
{
    public class MediaModel : INotifyPropertyChanged
    {
        #region Members

        string _Caption;
        string _Image;
        IMLItem _LibraryItem;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructors
        public MediaModel()
        {
            Init();
        }

        void Init()
        {
            //ServiceScope.Get<IThumbnailBuilder>().OnThumbnailGenerated += new ThumbNailGenerateHandler(VideoModel_OnThumbnailGenerated);
        }
        #endregion

        #region properties
        public IMLItem LibraryItem
        {
            get{return _LibraryItem;}
            set{_LibraryItem = value;}
        }

        public string Caption
        {
            get { return _Caption; }
            set { _Caption = value; }
        }

        public string Title
        {
            get { return _Caption; }
            set { _Caption = value; }
        }

        public string Image
        {
            get{return _Image;}
            set { _Image = value; }
        }
        #endregion
    }
}