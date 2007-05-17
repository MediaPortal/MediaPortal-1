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
    class SettingsViewModel
    {
        SelectCommand _selectCommand;
        SectionDatabaseModel _dataModel;
        SectionCollectionView _Items;
        public SettingsViewModel()
        {
            _dataModel = new SectionDatabaseModel(this);
            _Items = new SectionCollectionView(_dataModel);
            ServiceScope.Get<IMessageBroker>().Register(this);
        }

        public CollectionView Items
        {
            get
            {
                if (_Items == null)
                    _Items = new SectionCollectionView(_dataModel);
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

        private class SelectCommand : ICommand
        {
            private SettingsViewModel _viewModel;

            public SelectCommand(SettingsViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                int Index = (int)parameter;
                if (Index >= 0)
                {
                    MediaSettings settings = new MediaSettings();
                    ServiceScope.Get<ISettingsManager>().Load(settings);
                    settings.Section = _viewModel._dataModel.GetSection(Index);
                    ServiceScope.Get<ISettingsManager>().Save(settings);
                }
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }
        }

    }

    #region SectionDatabaseModel class
    class SectionDatabaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        SettingsViewModel _viewModel;
        IMediaLibrary Library;
        List<MediaModel> _Items = new List<MediaModel>();

        public IList Videos
        {
            get { return _Items; }
        }

        public SectionDatabaseModel(SettingsViewModel model)
        {
            _viewModel = model;
            Library = ServiceScope.Get<IMediaLibrary>();
            Refresh();
        }

        public void Refresh()
        {
            _Items.Clear();
            for (int i = 0; i < Library.SectionCount; i++)
            {
                MediaModel Section = new MediaModel();
                Section.Caption = GetSection(i);
                _Items.Add(Section);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Items"));
            }
        }
        public string GetSection(int i)
        {
            return Library.Sections(i);
        }
    }
    #endregion

    #region SectionCollectionView class
    /// <summary>
    /// This class represents the video view.
    /// </summary>
    class SectionCollectionView : ListCollectionView
    {
        private SectionDatabaseModel _model;
        public SectionCollectionView(SectionDatabaseModel model)
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
