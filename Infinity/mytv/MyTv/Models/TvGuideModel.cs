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

    #region variables
    DataGridCollection _epgRows = new DataGridCollection();
    ICommand _programClicked;
    ICommand _scrollCommand;
    int _maxChannels;
    IList _recordingList;
    IList _groupMaps;
    int _multiOffset = 0;
    DateTime _currentTime = DateTime.Now;
    Channel _selectedChannel;
    bool _singleMode = false;
    int _maxSingleRows;
    int _singleRowOffset = 0;
    #endregion

    #region ctor
    public TvGuideViewModel()
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
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 39);//tvguide
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
    public int SingleOffset
    {
      get
      {
        return _singleRowOffset;
      }
      set
      {
        _singleRowOffset = value;
      }
    }
    public int MultiOffset
    {
      get
      {
        return _multiOffset;
      }
      set
      {
        _multiOffset = value;
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

    public DateTime Time
    {
      get
      {
        return _currentTime;
      }
      set
      {
        _currentTime = value;
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
    public ICommand OnScroll
    {
      get
      {
        if (_scrollCommand == null)
          _scrollCommand = new ScrollCommand(this);
        return _scrollCommand;
      }
    }
    #endregion

    #region tvguide datagrid renderer
    public override void Refresh()
    {
      base.Refresh();
      Reload();
    }
    public void Reload()
    {
      if (false == ServiceScope.IsRegistered<ITvChannelNavigator>())
      {
        return;
      }
      if (false==ServiceScope.Get<ITvChannelNavigator>().IsInitialized)
      {
        return;
      }
      int selectedColumn = -1;
      int selectedRow = -1;
      if (_epgRows.CurrentItem != null)
      {
        selectedColumn = _epgRows.CurrentItem.DataGrid.SelectedColumn;
        selectedRow = _epgRows.CurrentItem.DataGrid.SelectedRow;
      }
      _maxChannels = (int)((Window.ActualHeight - 300) / 34);
      LoadChannels();
      RenderTvGuide();
      _epgRows.OnCollectionChanged();
      ChangeProperty("EpgData");
      while (selectedRow >= _epgRows.Count) selectedRow--;
      if (selectedRow >= 0)
      {
        while (selectedColumn >= _epgRows[selectedRow].Cells.Count) selectedColumn--;
        if (selectedColumn >= 0)
        {
          Keyboard.Focus(_epgRows[selectedRow].Cells[selectedColumn].Content);
        }
      }
      if (selectedRow < 0 || selectedColumn < 0)
      {
        if (_epgRows.Count > 1)
        {
          Keyboard.Focus(_epgRows[1].Cells[0].Content);
        }
      }
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
      for (int i = _multiOffset; i < _multiOffset + _maxChannels - 1; ++i)
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

      _epgRows.Add(new DataGridRow());

      for (int i = _singleRowOffset; i < _maxChannels; ++i)
      {
        Program program;
        if (i < programs.Count)
          program = (Program)programs[i];
        else
        {
          program = new Program(_selectedChannel.IdChannel, now, now.AddMinutes(30), 
            ServiceScope.Get<ILocalisation>().ToString("mytv",138)/*No information*/, 
            "", "", false,now,"","",0,"");
          now = now.AddMinutes(30);
        }

        DataGridRow dataRow = new DataGridRow();

        EpgGridCell dataCell = new EpgGridCell(0, 5);
        dataCell.ChannelName = program.StartTime.ToString("HH:mm");
        dataCell.Channel = _selectedChannel;
        dataRow.Cells.Add(dataCell);


        dataCell = new EpgGridCell(5, 25);
        dataCell.ButtonStyle = (Style)Application.Current.Resources["MpButton"];
        dataCell.ProgramModel = new ProgramModel(program);
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
      EpgGridCell cellHeader = new EpgGridCell(5, 6);
      cellHeader.Time = now.ToShortTimeString();
      row.Cells.Add(cellHeader);

      cellHeader = new EpgGridCell(11, 6);
      cellHeader.Time = now.AddMinutes(30).ToShortTimeString();
      row.Cells.Add(cellHeader);

      cellHeader = new EpgGridCell(17, 6);
      cellHeader.Time = now.AddMinutes(60).ToShortTimeString();
      row.Cells.Add(cellHeader);

      cellHeader = new EpgGridCell(23, 6);
      cellHeader.Time = now.AddMinutes(90).ToShortTimeString();
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
          Program p = new Program(channel.IdChannel, now, end, ServiceScope.Get<ILocalisation>().ToString("mytv", 138)/*No information*/, "", "", false, now, "", "", 0, "");
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
      EpgGridCell dataGridCell = new EpgGridCell(0, 5);
      dataGridCell.Channel = channel;
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

        EpgGridCell dataCell = new EpgGridCell(cell, span);
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

        bool isNow = false;
        //------start--------------end
        //---x           x
        //-----------------
        if (DateTime.Now >= program.StartTime && DateTime.Now <= program.EndTime) isNow = true;

        if (isNow)
        {
          if (program.StartTime < now && program.EndTime > end)
            dataCell.ButtonStyle = (Style)Application.Current.Resources["MpButtonLightBoth"];
          else if (program.StartTime < now)
            dataCell.ButtonStyle = (Style)Application.Current.Resources["MpButtonLightLeft"];
          else if (program.EndTime > end)
            dataCell.ButtonStyle = (Style)Application.Current.Resources["MpButtonLightRight"];
          else
            dataCell.ButtonStyle = (Style)Application.Current.Resources["MpButtonLight"];
        }
        else
        {
          if (program.StartTime < now && program.EndTime > end)
            dataCell.ButtonStyle = (Style)Application.Current.Resources["MpButtonBoth"];
          else if (program.StartTime < now)
            dataCell.ButtonStyle = (Style)Application.Current.Resources["MpButtonLeft"];
          else if (program.EndTime > end)
            dataCell.ButtonStyle = (Style)Application.Current.Resources["MpButtonRight"];
          else
            dataCell.ButtonStyle = (Style)Application.Current.Resources["MpButton"];

        }
        if (bRecording)
        {
          if (bConflict)
          {
            if (bSeries)
              dataCell.RecordingLogo = String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.TvConflictRecordingSeriesIcon);
            else
              dataCell.RecordingLogo = String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.TvConflictRecordingIcon);
          }
          else if (bSeries)
            dataCell.RecordingLogo = String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.TvRecordingSeriesIcon);
          else
            dataCell.RecordingLogo = String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.TvRecordingIcon);

        }
        dataCell.ProgramModel = new ProgramModel(program);
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
              model = new TvScheduledViewModel();
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

    #region ScrollCommand class
    public class ScrollCommand : ICommand
    {
      #region ICommand Members
      TvGuideViewModel _viewModel;
      public event EventHandler CanExecuteChanged;
      public ScrollCommand(TvGuideViewModel model)
      {
        _viewModel = model;
      }

      public bool CanExecute(object parameter)
      {
        return true;
      }


      public void Execute(object parameter)
      {
        string direction = parameter as string;
        if (direction == null) return;
        switch (direction)
        {
          case "Down":
            if (_viewModel.SingleMode)
            {
              _viewModel.SingleOffset++;
              _viewModel.Reload();
            }
            else
            {
              _viewModel.MultiOffset++;
              _viewModel.Reload();
            }
            break;
          case "Up":
            if (_viewModel.SingleMode)
            {
              if (_viewModel.SingleOffset > 0)
              {
                _viewModel.SingleOffset--;
                _viewModel.Reload();
              }
            }
            else
            {
              if (_viewModel.MultiOffset > 0)
              {
                _viewModel.MultiOffset--;
                _viewModel.Reload();
              }
            }
            break;
          case "Left":
            _viewModel.Time = _viewModel.Time.AddMinutes(-30);
            _viewModel.Reload();
            break;

          case "Right":
            _viewModel.Time = _viewModel.Time.AddMinutes(30);
            _viewModel.Reload();
            break;
        }
      }

      #endregion
    }
    #endregion
  }
}
