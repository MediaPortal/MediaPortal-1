using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using MediaPortal;

namespace MediaPortal
{

    public partial class GUIDateTime : TextBlock
    {
        private DateTime _dateTime;
        private DispatcherTimer _timer;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            UpdateValue();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1000 -
                             DateTime.Now.Millisecond);
            _timer.Tick += new EventHandler(TimerTick);
            _timer.Start();
        }

        public GUIDateTime()
        {
            _dateTime = new DateTime();
            UpdateValue();
        }

        void TimerTick(object sender, EventArgs e)
        {
            UpdateValue();
            _timer.Start();
        }

        public string DateTimeString
        {
            get
            {
                return Text;
            }
         }


        private void UpdateValue()
        {
            
            _dateTime = DateTime.Now;
            string text = _dateTime.DayOfWeek.ToString() + ", " + _dateTime.ToShortTimeString();
            if (Text != text)
                Text = text;
        }

 

      }
}
