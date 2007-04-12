using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
using ProjectInfinity.Controls;
using TvDatabase;

namespace MyTv
{
  public class TvGuideViewModel : TvBaseViewModel
  {
    #region private classes
    public class EpgGridCell : DataGridCell
    {
      ProgramModel _model;
      Channel _channel;

      public EpgGridCell()
      {
      }
      public EpgGridCell(int column)
        : base(column)
      {
      }
      public EpgGridCell(int column, int columnSpan)
        : base(column, columnSpan)
      {
      }
      public Channel Channel
      {
        get
        {
          return _channel;
        }
        set
        {
          _channel = value;
        }
      }
      public ProgramModel ProgramModel
      {
        get
        {
          return _model;
        }
        set
        {
          _model = value;
        }
      }
      public string Title
      {
        get
        {
          if (_model == null) return "";
          return _model.Title;
        }
      }
      public string Description
      {
        get
        {
          if (_model == null) return "";
          return _model.Description;
        }
      }
      public string ChannelName
      {
        get
        {
          if (_model == null) return "";
          return _model.Channel;
        }
      }
      public string Date
      {
        get
        {
          if (_model == null) return "";
          return _model.Date;
        }
      }
      public string Duration
      {
        get
        {
          if (_model == null) return "";
          return _model.Duration;
        }
      }
      public DateTime StartTime
      {
        get
        {
          if (_model == null) return DateTime.MinValue;
          return _model.StartTime;
        }
      }
      public DateTime EndTime
      {
        get
        {
          if (_model == null) return DateTime.MinValue;
          return _model.EndTime;
        }
      }
      public string Genre
      {
        get
        {
          if (_model == null) return "";
          return _model.Genre;
        }
      }
      public bool IsRecorded
      {
        get
        {
          if (_model == null) return false;
          return _model.IsRecorded;
        }
      }
      public string Logo
      {
        get
        {
          if (_model == null) return "";
          return _model.Logo;
        }
      }
      public Program Program
      {
        get
        {
          if (_model == null) return null;
          return _model.Program;
        }
      }
      public string RecordingLogo
      {
        get
        {
          if (_model == null) return "";
          return _model.RecordingLogo;
        }
      }
      public string StartEndLabel
      {
        get
        {
          if (_model == null) return "";
          return _model.StartEndLabel;
        }
      }
    }
    #endregion

    #region variables
    DataGridCollection _epgRows = new DataGridCollection();
    ICommand _programClicked;
    int _maxChannels;
    IList _recordingList;
    IList _groupMaps;
    int _currentChannelOffset = 0;
    DateTime _currentTime = DateTime.Now;
    Channel _selectedChannel;
    bool _singleMode = false;
    int _maxSingleRows;
    int _singleRowOffset = 0;
    #endregion

    #region ctor
    public TvGuideViewModel(Page page)
      : base(page)
    {
      Reload();
    }
    #endregion

