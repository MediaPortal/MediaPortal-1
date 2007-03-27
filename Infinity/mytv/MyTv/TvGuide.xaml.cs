using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;
namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvGuide.xaml
  /// </summary>

  public partial class TvGuide : System.Windows.Controls.Page
  {
    #region private classes
    class GuideTag
    {
      public Channel Channel;
      public Program Program;
      public bool IsBottomEdge;
      public bool IsRightEdge;
      public bool IsUpperEdge;
      public bool IsLeftEdge;
      public int RowNr;
      public int ColumnNr;
    };
    class GuideRow
    {
      public List<Button> Colums = new List<Button>();
    }
    class GuideGrid
    {
      public List<GuideRow> Rows = new List<GuideRow>();
    }
    #endregion

    #region variables
    IList _recordingList = new ArrayList();
    bool _reactOnMouseEvents = true;
    DateTime _currentTime = DateTime.Now;
    int _currentChannelOffset = 0;
    int _maxChannels = 6;
    IList _groupMaps;
    GuideTag _selectedItem;
    DateTime _selectedTime;
    GuideGrid _grid = new GuideGrid();
    int _focusedRow = 0;
    bool _singleMode = false;
    int _singleRowOffset = 0;
    int _maxSingleRows = 0;
    Channel _selectedChannel;
    #endregion

    #region ctor
    public TvGuide()
    {
      InitializeComponent();
    }
    #endregion

    #region tvguide render methods

    /// <summary>
    /// Loads the channels of the current selected group 
    /// and updates the date/time onscreen
    /// </summary>
    void LoadChannels()
    {
      labelDate.Content = DateTime.Now.ToString("dd-MM HH:mm");
      _recordingList = Schedule.ListAll();
      _groupMaps = ChannelNavigator.Instance.CurrentGroup.ReferringGroupMap();
    }

    /// <summary>
    /// Renders the tv guide grid.
    /// </summary>
    void RenderTvGuide()
    {
      _grid = new GuideGrid();
      _reactOnMouseEvents = false;
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
      gridGuide.Children.Clear();
      for (int i = 0; i <= tvChannels.Count; ++i)
        gridGuide.RowDefinitions.Add(new RowDefinition());
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

    #region single-tv channel mode rendering
    /// <summary>
    /// Renders the tvguide in single channel mode.
    /// </summary>
    /// <param name="now">The now.</param>
    /// <param name="end">The end.</param>
    void RenderSingleMode(DateTime now, DateTime end)
    {
      end = now.AddDays(2);
      TvBusinessLayer layer = new TvBusinessLayer();
      IList programs = layer.GetPrograms(_selectedChannel, now, end);
      _maxSingleRows = programs.Count;
      int rowNr = 0;
      for (int i = _singleRowOffset; i < _maxSingleRows; ++i)
      {
        Program program = (Program)programs[i];
        GuideTag tag = new GuideTag();
        tag.Channel = _selectedChannel;
        tag.Program = program;
        tag.RowNr = rowNr;
        tag.ColumnNr = 0;
        tag.IsUpperEdge = (rowNr == 0);
        tag.IsBottomEdge = ((rowNr) == _maxChannels);
        Button b = new Button();
        b.Tag = tag;
        b.Template = (ControlTemplate)Application.Current.Resources["MpButton"];
        b.Content = program.StartTime.ToString("HH:mm");
        b.MouseEnter += new MouseEventHandler(OnMouseEnter);
        b.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnButtonGotKeyboardFocus);
        b.Click += new RoutedEventHandler(OnClickChannel);
        Grid.SetRow(b, rowNr);
        Grid.SetColumn(b, 0);
        Grid.SetColumnSpan(b, 5);
        gridGuide.Children.Add(b);
        _grid.Rows.Add(new GuideRow());
        _grid.Rows[rowNr].Colums.Add(b);


        tag = new GuideTag();
        tag.Program = program;
        tag.Channel = _selectedChannel;
        tag.RowNr = rowNr;
        tag.ColumnNr = 1;
        tag.IsUpperEdge = (rowNr == 0);
        tag.IsBottomEdge = ((rowNr) == _maxChannels);
        b = new Button();
        b.Tag = tag;
        b.Template = (ControlTemplate)Application.Current.Resources["MpButton"];
        b.Content = program.Title;
        b.MouseEnter += new MouseEventHandler(OnMouseEnter);
        b.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnButtonGotKeyboardFocus);
        Grid.SetRow(b, rowNr);
        Grid.SetColumn(b, 5);
        Grid.SetColumnSpan(b, 24);
        gridGuide.Children.Add(b);
        _grid.Rows[rowNr].Colums.Add(b);
        rowNr++;
        if (rowNr == _maxChannels + 1) break;
      }
      if (_selectedItem != null)
      {
        Keyboard.Focus(_grid.Rows[_selectedItem.RowNr].Colums[_selectedItem.ColumnNr]);
      }
      else
      {
        Keyboard.Focus(_grid.Rows[0].Colums[0]);
      }
    }
    #endregion

    #region multi-channel mode rendering
    /// <summary>
    /// Renders the tvguide in multi channel mode.
    /// </summary>
    /// <param name="now">The now.</param>
    /// <param name="end">The end.</param>
    /// <param name="tvChannels">The tv channels.</param>
    void RenderMultiMode(DateTime now, DateTime end, List<Channel> tvChannels)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Label header1 = new Label();
      Grid.SetRow(header1, 0);
      Grid.SetColumn(header1, 5);
      Grid.SetColumnSpan(header1, 6);
      header1.Style = (Style)Application.Current.Resources["LabelHeaderNormalStyle"];
      header1.Content = now.ToShortTimeString();
      gridGuide.Children.Add(header1);
      header1 = new Label();
      Grid.SetRow(header1, 0);
      Grid.SetColumn(header1, 11);
      Grid.SetColumnSpan(header1, 6);
      header1.Style = (Style)Application.Current.Resources["LabelHeaderNormalStyle"];
      header1.Content = now.AddMinutes(30).ToShortTimeString();
      gridGuide.Children.Add(header1);
      header1 = new Label();
      Grid.SetRow(header1, 0);
      Grid.SetColumn(header1, 17);
      Grid.SetColumnSpan(header1, 6);
      header1.Style = (Style)Application.Current.Resources["LabelHeaderNormalStyle"];
      header1.Content = now.AddMinutes(60).ToShortTimeString();
      gridGuide.Children.Add(header1);
      header1 = new Label();
      Grid.SetRow(header1, 0);
      Grid.SetColumn(header1, 23);
      Grid.SetColumnSpan(header1, 6);
      header1.Style = (Style)Application.Current.Resources["LabelHeaderNormalStyle"];
      header1.Content = now.AddMinutes(90).ToShortTimeString();
      gridGuide.Children.Add(header1);


      Dictionary<int, List<Program>> programs = layer.GetProgramsForAllChannels(now, end, tvChannels);
      int count = 0;

      Button buttonToSelect = null;
      foreach (Channel channel in tvChannels)
      {
        if (programs.ContainsKey(channel.IdChannel))
        {
          Button b = RenderMultiChannelRow(count + 1, now, end, channel, programs[channel.IdChannel], (count == tvChannels.Count - 1));
          if (b != null) buttonToSelect = b;
          count++;
        }
        else
        {
          Program p = new Program(channel.IdChannel, now, end, "No Information", "", "", false);
          List<Program> tmpProgs = new List<Program>();
          tmpProgs.Add(p);
          Button b = RenderMultiChannelRow(count + 1, now, end, channel, tmpProgs, (count == tvChannels.Count - 1));
          if (b != null) buttonToSelect = b;
          count++;
        }
      }
      if (buttonToSelect != null)
      {
        GuideTag tag = buttonToSelect.Tag as GuideTag;
        _selectedItem = (GuideTag)tag;
        UpdateInfoBox();
      }
      Keyboard.Focus(buttonToSelect);
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
    Button RenderMultiChannelRow(int rowNr, DateTime startTime, DateTime endTime, Channel channel, List<Program> programs, bool isBottom)
    {
      Button buttonToSelect = null;
      GuideTag tag = new GuideTag();
      tag.IsLeftEdge = true;
      tag.RowNr = rowNr;
      tag.Channel = channel;
      Button b = new Button();
      b.Tag = tag;
      b.Template = (ControlTemplate)Application.Current.Resources["MpButton"];
      b.Content = channel.Name;
      b.MouseEnter += new MouseEventHandler(OnMouseEnter);
      b.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnButtonGotKeyboardFocus);
      b.Click += new RoutedEventHandler(OnClickChannel);
      Grid.SetRow(b, rowNr);
      Grid.SetColumn(b, 0);
      Grid.SetColumnSpan(b, 5);
      gridGuide.Children.Add(b);
      _grid.Rows.Add(new GuideRow());
      if (_selectedItem != null)
      {
        if (_selectedItem.RowNr == rowNr)
        {
          if (_selectedItem.Program == null)
          {
            buttonToSelect = b;
          }
        }
      }

      Button result = RenderMultiChannelCells(rowNr, startTime, endTime, channel, programs, isBottom);
      if (result != null) return result;
      return buttonToSelect;
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
    Button RenderMultiChannelCells(int rowNr, DateTime now, DateTime end, Channel channel, List<Program> programs, bool isBottom)
    {
      Button buttonToSelect = null;

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

        Button b = new Button();
        bool isNow = false;
        //------start--------------end
        //---x           x
        //-----------------
        if (DateTime.Now >= program.StartTime && DateTime.Now <= program.EndTime) isNow = true;

        if (isNow)
        {
          if (program.StartTime < now && program.EndTime > end)
            b.Template = (ControlTemplate)Application.Current.Resources["MpButtonLightBoth"];
          else if (program.StartTime < now)
            b.Template = (ControlTemplate)Application.Current.Resources["MpButtonLightLeft"];
          else if (program.EndTime > end)
            b.Template = (ControlTemplate)Application.Current.Resources["MpButtonLightRight"];
          else
            b.Template = (ControlTemplate)Application.Current.Resources["MpButtonLight"];
        }
        else
        {
          if (program.StartTime < now && program.EndTime > end)
            b.Template = (ControlTemplate)Application.Current.Resources["MpButtonBoth"];
          else if (program.StartTime < now)
            b.Template = (ControlTemplate)Application.Current.Resources["MpButtonLeft"];
          else if (program.EndTime > end)
            b.Template = (ControlTemplate)Application.Current.Resources["MpButtonRight"];
          else
            b.Template = (ControlTemplate)Application.Current.Resources["MpButton"];

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
          Image image = new Image();
          PngBitmapDecoder decoder = new PngBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
          image.Source = decoder.Frames[0];
          image.Width = image.Height = 20;
          image.VerticalAlignment = VerticalAlignment.Center;

          Label label = new Label();
          label.Content = program.Title;
          label.VerticalAlignment = VerticalAlignment.Center;
          label.Style = (Style)Application.Current.Resources["Label20Style"];
          label.OpacityMask = (Brush)Application.Current.Resources["fadeOpacityBrush"];
          panel.Children.Add(label);
          panel.Children.Add(image);
          b.Content = panel;
        }
        else
        {
          Label label = new Label();
          label.Content = program.Title;
          label.Style = (Style)Application.Current.Resources["Label20Style"];
          label.OpacityMask = (Brush)Application.Current.Resources["fadeOpacityBrush"];
          b.Content = label;
        }
        b.MouseEnter += new MouseEventHandler(OnMouseEnter);
        GuideTag tag = new GuideTag();
        tag.Program = program;
        tag.Channel = channel;
        tag.IsRightEdge = (prognr == cells.Count - 1);
        tag.IsBottomEdge = isBottom;
        tag.IsUpperEdge = (rowNr == 1);
        tag.IsLeftEdge = false;
        tag.RowNr = rowNr;
        b.Tag = tag;
        b.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnButtonGotKeyboardFocus);
        Grid.SetRow(b, rowNr);
        Grid.SetColumn(b, cell);
        Grid.SetColumnSpan(b, span);
        _grid.Rows[rowNr - 1].Colums.Add(b);

        bool select = false;
        if (_selectedItem != null)
        {
          if (_selectedItem.RowNr == rowNr)
          {
            if (buttonToSelect == null)
            {
              buttonToSelect = b;
            };

            if (_selectedTime < now) _selectedTime = now;
            if (_selectedTime > end) _selectedTime = end;
            if (_selectedTime >= program.StartTime && _selectedTime <= program.EndTime)
              select = true;
          }
        }
        else
        {
          _selectedItem = tag;
          _focusedRow = rowNr - 1;
          _selectedTime = program.StartTime;
          select = true;
        }
        gridGuide.Children.Add(b);
        cell += span;
        if (select)
        {
          buttonToSelect = b;
        }
      }
      return buttonToSelect;
    }
    #endregion

    /// <summary>
    /// Updates the title/description/start-end times and genre onscreen.
    /// </summary>
    void UpdateInfoBox()
    {
      if (_selectedItem == null)
      {
        labelTitle.Text = "";
        labelDescription.Text = "";
        labelStartEnd.Text = "";
        labelGenre.Text = "";
        return;
      }
      if (_selectedItem.Program == null)
      {
        labelTitle.Text = "";
        labelDescription.Text = "";
        labelStartEnd.Text = "";
        labelGenre.Text = "";
        return;
      }
      labelTitle.Text = _selectedItem.Program.Title;
      labelDescription.Text = _selectedItem.Program.Description;
      labelStartEnd.Text = String.Format("{0}-{1}", _selectedItem.Program.StartTime.ToString("HH:mm"), _selectedItem.Program.EndTime.ToString("HH:mm"));
      labelGenre.Text = _selectedItem.Program.Genre;
    }
    #endregion

    #region event handlers
    /// <summary>
    /// Called when windows is loaded
    /// Refreshes & redraws the entire tvguide screen
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      _maxChannels = (int)((this.ActualHeight - 300) / 34);
      this.SizeChanged += new SizeChangedEventHandler(TvGuide_SizeChanged);
      LoadChannels();
      RenderTvGuide();
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));

      if (ChannelNavigator.Instance.Card != null)
      {
        if (ChannelNavigator.Instance.Card.IsTimeShifting)
        {
          Uri uri = new Uri(ChannelNavigator.Instance.Card.TimeShiftFileName, UriKind.Absolute);
          for (int i = 0; i < TvPlayerCollection.Instance.Count; ++i)
          {
            if (TvPlayerCollection.Instance[i].Source == uri)
            {
              MediaPlayer player = TvPlayerCollection.Instance[i];
              VideoDrawing videoDrawing = new VideoDrawing();
              videoDrawing.Player = player;
              videoDrawing.Rect = new Rect(0, 0, videoWindow.ActualWidth, videoWindow.ActualHeight);
              DrawingBrush videoBrush = new DrawingBrush();
              videoBrush.Drawing = videoDrawing;
              videoWindow.Fill = videoBrush;
              break;
            }
          }
        }
      }
    }

    /// <summary>
    /// Handles the SizeChanged event of the TvGuide control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
    void TvGuide_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      _maxChannels = (int)((this.ActualHeight - 300) / 34);
      LoadChannels();
      RenderTvGuide();
    }

    /// <summary>
    /// handles the key down event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      if (_selectedItem == null) return;
      if (e.Key == Key.Escape)
      {
        //return to previous screen
        this.NavigationService.GoBack();
        return;
      }
      if (_singleMode)
      {
        if (e.Key == Key.Enter)
        {
          OnClickChannel(null, null);
          e.Handled = true;
          return;
        }
        if (e.Key == Key.Up)
        {
          if (_selectedItem.IsUpperEdge == false) return;
          if (_singleRowOffset > 0) _singleRowOffset--;
          RenderTvGuide();
          e.Handled = true;
        }

        if (e.Key == Key.Down)
        {
          if (_selectedItem.IsBottomEdge == false) return;
          if (_singleRowOffset < _maxSingleRows - 1) _singleRowOffset++;
          RenderTvGuide();
          e.Handled = true;
        }
        if (e.Key == Key.PageUp)
        {
          _singleRowOffset -= _maxChannels;
          if (_singleRowOffset < 0) _singleRowOffset = 0;
          RenderTvGuide();
          e.Handled = true;
        }
        if (e.Key == Key.PageDown)
        {
          _singleRowOffset += _maxChannels;
          if (_singleRowOffset > _maxSingleRows) _singleRowOffset -= _maxSingleRows;
          RenderTvGuide();
          e.Handled = true;
        }
        return;
      }
      if (e.Key == Key.Enter)
      {
        if (_selectedItem.IsLeftEdge)
        {
          OnClickChannel(null, null);
          e.Handled = true;
          return;
        }
      }

      if (e.Key == Key.PageDown)
      {
        _currentChannelOffset += _maxChannels;
        if (_currentChannelOffset > _maxChannels) _maxChannels -= _maxChannels;
        RenderTvGuide();
        e.Handled = true;
        return;
      }
      if (e.Key == Key.PageUp)
      {
        _currentChannelOffset -= _maxChannels;
        if (_currentChannelOffset < 0) _maxChannels += _maxChannels;
        RenderTvGuide();
        e.Handled = true;
        return;
      }
      if (e.Key == Key.Down)
      {
        if (_selectedItem.IsBottomEdge)
        {
          if (_currentChannelOffset < _groupMaps.Count)
            _currentChannelOffset++;
          else
            _currentChannelOffset = 0;
          RenderTvGuide();
          e.Handled = true;
        }
        else
        {
          _focusedRow++;
          for (int i = 0; i < _grid.Rows[_focusedRow].Colums.Count; ++i)
          {
            Button b = _grid.Rows[_focusedRow].Colums[i];
            GuideTag tag = b.Tag as GuideTag;
            if (tag.Program != null)
            {
              if (_selectedTime >= tag.Program.StartTime && _selectedTime < tag.Program.EndTime)
              {
                Keyboard.Focus(b);
                e.Handled = true;
                return;
              }
            }
          }
        }
      }
      if (e.Key == Key.Up)
      {
        if (_selectedItem.IsUpperEdge)
        {
          if (_currentChannelOffset > 0)
            _currentChannelOffset--;
          else
            _currentChannelOffset = _groupMaps.Count - 1;
          RenderTvGuide();
          e.Handled = true;
        }
        else
        {
          _focusedRow--;
          for (int i = 0; i < _grid.Rows[_focusedRow].Colums.Count; ++i)
          {
            Button b = _grid.Rows[_focusedRow].Colums[i];
            GuideTag tag = b.Tag as GuideTag;
            if (tag.Program != null)
            {
              if (_selectedTime >= tag.Program.StartTime && _selectedTime < tag.Program.EndTime)
              {
                Keyboard.Focus(b);
                e.Handled = true;
                return;
              }
            }
          }
        }
      }
      if (e.Key == Key.Right)
      {
        if (_selectedItem.IsRightEdge)
        {
          _currentTime = _currentTime.AddMinutes(30);
          RenderTvGuide();
          e.Handled = true;
        }
        else
        {
          for (int i = 1; i < _grid.Rows[_focusedRow].Colums.Count + 1; ++i)
          {
            if (_grid.Rows[_focusedRow].Colums[i - 1].IsFocused)
            {
              Button b = _grid.Rows[_focusedRow].Colums[i];
              GuideTag tag = b.Tag as GuideTag;
              if (tag.Program != null)
              {
                _selectedTime = tag.Program.StartTime;
                Keyboard.Focus(b);
                e.Handled = true;
                return;
              }
            }
          }
        }
      }
      if (e.Key == Key.Left)
      {
        if (_selectedItem.IsLeftEdge)
        {
          _currentTime = _currentTime.AddMinutes(-30);
          RenderTvGuide();
          e.Handled = true;
        }
        else
        {
          for (int i = 0; i < _grid.Rows[_focusedRow].Colums.Count - 1; ++i)
          {
            if (_grid.Rows[_focusedRow].Colums[i + 1].IsFocused)
            {
              Button b = _grid.Rows[_focusedRow].Colums[i];
              GuideTag tag = b.Tag as GuideTag;
              if (tag.Program != null)
              {
                _selectedTime = tag.Program.StartTime;
                Keyboard.Focus(b);
                e.Handled = true;
                return;
              }
            }
          }
        }
      }

    }

    /// <summary>
    /// Called when user clicks on a tv channel
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void OnClickChannel(object sender, RoutedEventArgs e)
    {
      if (_singleMode)
      {
        _singleMode = false;
        _selectedItem = null;
        RenderTvGuide();
      }
      else
      {
        if (_selectedItem.IsLeftEdge)
        {
          _singleMode = true;
          _selectedChannel = _selectedItem.Channel;
          _selectedItem = null;
          _singleRowOffset = 0;
          _maxSingleRows = 0;
          RenderTvGuide();
          return;
        }
      }
    }

    /// <summary>
    /// Called when a button receives focus
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.KeyboardFocusChangedEventArgs"/> instance containing the event data.</param>
    void OnButtonGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      Button b = sender as Button;
      if (b == null) return;
      _selectedItem = b.Tag as GuideTag;
      _focusedRow = _selectedItem.RowNr - 1;
      UpdateInfoBox();
    }

    /// <summary>
    /// Called when the mouse enters a button.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
    void OnMouseEnter(object sender, MouseEventArgs e)
    {
      if (!_reactOnMouseEvents)
      {
        _reactOnMouseEvents = true;
        return;
      }
      Button b = sender as Button;
      if (b != null)
      {
        _selectedItem = b.Tag as GuideTag;
        if (_selectedItem.Program != null)
        {
          _selectedTime = _selectedItem.Program.StartTime;
        }
        _focusedRow = _selectedItem.RowNr - 1;
        Keyboard.Focus(b);
      }
    }
    #endregion
  }
}