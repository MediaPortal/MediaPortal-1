using System;
using System.Collections;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TvDatabase;
using TvControl;
using Dialogs;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;

namespace MyTv
{
  public class TvScheduledViewModel : TvBaseViewModel
  {
    #region variables
    /// <summary>
    /// Different ways the recordings view can be sorted
    /// </summary>
    public enum SortType
    {
      Duration,
      Channel,
      Date,
      Title
    };
    /// <summary>
    /// Different views on the recordings
    /// </summary>
    public enum ViewType
    {
      List,
      Icon
    };
    ScheduleCollectionView _scheduleView;
    EpisodeCollectionView _episodesView;
    ScheduleDatabaseModel _dataModel;
    ViewType _viewMode = ViewType.Icon;

    ICommand _sortCommand;
    ICommand _viewCommand;
    ICommand _cleanUpCommand;
    ICommand _deleteCommand;
    ICommand _newCommand;
    ICommand _quickRecordCommand;
    ICommand _advancedRecordCommand;
    ICommand _searchTitleCommand;
    ICommand _searchGenreCommand;
    ICommand _searchKeywordCommand;
    ICommand _contextMenuCommand;
    ICommand _recordProgramCommand;
    ICommand _recordProgramAdvancedCommand;

    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvScheduledViewModel"/> class.
    /// </summary>
    /// <param name="page">The page.</param>
    public TvScheduledViewModel(Page page)
      : base(page)
    {
      //create a new data model
      _dataModel = new ScheduleDatabaseModel();

      //store page & window
      _scheduleView = new ScheduleCollectionView(_dataModel);
      _episodesView = new EpisodeCollectionView(_dataModel);
    }
    #endregion

    #region properties

    /// <summary>
    /// Gets the data model.
    /// </summary>
    /// <value>The data model.</value>
    public ScheduleDatabaseModel DataModel
    {
      get
      {
        return _dataModel;
      }
    }

