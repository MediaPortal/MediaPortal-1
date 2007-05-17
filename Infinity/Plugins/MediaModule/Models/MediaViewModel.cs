using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using ProjectInfinity;
using ProjectInfinity.Localisation;
using System.IO;
using System.Collections;
using System.Windows.Data;
using System.Windows.Threading;
using Dialogs;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using System.Windows.Media;
using System.Windows.Navigation;
using ProjectInfinity.Playlist;
using ProjectInfinity.Settings;
using ProjectInfinity.Navigation;
using ProjectInfinity.Messaging;
using Dialogs;
using MediaLibrary;


namespace MediaModule
{
    public class MediaViewModel : INotifyPropertyChanged
    {
        #region Members

        IMLItem _SelectedItem;
        IMLHashItem _Data;
        ICommand _selectCommand;
        ICommand _backCommand;
        MediaCollectionView _Items;
        MediaDatabaseModel _dataModel;



        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructors

        public MediaViewModel()
        {
            _dataModel = new MediaDatabaseModel(this);
            _Items = new MediaCollectionView(_dataModel);
            ServiceScope.Get<IMessageBroker>().Register(this);
        }

        #endregion

        #region public void ChangeProperty(string propertyName)
        /// <summary>
        /// Notifies subscribers that property has been changed
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void ChangeProperty(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region public IMLHashItem Data
        /// <summary>
        /// Get/Sets the Data of the MediaViewModel
        /// </summary>
        /// <value></value>
        public IMLHashItem Data
        {
            get { return _Data; }
            set { _Data = value; }
        }
        #endregion

        public CollectionView Items
        {
            get
            {
                if (_Items == null)
                    _Items = new MediaCollectionView(_dataModel);
                return _Items;
            }
        }

        public string DateLabel
        {
            get { return DateTime.Now.ToString("dd-MM HH:mm"); }
        }

        public Window Window
        {
            get { return ServiceScope.Get<INavigationService>().GetWindow(); }
        }

        public void Dispose()
        {
        }

        public ICommand Select
        {
            get
            {
                if (_selectCommand == null)
                    _selectCommand = new SelectCommand(this);
                return _selectCommand;
            }
        }

        public ICommand Back
        {
            get
            {
                if (_backCommand == null)
                    _backCommand = new BackCommand(this);

                return _backCommand;
            }
        }

        private class SelectCommand : ICommand
        {
            private MediaViewModel _viewModel;

            public SelectCommand(MediaViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                int Index = (int)parameter;
                if(Index >= 0)
                    if (!_viewModel._dataModel.Advance(Index))
                        ;//If you can't advance, then launch the item
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }
        }
        
        private class BackCommand : ICommand
        {
            private MediaViewModel _viewModel;

            public BackCommand(MediaViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                if (!_viewModel._dataModel.Back())
                    ;//If you can't go back, close the plugin...how?
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }
        }

    }
    #region MediaDatabaseModel class
    class MediaDatabaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        MediaViewModel _viewModel;
        IMediaLibrary Library;
        IMLSection Section;
        IMLViewNavigator Navigator;
        List<MediaModel> _Items = new List<MediaModel>();

        public IList Videos
        {
            get { return _Items; }
        }

        public MediaDatabaseModel(MediaViewModel model)
        {
            _viewModel = model;
            Library = ServiceScope.Get<IMediaLibrary>();
            Section = Library.FindSection("Music", true);
            Navigator = Section.GetViewNavigator();
            Refresh();
        }

        public bool Advance(int Index)
        {
            if (Navigator.Select(Index))
            {
                Refresh();
                return true;
            }
            return false;
        }

        public bool Back()
        {
            if (Navigator.Back())
            {
                Refresh();
                return true;
            }
            return false;
        }

        public void Refresh()
        {
            _Items.Clear();
            for (int i = 0; i < Navigator.Count; i++)
            {
                MediaModel Video = new MediaModel();
                Video.Caption = Navigator.Choices(i);
                Video.Image = Navigator.Images(i);
                if (Navigator.Items != null)
                    Video.LibraryItem = Navigator.Items[i];
                _Items.Add(Video);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Items"));
            }
        }
    }
    #endregion

    #region MediaCollectionView class
    /// <summary>
    /// This class represents the video view.
    /// </summary>
    class MediaCollectionView : ListCollectionView
    {
        private MediaDatabaseModel _model;
        public MediaCollectionView(MediaDatabaseModel model)
            : base(model.Videos)
        {
            _model = model;
            _model.PropertyChanged += new PropertyChangedEventHandler(_model_PropertyChanged);
        }

        void _model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        }
    }
    #endregion
}