    #region properties
    public DataGridCollection EpgData
    {
      get
      {
        return _epgRows;
      }
    }
    public override string HeaderLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 0);//tvguide
      }
    }
    public bool SingleMode
    {
      get
      {
        return _singleMode;
      }
      set
      {
        _singleMode = value;
      }
    }
    public Channel SelectedChannel
    {
      get
      {
        return _selectedChannel;
      }
      set
      {
        _selectedChannel = value;
      }
    }

    public ICommand ProgramClicked
    {
      get
      {
        if (_programClicked == null)
          _programClicked = new ProgramClickedCommand(this);
        return _programClicked;
      }
    }
    #endregion

    #region tvguide datagrid renderer
    public void Reload()
    {
      _maxChannels = (int)((Window.ActualHeight - 300) / 34);
      LoadChannels();
      RenderTvGuide();
      _epgRows.OnCollectionChanged();
      ChangeProperty("EpgData");
    }
    /// <summary>
    /// Loads the channels of the current selected group 
    /// and updates the date/time onscreen
    /// </summary>
    void LoadChannels()
    {
      _recordingList = Schedule.ListAll();
      _groupMaps = ServiceScope.Get<ITvChannelNavigator>().CurrentGroup.ReferringGroupMap();
    }
    void RenderTvGuide()
    {
      _epgRows.Clear();
      List<Channel> tvChannels = new List<Channel>();
      for (int i = _currentChannelOffset; i < _currentChannelOffset + _maxChannels; ++i)
      {
        int off = i;
        if (off >= _groupMaps.Count)
        {
          off -= _groupMaps.Count;
        }
        if (off < _groupMaps.Count)
        {
          Channel channel = ((GroupMap)_groupMaps[off]).ReferencedChannel();
          tvChannels.Add(channel);
        }
      }
      DateTime now;
      now = (DateTime)_currentTime;

      int min = now.Minute;
      if (min < 30) min = 0;
      else min = 30;
      now = now.AddMinutes(-now.Minute + min);
      now = now.AddSeconds(-now.Second);
      now = now.AddMilliseconds(-now.Millisecond);
      DateTime end = now.AddHours(2);
      if (_singleMode)
        RenderSingleMode(now, end);
      else
        RenderMultiMode(now, end, tvChannels);
    }
    void RenderSingleMode(DateTime now, DateTime end)
    {
      end = now.AddDays(2);
      TvBusinessLayer layer = new TvBusinessLayer();
      IList programs = layer.GetPrograms(_selectedChannel, now, end);
      _maxSingleRows = programs.Count;
      int rowNr = 0;
      for (int i = _singleRowOffset; i < _maxChannels; ++i)
      {
        Program program;
        if (i < programs.Count)
          program = (Program)programs[i];
        else
        {
          program = new Program(_selectedChannel.IdChannel, now, now.AddMinutes(30), "No information", "", "", false);
          now = now.AddMinutes(30);
        }
        
        DataGridRow dataRow = new DataGridRow();

        ProjectInfinity.Controls.Button b = new ProjectInfinity.Controls.Button();
        b.Style = (Style)Application.Current.Resources["MpButton"];
        b.Content = program.StartTime.ToString("HH:mm");
        EpgGridCell dataCell = new EpgGridCell(0, 5);
        dataCell.Content = b;
        dataCell.Channel = _selectedChannel;
        dataRow.Cells.Add(dataCell);



        dataCell = new EpgGridCell(5, 25);
        dataCell.ProgramModel = new ProgramModel(program);
        b = new ProjectInfinity.Controls.Button();
        b.Style = (Style)Application.Current.Resources["MpButton"];
        b.Content = program.Title;
        dataCell.Content = b;
        dataRow.Cells.Add(dataCell);

        rowNr++;
        if (rowNr == _maxChannels + 1) break;
        _epgRows.Add(dataRow);
      }

    }
    /// <summary>
    /// Renders the tvguide in multi channel mode.
    /// </summary>
    /// <param name="now">The now.</param>
    /// <param name="end">The end.</param>
    /// <param name="tvChannels">The tv channels.</param>
    void RenderMultiMode(DateTime now, DateTime end, List<Channel> tvChannels)
    {
      DataGridRow row = new DataGridRow();
      TvBusinessLayer layer = new TvBusinessLayer();
      DataGridCell cellHeader = new DataGridCell(5, 6);
      Label header1 = new Label();
      header1.Style = (Style)Application.Current.Resources["LabelHeaderNormalStyle"];
      header1.Content = now.ToShortTimeString();
      cellHeader.Content = header1;
      row.Cells.Add(cellHeader);

      cellHeader = new DataGridCell(11, 6);
      header1 = new Label();
      header1.Style = (Style)Application.Current.Resources["LabelHeaderNormalStyle"];
      header1.Content = now.AddMinutes(30).ToShortTimeString();
      cellHeader.Content = header1;
      row.Cells.Add(cellHeader);

      cellHeader = new DataGridCell(17, 6);
      header1 = new Label();
      header1.Style = (Style)Application.Current.Resources["LabelHeaderNormalStyle"];
      header1.Content = now.AddMinutes(60).ToShortTimeString();

      cellHeader.Content = header1;
      row.Cells.Add(cellHeader);

      cellHeader = new DataGridCell(23, 6);
      header1 = new Label();
      header1.Style = (Style)Application.Current.Resources["LabelHeaderNormalStyle"];
      header1.Content = now.AddMinutes(90).ToShortTimeString();
      cellHeader.Content = header1;
      row.Cells.Add(cellHeader);
      _epgRows.Add(row);


      Dictionary<int, List<Program>> programs = layer.GetProgramsForAllChannels(now, end, tvChannels);
      int count = 0;

      foreach (Channel channel in tvChannels)
      {
        if (programs.ContainsKey(channel.IdChannel))
        {
          RenderMultiChannelRow(count + 1, now, end, channel, programs[channel.IdChannel], (count == tvChannels.Count - 1));
          count++;
        }
        else
        {
          Program p = new Program(channel.IdChannel, now, end, "No Information", "", "", false);
          List<Program> tmpProgs = new List<Program>();
          tmpProgs.Add(p);
          RenderMultiChannelRow(count + 1, now, end, channel, tmpProgs, (count == tvChannels.Count - 1));
          count++;
        }
      }
    }

    /// <summary>
    /// Renders a tv channel row for multi-channel mode.
    /// </summary>
    /// <param name="rowNr">The row nr.</param>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="programs">The programs.</param>
    /// <param name="isBottom">if set to <c>true</c> [is bottom].</param>
    /// <returns></returns>
    void RenderMultiChannelRow(int rowNr, DateTime startTime, DateTime endTime, Channel channel, List<Program> programs, bool isBottom)
    {
      DataGridRow dataRow = new DataGridRow();

      ProjectInfinity.Controls.Button b = new ProjectInfinity.Controls.Button();
      b.Style = (Style)Application.Current.Resources["MpButton"];
      Grid grid = new Grid();
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.RowDefinitions.Add(new RowDefinition());
      string channelLogoFileName = Thumbs.GetLogoFileName(channel.Name);
      if (System.IO.File.Exists(channelLogoFileName))
      {
        Image image = new Image();
        PngBitmapDecoder decoder = new PngBitmapDecoder(new Uri(channelLogoFileName, UriKind.Relative), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

        image.Source = decoder.Frames[0];
        Grid.SetColumn(image, 0);
        Grid.SetRow(image, 0);
        grid.Children.Add(image);
      }
      Label label = new Label();
      label.Content = channel.Name;
      label.Style = (Style)Application.Current.Resources["LabelNormalStyleWhite"];
      Grid.SetColumn(label, 1);
      Grid.SetRow(label, 0);
      Grid.SetColumnSpan(label, 3);
      grid.Children.Add(label);

      b.Content = grid;
      EpgGridCell dataGridCell = new EpgGridCell(0, 5);
      dataGridCell.Channel = channel;
      dataGridCell.Content = b;
      dataRow.Cells.Add(dataGridCell);

      RenderMultiChannelCells(ref dataRow, rowNr, startTime, endTime, channel, programs, isBottom);
      _epgRows.Add(dataRow);
    }

    /// <summary>
    /// Renders the cells of a row in multi-channel mode.
    /// </summary>
    /// <param name="rowNr">The row nr.</param>
    /// <param name="now">The now.</param>
    /// <param name="end">The end.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="programs">The programs.</param>
    /// <param name="isBottom">if set to <c>true</c> [is bottom].</param>
    /// <returns></returns>
    void RenderMultiChannelCells(ref DataGridRow dataRow, int rowNr, DateTime now, DateTime end, Channel channel, List<Program> programs, bool isBottom)
    {

      int cell = 5;
      List<Program> cells = new List<Program>();
      for (int prognr = 0; prognr < programs.Count; ++prognr)
      {
        Program program = programs[prognr];
        DateTime startTime = program.StartTime;
        if (startTime < now) startTime = now;
        DateTime endTime = program.EndTime;
        if (endTime > end) endTime = end;
        int min = startTime.Minute % 10;
        if (min > 0 && min < 5) startTime = startTime.AddMinutes(-min);
        else if (min > 5) startTime = startTime.AddMinutes((10 - min));
        min = endTime.Minute % 10;
        if (min > 0 && min < 5) endTime = endTime.AddMinutes(-min);
        else if (min > 5) endTime = endTime.AddMinutes((10 - min));
        TimeSpan ts = endTime - startTime;
        int span = (int)((ts.TotalMinutes + 0.5) / 5);
        if (span <= 0) continue;
        cells.Add(program);
      }
      for (int prognr = 0; prognr < cells.Count; ++prognr)
      {
        Program program = cells[prognr];
        DateTime startTime = program.StartTime;
        if (startTime < now) startTime = now;
        DateTime endTime = program.EndTime;
        if (endTime > end) endTime = end;
        int min = startTime.Minute % 10;
        if (min > 0 && min < 5) startTime = startTime.AddMinutes(-min);
        else if (min > 5) startTime = startTime.AddMinutes((10 - min));
        min = endTime.Minute % 10;
        if (min > 0 && min < 5) endTime = endTime.AddMinutes(-min);
        else if (min > 5) endTime = endTime.AddMinutes((10 - min));
        TimeSpan ts = endTime - startTime;
        int span = (int)((ts.TotalMinutes + 0.5) / 5);
        //if (span <= 0) continue;

        bool bRecording = false;
        bool bSeries = false;
        bool bConflict = false;
        if (_recordingList != null)
        {
          foreach (Schedule record in _recordingList)
          {
            if (record.IsRecordingProgram(program, true))
            {
              if (record.ReferringConflicts().Count != 0)
                bConflict = true;
              if ((ScheduleRecordingType)record.ScheduleType != ScheduleRecordingType.Once)
                bSeries = true;
              bRecording = true;
              break;
            }
          }
        }

        ProjectInfinity.Controls.Button b = new ProjectInfinity.Controls.Button();
        bool isNow = false;
        //------start--------------end
        //---x           x
        //-----------------
        if (DateTime.Now >= program.StartTime && DateTime.Now <= program.EndTime) isNow = true;

        if (isNow)
        {
          if (program.StartTime < now && program.EndTime > end)
            b.Style = (Style)Application.Current.Resources["MpButtonLightBoth"];
          else if (program.StartTime < now)
            b.Style = (Style)Application.Current.Resources["MpButtonLightLeft"];
          else if (program.EndTime > end)
            b.Style = (Style)Application.Current.Resources["MpButtonLightRight"];
          else
            b.Style = (Style)Application.Current.Resources["MpButtonLight"];
        }
        else
        {
          if (program.StartTime < now && program.EndTime > end)
            b.Style = (Style)Application.Current.Resources["MpButtonBoth"];
          else if (program.StartTime < now)
            b.Style = (Style)Application.Current.Resources["MpButtonLeft"];
          else if (program.EndTime > end)
            b.Style = (Style)Application.Current.Resources["MpButtonRight"];
          else
            b.Style = (Style)Application.Current.Resources["MpButton"];

        }
        if (bRecording)
        {
          Uri uri;
          if (bConflict)
          {
            if (bSeries)
              uri = new Uri(Thumbs.TvConflictRecordingSeriesIcon, UriKind.Relative);
            else
              uri = new Uri(Thumbs.TvConflictRecordingIcon, UriKind.Relative);
          }
          else if (bSeries)
            uri = new Uri(Thumbs.TvRecordingSeriesIcon, UriKind.Relative);
          else
            uri = new Uri(Thumbs.TvRecordingIcon, UriKind.Relative);

          Grid panel = new Grid();
          panel.Margin = new Thickness(0.0d);
          Image image = new Image();
          PngBitmapDecoder decoder = new PngBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
          image.Source = decoder.Frames[0];
          image.Width = image.Height = 20;
          image.VerticalAlignment = VerticalAlignment.Center;

          Label label = new Label();
          label.Margin = new Thickness(0.0d);
          label.Content = program.Title;
          label.VerticalAlignment = VerticalAlignment.Center;
          label.Style = (Style)Application.Current.Resources["Label20Style"];
          panel.Children.Add(label);
          panel.Children.Add(image);
          panel.OpacityMask = (Brush)Application.Current.Resources["fadeOpacityBrush"];
          b.Content = panel;
        }
        else
        {
          Grid panel = new Grid();
          panel.ShowGridLines = true;
          Label label = new Label();
          label.Content = program.Title;
          label.Style = (Style)Application.Current.Resources["Label20Style"];
          panel.Children.Add(label);
          panel.OpacityMask = (Brush)Application.Current.Resources["fadeOpacityBrush"];
          b.Content = panel;
        }

        EpgGridCell dataCell = new EpgGridCell(cell, span);
        dataCell.ProgramModel = new ProgramModel(program);
        dataCell.Content = b;
        dataRow.Cells.Add(dataCell);
        cell += span;
      }
    }
    #endregion

    #region ProgramClickedCommand class
    public class ProgramClickedCommand : ICommand
    {
      #region ICommand Members
      TvGuideViewModel _viewModel;
      public ProgramClickedCommand(TvGuideViewModel model)
      {
        _viewModel = model;
      }

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public event EventHandler CanExecuteChanged;

      public void Execute(object parameter)
      {
        EpgGridCell cell = parameter as EpgGridCell;
        if (cell == null) return;
        if (cell.Channel != null)
        {
          _viewModel.SelectedChannel = cell.Channel;
          _viewModel.SingleMode = !_viewModel.SingleMode;
          _viewModel.Reload();
        }
        else
        {
          if (DateTime.Now >= cell.Program.StartTime && DateTime.Now <= cell.Program.EndTime)
          {
            //show tv channel
            ICommand cmd = _viewModel.TimeShift;
            cmd.Execute(cell.Program.ReferencedChannel());
          }
          else
          {
            TvScheduledViewModel model;
            if (!ServiceScope.IsRegistered<TvScheduledViewModel>())
            {
              model = new TvScheduledViewModel(_viewModel.Page);
            }
            else
              model = ServiceScope.Get<TvScheduledViewModel>();
            model.CurrentProgram = cell.ProgramModel;
            ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvProgramInfo.xaml", UriKind.Relative));
          }
        }
      }

      #endregion
    }
    #endregion
  }
}
