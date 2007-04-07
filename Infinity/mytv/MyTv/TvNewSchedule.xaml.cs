using System;
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
using Dialogs;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvNewSchedule.xaml
  /// </summary>

  public partial class TvNewSchedule : System.Windows.Controls.Page
  {
    public TvNewSchedule()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      labelHeader.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 40);//new schedule
      buttonQuickRecord.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 41);//Quick Record
      buttonAdvancedRecord.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 42);//Advanced Record
      buttonTvGuide.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 0);//Tv guide
      buttonSearchTitle.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 43);//Search by title
      buttonSearchKeyword.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 44);//Search by keyword
      buttonSearchGenre.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 45);//Search by genre
      // Sets keyboard focus on the first Button in the sample.
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(handleMouse));
      Keyboard.Focus(buttonQuickRecord);
      labelDate.Content = DateTime.Now.ToString("dd-MM HH:mm");
    }
    void handleMouse(object sender, MouseEventArgs e)
    {
      FrameworkElement element = Mouse.DirectlyOver as FrameworkElement;
      while (element.TemplatedParent != null)
      {
        element = (FrameworkElement)element.TemplatedParent;
        if (element as Button != null)
        {
          Keyboard.Focus((Button)element);
          return;
        }
      }
    }

    void OnTvGuide(object sender, RoutedEventArgs e)
    {
      ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvGuide.xaml", UriKind.Relative));
    }
    void OnSearchTitle(object sender, RoutedEventArgs e)
    {
      ServiceScope.Get<INavigationService>().Navigate(new Uri("/MyTv;component/TvSearch.xaml", UriKind.Relative));

    }
    void OnSearchKeyword(object sender, RoutedEventArgs e)
    {
      TvSearch search = new TvSearch(TvSearch.SearchType.Description);
      ServiceScope.Get<INavigationService>().Navigate(search);
    }
    void OnSearchGenre(object sender, RoutedEventArgs e)
    {
      TvSearch search = new TvSearch(TvSearch.SearchType.Genre);
      ServiceScope.Get<INavigationService>().Navigate(search);
    }
    void OnAdvancedRecord(object sender, RoutedEventArgs e)
    {
      MpMenuWithLogo dlgLogoMenu = new MpMenuWithLogo();
      Window w = Window.GetWindow(this);
      dlgLogoMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgLogoMenu.Owner = w;
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
      if (dlgMenu.SelectedIndex < 0) return;

      Schedule rec = new Schedule(selectedChannel.IdChannel, "", Schedule.MinSchedule, Schedule.MinSchedule);
      TvBusinessLayer layer = new TvBusinessLayer();
      rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
      rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
      rec.ScheduleType = (int)dlgMenu.SelectedIndex;


      DateTime dtNow = DateTime.Now;
      dlgMenu = new MpMenu();
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
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
      dlgMenu.Owner = w;
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
      dlgMenu.Owner = w;
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
    void OnQuickRecord(object sender, RoutedEventArgs e)
    {
      MpMenuWithLogo dlgLogoMenu = new MpMenuWithLogo();
      Window w = Window.GetWindow(this);
      dlgLogoMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgLogoMenu.Owner = w;
      dlgLogoMenu.Items.Clear();
      dlgLogoMenu.Header = "Channel";
      dlgLogoMenu.SubTitle = "";

      IList channels = ServiceScope.Get<ITvChannelNavigator>().CurrentGroup.ReferringGroupMap();
      foreach (GroupMap chan in channels)
      {
        string logo = String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.GetLogoFileName(chan.ReferencedChannel().Name));
        if (!System.IO.File.Exists(logo))
          logo = "";
        dlgLogoMenu.Items.Add(new DialogMenuItem(logo, chan.ReferencedChannel().Name,"", ""));
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
      dlgMenu.Owner = w;
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
      dlgMenu.Owner = w;
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
}