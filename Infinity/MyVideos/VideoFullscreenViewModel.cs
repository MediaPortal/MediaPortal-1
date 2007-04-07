using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MyVideos
{
    public class VideoFullscreenViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Window _window;
        private Page _page;

        private VideoDatabaseModel _dataModel;

        public VideoFullscreenViewModel(Page page)
        {
            _dataModel = new VideoDatabaseModel();

            _page = page;
            _window = Window.GetWindow(_page);
        }
    }
}