    public ProgramModel CurrentProgram
    {
      get
      {
        return _dataModel.CurrentProgram;
      }
      set
      {
        _dataModel.CurrentProgram = value;
      }
    }
    /// <summary>
    /// Gets the localized-label for the programinfo header
    /// </summary>
    /// <value>The localized label.</value>
    public string ProgramInfoHeaderLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 46);//program info
      }
    }
    public string RecordLabel
    {
      get
      {
        if (_dataModel.CurrentProgram != null)
        {
          if (_dataModel.CurrentProgram.IsRecorded)
          {
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 53);//Dont Record
          }
        }
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 13);//Record
      }
    }
    public string KeepUntilLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 47);//Keep until
      }
    }
    public string AlertMeLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 48);//Alert me
      }
    }

    public string QualityLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 49);//Quality setting
      }
    }
    public string EpisodesLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 50);//Episodes management
      }
    }
    public string PreRecordLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 51);//Pre-record
      }
    }
    public string PostRecordLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 52);//Post-record
      }
    }


    /// <summary>
    /// Gets the localized-label for the header
    /// </summary>
    /// <value>The localized label.</value>
    public override string HeaderLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 111);// "scheduled";
      }
    }

    /// <summary>
    /// Returns the ListViewCollection containing the schedules
    /// </summary>
    /// <value>The recordings.</value>
    public CollectionView Schedules
    {
      get
      {
        if (_scheduleView == null)
        {
          _scheduleView = new ScheduleCollectionView(_dataModel);
        }
        return _scheduleView;
      }
    }
    /// <summary>
    /// Returns the ListViewCollection containing the upcoming episodes
    /// </summary>
    /// <value>The recordings.</value>
    public CollectionView Episodes
    {
      get
      {
        if (_episodesView == null)
        {
          _episodesView = new EpisodeCollectionView(_dataModel);
        }
        return _episodesView;
      }
    }

    /// <summary>
    /// Gets the localized-label for the Sort button
    /// </summary>
    /// <value>The localized label.</value>
    public string SortLabel
    {
      get
      {

        switch (_scheduleView.SortMode)
        {
          case SortType.Channel:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 84);//"Sort:Channel";
          case SortType.Date:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 85);//"Sort:Date";
          case SortType.Duration:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 86);//"Sort:Duration";
          case SortType.Title:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 88);//"Sort:Title";
        }
        return "";
      }
    }
    /// <summary>
    /// Gets the localized-label for the View button
    /// </summary>
    /// <value>The localized label.</value>
    public string ViewLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 79);//"View";
      }
    }
    /// <summary>
    /// Gets the localized-label for the Cleanup button
    /// </summary>
    /// <value>The localized label.</value>
    public string CleanUpLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 82);//Cleanup
      }
    }
    /// <summary>
    /// Gets the localized-label for the New button
    /// </summary>
    /// <value>The localized label.</value>
    public string NewLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 123);//New
      }
    }
    /// <summary>
    /// Gets the localized-label for the Priorities button
    /// </summary>
    /// <value>The localized label.</value>
    public string PrioritiesLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 124);//Priorities
      }
    }
    /// <summary>
    /// Gets the localized-label for the Conflicts button
    /// </summary>
    /// <value>The localized label.</value>
    public string ConflictsLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 125);//Conflicts
      }
    }
    /// <summary>
    /// Gets the localized-label for the new schedule header label
    /// </summary>
    /// <value>The localized label.</value>
    public string NewScheduleLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 40);//new schedule
      }
    }
    /// <summary>
    /// Gets the quick record label.
    /// </summary>
    /// <value>The quick record label.</value>
    public string QuickRecordLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 41);//Quick Record
      }
    }
    /// <summary>
    /// Gets the advanced record label.
    /// </summary>
    /// <value>The advanced record label.</value>
    public string AdvancedRecordLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 42);//Advanced Record
      }
    }
    /// <summary>
    /// Gets the search title label.
    /// </summary>
    /// <value>The search title label.</value>
    public string SearchTitleLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 43);//Search by title
      }
    }
    /// <summary>
    /// Gets the search keyword label.
    /// </summary>
    /// <value>The search keyword label.</value>
    public string SearchKeywordLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 44);//Search by keyword
      }
    }
    /// <summary>
    /// Gets the search genre label.
    /// </summary>
    /// <value>The search genre label.</value>
    public string SearchGenreLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 45);//Search by genre
      }
    }

    /// <summary>
    /// Gets or sets the view mode.
    /// </summary>
    /// <value>The view mode.</value>
    public ViewType ViewMode
    {
      get
      {
        return _viewMode;
      }
      set
      {
        if (_viewMode != value)
        {
          _viewMode = value;
          ChangeProperty("ViewModeType");
        }
      }
    }
    public string ViewModeType
    {
      get
      {
        switch (_viewMode)
        {
          case ViewType.Icon:
            return "Icon";
          default:
            return "List";
        }
      }
    }
    #endregion

    #region commands
    /// <summary>
    /// Returns a ICommand for sorting
    /// </summary>
    /// <value>The command.</value>
    public ICommand Sort
    {
      get
      {
        if (_sortCommand == null)
        {
          _sortCommand = new SortCommand(this);
        }
        return _sortCommand;
      }
    }

    /// <summary>
    /// Returns a ICommand for changing the view mode.
    /// </summary>
    /// <value>The command.</value>
    public ICommand View
    {
      get
      {
        if (_viewCommand == null)
        {
          _viewCommand = new ViewCommand(this);
        }
        return _viewCommand;
      }
    }

    /// <summary>
    /// Returns a ICommand for cleaning up watched recordings
    /// </summary>
    /// <value>The command.</value>
    public ICommand CleanUp
    {
      get
      {
        if (_cleanUpCommand == null)
        {
          _cleanUpCommand = new CleanUpCommand(this);
        }
        return _cleanUpCommand;
      }
    }

    /// <summary>
    /// Returns a ICommand for deleting a schedule
    /// </summary>
    /// <value>The command.</value>
    public ICommand Delete
    {
      get
      {
        if (_deleteCommand == null)
        {
          _deleteCommand = new DeleteCommand(this);
        }
        return _deleteCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for creating a schedule
    /// </summary>
    /// <value>The command.</value>
    public ICommand New
    {
      get
      {
        if (_newCommand == null)
        {
          _newCommand = new NewCommand(this);
        }
        return _newCommand;
      }
    }

    /// <summary>
    /// Returns a ICommand for showing the context menu
    /// </summary>
    /// <value>The command.</value>
    public ICommand ContextMenu
    {
      get
      {
        if (_contextMenuCommand == null)
        {
          _contextMenuCommand = new ContextMenuCommand(this);
        }
        return _contextMenuCommand;
      }
    }
    /// <summary>
    /// Gets the quick record command
    /// </summary>
    /// <value>The quick record.</value>
    public ICommand QuickRecord
    {
      get
      {
        if (_quickRecordCommand == null)
        {
          _quickRecordCommand = new QuickRecordCommand(this);
        }
        return _quickRecordCommand;
      }
    }
    /// <summary>
    /// Gets the advanced record command
    /// </summary>
    /// <value>The advanced record.</value>
    public ICommand AdvancedRecord
    {
      get
      {
        if (_advancedRecordCommand == null)
        {
          _advancedRecordCommand = new AdvancedRecordCommand(this);
        }
        return _advancedRecordCommand;
      }
    }
    /// <summary>
    /// Gets the search title command.
    /// </summary>
    /// <value>The search title.</value>
    public ICommand SearchTitle
    {
      get
      {
        if (_searchTitleCommand == null)
        {
          _searchTitleCommand = new SearchTitleCommand(this);
        }
        return _searchTitleCommand;
      }
    }
    /// <summary>
    /// Gets the search genre command.
    /// </summary>
    /// <value>The search genre.</value>
    public ICommand SearchGenre
    {
      get
      {
        if (_searchGenreCommand == null)
        {
          _searchGenreCommand = new SearchGenreCommand(this);
        }
        return _searchGenreCommand;
      }
    }
    /// <summary>
    /// Gets the search keyword command.
    /// </summary>
    /// <value>The search keyword.</value>
    public ICommand SearchKeyword
    {
      get
      {
        if (_searchKeywordCommand == null)
        {
          _searchKeywordCommand = new SearchKeywordCommand(this);
        }
        return _searchKeywordCommand;
      }
    }

    public ICommand RecordProgram
    {
      get
      {
        if (_recordProgramCommand == null)
        {
          _recordProgramCommand = new RecordProgramCommand(this);
        }
        return _recordProgramCommand;
      }
    }
    public ICommand RecordProgramAdvanced
    {
      get
      {
        if (_recordProgramAdvancedCommand == null)
        {
          _recordProgramAdvancedCommand = new RecordProgramAdvancedCommand(this);
        }
        return _recordProgramAdvancedCommand;
      }
    }
    #endregion

    #region Commands subclasses
    #region base command class
    public abstract class RecordedCommand : ICommand
    {
      protected TvScheduledViewModel _viewModel;
      public event EventHandler CanExecuteChanged;

      public RecordedCommand(TvScheduledViewModel viewModel)
      {
        _viewModel = viewModel;
      }

      public abstract void Execute(object parameter);

      public virtual bool CanExecute(object parameter)
      {
        return true;
      }

      protected void OnCanExecuteChanged()
      {
        if (this.CanExecuteChanged != null)
        {
          this.CanExecuteChanged(this, EventArgs.Empty);
        }
      }
    }
    #endregion

    #region sort command class
    /// <summary>
    /// SortCommand changes the way the view gets sorted
    /// </summary>
    public class SortCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="SortCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public SortCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        //show dialog menu with all sorting options
        MpMenu dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Items.Clear();
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
        dlgMenu.SubTitle = "";
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 97)/*Duration*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 2)/*Channel*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 73)/*Date*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 98)/*Title*/));
        ScheduleCollectionView view = (ScheduleCollectionView)_viewModel.Schedules;
        dlgMenu.SelectedIndex = (int)view.SortMode;
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0) return;//nothing selected

        //tell the view to sort
        view.SortMode = (TvScheduledViewModel.SortType)dlgMenu.SelectedIndex;

        //and tell the model that the sort property is changed
        _viewModel.ChangeProperty("SortLabel");
        _viewModel.ChangeProperty("Schedules");
      }
    }
    #endregion

    #region view command class
    /// <summary>
    /// ViewCommand changes the way each listbox item gets displayed
    /// </summary>
    public class ViewCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="ViewCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public ViewCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        //change the viewmode
        switch (_viewModel.ViewMode)
        {
          case ViewType.Icon:
            _viewModel.ViewMode = ViewType.List;
            break;
          case ViewType.List:
            _viewModel.ViewMode = ViewType.Icon;
            break;
        }
      }
    }
    #endregion

    #region cleanup command class
    /// <summary>
    /// Cleanup command will delete recordings which have been watched
    /// </summary>
    public class CleanUpCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public CleanUpCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        int iCleaned = 0;
        IList itemlist = Schedule.ListAll();
        foreach (Schedule rec in itemlist)
        {
          if (rec.IsDone() || rec.Canceled != Schedule.MinSchedule)
          {
            iCleaned++;
            _viewModel.DataModel.Delete(rec.IdSchedule);
          }
        }
        MpDialogOk dlgMenu = new MpDialogOk();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 84);//"Cleanup";
        dlgMenu.Header = "";
        dlgMenu.Content = String.Format(ServiceScope.Get<ILocalisation>().ToString("mytv", 116)/*Cleaned up {0} schedules "*/, iCleaned);
        dlgMenu.ShowDialog();
        _viewModel.ChangeProperty("Schedules");
      }
    }
    #endregion

    #region Delete command class
    /// <summary>
    /// Delete command will delete a recoring
    /// </summary>
    public class DeleteCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public DeleteCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        _viewModel.ChangeProperty("Schedules");
      }
    }
    #endregion

    #region New command class
    /// <summary>
    /// Delete command will create a recoring
    /// </summary>
    public class NewCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public NewCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvNewSchedule.xaml", UriKind.Relative));
      }
    }
    #endregion

    #region ContextMenu command class
    /// <summary>
    /// ContextMenuCommand will show the context menu
    /// </summary> 
    public class ContextMenuCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public ContextMenuCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        ScheduleModel item = parameter as ScheduleModel;
        if (item == null) return;
        Schedule rec = item.Schedule;
        if (rec == null) return;

        MpMenu dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Items.Clear();
        dlgMenu.Header = "Menu";
        dlgMenu.SubTitle = "";
        dlgMenu.SelectedIndex = (int)0;
        if (dlgMenu.SelectedIndex < 0) return;//nothing selected
        int[] options = new int[10];
        int option = 0;
        if (rec.Series == false)
        {
          dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 56)/*Delete*/));
          options[option++] = 618;
        }
        else
        {
          dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 112)/*Cancel this show*/));
          options[option++] = 981;
          dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 113)/*Delete this entire recording*/));
          options[option++] = 982;
          dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 50)/*Episodes management*/));
          options[option++] = 888;
        }
        VirtualCard card;
        TvServer server = new TvServer();
        if (server.IsRecordingSchedule(rec.IdSchedule, out card))
        {
          dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 114)/*Play recording from beginning*/));
          options[option++] = 979;
          dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 115)/*Play recording from live point*/));
          options[option++] = 980;
        }
        else
        {
          dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 49)/*Quality settings*/));
          options[option++] = 882;
        }
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 94)/*Settings*/));
        options[option++] = 1048;
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex == -1) return;

        string fileName = "";
        if (server.IsRecordingSchedule(rec.IdSchedule, out card))
        {
          fileName = card.RecordingFileName;
        }
        switch (options[dlgMenu.SelectedIndex])
        {
          case 888:////Episodes management
            //TvPriorities.OnSetEpisodesToKeep(rec);
            break;

          case 1048:////settings
            TvScheduleInfo infopage = new TvScheduleInfo(rec);
            ServiceScope.Get<INavigationService>().Navigate(infopage);
            //TVProgramInfo.CurrentRecording = item.MusicTag as Schedule;
            //GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO);
            return;
          case 882:////Quality settings
            //GUITVPriorities.OnSetQuality(rec);
            break;

          case 981: //Cancel this show
            {
              if (server.IsRecordingSchedule(rec.IdSchedule, out card))
              {

                MpDialogYesNo dlgYesNo = new MpDialogYesNo();
                dlgYesNo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlgYesNo.Owner = _viewModel.Window;
                dlgYesNo.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
                dlgYesNo.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 57);// "Delete this recording? This schedule is recording. If you delete the schedule then the recording is stopped.";
                dlgYesNo.ShowDialog();
                if (dlgYesNo.DialogResult == DialogResult.No) return;
                server.StopRecordingSchedule(rec.IdSchedule);
                _viewModel.DataModel.CancelSchedule(rec.IdSchedule, rec.StartTime);
              }
              else
              {
                server.StopRecordingSchedule(rec.IdSchedule);
                _viewModel.DataModel.CancelSchedule(rec.IdSchedule, rec.StartTime);
              }
            }
            break;

          case 982: //Delete series recording
            goto case 618;

          case 618: // delete entire recording
            {
              if (server.IsRecordingSchedule(rec.IdSchedule, out card))
              {
                MpDialogYesNo dlgYesNo = new MpDialogYesNo();
                dlgYesNo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlgYesNo.Owner = _viewModel.Window;
                dlgYesNo.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
                dlgYesNo.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 57);// "Delete this recording? This schedule is recording. If you delete the schedule then the recording is stopped.";
                dlgYesNo.ShowDialog();
                if (dlgYesNo.DialogResult == DialogResult.No) return;
                server.StopRecordingSchedule(rec.IdSchedule);
                _viewModel.DataModel.Delete(rec.IdSchedule);
              }
              else
              {
                _viewModel.DataModel.Delete(rec.IdSchedule);

              }
            }
            break;

          case 979: // Play recording from beginning
            {
              ICommand cmd = _viewModel.Play;
              cmd.Execute(new PlayCommand.PlayParameter(fileName, null, false));
            }
            return;

          case 980: // Play recording from live point
            {
              ICommand cmd = _viewModel.Play;
              cmd.Execute(new PlayCommand.PlayParameter(fileName, null));
            }
            break;
        }
      }
    }
    #endregion

    #region QuickRecord command class
    /// <summary>
    /// QuickRecord Command
    /// </summary> 
    public class QuickRecordCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="QuickRecord"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public QuickRecordCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        MpMenuWithLogo dlgLogoMenu = new MpMenuWithLogo();
        dlgLogoMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgLogoMenu.Owner = _viewModel.Window;
        dlgLogoMenu.Items.Clear();
        dlgLogoMenu.Header = "Channel";
        dlgLogoMenu.SubTitle = "";

        IList channels = ServiceScope.Get<ITvChannelNavigator>().CurrentGroup.ReferringGroupMap();
        foreach (GroupMap chan in channels)
        {
          string logo = String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.GetLogoFileName(chan.ReferencedChannel().Name));
          if (!System.IO.File.Exists(logo))
            logo = "";
          dlgLogoMenu.Items.Add(new DialogMenuItem(logo, chan.ReferencedChannel().Name, "", ""));
        }
        dlgLogoMenu.ShowDialog();
        if (dlgLogoMenu.SelectedIndex < 0) return;

        Channel selectedChannel = ((GroupMap)channels[dlgLogoMenu.SelectedIndex]).ReferencedChannel() as Channel;
        /*
        dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = w;
        dlgMenu.Header = "Type";
        dlgMenu.Items.Add(new DialogMenuItem("Once"));
        dlgMenu.Items.Add(new DialogMenuItem("Daily"));
        dlgMenu.Items.Add(new DialogMenuItem("Weekly"));
        dlgMenu.Items.Add(new DialogMenuItem("Every time on this channel"));
        dlgMenu.Items.Add(new DialogMenuItem("Every time on every channel"));
        dlgMenu.Items.Add(new DialogMenuItem("Sat-Sun"));
        dlgMenu.Items.Add(new DialogMenuItem("Mon-Fri"));
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0) return;*/

        Schedule rec = new Schedule(selectedChannel.IdChannel, "", Schedule.MinSchedule, Schedule.MinSchedule);
        TvBusinessLayer layer = new TvBusinessLayer();
        rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
        rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
        rec.ScheduleType = (int)ScheduleRecordingType.Once;

        DateTime dtNow = DateTime.Now;
        int day;
        day = 0;


        MpMenu dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Header = "Start";

        //time
        //int no = 0;
        int hour, minute, steps;
        steps = 15;
        dlgMenu.Items.Add(new DialogMenuItem("00:00"));
        for (hour = 0; hour <= 23; hour++)
        {
          for (minute = 0; minute < 60; minute += steps)
          {
            if (hour == 0 && minute == 0) continue;
            string time = "";
            if (hour < 10) time = "0" + hour.ToString();
            else time = hour.ToString();
            time += ":";
            if (minute < 10) time = time + "0" + minute.ToString();
            else time += minute.ToString();

            //if (hour < 1) time = String.Format("{0} {1}", minute, GUILocalizeStrings.Get(3004));
            dlgMenu.Items.Add(new DialogMenuItem(time));
          }
        }
        // pre-select the current time
        dlgMenu.SelectedIndex = (DateTime.Now.Hour * (60 / steps)) + (Convert.ToInt16(DateTime.Now.Minute / steps));
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex == -1) return;

        int mins = (dlgMenu.SelectedIndex) * steps;
        hour = (mins) / 60;
        minute = ((mins) % 60);



        dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Header = "Duration";
        //duration
        for (float hours = 0.5f; hours <= 24f; hours += 0.5f)
        {
          dlgMenu.Items.Add(new DialogMenuItem(String.Format("{0} hours", hours.ToString("f2"))));
        }
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex == -1) return;

        int duration = (dlgMenu.SelectedIndex + 1) * 30;


        dtNow = DateTime.Now.AddDays(day);
        rec.StartTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, hour, minute, 0, 0);
        rec.EndTime = rec.StartTime.AddMinutes(duration);
        rec.ProgramName = "Manual" + " (" + rec.ReferencedChannel().Name + ")";
        rec.Persist();
        TvServer server = new TvServer();
        server.OnNewSchedule();
        ServiceScope.Get<INavigationService>().GoBack();
      }
    }
    #endregion

    #region AdvancedRecord command class
    /// <summary>
    /// QuickRecord Command
    /// </summary> 
    public class AdvancedRecordCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="AdvancedRecordCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public AdvancedRecordCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        MpMenuWithLogo dlgLogoMenu = new MpMenuWithLogo();
        dlgLogoMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgLogoMenu.Owner = _viewModel.Window;
        dlgLogoMenu.Items.Clear();
        dlgLogoMenu.Header = "Channel";
        dlgLogoMenu.SubTitle = "";

        IList channels = ServiceScope.Get<ITvChannelNavigator>().CurrentGroup.ReferringGroupMap();
        foreach (GroupMap chan in channels)
        {
          string logo = String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.GetLogoFileName(chan.ReferencedChannel().Name));
          if (!System.IO.File.Exists(logo))
            logo = "";
          dlgLogoMenu.Items.Add(new DialogMenuItem(logo, chan.ReferencedChannel().Name, "", ""));
        }
        dlgLogoMenu.ShowDialog();
        if (dlgLogoMenu.SelectedIndex < 0) return;

        Channel selectedChannel = ((GroupMap)channels[dlgLogoMenu.SelectedIndex]).ReferencedChannel() as Channel;

        MpMenu dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Header = "Type";
        dlgMenu.Items.Add(new DialogMenuItem("Once"));
        dlgMenu.Items.Add(new DialogMenuItem("Daily"));
        dlgMenu.Items.Add(new DialogMenuItem("Weekly"));
        dlgMenu.Items.Add(new DialogMenuItem("Every time on this channel"));
        dlgMenu.Items.Add(new DialogMenuItem("Every time on every channel"));
        dlgMenu.Items.Add(new DialogMenuItem("Sat-Sun"));
        dlgMenu.Items.Add(new DialogMenuItem("Mon-Fri"));
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0) return;

        Schedule rec = new Schedule(selectedChannel.IdChannel, "", Schedule.MinSchedule, Schedule.MinSchedule);
        TvBusinessLayer layer = new TvBusinessLayer();
        rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
        rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
        rec.ScheduleType = (int)dlgMenu.SelectedIndex;


        DateTime dtNow = DateTime.Now;
        dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Header = "Date";
        int day;

        for (day = 0; day < 30; day++)
        {
          if (day > 0)
            dtNow = DateTime.Now.AddDays(day);
          dlgMenu.Items.Add(new DialogMenuItem(dtNow.ToLongDateString()));
        }
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex == -1)
          return;
        day = dlgMenu.SelectedIndex;
        dtNow = DateTime.Now.AddDays(day);

        dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Header = "Start";

        //time
        //int no = 0;
        int hour, minute, steps;
        steps = 15;
        dlgMenu.Items.Add(new DialogMenuItem("00:00"));
        for (hour = 0; hour <= 23; hour++)
        {
          for (minute = 0; minute < 60; minute += steps)
          {
            if (hour == 0 && minute == 0) continue;
            string time = "";
            if (hour < 10) time = "0" + hour.ToString();
            else time = hour.ToString();
            time += ":";
            if (minute < 10) time = time + "0" + minute.ToString();
            else time += minute.ToString();

            //if (hour < 1) time = String.Format("{0} {1}", minute, GUILocalizeStrings.Get(3004));
            dlgMenu.Items.Add(new DialogMenuItem(time));
          }
        }
        // pre-select the current time
        dlgMenu.SelectedIndex = (DateTime.Now.Hour * (60 / steps)) + (Convert.ToInt16(DateTime.Now.Minute / steps));
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex == -1) return;

        int mins = (dlgMenu.SelectedIndex) * steps;
        hour = (mins) / 60;
        minute = ((mins) % 60);



        dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Header = "Duration";
        //duration
        for (float hours = 0.5f; hours <= 24f; hours += 0.5f)
        {
          dlgMenu.Items.Add(new DialogMenuItem(String.Format("{0} hours", hours.ToString("f2"))));
        }
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex == -1) return;

        int duration = (dlgMenu.SelectedIndex + 1) * 30;


        dtNow = DateTime.Now.AddDays(day);
        rec.StartTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, hour, minute, 0, 0);
        rec.EndTime = rec.StartTime.AddMinutes(duration);
        rec.ProgramName = "Manual" + " (" + rec.ReferencedChannel().Name + ")";
        rec.Persist();
        TvServer server = new TvServer();
        server.OnNewSchedule();
        ServiceScope.Get<INavigationService>().GoBack();
      }
    }
    #endregion

    #region SearchTitle command class
    /// <summary>
    /// SearchTitleCommand Command
    /// </summary> 
    public class SearchTitleCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="SearchTitleCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public SearchTitleCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        TvSearch.SearchMode = TvSearchViewModel.SearchType.Title;
        ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvSearch.xaml", UriKind.Relative));
      }
    }
    #endregion

    #region SearchGenreCommand class
    /// <summary>
    /// SearchGenreCommand Command
    /// </summary> 
    public class SearchGenreCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="SearchGenreCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public SearchGenreCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        TvSearch.SearchMode = TvSearchViewModel.SearchType.Genre;
        ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvSearch.xaml", UriKind.Relative));
      }
    }
    #endregion

    #region SearchKeywordCommand class
    /// <summary>
    /// SearchGenreCommand Command
    /// </summary> 
    public class SearchKeywordCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="SearchKeywordCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public SearchKeywordCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        TvSearch.SearchMode = TvSearchViewModel.SearchType.Description;
        ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvSearch.xaml", UriKind.Relative));
      }
    }
    #endregion

    #region RecordProgramCommand class

    public class RecordProgramCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="RecordProgramCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public RecordProgramCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        if (parameter == null) return;
        ProgramModel program = parameter as ProgramModel;
        Program _program = program.Program;
        Schedule recordingSchedule;
        if (IsRecordingProgram(_program, out  recordingSchedule, false))
        {
          //already recording this program
          if (recordingSchedule.ScheduleType != (int)ScheduleRecordingType.Once)
          {
            MpMenu dlgMenu = new MpMenu();
            dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlgMenu.Owner = _viewModel.Window;
            dlgMenu.Items.Clear();
            dlgMenu.Header = "Menu";
            dlgMenu.SubTitle = "";
            dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 54)/* Delete this recording*/));
            dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 55)/* Delete series recording*/));
            dlgMenu.ShowDialog();
            if (dlgMenu.SelectedIndex == -1)
              return;
            switch (dlgMenu.SelectedIndex)
            {
              case 0: //Delete this recording only
                {
                  if (CheckIfRecording(recordingSchedule))
                  {
                    //delete specific series
                    CanceledSchedule canceledSchedule = new CanceledSchedule(recordingSchedule.IdSchedule, program.StartTime);
                    canceledSchedule.Persist();
                    TvServer server = new TvServer();
                    server.StopRecordingSchedule(recordingSchedule.IdSchedule);
                    server.OnNewSchedule();
                  }
                }
                break;
              case 1: //Delete entire recording
                {
                  if (CheckIfRecording(recordingSchedule))
                  {
                    //cancel recording
                    TvServer server = new TvServer();
                    server.StopRecordingSchedule(recordingSchedule.IdSchedule);
                    recordingSchedule.Delete();
                    server.OnNewSchedule();
                  }
                }
                break;
            }
          }
          else
          {
            if (CheckIfRecording(recordingSchedule))
            {
              TvServer server = new TvServer();
              server.StopRecordingSchedule(recordingSchedule.IdSchedule);
              recordingSchedule.Delete();
              server.OnNewSchedule();
            }
          }
        }
        else
        {
          //not recording this program
          // check if this program is conflicting with any other already scheduled recording
          TvBusinessLayer layer = new TvBusinessLayer();
          Schedule rec = new Schedule(_program.IdChannel, program.Title, program.StartTime, program.EndTime);
          rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
          rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
          //if (SkipForConflictingRecording(rec)) return;

          rec.Persist();
          TvServer server = new TvServer();
          server.OnNewSchedule();
        }
        _viewModel.DataModel.Reload();
        _viewModel.ChangeProperty("RecordLabel");
      }
      protected bool CheckIfRecording(Schedule rec)
      {

        VirtualCard card;
        TvServer server = new TvServer();
        if (!server.IsRecordingSchedule(rec.IdSchedule, out card)) return true;
        MpDialogYesNo dlgMenu = new MpDialogYesNo();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 56);//"Delete";
        dlgMenu.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 57);//"Delete this recording ? This schedule is recording. If you delete the schedule then the recording will be stopped.";
        dlgMenu.ShowDialog();
        if (dlgMenu.DialogResult == DialogResult.Yes)
        {
          return true;
        }
        return false;
      }
      protected bool IsRecordingProgram(Program program, out Schedule recordingSchedule, bool filterCanceledRecordings)
      {
        recordingSchedule = null;
        IList schedules = Schedule.ListAll();
        foreach (Schedule schedule in schedules)
        {
          if (schedule.Canceled != Schedule.MinSchedule) continue;
          if (schedule.IsRecordingProgram(program, filterCanceledRecordings))
          {
            recordingSchedule = schedule;
            return true;
          }
        }
        return false;
      }
    }
    #endregion

    #region RecordProgramAdvancedCommand class

    public class RecordProgramAdvancedCommand : RecordProgramCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="RecordProgramAdvancedCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public RecordProgramAdvancedCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        ProgramModel model = parameter as ProgramModel;
        if (model == null) return;
        Program _program = model.Program;
        if (_program == null)
          return;
        MpMenu dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Items.Clear();
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 13);//""Record";
        dlgMenu.SubTitle = "";
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 58)/* "None"*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 59)/*"Record once"*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 60)/*"Record everytime on this channel"*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 61)/*"Record everytime on every channel"*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 62)/*"Record every week at this time"*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 63)/*"Record every day at this time"*/)); ;
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 64)/*"Record Mon-fri"*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 65)/*"Record Sat-Sun"*/));
        dlgMenu.ShowDialog();

        if (dlgMenu.SelectedIndex < 1) return;

        Schedule rec = new Schedule(_program.IdChannel, _program.Title, _program.StartTime, _program.EndTime);
        switch (dlgMenu.SelectedIndex)
        {
          case 1://once
            rec.ScheduleType = (int)ScheduleRecordingType.Once;
            break;
          case 2://everytime, this channel
            rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnThisChannel;
            break;
          case 3://everytime, all channels
            rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnEveryChannel;
            break;
          case 4://weekly
            rec.ScheduleType = (int)ScheduleRecordingType.Weekly;
            break;
          case 5://daily
            rec.ScheduleType = (int)ScheduleRecordingType.Daily;
            break;
          case 6://Mo-Fi
            rec.ScheduleType = (int)ScheduleRecordingType.WorkingDays;
            break;
          case 7://Record Sat-Sun
            rec.ScheduleType = (int)ScheduleRecordingType.Weekends;
            break;
        }
        //if (SkipForConflictingRecording(rec)) return;

        TvBusinessLayer layer = new TvBusinessLayer();
        rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
        rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
        rec.Persist();
        TvServer server = new TvServer();
        server.OnNewSchedule();

        //check if this program is interrupted (for example by a news bulletin)
        //ifso ask the user if he wants to record the 2nd part also
        IList programs = new ArrayList();
        DateTime dtStart = rec.EndTime.AddMinutes(1);
        DateTime dtEnd = dtStart.AddHours(3);
        programs = layer.GetPrograms(rec.ReferencedChannel(), dtStart, dtEnd);
        if (programs.Count >= 2)
        {
          Program next = programs[0] as Program;
          Program nextNext = programs[1] as Program;
          if (nextNext.Title == rec.ProgramName)
          {
            TimeSpan ts = next.EndTime - next.StartTime;
            if (ts.TotalMinutes <= 40)
            {
              MpDialogYesNo dlgYesNo = new MpDialogYesNo();
              dlgYesNo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
              dlgYesNo.Owner = _viewModel.Window;
              dlgYesNo.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 66);//""Multipart";
              dlgYesNo.Content = String.Format(ServiceScope.Get<ILocalisation>().ToString("mytv", 667)/*This program will be interrupted by {0} Would you like to record the second part also?")*/, next.Title);
              dlgYesNo.ShowDialog();
              if (dlgYesNo.DialogResult == DialogResult.Yes)
              {
                rec = new Schedule(_program.IdChannel, _program.Title, nextNext.StartTime, nextNext.EndTime);

                rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                rec.Persist();
                server.OnNewSchedule();
              }
            }
          }
        }
        _viewModel.DataModel.Reload();
        _viewModel.ChangeProperty("RecordLabel");
      }
    }
    #endregion
    #endregion

    #region ScheduleDatabaseModel class
    /// <summary>
    /// Class representing our database model.
    /// It simply retrieves all recordings from the tv database and 
    /// creates a list of ScheduleModel
    /// </summary>
    public class ScheduleDatabaseModel : INotifyPropertyChanged
    {
      #region variables
      public event PropertyChangedEventHandler PropertyChanged;
      ProgramModel _program;
      List<ScheduleModel> _listSchedules = new List<ScheduleModel>();
      List<ProgramModel> _listEpisodes = new List<ProgramModel>();
      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="ScheduleDatabaseModel"/> class.
      /// </summary>
      public ScheduleDatabaseModel()
      {
        Reload();
      }
      public void CancelSchedule(int idSchedule, DateTime cancelTime)
      {
        CanceledSchedule schedule = new CanceledSchedule(idSchedule, cancelTime);
        schedule.Persist();
        TvServer server = new TvServer();
        server.OnNewSchedule();
        Reload();
        if (PropertyChanged != null)
        {
          PropertyChanged(this, new PropertyChangedEventArgs("Schedules"));
        }
      }
      /// <summary>
      /// Deletes this instance.
      /// </summary>
      public void Delete(int idSchedule)
      {
        TvServer server = new TvServer();
        for (int i = 0; i < _listSchedules.Count; ++i)
        {
          if (_listSchedules[i].Schedule.IdSchedule == idSchedule)
          {
            _listSchedules[i].Schedule.Delete();
            server.OnNewSchedule();
            _listSchedules.RemoveAt(i);
            if (PropertyChanged != null)
            {
              PropertyChanged(this, new PropertyChangedEventArgs("Schedules"));
            }
            break;
          }
        }
      }

      /// <summary>
      /// Refreshes the list with the database.
      /// </summary>
      public void Reload()
      {
        _listSchedules.Clear();
        IList schedules = Schedule.ListAll();

        foreach (Schedule schedule in schedules)
        {
          ScheduleModel item = new ScheduleModel(schedule);
          _listSchedules.Add(item);
        }
        _listEpisodes.Clear();
        if (_program != null)
        {
          TvBusinessLayer layer = new TvBusinessLayer();
          DateTime dtDay = DateTime.Now;
          IList episodes = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(14), _program.Title, null);
          foreach (Program episode in episodes)
          {
            _listEpisodes.Add(new ProgramModel(episode));
          }
        }
        if (PropertyChanged != null)
          PropertyChanged(this, new PropertyChangedEventArgs("Episodes"));
      }

      public ProgramModel CurrentProgram
      {
        get
        {
          return _program;
        }
        set
        {
          _program = value;
          Reload();
        }
      }

      /// <summary>
      /// Gets the recordings.
      /// </summary>
      /// <value>IList containing 0 or more ScheduleModel instances.</value>
      public IList Schedules
      {
        get
        {
          return _listSchedules;
        }
      }
      /// <summary>
      /// Gets the upcoming episodes.
      /// </summary>
      /// <value>IList containing 0 or more ScheduleModel instances.</value>
      public IList Episodes
      {
        get
        {
          return _listEpisodes;
        }
      }
    }
    #endregion

    #region ScheduleCollectionView class
    /// <summary>
    /// This class represents the schedule view
    /// </summary>
    class ScheduleCollectionView : ListCollectionView
    {
      #region variables
      SortType _sortMode = SortType.Date;
      private ScheduleDatabaseModel _model;
      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="ScheduleCollectionView"/> class.
      /// </summary>
      /// <param name="model">The database model.</param>
      public ScheduleCollectionView(ScheduleDatabaseModel model)
        : base(model.Schedules)
      {
        _model = model;
        _model.PropertyChanged += new PropertyChangedEventHandler(onDatabaseChanged);
      }

      void onDatabaseChanged(object sender, PropertyChangedEventArgs e)
      {
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }

      /// <summary>
      /// Gets or sets the sort mode.
      /// </summary>
      /// <value>The sort mode.</value>
      public SortType SortMode
      {
        get
        {
          return _sortMode;
        }
        set
        {
          if (_sortMode != value)
          {
            _sortMode = value;
            this.CustomSort = new ScheduleComparer(_sortMode);
          }
        }
      }
    }
    #endregion

    #region EpisodeCollectionView class
    /// <summary>
    /// This class represents the schedule view
    /// </summary>
    class EpisodeCollectionView : ListCollectionView
    {
      #region variables
      SortType _sortMode = SortType.Date;
      private ScheduleDatabaseModel _model;
      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="ScheduleCollectionView"/> class.
      /// </summary>
      /// <param name="model">The database model.</param>
      public EpisodeCollectionView(ScheduleDatabaseModel model)
        : base(model.Episodes)
      {
        _model = model;
        _model.PropertyChanged += new PropertyChangedEventHandler(onDatabaseChanged);
      }

      void onDatabaseChanged(object sender, PropertyChangedEventArgs e)
      {
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }

      /// <summary>
      /// Gets or sets the sort mode.
      /// </summary>
      /// <value>The sort mode.</value>
      public SortType SortMode
      {
        get
        {
          return _sortMode;
        }
        set
        {
          if (_sortMode != value)
          {
            _sortMode = value;
            //this.CustomSort = new ScheduleComparer(_sortMode);
          }
        }
      }
    }
    #endregion

    #region ScheduleComparer class
    /// <summary>
    /// Helper class to compare 2 RecordingModels
    /// </summary>
    public class ScheduleComparer : IComparer
    {
      #region variables
      SortType _sortMode;
      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="ScheduleComparer"/> class.
      /// </summary>
      /// <param name="sortMode">The sort mode.</param>
      public ScheduleComparer(SortType sortMode)
      {
        _sortMode = sortMode;
      }
      /// <summary>
      /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
      /// </summary>
      /// <param name="x">The first object to compare.</param>
      /// <param name="y">The second object to compare.</param>
      /// <returns>
      /// Value Condition Less than zero x is less than y. Zero x equals y. Greater than zero x is greater than y.
      /// </returns>
      /// <exception cref="T:System.ArgumentException">Neither x nor y implements the <see cref="T:System.IComparable"></see> interface.-or- x and y are of different types and neither one can handle comparisons with the other. </exception>
      public int Compare(object x, object y)
      {
        ScheduleModel model1 = (ScheduleModel)x;
        ScheduleModel model2 = (ScheduleModel)y;
        switch (_sortMode)
        {
          case SortType.Channel:
            return String.Compare(model1.Channel.ToString(), model2.Channel, true);
          case SortType.Date:
            return model1.StartTime.CompareTo(model2.StartTime);
          case SortType.Duration:
            TimeSpan t1 = model1.EndTime - model1.StartTime;
            TimeSpan t2 = model2.EndTime - model2.StartTime;
            return t1.CompareTo(t2);
          case SortType.Title:
            return String.Compare(model1.Title, model2.Title, true);
        }
        return 0;
      }
    }
    #endregion
  }
}
